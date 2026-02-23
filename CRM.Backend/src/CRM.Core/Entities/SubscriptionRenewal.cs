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
/// Status of a subscription renewal attempt.
/// </summary>
public enum SubscriptionRenewalStatus
{
    /// <summary>Renewal is scheduled and awaiting processing.</summary>
    Pending = 0,

    /// <summary>Renewal was processed and payment collected successfully.</summary>
    Completed = 1,

    /// <summary>Renewal attempt failed (payment declined or other error).</summary>
    Failed = 2,

    /// <summary>Renewal was intentionally skipped (e.g., manual override or trial extension).</summary>
    Skipped = 3
}

/// <summary>
/// SubscriptionRenewal Entity - Represents a single renewal event in the lifecycle
/// of a recurring subscription.
///
/// Each time a subscription is renewed (automatically or manually), a SubscriptionRenewal
/// record is created to document: when the renewal occurred, the billing period covered,
/// the amount charged, and whether it succeeded or failed.
///
/// BUSINESS PURPOSE:
/// - Provides a complete audit trail of subscription renewal history.
/// - Enables dunning workflows to track failed renewals requiring retry.
/// - Allows finance to reconcile renewals against invoices and payments.
/// - Supports analytics: renewal rates, churn prediction, ARR forecasting.
///
/// FINANCIAL PRECISION:
/// Uses DECIMAL(18,4) for Amount to prevent accumulated floating-point rounding
/// in financial calculations across reporting periods.
/// </summary>
[Table("SubscriptionRenewals")]
public class SubscriptionRenewal : BaseEntity
{
    /// <summary>
    /// The subscription this renewal event belongs to.
    /// Required — every renewal must be linked to a subscription.
    /// </summary>
    [Required]
    [ForeignKey("Subscription")]
    public int SubscriptionId { get; set; }

    /// <summary>Navigation property: the parent subscription.</summary>
    public Subscription? Subscription { get; set; }

    /// <summary>
    /// Date the renewal was executed or attempted (UTC).
    /// For automatic renewals, this is the scheduled renewal date.
    /// For manual renewals, this is when the agent triggered it.
    /// </summary>
    public DateTime RenewalDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Current status of this renewal attempt.
    /// See <see cref="SubscriptionRenewalStatus"/> for lifecycle values.
    /// </summary>
    [Required]
    public SubscriptionRenewalStatus Status { get; set; } = SubscriptionRenewalStatus.Pending;

    /// <summary>
    /// Amount charged (or attempted) for this renewal cycle.
    /// PRECISION: DECIMAL(18,4) — supports amounts up to $999,999,999,999.9999.
    /// </summary>
    [Column(TypeName = "DECIMAL(18,4)")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Start date of the billing period covered by this renewal (inclusive, UTC).
    /// Example: 2026-03-01 for a March billing cycle.
    /// </summary>
    public DateTime BillingPeriodStart { get; set; }

    /// <summary>
    /// End date of the billing period covered by this renewal (inclusive, UTC).
    /// Example: 2026-03-31 for a March billing cycle.
    /// </summary>
    public DateTime BillingPeriodEnd { get; set; }

    /// <summary>
    /// Optional reference to the invoice generated for this renewal.
    /// Nullable — invoice may be created asynchronously after the renewal event.
    /// </summary>
    [ForeignKey("Invoice")]
    public int? InvoiceId { get; set; }

    /// <summary>Navigation property: the invoice linked to this renewal.</summary>
    public Invoice? Invoice { get; set; }

    /// <summary>
    /// Optional free-text notes about this renewal.
    /// Used for manual overrides, failure explanations, or agent comments.
    /// Maximum 1000 characters.
    /// </summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }
}
