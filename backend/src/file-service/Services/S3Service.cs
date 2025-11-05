using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using file_service.Enums;

namespace file_service.Services;

public class S3Service : IS3Service
{
    private readonly IAmazonS3 _s3Client;
    private readonly IConfiguration _configuration;
    private readonly ILogger<S3Service> _logger;
    private readonly string _bucketName;

    public S3Service(IAmazonS3 s3Client, IConfiguration configuration, ILogger<S3Service> logger)
    {
        _s3Client = s3Client;
        _configuration = configuration;
        _logger = logger;
        _bucketName = configuration["AWS:BucketName"] ?? throw new ArgumentNullException("AWS:BucketName");
    }

    public async Task<Models.FileUploadResponse> UploadFileAsync(IFormFile file, FileCategory category, string? customFileName = null)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty or null");
            }

            // Validate file theo category
            var validation = Validators.FileValidator.ValidateFile(file, category);
            if (!validation.IsValid)
            {
                throw new ArgumentException(validation.ErrorMessage);
            }

            // Validate file content (magic bytes) cho images
            if (category == FileCategory.Image || category == FileCategory.Avatar)
            {
                var isValidContent = await Validators.FileValidator.ValidateFileContentAsync(file, category);
                if (!isValidContent)
                {
                    throw new ArgumentException("File content does not match the file extension. Possible file type spoofing detected.");
                }
            }

            // Get folder path từ configuration
            var folderPath = Models.FileCategoryConfiguration.GetFolderPath(category);

            // Generate unique and safe file name
            var originalFileName = Validators.FileValidator.SanitizeFileName(file.FileName);
            var fileExtension = Path.GetExtension(originalFileName);
            
            var fileName = !string.IsNullOrEmpty(customFileName)
                ? Validators.FileValidator.SanitizeFileName(customFileName) + fileExtension
                : $"{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}_{Guid.NewGuid().ToString("N").Substring(0, 8)}{fileExtension}";
            
            var key = $"{folderPath}/{fileName}";

            using var stream = file.OpenReadStream();
            
            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = stream,
                Key = key,
                BucketName = _bucketName,
                ContentType = file.ContentType,
                CannedACL = S3CannedACL.PublicRead, // Public access for students to view problem attachments
                Metadata =
                {
                    ["original-filename"] = originalFileName,
                    ["category"] = category.ToString(),
                    ["upload-date"] = DateTimeOffset.UtcNow.ToString("o")
                }
            };

            var transferUtility = new TransferUtility(_s3Client);
            
            _logger.LogInformation($"Starting upload for file: {originalFileName} to key: {key}");
            
            await transferUtility.UploadAsync(uploadRequest);

            // Verify upload thành công bằng cách kiểm tra file tồn tại
            var uploadSuccessful = await FileExistsAsync(key);
            
            if (!uploadSuccessful)
            {
                _logger.LogError($"Upload verification failed for file: {originalFileName}");
                throw new InvalidOperationException($"File upload failed - file not found in S3 after upload: {key}");
            }

            _logger.LogInformation($"File uploaded and verified successfully: {originalFileName} -> {key} (Category: {category})");

            // Return public S3 URL
            var region = _configuration["AWS:Region"];
            var publicUrl = $"https://{_bucketName}.s3.{region}.amazonaws.com/{key}";

            return new Models.FileUploadResponse
            {
                FileName = originalFileName,
                FileUrl = publicUrl,
                Key = key,
                Size = file.Length,
                ContentType = file.ContentType
            };
        }
        catch (AmazonS3Exception s3Ex)
        {
            _logger.LogError(s3Ex, $"S3 error uploading file: {file?.FileName}. Error Code: {s3Ex.ErrorCode}, Status: {s3Ex.StatusCode}");
            throw new InvalidOperationException($"S3 upload failed: {s3Ex.Message}", s3Ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error uploading file: {file?.FileName}");
            throw;
        }
    }    public async Task<Stream> DownloadFileAsync(string key)
    {
        try
        {
            var request = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            var response = await _s3Client.GetObjectAsync(request);
            
            // Copy to memory stream to avoid disposing issues
            var memoryStream = new MemoryStream();
            await response.ResponseStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            
            _logger.LogInformation($"File downloaded successfully: {key}");
            
            return memoryStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error downloading file: {key}");
            throw;
        }
    }

    public async Task<bool> DeleteFileAsync(string key)
    {
        try
        {
            var request = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            await _s3Client.DeleteObjectAsync(request);
            
            _logger.LogInformation($"File deleted successfully: {key}");
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting file: {key}");
            throw;
        }
    }

    public async Task<string> GetPresignedUrlAsync(string key, int expirationMinutes = 60)
    {
        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = key,
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                Protocol = Protocol.HTTPS
            };

            var url = await _s3Client.GetPreSignedURLAsync(request);
            
            _logger.LogInformation($"Presigned URL generated for: {key}");
            
            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error generating presigned URL for: {key}");
            throw;
        }
    }

    public async Task<List<Models.FileInfo>> ListFilesAsync(string? prefix = null)
    {
        try
        {
            var request = new ListObjectsV2Request
            {
                BucketName = _bucketName,
                Prefix = prefix
            };

            var response = await _s3Client.ListObjectsV2Async(request);
            
            var files = response.S3Objects.Select(obj => new Models.FileInfo
            {
                FileName = Path.GetFileName(obj.Key),
                Key = obj.Key,
                Size = obj.Size,
                LastModified = obj.LastModified,
                Url = $"https://{_bucketName}.s3.amazonaws.com/{obj.Key}"
            }).ToList();

            _logger.LogInformation($"Listed {files.Count} files with prefix: {prefix ?? "none"}");
            
            return files;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error listing files with prefix: {prefix}");
            throw;
        }
    }

    public async Task<bool> FileExistsAsync(string key)
    {
        try
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            await _s3Client.GetObjectMetadataAsync(request);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error checking file existence: {key}");
            throw;
        }
    }
}
