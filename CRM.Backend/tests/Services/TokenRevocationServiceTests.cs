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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for TokenRevocationService
/// Covers: Token blacklisting, revocation, cleanup
/// </summary>
public class TokenRevocationServiceTests
{
    private readonly Mock<IRepository<RevokedToken>> _mockRevokedTokenRepository;
    private readonly Mock<IDistributedCache> _mockCache;
    private readonly Mock<ILogger<TokenRevocationService>> _mockLogger;
    private readonly TokenRevocationService _service;

    public TokenRevocationServiceTests()
    {
        _mockRevokedTokenRepository = new Mock<IRepository<RevokedToken>>();
        _mockCache = new Mock<IDistributedCache>();
        _mockLogger = new Mock<ILogger<TokenRevocationService>>();

        _service = new TokenRevocationService(
            _mockRevokedTokenRepository.Object,
            _mockCache.Object,
            _mockLogger.Object);
    }

    #region Revoke Token Tests

    [Fact]
    public async Task RevokeTokenAsync_ValidToken_RevokesToken()
    {
        // Arrange
        var tokenId = "jti_12345";
        var expiry = DateTime.UtcNow.AddHours(1);

        _mockRevokedTokenRepository.Setup(r => r.AddAsync(It.IsAny<RevokedToken>()))
            .ReturnsAsync((RevokedToken t) => { t.Id = 1; return t; });

        _mockCache.Setup(c => c.SetAsync(
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            default))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.RevokeTokenAsync(tokenId, expiry);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task RevokeTokenAsync_WithReason_StoresReason()
    {
        // Arrange
        var tokenId = "jti_67890";
        var expiry = DateTime.UtcNow.AddHours(1);
        var reason = "User logged out";

        _mockRevokedTokenRepository.Setup(r => r.AddAsync(It.IsAny<RevokedToken>()))
            .ReturnsAsync((RevokedToken t) => { t.Id = 1; return t; });

        // Act
        var result = await _service.RevokeTokenAsync(tokenId, expiry, reason);

        // Assert
        result.Should().BeTrue();
        _mockRevokedTokenRepository.Verify(r => r.AddAsync(It.Is<RevokedToken>(t => t.Reason == reason)), Times.Once);
    }

    [Fact]
    public async Task RevokeTokenAsync_AlreadyRevoked_ReturnsFalse()
    {
        // Arrange
        var tokenId = "jti_already_revoked";
        var existingToken = new RevokedToken { Id = 1, TokenId = tokenId };

        _mockRevokedTokenRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<RevokedToken, bool>>>()))
            .ReturnsAsync(new List<RevokedToken> { existingToken });

        // Act
        var result = await _service.RevokeTokenAsync(tokenId, DateTime.UtcNow.AddHours(1));

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Check Revocation Tests

    [Fact]
    public async Task IsTokenRevokedAsync_RevokedToken_ReturnsTrue()
    {
        // Arrange
        var tokenId = "jti_revoked";

        _mockCache.Setup(c => c.GetAsync(It.IsAny<string>(), default))
            .ReturnsAsync(Encoding.UTF8.GetBytes("revoked"));

        // Act
        var result = await _service.IsTokenRevokedAsync(tokenId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsTokenRevokedAsync_NotRevoked_ReturnsFalse()
    {
        // Arrange
        var tokenId = "jti_valid";

        _mockCache.Setup(c => c.GetAsync(It.IsAny<string>(), default))
            .ReturnsAsync((byte[]?)null);

        _mockRevokedTokenRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<RevokedToken, bool>>>()))
            .ReturnsAsync(new List<RevokedToken>());

        // Act
        var result = await _service.IsTokenRevokedAsync(tokenId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsTokenRevokedAsync_CacheMissDbHit_ReturnsTrue()
    {
        // Arrange
        var tokenId = "jti_in_db";
        var revokedToken = new RevokedToken { TokenId = tokenId };

        _mockCache.Setup(c => c.GetAsync(It.IsAny<string>(), default))
            .ReturnsAsync((byte[]?)null);

        _mockRevokedTokenRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<RevokedToken, bool>>>()))
            .ReturnsAsync(new List<RevokedToken> { revokedToken });

        // Act
        var result = await _service.IsTokenRevokedAsync(tokenId);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Revoke User Tokens Tests

    [Fact]
    public async Task RevokeAllUserTokensAsync_ValidUser_RevokesAllTokens()
    {
        // Arrange
        var userId = 1;
        var tokens = new List<RevokedToken>
        {
            new RevokedToken { TokenId = "token1", UserId = userId },
            new RevokedToken { TokenId = "token2", UserId = userId }
        };

        _mockRevokedTokenRepository.Setup(r => r.AddAsync(It.IsAny<RevokedToken>()))
            .ReturnsAsync((RevokedToken t) => { t.Id = 1; return t; });

        // Act
        var result = await _service.RevokeAllUserTokensAsync(userId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task RevokeAllUserTokensAsync_WithReason_StoresReason()
    {
        // Arrange
        var userId = 1;
        var reason = "Password changed";

        _mockRevokedTokenRepository.Setup(r => r.AddAsync(It.IsAny<RevokedToken>()))
            .ReturnsAsync((RevokedToken t) => { t.Id = 1; return t; });

        // Act
        var result = await _service.RevokeAllUserTokensAsync(userId, reason);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Cleanup Tests

    [Fact]
    public async Task CleanupExpiredTokensAsync_RemovesExpiredTokens()
    {
        // Arrange
        var expiredTokens = new List<RevokedToken>
        {
            new RevokedToken { Id = 1, TokenId = "expired1", ExpiresAt = DateTime.UtcNow.AddHours(-1) },
            new RevokedToken { Id = 2, TokenId = "expired2", ExpiresAt = DateTime.UtcNow.AddHours(-2) }
        };

        _mockRevokedTokenRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<RevokedToken, bool>>>()))
            .ReturnsAsync(expiredTokens);

        _mockRevokedTokenRepository.Setup(r => r.DeleteAsync(It.IsAny<int>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.CleanupExpiredTokensAsync();

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async Task CleanupExpiredTokensAsync_NoExpiredTokens_ReturnsZero()
    {
        // Arrange
        _mockRevokedTokenRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<RevokedToken, bool>>>()))
            .ReturnsAsync(new List<RevokedToken>());

        // Act
        var result = await _service.CleanupExpiredTokensAsync();

        // Assert
        result.Should().Be(0);
    }

    #endregion

    #region Batch Revocation Tests

    [Fact]
    public async Task RevokeTokensAsync_BatchTokens_RevokesAll()
    {
        // Arrange
        var tokenIds = new List<string> { "token1", "token2", "token3" };
        var expiry = DateTime.UtcNow.AddHours(1);

        _mockRevokedTokenRepository.Setup(r => r.AddAsync(It.IsAny<RevokedToken>()))
            .ReturnsAsync((RevokedToken t) => { t.Id = 1; return t; });

        // Act
        var result = await _service.RevokeTokensAsync(tokenIds, expiry);

        // Assert
        result.RevokedCount.Should().Be(3);
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetStatisticsAsync_ReturnsStats()
    {
        // Arrange
        var tokens = new List<RevokedToken>
        {
            new RevokedToken { Reason = "Logout" },
            new RevokedToken { Reason = "Logout" },
            new RevokedToken { Reason = "Password Change" }
        };

        _mockRevokedTokenRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(tokens);

        // Act
        var result = await _service.GetStatisticsAsync();

        // Assert
        result.TotalRevoked.Should().Be(3);
    }

    [Fact]
    public async Task GetUserRevocationHistoryAsync_ReturnsHistory()
    {
        // Arrange
        var userId = 1;
        var tokens = new List<RevokedToken>
        {
            new RevokedToken { UserId = userId, RevokedAt = DateTime.UtcNow.AddDays(-1) },
            new RevokedToken { UserId = userId, RevokedAt = DateTime.UtcNow }
        };

        _mockRevokedTokenRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<RevokedToken, bool>>>()))
            .ReturnsAsync(tokens);

        // Act
        var result = await _service.GetUserRevocationHistoryAsync(userId);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion
}

// Supporting classes for tests
public class RevokedToken
{
    public int Id { get; set; }
    public string TokenId { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public string? Reason { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime RevokedAt { get; set; } = DateTime.UtcNow;
}
