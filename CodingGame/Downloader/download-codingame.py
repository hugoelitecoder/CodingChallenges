import os
import re
import sys
import traceback
import browser_cookie3
import codingame

EXTENSIONS = {
    'bash': 'sh', 'c': 'c', 'c#': 'cs', 'c++': 'cpp', 'clojure': 'clj', 'd': 'd', 'dart': 'dart',
    'f#': 'fs', 'go': 'go', 'groovy': 'groovy', 'haskell': 'hs', 'java': 'java', 'javascript': 'js',
    'kotlin': 'kt', 'lua': 'lua', 'objectivec': 'm', 'ocaml': 'ml', 'pascal': 'pas', 'perl': 'pl',
    'php': 'php', 'python3': 'py', 'ruby': 'rb', 'rust': 'rs', 'scala': 'scala', 'swift': 'swift',
    'typescript': 'ts', 'vb.net': 'vb',
}

# Always use relative to script location
BASE_FOLDER = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "Solutions"))

def clean_name(name):
    # Remove invalid filename characters
    name = re.sub(r'[\\/*?:"<>|]', "", name)
    # Remove C_ or c_ prefix
    name = re.sub(r'^[Cc]_', '', name)
    # Always start with uppercase
    if name and not name[0].isupper():
        name = name[0].upper() + name[1:]
    return name

def get_remember_me_cookie():
    # Try browsers in order: Brave, Chrome, Edge, Firefox
    print("Trying to detect CodinGame cookie automatically...")
    cj = None
    try:
        cj = browser_cookie3.brave()
    except Exception:
        pass
    if cj is None or not list(cj):
        try:
            cj = browser_cookie3.chrome()
        except Exception:
            pass
    if cj is None or not list(cj):
        try:
            cj = browser_cookie3.edge()
        except Exception:
            pass
    if cj is None or not list(cj):
        try:
            cj = browser_cookie3.firefox()
        except Exception:
            pass
    if cj:
        for c in cj:
            if 'codingame' in c.domain and c.name == 'rememberMe':
                print("Successfully found rememberMe cookie in your browser.")
                return c.value

    print("\nCould not detect your CodinGame login automatically.")
    print("Open CodinGame in your browser, login, then:")
    print("  - Open DevTools (F12), go to Application > Cookies > https://www.codingame.com")
    print("  - Copy the value of the 'rememberMe' cookie.")
    return input("Paste your rememberMe cookie here: ").strip()

def main():
    os.makedirs(BASE_FOLDER, exist_ok=True)
    remember_me = get_remember_me_cookie()
    client = codingame.Client()
    print("Logging in...")
    client.login(remember_me_cookie=remember_me)

    if not client.logged_in or not client.codingamer:
        print("Login failed. Check your rememberMe cookie.")
        input("Press Enter to exit.")
        return

    user_id = client.codingamer.id
    print(f"Logged in as: {client.codingamer.pseudo} (ID: {user_id})")
    try:
        levels = client.request("Puzzle", "findAllMinimalProgress", [user_id])
    except Exception as e:
        print("Error fetching puzzles:", e)
        input("Press Enter to exit.")
        return

    print(f"Found {len(levels)} puzzles.")

    for level in levels:
        try:
            level_name = level.get("level", "Unknown")
            # Ensure level folder starts with capital
            folder = os.path.join(BASE_FOLDER, level_name.capitalize())
            os.makedirs(folder, exist_ok=True)
            puzzle_id = level.get("id")
            # Find title
            progress = client.request('Puzzle', 'findProgressByIds', [[puzzle_id], user_id, 2])
            if progress and isinstance(progress, list) and len(progress) > 0:
                title = progress[0].get("title", "unknown").strip()
            else:
                title = level.get("title", "unknown").strip()
            cleaned_title = clean_name(title)
            solutions = client.request("Solution", "findMySolutions", [user_id, puzzle_id, None])
            by_lang = {}
            for s in solutions:
                lang = s.get('programmingLanguageId', 'txt').lower()
                # Only keep the latest per language
                if lang not in by_lang or s['creationTime'] > by_lang[lang]['creationTime']:
                    by_lang[lang] = s

            for lang, sol in by_lang.items():
                extension = EXTENSIONS.get(lang, 'txt')
                # Remove redundant lang from filename 
                filename = f"{cleaned_title}.{extension}"
                filepath = os.path.join(folder, filename)
                solution = client.request("Solution", "findSolution", [user_id, sol["testSessionQuestionSubmissionId"]])
                code = solution.get("code", "")
                with open(filepath, "w", encoding="utf-8") as f:
                    f.write(code)
                # Set file time if possible
                mtime = sol["creationTime"] // 1000 if "creationTime" in sol else None
                if mtime:
                    os.utime(filepath, (mtime, mtime))
                print(f"Saved: {filepath}")
        except Exception as ex:
            print(f"Error processing puzzle: {ex}")
            traceback.print_exc()

    print("\nAll done!")
    input("Press Enter to exit.")

if __name__ == "__main__":
    main()
