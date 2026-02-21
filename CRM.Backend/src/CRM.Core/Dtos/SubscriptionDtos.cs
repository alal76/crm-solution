// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Dtos;

/// <summary>
/// DTO for creating a subscription.
/// </summary>
public class CreateSubscriptionDto
{
    public int AccountId { get; set; }
    public int? ProductId { get; set; }
    public decimal Amount { get; set; }
    public string BillingCycle { get; set; } = "Monthly";
    public DateTime BillingStartDate { get; set; }
    public DateTime? TrialEndDate { get; set; }
    public bool IsAutoRenewal { get; set; } = true;
    public string? ProrationType { get; set; } = "ProRata";
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for updating a subscription.
/// </summary>
public class UpdateSubscriptionDto
{
    public decimal? Amount { get; set; }
    public string? BillingCycle { get; set; }
    public bool? IsAutoRenewal { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for subscription response.
/// </summary>
public class SubscriptionDto
{
    public int Id { get; set; }
    public string SubscriptionNumber { get; set; } = string.Empty;
    public int AccountId { get; set; }
    public int? ProductId { get; set; }
    public decimal Amount { get; set; }
    public string BillingCycle { get; set; } = string.Empty;
    public DateTime BillingStartDate { get; set; }
    public DateTime? BillingEndDate { get; set; }
    public DateTime? NextBillingDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal MRR { get; set; }
    public decimal ARR { get; set; }
    public bool IsAutoRenewal { get; set; }
    public string? ProrationType { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    public List<SubscriptionItemDto> Items { get; set; } = new();
}

/// <summary>
/// DTO for subscription items (add-ons, line items).
/// </summary>
public class SubscriptionItemDto
{
    public int Id { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }
    public bool IsAddon { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

/// <summary>
/// DTO for plan change request (upgrade/downgrade).
/// </summary>
public class PlanChangeDto
{
    public int NewProductId { get; set; }
    public decimal NewAmount { get; set; }
    public string ChangeType { get; set; } = "Immediate"; // Immediate or EndOfPeriod
    public string? ProrationType { get; set; } = "ProRata";
    public string? Reason { get; set; }
}

/// <summary>
/// DTO for recording usage.
/// </summary>
public class RecordUsageDto
{
    public string MetricName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string? Unit { get; set; }
    public DateTime? UsageDate { get; set; }
}

/// <summary>
/// DTO for subscription usage metrics.
/// </summary>
public class SubscriptionUsageDto
{
    public string MetricName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string? Unit { get; set; }
    public decimal OverageAmount { get; set; }
    public bool Invoiced { get; set; }
    public DateTime DateRecorded { get; set; }
}

/// <summary>
/// DTO for billing history record.
/// </summary>
public class BillingHistoryDto
{
    public int Id { get; set; }
    public int SubscriptionId { get; set; }
    public int? InvoiceId { get; set; }
    public DateTime CycleStartDate { get; set; }
    public DateTime CycleEndDate { get; set; }
    public decimal Amount { get; set; }
    public decimal? ProratedAmount { get; set; }
    public decimal? UsageCharges { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? TaxAmount { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? EventDetails { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? BilledDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public DateTime EventDate { get; set; }
}

/// <summary>
/// DTO for dunning record.
/// </summary>
public class DunningRecordDto
{
    public int Id { get; set; }
    public int SubscriptionId { get; set; }
    public int InvoiceId { get; set; }
    public int RetryAttempt { get; set; }
    public DateTime NextRetryDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime InitialFailureDate { get; set; }
    public bool IsExhausted { get; set; }
    public decimal OutstandingAmount { get; set; }
    public decimal? RecoveredAmount { get; set; }
}

/// <summary>
/// DTO for company-wide subscription analytics.
/// </summary>
public class SubscriptionAnalyticsDto
{
    public int TotalSubscriptions { get; set; }
    public int ActiveSubscriptions { get; set; }
    public int PausedSubscriptions { get; set; }
    public int CancelledSubscriptions { get; set; }
    public decimal TotalMRR { get; set; }
    public decimal TotalARR { get; set; }
    public decimal ChurnRate { get; set; } // Percentage
    public decimal NetRevenueRetention { get; set; } // Percentage
    public decimal AverageContractValue { get; set; }
    public decimal CustomerLifetimeValue { get; set; }
    public DateTime CalculatedAt { get; set; }
}

/// <summary>
/// DTO for proration calculation results.
/// </summary>
public class ProrateResultDto
{
    public int SubscriptionId { get; set; }
    public string ProrationType { get; set; } = string.Empty;
    public decimal OriginalAmount { get; set; }
    public decimal DaysInCycle { get; set; }
    public decimal DaysUsed { get; set; }
    public decimal ProratedAmount { get; set; }
    public decimal CreditOrCharge { get; set; } // Positive = charge, Negative = credit
    public DateTime EffectiveDate { get; set; }
    public string? CalculationDetails { get; set; }
}

/// <summary>
/// DTO for billing result from recurring billing engine.
/// </summary>
public class BillingResultDto
{
    public int SubscriptionId { get; set; }
    public bool Success { get; set; }
    public int? InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime ProcessedAt { get; set; }
}

/// <summary>
/// DTO for subscription filter/search parameters.
/// </summary>
public class SubscriptionFilterDto
{
    public int? AccountId { get; set; }
    public string? Status { get; set; }
    public string? BillingCycle { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; } = "CreatedAt";
    public string? SortOrder { get; set; } = "desc";
}

// Note: PagedResultDto<T> is already defined in InvoiceDto.cs
// No need to duplicate - use that shared definition
