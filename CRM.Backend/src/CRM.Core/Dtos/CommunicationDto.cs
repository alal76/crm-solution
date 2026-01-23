namespace CRM.Core.Dtos;

using CRM.Core.Entities;

#region Channel DTOs

/// <summary>
/// DTO for listing communication channels
/// </summary>
public class CommunicationChannelListDto
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
/// DTO for channel details
/// </summary>
public class CommunicationChannelDto
{
    public int Id { get; set; }
    public string ChannelType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public bool IsDefault { get; set; }
    
    // Email settings
    public string? SmtpServer { get; set; }
    public int? SmtpPort { get; set; }
    public bool SmtpUseSsl { get; set; }
    public string? SmtpUsername { get; set; }
    public string? ImapServer { get; set; }
    public int? ImapPort { get; set; }
    public bool ImapUseSsl { get; set; }
    public string? FromEmail { get; set; }
    public string? FromName { get; set; }
    
    // WhatsApp settings
    public string? WhatsAppBusinessAccountId { get; set; }
    public string? WhatsAppPhoneNumberId { get; set; }
    
    // Social settings
    public string? SocialAccountId { get; set; }
    public string? SocialUsername { get; set; }
    
    // Webhook
    public string? WebhookUrl { get; set; }
    public bool WebhookEnabled { get; set; }
    
    public DateTime? LastConnectedAt { get; set; }
    public string? LastError { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for creating/updating a channel
/// </summary>
public class CommunicationChannelCreateDto
{
    public string ChannelType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public bool IsDefault { get; set; } = false;
    
    // Credentials
    public string? ApiKey { get; set; }
    public string? ApiSecret { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    
    // Email settings
    public string? SmtpServer { get; set; }
    public int? SmtpPort { get; set; }
    public bool SmtpUseSsl { get; set; } = true;
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; }
    public string? ImapServer { get; set; }
    public int? ImapPort { get; set; }
    public bool ImapUseSsl { get; set; } = true;
    public string? FromEmail { get; set; }
    public string? FromName { get; set; }
    
    // WhatsApp settings
    public string? WhatsAppBusinessAccountId { get; set; }
    public string? WhatsAppPhoneNumberId { get; set; }
    public string? WhatsAppVerifyToken { get; set; }
    
    // Social settings
    public string? SocialAccountId { get; set; }
    public string? SocialUsername { get; set; }
    public string? PageAccessToken { get; set; }
}

#endregion

#region Message DTOs

/// <summary>
/// DTO for listing messages
/// </summary>
public class CommunicationMessageListDto
{
    public int Id { get; set; }
    public string ChannelType { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string? BodyPreview { get; set; }
    public string Direction { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? FromAddress { get; set; }
    public string? FromName { get; set; }
    public string? ToAddress { get; set; }
    public string? ToName { get; set; }
    public int AttachmentCount { get; set; }
    public bool IsRead { get; set; }
    public bool IsStarred { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Linked entities
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int? ContactId { get; set; }
    public string? ContactName { get; set; }
}

/// <summary>
/// DTO for message details
/// </summary>
public class CommunicationMessageDto
{
    public int Id { get; set; }
    public int ChannelId { get; set; }
    public string ChannelType { get; set; } = string.Empty;
    public string ChannelName { get; set; } = string.Empty;
    
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public string? HtmlBody { get; set; }
    
    public string Direction { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    
    public string? FromAddress { get; set; }
    public string? FromName { get; set; }
    public string? ToAddress { get; set; }
    public string? ToName { get; set; }
    public List<string>? CcAddresses { get; set; }
    public List<string>? BccAddresses { get; set; }
    
    public int AttachmentCount { get; set; }
    public List<MessageAttachmentDto>? Attachments { get; set; }
    
    public string? ConversationId { get; set; }
    public int? ParentMessageId { get; set; }
    
    // Linked entities
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int? ContactId { get; set; }
    public string? ContactName { get; set; }
    public int? LeadId { get; set; }
    public int? OpportunityId { get; set; }
    
    // Timestamps
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Tracking
    public int OpenCount { get; set; }
    public int ClickCount { get; set; }
    
    // Social media specific
    public bool IsPublicPost { get; set; }
    public int LikeCount { get; set; }
    public int ShareCount { get; set; }
    public int CommentCount { get; set; }
    
    public bool IsRead { get; set; }
    public bool IsStarred { get; set; }
    public bool IsArchived { get; set; }
    
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// DTO for message attachments
/// </summary>
public class MessageAttachmentDto
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public string? Url { get; set; }
}

/// <summary>
/// DTO for sending a message
/// </summary>
public class SendMessageDto
{
    public int ChannelId { get; set; }
    public string? Subject { get; set; }
    public string Body { get; set; } = string.Empty;
    public string? HtmlBody { get; set; }
    public string ToAddress { get; set; } = string.Empty;
    public string? ToName { get; set; }
    public List<string>? CcAddresses { get; set; }
    public List<string>? BccAddresses { get; set; }
    public string Priority { get; set; } = "Normal";
    
    // Scheduling
    public DateTime? ScheduledAt { get; set; }
    
    // Tracking
    public bool TrackOpens { get; set; } = true;
    public bool TrackClicks { get; set; } = true;
    
    // Template
    public int? EmailTemplateId { get; set; }
    public Dictionary<string, string>? MergeFields { get; set; }
    
    // Linked entities
    public int? CustomerId { get; set; }
    public int? ContactId { get; set; }
    public int? LeadId { get; set; }
    public int? OpportunityId { get; set; }
    
    // Threading
    public int? ParentMessageId { get; set; }
    public string? ConversationId { get; set; }
}

/// <summary>
/// DTO for social media post
/// </summary>
public class SocialPostDto
{
    public int ChannelId { get; set; }
    public string Body { get; set; } = string.Empty;
    public List<string>? MediaUrls { get; set; }
    public DateTime? ScheduledAt { get; set; }
    
    // For replies/retweets
    public string? InReplyToId { get; set; }
    public bool IsRetweet { get; set; } = false;
    public bool IsQuoteRetweet { get; set; } = false;
}

/// <summary>
/// DTO for WhatsApp message
/// </summary>
public class WhatsAppMessageDto
{
    public int ChannelId { get; set; }
    public string ToPhoneNumber { get; set; } = string.Empty;
    public string MessageType { get; set; } = "text"; // text, image, document, template
    public string? Body { get; set; }
    public string? MediaUrl { get; set; }
    public string? TemplateName { get; set; }
    public Dictionary<string, string>? TemplateParameters { get; set; }
    
    // Linked entities
    public int? CustomerId { get; set; }
    public int? ContactId { get; set; }
    public int? LeadId { get; set; }
}

#endregion

#region Email Template DTOs

/// <summary>
/// DTO for listing email templates
/// </summary>
public class EmailTemplateListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsSystem { get; set; }
    public int UsageCount { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for email template details
/// </summary>
public class EmailTemplateDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? PlainTextBody { get; set; }
    public string? HtmlBody { get; set; }
    public bool IsActive { get; set; }
    public bool IsSystem { get; set; }
    public List<string>? MergeFields { get; set; }
    public string? FromEmail { get; set; }
    public string? FromName { get; set; }
    public string? ReplyToEmail { get; set; }
    public string? PreviewText { get; set; }
    public int UsageCount { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for creating/updating email template
/// </summary>
public class EmailTemplateCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = "General";
    public string Subject { get; set; } = string.Empty;
    public string? PlainTextBody { get; set; }
    public string? HtmlBody { get; set; }
    public bool IsActive { get; set; } = true;
    public string? FromEmail { get; set; }
    public string? FromName { get; set; }
    public string? ReplyToEmail { get; set; }
    public string? PreviewText { get; set; }
}

#endregion

#region Conversation DTOs

/// <summary>
/// DTO for listing conversations
/// </summary>
public class ConversationListDto
{
    public int Id { get; set; }
    public string ConversationId { get; set; } = string.Empty;
    public string ChannelType { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string? LastMessagePreview { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ParticipantAddress { get; set; }
    public string? ParticipantName { get; set; }
    public int MessageCount { get; set; }
    public int UnreadCount { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public bool IsStarred { get; set; }
    public bool IsPinned { get; set; }
    
    // Linked entities
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int? ContactId { get; set; }
    public string? ContactName { get; set; }
}

/// <summary>
/// DTO for conversation details with messages
/// </summary>
public class ConversationDto
{
    public int Id { get; set; }
    public string ConversationId { get; set; } = string.Empty;
    public string ChannelType { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    
    public string? ParticipantAddress { get; set; }
    public string? ParticipantName { get; set; }
    
    public int? CustomerId { get; set; }
    public int? ContactId { get; set; }
    public int? LeadId { get; set; }
    public int? AssignedToUserId { get; set; }
    
    public int MessageCount { get; set; }
    public int UnreadCount { get; set; }
    public DateTime? FirstMessageAt { get; set; }
    public DateTime? LastMessageAt { get; set; }
    
    public List<string>? Tags { get; set; }
    public bool IsStarred { get; set; }
    public bool IsMuted { get; set; }
    public bool IsPinned { get; set; }
    
    public List<CommunicationMessageListDto>? Messages { get; set; }
}

#endregion

#region Dashboard/Stats DTOs

/// <summary>
/// DTO for communication dashboard statistics
/// </summary>
public class CommunicationStatsDto
{
    public int TotalMessages { get; set; }
    public int TotalInbound { get; set; }
    public int TotalOutbound { get; set; }
    public int UnreadCount { get; set; }
    public int OpenConversations { get; set; }
    public int PendingMessages { get; set; }
    
    public ChannelStatsDto? EmailStats { get; set; }
    public ChannelStatsDto? WhatsAppStats { get; set; }
    public ChannelStatsDto? TwitterStats { get; set; }
    public ChannelStatsDto? FacebookStats { get; set; }
}

/// <summary>
/// DTO for per-channel statistics
/// </summary>
public class ChannelStatsDto
{
    public string ChannelType { get; set; } = string.Empty;
    public int TotalMessages { get; set; }
    public int Inbound { get; set; }
    public int Outbound { get; set; }
    public int Unread { get; set; }
    public int Pending { get; set; }
    public int Failed { get; set; }
    public int OpenRate { get; set; } // For email
    public int ClickRate { get; set; } // For email
}

#endregion
