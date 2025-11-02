using System.ComponentModel.DataAnnotations;

namespace AssignmentService.Application.DTOs.Common;

public class LanguageLimitDto
{
    public Guid? LanguageLimitId { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Lang { get; set; } = string.Empty;
    
    [Range(0.1, 10)]
    public decimal TimeFactor { get; set; } = 1.0m;
    
    [Range(1024, 1048576)]
    public int? MemoryKbOverride { get; set; }
    
    /// <summary>
    /// Code template - phần đầu (imports, setup)
    /// </summary>
    public string? Head { get; set; }
    
    /// <summary>
    /// Code template - phần chính (user code placeholder)
    /// </summary>
    public string? Body { get; set; }
    
    /// <summary>
    /// Code template - phần cuối (test runner, main function)
    /// </summary>
    public string? Tail { get; set; }
}