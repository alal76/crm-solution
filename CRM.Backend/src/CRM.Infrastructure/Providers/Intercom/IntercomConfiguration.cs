// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Infrastructure.Providers.Intercom;

/// <summary>
/// Configuration settings for Intercom chat provider.
/// Intercom is a customer messaging platform that supports live chat, email, and product tours.
/// API Reference: https://developers.intercom.com/docs/references/rest-api/api.intercom.io/
/// </summary>
public class IntercomConfiguration
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "Providers:Chat:Intercom";

    /// <summary>
    /// Intercom API base URL.
    /// Default: https://api.intercom.io
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.intercom.io";

    /// <summary>
    /// Intercom Access Token for API authentication.
    /// Generated from Intercom Developer Hub > Authentication.
    /// Required scope: read and write for contacts, conversations, and messages.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Intercom App ID for widget identification.
    /// Found in Intercom Settings > Installation.
    /// </summary>
    public string AppId { get; set; } = string.Empty;

    /// <summary>
    /// Webhook secret for validating incoming webhook signatures.
    /// Set in Intercom Developer Hub > Webhooks.
    /// </summary>
    public string? WebhookSecret { get; set; }

    /// <summary>
    /// Default admin/operator ID to assign conversations.
    /// If not set, conversations go to unassigned.
    /// </summary>
    public string? DefaultAdminId { get; set; }

    /// <summary>
    /// Default team ID for routing conversations.
    /// </summary>
    public string? DefaultTeamId { get; set; }

    /// <summary>
    /// API version to use. Default is the latest stable.
    /// Format: Intercom-Version header (e.g., "2.11")
    /// </summary>
    public string ApiVersion { get; set; } = "2.11";

    /// <summary>
    /// HTTP request timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Maximum retries for transient failures.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Whether to sync custom attributes from CRM to Intercom.
    /// </summary>
    public bool SyncCustomAttributes { get; set; } = true;

    /// <summary>
    /// Custom attribute mapping from CRM Contact fields to Intercom custom attributes.
    /// Key: CRM field name, Value: Intercom custom attribute name.
    /// </summary>
    public Dictionary<string, string> CustomAttributeMapping { get; set; } = new()
    {
        { "AccountTier", "account_tier" },
        { "LifetimeValue", "lifetime_value" },
        { "Industry", "industry" }
    };

    /// <summary>
    /// Whether to create companies in Intercom from CRM accounts.
    /// </summary>
    public bool CreateCompanies { get; set; } = true;

    /// <summary>
    /// Validates the configuration and returns validation errors.
    /// </summary>
    /// <returns>Tuple of (IsValid, ValidationErrors).</returns>
    public (bool IsValid, IEnumerable<string> Errors) Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(AccessToken))
        {
            errors.Add("Intercom AccessToken is required");
        }

        if (string.IsNullOrWhiteSpace(AppId))
        {
            errors.Add("Intercom AppId is required");
        }

        if (string.IsNullOrWhiteSpace(BaseUrl))
        {
            errors.Add("Intercom BaseUrl is required");
        }

        if (TimeoutSeconds <= 0)
        {
            errors.Add("Intercom TimeoutSeconds must be positive");
        }

        return (errors.Count == 0, errors);
    }
}
