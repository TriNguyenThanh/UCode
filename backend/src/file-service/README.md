# File Service - AWS S3 Integration

Service quản lý file sử dụng AWS S3 cho hệ thống UCode.

## Tính năng

- ✅ Upload file lên S3
- ✅ Download file từ S3
- ✅ Xóa file trên S3
- ✅ Tạo presigned URL để truy cập file tạm thời
- ✅ Liệt kê các file trong bucket
- ✅ Kiểm tra sự tồn tại của file

## Cấu hình AWS

### 1. Tạo AWS Credentials

Có 2 cách để cấu hình credentials:

#### Cách 1: Sử dụng AWS CLI (Recommended)

```bash
aws configure
```

Nhập:
- AWS Access Key ID
- AWS Secret Access Key
- Default region: `ap-southeast-1`
- Default output format: `json`

#### Cách 2: Cấu hình trong appsettings.json

Cập nhật file `appsettings.Development.json`:

```json
{
  "AWS": {
    "Profile": "default",
    "Region": "ap-southeast-1",
    "BucketName": "ucode-files-dev",
    "AccessKey": "YOUR_ACCESS_KEY",
    "SecretKey": "YOUR_SECRET_KEY"
  }
}
```

⚠️ **Lưu ý**: Không commit credentials vào Git. Sử dụng environment variables hoặc AWS CLI configuration.

### 2. Tạo S3 Bucket

1. Đăng nhập AWS Console
2. Vào S3 Service
3. Tạo bucket mới với tên: `ucode-files-dev` (hoặc tên bạn muốn)
4. Chọn region: `ap-southeast-1` (Singapore)
5. Block all public access (recommended)
6. Enable versioning (optional)

### 3. Cấu hình IAM Policy

Tạo IAM user với policy sau:

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "s3:PutObject",
        "s3:GetObject",
        "s3:DeleteObject",
        "s3:ListBucket",
        "s3:GetObjectMetadata"
      ],
      "Resource": [
        "arn:aws:s3:::ucode-files-dev",
        "arn:aws:s3:::ucode-files-dev/*"
      ]
    }
  ]
}
```

## Cài đặt

### Dependencies

```bash
cd backend/src/file-service
dotnet restore
```

Packages đã được cài:
- AWSSDK.S3 (3.7.400)
- AWSSDK.Extensions.NETCore.Setup (3.7.301)
- Swashbuckle.AspNetCore (7.2.0)

## Chạy Service

### Development

```bash
dotnet run
```

Service sẽ chạy tại: `http://localhost:5073`

### Production

```bash
dotnet publish -c Release
dotnet ./bin/Release/net9.0/file-service.dll
```

## API Endpoints

### 1. Upload File
```http
POST /api/files/upload
Content-Type: multipart/form-data

Form Data:
- file: [binary file]
- folder: [optional] thư mục đích (vd: "uploads", "documents")
- customFileName: [optional] tên file tùy chỉnh
```

### 2. Download File
```http
GET /api/files/download/{key}

Example: GET /api/files/download/uploads/test.txt
```

### 3. Delete File
```http
DELETE /api/files/{key}

Example: DELETE /api/files/uploads/test.txt
```

### 4. Get Presigned URL
```http
POST /api/files/presigned-url
Content-Type: application/json

{
  "key": "uploads/test.txt",
  "expirationMinutes": 60
}
```

### 5. List Files
```http
GET /api/files/list?prefix={optional}

Example: GET /api/files/list?prefix=uploads
```

### 6. Check File Exists
```http
GET /api/files/exists/{key}

Example: GET /api/files/exists/uploads/test.txt
```

## Swagger UI

Truy cập Swagger UI tại: `http://localhost:5073/swagger`

## Testing

Sử dụng file `file-service.http` để test các API endpoints:

```bash
# Mở file file-service.http trong VS Code
# Click vào "Send Request" để test từng endpoint
```

## Environment Variables

Có thể override configuration bằng environment variables:

```bash
export AWS__Region=ap-southeast-1
export AWS__BucketName=ucode-files-dev
export AWS__AccessKey=YOUR_ACCESS_KEY
export AWS__SecretKey=YOUR_SECRET_KEY
```

## Docker

### Build Image

```bash
docker build -t file-service:latest -f Dockerfile.dev .
```

### Run Container

```bash
docker run -p 5073:8080 \
  -e AWS__Region=ap-southeast-1 \
  -e AWS__BucketName=ucode-files-dev \
  -e AWS__AccessKey=YOUR_ACCESS_KEY \
  -e AWS__SecretKey=YOUR_SECRET_KEY \
  file-service:latest
```

## Security Best Practices

1. ✅ Không commit AWS credentials vào Git
2. ✅ Sử dụng IAM roles khi chạy trên EC2/ECS
3. ✅ Giới hạn file size upload (default: 100MB)
4. ✅ Validate file types trước khi upload
5. ✅ Sử dụng presigned URLs cho file private
6. ✅ Enable CORS chỉ cho trusted origins
7. ✅ Enable S3 bucket versioning
8. ✅ Enable S3 bucket encryption

## Troubleshooting

### Lỗi: Unable to get IAM security credentials

**Giải pháp**: Cấu hình AWS credentials đúng cách (xem phần Cấu hình AWS)

### Lỗi: Access Denied

**Giải pháp**: Kiểm tra IAM policy có đủ quyền truy cập S3 bucket

### Lỗi: Bucket does not exist

**Giải pháp**: Tạo S3 bucket hoặc cập nhật tên bucket trong appsettings.json

### Lỗi: Request size too large

**Giải pháp**: Tăng `RequestSizeLimit` trong controller hoặc cấu hình IIS/Kestrel

## License

MIT
