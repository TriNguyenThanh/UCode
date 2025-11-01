namespace AssignmentService.Application.DTOs.Common;

/// <summary>
/// Standard error response structure
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Indicates if the operation was successful
    /// </summary>
    /// <example>false</example>
    public bool Success { get; set; } = false;
    
    /// <summary>
    /// Error message describing what went wrong
    /// </summary>
    /// <example>An error occurred while processing your request</example>
    public string? Message { get; set; }
    
    /// <summary>
    /// List of detailed error messages
    /// </summary>
    /// <example>["Validation failed", "Required field is missing"]</example>
    public List<string>? Errors { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public ErrorResponse()
    {
        Success = false;
        Message = "An error occurred while processing your request";
    }

    /// <summary>
    /// Constructor with message
    /// </summary>
    /// <param name="message">Error message</param>
    public ErrorResponse(string message)
    {
        Success = false;
        Message = message;
    }

    /// <summary>
    /// Constructor with message and errors
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="errors">List of detailed errors</param>
    public ErrorResponse(string message, List<string> errors)
    {
        Success = false;
        Message = message;
        Errors = errors;
    }
}

/// <summary>
/// Validation error response with field-specific errors
/// </summary>
public class ValidationErrorResponse : ErrorResponse
{
    /// <summary>
    /// Dictionary of field names and their validation errors
    /// </summary>
    /// <example>{"Code": ["Code is required", "Code must not exceed 50 characters"]}</example>
    public Dictionary<string, List<string>>? FieldErrors { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public ValidationErrorResponse() : base()
    {
        Success = false;
        Message = "Validation failed";
    }

    /// <summary>
    /// Constructor with field errors
    /// </summary>
    /// <param name="fieldErrors">Dictionary of field errors</param>
    public ValidationErrorResponse(Dictionary<string, List<string>> fieldErrors) : base()
    {
        Success = false;
        Message = "Validation failed";
        FieldErrors = fieldErrors;
    }
}

/// <summary>
/// Unauthorized error response
/// </summary>
public class UnauthorizedErrorResponse : ErrorResponse
{
    /// <summary>
    /// Indicates the type of authentication error
    /// </summary>
    /// <example>INVALID_TOKEN</example>
    public string? AuthErrorType { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public UnauthorizedErrorResponse() : base()
    {
        Success = false;
        Message = "Authentication failed or token is invalid";
    }
}

/// <summary>
/// Forbidden error response
/// </summary>
public class ForbiddenErrorResponse : ErrorResponse
{
    /// <summary>
    /// Indicates the required permission or role
    /// </summary>
    /// <example>TEACHER_ROLE_REQUIRED</example>
    public string? RequiredPermission { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public ForbiddenErrorResponse() : base()
    {
        Success = false;
        Message = "You do not have permission to perform this action";
    }
}
