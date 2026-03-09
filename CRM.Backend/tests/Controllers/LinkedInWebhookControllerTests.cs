// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
// Unit Tests: LinkedInWebhookController
//
// Verified from source before writing:
//   Class: LinkedInWebhookController, Namespace: CRM.Api.Controllers.Webhooks
//   Base: CrmControllerBase (inherits ControllerBase)
//   Constructor: (IOptions<LinkedInMessagingOptions>, ILogger<LinkedInWebhookController>)
//   POST /api/webhooks/linkedin:
//     Reads raw body bytes, validates x-li-signature (base64 HMACSHA256 of body with ClientSecret)
//     When ClientSecret configured: if invalid → StatusCode(403)
//     On valid (or no ClientSecret): logs event, returns Ok()
//   IsValidLinkedInSignature: public static, testable directly
//     Algorithm: base64(HMACSHA256(key=clientSecret, data=rawBody))
//     No sha256= prefix — raw base64 value only
//     Constant-time comparison via CryptographicOperations.FixedTimeEquals
using System.Security.Cryptography;
using System.Text;
using CRM.Api.Controllers.Webhooks;
using CRM.Core.Configuration;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for <see cref="LinkedInWebhookController"/>.
/// COMM-004: Verifies inbound webhook signature validation.
/// </summary>
public class LinkedInWebhookControllerTests
{
    // ── Helpers ──────────────────────────────────────────────────────────────

    private static LinkedInWebhookController BuildController(
        LinkedInMessagingOptions options)
    {
        var optionsMock = Options.Create(options);
        var logger = new Mock<ILogger<LinkedInWebhookController>>();
        return new LinkedInWebhookController(optionsMock, logger.Object);
    }

    /// <summary>
    /// Computes the expected x-li-signature header value (raw base64 HMAC-SHA256).
    /// </summary>
    private static string ComputeLinkedInSignature(byte[] rawBody, string clientSecret)
    {
        var key = Encoding.UTF8.GetBytes(clientSecret);
        using var hmac = new HMACSHA256(key);
        var hash = hmac.ComputeHash(rawBody);
        return Convert.ToBase64String(hash);
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
        {
            ctx.Request.Headers["x-li-signature"] = signatureHeader;
        }

        return ctx;
    }

    // ── IsValidLinkedInSignature (static) ────────────────────────────────────

    [Fact]
    public void IsValidLinkedInSignature_ReturnsFalse_WhenHeaderIsEmpty()
    {
        var body = Encoding.UTF8.GetBytes("{\"eventType\":\"MESSAGE\"}");
        var result = LinkedInWebhookController.IsValidLinkedInSignature(body, string.Empty, "secret");
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidLinkedInSignature_ReturnsFalse_WhenSignatureIsWrong()
    {
        var body = Encoding.UTF8.GetBytes("{\"eventType\":\"MESSAGE\"}");
        var result = LinkedInWebhookController.IsValidLinkedInSignature(
            body, "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA==", "secret");
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidLinkedInSignature_ReturnsTrue_WhenSignatureIsCorrect()
    {
        const string clientSecret = "test_linkedin_client_secret_abc";
        var body = Encoding.UTF8.GetBytes("{\"eventType\":\"MEMBER_MESSAGE\",\"data\":{}}");
        var signature = ComputeLinkedInSignature(body, clientSecret);

        var result = LinkedInWebhookController.IsValidLinkedInSignature(body, signature, clientSecret);
        result.Should().BeTrue();
    }

    // ── ReceiveEvent (POST) ───────────────────────────────────────────────────

    [Fact]
    public async Task Post_Returns200_WhenSecretNotConfigured()
    {
        var controller = BuildController(new LinkedInMessagingOptions
        {
            ClientSecret = string.Empty,
            Enabled = false,
            MockMode = true
        });

        var body = Encoding.UTF8.GetBytes("{\"eventType\":\"MEMBER_MESSAGE\"}");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = BuildHttpContextWithBody(body)
        };

        var result = await controller.ReceiveEvent(CancellationToken.None);

        result.Should().BeOfType<OkResult>(
            "COMM-004: when ClientSecret is not configured (dev mode), all events are accepted.");
    }

    [Fact]
    public async Task Post_Returns200_WhenValidSignature()
    {
        const string clientSecret = "valid_linkedin_client_secret_xyz";
        var body = Encoding.UTF8.GetBytes("{\"eventType\":\"MEMBER_MESSAGE\",\"data\":{}}");
        var signature = ComputeLinkedInSignature(body, clientSecret);

        var controller = BuildController(new LinkedInMessagingOptions
        {
            ClientSecret = clientSecret,
            Enabled = false,
            MockMode = true
        });

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = BuildHttpContextWithBody(body, signature)
        };

        var result = await controller.ReceiveEvent(CancellationToken.None);

        result.Should().BeOfType<OkResult>("valid HMAC-SHA256 signature should be accepted.");
    }

    [Fact]
    public async Task Post_Returns403_WhenSignatureIsInvalid()
    {
        const string clientSecret = "valid_linkedin_client_secret_xyz";
        var body = Encoding.UTF8.GetBytes("{\"eventType\":\"MEMBER_MESSAGE\"}");

        var controller = BuildController(new LinkedInMessagingOptions
        {
            ClientSecret = clientSecret,
            Enabled = false,
            MockMode = true
        });

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = BuildHttpContextWithBody(body, "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=")
        };

        var result = await controller.ReceiveEvent(CancellationToken.None);

        var statusResult = result.Should().BeOfType<StatusCodeResult>().Subject;
        statusResult.StatusCode.Should().Be(403, "invalid signature must be rejected.");
    }

    [Fact]
    public async Task Post_Returns200_WhenPayloadIsMalformedJson()
    {
        var controller = BuildController(new LinkedInMessagingOptions
        {
            ClientSecret = string.Empty,
            Enabled = false,
            MockMode = true
        });

        var body = Encoding.UTF8.GetBytes("not-valid-json");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = BuildHttpContextWithBody(body)
        };

        var result = await controller.ReceiveEvent(CancellationToken.None);

        result.Should().BeOfType<OkResult>(
            "malformed JSON should be logged and swallowed; LinkedIn must receive 200.");
    }
}
