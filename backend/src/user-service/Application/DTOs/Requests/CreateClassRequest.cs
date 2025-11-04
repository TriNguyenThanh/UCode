using System.ComponentModel.DataAnnotations;

namespace UserService.Application.DTOs.Requests;

/// <summary>
/// DTO cho request tạo Class mới
/// </summary>
public class CreateClassRequest
{
    /// <summary>
    /// Tên lớp học
    /// </summary>
    [Required(ErrorMessage = "Class name is required")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Mô tả lớp học
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// ID của giáo viên phụ trách
    /// </summary>
    [Required(ErrorMessage = "TeacherId is required")]
    public string TeacherId { get; set; } = string.Empty;

    /// <summary>
    /// Mã lớp học (unique): CLS2401, CLS2402,... (tự động tạo nếu không cung cấp)
    /// </summary>
    [MaxLength(10)]
    public string? ClassCode { get; set; }
}

