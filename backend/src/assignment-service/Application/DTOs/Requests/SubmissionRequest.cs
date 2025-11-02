namespace AssignmentService.Application.DTOs.Requests;

public class SubmissionRequest
{
    public string ProblemId { get; set; } = string.Empty;
    public string AssignmentStudentId { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
}