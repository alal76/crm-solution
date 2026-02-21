// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
#nullable enable

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace CRM.Api.Hubs;

/// <summary>
/// SignalR hub for real-time AI agent approval workflow notifications.
/// Handles broadcasting approval requests to administrators and notifying
/// requesters of approval outcomes.
/// </summary>
[Authorize]
public class AgentApprovalHub : Hub
{
    #region Constants

    /// <summary>
    /// The name of the SignalR group for users who can approve agent actions.
    /// </summary>
    private const string ApproversGroup = "approvers";

    /// <summary>
    /// Client method name for receiving new approval requests.
    /// </summary>
    private const string ReceiveApprovalRequest = "ReceiveApprovalRequest";

    /// <summary>
    /// Client method name for receiving approval results.
    /// </summary>
    private const string ReceiveApprovalResult = "ReceiveApprovalResult";

    #endregion

    #region Fields

    private readonly ILogger<AgentApprovalHub> _logger;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="AgentApprovalHub"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public AgentApprovalHub(ILogger<AgentApprovalHub> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    #region Connection Lifecycle

    /// <summary>
    /// Called when a client connects. Adds admin users to the "approvers" group
    /// so they receive real-time approval request notifications.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

        _logger.LogInformation(
            "AgentApprovalHub: User {UserId} connected (Role: {Role}, ConnectionId: {ConnectionId})",
            userId,
            role,
            Context.ConnectionId);

        // Add admin users to the approvers group for broadcast notifications
        if (!string.IsNullOrEmpty(role) &&
            (role.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
             role.Equals("SystemAdmin", StringComparison.OrdinalIgnoreCase)))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, ApproversGroup);
            _logger.LogInformation(
                "AgentApprovalHub: User {UserId} added to approvers group",
                userId);
        }

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects.
    /// </summary>
    /// <param name="exception">The exception that caused the disconnect, if any.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (exception is not null)
        {
            _logger.LogWarning(
                exception,
                "AgentApprovalHub: User {UserId} disconnected with error (ConnectionId: {ConnectionId})",
                userId,
                Context.ConnectionId);
        }
        else
        {
            _logger.LogInformation(
                "AgentApprovalHub: User {UserId} disconnected (ConnectionId: {ConnectionId})",
                userId,
                Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    #endregion

    #region Hub Methods

    /// <summary>
    /// Sends an approval request notification to all users in the approvers group.
    /// Called by the server when a new agent action requires human approval.
    /// </summary>
    /// <param name="approvalId">The approval request ID.</param>
    /// <param name="actionDescription">A human-readable description of the action requiring approval.</param>
    /// <param name="tier">The approval tier (e.g., "Standard", "Elevated", "Critical").</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SendApprovalRequest(int approvalId, string actionDescription, string tier)
    {
        _logger.LogInformation(
            "AgentApprovalHub: Broadcasting approval request {ApprovalId} (Tier: {Tier}) to approvers group",
            approvalId,
            tier);

        await Clients.Group(ApproversGroup).SendAsync(
            ReceiveApprovalRequest,
            new
            {
                ApprovalId = approvalId,
                ActionDescription = actionDescription,
                Tier = tier,
                Timestamp = DateTime.UtcNow,
            });
    }

    /// <summary>
    /// Notifies the original requester of the approval result.
    /// Called by the server after an approval is approved or rejected.
    /// </summary>
    /// <param name="approvalId">The approval request ID.</param>
    /// <param name="approved">Whether the action was approved.</param>
    /// <param name="reason">Optional reason for the decision (typically provided on rejection).</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task NotifyApprovalResult(int approvalId, bool approved, string? reason)
    {
        var status = approved ? "approved" : "rejected";
        _logger.LogInformation(
            "AgentApprovalHub: Broadcasting approval result for {ApprovalId}: {Status}",
            approvalId,
            status);

        // Broadcast to all connected clients; the frontend filters by relevance
        await Clients.All.SendAsync(
            ReceiveApprovalResult,
            new
            {
                ApprovalId = approvalId,
                Approved = approved,
                Reason = reason,
                Timestamp = DateTime.UtcNow,
            });
    }

    #endregion
}
