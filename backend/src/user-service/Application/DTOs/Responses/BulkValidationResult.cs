namespace UserService.Application.DTOs.Responses;

/// <summary>
/// Kết quả validate bulk của một sinh viên (dùng cho Import Excel optimization)
/// </summary>
public class BulkValidationResult
{
    /// <summary>
    /// Mã sinh viên
    /// </summary>
    public string StudentCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Sinh viên đã tồn tại trong hệ thống hay chưa
    /// </summary>
    public bool Exists { get; set; }
    
    /// <summary>
    /// User ID nếu sinh viên đã tồn tại
    /// </summary>
    public string? UserId { get; set; }
}
