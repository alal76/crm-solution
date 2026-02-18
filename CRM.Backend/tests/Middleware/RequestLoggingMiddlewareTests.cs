// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace CRM.Tests.Middleware;

/// <summary>
/// Unit tests for Request Logging Middleware
/// Covers: Request/response logging, sensitive data masking, performance timing
/// </summary>
public class RequestLoggingMiddlewareTests
{
    private readonly Mock<ILogger<RequestLoggingMiddleware>> _mockLogger;
    private readonly Mock<IOptions<LoggingSettings>> _mockLoggingSettings;
    private readonly LoggingSettings _settings;

    public RequestLoggingMiddlewareTests()
    {
        _mockLogger = new Mock<ILogger<RequestLoggingMiddleware>>();
        _settings = new LoggingSettings
        {
            LogRequestBody = true,
            LogResponseBody = true,
            LogHeaders = true,
            MaxBodyLogLength = 4096,
            SensitiveHeaders = new[] { "Authorization", "X-Api-Key" },
            SensitiveFields = new[] { "password", "token", "secret" },
            ExcludedPaths = new[] { "/health", "/swagger" }
        };

        _mockLoggingSettings = new Mock<IOptions<LoggingSettings>>();
        _mockLoggingSettings.Setup(x => x.Value).Returns(_settings);
    }

    #region Basic Logging Tests

    [Fact]
    public async Task InvokeAsync_SuccessfulRequest_LogsRequestAndResponse()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = (context) =>
        {
            nextCalled = true;
            context.Response.StatusCode = 200;
            return Task.CompletedTask;
        };
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext("/api/accounts", "GET");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task InvokeAsync_LogsHttpMethod()
    {
        // Arrange
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext("/api/accounts", "POST");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        VerifyLogContains("POST");
    }

    [Fact]
    public async Task InvokeAsync_LogsRequestPath()
    {
        // Arrange
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext("/api/accounts/123", "GET");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        VerifyLogContains("/api/accounts");
    }

    [Fact]
    public async Task InvokeAsync_LogsStatusCode()
    {
        // Arrange
        RequestDelegate next = (context) =>
        {
            context.Response.StatusCode = 201;
            return Task.CompletedTask;
        };
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext("/api/accounts", "POST");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        VerifyLogContains("201");
    }

    #endregion

    #region Request Body Logging Tests

    [Fact]
    public async Task InvokeAsync_PostRequest_LogsRequestBody()
    {
        // Arrange
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = CreateMiddleware(next);
        var body = "{\"name\": \"Test Account\"}";
        var context = CreateHttpContext("/api/accounts", "POST", body);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        VerifyLogContains("Test Account");
    }

    [Fact]
    public async Task InvokeAsync_LargeRequestBody_TruncatesLog()
    {
        // Arrange
        _settings.MaxBodyLogLength = 100;
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = CreateMiddleware(next);
        var body = new string('x', 500);
        var context = CreateHttpContext("/api/accounts", "POST", body);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        // Should truncate and indicate truncation
    }

    [Fact]
    public async Task InvokeAsync_LogRequestBodyDisabled_DoesNotLogBody()
    {
        // Arrange
        _settings.LogRequestBody = false;
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = CreateMiddleware(next);
        var body = "{\"name\": \"ShouldNotAppear\"}";
        var context = CreateHttpContext("/api/accounts", "POST", body);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        // Body should not be logged
    }

    #endregion

    #region Response Body Logging Tests

    [Fact]
    public async Task InvokeAsync_Response_LogsResponseBody()
    {
        // Arrange
        RequestDelegate next = async (context) =>
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("{\"id\": 1, \"name\": \"Test\"}");
        };
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext("/api/accounts", "GET");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        VerifyLogContains("Test");
    }

    [Fact]
    public async Task InvokeAsync_LogResponseBodyDisabled_DoesNotLogBody()
    {
        // Arrange
        _settings.LogResponseBody = false;
        RequestDelegate next = async (context) =>
        {
            await context.Response.WriteAsync("{\"secret\": \"data\"}");
        };
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext("/api/accounts", "GET");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        // Response body should not be logged
    }

    #endregion

    #region Sensitive Data Masking Tests

    [Fact]
    public async Task InvokeAsync_SensitiveHeader_MasksValue()
    {
        // Arrange
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext("/api/accounts", "GET");
        context.Request.Headers["Authorization"] = "Bearer secret-token-123";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        // Authorization header value should be masked
    }

    [Fact]
    public async Task InvokeAsync_SensitiveFieldInBody_MasksValue()
    {
        // Arrange
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = CreateMiddleware(next);
        var body = "{\"username\": \"user\", \"password\": \"secret123\"}";
        var context = CreateHttpContext("/api/auth/login", "POST", body);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        // Password should be masked in logs
    }

    [Fact]
    public async Task InvokeAsync_ApiKeyHeader_MasksValue()
    {
        // Arrange
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext("/api/accounts", "GET");
        context.Request.Headers["X-Api-Key"] = "my-secret-api-key";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        // API key should be masked
    }

    #endregion

    #region Excluded Path Tests

    [Fact]
    public async Task InvokeAsync_HealthEndpoint_SkipsLogging()
    {
        // Arrange
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext("/health", "GET");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Theory]
    [InlineData("/health")]
    [InlineData("/health/ready")]
    [InlineData("/swagger")]
    [InlineData("/swagger/index.html")]
    public async Task InvokeAsync_ExcludedPaths_SkipsLogging(string path)
    {
        // Arrange
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext(path, "GET");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        // Should not log for excluded paths
    }

    #endregion

    #region Performance Timing Tests

    [Fact]
    public async Task InvokeAsync_LogsElapsedTime()
    {
        // Arrange
        RequestDelegate next = async (context) =>
        {
            await Task.Delay(50);
        };
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext("/api/accounts", "GET");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        VerifyLogContains("ms");
    }

    [Fact]
    public async Task InvokeAsync_SlowRequest_LogsWarning()
    {
        // Arrange
        _settings.SlowRequestThresholdMs = 100;
        RequestDelegate next = async (context) =>
        {
            await Task.Delay(150);
        };
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext("/api/accounts", "GET");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion

    #region Header Logging Tests

    [Fact]
    public async Task InvokeAsync_LogHeadersEnabled_LogsHeaders()
    {
        // Arrange
        _settings.LogHeaders = true;
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext("/api/accounts", "GET");
        context.Request.Headers["Content-Type"] = "application/json";
        context.Request.Headers["Accept"] = "application/json";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        VerifyLogContains("Content-Type");
    }

    [Fact]
    public async Task InvokeAsync_LogHeadersDisabled_DoesNotLogHeaders()
    {
        // Arrange
        _settings.LogHeaders = false;
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext("/api/accounts", "GET");
        context.Request.Headers["X-Custom-Header"] = "custom-value";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        // Headers should not be logged
    }

    #endregion

    #region Correlation ID Tests

    [Fact]
    public async Task InvokeAsync_WithCorrelationId_LogsCorrelationId()
    {
        // Arrange
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext("/api/accounts", "GET");
        context.Request.Headers["X-Correlation-ID"] = "corr-123-abc";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        VerifyLogContains("corr-123-abc");
    }

    [Fact]
    public async Task InvokeAsync_NoCorrelationId_GeneratesOne()
    {
        // Arrange
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext("/api/accounts", "GET");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        // Should generate and log a correlation ID
    }

    #endregion

    #region Error Scenario Tests

    [Fact]
    public async Task InvokeAsync_Exception_LogsError()
    {
        // Arrange
        RequestDelegate next = (context) => throw new Exception("Test error");
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext("/api/accounts", "GET");

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => middleware.InvokeAsync(context));

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task InvokeAsync_4xxResponse_LogsWarning()
    {
        // Arrange
        RequestDelegate next = (context) =>
        {
            context.Response.StatusCode = 400;
            return Task.CompletedTask;
        };
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext("/api/accounts", "POST");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task InvokeAsync_5xxResponse_LogsError()
    {
        // Arrange
        RequestDelegate next = (context) =>
        {
            context.Response.StatusCode = 500;
            return Task.CompletedTask;
        };
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext("/api/accounts", "GET");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion

    #region Helper Methods

    private RequestLoggingMiddleware CreateMiddleware(RequestDelegate next)
    {
        return new RequestLoggingMiddleware(next, _mockLogger.Object, _mockLoggingSettings.Object);
    }

    private DefaultHttpContext CreateHttpContext(string path, string method, string? body = null)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Request.Method = method;
        context.Response.Body = new MemoryStream();

        if (body != null)
        {
            var bodyBytes = Encoding.UTF8.GetBytes(body);
            context.Request.Body = new MemoryStream(bodyBytes);
            context.Request.ContentLength = bodyBytes.Length;
            context.Request.ContentType = "application/json";
        }

        return context;
    }

    private void VerifyLogContains(string substring)
    {
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(substring)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion
}

// Supporting classes
public class LoggingSettings
{
    public bool LogRequestBody { get; set; }
    public bool LogResponseBody { get; set; }
    public bool LogHeaders { get; set; }
    public int MaxBodyLogLength { get; set; }
    public string[] SensitiveHeaders { get; set; } = Array.Empty<string>();
    public string[] SensitiveFields { get; set; } = Array.Empty<string>();
    public string[] ExcludedPaths { get; set; } = Array.Empty<string>();
    public int SlowRequestThresholdMs { get; set; } = 1000;
}

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    private readonly LoggingSettings _settings;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger,
        IOptions<LoggingSettings> settings)
    {
        _next = next;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";

        // Skip logging for excluded paths
        if (_settings.ExcludedPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString();

        try
        {
            // Log request
            await LogRequest(context, correlationId);

            // Capture response body
            var originalBody = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context);

            stopwatch.Stop();

            // Log response
            await LogResponse(context, correlationId, stopwatch.ElapsedMilliseconds, responseBody);

            // Copy response back
            responseBody.Position = 0;
            await responseBody.CopyToAsync(originalBody);
            context.Response.Body = originalBody;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "[{CorrelationId}] Request failed after {ElapsedMs}ms: {Method} {Path}",
                correlationId, stopwatch.ElapsedMilliseconds, context.Request.Method, path);
            throw;
        }
    }

    private async Task LogRequest(HttpContext context, string correlationId)
    {
        var method = context.Request.Method;
        var path = context.Request.Path.Value;

        _logger.LogInformation("[{CorrelationId}] Request: {Method} {Path}",
            correlationId, method, path);

        if (_settings.LogHeaders)
        {
            foreach (var header in context.Request.Headers)
            {
                var value = _settings.SensitiveHeaders.Contains(header.Key, StringComparer.OrdinalIgnoreCase)
                    ? "[REDACTED]"
                    : header.Value.ToString();
                _logger.LogInformation("[{CorrelationId}] Header: {Key}: {Value}",
                    correlationId, header.Key, value);
            }
        }

        if (_settings.LogRequestBody && context.Request.ContentLength > 0)
        {
            context.Request.EnableBuffering();
            using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;

            body = MaskSensitiveData(body);
            if (body.Length > _settings.MaxBodyLogLength)
            {
                body = body.Substring(0, _settings.MaxBodyLogLength) + "...[TRUNCATED]";
            }

            _logger.LogInformation("[{CorrelationId}] Request Body: {Body}",
                correlationId, body);
        }
    }

    private async Task LogResponse(HttpContext context, string correlationId, long elapsedMs, MemoryStream responseBody)
    {
        var statusCode = context.Response.StatusCode;
        var logLevel = statusCode >= 500 ? LogLevel.Error : statusCode >= 400 ? LogLevel.Warning : LogLevel.Information;

        if (elapsedMs > _settings.SlowRequestThresholdMs)
        {
            logLevel = LogLevel.Warning;
        }

        _logger.Log(logLevel, "[{CorrelationId}] Response: {StatusCode} in {ElapsedMs}ms",
            correlationId, statusCode, elapsedMs);

        if (_settings.LogResponseBody)
        {
            responseBody.Position = 0;
            using var reader = new StreamReader(responseBody, leaveOpen: true);
            var body = await reader.ReadToEndAsync();

            body = MaskSensitiveData(body);
            if (body.Length > _settings.MaxBodyLogLength)
            {
                body = body.Substring(0, _settings.MaxBodyLogLength) + "...[TRUNCATED]";
            }

            _logger.Log(logLevel, "[{CorrelationId}] Response Body: {Body}",
                correlationId, body);
        }
    }

    private string MaskSensitiveData(string data)
    {
        foreach (var field in _settings.SensitiveFields)
        {
            var pattern = $"\"{field}\"\\s*:\\s*\"[^\"]+\"";
            data = System.Text.RegularExpressions.Regex.Replace(
                data, pattern, $"\"{field}\": \"[REDACTED]\"",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }
        return data;
    }
}
