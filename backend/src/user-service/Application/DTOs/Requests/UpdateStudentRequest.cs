using System.ComponentModel.DataAnnotations;

namespace UserService.Application.DTOs.Requests;

/// <summary>
/// Request để cập nhật thông tin sinh viên
/// </summary>
public class UpdateStudentRequest
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
    public string? Major { get; set; }

    [Range(1, 6, ErrorMessage = "ClassYear must be between 1 and 6")]
    public int? ClassYear { get; set; }

    [Range(2000, 2100, ErrorMessage = "Invalid enrollment year")]
    public int? EnrollmentYear { get; set; }
}
