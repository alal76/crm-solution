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

using CRM.Api.Controllers;
using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for AuthController.
/// Tests authentication endpoints including login, registration, token refresh, and 2FA.
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

    #region Register Tests

    [Fact]
    public async Task Register_WithValidRequest_ReturnsOkWithAuthResponse()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "newuser@example.com",
            FirstName = "John",
            LastName = "Doe",
            Password = "SecurePassword@123",
            ConfirmPassword = "SecurePassword@123"
        };

        var authResponse = new AuthResponse
        {
            AccessToken = "valid_access_token",
            RefreshToken = "valid_refresh_token",
            ExpiresIn = 3600,
            User = new UserDto { Id = 1, Email = "newuser@example.com", FirstName = "John" }
        };

        _mockAuthService.Setup(s => s.RegisterAsync(request)).ReturnsAsync(authResponse);

        // Act
        var result = await _controller.Register(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<AuthResponse>().Subject;
        response.AccessToken.Should().Be("valid_access_token");
        response.User?.Email.Should().Be("newuser@example.com");
    }

    [Fact]
    public async Task Register_WithMismatchedPasswords_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "newuser@example.com",
            FirstName = "John",
            LastName = "Doe",
            Password = "SecurePassword@123",
            ConfirmPassword = "DifferentPassword@456"
        };

        _mockAuthService.Setup(s => s.RegisterAsync(request))
            .ThrowsAsync(new ArgumentException("Passwords do not match"));

        // Act
        var result = await _controller.Register(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "existing@example.com",
            FirstName = "John",
            LastName = "Doe",
            Password = "SecurePassword@123",
            ConfirmPassword = "SecurePassword@123"
        };

        _mockAuthService.Setup(s => s.RegisterAsync(request))
            .ThrowsAsync(new InvalidOperationException("User with this email already exists"));

        // Act
        var result = await _controller.Register(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithAuthResponse()
    {
        // Arrange
        var request = new LoginRequest { Email = "test@example.com", Password = "SecurePassword@123" };

        var authResponse = new AuthResponse
        {
            AccessToken = "valid_access_token",
            RefreshToken = "valid_refresh_token",
            ExpiresIn = 3600,
            User = new UserDto { Id = 1, Email = "test@example.com", FirstName = "John" }
        };

        _mockAuthService.Setup(s => s.LoginAsync(request)).ReturnsAsync(authResponse);

        // Act
        var result = await _controller.Login(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<AuthResponse>().Subject;
        response.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithInvalidEmail_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest { Email = "nonexistent@example.com", Password = "AnyPassword" };

        _mockAuthService.Setup(s => s.LoginAsync(request))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid email or password"));

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_WithIncorrectPassword_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest { Email = "test@example.com", Password = "WrongPassword" };

        _mockAuthService.Setup(s => s.LoginAsync(request))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid email or password"));

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    #endregion

    #region Logout Tests

    [Fact]
    public async Task Logout_WithValidUserId_ReturnsOk()
    {
        // Arrange
        _mockAuthService.Setup(s => s.LogoutAsync(It.IsAny<int>())).ReturnsAsync(true);

        // Act
        var result = await _controller.Logout();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region RefreshToken Tests

    [Fact]
    public async Task RefreshToken_WithValidRefreshToken_ReturnsOkWithNewAccessToken()
    {
        // Arrange
        var request = new RefreshTokenRequest { RefreshToken = "valid_refresh_token" };

        var authResponse = new AuthResponse
        {
            AccessToken = "new_access_token",
            RefreshToken = "valid_refresh_token",
            ExpiresIn = 3600
        };

        _mockAuthService.Setup(s => s.RefreshTokenAsync(request.RefreshToken)).ReturnsAsync(authResponse);

        // Act
        var result = await _controller.RefreshToken(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<AuthResponse>().Subject;
        response.AccessToken.Should().Be("new_access_token");
    }

    [Fact]
    public async Task RefreshToken_WithExpiredToken_ReturnsUnauthorized()
    {
        // Arrange
        var request = new RefreshTokenRequest { RefreshToken = "expired_token" };

        _mockAuthService.Setup(s => s.RefreshTokenAsync(request.RefreshToken))
            .ThrowsAsync(new UnauthorizedAccessException("Refresh token expired"));

        // Act
        var result = await _controller.RefreshToken(request);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    #endregion

    #region ChangePassword Tests

    [Fact]
    public async Task ChangePassword_WithValidData_ReturnsOk()
    {
        // Arrange
        var request = new CRM.Core.Dtos.ChangePasswordRequest
        {
            OldPassword = "OldPassword@123",
            NewPassword = "NewPassword@456",
            ConfirmPassword = "NewPassword@456"
        };

        var authResponse = new AuthResponse
        {
            AccessToken = "token",
            RefreshToken = "refresh",
            ExpiresIn = 3600,
            User = new UserDto { Id = 1, Email = "test@example.com" }
        };

        _mockAuthService.Setup(s => s.ChangePasswordAsync(It.IsAny<int>(), request.OldPassword, request.NewPassword, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authResponse);

        // Act
        var result = await _controller.ChangePassword(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ChangePassword_WithIncorrectCurrentPassword_ReturnsBadRequest()
    {
        // Arrange
        var request = new CRM.Core.Dtos.ChangePasswordRequest
        {
            OldPassword = "WrongPassword",
            NewPassword = "NewPassword@456",
            ConfirmPassword = "NewPassword@456"
        };

        _mockAuthService.Setup(s => s.ChangePasswordAsync(It.IsAny<int>(), request.OldPassword, request.NewPassword, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Current password is incorrect"));

        // Act
        var result = await _controller.ChangePassword(request);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    #endregion
}

/// <summary>
/// Test helper classes
#region Test DTOs - REMOVED
// NOTE: ChangePasswordRequest has been removed from this file
// Use CRM.Core.Dtos.ChangePasswordRequest instead
#endregion
