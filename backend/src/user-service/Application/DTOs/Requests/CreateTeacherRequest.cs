using System.ComponentModel.DataAnnotations;

namespace UserService.Application.DTOs.Requests;

/// <summary>
/// DTO cho request tạo Teacher mới
/// </summary>
public class CreateTeacherRequest
{
    /// <summary>
    /// Mã nhân viên (unique): GV001, GV002,...
    /// </summary>
    [Required(ErrorMessage = "EmployeeId is required")]
    [MaxLength(20)]
    public string EmployeeId { get; set; } = string.Empty;

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
    /// Mật khẩu (tối thiểu 6 ký tự)
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Họ và tên đầy đủ
    /// </summary>
    [Required(ErrorMessage = "FullName is required")]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Khoa/Bộ môn
    /// </summary>
    [Required(ErrorMessage = "Department is required")]
    [MaxLength(100)]
    public string Department { get; set; } = string.Empty;

    /// <summary>
    /// Chức danh: Giảng viên, Phó Giáo sư, Giáo sư,...
    /// </summary>
    [MaxLength(50)]
    public string? Title { get; set; }

    /// <summary>
    /// Số điện thoại
    /// </summary>
    [MaxLength(15)]
    [Phone(ErrorMessage = "Invalid phone number format")]
    public string? Phone { get; set; }
}

