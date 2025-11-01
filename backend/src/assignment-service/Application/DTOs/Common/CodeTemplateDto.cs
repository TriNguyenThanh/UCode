using System.ComponentModel.DataAnnotations;

namespace AssignmentService.Application.DTOs.Common;

public class CodeTemplateDto
{
    public Guid? CodeTemplateId { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Lang { get; set; } = string.Empty;
    
    [Required]
    [StringLength(500)]
    public string StarterRef { get; set; } = string.Empty;
}