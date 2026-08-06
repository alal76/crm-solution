// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// Unit Tests: CalendlyWebhookController
// Verified from source before writing:
//   Class: CalendlyWebhookController, Namespace: CRM.Api.Controllers.Webhooks
//   Base: CrmControllerBase (inherits ControllerBase)
//   Constructor: (IOptions<CalendlyOptions>, IInteractionService, ICrmDbContext,
//                 ILogger<CalendlyWebhookController>)
//   POST /api/webhooks/calendly:
//     Reads raw body bytes via CopyToAsync; validates Calendly-Webhook-Signature (HMAC-SHA256)
//     when WebhookSigningKey is configured. Header format: t={ts},v1={base64-hmac}
//     On invalid sig (and key configured): StatusCode(403)
//     On valid sig OR no key: parses JSON, dispatches to Create/Cancel handlers
//     Returns Ok({received:true}) on success
//   IsValidCalendlySignature: public static, testable directly
//     Algorithm: Base64(HMACSHA256(key=signingKey, msg="{t}.{rawBody}"))
//     Constant-time comparison via CryptographicOperations.FixedTimeEquals
//   invitee.created → IInteractionService.CreateAsync(Interaction{Type=Meeting})
//   invitee.canceled → GetInteractionsAsync(time-window) + UpdateAsync to Cancelled
using System.Security.Cryptography;
using System.Text;
using CRM.Api.Controllers.Webhooks;
using CRM.Core.Configuration;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for <see cref="CalendlyWebhookController"/>.
/// </summary>
public class CalendlyWebhookControllerTests
{
    // ── Helpers ──────────────────────────────────────────────────────────────

    private static CalendlyWebhookController BuildController(
        CalendlyOptions options,
        Mock<IInteractionService>? interactionMock = null,
        Mock<ICrmDbContext>? dbMock = null)
    {
        var optionsMock = Options.Create(options);
        var interaction = interactionMock ?? new Mock<IInteractionService>();
        var db = dbMock ?? BuildDefaultDbMock();
        var logger = new Mock<ILogger<CalendlyWebhookController>>();
        return new CalendlyWebhookController(optionsMock, interaction.Object, db.Object, logger.Object);
    }

    private static Mock<ICrmDbContext> BuildDefaultDbMock()
    {
        var dbMock = new Mock<ICrmDbContext>();
        var emptyContacts = new List<CRM.Core.Models.Contact>();
        var mockContactSet = MockDbSetFactory.CreateMockDbSet(emptyContacts);
        dbMock.Setup(d => d.Contacts).Returns(mockContactSet.Object);
        return dbMock;
    }

    private static DefaultHttpContext BuildHttpContextWithBody(
        byte[] body,
        string? signatureHeader = null)
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Body = new MemoryStream(body);
        ctx.Request.ContentLength = body.Length;
        ctx.Request.ContentType = "application/json";

        if (signatureHeader != null)
            ctx.Request.Headers["Calendly-Webhook-Signature"] = signatureHeader;

        return ctx;
    }

    /// <summary>Computes the expected Calendly HMAC-SHA256 signature for a given body and key.</summary>
    private static string ComputeCalendlySignature(string rawBody, string signingKey, string? timestamp = null)
    {
        var ts = timestamp ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var message = $"{ts}.{rawBody}";
        var key = Encoding.UTF8.GetBytes(signingKey);
        var data = Encoding.UTF8.GetBytes(message);
        using var hmac = new HMACSHA256(key);
        var hash = Convert.ToBase64String(hmac.ComputeHash(data));
        return $"t={ts},v1={hash}";
    }

    private static string BuildInviteeCreatedJson(
        string eventName = "30 Minute Meeting",
        string inviteeName = "Jane Doe",
        string inviteeEmail = "jane@example.com",
        string startTime = "2026-03-15T10:00:00Z",
        string endTime = "2026-03-15T10:30:00Z",
        string? joinUrl = "https://zoom.us/j/123456")
    {
        var location = joinUrl != null
            ? $", \"location\": {{ \"join_url\": \"{joinUrl}\" }}"
            : string.Empty;

        return $$"""
            {
              "event": "invitee.created",
              "payload": {
                "event": {
                  "name": "{{eventName}}",
                  "start_time": "{{startTime}}",
                  "end_time": "{{endTime}}"
                  {{location}}
                },
                "invitee": {
                  "name": "{{inviteeName}}",
                  "email": "{{inviteeEmail}}"
                }
              }
            }
            """;
    }

    private static string BuildInviteeCanceledJson(
        string inviteeEmail = "jane@example.com",
        string startTime = "2026-03-15T10:00:00Z")
    {
        return $$"""
            {
              "event": "invitee.canceled",
              "payload": {
                "event": {
                  "name": "30 Minute Meeting",
                  "start_time": "{{startTime}}",
                  "end_time": "2026-03-15T10:30:00Z"
                },
                "invitee": {
                  "name": "Jane Doe",
                  "email": "{{inviteeEmail}}"
                }
              }
            }
            """;
    }

    // ── IsValidCalendlySignature (static) ────────────────────────────────────

    [Fact]
    public void IsValidCalendlySignature_ReturnsFalse_WhenHeaderIsEmpty()
    {
        var result = CalendlyWebhookController.IsValidCalendlySignature(
            string.Empty, "{\"event\":\"invitee.created\"}", "signingkey");
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidCalendlySignature_ReturnsFalse_WhenHeaderMissingV1()
    {
        var result = CalendlyWebhookController.IsValidCalendlySignature(
            "t=1234567890", "{\"event\":\"invitee.created\"}", "signingkey");
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidCalendlySignature_ReturnsFalse_WhenSignatureIsWrong()
    {
        var result = CalendlyWebhookController.IsValidCalendlySignature(
            "t=1234567890,v1=AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA==",
            "{\"event\":\"invitee.created\"}",
            "signingkey");
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidCalendlySignature_ReturnsTrue_WhenSignatureIsCorrect()
    {
        const string signingKey = "test_signing_key";
        const string rawBody = "{\"event\":\"invitee.created\"}";
        const string ts = "1741680000";

        var message = $"{ts}.{rawBody}";
        var key = Encoding.UTF8.GetBytes(signingKey);
        var data = Encoding.UTF8.GetBytes(message);
        using var hmac = new HMACSHA256(key);
        var expected = Convert.ToBase64String(hmac.ComputeHash(data));
        var header = $"t={ts},v1={expected}";

        var result = CalendlyWebhookController.IsValidCalendlySignature(header, rawBody, signingKey);
        result.Should().BeTrue();
    }

    // ── POST — signature validation ──────────────────────────────────────────

    [Fact]
    public async Task Post_Returns403_WhenInvalidSignature_AndKeyConfigured()
    {
        // Arrange
        var options = new CalendlyOptions { WebhookSigningKey = "real_signing_key" };
        var controller = BuildController(options);

        var body = Encoding.UTF8.GetBytes("{\"event\":\"invitee.created\",\"payload\":{}}");
        var ctx = BuildHttpContextWithBody(body, signatureHeader: "t=1234567890,v1=AAAAAAAAAAAA==");
        controller.ControllerContext = new ControllerContext { HttpContext = ctx };

        // Act
        var result = await controller.ReceiveEvent(CancellationToken.None);

        // Assert
        result.Should().BeOfType<StatusCodeResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task Post_Returns200_WhenKeyNotConfigured()
    {
        // Arrange — when no signing key is set, skip validation (dev convenience)
        var options = new CalendlyOptions { WebhookSigningKey = string.Empty };
        var interactionMock = new Mock<IInteractionService>();
        interactionMock
            .Setup(s => s.CreateAsync(It.IsAny<Interaction>()))
            .ReturnsAsync(new Interaction { Id = 1, Subject = "Calendly: Meeting" });

        var controller = BuildController(options, interactionMock);

        var json = BuildInviteeCreatedJson();
        var body = Encoding.UTF8.GetBytes(json);
        var ctx = BuildHttpContextWithBody(body, signatureHeader: null);
        controller.ControllerContext = new ControllerContext { HttpContext = ctx };

        // Act
        var result = await controller.ReceiveEvent(CancellationToken.None);

        // Assert — 200 even though no signature header present
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Post_Returns200_WhenValidSignature()
    {
        // Arrange
        const string signingKey = "my_test_key";
        var options = new CalendlyOptions { WebhookSigningKey = signingKey };
        var interactionMock = new Mock<IInteractionService>();
        interactionMock
            .Setup(s => s.CreateAsync(It.IsAny<Interaction>()))
            .ReturnsAsync(new Interaction { Id = 5, Subject = "Calendly: 30 Minute Meeting" });

        var controller = BuildController(options, interactionMock);

        var json = BuildInviteeCreatedJson();
        var body = Encoding.UTF8.GetBytes(json);
        var sigHeader = ComputeCalendlySignature(json, signingKey, "1741680000");

        var ctx = BuildHttpContextWithBody(body, signatureHeader: sigHeader);
        controller.ControllerContext = new ControllerContext { HttpContext = ctx };

        // Act
        var result = await controller.ReceiveEvent(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    // ── POST — invitee.created ────────────────────────────────────────────────

    [Fact]
    public async Task Post_CreatesInteraction_WhenInviteeCreatedEvent()
    {
        // Arrange
        var options = new CalendlyOptions { WebhookSigningKey = string.Empty };
        var interactionMock = new Mock<IInteractionService>();

        Interaction? capturedInteraction = null;
        interactionMock
            .Setup(s => s.CreateAsync(It.IsAny<Interaction>()))
            .Callback<Interaction>(i => capturedInteraction = i)
            .ReturnsAsync(new Interaction { Id = 10, Subject = "Calendly: 30 Minute Meeting" });

        var controller = BuildController(options, interactionMock);

        var json = BuildInviteeCreatedJson(
            eventName: "30 Minute Meeting",
            inviteeName: "Jane Doe",
            inviteeEmail: "jane@example.com",
            startTime: "2026-03-15T10:00:00Z",
            endTime: "2026-03-15T10:30:00Z",
            joinUrl: "https://zoom.us/j/123456");
        var body = Encoding.UTF8.GetBytes(json);
        var ctx = BuildHttpContextWithBody(body);
        controller.ControllerContext = new ControllerContext { HttpContext = ctx };

        // Act
        var result = await controller.ReceiveEvent(CancellationToken.None);

        // Assert response
        result.Should().BeOfType<OkObjectResult>();

        // Assert interaction was created with correct values
        interactionMock.Verify(s => s.CreateAsync(It.IsAny<Interaction>()), Times.Once);
        capturedInteraction.Should().NotBeNull();
        capturedInteraction!.InteractionType.Should().Be(InteractionType.Meeting);
        capturedInteraction.Subject.Should().Be("Calendly: 30 Minute Meeting");
        capturedInteraction.EmailAddress.Should().Be("jane@example.com");
        capturedInteraction.IsCompleted.Should().BeFalse();
        capturedInteraction.Outcome.Should().Be(InteractionOutcome.None);
        capturedInteraction.MeetingLink.Should().Be("https://zoom.us/j/123456");
        capturedInteraction.DurationMinutes.Should().Be(30);
    }

    // ── POST — invitee.canceled ───────────────────────────────────────────────

    [Fact]
    public async Task Post_Returns200_ForInviteeCanceledEvent()
    {
        // Arrange
        var options = new CalendlyOptions { WebhookSigningKey = string.Empty };
        var startTime = new DateTime(2026, 3, 15, 10, 0, 0, DateTimeKind.Utc);

        var existingInteraction = new Interaction
        {
            Id = 7,
            InteractionType = InteractionType.Meeting,
            Subject = "Calendly: 30 Minute Meeting",
            EmailAddress = "jane@example.com",
            InteractionDate = startTime,
            Outcome = InteractionOutcome.None,
            IsCompleted = false
        };

        var interactionMock = new Mock<IInteractionService>();
        interactionMock
            .Setup(s => s.GetInteractionsAsync(
                It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(),
                It.IsAny<InteractionType?>(), It.IsAny<InteractionOutcome?>(),
                It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(new List<Interaction> { existingInteraction });

        interactionMock
            .Setup(s => s.UpdateAsync(It.IsAny<int>(), It.IsAny<Interaction>()))
            .ReturnsAsync(true);

        var controller = BuildController(options, interactionMock);

        var json = BuildInviteeCanceledJson(
            inviteeEmail: "jane@example.com",
            startTime: "2026-03-15T10:00:00Z");
        var body = Encoding.UTF8.GetBytes(json);
        var ctx = BuildHttpContextWithBody(body);
        controller.ControllerContext = new ControllerContext { HttpContext = ctx };

        // Act
        var result = await controller.ReceiveEvent(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        interactionMock.Verify(
            s => s.UpdateAsync(7, It.Is<Interaction>(i =>
                i.Outcome == InteractionOutcome.Cancelled &&
                i.IsCompleted == true)),
            Times.Once);
    }
}
