"""
Message Handler Module
Xử lý các message từ RabbitMQ queue
"""
import json
import os
from app.executor_isolate import execute_in_sandbox, TESTCASE_STATUS
from app.health_check import should_accept_submission

MAX_RETRY_COUNT = int(os.getenv("MAX_RETRY_COUNT", "3"))  # Số lần retry tối đa

class SubmissionStatus:
    PENDING = "Pending"
    RUNNING = "Running"
    PASSED = "Passed"
    FAILED = "Failed"

TESTCASE_STATUS_CODE = {
    "Passed": "0",
    "TimeLimitExceeded": "1",
    "MemoryLimitExceeded": "2",
    "RuntimeError": "3",           # Runtime errors (division by zero, null pointer, etc.)
    "InternalError": "4",          # System errors
    "WrongAnswer": "5",
    "CompilationError": "6",       # Syntax errors, compile errors
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

class MessageHandler:
    @staticmethod
    def handle_message(body, properties, retry_count=0):
        """
        Xử lý message submission và trả về response message
        
        Args:
            body: Raw message body từ RabbitMQ
            properties: Message properties (correlation_id, reply_to, headers)
            retry_count: Số lần retry hiện tại
            
        Returns:
            dict: {
                "should_ack": bool,  # True = ACK, False = NACK/Requeue
                "should_requeue": bool,  # True = requeue với retry count tăng
                "response": dict|None,  # Response message để gửi về (nếu có)
                "new_body": bytes|None,  # Message body mới để requeue (nếu cần)
                "new_headers": dict|None  # Headers mới để requeue (nếu cần)
            }
        """
        result = {
            "should_ack": False,
            "should_requeue": False,
            "response": None,
            "new_body": None,
            "new_headers": None
        }
        
        # 1. Kiểm tra retry count
        if retry_count >= MAX_RETRY_COUNT:
            print(f"[ERROR] Message exceeded max retry count ({MAX_RETRY_COUNT})")
            try:
                print(f"[DEBUG] Raw message: {json.dumps(json.loads(body), indent=4)}")
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
        
        # 2. Parse JSON
        try:
            data = json.loads(body)
        except json.JSONDecodeError as e:
            print(f"[ERROR] Invalid JSON message: {e}")
            result["should_ack"] = True
            result["response"] = MessageHandler._create_error_response(
                submission_id="unknown",
                error_code="INVALID_JSON"
            )
            return result
        
        submission_id = data.get("SubmissionId", "N/A")
        print(f"[*] Processing submission {submission_id}")
        print(f"[DEBUG] Message data: language={data.get('Language')}, testcases_count={len(data.get('Testcases', []))}")
        
        # 3. Kiểm tra health last-minute
        can_accept, reason = should_accept_submission()
        if not can_accept:
            print(f"[WARNING] Last-minute overload detected, requeuing {submission_id} (retry {retry_count + 1}/{MAX_RETRY_COUNT})")
            
            # Chuẩn bị requeue
            new_headers = properties.headers.copy() if properties.headers else {}
            new_headers['x-retry-count'] = retry_count + 1
            
            result["should_ack"] = True  # ACK message cũ
            result["should_requeue"] = True
            result["new_body"] = body
            result["new_headers"] = new_headers
            return result
        
        # 4. Validate dữ liệu
        language = data.get("Language")
        code = data.get("Code")
        timelimit = data.get("TimeLimit", 3)
        memorylimit = data.get("MemoryLimit", 256)
        testcases = data.get("Testcases", [])
        
        if not submission_id:
            print(f"[ERROR] Invalid submission: missing submissionId")
            result["should_ack"] = True
            result["response"] = MessageHandler._create_error_response(
                submission_id=submission_id,
                error_code="MISSING_REQUIRED_FIELDS"
            )
            return result
        
        if not language or not code:
            print(f"[ERROR] Invalid submission {submission_id}: missing language or code")
            result["should_ack"] = True
            result["response"] = MessageHandler._create_error_response(
                submission_id=submission_id,
                error_code="MISSING_REQUIRED_FIELDS"
            )
            return result
        
        if not testcases or len(testcases) == 0:
            print(f"[WARNING] Submission {submission_id} has no testcases")
            result["should_ack"] = True
            result["response"] = MessageHandler._create_error_response(
                submission_id=submission_id,
                error_code="NO_TESTCASES"
            )
            return result
        
        # 5. Xử lý submission
        try:
            success, results, error_code, error_msg, compile_result = MessageHandler._process_submission(
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
            
            # 6. Tạo success response
            result["should_ack"] = True
            result["response"] = MessageHandler._create_success_response(
                submission_id=submission_id,
                results=results
            )
            return result
            
        except Exception as e:
            print(f"[ERROR] Failed to process submission {submission_id}: {e}")
            print(f"[DEBUG] Retry count: {retry_count}/{MAX_RETRY_COUNT}")
            
            # Requeue với retry count tăng
            new_headers = properties.headers.copy() if properties.headers else {}
            new_headers['x-retry-count'] = retry_count + 1
            
            result["should_ack"] = True  # ACK message cũ
            result["should_requeue"] = True
            result["new_body"] = body
            result["new_headers"] = new_headers
            return result
    
    @staticmethod
    def _create_error_response(submission_id, error_code, error_message=None, compile_result=""):
        """
        Tạo error response message theo format C#:
        class ResultMessageResponse {
            public Guid SubmissionId { get; set; }
            public string CompileResult { get; set; } = null!;
            public int TotalTime { get; set; }
            public int TotalMemory { get; set; }
            public string ErrorCode { get; set; } = null!;
            public string ErrorMessage { get; set; } = null!;
        }
        """
        response = {
            "SubmissionId": submission_id,
            "CompileResult": compile_result,  # "6" cho CompilationError, "3" cho RuntimeError
            "TotalTime": total_time,
            "TotalMemory": total_memory,
            "ErrorCode": error_code,
            "ErrorMessage": error_message
        }
        
        if error_message is None:
            if error_code == "MAX_RETRY_EXCEEDED":
                response["ErrorMessage"] = "Failed after maximum retry attempts. Please contact system administrator."
            elif error_code == "INVALID_JSON":
                response["ErrorMessage"] = "Invalid JSON message format."
            elif error_code == "MISSING_REQUIRED_FIELDS":
                response["ErrorMessage"] = "Missing required fields in submission."
            elif error_code == "NO_TESTCASES":
                response["ErrorMessage"] = "No testcases provided."
            else:
                response["ErrorMessage"] = STATUS_MESSAGE.get(error_code, "Unknown error occurred.")
        
        print(f"[✓] Created error response for submission {submission_id} ({error_code})")
        print("=" * 81)
        return response
    
    @staticmethod
    def _create_success_response(submission_id, results):
        """
        Tạo success response message theo format C#
        CompileResult: String với mỗi ký tự là status code của testcase tương ứng
        Ví dụ: "00125" = testcase 1,2,3 passed (0), testcase 4 TLE (1), testcase 5 WrongAnswer (5)
        """
        # Build CompileResult string từ results
        compile_result = ""
        total_time = 0
        total_memory = 0
        
        for result in results:
            # Lấy status code (0-7) từ result
            status = result.get("status", "InternalError")
            status_code = TESTCASE_STATUS_CODE.get(status, "4")  # Default to InternalError (4)
            compile_result += status_code
            
            # Accumulate time và memory
            total_time += result.get("time", 0)
            total_memory += result.get("memory", 0)
            
            # Log từng testcase
            if status == "Passed":
                print(f"[✓] Test case {result.get('testcaseId')} passed")
            else:
                print(f"[ERROR] Test case {result.get('testcaseId')} failed: {status}")
        
        # Kiểm tra nếu tất cả passed
        all_passed = all(c == "0" for c in compile_result)
        
        response = {
            "SubmissionId": submission_id,
            "CompileResult": compile_result,
            "TotalTime": total_time,
            "TotalMemory": total_memory,
            "ErrorCode": SubmissionStatus.PASSED if all_passed else SubmissionStatus.FAILED,
            "ErrorMessage": "" if all_passed else "Some test cases failed"
        }
        
        print(f"[✓] Created success response for submission {submission_id}")
        print(f"    CompileResult: {compile_result} ({len(results)} testcases)")
        print(f"    TotalTime: {total_time}ms, TotalMemory: {total_memory}KB")
        print("=" * 81)
        return response
    
    @staticmethod
    def _process_submission(data, language, code, timelimit, memorylimit, testcases):
        """
        Xử lý submission bằng cách chạy testcases
        
        Args:
            data: Dữ liệu submission gốc
            language: Ngôn ngữ lập trình
            code: Source code
            timelimit: Giới hạn thời gian (giây)
            memorylimit: Giới hạn bộ nhớ (MB)
            testcases: Danh sách testcases theo format C# (TestCaseId, IndexNo, InputRef, OutputRef)
            
        Returns:
            tuple: (success: bool, results: list, error_code: str|None, error_msg: str|None, compile_result: str)
        """
        submission_id = data.get("SubmissionId", "N/A")

        try:
            # Chạy tất cả testcases cùng lúc (executor sẽ sort theo IndexNo)
            isolate_results = execute_in_sandbox(
                language=language,
                code=code,
                testcases=testcases,  # Pass toàn bộ testcases với format C#
                timelimit=timelimit,
                memorylimit=memorylimit
            )

            # Validate results
            if not isinstance(isolate_results, list) or len(isolate_results) == 0:
                print(f"[ERROR] Invalid result from isolate executor for {submission_id}")
                return False, [], "InternalError", "Invalid result from isolate executor", "4"

            # Kiểm tra nếu có CompilationError hoặc InternalError nghiêm trọng
            first_result = isolate_results[0]
            first_status = first_result.get("status")
            
            if first_status in ("CompilationError", "InternalError"):
                print(f"[ERROR] Critical error: {first_status} for {submission_id}")
                compile_result = TESTCASE_STATUS_CODE.get(first_status, "4")
                return False, isolate_results, first_status, None, compile_result
            
            # Trả về kết quả thành công
            return True, isolate_results, None, None, ""

        except Exception as e:
            print(f"[ERROR] Exception during execution for {submission_id}: {e}")
            return False, [], "InternalError", str(e), "4"