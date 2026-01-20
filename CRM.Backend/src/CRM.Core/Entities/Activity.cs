namespace CRM.Core.Entities;

/// <summary>
/// Activity type enumeration - used for timeline/activity feed
/// </summary>
public enum ActivityType
{
    // Communication
    EmailSent = 0,
    EmailReceived = 1,
    CallMade = 2,
    CallReceived = 3,
    MeetingScheduled = 4,
    MeetingCompleted = 5,
    ChatMessage = 6,
    SMSSent = 7,
    
    // CRM Actions
    CustomerCreated = 10,
    CustomerUpdated = 11,
    OpportunityCreated = 12,
    OpportunityUpdated = 13,
    OpportunityWon = 14,
    OpportunityLost = 15,
    OpportunityStageChanged = 16,
    
    // Documents
    QuoteCreated = 20,
    QuoteSent = 21,
    QuoteAccepted = 22,
    QuoteRejected = 23,
    
    // Tasks
    TaskCreated = 30,
    TaskCompleted = 31,
    TaskOverdue = 32,
    
    // Notes
    NoteAdded = 40,
    NoteUpdated = 41,
    
    // Campaigns
    CampaignLaunched = 50,
    CampaignCompleted = 51,
    LeadCaptured = 52,
    
    // System
    OwnerChanged = 60,
    TagsChanged = 61,
    StatusChanged = 62,
    FileUploaded = 63,
    FileDeleted = 64,
    
    // Custom
    Other = 99
}

/// <summary>
/// Activity entity for tracking all actions and creating activity timeline
/// </summary>
public class Activity : BaseEntity
{
    // Activity Information
    public ActivityType ActivityType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Details { get; set; } // JSON for additional details
    
    // Timing
    public DateTime ActivityDate { get; set; } = DateTime.UtcNow;
    public int? DurationMinutes { get; set; }
    
    // Actor
    public int? UserId { get; set; }
    public string? UserName { get; set; } // Denormalized for display
    public string? UserEmail { get; set; }
    
    // Related Entity (polymorphic)
    public string? EntityType { get; set; } // Customer, Opportunity, Contact, etc.
    public int? EntityId { get; set; }
    public string? EntityName { get; set; } // Denormalized for display
    
    // Secondary Related Entity
    public string? SecondaryEntityType { get; set; }
    public int? SecondaryEntityId { get; set; }
    public string? SecondaryEntityName { get; set; }
    
    // Specific Relationships (for querying)
    public int? CustomerId { get; set; }
    public int? ContactId { get; set; }
    public int? OpportunityId { get; set; }
    public int? CampaignId { get; set; }
    public int? ProductId { get; set; }
    public int? TaskId { get; set; }
    public int? QuoteId { get; set; }
    public int? InteractionId { get; set; }
    public int? NoteId { get; set; }
    
    // Changes (for update activities)
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? FieldsChanged { get; set; } // JSON array of changed field names
    
    // Visibility
    public bool IsSystem { get; set; } = false; // System-generated vs user action
    public bool IsPrivate { get; set; } = false;
    public bool IsImportant { get; set; } = false;
    
    // Classification
    public string? Tags { get; set; }
    public string? Category { get; set; }
    
    // Metadata
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Source { get; set; } // API, Web, Mobile, Import, etc.
    
    // Custom Fields
    public string? CustomFields { get; set; }

    // Navigation properties
    public User? User { get; set; }
    public Customer? Customer { get; set; }
    public Opportunity? Opportunity { get; set; }
    public MarketingCampaign? Campaign { get; set; }
    public Product? Product { get; set; }
}
