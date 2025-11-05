using System.Text.RegularExpressions;

namespace ApiGateway.Middlewares
{
    public class RequestValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestValidationMiddleware> _logger;

        public RequestValidationMiddleware(RequestDelegate next, ILogger<RequestValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip size validation for file upload endpoints
            if (IsFileUploadEndpoint(context.Request.Path))
            {
                await _next(context);
                return;
            }

            // Validate request size
            if (context.Request.ContentLength > 30 * 1024 * 1024) // 30MB limit
            {
                _logger.LogWarning("Request too large: {ContentLength} bytes from {RemoteIpAddress}", 
                    context.Request.ContentLength, context.Connection.RemoteIpAddress);
                
                context.Response.StatusCode = 413;
                await context.Response.WriteAsync("Request too large");
                return;
            }

            // Validate path for potential attacks
            var path = context.Request.Path.Value?.ToLowerInvariant();
            if (!string.IsNullOrEmpty(path))
            {
                // Check for path traversal attempts
                if (path.Contains("..") || path.Contains("//") || path.Contains("\\"))
                {
                    _logger.LogWarning("Potential path traversal attack from {RemoteIpAddress}: {Path}", 
                        context.Connection.RemoteIpAddress, path);
                    
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Invalid path");
                    return;
                }

                // Check for SQL injection patterns in path
                if (ContainsSqlInjectionPattern(path))
                {
                    _logger.LogWarning("Potential SQL injection attempt from {RemoteIpAddress}: {Path}", 
                        context.Connection.RemoteIpAddress, path);
                    
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Invalid request");
                    return;
                }
            }

            // Validate User-Agent header
            var userAgent = context.Request.Headers["User-Agent"].ToString();
            if (string.IsNullOrEmpty(userAgent) || userAgent.Length > 500)
            {
                _logger.LogWarning("Suspicious User-Agent from {RemoteIpAddress}: {UserAgent}", 
                    context.Connection.RemoteIpAddress, userAgent);
            }

            await _next(context);
        }

        private bool ContainsSqlInjectionPattern(string input)
        {
            var sqlPatterns = new[]
            {
                @"union\s+select",
                @"drop\s+table",
                @"delete\s+from",
                @"insert\s+into",
                @"update\s+set",
                @"exec\s*\(",
                @"sp_",
                @"xp_",
                @"'or\s+1=1",
                @"'or\s+'1'='1",
                @"--",
                @"/\*.*\*/"
            };

            foreach (var pattern in sqlPatterns)
            {
                if (Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsFileUploadEndpoint(PathString path)
        {
            var fileUploadPaths = new[]
            {
                "/api/files/upload",
                "/api/assignments/submit_code",
                "/api/assignments/run_code",
                "/api/problems/testcases/upload"
            };

            return fileUploadPaths.Any(uploadPath => 
                path.StartsWithSegments(uploadPath, StringComparison.OrdinalIgnoreCase));
        }
    }
}
