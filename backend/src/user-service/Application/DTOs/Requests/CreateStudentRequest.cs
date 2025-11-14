using System.ComponentModel.DataAnnotations;

namespace UserService.Application.DTOs.Requests;

/// <summary>
/// DTO cho request tạo Student mới
/// </summary>
public class CreateStudentRequest
{
    /// <summary>
    /// Mã sinh viên (unique): SV001, SV002,...
    /// </summary>
    [Required(ErrorMessage = "StudentCode is required")]
    [MaxLength(20)]
    public string StudentCode { get; set; } = string.Empty;

    /// <summary>
    /// Tên đăng nhập (unique)
    /// </summary>
    [Required(ErrorMessage = "Username is required")]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Email (unique)
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Mật khẩu (tối thiểu 6 ký tự). Nếu không cung cấp, mặc định sẽ là "123456"
    /// </summary>
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    [MaxLength(100)]
    public string? Password { get; set; }

    /// <summary>
    /// Họ và tên đầy đủ
    /// </summary>
    [Required(ErrorMessage = "FullName is required")]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Chuyên ngành học
    /// </summary>
    [Required(ErrorMessage = "Major is required")]
    [MaxLength(100)]
    public string Major { get; set; } = string.Empty;

    /// <summary>
    /// Năm nhập học
    /// </summary>
    [Required(ErrorMessage = "EnrollmentYear is required")]
    [Range(2000, 2100, ErrorMessage = "EnrollmentYear must be between 2000 and 2100")]
    public int EnrollmentYear { get; set; }

    /// <summary>
    /// Năm học hiện tại (1-6)
    /// </summary>
    [Required(ErrorMessage = "ClassYear is required")]
    [Range(1, 6, ErrorMessage = "ClassYear must be between 1 and 6")]
    public int ClassYear { get; set; }
}

