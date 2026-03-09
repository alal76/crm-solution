// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
// Unit Tests: TwitterWebhookController
//
// Verified from source before writing:
//   Class: TwitterWebhookController, Namespace: CRM.Api.Controllers.Webhooks
//   Base: CrmControllerBase (inherits ControllerBase)
//   Constructor: (IOptions<TwitterMessagingOptions>, ILogger<TwitterWebhookController>)
//   GET /api/webhooks/twitter:
//     [FromQuery] crc_token
//     If ConsumerSecret empty: BadRequest (cannot sign)
//     If crc_token empty: BadRequest
//     Else: HMAC-SHA256(key=ConsumerSecret, data=crc_token), Base64 → {"response_token": "sha256=<b64>"}
//   POST /api/webhooks/twitter:
//     Reads raw body bytes, validates x-twitter-webhooks-signature (sha256=+HMACSHA256+base64)
//     When ConsumerSecret configured: if invalid → StatusCode(403)
//     On valid (or no ConsumerSecret): logs event, returns Ok()
//   IsValidTwitterSignature: public static, testable directly
//     Algorithm: sha256= + base64(HMACSHA256(key=consumerSecret, data=rawBody))
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
/// Unit tests for <see cref="TwitterWebhookController"/>.
/// COMM-003: Verifies CRC challenge response and inbound webhook signature validation.
/// </summary>
public class TwitterWebhookControllerTests
{
    // ── Helpers ──────────────────────────────────────────────────────────────

    private static TwitterWebhookController BuildController(
        TwitterMessagingOptions options)
    {
        var optionsMock = Options.Create(options);
        var logger = new Mock<ILogger<TwitterWebhookController>>();
        return new TwitterWebhookController(optionsMock, logger.Object);
    }

    /// <summary>
    /// Computes the expected CRC challenge response token (base64 HMAC-SHA256).
    /// </summary>
    private static string ComputeCrcResponse(string crcToken, string consumerSecret)
    {
        var key = Encoding.UTF8.GetBytes(consumerSecret);
        var data = Encoding.UTF8.GetBytes(crcToken);
        using var hmac = new HMACSHA256(key);
        var hash = hmac.ComputeHash(data);
        return "sha256=" + Convert.ToBase64String(hash);
    }

    /// <summary>
    /// Computes the expected x-twitter-webhooks-signature header value.
    /// </summary>
    private static string ComputeTwitterSignature(byte[] rawBody, string consumerSecret)
    {
        var key = Encoding.UTF8.GetBytes(consumerSecret);
        using var hmac = new HMACSHA256(key);
        var hash = hmac.ComputeHash(rawBody);
        return "sha256=" + Convert.ToBase64String(hash);
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
            ctx.Request.Headers["x-twitter-webhooks-signature"] = signatureHeader;
        }

        return ctx;
    }

    // ── IsValidTwitterSignature (static) ─────────────────────────────────────

    [Fact]
    public void IsValidTwitterSignature_ReturnsFalse_WhenHeaderIsEmpty()
    {
        var body = Encoding.UTF8.GetBytes("{\"for_user_id\":\"123\"}");
        var result = TwitterWebhookController.IsValidTwitterSignature(body, string.Empty, "secret");
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidTwitterSignature_ReturnsFalse_WhenHeaderMissesSha256Prefix()
    {
        var body = Encoding.UTF8.GetBytes("{\"for_user_id\":\"123\"}");
        var result = TwitterWebhookController.IsValidTwitterSignature(body, "AAAA//dead=", "secret");
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidTwitterSignature_ReturnsFalse_WhenSignatureIsWrong()
    {
        var body = Encoding.UTF8.GetBytes("{\"for_user_id\":\"123\"}");
        var result = TwitterWebhookController.IsValidTwitterSignature(
            body, "sha256=AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=", "secret");
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidTwitterSignature_ReturnsTrue_WhenSignatureIsCorrect()
    {
        const string consumerSecret = "test_consumer_secret_abcdef";
        var body = Encoding.UTF8.GetBytes("{\"for_user_id\":\"123\",\"direct_message_events\":[]}");
        var signature = ComputeTwitterSignature(body, consumerSecret);

        var result = TwitterWebhookController.IsValidTwitterSignature(body, signature, consumerSecret);
        result.Should().BeTrue();
    }

    // ── HandleCrcChallenge (GET) ─────────────────────────────────────────────

    [Fact]
    public void Get_ReturnsBadRequest_WhenCrcTokenIsEmpty()
    {
        var controller = BuildController(new TwitterMessagingOptions
        {
            ConsumerSecret = "test_secret",
            Enabled = false,
            MockMode = true
        });

        var result = controller.HandleCrcChallenge(string.Empty);

        result.Should().BeOfType<BadRequestObjectResult>(
            "empty crc_token is not a valid challenge request.");
    }

    [Fact]
    public void Get_ReturnsBadRequest_WhenConsumerSecretIsNotConfigured()
    {
        var controller = BuildController(new TwitterMessagingOptions
        {
            ConsumerSecret = string.Empty,
            Enabled = false,
            MockMode = true
        });

        var result = controller.HandleCrcChallenge("random_crc_token_abc123");

        result.Should().BeOfType<BadRequestObjectResult>(
            "without ConsumerSecret the CRC challenge cannot be signed.");
    }

    [Fact]
    public void Get_ReturnsCrcChallenge_WithValidSignature()
    {
        const string consumerSecret = "my_consumer_secret_xyz";
        const string crcToken = "challenge_token_abc12345";

        var controller = BuildController(new TwitterMessagingOptions
        {
            ConsumerSecret = consumerSecret,
            Enabled = false,
            MockMode = true
        });

        var result = controller.HandleCrcChallenge(crcToken);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var responseToken = okResult.Value!
            .GetType()
            .GetProperty("response_token")!
            .GetValue(okResult.Value) as string;

        var expected = ComputeCrcResponse(crcToken, consumerSecret);
        responseToken.Should().Be(expected,
            "the CRC response must be HMAC-SHA256(consumerSecret, crc_token) base64-encoded.");
    }

    // ── ReceiveEvent (POST) ───────────────────────────────────────────────────

    [Fact]
    public async Task Post_Returns200_WhenSecretNotConfigured()
    {
        var controller = BuildController(new TwitterMessagingOptions
        {
            ConsumerSecret = string.Empty,
            Enabled = false,
            MockMode = true
        });

        var body = Encoding.UTF8.GetBytes("{\"for_user_id\":\"123\",\"direct_message_events\":[]}");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = BuildHttpContextWithBody(body)
        };

        var result = await controller.ReceiveEvent(CancellationToken.None);

        result.Should().BeOfType<OkResult>(
            "COMM-003: when ConsumerSecret is not configured (dev mode), all events are accepted.");
    }

    [Fact]
    public async Task Post_Returns200_WhenValidSignature()
    {
        const string consumerSecret = "valid_consumer_secret_123";
        var body = Encoding.UTF8.GetBytes("{\"for_user_id\":\"999\",\"direct_message_events\":[]}");
        var signature = ComputeTwitterSignature(body, consumerSecret);

        var controller = BuildController(new TwitterMessagingOptions
        {
            ConsumerSecret = consumerSecret,
            Enabled = false,
            MockMode = true
        });

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = BuildHttpContextWithBody(body, signature)
        };

        var result = await controller.ReceiveEvent(CancellationToken.None);

        result.Should().BeOfType<OkResult>("valid HMAC signature should be accepted.");
    }

    [Fact]
    public async Task Post_Returns403_WhenSignatureIsInvalid()
    {
        const string consumerSecret = "valid_consumer_secret_123";
        var body = Encoding.UTF8.GetBytes("{\"for_user_id\":\"999\"}");

        var controller = BuildController(new TwitterMessagingOptions
        {
            ConsumerSecret = consumerSecret,
            Enabled = false,
            MockMode = true
        });

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = BuildHttpContextWithBody(body, "sha256=AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=")
        };

        var result = await controller.ReceiveEvent(CancellationToken.None);

        var statusResult = result.Should().BeOfType<StatusCodeResult>().Subject;
        statusResult.StatusCode.Should().Be(403, "wrong signature must be rejected.");
    }

    [Fact]
    public async Task Post_Returns200_WhenPayloadIsMalformedJson()
    {
        var controller = BuildController(new TwitterMessagingOptions
        {
            ConsumerSecret = string.Empty,
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
            "malformed JSON should be logged and swallowed; Twitter must receive 200.");
    }
}
