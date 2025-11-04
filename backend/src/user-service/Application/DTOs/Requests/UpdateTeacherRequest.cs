using System.ComponentModel.DataAnnotations;

namespace UserService.Application.DTOs.Requests;

/// <summary>
/// Request để cập nhật thông tin giáo viên
/// </summary>
public class UpdateTeacherRequest
{
    [MaxLength(100)]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? Email { get; set; }

    [MaxLength(100)]
    public string? FullName { get; set; }

    [MaxLength(15)]
    [Phone(ErrorMessage = "Invalid phone number format")]
    public string? Phone { get; set; }

    [MaxLength(100)]
    public string? Department { get; set; }

    [MaxLength(50)]
    public string? Title { get; set; }
}
