// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal

using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// API controller for admin dashboard and system monitoring.
/// Provides endpoints for system statistics, module status, and provider health.
/// </summary>
[ApiController]
[Route("api/admin")]
[Authorize]
public class AdminDashboardController : ControllerBase
{
    private readonly IAdminDashboardService _adminDashboardService;
    private readonly IProviderHealthService _providerHealthService;
    private readonly IRBACService _rbacService;
    private readonly ILogger<AdminDashboardController> _logger;

    public AdminDashboardController(
        IAdminDashboardService adminDashboardService,
        IProviderHealthService providerHealthService,
        IRBACService rbacService,
        ILogger<AdminDashboardController> logger)
    {
        _adminDashboardService = adminDashboardService;
        _providerHealthService = providerHealthService;
        _rbacService = rbacService;
        _logger = logger;
    }

    /// <summary>
    /// Get complete admin dashboard data
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(AdminDashboardDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAdminDashboard(
        [FromQuery] int timeRangeHours = 24,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var dashboard = await _adminDashboardService.GetCompleteAdminDashboardAsync(timeRangeHours, cancellationToken);
            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin dashboard");
            return StatusCode(500, new { message = "Error retrieving dashboard data" });
        }
    }

    /// <summary>
    /// Get system statistics
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(SystemStatisticsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSystemStatistics(CancellationToken cancellationToken)
    {
        try
        {
            var stats = await _adminDashboardService.GetSystemStatisticsAsync(cancellationToken);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system statistics");
            return StatusCode(500, new { message = "Error retrieving statistics" });
        }
    }

    /// <summary>
    /// Get detailed system statistics with trends
    /// </summary>
    [HttpGet("statistics/detailed")]
    [ProducesResponseType(typeof(DetailedSystemStatisticsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDetailedStatistics(
        [FromQuery] int daysBack = 30,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var stats = await _adminDashboardService.GetDetailedSystemStatisticsAsync(daysBack, cancellationToken);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting detailed statistics");
            return StatusCode(500, new { message = "Error retrieving statistics" });
        }
    }

    /// <summary>
    /// Get operational status of all modules
    /// </summary>
    [HttpGet("modules/status")]
    [ProducesResponseType(typeof(IDictionary<string, ModuleStatusDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetModuleStatus(CancellationToken cancellationToken)
    {
        try
        {
            var status = await _adminDashboardService.GetAllModuleStatusAsync(cancellationToken);
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting module status");
            return StatusCode(500, new { message = "Error retrieving module status" });
        }
    }

    /// <summary>
    /// Check system health
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> IsSystemHealthy(CancellationToken cancellationToken)
    {
        try
        {
            var isHealthy = await _adminDashboardService.IsSystemHealthyAsync(cancellationToken);
            return Ok(new { isHealthy });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking system health");
            return StatusCode(500, new { message = "Error checking health" });
        }
    }

    /// <summary>
    /// Get provider health dashboard
    /// </summary>
    [HttpGet("providers/health")]
    [ProducesResponseType(typeof(ProviderHealthDashboardDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProviderHealth(CancellationToken cancellationToken)
    {
        try
        {
            var health = await _adminDashboardService.GetProviderHealthSummaryAsync(cancellationToken);
            return Ok(health);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting provider health");
            return StatusCode(500, new { message = "Error retrieving provider health" });
        }
    }

    /// <summary>
    /// Get system performance metrics
    /// </summary>
    [HttpGet("performance")]
    [ProducesResponseType(typeof(SystemPerformanceMetricsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPerformanceMetrics(
        [FromQuery] int hoursBack = 24,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var metrics = await _adminDashboardService.GetSystemPerformanceMetricsAsync(hoursBack, cancellationToken);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting performance metrics");
            return StatusCode(500, new { message = "Error retrieving metrics" });
        }
    }

    /// <summary>
    /// Get endpoint performance metrics
    /// </summary>
    [HttpGet("performance/endpoints")]
    [ProducesResponseType(typeof(IEnumerable<EndpointPerformanceMetricsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEndpointMetrics(
        [FromQuery] int hoursBack = 24,
        [FromQuery] int topCount = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var metrics = await _adminDashboardService.GetEndpointPerformanceMetricsAsync(hoursBack, topCount, cancellationToken);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting endpoint metrics");
            return StatusCode(500, new { message = "Error retrieving metrics" });
        }
    }

    /// <summary>
    /// Get database performance metrics
    /// </summary>
    [HttpGet("performance/database")]
    [ProducesResponseType(typeof(DatabasePerformanceMetricsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDatabaseMetrics(
        [FromQuery] int hoursBack = 24,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var metrics = await _adminDashboardService.GetDatabasePerformanceMetricsAsync(hoursBack, cancellationToken);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting database metrics");
            return StatusCode(500, new { message = "Error retrieving metrics" });
        }
    }

    /// <summary>
    /// Get recent alerts
    /// </summary>
    [HttpGet("alerts")]
    [ProducesResponseType(typeof(IEnumerable<AdminAlertDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAlerts(
        [FromQuery] int hoursBack = 24,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var alerts = await _adminDashboardService.GetRecentAlertsAsync(hoursBack, cancellationToken);
            return Ok(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting alerts");
            return StatusCode(500, new { message = "Error retrieving alerts" });
        }
    }

    /// <summary>
    /// Get quick actions summary
    /// </summary>
    [HttpGet("quick-actions")]
    [ProducesResponseType(typeof(QuickActionsSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetQuickActions(CancellationToken cancellationToken)
    {
        try
        {
            var summary = await _adminDashboardService.GetQuickActionsSummaryAsync(cancellationToken);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting quick actions");
            return StatusCode(500, new { message = "Error retrieving actions" });
        }
    }

    /// <summary>
    /// Refresh dashboard cache
    /// </summary>
    [HttpPost("cache/refresh")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RefreshCache(CancellationToken cancellationToken)
    {
        try
        {
            await _adminDashboardService.RefreshDashboardCacheAsync(cancellationToken);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing cache");
            return StatusCode(500, new { message = "Error refreshing cache" });
        }
    }
}
