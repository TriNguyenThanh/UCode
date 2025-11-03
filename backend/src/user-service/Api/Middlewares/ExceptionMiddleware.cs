using System.Net;
using System.Text.Json;
using UserService.Application.DTOs.Common;

namespace UserService.Api.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            ApiException apiEx => new
            {
                statusCode = apiEx.StatusCode,
                response = ApiResponse<object>.ErrorResponse(apiEx.Message, apiEx.Errors)
            },
            _ => new
            {
                statusCode = (int)HttpStatusCode.InternalServerError,
                response = ApiResponse<object>.ErrorResponse(
                    "An internal server error occurred. Please try again later.",
                    new List<string> { exception.Message }
                )
            }
        };

        context.Response.StatusCode = response.statusCode;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        return context.Response.WriteAsJsonAsync(response.response, jsonOptions);
    }
}

