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
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Providers.Novu;

/// <summary>
/// ASP.NET Core health check for the Novu notification provider.
/// Verifies connectivity to the Novu API using HTTP client.
/// </summary>
public class NovuHealthCheck : IHealthCheck
{
    private readonly NovuConfiguration _config;
    private readonly HttpClient _httpClient;
    private readonly ILogger<NovuHealthCheck> _logger;

    public NovuHealthCheck(
        IOptions<NovuConfiguration> config,
        HttpClient httpClient,
        ILogger<NovuHealthCheck> logger)
    {
        _config = config.Value;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>
        {
            { "provider", "Novu" },
            { "url", _config.Url },
            { "useSelfHosted", _config.UseSelfHosted }
        };

        if (!_config.IsValid())
        {
            return HealthCheckResult.Unhealthy(
                "Novu is not configured - missing API key or URL",
                data: data);
        }

        try
        {
            // Try to get subscribers to verify connection
            var response = await _httpClient.GetAsync("v1/subscribers?page=0&limit=1", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("Novu health check passed");
                return HealthCheckResult.Healthy("Novu API is responding", data);
            }

            data["statusCode"] = (int)response.StatusCode;
            return HealthCheckResult.Degraded(
                $"Novu API returned status code: {response.StatusCode}",
                data: data);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Novu health check failed");
            data["error"] = ex.Message;

            return HealthCheckResult.Unhealthy(
                $"Novu API check failed: {ex.Message}",
                exception: ex,
                data: data);
        }
    }
}
