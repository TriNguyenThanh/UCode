#!/usr/bin/env python3
"""
Test script to submit code to API and poll for results
"""
import requests
import time
import json
from datetime import datetime

# API Configuration
BASE_URL = "http://localhost:5000/api/v1/submissions"
SUBMIT_ENDPOINT = f"{BASE_URL}/submit-code"
POLL_ENDPOINT = f"{BASE_URL}/{{submission_id}}"  # Will be formatted with submission_id

# Headers
HEADERS = {
    "accept": "application/json, text/plain, */*",
    "accept-encoding": "gzip, deflate, br, zstd",
    "accept-language": "en-US,en;q=0.9",
    "authorization": "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiIxMWQ5Nzc3OS1mZmYyLTQ4OTUtYWQ5NC03NzIzOTY4YzBkNTAiLCJ1bmlxdWVfbmFtZSI6InRlYWNoZXIwMSIsInJvbGUiOiJUZWFjaGVyIiwianRpIjoiNGUzMGNlZjgtMDBkZC00N2NjLTg1NTYtNzgwMjhiMDIxZTdjIiwiaWF0IjoxNzYyNjk5NjYyLCJ0ZWFjaGVyQ29kZSI6IkdWMDAxIiwibmJmIjoxNzYyNjk5NjYyLCJleHAiOjE3NjMzMDQ0NjIsImlzcyI6IlVzZXJTZXJ2aWNlIiwiYXVkIjoiVXNlclNlcnZpY2VDbGllbnQifQ.cf3RcFyTLWH-rl8nTDeTsc9AhRtrOFh4kKyvp-9o_fo",
    "connection": "keep-alive",
    "content-type": "application/json",
    "origin": "http://localhost:3000",
    "referer": "http://localhost:3000/",
    "sec-ch-ua": '"Chromium";v="142", "Google Chrome";v="142", "Not_A Brand";v="99"',
    "sec-ch-ua-mobile": "?0",
    "sec-ch-ua-platform": '"Linux"',
    "sec-fetch-dest": "empty",
    "sec-fetch-mode": "cors",
    "sec-fetch-site": "same-site",
    "user-agent": "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/142.0.0.0 Safari/537.36"
}

# Payload
PAYLOAD = {
    "languageId": "eac3cb6a-c218-4454-953f-138cfb22e60c",
    "problemId": "6f6af8f8-da44-4eb2-9dd3-1e08abeb2f31",
    "sourceCode": "#include <iostream>\n\n// Hàm đệ quy đơn giản\nunsigned long long fib_recursive(int n) {\n    // 1. Trường hợp cơ bản (điểm dừng của đệ quy)\n    if (n == 0) {\n        return 0;\n    }\n    if (n == 1) {\n        return 1;\n    }\n\n    // 2. Bước đệ quy (hàm gọi lại chính nó)\n    return fib_recursive(n - 1) + fib_recursive(n - 2);\n}\n\nint main() {\n    int n;\n    std::cin >> n;\n    std::cout << fib_recursive(n) << std::endl;\n    return 0;\n}"
}

# Polling Configuration
POLL_INTERVAL = 2  # seconds
MAX_POLL_ATTEMPTS = 150  # 150 * 2s = 5 minutes max


def print_section(title):
    """Print formatted section header"""
    print("\n" + "=" * 70)
    print(f"  {title}")
    print("=" * 70)


def submit_code():
    """Submit code to API and return submission ID"""
    print_section("📤 SUBMITTING CODE")
    
    print(f"URL: {SUBMIT_ENDPOINT}")
    print(f"Payload size: {len(json.dumps(PAYLOAD))} bytes")
    print(f"Code length: {len(PAYLOAD['sourceCode'])} characters")
    
    try:
        response = requests.post(
            SUBMIT_ENDPOINT,
            headers=HEADERS,
            json=PAYLOAD,
            timeout=30
        )
        
        print(f"\n📊 Response Status: {response.status_code}")
        
        if response.status_code in [200, 201]:
            result = response.json()
            print(f"✅ Submission successful!")
            print(f"Response: {json.dumps(result, indent=2)}")
            
            # Extract submission ID from nested data structure
            # Response format: {"success": true, "data": {"submissionId": "...", ...}}
            if result.get('success'):
                data = result.get('data', {})
                submission_id = data.get('submissionId')
                status = data.get('status')
                submitted_at = data.get('submittedAt')
                message = result.get('message')
                
                if submission_id:
                    print(f"\n🎯 Submission ID: {submission_id}")
                    print(f"📊 Initial Status: {status}")
                    print(f"💬 Message: {message}")
                    print(f"⏰ Submitted At: {submitted_at}")
                    return submission_id
                else:
                    print(f"⚠️  Could not extract submission ID from response")
                    return None
            else:
                print(f"❌ Submission not successful")
                print(f"Errors: {result.get('errors')}")
                return None
        else:
            print(f"❌ Submission failed!")
            print(f"Response: {response.text}")
            return None
            
    except requests.exceptions.RequestException as e:
        print(f"❌ Request error: {e}")
        return None


def poll_submission(submission_id):
    """Poll for submission results until status is Passed or Failed"""
    print_section(f"🔄 POLLING FOR RESULTS - {submission_id}")
    
    poll_url = POLL_ENDPOINT.format(submission_id=submission_id)
    print(f"Poll URL: {poll_url}")
    print(f"Poll interval: {POLL_INTERVAL}s")
    print(f"Max attempts: {MAX_POLL_ATTEMPTS}")
    print(f"Waiting for status: Passed or Failed")
    
    attempt = 0
    start_time = time.time()
    
    while attempt < MAX_POLL_ATTEMPTS:
        attempt += 1
        elapsed = time.time() - start_time
        
        print(f"\n[{datetime.now().strftime('%H:%M:%S')}] Attempt {attempt}/{MAX_POLL_ATTEMPTS} (elapsed: {elapsed:.1f}s)")
        
        try:
            response = requests.get(
                poll_url,
                headers=HEADERS,
                timeout=10
            )
            
            if response.status_code == 200:
                result = response.json()
                
                # Check if response is successful
                if result.get('success'):
                    data = result.get('data', {})
                    
                    # Extract status from nested data structure
                    status = data.get('status', 'Unknown')
                    compare_result = data.get('compareResult', '')
                    error_code = data.get('errorCode', '')
                    error_message = data.get('errorMessage', '')
                    total_time = data.get('totalTime', 0)
                    total_memory = data.get('totalMemory', 0)
                    
                    print(f"   Status: {status}")
                    print(f"   Compare Result: {compare_result[:50]}..." if compare_result else "")
                    
                    if total_time > 0 or total_memory > 0:
                        print(f"   Time: {total_time}ms, Memory: {total_memory}KB")
                    
                    # Check if submission is complete (Passed or Failed)
                    if status in ['Passed', 'Failed']:
                        print_section("✅ SUBMISSION COMPLETE")
                        print(f"Final Status: {status}")
                        print(f"\nFull Response:")
                        print(json.dumps(result, indent=2))
                        
                        # Print summary
                        print(f"\n📊 SUMMARY:")
                        print(f"   Status: {status}")
                        print(f"   Total Time: {total_time}ms")
                        print(f"   Total Memory: {total_memory}KB")
                        print(f"   Compare Result: {compare_result}")
                        
                        if error_code or error_message:
                            print(f"\n⚠️  ERRORS:")
                            print(f"   Error Code: {error_code}")
                            print(f"   Error Message: {error_message}")
                        
                        return result
                    else:
                        # Still processing (Pending, Running, etc.)
                        print(f"   ⏳ Still processing... (Status: {status})")
                else:
                    # API returned success=false
                    print(f"   ⚠️  API Error: {result.get('message')}")
                    errors = result.get('errors', [])
                    if errors:
                        print(f"   Errors: {errors}")
                        
            elif response.status_code == 404:
                print(f"   ⚠️  Submission not found (404)")
            else:
                print(f"   ⚠️  Unexpected status code: {response.status_code}")
                print(f"   Response: {response.text[:200]}")
                
        except requests.exceptions.RequestException as e:
            print(f"   ❌ Poll error: {e}")
        except json.JSONDecodeError as e:
            print(f"   ❌ JSON decode error: {e}")
            print(f"   Response text: {response.text[:200]}")
        
        # Wait before next poll
        if attempt < MAX_POLL_ATTEMPTS:
            time.sleep(POLL_INTERVAL)
    
    print_section("⏰ POLLING TIMEOUT")
    print(f"Max attempts ({MAX_POLL_ATTEMPTS}) reached after {elapsed:.1f}s")
    print(f"Submission did not reach final state (Passed/Failed)")
    return None


def main():
    """Main execution"""
    print_section("🚀 CODE SUBMISSION TEST")
    print(f"Timestamp: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
    
    # Step 1: Submit code
    submission_id = submit_code()
    
    if not submission_id:
        print("\n❌ Failed to submit code. Exiting.")
        return
    
    # Step 2: Poll for results
    print("\n⏳ Waiting 3 seconds before polling...")
    time.sleep(3)
    
    result = poll_submission(submission_id)
    
    if result:
        print_section("🎉 TEST COMPLETE")
        print("Final result received successfully!")
    else:
        print_section("❌ TEST FAILED")
        print("Did not receive final result within timeout period.")


if __name__ == "__main__":
    main()
