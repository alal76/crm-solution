// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Net.Http.Json;
using System.Text.Json;
using CRM.Core.Interfaces.ITSM;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.ITSM;

/// <summary>
/// Sends ITSM event notifications to a Slack channel via Incoming Webhook.
/// Configuration section: <c>ItsmNotifications:Slack</c>.
/// TODO-SD005-010: Slack integration for ITSM notifications.
/// </summary>
public sealed class SlackItsmNotificationService : IItsmNotificationChannel
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SlackItsmNotificationService> _logger;
    private readonly string? _webhookUrl;
    private readonly string _channel;

    public string ChannelName => "Slack";
    public bool IsConfigured => !string.IsNullOrWhiteSpace(_webhookUrl);

    public SlackItsmNotificationService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<SlackItsmNotificationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _webhookUrl = configuration["ItsmNotifications:Slack:WebhookUrl"];
        _channel = configuration["ItsmNotifications:Slack:Channel"] ?? "#itsm-alerts";
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
            _logger.LogDebug("Slack ITSM notification skipped — WebhookUrl not configured");
            return;
        }

        var emoji = urgency.ToUpperInvariant() switch
        {
            "CRITICAL" => ":rotating_light:",
            "HIGH" => ":red_circle:",
            "MEDIUM" => ":large_yellow_circle:",
            _ => ":large_blue_circle:"
        };

        var ticketRef = serviceRequestId.HasValue ? $" (SR#{serviceRequestId})" : string.Empty;

        var payload = new
        {
            channel = _channel,
            text = $"{emoji} *{title}*{ticketRef}",
            blocks = new object[]
            {
                new
                {
                    type = "header",
                    text = new { type = "plain_text", text = $"{title}{ticketRef}", emoji = true }
                },
                new
                {
                    type = "section",
                    fields = new[]
                    {
                        new { type = "mrkdwn", text = $"*Urgency:*\n{urgency}" },
                        new { type = "mrkdwn", text = $"*Details:*\n{body}" }
                    }
                }
            }
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync(_webhookUrl, payload, ct);
            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning(
                    "Slack ITSM notification failed: HTTP {Status} — {Body}",
                    (int)response.StatusCode,
                    responseBody);
            }
            else
            {
                _logger.LogInformation(
                    "Slack ITSM notification sent: {Title} (urgency={Urgency})",
                    title,
                    urgency);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error sending Slack ITSM notification: {Title}", title);
        }
    }
}
