using AssignmentService.Application.DTOs.Common;

namespace AssignmentService.Application.DTOs.Responses;

public class AssignmentResponse
{
    public Guid AssignmentId { get; set; }
    
    public string AssignmentType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    
    public Guid ClassId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    
    public Guid AssignedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? AssignedAt { get; set; }
    
    public int? TotalPoints { get; set; }
    public bool AllowLateSubmission { get; set; }
    
    /// <summary>
    /// Danh sách problems trong assignment với thông tin chi tiết
    /// </summary>
    public List<AssignmentProblemDetailDto>? Problems { get; set; }
    
    /// <summary>
    /// Thống kê cho teacher
    /// </summary>
    public AssignmentStatistics? Statistics { get; set; }
}

public class AssignmentProblemDetailDto
{
    public Guid ProblemId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public int Points { get; set; }
    public int OrderIndex { get; set; }
}

public class AssignmentStatistics
{
    public int TotalStudents { get; set; }
    public int NotStarted { get; set; }
    public int InProgress { get; set; }
    public int Submitted { get; set; }
    public int Graded { get; set; }
    public double AverageScore { get; set; }
    public double CompletionRate { get; set; }
}
