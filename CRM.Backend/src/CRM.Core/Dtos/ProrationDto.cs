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

using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Dtos;

/// <summary>
/// DTO for subscription proration calculations
/// Handles billing adjustments when subscriptions change mid-billing cycle
/// (e.g., upgrade/downgrade, plan changes, add-ons, cancellations)
/// All monetary values are decimal[18,4] for precision
/// </summary>
public class ProrationDto
{
    /// <summary>Unique identifier for proration record</summary>
    public int Id { get; set; }

    /// <summary>Subscription ID being prorated</summary>
    public int SubscriptionId { get; set; }

    /// <summary>Subscription number for display</summary>
    public string? SubscriptionNumber { get; set; }

    /// <summary>Associated account ID</summary>
    public int? AccountId { get; set; }

    /// <summary>Account name for display</summary>
    public string? AccountName { get; set; }

    /// <summary>Type of change triggering proration (Upgrade, Downgrade, AddOn, Cancellation, PlanChange)</summary>
    public string ChangeType { get; set; } = "PlanChange";

    /// <summary>Current billing period start date</summary>
    public DateTime BillingPeriodStartDate { get; set; }

    /// <summary>Current billing period end date</summary>
    public DateTime BillingPeriodEndDate { get; set; }

    /// <summary>Date of the change/effective date</summary>
    public DateTime EffectiveDate { get; set; }

    /// <summary>Current plan/period charge (decimal[18,4])</summary>
    [Range(0, 999999999.99999)]
    public decimal CurrentChargeAmount { get; set; }

    /// <summary>New plan/period charge (decimal[18,4])</summary>
    [Range(0, 999999999.99999)]
    public decimal NewChargeAmount { get; set; }

    /// <summary>Days in current billing period</summary>
    [Range(1, int.MaxValue)]
    public int DaysInBillingPeriod { get; set; }

    /// <summary>Remaining days in billing period after effective date</summary>
    [Range(0, int.MaxValue)]
    public int RemainingDays { get; set; }

    /// <summary>Days in period before change</summary>
    [Range(0, int.MaxValue)]
    public int UsedDays { get; set; }

    /// <summary>Daily rate of current charge</summary>
    [Range(0, 999999999.99999)]
    public decimal DailyRateCurrentCharge { get; set; }

    /// <summary>Daily rate of new charge</summary>
    [Range(0, 999999999.99999)]
    public decimal DailyRateNewCharge { get; set; }

    /// <summary>Credit amount for used days of old plan (decimal[18,4])</summary>
    [Range(-999999999.99999, 999999999.99999)]
    public decimal CreditAmount { get; set; }

    /// <summary>Debit amount for remaining days of new plan (decimal[18,4])</summary>
    [Range(-999999999.99999, 999999999.99999)]
    public decimal DebitAmount { get; set; }

    /// <summary>Net proration adjustment (Credit - Debit)</summary>
    [Range(-999999999.99999, 999999999.99999)]
    public decimal NetProrationAmount { get; set; }

    /// <summary>Proration method used (DayBased, ImmediateCharge, NextBillingCycle)</summary>
    public string ProrationType { get; set; } = "DayBased";

    /// <summary>Invoice applied to (if generated)</summary>
    public int? InvoiceId { get; set; }

    /// <summary>Invoice number</summary>
    public string? InvoiceNumber { get; set; }

    /// <summary>Proration status (Calculated, Applied, Voided, Reversed)</summary>
    public string Status { get; set; } = "Calculated";

    /// <summary>Notes about the proration</summary>
    [StringLength(1000)]
    public string? Notes { get; set; }

    /// <summary>Record creation timestamp</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Record last update timestamp</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>Breakdown details of the calculation</summary>
    public List<ProrationLineItemDto> LineItems { get; set; } = new();
}

/// <summary>
/// DTO for individual proration line items
/// Breaks down composite prorations (e.g., multiple add-on changes)
/// </summary>
public class ProrationLineItemDto
{
    /// <summary>Line item description</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Line type (Credit, Debit)</summary>
    public string LineType { get; set; } = "Debit";

    /// <summary>Original charge amount</summary>
    [Range(0, 999999999.99999)]
    public decimal OriginalAmount { get; set; }

    /// <summary>Prorated amount</summary>
    [Range(-999999999.99999, 999999999.99999)]
    public decimal ProratedAmount { get; set; }

    /// <summary>Quantity (for add-ons)</summary>
    [Range(0, 999999)]
    public decimal? Quantity { get; set; }

    /// <summary>Unit price (for add-ons)</summary>
    [Range(0, 999999999.99999)]
    public decimal? UnitPrice { get; set; }

    /// <summary>Days affected</summary>
    [Range(0, int.MaxValue)]
    public int DaysAffected { get; set; }

    /// <summary>Calculation details</summary>
    [StringLength(500)]
    public string? CalculationDetails { get; set; }
}

/// <summary>
/// DTO for calculating proration (used in requests)
/// </summary>
public class CreateProrationDto
{
    /// <summary>Subscription ID</summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int SubscriptionId { get; set; }

    /// <summary>Type of change</summary>
    [Required]
    [StringLength(50)]
    public string ChangeType { get; set; } = string.Empty;

    /// <summary>Effective date of change</summary>
    [Required]
    public DateTime EffectiveDate { get; set; }

    /// <summary>New charge amount</summary>
    [Required]
    [Range(0, 999999999.99999)]
    public decimal NewChargeAmount { get; set; }

    /// <summary>Proration type</summary>
    [StringLength(50)]
    public string ProrationType { get; set; } = "DayBased";

    /// <summary>Notes</summary>
    [StringLength(1000)]
    public string? Notes { get; set; }

    /// <summary>Additional line items to include in proration</summary>
    public List<ProrationLineItemDto> AdditionalLineItems { get; set; } = new();
}

/// <summary>
/// DTO for updating proration record
/// </summary>
public class UpdateProrationDto
{
    /// <summary>New status</summary>
    [StringLength(50)]
    public string? Status { get; set; }

    /// <summary>Associated invoice ID</summary>
    [Range(1, int.MaxValue)]
    public int? InvoiceId { get; set; }

    /// <summary>Notes</summary>
    [StringLength(1000)]
    public string? Notes { get; set; }
}

/// <summary>
/// List DTO for prorations (paginated)
/// </summary>
public class ProrationListDto
{
    /// <summary>Record ID</summary>
    public int Id { get; set; }

    /// <summary>Subscription number</summary>
    public string? SubscriptionNumber { get; set; }

    /// <summary>Account name</summary>
    public string? AccountName { get; set; }

    /// <summary>Change type</summary>
    public string ChangeType { get; set; } = "PlanChange";

    /// <summary>Effective date</summary>
    public DateTime EffectiveDate { get; set; }

    /// <summary>Net proration amount</summary>
    public decimal NetProrationAmount { get; set; }

    /// <summary>Status</summary>
    public string Status { get; set; } = "Calculated";

    /// <summary>Creation date</summary>
    public DateTime CreatedAt { get; set; }
}
