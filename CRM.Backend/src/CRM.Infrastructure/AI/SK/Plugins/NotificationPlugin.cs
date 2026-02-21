// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
#nullable enable

using System.ComponentModel;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.AI.SK.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace CRM.Infrastructure.AI.SK.Plugins;

/// <summary>
/// Semantic Kernel plugin for CRM notification operations.
/// Provides AI-accessible functions for viewing notification history and sending notifications.
/// </summary>
public class NotificationPlugin : CrmPluginBase
{
    private readonly INotificationPort _notificationPort;
    private readonly ICrmDbContext _context;

    /// <inheritdoc />
    public override string PluginName => "Notification";

    /// <inheritdoc />
    public override string Description => "Manage CRM notifications — view notification/message history and send email notifications.";

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationPlugin"/> class.
    /// </summary>
    /// <param name="notificationPort">The notification port for sending notifications.</param>
    /// <param name="context">The database context for querying message history.</param>
    /// <param name="logger">The logger instance.</param>
    public NotificationPlugin(
        INotificationPort notificationPort,
        ICrmDbContext context,
        ILogger<NotificationPlugin> logger) : base(logger)
    {
        _notificationPort = notificationPort ?? throw new ArgumentNullException(nameof(notificationPort));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    #region Read Operations

    /// <summary>
    /// Retrieves recent communication message history from the CRM.
    /// </summary>
    /// <param name="maxResults">Maximum number of messages to return. Defaults to 25.</param>
    /// <param name="direction">Optional filter by direction: "Inbound" or "Outbound".</param>
    /// <returns>A JSON array of recent communication messages.</returns>
    [KernelFunction("GetNotificationHistory")]
    [Description("Get recent communication/notification history from the CRM unified inbox.")]
    public async Task<string> GetNotificationHistoryAsync(
        [Description("Maximum number of messages to return")] int maxResults = 25,
        [Description("Filter by direction: Inbound, Outbound, or leave empty for all")] string? direction = null)
    {
        try
        {
            var query = _context.CommunicationMessages
                .Where(m => !m.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrEmpty(direction) && Enum.TryParse<CRM.Core.Entities.MessageDirection>(direction, true, out var dir))
            {
                query = query.Where(m => m.Direction == dir);
            }

            var messages = await query
                .OrderByDescending(m => m.CreatedAt)
                .Take(maxResults)
                .Select(m => new
                {
                    m.Id,
                    ChannelType = m.ChannelType.ToString(),
                    Direction = m.Direction.ToString(),
                    Status = m.Status.ToString(),
                    m.Subject,
                    m.Body,
                    m.CreatedAt
                })
                .ToListAsync();

            return SuccessResult(new { totalReturned = messages.Count, messages });
        }
        catch (Exception ex)
        {
            return ErrorResult("GetNotificationHistory", ex.Message);
        }
    }

    /// <summary>
    /// Gets a summary of notification statistics across channels and statuses.
    /// </summary>
    /// <param name="daysBack">Number of days to look back. Defaults to 7.</param>
    /// <returns>A JSON object with counts grouped by channel type and status.</returns>
    [KernelFunction("GetNotificationStats")]
    [Description("Get notification statistics grouped by channel and status for a given period.")]
    public async Task<string> GetNotificationStatsAsync(
        [Description("Number of days to look back")] int daysBack = 7)
    {
        try
        {
            var fromDate = DateTime.UtcNow.AddDays(-daysBack);

            var messages = await _context.CommunicationMessages
                .Where(m => !m.IsDeleted && m.CreatedAt >= fromDate)
                .ToListAsync();

            var byChannel = messages
                .GroupBy(m => m.ChannelType.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            var byStatus = messages
                .GroupBy(m => m.Status.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            var byDirection = messages
                .GroupBy(m => m.Direction.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            return SuccessResult(new
            {
                period = $"Last {daysBack} days",
                totalMessages = messages.Count,
                byChannel,
                byStatus,
                byDirection
            });
        }
        catch (Exception ex)
        {
            return ErrorResult("GetNotificationStats", ex.Message);
        }
    }

    #endregion

    #region Write Operations

    /// <summary>
    /// Sends an email notification via the configured notification provider.
    /// </summary>
    /// <param name="to">The recipient email address.</param>
    /// <param name="subject">The email subject line.</param>
    /// <param name="body">The email body content (HTML supported).</param>
    /// <returns>A JSON object indicating send success or failure.</returns>
    [KernelFunction("SendNotification")]
    [Description("Send an email notification to a recipient via the configured notification provider.")]
    [RequiresApproval(Tier = "standard", Description = "Sends an email notification to an external recipient")]
    public async Task<string> SendNotificationAsync(
        [Description("Recipient email address")] string to,
        [Description("Email subject line")] string subject,
        [Description("Email body content (HTML supported)")] string body)
    {
        try
        {
            var request = new EmailNotificationRequest
            {
                To = to,
                Subject = subject,
                Body = body,
                IsHtml = true
            };

            var result = await _notificationPort.SendEmailAsync(request);

            return SuccessResult(new
            {
                success = result.Success,
                messageId = result.MessageId,
                provider = result.Provider,
                message = result.Success ? "Notification sent successfully" : result.Error
            });
        }
        catch (Exception ex)
        {
            return ErrorResult("SendNotification", ex.Message);
        }
    }

    #endregion
}
