// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Output.Providers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CRM.Api.Controllers.Webhooks;

/// <summary>
/// Handles incoming Twilio webhook callbacks for SMS status and inbound messages.
/// </summary>
[ApiController]
[Route("api/webhooks/twilio")]
public class TwilioWebhookController : ControllerBase
{
    private readonly INotificationPort _notificationProvider;
    private readonly IActivityService _activityService;
    private readonly ILogger<TwilioWebhookController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TwilioWebhookController"/> class.
    /// </summary>
    public TwilioWebhookController(
        INotificationPort notificationProvider,
        IActivityService activityService,
        ILogger<TwilioWebhookController> logger)
    {
        _notificationProvider = notificationProvider;
        _activityService = activityService;
        _logger = logger;
    }

    [HttpPost("status")]
    [Consumes("application/x-www-form-urlencoded")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> HandleStatusCallback()
    {
        try
        {
            var form = await Request.ReadFormAsync();
            var messageSid = form["MessageSid"].ToString();
            var messageStatus = form["MessageStatus"].ToString();
            var to = form["To"].ToString();
            var from = form["From"].ToString();
            var errorCode = form["ErrorCode"].ToString();

            _logger.LogInformation("Twilio status: Sid={Sid}, Status={Status}, To={To}", messageSid, messageStatus, to);

            var payload = string.Join("&", form.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value.ToString())}"));
            var deliveryEvent = await _notificationProvider.ProcessDeliveryWebhookAsync(messageStatus, payload);

            try
            {
                var activity = new Activity
                {
                    ActivityType = GetActivityTypeForStatus(messageStatus),
                    Title = GetActivityTitle(messageStatus, to),
                    Description = GetActivityDescription(messageStatus, messageSid, errorCode),
                    Details = System.Text.Json.JsonSerializer.Serialize(new { MessageSid = messageSid, Status = messageStatus, To = to, From = from, ErrorCode = errorCode, Provider = "Twilio" }),
                    ActivityDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    Source = "Twilio",
                    IsSystem = true,
                    Category = "SMS"
                };
                await _activityService.CreateAsync(activity);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create Activity for Twilio status");
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Twilio webhook");
            return Ok();
        }
    }

    [HttpPost("inbound")]
    [Consumes("application/x-www-form-urlencoded")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> HandleInboundMessage()
    {
        try
        {
            var form = await Request.ReadFormAsync();
            var messageSid = form["MessageSid"].ToString();
            var body = form["Body"].ToString();
            var from = form["From"].ToString();
            var to = form["To"].ToString();

            _logger.LogInformation("Twilio inbound: From={From}, To={To}", from, to);

            try
            {
                var activity = new Activity
                {
                    ActivityType = ActivityType.SMSReceived,
                    Title = "SMS received from " + from,
                    Description = TruncateForLog(body, 100),
                    Details = System.Text.Json.JsonSerializer.Serialize(new { MessageSid = messageSid, Body = body, From = from, To = to, Provider = "Twilio" }),
                    ActivityDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    Source = "Twilio",
                    IsSystem = true,
                    Category = "SMS"
                };
                await _activityService.CreateAsync(activity);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create Activity for inbound SMS");
            }

            return Content("<?xml version=\"1.0\" encoding=\"UTF-8\"?><Response></Response>", "application/xml");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing inbound message");
            return Content("<?xml version=\"1.0\" encoding=\"UTF-8\"?><Response></Response>", "application/xml");
        }
    }

    [HttpPost("whatsapp/status")]
    [Consumes("application/x-www-form-urlencoded")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<IActionResult> HandleWhatsAppStatus() => HandleStatusCallback();

    private static string TruncateForLog(string? text, int maxLength = 50) =>
        string.IsNullOrEmpty(text) ? "" : text.Length <= maxLength ? text : text[..maxLength] + "...";

    private static ActivityType GetActivityTypeForStatus(string status) =>
        status.ToLowerInvariant() switch
        {
            "delivered" or "sent" => ActivityType.SMSSent,
            "failed" or "undelivered" => ActivityType.SMSSent,
            "received" => ActivityType.SMSReceived,
            _ => ActivityType.Other
        };

    private static string GetActivityTitle(string status, string to) =>
        status.ToLowerInvariant() switch
        {
            "queued" => "SMS queued for " + to,
            "sending" => "SMS sending to " + to,
            "sent" => "SMS sent to " + to,
            "delivered" => "SMS delivered to " + to,
            "failed" => "SMS failed for " + to,
            "undelivered" => "SMS undelivered to " + to,
            _ => "Twilio SMS: " + status
        };

    private static string GetActivityDescription(string status, string messageSid, string? errorCode) =>
        string.IsNullOrEmpty(errorCode)
            ? "Twilio SMS " + status + " - Message ID: " + messageSid
            : "Twilio SMS " + status + " - Message ID: " + messageSid + " (Error: " + errorCode + ")";
}
