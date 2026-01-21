using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Entities;

/// <summary>
/// AccountStatus limited to Current or Churned
/// </summary>
public enum AccountStatus
{
    Current = 0,
    Churned = 1
}

/// <summary>
/// Account (or Contract) entity representing a customer's subscription/purchase
/// Tightly linked to a Customer and optionally a Product/Service
/// Includes metadata to store an uploaded physical contract document (file path/metadata)
/// </summary>
public class Account : BaseEntity
{
    // Basic identifiers
    public string AccountNumber { get; set; } = string.Empty;

    // Relationships
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public int? ProductId { get; set; }
    public Product? Product { get; set; }

    // Status (Current / Churned)
    public AccountStatus Status { get; set; } = AccountStatus.Current;

    // Financial fields
    public decimal? MRR { get; set; } // Monthly recurring revenue
    public decimal? ARR { get; set; } // Annual recurring revenue
    public decimal? OneTimeFee { get; set; }
    public string? Currency { get; set; }
    public int? CurrencyLookupId { get; set; }
    public string? BillingCycle { get; set; }
    public DateTime? BillingStartDate { get; set; }
    public DateTime? BillingEndDate { get; set; }

    // Contract / agreement fields
    public string? ContractReference { get; set; }
    public DateTime? ContractStartDate { get; set; }
    public DateTime? ContractEndDate { get; set; }
    public ContractTermCategory? TermCategory { get; set; }
    public ServiceTier? ServiceTier { get; set; }
    public string? SLA { get; set; }
    public string? ContractNotes { get; set; }

    // Billing & payment
    public string? BillingAddress { get; set; }
    public string? BillingCity { get; set; }
    public string? BillingState { get; set; }
    public string? BillingZip { get; set; }
    public string? BillingCountry { get; set; }
    public string? BillingContactName { get; set; }
    public string? BillingContactEmail { get; set; }
    public string? BillingContactPhone { get; set; }

    public LookupItem? CurrencyLookup { get; set; }

    // Contract document storage (file metadata)
    // The system stores the file on disk or object storage; DB contains metadata & path
    public string? ContractFileName { get; set; }
    public string? ContractFilePath { get; set; }
    public string? ContractContentType { get; set; }
    public long? ContractFileSize { get; set; }

    // Operational fields
    public bool IsAutoRenew { get; set; } = false;
    public DateTime? RenewalDate { get; set; }
    public bool IsActive { get; set; } = true;

    // Additional metadata
    public string? AccountOwner { get; set; }
    public int? AccountManagerId { get; set; }
    public string? Tags { get; set; }

    // Navigation collections
    public ICollection<Opportunity>? Opportunities { get; set; }

    // Contact information links (addresses, phones/emails, social accounts)
    public ICollection<ContactInfoLink>? ContactInfoLinks { get; set; }

    // Validation helper
    [MaxLength(50)]
    public string? ExternalReference { get; set; }
}
