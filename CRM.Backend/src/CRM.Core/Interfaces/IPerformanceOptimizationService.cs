// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for performance monitoring, optimization, and analysis
/// </summary>
public interface IPerformanceOptimizationService
{
    /// <summary>
    /// Record an API request metric
    /// </summary>
    Task<bool> RecordMetricAsync(PerformanceMetricDto metric, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get performance statistics for an endpoint
    /// </summary>
    Task<PerformanceStatisticsDto?> GetEndpointStatisticsAsync(string endpoint, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get top slow endpoints
    /// </summary>
    Task<IEnumerable<PerformanceStatisticsDto>> GetSlowEndpointsAsync(int count = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get query performance analysis
    /// </summary>
    Task<IEnumerable<QueryPerformanceDto>> GetQueryPerformanceAsync(int count = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get performance recommendations
    /// </summary>
    Task<IEnumerable<PerformanceRecommendationDto>> GetPerformanceRecommendationsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get overall system performance dashboard
    /// </summary>
    Task<PerformanceDashboardDto> GetPerformanceDashboardAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get cache statistics
    /// </summary>
    Task<CacheStatisticsDto> GetCacheStatisticsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Clear cache
    /// </summary>
    Task<bool> ClearCacheAsync(string? pattern = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get rate limit status for an endpoint
    /// </summary>
    Task<RateLimitConfigDto?> GetRateLimitAsync(string endpoint, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update rate limit configuration
    /// </summary>
    Task<bool> UpdateRateLimitAsync(RateLimitConfigDto config, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get error rate statistics
    /// </summary>
    Task<ErrorStatisticsDto> GetErrorStatisticsAsync(DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Purge old performance metrics
    /// </summary>
    Task<int> PurgeOldMetricsAsync(int daysToKeep = 30, CancellationToken cancellationToken = default);
}
