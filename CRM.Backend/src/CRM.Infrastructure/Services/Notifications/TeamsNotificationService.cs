// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Net.Http.Json;
using System.Text.Json;
using CRM.Core.Interfaces.Notifications;
using CRM.Infrastructure.Providers.Teams;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Services.Notifications;

/// <summary>
/// Microsoft Teams notification service backed by a Teams Incoming Webhook.
/// Posts real Adaptive Card messages to the webhook URL supplied by the caller;
/// when the caller does not supply one, falls back to the configured default
/// (Providers:Notifications:Teams:WebhookUrl, env var TEAMS_WEBHOOK_URL).
/// Falls back to log-only behavior when no webhook URL is available from either
/// source (a legitimate "not configured yet" state, not an error).
/// TODO-SD005-010: Slack/Teams integration for escalation notifications.
/// </summary>
public class TeamsNotificationService : ITeamsNotificationService
{
    private readonly ILogger<TeamsNotificationService> _logger;
    private readonly HttpClient _httpClient;
    private readonly TeamsConfiguration _config;

    public TeamsNotificationService(
        ILogger<TeamsNotificationService> logger,
        IHttpClientFactory? httpClientFactory = null,
        IOptions<TeamsConfiguration>? teamsConfig = null)
    {
        _logger = logger;
        _httpClient = httpClientFactory?.CreateClient("Teams") ?? new HttpClient();
        _config = teamsConfig?.Value ?? new TeamsConfiguration();
    }

    public async Task<bool> SendChannelMessageAsync(
        string webhookUrl,
        string message,
        CancellationToken cancellationToken = default)
    {
        var effectiveUrl = ResolveWebhookUrl(webhookUrl);

        if (string.IsNullOrWhiteSpace(effectiveUrl))
        {
            _logger.LogInformation(
                "Teams (not configured): Message={Message}",
                Truncate(message, 100));
            await Task.Delay(10, cancellationToken);
            return true;
        }

        var payload = new
        {
            @type = "MessageCard",
            @context = "http://schema.org/extensions", // NOSONAR - JSON-LD context identifier URI, not a network connection
            themeColor = _config.ThemeColor,
            text = message
        };

        return await PostToWebhookAsync(effectiveUrl, payload, "channel message", cancellationToken);
    }

    public async Task<bool> SendAdaptiveCardAsync(
        string webhookUrl,
        string adaptiveCardJson,
        CancellationToken cancellationToken = default)
    {
        var effectiveUrl = ResolveWebhookUrl(webhookUrl);

        if (string.IsNullOrWhiteSpace(effectiveUrl))
        {
            _logger.LogInformation(
                "Teams Adaptive Card (not configured): CardLength={CardLength}",
                adaptiveCardJson.Length);
            await Task.Delay(10, cancellationToken);
            return true;
        }

        JsonElement cardObject;
        try
        {
            cardObject = JsonSerializer.Deserialize<JsonElement>(adaptiveCardJson);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid adaptive card JSON");
            return false;
        }

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

        return await PostToWebhookAsync(effectiveUrl, payload, "adaptive card", cancellationToken);
    }

    public async Task<bool> SendEscalationAlertAsync(
        string webhookUrl,
        TeamsEscalationInfo escalationInfo,
        CancellationToken cancellationToken = default)
    {
        var cardJson = CreateEscalationCard(escalationInfo);

        _logger.LogDebug(
            "Teams Escalation Alert: SR={ServiceRequest}, Level={Level}",
            escalationInfo.ServiceRequestNumber,
            escalationInfo.EscalationLevel);

        return await SendAdaptiveCardAsync(webhookUrl, cardJson, cancellationToken);
    }

    public string CreateEscalationCard(TeamsEscalationInfo escalationInfo)
    {
        var card = new
        {
            type = "message",
            attachments = new[]
            {
                new
                {
                    contentType = "application/vnd.microsoft.card.adaptive",
                    content = new
                    {
                        type = "AdaptiveCard",
                        version = "1.4",
                        body = new object[]
                        {
                            new
                            {
                                type = "TextBlock",
                                text = $"🚨 Escalation Alert - Level {escalationInfo.EscalationLevel}",
                                weight = "bolder",
                                size = "large",
                                color = "attention"
                            },
                            new
                            {
                                type = "FactSet",
                                facts = new[]
                                {
                                    new { title = "Ticket", value = escalationInfo.ServiceRequestNumber },
                                    new { title = "Title", value = escalationInfo.Title },
                                    new { title = "Priority", value = escalationInfo.Priority },
                                    new { title = "Assigned To", value = escalationInfo.AssignedTo ?? "Unassigned" },
                                    new { title = "SLA Breach", value = escalationInfo.SlaBreachTime.ToString("g") }
                                }
                            }
                        },
                        actions = escalationInfo.Actions?.Select(a => new
                        {
                            type = "Action.OpenUrl",
                            title = a.Title,
                            url = a.Url
                        }).ToArray() ?? Array.Empty<object>()
                    }
                }
            }
        };

        return JsonSerializer.Serialize(card, new JsonSerializerOptions { WriteIndented = false });
    }

    private string? ResolveWebhookUrl(string webhookUrl) =>
        !string.IsNullOrWhiteSpace(webhookUrl) ? webhookUrl : _config.WebhookUrl;

    private async Task<bool> PostToWebhookAsync(
        string webhookUrl,
        object payload,
        string context,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(webhookUrl, payload, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Teams {Context} sent successfully", context);
                return true;
            }

            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning(
                "Teams webhook returned {StatusCode} for {Context}: {Response}",
                response.StatusCode,
                context,
                responseText);
            return false;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to send Teams {Context}", context);
            return false;
        }
    }

    private static string Truncate(string text, int maxLength) =>
        text.Length > maxLength ? text[..maxLength] + "..." : text;
}
