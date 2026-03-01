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

namespace CRM.Infrastructure.Providers.Novu;

/// <summary>
/// Novu notification provider implementing INotificationPort.
/// Provides multi-channel notification delivery via the Novu platform.
/// Supports email, SMS, push notifications, and in-app messaging.
/// Uses HTTP client directly for Novu API v1 compatibility.
/// </summary>
public class NovuProvider : INotificationPort
{
    private readonly NovuConfiguration _config;
    private readonly ILogger<NovuProvider> _logger;
    private readonly HttpClient _httpClient;
    private readonly bool _isConfigured;
    private readonly JsonSerializerOptions _jsonOptions;

    public NovuProvider(
        IOptions<NovuConfiguration> config,
        HttpClient httpClient,
        ILogger<NovuProvider> logger)
    {
        _config = config.Value;
        _httpClient = httpClient;
        _logger = logger;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        _isConfigured = _config.IsValid();

        if (_isConfigured)
        {
            _logger.LogInformation("Novu provider initialized with URL: {Url}", _config.Url);
        }
        else
        {
            _logger.LogWarning("Novu provider not configured - API key or URL missing");
        }
    }

    /// <inheritdoc />
    public string ProviderName => "Novu";

    /// <inheritdoc />
    public IEnumerable<string> SupportedChannels => new[] { "email", "sms", "push", "in_app", "chat" };

    /// <inheritdoc />
    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        if (!_isConfigured)
        {
            return false;
        }

        try
        {
            var response = await _httpClient.GetAsync("v1/subscribers?page=0&limit=1", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Novu health check failed");
            return false;
        }
    }

    #region Email Operations

    /// <inheritdoc />
    public async Task<NotificationResult> SendEmailAsync(
        EmailNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.To))
        {
            throw new ArgumentException("Recipient email is required", nameof(request));
        }

        if (!_isConfigured)
        {
            return CreateFailedResult("Novu provider not configured", "email");
        }

        try
        {
            // Ensure subscriber exists
            var subscriberId = await EnsureSubscriberAsync(
                request.To.Replace("@", "_at_").Replace(".", "_"),
                request.ToName ?? request.To,
                request.To,
                null,
                cancellationToken);

            // Trigger email workflow
            var triggerPayload = new NovuTriggerRequest
            {
                Name = _config.EmailWorkflowId,
                To = new NovuSubscriberTo { SubscriberId = subscriberId },
                Payload = new Dictionary<string, object>
                {
                    ["subject"] = request.Subject,
                    ["body"] = request.Body,
                    ["isHtml"] = request.IsHtml
                }
            };

            if (!string.IsNullOrEmpty(request.From))
            {
                triggerPayload.Payload["from"] = request.From;
            }
            if (!string.IsNullOrEmpty(request.ReplyTo))
            {
                triggerPayload.Payload["replyTo"] = request.ReplyTo;
            }

            var response = await _httpClient.PostAsJsonAsync(
                "v1/events/trigger",
                triggerPayload,
                _jsonOptions,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<NovuTriggerResponse>(
                    _jsonOptions, cancellationToken);

                if (result?.Data?.Acknowledged == true)
                {
                    _logger.LogInformation(
                        "Email sent via Novu. TransactionId: {TransactionId}, To: {To}",
                        result.Data.TransactionId, request.To);

                    return CreateSuccessResult(result.Data.TransactionId, "email");
                }
            }

            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Novu email trigger failed: {Error}", error);
            return CreateFailedResult($"Novu trigger failed: {response.StatusCode}", "email");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email via Novu to {To}", request.To);
            return CreateFailedResult(ex.Message, "email");
        }
    }

    /// <inheritdoc />
    public async Task<NotificationResult> SendTemplateEmailAsync(
        string templateId,
        string recipientEmail,
        object data,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(templateId))
        {
            throw new ArgumentException("Template ID is required", nameof(templateId));
        }
        if (string.IsNullOrWhiteSpace(recipientEmail))
        {
            throw new ArgumentException("Recipient email is required", nameof(recipientEmail));
        }

        if (!_isConfigured)
        {
            return CreateFailedResult("Novu provider not configured", "email");
        }

        try
        {
            var subscriberId = await EnsureSubscriberAsync(
                recipientEmail.Replace("@", "_at_").Replace(".", "_"),
                null,
                recipientEmail,
                null,
                cancellationToken);

            var triggerPayload = new NovuTriggerRequest
            {
                Name = templateId,
                To = new NovuSubscriberTo { SubscriberId = subscriberId },
                Payload = data as Dictionary<string, object> ?? new Dictionary<string, object> { ["data"] = data }
            };

            var response = await _httpClient.PostAsJsonAsync(
                "v1/events/trigger",
                triggerPayload,
                _jsonOptions,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<NovuTriggerResponse>(
                    _jsonOptions, cancellationToken);

                if (result?.Data?.Acknowledged == true)
                {
                    return CreateSuccessResult(result.Data.TransactionId, "email");
                }
            }

            return CreateFailedResult("Template email not acknowledged", "email");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send template email via Novu");
            return CreateFailedResult(ex.Message, "email");
        }
    }

    #endregion

    #region SMS Operations

    /// <inheritdoc />
    public async Task<NotificationResult> SendSmsAsync(
        SmsNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.To))
        {
            throw new ArgumentException("Phone number is required", nameof(request));
        }

        if (!_isConfigured)
        {
            return CreateFailedResult("Novu provider not configured", "sms");
        }

        try
        {
            var subscriberId = await EnsureSubscriberAsync(
                $"sms_{request.To.Replace("+", "").Replace("-", "").Replace(" ", "")}",
                null,
                null,
                request.To,
                cancellationToken);

            var triggerPayload = new NovuTriggerRequest
            {
                Name = _config.SmsWorkflowId,
                To = new NovuSubscriberTo { SubscriberId = subscriberId },
                Payload = new Dictionary<string, object>
                {
                    ["message"] = request.Message,
                    ["phone"] = request.To
                }
            };

            var response = await _httpClient.PostAsJsonAsync(
                "v1/events/trigger",
                triggerPayload,
                _jsonOptions,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<NovuTriggerResponse>(
                    _jsonOptions, cancellationToken);

                if (result?.Data?.Acknowledged == true)
                {
                    _logger.LogInformation("SMS triggered via Novu. TransactionId: {TransactionId}",
                        result.Data.TransactionId);
                    return CreateSuccessResult(result.Data.TransactionId, "sms");
                }
            }

            return CreateFailedResult("SMS not acknowledged", "sms");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS via Novu to {To}", request.To);
            return CreateFailedResult(ex.Message, "sms");
        }
    }

    #endregion

    #region Push Notification Operations

    /// <inheritdoc />
    public async Task<NotificationResult> SendPushAsync(
        PushNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.To))
        {
            throw new ArgumentException("Device token or subscriber ID is required", nameof(request));
        }

        if (!_isConfigured)
        {
            return CreateFailedResult("Novu provider not configured", "push");
        }

        try
        {
            var triggerPayload = new NovuTriggerRequest
            {
                Name = _config.PushWorkflowId,
                To = new NovuSubscriberTo { SubscriberId = request.To },
                Payload = new Dictionary<string, object>
                {
                    ["title"] = request.Title,
                    ["body"] = request.Body
                }
            };

            if (!string.IsNullOrEmpty(request.Icon))
            {
                triggerPayload.Payload["icon"] = request.Icon;
            }
            if (!string.IsNullOrEmpty(request.ActionUrl))
            {
                triggerPayload.Payload["actionUrl"] = request.ActionUrl;
            }
            if (request.Data != null)
            {
                triggerPayload.Payload["data"] = request.Data;
            }

            var response = await _httpClient.PostAsJsonAsync(
                "v1/events/trigger",
                triggerPayload,
                _jsonOptions,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<NovuTriggerResponse>(
                    _jsonOptions, cancellationToken);

                if (result?.Data?.Acknowledged == true)
                {
                    _logger.LogInformation("Push notification triggered via Novu. TransactionId: {TransactionId}",
                        result.Data.TransactionId);
                    return CreateSuccessResult(result.Data.TransactionId, "push");
                }
            }

            return CreateFailedResult("Push notification not acknowledged", "push");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send push notification via Novu");
            return CreateFailedResult(ex.Message, "push");
        }
    }

    #endregion

    #region In-App Notification Operations

    /// <inheritdoc />
    public async Task<NotificationResult> SendInAppAsync(
        InAppNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            throw new ArgumentException("User ID is required", nameof(request));
        }

        if (!_isConfigured)
        {
            return CreateFailedResult("Novu provider not configured", "in_app");
        }

        try
        {
            var triggerPayload = new NovuTriggerRequest
            {
                Name = _config.InAppWorkflowId,
                To = new NovuSubscriberTo { SubscriberId = request.UserId },
                Payload = new Dictionary<string, object>
                {
                    ["title"] = request.Title,
                    ["content"] = request.Content
                }
            };

            if (!string.IsNullOrEmpty(request.Type))
            {
                triggerPayload.Payload["type"] = request.Type;
            }
            if (!string.IsNullOrEmpty(request.ActionUrl))
            {
                triggerPayload.Payload["actionUrl"] = request.ActionUrl;
            }
            if (!string.IsNullOrEmpty(request.Avatar))
            {
                triggerPayload.Payload["avatar"] = request.Avatar;
            }
            if (request.Data != null)
            {
                triggerPayload.Payload["data"] = request.Data;
            }

            var response = await _httpClient.PostAsJsonAsync(
                "v1/events/trigger",
                triggerPayload,
                _jsonOptions,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<NovuTriggerResponse>(
                    _jsonOptions, cancellationToken);

                if (result?.Data?.Acknowledged == true)
                {
                    _logger.LogInformation("In-app notification sent via Novu. TransactionId: {TransactionId}",
                        result.Data.TransactionId);
                    return CreateSuccessResult(result.Data.TransactionId, "in_app");
                }
            }

            return CreateFailedResult("In-app notification not acknowledged", "in_app");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send in-app notification via Novu to user {UserId}", request.UserId);
            return CreateFailedResult(ex.Message, "in_app");
        }
    }

    #endregion

    #region Multi-Channel Operations

    /// <inheritdoc />
    public async Task<MultiChannelNotificationResult> SendNotificationAsync(
        MultiChannelNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.SubscriberId))
        {
            throw new ArgumentException("Subscriber ID is required", nameof(request));
        }

        var result = new MultiChannelNotificationResult
        {
            ChannelResults = new Dictionary<string, NotificationResult>()
        };

        if (!_isConfigured)
        {
            result.Success = false;
            foreach (var channel in request.Channels)
            {
                result.ChannelResults[channel] = CreateFailedResult("Novu provider not configured", channel);
            }
            return result;
        }

        try
        {
            var workflowId = !string.IsNullOrEmpty(request.TemplateId)
                ? request.TemplateId
                : _config.MultiChannelWorkflowId;

            var triggerPayload = new NovuTriggerRequest
            {
                Name = workflowId,
                To = new NovuSubscriberTo { SubscriberId = request.SubscriberId },
                Payload = request.Payload ?? request.Content ?? new Dictionary<string, object>()
            };

            var response = await _httpClient.PostAsJsonAsync(
                "v1/events/trigger",
                triggerPayload,
                _jsonOptions,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var triggerResult = await response.Content.ReadFromJsonAsync<NovuTriggerResponse>(
                    _jsonOptions, cancellationToken);

                if (triggerResult?.Data?.Acknowledged == true)
                {
                    result.Success = true;
                    result.TransactionId = triggerResult.Data.TransactionId;

                    foreach (var channel in request.Channels)
                    {
                        result.ChannelResults[channel] = CreateSuccessResult(triggerResult.Data.TransactionId, channel);
                    }

                    _logger.LogInformation(
                        "Multi-channel notification sent via Novu. TransactionId: {TransactionId}, Channels: {Channels}",
                        triggerResult.Data.TransactionId, string.Join(", ", request.Channels));
                }
            }
            else
            {
                result.Success = false;
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                foreach (var channel in request.Channels)
                {
                    result.ChannelResults[channel] = CreateFailedResult(error, channel);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send multi-channel notification via Novu");
            result.Success = false;
            foreach (var channel in request.Channels)
            {
                result.ChannelResults[channel] = CreateFailedResult(ex.Message, channel);
            }
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<NotificationResult> TriggerWorkflowAsync(
        string workflowId,
        string subscriberId,
        object payload,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(workflowId))
        {
            throw new ArgumentException("Workflow ID is required", nameof(workflowId));
        }
        if (string.IsNullOrWhiteSpace(subscriberId))
        {
            throw new ArgumentException("Subscriber ID is required", nameof(subscriberId));
        }

        if (!_isConfigured)
        {
            return CreateFailedResult("Novu provider not configured", null);
        }

        try
        {
            var triggerPayload = new NovuTriggerRequest
            {
                Name = workflowId,
                To = new NovuSubscriberTo { SubscriberId = subscriberId },
                Payload = payload as Dictionary<string, object> ?? new Dictionary<string, object> { ["data"] = payload }
            };

            var response = await _httpClient.PostAsJsonAsync(
                "v1/events/trigger",
                triggerPayload,
                _jsonOptions,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<NovuTriggerResponse>(
                    _jsonOptions, cancellationToken);

                if (result?.Data?.Acknowledged == true)
                {
                    _logger.LogInformation(
                        "Workflow triggered via Novu. WorkflowId: {WorkflowId}, TransactionId: {TransactionId}",
                        workflowId, result.Data.TransactionId);
                    return CreateSuccessResult(result.Data.TransactionId, null);
                }
            }

            return CreateFailedResult("Workflow not acknowledged", null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to trigger workflow {WorkflowId} via Novu", workflowId);
            return CreateFailedResult(ex.Message, null);
        }
    }

    #endregion

    #region Bulk Operations

    /// <inheritdoc />
    public async Task<BulkNotificationResult> SendBulkEmailAsync(
        IEnumerable<EmailNotificationRequest> requests,
        CancellationToken cancellationToken = default)
    {
        var result = new BulkNotificationResult
        {
            Results = new List<NotificationResult>()
        };

        var requestList = requests.ToList();
        result.TotalCount = requestList.Count;

        foreach (var request in requestList)
        {
            var sendResult = await SendEmailAsync(request, cancellationToken);
            result.Results.Add(sendResult);

            if (sendResult.Success)
            {
                result.SuccessCount++;
            }
            else
            {
                result.FailureCount++;
            }
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<BulkNotificationResult> SendBulkSmsAsync(
        IEnumerable<SmsNotificationRequest> requests,
        CancellationToken cancellationToken = default)
    {
        var result = new BulkNotificationResult
        {
            Results = new List<NotificationResult>()
        };

        var requestList = requests.ToList();
        result.TotalCount = requestList.Count;

        foreach (var request in requestList)
        {
            var sendResult = await SendSmsAsync(request, cancellationToken);
            result.Results.Add(sendResult);

            if (sendResult.Success)
            {
                result.SuccessCount++;
            }
            else
            {
                result.FailureCount++;
            }
        }

        return result;
    }

    #endregion

    #region Subscriber Management

    /// <inheritdoc />
    public async Task<string> UpsertSubscriberAsync(
        SubscriberRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.SubscriberId))
        {
            throw new ArgumentException("Subscriber ID is required", nameof(request));
        }

        if (!_isConfigured)
        {
            throw new InvalidOperationException("Novu provider not configured");
        }

        try
        {
            var subscriberData = new NovuSubscriberCreateRequest
            {
                SubscriberId = request.SubscriberId,
                Email = request.Email,
                Phone = request.Phone,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Avatar = request.Avatar,
                Locale = request.Locale,
                Data = request.Data
            };

            var response = await _httpClient.PutAsJsonAsync(
                $"v1/subscribers/{request.SubscriberId}",
                subscriberData,
                _jsonOptions,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Subscriber upserted in Novu: {SubscriberId}", request.SubscriberId);
                return request.SubscriberId;
            }

            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Failed to upsert subscriber: {error}");
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            _logger.LogError(ex, "Failed to upsert subscriber {SubscriberId} in Novu", request.SubscriberId);
            throw new InvalidOperationException($"Failed to upsert subscriber: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task DeleteSubscriberAsync(
        string subscriberId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(subscriberId))
        {
            throw new ArgumentException("Subscriber ID is required", nameof(subscriberId));
        }

        if (!_isConfigured)
        {
            throw new InvalidOperationException("Novu provider not configured");
        }

        try
        {
            var response = await _httpClient.DeleteAsync(
                $"v1/subscribers/{subscriberId}",
                cancellationToken);

            if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException($"Failed to delete subscriber: {error}");
            }

            _logger.LogInformation("Subscriber deleted from Novu: {SubscriberId}", subscriberId);
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            _logger.LogError(ex, "Failed to delete subscriber {SubscriberId} from Novu", subscriberId);
            throw new InvalidOperationException($"Failed to delete subscriber: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<SubscriberPreferences?> GetSubscriberPreferencesAsync(
        string subscriberId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(subscriberId))
        {
            throw new ArgumentException("Subscriber ID is required", nameof(subscriberId));
        }

        if (!_isConfigured)
        {
            return null;
        }

        try
        {
            var response = await _httpClient.GetAsync(
                $"v1/subscribers/{subscriberId}/preferences",
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var novuPrefs = await response.Content.ReadFromJsonAsync<NovuPreferencesResponse>(
                    _jsonOptions, cancellationToken);

                if (novuPrefs?.Data != null)
                {
                    return MapNovuPreferences(novuPrefs.Data);
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get preferences for subscriber {SubscriberId}", subscriberId);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task UpdateSubscriberPreferencesAsync(
        string subscriberId,
        SubscriberPreferences preferences,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(subscriberId))
        {
            throw new ArgumentException("Subscriber ID is required", nameof(subscriberId));
        }
        ArgumentNullException.ThrowIfNull(preferences);

        if (!_isConfigured)
        {
            throw new InvalidOperationException("Novu provider not configured");
        }

        try
        {
            var novuPrefs = MapToNovuPreferences(preferences);

            var response = await _httpClient.PatchAsJsonAsync(
                $"v1/subscribers/{subscriberId}/preferences",
                novuPrefs,
                _jsonOptions,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException($"Failed to update preferences: {error}");
            }

            _logger.LogInformation("Preferences updated for subscriber: {SubscriberId}", subscriberId);
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            _logger.LogError(ex, "Failed to update preferences for subscriber {SubscriberId}", subscriberId);
            throw new InvalidOperationException($"Failed to update preferences: {ex.Message}", ex);
        }
    }

    #endregion

    #region Delivery Status

    /// <inheritdoc />
    public async Task<DeliveryStatus?> GetDeliveryStatusAsync(
        string notificationId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(notificationId))
        {
            throw new ArgumentException("Notification ID is required", nameof(notificationId));
        }

        if (!_isConfigured)
        {
            return null;
        }

        try
        {
            var response = await _httpClient.GetAsync(
                $"v1/notifications/{notificationId}",
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var notification = await response.Content.ReadFromJsonAsync<NovuNotificationResponse>(
                    _jsonOptions, cancellationToken);

                if (notification?.Data != null)
                {
                    return new DeliveryStatus
                    {
                        NotificationId = notificationId,
                        Status = notification.Data.Status ?? "unknown",
                        Channel = notification.Data.Channel ?? "unknown"
                    };
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get delivery status for notification {NotificationId}", notificationId);
            return null;
        }
    }

    /// <inheritdoc />
    public Task<DeliveryEvent> ProcessDeliveryWebhookAsync(
        string eventType,
        string payload,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(eventType))
        {
            throw new ArgumentException("Event type is required", nameof(eventType));
        }
        if (string.IsNullOrWhiteSpace(payload))
        {
            throw new ArgumentException("Payload is required", nameof(payload));
        }

        try
        {
            var webhookData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(payload, _jsonOptions);

            var deliveryEvent = new DeliveryEvent
            {
                EventType = eventType,
                Timestamp = DateTime.UtcNow
            };

            if (webhookData != null)
            {
                if (webhookData.TryGetValue("notificationId", out var notifId))
                {
                    deliveryEvent.NotificationId = notifId.GetString() ?? string.Empty;
                }
                if (webhookData.TryGetValue("subscriberId", out var subId))
                {
                    deliveryEvent.SubscriberId = subId.GetString() ?? string.Empty;
                }
                if (webhookData.TryGetValue("channel", out var channel))
                {
                    deliveryEvent.Channel = channel.GetString() ?? string.Empty;
                }

                deliveryEvent.Data = webhookData.ToDictionary(
                    kvp => kvp.Key,
                    kvp => (object)kvp.Value.ToString());
            }

            _logger.LogInformation(
                "Processed Novu delivery webhook. EventType: {EventType}, NotificationId: {NotificationId}",
                eventType, deliveryEvent.NotificationId);

            return Task.FromResult(deliveryEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process Novu delivery webhook");
            return Task.FromResult(new DeliveryEvent
            {
                EventType = eventType,
                Timestamp = DateTime.UtcNow,
                Data = new Dictionary<string, object> { ["error"] = ex.Message }
            });
        }
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

        if (!_isConfigured)
        {
            result.IsHealthy = false;
            result.Message = "Novu provider is not configured";
            return result;
        }

        try
        {
            var isAvailable = await IsAvailableAsync(cancellationToken);
            result.IsHealthy = isAvailable;
            result.Message = isAvailable
                ? "Novu API is accessible"
                : "Novu API is not accessible";

            result.Details = new Dictionary<string, object>
            {
                ["url"] = _config.Url,
                ["useSelfHosted"] = _config.UseSelfHosted,
                ["supportedChannels"] = SupportedChannels.ToList()
            };
        }
        catch (Exception ex)
        {
            result.IsHealthy = false;
            result.Message = ex.Message;
        }

        return result;
    }

    #endregion

    #region Helper Methods

    private async Task<string> EnsureSubscriberAsync(
        string subscriberId,
        string? name,
        string? email,
        string? phone,
        CancellationToken cancellationToken)
    {
        try
        {
            var subscriberData = new NovuSubscriberCreateRequest
            {
                SubscriberId = subscriberId,
                Email = email,
                Phone = phone
            };

            if (!string.IsNullOrEmpty(name))
            {
                var nameParts = name.Split(' ', 2);
                subscriberData.FirstName = nameParts[0];
                if (nameParts.Length > 1)
                {
                    subscriberData.LastName = nameParts[1];
                }
            }

            var response = await _httpClient.PostAsJsonAsync(
                "v1/subscribers",
                subscriberData,
                _jsonOptions,
                cancellationToken);

            // If subscriber already exists (409), that's fine
            if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                return subscriberId;
            }

            _logger.LogWarning("Failed to ensure subscriber {SubscriberId}: {StatusCode}",
                subscriberId, response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to ensure subscriber {SubscriberId}", subscriberId);
        }

        return subscriberId;
    }

    private NotificationResult CreateSuccessResult(string? messageId, string? channel)
    {
        return new NotificationResult
        {
            Success = true,
            MessageId = messageId,
            Provider = ProviderName,
            Channel = channel,
            SentAt = DateTime.UtcNow
        };
    }

    private NotificationResult CreateFailedResult(string error, string? channel)
    {
        return new NotificationResult
        {
            Success = false,
            Error = error,
            Provider = ProviderName,
            Channel = channel,
            SentAt = DateTime.UtcNow
        };
    }

    private SubscriberPreferences MapNovuPreferences(List<NovuPreferenceItem> novuPrefs)
    {
        var prefs = new SubscriberPreferences
        {
            ChannelPreferences = new Dictionary<string, bool>(),
            CategoryPreferences = new Dictionary<string, SubscriberCategoryPreference>()
        };

        foreach (var pref in novuPrefs)
        {
            if (pref.Template?.Critical == true)
            {
                continue; // Critical notifications can't be disabled
            }

            var category = pref.Template?.Name ?? pref.Template?.Identifier ?? "default";
            prefs.CategoryPreferences[category] = new SubscriberCategoryPreference
            {
                Enabled = pref.Preference?.Enabled ?? true
            };
        }

        return prefs;
    }

    private object MapToNovuPreferences(SubscriberPreferences preferences)
    {
        return new
        {
            global = !preferences.GlobalOptOut,
            channels = preferences.ChannelPreferences
        };
    }

    #endregion
}

#region Novu API Models

internal class NovuTriggerRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("to")]
    public NovuSubscriberTo To { get; set; } = new();

    [JsonPropertyName("payload")]
    public Dictionary<string, object> Payload { get; set; } = new();
}

internal class NovuSubscriberTo
{
    [JsonPropertyName("subscriberId")]
    public string SubscriberId { get; set; } = string.Empty;
}

internal class NovuTriggerResponse
{
    [JsonPropertyName("data")]
    public NovuTriggerData? Data { get; set; }
}

internal class NovuTriggerData
{
    [JsonPropertyName("acknowledged")]
    public bool Acknowledged { get; set; }

    [JsonPropertyName("transactionId")]
    public string? TransactionId { get; set; }
}

internal class NovuSubscriberCreateRequest
{
    [JsonPropertyName("subscriberId")]
    public string SubscriberId { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }

    [JsonPropertyName("lastName")]
    public string? LastName { get; set; }

    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }

    [JsonPropertyName("locale")]
    public string? Locale { get; set; }

    [JsonPropertyName("data")]
    public Dictionary<string, object>? Data { get; set; }
}

internal class NovuPreferencesResponse
{
    [JsonPropertyName("data")]
    public List<NovuPreferenceItem>? Data { get; set; }
}

internal class NovuPreferenceItem
{
    [JsonPropertyName("template")]
    public NovuTemplateInfo? Template { get; set; }

    [JsonPropertyName("preference")]
    public NovuPreferenceValue? Preference { get; set; }
}

internal class NovuTemplateInfo
{
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("identifier")]
    public string? Identifier { get; set; }

    [JsonPropertyName("critical")]
    public bool Critical { get; set; }
}

internal class NovuPreferenceValue
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("channels")]
    public Dictionary<string, bool>? Channels { get; set; }
}

internal class NovuNotificationResponse
{
    [JsonPropertyName("data")]
    public NovuNotificationData? Data { get; set; }
}

internal class NovuNotificationData
{
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("channel")]
    public string? Channel { get; set; }
}

#endregion
