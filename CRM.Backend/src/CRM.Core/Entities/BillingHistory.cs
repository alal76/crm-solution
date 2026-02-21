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
/// Billing event types for audit trail and workflow tracking.
/// </summary>
public enum BillingEventType
{
    Created = 0,
    Activated = 1,
    PlanChanged = 2,
    Invoiced = 3,
    Cancelled = 4,
    Renewed = 5,
    Paused = 6,
    Resumed = 7,
    Suspended = 8,
    PaymentCollected = 9,
    PaymentFailed = 10,
    ProrationApplied = 11,
    UsageChargeApplied = 12
}

/// <summary>
/// Billing History Entity - Audit trail for all subscription billing events.
/// Tracks: invoice generation, payment collection, plan changes, prorations, renewals.
/// Each record documents WHEN (date), WHAT (event type), HOW MUCH (amount), and WHO (user).
///
/// FINANCIAL PRECISION: Uses DECIMAL(18,4) for all monetary amounts to prevent
/// accumulated floating-point rounding errors in financial calculations.
/// </summary>
[Table("BillingHistory")]
public class BillingHistory : BaseEntity
{
    /// <summary>Subscription this billing event relates to.</summary>
    [Required]
    [ForeignKey("Subscription")]
    public int SubscriptionId { get; set; }
    public Subscription? Subscription { get; set; }

    /// <summary>Related invoice (if applicable).</summary>
    [ForeignKey("Invoice")]
    public int? InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }

    /// <summary>Cycle start date (inclusive) for this billing period.</summary>
    public DateTime CycleStartDate { get; set; }

    /// <summary>Cycle end date (inclusive) for this billing period.</summary>
    public DateTime CycleEndDate { get; set; }

    /// <summary>
    /// Amount charged in this billing cycle.
    /// PRECISION: DECIMAL(18,4) = $999,999,999,999.9999 max
    /// Stores exact financial amounts without floating-point errors.
    /// </summary>
    [Column(TypeName = "DECIMAL(18,4)")]
    public decimal Amount { get; set; }

    /// <summary>Prorated amount (if subscription changed mid-cycle).</summary>
    [Column(TypeName = "DECIMAL(18,4)")]
    public decimal? ProratedAmount { get; set; }

    /// <summary>Usage-based additional charges.</summary>
    [Column(TypeName = "DECIMAL(18,4)")]
    public decimal? UsageCharges { get; set; }

    /// <summary>Discount or credit applied.</summary>
    [Column(TypeName = "DECIMAL(18,4)")]
    public decimal? DiscountAmount { get; set; }

    /// <summary>Tax applied (if applicable).</summary>
    [Column(TypeName = "DECIMAL(18,4)")]
    public decimal? TaxAmount { get; set; }

    /// <summary>Event type: Created, Invoiced, Renewed, Cancelled, etc.</summary>
    [Required]
    public BillingEventType EventType { get; set; } = BillingEventType.Invoiced;

    /// <summary>Human-readable description of the billing event.</summary>
    [MaxLength(500)]
    public string? EventDetails { get; set; }

    /// <summary>User who triggered this billing event (if manual).</summary>
    [ForeignKey("User")]
    public int? UserId { get; set; }
    public User? User { get; set; }

    /// <summary>When the event occurred (UTC).</summary>
    public DateTime EventDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Billing status: Pending (draft), Billed (invoice sent), Paid (payment received),
    /// Failed (payment failed), WrittenOff (bad debt).
    /// </summary>
    [MaxLength(50)]
    public string Status { get; set; } = "Pending";

    /// <summary>When this billing period was actually billed to customer.</summary>
    public DateTime? BilledDate { get; set; }

    /// <summary>When payment was received (if status = Paid).</summary>
    public DateTime? PaidDate { get; set; }

    /// <summary>
    /// Reference to the dunning record if this billing failed and entered dunning.
    /// Enables correlation of failed payments with retry attempts.
    /// </summary>
    [ForeignKey("DunningRecord")]
    public int? DunningRecordId { get; set; }
    public DunningRecord? DunningRecord { get; set; }
}
