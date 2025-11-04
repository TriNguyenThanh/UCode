# Quick Setup Guide - File Service

## 1. Cài đặt Dependencies

```bash
cd backend/src/file-service
dotnet restore
```

## 2. Cấu hình AWS

### Option A: AWS CLI (Recommended)
```bash
aws configure
# Nhập: Access Key, Secret Key, Region (ap-southeast-1)
```

### Option B: Environment Variables
```bash
# Windows PowerShell
$env:AWS__Region="ap-southeast-1"
$env:AWS__BucketName="ucode-files-dev"
$env:AWS__AccessKey="YOUR_ACCESS_KEY"
$env:AWS__SecretKey="YOUR_SECRET_KEY"

# Linux/Mac
export AWS__Region=ap-southeast-1
export AWS__BucketName=ucode-files-dev
export AWS__AccessKey=YOUR_ACCESS_KEY
export AWS__SecretKey=YOUR_SECRET_KEY
```

### Option C: appsettings.Development.json
Cập nhật file `appsettings.Development.json`:
```json
{
  "AWS": {
    "Region": "ap-southeast-1",
    "BucketName": "ucode-files-dev",
    "AccessKey": "YOUR_ACCESS_KEY",
    "SecretKey": "YOUR_SECRET_KEY"
  }
}
```

⚠️ **QUAN TRỌNG**: Không commit credentials vào Git!

## 3. Tạo S3 Bucket

1. Đăng nhập AWS Console: https://console.aws.amazon.com
2. Vào S3 Service
3. Click "Create bucket"
4. Bucket name: `ucode-files-dev`
5. Region: `Asia Pacific (Singapore) ap-southeast-1`
6. Block all public access: ✅ (recommended)
7. Bucket Versioning: Enable (optional)
8. Click "Create bucket"

## 4. Tạo IAM User với S3 Access

1. Vào IAM Service
2. Click "Users" → "Add users"
3. User name: `ucode-file-service`
4. Access type: Programmatic access
5. Attach policy: `AmazonS3FullAccess` (hoặc custom policy)
6. Copy Access Key ID và Secret Access Key

### Custom IAM Policy (Recommended - Least Privilege)
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

## 5. Build và Run

```bash
# Build
dotnet build

# Run
dotnet run

# Service sẽ chạy tại: http://localhost:5073
```

## 6. Test API

### Option A: Swagger UI
Mở browser: http://localhost:5073/swagger

### Option B: VS Code REST Client
Mở file `file-service.http` và click "Send Request"

### Option C: curl
```bash
# Health check
curl http://localhost:5073/health

# Upload file
curl -X POST http://localhost:5073/api/files/upload \
  -F "file=@/path/to/file.txt" \
  -F "folder=uploads"

# List files
curl http://localhost:5073/api/files/list
```

## 7. Verify Setup

1. Health check: `GET http://localhost:5073/health`
   - Response: `Healthy` = ✅ S3 connection successful
   - Response: `Unhealthy` = ❌ Check AWS credentials

2. Upload test file: `POST http://localhost:5073/api/files/upload`
3. List files: `GET http://localhost:5073/api/files/list`

## Troubleshooting

### Error: "Unable to get IAM security credentials"
- Kiểm tra AWS credentials đã được cấu hình đúng
- Thử: `aws s3 ls` để verify credentials

### Error: "Access Denied"
- Kiểm tra IAM policy có đủ permissions
- Kiểm tra bucket name trong config

### Error: "The specified bucket does not exist"
- Tạo bucket hoặc sửa bucket name trong appsettings

### Error: "Request size too large"
- Tăng `RequestSizeLimit` trong `FilesController.cs`
- Default limit: 100MB

## Production Deployment

1. Cập nhật `appsettings.Production.json`
2. Sử dụng IAM Roles thay vì Access Keys (nếu deploy trên AWS EC2/ECS)
3. Enable S3 bucket encryption
4. Enable CloudFront CDN (optional)
5. Setup backup policy

## API Endpoints

- `GET /` - Service info
- `GET /health` - Health check
- `POST /api/files/upload` - Upload file
- `GET /api/files/download/{key}` - Download file
- `DELETE /api/files/{key}` - Delete file
- `POST /api/files/presigned-url` - Get presigned URL
- `GET /api/files/list` - List files
- `GET /api/files/exists/{key}` - Check file exists

## Next Steps

1. ✅ Setup complete
2. Test all endpoints
3. Integrate with other services
4. Setup monitoring and logging
5. Configure backup strategy

For detailed documentation, see `README.md`
