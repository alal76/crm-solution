// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for AuthenticationService.
/// Tests login, registration, token management, and 2FA functionality.
/// </summary>
public class AuthenticationServiceTests
{
    private readonly Mock<IRepository<User>> _mockUserRepository;
    private readonly Mock<IRepository<OAuthToken>> _mockOAuthTokenRepository;
    private readonly CrmDbContext _dbContext;
    private readonly Mock<IJwtTokenService> _mockJwtTokenService;
    private readonly Mock<CRM.Core.Interfaces.ITotpService> _mockTotpService;
    private readonly Mock<IMemoryCache> _mockMemoryCache;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<INotificationPort> _mockNotificationPort;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<AuthenticationService>> _mockLogger;
    private readonly AuthenticationService _service;

    public AuthenticationServiceTests()
    {
        _mockUserRepository = new Mock<IRepository<User>>();
        _mockOAuthTokenRepository = new Mock<IRepository<OAuthToken>>();

        // CrmDbContext requires constructor args; use InMemory options to enable mocking
        // create in-memory database context for testing
        var dbOptions = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new CrmDbContext(dbOptions, new Mock<IConfiguration>().Object);

        _mockJwtTokenService = new Mock<IJwtTokenService>();
        _mockTotpService = new Mock<CRM.Core.Interfaces.ITotpService>();
        _mockMemoryCache = new Mock<IMemoryCache>();
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockNotificationPort = new Mock<INotificationPort>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<AuthenticationService>>();

        _service = new AuthenticationService(
            _mockUserRepository.Object,
            _mockOAuthTokenRepository.Object,
            _dbContext,
            _mockJwtTokenService.Object,
            _mockTotpService.Object,
            _mockMemoryCache.Object,
            _mockHttpClientFactory.Object,
            _mockNotificationPort.Object,
            _mockConfiguration.Object,
            _mockLogger.Object);
    }

    #region RegisterAsync Tests

    [Fact]
    public async Task RegisterAsync_WithValidData_CreatesNewUser()
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

        var mockUserSet = MockDbSetFactory.CreateMockDbSet(new List<User>());
        var mockApprovalSet = MockDbSetFactory.CreateMockDbSet(new List<UserApprovalRequest>());
        var mockSettingsSet = MockDbSetFactory.CreateMockDbSet(new List<SystemSettings>
        {
            new() { RequireApprovalForNewUsers = false }
        });

        // other tests will configure Users/RefreshTokens via interface as needed
        // seed the in-memory context directly
        _dbContext.Users.AddRange(mockUserSet.Object);
        _dbContext.UserApprovalRequests.AddRange(mockApprovalSet.Object);
        _dbContext.SystemSettings.AddRange(mockSettingsSet.Object);
        await _dbContext.SaveChangesAsync();
        _mockJwtTokenService.Setup(j => j.GenerateAccessToken(It.IsAny<User>()))
            .Returns("valid_access_token");
        _mockJwtTokenService.Setup(j => j.GenerateRefreshToken())
            .Returns("valid_refresh_token");

        // Act
        var result = await _service.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RegisterAsync_WithMismatchedPasswords_ThrowsException()
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

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.RegisterAsync(request));
    }

    [Fact]
    public async Task RegisterAsync_WithMissingEmail_ThrowsException()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = string.Empty,
            FirstName = "John",
            LastName = "Doe",
            Password = "SecurePassword@123",
            ConfirmPassword = "SecurePassword@123"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.RegisterAsync(request));
    }

    #endregion

    #region LoginAsync Tests

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        var password = "SecurePassword@123";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            Username = "testuser",
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = passwordHash,
            Role = 2,
            IsActive = true,
            IsLocked = false,
            CreatedAt = DateTime.UtcNow
        };

        var request = new LoginRequest { Email = "test@example.com", Password = password };

        // seed in-memory context directly
        _dbContext.Users.Add(user);
        _dbContext.RefreshTokens.AddRange(new List<RefreshToken>());
        await _dbContext.SaveChangesAsync();
        _mockJwtTokenService.Setup(j => j.GenerateAccessToken(It.IsAny<User>()))
            .Returns("valid_access_token");
        _mockJwtTokenService.Setup(j => j.GenerateRefreshToken())
            .Returns("valid_refresh_token");

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.User.Should().NotBeNull();
        result.User!.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task LoginAsync_WithInvalidEmail_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest { Email = "nonexistent@example.com", Password = "AnyPassword" };

        // no users in database
        await _dbContext.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_WithIncorrectPassword_ReturnsUnauthorized()
    {
        // Arrange
        var password = "SecurePassword@123";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            Username = "testuser",
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = passwordHash,
            Role = 2,
            IsActive = true,
            IsLocked = false,
            CreatedAt = DateTime.UtcNow
        };

        var request = new LoginRequest { Email = "test@example.com", Password = "WrongPassword" };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_WithInactiveUser_ReturnsUnauthorized()
    {
        // Arrange
        var password = "SecurePassword@123";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            Username = "testuser",
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = passwordHash,
            Role = 2,
            IsActive = false,
            IsLocked = false,
            CreatedAt = DateTime.UtcNow
        };

        var request = new LoginRequest { Email = "test@example.com", Password = password };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.LoginAsync(request));
    }

    #endregion

    #region LogoutAsync Tests

    [Fact]
    public async Task LogoutAsync_WithValidUserId_RevokesRefreshTokens()
    {
        // Arrange
        var userId = 1;
        var refreshToken = new RefreshToken
        {
            Id = 1,
            Token = "valid_token",
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            RevokedAt = null
        };

        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync();

        // Act
        await _service.LogoutAsync(userId);

        // Assert
        refreshToken.RevokedAt.Should().NotBeNull();
    }

    #endregion

    #region RefreshAccessTokenAsync Tests

    [Fact]
    public async Task RefreshAccessTokenAsync_WithValidRefreshToken_ReturnsNewAccessToken()
    {
        // Arrange
        var userId = 1;
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            Username = "testuser",
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = "hash",
            Role = 2,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var refreshToken = new RefreshToken
        {
            Id = 1,
            Token = "valid_refresh_token",
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            RevokedAt = null
        };

        _dbContext.Users.Add(user);
        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync();
        _mockJwtTokenService.Setup(j => j.GenerateAccessToken(It.IsAny<User>()))
            .Returns("new_access_token");
        _mockJwtTokenService.Setup(j => j.GenerateRefreshToken())
            .Returns("rotated_refresh_token");

        // Act
        var result = await _service.RefreshAccessTokenAsync("valid_refresh_token");

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("new_access_token");
    }

    [Fact]
    public async Task RefreshAccessTokenAsync_WithExpiredRefreshToken_ThrowsException()
    {
        // Arrange
        var refreshToken = new RefreshToken
        {
            Id = 1,
            Token = "expired_refresh_token",
            UserId = 1,
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            RevokedAt = null
        };

        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.RefreshAccessTokenAsync("expired_refresh_token"));
    }

    #endregion
}
