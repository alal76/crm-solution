// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Claims;
using CRM.Api.Controllers;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthenticationService> _mockAuthService;
    private readonly Mock<ILogger<AuthController>> _mockLogger;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockAuthService = new Mock<IAuthenticationService>();
        _mockLogger = new Mock<ILogger<AuthController>>();

        // Create minimal mock dependencies for AuthController constructor
        var mockHttpClient = new HttpClient();
        var mockLinkedInOptions = Options.Create(new LinkedInOAuthOptions());
        var mockLinkedInLogger = new Mock<ILogger<LinkedInOAuthProvider>>();
        var linkedInProvider = new LinkedInOAuthProvider(mockHttpClient, mockLinkedInOptions, mockLinkedInLogger.Object);

        var mockAppleOptions = Options.Create(new AppleOAuthOptions());
        var mockAppleLogger = new Mock<ILogger<AppleOAuthProvider>>();
        var appleProvider = new AppleOAuthProvider(mockHttpClient, mockAppleOptions, mockAppleLogger.Object);

        var mockWebAuthnService = new Mock<IWebAuthnService>();

        _controller = new AuthController(
            _mockAuthService.Object,
            _mockLogger.Object,
            linkedInProvider,
            appleProvider,
            mockWebAuthnService.Object);
    }

    private void SetupAuthenticatedUser(string userId = "1", string role = "0")
    {
        var claims = new List<Claim>
        {
            new Claim("sub", userId),
            new Claim("role", role)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    private void SetupUnauthenticatedUser()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
        };
    }

    // ─── Register ───

    [Fact]
    public async Task Register_ShouldReturnOk_WhenRegistrationSucceeds()
    {
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "Test@123",
            FirstName = "Test",
            LastName = "User",
            Username = "testuser"
        };
        var authResponse = new AuthResponse { AccessToken = "jwt-token", RefreshToken = "refresh-token" };
        _mockAuthService.Setup(s => s.RegisterAsync(request)).ReturnsAsync(authResponse);

        var result = await _controller.Register(request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(authResponse);
    }

    [Fact]
    public async Task Register_ShouldReturn500_WhenExceptionThrown()
    {
        var request = new RegisterRequest { Email = "test@example.com", Password = "Test@123" };
        _mockAuthService.Setup(s => s.RegisterAsync(request)).ThrowsAsync(new Exception("DB error"));

        var result = await _controller.Register(request);

        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
    }

    // ─── Login ───

    [Fact]
    public async Task Login_ShouldReturnOk_WhenCredentialsValid()
    {
        var request = new LoginRequest { Email = "admin@crm.local", Password = "Admin@123" };
        var authResponse = new AuthResponse { AccessToken = "jwt-token", RefreshToken = "refresh-token" };
        _mockAuthService.Setup(s => s.LoginAsync(request)).ReturnsAsync(authResponse);

        var result = await _controller.Login(request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(authResponse);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenCredentialsInvalid()
    {
        var request = new LoginRequest { Email = "admin@crm.local", Password = "wrong" };
        _mockAuthService.Setup(s => s.LoginAsync(request))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid credentials"));

        var result = await _controller.Login(request);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    // ─── LoginWith2FA ───

    [Fact]
    public async Task LoginWith2FA_ShouldReturnOk_WhenCodeValid()
    {
        var request = new TwoFactorLoginRequest { TwoFactorToken = "2fa-token", Code = "123456" };
        var authResponse = new AuthResponse { AccessToken = "jwt-token" };
        _mockAuthService.Setup(s => s.VerifyTwoFactorLoginAsync("2fa-token", "123456"))
            .ReturnsAsync(authResponse);

        var result = await _controller.LoginWith2FA(request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(authResponse);
    }

    [Fact]
    public async Task LoginWith2FA_ShouldReturnUnauthorized_WhenCodeInvalid()
    {
        var request = new TwoFactorLoginRequest { TwoFactorToken = "2fa-token", Code = "000000" };
        _mockAuthService.Setup(s => s.VerifyTwoFactorLoginAsync("2fa-token", "000000"))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid 2FA code"));

        var result = await _controller.LoginWith2FA(request);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    // ─── RefreshToken ───

    [Fact]
    public async Task RefreshToken_ShouldReturnOk_WhenTokenValid()
    {
        var request = new RefreshTokenRequest { RefreshToken = "valid-refresh" };
        var authResponse = new AuthResponse { AccessToken = "new-jwt" };
        _mockAuthService.Setup(s => s.RefreshTokenAsync("valid-refresh"))
            .ReturnsAsync(authResponse);

        var result = await _controller.RefreshToken(request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(authResponse);
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnBadRequest_WhenArgumentException()
    {
        var request = new RefreshTokenRequest { RefreshToken = "bad" };
        _mockAuthService.Setup(s => s.RefreshTokenAsync("bad"))
            .ThrowsAsync(new ArgumentException("Token is required"));

        var result = await _controller.RefreshToken(request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnUnauthorized_WhenTokenInvalid()
    {
        var request = new RefreshTokenRequest { RefreshToken = "expired" };
        _mockAuthService.Setup(s => s.RefreshTokenAsync("expired"))
            .ThrowsAsync(new UnauthorizedAccessException("Token expired"));

        var result = await _controller.RefreshToken(request);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    // ─── OAuthLogin ───

    [Fact]
    public async Task OAuthLogin_ShouldReturnOk_WhenOAuthSucceeds()
    {
        var request = new OAuthLoginRequest { Provider = "Google", Token = "google-token" };
        var authResponse = new AuthResponse { AccessToken = "jwt-token" };
        _mockAuthService.Setup(s => s.OAuthLoginAsync(request)).ReturnsAsync(authResponse);

        var result = await _controller.OAuthLogin(request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(authResponse);
    }

    // ─── VerifyToken ───

    [Fact]
    public async Task VerifyToken_ShouldReturnOk_WithIsValidTrue()
    {
        _mockAuthService.Setup(s => s.VerifyTokenAsync("valid-token")).ReturnsAsync(true);

        var result = await _controller.VerifyToken("valid-token");

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        // Returns anonymous type { isValid = true }
        okResult.Value.Should().NotBeNull();
    }

    // ─── GetCurrentUser ───

    [Fact]
    public async Task GetCurrentUser_ShouldReturnOk_WhenUserExists()
    {
        SetupAuthenticatedUser("5");
        var user = new User { Id = 5, Username = "admin", Email = "admin@crm.local", FirstName = "Admin", LastName = "User" };
        _mockAuthService.Setup(s => s.GetUserByIdAsync(5)).ReturnsAsync(user);

        var result = await _controller.GetCurrentUser();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetCurrentUser_ShouldReturnUnauthorized_WhenNoSubClaim()
    {
        SetupUnauthenticatedUser();

        var result = await _controller.GetCurrentUser();

        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task GetCurrentUser_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        SetupAuthenticatedUser("999");
        _mockAuthService.Setup(s => s.GetUserByIdAsync(999)).ReturnsAsync((User?)null);

        var result = await _controller.GetCurrentUser();

        result.Should().BeOfType<NotFoundResult>();
    }

    // ─── Setup2FA ───

    [Fact]
    public async Task Setup2FA_ShouldReturnOk_WhenSetupSucceeds()
    {
        SetupAuthenticatedUser("1");
        var setupResponse = new TwoFactorSetupResponse { QrCodeUrl = "otpauth://...", Secret = "BASE32SECRET" };
        _mockAuthService.Setup(s => s.SetupTwoFactorAsync(1)).ReturnsAsync(setupResponse);

        var result = await _controller.Setup2FA();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(setupResponse);
    }

    [Fact]
    public async Task Setup2FA_ShouldReturnUnauthorized_WhenNoSubClaim()
    {
        SetupUnauthenticatedUser();

        var result = await _controller.Setup2FA();

        result.Should().BeOfType<UnauthorizedResult>();
    }

    // ─── Verify2FA ───

    [Fact]
    public async Task Verify2FA_ShouldReturnOk_WhenCodeValid()
    {
        SetupAuthenticatedUser("1");
        var request = new TwoFactorVerification { Code = "123456" };
        _mockAuthService.Setup(s => s.VerifyTwoFactorCodeAsync(1, "123456")).ReturnsAsync(true);

        var result = await _controller.Verify2FA(request);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Verify2FA_ShouldReturnBadRequest_WhenCodeInvalid()
    {
        SetupAuthenticatedUser("1");
        var request = new TwoFactorVerification { Code = "000000" };
        _mockAuthService.Setup(s => s.VerifyTwoFactorCodeAsync(1, "000000")).ReturnsAsync(false);

        var result = await _controller.Verify2FA(request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    // ─── Enable2FA ───

    [Fact]
    public async Task Enable2FA_ShouldReturnOk_WhenEnableSucceeds()
    {
        SetupAuthenticatedUser("1");
        var request = new TwoFactorEnableRequest
        {
            Secret = "BASE32SECRET",
            BackupCodes = new List<string> { "code1", "code2" }
        };
        _mockAuthService.Setup(s => s.EnableTwoFactorAsync(1, "BASE32SECRET", request.BackupCodes))
            .Returns(Task.CompletedTask);

        var result = await _controller.Enable2FA(request);

        result.Should().BeOfType<OkObjectResult>();
    }

    // ─── Disable2FA ───

    [Fact]
    public async Task Disable2FA_ShouldReturnOk_WhenDisableSucceeds()
    {
        SetupAuthenticatedUser("1");
        _mockAuthService.Setup(s => s.DisableTwoFactorAsync(1)).Returns(Task.CompletedTask);

        var result = await _controller.Disable2FA();

        result.Should().BeOfType<OkObjectResult>();
    }

    // ─── RequestPasswordReset ───

    [Fact]
    public async Task RequestPasswordReset_ShouldReturnOk_WhenRequestSucceeds()
    {
        var request = new CreatePasswordResetDto { Email = "test@example.com" };
        _mockAuthService.Setup(s => s.RequestPasswordResetAsync("test@example.com"))
            .ReturnsAsync("reset-token");

        var result = await _controller.RequestPasswordReset(request);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task RequestPasswordReset_ShouldReturnOk_EvenWhenEmailNotFound()
    {
        // Security: Don't reveal if email exists
        var request = new CreatePasswordResetDto { Email = "nonexistent@example.com" };
        _mockAuthService.Setup(s => s.RequestPasswordResetAsync("nonexistent@example.com"))
            .ThrowsAsync(new InvalidOperationException("Email not found"));

        var result = await _controller.RequestPasswordReset(request);

        // Still returns Ok (with generic message) — doesn't reveal email existence
        result.Should().BeOfType<OkObjectResult>();
    }

    // ─── ConfirmPasswordReset ───

    [Fact]
    public async Task ConfirmPasswordReset_ShouldReturnOk_WhenResetSucceeds()
    {
        var request = new ConfirmPasswordResetDto
        {
            Token = "valid-token",
            NewPassword = "NewPass@123",
            ConfirmPassword = "NewPass@123"
        };
        _mockAuthService.Setup(s => s.ResetPasswordAsync("valid-token", "NewPass@123"))
            .ReturnsAsync(true);

        var result = await _controller.ConfirmPasswordReset(request);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ConfirmPasswordReset_ShouldReturnBadRequest_WhenPasswordsMismatch()
    {
        var request = new ConfirmPasswordResetDto
        {
            Token = "valid-token",
            NewPassword = "Pass@123",
            ConfirmPassword = "DifferentPass@123"
        };

        var result = await _controller.ConfirmPasswordReset(request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ConfirmPasswordReset_ShouldReturnBadRequest_WhenTokenInvalid()
    {
        var request = new ConfirmPasswordResetDto
        {
            Token = "expired-token",
            NewPassword = "Pass@123",
            ConfirmPassword = "Pass@123"
        };
        _mockAuthService.Setup(s => s.ResetPasswordAsync("expired-token", "Pass@123"))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid or expired token"));

        var result = await _controller.ConfirmPasswordReset(request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    // ─── AdminResetPassword ───

    [Fact]
    public async Task AdminResetPassword_ShouldReturnOk_WhenAdminResetsPassword()
    {
        SetupAuthenticatedUser("1", "0"); // Admin role = "0"
        var request = new AdminPasswordResetDto { NewPassword = "NewPass@123" };
        _mockAuthService.Setup(s => s.AdminResetPasswordAsync(5, "NewPass@123"))
            .ReturnsAsync(true);

        var result = await _controller.AdminResetPassword(5, request);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task AdminResetPassword_ShouldReturnForbid_WhenNotAdmin()
    {
        SetupAuthenticatedUser("2", "1"); // Non-admin role = "1"
        var request = new AdminPasswordResetDto { NewPassword = "NewPass@123" };

        var result = await _controller.AdminResetPassword(5, request);

        result.Should().BeOfType<ForbidResult>();
    }

    // ─── SetupPassword ───

    [Fact]
    public async Task SetupPassword_ShouldReturnOk_WhenSetupSucceeds()
    {
        var request = new SetPasswordRequest { };
        var authResponse = new AuthResponse { AccessToken = "jwt-token" };
        _mockAuthService.Setup(s => s.SetupPasswordAsync(request)).ReturnsAsync(authResponse);

        var result = await _controller.SetupPassword(request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(authResponse);
    }

    [Fact]
    public async Task SetupPassword_ShouldReturnUnauthorized_WhenTokenExpired()
    {
        var request = new SetPasswordRequest { };
        _mockAuthService.Setup(s => s.SetupPasswordAsync(request))
            .ThrowsAsync(new UnauthorizedAccessException("Token expired"));

        var result = await _controller.SetupPassword(request);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task SetupPassword_ShouldReturnBadRequest_WhenPasswordInvalid()
    {
        var request = new SetPasswordRequest { };
        _mockAuthService.Setup(s => s.SetupPasswordAsync(request))
            .ThrowsAsync(new ArgumentException("Password too weak"));

        var result = await _controller.SetupPassword(request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    // ─── GetPasswordRequirements ───

    [Fact]
    public async Task GetPasswordRequirements_ShouldReturnOk_WithRequirements()
    {
        var requirements = new PasswordComplexityRequirements
        {
            MinLength = 8,
            RequireUppercase = true,
            RequireLowercase = true,
            RequireNumbers = true
        };
        _mockAuthService.Setup(s => s.GetPasswordRequirementsAsync()).ReturnsAsync(requirements);

        var result = await _controller.GetPasswordRequirements();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(requirements);
    }

    [Fact]
    public async Task GetPasswordRequirements_ShouldReturn500_WhenExceptionThrown()
    {
        _mockAuthService.Setup(s => s.GetPasswordRequirementsAsync())
            .ThrowsAsync(new Exception("DB error"));

        var result = await _controller.GetPasswordRequirements();

        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
    }
}
