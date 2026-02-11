// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Concurrent;

namespace CRM.Api.Middleware;

/// <summary>
/// Middleware that implements rate limiting to protect the API from abuse.
/// Uses a sliding window algorithm with configurable limits per client.
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly RateLimitOptions _options;
    private readonly ConcurrentDictionary<string, ClientRateInfo> _clients = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitingMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware delegate.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="options">Optional rate limit configuration options.</param>
    public RateLimitingMiddleware(
        RequestDelegate next,
        ILogger<RateLimitingMiddleware> logger,
        RateLimitOptions? options = null)
    {
        _next = next;
        _logger = logger;
        _options = options ?? new RateLimitOptions();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip rate limiting for health checks and internal endpoints
        if (ShouldSkipRateLimiting(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var clientId = GetClientIdentifier(context);
        var now = DateTime.UtcNow;

        var clientInfo = _clients.AddOrUpdate(
            clientId,
            _ => new ClientRateInfo { RequestCount = 1, WindowStart = now },
            (_, existing) =>
            {
                // Reset window if expired
                if (now - existing.WindowStart > _options.WindowDuration)
                {
                    return new ClientRateInfo { RequestCount = 1, WindowStart = now };
                }

                existing.RequestCount++;
                return existing;
            });

        // Check if rate limit exceeded
        if (clientInfo.RequestCount > _options.MaxRequestsPerWindow)
        {
            _logger.LogWarning(
                "Rate limit exceeded for client {ClientId}. Requests: {RequestCount}/{MaxRequests}",
                clientId, clientInfo.RequestCount, _options.MaxRequestsPerWindow);

            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers["Retry-After"] = GetRetryAfterSeconds(clientInfo, now).ToString();
            context.Response.Headers["X-RateLimit-Limit"] = _options.MaxRequestsPerWindow.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = "0";
            context.Response.Headers["X-RateLimit-Reset"] = GetResetTimestamp(clientInfo).ToString();

            await context.Response.WriteAsJsonAsync(new
            {
                error = "Rate limit exceeded",
                message = $"Too many requests. Please try again in {GetRetryAfterSeconds(clientInfo, now)} seconds.",
                retryAfter = GetRetryAfterSeconds(clientInfo, now)
            });
            return;
        }

        // Add rate limit headers
        var remaining = Math.Max(0, _options.MaxRequestsPerWindow - clientInfo.RequestCount);
        context.Response.Headers["X-RateLimit-Limit"] = _options.MaxRequestsPerWindow.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = remaining.ToString();
        context.Response.Headers["X-RateLimit-Reset"] = GetResetTimestamp(clientInfo).ToString();

        await _next(context);

        // Cleanup old entries periodically
        if (_clients.Count > 10000)
        {
            CleanupOldEntries(now);
        }
    }

    private bool ShouldSkipRateLimiting(PathString path)
    {
        var skipPaths = new[]
        {
            "/health",
            "/health/ready",
            "/health/live",
            "/metrics",
            "/swagger",
            "/favicon.ico"
        };

        return skipPaths.Any(p => path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase));
    }

    private string GetClientIdentifier(HttpContext context)
    {
        // Try to get client IP from X-Forwarded-For header (for proxied requests)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',').First().Trim();
        }

        // Fall back to connection remote IP
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private int GetRetryAfterSeconds(ClientRateInfo clientInfo, DateTime now)
    {
        var windowEnd = clientInfo.WindowStart + _options.WindowDuration;
        return Math.Max(1, (int)(windowEnd - now).TotalSeconds);
    }

    private long GetResetTimestamp(ClientRateInfo clientInfo)
    {
        var windowEnd = clientInfo.WindowStart + _options.WindowDuration;
        return new DateTimeOffset(windowEnd).ToUnixTimeSeconds();
    }

    private void CleanupOldEntries(DateTime now)
    {
        var expiredKeys = _clients
            .Where(kvp => now - kvp.Value.WindowStart > _options.WindowDuration * 2)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _clients.TryRemove(key, out _);
        }
    }

    private class ClientRateInfo
    {
        public int RequestCount { get; set; }
        public DateTime WindowStart { get; set; }
    }
}

/// <summary>
/// Configuration options for rate limiting.
/// </summary>
public class RateLimitOptions
{
    /// <summary>
    /// Maximum number of requests allowed per time window. Default: 100.
    /// </summary>
    public int MaxRequestsPerWindow { get; set; } = 100;

    /// <summary>
    /// Duration of the rate limiting window. Default: 1 minute.
    /// </summary>
    public TimeSpan WindowDuration { get; set; } = TimeSpan.FromMinutes(1);
}

/// <summary>
/// Extension methods for RateLimitingMiddleware.
/// </summary>
public static class RateLimitingMiddlewareExtensions
{
    /// <summary>
    /// Adds rate limiting middleware to the application pipeline.
    /// </summary>
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder, RateLimitOptions? options = null)
    {
        return builder.UseMiddleware<RateLimitingMiddleware>(options ?? new RateLimitOptions());
    }
}
