namespace UserService.Application.DTOs.Responses;

public class StudentResponse : UserResponse
{
    public string StudentCode { get; set; } = string.Empty;
    public int EnrollmentYear { get; set; }
    public string Major { get; set; } = string.Empty;
    public int ClassYear { get; set; }
    public List<ClassResponse> Classes { get; set; } = new List<ClassResponse>();
}

