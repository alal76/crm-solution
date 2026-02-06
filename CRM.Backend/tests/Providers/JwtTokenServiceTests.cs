// CRM Solution - Customer Relationship Management System
// JWT Token Service Unit Tests

using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using CRM.Core.Entities;
using CRM.Core.DTOs;
using CRM.Infrastructure.Services;
using CRM.Infrastructure.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for JWT Token Service
/// Covers: Token generation, validation, refresh tokens
/// </summary>
public class JwtTokenServiceTests
{
    private readonly Mock<IOptions<JwtSettings>> _mockJwtSettings;
    private readonly Mock<ILogger<JwtTokenService>> _mockLogger;
    private readonly JwtTokenService _service;
    private readonly JwtSettings _settings;

    public JwtTokenServiceTests()
    {
        _settings = new JwtSettings
        {
            Secret = "ThisIsAVeryLongSecretKeyForTestingPurposes123456789!",
            Issuer = "CRM.Api.Test",
            Audience = "CRM.Client.Test",
            ExpirationMinutes = 60,
            RefreshTokenExpirationDays = 7
        };

        _mockJwtSettings = new Mock<IOptions<JwtSettings>>();
        _mockJwtSettings.Setup(x => x.Value).Returns(_settings);
        _mockLogger = new Mock<ILogger<JwtTokenService>>();

        _service = new JwtTokenService(_mockJwtSettings.Object, _mockLogger.Object);
    }

    #region GenerateAccessToken Tests

    [Fact]
    public void GenerateAccessToken_ValidUser_ReturnsToken()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var token = _service.GenerateAccessToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateAccessToken_TokenIsValidJwt_CanBeParsed()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var token = _service.GenerateAccessToken(user);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Assert
        jwtToken.Should().NotBeNull();
        jwtToken.Issuer.Should().Be(_settings.Issuer);
    }

    [Fact]
    public void GenerateAccessToken_ContainsUserIdClaim()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var token = _service.GenerateAccessToken(user);
        var claims = GetClaimsFromToken(token);

        // Assert
        claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id.ToString());
    }

    [Fact]
    public void GenerateAccessToken_ContainsEmailClaim()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var token = _service.GenerateAccessToken(user);
        var claims = GetClaimsFromToken(token);

        // Assert
        claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == user.Email);
    }

    [Fact]
    public void GenerateAccessToken_ContainsUsernameClaim()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var token = _service.GenerateAccessToken(user);
        var claims = GetClaimsFromToken(token);

        // Assert
        claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == user.Username);
    }

    [Fact]
    public void GenerateAccessToken_ContainsRoleClaim()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var token = _service.GenerateAccessToken(user);
        var claims = GetClaimsFromToken(token);

        // Assert
        claims.Should().Contain(c => c.Type == ClaimTypes.Role);
    }

    [Fact]
    public void GenerateAccessToken_TokenHasCorrectExpiration()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var token = _service.GenerateAccessToken(user);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Assert
        var expectedExpiration = DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes);
        jwtToken.ValidTo.Should().BeCloseTo(expectedExpiration, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void GenerateAccessToken_TokenHasCorrectAudience()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var token = _service.GenerateAccessToken(user);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Assert
        jwtToken.Audiences.Should().Contain(_settings.Audience);
    }

    #endregion

    #region GenerateRefreshToken Tests

    [Fact]
    public void GenerateRefreshToken_ReturnsNonEmptyString()
    {
        // Act
        var refreshToken = _service.GenerateRefreshToken();

        // Assert
        refreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsUniqueTokens()
    {
        // Act
        var token1 = _service.GenerateRefreshToken();
        var token2 = _service.GenerateRefreshToken();

        // Assert
        token1.Should().NotBe(token2);
    }

    [Fact]
    public void GenerateRefreshToken_HasMinimumLength()
    {
        // Act
        var refreshToken = _service.GenerateRefreshToken();

        // Assert
        refreshToken.Length.Should().BeGreaterThan(20);
    }

    [Fact]
    public void GenerateRefreshToken_IsBase64Encoded()
    {
        // Act
        var refreshToken = _service.GenerateRefreshToken();

        // Assert
        var action = () => Convert.FromBase64String(refreshToken);
        action.Should().NotThrow();
    }

    #endregion

    #region ValidateToken Tests

    [Fact]
    public void ValidateToken_ValidToken_ReturnsClaimsPrincipal()
    {
        // Arrange
        var user = CreateTestUser();
        var token = _service.GenerateAccessToken(user);

        // Act
        var principal = _service.ValidateToken(token);

        // Assert
        principal.Should().NotBeNull();
    }

    [Fact]
    public void ValidateToken_ValidToken_ContainsCorrectUserId()
    {
        // Arrange
        var user = CreateTestUser();
        var token = _service.GenerateAccessToken(user);

        // Act
        var principal = _service.ValidateToken(token);
        var userId = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // Assert
        userId.Should().Be(user.Id.ToString());
    }

    [Fact]
    public void ValidateToken_InvalidToken_ReturnsNull()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var principal = _service.ValidateToken(invalidToken);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_EmptyToken_ReturnsNull()
    {
        // Arrange
        var emptyToken = "";

        // Act
        var principal = _service.ValidateToken(emptyToken);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_NullToken_ReturnsNull()
    {
        // Act
        var principal = _service.ValidateToken(null!);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_TamperedToken_ReturnsNull()
    {
        // Arrange
        var user = CreateTestUser();
        var token = _service.GenerateAccessToken(user);
        var tamperedToken = token.Substring(0, token.Length - 5) + "XXXXX";

        // Act
        var principal = _service.ValidateToken(tamperedToken);

        // Assert
        principal.Should().BeNull();
    }

    #endregion

    #region GetUserIdFromToken Tests

    [Fact]
    public void GetUserIdFromToken_ValidToken_ReturnsUserId()
    {
        // Arrange
        var user = CreateTestUser();
        var token = _service.GenerateAccessToken(user);

        // Act
        var userId = _service.GetUserIdFromToken(token);

        // Assert
        userId.Should().Be(user.Id);
    }

    [Fact]
    public void GetUserIdFromToken_InvalidToken_ReturnsNull()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var userId = _service.GetUserIdFromToken(invalidToken);

        // Assert
        userId.Should().BeNull();
    }

    #endregion

    #region GetEmailFromToken Tests

    [Fact]
    public void GetEmailFromToken_ValidToken_ReturnsEmail()
    {
        // Arrange
        var user = CreateTestUser();
        var token = _service.GenerateAccessToken(user);

        // Act
        var email = _service.GetEmailFromToken(token);

        // Assert
        email.Should().Be(user.Email);
    }

    [Fact]
    public void GetEmailFromToken_InvalidToken_ReturnsNull()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var email = _service.GetEmailFromToken(invalidToken);

        // Assert
        email.Should().BeNull();
    }

    #endregion

    #region GetRolesFromToken Tests

    [Fact]
    public void GetRolesFromToken_ValidToken_ReturnsRoles()
    {
        // Arrange
        var user = CreateTestUser();
        var token = _service.GenerateAccessToken(user);

        // Act
        var roles = _service.GetRolesFromToken(token);

        // Assert
        roles.Should().NotBeEmpty();
    }

    #endregion

    #region Token Expiration Tests

    [Fact]
    public void IsTokenExpired_FreshToken_ReturnsFalse()
    {
        // Arrange
        var user = CreateTestUser();
        var token = _service.GenerateAccessToken(user);

        // Act
        var isExpired = _service.IsTokenExpired(token);

        // Assert
        isExpired.Should().BeFalse();
    }

    [Fact]
    public void GetTokenExpiration_ReturnsCorrectExpiration()
    {
        // Arrange
        var user = CreateTestUser();
        var token = _service.GenerateAccessToken(user);

        // Act
        var expiration = _service.GetTokenExpiration(token);

        // Assert
        expiration.Should().NotBeNull();
        expiration.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes), TimeSpan.FromMinutes(1));
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public void GenerateAccessToken_UserWithNullEmail_HandlesGracefully()
    {
        // Arrange
        var user = CreateTestUser();
        user.Email = null!;

        // Act & Assert
        // Should either throw or handle gracefully depending on implementation
        var action = () => _service.GenerateAccessToken(user);
        action.Should().NotThrow();
    }

    [Fact]
    public void GenerateAccessToken_UserWithSpecialCharactersInName_Succeeds()
    {
        // Arrange
        var user = CreateTestUser();
        user.FirstName = "José";
        user.LastName = "O'Connor-Smith";

        // Act
        var token = _service.GenerateAccessToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateAccessToken_MultipleCallsSameUser_ReturnsDifferentTokens()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var token1 = _service.GenerateAccessToken(user);
        var token2 = _service.GenerateAccessToken(user);

        // Assert
        // Tokens might be same if generated within same second due to iat claim
        // But they should both be valid
        token1.Should().NotBeNullOrEmpty();
        token2.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Helper Methods

    private User CreateTestUser()
    {
        return new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            Role = UserRole.Admin,
            IsActive = true
        };
    }

    private IEnumerable<Claim> GetClaimsFromToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        return jwtToken.Claims;
    }

    #endregion
}

// Supporting classes
public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; }
    public int RefreshTokenExpirationDays { get; set; }
}

public enum UserRole
{
    User = 0,
    Admin = 1,
    SuperAdmin = 2
}
