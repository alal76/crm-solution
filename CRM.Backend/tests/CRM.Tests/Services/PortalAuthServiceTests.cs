// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for PortalAuthService.
/// Covers Login, Register, ForgotPassword, ResetPassword, VerifyEmail.
/// PORTAL-040
/// </summary>
public class PortalAuthServiceTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<PortalAuthService>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly PortalAuthService _service;

    private readonly List<PortalUser> _portalUsers;
    private readonly List<PortalConfig> _portalConfigs;

    public PortalAuthServiceTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<PortalAuthService>>();
        _mockConfiguration = new Mock<IConfiguration>();

        // JWT settings — secret must be >= 32 characters for HMAC-SHA256
        _mockConfiguration.Setup(c => c["Jwt:Secret"])
            .Returns("unit-test-jwt-secret-key-32chars!");
        _mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
        _mockConfiguration.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");
        _mockConfiguration.Setup(c => c["Jwt:ExpirationMinutes"]).Returns("60");

        _portalUsers = new List<PortalUser>();
        _portalConfigs = new List<PortalConfig>(); // empty → null config → self-registration allowed

        SetupMockDbSets();

        _service = new PortalAuthService(
            _mockContext.Object,
            _mockLogger.Object,
            _mockConfiguration.Object);
    }

    private void SetupMockDbSets()
    {
        var mockPortalUsers = MockDbSetFactory.CreateMockDbSet(_portalUsers);
        var mockPortalConfigs = MockDbSetFactory.CreateMockDbSet(_portalConfigs);

        _mockContext.Setup(c => c.PortalUsers).Returns(mockPortalUsers.Object);
        _mockContext.Setup(c => c.PortalConfigs).Returns(mockPortalConfigs.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    // ── PORTAL-040: Login ─────────────────────────────────────────────────────

    [Fact]
    public async Task Login_ShouldReturnToken_WhenCredentialsValid()
    {
        // Arrange
        const string password = "ValidPass@1";
        _portalUsers.Add(new PortalUser
        {
            Id = 1,
            Email = "valid@portal.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        SetupMockDbSets();

        var dto = new PortalLoginDto { Email = "valid@portal.com", Password = password };

        // Act
        var result = await _service.LoginAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrEmpty();
        result.Email.Should().Be("valid@portal.com");
        result.PortalUserId.Should().Be(1);
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_ShouldReturnNull_WhenEmailNotFound()
    {
        // Arrange — no users in DB
        SetupMockDbSets();
        var dto = new PortalLoginDto { Email = "ghost@portal.com", Password = "AnyPass@1" };

        // Act
        var result = await _service.LoginAsync(dto);

        // Assert — returns null, does not throw
        result.Should().BeNull();
    }

    [Fact]
    public async Task Login_ShouldReturnNull_WhenPasswordInvalid()
    {
        // Arrange
        _portalUsers.Add(new PortalUser
        {
            Id = 1,
            Email = "user@portal.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPass@1"),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        SetupMockDbSets();

        var dto = new PortalLoginDto { Email = "user@portal.com", Password = "WrongPass@9" };

        // Act
        var result = await _service.LoginAsync(dto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Login_ShouldReturnNull_WhenAccountIsInactive()
    {
        // Arrange
        const string password = "SomePass@1";
        _portalUsers.Add(new PortalUser
        {
            Id = 1,
            Email = "inactive@portal.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            IsActive = false, // deactivated
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        SetupMockDbSets();

        var dto = new PortalLoginDto { Email = "inactive@portal.com", Password = password };

        // Act
        var result = await _service.LoginAsync(dto);

        // Assert
        result.Should().BeNull();
    }

    // ── PORTAL-040: Register ──────────────────────────────────────────────────

    [Fact]
    public async Task Register_ShouldHashPassword_BeforeStoring()
    {
        // Arrange — empty user list, no portal config → self-registration allowed
        SetupMockDbSets();

        const string plainPassword = "SecurePass@1";
        var dto = new PortalRegisterDto
        {
            Email = "newuser@portal.com",
            Password = plainPassword,
            ConfirmPassword = plainPassword,
            DisplayName = "New Portal User"
        };

        // Act
        var result = await _service.RegisterAsync(dto);

        // Assert — registration succeeded
        result.Should().NotBeNull();
        result.Email.Should().Be("newuser@portal.com");
        result.DisplayName.Should().Be("New Portal User");

        // The stored password hash must NOT equal the plain password
        _portalUsers.Should().HaveCount(1);
        _portalUsers[0].PasswordHash.Should().NotBe(plainPassword);

        // The stored hash must verify against the plain password using BCrypt
        BCrypt.Net.BCrypt.Verify(plainPassword, _portalUsers[0].PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task Register_ShouldThrow_WhenEmailAlreadyExists()
    {
        // Arrange — existing user with same email
        _portalUsers.Add(new PortalUser
        {
            Id = 1,
            Email = "existing@portal.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("OldPass@1"),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        SetupMockDbSets();

        var dto = new PortalRegisterDto
        {
            Email = "existing@portal.com",
            Password = "AnotherPass@1",
            ConfirmPassword = "AnotherPass@1"
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RegisterAsync(dto));
        ex.Message.Should().Contain("existing@portal.com");
    }

    [Fact]
    public async Task Register_ShouldThrow_WhenPortalIsDisabled()
    {
        // Arrange — portal config with IsEnabled = false
        _portalConfigs.Add(new PortalConfig
        {
            Id = 1,
            IsEnabled = false,
            AllowSelfRegistration = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        SetupMockDbSets();

        var dto = new PortalRegisterDto
        {
            Email = "newuser2@portal.com",
            Password = "SomePass@1",
            ConfirmPassword = "SomePass@1"
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RegisterAsync(dto));
        ex.Message.Should().ContainEquivalentOf("disabled");
    }

    // ── PORTAL-040: Password helpers ──────────────────────────────────────────

    [Fact]
    public async Task ForgotPassword_ShouldReturnFalse_WhenEmailNotFound()
    {
        // Arrange — no users
        SetupMockDbSets();

        // Act
        var result = await _service.ForgotPasswordAsync("notfound@portal.com");

        // Assert — returns false silently (does not reveal user existence)
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ForgotPassword_ShouldSetResetToken_WhenUserExists()
    {
        // Arrange
        var user = new PortalUser
        {
            Id = 1,
            Email = "forgotme@portal.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@1"),
            IsActive = true,
            PasswordResetToken = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _portalUsers.Add(user);
        SetupMockDbSets();

        // Act
        var result = await _service.ForgotPasswordAsync("forgotme@portal.com");

        // Assert
        result.Should().BeTrue();
        user.PasswordResetToken.Should().NotBeNullOrEmpty();
        user.PasswordResetExpiry.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task ResetPassword_ShouldReturnFalse_WhenTokenNotFound()
    {
        // Arrange — no users
        SetupMockDbSets();

        // Act
        var result = await _service.ResetPasswordAsync("invalid-token", "NewPass@1");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyEmail_ShouldReturnFalse_WhenTokenNotFound()
    {
        // Arrange — no users
        SetupMockDbSets();

        // Act
        var result = await _service.VerifyEmailAsync("invalid-verification-token");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyEmail_ShouldMarkEmailVerified_WhenTokenValid()
    {
        // Arrange
        var token = Guid.NewGuid().ToString("N");
        var user = new PortalUser
        {
            Id = 1,
            Email = "verify@portal.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@1"),
            IsActive = true,
            IsEmailVerified = false,
            EmailVerificationToken = token,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _portalUsers.Add(user);
        SetupMockDbSets();

        // Act
        var result = await _service.VerifyEmailAsync(token);

        // Assert
        result.Should().BeTrue();
        user.IsEmailVerified.Should().BeTrue();
        user.EmailVerificationToken.Should().BeNull();
        user.EmailVerifiedAt.Should().NotBeNull();
    }
}
