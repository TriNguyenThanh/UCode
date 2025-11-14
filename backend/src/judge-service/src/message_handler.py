"""
Async Message Handler Module (safe version)
Xử lý message từ RabbitMQ và chạy sandbox qua process riêng (sync)
"""
import json
import os
import asyncio
import subprocess
import logging

MAX_RETRY_COUNT = int(os.getenv("MAX_RETRY_COUNT", "3"))

class SubmissionStatus:
    PENDING = "Pending"
    RUNNING = "Running"
    PASSED = "Passed"
    FAILED = "Failed"

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

STATUS_MESSAGE = {
    "Pending": "Testcase is pending execution",
    "Running": "Testcase is currently running",
    "Passed": "Testcase passed",
    "Failed": "Testcase failed",
    "TimeLimitExceeded": "Testcase exceeded time limit",
    "MemoryLimitExceeded": "Testcase exceeded memory limit",
    "RuntimeError": "Testcase runtime error",
    "InternalError": "Internal error during testcase execution",
    "WrongAnswer": "Testcase produced wrong answer",
    "CompilationError": "Code compilation error",
    "Skipped": "Testcase was skipped"
}

# Thêm logging
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

class MessageHandler:
    @staticmethod
    async def handle_message(body, properties, retry_count=0):
        # Đọc retry count từ headers nếu có
        if properties.headers:
            retry_count = properties.headers.get('x-retry-count', 0)
        
        result = {
            "should_ack": False,
            "should_requeue": False,
            "response": None,
            "new_body": None,
            "new_headers": None
        }

        # Retry limit
        if retry_count >= MAX_RETRY_COUNT:
            logger.error(f"Message exceeded max retry count ({MAX_RETRY_COUNT})")
            try:
                data = json.loads(body)
                submission_id = data.get("SubmissionId", "unknown")
            except:
                submission_id = "unknown"
            result["should_ack"] = True
            result["response"] = MessageHandler._create_error_response(
                submission_id=submission_id,
                error_code="MAX_RETRY_EXCEEDED"
            )
            return result

        # Parse JSON
        try:
            data = json.loads(body)
        except json.JSONDecodeError as e:
            logger.error(f"Invalid JSON message: {e}")
            result["should_ack"] = True
            result["response"] = MessageHandler._create_error_response(
                submission_id="unknown",
                error_code="INVALID_JSON"
            )
            return result

        submission_id = data.get("SubmissionId", "N/A")
        logger.info(f"Processing submission {submission_id}, retry_count={retry_count}")

        # Validate
        language = data.get("Language")
        code = data.get("Code")
        # TimeLimit: milliseconds → convert to seconds
        # MemoryLimit: KB (giữ nguyên, không convert)
        timelimit_ms = int(data.get("TimeLimit", 3000))  # Default 3000ms
        memorylimit_kb = int(data.get("MemoryLimit", 262144))  # Default 256MB = 262144 KB
        
        # Chuyển đổi TimeLimit từ ms sang seconds
        timelimit = timelimit_ms / 1000.0  # Convert ms to seconds
        memorylimit = memorylimit_kb  # Keep in KB (no conversion needed)
        
        # Validation: đảm bảo giá trị hợp lý
        if timelimit <= 0 or timelimit > 60:  # Max 60 seconds per testcase
            logger.warning(f"Invalid TimeLimit: {timelimit}s, using default 3s")
            timelimit = 3.0
        
        if memorylimit <= 0 or memorylimit > 2097152:  # Max 2GB = 2097152 KB
            logger.warning(f"Invalid MemoryLimit: {memorylimit}KB, using default 262144KB")
            memorylimit = 262144
        
        testcases = data.get("Testcases", [])
        
        logger.info(f"Submission {submission_id}: TimeLimit={timelimit}s, MemoryLimit={memorylimit}KB, Testcases={len(testcases)}")

        if not submission_id or not language or not code:
            result["should_ack"] = True
            result["response"] = MessageHandler._create_error_response(
                submission_id=submission_id,
                error_code="MISSING_REQUIRED_FIELDS"
            )
            return result

        if not testcases:
            result["should_ack"] = True
            result["response"] = MessageHandler._create_error_response(
                submission_id=submission_id,
                error_code="NO_TESTCASES"
            )
            return result

        # Xử lý bằng subprocess (đảm bảo quyền isolate)
        try:
            success, results, error_code, error_msg, compile_result = await MessageHandler._process_submission(
                data=data,
                language=language,
                code=code,
                timelimit=timelimit,
                memorylimit=memorylimit,
                testcases=testcases
            )

            if not success:
                result["should_ack"] = True
                result["response"] = MessageHandler._create_error_response(
                    submission_id=submission_id,
                    error_code=error_code,
                    error_message=error_msg,
                    compile_result=compile_result
                )
                return result

            result["should_ack"] = True
            result["response"] = MessageHandler._create_success_response(
                submission_id=submission_id,
                results=results
            )
            return result

        except Exception as e:
            logger.exception(f"Failed to process submission {submission_id}")
            new_headers = properties.headers.copy() if properties.headers else {}
            new_headers['x-retry-count'] = retry_count + 1
            result.update({
                "should_ack": True,
                "should_requeue": True,
                "new_body": body,
                "new_headers": new_headers
            })
            return result

    # ---------------------------------------------------------------------
    # Helper functions
    # ---------------------------------------------------------------------
    @staticmethod
    def _create_error_response(submission_id, error_code, error_message=None, compile_result=""):
        response = {
            "SubmissionId": submission_id,
            "CompileResult": compile_result,
            "TotalTime": 0,
            "TotalMemory": 0,
            "ErrorCode": error_code,
            "ErrorMessage": error_message or STATUS_MESSAGE.get(error_code, "Unknown error occurred.")
        }
        logger.info(f"[✗] Error response for {submission_id} ({error_code})")
        return response

    @staticmethod
    def _create_success_response(submission_id, results):
        compile_result = ""
        total_time = 0
        total_memory = 0
        first_error_message = ""

        for result in results:
            status = result.get("status", "InternalError")
            status_code = TESTCASE_STATUS_CODE.get(status, "4")
            compile_result += status_code
            total_time += result.get("time", 0)
            total_memory += result.get("memory", 0)
            if status != "Passed" and not first_error_message:
                error_detail = result.get("error", "")
                testcase_index = result.get("indexNo", "?")
                first_error_message = f"Testcase #{testcase_index} - {status}: {error_detail}"

        all_passed = all(c == "0" for c in compile_result)

        response = {
            "SubmissionId": submission_id,
            "CompileResult": compile_result,
            "TotalTime": total_time,
            "TotalMemory": total_memory,
            "ErrorCode": SubmissionStatus.PASSED if all_passed else SubmissionStatus.FAILED,
            "ErrorMessage": "" if all_passed else (first_error_message or "Some testcases failed")
        }
        logger.info(f"[✓] Completed submission {submission_id}")
        return response

    @staticmethod
    async def _process_submission(data, language, code, timelimit, memorylimit, testcases):
        """
        Gọi isolate sync runner qua subprocess
        
        Args:
            data: Full submission data dict
            language: Programming language (python, cpp, etc.)
            code: Source code string
            timelimit: Time limit in SECONDS (đã convert từ ms)
            memorylimit: Memory limit in KB (giữ nguyên từ message)
            testcases: List of testcase dicts
            
        Returns:
            Tuple: (success, results, error_code, error_msg, compile_result)
        """
        submission_id = data.get("SubmissionId", "N/A")
        payload = json.dumps({
            "language": language,
            "code": code,
            "testcases": testcases,
            "timelimit": timelimit,
            "memorylimit": memorylimit
        })
        
        proc = None
        try:
            current_dir = os.path.dirname(os.path.abspath(__file__))
            sandbox_runner_path = os.path.join(current_dir, "sandbox_runner.py")
            
            # Tính timeout cho subprocess
            # Với batch execution, không phải tất cả testcases chạy tuần tự
            max_parallel = int(os.getenv("MAX_PARALLEL_TESTCASES", "4"))
            estimated_batches = (len(testcases) + max_parallel - 1) // max_parallel
            # Mỗi batch timeout = (timelimit + 2s buffer) * số testcases trong batch
            batch_timeout = (timelimit + 2) * max_parallel
            # Tổng timeout = số batch * batch_timeout + 60s buffer
            timeout_seconds = estimated_batches * batch_timeout + 60
            
            # Giới hạn timeout tối đa để tránh treo vô hạn
            max_timeout = 300  # 5 phút
            timeout_seconds = min(timeout_seconds, max_timeout)
            
            logger.info(f"Starting sandbox runner for {submission_id}, timeout={timeout_seconds:.1f}s (batches={estimated_batches}, timelimit={timelimit}s)")
            
            proc = await asyncio.create_subprocess_exec(
                "python3", sandbox_runner_path, payload,
                stdout=asyncio.subprocess.PIPE,
                stderr=asyncio.subprocess.PIPE,
                limit=1024 * 1024 * 10  # 10MB buffer limit
            )
            
            logger.info(f"Subprocess started for {submission_id}, PID={proc.pid}")
            
            try:
                out, err = await asyncio.wait_for(
                    proc.communicate(), 
                    timeout=timeout_seconds
                )
                logger.info(f"Subprocess completed for {submission_id}")
                
                # Log stderr nếu có (debug messages)
                if err:
                    stderr_content = err.decode().strip()
                    if stderr_content:
                        logger.debug(f"Subprocess stderr for {submission_id}:\n{stderr_content}")
                        
            except asyncio.TimeoutError:
                logger.error(f"Subprocess timeout for {submission_id}")
                proc.kill()
                await proc.wait()
                return False, [], "TimeLimitExceeded", "Sandbox execution timeout", "1"

            if proc.returncode != 0:
                error_msg = err.decode().strip()
                logger.error(f"Sandbox runner failed for {submission_id}: {error_msg}")
                return False, [], "InternalError", error_msg, "4"

            output = out.decode().strip()
            logger.info(f"Subprocess output length: {len(output)} bytes")
            
            try:
                isolate_results = json.loads(output)
            except json.JSONDecodeError as e:
                logger.error(f"Invalid JSON from sandbox runner: {e}")
                logger.error(f"Raw output: {output[:500]}")  # Log first 500 chars
                return False, [], "InternalError", f"Invalid JSON output: {str(e)}", "4"
            
            if not isinstance(isolate_results, list) or not isolate_results:
                logger.error(f"Invalid result format from sandbox runner")
                return False, [], "InternalError", "Invalid result from isolate executor", "4"

            first_status = isolate_results[0].get("status")
            if first_status in ("CompilationError", "InternalError"):
                compile_result = TESTCASE_STATUS_CODE.get(first_status, "4")
                return False, isolate_results, first_status, None, compile_result

            logger.info(f"Successfully processed {submission_id}")
            return True, isolate_results, None, None, ""

        except Exception as e:
            logger.exception(f"Exception in sandbox runner for {submission_id}")
            # Cleanup process if still running
            if proc and proc.returncode is None:
                try:
                    logger.warning(f"Killing hanging process for {submission_id}")
                    proc.kill()
                    await proc.wait()
                except Exception as kill_error:
                    logger.error(f"Failed to kill process: {kill_error}")
            return False, [], "InternalError", str(e), "4"