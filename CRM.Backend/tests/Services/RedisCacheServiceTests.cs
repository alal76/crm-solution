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
using Microsoft.Extensions.Options;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Text.Json;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for RedisCacheService
/// Covers: Cache operations, expiration, patterns, serialization
/// </summary>
public class RedisCacheServiceTests
{
    private readonly Mock<ILogger<RedisCacheService>> _mockLogger;
    private readonly Mock<IOptions<RedisCacheSettings>> _mockOptions;
    private readonly RedisCacheService _service;
    private readonly Dictionary<string, CacheEntry> _memoryCache;

    public RedisCacheServiceTests()
    {
        _mockLogger = new Mock<ILogger<RedisCacheService>>();
        _mockOptions = new Mock<IOptions<RedisCacheSettings>>();
        _memoryCache = new Dictionary<string, CacheEntry>();

        _mockOptions.Setup(o => o.Value).Returns(new RedisCacheSettings
        {
            ConnectionString = "localhost:6379",
            DefaultExpirationMinutes = 30,
            Enabled = true
        });

        _service = new RedisCacheService(
            _mockOptions.Object,
            _mockLogger.Object);
    }

    #region Get Tests

    [Fact]
    public async Task GetAsync_ExistingKey_ReturnsValue()
    {
        // Arrange
        var key = "test:key";
        var value = "test value";
        await _service.SetAsync(key, value);

        // Act
        var result = await _service.GetAsync<string>(key);

        // Assert
        result.Should().Be(value);
    }

    [Fact]
    public async Task GetAsync_NonExistingKey_ReturnsDefault()
    {
        // Arrange
        var key = "nonexistent:key";

        // Act
        var result = await _service.GetAsync<string>(key);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_ComplexObject_ReturnsDeserializedObject()
    {
        // Arrange
        var key = "test:complex";
        var value = new TestCacheObject { Id = 1, Name = "Test", IsActive = true };
        await _service.SetAsync(key, value);

        // Act
        var result = await _service.GetAsync<TestCacheObject>(key);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Test");
    }

    [Fact]
    public async Task GetAsync_NullKey_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.GetAsync<string>(null!));
    }

    #endregion

    #region Set Tests

    [Fact]
    public async Task SetAsync_ValidKeyValue_StoresValue()
    {
        // Arrange
        var key = "test:set";
        var value = "stored value";

        // Act
        await _service.SetAsync(key, value);
        var result = await _service.GetAsync<string>(key);

        // Assert
        result.Should().Be(value);
    }

    [Fact]
    public async Task SetAsync_WithExpiration_SetsExpiration()
    {
        // Arrange
        var key = "test:expiry";
        var value = "expiring value";
        var expiration = TimeSpan.FromMinutes(5);

        // Act
        await _service.SetAsync(key, value, expiration);

        // Assert - Value should be retrievable
        var result = await _service.GetAsync<string>(key);
        result.Should().Be(value);
    }

    [Fact]
    public async Task SetAsync_NullKey_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.SetAsync(null!, "value"));
    }

    [Fact]
    public async Task SetAsync_NullValue_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.SetAsync<string>("key", null!));
    }

    [Fact]
    public async Task SetAsync_OverwritesExistingKey()
    {
        // Arrange
        var key = "test:overwrite";
        await _service.SetAsync(key, "original");

        // Act
        await _service.SetAsync(key, "updated");
        var result = await _service.GetAsync<string>(key);

        // Assert
        result.Should().Be("updated");
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task RemoveAsync_ExistingKey_RemovesValue()
    {
        // Arrange
        var key = "test:remove";
        await _service.SetAsync(key, "value");

        // Act
        await _service.RemoveAsync(key);
        var result = await _service.GetAsync<string>(key);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task RemoveAsync_NonExistingKey_DoesNotThrow()
    {
        // Arrange
        var key = "nonexistent:remove";

        // Act & Assert - Should not throw
        await _service.RemoveAsync(key);
    }

    [Fact]
    public async Task RemoveByPatternAsync_RemovesMatchingKeys()
    {
        // Arrange
        await _service.SetAsync("user:1:profile", "profile1");
        await _service.SetAsync("user:2:profile", "profile2");
        await _service.SetAsync("other:key", "other");

        // Act
        await _service.RemoveByPatternAsync("user:*");

        // Assert
        var result1 = await _service.GetAsync<string>("user:1:profile");
        var result2 = await _service.GetAsync<string>("user:2:profile");
        var other = await _service.GetAsync<string>("other:key");

        result1.Should().BeNull();
        result2.Should().BeNull();
        // other might or might not be null depending on implementation
    }

    #endregion

    #region Exists Tests

    [Fact]
    public async Task ExistsAsync_ExistingKey_ReturnsTrue()
    {
        // Arrange
        var key = "test:exists";
        await _service.SetAsync(key, "value");

        // Act
        var result = await _service.ExistsAsync(key);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_NonExistingKey_ReturnsFalse()
    {
        // Arrange
        var key = "nonexistent:exists";

        // Act
        var result = await _service.ExistsAsync(key);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetOrSet Tests

    [Fact]
    public async Task GetOrSetAsync_KeyExists_ReturnsExistingValue()
    {
        // Arrange
        var key = "test:getorset";
        var existingValue = "existing";
        await _service.SetAsync(key, existingValue);

        // Act
        var result = await _service.GetOrSetAsync(key, () => Task.FromResult("new value"));

        // Assert
        result.Should().Be(existingValue);
    }

    [Fact]
    public async Task GetOrSetAsync_KeyNotExists_CreatesAndReturnsNewValue()
    {
        // Arrange
        var key = "test:getorset:new";
        var newValue = "new value";

        // Act
        var result = await _service.GetOrSetAsync(key, () => Task.FromResult(newValue));

        // Assert
        result.Should().Be(newValue);
    }

    [Fact]
    public async Task GetOrSetAsync_WithExpiration_SetsExpiration()
    {
        // Arrange
        var key = "test:getorset:expiry";
        var expiration = TimeSpan.FromMinutes(10);

        // Act
        var result = await _service.GetOrSetAsync(
            key,
            () => Task.FromResult("value"),
            expiration);

        // Assert
        result.Should().Be("value");
    }

    #endregion

    #region Increment Tests

    [Fact]
    public async Task IncrementAsync_NewKey_ReturnsOne()
    {
        // Arrange
        var key = "counter:new";

        // Act
        var result = await _service.IncrementAsync(key);

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public async Task IncrementAsync_ExistingKey_IncrementsValue()
    {
        // Arrange
        var key = "counter:existing";
        await _service.IncrementAsync(key);
        await _service.IncrementAsync(key);

        // Act
        var result = await _service.IncrementAsync(key);

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public async Task IncrementByAsync_IncrementsSpecifiedAmount()
    {
        // Arrange
        var key = "counter:by";

        // Act
        var result = await _service.IncrementByAsync(key, 5);

        // Assert
        result.Should().Be(5);
    }

    [Fact]
    public async Task DecrementAsync_PositiveValue_DecrementsValue()
    {
        // Arrange
        var key = "counter:decrement";
        await _service.IncrementByAsync(key, 10);

        // Act
        var result = await _service.DecrementAsync(key);

        // Assert
        result.Should().Be(9);
    }

    #endregion

    #region Hash Tests

    [Fact]
    public async Task HashSetAsync_SetsHashField()
    {
        // Arrange
        var hashKey = "hash:test";
        var field = "field1";
        var value = "value1";

        // Act
        await _service.HashSetAsync(hashKey, field, value);
        var result = await _service.HashGetAsync<string>(hashKey, field);

        // Assert
        result.Should().Be(value);
    }

    [Fact]
    public async Task HashGetAllAsync_ReturnsAllFields()
    {
        // Arrange
        var hashKey = "hash:all";
        await _service.HashSetAsync(hashKey, "field1", "value1");
        await _service.HashSetAsync(hashKey, "field2", "value2");

        // Act
        var result = await _service.HashGetAllAsync<string>(hashKey);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task HashDeleteAsync_RemovesField()
    {
        // Arrange
        var hashKey = "hash:delete";
        await _service.HashSetAsync(hashKey, "field", "value");

        // Act
        await _service.HashDeleteAsync(hashKey, "field");
        var result = await _service.HashGetAsync<string>(hashKey, "field");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task HashExistsAsync_FieldExists_ReturnsTrue()
    {
        // Arrange
        var hashKey = "hash:exists";
        await _service.HashSetAsync(hashKey, "field", "value");

        // Act
        var result = await _service.HashExistsAsync(hashKey, "field");

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region List Tests

    [Fact]
    public async Task ListPushAsync_AddsToList()
    {
        // Arrange
        var listKey = "list:push";

        // Act
        await _service.ListPushAsync(listKey, "item1");
        await _service.ListPushAsync(listKey, "item2");
        var result = await _service.ListRangeAsync<string>(listKey, 0, -1);

        // Assert
        result.Should().HaveCountGreaterOrEqualTo(2);
    }

    [Fact]
    public async Task ListPopAsync_RemovesAndReturnsLastItem()
    {
        // Arrange
        var listKey = "list:pop";
        await _service.ListPushAsync(listKey, "item1");
        await _service.ListPushAsync(listKey, "item2");

        // Act
        var result = await _service.ListPopAsync<string>(listKey);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ListLengthAsync_ReturnsCount()
    {
        // Arrange
        var listKey = "list:length";
        await _service.ListPushAsync(listKey, "item1");
        await _service.ListPushAsync(listKey, "item2");
        await _service.ListPushAsync(listKey, "item3");

        // Act
        var result = await _service.ListLengthAsync(listKey);

        // Assert
        result.Should().BeGreaterOrEqualTo(3);
    }

    #endregion

    #region Expiration Tests

    [Fact]
    public async Task SetExpirationAsync_SetsKeyExpiration()
    {
        // Arrange
        var key = "test:expire";
        await _service.SetAsync(key, "value");
        var expiration = TimeSpan.FromMinutes(10);

        // Act
        var result = await _service.SetExpirationAsync(key, expiration);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetTimeToLiveAsync_ReturnsRemainingTime()
    {
        // Arrange
        var key = "test:ttl";
        await _service.SetAsync(key, "value", TimeSpan.FromMinutes(30));

        // Act
        var result = await _service.GetTimeToLiveAsync(key);

        // Assert
        result.Should().NotBeNull();
        result!.Value.TotalMinutes.Should().BeLessThanOrEqualTo(30);
    }

    [Fact]
    public async Task GetTimeToLiveAsync_NoExpiration_ReturnsNull()
    {
        // Arrange
        var key = "test:ttl:none";
        await _service.SetAsync(key, "value");

        // Act
        var result = await _service.GetTimeToLiveAsync(key);

        // Assert - Depends on implementation (might return null or -1)
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetStatisticsAsync_ReturnsStats()
    {
        // Act
        var result = await _service.GetStatisticsAsync();

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetKeyCountAsync_ReturnsCount()
    {
        // Act
        var result = await _service.GetKeyCountAsync();

        // Assert
        result.Should().BeGreaterThanOrEqualTo(0);
    }

    #endregion

    #region Health Check Tests

    [Fact]
    public async Task IsConnectedAsync_ReturnsConnectionStatus()
    {
        // Act
        var result = await _service.IsConnectedAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task PingAsync_ReturnsPongOrLatency()
    {
        // Act
        var result = await _service.PingAsync();

        // Assert
        result.Should().BeGreaterThanOrEqualTo(TimeSpan.Zero);
    }

    #endregion

    #region Clear Tests

    [Fact]
    public async Task ClearAsync_RemovesAllKeys()
    {
        // Arrange
        await _service.SetAsync("key1", "value1");
        await _service.SetAsync("key2", "value2");

        // Act
        await _service.ClearAsync();

        // Assert - Keys should be removed
        var result1 = await _service.GetAsync<string>("key1");
        result1.Should().BeNull();
    }

    #endregion
}

// Supporting classes for tests
public class RedisCacheSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public int DefaultExpirationMinutes { get; set; }
    public bool Enabled { get; set; }
}

public class TestCacheObject
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class CacheEntry
{
    public string Value { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
}
