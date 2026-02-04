// CRM Solution - Pluggable Architecture
// Algolia Health Check

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Providers.Algolia;

public class AlgoliaHealthCheck : IHealthCheck
{
    private readonly AlgoliaProvider _provider;
    private readonly ILogger<AlgoliaHealthCheck> _logger;

    public AlgoliaHealthCheck(AlgoliaProvider provider, ILogger<AlgoliaHealthCheck> logger)
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
            var isAvailable = await _provider.IsAvailableAsync();
            if (isAvailable)
                return HealthCheckResult.Healthy("Algolia search is reachable");
            return HealthCheckResult.Degraded("Algolia search is not responding");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Algolia health check failed");
            return HealthCheckResult.Unhealthy("Algolia health check failed", ex);
        }
    }
}
