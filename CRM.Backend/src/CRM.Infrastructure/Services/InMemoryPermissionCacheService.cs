// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Collections.Concurrent;
using CRM.Core.Dtos;
using CRM.Core.Interfaces;

namespace CRM.Infrastructure.Services;

/// <summary>
/// In-memory implementation of IPermissionCacheService.
/// Used as a fallback when Redis is disabled.
///
/// LIMITATIONS:
/// - Does not persist across application restarts
/// - Memory usage increases with number of cached users
/// - Not suitable for distributed deployments
/// - No TTL enforcement (uses simple expiration strategy)
///
/// WHEN TO USE:
/// - Development environments
/// - Single-machine deployments
/// - When Redis is not available or disabled
/// </summary>
public class InMemoryPermissionCacheService : IPermissionCacheService
{
    // Simple in-memory cache: userId -> (permissions, timestamp)
    private static readonly ConcurrentDictionary<int, CacheEntry> _cache = new();
    private const int DEFAULT_TTL_SECONDS = 3600; // 1 hour

    private class CacheEntry
    {
        public ISet<string> Permissions { get; set; } = new HashSet<string>();
        public DateTime ExpirationTime { get; set; } = DateTime.UtcNow.AddSeconds(DEFAULT_TTL_SECONDS);
    }

    #region Cache Operations

    public Task<ISet<string>> GetUserPermissionsFromCacheAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(userId, out var entry))
        {
            // Check if expired
            if (DateTime.UtcNow < entry.ExpirationTime)
            {
                return Task.FromResult<ISet<string>>(new HashSet<string>(entry.Permissions));
            }
            else
            {
                // Remove expired entry
                _cache.TryRemove(userId, out _);
            }
        }

        return Task.FromResult<ISet<string>>(new HashSet<string>());
    }

    public Task SetUserPermissionsInCacheAsync(int userId, ISet<string> permissions, int? ttlSeconds = null, CancellationToken cancellationToken = default)
    {
        var expirationTime = DateTime.UtcNow.AddSeconds(ttlSeconds ?? DEFAULT_TTL_SECONDS);
        var entry = new CacheEntry
        {
            Permissions = new HashSet<string>(permissions),
            ExpirationTime = expirationTime
        };

        _cache.AddOrUpdate(userId, entry, (_, _) => entry);
        return Task.CompletedTask;
    }

    public Task<bool> IsUserPermissionsCachedAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(userId, out var entry))
        {
            if (DateTime.UtcNow < entry.ExpirationTime)
            {
                return Task.FromResult(true);
            }
            else
            {
                _cache.TryRemove(userId, out _);
            }
        }

        return Task.FromResult(false);
    }

    public Task InvalidateUserCacheAsync(int userId, CancellationToken cancellationToken = default)
    {
        _cache.TryRemove(userId, out _);
        return Task.CompletedTask;
    }

    public Task InvalidateMultipleUsersAsync(IEnumerable<int> userIds, CancellationToken cancellationToken = default)
    {
        foreach (var userId in userIds)
        {
            _cache.TryRemove(userId, out _);
        }

        return Task.CompletedTask;
    }

    public Task InvalidateAllAsync(CancellationToken cancellationToken = default)
    {
        _cache.Clear();
        return Task.CompletedTask;
    }

    #endregion

    #region Cache Warming

    public Task<ISet<string>> WarmUserCacheAsync(int userId, CancellationToken cancellationToken = default)
    {
        // In-memory implementation doesn't have access to database
        // Return empty set as warhead would need IRBACService context
        return Task.FromResult<ISet<string>>(new HashSet<string>());
    }

    public Task WarmMultipleUsersAsync(IEnumerable<int> userIds, CancellationToken cancellationToken = default)
    {
        // In-memory implementation doesn't have access to database
        return Task.CompletedTask;
    }

    #endregion

    #region Configuration & Statistics

    public int GetCacheTtlSeconds()
    {
        return DEFAULT_TTL_SECONDS;
    }

    public void SetCacheTtlSeconds(int ttlSeconds)
    {
        // In-memory implementation uses fixed TTL, cannot change
    }

    public Task<PermissionCacheStatisticsDto> GetCacheStatisticsAsync(CancellationToken cancellationToken = default)
    {
        // Remove expired entries first
        var userIds = _cache.Keys.ToList();
        foreach (var userId in userIds)
        {
            if (_cache.TryGetValue(userId, out var entry) && DateTime.UtcNow >= entry.ExpirationTime)
            {
                _cache.TryRemove(userId, out _);
            }
        }

        var stats = new PermissionCacheStatisticsDto
        {
            CachedUserCount = _cache.Count,
            TotalHits = 0,
            TotalMisses = 0,
            AveragePermissionsPerUser = _cache.Count > 0 ? _cache.Values.Average(entry => entry.Permissions.Count) : 0,
            ApproximateMemoryUsageBytes = _cache.Sum(kvp => kvp.Value.Permissions.Sum(p => p.Length)),
            LastResetAt = null
        };

        return Task.FromResult(stats);
    }

    public Task ResetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        // Simple in-memory implementation - no stats to reset
        return Task.CompletedTask;
    }

    #endregion
}
