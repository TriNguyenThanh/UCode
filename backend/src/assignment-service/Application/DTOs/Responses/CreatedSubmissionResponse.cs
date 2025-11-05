namespace AssignmentService.Application.DTOs.Responses;

public class CreateSubmissionResponse
{
    public string SubmissionId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}