// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal

namespace CRM.Core.Interfaces;

/// <summary>
/// Service for managing permission caching using Redis.
/// Caches user permissions to avoid repeated database queries.
///
/// HEXAGONAL ARCHITECTURE:
/// - Port: Defines contract for permission caching
/// - Accessed by: IRBACService, Request middleware
/// - Depends on: Redis (ICacheService)
///
/// CACHE KEY STRATEGY:
/// - "permission:{userId}" -> Set<string> of permission names
/// - "permission:metadata:{userId}" -> CacheMetadata (timestamp, hit count)
/// - TTL: Configurable, default 1 hour
/// </summary>
public interface IPermissionCacheService
{
    #region Cache Operations
    
    /// <summary>
    /// Get cached permissions for a user.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Set of permission names, or empty if not cached</returns>
    Task<ISet<string>> GetUserPermissionsFromCacheAsync(int userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Set permissions in cache for a user.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="permissions">Set of permission names</param>
    /// <param name="ttlSeconds">Time-to-live in seconds (optional, uses default if null)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task completion</returns>
    Task SetUserPermissionsInCacheAsync(int userId, ISet<string> permissions, int? ttlSeconds = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Check if a user's permissions are cached.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if permissions are cached</returns>
    Task<bool> IsUserPermissionsCachedAsync(int userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Invalidate cache for a specific user.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task completion</returns>
    Task InvalidateUserCacheAsync(int userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Invalidate cache for multiple users.
    /// </summary>
    /// <param name="userIds">List of user IDs</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task completion</returns>
    Task InvalidateMultipleUsersAsync(IEnumerable<int> userIds, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Invalidate all permission caches.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task completion</returns>
    Task InvalidateAllAsync(CancellationToken cancellationToken = default);
    
    #endregion

    #region Cache Warming
    
    /// <summary>
    /// Warm up cache for a user by loading permissions from database.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Set of warmed permissions</returns>
    Task<ISet<string>> WarmUserCacheAsync(int userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Warm up cache for multiple users.
    /// </summary>
    /// <param name="userIds">List of user IDs</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task completion</returns>
    Task WarmMultipleUsersAsync(IEnumerable<int> userIds, CancellationToken cancellationToken = default);
    
    #endregion

    #region Configuration & Statistics
    
    /// <summary>
    /// Get cache TTL configuration in seconds.
    /// </summary>
    /// <returns>TTL in seconds</returns>
    int GetCacheTtlSeconds();
    
    /// <summary>
    /// Set cache TTL configuration in seconds.
    /// </summary>
    /// <param name="ttlSeconds">TTL in seconds</param>
    void SetCacheTtlSeconds(int ttlSeconds);
    
    /// <summary>
    /// Get cache statistics for monitoring.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cache statistics DTO</returns>
    Task<PermissionCacheStatisticsDto> GetCacheStatisticsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Reset cache statistics.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task completion</returns>
    Task ResetStatisticsAsync(CancellationToken cancellationToken = default);
    
    #endregion
}

/// <summary>
/// DTO for cache statistics and monitoring.
/// </summary>
public class PermissionCacheStatisticsDto
{
    /// <summary>
    /// Total number of cached users
    /// </summary>
    public int CachedUserCount { get; set; }
    
    /// <summary>
    /// Total cache hits since last reset
    /// </summary>
    public long TotalHits { get; set; }
    
    /// <summary>
    /// Total cache misses since last reset
    /// </summary>
    public long TotalMisses { get; set; }
    
    /// <summary>
    /// Cache hit rate percentage (0-100)
    /// </summary>
    public decimal HitRatePercentage => TotalHits + TotalMisses > 0 
        ? (TotalHits * 100) / (TotalHits + TotalMisses) 
        : 0;
    
    /// <summary>
    /// Average permissions per cached user
    /// </summary>
    public double AveragePermissionsPerUser { get; set; }
    
    /// <summary>
    /// Current cache memory usage in bytes (approximate)
    /// </summary>
    public long ApproximateMemoryUsageBytes { get; set; }
    
    /// <summary>
    /// When statistics were last reset
    /// </summary>
    public DateTime? LastResetAt { get; set; }
}
