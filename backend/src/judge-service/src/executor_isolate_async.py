"""
Async Executor sử dụng Isolate sandbox cho code execution.
Cho phép chạy nhiều testcases song song để tăng performance.
"""

import asyncio
import subprocess
import os
import time
import uuid
import json
import base64
import py_compile
import tempfile
import sys
from concurrent.futures import ThreadPoolExecutor

# Đọc default limits từ environment variables
# DEFAULT_MEMORY_LIMIT: Memory limit in KB (default: 262144 KB = 256 MB)
# DEFAULT_TIME_LIMIT: Time limit in seconds (default: 3 seconds)
DEFAULT_MEMORY_LIMIT = int(os.getenv("DEFAULT_MEMORY_LIMIT", "262144"))
DEFAULT_TIME_LIMIT = int(os.getenv("DEFAULT_TIME_LIMIT", "3"))
MAX_PARALLEL_TESTCASES = int(os.getenv("MAX_PARALLEL_TESTCASES", "4"))

class TESTCASE_STATUS:
    Pending = "Pending"
    Passed = "Passed"
    WrongAnswer = "WrongAnswer"
    TimeLimitExceeded = "TimeLimitExceeded"
    MemoryLimitExceeded = "MemoryLimitExceeded"
    RuntimeError = "RuntimeError"
    InternalError = "InternalError"
    CompilationError = "CompilationError"
    Skipped = "Skipped"

# Thread pool for running subprocess commands
executor = ThreadPoolExecutor(max_workers=MAX_PARALLEL_TESTCASES * 2)

LOW_PRIORITY_NICE = int(os.getenv("ISOLATE_NICE", "10"))
ISOLATE_CPU_AFFINITY = os.getenv("ISOLATE_CPU_AFFINITY", "").strip()  # ví dụ: "1-7" hoặc "2,3,4"

def _parse_affinity(s: str):
    cpus = set()
    for part in s.split(","):
        part = part.strip()
        if not part:
            continue
        if "-" in part:
            a, b = part.split("-", 1)
            cpus.update(range(int(a), int(b) + 1))
        else:
            cpus.add(int(part))
    return sorted(cpus)

def _set_low_priority():
    try:
        # Hạ ưu tiên CPU cho process con (giá trị nice lớn hơn = kém ưu tiên hơn)
        os.nice(LOW_PRIORITY_NICE)
    except Exception:
        pass
    if ISOLATE_CPU_AFFINITY:
        try:
            cpus = _parse_affinity(ISOLATE_CPU_AFFINITY)
            if cpus:
                os.sched_setaffinity(0, cpus)
        except Exception:
            pass

def debug_log(msg):
    """Print debug message to stderr to avoid polluting stdout JSON output"""
    print(msg, file=sys.stderr, flush=True)

async def execute_in_sandbox(language, code, testcases, timelimit=None, memorylimit=None, mem_keys=None):
    """
    Execute code against multiple testcases using Isolate sandbox (ASYNC).
    Each testcase gets its own isolate box for parallel execution.
    
    Args:
        language: Ngôn ngữ lập trình (python, cpp)
        code: Source code
        testcases: List of testcase dicts theo format C#
        timelimit: Time limit in seconds
        memorylimit: Memory limit in KB (kilobytes)
        mem_keys: Optional list of meta keys for memory measurement
        
    Returns:
        List of results sorted by IndexNo
    """
    if timelimit is None:
        timelimit = DEFAULT_TIME_LIMIT
    if memorylimit is None:
        memorylimit = DEFAULT_MEMORY_LIMIT

    if mem_keys is None:
        mem_keys = ["cg-mem", "max-rss", "measured", "memory", "mem", "rss"]

    # Sort testcases by IndexNo
    sorted_testcases = sorted(testcases, key=lambda tc: tc.get("IndexNo", 0))
    
    #  COMPILE/SYNTAX CHECK CHỈ 1 LẦN cho tất cả testcases
    debug_log(f"[DEBUG] Compiling/checking code once for all {len(sorted_testcases)} testcases...")
    try:
        run_cmd = await _compile_code_once(language, code, timelimit, memorylimit)
    except ValueError as e:
        # Compilation/Syntax error - trả về lỗi cho tất cả testcases
        error_msg = str(e)
        debug_log(f"[ERROR] Compilation failed: {error_msg}")
        return _error_result(sorted_testcases, TESTCASE_STATUS.CompilationError, error_msg)
    
    debug_log(f"[DEBUG] Compilation successful, run command: {run_cmd}")
    
    #  BATCH EXECUTION: Trong batch chạy song song, giữa các batch chạy tuần tự
    # Early stopping: Nếu TẤT CẢ testcases trong batch đều TLE → dừng các batch sau
    debug_log(f"[DEBUG] Running {len(sorted_testcases)} testcases in batches")
    debug_log(f"[DEBUG] Batch size: {MAX_PARALLEL_TESTCASES} (parallel within batch, sequential between batches)")
    debug_log(f"[DEBUG] Early stopping: If ALL testcases in a batch are TLE → stop")
    
    results = []
    stop_execution = False
    
    try:
        # Chia testcases thành các batch
        num_batches = (len(sorted_testcases) + MAX_PARALLEL_TESTCASES - 1) // MAX_PARALLEL_TESTCASES
        
        for batch_idx in range(num_batches):
            # Lấy batch hiện tại
            start_idx = batch_idx * MAX_PARALLEL_TESTCASES
            end_idx = min(start_idx + MAX_PARALLEL_TESTCASES, len(sorted_testcases))
            batch = sorted_testcases[start_idx:end_idx]
            
            # Nếu đã early stop, skip batch này
            if stop_execution:
                for tc in batch:
                    debug_log(f"[PAUSE] Skipping testcase (IndexNo={tc.get('IndexNo')}) - Early stopped")
                    results.append({
                        "testcaseId": tc.get("TestCaseId") or tc.get("testcaseId", "unknown"),
                        "indexNo": tc.get("IndexNo", tc.get("indexNo", 0)),
                        "status": TESTCASE_STATUS.TimeLimitExceeded,
                        "time": 0,
                        "memory": 0,
                        "output": "",
                        "error": "Skipped due to early stopping (previous batch was all TLE)"
                    })
                continue  # Skip batch này, chuyển sang batch tiếp theo
            
            # Chạy batch hiện tại
            
            debug_log(f"\n[#] Batch {batch_idx + 1}/{num_batches}: Running {len(batch)} testcases in PARALLEL")
            debug_log(f"    Testcases: {start_idx + 1}-{end_idx}")
            
            # Chạy SONG SONG tất cả testcases trong batch này
            batch_tasks = []
            for tc in batch:
                task = _run_single_testcase_with_own_box(
                    tc, language, code, run_cmd,
                    timelimit, memorylimit, mem_keys
                )
                batch_tasks.append(task)
            
            batch_results = await asyncio.gather(*batch_tasks)
            
            # Thêm results vào list chính
            results.extend(batch_results)
            
            # Kiểm tra xem TẤT CẢ testcases trong batch có phải TLE không
            tle_count = sum(1 for r in batch_results if r.get("status") == TESTCASE_STATUS.TimeLimitExceeded)
            total_in_batch = len(batch_results)
            
            debug_log(f"[JUDGE] Batch {batch_idx + 1} results: {tle_count}/{total_in_batch} TLE")
            
            if tle_count == total_in_batch and total_in_batch > 0:
                # TẤT CẢ đều TLE → Early stop
                debug_log(f"[STOP] EARLY STOPPING: ALL {total_in_batch} testcases in batch {batch_idx + 1} are TLE!")
                remaining_batches = num_batches - batch_idx - 1
                if remaining_batches > 0:
                    remaining_testcases = len(sorted_testcases) - end_idx
                    debug_log(f"[STOP] Stopping {remaining_batches} remaining batches ({remaining_testcases} testcases)")
                    stop_execution = True
            else:
                # Có ít nhất 1 testcase không TLE → tiếp tục
                passed = sum(1 for r in batch_results if r.get("status") == TESTCASE_STATUS.Passed)
                other = total_in_batch - tle_count - passed
                debug_log(f"[RESULT] Batch {batch_idx + 1} has non-TLE results: {passed} Passed, {other} Other → Continue")
        
        return results
        
    except Exception as e:
        debug_log(f"[ERROR] Critical error in execute_in_sandbox: {e}")
        import traceback
        traceback.print_exc(file=sys.stderr)
        return _error_result(testcases, TESTCASE_STATUS.InternalError, f"Critical error: {e}")


async def _compile_code_once(language, code, timelimit, memorylimit):
    """
    Compile/check code CHỈ 1 LẦN cho tất cả testcases.
    Trả về run_cmd hoặc raise ValueError nếu lỗi.
    """
    # Tạo temporary box để compile
    temp_box_id = int(uuid.uuid4().hex[:3], 16) % 1000
    temp_box_path = f"/var/local/lib/isolate/{temp_box_id}/box"
    
    try:
        # Init temporary box
        await _run_command(["isolate", "--box-id", str(temp_box_id), "--cleanup"], timeout=5)
        await _run_command(["isolate", "--box-id", str(temp_box_id), "--init"], timeout=5)
        
        if language == "python":
            code_file = f"{temp_box_path}/main.py"
            
            # Write code to file
            loop = asyncio.get_event_loop()
            await loop.run_in_executor(None, _write_file, code_file, code)
            
            # Check syntax CHỈ 1 LẦN
            try:
                await loop.run_in_executor(None, _check_python_syntax, code_file, temp_box_path)
                debug_log(f"[✓] Python syntax check passed")
            except Exception as e:
                error_msg = str(e)
                raise ValueError(f"Python Syntax Error:\n{error_msg}")
            
            run_cmd = ["/usr/bin/python3", "main.py"]
            return run_cmd

        elif language == "cpp":
            code_file = f"{temp_box_path}/main.cpp"
            compile_stdout_file = f"{temp_box_path}/compile_out.txt"
            compile_stderr_file = f"{temp_box_path}/compile_err.txt"
            
            # Write code to file
            loop = asyncio.get_event_loop()
            await loop.run_in_executor(None, _write_file, code_file, code)
            
            # Compile CHỈ 1 LẦN - Capture BOTH stdout và stderr
            debug_log(f"[DEBUG] Compiling C++ code...")
            compile_cmd = [
                "isolate", "--box-id", str(temp_box_id),
                "--time=10", "--wall-time=15", "--mem=512000", "--processes", "--full-env",
                "--stdout=compile_out.txt",  #  Capture stdout
                "--stderr=compile_err.txt",  #  Capture stderr
                "--run", "--",
                "/usr/bin/g++", "-std=c++17", "-O2", "-Wall", "-Wextra",  #  Thêm -Wall -Wextra để có nhiều warning
                "-o", "main", "main.cpp"
            ]
            
            compile_result = await _run_command(compile_cmd, timeout=20, capture_output=True)
            
            if compile_result.returncode != 0:
                #  ĐỌC ĐẦY ĐỦ cả stdout và stderr từ file
                stdout_content = await loop.run_in_executor(None, _read_file, compile_stdout_file)
                stderr_content = await loop.run_in_executor(None, _read_file, compile_stderr_file)
                
                #  Kết hợp cả 2 outputs (một số compiler output vào stdout)
                full_error = ""
                if stderr_content:
                    full_error += f"STDERR:\n{stderr_content}\n"
                if stdout_content:
                    full_error += f"STDOUT:\n{stdout_content}\n"
                
                #  Fallback: Nếu file rỗng, lấy từ process
                if not full_error.strip():
                    try:
                        proc_stdout = compile_result.stdout.decode('utf-8', errors='replace') if compile_result.stdout else ""
                        proc_stderr = compile_result.stderr.decode('utf-8', errors='replace') if compile_result.stderr else ""
                        if proc_stderr:
                            full_error += f"Process STDERR:\n{proc_stderr}\n"
                        if proc_stdout:
                            full_error += f"Process STDOUT:\n{proc_stdout}\n"
                    except Exception as decode_err:
                        full_error += f"\n[Error decoding compiler output: {decode_err}]"
                
                #  Nếu vẫn không có gì, thông báo generic
                if not full_error.strip():
                    full_error = f"Compilation failed with exit code {compile_result.returncode}\nNo error message available."
                
                debug_log(f"[ERROR] C++ Compilation Error:\n{full_error}")
                raise ValueError(f"C++ Compilation Error:\n{full_error}")

            debug_log(f"[RESULT] C++ compilation successful")
            run_cmd = ["./main"]
            return run_cmd

        else:
            raise ValueError(f"Unsupported language: {language}")
            
    finally:
        # Cleanup temporary box
        try:
            await _run_command(["isolate", "--box-id", str(temp_box_id), "--cleanup"], timeout=5)
        except Exception as cleanup_err:
            debug_log(f"[WARNING] Failed to cleanup temp box {temp_box_id}: {cleanup_err}")


async def _run_single_testcase_with_own_box(tc, language, code, run_cmd, timelimit, memorylimit, mem_keys):
    """
    Chạy một testcase với isolate box riêng biệt.
    Mỗi testcase có box độc lập để tránh xung đột khi chạy song song.
    """
    # Extract testcase info
    tc_id = tc.get("TestCaseId") or tc.get("testcaseId", "unknown")
    index_no = tc.get("IndexNo", tc.get("indexNo", 0))
    input_ref = str(tc.get("InputRef") or tc.get("inputRef", "")).strip()
    output_ref = str(tc.get("OutputRef") or tc.get("outputRef", "")).strip()

    # Tạo box ID duy nhất cho testcase này
    box_id = int(uuid.uuid4().hex[:3], 16) % 1000
    box_path = f"/var/local/lib/isolate/{box_id}/box"
    
    # File paths
    input_file = f"{box_path}/input.txt"
    output_file = f"{box_path}/output.txt"
    error_file = f"{box_path}/error.txt"
    meta_file = f"{box_path}/meta.txt"
    compile_error_file = f"{box_path}/compile_err.txt"

    # Result template
    result = {
        "testcaseId": tc_id,
        "indexNo": index_no,
        "status": TESTCASE_STATUS.Pending,
        "time": 0,
        "memory": 0,
        "output": "",
        "error": ""
    }

    try:
        # Cleanup & init box
        await _run_command(["isolate", "--box-id", str(box_id), "--cleanup"], timeout=5)
        await _run_command(["isolate", "--box-id", str(box_id), "--init"], timeout=5)
        
        # CHỈ COPY CODE, KHÔNG COMPILE LẠI (đã compile ở _compile_code_once)
        loop = asyncio.get_event_loop()
        
        if language == "python":
            code_file = f"{box_path}/main.py"
            await loop.run_in_executor(None, _write_file, code_file, code)
        elif language == "cpp":
            # C++ đã compile kiểm tra syntax rồi, nhưng mỗi box cần binary riêng
            # (Isolate không share files giữa các boxes)
            code_file = f"{box_path}/main.cpp"
            compile_stdout_file = f"{box_path}/compile_out.txt"  #  Thêm stdout file
            compile_stderr_file = f"{box_path}/compile_err.txt"
            await loop.run_in_executor(None, _write_file, code_file, code)
            
            # Re-compile trong box này
            compile_cmd = [
                "isolate", "--box-id", str(box_id),
                "--time=10", "--wall-time=15", "--mem=512000", "--processes", "--full-env",
                "--stdout=compile_out.txt",  #  Capture stdout
                "--stderr=compile_err.txt",  #  Capture stderr
                "--run", "--",
                "/usr/bin/g++", "-std=c++17", "-O2", "-Wall", "-Wextra",
                "-o", "main", "main.cpp"
            ]
            compile_result = await _run_command(compile_cmd, timeout=20, capture_output=True)
            
            if compile_result.returncode != 0:
                #  ĐỌC ĐẦY ĐỦ compile error
                stdout_content = await loop.run_in_executor(None, _read_file, compile_stdout_file)
                stderr_content = await loop.run_in_executor(None, _read_file, compile_stderr_file)
                
                full_error = ""
                if stderr_content:
                    full_error += stderr_content
                if stdout_content:
                    full_error += f"\n{stdout_content}"
                
                if not full_error.strip():
                    full_error = f"Compilation failed with exit code {compile_result.returncode}"
                
                result["status"] = TESTCASE_STATUS.CompilationError
                result["error"] = f"Compilation Error:\n{full_error}"
                debug_log(f"[ERROR] Box {box_id} compilation error:\n{full_error}")
                return result

        # Write input file
        await loop.run_in_executor(None, _write_file, input_file, input_ref)

        # Run isolate
        isolate_cmd = [
            "isolate", "--box-id", str(box_id),
            f"--stdin=input.txt", 
            f"--stdout=output.txt", 
            f"--stderr=error.txt",
            f"--time={timelimit}", f"--wall-time={timelimit + 2}",
            f"--mem={memorylimit}", "--processes",  # memorylimit đã là KB, dùng trực tiếp
            "--meta", meta_file,
            "--run", "--"
        ] + run_cmd

        start_time = time.time()
        exec_result = await _run_command(isolate_cmd, timeout=timelimit + 5, capture_output=True)
        exec_time_ms = int((time.time() - start_time) * 1000)

        # Read meta and error
        meta = await loop.run_in_executor(None, _read_meta, meta_file)
        err = await loop.run_in_executor(None, _read_file, error_file)
        
        # Get time and memory
        result["time"] = int(float(meta.get("time", exec_time_ms / 1000)) * 1000)
        result["memory"] = _get_memory_kb_from_meta(meta, mem_keys) or int(meta.get("cg-mem", "0") or meta.get("max-rss", "0") or 0)

        # Check isolate status
        status = meta.get("status", "")
        if status == "TO":
            result["status"] = TESTCASE_STATUS.TimeLimitExceeded
            result["error"] = f"Time limit exceeded ({timelimit}s)"
            return result
        elif status in ("RE", "SG"):
            result["status"] = TESTCASE_STATUS.RuntimeError
            error_detail = err or meta.get("message", "Runtime error")
            result["error"] = f"Runtime Error:\n{error_detail}"
            return result
        elif status == "XX":
            result["status"] = TESTCASE_STATUS.InternalError
            result["error"] = f"Internal Error: {err or 'Sandbox internal error'}"
            return result

        # Check process return code
        if exec_result.returncode != 0 and not status:
            result["status"] = TESTCASE_STATUS.RuntimeError
            stderr_content = ""
            try:
                stderr_content = exec_result.stderr.decode() if exec_result.stderr else ""
            except Exception:
                pass
            error_detail = err or stderr_content or "Process exited with non-zero code"
            result["error"] = f"Runtime Error (Exit Code {exec_result.returncode}):\n{error_detail}"
            return result

        # Read output and compare
        actual_output = await loop.run_in_executor(None, _read_file, output_file)
        actual_output = actual_output.strip()
        result["output"] = actual_output
        
        expected = output_ref.strip()
        if actual_output == expected:
            result["status"] = TESTCASE_STATUS.Passed
            debug_log(f"[RESULT] Testcase #{index_no} ({tc_id}) passed (box {box_id})")
        else:
            result["status"] = TESTCASE_STATUS.WrongAnswer
            result["error"] = f"Expected: {expected[:100]}... | Got: {actual_output[:100]}..."
            debug_log(f"[RESULT] Testcase #{index_no} ({tc_id}) wrong answer (box {box_id})")
        return result

    except asyncio.TimeoutError:
        result["status"] = TESTCASE_STATUS.TimeLimitExceeded
        result["error"] = "Execution timeout (failsafe)"
        result["time"] = timelimit * 1000
        return result
    except Exception as e:
        result["status"] = TESTCASE_STATUS.InternalError
        result["error"] = f"Unexpected error: {e}"
        debug_log(f"[ERROR] Testcase #{index_no} ({tc_id}) box {box_id} error: {e}")
        import traceback
        traceback.print_exc(file=sys.stderr)
        return result
    finally:
        # Cleanup box
        try:
            await _run_command(["isolate", "--box-id", str(box_id), "--cleanup"], timeout=5)
        except Exception as cleanup_err:
            debug_log(f"[WARNING] Failed to cleanup box {box_id}: {cleanup_err}")


# ============================================================================
# HELPER FUNCTIONS
# ============================================================================

async def _run_command(cmd, timeout=None, capture_output=False):
    """
    Chạy subprocess command trong async context.
    """
    loop = asyncio.get_event_loop()
    
    def _run():
        if capture_output:
            return subprocess.run(
                cmd,
                capture_output=True,
                timeout=timeout,
                preexec_fn=_set_low_priority
            )
        else:
            return subprocess.run(
                cmd,
                timeout=timeout,
                stdout=subprocess.DEVNULL,
                stderr=subprocess.DEVNULL,
                preexec_fn=_set_low_priority
            )
    
    return await loop.run_in_executor(executor, _run)

def _write_file(filepath, content):
    """Write content to file (sync)"""
    with open(filepath, "w", encoding="utf-8") as f:
        f.write(content)


def _read_file(filepath):
    """Read file content (sync)"""
    if os.path.exists(filepath):
        with open(filepath, "r", encoding="utf-8") as f:
            return f.read().strip()
    return ""


def _safe_remove(filepath):
    """Safely remove a file (sync)"""
    if os.path.exists(filepath):
        os.remove(filepath)


def _check_python_syntax(code_file, box_path):
    """Check Python syntax (sync)"""
    compiled_file = py_compile.compile(code_file, doraise=True)
    # Xóa file bytecode
    if compiled_file and os.path.exists(compiled_file):
        os.remove(compiled_file)
    # Xóa __pycache__
    pycache_dir = f"{box_path}/__pycache__"
    if os.path.exists(pycache_dir):
        import shutil
        shutil.rmtree(pycache_dir)


def _read_meta(meta_path):
    """Read isolate meta file (sync)"""
    meta = {}
    if os.path.exists(meta_path):
        with open(meta_path, "r") as f:
            for line in f:
                if ':' in line:
                    k, v = line.strip().split(':', 1)
                    meta[k] = v
    return meta


def _parse_memory_to_kb(val):
    """Parse memory value to KB"""
    if val is None:
        return 0
    s = str(val).strip()
    if s == "":
        return 0
    s = s.strip('"').strip("'")
    try:
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
            return int(float(low[:-1]) / 1024)
        v = float(low)
        if v > 1024 * 1024:
            return int(v // 1024)
        if v > 1024 * 10:
            return int(v // 1024)
        return int(v)
    except Exception:
        return 0


def _get_memory_kb_from_meta(meta, keys_order=None):
    """Extract memory from meta dict"""
    if not meta:
        return 0
    if keys_order is None:
        keys_order = ["cg-mem", "max-rss", "measured", "memory", "mem", "rss"]

    for k in keys_order:
        if k in meta and meta[k] is not None and meta[k] != "":
            kb = _parse_memory_to_kb(meta[k])
            if kb > 0:
                return kb
    for v in meta.values():
        kb = _parse_memory_to_kb(v)
        if kb > 0:
            return kb
    return 0


def _error_result(testcases, error_code, error_msg):
    """Create error result for all testcases"""
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
