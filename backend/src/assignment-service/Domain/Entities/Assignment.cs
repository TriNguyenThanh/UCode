using AssignmentService.Domain.Enums;

namespace AssignmentService.EF.Entities;

/// <summary>
/// Assignment để giao bài tập cho students
/// CÓ THỂ chứa NHIỀU Problems
/// </summary>
public class Assignment
{
    public Guid AssignmentId { get; set; }

    /// <summary>
    /// Loại assignment: HOMEWORK | PRACTICE | CONTEST
    /// </summary>
    public AssignmentType AssignmentType { get; set; }

    /// <summary>
    /// ID của Class được giao bài
    /// </summary>
    public Guid ClassId { get; set; }

    /// <summary>
    /// Tiêu đề assignment (VD: "Bài tập tuần 1", "Kiểm tra giữa kỳ")
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Mô tả assignment
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Thời gian bắt đầu assignment (khi nào students có thể bắt đầu làm)
    /// Null = Ngay khi publish
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// Thời gian kết thúc assignment (deadline, hết hạn)
    /// Sau thời gian này → Assignment tự động CLOSED
    /// Null = Không có thời hạn (mở vô thời hạn)
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Teacher giao bài
    /// </summary>
    public Guid AssignedBy { get; set; }

    /// <summary>
    /// Ngày tạo assignment
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Ngày giao cho class
    /// </summary>
    public DateTime? AssignedAt { get; set; }

    /// <summary>
    /// Điểm tối đa (tổng điểm của tất cả problems)
    /// </summary>
    public int? TotalPoints { get; set; }

    /// <summary>
    /// Cho phép nộp muộn
    /// </summary>
    public bool AllowLateSubmission { get; set; } = false;

    /// <summary>
    /// Trạng thái: DRAFT, PUBLISHED, CLOSED
    /// </summary>
    public AssignmentStatus Status { get; set; } = AssignmentStatus.DRAFT;

    // Navigation properties
    // public virtual User AssignedByUser { get; set; } = null!;
    
    /// <summary>
    /// Danh sách problems trong assignment (many-to-many)
    /// </summary>
    public ICollection<AssignmentProblem> AssignmentProblems { get; set; } = new List<AssignmentProblem>();
    
    /// <summary>
    /// Chi tiết assignments cho từng student
    /// </summary>
    public ICollection<AssignmentDetail> AssignmentDetails { get; set; } = new List<AssignmentDetail>();
}