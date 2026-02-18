// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Dtos;

/// <summary>
/// DTO for commission calculation details
/// Contains the calculation breakdown for a specific commission
/// </summary>
public class CommissionCalculationDto
{
    /// <summary>Unique identifier for the calculation record</summary>
    public int Id { get; set; }

    /// <summary>Commission rule ID that was applied</summary>
    [Required]
    public int RuleId { get; set; }

    /// <summary>Commission rule name for display</summary>
    public string? RuleName { get; set; }

    /// <summary>Base deal/opportunity amount (decimal[18,4])</summary>
    [Range(0, 999999999.99999, ErrorMessage = "Deal amount must be between 0 and 999999999.9999")]
    public decimal DealAmount { get; set; }

    /// <summary>Calculated commission amount (decimal[18,4])</summary>
    [Range(0, 999999999.99999, ErrorMessage = "Commission must be between 0 and 999999999.9999")]
    public decimal Commission { get; set; }

    /// <summary>Commission tier applied (Bronze, Silver, Gold, Platinum, etc.)</summary>
    public string? Tier { get; set; }

    /// <summary>Commission rate percentage applied (e.g., 5.0 for 5%)</summary>
    [Range(0, 100, ErrorMessage = "Commission rate must be between 0 and 100")]
    public decimal CommissionRate { get; set; }

    /// <summary>Cap limit applied (if any)</summary>
    [Range(0, 999999999.99999)]
    public decimal? AppliedCap { get; set; }

    /// <summary>Claw-back amount (if applicable)</summary>
    [Range(0, 999999999.99999)]
    public decimal? ClawbackAmount { get; set; }

    /// <summary>Net commission after adjustments</summary>
    [Range(0, 999999999.99999)]
    public decimal NetCommission { get; set; }

    /// <summary>Sales user receiving commission</summary>
    public int? UserId { get; set; }

    /// <summary>Sales user name</summary>
    public string? UserName { get; set; }

    /// <summary>Related opportunity ID</summary>
    public int? OpportunityId { get; set; }

    /// <summary>Related order ID</summary>
    public int? OrderId { get; set; }

    /// <summary>Related invoice ID</summary>
    public int? InvoiceId { get; set; }

    /// <summary>Calculation status (Pending, Approved, Paid, Disputed, Cancelled)</summary>
    public string Status { get; set; } = "Pending";

    /// <summary>Calculation notes or adjustment reasons</summary>
    [StringLength(1000)]
    public string? Notes { get; set; }

    /// <summary>When the calculation was performed</summary>
    public DateTime CalculatedAt { get; set; }

    /// <summary>When the record was created</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>When the record was last updated</summary>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating a commission calculation
/// </summary>
public class CreateCommissionCalculationDto
{
    /// <summary>Commission rule ID to apply</summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int RuleId { get; set; }

    /// <summary>Base deal amount</summary>
    [Required]
    [Range(0, 999999999.99999)]
    public decimal DealAmount { get; set; }

    /// <summary>Sales user receiving commission</summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int UserId { get; set; }

    /// <summary>Related opportunity ID</summary>
    [Range(1, int.MaxValue)]
    public int? OpportunityId { get; set; }

    /// <summary>Related order ID</summary>
    [Range(1, int.MaxValue)]
    public int? OrderId { get; set; }

    /// <summary>Related invoice ID</summary>
    [Range(1, int.MaxValue)]
    public int? InvoiceId { get; set; }

    /// <summary>Optional notes</summary>
    [StringLength(1000)]
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for updating a commission calculation
/// </summary>
public class UpdateCommissionCalculationDto
{
    /// <summary>Adjusted deal amount</summary>
    [Range(0, 999999999.99999)]
    public decimal? DealAmount { get; set; }

    /// <summary>Manual adjustment amount</summary>
    [Range(-999999999.99999, 999999999.99999)]
    public decimal? AdjustmentAmount { get; set; }

    /// <summary>New status</summary>
    [StringLength(50)]
    public string? Status { get; set; }

    /// <summary>Adjustment notes</summary>
    [StringLength(1000)]
    public string? Notes { get; set; }
}

/// <summary>
/// List DTO for commission calculations (paginated)
/// </summary>
public class CommissionCalculationListDto
{
    /// <summary>Record ID</summary>
    public int Id { get; set; }

    /// <summary>Rule name</summary>
    public string? RuleName { get; set; }

    /// <summary>Deal amount</summary>
    public decimal DealAmount { get; set; }

    /// <summary>Calculated commission</summary>
    public decimal Commission { get; set; }

    /// <summary>Sales user name</summary>
    public string? UserName { get; set; }

    /// <summary>Tier applied</summary>
    public string? Tier { get; set; }

    /// <summary>Status</summary>
    public string Status { get; set; } = "Pending";

    /// <summary>Creation date</summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for commission calculation on deals.
/// </summary>
public class CommissionDealCalculationDto
{
    public int OpportunityId { get; set; }
    public string? DealName { get; set; }
    public decimal DealAmount { get; set; }
    public decimal Commission { get; set; }
    public string CommissionTier { get; set; } = string.Empty;
    public decimal CommissionRate { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime CalculatedAt { get; set; }
}

/// <summary>
/// DTO for commission calculation on orders.
/// </summary>
public class CommissionOrderCalculationDto
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public decimal OrderAmount { get; set; }
    public decimal Commission { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for commission calculation for a period.
/// </summary>
public class CommissionPeriodCalculationDto
{
    [Required]
    public int UserId { get; set; }
    
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
}

/// <summary>
/// Result of period commission calculation.
/// </summary>
public class CommissionPeriodCalculationResultDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalDealAmount { get; set; }
    public decimal TotalCommission { get; set; }
    public int DealCount { get; set; }
}

/// <summary>
/// DTO for validating commission calculations.
/// </summary>
public class CommissionCalculationValidationDto
{
    [Required]
    public int RuleId { get; set; }
    
    [Required]
    [Range(0, 999999999.99999)]
    public decimal DealAmount { get; set; }
    
    [Required]
    public int UserId { get; set; }
}

/// <summary>
/// Result of commission validation.
/// </summary>
public class CommissionValidationResultDto
{
    public bool IsValid { get; set; }
    public decimal CalculatedCommission { get; set; }
    public string? ValidationMessage { get; set; }
    public List<string> ValidationErrors { get; set; } = new();
}

/// <summary>
/// DTO for commission clawback.
/// </summary>
public class CommissionClawbackDto
{
    public int CommissionCalculationId { get; set; }
    public decimal ClawbackAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int CreatedById { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for commission reconciliation.
/// </summary>
public class CommissionReconciliationDto
{
    public int Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalRecords { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ReconciliationDate { get; set; }
    public string? Notes { get; set; }
}

