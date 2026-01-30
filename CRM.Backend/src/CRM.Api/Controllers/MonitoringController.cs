/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * Comprehensive Monitoring Controller - Environment & Infrastructure Aware
 */

using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
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
/// Comprehensive monitoring controller providing infrastructure, system, and database metrics.
/// Automatically detects deployment environment (Docker, Kubernetes, VM) and adapts monitoring.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MonitoringController : ControllerBase
{
    private readonly IMonitoringService _monitoringService;
    private readonly CrmDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MonitoringController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public MonitoringController(
        IMonitoringService monitoringService,
        CrmDbContext context,
        IConfiguration configuration,
        ILogger<MonitoringController> logger,
        IHttpClientFactory httpClientFactory)
    {
        _monitoringService = monitoringService;
        _context = context;
        _configuration = configuration;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    #region Public Health Endpoints (No Authentication Required)

    /// <summary>
    /// Basic health check endpoint - for load balancers and container orchestrators
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(HealthResponse), 200)]
    public ActionResult GetHealth()
    {
        return Ok(new HealthResponse
        {
            Status = "healthy",
            Timestamp = DateTime.UtcNow,
            Service = "CRM API",
            Version = GetAssemblyVersion()
        });
    }

    /// <summary>
    /// Liveness probe - for Kubernetes liveness checks
    /// </summary>
    [HttpGet("health/live")]
    [AllowAnonymous]
    public ActionResult GetLiveness()
    {
        return Ok(new { status = "alive", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Readiness probe - checks if service is ready to accept traffic
    /// </summary>
    [HttpGet("health/ready")]
    [AllowAnonymous]
    public async Task<ActionResult> GetReadiness(CancellationToken ct)
    {
        try
        {
            var dbReady = await _context.Database.CanConnectAsync(ct);
            
            if (dbReady)
            {
                return Ok(new { status = "ready", database = "connected", timestamp = DateTime.UtcNow });
            }
            
            return StatusCode(503, new { status = "not-ready", database = "disconnected", timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Readiness check failed");
            return StatusCode(503, new { status = "not-ready", error = ex.Message, timestamp = DateTime.UtcNow });
        }
    }

    /// <summary>
    /// Environment info - public endpoint showing deployment context
    /// </summary>
    [HttpGet("environment")]
    [AllowAnonymous]
    public async Task<ActionResult<EnvironmentInfo>> GetEnvironment(CancellationToken ct)
    {
        try
        {
            var options = _monitoringService.GetMonitoringOptions();
            var infra = await _monitoringService.GetInfrastructureInfoAsync(ct);
            
            return Ok(new EnvironmentInfo
            {
                DeploymentType = infra.DeploymentTypeName,
                IsDocker = infra.Host.IsDocker,
                IsKubernetes = infra.Host.IsKubernetes,
                DatabaseProvider = infra.Database.ProviderName,
                DatabaseConnected = infra.Database.IsConnected,
                Hostname = infra.Host.Hostname,
                Version = GetAssemblyVersion(),
                DotNetVersion = infra.Host.DotNetVersion,
                EnabledMonitors = infra.ActiveMonitors,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting environment info");
            return StatusCode(500, new { message = "Error getting environment info", error = ex.Message });
        }
    }

    /// <summary>
    /// Get Uptime Kuma status - proxied from Uptime Kuma API
    /// </summary>
    [HttpGet("uptime-kuma/status")]
    [AllowAnonymous]
    public async Task<ActionResult> GetUptimeKumaStatus(CancellationToken ct)
    {
        try
        {
            var uptimeKumaHost = _configuration.GetValue<string>("Monitoring:UptimeKumaHost", "uptime-kuma");
            var uptimeKumaPort = _configuration.GetValue<int>("Monitoring:UptimeKumaPort", 3001);
            var statusPageSlug = _configuration.GetValue<string>("Monitoring:StatusPageSlug", "crm-status");
            
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            
            // Try to get heartbeat data from status page
            var heartbeatUrl = $"http://{uptimeKumaHost}:{uptimeKumaPort}/api/status-page/heartbeat/{statusPageSlug}";
            
            try
            {
                var response = await client.GetAsync(heartbeatUrl, ct);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(ct);
                    var heartbeatData = JsonSerializer.Deserialize<JsonElement>(content);
                    
                    return Ok(new
                    {
                        status = "connected",
                        source = "status-page",
                        data = heartbeatData,
                        timestamp = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Could not fetch status page heartbeat");
            }
            
            // Fallback: just check if Uptime Kuma is reachable
            try
            {
                var pingUrl = $"http://{uptimeKumaHost}:{uptimeKumaPort}/";
                var pingResponse = await client.GetAsync(pingUrl, ct);
                
                return Ok(new
                {
                    status = pingResponse.IsSuccessStatusCode ? "online" : "degraded",
                    source = "ping",
                    httpStatus = (int)pingResponse.StatusCode,
                    message = pingResponse.IsSuccessStatusCode ? "Uptime Kuma is running" : "Uptime Kuma returned error",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    status = "offline",
                    source = "ping",
                    message = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Uptime Kuma status");
            return Ok(new
            {
                status = "error",
                message = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Get Portainer status - proxied from Portainer API
    /// </summary>
    [HttpGet("portainer/status")]
    [AllowAnonymous]
    public async Task<ActionResult> GetPortainerStatus(CancellationToken ct)
    {
        try
        {
            var portainerHost = _configuration.GetValue<string>("Monitoring:PortainerHost", "portainer");
            var portainerPort = _configuration.GetValue<int>("Monitoring:PortainerPort", 9000);
            
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            
            // Get Portainer version/status (public endpoint)
            var statusUrl = $"http://{portainerHost}:{portainerPort}/api/status";
            
            try
            {
                var response = await client.GetAsync(statusUrl, ct);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(ct);
                    var statusData = JsonSerializer.Deserialize<JsonElement>(content);
                    
                    return Ok(new
                    {
                        status = "connected",
                        version = statusData.TryGetProperty("Version", out var version) ? version.GetString() : "unknown",
                        instanceId = statusData.TryGetProperty("InstanceID", out var instanceId) ? instanceId.GetString() : null,
                        timestamp = DateTime.UtcNow
                    });
                }
                
                return Ok(new
                {
                    status = "degraded",
                    httpStatus = (int)response.StatusCode,
                    message = "Portainer returned error",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (HttpRequestException ex)
            {
                return Ok(new
                {
                    status = "offline",
                    message = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Portainer status");
            return Ok(new
            {
                status = "error",
                message = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Get combined external monitoring tools status
    /// </summary>
    [HttpGet("tools/status")]
    [AllowAnonymous]
    public async Task<ActionResult> GetMonitoringToolsStatus(CancellationToken ct)
    {
        var uptimeKumaTask = GetUptimeKumaStatusInternal(ct);
        var portainerTask = GetPortainerStatusInternal(ct);
        
        await Task.WhenAll(uptimeKumaTask, portainerTask);
        
        return Ok(new
        {
            uptimeKuma = await uptimeKumaTask,
            portainer = await portainerTask,
            timestamp = DateTime.UtcNow
        });
    }

    private async Task<object> GetUptimeKumaStatusInternal(CancellationToken ct)
    {
        try
        {
            var uptimeKumaHost = _configuration.GetValue<string>("Monitoring:UptimeKumaHost", "uptime-kuma");
            var uptimeKumaPort = _configuration.GetValue<int>("Monitoring:UptimeKumaPort", 3001);
            
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            
            var response = await client.GetAsync($"http://{uptimeKumaHost}:{uptimeKumaPort}/", ct);
            
            return new
            {
                status = response.IsSuccessStatusCode ? "online" : "degraded",
                url = $"http://localhost:{uptimeKumaPort}",
                port = uptimeKumaPort
            };
        }
        catch
        {
            return new { status = "offline", url = "http://localhost:3001", port = 3001 };
        }
    }

    private async Task<object> GetPortainerStatusInternal(CancellationToken ct)
    {
        try
        {
            var portainerHost = _configuration.GetValue<string>("Monitoring:PortainerHost", "portainer");
            var portainerPort = _configuration.GetValue<int>("Monitoring:PortainerPort", 9000);
            
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            
            var response = await client.GetAsync($"http://{portainerHost}:{portainerPort}/api/status", ct);
            string? version = null;
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(ct);
                var data = JsonSerializer.Deserialize<JsonElement>(content);
                if (data.TryGetProperty("Version", out var v))
                    version = v.GetString();
            }
            
            return new
            {
                status = response.IsSuccessStatusCode ? "online" : "degraded",
                version = version,
                url = $"http://localhost:{portainerPort}",
                port = portainerPort
            };
        }
        catch
        {
            return new { status = "offline", url = "http://localhost:9000", port = 9000, version = (string?)null };
        }
    }

    #endregion

    #region Authenticated Monitoring Endpoints

    /// <summary>
    /// Get all monitoring data in a single call - primary endpoint for dashboard
    /// </summary>
    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<MonitoringData>> GetAllMonitoringData(CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Fetching all monitoring data");
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
    [Authorize(Roles = "Admin")]
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
    [Authorize(Roles = "Admin")]
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
    [Authorize(Roles = "Admin")]
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
    [Authorize(Roles = "Admin")]
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
    [Authorize(Roles = "Admin")]
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
    [Authorize(Roles = "Admin")]
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
    [Authorize(Roles = "Admin")]
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
    [Authorize(Roles = "Admin")]
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
    /// Detailed health check with database connectivity and full diagnostics
    /// </summary>
    [HttpGet("health/detailed")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> GetDetailedHealth(CancellationToken ct)
    {
        try
        {
            var dbHealthy = await _context.Database.CanConnectAsync(ct);
            var options = _monitoringService.GetMonitoringOptions();
            var system = await _monitoringService.GetSystemMetricsAsync(ct);
            
            return Ok(new
            {
                status = dbHealthy ? "healthy" : "degraded",
                timestamp = DateTime.UtcNow,
                version = GetAssemblyVersion(),
                database = new
                {
                    provider = options.DatabaseProvider,
                    connected = dbHealthy
                },
                deployment = new
                {
                    type = options.DeploymentType,
                    server = options.BuildServer
                },
                system = new
                {
                    cpuPercent = system.Cpu.UsagePercent,
                    memoryMB = system.Memory.ProcessWorkingSetMB,
                    uptime = system.Process.UptimeFormatted
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during detailed health check");
            return StatusCode(500, new { status = "error", message = ex.Message });
        }
    }

    #endregion

    #region Private Methods

    private async Task<List<UserSession>> GetActiveSessionsFromDb(CancellationToken ct)
    {
        var recentThreshold = DateTime.UtcNow.AddHours(-24);
        
        try
        {
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
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get active sessions from database");
            return new List<UserSession>();
        }
    }

    private static string GetAssemblyVersion()
    {
        return typeof(MonitoringController).Assembly.GetName().Version?.ToString() ?? "1.0.0";
    }

    #endregion
}

#region Response DTOs

public class HealthResponse
{
    public string Status { get; set; } = "healthy";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Service { get; set; } = "";
    public string Version { get; set; } = "";
}

public class EnvironmentInfo
{
    public string DeploymentType { get; set; } = "";
    public bool IsDocker { get; set; }
    public bool IsKubernetes { get; set; }
    public string DatabaseProvider { get; set; } = "";
    public bool DatabaseConnected { get; set; }
    public string Hostname { get; set; } = "";
    public string Version { get; set; } = "";
    public string DotNetVersion { get; set; } = "";
    public List<string> EnabledMonitors { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

#endregion
