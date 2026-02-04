// CRM Solution - Pluggable Architecture
// Chatwoot Provider Configuration
// Week 12: Chatwoot Chat Provider - Core Implementation

namespace CRM.Infrastructure.Providers.Chatwoot;

/// <summary>
/// Configuration settings for Chatwoot chat provider.
/// Supports live chat via web widget, WhatsApp, Facebook, SMS, Email, API channels.
/// </summary>
public class ChatwootConfiguration
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "Providers:Chat:Chatwoot";

    /// <summary>
    /// Chatwoot API base URL.
    /// Example: https://app.chatwoot.com or http://localhost:3000 (self-hosted)
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// API access token for authentication.
    /// Generated from Chatwoot Settings > Profile Settings > Access Token.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Chatwoot account ID.
    /// </summary>
    public int AccountId { get; set; } = 1;

    /// <summary>
    /// Default inbox ID for creating conversations.
    /// The inbox determines the channel (Web, WhatsApp, etc.).
    /// </summary>
    public int? DefaultInboxId { get; set; }

    /// <summary>
    /// Inbox ID for API channel (used for programmatic conversations).
    /// </summary>
    public int? ApiInboxId { get; set; }

    /// <summary>
    /// Webhook secret for validating incoming webhooks.
    /// Set in Chatwoot Settings > Integrations > Webhooks.
    /// </summary>
    public string? WebhookSecret { get; set; }

    /// <summary>
    /// Default team ID to assign new conversations.
    /// </summary>
    public int? DefaultTeamId { get; set; }

    /// <summary>
    /// Default agent ID to assign new conversations.
    /// If not set, uses round-robin assignment.
    /// </summary>
    public int? DefaultAgentId { get; set; }

    /// <summary>
    /// Whether to automatically resolve conversations after a period of inactivity.
    /// </summary>
    public bool AutoResolve { get; set; }

    /// <summary>
    /// Hours of inactivity before auto-resolving a conversation.
    /// </summary>
    public int AutoResolveHours { get; set; } = 24;

    /// <summary>
    /// Whether to sync contact custom attributes from CRM.
    /// </summary>
    public bool SyncCustomAttributes { get; set; } = true;

    /// <summary>
    /// Custom attribute mapping from CRM Contact fields to Chatwoot custom attributes.
    /// Key: CRM field name, Value: Chatwoot custom attribute name.
    /// </summary>
    public Dictionary<string, string> CustomAttributeMapping { get; set; } = new()
    {
        { "AccountName", "company" },
        { "AccountTier", "customer_tier" },
        { "LifetimeValue", "lifetime_value" },
        { "OwnerName", "crm_owner" }
    };

    /// <summary>
    /// Timeout in seconds for API requests.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Maximum retries for failed API requests.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Whether this is a self-hosted Chatwoot instance.
    /// </summary>
    public bool IsSelfHosted { get; set; }

    /// <summary>
    /// Validates the configuration.
    /// </summary>
    /// <returns>Tuple of (isValid, errors) where errors is the list of validation messages.</returns>
    public (bool IsValid, IEnumerable<string> Errors) Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(BaseUrl))
        {
            errors.Add("BaseUrl is required");
        }
        else if (!Uri.TryCreate(BaseUrl, UriKind.Absolute, out _))
        {
            errors.Add("BaseUrl must be a valid URL");
        }

        if (string.IsNullOrWhiteSpace(ApiKey))
        {
            errors.Add("ApiKey is required");
        }

        if (AccountId <= 0)
        {
            errors.Add("AccountId must be greater than 0");
        }

        return (errors.Count == 0, errors);
    }
}
