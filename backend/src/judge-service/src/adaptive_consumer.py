"""
Async Adaptive Consumer - Xử lý nhiều messages song song
Dùng aio-pika để kết nối RabbitMQ và gọi MessageHandler (an toàn với isolate)
"""
import asyncio
import json
import os
import aio_pika
from message_handler import MessageHandler  # ✅ import đúng file

MAX_CONCURRENT_SUBMISSIONS = int(os.getenv("MAX_CONCURRENT_SUBMISSIONS", "4"))

class AsyncAdaptiveConsumer:
    def __init__(self):
        self.connection = None
        self.channel = None
        self.should_stop = False

    async def start(self):
        """Khởi động async consumer"""
        rabbit_host = os.getenv("RABBITMQ_HOST", "localhost")
        rabbit_user = os.getenv("RABBITMQ_USER", "guest")
        rabbit_pass = os.getenv("RABBITMQ_PASS", "guest")
        submission_queue_name = os.getenv("SUBMISSION_QUEUE", "submission_queue")

        # Retry connect
        max_retries = 30
        retry_delay = 2
        for attempt in range(1, max_retries + 1):
            try:
                print(f"[*] Connecting to RabbitMQ ({attempt}/{max_retries})...")
                self.connection = await aio_pika.connect_robust(
                    f"amqp://{rabbit_user}:{rabbit_pass}@{rabbit_host}/",
                    heartbeat=600,
                )
                print(f"[✓] Connected to RabbitMQ at {rabbit_host}")
                break
            except Exception as e:
                if attempt == max_retries:
                    raise RuntimeError(f"[FATAL] Could not connect to RabbitMQ: {e}")
                print(f"[WARNING] Retry in {retry_delay}s... ({e})")
                await asyncio.sleep(retry_delay)

        # Tạo channel và declare queue
        self.channel = await self.connection.channel()
        await self.channel.set_qos(prefetch_count=MAX_CONCURRENT_SUBMISSIONS)
        submission_queue = await self.channel.declare_queue(submission_queue_name, durable=True)
        await self.channel.declare_queue("result_queue", durable=True)

        print(f"[✓] Consumer ready - processing up to {MAX_CONCURRENT_SUBMISSIONS} submissions concurrently")

        # Bắt đầu consume
        await submission_queue.consume(self._message_callback)
        print("[✓] Waiting for submissions... Press CTRL+C to stop.")
        try:
            await asyncio.Future()  # chạy vô hạn
        except asyncio.CancelledError:
            print("\n[*] Shutting down consumer gracefully...")
        finally:
            await self._cleanup()

    async def _message_callback(self, message: aio_pika.IncomingMessage):
        """Xử lý từng message"""
        retry_count = 0
        if message.headers:
            retry_count = message.headers.get('x-retry-count', 0)

        try:
            result = await MessageHandler.handle_message(
                message.body,
                message,
                retry_count
            )

            # 1️⃣ Requeue nếu cần
            if result["should_requeue"] and result["new_body"] and result["new_headers"]:
                await self.channel.default_exchange.publish(
                    aio_pika.Message(
                        body=result["new_body"],
                        delivery_mode=aio_pika.DeliveryMode.PERSISTENT,
                        headers=result["new_headers"],
                        reply_to=message.reply_to,
                        correlation_id=message.correlation_id
                    ),
                    routing_key="submission_queue"
                )
                print(f"[↻] Requeued message for retry (count={result['new_headers'].get('x-retry-count', 1)})")

            # 2️⃣ Gửi kết quả về queue reply_to (nếu có)
            if result["response"] and message.reply_to:
                await self._send_response(
                    result["response"],
                    message.reply_to,
                    message.correlation_id
                )

            # 3️⃣ ACK message
            if result["should_ack"]:
                await message.ack()
                print(f"[✓] ACK message for submission done.")
            else:
                await message.nack(requeue=False)
                print(f"[✗] NACK message (not requeued).")

        except Exception as e:
            print(f"[ERROR] Fatal exception in message callback: {e}")
            await message.nack(requeue=True)

    async def _send_response(self, response_body, reply_queue, correlation_id):
        """Gửi kết quả về lại server qua reply_to"""
        try:
            await self.channel.default_exchange.publish(
                aio_pika.Message(
                    body=json.dumps(response_body).encode(),
                    delivery_mode=aio_pika.DeliveryMode.PERSISTENT,
                    correlation_id=correlation_id
                ),
                routing_key=reply_queue
            )
            print(f"[→] Sent response to {reply_queue} (CID={correlation_id})")
        except Exception as e:
            print(f"[ERROR] Failed to send response: {e}")

    async def _cleanup(self):
        """Đóng kết nối gọn gàng"""
        if self.channel and not self.channel.is_closed:
            await self.channel.close()
        if self.connection and not self.connection.is_closed:
            await self.connection.close()
        print("[✓] Consumer stopped cleanly.")


# # ------------------------------------------
# # MAIN ENTRY
# # ------------------------------------------
# if __name__ == "__main__":
#     consumer = AsyncAdaptiveConsumer()
#     try:
#         asyncio.run(consumer.start())
#     except KeyboardInterrupt:
#         print("\n[*] Stopping consumer manually...")
