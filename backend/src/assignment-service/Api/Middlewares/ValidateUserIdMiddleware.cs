using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using AssignmentService.Application.DTOs.Common;

namespace AssignmentService.Api.Middlewares;

/// <summary>
/// Attribute to skip X-User-Id validation for internal service-to-service calls
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class SkipValidateUserIdAttribute : Attribute
{
}

public class ValidateUserIdAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // Skip validation if SkipValidateUserId attribute is present
        var skipValidation = context.ActionDescriptor.EndpointMetadata
            .Any(m => m is SkipValidateUserIdAttribute);

        if (skipValidation)
        {
            return;
        }

        var userIdHeader = context.HttpContext.Request.Headers["X-User-Id"].ToString();

        if (string.IsNullOrWhiteSpace(userIdHeader))
        {
            context.Result = new BadRequestObjectResult(
                ApiResponse<object>.ErrorResponse("Missing X-User-Id header")
            );
            return;
        }

        if (!Guid.TryParse(userIdHeader, out var userId))
        {
            context.Result = new BadRequestObjectResult(
                ApiResponse<object>.ErrorResponse("Invalid X-User-Id format. Must be a valid GUID.")
            );
            return;
        }

        // Lưu userId vào HttpContext.Items để sử dụng trong controller
        context.HttpContext.Items["X-User-Id"] = userId;
    }

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        // Không cần xử lý sau khi action thực thi
    }
}