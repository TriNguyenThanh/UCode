# Import thư viện pika để làm việc với RabbitMQ message queue
import pika
import json
import os
import time
import threading
from app.health_check import should_accept_submission, log_system_stats
from app.message_handler import MessageHandler


# =============================================================================
# ADAPTIVE CONSUMER - Tự động điều chỉnh theo tải hệ thống
# =============================================================================
# Thay vì reject và requeue, consumer sẽ:
# 1. TẠM DỪNG khi hệ thống quá tải (stop consuming)
# 2. TỰ ĐỘNG BẬT LẠI khi hệ thống khỏe (resume consuming)
# 3. Không lãng phí CPU cho việc reject/requeue liên tục
# =============================================================================

MAX_CONCURRENT_SUBMISSIONS = int(os.getenv("MAX_CONCURRENT_SUBMISSIONS", "1"))
HEALTH_CHECK_INTERVAL = 5  # Kiểm tra health mỗi 5 giây

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
            
            # Kiểm tra health mỗi X giây
            time.sleep(HEALTH_CHECK_INTERVAL)

    def _message_callback(self, ch, method, properties, body):
        # Lấy retry count từ headers
        retry_count = 0
        if properties.headers and 'x-retry-count' in properties.headers:
            retry_count = properties.headers['x-retry-count']
        
        # Đưa message cho MessageHandler xử lý
        result = MessageHandler.handle_message(body, properties, retry_count)
        
        # Xử lý kết quả từ MessageHandler
        # 1. ACK message
        if result["should_ack"]:
            ch.basic_ack(delivery_tag=method.delivery_tag)
        
        # 2. Requeue (với retry count tăng)
        if result["should_requeue"] and result["new_body"] and result["new_headers"]:
            ch.basic_publish(
                exchange='',
                routing_key='submission_queue',
                body=result["new_body"],
                properties=pika.BasicProperties(
                    delivery_mode=2,  # persistent
                    headers=result["new_headers"],
                    reply_to=properties.reply_to,
                    correlation_id=properties.correlation_id
                )
            )
        
        # 3. Gửi response về server
        if result["response"] and properties.reply_to:
            try:
                ch.basic_publish(
                    exchange="",
                    routing_key=properties.reply_to,
                    body=json.dumps(result["response"]),
                    properties=pika.BasicProperties(
                        correlation_id=properties.correlation_id
                    )
                )
            except Exception as e:
                print(f"[ERROR] Failed to send response to server: {e}")

def main():
    consumer = AdaptiveConsumer()
    consumer.start()

if __name__ == "__main__":
    main()