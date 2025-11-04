using System.ComponentModel.DataAnnotations;

namespace UserService.Application.DTOs.Requests;

public class AddStudentsToClassRequest
{
    [Required(ErrorMessage = "ClassId is required")]
    public Guid ClassId { get; set; }

    [Required(ErrorMessage = "At least one StudentId is required")]
    [MinLength(1, ErrorMessage = "At least one StudentId is required")]
    public List<string> StudentIds { get; set; } = new List<string>();
}

