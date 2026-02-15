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

/// <summary>
/// DTO for performance dashboard
/// </summary>
public class PerformanceDashboardDto
{
    public double AverageResponseTimeMs { get; set; }
    public int P95ResponseTimeMs { get; set; }
    public int P99ResponseTimeMs { get; set; }
    public double CacheHitRate { get; set; }
    public double ErrorRate { get; set; }
    public int TotalRequestsLastHour { get; set; }
    public int TotalRequestsLastDay { get; set; }
    public PerformanceStatisticsDto[] TopEndpoints { get; set; } = Array.Empty<PerformanceStatisticsDto>();
    public PerformanceRecommendationDto[] Recommendations { get; set; } = Array.Empty<PerformanceRecommendationDto>();
}

/// <summary>
/// DTO for error statistics
/// </summary>
public class ErrorStatisticsDto
{
    public int TotalErrors { get; set; }
    public double ErrorRate { get; set; }
    public Dictionary<int, int> ErrorsByStatus { get; set; } = new();
    public Dictionary<string, int> ErrorsByEndpoint { get; set; } = new();
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}
