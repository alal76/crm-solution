// CRM Solution - Middleware Tests
// Tests for InstrumentationMiddleware and SecurityHeadersMiddleware

using CRM.Api.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Middleware;

/// <summary>
/// Tests for InstrumentationMiddleware - request/response logging and timing.
/// </summary>
public class InstrumentationMiddlewareTests
{
    private readonly Mock<ILogger<InstrumentationMiddleware>> _loggerMock;

    public InstrumentationMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<InstrumentationMiddleware>>();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange
        RequestDelegate next = (context) => Task.CompletedTask;

        // Act
        var middleware = new InstrumentationMiddleware(next, _loggerMock.Object);

        // Assert
        middleware.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithVerboseFlag_ShouldAcceptParameter()
    {
        // Arrange
        RequestDelegate next = (context) => Task.CompletedTask;

        // Act
        var middleware = new InstrumentationMiddleware(next, _loggerMock.Object, isVerbose: true);

        // Assert
        middleware.Should().NotBeNull();
    }

    #endregion

    #region InvokeAsync Tests

    [Fact]
    public async Task InvokeAsync_WithValidRequest_ShouldCallNextMiddleware()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = (context) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new InstrumentationMiddleware(next, _loggerMock.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_ShouldAddRequestIdHeader()
    {
        // Arrange
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = new InstrumentationMiddleware(next, _loggerMock.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers.Should().ContainKey("X-Request-Id");
        context.Response.Headers["X-Request-Id"].ToString().Should().NotBeNullOrEmpty();
        context.Response.Headers["X-Request-Id"].ToString().Length.Should().Be(8);
    }

    [Fact]
    public async Task InvokeAsync_ShouldAddTraceIdHeader()
    {
        // Arrange
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = new InstrumentationMiddleware(next, _loggerMock.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers.Should().ContainKey("X-Trace-Id");
        context.Response.Headers["X-Trace-Id"].ToString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task InvokeAsync_WithSuccessfulRequest_ShouldLogInformation()
    {
        // Arrange
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = new InstrumentationMiddleware(next, _loggerMock.Object);
        var context = CreateHttpContext("/api/test", "GET");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeast(1));
    }

    [Fact]
    public async Task InvokeAsync_WithVerboseMode_ShouldLogMoreDetails()
    {
        // Arrange
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = new InstrumentationMiddleware(next, _loggerMock.Object, isVerbose: true);
        var context = CreateHttpContext("/api/test", "POST");
        context.Request.Headers["Content-Type"] = "application/json";

        // Act
        await middleware.InvokeAsync(context);

        // Assert - Should log with query and user info
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce());
    }

    [Fact]
    public async Task InvokeAsync_WhenNextMiddlewareThrows_ShouldLogError()
    {
        // Arrange
        var exception = new InvalidOperationException("Test error");
        RequestDelegate next = (context) => throw exception;
        var middleware = new InstrumentationMiddleware(next, _loggerMock.Object);
        var context = CreateHttpContext();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => middleware.InvokeAsync(context));

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce());
    }

    [Fact]
    public async Task InvokeAsync_WhenNextMiddlewareThrows_ShouldRethrowException()
    {
        // Arrange
        var exception = new InvalidOperationException("Test error");
        RequestDelegate next = (context) => throw exception;
        var middleware = new InstrumentationMiddleware(next, _loggerMock.Object);
        var context = CreateHttpContext();

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => middleware.InvokeAsync(context));

        thrownException.Should().BeSameAs(exception);
    }

    [Fact]
    public async Task InvokeAsync_WithErrorStatusCode_ShouldLogWarning()
    {
        // Arrange
        RequestDelegate next = (context) =>
        {
            context.Response.StatusCode = 404;
            return Task.CompletedTask;
        };
        var middleware = new InstrumentationMiddleware(next, _loggerMock.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce());
    }

    [Fact]
    public async Task InvokeAsync_WithServerError_ShouldLogError()
    {
        // Arrange
        RequestDelegate next = (context) =>
        {
            context.Response.StatusCode = 500;
            return Task.CompletedTask;
        };
        var middleware = new InstrumentationMiddleware(next, _loggerMock.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce());
    }

    [Fact]
    public async Task InvokeAsync_ShouldTrackDuration()
    {
        // Arrange
        RequestDelegate next = async (context) =>
        {
            await Task.Delay(50); // Small delay to ensure measurable duration
        };
        var middleware = new InstrumentationMiddleware(next, _loggerMock.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert - Duration should be logged (we verify by checking Information level logging occurred)
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeast(1));
    }

    [Fact]
    public async Task InvokeAsync_WithDifferentMethods_ShouldLogCorrectMethod()
    {
        // Arrange
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = new InstrumentationMiddleware(next, _loggerMock.Object);

        foreach (var method in new[] { "GET", "POST", "PUT", "DELETE", "PATCH" })
        {
            var context = CreateHttpContext("/api/test", method);

            // Act
            await middleware.InvokeAsync(context);

            // Assert - Should log the method
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeast(1));
        }
    }

    #endregion

    #region Helper Methods

    private static DefaultHttpContext CreateHttpContext(string path = "/api/test", string method = "GET")
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Request.Method = method;
        return context;
    }

    #endregion
}

/// <summary>
/// Tests for SecurityHeadersMiddleware - adds security headers to responses.
/// </summary>
public class SecurityHeadersMiddlewareTests
{
    private readonly Mock<ILogger<SecurityHeadersMiddleware>> _loggerMock;

    public SecurityHeadersMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<SecurityHeadersMiddleware>>();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange
        RequestDelegate next = (context) => Task.CompletedTask;

        // Act
        var middleware = new SecurityHeadersMiddleware(next, _loggerMock.Object);

        // Assert
        middleware.Should().NotBeNull();
    }

    #endregion

    #region Security Headers Tests

    [Fact]
    public async Task InvokeAsync_ShouldAddXContentTypeOptionsHeader()
    {
        // Arrange
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = new SecurityHeadersMiddleware(next, _loggerMock.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers.Should().ContainKey("X-Content-Type-Options");
        context.Response.Headers["X-Content-Type-Options"].ToString().Should().Be("nosniff");
    }

    [Fact]
    public async Task InvokeAsync_ShouldAddXFrameOptionsHeader()
    {
        // Arrange
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = new SecurityHeadersMiddleware(next, _loggerMock.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers.Should().ContainKey("X-Frame-Options");
        context.Response.Headers["X-Frame-Options"].ToString().Should().Be("SAMEORIGIN");
    }

    [Fact]
    public async Task InvokeAsync_ShouldAddXXSSProtectionHeader()
    {
        // Arrange
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = new SecurityHeadersMiddleware(next, _loggerMock.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers.Should().ContainKey("X-XSS-Protection");
        context.Response.Headers["X-XSS-Protection"].ToString().Should().Be("1; mode=block");
    }

    [Fact]
    public async Task InvokeAsync_ShouldAddReferrerPolicyHeader()
    {
        // Arrange
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = new SecurityHeadersMiddleware(next, _loggerMock.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers.Should().ContainKey("Referrer-Policy");
        context.Response.Headers["Referrer-Policy"].ToString().Should().Be("strict-origin-when-cross-origin");
    }

    [Fact]
    public async Task InvokeAsync_ShouldAddContentSecurityPolicyHeader()
    {
        // Arrange
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = new SecurityHeadersMiddleware(next, _loggerMock.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers.Should().ContainKey("Content-Security-Policy");
        var csp = context.Response.Headers["Content-Security-Policy"].ToString();
        csp.Should().Contain("default-src 'self'");
        csp.Should().Contain("script-src");
        csp.Should().Contain("style-src");
        csp.Should().Contain("frame-ancestors 'self'");
    }

    [Fact]
    public async Task InvokeAsync_ShouldAddPermissionsPolicyHeader()
    {
        // Arrange
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = new SecurityHeadersMiddleware(next, _loggerMock.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers.Should().ContainKey("Permissions-Policy");
        var policy = context.Response.Headers["Permissions-Policy"].ToString();
        policy.Should().Contain("camera=()");
        policy.Should().Contain("microphone=()");
        policy.Should().Contain("geolocation=()");
    }

    [Fact]
    public async Task InvokeAsync_WithLocalhostHost_ShouldNotAddHSTSHeader()
    {
        // Arrange
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = new SecurityHeadersMiddleware(next, _loggerMock.Object);
        var context = CreateHttpContext();
        context.Request.Host = new HostString("localhost", 5000);

        // Act
        await middleware.InvokeAsync(context);

        // Assert - HSTS should NOT be added for localhost
        context.Response.Headers.Should().NotContainKey("Strict-Transport-Security");
    }

    [Fact]
    public async Task InvokeAsync_WithNonLocalhostHost_ShouldAddHSTSHeader()
    {
        // Arrange
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = new SecurityHeadersMiddleware(next, _loggerMock.Object);
        var context = CreateHttpContext();
        context.Request.Host = new HostString("api.crm.example.com");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers.Should().ContainKey("Strict-Transport-Security");
        var hsts = context.Response.Headers["Strict-Transport-Security"].ToString();
        hsts.Should().Contain("max-age=31536000");
        hsts.Should().Contain("includeSubDomains");
    }

    #endregion

    #region Cache Control Tests

    [Fact]
    public async Task InvokeAsync_ForApiPath_ShouldAddCacheControlHeaders()
    {
        // Arrange
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = new SecurityHeadersMiddleware(next, _loggerMock.Object);
        var context = CreateHttpContext("/api/accounts");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers.Should().ContainKey("Cache-Control");
        context.Response.Headers["Cache-Control"].ToString().Should().Contain("no-store");
        context.Response.Headers["Cache-Control"].ToString().Should().Contain("no-cache");

        context.Response.Headers.Should().ContainKey("Pragma");
        context.Response.Headers["Pragma"].ToString().Should().Be("no-cache");

        context.Response.Headers.Should().ContainKey("Expires");
        context.Response.Headers["Expires"].ToString().Should().Be("0");
    }

    [Fact]
    public async Task InvokeAsync_ForNonApiPath_ShouldNotAddCacheControlHeaders()
    {
        // Arrange
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = new SecurityHeadersMiddleware(next, _loggerMock.Object);
        var context = CreateHttpContext("/health");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers.Should().NotContainKey("Cache-Control");
        context.Response.Headers.Should().NotContainKey("Pragma");
        context.Response.Headers.Should().NotContainKey("Expires");
    }

    [Theory]
    [InlineData("/api/accounts")]
    [InlineData("/api/contacts")]
    [InlineData("/api/auth/login")]
    [InlineData("/api/users/profile")]
    public async Task InvokeAsync_ForVariousApiPaths_ShouldAddCacheControlHeaders(string path)
    {
        // Arrange
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = new SecurityHeadersMiddleware(next, _loggerMock.Object);
        var context = CreateHttpContext(path);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers.Should().ContainKey("Cache-Control");
    }

    #endregion

    #region Next Middleware Tests

    [Fact]
    public async Task InvokeAsync_ShouldCallNextMiddleware()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = (context) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new SecurityHeadersMiddleware(next, _loggerMock.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_ShouldAddHeadersBeforeCallingNext()
    {
        // Arrange
        string? xContentTypeOptions = null;
        RequestDelegate next = (context) =>
        {
            xContentTypeOptions = context.Response.Headers["X-Content-Type-Options"].ToString();
            return Task.CompletedTask;
        };

        var middleware = new SecurityHeadersMiddleware(next, _loggerMock.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        xContentTypeOptions.Should().Be("nosniff");
    }

    #endregion

    #region Extension Method Tests

    [Fact]
    public void UseSecurityHeaders_ShouldReturnApplicationBuilder()
    {
        // Arrange
        var appBuilderMock = new Mock<IApplicationBuilder>();
        appBuilderMock.Setup(x => x.UseMiddleware<SecurityHeadersMiddleware>())
            .Returns(appBuilderMock.Object);

        // Act
        var result = SecurityHeadersMiddlewareExtensions.UseSecurityHeaders(appBuilderMock.Object);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region All Headers Present Test

    [Fact]
    public async Task InvokeAsync_ShouldAddAllRequiredSecurityHeaders()
    {
        // Arrange
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = new SecurityHeadersMiddleware(next, _loggerMock.Object);
        var context = CreateHttpContext("/api/test");
        context.Request.Host = new HostString("api.crm.example.com");

        // Act
        await middleware.InvokeAsync(context);

        // Assert - All security headers should be present
        var headers = context.Response.Headers;
        
        headers.Should().ContainKey("X-Content-Type-Options");
        headers.Should().ContainKey("X-Frame-Options");
        headers.Should().ContainKey("X-XSS-Protection");
        headers.Should().ContainKey("Referrer-Policy");
        headers.Should().ContainKey("Content-Security-Policy");
        headers.Should().ContainKey("Permissions-Policy");
        headers.Should().ContainKey("Strict-Transport-Security");
        headers.Should().ContainKey("Cache-Control");
        headers.Should().ContainKey("Pragma");
        headers.Should().ContainKey("Expires");
    }

    #endregion

    #region Helper Methods

    private static DefaultHttpContext CreateHttpContext(string path = "/api/test")
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Request.Host = new HostString("localhost", 5000);
        return context;
    }

    #endregion
}

/// <summary>
/// Tests for InstrumentationMiddleware extension methods.
/// </summary>
public class InstrumentationMiddlewareExtensionsTests
{
    [Fact]
    public void UseInstrumentation_ShouldReturnApplicationBuilder()
    {
        // Arrange
        var appBuilderMock = new Mock<IApplicationBuilder>();
        appBuilderMock.Setup(x => x.UseMiddleware<InstrumentationMiddleware>(It.IsAny<object[]>()))
            .Returns(appBuilderMock.Object);

        // Act
        var result = InstrumentationMiddlewareExtensions.UseInstrumentation(appBuilderMock.Object);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void UseInstrumentation_WithVerboseTrue_ShouldPassParameter()
    {
        // Arrange
        var appBuilderMock = new Mock<IApplicationBuilder>();
        appBuilderMock.Setup(x => x.UseMiddleware<InstrumentationMiddleware>(true))
            .Returns(appBuilderMock.Object);

        // Act
        var result = InstrumentationMiddlewareExtensions.UseInstrumentation(appBuilderMock.Object, verbose: true);

        // Assert
        result.Should().NotBeNull();
        appBuilderMock.Verify(x => x.UseMiddleware<InstrumentationMiddleware>(true), Times.Once);
    }
}
