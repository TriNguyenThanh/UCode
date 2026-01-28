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
import traceback
import shutil
import atexit
from concurrent.futures import ThreadPoolExecutor

# Default limits
DEFAULT_MEMORY_LIMIT = int(os.getenv("DEFAULT_MEMORY_LIMIT", "262144"))
DEFAULT_TIME_LIMIT = int(os.getenv("DEFAULT_TIME_LIMIT", "2"))
MAX_PARALLEL_TESTCASES = int(os.getenv("MAX_PARALLEL_TESTCASES", "5"))

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

# Thread pool with shutdown handling (CRITICAL #2)
executor = ThreadPoolExecutor(max_workers=MAX_PARALLEL_TESTCASES * 2)

def _shutdown_executor():
    try:
        executor.shutdown(wait=False)
    except: pass

atexit.register(_shutdown_executor)

LOW_PRIORITY_NICE = int(os.getenv("ISOLATE_NICE", "10"))
ISOLATE_CPU_AFFINITY = os.getenv("ISOLATE_CPU_AFFINITY", "").strip()
ISOLATE_ROOT = "/var/local/lib/isolate"

def debug_log(msg):
    # LOW #15: Handle closed stderr
    try:
        print(msg, file=sys.stderr, flush=True)
    except (ValueError, OSError):
        pass

def _sanitize_error(msg):
    """Remove internal paths from error messages"""
    if not msg: return ""
    return str(msg).replace(ISOLATE_ROOT, "[SANDBOX]")

async def execute_in_sandbox(language, code, testcases, timelimit=None, memorylimit=None, mem_keys=None, slot_id=0):
    """
    Execute code using Optimized Write-Over-Run Strategy.
    """
    # 1. Validation
    if not code:
        return _error_result(testcases, TESTCASE_STATUS.InternalError, "No code provided")
    if not testcases:
        return []
    
    if timelimit is None: timelimit = DEFAULT_TIME_LIMIT
    if memorylimit is None: memorylimit = DEFAULT_MEMORY_LIMIT
    if timelimit <= 0: timelimit = DEFAULT_TIME_LIMIT
    if memorylimit <= 0: memorylimit = DEFAULT_MEMORY_LIMIT
    
    if mem_keys is None:
        mem_keys = ["cg-mem", "max-rss", "measured", "memory", "mem", "rss"]

    # Calculate Box Range
    start_box = slot_id * MAX_PARALLEL_TESTCASES
    end_box = start_box + MAX_PARALLEL_TESTCASES - 1
    
    # LOW #23: Validate Box ID Range
    # Assuming max boxes 20 (0-19)
    if not (0 <= start_box < 20) or not (0 <= end_box < 20):
        debug_log(f"[ERROR] Invalid box range {start_box}-{end_box} for slot {slot_id}")
        return _error_result(testcases, TESTCASE_STATUS.InternalError, "System Configuration Error: Box ID out of range")

    debug_log(f"[SLOT {slot_id}] Starting execution (Boxes {start_box}-{end_box})")

    # Sort testcases
    sorted_testcases = sorted(testcases, key=lambda tc: tc.get("IndexNo", 0))
    results = []

    try:
        # --- PHASE 1: COLD START ---
        debug_log(f"[SLOT {slot_id}] Phase 1: Init & Compile")
        
        # 1.1 Init all boxes in parallel
        await _init_slot_boxes(slot_id)
        
        # 1.2 Compile (using local box 0)
        compile_box_id = start_box
        run_cmd = await _compile_and_broadcast(slot_id, language, code, compile_box_id, timelimit)
        
        # --- PHASE 2: EXECUTION LOOP (BATCHED) ---
        debug_log(f"[SLOT {slot_id}] Phase 2: Execution Loop (Write-Over with Batching)")
        
        num_batches = (len(sorted_testcases) + MAX_PARALLEL_TESTCASES - 1) // MAX_PARALLEL_TESTCASES
        stop_execution = False
        
        for batch_idx in range(num_batches):
            start_idx = batch_idx * MAX_PARALLEL_TESTCASES
            end_idx = min(start_idx + MAX_PARALLEL_TESTCASES, len(sorted_testcases))
            batch = sorted_testcases[start_idx:end_idx]
            
            # CHECK EARLY STOP
            if stop_execution:
                for tc in batch:
                    results.append({
                        "testcaseId": tc.get("TestCaseId"),
                        "indexNo": tc.get("IndexNo", 0),
                        "status": TESTCASE_STATUS.TimeLimitExceeded,
                        "time": 0, "memory": 0, "output": "",
                        "error": "Skipped due to early stopping"
                    })
                continue

            # debug_log(f"[SLOT {slot_id}] Batch {batch_idx+1}/{num_batches}: Running {len(batch)} testcases")
            
            batch_tasks = []
            for i, tc in enumerate(batch):
                local_id = i 
                box_id = start_box + local_id
                
                batch_tasks.append(_run_testcase_optimized(
                    tc, box_id, run_cmd, 
                    timelimit, memorylimit, mem_keys
                ))
            
            batch_results = await asyncio.gather(*batch_tasks, return_exceptions=True)
            
            # Process results and handle exceptions
            processed_results = []
            for res in batch_results:
                if isinstance(res, Exception):
                    debug_log(f"[ERROR] Batch worker exception: {res}")
                    processed_results.append({
                        "status": TESTCASE_STATUS.InternalError,
                        "error": _sanitize_error(str(res))
                    })
                else:
                    processed_results.append(res)
            
            results.extend(processed_results)
            
            # CHECK FOR ALL TLE
            tle_count = sum(1 for r in processed_results if r.get("status") == TESTCASE_STATUS.TimeLimitExceeded)
            if tle_count == len(processed_results) and len(processed_results) > 0:
                debug_log(f"[SLOT {slot_id}] EARLY STOPPING: All testcases in batch {batch_idx+1} are TLE.")
                stop_execution = True

        return results

    except asyncio.CancelledError:
        debug_log(f"[SLOT {slot_id}] Execution Cancelled")
        raise
    except Exception as e:
        debug_log(f"[ERROR] Slot {slot_id} critical error: {e}")
        traceback.print_exc()
        return _error_result(sorted_testcases, TESTCASE_STATUS.InternalError, _sanitize_error(str(e)))
        
    finally:
        # --- PHASE 3: TEARDOWN ---
        # Cleanup is critical
        debug_log(f"[SLOT {slot_id}] Phase 3: Cleanup")
        await _cleanup_slot_boxes(slot_id)


async def _init_slot_boxes(slot_id):
    """Run isolate --init for all boxes in slot"""
    tasks = []
    start_box = slot_id * MAX_PARALLEL_TESTCASES
    for i in range(MAX_PARALLEL_TESTCASES):
        box_id = start_box + i
        tasks.append(_run_command(["isolate", "--box-id", str(box_id), "--init"], timeout=10))
    
    results = await asyncio.gather(*tasks, return_exceptions=True)
    
    failures = []
    for i, res in enumerate(results):
        if isinstance(res, Exception) or (hasattr(res, 'returncode') and res.returncode != 0):
             failures.append(f"Box {start_box+i}")
    
    if failures:
        raise RuntimeError(f"Init failed for: {', '.join(failures)}")

    debug_log(f"[SLOT {slot_id}] Initialized boxes {start_box}-{start_box+MAX_PARALLEL_TESTCASES-1}")

async def _cleanup_slot_boxes(slot_id):
    """Run isolate --cleanup for all boxes"""
    tasks = []
    start_box = slot_id * MAX_PARALLEL_TESTCASES
    for i in range(MAX_PARALLEL_TESTCASES):
        box_id = start_box + i
        tasks.append(_run_command(["isolate", "--box-id", str(box_id), "--cleanup"], timeout=5))
    
    await asyncio.gather(*tasks, return_exceptions=True)

async def _compile_and_broadcast(slot_id, language, code, compile_box_id, timelimit):
    """
    Compile in compile_box_id, then copy exe to other boxes in the slot.
    Returns run_cmd.
    """
    loop = asyncio.get_event_loop()
    box_path = f"{ISOLATE_ROOT}/{compile_box_id}/box"
    
    # 1. Write Code
    if language == "python":
        code_file = f"{box_path}/main.py"
        await loop.run_in_executor(None, _write_file, code_file, code)
        
        # Check syntax
        try:
            await loop.run_in_executor(None, _check_python_syntax, code_file, box_path)
        except Exception as e:
             raise ValueError(_sanitize_error(f"Python Syntax Error:\n{e}"))
            
        # Broadcast (Copy main.py to other boxes)
        await _broadcast_file(slot_id, compile_box_id, "main.py")
        return ["/usr/bin/python3", "main.py"]

    elif language == "cpp":
        code_file = f"{box_path}/main.cpp"
        await loop.run_in_executor(None, _write_file, code_file, code)
        
        # Compile
        compile_cmd = [
            "isolate", "--box-id", str(compile_box_id),
            "--time=10", "--wall-time=15", "--mem=512000", "--processes", "--full-env",
            "--stdout=compile_out.txt", "--stderr=compile_err.txt",
            "--run", "--",
            "/usr/bin/g++", "-std=c++17", "-O2", "-Wall", "-Wextra",
            "-o", "main", "main.cpp"
        ]
        
        res = await _run_command(compile_cmd, timeout=20, capture_output=True)
        if res.returncode != 0:
            stderr = await loop.run_in_executor(None, _read_file, f"{box_path}/compile_err.txt")
            stdout = await loop.run_in_executor(None, _read_file, f"{box_path}/compile_out.txt")
            raise ValueError(_sanitize_error(f"C++ Compile Error:\n{stderr}\n{stdout}"))
            
        # Broadcast (Copy binary 'main')
        await _broadcast_file(slot_id, compile_box_id, "main")
        return ["./main"]
    else:
        raise ValueError(f"Unsupported language: {language}")

async def _broadcast_file(slot_id, src_box_id, filename):
    """Copy file from src_box to all other boxes in slot"""
    loop = asyncio.get_event_loop()
    start_box = slot_id * MAX_PARALLEL_TESTCASES
    src_path = f"{ISOLATE_ROOT}/{src_box_id}/box/{filename}"
    
    def copy_task(target_box_id):
        if target_box_id == src_box_id: return
        dst_path = f"{ISOLATE_ROOT}/{target_box_id}/box/{filename}"
        
        # HIGH #6: Simple race condition protection / error handling
        try:
            if not os.path.exists(src_path):
                raise FileNotFoundError(f"Source missing: {src_path}")
            shutil.copy2(src_path, dst_path)
        except Exception as e:
            raise RuntimeError(f"Copy failed {target_box_id}: {e}")
    
    tasks = []
    for i in range(MAX_PARALLEL_TESTCASES):
        target_id = start_box + i
        if target_id != src_box_id:
            tasks.append(loop.run_in_executor(None, copy_task, target_id))
            
    results = await asyncio.gather(*tasks, return_exceptions=True)
    failures = [str(r) for r in results if isinstance(r, Exception)]
    if failures:
        raise RuntimeError(f"Broadcast failed: {'; '.join(failures)}")
        
    debug_log(f"[SLOT {slot_id}] Broadcasted '{filename}' to all boxes")

async def _run_testcase_optimized(tc, box_id, run_cmd, timelimit, memorylimit, mem_keys):
    """
    Run testcase using Write-Over strategy (No Init/Cleanup).
    """
    tc_id = tc.get("TestCaseId", "unknown")
    index_no = tc.get("IndexNo", 0)
    input_ref = str(tc.get("InputRef", "")).strip()
    output_ref = str(tc.get("OutputRef", "")).strip()
    
    box_path = f"{ISOLATE_ROOT}/{box_id}/box"
    input_file = f"{box_path}/input.txt"
    output_file = f"{box_path}/output.txt"
    meta_file = f"{box_path}/meta.txt"
    error_file = f"{box_path}/error.txt"
    
    result = {
        "testcaseId": tc_id,
        "indexNo": index_no,
        "status": TESTCASE_STATUS.Pending,
        "time": 0, "memory": 0, "output": "", "error": ""
    }
    
    try:
        loop = asyncio.get_event_loop()
        
        # 1. WRITE INPUT (Overwrite)
        try:
            await loop.run_in_executor(None, _write_file, input_file, input_ref)
        except Exception as e:
            result["status"] = TESTCASE_STATUS.InternalError
            result["error"] = f"Input write failed: {_sanitize_error(e)}"
            return result
        
        # 2. EXECUTE
        isolate_cmd = [
            "isolate", "--box-id", str(box_id),
            "--stdin=input.txt", "--stdout=output.txt", "--stderr=error.txt",
            f"--time={timelimit}", f"--wall-time={timelimit + 1}",
            f"--mem={memorylimit}", "--processes",
            "--meta", meta_file,
            "--run", "--"
        ] + run_cmd
        
        start_t = time.time()
        
        try:
            # CRITICAL #1: Handle subprocess timeout explicitly
            exec_res = await _run_command(isolate_cmd, timeout=timelimit + 5, capture_output=True)
        except subprocess.TimeoutExpired:
            result["status"] = TESTCASE_STATUS.TimeLimitExceeded
            result["error"] = "Execution Timeout (Subprocess)"
            result["time"] = (timelimit + 5) * 1000
            return result
            
        exec_time_ms = int((time.time() - start_t) * 1000)
        
        # 3. READ RESULTS
        meta = await loop.run_in_executor(None, _read_meta, meta_file)
        
        result["time"] = int(float(meta.get("time", exec_time_ms/1000)) * 1000)
        result["memory"] = _get_memory_kb_from_meta(meta, mem_keys)
        
        status = meta.get("status", "")
        if status == "TO":
            result["status"] = TESTCASE_STATUS.TimeLimitExceeded
        elif status in ("RE", "SG"):
            err_content = await loop.run_in_executor(None, _read_file, error_file)
            result["status"] = TESTCASE_STATUS.RuntimeError
            result["error"] = _sanitize_error(err_content or meta.get("message", "Runtime Error"))
        elif status == "XX":
             msg = await loop.run_in_executor(None, _read_file, error_file)
             result["status"] = TESTCASE_STATUS.InternalError
             result["error"] = f"Sandbox Internal Error: {_sanitize_error(msg)}"
        elif exec_res.returncode != 0:
             result["status"] = TESTCASE_STATUS.RuntimeError
             result["error"] = f"Non-zero exit code {exec_res.returncode}"
        else:
            actual = await loop.run_in_executor(None, _read_file, output_file)
            result["output"] = actual
            if actual == output_ref:
                result["status"] = TESTCASE_STATUS.Passed
            else:
                result["status"] = TESTCASE_STATUS.WrongAnswer
                result["error"] = f"Expected start: {output_ref[:50]}... Got: {actual[:50]}..."
                
        return result

    except Exception as e:
        result["status"] = TESTCASE_STATUS.InternalError
        result["error"] = _sanitize_error(str(e))
        return result

# --- HELPERS ---

def _set_low_priority():
    try:
        os.nice(LOW_PRIORITY_NICE)
    except: pass
    if ISOLATE_CPU_AFFINITY:
        try: pass 
        except: pass

async def _run_command(cmd, timeout=None, capture_output=False):
    loop = asyncio.get_event_loop()
    def _run():
        # CRITICAL #1: subprocess.run raises TimeoutExpired if timeout occurs
        # We allow it to bubble up to be caught by caller (_run_testcase_optimized)
        kwargs = {"timeout": timeout, "preexec_fn": _set_low_priority}
        if capture_output:
            return subprocess.run(cmd, capture_output=True, **kwargs)
        else:
            return subprocess.run(cmd, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL, **kwargs)
    return await loop.run_in_executor(executor, _run)

def _write_file(path, content):
    try:
        with open(path, "w", encoding="utf-8") as f: f.write(content)
    except Exception as e:
        raise OSError(f"Write failed {path}: {e}")

def _read_file(path):
    try:
        if os.path.exists(path):
            with open(path, "r", encoding="utf-8") as f: return f.read().strip()
    except Exception:
        pass
    return ""

def _read_meta(path):
    m = {}
    try:
        if os.path.exists(path):
            with open(path, "r") as f:
                for line in f:
                    if ':' in line: 
                        k,v = line.strip().split(':', 1)
                        m[k] = v
    except Exception:
         pass
    return m

def _check_python_syntax(code_file, box_path):
    try:
        py_compile.compile(code_file, doraise=True)
    except py_compile.PyCompileError as e:
        raise RuntimeError(str(e))
    except Exception as e:
        raise RuntimeError(f"Syntax check error: {e}")
    finally:
        try:
            pycache = os.path.join(box_path, "__pycache__")
            if os.path.exists(pycache): shutil.rmtree(pycache)
        except: pass

def _parse_memory_to_kb(val):
    if not val: return None # MEDIUM #13: Return None instead of 0
    s = str(val).lower().strip()
    if s.endswith("mb"): return int(float(s[:-2])*1024)
    if s.endswith("kb"): return int(float(s[:-2]))
    try: return int(float(s))
    except: return None

def _get_memory_kb_from_meta(meta, keys):
    for k in keys:
        if k in meta: 
            v = _parse_memory_to_kb(meta[k])
            if v is not None: return v
    return 0

def _error_result(testcases, status, msg):
    return [{"testcaseId": tc.get("TestCaseId"), "status": status, "error": msg} for tc in testcases]
