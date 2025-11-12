namespace UserService.Application.DTOs.Responses;

/// <summary>
/// Kết quả thêm nhiều sinh viên vào lớp cùng lúc
/// </summary>
public class BulkEnrollResult
{
    /// <summary>
    /// ID lớp học
    /// </summary>
    public string ClassId { get; set; } = string.Empty;
    
    /// <summary>
    /// Số lượng sinh viên thêm thành công
    /// </summary>
    public int SuccessCount { get; set; }
    
    /// <summary>
    /// Số lượng sinh viên thêm thất bại
    /// </summary>
    public int FailureCount { get; set; }
    
    /// <summary>
    /// Chi tiết kết quả từng sinh viên
    /// </summary>
    public List<BulkEnrollStudentResult> Results { get; set; } = new();
}

/// <summary>
/// Kết quả thêm một sinh viên vào lớp trong bulk operation
/// </summary>
public class BulkEnrollStudentResult
{
    /// <summary>
    /// User ID của sinh viên
    /// </summary>
    public string StudentId { get; set; } = string.Empty;
    
    /// <summary>
    /// Thêm thành công hay không
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Thông báo lỗi nếu thất bại
    /// </summary>
    public string? ErrorMessage { get; set; }
}
