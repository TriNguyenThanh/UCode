using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UserService.Application.Interfaces.Services;

namespace UserService.Infrastructure.Services;

/// <summary>
/// Client để gọi Face Recognition Service API
/// </summary>
public class FaceServiceClient : IFaceServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<FaceServiceClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public FaceServiceClient(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<FaceServiceClient> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        var baseUrl = _configuration["ServiceUrls:FaceService"] ?? "http://localhost:5003";
        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(30);

        _httpClient.DefaultRequestHeaders.Add("ngrok-skip-browser-warning", "true");

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<FaceRegisterResult> RegisterFaceAsync(string userId, string imageBase64)
    {
        try
        {
            _logger.LogInformation("Registering face for user: {UserId}", userId);

            var request = new
            {
                user_id = userId,
                image = imageBase64
            };

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("/register", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogDebug("Face service response: {StatusCode} - {Content}", 
                response.StatusCode, responseContent);

            var apiResponse = JsonSerializer.Deserialize<FaceApiResponse>(responseContent, _jsonOptions);

            if (apiResponse == null)
            {
                return new FaceRegisterResult
                {
                    Success = false,
                    Message = "Invalid response from face service"
                };
            }

            var result = new FaceRegisterResult
            {
                Success = apiResponse.Success,
                Message = apiResponse.Message
            };

            if (apiResponse.Success && apiResponse.Data != null)
            {
                result.UserId = apiResponse.Data.UserId;
                result.Confidence = apiResponse.Data.Confidence;
                result.ImageFilename = apiResponse.Data.ImageFilename;
            }
            else if (!apiResponse.Success && apiResponse.Data != null)
            {
                // Error case - face already registered
                result.MatchedUserId = apiResponse.Data.MatchedUserId;
                result.Similarity = apiResponse.Data.Similarity;
            }

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error calling face service for user: {UserId}", userId);
            return new FaceRegisterResult
            {
                Success = false,
                Message = $"Face service unavailable: {ex.Message}"
            };
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout calling face service for user: {UserId}", userId);
            return new FaceRegisterResult
            {
                Success = false,
                Message = "Face service timeout"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering face for user: {UserId}", userId);
            return new FaceRegisterResult
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    public async Task<FaceVerifyResult> VerifyFaceAsync(string userId, string imageBase64, float threshold = 0.6f)
    {
        try
        {
            _logger.LogInformation("Verifying face for user: {UserId}", userId);

            var request = new
            {
                user_id = userId,
                image = imageBase64,
                threshold = threshold
            };

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("/verify", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            var apiResponse = JsonSerializer.Deserialize<FaceApiResponse>(responseContent, _jsonOptions);

            if (apiResponse == null)
            {
                return new FaceVerifyResult
                {
                    Success = false,
                    Message = "Invalid response from face service"
                };
            }

            return new FaceVerifyResult
            {
                Success = apiResponse.Success,
                Message = apiResponse.Message,
                IsMatch = apiResponse.Data?.IsMatch ?? false,
                Similarity = apiResponse.Data?.Similarity ?? 0,
                Threshold = apiResponse.Data?.Threshold ?? threshold
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying face for user: {UserId}", userId);
            return new FaceVerifyResult
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    public async Task<bool> DeleteFaceAsync(string userId)
    {
        try
        {
            _logger.LogInformation("Deleting face for user: {UserId}", userId);

            var request = new HttpRequestMessage(HttpMethod.Delete, "/delete")
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new { user_id = userId }),
                    Encoding.UTF8,
                    "application/json")
            };

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            var apiResponse = JsonSerializer.Deserialize<FaceApiResponse>(responseContent, _jsonOptions);

            if (apiResponse?.Success == true)
            {
                _logger.LogInformation("Successfully deleted face for user: {UserId}", userId);
                return true;
            }

            _logger.LogWarning("Failed to delete face for user: {UserId} - {Message}", 
                userId, apiResponse?.Message);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting face for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<FaceServiceHealthResult?> HealthCheckAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/health");
            var responseContent = await response.Content.ReadAsStringAsync();

            var apiResponse = JsonSerializer.Deserialize<FaceApiResponse>(responseContent, _jsonOptions);

            if (apiResponse?.Success == true && apiResponse.Data != null)
            {
                return new FaceServiceHealthResult
                {
                    Status = apiResponse.Data.Status ?? "unknown",
                    Detector = apiResponse.Data.Detector ?? "unknown",
                    Recognizer = apiResponse.Data.Recognizer ?? "unknown",
                    RegisteredFaces = apiResponse.Data.RegisteredFaces ?? 0
                };
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking face service health");
            return null;
        }
    }
}

/// <summary>
/// Response format từ Face Service
/// </summary>
internal class FaceApiResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public FaceApiData? Data { get; set; }
}

internal class FaceApiData
{
    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }

    [JsonPropertyName("confidence")]
    public float? Confidence { get; set; }

    [JsonPropertyName("image_filename")]
    public string? ImageFilename { get; set; }

    [JsonPropertyName("matched_user_id")]
    public string? MatchedUserId { get; set; }

    [JsonPropertyName("similarity")]
    public float? Similarity { get; set; }

    [JsonPropertyName("threshold")]
    public float? Threshold { get; set; }

    [JsonPropertyName("is_match")]
    public bool? IsMatch { get; set; }

    // Health check fields
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("detector")]
    public string? Detector { get; set; }

    [JsonPropertyName("recognizer")]
    public string? Recognizer { get; set; }

    [JsonPropertyName("registered_faces")]
    public int? RegisteredFaces { get; set; }
}
