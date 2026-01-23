namespace CRM.Core.Entities;

/// <summary>
/// Direction of the communication message
/// </summary>
public enum MessageDirection
{
    Outbound = 0,   // Sent by CRM user
    Inbound = 1     // Received from customer/prospect
}

/// <summary>
/// Status of the message
/// </summary>
public enum MessageStatus
{
    Draft = 0,
    Queued = 1,
    Sending = 2,
    Sent = 3,
    Delivered = 4,
    Read = 5,
    Failed = 6,
    Bounced = 7,
    Replied = 8,
    Deleted = 9
}

/// <summary>
/// Priority level for messages
/// </summary>
public enum MessagePriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Urgent = 3
}

/// <summary>
/// Communication message entity - stores all messages across all channels
/// Unified inbox/outbox for Email, WhatsApp, X, Facebook, etc.
/// </summary>
public class CommunicationMessage : BaseEntity
{
    #region Channel Information
    
    /// <summary>
    /// The channel through which this message was sent/received
    /// </summary>
    public int ChannelId { get; set; }
    
    /// <summary>
    /// Type of channel (denormalized for quick filtering)
    /// </summary>
    public ChannelType ChannelType { get; set; }
    
    #endregion
    
    #region Message Content
    
    /// <summary>
    /// Subject line (primarily for emails)
    /// </summary>
    public string? Subject { get; set; }
    
    /// <summary>
    /// Plain text body of the message
    /// </summary>
    public string? Body { get; set; }
    
    /// <summary>
    /// HTML body of the message (for emails)
    /// </summary>
    public string? HtmlBody { get; set; }
    
    /// <summary>
    /// JSON array of attachment metadata
    /// </summary>
    public string? AttachmentsJson { get; set; }
    
    /// <summary>
    /// Number of attachments
    /// </summary>
    public int AttachmentCount { get; set; } = 0;
    
    #endregion
    
    #region Direction and Status
    
    /// <summary>
    /// Whether message is inbound or outbound
    /// </summary>
    public MessageDirection Direction { get; set; }
    
    /// <summary>
    /// Current status of the message
    /// </summary>
    public MessageStatus Status { get; set; } = MessageStatus.Draft;
    
    /// <summary>
    /// Priority level
    /// </summary>
    public MessagePriority Priority { get; set; } = MessagePriority.Normal;
    
    #endregion
    
    #region Sender/Recipient Information
    
    /// <summary>
    /// From email/phone/handle
    /// </summary>
    public string? FromAddress { get; set; }
    
    /// <summary>
    /// From name
    /// </summary>
    public string? FromName { get; set; }
    
    /// <summary>
    /// To email/phone/handle (primary recipient)
    /// </summary>
    public string? ToAddress { get; set; }
    
    /// <summary>
    /// To name
    /// </summary>
    public string? ToName { get; set; }
    
    /// <summary>
    /// CC recipients (JSON array for email)
    /// </summary>
    public string? CcAddresses { get; set; }
    
    /// <summary>
    /// BCC recipients (JSON array for email)
    /// </summary>
    public string? BccAddresses { get; set; }
    
    /// <summary>
    /// Reply-To address
    /// </summary>
    public string? ReplyToAddress { get; set; }
    
    #endregion
    
    #region CRM Entity Linking
    
    /// <summary>
    /// Linked Customer ID
    /// </summary>
    public int? CustomerId { get; set; }
    
    /// <summary>
    /// Linked Contact ID
    /// </summary>
    public int? ContactId { get; set; }
    
    /// <summary>
    /// Linked Lead ID
    /// </summary>
    public int? LeadId { get; set; }
    
    /// <summary>
    /// Linked Opportunity ID
    /// </summary>
    public int? OpportunityId { get; set; }
    
    /// <summary>
    /// Generic entity type for polymorphic linking
    /// </summary>
    public string? LinkedEntityType { get; set; }
    
    /// <summary>
    /// Generic entity ID for polymorphic linking
    /// </summary>
    public int? LinkedEntityId { get; set; }
    
    #endregion
    
    #region Conversation Threading
    
    /// <summary>
    /// Conversation/Thread ID for grouping related messages
    /// </summary>
    public string? ConversationId { get; set; }
    
    /// <summary>
    /// Parent message ID for threading
    /// </summary>
    public int? ParentMessageId { get; set; }
    
    /// <summary>
    /// Message ID from external system (email Message-ID, WhatsApp message ID, etc.)
    /// </summary>
    public string? ExternalMessageId { get; set; }
    
    /// <summary>
    /// External ID of the message this is replying to
    /// </summary>
    public string? InReplyToExternalId { get; set; }
    
    #endregion
    
    #region Timestamps
    
    /// <summary>
    /// When the message was sent
    /// </summary>
    public DateTime? SentAt { get; set; }
    
    /// <summary>
    /// When the message was delivered
    /// </summary>
    public DateTime? DeliveredAt { get; set; }
    
    /// <summary>
    /// When the message was read
    /// </summary>
    public DateTime? ReadAt { get; set; }
    
    /// <summary>
    /// When the message was received (for inbound)
    /// </summary>
    public DateTime? ReceivedAt { get; set; }
    
    /// <summary>
    /// Scheduled send time
    /// </summary>
    public DateTime? ScheduledAt { get; set; }
    
    #endregion
    
    #region User Assignment
    
    /// <summary>
    /// User who sent/created the message
    /// </summary>
    public int? SentByUserId { get; set; }
    
    /// <summary>
    /// User assigned to handle this message
    /// </summary>
    public int? AssignedToUserId { get; set; }
    
    #endregion
    
    #region Email-Specific Fields
    
    /// <summary>
    /// Email template ID if created from template
    /// </summary>
    public int? EmailTemplateId { get; set; }
    
    /// <summary>
    /// Whether tracking pixel was included
    /// </summary>
    public bool TrackOpens { get; set; } = false;
    
    /// <summary>
    /// Whether link tracking was enabled
    /// </summary>
    public bool TrackClicks { get; set; } = false;
    
    /// <summary>
    /// Number of times email was opened
    /// </summary>
    public int OpenCount { get; set; } = 0;
    
    /// <summary>
    /// Number of link clicks
    /// </summary>
    public int ClickCount { get; set; } = 0;
    
    #endregion
    
    #region Social Media Specific Fields
    
    /// <summary>
    /// Social media post/tweet ID
    /// </summary>
    public string? SocialPostId { get; set; }
    
    /// <summary>
    /// Whether this is a public post or direct message
    /// </summary>
    public bool IsPublicPost { get; set; } = false;
    
    /// <summary>
    /// Number of likes/reactions
    /// </summary>
    public int LikeCount { get; set; } = 0;
    
    /// <summary>
    /// Number of shares/retweets
    /// </summary>
    public int ShareCount { get; set; } = 0;
    
    /// <summary>
    /// Number of comments/replies
    /// </summary>
    public int CommentCount { get; set; } = 0;
    
    #endregion
    
    #region WhatsApp Specific Fields
    
    /// <summary>
    /// WhatsApp message type (text, image, document, template, etc.)
    /// </summary>
    public string? WhatsAppMessageType { get; set; }
    
    /// <summary>
    /// WhatsApp template name if using template
    /// </summary>
    public string? WhatsAppTemplateName { get; set; }
    
    #endregion
    
    #region Error Handling
    
    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Error code from external service
    /// </summary>
    public string? ErrorCode { get; set; }
    
    /// <summary>
    /// Number of retry attempts
    /// </summary>
    public int RetryCount { get; set; } = 0;
    
    #endregion
    
    #region Metadata
    
    /// <summary>
    /// Additional metadata as JSON
    /// </summary>
    public string? MetadataJson { get; set; }
    
    /// <summary>
    /// Tags for categorization (JSON array)
    /// </summary>
    public string? TagsJson { get; set; }
    
    /// <summary>
    /// Whether this message has been archived
    /// </summary>
    public bool IsArchived { get; set; } = false;
    
    /// <summary>
    /// Whether this message is starred/important
    /// </summary>
    public bool IsStarred { get; set; } = false;
    
    /// <summary>
    /// Whether message has been read by CRM user
    /// </summary>
    public bool IsRead { get; set; } = false;
    
    #endregion
    
    // Navigation Properties
    public virtual CommunicationChannel? Channel { get; set; }
    public virtual CommunicationMessage? ParentMessage { get; set; }
    public virtual ICollection<CommunicationMessage> Replies { get; set; } = new List<CommunicationMessage>();
    public virtual EmailTemplate? EmailTemplate { get; set; }
}
