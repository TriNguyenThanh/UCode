#!/usr/bin/env python3
"""
Concurrent Test Runner - Chạy nhiều test submission cùng lúc
Dùng để test khả năng xử lý nhiều request đồng thời của judge service
"""
import subprocess
import time
import threading
import json
from datetime import datetime
from concurrent.futures import ThreadPoolExecutor, as_completed

# Configuration
NUM_CONCURRENT_TESTS = 10  # Số lượng test chạy đồng thời
TEST_SCRIPT = "./run_test.sh"  # Script để chạy
MAX_WORKERS = 10  # Max số thread pool workers

# Statistics
results = {
    "success": 0,
    "failed": 0,
    "total": 0,
    "start_time": None,
    "end_time": None,
    "durations": []
}

results_lock = threading.Lock()


def print_header(title):
    """Print formatted header"""
    print("\n" + "=" * 80)
    print(f"  {title}")
    print("=" * 80)


def run_single_test(test_id):
    """
    Chạy một test submission
    
    Args:
        test_id: ID của test (để tracking)
        
    Returns:
        dict: Kết quả test
    """
    start_time = time.time()
    result = {
        "test_id": test_id,
        "success": False,
        "duration": 0,
        "output": "",
        "error": ""
    }
    
    try:
        print(f"[Test {test_id:2d}] 🚀 Starting at {datetime.now().strftime('%H:%M:%S')}")
        
        # Chạy test script
        process = subprocess.run(
            ["bash", TEST_SCRIPT],
            capture_output=True,
            text=True,
            timeout=300  # 5 minutes timeout
        )
        
        duration = time.time() - start_time
        result["duration"] = duration
        result["output"] = process.stdout
        result["error"] = process.stderr
        result["return_code"] = process.returncode
        
        if process.returncode == 0:
            result["success"] = True
            print(f"[Test {test_id:2d}] ✅ Completed in {duration:.1f}s")
        else:
            print(f"[Test {test_id:2d}] ❌ Failed after {duration:.1f}s (code: {process.returncode})")
            
    except subprocess.TimeoutExpired:
        duration = time.time() - start_time
        result["duration"] = duration
        result["error"] = "Timeout after 300s"
        print(f"[Test {test_id:2d}] ⏰ Timeout after {duration:.1f}s")
        
    except Exception as e:
        duration = time.time() - start_time
        result["duration"] = duration
        result["error"] = str(e)
        print(f"[Test {test_id:2d}] ❌ Exception: {e}")
    
    # Update statistics
    with results_lock:
        results["total"] += 1
        results["durations"].append(duration)
        if result["success"]:
            results["success"] += 1
        else:
            results["failed"] += 1
    
    return result


def run_concurrent_tests(num_tests, max_workers=None):
    """
    Chạy nhiều test cùng lúc sử dụng ThreadPoolExecutor
    
    Args:
        num_tests: Số lượng test cần chạy
        max_workers: Số lượng worker thread (mặc định = num_tests)
    """
    print_header(f"🚀 RUNNING {num_tests} CONCURRENT TESTS")
    
    if max_workers is None:
        max_workers = min(num_tests, MAX_WORKERS)
    
    print(f"📊 Configuration:")
    print(f"   Number of tests: {num_tests}")
    print(f"   Max workers: {max_workers}")
    print(f"   Test script: {TEST_SCRIPT}")
    print(f"   Timeout per test: 300s")
    
    results["start_time"] = time.time()
    test_results = []
    
    # Chạy tests sử dụng ThreadPoolExecutor
    with ThreadPoolExecutor(max_workers=max_workers) as executor:
        # Submit all tests
        futures = {executor.submit(run_single_test, i+1): i+1 for i in range(num_tests)}
        
        print(f"\n⏳ All {num_tests} tests submitted. Waiting for completion...\n")
        
        # Collect results as they complete
        for future in as_completed(futures):
            test_id = futures[future]
            try:
                result = future.result()
                test_results.append(result)
            except Exception as e:
                print(f"[Test {test_id:2d}] ❌ Exception in future: {e}")
    
    results["end_time"] = time.time()
    
    return test_results


def print_summary(test_results):
    """In tóm tắt kết quả"""
    print_header("📊 TEST SUMMARY")
    
    total_duration = results["end_time"] - results["start_time"]
    
    print(f"\n⏱️  Overall Timing:")
    print(f"   Total Duration: {total_duration:.2f}s")
    print(f"   Start Time: {datetime.fromtimestamp(results['start_time']).strftime('%H:%M:%S')}")
    print(f"   End Time: {datetime.fromtimestamp(results['end_time']).strftime('%H:%M:%S')}")
    
    print(f"\n📈 Results:")
    print(f"   Total Tests: {results['total']}")
    print(f"   ✅ Successful: {results['success']}")
    print(f"   ❌ Failed: {results['failed']}")
    print(f"   Success Rate: {results['success']/results['total']*100:.1f}%")
    
    if results["durations"]:
        durations = results["durations"]
        print(f"\n⏱️  Individual Test Duration:")
        print(f"   Min: {min(durations):.2f}s")
        print(f"   Max: {max(durations):.2f}s")
        print(f"   Average: {sum(durations)/len(durations):.2f}s")
        print(f"   Total Sequential Time: {sum(durations):.2f}s")
        print(f"   Speedup: {sum(durations)/total_duration:.2f}x")
    
    # Detailed results table
    print(f"\n📋 Detailed Results:")
    print(f"{'ID':^5} | {'Status':^10} | {'Duration':^12} | {'Notes':^40}")
    print("-" * 80)
    
    for result in sorted(test_results, key=lambda x: x["test_id"]):
        status = "✅ SUCCESS" if result["success"] else "❌ FAILED"
        duration = f"{result['duration']:.2f}s"
        notes = result.get("error", "")[:40] if not result["success"] else "OK"
        print(f"{result['test_id']:^5} | {status:^10} | {duration:^12} | {notes:<40}")


def save_results(test_results, filename="test_results.json"):
    """Lưu kết quả vào file JSON"""
    output = {
        "timestamp": datetime.now().isoformat(),
        "summary": {
            "total": results["total"],
            "success": results["success"],
            "failed": results["failed"],
            "total_duration": results["end_time"] - results["start_time"],
            "min_duration": min(results["durations"]) if results["durations"] else 0,
            "max_duration": max(results["durations"]) if results["durations"] else 0,
            "avg_duration": sum(results["durations"])/len(results["durations"]) if results["durations"] else 0
        },
        "results": test_results
    }
    
    with open(filename, "w") as f:
        json.dump(output, f, indent=2)
    
    print(f"\n💾 Results saved to: {filename}")


def run_sequential_tests(num_tests):
    """Chạy tests tuần tự (để so sánh performance)"""
    print_header(f"🔄 RUNNING {num_tests} SEQUENTIAL TESTS")
    
    print(f"📊 Configuration:")
    print(f"   Number of tests: {num_tests}")
    print(f"   Mode: Sequential (one by one)")
    
    results["start_time"] = time.time()
    test_results = []
    
    for i in range(num_tests):
        result = run_single_test(i + 1)
        test_results.append(result)
    
    results["end_time"] = time.time()
    
    return test_results


def main():
    """Main function"""
    print_header("🧪 CONCURRENT TEST RUNNER")
    print(f"Timestamp: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
    
    # Menu
    print("\nSelect test mode:")
    print("1. Concurrent tests (default)")
    print("2. Sequential tests (for comparison)")
    print("3. Custom configuration")
    
    try:
        choice = input("\nEnter choice (1-3, default=1): ").strip() or "1"
    except (EOFError, KeyboardInterrupt):
        choice = "1"
        print()
    
    num_tests = NUM_CONCURRENT_TESTS
    max_workers = MAX_WORKERS
    
    if choice == "3":
        try:
            num_tests = int(input(f"Number of tests (default={NUM_CONCURRENT_TESTS}): ").strip() or NUM_CONCURRENT_TESTS)
            max_workers = int(input(f"Max workers (default={MAX_WORKERS}): ").strip() or MAX_WORKERS)
        except (ValueError, EOFError, KeyboardInterrupt):
            print("Using default values")
    
    # Run tests
    if choice == "2":
        test_results = run_sequential_tests(num_tests)
    else:
        test_results = run_concurrent_tests(num_tests, max_workers)
    
    # Print summary
    print_summary(test_results)
    
    # Save results
    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
    mode = "sequential" if choice == "2" else "concurrent"
    filename = f"test_results_{mode}_{timestamp}.json"
    save_results(test_results, filename)
    
    print_header("✅ TEST COMPLETED")


if __name__ == "__main__":
    try:
        main()
    except KeyboardInterrupt:
        print("\n\n⚠️  Tests interrupted by user")
        if results["total"] > 0:
            print_summary([])
    except Exception as e:
        print(f"\n❌ Fatal error: {e}")
        import traceback
        traceback.print_exc()
