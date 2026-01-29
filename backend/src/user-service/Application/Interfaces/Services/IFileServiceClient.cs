namespace UserService.Application.Interfaces.Services;

/// <summary>
/// Interface để gọi File Service API
/// </summary>
public interface IFileServiceClient
{
    /// <summary>
    /// Upload ảnh khuôn mặt lên file-service
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="imageBase64">Ảnh dạng base64</param>
    /// <returns>File URL nếu thành công</returns>
    Task<FileUploadResult?> UploadFaceImageAsync(string userId, string imageBase64, string fileName);
}

/// <summary>
/// Kết quả upload file
/// </summary>
public class FileUploadResult
{
    public bool Success { get; set; }
    public string? FileUrl { get; set; }
    public string? Key { get; set; }
    public string? ErrorMessage { get; set; }
}
