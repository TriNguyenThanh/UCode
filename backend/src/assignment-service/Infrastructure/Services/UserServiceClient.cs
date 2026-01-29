using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using AssignmentService.Application.Interfaces.Services;
using AssignmentService.Infrastructure.EF;
using AssignmentService.Application.DTOs.Common;
using AssignmentService.Application.DTOs.Requests;
using AssignmentService.Application.DTOs.Responses;

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

    public async Task<List<Guid>> GetUserIdsByClassIdAsync(Guid classId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Kiểm tra BaseAddress
            if (_httpClient.BaseAddress == null)
            {
                throw new ApiException("UserService BaseAddress is not configured. Please check UserService:BaseUrl in appsettings.json");
            }

            var url = $"/api/v1/webhooks/class/{classId}/students";
            var apiKey = Environment.GetEnvironmentVariable("INTERNAL_API_KEY") ?? "ucode-internal-service-key-2024";
            
            // Add internal API key header
            _httpClient.DefaultRequestHeaders.Remove("X-Internal-Api-Key");
            _httpClient.DefaultRequestHeaders.Add("X-Internal-Api-Key", apiKey);

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
                        Id = studentElement.GetProperty("userId").GetGuid(),
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
    public async Task<List<string>> GetUserEmailByIdAsync(List<Guid> userIds)
    {
        try
        {
            var url = $"/api/v1/webhooks/get-emails";
            var apiKey = Environment.GetEnvironmentVariable("INTERNAL_API_KEY") ?? "ucode-internal-service-key-2024";
            
            var requestBody = new UserIdRequest
            {
                Ids = userIds
            };
            
            var json = JsonSerializer.Serialize(requestBody);
            Console.WriteLine($"[→] Request to {_httpClient.BaseAddress}{url}");
            Console.WriteLine($"[→] Request JSON: {json}");
            
            // Add internal API key header
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("X-Internal-Api-Key", apiKey);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.SendAsync(request);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[←] Response status: {response.StatusCode}");
            Console.WriteLine($"[←] Response JSON: {responseContent}");
            
            // ✅ Check HTTP status first
            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException($"User service returned {response.StatusCode}: {responseContent}");
            }
            
            // ✅ Deserialize with options (case-insensitive)
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            
            var responseData = JsonSerializer.Deserialize<ApiResponse<UserEmailResponse>>(
                responseContent, 
                options);
            
            // ✅ NULL checks
            if (responseData == null)
            {
                throw new ApiException("Failed to deserialize response from user service (responseData is null)");
            }
            
            if (!responseData.Success)
            {
                throw new ApiException($"User service returned error: {responseData.Message}");
            }
            
            if (responseData.Data == null)
            {
                Console.WriteLine("⚠️ User service returned success but Data is null");
                return new List<string>();
            }
            
            if (responseData.Data.Emails == null)
            {
                Console.WriteLine("⚠️ User service returned success but Emails array is null");
                return new List<string>();
            }
            
            Console.WriteLine($"[✅] Retrieved {responseData.Data.Emails.Count} emails");
            return responseData.Data.Emails;
        }
        catch (HttpRequestException ex)
        {
            throw new ApiException($"HTTP error getting user email by id: {ex.Message}");
        }
        catch (TaskCanceledException ex)
        {
            throw new ApiException($"Timeout getting user email by id: {ex.Message}");
        }
        catch (JsonException ex)
        {
            throw new ApiException($"JSON error getting user email by id: {ex.Message}");
        }
        catch (ApiException)
        {
            throw; // Re-throw ApiException as-is
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error getting user email by id: {ex.Message}");
        }
    }
}