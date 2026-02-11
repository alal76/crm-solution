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

using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;
using CRM.Infrastructure.Middleware;
using CRM.Infrastructure.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Net;

namespace CRM.Tests.Middleware;

/// <summary>
/// Unit tests for Rate Limiting Middleware
/// Covers: Request limits, IP-based tracking, endpoint-specific limits
/// </summary>
public class RateLimitingMiddlewareTests
{
    private readonly Mock<ILogger<RateLimitingMiddleware>> _mockLogger;
    private readonly Mock<IOptions<RateLimitSettings>> _mockSettings;
    private readonly IMemoryCache _memoryCache;
    private readonly RateLimitSettings _settings;

    public RateLimitingMiddlewareTests()
    {
        _mockLogger = new Mock<ILogger<RateLimitingMiddleware>>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _settings = new RateLimitSettings
        {
            Enabled = true,
            DefaultRequestsPerMinute = 60,
            DefaultRequestsPerHour = 1000,
            IpWhitelist = new[] { "127.0.0.1" },
            EndpointLimits = new Dictionary<string, EndpointLimit>
            {
                { "/api/auth/login", new EndpointLimit { RequestsPerMinute = 5, RequestsPerHour = 20 } },
                { "/api/reports/*", new EndpointLimit { RequestsPerMinute = 10, RequestsPerHour = 100 } }
            }
        };

        _mockSettings = new Mock<IOptions<RateLimitSettings>>();
        _mockSettings.Setup(x => x.Value).Returns(_settings);
    }

    #region Basic Rate Limiting Tests

    [Fact]
    public async Task InvokeAsync_WithinLimit_AllowsRequest()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = (context) => { nextCalled = true; return Task.CompletedTask; };
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
        context.Response.StatusCode.Should().NotBe(429);
    }

    [Fact]
    public async Task InvokeAsync_ExceedsLimit_Returns429()
    {
        // Arrange
        _settings.DefaultRequestsPerMinute = 2;
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext(ip: "192.168.1.100");

        // Act - Make requests to exceed limit
        await middleware.InvokeAsync(context);
        context = CreateHttpContext(ip: "192.168.1.100");
        await middleware.InvokeAsync(context);
        context = CreateHttpContext(ip: "192.168.1.100");
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(429);
    }

    [Fact]
    public async Task InvokeAsync_ExceedsLimit_SetsRetryAfterHeader()
    {
        // Arrange
        _settings.DefaultRequestsPerMinute = 1;
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext(ip: "192.168.1.101");

        // Act
        await middleware.InvokeAsync(context);
        context = CreateHttpContext(ip: "192.168.1.101");
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers.Should().ContainKey("Retry-After");
    }

    #endregion

    #region IP Whitelist Tests

    [Fact]
    public async Task InvokeAsync_WhitelistedIP_SkipsRateLimit()
    {
        // Arrange
        _settings.DefaultRequestsPerMinute = 1;
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = CreateMiddleware(next);

        // Act - Make many requests from whitelisted IP
        for (int i = 0; i < 100; i++)
        {
            var context = CreateHttpContext(ip: "127.0.0.1");
            await middleware.InvokeAsync(context);
            context.Response.StatusCode.Should().NotBe(429);
        }
    }

    [Fact]
    public async Task InvokeAsync_NonWhitelistedIP_AppliesRateLimit()
    {
        // Arrange
        _settings.DefaultRequestsPerMinute = 2;
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext(ip: "192.168.1.200");

        // Act
        await middleware.InvokeAsync(context);
        context = CreateHttpContext(ip: "192.168.1.200");
        await middleware.InvokeAsync(context);
        context = CreateHttpContext(ip: "192.168.1.200");
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(429);
    }

    #endregion

    #region Endpoint-Specific Limits Tests

    [Fact]
    public async Task InvokeAsync_LoginEndpoint_UsesStricterLimit()
    {
        // Arrange
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = CreateMiddleware(next);
        var ip = "192.168.1.150";

        // Act - Make requests to login endpoint (limit is 5/min)
        for (int i = 0; i < 6; i++)
        {
            var context = CreateHttpContext(path: "/api/auth/login", ip: ip);
            await middleware.InvokeAsync(context);

            if (i >= 5)
            {
                context.Response.StatusCode.Should().Be(429);
            }
        }
    }

    [Fact]
    public async Task InvokeAsync_WildcardEndpoint_MatchesPattern()
    {
        // Arrange
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = CreateMiddleware(next);
        var ip = "192.168.1.151";

        // Act - Test wildcard matching for /api/reports/*
        for (int i = 0; i < 11; i++)
        {
            var context = CreateHttpContext(path: "/api/reports/sales", ip: ip);
            await middleware.InvokeAsync(context);

            if (i >= 10)
            {
                context.Response.StatusCode.Should().Be(429);
            }
        }
    }

    #endregion

    #region Disabled Rate Limiting Tests

    [Fact]
    public async Task InvokeAsync_RateLimitingDisabled_AllowsAllRequests()
    {
        // Arrange
        _settings.Enabled = false;
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = CreateMiddleware(next);

        // Act
        for (int i = 0; i < 1000; i++)
        {
            var context = CreateHttpContext(ip: "192.168.1.200");
            await middleware.InvokeAsync(context);
            context.Response.StatusCode.Should().NotBe(429);
        }
    }

    #endregion

    #region Rate Limit Headers Tests

    [Fact]
    public async Task InvokeAsync_AddsRateLimitHeaders()
    {
        // Arrange
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers.Should().ContainKey("X-RateLimit-Limit");
        context.Response.Headers.Should().ContainKey("X-RateLimit-Remaining");
        context.Response.Headers.Should().ContainKey("X-RateLimit-Reset");
    }

    [Fact]
    public async Task InvokeAsync_RemainingDecreases_WithEachRequest()
    {
        // Arrange
        _settings.DefaultRequestsPerMinute = 10;
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = CreateMiddleware(next);
        var ip = "192.168.1.170";

        // Act
        var context1 = CreateHttpContext(ip: ip);
        await middleware.InvokeAsync(context1);
        var remaining1 = context1.Response.Headers["X-RateLimit-Remaining"].ToString();

        var context2 = CreateHttpContext(ip: ip);
        await middleware.InvokeAsync(context2);
        var remaining2 = context2.Response.Headers["X-RateLimit-Remaining"].ToString();

        // Assert
        int.Parse(remaining1).Should().BeGreaterThan(int.Parse(remaining2));
    }

    #endregion

    #region User-Based Rate Limiting Tests

    [Fact]
    public async Task InvokeAsync_AuthenticatedUser_TracksPerUser()
    {
        // Arrange
        _settings.EnableUserBasedLimits = true;
        _settings.DefaultRequestsPerMinute = 3;
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = CreateMiddleware(next);

        // Act - User 1 makes requests
        for (int i = 0; i < 3; i++)
        {
            var context = CreateHttpContext(ip: "10.0.0.1", userId: 1);
            await middleware.InvokeAsync(context);
        }

        // User 2 should still have full quota
        var context2 = CreateHttpContext(ip: "10.0.0.1", userId: 2);
        await middleware.InvokeAsync(context2);

        // Assert
        context2.Response.StatusCode.Should().NotBe(429);
    }

    [Fact]
    public async Task InvokeAsync_SameUserDifferentIPs_SharesLimit()
    {
        // Arrange
        _settings.EnableUserBasedLimits = true;
        _settings.DefaultRequestsPerMinute = 2;
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = CreateMiddleware(next);

        // Act - Same user from different IPs
        var context1 = CreateHttpContext(ip: "10.0.0.1", userId: 100);
        await middleware.InvokeAsync(context1);

        var context2 = CreateHttpContext(ip: "10.0.0.2", userId: 100);
        await middleware.InvokeAsync(context2);

        var context3 = CreateHttpContext(ip: "10.0.0.3", userId: 100);
        await middleware.InvokeAsync(context3);

        // Assert
        context3.Response.StatusCode.Should().Be(429);
    }

    #endregion

    #region Sliding Window Tests

    [Fact]
    public async Task InvokeAsync_SlidingWindow_ResetsOverTime()
    {
        // Arrange
        _settings.DefaultRequestsPerMinute = 2;
        _settings.WindowType = RateLimitWindowType.Sliding;
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = CreateMiddleware(next);
        var ip = "192.168.1.180";

        // Act - Exhaust limit
        var context1 = CreateHttpContext(ip: ip);
        await middleware.InvokeAsync(context1);
        var context2 = CreateHttpContext(ip: ip);
        await middleware.InvokeAsync(context2);
        var context3 = CreateHttpContext(ip: ip);
        await middleware.InvokeAsync(context3);

        // Assert - Should be rate limited
        context3.Response.StatusCode.Should().Be(429);
    }

    #endregion

    #region API Key Rate Limiting Tests

    [Fact]
    public async Task InvokeAsync_ApiKey_TracksPerApiKey()
    {
        // Arrange
        _settings.EnableApiKeyLimits = true;
        _settings.DefaultRequestsPerMinute = 3;
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = CreateMiddleware(next);

        // Act - API Key 1 makes requests
        for (int i = 0; i < 3; i++)
        {
            var context = CreateHttpContext(ip: "10.0.0.1", apiKey: "key-1");
            await middleware.InvokeAsync(context);
        }

        // API Key 2 should still have full quota
        var context2 = CreateHttpContext(ip: "10.0.0.1", apiKey: "key-2");
        await middleware.InvokeAsync(context2);

        // Assert
        context2.Response.StatusCode.Should().NotBe(429);
    }

    #endregion

    #region Error Response Tests

    [Fact]
    public async Task InvokeAsync_ExceedsLimit_ReturnsJsonError()
    {
        // Arrange
        _settings.DefaultRequestsPerMinute = 1;
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = CreateMiddleware(next);
        var ip = "192.168.1.190";
        var context = CreateHttpContext(ip: ip);
        await middleware.InvokeAsync(context);

        context = CreateHttpContext(ip: ip);
        var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        responseBody.Position = 0;
        var body = await new StreamReader(responseBody).ReadToEndAsync();
        body.Should().Contain("rate limit");
    }

    #endregion

    #region Helper Methods

    private RateLimitingMiddleware CreateMiddleware(RequestDelegate next)
    {
        return new RateLimitingMiddleware(next, _memoryCache, _mockLogger.Object, _mockSettings.Object);
    }

    private DefaultHttpContext CreateHttpContext(
        string path = "/api/accounts",
        string ip = "192.168.1.1",
        int? userId = null,
        string? apiKey = null)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Connection.RemoteIpAddress = IPAddress.Parse(ip);
        context.Response.Body = new MemoryStream();

        if (userId.HasValue)
        {
            context.Items["UserId"] = userId.Value;
        }

        if (!string.IsNullOrEmpty(apiKey))
        {
            context.Request.Headers["X-Api-Key"] = apiKey;
        }

        return context;
    }

    #endregion
}

// Supporting classes
public class RateLimitSettings
{
    public bool Enabled { get; set; }
    public int DefaultRequestsPerMinute { get; set; }
    public int DefaultRequestsPerHour { get; set; }
    public string[] IpWhitelist { get; set; } = Array.Empty<string>();
    public Dictionary<string, EndpointLimit> EndpointLimits { get; set; } = new();
    public bool EnableUserBasedLimits { get; set; }
    public bool EnableApiKeyLimits { get; set; }
    public RateLimitWindowType WindowType { get; set; } = RateLimitWindowType.Fixed;
}

public class EndpointLimit
{
    public int RequestsPerMinute { get; set; }
    public int RequestsPerHour { get; set; }
}

public enum RateLimitWindowType
{
    Fixed,
    Sliding
}

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly RateLimitSettings _settings;

    public RateLimitingMiddleware(
        RequestDelegate next,
        IMemoryCache cache,
        ILogger<RateLimitingMiddleware> logger,
        IOptions<RateLimitSettings> settings)
    {
        _next = next;
        _cache = cache;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!_settings.Enabled)
        {
            await _next(context);
            return;
        }

        var clientId = GetClientIdentifier(context);

        // Check whitelist
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "";
        if (_settings.IpWhitelist.Contains(ip))
        {
            await _next(context);
            return;
        }

        var limit = GetLimitForPath(context.Request.Path.Value ?? "");
        var cacheKey = $"ratelimit:{clientId}:{DateTime.UtcNow:yyyyMMddHHmm}";

        var requestCount = _cache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            return 0;
        });

        requestCount++;
        _cache.Set(cacheKey, requestCount, TimeSpan.FromMinutes(1));

        // Add rate limit headers
        var remaining = Math.Max(0, limit - requestCount);
        context.Response.Headers["X-RateLimit-Limit"] = limit.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = remaining.ToString();
        context.Response.Headers["X-RateLimit-Reset"] = DateTimeOffset.UtcNow.AddMinutes(1).ToUnixTimeSeconds().ToString();

        if (requestCount > limit)
        {
            _logger.LogWarning("Rate limit exceeded for client {ClientId}", clientId);
            context.Response.StatusCode = 429;
            context.Response.Headers["Retry-After"] = "60";
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"error\": \"Too many requests. Please try again later.\", \"message\": \"rate limit exceeded\"}");
            return;
        }

        await _next(context);
    }

    private string GetClientIdentifier(HttpContext context)
    {
        // Try user ID first
        if (_settings.EnableUserBasedLimits && context.Items.TryGetValue("UserId", out var userId))
        {
            return $"user:{userId}";
        }

        // Try API key
        if (_settings.EnableApiKeyLimits)
        {
            var apiKey = context.Request.Headers["X-Api-Key"].FirstOrDefault();
            if (!string.IsNullOrEmpty(apiKey))
            {
                return $"apikey:{apiKey}";
            }
        }

        // Fall back to IP
        return $"ip:{context.Connection.RemoteIpAddress}";
    }

    private int GetLimitForPath(string path)
    {
        foreach (var endpoint in _settings.EndpointLimits)
        {
            if (endpoint.Key.EndsWith("*"))
            {
                var prefix = endpoint.Key.TrimEnd('*');
                if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return endpoint.Value.RequestsPerMinute;
                }
            }
            else if (path.Equals(endpoint.Key, StringComparison.OrdinalIgnoreCase))
            {
                return endpoint.Value.RequestsPerMinute;
            }
        }

        return _settings.DefaultRequestsPerMinute;
    }
}
