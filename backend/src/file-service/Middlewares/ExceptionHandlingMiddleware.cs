using System.Net;
using System.Text.Json;
using file_service.Models;

namespace file_service.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = exception switch
        {
            ArgumentException => new
            {
                statusCode = (int)HttpStatusCode.BadRequest,
                response = ApiResponse<object>.ErrorResponse(exception.Message)
            },
            UnauthorizedAccessException => new
            {
                statusCode = (int)HttpStatusCode.Unauthorized,
                response = ApiResponse<object>.ErrorResponse("Unauthorized access")
            },
            FileNotFoundException => new
            {
                statusCode = (int)HttpStatusCode.NotFound,
                response = ApiResponse<object>.ErrorResponse("File not found")
            },
            _ => new
            {
                statusCode = (int)HttpStatusCode.InternalServerError,
                response = ApiResponse<object>.ErrorResponse(
                    "An error occurred while processing your request",
                    new List<string> { exception.Message }
                )
            }
        };

        context.Response.StatusCode = response.statusCode;
        
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        await context.Response.WriteAsync(JsonSerializer.Serialize(response.response, jsonOptions));
    }
}
