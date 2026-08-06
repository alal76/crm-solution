// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Net.Http.Json;
using CRM.Core.Interfaces.Notifications;
using CRM.Infrastructure.Providers.Slack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Services.Notifications;

/// <summary>
/// Slack notification service backed by a Slack Incoming Webhook.
/// Posts real Block Kit messages when a webhook URL is configured
/// (Providers:Notifications:Slack:WebhookUrl, env var SLACK_WEBHOOK_URL);
/// falls back to log-only behavior when no webhook URL is configured
/// (a legitimate "not configured yet" state, not an error).
/// TODO-SD005-010: Slack/Teams integration for escalation notifications.
/// </summary>
public class SlackNotificationService : ISlackNotificationService
{
    private readonly ILogger<SlackNotificationService> _logger;
    private readonly HttpClient _httpClient;
    private readonly SlackConfiguration _config;

    public SlackNotificationService(
        ILogger<SlackNotificationService> logger,
        IHttpClientFactory? httpClientFactory = null,
        IOptions<SlackConfiguration>? slackConfig = null)
    {
        _logger = logger;
        _httpClient = httpClientFactory?.CreateClient("Slack") ?? new HttpClient();
        _config = slackConfig?.Value ?? new SlackConfiguration();
    }

    /// <summary>
    /// True when a Slack Incoming Webhook URL has been configured for this environment.
    /// </summary>
    public bool IsConfigured => !string.IsNullOrWhiteSpace(_config.WebhookUrl);

    public async Task<bool> SendChannelMessageAsync(
        string channelId,
        string message,
        CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            _logger.LogInformation(
                "Slack (not configured): Channel={ChannelId}, Message={Message}",
                channelId,
                Truncate(message));
            await Task.Delay(10, cancellationToken);
            return true;
        }

        var payload = BuildPayload(channelId, message);
        return await PostToWebhookAsync(payload, "channel message", cancellationToken);
    }

    public async Task<bool> SendDirectMessageAsync(
        string userId,
        string message,
        CancellationToken cancellationToken = default)
    {
        // Slack Incoming Webhooks always post to a fixed (or channel-overridden)
        // destination; they cannot open a true 1:1 DM without the Slack Web API and a
        // bot token. Same limitation documented for Teams DMs in
        // TeamsNotificationChannelService.SendDirectMessageAsync.
        _logger.LogWarning(
            "Slack direct messaging requires the Slack Web API with a bot token (not supported via Incoming Webhook)");
        _logger.LogInformation(
            "Slack DM (not sent): User={UserId}, Message={Message}",
            userId,
            Truncate(message));

        await Task.Delay(10, cancellationToken);
        return false;
    }

    public async Task<bool> SendEscalationAlertAsync(
        string channelId,
        SlackEscalationInfo escalationInfo,
        CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            _logger.LogInformation(
                "Slack Escalation Alert (not configured): Channel={ChannelId}, SR={ServiceRequest}, Level={Level}",
                channelId,
                escalationInfo.ServiceRequestNumber,
                escalationInfo.EscalationLevel);
            await Task.Delay(10, cancellationToken);
            return true;
        }

        var blocks = new object[]
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
        };

        var payload = BuildPayload(
            channelId,
            $"🚨 Escalation Alert - Level {escalationInfo.EscalationLevel}: {escalationInfo.ServiceRequestNumber}",
            blocks);

        return await PostToWebhookAsync(payload, "escalation alert", cancellationToken);
    }

    public async Task<bool> PostInteractiveMessageAsync(
        string channelId,
        string message,
        IEnumerable<SlackAction> actions,
        CancellationToken cancellationToken = default)
    {
        var actionList = actions.ToList();

        if (!IsConfigured)
        {
            _logger.LogInformation(
                "Slack Interactive (not configured): Channel={ChannelId}, Actions={ActionCount}",
                channelId,
                actionList.Count);
            await Task.Delay(10, cancellationToken);
            return true;
        }

        var blocks = new object[]
        {
            new
            {
                type = "section",
                text = new { type = "mrkdwn", text = message }
            },
            new
            {
                type = "actions",
                elements = actionList.Select(a => new Dictionary<string, object?>
                {
                    ["type"] = "button",
                    ["text"] = new { type = "plain_text", text = a.Text },
                    ["action_id"] = a.ActionId,
                    ["style"] = a.Style is "primary" or "danger" ? a.Style : null,
                    ["value"] = a.Value
                }).ToArray()
            }
        };

        var payload = BuildPayload(channelId, message, blocks);
        return await PostToWebhookAsync(payload, "interactive message", cancellationToken);
    }

    private Dictionary<string, object?> BuildPayload(string channelId, string text, object[]? blocks = null)
    {
        var payload = new Dictionary<string, object?>
        {
            ["text"] = text,
            ["username"] = _config.Username,
            ["icon_emoji"] = _config.IconEmoji
        };

        if (!string.IsNullOrWhiteSpace(channelId))
        {
            payload["channel"] = channelId;
        }

        if (blocks != null)
        {
            payload["blocks"] = blocks;
        }

        return payload;
    }

    private async Task<bool> PostToWebhookAsync(
        object payload,
        string context,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(_config.WebhookUrl, payload, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Slack {Context} sent successfully", context);
                return true;
            }

            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning(
                "Slack webhook returned {StatusCode} for {Context}: {Response}",
                response.StatusCode,
                context,
                responseText);
            return false;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to send Slack {Context}", context);
            return false;
        }
    }

    private static string Truncate(string message) =>
        message.Length > 100 ? message[..100] + "..." : message;
}
