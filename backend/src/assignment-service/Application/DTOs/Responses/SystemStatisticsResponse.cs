namespace AssignmentService.Application.DTOs.Responses;

public class SystemStatisticsResponse
{
    public int TotalAssignments { get; set; }
    public int TotalProblems { get; set; }
    public int TotalSubmissions { get; set; }
    public int TotalUsers { get; set; } // Tổng số sinh viên được giao bài (AssignmentUsers)
    public DateTime GeneratedAt { get; set; }
}
