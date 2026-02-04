// CRM Solution - Pluggable Architecture
// Twilio Webhook Controller
// Week 10: Handle SMS delivery status callbacks

using CRM.Core.Ports.Output.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CRM.Api.Controllers.Webhooks;

/// <summary>
/// Webhook controller for Twilio SMS delivery status callbacks.
/// </summary>
[ApiController]
[Route("api/webhooks/twilio")]
public class TwilioWebhookController : ControllerBase
{
    private readonly INotificationPort _notificationProvider;
    private readonly ILogger<TwilioWebhookController> _logger;

    public TwilioWebhookController(
        INotificationPort notificationProvider,
        ILogger<TwilioWebhookController> logger)
    {
        _notificationProvider = notificationProvider;
        _logger = logger;
    }

    /// <summary>
    /// Receives SMS delivery status updates from Twilio.
    /// </summary>
    /// <remarks>
    /// Twilio sends form-urlencoded data with these fields:
    /// - MessageSid: Unique message identifier
    /// - MessageStatus: queued, sending, sent, delivered, undelivered, failed
    /// - To: Recipient phone number
    /// - From: Sender phone number
    /// - ErrorCode: Error code (if failed)
    /// - ErrorMessage: Error description (if failed)
    /// </remarks>
    [HttpPost("status")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> HandleStatusCallback()
    {
        try
        {
            // Read form data
            var form = await Request.ReadFormAsync();
            var messageSid = form["MessageSid"].ToString();
            var messageStatus = form["MessageStatus"].ToString();
            var to = form["To"].ToString();
            var from = form["From"].ToString();
            var errorCode = form["ErrorCode"].ToString();
            var errorMessage = form["ErrorMessage"].ToString();

            _logger.LogInformation(
                "Twilio status callback: MessageSid={Sid}, Status={Status}, To={To}", 
                messageSid, messageStatus, to);

            // Build payload for processing
            var payload = string.Join("&", form.Select(kvp => 
                $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value.ToString())}"));

            // Process the webhook
            var deliveryEvent = await _notificationProvider.ProcessDeliveryWebhookAsync(
                messageStatus, payload);

            _logger.LogDebug(
                "Processed Twilio webhook: MessageId={Id}, Event={Event}",
                deliveryEvent.NotificationId, deliveryEvent.EventType);

            // TODO: Store delivery event in Activity timeline
            // await _activityService.CreateDeliveryActivityAsync(deliveryEvent);

            // Twilio expects a 200 OK response
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Twilio webhook");
            // Still return 200 to prevent Twilio from retrying
            return Ok();
        }
    }

    /// <summary>
    /// Receives inbound SMS messages from Twilio.
    /// </summary>
    [HttpPost("inbound")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> HandleInboundMessage()
    {
        try
        {
            var form = await Request.ReadFormAsync();
            var messageSid = form["MessageSid"].ToString();
            var body = form["Body"].ToString();
            var from = form["From"].ToString();
            var to = form["To"].ToString();

            _logger.LogInformation(
                "Twilio inbound SMS: From={From}, To={To}, Body={Body}", 
                from, to, TruncateForLog(body));

            // TODO: Route inbound message to appropriate handler
            // - Auto-reply
            // - Create support ticket
            // - Log as activity

            // Return TwiML response (empty for now)
            return Content("<?xml version=\"1.0\" encoding=\"UTF-8\"?><Response></Response>", 
                "application/xml");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing inbound Twilio message");
            return Content("<?xml version=\"1.0\" encoding=\"UTF-8\"?><Response></Response>", 
                "application/xml");
        }
    }

    /// <summary>
    /// Receives WhatsApp delivery status updates.
    /// </summary>
    [HttpPost("whatsapp/status")]
    [Consumes("application/x-www-form-urlencoded")]
    public Task<IActionResult> HandleWhatsAppStatus()
    {
        // Same format as SMS status callback
        return HandleStatusCallback();
    }

    private static string TruncateForLog(string? text, int maxLength = 50)
    {
        if (string.IsNullOrEmpty(text)) return "";
        return text.Length <= maxLength ? text : text[..maxLength] + "...";
    }
}
