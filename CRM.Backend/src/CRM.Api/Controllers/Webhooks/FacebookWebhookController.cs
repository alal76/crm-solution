// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CRM.Api.Infrastructure;
using CRM.Core.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Api.Controllers.Webhooks;

/// <summary>
/// Receives Facebook Messenger webhook events from the Facebook Platform.
/// The endpoint is public (<see cref="AllowAnonymousAttribute"/>) because Facebook
/// cannot provide a JWT. Authenticity is verified via the <c>X-Hub-Signature-256</c>
/// HMAC-SHA256 header when <c>AppSecret</c> is configured.
/// </summary>
[ApiController]
[Route("api/webhooks/facebook")]
[AllowAnonymous]
public class FacebookWebhookController : CrmControllerBase
{
    private readonly FacebookMessengerOptions _options;
    private readonly ILogger<FacebookWebhookController> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="FacebookWebhookController"/>.
    /// </summary>
    public FacebookWebhookController(
        IOptions<FacebookMessengerOptions> options,
        ILogger<FacebookWebhookController> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Handles the Facebook webhook verification challenge (GET).
    /// Facebook calls this endpoint when a webhook subscription is first created or updated.
    /// </summary>
    /// <param name="mode">Must be <c>subscribe</c> for a valid verification request.</param>
    /// <param name="verifyToken">Token that must match <see cref="FacebookMessengerOptions.VerifyToken"/>.</param>
    /// <param name="challenge">Random string that must be echoed back verbatim to confirm ownership.</param>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult VerifyWebhook(
        [FromQuery(Name = "hub.mode")] string mode = "",
        [FromQuery(Name = "hub.verify_token")] string verifyToken = "",
        [FromQuery(Name = "hub.challenge")] string challenge = "")
    {
        if (mode == "subscribe" &&
            !string.IsNullOrEmpty(_options.VerifyToken) &&
            verifyToken == _options.VerifyToken)
        {
            _logger.LogInformation("Facebook webhook verification challenge accepted.");
            // Facebook requires the challenge echoed back as plain text (not JSON).
            return Content(challenge, "text/plain");
        }

        _logger.LogWarning(
            "Facebook webhook verification failed. mode={Mode}, token_match={Match}",
            mode, verifyToken == _options.VerifyToken);
        return Forbid();
    }

    /// <summary>
    /// Receives inbound Facebook Messenger events (POST).
    /// Validates the <c>X-Hub-Signature-256</c> HMAC-SHA256 header before processing.
    /// Facebook requires an HTTP 200 response within 20 seconds.
    /// </summary>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ReceiveEvent(CancellationToken cancellationToken)
    {
        // Buffer the body so we can read it for both signature validation and JSON parsing.
        Request.EnableBuffering();

        byte[] rawBody;
        using (var ms = new MemoryStream())
        {
            await Request.Body.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
            rawBody = ms.ToArray();
        }

        // Validate X-Hub-Signature-256 when AppSecret is configured.
        if (!string.IsNullOrWhiteSpace(_options.AppSecret))
        {
            var signatureHeader = Request.Headers["X-Hub-Signature-256"].ToString();
            if (!IsValidHubSignature(rawBody, signatureHeader, _options.AppSecret))
            {
                _logger.LogWarning("Invalid X-Hub-Signature-256 on Facebook webhook. Rejecting.");
                return StatusCode(StatusCodes.Status403Forbidden);
            }
        }

        try
        {
            var body = Encoding.UTF8.GetString(rawBody);
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            // Extract sender PSID and message text from entry[0].messaging[0]
            if (root.TryGetProperty("entry", out var entries) &&
                entries.GetArrayLength() > 0)
            {
                var firstEntry = entries[0];
                if (firstEntry.TryGetProperty("messaging", out var messaging) &&
                    messaging.GetArrayLength() > 0)
                {
                    var firstEvent = messaging[0];
                    var senderPsid = firstEvent
                        .TryGetProperty("sender", out var sender)
                        ? sender.TryGetProperty("id", out var sid) ? sid.GetString() : null
                        : null;

                    var messageText = firstEvent
                        .TryGetProperty("message", out var msg)
                        ? msg.TryGetProperty("text", out var txt) ? txt.GetString() : null
                        : null;

                    _logger.LogInformation(
                        "Facebook Messenger inbound: SenderPsid={Psid}, Text={Text}",
                        senderPsid ?? "(unknown)",
                        TruncateForLog(messageText, 100));
                }
            }
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse Facebook Messenger webhook payload.");
        }

        // Facebook requires a 200 OK within 20 seconds; always return OK.
        return Ok();
    }

    /// <summary>
    /// Validates the <c>X-Hub-Signature-256</c> header sent by Facebook.
    /// </summary>
    /// <remarks>
    /// Algorithm (per Facebook docs):
    /// 1. Compute HMAC-SHA256 of the raw request body using the App Secret as the key.
    /// 2. Encode the hash as a lowercase hex string prefixed with <c>sha256=</c>.
    /// 3. Compare the result to the header value using constant-time equality.
    /// </remarks>
    public static bool IsValidHubSignature(
        byte[] rawBody,
        string signatureHeader,
        string appSecret)
    {
        if (string.IsNullOrWhiteSpace(signatureHeader)) return false;
        if (!signatureHeader.StartsWith("sha256=", StringComparison.OrdinalIgnoreCase)) return false;

        var providedHex = signatureHeader["sha256=".Length..];

        var key = Encoding.UTF8.GetBytes(appSecret);
        using var hmac = new HMACSHA256(key);
        var hash = hmac.ComputeHash(rawBody);
        var expectedHex = Convert.ToHexString(hash).ToLowerInvariant();

        // Constant-time comparison prevents timing-oracle attacks.
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expectedHex),
            Encoding.UTF8.GetBytes(providedHex));
    }

    private static string TruncateForLog(string? text, int maxLength = 50)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        return text.Length <= maxLength ? text : text[..maxLength] + "...";
    }
}
