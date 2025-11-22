using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Sockets;

namespace UserService.Api.Middlewares
{
    public class IpAddressMiddleware
    {
        private readonly RequestDelegate _next;

        public IpAddressMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientIp = GetClientIpv4(context);
            if (!string.IsNullOrEmpty(clientIp))
            {
                context.Request.Headers["X-Client-IP"] = clientIp;
                context.Items["ClientIp"] = clientIp;
            }

            await _next(context);
        }

        private string? GetClientIpv4(HttpContext context)
        {
            // ✅ BƯỚC 1: ƯU TIÊN ĐỌC TỪ X-Forwarded-For (từ API Gateway)
            // IP đầu tiên trong danh sách là IP thật của client
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                var ips = forwardedFor.Split(',');
                var firstIp = ips[0].Trim(); // IP đầu tiên là IP gốc của client
                
                if (IPAddress.TryParse(firstIp, out var parsedIp))
                {
                    // Kiểm tra nếu là IPv4
                    if (parsedIp.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return parsedIp.ToString();
                    }
                    // Kiểm tra nếu là IPv6 mapped to IPv4
                    if (parsedIp.AddressFamily == AddressFamily.InterNetworkV6 && parsedIp.IsIPv4MappedToIPv6)
                    {
                        return parsedIp.MapToIPv4().ToString();
                    }
                }
            }

            // ✅ BƯỚC 2: Thử đọc từ X-Real-IP
            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp) && IPAddress.TryParse(realIp, out var realParsed))
            {
                if (realParsed.AddressFamily == AddressFamily.InterNetwork)
                {
                    return realParsed.ToString();
                }
                if (realParsed.AddressFamily == AddressFamily.InterNetworkV6 && realParsed.IsIPv4MappedToIPv6)
                {
                    return realParsed.MapToIPv4().ToString();
                }
            }

            // ✅ BƯỚC 3: Cuối cùng mới dùng RemoteIpAddress
            var remoteIp = context.Connection.RemoteIpAddress;
            if (remoteIp == null)
                return null;

            // If it's IPv4, return it
            if (remoteIp.AddressFamily == AddressFamily.InterNetwork)
            {
                return remoteIp.ToString();
            }

            // Check if IPv6 is IPv4-mapped (::ffff:x.x.x.x)
            if (remoteIp.AddressFamily == AddressFamily.InterNetworkV6 && remoteIp.IsIPv4MappedToIPv6)
            {
                return remoteIp.MapToIPv4().ToString();
            }

            // Nếu không tìm thấy IPv4, trả về IPv6 (fallback)
            return remoteIp.ToString();
        }
    }
}