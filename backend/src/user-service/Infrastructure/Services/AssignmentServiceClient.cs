using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UserService.Application.Interfaces.Services;
using UserService.Application.Interfaces.MessageBrokers;
using UserService.Application.Events;

namespace UserService.Infrastructure.Services;

public class AssignmentServiceClient : IAssignmentServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AssignmentServiceClient> _logger;
    private readonly IRabbitMqService _rabbitMqService;

    public AssignmentServiceClient(
        HttpClient httpClient, 
        IConfiguration configuration,
        ILogger<AssignmentServiceClient> logger,
        IRabbitMqService rabbitMqService)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _rabbitMqService = rabbitMqService;

        var baseUrl = _configuration["ServiceUrls:AssignmentService"] ?? "http://localhost:5002";
        _httpClient.BaseAddress = new Uri(baseUrl);
    }

    public async Task<int> SyncStudentsToClassAssignmentsAsync(Guid classId, List<Guid> studentIds)
    {
        try
        {
            // Publish event to RabbitMQ instead of HTTP call
            var @event = new StudentsAddedToClassEvent
            {
                ClassId = classId,
                StudentIds = studentIds,
                OccurredAt = DateTime.UtcNow
            };

            await _rabbitMqService.PublishMessageAsync(@event, "user_service.students_added");

            _logger.LogInformation(
                "✅ Published StudentsAddedToClass event. ClassId: {ClassId}, StudentCount: {StudentCount}",
                classId,
                studentIds.Count);

            // Since this is async, we can't return the actual count
            // Return expected count for backwards compatibility
            return studentIds.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex, 
                "❌ Error publishing StudentsAddedToClass event. ClassId: {ClassId}, StudentCount: {StudentCount}", 
                classId, 
                studentIds.Count);

            return 0;
        }
    }

    public async Task<bool> SyncDeleteUserAsync(Guid userId)
    {
        try
        {
            // Publish event to RabbitMQ instead of HTTP call
            var @event = new UserDeletedEvent
            {
                UserId = userId,
                OccurredAt = DateTime.UtcNow
            };

            // hiện tại không xóa assignment_user khi xóa user khỏi class

            await _rabbitMqService.PublishMessageAsync(@event, "user_service.user_deleted");

            _logger.LogInformation(
                "✅ Published UserDeleted event. UserId: {UserId}",
                userId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex, 
                "❌ Error publishing UserDeleted event. UserId: {UserId}", 
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
