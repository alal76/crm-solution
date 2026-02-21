// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Entities;

#region Credit Memo Enumerations

/// <summary>
/// FUNCTIONAL: Credit memo status through approval and application.
/// TECHNICAL: Controls credit application workflow.
/// </summary>
public enum CreditMemoStatus
{
    /// <summary>Credit memo created, pending approval</summary>
    Draft = 0,

    /// <summary>Pending approval</summary>
    PendingApproval = 1,

    /// <summary>Approved and available for use</summary>
    Approved = 2,

    /// <summary>Partially applied to invoices</summary>
    PartiallyApplied = 3,

    /// <summary>Fully applied to invoices</summary>
    Applied = 4,

    /// <summary>Refunded to customer</summary>
    Refunded = 5,

    /// <summary>Voided/cancelled</summary>
    Voided = 6,

    /// <summary>Expired</summary>
    Expired = 7
}

/// <summary>
/// FUNCTIONAL: Reason for credit memo issuance.
/// TECHNICAL: Drives reporting and approval workflows.
/// </summary>
public enum CreditMemoReason
{
    /// <summary>Product returned</summary>
    Return = 0,

    /// <summary>Billing error correction</summary>
    BillingError = 1,

    /// <summary>Pricing adjustment</summary>
    PriceAdjustment = 2,

    /// <summary>Goodwill gesture</summary>
    Goodwill = 3,

    /// <summary>Service credit (SLA violation)</summary>
    ServiceCredit = 4,

    /// <summary>Duplicate charge</summary>
    DuplicateCharge = 5,

    /// <summary>Cancelled order</summary>
    CancelledOrder = 6,

    /// <summary>Early termination</summary>
    EarlyTermination = 7,

    /// <summary>Promotion/discount</summary>
    Promotion = 8,

    /// <summary>Subscription downgrade</summary>
    Downgrade = 9,

    /// <summary>Referral credit</summary>
    Referral = 10,

    /// <summary>Other reason</summary>
    Other = 11
}

#endregion

/// <summary>
/// Credit memo entity for customer credits and refunds.
/// Can be applied to invoices or refunded.
/// </summary>
public class CreditMemo : BaseEntity
{
    #region Identification

    /// <summary>System-generated credit memo number</summary>
    public string CreditMemoNumber { get; set; } = string.Empty;

    /// <summary>External reference number</summary>
    public string? ExternalCreditMemoId { get; set; }

    #endregion

    #region Credit Details

    /// <summary>Credit memo description</summary>
    public string? Description { get; set; }

    /// <summary>Current status</summary>
    public CreditMemoStatus Status { get; set; } = CreditMemoStatus.Draft;

    /// <summary>Reason for credit</summary>
    public CreditMemoReason Reason { get; set; } = CreditMemoReason.Other;

    /// <summary>Detailed reason notes</summary>
    public string? ReasonDetails { get; set; }

    #endregion

    #region Dates

    /// <summary>Date credit memo was created</summary>
    public DateTime CreditMemoDate { get; set; } = DateTime.UtcNow;

    /// <summary>Date credit memo was approved</summary>
    public DateTime? ApprovedDate { get; set; }

    /// <summary>Expiration date for credit use</summary>
    public DateTime? ExpirationDate { get; set; }

    /// <summary>Date credit was fully applied</summary>
    public DateTime? AppliedDate { get; set; }

    /// <summary>Date credit was refunded</summary>
    public DateTime? RefundedDate { get; set; }

    #endregion

    #region Amounts

    /// <summary>Total credit amount</summary>
    public decimal Amount { get; set; } = 0;

    /// <summary>Amount applied to invoices</summary>
    public decimal AmountApplied { get; set; } = 0;

    /// <summary>Amount refunded</summary>
    public decimal AmountRefunded { get; set; } = 0;

    /// <summary>Remaining available balance</summary>
    public decimal BalanceRemaining => Amount - AmountApplied - AmountRefunded;

    /// <summary>Tax amount (if applicable)</summary>
    public decimal TaxAmount { get; set; } = 0;

    /// <summary>Currency code (ISO 4217)</summary>
    public string CurrencyCode { get; set; } = "USD";

    #endregion

    #region Relationships

    /// <summary>Customer account ID</summary>
    public int AccountId { get; set; }

    /// <summary>Navigation to customer account</summary>
    public Account? Account { get; set; }

    /// <summary>Related invoice ID (source of credit)</summary>
    public int? SourceInvoiceId { get; set; }

    /// <summary>Navigation to source invoice</summary>
    public Invoice? SourceInvoice { get; set; }

    /// <summary>Related order ID</summary>
    public int? OrderId { get; set; }

    /// <summary>Navigation to related order</summary>
    public Order? Order { get; set; }

    /// <summary>User who created the credit memo</summary>
    public int? CreatedById { get; set; }

    /// <summary>Navigation to creator</summary>
    public User? CreatedBy { get; set; }

    /// <summary>User who approved the credit memo</summary>
    public int? ApprovedById { get; set; }

    /// <summary>Navigation to approver</summary>
    public User? ApprovedBy { get; set; }

    /// <summary>Credit applications to invoices</summary>
    public ICollection<CreditApplication> Applications { get; set; } = new List<CreditApplication>();

    /// <summary>Line items (for itemized credits)</summary>
    public ICollection<CreditMemoLineItem> LineItems { get; set; } = new List<CreditMemoLineItem>();

    #endregion

    #region Notes

    /// <summary>Internal notes</summary>
    public string? InternalNotes { get; set; }

    /// <summary>Customer-facing notes</summary>
    public string? CustomerNotes { get; set; }

    #endregion
}

/// <summary>
/// Line item within a credit memo.
/// </summary>
public class CreditMemoLineItem : BaseEntity
{
    /// <summary>Line number</summary>
    public int LineNumber { get; set; }

    /// <summary>Item description</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Detailed description</summary>
    public string? Description { get; set; }

    /// <summary>Quantity credited</summary>
    public decimal Quantity { get; set; } = 1;

    /// <summary>Unit price</summary>
    public decimal UnitPrice { get; set; } = 0;

    /// <summary>Total amount for this line</summary>
    public decimal Amount { get; set; } = 0;

    /// <summary>Parent credit memo ID</summary>
    public int CreditMemoId { get; set; }

    /// <summary>Navigation to credit memo</summary>
    public CreditMemo? CreditMemo { get; set; }

    /// <summary>Related product ID</summary>
    public int? ProductId { get; set; }

    /// <summary>Navigation to product</summary>
    public Product? Product { get; set; }

    /// <summary>Related invoice line item ID</summary>
    public int? InvoiceLineItemId { get; set; }

    /// <summary>Navigation to invoice line item</summary>
    public InvoiceLineItem? InvoiceLineItem { get; set; }
}

/// <summary>
/// Application of credit memo to an invoice.
/// </summary>
public class CreditApplication : BaseEntity
{
    /// <summary>Amount applied</summary>
    public decimal Amount { get; set; } = 0;

    /// <summary>Date applied</summary>
    public DateTime AppliedDate { get; set; } = DateTime.UtcNow;

    /// <summary>Credit memo ID</summary>
    public int CreditMemoId { get; set; }

    /// <summary>Navigation to credit memo</summary>
    public CreditMemo? CreditMemo { get; set; }

    /// <summary>Invoice ID credit is applied to</summary>
    public int InvoiceId { get; set; }

    /// <summary>Navigation to invoice</summary>
    public Invoice? Invoice { get; set; }

    /// <summary>User who applied the credit</summary>
    public int? AppliedById { get; set; }

    /// <summary>Navigation to user</summary>
    public User? AppliedBy { get; set; }

    /// <summary>Notes</summary>
    public string? Notes { get; set; }
}
