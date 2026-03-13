// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Infrastructure.Providers.Stripe;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for StripeIntegrationService.
/// No real Stripe credentials are used — all operations are simulated in the service.
/// </summary>
public class StripeIntegrationServiceTests
{
    private readonly StripeIntegrationService _service;

    public StripeIntegrationServiceTests()
    {
        var config = Options.Create(new StripeConfiguration
        {
            SecretKey = "sk_test_fake",
            PublishableKey = "pk_test_fake",
            WebhookSecret = "whsec_test_fake",
            WebhookToleranceSeconds = 300,
            ApiVersion = "2024-06-20"
        });

        var logger = new Mock<ILogger<StripeIntegrationService>>().Object;
        _service = new StripeIntegrationService(config, logger);
    }

    // ── CreatePaymentIntentAsync ─────────────────────────────────────────────

    [Fact]
    public async Task CreatePaymentIntentAsync_ShouldReturnSuccess_WhenAmountIsPositive()
    {
        var result = await _service.CreatePaymentIntentAsync(1000, "usd");

        result.Success.Should().BeTrue();
        result.Amount.Should().Be(1000);
        result.Currency.Should().Be("usd");
        result.PaymentIntentId.Should().StartWith("pi_");
        result.ClientSecret.Should().NotBeNullOrEmpty();
        result.Status.Should().Be("requires_payment_method");
    }

    [Fact]
    public async Task CreatePaymentIntentAsync_ShouldReturnFailure_WhenAmountIsZero()
    {
        var result = await _service.CreatePaymentIntentAsync(0, "usd");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreatePaymentIntentAsync_ShouldReturnFailure_WhenAmountIsNegative()
    {
        var result = await _service.CreatePaymentIntentAsync(-500, "usd");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("greater than zero");
    }

    // ── CreateChargeAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task CreateChargeAsync_ShouldReturnSuccess_WhenAmountAndTokenAreValid()
    {
        var result = await _service.CreateChargeAsync(5000, "tok_visa", "usd", "Test charge");

        result.Success.Should().BeTrue();
        result.Status.Should().Be("succeeded");
        result.ChargeId.Should().StartWith("ch_");
        result.Amount.Should().Be(50m); // 5000 cents → $50.00
        result.Currency.Should().Be("usd");
        result.ReceiptUrl.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateChargeAsync_ShouldReturnFailure_WhenAmountIsZero()
    {
        var result = await _service.CreateChargeAsync(0, "tok_visa", "usd");

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("invalid_amount");
    }

    // ── ConfirmPaymentIntentAsync ────────────────────────────────────────────

    [Fact]
    public async Task ConfirmPaymentIntentAsync_ShouldReturnSucceeded_WhenIntentIdIsProvided()
    {
        var result = await _service.ConfirmPaymentIntentAsync("pi_test_123", "pm_card_visa");

        result.Success.Should().BeTrue();
        result.PaymentIntentId.Should().Be("pi_test_123");
        result.Status.Should().Be("succeeded");
    }

    // ── CapturePaymentIntentAsync ────────────────────────────────────────────

    [Fact]
    public async Task CapturePaymentIntentAsync_ShouldReturnCaptured_WhenIntentIdIsProvided()
    {
        var result = await _service.CapturePaymentIntentAsync("pi_test_456");

        result.Success.Should().BeTrue();
        result.Status.Should().Be("captured");
        result.PaymentIntentId.Should().Be("pi_test_456");
    }

    // ── CancelPaymentIntentAsync ─────────────────────────────────────────────

    [Fact]
    public async Task CancelPaymentIntentAsync_ShouldReturnCanceled_WhenIntentIdIsProvided()
    {
        var result = await _service.CancelPaymentIntentAsync("pi_test_789", "requested_by_customer");

        result.Success.Should().BeTrue();
        result.Status.Should().Be("canceled");
        result.PaymentIntentId.Should().Be("pi_test_789");
    }

    // ── CreateRefundAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task CreateRefundAsync_ShouldReturnSucceeded_WhenIntentIdIsProvided()
    {
        var result = await _service.CreateRefundAsync("pi_test_abc", 1000, "duplicate");

        result.Success.Should().BeTrue();
        result.Status.Should().Be("succeeded");
        result.ChargeId.Should().StartWith("re_");
        result.Amount.Should().Be(10m); // 1000 cents → $10.00
    }
}
