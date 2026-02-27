// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.Text;
using System.Text.Json;
using ISlackIntegrationService = CRM.Core.Ports.Input.ISlackIntegrationService;
using SlackNotificationResult = CRM.Core.Ports.Input.SlackNotificationResult;
using CrmSlackNotification = CRM.Core.Ports.Input.CrmSlackNotification;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.Integrations;

/// <summary>
/// Implementation of ISlackIntegrationService for Slack notifications.
/// Uses Slack incoming webhook URLs and Block Kit for message formatting.
/// Implements TODO-INT-06.
/// </summary>
public class SlackNotificationService : ISlackIntegrationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SlackNotificationService> _logger;

    public SlackNotificationService(
        HttpClient httpClient,
        ILogger<SlackNotificationService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<SlackNotificationResult> SendMessageAsync(
        string webhookUrl,
        string message,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(webhookUrl))
            return SlackNotificationResult.Failed("Webhook URL is required");

        var payload = new { text = message };
        return await PostToSlackAsync(webhookUrl, payload, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<SlackNotificationResult> SendBlocksAsync(
        string webhookUrl,
        string blocks,
        string? fallbackText = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(webhookUrl))
            return SlackNotificationResult.Failed("Webhook URL is required");

        try
        {
            // Parse blocks JSON to validate it
            var blocksArray = JsonSerializer.Deserialize<JsonElement>(blocks);
            var payload = new
            {
                text = fallbackText ?? "CRM Notification",
                blocks = blocksArray
            };

            return await PostToSlackAsync(webhookUrl, payload, cancellationToken);
        }
        catch (JsonException ex)
        {
            return SlackNotificationResult.Failed($"Invalid blocks JSON: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<SlackNotificationResult> SendCrmNotificationAsync(
        string webhookUrl,
        CrmSlackNotification notification,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(webhookUrl))
            return SlackNotificationResult.Failed("Webhook URL is required");

        // Build Block Kit message
        var blocks = new List<object>
        {
            new
            {
                type = "header",
                text = new { type = "plain_text", text = notification.Title, emoji = true }
            },
            new
            {
                type = "section",
                text = new { type = "mrkdwn", text = notification.Message }
            },
            new
            {
                type = "context",
                elements = new object[]
                {
                    new { type = "mrkdwn", text = $"*Entity:* {notification.EntityType} #{notification.EntityId}" },
                    new { type = "mrkdwn", text = $"*Event:* {notification.EventType}" }
                }
            }
        };

        // Add fields if provided
        if (notification.Fields != null && notification.Fields.Count > 0)
        {
            var fields = notification.Fields.Select(kvp => new
            {
                type = "mrkdwn",
                text = $"*{kvp.Key}:*\n{kvp.Value}"
            }).ToArray();

            blocks.Add(new { type = "section", fields });
        }

        // Add action button if entity URL provided
        if (!string.IsNullOrEmpty(notification.EntityUrl))
        {
            blocks.Add(new
            {
                type = "actions",
                elements = new[]
                {
                    new
                    {
                        type = "button",
                        text = new { type = "plain_text", text = "View in CRM", emoji = true },
                        url = notification.EntityUrl,
                        style = "primary"
                    }
                }
            });
        }

        blocks.Add(new { type = "divider" });

        var payload = new
        {
            text = notification.Title,
            attachments = new[]
            {
                new
                {
                    color = notification.Color ?? "#0078D4",
                    blocks
                }
            }
        };

        return await PostToSlackAsync(webhookUrl, payload, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ValidateWebhookAsync(string webhookUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(webhookUrl))
            return false;

        if (!Uri.TryCreate(webhookUrl, UriKind.Absolute, out var uri))
            return false;

        // Slack webhook URLs are at hooks.slack.com
        if (!uri.Host.Contains("hooks.slack.com", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("URL host {Host} does not match expected Slack webhook pattern", uri.Host);
            return false;
        }

        return true;
    }

    private async Task<SlackNotificationResult> PostToSlackAsync(string webhookUrl, object payload, CancellationToken cancellationToken)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await _httpClient.PostAsync(webhookUrl, content, cancellationToken);

            sw.Stop();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("Slack notification sent successfully in {Latency}ms", sw.ElapsedMilliseconds);
                return SlackNotificationResult.Succeeded((int)response.StatusCode, sw.ElapsedMilliseconds);
            }

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning(
                "Slack notification failed with status {StatusCode}: {Body}",
                (int)response.StatusCode, responseBody);

            return SlackNotificationResult.Failed(
                $"HTTP {(int)response.StatusCode}: {responseBody}",
                (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "Error sending Slack notification to {Url}", webhookUrl);
            return SlackNotificationResult.Failed(ex.Message);
        }
    }
}
