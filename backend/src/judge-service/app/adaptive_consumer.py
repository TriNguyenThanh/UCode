# Import thư viện pika để làm việc với RabbitMQ message queue
import pika
import json
import os
import time
import threading
from concurrent.futures import ThreadPoolExecutor, as_completed
from app.executor_isolate import execute_in_sandbox, TESTCASE_STATUS
from app.health_check import should_accept_submission, log_system_stats

class SubmissionStatus:
    PENDING = "Pending"
    RUNNING = "Running"
    PASSED = "Passed"
    FAILED = "Failed"
    TIME_LIMIT_EXCEEDED = "TimeLimitExceeded"
    MEMORY_LIMIT_EXCEEDED = "MemoryLimitExceeded"
    RUNTIME_ERROR = "RuntimeError"
    INTERNAL_ERROR = "InternalError"
    WRONG_ANSWER = "WrongAnswer"
    COMPILATION_ERROR = "CompilationError"
    SKIPPED = "Skipped"


# =============================================================================
# ADAPTIVE CONSUMER - Tự động điều chỉnh theo tải hệ thống
# =============================================================================
# Thay vì reject và requeue, consumer sẽ:
# 1. TẠM DỪNG khi hệ thống quá tải (stop consuming)
# 2. TỰ ĐỘNG BẬT LẠI khi hệ thống khỏe (resume consuming)
# 3. Không lãng phí CPU cho việc reject/requeue liên tục
# =============================================================================

MAX_CONCURRENT_SUBMISSIONS = int(os.getenv("MAX_CONCURRENT_SUBMISSIONS", "1"))
MAX_WORKERS_PER_SUBMISSION = int(os.getenv("MAX_WORKERS_PER_SUBMISSION", "4"))
HEALTH_CHECK_INTERVAL = 5  # Kiểm tra health mỗi 5 giây
MAX_RETRY_COUNT = int(os.getenv("MAX_RETRY_COUNT", "3"))  # Số lần retry tối đa

TESTCASE_STATUS_MESSAGE = {
    "Pending": "Testcase is pending execution",
    "Running": "Testcase is currently running",
    "Passed": "Testcase passed",
    "TimeLimitExceeded": "Testcase exceeded time limit",
    "MemoryLimitExceeded": "Testcase exceeded memory limit",
    "RuntimeError": "Testcase runtime error",
    "InternalError": "Internal error during testcase execution",
    "WrongAnswer": "Testcase produced wrong answer",
    "CompilationError": "Code compilation error",
    "Skipped": "Testcase was skipped"
}

class AdaptiveConsumer:
    def __init__(self):
        self.connection = None
        self.channel = None
        self.consumer_tag = None
        self.is_consuming = False
        self.should_stop = False
        
    def start(self):
        """Khởi động consumer với adaptive behavior"""
        rabbit_host = os.getenv("RABBITMQ_HOST", "localhost")
        rabbit_user = os.getenv("RABBITMQ_USER", "guest")
        rabbit_pass = os.getenv("RABBITMQ_PASS", "guest")
        
        # Retry logic: Chờ RabbitMQ sẵn sàng
        max_retries = 30
        retry_delay = 2
        
        for attempt in range(1, max_retries + 1):
            try:
                print(f"[*] Connecting to RabbitMQ (attempt {attempt}/{max_retries})...")
                credentials = pika.PlainCredentials(rabbit_user, rabbit_pass)
                self.connection = pika.BlockingConnection(
                    pika.ConnectionParameters(
                        host=rabbit_host,
                        credentials=credentials,
                        heartbeat=600,
                        blocked_connection_timeout=300
                    )
                )
                print(f"[✓] Connected to RabbitMQ at {rabbit_host}")
                break
            except pika.exceptions.AMQPConnectionError as e:
                if attempt == max_retries:
                    print(f"[ERROR] Failed to connect to RabbitMQ after {max_retries} attempts")
                    raise
                print(f"[WARNING] RabbitMQ not ready, retrying in {retry_delay}s... ({e})")
                time.sleep(retry_delay)
        
        self.channel = self.connection.channel()
        self.channel.queue_declare(queue="submission_queue", durable=True)
        self.channel.basic_qos(prefetch_count=MAX_CONCURRENT_SUBMISSIONS)
        
        print("[*] Adaptive Consumer started")
        
        # Thread để monitor health và điều chỉnh consumer
        health_monitor = threading.Thread(target=self._health_monitor_loop, daemon=True)
        health_monitor.start()
        
        # Bắt đầu consuming
        self._start_consuming()
        
        try:
            # Giữ connection alive
            while not self.should_stop:
                self.connection.process_data_events(time_limit=1)
        except KeyboardInterrupt:
            print("\n[*] Shutting down...")
            self.should_stop = True
        finally:
            self._stop_consuming()
            self.connection.close()
    
    def _start_consuming(self):
        """Bắt đầu nhận messages từ queue"""
        if not self.is_consuming:
            self.consumer_tag = self.channel.basic_consume(
                queue="submission_queue",
                on_message_callback=self._message_callback,
                auto_ack=False
            )
            self.is_consuming = True
            print("[✓] Consumer STARTED - Ready to process submissions")
    
    def _stop_consuming(self):
        """Tạm dừng nhận messages từ queue"""
        if self.is_consuming and self.consumer_tag:
            self.channel.basic_cancel(self.consumer_tag)
            self.is_consuming = False
            print("[⏸] Consumer PAUSED - System overloaded")
    
    def _health_monitor_loop(self):
        """
        Background thread để monitor health và điều chỉnh consumer.
        Chạy độc lập, không block message processing.
        """
        while not self.should_stop:
            try:
                can_accept, reason = should_accept_submission()
                
                if can_accept and not self.is_consuming:
                    # Hệ thống đã khỏe lại → BẬT consumer
                    print(f"[✓] System healthy again, resuming consumer...")
                    log_system_stats()
                    self._start_consuming()
                    
                elif not can_accept and self.is_consuming:
                    # Hệ thống quá tải → TẮT consumer
                    print(f"[⚠] System overloaded: {reason}")
                    log_system_stats()
                    self._stop_consuming()
                
            except Exception as e:
                print(f"[ERROR] Health monitor error: {e}")
            
            # Kiểm tra mỗi X giây
            time.sleep(HEALTH_CHECK_INTERVAL)

    def get_response(self, ch=None, properties=None, SubmissionId=None, ErrorCode=None, ErrorMessage=None) -> dict:
        """
        Tạo response chuẩn cho các trường hợp lỗi và tự động gửi nếu có thông tin channel.
        
        Args:
            ch: RabbitMQ channel (optional, nếu có sẽ tự động gửi response)
            properties: Message properties chứa reply_to và correlation_id
            SubmissionId: ID của submission
            ErrorCode: Mã lỗi (MAX_RETRY_EXCEEDED, INVALID_JSON, MISSING_REQUIRED_FIELDS, NO_TESTCASES)
            
        Returns:
            dict: Response object với ErrorCode và ErrorMessage tương ứng
        """
        response = {
            "SubmissionId": SubmissionId,
            "ErrorCode": ErrorCode,
            "ErrorMessage": ErrorMessage,
            "Results": []
        }
        if ErrorCode is not None and ErrorMessage is None:
            if ErrorCode == "MAX_RETRY_EXCEEDED":
                response["ErrorMessage"] =  f"Failed after {MAX_RETRY_COUNT} retry attempts. Please contact system administrator."
            elif ErrorCode == "INVALID_JSON":
                response["ErrorMessage"] = "Invalid JSON message format."
            elif ErrorCode == "MISSING_REQUIRED_FIELDS":
                response["ErrorMessage"] = "Missing required fields in submission."
            elif ErrorCode == "NO_TESTCASES":
                response["ErrorMessage"] = "No testcases provided."
            else:
                response["ErrorMessage"] = TESTCASE_STATUS_MESSAGE.get(ErrorCode, "Unknown error occurred.")

            # Tự động gửi response nếu có channel và reply_to
            if ch is not None and properties is not None and properties.reply_to:
                try:
                    ch.basic_publish(
                        exchange="",
                        routing_key=properties.reply_to,
                        body=json.dumps(response),
                        properties=pika.BasicProperties(correlation_id=properties.correlation_id)
                    )
                    print(f"[✓] Sent error response for submission {SubmissionId} ({ErrorCode})")
                    print("=" * 81)
                except Exception as e:
                    print(f"[ERROR] Failed to send error response: {e}")
            
        return response

    def _message_callback(self, ch, method, properties, body):
        """Xử lý message khi consumer đang active"""
        # Kiểm tra retry count để tránh vòng lặp vô tận
        retry_count = 0
        if properties.headers and 'x-retry-count' in properties.headers:
            retry_count = properties.headers['x-retry-count']
        
        if retry_count >= MAX_RETRY_COUNT:
            print(f"[ERROR] Message exceeded max retry count ({MAX_RETRY_COUNT})")
            print(f"[DEBUG] Raw message: {json.dumps(json.loads(body), indent=4)}")
                
            # Gửi error response về reply_to trước khi loại bỏ message
            try:
                data = json.loads(body)
                submission_id = data.get("SubmissionId", "unknown")
                self.get_response(
                    ch=ch,
                    properties=properties,
                    SubmissionId=submission_id,
                    ErrorCode="MAX_RETRY_EXCEEDED"
                )
            except Exception as e:
                print(f"[ERROR] Failed to send error response: {e}")
            
            # ACK để loại bỏ message sau khi đã gửi response
            ch.basic_ack(delivery_tag=method.delivery_tag)
            return
        
        try:
            data = json.loads(body)
        except json.JSONDecodeError as e:
            print(f"[ERROR] Invalid JSON message: {e}")
            print(f"[DEBUG] Raw message: {json.dumps(json.loads(body), indent=4)}")
            
            # Gửi error response về reply_to trước khi loại bỏ message
            if properties.reply_to:
                self.get_response(
                    ch=ch,
                    properties=properties,
                    SubmissionId="unknown",
                    ErrorCode="INVALID_JSON"
                )
            
            ch.basic_ack(delivery_tag=method.delivery_tag)  # ACK để không requeue message lỗi
            return
        
        submission_id = data.get("SubmissionId", "N/A")
        print(f"[*] Processing submission {submission_id}")
        print(f"[DEBUG] Message data: language={data.get('Language')}, testcases_count={len(data.get('Testcases', []))}")

        try:
            # Double-check health ngay trước khi xử lý
            can_accept, reason = should_accept_submission()
            if not can_accept:
                # Edge case: Hệ thống mới vừa quá tải trong chớp mắt
                print(f"[WARNING] Last-minute overload detected, requeuing {submission_id} (retry {retry_count + 1}/{MAX_RETRY_COUNT})")
                
                # Tăng retry count và requeue
                new_headers = properties.headers.copy() if properties.headers else {}
                new_headers['x-retry-count'] = retry_count + 1
                
                ch.basic_publish(
                    exchange='',
                    routing_key='submission_queue',
                    body=body,
                    properties=pika.BasicProperties(
                        delivery_mode=2,  # persistent
                        headers=new_headers,
                        reply_to=properties.reply_to,
                        correlation_id=properties.correlation_id
                    )
                )
                ch.basic_ack(delivery_tag=method.delivery_tag)  # ACK message cũ
                # Health monitor sẽ tự động stop consumer ở lần check tiếp theo
                return

            # Xử lý submission
            submission_id = data.get("SubmissionId")
            language = data.get("Language")
            code = data.get("Code")
            timelimit = data.get("TimeLimit", 3)
            memorylimit = data.get("MemoryLimit", 256)
            testcases = data.get("Testcases", [])

            # Kiểm tra dữ liệu hợp lệ
            if not submission_id:
                print(f"[ERROR] Invalid submission: missing submissionId")
                self.get_response(
                    ch=ch,
                    properties=properties,
                    SubmissionId=submission_id,
                    ErrorCode="MISSING_REQUIRED_FIELDS"
                )
                ch.basic_ack(delivery_tag=method.delivery_tag)
                return
            
            if not language or not code:
                print(f"[ERROR] Invalid submission {submission_id}: missing language or code")
                self.get_response(
                    ch=ch,
                    properties=properties,
                    SubmissionId=submission_id,
                    ErrorCode="MISSING_REQUIRED_FIELDS"
                )
                ch.basic_ack(delivery_tag=method.delivery_tag)
                return
            
            if not testcases or len(testcases) == 0:
                print(f"[WARNING] Submission {submission_id} has no testcases")
                self.get_response(
                    ch=ch,
                    properties=properties,
                    SubmissionId=submission_id,
                    ErrorCode="NO_TESTCASES"
                )
                ch.basic_ack(delivery_tag=method.delivery_tag)
                return
            
            # Chạy testcase đầu tiên trước để phát hiện lỗi sớm
            results = []

            # Prepare first testcase in the format expected by executor_isolate
            first_testcase = testcases[0]
            first_tc_payload = {
                "testcaseId": first_testcase.get("TestCaseId"),
                "inputRef": first_testcase.get("InputRef"),
                "outputRef": first_testcase.get("OutputRef")
            }

            try:
                isolate_results = execute_in_sandbox(
                    # testcaseId=first_tc_payload.get("testcaseId"),
                    language=language,
                    code=code,
                    testcases=[first_tc_payload],
                    timelimit=timelimit,
                    memorylimit=memorylimit
                )

                # executor_isolate returns a list of results (one per testcase)
                if not isinstance(isolate_results, list) or len(isolate_results) == 0:
                    # Treat as internal error
                    print(f"[ERROR] Invalid result from isolate executor for {submission_id}")
                    self.get_response(ch=ch, properties=properties, SubmissionId=submission_id, ErrorCode="InternalError", ErrorMessage="Invalid result from isolate executor")
                    ch.basic_ack(delivery_tag=method.delivery_tag)
                    return

                first_result = isolate_results[0]
                results.append(first_result)

                # Kiểm tra nếu testcase đầu tiên gặp lỗi nghiêm trọng
                first_status = first_result.get("Status")
                if first_status in (TESTCASE_STATUS.CompilationError, TESTCASE_STATUS.InternalError, TESTCASE_STATUS.MemoryLimitExceeded, TESTCASE_STATUS.TimeLimitExceeded):
                    self.get_response(
                        ch=ch,
                        properties=properties,
                        SubmissionId=submission_id,
                        ErrorCode=first_status
                    )
                    print(f"[ERROR] First testcase resulted in {first_status} for {submission_id}, skipping remaining testcases")
                    ch.basic_ack(delivery_tag=method.delivery_tag)
                    return
                else:
                    # Testcase đầu tiên thành công hoặc chỉ fail thông thường (WrongAnswer, TimeLimitExceeded, MemoryLimitExceeded)
                    # Tiếp tục chạy các testcase còn lại song song
                    
                    # print(f"[✓] First testcase finished, running remaining {len(remaining)} testcases for {submission_id}")
                    remaining_tcs = []
                    for tc in testcases[1:]:
                        
                        remaining_tcs.append({
                            "testcaseId": tc.get("TestCaseId"),
                            "inputRef": tc.get("InputRef"),
                            "outputRef": tc.get("OutputRef")
                        })

                    results.extend(
                    execute_in_sandbox(
                        language=language,
                        code=code,
                        testcases=remaining_tcs,
                        timelimit=timelimit,
                        memorylimit=memorylimit
                    ))

            except Exception as e:
                # Lỗi khi chạy testcase đầu tiên
                print(f"[ERROR] Exception on first testcase for {submission_id}: {e}")
                self.get_response(
                    ch=ch,
                    properties=properties,
                    SubmissionId=submission_id,
                    ErrorCode=TESTCASE_STATUS.InternalError
                )
                ch.basic_ack(delivery_tag=method.delivery_tag)
                return
        except Exception as e:
            print(f"[ERROR] Failed to process submission {submission_id}: {e}")
            print(f"[DEBUG] Retry count: {retry_count}/{MAX_RETRY_COUNT}")
            
            # Tăng retry count và requeue với header mới
            new_headers = properties.headers.copy() if properties.headers else {}
            new_headers['x-retry-count'] = retry_count + 1
            
            ch.basic_publish(
                exchange='',
                routing_key='submission_queue',
                body=body,
                properties=pika.BasicProperties(
                    delivery_mode=2,  # persistent
                    headers=new_headers,
                    reply_to=properties.reply_to,
                    correlation_id=properties.correlation_id
                )
            )
            ch.basic_ack(delivery_tag=method.delivery_tag)  # ACK message cũ
            return
    
        # Chuẩn bị response
        response = {
            "SubmissionId": submission_id,
            "ErrorCode": TESTCASE_STATUS.Passed,
            "ErrorMessage": "",
            "Results": results
        }
        # Nếu trong results không có status nào khác ngoài passed, trả về passed
        passed = True
        for result in results:
            if result.get("status") != TESTCASE_STATUS.Passed:
                print(f"[ERROR] Test case {result.get('testcaseId')} failed: {result.get('status')}")
                passed = False
                break
            else:
                print(f"[✓] Test case {result.get('testcaseId')} passed")
        if not passed:
            response["ErrorCode"] = SubmissionStatus.FAILED
            response["ErrorMessage"] = "Some test cases failed"

        # Sau khi thu thập hết kết quả, gửi response thành công
        if properties.reply_to:
            ch.basic_publish(
                exchange="",
                routing_key=properties.reply_to,
                body=json.dumps(response),
                properties=pika.BasicProperties(correlation_id=properties.correlation_id)
            )
            # print(json.dumps(response))
            print(f"[✓] Sent success response for submission {submission_id} with {len(results)} results")
            print("=" * 81)

        ch.basic_ack(delivery_tag=method.delivery_tag)

def main():
    """Entry point cho adaptive consumer"""
    consumer = AdaptiveConsumer()
    consumer.start()

if __name__ == "__main__":
    main()