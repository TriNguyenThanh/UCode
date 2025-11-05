using System.ComponentModel.DataAnnotations;

namespace UserService.Application.DTOs.Requests;

public class UpdateUserRequest
{
    [Required(ErrorMessage = "UserId is required")]
    public Guid UserId { get; set; }

    [MaxLength(100)]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? Email { get; set; }

    [MaxLength(100)]
    public string? FullName { get; set; }

    [MaxLength(15)]
    [Phone(ErrorMessage = "Invalid phone number format")]
    public string? Phone { get; set; }

    // For Student
    [MaxLength(100)]
    public string? Major { get; set; }

    [Range(1, 6)]
    public int? ClassYear { get; set; }

    // For Teacher
    [MaxLength(100)]
    public string? Department { get; set; }

    [MaxLength(50)]
    public string? Title { get; set; }
}

/// <summary>
/// DTO để Admin cập nhật thông tin user (không cần truyền UserId trong body)
/// </summary>
public class UpdateUserByAdminRequest
{
    [MaxLength(100)]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? Email { get; set; }

    [MaxLength(100)]
    public string? FullName { get; set; }

    [MaxLength(15)]
    [Phone(ErrorMessage = "Invalid phone number format")]
    public string? Phone { get; set; }

    // For Student
    [MaxLength(100)]
    public string? Major { get; set; }

    [Range(1, 6)]
    public int? ClassYear { get; set; }

    // For Teacher
    [MaxLength(100)]
    public string? Department { get; set; }

    [MaxLength(50)]
    public string? Title { get; set; }
}

