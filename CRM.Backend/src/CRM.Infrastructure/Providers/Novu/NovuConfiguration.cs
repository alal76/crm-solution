// CRM Solution - Novu Provider Configuration
// Phase 2 Week 9: Configuration for Novu notification service
// Part of the Pluggable Architecture implementation

namespace CRM.Infrastructure.Providers.Novu;

/// <summary>
/// Configuration settings for the Novu notification provider.
/// Binds to the Providers:Notifications:Novu section in appsettings.json.
/// </summary>
public class NovuConfiguration
{
    /// <summary>
    /// The Novu API URL. Defaults to cloud Novu.
    /// For self-hosted: http://localhost:3000/v1 or your Novu server URL.
    /// </summary>
    public string Url { get; set; } = "https://api.novu.co/v1";

    /// <summary>
    /// The Novu API key from your Novu dashboard.
    /// Found in Settings > API Keys.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Application identifier for this CRM instance.
    /// Used to distinguish different deployments.
    /// </summary>
    public string ApplicationId { get; set; } = "crm";

    /// <summary>
    /// Timeout for API calls in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Whether to use the self-hosted Novu instance.
    /// When true, uses the Url configuration; when false, uses cloud Novu.
    /// </summary>
    public bool UseSelfHosted { get; set; } = false;

    /// <summary>
    /// Default workflow/template identifier for email notifications.
    /// This should match a workflow created in Novu dashboard.
    /// </summary>
    public string EmailWorkflowId { get; set; } = "email-notification";

    /// <summary>
    /// Default workflow/template identifier for SMS notifications.
    /// </summary>
    public string SmsWorkflowId { get; set; } = "sms-notification";

    /// <summary>
    /// Default workflow/template identifier for push notifications.
    /// </summary>
    public string PushWorkflowId { get; set; } = "push-notification";

    /// <summary>
    /// Default workflow/template identifier for in-app notifications.
    /// </summary>
    public string InAppWorkflowId { get; set; } = "inapp-notification";

    /// <summary>
    /// Default workflow/template identifier for multi-channel notifications.
    /// </summary>
    public string MultiChannelWorkflowId { get; set; } = "multi-channel-notification";

    /// <summary>
    /// Enable detailed logging of Novu API calls.
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = false;

    /// <summary>
    /// Maximum number of retry attempts for failed API calls.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Validates the configuration settings.
    /// </summary>
    /// <returns>True if configuration is valid.</returns>
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(ApiKey) && !string.IsNullOrEmpty(Url);
    }
}
