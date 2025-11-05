using System.ComponentModel.DataAnnotations;

namespace UserService.Application.DTOs.Requests;

public class ChangePasswordRequest
{
    [Required(ErrorMessage = "UserId is required")]
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Current password is required")]
    public string OldPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    [MaxLength(100)]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirm password is required")]
    [Compare("NewPassword", ErrorMessage = "Password and confirmation do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

