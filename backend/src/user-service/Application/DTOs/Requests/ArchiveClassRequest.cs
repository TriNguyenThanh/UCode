using System.ComponentModel.DataAnnotations;

namespace UserService.Application.DTOs.Requests;

/// <summary>
/// Request DTO để archive/unarchive lớp học
/// </summary>
public class ArchiveClassRequest
{
    [Required(ErrorMessage = "ClassId is required")]
    public string ClassId { get; set; } = string.Empty;
    
    public string? Reason { get; set; }
}
