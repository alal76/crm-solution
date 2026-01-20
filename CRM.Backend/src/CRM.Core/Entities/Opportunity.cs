namespace CRM.Core.Entities;

/// <summary>
/// Opportunity stage enumeration
/// </summary>
public enum OpportunityStage
{
    Prospecting = 0,
    Qualification = 1,
    NeedsAnalysis = 2,
    ValueProposition = 3,
    IdentifyDecisionMakers = 4,
    PerceptionAnalysis = 5,
    ProposalQuote = 6,
    NegotiationReview = 7,
    ClosedWon = 8,
    ClosedLost = 9
}

/// <summary>
/// Opportunity type enumeration
/// </summary>
public enum OpportunityType
{
    NewBusiness = 0,
    ExistingBusiness = 1,
    Renewal = 2,
    Upsell = 3,
    CrossSell = 4,
    Referral = 5
}

/// <summary>
/// Opportunity priority level
/// </summary>
public enum OpportunityPriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

/// <summary>
/// Forecast category
/// </summary>
public enum ForecastCategory
{
    Pipeline = 0,
    BestCase = 1,
    Commit = 2,
    Closed = 3,
    Omitted = 4
}

/// <summary>
/// Opportunity entity for managing sales opportunities
/// </summary>
public class Opportunity : BaseEntity
{
    // Basic Information
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public OpportunityType OpportunityType { get; set; } = OpportunityType.NewBusiness;
    public OpportunityPriority Priority { get; set; } = OpportunityPriority.Medium;
    
    // Financial
    public decimal Amount { get; set; } = 0;
    public decimal ExpectedRevenue { get; set; } = 0; // Amount * Probability
    public decimal RecurringRevenue { get; set; } = 0; // Monthly recurring
    public decimal OneTimeRevenue { get; set; } = 0;
    public decimal Discount { get; set; } = 0;
    public decimal DiscountPercent { get; set; } = 0;
    public string? CurrencyCode { get; set; } = "USD";
    
    // Stage & Probability
    public OpportunityStage Stage { get; set; } = OpportunityStage.Prospecting;
    public double Probability { get; set; } = 0; // 0-100
    public ForecastCategory ForecastCategory { get; set; } = ForecastCategory.Pipeline;
    
    // Dates
    public DateTime? CloseDate { get; set; }
    public DateTime? ExpectedCloseDate { get; set; }
    public DateTime? ActualCloseDate { get; set; }
    public DateTime? NextStepDate { get; set; }
    public DateTime? LastActivityDate { get; set; }
    public int? DaysInCurrentStage { get; set; }
    public int? TotalDaysOpen { get; set; }
    
    // Sales Process
    public string? NextStep { get; set; }
    public string? LossReason { get; set; } // If closed lost
    public string? WinReason { get; set; } // If closed won
    public string? CompetitorName { get; set; }
    public string? CompetitorStrengths { get; set; }
    public string? CompetitorWeaknesses { get; set; }
    
    // Decision Making
    public string? DecisionMakers { get; set; } // JSON array of decision makers
    public string? BudgetStatus { get; set; } // Approved, Pending, Not Allocated
    public string? DecisionProcess { get; set; }
    public string? PainPoints { get; set; }
    public string? ProposedSolution { get; set; }
    
    // Source & Campaign
    public string? LeadSource { get; set; }
    public int? CampaignId { get; set; }
    public int? OriginalLeadId { get; set; } // If converted from lead
    
    // Relationships
    public int CustomerId { get; set; }
    public int? PrimaryContactId { get; set; }
    public int? AssignedToUserId { get; set; }
    public int? ProductId { get; set; }
    public int? QuoteId { get; set; }
    
    // Classification
    public string? Tags { get; set; } // Comma-separated tags
    public string? Territory { get; set; }
    public string? Region { get; set; }
    
    // Documentation
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }
    
    // Custom Fields
    public string? CustomFields { get; set; } // JSON for custom data

    // Navigation properties
    public Customer? Customer { get; set; }
    public Product? Product { get; set; }
    public User? AssignedToUser { get; set; }
    public MarketingCampaign? Campaign { get; set; }
}
