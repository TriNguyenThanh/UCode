using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ApiGateway.Middlewares
{
    public class JwtAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtAuthenticationMiddleware> _logger;

        public JwtAuthenticationMiddleware(
            RequestDelegate next,
            IConfiguration configuration,
            ILogger<JwtAuthenticationMiddleware> logger)
        {
            _next = next;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Skip authentication for public endpoints
                if (IsPublicEndpoint(context.Request.Path))
                {
                    await _next(context);
                    return;
                }

                // Extract JWT token from Authorization header
                var token = ExtractTokenFromHeader(context);

                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Missing JWT token for path: {Path}", context.Request.Path);
                    await ReturnUnauthorized(context, "Missing authentication token");
                    return;
                }

                // Validate and decode JWT token
                var (isValid, principal) = await ValidateJwtToken(token);

                if (!isValid || principal == null)
                {
                    _logger.LogWarning("Invalid JWT token for path: {Path}", context.Request.Path);
                    await ReturnUnauthorized(context, "Invalid authentication token");
                    return;
                }

                // Set user principal to context
                context.User = principal;

                // Extract user information from claims (same as ClaimsToHeadersMiddleware)
                var userId = principal.FindFirst("sub")?.Value
                          ?? principal.FindFirst("nameid")?.Value
                          ?? principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                var userRole = principal.FindFirst("role")?.Value
                            ?? principal.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                var userName = principal.FindFirst("name")?.Value
                            ?? principal.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Missing user ID in JWT token for path: {Path}", context.Request.Path);
                    await ReturnUnauthorized(context, "Invalid user information in token");
                    return;
                }

                // Add secure headers for downstream services
                context.Request.Headers["X-User-Id"] = userId;
                context.Request.Headers["X-Role"] = userRole?.ToLower();
                context.Request.Headers["X-User-Name"] = userName;


                // Add user info to context for logging
                context.Items["AuthenticatedUserId"] = userId;
                context.Items["AuthenticatedUserRole"] = userRole;

                _logger.LogInformation("Authenticated user {UserId} with role {Role} for path {Path}",
                    userId, userRole ?? "No Role", context.Request.Path);

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in JWT authentication middleware");
                await ReturnUnauthorized(context, "Authentication error");
            }
        }

        private string? ExtractTokenFromHeader(HttpContext context)
        {
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrEmpty(authHeader))
                return null;

            if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return authHeader.Substring("Bearer ".Length).Trim();
            }

            return null;
        }

        private Task<(bool IsValid, ClaimsPrincipal? Principal)> ValidateJwtToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured"));

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero // No tolerance for expired tokens
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

                if (validatedToken is not JwtSecurityToken jwtToken)
                {
                    return Task.FromResult<(bool, ClaimsPrincipal?)>((false, null));
                }

                return Task.FromResult((true, (ClaimsPrincipal?)principal));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "JWT token validation failed");
                return Task.FromResult<(bool, ClaimsPrincipal?)>((false, null));
            }
        }

        private bool IsPublicEndpoint(PathString path)
        {
            var publicPaths = new[]
            {
                "/swagger",
                "/health",
                "/api/v1/auth/login",
                "/api/v1/auth/register",
                "/api/v1/auth/forgot-password",
                "/api/v1/auth/request-reset-password",
                "/api/v1/auth/verify-email",
                "/api/v1/auth/refresh-token",
                "/api/v1/auth/verify-otp"
            };

            return publicPaths.Any(publicPath => path.StartsWithSegments(publicPath, StringComparison.OrdinalIgnoreCase));
        }

        private async Task ReturnUnauthorized(HttpContext context, string message)
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = "Unauthorized",
                message = message,
                timestamp = DateTime.UtcNow,
                path = context.Request.Path,
                success = false
            };

            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
        }
    }
}
