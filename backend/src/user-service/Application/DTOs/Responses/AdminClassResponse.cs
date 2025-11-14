namespace UserService.Application.DTOs.Responses;

/// <summary>
/// Response DTO cho Admin xem thông tin lớp học
/// </summary>
public class AdminClassResponse
{
    public Guid ClassId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ClassCode { get; set; } = string.Empty;
    
    // Teacher information
    public Guid TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public string TeacherEmail { get; set; } = string.Empty;
    
    // Statistics
    public int StudentCount { get; set; }
    public int ActiveStudentCount { get; set; }
    public int AssignmentCount { get; set; }
    public int SubmissionCount { get; set; }
    
    // Status
    public bool IsActive { get; set; }
    public bool IsArchived { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ArchivedAt { get; set; }
}
