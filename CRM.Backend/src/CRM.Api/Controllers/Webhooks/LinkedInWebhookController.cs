// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
// COMM-004: Inbound only. Outbound messaging deferred to production (requires LinkedIn Sales Navigator).
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
/// Receives LinkedIn webhook events (e.g. inbound message notifications).
/// The endpoint is public (<see cref="AllowAnonymousAttribute"/>) because LinkedIn
/// cannot provide a JWT. Authenticity is verified via the <c>x-li-signature</c>
/// HMAC-SHA256 header when <c>ClientSecret</c> is configured.
/// <para>
/// COMM-004: Inbound only. Outbound messaging deferred to production
/// (requires LinkedIn Sales Navigator at $1,600+/year).
/// </para>
/// </summary>
[ApiController]
[Route("api/webhooks/linkedin")]
[AllowAnonymous]
public class LinkedInWebhookController : CrmControllerBase
{
    private readonly LinkedInMessagingOptions _options;
    private readonly ILogger<LinkedInWebhookController> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="LinkedInWebhookController"/>.
    /// </summary>
    public LinkedInWebhookController(
        IOptions<LinkedInMessagingOptions> options,
        ILogger<LinkedInWebhookController> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Receives inbound LinkedIn webhook events (POST).
    /// Validates the <c>x-li-signature</c> HMAC-SHA256 header when
    /// <c>ClientSecret</c> is configured. Returns HTTP 200 immediately.
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

        // Validate x-li-signature when ClientSecret is configured.
        if (!string.IsNullOrWhiteSpace(_options.ClientSecret))
        {
            var signatureHeader = Request.Headers["x-li-signature"].ToString();
            if (!IsValidLinkedInSignature(rawBody, signatureHeader, _options.ClientSecret))
            {
                _logger.LogWarning("Invalid x-li-signature on LinkedIn webhook. Rejecting.");
                return StatusCode(StatusCodes.Status403Forbidden);
            }
        }

        try
        {
            var body = Encoding.UTF8.GetString(rawBody);
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            // Log the event type for observability.
            var eventType = root.TryGetProperty("eventType", out var et)
                ? et.GetString() ?? "unknown"
                : "unknown";

            _logger.LogInformation(
                "LinkedIn inbound webhook event received. EventType={EventType}. " +
                "COMM-004: Inbound simulation via Mockoon. Outbound requires Sales Navigator ($1,600+/year).",
                eventType);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse LinkedIn webhook payload.");
        }

        // LinkedIn requires a 200 OK; always return OK.
        return Ok();
    }

    /// <summary>
    /// Validates the <c>x-li-signature</c> header sent by LinkedIn.
    /// </summary>
    /// <remarks>
    /// Algorithm (per LinkedIn webhook docs):
    /// 1. Compute HMAC-SHA256 of the raw request body using the Client Secret as the key.
    /// 2. Base64-encode the resulting hash.
    /// 3. Compare to the received header value using constant-time equality.
    /// </remarks>
    public static bool IsValidLinkedInSignature(
        byte[] rawBody,
        string signatureHeader,
        string clientSecret)
    {
        if (string.IsNullOrWhiteSpace(signatureHeader)) return false;

        var key = Encoding.UTF8.GetBytes(clientSecret);
        using var hmac = new HMACSHA256(key);
        var hash = hmac.ComputeHash(rawBody);
        var expectedBase64 = Convert.ToBase64String(hash);

        // Constant-time comparison prevents timing-oracle attacks.
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expectedBase64),
            Encoding.UTF8.GetBytes(signatureHeader));
    }
}
