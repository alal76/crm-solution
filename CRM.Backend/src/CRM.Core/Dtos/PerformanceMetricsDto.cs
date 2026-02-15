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

namespace CRM.Core.Dtos;

/// <summary>
/// DTO for performance metrics
/// </summary>
public class PerformanceMetricDto
{
    public int Id { get; set; }
    public string EndpointName { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public long ResponseTimeMs { get; set; }
    public int StatusCode { get; set; }
    public long? QueryDurationMs { get; set; }
    public int? RowsAffected { get; set; }
    public bool WasCached { get; set; }
    public int? UserId { get; set; }
    public DateTime RequestTime { get; set; }
    public string? ErrorMessage { get; set; }
    public string? QuerySignature { get; set; }
}

/// <summary>
/// DTO for performance statistics
/// </summary>
public class PerformanceStatisticsDto
{
    public string Endpoint { get; set; } = string.Empty;
    public int TotalRequests { get; set; }
    public double AverageResponseTimeMs { get; set; }
    public long MinResponseTimeMs { get; set; }
    public long MaxResponseTimeMs { get; set; }
    public int P95ResponseTimeMs { get; set; }
    public int P99ResponseTimeMs { get; set; }
    public double CacheHitRate { get; set; }
    public int ErrorCount { get; set; }
    public double ErrorRate { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}

/// <summary>
/// DTO for query performance analysis
/// </summary>
public class QueryPerformanceDto
{
    public string QuerySignature { get; set; } = string.Empty;
    public int ExecutionCount { get; set; }
    public double AverageDurationMs { get; set; }
    public long MaxDurationMs { get; set; }
    public double TotalTimeMs { get; set; }
    public string? RecommendedOptimization { get; set; }
}

/// <summary>
/// DTO for performance recommendations
/// </summary>
public class PerformanceRecommendationDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium";
    public string Endpoint { get; set; } = string.Empty;
    public string? RecommendedAction { get; set; }
    public double PotentialImprovementPercent { get; set; }
}

/// <summary>
/// DTO for rate limiting configuration
/// </summary>
public class RateLimitConfigDto
{
    public string Endpoint { get; set; } = string.Empty;
    public int RequestsPerMinute { get; set; } = 60;
    public int RequestsPerHour { get; set; } = 1000;
    public bool Enabled { get; set; } = true;
}

/// <summary>
/// DTO for cache statistics
/// </summary>
public class CacheStatisticsDto
{
    public long TotalHits { get; set; }
    public long TotalMisses { get; set; }
    public double HitRate { get; set; }
    public long MemoryUsedBytes { get; set; }
    public long MaxMemoryBytes { get; set; }
    public int CachedItemCount { get; set; }
    public DateTime LastClearedAt { get; set; }
}
