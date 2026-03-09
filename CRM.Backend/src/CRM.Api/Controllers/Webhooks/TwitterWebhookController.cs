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
/// Receives Twitter/X Account Activity API webhook events.
/// The endpoint is public (<see cref="AllowAnonymousAttribute"/>) because Twitter
/// cannot provide a JWT. CRC challenge responses use HMAC-SHA256 with the Consumer Secret.
/// Inbound event authenticity is verified via the <c>x-twitter-webhooks-signature</c> header.
/// <para>
/// COMM-003: Inbound only. Outbound DMs deferred to production (requires $100/month Twitter API tier).
/// </para>
/// </summary>
[ApiController]
[Route("api/webhooks/twitter")]
[AllowAnonymous]
public class TwitterWebhookController : CrmControllerBase
{
    private readonly TwitterMessagingOptions _options;
    private readonly ILogger<TwitterWebhookController> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="TwitterWebhookController"/>.
    /// </summary>
    public TwitterWebhookController(
        IOptions<TwitterMessagingOptions> options,
        ILogger<TwitterWebhookController> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Handles the Twitter CRC (Consumer Resource Challenge) verification challenge (GET).
    /// Twitter sends this request to verify webhook ownership. The response must be the
    /// HMAC-SHA256 of the <paramref name="crc_token"/> using the Consumer Secret as key,
    /// base64-encoded and prefixed with <c>sha256=</c>, returned as JSON.
    /// </summary>
    /// <param name="crc_token">Random token provided by Twitter to be signed and echoed back.</param>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult HandleCrcChallenge([FromQuery] string crc_token = "")
    {
        if (string.IsNullOrWhiteSpace(crc_token))
        {
            _logger.LogWarning("Twitter CRC challenge received with empty crc_token.");
            return BadRequest(new { error = "crc_token is required." });
        }

        if (string.IsNullOrWhiteSpace(_options.ConsumerSecret))
        {
            // No secret configured — cannot sign the challenge.
            // Log and return a placeholder response so the endpoint doesn't error.
            _logger.LogWarning(
                "Twitter CRC challenge received but ConsumerSecret is not configured. " +
                "COMM-003: Set Providers:Messaging:Twitter:ConsumerSecret to handle challenges.");
            return BadRequest(new { error = "Twitter ConsumerSecret is not configured." });
        }

        var key = Encoding.UTF8.GetBytes(_options.ConsumerSecret);
        var data = Encoding.UTF8.GetBytes(crc_token);

        using var hmac = new HMACSHA256(key);
        var hash = hmac.ComputeHash(data);
        var responseToken = "sha256=" + Convert.ToBase64String(hash);

        _logger.LogInformation("Twitter CRC challenge accepted and signed.");

        // Twitter requires exactly {"response_token": "sha256=<base64>"}.
        return Ok(new { response_token = responseToken });
    }

    /// <summary>
    /// Receives inbound Twitter/X Account Activity API events (POST).
    /// Validates the <c>x-twitter-webhooks-signature</c> HMAC-SHA256 header when
    /// <c>ConsumerSecret</c> is configured. Returns HTTP 200 immediately (Twitter
    /// requires a response within 10 seconds).
    /// </summary>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ReceiveEvent(CancellationToken cancellationToken)
    {
        // Buffer the body to allow both signature validation and JSON parsing.
        Request.EnableBuffering();

        byte[] rawBody;
        using (var ms = new MemoryStream())
        {
            await Request.Body.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
            rawBody = ms.ToArray();
        }

        // Validate x-twitter-webhooks-signature when ConsumerSecret is configured.
        if (!string.IsNullOrWhiteSpace(_options.ConsumerSecret))
        {
            var signatureHeader = Request.Headers["x-twitter-webhooks-signature"].ToString();
            if (!IsValidTwitterSignature(rawBody, signatureHeader, _options.ConsumerSecret))
            {
                _logger.LogWarning("Invalid x-twitter-webhooks-signature on Twitter webhook. Rejecting.");
                return StatusCode(StatusCodes.Status403Forbidden);
            }
        }

        try
        {
            var body = Encoding.UTF8.GetString(rawBody);
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            // Log the event type for observability.
            // Twitter AAA events include direct_message_events, follow_events, etc.
            var eventType = root.TryGetProperty("for_user_id", out var uid)
                ? $"for_user_id={uid.GetString()}"
                : "unknown";

            _logger.LogInformation(
                "Twitter/X inbound webhook event received. {EventType}. " +
                "COMM-003: Inbound simulation via Mockoon. Outbound requires paid API tier.",
                eventType);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse Twitter webhook payload.");
        }

        // Twitter requires a 200 OK within 10 seconds; always return OK.
        return Ok();
    }

    /// <summary>
    /// Validates the <c>x-twitter-webhooks-signature</c> header sent by Twitter.
    /// </summary>
    /// <remarks>
    /// Algorithm (per Twitter Account Activity API docs):
    /// 1. Compute HMAC-SHA256 of the raw request body using the Consumer Secret as the key.
    /// 2. Base64-encode the resulting hash.
    /// 3. Prepend <c>sha256=</c> to form the expected header value.
    /// 4. Compare to the received header value using constant-time equality.
    /// </remarks>
    public static bool IsValidTwitterSignature(
        byte[] rawBody,
        string signatureHeader,
        string consumerSecret)
    {
        if (string.IsNullOrWhiteSpace(signatureHeader)) return false;
        if (!signatureHeader.StartsWith("sha256=", StringComparison.OrdinalIgnoreCase)) return false;

        var providedBase64 = signatureHeader["sha256=".Length..];

        var key = Encoding.UTF8.GetBytes(consumerSecret);
        using var hmac = new HMACSHA256(key);
        var hash = hmac.ComputeHash(rawBody);
        var expectedBase64 = Convert.ToBase64String(hash);

        // Constant-time comparison prevents timing-oracle attacks.
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expectedBase64),
            Encoding.UTF8.GetBytes(providedBase64));
    }
}
