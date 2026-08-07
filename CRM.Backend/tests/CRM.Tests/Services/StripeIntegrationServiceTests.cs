// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Net;
using System.Net.Http;
using System.Text;
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
///
/// The service wraps the real Stripe.net SDK (<c>Stripe.StripeClient</c>), which is constructed
/// from an injected <see cref="HttpClient"/>. All outbound calls in these tests are redirected
/// through a fake <see cref="HttpMessageHandler"/> (<see cref="StripeMockHandler"/>) that returns
/// canned Stripe-shaped JSON responses — no real network calls are made to api.stripe.com.
///
/// MANDATORY: Written after verifying source for:
///   Class: StripeIntegrationService, Namespace: CRM.Infrastructure.Services
///   Constructor: (IOptions&lt;StripeConfiguration&gt;, HttpClient, ILogger&lt;StripeIntegrationService&gt;)
///   SDK: Stripe.StripeClient(apiKey, httpClient: new SystemNetHttpClient(HttpClient))
/// </summary>
public class StripeIntegrationServiceTests
{
    // ── Factory helpers ─────────────────────────────────────────────────────

    private static StripeConfiguration ValidConfig() => new()
    {
        SecretKey = "sk_test_fake",
        PublishableKey = "pk_test_fake",
        WebhookSecret = "whsec_test_fake",
        WebhookToleranceSeconds = 300,
        ApiVersion = "2024-06-20"
    };

    private static (StripeIntegrationService service, StripeMockHandler handler) CreateService(
        HttpStatusCode responseStatus,
        string responseBody,
        StripeConfiguration? config = null)
    {
        var handler = new StripeMockHandler(responseStatus, responseBody);
        var httpClient = new HttpClient(handler);
        var options = Options.Create(config ?? ValidConfig());
        var logger = new Mock<ILogger<StripeIntegrationService>>();

        var service = new StripeIntegrationService(options, httpClient, logger.Object);
        return (service, handler);
    }

    // ── Constructor Guards ───────────────────────────────────────────────────

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenConfigIsNull()
    {
        var act = () => new StripeIntegrationService(
            null!,
            new HttpClient(),
            new Mock<ILogger<StripeIntegrationService>>().Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("config");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenHttpClientIsNull()
    {
        var options = Options.Create(ValidConfig());

        var act = () => new StripeIntegrationService(
            options,
            null!,
            new Mock<ILogger<StripeIntegrationService>>().Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("httpClient");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        var options = Options.Create(ValidConfig());

        var act = () => new StripeIntegrationService(options, new HttpClient(), null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    // ── CreatePaymentIntentAsync ─────────────────────────────────────────────

    [Fact]
    public async Task CreatePaymentIntentAsync_ShouldReturnSuccess_WhenStripeReturns200()
    {
        const string json = """
        {
          "id": "pi_test_123",
          "object": "payment_intent",
          "amount": 1000,
          "currency": "usd",
          "status": "requires_payment_method",
          "client_secret": "pi_test_123_secret_abc"
        }
        """;
        var (service, handler) = CreateService(HttpStatusCode.OK, json);

        var result = await service.CreatePaymentIntentAsync(1000, "usd");

        result.Success.Should().BeTrue();
        result.Amount.Should().Be(1000);
        result.Currency.Should().Be("usd");
        result.PaymentIntentId.Should().Be("pi_test_123");
        result.ClientSecret.Should().Be("pi_test_123_secret_abc");
        result.Status.Should().Be("requires_payment_method");
        handler.Requests.Should().HaveCount(1);
        handler.Requests[0].RequestUri!.ToString().Should().Contain("/v1/payment_intents");
    }

    [Fact]
    public async Task CreatePaymentIntentAsync_ShouldReturnFailure_WhenAmountIsZero_WithoutCallingStripe()
    {
        var (service, handler) = CreateService(HttpStatusCode.OK, "{}");

        var result = await service.CreatePaymentIntentAsync(0, "usd");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
        handler.Requests.Should().BeEmpty("zero amount should be rejected locally before any Stripe call");
    }

    [Fact]
    public async Task CreatePaymentIntentAsync_ShouldReturnFailure_WhenAmountIsNegative()
    {
        var (service, _) = CreateService(HttpStatusCode.OK, "{}");

        var result = await service.CreatePaymentIntentAsync(-500, "usd");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("greater than zero");
    }

    [Fact]
    public async Task CreatePaymentIntentAsync_ShouldReturnFailure_WhenStripeReturnsCardError()
    {
        const string json = """
        {
          "error": {
            "type": "card_error",
            "code": "card_declined",
            "decline_code": "generic_decline",
            "message": "Your card was declined."
          }
        }
        """;
        var (service, _) = CreateService(HttpStatusCode.PaymentRequired, json);

        var result = await service.CreatePaymentIntentAsync(1000, "usd");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("declined");
    }

    // ── CreateChargeAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task CreateChargeAsync_ShouldReturnSuccess_WhenStripeReturns200()
    {
        const string json = """
        {
          "id": "ch_test_123",
          "object": "charge",
          "amount": 5000,
          "currency": "usd",
          "status": "succeeded",
          "paid": true,
          "captured": true,
          "created": 1700000000,
          "receipt_url": "https://pay.stripe.com/receipts/ch_test_123"
        }
        """;
        var (service, handler) = CreateService(HttpStatusCode.OK, json);

        var result = await service.CreateChargeAsync(5000, "tok_visa", "usd", "Test charge");

        result.Success.Should().BeTrue();
        result.Status.Should().Be("succeeded");
        result.ChargeId.Should().Be("ch_test_123");
        result.Amount.Should().Be(50m); // 5000 cents -> $50.00
        result.Currency.Should().Be("usd");
        result.ReceiptUrl.Should().Be("https://pay.stripe.com/receipts/ch_test_123");
        handler.Requests.Should().HaveCount(1);
        handler.Requests[0].RequestUri!.ToString().Should().Contain("/v1/charges");
    }

    [Fact]
    public async Task CreateChargeAsync_ShouldReturnFailure_WhenAmountIsZero_WithoutCallingStripe()
    {
        var (service, handler) = CreateService(HttpStatusCode.OK, "{}");

        var result = await service.CreateChargeAsync(0, "tok_visa", "usd");

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("invalid_amount");
        handler.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateChargeAsync_ShouldReturnFailure_WhenTokenMissing()
    {
        var (service, handler) = CreateService(HttpStatusCode.OK, "{}");

        var result = await service.CreateChargeAsync(1000, string.Empty, "usd");

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("missing_token");
        handler.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateChargeAsync_ShouldReturnFailure_WhenStripeReturnsError()
    {
        const string json = """
        {
          "error": {
            "type": "invalid_request_error",
            "code": "parameter_missing",
            "message": "Missing required param: source."
          }
        }
        """;
        var (service, _) = CreateService(HttpStatusCode.BadRequest, json);

        var result = await service.CreateChargeAsync(1000, "tok_visa", "usd");

        result.Success.Should().BeFalse();
        result.Status.Should().Be("failed");
        result.ErrorCode.Should().Be("parameter_missing");
        result.ErrorMessage.Should().Contain("Missing required param");
    }

    // ── ConfirmPaymentIntentAsync ────────────────────────────────────────────

    [Fact]
    public async Task ConfirmPaymentIntentAsync_ShouldReturnSucceeded_WhenStripeReturns200()
    {
        const string json = """
        {
          "id": "pi_test_123",
          "object": "payment_intent",
          "status": "succeeded",
          "amount": 1000,
          "currency": "usd"
        }
        """;
        var (service, _) = CreateService(HttpStatusCode.OK, json);

        var result = await service.ConfirmPaymentIntentAsync("pi_test_123", "pm_card_visa");

        result.Success.Should().BeTrue();
        result.PaymentIntentId.Should().Be("pi_test_123");
        result.Status.Should().Be("succeeded");
    }

    // ── CapturePaymentIntentAsync ────────────────────────────────────────────

    [Fact]
    public async Task CapturePaymentIntentAsync_ShouldReturnSucceeded_WhenStripeReturns200()
    {
        const string json = """
        {
          "id": "pi_test_456",
          "object": "payment_intent",
          "status": "succeeded",
          "amount": 2000,
          "amount_received": 2000,
          "currency": "usd"
        }
        """;
        var (service, handler) = CreateService(HttpStatusCode.OK, json);

        var result = await service.CapturePaymentIntentAsync("pi_test_456");

        result.Success.Should().BeTrue();
        result.Status.Should().Be("succeeded");
        result.PaymentIntentId.Should().Be("pi_test_456");
        result.Amount.Should().Be(2000);
        handler.Requests[0].RequestUri!.ToString().Should().Contain("/capture");
    }

    [Fact]
    public async Task CapturePaymentIntentAsync_ShouldReturnFailure_WhenStripeReturnsError()
    {
        const string json = """
        {
          "error": {
            "type": "invalid_request_error",
            "code": "payment_intent_unexpected_state",
            "message": "This PaymentIntent could not be captured."
          }
        }
        """;
        var (service, _) = CreateService(HttpStatusCode.BadRequest, json);

        var result = await service.CapturePaymentIntentAsync("pi_test_456");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("could not be captured");
    }

    // ── CancelPaymentIntentAsync ─────────────────────────────────────────────

    [Fact]
    public async Task CancelPaymentIntentAsync_ShouldReturnCanceled_WhenStripeReturns200()
    {
        const string json = """
        {
          "id": "pi_test_789",
          "object": "payment_intent",
          "status": "canceled",
          "amount": 1000,
          "currency": "usd"
        }
        """;
        var (service, _) = CreateService(HttpStatusCode.OK, json);

        var result = await service.CancelPaymentIntentAsync("pi_test_789", "requested_by_customer");

        result.Success.Should().BeTrue();
        result.Status.Should().Be("canceled");
        result.PaymentIntentId.Should().Be("pi_test_789");
    }

    // ── CreateRefundAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task CreateRefundAsync_ShouldReturnSucceeded_WhenStripeReturns200()
    {
        const string json = """
        {
          "id": "re_test_abc",
          "object": "refund",
          "status": "succeeded",
          "amount": 1000,
          "currency": "usd",
          "created": 1700000000
        }
        """;
        var (service, handler) = CreateService(HttpStatusCode.OK, json);

        var result = await service.CreateRefundAsync("pi_test_abc", 1000, "duplicate");

        result.Success.Should().BeTrue();
        result.Status.Should().Be("succeeded");
        result.ChargeId.Should().Be("re_test_abc");
        result.Amount.Should().Be(10m); // 1000 cents -> $10.00
        handler.Requests[0].RequestUri!.ToString().Should().Contain("/v1/refunds");
    }

    [Fact]
    public async Task CreateRefundAsync_ShouldReturnFailure_WhenStripeReturnsError()
    {
        const string json = """
        {
          "error": {
            "type": "invalid_request_error",
            "code": "charge_already_refunded",
            "message": "Charge has already been refunded."
          }
        }
        """;
        var (service, _) = CreateService(HttpStatusCode.BadRequest, json);

        var result = await service.CreateRefundAsync("pi_test_abc");

        result.Success.Should().BeFalse();
        result.Status.Should().Be("failed");
        result.ErrorCode.Should().Be("charge_already_refunded");
        result.ErrorMessage.Should().Contain("already been refunded");
    }

    // ── VerifyWebhookSignature (unchanged local stub — real verification is in StripeWebhookController) ──

    [Fact]
    public void VerifyWebhookSignature_ReturnsFalse_WhenSecretNotConfigured()
    {
        var configWithoutWebhookSecret = ValidConfig();
        configWithoutWebhookSecret.WebhookSecret = string.Empty;
        var (service, _) = CreateService(HttpStatusCode.OK, "{}", configWithoutWebhookSecret);

        var result = service.VerifyWebhookSignature("payload", "t=123,v1=abc");

        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyWebhookSignature_ReturnsTrue_WhenSignatureShapeIsValid()
    {
        var (service, _) = CreateService(HttpStatusCode.OK, "{}");

        var result = service.VerifyWebhookSignature("payload", "t=123,v1=abc");

        result.Should().BeTrue();
    }
}

// ── Private handler helper for Stripe tests ─────────────────────────────────

/// <summary>
/// Fake HttpMessageHandler that captures outbound requests and returns a single canned
/// response for every call. Used so StripeIntegrationService tests never touch the network.
/// </summary>
internal sealed class StripeMockHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _status;
    private readonly string _body;

    public List<HttpRequestMessage> Requests { get; } = new();

    public StripeMockHandler(HttpStatusCode status, string body)
    {
        _status = status;
        _body = body;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request);
        return Task.FromResult(new HttpResponseMessage(_status)
        {
            Content = new StringContent(_body, Encoding.UTF8, "application/json")
        });
    }
}
