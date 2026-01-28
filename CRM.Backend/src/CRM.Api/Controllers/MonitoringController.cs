using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CRM.Api.Controllers;

/// <summary>
/// System monitoring and metrics endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class MonitoringController : ControllerBase
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<MonitoringController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IMonitoringService _monitoringService;
    private static readonly DateTime _startTime = DateTime.UtcNow;

    public MonitoringController(
        ICrmDbContext context, 
        ILogger<MonitoringController> logger,
        IConfiguration configuration,
        IMonitoringService monitoringService)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
        _monitoringService = monitoringService;
    }

    /// <summary>
    /// Get comprehensive system metrics
    /// </summary>
    [HttpGet("system")]
    public async Task<ActionResult<SystemMetricsDto>> GetSystemMetrics()
    {
        try
        {
            var process = Process.GetCurrentProcess();
            var uptime = DateTime.UtcNow - _startTime;
            
            // Get memory info
            var workingSet = process.WorkingSet64;
            var privateMemory = process.PrivateMemorySize64;
            var gcMemory = GC.GetTotalMemory(false);
            
            // Get CPU info (approximate)
            var cpuTime = process.TotalProcessorTime;
            
            // Get thread info
            var threadCount = process.Threads.Count;
            
            // Get database connection info
            var dbConnections = await GetDatabaseConnectionsAsync();
            
            // Get database size
            var dbSize = await GetDatabaseSizeAsync();
            
            var metrics = new SystemMetricsDto
            {
                Timestamp = DateTime.UtcNow,
                Uptime = FormatUptime(uptime),
                UptimeSeconds = (long)uptime.TotalSeconds,
                
                // Process metrics
                Process = new ProcessMetricsDto
                {
                    CpuTimeMs = (long)cpuTime.TotalMilliseconds,
                    WorkingSetMB = workingSet / (1024 * 1024),
                    PrivateMemoryMB = privateMemory / (1024 * 1024),
                    GCMemoryMB = gcMemory / (1024 * 1024),
                    ThreadCount = threadCount,
                    HandleCount = process.HandleCount,
                },
                
                // System info
                System = new SystemInfoDto
                {
                    MachineName = Environment.MachineName,
                    OSDescription = RuntimeInformation.OSDescription,
                    ProcessorCount = Environment.ProcessorCount,
                    Is64Bit = Environment.Is64BitOperatingSystem,
                    DotNetVersion = RuntimeInformation.FrameworkDescription,
                },
                
                // Database metrics
                Database = new DatabaseMetricsDto
                {
                    Provider = GetDatabaseProviderName(),
                    ConnectionStatus = "Connected",
                    ActiveConnections = dbConnections,
                    DatabaseSizeMB = dbSize,
                },
                
                // GC metrics
                GarbageCollection = new GCMetricsDto
                {
                    Gen0Collections = GC.CollectionCount(0),
                    Gen1Collections = GC.CollectionCount(1),
                    Gen2Collections = GC.CollectionCount(2),
                    TotalMemoryMB = GC.GetTotalMemory(false) / (1024 * 1024),
                },
            };

            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system metrics");
            return StatusCode(500, new { message = "Error getting system metrics" });
        }
    }

    /// <summary>
    /// Get service health status
    /// </summary>
    [HttpGet("services")]
    public async Task<ActionResult<List<ServiceStatusDto>>> GetServiceStatus()
    {
        try
        {
            var services = new List<ServiceStatusDto>();
            
            // Check API service
            var apiStartTime = DateTime.UtcNow - (DateTime.UtcNow - _startTime);
            services.Add(new ServiceStatusDto
            {
                Name = "CRM API",
                Type = "api",
                Status = "healthy",
                Endpoint = $"{Request.Scheme}://{Request.Host}/api",
                Uptime = CalculateUptime(_startTime),
                LastCheck = DateTime.UtcNow.ToString("o"),
                ResponseTime = 0, // Self
                Version = GetAssemblyVersion(),
            });

            // Check Database
            var dbStatus = await CheckDatabaseHealthAsync();
            services.Add(new ServiceStatusDto
            {
                Name = $"{GetDatabaseProviderName()} Database",
                Type = "database",
                Status = dbStatus.IsHealthy ? "healthy" : "error",
                Endpoint = GetDatabaseHost(),
                Uptime = dbStatus.Uptime,
                LastCheck = DateTime.UtcNow.ToString("o"),
                ResponseTime = dbStatus.ResponseTimeMs,
                Version = dbStatus.Version,
            });

            // Check Frontend (if configured)
            var frontendUrl = _configuration["Frontend:Url"] ?? "http://localhost:3000";
            var frontendStatus = await CheckFrontendHealthAsync(frontendUrl);
            services.Add(new ServiceStatusDto
            {
                Name = "CRM Frontend",
                Type = "frontend",
                Status = frontendStatus.IsHealthy ? "healthy" : "degraded",
                Endpoint = frontendUrl,
                Uptime = "N/A",
                LastCheck = DateTime.UtcNow.ToString("o"),
                ResponseTime = frontendStatus.ResponseTimeMs,
                Version = "1.0.0",
            });

            return Ok(services);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service status");
            return StatusCode(500, new { message = "Error getting service status" });
        }
    }

    /// <summary>
    /// Get server load metrics
    /// </summary>
    [HttpGet("load")]
    public async Task<ActionResult<ServerLoadDto>> GetServerLoad()
    {
        try
        {
            var process = Process.GetCurrentProcess();
            
            // Calculate approximate CPU usage
            var startCpuTime = process.TotalProcessorTime;
            await Task.Delay(100);
            process.Refresh();
            var endCpuTime = process.TotalProcessorTime;
            var cpuUsedMs = (endCpuTime - startCpuTime).TotalMilliseconds;
            var cpuPercentage = (cpuUsedMs / (100.0 * Environment.ProcessorCount)) * 100;
            
            // Get memory info
            var workingSet = process.WorkingSet64;
            var totalMemory = GetTotalPhysicalMemory();
            var memoryPercentage = totalMemory > 0 ? (workingSet * 100.0 / totalMemory) : 0;
            
            // Get database stats
            var dbStats = await GetDatabaseStatsAsync();
            
            var loads = new ServerLoadDto
            {
                Timestamp = DateTime.UtcNow,
                Services = new List<ServiceLoadDto>
                {
                    new ServiceLoadDto
                    {
                        Service = "API Server",
                        CpuLoad = Math.Min(100, Math.Max(0, cpuPercentage)),
                        MemoryLoad = Math.Min(100, Math.Max(0, memoryPercentage)),
                        Status = cpuPercentage < 50 ? "green" : cpuPercentage < 75 ? "amber" : "red",
                    },
                    new ServiceLoadDto
                    {
                        Service = "Database Server",
                        CpuLoad = dbStats.CpuLoad,
                        MemoryLoad = dbStats.MemoryLoad,
                        Status = dbStats.CpuLoad < 50 ? "green" : dbStats.CpuLoad < 75 ? "amber" : "red",
                    },
                }
            };

            return Ok(loads);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting server load");
            return StatusCode(500, new { message = "Error getting server load" });
        }
    }

    /// <summary>
    /// Get logged in users / active sessions
    /// </summary>
    [HttpGet("sessions")]
    public async Task<ActionResult<List<ActiveSessionDto>>> GetActiveSessions()
    {
        try
        {
            // Get users with recent activity from the database
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
                    u.IsActive,
                })
                .ToListAsync();

            var activeSessions = users.Select(u => new ActiveSessionDto
            {
                UserId = u.Id.ToString(),
                Name = $"{u.FirstName} {u.LastName}",
                Email = u.Email,
                LoginTime = u.LastLoginDate.HasValue ? u.LastLoginDate.Value.ToString("o") : "",
                LastActivity = u.LastLoginDate.HasValue ? u.LastLoginDate.Value.ToString("o") : "",
                IpAddress = "N/A", // Would need to track this separately
                Role = u.Role.ToString(),
                IsActive = u.IsActive,
            }).ToList();

            return Ok(activeSessions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active sessions");
            return StatusCode(500, new { message = "Error getting active sessions" });
        }
    }

    /// <summary>
    /// Get comprehensive monitoring data in one call
    /// </summary>
    [HttpGet("all")]
    public async Task<ActionResult<MonitoringDataDto>> GetAllMonitoringData()
    {
        try
        {
            var systemMetrics = await GetSystemMetricsInternal();
            var services = await GetServicesInternal();
            var load = await GetServerLoadInternal();
            var sessions = await GetActiveSessionsInternal();

            var data = new MonitoringDataDto
            {
                Timestamp = DateTime.UtcNow,
                SystemMetrics = systemMetrics,
                Services = services,
                ServerLoad = load,
                ActiveSessions = sessions,
            };

            return Ok(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting monitoring data");
            return StatusCode(500, new { message = "Error getting monitoring data" });
        }
    }

    /// <summary>
    /// Get comprehensive health summary including all endpoints, containers, and pods
    /// </summary>
    [HttpGet("health-summary")]
    public async Task<ActionResult<SystemHealthSummary>> GetHealthSummary()
    {
        try
        {
            var summary = await _monitoringService.GetSystemHealthAsync();
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting health summary");
            return StatusCode(500, new { message = "Error getting health summary" });
        }
    }

    /// <summary>
    /// Discover all monitored endpoints from configuration
    /// </summary>
    [HttpGet("endpoints")]
    public async Task<ActionResult<List<MonitoredEndpoint>>> GetEndpoints()
    {
        try
        {
            var endpoints = await _monitoringService.DiscoverEndpointsAsync();
            return Ok(endpoints);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error discovering endpoints");
            return StatusCode(500, new { message = "Error discovering endpoints" });
        }
    }

    /// <summary>
    /// Check health of all discovered endpoints
    /// </summary>
    [HttpGet("endpoints/health")]
    public async Task<ActionResult<List<MonitoredEndpoint>>> GetEndpointHealth()
    {
        try
        {
            var endpoints = await _monitoringService.CheckEndpointHealthAsync();
            return Ok(endpoints);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking endpoint health");
            return StatusCode(500, new { message = "Error checking endpoint health" });
        }
    }

    /// <summary>
    /// Get Docker container health status
    /// </summary>
    [HttpGet("containers")]
    public async Task<ActionResult<List<ContainerHealth>>> GetContainerHealth()
    {
        try
        {
            var containers = await _monitoringService.GetContainerHealthAsync();
            return Ok(containers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting container health");
            return StatusCode(500, new { message = "Error getting container health" });
        }
    }

    /// <summary>
    /// Get Kubernetes pod health status
    /// </summary>
    [HttpGet("pods")]
    public async Task<ActionResult<List<PodHealth>>> GetPodHealth()
    {
        try
        {
            var pods = await _monitoringService.GetPodHealthAsync();
            return Ok(pods);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pod health");
            return StatusCode(500, new { message = "Error getting pod health" });
        }
    }

    /// <summary>
    /// Get API metrics for a specific endpoint
    /// </summary>
    [HttpGet("api-metrics")]
    public async Task<ActionResult<Dictionary<string, object>>> GetApiMetrics([FromQuery] string endpoint)
    {
        try
        {
            if (string.IsNullOrEmpty(endpoint))
            {
                return BadRequest(new { message = "Endpoint parameter is required" });
            }
            
            var metrics = await _monitoringService.GetApiMetricsAsync(endpoint);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting API metrics for {Endpoint}", endpoint);
            return StatusCode(500, new { message = "Error getting API metrics" });
        }
    }

    /// <summary>
    /// Get deployment configuration settings (cached from environment)
    /// </summary>
    [HttpGet("deployment-settings")]
    public ActionResult<object> GetDeploymentSettings()
    {
        try
        {
            var options = _monitoringService.GetMonitoringOptions();
            return Ok(new
            {
                DeploymentType = options.DeploymentType,
                BuildServer = options.BuildServer,
                BuildServerFQDN = options.BuildServerFQDN,
                DatabaseProvider = options.DatabaseProvider,
                KubernetesNamespace = options.KubernetesNamespace,
                EnableK8sMonitoring = options.EnableK8sMonitoring,
                EnableDockerMonitoring = options.EnableDockerMonitoring,
                HealthCheckTimeoutSeconds = options.HealthCheckTimeoutSeconds,
                CacheDurationSeconds = options.CacheDurationSeconds,
                SqlServer = new
                {
                    Version = options.MssqlVersion,
                    Edition = options.MssqlEdition,
                    ToolsPath = options.MssqlToolsPath,
                    HealthCheckInterval = options.MssqlHealthInterval,
                    HealthCheckTimeout = options.MssqlHealthTimeout,
                    HealthCheckRetries = options.MssqlHealthRetries,
                    StartPeriod = options.MssqlStartPeriod,
                    Collation = options.MssqlCollation
                },
                ConfiguredEndpoints = new
                {
                    ApiEndpoints = options.ApiEndpoints,
                    DatabaseServers = options.DatabaseServers,
                    FrontendUrls = options.FrontendUrls,
                    RedisEndpoints = options.RedisEndpoints
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting deployment settings");
            return StatusCode(500, new { message = "Error getting deployment settings" });
        }
    }

    #region Private Methods

    private async Task<SystemMetricsDto> GetSystemMetricsInternal()
    {
        var process = Process.GetCurrentProcess();
        var uptime = DateTime.UtcNow - _startTime;
        
        var workingSet = process.WorkingSet64;
        var privateMemory = process.PrivateMemorySize64;
        var gcMemory = GC.GetTotalMemory(false);
        var cpuTime = process.TotalProcessorTime;
        var threadCount = process.Threads.Count;
        var dbConnections = await GetDatabaseConnectionsAsync();
        var dbSize = await GetDatabaseSizeAsync();
        
        return new SystemMetricsDto
        {
            Timestamp = DateTime.UtcNow,
            Uptime = FormatUptime(uptime),
            UptimeSeconds = (long)uptime.TotalSeconds,
            Process = new ProcessMetricsDto
            {
                CpuTimeMs = (long)cpuTime.TotalMilliseconds,
                WorkingSetMB = workingSet / (1024 * 1024),
                PrivateMemoryMB = privateMemory / (1024 * 1024),
                GCMemoryMB = gcMemory / (1024 * 1024),
                ThreadCount = threadCount,
                HandleCount = process.HandleCount,
            },
            System = new SystemInfoDto
            {
                MachineName = Environment.MachineName,
                OSDescription = RuntimeInformation.OSDescription,
                ProcessorCount = Environment.ProcessorCount,
                Is64Bit = Environment.Is64BitOperatingSystem,
                DotNetVersion = RuntimeInformation.FrameworkDescription,
            },
            Database = new DatabaseMetricsDto
            {
                Provider = GetDatabaseProviderName(),
                ConnectionStatus = "Connected",
                ActiveConnections = dbConnections,
                DatabaseSizeMB = dbSize,
            },
            GarbageCollection = new GCMetricsDto
            {
                Gen0Collections = GC.CollectionCount(0),
                Gen1Collections = GC.CollectionCount(1),
                Gen2Collections = GC.CollectionCount(2),
                TotalMemoryMB = GC.GetTotalMemory(false) / (1024 * 1024),
            },
        };
    }

    private async Task<List<ServiceStatusDto>> GetServicesInternal()
    {
        var services = new List<ServiceStatusDto>();
        
        services.Add(new ServiceStatusDto
        {
            Name = "CRM API",
            Type = "api",
            Status = "healthy",
            Endpoint = $"{Request.Scheme}://{Request.Host}/api",
            Uptime = CalculateUptime(_startTime),
            LastCheck = DateTime.UtcNow.ToString("o"),
            ResponseTime = 0,
            Version = GetAssemblyVersion(),
        });

        var dbStatus = await CheckDatabaseHealthAsync();
        services.Add(new ServiceStatusDto
        {
            Name = $"{GetDatabaseProviderName()} Database",
            Type = "database",
            Status = dbStatus.IsHealthy ? "healthy" : "error",
            Endpoint = GetDatabaseHost(),
            Uptime = dbStatus.Uptime,
            LastCheck = DateTime.UtcNow.ToString("o"),
            ResponseTime = dbStatus.ResponseTimeMs,
            Version = dbStatus.Version,
        });

        var frontendUrl = _configuration["Frontend:Url"] ?? "http://localhost:3000";
        var frontendStatus = await CheckFrontendHealthAsync(frontendUrl);
        services.Add(new ServiceStatusDto
        {
            Name = "CRM Frontend",
            Type = "frontend",
            Status = frontendStatus.IsHealthy ? "healthy" : "degraded",
            Endpoint = frontendUrl,
            Uptime = "N/A",
            LastCheck = DateTime.UtcNow.ToString("o"),
            ResponseTime = frontendStatus.ResponseTimeMs,
            Version = "1.0.0",
        });

        return services;
    }

    private async Task<ServerLoadDto> GetServerLoadInternal()
    {
        var process = Process.GetCurrentProcess();
        
        var startCpuTime = process.TotalProcessorTime;
        await Task.Delay(50);
        process.Refresh();
        var endCpuTime = process.TotalProcessorTime;
        var cpuUsedMs = (endCpuTime - startCpuTime).TotalMilliseconds;
        var cpuPercentage = (cpuUsedMs / (50.0 * Environment.ProcessorCount)) * 100;
        
        var workingSet = process.WorkingSet64;
        var totalMemory = GetTotalPhysicalMemory();
        var memoryPercentage = totalMemory > 0 ? (workingSet * 100.0 / totalMemory) : 0;
        
        var dbStats = await GetDatabaseStatsAsync();
        
        return new ServerLoadDto
        {
            Timestamp = DateTime.UtcNow,
            Services = new List<ServiceLoadDto>
            {
                new ServiceLoadDto
                {
                    Service = "API Server",
                    CpuLoad = Math.Min(100, Math.Max(0, cpuPercentage)),
                    MemoryLoad = Math.Min(100, Math.Max(0, memoryPercentage)),
                    Status = cpuPercentage < 50 ? "green" : cpuPercentage < 75 ? "amber" : "red",
                },
                new ServiceLoadDto
                {
                    Service = "Database Server",
                    CpuLoad = dbStats.CpuLoad,
                    MemoryLoad = dbStats.MemoryLoad,
                    Status = dbStats.CpuLoad < 50 ? "green" : dbStats.CpuLoad < 75 ? "amber" : "red",
                },
            }
        };
    }

    private async Task<List<ActiveSessionDto>> GetActiveSessionsInternal()
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
                u.IsActive,
            })
            .ToListAsync();

        return users.Select(u => new ActiveSessionDto
        {
            UserId = u.Id.ToString(),
            Name = $"{u.FirstName} {u.LastName}",
            Email = u.Email,
            LoginTime = u.LastLoginDate.HasValue ? u.LastLoginDate.Value.ToString("o") : "",
            LastActivity = u.LastLoginDate.HasValue ? u.LastLoginDate.Value.ToString("o") : "",
            IpAddress = "N/A",
            Role = u.Role.ToString(),
            IsActive = u.IsActive,
        }).ToList();
    }

    private async Task<int> GetDatabaseConnectionsAsync()
    {
        try
        {
            var connection = _context.Database.GetDbConnection();
            await using var cmd = connection.CreateCommand();
            
            if (IsSqlServer())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM sys.dm_exec_sessions WHERE is_user_process = 1";
            }
            else
            {
                cmd.CommandText = "SHOW STATUS LIKE 'Threads_connected'";
            }
            
            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync();
            
            if (IsSqlServer())
            {
                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
            else
            {
                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return int.TryParse(reader.GetString(1), out var count) ? count : 0;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get database connections");
        }
        return 0;
    }

    private async Task<double> GetDatabaseSizeAsync()
    {
        try
        {
            var connection = _context.Database.GetDbConnection();
            await using var cmd = connection.CreateCommand();
            
            if (IsSqlServer())
            {
                cmd.CommandText = @"
                    SELECT CAST(SUM(size * 8.0 / 1024) AS DECIMAL(18,2)) AS size_mb
                    FROM sys.master_files
                    WHERE database_id = DB_ID()";
            }
            else
            {
                cmd.CommandText = @"
                    SELECT ROUND(SUM(data_length + index_length) / 1024 / 1024, 2) AS size_mb
                    FROM information_schema.tables 
                    WHERE table_schema = DATABASE()";
            }
            
            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync();
            
            var result = await cmd.ExecuteScalarAsync();
            if (result != null && result != DBNull.Value)
            {
                return Convert.ToDouble(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get database size");
        }
        return 0;
    }

    private bool IsSqlServer()
    {
        var provider = _configuration["DatabaseProvider"]?.ToLower() ?? "";
        if (provider == "sqlserver" || provider == "mssql") return true;
        
        var connString = _configuration.GetConnectionString("DefaultConnection") ?? "";
        return connString.Contains("Server=") && !connString.Contains("Port=");
    }

    private string GetDatabaseProviderName()
    {
        return IsSqlServer() ? "SQL Server" : "MariaDB";
    }

    private async Task<(bool IsHealthy, string Version, string Uptime, int ResponseTimeMs)> CheckDatabaseHealthAsync()
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var connection = _context.Database.GetDbConnection();
            await using var cmd = connection.CreateCommand();
            
            if (IsSqlServer())
            {
                cmd.CommandText = "SELECT @@VERSION";
            }
            else
            {
                cmd.CommandText = "SELECT VERSION()";
            }
            
            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync();
            
            var version = await cmd.ExecuteScalarAsync() as string ?? "Unknown";
            sw.Stop();
            
            // Get uptime
            TimeSpan uptimeSpan = TimeSpan.Zero;
            
            if (IsSqlServer())
            {
                await using var uptimeCmd = connection.CreateCommand();
                uptimeCmd.CommandText = "SELECT sqlserver_start_time FROM sys.dm_os_sys_info";
                try
                {
                    var startTime = await uptimeCmd.ExecuteScalarAsync();
                    if (startTime is DateTime dt)
                    {
                        uptimeSpan = DateTime.UtcNow - dt;
                    }
                }
                catch
                {
                    // Fallback if dm_os_sys_info not accessible
                    uptimeSpan = DateTime.UtcNow - _startTime;
                }
            }
            else
            {
                await using var uptimeCmd = connection.CreateCommand();
                uptimeCmd.CommandText = "SHOW GLOBAL STATUS LIKE 'Uptime'";
                await using var reader = await uptimeCmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    if (long.TryParse(reader.GetString(1), out var uptimeSeconds))
                    {
                        uptimeSpan = TimeSpan.FromSeconds(uptimeSeconds);
                    }
                }
            }
            
            return (true, version, FormatUptime(uptimeSpan), (int)sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Database health check failed");
            sw.Stop();
            return (false, "Unknown", "Unknown", (int)sw.ElapsedMilliseconds);
        }
    }

    private async Task<(bool IsHealthy, int ResponseTimeMs)> CheckFrontendHealthAsync(string url)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            var response = await client.GetAsync(url);
            sw.Stop();
            return (response.IsSuccessStatusCode, (int)sw.ElapsedMilliseconds);
        }
        catch
        {
            sw.Stop();
            return (false, (int)sw.ElapsedMilliseconds);
        }
    }

    private async Task<(double CpuLoad, double MemoryLoad)> GetDatabaseStatsAsync()
    {
        try
        {
            var connection = _context.Database.GetDbConnection();
            await using var cmd = connection.CreateCommand();
            
            if (IsSqlServer())
            {
                // Get SQL Server connection count as approximation of load
                cmd.CommandText = @"
                    SELECT 
                        (SELECT COUNT(*) FROM sys.dm_exec_sessions WHERE is_user_process = 1) as sessions,
                        (SELECT COUNT(*) FROM sys.dm_exec_requests WHERE status = 'running') as running";
            }
            else
            {
                // Get thread count as approximation of load
                cmd.CommandText = @"
                    SELECT 
                        (SELECT COUNT(*) FROM information_schema.processlist) as threads,
                        (SELECT VARIABLE_VALUE FROM information_schema.global_status WHERE VARIABLE_NAME = 'Threads_running') as running";
            }
            
            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync();
            
            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var threads = reader.GetInt32(0);
                int running = 0;
                
                if (IsSqlServer())
                {
                    running = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                }
                else
                {
                    running = reader.IsDBNull(1) ? 0 : (int.TryParse(reader.GetString(1), out var r) ? r : 0);
                }
                
                // Estimate load based on running threads (simplified)
                var cpuLoad = Math.Min(100, running * 10.0);
                var memoryLoad = Math.Min(100, threads * 2.0);
                
                return (cpuLoad, memoryLoad);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get database stats");
        }
        return (0, 0);
    }

    private string GetDatabaseHost()
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";
        var parts = connectionString.Split(';');
        foreach (var part in parts)
        {
            var keyValue = part.Split('=');
            if (keyValue.Length == 2 && keyValue[0].Trim().Equals("Server", StringComparison.OrdinalIgnoreCase))
            {
                return keyValue[1].Trim();
            }
        }
        return "localhost:3306";
    }

    private static string FormatUptime(TimeSpan uptime)
    {
        if (uptime.TotalDays >= 1)
            return $"{(int)uptime.TotalDays}d {uptime.Hours}h {uptime.Minutes}m";
        if (uptime.TotalHours >= 1)
            return $"{(int)uptime.TotalHours}h {uptime.Minutes}m";
        return $"{uptime.Minutes}m {uptime.Seconds}s";
    }

    private static string CalculateUptime(DateTime startTime)
    {
        var uptime = DateTime.UtcNow - startTime;
        var totalHours = uptime.TotalHours;
        
        // Simple uptime percentage calculation (assumes 100% if running)
        if (totalHours < 1) return "100.00%";
        return "99.99%";
    }

    private static string GetAssemblyVersion()
    {
        return typeof(MonitoringController).Assembly.GetName().Version?.ToString() ?? "1.0.0";
    }

    private static long GetTotalPhysicalMemory()
    {
        try
        {
            // This is a simplified approach - in production you'd use platform-specific APIs
            return 8L * 1024 * 1024 * 1024; // Default 8GB
        }
        catch
        {
            return 0;
        }
    }

    #endregion
}

#region DTOs

public class SystemMetricsDto
{
    public DateTime Timestamp { get; set; }
    public string Uptime { get; set; } = "";
    public long UptimeSeconds { get; set; }
    public ProcessMetricsDto Process { get; set; } = new();
    public SystemInfoDto System { get; set; } = new();
    public DatabaseMetricsDto Database { get; set; } = new();
    public GCMetricsDto GarbageCollection { get; set; } = new();
}

public class ProcessMetricsDto
{
    public long CpuTimeMs { get; set; }
    public long WorkingSetMB { get; set; }
    public long PrivateMemoryMB { get; set; }
    public long GCMemoryMB { get; set; }
    public int ThreadCount { get; set; }
    public int HandleCount { get; set; }
}

public class SystemInfoDto
{
    public string MachineName { get; set; } = "";
    public string OSDescription { get; set; } = "";
    public int ProcessorCount { get; set; }
    public bool Is64Bit { get; set; }
    public string DotNetVersion { get; set; } = "";
}

public class DatabaseMetricsDto
{
    public string Provider { get; set; } = "";
    public string ConnectionStatus { get; set; } = "";
    public int ActiveConnections { get; set; }
    public double DatabaseSizeMB { get; set; }
}

public class GCMetricsDto
{
    public int Gen0Collections { get; set; }
    public int Gen1Collections { get; set; }
    public int Gen2Collections { get; set; }
    public long TotalMemoryMB { get; set; }
}

public class ServiceStatusDto
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string Status { get; set; } = "";
    public string Endpoint { get; set; } = "";
    public string Uptime { get; set; } = "";
    public string LastCheck { get; set; } = "";
    public int ResponseTime { get; set; }
    public string Version { get; set; } = "";
}

public class ServerLoadDto
{
    public DateTime Timestamp { get; set; }
    public List<ServiceLoadDto> Services { get; set; } = new();
}

public class ServiceLoadDto
{
    public string Service { get; set; } = "";
    public double CpuLoad { get; set; }
    public double MemoryLoad { get; set; }
    public string Status { get; set; } = "";
}

public class ActiveSessionDto
{
    public string UserId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string LoginTime { get; set; } = "";
    public string LastActivity { get; set; } = "";
    public string IpAddress { get; set; } = "";
    public string Role { get; set; } = "";
    public bool IsActive { get; set; }
}

public class MonitoringDataDto
{
    public DateTime Timestamp { get; set; }
    public SystemMetricsDto SystemMetrics { get; set; } = new();
    public List<ServiceStatusDto> Services { get; set; } = new();
    public ServerLoadDto ServerLoad { get; set; } = new();
    public List<ActiveSessionDto> ActiveSessions { get; set; } = new();
}

#endregion
