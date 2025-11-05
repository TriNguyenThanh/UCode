using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace file_service.Api.Middlewares;

public class GatewayRoleMiddleware
{
    private readonly RequestDelegate _next;
    private const string HeaderRole = "X-Role";
    private const string HeaderUserId = "X-User-Id";

    public GatewayRoleMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var roleHeader = context.Request.Headers[HeaderRole].ToString();
        var userIdHeader = context.Request.Headers[HeaderUserId].ToString();

        var claims = new List<Claim>();
        if (!string.IsNullOrWhiteSpace(userIdHeader))
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userIdHeader));

        if (!string.IsNullOrWhiteSpace(roleHeader))
        {
            var roles = roleHeader.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var r in roles) claims.Add(new Claim(ClaimTypes.Role, r.Trim()));
        }

        if (claims.Any())
        {
            var identity = new ClaimsIdentity(claims, "Gateway");
            context.User = new ClaimsPrincipal(identity);
        }

        // Optional: check endpoint metadata RequireRoleAttribute and short-circuit
        var endpoint = context.GetEndpoint();
        if (endpoint != null)
        {
            var require = endpoint.Metadata.GetMetadata<RequireRoleAttribute>();
            if (require != null)
            {
                // ✅ Support multiple roles separated by comma
                var requiredRoles = require.Role.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(r => r.Trim())
                    .ToList();
                
                // Check if user has ANY of the required roles
                var hasRequiredRole = requiredRoles.Any(role => context.User?.IsInRole(role) == true);
                
                if (!hasRequiredRole)
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsJsonAsync(
                        new
                        {
                            success = false,
                            message = "Forbidden: Missing required role.",
                            error = "Forbidden"
                        }
                    );
                    return;
                }
            }
        }

        await _next(context);
    }
}

// Simple metadata attribute to mark endpoints that require a role (middleware reads it)
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequireRoleAttribute : Attribute
{
    public string Role { get; }
    public RequireRoleAttribute(string role) => Role = role;
}