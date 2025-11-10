#!/usr/bin/env python3
"""
Multi-process API Test - Gửi nhiều submission cùng lúc
Test load balancing và concurrent processing của judge service
"""
import requests
import time
import json
from datetime import datetime
from concurrent.futures import ThreadPoolExecutor, as_completed
import threading

# API Configuration
API_URL = "http://localhost:5000/api/v1/submissions/submit-code"
TOKEN = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiIxMWQ5Nzc3OS1mZmYyLTQ4OTUtYWQ5NC03NzIzOTY4YzBkNTAiLCJ1bmlxdWVfbmFtZSI6InRlYWNoZXIwMSIsInJvbGUiOiJUZWFjaGVyIiwianRpIjoiNGUzMGNlZjgtMDBkZC00N2NjLTg1NTYtNzgwMjhiMDIxZTdjIiwiaWF0IjoxNzYyNjk5NjYyLCJ0ZWFjaGVyQ29kZSI6IkdWMDAxIiwibmJmIjoxNzYyNjk5NjYyLCJleHAiOjE3NjMzMDQ0NjIsImlzcyI6IlVzZXJTZXJ2aWNlIiwiYXVkIjoiVXNlclNlcnZpY2VDbGllbnQifQ.cf3RcFyTLWH-rl8nTDeTsc9AhRtrOFh4kKyvp-9o_fo"

# Test Configuration
NUM_CONCURRENT_SUBMISSIONS = 5
LANGUAGE_ID = "eac3cb6a-c218-4454-953f-138cfb22e60c"  # C++
PROBLEM_ID = "6f6af8f8-da44-4eb2-9dd3-1e08abeb2f31"   # Fibonacci

# Code samples
CPP_FIBONACCI = """#include <iostream>

unsigned long long fib_recursive(int n) {
    if (n == 0) return 0;
    if (n == 1) return 1;
    return fib_recursive(n - 1) + fib_recursive(n - 2);
}

int main() {
    int n;
    std::cin >> n;
    std::cout << fib_recursive(n) << std::endl;
    return 0;
}"""

# Thread-safe stats
stats_lock = threading.Lock()
stats = {
    "submitted": 0,
    "polling": 0,
    "completed": 0,
    "failed": 0,
    "passed": 0
}


def submit_code(test_id):
    """Submit code và trả về submission ID"""
    headers = {
        "authorization": TOKEN,
        "content-type": "application/json"
    }
    
    payload = {
        "languageId": LANGUAGE_ID,
        "problemId": PROBLEM_ID,
        "sourceCode": CPP_FIBONACCI
    }
    
    try:
        response = requests.post(API_URL, headers=headers, json=payload, timeout=30)
        
        if response.status_code in [200, 201]:
            result = response.json()
            if result.get('success'):
                data = result.get('data', {})
                submission_id = data.get('submissionId')
                
                with stats_lock:
                    stats["submitted"] += 1
                
                return submission_id
        
        return None
        
    except Exception as e:
        print(f"[{test_id:2d}] ❌ Submit error: {e}")
        return None


def poll_until_complete(test_id, submission_id):
    """Poll submission cho đến khi Passed hoặc Failed"""
    poll_url = f"http://localhost:5000/api/v1/submissions/{submission_id}"
    headers = {
        "authorization": TOKEN,
        "content-type": "application/json"
    }
    
    with stats_lock:
        stats["polling"] += 1
    
    attempt = 0
    start_time = time.time()
    
    while attempt < 150:  # Max 5 minutes
        attempt += 1
        time.sleep(2)
        
        try:
            response = requests.get(poll_url, headers=headers, timeout=10)
            
            if response.status_code == 200:
                result = response.json()
                
                if result.get('success'):
                    data = result.get('data', {})
                    status = data.get('status', 'Unknown')
                    
                    # Check if completed
                    if status in ['Passed', 'Failed']:
                        duration = time.time() - start_time
                        
                        with stats_lock:
                            stats["completed"] += 1
                            if status == 'Passed':
                                stats["passed"] += 1
                            else:
                                stats["failed"] += 1
                        
                        return {
                            "test_id": test_id,
                            "submission_id": submission_id,
                            "status": status,
                            "duration": duration,
                            "total_time": data.get('totalTime', 0),
                            "total_memory": data.get('totalMemory', 0),
                            "compare_result": data.get('compareResult', ''),
                            "error_message": data.get('errorMessage', '')
                        }
                        
        except Exception as e:
            pass  # Continue polling
    
    # Timeout
    duration = time.time() - start_time
    return {
        "test_id": test_id,
        "submission_id": submission_id,
        "status": "Timeout",
        "duration": duration,
        "error": "Polling timeout"
    }


def run_single_test(test_id):
    """Chạy một test submission hoàn chỉnh"""
    start_time = time.time()
    
    print(f"[{test_id:2d}] 🚀 Starting at {datetime.now().strftime('%H:%M:%S.%f')[:-3]}")
    
    # Submit
    submission_id = submit_code(test_id)
    
    if not submission_id:
        print(f"[{test_id:2d}] ❌ Failed to submit")
        return {
            "test_id": test_id,
            "status": "SubmitFailed",
            "duration": time.time() - start_time
        }
    
    print(f"[{test_id:2d}] ✅ Submitted: {submission_id}")
    
    # Poll
    result = poll_until_complete(test_id, submission_id)
    
    # Print result
    status_icon = "✅" if result["status"] == "Passed" else "❌"
    print(f"[{test_id:2d}] {status_icon} {result['status']} in {result['duration']:.1f}s "
          f"(Time: {result.get('total_time', 0)}ms, Mem: {result.get('total_memory', 0)}KB)")
    
    return result


def main():
    """Main function"""
    print("=" * 80)
    print("  🚀 CONCURRENT API SUBMISSION TEST")
    print("=" * 80)
    print(f"Timestamp: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
    print(f"API: {API_URL}")
    print(f"Concurrent submissions: {NUM_CONCURRENT_SUBMISSIONS}")
    print("=" * 80)
    
    overall_start = time.time()
    results = []
    
    # Run concurrent tests
    with ThreadPoolExecutor(max_workers=NUM_CONCURRENT_SUBMISSIONS) as executor:
        futures = {executor.submit(run_single_test, i+1): i+1 
                   for i in range(NUM_CONCURRENT_SUBMISSIONS)}
        
        print(f"\n⏳ All {NUM_CONCURRENT_SUBMISSIONS} submissions started...\n")
        
        for future in as_completed(futures):
            try:
                result = future.result()
                results.append(result)
            except Exception as e:
                test_id = futures[future]
                print(f"[{test_id:2d}] ❌ Exception: {e}")
    
    overall_duration = time.time() - overall_start
    
    # Print summary
    print("\n" + "=" * 80)
    print("  📊 SUMMARY")
    print("=" * 80)
    
    print(f"\n⏱️  Overall Duration: {overall_duration:.2f}s")
    print(f"\n📈 Statistics:")
    print(f"   Submitted: {stats['submitted']}")
    print(f"   Polling: {stats['polling']}")
    print(f"   Completed: {stats['completed']}")
    print(f"   ✅ Passed: {stats['passed']}")
    print(f"   ❌ Failed: {stats['failed']}")
    
    if results:
        durations = [r['duration'] for r in results]
        print(f"\n⏱️  Test Durations:")
        print(f"   Min: {min(durations):.2f}s")
        print(f"   Max: {max(durations):.2f}s")
        print(f"   Avg: {sum(durations)/len(durations):.2f}s")
    
    # Detailed table
    print(f"\n📋 Detailed Results:")
    print(f"{'ID':^5} | {'Submission ID':^38} | {'Status':^10} | {'Duration':^10} | {'Time':^8} | {'Mem':^8}")
    print("-" * 95)
    
    for r in sorted(results, key=lambda x: x['test_id']):
        sid = r.get('submission_id', 'N/A')[:36]
        status = r.get('status', 'Unknown')
        duration = f"{r['duration']:.1f}s"
        total_time = f"{r.get('total_time', 0)}ms"
        total_mem = f"{r.get('total_memory', 0)}KB"
        
        print(f"{r['test_id']:^5} | {sid:^38} | {status:^10} | {duration:^10} | {total_time:^8} | {total_mem:^8}")
    
    # Save results
    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
    filename = f"api_test_results_{timestamp}.json"
    
    output = {
        "timestamp": datetime.now().isoformat(),
        "config": {
            "num_concurrent": NUM_CONCURRENT_SUBMISSIONS,
            "api_url": API_URL
        },
        "summary": {
            "overall_duration": overall_duration,
            "stats": stats
        },
        "results": results
    }
    
    with open(filename, "w") as f:
        json.dump(output, f, indent=2)
    
    print(f"\n💾 Results saved to: {filename}")
    print("\n" + "=" * 80)
    print("✅ TEST COMPLETED")
    print("=" * 80)


if __name__ == "__main__":
    try:
        main()
    except KeyboardInterrupt:
        print("\n\n⚠️  Test interrupted by user")
    except Exception as e:
        print(f"\n❌ Fatal error: {e}")
        import traceback
        traceback.print_exc()
