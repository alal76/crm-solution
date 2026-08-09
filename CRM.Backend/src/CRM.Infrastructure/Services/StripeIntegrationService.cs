// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Net.Http;
using CRM.Core.Dtos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using LocalStripeConfiguration = CRM.Infrastructure.Providers.Stripe.StripeConfiguration;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Enhanced Stripe integration service with payment intent and charge methods.
/// Extends webhook-only functionality to include real outbound payment processing via the
/// official Stripe.net SDK (<see cref="global::Stripe.StripeClient"/>).
/// </summary>
/// <remarks>
/// The underlying <see cref="global::Stripe.StripeClient"/> is built from an injected
/// <see cref="HttpClient"/> (via <see cref="global::Stripe.SystemNetHttpClient"/>), which is the
/// mechanism Stripe.net exposes for redirecting outbound requests through a custom
/// <see cref="HttpMessageHandler"/>. This makes the service unit-testable with a fake handler
/// instead of making real calls to api.stripe.com.
/// Inbound webhook signature verification/handling is handled separately by
/// <c>StripeWebhookController</c> and is not part of this service.
/// </remarks>
public class StripeIntegrationService
{
    private readonly LocalStripeConfiguration _config;
    private readonly ILogger<StripeIntegrationService> _logger;
    private readonly IStripeClient _stripeClient;

    public StripeIntegrationService(
        IOptions<LocalStripeConfiguration> config,
        HttpClient httpClient,
        ILogger<StripeIntegrationService> logger)
    {
        _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        if (httpClient is null)
        {
            throw new ArgumentNullException(nameof(httpClient));
        }

        _stripeClient = new StripeClient(
            apiKey: _config.SecretKey,
            httpClient: new SystemNetHttpClient(httpClient));
    }

    /// <summary>
    /// Creates a Stripe PaymentIntent for a given amount.
    /// Use client_secret on frontend to complete payment with Stripe.js.
    /// </summary>
    /// <param name="amount">Amount in smallest currency unit (e.g., cents for USD)</param>
    /// <param name="currency">Three-letter ISO currency code (lowercase)</param>
    /// <param name="customerId">Optional Stripe customer ID</param>
    /// <param name="description">Payment description</param>
    /// <param name="metadata">Additional metadata</param>
    /// <param name="paymentMethodId">
    /// Optional saved Stripe PaymentMethod ID (pm_xxx) to charge immediately. When supplied,
    /// the PaymentIntent is created and confirmed in the same call — used for off-session
    /// charges against a payment method saved on a previous checkout (e.g. dunning retries).
    /// Additive parameter; existing callers that only create a client-confirmed intent are
    /// unaffected because it defaults to null.
    /// </param>
    /// <param name="offSession">
    /// When true (and <paramref name="paymentMethodId"/> is supplied), tells Stripe this is an
    /// off-session/merchant-initiated charge (no customer present), matching Stripe's documented
    /// saved-card/dunning-retry semantics for <c>PaymentIntentCreateOptions.OffSession</c>.
    /// </param>
    /// <returns>PaymentIntent result with client_secret</returns>
    public async Task<PaymentIntentResultDto> CreatePaymentIntentAsync(
        long amount,
        string currency = "usd",
        string? customerId = null,
        string? description = null,
        Dictionary<string, string>? metadata = null,
        string? paymentMethodId = null,
        bool offSession = false)
    {
        try
        {
            _logger.LogInformation("Creating PaymentIntent: Amount={Amount}, Currency={Currency}", amount, currency);

            // Validate inputs
            if (amount <= 0)
            {
                return new PaymentIntentResultDto
                {
                    Success = false,
                    ErrorMessage = "Amount must be greater than zero"
                };
            }

            var options = new PaymentIntentCreateOptions
            {
                Amount = amount,
                Currency = currency,
                Customer = customerId,
                Description = description,
                Metadata = metadata
            };

            if (!string.IsNullOrEmpty(paymentMethodId))
            {
                // Saved-card / off-session charge (e.g. dunning retry): confirm in the same call.
                options.PaymentMethod = paymentMethodId;
                options.Confirm = true;
                if (offSession)
                {
                    options.OffSession = true;
                }
            }

            var service = new PaymentIntentService(_stripeClient);
            var paymentIntent = await service.CreateAsync(options).ConfigureAwait(false);

            _logger.LogInformation("PaymentIntent created: {PaymentIntentId}", paymentIntent.Id);

            return new PaymentIntentResultDto
            {
                PaymentIntentId = paymentIntent.Id,
                ClientSecret = paymentIntent.ClientSecret,
                Status = paymentIntent.Status,
                Amount = paymentIntent.Amount,
                Currency = paymentIntent.Currency,
                Success = true
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error creating PaymentIntent: {ErrorCode}", ex.StripeError?.Code);
            return new PaymentIntentResultDto
            {
                Success = false,
                ErrorMessage = ex.StripeError?.Message ?? ex.Message,
                ErrorCode = ex.StripeError?.Code ?? "stripe_error"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating PaymentIntent");
            return new PaymentIntentResultDto
            {
                Success = false,
                ErrorMessage = ex.Message,
                ErrorCode = "payment_intent_error"
            };
        }
    }

    /// <summary>
    /// Creates a charge using a saved payment token.
    /// </summary>
    /// <param name="amount">Amount to charge (in smallest currency unit)</param>
    /// <param name="token">Payment source token (tok_xxx) or PaymentMethod (pm_xxx)</param>
    /// <param name="currency">Three-letter ISO currency code</param>
    /// <param name="description">Charge description</param>
    /// <param name="metadata">Additional metadata</param>
    /// <returns>Charge result</returns>
    public async Task<ChargeResultDto> CreateChargeAsync(
        long amount,
        string token,
        string currency = "usd",
        string? description = null,
        Dictionary<string, string>? metadata = null)
    {
        try
        {
            _logger.LogInformation("Creating charge: Amount={Amount}, Token={TokenPrefix}...",
                amount, token.Length > 8 ? token.Substring(0, 8) : token);

            // Validate inputs
            if (amount <= 0)
            {
                return new ChargeResultDto
                {
                    Success = false,
                    Status = "failed",
                    ErrorMessage = "Amount must be greater than zero",
                    ErrorCode = "invalid_amount"
                };
            }

            if (string.IsNullOrEmpty(token))
            {
                return new ChargeResultDto
                {
                    Success = false,
                    Status = "failed",
                    ErrorMessage = "Payment token is required",
                    ErrorCode = "missing_token"
                };
            }

            var options = new ChargeCreateOptions
            {
                Amount = amount,
                Currency = currency,
                Source = token,
                Description = description,
                Metadata = metadata
            };

            var service = new ChargeService(_stripeClient);
            var charge = await service.CreateAsync(options).ConfigureAwait(false);

            _logger.LogInformation("Charge created: {ChargeId}", charge.Id);

            return new ChargeResultDto
            {
                ChargeId = charge.Id,
                Success = true,
                Status = charge.Status,
                Amount = charge.Amount / 100m, // Convert from cents to dollars
                Currency = charge.Currency,
                ChargedAt = charge.Created,
                ReceiptUrl = charge.ReceiptUrl
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error creating charge: {ErrorCode}", ex.StripeError?.Code);
            return new ChargeResultDto
            {
                Success = false,
                Status = "failed",
                ErrorMessage = ex.StripeError?.Message ?? ex.Message,
                ErrorCode = ex.StripeError?.Code ?? "stripe_error"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating charge");
            return new ChargeResultDto
            {
                Success = false,
                Status = "failed",
                ErrorMessage = ex.Message,
                ErrorCode = "charge_error"
            };
        }
    }

    /// <summary>
    /// Confirms a PaymentIntent (typically called after customer provides payment details).
    /// </summary>
    /// <param name="paymentIntentId">PaymentIntent ID to confirm</param>
    /// <param name="paymentMethodId">Payment method ID</param>
    /// <returns>Updated PaymentIntent status</returns>
    public async Task<PaymentIntentResultDto> ConfirmPaymentIntentAsync(
        string paymentIntentId,
        string paymentMethodId)
    {
        try
        {
            _logger.LogInformation("Confirming PaymentIntent: {PaymentIntentId}", paymentIntentId);

            var options = new PaymentIntentConfirmOptions
            {
                PaymentMethod = paymentMethodId
            };

            var service = new PaymentIntentService(_stripeClient);
            var paymentIntent = await service.ConfirmAsync(paymentIntentId, options).ConfigureAwait(false);

            return new PaymentIntentResultDto
            {
                PaymentIntentId = paymentIntent.Id,
                ClientSecret = paymentIntent.ClientSecret,
                Status = paymentIntent.Status,
                Amount = paymentIntent.Amount,
                Currency = paymentIntent.Currency,
                Success = true
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error confirming PaymentIntent: {ErrorCode}", ex.StripeError?.Code);
            return new PaymentIntentResultDto
            {
                PaymentIntentId = paymentIntentId,
                Success = false,
                ErrorMessage = ex.StripeError?.Message ?? ex.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming PaymentIntent");
            return new PaymentIntentResultDto
            {
                PaymentIntentId = paymentIntentId,
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Captures a previously authorized PaymentIntent.
    /// </summary>
    /// <param name="paymentIntentId">PaymentIntent ID to capture</param>
    /// <param name="amountToCapture">Optional amount to capture (for partial capture)</param>
    /// <returns>Capture result</returns>
    public async Task<PaymentIntentResultDto> CapturePaymentIntentAsync(
        string paymentIntentId,
        long? amountToCapture = null)
    {
        try
        {
            _logger.LogInformation("Capturing PaymentIntent: {PaymentIntentId}", paymentIntentId);

            var options = new PaymentIntentCaptureOptions();
            if (amountToCapture.HasValue)
            {
                options.AmountToCapture = amountToCapture.Value;
            }

            var service = new PaymentIntentService(_stripeClient);
            var paymentIntent = await service.CaptureAsync(paymentIntentId, options).ConfigureAwait(false);

            return new PaymentIntentResultDto
            {
                PaymentIntentId = paymentIntent.Id,
                ClientSecret = paymentIntent.ClientSecret,
                Status = paymentIntent.Status,
                Amount = paymentIntent.AmountReceived > 0 ? paymentIntent.AmountReceived : paymentIntent.Amount,
                Currency = paymentIntent.Currency,
                Success = true
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error capturing PaymentIntent: {ErrorCode}", ex.StripeError?.Code);
            return new PaymentIntentResultDto
            {
                PaymentIntentId = paymentIntentId,
                Success = false,
                ErrorMessage = ex.StripeError?.Message ?? ex.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error capturing PaymentIntent");
            return new PaymentIntentResultDto
            {
                PaymentIntentId = paymentIntentId,
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Cancels a PaymentIntent.
    /// </summary>
    /// <param name="paymentIntentId">PaymentIntent ID to cancel</param>
    /// <param name="cancellationReason">Reason for cancellation</param>
    /// <returns>Cancellation result</returns>
    public async Task<PaymentIntentResultDto> CancelPaymentIntentAsync(
        string paymentIntentId,
        string? cancellationReason = null)
    {
        try
        {
            _logger.LogInformation("Canceling PaymentIntent: {PaymentIntentId}", paymentIntentId);

            var options = new PaymentIntentCancelOptions
            {
                CancellationReason = cancellationReason
            };

            var service = new PaymentIntentService(_stripeClient);
            var paymentIntent = await service.CancelAsync(paymentIntentId, options).ConfigureAwait(false);

            return new PaymentIntentResultDto
            {
                PaymentIntentId = paymentIntent.Id,
                ClientSecret = paymentIntent.ClientSecret,
                Status = paymentIntent.Status,
                Amount = paymentIntent.Amount,
                Currency = paymentIntent.Currency,
                Success = true
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error canceling PaymentIntent: {ErrorCode}", ex.StripeError?.Code);
            return new PaymentIntentResultDto
            {
                PaymentIntentId = paymentIntentId,
                Success = false,
                ErrorMessage = ex.StripeError?.Message ?? ex.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error canceling PaymentIntent");
            return new PaymentIntentResultDto
            {
                PaymentIntentId = paymentIntentId,
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Creates a refund for a charge or PaymentIntent.
    /// </summary>
    /// <param name="paymentIntentId">PaymentIntent ID to refund</param>
    /// <param name="amount">Amount to refund (null for full refund)</param>
    /// <param name="reason">Refund reason</param>
    /// <returns>Refund result</returns>
    public async Task<ChargeResultDto> CreateRefundAsync(
        string paymentIntentId,
        long? amount = null,
        string? reason = null)
    {
        try
        {
            _logger.LogInformation("Creating refund for: {PaymentIntentId}", paymentIntentId);

            var options = new RefundCreateOptions
            {
                PaymentIntent = paymentIntentId,
                Reason = reason
            };
            if (amount.HasValue)
            {
                options.Amount = amount.Value;
            }

            var service = new RefundService(_stripeClient);
            var refund = await service.CreateAsync(options).ConfigureAwait(false);

            return new ChargeResultDto
            {
                ChargeId = refund.Id,
                Success = true,
                Status = refund.Status,
                Amount = refund.Amount / 100m,
                Currency = refund.Currency,
                ChargedAt = refund.Created
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error creating refund: {ErrorCode}", ex.StripeError?.Code);
            return new ChargeResultDto
            {
                Success = false,
                Status = "failed",
                ErrorMessage = ex.StripeError?.Message ?? ex.Message,
                ErrorCode = ex.StripeError?.Code ?? "refund_error"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating refund");
            return new ChargeResultDto
            {
                Success = false,
                Status = "failed",
                ErrorMessage = ex.Message,
                ErrorCode = "refund_error"
            };
        }
    }

    /// <summary>
    /// Verifies webhook signature for security.
    /// </summary>
    /// <param name="payload">Raw webhook payload</param>
    /// <param name="signature">Stripe-Signature header value</param>
    /// <returns>True if signature is valid</returns>
    /// <remarks>
    /// This is a lightweight local convenience check retained for backward compatibility.
    /// It is not invoked from any controller — real inbound webhook signature verification
    /// (HMAC-SHA256 via Stripe.net's EventUtility) is implemented in
    /// <c>StripeWebhookController</c>, which is out of scope for this change.
    /// </remarks>
    public bool VerifyWebhookSignature(string payload, string signature)
    {
        if (string.IsNullOrEmpty(_config.WebhookSecret))
        {
            _logger.LogWarning("Webhook secret not configured");
            return false;
        }

        // Basic shape validation only — see remarks above.
        return !string.IsNullOrEmpty(signature) && signature.Contains("t=") && signature.Contains("v1=");
    }
}
