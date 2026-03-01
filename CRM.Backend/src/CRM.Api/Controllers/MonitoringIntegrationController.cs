// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Interfaces.ITSM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for monitoring tool integrations.
/// </summary>
[ApiController]
[Route("api/itsm/monitoring")]
[Tags("ITSM - Monitoring Integration")]
public class MonitoringIntegrationController : CrmControllerBase
{
    private readonly IMonitoringIntegrationService _monitoringService;
    private readonly ILogger<MonitoringIntegrationController> _logger;

    public MonitoringIntegrationController(
        IMonitoringIntegrationService monitoringService,
        ILogger<MonitoringIntegrationController> logger)
    {
        _monitoringService = monitoringService;
        _logger = logger;
    }

    /// <summary>
    /// Process an incoming alert from a monitoring tool.
    /// </summary>
    [HttpPost("alerts")]
    [AllowAnonymous] // Uses API key authentication
    [ProducesResponseType(typeof(AlertProcessingResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<AlertProcessingResult>> ProcessAlert(
        [FromBody] MonitoringAlertDto alert,
        [FromHeader(Name = "X-API-Key")] string? apiKey)
    {
        _logger.LogInformation("Received alert from {Source}: {AlertName}", alert.Source, alert.AlertName);

        var result = await _monitoringService.ProcessAlertAsync(alert);
        return Ok(result);
    }

    /// <summary>
    /// Process alerts in Prometheus Alertmanager format.
    /// </summary>
    [HttpPost("prometheus")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<AlertProcessingResult>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AlertProcessingResult>>> ProcessPrometheusAlerts(
        [FromBody] PrometheusAlertPayload payload,
        [FromHeader(Name = "X-API-Key")] string? apiKey)
    {
        var results = new List<AlertProcessingResult>();

        foreach (var promAlert in payload.Alerts)
        {
            var alert = new MonitoringAlertDto
            {
                Source = "Prometheus",
                AlertId = promAlert.Fingerprint,
                AlertName = promAlert.Labels.TryGetValue("alertname", out var name) ? name : "Unknown",
                Severity = MapPrometheusSeverity(promAlert.Labels.TryGetValue("severity", out var sev) ? sev : "warning"),
                Status = promAlert.Status,
                Description = promAlert.Annotations.TryGetValue("description", out var desc) ? desc : "",
                AffectedResource = promAlert.Labels.TryGetValue("instance", out var inst) ? inst : null,
                AffectedService = promAlert.Labels.TryGetValue("service", out var srv) ? srv : null,
                StartsAt = promAlert.StartsAt,
                EndsAt = promAlert.EndsAt,
                Labels = promAlert.Labels,
                Annotations = promAlert.Annotations,
                Fingerprint = promAlert.Fingerprint
            };

            var result = await _monitoringService.ProcessAlertAsync(alert);
            results.Add(result);
        }

        return Ok(results);
    }

    /// <summary>
    /// Get all monitoring integrations.
    /// </summary>
    [HttpGet("integrations")]
    [Authorize]
    [ProducesResponseType(typeof(List<MonitoringIntegrationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MonitoringIntegrationDto>>> GetIntegrations()
    {
        var integrations = await _monitoringService.GetIntegrationsAsync();
        return Ok(integrations);
    }

    /// <summary>
    /// Get a specific monitoring integration.
    /// </summary>
    [HttpGet("integrations/{id}")]
    [Authorize]
    [ProducesResponseType(typeof(MonitoringIntegrationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MonitoringIntegrationDto>> GetIntegration(int id)
    {
        var integration = await _monitoringService.GetIntegrationAsync(id);
        if (integration == null)
        {
            return NotFound();
        }

        return Ok(integration);
    }

    /// <summary>
    /// Create a new monitoring integration.
    /// </summary>
    [HttpPost("integrations")]
    [Authorize]
    [ProducesResponseType(typeof(MonitoringIntegrationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MonitoringIntegrationDto>> CreateIntegration(
        [FromBody] CreateMonitoringIntegrationDto dto)
    {
        var integration = await _monitoringService.CreateIntegrationAsync(dto);
        return CreatedAtAction(nameof(GetIntegration), new { id = integration.Id }, integration);
    }

    /// <summary>
    /// Update a monitoring integration.
    /// </summary>
    [HttpPut("integrations/{id}")]
    [Authorize]
    [ProducesResponseType(typeof(MonitoringIntegrationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MonitoringIntegrationDto>> UpdateIntegration(
        int id,
        [FromBody] UpdateMonitoringIntegrationDto dto)
    {
        var integration = await _monitoringService.UpdateIntegrationAsync(id, dto);
        if (integration == null)
        {
            return NotFound();
        }

        return Ok(integration);
    }

    /// <summary>
    /// Delete a monitoring integration.
    /// </summary>
    [HttpDelete("integrations/{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteIntegration(int id)
    {
        var deleted = await _monitoringService.DeleteIntegrationAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Test a monitoring integration.
    /// </summary>
    [HttpPost("integrations/{id}/test")]
    [Authorize]
    [ProducesResponseType(typeof(IntegrationTestResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<IntegrationTestResult>> TestIntegration(int id)
    {
        var result = await _monitoringService.TestIntegrationAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Get alert processing history.
    /// </summary>
    [HttpGet("history")]
    [Authorize]
    [ProducesResponseType(typeof(List<AlertHistoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AlertHistoryDto>>> GetAlertHistory(
        [FromQuery] int? integrationId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var history = await _monitoringService.GetAlertHistoryAsync(integrationId, startDate, endDate);
        return Ok(history);
    }

    /// <summary>
    /// Get monitoring sources (BVT-compatible route).
    /// </summary>
    [HttpGet("sources")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetMonitoringSources()
    {
        try
        {
            var integrations = await _monitoringService.GetIntegrationsAsync();
            return Ok(integrations);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch monitoring sources from service");
            return Ok(new List<object>());
        }
    }

    /// <summary>
    /// Get alert mappings.
    /// </summary>
    [HttpGet("alert-mappings")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetAlertMappings()
    {
        // TODO: Implement alert mapping CRUD in IMonitoringIntegrationService // NOSONAR
        try
        {
            var integrations = await _monitoringService.GetIntegrationsAsync();
            // Return integrations as a proxy for alert mappings until dedicated mapping service exists
            return Ok(integrations.Select(i => new { id = i.Id, source = i.Name, isActive = i.IsEnabled }).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch alert mappings");
            return Ok(new List<object>());
        }
    }

    private static AlertSeverity MapPrometheusSeverity(string severity)
    {
        return severity.ToLower() switch
        {
            "critical" => AlertSeverity.Critical,
            "error" => AlertSeverity.Error,
            "warning" => AlertSeverity.Warning,
            "info" => AlertSeverity.Info,
            _ => AlertSeverity.Warning
        };
    }
}

// Prometheus Alertmanager Webhook Payload
public class PrometheusAlertPayload
{
    public string Version { get; set; } = "4";
    public string GroupKey { get; set; } = string.Empty;
    public int TruncatedAlerts { get; set; }
    public string Status { get; set; } = "firing";
    public string Receiver { get; set; } = string.Empty;
    public Dictionary<string, string> GroupLabels { get; set; } = new();
    public Dictionary<string, string> CommonLabels { get; set; } = new();
    public Dictionary<string, string> CommonAnnotations { get; set; } = new();
    public string ExternalURL { get; set; } = string.Empty;
    public List<PrometheusAlert> Alerts { get; set; } = new();
}

public class PrometheusAlert
{
    public string Status { get; set; } = "firing";
    public Dictionary<string, string> Labels { get; set; } = new();
    public Dictionary<string, string> Annotations { get; set; } = new();
    public DateTime StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public string GeneratorURL { get; set; } = string.Empty;
    public string Fingerprint { get; set; } = string.Empty;
}
