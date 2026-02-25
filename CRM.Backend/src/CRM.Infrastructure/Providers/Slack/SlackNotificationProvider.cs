// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CRM.Core.Ports.Output.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Providers.Slack;

/// <summary>
/// Slack notification provider implementing INotificationPort via Incoming Webhooks.
/// Formats notifications as Slack Block Kit messages and posts them to a configured channel.
/// Supports email (as rich message), multi-channel, and in-app channels mapped to Slack messages.
/// SMS and Push return not-supported results.
/// Implements TODO-INT-06.
/// </summary>
public class SlackNotificationProvider : INotificationPort
{
    private readonly SlackConfiguration _config;
    private readonly HttpClient _httpClient;
    private readonly ILogger<SlackNotificationProvider> _logger;
    private readonly bool _isConfigured;
    private readonly JsonSerializerOptions _jsonOptions;

    public SlackNotificationProvider(
        IOptions<SlackConfiguration> config,
        HttpClient httpClient,
        ILogger<SlackNotificationProvider> logger)
    {
        _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };

        _isConfigured = _config.IsValid();

        if (_isConfigured)
        {
            _logger.LogInformation("Slack notification provider initialized with webhook");
        }
        else
        {
            _logger.LogWarning("Slack notification provider is not configured — WebhookUrl missing");
        }
    }

    /// <inheritdoc />
    public string ProviderName => "Slack";

    /// <inheritdoc />
    public IEnumerable<string> SupportedChannels => new[] { "slack", "email", "in_app" };

    /// <inheritdoc />
    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_isConfigured);

    #region Email → Slack Block Kit

    /// <inheritdoc />
    /// <remarks>
    /// Sends the email content to Slack as a Block Kit message via the configured Incoming Webhook.
    /// The Subject becomes the header block; the Body (HTML-stripped) becomes the section block.
    /// </remarks>
    public async Task<NotificationResult> SendEmailAsync(
        EmailNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        if (!_isConfigured)
        {
            return NotConfiguredResult("email");
        }

        try
        {
            _logger.LogDebug("Slack: forwarding email notification. Subject: {Subject}", request.Subject);

            var plainBody = StripHtml(request.Body);
            var payload = BuildBlockKitMessage(
                header: request.Subject,
                body: plainBody,
                footer: $"To: {request.To}");

            return await PostToWebhookAsync(payload, "email", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Slack: failed to send email notification. Subject: {Subject}", request.Subject);
            return ErrorResult("email", ex.Message);
        }
    }

    /// <inheritdoc />
    public Task<NotificationResult> SendTemplateEmailAsync(
        string templateId,
        string recipientEmail,
        object data,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Slack: forwarding template email {TemplateId}", templateId);

        var body = data != null
            ? JsonSerializer.Serialize(data, _jsonOptions)
            : "(no data)";

        return SendEmailAsync(new EmailNotificationRequest
        {
            To = recipientEmail,
            Subject = $"Template: {templateId}",
            Body = body,
            IsHtml = false
        }, cancellationToken);
    }

    #endregion

    #region SMS / Push — Not Supported

    /// <inheritdoc />
    public Task<NotificationResult> SendSmsAsync(
        SmsNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Slack provider does not support SMS");
        return Task.FromResult(NotSupportedResult("sms", "Slack Incoming Webhook does not support SMS"));
    }

    /// <inheritdoc />
    public Task<NotificationResult> SendPushAsync(
        PushNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Slack provider does not support Push notifications");
        return Task.FromResult(NotSupportedResult("push", "Slack Incoming Webhook does not support Push notifications"));
    }

    #endregion

    #region In-App → Slack Message

    /// <inheritdoc />
    public async Task<NotificationResult> SendInAppAsync(
        InAppNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        if (!_isConfigured)
        {
            return NotConfiguredResult("in_app");
        }

        try
        {
            _logger.LogDebug("Slack: sending in-app notification for user {UserId}", request.UserId);

            var payload = BuildBlockKitMessage(
                header: request.Title,
                body: request.Content,
                footer: $"User: {request.UserId}");

            return await PostToWebhookAsync(payload, "in_app", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Slack: failed to send in-app notification");
            return ErrorResult("in_app", ex.Message);
        }
    }

    #endregion

    #region Multi-Channel Notification

    /// <inheritdoc />
    public async Task<MultiChannelNotificationResult> SendNotificationAsync(
        MultiChannelNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        if (!_isConfigured)
        {
            return new MultiChannelNotificationResult
            {
                Success = false,
                ChannelResults = new Dictionary<string, NotificationResult>
                {
                    ["slack"] = NotConfiguredResult("slack")
                }
            };
        }

        try
        {
            _logger.LogDebug("Slack: sending multi-channel notification for subscriber {SubscriberId}", request.SubscriberId);

            var subject = request.TemplateId ?? "CRM Notification";
            var body = request.Payload != null
                ? JsonSerializer.Serialize(request.Payload, _jsonOptions)
                : "(no payload)";

            var payload = BuildBlockKitMessage(
                header: subject,
                body: body,
                footer: $"Subscriber: {request.SubscriberId}");

            var result = await PostToWebhookAsync(payload, "slack", cancellationToken);

            return new MultiChannelNotificationResult
            {
                Success = result.Success,
                TransactionId = result.MessageId,
                ChannelResults = new Dictionary<string, NotificationResult> { ["slack"] = result }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Slack: failed to send multi-channel notification");
            var err = ErrorResult("slack", ex.Message);
            return new MultiChannelNotificationResult
            {
                Success = false,
                ChannelResults = new Dictionary<string, NotificationResult> { ["slack"] = err }
            };
        }
    }

    /// <inheritdoc />
    public Task<NotificationResult> TriggerWorkflowAsync(
        string workflowId,
        string subscriberId,
        object payload,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Slack: triggering workflow {WorkflowId} for subscriber {SubscriberId}", workflowId, subscriberId);

        var body = payload != null ? JsonSerializer.Serialize(payload, _jsonOptions) : "(no payload)";

        return SendEmailAsync(new EmailNotificationRequest
        {
            To = subscriberId,
            Subject = $"Workflow: {workflowId}",
            Body = body,
            IsHtml = false
        }, cancellationToken);
    }

    #endregion

    #region Bulk Operations

    /// <inheritdoc />
    public async Task<BulkNotificationResult> SendBulkEmailAsync(
        IEnumerable<EmailNotificationRequest> requests,
        CancellationToken cancellationToken = default)
    {
        var result = new BulkNotificationResult();
        foreach (var req in requests)
        {
            var r = await SendEmailAsync(req, cancellationToken);
            result.Results.Add(r);
            result.TotalCount++;
            if (r.Success) result.SuccessCount++;
            else result.FailureCount++;
        }
        return result;
    }

    /// <inheritdoc />
    public async Task<BulkNotificationResult> SendBulkSmsAsync(
        IEnumerable<SmsNotificationRequest> requests,
        CancellationToken cancellationToken = default)
    {
        var result = new BulkNotificationResult();
        foreach (var req in requests)
        {
            var r = await SendSmsAsync(req, cancellationToken);
            result.Results.Add(r);
            result.TotalCount++;
            if (r.Success) result.SuccessCount++;
            else result.FailureCount++;
        }
        return result;
    }

    #endregion

    #region Subscriber Management — Pass-through

    /// <inheritdoc />
    public Task<string> UpsertSubscriberAsync(SubscriberRequest request, CancellationToken cancellationToken = default)
        => Task.FromResult($"slack_{Guid.NewGuid():N}");

    /// <inheritdoc />
    public Task DeleteSubscriberAsync(string subscriberId, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <inheritdoc />
    public Task<SubscriberPreferences?> GetSubscriberPreferencesAsync(string subscriberId, CancellationToken cancellationToken = default)
        => Task.FromResult<SubscriberPreferences?>(null);

    /// <inheritdoc />
    public Task UpdateSubscriberPreferencesAsync(string subscriberId, SubscriberPreferences preferences, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    #endregion

    #region Delivery Status

    /// <inheritdoc />
    public Task<DeliveryStatus?> GetDeliveryStatusAsync(string notificationId, CancellationToken cancellationToken = default)
        => Task.FromResult<DeliveryStatus?>(null);

    /// <inheritdoc />
    public Task<DeliveryEvent> ProcessDeliveryWebhookAsync(string eventType, string payload, CancellationToken cancellationToken = default)
        => Task.FromResult(new DeliveryEvent
        {
            EventType = eventType,
            NotificationId = string.Empty,
            SubscriberId = string.Empty,
            Channel = "slack",
            Timestamp = DateTime.UtcNow
        });

    #endregion

    /// <inheritdoc />
    public async Task<ProviderHealthResult> HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        var start = DateTime.UtcNow;

        if (!_isConfigured)
        {
            return new ProviderHealthResult
            {
                ProviderName = ProviderName,
                IsHealthy = false,
                Message = "Slack webhook URL not configured",
                CheckedAt = start
            };
        }

        // Send a minimal test payload to verify the webhook URL is still active
        // Slack returns "ok" on a valid webhook POST
        try
        {
            // Build minimal valid payload
            var testPayload = new { text = "_CRM health check_" };
            var response = await _httpClient.PostAsJsonAsync(_config.WebhookUrl, testPayload, _jsonOptions, cancellationToken);
            var ms = (long)(DateTime.UtcNow - start).TotalMilliseconds;

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            var healthy = response.IsSuccessStatusCode && responseBody.Trim() == "ok";

            return new ProviderHealthResult
            {
                ProviderName = ProviderName,
                IsHealthy = healthy,
                Message = healthy ? "Slack webhook is healthy" : $"Unexpected response: {responseBody}",
                ResponseTimeMs = ms,
                CheckedAt = start
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Slack health check failed");
            return new ProviderHealthResult
            {
                ProviderName = ProviderName,
                IsHealthy = false,
                Message = ex.Message,
                CheckedAt = start
            };
        }
    }

    #region Private Helpers

    /// <summary>
    /// Posts a Slack Block Kit payload to the configured Incoming Webhook URL.
    /// </summary>
    private async Task<NotificationResult> PostToWebhookAsync(
        object payload,
        string channel,
        CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync(_config.WebhookUrl, payload, _jsonOptions, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var messageId = $"slack_{Guid.NewGuid():N}";
            _logger.LogInformation("Slack notification sent successfully. MessageId: {MessageId}", messageId);
            return new NotificationResult
            {
                Success = true,
                MessageId = messageId,
                Provider = ProviderName,
                Channel = channel
            };
        }

        _logger.LogError("Slack webhook returned {Status}: {Body}", response.StatusCode, body);
        return new NotificationResult
        {
            Success = false,
            Provider = ProviderName,
            Channel = channel,
            Error = $"HTTP {(int)response.StatusCode}: {body}",
            ErrorCode = ((int)response.StatusCode).ToString()
        };
    }

    /// <summary>
    /// Builds a Slack Block Kit message payload with header, body section, and optional footer.
    /// </summary>
    private object BuildBlockKitMessage(string header, string body, string? footer = null)
    {
        var blocks = new List<object>
        {
            new
            {
                type = "header",
                text = new { type = "plain_text", text = Truncate(header, 150), emoji = true }
            },
            new
            {
                type = "section",
                text = new { type = "mrkdwn", text = Truncate(body, 3000) }
            }
        };

        if (!string.IsNullOrWhiteSpace(footer))
        {
            blocks.Add(new
            {
                type = "context",
                elements = new[]
                {
                    new { type = "mrkdwn", text = footer }
                }
            });
        }

        var payload = new Dictionary<string, object>
        {
            ["username"] = _config.Username,
            ["icon_emoji"] = _config.IconEmoji,
            ["blocks"] = blocks,
            ["text"] = header // Fallback text for notifications
        };

        if (!string.IsNullOrWhiteSpace(_config.Channel))
        {
            payload["channel"] = _config.Channel;
        }

        return payload;
    }

    /// <summary>Strips basic HTML tags from a string for plain-text rendering.</summary>
    private static string StripHtml(string html)
    {
        if (string.IsNullOrWhiteSpace(html)) return string.Empty;
        return System.Text.RegularExpressions.Regex.Replace(html, "<[^>]*>", string.Empty).Trim();
    }

    /// <summary>Truncates a string to avoid exceeding Slack's field limits.</summary>
    private static string Truncate(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        return text.Length <= maxLength ? text : text[..(maxLength - 3)] + "...";
    }

    private NotificationResult NotConfiguredResult(string channel) =>
        new()
        {
            Success = false,
            Provider = ProviderName,
            Channel = channel,
            Error = "Slack notification provider is not configured. Set Providers:Notifications:Slack:WebhookUrl.",
            ErrorCode = "NOT_CONFIGURED"
        };

    private static NotificationResult NotSupportedResult(string channel, string reason) =>
        new()
        {
            Success = false,
            Provider = "Slack",
            Channel = channel,
            Error = reason,
            ErrorCode = "NOT_SUPPORTED"
        };

    private static NotificationResult ErrorResult(string channel, string error) =>
        new()
        {
            Success = false,
            Provider = "Slack",
            Channel = channel,
            Error = error
        };

    #endregion
}
