// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text;
using System.Text.Json;
using CRM.Api.Controllers.Webhooks;
using CRM.Core.Features;
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

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for StripeWebhookController — FLAG-006 subscription tracking.
/// </summary>
public class StripeWebhookControllerTests
{
    private readonly Mock<IOptions<StripeConfiguration>> _mockOptions;
    private readonly Mock<IPaymentService> _mockPaymentService;
    private readonly Mock<IActivityService> _mockActivityService;
    private readonly Mock<ISubscriptionService> _mockSubscriptionService;
    private readonly Mock<IFeatureManager> _mockFeatureManager;
    private readonly Mock<ILogger<StripeWebhookController>> _mockLogger;

    public StripeWebhookControllerTests()
    {
        _mockOptions = new Mock<IOptions<StripeConfiguration>>();
        _mockPaymentService = new Mock<IPaymentService>();
        _mockActivityService = new Mock<IActivityService>();
        _mockSubscriptionService = new Mock<ISubscriptionService>();
        _mockFeatureManager = new Mock<IFeatureManager>();
        _mockLogger = new Mock<ILogger<StripeWebhookController>>();

        // Default config: no webhook secret → signature validation skipped
        _mockOptions.Setup(o => o.Value).Returns(new StripeConfiguration
        {
            WebhookSecret = string.Empty,
            SecretKey = "sk_test_unit",
            PublishableKey = "pk_test_unit"
        });

        // Default: EnableSubscriptionTracking = true
        _mockFeatureManager
            .Setup(f => f.IsEnabledAsync(FeatureFlags.EnableSubscriptionTracking))
            .ReturnsAsync(true);

        // Default activity creation succeeds
        _mockActivityService
            .Setup(a => a.CreateAsync(It.IsAny<CRM.Core.Entities.Activity>()))
            .ReturnsAsync(new CRM.Core.Dtos.ActivityDto());

        // Default subscription sync succeeds
        _mockSubscriptionService
            .Setup(s => s.SyncSubscriptionFromStripeAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private StripeWebhookController BuildController(string? webhookSecret = null)
    {
        if (webhookSecret != null)
        {
            _mockOptions.Setup(o => o.Value).Returns(new StripeConfiguration
            {
                WebhookSecret = webhookSecret,
                SecretKey = "sk_test_unit",
                PublishableKey = "pk_test_unit"
            });
        }

        var controller = new StripeWebhookController(
            _mockOptions.Object,
            _mockPaymentService.Object,
            _mockActivityService.Object,
            _mockSubscriptionService.Object,
            _mockFeatureManager.Object,
            _mockLogger.Object);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        return controller;
    }

    private static void SetRequestBody(StripeWebhookController controller, string json)
    {
        var bytes = Encoding.UTF8.GetBytes(json);
        controller.HttpContext.Request.Body = new MemoryStream(bytes);
        controller.HttpContext.Request.ContentType = "application/json";
    }

    private static string BuildSubscriptionEvent(string eventType, string subscriptionId, string status = "active") =>
        JsonSerializer.Serialize(new
        {
            id = $"evt_{Guid.NewGuid():N}",
            type = eventType,
            created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            livemode = false,
            data = new
            {
                @object = new
                {
                    id = subscriptionId,
                    status,
                    customer = "cus_test123"
                }
            }
        });

    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task StripeWebhookController_ShouldCreateSubscription_WhenCustomerSubscriptionCreatedEvent()
    {
        // Arrange
        var controller = BuildController();
        var stripeSubId = $"sub_{Guid.NewGuid():N}";
        var payload = BuildSubscriptionEvent("customer.subscription.created", stripeSubId, "active");
        SetRequestBody(controller, payload);

        // Act
        var result = await controller.HandleWebhook(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mockSubscriptionService.Verify(
            s => s.SyncSubscriptionFromStripeAsync(stripeSubId, "active", It.IsAny<CancellationToken>()),
            Times.Once,
            "Subscription sync should be called with the Stripe subscription ID and status 'active'");
    }

    [Fact]
    public async Task StripeWebhookController_ShouldUpdateStatus_WhenCustomerSubscriptionUpdatedEvent()
    {
        // Arrange
        var controller = BuildController();
        var stripeSubId = $"sub_{Guid.NewGuid():N}";
        var payload = BuildSubscriptionEvent("customer.subscription.updated", stripeSubId, "past_due");
        SetRequestBody(controller, payload);

        // Act
        var result = await controller.HandleWebhook(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mockSubscriptionService.Verify(
            s => s.SyncSubscriptionFromStripeAsync(stripeSubId, "past_due", It.IsAny<CancellationToken>()),
            Times.Once,
            "Subscription sync should be called with status 'past_due' from the webhook event");
    }

    [Fact]
    public async Task StripeWebhookController_ShouldReturn400_WhenInvalidSignature()
    {
        // Arrange — configure a webhook secret so signature validation is active
        var controller = BuildController(webhookSecret: "whsec_realsecret");
        var payload = BuildSubscriptionEvent("customer.subscription.created", "sub_test");
        SetRequestBody(controller, payload);
        // Deliberately omit (or send wrong) Stripe-Signature header
        controller.HttpContext.Request.Headers["Stripe-Signature"] = "t=0,v1=badsig";

        // Act
        var result = await controller.HandleWebhook(CancellationToken.None);

        // Assert — controller returns Unauthorized (401) when signature is invalid
        result.Should().BeOfType<UnauthorizedObjectResult>(
            "a tampered or missing Stripe-Signature must be rejected with 401 Unauthorized");

        _mockSubscriptionService.Verify(
            s => s.SyncSubscriptionFromStripeAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "SyncSubscriptionFromStripeAsync must NOT be called when signature validation fails");
    }
}
