using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Net;
using System.Threading.Tasks;

namespace CRM.Api.Middleware
{
    /// <summary>
    /// Middleware to implement rate limiting per IP address or user.
    /// </summary>
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly int _requestLimit;
        private readonly TimeSpan _timeWindow;

        public RateLimitingMiddleware(
            RequestDelegate next,
            IMemoryCache cache,
            int requestLimit = 100,
            int timeWindowSeconds = 60)
        {
            _next = next;
            _cache = cache;
            _requestLimit = requestLimit;
            _timeWindow = TimeSpan.FromSeconds(timeWindowSeconds);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip rate limiting for health checks and swagger
            var path = context.Request.Path.Value?.ToLower() ?? string.Empty;
            if (path.Contains("/health") || 
                path.Contains("/swagger") || 
                path.Contains("/api-docs"))
            {
                await _next(context);
                return;
            }

            // Get client identifier (user or IP)
            var clientId = GetClientIdentifier(context);

            if (string.IsNullOrEmpty(clientId))
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.Response.WriteAsync("Unable to identify client for rate limiting");
                return;
            }

            var cacheKey = $"rate_limit_{clientId}";
            
            // Get current request count
            var requestInfo = _cache.GetOrCreate(cacheKey, entry =>
            {
                entry.SetAbsoluteExpiration(_timeWindow);
                return new RateLimitInfo
                {
                    Count = 0,
                    WindowStart = DateTime.UtcNow
                };
            });

            // Check if rate limit exceeded
            if (requestInfo.Count >= _requestLimit)
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.Headers.Add("X-Rate-Limit-Limit", _requestLimit.ToString());
                context.Response.Headers.Add("X-Rate-Limit-Remaining", "0");
                context.Response.Headers.Add("X-Rate-Limit-Reset", 
                    requestInfo.WindowStart.Add(_timeWindow).ToString("o"));
                
                await context.Response.WriteAsync(
                    $"Rate limit exceeded. Maximum {_requestLimit} requests per {_timeWindow.TotalSeconds} seconds.");
                return;
            }

            // Increment counter
            requestInfo.Count++;
            _cache.Set(cacheKey, requestInfo, _timeWindow);

            // Add rate limit headers to response
            context.Response.Headers.Add("X-Rate-Limit-Limit", _requestLimit.ToString());
            context.Response.Headers.Add("X-Rate-Limit-Remaining", 
                (_requestLimit - requestInfo.Count).ToString());
            context.Response.Headers.Add("X-Rate-Limit-Reset", 
                requestInfo.WindowStart.Add(_timeWindow).ToString("o"));

            await _next(context);
        }

        private string GetClientIdentifier(HttpContext context)
        {
            // Prefer authenticated user identity
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                return context.User.Identity.Name ?? string.Empty;
            }

            // Fall back to IP address
            var ipAddress = context.Connection.RemoteIpAddress;
            
            // Check for forwarded IP (behind proxy/load balancer)
            if (context.Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                var forwardedFor = context.Request.Headers["X-Forwarded-For"].ToString();
                if (!string.IsNullOrEmpty(forwardedFor))
                {
                    var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    if (ips.Length > 0 && IPAddress.TryParse(ips[0].Trim(), out var parsedIp))
                    {
                        ipAddress = parsedIp;
                    }
                }
            }

            return ipAddress?.ToString() ?? "unknown";
        }

        private class RateLimitInfo
        {
            public int Count { get; set; }
            public DateTime WindowStart { get; set; }
        }
    }
}
