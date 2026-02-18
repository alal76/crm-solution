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
using CRM.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Security.Claims;
using System.IO;
using System.Text;

namespace CRM.Tests.Middleware;

/// <summary>
/// Unit tests for Authentication Middleware
/// Covers: Token validation, anonymous paths, user extraction
/// </summary>
public class AuthenticationMiddlewareTests
{
    private readonly Mock<IJwtTokenService> _mockTokenService;
    private readonly Mock<ILogger<AuthenticationMiddleware>> _mockLogger;
    private readonly Mock<IOptions<JwtSettings>> _mockJwtSettings;

    public AuthenticationMiddlewareTests()
    {
        _mockTokenService = new Mock<IJwtTokenService>();
        _mockLogger = new Mock<ILogger<AuthenticationMiddleware>>();
        _mockJwtSettings = new Mock<IOptions<JwtSettings>>();
        _mockJwtSettings.Setup(x => x.Value).Returns(new JwtSettings
        {
            Secret = "test-secret-key-for-testing-purposes-123",
            Issuer = "CRM.Test",
            Audience = "CRM.Client"
        });
    }

    #region Authentication Flow Tests

    [Fact]
    public async Task InvokeAsync_ValidToken_CallsNextMiddleware()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = (context) => { nextCalled = true; return Task.CompletedTask; };
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext(bearerToken: "valid-token");

        _mockTokenService.Setup(t => t.ValidateToken("valid-token"))
            .Returns(CreateClaimsPrincipal());

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_ValidToken_SetsUserOnContext()
    {
        // Arrange
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext(bearerToken: "valid-token");
        var expectedPrincipal = CreateClaimsPrincipal();

        _mockTokenService.Setup(t => t.ValidateToken("valid-token"))
            .Returns(expectedPrincipal);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.User.Should().NotBeNull();
        context.User.Identity.Should().NotBeNull();
    }

    [Fact]
    public async Task InvokeAsync_InvalidToken_Returns401()
    {
        // Arrange
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext(bearerToken: "invalid-token");

        _mockTokenService.Setup(t => t.ValidateToken("invalid-token"))
            .Returns((ClaimsPrincipal?)null);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task InvokeAsync_NoToken_Returns401()
    {
        // Arrange
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext(); // No token

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task InvokeAsync_MalformedAuthHeader_Returns401()
    {
        // Arrange
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();
        context.Request.Headers["Authorization"] = "Invalid-Format";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task InvokeAsync_BearerWithoutToken_Returns401()
    {
        // Arrange
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();
        context.Request.Headers["Authorization"] = "Bearer ";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(401);
    }

    #endregion

    #region Anonymous Path Tests

    [Fact]
    public async Task InvokeAsync_AnonymousPath_SkipsAuth()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = (context) => { nextCalled = true; return Task.CompletedTask; };
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext(path: "/api/auth/login");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
        _mockTokenService.Verify(t => t.ValidateToken(It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData("/api/auth/login")]
    [InlineData("/api/auth/register")]
    [InlineData("/api/auth/refresh")]
    [InlineData("/api/auth/forgot-password")]
    [InlineData("/health")]
    [InlineData("/health/ready")]
    [InlineData("/health/live")]
    [InlineData("/swagger")]
    [InlineData("/swagger/index.html")]
    public async Task InvokeAsync_KnownAnonymousPaths_SkipsAuth(string path)
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = (context) => { nextCalled = true; return Task.CompletedTask; };
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext(path: path);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_ProtectedPath_RequiresAuth()
    {
        // Arrange
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext(path: "/api/accounts");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(401);
    }

    #endregion

    #region Token Extraction Tests

    [Fact]
    public async Task InvokeAsync_TokenInHeader_ExtractsToken()
    {
        // Arrange
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext(bearerToken: "test-token");

        _mockTokenService.Setup(t => t.ValidateToken("test-token"))
            .Returns(CreateClaimsPrincipal());

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockTokenService.Verify(t => t.ValidateToken("test-token"), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_TokenInQueryString_ExtractsToken()
    {
        // Arrange
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = CreateMiddlewareWithQueryTokenSupport(next);
        var context = CreateHttpContext(path: "/api/download?access_token=query-token");

        _mockTokenService.Setup(t => t.ValidateToken("query-token"))
            .Returns(CreateClaimsPrincipal());

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockTokenService.Verify(t => t.ValidateToken("query-token"), Times.Once);
    }

    #endregion

    #region Claims Extraction Tests

    [Fact]
    public async Task InvokeAsync_ValidToken_ExtractsUserId()
    {
        // Arrange
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext(bearerToken: "valid-token");
        var principal = CreateClaimsPrincipal(userId: 123);

        _mockTokenService.Setup(t => t.ValidateToken("valid-token"))
            .Returns(principal);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Items["UserId"].Should().Be(123);
    }

    [Fact]
    public async Task InvokeAsync_ValidToken_ExtractsEmail()
    {
        // Arrange
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext(bearerToken: "valid-token");
        var principal = CreateClaimsPrincipal(email: "user@test.com");

        _mockTokenService.Setup(t => t.ValidateToken("valid-token"))
            .Returns(principal);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Items["Email"].Should().Be("user@test.com");
    }

    [Fact]
    public async Task InvokeAsync_ValidToken_ExtractsRoles()
    {
        // Arrange
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext(bearerToken: "valid-token");
        var principal = CreateClaimsPrincipal(roles: new[] { "Admin", "User" });

        _mockTokenService.Setup(t => t.ValidateToken("valid-token"))
            .Returns(principal);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Items.Should().ContainKey("Roles");
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task InvokeAsync_TokenServiceThrows_Returns500()
    {
        // Arrange
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext(bearerToken: "valid-token");

        _mockTokenService.Setup(t => t.ValidateToken(It.IsAny<string>()))
            .Throws(new Exception("Token service error"));

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task InvokeAsync_ExpiredToken_Returns401()
    {
        // Arrange
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext(bearerToken: "expired-token");

        _mockTokenService.Setup(t => t.ValidateToken("expired-token"))
            .Returns((ClaimsPrincipal?)null);
        _mockTokenService.Setup(t => t.IsTokenExpired("expired-token"))
            .Returns(true);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(401);
    }

    #endregion

    #region Response Modification Tests

    [Fact]
    public async Task InvokeAsync_AuthError_SetsWWWAuthenticateHeader()
    {
        // Arrange
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers.Should().ContainKey("WWW-Authenticate");
    }

    [Fact]
    public async Task InvokeAsync_AuthError_ReturnsJsonError()
    {
        // Arrange
        RequestDelegate next = (context) => Task.CompletedTask;
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();
        var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        responseBody.Position = 0;
        var body = await new StreamReader(responseBody).ReadToEndAsync();
        body.Should().Contain("error");
    }

    #endregion

    #region Helper Methods

    private AuthenticationMiddleware CreateMiddleware(RequestDelegate next)
    {
        return new AuthenticationMiddleware(
            next,
            _mockTokenService.Object,
            _mockLogger.Object,
            _mockJwtSettings.Object);
    }

    private AuthenticationMiddleware CreateMiddlewareWithQueryTokenSupport(RequestDelegate next)
    {
        var settings = new JwtSettings
        {
            Secret = "test-secret",
            Issuer = "test",
            Audience = "test",
            AllowQueryStringToken = true
        };
        var mockSettings = new Mock<IOptions<JwtSettings>>();
        mockSettings.Setup(x => x.Value).Returns(settings);

        return new AuthenticationMiddleware(
            next,
            _mockTokenService.Object,
            _mockLogger.Object,
            mockSettings.Object);
    }

    private DefaultHttpContext CreateHttpContext(string path = "/api/test", string? bearerToken = null)
    {
        var context = new DefaultHttpContext();

        // Parse query string from path if present
        var queryIndex = path.IndexOf('?');
        if (queryIndex >= 0)
        {
            context.Request.QueryString = new QueryString(path.Substring(queryIndex));
            context.Request.Path = path.Substring(0, queryIndex);
        }
        else
        {
            context.Request.Path = path;
        }

        context.Response.Body = new MemoryStream();

        if (!string.IsNullOrEmpty(bearerToken))
        {
            context.Request.Headers["Authorization"] = $"Bearer {bearerToken}";
        }

        return context;
    }

    private ClaimsPrincipal CreateClaimsPrincipal(int userId = 1, string email = "test@test.com", string[]? roles = null)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, "TestUser")
        };

        foreach (var role in roles ?? new[] { "User" })
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var identity = new ClaimsIdentity(claims, "Bearer");
        return new ClaimsPrincipal(identity);
    }

    #endregion
}

// Supporting classes
public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IJwtTokenService _tokenService;
    private readonly ILogger<AuthenticationMiddleware> _logger;
    private readonly JwtSettings _settings;

    private readonly HashSet<string> _anonymousPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/auth/login",
        "/api/auth/register",
        "/api/auth/refresh",
        "/api/auth/forgot-password",
        "/health",
        "/health/ready",
        "/health/live",
        "/swagger"
    };

    public AuthenticationMiddleware(
        RequestDelegate next,
        IJwtTokenService tokenService,
        ILogger<AuthenticationMiddleware> logger,
        IOptions<JwtSettings> settings)
    {
        _next = next;
        _tokenService = tokenService;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";

        // Check if path is anonymous
        if (IsAnonymousPath(path))
        {
            await _next(context);
            return;
        }

        // Extract token
        var token = ExtractToken(context);

        if (string.IsNullOrEmpty(token))
        {
            await WriteUnauthorizedResponse(context, "Authorization token required");
            return;
        }

        try
        {
            var principal = _tokenService.ValidateToken(token);

            if (principal == null)
            {
                await WriteUnauthorizedResponse(context, "Invalid or expired token");
                return;
            }

            context.User = principal;
            ExtractUserInfo(context, principal);

            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authentication error");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("{\"error\": \"Authentication service error\"}");
        }
    }

    private bool IsAnonymousPath(string path)
    {
        return _anonymousPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));
    }

    private string? ExtractToken(HttpContext context)
    {
        // Try Authorization header first
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            var token = authHeader.Substring("Bearer ".Length).Trim();
            return string.IsNullOrEmpty(token) ? null : token;
        }

        // Try query string if allowed
        if (_settings.AllowQueryStringToken)
        {
            var queryToken = context.Request.Query["access_token"].FirstOrDefault();
            if (!string.IsNullOrEmpty(queryToken))
            {
                return queryToken;
            }
        }

        return null;
    }

    private void ExtractUserInfo(HttpContext context, ClaimsPrincipal principal)
    {
        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
        {
            context.Items["UserId"] = userId;
        }

        var email = principal.FindFirst(ClaimTypes.Email)?.Value;
        if (!string.IsNullOrEmpty(email))
        {
            context.Items["Email"] = email;
        }

        var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        if (roles.Any())
        {
            context.Items["Roles"] = roles;
        }
    }

    private async Task WriteUnauthorizedResponse(HttpContext context, string message)
    {
        context.Response.StatusCode = 401;
        context.Response.Headers["WWW-Authenticate"] = "Bearer";
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync($"{{\"error\": \"{message}\"}}");
    }
}

public interface IJwtTokenService
{
    ClaimsPrincipal? ValidateToken(string token);
    bool IsTokenExpired(string token);
}

public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public bool AllowQueryStringToken { get; set; }
}
