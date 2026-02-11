// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

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
