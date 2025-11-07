namespace AssignmentService.Application.DTOs.Requests;

public class SubmissionRequest
{
    public string ProblemId { get; set; } = string.Empty;
    public string? AssignmentId { get; set; }
    public string SourceCode { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
}