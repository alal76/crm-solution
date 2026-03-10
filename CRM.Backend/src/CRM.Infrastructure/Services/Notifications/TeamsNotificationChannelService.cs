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

namespace CRM.Infrastructure.Services.Notifications;

/// <summary>
/// Stub implementation of Teams notification channel.
/// Uses Microsoft Teams incoming webhooks for notifications.
/// TODO-SD005-010: Teams escalation notifications.
/// </summary>
public class TeamsNotificationChannelService : ITeamsNotificationChannel
{
    private readonly ILogger<TeamsNotificationChannelService> _logger;
    private readonly IHttpClientFactory? _httpClientFactory;

    public TeamsNotificationChannelService(
        ILogger<TeamsNotificationChannelService> logger,
        IHttpClientFactory? httpClientFactory = null)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    /// <inheritdoc />
    public async Task<bool> SendChannelMessageAsync(
        string webhookUrl,
        string title,
        string message,
        Dictionary<string, string>? facts = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            _logger.LogWarning("Teams webhook URL is required");
            return false;
        }

        try
        {
            // Build MessageCard payload
            var factsArray = facts?.Select(f => new { name = f.Key, value = f.Value }).ToArray();

            var messageCard = new
            {
                @type = "MessageCard",
                @context = "http://schema.org/extensions", // NOSONAR - JSON-LD context identifier URI, not a network connection
                themeColor = "0076D7",
                summary = title,
                sections = new[]
                {
                    new
                    {
                        activityTitle = title,
                        facts = factsArray ?? Array.Empty<object>(),
                        text = message,
                        markdown = true
                    }
                }
            };

            if (_httpClientFactory != null)
            {
                var client = _httpClientFactory.CreateClient("TeamsWebhook");
                var response = await client.PostAsJsonAsync(webhookUrl, messageCard, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Teams message sent successfully: {Title}", title);
                    return true;
                }

                _logger.LogWarning("Teams webhook returned {StatusCode}", response.StatusCode);
                return false;
            }

            // Stub mode - just log
            _logger.LogInformation("Teams stub: Would send '{Title}' to webhook", title);
            await Task.Delay(10, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send Teams message");
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> SendDirectMessageAsync(
        string userEmail,
        string message,
        CancellationToken cancellationToken = default)
    {
        // Direct messages require Microsoft Graph API with proper permissions
        // This is a placeholder for future implementation

        _logger.LogWarning("Teams direct messaging requires Graph API integration (not implemented)");
        _logger.LogInformation("Teams stub: Would send DM to {Email}: {Message}",
            userEmail, message.Length > 50 ? message.Substring(0, 50) + "..." : message);

        await Task.Delay(10, cancellationToken);
        return false; // Return false as this is not actually implemented
    }

    /// <inheritdoc />
    public async Task<bool> SendAdaptiveCardAsync(
        string webhookUrl,
        string adaptiveCardJson,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(webhookUrl) || string.IsNullOrWhiteSpace(adaptiveCardJson))
        {
            _logger.LogWarning("Webhook URL and adaptive card JSON are required");
            return false;
        }

        try
        {
            // Validate JSON
            var cardObject = JsonSerializer.Deserialize<JsonElement>(adaptiveCardJson);

            var payload = new
            {
                type = "message",
                attachments = new[]
                {
                    new
                    {
                        contentType = "application/vnd.microsoft.card.adaptive",
                        contentUrl = (string?)null,
                        content = cardObject
                    }
                }
            };

            if (_httpClientFactory != null)
            {
                var client = _httpClientFactory.CreateClient("TeamsWebhook");
                var response = await client.PostAsJsonAsync(webhookUrl, payload, cancellationToken);
                return response.IsSuccessStatusCode;
            }

            _logger.LogInformation("Teams stub: Would send adaptive card to webhook");
            await Task.Delay(10, cancellationToken);
            return true;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid adaptive card JSON");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send Teams adaptive card");
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> TestWebhookAsync(string webhookUrl, CancellationToken cancellationToken = default)
    {
        return await SendChannelMessageAsync(
            webhookUrl,
            "CRM Integration Test",
            "This is a test message from the CRM system to verify Teams webhook connectivity.",
            new Dictionary<string, string>
            {
                { "Timestamp", DateTime.UtcNow.ToString("O") },
                { "Status", "Test" }
            },
            cancellationToken);
    }
}
