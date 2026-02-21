// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service for monitoring and reporting health status of all pluggable providers.
///
/// HEXAGONAL ARCHITECTURE:
/// - Port: Defines contract for provider health monitoring
/// - Accessed by: AdminDashboardService, AdminDashboardPage
/// - Depends on: Provider factories, health check ports
///
/// PROVIDER CATEGORIES:
/// - Search (Meilisearch, Algolia, Elasticsearch, etc.)
/// - Chat (Chatwoot, Intercom, etc.)
/// - Notifications (Novu, Twilio, SendGrid, etc.)
/// - Analytics (Superset, Metabase, PowerBI, etc.)
/// - Signatures (DocuSeal, DocuSign, etc.)
/// - AI (Ollama, OpenAI, Azure, Anthropic, etc.)
/// - Integrations (N8n, Zapier, etc.)
/// </summary>
public interface IProviderHealthService
{
    #region Individual Provider Health

    /// <summary>
    /// Get health status of a specific provider.
    /// </summary>
    /// <param name="providerCategory">Provider category (e.g., "Search", "Chat")</param>
    /// <param name="providerName">Provider name (e.g., "Meilisearch")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Provider health DTO with status and metrics</returns>
    Task<ProviderHealthDto> GetProviderHealthAsync(string providerCategory, string providerName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get health status of all providers in a category.
    /// </summary>
    /// <param name="category">Provider category (Search, Chat, Notifications, etc.)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of provider health DTOs</returns>
    Task<IEnumerable<ProviderHealthDto>> GetCategoryProvidersHealthAsync(string category, CancellationToken cancellationToken = default);

    #endregion

    #region All Providers Health

    /// <summary>
    /// Get health status of all configured providers.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary of category to provider health list</returns>
    Task<IDictionary<string, IEnumerable<ProviderHealthDto>>> GetAllProvidersHealthAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get aggregated health dashboard data.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Provider health dashboard DTO</returns>
    Task<ProviderHealthDashboardDto> GetProviderHealthDashboardAsync(CancellationToken cancellationToken = default);

    #endregion

    #region Health Checks

    /// <summary>
    /// Perform a health check for a specific provider.
    /// Returns detailed diagnostics.
    /// </summary>
    /// <param name="providerCategory">Provider category</param>
    /// <param name="providerName">Provider name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Detailed health check result</returns>
    Task<ProviderHealthCheckDetailDto> PerformProviderHealthCheckAsync(string providerCategory, string providerName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Perform health checks for all providers.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary of category to detailed health results</returns>
    Task<IDictionary<string, IEnumerable<ProviderHealthCheckDetailDto>>> PerformAllHealthChecksAsync(CancellationToken cancellationToken = default);

    #endregion

    #region Performance Metrics

    /// <summary>
    /// Get performance metrics for a provider.
    /// Includes response times, error rates, throughput.
    /// </summary>
    /// <param name="providerCategory">Provider category</param>
    /// <param name="providerName">Provider name</param>
    /// <param name="hoursBack">Number of hours to look back in metrics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Performance metrics DTO</returns>
    Task<ProviderPerformanceMetricsDto> GetProviderPerformanceMetricsAsync(string providerCategory, string providerName, int hoursBack = 24, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get performance metrics for all providers.
    /// </summary>
    /// <param name="hoursBack">Number of hours to look back</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary of provider to performance metrics</returns>
    Task<IDictionary<string, ProviderPerformanceMetricsDto>> GetAllPerformanceMetricsAsync(int hoursBack = 24, CancellationToken cancellationToken = default);

    #endregion

    #region Fallback & Redundancy

    /// <summary>
    /// Get available fallback providers for a category.
    /// Returns configured fallback chain.
    /// </summary>
    /// <param name="category">Provider category</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of fallback provider names in order</returns>
    Task<IEnumerable<string>> GetFallbackProvidersAsync(string category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a fallback provider is available.
    /// </summary>
    /// <param name="category">Provider category</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Fallback provider name if available, null otherwise</returns>
    Task<string?> GetAvailableFallbackProviderAsync(string category, CancellationToken cancellationToken = default);

    #endregion

    #region Alerts & Notifications

    /// <summary>
    /// Get health alerts for providers (issues, degradation, etc.).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of provider health alert DTOs</returns>
    Task<IEnumerable<ProviderHealthAlertDto>> GetProviderHealthAlertsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if any provider is in critical status.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if any provider is critical</returns>
    Task<bool> HasCriticalProvidersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get providers with status warnings/issues.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of provider names with issues</returns>
    Task<IEnumerable<string>> GetProvidersWithIssuesAsync(CancellationToken cancellationToken = default);

    #endregion

    #region Configuration & Cache

    /// <summary>
    /// Refresh provider health cache.
    /// Useful when provider configuration changes.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task completion</returns>
    Task RefreshProviderHealthCacheAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Set health check interval in seconds.
    /// </summary>
    /// <param name="intervalSeconds">Interval in seconds</param>
    void SetHealthCheckIntervalSeconds(int intervalSeconds);

    #endregion
}

/// <summary>
/// Enum for provider health status.
/// </summary>
public enum ProviderHealthStatus
{
    /// <summary>
    /// Provider is healthy and responding normally
    /// </summary>
    Healthy = 0,

    /// <summary>
    /// Provider is degraded but still operational
    /// </summary>
    Degraded = 1,

    /// <summary>
    /// Provider is unavailable or not responding
    /// </summary>
    Unhealthy = 2,

    /// <summary>
    /// Provider has not been checked yet
    /// </summary>
    Unknown = 3,

    /// <summary>
    /// Provider is not configured
    /// </summary>
    NotConfigured = 4
}
