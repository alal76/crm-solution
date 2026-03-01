// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text.Json;
using CRM.Core.Ports.Output.Providers;
using global::SendGrid;
using global::SendGrid.Helpers.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ISendGridClient = global::SendGrid.ISendGridClient;
using SendGridClient = global::SendGrid.SendGridClient;
using SendGridEmailAddress = global::SendGrid.Helpers.Mail.EmailAddress;
using SendGridMessage = global::SendGrid.Helpers.Mail.SendGridMessage;

namespace CRM.Infrastructure.Providers.SendGrid;

/// <summary>
/// SendGrid implementation of INotificationPort for email notifications.
/// Supports transactional email, templates, and bulk email operations.
/// </summary>
public class SendGridProvider : INotificationPort
{
    private readonly SendGridConfiguration _config;
    private readonly ISendGridClient _client;
    private readonly ILogger<SendGridProvider> _logger;

    /// <summary>
    /// Initializes a new instance of SendGridProvider.
    /// </summary>
    public SendGridProvider(
        IOptions<SendGridConfiguration> options,
        ILogger<SendGridProvider> logger)
    {
        _config = options.Value;
        _logger = logger;
        _client = new SendGridClient(_config.ApiKey);
    }

    /// <summary>
    /// Constructor for testing with custom client.
    /// </summary>
    public SendGridProvider(
        IOptions<SendGridConfiguration> options,
        ISendGridClient client,
        ILogger<SendGridProvider> logger)
    {
        _config = options.Value;
        _client = client;
        _logger = logger;
    }

    /// <inheritdoc />
    public string ProviderName => "SendGrid";

    /// <inheritdoc />
    public IEnumerable<string> SupportedChannels => new[] { "email" };

    /// <inheritdoc />
    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_config.IsValid());
    }

    #region Email Operations

    /// <inheritdoc />
    public async Task<NotificationResult> SendEmailAsync(
        EmailNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_config.IsValid())
        {
            return new NotificationResult
            {
                Success = false,
                Error = "SendGrid configuration is invalid",
                Provider = ProviderName,
                Channel = "email"
            };
        }

        try
        {
            if (_config.TestMode)
            {
                _logger.LogInformation("TEST MODE: Would send email to {To}: {Subject}",
                    request.To, request.Subject);
                return new NotificationResult
                {
                    Success = true,
                    MessageId = $"test_{Guid.NewGuid():N}",
                    Provider = ProviderName,
                    Channel = "email"
                };
            }

            var msg = BuildSendGridMessage(request);

            // Enable sandbox mode if configured
            if (_config.SandboxMode)
            {
                msg.MailSettings = new MailSettings
                {
                    SandboxMode = new SandboxMode { Enable = true }
                };
            }

            var response = await _client.SendEmailAsync(msg, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                // Get message ID from headers
                var messageId = response.Headers.TryGetValues("X-Message-Id", out var values)
                    ? values.FirstOrDefault()
                    : Guid.NewGuid().ToString();

                _logger.LogInformation("Email sent successfully to {To}. MessageId: {MessageId}",
                    request.To, messageId);

                return new NotificationResult
                {
                    Success = true,
                    MessageId = messageId,
                    Provider = ProviderName,
                    Channel = "email"
                };
            }
            else
            {
                var errorBody = await response.Body.ReadAsStringAsync(cancellationToken);
                _logger.LogError("SendGrid API error: {StatusCode} - {Body}",
                    response.StatusCode, errorBody);

                return new NotificationResult
                {
                    Success = false,
                    Error = $"SendGrid API error: {response.StatusCode} - {errorBody}",
                    Provider = ProviderName,
                    Channel = "email"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {To}", request.To);
            return new NotificationResult
            {
                Success = false,
                Error = ex.Message,
                Provider = ProviderName,
                Channel = "email"
            };
        }
    }

    /// <inheritdoc />
    public async Task<NotificationResult> SendTemplateEmailAsync(
        string templateId,
        string recipientEmail,
        object data,
        CancellationToken cancellationToken = default)
    {
        if (!_config.IsValid())
        {
            return new NotificationResult
            {
                Success = false,
                Error = "SendGrid configuration is invalid",
                Provider = ProviderName
            };
        }

        try
        {
            var msg = new SendGridMessage
            {
                From = new SendGridEmailAddress(_config.FromEmail, _config.FromName),
                TemplateId = templateId
            };

            msg.AddTo(new SendGridEmailAddress(recipientEmail));

            // Convert data to dynamic template data
            if (data is Dictionary<string, object> dict)
            {
                msg.SetTemplateData(dict);
            }
            else
            {
                // Serialize and deserialize to get a dictionary
                var json = JsonSerializer.Serialize(data);
                var templateData = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                if (templateData != null)
                {
                    msg.SetTemplateData(templateData);
                }
            }

            ApplyTrackingSettings(msg);

            var response = await _client.SendEmailAsync(msg, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var messageId = response.Headers.TryGetValues("X-Message-Id", out var values)
                    ? values.FirstOrDefault()
                    : Guid.NewGuid().ToString();

                return new NotificationResult
                {
                    Success = true,
                    MessageId = messageId,
                    Provider = ProviderName,
                    Channel = "email"
                };
            }
            else
            {
                var errorBody = await response.Body.ReadAsStringAsync(cancellationToken);
                return new NotificationResult
                {
                    Success = false,
                    Error = $"SendGrid template error: {response.StatusCode} - {errorBody}",
                    Provider = ProviderName
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending template email to {Email}", recipientEmail);
            return new NotificationResult
            {
                Success = false,
                Error = ex.Message,
                Provider = ProviderName
            };
        }
    }

    /// <inheritdoc />
    public async Task<BulkNotificationResult> SendBulkEmailAsync(
        IEnumerable<EmailNotificationRequest> requests,
        CancellationToken cancellationToken = default)
    {
        var requestList = requests.ToList();
        var results = new List<NotificationResult>();
        var successCount = 0;
        var failureCount = 0;

        // Process in batches
        var batches = requestList
            .Select((r, i) => new { Request = r, Index = i })
            .GroupBy(x => x.Index / _config.MaxBatchSize)
            .Select(g => g.Select(x => x.Request).ToList());

        foreach (var batch in batches)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            // For bulk, send individually but in parallel with rate limiting
            var tasks = batch.Select(async request =>
            {
                var result = await SendEmailAsync(request, cancellationToken);
                return result;
            });

            var batchResults = await Task.WhenAll(tasks);

            foreach (var result in batchResults)
            {
                results.Add(result);
                if (result.Success)
                {
                    successCount++;
                }
                else
                {
                    failureCount++;
                }
            }

            // Respect rate limiting between batches
            if (_config.RateLimitPerSecond > 0 && !_config.TestMode)
            {
                var delay = (batch.Count * 1000) / _config.RateLimitPerSecond;
                await Task.Delay(delay, cancellationToken);
            }
        }

        return new BulkNotificationResult
        {
            TotalCount = requestList.Count,
            SuccessCount = successCount,
            FailureCount = failureCount,
            Results = results
        };
    }

    private SendGridMessage BuildSendGridMessage(EmailNotificationRequest request)
    {
        var msg = new SendGridMessage
        {
            From = new SendGridEmailAddress(
                request.From ?? _config.FromEmail,
                request.FromName ?? _config.FromName),
            Subject = request.Subject
        };

        msg.AddTo(new SendGridEmailAddress(request.To, request.ToName));

        // Set content
        if (request.IsHtml)
        {
            msg.HtmlContent = request.Body;
            if (!string.IsNullOrEmpty(request.PlainTextBody))
            {
                msg.PlainTextContent = request.PlainTextBody;
            }
        }
        else
        {
            msg.PlainTextContent = request.Body;
        }

        // Add CC recipients
        if (request.Cc?.Any() == true)
        {
            msg.AddCcs(request.Cc.Select(email => new SendGridEmailAddress(email)).ToList());
        }

        // Add BCC recipients
        if (request.Bcc?.Any() == true)
        {
            msg.AddBccs(request.Bcc.Select(email => new SendGridEmailAddress(email)).ToList());
        }

        // Set reply-to
        if (!string.IsNullOrEmpty(request.ReplyTo))
        {
            msg.ReplyTo = new SendGridEmailAddress(request.ReplyTo);
        }
        else if (!string.IsNullOrEmpty(_config.ReplyToEmail))
        {
            msg.ReplyTo = new SendGridEmailAddress(_config.ReplyToEmail);
        }

        // Add attachments
        if (request.Attachments?.Any() == true)
        {
            foreach (var attachment in request.Attachments)
            {
                if (attachment.Content != null)
                {
                    msg.AddAttachment(
                        attachment.FileName,
                        Convert.ToBase64String(attachment.Content),
                        attachment.ContentType,
                        attachment.IsInline ? "inline" : "attachment",
                        attachment.ContentId);
                }
            }
        }

        // Add custom headers
        if (request.Headers?.Any() == true)
        {
            foreach (var header in request.Headers)
            {
                msg.AddHeader(header.Key, header.Value);
            }
        }

        // Add categories/tags
        var categories = new List<string>();
        if (_config.DefaultCategories?.Any() == true)
        {
            categories.AddRange(_config.DefaultCategories);
        }
        if (request.Tags?.Any() == true)
        {
            categories.AddRange(request.Tags);
        }
        if (categories.Any())
        {
            msg.AddCategories(categories);
        }

        // Apply tracking settings
        ApplyTrackingSettings(msg);

        return msg;
    }

    private void ApplyTrackingSettings(SendGridMessage msg)
    {
        msg.TrackingSettings = new TrackingSettings
        {
            ClickTracking = new ClickTracking
            {
                Enable = _config.EnableClickTracking,
                EnableText = _config.EnableClickTracking
            },
            OpenTracking = new OpenTracking
            {
                Enable = _config.EnableOpenTracking
            }
        };

        if (_config.EnableUnsubscribeTracking)
        {
            msg.TrackingSettings.SubscriptionTracking = new SubscriptionTracking
            {
                Enable = true
            };
        }
    }

    #endregion

    #region SMS Operations (Not Supported - Use Twilio)

    /// <inheritdoc />
    public Task<NotificationResult> SendSmsAsync(
        SmsNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("SMS not supported by SendGrid provider. Use Twilio for SMS.");
        return Task.FromResult(new NotificationResult
        {
            Success = false,
            Error = "SMS not supported by SendGrid. Use Twilio provider for SMS.",
            Provider = ProviderName,
            Channel = "sms"
        });
    }

    /// <inheritdoc />
    public Task<BulkNotificationResult> SendBulkSmsAsync(
        IEnumerable<SmsNotificationRequest> requests,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new BulkNotificationResult
        {
            TotalCount = requests.Count(),
            SuccessCount = 0,
            FailureCount = requests.Count(),
            Results = requests.Select(r => new NotificationResult
            {
                Success = false,
                Error = "SMS not supported by SendGrid",
                Provider = ProviderName
            }).ToList()
        });
    }

    #endregion

    #region Push/In-App (Not Supported)

    /// <inheritdoc />
    public Task<NotificationResult> SendPushAsync(
        PushNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new NotificationResult
        {
            Success = false,
            Error = "Push notifications not supported by SendGrid.",
            Provider = ProviderName,
            Channel = "push"
        });
    }

    /// <inheritdoc />
    public Task<NotificationResult> SendInAppAsync(
        InAppNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new NotificationResult
        {
            Success = false,
            Error = "In-app notifications not supported by SendGrid.",
            Provider = ProviderName,
            Channel = "in_app"
        });
    }

    #endregion

    #region Multi-Channel Operations

    /// <inheritdoc />
    public async Task<MultiChannelNotificationResult> SendNotificationAsync(
        MultiChannelNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        var channelResults = new Dictionary<string, NotificationResult>();
        var overallSuccess = false;

        // Extract email, subject, and body from Content dictionary
        var email = request.Content.TryGetValue("email", out var emailObj) ? emailObj?.ToString() : null;
        var subject = request.Content.TryGetValue("subject", out var subjectObj) ? subjectObj?.ToString() : "Notification";
        var body = request.Content.TryGetValue("body", out var bodyObj) ? bodyObj?.ToString() : null;
        var message = request.Content.TryGetValue("message", out var msgObj) ? msgObj?.ToString() : body;

        foreach (var channel in request.Channels ?? new List<string> { "email" })
        {
            if (channel.ToLowerInvariant() == "email" && !string.IsNullOrEmpty(email))
            {
                var result = await SendEmailAsync(new EmailNotificationRequest
                {
                    To = email,
                    Subject = subject ?? "Notification",
                    Body = message ?? string.Empty,
                    IsHtml = true
                }, cancellationToken);

                channelResults["email"] = result;
                if (result.Success)
                {
                    overallSuccess = true;
                }
            }
            else
            {
                channelResults[channel] = new NotificationResult
                {
                    Success = false,
                    Error = $"Channel '{channel}' not supported by SendGrid",
                    Provider = ProviderName,
                    Channel = channel
                };
            }
        }

        return new MultiChannelNotificationResult
        {
            Success = overallSuccess,
            ChannelResults = channelResults
        };
    }

    /// <inheritdoc />
    public Task<NotificationResult> TriggerWorkflowAsync(
        string workflowId,
        string subscriberId,
        object payload,
        CancellationToken cancellationToken = default)
    {
        // Use template ID as workflow ID
        return SendTemplateEmailAsync(workflowId, subscriberId, payload, cancellationToken);
    }

    #endregion

    #region Subscriber Management (Minimal)

    /// <inheritdoc />
    public Task<string> UpsertSubscriberAsync(
        SubscriberRequest request,
        CancellationToken cancellationToken = default)
    {
        // SendGrid doesn't have subscriber management in the same way as Novu
        // Return email as subscriber ID
        var subscriberId = request.Email ?? request.SubscriberId ?? Guid.NewGuid().ToString();
        return Task.FromResult(subscriberId);
    }

    /// <inheritdoc />
    public Task DeleteSubscriberAsync(string subscriberId, CancellationToken cancellationToken = default)
    {
        // No-op for SendGrid
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<SubscriberPreferences?> GetSubscriberPreferencesAsync(
        string subscriberId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<SubscriberPreferences?>(null);
    }

    /// <inheritdoc />
    public Task UpdateSubscriberPreferencesAsync(
        string subscriberId,
        SubscriberPreferences preferences,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    #endregion

    #region Delivery Status

    /// <inheritdoc />
    public Task<DeliveryStatus?> GetDeliveryStatusAsync(
        string notificationId,
        CancellationToken cancellationToken = default)
    {
        // SendGrid doesn't provide a direct API to fetch message status
        // Status is typically received via webhooks
        _logger.LogDebug("SendGrid does not support fetching delivery status. Use webhooks instead.");
        return Task.FromResult<DeliveryStatus?>(null);
    }

    /// <inheritdoc />
    public Task<DeliveryEvent> ProcessDeliveryWebhookAsync(
        string eventType,
        string payload,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Parse SendGrid Event Webhook payload (JSON array)
            var events = JsonSerializer.Deserialize<List<SendGridWebhookEvent>>(payload);
            var evt = events?.FirstOrDefault();

            if (evt == null)
            {
                return Task.FromResult(new DeliveryEvent
                {
                    EventType = "unknown",
                    Channel = "email",
                    Timestamp = DateTime.UtcNow,
                    Data = new Dictionary<string, object>()
                });
            }

            return Task.FromResult(new DeliveryEvent
            {
                NotificationId = evt.SgMessageId ?? evt.SgEventId ?? "",
                EventType = evt.Event ?? eventType,
                Channel = "email",
                Timestamp = DateTimeOffset.FromUnixTimeSeconds(evt.Timestamp).UtcDateTime,
                SubscriberId = evt.Email ?? "",
                Data = new Dictionary<string, object>
                {
                    ["sendgridEvent"] = evt.Event ?? "",
                    ["category"] = evt.Category ?? new List<string>(),
                    ["sgEventId"] = evt.SgEventId ?? "",
                    ["ip"] = evt.Ip ?? "",
                    ["userAgent"] = evt.Useragent ?? "",
                    ["url"] = evt.Url ?? "",
                    ["status"] = MapSendGridEventToStatus(evt.Event)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing SendGrid webhook");
            return Task.FromResult(new DeliveryEvent
            {
                EventType = "error",
                Channel = "email",
                Timestamp = DateTime.UtcNow,
                Data = new Dictionary<string, object>
                {
                    ["error"] = ex.Message
                }
            });
        }
    }

    private static string MapSendGridEventToStatus(string? sendGridEvent)
    {
        return sendGridEvent?.ToLowerInvariant() switch
        {
            "processed" => "pending",
            "dropped" => "failed",
            "delivered" => "delivered",
            "deferred" => "pending",
            "bounce" => "bounced",
            "open" => "opened",
            "click" => "clicked",
            "spamreport" => "spam",
            "unsubscribe" => "unsubscribed",
            "group_unsubscribe" => "unsubscribed",
            "group_resubscribe" => "delivered",
            _ => "unknown"
        };
    }

    #endregion

    #region Health Check

    /// <inheritdoc />
    public async Task<ProviderHealthResult> HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        var result = new ProviderHealthResult
        {
            ProviderName = ProviderName,
            CheckedAt = DateTime.UtcNow
        };

        if (!_config.IsValid())
        {
            result.IsHealthy = false;
            result.Message = "SendGrid configuration is invalid";
            return result;
        }

        try
        {
            // Verify API key by making a simple API call
            var response = await _client.RequestAsync(
                method: global::SendGrid.BaseClient.Method.GET,
                urlPath: "user/profile",
                cancellationToken: cancellationToken);

            result.IsHealthy = response.IsSuccessStatusCode;
            result.Message = result.IsHealthy
                ? "SendGrid API is accessible"
                : $"SendGrid API error: {response.StatusCode}";
        }
        catch (Exception ex)
        {
            result.IsHealthy = false;
            result.Message = $"SendGrid health check failed: {ex.Message}";
            _logger.LogError(ex, "SendGrid health check failed");
        }

        return result;
    }

    #endregion

    #region Webhook Event Model

    private class SendGridWebhookEvent
    {
        public string? Email { get; set; }
        public long Timestamp { get; set; }
        public string? Event { get; set; }
        public string? SgEventId { get; set; }
        public string? SgMessageId { get; set; }
        public List<string>? Category { get; set; }
        public string? Ip { get; set; }
        public string? Useragent { get; set; }
        public string? Url { get; set; }
        public string? Reason { get; set; }
        public string? Status { get; set; }
        public string? Type { get; set; }
    }

    #endregion
}
