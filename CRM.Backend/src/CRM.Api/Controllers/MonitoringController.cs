/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 */

using System.Diagnostics;
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CRM.Api.Controllers;

/// <summary>
/// Comprehensive monitoring controller providing infrastructure, system, and database metrics
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class MonitoringController : ControllerBase
{
    private readonly IMonitoringService _monitoringService;
    private readonly CrmDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MonitoringController> _logger;

    public MonitoringController(
        IMonitoringService monitoringService,
        CrmDbContext context,
        IConfiguration configuration,
        ILogger<MonitoringController> logger)
    {
        _monitoringService = monitoringService;
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Get all monitoring data in a single call - primary endpoint for dashboard
    /// </summary>
    [HttpGet("all")]
    public async Task<ActionResult<MonitoringData>> GetAllMonitoringData(CancellationToken ct)
    {
        try
        {
            var data = await _monitoringService.GetAllMonitoringDataAsync(ct);
            
            // Add active sessions from database
            data.ActiveSessions = await GetActiveSessionsFromDb(ct);
            
            return Ok(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all monitoring data");
            return StatusCode(500, new { message = "Error getting monitoring data", error = ex.Message });
        }
    }

    /// <summary>
    /// Get infrastructure detection information
    /// </summary>
    [HttpGet("infrastructure")]
    public async Task<ActionResult<InfrastructureInfo>> GetInfrastructure(CancellationToken ct)
    {
        try
        {
            var info = await _monitoringService.GetInfrastructureInfoAsync(ct);
            return Ok(info);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting infrastructure info");
            return StatusCode(500, new { message = "Error getting infrastructure info" });
        }
    }

    /// <summary>
    /// Get system metrics (CPU, memory, disk, network)
    /// </summary>
    [HttpGet("system")]
    public async Task<ActionResult<SystemMetrics>> GetSystemMetrics(CancellationToken ct)
    {
        try
        {
            var metrics = await _monitoringService.GetSystemMetricsAsync(ct);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system metrics");
            return StatusCode(500, new { message = "Error getting system metrics" });
        }
    }

    /// <summary>
    /// Get database-specific metrics
    /// </summary>
    [HttpGet("database")]
    public async Task<ActionResult<DatabaseMetrics>> GetDatabaseMetrics(CancellationToken ct)
    {
        try
        {
            var metrics = await _monitoringService.GetDatabaseMetricsAsync(ct);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting database metrics");
            return StatusCode(500, new { message = "Error getting database metrics" });
        }
    }

    /// <summary>
    /// Get health status of all services
    /// </summary>
    [HttpGet("services")]
    public async Task<ActionResult<List<ServiceHealth>>> GetServices(CancellationToken ct)
    {
        try
        {
            var services = await _monitoringService.GetServiceHealthAsync(ct);
            return Ok(services);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service health");
            return StatusCode(500, new { message = "Error getting service health" });
        }
    }

    /// <summary>
    /// Get Docker container health (if Docker monitoring is enabled)
    /// </summary>
    [HttpGet("containers")]
    public async Task<ActionResult<List<ContainerHealth>>> GetContainers(CancellationToken ct)
    {
        try
        {
            var containers = await _monitoringService.GetContainerHealthAsync(ct);
            return Ok(containers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting container health");
            return StatusCode(500, new { message = "Error getting container health" });
        }
    }

    /// <summary>
    /// Get Kubernetes pod health (if Kubernetes monitoring is enabled)
    /// </summary>
    [HttpGet("pods")]
    public async Task<ActionResult<List<PodHealth>>> GetPods(CancellationToken ct)
    {
        try
        {
            var pods = await _monitoringService.GetPodHealthAsync(ct);
            return Ok(pods);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pod health");
            return StatusCode(500, new { message = "Error getting pod health" });
        }
    }

    /// <summary>
    /// Get active user sessions
    /// </summary>
    [HttpGet("sessions")]
    public async Task<ActionResult<List<UserSession>>> GetSessions(CancellationToken ct)
    {
        try
        {
            var sessions = await GetActiveSessionsFromDb(ct);
            return Ok(sessions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active sessions");
            return StatusCode(500, new { message = "Error getting active sessions" });
        }
    }

    /// <summary>
    /// Get monitoring options/configuration
    /// </summary>
    [HttpGet("config")]
    public ActionResult<MonitoringOptions> GetConfig()
    {
        try
        {
            var options = _monitoringService.GetMonitoringOptions();
            return Ok(options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting monitoring config");
            return StatusCode(500, new { message = "Error getting monitoring config" });
        }
    }

    /// <summary>
    /// Basic health check endpoint (no auth required for load balancers)
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    public ActionResult GetHealth()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Detailed health check with database connectivity
    /// </summary>
    [HttpGet("health/detailed")]
    public async Task<ActionResult> GetDetailedHealth(CancellationToken ct)
    {
        try
        {
            var dbHealthy = await _context.Database.CanConnectAsync(ct);
            var options = _monitoringService.GetMonitoringOptions();
            
            return Ok(new
            {
                status = dbHealthy ? "healthy" : "degraded",
                timestamp = DateTime.UtcNow,
                database = new
                {
                    provider = options.DatabaseProvider,
                    connected = dbHealthy
                },
                deployment = new
                {
                    type = options.DeploymentType,
                    server = options.BuildServer
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during detailed health check");
            return StatusCode(500, new { status = "error", message = ex.Message });
        }
    }

    #region Private Methods

    private async Task<List<UserSession>> GetActiveSessionsFromDb(CancellationToken ct)
    {
        var recentThreshold = DateTime.UtcNow.AddHours(-24);
        
        var users = await _context.Users
            .Where(u => u.LastLoginDate != null && u.LastLoginDate > recentThreshold)
            .OrderByDescending(u => u.LastLoginDate)
            .Take(50)
            .Select(u => new
            {
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email,
                u.LastLoginDate,
                u.Role,
                u.IsActive
            })
            .ToListAsync(ct);

        return users.Select(u => new UserSession
        {
            UserId = u.Id.ToString(),
            UserName = $"{u.FirstName} {u.LastName}",
            Email = u.Email,
            Role = u.Role.ToString(),
            LoginTime = u.LastLoginDate ?? DateTime.UtcNow,
            LastActivity = u.LastLoginDate ?? DateTime.UtcNow,
            IpAddress = "N/A",
            IsActive = u.IsActive
        }).ToList();
    }

    #endregion
}
