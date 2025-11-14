namespace UserService.Application.DTOs.Requests;

public class BulkUserActionRequest
{
    public string Action { get; set; } = string.Empty; // activate, deactivate, delete, changeRole
    public List<string> UserIds { get; set; } = new();
    public string? NewRole { get; set; } // for changeRole: Admin, Teacher, Student
    public string? Reason { get; set; } // optional reason for action
}
