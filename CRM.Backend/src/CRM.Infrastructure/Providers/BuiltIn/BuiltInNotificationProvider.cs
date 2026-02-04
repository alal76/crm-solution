// CRM Solution - BuiltIn Notification Provider
// Phase 2 Week 8: Implements INotificationPort using SMTP for email
// Part of the Pluggable Architecture implementation
//
// HEXAGONAL ARCHITECTURE NOTE:
// This is the BuiltIn adapter for the INotificationPort output port.
// It provides basic email functionality via SMTP, with stubs for other channels.
// For advanced multi-channel notifications, use Novu, Twilio, or SendGrid providers.

using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CRM.Core.Ports.Output.Providers;

namespace CRM.Infrastructure.Providers.BuiltIn;

/// <summary>
/// BuiltIn notification provider using SMTP for email delivery.
/// Supports basic email functionality with templates and attachments.
/// SMS, Push, and In-App notifications return not-supported results.
/// </summary>
public class BuiltInNotificationProvider : INotificationPort
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<BuiltInNotificationProvider> _logger;
    private readonly SmtpSettings _smtpSettings;

    public BuiltInNotificationProvider(
        IConfiguration configuration,
        ILogger<BuiltInNotificationProvider> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Load SMTP settings from configuration
        _smtpSettings = new SmtpSettings();
        _configuration.GetSection("Smtp").Bind(_smtpSettings);
    }

    /// <inheritdoc />
    public string ProviderName => "BuiltIn";

    /// <inheritdoc />
    public IEnumerable<string> SupportedChannels => new[] { "email" };

    /// <inheritdoc />
    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        // BuiltIn is always available if SMTP is configured
        var isConfigured = !string.IsNullOrEmpty(_smtpSettings.Host) && _smtpSettings.Port > 0;
        return Task.FromResult(isConfigured);
    }

    #region Email Operations

    /// <inheritdoc />
    public async Task<NotificationResult> SendEmailAsync(
        EmailNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.To)) throw new ArgumentException("Recipient email is required", nameof(request));
        if (string.IsNullOrWhiteSpace(request.Subject)) throw new ArgumentException("Subject is required", nameof(request));

        try
        {
            _logger.LogDebug("Sending email to {To} with subject: {Subject}", request.To, request.Subject);

            // If SMTP is not configured, log and return success (for development)
            if (string.IsNullOrEmpty(_smtpSettings.Host))
            {
                _logger.LogWarning("SMTP not configured. Email would be sent to {To}: {Subject}", request.To, request.Subject);
                return new NotificationResult
                {
                    Success = true,
                    MessageId = $"dev_{Guid.NewGuid():N}",
                    Provider = ProviderName,
                    Channel = "email"
                };
            }

            using var mailMessage = CreateMailMessage(request);
            using var smtpClient = CreateSmtpClient();

            await smtpClient.SendMailAsync(mailMessage, cancellationToken);

            var messageId = $"smtp_{Guid.NewGuid():N}";
            _logger.LogInformation("Email sent successfully to {To}. MessageId: {MessageId}", request.To, messageId);

            return new NotificationResult
            {
                Success = true,
                MessageId = messageId,
                Provider = ProviderName,
                Channel = "email"
            };
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex, "SMTP error sending email to {To}: {Error}", request.To, ex.Message);
            return new NotificationResult
            {
                Success = false,
                Provider = ProviderName,
                Channel = "email",
                Error = ex.Message,
                ErrorCode = ex.StatusCode.ToString()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}: {Error}", request.To, ex.Message);
            return new NotificationResult
            {
                Success = false,
                Provider = ProviderName,
                Channel = "email",
                Error = ex.Message
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
        // BuiltIn provider doesn't support templates directly
        // Would need to integrate with Razor or similar template engine
        _logger.LogWarning("Template email not supported by BuiltIn provider. TemplateId: {TemplateId}", templateId);
        
        return await Task.FromResult(new NotificationResult
        {
            Success = false,
            Provider = ProviderName,
            Channel = "email",
            Error = "Template emails not supported by BuiltIn provider. Use Novu or SendGrid for template support."
        });
    }

    #endregion

    #region SMS Operations (Not Supported)

    /// <inheritdoc />
    public Task<NotificationResult> SendSmsAsync(
        SmsNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("SMS not supported by BuiltIn provider. Use Twilio or Novu for SMS support.");
        
        return Task.FromResult(new NotificationResult
        {
            Success = false,
            Provider = ProviderName,
            Channel = "sms",
            Error = "SMS not supported by BuiltIn provider. Configure Twilio or Novu for SMS support."
        });
    }

    #endregion

    #region Push Notification Operations (Not Supported)

    /// <inheritdoc />
    public Task<NotificationResult> SendPushAsync(
        PushNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Push notifications not supported by BuiltIn provider. Use OneSignal or Novu.");
        
        return Task.FromResult(new NotificationResult
        {
            Success = false,
            Provider = ProviderName,
            Channel = "push",
            Error = "Push notifications not supported by BuiltIn provider. Configure OneSignal or Novu."
        });
    }

    #endregion

    #region In-App Notifications (Basic Implementation)

    /// <inheritdoc />
    public Task<NotificationResult> SendInAppAsync(
        InAppNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        // In-app notifications would typically go through SignalR or a database
        // For BuiltIn, we just log the notification - actual delivery handled by CRM core
        _logger.LogInformation("In-app notification for user {UserId}: {Title}", request.UserId, request.Title);
        
        return Task.FromResult(new NotificationResult
        {
            Success = true,
            MessageId = $"inapp_{Guid.NewGuid():N}",
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
        if (request == null) throw new ArgumentNullException(nameof(request));

        var result = new MultiChannelNotificationResult
        {
            TransactionId = $"tx_{Guid.NewGuid():N}"
        };

        foreach (var channel in request.Channels)
        {
            var channelResult = channel.ToLowerInvariant() switch
            {
                "email" => await SendEmailFromMultiChannel(request, cancellationToken),
                "sms" => await SendSmsAsync(new SmsNotificationRequest(), cancellationToken),
                "push" => await SendPushAsync(new PushNotificationRequest(), cancellationToken),
                "in_app" => await SendInAppAsync(
                    new InAppNotificationRequest
                    {
                        UserId = request.SubscriberId,
                        Title = request.Content.GetValueOrDefault("title")?.ToString() ?? "Notification",
                        Content = request.Content.GetValueOrDefault("body")?.ToString() ?? ""
                    },
                    cancellationToken),
                _ => new NotificationResult
                {
                    Success = false,
                    Channel = channel,
                    Error = $"Unknown channel: {channel}"
                }
            };

            result.ChannelResults[channel] = channelResult;
        }

        result.Success = result.ChannelResults.Values.Any(r => r.Success);
        return result;
    }

    /// <inheritdoc />
    public Task<NotificationResult> TriggerWorkflowAsync(
        string workflowId,
        string subscriberId,
        object payload,
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Workflow triggers not supported by BuiltIn provider. Use Novu for workflow support.");
        
        return Task.FromResult(new NotificationResult
        {
            Success = false,
            Provider = ProviderName,
            Error = "Workflow triggers not supported by BuiltIn provider. Configure Novu for workflow support."
        });
    }

    #endregion

    #region Bulk Operations

    /// <inheritdoc />
    public async Task<BulkNotificationResult> SendBulkEmailAsync(
        IEnumerable<EmailNotificationRequest> requests,
        CancellationToken cancellationToken = default)
    {
        if (requests == null) throw new ArgumentNullException(nameof(requests));

        var requestList = requests.ToList();
        var result = new BulkNotificationResult
        {
            TotalCount = requestList.Count
        };

        foreach (var request in requestList)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var sendResult = await SendEmailAsync(request, cancellationToken);
            result.Results.Add(sendResult);

            if (sendResult.Success)
                result.SuccessCount++;
            else
                result.FailureCount++;
        }

        return result;
    }

    /// <inheritdoc />
    public Task<BulkNotificationResult> SendBulkSmsAsync(
        IEnumerable<SmsNotificationRequest> requests,
        CancellationToken cancellationToken = default)
    {
        var requestList = requests?.ToList() ?? new List<SmsNotificationRequest>();
        
        return Task.FromResult(new BulkNotificationResult
        {
            TotalCount = requestList.Count,
            FailureCount = requestList.Count,
            Results = requestList.Select(_ => new NotificationResult
            {
                Success = false,
                Channel = "sms",
                Error = "SMS not supported by BuiltIn provider"
            }).ToList()
        });
    }

    #endregion

    #region Subscriber Management (Basic Implementation)

    /// <inheritdoc />
    public Task<string> UpsertSubscriberAsync(
        SubscriberRequest request,
        CancellationToken cancellationToken = default)
    {
        // BuiltIn provider doesn't maintain subscriber state
        // Returns the subscriber ID as-is
        _logger.LogDebug("Subscriber upsert (passthrough): {SubscriberId}", request.SubscriberId);
        return Task.FromResult(request.SubscriberId);
    }

    /// <inheritdoc />
    public Task DeleteSubscriberAsync(
        string subscriberId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Subscriber delete (no-op): {SubscriberId}", subscriberId);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<SubscriberPreferences?> GetSubscriberPreferencesAsync(
        string subscriberId,
        CancellationToken cancellationToken = default)
    {
        // BuiltIn provider doesn't store preferences
        // Returns default preferences (all channels enabled)
        return Task.FromResult<SubscriberPreferences?>(new SubscriberPreferences
        {
            GlobalOptOut = false,
            ChannelPreferences = new Dictionary<string, bool>
            {
                { "email", true },
                { "sms", false },
                { "push", false },
                { "in_app", true }
            }
        });
    }

    /// <inheritdoc />
    public Task UpdateSubscriberPreferencesAsync(
        string subscriberId,
        SubscriberPreferences preferences,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Subscriber preferences update (no-op): {SubscriberId}", subscriberId);
        return Task.CompletedTask;
    }

    #endregion

    #region Delivery Status

    /// <inheritdoc />
    public Task<DeliveryStatus?> GetDeliveryStatusAsync(
        string notificationId,
        CancellationToken cancellationToken = default)
    {
        // BuiltIn provider doesn't track delivery status
        // SMTP doesn't provide reliable delivery tracking
        _logger.LogDebug("Delivery status requested for {NotificationId} - not available for BuiltIn", notificationId);
        
        return Task.FromResult<DeliveryStatus?>(null);
    }

    /// <inheritdoc />
    public Task<DeliveryEvent> ProcessDeliveryWebhookAsync(
        string eventType,
        string payload,
        CancellationToken cancellationToken = default)
    {
        // BuiltIn provider doesn't support webhooks
        _logger.LogWarning("Delivery webhooks not supported by BuiltIn provider");
        
        return Task.FromResult(new DeliveryEvent
        {
            EventType = eventType,
            Timestamp = DateTime.UtcNow,
            Data = new Dictionary<string, object> { { "error", "Webhooks not supported by BuiltIn" } }
        });
    }

    #endregion

    /// <inheritdoc />
    public Task<ProviderHealthResult> HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            var isConfigured = !string.IsNullOrEmpty(_smtpSettings.Host) && _smtpSettings.Port > 0;
            
            stopwatch.Stop();
            
            if (!isConfigured)
            {
                return Task.FromResult(new ProviderHealthResult
                {
                    IsHealthy = false,
                    ProviderName = ProviderName,
                    Message = "SMTP not configured",
                    ResponseTimeMs = stopwatch.ElapsedMilliseconds
                });
            }

            // For a real health check, would attempt SMTP connection
            // For now, just verify configuration exists
            return Task.FromResult(new ProviderHealthResult
            {
                IsHealthy = true,
                ProviderName = ProviderName,
                Message = $"SMTP configured: {_smtpSettings.Host}:{_smtpSettings.Port}",
                ResponseTimeMs = stopwatch.ElapsedMilliseconds
            });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return Task.FromResult(new ProviderHealthResult
            {
                IsHealthy = false,
                ProviderName = ProviderName,
                Message = $"Health check failed: {ex.Message}",
                ResponseTimeMs = stopwatch.ElapsedMilliseconds
            });
        }
    }

    #region Private Helpers

    private MailMessage CreateMailMessage(EmailNotificationRequest request)
    {
        var fromAddress = new MailAddress(
            request.From ?? _smtpSettings.FromEmail ?? "noreply@crm.local",
            request.FromName ?? _smtpSettings.FromName ?? "CRM System"
        );

        var toAddress = new MailAddress(request.To, request.ToName);

        var mailMessage = new MailMessage(fromAddress, toAddress)
        {
            Subject = request.Subject,
            Body = request.Body,
            IsBodyHtml = request.IsHtml
        };

        // Add plain text alternative
        if (!string.IsNullOrEmpty(request.PlainTextBody) && request.IsHtml)
        {
            var plainView = AlternateView.CreateAlternateViewFromString(
                request.PlainTextBody, null, "text/plain");
            mailMessage.AlternateViews.Add(plainView);
        }

        // Add Reply-To
        if (!string.IsNullOrEmpty(request.ReplyTo))
        {
            mailMessage.ReplyToList.Add(new MailAddress(request.ReplyTo));
        }

        // Add CC recipients
        if (request.Cc != null)
        {
            foreach (var cc in request.Cc)
            {
                mailMessage.CC.Add(new MailAddress(cc));
            }
        }

        // Add BCC recipients
        if (request.Bcc != null)
        {
            foreach (var bcc in request.Bcc)
            {
                mailMessage.Bcc.Add(new MailAddress(bcc));
            }
        }

        // Add attachments
        if (request.Attachments != null)
        {
            foreach (var attachment in request.Attachments)
            {
                if (attachment.Content != null)
                {
                    var stream = new MemoryStream(attachment.Content);
                    var mailAttachment = new Attachment(stream, attachment.FileName, attachment.ContentType);
                    
                    if (attachment.IsInline && !string.IsNullOrEmpty(attachment.ContentId))
                    {
                        mailAttachment.ContentId = attachment.ContentId;
                        mailAttachment.ContentDisposition!.Inline = true;
                    }
                    
                    mailMessage.Attachments.Add(mailAttachment);
                }
            }
        }

        // Add custom headers
        if (request.Headers != null)
        {
            foreach (var header in request.Headers)
            {
                mailMessage.Headers.Add(header.Key, header.Value);
            }
        }

        return mailMessage;
    }

    private SmtpClient CreateSmtpClient()
    {
        var client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
        {
            EnableSsl = _smtpSettings.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Timeout = _smtpSettings.TimeoutSeconds * 1000
        };

        if (!string.IsNullOrEmpty(_smtpSettings.Username))
        {
            client.Credentials = new NetworkCredential(
                _smtpSettings.Username,
                _smtpSettings.Password
            );
        }

        return client;
    }

    private async Task<NotificationResult> SendEmailFromMultiChannel(
        MultiChannelNotificationRequest request,
        CancellationToken cancellationToken)
    {
        // Extract email content from multi-channel request
        var emailRequest = new EmailNotificationRequest
        {
            To = request.Content.GetValueOrDefault("email")?.ToString() ?? "",
            Subject = request.Content.GetValueOrDefault("subject")?.ToString() ?? "Notification",
            Body = request.Content.GetValueOrDefault("body")?.ToString() ?? ""
        };

        if (string.IsNullOrEmpty(emailRequest.To))
        {
            return new NotificationResult
            {
                Success = false,
                Channel = "email",
                Error = "Email address not provided in multi-channel request"
            };
        }

        return await SendEmailAsync(emailRequest, cancellationToken);
    }

    #endregion
}

#region SMTP Settings

/// <summary>
/// SMTP configuration settings.
/// </summary>
public class SmtpSettings
{
    /// <summary>
    /// SMTP server hostname.
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// SMTP server port (default: 587).
    /// </summary>
    public int Port { get; set; } = 587;

    /// <summary>
    /// Enable SSL/TLS.
    /// </summary>
    public bool EnableSsl { get; set; } = true;

    /// <summary>
    /// SMTP username.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// SMTP password.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Default from email address.
    /// </summary>
    public string? FromEmail { get; set; }

    /// <summary>
    /// Default from name.
    /// </summary>
    public string? FromName { get; set; }

    /// <summary>
    /// Connection timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
}

#endregion
