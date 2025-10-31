using AssignmentService.Domain.Enums;

namespace AssignmentService.EF.Entities;

/// <summary>
/// Chi tiết student làm problem nào trong assignment
/// Quản lý điểm số và trạng thái cho từng problem riêng biệt
/// </summary>
public class AssignmentProblemSubmission
{
    public Guid SubmissionId { get; set; }
    
    /// <summary>
    /// FK to AssignmentDetail
    /// </summary>
    public Guid AssignmentDetailId { get; set; }
    
    /// <summary>
    /// FK to Problem
    /// </summary>
    public Guid ProblemId { get; set; }
    
    /// <summary>
    /// Trạng thái submission cho problem này
    /// </summary>
    public AssignmentProblemSubmissionStatus Status { get; set; }
    
    /// <summary>
    /// Thời gian bắt đầu làm problem này
    /// </summary>
    public DateTime? StartedAt { get; set; }
    
    /// <summary>
    /// Thời gian nộp bài cho problem này
    /// </summary>
    public DateTime? SubmittedAt { get; set; }
    
    /// <summary>
    /// Điểm thực tế đạt được
    /// </summary>
    public int? Score { get; set; }
    
    /// <summary>
    /// Điểm tối đa cho problem này (copy từ AssignmentProblem.Points)
    /// </summary>
    public int MaxScore { get; set; }
    
    /// <summary>
    /// Code solution của student
    /// </summary>
    public string? SolutionCode { get; set; }
    
    /// <summary>
    /// Feedback từ teacher cho problem này
    /// </summary>
    public string? TeacherFeedback { get; set; }
    
    /// <summary>
    /// Số lần thử làm problem này
    /// </summary>
    public int AttemptCount { get; set; } = 0;
    
    /// <summary>
    /// Thời gian thực thi (ms)
    /// </summary>
    public long? ExecutionTime { get; set; }
    
    /// <summary>
    /// Bộ nhớ sử dụng (KB)
    /// </summary>
    public long? MemoryUsed { get; set; }
    
    // Navigation properties
    public AssignmentDetail AssignmentDetail { get; set; } = null!;
    // public AssignmentProblem AssignmentProblem { get; set; } = null!; // Removed to avoid cascade cycles
    public Problem Problem { get; set; } = null!;
    public Submission Submission { get; set; } = null!;
}
