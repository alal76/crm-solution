// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

namespace CRM.Core.Entities;

#region Commission Enumerations

/// <summary>
/// FUNCTIONAL: Commission calculation method.
/// TECHNICAL: Determines commission amount calculation.
/// </summary>
public enum CommissionType
{
    /// <summary>Flat percentage of revenue</summary>
    FlatPercentage = 0,

    /// <summary>Tiered percentage based on quota attainment</summary>
    TieredPercentage = 1,

    /// <summary>Fixed amount per deal</summary>
    FixedAmount = 2,

    /// <summary>Tiered fixed amount</summary>
    TieredAmount = 3,

    /// <summary>Margin-based commission</summary>
    MarginBased = 4,

    /// <summary>Custom formula</summary>
    Custom = 5
}

/// <summary>
/// FUNCTIONAL: When commission is earned/paid.
/// TECHNICAL: Controls commission calculation timing.
/// </summary>
public enum CommissionTrigger
{
    /// <summary>On opportunity close</summary>
    OnClose = 0,

    /// <summary>On order creation</summary>
    OnOrder = 1,

    /// <summary>On invoice</summary>
    OnInvoice = 2,

    /// <summary>On payment received</summary>
    OnPayment = 3,

    /// <summary>On subscription activation</summary>
    OnSubscriptionStart = 4,

    /// <summary>On contract signature</summary>
    OnSignature = 5,

    /// <summary>Pro-rated monthly</summary>
    Monthly = 6
}

/// <summary>
/// FUNCTIONAL: Commission payout status.
/// TECHNICAL: Tracks commission through payout cycle.
/// </summary>
public enum CommissionStatus
{
    /// <summary>Commission calculated, pending approval</summary>
    Pending = 0,

    /// <summary>Commission approved</summary>
    Approved = 1,

    /// <summary>Commission held (clawback risk, etc.)</summary>
    Held = 2,

    /// <summary>Commission paid out</summary>
    Paid = 3,

    /// <summary>Commission clawed back</summary>
    Clawback = 4,

    /// <summary>Commission adjusted</summary>
    Adjusted = 5,

    /// <summary>Commission cancelled</summary>
    Cancelled = 6,

    /// <summary>Commission rejected in approval</summary>
    Rejected = 7,

    /// <summary>Commission in draft state</summary>
    Draft = 8
}

/// <summary>
/// FUNCTIONAL: Commission plan status.
/// TECHNICAL: Controls plan applicability.
/// </summary>
public enum CommissionPlanStatus
{
    /// <summary>Draft - not active</summary>
    Draft = 0,

    /// <summary>Active - in use</summary>
    Active = 1,

    /// <summary>Inactive - disabled</summary>
    Inactive = 2,

    /// <summary>Archived</summary>
    Archived = 3
}

#endregion

/// <summary>
/// Commission plan defining compensation structure.
/// </summary>
public class CommissionPlan : BaseEntity
{
    #region Identification

    /// <summary>Plan name</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Plan code</summary>
    public string? Code { get; set; }

    /// <summary>Plan description</summary>
    public string? Description { get; set; }

    /// <summary>Plan status</summary>
    public CommissionPlanStatus Status { get; set; } = CommissionPlanStatus.Draft;

    #endregion

    #region Validity

    /// <summary>Effective start date</summary>
    public DateTime EffectiveStartDate { get; set; }

    /// <summary>Effective end date</summary>
    public DateTime? EffectiveEndDate { get; set; }

    /// <summary>Fiscal year</summary>
    public int? FiscalYear { get; set; }

    #endregion

    #region Commission Rules

    /// <summary>Commission type</summary>
    public CommissionType CommissionType { get; set; } = CommissionType.FlatPercentage;

    /// <summary>Base commission rate (percentage)</summary>
    public decimal BaseRate { get; set; } = 0;

    /// <summary>Primary commission rate (percentage) - alias for BaseRate</summary>
    public decimal Rate { get; set; } = 0;

    /// <summary>Whether this plan is active</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>When commission is triggered</summary>
    public CommissionTrigger Trigger { get; set; } = CommissionTrigger.OnClose;

    /// <summary>Clawback period in days</summary>
    public int? ClawbackPeriodDays { get; set; }

    /// <summary>Minimum deal size for commission</summary>
    public decimal? MinDealSize { get; set; }

    /// <summary>Commission cap per deal</summary>
    public decimal? MaxCommissionPerDeal { get; set; }

    /// <summary>Commission cap per period</summary>
    public decimal? MaxCommissionPerPeriod { get; set; }

    #endregion

    #region Splits

    /// <summary>Allow commission splits</summary>
    public bool AllowSplits { get; set; } = true;

    /// <summary>Default overlay/support split percentage</summary>
    public decimal? DefaultOverlayPercent { get; set; }

    /// <summary>Manager override percentage</summary>
    public decimal? ManagerOverridePercent { get; set; }

    #endregion

    #region Tiers (JSON)

    /// <summary>Tiered commission rates (JSON)</summary>
    public string? TierRates { get; set; }
    /*
    [
        { "minAttainment": 0, "maxAttainment": 50, "rate": 5 },
        { "minAttainment": 50, "maxAttainment": 100, "rate": 8 },
        { "minAttainment": 100, "maxAttainment": 150, "rate": 12 },
        { "minAttainment": 150, "maxAttainment": null, "rate": 15 }
    ]
    */

    #endregion

    #region Product Rules

    /// <summary>Apply to all products</summary>
    public bool AppliesToAllProducts { get; set; } = true;

    /// <summary>Product categories (comma-separated)</summary>
    public string? ProductCategories { get; set; }

    /// <summary>Product IDs (comma-separated)</summary>
    public string? ProductIds { get; set; }

    /// <summary>Product-specific rates (JSON)</summary>
    public string? ProductRates { get; set; }

    #endregion

    #region Relationships

    /// <summary>Commission tiers</summary>
    public ICollection<CommissionTier> Tiers { get; set; } = new List<CommissionTier>();

    /// <summary>Users assigned to this plan</summary>
    public ICollection<CommissionPlanAssignment> Assignments { get; set; } = new List<CommissionPlanAssignment>();

    #endregion
}

/// <summary>
/// Commission tier for tiered commission plans.
/// </summary>
public class CommissionTier : BaseEntity
{
    /// <summary>Tier name</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Tier order</summary>
    public int TierOrder { get; set; }

    /// <summary>Minimum quota attainment percent</summary>
    public decimal MinAttainmentPercent { get; set; }

    /// <summary>Maximum quota attainment percent (null = unlimited)</summary>
    public decimal? MaxAttainmentPercent { get; set; }

    /// <summary>Commission rate for this tier</summary>
    public decimal CommissionRate { get; set; }

    /// <summary>Fixed amount per deal (if fixed)</summary>
    public decimal? FixedAmount { get; set; }

    /// <summary>Accelerator multiplier</summary>
    public decimal Multiplier { get; set; } = 1;

    /// <summary>Parent plan ID</summary>
    public int CommissionPlanId { get; set; }

    /// <summary>Navigation to plan</summary>
    public CommissionPlan? CommissionPlan { get; set; }

    /// <summary>Alias for MinAttainmentPercent for service compatibility</summary>
    public decimal MinValue
    {
        get => MinAttainmentPercent;
        set => MinAttainmentPercent = value;
    }
}

/// <summary>
/// Assignment of user to commission plan.
/// </summary>
public class CommissionPlanAssignment : BaseEntity
{
    /// <summary>User ID</summary>
    public int UserId { get; set; }

    /// <summary>Navigation to user</summary>
    public User? User { get; set; }

    /// <summary>Commission plan ID</summary>
    public int CommissionPlanId { get; set; }

    /// <summary>Navigation to plan</summary>
    public CommissionPlan? CommissionPlan { get; set; }

    /// <summary>Assignment start date</summary>
    public DateTime StartDate { get; set; }

    /// <summary>Assignment end date</summary>
    public DateTime? EndDate { get; set; }

    /// <summary>Rate override</summary>
    public decimal? RateOverride { get; set; }

    /// <summary>Whether assignment is active</summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Individual commission record for a deal.
/// </summary>
public class Commission : BaseEntity
{
    #region Identification

    /// <summary>Commission number</summary>
    public string CommissionNumber { get; set; } = string.Empty;

    /// <summary>Commission status</summary>
    public CommissionStatus Status { get; set; } = CommissionStatus.Pending;

    /// <summary>Rejection reason if commission was rejected</summary>
    public string? RejectionReason { get; set; }

    #endregion

    #region Period

    /// <summary>Commission period (e.g., "2024-Q1")</summary>
    public string CommissionPeriod { get; set; } = string.Empty;

    /// <summary>Period start date</summary>
    public DateTime PeriodStartDate { get; set; }

    /// <summary>Period end date</summary>
    public DateTime PeriodEndDate { get; set; }

    #endregion

    #region Deal Details

    /// <summary>Deal/revenue amount</summary>
    public decimal DealAmount { get; set; } = 0;

    /// <summary>Commissionable amount (may differ from deal)</summary>
    public decimal CommissionableAmount { get; set; } = 0;

    /// <summary>Commission rate applied</summary>
    public decimal CommissionRate { get; set; } = 0;

    /// <summary>Calculated commission amount</summary>
    public decimal CommissionAmount { get; set; } = 0;

    /// <summary>Split percentage (if split)</summary>
    public decimal SplitPercent { get; set; } = 100;

    /// <summary>Final commission after split</summary>
    public decimal FinalCommissionAmount { get; set; } = 0;

    /// <summary>Currency code</summary>
    public string CurrencyCode { get; set; } = "USD";

    #endregion

    #region Quota Context

    /// <summary>Quota amount at time of commission</summary>
    public decimal? QuotaAmount { get; set; }

    /// <summary>Attainment at time of commission</summary>
    public decimal? AttainmentPercent { get; set; }

    /// <summary>Tier applied</summary>
    public string? TierName { get; set; }

    /// <summary>Multiplier applied</summary>
    public decimal? Multiplier { get; set; }

    #endregion

    #region Dates

    /// <summary>Date commission was earned</summary>
    public DateTime EarnedDate { get; set; } = DateTime.UtcNow;

    /// <summary>Date commission was approved</summary>
    public DateTime? ApprovedDate { get; set; }

    /// <summary>Date commission was paid</summary>
    public DateTime? PaidDate { get; set; }

    /// <summary>Clawback end date</summary>
    public DateTime? ClawbackEndDate { get; set; }

    /// <summary>Date clawed back (if applicable)</summary>
    public DateTime? ClawbackDate { get; set; }

    #endregion

    #region Adjustments

    /// <summary>Adjustment amount</summary>
    public decimal AdjustmentAmount { get; set; } = 0;

    /// <summary>Adjustment reason</summary>
    public string? AdjustmentReason { get; set; }

    /// <summary>Clawback amount</summary>
    public decimal ClawbackAmount { get; set; } = 0;

    /// <summary>Clawback reason</summary>
    public string? ClawbackReason { get; set; }

    #endregion

    #region Relationships

    /// <summary>Payee user ID</summary>
    public int UserId { get; set; }

    /// <summary>Navigation to user</summary>
    public User? User { get; set; }

    /// <summary>Commission plan ID</summary>
    public int CommissionPlanId { get; set; }

    /// <summary>Navigation to commission plan</summary>
    public CommissionPlan? CommissionPlan { get; set; }

    /// <summary>Opportunity ID</summary>
    public int? OpportunityId { get; set; }

    /// <summary>Navigation to opportunity</summary>
    public Opportunity? Opportunity { get; set; }

    /// <summary>Order ID</summary>
    public int? OrderId { get; set; }

    /// <summary>Navigation to order</summary>
    public Order? Order { get; set; }

    /// <summary>Invoice ID</summary>
    public int? InvoiceId { get; set; }

    /// <summary>Navigation to invoice</summary>
    public Invoice? Invoice { get; set; }

    /// <summary>Subscription ID</summary>
    public int? SubscriptionId { get; set; }

    /// <summary>Navigation to subscription</summary>
    public Subscription? Subscription { get; set; }

    /// <summary>Original commission (for adjustments)</summary>
    public int? OriginalCommissionId { get; set; }

    /// <summary>Navigation to original commission</summary>
    public Commission? OriginalCommission { get; set; }

    /// <summary>Approved by user ID</summary>
    public int? ApprovedById { get; set; }

    /// <summary>Navigation to approver</summary>
    public User? ApprovedBy { get; set; }

    #endregion

    #region Notes

    /// <summary>Commission notes</summary>
    public string? Notes { get; set; }

    #endregion

    #region Aliases

    /// <summary>Alias for CommissionAmount for service compatibility</summary>
    public decimal Amount
    {
        get => CommissionAmount;
        set => CommissionAmount = value;
    }

    /// <summary>Alias for CommissionPlan for service compatibility</summary>
    public CommissionPlan? Plan => CommissionPlan;

    /// <summary>Alias for ApprovedDate for service compatibility</summary>
    public DateTime? ApprovedAt
    {
        get => ApprovedDate;
        set => ApprovedDate = value;
    }

    /// <summary>Alias for PaidDate for service compatibility</summary>
    public DateTime? PaidAt
    {
        get => PaidDate;
        set => PaidDate = value;
    }

    #endregion
}

/// <summary>
/// Commission statement/payout record.
/// </summary>
public class CommissionStatement : BaseEntity
{
    /// <summary>Statement number</summary>
    public string StatementNumber { get; set; } = string.Empty;

    /// <summary>Statement period</summary>
    public string Period { get; set; } = string.Empty;

    /// <summary>Period start date</summary>
    public DateTime PeriodStartDate { get; set; }

    /// <summary>Period end date</summary>
    public DateTime PeriodEndDate { get; set; }

    /// <summary>User ID</summary>
    public int UserId { get; set; }

    /// <summary>Navigation to user</summary>
    public User? User { get; set; }

    /// <summary>Total commissions earned</summary>
    public decimal TotalEarned { get; set; } = 0;

    /// <summary>Total adjustments</summary>
    public decimal TotalAdjustments { get; set; } = 0;

    /// <summary>Total clawbacks</summary>
    public decimal TotalClawbacks { get; set; } = 0;

    /// <summary>Net payout amount</summary>
    public decimal NetPayout { get; set; } = 0;

    /// <summary>Currency code</summary>
    public string CurrencyCode { get; set; } = "USD";

    /// <summary>Whether paid</summary>
    public bool IsPaid { get; set; } = false;

    /// <summary>Paid date</summary>
    public DateTime? PaidDate { get; set; }

    /// <summary>Payment reference</summary>
    public string? PaymentReference { get; set; }

    /// <summary>Statement PDF URL</summary>
    public string? StatementUrl { get; set; }

    /// <summary>Generated date</summary>
    public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;

    #region Service Compatibility Aliases

    /// <summary>Alias for PeriodStartDate</summary>
    public DateTime PeriodStart
    {
        get => PeriodStartDate;
        set => PeriodStartDate = value;
    }

    /// <summary>Alias for PeriodEndDate</summary>
    public DateTime PeriodEnd
    {
        get => PeriodEndDate;
        set => PeriodEndDate = value;
    }

    /// <summary>Alias for TotalEarned</summary>
    public decimal TotalCommissions
    {
        get => TotalEarned;
        set => TotalEarned = value;
    }

    /// <summary>Alias for TotalClawbacks as deductions</summary>
    public decimal TotalDeductions
    {
        get => TotalClawbacks + TotalAdjustments;
        set => TotalClawbacks = value;
    }

    /// <summary>Alias for NetPayout</summary>
    public decimal NetAmount
    {
        get => NetPayout;
        set => NetPayout = value;
    }

    /// <summary>Statement status</summary>
    public CommissionStatementStatus Status { get; set; } = CommissionStatementStatus.Draft;

    /// <summary>Alias for GeneratedDate</summary>
    public DateTime GeneratedAt
    {
        get => GeneratedDate;
        set => GeneratedDate = value;
    }

    /// <summary>Date statement was finalized</summary>
    public DateTime? FinalizedAt { get; set; }

    #endregion
}

/// <summary>
/// Commission statement status
/// </summary>
public enum CommissionStatementStatus
{
    /// <summary>Draft - not finalized</summary>
    Draft = 0,

    /// <summary>Pending review</summary>
    PendingReview = 1,

    /// <summary>Finalized</summary>
    Finalized = 2,

    /// <summary>Paid</summary>
    Paid = 3
}
