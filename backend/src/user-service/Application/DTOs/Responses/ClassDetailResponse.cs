namespace UserService.Application.DTOs.Responses;

public class ClassDetailResponse : ClassResponse
{
    public List<StudentResponse> Students { get; set; } = new List<StudentResponse>();
    
    public ClassStatistics Statistics { get; set; } = new ClassStatistics();
}

public class ClassStatistics
{
    public int TotalStudents { get; set; }
    public int ActiveStudents { get; set; }
    public int InactiveStudents { get; set; }
}

