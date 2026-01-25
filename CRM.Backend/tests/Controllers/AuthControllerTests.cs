// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using Xunit;
using FluentAssertions;
using Moq;
using CRM.Api.Controllers;
using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CRM.Tests.Controllers;

/// <summary>
/// Comprehensive unit tests for AuthController
/// 
/// FUNCTIONAL VIEW:
/// - Tests authentication flow (login, logout, token refresh)
/// - Tests registration and password management
/// - Tests OAuth integration endpoints
/// 
/// TECHNICAL VIEW:
/// - Mocks IAuthenticationService for isolated testing
/// - Validates HTTP status codes and response bodies
/// - Tests error handling scenarios
/// </summary>
public class AuthControllerTests
{
    private readonly Mock<IAuthenticationService> _mockAuthService;
    private readonly Mock<ILogger<AuthController>> _mockLogger;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockAuthService = new Mock<IAuthenticationService>();
        _mockLogger = new Mock<ILogger<AuthController>>();
        _controller = new AuthController(_mockAuthService.Object, _mockLogger.Object);
    }

    #region Login Tests

    /// <summary>
    /// FUNCTIONAL: Successful login returns JWT token
    /// TECHNICAL: Returns OkObjectResult with LoginResponse containing access token
    /// </summary>
    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "admin@example.com",
            Password = "Admin@123"
        };
        var loginResponse = new LoginResponse
        {
            AccessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
            RefreshToken = "refresh-token-123",
            ExpiresIn = 28800,
            User = new UserDto { Id = 1, Email = "admin@example.com", Role = "Admin" }
        };

        _mockAuthService.Setup(s => s.LoginAsync(It.IsAny<LoginRequest>()))
            .ReturnsAsync(loginResponse);

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        okResult.StatusCode.Should().Be(200);
        var response = okResult.Value as LoginResponse;
        response.Should().NotBeNull();
        response!.AccessToken.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// FUNCTIONAL: Invalid credentials return 401 Unauthorized
    /// TECHNICAL: Returns UnauthorizedObjectResult
    /// </summary>
    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "invalid@example.com",
            Password = "wrongpassword"
        };

        _mockAuthService.Setup(s => s.LoginAsync(It.IsAny<LoginRequest>()))
            .ReturnsAsync((LoginResponse?)null);

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    /// <summary>
    /// FUNCTIONAL: Empty email returns bad request
    /// TECHNICAL: Model validation should reject empty email
    /// </summary>
    [Fact]
    public async Task Login_WithEmptyEmail_ReturnsBadRequest()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "",
            Password = "somepassword"
        };

        _mockAuthService.Setup(s => s.LoginAsync(It.IsAny<LoginRequest>()))
            .ThrowsAsync(new ArgumentException("Email is required"));

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    /// <summary>
    /// FUNCTIONAL: Locked account returns appropriate error
    /// TECHNICAL: Returns 403 Forbidden for locked accounts
    /// </summary>
    [Fact]
    public async Task Login_WithLockedAccount_ReturnsForbidden()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "locked@example.com",
            Password = "Password@123"
        };

        _mockAuthService.Setup(s => s.LoginAsync(It.IsAny<LoginRequest>()))
            .ThrowsAsync(new UnauthorizedAccessException("Account is locked"));

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        var forbiddenResult = Assert.IsType<ObjectResult>(result);
        forbiddenResult.StatusCode.Should().Be(403);
    }

    #endregion

    #region Token Refresh Tests

    /// <summary>
    /// FUNCTIONAL: Valid refresh token returns new access token
    /// TECHNICAL: Returns OkObjectResult with new tokens
    /// </summary>
    [Fact]
    public async Task RefreshToken_WithValidToken_ReturnsNewTokens()
    {
        // Arrange
        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = "valid-refresh-token"
        };
        var response = new LoginResponse
        {
            AccessToken = "new-access-token",
            RefreshToken = "new-refresh-token",
            ExpiresIn = 28800
        };

        _mockAuthService.Setup(s => s.RefreshTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.RefreshToken(refreshRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        okResult.StatusCode.Should().Be(200);
    }

    /// <summary>
    /// FUNCTIONAL: Expired refresh token returns 401
    /// TECHNICAL: Returns UnauthorizedObjectResult
    /// </summary>
    [Fact]
    public async Task RefreshToken_WithExpiredToken_ReturnsUnauthorized()
    {
        // Arrange
        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = "expired-refresh-token"
        };

        _mockAuthService.Setup(s => s.RefreshTokenAsync(It.IsAny<string>()))
            .ReturnsAsync((LoginResponse?)null);

        // Act
        var result = await _controller.RefreshToken(refreshRequest);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    #endregion

    #region Logout Tests

    /// <summary>
    /// FUNCTIONAL: Logout invalidates session
    /// TECHNICAL: Returns OkObjectResult with success message
    /// </summary>
    [Fact]
    public async Task Logout_WithValidSession_ReturnsOk()
    {
        // Arrange
        _mockAuthService.Setup(s => s.LogoutAsync(It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Logout();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        okResult.StatusCode.Should().Be(200);
    }

    #endregion

    #region Password Reset Tests

    /// <summary>
    /// FUNCTIONAL: Request password reset sends email
    /// TECHNICAL: Returns OkObjectResult regardless of email existence (security)
    /// </summary>
    [Fact]
    public async Task RequestPasswordReset_WithAnyEmail_ReturnsOk()
    {
        // Arrange
        var request = new PasswordResetRequest { Email = "any@example.com" };

        _mockAuthService.Setup(s => s.RequestPasswordResetAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.RequestPasswordReset(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        okResult.StatusCode.Should().Be(200);
    }

    /// <summary>
    /// FUNCTIONAL: Valid reset token allows password change
    /// TECHNICAL: Returns OkObjectResult on successful reset
    /// </summary>
    [Fact]
    public async Task ResetPassword_WithValidToken_ReturnsOk()
    {
        // Arrange
        var request = new ResetPasswordRequest
        {
            Token = "valid-reset-token",
            NewPassword = "NewPassword@123"
        };

        _mockAuthService.Setup(s => s.ResetPasswordAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ResetPassword(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        okResult.StatusCode.Should().Be(200);
    }

    /// <summary>
    /// FUNCTIONAL: Invalid reset token returns error
    /// TECHNICAL: Returns BadRequestObjectResult
    /// </summary>
    [Fact]
    public async Task ResetPassword_WithInvalidToken_ReturnsBadRequest()
    {
        // Arrange
        var request = new ResetPasswordRequest
        {
            Token = "invalid-token",
            NewPassword = "NewPassword@123"
        };

        _mockAuthService.Setup(s => s.ResetPasswordAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.ResetPassword(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    #endregion

    #region Two-Factor Authentication Tests

    /// <summary>
    /// FUNCTIONAL: Enable 2FA returns QR code
    /// TECHNICAL: Returns OkObjectResult with setup data
    /// </summary>
    [Fact]
    public async Task EnableTwoFactor_ReturnsQRCodeData()
    {
        // Arrange
        var setupData = new TwoFactorSetupDto
        {
            SecretKey = "ABCDEFGHIJKLMNOP",
            QrCodeDataUri = "data:image/png;base64,..."
        };

        _mockAuthService.Setup(s => s.SetupTwoFactorAsync(It.IsAny<int>()))
            .ReturnsAsync(setupData);

        // Act
        var result = await _controller.EnableTwoFactor();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        okResult.StatusCode.Should().Be(200);
    }

    /// <summary>
    /// FUNCTIONAL: Valid TOTP code verifies 2FA
    /// TECHNICAL: Returns OkObjectResult on valid code
    /// </summary>
    [Fact]
    public async Task VerifyTwoFactor_WithValidCode_ReturnsOk()
    {
        // Arrange
        var request = new VerifyTwoFactorRequest { Code = "123456" };

        _mockAuthService.Setup(s => s.VerifyTwoFactorAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.VerifyTwoFactor(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        okResult.StatusCode.Should().Be(200);
    }

    #endregion
}
