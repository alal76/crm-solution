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

using Xunit;
using Moq;
using FluentAssertions;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for AuthenticationService
/// 
/// FUNCTIONAL VIEW:
/// Tests cover authentication operations including:
/// - User login and registration
/// - Password validation
/// - Two-factor authentication
/// - Token generation and validation
/// 
/// TECHNICAL VIEW:
/// - Uses InMemory database for testing
/// - Tests service layer authentication logic
/// - Validates security-related operations
/// </summary>
public class AuthenticationServiceTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<AuthenticationService>> _mockLogger;
    private readonly Mock<ISystemSettingsService> _mockSystemSettings;
    private readonly CrmDbContext _dbContext;
    private readonly AuthenticationService _service;

    public AuthenticationServiceTests()
    {
        // Setup InMemory database
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _dbContext = new CrmDbContext(options, null);

        // Setup configuration
        _mockConfiguration = new Mock<IConfiguration>();
        _mockConfiguration.Setup(x => x["Jwt:Key"]).Returns("ThisIsASecretKeyForTestingPurposesOnly12345");
        _mockConfiguration.Setup(x => x["Jwt:Issuer"]).Returns("TestIssuer");
        _mockConfiguration.Setup(x => x["Jwt:Audience"]).Returns("TestAudience");

        _mockLogger = new Mock<ILogger<AuthenticationService>>();
        _mockSystemSettings = new Mock<ISystemSettingsService>();

        _service = new AuthenticationService(
            _dbContext,
            _mockConfiguration.Object,
            _mockLogger.Object,
            _mockSystemSettings.Object);
    }

    #region Login Tests

    /// <summary>
    /// Verifies login returns null for non-existent user
    /// </summary>
    [Fact]
    public async Task LoginAsync_WithNonExistentEmail_ReturnsNull()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "password123"
        };

        // Act
        var result = await _service.LoginAsync(loginRequest);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies login returns null for incorrect password
    /// </summary>
    [Fact]
    public async Task LoginAsync_WithIncorrectPassword_ReturnsNull()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctPassword"),
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "wrongPassword"
        };

        // Act
        var result = await _service.LoginAsync(loginRequest);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies login returns null for inactive user
    /// </summary>
    [Fact]
    public async Task LoginAsync_WithInactiveUser_ReturnsNull()
    {
        // Arrange
        var user = new User
        {
            Id = 2,
            Email = "inactive@example.com",
            Username = "inactiveuser",
            FirstName = "Inactive",
            LastName = "User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            IsActive = false,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var loginRequest = new LoginRequest
        {
            Email = "inactive@example.com",
            Password = "password123"
        };

        // Act
        var result = await _service.LoginAsync(loginRequest);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies successful login returns auth response with token
    /// </summary>
    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        var password = "SecurePassword123!";
        var user = new User
        {
            Id = 3,
            Email = "valid@example.com",
            Username = "validuser",
            FirstName = "Valid",
            LastName = "User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            IsActive = true,
            IsDeleted = false,
            Role = (int)UserRole.Sales,
            CreatedAt = DateTime.UtcNow
        };
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var loginRequest = new LoginRequest
        {
            Email = "valid@example.com",
            Password = password
        };

        // Act
        var result = await _service.LoginAsync(loginRequest);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.Email.Should().Be("valid@example.com");
        result.Username.Should().Be("validuser");
    }

    #endregion

    #region Registration Tests

    /// <summary>
    /// Verifies registration fails for existing email
    /// </summary>
    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ReturnsNull()
    {
        // Arrange
        var existingUser = new User
        {
            Id = 10,
            Email = "existing@example.com",
            Username = "existinguser",
            FirstName = "Existing",
            LastName = "User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await _dbContext.Users.AddAsync(existingUser);
        await _dbContext.SaveChangesAsync();

        var registerRequest = new RegisterRequest
        {
            Email = "existing@example.com",
            Password = "NewPassword123!",
            Username = "newuser",
            FirstName = "New",
            LastName = "User"
        };

        // Act
        var result = await _service.RegisterAsync(registerRequest);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies successful registration creates user
    /// </summary>
    [Fact]
    public async Task RegisterAsync_WithValidData_ReturnsAuthResponse()
    {
        // Arrange
        _mockSystemSettings
            .Setup(x => x.GetSettingAsync("RequireApproval"))
            .ReturnsAsync((SystemSettings?)null);

        var registerRequest = new RegisterRequest
        {
            Email = "newuser@example.com",
            Password = "SecurePassword123!",
            Username = "newuser",
            FirstName = "New",
            LastName = "User"
        };

        // Act
        var result = await _service.RegisterAsync(registerRequest);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("newuser@example.com");
        result.Username.Should().Be("newuser");
        
        // Verify user was created in database
        var createdUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == "newuser@example.com");
        createdUser.Should().NotBeNull();
    }

    #endregion

    #region Password Validation Tests

    /// <summary>
    /// Verifies password hashing works correctly
    /// </summary>
    [Fact]
    public void PasswordHash_ShouldBeVerifiable()
    {
        // Arrange
        var password = "TestPassword123!";
        var hash = BCrypt.Net.BCrypt.HashPassword(password);

        // Assert
        BCrypt.Net.BCrypt.Verify(password, hash).Should().BeTrue();
        BCrypt.Net.BCrypt.Verify("wrongpassword", hash).Should().BeFalse();
    }

    #endregion

    #region Two-Factor Authentication Tests

    /// <summary>
    /// Verifies 2FA login request generates two-factor token when enabled
    /// </summary>
    [Fact]
    public async Task LoginAsync_WithTwoFactorEnabled_RequiresTwoFactorCode()
    {
        // Arrange
        var password = "SecurePassword123!";
        var user = new User
        {
            Id = 20,
            Email = "2fa@example.com",
            Username = "2fauser",
            FirstName = "TwoFactor",
            LastName = "User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            IsActive = true,
            IsDeleted = false,
            Role = (int)UserRole.Sales,
            TwoFactorEnabled = true,
            TwoFactorSecret = "JBSWY3DPEHPK3PXP", // Sample TOTP secret
            CreatedAt = DateTime.UtcNow
        };
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var loginRequest = new LoginRequest
        {
            Email = "2fa@example.com",
            Password = password
        };

        // Act
        var result = await _service.LoginAsync(loginRequest);

        // Assert
        result.Should().NotBeNull();
        result!.RequiresTwoFactor.Should().BeTrue();
        result.TwoFactorToken.Should().NotBeNullOrEmpty();
        result.Token.Should().BeNullOrEmpty(); // Token not issued until 2FA verified
    }

    /// <summary>
    /// Verifies 2FA verification with invalid token returns null
    /// </summary>
    [Fact]
    public async Task VerifyTwoFactorLoginAsync_WithInvalidToken_ReturnsNull()
    {
        // Arrange
        var request = new TwoFactorLoginRequest
        {
            TwoFactorToken = "invalid_token",
            Code = "123456"
        };

        // Act
        var result = await _service.VerifyTwoFactorLoginAsync(request);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Token Tests

    /// <summary>
    /// Verifies JWT token contains expected claims
    /// </summary>
    [Fact]
    public async Task LoginAsync_ReturnsTokenWithCorrectClaims()
    {
        // Arrange
        var password = "SecurePassword123!";
        var user = new User
        {
            Id = 30,
            Email = "claims@example.com",
            Username = "claimsuser",
            FirstName = "Claims",
            LastName = "User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            IsActive = true,
            IsDeleted = false,
            Role = (int)UserRole.Admin,
            CreatedAt = DateTime.UtcNow
        };
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var loginRequest = new LoginRequest
        {
            Email = "claims@example.com",
            Password = password
        };

        // Act
        var result = await _service.LoginAsync(loginRequest);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.Role.Should().Be("Admin");
    }

    #endregion

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
