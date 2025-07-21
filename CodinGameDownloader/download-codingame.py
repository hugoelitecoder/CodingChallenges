import os
import re
import sys
import traceback

try:
    import browser_cookie3
except ImportError:
    print("browser_cookie3 is not installed. Run: pip install browser_cookie3")
    sys.exit(1)
try:
    import codingame
except ImportError:
    print("codingame is not installed. Run: pip install codingame")
    sys.exit(1)

def clean_filename(name):
    # Remove or replace chars that are invalid in filenames
    s = re.sub(r'[\\/*?:"<>|]', "_", name)
    s = s.replace("C_", "")
    s = s.replace("csharp", "").replace("CSharp", "")
    return s.strip()

def get_remember_me_cookie():
    browsers = [
        ("Brave", getattr(browser_cookie3, "brave", None)),
        ("Chrome", getattr(browser_cookie3, "chrome", None)),
        ("Edge", getattr(browser_cookie3, "edge", None)),
        ("Firefox", getattr(browser_cookie3, "firefox", None)),
    ]
    for browser_name, browser_func in browsers:
        if browser_func is None:
            continue
        try:
            cj = browser_func()
            for c in cj:
                if "codingame" in c.domain and c.name == "rememberMe":
                    print(f"Found rememberMe in {browser_name}.")
                    return c.value
        except Exception:
            continue

    print("\nCould not auto-detect your CodinGame rememberMe cookie!\n")
    print("** To get your rememberMe cookie manually, do this:**")
    print("  1. Open https://www.codingame.com and log in.")
    print("  2. Open browser DevTools (F12).")
    print("  3. Go to Application/Storage/Privacy tab.")
    print("  4. Click 'Cookies', then 'https://www.codingame.com'.")
    print("  5. Look for the 'rememberMe' row. Double-click its value and copy it.\n")
    val = input("Paste your rememberMe cookie value here: ").strip()
    if not val:
        print("No cookie value entered. Exiting.")
        sys.exit(1)
    return val

EXTENSIONS = {
    'bash': 'sh', 'c': 'c', 'c#': 'cs', 'c++': 'cpp', 'clojure': 'clj', 'd': 'd', 'dart': 'dart', 'f#': 'fs',
    'go': 'go', 'groovy': 'groovy', 'haskell': 'hs', 'java': 'java', 'javascript': 'js', 'kotlin': 'kt',
    'lua': 'lua', 'objectivec': 'm', 'ocaml': 'ml', 'pascal': 'pas', 'perl': 'pl', 'php': 'php',
    'python3': 'py', 'ruby': 'rb', 'rust': 'rs', 'scala': 'scala', 'swift': 'swift', 'typescript': 'ts',
    'vb.net': 'vb'
}

def main():
    print("Trying to fetch cookie from open browsers [www.codingame.com]...")
    remember_me = get_remember_me_cookie()
    print("Logging in to CodinGame...")

    try:
        client = codingame.Client()
        client.login(remember_me_cookie=remember_me)
    except Exception as ex:
        print("Login failed! Exception:", ex)
        input("Press Enter to exit...")
        sys.exit(1)

    if not client.logged_in or not client.codingamer:
        print("Login failed. Check your rememberMe cookie!")
        input("Press Enter to exit...")
        sys.exit(1)

    print(f"Logged in as: {client.codingamer.pseudo} (ID: {client.codingamer.id})")

    user_id = client.codingamer.id
    try:
        puzzles = client.request('Puzzle', 'findAllMinimalProgress', [user_id])
    except Exception as ex:
        print("Could not fetch puzzle list!", ex)
        input("Press Enter to exit...")
        sys.exit(1)

    print(f"Found {len(puzzles)} puzzles.")

    # Bulk fetch details for all puzzles for titles etc
    all_ids = [p['id'] for p in puzzles]
    progress_details = {}
    try:
        details_list = client.request('Puzzle', 'findProgressByIds', [all_ids, user_id, 2])
        for d in details_list:
            progress_details[d['id']] = d
    except Exception as ex:
        print("Bulk details fetch failed:", ex)

    # Level to folder mapping, capitalized
    level_map = {
        "easy": "Easy",
        "medium": "Medium",
        "hard": "Hard",
        "very_hard": "Very Hard"
    }

    out_base = "CodingameSolutions"
    os.makedirs(out_base, exist_ok=True)

    for puzzle in puzzles:
        try:
            puzzle_id = puzzle["id"]
            level = puzzle.get("level", "unknown").lower()
            folder = os.path.join(out_base, level_map.get(level, level.title()))
            os.makedirs(folder, exist_ok=True)

            progress = progress_details.get(puzzle_id)
            title = "unknown"
            if progress is not None:
                title = progress.get("title", "unknown")
            else:
                # Fallback: try to fetch details by prettyId
                pretty_id = puzzle.get("prettyId", "")
                if pretty_id:
                    try:
                        single = client.request("Puzzle", "findProgressByPrettyId", [pretty_id, user_id])
                        if single and single.get("title"):
                            title = single["title"]
                    except Exception as e:
                        print(f"  [WARN] Couldn't fetch title for puzzle id={puzzle_id}: {e}")
            if not title or title.strip() == "":
                print(f"  Skipping puzzle with id={puzzle_id} (no title returned)")
                continue
            title = title.strip()
            print(f"\nPuzzle: {title} | Level: {folder}")

            try:
                solutions = client.request("Solution", "findMySolutions", [user_id, puzzle_id, None])
            except Exception as ex:
                print(f"  Skipping {title} (could not fetch solutions): {ex}")
                continue

            latest_per_lang = {}
            for s in solutions:
                lang = s["programmingLanguageId"].lower()
                if lang not in latest_per_lang or s["creationTime"] > latest_per_lang[lang]["creationTime"]:
                    latest_per_lang[lang] = s

            for lang, s in latest_per_lang.items():
                clean_title = clean_filename(title)
                ext = EXTENSIONS.get(lang, lang)
                # Only use clean title for filename, drop lang in name (extension is enough)
                filename = f"{clean_title}.{ext}"
                filepath = os.path.join(folder, filename)
                try:
                    solution = client.request("Solution", "findSolution", [user_id, s["testSessionQuestionSubmissionId"]])
                    with open(filepath, "w", encoding="utf-8") as f:
                        f.write(solution["code"])
                    print(f"  Saved: {filepath}")
                except Exception as ex:
                    print(f"  Failed to save {filename}: {ex}")
        except Exception as ex:
            print(f"Error processing puzzle: {ex}")
            traceback.print_exc()

    print("\nAll done. Press Enter to exit.")
    input()

if __name__ == '__main__':
    main()
