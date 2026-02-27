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
/// Stub implementation of Microsoft Teams notification service.
/// Can be integrated with Teams webhooks when configured.
/// TODO-SD005-010: Slack/Teams integration for escalation notifications.
/// </summary>
public class TeamsNotificationService : ITeamsNotificationService
{
    private readonly ILogger<TeamsNotificationService> _logger;
    private readonly HttpClient _httpClient;

    public TeamsNotificationService(
        ILogger<TeamsNotificationService> logger,
        IHttpClientFactory? httpClientFactory = null)
    {
        _logger = logger;
        _httpClient = httpClientFactory?.CreateClient("Teams") ?? new HttpClient();
    }

    public async Task<bool> SendChannelMessageAsync(
        string webhookUrl,
        string message,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Teams (stub): Webhook={WebhookUrl}, Message={Message}",
            webhookUrl.Length > 50 ? webhookUrl[..50] + "..." : webhookUrl,
            message.Length > 100 ? message[..100] + "..." : message);

        await Task.Delay(10, cancellationToken);
        return true;
    }

    public async Task<bool> SendAdaptiveCardAsync(
        string webhookUrl,
        string adaptiveCardJson,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Teams Adaptive Card (stub): Webhook={WebhookUrl}, CardLength={CardLength}",
            webhookUrl.Length > 50 ? webhookUrl[..50] + "..." : webhookUrl,
            adaptiveCardJson.Length);

        await Task.Delay(10, cancellationToken);
        return true;
    }

    public async Task<bool> SendEscalationAlertAsync(
        string webhookUrl,
        TeamsEscalationInfo escalationInfo,
        CancellationToken cancellationToken = default)
    {
        var cardJson = CreateEscalationCard(escalationInfo);

        _logger.LogInformation(
            "Teams Escalation Alert (stub): SR={ServiceRequest}, Level={Level}",
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
}
