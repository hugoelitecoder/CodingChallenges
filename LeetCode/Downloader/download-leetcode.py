import os
import re
import sys
import time
import json
import traceback
import browser_cookie3
import requests
from concurrent.futures import ThreadPoolExecutor, as_completed

EXTENSIONS = {
    'bash': 'sh', 'c': 'c', 'csharp': 'cs', 'cpp': 'cpp', 'java': 'java',
    'javascript': 'js', 'kotlin': 'kt', 'mysql': 'sql', 'python': 'py',
    'python3': 'py', 'ruby': 'rb', 'swift': 'swift', 'go': 'go',
    'rust': 'rs', 'typescript': 'ts', 'php': 'php', 'scala': 'scala',
    'mssql': 'sql', 'oraclesql': 'sql', 'pythondata': 'py',
}

BASE_FOLDER = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "Solutions"))

# Configuration for the retry mechanism
RETRY_CONFIG = {
    "max_retries": 3,       # Number of times to retry a failed request
    "backoff_factor": 1.0,  # Base delay in seconds, doubles each retry
}

def clean_name_for_path(name):
    name = re.sub(r'[\\/*?:"<>|]', "", name)
    name = re.sub(r'\s+', " ", name).strip()
    return name

def get_leetcode_session_cookie():
    print("Trying to detect LEETCODE_SESSION cookie from your browsers...")
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
            cj = browser_func(domain_name='leetcode.com')
            for cookie in cj:
                if cookie.name == 'LEETCODE_SESSION':
                    print(" ✅ Found!")
                    print(f"Successfully found LEETCODE_SESSION cookie in {name}.")
                    return cookie.value
            print(" (not found)")
        except Exception:
            print(" (failed to read)")
            continue

    print("\n❌ Could not detect your LeetCode login automatically.")
    print("To get the cookie manually:")
    print("  1. Open LeetCode in your browser and log in.")
    print("  2. Open the Developer Tools (usually F12 or Ctrl+Shift+I).")
    print("  3. Go to the 'Application' (or 'Storage') tab.")
    print("  4. Find 'Cookies' -> 'https://leetcode.com'.")
    print("  5. Find the cookie named 'LEETCODE_SESSION' and copy its 'Value'.")
    print("Paste the cookie value here and press Enter:")
    return input().strip()

def graphql_request(query, variables, session_cookie, retries=RETRY_CONFIG["max_retries"], backoff=RETRY_CONFIG["backoff_factor"]):
    url = "https://leetcode.com/graphql"
    headers = {
        'Cookie': f'LEETCODE_SESSION={session_cookie}', 'Content-Type': 'application/json',
        'Referer': 'https://leetcode.com', 'User-Agent': 'Mozilla/5.0'
    }
    payload = {"query": query, "variables": variables}
    last_exception = None

    for attempt in range(retries):
        try:
            response = requests.post(url, headers=headers, json=payload, timeout=20)
            response.raise_for_status()
            data = response.json()
            # Check for GraphQL-level errors, which are not connection errors
            if "errors" in data:
                if any("unauthorized" in e.get("message", "").lower() for e in data["errors"]):
                     print("\n❌ Authentication Error: Your LEETCODE_SESSION cookie is likely invalid or expired.")
                     return "AUTH_ERROR" # No point in retrying auth errors
                # For other GraphQL errors, we might still retry
                raise ValueError(f"GraphQL query returned errors: {data['errors']}")
            return data # Success
        except (requests.exceptions.RequestException, ValueError, json.JSONDecodeError) as e:
            last_exception = e
            delay = backoff * (2 ** attempt)
            print(f"\n   -> ⚠️ Request failed ({e.__class__.__name__}). Retrying in {delay:.1f}s... (Attempt {attempt + 1}/{retries})", flush=True)
            time.sleep(delay)

    # If all retries fail, raise the last captured exception
    raise last_exception

def get_user_info(session_cookie):
    query_user = "query globalData { userStatus { username } }"
    user_data = graphql_request(query_user, {}, session_cookie)
    if user_data == "AUTH_ERROR": return None, 0
    if not user_data or not user_data.get('data', {}).get('userStatus'): return None, 0
    username = user_data['data']['userStatus']['username']

    query_stats = """
    query userProfile($username: String!) {
      matchedUser(username: $username) {
        submitStats: submitStatsGlobal {
          acSubmissionNum { difficulty, count }
        }
      }
    }
    """
    stats_data = graphql_request(query_stats, {"username": username}, session_cookie)
    if not stats_data or not stats_data.get('data', {}).get('matchedUser'): return username, 0
    
    submissions_by_difficulty = stats_data['data']['matchedUser']['submitStats']['acSubmissionNum']
    all_stats = next((s for s in submissions_by_difficulty if s['difficulty'] == 'All'), None)
    total_unique_solved = all_stats['count'] if all_stats else 0
    
    return username, total_unique_solved

def fetch_submission_detail(submission_id, session_cookie):
    query = """
    query submissionDetails($submissionId: Int!) {
      submissionDetails(submissionId: $submissionId) {
        code
        question { questionId, title, content, difficulty }
      }
    }
    """
    return graphql_request(query, {"submissionId": int(submission_id)}, session_cookie)

def save_solution_files(question, code, language_slug):
    q_id = question['questionId'].zfill(4)
    q_title_cleaned = clean_name_for_path(question['title'])
    q_difficulty = question['difficulty']
    problem_folder = os.path.join(BASE_FOLDER, q_difficulty.capitalize(), f"{q_id}. {q_title_cleaned}")
    ext = EXTENSIONS.get(language_slug, "txt")
    code_filename_cleaned = clean_name_for_path(q_title_cleaned).replace(" ", "-")
    code_filepath = os.path.join(problem_folder, f"{code_filename_cleaned}.{ext}")

    if os.path.exists(code_filepath): return False, code_filepath

    os.makedirs(problem_folder, exist_ok=True)
    md_filepath = os.path.join(problem_folder, "README.md")
    if not os.path.exists(md_filepath):
        with open(md_filepath, "w", encoding="utf-8") as f:
            f.write(f"# {q_id}. {question['title']}\n\n**Difficulty:** {q_difficulty}\n\n---\n\n{question['content']}\n")
    with open(code_filepath, "w", encoding="utf-8") as f:
        f.write(code)
    
    return True, code_filepath

def main():
    print(f"Solutions will be saved to: {BASE_FOLDER}")
    os.makedirs(BASE_FOLDER, exist_ok=True)
    
    session_cookie = get_leetcode_session_cookie()
    if not session_cookie: return

    try:
        username, total_unique_solved_count = get_user_info(session_cookie)
    except Exception as e:
        print(f"\n❌ Failed to get user info. Your cookie may be invalid. Error: {e}")
        return

    if not username:
        print("❌ Could not verify session cookie or retrieve user info. Please check your cookie and try again. Exiting.")
        return
    
    print(f"\n✅ Successfully authenticated as '{username}'.")
    print(f"Found {total_unique_solved_count} unique accepted problems in your history.")

    query_submissions = """
    query submissionList($offset: Int!, $limit: Int!, $lastKey: String) {
        submissionList(offset: $offset, limit: $limit, lastKey: $lastKey, questionSlug: "") {
            lastKey, hasNext, submissions { id, title, titleSlug, timestamp, statusDisplay, lang }
        }
    }
    """
    
    PAGE_SIZE = 20
    has_next = True
    last_key = None
    offset = 0
    processed_solutions, saved_count, skipped_count = set(), 0, 0
    
    print(f"\nProcessing submission history... (Page Size / Concurrent Workers: {PAGE_SIZE})")
    while has_next:
        variables = {"offset": offset, "limit": PAGE_SIZE, "lastKey": last_key}
        
        try:
            result = graphql_request(query_submissions, variables, session_cookie, retries=5) # More retries for the main list
        except Exception as e:
            print(f"\n❌ Failed to fetch submission list page: {e}. Stopping.")
            break
        
        if not result or 'data' not in result or 'submissionList' not in result['data']:
            print("\n❌ Stopping pagination due to an error or invalid response.")
            break

        submission_list = result['data']['submissionList']
        submissions = submission_list.get('submissions', [])
        if not submissions: break

        newly_accepted_subs = []
        for sub in submissions:
            if sub['statusDisplay'] == 'Accepted' and (sub['titleSlug'], sub['lang']) not in processed_solutions:
                processed_solutions.add((sub['titleSlug'], sub['lang']))
                newly_accepted_subs.append(sub)

        if newly_accepted_subs:
            with ThreadPoolExecutor(max_workers=PAGE_SIZE) as executor:
                future_to_sub = {executor.submit(fetch_submission_detail, sub['id'], session_cookie): sub for sub in newly_accepted_subs}
                
                for future in as_completed(future_to_sub):
                    sub = future_to_sub[future]
                    try:
                        details = future.result()
                        submission_data = details.get('data', {}).get('submissionDetails') if details else None
                        
                        if submission_data and submission_data.get('question') and submission_data.get('code'):
                            question, code = submission_data['question'], submission_data['code']
                            processed_count = saved_count + skipped_count
                            print(f"[Processed: {processed_count+1:4d}] Checking: {question['title']} ({question['difficulty']}) [{sub['lang']}]")
                            saved, filepath = save_solution_files(question, code, sub['lang'])
                            if saved:
                                saved_count += 1
                                print(f"   -> ✅ Saved: {os.path.basename(filepath)}")
                            else:
                                skipped_count += 1
                                print(f"   -> ⏩ Skipped (exists): {os.path.basename(filepath)}")
                        else:
                            print(f"   -> ⚠️  Could not fetch valid details for '{sub['title']}'. Skipping.")
                    except Exception as e:
                        print(f"❌ Final error after retries for '{sub['title']}': {e}")
        
        time.sleep(1)
        has_next = submission_list['hasNext']
        last_key = submission_list['lastKey']
        offset += len(submissions)
        print(f"\n--- Page finished. Scanned {offset} total submissions from history ---\n")

    print(f"\n🎉 All done! Processed all available submission history.")
    print(f"   - Saved:   {saved_count} new unique solution(s).")
    print(f"   - Skipped: {skipped_count} solution(s) that already existed.")
    input("Press Enter to exit.")

if __name__ == "__main__":
    main()