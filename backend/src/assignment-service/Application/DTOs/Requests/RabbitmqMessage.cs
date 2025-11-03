using AssignmentService.Application.DTOs.Common;

public class RabbitMqMessage
{
    public string SubmissionId { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public int TimeLimit { get; set; } = 2000;
    public int MemoryLimit { get; set; } = 262144; // 256MB
    public List<TestCaseDto> Testcases { get; set; } = new List<TestCaseDto>();
}