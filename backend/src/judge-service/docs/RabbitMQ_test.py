import pika
import json

connection = pika.BlockingConnection(pika.ConnectionParameters('localhost'))
channel = connection.channel()

method_frame, header_frame, body = channel.basic_get('result_queue', auto_ack=False)
if method_frame:
    print(json.dumps(json.loads(body), indent=4))
    channel.basic_nack(method_frame.delivery_tag, requeue=True)
else:
    print("No message in queue.")
