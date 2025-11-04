using System.Collections.Concurrent;
using System.Net;

namespace ApiGateway.Middlewares
{
    public class CustomRateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CustomRateLimitMiddleware> _logger;
        private readonly ConcurrentDictionary<string, RateLimitInfo> _rateLimitStore = new();
        private readonly int _maxRequests;
        private readonly TimeSpan _window;

        public CustomRateLimitMiddleware(RequestDelegate next, ILogger<CustomRateLimitMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            _maxRequests = 100; // 100 requests per minute
            _window = TimeSpan.FromMinutes(1);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientIp = GetClientIpAddress(context);
            var key = $"{clientIp}:{DateTime.UtcNow:yyyy-MM-dd-HH-mm}";

            var rateLimitInfo = _rateLimitStore.GetOrAdd(key, _ => new RateLimitInfo
            {
                Count = 0,
                WindowStart = DateTime.UtcNow
            });

            // Clean up old entries
            CleanupOldEntries();

            // Check if rate limit exceeded
            if (rateLimitInfo.Count >= _maxRequests)
            {
                _logger.LogWarning("Rate limit exceeded for IP: {ClientIp}, Count: {Count}", 
                    clientIp, rateLimitInfo.Count);

                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.Response.ContentType = "application/json";
                
                var response = new
                {
                    error = "Too Many Requests",
                    message = "Rate limit exceeded. Please try again later.",
                    retryAfter = 60,
                    timestamp = DateTime.UtcNow
                };

                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
                return;
            }

            // Increment counter
            Interlocked.Increment(ref rateLimitInfo.Count);

            // Add rate limit headers
            context.Response.Headers.Append("X-RateLimit-Limit", _maxRequests.ToString());
            context.Response.Headers.Append("X-RateLimit-Remaining", Math.Max(0, _maxRequests - rateLimitInfo.Count).ToString());
            context.Response.Headers.Append("X-RateLimit-Reset", DateTime.UtcNow.Add(_window).ToString("R"));

            await _next(context);
        }

        private string GetClientIpAddress(HttpContext context)
        {
            // Check for forwarded IP first
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        private void CleanupOldEntries()
        {
            var cutoff = DateTime.UtcNow.AddMinutes(-5);
            var keysToRemove = _rateLimitStore
                .Where(kvp => kvp.Value.WindowStart < cutoff)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in keysToRemove)
            {
                _rateLimitStore.TryRemove(key, out _);
            }
        }
    }

    public class RateLimitInfo
    {
        public int Count;
        public DateTime WindowStart;
    }
}

