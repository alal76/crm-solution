// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using CRM.Core.Entities;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for JwtTokenService
/// Covers: Token generation, validation, refresh tokens
/// </summary>
public class JwtTokenServiceTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly JwtTokenService _service;

    public JwtTokenServiceTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();

        SetupConfiguration();

        _service = new JwtTokenService(
            _mockConfiguration.Object);
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
            Role = (int)UserRole.Sales
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
            Role = (int)UserRole.Admin
        };

        // Act
        var token = _service.GenerateAccessToken(user);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Assert
        jwtToken.Claims.Should().Contain(c => c.Type == "nameid" && c.Value == "1");
        jwtToken.Claims.Should().Contain(c => c.Type == "email" && c.Value == "test@example.com");
        jwtToken.Claims.Should().Contain(c => c.Type == "unique_name" && c.Value == "testuser");
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
            Role = (int)UserRole.Admin
        };

        // Act
        var token = _service.GenerateAccessToken(user);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Assert
        jwtToken.Claims.Should().Contain(c => c.Type == "role" && c.Value == "Admin");
    }

    [Fact]
    public void GenerateAccessToken_NullUser_ThrowsException()
    {
        // Act & Assert
        Assert.ThrowsAny<Exception>(() => _service.GenerateAccessToken(null!));
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
            Role = (int)UserRole.Sales
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
            Role = (int)UserRole.Sales
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
            Role = (int)UserRole.Sales
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
        // Arrange - manually create an expired token with NotBefore in the past
        var handler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes("YourSecretKeyHereThatIsAtLeast32CharactersLong!");
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim("nameid", "1") }),
            NotBefore = DateTime.UtcNow.AddMinutes(-10),
            Expires = DateTime.UtcNow.AddMinutes(-5),
            Issuer = "CRM.Tests",
            Audience = "CRM.Tests.Users",
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var secToken = handler.CreateToken(tokenDescriptor);
        var token = handler.WriteToken(secToken);

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
            Role = (int)UserRole.Sales
        };
        var token = _service.GenerateAccessToken(user);

        // Act
        var userId = _service.GetUserIdFromToken(token);

        // Assert
        userId.Should().Be(123);
    }

    [Fact]
    public void GetUserIdFromToken_InvalidToken_ReturnsZero()
    {
        // Act
        var userId = _service.GetUserIdFromToken("invalid.token");

        // Assert
        userId.Should().Be(0);
    }

    #endregion

    #region Token with Custom Claims Tests

    [Fact]
    public void GenerateAccessToken_WithDepartment_TokenContainsDepartment()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            Role = (int)UserRole.Sales,
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
            Role = (int)UserRole.Sales
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
            Role = (int)UserRole.Sales
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
            Role = (int)UserRole.Sales
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
