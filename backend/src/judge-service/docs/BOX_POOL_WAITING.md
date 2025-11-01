# Box Pool Waiting Behavior

## 🔄 Cơ chế chờ (Waiting Mechanism)

Khi tất cả boxes đều đang bận, **Box Pool sẽ TỰ ĐỘNG CHỜ** cho đến khi có box trống.

### Cách hoạt động:

```python
box_id = box_pool.acquire_box(timeout=None)  # None = chờ vô hạn
```

**Flow:**

```
Request 1 ─┐
Request 2 ─┤
Request 3 ─┤
Request 4 ─┤  ┌─────────────────┐
Request 5 ─┼─→│   Box Pool      │
Request 6 ─┤  │  (10 boxes)     │
Request 7 ─┤  └─────────────────┘
Request 8 ─┤         │
Request 9 ─┤         ├─→ Box 0 (busy) ─┐
Request10 ─┤         ├─→ Box 1 (busy) ─┤
Request11 ─┤ WAIT    ├─→ Box 2 (busy) ─┤  Wave 1
Request12 ─┤ WAIT    ├─→ ...          ─┤  (10 concurrent)
Request13 ─┤ WAIT    └─→ Box 9 (busy) ─┘
Request14 ─┤ WAIT              │
Request15 ─┘ WAIT              ↓
              │        Boxes completed
              │               │
              └───────────────┤
                              ↓
                        Box 0 (free) ─┐
                        Box 1 (free) ─┤
                        Box 2 (free) ─┤  Wave 2
                        ...          ─┤  (5 concurrent)
                        Box 4 (free) ─┘
```

## 📊 Scenarios

### Scenario 1: Ít testcases hơn số boxes

**Setup:**
- Pool size: 10 boxes
- Testcases: 5

**Kết quả:**
```
Time: 0s ──────────────────────────> 0.5s
Box 0: [██████ TC-1 ██████]
Box 1: [██████ TC-2 ██████]
Box 2: [██████ TC-3 ██████]
Box 3: [██████ TC-4 ██████]
Box 4: [██████ TC-5 ██████]
Box 5: [idle]
Box 6: [idle]
Box 7: [idle]
Box 8: [idle]
Box 9: [idle]

✅ Không có waiting
✅ Duration: ~500ms (1 wave)
```

### Scenario 2: Nhiều testcases hơn số boxes

**Setup:**
- Pool size: 10 boxes
- Testcases: 25

**Kết quả:**
```
Time: 0s ───────> 0.5s ───────> 1.0s ───────> 1.5s
       Wave 1         Wave 2         Wave 3
Box 0: [█ TC-1 █][█ TC-11 █][█ TC-21 █]
Box 1: [█ TC-2 █][█ TC-12 █][█ TC-22 █]
Box 2: [█ TC-3 █][█ TC-13 █][█ TC-23 █]
Box 3: [█ TC-4 █][█ TC-14 █][█ TC-24 █]
Box 4: [█ TC-5 █][█ TC-15 █][█ TC-25 █]
Box 5: [█ TC-6 █][█ TC-16 █][idle]
Box 6: [█ TC-7 █][█ TC-17 █][idle]
Box 7: [█ TC-8 █][█ TC-18 █][idle]
Box 8: [█ TC-9 █][█ TC-19 █][idle]
Box 9: [█ TC-10█][█ TC-20 █][idle]

⏳ TC-11 đến TC-20 chờ Wave 1 hoàn thành
⏳ TC-21 đến TC-25 chờ Wave 2 hoàn thành
✅ Duration: ~1.5s (3 waves)
```

### Scenario 3: Testcases có thời gian chạy khác nhau

**Setup:**
- Pool size: 3 boxes
- Testcases: 6 (thời gian khác nhau)

**Kết quả:**
```
Time: 0s ────────────────────────────────> 2.5s

Box 0: [████ TC-1 (fast) ████][████ TC-4 (fast) ████]
Box 1: [████████████ TC-2 (slow) ████████████]
Box 2: [██████ TC-3 (medium) ██████][██ TC-5 █]

                TC-6 waiting ────────────┐
                                         ↓
                                    [█ TC-6 █]

📝 Box được release ngay khi testcase hoàn thành
📝 Testcase đang chờ sẽ lấy box đầu tiên khả dụng
```

## 🎯 Logging Examples

### Khi có box trống ngay

```
[DEBUG] Acquired box 3 (no wait)
[DEBUG] Using box 3 for testcase tc-001
[DEBUG] Executing in box 3 (timeout=3s, mem=256MB)
[DEBUG] Released box 3 back to pool
```

### Khi phải chờ box

```
[INFO] All 10 boxes are busy, waiting for available box...
[INFO] Acquired box 7 after waiting 1.23s
[DEBUG] Using box 7 for testcase tc-015
[DEBUG] Executing in box 7 (timeout=3s, mem=256MB)
[DEBUG] Released box 7 back to pool
```

## 📈 Pool Status Monitoring

### API để monitor pool

```python
from app.executor_isolate_pool import get_pool_status

status = get_pool_status()
print(status)
```

**Output:**
```json
{
  "total_boxes": 10,
  "available_boxes": 7,
  "busy_boxes": 3,
  "utilization_percent": 30.0
}
```

### Real-time monitoring

```python
import time
from app.executor_isolate_pool import get_pool_status

while True:
    status = get_pool_status()
    print(f"Available: {status['available_boxes']}/{status['total_boxes']} "
          f"(Utilization: {status['utilization_percent']}%)")
    time.sleep(1)
```

**Output:**
```
Available: 10/10 (Utilization: 0%)
Available: 3/10 (Utilization: 70%)
Available: 0/10 (Utilization: 100%)  ← All busy
Available: 2/10 (Utilization: 80%)
Available: 5/10 (Utilization: 50%)
Available: 10/10 (Utilization: 0%)   ← All free
```

## ⚙️ Timeout Configuration

### Chờ vô hạn (Default - Recommended)

```python
box_id = box_pool.acquire_box(timeout=None)
# ✅ Sẽ chờ cho đến khi có box trống
# ✅ Không bao giờ trả về None
# ✅ Phù hợp cho production
```

### Có timeout

```python
box_id = box_pool.acquire_box(timeout=30.0)
# ⏱️ Chờ tối đa 30 giây
# ❌ Trả về None nếu timeout
# ⚠️ Cần xử lý trường hợp None
```

### Không chờ (immediate fail)

```python
box_id = box_pool.acquire_box(timeout=0)
# 🚫 Không chờ, fail ngay nếu không có box
# ❌ Trả về None nếu không có box trống
# ⚠️ Không khuyến khích
```

## 🔧 Tuning Pool Size

### Cách tính pool size tối ưu

**Formula:**
```
pool_size = avg_concurrent_submissions × avg_testcases_per_submission × 1.2
```

**Ví dụ:**
- Trung bình: 5 submissions đồng thời
- Mỗi submission: 10 testcases
- Buffer: 20%
- **Pool size = 5 × 10 × 1.2 = 60 boxes**

### Configuration

```yaml
# docker-compose.yml
execution-service:
  environment:
    - MAX_CONCURRENT_EXECUTIONS=60
```

## 🎭 Edge Cases

### Case 1: Deadlock Prevention

**Vấn đề:** Thread A giữ box 0, đợi box 1. Thread B giữ box 1, đợi box 0.

**Giải pháp:** Isolate Pool KHÔNG có vấn đề này vì:
- Mỗi thread chỉ acquire 1 box
- Execute xong → release ngay
- Không có nested acquire

### Case 2: Box Pool Exhaustion

**Triệu chứng:**
```
[INFO] All 10 boxes are busy, waiting for available box...
[INFO] All 10 boxes are busy, waiting for available box...
[INFO] All 10 boxes are busy, waiting for available box...
... (nhiều requests đang chờ)
```

**Giải pháp:**
1. Tăng pool size
2. Optimize code execution time
3. Add request queue limit

### Case 3: Long-running Testcase

**Vấn đề:** 1 testcase chạy rất lâu, chiếm box.

**Impact:**
- Các testcase khác phải chờ
- Pool utilization giảm

**Giải pháp:**
- Set `timelimit` phù hợp
- Monitor và kill testcases chạy quá lâu

## 📊 Performance Metrics

### Waiting Time Distribution

```
Pool size: 10 boxes
100 testcases, each ~100ms

Waiting time histogram:
0-100ms:   ████████████████████ 40 testcases (no wait)
100-200ms: ███████████████ 30 testcases (wait 1 cycle)
200-300ms: ██████████ 20 testcases (wait 2 cycles)
300-400ms: █████ 10 testcases (wait 3 cycles)

Average waiting time: ~150ms
```

### Throughput vs Pool Size

```
100 testcases, each ~100ms execution time

Pool size 5:  ~2.0s total (50 testcases/s)
Pool size 10: ~1.0s total (100 testcases/s)
Pool size 20: ~0.5s total (200 testcases/s)
Pool size 50: ~0.2s total (500 testcases/s)

Diminishing returns after pool_size > concurrent_requests
```

## 🚦 Best Practices

### ✅ DO

- Dùng `timeout=None` (chờ vô hạn) trong production
- Monitor pool utilization thường xuyên
- Set pool size dựa trên traffic pattern
- Release box ngay sau khi xong (trong finally block)

### ❌ DON'T

- Dùng timeout quá ngắn (< 10s)
- Acquire nhiều boxes cùng lúc trong 1 thread
- Forget to release box (sẽ gây pool exhaustion)
- Set pool size quá nhỏ so với concurrent load

## 🐛 Debugging

### Check pool status

```bash
# Trong container
docker exec execution-service python -c "
from app.executor_isolate_pool import get_pool_status
import json
print(json.dumps(get_pool_status(), indent=2))
"
```

### Monitor waiting threads

```python
import threading

# List tất cả threads
for thread in threading.enumerate():
    print(f"Thread: {thread.name}, Alive: {thread.is_alive()}")
```

### Detect stuck boxes

```bash
# List tất cả isolate processes
ps aux | grep isolate

# Cleanup manually nếu cần
for i in {0..9}; do isolate --box-id $i --cleanup; done
```

## 📚 Related Topics

- [ISOLATE_GUIDE.md](./ISOLATE_GUIDE.md) - Hướng dẫn tổng quan
- [executor_isolate_pool.py](./app/executor_isolate_pool.py) - Implementation
- [test-isolate-pool.sh](./test-isolate-pool.sh) - Test scripts
