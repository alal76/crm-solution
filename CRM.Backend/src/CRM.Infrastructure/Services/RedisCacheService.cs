/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 */

using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Redis cache configuration options
/// </summary>
public class RedisCacheOptions
{
    public const string SectionName = "Redis";
    
    /// <summary>Redis connection string (e.g., "localhost:6379")</summary>
    public string ConnectionString { get; set; } = "localhost:6379";
    
    /// <summary>Instance name prefix for all cache keys</summary>
    public string InstanceName { get; set; } = "crm_";
    
    /// <summary>Whether Redis caching is enabled</summary>
    public bool Enabled { get; set; } = true;
    
    /// <summary>Default cache duration in minutes</summary>
    public int DefaultExpirationMinutes { get; set; } = 30;
    
    /// <summary>Duration for short-lived cache items (minutes)</summary>
    public int ShortExpirationMinutes { get; set; } = 5;
    
    /// <summary>Duration for long-lived cache items (minutes)</summary>
    public int LongExpirationMinutes { get; set; } = 120;
}

/// <summary>
/// Interface for distributed cache operations
/// </summary>
public interface IRedisCacheService
{
    /// <summary>Get a cached item by key</summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;
    
    /// <summary>Set a cached item with default expiration</summary>
    Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default) where T : class;
    
    /// <summary>Set a cached item with custom expiration</summary>
    Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default) where T : class;
    
    /// <summary>Get or create a cached item</summary>
    Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;
    
    /// <summary>Remove a cached item</summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    
    /// <summary>Remove all cached items with a key prefix</summary>
    Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
    
    /// <summary>Check if Redis is available</summary>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Redis distributed cache service implementation
/// Provides type-safe caching with automatic serialization
/// </summary>
public class RedisCacheService : IRedisCacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly RedisCacheOptions _options;
    private readonly string _instanceName;
    private readonly JsonSerializerOptions _jsonOptions;

    // Static cache of registered keys for prefix-based removal
    private static readonly HashSet<string> RegisteredKeys = new();
    private static readonly object KeysLock = new();

    public RedisCacheService(
        IDistributedCache cache,
        IOptions<RedisCacheOptions> options,
        ILogger<RedisCacheService> logger)
    {
        _cache = cache;
        _options = options.Value;
        _logger = logger;
        _instanceName = _options.InstanceName;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        if (!_options.Enabled) return null;

        try
        {
            var fullKey = GetFullKey(key);
            var data = await _cache.GetStringAsync(fullKey, cancellationToken);
            
            if (string.IsNullOrEmpty(data))
            {
                return null;
            }

            return JsonSerializer.Deserialize<T>(data, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get cached value for key: {Key}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default) where T : class
    {
        await SetAsync(key, value, TimeSpan.FromMinutes(_options.DefaultExpirationMinutes), cancellationToken);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default) where T : class
    {
        if (!_options.Enabled) return;

        try
        {
            var fullKey = GetFullKey(key);
            var data = JsonSerializer.Serialize(value, _jsonOptions);
            
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration,
                SlidingExpiration = TimeSpan.FromMinutes(Math.Min(expiration.TotalMinutes / 2, 15))
            };

            await _cache.SetStringAsync(fullKey, data, options, cancellationToken);
            
            // Track key for prefix-based removal
            lock (KeysLock)
            {
                RegisteredKeys.Add(fullKey);
            }

            _logger.LogDebug("Cached value for key: {Key} with expiration: {Expiration}", key, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to set cached value for key: {Key}", key);
        }
    }

    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        // Try to get from cache first
        var cached = await GetAsync<T>(key, cancellationToken);
        if (cached != null)
        {
            return cached;
        }

        // Execute factory to get value
        var value = await factory();
        if (value == null)
        {
            return null;
        }

        // Cache the value
        var exp = expiration ?? TimeSpan.FromMinutes(_options.DefaultExpirationMinutes);
        await SetAsync(key, value, exp, cancellationToken);

        return value;
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled) return;

        try
        {
            var fullKey = GetFullKey(key);
            await _cache.RemoveAsync(fullKey, cancellationToken);
            
            lock (KeysLock)
            {
                RegisteredKeys.Remove(fullKey);
            }

            _logger.LogDebug("Removed cached value for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to remove cached value for key: {Key}", key);
        }
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled) return;

        try
        {
            var fullPrefix = GetFullKey(prefix);
            List<string> keysToRemove;
            
            lock (KeysLock)
            {
                keysToRemove = RegisteredKeys
                    .Where(k => k.StartsWith(fullPrefix, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            foreach (var key in keysToRemove)
            {
                await _cache.RemoveAsync(key, cancellationToken);
                lock (KeysLock)
                {
                    RegisteredKeys.Remove(key);
                }
            }

            _logger.LogDebug("Removed {Count} cached values with prefix: {Prefix}", keysToRemove.Count, prefix);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to remove cached values with prefix: {Prefix}", prefix);
        }
    }

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled) return false;

        try
        {
            var testKey = GetFullKey("__health_check__");
            await _cache.SetStringAsync(testKey, "ok", new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5)
            }, cancellationToken);
            
            var result = await _cache.GetStringAsync(testKey, cancellationToken);
            return result == "ok";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis health check failed");
            return false;
        }
    }

    private string GetFullKey(string key) => $"{_instanceName}{key}";
}

/// <summary>
/// Fallback cache service that uses in-memory caching when Redis is unavailable
/// </summary>
public class FallbackCacheService : IRedisCacheService
{
    private readonly IRedisCacheService _redisCache;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<FallbackCacheService> _logger;
    private readonly RedisCacheOptions _options;
    private bool _redisAvailable = true;
    private DateTime _lastRedisCheck = DateTime.MinValue;
    private static readonly TimeSpan RedisCheckInterval = TimeSpan.FromMinutes(1);

    public FallbackCacheService(
        IRedisCacheService redisCache,
        IMemoryCache memoryCache,
        IOptions<RedisCacheOptions> options,
        ILogger<FallbackCacheService> logger)
    {
        _redisCache = redisCache;
        _memoryCache = memoryCache;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        if (await ShouldUseRedisAsync(cancellationToken))
        {
            var result = await _redisCache.GetAsync<T>(key, cancellationToken);
            if (result != null) return result;
        }

        // Fallback to memory cache
        if (_memoryCache.TryGetValue($"fallback:{key}", out object? cachedValue) && cachedValue is T typedValue)
        {
            return typedValue;
        }
        return null;
    }

    public async Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default) where T : class
    {
        await SetAsync(key, value, TimeSpan.FromMinutes(_options.DefaultExpirationMinutes), cancellationToken);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default) where T : class
    {
        if (await ShouldUseRedisAsync(cancellationToken))
        {
            await _redisCache.SetAsync(key, value, expiration, cancellationToken);
        }

        // Always set in memory cache as fallback using extension method
        _memoryCache.Set($"fallback:{key}", value, expiration);
    }

    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        var cached = await GetAsync<T>(key, cancellationToken);
        if (cached != null) return cached;

        var value = await factory();
        if (value == null) return null;

        await SetAsync(key, value, expiration ?? TimeSpan.FromMinutes(_options.DefaultExpirationMinutes), cancellationToken);
        return value;
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        if (await ShouldUseRedisAsync(cancellationToken))
        {
            await _redisCache.RemoveAsync(key, cancellationToken);
        }
        _memoryCache.Remove($"fallback:{key}");
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        if (await ShouldUseRedisAsync(cancellationToken))
        {
            await _redisCache.RemoveByPrefixAsync(prefix, cancellationToken);
        }
        // Memory cache doesn't support prefix removal easily
    }

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return await _redisCache.IsAvailableAsync(cancellationToken);
    }

    private async Task<bool> ShouldUseRedisAsync(CancellationToken cancellationToken)
    {
        if (!_options.Enabled) return false;

        // Periodically recheck Redis availability
        if (!_redisAvailable && DateTime.UtcNow - _lastRedisCheck > RedisCheckInterval)
        {
            _redisAvailable = await _redisCache.IsAvailableAsync(cancellationToken);
            _lastRedisCheck = DateTime.UtcNow;
            
            if (_redisAvailable)
            {
                _logger.LogInformation("Redis connection restored");
            }
        }

        return _redisAvailable;
    }
}
