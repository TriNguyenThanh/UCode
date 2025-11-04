#!/usr/bin/env python3
"""
Test integration giữa executor_isolate và message_handler
"""
import json
from app.executor_isolate import execute_in_sandbox, TESTCASE_STATUS

def test_python_simple():
    """Test simple Python code"""
    print("=" * 80)
    print("TEST 1: Python - Simple Addition")
    print("=" * 80)
    
    code = """
a, b = map(int, input().split())
print(a + b)
"""
    
    # Format C# chuẩn với TestCaseId, IndexNo, InputRef, OutputRef
    testcases = [
        {
            "TestCaseId": "tc-001",
            "IndexNo": 0,
            "InputRef": "1 2",
            "OutputRef": "3"
        },
        {
            "TestCaseId": "tc-002",
            "IndexNo": 1,
            "InputRef": "5 7",
            "OutputRef": "12"
        },
        {
            "TestCaseId": "tc-003",
            "IndexNo": 2,
            "InputRef": "10 20",
            "OutputRef": "30"
        },
    ]
    
    results = execute_in_sandbox(
        language="python",
        code=code,
        testcases=testcases,
        timelimit=2,
        memorylimit=128
    )
    
    print(json.dumps(results, indent=2))
    
    # Verify format
    for r in results:
        assert "testcaseId" in r
        assert "indexNo" in r  # New field
        assert "status" in r
        assert "time" in r
        assert "memory" in r
        assert "output" in r
        assert "error" in r
        assert r["status"] == TESTCASE_STATUS.Passed
    
    # Verify order by IndexNo
    for i, r in enumerate(results):
        assert r["indexNo"] == i, f"Expected indexNo {i}, got {r['indexNo']}"
    
    print("✅ All testcases passed!")
    return results


def test_python_wrong_answer():
    """Test Python code with wrong answer"""
    print("\n" + "=" * 80)
    print("TEST 2: Python - Wrong Answer")
    print("=" * 80)
    
    code = """
a, b = map(int, input().split())
print(a - b)  # Wrong: should be addition
"""
    
    testcases = [
        {
            "TestCaseId": "tc-001",
            "IndexNo": 0,
            "InputRef": "1 2",
            "OutputRef": "3"
        },
    ]
    
    results = execute_in_sandbox(
        language="python",
        code=code,
        testcases=testcases,
        timelimit=2,
        memorylimit=128
    )
    
    print(json.dumps(results, indent=2))
    assert results[0]["status"] == TESTCASE_STATUS.WrongAnswer
    print("✅ Wrong answer detected correctly!")
    return results


def test_cpp_simple():
    """Test simple C++ code"""
    print("\n" + "=" * 80)
    print("TEST 3: C++ - Simple Addition")
    print("=" * 80)
    
    code = """
#include <iostream>
using namespace std;

int main() {
    int a, b;
    cin >> a >> b;
    cout << a + b << endl;
    return 0;
}
"""
    
    testcases = [
        {
            "TestCaseId": "tc-001",
            "IndexNo": 0,
            "InputRef": "1 2",
            "OutputRef": "3"
        },
        {
            "TestCaseId": "tc-002",
            "IndexNo": 1,
            "InputRef": "5 7",
            "OutputRef": "12"
        },
    ]
    
    results = execute_in_sandbox(
        language="cpp",
        code=code,
        testcases=testcases,
        timelimit=2,
        memorylimit=128
    )
    
    print(json.dumps(results, indent=2))
    
    for r in results:
        assert r["status"] == TESTCASE_STATUS.Passed
    
    print("✅ All C++ testcases passed!")
    return results


def test_compile_result_format():
    """Test CompileResult string format như message_handler"""
    print("\n" + "=" * 80)
    print("TEST 4: CompileResult Format")
    print("=" * 80)
    
    code = """
a, b = map(int, input().split())
print(a + b)
"""
    
    testcases = [
        {
            "TestCaseId": "tc-001",
            "IndexNo": 0,
            "InputRef": "1 2",
            "OutputRef": "3"
        },      # Pass -> 0
        {
            "TestCaseId": "tc-002",
            "IndexNo": 1,
            "InputRef": "5 7",
            "OutputRef": "99"
        },     # Wrong -> 5
        {
            "TestCaseId": "tc-003",
            "IndexNo": 2,
            "InputRef": "10 20",
            "OutputRef": "30"
        },   # Pass -> 0
    ]
    
    results = execute_in_sandbox(
        language="python",
        code=code,
        testcases=testcases,
        timelimit=2,
        memorylimit=128
    )
    
    # Build CompileResult như message_handler
    TESTCASE_STATUS_CODE = {
        "Passed": "0",
        "TimeLimitExceeded": "1",
        "MemoryLimitExceeded": "2",
        "RuntimeError": "3",
        "InternalError": "4",
        "WrongAnswer": "5",
        "CompilationError": "6",
        "Skipped": "7"
    }
    
    compile_result = ""
    total_time = 0
    total_memory = 0
    
    for result in results:
        status = result.get("status", "InternalError")
        status_code = TESTCASE_STATUS_CODE.get(status, "4")
        compile_result += status_code
        total_time += result.get("time", 0)
        total_memory += result.get("memory", 0)
    
    print(f"CompileResult: {compile_result}")
    print(f"TotalTime: {total_time}ms")
    print(f"TotalMemory: {total_memory}KB")
    
    assert compile_result == "050", f"Expected '050', got '{compile_result}'"
    print("✅ CompileResult format correct!")
    
    
def test_index_ordering():
    """Test that results are ordered by IndexNo"""
    print("\n" + "=" * 80)
    print("TEST 5: IndexNo Ordering")
    print("=" * 80)
    
    code = """
a, b = map(int, input().split())
print(a + b)
"""
    
    # Testcases không theo thứ tự IndexNo
    testcases = [
        {
            "TestCaseId": "tc-003",
            "IndexNo": 2,
            "InputRef": "100 200",
            "OutputRef": "300"
        },
        {
            "TestCaseId": "tc-001",
            "IndexNo": 0,
            "InputRef": "1 2",
            "OutputRef": "3"
        },
        {
            "TestCaseId": "tc-002",
            "IndexNo": 1,
            "InputRef": "10 20",
            "OutputRef": "30"
        },
    ]
    
    results = execute_in_sandbox(
        language="python",
        code=code,
        testcases=testcases,
        timelimit=2,
        memorylimit=128
    )
    
    print(json.dumps(results, indent=2))
    
    # Verify results are sorted by IndexNo
    for i, result in enumerate(results):
        assert result["indexNo"] == i, f"Expected indexNo {i}, got {result['indexNo']}"
        print(f"✓ Result {i}: IndexNo={result['indexNo']}, TestCaseId={result['testcaseId']}")
    
    print("✅ Results correctly sorted by IndexNo!")


def test_python_syntax_error():
    """Test Python code với lỗi syntax"""
    print("\n" + "=" * 80)
    print("TEST 6: Python Syntax Error Detection")
    print("=" * 80)
    
    code = """
a = int(input())
print(a * 2  # ❌ Thiếu dấu đóng ngoặc
"""
    
    testcases = [
        {
            "TestCaseId": "tc-001",
            "IndexNo": 0,
            "InputRef": "5",
            "OutputRef": "10"
        },
        {
            "TestCaseId": "tc-002",
            "IndexNo": 1,
            "InputRef": "10",
            "OutputRef": "20"
        },
    ]
    
    results = execute_in_sandbox(
        language="python",
        code=code,
        testcases=testcases,
        timelimit=2,
        memorylimit=128
    )
    
    print(json.dumps(results, indent=2))
    
    # Verify ALL testcases have CompilationError
    for result in results:
        assert result["status"] == TESTCASE_STATUS.CompilationError, f"Expected CompilationError, got {result['status']}"
        assert "Syntax Error" in result["error"] or "SyntaxError" in result["error"]
        print(f"✓ Testcase #{result['indexNo']}: CompilationError detected")
    
    print("✅ Python syntax error detected correctly!")


def test_python_runtime_error_division_by_zero():
    """Test Python code với lỗi division by zero"""
    print("\n" + "=" * 80)
    print("TEST 7: Python Runtime Error - Division by Zero")
    print("=" * 80)
    
    code = """
a = int(input())
result = 10 / a  # ❌ Division by zero nếu input = 0
print(result)
"""
    
    testcases = [
        {
            "TestCaseId": "tc-001",
            "IndexNo": 0,
            "InputRef": "0",
            "OutputRef": "error"
        },
    ]
    
    results = execute_in_sandbox(
        language="python",
        code=code,
        testcases=testcases,
        timelimit=2,
        memorylimit=128
    )
    
    print(json.dumps(results, indent=2))
    
    # Verify RuntimeError
    assert results[0]["status"] == TESTCASE_STATUS.RuntimeError
    assert "ZeroDivisionError" in results[0]["error"] or "division by zero" in results[0]["error"].lower()
    print(f"✓ Error message: {results[0]['error'][:200]}...")
    print("✅ Python runtime error (division by zero) detected correctly!")


def test_python_runtime_error_index_out_of_bounds():
    """Test Python code với lỗi index out of bounds"""
    print("\n" + "=" * 80)
    print("TEST 8: Python Runtime Error - Index Out of Bounds")
    print("=" * 80)
    
    code = """
arr = [1, 2, 3]
index = int(input())
print(arr[index])  # ❌ IndexError nếu index >= 3
"""
    
    testcases = [
        {
            "TestCaseId": "tc-001",
            "IndexNo": 0,
            "InputRef": "5",
            "OutputRef": "error"
        },
    ]
    
    results = execute_in_sandbox(
        language="python",
        code=code,
        testcases=testcases,
        timelimit=2,
        memorylimit=128
    )
    
    print(json.dumps(results, indent=2))
    
    # Verify RuntimeError
    assert results[0]["status"] == TESTCASE_STATUS.RuntimeError
    assert "IndexError" in results[0]["error"] or "out of range" in results[0]["error"].lower()
    print(f"✓ Error message: {results[0]['error'][:200]}...")
    print("✅ Python runtime error (index out of bounds) detected correctly!")


def test_python_type_error():
    """Test Python code với lỗi type mismatch"""
    print("\n" + "=" * 80)
    print("TEST 9: Python Runtime Error - Type Error")
    print("=" * 80)
    
    code = """
a = input()  # String input
result = a + 5  # ❌ TypeError: can't add string and int
print(result)
"""
    
    testcases = [
        {
            "TestCaseId": "tc-001",
            "IndexNo": 0,
            "InputRef": "hello",
            "OutputRef": "error"
        },
    ]
    
    results = execute_in_sandbox(
        language="python",
        code=code,
        testcases=testcases,
        timelimit=2,
        memorylimit=128
    )
    
    print(json.dumps(results, indent=2))
    
    # Verify RuntimeError
    assert results[0]["status"] == TESTCASE_STATUS.RuntimeError
    assert "TypeError" in results[0]["error"]
    print(f"✓ Error message: {results[0]['error'][:200]}...")
    print("✅ Python type error detected correctly!")


def test_cpp_compilation_error():
    """Test C++ code với lỗi compilation"""
    print("\n" + "=" * 80)
    print("TEST 10: C++ Compilation Error Detection")
    print("=" * 80)
    
    code = """
#include <iostream>
using namespace std;

int main() {
    int a, b;
    cin >> a >> b;
    cout << a + b << endl  // ❌ Thiếu dấu chấm phẩy
    return 0;
}
"""
    
    testcases = [
        {
            "TestCaseId": "tc-001",
            "IndexNo": 0,
            "InputRef": "5 3",
            "OutputRef": "8"
        },
        {
            "TestCaseId": "tc-002",
            "IndexNo": 1,
            "InputRef": "10 20",
            "OutputRef": "30"
        },
    ]
    
    results = execute_in_sandbox(
        language="cpp",
        code=code,
        testcases=testcases,
        timelimit=2,
        memorylimit=128
    )
    
    print(json.dumps(results, indent=2))
    
    # Verify ALL testcases have CompilationError
    for result in results:
        assert result["status"] == TESTCASE_STATUS.CompilationError
        assert "error" in result["error"].lower() or "Compilation Error" in result["error"]
        print(f"✓ Testcase #{result['indexNo']}: CompilationError detected")
    
    print("✅ C++ compilation error detected correctly!")


def test_cpp_runtime_error():
    """Test C++ code với runtime error (segmentation fault)"""
    print("\n" + "=" * 80)
    print("TEST 11: C++ Runtime Error - Segmentation Fault")
    print("=" * 80)
    
    code = """
#include <iostream>
using namespace std;

int main() {
    int* ptr = nullptr;
    cout << *ptr << endl;  // ❌ Segmentation fault
    return 0;
}
"""
    
    testcases = [
        {
            "TestCaseId": "tc-001",
            "IndexNo": 0,
            "InputRef": "",
            "OutputRef": "error"
        },
    ]
    
    results = execute_in_sandbox(
        language="cpp",
        code=code,
        testcases=testcases,
        timelimit=2,
        memorylimit=128
    )
    
    print(json.dumps(results, indent=2))
    
    # Verify RuntimeError
    assert results[0]["status"] == TESTCASE_STATUS.RuntimeError
    print(f"✓ Error message: {results[0]['error'][:200]}...")
    print("✅ C++ runtime error detected correctly!")


def test_mixed_results():
    """Test submission với kết quả hỗn hợp (pass, fail, error)"""
    print("\n" + "=" * 80)
    print("TEST 12: Mixed Results (Pass + Wrong Answer + Runtime Error)")
    print("=" * 80)
    
    code = """
a = int(input())
if a == 0:
    raise ValueError("Cannot be zero")
elif a < 0:
    print("wrong answer")
else:
    print(a * 2)
"""
    
    testcases = [
        {
            "TestCaseId": "tc-001",
            "IndexNo": 0,
            "InputRef": "5",
            "OutputRef": "10"
        },  # Pass
        {
            "TestCaseId": "tc-002",
            "IndexNo": 1,
            "InputRef": "-3",
            "OutputRef": "-6"
        },  # Wrong Answer
        {
            "TestCaseId": "tc-003",
            "IndexNo": 2,
            "InputRef": "0",
            "OutputRef": "0"
        },  # Runtime Error
    ]
    
    results = execute_in_sandbox(
        language="python",
        code=code,
        testcases=testcases,
        timelimit=2,
        memorylimit=128
    )
    
    print(json.dumps(results, indent=2))
    
    # Verify mixed results
    assert results[0]["status"] == TESTCASE_STATUS.Passed
    assert results[1]["status"] == TESTCASE_STATUS.WrongAnswer
    assert results[2]["status"] == TESTCASE_STATUS.RuntimeError
    
    print(f"✓ TC#0: {results[0]['status']}")
    print(f"✓ TC#1: {results[1]['status']}")
    print(f"✓ TC#2: {results[2]['status']}")
    print("✅ Mixed results handled correctly!")


if __name__ == "__main__":
    try:
        print("=" * 80)
        print("🚀 COMPREHENSIVE ERROR HANDLING TEST SUITE")
        print("=" * 80)
        
        # Basic tests
        test_python_simple()
        test_python_wrong_answer()
        test_cpp_simple()
        test_compile_result_format()
        test_index_ordering()
        
        # Error handling tests
        test_python_syntax_error()
        test_python_runtime_error_division_by_zero()
        test_python_runtime_error_index_out_of_bounds()
        test_python_type_error()
        test_cpp_compilation_error()
        test_cpp_runtime_error()
        test_mixed_results()
        
        print("\n" + "=" * 80)
        print("🎉 ALL 12 TESTS PASSED!")
        print("=" * 80)
        print("\n✅ Summary:")
        print("  - Basic execution: ✓")
        print("  - Syntax errors: ✓")
        print("  - Runtime errors: ✓")
        print("  - Type errors: ✓")
        print("  - Compilation errors: ✓")
        print("  - Mixed results: ✓")
        print("  - IndexNo ordering: ✓")
    except Exception as e:
        print(f"\n❌ TEST FAILED: {e}")
        import traceback
        traceback.print_exc()
