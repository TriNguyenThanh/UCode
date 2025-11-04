namespace AssignmentService.Application.DTOs.Responses;
public class ResultMessageResponse
{
    public Guid SubmissionId { get; set; }
    public string CompileResult { get; set; } = null!;
    public int TotalTime { get; set; }
    public int TotalMemory { get; set; }
    public string ErrorCode { get; set; } = null!;
    public string ErrorMessage { get; set; } = null!;
}