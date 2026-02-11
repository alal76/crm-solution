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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CRM.Infrastructure.Services.ITSM;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace CRM.Tests.Services.ITSM;

public class MonitoringIntegrationServiceTests
{
    private readonly Mock<ILogger<MonitoringIntegrationService>> _mockLogger;
    private readonly MonitoringIntegrationService _service;

    public MonitoringIntegrationServiceTests()
    {
        _mockLogger = new Mock<ILogger<MonitoringIntegrationService>>();
        _service = new MonitoringIntegrationService(_mockLogger.Object);
    }

    #region ProcessAlertAsync Tests

    [Fact]
    public async Task ProcessAlertAsync_WhenValidPrometheusAlert_CreatesIncident()
    {
        // Arrange
        var alert = new MonitoringAlertDto
        {
            AlertId = "alert-001",
            AlertName = "HighCPUUsage",
            Source = "Prometheus",
            Status = "firing",
            Severity = AlertSeverity.Critical,
            Fingerprint = "fp-001"
        };

        // Act
        var result = await _service.ProcessAlertAsync(alert);

        // Assert
        result.Success.Should().BeTrue();
        result.Action.Should().Be(AlertAction.IncidentCreated);
        result.IncidentNumber.Should().StartWith("INC-");
    }

    [Fact]
    public async Task ProcessAlertAsync_WhenDuplicate_DeduplicatesAlert()
    {
        // Arrange
        var alert = new MonitoringAlertDto
        {
            AlertId = "alert-dup",
            AlertName = "DiskSpaceLow",
            Source = "Prometheus",
            Status = "firing",
            Severity = AlertSeverity.Warning,
            Fingerprint = "fp-dup-001"
        };

        // Act - send same alert twice
        var first = await _service.ProcessAlertAsync(alert);
        var second = await _service.ProcessAlertAsync(alert);

        // Assert
        first.Action.Should().Be(AlertAction.IncidentCreated);
        second.Action.Should().Be(AlertAction.Deduplicated);
        second.IncidentId.Should().Be(first.IncidentId);
    }

    [Fact]
    public async Task ProcessAlertAsync_WhenResolved_ResolvesIncident()
    {
        // Arrange - first create incident
        var firingAlert = new MonitoringAlertDto
        {
            AlertId = "alert-resolve",
            AlertName = "ServiceDown",
            Source = "Prometheus",
            Status = "firing",
            Severity = AlertSeverity.Error,
            Fingerprint = "fp-resolve"
        };
        await _service.ProcessAlertAsync(firingAlert);

        var resolvedAlert = new MonitoringAlertDto
        {
            AlertId = "alert-resolve",
            AlertName = "ServiceDown",
            Source = "Prometheus",
            Status = "resolved",
            Severity = AlertSeverity.Error,
            Fingerprint = "fp-resolve"
        };

        // Act
        var result = await _service.ProcessAlertAsync(resolvedAlert);

        // Assert
        result.Success.Should().BeTrue();
        result.Action.Should().Be(AlertAction.IncidentResolved);
    }

    [Fact]
    public async Task ProcessAlertAsync_WhenNoIntegration_ReturnsFailed()
    {
        // Arrange
        var alert = new MonitoringAlertDto
        {
            AlertId = "alert-unknown",
            AlertName = "UnknownAlert",
            Source = "UnknownTool",
            Status = "firing",
            Severity = AlertSeverity.Warning
        };

        // Act
        var result = await _service.ProcessAlertAsync(alert);

        // Assert
        result.Success.Should().BeFalse();
        result.Action.Should().Be(AlertAction.Failed);
        result.Message.Should().Contain("No enabled integration");
    }

    [Fact]
    public async Task ProcessAlertAsync_WhenGrafanaAlert_ProcessesCorrectly()
    {
        // Arrange
        var alert = new MonitoringAlertDto
        {
            AlertId = "grafana-001",
            AlertName = "HighMemory",
            Source = "Grafana",
            Status = "firing",
            Severity = AlertSeverity.Warning,
            Fingerprint = "fp-grafana"
        };

        // Act
        var result = await _service.ProcessAlertAsync(alert);

        // Assert
        result.Success.Should().BeTrue();
        result.Action.Should().Be(AlertAction.IncidentCreated);
    }

    [Fact]
    public async Task ProcessAlertAsync_WhenResolved_ButAutoResolveDisabled_IgnoresAlert()
    {
        // Arrange - Grafana has AutoResolveIncidents = false
        var firingAlert = new MonitoringAlertDto
        {
            AlertId = "grafana-no-auto",
            AlertName = "TestAlert",
            Source = "Grafana",
            Status = "firing",
            Severity = AlertSeverity.Warning,
            Fingerprint = "fp-no-auto"
        };
        await _service.ProcessAlertAsync(firingAlert);

        var resolvedAlert = new MonitoringAlertDto
        {
            AlertId = "grafana-no-auto",
            AlertName = "TestAlert",
            Source = "Grafana",
            Status = "resolved",
            Severity = AlertSeverity.Warning,
            Fingerprint = "fp-no-auto"
        };

        // Act
        var result = await _service.ProcessAlertAsync(resolvedAlert);

        // Assert
        result.Action.Should().Be(AlertAction.Ignored);
    }

    [Fact]
    public async Task ProcessAlertAsync_MapsSeverityToPriority()
    {
        // Arrange
        var criticalAlert = new MonitoringAlertDto
        {
            AlertId = "prio-001",
            AlertName = "CriticalAlert",
            Source = "Prometheus",
            Status = "firing",
            Severity = AlertSeverity.Critical,
            Fingerprint = "fp-critical"
        };

        // Act
        var result = await _service.ProcessAlertAsync(criticalAlert);

        // Assert
        result.Message.Should().Contain("priority 1");
    }

    #endregion

    #region GetIntegrationsAsync Tests

    [Fact]
    public async Task GetIntegrationsAsync_ReturnsDefaultIntegrations()
    {
        // Act
        var result = await _service.GetIntegrationsAsync();

        // Assert
        result.Should().HaveCountGreaterOrEqualTo(2);
        result.Should().Contain(i => i.Name == "Prometheus Alertmanager");
        result.Should().Contain(i => i.Name == "Grafana Alerts");
    }

    #endregion

    #region GetIntegrationAsync Tests

    [Fact]
    public async Task GetIntegrationAsync_WhenExists_ReturnsIntegration()
    {
        // Arrange
        var integrations = await _service.GetIntegrationsAsync();
        var targetId = integrations.First().Id;

        // Act
        var result = await _service.GetIntegrationAsync(targetId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(targetId);
    }

    [Fact]
    public async Task GetIntegrationAsync_WhenNotExists_ReturnsNull()
    {
        // Act
        var result = await _service.GetIntegrationAsync(9999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CreateIntegrationAsync Tests

    [Fact]
    public async Task CreateIntegrationAsync_CreatesWithCorrectProperties()
    {
        // Arrange
        var dto = new CreateMonitoringIntegrationDto
        {
            Name = "New Datadog Integration",
            ToolType = MonitoringToolType.Custom,
            IsEnabled = true,
            AutoCreateIncidents = true,
            AutoResolveIncidents = true,
            DeduplicationWindowMinutes = 60,
            DefaultCategory = "Application"
        };

        // Act
        var result = await _service.CreateIntegrationAsync(dto);

        // Assert
        result.Name.Should().Be("New Datadog Integration");
        result.ToolType.Should().Be(MonitoringToolType.Custom);
        result.IsEnabled.Should().BeTrue();
        result.AutoCreateIncidents.Should().BeTrue();
        result.AutoResolveIncidents.Should().BeTrue();
        result.DeduplicationWindowMinutes.Should().Be(60);
        result.ApiKey.Should().StartWith("mnt_");
    }

    [Fact]
    public async Task CreateIntegrationAsync_AssignsDefaultSeverityMapping()
    {
        // Arrange
        var dto = new CreateMonitoringIntegrationDto
        {
            Name = "Integration Without Mapping",
            ToolType = MonitoringToolType.Custom,
            IsEnabled = true
        };

        // Act
        var result = await _service.CreateIntegrationAsync(dto);

        // Assert
        result.SeverityToPriorityMapping.Should().ContainKey(AlertSeverity.Critical);
        result.SeverityToPriorityMapping[AlertSeverity.Critical].Should().Be(1);
    }

    [Fact]
    public async Task CreateIntegrationAsync_LogsCreation()
    {
        // Arrange
        var dto = new CreateMonitoringIntegrationDto { Name = "Test", ToolType = MonitoringToolType.Custom };

        // Act
        await _service.CreateIntegrationAsync(dto);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Created monitoring integration")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region UpdateIntegrationAsync Tests

    [Fact]
    public async Task UpdateIntegrationAsync_UpdatesProperties()
    {
        // Arrange
        var integrations = await _service.GetIntegrationsAsync();
        var targetId = integrations.First().Id;
        var dto = new UpdateMonitoringIntegrationDto
        {
            Name = "Updated Name",
            IsEnabled = false,
            DeduplicationWindowMinutes = 45
        };

        // Act
        var result = await _service.UpdateIntegrationAsync(targetId, dto);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");
        result.IsEnabled.Should().BeFalse();
        result.DeduplicationWindowMinutes.Should().Be(45);
    }

    [Fact]
    public async Task UpdateIntegrationAsync_WhenNotFound_ReturnsNull()
    {
        // Arrange
        var dto = new UpdateMonitoringIntegrationDto { Name = "Test" };

        // Act
        var result = await _service.UpdateIntegrationAsync(9999, dto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateIntegrationAsync_PartialUpdate_PreservesOtherFields()
    {
        // Arrange
        var created = await _service.CreateIntegrationAsync(new CreateMonitoringIntegrationDto
        {
            Name = "Original",
            ToolType = MonitoringToolType.Prometheus,
            IsEnabled = true,
            AutoCreateIncidents = true
        });

        var dto = new UpdateMonitoringIntegrationDto { Name = "Changed Name" };

        // Act
        var result = await _service.UpdateIntegrationAsync(created.Id, dto);

        // Assert
        result!.Name.Should().Be("Changed Name");
        result.ToolType.Should().Be(MonitoringToolType.Prometheus);
        result.AutoCreateIncidents.Should().BeTrue();
    }

    #endregion

    #region DeleteIntegrationAsync Tests

    [Fact]
    public async Task DeleteIntegrationAsync_WhenExists_ReturnsTrue()
    {
        // Arrange
        var created = await _service.CreateIntegrationAsync(new CreateMonitoringIntegrationDto
        {
            Name = "To Delete",
            ToolType = MonitoringToolType.Custom
        });

        // Act
        var result = await _service.DeleteIntegrationAsync(created.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteIntegrationAsync_WhenNotExists_ReturnsFalse()
    {
        // Act
        var result = await _service.DeleteIntegrationAsync(9999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteIntegrationAsync_RemovesFromList()
    {
        // Arrange
        var created = await _service.CreateIntegrationAsync(new CreateMonitoringIntegrationDto
        {
            Name = "To Be Removed",
            ToolType = MonitoringToolType.Custom
        });

        // Act
        await _service.DeleteIntegrationAsync(created.Id);
        var deleted = await _service.GetIntegrationAsync(created.Id);

        // Assert
        deleted.Should().BeNull();
    }

    #endregion

    #region TestIntegrationAsync Tests

    [Fact]
    public async Task TestIntegrationAsync_WhenExists_ReturnsSuccess()
    {
        // Arrange
        var integrations = await _service.GetIntegrationsAsync();
        var targetId = integrations.First().Id;

        // Act
        var result = await _service.TestIntegrationAsync(targetId);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("Successfully connected");
    }

    [Fact]
    public async Task TestIntegrationAsync_WhenNotExists_ReturnsFailed()
    {
        // Act
        var result = await _service.TestIntegrationAsync(9999);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    #endregion

    #region GetAlertHistoryAsync Tests

    [Fact]
    public async Task GetAlertHistoryAsync_ReturnsAllHistoryWhenNoFilter()
    {
        // Arrange - process some alerts first
        await _service.ProcessAlertAsync(new MonitoringAlertDto
        {
            AlertId = "hist-001",
            AlertName = "Alert1",
            Source = "Prometheus",
            Status = "firing",
            Severity = AlertSeverity.Warning,
            Fingerprint = "fp-hist-001"
        });
        await _service.ProcessAlertAsync(new MonitoringAlertDto
        {
            AlertId = "hist-002",
            AlertName = "Alert2",
            Source = "Grafana",
            Status = "firing",
            Severity = AlertSeverity.Error,
            Fingerprint = "fp-hist-002"
        });

        // Act
        var result = await _service.GetAlertHistoryAsync(null, null, null);

        // Assert
        result.Should().HaveCountGreaterOrEqualTo(2);
    }

    [Fact]
    public async Task GetAlertHistoryAsync_FiltersByIntegrationId()
    {
        // Arrange
        var integrations = await _service.GetIntegrationsAsync();
        var prometheusId = integrations.First(i => i.ToolType == MonitoringToolType.Prometheus).Id;

        await _service.ProcessAlertAsync(new MonitoringAlertDto
        {
            AlertId = "filter-001",
            AlertName = "PrometheusAlert",
            Source = "Prometheus",
            Status = "firing",
            Severity = AlertSeverity.Info,
            Fingerprint = "fp-filter-001"
        });

        // Act
        var result = await _service.GetAlertHistoryAsync(prometheusId, null, null);

        // Assert
        result.Should().AllSatisfy(h => h.IntegrationId.Should().Be(prometheusId));
    }

    [Fact]
    public async Task GetAlertHistoryAsync_FiltersByDateRange()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddMinutes(-5);
        await _service.ProcessAlertAsync(new MonitoringAlertDto
        {
            AlertId = "date-001",
            AlertName = "DateAlert",
            Source = "Prometheus",
            Status = "firing",
            Severity = AlertSeverity.Warning,
            Fingerprint = "fp-date-001"
        });
        var endDate = DateTime.UtcNow.AddMinutes(5);

        // Act
        var result = await _service.GetAlertHistoryAsync(null, startDate, endDate);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().AllSatisfy(h =>
        {
            h.ProcessedAt.Should().BeOnOrAfter(startDate);
            h.ProcessedAt.Should().BeOnOrBefore(endDate);
        });
    }

    [Fact]
    public async Task GetAlertHistoryAsync_OrdersByProcessedAtDescending()
    {
        // Arrange
        await _service.ProcessAlertAsync(new MonitoringAlertDto { AlertId = "order-1", Source = "Prometheus", Severity = AlertSeverity.Info, Fingerprint = "fp-1" });
        await _service.ProcessAlertAsync(new MonitoringAlertDto { AlertId = "order-2", Source = "Prometheus", Severity = AlertSeverity.Info, Fingerprint = "fp-2" });

        // Act
        var result = (await _service.GetAlertHistoryAsync(null, null, null)).ToList();

        // Assert
        if (result.Count >= 2)
        {
            result[0].ProcessedAt.Should().BeOnOrAfter(result[1].ProcessedAt);
        }
    }

    #endregion
}

#region Supporting DTOs and Enums (if not imported)

// Note: These are placeholder definitions. Real DTOs should be imported from CRM.Core
public class MonitoringAlertDto
{
    public string AlertId { get; set; } = "";
    public string AlertName { get; set; } = "";
    public string Source { get; set; } = "";
    public string Status { get; set; } = "";
    public AlertSeverity Severity { get; set; }
    public string? Fingerprint { get; set; }
}

public enum AlertSeverity { Info, Warning, Error, Critical }
public enum AlertAction { IncidentCreated, Deduplicated, IncidentResolved, Ignored, Failed }
public enum MonitoringToolType { Prometheus, Grafana, Datadog, NewRelic, PagerDuty, Custom }

public class AlertProcessingResult
{
    public bool Success { get; set; }
    public AlertAction Action { get; set; }
    public int? IncidentId { get; set; }
    public string? IncidentNumber { get; set; }
    public string Message { get; set; } = "";
}

public class MonitoringIntegrationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public MonitoringToolType ToolType { get; set; }
    public string ApiKey { get; set; } = "";
    public bool IsEnabled { get; set; }
    public bool AutoCreateIncidents { get; set; }
    public bool AutoResolveIncidents { get; set; }
    public int DeduplicationWindowMinutes { get; set; }
    public Dictionary<AlertSeverity, int> SeverityToPriorityMapping { get; set; } = new();
    public int? DefaultAssignmentGroupId { get; set; }
    public string? DefaultCategory { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateMonitoringIntegrationDto
{
    public string Name { get; set; } = "";
    public MonitoringToolType ToolType { get; set; }
    public bool IsEnabled { get; set; }
    public bool AutoCreateIncidents { get; set; }
    public bool AutoResolveIncidents { get; set; }
    public int DeduplicationWindowMinutes { get; set; }
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
    public string Message { get; set; } = "";
}

public class AlertHistoryDto
{
    public int Id { get; set; }
    public int IntegrationId { get; set; }
    public string IntegrationName { get; set; } = "";
    public string AlertId { get; set; } = "";
    public string AlertName { get; set; } = "";
    public AlertSeverity Severity { get; set; }
    public AlertAction ActionTaken { get; set; }
    public int? IncidentId { get; set; }
    public string? IncidentNumber { get; set; }
    public DateTime ProcessedAt { get; set; }
}

#endregion
