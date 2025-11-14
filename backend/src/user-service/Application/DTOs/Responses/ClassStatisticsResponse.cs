namespace UserService.Application.DTOs.Responses;

/// <summary>
/// Response DTO cho thống kê hệ thống về classes
/// </summary>
public class ClassStatisticsResponse
{
    public int TotalClasses { get; set; }
    public int ActiveClasses { get; set; }
    public int ArchivedClasses { get; set; }
    public int TotalStudentsEnrolled { get; set; }
    public int TotalTeachers { get; set; }
    public double AverageStudentsPerClass { get; set; }
    public ClassWithMostStudents? MostPopularClass { get; set; }
    public List<TeacherClassCount>? TopTeachers { get; set; }
}

public class ClassWithMostStudents
{
    public Guid ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public int StudentCount { get; set; }
}

public class TeacherClassCount
{
    public Guid TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public int ClassCount { get; set; }
}
