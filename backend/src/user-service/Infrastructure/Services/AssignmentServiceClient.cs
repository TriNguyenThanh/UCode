using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UserService.Application.Interfaces.Services;

namespace UserService.Infrastructure.Services;

public class AssignmentServiceClient : IAssignmentServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AssignmentServiceClient> _logger;

    public AssignmentServiceClient(
        HttpClient httpClient, 
        IConfiguration configuration,
        ILogger<AssignmentServiceClient> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        var baseUrl = _configuration["ServiceUrls:AssignmentService"] ?? "http://localhost:5002";
        _httpClient.BaseAddress = new Uri(baseUrl);
    }

    public async Task<int> SyncStudentsToClassAssignmentsAsync(Guid classId, List<Guid> studentIds)
    {
        try
        {
            var request = new { StudentIds = studentIds };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                $"/api/v1/assignments/classes/{classId}/students/sync", 
                content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<SyncResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result?.Data?.AssignmentUsersCreated ?? 0;
            }

            _logger.LogWarning(
                "Failed to sync students to assignments. ClassId: {ClassId}, StatusCode: {StatusCode}", 
                classId, 
                response.StatusCode);

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex, 
                "Error syncing students to assignments. ClassId: {ClassId}, StudentCount: {StudentCount}", 
                classId, 
                studentIds.Count);

            // Don't throw - just log and return 0
            // This is a background sync, we don't want to block the main operation
            return 0;
        }
    }

    private class SyncResponse
    {
        public SyncData? Data { get; set; }
    }

    private class SyncData
    {
        public int AssignmentUsersCreated { get; set; }
    }
}
