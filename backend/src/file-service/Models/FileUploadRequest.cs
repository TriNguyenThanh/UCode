using file_service.Enums;

namespace file_service.Models;

public class FileUploadRequest
{
    public IFormFile File { get; set; } = null!;
    
    /// <summary>
    /// Loại file - bắt buộc để xác định folder và validation rules
    /// </summary>
    public FileCategory Category { get; set; }
    
    /// <summary>
    /// Tên file tùy chỉnh (optional) - nếu không có sẽ tự động generate
    /// </summary>
    public string? CustomFileName { get; set; }
}
