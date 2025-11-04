namespace UserService.Application.DTOs.Responses;

public class ClassResponse
{
    public Guid ClassId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public string ClassCode { get; set; } = string.Empty;
    public int StudentCount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

