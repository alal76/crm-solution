// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Collections.Concurrent;
using CRM.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Rate limiting service implementation with in-memory and optional Redis backing.
/// TODO-SYS005-002: Rate Limiting Service
/// </summary>
public class RateLimitingService : IRateLimitingService
{
    private readonly ILogger<RateLimitingService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IConnectionMultiplexer? _redis;
    private readonly bool _useRedis;

    // In-memory storage for rate limits (fallback when Redis unavailable)
    private readonly ConcurrentDictionary<string, RateLimitEntry> _inMemoryLimits = new();

    // Default limits per endpoint (requests per minute)
    private readonly Dictionary<string, int> _endpointLimits;
    private readonly int _defaultLimit;
    private readonly int _defaultPeriodSeconds;

    public RateLimitingService(
        ILogger<RateLimitingService> logger,
        IConfiguration configuration,
        IConnectionMultiplexer? redis = null)
    {
        _logger = logger;
        _configuration = configuration;
        _redis = redis;
        _useRedis = redis != null && configuration.GetValue("Redis:Enabled", true);

        // Load endpoint limits from configuration
        _endpointLimits = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var endpointRules = configuration.GetSection("RateLimiting:EndpointRules");
        foreach (var rule in endpointRules.GetChildren())
        {
            var limit = rule.GetValue<int>("Limit", 100);
            _endpointLimits[rule.Key] = limit;
        }

        _defaultLimit = configuration.GetValue("RateLimiting:DefaultLimit", 100);
        _defaultPeriodSeconds = configuration.GetValue("RateLimiting:DefaultPeriodSeconds", 60);
    }

    /// <inheritdoc />
    public async Task<RateLimitQuotaResult> CheckQuotaAsync(int userId, string endpoint, CancellationToken ct = default)
    {
        var key = GetRateLimitKey(userId, endpoint);
        var limit = GetEndpointLimit(endpoint);
        var periodSeconds = GetEndpointPeriodSeconds(endpoint);

        int currentUsage = await GetCurrentUsageAsync(key);
        var resetAt = GetWindowResetTime(periodSeconds);
        var remaining = Math.Max(0, limit - currentUsage);

        return new RateLimitQuotaResult
        {
            IsAllowed = currentUsage < limit,
            RemainingQuota = remaining,
            MaxQuota = limit,
            ResetAt = resetAt,
            RetryAfterSeconds = currentUsage >= limit ? (int)(resetAt - DateTime.UtcNow).TotalSeconds : 0,
            Endpoint = endpoint
        };
    }

    /// <inheritdoc />
    public async Task<RateLimitQuotaResult> CheckAndIncrementAsync(int userId, string endpoint, CancellationToken ct = default)
    {
        var key = GetRateLimitKey(userId, endpoint);
        var limit = GetEndpointLimit(endpoint);
        var periodSeconds = GetEndpointPeriodSeconds(endpoint);

        int currentUsage = await GetCurrentUsageAsync(key);

        if (currentUsage >= limit)
        {
            var resetAt = GetWindowResetTime(periodSeconds);
            return new RateLimitQuotaResult
            {
                IsAllowed = false,
                RemainingQuota = 0,
                MaxQuota = limit,
                ResetAt = resetAt,
                RetryAfterSeconds = (int)(resetAt - DateTime.UtcNow).TotalSeconds,
                Endpoint = endpoint
            };
        }

        await IncrementUsageInternalAsync(key, periodSeconds);
        var remaining = Math.Max(0, limit - currentUsage - 1);

        return new RateLimitQuotaResult
        {
            IsAllowed = true,
            RemainingQuota = remaining,
            MaxQuota = limit,
            ResetAt = GetWindowResetTime(periodSeconds),
            RetryAfterSeconds = 0,
            Endpoint = endpoint
        };
    }

    /// <inheritdoc />
    public async Task IncrementUsageAsync(int userId, string endpoint, int count = 1, CancellationToken ct = default)
    {
        var key = GetRateLimitKey(userId, endpoint);
        var periodSeconds = GetEndpointPeriodSeconds(endpoint);

        for (int i = 0; i < count; i++)
        {
            await IncrementUsageInternalAsync(key, periodSeconds);
        }
    }

    /// <inheritdoc />
    public async Task<RateLimitUsageStats> GetUsageStatsAsync(int userId, CancellationToken ct = default)
    {
        var stats = new RateLimitUsageStats
        {
            UserId = userId,
            PeriodStart = GetWindowStartTime(_defaultPeriodSeconds),
            PeriodEnd = GetWindowResetTime(_defaultPeriodSeconds),
            EndpointStats = new Dictionary<string, EndpointUsageStats>()
        };

        // Get stats for commonly tracked endpoints
        var endpoints = new[] { "/api/accounts", "/api/contacts", "/api/leads", "/api/opportunities", "/api/auth/login" };

        foreach (var endpoint in endpoints)
        {
            var key = GetRateLimitKey(userId, endpoint);
            var usage = await GetCurrentUsageAsync(key);
            var limit = GetEndpointLimit(endpoint);

            stats.EndpointStats[endpoint] = new EndpointUsageStats
            {
                Endpoint = endpoint,
                CurrentUsage = usage,
                MaxAllowed = limit
            };

            stats.TotalRequests += usage;
        }

        return stats;
    }

    /// <inheritdoc />
    public async Task ResetQuotaAsync(int userId, string endpoint, CancellationToken ct = default)
    {
        if (endpoint == "*")
        {
            // Reset all endpoints for user
            var endpoints = new[] { "/api/accounts", "/api/contacts", "/api/leads", "/api/opportunities", "/api/auth/login" };
            foreach (var ep in endpoints)
            {
                var key = GetRateLimitKey(userId, ep);
                await ResetKeyAsync(key);
            }
        }
        else
        {
            var key = GetRateLimitKey(userId, endpoint);
            await ResetKeyAsync(key);
        }

        _logger.LogInformation("Reset rate limit quota for user {UserId}, endpoint {Endpoint}", userId, endpoint);
    }

    /// <inheritdoc />
    public int GetEndpointLimit(string endpoint)
    {
        // Normalize endpoint
        var normalizedEndpoint = NormalizeEndpoint(endpoint);

        if (_endpointLimits.TryGetValue(normalizedEndpoint, out var limit))
        {
            return limit;
        }

        // Check for pattern matches
        foreach (var (pattern, patternLimit) in _endpointLimits)
        {
            if (normalizedEndpoint.StartsWith(pattern, StringComparison.OrdinalIgnoreCase))
            {
                return patternLimit;
            }
        }

        return _defaultLimit;
    }

    /// <inheritdoc />
    public int GetEndpointPeriodSeconds(string endpoint)
    {
        // For now, all endpoints use the same period
        // Could be extended to support per-endpoint periods
        return _defaultPeriodSeconds;
    }

    private string GetRateLimitKey(int userId, string endpoint)
    {
        var normalizedEndpoint = NormalizeEndpoint(endpoint);
        var windowStart = GetWindowStartTime(_defaultPeriodSeconds);
        return $"ratelimit:{userId}:{normalizedEndpoint}:{windowStart:yyyyMMddHHmm}";
    }

    private string NormalizeEndpoint(string endpoint)
    {
        // Remove query strings and trailing slashes
        var normalized = endpoint.Split('?')[0].TrimEnd('/').ToLowerInvariant();

        // Remove numeric IDs from paths (e.g., /api/accounts/123 -> /api/accounts)
        var segments = normalized.Split('/');
        var cleanedSegments = segments.Where(s => !int.TryParse(s, out _)).ToArray();

        return string.Join("/", cleanedSegments);
    }

    private DateTime GetWindowStartTime(int periodSeconds)
    {
        var now = DateTime.UtcNow;
        var epochSeconds = (long)(now - DateTime.UnixEpoch).TotalSeconds;
        var windowStart = epochSeconds - (epochSeconds % periodSeconds);
        return DateTime.UnixEpoch.AddSeconds(windowStart);
    }

    private DateTime GetWindowResetTime(int periodSeconds)
    {
        return GetWindowStartTime(periodSeconds).AddSeconds(periodSeconds);
    }

    private async Task<int> GetCurrentUsageAsync(string key)
    {
        if (_useRedis && _redis != null)
        {
            try
            {
                var db = _redis.GetDatabase();
                var value = await db.StringGetAsync(key);
                return value.HasValue ? (int)value : 0;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get rate limit from Redis, falling back to in-memory");
            }
        }

        // In-memory fallback
        if (_inMemoryLimits.TryGetValue(key, out var entry) && entry.ExpiresAt > DateTime.UtcNow)
        {
            return entry.Count;
        }

        return 0;
    }

    private async Task IncrementUsageInternalAsync(string key, int periodSeconds)
    {
        if (_useRedis && _redis != null)
        {
            try
            {
                var db = _redis.GetDatabase();
                var newValue = await db.StringIncrementAsync(key);

                if (newValue == 1)
                {
                    // First request in window, set expiration
                    await db.KeyExpireAsync(key, TimeSpan.FromSeconds(periodSeconds));
                }

                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to increment rate limit in Redis, falling back to in-memory");
            }
        }

        // In-memory fallback
        var expiresAt = GetWindowResetTime(periodSeconds);
        _inMemoryLimits.AddOrUpdate(key,
            _ => new RateLimitEntry { Count = 1, ExpiresAt = expiresAt },
            (_, existing) =>
            {
                if (existing.ExpiresAt <= DateTime.UtcNow)
                {
                    return new RateLimitEntry { Count = 1, ExpiresAt = expiresAt };
                }
                existing.Count++;
                return existing;
            });
    }

    private async Task ResetKeyAsync(string key)
    {
        if (_useRedis && _redis != null)
        {
            try
            {
                var db = _redis.GetDatabase();
                await db.KeyDeleteAsync(key);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to reset rate limit in Redis");
            }
        }

        _inMemoryLimits.TryRemove(key, out _);
    }

    private class RateLimitEntry
    {
        public int Count { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
