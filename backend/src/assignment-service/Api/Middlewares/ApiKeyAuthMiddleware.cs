using System.Net;
using System.Text.Json;
using AssignmentService.Application.DTOs.Common;

namespace AssignmentService.Api.Middlewares;

/// <summary>
/// Middleware xác thực API Key cho internal services
/// </summary>
public class ApiKeyAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiKeyAuthMiddleware> _logger;
    private const string API_KEY_HEADER_NAME = "X-Internal-Api-Key";

    public ApiKeyAuthMiddleware(RequestDelegate next, ILogger<ApiKeyAuthMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Chỉ áp dụng cho các endpoint webhook/internal
        if (!context.Request.Path.StartsWithSegments("/api/v1/webhooks"))
        {
            await _next(context);
            return;
        }

        // Lấy API key từ header
        if (!context.Request.Headers.TryGetValue(API_KEY_HEADER_NAME, out var extractedApiKey))
        {
            _logger.LogWarning("API Key missing for webhook endpoint: {Path}", context.Request.Path);
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Response.ContentType = "application/json";
            
            var response = ApiResponse<object>.ErrorResponse("API Key is missing");
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            return;
        }

        // Lấy API key từ configuration
        var expectedApiKey = Environment.GetEnvironmentVariable("INTERNAL_API_KEY") 
            ?? "ucode-internal-service-key-2024"; // Default key for development

        // Validate API key
        if (extractedApiKey != expectedApiKey)
        {
            _logger.LogWarning("Invalid API Key attempt for webhook endpoint: {Path}", context.Request.Path);
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            context.Response.ContentType = "application/json";
            
            var response = ApiResponse<object>.ErrorResponse("Invalid API Key");
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            return;
        }

        await _next(context);
    }
}
