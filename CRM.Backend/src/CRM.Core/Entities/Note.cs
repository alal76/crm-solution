namespace CRM.Core.Entities;

/// <summary>
/// Note visibility enumeration
/// </summary>
public enum NoteVisibility
{
    Private = 0,
    Team = 1,
    Public = 2
}

/// <summary>
/// Note type enumeration
/// </summary>
public enum NoteType
{
    General = 0,
    CallNotes = 1,
    MeetingNotes = 2,
    Feedback = 3,
    Requirement = 4,
    Issue = 5,
    Idea = 6,
    Warning = 7,
    Other = 8
}

/// <summary>
/// Note entity for storing notes and comments
/// </summary>
public class Note : BaseEntity
{
    // Content
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Summary { get; set; }
    
    // Classification
    public NoteType NoteType { get; set; } = NoteType.General;
    public NoteVisibility Visibility { get; set; } = NoteVisibility.Team;
    public bool IsPinned { get; set; } = false;
    public bool IsImportant { get; set; } = false;
    
    // Relationships - polymorphic
    public int? CustomerId { get; set; }
    public int? ContactId { get; set; }
    public int? OpportunityId { get; set; }
    public int? CampaignId { get; set; }
    public int? ProductId { get; set; }
    public int? TaskId { get; set; }
    public int? InteractionId { get; set; }
    
    // Authorship
    public int? CreatedByUserId { get; set; }
    public int? LastModifiedByUserId { get; set; }
    
    // Classification
    public string? Tags { get; set; }
    public string? Category { get; set; }
    
    // Attachments
    public string? Attachments { get; set; } // JSON array of file URLs
    
    // Mentions & Links
    public string? MentionedUsers { get; set; } // JSON array of user IDs
    public string? RelatedNotes { get; set; } // JSON array of note IDs
    
    // Custom Fields
    public string? CustomFields { get; set; }

    // Navigation properties
    public Customer? Customer { get; set; }
    public Opportunity? Opportunity { get; set; }
    public MarketingCampaign? Campaign { get; set; }
    public Product? Product { get; set; }
    public User? CreatedByUser { get; set; }
    public User? LastModifiedByUser { get; set; }
}
