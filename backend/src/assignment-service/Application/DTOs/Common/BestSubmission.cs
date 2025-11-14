using AssignmentService.Domain.Entities;
using AssignmentService.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace AssignmentService.Application.DTOs.Common;

public class BestSubmissionDto
{
    public Guid? SubmissionId { get; set; }
    
    public Guid? AssignmentUserId { get; set; } = Guid.Empty;
    
    [Required]
    public Guid ProblemId { get; set; }

    public Guid? UserId {get; set;}
    
    public SubmissionStatus Status { get; set; }
    
    public DateTime? StartedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    
    public int? Score { get; set; }
    
    [StringLength(10000)]
    public string? SolutionCode { get; set; }
    
    [StringLength(2000)]
    public string? TeacherFeedback { get; set; }
    
    public int AttemptCount { get; set; }
    
    /// <summary>
    /// Tổng số test cases
    /// </summary>
    public int? TotalTestCases { get; set; }
    
    /// <summary>
    /// Số test cases đã pass
    /// </summary>
    public int? PassedTestCases { get; set; }
    
    /// <summary>
    /// Thời gian thực thi (ms)
    /// </summary>
    public long? ExecutionTime { get; set; }
    
    /// <summary>
    /// Bộ nhớ sử dụng (KB)
    /// </summary>
    public long? MemoryUsed { get; set; }
}
