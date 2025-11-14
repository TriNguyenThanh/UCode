namespace UserService.Application.DTOs.Responses;

public class AdminUserDetailResponse : AdminUserResponse
{
    public string? Phone { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool EmailVerified { get; set; }
    
    // Extended Statistics
    public int TotalAssignments { get; set; } // for teachers
    public int TotalSubmissions { get; set; } // for students
    public double AverageScore { get; set; } // for students
}
