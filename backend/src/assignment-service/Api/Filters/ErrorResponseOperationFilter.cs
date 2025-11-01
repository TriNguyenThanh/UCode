using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using AssignmentService.Application.DTOs.Common;

namespace AssignmentService.Api.Filters;

/// <summary>
/// Operation filter to add standard error responses to all endpoints
/// </summary>
public class ErrorResponseOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Add standard error responses only if they don't already exist
        var errorResponses = new Dictionary<string, (string Description, Type ResponseType)>
        {
            ["400"] = ("Bad Request - Invalid request data or validation failed", typeof(ErrorResponse)),
            ["401"] = ("Unauthorized - Authentication required", typeof(UnauthorizedErrorResponse)),
            ["403"] = ("Forbidden - Insufficient permissions", typeof(ForbiddenErrorResponse)),
            ["404"] = ("Not Found - Resource not found", typeof(ErrorResponse)),
            ["409"] = ("Conflict - Resource already exists or conflict with current state", typeof(ErrorResponse)),
            ["422"] = ("Unprocessable Entity - Validation errors", typeof(ValidationErrorResponse)),
            ["500"] = ("Internal Server Error - Unexpected server error", typeof(ErrorResponse))
        };

        foreach (var (statusCode, (description, responseType)) in errorResponses)
        {
            if (!operation.Responses.ContainsKey(statusCode))
            {
                operation.Responses.Add(statusCode, new OpenApiResponse
                {
                    Description = description,
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new OpenApiMediaType
                        {
                            Schema = context.SchemaGenerator.GenerateSchema(responseType, context.SchemaRepository)
                        }
                    }
                });
            }
        }
    }
}
