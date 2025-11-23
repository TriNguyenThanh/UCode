namespace AssignmentService.Application.DTOs.Requests;

public class EmailQueueMessage
{
    public string To { get; set; } = string.Empty;
    public List<string> Bcc { get; set; } = new();
    public string Subject { get; set; } = string.Empty;
    public string HtmlContent { get; set; } = string.Empty;
}

public class BatchCreateAccountRequest
{
    public List<string> FullNames { get; set; } = new();
    public List<string> Emails { get; set; } = new();
    public string Password { get; set; } = string.Empty;
}

public class BatchAddStudentsToClassRequest
{
    public List<string> Emails { get; set; } = new();
    public string ClassName { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public string StartDate { get; set; } = string.Empty;
}