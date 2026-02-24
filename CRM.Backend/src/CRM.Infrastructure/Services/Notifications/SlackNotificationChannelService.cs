// CRM Solution - Customer Relationship Management System// CRM Solution - Customer Relationship Management System


































































































































































































}    }            cancellationToken);            ":robot_face:",            "CRM Bot",            ":white_check_mark: CRM Test Notification - This is a test message from CRM Solution to verify Slack integration.",            webhookUrl,        return await SendMessageAsync(    {    public async Task<bool> TestWebhookAsync(string webhookUrl, CancellationToken cancellationToken = default)    /// <inheritdoc />    }        }            return false;            _logger.LogError(ex, "Failed to send Slack Block Kit message");        {        catch (Exception ex)        }            return true;            _logger.LogInformation("Slack stub: Would send Block Kit message");            // Stub mode            }                return response.IsSuccessStatusCode;                var response = await client.PostAsJsonAsync(webhookUrl, payload, cancellationToken);                var client = _httpClientFactory.CreateClient("Slack");            {            if (_httpClientFactory != null)            var payload = new { blocks = blocks };            var blocks = JsonSerializer.Deserialize<object>(blocksJson);        {        try        }            return false;            _logger.LogWarning("Webhook URL and blocks JSON are required");        {        if (string.IsNullOrWhiteSpace(webhookUrl) || string.IsNullOrWhiteSpace(blocksJson))    {        CancellationToken cancellationToken = default)        string blocksJson,        string webhookUrl,    public async Task<bool> SendBlockKitMessageAsync(    /// <inheritdoc />    }        }            return false;            _logger.LogError(ex, "Failed to send Slack rich message");        {        catch (Exception ex)        }            return true;            _logger.LogInformation("Slack stub: Would send rich message with {Count} attachments", attachments.Count());            // Stub mode            }                return response.IsSuccessStatusCode;                var response = await client.PostAsJsonAsync(webhookUrl, payload, cancellationToken);                var client = _httpClientFactory.CreateClient("Slack");            {            if (_httpClientFactory != null)            };                }).ToArray()                    }).ToArray()                        @short = f.Short                        value = f.Value,                        title = f.Title,                    {                    fields = a.Fields?.Select(f => new                    ts = a.Timestamp,                    footer = a.Footer,                    author_name = a.AuthorName,                    text = a.Text,                    title_link = a.TitleLink,                    title = a.Title,                    color = a.Color,                    fallback = a.Fallback,                {                attachments = attachments.Select(a => new                text = text,            {            var payload = new        {        try        }            return false;            _logger.LogWarning("Slack webhook URL is required");        {        if (string.IsNullOrWhiteSpace(webhookUrl))    {        CancellationToken cancellationToken = default)        IEnumerable<SlackAttachment> attachments,        string text,        string webhookUrl,    public async Task<bool> SendRichMessageAsync(    /// <inheritdoc />    }        }            return false;            _logger.LogError(ex, "Failed to send Slack message");        {        catch (Exception ex)        }            return true;            await Task.Delay(10, cancellationToken);                message.Length > 50 ? message.Substring(0, 50) + "..." : message);            _logger.LogInformation("Slack stub: Would send message: {Message}",             // Stub mode            }                return false;                _logger.LogWarning("Slack message failed: {StatusCode} - {Error}", response.StatusCode, errorContent);                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);                }                    return true;                    _logger.LogInformation("Slack message sent successfully");                {                if (response.IsSuccessStatusCode)                var response = await client.PostAsJsonAsync(webhookUrl, payload, cancellationToken);                var client = _httpClientFactory.CreateClient("Slack");            {            if (_httpClientFactory != null)                payload["icon_emoji"] = iconEmoji;            if (!string.IsNullOrWhiteSpace(iconEmoji))                payload["username"] = username;            if (!string.IsNullOrWhiteSpace(username))            };                ["text"] = message            {            var payload = new Dictionary<string, object>        {        try        }            return false;            _logger.LogWarning("Slack message content is required");        {        if (string.IsNullOrWhiteSpace(message))        }            return false;            _logger.LogWarning("Slack webhook URL is required");        {        if (string.IsNullOrWhiteSpace(webhookUrl))    {        CancellationToken cancellationToken = default)        string? iconEmoji = null,        string? username = null,        string message,        string webhookUrl,    public async Task<bool> SendMessageAsync(    /// <inheritdoc />    }        _httpClientFactory = httpClientFactory;        _logger = logger;    {        IHttpClientFactory? httpClientFactory = null)        ILogger<SlackNotificationChannelService> logger,    public SlackNotificationChannelService(    private readonly IHttpClientFactory? _httpClientFactory;    private readonly ILogger<SlackNotificationChannelService> _logger;{public class SlackNotificationChannelService : ISlackNotificationChannel/// </summary>/// TODO-SD005-010: Slack escalation notifications./// Uses incoming webhooks for channel messages./// Stub implementation of Slack notification channel./// <summary>namespace CRM.Infrastructure.Services.Notifications;using Microsoft.Extensions.Logging;using CRM.Core.Interfaces.Notifications;using System.Text.Json;using System.Net.Http.Json;// See the LICENSE file in the root directory for full terms.// the terms of the LICENSE file. Commercial use requires a separate license.// This software is source-available. Non-commercial use is permitted under//// Copyright (C) 2024-2026 Abhishek Lal// Copyright (C) 2024-2026 Abhishek Lal
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
