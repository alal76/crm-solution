// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Cryptography;
using System.Text;
using CRM.Api.Controllers.Webhooks;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Providers.Stripe;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers.Webhooks;

/// <summary>
/// Unit tests for StripeWebhookController (TCOV-050).
/// </summary>
public class StripeWebhookControllerTests
{
    private readonly Mock<IPaymentService> _mockPaymentService;
    private readonly Mock<IActivityService> _mockActivityService;
    private readonly Mock<ISubscriptionService> _mockSubscriptionService;
    private readonly Mock<IFeatureManager> _mockFeatureManager;
    private readonly Mock<ILogger<StripeWebhookController>> _mockLogger;

    public StripeWebhookControllerTests()
    {
        _mockPaymentService = new Mock<IPaymentService>();
        _mockActivityService = new Mock<IActivityService>();
        _mockSubscriptionService = new Mock<ISubscriptionService>();
        _mockFeatureManager = new Mock<IFeatureManager>();
        _mockLogger = new Mock<ILogger<StripeWebhookController>>();
    }

    private const string TestWebhookSecret = "whsec_test_secret";

    private StripeWebhookController BuildControllerWithBody(string body, string webhookSecret = TestWebhookSecret, bool withValidSignature = true)
    {
        var config = new StripeConfiguration { WebhookSecret = webhookSecret };
        var options = Options.Create(config);
        var controller = new StripeWebhookController(
            options,
            _mockPaymentService.Object,
            _mockActivityService.Object,
            _mockSubscriptionService.Object,
            _mockFeatureManager.Object,
            _mockLogger.Object);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        httpContext.Request.ContentType = "application/json";
        if (withValidSignature && !string.IsNullOrEmpty(webhookSecret))
        {
            httpContext.Request.Headers["Stripe-Signature"] = ComputeSignatureHeader(body, webhookSecret);
        }
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        return controller;
    }

    private static string ComputeSignatureHeader(string payload, string secret)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var signedPayload = $"{timestamp}.{payload}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signedPayload));
        var signature = Convert.ToHexString(hash).ToLowerInvariant();
        return $"t={timestamp},v1={signature}";
    }

    [Fact]
    public async Task HandleWebhook_ShouldReturnBadRequest_WhenBodyIsEmpty()
    {
        var controller = BuildControllerWithBody(string.Empty);

        var result = await controller.HandleWebhook(default);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task HandleWebhook_ShouldReturnUnauthorized_WhenSignatureInvalid()
    {
        var body = @"{""id"":""evt_test"",""type"":""payment_intent.succeeded""}";
        var controller = BuildControllerWithBody(body, webhookSecret: "whsec_test_secret", withValidSignature: false);
        // No Stripe-Signature header -> validation fails

        var result = await controller.HandleWebhook(default);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task HandleWebhook_ShouldReturnUnauthorized_WhenWebhookSecretNotConfigured()
    {
        // REM-BUG-006: an unconfigured secret must fail closed, not silently skip verification.
        var body = @"{""id"":""evt_test"",""type"":""payment_intent.succeeded""}";
        var controller = BuildControllerWithBody(body, webhookSecret: "");

        var result = await controller.HandleWebhook(default);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task HandleWebhook_ShouldReturnBadRequest_WhenPayloadIsInvalidJson()
    {
        var controller = BuildControllerWithBody("not-valid-json");

        var result = await controller.HandleWebhook(default);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task HandleWebhook_ShouldReturnOk_WhenValidPayloadAndValidSignature()
    {
        var body = @"{""id"":""evt_test_123"",""type"":""payment_intent.succeeded"",""data"":{""object"":{}}}";
        var controller = BuildControllerWithBody(body);
        _mockFeatureManager.Setup(f => f.IsEnabledAsync(It.IsAny<string>())).ReturnsAsync(true);

        var result = await controller.HandleWebhook(default);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task HandleWebhook_ShouldReturnOk_WhenChargeSucceededEvent()
    {
        var body = @"{""id"":""evt_charge_1"",""type"":""charge.succeeded"",""data"":{""object"":{""id"":""ch_test"",""amount"":1000,""currency"":""usd""}}}";
        var controller = BuildControllerWithBody(body);
        _mockFeatureManager.Setup(f => f.IsEnabledAsync(It.IsAny<string>())).ReturnsAsync(true);

        var result = await controller.HandleWebhook(default);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task HandleWebhook_ShouldReturnOk_WhenUnknownEventType()
    {
        var body = @"{""id"":""evt_unknown"",""type"":""customer.created"",""data"":{""object"":{}}}";
        var controller = BuildControllerWithBody(body);

        var result = await controller.HandleWebhook(default);

        result.Should().BeOfType<OkObjectResult>();
    }
}
