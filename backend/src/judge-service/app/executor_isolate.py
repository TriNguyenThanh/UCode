"""
Executor sử dụng Isolate sandbox cho code execution.
Isolate là sandbox chuyên dụng cho competitive programming, được sử dụng bởi:
- IOI (International Olympiad in Informatics)
- Codeforces
- Judge0
"""

import subprocess
import os
import time
import uuid
import json
import base64
import py_compile
import tempfile

# Đọc default limits từ environment variables
DEFAULT_MEMORY_LIMIT = int(os.getenv("DEFAULT_MEMORY_LIMIT", "256"))
DEFAULT_TIME_LIMIT = int(os.getenv("DEFAULT_TIME_LIMIT", "3"))

class TESTCASE_STATUS:
    Pending = "Pending"
    Passed = "Passed"
    WrongAnswer = "WrongAnswer"
    TimeLimitExceeded = "TimeLimitExceeded"
    MemoryLimitExceeded = "MemoryLimitExceeded"
    RuntimeError = "RuntimeError"
    InternalError = "InternalError"
    CompilationError = "CompilationError"

def execute_in_sandbox(language, code, testcases, timelimit=None, memorylimit=None, mem_keys=None):
    """
    Execute code against multiple testcases using Isolate sandbox.
    
    Args:
        language: Ngôn ngữ lập trình (python, cpp)
        code: Source code
        testcases: List of testcase dicts theo format C#:
            [
                {
                    "TestCaseId": "guid-string",
                    "IndexNo": 0,
                    "InputRef": "1 2\n",
                    "OutputRef": "3\n"
                },
                ...
            ]
        timelimit: Time limit in seconds
        memorylimit: Memory limit in MB
        mem_keys: Optional list of meta keys for memory measurement
        
    Returns:
        List of results sorted by IndexNo: [
            {
                "testcaseId": str,
                "indexNo": int,
                "status": str,  # TESTCASE_STATUS constant
                "time": int,    # execution time in milliseconds
                "memory": int,  # memory usage in KB
                "output": str,  # actual output
                "error": str    # error message if any
            }
        ]
    """
    if timelimit is None:
        timelimit = DEFAULT_TIME_LIMIT
    if memorylimit is None:
        memorylimit = DEFAULT_MEMORY_LIMIT

    if mem_keys is None:
        mem_keys = ["cg-mem", "max-rss", "measured", "memory", "mem", "rss"]

    # Kết quả cuối cùng
    results = []

    # Tạo box ID duy nhất
    box_id = int(uuid.uuid4().hex[:3], 16) % 1000
    box_path = f"/var/local/lib/isolate/{box_id}/box"
    meta_file = f"{box_path}/meta.txt"

    compile_error_file = f"{box_path}/compile_err.txt"
    # Biến lưu trạng thái compile (chỉ compile 1 lần)
    compile_success = False
    run_cmd = None

    try:
        print(f"[DEBUG] Initializing isolate box {box_id}")

        # Cleanup cũ & khởi tạo box
        subprocess.run(["isolate", "--box-id", str(box_id), "--cleanup"], check=False, timeout=5, capture_output=True)
        subprocess.run(["isolate", "--box-id", str(box_id), "--init"], check=True, timeout=5, capture_output=True)

        # === BƯỚC 1: Chuẩn bị code & Compile (chỉ 1 lần) ===
        if language == "python":
            code_file = f"{box_path}/main.py"
            with open(code_file, "w", encoding="utf-8") as f:
                f.write(code)
            
            # ✅ CHECK SYNTAX trước khi chạy testcases
            try:
                # Compile để check syntax, sau đó XÓA file .pyc để tránh cache
                compiled_file = py_compile.compile(code_file, doraise=True)
                # Xóa file bytecode để tránh cache giữa các lần chạy
                if compiled_file and os.path.exists(compiled_file):
                    os.remove(compiled_file)
                # Xóa thư mục __pycache__ nếu có
                pycache_dir = f"{box_path}/__pycache__"
                if os.path.exists(pycache_dir):
                    import shutil
                    shutil.rmtree(pycache_dir)
                print(f"[DEBUG] Python syntax check passed")
            except py_compile.PyCompileError as e:
                # Parse error message để lấy thông tin chi tiết
                error_msg = str(e)
                print(f"[ERROR] Python syntax error: {error_msg}")
                # ⚠️ CLEANUP trước khi return!
                subprocess.run(["isolate", "--box-id", str(box_id), "--cleanup"], check=False, timeout=5, capture_output=True)
                return _error_result(
                    testcases, 
                    error_code=TESTCASE_STATUS.CompilationError, 
                    error_msg=f"Python Syntax Error:\n{error_msg}"
                )
            except SyntaxError as e:
                # Direct syntax error
                error_msg = f"Line {e.lineno}: {e.msg}\n{e.text}" if e.text else f"Line {e.lineno}: {e.msg}"
                print(f"[ERROR] Python syntax error: {error_msg}")
                # ⚠️ CLEANUP trước khi return!
                subprocess.run(["isolate", "--box-id", str(box_id), "--cleanup"], check=False, timeout=5, capture_output=True)
                return _error_result(
                    testcases,
                    error_code=TESTCASE_STATUS.CompilationError,
                    error_msg=f"Python Syntax Error:\n{error_msg}"
                )
            
            run_cmd = ["/usr/bin/python3", "main.py"]
            compile_success = True
            print(f"[DEBUG] Python code ready for execution")

        elif language == "cpp":
            code_file = f"{box_path}/main.cpp"
            # Xử lý code (base64 hoặc plain)
            try:
                if isinstance(code, (bytes, bytearray)):
                    raw_bytes = bytes(code)
                else:
                    try:
                        raw_bytes = base64.b64decode(code, validate=True)
                    except:
                        raw_bytes = code.encode("utf-8")
                with open(code_file, "w") as f:
                    f.write(code)
            except Exception as e:
                # ⚠️ CLEANUP trước khi return!
                subprocess.run(["isolate", "--box-id", str(box_id), "--cleanup"], check=False, timeout=5, capture_output=True)
                return _error_result(testcases, error_code=TESTCASE_STATUS.InternalError, error_msg=f"Failed to write C++ source: {e}")

            # Compile
            print(f"[DEBUG] Compiling C++ code...")
            compile_cmd = [
                "isolate", "--box-id", str(box_id),
                "--time=10", "--wall-time=15", "--mem=512000", "--processes", "--full-env",
                "--stderr=compile_err.txt",
                "--run", "--",
                "/usr/bin/g++", "-std=c++17", "-O2", "-o", "main", "main.cpp"
            ]
            compile_result = subprocess.run(compile_cmd, capture_output=True, text=True, timeout=20)

            if compile_result.returncode != 0:
                stderr = _read_file(compile_error_file) or compile_result.stderr
                print(f"[ERROR] C++ compilation failed:\n{stderr}")
                # ⚠️ CLEANUP trước khi return!
                subprocess.run(["isolate", "--box-id", str(box_id), "--cleanup"], check=False, timeout=5, capture_output=True)
                return _error_result(
                    testcases, 
                    error_code=TESTCASE_STATUS.CompilationError, 
                    error_msg=f"C++ Compilation Error:\n{stderr}"
                )

            run_cmd = ["./main"]
            compile_success = True
            print(f"[DEBUG] Compilation successful")

        else:
            # ⚠️ CLEANUP trước khi return!
            subprocess.run(["isolate", "--box-id", str(box_id), "--cleanup"], check=False, timeout=5, capture_output=True)
            return _error_result(testcases, error_code=TESTCASE_STATUS.InternalError, error_msg=f"Unsupported language: {language}")

        # Nếu compile thất bại → tất cả testcase đều lỗi compile
        if not compile_success:
            return results  # đã xử lý ở trên

        # === BƯỚC 2: Chạy từng testcase (sắp xếp theo IndexNo) ===
        # Sort testcases by IndexNo để đảm bảo thứ tự đúng
        sorted_testcases = sorted(testcases, key=lambda tc: tc.get("IndexNo", 0))
        
        for tc in sorted_testcases:
            # Hỗ trợ cả lowercase (từ Python) và PascalCase (từ C#)
            tc_id = tc.get("TestCaseId") or tc.get("testcaseId", "unknown")
            index_no = tc.get("IndexNo", tc.get("indexNo", 0))
            input_ref = str(tc.get("InputRef") or tc.get("inputRef", "")).strip()
            output_ref = str(tc.get("OutputRef") or tc.get("outputRef", "")).strip()

            input_file = f"{box_path}/input.txt"
            output_file = f"{box_path}/output.txt"
            error_file = f"{box_path}/error.txt"

            # ⚠️ XÓA các file cũ từ testcase trước để tránh cache/contamination
            for cleanup_file in [input_file, output_file, error_file, meta_file]:
                if os.path.exists(cleanup_file):
                    os.remove(cleanup_file)

            # Tạo file input
            with open(input_file, "w", encoding="utf-8") as f:
                f.write(input_ref)

            # Result format với IndexNo
            result = {
                "testcaseId": tc_id,
                "indexNo": index_no,
                "status": TESTCASE_STATUS.Pending,
                "time": 0,      # milliseconds
                "memory": 0,    # KB
                "output": "",   # actual output
                "error": ""     # error message
            }

            try:
                # Chạy isolate với file I/O
                isolate_cmd = [
                    "isolate", "--box-id", str(box_id),
                    "--stdin=input.txt", "--stdout=output.txt", "--stderr=error.txt",
                    f"--time={timelimit}", f"--wall-time={timelimit + 2}",
                    f"--mem={memorylimit * 1024}", "--processes",
                    "--meta", meta_file,
                    "--run", "--"
                ] + run_cmd

                start_time = time.time()
                exec_result = subprocess.run(isolate_cmd, capture_output=True, text=True, timeout=timelimit + 5)
                exec_time_ms = int((time.time() - start_time) * 1000)

                # Đọc meta và error
                meta = _read_meta(meta_file)
                err = _read_file(error_file)
                
                # Lấy thông tin thời gian và bộ nhớ
                result["time"] = int(float(meta.get("time", exec_time_ms / 1000)) * 1000)
                result["memory"] = _get_memory_kb_from_meta(meta, mem_keys) or int(meta.get("cg-mem") or meta.get("max-rss") or 0)
    
                # Kiểm tra trạng thái isolate
                status = meta.get("status", "")
                if status == "TO":
                    result["status"] = TESTCASE_STATUS.TimeLimitExceeded
                    result["error"] = f"Time limit exceeded ({timelimit}s)"
                    results.append(result)
                    print(f"[DEBUG] Testcase #{index_no} ({tc_id}) TLE")
                    continue
                elif status in ("RE", "SG"):
                    result["status"] = TESTCASE_STATUS.RuntimeError
                    # Format error message với stack trace nếu có
                    error_detail = err or meta.get("message", "Runtime error")
                    result["error"] = f"Runtime Error:\n{error_detail}"
                    results.append(result)
                    print(f"[DEBUG] Testcase #{index_no} ({tc_id}) RE: {error_detail[:100]}...")
                    continue
                elif status == "XX":
                    result["status"] = TESTCASE_STATUS.InternalError
                    result["error"] = f"Internal Error: {err or 'Sandbox internal error'}"
                    results.append(result)
                    print(f"[DEBUG] Testcase #{index_no} ({tc_id}) Internal Error")
                    continue

                # Nếu process trả về non-zero → runtime error
                if exec_result.returncode != 0 and not status:
                    result["status"] = TESTCASE_STATUS.RuntimeError
                    error_detail = err or exec_result.stderr or "Process exited with non-zero code"
                    result["error"] = f"Runtime Error (Exit Code {exec_result.returncode}):\n{error_detail}"
                    results.append(result)
                    print(f"[DEBUG] Testcase #{index_no} ({tc_id}) process exit != 0")
                    continue

                # Đọc output và so sánh
                actual_output = _read_file(output_file).strip()
                result["output"] = actual_output
                
                expected = output_ref.strip()
                if actual_output == expected:
                    result["status"] = TESTCASE_STATUS.Passed
                    print(f"[✓] Testcase #{index_no} ({tc_id}) passed")
                else:
                    result["status"] = TESTCASE_STATUS.WrongAnswer
                    result["error"] = f"Expected: {expected[:100]}... | Got: {actual_output[:100]}..."
                    print(f"[✗] Testcase #{index_no} ({tc_id}) wrong answer")

                results.append(result)

            except subprocess.TimeoutExpired:
                result["status"] = TESTCASE_STATUS.TimeLimitExceeded
                result["error"] = "Execution timeout (failsafe)"
                result["time"] = timelimit * 1000
                results.append(result)
            except Exception as e:
                result["status"] = TESTCASE_STATUS.InternalError
                result["error"] = f"Unexpected error: {e}"
                results.append(result)

        return results

    except Exception as e:
        print(f"[ERROR] Critical error: {e}")
        # ⚠️ CLEANUP ngay lập tức khi có critical error
        try:
            subprocess.run(["isolate", "--box-id", str(box_id), "--cleanup"], check=False, timeout=5, capture_output=True)
            print(f"[DEBUG] Emergency cleanup box {box_id} after critical error")
        except:
            pass
        return _error_result(testcases, error_code=TESTCASE_STATUS.InternalError, error_msg=f"Critical error: {e}")
    finally:
        # Cleanup
        try:
            subprocess.run(["isolate", "--box-id", str(box_id), "--cleanup"], check=False, timeout=5)
            print(f"[DEBUG] Cleaned up box {box_id}")
        except:
            pass


# === Helper Functions ===
def _read_meta(meta_path):
    meta = {}
    if os.path.exists(meta_path):
        with open(meta_path, "r") as f:
            for line in f:
                if ':' in line:
                    k, v = line.strip().split(':', 1)
                    meta[k] = v
    return meta

def _read_file(filepath):
    if os.path.exists(filepath):
        with open(filepath, "r", encoding="utf-8") as f:
            err = f.read().strip()
            # print(f"[DEBUG] Read file {filepath}: {err}")
            return err
    return ""

def _parse_memory_to_kb(val):
    """
    Parse a memory value (string/number) with optional units into KB (int).
    Accepts values like '12345', '12345K', '12MB', '12345B'.
    """
    if val is None:
        return 0
    s = str(val).strip()
    if s == "":
        return 0
    # remove surrounding quotes if any
    s = s.strip('"').strip("'")
    try:
        # unit suffixes
        low = s.lower()
        if low.endswith("kb"):
            return int(float(low[:-2]) * 1)
        if low.endswith("k"):
            return int(float(low[:-1]) * 1)
        if low.endswith("mb"):
            return int(float(low[:-2]) * 1024)
        if low.endswith("m"):
            return int(float(low[:-1]) * 1024)
        if low.endswith("b"):
            # bytes -> KB
            return int(float(low[:-1]) / 1024)
        # pure number: heuristics - treat as KB if reasonably small, else bytes
        v = float(low)
        if v > 1024 * 1024:  # > ~1GB -> probably bytes
            return int(v // 1024)
        if v > 1024 * 10:  # >10MB -> probably bytes
            return int(v // 1024)
        return int(v)
    except Exception:
        return 0

def _get_memory_kb_from_meta(meta, keys_order=None):
    """
    Try to extract memory usage from meta dict using keys_order list.
    Returns int KB or 0.
    """
    if not meta:
        return 0
    if keys_order is None:
        keys_order = ["cg-mem", "max-rss", "measured", "memory", "mem", "rss"]

    for k in keys_order:
        if k in meta and meta[k] is not None and meta[k] != "":
            kb = _parse_memory_to_kb(meta[k])
            if kb > 0:
                return kb
    # fallback: try any numeric value in meta
    for v in meta.values():
        kb = _parse_memory_to_kb(v)
        if kb > 0:
            return kb
    return 0

def _error_result(testcases, error_code, error_msg):
    """
    Tạo error result cho tất cả testcases khi có lỗi nghiêm trọng (compile error, internal error)
    """
    return [
        {
            "testcaseId": tc.get("TestCaseId") or tc.get("testcaseId", "unknown"),
            "indexNo": tc.get("IndexNo", tc.get("indexNo", 0)),
            "status": error_code,
            "time": 0,
            "memory": 0,
            "output": "",
            "error": error_msg
        }
        for tc in testcases
    ]