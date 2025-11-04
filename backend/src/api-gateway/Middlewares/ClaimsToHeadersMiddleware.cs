using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ApiGateway.Middlewares
{
    public class ClaimsToHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ClaimsToHeadersMiddleware> _logger;

        public ClaimsToHeadersMiddleware(RequestDelegate next, ILogger<ClaimsToHeadersMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                // Lấy claims từ JWT token
                var userId = context.User.FindFirst("sub")?.Value
                          ?? context.User.FindFirst("nameid")?.Value
                          ?? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                var role = context.User.FindFirst("role")?.Value
                        ?? context.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                var userName = context.User.FindFirst("name")?.Value
                            ?? context.User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

                context.Request.Headers["X-User-Id"] = userId;
                context.Request.Headers["X-Role"] = role.ToLower();
                context.Request.Headers["X-User-Name"] = userName;
            }
            else
            {
                _logger.LogWarning("User is not authenticated");
            }

            await _next(context);
        }
    }
}
