"""Download accepted LeetCode submissions into Solutions/<Difficulty>/<id>. <Title>/.

Each problem folder contains README.md (the problem statement) and one code
file per language. Credentials are read from the LEETCODE_SESSION (and
optional LEETCODE_CSRFTOKEN) environment variables or a local .env file, and
otherwise detected from an installed browser.
"""
import argparse
import glob
import html
import json
import os
import re
import sys
import threading
import time
from concurrent.futures import ThreadPoolExecutor, as_completed
from html.parser import HTMLParser

import requests

EXTENSIONS = {
    'bash': 'sh', 'c': 'c', 'csharp': 'cs', 'cpp': 'cpp', 'java': 'java',
    'javascript': 'js', 'kotlin': 'kt', 'mysql': 'sql', 'python': 'py',
    'python3': 'py', 'ruby': 'rb', 'swift': 'swift', 'go': 'go', 'golang': 'go',
    'rust': 'rs', 'typescript': 'ts', 'php': 'php', 'scala': 'scala',
    'mssql': 'sql', 'oraclesql': 'sql', 'postgresql': 'sql', 'pythondata': 'py',
    'dart': 'dart', 'elixir': 'ex', 'erlang': 'erl', 'racket': 'rkt',
}
SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
DEFAULT_OUT = os.path.abspath(os.path.join(SCRIPT_DIR, "..", "Solutions"))
GRAPHQL_URL = "https://leetcode.com/graphql"
PAGE_SIZE = 20
STATE_FILE = ".download-state.json"
CACHE_DIR = ".cache"

Q_USER = "query globalData { userStatus { username isSignedIn } }"
Q_SUBMISSION_LIST = """
query submissionList($offset: Int!, $limit: Int!, $lastKey: String, $questionSlug: String!) {
  submissionList(offset: $offset, limit: $limit, lastKey: $lastKey, questionSlug: $questionSlug) {
    lastKey hasNext
    submissions { id title titleSlug statusDisplay lang timestamp runtime }
  }
}"""
Q_SUBMISSION_DETAILS = """
query submissionDetails($submissionId: Int!) {
  submissionDetails(submissionId: $submissionId) {
    code timestamp runtimeDisplay runtimePercentile memoryDisplay memoryPercentile
    question { questionId title titleSlug content difficulty }
  }
}"""
Q_QUESTION = """
query questionData($titleSlug: String!) {
  question(titleSlug: $titleSlug) { questionId questionFrontendId title titleSlug content difficulty }
}"""


# --------------------------------------------------------------- credentials

def load_dotenv(paths):
    """Read KEY=VALUE lines; variables already set in the environment win."""
    for path in paths:
        if not os.path.isfile(path):
            continue
        with open(path, encoding="utf-8") as f:
            for line in f:
                line = line.strip()
                if not line or line.startswith("#") or "=" not in line:
                    continue
                key, val = line.split("=", 1)
                key = key.strip()
                if key.startswith("export "):
                    key = key[len("export "):].strip()
                os.environ.setdefault(key, val.strip().strip('"').strip("'"))


def cookies_from_browser():
    print("Trying to detect LEETCODE_SESSION cookie from your browsers...")
    print("NOTE: For best results, completely close your browser before running.")
    try:
        import browser_cookie3
    except ImportError:
        browser_cookie3 = None
        print("   - browser_cookie3 is not installed (pip install browser_cookie3)")
    names = ['Brave', 'Chrome', 'Edge', 'Firefox', 'Safari', 'Opera', 'Vivaldi']
    for name in names if browser_cookie3 else []:
        try:
            print(f"   - Checking {name}...", end='')
            jar = getattr(browser_cookie3, name.lower())(domain_name='leetcode.com')
            found = {c.name: c.value for c in jar if c.name in ('LEETCODE_SESSION', 'csrftoken')}
            if found.get('LEETCODE_SESSION'):
                print(" found")
                return found['LEETCODE_SESSION'], found.get('csrftoken')
            print(" (not found)")
        except Exception:
            print(" (failed to read)")
    print("\nCould not detect your LeetCode login automatically.")
    print("Paste the 'value' of the LEETCODE_SESSION cookie here and press Enter:")
    return input().strip(), None


def get_credentials(use_browser=False):
    load_dotenv([os.path.join(os.getcwd(), ".env"), os.path.join(SCRIPT_DIR, ".env")])
    session = None if use_browser else os.environ.get("LEETCODE_SESSION")
    csrf = os.environ.get("LEETCODE_CSRFTOKEN") or os.environ.get("CSRFTOKEN")
    if session:
        return session, csrf
    session, browser_csrf = cookies_from_browser()
    return session, browser_csrf or csrf


# --------------------------------------------------------------- HTTP client

class AuthError(RuntimeError):
    pass


class RateLimiter:
    """Enforces a minimum interval between request starts across threads."""

    def __init__(self, interval, clock=time.monotonic, sleep=time.sleep):
        self.interval, self.clock, self.sleep = interval, clock, sleep
        self.lock = threading.Lock()
        self.next_at = 0.0

    def wait(self):
        with self.lock:
            now = self.clock()
            start = max(now, self.next_at)
            self.next_at = start + self.interval
        if start > now:
            self.sleep(start - now)

    def pause(self, seconds):
        """Delay every thread, used when the server asks to back off."""
        with self.lock:
            self.next_at = max(self.next_at, self.clock() + seconds)


class LeetCodeClient:
    def __init__(self, session_cookie, csrf=None, http=None, interval=0.4, max_retries=6,
                 backoff=2.0, max_backoff=120.0, sleep=time.sleep):
        self.http = http or requests.Session()
        self.max_retries, self.backoff, self.max_backoff = max_retries, backoff, max_backoff
        self.sleep = sleep
        self.limiter = RateLimiter(interval, sleep=sleep)
        cookie = f"LEETCODE_SESSION={session_cookie}"
        self.headers = {"Content-Type": "application/json", "Referer": "https://leetcode.com/",
                        "Origin": "https://leetcode.com", "User-Agent": "Mozilla/5.0"}
        if csrf:
            cookie += f"; csrftoken={csrf}"
            self.headers["x-csrftoken"] = csrf
        self.headers["Cookie"] = cookie
        self.requests_made = 0

    def graphql(self, query, variables):
        payload = {"query": query, "variables": variables}
        last_err = None
        for attempt in range(self.max_retries):
            self.limiter.wait()
            self.requests_made += 1
            wait = min(self.backoff * 2 ** attempt, self.max_backoff)
            try:
                r = self.http.post(GRAPHQL_URL, json=payload, headers=self.headers, timeout=30)
            except requests.RequestException as e:
                last_err = e
                self.sleep(wait)
                continue
            if r.status_code in (429, 403) or r.status_code >= 500:
                retry_after = (r.headers or {}).get("Retry-After")
                if retry_after and str(retry_after).strip().isdigit():
                    wait = float(retry_after)
                last_err = RuntimeError(f"HTTP {r.status_code}")
                print(f"   -> HTTP {r.status_code}, backing off {wait:.0f}s", flush=True)
                self.limiter.pause(wait)
                continue
            if r.status_code == 401:
                raise AuthError("HTTP 401: cookie rejected")
            if r.status_code != 200:
                raise RuntimeError(f"HTTP {r.status_code}")
            data = r.json()
            errors = data.get("errors")
            if errors:
                msg = "; ".join(str(e.get("message", e)) for e in errors)
                if "unauthorized" in msg.lower() or "login" in msg.lower():
                    raise AuthError(msg)
                if not data.get("data") or all(v is None for v in data["data"].values()):
                    raise RuntimeError(f"GraphQL error: {msg}")
            return data.get("data") or {}
        raise RuntimeError(f"giving up after {self.max_retries} attempts: {last_err}")

    def username(self):
        status = self.graphql(Q_USER, {}).get("userStatus") or {}
        return status.get("username") if status.get("isSignedIn", True) else None

    def submission_pages(self):
        offset, last_key = 0, None
        while True:
            res = self.graphql(Q_SUBMISSION_LIST, {"offset": offset, "limit": PAGE_SIZE,
                                                   "lastKey": last_key, "questionSlug": ""})
            page = res.get("submissionList") or {}
            subs = page.get("submissions") or []
            yield subs
            offset += len(subs)
            if not subs or not page.get("hasNext"):
                return
            last_key = page.get("lastKey")

    def submission_details(self, sub_id):
        return self.graphql(Q_SUBMISSION_DETAILS, {"submissionId": int(sub_id)}).get("submissionDetails")

    def question(self, slug):
        return self.graphql(Q_QUESTION, {"titleSlug": slug}).get("question")


# --------------------------------------------------------------- HTML to Markdown

class _Markdown(HTMLParser):
    BLOCK = {"p", "div", "ul", "ol", "pre", "h1", "h2", "h3", "h4", "table", "tr", "blockquote"}

    def __init__(self):
        super().__init__(convert_charrefs=True)
        self.out, self.pre, self.lists = [], 0, []

    def _at_bullet(self):
        return bool(self.out) and re.search(r"\n *(-|\d+\.) $", self.out[-1]) is not None

    def handle_starttag(self, tag, attrs):
        if tag in self.BLOCK:
            # List items are often wrapped as <li><p>text</p></li>; keep the
            # text on the bullet line instead of emitting an empty bullet.
            if not (tag in ("p", "div") and self.lists and self._at_bullet()):
                self.out.append("\n\n")
        if tag == "pre":
            self.pre += 1
            self.out.append("```\n")
        elif tag in ("ul", "ol"):
            self.lists.append([tag, 0])
        elif tag == "li":
            indent = "  " * max(len(self.lists) - 1, 0)
            if self.lists and self.lists[-1][0] == "ol":
                self.lists[-1][1] += 1
                self.out.append(f"\n{indent}{self.lists[-1][1]}. ")
            else:
                self.out.append(f"\n{indent}- ")
        elif tag == "br":
            self.out.append("\n")
        elif tag == "code" and not self.pre:
            self.out.append("`")
        elif tag in ("strong", "b") and not self.pre:
            self.out.append("**")
        elif tag in ("em", "i") and not self.pre:
            self.out.append("*")
        elif tag == "sup":
            self.out.append("^")
        elif tag == "img":
            a = dict(attrs)
            self.out.append(f"![{a.get('alt') or 'image'}]({a.get('src') or ''})")
        elif tag in ("td", "th"):
            self.out.append(" | ")

    def handle_endtag(self, tag):
        if tag == "pre":
            self.pre -= 1
            while self.out and not self.out[-1].strip() and self.out[-1] != "```\n":
                self.out.pop()
            if self.out:
                self.out[-1] = self.out[-1].rstrip()
            self.out.append("\n```")
        elif tag in ("ul", "ol") and self.lists:
            self.lists.pop()
        elif tag == "code" and not self.pre:
            self.out.append("`")
        elif tag in ("strong", "b", "em", "i") and not self.pre:
            mark = "**" if tag in ("strong", "b") else "*"
            trail = ""
            if self.out and self.out[-1] != self.out[-1].rstrip(" "):
                self.out[-1] = self.out[-1].rstrip(" ")
                trail = " "
            self.out.append(mark + trail)
        if tag in self.BLOCK and not (tag in ("p", "div") and self.lists):
            self.out.append("\n\n")

    def handle_data(self, data):
        if self.pre:
            if self.out and self.out[-1] == "```\n":
                data = data.lstrip("\n")
            self.out.append(data)
        else:
            self.out.append(re.sub(r"\s+", " ", data))


def html_to_markdown(content):
    if not content:
        return ""
    parser = _Markdown()
    parser.feed(content)
    text = html.unescape("".join(parser.out)).replace("\xa0", " ")
    text = text.replace("**``**", "").replace("````", "")
    text = "\n".join(line.rstrip() for line in text.splitlines())
    text = re.sub(r"\n (?=\S)", "\n", text)
    text = re.sub(r"\n{3,}", "\n\n", text)
    text = re.sub(r"```\n\s*\n", "```\n", text)
    return text.strip()


def parse_runtime_ms(display):
    m = re.search(r"\d+(?:\.\d+)?", str(display or ""))
    return float(m.group()) if m else None


def round_percentile(value):
    try:
        return round(float(value), 2) if value is not None else None
    except (TypeError, ValueError):
        return None


# --------------------------------------------------------------- output

def clean_name_for_path(name):
    name = re.sub(r'[\\/*?:"<>|]', "", name)
    return re.sub(r'\s+', " ", name).strip()


def code_file_name(title, lang):
    return f"{clean_name_for_path(title).replace(' ', '-')}.{EXTENSIONS.get(lang, 'txt')}"


def problem_folder(base, question):
    q_id = str(question['questionId']).zfill(4)
    return os.path.join(base, question['difficulty'].capitalize(),
                        f"{q_id}. {clean_name_for_path(question['title'])}")


def index_existing(base):
    """Map code file name -> path for every solution already on disk."""
    index = {}
    for path in glob.glob(os.path.join(glob.escape(base), "*", "*", "*")):
        name = os.path.basename(path)
        if name != "README.md" and os.path.isfile(path):
            index[name] = path
    return index


def write_text(path, text):
    tmp = path + ".tmp"
    with open(tmp, "w", encoding="utf-8", newline="") as f:
        f.write(text)
    os.replace(tmp, path)


class Downloader:
    def __init__(self, client, base, markdown=False, overwrite_faster=False, workers=3):
        self.client, self.base = client, base
        self.markdown, self.overwrite_faster, self.workers = markdown, overwrite_faster, workers
        self.cache_dir = os.path.join(SCRIPT_DIR, CACHE_DIR, "questions")
        self.state_path = os.path.join(base, STATE_FILE)
        self.state = self._load_state()
        self.existing = index_existing(base)
        self._questions, self._qlock = {}, threading.Lock()
        self._slug_locks = {}

    def _load_state(self):
        try:
            with open(self.state_path, encoding="utf-8") as f:
                state = json.load(f)
        except (OSError, ValueError):
            state = {}
        state.setdefault("saved", {})
        return state

    def save_state(self):
        os.makedirs(self.base, exist_ok=True)
        write_text(self.state_path, json.dumps(self.state, indent=1, sort_keys=True))

    def question(self, slug, seed=None):
        """Return question metadata from memory, the disk cache, seed data or the API."""
        with self._qlock:
            if slug in self._questions:
                return self._questions[slug]
            lock = self._slug_locks.setdefault(slug, threading.Lock())
        with lock:
            if slug in self._questions:
                return self._questions[slug]
            path = os.path.join(self.cache_dir, f"{slug}.json")
            q = None
            if os.path.isfile(path):
                with open(path, encoding="utf-8") as f:
                    q = json.load(f)
            elif seed and seed.get("questionId") and seed.get("difficulty") and seed.get("title"):
                q = seed
            else:
                q = self.client.question(slug)
                if not q:
                    raise RuntimeError(f"no question data for {slug}")
            if not os.path.isfile(path):
                os.makedirs(self.cache_dir, exist_ok=True)
                write_text(path, json.dumps(q, ensure_ascii=False))
            self._questions[slug] = q
            return q

    def plan(self, sub, seen):
        """Return True if the listed accepted submission should be downloaded."""
        key = f"{sub['titleSlug']}|{sub['lang']}"
        if key in seen:
            return False
        seen.add(key)
        if code_file_name(sub['title'], sub['lang']) not in self.existing:
            return True
        if not self.overwrite_faster:
            return False
        saved = self.state["saved"].get(key) or {}
        new_rt, old_rt = parse_runtime_ms(sub.get("runtime")), saved.get("runtime_ms")
        return new_rt is not None and old_rt is not None and new_rt < old_rt and \
            int(sub["id"]) != saved.get("id")

    def fetch_and_save(self, sub):
        det = self.client.submission_details(sub["id"])
        if not det or det.get("code") is None:
            raise RuntimeError("empty submission details")
        q = self.question(sub["titleSlug"], det.get("question"))
        folder = problem_folder(self.base, q)
        os.makedirs(folder, exist_ok=True)
        readme = os.path.join(folder, "README.md")
        if not os.path.exists(readme):
            body = html_to_markdown(q.get("content")) if self.markdown else (q.get("content") or "")
            q_id = str(q['questionId']).zfill(4)
            write_text(readme, f"# {q_id}. {q['title']}\n\n**Difficulty:** {q['difficulty']}\n\n---\n\n{body}\n")
        name = code_file_name(q['title'], sub['lang'])
        path = os.path.join(folder, name)
        replaced = os.path.exists(path)
        write_text(path, det["code"])
        return q, path, replaced, {
            "id": int(sub["id"]),
            "timestamp": int(det.get("timestamp") or sub.get("timestamp") or 0) or None,
            "runtime_ms": parse_runtime_ms(det.get("runtimeDisplay") or sub.get("runtime")),
            "runtime_percentile": round_percentile(det.get("runtimePercentile")),
            "memory": det.get("memoryDisplay"),
            "memory_percentile": round_percentile(det.get("memoryPercentile")),
        }

    def list_pending(self, limit=None, incremental=False):
        pending, seen = [], set()
        last_seen = int(self.state.get("newest_id") or 0)
        newest, scanned = last_seen, 0
        for page in self.client.submission_pages():
            reached_known = False
            for sub in page:
                scanned += 1
                sid = int(sub["id"])
                newest = max(newest, sid)
                if incremental and last_seen and sid <= last_seen:
                    reached_known = True
                    break
                if sub.get("statusDisplay") == "Accepted" and self.plan(sub, seen):
                    pending.append(sub)
                    if limit and len(pending) >= limit:
                        break
            if reached_known:
                print(f"Reached previously downloaded submissions after {scanned} entries.")
                break
            if limit and len(pending) >= limit:
                print(f"Limit of {limit} reached after scanning {scanned} entries.")
                break
        return pending, newest, scanned, reached_known or not limit

    def run_pass(self, items, workers, stats):
        failed = []
        with ThreadPoolExecutor(max_workers=max(1, workers)) as pool:
            futures = {pool.submit(self.fetch_and_save, sub): sub for sub in items}
            for fut in as_completed(futures):
                sub = futures[fut]
                try:
                    q, path, replaced, info = fut.result()
                except AuthError:
                    raise
                except Exception as e:
                    failed.append(sub)
                    print(f"   -> Error for '{sub['title']}' [{sub['lang']}]: {e}")
                    continue
                key = f"{sub['titleSlug']}|{sub['lang']}"
                self.state["saved"][key] = info
                self.existing[os.path.basename(path)] = path
                stats["replaced" if replaced else "saved"] += 1
                label = "Replaced (faster)" if replaced else "Saved"
                print(f"   -> {label}: ({q['difficulty']}) [{sub['lang']}] - {q['title']}")
        return failed

    def run(self, limit=None, incremental=False, retry_interval=2.0):
        stats = {"saved": 0, "replaced": 0}
        pending, newest, scanned, complete = self.list_pending(limit, incremental)
        print(f"Scanned {scanned} submissions, {len(pending)} to download.")
        first_failed = self.run_pass(pending, self.workers, stats)
        final_failed = []
        if first_failed:
            print(f"\nRetrying {len(first_failed)} failed submissions sequentially...")
            old = self.client.limiter.interval
            self.client.limiter.interval = max(old, retry_interval)
            try:
                final_failed = self.run_pass(first_failed, 1, stats)
            finally:
                self.client.limiter.interval = old
        # Only advance the incremental marker when nothing newer was left behind.
        if complete and not final_failed:
            self.state["newest_id"] = newest
        self.save_state()
        stats.update(scanned=scanned, first_failed=len(first_failed), failed=len(final_failed))
        return stats


def main(argv=None):
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--out", default=DEFAULT_OUT, help="output folder (default: ../Solutions)")
    ap.add_argument("--limit", type=int, help="download at most N submissions and stop listing early")
    ap.add_argument("--incremental", action="store_true",
                    help="stop paging at the newest submission seen by a previous run")
    ap.add_argument("--overwrite-faster", action="store_true",
                    help="replace a saved solution when a newer accepted one has a lower runtime")
    ap.add_argument("--markdown", action="store_true", help="convert the problem statement to Markdown")
    ap.add_argument("--workers", type=int, default=3, help="concurrent detail requests (default: 3)")
    ap.add_argument("--interval", type=float, default=0.4,
                    help="minimum seconds between requests across all workers (default: 0.4)")
    ap.add_argument("--browser", action="store_true", help="ignore LEETCODE_SESSION and read browser cookies")
    ap.add_argument("--no-pause", action="store_true", help="do not wait for Enter before exiting")
    a = ap.parse_args(argv)

    base = os.path.abspath(a.out)
    print(f"Solutions will be saved to: {base}")
    session, csrf = get_credentials(a.browser)
    if not session:
        return 1
    client = LeetCodeClient(session, csrf, interval=a.interval)
    try:
        username = client.username()
    except Exception as e:
        print(f"Could not verify session cookie: {e}")
        return 1
    if not username:
        print("Could not verify session cookie. Please check your cookie and try again.")
        return 1
    print(f"Authenticated as '{username}'.")

    os.makedirs(base, exist_ok=True)
    downloader = Downloader(client, base, markdown=a.markdown,
                            overwrite_faster=a.overwrite_faster, workers=a.workers)
    start = time.monotonic()
    try:
        stats = downloader.run(limit=a.limit, incremental=a.incremental)
    except AuthError as e:
        downloader.save_state()
        print(f"Session rejected: {e}. Update the cookie and run again.")
        return 1
    except KeyboardInterrupt:
        downloader.save_state()
        print("Interrupted.")
        return 130

    print(f"\nDone in {time.monotonic() - start:.1f}s ({client.requests_made} requests).")
    print(f"   - Solutions saved:    {stats['saved']}")
    print(f"   - Solutions replaced: {stats['replaced']}")
    if stats["first_failed"]:
        print(f"   - Failed on first pass:   {stats['first_failed']}")
        print(f"   - Recovered on retry:     {stats['first_failed'] - stats['failed']}")
        print(f"   - Unrecoverable failures: {stats['failed']}")
    if not a.no_pause and sys.stdin.isatty():
        input("\nPress Enter to exit.")
    return 0 if stats["failed"] == 0 else 2


if __name__ == "__main__":
    sys.exit(main())
