// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Input;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.Twilio;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers.Webhooks;

/// <summary>
/// Handles incoming Twilio webhook callbacks for SMS status, inbound messages, and voice
/// call status callbacks.
/// </summary>
[ApiController]
[Route("api/webhooks/twilio")]
public class TwilioWebhookController : CrmControllerBase
{
    private readonly INotificationPort _notificationProvider;
    private readonly IActivityService _activityService;
    private readonly ITwilioCallLoggingService _callLoggingService;
    private readonly TwilioConfiguration _twilioConfig;
    private readonly ILogger<TwilioWebhookController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TwilioWebhookController"/> class.
    /// </summary>
    public TwilioWebhookController(
        INotificationPort notificationProvider,
        IActivityService activityService,
        ITwilioCallLoggingService callLoggingService,
        IOptions<TwilioConfiguration> twilioOptions,
        ILogger<TwilioWebhookController> logger)
    {
        _notificationProvider = notificationProvider;
        _activityService = activityService;
        _callLoggingService = callLoggingService;
        _twilioConfig = twilioOptions?.Value ?? throw new ArgumentNullException(nameof(twilioOptions));
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
            await _notificationProvider.ProcessDeliveryWebhookAsync(messageStatus, payload);

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

    /// <summary>
    /// Handles Twilio's voice call status callback (fired for events such as
    /// <c>queued</c>, <c>ringing</c>, <c>in-progress</c>, <c>completed</c>, <c>busy</c>,
    /// <c>failed</c>, <c>no-answer</c> and <c>canceled</c>) and logs/updates the call via
    /// <see cref="ITwilioCallLoggingService"/>.
    /// Authenticity is verified via the <c>X-Twilio-Signature</c> HMAC-SHA1 header when a
    /// Twilio <c>AuthToken</c> is configured, using the same algorithm as
    /// <see cref="WhatsAppWebhookController.IsValidTwilioSignature"/>.
    /// </summary>
    [HttpPost("call-status")]
    [Consumes("application/x-www-form-urlencoded")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> HandleCallStatusCallback()
    {
        try
        {
            var form = await Request.ReadFormAsync();

            // Validate Twilio signature when AuthToken is configured.
            // If AuthToken is absent (e.g. development), validation is skipped.
            if (!string.IsNullOrWhiteSpace(_twilioConfig.AuthToken))
            {
                var twilioSignature = Request.Headers["X-Twilio-Signature"].ToString();
                var webhookUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";

                if (!WhatsAppWebhookController.IsValidTwilioSignature(twilioSignature, webhookUrl, form, _twilioConfig.AuthToken))
                {
                    _logger.LogWarning("Invalid Twilio signature on call-status webhook. Rejecting.");
                    return StatusCode(StatusCodes.Status403Forbidden);
                }
            }

            var callSid = form["CallSid"].ToString();
            if (string.IsNullOrWhiteSpace(callSid))
            {
                _logger.LogWarning("Twilio call-status webhook received without a CallSid; ignoring");
                return Ok();
            }

            var callStatus = form["CallStatus"].ToString();
            var from = form["From"].ToString();
            var to = form["To"].ToString();
            var direction = form["Direction"].ToString();
            var durationRaw = form["CallDuration"].ToString();
            var duration = int.TryParse(durationRaw, out var parsedDuration) ? parsedDuration : (int?)null;

            _logger.LogInformation(
                "Twilio call-status: Sid={Sid}, Status={Status}, From={From}, To={To}, Direction={Direction}",
                callSid, callStatus, from, to, direction);

            try
            {
                // The first callback for a call is "queued" (outbound) or "ringing" (inbound);
                // log a new call record then. Every subsequent status is an update to that record.
                if (string.Equals(callStatus, "queued", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(callStatus, "ringing", StringComparison.OrdinalIgnoreCase))
                {
                    var callEvent = new TwilioCallEvent
                    {
                        CallSid = callSid,
                        From = from,
                        To = to,
                        Direction = direction,
                        Status = callStatus,
                        Duration = duration,
                        Timestamp = DateTime.UtcNow
                    };

                    if (direction.StartsWith("inbound", StringComparison.OrdinalIgnoreCase))
                    {
                        await _callLoggingService.LogInboundCallAsync(callEvent);
                    }
                    else
                    {
                        await _callLoggingService.LogOutboundCallAsync(callEvent);
                    }
                }
                else
                {
                    await _callLoggingService.UpdateCallStatusAsync(callSid, callStatus, duration);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to log/update Twilio call {CallSid}", callSid);
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Twilio call-status webhook");
            return Ok();
        }
    }

    private static string TruncateForLog(string? text, int maxLength = 50)
    {
        if (string.IsNullOrEmpty(text)) return "";
        return text.Length <= maxLength ? text : text[..maxLength] + "...";
    }

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
