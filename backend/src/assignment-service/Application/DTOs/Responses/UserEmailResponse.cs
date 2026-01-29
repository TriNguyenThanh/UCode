using System.Text.Json.Serialization;

namespace AssignmentService.Application.DTOs.Responses;

public class UserEmailResponse
{
    [JsonPropertyName("emails")]  // ✅ Map lowercase 'emails' từ JSON
    public List<string> Emails { get; set; } = new();
}