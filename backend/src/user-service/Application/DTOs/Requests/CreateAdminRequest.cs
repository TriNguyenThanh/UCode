using System.ComponentModel.DataAnnotations;

namespace UserService.Application.DTOs.Requests;

public class CreateAdminRequest
{
    [Required(ErrorMessage = "Username is required")]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "FullName is required")]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;
}

