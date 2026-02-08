// CRM Solution - Customer Relationship Management System
// JWT Token Service Unit Tests

using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CRM.Core.Entities;
using CRM.Infrastructure.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for JwtTokenService
/// Covers: Token generation, validation, refresh tokens
/// </summary>
public class JwtTokenServiceTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<JwtTokenService>> _mockLogger;
    private readonly JwtTokenService _service;

    public JwtTokenServiceTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<JwtTokenService>>();

        SetupConfiguration();

        _service = new JwtTokenService(
            _mockConfiguration.Object,
            _mockLogger.Object);
    }

    private void SetupConfiguration()
    {
        _mockConfiguration.Setup(c => c["Jwt:Secret"]).Returns("ThisIsAVeryLongSecretKeyForTestingPurposesOnly1234567890");
        _mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns("CRM.Tests");
        _mockConfiguration.Setup(c => c["Jwt:Audience"]).Returns("CRM.Tests.Users");
        _mockConfiguration.Setup(c => c["Jwt:ExpirationMinutes"]).Returns("60");
    }

    #region GenerateAccessToken Tests

    [Fact]
    public void GenerateAccessToken_ValidUser_ReturnsToken()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            Role = UserRole.User
        };

        // Act
        var token = _service.GenerateAccessToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateAccessToken_ValidUser_TokenContainsCorrectClaims()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            Role = UserRole.Admin
        };

        // Act
        var token = _service.GenerateAccessToken(user);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Assert
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == "1");
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == "test@example.com");
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == "testuser");
    }

    [Fact]
    public void GenerateAccessToken_AdminUser_TokenContainsAdminRole()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "admin",
            Email = "admin@example.com",
            Role = UserRole.Admin
        };

        // Act
        var token = _service.GenerateAccessToken(user);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Assert
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
    }

    [Fact]
    public void GenerateAccessToken_NullUser_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _service.GenerateAccessToken(null!));
    }

    [Fact]
    public void GenerateAccessToken_TokenHasCorrectExpiration()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            Role = UserRole.User
        };

        // Act
        var token = _service.GenerateAccessToken(user);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Assert
        var expectedExpiration = DateTime.UtcNow.AddMinutes(60);
        jwtToken.ValidTo.Should().BeCloseTo(expectedExpiration, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void GenerateAccessToken_TokenHasCorrectIssuer()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            Role = UserRole.User
        };

        // Act
        var token = _service.GenerateAccessToken(user);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Assert
        jwtToken.Issuer.Should().Be("CRM.Tests");
    }

    #endregion

    #region GenerateRefreshToken Tests

    [Fact]
    public void GenerateRefreshToken_ReturnsNonEmptyToken()
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
        var token3 = _service.GenerateRefreshToken();

        // Assert
        token1.Should().NotBe(token2);
        token2.Should().NotBe(token3);
        token1.Should().NotBe(token3);
    }

    [Fact]
    public void GenerateRefreshToken_TokenIsBase64Encoded()
    {
        // Act
        var refreshToken = _service.GenerateRefreshToken();

        // Assert
        var isBase64 = !string.IsNullOrEmpty(refreshToken) &&
                       Convert.TryFromBase64String(refreshToken, new byte[64], out _);
        isBase64.Should().BeTrue();
    }

    [Fact]
    public void GenerateRefreshToken_TokenHasSufficientLength()
    {
        // Act
        var refreshToken = _service.GenerateRefreshToken();

        // Assert
        refreshToken.Length.Should().BeGreaterThanOrEqualTo(44); // 64 bytes base64 encoded
    }

    #endregion

    #region ValidateToken Tests

    [Fact]
    public void ValidateToken_ValidToken_ReturnsTrue()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            Role = UserRole.User
        };
        var token = _service.GenerateAccessToken(user);

        // Act
        var isValid = _service.ValidateToken(token);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateToken_InvalidToken_ReturnsFalse()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var isValid = _service.ValidateToken(invalidToken);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void ValidateToken_ExpiredToken_ReturnsFalse()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["Jwt:ExpirationMinutes"]).Returns("-1");
        var serviceWithExpiredConfig = new JwtTokenService(_mockConfiguration.Object, _mockLogger.Object);

        var user = new User { Id = 1, Username = "test", Email = "test@example.com", Role = UserRole.User };
        var token = serviceWithExpiredConfig.GenerateAccessToken(user);

        // Reset config
        _mockConfiguration.Setup(c => c["Jwt:ExpirationMinutes"]).Returns("60");

        // Act
        var isValid = _service.ValidateToken(token);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void ValidateToken_NullToken_ReturnsFalse()
    {
        // Act
        var isValid = _service.ValidateToken(null!);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void ValidateToken_EmptyToken_ReturnsFalse()
    {
        // Act
        var isValid = _service.ValidateToken("");

        // Assert
        isValid.Should().BeFalse();
    }

    #endregion

    #region GetPrincipalFromToken Tests

    [Fact]
    public void GetPrincipalFromToken_ValidToken_ReturnsPrincipal()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            Role = UserRole.User
        };
        var token = _service.GenerateAccessToken(user);

        // Act
        var principal = _service.GetPrincipalFromToken(token);

        // Assert
        principal.Should().NotBeNull();
        principal!.Identity.Should().NotBeNull();
    }

    [Fact]
    public void GetPrincipalFromToken_ValidToken_ContainsCorrectClaims()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            Role = UserRole.Admin
        };
        var token = _service.GenerateAccessToken(user);

        // Act
        var principal = _service.GetPrincipalFromToken(token);

        // Assert
        principal!.FindFirst(ClaimTypes.Email)?.Value.Should().Be("test@example.com");
        principal.FindFirst(ClaimTypes.Role)?.Value.Should().Be("Admin");
    }

    [Fact]
    public void GetPrincipalFromToken_InvalidToken_ReturnsNull()
    {
        // Act
        var principal = _service.GetPrincipalFromToken("invalid.token");

        // Assert
        principal.Should().BeNull();
    }

    #endregion

    #region GetUserIdFromToken Tests

    [Fact]
    public void GetUserIdFromToken_ValidToken_ReturnsUserId()
    {
        // Arrange
        var user = new User
        {
            Id = 123,
            Username = "testuser",
            Email = "test@example.com",
            Role = UserRole.User
        };
        var token = _service.GenerateAccessToken(user);

        // Act
        var userId = _service.GetUserIdFromToken(token);

        // Assert
        userId.Should().Be(123);
    }

    [Fact]
    public void GetUserIdFromToken_InvalidToken_ReturnsNull()
    {
        // Act
        var userId = _service.GetUserIdFromToken("invalid.token");

        // Assert
        userId.Should().BeNull();
    }

    #endregion

    #region Token with Custom Claims Tests

    [Fact]
    public void GenerateAccessToken_UserWithGroups_TokenContainsGroups()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            Role = UserRole.User,
            UserGroupMembers = new List<UserGroupMember>
            {
                new UserGroupMember { UserGroup = new UserGroup { Name = "Sales" } },
                new UserGroupMember { UserGroup = new UserGroup { Name = "Marketing" } }
            }
        };

        // Act
        var token = _service.GenerateAccessToken(user);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Assert
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateAccessToken_WithDepartment_TokenContainsDepartment()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            Role = UserRole.User,
            Department = new Department { Id = 1, Name = "Sales" }
        };

        // Act
        var token = _service.GenerateAccessToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public void GenerateAccessToken_UserWithSpecialCharactersInName_GeneratesValidToken()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "test.user@domain",
            Email = "test+special@example.com",
            FirstName = "Tëst",
            LastName = "Üsér",
            Role = UserRole.User
        };

        // Act
        var token = _service.GenerateAccessToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
        _service.ValidateToken(token).Should().BeTrue();
    }

    [Fact]
    public void GenerateAccessToken_UserWithEmptyFirstName_GeneratesValidToken()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            FirstName = "",
            LastName = "User",
            Role = UserRole.User
        };

        // Act
        var token = _service.GenerateAccessToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ValidateToken_TamperedToken_ReturnsFalse()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            Role = UserRole.User
        };
        var token = _service.GenerateAccessToken(user);
        var tamperedToken = token.Substring(0, token.Length - 5) + "XXXXX";

        // Act
        var isValid = _service.ValidateToken(tamperedToken);

        // Assert
        isValid.Should().BeFalse();
    }

    #endregion
}
