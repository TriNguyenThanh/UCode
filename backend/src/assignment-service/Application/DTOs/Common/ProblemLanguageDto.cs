using System.ComponentModel.DataAnnotations;

namespace AssignmentService.Application.DTOs.Common;

/// <summary>
/// DTO cho problem-specific language configuration.
/// SIMPLIFIED: Chỉ dùng Head/Body/Tail (null = use default from Language table)
/// Composite Key: (ProblemId, LanguageId)
/// </summary>
public class ProblemLanguageDto
{
    /// <summary>
    /// Problem ID this configuration belongs to (part of composite PK)
    /// </summary>
    public Guid ProblemId { get; set; }
    
    /// <summary>
    /// Language ID reference (part of composite PK)
    /// </summary>
    [Required(ErrorMessage = "LanguageId is required")]
    public Guid LanguageId { get; set; }
    
    /// <summary>
    /// Language code (from Language table) - for display/reference
    /// e.g., "cpp", "java", "python"
    /// </summary>
    public string? LanguageCode { get; set; }
    
    /// <summary>
    /// Language display name (from Language table) - for display
    /// e.g., "C++", "Java", "Python"
    /// </summary>
    public string? LanguageDisplayName { get; set; }
    
    /// <summary>
    /// Time factor override (null = use default from Language)
    /// Used to adjust time limit based on language speed
    /// </summary>
    [Range(0.1, 10)]
    public decimal? TimeFactor { get; set; }
    
    /// <summary>
    /// Memory limit override in KB (null = use default from Language)
    /// </summary>
    [Range(1024, 2097152)]
    public int? MemoryKb { get; set; }
    
    /// <summary>
    /// Code template head override (null = use default from Language)
    /// Usually contains imports and setup code
    /// </summary>
    public string? Head { get; set; }
    
    /// <summary>
    /// Code template body override (null = use default from Language)
    /// User code placeholder section
    /// </summary>
    public string? Body { get; set; }
    
    /// <summary>
    /// Code template tail override (null = use default from Language)
    /// Usually contains test runner and main function
    /// </summary>
    public string? Tail { get; set; }
    
    /// <summary>
    /// Whether this language is allowed for this problem
    /// </summary>
    public bool IsAllowed { get; set; } = true;
}