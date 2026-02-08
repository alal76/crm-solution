// CRM Solution - Pluggable Architecture
// SendGrid Webhook Controller
// Week 10: Handle email delivery status callbacks

using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Output.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace CRM.Api.Controllers.Webhooks;

/// <summary>
/// Webhook controller for SendGrid email event callbacks.
/// </summary>
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

    /// <summary>
    /// Receives email event notifications from SendGrid.
    /// </summary>
    /// <remarks>
    /// SendGrid sends JSON array with events:
    /// - processed: Message accepted by SendGrid
    /// - dropped: Message was rejected
    /// - delivered: Message was delivered
    /// - deferred: Temporary delivery failure
    /// - bounce: Permanent delivery failure
    /// - open: Recipient opened the email
    /// - click: Recipient clicked a link
    /// - spamreport: Recipient reported as spam
    /// - unsubscribe: Recipient unsubscribed
    /// </remarks>
    [HttpPost("events")]
    [Consumes("application/json")]
    public async Task<IActionResult> HandleEvents()
    {
        try
        {
            // Read JSON body
            using var reader = new StreamReader(Request.Body, Encoding.UTF8);
            var payload = await reader.ReadToEndAsync();

            _logger.LogDebug("SendGrid webhook received: {Payload}", 
                TruncateForLog(payload, 200));

            // TODO: Validate webhook signature using SendGrid Event Webhook Signature
            // var signature = Request.Headers["X-Twilio-Email-Event-Webhook-Signature"];
            // var timestamp = Request.Headers["X-Twilio-Email-Event-Webhook-Timestamp"];

            // Process each event
            var deliveryEvent = await _notificationProvider.ProcessDeliveryWebhookAsync(
                "batch", payload);

            _logger.LogInformation(
                "Processed SendGrid webhook: MessageId={Id}, Event={Event}",
                deliveryEvent.NotificationId, deliveryEvent.EventType);

            // TODO: Store delivery events in Activity timeline
            // await _activityService.CreateDeliveryActivityAsync(deliveryEvent);

            // SendGrid expects 200 OK
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing SendGrid webhook");
            // Return 200 to prevent retries
            return Ok();
        }
    }

    /// <summary>
    /// Receives inbound email parse webhook from SendGrid.
    /// </summary>
    /// <remarks>
    /// Requires SendGrid Inbound Parse to be configured.
    /// Useful for email-to-case, email-based ticket updates, etc.
    /// </remarks>
    [HttpPost("inbound")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> HandleInboundEmail()
    {
        try
        {
            var form = await Request.ReadFormAsync();
            
            var from = form["from"].ToString();
            var to = form["to"].ToString();
            var subject = form["subject"].ToString();
            var text = form["text"].ToString();
            var html = form["html"].ToString();
            var headers = form["headers"].ToString();
            var envelope = form["envelope"].ToString();

            _logger.LogInformation(
                "SendGrid inbound email: From={From}, To={To}, Subject={Subject}", 
                from, to, subject);

            // Handle attachments
            var attachments = new List<(string FileName, string ContentType, byte[] Content)>();
            foreach (var file in Request.Form.Files)
            {
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                attachments.Add((file.FileName, file.ContentType, ms.ToArray()));
            }

            if (attachments.Any())
            {
                _logger.LogInformation("Inbound email has {Count} attachments", attachments.Count);
            }

            // TODO: Route inbound email to appropriate handler
            // - Create support ticket from email
            // - Associate with existing conversation
            // - Auto-reply based on rules

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing SendGrid inbound email");
            return Ok();
        }
    }

    private static string TruncateForLog(string? text, int maxLength = 100)
    {
        if (string.IsNullOrEmpty(text)) return "";
        return text.Length <= maxLength ? text : text[..maxLength] + "...";
    }
}
