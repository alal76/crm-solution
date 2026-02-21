// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Infrastructure.Providers.SendGrid;

/// <summary>
/// Configuration settings for SendGrid email provider.
/// </summary>
public class SendGridConfiguration
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "Providers:Notifications:SendGrid";

    /// <summary>
    /// SendGrid API Key.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Default sender email address.
    /// </summary>
    public string FromEmail { get; set; } = string.Empty;

    /// <summary>
    /// Default sender name.
    /// </summary>
    public string FromName { get; set; } = string.Empty;

    /// <summary>
    /// Default reply-to email address.
    /// </summary>
    public string? ReplyToEmail { get; set; }

    /// <summary>
    /// Whether to enable click tracking.
    /// </summary>
    public bool EnableClickTracking { get; set; } = true;

    /// <summary>
    /// Whether to enable open tracking.
    /// </summary>
    public bool EnableOpenTracking { get; set; } = true;

    /// <summary>
    /// Whether to enable unsubscribe tracking.
    /// </summary>
    public bool EnableUnsubscribeTracking { get; set; }

    /// <summary>
    /// IP Pool name for sending.
    /// </summary>
    public string? IpPoolName { get; set; }

    /// <summary>
    /// Whether to use sandbox mode (for testing).
    /// </summary>
    public bool SandboxMode { get; set; }

    /// <summary>
    /// Webhook signing key for verifying webhook authenticity.
    /// </summary>
    public string? WebhookSigningKey { get; set; }

    /// <summary>
    /// Maximum batch size for bulk email operations.
    /// SendGrid supports up to 1000 per API call.
    /// </summary>
    public int MaxBatchSize { get; set; } = 1000;

    /// <summary>
    /// Rate limit (emails per second).
    /// </summary>
    public int RateLimitPerSecond { get; set; } = 100;

    /// <summary>
    /// Whether to enable test mode (logs but doesn't send).
    /// </summary>
    public bool TestMode { get; set; }

    /// <summary>
    /// Custom categories to apply to all emails.
    /// </summary>
    public List<string>? DefaultCategories { get; set; }

    /// <summary>
    /// Validates the configuration.
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(ApiKey) && !string.IsNullOrEmpty(FromEmail);
    }
}
