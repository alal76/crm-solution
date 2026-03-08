// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using CRM.Core.Exceptions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using RealMiddleware = CRM.Api.Middleware.ErrorHandlingMiddleware;

namespace CRM.Tests.Middleware;

/// <summary>
/// AP-032: Integration tests for the real ErrorHandlingMiddleware using real CRM.Core.Exceptions typed exceptions.
/// Verifies that each typed domain exception maps to the correct HTTP status code.
/// </summary>
public class GlobalExceptionHandlerMiddlewareTests
{
    private readonly Mock<ILogger<RealMiddleware>> _mockLogger;

    public GlobalExceptionHandlerMiddlewareTests()
    {
        _mockLogger = new Mock<ILogger<RealMiddleware>>();
    }

    #region AP-032: EntityNotFoundException → 404

    [Fact]
    public async Task InvokeAsync_EntityNotFoundException_Returns404()
    {
        // Arrange — AP-032: EntityNotFoundException should map to HTTP 404
        RequestDelegate next = _ => throw new EntityNotFoundException("Account", 42);
        var middleware = new RealMiddleware(next, _mockLogger.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task InvokeAsync_EntityNotFoundException_ResponseBodyContainsEntityInfo()
    {
        // Arrange
        RequestDelegate next = _ => throw new EntityNotFoundException("Lead", 99);
        var middleware = new RealMiddleware(next, _mockLogger.Object);
        var context = CreateHttpContext();
        var body = new MemoryStream();
        context.Response.Body = body;

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var json = await ReadBodyAsync(body);
        json.Should().Contain("Lead");
        json.Should().Contain("99");
        json.Should().Contain("ENTITY_NOT_FOUND");
    }

    [Fact]
    public async Task InvokeAsync_EntityNotFoundException_ContentTypeIsJson()
    {
        // Arrange
        RequestDelegate next = _ => throw new EntityNotFoundException("Contact", 1);
        var middleware = new RealMiddleware(next, _mockLogger.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.ContentType.Should().Be("application/json");
    }

    #endregion

    #region AP-032: ValidationException → 400

    [Fact]
    public async Task InvokeAsync_ValidationException_Returns400()
    {
        // Arrange — AP-032: ValidationException should map to HTTP 400
        RequestDelegate next = _ => throw new CRM.Core.Exceptions.ValidationException("Name is required");
        var middleware = new RealMiddleware(next, _mockLogger.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task InvokeAsync_ValidationException_ResponseBodyContainsValidationError()
    {
        // Arrange
        RequestDelegate next = _ => throw new CRM.Core.Exceptions.ValidationException("Email", "must be a valid email");
        var middleware = new RealMiddleware(next, _mockLogger.Object);
        var context = CreateHttpContext();
        var body = new MemoryStream();
        context.Response.Body = body;

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var json = await ReadBodyAsync(body);
        json.Should().Contain("VALIDATION_ERROR");
    }

    [Fact]
    public async Task InvokeAsync_ValidationException_ResponseBodyContainsErrors()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            { "Email", new[] { "required", "invalid format" } },
            { "Phone", new[] { "invalid format" } }
        };
        RequestDelegate next = _ => throw new CRM.Core.Exceptions.ValidationException("Validation failed", errors);
        var middleware = new RealMiddleware(next, _mockLogger.Object);
        var context = CreateHttpContext();
        var body = new MemoryStream();
        context.Response.Body = body;

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var json = await ReadBodyAsync(body);
        json.Should().Contain("Email");
        json.Should().Contain("Phone");
    }

    #endregion

    #region AP-032: AuthorizationException → 403

    [Fact]
    public async Task InvokeAsync_AuthorizationException_Returns403()
    {
        // Arrange — AP-032: AuthorizationException should map to HTTP 403
        RequestDelegate next = _ => throw new AuthorizationException("You do not have permission to perform this action");
        var middleware = new RealMiddleware(next, _mockLogger.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task InvokeAsync_AuthorizationException_ResponseBodyContainsAccessDenied()
    {
        // Arrange
        RequestDelegate next = _ => throw new AuthorizationException("Forbidden", "delete:accounts");
        var middleware = new RealMiddleware(next, _mockLogger.Object);
        var context = CreateHttpContext();
        var body = new MemoryStream();
        context.Response.Body = body;

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var json = await ReadBodyAsync(body);
        json.Should().Contain("ACCESS_DENIED");
    }

    #endregion

    #region AP-032: ConcurrencyException → 409

    [Fact]
    public async Task InvokeAsync_ConcurrencyException_Returns409()
    {
        // Arrange — AP-032: ConcurrencyException should map to HTTP 409
        RequestDelegate next = _ => throw new ConcurrencyException("Opportunity", 7);
        var middleware = new RealMiddleware(next, _mockLogger.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task InvokeAsync_DuplicateEntityException_Returns409()
    {
        // Arrange — AP-032: DuplicateEntityException should also map to HTTP 409
        RequestDelegate next = _ => throw new DuplicateEntityException("Contact", new[] { 1, 2 });
        var middleware = new RealMiddleware(next, _mockLogger.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(409);
    }

    #endregion

    #region AP-032: BusinessRuleException → 422

    [Fact]
    public async Task InvokeAsync_BusinessRuleException_Returns422()
    {
        // Arrange — AP-032: BusinessRuleException should map to HTTP 422 Unprocessable Entity
        RequestDelegate next = _ => throw new CRM.Core.Exceptions.BusinessRuleException("RecallRequest", "Can only recall pending requests");
        var middleware = new RealMiddleware(next, _mockLogger.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(422);
    }

    #endregion

    #region AP-032: AuthenticationException → 401

    [Fact]
    public async Task InvokeAsync_AuthenticationException_Returns401()
    {
        // Arrange — AP-032: AuthenticationException should map to HTTP 401
        RequestDelegate next = _ => throw new AuthenticationException("Authentication required");
        var middleware = new RealMiddleware(next, _mockLogger.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(401);
    }

    #endregion

    #region AP-032: RateLimitException → 429

    [Fact]
    public async Task InvokeAsync_RateLimitException_Returns429()
    {
        // Arrange — AP-032: RateLimitException should map to HTTP 429
        RequestDelegate next = _ => throw new RateLimitException(60);
        var middleware = new RealMiddleware(next, _mockLogger.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(429);
    }

    [Fact]
    public async Task InvokeAsync_RateLimitException_ResponseBodyContainsRetryAfter()
    {
        // Arrange
        RequestDelegate next = _ => throw new RateLimitException(30);
        var middleware = new RealMiddleware(next, _mockLogger.Object);
        var context = CreateHttpContext();
        var body = new MemoryStream();
        context.Response.Body = body;

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var json = await ReadBodyAsync(body);
        json.Should().Contain("30");
        json.Should().Contain("RATE_LIMIT_EXCEEDED");
    }

    #endregion

    #region AP-032: Unhandled Exception → 500

    [Fact]
    public async Task InvokeAsync_UnhandledException_Returns500()
    {
        // Arrange — AP-032: Any unhandled exception should map to HTTP 500
        RequestDelegate next = _ => throw new InvalidOperationException("Unexpected failure");
        var middleware = new RealMiddleware(next, _mockLogger.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task InvokeAsync_UnhandledException_ResponseBodyContainsInternalError()
    {
        // Arrange
        RequestDelegate next = _ => throw new NullReferenceException("Object was null");
        var middleware = new RealMiddleware(next, _mockLogger.Object);
        var context = CreateHttpContext();
        var body = new MemoryStream();
        context.Response.Body = body;

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var json = await ReadBodyAsync(body);
        json.Should().Contain("INTERNAL_ERROR");
    }

    [Fact]
    public async Task InvokeAsync_UnhandledException_LogsError()
    {
        // Arrange
        RequestDelegate next = _ => throw new Exception("Test unhandled error");
        var middleware = new RealMiddleware(next, _mockLogger.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert — unhandled exceptions must be logged at Error level
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

    #region AP-032: CrmException Logging — Warning not Error

    [Fact]
    public async Task InvokeAsync_CrmException_LogsWarningNotError()
    {
        // Arrange — AP-032: known CRM exceptions are logged at Warning, not Error
        RequestDelegate next = _ => throw new EntityNotFoundException("Quote", 5);
        var middleware = new RealMiddleware(next, _mockLogger.Object);
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

    #region AP-032: Response Format

    [Fact]
    public async Task InvokeAsync_AnyException_SetsContentTypeJson()
    {
        // Arrange
        RequestDelegate next = _ => throw new EntityNotFoundException("Account", 1);
        var middleware = new RealMiddleware(next, _mockLogger.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task InvokeAsync_AnyException_ResponseBodyIsValidJson()
    {
        // Arrange
        RequestDelegate next = _ => throw new CRM.Core.Exceptions.ValidationException("Bad input");
        var middleware = new RealMiddleware(next, _mockLogger.Object);
        var context = CreateHttpContext();
        var body = new MemoryStream();
        context.Response.Body = body;

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var json = await ReadBodyAsync(body);
        var act = () => JsonDocument.Parse(json);
        act.Should().NotThrow("response body must always be valid JSON");
    }

    [Fact]
    public async Task InvokeAsync_NoException_PipelineContinuesNormally()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var middleware = new RealMiddleware(next, _mockLogger.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(200);
    }

    #endregion

    #region Helpers

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<string> ReadBodyAsync(MemoryStream stream)
    {
        stream.Position = 0;
        return await new StreamReader(stream).ReadToEndAsync();
    }

    #endregion
}
