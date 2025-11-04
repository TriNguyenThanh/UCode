using file_service.Enums;

namespace file_service.Services;

public interface IS3Service
{
    Task<Models.FileUploadResponse> UploadFileAsync(IFormFile file, FileCategory category, string? customFileName = null);
    Task<Stream> DownloadFileAsync(string key);
    Task<bool> DeleteFileAsync(string key);
    Task<string> GetPresignedUrlAsync(string key, int expirationMinutes = 60);
    Task<List<Models.FileInfo>> ListFilesAsync(string? prefix = null);
    Task<bool> FileExistsAsync(string key);
}
