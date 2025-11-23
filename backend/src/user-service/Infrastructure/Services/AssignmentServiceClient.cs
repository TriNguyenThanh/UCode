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

    public async Task<bool> SyncDeleteUserAsync(Guid userId)
    {
        try
        {
            var json = JsonSerializer.Serialize(userId);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                "/api/v1/assignments/webhook/sync-delete-user", 
                content);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            _logger.LogWarning(
                "Failed to sync delete user. UserId: {UserId}, StatusCode: {StatusCode}", 
                userId, 
                response.StatusCode);

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex, 
                "Error syncing delete user. UserId: {UserId}", 
                userId);
            return false;
        }
    }

    /// <summary>
    /// Tạo tài khoản cho sinh viên
    /// </summary>
    /// <param name="fullNames">Tên đầy đủ của sinh viên</param>
    /// <param name="emails">Danh sách email của sinh viên</param>
    /// <param name="password">Mật khẩu tạm thời của sinh viên</param>
    /// <returns>Success message</returns>
    /// <response code="200">Account created successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="500">Failed to create account</response>
    public async Task<bool> SendCreatedAccountEmails(List<string> fullNames, List<string> emails, string password)
    {
        try
        {
            var request = new
            {
                fullNames = fullNames,
                emails = emails,
                password = password
            };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                "api/v1/email/queue-batch-create-accounts",
                content);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return false;
        }
    }
    
    /// <summary>
    /// Thêm 1 sinh viên vào lớp
    /// </summary>
    /// <param name="Emails">Danh sách email của sinh viên</param>
    /// <param name="ClassName">Tên lớp</param>
    /// <param name="TeacherName">Tên giảng viên</param>
    /// <param name="StartDate">Ngày bắt đầu lớp</param>
    /// <returns>Success message</returns>
    /// <response code="200">Account created successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="500">Failed to create account</response>
    public async Task<bool> SendAddedToClassEmails(List<string> Emails, string ClassName,  string TeacherName, DateTime StartDate)
    {
        try
        {
            var request = new
            {
                emails = Emails,
                className = ClassName,
                teacherName = TeacherName,
                startDate = StartDate
            };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                "api/v1/email/queue-batch-add-students-to-class",
                content);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return false;
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
