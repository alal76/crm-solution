// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Net.Http.Json;
using CRM.Core.Interfaces.ITSM;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.ITSM;

/// <summary>
/// Sends ITSM event notifications to Microsoft Teams via Incoming Webhook.
/// Configuration section: <c>ItsmNotifications:Teams</c>.
/// TODO-SD005-010: Teams integration for ITSM notifications.
/// </summary>
public sealed class TeamsItsmNotificationService : IItsmNotificationChannel
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TeamsItsmNotificationService> _logger;
    private readonly string? _webhookUrl;

    public string ChannelName => "Teams";
    public bool IsConfigured => !string.IsNullOrWhiteSpace(_webhookUrl);

    public TeamsItsmNotificationService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<TeamsItsmNotificationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _webhookUrl = configuration["ItsmNotifications:Teams:WebhookUrl"];
    }

    /// <inheritdoc/>
    public async Task SendNotificationAsync(
        string title,
        string body,
        string urgency,
        int? serviceRequestId = null,
        CancellationToken ct = default)
    {
        if (!IsConfigured)
        {
            _logger.LogDebug("Teams ITSM notification skipped — WebhookUrl not configured");
            return;
        }

        var themeColor = urgency.ToUpperInvariant() switch
        {
            "CRITICAL" => "FF0000",
            "HIGH"     => "FF6600",
            "MEDIUM"   => "FFC300",
            _          => "0076D7"
        };

        var ticketRef = serviceRequestId.HasValue ? $" (SR#{serviceRequestId})" : string.Empty;

        // Teams Adaptive Card via Incoming Webhook (MessageCard format for broad compatibility)
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
                                size = "Medium",
                                weight = "Bolder",
                                text = $"{title}{ticketRef}",
                                color = urgency.ToUpperInvariant() is "CRITICAL" or "HIGH" ? "Attention" : "Default"
                            },
                            new
                            {
                                type = "FactSet",
                                facts = new[]
                                {
                                    new { title = "Urgency", value = urgency },
                                    new { title = "Details", value = body }
                                }
                            }
                        }
                    }
                }
            }
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync(_webhookUrl, card, ct);
            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning(
                    "Teams ITSM notification failed: HTTP {Status} — {Body}",
                    (int)response.StatusCode,
                    responseBody);
            }
            else
            {
                _logger.LogInformation(
                    "Teams ITSM notification sent: {Title} (urgency={Urgency})",
                    title,
                    urgency);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error sending Teams ITSM notification: {Title}", title);
        }
    }
}
