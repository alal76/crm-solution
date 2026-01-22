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
using Microsoft.Extensions.Caching.Memory;
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
/// - Uses mock repositories and services
/// - Tests service layer authentication logic
/// </summary>
public class AuthenticationServiceTests : IDisposable
{
    private readonly Mock<IRepository<User>> _mockUserRepo;
    private readonly Mock<IRepository<OAuthToken>> _mockOAuthTokenRepo;
    private readonly CrmDbContext _dbContext;
    private readonly Mock<IJwtTokenService> _mockJwtTokenService;
    private readonly Mock<ITotpService> _mockTotpService;
    private readonly IMemoryCache _memoryCache;
    private readonly Mock<ILogger<AuthenticationService>> _mockLogger;
    private readonly AuthenticationService _service;

    public AuthenticationServiceTests()
    {
        // Setup InMemory database
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _dbContext = new CrmDbContext(options, null);

        _mockUserRepo = new Mock<IRepository<User>>();
        _mockOAuthTokenRepo = new Mock<IRepository<OAuthToken>>();
        _mockJwtTokenService = new Mock<IJwtTokenService>();
        _mockTotpService = new Mock<ITotpService>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _mockLogger = new Mock<ILogger<AuthenticationService>>();

        // Setup JWT token service mock to return valid tokens
        _mockJwtTokenService.Setup(x => x.GenerateAccessToken(It.IsAny<User>()))
            .Returns("test-access-token");
        _mockJwtTokenService.Setup(x => x.GenerateRefreshToken())
            .Returns("test-refresh-token");

        _service = new AuthenticationService(
            _mockUserRepo.Object,
            _mockOAuthTokenRepo.Object,
            _dbContext,
            _mockJwtTokenService.Object,
            _mockTotpService.Object,
            _memoryCache,
            _mockLogger.Object);
    }

    #region Login Tests

    /// <summary>
    /// Verifies login throws exception for non-existent user
    /// </summary>
    [Fact]
    public async Task LoginAsync_WithNonExistentEmail_ThrowsUnauthorizedException()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "password123"
        };

        // Act
        Func<Task> act = async () => await _service.LoginAsync(loginRequest);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    /// <summary>
    /// Verifies login throws exception for incorrect password
    /// </summary>
    [Fact]
    public async Task LoginAsync_WithIncorrectPassword_ThrowsUnauthorizedException()
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
        Func<Task> act = async () => await _service.LoginAsync(loginRequest);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    /// <summary>
    /// Verifies login throws exception for inactive user
    /// </summary>
    [Fact]
    public async Task LoginAsync_WithInactiveUser_ThrowsUnauthorizedException()
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
        Func<Task> act = async () => await _service.LoginAsync(loginRequest);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    /// <summary>
    /// Verifies successful login returns auth response with tokens
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

        // Setup user repo for update
        _mockUserRepo.Setup(r => r.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
        _mockUserRepo.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

        var loginRequest = new LoginRequest
        {
            Email = "valid@example.com",
            Password = password
        };

        // Act
        var result = await _service.LoginAsync(loginRequest);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.Email.Should().Be("valid@example.com");
        result.Username.Should().Be("validuser");
    }

    #endregion

    #region Registration Tests

    /// <summary>
    /// Verifies registration fails for existing email
    /// </summary>
    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ThrowsException()
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

        _mockUserRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User> { existingUser });

        var registerRequest = new RegisterRequest
        {
            Email = "existing@example.com",
            Password = "NewPassword123!",
            ConfirmPassword = "NewPassword123!",
            Username = "newuser",
            FirstName = "New",
            LastName = "User"
        };

        // Act
        Func<Task> act = async () => await _service.RegisterAsync(registerRequest);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    /// <summary>
    /// Verifies registration fails when passwords don't match
    /// </summary>
    [Fact]
    public async Task RegisterAsync_WithMismatchedPasswords_ThrowsException()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Email = "newuser@example.com",
            Password = "Password123!",
            ConfirmPassword = "DifferentPassword123!",
            FirstName = "New",
            LastName = "User"
        };

        // Act
        Func<Task> act = async () => await _service.RegisterAsync(registerRequest);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Passwords do not match*");
    }

    /// <summary>
    /// Verifies successful registration creates user and returns tokens
    /// </summary>
    [Fact]
    public async Task RegisterAsync_WithValidData_ReturnsAuthResponse()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User>());
        _mockUserRepo.Setup(r => r.AddAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);
        _mockUserRepo.Setup(r => r.SaveAsync())
            .Returns(Task.CompletedTask);

        var registerRequest = new RegisterRequest
        {
            Email = "newuser@example.com",
            Password = "SecurePassword123!",
            ConfirmPassword = "SecurePassword123!",
            Username = "newuser",
            FirstName = "New",
            LastName = "User"
        };

        // Act
        var result = await _service.RegisterAsync(registerRequest);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be("newuser@example.com");
        result.FirstName.Should().Be("New");
        result.LastName.Should().Be("User");
    }

    /// <summary>
    /// Verifies registration fails with missing email
    /// </summary>
    [Fact]
    public async Task RegisterAsync_WithMissingEmail_ThrowsException()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Email = "",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FirstName = "New",
            LastName = "User"
        };

        // Act
        Func<Task> act = async () => await _service.RegisterAsync(registerRequest);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
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
        result.RequiresTwoFactor.Should().BeTrue();
        result.TwoFactorToken.Should().NotBeNullOrEmpty();
        result.AccessToken.Should().BeNullOrEmpty(); // Token not issued until 2FA verified
    }

    /// <summary>
    /// Verifies 2FA verification with invalid token throws exception
    /// </summary>
    [Fact]
    public async Task VerifyTwoFactorLoginAsync_WithInvalidToken_ThrowsException()
    {
        // Act
        Func<Task> act = async () => await _service.VerifyTwoFactorLoginAsync("invalid_token", "123456");

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    #endregion

    #region Token Refresh Tests

    /// <summary>
    /// Verifies token refresh with invalid token throws exception
    /// </summary>
    [Fact]
    public async Task RefreshTokenAsync_WithInvalidToken_ThrowsException()
    {
        // Act
        Func<Task> act = async () => await _service.RefreshTokenAsync("invalid-refresh-token");

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    #endregion

    #region Verify Token Tests

    /// <summary>
    /// Verifies VerifyTokenAsync returns false for invalid token
    /// </summary>
    [Fact]
    public async Task VerifyTokenAsync_WithInvalidToken_ReturnsFalse()
    {
        // Arrange
        _mockJwtTokenService.Setup(x => x.ValidateToken(It.IsAny<string>()))
            .Returns(false);

        // Act
        var result = await _service.VerifyTokenAsync("invalid-token");

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Verifies VerifyTokenAsync returns true for valid token
    /// </summary>
    [Fact]
    public async Task VerifyTokenAsync_WithValidToken_ReturnsTrue()
    {
        // Arrange
        _mockJwtTokenService.Setup(x => x.ValidateToken(It.IsAny<string>()))
            .Returns(true);

        // Act
        var result = await _service.VerifyTokenAsync("valid-token");

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    public void Dispose()
    {
        _memoryCache.Dispose();
        _dbContext.Dispose();
    }
}
