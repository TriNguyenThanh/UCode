using AssignmentService.Domain.Enums;
using AssignmentService.Application.DTOs.Common;

namespace AssignmentService.Application.DTOs.Responses;

/// <summary>
/// Response DTO for problem information
/// </summary>
public class ProblemResponse
{
    /// <summary>
    /// Unique identifier of the problem
    /// </summary>
    /// <example>11111111-1111-1111-1111-111111111111</example>
    public Guid ProblemId { get; set; }
    
    /// <summary>
    /// Unique problem code
    /// </summary>
    /// <example>P001</example>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// URL-friendly slug
    /// </summary>
    /// <example>two-sum</example>
    public string Slug { get; set; } = string.Empty;
    
    /// <summary>
    /// Problem title
    /// </summary>
    /// <example>Two Sum</example>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Problem difficulty level
    /// </summary>
    /// <example>EASY</example>
    public Difficulty Difficulty { get; set; }
    
    /// <summary>
    /// ID of the teacher who owns this problem
    /// </summary>
    /// <example>22222222-2222-2222-2222-222222222222</example>
    public Guid OwnerId { get; set; }
    
    /// <summary>
    /// Problem visibility setting
    /// </summary>
    /// <example>PUBLIC</example>
    public Visibility Visibility { get; set; }
    
    /// <summary>
    /// Problem status
    /// </summary>
    /// <example>PUBLISHED</example>
    public ProblemStatus Status { get; set; }
    
    /// <summary>
    /// Time limit in milliseconds
    /// </summary>
    /// <example>1000</example>
    public int TimeLimitMs { get; set; }
    
    /// <summary>
    /// Memory limit in KB
    /// </summary>
    /// <example>262144</example>
    public int MemoryLimitKb { get; set; }
    
    /// <summary>
    /// Source code size limit in KB
    /// </summary>
    /// <example>65536</example>
    public int SourceLimitKb { get; set; }
    
    /// <summary>
    /// Stack size limit in KB
    /// </summary>
    /// <example>8192</example>
    public int StackLimitKb { get; set; }
    
    /// <summary>
    /// I/O mode for the problem
    /// </summary>
    /// <example>STDIO</example>
    public IoMode IoMode { get; set; }
    
    /// <summary>
    /// Reference to problem statement markdown file
    /// </summary>
    /// <example>problems/two-sum/statement.md</example>
    public string? StatementMdRef { get; set; }
    
    /// <summary>
    /// Reference solution for the problem
    /// </summary>
    /// <example>Solution using hash map approach...</example>
    public string? Solution { get; set; }
    
    /// <summary>
    /// Input format description
    /// </summary>
    /// <example>First line contains N, the number of elements...</example>
    public string? InputFormat { get; set; }
    
    /// <summary>
    /// Output format description
    /// </summary>
    /// <example>Print a single integer representing...</example>
    public string? OutputFormat { get; set; }
    
    /// <summary>
    /// Problem constraints
    /// </summary>
    /// <example>1 <= N <= 10^5, 1 <= A[i] <= 10^9</example>
    public string? Constraints { get; set; }
    
    /// <summary>
    /// Reference to custom validator script
    /// </summary>
    /// <example>validators/two-sum/validator.py</example>
    public string? ValidatorRef { get; set; }
    
    /// <summary>
    /// Changelog notes for this version
    /// </summary>
    /// <example>Initial version - Classic two sum problem</example>
    public string? Changelog { get; set; }
    
    /// <summary>
    /// Whether the problem is locked from editing
    /// </summary>
    /// <example>false</example>
    public bool IsLocked { get; set; }
    
    /// <summary>
    /// Date and time when the problem was created
    /// </summary>
    /// <example>2024-01-15T10:30:00Z</example>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Date and time when the problem was last updated
    /// </summary>
    /// <example>2024-01-15T10:30:00Z</example>
    public DateTime UpdatedAt { get; set; }
    
    /// <summary>
    /// List of tag IDs associated with this problem
    /// </summary>
    /// <example>["11111111-1111-1111-1111-111111111111", "22222222-2222-2222-2222-222222222222"]</example>
    public List<Guid> TagIds { get; set; } = new List<Guid>();
    
    /// <summary>
    /// List of problem assets (statements, solutions, tutorials, etc.)
    /// </summary>
    public List<ProblemAssetDto> ProblemAssets { get; set; } = new List<ProblemAssetDto>();
}