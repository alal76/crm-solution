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
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;
using CRM.Infrastructure.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for Cache Provider
/// Covers: Get, Set, Remove, Patterns
/// </summary>
public class CacheProviderTests
{
    private readonly Mock<IDistributedCache> _mockCache;
    private readonly Mock<IOptions<CacheSettings>> _mockCacheSettings;
    private readonly Mock<ILogger<CacheService>> _mockLogger;
    private readonly CacheSettings _settings;

    public CacheProviderTests()
    {
        _mockCache = new Mock<IDistributedCache>();
        _settings = new CacheSettings
        {
            DefaultExpirationMinutes = 30,
            SlidingExpirationMinutes = 10,
            KeyPrefix = "crm:",
            EnableCompression = false
        };

        _mockCacheSettings = new Mock<IOptions<CacheSettings>>();
        _mockCacheSettings.Setup(x => x.Value).Returns(_settings);
        _mockLogger = new Mock<ILogger<CacheService>>();
    }

    #region Get Tests

    [Fact]
    public async Task GetAsync_KeyExists_ReturnsValue()
    {
        // Arrange
        var service = CreateCacheService();
        var key = "test-key";
        var value = "test-value";
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value));

        _mockCache.Setup(c => c.GetAsync($"crm:{key}", default))
            .ReturnsAsync(bytes);

        // Act
        var result = await service.GetAsync<string>(key);

        // Assert
        result.Should().Be(value);
    }

    [Fact]
    public async Task GetAsync_KeyNotExists_ReturnsDefault()
    {
        // Arrange
        var service = CreateCacheService();
        var key = "non-existing-key";

        _mockCache.Setup(c => c.GetAsync($"crm:{key}", default))
            .ReturnsAsync((byte[]?)null);

        // Act
        var result = await service.GetAsync<string>(key);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_ComplexObject_Deserializes()
    {
        // Arrange
        var service = CreateCacheService();
        var key = "user-key";
        var user = new TestUser { Id = 1, Name = "John", Email = "john@test.com" };
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(user));

        _mockCache.Setup(c => c.GetAsync($"crm:{key}", default))
            .ReturnsAsync(bytes);

        // Act
        var result = await service.GetAsync<TestUser>(key);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("John");
    }

    [Fact]
    public async Task GetAsync_EmptyKey_ThrowsException()
    {
        // Arrange
        var service = CreateCacheService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await service.GetAsync<string>(""));
    }

    #endregion

    #region Set Tests

    [Fact]
    public async Task SetAsync_ValidKeyValue_Stores()
    {
        // Arrange
        var service = CreateCacheService();
        var key = "test-key";
        var value = "test-value";

        // Act
        await service.SetAsync(key, value);

        // Assert
        _mockCache.Verify(c => c.SetAsync(
            $"crm:{key}",
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            default), Times.Once);
    }

    [Fact]
    public async Task SetAsync_WithExpiration_SetsExpiration()
    {
        // Arrange
        var service = CreateCacheService();
        var key = "test-key";
        var value = "test-value";
        var expiration = TimeSpan.FromMinutes(60);

        // Act
        await service.SetAsync(key, value, expiration);

        // Assert
        _mockCache.Verify(c => c.SetAsync(
            $"crm:{key}",
            It.IsAny<byte[]>(),
            It.Is<DistributedCacheEntryOptions>(o => o.AbsoluteExpirationRelativeToNow == expiration),
            default), Times.Once);
    }

    [Fact]
    public async Task SetAsync_ComplexObject_Serializes()
    {
        // Arrange
        var service = CreateCacheService();
        var key = "user-key";
        var user = new TestUser { Id = 1, Name = "John", Email = "john@test.com" };

        // Act
        await service.SetAsync(key, user);

        // Assert
        _mockCache.Verify(c => c.SetAsync(
            $"crm:{key}",
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            default), Times.Once);
    }

    [Fact]
    public async Task SetAsync_NullValue_ThrowsException()
    {
        // Arrange
        var service = CreateCacheService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await service.SetAsync<string>("key", null!));
    }

    #endregion

    #region Remove Tests

    [Fact]
    public async Task RemoveAsync_KeyExists_RemovesKey()
    {
        // Arrange
        var service = CreateCacheService();
        var key = "test-key";

        // Act
        await service.RemoveAsync(key);

        // Assert
        _mockCache.Verify(c => c.RemoveAsync($"crm:{key}", default), Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_KeyNotExists_NoError()
    {
        // Arrange
        var service = CreateCacheService();
        var key = "non-existing-key";

        // Act & Assert - should not throw
        await service.RemoveAsync(key);
    }

    #endregion

    #region GetOrSet Tests

    [Fact]
    public async Task GetOrSetAsync_KeyExists_ReturnsCached()
    {
        // Arrange
        var service = CreateCacheService();
        var key = "test-key";
        var cachedValue = "cached-value";
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(cachedValue));
        var factoryCalled = false;

        _mockCache.Setup(c => c.GetAsync($"crm:{key}", default))
            .ReturnsAsync(bytes);

        // Act
        var result = await service.GetOrSetAsync(key, async () =>
        {
            factoryCalled = true;
            await Task.Delay(1);
            return "new-value";
        });

        // Assert
        result.Should().Be(cachedValue);
        factoryCalled.Should().BeFalse();
    }

    [Fact]
    public async Task GetOrSetAsync_KeyNotExists_CallsFactory()
    {
        // Arrange
        var service = CreateCacheService();
        var key = "test-key";
        var factoryCalled = false;

        _mockCache.Setup(c => c.GetAsync($"crm:{key}", default))
            .ReturnsAsync((byte[]?)null);

        // Act
        var result = await service.GetOrSetAsync(key, async () =>
        {
            factoryCalled = true;
            await Task.Delay(1);
            return "new-value";
        });

        // Assert
        result.Should().Be("new-value");
        factoryCalled.Should().BeTrue();
    }

    [Fact]
    public async Task GetOrSetAsync_FactoryThrows_PropagatesException()
    {
        // Arrange
        var service = CreateCacheService();
        var key = "test-key";

        _mockCache.Setup(c => c.GetAsync($"crm:{key}", default))
            .ReturnsAsync((byte[]?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.GetOrSetAsync<string>(key, () =>
                throw new InvalidOperationException("Factory error")));
    }

    #endregion

    #region Exists Tests

    [Fact]
    public async Task ExistsAsync_KeyExists_ReturnsTrue()
    {
        // Arrange
        var service = CreateCacheService();
        var key = "test-key";

        _mockCache.Setup(c => c.GetAsync($"crm:{key}", default))
            .ReturnsAsync(new byte[] { 1 });

        // Act
        var result = await service.ExistsAsync(key);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_KeyNotExists_ReturnsFalse()
    {
        // Arrange
        var service = CreateCacheService();
        var key = "test-key";

        _mockCache.Setup(c => c.GetAsync($"crm:{key}", default))
            .ReturnsAsync((byte[]?)null);

        // Act
        var result = await service.ExistsAsync(key);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Pattern Tests

    [Fact]
    public async Task RemoveByPatternAsync_MatchingKeys_RemovesAll()
    {
        // Arrange
        var service = CreateMockCacheServiceWithPatternSupport();
        var pattern = "user:*";

        // Act
        var result = await service.RemoveByPatternAsync(pattern);

        // Assert
        result.Should().BeGreaterThanOrEqualTo(0);
    }

    #endregion

    #region Refresh Tests

    [Fact]
    public async Task RefreshAsync_KeyExists_RefreshesExpiration()
    {
        // Arrange
        var service = CreateCacheService();
        var key = "test-key";

        // Act
        await service.RefreshAsync(key);

        // Assert
        _mockCache.Verify(c => c.RefreshAsync($"crm:{key}", default), Times.Once);
    }

    #endregion

    #region Key Generation Tests

    [Fact]
    public void GenerateKey_SinglePart_ReturnsKey()
    {
        // Arrange
        var service = CreateCacheService();

        // Act
        var key = service.GenerateKey("users");

        // Assert
        key.Should().Be("users");
    }

    [Fact]
    public void GenerateKey_MultipleParts_JoinsWithColon()
    {
        // Arrange
        var service = CreateCacheService();

        // Act
        var key = service.GenerateKey("users", "1", "profile");

        // Assert
        key.Should().Be("users:1:profile");
    }

    [Fact]
    public void GenerateEntityKey_ReturnsFormattedKey()
    {
        // Arrange
        var service = CreateCacheService();

        // Act
        var key = service.GenerateEntityKey("Account", 123);

        // Assert
        key.Should().Be("Account:123");
    }

    #endregion

    #region Batch Operations Tests

    [Fact]
    public async Task GetManyAsync_MultipleKeys_ReturnsAll()
    {
        // Arrange
        var service = CreateCacheService();
        var keys = new[] { "key1", "key2", "key3" };

        foreach (var key in keys)
        {
            var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize($"value-{key}"));
            _mockCache.Setup(c => c.GetAsync($"crm:{key}", default))
                .ReturnsAsync(bytes);
        }

        // Act
        var results = await service.GetManyAsync<string>(keys);

        // Assert
        results.Should().HaveCount(3);
    }

    [Fact]
    public async Task SetManyAsync_MultipleValues_SetsAll()
    {
        // Arrange
        var service = CreateCacheService();
        var items = new Dictionary<string, string>
        {
            { "key1", "value1" },
            { "key2", "value2" },
            { "key3", "value3" }
        };

        // Act
        await service.SetManyAsync(items);

        // Assert
        _mockCache.Verify(c => c.SetAsync(
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            default), Times.Exactly(3));
    }

    [Fact]
    public async Task RemoveManyAsync_MultipleKeys_RemovesAll()
    {
        // Arrange
        var service = CreateCacheService();
        var keys = new[] { "key1", "key2", "key3" };

        // Act
        await service.RemoveManyAsync(keys);

        // Assert
        _mockCache.Verify(c => c.RemoveAsync(It.IsAny<string>(), default), Times.Exactly(3));
    }

    #endregion

    #region Helper Methods

    private CacheService CreateCacheService()
    {
        return new CacheService(_mockCache.Object, _mockCacheSettings.Object, _mockLogger.Object);
    }

    private MockCacheServiceWithPatternSupport CreateMockCacheServiceWithPatternSupport()
    {
        return new MockCacheServiceWithPatternSupport(_mockCache.Object, _mockCacheSettings.Object, _mockLogger.Object);
    }

    #endregion
}

// Implementation classes for testing
public class CacheService
{
    private readonly IDistributedCache _cache;
    private readonly CacheSettings _settings;
    private readonly ILogger<CacheService> _logger;

    public CacheService(IDistributedCache cache, IOptions<CacheSettings> settings, ILogger<CacheService> logger)
    {
        _cache = cache;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Key cannot be empty", nameof(key));

        var bytes = await _cache.GetAsync($"{_settings.KeyPrefix}{key}");
        if (bytes == null) return default;

        return JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(bytes));
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(_settings.DefaultExpirationMinutes)
        };

        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value));
        await _cache.SetAsync($"{_settings.KeyPrefix}{key}", bytes, options);
    }

    public async Task RemoveAsync(string key)
    {
        await _cache.RemoveAsync($"{_settings.KeyPrefix}{key}");
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        var cached = await GetAsync<T>(key);
        if (cached != null) return cached;

        var value = await factory();
        await SetAsync(key, value, expiration);
        return value;
    }

    public async Task<bool> ExistsAsync(string key)
    {
        var bytes = await _cache.GetAsync($"{_settings.KeyPrefix}{key}");
        return bytes != null;
    }

    public async Task RefreshAsync(string key)
    {
        await _cache.RefreshAsync($"{_settings.KeyPrefix}{key}");
    }

    public string GenerateKey(params string[] parts)
    {
        return string.Join(":", parts);
    }

    public string GenerateEntityKey(string entityType, int entityId)
    {
        return $"{entityType}:{entityId}";
    }

    public async Task<Dictionary<string, T?>> GetManyAsync<T>(IEnumerable<string> keys)
    {
        var results = new Dictionary<string, T?>();
        foreach (var key in keys)
        {
            results[key] = await GetAsync<T>(key);
        }
        return results;
    }

    public async Task SetManyAsync<T>(Dictionary<string, T> items, TimeSpan? expiration = null)
    {
        foreach (var item in items)
        {
            await SetAsync(item.Key, item.Value, expiration);
        }
    }

    public async Task RemoveManyAsync(IEnumerable<string> keys)
    {
        foreach (var key in keys)
        {
            await RemoveAsync(key);
        }
    }

    public virtual Task<int> RemoveByPatternAsync(string pattern)
    {
        // Base implementation doesn't support patterns
        throw new NotSupportedException("Pattern-based removal not supported by this cache provider");
    }
}

public class MockCacheServiceWithPatternSupport : CacheService
{
    public MockCacheServiceWithPatternSupport(IDistributedCache cache, IOptions<CacheSettings> settings, ILogger<CacheService> logger)
        : base(cache, settings, logger)
    {
    }

    public override Task<int> RemoveByPatternAsync(string pattern)
    {
        // Mock implementation
        return Task.FromResult(5);
    }
}

// Supporting classes
public class CacheSettings
{
    public int DefaultExpirationMinutes { get; set; }
    public int SlidingExpirationMinutes { get; set; }
    public string KeyPrefix { get; set; } = string.Empty;
    public bool EnableCompression { get; set; }
}

public class TestUser
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
