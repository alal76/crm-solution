// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
#pragma warning disable SA1649 // file name should match first type name
namespace CRM.Core.Dtos;

#region Role DTOs

/// <summary>
/// DTO for role response.
/// </summary>
public class RoleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int HierarchyLevel { get; set; }
    public bool IsSystemDefined { get; set; }
    public bool IsActive { get; set; }
    public int PermissionCount { get; set; }
    public int UserCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating a new role.
/// </summary>
public class CreateRoleDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int HierarchyLevel { get; set; }
}

/// <summary>
/// DTO for updating a role.
/// </summary>
public class UpdateRoleDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? HierarchyLevel { get; set; }
}

#endregion

#region Permission DTOs

/// <summary>
/// DTO for permission response.
/// </summary>
public class PermissionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsSystemDefined { get; set; }
    public bool IsActive { get; set; }
    public int RoleCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for creating a custom permission.
/// </summary>
public class CreatePermissionDto
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

#endregion

#region Role-Permission DTOs

/// <summary>
/// DTO for role-permission assignment.
/// </summary>
public class RolePermissionDto
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public int PermissionId { get; set; }
    public PermissionDto? Permission { get; set; }
    public DateTime AssignedAt { get; set; }
}

#endregion

#region User-Role DTOs

/// <summary>
/// DTO for user-role assignment with temporal validity.
/// </summary>
public class UserRoleDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int RoleId { get; set; }
    public RoleDto? Role { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive => DateTime.UtcNow >= EffectiveFrom && (EffectiveTo == null || DateTime.UtcNow < EffectiveTo);
    public DateTime AssignedAt { get; set; }
}

#endregion

#region Role Hierarchy DTOs

/// <summary>
/// DTO for role hierarchy.
/// </summary>
public class RoleHierarchyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }
    public List<RoleHierarchyDto> Children { get; set; } = new();
}

#endregion

#region Provider Health DTOs

/// <summary>
/// DTO for provider health status.
/// </summary>
public class ProviderHealthDto
{
    public string Category { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int Status { get; set; } // ProviderHealthStatus enum
    public string StatusText { get; set; } = string.Empty;
    public DateTime? LastCheckedAt { get; set; }
    public int? ResponseTimeMs { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsConfigured { get; set; }
    public decimal? SuccessRate { get; set; }
}

/// <summary>
/// DTO for provider health dashboard aggregating all providers.
/// </summary>
public class ProviderHealthDashboardDto
{
    public int TotalProviders { get; set; }
    public int HealthyCount { get; set; }
    public int DegradedCount { get; set; }
    public int UnhealthyCount { get; set; }
    public int NotConfiguredCount { get; set; }
    public Dictionary<string, List<ProviderHealthDto>> ProvidersByCategory { get; set; } = new();
    public DateTime LastRefreshAt { get; set; }
}

/// <summary>
/// DTO for detailed health check result.
/// </summary>
public class ProviderHealthCheckDetailDto
{
    public string Category { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public int Status { get; set; } // ProviderHealthStatus enum
    public string StatusMessage { get; set; } = string.Empty;
    public DateTime CheckedAt { get; set; }
    public int ResponseTimeMs { get; set; }
    public Dictionary<string, object> Diagnostics { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// DTO for provider performance metrics.
/// </summary>
public class ProviderPerformanceMetricsDto
{
    public string ProviderName { get; set; } = string.Empty;
    public int AverageResponseTimeMs { get; set; }
    public int MaxResponseTimeMs { get; set; }
    public int MinResponseTimeMs { get; set; }
    public decimal ErrorRatePercent { get; set; }
    public int TotalRequests { get; set; }
    public int FailedRequests { get; set; }
    public decimal ThroughputRequestsPerSecond { get; set; }
    public DateTime MeasurementStart { get; set; }
    public DateTime MeasurementEnd { get; set; }
}

/// <summary>
/// DTO for provider health alerts.
/// </summary>
public class ProviderHealthAlertDto
{
    public int Id { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string AlertType { get; set; } = string.Empty; // Critical, Warning, Info
    public string Message { get; set; } = string.Empty;
    public DateTime AlertedAt { get; set; }
    public bool IsResolved { get; set; }
}

#endregion

#region Admin Dashboard DTOs

/// <summary>
/// Complete admin dashboard DTO.
/// </summary>
public class AdminDashboardDto
{
    public SystemStatisticsDto SystemStatistics { get; set; } = new();
    public Dictionary<string, ModuleStatusDto> ModuleStatus { get; set; } = new();
    public ProviderHealthDashboardDto ProviderHealth { get; set; } = new();
    public SystemPerformanceMetricsDto PerformanceMetrics { get; set; } = new();
    public List<AdminAlertDto> RecentAlerts { get; set; } = new();
    public QuickActionsSummaryDto QuickActions { get; set; } = new();
    public DateTime RefreshedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// DTO for system-wide statistics.
/// </summary>
public class SystemStatisticsDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalAccounts { get; set; }
    public int TotalContacts { get; set; }
    public int TotalOpportunities { get; set; }
    public int TotalLeads { get; set; }
    public int TotalServiceRequests { get; set; }
    public int TotalActivities { get; set; }
    public int RecentActivities24h { get; set; }
    public decimal ActiveUserPercentage { get; set; }
}

/// <summary>
/// DTO for detailed system statistics with trends.
/// </summary>
public class DetailedSystemStatisticsDto : SystemStatisticsDto
{
    public List<DailyStatisticDto> DailyStats { get; set; } = new();
    public UserGrowthTrendDto UserTrend { get; set; } = new();
    public AccountGrowthTrendDto AccountTrend { get; set; } = new();
    public ActivityTrendDto ActivityTrend { get; set; } = new();
}

/// <summary>
/// DTO for daily statistics.
/// </summary>
public class DailyStatisticDto
{
    public DateTime Date { get; set; }
    public int NewUsers { get; set; }
    public int NewAccounts { get; set; }
    public int NewActivities { get; set; }
    public int LoginCount { get; set; }
}

/// <summary>
/// DTO for user growth trend.
/// </summary>
public class UserGrowthTrendDto
{
    public int Current { get; set; }
    public int Previous { get; set; }
    public decimal PercentChange { get; set; }
}

/// <summary>
/// DTO for account growth trend.
/// </summary>
public class AccountGrowthTrendDto
{
    public int Current { get; set; }
    public int Previous { get; set; }
    public decimal PercentChange { get; set; }
}

/// <summary>
/// DTO for activity trend.
/// </summary>
public class ActivityTrendDto
{
    public int Current { get; set; }
    public int Previous { get; set; }
    public decimal PercentChange { get; set; }
}

/// <summary>
/// DTO for module operational status.
/// </summary>
public class ModuleStatusDto
{
    public string ModuleName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsOperational { get; set; }
    public string Status { get; set; } = string.Empty; // Operational, Degraded, Down
    public int? ErrorCount { get; set; }
    public string? LastErrorMessage { get; set; }
    public DateTime? LastCheckAt { get; set; }
}

/// <summary>
/// DTO for system performance metrics.
/// </summary>
public class SystemPerformanceMetricsDto
{
    public double AverageCpuUsagePercent { get; set; }
    public double AverageMemoryUsagePercent { get; set; }
    public double AverageDiskUsagePercent { get; set; }
    public int AverageApiResponseTimeMs { get; set; }
    public int AverageDatabaseQueryTimeMs { get; set; }
    public decimal RequestsPerSecond { get; set; }
    public decimal ErrorRatePercent { get; set; }
    public DateTime MeasurementStart { get; set; }
    public DateTime MeasurementEnd { get; set; }
}

/// <summary>
/// DTO for endpoint performance metrics.
/// </summary>
public class EndpointPerformanceMetricsDto
{
    public string Endpoint { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
    public int CallCount { get; set; }
    public int AverageResponseTimeMs { get; set; }
    public int MaxResponseTimeMs { get; set; }
    public decimal ErrorRatePercent { get; set; }
}

/// <summary>
/// DTO for database performance metrics.
/// </summary>
public class DatabasePerformanceMetricsDto
{
    public int ActiveConnections { get; set; }
    public int MaxConnections { get; set; }
    public int AverageQueryTimeMs { get; set; }
    public int SlowQueryCount { get; set; }
    public long DatabaseSizeBytes { get; set; }
    public decimal TableFragmentationPercent { get; set; }
}

/// <summary>
/// DTO for admin alerts.
/// </summary>
public class AdminAlertDto
{
    public int Id { get; set; }
    public string AlertType { get; set; } = string.Empty; // Critical, Warning, Info
    public string Severity { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime AlertedAt { get; set; }
    public bool IsAcknowledged { get; set; }
    public string? ActionUrl { get; set; }
}

/// <summary>
/// DTO for quick admin actions summary.
/// </summary>
public class QuickActionsSummaryDto
{
    public int PendingApprovals { get; set; }
    public int FailedJobs { get; set; }
    public int OverdueTickets { get; set; }
    public int UnresolvedAlerts { get; set; }
    public int FailedIntegrations { get; set; }
}

#endregion
