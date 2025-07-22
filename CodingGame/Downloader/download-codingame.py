import os
import re
import sys
import time
import browser_cookie3
import codingame
from requests.exceptions import HTTPError

# --- Constants and Configuration ---
EXTENSIONS = {
    'bash': 'sh', 'c': 'c', 'c#': 'cs', 'c++': 'cpp', 'clojure': 'clj', 'd': 'd', 'dart': 'dart',
    'f#': 'fs', 'go': 'go', 'groovy': 'groovy', 'haskell': 'hs', 'java': 'java', 'javascript': 'js',
    'kotlin': 'kt', 'lua': 'lua', 'objectivec': 'm', 'ocaml': 'ml', 'pascal': 'pas', 'perl': 'pl',
    'php': 'php', 'python3': 'py', 'ruby': 'rb', 'rust': 'rs', 'scala': 'scala', 'swift': 'swift',
    'typescript': 'ts', 'vb.net': 'vb',
}
BASE_FOLDER = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "Solutions"))
# Configuration for individual request retries
RETRY_CONFIG = { "max_retries": 3, "backoff_factor": 1.5 }

def clean_name(name):
    name = re.sub(r'[\\/*?:"<>|]', "", name)
    name = re.sub(r'^[Cc]_', '', name)
    if name and not name[0].isupper():
        name = name[0].upper() + name[1:]
    return name

def get_remember_me_cookie():
    print("Trying to detect CodinGame cookie automatically...")
    print("NOTE: For best results, completely close your browser before running.")
    browsers_to_try = [
        ('Brave', browser_cookie3.brave), ('Chrome', browser_cookie3.chrome),
        ('Edge', browser_cookie3.edge), ('Firefox', browser_cookie3.firefox),
        ('Safari', browser_cookie3.safari), ('Opera', browser_cookie3.opera),
        ('Vivaldi', browser_cookie3.vivaldi)
    ]
    for name, browser_func in browsers_to_try:
        try:
            print(f"   - Checking {name}...", end='')
            cj = browser_func(domain_name='codingame.com')
            for cookie in cj:
                if cookie.name == 'rememberMe':
                    print(" ✅ Found!")
                    print(f"Successfully found rememberMe cookie in {name}.")
                    return cookie.value
            print(" (not found)")
        except Exception:
            print(" (failed to read)")
    print("\n❌ Could not detect your CodinGame login automatically.")
    print("Paste the 'value' of the rememberMe cookie here and press Enter:")
    return input().strip()

def resilient_request(client, service, method, args):
    retries=RETRY_CONFIG["max_retries"]
    backoff=RETRY_CONFIG["backoff_factor"]
    last_exception = None
    for attempt in range(retries):
        try:
            # Add a small, consistent delay to be respectful to the API
            time.sleep(0.5)
            return client.request(service, method, args)
        except HTTPError as e:
            # Handle non-retriable errors immediately
            if e.response.status_code == 422: # Unprocessable Entity
                raise e # This is a permanent error for this puzzle, so don't retry
            last_exception = e
        except Exception as e:
            last_exception = e

        if attempt < retries - 1:
            delay = backoff * (2 ** attempt)
            print(f"\n   -> ⚠️ Request for '{method}' failed. Retrying in {delay:.1f}s...", flush=True)
            time.sleep(delay)
            
    raise last_exception

def process_puzzle(puzzle_progress, client, user_id, current_index, total_puzzles):
    """
    Processes a single puzzle sequentially.
    Returns a tuple of (saved_count, skipped_count, error_count) for this puzzle.
    """
    puzzle_id = puzzle_progress.get("id")
    level_name = puzzle_progress.get("level", "Unknown")
    folder_path = os.path.join(BASE_FOLDER, level_name.capitalize())
    progress_str = f"[{current_index + 1:4d}/{total_puzzles}]"

    # Fetch puzzle title
    full_progress = resilient_request(client, 'Puzzle', 'findProgressByIds', [[puzzle_id], user_id, 2])
    if not full_progress or not isinstance(full_progress, list) or not full_progress[0]:
        raise ValueError(f"Could not fetch title for puzzle ID {puzzle_id}.")
    title = full_progress[0].get("title", "unknown").strip()
    cleaned_title = clean_name(title)

    # Fetch solutions
    solutions = resilient_request(client, "Solution", "findMySolutions", [user_id, puzzle_id, None])
    if not solutions:
        print(f"{progress_str} ⏩ Skipped: (No solutions for '{cleaned_title}')")
        return 0, 0, 0

    latest_by_lang = {}
    for s in solutions:
        lang = s.get('programmingLanguageId', 'txt').lower()
        if lang not in latest_by_lang or s['creationTime'] > latest_by_lang[lang]['creationTime']:
            latest_by_lang[lang] = s
    
    saved_in_puzzle, skipped_in_puzzle = 0, 0
    for lang, sol in latest_by_lang.items():
        ext = EXTENSIONS.get(lang, 'txt')
        filepath = os.path.join(folder_path, f"{cleaned_title}.{ext}")
        
        if os.path.exists(filepath):
            skipped_in_puzzle += 1
            continue

        os.makedirs(folder_path, exist_ok=True)
        solution_details = resilient_request(client, "Solution", "findSolution", [user_id, sol["testSessionQuestionSubmissionId"]])
        code = solution_details.get("code", "")
        with open(filepath, "w", encoding="utf-8") as f: f.write(code)
        
        mtime = sol.get("creationTime")
        if mtime: os.utime(filepath, (mtime // 1000, mtime // 1000))
        
        print(f"{progress_str} ✅ Saved:   {os.path.relpath(filepath, BASE_FOLDER)}")
        saved_in_puzzle += 1
    
    if skipped_in_puzzle > 0:
         print(f"{progress_str} ⏩ Skipped: {skipped_in_puzzle} file(s) for '{cleaned_title}' (already exist)")

    return saved_in_puzzle, skipped_in_puzzle, 0

def main():
    os.makedirs(BASE_FOLDER, exist_ok=True)
    remember_me = get_remember_me_cookie()
    try:
        client = codingame.Client()
        print("Logging in...")
        client.login(remember_me_cookie=remember_me)
        if not client.logged_in or not client.codingamer: raise ConnectionRefusedError("Login failed")
        user_id = client.codingamer.id
        print(f"✅ Successfully logged in as: {client.codingamer.pseudo} (ID: {user_id})")
        
        print("Fetching all puzzle progress...")
        all_puzzles = resilient_request(client, "Puzzle", "findAllMinimalProgress", [user_id])
        total_puzzles = len(all_puzzles)
        print(f"Found {total_puzzles} puzzles in your history.")
    except Exception as e:
        print(f"❌ Critical error during setup: {e}"); return

    saved_count, skipped_count, error_count = 0, 0, 0

    print(f"\n--- Starting Sequential Processing ---")
    for i, puzzle_progress in enumerate(all_puzzles):
        try:
            s, k, e = process_puzzle(puzzle_progress, client, user_id, i, total_puzzles)
            saved_count += s
            skipped_count += k
            error_count += e
        except Exception as e:
            error_count += 1
            progress_str = f"[{i + 1:4d}/{total_puzzles}]"
            title_for_error = puzzle_progress.get('prettyId', f"ID {puzzle_progress.get('id', 'Unknown')}")
            print(f"{progress_str} ❌ Error processing '{title_for_error}': {e}")
            
    print("\n🎉 All done!")
    print(f"--- Summary ---")
    print(f"   - Solutions Saved:   {saved_count}")
    print(f"   - Solutions Skipped (already existed): {skipped_count}")
    if error_count > 0:
        print(f"   - Puzzles with Errors: {error_count}")
        
    input("\nPress Enter to exit.")

if __name__ == "__main__":
    main()