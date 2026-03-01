// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Middleware;

/// <summary>
/// Unit tests for Error Handling Middleware
/// Covers: Exception handling, error responses, logging
/// </summary>
public class ErrorHandlingMiddlewareTests
{
    private readonly Mock<ILogger<ErrorHandlingMiddleware>> _mockLogger;
    private readonly Mock<IHostEnvironment> _mockEnvironment;

    public ErrorHandlingMiddlewareTests()
    {
        _mockLogger = new Mock<ILogger<ErrorHandlingMiddleware>>();
        _mockEnvironment = new Mock<IHostEnvironment>();
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Production");
    }

    #region Success Path Tests

    [Fact]
    public async Task InvokeAsync_NoException_CallsNext()
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
        context.Response.StatusCode.Should().Be(200);
    }

    #endregion

    #region Validation Exception Tests

    [Fact]
    public async Task InvokeAsync_ValidationException_Returns400()
    {
        // Arrange
        RequestDelegate next = (context) => throw new ValidationException("Validation failed");
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task InvokeAsync_ValidationException_ReturnsErrorMessage()
    {
        // Arrange
        RequestDelegate next = (context) => throw new ValidationException("Email is required");
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();
        var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var response = await GetResponseBody(responseBody);
        response.Should().Contain("Email is required");
    }

    [Fact]
    public async Task InvokeAsync_ValidationExceptionWithErrors_ReturnsAllErrors()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            { "Email", new[] { "Email is required", "Email format invalid" } },
            { "Name", new[] { "Name is required" } }
        };
        RequestDelegate next = (context) => throw new ValidationException("Validation failed", errors);
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();
        var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var response = await GetResponseBody(responseBody);
        response.Should().Contain("Email");
        response.Should().Contain("Name");
    }

    #endregion

    #region Not Found Exception Tests

    [Fact]
    public async Task InvokeAsync_NotFoundException_Returns404()
    {
        // Arrange
        RequestDelegate next = (context) => throw new NotFoundException("Account", 123);
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task InvokeAsync_NotFoundException_ReturnsResourceInfo()
    {
        // Arrange
        RequestDelegate next = (context) => throw new NotFoundException("Account", 123);
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();
        var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var response = await GetResponseBody(responseBody);
        response.Should().Contain("Account");
        response.Should().Contain("123");
    }

    #endregion

    #region Unauthorized Exception Tests

    [Fact]
    public async Task InvokeAsync_UnauthorizedException_Returns401()
    {
        // Arrange
        RequestDelegate next = (context) => throw new UnauthorizedException("Invalid credentials");
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task InvokeAsync_UnauthorizedException_SetsWWWAuthHeader()
    {
        // Arrange
        RequestDelegate next = (context) => throw new UnauthorizedException("Invalid token");
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers.Should().ContainKey("WWW-Authenticate");
    }

    #endregion

    #region Forbidden Exception Tests

    [Fact]
    public async Task InvokeAsync_ForbiddenException_Returns403()
    {
        // Arrange
        RequestDelegate next = (context) => throw new ForbiddenException("Access denied");
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(403);
    }

    #endregion

    #region Conflict Exception Tests

    [Fact]
    public async Task InvokeAsync_ConflictException_Returns409()
    {
        // Arrange
        RequestDelegate next = (context) => throw new ConflictException("Email already exists");
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(409);
    }

    #endregion

    #region Business Rule Exception Tests

    [Fact]
    public async Task InvokeAsync_BusinessRuleException_Returns422()
    {
        // Arrange
        RequestDelegate next = (context) => throw new BusinessRuleException("Cannot delete account with active opportunities");
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(422);
    }

    #endregion

    #region Generic Exception Tests

    [Fact]
    public async Task InvokeAsync_GenericException_Returns500()
    {
        // Arrange
        RequestDelegate next = (context) => throw new Exception("Something went wrong");
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task InvokeAsync_GenericException_Production_HidesDetails()
    {
        // Arrange
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Production");
        RequestDelegate next = (context) => throw new Exception("Sensitive database error");
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();
        var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var response = await GetResponseBody(responseBody);
        response.Should().NotContain("database");
        response.Should().Contain("error");
    }

    [Fact]
    public async Task InvokeAsync_GenericException_Development_ShowsDetails()
    {
        // Arrange
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Development");
        RequestDelegate next = (context) => throw new Exception("Debug error info");
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();
        var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var response = await GetResponseBody(responseBody);
        response.Should().Contain("Debug error info");
    }

    #endregion

    #region Logging Tests

    [Fact]
    public async Task InvokeAsync_Exception_LogsError()
    {
        // Arrange
        RequestDelegate next = (context) => throw new Exception("Test error");
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

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

    [Fact]
    public async Task InvokeAsync_ValidationException_LogsWarning()
    {
        // Arrange
        RequestDelegate next = (context) => throw new ValidationException("Validation error");
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

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

    #region Response Format Tests

    [Fact]
    public async Task InvokeAsync_Exception_SetsContentTypeJson()
    {
        // Arrange
        RequestDelegate next = (context) => throw new Exception("Error");
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.ContentType.Should().Contain("application/json");
    }

    [Fact]
    public async Task InvokeAsync_Exception_ReturnsValidJson()
    {
        // Arrange
        RequestDelegate next = (context) => throw new NotFoundException("Item", 1);
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();
        var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var response = await GetResponseBody(responseBody);
        var action = () => JsonDocument.Parse(response);
        action.Should().NotThrow();
    }

    [Fact]
    public async Task InvokeAsync_Exception_IncludesTraceId()
    {
        // Arrange
        RequestDelegate next = (context) => throw new Exception("Error");
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();
        context.TraceIdentifier = "test-trace-123";
        var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var response = await GetResponseBody(responseBody);
        response.Should().Contain("test-trace-123");
    }

    #endregion

    #region Aggregate Exception Tests

    [Fact]
    public async Task InvokeAsync_AggregateException_HandlesInnerException()
    {
        // Arrange
        RequestDelegate next = (context) =>
        {
            throw new AggregateException(new ValidationException("Inner validation error"));
        };
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(400);
    }

    #endregion

    #region Task Cancellation Tests

    [Fact]
    public async Task InvokeAsync_TaskCancelledException_DoesNotLog()
    {
        // Arrange
        RequestDelegate next = (context) => throw new TaskCanceledException("Request cancelled");
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        // Task cancellations should be handled gracefully without error logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_OperationCancelledException_DoesNotLog()
    {
        // Arrange
        RequestDelegate next = (context) => throw new OperationCanceledException("Operation cancelled");
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        // Operation cancellations should be handled gracefully
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    #endregion

    #region Helper Methods

    private ErrorHandlingMiddleware CreateMiddleware(RequestDelegate next)
    {
        return new ErrorHandlingMiddleware(next, _mockLogger.Object, _mockEnvironment.Object);
    }

    private DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private async Task<string> GetResponseBody(MemoryStream stream)
    {
        stream.Position = 0;
        return await new StreamReader(stream).ReadToEndAsync();
    }

    #endregion
}

// Supporting exception classes
public class ValidationException : Exception
{
    public Dictionary<string, string[]>? Errors { get; }

    public ValidationException(string message) : base(message) { }
    public ValidationException(string message, Dictionary<string, string[]> errors) : base(message)
    {
        Errors = errors;
    }
}

public class NotFoundException : Exception
{
    public string ResourceType { get; }
    public object ResourceId { get; }

    public NotFoundException(string resourceType, object resourceId)
        : base($"{resourceType} with ID {resourceId} not found")
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }
}

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message) { }
}

public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message) { }
}

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}

public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}

// Middleware implementation
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ErrorHandlingMiddleware(
        RequestDelegate next,
        ILogger<ErrorHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (TaskCanceledException)
        {
            // Request cancelled - ignore
        }
        catch (OperationCanceledException)
        {
            // Operation cancelled - ignore
        }
        catch (AggregateException ae) when (ae.InnerException != null)
        {
            await HandleException(context, ae.InnerException);
        }
        catch (Exception ex)
        {
            await HandleException(context, ex);
        }
    }

    private async Task HandleException(HttpContext context, Exception exception)
    {
        var (statusCode, logLevel) = exception switch
        {
            ValidationException => (400, LogLevel.Warning),
            NotFoundException => (404, LogLevel.Warning),
            UnauthorizedException => (401, LogLevel.Warning),
            ForbiddenException => (403, LogLevel.Warning),
            ConflictException => (409, LogLevel.Warning),
            BusinessRuleException => (422, LogLevel.Warning),
            _ => (500, LogLevel.Error)
        };

        _logger.Log(logLevel, exception, "Request error: {Message}", exception.Message);

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        if (statusCode == 401)
        {
            context.Response.Headers["WWW-Authenticate"] = "Bearer";
        }

        var response = CreateErrorResponse(context, exception, statusCode);
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private object CreateErrorResponse(HttpContext context, Exception exception, int statusCode)
    {
        var response = new Dictionary<string, object>
        {
            ["error"] = statusCode == 500 && _environment.IsProduction()
                ? "An error occurred processing your request"
                : exception.Message,
            ["statusCode"] = statusCode,
            ["traceId"] = context.TraceIdentifier
        };

        if (exception is ValidationException ve && ve.Errors != null)
        {
            response["errors"] = ve.Errors;
        }

        if (exception is NotFoundException nfe)
        {
            response["resourceType"] = nfe.ResourceType;
            response["resourceId"] = nfe.ResourceId;
        }

        if (!_environment.IsProduction())
        {
            response["stackTrace"] = exception.StackTrace ?? string.Empty;
        }

        return response;
    }
}
