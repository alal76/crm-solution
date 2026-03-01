// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Providers.Stripe;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers.Webhooks;

/// <summary>
/// Webhook controller for Stripe payment events.
/// Handles payment intents, charges, refunds, disputes, and subscription events.
/// </summary>
[ApiController]
[Route("api/webhooks/stripe")]
[AllowAnonymous]
public class StripeWebhookController : CrmControllerBase
{
    private readonly StripeConfiguration _config;
    private readonly IPaymentService _paymentService;
    private readonly IActivityService _activityService;
    private readonly ILogger<StripeWebhookController> _logger;

    public StripeWebhookController(
        IOptions<StripeConfiguration> config,
        IPaymentService paymentService,
        IActivityService activityService,
        ILogger<StripeWebhookController> logger)
    {
        _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
        _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
        _activityService = activityService ?? throw new ArgumentNullException(nameof(activityService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles incoming Stripe webhook events.
    /// Validates the Stripe-Signature header and dispatches events to appropriate handlers.
    /// </summary>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> HandleWebhook(CancellationToken cancellationToken)
    {
        try
        {
            // Read the raw body
            using var reader = new StreamReader(Request.Body, Encoding.UTF8);
            var body = await reader.ReadToEndAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(body))
            {
                _logger.LogWarning("Received empty webhook payload from Stripe");
                return BadRequest("Empty payload");
            }

            // Validate signature if webhook secret is configured
            if (!string.IsNullOrWhiteSpace(_config.WebhookSecret))
            {
                var signatureHeader = Request.Headers["Stripe-Signature"].FirstOrDefault();
                if (!ValidateSignature(body, signatureHeader))
                {
                    _logger.LogWarning("Stripe webhook signature validation failed");
                    return Unauthorized("Invalid signature");
                }
            }

            // Parse the event
            var stripeEvent = ParseEvent(body);
            if (stripeEvent == null)
            {
                _logger.LogWarning("Failed to parse Stripe webhook event");
                return BadRequest("Invalid event payload");
            }

            _logger.LogInformation(
                "Processing Stripe webhook: Type={EventType}, Id={EventId}",
                stripeEvent.Type,
                stripeEvent.Id);

            // Dispatch to event-specific handlers
            await ProcessEventAsync(stripeEvent, cancellationToken);

            return Ok(new { received = true, eventType = stripeEvent.Type, eventId = stripeEvent.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Stripe webhook");
            // Always return 200 to prevent Stripe from retrying on application errors
            return Ok(new { received = true, error = "Processing error logged" });
        }
    }

    /// <summary>
    /// Health check endpoint for Stripe webhook configuration.
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = "healthy",
            provider = "Stripe",
            webhookSecretConfigured = !string.IsNullOrWhiteSpace(_config.WebhookSecret),
            apiVersion = _config.ApiVersion,
            toleranceSeconds = _config.WebhookToleranceSeconds
        });
    }

    #region Signature Validation

    /// <summary>
    /// Validates the Stripe webhook signature using HMAC-SHA256.
    /// Stripe-Signature header format: t=timestamp,v1=signature[,v0=legacy_signature]
    /// Signed payload: "{timestamp}.{body}"
    /// </summary>
    private bool ValidateSignature(string payload, string? signatureHeader)
    {
        if (string.IsNullOrWhiteSpace(signatureHeader))
        {
            _logger.LogWarning("Missing Stripe-Signature header");
            return false;
        }

        try
        {
            // Parse the Stripe-Signature header into key-value pairs
            var elements = signatureHeader
                .Split(',')
                .Select(part => part.Split('=', 2))
                .Where(parts => parts.Length == 2)
                .ToDictionary(parts => parts[0].Trim(), parts => parts[1].Trim());

            if (!elements.TryGetValue("t", out var timestampStr) ||
                !elements.TryGetValue("v1", out var expectedSignature))
            {
                _logger.LogWarning("Stripe-Signature header missing t or v1 components");
                return false;
            }

            // Parse and validate timestamp (protect against replay attacks)
            if (!long.TryParse(timestampStr, out var timestamp))
            {
                _logger.LogWarning("Invalid timestamp in Stripe-Signature header");
                return false;
            }

            var eventTime = DateTimeOffset.FromUnixTimeSeconds(timestamp);
            var tolerance = TimeSpan.FromSeconds(_config.WebhookToleranceSeconds);
            if (DateTimeOffset.UtcNow - eventTime > tolerance)
            {
                _logger.LogWarning(
                    "Stripe webhook timestamp too old: {EventTime} (tolerance: {Tolerance}s)",
                    eventTime,
                    _config.WebhookToleranceSeconds);
                return false;
            }

            // Compute expected signature: HMAC-SHA256(webhook_secret, "{timestamp}.{payload}")
            var signedPayload = $"{timestampStr}.{payload}";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_config.WebhookSecret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signedPayload));
            var computedSignature = Convert.ToHexString(hash).ToLowerInvariant();

            // Constant-time comparison to prevent timing attacks
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(computedSignature),
                Encoding.UTF8.GetBytes(expectedSignature.ToLowerInvariant()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Stripe webhook signature");
            return false;
        }
    }

    #endregion

    #region Event Processing

    private async Task ProcessEventAsync(StripeWebhookEvent stripeEvent, CancellationToken cancellationToken)
    {
        switch (stripeEvent.Type)
        {
            // Payment Intent events
            case "payment_intent.succeeded":
                await HandlePaymentIntentSucceededAsync(stripeEvent, cancellationToken);
                break;
            case "payment_intent.payment_failed":
                await HandlePaymentIntentFailedAsync(stripeEvent, cancellationToken);
                break;
            case "payment_intent.canceled":
                await HandlePaymentIntentCanceledAsync(stripeEvent, cancellationToken);
                break;
            case "payment_intent.requires_action":
                await HandlePaymentIntentRequiresActionAsync(stripeEvent, cancellationToken);
                break;

            // Charge events
            case "charge.succeeded":
                await HandleChargeSucceededAsync(stripeEvent, cancellationToken);
                break;
            case "charge.refunded":
                await HandleChargeRefundedAsync(stripeEvent, cancellationToken);
                break;
            case "charge.dispute.created":
                await HandleDisputeCreatedAsync(stripeEvent, cancellationToken);
                break;
            case "charge.dispute.closed":
                await HandleDisputeClosedAsync(stripeEvent, cancellationToken);
                break;

            // Checkout Session events
            case "checkout.session.completed":
                await HandleCheckoutSessionCompletedAsync(stripeEvent, cancellationToken);
                break;

            // Invoice events (for subscriptions)
            case "invoice.paid":
                await HandleInvoicePaidAsync(stripeEvent, cancellationToken);
                break;
            case "invoice.payment_failed":
                await HandleInvoicePaymentFailedAsync(stripeEvent, cancellationToken);
                break;

            // Subscription events
            case "customer.subscription.created":
            case "customer.subscription.updated":
            case "customer.subscription.deleted":
                await HandleSubscriptionEventAsync(stripeEvent, cancellationToken);
                break;

            default:
                _logger.LogDebug("Unhandled Stripe event type: {EventType}", stripeEvent.Type);
                break;
        }
    }

    private async Task HandlePaymentIntentSucceededAsync(StripeWebhookEvent stripeEvent, CancellationToken cancellationToken)
    {
        var paymentIntentId = GetObjectId(stripeEvent);
        var amount = GetAmount(stripeEvent);
        var currency = GetObjectStringProperty(stripeEvent, "currency");

        _logger.LogInformation(
            "Payment intent succeeded: {PaymentIntentId}, Amount={Amount} {Currency}",
            paymentIntentId, amount, currency);

        var payment = await FindPaymentByTransactionIdAsync(paymentIntentId, cancellationToken);
        if (payment != null)
        {
            await _paymentService.MarkAsCompletedAsync(payment.Id, cancellationToken);
            UpdatePaymentGatewayFields(payment, stripeEvent);
            await _paymentService.UpdateAsync(payment, cancellationToken);
        }

        await CreateActivityAsync(
            "Payment Succeeded",
            $"Stripe payment of {FormatAmount(amount, currency)} completed successfully.",
            paymentIntentId,
            stripeEvent,
            cancellationToken);
    }

    private async Task HandlePaymentIntentFailedAsync(StripeWebhookEvent stripeEvent, CancellationToken cancellationToken)
    {
        var paymentIntentId = GetObjectId(stripeEvent);
        var failureMessage = GetObjectStringProperty(stripeEvent, "last_payment_error", "message") ?? "Payment failed";
        var failureCode = GetObjectStringProperty(stripeEvent, "last_payment_error", "code");

        _logger.LogWarning(
            "Payment intent failed: {PaymentIntentId}, Reason={Reason}, Code={Code}",
            paymentIntentId, failureMessage, failureCode);

        var payment = await FindPaymentByTransactionIdAsync(paymentIntentId, cancellationToken);
        if (payment != null)
        {
            await _paymentService.MarkAsFailedAsync(payment.Id, failureMessage, cancellationToken);
            UpdatePaymentGatewayFields(payment, stripeEvent);
            payment.GatewayResponseCode = failureCode;
            payment.GatewayResponseMessage = failureMessage;
            await _paymentService.UpdateAsync(payment, cancellationToken);
        }

        await CreateActivityAsync(
            "Payment Failed",
            $"Stripe payment failed: {failureMessage}",
            paymentIntentId,
            stripeEvent,
            cancellationToken);
    }

    private async Task HandlePaymentIntentCanceledAsync(StripeWebhookEvent stripeEvent, CancellationToken cancellationToken)
    {
        var paymentIntentId = GetObjectId(stripeEvent);
        var cancellationReason = GetObjectStringProperty(stripeEvent, "cancellation_reason") ?? "Canceled";

        _logger.LogInformation("Payment intent canceled: {PaymentIntentId}, Reason={Reason}", paymentIntentId, cancellationReason);

        var payment = await FindPaymentByTransactionIdAsync(paymentIntentId, cancellationToken);
        if (payment != null)
        {
            await _paymentService.UpdateStatusAsync(payment.Id, PaymentStatus.Cancelled, cancellationToken);
        }

        await CreateActivityAsync(
            "Payment Canceled",
            $"Stripe payment canceled: {cancellationReason}",
            paymentIntentId,
            stripeEvent,
            cancellationToken);
    }

    private async Task HandlePaymentIntentRequiresActionAsync(StripeWebhookEvent stripeEvent, CancellationToken cancellationToken)
    {
        var paymentIntentId = GetObjectId(stripeEvent);

        _logger.LogInformation("Payment intent requires action: {PaymentIntentId}", paymentIntentId);

        var payment = await FindPaymentByTransactionIdAsync(paymentIntentId, cancellationToken);
        if (payment != null)
        {
            await _paymentService.UpdateStatusAsync(payment.Id, PaymentStatus.OnHold, cancellationToken);
        }

        await CreateActivityAsync(
            "Payment Requires Action",
            "Stripe payment requires additional customer action (e.g., 3D Secure authentication).",
            paymentIntentId,
            stripeEvent,
            cancellationToken);
    }

    private async Task HandleChargeSucceededAsync(StripeWebhookEvent stripeEvent, CancellationToken cancellationToken)
    {
        var chargeId = GetObjectId(stripeEvent);
        var paymentIntentId = GetObjectStringProperty(stripeEvent, "payment_intent");
        var amount = GetAmount(stripeEvent);
        var currency = GetObjectStringProperty(stripeEvent, "currency");

        _logger.LogInformation(
            "Charge succeeded: {ChargeId}, PaymentIntent={PaymentIntentId}, Amount={Amount} {Currency}",
            chargeId, paymentIntentId, amount, currency);

        // Try to find by payment_intent first, then by charge id
        var payment = paymentIntentId != null
            ? await FindPaymentByTransactionIdAsync(paymentIntentId, cancellationToken)
            : null;
        payment ??= await FindPaymentByTransactionIdAsync(chargeId, cancellationToken);

        if (payment != null)
        {
            // Update card details from the charge
            var cardBrand = GetObjectStringProperty(stripeEvent, "payment_method_details", "card", "brand");
            var cardLast4 = GetObjectStringProperty(stripeEvent, "payment_method_details", "card", "last4");
            if (!string.IsNullOrEmpty(cardBrand))
                payment.CardBrand = cardBrand;
            if (!string.IsNullOrEmpty(cardLast4))
                payment.CardLast4 = cardLast4;
            payment.GatewayReference = chargeId;
            await _paymentService.UpdateAsync(payment, cancellationToken);
        }

        await CreateActivityAsync(
            "Charge Succeeded",
            $"Stripe charge of {FormatAmount(amount, currency)} completed.",
            chargeId,
            stripeEvent,
            cancellationToken);
    }

    private async Task HandleChargeRefundedAsync(StripeWebhookEvent stripeEvent, CancellationToken cancellationToken)
    {
        var chargeId = GetObjectId(stripeEvent);
        var paymentIntentId = GetObjectStringProperty(stripeEvent, "payment_intent");
        var amountRefunded = GetObjectDecimalProperty(stripeEvent, "amount_refunded");
        var amount = GetAmount(stripeEvent);
        var currency = GetObjectStringProperty(stripeEvent, "currency");
        var refunded = GetObjectBoolProperty(stripeEvent, "refunded");

        _logger.LogInformation(
            "Charge refunded: {ChargeId}, Refunded={AmountRefunded}/{TotalAmount} {Currency}, FullRefund={FullRefund}",
            chargeId, amountRefunded, amount, currency, refunded);

        var payment = paymentIntentId != null
            ? await FindPaymentByTransactionIdAsync(paymentIntentId, cancellationToken)
            : null;
        payment ??= await FindPaymentByTransactionIdAsync(chargeId, cancellationToken);

        if (payment != null)
        {
            var status = refunded
                ? PaymentStatus.Refunded
                : PaymentStatus.PartiallyRefunded;
            await _paymentService.UpdateStatusAsync(payment.Id, status, cancellationToken);

            payment.RefundedAmount = ConvertStripeAmount(amountRefunded);
            payment.RefundReason = "Refunded via Stripe";
            await _paymentService.UpdateAsync(payment, cancellationToken);
        }

        await CreateActivityAsync(
            refunded ? "Payment Fully Refunded" : "Payment Partially Refunded",
            $"Stripe refund of {FormatAmount(amountRefunded, currency)} processed.",
            chargeId,
            stripeEvent,
            cancellationToken);
    }

    private async Task HandleDisputeCreatedAsync(StripeWebhookEvent stripeEvent, CancellationToken cancellationToken)
    {
        var disputeId = GetObjectId(stripeEvent);
        var chargeId = GetObjectStringProperty(stripeEvent, "charge");
        var amount = GetAmount(stripeEvent);
        var currency = GetObjectStringProperty(stripeEvent, "currency");
        var reason = GetObjectStringProperty(stripeEvent, "reason") ?? "Unknown";

        _logger.LogWarning(
            "Dispute created: {DisputeId}, Charge={ChargeId}, Amount={Amount} {Currency}, Reason={Reason}",
            disputeId, chargeId, amount, currency, reason);

        if (!string.IsNullOrEmpty(chargeId))
        {
            var payment = await FindPaymentByTransactionIdAsync(chargeId, cancellationToken);
            if (payment == null)
            {
                // Also try to find by GatewayReference
                payment = await _paymentService.GetByTransactionIdAsync(chargeId, cancellationToken);
            }

            if (payment != null)
            {
                await _paymentService.UpdateStatusAsync(payment.Id, PaymentStatus.Disputed, cancellationToken);
            }
        }

        await CreateActivityAsync(
            "Payment Disputed",
            $"Stripe dispute opened for {FormatAmount(amount, currency)}. Reason: {reason}",
            disputeId,
            stripeEvent,
            cancellationToken);
    }

    private async Task HandleDisputeClosedAsync(StripeWebhookEvent stripeEvent, CancellationToken cancellationToken)
    {
        var disputeId = GetObjectId(stripeEvent);
        var chargeId = GetObjectStringProperty(stripeEvent, "charge");
        var status = GetObjectStringProperty(stripeEvent, "status") ?? "unknown";

        _logger.LogInformation("Dispute closed: {DisputeId}, Charge={ChargeId}, Status={Status}", disputeId, chargeId, status);

        if (!string.IsNullOrEmpty(chargeId))
        {
            var payment = await FindPaymentByTransactionIdAsync(chargeId, cancellationToken);
            if (payment != null)
            {
                // Map Stripe dispute status to payment status
                var paymentStatus = status switch
                {
                    "won" => PaymentStatus.Completed,
                    "lost" => PaymentStatus.Refunded,
                    _ => PaymentStatus.Completed
                };
                await _paymentService.UpdateStatusAsync(payment.Id, paymentStatus, cancellationToken);
            }
        }

        await CreateActivityAsync(
            "Dispute Closed",
            $"Stripe dispute {disputeId} closed with status: {status}",
            disputeId,
            stripeEvent,
            cancellationToken);
    }

    private async Task HandleCheckoutSessionCompletedAsync(StripeWebhookEvent stripeEvent, CancellationToken cancellationToken)
    {
        var sessionId = GetObjectId(stripeEvent);
        var paymentIntentId = GetObjectStringProperty(stripeEvent, "payment_intent");
        var amountTotal = GetObjectDecimalProperty(stripeEvent, "amount_total");
        var currency = GetObjectStringProperty(stripeEvent, "currency");
        var customerEmail = GetObjectStringProperty(stripeEvent, "customer_details", "email");

        _logger.LogInformation(
            "Checkout session completed: {SessionId}, PaymentIntent={PaymentIntentId}, Email={Email}",
            sessionId, paymentIntentId, customerEmail);

        await CreateActivityAsync(
            "Checkout Completed",
            $"Stripe checkout session completed for {FormatAmount(amountTotal, currency)}. Customer: {customerEmail ?? "N/A"}",
            sessionId,
            stripeEvent,
            cancellationToken);
    }

    private async Task HandleInvoicePaidAsync(StripeWebhookEvent stripeEvent, CancellationToken cancellationToken)
    {
        var invoiceId = GetObjectId(stripeEvent);
        var subscriptionId = GetObjectStringProperty(stripeEvent, "subscription");
        var amountPaid = GetObjectDecimalProperty(stripeEvent, "amount_paid");
        var currency = GetObjectStringProperty(stripeEvent, "currency");

        _logger.LogInformation(
            "Invoice paid: {InvoiceId}, Subscription={SubscriptionId}, Amount={Amount} {Currency}",
            invoiceId, subscriptionId, amountPaid, currency);

        await CreateActivityAsync(
            "Invoice Paid",
            $"Stripe invoice paid: {FormatAmount(amountPaid, currency)}. Subscription: {subscriptionId ?? "N/A"}",
            invoiceId,
            stripeEvent,
            cancellationToken);
    }

    private async Task HandleInvoicePaymentFailedAsync(StripeWebhookEvent stripeEvent, CancellationToken cancellationToken)
    {
        var invoiceId = GetObjectId(stripeEvent);
        var subscriptionId = GetObjectStringProperty(stripeEvent, "subscription");
        var attemptCount = GetObjectDecimalProperty(stripeEvent, "attempt_count");

        _logger.LogWarning(
            "Invoice payment failed: {InvoiceId}, Subscription={SubscriptionId}, Attempt={Attempt}",
            invoiceId, subscriptionId, attemptCount);

        await CreateActivityAsync(
            "Invoice Payment Failed",
            $"Stripe invoice payment failed (attempt #{attemptCount}). Subscription: {subscriptionId ?? "N/A"}",
            invoiceId,
            stripeEvent,
            cancellationToken);
    }

    private async Task HandleSubscriptionEventAsync(StripeWebhookEvent stripeEvent, CancellationToken cancellationToken)
    {
        var subscriptionId = GetObjectId(stripeEvent);
        var status = GetObjectStringProperty(stripeEvent, "status");
        var stripeCustomerId = GetObjectStringProperty(stripeEvent, "customer");

        var action = stripeEvent.Type switch
        {
            "customer.subscription.created" => "Created",
            "customer.subscription.updated" => "Updated",
            "customer.subscription.deleted" => "Canceled",
            _ => "Changed"
        };

        _logger.LogInformation(
            "Subscription {Action}: {SubscriptionId}, Status={Status}, StripeCustomerId={StripeCustomerId}",
            action, subscriptionId, status, stripeCustomerId);

        await CreateActivityAsync(
            $"Subscription {action}",
            $"Stripe subscription {subscriptionId} {action.ToLowerInvariant()}. Status: {status ?? "N/A"}",
            subscriptionId,
            stripeEvent,
            cancellationToken);
    }

    #endregion

    #region Helper Methods

    private async Task<Payment?> FindPaymentByTransactionIdAsync(string? transactionId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(transactionId))
            return null;

        try
        {
            return await _paymentService.GetByTransactionIdAsync(transactionId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to find payment by transaction ID: {TransactionId}", transactionId);
            return null;
        }
    }

    private static void UpdatePaymentGatewayFields(Payment payment, StripeWebhookEvent stripeEvent)
    {
        payment.Gateway = "Stripe";
        var rawData = stripeEvent.Data?.GetProperty("object").ToString();
        if (rawData != null && rawData.Length <= 4000)
        {
            payment.GatewayResponseRaw = rawData;
        }
    }

    private async Task CreateActivityAsync(
        string title,
        string description,
        string? referenceId,
        StripeWebhookEvent stripeEvent,
        CancellationToken cancellationToken)
    {
        try
        {
            var activity = new Activity
            {
                ActivityType = ActivityType.PaymentReceived,
                Title = title,
                Description = description,
                Details = JsonSerializer.Serialize(new
                {
                    eventId = stripeEvent.Id,
                    eventType = stripeEvent.Type,
                    referenceId,
                    provider = "Stripe",
                    timestamp = stripeEvent.Created
                }),
                ActivityDate = DateTimeOffset.FromUnixTimeSeconds(stripeEvent.Created).UtcDateTime,
                CreatedAt = DateTime.UtcNow,
                Source = "Stripe",
                IsSystem = true,
                Category = "Payment"
            };

            await _activityService.CreateAsync(activity);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create activity for Stripe event {EventType}", stripeEvent.Type);
        }
    }

    private static StripeWebhookEvent? ParseEvent(string body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;

            return new StripeWebhookEvent
            {
                Id = root.TryGetProperty("id", out var idProp) ? idProp.GetString() ?? string.Empty : string.Empty,
                Type = root.TryGetProperty("type", out var typeProp) ? typeProp.GetString() ?? string.Empty : string.Empty,
                Created = root.TryGetProperty("created", out var createdProp) ? createdProp.GetInt64() : 0,
                LiveMode = root.TryGetProperty("livemode", out var liveProp) && liveProp.GetBoolean(),
                Data = root.TryGetProperty("data", out var dataProp) ? dataProp.Clone() : default
            };
        }
        catch
        {
            return null;
        }
    }

    private static string GetObjectId(StripeWebhookEvent stripeEvent)
    {
        try
        {
            if (stripeEvent.Data?.ValueKind == JsonValueKind.Object &&
                stripeEvent.Data.Value.TryGetProperty("object", out var obj) &&
                obj.TryGetProperty("id", out var idProp))
            {
                return idProp.GetString() ?? string.Empty;
            }
        }
        catch { /* Swallow parse errors */ }

        return string.Empty;
    }

    private static decimal GetAmount(StripeWebhookEvent stripeEvent)
    {
        return GetObjectDecimalProperty(stripeEvent, "amount");
    }

    private static decimal GetObjectDecimalProperty(StripeWebhookEvent stripeEvent, params string[] path)
    {
        try
        {
            if (stripeEvent.Data?.ValueKind != JsonValueKind.Object)
                return 0;

            var current = stripeEvent.Data.Value.GetProperty("object");
            foreach (var segment in path)
            {
                if (!current.TryGetProperty(segment, out current))
                    return 0;
            }

            return current.ValueKind == JsonValueKind.Number ? current.GetDecimal() : 0;
        }
        catch { return 0; }
    }

    private static bool GetObjectBoolProperty(StripeWebhookEvent stripeEvent, string propertyName)
    {
        try
        {
            if (stripeEvent.Data?.ValueKind == JsonValueKind.Object &&
                stripeEvent.Data.Value.TryGetProperty("object", out var obj) &&
                obj.TryGetProperty(propertyName, out var prop))
            {
                return prop.ValueKind == JsonValueKind.True;
            }
        }
        catch { /* Swallow parse errors */ }

        return false;
    }

    private static string? GetObjectStringProperty(StripeWebhookEvent stripeEvent, params string[] path)
    {
        try
        {
            if (stripeEvent.Data?.ValueKind != JsonValueKind.Object)
                return null;

            var current = stripeEvent.Data.Value.GetProperty("object");
            foreach (var segment in path)
            {
                if (!current.TryGetProperty(segment, out current))
                    return null;
            }

            return current.ValueKind == JsonValueKind.String ? current.GetString() : null;
        }
        catch { return null; }
    }

    /// <summary>
    /// Converts a Stripe amount (in smallest currency unit, e.g., cents) to a decimal amount.
    /// </summary>
    private static decimal ConvertStripeAmount(decimal stripeAmount)
    {
        return stripeAmount / 100m;
    }

    /// <summary>
    /// Formats a Stripe amount for display purposes.
    /// </summary>
    private static string FormatAmount(decimal stripeAmount, string? currency)
    {
        var displayAmount = ConvertStripeAmount(stripeAmount);
        var currencyCode = currency?.ToUpperInvariant() ?? "USD";
        return $"{displayAmount:N2} {currencyCode}";
    }

    #endregion

    #region Internal Types

    /// <summary>
    /// Lightweight representation of a Stripe webhook event.
    /// </summary>
    private class StripeWebhookEvent
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public long Created { get; set; }
        public bool LiveMode { get; set; }
        public JsonElement? Data { get; set; }
    }

    #endregion
}
