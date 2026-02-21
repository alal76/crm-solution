// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Core.Entities;

/// <summary>
/// Subscription status values for lifecycle management.
/// </summary>
public enum SubscriptionStatus
{
    Active = 0,
    Paused = 1,
    Cancelled = 2,
    Suspended = 3,
    PendingCancellation = 4,
    Expired = 5,
    Trial = 6,
    Current = 0, // Alias for Active (backward compatibility)
    Churned = 2 // Alias for Cancelled (backward compatibility)
}

/// <summary>
/// Subscription entity representing an account's recurring subscription/purchase.
/// Tightly linked to an Account (formerly Customer) and optionally a Product/Service.
/// Includes billing information and contract document storage.
/// Note: This was previously named "Account" but renamed to "Subscription" to avoid
/// confusion with the Account entity (the company/person, formerly Customer).
/// </summary>
[Table("Subscriptions")]
public class Subscription : BaseEntity
{
    #region Identification

    /// <summary>Unique subscription number</summary>
    [Required]
    [MaxLength(50)]
    public string SubscriptionNumber { get; set; } = string.Empty;

    #endregion

    // Relationships - linked to Account (formerly Customer)
    public int AccountId { get; set; }
    public Account? Account { get; set; }

    public int? ProductId { get; set; }
    public Product? Product { get; set; }

    // Status (Current / Churned)
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

    /// <summary>Billing period enumeration (computed from BillingCycle string)</summary>
    [NotMapped]
    public CRM.Core.Interfaces.BillingPeriod BillingPeriod
    {
        get => BillingCycle?.ToLowerInvariant() switch
        {
            "weekly" => CRM.Core.Interfaces.BillingPeriod.Weekly,
            "quarterly" => CRM.Core.Interfaces.BillingPeriod.Quarterly,
            "yearly" or "annual" => CRM.Core.Interfaces.BillingPeriod.Yearly,
            _ => CRM.Core.Interfaces.BillingPeriod.Monthly
        };
        set => BillingCycle = value switch
        {
            CRM.Core.Interfaces.BillingPeriod.Weekly => "Weekly",
            CRM.Core.Interfaces.BillingPeriod.Monthly => "Monthly",
            CRM.Core.Interfaces.BillingPeriod.Quarterly => "Quarterly",
            CRM.Core.Interfaces.BillingPeriod.Yearly => "Yearly",
            _ => "Monthly"
        };
    }

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
    public string? SubscriptionOwner { get; set; }

    /// <summary>Subscription manager user ID</summary>
    [ForeignKey("SubscriptionManager")]
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

    #region Service-Required Properties

    /// <summary>Related order ID (if created from order)</summary>
    public int? OrderId { get; set; }

    /// <summary>Subscription amount</summary>
    public decimal Amount { get; set; }

    /// <summary>Subscription start date</summary>
    public DateTime? StartDate { get; set; }

    /// <summary>Subscription end date</summary>
    public DateTime? EndDate { get; set; }

    /// <summary>Next billing date</summary>
    public DateTime? NextBillingDate { get; set; }

    /// <summary>Current billing period end date</summary>
    public DateTime? CurrentPeriodEnd { get; set; }

    /// <summary>Current billing period start date</summary>
    public DateTime? CurrentPeriodStart { get; set; }

    /// <summary>Cancellation date if cancelled</summary>
    public DateTime? CancelledAt { get; set; }

    /// <summary>Cancellation reason</summary>
    public string? CancellationReason { get; set; }

    /// <summary>Whether subscription is pending cancellation at period end</summary>
    public bool CancelAtPeriodEnd { get; set; }

    /// <summary>Paused at date</summary>
    public DateTime? PausedAt { get; set; }

    /// <summary>Pause reason</summary>
    public string? PauseReason { get; set; }

    #endregion
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

    /// <summary>Metric name for usage tracking</summary>
    public string MetricName { get; set; } = string.Empty;

    /// <summary>Timestamp of usage</summary>
    public DateTime? Timestamp { get; set; }
}

/// <summary>
/// Usage limit definition for a subscription/metric pair.
/// </summary>
public class SubscriptionUsageLimit : BaseEntity
{
    public int SubscriptionId { get; set; }
    public Subscription? Subscription { get; set; }

    [MaxLength(100)]
    public string MetricName { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal Limit { get; set; }

    /// <summary>Unit label (e.g., seats, GB).</summary>
    [MaxLength(50)]
    public string? Unit { get; set; }

    /// <summary>When true, usage above the limit should be rejected.</summary>
    public bool EnforceCap { get; set; }
}
