namespace CRM.Core.Entities;

/// <summary>
/// Interaction type enumeration
/// </summary>
public enum InteractionType
{
    Email = 0,
    Phone = 1,
    Meeting = 2,
    VideoCall = 3,
    Chat = 4,
    SMS = 5,
    SocialMedia = 6,
    InPerson = 7,
    WebForm = 8,
    Note = 9,
    Task = 10,
    Demo = 11,
    Presentation = 12,
    Contract = 13,
    Support = 14,
    Other = 15
}

/// <summary>
/// Interaction direction
/// </summary>
public enum InteractionDirection
{
    Inbound = 0,
    Outbound = 1,
    Internal = 2
}

/// <summary>
/// Interaction outcome
/// </summary>
public enum InteractionOutcome
{
    None = 0,
    Successful = 1,
    Unsuccessful = 2,
    FollowUpRequired = 3,
    NoResponse = 4,
    Voicemail = 5,
    Rescheduled = 6,
    Cancelled = 7
}

/// <summary>
/// Interaction sentiment
/// </summary>
public enum InteractionSentiment
{
    VeryNegative = 0,
    Negative = 1,
    Neutral = 2,
    Positive = 3,
    VeryPositive = 4
}

/// <summary>
/// Interaction entity for tracking customer communications
/// </summary>
public class Interaction : BaseEntity
{
    // Basic Information
    public InteractionType InteractionType { get; set; } = InteractionType.Note;
    public string Type { get; set; } = string.Empty; // Legacy field
    public InteractionDirection Direction { get; set; } = InteractionDirection.Outbound;
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    // Timing
    public DateTime InteractionDate { get; set; }
    public DateTime? EndTime { get; set; }
    public int? DurationMinutes { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    
    // Status & Outcome
    public InteractionOutcome Outcome { get; set; } = InteractionOutcome.None;
    public InteractionSentiment Sentiment { get; set; } = InteractionSentiment.Neutral;
    public bool IsCompleted { get; set; } = false;
    public bool IsPrivate { get; set; } = false;
    public int? Priority { get; set; } = 1;
    
    // Communication Details
    public string? PhoneNumber { get; set; }
    public string? EmailAddress { get; set; }
    public string? Location { get; set; }
    public string? MeetingLink { get; set; }
    public string? RecordingUrl { get; set; }
    
    // Email Specific
    public string? EmailCc { get; set; }
    public string? EmailBcc { get; set; }
    public bool? EmailOpened { get; set; }
    public DateTime? EmailOpenedDate { get; set; }
    public bool? EmailClicked { get; set; }
    public DateTime? EmailClickedDate { get; set; }
    public bool? EmailBounced { get; set; }
    
    // Meeting Specific
    public string? Attendees { get; set; } // JSON array
    public string? MeetingNotes { get; set; }
    public string? MeetingAgenda { get; set; }
    public string? ActionItems { get; set; } // JSON array
    
    // Call Specific
    public string? CallRecordingUrl { get; set; }
    public string? CallTranscript { get; set; }
    public string? CallDisposition { get; set; }
    
    // Follow-up
    public DateTime? FollowUpDate { get; set; }
    public string? FollowUpNotes { get; set; }
    public int? FollowUpInteractionId { get; set; }
    
    // Relationships
    public int? CustomerId { get; set; }
    public int? ContactId { get; set; }
    public int? OpportunityId { get; set; }
    public int? CampaignId { get; set; }
    public int? AssignedToUserId { get; set; }
    public int? CreatedByUserId { get; set; }
    
    // Classification
    public string? Tags { get; set; }
    public string? Category { get; set; }
    
    // Attachments
    public string? Attachments { get; set; } // JSON array of file URLs
    
    // Custom Fields
    public string? CustomFields { get; set; } // JSON for custom data

    // Navigation properties
    public Customer? Customer { get; set; }
    public Opportunity? Opportunity { get; set; }
    public MarketingCampaign? Campaign { get; set; }
    public User? AssignedToUser { get; set; }
    public User? CreatedByUser { get; set; }
    public Interaction? FollowUpInteraction { get; set; }
}
