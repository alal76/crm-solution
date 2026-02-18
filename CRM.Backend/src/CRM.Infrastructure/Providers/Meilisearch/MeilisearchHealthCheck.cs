// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

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
