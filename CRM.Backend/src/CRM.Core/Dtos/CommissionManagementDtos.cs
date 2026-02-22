// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
#pragma warning disable SA1649 // file name should match first type name
using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Dtos;

/// <summary>
/// DTO for commission response.
/// </summary>
public class CommissionDto
{
    public int Id { get; set; }
    public string CommissionNumber { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public int CommissionPlanId { get; set; }
    public string? PlanName { get; set; }
    public int? OpportunityId { get; set; }
    public int? OrderId { get; set; }
    public int? InvoiceId { get; set; }
    public int? SubscriptionId { get; set; }
    public decimal DealAmount { get; set; }
    public decimal CommissionableAmount { get; set; }
    public decimal CommissionRate { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal? SplitPercent { get; set; }
    public decimal FinalCommissionAmount { get; set; }
    public int Status { get; set; }
    public string? StatusName { get; set; }
    public string? CurrencyCode { get; set; }
    public DateTime? PaidDate { get; set; }
    public DateTime? ClawbackDate { get; set; }
    public string? ClawbackReason { get; set; }
    public string? Notes { get; set; }
    public int? ApprovedById { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating commission.
/// </summary>
public class CreateCommissionDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int UserId { get; set; }

    [Range(1, int.MaxValue)]
    public int CommissionPlanId { get; set; }

    [Range(1, int.MaxValue)]
    public int? OpportunityId { get; set; }

    [Range(1, int.MaxValue)]
    public int? OrderId { get; set; }

    [Range(1, int.MaxValue)]
    public int? InvoiceId { get; set; }

    [Range(1, int.MaxValue)]
    public int? SubscriptionId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal DealAmount { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? CommissionableAmount { get; set; }

    [Range(0, 100)]
    public decimal CommissionRate { get; set; }

    [Range(0, double.MaxValue)]
    public decimal CommissionAmount { get; set; }

    [Range(0, 100)]
    public decimal? SplitPercent { get; set; }

    [StringLength(3)]
    public string? CurrencyCode { get; set; } = "USD";

    [StringLength(500)]
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for updating commission.
/// </summary>
public class UpdateCommissionDto
{
    [Range(0, double.MaxValue)]
    public decimal? DealAmount { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? CommissionableAmount { get; set; }

    [Range(0, 100)]
    public decimal? CommissionRate { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? CommissionAmount { get; set; }

    [Range(0, 100)]
    public decimal? SplitPercent { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for commission plan.
/// </summary>
public class CommissionPlanDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public int Status { get; set; }
    public int CommissionType { get; set; }
    public int Trigger { get; set; }
    public decimal BaseRate { get; set; }
    public decimal Rate { get; set; }
    public decimal? MaxCap { get; set; }
    public decimal? MinThreshold { get; set; }
    public bool IsActive { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public DateTime? EffectiveStartDate { get; set; }
    public DateTime? EffectiveEndDate { get; set; }
    public int? FiscalYear { get; set; }
    public int? ClawbackPeriodDays { get; set; }
    public decimal? MinDealSize { get; set; }
    public decimal? MaxCommissionPerDeal { get; set; }
    public decimal? MaxCommissionPerPeriod { get; set; }
    public bool AllowSplits { get; set; }
    public decimal? DefaultOverlayPercent { get; set; }
    public decimal? ManagerOverridePercent { get; set; }
    public int TierCount { get; set; }
    public string? SplitRules { get; set; }
    public List<CommissionTierDto> Tiers { get; set; } = new();
    public int UserCount { get; set; }
    public int CommissionCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating commission plan.
/// </summary>
public class CreateCommissionPlanDto
{
    [Required(ErrorMessage = "Plan name is required")]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Code { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    [Range(0, 10)]
    public int CommissionType { get; set; }

    [Required]
    [Range(0, 10)]
    public int Trigger { get; set; }

    [Required]
    [Range(0, 100)]
    public decimal BaseRate { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MaxCap { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MinThreshold { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public DateTime? EffectiveStartDate { get; set; }
    public DateTime? EffectiveEndDate { get; set; }
    public int? FiscalYear { get; set; }
    public int? ClawbackPeriodDays { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MinDealSize { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MaxCommissionPerDeal { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MaxCommissionPerPeriod { get; set; }

    public bool? AllowSplits { get; set; }

    [Range(0, 100)]
    public decimal? DefaultOverlayPercent { get; set; }

    [StringLength(2000)]
    public string? SplitRules { get; set; }
}

/// <summary>
/// DTO for updating commission plan.
/// </summary>
public class UpdateCommissionPlanDto
{
    [StringLength(255)]
    public string? Name { get; set; }

    [StringLength(50)]
    public string? Code { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    [Range(0, 10)]
    public int? CommissionType { get; set; }

    [Range(0, 10)]
    public int? Trigger { get; set; }

    [Range(0, 100)]
    public decimal? BaseRate { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MaxCap { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MinThreshold { get; set; }

    public int? Status { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public DateTime? EffectiveStartDate { get; set; }
    public DateTime? EffectiveEndDate { get; set; }
    public int? FiscalYear { get; set; }
    public int? ClawbackPeriodDays { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MinDealSize { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MaxCommissionPerDeal { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MaxCommissionPerPeriod { get; set; }

    public bool? AllowSplits { get; set; }

    [Range(0, 100)]
    public decimal? DefaultOverlayPercent { get; set; }

    [StringLength(2000)]
    public string? SplitRules { get; set; }
}

/// <summary>
/// DTO for commission tier.
/// </summary>
public class CommissionTierDto
{
    public int Id { get; set; }
    public int PlanId { get; set; }
    public int CommissionPlanId { get; set; }
    public int TierLevel { get; set; }
    public int Sequence { get; set; }
    public string? TierName { get; set; }
    public decimal MinValue { get; set; }
    public decimal MinimumAmount { get; set; }
    public decimal MaxValue { get; set; }
    public decimal MaximumAmount { get; set; }
    public decimal Rate { get; set; }
    public decimal CommissionRate { get; set; }
    public decimal? Accelerator { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for creating commission tier.
/// </summary>
public class CreateCommissionTierDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int TierLevel { get; set; }

    [StringLength(100)]
    public string? TierName { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal MinValue { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal MaxValue { get; set; }

    [Required]
    [Range(0, 100)]
    public decimal Rate { get; set; }

    [Range(0, 100)]
    public decimal? Accelerator { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal MinimumAmount { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal MaximumAmount { get; set; }

    [Required]
    [Range(0, 100)]
    public decimal CommissionRate { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int Sequence { get; set; }
}

/// <summary>
/// DTO for updating commission tier.
/// </summary>
public class UpdateCommissionTierDto
{
    [Range(1, int.MaxValue)]
    public int? TierLevel { get; set; }

    [StringLength(100)]
    public string? TierName { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MinValue { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MaxValue { get; set; }

    [Range(0, 100)]
    public decimal? Rate { get; set; }

    [Range(0, 100)]
    public decimal? Accelerator { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MinimumAmount { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MaximumAmount { get; set; }

    [Range(0, 100)]
    public decimal? CommissionRate { get; set; }

    [Range(0, int.MaxValue)]
    public int? Sequence { get; set; }
}

/// <summary>
/// DTO for commission statement.
/// </summary>
public class CommissionStatementDto
{
    public int Id { get; set; }
    public string StatementNumber { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public DateTime PeriodStartDate { get; set; }
    public DateTime PeriodEndDate { get; set; }
    public int CommissionCount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal ApprovedAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public int Status { get; set; }
    public DateTime? FinalizedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for generating commission statement.
/// </summary>
public class GenerateCommissionStatementDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int UserId { get; set; }

    [Required]
    public DateTime PeriodStartDate { get; set; }

    [Required]
    public DateTime PeriodEndDate { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for commission approval request.
/// </summary>
public class ApproveCommissionDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int ApprovedById { get; set; }

    [StringLength(500)]
    public string? ApprovalNotes { get; set; }
}

/// <summary>
/// DTO for commission rejection request.
/// </summary>
public class RejectCommissionDto
{
    [Required(ErrorMessage = "Reason is required")]
    [StringLength(500)]
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// DTO for commission payout request.
/// </summary>
public class PayoutCommissionDto
{
    public DateTime? PaidDate { get; set; }

    [StringLength(100)]
    public string? PaymentReference { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for commission clawback request.
/// </summary>
public class ClawbackCommissionDto
{
    [Required(ErrorMessage = "Reason is required")]
    [StringLength(500)]
    public string Reason { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal? ClawbackAmount { get; set; }

    [StringLength(100)]
    public string? ReferenceNumber { get; set; }
}

/// <summary>
/// DTO for commission leaderboard.
/// </summary>
public class CommissionLeaderboardDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public decimal TotalCommission { get; set; }
    public int CommissionCount { get; set; }
    public decimal AverageCommission { get; set; }
    public int Rank { get; set; }
    public string? Department { get; set; }
}

/// <summary>
/// DTO for commission forecast.
/// </summary>
public class CommissionForecastDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public decimal ForecastedCommission { get; set; }
    public decimal CurrentCommission { get; set; }
    public decimal ProjectedTotal { get; set; }
    public int PipelineOpportunities { get; set; }
    public decimal PipelineValue { get; set; }
    public double WinRate { get; set; }
    public DateTime ForecastAsOfDate { get; set; }
}

/// <summary>
/// DTO for commission statistics.
/// </summary>
public class CommissionStatisticsDto
{
    public int TotalCommissions { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal ApprovedAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public int PendingCount { get; set; }
    public int ApprovedCount { get; set; }
    public int PaidCount { get; set; }
    public int ClawedBackCount { get; set; }
    public decimal AverageCommission { get; set; }
    public decimal MaxCommission { get; set; }
    public decimal MinCommission { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

/// <summary>
/// DTO for commission calculation result.
/// </summary>
public class CommissionCalculationResultDto
{
    public int UserId { get; set; }
    public int? OpportunityId { get; set; }
    public int? OrderId { get; set; }
    public int PlanId { get; set; }
    public int CommissionPlanId { get; set; }
    public string? PlanName { get; set; }
    public decimal DealAmount { get; set; }
    public decimal Amount { get; set; }
    public decimal CommissionRate { get; set; }
    public decimal BaseAmount { get; set; }
    public decimal BaseCommissionAmount { get; set; }
    public decimal? Accelerator { get; set; }
    public decimal FinalAmount { get; set; }
    public decimal FinalCommissionAmount { get; set; }
    public int? TierLevel { get; set; }
    public string? TierName { get; set; }
    public List<CommissionBreakdownDto> Breakdown { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public decimal BaseCommissionRate { get; set; }
    public decimal? TierCommissionAmount { get; set; }
    public decimal? TierCommissionRate { get; set; }
}

/// <summary>
/// DTO for commission breakdown.
/// </summary>
public class CommissionBreakdownDto
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Rate { get; set; }
    public decimal Result { get; set; }
}

/// <summary>
/// DTO for paginated commission list.
/// </summary>
public class CommissionListDto
{
    public List<CommissionDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
