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
using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Services;

#region Configuration Options

/// <summary>
/// Monitoring configuration options from environment variables
/// </summary>
public class MonitoringOptions
{
    public const string SectionName = "Monitoring";
    
    /// <summary>Deployment type: docker, kubernetes, hybrid</summary>
    public string DeploymentType { get; set; } = "docker";
    
    /// <summary>Build server hostname/IP</summary>
    public string BuildServer { get; set; } = "localhost";
    
    /// <summary>Build server FQDN</summary>
    public string BuildServerFQDN { get; set; } = "";
    
    /// <summary>API endpoints to monitor (comma-separated)</summary>
    public string ApiEndpoints { get; set; } = "";
    
    /// <summary>Database servers to monitor (comma-separated)</summary>
    public string DatabaseServers { get; set; } = "";
    
    /// <summary>Frontend URLs to monitor (comma-separated)</summary>
    public string FrontendUrls { get; set; } = "";
    
    /// <summary>Redis endpoints to monitor (comma-separated)</summary>
    public string RedisEndpoints { get; set; } = "";
    
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
    
    /// <summary>Database provider: mariadb, sqlserver, postgresql</summary>
    public string DatabaseProvider { get; set; } = "mariadb";
    
    #region SQL Server Configuration
    
    /// <summary>SQL Server version (e.g., 2022-latest, 2019-latest)</summary>
    public string MssqlVersion { get; set; } = "2022-latest";
    
    /// <summary>SQL Server Edition (Express, Developer, Standard, Enterprise)</summary>
    public string MssqlEdition { get; set; } = "Express";
    
    /// <summary>Path to SQL Server tools (sqlcmd). /opt/mssql-tools18/bin for 2022, /opt/mssql-tools/bin for 2019</summary>
    public string MssqlToolsPath { get; set; } = "/opt/mssql-tools18/bin";
    
    /// <summary>SQL Server health check interval</summary>
    public string MssqlHealthInterval { get; set; } = "30s";
    
    /// <summary>SQL Server health check timeout</summary>
    public string MssqlHealthTimeout { get; set; } = "10s";
    
    /// <summary>SQL Server health check retries</summary>
    public string MssqlHealthRetries { get; set; } = "5";
    
    /// <summary>SQL Server start period before health checks begin</summary>
    public string MssqlStartPeriod { get; set; } = "60s";
    
    /// <summary>SQL Server collation</summary>
    public string MssqlCollation { get; set; } = "SQL_Latin1_General_CP1_CI_AS";
    
    #endregion
}

#endregion

#region DTOs

public class MonitoredEndpoint
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = ""; // api, database, frontend, redis, container, pod
    public string Endpoint { get; set; } = "";
    public string Host { get; set; } = "";
    public int Port { get; set; }
    public string FQDN { get; set; } = "";
    public string Status { get; set; } = "unknown"; // healthy, degraded, error, unknown
    public int ResponseTimeMs { get; set; }
    public string Version { get; set; } = "";
    public string Uptime { get; set; } = "";
    public DateTime LastCheck { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class ContainerHealth
{
    public string ContainerId { get; set; } = "";
    public string ContainerName { get; set; } = "";
    public string Image { get; set; } = "";
    public string Status { get; set; } = "";
    public string Health { get; set; } = "";
    public DateTime StartedAt { get; set; }
    public string Uptime { get; set; } = "";
    public ResourceUsage Resources { get; set; } = new();
}

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

public class ResourceUsage
{
    public double CpuPercent { get; set; }
    public long MemoryBytes { get; set; }
    public long MemoryLimitBytes { get; set; }
    public double MemoryPercent { get; set; }
    public long NetworkRxBytes { get; set; }
    public long NetworkTxBytes { get; set; }
}

public class SystemHealthSummary
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string DeploymentType { get; set; } = "";
    public string BuildServer { get; set; } = "";
    public string OverallStatus { get; set; } = "unknown";
    public int HealthyServices { get; set; }
    public int DegradedServices { get; set; }
    public int ErrorServices { get; set; }
    public int TotalServices { get; set; }
    public List<MonitoredEndpoint> Endpoints { get; set; } = new();
    public List<ContainerHealth> Containers { get; set; } = new();
    public List<PodHealth> Pods { get; set; } = new();
    public Dictionary<string, object> Metrics { get; set; } = new();
}

#endregion

/// <summary>
/// Interface for system monitoring service
/// </summary>
public interface IMonitoringService
{
    /// <summary>Get all monitored endpoints from configuration</summary>
    Task<List<MonitoredEndpoint>> DiscoverEndpointsAsync(CancellationToken ct = default);
    
    /// <summary>Check health of all discovered endpoints</summary>
    Task<List<MonitoredEndpoint>> CheckEndpointHealthAsync(CancellationToken ct = default);
    
    /// <summary>Get container health (Docker)</summary>
    Task<List<ContainerHealth>> GetContainerHealthAsync(CancellationToken ct = default);
    
    /// <summary>Get pod health (Kubernetes)</summary>
    Task<List<PodHealth>> GetPodHealthAsync(CancellationToken ct = default);
    
    /// <summary>Get complete system health summary</summary>
    Task<SystemHealthSummary> GetSystemHealthAsync(CancellationToken ct = default);
    
    /// <summary>Get API metrics</summary>
    Task<Dictionary<string, object>> GetApiMetricsAsync(string endpoint, CancellationToken ct = default);
    
    /// <summary>Get current monitoring options (deployment settings)</summary>
    MonitoringOptions GetMonitoringOptions();
}

/// <summary>
/// Comprehensive monitoring service with endpoint discovery and health checks
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

    /// <summary>
    /// Get current monitoring options (deployment settings from environment)
    /// </summary>
    public MonitoringOptions GetMonitoringOptions() => _options;

    public async Task<List<MonitoredEndpoint>> DiscoverEndpointsAsync(CancellationToken ct = default)
    {
        const string cacheKey = "monitoring:endpoints";
        
        var cached = await _cache.GetAsync<List<MonitoredEndpoint>>(cacheKey, ct);
        if (cached != null) return cached;
        
        var endpoints = new List<MonitoredEndpoint>();
        
        // Discover API endpoints
        endpoints.AddRange(DiscoverApiEndpoints());
        
        // Discover Database servers
        endpoints.AddRange(DiscoverDatabaseServers());
        
        // Discover Frontend URLs
        endpoints.AddRange(DiscoverFrontendUrls());
        
        // Discover Redis endpoints
        endpoints.AddRange(DiscoverRedisEndpoints());
        
        // Cache the discovered endpoints
        await _cache.SetAsync(cacheKey, endpoints, TimeSpan.FromMinutes(5), ct);
        
        return endpoints;
    }

    public async Task<List<MonitoredEndpoint>> CheckEndpointHealthAsync(CancellationToken ct = default)
    {
        var cacheKey = "monitoring:health";
        
        var cached = await _cache.GetAsync<List<MonitoredEndpoint>>(cacheKey, ct);
        if (cached != null) return cached;
        
        var endpoints = await DiscoverEndpointsAsync(ct);
        var healthTasks = endpoints.Select(async endpoint =>
        {
            try
            {
                var health = await CheckEndpointAsync(endpoint, ct);
                return health;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Health check failed for {Endpoint}", endpoint.Endpoint);
                endpoint.Status = "error";
                endpoint.LastCheck = DateTime.UtcNow;
                return endpoint;
            }
        });
        
        var results = await Task.WhenAll(healthTasks);
        var resultList = results.ToList();
        
        await _cache.SetAsync(cacheKey, resultList, TimeSpan.FromSeconds(_options.CacheDurationSeconds), ct);
        
        return resultList;
    }

    public async Task<List<ContainerHealth>> GetContainerHealthAsync(CancellationToken ct = default)
    {
        if (!_options.EnableDockerMonitoring) return new List<ContainerHealth>();
        
        var cacheKey = "monitoring:containers";
        var cached = await _cache.GetAsync<List<ContainerHealth>>(cacheKey, ct);
        if (cached != null) return cached;
        
        var containers = new List<ContainerHealth>();
        
        try
        {
            // Try to get Docker container info via Docker API or CLI
            var dockerHost = _options.BuildServer;
            var dockerApiUrl = $"http://{dockerHost}:2375/containers/json?all=true";
            
            try
            {
                var response = await _httpClient.GetAsync(dockerApiUrl, ct);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync(ct);
                    var containerInfos = JsonSerializer.Deserialize<List<JsonElement>>(json);
                    
                    if (containerInfos != null)
                    {
                        foreach (var info in containerInfos)
                        {
                            containers.Add(ParseDockerContainer(info));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Docker API not available, using fallback detection");
                // Fallback: detect containers from environment/configuration
                containers = GetContainersFromConfig();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get container health");
        }
        
        await _cache.SetAsync(cacheKey, containers, TimeSpan.FromSeconds(_options.CacheDurationSeconds), ct);
        return containers;
    }

    public async Task<List<PodHealth>> GetPodHealthAsync(CancellationToken ct = default)
    {
        if (!_options.EnableK8sMonitoring) return new List<PodHealth>();
        
        var cacheKey = "monitoring:pods";
        var cached = await _cache.GetAsync<List<PodHealth>>(cacheKey, ct);
        if (cached != null) return cached;
        
        var pods = new List<PodHealth>();
        
        try
        {
            // Try to access Kubernetes API
            var k8sApiUrl = "https://kubernetes.default.svc/api/v1";
            var ns = _options.KubernetesNamespace;
            
            // Check for in-cluster config
            var tokenPath = "/var/run/secrets/kubernetes.io/serviceaccount/token";
            if (File.Exists(tokenPath))
            {
                var token = await File.ReadAllTextAsync(tokenPath, ct);
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                
                var response = await _httpClient.GetAsync($"{k8sApiUrl}/namespaces/{ns}/pods", ct);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync(ct);
                    pods = ParseKubernetesPods(json);
                }
            }
            else
            {
                _logger.LogDebug("Not running in Kubernetes, skipping pod discovery");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get pod health");
        }
        
        await _cache.SetAsync(cacheKey, pods, TimeSpan.FromSeconds(_options.CacheDurationSeconds), ct);
        return pods;
    }

    public async Task<SystemHealthSummary> GetSystemHealthAsync(CancellationToken ct = default)
    {
        var cacheKey = "monitoring:summary";
        var cached = await _cache.GetAsync<SystemHealthSummary>(cacheKey, ct);
        if (cached != null) return cached;
        
        var endpoints = await CheckEndpointHealthAsync(ct);
        var containers = await GetContainerHealthAsync(ct);
        var pods = await GetPodHealthAsync(ct);
        
        var healthyCount = endpoints.Count(e => e.Status == "healthy");
        var degradedCount = endpoints.Count(e => e.Status == "degraded");
        var errorCount = endpoints.Count(e => e.Status == "error");
        
        string overallStatus;
        if (errorCount > 0) overallStatus = "error";
        else if (degradedCount > 0) overallStatus = "degraded";
        else overallStatus = "healthy";
        
        var summary = new SystemHealthSummary
        {
            Timestamp = DateTime.UtcNow,
            DeploymentType = _options.DeploymentType,
            BuildServer = !string.IsNullOrEmpty(_options.BuildServerFQDN) 
                ? _options.BuildServerFQDN 
                : _options.BuildServer,
            OverallStatus = overallStatus,
            HealthyServices = healthyCount,
            DegradedServices = degradedCount,
            ErrorServices = errorCount,
            TotalServices = endpoints.Count,
            Endpoints = endpoints,
            Containers = containers,
            Pods = pods,
            Metrics = GetSystemMetrics()
        };
        
        await _cache.SetAsync(cacheKey, summary, TimeSpan.FromSeconds(_options.CacheDurationSeconds), ct);
        return summary;
    }

    public async Task<Dictionary<string, object>> GetApiMetricsAsync(string endpoint, CancellationToken ct = default)
    {
        var metrics = new Dictionary<string, object>();
        
        try
        {
            var sw = Stopwatch.StartNew();
            var response = await _httpClient.GetAsync($"{endpoint}/health", ct);
            sw.Stop();
            
            metrics["responseTimeMs"] = sw.ElapsedMilliseconds;
            metrics["statusCode"] = (int)response.StatusCode;
            metrics["healthy"] = response.IsSuccessStatusCode;
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(ct);
                try
                {
                    var healthData = JsonSerializer.Deserialize<Dictionary<string, object>>(content);
                    if (healthData != null)
                    {
                        foreach (var kvp in healthData)
                        {
                            metrics[kvp.Key] = kvp.Value;
                        }
                    }
                }
                catch { /* Ignore parse errors */ }
            }
        }
        catch (Exception ex)
        {
            metrics["error"] = ex.Message;
            metrics["healthy"] = false;
        }
        
        return metrics;
    }

    #region Private Methods

    private List<MonitoredEndpoint> DiscoverApiEndpoints()
    {
        var endpoints = new List<MonitoredEndpoint>();
        
        // Get from configuration
        var configuredApis = _options.ApiEndpoints;
        if (!string.IsNullOrEmpty(configuredApis))
        {
            foreach (var api in configuredApis.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = api.Trim().Split('|');
                var url = parts[0].Trim();
                var name = parts.Length > 1 ? parts[1].Trim() : ExtractHostFromUrl(url);
                
                endpoints.Add(CreateEndpointFromUrl(url, name, "api"));
            }
        }
        
        // Auto-discover from environment
        var apiUrl = _configuration["REACT_APP_API_URL"] 
            ?? _configuration["API_URL"] 
            ?? $"http://{_options.BuildServer}:5000/api";
        
        if (!endpoints.Any(e => e.Endpoint.Contains(apiUrl)))
        {
            endpoints.Add(CreateEndpointFromUrl(apiUrl, "CRM API", "api"));
        }
        
        return endpoints;
    }

    private List<MonitoredEndpoint> DiscoverDatabaseServers()
    {
        var endpoints = new List<MonitoredEndpoint>();
        
        // Get from configuration
        var dbServers = _options.DatabaseServers;
        if (!string.IsNullOrEmpty(dbServers))
        {
            foreach (var db in dbServers.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = db.Trim().Split('|');
                var hostPort = parts[0].Trim();
                var name = parts.Length > 1 ? parts[1].Trim() : $"Database ({hostPort})";
                
                var (host, port) = ParseHostPort(hostPort, GetDefaultDbPort());
                
                endpoints.Add(new MonitoredEndpoint
                {
                    Name = name,
                    Type = "database",
                    Host = host,
                    Port = port,
                    Endpoint = $"{host}:{port}",
                    FQDN = ResolveFQDN(host),
                    Metadata = new Dictionary<string, object>
                    {
                        ["provider"] = _options.DatabaseProvider
                    }
                });
            }
        }
        
        // Auto-discover from connection string
        var connString = _configuration.GetConnectionString("DefaultConnection") 
            ?? _configuration["DB_HOST"];
        
        if (!string.IsNullOrEmpty(connString))
        {
            var dbHost = ExtractDbHostFromConnectionString(connString);
            var dbPort = ExtractDbPortFromConnectionString(connString);
            
            if (!endpoints.Any(e => e.Host == dbHost))
            {
                endpoints.Add(new MonitoredEndpoint
                {
                    Name = $"{_options.DatabaseProvider.ToUpper()} Database",
                    Type = "database",
                    Host = dbHost,
                    Port = dbPort,
                    Endpoint = $"{dbHost}:{dbPort}",
                    FQDN = ResolveFQDN(dbHost),
                    Metadata = new Dictionary<string, object>
                    {
                        ["provider"] = _options.DatabaseProvider
                    }
                });
            }
        }
        
        return endpoints;
    }

    private List<MonitoredEndpoint> DiscoverFrontendUrls()
    {
        var endpoints = new List<MonitoredEndpoint>();
        
        var frontendUrls = _options.FrontendUrls;
        if (!string.IsNullOrEmpty(frontendUrls))
        {
            foreach (var url in frontendUrls.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                endpoints.Add(CreateEndpointFromUrl(url.Trim(), "CRM Frontend", "frontend"));
            }
        }
        
        // Auto-discover
        var frontendUrl = _configuration["Frontend:Url"] 
            ?? $"http://{_options.BuildServer}";
        
        if (!endpoints.Any(e => e.Endpoint.Contains(frontendUrl)))
        {
            endpoints.Add(CreateEndpointFromUrl(frontendUrl, "CRM Frontend", "frontend"));
        }
        
        return endpoints;
    }

    private List<MonitoredEndpoint> DiscoverRedisEndpoints()
    {
        var endpoints = new List<MonitoredEndpoint>();
        
        var redisEndpoints = _options.RedisEndpoints;
        if (!string.IsNullOrEmpty(redisEndpoints))
        {
            foreach (var redis in redisEndpoints.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var (host, port) = ParseHostPort(redis.Trim(), 6379);
                endpoints.Add(new MonitoredEndpoint
                {
                    Name = "Redis Cache",
                    Type = "redis",
                    Host = host,
                    Port = port,
                    Endpoint = $"{host}:{port}",
                    FQDN = ResolveFQDN(host)
                });
            }
        }
        
        // Auto-discover from config
        var redisConn = _configuration["Redis:ConnectionString"];
        if (!string.IsNullOrEmpty(redisConn))
        {
            var (host, port) = ParseHostPort(redisConn.Split(',')[0], 6379);
            if (!endpoints.Any(e => e.Host == host))
            {
                endpoints.Add(new MonitoredEndpoint
                {
                    Name = "Redis Cache",
                    Type = "redis",
                    Host = host,
                    Port = port,
                    Endpoint = $"{host}:{port}",
                    FQDN = ResolveFQDN(host)
                });
            }
        }
        
        return endpoints;
    }

    private async Task<MonitoredEndpoint> CheckEndpointAsync(MonitoredEndpoint endpoint, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        
        try
        {
            switch (endpoint.Type)
            {
                case "api":
                case "frontend":
                    await CheckHttpEndpointAsync(endpoint, ct);
                    break;
                case "database":
                    await CheckDatabaseEndpointAsync(endpoint, ct);
                    break;
                case "redis":
                    await CheckRedisEndpointAsync(endpoint, ct);
                    break;
                default:
                    await CheckTcpEndpointAsync(endpoint, ct);
                    break;
            }
        }
        catch (Exception ex)
        {
            endpoint.Status = "error";
            endpoint.Metadata["error"] = ex.Message;
        }
        
        sw.Stop();
        endpoint.ResponseTimeMs = (int)sw.ElapsedMilliseconds;
        endpoint.LastCheck = DateTime.UtcNow;
        
        return endpoint;
    }

    private async Task CheckHttpEndpointAsync(MonitoredEndpoint endpoint, CancellationToken ct)
    {
        var url = endpoint.Endpoint.StartsWith("http") 
            ? endpoint.Endpoint 
            : $"http://{endpoint.Endpoint}";
        
        var healthUrl = endpoint.Type == "api" 
            ? url.Replace("/api", "/health")
            : url;
        
        var response = await _httpClient.GetAsync(healthUrl, ct);
        endpoint.Status = response.IsSuccessStatusCode ? "healthy" : "degraded";
        endpoint.Metadata["statusCode"] = (int)response.StatusCode;
    }

    private async Task CheckDatabaseEndpointAsync(MonitoredEndpoint endpoint, CancellationToken ct)
    {
        // TCP port check for database
        await CheckTcpEndpointAsync(endpoint, ct);
    }

    private async Task CheckRedisEndpointAsync(MonitoredEndpoint endpoint, CancellationToken ct)
    {
        await CheckTcpEndpointAsync(endpoint, ct);
    }

    private async Task CheckTcpEndpointAsync(MonitoredEndpoint endpoint, CancellationToken ct)
    {
        using var tcpClient = new System.Net.Sockets.TcpClient();
        var connectTask = tcpClient.ConnectAsync(endpoint.Host, endpoint.Port);
        var timeoutTask = Task.Delay(_options.HealthCheckTimeoutSeconds * 1000, ct);
        
        if (await Task.WhenAny(connectTask, timeoutTask) == connectTask)
        {
            await connectTask; // Propagate exception if any
            endpoint.Status = "healthy";
        }
        else
        {
            endpoint.Status = "error";
            endpoint.Metadata["error"] = "Connection timeout";
        }
    }

    private MonitoredEndpoint CreateEndpointFromUrl(string url, string name, string type)
    {
        var uri = new Uri(url.StartsWith("http") ? url : $"http://{url}");
        return new MonitoredEndpoint
        {
            Name = name,
            Type = type,
            Host = uri.Host,
            Port = uri.Port,
            Endpoint = url,
            FQDN = ResolveFQDN(uri.Host)
        };
    }

    private (string host, int port) ParseHostPort(string hostPort, int defaultPort)
    {
        var parts = hostPort.Split(':');
        var host = parts[0];
        var port = parts.Length > 1 && int.TryParse(parts[1], out var p) ? p : defaultPort;
        return (host, port);
    }

    private int GetDefaultDbPort() => _options.DatabaseProvider.ToLower() switch
    {
        "sqlserver" => 1433,
        "postgresql" => 5432,
        "mariadb" => 3306,
        "mysql" => 3306,
        _ => 3306
    };

    private string ExtractHostFromUrl(string url)
    {
        try
        {
            var uri = new Uri(url.StartsWith("http") ? url : $"http://{url}");
            return uri.Host;
        }
        catch
        {
            return url;
        }
    }

    private string ExtractDbHostFromConnectionString(string connString)
    {
        // Parse Server=host or Host=host or Data Source=host
        var patterns = new[] { "Server=", "Host=", "Data Source=" };
        foreach (var pattern in patterns)
        {
            var idx = connString.IndexOf(pattern, StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
            {
                var start = idx + pattern.Length;
                var end = connString.IndexOfAny(new[] { ';', ',' }, start);
                var hostPart = end > 0 
                    ? connString.Substring(start, end - start) 
                    : connString.Substring(start);
                return hostPart.Split(':')[0].Split(',')[0];
            }
        }
        return _options.BuildServer;
    }

    private int ExtractDbPortFromConnectionString(string connString)
    {
        // Parse Port= or :port after host
        var portPattern = "Port=";
        var idx = connString.IndexOf(portPattern, StringComparison.OrdinalIgnoreCase);
        if (idx >= 0)
        {
            var start = idx + portPattern.Length;
            var end = connString.IndexOf(';', start);
            var portStr = end > 0 
                ? connString.Substring(start, end - start) 
                : connString.Substring(start);
            if (int.TryParse(portStr, out var port)) return port;
        }
        return GetDefaultDbPort();
    }

    private string ResolveFQDN(string host)
    {
        try
        {
            if (System.Net.IPAddress.TryParse(host, out _))
            {
                // It's an IP, try reverse DNS
                var entry = System.Net.Dns.GetHostEntry(host);
                return entry.HostName;
            }
            return host;
        }
        catch
        {
            return host;
        }
    }

    private ContainerHealth ParseDockerContainer(JsonElement info)
    {
        var container = new ContainerHealth();
        
        try
        {
            container.ContainerId = info.GetProperty("Id").GetString()?.Substring(0, 12) ?? "";
            
            var names = info.GetProperty("Names");
            container.ContainerName = names.GetArrayLength() > 0 
                ? names[0].GetString()?.TrimStart('/') ?? "" 
                : "";
            
            container.Image = info.GetProperty("Image").GetString() ?? "";
            container.Status = info.GetProperty("State").GetString() ?? "";
            
            if (info.TryGetProperty("Status", out var statusProp))
            {
                var status = statusProp.GetString() ?? "";
                container.Health = status.Contains("healthy") ? "healthy" 
                    : status.Contains("unhealthy") ? "unhealthy" 
                    : "unknown";
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error parsing Docker container info");
        }
        
        return container;
    }

    private List<ContainerHealth> GetContainersFromConfig()
    {
        // Fallback: return expected containers based on deployment type
        var containers = new List<ContainerHealth>();
        
        if (_options.DeploymentType == "docker" || _options.DeploymentType == "hybrid")
        {
            containers.Add(new ContainerHealth { ContainerName = "crm-api", Status = "running" });
            containers.Add(new ContainerHealth { ContainerName = "crm-frontend", Status = "running" });
            containers.Add(new ContainerHealth { ContainerName = "crm-mariadb", Status = "running" });
            containers.Add(new ContainerHealth { ContainerName = "crm-redis", Status = "running" });
        }
        
        return containers;
    }

    private List<PodHealth> ParseKubernetesPods(string json)
    {
        var pods = new List<PodHealth>();
        
        try
        {
            using var doc = JsonDocument.Parse(json);
            var items = doc.RootElement.GetProperty("items");
            
            foreach (var item in items.EnumerateArray())
            {
                var pod = new PodHealth
                {
                    PodName = item.GetProperty("metadata").GetProperty("name").GetString() ?? "",
                    Namespace = item.GetProperty("metadata").GetProperty("namespace").GetString() ?? "",
                    Phase = item.GetProperty("status").GetProperty("phase").GetString() ?? "",
                };
                
                if (item.GetProperty("status").TryGetProperty("conditions", out var conditions))
                {
                    foreach (var cond in conditions.EnumerateArray())
                    {
                        if (cond.GetProperty("type").GetString() == "Ready")
                        {
                            pod.Ready = cond.GetProperty("status").GetString() == "True";
                        }
                    }
                }
                
                pods.Add(pod);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error parsing Kubernetes pods");
        }
        
        return pods;
    }

    private Dictionary<string, object> GetSystemMetrics()
    {
        var process = Process.GetCurrentProcess();
        var uptime = DateTime.UtcNow - _startTime;
        
        return new Dictionary<string, object>
        {
            ["timestamp"] = DateTime.UtcNow,
            ["uptime"] = FormatUptime(uptime),
            ["uptimeSeconds"] = (long)uptime.TotalSeconds,
            ["cpuTimeMs"] = (long)process.TotalProcessorTime.TotalMilliseconds,
            ["memoryMB"] = process.WorkingSet64 / (1024 * 1024),
            ["threadCount"] = process.Threads.Count,
            ["handleCount"] = process.HandleCount,
            ["machineName"] = Environment.MachineName,
            ["osDescription"] = RuntimeInformation.OSDescription,
            ["processorCount"] = Environment.ProcessorCount,
            ["dotNetVersion"] = RuntimeInformation.FrameworkDescription
        };
    }

    private static string FormatUptime(TimeSpan uptime)
    {
        if (uptime.TotalDays >= 1)
            return $"{(int)uptime.TotalDays}d {uptime.Hours}h {uptime.Minutes}m";
        if (uptime.TotalHours >= 1)
            return $"{(int)uptime.TotalHours}h {uptime.Minutes}m";
        return $"{uptime.Minutes}m {uptime.Seconds}s";
    }

    #endregion
}
