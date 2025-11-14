namespace UserService.Application.DTOs.Responses;

/// <summary>
/// Kết quả tạo nhiều sinh viên cùng lúc
/// </summary>
public class BulkCreateResult
{
    /// <summary>
    /// Số lượng sinh viên tạo thành công
    /// </summary>
    public int SuccessCount { get; set; }
    
    /// <summary>
    /// Số lượng sinh viên tạo thất bại
    /// </summary>
    public int FailureCount { get; set; }
    
    /// <summary>
    /// Chi tiết kết quả từng sinh viên
    /// </summary>
    public List<BulkCreateStudentResult> Results { get; set; } = new();
}

/// <summary>
/// Kết quả tạo một sinh viên trong bulk operation
/// </summary>
public class BulkCreateStudentResult
{
    /// <summary>
    /// Mã sinh viên
    /// </summary>
    public string StudentCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Tạo thành công hay không
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Thông báo lỗi nếu thất bại
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// User ID nếu tạo thành công
    /// </summary>
    public string? UserId { get; set; }
}
