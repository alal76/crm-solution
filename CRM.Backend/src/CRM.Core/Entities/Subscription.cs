using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Core.Entities;

/// <summary>
/// SubscriptionStatus limited to Current or Churned
/// </summary>
public enum SubscriptionStatus
{
    Current = 0,
    Churned = 1
}

/// <summary>
/// Subscription entity representing an account's recurring subscription/purchase.
/// Tightly linked to an Account (formerly Customer) and optionally a Product/Service.
/// Includes billing information and contract document storage.
/// Note: This was previously named "Account" but renamed to "Subscription" to avoid
/// confusion with the Account entity (the company/person, formerly Customer).
/// </summary>
[Table("Accounts")] // Keep original table name for backward compatibility
public class Subscription : BaseEntity
{
    #region Identification
    
    /// <summary>Unique subscription number</summary>
    [Required]
    [MaxLength(50)]
    [Column("AccountNumber")]
    public string SubscriptionNumber { get; set; } = string.Empty;
    
    #endregion

    // Relationships - linked to Account (formerly Customer)
    [Column("CustomerId")]
    public int AccountId { get; set; }
    public Account? Account { get; set; }

    public int? ProductId { get; set; }
    public Product? Product { get; set; }

    // Status (Current / Churned)
    [Column("Status")]
    public SubscriptionStatus SubscriptionStatus { get; set; } = SubscriptionStatus.Current;

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
    
    /// <summary>Subscription owner name</summary>
    [MaxLength(255)]
    [Column("AccountOwner")]
    public string? SubscriptionOwner { get; set; }
    
    /// <summary>Subscription manager user ID</summary>
    [ForeignKey("SubscriptionManager")]
    [Column("AccountManagerId")]
    public int? SubscriptionManagerId { get; set; }
    
    /// <summary>Tags (comma-separated)</summary>
    [MaxLength(500)]
    public string? Tags { get; set; }
    
    #endregion

    // Navigation collections
    public ICollection<Opportunity>? Opportunities { get; set; }

    // Contact information links (addresses, phones/emails, social accounts)
    public ICollection<ContactInfoLink>? ContactInfoLinks { get; set; }
    
    // Subscription manager navigation
    public User? SubscriptionManager { get; set; }

    // Validation helper
    [MaxLength(50)]
    public string? ExternalReference { get; set; }
}

#region Backward Compatibility Alias

/// <summary>
/// Backward compatibility alias for AccountStatus - use SubscriptionStatus instead
/// </summary>
[Obsolete("Use SubscriptionStatus instead. AccountStatus has been renamed.")]
public enum AccountStatus
{
    Current = 0,
    Churned = 1
}

#endregion

/// <summary>
/// Subscription line item for a subscription
/// </summary>
public class SubscriptionItem : BaseEntity
{
    public int SubscriptionId { get; set; }
    public Subscription? Subscription { get; set; }
    
    public int? ProductId { get; set; }
    public Product? Product { get; set; }
    
    public string? ItemName { get; set; }
    public string? Description { get; set; }
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

/// <summary>
/// Usage record for a subscription
/// </summary>
public class SubscriptionUsage : BaseEntity
{
    public int SubscriptionId { get; set; }
    public Subscription? Subscription { get; set; }
    
    public int? SubscriptionItemId { get; set; }
    public SubscriptionItem? SubscriptionItem { get; set; }
    
    public DateTime UsageDate { get; set; }
    public decimal Quantity { get; set; }
    public string? UsageType { get; set; }
    public string? Description { get; set; }
}
