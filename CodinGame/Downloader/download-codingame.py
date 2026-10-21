"""Download CodinGame puzzle solutions into Solutions/<Level>/<Title>.<ext>.

The rememberMe cookie is read from the CODINGAME_REMEMBER_ME environment
variable or a local .env file, and otherwise detected from an installed browser.
"""
import argparse
import os
import re
import sys
import threading
import time
from concurrent.futures import ThreadPoolExecutor, as_completed

from requests.exceptions import HTTPError

EXTENSIONS = {
    'bash': 'sh', 'c': 'c', 'c#': 'cs', 'c++': 'cpp', 'clojure': 'clj', 'd': 'd', 'dart': 'dart',
    'f#': 'fs', 'go': 'go', 'groovy': 'groovy', 'haskell': 'hs', 'java': 'java', 'javascript': 'js',
    'kotlin': 'kt', 'lua': 'lua', 'objectivec': 'm', 'ocaml': 'ml', 'pascal': 'pas', 'perl': 'pl',
    'php': 'php', 'python3': 'py', 'ruby': 'rb', 'rust': 'rs', 'scala': 'scala', 'swift': 'swift',
    'typescript': 'ts', 'vb.net': 'vb',
}
SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
DEFAULT_OUT = os.path.abspath(os.path.join(SCRIPT_DIR, "..", "Solutions"))
TITLE_BATCH = 100


def clean_name(name):
    name = re.sub(r'[\\/*?:"<>|]', "", name)
    name = re.sub(r'^[Cc]_', '', name)
    if name and not name[0].isupper():
        name = name[0].upper() + name[1:]
    return name


def load_dotenv(paths):
    """Read KEY=VALUE lines; variables already set in the environment win."""
    for path in paths:
        if not os.path.isfile(path):
            continue
        with open(path, encoding="utf-8") as f:
            for line in f:
                line = line.strip()
                if line and not line.startswith("#") and "=" in line:
                    key, val = line.split("=", 1)
                    key = key.strip()
                    if key.startswith("export "):
                        key = key[len("export "):].strip()
                    os.environ.setdefault(key, val.strip().strip('"').strip("'"))


def get_remember_me_cookie(use_browser=False):
    load_dotenv([os.path.join(os.getcwd(), ".env"), os.path.join(SCRIPT_DIR, ".env")])
    if not use_browser and os.environ.get("CODINGAME_REMEMBER_ME"):
        return os.environ["CODINGAME_REMEMBER_ME"]
    print("Trying to detect CodinGame cookie automatically...")
    print("NOTE: For best results, completely close your browser before running.")
    try:
        import browser_cookie3
    except ImportError:
        browser_cookie3 = None
        print("   - browser_cookie3 is not installed (pip install browser_cookie3)")
    for name in ['Brave', 'Chrome', 'Edge', 'Firefox', 'Safari', 'Opera', 'Vivaldi'] if browser_cookie3 else []:
        try:
            print(f"   - Checking {name}...", end='')
            for cookie in getattr(browser_cookie3, name.lower())(domain_name='codingame.com'):
                if cookie.name == 'rememberMe':
                    print(" found")
                    return cookie.value
            print(" (not found)")
        except Exception:
            print(" (failed to read)")
    print("\nCould not detect your CodinGame login automatically.")
    print("Paste the 'value' of the rememberMe cookie here and press Enter:")
    return input().strip()


class RateLimiter:
    """Enforces a minimum interval between request starts across threads."""

    def __init__(self, interval):
        self.interval, self.lock, self.next_at = interval, threading.Lock(), 0.0

    def wait(self):
        with self.lock:
            now = time.monotonic()
            start = max(now, self.next_at)
            self.next_at = start + self.interval
        if start > now:
            time.sleep(start - now)

    def pause(self, seconds):
        with self.lock:
            self.next_at = max(self.next_at, time.monotonic() + seconds)


class Api:
    def __init__(self, client, interval=0.3, max_retries=5, backoff=2.0, max_backoff=120.0):
        self.client, self.limiter = client, RateLimiter(interval)
        self.max_retries, self.backoff, self.max_backoff = max_retries, backoff, max_backoff
        self.requests_made = 0

    def request(self, service, method, args):
        last_err = None
        for attempt in range(self.max_retries):
            self.limiter.wait()
            self.requests_made += 1
            wait = min(self.backoff * 2 ** attempt, self.max_backoff)
            try:
                return self.client.request(service, method, args)
            except HTTPError as e:
                status = e.response.status_code if e.response is not None else None
                if status == 422:
                    raise  # permanent error for this puzzle
                retry_after = e.response.headers.get("Retry-After") if e.response is not None else None
                if status in (429, 403) and retry_after and str(retry_after).strip().isdigit():
                    wait = float(retry_after)
                last_err = e
            except Exception as e:
                last_err = e
            print(f"   -> Request '{method}' failed ({last_err.__class__.__name__}), retrying in {wait:.0f}s", flush=True)
            self.limiter.pause(wait)
        raise last_err


def fetch_titles(api, user_id, puzzle_ids):
    """Fetch puzzle titles in batches instead of one request per puzzle."""
    titles = {}
    for i in range(0, len(puzzle_ids), TITLE_BATCH):
        chunk = puzzle_ids[i:i + TITLE_BATCH]
        for item in api.request('Puzzle', 'findProgressByIds', [chunk, user_id, 2]) or []:
            if item and item.get("id") is not None:
                titles[item["id"]] = (item.get("title") or "unknown").strip()
    return titles


def latest_by_language(solutions):
    latest = {}
    for s in solutions:
        lang = (s.get('programmingLanguageId') or 'txt').lower()
        if lang not in latest or (s.get('creationTime') or 0) > (latest[lang].get('creationTime') or 0):
            latest[lang] = s
    return latest


def process_puzzle(api, user_id, base, puzzle, title, label):
    """Download missing solutions for one puzzle. Returns (saved, skipped)."""
    folder = os.path.join(base, (puzzle.get("level") or "Unknown").capitalize())
    cleaned = clean_name(title)
    solutions = api.request("Solution", "findMySolutions", [user_id, puzzle["id"], None])
    if not solutions:
        return 0, 0
    saved = skipped = 0
    for lang, sol in latest_by_language(solutions).items():
        path = os.path.join(folder, f"{cleaned}.{EXTENSIONS.get(lang, 'txt')}")
        if os.path.exists(path):
            skipped += 1
            continue
        details = api.request("Solution", "findSolution", [user_id, sol["testSessionQuestionSubmissionId"]])
        os.makedirs(folder, exist_ok=True)
        with open(path, "w", encoding="utf-8") as f:
            f.write((details or {}).get("code", ""))
        mtime = sol.get("creationTime")
        if mtime:
            os.utime(path, (mtime // 1000, mtime // 1000))
        print(f"{label} Saved:   {os.path.relpath(path, base)}")
        saved += 1
    return saved, skipped


def main(argv=None):
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--out", default=DEFAULT_OUT, help="output folder (default: ../Solutions)")
    ap.add_argument("--limit", type=int, help="process at most N puzzles")
    ap.add_argument("--workers", type=int, default=3, help="concurrent puzzles (default: 3)")
    ap.add_argument("--interval", type=float, default=0.3,
                    help="minimum seconds between requests across all workers (default: 0.3)")
    ap.add_argument("--browser", action="store_true", help="ignore CODINGAME_REMEMBER_ME and read browser cookies")
    ap.add_argument("--no-pause", action="store_true", help="do not wait for Enter before exiting")
    a = ap.parse_args(argv)

    import codingame
    base = os.path.abspath(a.out)
    os.makedirs(base, exist_ok=True)
    remember_me = get_remember_me_cookie(a.browser)
    try:
        client = codingame.Client()
        print("Logging in...")
        client.login(remember_me_cookie=remember_me)
        if not client.logged_in or not client.codingamer:
            raise ConnectionRefusedError("Login failed")
        user_id = client.codingamer.id
        print(f"Logged in as: {client.codingamer.pseudo}")
        api = Api(client, interval=a.interval)
        puzzles = api.request("Puzzle", "findAllMinimalProgress", [user_id]) or []
        if a.limit:
            puzzles = puzzles[:a.limit]
        titles = fetch_titles(api, user_id, [p["id"] for p in puzzles])
    except Exception as e:
        print(f"Critical error during setup: {e}")
        return 1
    total = len(puzzles)
    print(f"Processing {total} puzzles.")

    start = time.monotonic()
    saved = skipped = errors = 0
    with ThreadPoolExecutor(max_workers=max(1, a.workers)) as pool:
        futures = {}
        for i, p in enumerate(puzzles):
            label = f"[{i + 1:4d}/{total}]"
            if p["id"] not in titles:
                errors += 1
                print(f"{label} Error: no title for puzzle ID {p['id']}")
                continue
            futures[pool.submit(process_puzzle, api, user_id, base, p, titles[p["id"]], label)] = (label, p)
        for fut in as_completed(futures):
            label, p = futures[fut]
            try:
                s, k = fut.result()
                saved, skipped = saved + s, skipped + k
            except Exception as e:
                errors += 1
                print(f"{label} Error processing '{p.get('prettyId', p.get('id'))}': {e}")

    print(f"\nDone in {time.monotonic() - start:.1f}s ({api.requests_made} requests).")
    print(f"   - Solutions saved:   {saved}")
    print(f"   - Solutions skipped (already existed): {skipped}")
    if errors:
        print(f"   - Puzzles with errors: {errors}")
    if not a.no_pause and sys.stdin.isatty():
        input("\nPress Enter to exit.")
    return 0 if not errors else 2


if __name__ == "__main__":
    sys.exit(main())
