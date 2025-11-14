namespace AssignmentService.Domain.Entities;

/// <summary>
/// Ghi lại các hoạt động của sinh viên trong quá trình làm bài kiểm tra
/// Tên table: ExamActivityLog
/// </summary>
public class ExamActivityLog
{
    public Guid ActivityLogId { get; set; }
    
    /// <summary>
    /// ID của AssignmentUser
    /// </summary>
    public Guid AssignmentUserId { get; set; }
    
    /// <summary>
    /// Loại hoạt động: TAB_SWITCH, COPY_PASTE, IDLE_START, IDLE_END, MOUSE_ACTIVE, KEYBOARD_ACTIVE
    /// </summary>
    public string ActivityType { get; set; } = string.Empty;
    
    /// <summary>
    /// Thời gian xảy ra hoạt động
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// Dữ liệu bổ sung (JSON): source, duration, text length, etc.
    /// </summary>
    public string? Metadata { get; set; }
    
    /// <summary>
    /// Mức độ nghi ngờ (0-100): 0 = bình thường, 100 = rất nghi ngờ
    /// </summary>
    public int SuspicionLevel { get; set; }

    // Navigation properties
    public AssignmentUser AssignmentUser { get; set; } = null!;
}
