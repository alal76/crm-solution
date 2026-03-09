// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Configuration;

/// <summary>
/// Configuration options for the Calendly scheduling webhook integration.
/// Binds to <c>Integrations:Calendly</c> in appsettings.json.
/// Implements INT-004.
/// </summary>
public class CalendlyOptions
{
    /// <summary>Configuration section path in appsettings.json.</summary>
    public const string SectionName = "Integrations:Calendly";

    /// <summary>Set to true to activate the Calendly integration.</summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// HMAC-SHA256 signing key provided by Calendly for webhook signature validation.
    /// When empty, signature validation is skipped (development convenience).
    /// </summary>
    public string WebhookSigningKey { get; set; } = string.Empty;

    /// <summary>
    /// Personal Access Token for making outbound Calendly API calls
    /// (e.g., webhook registration, retrieving upcoming events).
    /// </summary>
    public string PersonalAccessToken { get; set; } = string.Empty;
}
