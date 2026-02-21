// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Output.Providers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CRM.Api.Controllers.Webhooks;

[ApiController]
[Route("api/webhooks/sendgrid")]
public class SendGridWebhookController : ControllerBase
{
    private readonly INotificationPort _notificationProvider;
    private readonly IActivityService _activityService;
    private readonly ILogger<SendGridWebhookController> _logger;

    public SendGridWebhookController(
        INotificationPort notificationProvider,
        IActivityService activityService,
        ILogger<SendGridWebhookController> logger)
    {
        _notificationProvider = notificationProvider;
        _activityService = activityService;
        _logger = logger;
    }

    [HttpPost("events")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> HandleEvents()
    {
        try
        {
            using var reader = new StreamReader(Request.Body, Encoding.UTF8);
            var payload = await reader.ReadToEndAsync();
            _logger.LogDebug("SendGrid webhook: {Payload}", TruncateForLog(payload, 200));

            var deliveryEvent = await _notificationProvider.ProcessDeliveryWebhookAsync("batch", payload);
            _logger.LogInformation("SendGrid: Id={Id}, Event={Event}", deliveryEvent.NotificationId, deliveryEvent.EventType);

            try
            {
                var activity = new Activity
                {
                    ActivityType = GetActivityTypeForEvent(deliveryEvent.EventType),
                    Title = GetActivityTitle(deliveryEvent),
                    Description = GetActivityDescription(deliveryEvent),
                    Details = System.Text.Json.JsonSerializer.Serialize(new { deliveryEvent.NotificationId, deliveryEvent.SubscriberId, deliveryEvent.EventType, deliveryEvent.Timestamp, Provider = "SendGrid" }),
                    ActivityDate = deliveryEvent.Timestamp,
                    CreatedAt = DateTime.UtcNow,
                    Source = "SendGrid",
                    IsSystem = true,
                    Category = "Email"
                };
                await _activityService.CreateAsync(activity);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create Activity");
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing SendGrid webhook");
            return Ok();
        }
    }

    [HttpPost("inbound")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> HandleInboundEmail()
    {
        try
        {
            var form = await Request.ReadFormAsync();
            _logger.LogInformation("SendGrid inbound: From={From}, To={To}", form["from"], form["to"]);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing inbound email");
            return Ok();
        }
    }

    private static string TruncateForLog(string? text, int maxLength = 100) =>
        string.IsNullOrEmpty(text) ? "" : text.Length <= maxLength ? text : text[..maxLength] + "...";

    private static ActivityType GetActivityTypeForEvent(string eventType) =>
        eventType.ToLowerInvariant() switch
        {
            "processed" or "delivered" => ActivityType.EmailSent,
            "open" or "click" => ActivityType.EmailReceived,
            "bounce" or "dropped" or "deferred" => ActivityType.EmailSent,
            "spamreport" or "unsubscribe" => ActivityType.StatusChanged,
            _ => ActivityType.Other
        };

    private static string GetActivityTitle(DeliveryEvent e) =>
        e.EventType.ToLowerInvariant() switch
        {
            "processed" => "Email processed for " + e.SubscriberId,
            "delivered" => "Email delivered to " + e.SubscriberId,
            "open" => "Email opened by " + e.SubscriberId,
            "click" => "Link clicked by " + e.SubscriberId,
            "bounce" => "Email bounced for " + e.SubscriberId,
            "dropped" => "Email dropped for " + e.SubscriberId,
            "deferred" => "Email deferred for " + e.SubscriberId,
            "spamreport" => "Spam report from " + e.SubscriberId,
            "unsubscribe" => "Unsubscribe from " + e.SubscriberId,
            _ => "SendGrid: " + e.EventType
        };

    private static string GetActivityDescription(DeliveryEvent e) =>
        "SendGrid " + e.EventType + " for message " + e.NotificationId + " at " + e.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
}
