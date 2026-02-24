// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text.Json;
using CRM.Core.Interfaces.Notifications;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.Notifications;

/// <summary>
/// Stub implementation of Slack notification service.
/// Can be integrated with Slack Web API when configured.
/// TODO-SD005-010: Slack/Teams integration for escalation notifications.
/// </summary>
public class SlackNotificationService : ISlackNotificationService
{
    private readonly ILogger<SlackNotificationService> _logger;
    private readonly HttpClient _httpClient;

    public SlackNotificationService(
        ILogger<SlackNotificationService> logger,
        IHttpClientFactory? httpClientFactory = null)
    {
        _logger = logger;
        _httpClient = httpClientFactory?.CreateClient("Slack") ?? new HttpClient();
    }

    public async Task<bool> SendChannelMessageAsync(
        string channelId,
        string message,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Slack (stub): Channel={ChannelId}, Message={Message}",
            channelId,
            message.Length > 100 ? message[..100] + "..." : message);

        await Task.Delay(10, cancellationToken);
        return true;
    }

    public async Task<bool> SendDirectMessageAsync(
        string userId,
        string message,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Slack DM (stub): User={UserId}, Message={Message}",
            userId,
            message.Length > 100 ? message[..100] + "..." : message);

        await Task.Delay(10, cancellationToken);
        return true;
    }

    public async Task<bool> SendEscalationAlertAsync(
        string channelId,
        SlackEscalationInfo escalationInfo,
        CancellationToken cancellationToken = default)
    {
        var blocks = new
        {
            channel = channelId,
            blocks = new object[]
            {
                new
                {
                    type = "header",
                    text = new { type = "plain_text", text = $"🚨 Escalation Alert - Level {escalationInfo.EscalationLevel}" }
                },
                new
                {
                    type = "section",
                    fields = new object[]
                    {
                        new { type = "mrkdwn", text = $"*Ticket:*\n{escalationInfo.ServiceRequestNumber}" },
                        new { type = "mrkdwn", text = $"*Priority:*\n{escalationInfo.Priority}" }
                    }
                },
                new
                {
                    type = "section",
                    text = new { type = "mrkdwn", text = $"*Title:* {escalationInfo.Title}" }
                },
                new
                {
                    type = "context",
                    elements = new object[]
                    {
                        new { type = "mrkdwn", text = $"Assigned to: {escalationInfo.AssignedTo ?? "Unassigned"} | SLA Breach: {escalationInfo.SlaBreachTime:g}" }
                    }
                }
            }
        };

        _logger.LogInformation(
            "Slack Escalation Alert (stub): Channel={ChannelId}, SR={ServiceRequest}, Level={Level}",
            channelId,
            escalationInfo.ServiceRequestNumber,
            escalationInfo.EscalationLevel);

        await Task.Delay(10, cancellationToken);
        return true;
    }

    public async Task<bool> PostInteractiveMessageAsync(
        string channelId,
        string message,
        IEnumerable<SlackAction> actions,
        CancellationToken cancellationToken = default)
    {
        var actionList = actions.ToList();

        _logger.LogInformation(
            "Slack Interactive (stub): Channel={ChannelId}, Actions={ActionCount}",
            channelId,
            actionList.Count);

        await Task.Delay(10, cancellationToken);
        return true;
    }
}
