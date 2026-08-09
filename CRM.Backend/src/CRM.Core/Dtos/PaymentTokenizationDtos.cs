// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Dtos;

/// <summary>
/// DTO for payment tokenization request.
/// </summary>
public class PaymentTokenizationRequestDto
{
    /// <summary>Card number (will be tokenized)</summary>
    public string CardNumber { get; set; } = string.Empty;

    /// <summary>Card expiration month (1-12)</summary>
    public int ExpirationMonth { get; set; }

    /// <summary>Card expiration year (4 digits)</summary>
    public int ExpirationYear { get; set; }

    /// <summary>Card security code (CVV/CVC)</summary>
    public string SecurityCode { get; set; } = string.Empty;

    /// <summary>Cardholder name</summary>
    public string CardholderName { get; set; } = string.Empty;

    /// <summary>Billing address line 1</summary>
    public string? BillingAddressLine1 { get; set; }

    /// <summary>Billing address line 2</summary>
    public string? BillingAddressLine2 { get; set; }

    /// <summary>Billing city</summary>
    public string? BillingCity { get; set; }

    /// <summary>Billing state/province</summary>
    public string? BillingState { get; set; }

    /// <summary>Billing postal code</summary>
    public string? BillingPostalCode { get; set; }

    /// <summary>Billing country (ISO 2-letter code)</summary>
    public string? BillingCountry { get; set; }

    /// <summary>Customer ID for association</summary>
    public int? CustomerId { get; set; }
}

/// <summary>
/// DTO for payment tokenization result.
/// </summary>
public class PaymentTokenizationResultDto
{
    /// <summary>Payment token (safe to store)</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>Last four digits of card</summary>
    public string LastFourDigits { get; set; } = string.Empty;

    /// <summary>Card brand (Visa, MasterCard, etc.)</summary>
    public string CardBrand { get; set; } = string.Empty;

    /// <summary>Expiration month</summary>
    public int ExpirationMonth { get; set; }

    /// <summary>Expiration year</summary>
    public int ExpirationYear { get; set; }

    /// <summary>Token creation timestamp</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Whether tokenization was successful</summary>
    public bool Success { get; set; }

    /// <summary>Error message if failed</summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// DTO for charging a payment token.
/// </summary>
public class ChargeTokenRequestDto
{
    /// <summary>Payment token to charge</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>Amount to charge</summary>
    public decimal Amount { get; set; }

    /// <summary>Currency code (USD, EUR, etc.)</summary>
    public string Currency { get; set; } = "USD";

    /// <summary>Description of the charge</summary>
    public string? Description { get; set; }

    /// <summary>Order ID for reference</summary>
    public int? OrderId { get; set; }

    /// <summary>Invoice ID for reference</summary>
    public int? InvoiceId { get; set; }

    /// <summary>Customer ID for reference</summary>
    public int? CustomerId { get; set; }

    /// <summary>Idempotency key to prevent duplicate charges</summary>
    public string? IdempotencyKey { get; set; }
}

/// <summary>
/// DTO for charge result.
/// </summary>
public class ChargeResultDto
{
    /// <summary>Charge/Transaction ID</summary>
    public string ChargeId { get; set; } = string.Empty;

    /// <summary>Whether charge was successful</summary>
    public bool Success { get; set; }

    /// <summary>Charge status (succeeded, pending, failed)</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Amount charged</summary>
    public decimal Amount { get; set; }

    /// <summary>Currency charged</summary>
    public string Currency { get; set; } = "USD";

    /// <summary>Error message if failed</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Error code if failed</summary>
    public string? ErrorCode { get; set; }

    /// <summary>Timestamp of the charge</summary>
    public DateTime ChargedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Receipt URL</summary>
    public string? ReceiptUrl { get; set; }
}

/// <summary>
/// DTO for creating a Stripe payment intent.
/// </summary>
public class CreatePaymentIntentRequestDto
{
    /// <summary>Amount in smallest currency unit (cents for USD)</summary>
    public long Amount { get; set; }

    /// <summary>Three-letter ISO currency code</summary>
    public string Currency { get; set; } = "usd";

    /// <summary>Stripe customer ID</summary>
    public string? CustomerId { get; set; }

    /// <summary>Description for the payment</summary>
    public string? Description { get; set; }

    /// <summary>Metadata key-value pairs</summary>
    public Dictionary<string, string>? Metadata { get; set; }

    /// <summary>Whether to capture immediately or authorize only</summary>
    public bool CaptureImmediately { get; set; } = true;
}

/// <summary>
/// DTO for payment intent result.
/// </summary>
public class PaymentIntentResultDto
{
    /// <summary>Payment intent ID</summary>
    public string PaymentIntentId { get; set; } = string.Empty;

    /// <summary>Client secret for frontend confirmation</summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>Payment intent status</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Amount in smallest currency unit</summary>
    public long Amount { get; set; }

    /// <summary>Currency code</summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>Whether creation was successful</summary>
    public bool Success { get; set; }

    /// <summary>Error message if failed</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Stripe error code if failed (e.g. "card_declined", "insufficient_funds")</summary>
    public string? ErrorCode { get; set; }
}
