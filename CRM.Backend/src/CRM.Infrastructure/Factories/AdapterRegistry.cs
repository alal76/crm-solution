// CRM Solution - Adapter Registry
// Phase 0 Week 3 Task 3.8: Health monitoring registry for all providers
// Part of the Pluggable Architecture implementation

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using HealthChecks = Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CRM.Infrastructure.Factories;

/// <summary>
/// Central registry for tracking provider adapter status and health.
/// Provides health monitoring, status tracking, and metrics for all pluggable providers.
/// </summary>
public class AdapterRegistry
{
    private readonly ConcurrentDictionary<string, AdapterInfo> _adapters = new();
    private readonly ILogger<AdapterRegistry> _logger;

    public AdapterRegistry(ILogger<AdapterRegistry> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Registers a provider adapter with the registry.
    /// </summary>
    /// <param name="category">Provider category (e.g., "Search", "Chat", "Notifications")</param>
    /// <param name="providerName">Provider name (e.g., "Meilisearch", "Chatwoot")</param>
    /// <param name="isActive">Whether this is the currently active provider for the category</param>
    public void Register(string category, string providerName, bool isActive = false)
    {
        var key = GetKey(category, providerName);
        var info = new AdapterInfo
        {
            Category = category,
            ProviderName = providerName,
            IsActive = isActive,
            RegisteredAt = DateTime.UtcNow,
            Status = AdapterStatus.Unknown
        };

        _adapters.AddOrUpdate(key, info, (_, existing) =>
        {
            existing.IsActive = isActive;
            return existing;
        });

        _logger.LogInformation("Registered adapter {Category}:{ProviderName}, Active: {IsActive}", 
            category, providerName, isActive);
    }

    /// <summary>
    /// Sets the active provider for a category.
    /// </summary>
    public void SetActive(string category, string providerName)
    {
        // Deactivate all providers in the category
        foreach (var kvp in _adapters.Where(a => a.Value.Category == category))
        {
            kvp.Value.IsActive = false;
        }

        // Activate the specified provider
        var key = GetKey(category, providerName);
        if (_adapters.TryGetValue(key, out var info))
        {
            info.IsActive = true;
            _logger.LogInformation("Set active adapter for {Category}: {ProviderName}", category, providerName);
        }
    }

    /// <summary>
    /// Updates the health status of a provider.
    /// </summary>
    public void UpdateHealth(string category, string providerName, HealthChecks.HealthCheckResult result)
    {
        var key = GetKey(category, providerName);
        if (_adapters.TryGetValue(key, out var info))
        {
            info.LastHealthCheck = DateTime.UtcNow;
            info.Status = result.Status switch
            {
                HealthChecks.HealthStatus.Healthy => AdapterStatus.Healthy,
                HealthChecks.HealthStatus.Degraded => AdapterStatus.Degraded,
                HealthChecks.HealthStatus.Unhealthy => AdapterStatus.Unhealthy,
                _ => AdapterStatus.Unknown
            };
            info.LastHealthMessage = result.Description;
            info.HealthCheckCount++;

            if (result.Status != HealthChecks.HealthStatus.Healthy)
            {
                info.FailureCount++;
                info.LastFailureTime = DateTime.UtcNow;
            }

            _logger.LogDebug("Updated health for {Category}:{ProviderName}: {Status}", 
                category, providerName, info.Status);
        }
    }

    /// <summary>
    /// Records a successful operation for metrics tracking.
    /// </summary>
    public void RecordSuccess(string category, string providerName, TimeSpan duration)
    {
        var key = GetKey(category, providerName);
        if (_adapters.TryGetValue(key, out var info))
        {
            info.SuccessCount++;
            info.TotalOperationTime += duration;
            info.LastOperationTime = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Records a failed operation for metrics tracking.
    /// </summary>
    public void RecordFailure(string category, string providerName, string? errorMessage = null)
    {
        var key = GetKey(category, providerName);
        if (_adapters.TryGetValue(key, out var info))
        {
            info.FailureCount++;
            info.LastFailureTime = DateTime.UtcNow;
            info.LastFailureMessage = errorMessage;
        }
    }

    /// <summary>
    /// Gets all registered adapters.
    /// </summary>
    public IEnumerable<AdapterInfo> GetAllAdapters()
    {
        return _adapters.Values.ToList();
    }

    /// <summary>
    /// Gets all adapters in a specific category.
    /// </summary>
    public IEnumerable<AdapterInfo> GetAdaptersByCategory(string category)
    {
        return _adapters.Values.Where(a => a.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    /// <summary>
    /// Gets the active adapter for a category.
    /// </summary>
    public AdapterInfo? GetActiveAdapter(string category)
    {
        return _adapters.Values.FirstOrDefault(a => 
            a.Category.Equals(category, StringComparison.OrdinalIgnoreCase) && a.IsActive);
    }

    /// <summary>
    /// Gets a specific adapter's info.
    /// </summary>
    public AdapterInfo? GetAdapter(string category, string providerName)
    {
        var key = GetKey(category, providerName);
        return _adapters.TryGetValue(key, out var info) ? info : null;
    }

    /// <summary>
    /// Gets health summary for all adapters.
    /// </summary>
    public AdapterHealthSummary GetHealthSummary()
    {
        var adapters = _adapters.Values.ToList();
        return new AdapterHealthSummary
        {
            TotalAdapters = adapters.Count,
            HealthyCount = adapters.Count(a => a.Status == AdapterStatus.Healthy),
            DegradedCount = adapters.Count(a => a.Status == AdapterStatus.Degraded),
            UnhealthyCount = adapters.Count(a => a.Status == AdapterStatus.Unhealthy),
            UnknownCount = adapters.Count(a => a.Status == AdapterStatus.Unknown),
            ActiveAdapters = adapters.Where(a => a.IsActive).Select(a => $"{a.Category}:{a.ProviderName}").ToList(),
            LastUpdated = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Performs health checks on all registered adapters using the provided check function.
    /// </summary>
    public async Task<Dictionary<string, HealthChecks.HealthCheckResult>> HealthCheckAllAsync(
        Func<string, string, Task<HealthChecks.HealthCheckResult>> healthCheckFunc)
    {
        var results = new Dictionary<string, HealthChecks.HealthCheckResult>();

        foreach (var adapter in _adapters.Values)
        {
            var key = GetKey(adapter.Category, adapter.ProviderName);
            try
            {
                var result = await healthCheckFunc(adapter.Category, adapter.ProviderName);
                results[key] = result;
                UpdateHealth(adapter.Category, adapter.ProviderName, result);
            }
            catch (Exception ex)
            {
                var result = HealthChecks.HealthCheckResult.Unhealthy($"Health check failed: {ex.Message}");
                results[key] = result;
                UpdateHealth(adapter.Category, adapter.ProviderName, result);
            }
        }

        return results;
    }

    private static string GetKey(string category, string providerName)
    {
        return $"{category}:{providerName}".ToLowerInvariant();
    }
}

/// <summary>
/// Information about a registered provider adapter.
/// </summary>
public class AdapterInfo
{
    public string Category { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public AdapterStatus Status { get; set; } = AdapterStatus.Unknown;
    public DateTime RegisteredAt { get; set; }
    public DateTime? LastHealthCheck { get; set; }
    public string? LastHealthMessage { get; set; }
    public DateTime? LastOperationTime { get; set; }
    public DateTime? LastFailureTime { get; set; }
    public string? LastFailureMessage { get; set; }
    public long SuccessCount { get; set; }
    public long FailureCount { get; set; }
    public long HealthCheckCount { get; set; }
    public TimeSpan TotalOperationTime { get; set; }

    /// <summary>
    /// Gets the average operation time in milliseconds.
    /// </summary>
    public double AverageOperationTimeMs => 
        SuccessCount > 0 ? TotalOperationTime.TotalMilliseconds / SuccessCount : 0;

    /// <summary>
    /// Gets the success rate as a percentage.
    /// </summary>
    public double SuccessRate => 
        (SuccessCount + FailureCount) > 0 
            ? (double)SuccessCount / (SuccessCount + FailureCount) * 100 
            : 0;
}

/// <summary>
/// Health status of a provider adapter.
/// </summary>
public enum AdapterStatus
{
    Unknown,
    Healthy,
    Degraded,
    Unhealthy
}

/// <summary>
/// Summary of adapter health across all categories.
/// </summary>
public class AdapterHealthSummary
{
    public int TotalAdapters { get; set; }
    public int HealthyCount { get; set; }
    public int DegradedCount { get; set; }
    public int UnhealthyCount { get; set; }
    public int UnknownCount { get; set; }
    public List<string> ActiveAdapters { get; set; } = new();
    public DateTime LastUpdated { get; set; }

    public bool IsOverallHealthy => UnhealthyCount == 0 && DegradedCount == 0;
}
