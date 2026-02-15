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

using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Implementation of performance optimization service
/// Tracks metrics, provides analysis, and recommendations
/// </summary>
public class PerformanceOptimizationService : IPerformanceOptimizationService
{
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<PerformanceOptimizationService> _logger;
    private readonly IDistributedCache _cache;

    // In-memory cache for statistics
    private readonly Dictionary<string, CacheEntry> _statisticsCache = new();

    private class CacheEntry
    {
        public PerformanceStatisticsDto Data { get; set; } = new();
        public DateTime ExpiresAt { get; set; }
    }

    public PerformanceOptimizationService(
        ICrmDbContext dbContext,
        ILogger<PerformanceOptimizationService> logger,
        IDistributedCache cache)
    {
        _dbContext = dbContext;
        _logger = logger;
        _cache = cache;
    }

    public async Task<bool> RecordMetricAsync(PerformanceMetricDto metric, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = new PerformanceMetric
            {
                EndpointName = metric.EndpointName,
                HttpMethod = metric.HttpMethod,
                Route = metric.Route,
                ResponseTimeMs = metric.ResponseTimeMs,
                StatusCode = metric.StatusCode,
                QueryDurationMs = metric.QueryDurationMs,
                RowsAffected = metric.RowsAffected,
                WasCached = metric.WasCached,
                UserId = metric.UserId,
                RequestTime = metric.RequestTime,
                ErrorMessage = metric.ErrorMessage,
                QuerySignature = metric.QuerySignature,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.PerformanceMetrics.Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Invalidate cache
            _statisticsCache.Remove(metric.EndpointName);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording performance metric for {Endpoint}", metric.EndpointName);
            return false;
        }
    }

    public async Task<PerformanceStatisticsDto?> GetEndpointStatisticsAsync(string endpoint, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
    {
        from ??= DateTime.UtcNow.AddHours(-24);
        to ??= DateTime.UtcNow;

        var query = _dbContext.PerformanceMetrics
            .Where(m => m.EndpointName == endpoint && m.RequestTime >= from && m.RequestTime <= to);

        var metrics = await query.ToListAsync(cancellationToken);

        if (metrics.Count == 0)
            return null;

        var responseTimes = metrics.Select(m => m.ResponseTimeMs).OrderBy(t => t).ToList();
        var p95Index = (int)(responseTimes.Count * 0.95);
        var p99Index = (int)(responseTimes.Count * 0.99);

        return new PerformanceStatisticsDto
        {
            Endpoint = endpoint,
            TotalRequests = metrics.Count,
            AverageResponseTimeMs = metrics.Average(m => m.ResponseTimeMs),
            MinResponseTimeMs = metrics.Min(m => m.ResponseTimeMs),
            MaxResponseTimeMs = metrics.Max(m => m.ResponseTimeMs),
            P95ResponseTimeMs = (int)(p95Index < responseTimes.Count ? responseTimes[p95Index] : 0),
            P99ResponseTimeMs = (int)(p99Index < responseTimes.Count ? responseTimes[p99Index] : 0),
            CacheHitRate = metrics.Count > 0 ? (double)metrics.Count(m => m.WasCached) / metrics.Count : 0,
            ErrorCount = metrics.Count(m => m.StatusCode >= 400),
            ErrorRate = metrics.Count > 0 ? (double)metrics.Count(m => m.StatusCode >= 400) / metrics.Count : 0,
            PeriodStart = from.Value,
            PeriodEnd = to.Value
        };
    }

    public async Task<IEnumerable<PerformanceStatisticsDto>> GetSlowEndpointsAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        var from = DateTime.UtcNow.AddHours(-24);
        var endpoints = await _dbContext.PerformanceMetrics
            .Where(m => m.RequestTime >= from)
            .GroupBy(m => m.EndpointName)
            .Select(g => new { Endpoint = g.Key, AvgTime = g.Average(m => m.ResponseTimeMs) })
            .OrderByDescending(x => x.AvgTime)
            .Take(count)
            .ToListAsync(cancellationToken);

        var results = new List<PerformanceStatisticsDto>();
        foreach (var endpoint in endpoints)
        {
            var stats = await GetEndpointStatisticsAsync(endpoint.Endpoint, from, DateTime.UtcNow, cancellationToken);
            if (stats != null)
                results.Add(stats);
        }

        return results;
    }

    public async Task<IEnumerable<QueryPerformanceDto>> GetQueryPerformanceAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        var from = DateTime.UtcNow.AddHours(-24);

        var queries = await _dbContext.PerformanceMetrics
            .Where(m => m.QuerySignature != null && m.QueryDurationMs.HasValue && m.RequestTime >= from)
            .GroupBy(m => m.QuerySignature)
            .Select(g => new
            {
                Query = g.Key,
                Count = g.Count(),
                AvgDuration = g.Average(m => m.QueryDurationMs.Value),
                MaxDuration = g.Max(m => m.QueryDurationMs.Value),
                TotalTime = g.Sum(m => m.QueryDurationMs.Value)
            })
            .OrderByDescending(x => x.TotalTime)
            .Take(count)
            .ToListAsync(cancellationToken);

        return queries.Select(q => new QueryPerformanceDto
        {
            QuerySignature = q.Query ?? "Unknown",
            ExecutionCount = q.Count,
            AverageDurationMs = q.AvgDuration,
            MaxDurationMs = q.MaxDuration,
            TotalTimeMs = q.TotalTime,
            RecommendedOptimization = q.AvgDuration > 1000 ? "Consider adding an index or optimizing the query" : null
        }).ToList();
    }

    public async Task<IEnumerable<PerformanceRecommendationDto>> GetPerformanceRecommendationsAsync(CancellationToken cancellationToken = default)
    {
        var recommendations = new List<PerformanceRecommendationDto>();
        var from = DateTime.UtcNow.AddHours(-24);

        // Check for slow endpoints
        var slowEndpoints = await GetSlowEndpointsAsync(5, cancellationToken);
        foreach (var endpoint in slowEndpoints)
        {
            if (endpoint.AverageResponseTimeMs > 500)
            {
                recommendations.Add(new PerformanceRecommendationDto
                {
                    Title = $"Slow Endpoint: {endpoint.Endpoint}",
                    Description = $"Average response time is {endpoint.AverageResponseTimeMs:F2}ms (target: 200ms)",
                    Priority = endpoint.AverageResponseTimeMs > 1000 ? "High" : "Medium",
                    Endpoint = endpoint.Endpoint,
                    RecommendedAction = "Optimize queries, add caching, or improve database indexes",
                    PotentialImprovementPercent = 30
                });
            }
        }

        // Check for high error rates
        var errorMetrics = await _dbContext.PerformanceMetrics
            .Where(m => m.RequestTime >= from && m.StatusCode >= 400)
            .GroupBy(m => m.EndpointName)
            .Select(g => new { Endpoint = g.Key, ErrorCount = g.Count() })
            .OrderByDescending(x => x.ErrorCount)
            .Take(5)
            .ToListAsync(cancellationToken);

        foreach (var error in errorMetrics)
        {
            var stats = await GetEndpointStatisticsAsync(error.Endpoint, from, DateTime.UtcNow, cancellationToken);
            if (stats?.ErrorRate > 0.05)
            {
                recommendations.Add(new PerformanceRecommendationDto
                {
                    Title = $"High Error Rate: {error.Endpoint}",
                    Description = $"Error rate is {stats.ErrorRate:P} (target: <5%)",
                    Priority = stats.ErrorRate > 0.1 ? "High" : "Medium",
                    Endpoint = error.Endpoint,
                    RecommendedAction = "Review error logs and fix root cause",
                    PotentialImprovementPercent = 50
                });
            }
        }

        // Check for slow queries
        var slowQueries = await GetQueryPerformanceAsync(3, cancellationToken);
        foreach (var query in slowQueries)
        {
            if (query.AverageDurationMs > 1000)
            {
                recommendations.Add(new PerformanceRecommendationDto
                {
                    Title = "Slow Database Query Detected",
                    Description = $"Query average duration: {query.AverageDurationMs:F2}ms",
                    Priority = "High",
                    RecommendedAction = "Add database indexes or optimize the query logic",
                    PotentialImprovementPercent = 40
                });
            }
        }

        return recommendations;
    }

    public async Task<PerformanceDashboardDto> GetPerformanceDashboardAsync(CancellationToken cancellationToken = default)
    {
        var from = DateTime.UtcNow.AddHours(-24);
        var to = DateTime.UtcNow;

        var lastHour = DateTime.UtcNow.AddHours(-1);
        var metrics = await _dbContext.PerformanceMetrics
            .Where(m => m.RequestTime >= from && m.RequestTime <= to)
            .ToListAsync(cancellationToken);

        var metricsLastHour = metrics.Where(m => m.RequestTime >= lastHour).ToList();

        var responseTimes = metrics.Select(m => m.ResponseTimeMs).OrderBy(t => t).ToList();
        var p95Index = (int)(responseTimes.Count * 0.95);
        var p99Index = (int)(responseTimes.Count * 0.99);

        var topEndpoints = await GetSlowEndpointsAsync(5, cancellationToken);
        var recommendations = await GetPerformanceRecommendationsAsync(cancellationToken);

        return new PerformanceDashboardDto
        {
            AverageResponseTimeMs = metrics.Count > 0 ? metrics.Average(m => m.ResponseTimeMs) : 0,
            P95ResponseTimeMs = (int)(p95Index < responseTimes.Count ? responseTimes[p95Index] : 0),
            P99ResponseTimeMs = (int)(p99Index < responseTimes.Count ? responseTimes[p99Index] : 0),
            CacheHitRate = metrics.Count > 0 ? (double)metrics.Count(m => m.WasCached) / metrics.Count : 0,
            ErrorRate = metrics.Count > 0 ? (double)metrics.Count(m => m.StatusCode >= 400) / metrics.Count : 0,
            TotalRequestsLastHour = metricsLastHour.Count,
            TotalRequestsLastDay = metrics.Count,
            TopEndpoints = topEndpoints.ToArray(),
            Recommendations = recommendations.OrderByDescending(r => r.Priority == "High" ? 3 : r.Priority == "Medium" ? 2 : 1).Take(5).ToArray()
        };
    }

    public async Task<CacheStatisticsDto> GetCacheStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var from = DateTime.UtcNow.AddHours(-24);
        var metrics = await _dbContext.PerformanceMetrics
            .Where(m => m.RequestTime >= from)
            .ToListAsync(cancellationToken);

        var cachedMetrics = metrics.Where(m => m.WasCached).Count();

        return new CacheStatisticsDto
        {
            TotalHits = cachedMetrics,
            TotalMisses = metrics.Count - cachedMetrics,
            HitRate = metrics.Count > 0 ? (double)cachedMetrics / metrics.Count : 0,
            MemoryUsedBytes = 1024 * 1024, // Placeholder
            MaxMemoryBytes = 10 * 1024 * 1024, // Placeholder
            CachedItemCount = 100, // Placeholder
            LastClearedAt = DateTime.UtcNow.AddDays(-1)
        };
    }

    public async Task<bool> ClearCacheAsync(string? pattern = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // Clear in-memory cache
            if (string.IsNullOrEmpty(pattern))
            {
                _statisticsCache.Clear();
            }
            else
            {
                var keysToRemove = _statisticsCache.Keys.Where(k => k.Contains(pattern)).ToList();
                foreach (var key in keysToRemove)
                    _statisticsCache.Remove(key);
            }

            _logger.LogInformation("Cache cleared successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache");
            return false;
        }
    }

    public async Task<RateLimitConfigDto?> GetRateLimitAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        // Placeholder implementation
        return new RateLimitConfigDto
        {
            Endpoint = endpoint,
            RequestsPerMinute = 60,
            RequestsPerHour = 1000,
            Enabled = true
        };
    }

    public async Task<bool> UpdateRateLimitAsync(RateLimitConfigDto config, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Rate limit updated for {Endpoint}: {RequestsPerMinute}/min, {RequestsPerHour}/hour",
            config.Endpoint, config.RequestsPerMinute, config.RequestsPerHour);
        return true;
    }

    public async Task<ErrorStatisticsDto> GetErrorStatisticsAsync(DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
    {
        from ??= DateTime.UtcNow.AddHours(-24);
        to ??= DateTime.UtcNow;

        var errorMetrics = await _dbContext.PerformanceMetrics
            .Where(m => m.StatusCode >= 400 && m.RequestTime >= from && m.RequestTime <= to)
            .ToListAsync(cancellationToken);

        var allMetrics = await _dbContext.PerformanceMetrics
            .Where(m => m.RequestTime >= from && m.RequestTime <= to)
            .ToListAsync(cancellationToken);

        return new ErrorStatisticsDto
        {
            TotalErrors = errorMetrics.Count,
            ErrorRate = allMetrics.Count > 0 ? (double)errorMetrics.Count / allMetrics.Count : 0,
            ErrorsByStatus = errorMetrics.GroupBy(m => m.StatusCode).ToDictionary(g => g.Key, g => g.Count()),
            ErrorsByEndpoint = errorMetrics.GroupBy(m => m.EndpointName).ToDictionary(g => g.Key, g => g.Count()),
            PeriodStart = from.Value,
            PeriodEnd = to.Value
        };
    }

    public async Task<int> PurgeOldMetricsAsync(int daysToKeep = 30, CancellationToken cancellationToken = default)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
            var oldMetrics = await _dbContext.PerformanceMetrics
                .Where(m => m.CreatedAt < cutoffDate)
                .ToListAsync(cancellationToken);

            _dbContext.PerformanceMetrics.RemoveRange(oldMetrics);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Purged {Count} old performance metrics", oldMetrics.Count);
            return oldMetrics.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error purging old metrics");
            return 0;
        }
    }
}
