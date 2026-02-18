// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
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
    private readonly IActivityService _activityService;
    private readonly ILogger<NovuWebhookController> _logger;

    public NovuWebhookController(
        INotificationPort notificationProvider,
        IActivityService activityService,
        ILogger<NovuWebhookController> logger)
    {
        _notificationProvider = notificationProvider;
        _activityService = activityService;
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
    [ProducesResponseType(StatusCodes.Status200OK)]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
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

    private async Task HandleNotificationSentAsync(NovuWebhookEvent evt)
    {
        _logger.LogInformation(
            "Notification sent. TransactionId: {TransactionId}, Channel: {Channel}",
            evt.TransactionId, evt.Channel);

        await _activityService.CreateAsync(new Activity
        {
            ActivityType = MapActivityType(evt.Channel, ActivityType.EmailSent),
            Title = "Notification sent",
            Description = $"Notification sent via {evt.Channel ?? "unknown"}",
            Details = JsonSerializer.Serialize(new
            {
                evt.TransactionId,
                evt.NotificationId,
                evt.SubscriberId,
                evt.Channel,
                evt.Status
            }),
            ActivityDate = DateTime.UtcNow,
            IsSystem = true,
            Source = "Novu"
        });
    }

    private async Task HandleNotificationDeliveredAsync(NovuWebhookEvent evt)
    {
        _logger.LogInformation(
            "Notification delivered. TransactionId: {TransactionId}, Channel: {Channel}",
            evt.TransactionId, evt.Channel);

        await _activityService.CreateAsync(new Activity
        {
            ActivityType = ActivityType.StatusChanged,
            Title = "Notification delivered",
            Description = $"Notification delivered via {evt.Channel ?? "unknown"}",
            Details = JsonSerializer.Serialize(new
            {
                evt.TransactionId,
                evt.NotificationId,
                evt.SubscriberId,
                evt.Channel,
                Status = "delivered"
            }),
            ActivityDate = DateTime.UtcNow,
            IsSystem = true,
            Source = "Novu"
        });
    }

    private async Task HandleNotificationBouncedAsync(NovuWebhookEvent evt)
    {
        _logger.LogWarning(
            "Notification bounced. TransactionId: {TransactionId}, Channel: {Channel}",
            evt.TransactionId, evt.Channel);

        await _activityService.CreateAsync(new Activity
        {
            ActivityType = ActivityType.StatusChanged,
            Title = "Notification bounced",
            Description = $"Notification bounced via {evt.Channel ?? "unknown"}",
            Details = JsonSerializer.Serialize(new
            {
                evt.TransactionId,
                evt.NotificationId,
                evt.SubscriberId,
                evt.Channel,
                Status = "bounced"
            }),
            ActivityDate = DateTime.UtcNow,
            IsSystem = true,
            Source = "Novu"
        });
    }

    private async Task HandleNotificationOpenedAsync(NovuWebhookEvent evt)
    {
        _logger.LogInformation(
            "Notification opened. TransactionId: {TransactionId}",
            evt.TransactionId);

        await _activityService.CreateAsync(new Activity
        {
            ActivityType = ActivityType.Other,
            Title = "Notification opened",
            Description = "Notification opened",
            Details = JsonSerializer.Serialize(new
            {
                evt.TransactionId,
                evt.NotificationId,
                evt.SubscriberId,
                evt.Channel,
                Status = "opened"
            }),
            ActivityDate = DateTime.UtcNow,
            IsSystem = true,
            Source = "Novu"
        });
    }

    private async Task HandleNotificationClickedAsync(NovuWebhookEvent evt)
    {
        _logger.LogInformation(
            "Link clicked in notification. TransactionId: {TransactionId}",
            evt.TransactionId);

        await _activityService.CreateAsync(new Activity
        {
            ActivityType = ActivityType.Other,
            Title = "Notification link clicked",
            Description = "Notification link clicked",
            Details = JsonSerializer.Serialize(new
            {
                evt.TransactionId,
                evt.NotificationId,
                evt.SubscriberId,
                evt.Channel,
                Status = "clicked"
            }),
            ActivityDate = DateTime.UtcNow,
            IsSystem = true,
            Source = "Novu"
        });
    }

    private async Task HandleUnsubscribedAsync(NovuWebhookEvent evt)
    {
        _logger.LogInformation(
            "Subscriber unsubscribed. SubscriberId: {SubscriberId}",
            evt.SubscriberId);

        await _activityService.CreateAsync(new Activity
        {
            ActivityType = ActivityType.StatusChanged,
            Title = "Subscriber unsubscribed",
            Description = "Subscriber unsubscribed from notifications",
            Details = JsonSerializer.Serialize(new
            {
                evt.TransactionId,
                evt.NotificationId,
                evt.SubscriberId,
                evt.Channel,
                Status = "unsubscribed"
            }),
            ActivityDate = DateTime.UtcNow,
            IsSystem = true,
            Source = "Novu"
        });
    }

    private static ActivityType MapActivityType(string? channel, ActivityType fallback)
    {
        return channel?.ToLowerInvariant() switch
        {
            "sms" => ActivityType.SMSSent,
            "push" => ActivityType.PushSent,
            "email" => ActivityType.EmailSent,
            _ => fallback
        };
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
