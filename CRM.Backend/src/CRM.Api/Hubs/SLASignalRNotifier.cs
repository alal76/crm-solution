// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Interfaces.ITSM;
using Microsoft.AspNetCore.SignalR;

namespace CRM.Api.Hubs;

/// <summary>
/// Implementation of ISLASignalRNotifier using SignalR hub context
/// to push real-time SLA countdown updates to connected clients.
/// </summary>
public class SLASignalRNotifier : ISLASignalRNotifier
{
    private readonly IHubContext<SLACountdownHub> _hubContext;
    private readonly ILogger<SLASignalRNotifier> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SLASignalRNotifier"/> class.
    /// </summary>
    public SLASignalRNotifier(
        IHubContext<SLACountdownHub> hubContext,
        ILogger<SLASignalRNotifier> logger)
    {
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task NotifySLAUpdate(int serviceRequestId, SLACountdownDto countdown)
    {
        try
        {
            _logger.LogDebug(
                "Sending SLA update for ServiceRequest {ServiceRequestId}: Status={Status}, ResponseRemaining={ResponseRemaining}, ResolutionRemaining={ResolutionRemaining}",
                serviceRequestId, countdown.Status, countdown.ResponseTimeRemaining, countdown.ResolutionTimeRemaining);

            // Notify the ticket-specific group
            await _hubContext.Clients
                .Group($"sla-{serviceRequestId}")
                .SendAsync("SLAUpdate", countdown);

            // Also notify the all-SLA dashboard group
            await _hubContext.Clients
                .Group("sla-all")
                .SendAsync("SLAUpdate", countdown);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send SLA update notification for ServiceRequest {ServiceRequestId}",
                serviceRequestId);
        }
    }

    /// <inheritdoc />
    public async Task NotifySLABreach(int serviceRequestId, string breachType)
    {
        try
        {
            _logger.LogWarning(
                "SLA BREACH for ServiceRequest {ServiceRequestId}: Type={BreachType}",
                serviceRequestId, breachType);

            var payload = new
            {
                ServiceRequestId = serviceRequestId,
                BreachType = breachType,
                Timestamp = DateTime.UtcNow
            };

            // Notify the ticket-specific group
            await _hubContext.Clients
                .Group($"sla-{serviceRequestId}")
                .SendAsync("SLABreach", payload);

            // Also notify the all-SLA dashboard group
            await _hubContext.Clients
                .Group("sla-all")
                .SendAsync("SLABreach", payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send SLA breach notification for ServiceRequest {ServiceRequestId}",
                serviceRequestId);
        }
    }

    /// <inheritdoc />
    public async Task NotifySLAWarning(int serviceRequestId, string warningType, TimeSpan timeRemaining)
    {
        try
        {
            _logger.LogInformation(
                "SLA WARNING for ServiceRequest {ServiceRequestId}: Type={WarningType}, TimeRemaining={TimeRemaining}",
                serviceRequestId, warningType, timeRemaining);

            var payload = new
            {
                ServiceRequestId = serviceRequestId,
                WarningType = warningType,
                TimeRemaining = timeRemaining.ToString(@"d\.hh\:mm\:ss"),
                TimeRemainingMinutes = timeRemaining.TotalMinutes,
                Timestamp = DateTime.UtcNow
            };

            // Notify the ticket-specific group
            await _hubContext.Clients
                .Group($"sla-{serviceRequestId}")
                .SendAsync("SLAWarning", payload);

            // Also notify the all-SLA dashboard group
            await _hubContext.Clients
                .Group("sla-all")
                .SendAsync("SLAWarning", payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send SLA warning notification for ServiceRequest {ServiceRequestId}",
                serviceRequestId);
        }
    }
}
