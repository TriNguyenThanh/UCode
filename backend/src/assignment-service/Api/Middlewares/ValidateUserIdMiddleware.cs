using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using AssignmentService.Application.DTOs.Common;

namespace AssignmentService.Api.Middlewares;

public class ValidateUserIdAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
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