// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Net;
using System.Text.Json;
using CRM.Api.Middleware;
using CRM.Core.Exceptions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Middleware;

/// <summary>
/// Unit tests for ErrorHandlingMiddleware (TCOV-014).
/// </summary>
public class ErrorHandlingMiddlewareTests
{
    private readonly Mock<ILogger<ErrorHandlingMiddleware>> _mockLogger = new();

    private (ErrorHandlingMiddleware middleware, DefaultHttpContext context) BuildMiddleware(
        RequestDelegate next)
    {
        var middleware = new ErrorHandlingMiddleware(next, _mockLogger.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return (middleware, context);
    }

    [Fact]
    public async Task InvokeAsync_ShouldReturn500_WhenGeneralExceptionThrown()
    {
        var (middleware, context) = BuildMiddleware(_ => throw new Exception("Unexpected error"));

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task InvokeAsync_ShouldReturn404_WhenEntityNotFoundExceptionThrown()
    {
        var (middleware, context) = BuildMiddleware(_ => throw new EntityNotFoundException("Account", 1));

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task InvokeAsync_ShouldReturn400_WhenValidationExceptionThrown()
    {
        var (middleware, context) = BuildMiddleware(_ => throw new ValidationException("Name is required"));

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task InvokeAsync_ShouldReturn409_WhenDbUpdateConcurrencyExceptionThrown()
    {
        var (middleware, context) = BuildMiddleware(_ => throw new DbUpdateConcurrencyException("Conflict"));

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task InvokeAsync_ShouldPassThrough_WhenNoExceptionThrown()
    {
        var (middleware, context) = BuildMiddleware(_ => Task.CompletedTask);
        context.Response.StatusCode = 200;

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task InvokeAsync_ShouldWriteJsonResponse_WhenExceptionThrown()
    {
        var (middleware, context) = BuildMiddleware(_ => throw new Exception("Test error"));

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        body.Should().NotBeNullOrEmpty();
    }
}
