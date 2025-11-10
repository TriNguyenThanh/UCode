# Async Executor Migration Guide

## Overview

I've created async versions of your executor that can process **multiple testcases in parallel**, maximizing the benefit of your `prefetch_count=4` setting.

## What Changed?

### 1. **New Files Created**

- `executor_isolate_async.py` - Async version of executor (runs testcases in parallel)
- `message_handler_async.py` - Async version of message handler
- `adaptive_consumer_async.py` - Async consumer using `aio-pika`

### 2. **Key Improvements**

#### **Before (Synchronous)**
```python
# prefetch_count=4 but processes ONE message at a time
self.channel.basic_qos(prefetch_count=4)

# Message callback processes synchronously
def _message_callback(self, ch, method, properties, body):
    result = MessageHandler.handle_message(body, properties, retry_count)
    # One message → One testcase → Sequential processing
```

#### **After (Asynchronous)**
```python
# prefetch_count=4 and processes UP TO 4 messages concurrently
await self.channel.set_qos(prefetch_count=4)

# Message callback processes asynchronously
async def _message_callback(self, message: aio_pika.IncomingMessage):
    result = await MessageHandler.handle_message(message.body, message, retry_count)
    # 4 messages can run in parallel
    # Each message's testcases ALSO run in parallel (configurable)
```

## Performance Gains

### **Scenario: 4 submissions, each with 10 testcases**

**Synchronous (Current)**
```
Total time = 4 submissions × 10 testcases × 1 second = 40 seconds
```

**Asynchronous (New)**
```
With MAX_CONCURRENT_SUBMISSIONS=4 and MAX_PARALLEL_TESTCASES=4:
Total time = 10 testcases × 1 second = 10 seconds (4x faster!)
```

## Configuration

Add to your environment variables:

```bash
# Maximum submissions processing at the same time
MAX_CONCURRENT_SUBMISSIONS=4

# Maximum testcases running in parallel PER submission
MAX_PARALLEL_TESTCASES=4
```

**Resource calculation:**
- Max concurrent isolate boxes = `MAX_CONCURRENT_SUBMISSIONS × MAX_PARALLEL_TESTCASES`
- Example: 4 × 4 = **16 isolate boxes** running simultaneously

## How It Works

### Parallel Execution Flow

```
RabbitMQ Queue: [S1] [S2] [S3] [S4] [S5] [S6]
                 ↓    ↓    ↓    ↓
Consumer fetches 4 submissions in parallel:
  
  S1 → [TC1, TC2, TC3, TC4] ← All running in parallel
  S2 → [TC1, TC2, TC3, TC4] ← All running in parallel  
  S3 → [TC1, TC2, TC3, TC4] ← All running in parallel
  S4 → [TC1, TC2, TC3, TC4] ← All running in parallel

Total: 16 testcases running simultaneously!

When S1 completes:
  S5 → [TC1, TC2, TC3, TC4] ← Starts immediately
```

### Isolate Box Management

Each testcase gets its own unique files to avoid conflicts:

```python
# Unique files per testcase
input_file = f"{box_path}/input_{index_no}.txt"
output_file = f"{box_path}/output_{index_no}.txt"
error_file = f"{box_path}/error_{index_no}.txt"
meta_file = f"{box_path}/meta_{index_no}.txt"
```

## Migration Steps

### Option 1: Quick Switch (Recommended for Testing)

```bash
# Install async dependencies
pip install aio-pika

# Run async consumer instead of sync
python app/adaptive_consumer_async.py
```

### Option 2: Update Dockerfile

Update your `Dockerfile.dev`:

```dockerfile
# Change the CMD
CMD ["python", "app/adaptive_consumer_async.py"]
```

### Option 3: Gradual Migration

Run both consumers side-by-side:
1. Keep sync consumer for stability
2. Start async consumer with lower `MAX_CONCURRENT_SUBMISSIONS`
3. Monitor performance
4. Gradually increase async workers
5. Deprecate sync consumer

## Testing

### Test async executor directly:

```python
import asyncio
from executor_isolate_async import execute_in_sandbox

async def test():
    testcases = [
        {
            "TestCaseId": "test-1",
            "IndexNo": 0,
            "InputRef": "1 2\n",
            "OutputRef": "3\n"
        },
        {
            "TestCaseId": "test-2",
            "IndexNo": 1,
            "InputRef": "5 3\n",
            "OutputRef": "8\n"
        }
    ]
    
    code = """
a, b = map(int, input().split())
print(a + b)
"""
    
    results = await execute_in_sandbox("python", code, testcases)
    print(results)

asyncio.run(test())
```

## Monitoring

Watch for these improvements:

1. **Throughput**: Messages processed per minute should increase significantly
2. **Latency**: Time from message received to response sent should decrease
3. **Resource Usage**: CPU usage should be higher (utilizing multiple cores)

### Logs to monitor:

```bash
# You'll see parallel execution logs:
[DEBUG] Running 10 testcases in parallel (max 4 concurrent)
[✓] Testcase #0 (test-1) passed
[✓] Testcase #2 (test-3) passed  ← Running simultaneously
[✓] Testcase #1 (test-2) passed  ← Not sequential!
```

## Rollback Plan

If you encounter issues:

1. **Stop async consumer**
   ```bash
   # Kill async process
   pkill -f adaptive_consumer_async
   ```

2. **Start sync consumer**
   ```bash
   python app/adaptive_consumer.py
   ```

3. **Your original files are untouched** - `executor_isolate.py`, `message_handler.py`, and `adaptive_consumer.py` remain unchanged

## Potential Issues & Solutions

### Issue: Too many isolate boxes

**Symptom**: "Failed to initialize isolate box" errors

**Solution**: Reduce `MAX_PARALLEL_TESTCASES`
```bash
export MAX_PARALLEL_TESTCASES=2  # Instead of 4
```

### Issue: High memory usage

**Symptom**: System runs out of memory

**Solution**: 
1. Reduce concurrent submissions
2. Lower memory limits per testcase
3. Monitor with `htop` or `ps aux`

### Issue: Box ID conflicts

**Symptom**: Random failures with box cleanup

**Solution**: Already handled! Each testcase uses unique file names within the same box

## Advantages of Async Version

✅ **4-16x faster** for submissions with multiple testcases  
✅ **Better resource utilization** (uses all CPU cores)  
✅ **Same reliability** (error handling preserved)  
✅ **Backward compatible** (same message format)  
✅ **Scalable** (adjust concurrency with env vars)  

## Questions?

- **Q: Do I need to modify my C# backend?**  
  A: No! The message format is identical.

- **Q: Can I run both versions?**  
  A: Yes! They can coexist on different consumers.

- **Q: What if compilation fails?**  
  A: Same behavior - all testcases get compilation error status.

- **Q: Is it thread-safe?**  
  A: Yes! Uses asyncio (single-threaded concurrency) + unique file paths.

---

**Recommendation**: Start with `MAX_CONCURRENT_SUBMISSIONS=2` and `MAX_PARALLEL_TESTCASES=2`, then gradually increase based on your server capacity.
