// CRM Solution - Novu Webhook Controller
// Phase 2 Week 9: Handles webhook callbacks from Novu
// Part of the Pluggable Architecture implementation

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CRM.Core.Ports.Output.Providers;
using System.Text.Json;

namespace CRM.Api.Controllers.Webhooks;

/// <summary>
/// Handles webhook callbacks from Novu notification platform.
/// Processes delivery status updates and syncs them to the CRM Activity timeline.
/// </summary>
[ApiController]
[Route("api/webhooks/novu")]
public class NovuWebhookController : ControllerBase
{
    private readonly INotificationPort _notificationProvider;
    private readonly ILogger<NovuWebhookController> _logger;

    public NovuWebhookController(
        INotificationPort notificationProvider,
        ILogger<NovuWebhookController> logger)
    {
        _notificationProvider = notificationProvider;
        _logger = logger;
    }

    /// <summary>
    /// Receives delivery status webhooks from Novu.
    /// Configure this endpoint in Novu dashboard under Integrations → Webhooks.
    /// </summary>
    /// <remarks>
    /// Novu webhook events include:
    /// - notification_sent: Notification was sent to the provider
    /// - notification_delivered: Provider confirmed delivery
    /// - notification_bounced: Email bounced
    /// - notification_clicked: Link clicked (email)
    /// - notification_opened: Email opened
    /// - notification_unsubscribed: User unsubscribed
    /// </remarks>
    [HttpPost("delivery")]
    public async Task<IActionResult> HandleDeliveryWebhook(
        [FromHeader(Name = "X-Novu-Signature")] string? signature)
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var payload = await reader.ReadToEndAsync();

            if (string.IsNullOrEmpty(payload))
            {
                _logger.LogWarning("Empty webhook payload received from Novu");
                return BadRequest("Empty payload");
            }

            _logger.LogInformation("Received Novu webhook. Payload length: {Length}", payload.Length);

            // Parse the webhook event
            var webhookEvent = ParseNovuWebhook(payload);

            if (webhookEvent == null)
            {
                _logger.LogWarning("Failed to parse Novu webhook payload");
                return BadRequest("Invalid payload format");
            }

            // Process based on event type
            await ProcessWebhookEventAsync(webhookEvent);

            return Ok(new { status = "processed", eventType = webhookEvent.EventType });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Novu webhook");
            // Return 200 to prevent Novu from retrying indefinitely
            return Ok(new { status = "error", message = ex.Message });
        }
    }

    /// <summary>
    /// Verify webhook connectivity (Novu may ping this endpoint).
    /// </summary>
    [HttpGet("delivery")]
    public IActionResult VerifyWebhook()
    {
        return Ok(new 
        { 
            status = "active",
            provider = "novu",
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Handles notification status updates from Novu.
    /// </summary>
    [HttpPost("notification-status")]
    public async Task<IActionResult> HandleNotificationStatus()
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var payload = await reader.ReadToEndAsync();

            _logger.LogInformation("Received Novu notification status update");

            var webhookEvent = ParseNovuWebhook(payload);
            if (webhookEvent != null)
            {
                await ProcessWebhookEventAsync(webhookEvent);
            }

            return Ok(new { status = "processed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Novu notification status");
            return Ok(new { status = "error", message = ex.Message });
        }
    }

    #region Private Helpers

    private NovuWebhookEvent? ParseNovuWebhook(string payload)
    {
        try
        {
            var doc = JsonDocument.Parse(payload);
            var root = doc.RootElement;

            return new NovuWebhookEvent
            {
                EventType = GetJsonString(root, "event") ?? GetJsonString(root, "type") ?? "unknown",
                TransactionId = GetJsonString(root, "transactionId"),
                SubscriberId = GetJsonString(root, "subscriberId"),
                NotificationId = GetJsonString(root, "notificationId"),
                Channel = GetJsonString(root, "channel"),
                Status = GetJsonString(root, "status"),
                Timestamp = DateTime.UtcNow,
                RawPayload = payload
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse Novu webhook JSON");
            return null;
        }
    }

    private static string? GetJsonString(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var value) && 
            value.ValueKind == JsonValueKind.String)
        {
            return value.GetString();
        }
        return null;
    }

    private async Task ProcessWebhookEventAsync(NovuWebhookEvent webhookEvent)
    {
        _logger.LogInformation(
            "Processing Novu event: {EventType}, TransactionId: {TransactionId}, Channel: {Channel}",
            webhookEvent.EventType,
            webhookEvent.TransactionId,
            webhookEvent.Channel);

        switch (webhookEvent.EventType.ToLowerInvariant())
        {
            case "notification_sent":
            case "message.sent":
                await HandleNotificationSentAsync(webhookEvent);
                break;

            case "notification_delivered":
            case "message.delivered":
                await HandleNotificationDeliveredAsync(webhookEvent);
                break;

            case "notification_bounced":
            case "message.bounced":
                await HandleNotificationBouncedAsync(webhookEvent);
                break;

            case "notification_opened":
            case "message.opened":
                await HandleNotificationOpenedAsync(webhookEvent);
                break;

            case "notification_clicked":
            case "message.clicked":
                await HandleNotificationClickedAsync(webhookEvent);
                break;

            case "notification_unsubscribed":
            case "subscriber.unsubscribed":
                await HandleUnsubscribedAsync(webhookEvent);
                break;

            default:
                _logger.LogDebug("Unhandled Novu event type: {EventType}", webhookEvent.EventType);
                break;
        }
    }

    private Task HandleNotificationSentAsync(NovuWebhookEvent evt)
    {
        _logger.LogInformation(
            "Notification sent. TransactionId: {TransactionId}, Channel: {Channel}",
            evt.TransactionId, evt.Channel);

        // TODO: Create Activity record for sent notification
        // await _activityService.CreateAsync(new Activity { ... });

        return Task.CompletedTask;
    }

    private Task HandleNotificationDeliveredAsync(NovuWebhookEvent evt)
    {
        _logger.LogInformation(
            "Notification delivered. TransactionId: {TransactionId}, Channel: {Channel}",
            evt.TransactionId, evt.Channel);

        // TODO: Update Activity record with delivery confirmation

        return Task.CompletedTask;
    }

    private Task HandleNotificationBouncedAsync(NovuWebhookEvent evt)
    {
        _logger.LogWarning(
            "Notification bounced. TransactionId: {TransactionId}, Channel: {Channel}",
            evt.TransactionId, evt.Channel);

        // TODO: Update Activity record with bounce status
        // TODO: Potentially mark contact email as invalid

        return Task.CompletedTask;
    }

    private Task HandleNotificationOpenedAsync(NovuWebhookEvent evt)
    {
        _logger.LogInformation(
            "Notification opened. TransactionId: {TransactionId}",
            evt.TransactionId);

        // TODO: Create Activity for email open tracking

        return Task.CompletedTask;
    }

    private Task HandleNotificationClickedAsync(NovuWebhookEvent evt)
    {
        _logger.LogInformation(
            "Link clicked in notification. TransactionId: {TransactionId}",
            evt.TransactionId);

        // TODO: Create Activity for click tracking

        return Task.CompletedTask;
    }

    private Task HandleUnsubscribedAsync(NovuWebhookEvent evt)
    {
        _logger.LogInformation(
            "Subscriber unsubscribed. SubscriberId: {SubscriberId}",
            evt.SubscriberId);

        // TODO: Update contact preferences to reflect unsubscription

        return Task.CompletedTask;
    }

    #endregion
}

/// <summary>
/// Represents a parsed Novu webhook event.
/// </summary>
public class NovuWebhookEvent
{
    public string EventType { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public string? SubscriberId { get; set; }
    public string? NotificationId { get; set; }
    public string? Channel { get; set; }
    public string? Status { get; set; }
    public DateTime Timestamp { get; set; }
    public string? RawPayload { get; set; }
}
