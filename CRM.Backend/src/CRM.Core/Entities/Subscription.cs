// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CRM.Core.Entities.Events;
using CRM.Core.Exceptions;
using CRM.Core.Ports.Output.Events;

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
public class Subscription : BaseEntity, IHasDomainEvents
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

    /// <summary>Scheduled auto-resume date (null = manual resume only)</summary>
    public DateTime? ResumeAt { get; set; }

    #endregion

    #region Trial Properties (TODO-SALES006-019)

    /// <summary>Trial period start date (null = no trial)</summary>
    public DateTime? TrialStartDate { get; set; }

    /// <summary>Trial period end date (must be > TrialStartDate)</summary>
    public DateTime? TrialEndDate { get; set; }

    /// <summary>How proration is calculated for mid-cycle changes</summary>
    public ProrationStrategy ProrationType { get; set; } = ProrationStrategy.Daily;

    #endregion

    #region Timezone & Billing Configuration (TODO-SALES006-023)

    /// <summary>
    /// Timezone identifier for billing date calculations (IANA format).
    /// Example: "America/New_York", "Europe/London", "UTC"
    /// </summary>
    [MaxLength(100)]
    public string? BillingTimezone { get; set; }

    #endregion

    #region Payment Gateway (REV-STUB-012/013)

    /// <summary>
    /// Stripe Customer ID (cus_xxx) for this subscription's billing account.
    /// Populated when the customer completes checkout with a saved payment method.
    /// Null for subscriptions created before Stripe checkout capture was wired up, or for
    /// subscriptions billed via a non-Stripe payment method — dunning retries gracefully skip
    /// the real charge attempt (see DunningManager.RetryFailedPaymentAsync) when this is null.
    /// </summary>
    [MaxLength(255)]
    public string? StripeCustomerId { get; set; }

    /// <summary>
    /// Stripe PaymentMethod ID (pm_xxx) saved on file for off-session/dunning retry charges.
    /// Null until the subscription checkout flow captures and saves a payment method
    /// (that capture flow is a separate, not-yet-implemented change).
    /// </summary>
    [MaxLength(255)]
    public string? StripePaymentMethodId { get; set; }

    #endregion

    #region Dunning Configuration (TODO-SALES006-025)

    /// <summary>Grace period in days before full suspension (default 3 days)</summary>
    public int DunningGracePeriodDays { get; set; } = 3;

    /// <summary>Whether escalation emails should be sent during dunning</summary>
    public bool SendDunningEscalationEmails { get; set; } = true;

    /// <summary>Email addresses for dunning notifications (comma-separated)</summary>
    [MaxLength(500)]
    public string? DunningNotificationEmails { get; set; }

    /// <summary>Date when dunning process was last triggered</summary>
    public DateTime? LastDunningDate { get; set; }

    /// <summary>Current dunning attempt count</summary>
    public int DunningAttemptCount { get; set; }

    #endregion

    #region Concurrency Control (TODO-SALES006-022)

    /// <summary>
    /// Optimistic concurrency token for preventing concurrent updates.
    /// EF Core uses this for conflict detection.
    /// </summary>
    [Timestamp]
    public new byte[]? RowVersion { get; set; }

    #endregion

    #region Domain Events

    private readonly List<IDomainEvent> _domainEvents = new();

    /// <inheritdoc />
    [NotMapped]
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <inheritdoc />
    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    /// <inheritdoc />
    public void RemoveDomainEvent(IDomainEvent domainEvent) => _domainEvents.Remove(domainEvent);

    /// <inheritdoc />
    public void ClearDomainEvents() => _domainEvents.Clear();

    #endregion

    #region Business Methods

    /// <summary>
    /// Cancels the subscription with a reason. Throws if already cancelled or expired.
    /// </summary>
    public void Cancel(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new BusinessRuleException("Subscription.Cancel", "Cancellation reason is required.");
        if (SubscriptionStatus == SubscriptionStatus.Cancelled)
            throw new BusinessRuleException("Subscription.Cancel", "Subscription is already cancelled.");
        if (SubscriptionStatus == SubscriptionStatus.Expired)
            throw new BusinessRuleException("Subscription.Cancel", "Cannot cancel an expired subscription.");

        SubscriptionStatus = SubscriptionStatus.Cancelled;
        IsActive = false;
        CancelledAt = DateTime.UtcNow;
        CancellationReason = reason;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new SubscriptionCancelledEvent(Id, reason, DateTime.UtcNow));
    }

    /// <summary>
    /// Reinstates a cancelled or paused subscription. Throws if already active or expired.
    /// </summary>
    public void Reinstate()
    {
        if (SubscriptionStatus == SubscriptionStatus.Active)
            throw new BusinessRuleException("Subscription.Reinstate", "Subscription is already active.");
        if (SubscriptionStatus == SubscriptionStatus.Expired)
            throw new BusinessRuleException("Subscription.Reinstate", "Cannot reinstate an expired subscription.");
        if (SubscriptionStatus != SubscriptionStatus.Cancelled && SubscriptionStatus != SubscriptionStatus.Paused && SubscriptionStatus != SubscriptionStatus.Suspended)
            throw new BusinessRuleException("Subscription.Reinstate", "Subscription must be Cancelled, Paused, or Suspended to reinstate.");

        SubscriptionStatus = SubscriptionStatus.Active;
        IsActive = true;
        CancelledAt = null;
        CancellationReason = null;
        CancelAtPeriodEnd = false;
        PausedAt = null;
        ResumeAt = null;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new SubscriptionReinstatedEvent(Id, DateTime.UtcNow));
    }

    /// <summary>
    /// Factory method for unit testing — creates a Subscription with specified status.
    /// </summary>
    internal static Subscription CreateForTesting(
        SubscriptionStatus status = SubscriptionStatus.Active,
        bool isActive = true)
    {
        return new Subscription
        {
            Id = 1,
            SubscriptionStatus = status,
            IsActive = isActive,
            SubscriptionNumber = "SUB-TEST-001",
            CreatedAt = DateTime.UtcNow
        };
    }

    #endregion
}

/// <summary>
/// Strategy for calculating prorated amounts during mid-cycle subscription changes.
/// </summary>
public enum ProrationStrategy
{
    /// <summary>Prorate based on exact days remaining in period</summary>
    Daily = 0,

    /// <summary>Prorate based on half-month increments</summary>
    HalfMonth = 1,

    /// <summary>Prorate based on full months only (no partial)</summary>
    FullMonth = 2,

    /// <summary>No proration - charge full amount immediately</summary>
    None = 3,

    /// <summary>Credit remaining balance to next invoice</summary>
    Credit = 4
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
