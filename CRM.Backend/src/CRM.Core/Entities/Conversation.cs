namespace CRM.Core.Entities;

/// <summary>
/// Status of a communication conversation
/// </summary>
public enum ConversationStatus
{
    Open = 0,
    Pending = 1,
    Resolved = 2,
    Closed = 3,
    Spam = 4,
    Archived = 5
}

/// <summary>
/// Conversation entity for grouping related messages into threads
/// Provides a unified view of all communications with a contact/customer
/// </summary>
public class Conversation : BaseEntity
{
    /// <summary>
    /// Primary channel type for this conversation
    /// </summary>
    public ChannelType PrimaryChannelType { get; set; }
    
    /// <summary>
    /// Unique conversation identifier
    /// </summary>
    public string ConversationId { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Subject or title of the conversation
    /// </summary>
    public string? Subject { get; set; }
    
    /// <summary>
    /// Preview of the last message
    /// </summary>
    public string? LastMessagePreview { get; set; }
    
    /// <summary>
    /// Current status of the conversation
    /// </summary>
    public ConversationStatus Status { get; set; } = ConversationStatus.Open;
    
    /// <summary>
    /// Priority level
    /// </summary>
    public MessagePriority Priority { get; set; } = MessagePriority.Normal;
    
    #region Participant Information
    
    /// <summary>
    /// External participant identifier (email, phone, handle)
    /// </summary>
    public string? ParticipantAddress { get; set; }
    
    /// <summary>
    /// External participant name
    /// </summary>
    public string? ParticipantName { get; set; }
    
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
    
    #endregion
    
    #region Assignment
    
    /// <summary>
    /// User assigned to this conversation
    /// </summary>
    public int? AssignedToUserId { get; set; }
    
    /// <summary>
    /// User who last responded
    /// </summary>
    public int? LastRespondedByUserId { get; set; }
    
    #endregion
    
    #region Statistics
    
    /// <summary>
    /// Total number of messages in conversation
    /// </summary>
    public int MessageCount { get; set; } = 0;
    
    /// <summary>
    /// Number of unread messages
    /// </summary>
    public int UnreadCount { get; set; } = 0;
    
    /// <summary>
    /// Number of inbound messages
    /// </summary>
    public int InboundCount { get; set; } = 0;
    
    /// <summary>
    /// Number of outbound messages
    /// </summary>
    public int OutboundCount { get; set; } = 0;
    
    #endregion
    
    #region Timestamps
    
    /// <summary>
    /// When the first message was sent/received
    /// </summary>
    public DateTime? FirstMessageAt { get; set; }
    
    /// <summary>
    /// When the last message was sent/received
    /// </summary>
    public DateTime? LastMessageAt { get; set; }
    
    /// <summary>
    /// When the last inbound message was received
    /// </summary>
    public DateTime? LastInboundAt { get; set; }
    
    /// <summary>
    /// When the last outbound message was sent
    /// </summary>
    public DateTime? LastOutboundAt { get; set; }
    
    /// <summary>
    /// When the conversation was resolved/closed
    /// </summary>
    public DateTime? ResolvedAt { get; set; }
    
    #endregion
    
    #region Metadata
    
    /// <summary>
    /// Tags for categorization (JSON array)
    /// </summary>
    public string? TagsJson { get; set; }
    
    /// <summary>
    /// Additional metadata as JSON
    /// </summary>
    public string? MetadataJson { get; set; }
    
    /// <summary>
    /// Whether this conversation is starred
    /// </summary>
    public bool IsStarred { get; set; } = false;
    
    /// <summary>
    /// Whether this conversation is muted
    /// </summary>
    public bool IsMuted { get; set; } = false;
    
    /// <summary>
    /// Whether this conversation is pinned
    /// </summary>
    public bool IsPinned { get; set; } = false;
    
    #endregion
    
    // Navigation
    public virtual ICollection<CommunicationMessage> Messages { get; set; } = new List<CommunicationMessage>();
}
