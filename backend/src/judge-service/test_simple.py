import subprocess
import os
import shutil
import concurrent.futures
import multiprocessing
import traceback
import time
from dataclasses import dataclass, asdict
from typing import List, Optional

# Cấu hình
ISOLATE_BIN = "/usr/local/bin/isolate"
SANDBOX_ROOT = "/var/local/lib/isolate"

@dataclass
class TestCaseResult:
    test_id: int
    verdict: str  # AC, WA, TLE, MLE, RE, OLE
    time: float
    memory: int
    message: str

@dataclass
class SubmissionResult:
    submission_id: str
    status: str
    verdict: str
    score: float
    compile_msg: str
    details: List[TestCaseResult]
    system_error: Optional[str] = None

# --- Quản lý Box ID (tránh xung đột) ---
# Sử dụng Queue để quản lý pool các box ID có sẵn (0-99)
box_pool = multiprocessing.Manager().Queue()
for i in range(100): 
    box_pool.put(i)

class JudgeError(Exception):
    """Lỗi nội bộ của hệ thống chấm (không phải lỗi sinh viên)"""
    pass

def safe_run_command(cmd, input_data=None, timeout=None):
    """Wrapper cho subprocess để bắt lỗi hệ thống"""
    try:
        res = subprocess.run(
            cmd, 
            input=input_data, 
            capture_output=True, 
            text=True, # Python 3.7+ xử lý string
            timeout=timeout
        )
        return res
    except subprocess.TimeoutExpired:
        raise JudgeError("System timeout executing isolate command")
    except Exception as e:
        raise JudgeError(f"Subprocess failed: {str(e)}")

def parse_meta_file(meta_path):
    """Đọc file meta của isolate để xác định lỗi RE/TLE/MLE"""
    info = {'time': 0.0, 'max-rss': 0, 'status': 'OK', 'message': ''}
    if not os.path.exists(meta_path):
        return info # Mặc định
        
    with open(meta_path, 'r') as f:
        for line in f:
            parts = line.strip().split(':', 1)
            if len(parts) == 2:
                key, val = parts
                if key == 'time': info['time'] = float(val)
                elif key == 'max-rss': info['max-rss'] = int(val)
                elif key == 'status': info['status'] = val
                elif key == 'message': info['message'] = val
    return info

def run_testcase(box_id, binary_path, testcase, time_limit, mem_limit) -> TestCaseResult:
    """Chạy 1 testcase cụ thể trong box đã định"""
    # 1. Setup paths
    box_dir = os.path.join(SANDBOX_ROOT, str(box_id), "box")
    meta_path = os.path.join("/tmp", f"meta_{box_id}.txt")
    
    # 2. Prepare Input
    # Copy hoặc write input file
    input_file = os.path.join(box_dir, "stdin.txt")
    try:
        with open(input_file, "w") as f:
            f.write(testcase['input'])
    except OSError as e:
        return TestCaseResult(testcase['id'], "IE", 0, 0, f"Disk Error: {str(e)}")

    # 3. Execute Isolate
    cmd = [
        ISOLATE_BIN, "--box-id", str(box_id),
        "--meta", meta_path,
        "--time", str(time_limit),
        "--wall-time", str(time_limit * 3), # Wall time gấp 3 để tránh kill nhầm
        "--mem", str(mem_limit),
        "--fsize", "10240", # Max output 10MB (chống OLE spam)
        "--run", "--", "./solution"
    ]
    
    # Redirect IO
    stdout_path = os.path.join(box_dir, "stdout.txt")
    
    try:
        with open(input_file, 'r') as fin, open(stdout_path, 'w') as fout:
            # Lưu ý: subprocess gọi isolate, isolate gọi code user.
            # Ta không set timeout cho subprocess này vì isolate tự quản lý time.
            res = subprocess.run(cmd, stdin=fin, stdout=fout, stderr=subprocess.PIPE)
    except Exception as e:
        return TestCaseResult(testcase['id'], "IE", 0, 0, f"Execution failed: {str(e)}")

    # 4. Analyze Result
    meta = parse_meta_file(meta_path)
    
    # Mapping isolate status -> System Verdict
    verdict = "AC"
    msg = meta['message']
    
    if meta['status'] == 'TO': verdict = "TLE"
    elif meta['status'] == 'SG': verdict = "RE" # Segfault
    elif meta['status'] == 'RE': verdict = "RE" # Runtime Error (return != 0)
    elif meta['status'] == 'XX': verdict = "IE" # Internal Error của Isolate
    elif meta['status'] == 'OL': verdict = "OLE" # Output Limit
    
    # Nếu isolate báo OK, check tính đúng đắn (Logic Check)
    if verdict == "AC":
        try:
            with open(stdout_path, 'r') as f:
                actual_out = f.read().strip()
            expected_out = testcase['output'].strip()
            
            if actual_out != expected_out:
                verdict = "WA"
                msg = f"Expected length {len(expected_out)}, got {len(actual_out)}"
        except Exception as e:
            verdict = "IE"
            msg = f"Read output failed: {str(e)}"

    return TestCaseResult(testcase['id'], verdict, meta['time'], meta['max-rss'], msg)

def process_single_submission(sub_data):
    """
    Hàm xử lý trọn vẹn 1 bài nộp.
    Được chạy trong 1 Process riêng biệt.
    """
    sub_id = sub_data['id']
    code = sub_data['code']
    lang = sub_data['lang'] # 'cpp', 'py', etc.
    testcases = sub_data['testcases']
    
    # Lấy 1 Box để Compile (Tạm gọi là Master Box cho sub này)
    # Trong mô hình này, ta cần 1 box để compile, và N box để chạy test.
    # Để tối ưu, ta compile ở box_id_0, sau đó copy binary sang các box khác.
    
    compile_box_id = box_pool.get()
    
    try:
        # --- GIAI ĐOẠN 1: INIT & COMPILE ---
        safe_run_command([ISOLATE_BIN, "--box-id", str(compile_box_id), "--init"])
        box_dir = os.path.join(SANDBOX_ROOT, str(compile_box_id), "box")
        
        # Ghi code
        src_file = "solution.cpp" if lang == 'cpp' else "solution.py"
        with open(os.path.join(box_dir, src_file), "w") as f:
            f.write(code)
            
        # Compile (Ví dụ C++)
        if lang == 'cpp':
            cmd_compile = [
                ISOLATE_BIN, "--box-id", str(compile_box_id),
                "--mem", "512000", "--processes", "20", "--time", "10",
                "--run", "--", "/usr/bin/g++", src_file, "-o", "solution"
            ]
            res = safe_run_command(cmd_compile)
            if res.returncode != 0:
                # Trả về Compile Error
                # Đọc stderr để lấy lỗi compile
                err_msg = ""
                # Mẹo: isolate redirect stderr của code user ra stderr của chính nó
                # Cần đọc file stderr trong box nếu muốn chi tiết hơn
                return SubmissionResult(sub_id, "PROCESSED", "CE", 0, "Compilation Failed", [], None)
                
        binary_source_path = os.path.join(box_dir, "solution")
        
        # --- GIAI ĐOẠN 2: CHẠY TEST SONG SONG (Multithreading) ---
        # Ta cần N box con cho N testcase.
        # Ở đây dùng ThreadPool, mỗi thread sẽ claim 1 box ID mới từ pool.
        
        def worker_run_test(tc):
            # Mỗi testcase cần 1 box riêng để không đánh nhau file stdin/stdout
            my_box_id = box_pool.get()
            try:
                safe_run_command([ISOLATE_BIN, "--box-id", str(my_box_id), "--init"])
                # Copy binary từ compile box sang running box
                my_box_dir = os.path.join(SANDBOX_ROOT, str(my_box_id), "box")
                shutil.copy(binary_source_path, os.path.join(my_box_dir, "solution"))
                
                # Run
                return run_testcase(my_box_id, None, tc, 1.0, 128000)
            except Exception as e:
                return TestCaseResult(tc['id'], "IE", 0, 0, str(e))
            finally:
                safe_run_command([ISOLATE_BIN, "--box-id", str(my_box_id), "--cleanup"])
                box_pool.put(my_box_id)

        results = []
        with concurrent.futures.ThreadPoolExecutor(max_workers=5) as executor:
            future_to_tc = {executor.submit(worker_run_test, tc): tc for tc in testcases}
            for future in concurrent.futures.as_completed(future_to_tc):
                results.append(future.result())
                
        # --- GIAI ĐOẠN 3: TỔNG HỢP KẾT QUẢ ---
        final_verdict = "AC"
        total_score = 0
        point_per_test = 100 / len(testcases)
        
        for r in results:
            if r.verdict != "AC":
                final_verdict = r.verdict # Lấy lỗi đầu tiên gặp (thường ưu tiên RE/TLE > WA)
            else:
                total_score += point_per_test
                
        return SubmissionResult(sub_id, "PROCESSED", final_verdict, total_score, "", results, None)

    except Exception as e:
        # Bắt lỗi không mong muốn ở cấp độ Submission
        return SubmissionResult(sub_id, "ERROR", "IE", 0, "", [], str(traceback.format_exc()))
        
    finally:
        # Dọn dẹp box compile
        safe_run_command([ISOLATE_BIN, "--box-id", str(compile_box_id), "--cleanup"])
        box_pool.put(compile_box_id)

# --- API MÔ PHỎNG (ENTRY POINT) ---
if __name__ == "__main__":
    # Dữ liệu giả lập 5 bài nộp
    dummy_submissions = []
    for i in range(5):
        dummy_submissions.append({
            'id': f'sub_{i}',
            'lang': 'cpp',
            'code': '#include <iostream>\nint main() { int a; std::cin >> a; std::cout << a; return 0; }',
            'testcases': [{'id': j, 'input': '10', 'output': '10'} for j in range(5)]
        })

    print("Hệ thống bắt đầu chấm...")
    start_time = time.time()

    # Dùng ProcessPoolExecutor để chạy 5 bài nộp trên 5 Process khác nhau (tận dụng đa nhân)
    # Đây là tầng Parallelism thứ nhất
    with concurrent.futures.ProcessPoolExecutor(max_workers=5) as executor:
        futures = [executor.submit(process_single_submission, sub) for sub in dummy_submissions]
        
        for future in concurrent.futures.as_completed(futures):
            res = future.result()
            print(f"Done Submission {res.submission_id}: Verdict={res.verdict}, Score={res.score}")
            if res.verdict == "IE":
                print(f"System Error Detail: {res.system_error}")

    print(f"Tổng thời gian: {time.time() - start_time:.2f}s")