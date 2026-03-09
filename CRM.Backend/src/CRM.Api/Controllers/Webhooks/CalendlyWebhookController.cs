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
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Api.Controllers.Webhooks;

/// <summary>
/// Receives Calendly webhook events and creates CRM <see cref="Interaction"/> records
/// for meeting bookings and cancellations.
/// Route is public (<see cref="AllowAnonymousAttribute"/>) — authenticity verified
/// via HMAC-SHA256 signature when <c>WebhookSigningKey</c> is configured.
/// Implements INT-004.
/// </summary>
[ApiController]
[Route("api/webhooks/calendly")]
[AllowAnonymous] // NOSONAR - S4834: Calendly webhook; HMAC-SHA256 signature verified in handler
public class CalendlyWebhookController : CrmControllerBase
{
    private readonly CalendlyOptions _options;
    private readonly IInteractionService _interactionService;
    private readonly ICrmDbContext _db;
    private readonly ILogger<CalendlyWebhookController> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="CalendlyWebhookController"/>.
    /// </summary>
    public CalendlyWebhookController(
        IOptions<CalendlyOptions> options,
        IInteractionService interactionService,
        ICrmDbContext db,
        ILogger<CalendlyWebhookController> logger)
    {
        _options = options.Value;
        _interactionService = interactionService;
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Handles an inbound Calendly webhook event (<c>invitee.created</c> or <c>invitee.canceled</c>).
    /// </summary>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ReceiveEvent(CancellationToken ct)
    {
        // Buffer the body so it can be used for both signature validation and JSON parsing.
        Request.EnableBuffering();

        byte[] rawBytes;
        using (var ms = new MemoryStream())
        {
            await Request.Body.CopyToAsync(ms, ct);
            rawBytes = ms.ToArray();
        }

        var rawBody = Encoding.UTF8.GetString(rawBytes);

        // Validate Calendly-Webhook-Signature when signing key is configured.
        // Skip validation when key is absent (development convenience).
        if (!string.IsNullOrWhiteSpace(_options.WebhookSigningKey))
        {
            var sigHeader = Request.Headers["Calendly-Webhook-Signature"].ToString();
            if (!IsValidCalendlySignature(sigHeader, rawBody, _options.WebhookSigningKey))
            {
                _logger.LogWarning(
                    "Invalid Calendly webhook signature on request from {RemoteIp}.",
                    HttpContext.Connection.RemoteIpAddress);
                return StatusCode(StatusCodes.Status403Forbidden);
            }
        }

        try
        {
            using var doc = JsonDocument.Parse(rawBody);
            var root = doc.RootElement;

            var eventType = root.TryGetProperty("event", out var evtProp)
                ? evtProp.GetString() ?? string.Empty
                : string.Empty;

            if (!root.TryGetProperty("payload", out var payload))
            {
                _logger.LogWarning("Calendly webhook received without 'payload' field. Ignoring.");
                return Ok(new { received = true });
            }

            // Extract meeting details from payload.event
            string? eventName = null;
            DateTime? startTime = null;
            DateTime? endTime = null;
            string? joinUrl = null;

            if (payload.TryGetProperty("event", out var eventObj))
            {
                if (eventObj.TryGetProperty("name", out var n))
                    eventName = n.GetString();

                if (eventObj.TryGetProperty("start_time", out var s) &&
                    DateTime.TryParse(s.GetString(), null,
                        System.Globalization.DateTimeStyles.RoundtripKind, out var st))
                    startTime = st;

                if (eventObj.TryGetProperty("end_time", out var e) &&
                    DateTime.TryParse(e.GetString(), null,
                        System.Globalization.DateTimeStyles.RoundtripKind, out var et))
                    endTime = et;

                if (eventObj.TryGetProperty("location", out var loc) &&
                    loc.TryGetProperty("join_url", out var j))
                    joinUrl = j.GetString();
            }

            // Extract invitee details from payload.invitee
            string? inviteeName = null;
            string? inviteeEmail = null;

            if (payload.TryGetProperty("invitee", out var invitee))
            {
                if (invitee.TryGetProperty("name", out var n)) inviteeName = n.GetString();
                if (invitee.TryGetProperty("email", out var e)) inviteeEmail = e.GetString();
            }

            _logger.LogInformation(
                "Calendly webhook received: event={EventType}, meeting={MeetingName}, invitee={InviteeEmail}",
                eventType, eventName, inviteeEmail);

            if (string.Equals(eventType, "invitee.created", StringComparison.OrdinalIgnoreCase))
            {
                await HandleInviteeCreatedAsync(
                    eventName, inviteeName, inviteeEmail, startTime, endTime, joinUrl, ct);
            }
            else if (string.Equals(eventType, "invitee.canceled", StringComparison.OrdinalIgnoreCase))
            {
                await HandleInviteeCanceledAsync(eventName, inviteeEmail, startTime, ct);
            }
            else
            {
                _logger.LogInformation(
                    "Calendly webhook event type '{EventType}' is not handled.", eventType);
            }
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Unable to parse Calendly webhook JSON payload.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing Calendly webhook event.");
        }

        return Ok(new { received = true });
    }

    private async Task HandleInviteeCreatedAsync(
        string? eventName,
        string? inviteeName,
        string? inviteeEmail,
        DateTime? startTime,
        DateTime? endTime,
        string? joinUrl,
        CancellationToken ct)
    {
        // Attempt to link to an existing CRM contact by email
        int? contactId = null;
        if (!string.IsNullOrWhiteSpace(inviteeEmail))
        {
            try
            {
                var contact = await _db.Contacts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.EmailPrimary == inviteeEmail, ct);
                contactId = contact?.Id;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Could not look up CRM contact by email {Email}. Proceeding without link.",
                    inviteeEmail);
            }
        }

        var subject = $"Calendly: {eventName ?? "Meeting"}";
        if (subject.Length > 500) subject = subject[..500];

        var descParts = new StringBuilder();
        descParts.Append($"Meeting with {inviteeName ?? "Unknown"} ({inviteeEmail ?? "N/A"})");
        if (!string.IsNullOrWhiteSpace(joinUrl))
            descParts.Append($"\nJoin URL: {joinUrl}");

        var description = descParts.ToString();
        if (description.Length > 10000) description = description[..10000];

        int? durationMinutes = null;
        if (startTime.HasValue && endTime.HasValue)
        {
            durationMinutes = (int)Math.Clamp(
                (endTime.Value - startTime.Value).TotalMinutes, 0, 1440);
        }

        var interaction = new Interaction
        {
            InteractionType = InteractionType.Meeting,
            Type = "Meeting",
            Direction = InteractionDirection.Inbound,
            Subject = subject,
            Description = description,
            InteractionDate = startTime.HasValue
                ? DateTime.SpecifyKind(startTime.Value, DateTimeKind.Utc)
                : DateTime.UtcNow,
            EndTime = endTime.HasValue
                ? DateTime.SpecifyKind(endTime.Value, DateTimeKind.Utc)
                : null,
            DurationMinutes = durationMinutes,
            MeetingLink = joinUrl?.Length > 1000 ? joinUrl[..1000] : joinUrl,
            EmailAddress = inviteeEmail?.Length > 200 ? inviteeEmail[..200] : inviteeEmail,
            ContactId = contactId,
            IsCompleted = false,
            Outcome = InteractionOutcome.None,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _interactionService.CreateAsync(interaction);

        _logger.LogInformation(
            "Created Interaction {Id} for Calendly booking '{Subject}' (ContactId={ContactId}).",
            created.Id, created.Subject, contactId);
    }

    private async Task HandleInviteeCanceledAsync(
        string? eventName,
        string? inviteeEmail,
        DateTime? startTime,
        CancellationToken ct)
    {
        // Search by time window ±5 min around the booked start time
        var from = startTime.HasValue ? startTime.Value.AddMinutes(-5) : DateTime.UtcNow.AddMinutes(-5);
        var to = startTime.HasValue ? startTime.Value.AddMinutes(5) : DateTime.UtcNow.AddMinutes(5);

        var candidates = await _interactionService.GetInteractionsAsync(
            interactionType: InteractionType.Meeting,
            fromDate: from,
            toDate: to);

        var interaction = candidates.FirstOrDefault(i =>
            !string.IsNullOrWhiteSpace(inviteeEmail) &&
            string.Equals(i.EmailAddress, inviteeEmail, StringComparison.OrdinalIgnoreCase) &&
            i.Subject.StartsWith("Calendly:", StringComparison.OrdinalIgnoreCase));

        if (interaction is null)
        {
            _logger.LogWarning(
                "Calendly cancellation: no matching Interaction found " +
                "for email={Email}, meeting={EventName}.",
                inviteeEmail, eventName);
            return;
        }

        interaction.Outcome = InteractionOutcome.Cancelled;
        interaction.IsCompleted = true;
        interaction.UpdatedAt = DateTime.UtcNow;

        await _interactionService.UpdateAsync(interaction.Id, interaction);

        _logger.LogInformation(
            "Interaction {Id} marked Cancelled for Calendly cancellation: '{Subject}'.",
            interaction.Id, interaction.Subject);
    }

    /// <summary>
    /// Validates a Calendly HMAC-SHA256 webhook signature.
    /// </summary>
    /// <remarks>
    /// Algorithm (per Calendly docs):
    /// <list type="number">
    ///   <item>Parse <paramref name="header"/>: <c>t={timestamp},v1={base64-hmac}</c></item>
    ///   <item>Construct the message: <c>{timestamp}.{rawBody}</c></item>
    ///   <item>Compute HMAC-SHA256 of the message using <paramref name="signingKey"/>.</item>
    ///   <item>Base64-encode the hash and compare to <c>v1</c> using constant-time equality.</item>
    /// </list>
    /// </remarks>
    public static bool IsValidCalendlySignature(
        string header, string rawBody, string signingKey)
    {
        if (string.IsNullOrWhiteSpace(header)) return false;

        string? timestamp = null;
        string? v1Sig = null;

        foreach (var part in header.Split(','))
        {
            if (part.StartsWith("t=", StringComparison.Ordinal))
                timestamp = part[2..];
            else if (part.StartsWith("v1=", StringComparison.Ordinal))
                v1Sig = part[3..];
        }

        if (timestamp is null || v1Sig is null) return false;

        var message = $"{timestamp}.{rawBody}";
        var key = Encoding.UTF8.GetBytes(signingKey);
        var data = Encoding.UTF8.GetBytes(message);

        using var hmac = new HMACSHA256(key);
        var expected = Convert.ToBase64String(hmac.ComputeHash(data));

        // Constant-time comparison prevents timing-oracle attacks.
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected),
            Encoding.UTF8.GetBytes(v1Sig));
    }
}
