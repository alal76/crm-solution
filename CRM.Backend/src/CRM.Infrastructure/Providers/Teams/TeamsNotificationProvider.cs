// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CRM.Core.Ports.Output.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Providers.Teams;

/// <summary>
/// Microsoft Teams notification provider implementing INotificationPort via Incoming Webhooks.
/// Formats notifications as Adaptive Cards and posts them to a configured Teams channel.
/// Supports email (as rich card), multi-channel, and in-app channels mapped to Teams messages.
/// SMS and Push return not-supported results.
/// Implements TODO-INT-05.
/// </summary>
public class TeamsNotificationProvider : INotificationPort
{
    private readonly TeamsConfiguration _config;
    private readonly HttpClient _httpClient;
    private readonly ILogger<TeamsNotificationProvider> _logger;
    private readonly bool _isConfigured;
    private readonly JsonSerializerOptions _jsonOptions;

    public TeamsNotificationProvider(
        IOptions<TeamsConfiguration> config,
        HttpClient httpClient,
        ILogger<TeamsNotificationProvider> logger)
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
            _logger.LogInformation("Teams notification provider initialized with webhook");
        }
        else
        {
            _logger.LogWarning("Teams notification provider is not configured — WebhookUrl missing");
        }
    }

    /// <inheritdoc />
    public string ProviderName => "Teams";

    /// <inheritdoc />
    public IEnumerable<string> SupportedChannels => new[] { "teams", "email", "in_app" };

    /// <inheritdoc />
    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_isConfigured);

    #region Email → Teams Adaptive Card

    /// <inheritdoc />
    /// <remarks>
    /// Sends the email content to Teams as an Adaptive Card via the configured Incoming Webhook.
    /// The To, Subject and Body fields are rendered in the card; HTML tags are stripped from the body.
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
            _logger.LogDebug("Forwarding email to Teams. Subject: {Subject}, To: {To}", request.Subject, request.To);

            var plainBody = StripHtml(request.Body);
            var card = BuildAdaptiveCard(request.Subject, plainBody, $"To: {request.To}");
            var payload = WrapCard(card);

            return await PostToWebhookAsync(payload, "email", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Teams: failed to send email notification. Subject: {Subject}", request.Subject);
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
        _logger.LogDebug("Teams: forwarding template email {TemplateId} to Teams", templateId);

        var subject = $"Template: {templateId}";
        var body = data != null
            ? JsonSerializer.Serialize(data, _jsonOptions)
            : "(no data)";

        return SendEmailAsync(new EmailNotificationRequest
        {
            To = recipientEmail,
            Subject = subject,
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
        _logger.LogDebug("Teams provider does not support SMS");
        return Task.FromResult(NotSupportedResult("sms", "Teams Incoming Webhook does not support SMS"));
    }

    /// <inheritdoc />
    public Task<NotificationResult> SendPushAsync(
        PushNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Teams provider does not support Push notifications");
        return Task.FromResult(NotSupportedResult("push", "Teams Incoming Webhook does not support Push notifications"));
    }

    #endregion

    #region In-App → Teams Notification

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
            _logger.LogDebug("Teams: sending in-app notification for user {UserId}", request.UserId);

            var card = BuildAdaptiveCard(request.Title, request.Content, $"User: {request.UserId}");
            var payload = WrapCard(card);
            return await PostToWebhookAsync(payload, "in_app", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Teams: failed to send in-app notification");
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
                    ["teams"] = NotConfiguredResult("teams")
                }
            };
        }

        try
        {
            _logger.LogDebug("Teams: sending multi-channel notification for subscriber {SubscriberId}", request.SubscriberId);

            var subject = request.TemplateId ?? "CRM Notification";
            var body = request.Payload != null
                ? JsonSerializer.Serialize(request.Payload, _jsonOptions)
                : "(no payload)";

            var card = BuildAdaptiveCard(subject, body, $"Subscriber: {request.SubscriberId}");
            var payload = WrapCard(card);
            var result = await PostToWebhookAsync(payload, "teams", cancellationToken);

            return new MultiChannelNotificationResult
            {
                Success = result.Success,
                TransactionId = result.MessageId,
                ChannelResults = new Dictionary<string, NotificationResult> { ["teams"] = result }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Teams: failed to send multi-channel notification");
            var err = ErrorResult("teams", ex.Message);
            return new MultiChannelNotificationResult
            {
                Success = false,
                ChannelResults = new Dictionary<string, NotificationResult> { ["teams"] = err }
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
        _logger.LogDebug("Teams: triggering workflow {WorkflowId} for subscriber {SubscriberId}", workflowId, subscriberId);

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

    #region Subscriber Management — Not Supported

    /// <inheritdoc />
    public Task<string> UpsertSubscriberAsync(SubscriberRequest request, CancellationToken cancellationToken = default)
        => Task.FromResult($"teams_{Guid.NewGuid():N}");

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

    #region Delivery Status — Not Applicable

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
            Channel = "teams",
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
                Message = "Teams webhook URL not configured",
                CheckedAt = start
            };
        }

        // Teams webhooks have no GET health endpoint — just verify URL is reachable
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(5));

            var response = await _httpClient.GetAsync(_config.WebhookUrl, cts.Token);
            var ms = (long)(DateTime.UtcNow - start).TotalMilliseconds;

            // Teams webhook returns 405 MethodNotAllowed on GET — that still means it's reachable
            var healthy = response.StatusCode != System.Net.HttpStatusCode.ServiceUnavailable;
            return new ProviderHealthResult
            {
                ProviderName = ProviderName,
                IsHealthy = healthy,
                Message = healthy ? "Teams webhook reachable" : $"Unexpected status: {response.StatusCode}",
                ResponseTimeMs = ms,
                CheckedAt = start
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Teams health check failed");
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
    /// Posts an Adaptive Card payload to the configured Teams Incoming Webhook URL.
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
            var messageId = $"teams_{Guid.NewGuid():N}";
            _logger.LogInformation("Teams notification sent successfully. MessageId: {MessageId}", messageId);
            return new NotificationResult
            {
                Success = true,
                MessageId = messageId,
                Provider = ProviderName,
                Channel = channel
            };
        }

        _logger.LogError("Teams webhook returned {Status}: {Body}", response.StatusCode, body);
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
    /// Builds an Adaptive Card JSON object with title, body, and optional footer.
    /// </summary>
    private object BuildAdaptiveCard(string title, string body, string? footer = null)
    {
        var bodyItems = new List<object>
        {
            new
            {
                type = "TextBlock",
                text = title,
                weight = "Bolder",
                size = "Medium",
                color = "Accent",
                wrap = true
            },
            new
            {
                type = "TextBlock",
                text = body,
                wrap = true,
                spacing = "Small"
            }
        };

        if (!string.IsNullOrWhiteSpace(footer))
        {
            bodyItems.Add(new
            {
                type = "TextBlock",
                text = footer,
                isSubtle = true,
                size = "Small",
                spacing = "Medium"
            });
        }

        if (_config.IncludeTimestamp)
        {
            bodyItems.Add(new
            {
                type = "TextBlock",
                text = $"{{{{DATE({DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ},SHORT)}}}}",
                isSubtle = true,
                size = "Small"
            });
        }

        return new
        {
            @type = "AdaptiveCard",
            @version = "1.4",
            body = bodyItems,
            msteams = new { width = "Full" }
        };
    }

    /// <summary>
    /// Wraps an Adaptive Card in a Teams Webhook message envelope.
    /// </summary>
    private static object WrapCard(object card)
    {
        return new
        {
            type = "message",
            attachments = new[]
            {
                new
                {
                    contentType = "application/vnd.microsoft.card.adaptive",
                    contentUrl = (string?)null,
                    content = card
                }
            }
        };
    }

    /// <summary>
    /// Strips basic HTML tags from a string for plain-text rendering.
    /// </summary>
    private static string StripHtml(string html)
    {
        if (string.IsNullOrWhiteSpace(html)) return string.Empty;
        return System.Text.RegularExpressions.Regex.Replace(html, "<[^>]*>", string.Empty, System.Text.RegularExpressions.RegexOptions.None, TimeSpan.FromSeconds(1)).Trim();
    }

    private NotificationResult NotConfiguredResult(string channel) =>
        new()
        {
            Success = false,
            Provider = ProviderName,
            Channel = channel,
            Error = "Teams notification provider is not configured. Set Providers:Notifications:Teams:WebhookUrl.",
            ErrorCode = "NOT_CONFIGURED"
        };

    private NotificationResult NotSupportedResult(string channel, string reason) =>
        new()
        {
            Success = false,
            Provider = ProviderName,
            Channel = channel,
            Error = reason,
            ErrorCode = "NOT_SUPPORTED"
        };

    private static NotificationResult ErrorResult(string channel, string error) =>
        new()
        {
            Success = false,
            Provider = "Teams",
            Channel = channel,
            Error = error
        };

    #endregion
}
