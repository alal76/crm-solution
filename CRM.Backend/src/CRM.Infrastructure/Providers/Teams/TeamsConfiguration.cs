// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Infrastructure.Providers.Teams;

/// <summary>
/// Configuration settings for the Microsoft Teams notification provider.
/// Binds to the Providers:Notifications:Teams section in appsettings.json.
/// Uses Teams Incoming Webhook for Adaptive Card delivery.
/// </summary>
public class TeamsConfiguration
{
    /// <summary>
    /// The Teams Incoming Webhook URL.
    /// Create via: Channel → Connectors → Incoming Webhook.
    /// Example: https://xxx.webhook.office.com/webhookb2/...
    /// </summary>
    public string WebhookUrl { get; set; } = string.Empty;

    /// <summary>
    /// HTTP request timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Optional theme color for Adaptive Card header (hex without #).
    /// Defaults to CRM blue.
    /// </summary>
    public string ThemeColor { get; set; } = "0078D4";

    /// <summary>
    /// Whether to include a timestamp footer on Adaptive Cards.
    /// </summary>
    public bool IncludeTimestamp { get; set; } = true;

    /// <summary>
    /// Returns true if the provider is properly configured with a webhook URL.
    /// </summary>
    public bool IsValid() => !string.IsNullOrWhiteSpace(WebhookUrl);
}
