// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Infrastructure.Providers.Stripe;

/// <summary>
/// Configuration settings for Stripe payment webhook integration.
/// </summary>
public class StripeConfiguration
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "Providers:Payment:Stripe";

    /// <summary>
    /// Stripe webhook signing secret (whsec_...).
    /// Used to validate incoming webhook signatures.
    /// </summary>
    public string WebhookSecret { get; set; } = string.Empty;

    /// <summary>
    /// Stripe secret API key (sk_...).
    /// Used for server-side API calls if needed.
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Stripe publishable API key (pk_...).
    /// Used by the frontend for Stripe.js initialization.
    /// </summary>
    public string PublishableKey { get; set; } = string.Empty;

    /// <summary>
    /// Maximum allowed age (in seconds) for webhook event timestamps.
    /// Events older than this tolerance are rejected to prevent replay attacks.
    /// Default: 300 seconds (5 minutes).
    /// </summary>
    public int WebhookToleranceSeconds { get; set; } = 300;

    /// <summary>
    /// Stripe API version to use for webhook event parsing.
    /// </summary>
    public string ApiVersion { get; set; } = "2024-06-20";
}
