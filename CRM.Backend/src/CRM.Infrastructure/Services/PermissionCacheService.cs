// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text.Json;
using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for managing permission caching using Redis.
/// Implements IPermissionCacheService to cache user permissions for performance.
///
/// CACHE KEY STRUCTURE:
/// - prefixperm:uid{userId} -> Set<string> of permission names
/// - perm:metadata:{userId} -> PermissionCacheMetadata (JSON)
/// - perm:stats -> PermissionCacheStats (JSON)
///
/// FEATURES:
/// - Configurable TTL (default 1 hour)
/// - Cache statistics tracking
/// - Bulk cache warming
/// - Automatic expiration
/// </summary>
public class PermissionCacheService : IPermissionCacheService
{
    private const string CACHE_KEY_PREFIX = "perm:";
    private const string USER_PERMISSION_KEY_SUFFIX = ":perms";
    private const string METADATA_KEY_SUFFIX = ":meta";
    private const string STATS_KEY = "perm:stats";
    private const int DEFAULT_TTL_SECONDS = 3600; // 1 hour

    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<PermissionCacheService> _logger;
    private int _cacheTtlSeconds;

    public PermissionCacheService(IConnectionMultiplexer redis, ILogger<PermissionCacheService> logger)
    {
        _redis = redis;
        _logger = logger;
        _cacheTtlSeconds = DEFAULT_TTL_SECONDS;
    }

    #region Cache Operations

    public async Task<ISet<string>> GetUserPermissionsFromCacheAsync(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = GetUserPermissionKey(userId);

            // Try to get from cache
            var cachedValue = await db.StringGetAsync(key);

            if (cachedValue.HasValue)
            {
                var permissions = JsonSerializer.Deserialize<HashSet<string>>(cachedValue.ToString()) ?? new HashSet<string>();

                // Increment hit count in metadata
                await IncrementCacheHitAsync(db, userId);

                _logger.LogDebug($"Cache hit for user {userId}: {permissions.Count} permissions");
                return permissions;
            }

            // Increment miss count
            await IncrementCacheMissAsync(db);
            _logger.LogDebug($"Cache miss for user {userId}");
            return new HashSet<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting permissions from cache for user {userId}");
            return new HashSet<string>();
        }
    }

    public async Task SetUserPermissionsInCacheAsync(int userId, ISet<string> permissions, int? ttlSeconds = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = GetUserPermissionKey(userId);
            var ttl = ttlSeconds ?? _cacheTtlSeconds;

            // Serialize and cache permissions
            var serialized = JsonSerializer.Serialize(permissions);
            var expiry = TimeSpan.FromSeconds(ttl);

            await db.StringSetAsync(key, serialized, expiry);

            // Store metadata
            var metadata = new PermissionCacheMetadata
            {
                UserId = userId,
                CachedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddSeconds(ttl),
                PermissionCount = permissions.Count
            };

            var metadataKey = GetMetadataKey(userId);
            var metadataJson = JsonSerializer.Serialize(metadata);
            await db.StringSetAsync(metadataKey, metadataJson, expiry);

            _logger.LogDebug($"Cached {permissions.Count} permissions for user {userId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error setting permissions in cache for user {userId}");
        }
    }

    public async Task<bool> IsUserPermissionsCachedAsync(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = GetUserPermissionKey(userId);
            return await db.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error checking if permissions cached for user {userId}");
            return false;
        }
    }

    public async Task InvalidateUserCacheAsync(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = GetUserPermissionKey(userId);
            var metadataKey = GetMetadataKey(userId);

            await Task.WhenAll(
                db.KeyDeleteAsync(key),
                db.KeyDeleteAsync(metadataKey));

            _logger.LogInformation($"Invalidated cache for user {userId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error invalidating cache for user {userId}");
        }
    }

    public async Task InvalidateMultipleUsersAsync(IEnumerable<int> userIds, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var tasks = userIds.Select(userId => InvalidateUserCacheAsync(userId, cancellationToken)).ToList();
            await Task.WhenAll(tasks);

            _logger.LogInformation($"Invalidated cache for {tasks.Count} users");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating multiple user caches");
        }
    }

    public async Task InvalidateAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var server = _redis.GetServer(_redis.GetEndPoints().First());

            // Find and delete all permission cache keys
            var keys = server.Keys(pattern: $"{CACHE_KEY_PREFIX}*");
            var keysArray = keys.ToArray();

            if (keysArray.Length > 0)
            {
                await db.KeyDeleteAsync(keysArray);
            }

            _logger.LogInformation($"Invalidated all permission caches ({keysArray.Length} keys deleted)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating all caches");
        }
    }

    #endregion

    #region Cache Warming

    public async Task<ISet<string>> WarmUserCacheAsync(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            // This would typically be called from IRBACService after loading permissions from DB
            // For now, just return empty set - actual warming happens in RBACService.GetUserPermissionsAsync
            _logger.LogDebug($"Warming cache for user {userId}");
            return new HashSet<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error warming cache for user {userId}");
            return new HashSet<string>();
        }
    }

    public async Task WarmMultipleUsersAsync(IEnumerable<int> userIds, CancellationToken cancellationToken = default)
    {
        try
        {
            var tasks = userIds.Select(userId => WarmUserCacheAsync(userId, cancellationToken)).ToList();
            await Task.WhenAll(tasks);
            _logger.LogInformation($"Warmed cache for {tasks.Count} users");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error warming multiple user caches");
        }
    }

    #endregion

    #region Configuration & Statistics

    public int GetCacheTtlSeconds() => _cacheTtlSeconds;

    public void SetCacheTtlSeconds(int ttlSeconds)
    {
        _cacheTtlSeconds = ttlSeconds;
        _logger.LogInformation($"Permission cache TTL set to {ttlSeconds} seconds");
    }

    public async Task<PermissionCacheStatisticsDto> GetCacheStatisticsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var statsJson = await db.StringGetAsync(STATS_KEY);

            if (statsJson.HasValue)
            {
                return JsonSerializer.Deserialize<PermissionCacheStatisticsDto>(statsJson.ToString())
                    ?? new PermissionCacheStatisticsDto();
            }

            return new PermissionCacheStatisticsDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache statistics");
            return new PermissionCacheStatisticsDto();
        }
    }

    public async Task ResetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync(STATS_KEY);
            _logger.LogInformation("Reset cache statistics");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting statistics");
        }
    }

    #endregion

    #region Private Helper Methods

    private string GetUserPermissionKey(int userId) => $"{CACHE_KEY_PREFIX}uid{userId}{USER_PERMISSION_KEY_SUFFIX}";
    private string GetMetadataKey(int userId) => $"{CACHE_KEY_PREFIX}uid{userId}{METADATA_KEY_SUFFIX}";

    private async Task IncrementCacheHitAsync(IDatabase db, int userId)
    {
        try
        {
            var statsJson = await db.StringGetAsync(STATS_KEY);
            var stats = statsJson.HasValue
                ? JsonSerializer.Deserialize<PermissionCacheStatisticsDto>(statsJson.ToString())
                : new PermissionCacheStatisticsDto();

            if (stats != null)
            {
                stats.TotalHits++;
                var updated = JsonSerializer.Serialize(stats);
                await db.StringSetAsync(STATS_KEY, updated);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error incrementing cache hit");
        }
    }

    private async Task IncrementCacheMissAsync(IDatabase db)
    {
        try
        {
            var statsJson = await db.StringGetAsync(STATS_KEY);
            var stats = statsJson.HasValue
                ? JsonSerializer.Deserialize<PermissionCacheStatisticsDto>(statsJson.ToString())
                : new PermissionCacheStatisticsDto();

            if (stats != null)
            {
                stats.TotalMisses++;
                var updated = JsonSerializer.Serialize(stats);
                await db.StringSetAsync(STATS_KEY, updated);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error incrementing cache miss");
        }
    }

    #endregion
}

/// <summary>
/// Internal metadata for cached permissions.
/// </summary>
internal class PermissionCacheMetadata
{
    public int UserId { get; set; }
    public DateTime CachedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int PermissionCount { get; set; }
}
