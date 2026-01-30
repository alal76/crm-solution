using CRM.Core.Models;

namespace CRM.Core.Entities;

#region Subscription Enumerations

/// <summary>
/// FUNCTIONAL: Subscription lifecycle status.
/// TECHNICAL: Controls billing, access, and renewal automation.
/// </summary>
public enum SubscriptionStatus
{
    /// <summary>Subscription pending activation</summary>
    Pending = 0,
    
    /// <summary>Trial period active</summary>
    Trial = 1,
    
    /// <summary>Subscription is active and billing</summary>
    Active = 2,
    
    /// <summary>Subscription paused (billing suspended)</summary>
    Paused = 3,
    
    /// <summary>Past due - payment failed</summary>
    PastDue = 4,
    
    /// <summary>In grace period after failed payment</summary>
    GracePeriod = 5,
    
    /// <summary>Pending cancellation (will cancel at end of period)</summary>
    PendingCancellation = 6,
    
    /// <summary>Cancelled by customer or admin</summary>
    Cancelled = 7,
    
    /// <summary>Expired (term ended)</summary>
    Expired = 8,
    
    /// <summary>Suspended due to non-payment</summary>
    Suspended = 9,
    
    /// <summary>Pending renewal</summary>
    PendingRenewal = 10,
    
    /// <summary>Renewed successfully</summary>
    Renewed = 11,
    
    /// <summary>Upgraded to different plan</summary>
    Upgraded = 12,
    
    /// <summary>Downgraded to different plan</summary>
    Downgraded = 13,
    
    /// <summary>Terminated (ended early)</summary>
    Terminated = 14
}

/// <summary>
/// FUNCTIONAL: How to prorate subscription changes.
/// TECHNICAL: Determines billing adjustment calculations.
/// </summary>
public enum ProrationType
{
    /// <summary>No proration - changes apply next period</summary>
    None = 0,
    
    /// <summary>Prorate immediately with credit/charge</summary>
    Immediate = 1,
    
    /// <summary>Prorate at next billing cycle</summary>
    NextCycle = 2,
    
    /// <summary>Custom proration</summary>
    Custom = 3
}

/// <summary>
/// FUNCTIONAL: Cancellation reason categories.
/// TECHNICAL: Drives churn analysis and retention workflows.
/// </summary>
public enum CancellationReason
{
    /// <summary>Customer requested cancellation</summary>
    CustomerRequest = 0,
    
    /// <summary>Non-payment</summary>
    NonPayment = 1,
    
    /// <summary>Too expensive</summary>
    TooExpensive = 2,
    
    /// <summary>Missing features</summary>
    MissingFeatures = 3,
    
    /// <summary>Found alternative solution</summary>
    CompetitorSwitch = 4,
    
    /// <summary>No longer needed</summary>
    NoLongerNeeded = 5,
    
    /// <summary>Business closed</summary>
    BusinessClosed = 6,
    
    /// <summary>Technical issues</summary>
    TechnicalIssues = 7,
    
    /// <summary>Poor support</summary>
    PoorSupport = 8,
    
    /// <summary>Contract violation</summary>
    ContractViolation = 9,
    
    /// <summary>Fraud</summary>
    Fraud = 10,
    
    /// <summary>Other reason</summary>
    Other = 11
}

#endregion

/// <summary>
/// Subscription entity for recurring revenue management.
/// Drives billing automation, renewal, and churn tracking.
/// </summary>
public class Subscription : BaseEntity
{
    #region Identification
    
    /// <summary>System-generated subscription number</summary>
    public string SubscriptionNumber { get; set; } = string.Empty;
    
    /// <summary>External subscription reference</summary>
    public string? ExternalSubscriptionId { get; set; }
    
    /// <summary>Gateway subscription ID (Stripe, etc.)</summary>
    public string? GatewaySubscriptionId { get; set; }
    
    #endregion
    
    #region Subscription Details
    
    /// <summary>Subscription name/description</summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>Detailed description</summary>
    public string? Description { get; set; }
    
    /// <summary>Current subscription status</summary>
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Pending;
    
    /// <summary>Billing frequency</summary>
    public BillingFrequency BillingFrequency { get; set; } = BillingFrequency.Monthly;
    
    /// <summary>How to handle proration on changes</summary>
    public ProrationType ProrationType { get; set; } = ProrationType.Immediate;
    
    #endregion
    
    #region Term Details
    
    /// <summary>Subscription start date</summary>
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>Subscription end date (for fixed-term)</summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>Trial start date</summary>
    public DateTime? TrialStartDate { get; set; }
    
    /// <summary>Trial end date</summary>
    public DateTime? TrialEndDate { get; set; }
    
    /// <summary>Whether currently in trial</summary>
    public bool IsInTrial => TrialEndDate.HasValue && DateTime.UtcNow <= TrialEndDate;
    
    /// <summary>Cancellation date</summary>
    public DateTime? CancelledDate { get; set; }
    
    /// <summary>Effective cancellation date (end of period)</summary>
    public DateTime? CancellationEffectiveDate { get; set; }
    
    /// <summary>Reason for cancellation</summary>
    public CancellationReason? CancellationReason { get; set; }
    
    /// <summary>Cancellation notes</summary>
    public string? CancellationNotes { get; set; }
    
    /// <summary>Term length in months</summary>
    public int? TermLengthMonths { get; set; }
    
    /// <summary>Current term number (for multi-term)</summary>
    public int CurrentTerm { get; set; } = 1;
    
    #endregion
    
    #region Billing Details
    
    /// <summary>Current billing period start</summary>
    public DateTime CurrentPeriodStart { get; set; } = DateTime.UtcNow;
    
    /// <summary>Current billing period end</summary>
    public DateTime CurrentPeriodEnd { get; set; }
    
    /// <summary>Next billing date</summary>
    public DateTime? NextBillingDate { get; set; }
    
    /// <summary>Last billing date</summary>
    public DateTime? LastBillingDate { get; set; }
    
    /// <summary>Number of billing cycles completed</summary>
    public int BillingCycleCount { get; set; } = 0;
    
    /// <summary>Billing day of month (1-28)</summary>
    public int? BillingDayOfMonth { get; set; }
    
    #endregion
    
    #region Pricing
    
    /// <summary>Quantity/seats</summary>
    public decimal Quantity { get; set; } = 1;
    
    /// <summary>Unit price per billing cycle</summary>
    public decimal UnitPrice { get; set; } = 0;
    
    /// <summary>Total price per billing cycle (quantity × unit price)</summary>
    public decimal RecurringAmount { get; set; } = 0;
    
    /// <summary>Discount amount per cycle</summary>
    public decimal DiscountAmount { get; set; } = 0;
    
    /// <summary>Discount percentage</summary>
    public decimal DiscountPercent { get; set; } = 0;
    
    /// <summary>Net amount per billing cycle</summary>
    public decimal NetAmount => RecurringAmount - DiscountAmount;
    
    /// <summary>One-time setup fee</summary>
    public decimal SetupFee { get; set; } = 0;
    
    /// <summary>Currency code (ISO 4217)</summary>
    public string CurrencyCode { get; set; } = "USD";
    
    #endregion
    
    #region Revenue Metrics
    
    /// <summary>Monthly Recurring Revenue</summary>
    public decimal MRR { get; set; } = 0;
    
    /// <summary>Annual Recurring Revenue</summary>
    public decimal ARR { get; set; } = 0;
    
    /// <summary>Total Contract Value</summary>
    public decimal? TCV { get; set; }
    
    /// <summary>Lifetime value to date</summary>
    public decimal LifetimeValue { get; set; } = 0;
    
    /// <summary>Total invoiced to date</summary>
    public decimal TotalInvoiced { get; set; } = 0;
    
    /// <summary>Total paid to date</summary>
    public decimal TotalPaid { get; set; } = 0;
    
    #endregion
    
    #region Renewal
    
    /// <summary>Whether auto-renewal is enabled</summary>
    public bool AutoRenew { get; set; } = true;
    
    /// <summary>Renewal date</summary>
    public DateTime? RenewalDate { get; set; }
    
    /// <summary>Renewal reminder sent</summary>
    public bool RenewalReminderSent { get; set; } = false;
    
    /// <summary>Date renewal reminder was sent</summary>
    public DateTime? RenewalReminderSentDate { get; set; }
    
    /// <summary>Days before renewal to send reminder</summary>
    public int RenewalReminderDays { get; set; } = 30;
    
    /// <summary>Number of times renewed</summary>
    public int RenewalCount { get; set; } = 0;
    
    /// <summary>Price change on renewal</summary>
    public decimal? RenewalPriceChange { get; set; }
    
    /// <summary>New price on renewal</summary>
    public decimal? RenewalPrice { get; set; }
    
    #endregion
    
    #region Payment
    
    /// <summary>Days past due</summary>
    public int DaysPastDue { get; set; } = 0;
    
    /// <summary>Number of failed payment attempts</summary>
    public int FailedPaymentAttempts { get; set; } = 0;
    
    /// <summary>Last failed payment date</summary>
    public DateTime? LastFailedPaymentDate { get; set; }
    
    /// <summary>Grace period end date</summary>
    public DateTime? GracePeriodEndDate { get; set; }
    
    /// <summary>Default payment method ID</summary>
    public string? DefaultPaymentMethodId { get; set; }
    
    #endregion
    
    #region Upgrade/Downgrade
    
    /// <summary>Previous subscription (for upgrades/downgrades)</summary>
    public int? PreviousSubscriptionId { get; set; }
    
    /// <summary>Navigation to previous subscription</summary>
    public Subscription? PreviousSubscription { get; set; }
    
    /// <summary>Upgrade/downgrade effective date</summary>
    public DateTime? ChangeEffectiveDate { get; set; }
    
    /// <summary>Proration credit from change</summary>
    public decimal? ProrationCredit { get; set; }
    
    /// <summary>Proration charge from change</summary>
    public decimal? ProrationCharge { get; set; }
    
    #endregion
    
    #region Relationships
    
    /// <summary>Customer account ID</summary>
    public int AccountId { get; set; }
    
    /// <summary>Navigation to customer account</summary>
    public Account? Account { get; set; }
    
    /// <summary>Primary contact ID</summary>
    public int? ContactId { get; set; }
    
    /// <summary>Navigation to primary contact</summary>
    public Contact? Contact { get; set; }
    
    /// <summary>Product/plan ID</summary>
    public int ProductId { get; set; }
    
    /// <summary>Navigation to product/plan</summary>
    public Product? Product { get; set; }
    
    /// <summary>Source order ID</summary>
    public int? OrderId { get; set; }
    
    /// <summary>Navigation to source order</summary>
    public Order? Order { get; set; }
    
    /// <summary>Source opportunity ID</summary>
    public int? OpportunityId { get; set; }
    
    /// <summary>Navigation to source opportunity</summary>
    public Opportunity? Opportunity { get; set; }
    
    /// <summary>Sales owner ID</summary>
    public int? OwnerId { get; set; }
    
    /// <summary>Navigation to sales owner</summary>
    public User? Owner { get; set; }
    
    /// <summary>Related invoices</summary>
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    
    /// <summary>Related payments</summary>
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    
    /// <summary>Subscription items (for multi-item subscriptions)</summary>
    public ICollection<SubscriptionItem> Items { get; set; } = new List<SubscriptionItem>();
    
    /// <summary>Usage records</summary>
    public ICollection<SubscriptionUsage> UsageRecords { get; set; } = new List<SubscriptionUsage>();
    
    #endregion
    
    #region Notes
    
    /// <summary>Internal notes</summary>
    public string? Notes { get; set; }
    
    #endregion
}

/// <summary>
/// Individual items within a subscription (for multi-product subscriptions).
/// </summary>
public class SubscriptionItem : BaseEntity
{
    /// <summary>Item name</summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>Item description</summary>
    public string? Description { get; set; }
    
    /// <summary>Quantity</summary>
    public decimal Quantity { get; set; } = 1;
    
    /// <summary>Unit price</summary>
    public decimal UnitPrice { get; set; } = 0;
    
    /// <summary>Total amount</summary>
    public decimal TotalAmount { get; set; } = 0;
    
    /// <summary>Billing frequency for this item</summary>
    public BillingFrequency BillingFrequency { get; set; } = BillingFrequency.Monthly;
    
    /// <summary>Parent subscription ID</summary>
    public int SubscriptionId { get; set; }
    
    /// <summary>Navigation to subscription</summary>
    public Subscription? Subscription { get; set; }
    
    /// <summary>Product ID</summary>
    public int? ProductId { get; set; }
    
    /// <summary>Navigation to product</summary>
    public Product? Product { get; set; }
}

/// <summary>
/// Usage record for usage-based subscriptions.
/// </summary>
public class SubscriptionUsage : BaseEntity
{
    /// <summary>Usage period start</summary>
    public DateTime PeriodStart { get; set; }
    
    /// <summary>Usage period end</summary>
    public DateTime PeriodEnd { get; set; }
    
    /// <summary>Quantity used</summary>
    public decimal Quantity { get; set; } = 0;
    
    /// <summary>Unit of measure</summary>
    public string? UnitOfMeasure { get; set; }
    
    /// <summary>Usage type/metric name</summary>
    public string UsageType { get; set; } = string.Empty;
    
    /// <summary>Unit price for usage</summary>
    public decimal? UnitPrice { get; set; }
    
    /// <summary>Total amount for this usage</summary>
    public decimal? TotalAmount { get; set; }
    
    /// <summary>Whether invoiced</summary>
    public bool IsInvoiced { get; set; } = false;
    
    /// <summary>Invoice ID when invoiced</summary>
    public int? InvoiceId { get; set; }
    
    /// <summary>Parent subscription ID</summary>
    public int SubscriptionId { get; set; }
    
    /// <summary>Navigation to subscription</summary>
    public Subscription? Subscription { get; set; }
    
    /// <summary>Subscription item ID (if applicable)</summary>
    public int? SubscriptionItemId { get; set; }
    
    /// <summary>Navigation to subscription item</summary>
    public SubscriptionItem? SubscriptionItem { get; set; }
}
