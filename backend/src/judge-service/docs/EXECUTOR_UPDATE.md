# Executor Isolate - Result Format Update

## 📋 Thay đổi chính

### **Result Format Mới - Đơn giản & Nhất quán**

Trước đây executor trả về format rườm rà với nhiều tên field khác nhau. Giờ đã được chuẩn hóa:

```python
# OLD FORMAT (inconsistent)
{
    "testcaseId": str,
    "actualOutput": str,
    "status": str,
    "errorMessage": str,
    "executionTimeMs": int,
    "memoryUsageKb": int
}

# NEW FORMAT (clean & simple)
{
    "testcaseId": str,
    "status": str,      # TESTCASE_STATUS constant
    "time": int,        # milliseconds
    "memory": int,      # KB
    "output": str,      # actual output
    "error": str        # error message if any
}
```

### **Lợi ích**

1. ✅ **Tên field ngắn gọn**: `time`, `memory`, `output`, `error`
2. ✅ **Nhất quán**: Không còn `actualOutput` vs `output`, `errorMessage` vs `error`
3. ✅ **Dễ đọc code**: `result.get("time")` thay vì `result.get("executionTimeMs")`
4. ✅ **Match với message_handler**: Format đã được sync hoàn toàn

---

## 🔄 Flow xử lý

### **1. Executor trả về results**
```python
results = execute_in_sandbox(
    language="python",
    code=code,
    testcases=[...]
)

# results = [
#     {"testcaseId": "tc1", "status": "Passed", "time": 45, "memory": 2048, ...},
#     {"testcaseId": "tc2", "status": "WrongAnswer", "time": 50, "memory": 2100, ...},
# ]
```

### **2. Message Handler xử lý results**
```python
# Build CompileResult string
compile_result = ""
total_time = 0
total_memory = 0

for result in results:
    status = result.get("status")  # Clean access
    status_code = TESTCASE_STATUS_CODE.get(status, "4")
    compile_result += status_code
    
    total_time += result.get("time", 0)      # Simple field name
    total_memory += result.get("memory", 0)   # Simple field name

# compile_result = "050" -> TC1 passed, TC2 WrongAnswer, TC3 passed
```

### **3. Response gửi về server**
```python
{
    "SubmissionId": "uuid",
    "CompileResult": "050",      # Status codes của từng testcase
    "TotalTime": 1250,           # Tổng time từ tất cả testcases
    "TotalMemory": 45600,        # Tổng memory từ tất cả testcases
    "ErrorCode": "Failed",
    "ErrorMessage": "Some test cases failed"
}
```

---

## 📝 Status Codes

Mapping từ TESTCASE_STATUS sang CompileResult string:

| Status | Code | Meaning |
|--------|------|---------|
| `Passed` | `0` | Testcase passed ✅ |
| `TimeLimitExceeded` | `1` | TLE ⏱️ |
| `MemoryLimitExceeded` | `2` | MLE 💾 |
| `RuntimeError` | `3` | RE 💥 |
| `InternalError` | `4` | Internal error 🔧 |
| `WrongAnswer` | `5` | WA ❌ |
| `CompilationError` | `6` | CE 🔨 |
| `Skipped` | `7` | Skipped ⏭️ |

---

## 🧪 Testing

Chạy test để verify integration:

```bash
cd /home/trislord/Code/UCode/backend/src/judge-service
python3 -m app.test_integration
```

Test coverage:
- ✅ Python simple addition (all pass)
- ✅ Python wrong answer detection
- ✅ C++ compilation and execution
- ✅ CompileResult string format
- ✅ TotalTime & TotalMemory calculation

---

## 📂 Files Changed

1. **`executor_isolate.py`**
   - Updated result format: `time`, `memory`, `output`, `error`
   - Simplified field names
   - Cleaner logging output

2. **`message_handler.py`**
   - Already using new format (`result.get("time")`, `result.get("memory")`)
   - Builds `CompileResult` string correctly
   - Calculates `TotalTime` and `TotalMemory`

3. **`test_integration.py`** (NEW)
   - Integration tests
   - Verifies executor output format
   - Tests CompileResult string generation

---

## 🎯 Summary

**Trước**: Executor và Handler dùng field names khác nhau, gây confusion  
**Sau**: Format thống nhất, clean, dễ maintain

**Trước**: `executionTimeMs`, `memoryUsageKb`, `actualOutput`, `errorMessage`  
**Sau**: `time`, `memory`, `output`, `error`

Simple is better! 🚀
