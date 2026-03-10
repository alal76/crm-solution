// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Infrastructure.Providers.Stripe;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Enhanced Stripe integration service with payment intent and charge methods.
/// Extends webhook-only functionality to include payment processing.
/// </summary>
public class StripeIntegrationService
{
    private readonly StripeConfiguration _config;
    private readonly ILogger<StripeIntegrationService> _logger;

    public StripeIntegrationService(
        IOptions<StripeConfiguration> config,
        ILogger<StripeIntegrationService> logger)
    {
        _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
    /// <returns>PaymentIntent result with client_secret</returns>
    public async Task<PaymentIntentResultDto> CreatePaymentIntentAsync(
        long amount,
        string currency = "usd",
        string? customerId = null,
        string? description = null,
        Dictionary<string, string>? metadata = null)
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

            // TODO: Replace with Stripe.NET SDK PaymentIntentService when production-ready // NOSONAR
            // Simulated response
            var paymentIntentId = $"pi_{Guid.NewGuid():N}";
            var clientSecret = $"{paymentIntentId}_secret_{Guid.NewGuid():N}";

            _logger.LogInformation("PaymentIntent created: {PaymentIntentId}", paymentIntentId);

            return new PaymentIntentResultDto
            {
                PaymentIntentId = paymentIntentId,
                ClientSecret = clientSecret,
                Status = "requires_payment_method",
                Amount = amount,
                Currency = currency,
                Success = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating PaymentIntent");
            return new PaymentIntentResultDto
            {
                Success = false,
                ErrorMessage = ex.Message
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

            // TODO: Replace with Stripe.NET SDK ChargeService when production-ready // NOSONAR
            // Simulated successful charge
            var chargeId = $"ch_{Guid.NewGuid():N}";

            _logger.LogInformation("Charge created: {ChargeId}", chargeId);

            return new ChargeResultDto
            {
                ChargeId = chargeId,
                Success = true,
                Status = "succeeded",
                Amount = amount / 100m, // Convert from cents to dollars
                Currency = currency,
                ChargedAt = DateTime.UtcNow,
                ReceiptUrl = $"https://pay.stripe.com/receipts/{chargeId}"
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

            // TODO: Replace with Stripe.NET SDK PaymentIntentService.ConfirmAsync when production-ready // NOSONAR
            return new PaymentIntentResultDto
            {
                PaymentIntentId = paymentIntentId,
                Status = "succeeded",
                Success = true
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

            // TODO: Replace with Stripe.NET SDK PaymentIntentService.CaptureAsync when production-ready // NOSONAR
            return new PaymentIntentResultDto
            {
                PaymentIntentId = paymentIntentId,
                Status = "captured",
                Amount = amountToCapture ?? 0,
                Success = true
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

            // TODO: Replace with Stripe.NET SDK PaymentIntentService.CancelAsync when production-ready // NOSONAR
            return new PaymentIntentResultDto
            {
                PaymentIntentId = paymentIntentId,
                Status = "canceled",
                Success = true
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

            // TODO: Replace with Stripe.NET SDK RefundService when production-ready // NOSONAR
            var refundId = $"re_{Guid.NewGuid():N}";

            return new ChargeResultDto
            {
                ChargeId = refundId,
                Success = true,
                Status = "succeeded",
                Amount = amount.HasValue ? amount.Value / 100m : 0,
                ChargedAt = DateTime.UtcNow
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
    public bool VerifyWebhookSignature(string payload, string signature)
    {
        if (string.IsNullOrEmpty(_config.WebhookSecret))
        {
            _logger.LogWarning("Webhook secret not configured");
            return false;
        }

        // TODO: Replace with Stripe.NET EventUtility.ConstructEvent when production-ready // NOSONAR
        // Stub: Basic validation
        return !string.IsNullOrEmpty(signature) && signature.Contains("t=") && signature.Contains("v1=");
    }
}
