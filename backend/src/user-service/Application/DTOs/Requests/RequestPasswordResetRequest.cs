using System.ComponentModel.DataAnnotations;

namespace UserService.Application.DTOs.Requests;

public class RequestPasswordResetRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;
}

