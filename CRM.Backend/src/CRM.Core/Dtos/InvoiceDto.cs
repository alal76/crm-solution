// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;

namespace CRM.Core.Dtos;

/// <summary>
/// Invoice data transfer object for read operations
/// </summary>
public class InvoiceDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int AccountId { get; set; }
    public string? AccountName { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? SentDate { get; set; }
    public DateTime? ViewedDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public DateTime? VoidedDate { get; set; }

    public InvoiceStatus Status { get; set; }
    public InvoiceType InvoiceType { get; set; }
    public PaymentTerms PaymentTerms { get; set; }

    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal FeesAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal BalanceDue { get; set; }

    public string CurrencyCode { get; set; } = "USD";
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public string? VoidReason { get; set; }

    public int? OrderId { get; set; }
    public int? QuoteId { get; set; }
    public bool IsDraft => Status == InvoiceStatus.Draft;
    public bool IsOverdue => Status != InvoiceStatus.Paid && DueDate < DateTime.UtcNow.Date;

    public List<InvoiceLineItemDto> LineItems { get; set; } = new();

    // Dates
    public DateTime? ServicePeriodStart { get; set; }
    public DateTime? ServicePeriodEnd { get; set; }

    // Pricing details
    public decimal? DiscountPercent { get; set; }
    public decimal? AmountCredited { get; set; }
    public decimal? ExchangeRate { get; set; }

    // Early payment discount
    public decimal? EarlyPaymentDiscountPercent { get; set; }
    public int? EarlyPaymentDiscountDays { get; set; }
    public decimal? EarlyPaymentDiscountAmount { get; set; }

    // Late fees
    public decimal? LateFeePercent { get; set; }
    public decimal? LateFeeAmount { get; set; }

    // Billing address
    public string? BillingName { get; set; }
    public string? BillingCompany { get; set; }
    public string? BillingStreet { get; set; }
    public string? BillingCity { get; set; }
    public string? BillingState { get; set; }
    public string? BillingPostalCode { get; set; }
    public string? BillingCountry { get; set; }
    public string? BillingEmail { get; set; }
    public string? BillingPhone { get; set; }

    // Collections & dunning
    public int? ReminderCount { get; set; }
    public DateTime? LastReminderDate { get; set; }
    public DateTime? NextReminderDate { get; set; }
    public bool InCollections { get; set; }

    // Documentation
    public string? InternalNotes { get; set; }
    public string? Footer { get; set; }
    public string? TermsAndConditions { get; set; }
    public string? DisputeReason { get; set; }
    public string? PdfUrl { get; set; }

    // Additional relationships
    public int? ContactId { get; set; }
    public int? SubscriptionId { get; set; }
    public int? OriginalInvoiceId { get; set; }
}

/// <summary>
/// Create invoice DTO
/// </summary>
public class CreateInvoiceDto
{
    public int AccountId { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public DateTime? DueDate { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    public InvoiceType InvoiceType { get; set; } = InvoiceType.Standard;
    public PaymentTerms PaymentTerms { get; set; } = PaymentTerms.Net30;

    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal FeesAmount { get; set; }

    public string CurrencyCode { get; set; } = "USD";
    public string? Description { get; set; }
    public string? Notes { get; set; }

    public int? OrderId { get; set; }
    public int? QuoteId { get; set; }

    public string? InternalNotes { get; set; }
    public string? TermsAndConditions { get; set; }

    // Billing address
    public string? BillingName { get; set; }
    public string? BillingStreet { get; set; }
    public string? BillingCity { get; set; }
    public string? BillingState { get; set; }
    public string? BillingCountry { get; set; }

    public List<CreateInvoiceLineItemDto> LineItems { get; set; } = new();
}

/// <summary>
/// Update invoice DTO
/// </summary>
public class UpdateInvoiceDto
{
    public DateTime? DueDate { get; set; }
    public InvoiceStatus? Status { get; set; }
    public InvoiceType? InvoiceType { get; set; }
    public PaymentTerms? PaymentTerms { get; set; }

    public decimal? DiscountAmount { get; set; }
    public decimal? ShippingAmount { get; set; }
    public decimal? FeesAmount { get; set; }

    public string? Description { get; set; }
    public string? Notes { get; set; }

    public string? InternalNotes { get; set; }
    public string? TermsAndConditions { get; set; }

    // Billing address
    public string? BillingName { get; set; }
    public string? BillingStreet { get; set; }
    public string? BillingCity { get; set; }
    public string? BillingState { get; set; }
    public string? BillingCountry { get; set; }
}

/// <summary>
/// Invoice filter DTO
/// </summary>
public class InvoiceFilterDto
{
    public int? AccountId { get; set; }
    public InvoiceStatus? Status { get; set; }
    public InvoiceType? InvoiceType { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; } = "InvoiceDate";
    public string? SortOrder { get; set; } = "desc";
}

/// <summary>
/// Invoice line item DTO
/// </summary>
public class InvoiceLineItemDto
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public int LineNumber { get; set; }
    public int? ProductId { get; set; }
    public string? ProductName { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
}

/// <summary>
/// Create invoice line item DTO
/// </summary>
public class CreateInvoiceLineItemDto
{
    public int? ProductId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
}


/// <summary>
/// Paged result DTO
/// </summary>
public class PagedResultDto<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
}
