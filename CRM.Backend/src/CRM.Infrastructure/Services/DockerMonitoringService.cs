// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// AP-038: Extracted from MonitoringService.cs (god-class split)
// Contains IDockerMonitoringService interface and DockerMonitoringService implementation.
// MonitoringService.GetContainerHealthAsync now delegates to this class.

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Services;

/// <summary>
/// AP-038: Interface for Docker container health monitoring.
/// Extracted from MonitoringService to separate Docker concerns.
/// </summary>
public interface IDockerMonitoringService
{
    /// <summary>Get health status of all Docker containers</summary>
    Task<List<ContainerHealth>> GetContainerHealthAsync(CancellationToken ct = default);
}

/// <summary>
/// AP-038: Docker container monitoring.
/// Executes docker ps to retrieve container state; falls back to self-reporting
/// when running inside a container without socket access.
/// Extracted from MonitoringService.cs to reduce god-class complexity.
/// </summary>
public class DockerMonitoringService : IDockerMonitoringService
{
    private readonly MonitoringOptions _options;
    private readonly ILogger<DockerMonitoringService> _logger;
    private static readonly DateTime _startTime = DateTime.UtcNow; // AP-038: service start time for uptime calculation

    public DockerMonitoringService(
        IOptions<MonitoringOptions> options,
        ILogger<DockerMonitoringService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<List<ContainerHealth>> GetContainerHealthAsync(CancellationToken ct = default)
    {
        if (!_options.EnableDockerMonitoring && !File.Exists("/.dockerenv"))
        {
            return new List<ContainerHealth>();
        }

        var containers = new List<ContainerHealth>();

        try
        {
            var dockerSocket = "/var/run/docker.sock";
            if (File.Exists(dockerSocket))
            {
                containers = await GetDockerContainersAsync(ct);
            }
            else
            {
                // Fallback: report self when running inside a container without socket
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
            var psi = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = "ps --format \"{{.ID}}|{{.Names}}|{{.Image}}|{{.Status}}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi); // NOSONAR - fixed 'docker' executable name, no user-controlled input in executable path
            if (process == null)
            {
                return containers;
            }

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

    private static string FormatUptime(TimeSpan uptime)
    {
        if (uptime.TotalDays >= 1)
        {
            return $"{(int)uptime.TotalDays}d {uptime.Hours}h {uptime.Minutes}m";
        }
        if (uptime.TotalHours >= 1)
        {
            return $"{(int)uptime.TotalHours}h {uptime.Minutes}m";
        }
        return $"{uptime.Minutes}m {uptime.Seconds}s";
    }
}
