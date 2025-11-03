namespace UserService.Application.DTOs.Responses;

public class TeacherResponse : UserResponse
{
    public string TeacherCode { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int ClassCount { get; set; }
}

