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
/// DTO for commission payout details
/// Represents a single payout event to a sales person
/// </summary>
public class CommissionPayoutDto
{
    /// <summary>Unique identifier for the payout record</summary>
    public int Id { get; set; }

    /// <summary>Payout number/reference</summary>
    public string PayoutNumber { get; set; } = string.Empty;

    /// <summary>Sales user receiving the payout</summary>
    public int UserId { get; set; }

    /// <summary>Sales user name</summary>
    public string? UserName { get; set; }

    /// <summary>Commission plan ID (if plan-based payout)</summary>
    public int? CommissionPlanId { get; set; }

    /// <summary>Total commission amount for payout (decimal[18,4])</summary>
    [Range(0, 999999999.99999)]
    public decimal TotalCommissionAmount { get; set; }

    /// <summary>Number of commissions included in this payout</summary>
    [Range(0, int.MaxValue)]
    public int CommissionCount { get; set; }

    /// <summary>Payout period start date</summary>
    public DateTime? PeriodStartDate { get; set; }

    /// <summary>Payout period end date</summary>
    public DateTime? PeriodEndDate { get; set; }

    /// <summary>Scheduled payout date</summary>
    public DateTime? ScheduledPayoutDate { get; set; }

    /// <summary>Actual payout date (null if not yet paid)</summary>
    public DateTime? ActualPayoutDate { get; set; }

    /// <summary>Payout method (Bank Transfer, Check, PayPal, etc.)</summary>
    public string? PayoutMethod { get; set; }

    /// <summary>Payout status (Pending, Scheduled, Processing, Completed, Failed, Cancelled)</summary>
    public string Status { get; set; } = "Pending";

    /// <summary>Deductions from payout (taxes, fees, etc.)</summary>
    [Range(0, 999999999.99999)]
    public decimal TotalDeductions { get; set; }

    /// <summary>Net amount after deductions</summary>
    [Range(0, 999999999.99999)]
    public decimal NetPayoutAmount { get; set; }

    /// <summary>Payment reference or transaction ID</summary>
    public string? PaymentReferenceId { get; set; }

    /// <summary>Notes about the payout</summary>
    [StringLength(1000)]
    public string? Notes { get; set; }

    /// <summary>Approved by user ID</summary>
    public int? ApprovedById { get; set; }

    /// <summary>Approved by user name</summary>
    public string? ApprovedByName { get; set; }

    /// <summary>Approval timestamp</summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>Record creation timestamp</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Record last update timestamp</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>Detailed breakdown of commissions in this payout</summary>
    public List<CommissionPayoutDetailDto> Details { get; set; } = new();
}

/// <summary>
/// Detail record for individual commissions in a payout
/// </summary>
public class CommissionPayoutDetailDto
{
    /// <summary>Commission ID</summary>
    public int CommissionId { get; set; }

    /// <summary>Commission number</summary>
    public string CommissionNumber { get; set; } = string.Empty;

    /// <summary>Commission amount</summary>
    public decimal CommissionAmount { get; set; }

    /// <summary>Related opportunity/deal ID</summary>
    public int? OpportunityId { get; set; }

    /// <summary>Related opportunity name</summary>
    public string? OpportunityName { get; set; }

    /// <summary>Invoice raised for this commission</summary>
    public int? InvoiceId { get; set; }
}

/// <summary>
/// DTO for creating a commission payout
/// </summary>
public class CreateCommissionPayoutDto
{
    /// <summary>Sales user ID</summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int UserId { get; set; }

    /// <summary>Commission plan ID (optional)</summary>
    [Range(1, int.MaxValue)]
    public int? CommissionPlanId { get; set; }

    /// <summary>Commission IDs to include in payout</summary>
    [Required]
    public List<int> CommissionIds { get; set; } = new();

    /// <summary>Payout period start date</summary>
    public DateTime? PeriodStartDate { get; set; }

    /// <summary>Payout period end date</summary>
    public DateTime? PeriodEndDate { get; set; }

    /// <summary>Scheduled payout date</summary>
    public DateTime? ScheduledPayoutDate { get; set; }

    /// <summary>Payout method</summary>
    [StringLength(50)]
    public string? PayoutMethod { get; set; }

    /// <summary>Notes</summary>
    [StringLength(1000)]
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for updating a commission payout
/// </summary>
public class UpdateCommissionPayoutDto
{
    /// <summary>Scheduled payout date</summary>
    public DateTime? ScheduledPayoutDate { get; set; }

    /// <summary>Payout method</summary>
    [StringLength(50)]
    public string? PayoutMethod { get; set; }

    /// <summary>Status update</summary>
    [StringLength(50)]
    public string? Status { get; set; }

    /// <summary>Notes</summary>
    [StringLength(1000)]
    public string? Notes { get; set; }
}

/// <summary>
/// List DTO for commission payouts (paginated)
/// </summary>
public class CommissionPayoutListDto
{
    /// <summary>Record ID</summary>
    public int Id { get; set; }

    /// <summary>Payout number</summary>
    public string PayoutNumber { get; set; } = string.Empty;

    /// <summary>Sales person name</summary>
    public string? UserName { get; set; }

    /// <summary>Total commission amount</summary>
    public decimal TotalCommissionAmount { get; set; }

    /// <summary>Net payout amount</summary>
    public decimal NetPayoutAmount { get; set; }

    /// <summary>Payout status</summary>
    public string Status { get; set; } = "Pending";

    /// <summary>Scheduled date</summary>
    public DateTime? ScheduledPayoutDate { get; set; }

    /// <summary>Actual payout date</summary>
    public DateTime? ActualPayoutDate { get; set; }

    /// <summary>Number of commissions</summary>
    public int CommissionCount { get; set; }

    /// <summary>Creation date</summary>
    public DateTime CreatedAt { get; set; }
}
