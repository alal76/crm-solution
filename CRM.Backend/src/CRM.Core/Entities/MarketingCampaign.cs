namespace CRM.Core.Entities;

/// <summary>
/// Campaign status enumeration
/// </summary>
public enum CampaignStatus
{
    Draft = 0,
    Scheduled = 1,
    Active = 2,
    Paused = 3,
    Completed = 4,
    Cancelled = 5,
    Archived = 6
}

/// <summary>
/// Campaign type enumeration
/// </summary>
public enum CampaignType
{
    Email = 0,
    SocialMedia = 1,
    PaidSearch = 2,
    DisplayAds = 3,
    ContentMarketing = 4,
    SEO = 5,
    Event = 6,
    Webinar = 7,
    DirectMail = 8,
    Telemarketing = 9,
    Referral = 10,
    Affiliate = 11,
    Influencer = 12,
    PR = 13,
    TradeShow = 14,
    Other = 15
}

/// <summary>
/// Campaign priority level
/// </summary>
public enum CampaignPriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

/// <summary>
/// Marketing Campaign entity for managing marketing campaigns
/// </summary>
public class MarketingCampaign : BaseEntity
{
    // Basic Information
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Objective { get; set; }
    public CampaignType CampaignType { get; set; } = CampaignType.Email;
    public CampaignStatus Status { get; set; } = CampaignStatus.Draft;
    public CampaignPriority Priority { get; set; } = CampaignPriority.Medium;
    
    // Legacy Type field (kept for backward compatibility)
    public string Type { get; set; } = string.Empty;
    
    // Dates
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    
    // Budget & Costs
    public decimal Budget { get; set; } = 0;
    public decimal ActualCost { get; set; } = 0;
    public decimal? ExpectedRevenue { get; set; }
    public decimal? ActualRevenue { get; set; }
    public decimal? CostPerLead { get; set; }
    public decimal? CostPerAcquisition { get; set; }
    public string? CurrencyCode { get; set; } = "USD";
    
    // Target Audience
    public int TargetAudience { get; set; } = 0;
    public string? TargetAudienceDescription { get; set; }
    public string? TargetDemographics { get; set; } // JSON
    public string? TargetGeography { get; set; }
    public string? TargetIndustries { get; set; }
    public string? TargetSegments { get; set; } // Customer segments
    public string? ExclusionCriteria { get; set; }
    
    // Performance Metrics
    public double ConversionRate { get; set; } = 0;
    public int Impressions { get; set; } = 0;
    public int Clicks { get; set; } = 0;
    public double ClickThroughRate { get; set; } = 0;
    public int LeadsGenerated { get; set; } = 0;
    public int OpportunitiesCreated { get; set; } = 0;
    public int CustomersAcquired { get; set; } = 0;
    public double ROI { get; set; } = 0;
    public int EmailsSent { get; set; } = 0;
    public int EmailsOpened { get; set; } = 0;
    public double OpenRate { get; set; } = 0;
    public int Bounces { get; set; } = 0;
    public int Unsubscribes { get; set; } = 0;
    
    // Social Media Metrics
    public int SocialReach { get; set; } = 0;
    public int SocialEngagement { get; set; } = 0;
    public int SocialShares { get; set; } = 0;
    public int SocialComments { get; set; } = 0;
    public int SocialLikes { get; set; } = 0;
    public int NewFollowers { get; set; } = 0;
    
    // Content
    public string? MessageSubject { get; set; }
    public string? MessageBody { get; set; }
    public string? CallToAction { get; set; }
    public string? LandingPageUrl { get; set; }
    public string? TrackingUrl { get; set; }
    public string? UtmSource { get; set; }
    public string? UtmMedium { get; set; }
    public string? UtmCampaign { get; set; }
    public string? UtmContent { get; set; }
    
    // A/B Testing
    public bool IsABTest { get; set; } = false;
    public string? ABTestVariants { get; set; } // JSON array of variants
    public string? WinningVariant { get; set; }
    
    // Channels & Platforms
    public string? Channels { get; set; } // JSON array: email, facebook, google, etc.
    public string? Platforms { get; set; } // Specific platforms used
    
    // Assignment
    public int? OwnerId { get; set; }
    public int? AssignedToUserId { get; set; }
    public string? TeamMembers { get; set; } // JSON array of user IDs
    
    // Related
    public int? ParentCampaignId { get; set; }
    public string? RelatedCampaigns { get; set; } // JSON array of related campaign IDs
    
    // Classification
    public string? Tags { get; set; }
    public string? Category { get; set; }
    
    // Documentation
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }
    public string? Attachments { get; set; } // JSON array of file URLs
    
    // Custom Fields
    public string? CustomFields { get; set; } // JSON for custom data

    // Navigation properties
    public ICollection<Product>? Products { get; set; }
    public ICollection<CampaignMetric>? Metrics { get; set; }
    public ICollection<Opportunity>? Opportunities { get; set; }
    public User? Owner { get; set; }
    public User? AssignedToUser { get; set; }
    public MarketingCampaign? ParentCampaign { get; set; }
    public ICollection<MarketingCampaign>? ChildCampaigns { get; set; }
}
