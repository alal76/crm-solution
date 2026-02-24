// CRM Solution - Customer Relationship Management System// CRM Solution - Customer Relationship Management System



































































































































































}    }            cancellationToken);            null,            "This is a test message from CRM Solution to verify Teams integration.",            "CRM Test Notification",            webhookUrl,        return await SendChannelMessageAsync(    {    public async Task<bool> TestWebhookAsync(string webhookUrl, CancellationToken cancellationToken = default)    /// <inheritdoc />    }        }            return false;            _logger.LogError(ex, "Failed to send Teams adaptive card");        {        catch (Exception ex)        }            return true;            _logger.LogInformation("Teams stub: Would send adaptive card to webhook");            // Stub mode            }                return response.IsSuccessStatusCode;                var response = await client.PostAsJsonAsync(webhookUrl, payload, cancellationToken);                var client = _httpClientFactory.CreateClient("Teams");            {            if (_httpClientFactory != null)            };                }                    }                        content = JsonSerializer.Deserialize<object>(adaptiveCardJson)                        contentUrl = (string?)null,                        contentType = "application/vnd.microsoft.card.adaptive",                    {                    new                {                attachments = new[]                type = "message",            {            var payload = new        {        try        }            return false;            _logger.LogWarning("Webhook URL and adaptive card JSON are required");        {        if (string.IsNullOrWhiteSpace(webhookUrl) || string.IsNullOrWhiteSpace(adaptiveCardJson))    {        CancellationToken cancellationToken = default)        string adaptiveCardJson,        string webhookUrl,    public async Task<bool> SendAdaptiveCardAsync(    /// <inheritdoc />    }        return false;        await Task.CompletedTask;        _logger.LogWarning("Teams direct messaging requires Graph API integration (not implemented). Email: {Email}", userEmail);        // This is a placeholder for future implementation        // Direct messaging requires Microsoft Graph API access    {        CancellationToken cancellationToken = default)        string message,        string userEmail,    public async Task<bool> SendDirectMessageAsync(    /// <inheritdoc />    }        }            return false;            _logger.LogError(ex, "Failed to send Teams message: {Title}", title);        {        catch (Exception ex)        }            return true;            await Task.Delay(10, cancellationToken);            _logger.LogInformation("Teams stub: Would send to webhook: {Title} - {Message}", title, message);            // Stub mode - no HTTP client            }                return false;                _logger.LogWarning("Teams message failed: {StatusCode} - {Error}", response.StatusCode, errorContent);                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);                }                    return true;                    _logger.LogInformation("Teams message sent successfully: {Title}", title);                {                if (response.IsSuccessStatusCode)                                var response = await client.PostAsJsonAsync(webhookUrl, card, cancellationToken);                var client = _httpClientFactory.CreateClient("Teams");            {            if (_httpClientFactory != null)            };                }                    }                        facts = facts?.Select(kv => new { name = kv.Key, value = kv.Value }).ToArray()                        text = message,                        activityTitle = title,                    {                    new                {                sections = new[]                summary = title,                themeColor = "0076D7",                context = "http://schema.org/extensions",                type = "MessageCard",            {            var card = new            // Build MessageCard payload        {        try        }            return false;            _logger.LogWarning("Teams webhook URL is required");        {        if (string.IsNullOrWhiteSpace(webhookUrl))    {        CancellationToken cancellationToken = default)        Dictionary<string, string>? facts = null,        string message,        string title,        string webhookUrl,    public async Task<bool> SendChannelMessageAsync(    /// <inheritdoc />    }        _httpClientFactory = httpClientFactory;        _logger = logger;    {        IHttpClientFactory? httpClientFactory = null)        ILogger<TeamsNotificationChannelService> logger,    public TeamsNotificationChannelService(    private readonly IHttpClientFactory? _httpClientFactory;    private readonly ILogger<TeamsNotificationChannelService> _logger;{public class TeamsNotificationChannelService : ITeamsNotificationChannel/// </summary>/// TODO-SD005-010: Teams escalation notifications./// Uses incoming webhooks for channel messages./// Stub implementation of Microsoft Teams notification channel./// <summary>namespace CRM.Infrastructure.Services.Notifications;using Microsoft.Extensions.Logging;using CRM.Core.Interfaces.Notifications;using System.Text.Json;using System.Net.Http.Json;// See the LICENSE file in the root directory for full terms.// the terms of the LICENSE file. Commercial use requires a separate license.// This software is source-available. Non-commercial use is permitted under//// Copyright (C) 2024-2026 Abhishek Lal// Copyright (C) 2024-2026 Abhishek Lal
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
                @context = "http://schema.org/extensions",
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
