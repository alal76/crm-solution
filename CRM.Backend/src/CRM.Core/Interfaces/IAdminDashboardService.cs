// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal

using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service for providing aggregated admin dashboard data.
/// Combines system statistics, module status, provider health, and performance metrics.
///
/// HEXAGONAL ARCHITECTURE:
/// - Port: Defines contract for admin dashboard data
/// - Accessed by: AdminDashboardController, AdminDashboardPage
/// - Depends on: IProviderHealthService, ISystemSettingsService, activity services
///
/// DASHBOARD SECTIONS:
/// - System Statistics (user count, account count, activity)
/// - Module Status (which modules are operational)
/// - Provider Health (all providers with status)
/// - Performance Metrics (system load, response times)
/// - Recent Alerts (configured alerts for admins)
/// </summary>
public interface IAdminDashboardService
{
    #region Complete Dashboard
    
    /// <summary>
    /// Get complete admin dashboard data.
    /// Includes all sections: stats, module status, provider health, metrics, alerts.
    /// </summary>
    /// <param name="timeRangeHours">Hours back to consider for metrics/activity</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Complete admin dashboard DTO</returns>
    Task<AdminDashboardDto> GetCompleteAdminDashboardAsync(int timeRangeHours = 24, CancellationToken cancellationToken = default);
    
    #endregion

    #region System Statistics
    
    /// <summary>
    /// Get system-wide statistics.
    /// Includes user count, account count, recent activity, etc.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>System statistics DTO</returns>
    Task<SystemStatisticsDto> GetSystemStatisticsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get detailed system statistics with trends.
    /// </summary>
    /// <param name="daysBack">Number of days to include in trends</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Detailed statistics DTO</returns>
    Task<DetailedSystemStatisticsDto> GetDetailedSystemStatisticsAsync(int daysBack = 30, CancellationToken cancellationToken = default);
    
    #endregion

    #region Module Status
    
    /// <summary>
    /// Get operational status of all modules.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary of module name to status</returns>
    Task<IDictionary<string, ModuleStatusDto>> GetAllModuleStatusAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Check if a specific module is operational.
    /// </summary>
    /// <param name="moduleName">Name of module (Accounts, Opportunities, ITSM, etc.)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Module status DTO</returns>
    Task<ModuleStatusDto> GetModuleStatusAsync(string moduleName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get system health summary (all modules operational?).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if all modules operational</returns>
    Task<bool> IsSystemHealthyAsync(CancellationToken cancellationToken = default);
    
    #endregion

    #region Provider Health (Summary)
    
    /// <summary>
    /// Get provider health summary from cached data.
    /// Note: Detailed checks use IProviderHealthService directly.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Provider health dashboard DTO</returns>
    Task<ProviderHealthDashboardDto> GetProviderHealthSummaryAsync(CancellationToken cancellationToken = default);
    
    #endregion

    #region Performance Metrics
    
    /// <summary>
    /// Get system performance metrics.
    /// </summary>
    /// <param name="hoursBack">Hours back to measure</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Performance metrics DTO</returns>
    Task<SystemPerformanceMetricsDto> GetSystemPerformanceMetricsAsync(int hoursBack = 24, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get API endpoint performance metrics.
    /// </summary>
    /// <param name="hoursBack">Hours back to measure</param>
    /// <param name="topCount">Number of top endpoints to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of endpoint metrics</returns>
    Task<IEnumerable<EndpointPerformanceMetricsDto>> GetEndpointPerformanceMetricsAsync(int hoursBack = 24, int topCount = 10, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get database performance metrics.
    /// </summary>
    /// <param name="hoursBack">Hours back to measure</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Database metrics DTO</returns>
    Task<DatabasePerformanceMetricsDto> GetDatabasePerformanceMetricsAsync(int hoursBack = 24, CancellationToken cancellationToken = default);
    
    #endregion

    #region Alerts & Health Checks
    
    /// <summary>
    /// Get recent alerts/notifications for admins.
    /// Includes provider issues, performance warnings, system alerts.
    /// </summary>
    /// <param name="hoursBack">Hours back to retrieve</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of alert DTOs</returns>
    Task<IEnumerable<AdminAlertDto>> GetRecentAlertsAsync(int hoursBack = 24, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get critical-level alerts only.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of critical alert DTOs</returns>
    Task<IEnumerable<AdminAlertDto>> GetCriticalAlertsAsync(CancellationToken cancellationToken = default);
    
    #endregion

    #region Quick Actions Summary
    
    /// <summary>
    /// Get summary for quick admin actions.
    /// Includes pending approvals, failed jobs, etc.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Quick actions summary DTO</returns>
    Task<QuickActionsSummaryDto> GetQuickActionsSummaryAsync(CancellationToken cancellationToken = default);
    
    #endregion

    #region Cache & Refresh
    
    /// <summary>
    /// Refresh dashboard cache.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task completion</returns>
    Task RefreshDashboardCacheAsync(CancellationToken cancellationToken = default);
    
    #endregion
}
