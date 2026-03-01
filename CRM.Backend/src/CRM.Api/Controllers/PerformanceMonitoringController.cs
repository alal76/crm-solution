// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Api.Authorization;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for performance monitoring and optimization
/// </summary>
[ApiController]
[Route("api/performance")]
[RequireRole(UserRole.Admin)]
public class PerformanceMonitoringController : CrmControllerBase
{
    private readonly IPerformanceOptimizationService _service;
    private readonly ILogger<PerformanceMonitoringController> _logger;

    public PerformanceMonitoringController(
        IPerformanceOptimizationService service,
        ILogger<PerformanceMonitoringController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Get performance dashboard
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(PerformanceDashboardDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PerformanceDashboardDto>> GetPerformanceDashboard(CancellationToken cancellationToken = default)
    {
                var dashboard = await _service.GetPerformanceDashboardAsync(cancellationToken);
        return Ok(dashboard);
    }

    /// <summary>
    /// Get statistics for a specific endpoint
    /// </summary>
    [HttpGet("endpoints/{endpoint}")]
    [ProducesResponseType(typeof(PerformanceStatisticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PerformanceStatisticsDto>> GetEndpointStatistics(string endpoint, CancellationToken cancellationToken = default)
    {
                var stats = await _service.GetEndpointStatisticsAsync(endpoint, cancellationToken: cancellationToken);

        if (stats == null)
        {
            return NotFound(new { error = $"No statistics found for endpoint {endpoint}" });
        }

        return Ok(stats);
    }

    /// <summary>
    /// Get slowest endpoints
    /// </summary>
    [HttpGet("slow-endpoints")]
    [ProducesResponseType(typeof(IEnumerable<PerformanceStatisticsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PerformanceStatisticsDto>>> GetSlowEndpoints(int count = 10, CancellationToken cancellationToken = default)
    {
                var endpoints = await _service.GetSlowEndpointsAsync(count, cancellationToken);
        return Ok(endpoints);
    }

    /// <summary>
    /// Get query performance analysis
    /// </summary>
    [HttpGet("query-performance")]
    [ProducesResponseType(typeof(IEnumerable<QueryPerformanceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<QueryPerformanceDto>>> GetQueryPerformance(int count = 10, CancellationToken cancellationToken = default)
    {
                var queries = await _service.GetQueryPerformanceAsync(count, cancellationToken);
        return Ok(queries);
    }

    /// <summary>
    /// Get performance recommendations
    /// </summary>
    [HttpGet("recommendations")]
    [ProducesResponseType(typeof(IEnumerable<PerformanceRecommendationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PerformanceRecommendationDto>>> GetRecommendations(CancellationToken cancellationToken = default)
    {
                var recommendations = await _service.GetPerformanceRecommendationsAsync(cancellationToken);
        return Ok(recommendations);
    }

    /// <summary>
    /// Get cache statistics
    /// </summary>
    [HttpGet("cache")]
    [ProducesResponseType(typeof(CacheStatisticsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CacheStatisticsDto>> GetCacheStatistics(CancellationToken cancellationToken = default)
    {
                var stats = await _service.GetCacheStatisticsAsync(cancellationToken);
        return Ok(stats);
    }

    /// <summary>
    /// Clear cache
    /// </summary>
    [HttpPost("cache/clear")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ClearCache(string? pattern = null, CancellationToken cancellationToken = default)
    {
                var result = await _service.ClearCacheAsync(pattern, cancellationToken);

        if (!result)
        {
            return BadRequest(new { error = "Failed to clear cache" });
        }

        _logger.LogInformation("Cache cleared{Pattern}", pattern != null ? $" (pattern: {pattern})" : "");
        return Ok(new { message = "Cache cleared successfully" });
    }

    /// <summary>
    /// Get rate limit configuration
    /// </summary>
    [HttpGet("rate-limit/{endpoint}")]
    [ProducesResponseType(typeof(RateLimitConfigDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<RateLimitConfigDto>> GetRateLimit(string endpoint, CancellationToken cancellationToken = default)
    {
                var config = await _service.GetRateLimitAsync(endpoint, cancellationToken);

        if (config == null)
        {
            return NotFound(new { error = "Rate limit configuration not found" });
        }

        return Ok(config);
    }

    /// <summary>
    /// Update rate limit configuration
    /// </summary>
    [HttpPut("rate-limit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateRateLimit(RateLimitConfigDto dto, CancellationToken cancellationToken = default)
    {
                var result = await _service.UpdateRateLimitAsync(dto, cancellationToken);

        if (!result)
        {
            return BadRequest(new { error = "Failed to update rate limit" });
        }

        _logger.LogInformation("Rate limit updated for {Endpoint}", dto.Endpoint);
        return Ok(new { message = "Rate limit updated" });
    }

    /// <summary>
    /// Get error statistics
    /// </summary>
    [HttpGet("errors")]
    [ProducesResponseType(typeof(ErrorStatisticsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ErrorStatisticsDto>> GetErrorStatistics(CancellationToken cancellationToken = default)
    {
                var stats = await _service.GetErrorStatisticsAsync(cancellationToken: cancellationToken);
        return Ok(stats);
    }

    /// <summary>
    /// Purge old performance metrics
    /// </summary>
    [HttpPost("metrics/purge")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> PurgeOldMetrics(int daysToKeep = 30, CancellationToken cancellationToken = default)
    {
                if (daysToKeep < 1)
                {
            return BadRequest(new { error = "daysToKeep must be at least 1" });
                }

        var count = await _service.PurgeOldMetricsAsync(daysToKeep, cancellationToken);

        _logger.LogInformation("Purged {Count} old performance metrics", count);
        return Ok(new { message = $"Purged {count} metrics older than {daysToKeep} days" });
    }
}
