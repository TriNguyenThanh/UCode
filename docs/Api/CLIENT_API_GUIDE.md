# UCode API - Hướng Dẫn Chi Tiết Cho Client

## Mục Lục
- [Authentication](#authentication)
- [API Lấy Danh Sách Bài Tập (Assignments)](#api-lấy-danh-sách-bài-tập-assignments)
- [API Lấy Danh Sách Nộp Bài (Submissions)](#api-lấy-danh-sách-nộp-bài-submissions)
- [Ví Dụ Code](#ví-dụ-code)
- [Response Format](#response-format)
- [Error Handling](#error-handling)

---

## Authentication

Tất cả API đều yêu cầu JWT token trong header:

```http
Authorization: Bearer <your_jwt_token>
```

Token sẽ tự động được gateway parse và thêm các header sau:
- `X-User-Id`: GUID của user
- `X-Role`: Role của user (student, teacher, admin)
- `X-User-Name`: Username
- `X-User-FullName`: Tên đầy đủ
- `X-User-Code`: Mã sinh viên/giảng viên

---

## API Lấy Danh Sách Bài Tập (Assignments)

### 1. Lấy danh sách bài tập của sinh viên

**Endpoint:** `GET /api/v1/assignments/student/my-assignments`

**Role:** `student`

**Description:** Lấy tất cả bài tập được giao cho sinh viên đang đăng nhập

**Response:**
```json
{
  "success": true,
  "message": "Success",
  "data": [
    {
      "assignmentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "title": "Bài tập tuần 1 - Cơ bản về lập trình",
      "description": "Làm các bài tập về vòng lặp và điều kiện",
      "assignmentType": "PRACTICE", // hoặc "EXAM"
      "classId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "startTime": "2026-01-20T00:00:00Z",
      "endTime": "2026-01-27T23:59:59Z",
      "assignedAt": "2026-01-15T10:00:00Z",
      "totalPoints": 100,
      "allowLateSubmission": true,
      "status": "PUBLISHED",
      "totalProblems": 5,
      "problems": [
        {
          "problemId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
          "title": "Tổng 2 số",
          "code": "P001",
          "difficulty": "EASY",
          "points": 20,
          "orderIndex": 0
        }
      ]
    }
  ],
  "errors": null
}
```

**Example:**
```javascript
const response = await fetch('https://api.ucode.io.vn/api/v1/assignments/student/my-assignments', {
  method: 'GET',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  }
});
const data = await response.json();
```

---

### 4. Lấy danh sách bài tập của giảng viên

**Endpoint:** `GET /api/v1/assignments/my-assignments`

**Role:** `teacher`, `admin`

**Description:** Lấy tất cả bài tập do giảng viên tạo

**Example:**
```javascript
const response = await fetch('https://api.ucode.io.vn/api/v1/assignments/my-assignments', {
  headers: {
    'Authorization': `Bearer ${token}`
  }
});
```


---

## API Lấy Danh Sách Nộp Bài (Submissions)

### 1. Lấy tất cả submissions của user

**Endpoint:** `GET /api/v1/submissions/user?pageNumber=1&pageSize=10`

**Role:** Tất cả

**Description:** Lấy danh sách submissions của user hiện tại (có phân trang)

**Query Parameters:**
- `pageNumber` (optional, default=1): Số trang
- `pageSize` (optional, default=10): Số items mỗi trang

**Response:**
```json
{
  "success": true,
  "message": "Retrieved 10 submissions",
  "data": [
    {
      "submissionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "problemId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "userCode": "SV001",
      "userFullName": "Nguyễn Văn A",
      "languageId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "code": "...",
      "status": "ACCEPTED", // PENDING, JUDGING, ACCEPTED, WRONG_ANSWER, TIME_LIMIT_EXCEEDED, etc.
      "score": 100,
      "passedTestcase": 10,
      "totalTestcase": 10,
      "executionTime": 120, // ms
      "memoryUsed": 2048, // KB
      "submittedAt": "2026-01-20T15:30:00Z",
      "errorMessage": null
    }
  ]
}
```

**Example:**
```javascript
const response = await fetch('https://api.ucode.io.vn/api/v1/submissions/user?pageNumber=1&pageSize=20', {
  headers: {
    'Authorization': `Bearer ${token}`
  }
});
const data = await response.json();
```

