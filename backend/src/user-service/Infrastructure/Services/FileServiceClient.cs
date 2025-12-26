using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UserService.Application.Interfaces.Services;

namespace UserService.Infrastructure.Services;

/// <summary>
/// Client để gọi File Service API
/// </summary>
public class FileServiceClient : IFileServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<FileServiceClient> _logger;

    public FileServiceClient(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<FileServiceClient> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        var baseUrl = _configuration["ServiceUrls:FileService"] ?? "http://localhost:5004";
        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public async Task<FileUploadResult?> UploadFaceImageAsync(string userId, string imageBase64, string fileName)
    {
        try
        {
            _logger.LogInformation("Uploading face image for user: {UserId}", userId);

            // Convert base64 to bytes
            var imageData = ConvertBase64ToBytes(imageBase64);
            if (imageData == null)
            {
                return new FileUploadResult
                {
                    Success = false,
                    ErrorMessage = "Invalid base64 image data"
                };
            }

            // Create multipart form data
            using var content = new MultipartFormDataContent();
            
            var fileContent = new ByteArrayContent(imageData);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            content.Add(fileContent, "file", $"{userId}_face.jpg");

            // Build URL with query parameters: category=3 (Avatar) and fileName
            var url = $"/api/files/upload?category=Image&fileName={Uri.EscapeDataString(fileName)}";
            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogDebug("File service response: {StatusCode} - {Content}", 
                response.StatusCode, responseContent);

            if (!response.IsSuccessStatusCode)
            {
                return new FileUploadResult
                {
                    Success = false,
                    ErrorMessage = $"File service returned {response.StatusCode}"
                };
            }

            var apiResponse = JsonSerializer.Deserialize<FileServiceResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (apiResponse?.Success == true && apiResponse.Data != null)
            {
                _logger.LogInformation("✅ Face image uploaded successfully for user {UserId}: {Url}", 
                    userId, apiResponse.Data.FileUrl);

                return new FileUploadResult
                {
                    Success = true,
                    FileUrl = apiResponse.Data.FileUrl,
                    Key = apiResponse.Data.Key
                };
            }

            return new FileUploadResult
            {
                Success = false,
                ErrorMessage = apiResponse?.Message ?? "Unknown error"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error uploading face image for user: {UserId}", userId);
            return new FileUploadResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private byte[]? ConvertBase64ToBytes(string base64String)
    {
        try
        {
            // Remove data:image prefix if exists
            if (base64String.Contains(","))
            {
                base64String = base64String.Split(',')[1];
            }

            return Convert.FromBase64String(base64String);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting base64 to bytes");
            return null;
        }
    }
}

/// <summary>
/// Response từ File Service
/// </summary>
internal class FileServiceResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("data")]
    public FileServiceData? Data { get; set; }
}

internal class FileServiceData
{
    [JsonPropertyName("fileName")]
    public string? FileName { get; set; }

    [JsonPropertyName("fileUrl")]
    public string? FileUrl { get; set; }

    [JsonPropertyName("key")]
    public string? Key { get; set; }

    [JsonPropertyName("size")]
    public long Size { get; set; }

    [JsonPropertyName("contentType")]
    public string? ContentType { get; set; }
}
