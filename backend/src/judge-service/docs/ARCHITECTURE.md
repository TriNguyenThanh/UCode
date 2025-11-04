# Architecture Overview

## Phân chia Trách nhiệm (Separation of Concerns)

### 1. `adaptive_consumer.py` - RabbitMQ Consumer Layer
**Vai trò**: Quản lý kết nối RabbitMQ và routing messages

**Nhiệm vụ**:
- ✅ Kết nối với RabbitMQ server
- ✅ Quản lý health monitoring (pause/resume consumer)
- ✅ Nhận message từ queue
- ✅ Gửi message đến MessageHandler
- ✅ Nhận kết quả từ MessageHandler
- ✅ ACK/NACK/Requeue messages
- ✅ Gửi response về server qua RabbitMQ

**Không làm**:
- ❌ Parse/validate message data
- ❌ Xử lý business logic
- ❌ Chạy code submissions
- ❌ Tạo response messages

### 2. `message_handler.py` - Business Logic Layer
**Vai trò**: Xử lý toàn bộ logic nghiệp vụ của submission

**Nhiệm vụ**:
- ✅ Parse và validate JSON message
- ✅ Kiểm tra các trường bắt buộc
- ✅ Kiểm tra health last-minute
- ✅ Xử lý submission (chạy testcases)
- ✅ Tạo error/success response messages
- ✅ Quyết định ACK/Requeue strategy

**Interface chính**: `handle_message(body, properties, retry_count)`

**Trả về**:
```python
{
    "should_ack": bool,         # Consumer nên ACK?
    "should_requeue": bool,     # Consumer nên requeue?
    "response": dict|None,      # Response message (nếu có)
    "new_body": bytes|None,     # Body mới để requeue
    "new_headers": dict|None    # Headers mới (có retry count)
}
```

### 3. `executor_isolate.py` - Execution Layer
**Vai trò**: Thực thi code trong sandbox

**Nhiệm vụ**:
- ✅ Compile code
- ✅ Run code với isolate sandbox
- ✅ So sánh output với expected
- ✅ Trả về kết quả execution

---

## Message Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                         RabbitMQ Queue                          │
└──────────────────────────────┬──────────────────────────────────┘
                               │
                               ▼
┌─────────────────────────────────────────────────────────────────┐
│              adaptive_consumer.py (Consumer Layer)              │
│                                                                 │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ _message_callback(ch, method, properties, body)          │  │
│  │   1. Extract retry_count from headers                    │  │
│  │   2. Call MessageHandler.handle_message()       ────────────┼──┐
│  │   3. Process result:                                     │  │  │
│  │      - ACK if should_ack                                 │  │  │
│  │      - Requeue if should_requeue                         │  │  │
│  │      - Send response to server if available              │  │  │
│  └──────────────────────────────────────────────────────────┘  │  │
└─────────────────────────────────────────────────────────────────┘  │
                                                                     │
                                                                     │
┌────────────────────────────────────────────────────────────────────┘
│
▼
┌─────────────────────────────────────────────────────────────────┐
│            message_handler.py (Business Logic Layer)            │
│                                                                 │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ handle_message(body, properties, retry_count)            │  │
│  │   1. Check retry_count (max exceeded?)                   │  │
│  │   2. Parse JSON (valid?)                                 │  │
│  │   3. Check health (should requeue?)                      │  │
│  │   4. Validate fields (complete?)                         │  │
│  │   5. Call _process_submission()              ──────────────┼──┐
│  │   6. Create error/success response                       │  │  │
│  │   7. Return result dict                                  │  │  │
│  └──────────────────────────────────────────────────────────┘  │  │
└─────────────────────────────────────────────────────────────────┘  │
                                                                     │
                                                                     │
┌────────────────────────────────────────────────────────────────────┘
│
▼
┌─────────────────────────────────────────────────────────────────┐
│             executor_isolate.py (Execution Layer)               │
│                                                                 │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ execute_in_sandbox(language, code, testcases, ...)      │  │
│  │   1. Compile code                                        │  │
│  │   2. Run testcases in isolate sandbox                    │  │
│  │   3. Compare outputs                                     │  │
│  │   4. Return results                                      │  │
│  └──────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

---

## Lợi ích của Kiến trúc này

### 1. **Tách biệt Concerns rõ ràng**
- Consumer chỉ lo RabbitMQ
- Handler chỉ lo business logic
- Executor chỉ lo chạy code

### 2. **Dễ Test**
- Test Handler độc lập không cần RabbitMQ
- Test Executor độc lập không cần queue
- Mock dễ dàng

### 3. **Dễ Maintain**
- Thay đổi validation logic → chỉ sửa Handler
- Thay đổi queue strategy → chỉ sửa Consumer
- Thay đổi sandbox → chỉ sửa Executor

### 4. **Dễ Mở rộng**
- Thêm message type mới → thêm handler method
- Thêm queue mới → tái sử dụng Handler
- Thêm ngôn ngữ mới → chỉ sửa Executor

### 5. **Stateless Handler**
- Handler không giữ state
- Có thể scale horizontally
- Pure functions, deterministic

---

## Error Handling Strategy

### Retry-able Errors (Requeue)
- System overload (health check failed)
- Temporary executor errors
- **Action**: Requeue với retry_count++

### Non-retry-able Errors (ACK + Error Response)
- Invalid JSON
- Missing required fields
- No testcases
- Max retry exceeded
- Compilation error
- **Action**: ACK message, gửi error response về server

### Success
- All testcases passed/failed normally
- **Action**: ACK message, gửi success response về server
