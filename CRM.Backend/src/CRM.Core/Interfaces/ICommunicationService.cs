using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for managing communication channels and messaging
/// </summary>
public interface ICommunicationService
{
    #region Channels

    /// <summary>
    /// Get all communication channels
    /// </summary>
    Task<IEnumerable<CommunicationChannelInfo>> GetChannelsAsync();

    /// <summary>
    /// Get a channel by ID
    /// </summary>
    Task<CommunicationChannelDetail?> GetChannelByIdAsync(int id);

    /// <summary>
    /// Create a new communication channel
    /// </summary>
    Task<CommunicationChannelDetail> CreateChannelAsync(CommunicationChannelCreateRequest dto);

    /// <summary>
    /// Update a communication channel
    /// </summary>
    Task<CommunicationChannelDetail?> UpdateChannelAsync(int id, CommunicationChannelCreateRequest dto);

    /// <summary>
    /// Delete a communication channel
    /// </summary>
    Task<bool> DeleteChannelAsync(int id);

    /// <summary>
    /// Test a channel connection
    /// </summary>
    Task<bool> TestChannelAsync(int id);

    #endregion

    #region Messages

    /// <summary>
    /// Get messages with filtering
    /// </summary>
    Task<IEnumerable<CommunicationMessage>> GetMessagesAsync(
        int? customerId = null,
        int? channelId = null,
        MessageDirection? direction = null,
        DateTime? fromDate = null,
        DateTime? toDate = null);

    /// <summary>
    /// Get a message by ID
    /// </summary>
    Task<CommunicationMessage?> GetMessageByIdAsync(int id);

    /// <summary>
    /// Send a message
    /// </summary>
    Task<CommunicationMessage> SendMessageAsync(SendMessageRequest request);

    /// <summary>
    /// Get conversation thread
    /// </summary>
    Task<IEnumerable<CommunicationMessage>> GetConversationAsync(int customerId, int? contactId = null);

    #endregion
}

/// <summary>
/// Communication channel info (list view)
/// </summary>
public class CommunicationChannelInfo
{
    public int Id { get; set; }
    public string ChannelType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public bool IsDefault { get; set; }
    public string? SocialUsername { get; set; }
    public string? FromEmail { get; set; }
    public DateTime? LastConnectedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Communication channel detail
/// </summary>
public class CommunicationChannelDetail : CommunicationChannelInfo
{
    public string? SmtpServer { get; set; }
    public int? SmtpPort { get; set; }
    public bool? SmtpUseSsl { get; set; }
    public string? SmtpUsername { get; set; }
    public string? ImapServer { get; set; }
    public int? ImapPort { get; set; }
    public bool? ImapUseSsl { get; set; }
    public string? FromName { get; set; }
    public string? WhatsAppBusinessAccountId { get; set; }
    public string? WhatsAppPhoneNumberId { get; set; }
    public string? SocialAccountId { get; set; }
    public string? WebhookUrl { get; set; }
    public bool? WebhookEnabled { get; set; }
    public string? LastError { get; set; }
}

/// <summary>
/// Communication channel create request
/// </summary>
public class CommunicationChannelCreateRequest
{
    public string ChannelType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public bool IsDefault { get; set; }
    public string? ApiKey { get; set; }
    public string? ApiSecret { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public string? SmtpServer { get; set; }
    public int? SmtpPort { get; set; }
    public bool? SmtpUseSsl { get; set; }
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; }
    public string? ImapServer { get; set; }
    public int? ImapPort { get; set; }
    public bool? ImapUseSsl { get; set; }
    public string? FromEmail { get; set; }
    public string? FromName { get; set; }
    public string? WhatsAppBusinessAccountId { get; set; }
    public string? WhatsAppPhoneNumberId { get; set; }
    public string? WhatsAppVerifyToken { get; set; }
    public string? SocialAccountId { get; set; }
    public string? SocialUsername { get; set; }
    public string? PageAccessToken { get; set; }
}

/// <summary>
/// Send message request
/// </summary>
public class SendMessageRequest
{
    public int ChannelId { get; set; }
    public int CustomerId { get; set; }
    public int? ContactId { get; set; }
    public string? ToEmail { get; set; }
    public string? ToPhone { get; set; }
    public string? Subject { get; set; }
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; }
    public int? UserId { get; set; }
}
