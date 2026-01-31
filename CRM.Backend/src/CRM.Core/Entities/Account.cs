using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
    #region Identification
    
    /// <summary>Unique account/contract number</summary>
    [Required]
    [MaxLength(50)]
    public string AccountNumber { get; set; } = string.Empty;
    
    #endregion

    // Relationships
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public int? ProductId { get; set; }
    public Product? Product { get; set; }

    // Status (Current / Churned)
    public AccountStatus Status { get; set; } = AccountStatus.Current;

    #region Financial
    
    /// <summary>Monthly recurring revenue</summary>
    [Range(0, double.MaxValue)]
    public decimal? MRR { get; set; }
    
    /// <summary>Annual recurring revenue</summary>
    [Range(0, double.MaxValue)]
    public decimal? ARR { get; set; }
    
    /// <summary>One-time fee amount</summary>
    [Range(0, double.MaxValue)]
    public decimal? OneTimeFee { get; set; }
    
    /// <summary>Currency code (3-letter ISO)</summary>
    [MaxLength(3)]
    public string? Currency { get; set; }
    
    public int? CurrencyLookupId { get; set; }
    
    /// <summary>Billing cycle (Monthly, Quarterly, Annual)</summary>
    [MaxLength(50)]
    public string? BillingCycle { get; set; }
    
    public DateTime? BillingStartDate { get; set; }
    public DateTime? BillingEndDate { get; set; }
    
    #endregion

    #region Contract
    
    /// <summary>Contract reference number</summary>
    [MaxLength(100)]
    public string? ContractReference { get; set; }
    
    public DateTime? ContractStartDate { get; set; }
    public DateTime? ContractEndDate { get; set; }
    public ContractTermCategory? TermCategory { get; set; }
    public ServiceTier? ServiceTier { get; set; }
    
    /// <summary>Service Level Agreement details</summary>
    [MaxLength(255)]
    public string? SLA { get; set; }
    
    /// <summary>Contract notes</summary>
    public string? ContractNotes { get; set; }
    
    #endregion

    #region Billing Address
    
    /// <summary>Billing street address</summary>
    [MaxLength(255)]
    public string? BillingAddress { get; set; }
    
    /// <summary>Billing city</summary>
    [MaxLength(100)]
    public string? BillingCity { get; set; }
    
    /// <summary>Billing state/province</summary>
    [MaxLength(100)]
    public string? BillingState { get; set; }
    
    /// <summary>Billing ZIP/postal code</summary>
    [MaxLength(20)]
    public string? BillingZip { get; set; }
    
    /// <summary>Billing country</summary>
    [MaxLength(100)]
    public string? BillingCountry { get; set; }
    
    /// <summary>Billing contact name</summary>
    [MaxLength(255)]
    public string? BillingContactName { get; set; }
    
    /// <summary>Billing contact email</summary>
    [MaxLength(255)]
    [EmailAddress]
    public string? BillingContactEmail { get; set; }
    
    /// <summary>Billing contact phone</summary>
    [MaxLength(30)]
    [Phone]
    public string? BillingContactPhone { get; set; }
    
    #endregion

    public LookupItem? CurrencyLookup { get; set; }

    #region Contract Document
    
    /// <summary>Uploaded contract file name</summary>
    [MaxLength(255)]
    public string? ContractFileName { get; set; }
    
    /// <summary>Path to contract file on storage</summary>
    [MaxLength(500)]
    public string? ContractFilePath { get; set; }
    
    /// <summary>MIME type of contract file</summary>
    [MaxLength(100)]
    public string? ContractContentType { get; set; }
    
    /// <summary>Contract file size in bytes</summary>
    public long? ContractFileSize { get; set; }
    
    #endregion

    #region Operational
    
    /// <summary>Whether contract auto-renews</summary>
    public bool IsAutoRenew { get; set; } = false;
    
    public DateTime? RenewalDate { get; set; }
    
    /// <summary>Whether account is currently active</summary>
    public bool IsActive { get; set; } = true;
    
    #endregion

    #region Assignment & Metadata
    
    /// <summary>Account owner name</summary>
    [MaxLength(255)]
    public string? AccountOwner { get; set; }
    
    /// <summary>Account manager user ID</summary>
    [ForeignKey("AccountManager")]
    public int? AccountManagerId { get; set; }
    
    /// <summary>Tags (comma-separated)</summary>
    [MaxLength(500)]
    public string? Tags { get; set; }
    
    #endregion

    // Navigation collections
    public ICollection<Opportunity>? Opportunities { get; set; }

    // Contact information links (addresses, phones/emails, social accounts)
    public ICollection<ContactInfoLink>? ContactInfoLinks { get; set; }

    // Validation helper
    [MaxLength(50)]
    public string? ExternalReference { get; set; }
}
