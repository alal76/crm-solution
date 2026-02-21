// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Interfaces.ITSM;

/// <summary>
/// Service for integrating with external monitoring tools (Prometheus, Grafana, Datadog, etc.).
/// </summary>
public interface IMonitoringIntegrationService
{
    /// <summary>
    /// Process an incoming alert from a monitoring tool.
    /// </summary>
    Task<AlertProcessingResult> ProcessAlertAsync(MonitoringAlertDto alert);

    /// <summary>
    /// Get all configured monitoring integrations.
    /// </summary>
    Task<List<MonitoringIntegrationDto>> GetIntegrationsAsync();

    /// <summary>
    /// Get a specific monitoring integration.
    /// </summary>
    Task<MonitoringIntegrationDto?> GetIntegrationAsync(int id);

    /// <summary>
    /// Create a new monitoring integration.
    /// </summary>
    Task<MonitoringIntegrationDto> CreateIntegrationAsync(CreateMonitoringIntegrationDto dto);

    /// <summary>
    /// Update a monitoring integration.
    /// </summary>
    Task<MonitoringIntegrationDto?> UpdateIntegrationAsync(int id, UpdateMonitoringIntegrationDto dto);

    /// <summary>
    /// Delete a monitoring integration.
    /// </summary>
    Task<bool> DeleteIntegrationAsync(int id);

    /// <summary>
    /// Test a monitoring integration.
    /// </summary>
    Task<IntegrationTestResult> TestIntegrationAsync(int id);

    /// <summary>
    /// Get alert processing history.
    /// </summary>
    Task<List<AlertHistoryDto>> GetAlertHistoryAsync(int? integrationId, DateTime? startDate, DateTime? endDate);
}

// ====== DTOs ======
public class MonitoringAlertDto
{
    public string Source { get; set; } = string.Empty;
    public string AlertId { get; set; } = string.Empty;
    public string AlertName { get; set; } = string.Empty;
    public AlertSeverity Severity { get; set; } = AlertSeverity.Warning;
    public string Status { get; set; } = "firing"; // firing, resolved
    public string Description { get; set; } = string.Empty;
    public string? AffectedResource { get; set; }
    public string? AffectedService { get; set; }
    public DateTime StartsAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndsAt { get; set; }
    public Dictionary<string, string> Labels { get; set; } = new();
    public Dictionary<string, string> Annotations { get; set; } = new();
    public string? Fingerprint { get; set; }
}

public enum AlertSeverity
{
    Info = 0,
    Warning = 1,
    Error = 2,
    Critical = 3
}

public class AlertProcessingResult
{
    public bool Success { get; set; }
    public AlertAction Action { get; set; }
    public int? IncidentId { get; set; }
    public string? IncidentNumber { get; set; }
    public string? Message { get; set; }
}

public enum AlertAction
{
    IncidentCreated,
    IncidentUpdated,
    IncidentResolved,
    Deduplicated,
    Suppressed,
    Ignored,
    Failed
}

public class MonitoringIntegrationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public MonitoringToolType ToolType { get; set; }
    public string ApiKey { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public bool AutoCreateIncidents { get; set; } = true;
    public bool AutoResolveIncidents { get; set; } = true;
    public int DeduplicationWindowMinutes { get; set; } = 30;
    public Dictionary<AlertSeverity, int> SeverityToPriorityMapping { get; set; } = new();
    public int? DefaultAssignmentGroupId { get; set; }
    public string? DefaultCategory { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum MonitoringToolType
{
    Prometheus,
    Grafana,
    Datadog,
    NewRelic,
    PagerDuty,
    Zabbix,
    Nagios,
    CloudWatch,
    AzureMonitor,
    Custom
}

public class CreateMonitoringIntegrationDto
{
    public string Name { get; set; } = string.Empty;
    public MonitoringToolType ToolType { get; set; }
    public bool IsEnabled { get; set; } = true;
    public bool AutoCreateIncidents { get; set; } = true;
    public bool AutoResolveIncidents { get; set; } = true;
    public int DeduplicationWindowMinutes { get; set; } = 30;
    public Dictionary<AlertSeverity, int>? SeverityToPriorityMapping { get; set; }
    public int? DefaultAssignmentGroupId { get; set; }
    public string? DefaultCategory { get; set; }
}

public class UpdateMonitoringIntegrationDto
{
    public string? Name { get; set; }
    public bool? IsEnabled { get; set; }
    public bool? AutoCreateIncidents { get; set; }
    public bool? AutoResolveIncidents { get; set; }
    public int? DeduplicationWindowMinutes { get; set; }
    public Dictionary<AlertSeverity, int>? SeverityToPriorityMapping { get; set; }
    public int? DefaultAssignmentGroupId { get; set; }
    public string? DefaultCategory { get; set; }
}

public class IntegrationTestResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime TestedAt { get; set; } = DateTime.UtcNow;
}

public class AlertHistoryDto
{
    public int Id { get; set; }
    public int IntegrationId { get; set; }
    public string IntegrationName { get; set; } = string.Empty;
    public string AlertId { get; set; } = string.Empty;
    public string AlertName { get; set; } = string.Empty;
    public AlertSeverity Severity { get; set; }
    public AlertAction ActionTaken { get; set; }
    public int? IncidentId { get; set; }
    public string? IncidentNumber { get; set; }
    public DateTime ProcessedAt { get; set; }
}
