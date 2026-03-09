// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
// Unit Tests: FacebookWebhookController
//
// Verified from source before writing:
//   Class: FacebookWebhookController, Namespace: CRM.Api.Controllers.Webhooks
//   Base: CrmControllerBase (inherits ControllerBase)
//   Constructor: (IOptions<FacebookMessengerOptions>, ILogger<FacebookWebhookController>)
//   GET /api/webhooks/facebook:
//     [FromQuery] hub.mode, hub.verify_token, hub.challenge
//     If mode=="subscribe" && verifyToken==options.VerifyToken → Content(challenge, "text/plain")
//     Else → Forbid()
//   POST /api/webhooks/facebook:
//     Reads raw body bytes, validates X-Hub-Signature-256 (sha256=+HMACSHA256)
//     When AppSecret configured: if invalid → StatusCode(403)
//     On valid (or no AppSecret): parses entry[0].messaging[0], logs PSID+text, returns Ok()
//   IsValidHubSignature: public static, testable directly
//     Algorithm: sha256= + lowercase hex of HMACSHA256(key=appSecret, data=rawBody)
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
/// Unit tests for <see cref="FacebookWebhookController"/>.
/// </summary>
public class FacebookWebhookControllerTests
{
    // ── Helpers ──────────────────────────────────────────────────────────────

    private static FacebookWebhookController BuildController(
        FacebookMessengerOptions options)
    {
        var optionsMock = Options.Create(options);
        var logger = new Mock<ILogger<FacebookWebhookController>>();
        return new FacebookWebhookController(optionsMock, logger.Object);
    }

    /// <summary>Computes the expected X-Hub-Signature-256 header value for a raw body.</summary>
    private static string ComputeHubSignature(byte[] rawBody, string appSecret)
    {
        var key = Encoding.UTF8.GetBytes(appSecret);
        using var hmac = new HMACSHA256(key);
        var hash = hmac.ComputeHash(rawBody);
        return "sha256=" + Convert.ToHexString(hash).ToLowerInvariant();
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
            ctx.Request.Headers["X-Hub-Signature-256"] = signatureHeader;
        }

        return ctx;
    }

    // ── IsValidHubSignature (static) ─────────────────────────────────────────

    [Fact]
    public void IsValidHubSignature_ReturnsFalse_WhenHeaderIsEmpty()
    {
        var body = Encoding.UTF8.GetBytes("{\"object\":\"page\"}");
        var result = FacebookWebhookController.IsValidHubSignature(body, string.Empty, "secret");
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidHubSignature_ReturnsFalse_WhenHeaderMissesSha256Prefix()
    {
        var body = Encoding.UTF8.GetBytes("{\"object\":\"page\"}");
        var result = FacebookWebhookController.IsValidHubSignature(body, "invalidsignature", "secret");
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidHubSignature_ReturnsFalse_WhenSignatureIsWrong()
    {
        var body = Encoding.UTF8.GetBytes("{\"object\":\"page\"}");
        var result = FacebookWebhookController.IsValidHubSignature(
            body, "sha256=aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", "secret");
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidHubSignature_ReturnsTrue_WhenSignatureIsCorrect()
    {
        const string appSecret = "test_app_secret";
        var body = Encoding.UTF8.GetBytes("{\"object\":\"page\",\"entry\":[]}");
        var signature = ComputeHubSignature(body, appSecret);

        var result = FacebookWebhookController.IsValidHubSignature(body, signature, appSecret);
        result.Should().BeTrue();
    }

    // ── VerifyWebhook (GET) — verification challenge ─────────────────────────

    [Fact]
    public void Get_ReturnsForbid_WhenVerifyTokenMismatch()
    {
        var options = new FacebookMessengerOptions
        {
            VerifyToken = "correct_token",
            Enabled = false
        };
        var controller = BuildController(options);

        var result = controller.VerifyWebhook(
            mode: "subscribe",
            verifyToken: "wrong_token",
            challenge: "challenge_abc");

        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public void Get_ReturnsForbid_WhenModeIsNotSubscribe()
    {
        var options = new FacebookMessengerOptions { VerifyToken = "my_token" };
        var controller = BuildController(options);

        var result = controller.VerifyWebhook(
            mode: "unsubscribe",
            verifyToken: "my_token",
            challenge: "challenge_abc");

        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public void Get_ReturnsForbid_WhenVerifyTokenOptionIsEmpty()
    {
        // When VerifyToken is unconfigured the endpoint must reject all challenges.
        var options = new FacebookMessengerOptions { VerifyToken = string.Empty };
        var controller = BuildController(options);

        var result = controller.VerifyWebhook(
            mode: "subscribe",
            verifyToken: string.Empty,
            challenge: "challenge_abc");

        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public void Get_ReturnsChallenge_WhenVerifyTokenMatches()
    {
        const string token = "crm-fb-verify-dev";
        const string challenge = "1234567890";

        var options = new FacebookMessengerOptions { VerifyToken = token };
        var controller = BuildController(options);

        var result = controller.VerifyWebhook(
            mode: "subscribe",
            verifyToken: token,
            challenge: challenge);

        result.Should().BeOfType<ContentResult>()
            .Which.Content.Should().Be(challenge);
        result.As<ContentResult>().ContentType.Should().Be("text/plain");
    }

    // ── ReceiveEvent (POST) — signature validation ──────────────────────────

    [Fact]
    public async Task Post_Returns403_WhenInvalidSignature()
    {
        var options = new FacebookMessengerOptions
        {
            AppSecret = "real_secret",
            Enabled = true
        };
        var controller = BuildController(options);

        var body = Encoding.UTF8.GetBytes("{\"object\":\"page\",\"entry\":[]}");
        var ctx = BuildHttpContextWithBody(body, signatureHeader: "sha256=bad_signature_value");
        controller.ControllerContext = new ControllerContext { HttpContext = ctx };

        var result = await controller.ReceiveEvent(CancellationToken.None);

        result.Should().BeOfType<StatusCodeResult>()
            .Which.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task Post_Returns403_WhenSignatureHeaderMissing()
    {
        var options = new FacebookMessengerOptions
        {
            AppSecret = "real_secret",
            Enabled = true
        };
        var controller = BuildController(options);

        var body = Encoding.UTF8.GetBytes("{\"object\":\"page\",\"entry\":[]}");
        var ctx = BuildHttpContextWithBody(body, signatureHeader: null);
        controller.ControllerContext = new ControllerContext { HttpContext = ctx };

        var result = await controller.ReceiveEvent(CancellationToken.None);

        result.Should().BeOfType<StatusCodeResult>()
            .Which.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task Post_Returns200_WhenValidSignature()
    {
        const string appSecret = "test_app_secret";
        var options = new FacebookMessengerOptions
        {
            AppSecret = appSecret,
            Enabled = true
        };
        var controller = BuildController(options);

        var body = Encoding.UTF8.GetBytes("{\"object\":\"page\",\"entry\":[{\"messaging\":[]}]}");
        var signature = ComputeHubSignature(body, appSecret);
        var ctx = BuildHttpContextWithBody(body, signatureHeader: signature);
        controller.ControllerContext = new ControllerContext { HttpContext = ctx };

        var result = await controller.ReceiveEvent(CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Post_Returns200_WhenAppSecretNotConfigured()
    {
        // When AppSecret is empty, signature validation is skipped — useful in development.
        var options = new FacebookMessengerOptions { AppSecret = string.Empty };
        var controller = BuildController(options);

        var body = Encoding.UTF8.GetBytes("{\"object\":\"page\",\"entry\":[]}");
        var ctx = BuildHttpContextWithBody(body, signatureHeader: null);
        controller.ControllerContext = new ControllerContext { HttpContext = ctx };

        var result = await controller.ReceiveEvent(CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Post_Returns200_WithInboundMessagePayload()
    {
        const string appSecret = "msg_secret";
        var options = new FacebookMessengerOptions { AppSecret = appSecret, Enabled = true };
        var controller = BuildController(options);

        var payload = """
            {
              "object": "page",
              "entry": [{
                "messaging": [{
                  "sender": { "id": "1234567890" },
                  "message": { "text": "Hello, CRM!" }
                }]
              }]
            }
            """;
        var body = Encoding.UTF8.GetBytes(payload);
        var signature = ComputeHubSignature(body, appSecret);
        var ctx = BuildHttpContextWithBody(body, signatureHeader: signature);
        controller.ControllerContext = new ControllerContext { HttpContext = ctx };

        var result = await controller.ReceiveEvent(CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }
}
