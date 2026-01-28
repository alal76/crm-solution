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
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySqlConnector;
using Npgsql;

namespace CRM.Infrastructure.Services;

#region Enums

/// <summary>Database providers supported by the monitoring system</summary>
public enum DatabaseProviderType
{
    Unknown,
    MariaDB,
    MySQL,
    SqlServer,
    PostgreSQL,
    MongoDB,
    Oracle
}

/// <summary>Deployment types supported by the monitoring system</summary>
public enum DeploymentType
{
    Docker,
    Kubernetes,
    VirtualMachine,
    Hybrid,
    Unknown
}

#endregion

#region Configuration Options

/// <summary>
/// Monitoring configuration options from environment variables
/// </summary>
public class MonitoringOptions
{
    public const string SectionName = "Monitoring";
    
    /// <summary>Deployment type: docker, kubernetes, vm, hybrid</summary>
    public string DeploymentType { get; set; } = "docker";
    
    /// <summary>Build server hostname/IP</summary>
    public string BuildServer { get; set; } = "localhost";
    
    /// <summary>Build server FQDN</summary>
    public string BuildServerFQDN { get; set; } = "";
    
    /// <summary>Database provider: mariadb, mysql, sqlserver, postgresql, mongodb, oracle</summary>
    public string DatabaseProvider { get; set; } = "sqlserver";
    
    /// <summary>Kubernetes namespace for pod discovery</summary>
    public string KubernetesNamespace { get; set; } = "crm-app";
    
    /// <summary>Enable Kubernetes pod monitoring</summary>
    public bool EnableK8sMonitoring { get; set; } = false;
    
    /// <summary>Enable Docker container monitoring</summary>
    public bool EnableDockerMonitoring { get; set; } = true;
    
    /// <summary>Healthcheck timeout in seconds</summary>
    public int HealthCheckTimeoutSeconds { get; set; } = 5;
    
    /// <summary>Cache duration for monitoring data in seconds</summary>
    public int CacheDurationSeconds { get; set; } = 30;
}

#endregion

#region DTOs

/// <summary>Detected infrastructure information</summary>
public class InfrastructureInfo
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public DeploymentType DeploymentType { get; set; }
    public string DeploymentTypeName { get; set; } = "";
    public DatabaseInfo Database { get; set; } = new();
    public HostInfo Host { get; set; } = new();
    public List<string> ActiveMonitors { get; set; } = new();
    public List<string> AvailableMonitors { get; set; } = new();
}

/// <summary>Database connection information</summary>
public class DatabaseInfo
{
    public DatabaseProviderType Provider { get; set; }
    public string ProviderName { get; set; } = "";
    public string Version { get; set; } = "";
    public string Host { get; set; } = "";
    public int Port { get; set; }
    public bool IsConnected { get; set; }
    public string Edition { get; set; } = "";
    public string Collation { get; set; } = "";
}

/// <summary>Host system information</summary>
public class HostInfo
{
    public string Hostname { get; set; } = "";
    public string FQDN { get; set; } = "";
    public string OSDescription { get; set; } = "";
    public string Architecture { get; set; } = "";
    public int ProcessorCount { get; set; }
    public long TotalMemoryMB { get; set; }
    public string DotNetVersion { get; set; } = "";
    public bool IsDocker { get; set; }
    public bool IsKubernetes { get; set; }
}

/// <summary>System metrics including CPU, memory, disk, network</summary>
public class SystemMetrics
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public CpuMetrics Cpu { get; set; } = new();
    public MemoryMetrics Memory { get; set; } = new();
    public DiskMetrics Disk { get; set; } = new();
    public NetworkMetrics Network { get; set; } = new();
    public ProcessMetrics Process { get; set; } = new();
}

public class CpuMetrics
{
    public double UsagePercent { get; set; }
    public int ProcessorCount { get; set; }
    public double ProcessCpuPercent { get; set; }
}

public class MemoryMetrics
{
    public long TotalMB { get; set; }
    public long UsedMB { get; set; }
    public long FreeMB { get; set; }
    public double UsagePercent { get; set; }
    public long ProcessWorkingSetMB { get; set; }
}

public class DiskMetrics
{
    public List<DiskInfo> Drives { get; set; } = new();
    public long TotalSpaceGB { get; set; }
    public long UsedSpaceGB { get; set; }
    public long FreeSpaceGB { get; set; }
    public double UsagePercent { get; set; }
}

public class DiskInfo
{
    public string Name { get; set; } = "";
    public string MountPoint { get; set; } = "";
    public long TotalGB { get; set; }
    public long UsedGB { get; set; }
    public long FreeGB { get; set; }
    public double UsagePercent { get; set; }
}

public class NetworkMetrics
{
    public long BytesReceived { get; set; }
    public long BytesSent { get; set; }
    public List<NetworkInterfaceInfo> Interfaces { get; set; } = new();
}

public class NetworkInterfaceInfo
{
    public string Name { get; set; } = "";
    public string IpAddress { get; set; } = "";
    public bool IsUp { get; set; }
    public long BytesReceived { get; set; }
    public long BytesSent { get; set; }
}

public class ProcessMetrics
{
    public int ThreadCount { get; set; }
    public int HandleCount { get; set; }
    public long WorkingSetMB { get; set; }
    public long PrivateMemoryMB { get; set; }
    public long CpuTimeMs { get; set; }
    public TimeSpan Uptime { get; set; }
    public string UptimeFormatted { get; set; } = "";
}

/// <summary>Database-specific metrics</summary>
public class DatabaseMetrics
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public DatabaseProviderType Provider { get; set; }
    public string ProviderName { get; set; } = "";
    public bool IsHealthy { get; set; }
    public int ResponseTimeMs { get; set; }
    public int ActiveConnections { get; set; }
    public double DatabaseSizeMB { get; set; }
    public string Version { get; set; } = "";
    public Dictionary<string, object> ProviderSpecificMetrics { get; set; } = new();
}

/// <summary>Service health status</summary>
public class ServiceHealth
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string Status { get; set; } = "unknown";
    public string Endpoint { get; set; } = "";
    public int ResponseTimeMs { get; set; }
    public string Version { get; set; } = "";
    public DateTime LastCheck { get; set; } = DateTime.UtcNow;
    public string Uptime { get; set; } = "";
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>Docker container health</summary>
public class ContainerHealth
{
    public string ContainerId { get; set; } = "";
    public string ContainerName { get; set; } = "";
    public string Image { get; set; } = "";
    public string Status { get; set; } = "";
    public string Health { get; set; } = "";
    public DateTime StartedAt { get; set; }
    public string Uptime { get; set; } = "";
    public double CpuPercent { get; set; }
    public long MemoryMB { get; set; }
    public long MemoryLimitMB { get; set; }
    public double MemoryPercent { get; set; }
}

/// <summary>Kubernetes pod health</summary>
public class PodHealth
{
    public string PodName { get; set; } = "";
    public string Namespace { get; set; } = "";
    public string Phase { get; set; } = "";
    public bool Ready { get; set; }
    public int RestartCount { get; set; }
    public string NodeName { get; set; } = "";
    public string PodIP { get; set; } = "";
    public DateTime StartedAt { get; set; }
    public string Uptime { get; set; } = "";
    public List<ContainerHealth> Containers { get; set; } = new();
}

/// <summary>User session information</summary>
public class UserSession
{
    public string UserId { get; set; } = "";
    public string UserName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
    public DateTime LoginTime { get; set; }
    public DateTime LastActivity { get; set; }
    public string IpAddress { get; set; } = "";
    public bool IsActive { get; set; }
}

/// <summary>Complete monitoring data bundle</summary>
public class MonitoringData
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public InfrastructureInfo Infrastructure { get; set; } = new();
    public SystemMetrics System { get; set; } = new();
    public DatabaseMetrics Database { get; set; } = new();
    public List<ServiceHealth> Services { get; set; } = new();
    public List<ContainerHealth> Containers { get; set; } = new();
    public List<PodHealth> Pods { get; set; } = new();
    public List<UserSession> ActiveSessions { get; set; } = new();
}

#endregion

#region Interface

/// <summary>Interface for the comprehensive monitoring service</summary>
public interface IMonitoringService
{
    /// <summary>Detect and return infrastructure information</summary>
    Task<InfrastructureInfo> GetInfrastructureInfoAsync(CancellationToken ct = default);
    
    /// <summary>Get system metrics (CPU, memory, disk, network)</summary>
    Task<SystemMetrics> GetSystemMetricsAsync(CancellationToken ct = default);
    
    /// <summary>Get database-specific metrics</summary>
    Task<DatabaseMetrics> GetDatabaseMetricsAsync(CancellationToken ct = default);
    
    /// <summary>Get health status of all services</summary>
    Task<List<ServiceHealth>> GetServiceHealthAsync(CancellationToken ct = default);
    
    /// <summary>Get Docker container health (if applicable)</summary>
    Task<List<ContainerHealth>> GetContainerHealthAsync(CancellationToken ct = default);
    
    /// <summary>Get Kubernetes pod health (if applicable)</summary>
    Task<List<PodHealth>> GetPodHealthAsync(CancellationToken ct = default);
    
    /// <summary>Get all monitoring data in one call</summary>
    Task<MonitoringData> GetAllMonitoringDataAsync(CancellationToken ct = default);
    
    /// <summary>Get active user sessions</summary>
    Task<List<UserSession>> GetActiveSessionsAsync(CancellationToken ct = default);
    
    /// <summary>Get monitoring options</summary>
    MonitoringOptions GetMonitoringOptions();
}

#endregion

#region Implementation

/// <summary>
/// Comprehensive monitoring service with multi-database and multi-infrastructure support
/// </summary>
public class MonitoringService : IMonitoringService
{
    private readonly IConfiguration _configuration;
    private readonly IRedisCacheService _cache;
    private readonly ILogger<MonitoringService> _logger;
    private readonly MonitoringOptions _options;
    private readonly HttpClient _httpClient;
    private static readonly DateTime _startTime = DateTime.UtcNow;

    public MonitoringService(
        IConfiguration configuration,
        IRedisCacheService cache,
        IOptions<MonitoringOptions> options,
        ILogger<MonitoringService> logger)
    {
        _configuration = configuration;
        _cache = cache;
        _options = options.Value;
        _logger = logger;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(_options.HealthCheckTimeoutSeconds)
        };
    }

    public MonitoringOptions GetMonitoringOptions() => _options;

    #region Infrastructure Detection

    public async Task<InfrastructureInfo> GetInfrastructureInfoAsync(CancellationToken ct = default)
    {
        var info = new InfrastructureInfo
        {
            Timestamp = DateTime.UtcNow,
            DeploymentType = DetectDeploymentType(),
            DeploymentTypeName = _options.DeploymentType,
            Host = GetHostInfo(),
            ActiveMonitors = new List<string>(),
            AvailableMonitors = GetAvailableMonitors()
        };
        
        info.Database = await DetectDatabaseAsync(ct);
        info.ActiveMonitors = GetActiveMonitors(info);
        
        return info;
    }

    private DeploymentType DetectDeploymentType()
    {
        // Check if running in Kubernetes
        if (Environment.GetEnvironmentVariable("KUBERNETES_SERVICE_HOST") != null ||
            File.Exists("/var/run/secrets/kubernetes.io/serviceaccount/token"))
        {
            return DeploymentType.Kubernetes;
        }
        
        // Check if running in Docker
        if (File.Exists("/.dockerenv") || 
            Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
        {
            return DeploymentType.Docker;
        }
        
        // Parse from configuration
        return _options.DeploymentType?.ToLowerInvariant() switch
        {
            "kubernetes" or "k8s" => DeploymentType.Kubernetes,
            "docker" => DeploymentType.Docker,
            "vm" or "virtualmachine" => DeploymentType.VirtualMachine,
            "hybrid" => DeploymentType.Hybrid,
            _ => DeploymentType.Unknown
        };
    }

    private HostInfo GetHostInfo()
    {
        return new HostInfo
        {
            Hostname = Environment.MachineName,
            FQDN = _options.BuildServerFQDN ?? _options.BuildServer ?? Environment.MachineName,
            OSDescription = RuntimeInformation.OSDescription,
            Architecture = RuntimeInformation.OSArchitecture.ToString(),
            ProcessorCount = Environment.ProcessorCount,
            TotalMemoryMB = GetTotalPhysicalMemory() / (1024 * 1024),
            DotNetVersion = RuntimeInformation.FrameworkDescription,
            IsDocker = File.Exists("/.dockerenv") || Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true",
            IsKubernetes = Environment.GetEnvironmentVariable("KUBERNETES_SERVICE_HOST") != null
        };
    }

    private async Task<DatabaseInfo> DetectDatabaseAsync(CancellationToken ct)
    {
        var provider = ParseDatabaseProvider(_options.DatabaseProvider);
        var info = new DatabaseInfo
        {
            Provider = provider,
            ProviderName = provider.ToString()
        };

        try
        {
            var connectionString = GetDatabaseConnectionString();
            
            switch (provider)
            {
                case DatabaseProviderType.SqlServer:
                    info = await GetSqlServerInfoAsync(connectionString, ct);
                    break;
                case DatabaseProviderType.MariaDB:
                case DatabaseProviderType.MySQL:
                    info = await GetMySqlInfoAsync(connectionString, provider, ct);
                    break;
                case DatabaseProviderType.PostgreSQL:
                    info = await GetPostgreSqlInfoAsync(connectionString, ct);
                    break;
                default:
                    info.IsConnected = false;
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to detect database info");
            info.IsConnected = false;
        }

        return info;
    }

    private DatabaseProviderType ParseDatabaseProvider(string provider)
    {
        return provider?.ToLowerInvariant() switch
        {
            "sqlserver" or "mssql" => DatabaseProviderType.SqlServer,
            "mariadb" => DatabaseProviderType.MariaDB,
            "mysql" => DatabaseProviderType.MySQL,
            "postgresql" or "postgres" => DatabaseProviderType.PostgreSQL,
            "mongodb" or "mongo" => DatabaseProviderType.MongoDB,
            "oracle" => DatabaseProviderType.Oracle,
            _ => DatabaseProviderType.Unknown
        };
    }

    private List<string> GetAvailableMonitors()
    {
        return new List<string>
        {
            "system", "cpu", "memory", "disk", "network", "process",
            "database", "mariadb", "mysql", "sqlserver", "postgresql", "mongodb", "oracle",
            "docker", "kubernetes", "services", "sessions"
        };
    }

    private List<string> GetActiveMonitors(InfrastructureInfo info)
    {
        var active = new List<string> { "system", "cpu", "memory", "disk", "network", "process", "services", "sessions" };
        
        // Add database-specific monitor
        active.Add("database");
        active.Add(info.Database.Provider.ToString().ToLowerInvariant());
        
        // Add deployment-specific monitors
        if (info.DeploymentType == DeploymentType.Docker || info.Host.IsDocker || _options.EnableDockerMonitoring)
        {
            active.Add("docker");
        }
        
        if (info.DeploymentType == DeploymentType.Kubernetes || info.Host.IsKubernetes || _options.EnableK8sMonitoring)
        {
            active.Add("kubernetes");
        }
        
        return active;
    }

    #endregion

    #region System Metrics

    public async Task<SystemMetrics> GetSystemMetricsAsync(CancellationToken ct = default)
    {
        var process = Process.GetCurrentProcess();
        
        return new SystemMetrics
        {
            Timestamp = DateTime.UtcNow,
            Cpu = await GetCpuMetricsAsync(process, ct),
            Memory = GetMemoryMetrics(process),
            Disk = GetDiskMetrics(),
            Network = GetNetworkMetrics(),
            Process = GetProcessMetrics(process)
        };
    }

    private async Task<CpuMetrics> GetCpuMetricsAsync(Process process, CancellationToken ct)
    {
        var startCpuTime = process.TotalProcessorTime;
        await Task.Delay(100, ct);
        process.Refresh();
        var endCpuTime = process.TotalProcessorTime;
        
        var cpuUsedMs = (endCpuTime - startCpuTime).TotalMilliseconds;
        var cpuPercent = (cpuUsedMs / (100.0 * Environment.ProcessorCount)) * 100;
        
        return new CpuMetrics
        {
            UsagePercent = Math.Min(100, Math.Max(0, cpuPercent)),
            ProcessorCount = Environment.ProcessorCount,
            ProcessCpuPercent = Math.Min(100, Math.Max(0, cpuPercent))
        };
    }

    private MemoryMetrics GetMemoryMetrics(Process process)
    {
        var totalMemory = GetTotalPhysicalMemory();
        var workingSet = process.WorkingSet64;
        
        return new MemoryMetrics
        {
            TotalMB = totalMemory / (1024 * 1024),
            UsedMB = (totalMemory - GC.GetGCMemoryInfo().HighMemoryLoadThresholdBytes) / (1024 * 1024),
            FreeMB = GC.GetGCMemoryInfo().HighMemoryLoadThresholdBytes / (1024 * 1024),
            UsagePercent = totalMemory > 0 ? (workingSet * 100.0 / totalMemory) : 0,
            ProcessWorkingSetMB = workingSet / (1024 * 1024)
        };
    }

    private DiskMetrics GetDiskMetrics()
    {
        var metrics = new DiskMetrics { Drives = new List<DiskInfo>() };
        
        try
        {
            var drives = DriveInfo.GetDrives().Where(d => d.IsReady && d.DriveType == DriveType.Fixed);
            
            foreach (var drive in drives)
            {
                var diskInfo = new DiskInfo
                {
                    Name = drive.Name,
                    MountPoint = drive.RootDirectory.FullName,
                    TotalGB = drive.TotalSize / (1024 * 1024 * 1024),
                    FreeGB = drive.AvailableFreeSpace / (1024 * 1024 * 1024),
                    UsedGB = (drive.TotalSize - drive.AvailableFreeSpace) / (1024 * 1024 * 1024),
                    UsagePercent = drive.TotalSize > 0 
                        ? ((drive.TotalSize - drive.AvailableFreeSpace) * 100.0 / drive.TotalSize) 
                        : 0
                };
                metrics.Drives.Add(diskInfo);
            }
            
            metrics.TotalSpaceGB = metrics.Drives.Sum(d => d.TotalGB);
            metrics.UsedSpaceGB = metrics.Drives.Sum(d => d.UsedGB);
            metrics.FreeSpaceGB = metrics.Drives.Sum(d => d.FreeGB);
            metrics.UsagePercent = metrics.TotalSpaceGB > 0 
                ? (metrics.UsedSpaceGB * 100.0 / metrics.TotalSpaceGB) 
                : 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get disk metrics");
        }
        
        return metrics;
    }

    private NetworkMetrics GetNetworkMetrics()
    {
        var metrics = new NetworkMetrics { Interfaces = new List<NetworkInterfaceInfo>() };
        
        try
        {
            var interfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
            
            foreach (var ni in interfaces.Where(n => n.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up))
            {
                var stats = ni.GetIPv4Statistics();
                var addresses = ni.GetIPProperties().UnicastAddresses
                    .Where(a => a.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    .Select(a => a.Address.ToString())
                    .FirstOrDefault();
                
                metrics.Interfaces.Add(new NetworkInterfaceInfo
                {
                    Name = ni.Name,
                    IpAddress = addresses ?? "",
                    IsUp = true,
                    BytesReceived = stats.BytesReceived,
                    BytesSent = stats.BytesSent
                });
                
                metrics.BytesReceived += stats.BytesReceived;
                metrics.BytesSent += stats.BytesSent;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get network metrics");
        }
        
        return metrics;
    }

    private ProcessMetrics GetProcessMetrics(Process process)
    {
        var uptime = DateTime.UtcNow - _startTime;
        
        return new ProcessMetrics
        {
            ThreadCount = process.Threads.Count,
            HandleCount = process.HandleCount,
            WorkingSetMB = process.WorkingSet64 / (1024 * 1024),
            PrivateMemoryMB = process.PrivateMemorySize64 / (1024 * 1024),
            CpuTimeMs = (long)process.TotalProcessorTime.TotalMilliseconds,
            Uptime = uptime,
            UptimeFormatted = FormatUptime(uptime)
        };
    }

    #endregion

    #region Database Metrics

    public async Task<DatabaseMetrics> GetDatabaseMetricsAsync(CancellationToken ct = default)
    {
        var provider = ParseDatabaseProvider(_options.DatabaseProvider);
        var metrics = new DatabaseMetrics
        {
            Timestamp = DateTime.UtcNow,
            Provider = provider,
            ProviderName = provider.ToString()
        };

        var connectionString = GetDatabaseConnectionString();
        var sw = Stopwatch.StartNew();

        try
        {
            switch (provider)
            {
                case DatabaseProviderType.SqlServer:
                    metrics = await GetSqlServerMetricsAsync(connectionString, ct);
                    break;
                case DatabaseProviderType.MariaDB:
                case DatabaseProviderType.MySQL:
                    metrics = await GetMySqlMetricsAsync(connectionString, provider, ct);
                    break;
                case DatabaseProviderType.PostgreSQL:
                    metrics = await GetPostgreSqlMetricsAsync(connectionString, ct);
                    break;
                default:
                    metrics.IsHealthy = false;
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get database metrics");
            metrics.IsHealthy = false;
        }

        sw.Stop();
        if (metrics.ResponseTimeMs == 0)
            metrics.ResponseTimeMs = (int)sw.ElapsedMilliseconds;

        return metrics;
    }

    private string GetDatabaseConnectionString()
    {
        return _configuration.GetConnectionString("DefaultConnection") ?? "";
    }

    private async Task<DatabaseInfo> GetSqlServerInfoAsync(string connectionString, CancellationToken ct)
    {
        var info = new DatabaseInfo
        {
            Provider = DatabaseProviderType.SqlServer,
            ProviderName = "SQL Server"
        };

        try
        {
            await using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync(ct);
            
            info.IsConnected = true;
            info.Host = conn.DataSource ?? "";
            
            var cmd = new SqlCommand(@"
                SELECT 
                    SERVERPROPERTY('ProductVersion') AS Version,
                    SERVERPROPERTY('Edition') AS Edition,
                    SERVERPROPERTY('Collation') AS Collation", conn);
            
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                info.Version = reader["Version"]?.ToString() ?? "";
                info.Edition = reader["Edition"]?.ToString() ?? "";
                info.Collation = reader["Collation"]?.ToString() ?? "";
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get SQL Server info");
            info.IsConnected = false;
        }

        return info;
    }

    private async Task<DatabaseMetrics> GetSqlServerMetricsAsync(string connectionString, CancellationToken ct)
    {
        var metrics = new DatabaseMetrics
        {
            Provider = DatabaseProviderType.SqlServer,
            ProviderName = "SQL Server",
            ProviderSpecificMetrics = new Dictionary<string, object>()
        };

        var sw = Stopwatch.StartNew();

        try
        {
            await using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync(ct);
            sw.Stop();
            metrics.ResponseTimeMs = (int)sw.ElapsedMilliseconds;
            metrics.IsHealthy = true;

            // Get version
            var versionCmd = new SqlCommand("SELECT SERVERPROPERTY('ProductVersion') AS Version", conn);
            var version = await versionCmd.ExecuteScalarAsync(ct);
            metrics.Version = version?.ToString() ?? "";

            // Get active connections
            var connCmd = new SqlCommand(@"
                SELECT COUNT(*) FROM sys.dm_exec_sessions 
                WHERE is_user_process = 1", conn);
            var connCount = await connCmd.ExecuteScalarAsync(ct);
            metrics.ActiveConnections = Convert.ToInt32(connCount);

            // Get database size
            var sizeCmd = new SqlCommand(@"
                SELECT SUM(size * 8.0 / 1024) AS SizeMB 
                FROM sys.master_files 
                WHERE database_id = DB_ID()", conn);
            var size = await sizeCmd.ExecuteScalarAsync(ct);
            metrics.DatabaseSizeMB = Convert.ToDouble(size ?? 0);

            // Get SQL Server specific metrics
            var perfCmd = new SqlCommand(@"
                SELECT 
                    (SELECT cntr_value FROM sys.dm_os_performance_counters 
                     WHERE counter_name = 'Batch Requests/sec' AND instance_name = '') AS BatchRequests,
                    (SELECT cntr_value FROM sys.dm_os_performance_counters 
                     WHERE counter_name = 'Buffer cache hit ratio' AND instance_name = '') AS CacheHitRatio,
                    (SELECT COUNT(*) FROM sys.dm_exec_requests WHERE status = 'running') AS ActiveQueries", conn);
            
            try
            {
                await using var reader = await perfCmd.ExecuteReaderAsync(ct);
                if (await reader.ReadAsync(ct))
                {
                    metrics.ProviderSpecificMetrics["batchRequestsPerSec"] = reader["BatchRequests"] ?? 0;
                    metrics.ProviderSpecificMetrics["bufferCacheHitRatio"] = reader["CacheHitRatio"] ?? 0;
                    metrics.ProviderSpecificMetrics["activeQueries"] = reader["ActiveQueries"] ?? 0;
                }
            }
            catch
            {
                // Permissions may prevent reading performance counters
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get SQL Server metrics");
            sw.Stop();
            metrics.ResponseTimeMs = (int)sw.ElapsedMilliseconds;
            metrics.IsHealthy = false;
        }

        return metrics;
    }

    private async Task<DatabaseInfo> GetMySqlInfoAsync(string connectionString, DatabaseProviderType provider, CancellationToken ct)
    {
        var info = new DatabaseInfo
        {
            Provider = provider,
            ProviderName = provider == DatabaseProviderType.MariaDB ? "MariaDB" : "MySQL"
        };

        try
        {
            await using var conn = new MySqlConnection(connectionString);
            await conn.OpenAsync(ct);
            
            info.IsConnected = true;
            info.Host = conn.DataSource ?? "";
            
            var cmd = new MySqlCommand("SELECT VERSION() AS Version, @@collation_database AS Collation", conn);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                info.Version = reader["Version"]?.ToString() ?? "";
                info.Collation = reader["Collation"]?.ToString() ?? "";
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get MySQL/MariaDB info");
            info.IsConnected = false;
        }

        return info;
    }

    private async Task<DatabaseMetrics> GetMySqlMetricsAsync(string connectionString, DatabaseProviderType provider, CancellationToken ct)
    {
        var metrics = new DatabaseMetrics
        {
            Provider = provider,
            ProviderName = provider == DatabaseProviderType.MariaDB ? "MariaDB" : "MySQL",
            ProviderSpecificMetrics = new Dictionary<string, object>()
        };

        var sw = Stopwatch.StartNew();

        try
        {
            await using var conn = new MySqlConnection(connectionString);
            await conn.OpenAsync(ct);
            sw.Stop();
            metrics.ResponseTimeMs = (int)sw.ElapsedMilliseconds;
            metrics.IsHealthy = true;

            var versionCmd = new MySqlCommand("SELECT VERSION()", conn);
            metrics.Version = (await versionCmd.ExecuteScalarAsync(ct))?.ToString() ?? "";

            var connCmd = new MySqlCommand("SHOW STATUS LIKE 'Threads_connected'", conn);
            await using (var reader = await connCmd.ExecuteReaderAsync(ct))
            {
                if (await reader.ReadAsync(ct))
                {
                    metrics.ActiveConnections = Convert.ToInt32(reader["Value"]);
                }
            }

            var sizeCmd = new MySqlCommand(@"
                SELECT SUM(data_length + index_length) / 1024 / 1024 AS SizeMB 
                FROM information_schema.tables 
                WHERE table_schema = DATABASE()", conn);
            var size = await sizeCmd.ExecuteScalarAsync(ct);
            metrics.DatabaseSizeMB = Convert.ToDouble(size ?? 0);

            // Get MySQL specific metrics
            var statusCmd = new MySqlCommand("SHOW GLOBAL STATUS WHERE Variable_name IN ('Queries', 'Slow_queries', 'Uptime')", conn);
            await using (var reader = await statusCmd.ExecuteReaderAsync(ct))
            {
                while (await reader.ReadAsync(ct))
                {
                    var name = reader["Variable_name"]?.ToString() ?? "";
                    var value = reader["Value"];
                    metrics.ProviderSpecificMetrics[name.ToLowerInvariant()] = value ?? 0;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get MySQL/MariaDB metrics");
            sw.Stop();
            metrics.ResponseTimeMs = (int)sw.ElapsedMilliseconds;
            metrics.IsHealthy = false;
        }

        return metrics;
    }

    private async Task<DatabaseInfo> GetPostgreSqlInfoAsync(string connectionString, CancellationToken ct)
    {
        var info = new DatabaseInfo
        {
            Provider = DatabaseProviderType.PostgreSQL,
            ProviderName = "PostgreSQL"
        };

        try
        {
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(ct);
            
            info.IsConnected = true;
            info.Host = conn.Host ?? "";
            info.Port = conn.Port;
            
            var cmd = new NpgsqlCommand("SELECT version(), current_setting('server_encoding')", conn);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                info.Version = reader.GetString(0);
                info.Collation = reader.GetString(1);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get PostgreSQL info");
            info.IsConnected = false;
        }

        return info;
    }

    private async Task<DatabaseMetrics> GetPostgreSqlMetricsAsync(string connectionString, CancellationToken ct)
    {
        var metrics = new DatabaseMetrics
        {
            Provider = DatabaseProviderType.PostgreSQL,
            ProviderName = "PostgreSQL",
            ProviderSpecificMetrics = new Dictionary<string, object>()
        };

        var sw = Stopwatch.StartNew();

        try
        {
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(ct);
            sw.Stop();
            metrics.ResponseTimeMs = (int)sw.ElapsedMilliseconds;
            metrics.IsHealthy = true;

            var versionCmd = new NpgsqlCommand("SELECT version()", conn);
            metrics.Version = (await versionCmd.ExecuteScalarAsync(ct))?.ToString() ?? "";

            var connCmd = new NpgsqlCommand("SELECT count(*) FROM pg_stat_activity WHERE state = 'active'", conn);
            metrics.ActiveConnections = Convert.ToInt32(await connCmd.ExecuteScalarAsync(ct));

            var sizeCmd = new NpgsqlCommand("SELECT pg_database_size(current_database()) / 1024.0 / 1024.0", conn);
            var size = await sizeCmd.ExecuteScalarAsync(ct);
            metrics.DatabaseSizeMB = Convert.ToDouble(size ?? 0);

            // Get PostgreSQL specific metrics
            var statsCmd = new NpgsqlCommand(@"
                SELECT 
                    xact_commit, xact_rollback, blks_read, blks_hit,
                    tup_returned, tup_fetched, tup_inserted, tup_updated, tup_deleted
                FROM pg_stat_database 
                WHERE datname = current_database()", conn);
            
            await using var reader = await statsCmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                metrics.ProviderSpecificMetrics["xact_commit"] = reader["xact_commit"];
                metrics.ProviderSpecificMetrics["xact_rollback"] = reader["xact_rollback"];
                metrics.ProviderSpecificMetrics["blks_read"] = reader["blks_read"];
                metrics.ProviderSpecificMetrics["blks_hit"] = reader["blks_hit"];
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get PostgreSQL metrics");
            sw.Stop();
            metrics.ResponseTimeMs = (int)sw.ElapsedMilliseconds;
            metrics.IsHealthy = false;
        }

        return metrics;
    }

    #endregion

    #region Service Health

    public async Task<List<ServiceHealth>> GetServiceHealthAsync(CancellationToken ct = default)
    {
        var services = new List<ServiceHealth>();
        
        // API Server (self)
        services.Add(GetApiHealth());
        
        // Database
        services.Add(await GetDatabaseHealthAsync(ct));
        
        // Frontend
        var frontendUrl = _configuration["Frontend:Url"] ?? "http://localhost:3000";
        services.Add(await CheckHttpHealthAsync("CRM Frontend", "frontend", frontendUrl, ct));
        
        return services;
    }

    private ServiceHealth GetApiHealth()
    {
        var uptime = DateTime.UtcNow - _startTime;
        
        return new ServiceHealth
        {
            Name = "CRM API",
            Type = "api",
            Status = "healthy",
            Endpoint = _options.BuildServer ?? "localhost",
            ResponseTimeMs = 0,
            Version = GetAssemblyVersion(),
            LastCheck = DateTime.UtcNow,
            Uptime = FormatUptime(uptime),
            Metadata = new Dictionary<string, object>
            {
                ["processId"] = Process.GetCurrentProcess().Id,
                ["threads"] = Process.GetCurrentProcess().Threads.Count,
                ["memoryMB"] = Process.GetCurrentProcess().WorkingSet64 / (1024 * 1024)
            }
        };
    }

    private async Task<ServiceHealth> GetDatabaseHealthAsync(CancellationToken ct)
    {
        var metrics = await GetDatabaseMetricsAsync(ct);
        
        return new ServiceHealth
        {
            Name = $"Database ({metrics.ProviderName})",
            Type = "database",
            Status = metrics.IsHealthy ? "healthy" : "error",
            Endpoint = GetDatabaseHost(),
            ResponseTimeMs = metrics.ResponseTimeMs,
            Version = metrics.Version,
            LastCheck = DateTime.UtcNow,
            Uptime = "N/A",
            Metadata = new Dictionary<string, object>
            {
                ["provider"] = metrics.ProviderName,
                ["activeConnections"] = metrics.ActiveConnections,
                ["databaseSizeMB"] = metrics.DatabaseSizeMB
            }
        };
    }

    private async Task<ServiceHealth> CheckHttpHealthAsync(string name, string type, string url, CancellationToken ct)
    {
        var health = new ServiceHealth
        {
            Name = name,
            Type = type,
            Endpoint = url,
            LastCheck = DateTime.UtcNow
        };

        var sw = Stopwatch.StartNew();

        try
        {
            var response = await _httpClient.GetAsync(url, ct);
            sw.Stop();
            
            health.ResponseTimeMs = (int)sw.ElapsedMilliseconds;
            health.Status = response.IsSuccessStatusCode ? "healthy" : "degraded";
            health.Version = "1.0.0";
            health.Uptime = "N/A";
        }
        catch (Exception ex)
        {
            sw.Stop();
            health.ResponseTimeMs = (int)sw.ElapsedMilliseconds;
            health.Status = "error";
            health.Metadata["error"] = ex.Message;
            _logger.LogWarning(ex, "Health check failed for {Name}", name);
        }

        return health;
    }

    private string GetDatabaseHost()
    {
        var connectionString = GetDatabaseConnectionString();
        
        // Parse different connection string formats
        if (connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase))
        {
            var match = System.Text.RegularExpressions.Regex.Match(
                connectionString, @"Server=([^;]+)", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[1].Value : "unknown";
        }
        
        if (connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase))
        {
            var match = System.Text.RegularExpressions.Regex.Match(
                connectionString, @"Host=([^;]+)", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[1].Value : "unknown";
        }
        
        return "unknown";
    }

    #endregion

    #region Container Monitoring

    public async Task<List<ContainerHealth>> GetContainerHealthAsync(CancellationToken ct = default)
    {
        if (!_options.EnableDockerMonitoring && !File.Exists("/.dockerenv"))
        {
            return new List<ContainerHealth>();
        }

        var containers = new List<ContainerHealth>();

        try
        {
            // Try to get container info via Docker socket
            var dockerSocket = "/var/run/docker.sock";
            if (File.Exists(dockerSocket))
            {
                containers = await GetDockerContainersAsync(ct);
            }
            else
            {
                // Fallback: return basic container info if we're in a container
                if (File.Exists("/.dockerenv"))
                {
                    containers.Add(new ContainerHealth
                    {
                        ContainerId = Environment.MachineName,
                        ContainerName = "crm-api",
                        Status = "running",
                        Health = "healthy",
                        StartedAt = _startTime,
                        Uptime = FormatUptime(DateTime.UtcNow - _startTime)
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get container health");
        }

        return containers;
    }

    private async Task<List<ContainerHealth>> GetDockerContainersAsync(CancellationToken ct)
    {
        var containers = new List<ContainerHealth>();
        
        try
        {
            // Execute docker ps command
            var psi = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = "ps --format \"{{.ID}}|{{.Names}}|{{.Image}}|{{.Status}}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null) return containers;

            var output = await process.StandardOutput.ReadToEndAsync(ct);
            await process.WaitForExitAsync(ct);

            foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = line.Split('|');
                if (parts.Length >= 4)
                {
                    var status = parts[3].ToLowerInvariant();
                    containers.Add(new ContainerHealth
                    {
                        ContainerId = parts[0],
                        ContainerName = parts[1],
                        Image = parts[2],
                        Status = status.Contains("up") ? "running" : "stopped",
                        Health = status.Contains("healthy") ? "healthy" : 
                                 status.Contains("unhealthy") ? "unhealthy" : "none",
                        Uptime = parts[3]
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to execute docker ps");
        }

        return containers;
    }

    #endregion

    #region Kubernetes Monitoring

    public async Task<List<PodHealth>> GetPodHealthAsync(CancellationToken ct = default)
    {
        if (!_options.EnableK8sMonitoring && 
            Environment.GetEnvironmentVariable("KUBERNETES_SERVICE_HOST") == null)
        {
            return new List<PodHealth>();
        }

        var pods = new List<PodHealth>();

        try
        {
            // Try kubectl if available
            pods = await GetKubernetePodsAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get pod health");
        }

        return pods;
    }

    private async Task<List<PodHealth>> GetKubernetePodsAsync(CancellationToken ct)
    {
        var pods = new List<PodHealth>();
        
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "kubectl",
                Arguments = $"get pods -n {_options.KubernetesNamespace} -o json",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null) return pods;

            var output = await process.StandardOutput.ReadToEndAsync(ct);
            await process.WaitForExitAsync(ct);

            if (process.ExitCode == 0 && !string.IsNullOrEmpty(output))
            {
                var json = JsonDocument.Parse(output);
                var items = json.RootElement.GetProperty("items");

                foreach (var item in items.EnumerateArray())
                {
                    var metadata = item.GetProperty("metadata");
                    var status = item.GetProperty("status");
                    var spec = item.GetProperty("spec");

                    pods.Add(new PodHealth
                    {
                        PodName = metadata.GetProperty("name").GetString() ?? "",
                        Namespace = metadata.GetProperty("namespace").GetString() ?? "",
                        Phase = status.GetProperty("phase").GetString() ?? "",
                        PodIP = status.TryGetProperty("podIP", out var ip) ? ip.GetString() ?? "" : "",
                        NodeName = spec.TryGetProperty("nodeName", out var node) ? node.GetString() ?? "" : "",
                        Ready = status.GetProperty("phase").GetString() == "Running"
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to execute kubectl");
        }

        return pods;
    }

    #endregion

    #region Active Sessions

    public async Task<List<UserSession>> GetActiveSessionsAsync(CancellationToken ct = default)
    {
        // This would be implemented to query active user sessions
        // For now, return empty list - the controller will handle this via EF Core
        return await Task.FromResult(new List<UserSession>());
    }

    #endregion

    #region All Monitoring Data

    public async Task<MonitoringData> GetAllMonitoringDataAsync(CancellationToken ct = default)
    {
        var cacheKey = "monitoring:all";
        
        var cached = await _cache.GetAsync<MonitoringData>(cacheKey, ct);
        if (cached != null) return cached;

        var data = new MonitoringData
        {
            Timestamp = DateTime.UtcNow
        };

        // Run all tasks in parallel for performance
        var infraTask = GetInfrastructureInfoAsync(ct);
        var systemTask = GetSystemMetricsAsync(ct);
        var dbTask = GetDatabaseMetricsAsync(ct);
        var servicesTask = GetServiceHealthAsync(ct);
        var containersTask = GetContainerHealthAsync(ct);
        var podsTask = GetPodHealthAsync(ct);

        await Task.WhenAll(infraTask, systemTask, dbTask, servicesTask, containersTask, podsTask);

        data.Infrastructure = await infraTask;
        data.System = await systemTask;
        data.Database = await dbTask;
        data.Services = await servicesTask;
        data.Containers = await containersTask;
        data.Pods = await podsTask;

        await _cache.SetAsync(cacheKey, data, TimeSpan.FromSeconds(_options.CacheDurationSeconds), ct);

        return data;
    }

    #endregion

    #region Helpers

    private static long GetTotalPhysicalMemory()
    {
        try
        {
            var gcInfo = GC.GetGCMemoryInfo();
            if (gcInfo.TotalAvailableMemoryBytes > 0)
            {
                return gcInfo.TotalAvailableMemoryBytes;
            }
        }
        catch { }
        
        return 8L * 1024 * 1024 * 1024; // Default 8GB
    }

    private static string FormatUptime(TimeSpan uptime)
    {
        if (uptime.TotalDays >= 1)
            return $"{(int)uptime.TotalDays}d {uptime.Hours}h {uptime.Minutes}m";
        if (uptime.TotalHours >= 1)
            return $"{(int)uptime.TotalHours}h {uptime.Minutes}m";
        return $"{uptime.Minutes}m {uptime.Seconds}s";
    }

    private static string GetAssemblyVersion()
    {
        return typeof(MonitoringService).Assembly.GetName().Version?.ToString() ?? "1.0.0";
    }

    #endregion
}

#endregion
