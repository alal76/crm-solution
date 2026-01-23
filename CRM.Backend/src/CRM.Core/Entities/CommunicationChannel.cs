namespace CRM.Core.Entities;

/// <summary>
/// Types of communication channels supported
/// </summary>
public enum ChannelType
{
    Email = 0,
    WhatsApp = 1,
    Twitter = 2,    // X (formerly Twitter)
    Facebook = 3,
    SMS = 4,
    LinkedIn = 5
}

/// <summary>
/// Status of the channel configuration
/// </summary>
public enum ChannelStatus
{
    NotConfigured = 0,
    Configured = 1,
    Connected = 2,
    Error = 3,
    Disabled = 4
}

/// <summary>
/// Communication channel configuration for connecting to external services
/// Stores API credentials and settings for each channel type
/// </summary>
public class CommunicationChannel : BaseEntity
{
    /// <summary>
    /// Type of communication channel
    /// </summary>
    public ChannelType ChannelType { get; set; }
    
    /// <summary>
    /// Display name for this channel configuration
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Current status of the channel
    /// </summary>
    public ChannelStatus Status { get; set; } = ChannelStatus.NotConfigured;
    
    /// <summary>
    /// Whether this channel is enabled for use
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    
    /// <summary>
    /// Whether this is the default channel for its type
    /// </summary>
    public bool IsDefault { get; set; } = false;
    
    #region API Credentials (Encrypted)
    
    /// <summary>
    /// API Key or Access Token
    /// </summary>
    public string? ApiKey { get; set; }
    
    /// <summary>
    /// API Secret or Access Token Secret
    /// </summary>
    public string? ApiSecret { get; set; }
    
    /// <summary>
    /// Client ID for OAuth apps
    /// </summary>
    public string? ClientId { get; set; }
    
    /// <summary>
    /// Client Secret for OAuth apps
    /// </summary>
    public string? ClientSecret { get; set; }
    
    /// <summary>
    /// Access Token for authenticated requests
    /// </summary>
    public string? AccessToken { get; set; }
    
    /// <summary>
    /// Refresh Token for token renewal
    /// </summary>
    public string? RefreshToken { get; set; }
    
    /// <summary>
    /// When the access token expires
    /// </summary>
    public DateTime? TokenExpiresAt { get; set; }
    
    #endregion
    
    #region Email-Specific Settings
    
    /// <summary>
    /// SMTP Server hostname (for Email)
    /// </summary>
    public string? SmtpServer { get; set; }
    
    /// <summary>
    /// SMTP Port (for Email)
    /// </summary>
    public int? SmtpPort { get; set; }
    
    /// <summary>
    /// Whether to use SSL for SMTP
    /// </summary>
    public bool SmtpUseSsl { get; set; } = true;
    
    /// <summary>
    /// SMTP Username
    /// </summary>
    public string? SmtpUsername { get; set; }
    
    /// <summary>
    /// SMTP Password (encrypted)
    /// </summary>
    public string? SmtpPassword { get; set; }
    
    /// <summary>
    /// IMAP Server for incoming mail (for Email)
    /// </summary>
    public string? ImapServer { get; set; }
    
    /// <summary>
    /// IMAP Port
    /// </summary>
    public int? ImapPort { get; set; }
    
    /// <summary>
    /// Whether to use SSL for IMAP
    /// </summary>
    public bool ImapUseSsl { get; set; } = true;
    
    /// <summary>
    /// Default From email address
    /// </summary>
    public string? FromEmail { get; set; }
    
    /// <summary>
    /// Default From name
    /// </summary>
    public string? FromName { get; set; }
    
    #endregion
    
    #region WhatsApp-Specific Settings
    
    /// <summary>
    /// WhatsApp Business Account ID
    /// </summary>
    public string? WhatsAppBusinessAccountId { get; set; }
    
    /// <summary>
    /// WhatsApp Phone Number ID
    /// </summary>
    public string? WhatsAppPhoneNumberId { get; set; }
    
    /// <summary>
    /// WhatsApp Webhook Verify Token
    /// </summary>
    public string? WhatsAppVerifyToken { get; set; }
    
    #endregion
    
    #region Social Media Settings
    
    /// <summary>
    /// Social media account/page ID
    /// </summary>
    public string? SocialAccountId { get; set; }
    
    /// <summary>
    /// Social media username/handle
    /// </summary>
    public string? SocialUsername { get; set; }
    
    /// <summary>
    /// Page Access Token (for Facebook Pages)
    /// </summary>
    public string? PageAccessToken { get; set; }
    
    #endregion
    
    #region Webhook Configuration
    
    /// <summary>
    /// Webhook URL for receiving messages
    /// </summary>
    public string? WebhookUrl { get; set; }
    
    /// <summary>
    /// Webhook secret for verification
    /// </summary>
    public string? WebhookSecret { get; set; }
    
    /// <summary>
    /// Whether webhook is configured and active
    /// </summary>
    public bool WebhookEnabled { get; set; } = false;
    
    #endregion
    
    #region Metadata
    
    /// <summary>
    /// Additional configuration as JSON
    /// </summary>
    public string? ConfigurationJson { get; set; }
    
    /// <summary>
    /// Last time the channel was connected/tested successfully
    /// </summary>
    public DateTime? LastConnectedAt { get; set; }
    
    /// <summary>
    /// Last error message if any
    /// </summary>
    public string? LastError { get; set; }
    
    /// <summary>
    /// User who created this configuration
    /// </summary>
    public int? CreatedByUserId { get; set; }
    
    #endregion
    
    // Navigation
    public virtual ICollection<CommunicationMessage> Messages { get; set; } = new List<CommunicationMessage>();
}
