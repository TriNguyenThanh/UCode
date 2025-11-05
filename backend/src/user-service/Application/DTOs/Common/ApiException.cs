namespace UserService.Application.DTOs.Common;

/// <summary>
/// Custom exception for API errors
/// </summary>
public class ApiException : Exception
{
    public int StatusCode { get; set; }
    public List<string>? Errors { get; set; }

    public ApiException(string message, int statusCode = 400, List<string>? errors = null) 
        : base(message)
    {
        StatusCode = statusCode;
        Errors = errors;
    }
}

