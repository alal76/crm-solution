// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
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
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// Comprehensive monitoring controller providing infrastructure, system, and database metrics.
/// Automatically detects deployment environment (Docker, Kubernetes, VM) and adapts monitoring.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MonitoringController : CrmControllerBase
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

    /// <summary>
    /// Get detailed monitor status from Uptime Kuma
    /// </summary>
    [HttpGet("uptime-kuma/monitors")]
    [AllowAnonymous]
    public async Task<ActionResult> GetUptimeKumaMonitors(CancellationToken ct)
    {
        try
        {
            var uptimeKumaHost = _configuration.GetValue<string>("Monitoring:UptimeKumaHost", "uptime-kuma");
            var uptimeKumaPort = _configuration.GetValue<int>("Monitoring:UptimeKumaPort", 3001);
            var statusPageSlug = _configuration.GetValue<string>("Monitoring:StatusPageSlug", "crm-status");

            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);

            // Get heartbeat data from status page (public endpoint)
            var heartbeatUrl = $"http://{uptimeKumaHost}:{uptimeKumaPort}/api/status-page/heartbeat/{statusPageSlug}";

            try
            {
                var response = await client.GetAsync(heartbeatUrl, ct);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(ct);
                    var data = JsonSerializer.Deserialize<JsonElement>(content);

                    var monitors = new List<object>();

                    // Parse heartbeatList from response
                    if (data.TryGetProperty("heartbeatList", out var heartbeatList))
                    {
                        foreach (var monitorProp in heartbeatList.EnumerateObject())
                        {
                            var monitorId = monitorProp.Name;
                            var heartbeats = monitorProp.Value;

                            // Get latest heartbeat
                            if (heartbeats.GetArrayLength() > 0)
                            {
                                var latest = heartbeats[heartbeats.GetArrayLength() - 1];
                                monitors.Add(new
                                {
                                    id = monitorId,
                                    status = latest.TryGetProperty("status", out var s) ? s.GetInt32() : 0,
                                    ping = latest.TryGetProperty("ping", out var p) ? p.GetInt32() : 0,
                                    time = latest.TryGetProperty("time", out var t) ? t.GetString() : null,
                                    msg = latest.TryGetProperty("msg", out var m) ? m.GetString() : null,
                                });
                            }
                        }
                    }

                    // Get uptime data
                    var uptimeData = new Dictionary<string, object>();
                    if (data.TryGetProperty("uptimeList", out var uptimeList))
                    {
                        foreach (var prop in uptimeList.EnumerateObject())
                        {
                            uptimeData[prop.Name] = prop.Value.GetDouble();
                        }
                    }

                    return Ok(new
                    {
                        connected = true,
                        monitors = monitors,
                        uptimeList = uptimeData,
                        monitorCount = monitors.Count,
                        timestamp = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Could not fetch Uptime Kuma monitors");
            }

            return Ok(new
            {
                connected = false,
                monitors = Array.Empty<object>(),
                monitorCount = 0,
                message = "Status page not accessible",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return Ok(new
            {
                connected = false,
                error = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Get container status from Portainer
    /// </summary>
    [HttpGet("portainer/containers")]
    [AllowAnonymous]
    public async Task<ActionResult> GetPortainerContainers(CancellationToken ct)
    {
        try
        {
            var portainerHost = _configuration.GetValue<string>("Monitoring:PortainerHost", "portainer");
            var portainerPort = _configuration.GetValue<int>("Monitoring:PortainerPort", 9000);

            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(5);

            // Get basic Portainer status (version info is public)
            var response = await client.GetAsync($"http://{portainerHost}:{portainerPort}/api/status", ct);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(ct);
                var statusData = JsonSerializer.Deserialize<JsonElement>(content);

                return Ok(new
                {
                    connected = true,
                    version = statusData.TryGetProperty("Version", out var v) ? v.GetString() : "unknown",
                    instanceId = statusData.TryGetProperty("InstanceID", out var i) ? i.GetString() : null,
                    message = "Portainer is running. Login to view containers.",
                    timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                connected = false,
                message = "Portainer not accessible",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return Ok(new
            {
                connected = false,
                error = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
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
                {
                    version = v.GetString();
                }
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
                _logger.LogInformation("Fetching all monitoring data");
        var data = await _monitoringService.GetAllMonitoringDataAsync(ct);

        // Add active sessions from database
        data.ActiveSessions = await GetActiveSessionsFromDb(ct);

        return Ok(data);
    }

    /// <summary>
    /// Get infrastructure detection information
    /// </summary>
    [HttpGet("infrastructure")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<InfrastructureInfo>> GetInfrastructure(CancellationToken ct)
    {
                var info = await _monitoringService.GetInfrastructureInfoAsync(ct);
        return Ok(info);
    }

    /// <summary>
    /// Get system metrics (CPU, memory, disk, network)
    /// </summary>
    [HttpGet("system")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SystemMetrics>> GetSystemMetrics(CancellationToken ct)
    {
                var metrics = await _monitoringService.GetSystemMetricsAsync(ct);
        return Ok(metrics);
    }

    /// <summary>
    /// Get database-specific metrics
    /// </summary>
    [HttpGet("database")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<DatabaseMetrics>> GetDatabaseMetrics(CancellationToken ct)
    {
                var metrics = await _monitoringService.GetDatabaseMetricsAsync(ct);
        return Ok(metrics);
    }

    /// <summary>
    /// Get health status of all services
    /// </summary>
    [HttpGet("services")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<ServiceHealth>>> GetServices(CancellationToken ct)
    {
                var services = await _monitoringService.GetServiceHealthAsync(ct);
        return Ok(services);
    }

    /// <summary>
    /// Get Docker container health (if Docker monitoring is enabled)
    /// </summary>
    [HttpGet("containers")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<ContainerHealth>>> GetContainers(CancellationToken ct)
    {
                var containers = await _monitoringService.GetContainerHealthAsync(ct);
        return Ok(containers);
    }

    /// <summary>
    /// Get Kubernetes pod health (if Kubernetes monitoring is enabled)
    /// </summary>
    [HttpGet("pods")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<PodHealth>>> GetPods(CancellationToken ct)
    {
                var pods = await _monitoringService.GetPodHealthAsync(ct);
        return Ok(pods);
    }

    /// <summary>
    /// Get active user sessions
    /// </summary>
    [HttpGet("sessions")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<MonitoringUserSession>>> GetSessions(CancellationToken ct)
    {
                var sessions = await GetActiveSessionsFromDb(ct);
        return Ok(sessions);
    }

    /// <summary>
    /// Get monitoring options/configuration
    /// </summary>
    [HttpGet("config")]
    [Authorize(Roles = "Admin")]
    public ActionResult<MonitoringOptions> GetConfig()
    {
                var options = _monitoringService.GetMonitoringOptions();
        return Ok(options);
    }

    /// <summary>
    /// Detailed health check with database connectivity and full diagnostics
    /// </summary>
    [HttpGet("health/detailed")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> GetDetailedHealth(CancellationToken ct)
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

    #endregion

    #region Private Methods

    private async Task<List<MonitoringUserSession>> GetActiveSessionsFromDb(CancellationToken ct)
    {
        var recentThreshold = DateTime.UtcNow.AddHours(-24);

        try
        {
            var users = await _context.Users
                .Where(u => u.LastLoginAt != null && u.LastLoginAt > recentThreshold)
                .OrderByDescending(u => u.LastLoginAt)
                .Take(50)
                .Select(u => new
                {
                    u.Id,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.LastLoginAt,
                    u.Role,
                    u.IsActive
                })
                .ToListAsync(ct);

            return users.Select(u => new MonitoringUserSession
            {
                UserId = u.Id.ToString(),
                UserName = $"{u.FirstName} {u.LastName}",
                Email = u.Email,
                Role = u.Role.ToString(),
                LoginTime = u.LastLoginAt ?? DateTime.UtcNow,
                LastActivity = u.LastLoginAt ?? DateTime.UtcNow,
                IpAddress = "N/A",
                IsActive = u.IsActive
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get active sessions from database");
            return new List<MonitoringUserSession>();
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
