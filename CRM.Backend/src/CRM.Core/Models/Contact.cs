namespace CRM.Core.Models;

/// <summary>
/// Contact type enumeration
/// </summary>
public enum ContactType
{
    Employee = 0,
    Partner = 1,
    Lead = 2,
    Customer = 3,
    Vendor = 4,
    Influencer = 5,
    Investor = 6,
    Media = 7,
    Other = 8
}

/// <summary>
/// Contact status
/// </summary>
public enum ContactStatus
{
    Active = 0,
    Inactive = 1,
    Pending = 2,
    Blocked = 3,
    Archived = 4
}

/// <summary>
/// Lead status for contacts of type Lead
/// </summary>
public enum LeadStatus
{
    New = 0,
    Contacted = 1,
    Qualified = 2,
    Unqualified = 3,
    Converted = 4,
    Lost = 5
}

/// <summary>
/// Preferred contact method
/// </summary>
public enum PreferredContactMethod
{
    Email = 0,
    Phone = 1,
    SMS = 2,
    Mail = 3,
    Social = 4,
    Any = 5
}

/// <summary>
/// Represents a contact entity (employee, partner, lead, or other)
/// </summary>
public class Contact
{
    public int Id { get; set; }
    
    // Type & Status
    public ContactType ContactType { get; set; }
    public ContactStatus Status { get; set; } = ContactStatus.Active;
    public LeadStatus? LeadStatus { get; set; } // Only for Lead type
    
    // Personal Information
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string? Salutation { get; set; } // Mr., Mrs., Dr., etc.
    public string? Suffix { get; set; } // Jr., Sr., III, etc.
    public string? Nickname { get; set; }
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    
    // Contact Information
    public string? EmailPrimary { get; set; }
    public string? EmailSecondary { get; set; }
    public string? EmailWork { get; set; }
    public string? PhonePrimary { get; set; }
    public string? PhoneSecondary { get; set; }
    public string? PhoneMobile { get; set; }
    public string? PhoneWork { get; set; }
    public string? PhoneFax { get; set; }
    
    // Address - Primary
    public string? Address { get; set; }
    public string? Address2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? ZipCode { get; set; }
    
    // Address - Mailing (if different)
    public string? MailingAddress { get; set; }
    public string? MailingCity { get; set; }
    public string? MailingState { get; set; }
    public string? MailingCountry { get; set; }
    public string? MailingZipCode { get; set; }
    
    // Professional Information
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public string? Company { get; set; }
    public string? Industry { get; set; }
    public string? ReportsTo { get; set; }
    public int? ReportsToContactId { get; set; }
    public int? AssistantContactId { get; set; }
    public string? AssistantName { get; set; }
    public string? AssistantPhone { get; set; }
    
    // Lead Information (for Lead type)
    public string? LeadSource { get; set; }
    public int? LeadScore { get; set; } // 0-100
    public bool? IsQualified { get; set; }
    public DateTime? QualifiedDate { get; set; }
    public DateTime? ConvertedDate { get; set; }
    public int? ConvertedToCustomerId { get; set; }
    public string? LeadRating { get; set; } // Hot, Warm, Cold
    
    // Communication Preferences
    public PreferredContactMethod PreferredContactMethod { get; set; } = PreferredContactMethod.Email;
    public string? PreferredContactTime { get; set; }
    public string? Timezone { get; set; }
    public string? PreferredLanguage { get; set; }
    public bool OptInEmail { get; set; } = true;
    public bool OptInSms { get; set; } = false;
    public bool OptInPhone { get; set; } = true;
    public bool OptInMail { get; set; } = true;
    public bool DoNotContact { get; set; } = false;
    public DateTime? LastOptInDate { get; set; }
    public DateTime? LastOptOutDate { get; set; }
    
    // Social Media
    public string? LinkedInUrl { get; set; }
    public string? TwitterHandle { get; set; }
    public string? FacebookUrl { get; set; }
    public string? InstagramHandle { get; set; }
    public string? Website { get; set; }
    public string? BlogUrl { get; set; }
    
    // Relationship Information
    public string? Notes { get; set; }
    public string? Description { get; set; }
    public string? Interests { get; set; } // Comma-separated
    public string? Tags { get; set; } // Comma-separated
    
    // Assignment
    public int? OwnerId { get; set; }
    public int? AssignedToUserId { get; set; }
    public string? Territory { get; set; }
    
    // Relationships
    public int? AccountId { get; set; } // Related Customer/Account
    public int? CampaignId { get; set; } // Source campaign
    
    // Engagement Tracking
    public DateTime? LastActivityDate { get; set; }
    public DateTime? LastContactedDate { get; set; }
    public DateTime? NextFollowUpDate { get; set; }
    public int? TotalInteractions { get; set; }
    public int? EmailsReceived { get; set; }
    public int? EmailsOpened { get; set; }
    public int? LinksClicked { get; set; }
    
    // System Fields
    public DateTime DateAdded { get; set; } = DateTime.UtcNow;
    public DateTime? LastModified { get; set; }
    public string? ModifiedBy { get; set; }
    public int? CreatedByUserId { get; set; }
    
    // Custom Fields
    public string? CustomFields { get; set; } // JSON for custom data
    
    // Photo
    public string? PhotoUrl { get; set; }
    
    // Navigation Properties
    public ICollection<SocialMediaLink> SocialMediaLinks { get; set; } = new List<SocialMediaLink>();
}
