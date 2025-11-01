using AssignmentService.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace AssignmentService.Application.DTOs.Requests;

/// <summary>
/// Request DTO for creating a new problem
/// </summary>
public class ProblemCreateRequest
{
    /// <summary>
    /// Unique problem code (e.g., P001, P002)
    /// </summary>
    /// <example>P001</example>
    [Required(ErrorMessage = "Code is required")]
    [StringLength(50, ErrorMessage = "Code must not exceed 50 characters")]
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Problem title
    /// </summary>
    /// <example>Two Sum</example>
    [Required(ErrorMessage = "Title is required")]
    [StringLength(500, ErrorMessage = "Title must not exceed 500 characters")]
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Problem difficulty level
    /// </summary>
    /// <example>EASY</example>
    [Required(ErrorMessage = "Difficulty is required")]
    public Difficulty Difficulty { get; set; } = Difficulty.EASY;
    
    /// <summary>
    /// Problem visibility setting
    /// </summary>
    /// <example>PUBLIC</example>
    public Visibility Visibility { get; set; } = Visibility.PRIVATE;
    
    /// <summary>
    /// Problem status
    /// </summary>
    /// <example>DRAFT</example>
    public ProblemStatus Status { get; set; } = ProblemStatus.DRAFT;
    
    /// <summary>
    /// Time limit in milliseconds
    /// </summary>
    /// <example>1000</example>
    [Range(100, 30000, ErrorMessage = "Time limit must be between 100ms and 30000ms")]
    public int TimeLimitMs { get; set; } = 1000;
    
    /// <summary>
    /// Memory limit in KB
    /// </summary>
    /// <example>262144</example>
    [Range(1024, 1048576, ErrorMessage = "Memory limit must be between 1MB and 1024MB")]
    public int MemoryLimitKb { get; set; } = 262144;
    
    /// <summary>
    /// Source code size limit in KB
    /// </summary>
    /// <example>65536</example>
    [Range(1, 102400, ErrorMessage = "Source limit must be between 1KB and 100MB")]
    public int SourceLimitKb { get; set; } = 65536;
    
    /// <summary>
    /// Stack size limit in KB
    /// </summary>
    /// <example>8192</example>
    [Range(1024, 32768, ErrorMessage = "Stack limit must be between 1MB and 32MB")]
    public int StackLimitKb { get; set; } = 8192;
    
    /// <summary>
    /// I/O mode for the problem
    /// </summary>
    /// <example>STDIO</example>
    public IoMode IoMode { get; set; } = IoMode.STDIO;
    
    /// <summary>
    /// Reference to problem statement markdown file
    /// </summary>
    /// <example>problems/two-sum/statement.md</example>
    [StringLength(500)]
    public string? StatementMdRef { get; set; }
    
    /// <summary>
    /// Reference to custom validator script
    /// </summary>
    /// <example>validators/two-sum/validator.py</example>
    [StringLength(500)]
    public string? ValidatorRef { get; set; }
    
    /// <summary>
    /// Changelog notes for this version
    /// </summary>
    /// <example>Initial version - Classic two sum problem</example>
    [StringLength(2000)]
    public string? Changelog { get; set; }
    
    /// <summary>
    /// Whether the problem is locked from editing
    /// </summary>
    /// <example>false</example>
    public bool IsLocked { get; set; } = false;
    
    /// <summary>
    /// List of tag IDs associated with this problem
    /// </summary>
    /// <example>["11111111-1111-1111-1111-111111111111", "22222222-2222-2222-2222-222222222222"]</example>
    public List<Guid> TagIds { get; set; } = new List<Guid>();
}
