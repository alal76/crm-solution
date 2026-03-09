// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using System.ComponentModel.DataAnnotations;

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
    public decimal AmountUnapplied { get; set; }

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

    // Identity additions
    public string? ExternalPaymentId { get; set; }
    public string? GatewayReference { get; set; }
    public string? CheckNumber { get; set; }

    // Amounts
    public decimal? ProcessingFee { get; set; }
    public decimal? NetAmount { get; set; }
    public decimal? ExchangeRate { get; set; }

    // Dates
    public DateTime? SettledDate { get; set; }
    public DateTime? DepositDate { get; set; }

    // Card details (masked - last 4 only for security)
    public string? CardBrand { get; set; }
    public int? CardExpMonth { get; set; }
    public int? CardExpYear { get; set; }

    // Bank details (masked)
    public string? BankName { get; set; }
    public string? AccountLast4 { get; set; }
    public string? AccountType { get; set; }
    public string? RoutingNumberLast4 { get; set; }

    // Gateway
    public string? Gateway { get; set; }
    public string? GatewayResponseCode { get; set; }
    public string? GatewayResponseMessage { get; set; }
    public string? AvsResponseCode { get; set; }
    public string? CvvResponseCode { get; set; }
    public decimal? RiskScore { get; set; }

    // Fraud & Risk
    public bool FraudFlagged { get; set; }

    // Relationships
    public int? OrderId { get; set; }
    public int? SubscriptionId { get; set; }
    public int? ProcessedById { get; set; }

    // Notes
    public string? InternalNotes { get; set; }
    public string? RefundReason { get; set; }
}

/// <summary>
/// Create payment DTO
/// </summary>
public class CreatePaymentDto
{
    public int? InvoiceId { get; set; }
    
    // AP-025: AccountId is required and must be a positive integer
    [Required(ErrorMessage = "Account ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Account ID must be greater than 0")]
    public int AccountId { get; set; }

    // AP-025: Amount is required and must be a positive monetary value
    [Required(ErrorMessage = "Amount is required")]
    [Range(typeof(decimal), "0.01", "999999999.99", ErrorMessage = "Amount must be between 0.01 and 999999999.99")]
    public decimal Amount { get; set; }
    
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CreditCard;
    public PaymentType PaymentType { get; set; } = PaymentType.Payment;
    public Entities.PaymentStatus Status { get; set; } = Entities.PaymentStatus.Pending;

    // AP-025: Scheduled date must be a valid date when provided
    [DataType(DataType.Date)]
    public DateTime? ScheduledDate { get; set; }

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    // For card payments - use tokenized card ID, never raw card data
    [StringLength(200, ErrorMessage = "Tokenized card ID cannot exceed 200 characters")]
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
/// Process payment DTO - standard name for payment processing requests.
/// </summary>
public class ProcessPaymentDto
{
    [Required(ErrorMessage = "Amount is required")]
    [Range(typeof(decimal), "0.01", "999999999.99", ErrorMessage = "Amount must be between 0.01 and 999999999.99")]
    public decimal Amount { get; set; }
    
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CreditCard;

    /// <summary>
    /// Tokenized card ID - never raw card data
    /// </summary>
    [StringLength(200, ErrorMessage = "Tokenized card ID cannot exceed 200 characters")]
    public string? TokenizedCardId { get; set; }

    [StringLength(100, ErrorMessage = "Authorization code cannot exceed 100 characters")]
    public string? AuthorizationCode { get; set; }
    
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }
}

// PRA-013: obsolete alias — safe to delete after confirming no external API consumers.
// No usages found in source or tests; PaymentsController uses its own local ProcessPaymentRequest class.
// [Obsolete] attribute already present. Commented out as dead code.
// /// <summary>
// /// Process payment request DTO.
// /// Retained for backward compatibility — use <see cref="ProcessPaymentDto"/> instead.
// /// </summary>
// [Obsolete("Use ProcessPaymentDto instead. This class will be removed in a future version.")]
// public class ProcessPaymentRequestDto : ProcessPaymentDto
// {
// }

/// <summary>
/// Refund payment request DTO
/// </summary>
public class RefundPaymentRequestDto
{
    [Range(typeof(decimal), "0.01", "999999999.99", ErrorMessage = "Refund amount must be between 0.01 and 999999999.99")]
    public decimal? RefundAmount { get; set; }
    
    [Required(ErrorMessage = "Reason is required")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "Reason must be between 10 and 500 characters")]
    public string Reason { get; set; } = string.Empty;
}
