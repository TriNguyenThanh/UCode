# API Submission Test Script

## 📝 Mô tả

Script này dùng để test việc gửi code submission tới API và polling kết quả.

## 🚀 Cách sử dụng

### Cách 1: Chạy script bash (khuyến nghị)

```bash
./run_test.sh
```

### Cách 2: Chạy trực tiếp Python

```bash
source .venv/bin/activate
python3 test_submit_api.py
```

## ⚙️ Cấu hình

Mở file `test_submit_api.py` để chỉnh sửa:

### 1. API URL
```python
BASE_URL = "http://localhost:5000/api/v1/submissions"
```

### 2. Authorization Token
Thay thế token trong `HEADERS`:
```python
"authorization": "Bearer YOUR_TOKEN_HERE"
```

### 3. Payload
Chỉnh sửa code, problemId, languageId trong `PAYLOAD`:
```python
PAYLOAD = {
    "languageId": "eac3cb6a-c218-4454-953f-138cfb22e60c",
    "problemId": "6f6af8f8-da44-4eb2-9dd3-1e08abeb2f31",
    "sourceCode": "your code here"
}
```

### 4. Polling Configuration
```python
POLL_INTERVAL = 2  # seconds giữa các lần poll
MAX_POLL_ATTEMPTS = 150  # số lần poll tối đa (150 * 2s = 5 phút)
```

## 📊 Output mẫu

```
======================================================================
  🚀 CODE SUBMISSION TEST
======================================================================
Timestamp: 2025-11-09 23:30:00

======================================================================
  📤 SUBMITTING CODE
======================================================================
URL: http://localhost:5000/api/v1/submissions/submit-code
Payload size: 612 bytes
Code length: 450 characters

📊 Response Status: 200
✅ Submission successful!
Response: {
  "submissionId": "d53f3458-ddc8-4eeb-ba8d-31ca66b0e00e",
  "status": "Pending"
}

🎯 Submission ID: d53f3458-ddc8-4eeb-ba8d-31ca66b0e00e

⏳ Waiting 3 seconds before polling...

======================================================================
  🔄 POLLING FOR RESULTS - d53f3458-ddc8-4eeb-ba8d-31ca66b0e00e
======================================================================
Poll URL: http://localhost:5000/api/v1/submissions/d53f3458-ddc8-4eeb-ba8d-31ca66b0e00e
Poll interval: 2s
Max attempts: 150

[23:30:03] Attempt 1/150 (elapsed: 0.1s)
   Status: Running
   Still processing... (Status: Running)

[23:30:05] Attempt 2/150 (elapsed: 2.2s)
   Status: Completed
   
======================================================================
  ✅ SUBMISSION COMPLETE
======================================================================
{
  "submissionId": "d53f3458-ddc8-4eeb-ba8d-31ca66b0e00e",
  "status": "Passed",
  "totalTime": 150,
  "totalMemory": 3500,
  ...
}
```

## 🔍 Troubleshooting

### Lỗi: Connection refused
- Đảm bảo API server đang chạy ở `localhost:5000`
- Kiểm tra URL trong cấu hình

### Lỗi: 401 Unauthorized
- Token đã hết hạn, cần lấy token mới
- Cập nhật token trong `HEADERS`

### Lỗi: Timeout
- Tăng `MAX_POLL_ATTEMPTS` nếu submission cần thời gian lâu
- Kiểm tra judge service có đang chạy không

## 📦 Dependencies

```bash
pip install requests
```

## 📝 Notes

- Token có thời gian expire, cần refresh định kỳ
- Submission ID format có thể khác nhau tùy API implementation
- Adjust polling logic dựa trên response structure thực tế của API
