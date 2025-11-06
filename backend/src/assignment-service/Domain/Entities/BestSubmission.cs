namespace AssignmentService.Domain.Entities;

/// <summary>
/// Entity đại diện cho bảng BestSubmissions
/// Lưu submission tốt nhất của user cho từng problem trong assignment
/// </summary>
public class BestSubmission
{
    public Guid BestSubmissionId { get; set; }
    
    /// <summary>
    /// ID của Assignment (stored only, không có FK relationship)
    /// Denormalized để query nhanh hơn
    /// </summary>
    public Guid AssignmentId { get; set; }

    /// <summary>
    /// ID của User (stored only, không có FK relationship)
    /// Denormalized để query nhanh hơn
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// ID của Problem (stored only, không có FK relationship)
    /// Denormalized để query nhanh hơn
    /// </summary>
    public Guid ProblemId { get; set; }
    
    /// <summary>
    /// Foreign Key đến Submission (best submission)
    /// </summary>
    public Guid SubmissionId { get; set; }
    
    /// <summary>
    /// Điểm đạt được
    /// </summary>
    public int Score { get; set; }
    
    /// <summary>
    /// Điểm tối đa
    /// </summary>
    public int MaxScore { get; set; }
    
    /// <summary>
    /// Tổng thời gian thực thi (ms)
    /// </summary>
    public long TotalTime { get; set; }
    
    /// <summary>
    /// Tổng bộ nhớ sử dụng (KB)
    /// </summary>
    public long TotalMemory { get; set; }
    
    /// <summary>
    /// Thời gian cập nhật lần cuối
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
