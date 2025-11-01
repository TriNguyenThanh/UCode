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
    mem_keys: optional list of meta keys to try in order for memory measurement (e.g. ["cg-mem","max-rss"])
    Returns list of results in the exact format you want.
    """
    # print(json.dumps(testcases, indent=4))
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
            run_cmd = ["/usr/bin/python3", "main.py"]
            compile_success = True  # Python không cần compile

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
                return _error_result(testcases, error_code=TESTCASE_STATUS.InternalError, error_msg=f"Failed to write C++ source: {e}")

            # Compile
            print(f"[DEBUG] Compiling C++ code...")
            compile_cmd = [
                "isolate", "--box-id", str(box_id),
                "--time=10", "--wall-time=15", "--mem=512000", "--processes", "--full-env",
                "--run", "--",
                "/usr/bin/g++", "-std=c++17", "-O2", "-o", "main", "main.cpp", ">", compile_error_file
            ]
            compile_result = subprocess.run(compile_cmd, capture_output=True, text=True, timeout=20)

            if compile_result.returncode != 0:
                stderr = _read_file(compile_error_file)
                return _error_result(testcases, error_code=TESTCASE_STATUS.CompilationError, error_msg=stderr)

            run_cmd = ["./main"]
            compile_success = True
            print(f"[DEBUG] Compilation successful")

        else:
            return _error_result(testcases, error_code=TESTCASE_STATUS.InternalError, error_msg=f"Unsupported language: {language}")

        # Nếu compile thất bại → tất cả testcase đều lỗi compile
        if not compile_success:
            return results  # đã xử lý ở trên

        # === BƯỚC 2: Chạy từng testcase ===
        for tc in testcases:
            # print(json.dumps(tc, indent=4))

            tc_id = tc.get("testcaseId", "unknown")
            input_ref = str(tc.get("inputRef", "")).strip()
            output_ref = str(tc.get("outputRef", "")).strip()

            input_file = f"{box_path}/input.txt"
            output_file = f"{box_path}/output.txt"
            error_file = f"{box_path}/error.txt"

            # Tạo file input
            with open(input_file, "w", encoding="utf-8") as f:
                f.write(input_ref)

            result = {
                "testcaseId": tc_id,
                "actualOutput": "",
                "status": TESTCASE_STATUS.Pending,
                "errorMessage": "",
                "executionTimeMs": 0,
                "memoryUsageKb": 0
            }

            try:
                # print(f"[DEBUG] Running testcase {tc_id}...")

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

                # Đọc meta
                meta = _read_meta(meta_file)
                # đọc error.txt nếu có
                err = _read_file(error_file)
                print(f"[DEBUG] Error output: {err}")
                # Lấy thông tin thời gian và bộ nhớ sử dụng
                result["executionTimeMs"] = int(float(meta.get("time", exec_time_ms / 1000)) * 1000)
                # new: try multiple keys and parse units to KB
                result["memoryUsageKb"] = _get_memory_kb_from_meta(meta, mem_keys) or int(meta.get("cg-mem") or meta.get("max-rss") or 0) or int(memorylimit * 1024)
    
                # Kiểm tra trạng thái isolate
                status = meta.get("status", "")
                if status == "TO":
                    result["status"] = TESTCASE_STATUS.TimeLimitExceeded
                    result["errorMessage"] = "Time limit exceeded"
                    results.append(result)
                    print(f"[DEBUG] Testcase {tc_id} TLE\n")
                    continue
                elif status in ("RE", "SG"):
                    result["status"] = TESTCASE_STATUS.RuntimeError
                    result["errorMessage"] = err or meta.get("message", "Runtime error")
                    results.append(result)
                    print(f"[DEBUG] Testcase {tc_id} RE\n")
                    continue
                elif status == "XX":
                    result["status"] = TESTCASE_STATUS.InternalError
                    result["errorMessage"] = err or "Sandbox internal error"
                    results.append(result)
                    print(f"[DEBUG] Testcase {tc_id} Internal Error\n")
                    continue

                # Nếu process trả về non-zero nhưng meta không chỉ rõ -> coi là runtime error và trả nội dung error.txt nếu có
                if exec_result.returncode != 0 and not status:
                    result["status"] = TESTCASE_STATUS.RuntimeError
                    result["errorMessage"] = err or exec_result.stderr or "Process exited with non-zero code"
                    results.append(result)
                    print(f"[DEBUG] Testcase {tc_id} process exit !=0\n")
                    continue

                # Đọc output
                actual_output = _read_file(output_file).strip()
                result["actualOutput"] = actual_output
                print(f"Your output: {actual_output}")
                # So sánh với outputRef
                expected = output_ref.strip()
                print(f"Expected output: {expected}")
                if actual_output == expected:
                    result["status"] = TESTCASE_STATUS.Passed
                else:
                    result["status"] = TESTCASE_STATUS.WrongAnswer
                    result["errorMessage"] = f"Expected:\n{expected}\n\nGot:\n{actual_output}"

                results.append(result)

            except subprocess.TimeoutExpired:
                err = _read_file(error_file)
                result["status"] = TESTCASE_STATUS.TimeLimitExceeded
                result["errorMessage"] = "Execution timeout (failsafe)" + (f"\n{err}" if err else "")
                result["executionTimeMs"] = timelimit * 1000
                results.append(result)
            except Exception as e:
                err = _read_file(error_file)
                result["status"] = TESTCASE_STATUS.InternalError
                result["errorMessage"] = f"Unexpected error: {e}" + (f"\n{err}" if err else "")
                results.append(result)

        return results

    except Exception as e:
        print(f"[ERROR] Critical error: {e}")
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
    if error_code == TESTCASE_STATUS.CompilationError:
        msg = f"Compilation failed:\n{error_msg}"
    elif error_code == TESTCASE_STATUS.InternalError:
        msg = error_msg

    return [
        {
            "testcaseId": tc["testcaseId"],
            "actualOutput": "",
            "status": error_code,
            "errorMessage": msg,
            "executionTimeMs": 0,
            "memoryUsageKb": 0
        }
        for tc in testcases
    ]