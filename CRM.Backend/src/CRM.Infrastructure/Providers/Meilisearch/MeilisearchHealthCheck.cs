// CRM Solution - Pluggable Architecture
// Meilisearch Health Check
//
// ASP.NET Core Health Check for Meilisearch provider.

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Providers.Meilisearch;

/// <summary>
/// ASP.NET Core health check for Meilisearch connectivity.
/// </summary>
public class MeilisearchHealthCheck : IHealthCheck
{
    private readonly MeilisearchProvider _provider;
    private readonly ILogger<MeilisearchHealthCheck> _logger;

    public MeilisearchHealthCheck(
        MeilisearchProvider provider,
        ILogger<MeilisearchHealthCheck> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _provider.HealthCheckAsync(cancellationToken);

            if (result.IsHealthy)
            {
                return HealthCheckResult.Healthy(
                    result.Message,
                    data: result.Details.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value));
            }

            return HealthCheckResult.Degraded(
                result.Message,
                data: result.Details.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Meilisearch health check failed");
            
            return HealthCheckResult.Unhealthy(
                $"Meilisearch health check failed: {ex.Message}",
                exception: ex);
        }
    }
}
