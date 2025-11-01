using System.Net.Http.Json;
using System.Text.Json;
using AssignmentService.Application.Interfaces.Services;
using AssignmentService.Infrastructure.EF;
using AssignmentService.Application.DTOs.Common;

namespace AssignmentService.Infrastructure.Services;

public class UserServiceClient : IUserServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly AssignmentDbContext _context;

    public UserServiceClient(HttpClient httpClient, AssignmentDbContext context)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(5);
        var baseUrl = Environment.GetEnvironmentVariable("UserService__BaseUrl") ?? "http://localhost:5001";

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new ApiException("UserService:BaseUrl environment variable is not set or empty. Please check your appsettings or environment configuration.");
        }

        _httpClient.BaseAddress = new Uri(baseUrl);
        _context = context;
    }

    public async Task<List<Guid>> GetStudentIdsByClassIdAsync(Guid classId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Kiểm tra BaseAddress
            if (_httpClient.BaseAddress == null)
            {
                throw new ApiException("UserService BaseAddress is not configured. Please check UserService:BaseUrl in appsettings.json");
            }

            var url = $"/api/v1/classes/{classId}/students";
            
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            // Đọc response như một object để có thể access các property
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            // Console.WriteLine($"Response content: {responseContent}");
            
            // Parse JSON response để lấy data array
            using var jsonDoc = JsonDocument.Parse(responseContent);
            var root = jsonDoc.RootElement;
            
            if (root.TryGetProperty("data", out var dataElement) && dataElement.ValueKind == JsonValueKind.Array)
            {
                var students = new List<StudentDto>();
                
                foreach (var studentElement in dataElement.EnumerateArray())
                {
                    var student = new StudentDto
                    {
                        Id = studentElement.GetProperty("id").GetGuid(),
                        Email = studentElement.TryGetProperty("email", out var emailProp) ? emailProp.GetString() ?? string.Empty : string.Empty,
                        EnrollmentYear = studentElement.TryGetProperty("enrollmentYear", out var yearProp) ? yearProp.GetInt32() : 0,
                        Major = studentElement.TryGetProperty("major", out var majorProp) ? majorProp.GetString() ?? string.Empty : string.Empty,
                        ClassYear = studentElement.TryGetProperty("classYear", out var classYearProp) ? classYearProp.GetInt32() : 0
                    };
                    students.Add(student);
                }
                
                return students.Select(s => s.Id).ToList();
            }
            
            return new List<Guid>();
        }
        catch (HttpRequestException ex)
        {
            throw new ApiException($"HTTP error getting students by class id: {ex.Message}");
        }
        catch (TaskCanceledException ex)
        {
            throw new ApiException($"Timeout getting students by class id: {ex.Message}");
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error getting students by class id: {ex.Message}");
        }
    }
}


