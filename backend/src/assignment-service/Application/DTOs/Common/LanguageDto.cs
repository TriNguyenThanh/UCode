using System.ComponentModel.DataAnnotations;

namespace AssignmentService.Application.DTOs.Common;

/// <summary>
/// DTO cho global language configuration
/// </summary>
public class LanguageDto
{
    public Guid LanguageId { get; set; }
    
    [Required]
    [StringLength(20)]
    public string Code { get; set; } = string.Empty; // cpp, java, python, javascript, csharp
    
    [Required]
    [StringLength(100)]
    public string DisplayName { get; set; } = string.Empty; // C++, Java, Python, JavaScript, C#
    
    [Range(0.1, 10)]
    public decimal DefaultTimeFactor { get; set; } = 1.0m;
    
    [Range(1024, 2097152)]
    public int DefaultMemoryKb { get; set; } = 262144; // 256 MB default
    
    public string DefaultHead { get; set; } = string.Empty;
    
    public string DefaultBody { get; set; } = string.Empty;
    
    public string DefaultTail { get; set; } = string.Empty;
    
    public bool IsEnabled { get; set; } = true;
    
    public int DisplayOrder { get; set; } = 0;
}
