// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Net.Http.Json;
using System.Text.Json;
using CRM.Core.Interfaces.Notifications;
using Microsoft.Extensions.Logging;
using SlackAttachment = CRM.Core.Interfaces.Notifications.SlackAttachment;

namespace CRM.Infrastructure.Services.Notifications;

/// <summary>
/// Stub implementation of Slack notification channel.
/// Uses Slack incoming webhooks for notifications.
/// TODO-SD005-010: Slack escalation notifications.
/// </summary>
public class SlackNotificationChannelService : ISlackNotificationChannel
{
    private readonly ILogger<SlackNotificationChannelService> _logger;
    private readonly IHttpClientFactory? _httpClientFactory;

    public SlackNotificationChannelService(
        ILogger<SlackNotificationChannelService> logger,
        IHttpClientFactory? httpClientFactory = null)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    /// <inheritdoc />
    public async Task<bool> SendMessageAsync(
        string webhookUrl,
        string message,
        string? username = null,
        string? iconEmoji = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            _logger.LogWarning("Slack webhook URL is required");
            return false;
        }

        try
        {
            var payload = new Dictionary<string, object?>
            {
                ["text"] = message
            };

            if (!string.IsNullOrEmpty(username))
                payload["username"] = username;

            if (!string.IsNullOrEmpty(iconEmoji))
                payload["icon_emoji"] = iconEmoji;

            if (_httpClientFactory != null)
            {
                var client = _httpClientFactory.CreateClient("SlackWebhook");
                var response = await client.PostAsJsonAsync(webhookUrl, payload, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Slack message sent successfully");
                    return true;
                }

                var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Slack webhook returned {StatusCode}: {Response}", 
                    response.StatusCode, responseText);
                return false;
            }

            // Stub mode
            _logger.LogInformation("Slack stub: Would send message to webhook: {Message}", 
                message.Length > 50 ? message.Substring(0, 50) + "..." : message);
            await Task.Delay(10, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send Slack message");
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> SendRichMessageAsync(
        string webhookUrl,
        string text,
        IEnumerable<SlackAttachment> attachments,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            _logger.LogWarning("Slack webhook URL is required");
            return false;
        }

        try
        {
            var attachmentsList = attachments.Select(a => new Dictionary<string, object?>
            {
                ["fallback"] = a.Fallback,
                ["color"] = a.Color,
                ["title"] = a.Title,
                ["title_link"] = a.TitleLink,
                ["text"] = a.Text,
                ["author_name"] = a.AuthorName,
                ["footer"] = a.Footer,
                ["ts"] = a.Timestamp,
                ["fields"] = a.Fields?.Select(f => new Dictionary<string, object>
                {
                    ["title"] = f.Title,
                    ["value"] = f.Value,
                    ["short"] = f.Short
                }).ToList()
            }).ToList();

            var payload = new
            {
                text,
                attachments = attachmentsList
            };

            if (_httpClientFactory != null)
            {
                var client = _httpClientFactory.CreateClient("SlackWebhook");
                var response = await client.PostAsJsonAsync(webhookUrl, payload, cancellationToken);
                return response.IsSuccessStatusCode;
            }

            _logger.LogInformation("Slack stub: Would send rich message with {Count} attachments", 
                attachmentsList.Count);
            await Task.Delay(10, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send Slack rich message");
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> SendBlockKitMessageAsync(
        string webhookUrl,
        string blocksJson,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(webhookUrl) || string.IsNullOrWhiteSpace(blocksJson))
        {
            _logger.LogWarning("Webhook URL and blocks JSON are required");
            return false;
        }

        try
        {
            var blocks = JsonSerializer.Deserialize<JsonElement>(blocksJson);
            var payload = new { blocks };

            if (_httpClientFactory != null)
            {
                var client = _httpClientFactory.CreateClient("SlackWebhook");
                var response = await client.PostAsJsonAsync(webhookUrl, payload, cancellationToken);
                return response.IsSuccessStatusCode;
            }

            _logger.LogInformation("Slack stub: Would send Block Kit message to webhook");
            await Task.Delay(10, cancellationToken);
            return true;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid Block Kit JSON");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send Slack Block Kit message");
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> TestWebhookAsync(string webhookUrl, CancellationToken cancellationToken = default)
    {
        return await SendMessageAsync(
            webhookUrl,
            ":white_check_mark: CRM Integration Test - This is a test message from the CRM system to verify Slack webhook connectivity.",
            "CRM Bot",
            ":robot_face:",
            cancellationToken);
    }
}
