// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
// Unit Tests: WhatsAppWebhookController
//
// Verified from source before writing:
//   Class: WhatsAppWebhookController, Namespace: CRM.Api.Controllers.Webhooks
//   Base: CrmControllerBase (inherits ControllerBase)
//   Constructor: (IOptions<WhatsAppOptions>, IActivityService, ILogger<WhatsAppWebhookController>)
//   POST /api/webhooks/whatsapp:
//     [FromForm] params: From, Body, To, MessageSid, NumMedia
//     When AuthToken configured: reads Request.Form, validates X-Twilio-Signature (HMAC-SHA1)
//     On invalid sig: returns StatusCode(403)
//     On valid sig (or no AuthToken): creates Activity (ActivityType.ChatMessage), returns XML TwiML
//   IsValidTwilioSignature: internal static, testable directly
//     Algorithm: HMAC-SHA1(key=authToken, data=url+sortedFormPairs), Base64 → compare to header
using System.Security.Cryptography;
using System.Text;
using CRM.Api.Controllers.Webhooks;
using CRM.Core.Configuration;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for <see cref="WhatsAppWebhookController"/>.
/// </summary>
public class WhatsAppWebhookControllerTests
{
    // ── Helpers ──────────────────────────────────────────────────────────────

    private static WhatsAppWebhookController BuildController(
        WhatsAppOptions options,
        Mock<IActivityService>? activityMock = null)
    {
        var optionsMock = Options.Create(options);
        var activity = activityMock ?? new Mock<IActivityService>();
        var logger = new Mock<ILogger<WhatsAppWebhookController>>();
        return new WhatsAppWebhookController(optionsMock, activity.Object, logger.Object);
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

        if (signature != null)
        {
            ctx.Request.Headers["X-Twilio-Signature"] = signature;
        }

        ctx.Request.Scheme = "https";
        ctx.Request.Host = new HostString("example.com");
        ctx.Request.Path = "/api/webhooks/whatsapp";
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

    // ── IsValidTwilioSignature ────────────────────────────────────────────────

    [Fact]
    public void IsValidTwilioSignature_ReturnsFalse_WhenSignatureIsEmpty()
    {
        var form = new FormCollection(new Dictionary<string, StringValues>
        {
            ["From"] = "whatsapp:+15551234567"
        });
        var result = WhatsAppWebhookController.IsValidTwilioSignature(
            string.Empty, "https://example.com/api/webhooks/whatsapp", form, "myAuthToken");
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidTwilioSignature_ReturnsFalse_WhenSignatureIsWrong()
    {
        var form = new FormCollection(new Dictionary<string, StringValues>
        {
            ["From"] = "whatsapp:+15551234567",
            ["Body"] = "Hello"
        });
        var result = WhatsAppWebhookController.IsValidTwilioSignature(
            "invalidsignature==", "https://example.com/api/webhooks/whatsapp", form, "myAuthToken");
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidTwilioSignature_ReturnsTrue_WhenSignatureIsCorrect()
    {
        const string url = "https://example.com/api/webhooks/whatsapp";
        const string authToken = "test_auth_token_12345";
        var formFields = new Dictionary<string, string>
        {
            ["Body"] = "Hello",
            ["From"] = "whatsapp:+15551234567",
            ["MessageSid"] = "SM123",
            ["To"] = "whatsapp:+14155238886"
        };

        var signature = ComputeTwilioSignature(url, formFields, authToken);

        var form = new FormCollection(formFields.ToDictionary(
            kv => kv.Key,
            kv => new StringValues(kv.Value)));

        var result = WhatsAppWebhookController.IsValidTwilioSignature(signature, url, form, authToken);
        result.Should().BeTrue();
    }

    // ── ReceiveMessage — signature validation ────────────────────────────────

    [Fact]
    public async Task Post_Returns403_WhenInvalidTwilioSignature()
    {
        // Arrange — controller configured with an AuthToken so signature check is active
        var options = new WhatsAppOptions
        {
            AccountSid = "AC123",
            AuthToken = "my_auth_token",
            Enabled = true
        };
        var controller = BuildController(options);

        var formFields = new Dictionary<string, string>
        {
            ["From"] = "whatsapp:+15551234567",
            ["Body"] = "Hi",
            ["To"] = "whatsapp:+14155238886",
            ["MessageSid"] = "SMtest"
        };
        var ctx = BuildHttpContextWithForm(formFields, signature: "wrong_signature==");
        controller.ControllerContext = new ControllerContext { HttpContext = ctx };

        // Act
        var result = await controller.ReceiveMessage(
            From: "whatsapp:+15551234567",
            Body: "Hi",
            To: "whatsapp:+14155238886",
            MessageSid: "SMtest");

        // Assert
        result.Should().BeOfType<StatusCodeResult>()
            .Which.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task Post_Returns403_WhenNoSignatureHeader()
    {
        // Arrange — AuthToken is configured but X-Twilio-Signature header is absent
        var options = new WhatsAppOptions { AuthToken = "real_token", AccountSid = "AC123" };
        var activityMock = new Mock<IActivityService>();
        var controller = BuildController(options, activityMock);

        var ctx = BuildHttpContextWithForm(
            new Dictionary<string, string> { ["From"] = "whatsapp:+15551234567" },
            signature: null);
        controller.ControllerContext = new ControllerContext { HttpContext = ctx };

        // Act
        var result = await controller.ReceiveMessage(From: "whatsapp:+15551234567");

        // Assert
        result.Should().BeOfType<StatusCodeResult>()
            .Which.StatusCode.Should().Be(403);
    }

    // ── ReceiveMessage — happy path ──────────────────────────────────────────

    [Fact]
    public async Task Post_ReturnsXmlTwiml_WhenAuthTokenNotConfigured()
    {
        // Arrange — empty AuthToken → signature validation is skipped
        var options = new WhatsAppOptions { AuthToken = string.Empty };
        var activityMock = new Mock<IActivityService>();
        activityMock
            .Setup(a => a.CreateAsync(It.IsAny<Activity>()))
            .ReturnsAsync(new ActivityDto());

        var controller = BuildController(options, activityMock);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        var result = await controller.ReceiveMessage(
            From: "whatsapp:+15551234567",
            Body: "Hello CRM",
            To: "whatsapp:+14155238886",
            MessageSid: "SM_test_001");

        // Assert — must return TwiML XML so Twilio doesn't retry
        result.Should().BeOfType<ContentResult>();
        var contentResult = (ContentResult)result;
        contentResult.ContentType.Should().Be("application/xml");
        contentResult.Content.Should().Contain("<Response>");
    }

    [Fact]
    public async Task Post_CreatesActivityRecord_WhenValidMessage()
    {
        // Arrange
        var options = new WhatsAppOptions { AuthToken = string.Empty };
        var activityMock = new Mock<IActivityService>();
        activityMock
            .Setup(a => a.CreateAsync(It.IsAny<Activity>()))
            .ReturnsAsync(new ActivityDto());

        var controller = BuildController(options, activityMock);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        await controller.ReceiveMessage(
            From: "whatsapp:+15551234567",
            Body: "Test message",
            To: "whatsapp:+14155238886",
            MessageSid: "SM_test_002");

        // Assert — CreateAsync called exactly once with a WhatsApp category activity
        activityMock.Verify(
            a => a.CreateAsync(It.Is<Activity>(act =>
                act.Source == "WhatsApp" &&
                act.ActivityType == ActivityType.ChatMessage)),
            Times.Once);
    }

    [Fact]
    public async Task Post_StillReturnsXml_WhenActivityCreationFails()
    {
        // Even if the DB write fails, Twilio must receive a 200 response
        var options = new WhatsAppOptions { AuthToken = string.Empty };
        var activityMock = new Mock<IActivityService>();
        activityMock
            .Setup(a => a.CreateAsync(It.IsAny<Activity>()))
            .ThrowsAsync(new InvalidOperationException("DB unavailable"));

        var controller = BuildController(options, activityMock);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        var result = await controller.ReceiveMessage(
            From: "whatsapp:+15551234567",
            Body: "Fail gracefully");

        // Assert — still returns valid TwiML (no 500)
        result.Should().BeOfType<ContentResult>();
        ((ContentResult)result).ContentType.Should().Be("application/xml");
    }

    [Fact]
    public async Task Post_ReturnsXmlTwiml_WhenValidSignature()
    {
        // Arrange — correct HMAC-SHA1 signature
        const string authToken = "valid_auth_token_xyz";
        const string url = "https://example.com/api/webhooks/whatsapp";

        var formFields = new Dictionary<string, string>
        {
            ["Body"] = "Hi there",
            ["From"] = "whatsapp:+15551234567",
            ["MessageSid"] = "SM_valid_001",
            ["NumMedia"] = "0",
            ["To"] = "whatsapp:+14155238886"
        };

        var correctSignature = ComputeTwilioSignature(url, formFields, authToken);

        var options = new WhatsAppOptions { AuthToken = authToken, AccountSid = "AC123" };
        var activityMock = new Mock<IActivityService>();
        activityMock
            .Setup(a => a.CreateAsync(It.IsAny<Activity>()))
            .ReturnsAsync(new ActivityDto());

        var controller = BuildController(options, activityMock);
        var ctx = BuildHttpContextWithForm(formFields, signature: correctSignature);
        controller.ControllerContext = new ControllerContext { HttpContext = ctx };

        // Act
        var result = await controller.ReceiveMessage(
            From: "whatsapp:+15551234567",
            Body: "Hi there",
            To: "whatsapp:+14155238886",
            MessageSid: "SM_valid_001",
            NumMedia: "0");

        // Assert
        result.Should().BeOfType<ContentResult>();
        ((ContentResult)result).ContentType.Should().Be("application/xml");
    }
}
