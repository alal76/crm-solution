// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CRM.Api.Hubs;

/// <summary>
/// SignalR Hub for real-time SLA countdown notifications.
/// Enables clients to subscribe to SLA updates for specific service requests
/// or to receive all SLA notifications system-wide.
///
/// FEATURES:
/// - Subscribe/unsubscribe to individual ticket SLA countdowns
/// - Subscribe to all SLA updates (dashboard view)
/// - Receive SLA breach and warning notifications in real-time
/// </summary>
[Authorize]
public class SLACountdownHub : Hub
{
    private readonly ILogger<SLACountdownHub> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SLACountdownHub"/> class.
    /// </summary>
    public SLACountdownHub(ILogger<SLACountdownHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Subscribe the current connection to SLA updates for a specific service request.
    /// </summary>
    /// <param name="serviceRequestId">The service request ID to subscribe to.</param>
    public async Task SubscribeToTicket(int serviceRequestId)
    {
        var userId = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst("userId")?.Value;
        _logger.LogInformation(
            "User {UserId} subscribing to SLA updates for ServiceRequest {ServiceRequestId}. ConnectionId: {ConnectionId}",
            userId, serviceRequestId, Context.ConnectionId);

        await Groups.AddToGroupAsync(Context.ConnectionId, $"sla-{serviceRequestId}");
    }

    /// <summary>
    /// Unsubscribe the current connection from SLA updates for a specific service request.
    /// </summary>
    /// <param name="serviceRequestId">The service request ID to unsubscribe from.</param>
    public async Task UnsubscribeFromTicket(int serviceRequestId)
    {
        var userId = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst("userId")?.Value;
        _logger.LogInformation(
            "User {UserId} unsubscribing from SLA updates for ServiceRequest {ServiceRequestId}. ConnectionId: {ConnectionId}",
            userId, serviceRequestId, Context.ConnectionId);

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"sla-{serviceRequestId}");
    }

    /// <summary>
    /// Subscribe the current connection to receive all SLA updates system-wide.
    /// Useful for SLA dashboard views.
    /// </summary>
    public async Task SubscribeToAllSLA()
    {
        var userId = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst("userId")?.Value;
        _logger.LogInformation(
            "User {UserId} subscribing to all SLA updates. ConnectionId: {ConnectionId}",
            userId, Context.ConnectionId);

        await Groups.AddToGroupAsync(Context.ConnectionId, "sla-all");
    }

    /// <summary>
    /// Unsubscribe the current connection from the all-SLA updates group.
    /// </summary>
    public async Task UnsubscribeFromAllSLA()
    {
        var userId = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst("userId")?.Value;
        _logger.LogInformation(
            "User {UserId} unsubscribing from all SLA updates. ConnectionId: {ConnectionId}",
            userId, Context.ConnectionId);

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "sla-all");
    }

    /// <summary>
    /// Called when a client connects to the SLA countdown hub.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst("userId")?.Value;
        _logger.LogInformation(
            "User {UserId} connected to SLA countdown hub. ConnectionId: {ConnectionId}",
            userId, Context.ConnectionId);

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects from the SLA countdown hub.
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst("userId")?.Value;
        if (exception != null)
        {
            _logger.LogWarning(exception,
                "User {UserId} disconnected from SLA countdown hub with error. ConnectionId: {ConnectionId}",
                userId, Context.ConnectionId);
        }
        else
        {
            _logger.LogInformation(
                "User {UserId} disconnected from SLA countdown hub. ConnectionId: {ConnectionId}",
                userId, Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }
}
