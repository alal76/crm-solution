// CRM Solution - Pluggable Architecture
// Twilio Provider Implementation
// Week 10: SMS/Voice notification provider using Twilio API

using CRM.Core.Ports.Output.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TwilioClient = Twilio.TwilioClient;
using Twilio.Rest.Api.V2010.Account;
using TwilioPhoneNumber = Twilio.Types.PhoneNumber;
using Twilio.Exceptions;

namespace CRM.Infrastructure.Providers.Twilio;

/// <summary>
/// Twilio implementation of INotificationPort for SMS notifications.
/// Supports SMS, MMS, and WhatsApp messaging.
/// </summary>
public class TwilioProvider : INotificationPort
{
    private readonly TwilioConfiguration _config;
    private readonly ILogger<TwilioProvider> _logger;
    private bool _isInitialized;

    /// <summary>
    /// Initializes a new instance of TwilioProvider.
    /// </summary>
    public TwilioProvider(
        IOptions<TwilioConfiguration> options,
        ILogger<TwilioProvider> logger)
    {
        _config = options.Value;
        _logger = logger;
        InitializeTwilioClient();
    }

    /// <inheritdoc />
    public string ProviderName => "Twilio";

    /// <inheritdoc />
    public IEnumerable<string> SupportedChannels => new[] { "sms", "whatsapp", "voice" };

    private void InitializeTwilioClient()
    {
        if (!_config.IsValid())
        {
            _logger.LogWarning("Twilio configuration is invalid. Provider will not be available.");
            return;
        }

        try
        {
            TwilioClient.Init(_config.AccountSid, _config.AuthToken);
            _isInitialized = true;
            _logger.LogInformation("Twilio client initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Twilio client");
            _isInitialized = false;
        }
    }

    /// <inheritdoc />
    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_isInitialized && _config.IsValid());
    }

    #region SMS Operations

    /// <inheritdoc />
    public async Task<NotificationResult> SendSmsAsync(
        SmsNotificationRequest request, 
        CancellationToken cancellationToken = default)
    {
        if (!_isInitialized)
        {
            return new NotificationResult
            {
                Success = false,
                Error = "Twilio client not initialized",
                Provider = ProviderName
            };
        }

        try
        {
            if (_config.TestMode)
            {
                _logger.LogInformation("TEST MODE: Would send SMS to {To}: {Message}", 
                    request.To, TruncateForLog(request.Message));
                return new NotificationResult
                {
                    Success = true,
                    MessageId = $"test_{Guid.NewGuid():N}",
                    Provider = ProviderName,
                    Channel = "sms"
                };
            }

            var messageOptions = new CreateMessageOptions(new TwilioPhoneNumber(request.To))
            {
                Body = request.Message
            };

            // Use messaging service or from number
            if (!string.IsNullOrEmpty(_config.MessagingServiceSid))
            {
                messageOptions.MessagingServiceSid = _config.MessagingServiceSid;
            }
            else
            {
                messageOptions.From = new TwilioPhoneNumber(_config.FromPhoneNumber);
            }

            // Add status callback if configured
            if (_config.TrackDeliveryStatus && !string.IsNullOrEmpty(_config.StatusCallbackUrl))
            {
                messageOptions.StatusCallback = new Uri(_config.StatusCallbackUrl);
            }

            var message = await MessageResource.CreateAsync(messageOptions);

            _logger.LogInformation("SMS sent successfully. SID: {Sid}, Status: {Status}", 
                message.Sid, message.Status);

            return new NotificationResult
            {
                Success = true,
                MessageId = message.Sid,
                Provider = ProviderName,
                Channel = "sms"
            };
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "Twilio API error sending SMS to {To}. Code: {Code}", 
                request.To, ex.Code);
            return new NotificationResult
            {
                Success = false,
                Error = $"Twilio API error: {ex.Message} (Code: {ex.Code})",
                Provider = ProviderName,
                Channel = "sms"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS to {To}", request.To);
            return new NotificationResult
            {
                Success = false,
                Error = ex.Message,
                Provider = ProviderName,
                Channel = "sms"
            };
        }
    }

    /// <inheritdoc />
    public async Task<BulkNotificationResult> SendBulkSmsAsync(
        IEnumerable<SmsNotificationRequest> requests, 
        CancellationToken cancellationToken = default)
    {
        var requestList = requests.ToList();
        var results = new List<NotificationResult>();
        var successCount = 0;
        var failureCount = 0;

        foreach (var request in requestList)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var result = await SendSmsAsync(request, cancellationToken);
            results.Add(result);

            if (result.Success)
            {
                successCount++;
            }
            else
            {
                failureCount++;
            }

            // Respect rate limiting
            if (_config.RateLimitPerSecond > 0 && !_config.TestMode)
            {
                await Task.Delay(1000 / _config.RateLimitPerSecond, cancellationToken);
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

    #endregion

    #region Email Operations (Not Supported - Use SendGrid)

    /// <inheritdoc />
    public Task<NotificationResult> SendEmailAsync(
        EmailNotificationRequest request, 
        CancellationToken cancellationToken = default)
    {
        // Twilio doesn't support email directly - use SendGrid for email
        _logger.LogWarning("Email not supported by Twilio provider. Use SendGrid for email.");
        return Task.FromResult(new NotificationResult
        {
            Success = false,
            Error = "Email not supported by Twilio. Use SendGrid provider for email.",
            Provider = ProviderName,
            Channel = "email"
        });
    }

    /// <inheritdoc />
    public Task<NotificationResult> SendTemplateEmailAsync(
        string templateId, 
        string recipientEmail, 
        object data, 
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new NotificationResult
        {
            Success = false,
            Error = "Email not supported by Twilio. Use SendGrid provider for email.",
            Provider = ProviderName
        });
    }

    /// <inheritdoc />
    public Task<BulkNotificationResult> SendBulkEmailAsync(
        IEnumerable<EmailNotificationRequest> requests, 
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
                Error = "Email not supported by Twilio",
                Provider = ProviderName
            }).ToList()
        });
    }

    #endregion

    #region Push Notifications (Not Supported)

    /// <inheritdoc />
    public Task<NotificationResult> SendPushAsync(
        PushNotificationRequest request, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Push notifications not supported by Twilio provider.");
        return Task.FromResult(new NotificationResult
        {
            Success = false,
            Error = "Push notifications not supported by Twilio. Use OneSignal or Firebase.",
            Provider = ProviderName,
            Channel = "push"
        });
    }

    #endregion

    #region In-App Notifications (Not Supported)

    /// <inheritdoc />
    public Task<NotificationResult> SendInAppAsync(
        InAppNotificationRequest request, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("In-app notifications not supported by Twilio provider.");
        return Task.FromResult(new NotificationResult
        {
            Success = false,
            Error = "In-app notifications not supported by Twilio. Use Novu or custom implementation.",
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

        // Extract phone and message from Content dictionary
        var phone = request.Content.TryGetValue("phone", out var phoneObj) ? phoneObj?.ToString() : null;
        var body = request.Content.TryGetValue("body", out var bodyObj) ? bodyObj?.ToString() : null;
        var message = request.Content.TryGetValue("message", out var msgObj) ? msgObj?.ToString() : body;

        foreach (var channel in request.Channels ?? new List<string> { "sms" })
        {
            NotificationResult? result = null;

            switch (channel.ToLowerInvariant())
            {
                case "sms":
                    if (!string.IsNullOrEmpty(phone))
                    {
                        result = await SendSmsAsync(new SmsNotificationRequest
                        {
                            To = phone,
                            Message = message ?? string.Empty
                        }, cancellationToken);
                    }
                    break;

                case "whatsapp":
                    if (!string.IsNullOrEmpty(phone) && !string.IsNullOrEmpty(_config.WhatsAppFromNumber))
                    {
                        result = await SendWhatsAppAsync(phone, 
                            message ?? string.Empty, 
                            cancellationToken);
                    }
                    break;

                default:
                    result = new NotificationResult
                    {
                        Success = false,
                        Error = $"Channel '{channel}' not supported by Twilio",
                        Provider = ProviderName,
                        Channel = channel
                    };
                    break;
            }

            if (result != null)
            {
                channelResults[channel] = result;
                if (result.Success)
                {
                    overallSuccess = true;
                }
            }
        }

        return new MultiChannelNotificationResult
        {
            Success = overallSuccess,
            ChannelResults = channelResults
        };
    }

    /// <summary>
    /// Sends a WhatsApp message.
    /// </summary>
    private async Task<NotificationResult> SendWhatsAppAsync(
        string to, 
        string message, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_config.WhatsAppFromNumber))
        {
            return new NotificationResult
            {
                Success = false,
                Error = "WhatsApp not configured",
                Provider = ProviderName,
                Channel = "whatsapp"
            };
        }

        try
        {
            var toWhatsApp = to.StartsWith("whatsapp:") ? to : $"whatsapp:{to}";

            var messageOptions = new CreateMessageOptions(new TwilioPhoneNumber(toWhatsApp))
            {
                From = new TwilioPhoneNumber(_config.WhatsAppFromNumber),
                Body = message
            };

            var msg = await MessageResource.CreateAsync(messageOptions);

            return new NotificationResult
            {
                Success = true,
                MessageId = msg.Sid,
                Provider = ProviderName,
                Channel = "whatsapp"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending WhatsApp message to {To}", to);
            return new NotificationResult
            {
                Success = false,
                Error = ex.Message,
                Provider = ProviderName,
                Channel = "whatsapp"
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
        // Twilio doesn't have workflow concept like Novu
        _logger.LogWarning("Workflow triggers not supported by Twilio. Use Novu for workflows.");
        return Task.FromResult(new NotificationResult
        {
            Success = false,
            Error = "Workflow triggers not supported by Twilio. Use Novu provider.",
            Provider = ProviderName
        });
    }

    #endregion

    #region Subscriber Management (Minimal Support)

    /// <inheritdoc />
    public Task<string> UpsertSubscriberAsync(
        SubscriberRequest request, 
        CancellationToken cancellationToken = default)
    {
        // Twilio doesn't have subscriber management - return the phone as ID
        var subscriberId = request.Phone ?? request.SubscriberId ?? Guid.NewGuid().ToString();
        _logger.LogDebug("Twilio doesn't manage subscribers. Using phone as ID: {Id}", subscriberId);
        return Task.FromResult(subscriberId);
    }

    /// <inheritdoc />
    public Task DeleteSubscriberAsync(string subscriberId, CancellationToken cancellationToken = default)
    {
        // No-op for Twilio
        _logger.LogDebug("Twilio doesn't manage subscribers. Delete is no-op.");
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<SubscriberPreferences?> GetSubscriberPreferencesAsync(
        string subscriberId, 
        CancellationToken cancellationToken = default)
    {
        // Twilio doesn't manage preferences
        return Task.FromResult<SubscriberPreferences?>(null);
    }

    /// <inheritdoc />
    public Task UpdateSubscriberPreferencesAsync(
        string subscriberId, 
        SubscriberPreferences preferences, 
        CancellationToken cancellationToken = default)
    {
        // No-op for Twilio
        return Task.CompletedTask;
    }

    #endregion

    #region Delivery Status

    /// <inheritdoc />
    public async Task<DeliveryStatus?> GetDeliveryStatusAsync(
        string notificationId, 
        CancellationToken cancellationToken = default)
    {
        if (!_isInitialized)
        {
            return null;
        }

        try
        {
            var message = await MessageResource.FetchAsync(notificationId);

            return new DeliveryStatus
            {
                NotificationId = message.Sid,
                Status = MapTwilioStatus(message.Status.ToString()),
                Channel = "sms",
                FailureReason = message.ErrorMessage
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching delivery status for {MessageId}", notificationId);
            return null;
        }
    }

    /// <inheritdoc />
    public Task<DeliveryEvent> ProcessDeliveryWebhookAsync(
        string eventType, 
        string payload, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Parse Twilio webhook payload (form-urlencoded)
            var formData = ParseFormUrlEncoded(payload);

            var messageSid = formData.GetValueOrDefault("MessageSid", "");
            var messageStatus = formData.GetValueOrDefault("MessageStatus", "");
            var errorCode = formData.GetValueOrDefault("ErrorCode", "");
            var errorMessage = formData.GetValueOrDefault("ErrorMessage", "");
            var to = formData.GetValueOrDefault("To", "");
            var from = formData.GetValueOrDefault("From", "");

            return Task.FromResult(new DeliveryEvent
            {
                NotificationId = messageSid,
                EventType = MapTwilioEventType(messageStatus),
                Channel = "sms",
                Timestamp = DateTime.UtcNow,
                SubscriberId = to,
                Data = new Dictionary<string, object>
                {
                    ["twilioStatus"] = messageStatus,
                    ["errorCode"] = errorCode,
                    ["errorMessage"] = errorMessage,
                    ["from"] = from,
                    ["provider"] = ProviderName
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Twilio webhook: {EventType}", eventType);
            return Task.FromResult(new DeliveryEvent
            {
                EventType = "error",
                Channel = "sms",
                Timestamp = DateTime.UtcNow,
                Data = new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["rawPayload"] = payload,
                    ["provider"] = ProviderName
                }
            });
        }
    }

    private static Dictionary<string, string> ParseFormUrlEncoded(string data)
    {
        var result = new Dictionary<string, string>();
        if (string.IsNullOrEmpty(data)) return result;

        var pairs = data.Split('&');
        foreach (var pair in pairs)
        {
            var parts = pair.Split('=', 2);
            if (parts.Length == 2)
            {
                var key = Uri.UnescapeDataString(parts[0]);
                var value = Uri.UnescapeDataString(parts[1]);
                result[key] = value;
            }
        }
        return result;
    }

    private static string MapTwilioStatus(string twilioStatus)
    {
        return twilioStatus.ToLowerInvariant() switch
        {
            "queued" => "pending",
            "sending" => "pending",
            "sent" => "sent",
            "delivered" => "delivered",
            "undelivered" => "failed",
            "failed" => "failed",
            "receiving" => "pending",
            "received" => "delivered",
            "accepted" => "pending",
            "scheduled" => "pending",
            "read" => "read",
            "canceled" => "cancelled",
            _ => "unknown"
        };
    }

    private static string MapTwilioEventType(string twilioStatus)
    {
        return twilioStatus.ToLowerInvariant() switch
        {
            "queued" => "queued",
            "sending" => "sending",
            "sent" => "sent",
            "delivered" => "delivered",
            "undelivered" => "failed",
            "failed" => "failed",
            "read" => "read",
            "canceled" => "cancelled",
            _ => "status_update"
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
            result.Message = "Twilio configuration is invalid";
            return result;
        }

        if (!_isInitialized)
        {
            result.IsHealthy = false;
            result.Message = "Twilio client not initialized";
            return result;
        }

        try
        {
            // Verify credentials by fetching account info
            var account = await global::Twilio.Rest.Api.V2010.AccountResource.FetchAsync(_config.AccountSid);
            
            result.IsHealthy = account.Status == global::Twilio.Rest.Api.V2010.AccountResource.StatusEnum.Active;
            result.Message = result.IsHealthy 
                ? $"Twilio account active: {account.FriendlyName}" 
                : $"Twilio account status: {account.Status}";
            result.ResponseTimeMs = 0; // Not measured in this simple check
            result.Details = new Dictionary<string, object>
            {
                ["accountSid"] = account.Sid,
                ["accountName"] = account.FriendlyName ?? "",
                ["accountStatus"] = account.Status?.ToString() ?? ""
            };
        }
        catch (Exception ex)
        {
            result.IsHealthy = false;
            result.Message = $"Twilio health check failed: {ex.Message}";
            _logger.LogError(ex, "Twilio health check failed");
        }

        return result;
    }

    #endregion

    #region Helper Methods

    private static string TruncateForLog(string? text, int maxLength = 50)
    {
        if (string.IsNullOrEmpty(text)) return "";
        return text.Length <= maxLength ? text : text[..maxLength] + "...";
    }

    #endregion
}