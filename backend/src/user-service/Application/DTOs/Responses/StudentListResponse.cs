namespace UserService.Application.DTOs.Responses;

/// <summary>
/// DTO cho danh sách sinh viên (không bao gồm classes)
/// </summary>
public class StudentListResponse : UserResponse
{
    public string StudentCode { get; set; } = string.Empty;
    public int EnrollmentYear { get; set; }
    public string Major { get; set; } = string.Empty;
    public int ClassYear { get; set; }
}
