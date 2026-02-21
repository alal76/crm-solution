// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for TokenRevocationService.
/// Real constructor: (IDistributedCache cache, IConfiguration configuration, ILogger logger)
/// Real interface methods:
///   - RevokeTokenAsync(string token, int? expirationMinutes = null) → Task
///   - RevokeAllUserTokensAsync(int userId) → Task
///   - IsTokenRevokedAsync(string token) → Task&lt;bool&gt;
///   - IsUserTokenRevokedAsync(int userId, DateTime tokenIssuedAt) → Task&lt;bool&gt;
/// </summary>
public class TokenRevocationServiceTests
{
    private readonly Mock<IDistributedCache> _mockCache;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<TokenRevocationService>> _mockLogger;
    private readonly TokenRevocationService _service;

    public TokenRevocationServiceTests()
    {
        _mockCache = new Mock<IDistributedCache>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<TokenRevocationService>>();

        // Default config: Jwt:ExpirationMinutes = 60
        _mockConfiguration.Setup(c => c["Jwt:ExpirationMinutes"]).Returns("60");

        _service = new TokenRevocationService(
            _mockCache.Object,
            _mockConfiguration.Object,
            _mockLogger.Object);
    }

    #region RevokeTokenAsync Tests

    [Fact]
    public async Task RevokeTokenAsync_ValidToken_SetsInCache()
    {
        // Arrange
        var token = "eyJhbGciOiJIUzI1NiJ9.test.signature";

        _mockCache.Setup(c => c.SetAsync(
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.RevokeTokenAsync(token);

        // Assert — cache.SetStringAsync is an extension that calls SetAsync
        _mockCache.Verify(c => c.SetAsync(
            It.Is<string>(k => k.StartsWith("revoked_token:")),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RevokeTokenAsync_WithCustomExpiration_UsesProvidedMinutes()
    {
        // Arrange
        var token = "test-token-custom-expiry";

        _mockCache.Setup(c => c.SetAsync(
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.RevokeTokenAsync(token, expirationMinutes: 120);

        // Assert
        _mockCache.Verify(c => c.SetAsync(
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.Is<DistributedCacheEntryOptions>(o =>
                o.AbsoluteExpirationRelativeToNow == TimeSpan.FromMinutes(120)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RevokeTokenAsync_NullOrWhitespaceToken_DoesNotCallCache()
    {
        // Act
        await _service.RevokeTokenAsync(null!);
        await _service.RevokeTokenAsync("");
        await _service.RevokeTokenAsync("   ");

        // Assert — cache should never be called
        _mockCache.Verify(c => c.SetAsync(
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RevokeTokenAsync_DefaultExpiration_Uses60Minutes()
    {
        // Arrange
        var token = "test-token-default-expiry";

        _mockCache.Setup(c => c.SetAsync(
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.RevokeTokenAsync(token);

        // Assert
        _mockCache.Verify(c => c.SetAsync(
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.Is<DistributedCacheEntryOptions>(o =>
                o.AbsoluteExpirationRelativeToNow == TimeSpan.FromMinutes(60)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RevokeTokenAsync_CacheThrows_PropagatesException()
    {
        // Arrange
        var token = "test-token-error";

        _mockCache.Setup(c => c.SetAsync(
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Cache unavailable"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RevokeTokenAsync(token));
    }

    #endregion

    #region RevokeAllUserTokensAsync Tests

    [Fact]
    public async Task RevokeAllUserTokensAsync_ValidUser_SetsRevocationTimestamp()
    {
        // Arrange
        var userId = 42;

        _mockCache.Setup(c => c.SetAsync(
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.RevokeAllUserTokensAsync(userId);

        // Assert — key should contain user_revoked_at:{userId}
        _mockCache.Verify(c => c.SetAsync(
            It.Is<string>(k => k == "user_revoked_at:42"),
            It.IsAny<byte[]>(),
            It.Is<DistributedCacheEntryOptions>(o =>
                o.AbsoluteExpirationRelativeToNow == TimeSpan.FromHours(24)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RevokeAllUserTokensAsync_CacheThrows_PropagatesException()
    {
        // Arrange
        _mockCache.Setup(c => c.SetAsync(
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Cache unavailable"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RevokeAllUserTokensAsync(1));
    }

    #endregion

    #region IsTokenRevokedAsync Tests

    [Fact]
    public async Task IsTokenRevokedAsync_TokenInCache_ReturnsTrue()
    {
        // Arrange
        var token = "revoked-jwt-token";

        // GetStringAsync is an extension that calls GetAsync
        _mockCache.Setup(c => c.GetAsync(
            It.Is<string>(k => k.StartsWith("revoked_token:")),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Encoding.UTF8.GetBytes("revoked"));

        // Act
        var result = await _service.IsTokenRevokedAsync(token);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsTokenRevokedAsync_TokenNotInCache_ReturnsFalse()
    {
        // Arrange
        var token = "valid-jwt-token";

        _mockCache.Setup(c => c.GetAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        // Act
        var result = await _service.IsTokenRevokedAsync(token);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsTokenRevokedAsync_NullOrWhitespace_ReturnsTrue()
    {
        // The source code returns true for null/whitespace tokens
        // Act & Assert
        (await _service.IsTokenRevokedAsync(null!)).Should().BeTrue();
        (await _service.IsTokenRevokedAsync("")).Should().BeTrue();
        (await _service.IsTokenRevokedAsync("  ")).Should().BeTrue();
    }

    [Fact]
    public async Task IsTokenRevokedAsync_CacheThrows_ReturnsFalse()
    {
        // Source: on error, assume token is valid to prevent lockouts
        var token = "test-token-cache-error";

        _mockCache.Setup(c => c.GetAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Cache unavailable"));

        // Act
        var result = await _service.IsTokenRevokedAsync(token);

        // Assert — returns false on error (fail-open to prevent lockouts)
        result.Should().BeFalse();
    }

    #endregion

    #region IsUserTokenRevokedAsync Tests

    [Fact]
    public async Task IsUserTokenRevokedAsync_TokenIssuedBeforeRevocation_ReturnsTrue()
    {
        // Arrange
        var userId = 1;
        // Use explicit timestamps to avoid DateTime.Kind / timezone parse issues
        var revocationTime = new DateTime(2026, 1, 1, 11, 0, 0, DateTimeKind.Utc);
        var tokenIssuedAt = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc); // Issued 1 hour BEFORE revocation

        _mockCache.Setup(c => c.GetAsync(
            It.Is<string>(k => k == "user_revoked_at:1"),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Encoding.UTF8.GetBytes(revocationTime.ToString("O")));

        // Act
        var result = await _service.IsUserTokenRevokedAsync(userId, tokenIssuedAt);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsUserTokenRevokedAsync_TokenIssuedAfterRevocation_ReturnsFalse()
    {
        // Arrange
        var userId = 1;
        // Use explicit timestamps to avoid DateTime.Kind / timezone parse issues
        var revocationTime = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var tokenIssuedAt = new DateTime(2026, 1, 1, 11, 0, 0, DateTimeKind.Utc); // Issued 1 hour AFTER revocation

        _mockCache.Setup(c => c.GetAsync(
            It.Is<string>(k => k == "user_revoked_at:1"),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Encoding.UTF8.GetBytes(revocationTime.ToString("O")));

        // Act
        var result = await _service.IsUserTokenRevokedAsync(userId, tokenIssuedAt);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsUserTokenRevokedAsync_NoRevocationRecord_ReturnsFalse()
    {
        // Arrange
        var userId = 99;

        _mockCache.Setup(c => c.GetAsync(
            It.Is<string>(k => k == "user_revoked_at:99"),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        // Act
        var result = await _service.IsUserTokenRevokedAsync(userId, DateTime.UtcNow);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsUserTokenRevokedAsync_CacheThrows_ReturnsFalse()
    {
        // Source: on error, assume token is valid to prevent lockouts
        _mockCache.Setup(c => c.GetAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Cache unavailable"));

        // Act
        var result = await _service.IsUserTokenRevokedAsync(1, DateTime.UtcNow);

        // Assert — fail-open to prevent lockouts
        result.Should().BeFalse();
    }

    #endregion

    #region Token Hashing Consistency Tests

    [Fact]
    public async Task RevokeAndCheck_SameToken_IsConsistent()
    {
        // Arrange — store what SetAsync receives and return it from GetAsync
        byte[]? storedValue = null;
        string? storedKey = null;

        _mockCache.Setup(c => c.SetAsync(
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()))
            .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>(
                (key, value, opts, ct) => { storedKey = key; storedValue = value; })
            .Returns(Task.CompletedTask);

        _mockCache.Setup(c => c.GetAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => storedValue);

        var token = "my-jwt-token-to-revoke";

        // Act — revoke then check
        await _service.RevokeTokenAsync(token);
        var isRevoked = await _service.IsTokenRevokedAsync(token);

        // Assert
        storedKey.Should().NotBeNull();
        storedKey.Should().StartWith("revoked_token:");
        isRevoked.Should().BeTrue();
    }

    #endregion
}
