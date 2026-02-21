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
/// Dunning Manager Status - Tracks failed payment recovery workflow state.
/// </summary>
public enum DunningStatus
{
    /// <summary>Payment failed; dunning process actively attempting recovery.</summary>
    Active = 0,

    /// <summary>Payment succeeded; dunning ended successfully.</summary>
    Resolved = 1,

    /// <summary>All 3 retry attempts exhausted; subscription to be cancelled/suspended.</summary>
    Exhausted = 2,

    /// <summary>Manual write-off: debt forgiven, subscription cancelled.</summary>
    WrittenOff = 3,

    /// <summary>Grace period active (customer contacted to resolve).</summary>
    GracePeriod = 4
}

/// <summary>
/// Dunning Record Entity - Payment failure recovery and retry tracking.
///
/// Implements the Dunning strategy:
/// - Attempt 1: +3 days after initial failure
/// - Attempt 2: +6 days after Attempt 1
/// - Attempt 3: +9 days after Attempt 2
/// - After 3 failures: Auto-cancel OR manual collection
///
/// BUSINESS LOGIC:
/// When an invoice payment fails, a DunningRecord is created with RetryAttempt=0.
/// A background job (Hangfire) checks daily for NextRetryDate <= Today.
/// On due date, the system attempts payment via IPaymentService.
/// If success: mark Resolved, resume billing.
/// If failure: increment RetryAttempt, extend NextRetryDate, send escalation email.
/// If RetryAttempt > 3: mark Exhausted, flag subscription for cancellation.
///
/// FINANCIAL IMPACT:
/// - Dunning extends invoice lifecycle by up to 27 days
/// - May trigger grace period (3 additional days without charge)
/// - Prevents customer access during grace period
/// - Sends escalation notifications: Day 3, Day 10, Day 24
/// </summary>
[Table("DunningRecords")]
public class DunningRecord : BaseEntity
{
    /// <summary>Subscription affected by payment failure.</summary>
    [Required]
    [ForeignKey("Subscription")]
    public int SubscriptionId { get; set; }
    public Subscription? Subscription { get; set; }

    /// <summary>Invoice that failed payment.</summary>
    [Required]
    [ForeignKey("Invoice")]
    public int InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }

    /// <summary>Current retry attempt: 1, 2, or 3 (max 3 before exhaustion).</summary>
    public int RetryAttempt { get; set; } = 1;

    /// <summary>When the next automatic retry will be attempted.</summary>
    public DateTime NextRetryDate { get; set; }

    /// <summary>Status of dunning process: Active, Resolved, Exhausted, WrittenOff, GracePeriod.</summary>
    [Required]
    public DunningStatus Status { get; set; } = DunningStatus.Active;

    /// <summary>Reason for dunning: "PaymentDeclined", "InsufficientFunds", "CardExpired", etc.</summary>
    [MaxLength(200)]
    public string? Reason { get; set; }

    /// <summary>Error message from last payment attempt (for diagnosis).</summary>
    [MaxLength(500)]
    public string? LastErrorMessage { get; set; }

    /// <summary>
    /// When the first payment failure occurred (UTC).
    /// Used to calculate age of dunning record for escalation decisions.
    /// </summary>
    public DateTime InitialFailureDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Email address dunning notifications were sent to.
    /// Stored for audit trail and potential manual follow-up.
    /// </summary>
    [MaxLength(255)]
    [EmailAddress]
    public string? NotificationEmail { get; set; }

    /// <summary>Whether dunning has exhausted all retries (3 attempts failed).</summary>
    public bool IsExhausted { get; set; } = false;

    /// <summary>When the subscription was actually cancelled due to dunning exhaustion.</summary>
    public DateTime? CancelledAt { get; set; }

    /// <summary>Grace period end date (if GracePeriod status active).</summary>
    public DateTime? GracePeriodEndDate { get; set; }

    /// <summary>
    /// Amount being recovered (DECIMAL(18,4)).
    /// May differ from invoice total if partial payment applied.
    /// </summary>
    [Column(TypeName = "DECIMAL(18,4)")]
    public decimal OutstandingAmount { get; set; }

    /// <summary>
    /// Total amount successfully recovered through dunning process.
    /// Incremented as retries succeed.
    /// </summary>
    [Column(TypeName = "DECIMAL(18,4)")]
    public decimal? RecoveredAmount { get; set; }

    /// <summary>Notes added during dunning process (escalation notes, agent interactions).</summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }

    /// <summary>Related billing history record (for audit trail).</summary>
    [ForeignKey("BillingHistory")]
    public int? BillingHistoryId { get; set; }
    public BillingHistory? BillingHistory { get; set; }
}
