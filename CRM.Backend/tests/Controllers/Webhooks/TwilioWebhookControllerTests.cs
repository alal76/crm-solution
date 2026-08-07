// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// Unit Tests: TwilioWebhookController — HandleCallStatusCallback (POST /api/webhooks/twilio/call-status)
// Verified from source before writing:
//   Class: TwilioWebhookController, Namespace: CRM.Api.Controllers.Webhooks
//   Base: CrmControllerBase (inherits ControllerBase)
//   Constructor: (INotificationPort, IActivityService, ITwilioCallLoggingService, IOptions<TwilioConfiguration>, ILogger<TwilioWebhookController>)
//   POST /api/webhooks/twilio/call-status:
//     Reads form fields: CallSid, CallStatus, From, To, Direction, CallDuration
//     When TwilioConfiguration.AuthToken configured: validates X-Twilio-Signature (HMAC-SHA1)
//       via WhatsAppWebhookController.IsValidTwilioSignature (same algorithm/order as the
//       WhatsApp webhook, reused rather than duplicated).
//     On invalid/missing sig (with AuthToken configured): returns StatusCode(403)
//     On missing CallSid: returns 200 OK without invoking ITwilioCallLoggingService
//     On CallStatus "queued"/"ringing": calls LogOutboundCallAsync/LogInboundCallAsync
//       (direction "inbound" -> inbound, otherwise outbound)
//     On any other CallStatus: calls UpdateCallStatusAsync(callSid, status, duration)
using System.Security.Cryptography;
using System.Text;
using CRM.Api.Controllers.Webhooks;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Input;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.Twilio;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers.Webhooks;

/// <summary>
/// Unit tests for <see cref="TwilioWebhookController"/>'s call-status callback action.
/// </summary>
public class TwilioWebhookControllerTests
{
    // ── Helpers ──────────────────────────────────────────────────────────────

    private static TwilioWebhookController BuildController(
        TwilioConfiguration config,
        Mock<ITwilioCallLoggingService>? callLoggingMock = null,
        Mock<INotificationPort>? notificationMock = null,
        Mock<IActivityService>? activityMock = null)
    {
        var notification = notificationMock ?? new Mock<INotificationPort>();
        var activity = activityMock ?? new Mock<IActivityService>();
        var callLogging = callLoggingMock ?? new Mock<ITwilioCallLoggingService>();
        var options = Options.Create(config);
        var logger = new Mock<ILogger<TwilioWebhookController>>();

        return new TwilioWebhookController(
            notification.Object,
            activity.Object,
            callLogging.Object,
            options,
            logger.Object);
    }

    private static DefaultHttpContext BuildHttpContextWithForm(
        Dictionary<string, string> formFields,
        string? signature = null)
    {
        var ctx = new DefaultHttpContext();
        var dict = formFields.ToDictionary(
            kv => kv.Key,
            kv => new StringValues(kv.Value));
        ctx.Request.Form = new FormCollection(dict);
        ctx.Request.ContentType = "application/x-www-form-urlencoded";

        if (signature != null)
        {
            ctx.Request.Headers["X-Twilio-Signature"] = signature;
        }

        ctx.Request.Scheme = "https";
        ctx.Request.Host = new HostString("example.com");
        ctx.Request.Path = "/api/webhooks/twilio/call-status";
        return ctx;
    }

    /// <summary>Computes the expected Twilio HMAC-SHA1 signature for a given URL and form.</summary>
    private static string ComputeTwilioSignature(string url, Dictionary<string, string> form, string authToken)
    {
        var sortedParams = form
            .OrderBy(kv => kv.Key, StringComparer.Ordinal)
            .Select(kv => kv.Key + kv.Value);
        var stringToSign = url + string.Concat(sortedParams);
        using var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(authToken));
        return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));
    }

    // ── Signature validation ────────────────────────────────────────────────

    [Fact]
    public async Task CallStatus_Returns403_WhenInvalidSignature()
    {
        // Arrange — AuthToken configured so signature check is active
        var config = new TwilioConfiguration
        {
            AccountSid = "AC123",
            AuthToken = "my_auth_token",
            FromPhoneNumber = "+15005550006"
        };
        var callLoggingMock = new Mock<ITwilioCallLoggingService>();
        var controller = BuildController(config, callLoggingMock);

        var formFields = new Dictionary<string, string>
        {
            ["CallSid"] = "CA123",
            ["CallStatus"] = "completed",
            ["From"] = "+15551234567",
            ["To"] = "+15557654321",
            ["Direction"] = "inbound",
            ["CallDuration"] = "42"
        };
        var ctx = BuildHttpContextWithForm(formFields, signature: "wrong_signature==");
        controller.ControllerContext = new ControllerContext { HttpContext = ctx };

        // Act
        var result = await controller.HandleCallStatusCallback();

        // Assert
        result.Should().BeOfType<StatusCodeResult>()
            .Which.StatusCode.Should().Be(403);
        callLoggingMock.Verify(
            s => s.UpdateCallStatusAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()),
            Times.Never);
        callLoggingMock.Verify(
            s => s.LogInboundCallAsync(It.IsAny<TwilioCallEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CallStatus_Returns403_WhenSignatureHeaderMissing()
    {
        // Arrange — AuthToken configured but X-Twilio-Signature header absent
        var config = new TwilioConfiguration
        {
            AccountSid = "AC123",
            AuthToken = "real_token",
            FromPhoneNumber = "+15005550006"
        };
        var callLoggingMock = new Mock<ITwilioCallLoggingService>();
        var controller = BuildController(config, callLoggingMock);

        var ctx = BuildHttpContextWithForm(
            new Dictionary<string, string> { ["CallSid"] = "CA123", ["CallStatus"] = "ringing" },
            signature: null);
        controller.ControllerContext = new ControllerContext { HttpContext = ctx };

        // Act
        var result = await controller.HandleCallStatusCallback();

        // Assert
        result.Should().BeOfType<StatusCodeResult>()
            .Which.StatusCode.Should().Be(403);
        callLoggingMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CallStatus_ProcessesCallback_WhenAuthTokenNotConfigured()
    {
        // Arrange — empty AuthToken → signature validation is skipped entirely
        var config = new TwilioConfiguration { AuthToken = string.Empty };
        var callLoggingMock = new Mock<ITwilioCallLoggingService>();
        callLoggingMock
            .Setup(s => s.UpdateCallStatusAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var controller = BuildController(config, callLoggingMock);
        var ctx = BuildHttpContextWithForm(new Dictionary<string, string>
        {
            ["CallSid"] = "CA999",
            ["CallStatus"] = "completed",
            ["From"] = "+15551234567",
            ["To"] = "+15557654321",
            ["Direction"] = "inbound",
            ["CallDuration"] = "60"
        });
        controller.ControllerContext = new ControllerContext { HttpContext = ctx };

        // Act
        var result = await controller.HandleCallStatusCallback();

        // Assert
        result.Should().BeOfType<OkResult>();
        callLoggingMock.Verify(
            s => s.UpdateCallStatusAsync("CA999", "completed", 60, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CallStatus_ProcessesCallback_WhenValidSignature()
    {
        // Arrange — correct HMAC-SHA1 signature
        const string authToken = "valid_auth_token_xyz";
        const string url = "https://example.com/api/webhooks/twilio/call-status";

        var formFields = new Dictionary<string, string>
        {
            ["CallSid"] = "CA_valid_001",
            ["CallStatus"] = "completed",
            ["From"] = "+15551234567",
            ["To"] = "+15557654321",
            ["Direction"] = "inbound",
            ["CallDuration"] = "30"
        };
        var correctSignature = ComputeTwilioSignature(url, formFields, authToken);

        var config = new TwilioConfiguration { AccountSid = "AC123", AuthToken = authToken, FromPhoneNumber = "+15005550006" };
        var callLoggingMock = new Mock<ITwilioCallLoggingService>();
        callLoggingMock
            .Setup(s => s.UpdateCallStatusAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var controller = BuildController(config, callLoggingMock);
        var ctx = BuildHttpContextWithForm(formFields, signature: correctSignature);
        controller.ControllerContext = new ControllerContext { HttpContext = ctx };

        // Act
        var result = await controller.HandleCallStatusCallback();

        // Assert
        result.Should().BeOfType<OkResult>();
        callLoggingMock.Verify(
            s => s.UpdateCallStatusAsync("CA_valid_001", "completed", 30, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ── Call logging routing ─────────────────────────────────────────────────

    [Fact]
    public async Task CallStatus_LogsInboundCall_WhenStatusIsRingingAndDirectionInbound()
    {
        var config = new TwilioConfiguration { AuthToken = string.Empty };
        var callLoggingMock = new Mock<ITwilioCallLoggingService>();
        callLoggingMock
            .Setup(s => s.LogInboundCallAsync(It.IsAny<TwilioCallEvent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var controller = BuildController(config, callLoggingMock);
        var ctx = BuildHttpContextWithForm(new Dictionary<string, string>
        {
            ["CallSid"] = "CA_inbound_ring",
            ["CallStatus"] = "ringing",
            ["From"] = "+15551234567",
            ["To"] = "+15557654321",
            ["Direction"] = "inbound"
        });
        controller.ControllerContext = new ControllerContext { HttpContext = ctx };

        var result = await controller.HandleCallStatusCallback();

        result.Should().BeOfType<OkResult>();
        callLoggingMock.Verify(
            s => s.LogInboundCallAsync(It.Is<TwilioCallEvent>(e => e.CallSid == "CA_inbound_ring"), It.IsAny<CancellationToken>()),
            Times.Once);
        callLoggingMock.Verify(
            s => s.LogOutboundCallAsync(It.IsAny<TwilioCallEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CallStatus_LogsOutboundCall_WhenStatusIsQueuedAndDirectionOutbound()
    {
        var config = new TwilioConfiguration { AuthToken = string.Empty };
        var callLoggingMock = new Mock<ITwilioCallLoggingService>();
        callLoggingMock
            .Setup(s => s.LogOutboundCallAsync(It.IsAny<TwilioCallEvent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        var controller = BuildController(config, callLoggingMock);
        var ctx = BuildHttpContextWithForm(new Dictionary<string, string>
        {
            ["CallSid"] = "CA_outbound_queued",
            ["CallStatus"] = "queued",
            ["From"] = "+15557654321",
            ["To"] = "+15551234567",
            ["Direction"] = "outbound-api"
        });
        controller.ControllerContext = new ControllerContext { HttpContext = ctx };

        var result = await controller.HandleCallStatusCallback();

        result.Should().BeOfType<OkResult>();
        callLoggingMock.Verify(
            s => s.LogOutboundCallAsync(It.Is<TwilioCallEvent>(e => e.CallSid == "CA_outbound_queued"), It.IsAny<CancellationToken>()),
            Times.Once);
        callLoggingMock.Verify(
            s => s.LogInboundCallAsync(It.IsAny<TwilioCallEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ── Payload edge cases ───────────────────────────────────────────────────

    [Fact]
    public async Task CallStatus_ReturnsOk_AndSkipsService_WhenCallSidMissing()
    {
        // Even malformed payloads must return 200 so Twilio doesn't retry indefinitely.
        var config = new TwilioConfiguration { AuthToken = string.Empty };
        var callLoggingMock = new Mock<ITwilioCallLoggingService>();

        var controller = BuildController(config, callLoggingMock);
        var ctx = BuildHttpContextWithForm(new Dictionary<string, string>
        {
            ["CallStatus"] = "completed",
            ["From"] = "+15551234567",
            ["To"] = "+15557654321"
            // CallSid intentionally omitted
        });
        controller.ControllerContext = new ControllerContext { HttpContext = ctx };

        var result = await controller.HandleCallStatusCallback();

        result.Should().BeOfType<OkResult>();
        callLoggingMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CallStatus_ReturnsOk_WhenCallLoggingServiceThrows()
    {
        // The webhook must still ack with 200 even if the downstream service fails.
        var config = new TwilioConfiguration { AuthToken = string.Empty };
        var callLoggingMock = new Mock<ITwilioCallLoggingService>();
        callLoggingMock
            .Setup(s => s.UpdateCallStatusAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("downstream unavailable"));

        var controller = BuildController(config, callLoggingMock);
        var ctx = BuildHttpContextWithForm(new Dictionary<string, string>
        {
            ["CallSid"] = "CA_err",
            ["CallStatus"] = "completed",
            ["From"] = "+15551234567",
            ["To"] = "+15557654321",
            ["Direction"] = "inbound"
        });
        controller.ControllerContext = new ControllerContext { HttpContext = ctx };

        var result = await controller.HandleCallStatusCallback();

        result.Should().BeOfType<OkResult>();
    }
}
