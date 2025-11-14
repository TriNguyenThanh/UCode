namespace AssignmentService.Application.DTOs.Requests;

public class ActivityLogRequest
{
    public string ActivityType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? Metadata { get; set; }
    public int SuspicionLevel { get; set; }
}

public class ActivityLogBatchRequest
{
    public List<ActivityLogRequest> Activities { get; set; } = new();
}
