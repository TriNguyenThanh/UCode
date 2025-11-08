# Import thư viện pika để làm việc với RabbitMQ message queue
import pika
import json
import os
import time
from message_handler import MessageHandler


# =============================================================================
# ADAPTIVE CONSUMER - Consumer đơn giản không kiểm tra quá tải
# =============================================================================
# Consumer sẽ:
# 1. Luôn chạy và xử lý message
# 2. Không tạm dừng khi hệ thống quá tải
# 3. Đơn giản và ổn định
# =============================================================================

MAX_CONCURRENT_SUBMISSIONS = int(os.getenv("MAX_CONCURRENT_SUBMISSIONS", "1"))

class AdaptiveConsumer:
    def __init__(self):
        self.connection = None
        self.channel = None
        self.should_stop = False
        
    def start(self):
        """Khởi động consumer đơn giản, không kiểm tra quá tải"""
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
        
        # Declare cả submission_queue và result_queue
        self.channel.queue_declare(queue="submission_queue", durable=True)
        self.channel.queue_declare(queue="result_queue", durable=True)
        
        self.channel.basic_qos(prefetch_count=MAX_CONCURRENT_SUBMISSIONS)
        
        # Bắt đầu consuming
        self.channel.basic_consume(
            queue="submission_queue",
            on_message_callback=self._message_callback,
            auto_ack=False
        )
        
        print("[✓] Consumer started - Ready to process submissions")
        
        try:
            # Bắt đầu consuming messages
            self.channel.start_consuming()
        except KeyboardInterrupt:
            print("\n[*] Shutting down...")
            self.should_stop = True
            self.channel.stop_consuming()
        except Exception as e:
            print(f"[ERROR] Connection error: {e}")
            self.should_stop = True
        finally:
            # Only close connection if it's still open
            if self.connection and self.connection.is_open:
                try:
                    self.connection.close()
                except Exception as e:
                    print(f"[WARNING] Error closing connection: {e}")

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
                print(f"[→] Sending response to queue: {properties.reply_to}")
                print(f"    Correlation ID: {properties.correlation_id}")
                print(f"    Response: {json.dumps(result['response'], indent=2)}")
                
                ch.basic_publish(
                    exchange="",
                    routing_key=properties.reply_to,
                    body=json.dumps(result["response"]),
                    properties=pika.BasicProperties(
                        correlation_id=properties.correlation_id,
                        delivery_mode=2  # persistent message
                    )
                )
                print(f"[✓] Response sent successfully to {properties.reply_to}")
            except Exception as e:
                print(f"[ERROR] Failed to send response to server: {e}")
                import traceback
                traceback.print_exc()

def main():
    consumer = AdaptiveConsumer()
    consumer.start()

if __name__ == "__main__":
    main()