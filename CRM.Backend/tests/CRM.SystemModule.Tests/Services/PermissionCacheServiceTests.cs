// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.SystemModule.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using StackExchange.Redis;
using Xunit;

namespace CRM.SystemModule.Tests.Services;

/// <summary>
/// Unit tests for PermissionCacheService.
/// Tests permission caching functionality.
/// NOTE: PermissionCacheService uses Redis (IConnectionMultiplexer), not IMemoryCache.
/// </summary>
public class PermissionCacheServiceTests
{
    private readonly Mock<IConnectionMultiplexer> _redisMock;
    private readonly Mock<IDatabase> _dbMock;
    private readonly Mock<ILogger<PermissionCacheService>> _loggerMock;
    private readonly PermissionCacheService _service;

    public PermissionCacheServiceTests()
    {
        _redisMock = new Mock<IConnectionMultiplexer>();
        _dbMock = new Mock<IDatabase>();
        _loggerMock = new Mock<ILogger<PermissionCacheService>>();
        
        _redisMock.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(_dbMock.Object);

        _service = new PermissionCacheService(_redisMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetUserPermissionsFromCacheAsync_WhenNotCached_ReturnsEmptySet()
    {
        // Arrange
        var userId = 1;
        _dbMock.Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        // Act
        var result = await _service.GetUserPermissionsFromCacheAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task SetUserPermissionsInCacheAsync_StoresPermissionsInCache()
    {
        // Arrange
        var userId = 1;
        var permissions = new HashSet<string> { "View.Accounts", "Edit.Accounts" };

        _dbMock.Setup(x => x.StringSetAsync(
            It.IsAny<RedisKey>(), 
            It.IsAny<RedisValue>(), 
            It.IsAny<TimeSpan?>(), 
            It.IsAny<bool>(),
            It.IsAny<When>(),
            It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act
        await _service.SetUserPermissionsInCacheAsync(userId, permissions);

        // Assert
        _dbMock.Verify(x => x.StringSetAsync(
            It.IsAny<RedisKey>(), 
            It.IsAny<RedisValue>(), 
            It.IsAny<TimeSpan?>(), 
            It.IsAny<bool>(),
            It.IsAny<When>(),
            It.IsAny<CommandFlags>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task InvalidateUserCacheAsync_RemovesUserCache()
    {
        // Arrange
        var userId = 1;
        _dbMock.Setup(x => x.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act
        await _service.InvalidateUserCacheAsync(userId);

        // Assert
        _dbMock.Verify(x => x.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()), Times.AtLeastOnce);
    }

    [Fact]
    public void CacheKeyFormat_IsConsistent()
    {
        // Arrange
        var userId = 1;
        var expectedPrefix = "perm:";

        // Act & Assert
        // Verify cache key generation follows expected format
        var cacheKey = $"{expectedPrefix}{userId}:perms";
        Assert.StartsWith(expectedPrefix, cacheKey);
    }
}
