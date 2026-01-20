namespace CRM.Core.Entities;

/// <summary>
/// Customer lifecycle stage enumeration
/// </summary>
public enum CustomerLifecycleStage
{
    Lead = 0,
    Prospect = 1,
    Opportunity = 2,
    Customer = 3,
    Churned = 4,
    Reactivated = 5
}

/// <summary>
/// Customer type enumeration
/// </summary>
public enum CustomerType
{
    Individual = 0,
    SmallBusiness = 1,
    MidMarket = 2,
    Enterprise = 3,
    Government = 4,
    NonProfit = 5
}

/// <summary>
/// Customer priority level
/// </summary>
public enum CustomerPriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

/// <summary>
/// Customer entity for managing customer information
/// </summary>
public class Customer : BaseEntity
{
    // Basic Information
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? SecondaryEmail { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? MobilePhone { get; set; }
    public string Company { get; set; } = string.Empty;
    public string? JobTitle { get; set; }
    public string? Website { get; set; }
    
    // Address Information
    public string Address { get; set; } = string.Empty;
    public string? Address2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    
    // Business Information
    public string? Industry { get; set; }
    public int? NumberOfEmployees { get; set; }
    public decimal AnnualRevenue { get; set; } = 0;
    public CustomerType CustomerType { get; set; } = CustomerType.Individual;
    public CustomerPriority Priority { get; set; } = CustomerPriority.Medium;
    
    // Lifecycle & Status
    public CustomerLifecycleStage LifecycleStage { get; set; } = CustomerLifecycleStage.Lead;
    public string? LeadSource { get; set; } // How did they find us?
    public DateTime? FirstContactDate { get; set; }
    public DateTime? ConversionDate { get; set; } // When became a customer
    public DateTime? LastActivityDate { get; set; }
    public DateTime? NextFollowUpDate { get; set; }
    
    // Financial
    public decimal TotalPurchases { get; set; } = 0;
    public decimal AccountBalance { get; set; } = 0;
    public decimal CreditLimit { get; set; } = 0;
    public string? PaymentTerms { get; set; } // Net 30, Net 60, etc.
    public string? PreferredPaymentMethod { get; set; }
    
    // Scoring & Rating
    public int LeadScore { get; set; } = 0; // 0-100
    public int CustomerHealthScore { get; set; } = 50; // 0-100
    public int NpsScore { get; set; } = 0; // -100 to 100
    public double SatisfactionRating { get; set; } = 0; // 0-5
    
    // Social & Communication
    public string? LinkedInUrl { get; set; }
    public string? TwitterHandle { get; set; }
    public string? FacebookUrl { get; set; }
    public bool OptInEmail { get; set; } = true;
    public bool OptInSms { get; set; } = false;
    public bool OptInPhone { get; set; } = true;
    public string? PreferredContactMethod { get; set; }
    public string? PreferredContactTime { get; set; }
    public string? Timezone { get; set; }
    
    // Assignment & Ownership
    public int? AssignedToUserId { get; set; }
    public int? AccountManagerId { get; set; }
    public string? Territory { get; set; }
    
    // Classification
    public string? Tags { get; set; } // Comma-separated tags
    public string? Segment { get; set; } // Customer segment
    public string? ReferralSource { get; set; }
    public int? ReferredByCustomerId { get; set; }
    
    // Documentation
    public string Notes { get; set; } = string.Empty;
    public string? InternalNotes { get; set; }
    public string? Description { get; set; }
    
    // Custom Fields (JSON for flexibility)
    public string? CustomFields { get; set; } // JSON object for custom data

    // Navigation properties
    public ICollection<Opportunity>? Opportunities { get; set; }
    public ICollection<Interaction>? Interactions { get; set; }
    public User? AssignedToUser { get; set; }
    public User? AccountManager { get; set; }
    public Customer? ReferredByCustomer { get; set; }
}
