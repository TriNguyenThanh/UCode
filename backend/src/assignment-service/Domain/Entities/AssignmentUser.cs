using AssignmentService.Domain.Enums;

namespace AssignmentService.Domain.Entities;

/// <summary>
/// Chi tiết việc giao bài cho từng user (student)
/// Tên table: AssignmentUser
/// </summary>
public class AssignmentUser
{
    public Guid AssignmentUserId { get; set; }
    
    /// <summary>
    /// ID của assignment
    /// </summary>
    public Guid AssignmentId { get; set; }
    
    /// <summary>
    /// ID của user (student) được giao bài
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Trạng thái: NOT_STARTED, IN_PROGRESS, SUBMITTED, GRADED
    /// </summary>
    public AssignmentUserStatus Status { get; set; }
    
    /// <summary>
    /// Thời gian được giao
    /// </summary>
    public DateTime AssignedAt { get; set; }
    
    /// <summary>
    /// Thời gian bắt đầu làm
    /// </summary>
    public DateTime? StartedAt { get; set; }
    
    /// <summary>
    /// Điểm số (nếu đã chấm)
    /// </summary>
    public int? Score { get; set; }
    
    /// <summary>
    /// Điểm tối đa
    /// </summary>
    public int? MaxScore { get; set; }
    

    // Navigation properties
    public Assignment Assignment { get; set; } = null!;
}
