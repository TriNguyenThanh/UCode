using System.ComponentModel.DataAnnotations;

namespace UserService.Application.DTOs.Requests;

public class BulkActionRequest
{
    [Required]
    public string Action { get; set; } = string.Empty; // "archive", "unarchive", "delete"
    
    [Required]
    [MinLength(1)]
    public List<string> ClassIds { get; set; } = new();
    
    public string? Reason { get; set; }
}
