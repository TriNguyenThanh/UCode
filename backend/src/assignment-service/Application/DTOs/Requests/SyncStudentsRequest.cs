namespace AssignmentService.Application.DTOs.Requests;

public class SyncStudentsRequest
{
    public List<Guid> StudentIds { get; set; } = new();
}
