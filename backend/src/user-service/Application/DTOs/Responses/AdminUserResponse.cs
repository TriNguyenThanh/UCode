namespace UserService.Application.DTOs.Responses;

public class AdminUserResponse
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // Admin/Teacher/Student
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? StudentCode { get; set; }
    public string? TeacherCode { get; set; }
    
    // Statistics
    public int ClassCount { get; set; }
    public int EnrolledClassCount { get; set; }
}
