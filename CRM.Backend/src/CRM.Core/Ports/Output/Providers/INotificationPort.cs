// CRM Solution - Pluggable Architecture
// Output Port for Notification Provider
// 
// HEXAGONAL ARCHITECTURE NOTE:
// This port defines the contract for notification/communication operations.
// Implementations: BuiltInNotificationProvider (email), NovuProvider, TwilioProvider, SendGridProvider
// Supports multi-channel notifications: email, SMS, push, in-app.

namespace CRM.Core.Ports.Output.Providers;

#region Notification Port Interface

/// <summary>
/// Output port for notification operations supporting pluggable notification providers.
/// Enables multi-channel notifications (email, SMS, push, in-app) with template support.
/// Implementations: BuiltIn (SMTP), Novu, Twilio, SendGrid, OneSignal.
/// </summary>
public interface INotificationPort
{
    /// <summary>
    /// Gets the unique identifier for this notification provider.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Checks if the notification provider is properly configured and available.
    /// </summary>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the channels supported by this provider.
    /// </summary>
    IEnumerable<string> SupportedChannels { get; }

    #region Email Operations

    /// <summary>
    /// Sends a simple email notification.
    /// </summary>
    /// <param name="request">Email details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Notification result with message ID.</returns>
    Task<NotificationResult> SendEmailAsync(EmailNotificationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a template-based email.
    /// </summary>
    /// <param name="templateId">The template identifier.</param>
    /// <param name="recipientEmail">Recipient email address.</param>
    /// <param name="data">Template variable data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Notification result.</returns>
    Task<NotificationResult> SendTemplateEmailAsync(string templateId, string recipientEmail, object data, CancellationToken cancellationToken = default);

    #endregion

    #region SMS Operations

    /// <summary>
    /// Sends an SMS notification.
    /// </summary>
    /// <param name="request">SMS details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Notification result.</returns>
    Task<NotificationResult> SendSmsAsync(SmsNotificationRequest request, CancellationToken cancellationToken = default);

    #endregion

    #region Push Notification Operations

    /// <summary>
    /// Sends a push notification.
    /// </summary>
    /// <param name="request">Push notification details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Notification result.</returns>
    Task<NotificationResult> SendPushAsync(PushNotificationRequest request, CancellationToken cancellationToken = default);

    #endregion

    #region In-App Notifications

    /// <summary>
    /// Sends an in-app notification.
    /// </summary>
    /// <param name="request">In-app notification details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Notification result.</returns>
    Task<NotificationResult> SendInAppAsync(InAppNotificationRequest request, CancellationToken cancellationToken = default);

    #endregion

    #region Multi-Channel Operations

    /// <summary>
    /// Sends a notification across multiple channels based on subscriber preferences.
    /// </summary>
    /// <param name="request">Multi-channel notification request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Notification result with per-channel status.</returns>
    Task<MultiChannelNotificationResult> SendNotificationAsync(MultiChannelNotificationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Triggers a workflow/template-based notification.
    /// </summary>
    /// <param name="workflowId">The workflow/template identifier.</param>
    /// <param name="subscriberId">The subscriber identifier.</param>
    /// <param name="payload">Notification payload data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Notification result.</returns>
    Task<NotificationResult> TriggerWorkflowAsync(string workflowId, string subscriberId, object payload, CancellationToken cancellationToken = default);

    #endregion

    #region Bulk Operations

    /// <summary>
    /// Sends bulk email notifications.
    /// </summary>
    /// <param name="requests">Email requests.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Bulk notification result.</returns>
    Task<BulkNotificationResult> SendBulkEmailAsync(IEnumerable<EmailNotificationRequest> requests, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends bulk SMS notifications.
    /// </summary>
    /// <param name="requests">SMS requests.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Bulk notification result.</returns>
    Task<BulkNotificationResult> SendBulkSmsAsync(IEnumerable<SmsNotificationRequest> requests, CancellationToken cancellationToken = default);

    #endregion

    #region Subscriber Management

    /// <summary>
    /// Creates or updates a subscriber in the notification system.
    /// </summary>
    /// <param name="request">Subscriber details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The subscriber ID.</returns>
    Task<string> UpsertSubscriberAsync(SubscriberRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a subscriber.
    /// </summary>
    /// <param name="subscriberId">The subscriber ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteSubscriberAsync(string subscriberId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets subscriber notification preferences.
    /// </summary>
    /// <param name="subscriberId">The subscriber ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Subscriber preferences.</returns>
    Task<SubscriberPreferences?> GetSubscriberPreferencesAsync(string subscriberId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates subscriber notification preferences.
    /// </summary>
    /// <param name="subscriberId">The subscriber ID.</param>
    /// <param name="preferences">Updated preferences.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateSubscriberPreferencesAsync(string subscriberId, SubscriberPreferences preferences, CancellationToken cancellationToken = default);

    #endregion

    #region Delivery Status

    /// <summary>
    /// Gets the delivery status of a notification.
    /// </summary>
    /// <param name="notificationId">The notification ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Delivery status.</returns>
    Task<DeliveryStatus?> GetDeliveryStatusAsync(string notificationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes a delivery webhook.
    /// </summary>
    /// <param name="eventType">The webhook event type.</param>
    /// <param name="payload">The webhook payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Processed delivery event.</returns>
    Task<DeliveryEvent> ProcessDeliveryWebhookAsync(string eventType, string payload, CancellationToken cancellationToken = default);

    #endregion

    /// <summary>
    /// Gets the health status of the notification provider.
    /// </summary>
    Task<ProviderHealthResult> HealthCheckAsync(CancellationToken cancellationToken = default);
}

#endregion

#region Notification DTOs

/// <summary>
/// Email notification request.
/// </summary>
public class EmailNotificationRequest
{
    /// <summary>
    /// Recipient email address.
    /// </summary>
    public string To { get; set; } = string.Empty;

    /// <summary>
    /// Recipient name (optional).
    /// </summary>
    public string? ToName { get; set; }

    /// <summary>
    /// Email subject.
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Email body content.
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Whether the body is HTML.
    /// </summary>
    public bool IsHtml { get; set; } = true;

    /// <summary>
    /// Plain text alternative (for HTML emails).
    /// </summary>
    public string? PlainTextBody { get; set; }

    /// <summary>
    /// From email address (optional, uses default if not specified).
    /// </summary>
    public string? From { get; set; }

    /// <summary>
    /// From name.
    /// </summary>
    public string? FromName { get; set; }

    /// <summary>
    /// Reply-to address.
    /// </summary>
    public string? ReplyTo { get; set; }

    /// <summary>
    /// CC recipients.
    /// </summary>
    public List<string>? Cc { get; set; }

    /// <summary>
    /// BCC recipients.
    /// </summary>
    public List<string>? Bcc { get; set; }

    /// <summary>
    /// Email attachments.
    /// </summary>
    public List<EmailAttachment>? Attachments { get; set; }

    /// <summary>
    /// Custom headers.
    /// </summary>
    public Dictionary<string, string>? Headers { get; set; }

    /// <summary>
    /// Tracking tags.
    /// </summary>
    public List<string>? Tags { get; set; }

    /// <summary>
    /// Custom metadata.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Email attachment.
/// </summary>
public class EmailAttachment
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/octet-stream";
    public byte[]? Content { get; set; }
    public string? ContentUrl { get; set; }
    public bool IsInline { get; set; }
    public string? ContentId { get; set; }
}

/// <summary>
/// SMS notification request.
/// </summary>
public class SmsNotificationRequest
{
    /// <summary>
    /// Recipient phone number (E.164 format).
    /// </summary>
    public string To { get; set; } = string.Empty;

    /// <summary>
    /// SMS message content.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// From phone number (optional).
    /// </summary>
    public string? From { get; set; }

    /// <summary>
    /// Sender ID for alpha-numeric sending.
    /// </summary>
    public string? SenderId { get; set; }

    /// <summary>
    /// Media URLs for MMS.
    /// </summary>
    public List<string>? MediaUrls { get; set; }

    /// <summary>
    /// Callback URL for delivery status.
    /// </summary>
    public string? StatusCallback { get; set; }

    /// <summary>
    /// Custom metadata.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Push notification request.
/// </summary>
public class PushNotificationRequest
{
    /// <summary>
    /// Device token or topic.
    /// </summary>
    public string To { get; set; } = string.Empty;

    /// <summary>
    /// Notification title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Notification body.
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Icon URL or name.
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Image URL for rich notifications.
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Deep link URL.
    /// </summary>
    public string? ActionUrl { get; set; }

    /// <summary>
    /// Action buttons.
    /// </summary>
    public List<PushAction>? Actions { get; set; }

    /// <summary>
    /// Custom data payload.
    /// </summary>
    public Dictionary<string, object>? Data { get; set; }

    /// <summary>
    /// Badge count.
    /// </summary>
    public int? Badge { get; set; }

    /// <summary>
    /// Sound name.
    /// </summary>
    public string? Sound { get; set; }

    /// <summary>
    /// Time to live in seconds.
    /// </summary>
    public int? TtlSeconds { get; set; }
}

/// <summary>
/// Push notification action button.
/// </summary>
public class PushAction
{
    public string ActionId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Url { get; set; }
}

/// <summary>
/// In-app notification request.
/// </summary>
public class InAppNotificationRequest
{
    /// <summary>
    /// User/subscriber ID.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Notification title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Notification content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Notification type/category.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Link to navigate to when clicked.
    /// </summary>
    public string? ActionUrl { get; set; }

    /// <summary>
    /// Avatar or icon URL.
    /// </summary>
    public string? Avatar { get; set; }

    /// <summary>
    /// Custom data.
    /// </summary>
    public Dictionary<string, object>? Data { get; set; }

    /// <summary>
    /// Whether to mark as read automatically.
    /// </summary>
    public bool AutoRead { get; set; }
}

/// <summary>
/// Multi-channel notification request.
/// </summary>
public class MultiChannelNotificationRequest
{
    /// <summary>
    /// Subscriber ID.
    /// </summary>
    public string SubscriberId { get; set; } = string.Empty;

    /// <summary>
    /// Channels to send on (email, sms, push, in_app).
    /// </summary>
    public List<string> Channels { get; set; } = new() { "email" };

    /// <summary>
    /// Template/workflow ID (optional).
    /// </summary>
    public string? TemplateId { get; set; }

    /// <summary>
    /// Notification content for each channel.
    /// </summary>
    public Dictionary<string, object> Content { get; set; } = new();

    /// <summary>
    /// Template/payload data.
    /// </summary>
    public Dictionary<string, object>? Payload { get; set; }

    /// <summary>
    /// Override subscriber preferences.
    /// </summary>
    public bool OverridePreferences { get; set; }
}

/// <summary>
/// Result of a notification send operation.
/// </summary>
public class NotificationResult
{
    /// <summary>
    /// Whether the send was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Provider-assigned message/notification ID.
    /// </summary>
    public string? MessageId { get; set; }

    /// <summary>
    /// The provider that sent the notification.
    /// </summary>
    public string? Provider { get; set; }

    /// <summary>
    /// The channel used.
    /// </summary>
    public string? Channel { get; set; }

    /// <summary>
    /// Error message if failed.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Error code if failed.
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Timestamp of send.
    /// </summary>
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Result of a multi-channel notification.
/// </summary>
public class MultiChannelNotificationResult
{
    public bool Success { get; set; }
    public string? TransactionId { get; set; }
    public Dictionary<string, NotificationResult> ChannelResults { get; set; } = new();
}

/// <summary>
/// Result of bulk notification operation.
/// </summary>
public class BulkNotificationResult
{
    public int TotalCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<NotificationResult> Results { get; set; } = new();
}

/// <summary>
/// Subscriber information for notification systems.
/// </summary>
public class SubscriberRequest
{
    /// <summary>
    /// Unique subscriber ID (typically CRM user ID).
    /// </summary>
    public string SubscriberId { get; set; } = string.Empty;

    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Avatar { get; set; }
    public string? Locale { get; set; }
    public string? Timezone { get; set; }

    /// <summary>
    /// Device tokens for push notifications.
    /// </summary>
    public List<DeviceToken>? DeviceTokens { get; set; }

    /// <summary>
    /// Custom data.
    /// </summary>
    public Dictionary<string, object>? Data { get; set; }
}

/// <summary>
/// Device token for push notifications.
/// </summary>
public class DeviceToken
{
    public string Token { get; set; } = string.Empty;
    public string Provider { get; set; } = "fcm"; // fcm, apns, expo
    public string? DeviceId { get; set; }
}

/// <summary>
/// Subscriber notification preferences.
/// </summary>
public class SubscriberPreferences
{
    /// <summary>
    /// Channel-level preferences.
    /// </summary>
    public Dictionary<string, bool> ChannelPreferences { get; set; } = new();

    /// <summary>
    /// Category-level preferences.
    /// </summary>
    public Dictionary<string, SubscriberCategoryPreference> CategoryPreferences { get; set; } = new();

    /// <summary>
    /// Global opt-out.
    /// </summary>
    public bool GlobalOptOut { get; set; }
}

/// <summary>
/// Category-specific preferences.
/// </summary>
public class SubscriberCategoryPreference
{
    public bool Enabled { get; set; } = true;
    public Dictionary<string, bool>? ChannelOverrides { get; set; }
}

/// <summary>
/// Notification delivery status.
/// </summary>
public class DeliveryStatus
{
    public string NotificationId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // queued, sent, delivered, bounced, failed, opened, clicked
    public string Channel { get; set; } = string.Empty;
    public DateTime? DeliveredAt { get; set; }
    public DateTime? OpenedAt { get; set; }
    public DateTime? ClickedAt { get; set; }
    public string? FailureReason { get; set; }
}

/// <summary>
/// Delivery webhook event.
/// </summary>
public class DeliveryEvent
{
    public string EventType { get; set; } = string.Empty;
    public string NotificationId { get; set; } = string.Empty;
    public string SubscriberId { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object>? Data { get; set; }
}

#endregion
