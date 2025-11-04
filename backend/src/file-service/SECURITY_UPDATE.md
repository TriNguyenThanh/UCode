# File Service - Security Update Summary

## ✅ Đã hoàn thành

### 1. **File Category System**
Đã tạo hệ thống phân loại file với 6 categories:

| ID | Category | Folder | Max Size | Use Case |
|----|----------|--------|----------|----------|
| 1 | AssignmentDocument | `assignments/` | 10MB | Đề bài, tài liệu |
| 2 | CodeSubmission | `submissions/` | 5MB | Code bài làm |
| 3 | Image | `images/` | 5MB | Hình ảnh minh họa |
| 4 | Avatar | `avatars/` | 2MB | Ảnh đại diện |
| 5 | TestCase | `testcases/` | 1MB | Input/Output test |
| 6 | Reference | `references/` | 20MB | Tài liệu tham khảo |

### 2. **Security Features**

#### ✅ File Extension Validation
- Chỉ cho phép extensions được định nghĩa trong config
- Mỗi category có danh sách riêng
- Ví dụ: AssignmentDocument chỉ cho phép `.pdf`, `.docx`, `.txt`, `.md`

#### ✅ MIME Type Validation
- Kiểm tra Content-Type của file upload
- Ngăn chặn upload file với MIME type không đúng

#### ✅ File Size Validation
- Mỗi category có giới hạn riêng
- Avatar: 2MB, Assignment: 10MB, Reference: 20MB

#### ✅ Magic Bytes Validation
- **Images và Avatars**: Validate file signature (magic bytes)
- Ngăn chặn: File `.exe` đổi tên thành `.jpg`
- Detect file spoofing attacks
- Support: JPEG, PNG, GIF, WEBP

#### ✅ Filename Sanitization
- Tự động làm sạch tên file
- Loại bỏ: `../`, `..\\`, `*`, `?`, `<`, `>`, `|`, etc.
- Giới hạn độ dài tên file (max 100 chars)

#### ✅ Metadata Tracking
- Lưu metadata cho mỗi file:
  - `original-filename`: Tên gốc
  - `category`: Loại file
  - `upload-date`: Thời gian upload

### 3. **API Changes**

#### Before (Không an toàn):
```http
POST /api/files/upload
- file: any file
- folder: "any-folder" (client tự chọn)
- customFileName: optional
```
❌ Client có thể tự chọn folder
❌ Không validate loại file
❌ Dễ bị hack

#### After (An toàn):
```http
POST /api/files/upload
- file: validated file
- category: 1-6 (enum, bắt buộc)
- customFileName: optional
```
✅ Server xác định folder dựa trên category
✅ Validate tất cả: extension, MIME, size, magic bytes
✅ Ngăn chặn file độc hại

### 4. **New Endpoints**

```http
GET /api/files/categories
```
Lấy danh sách tất cả categories với config

```http
GET /api/files/categories/{id}
```
Lấy config của category cụ thể

### 5. **Code Structure**

```
file-service/
├── Enums/
│   └── FileCategory.cs           ← File categories enum
├── Models/
│   ├── FileCategoryConfig.cs     ← Category configurations
│   └── FileUploadRequest.cs      ← Updated with category
├── Validators/
│   └── FileValidator.cs          ← Validation logic + magic bytes
├── Services/
│   ├── IS3Service.cs             ← Updated interface
│   └── S3Service.cs              ← Updated with validation
└── Controllers/
    └── FilesController.cs        ← Updated endpoints
```

## 🔒 Security Improvements

### Attack Vectors Prevented:

1. **Path Traversal**
   - ❌ Before: Client có thể dùng `folder=../../etc`
   - ✅ After: Server xác định folder dựa trên category

2. **File Type Spoofing**
   - ❌ Before: `.exe` đổi tên thành `.jpg` sẽ upload được
   - ✅ After: Magic bytes validation detect và reject

3. **Oversized Files**
   - ❌ Before: Giới hạn global 100MB cho tất cả
   - ✅ After: Giới hạn theo category (Avatar 2MB, TestCase 1MB)

4. **Malicious Filenames**
   - ❌ Before: `../../../etc/passwd` có thể gây lỗi
   - ✅ After: Sanitize và validate filename

5. **MIME Type Mismatch**
   - ❌ Before: Không kiểm tra
   - ✅ After: Validate MIME type theo category

## 📊 Example Usage

### Client Code (React/TypeScript)
```typescript
// 1. Get available categories
const categories = await fetch('/api/files/categories')
  .then(r => r.json());

// 2. Upload with validation
async function uploadAssignment(file: File) {
  // Client-side pre-validation
  if (file.size > 10 * 1024 * 1024) {
    throw new Error('File too large');
  }
  
  const formData = new FormData();
  formData.append('file', file);
  formData.append('category', '1'); // AssignmentDocument
  
  const response = await fetch('/api/files/upload', {
    method: 'POST',
    body: formData
  });
  
  return await response.json();
}
```

### Error Handling
```typescript
try {
  const result = await uploadFile(file, 1);
  if (!result.success) {
    // Display user-friendly error
    alert(result.message);
    // "File extension '.exe' is not allowed..."
  }
} catch (error) {
  console.error('Upload failed', error);
}
```

## 🧪 Testing

### Test Valid Upload
```bash
curl -X POST http://localhost:5073/api/files/upload \
  -F "file=@assignment.pdf" \
  -F "category=1"

# ✅ Success: File uploaded to assignments/
```

### Test Invalid Extension
```bash
curl -X POST http://localhost:5073/api/files/upload \
  -F "file=@malware.exe" \
  -F "category=1"

# ❌ Error: "File extension '.exe' is not allowed for AssignmentDocument"
```

### Test File Spoofing
```bash
# Rename malware.exe to fake.jpg
curl -X POST http://localhost:5073/api/files/upload \
  -F "file=@fake.jpg" \
  -F "category=3"

# ❌ Error: "File content does not match the file extension. Possible file type spoofing detected."
```

### Test File Size
```bash
curl -X POST http://localhost:5073/api/files/upload \
  -F "file=@huge-avatar.jpg" \
  -F "category=4"

# ❌ Error: "File size exceeds maximum limit of 2.00MB for Avatar"
```

## 📝 Documentation

- `README.md` - Tổng quan service
- `SETUP.md` - Hướng dẫn cài đặt nhanh
- `FILE_CATEGORIES.md` - Chi tiết về categories và security
- `file-service.http` - API test examples

## 🚀 Deployment Notes

### Environment Variables
```bash
AWS__Region=ap-southeast-1
AWS__BucketName=ucode-files-prod
AWS__AccessKey=<your-key>
AWS__SecretKey=<your-secret>
```

### S3 Bucket Structure
```
ucode-files-prod/
├── assignments/
├── submissions/
├── images/
├── avatars/
├── testcases/
└── references/
```

### IAM Policy
Service cần permissions:
- `s3:PutObject` (with metadata)
- `s3:GetObject`
- `s3:DeleteObject`
- `s3:ListBucket`
- `s3:GetObjectMetadata`

## 🔄 Migration Guide

Nếu có data cũ:

1. **Categorize existing files**
   - Xác định category cho mỗi file
   - Di chuyển vào folder tương ứng

2. **Update references**
   - Cập nhật database với key mới
   - Format: `{category-folder}/{filename}`

3. **Add metadata**
   - Sử dụng S3 CopyObject để thêm metadata
   - Không cần re-upload file

## ⚠️ Breaking Changes

### API Changes
- ❌ `POST /api/files/upload?folder=xxx` - **Removed**
- ✅ `POST /api/files/upload` with `category` parameter - **Required**

### Client Impact
All clients phải update:
1. Thay `folder` parameter bằng `category`
2. Sử dụng enum value (1-6)
3. Handle validation errors

## 📈 Benefits

1. **Security**: 5 layers of validation
2. **Organization**: Auto folder structure
3. **Compliance**: File type restrictions
4. **Performance**: Size limits per use case
5. **Maintainability**: Easy to add new categories
6. **Auditability**: Metadata tracking

## 🎯 Next Steps

Recommended enhancements:

1. **Virus Scanning**: Integrate ClamAV or AWS GuardDuty
2. **CDN**: Add CloudFront for faster access
3. **Thumbnails**: Auto-generate for images
4. **Compression**: Auto-compress large files
5. **Expiration**: Auto-delete old files
6. **Analytics**: Track usage per category
7. **Quotas**: Per-user upload limits
8. **Watermarking**: For images/PDFs

## 📞 Support

Issues? Check:
1. Logs: `logs/` folder
2. Swagger: http://localhost:5073/swagger
3. Health: http://localhost:5073/health
4. Categories: http://localhost:5073/api/files/categories
