// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Entities;

namespace CRM.Core.Dtos;

/// <summary>
/// Payment data transfer object for read operations
/// </summary>
public class PaymentDto
{
    public int Id { get; set; }
    public string PaymentNumber { get; set; } = string.Empty;
    public int? InvoiceId { get; set; }
    public string? InvoiceNumber { get; set; }
    public int AccountId { get; set; }
    public string? AccountName { get; set; }
    
    public decimal Amount { get; set; }
    public decimal RefundedAmount { get; set; }
    public decimal AmountApplied { get; set; }
    
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentType PaymentType { get; set; }
    public Entities.PaymentStatus Status { get; set; }
    
    public DateTime PaymentDate { get; set; }
    public DateTime? ProcessedDate { get; set; }
    public DateTime? RefundDate { get; set; }
    public DateTime? ScheduledDate { get; set; }
    
    public string? TransactionId { get; set; }
    public string? AuthorizationCode { get; set; }
    public string? CardLast4 { get; set; }
    public string? CardholderName { get; set; }
    public string? BankReference { get; set; }
    
    public bool IsReconciled { get; set; }
    public DateTime? ReconciledDate { get; set; }
    
    public string? Description { get; set; }
    public string? FailureReason { get; set; }
    public int RetryCount { get; set; }
    
    public int? OriginalPaymentId { get; set; }
    public bool IsRefund => PaymentType == PaymentType.Refund;
}

/// <summary>
/// Create payment DTO
/// </summary>
public class CreatePaymentDto
{
    public int? InvoiceId { get; set; }
    public int AccountId { get; set; }
    
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CreditCard;
    public PaymentType PaymentType { get; set; } = PaymentType.Payment;
    public Entities.PaymentStatus Status { get; set; } = Entities.PaymentStatus.Pending;
    
    public DateTime? ScheduledDate { get; set; }
    
    public string? Description { get; set; }
    
    // For card payments - use tokenized card ID, never raw card data
    public string? TokenizedCardId { get; set; }
}

/// <summary>
/// Update payment DTO
/// </summary>
public class UpdatePaymentDto
{
    public PaymentStatus? Status { get; set; }
    public string? Description { get; set; }
    public DateTime? PaymentDate { get; set; }
}

/// <summary>
/// Payment filter DTO
/// </summary>
public class PaymentFilterDto
{
    public int? AccountId { get; set; }
    public int? InvoiceId { get; set; }
    public PaymentStatus? Status { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; } = "PaymentDate";
    public string? SortOrder { get; set; } = "desc";
}

/// <summary>
/// Process payment request DTO
/// </summary>
public class ProcessPaymentRequestDto
{
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CreditCard;
    
    /// <summary>
    /// Tokenized card ID - never raw card data
    /// </summary>
    public string? TokenizedCardId { get; set; }
    
    public string? AuthorizationCode { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// Refund payment request DTO
/// </summary>
public class RefundPaymentRequestDto
{
    public decimal? RefundAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Payment statistics DTO
/// </summary>
public class PaymentStatistics
{
    public int TotalPayments { get; set; }
    public int SuccessfulPayments { get; set; }
    public int FailedPayments { get; set; }
    public int PendingPayments { get; set; }
    
    public decimal TotalAmount { get; set; }
    public decimal SuccessfulAmount { get; set; }
    public decimal RefundedAmount { get; set; }
    
    public double SuccessRate { get; set; }
    public double AveragePaymentAmount { get; set; }
    
    public Dictionary<PaymentMethod, int> PaymentsByMethod { get; set; } = new();
}

/// <summary>
/// Payment allocation DTO for applying payments to multiple invoices
/// </summary>
public class PaymentAllocation
{
    public int InvoiceId { get; set; }
    public decimal Amount { get; set; }
}
