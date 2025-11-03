using System.ComponentModel.DataAnnotations;

namespace UserService.Application.DTOs.Requests;

public class UpdateClassRequest
{
    [Required(ErrorMessage = "ClassId is required")]
    public Guid ClassId { get; set; }

    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public string? TeacherId { get; set; }

    public bool? IsActive { get; set; }
}

