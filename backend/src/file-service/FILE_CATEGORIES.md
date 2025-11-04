# File Service - Hướng dẫn sử dụng File Categories

## Tổng quan

File service sử dụng hệ thống phân loại file (File Categories) để:
- ✅ Tự động xác định thư mục lưu trữ
- ✅ Validate loại file được phép
- ✅ Kiểm tra kích thước file
- ✅ Ngăn chặn upload file độc hại
- ✅ Validate magic bytes (file signature)

## File Categories

### 1. Assignment Document (Category = 1)
**Mục đích**: Tài liệu đề bài, hướng dẫn bài tập

**Cấu hình**:
- Thư mục: `assignments/`
- Kích thước tối đa: **10MB**
- Định dạng cho phép: `.pdf`, `.docx`, `.doc`, `.txt`, `.md`
- MIME types: `application/pdf`, `application/msword`, `text/plain`, etc.

**Ví dụ sử dụng**:
```http
POST /api/files/upload
Content-Type: multipart/form-data

file: assignment.pdf
category: 1
```

---

### 2. Code Submission (Category = 2)
**Mục đích**: Code bài làm của sinh viên, file nén code

**Cấu hình**:
- Thư mục: `submissions/`
- Kích thước tối đa: **5MB**
- Định dạng cho phép: 
  - Archives: `.zip`, `.rar`, `.7z`
  - Source code: `.c`, `.cpp`, `.java`, `.py`, `.js`, `.ts`, `.cs`, `.go`, `.rb`, `.php`
  - Text: `.txt`, `.h`, `.hpp`
- MIME types: `application/zip`, `text/plain`, `text/x-c`, `text/x-python`, etc.

**Ví dụ sử dụng**:
```http
POST /api/files/upload
Content-Type: multipart/form-data

file: solution.zip
category: 2
```

---

### 3. Image (Category = 3)
**Mục đích**: Hình ảnh minh họa, diagram, screenshot

**Cấu hình**:
- Thư mục: `images/`
- Kích thước tối đa: **5MB**
- Định dạng cho phép: `.jpg`, `.jpeg`, `.png`, `.gif`, `.svg`, `.webp`
- MIME types: `image/jpeg`, `image/png`, `image/gif`, `image/svg+xml`, `image/webp`
- ⚠️ **Validation đặc biệt**: Kiểm tra magic bytes để ngăn file giả mạo

**Ví dụ sử dụng**:
```http
POST /api/files/upload
Content-Type: multipart/form-data

file: diagram.png
category: 3
```

---

### 4. Avatar (Category = 4)
**Mục đích**: Ảnh đại diện người dùng

**Cấu hình**:
- Thư mục: `avatars/`
- Kích thước tối đa: **2MB**
- Định dạng cho phép: `.jpg`, `.jpeg`, `.png`, `.webp`
- MIME types: `image/jpeg`, `image/png`, `image/webp`
- ⚠️ **Validation đặc biệt**: Kiểm tra magic bytes để ngăn file giả mạo

**Ví dụ sử dụng**:
```http
POST /api/files/upload
Content-Type: multipart/form-data

file: avatar.jpg
category: 4
customFileName: user-123
```

---

### 5. Test Case (Category = 5)
**Mục đích**: File input/output cho test cases

**Cấu hình**:
- Thư mục: `testcases/`
- Kích thước tối đa: **1MB**
- Định dạng cho phép: `.txt`, `.in`, `.out`
- MIME types: `text/plain`

**Ví dụ sử dụng**:
```http
POST /api/files/upload
Content-Type: multipart/form-data

file: test1.in
category: 5
```

---

### 6. Reference (Category = 6)
**Mục đích**: Tài liệu tham khảo, slide bài giảng

**Cấu hình**:
- Thư mục: `references/`
- Kích thước tối đa: **20MB**
- Định dạng cho phép: `.pdf`, `.docx`, `.doc`, `.pptx`, `.ppt`, `.xlsx`, `.xls`
- MIME types: `application/pdf`, `application/vnd.ms-powerpoint`, etc.

**Ví dụ sử dụng**:
```http
POST /api/files/upload
Content-Type: multipart/form-data

file: lecture.pptx
category: 6
```

---

## API Endpoints

### 1. Upload File
```http
POST /api/files/upload
Content-Type: multipart/form-data

Parameters:
- file (required): File to upload
- category (required): File category (1-6)
- customFileName (optional): Custom name without extension
```

**Response Success**:
```json
{
  "success": true,
  "message": "File uploaded successfully",
  "data": {
    "fileName": "assignment.pdf",
    "fileUrl": "https://bucket.s3.amazonaws.com/assignments/1730765432_a1b2c3d4.pdf",
    "key": "assignments/1730765432_a1b2c3d4.pdf",
    "size": 1024000,
    "contentType": "application/pdf"
  }
}
```

**Response Error**:
```json
{
  "success": false,
  "message": "File extension '.exe' is not allowed for AssignmentDocument. Allowed extensions: .pdf, .docx, .doc, .txt, .md",
  "data": null,
  "errors": null
}
```

### 2. Get All Categories
```http
GET /api/files/categories
```

**Response**:
```json
{
  "success": true,
  "message": "File categories retrieved successfully",
  "data": [
    {
      "id": 1,
      "name": "AssignmentDocument",
      "folderPath": "assignments",
      "maxFileSizeMB": 10.0,
      "allowedExtensions": [".pdf", ".docx", ".doc", ".txt", ".md"],
      "allowedMimeTypes": ["application/pdf", "application/msword", ...]
    },
    ...
  ]
}
```

### 3. Get Category Config
```http
GET /api/files/categories/{category}
```

**Example**: `GET /api/files/categories/2`

---

## Security Features

### 1. File Extension Validation ✅
Chỉ cho phép các extension được định nghĩa trong configuration.

### 2. MIME Type Validation ✅
Kiểm tra Content-Type của file upload.

### 3. File Size Validation ✅
Mỗi category có giới hạn kích thước riêng.

### 4. Magic Bytes Validation ✅
Đối với images và avatars, validate file signature để ngăn chặn:
- File `.exe` đổi tên thành `.jpg`
- File script độc hại giả mạo image

**Magic bytes được kiểm tra**:
- JPEG: `FF D8 FF`
- PNG: `89 50 4E 47`
- GIF: `47 49 46 38`
- WEBP: `52 49 46 46 ... 57 45 42 50`

### 5. Filename Sanitization ✅
Tự động làm sạch tên file:
- Loại bỏ path traversal (`../`, `..\\`)
- Loại bỏ ký tự đặc biệt nguy hiểm
- Giới hạn độ dài tên file

### 6. Metadata Tracking ✅
Mỗi file upload có metadata:
- `original-filename`: Tên file gốc
- `category`: Loại file
- `upload-date`: Thời gian upload

---

## Error Handling

### Common Errors

**1. Invalid Category**
```json
{
  "success": false,
  "message": "Invalid file category: 99"
}
```

**2. File Type Not Allowed**
```json
{
  "success": false,
  "message": "File extension '.exe' is not allowed for AssignmentDocument. Allowed extensions: .pdf, .docx, .doc, .txt, .md"
}
```

**3. File Size Exceeded**
```json
{
  "success": false,
  "message": "File size exceeds maximum limit of 10.00MB for AssignmentDocument"
}
```

**4. File Spoofing Detected**
```json
{
  "success": false,
  "message": "File content does not match the file extension. Possible file type spoofing detected."
}
```

**5. Dangerous Filename**
```json
{
  "success": false,
  "message": "File name contains invalid or dangerous characters"
}
```

---

## Client Integration Examples

### JavaScript/React
```javascript
async function uploadFile(file, category) {
  const formData = new FormData();
  formData.append('file', file);
  formData.append('category', category);
  
  const response = await fetch('http://localhost:5073/api/files/upload', {
    method: 'POST',
    body: formData
  });
  
  return await response.json();
}

// Usage
const file = document.getElementById('fileInput').files[0];
const result = await uploadFile(file, 1); // Assignment Document
```

### C# Client
```csharp
using var httpClient = new HttpClient();
using var form = new MultipartFormDataContent();

var fileContent = new StreamContent(File.OpenRead("assignment.pdf"));
fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

form.Add(fileContent, "file", "assignment.pdf");
form.Add(new StringContent("1"), "category");

var response = await httpClient.PostAsync(
    "http://localhost:5073/api/files/upload", 
    form
);

var result = await response.Content.ReadAsStringAsync();
```

### Python
```python
import requests

url = "http://localhost:5073/api/files/upload"

files = {'file': open('assignment.pdf', 'rb')}
data = {'category': '1'}

response = requests.post(url, files=files, data=data)
print(response.json())
```

---

## Testing

### 1. Get Available Categories
```bash
curl http://localhost:5073/api/files/categories
```

### 2. Upload Assignment Document
```bash
curl -X POST http://localhost:5073/api/files/upload \
  -F "file=@assignment.pdf" \
  -F "category=1"
```

### 3. Upload Code Submission
```bash
curl -X POST http://localhost:5073/api/files/upload \
  -F "file=@solution.zip" \
  -F "category=2"
```

### 4. Upload Avatar
```bash
curl -X POST http://localhost:5073/api/files/upload \
  -F "file=@avatar.jpg" \
  -F "category=4" \
  -F "customFileName=user-avatar"
```

### 5. Test Invalid File (Should Fail)
```bash
curl -X POST http://localhost:5073/api/files/upload \
  -F "file=@malware.exe" \
  -F "category=1"
```

---

## Best Practices

### For Frontend Developers

1. **Always get categories first**:
   ```javascript
   const categories = await fetch('/api/files/categories').then(r => r.json());
   ```

2. **Validate on client side before upload**:
   ```javascript
   function validateFile(file, category) {
     const config = categoryConfigs[category];
     if (file.size > config.maxFileSizeBytes) {
       alert(`File too large. Max: ${config.maxFileSizeMB}MB`);
       return false;
     }
     // Check extension
     const ext = file.name.split('.').pop().toLowerCase();
     if (!config.allowedExtensions.includes('.' + ext)) {
       alert(`File type not allowed`);
       return false;
     }
     return true;
   }
   ```

3. **Show user-friendly category names**:
   ```javascript
   const categoryNames = {
     1: "Assignment Document",
     2: "Code Submission",
     3: "Image",
     4: "Avatar",
     5: "Test Case",
     6: "Reference Material"
   };
   ```

4. **Handle errors gracefully**:
   ```javascript
   try {
     const result = await uploadFile(file, category);
     if (!result.success) {
       showError(result.message);
     }
   } catch (error) {
     showError("Upload failed. Please try again.");
   }
   ```

### For Backend Developers

1. **Never trust client input** - Server always validates
2. **Use enum for category** - Type-safe
3. **Log suspicious activities** - File spoofing attempts
4. **Monitor file uploads** - Track usage patterns
5. **Regular security audits** - Update allowed file types

---

## Extending Categories

Để thêm category mới:

1. **Cập nhật Enum** (`Enums/FileCategory.cs`):
   ```csharp
   public enum FileCategory
   {
       // ...existing
       VideoLecture = 7
   }
   ```

2. **Thêm Configuration** (`Models/FileCategoryConfig.cs`):
   ```csharp
   [FileCategory.VideoLecture] = new FileCategoryConfig
   {
       Category = FileCategory.VideoLecture,
       FolderPath = "videos",
       MaxFileSizeBytes = 100 * 1024 * 1024, // 100MB
       AllowedExtensions = new List<string> { ".mp4", ".avi", ".mov" },
       AllowedMimeTypes = new List<string> { "video/mp4", "video/avi" }
   }
   ```

3. Build và test!

---

## FAQ

**Q: Có thể upload file không có trong danh sách allowed extensions?**
A: Không. File sẽ bị reject với error message rõ ràng.

**Q: Nếu đổi extension file `.exe` thành `.pdf` có upload được không?**
A: Không. Đối với images, service validate magic bytes. Đối với các file khác, MIME type sẽ bị check.

**Q: Có giới hạn số lượng file upload?**
A: Mỗi request chỉ upload 1 file. Muốn upload nhiều file thì gọi API nhiều lần.

**Q: File name có bị thay đổi không?**
A: Có. File name sẽ được sanitize và thêm timestamp + unique ID để tránh trùng lặp. Tên gốc được lưu trong metadata.

**Q: Làm sao để lấy lại file đã upload?**
A: Sử dụng `key` trong response để download: `GET /api/files/download/{key}`

---

## Support

Nếu gặp vấn đề, check:
1. Logs trong `logs/` folder
2. Swagger UI: http://localhost:5073/swagger
3. Health check: http://localhost:5073/health
