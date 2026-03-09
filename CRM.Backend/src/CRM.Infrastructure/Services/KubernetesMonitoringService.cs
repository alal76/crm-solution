// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// AP-038: Extracted from MonitoringService.cs (god-class split)
// Contains IKubernetesMonitoringService interface and KubernetesMonitoringService.
// MonitoringService.GetPodHealthAsync now delegates to this class.

using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Services;

/// <summary>
/// AP-038: Interface for Kubernetes pod health monitoring.
/// Extracted from MonitoringService to separate K8s concerns.
/// </summary>
public interface IKubernetesMonitoringService
{
    /// <summary>Get health status of Kubernetes pods in the configured namespace</summary>
    Task<List<PodHealth>> GetPodHealthAsync(CancellationToken ct = default);
}

/// <summary>
/// AP-038: Kubernetes pod monitoring via kubectl.
/// Queries pods in the configured namespace and returns status.
/// Extracted from MonitoringService.cs to reduce god-class complexity.
/// </summary>
public class KubernetesMonitoringService : IKubernetesMonitoringService
{
    private readonly MonitoringOptions _options;
    private readonly ILogger<KubernetesMonitoringService> _logger;

    public KubernetesMonitoringService(
        IOptions<MonitoringOptions> options,
        ILogger<KubernetesMonitoringService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
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

            using var process = Process.Start(psi); // NOSONAR - fixed 'kubectl' executable name, no user-controlled input in executable path
            if (process == null)
            {
                return pods;
            }

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
}
