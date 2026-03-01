// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
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
            {
                return HealthCheckResult.Healthy("Algolia search is reachable");
            }
            return HealthCheckResult.Degraded("Algolia search is not responding");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Algolia health check failed");
            return HealthCheckResult.Unhealthy("Algolia health check failed", ex);
        }
    }
}
