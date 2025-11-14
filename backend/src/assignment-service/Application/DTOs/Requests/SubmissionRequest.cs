namespace AssignmentService.Application.DTOs.Requests;

public class SubmissionRequest
{
    public Guid ProblemId { get; set; } = Guid.Empty;
    public Guid? AssignmentId { get; set; } = null;
    public string SourceCode { get; set; } = string.Empty;
    public Guid LanguageId { get; set; } = Guid.Empty;
}