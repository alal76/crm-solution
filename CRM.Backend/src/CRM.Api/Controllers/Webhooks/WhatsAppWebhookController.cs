// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Cryptography;
using System.Text;
using CRM.Api.Infrastructure;
using CRM.Core.Configuration;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Api.Controllers.Webhooks;

/// <summary>
/// Receives inbound WhatsApp messages forwarded by the Twilio WhatsApp Sandbox.
/// The endpoint is public (<see cref="AllowAnonymousAttribute"/>) because Twilio
/// cannot provide a JWT. Authenticity is verified via the <c>X-Twilio-Signature</c>
/// HMAC-SHA1 header when <c>AuthToken</c> is configured.
/// </summary>
[ApiController]
[Route("api/webhooks/whatsapp")]
[AllowAnonymous]
public class WhatsAppWebhookController : CrmControllerBase
{
    private readonly WhatsAppOptions _options;
    private readonly IActivityService _activityService;
    private readonly ILogger<WhatsAppWebhookController> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="WhatsAppWebhookController"/>.
    /// </summary>
    public WhatsAppWebhookController(
        IOptions<WhatsAppOptions> options,
        IActivityService activityService,
        ILogger<WhatsAppWebhookController> logger)
    {
        _options = options.Value;
        _activityService = activityService;
        _logger = logger;
    }

    /// <summary>
    /// Handles an inbound WhatsApp message from the Twilio WhatsApp Sandbox.
    /// </summary>
    /// <param name="From">Sender WhatsApp number (e.g. <c>whatsapp:+15551234567</c>).</param>
    /// <param name="Body">Message text body.</param>
    /// <param name="To">Receiving number (the sandbox From number).</param>
    /// <param name="MessageSid">Twilio message SID.</param>
    /// <param name="NumMedia">Number of attached media files.</param>
    [HttpPost]
    [Consumes("application/x-www-form-urlencoded")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ReceiveMessage(
        [FromForm] string From = "",
        [FromForm] string Body = "",
        [FromForm] string To = "",
        [FromForm] string MessageSid = "",
        [FromForm] string NumMedia = "0")
    {
        // Validate Twilio signature when AuthToken is configured.
        // If AuthToken is absent (e.g. development), validation is skipped.
        if (!string.IsNullOrWhiteSpace(_options.AuthToken))
        {
            var twilioSignature = Request.Headers["X-Twilio-Signature"].ToString();
            var webhookUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";

            if (!IsValidTwilioSignature(twilioSignature, webhookUrl, Request.Form, _options.AuthToken))
            {
                _logger.LogWarning(
                    "Invalid Twilio signature on WhatsApp webhook from {From}. Rejecting.", From);
                return StatusCode(StatusCodes.Status403Forbidden);
            }
        }

        _logger.LogInformation(
            "WhatsApp inbound: From={From}, To={To}, Sid={Sid}, NumMedia={NumMedia}",
            From, To, MessageSid, NumMedia);

        try
        {
            var activity = new Activity
            {
                ActivityType = ActivityType.ChatMessage,
                Title = $"WhatsApp received from {From}",
                Description = TruncateForLog(Body, 100),
                Details = System.Text.Json.JsonSerializer.Serialize(new
                {
                    MessageSid,
                    Body,
                    From,
                    To,
                    NumMedia,
                    Provider = "WhatsApp"
                }),
                ActivityDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                Source = "WhatsApp",
                IsSystem = true,
                Category = "WhatsApp"
            };

            await _activityService.CreateAsync(activity);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create Activity for inbound WhatsApp message from {From}", From);
        }

        // Return empty TwiML response — Twilio requires a valid XML reply.
        return Content(
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?><Response></Response>",
            "application/xml");
    }

    /// <summary>
    /// Validates a Twilio webhook request using HMAC-SHA1.
    /// </summary>
    /// <remarks>
    /// Algorithm (per Twilio docs):
    /// 1. Start with the full webhook URL.
    /// 2. Sort all POST parameters alphabetically by key.
    /// 3. Append each key+value pair (no delimiter) to the URL.
    /// 4. Compute HMAC-SHA1 of the resulting string using <paramref name="authToken"/> as key.
    /// 5. Base64-encode the hash and compare to <paramref name="signature"/> using constant-time equality.
    /// </remarks>
    public static bool IsValidTwilioSignature(
        string signature,
        string url,
        IFormCollection form,
        string authToken)
    {
        if (string.IsNullOrWhiteSpace(signature)) return false;

        // Build string to sign: URL + sorted key=value (no separators)
        var sortedParams = form
            .OrderBy(kvp => kvp.Key, StringComparer.Ordinal)
            .Select(kvp => kvp.Key + kvp.Value.ToString());

        var stringToSign = url + string.Concat(sortedParams);

        var key = Encoding.UTF8.GetBytes(authToken);
        var data = Encoding.UTF8.GetBytes(stringToSign);

        using var hmac = new HMACSHA1(key);
        var hash = hmac.ComputeHash(data);
        var expected = Convert.ToBase64String(hash);

        // Constant-time comparison prevents timing-oracle attacks.
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected),
            Encoding.UTF8.GetBytes(signature));
    }

    private static string TruncateForLog(string? text, int maxLength = 50)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        return text.Length <= maxLength ? text : text[..maxLength] + "...";
    }
}
