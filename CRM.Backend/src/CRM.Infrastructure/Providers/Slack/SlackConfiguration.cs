// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Infrastructure.Providers.Slack;

/// <summary>
/// Configuration settings for the Slack notification provider.
/// Binds to the Providers:Notifications:Slack section in appsettings.json.
/// Uses Slack Incoming Webhooks for Block Kit message delivery.
/// </summary>
public class SlackConfiguration
{
    /// <summary>
    /// The Slack Incoming Webhook URL.
    /// Create via: Slack App → Incoming Webhooks → Add New Webhook to Workspace.
    /// Example: https://hooks.slack.com/services/T.../B.../xxx
    /// </summary>
    public string WebhookUrl { get; set; } = string.Empty;

    /// <summary>
    /// The Slack channel to post to (overrides webhook default channel).
    /// Example: #crm-notifications
    /// Leave empty to use the webhook's default channel.
    /// </summary>
    public string? Channel { get; set; }

    /// <summary>
    /// Display name used as the bot username in Slack.
    /// </summary>
    public string Username { get; set; } = "CRM System";

    /// <summary>
    /// Emoji icon for the bot (e.g., ":bell:", ":robot_face:").
    /// </summary>
    public string IconEmoji { get; set; } = ":bell:";

    /// <summary>
    /// HTTP request timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Returns true if the provider is properly configured with a webhook URL.
    /// </summary>
    public bool IsValid() => !string.IsNullOrWhiteSpace(WebhookUrl);
}
