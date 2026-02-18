// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

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
