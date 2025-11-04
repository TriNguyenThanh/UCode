using System.ComponentModel.DataAnnotations;

namespace UserService.Application.DTOs.Requests;

/// <summary>
/// DTO cho request cập nhật thông tin cá nhân (self-update)
/// Không bao gồm userId vì lấy từ JWT token
/// </summary>
public class UpdateMyProfileRequest
{
    /// <summary>
    /// Email mới
    /// </summary>
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? Email { get; set; }

    /// <summary>
    /// Họ và tên đầy đủ
    /// </summary>
    [MaxLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
    public string? FullName { get; set; }

    /// <summary>
    /// Số điện thoại
    /// </summary>
    [Phone(ErrorMessage = "Invalid phone number format")]
    [MaxLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
    public string? Phone { get; set; }
}
