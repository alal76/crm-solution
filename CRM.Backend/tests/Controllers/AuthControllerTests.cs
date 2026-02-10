// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Authentication Controller Unit Tests

using Xunit;
using Moq;
using FluentAssertions;
using CRM.Api.Controllers;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Security.Claims;

namespace CRM.Tests.Controllers;

/// <summary>
/// Comprehensive unit tests for AuthController
/// Covers: Login, Register, Logout, Password flows, 2FA, OAuth, Token refresh
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

        _controller = new AuthController(
            _mockAuthService.Object,
            _mockLogger.Object);

        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    #region Login Tests

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        var request = new LoginRequest { Email = "test@example.com", Password = "Password123!" };
        var response = new AuthResponse
        {
            Success = true,
            Token = "jwt-token",
            RefreshToken = "refresh-token",
            User = new UserDto { Id = 1, Email = "test@example.com" }
        };

        _mockAuthService.Setup(s => s.LoginAsync(request.Email, request.Password))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Login(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest { Email = "test@example.com", Password = "wrongpassword" };
        var response = new AuthResponse { Success = false, Message = "Invalid credentials" };

        _mockAuthService.Setup(s => s.LoginAsync(request.Email, request.Password))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_LockedAccount_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest { Email = "locked@example.com", Password = "Password123!" };
        var response = new AuthResponse { Success = false, Message = "Account is locked" };

        _mockAuthService.Setup(s => s.LoginAsync(request.Email, request.Password))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_PasswordNeverSet_ReturnsRequiresSetup()
    {
        // Arrange
        var request = new LoginRequest { Email = "new@example.com", Password = "temppass" };
        var response = new AuthResponse
        {
            Success = true,
            RequiresPasswordSetup = true,
            PasswordSetupToken = "setup-token"
        };

        _mockAuthService.Setup(s => s.LoginAsync(request.Email, request.Password))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Login(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var authResponse = okResult.Value as AuthResponse;
        authResponse!.RequiresPasswordSetup.Should().BeTrue();
    }

    [Fact]
    public async Task Login_MustResetPassword_ReturnsRequiresReset()
    {
        // Arrange
        var request = new LoginRequest { Email = "reset@example.com", Password = "oldpass" };
        var response = new AuthResponse
        {
            Success = true,
            MustResetPassword = true,
            PasswordSetupToken = "reset-token"
        };

        _mockAuthService.Setup(s => s.LoginAsync(request.Email, request.Password))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Login(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var authResponse = okResult.Value as AuthResponse;
        authResponse!.MustResetPassword.Should().BeTrue();
    }

    [Fact]
    public async Task Login_Requires2FA_ReturnsRequires2FA()
    {
        // Arrange
        var request = new LoginRequest { Email = "2fa@example.com", Password = "Password123!" };
        var response = new AuthResponse
        {
            Success = true,
            Requires2FA = true,
            TwoFactorToken = "2fa-token"
        };

        _mockAuthService.Setup(s => s.LoginAsync(request.Email, request.Password))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Login(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var authResponse = okResult.Value as AuthResponse;
        authResponse!.Requires2FA.Should().BeTrue();
    }

    [Fact]
    public async Task Login_NullRequest_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.Login(null!);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Login_EmptyEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginRequest { Email = "", Password = "Password123!" };

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Login_EmptyPassword_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginRequest { Email = "test@example.com", Password = "" };

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Login_ServiceThrowsException_ReturnsInternalError()
    {
        // Arrange
        var request = new LoginRequest { Email = "test@example.com", Password = "Password123!" };
        _mockAuthService.Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.Login(request);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
    }

    #endregion

    #region Register Tests

    [Fact]
    public async Task Register_ValidRequest_ReturnsOkWithUser()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "new@example.com",
            Password = "Password123!",
            FirstName = "John",
            LastName = "Doe"
        };
        var response = new AuthResponse
        {
            Success = true,
            User = new UserDto { Id = 1, Email = request.Email }
        };

        _mockAuthService.Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Register(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "existing@example.com",
            Password = "Password123!"
        };
        var response = new AuthResponse
        {
            Success = false,
            Message = "User with this email already exists"
        };

        _mockAuthService.Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Register(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Register_WeakPassword_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "new@example.com",
            Password = "weak"
        };
        var response = new AuthResponse
        {
            Success = false,
            Message = "Password does not meet complexity requirements"
        };

        _mockAuthService.Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Register(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Register_RequiresApproval_ReturnsOkWithPending()
    {
        // Arrange
        var request = new RegisterRequest { Email = "new@example.com", Password = "Password123!" };
        var response = new AuthResponse
        {
            Success = true,
            Message = "Registration pending approval"
        };

        _mockAuthService.Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Register(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task Register_InvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterRequest { Email = "invalid-email", Password = "Password123!" };

        // Act
        var result = await _controller.Register(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region Password Setup Tests

    [Fact]
    public async Task SetupPassword_ValidToken_ReturnsOk()
    {
        // Arrange
        var request = new SetupPasswordRequest
        {
            Token = "valid-token",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };
        var response = new AuthResponse { Success = true };

        _mockAuthService.Setup(s => s.SetupPasswordAsync(It.IsAny<SetupPasswordRequest>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.SetupPassword(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task SetupPassword_InvalidToken_ReturnsBadRequest()
    {
        // Arrange
        var request = new SetupPasswordRequest
        {
            Token = "invalid-token",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };
        var response = new AuthResponse { Success = false, Message = "Invalid or expired token" };

        _mockAuthService.Setup(s => s.SetupPasswordAsync(It.IsAny<SetupPasswordRequest>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.SetupPassword(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task SetupPassword_PasswordMismatch_ReturnsBadRequest()
    {
        // Arrange
        var request = new SetupPasswordRequest
        {
            Token = "valid-token",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "DifferentPassword123!"
        };

        // Act
        var result = await _controller.SetupPassword(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task SetupPassword_WeakPassword_ReturnsBadRequest()
    {
        // Arrange
        var request = new SetupPasswordRequest
        {
            Token = "valid-token",
            NewPassword = "weak",
            ConfirmPassword = "weak"
        };
        var response = new AuthResponse
        {
            Success = false,
            Message = "Password does not meet requirements"
        };

        _mockAuthService.Setup(s => s.SetupPasswordAsync(It.IsAny<SetupPasswordRequest>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.SetupPassword(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task SetupPassword_ExpiredToken_ReturnsBadRequest()
    {
        // Arrange
        var request = new SetupPasswordRequest
        {
            Token = "expired-token",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };
        var response = new AuthResponse { Success = false, Message = "Token has expired" };

        _mockAuthService.Setup(s => s.SetupPasswordAsync(It.IsAny<SetupPasswordRequest>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.SetupPassword(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region Forgot Password Tests

    [Fact]
    public async Task ForgotPassword_ValidEmail_ReturnsOk()
    {
        // Arrange
        var request = new ForgotPasswordRequest { Email = "user@example.com" };

        _mockAuthService.Setup(s => s.ForgotPasswordAsync(request.Email))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ForgotPassword(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ForgotPassword_UnknownEmail_ReturnsOk()
    {
        // Always return OK to prevent email enumeration
        var request = new ForgotPasswordRequest { Email = "unknown@example.com" };

        _mockAuthService.Setup(s => s.ForgotPasswordAsync(request.Email))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ForgotPassword(request);

        // Assert - should always return OK to prevent enumeration
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ForgotPassword_InvalidEmailFormat_ReturnsBadRequest()
    {
        // Arrange
        var request = new ForgotPasswordRequest { Email = "invalid-email" };

        // Act
        var result = await _controller.ForgotPassword(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ForgotPassword_EmptyEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new ForgotPasswordRequest { Email = "" };

        // Act
        var result = await _controller.ForgotPassword(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region Reset Password Tests

    [Fact]
    public async Task ResetPassword_ValidRequest_ReturnsOk()
    {
        // Arrange
        var request = new ResetPasswordRequest
        {
            Token = "valid-reset-token",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };
        var response = new AuthResponse { Success = true };

        _mockAuthService.Setup(s => s.ResetPasswordAsync(It.IsAny<ResetPasswordRequest>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.ResetPassword(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ResetPassword_InvalidToken_ReturnsBadRequest()
    {
        // Arrange
        var request = new ResetPasswordRequest
        {
            Token = "invalid-token",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };
        var response = new AuthResponse { Success = false, Message = "Invalid token" };

        _mockAuthService.Setup(s => s.ResetPasswordAsync(It.IsAny<ResetPasswordRequest>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.ResetPassword(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region Token Refresh Tests

    [Fact]
    public async Task RefreshToken_ValidToken_ReturnsNewTokens()
    {
        // Arrange
        var request = new RefreshTokenRequest { RefreshToken = "valid-refresh-token" };
        var response = new AuthResponse
        {
            Success = true,
            Token = "new-jwt-token",
            RefreshToken = "new-refresh-token"
        };

        _mockAuthService.Setup(s => s.RefreshTokenAsync(request.RefreshToken))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.RefreshToken(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var authResponse = okResult.Value as AuthResponse;
        authResponse!.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RefreshToken_ExpiredToken_ReturnsUnauthorized()
    {
        // Arrange
        var request = new RefreshTokenRequest { RefreshToken = "expired-token" };
        var response = new AuthResponse { Success = false, Message = "Refresh token expired" };

        _mockAuthService.Setup(s => s.RefreshTokenAsync(request.RefreshToken))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.RefreshToken(request);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task RefreshToken_RevokedToken_ReturnsUnauthorized()
    {
        // Arrange
        var request = new RefreshTokenRequest { RefreshToken = "revoked-token" };
        var response = new AuthResponse { Success = false, Message = "Token has been revoked" };

        _mockAuthService.Setup(s => s.RefreshTokenAsync(request.RefreshToken))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.RefreshToken(request);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task RefreshToken_InvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        var request = new RefreshTokenRequest { RefreshToken = "invalid-token" };
        var response = new AuthResponse { Success = false, Message = "Invalid refresh token" };

        _mockAuthService.Setup(s => s.RefreshTokenAsync(request.RefreshToken))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.RefreshToken(request);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    #endregion

    #region Logout Tests

    [Fact]
    public async Task Logout_ValidUser_ReturnsOk()
    {
        // Arrange - setup authenticated user
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "1") };
        var identity = new ClaimsIdentity(claims, "Test");
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(identity);

        _mockAuthService.Setup(s => s.LogoutAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Logout();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Logout_UnauthenticatedUser_ReturnsUnauthorized()
    {
        // Act
        var result = await _controller.Logout();

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
    }

    #endregion

    #region 2FA Tests

    [Fact]
    public async Task Verify2FA_ValidCode_ReturnsOkWithToken()
    {
        // Arrange
        var request = new Verify2FARequest
        {
            TwoFactorToken = "2fa-token",
            Code = "123456"
        };
        var response = new AuthResponse
        {
            Success = true,
            Token = "jwt-token",
            RefreshToken = "refresh-token"
        };

        _mockAuthService.Setup(s => s.Verify2FAAsync(request.TwoFactorToken, request.Code))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Verify2FA(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task Verify2FA_InvalidCode_ReturnsUnauthorized()
    {
        // Arrange
        var request = new Verify2FARequest
        {
            TwoFactorToken = "2fa-token",
            Code = "wrong-code"
        };
        var response = new AuthResponse { Success = false, Message = "Invalid 2FA code" };

        _mockAuthService.Setup(s => s.Verify2FAAsync(request.TwoFactorToken, request.Code))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Verify2FA(request);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Verify2FA_ExpiredToken_ReturnsUnauthorized()
    {
        // Arrange
        var request = new Verify2FARequest
        {
            TwoFactorToken = "expired-token",
            Code = "123456"
        };
        var response = new AuthResponse { Success = false, Message = "2FA token expired" };

        _mockAuthService.Setup(s => s.Verify2FAAsync(request.TwoFactorToken, request.Code))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Verify2FA(request);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Enable2FA_ValidUser_ReturnsSecretAndQR()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "1") };
        var identity = new ClaimsIdentity(claims, "Test");
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(identity);

        var response = new Enable2FAResponse
        {
            Secret = "JBSWY3DPEHPK3PXP",
            QrCodeUrl = "otpauth://totp/CRM:user@example.com?secret=JBSWY3DPEHPK3PXP"
        };

        _mockAuthService.Setup(s => s.Enable2FAAsync(1))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Enable2FA();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var enable2FAResponse = okResult.Value as Enable2FAResponse;
        enable2FAResponse!.Secret.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Confirm2FA_ValidCode_ReturnsBackupCodes()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "1") };
        var identity = new ClaimsIdentity(claims, "Test");
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(identity);

        var request = new Confirm2FARequest { Code = "123456" };
        var response = new Confirm2FAResponse
        {
            Success = true,
            BackupCodes = new[] { "ABC123", "DEF456", "GHI789" }
        };

        _mockAuthService.Setup(s => s.Confirm2FAAsync(1, request.Code))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Confirm2FA(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var confirm2FAResponse = okResult.Value as Confirm2FAResponse;
        confirm2FAResponse!.BackupCodes.Should().HaveCount(3);
    }

    [Fact]
    public async Task Disable2FA_ValidCode_ReturnsOk()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "1") };
        var identity = new ClaimsIdentity(claims, "Test");
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(identity);

        var request = new Disable2FARequest { Code = "123456" };

        _mockAuthService.Setup(s => s.Disable2FAAsync(1, request.Code))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Disable2FA(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region OAuth Tests

    [Fact]
    public async Task GoogleCallback_ValidCode_ReturnsOkWithToken()
    {
        // Arrange
        var request = new OAuthCallbackRequest { Code = "google-auth-code" };
        var response = new AuthResponse
        {
            Success = true,
            Token = "jwt-token",
            RefreshToken = "refresh-token",
            User = new UserDto { Id = 1, Email = "user@gmail.com" }
        };

        _mockAuthService.Setup(s => s.GoogleLoginAsync(request.Code))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.GoogleCallback(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task GoogleCallback_InvalidCode_ReturnsBadRequest()
    {
        // Arrange
        var request = new OAuthCallbackRequest { Code = "invalid-code" };
        var response = new AuthResponse { Success = false, Message = "Invalid OAuth code" };

        _mockAuthService.Setup(s => s.GoogleLoginAsync(request.Code))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.GoogleCallback(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task MicrosoftCallback_ValidCode_ReturnsOkWithToken()
    {
        // Arrange
        var request = new OAuthCallbackRequest { Code = "microsoft-auth-code" };
        var response = new AuthResponse
        {
            Success = true,
            Token = "jwt-token",
            User = new UserDto { Id = 1, Email = "user@outlook.com" }
        };

        _mockAuthService.Setup(s => s.MicrosoftLoginAsync(request.Code))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.MicrosoftCallback(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion

    #region Change Password Tests

    [Fact]
    public async Task ChangePassword_ValidRequest_ReturnsOk()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "1") };
        var identity = new ClaimsIdentity(claims, "Test");
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(identity);

        var request = new ChangePasswordRequest
        {
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };
        var response = new AuthResponse { Success = true };

        _mockAuthService.Setup(s => s.ChangePasswordAsync(1, request))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.ChangePassword(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ChangePassword_WrongCurrentPassword_ReturnsBadRequest()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "1") };
        var identity = new ClaimsIdentity(claims, "Test");
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(identity);

        var request = new ChangePasswordRequest
        {
            CurrentPassword = "WrongPassword",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };
        var response = new AuthResponse { Success = false, Message = "Current password is incorrect" };

        _mockAuthService.Setup(s => s.ChangePasswordAsync(1, request))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.ChangePassword(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ChangePassword_PasswordMismatch_ReturnsBadRequest()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "1") };
        var identity = new ClaimsIdentity(claims, "Test");
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(identity);

        var request = new ChangePasswordRequest
        {
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "DifferentPassword123!"
        };

        // Act
        var result = await _controller.ChangePassword(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region Get Current User Tests

    [Fact]
    public async Task GetCurrentUser_AuthenticatedUser_ReturnsUserDto()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "1") };
        var identity = new ClaimsIdentity(claims, "Test");
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(identity);

        var user = new User { Id = 1, Email = "user@example.com", FirstName = "John", Username = "john", LastName = "Doe", PasswordHash = "hash" };

        _mockAuthService.Setup(s => s.GetUserByIdAsync(1))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.GetCurrentUser();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var userDto = okResult.Value as UserDto;
        userDto!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetCurrentUser_UnauthenticatedUser_ReturnsUnauthorized()
    {
        // Act
        var result = await _controller.GetCurrentUser();

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
    }

    #endregion
}
