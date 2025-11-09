#!/usr/bin/env python3
"""
Simple Python test - Submit và poll kết quả
"""
import requests
import time
import json

# Config
API_URL = "http://localhost:5000/api/v1/submissions/submit-code"
TOKEN = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiIxMWQ5Nzc3OS1mZmYyLTQ4OTUtYWQ5NC03NzIzOTY4YzBkNTAiLCJ1bmlxdWVfbmFtZSI6InRlYWNoZXIwMSIsInJvbGUiOiJUZWFjaGVyIiwianRpIjoiNGUzMGNlZjgtMDBkZC00N2NjLTg1NTYtNzgwMjhiMDIxZTdjIiwiaWF0IjoxNzYyNjk5NjYyLCJ0ZWFjaGVyQ29kZSI6IkdWMDAxIiwibmJmIjoxNzYyNjk5NjYyLCJleHAiOjE3NjMzMDQ0NjIsImlzcyI6IlVzZXJTZXJ2aWNlIiwiYXVkIjoiVXNlclNlcnZpY2VDbGllbnQifQ.cf3RcFyTLWH-rl8nTDeTsc9AhRtrOFh4kKyvp-9o_fo"

# Simple Python code
SIMPLE_CODE = """n = int(input())
a, b = 0, 1
for _ in range(n):
    a, b = b, a + b
print(a)"""

# C++ Fibonacci code
CPP_FIBONACCI = """#include <iostream>

// Hàm đệ quy đơn giản
unsigned long long fib_recursive(int n) {
    // 1. Trường hợp cơ bản (điểm dừng của đệ quy)
    if (n == 0) {
        return 0;
    }
    if (n == 1) {
        return 1;
    }

    // 2. Bước đệ quy (hàm gọi lại chính nó)
    return fib_recursive(n - 1) + fib_recursive(n - 2);
}

int main() {
    int n;
    std::cin >> n;
    std::cout << fib_recursive(n) << std::endl;
    return 0;
}"""

def submit_and_poll(language_id, problem_id, source_code, description=""):
    """Submit code và poll kết quả cho đến khi Passed hoặc Failed"""
    
    print(f"\n{'='*70}")
    print(f"📤 Submitting: {description}")
    print(f"{'='*70}")
    
    # Submit
    headers = {
        "authorization": TOKEN,
        "content-type": "application/json"
    }
    
    payload = {
        "languageId": language_id,
        "problemId": problem_id,
        "sourceCode": source_code
    }
    
    try:
        print(f"POST {API_URL}")
        response = requests.post(API_URL, headers=headers, json=payload, timeout=30)
        print(f"Status: {response.status_code}")
        
        if response.status_code in [200, 201]:
            result = response.json()
            print(f"✅ Response: {json.dumps(result, indent=2)}")
            
            # Extract submission ID from response structure
            # {"success": true, "data": {"submissionId": "...", "status": "Running", ...}}
            if not result.get('success'):
                print(f"❌ Submission failed: {result.get('message')}")
                return None
            
            data = result.get('data', {})
            submission_id = data.get('submissionId')
            
            if submission_id:
                print(f"\n🎯 Submission ID: {submission_id}")
                print(f"📊 Initial Status: {data.get('status')}")
                
                # Poll for result
                poll_url = f"http://localhost:5000/api/v1/submissions/{submission_id}"
                print(f"\n⏳ Polling: {poll_url}")
                print(f"⏸️  Waiting for status: Passed or Failed\n")
                
                attempt = 0
                start_time = time.time()
                
                # Poll until Passed or Failed (max 5 minutes)
                while attempt < 150:
                    attempt += 1
                    time.sleep(2)
                    
                    poll_resp = requests.get(poll_url, headers=headers, timeout=10)
                    
                    if poll_resp.status_code == 200:
                        poll_result = poll_resp.json()
                        
                        if poll_result.get('success'):
                            poll_data = poll_result.get('data', {})
                            status = poll_data.get('status', 'Unknown')
                            total_time = poll_data.get('totalTime', 0)
                            total_memory = poll_data.get('totalMemory', 0)
                            
                            elapsed = time.time() - start_time
                            print(f"[{attempt:3d}] {elapsed:6.1f}s | Status: {status:10s} | Time: {total_time:4d}ms | Mem: {total_memory:6d}KB")
                            
                            # Check if completed (Passed or Failed)
                            if status in ['Passed', 'Failed']:
                                print(f"\n{'='*70}")
                                print(f"✅ COMPLETE! Final Status: {status}")
                                print(f"{'='*70}")
                                
                                compare_result = poll_data.get('compareResult', '')
                                error_message = poll_data.get('errorMessage', '')
                                
                                print(f"\n📊 SUMMARY:")
                                print(f"   Status: {status}")
                                print(f"   Total Time: {total_time}ms")
                                print(f"   Total Memory: {total_memory}KB")
                                print(f"   Compare Result: {compare_result}")
                                
                                if error_message:
                                    print(f"   Error: {error_message}")
                                
                                print(f"\n📄 Full Response:")
                                print(json.dumps(poll_result, indent=2))
                                
                                return poll_result
                        else:
                            print(f"[{attempt:3d}] API Error: {poll_result.get('message')}")
                    else:
                        print(f"[{attempt:3d}] HTTP {poll_resp.status_code}")
                
                print(f"\n⏰ Timeout after {attempt} attempts ({time.time() - start_time:.1f}s)")
            else:
                print("❌ No submission ID in response")
        else:
            print(f"❌ Error: {response.text}")
            
    except Exception as e:
        print(f"❌ Exception: {e}")
        import traceback
        traceback.print_exc()
    
    return None


if __name__ == "__main__":
    print("🚀 API Submission Test - Simple Version")
    
    # Test với C++ Fibonacci
    submit_and_poll(
        language_id="eac3cb6a-c218-4454-953f-138cfb22e60c",  # C++
        problem_id="6f6af8f8-da44-4eb2-9dd3-1e08abeb2f31",   # Fibonacci problem
        source_code=CPP_FIBONACCI,
        description="C++ Fibonacci (Recursive)"
    )
    
    print("\n" + "="*70)
    print("✅ Test completed!")
    print("="*70)
