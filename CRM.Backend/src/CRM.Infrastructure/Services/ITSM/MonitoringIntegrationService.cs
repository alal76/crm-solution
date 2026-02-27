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

using CRM.Core.Interfaces.ITSM;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.ITSM;

/// <summary>
/// Service for integrating with external monitoring tools.
/// Handles alert processing, deduplication, and automatic incident management.
/// </summary>
public class MonitoringIntegrationService : IMonitoringIntegrationService
{
    private readonly ILogger<MonitoringIntegrationService> _logger;
    private readonly List<MonitoringIntegrationDto> _integrations = new();
    private readonly List<AlertHistoryDto> _alertHistory = new();
    private readonly Dictionary<string, (DateTime ProcessedAt, int IncidentId)> _deduplicationCache = new();
    private int _nextIntegrationId = 1;
    private int _nextHistoryId = 1;

    public MonitoringIntegrationService(ILogger<MonitoringIntegrationService> logger)
    {
        _logger = logger;
        InitializeDefaultIntegrations();
    }

    private void InitializeDefaultIntegrations()
    {
        _integrations.Add(new MonitoringIntegrationDto
        {
            Id = _nextIntegrationId++,
            Name = "Prometheus Alertmanager",
            ToolType = MonitoringToolType.Prometheus,
            ApiKey = GenerateApiKey(),
            IsEnabled = true,
            AutoCreateIncidents = true,
            AutoResolveIncidents = true,
            DeduplicationWindowMinutes = 30,
            SeverityToPriorityMapping = new Dictionary<AlertSeverity, int>
            {
                { AlertSeverity.Critical, 1 },
                { AlertSeverity.Error, 2 },
                { AlertSeverity.Warning, 3 },
                { AlertSeverity.Info, 4 }
            },
            DefaultCategory = "Monitoring",
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            UpdatedAt = DateTime.UtcNow.AddDays(-7)
        });

        _integrations.Add(new MonitoringIntegrationDto
        {
            Id = _nextIntegrationId++,
            Name = "Grafana Alerts",
            ToolType = MonitoringToolType.Grafana,
            ApiKey = GenerateApiKey(),
            IsEnabled = true,
            AutoCreateIncidents = true,
            AutoResolveIncidents = false,
            DeduplicationWindowMinutes = 15,
            SeverityToPriorityMapping = new Dictionary<AlertSeverity, int>
            {
                { AlertSeverity.Critical, 1 },
                { AlertSeverity.Error, 2 },
                { AlertSeverity.Warning, 3 },
                { AlertSeverity.Info, 4 }
            },
            DefaultCategory = "Infrastructure",
            CreatedAt = DateTime.UtcNow.AddDays(-20),
            UpdatedAt = DateTime.UtcNow.AddDays(-2)
        });
    }

    /// <inheritdoc />
    public Task<AlertProcessingResult> ProcessAlertAsync(MonitoringAlertDto alert)
    {
        _logger.LogInformation("Processing alert {AlertId} from {Source}: {AlertName}",
            alert.AlertId, alert.Source, alert.AlertName);

        var integration = _integrations.FirstOrDefault(i =>
            i.IsEnabled && i.ToolType.ToString().Equals(alert.Source, StringComparison.OrdinalIgnoreCase));

        if (integration == null)
        {
            _logger.LogWarning("No enabled integration found for source: {Source}", alert.Source);
            return Task.FromResult(new AlertProcessingResult
            {
                Success = false,
                Action = AlertAction.Failed,
                Message = $"No enabled integration for source: {alert.Source}"
            });
        }

        // Check for alert resolution
        if (alert.Status.Equals("resolved", StringComparison.OrdinalIgnoreCase))
        {
            return HandleResolvedAlert(alert, integration);
        }

        // Check for deduplication
        var dedupKey = $"{integration.Id}:{alert.Fingerprint ?? alert.AlertId}";
        if (_deduplicationCache.TryGetValue(dedupKey, out var cached)
            && (DateTime.UtcNow - cached.ProcessedAt).TotalMinutes < integration.DeduplicationWindowMinutes)
        {
            _logger.LogInformation("Alert deduplicated, existing incident: INC-{IncidentId}", cached.IncidentId);

            RecordHistory(integration, alert, AlertAction.Deduplicated, cached.IncidentId);

            return Task.FromResult(new AlertProcessingResult
            {
                Success = true,
                Action = AlertAction.Deduplicated,
                IncidentId = cached.IncidentId,
                IncidentNumber = $"INC-{cached.IncidentId:D5}",
                Message = "Alert deduplicated with existing incident"
            });
        }

        // Create new incident
        if (integration.AutoCreateIncidents)
        {
            return CreateIncidentFromAlert(alert, integration, dedupKey);
        }

        RecordHistory(integration, alert, AlertAction.Ignored, null);
        return Task.FromResult(new AlertProcessingResult
        {
            Success = true,
            Action = AlertAction.Ignored,
            Message = "Auto-create disabled, alert ignored"
        });
    }

    private Task<AlertProcessingResult> HandleResolvedAlert(MonitoringAlertDto alert, MonitoringIntegrationDto integration)
    {
        var dedupKey = $"{integration.Id}:{alert.Fingerprint ?? alert.AlertId}";

        if (integration.AutoResolveIncidents && _deduplicationCache.TryGetValue(dedupKey, out var cached))
        {
            _deduplicationCache.Remove(dedupKey);
            _logger.LogInformation("Auto-resolving incident INC-{IncidentId} from alert resolution", cached.IncidentId);

            RecordHistory(integration, alert, AlertAction.IncidentResolved, cached.IncidentId);

            return Task.FromResult(new AlertProcessingResult
            {
                Success = true,
                Action = AlertAction.IncidentResolved,
                IncidentId = cached.IncidentId,
                IncidentNumber = $"INC-{cached.IncidentId:D5}",
                Message = "Incident auto-resolved"
            });
        }

        RecordHistory(integration, alert, AlertAction.Ignored, null);
        return Task.FromResult(new AlertProcessingResult
        {
            Success = true,
            Action = AlertAction.Ignored,
            Message = "Resolved alert - no matching incident or auto-resolve disabled"
        });
    }

    private Task<AlertProcessingResult> CreateIncidentFromAlert(
        MonitoringAlertDto alert,
        MonitoringIntegrationDto integration,
        string dedupKey)
    {
        var incidentId = Random.Shared.Next(10000, 99999);
        var priority = integration.SeverityToPriorityMapping.TryGetValue(alert.Severity, out var p) ? p : 3;

        _deduplicationCache[dedupKey] = (DateTime.UtcNow, incidentId);

        _logger.LogInformation("Created incident INC-{IncidentId} from alert with priority {Priority}",
            incidentId, priority);

        RecordHistory(integration, alert, AlertAction.IncidentCreated, incidentId);

        return Task.FromResult(new AlertProcessingResult
        {
            Success = true,
            Action = AlertAction.IncidentCreated,
            IncidentId = incidentId,
            IncidentNumber = $"INC-{incidentId:D5}",
            Message = $"Incident created with priority {priority}"
        });
    }

    private void RecordHistory(MonitoringIntegrationDto integration, MonitoringAlertDto alert, AlertAction action, int? incidentId)
    {
        _alertHistory.Add(new AlertHistoryDto
        {
            Id = _nextHistoryId++,
            IntegrationId = integration.Id,
            IntegrationName = integration.Name,
            AlertId = alert.AlertId,
            AlertName = alert.AlertName,
            Severity = alert.Severity,
            ActionTaken = action,
            IncidentId = incidentId,
            IncidentNumber = incidentId.HasValue ? $"INC-{incidentId:D5}" : null,
            ProcessedAt = DateTime.UtcNow
        });
    }

    /// <inheritdoc />
    public Task<List<MonitoringIntegrationDto>> GetIntegrationsAsync()
    {
        return Task.FromResult(_integrations.ToList());
    }

    /// <inheritdoc />
    public Task<MonitoringIntegrationDto?> GetIntegrationAsync(int id)
    {
        var integration = _integrations.FirstOrDefault(i => i.Id == id);
        return Task.FromResult(integration);
    }

    /// <inheritdoc />
    public Task<MonitoringIntegrationDto> CreateIntegrationAsync(CreateMonitoringIntegrationDto dto)
    {
        var integration = new MonitoringIntegrationDto
        {
            Id = _nextIntegrationId++,
            Name = dto.Name,
            ToolType = dto.ToolType,
            ApiKey = GenerateApiKey(),
            IsEnabled = dto.IsEnabled,
            AutoCreateIncidents = dto.AutoCreateIncidents,
            AutoResolveIncidents = dto.AutoResolveIncidents,
            DeduplicationWindowMinutes = dto.DeduplicationWindowMinutes,
            SeverityToPriorityMapping = dto.SeverityToPriorityMapping ?? new Dictionary<AlertSeverity, int>
            {
                { AlertSeverity.Critical, 1 },
                { AlertSeverity.Error, 2 },
                { AlertSeverity.Warning, 3 },
                { AlertSeverity.Info, 4 }
            },
            DefaultAssignmentGroupId = dto.DefaultAssignmentGroupId,
            DefaultCategory = dto.DefaultCategory,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _integrations.Add(integration);
        _logger.LogInformation("Created monitoring integration {Name} ({ToolType})", integration.Name, integration.ToolType);

        return Task.FromResult(integration);
    }

    /// <inheritdoc />
    public Task<MonitoringIntegrationDto?> UpdateIntegrationAsync(int id, UpdateMonitoringIntegrationDto dto)
    {
        var integration = _integrations.FirstOrDefault(i => i.Id == id);
        if (integration == null)
            return Task.FromResult<MonitoringIntegrationDto?>(null);

        if (dto.Name != null) integration.Name = dto.Name;
        if (dto.IsEnabled.HasValue) integration.IsEnabled = dto.IsEnabled.Value;
        if (dto.AutoCreateIncidents.HasValue) integration.AutoCreateIncidents = dto.AutoCreateIncidents.Value;
        if (dto.AutoResolveIncidents.HasValue) integration.AutoResolveIncidents = dto.AutoResolveIncidents.Value;
        if (dto.DeduplicationWindowMinutes.HasValue) integration.DeduplicationWindowMinutes = dto.DeduplicationWindowMinutes.Value;
        if (dto.SeverityToPriorityMapping != null) integration.SeverityToPriorityMapping = dto.SeverityToPriorityMapping;
        if (dto.DefaultAssignmentGroupId.HasValue) integration.DefaultAssignmentGroupId = dto.DefaultAssignmentGroupId;
        if (dto.DefaultCategory != null) integration.DefaultCategory = dto.DefaultCategory;

        integration.UpdatedAt = DateTime.UtcNow;

        _logger.LogInformation("Updated monitoring integration {Id}", id);
        return Task.FromResult<MonitoringIntegrationDto?>(integration);
    }

    /// <inheritdoc />
    public Task<bool> DeleteIntegrationAsync(int id)
    {
        var integration = _integrations.FirstOrDefault(i => i.Id == id);
        if (integration == null)
            return Task.FromResult(false);

        _integrations.Remove(integration);
        _logger.LogInformation("Deleted monitoring integration {Id}", id);
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task<IntegrationTestResult> TestIntegrationAsync(int id)
    {
        var integration = _integrations.FirstOrDefault(i => i.Id == id);
        if (integration == null)
        {
            return Task.FromResult(new IntegrationTestResult
            {
                Success = false,
                Message = "Integration not found"
            });
        }

        // Simulate connectivity test
        return Task.FromResult(new IntegrationTestResult
        {
            Success = true,
            Message = $"Successfully connected to {integration.ToolType} integration '{integration.Name}'"
        });
    }

    /// <inheritdoc />
    public Task<List<AlertHistoryDto>> GetAlertHistoryAsync(int? integrationId, DateTime? startDate, DateTime? endDate)
    {
        var query = _alertHistory.AsEnumerable();

        if (integrationId.HasValue)
            query = query.Where(h => h.IntegrationId == integrationId.Value);

        if (startDate.HasValue)
            query = query.Where(h => h.ProcessedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(h => h.ProcessedAt <= endDate.Value);

        return Task.FromResult(query.OrderByDescending(h => h.ProcessedAt).ToList());
    }

    private static string GenerateApiKey()
    {
        return $"mnt_{Guid.NewGuid():N}";
    }
}
