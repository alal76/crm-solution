// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Lightweight ProviderHealthService implementation that returns safe defaults.
/// The full implementation lives in ProviderHealthService.cs.disabled and depends
/// on AdapterRegistry. This stub satisfies the IProviderHealthService contract so
/// that AdminDashboardService and AdminDashboardController can resolve via DI
/// without requiring the full pluggable-provider infrastructure.
/// </summary>
public class ProviderHealthService : IProviderHealthService
{
    private readonly ILogger<ProviderHealthService> _logger;
    private int _healthCheckIntervalSeconds = 300;

    public ProviderHealthService(ILogger<ProviderHealthService> logger)
    {
        _logger = logger;
    }

    #region Individual Provider Health

    public Task<ProviderHealthDto> GetProviderHealthAsync(string providerCategory, string providerName, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ProviderHealthDto
        {
            Category = providerCategory,
            ProviderName = providerName,
            DisplayName = providerName,
            Status = (int)ProviderHealthStatus.NotConfigured,
            StatusText = ProviderHealthStatus.NotConfigured.ToString(),
            LastCheckedAt = DateTime.UtcNow,
            IsConfigured = false
        });
    }

    public Task<IEnumerable<ProviderHealthDto>> GetCategoryProvidersHealthAsync(string category, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Enumerable.Empty<ProviderHealthDto>());
    }

    #endregion

    #region All Providers Health

    public Task<IDictionary<string, IEnumerable<ProviderHealthDto>>> GetAllProvidersHealthAsync(CancellationToken cancellationToken = default)
    {
        IDictionary<string, IEnumerable<ProviderHealthDto>> result = new Dictionary<string, IEnumerable<ProviderHealthDto>>();
        return Task.FromResult(result);
    }

    public Task<ProviderHealthDashboardDto> GetProviderHealthDashboardAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ProviderHealthDashboardDto { LastRefreshAt = DateTime.UtcNow });
    }

    #endregion

    #region Health Checks

    public Task<ProviderHealthCheckDetailDto> PerformProviderHealthCheckAsync(string providerCategory, string providerName, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ProviderHealthCheckDetailDto
        {
            Category = providerCategory,
            ProviderName = providerName,
            Status = (int)ProviderHealthStatus.NotConfigured,
            StatusMessage = "Stub provider — full health checks disabled",
            CheckedAt = DateTime.UtcNow,
            ResponseTimeMs = 0,
            Diagnostics = new Dictionary<string, object>(),
            Warnings = new List<string>(),
            Errors = new List<string>()
        });
    }

    public Task<IDictionary<string, IEnumerable<ProviderHealthCheckDetailDto>>> PerformAllHealthChecksAsync(CancellationToken cancellationToken = default)
    {
        IDictionary<string, IEnumerable<ProviderHealthCheckDetailDto>> result = new Dictionary<string, IEnumerable<ProviderHealthCheckDetailDto>>();
        return Task.FromResult(result);
    }

    #endregion

    #region Performance Metrics

    public Task<ProviderPerformanceMetricsDto> GetProviderPerformanceMetricsAsync(string providerCategory, string providerName, int hoursBack = 24, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ProviderPerformanceMetricsDto
        {
            ProviderName = providerName,
            AverageResponseTimeMs = 0,
            MaxResponseTimeMs = 0,
            MinResponseTimeMs = 0,
            ErrorRatePercent = 0,
            TotalRequests = 0,
            FailedRequests = 0,
            ThroughputRequestsPerSecond = 0,
            MeasurementStart = DateTime.UtcNow.AddHours(-hoursBack),
            MeasurementEnd = DateTime.UtcNow
        });
    }

    public Task<IDictionary<string, ProviderPerformanceMetricsDto>> GetAllPerformanceMetricsAsync(int hoursBack = 24, CancellationToken cancellationToken = default)
    {
        IDictionary<string, ProviderPerformanceMetricsDto> result = new Dictionary<string, ProviderPerformanceMetricsDto>();
        return Task.FromResult(result);
    }

    #endregion

    #region Fallback & Redundancy

    public Task<IEnumerable<string>> GetFallbackProvidersAsync(string category, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Enumerable.Empty<string>());
    }

    public Task<string?> GetAvailableFallbackProviderAsync(string category, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<string?>(null);
    }

    #endregion

    #region Alerts & Notifications

    public Task<IEnumerable<ProviderHealthAlertDto>> GetProviderHealthAlertsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Enumerable.Empty<ProviderHealthAlertDto>());
    }

    public Task<bool> HasCriticalProvidersAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }

    public Task<IEnumerable<string>> GetProvidersWithIssuesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Enumerable.Empty<string>());
    }

    #endregion

    #region Configuration & Cache

    public Task RefreshProviderHealthCacheAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Provider health cache refresh requested (stub — no-op)");
        return Task.CompletedTask;
    }

    public void SetHealthCheckIntervalSeconds(int intervalSeconds)
    {
        _healthCheckIntervalSeconds = intervalSeconds;
    }

    #endregion
}
