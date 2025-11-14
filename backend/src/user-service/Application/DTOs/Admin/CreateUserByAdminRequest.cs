using System.ComponentModel.DataAnnotations;

namespace UserService.Application.DTOs.Admin;

public class CreateUserByAdminRequest
{
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role is required")]
    public string Role { get; set; } = string.Empty; // "Admin", "Teacher", "Student"

    public string? Phone { get; set; }

    public string? StudentCode { get; set; }

    public string? TeacherCode { get; set; }

    public bool IsActive { get; set; } = true;
}
