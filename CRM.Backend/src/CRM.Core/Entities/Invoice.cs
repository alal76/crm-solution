using CRM.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Entities;

#region Invoice Enumerations

/// <summary>
/// FUNCTIONAL: Invoice lifecycle status from creation to payment.
/// TECHNICAL: Controls AR workflow, dunning, and revenue recognition.
/// </summary>
public enum InvoiceStatus
{
    /// <summary>Invoice created, not yet finalized</summary>
    Draft = 0,
    
    /// <summary>Invoice pending approval</summary>
    PendingApproval = 1,
    
    /// <summary>Invoice approved and ready to send</summary>
    Approved = 2,
    
    /// <summary>Invoice sent to customer</summary>
    Sent = 3,
    
    /// <summary>Invoice viewed by customer</summary>
    Viewed = 4,
    
    /// <summary>Partial payment received</summary>
    PartiallyPaid = 5,
    
    /// <summary>Invoice fully paid</summary>
    Paid = 6,
    
    /// <summary>Invoice past due date</summary>
    Overdue = 7,
    
    /// <summary>Invoice disputed by customer</summary>
    Disputed = 8,
    
    /// <summary>Invoice voided (cancelled)</summary>
    Voided = 9,
    
    /// <summary>Invoice written off as bad debt</summary>
    WrittenOff = 10,
    
    /// <summary>Sent to collections</summary>
    Collections = 11,
    
    /// <summary>Refunded to customer</summary>
    Refunded = 12
}

/// <summary>
/// FUNCTIONAL: Type of invoice document.
/// TECHNICAL: Determines accounting treatment and workflow.
/// </summary>
public enum InvoiceType
{
    /// <summary>Standard invoice for goods/services</summary>
    Standard = 0,
    
    /// <summary>Credit memo/credit note</summary>
    Credit = 1,
    
    /// <summary>Proforma/estimate invoice</summary>
    Proforma = 2,
    
    /// <summary>Recurring subscription invoice</summary>
    Recurring = 3,
    
    /// <summary>Deposit/advance invoice</summary>
    Deposit = 4,
    
    /// <summary>Progress billing invoice</summary>
    Progress = 5,
    
    /// <summary>Final invoice</summary>
    Final = 6,
    
    /// <summary>Adjustment invoice</summary>
    Adjustment = 7,
    
    /// <summary>Debit memo</summary>
    DebitMemo = 8
}

/// <summary>
/// FUNCTIONAL: Payment terms for invoice.
/// TECHNICAL: Drives due date calculation and dunning.
/// </summary>
public enum PaymentTerms
{
    /// <summary>Due upon receipt</summary>
    DueOnReceipt = 0,
    
    /// <summary>Net 7 days</summary>
    Net7 = 1,
    
    /// <summary>Net 10 days</summary>
    Net10 = 2,
    
    /// <summary>Net 15 days</summary>
    Net15 = 3,
    
    /// <summary>Net 30 days</summary>
    Net30 = 4,
    
    /// <summary>Net 45 days</summary>
    Net45 = 5,
    
    /// <summary>Net 60 days</summary>
    Net60 = 6,
    
    /// <summary>Net 90 days</summary>
    Net90 = 7,
    
    /// <summary>2% 10 Net 30</summary>
    TwoTenNet30 = 8,
    
    /// <summary>End of month</summary>
    EndOfMonth = 9,
    
    /// <summary>Custom terms</summary>
    Custom = 10
}

#endregion

/// <summary>
/// Invoice entity for billing customers.
/// Created from Order, triggers Payment workflow.
/// </summary>
public class Invoice : BaseEntity
{
    #region Identification
    
    /// <summary>System-generated unique invoice number</summary>
    [Required]
    [MaxLength(50)]
    public string InvoiceNumber { get; set; } = string.Empty;
    
    /// <summary>External reference number</summary>
    [MaxLength(100)]
    public string? ExternalInvoiceId { get; set; }
    
    /// <summary>Reference to related documents</summary>
    [MaxLength(100)]
    public string? ReferenceNumber { get; set; }
    
    /// <summary>Batch number for batch processing</summary>
    [MaxLength(50)]
    public string? BatchNumber { get; set; }
    
    #endregion
    
    #region Invoice Details
    
    /// <summary>Invoice description/memo</summary>
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    /// <summary>Current invoice status</summary>
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    
    /// <summary>Type of invoice</summary>
    public InvoiceType InvoiceType { get; set; } = InvoiceType.Standard;
    
    /// <summary>Payment terms</summary>
    public PaymentTerms PaymentTerms { get; set; } = PaymentTerms.Net30;
    
    /// <summary>Custom payment terms description</summary>
    [MaxLength(500)]
    public string? PaymentTermsDescription { get; set; }
    
    #endregion
    
    #region Dates
    
    /// <summary>Date invoice was created</summary>
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>Payment due date</summary>
    public DateTime DueDate { get; set; }
    
    /// <summary>Date invoice was sent to customer</summary>
    public DateTime? SentDate { get; set; }
    
    /// <summary>Date customer viewed invoice</summary>
    public DateTime? ViewedDate { get; set; }
    
    /// <summary>Date invoice was paid in full</summary>
    public DateTime? PaidDate { get; set; }
    
    /// <summary>Date invoice was voided</summary>
    public DateTime? VoidedDate { get; set; }
    
    /// <summary>Service period start</summary>
    public DateTime? ServicePeriodStart { get; set; }
    
    /// <summary>Service period end</summary>
    public DateTime? ServicePeriodEnd { get; set; }
    
    #endregion
    
    #region Amounts
    
    /// <summary>Sum of line items before tax and discounts</summary>
    [Range(0, double.MaxValue)]
    public decimal Subtotal { get; set; } = 0;
    
    /// <summary>Total discount amount</summary>
    [Range(0, double.MaxValue)]
    public decimal DiscountAmount { get; set; } = 0;
    
    /// <summary>Discount percentage</summary>
    [Range(0, 100)]
    public decimal DiscountPercent { get; set; } = 0;
    
    /// <summary>Tax amount</summary>
    [Range(0, double.MaxValue)]
    public decimal TaxAmount { get; set; } = 0;
    
    /// <summary>Tax rate percentage</summary>
    [Range(0, 100)]
    public decimal TaxRate { get; set; } = 0;
    
    /// <summary>Shipping/freight charges</summary>
    [Range(0, double.MaxValue)]
    public decimal ShippingAmount { get; set; } = 0;
    
    /// <summary>Additional fees</summary>
    [Range(0, double.MaxValue)]
    public decimal FeesAmount { get; set; } = 0;
    
    /// <summary>Total invoice amount</summary>
    [Range(0, double.MaxValue)]
    public decimal TotalAmount { get; set; } = 0;
    
    /// <summary>Amount paid so far</summary>
    [Range(0, double.MaxValue)]
    public decimal AmountPaid { get; set; } = 0;
    
    /// <summary>Amount credited/adjusted</summary>
    [Range(0, double.MaxValue)]
    public decimal AmountCredited { get; set; } = 0;
    
    /// <summary>Outstanding balance</summary>
    public decimal BalanceDue => TotalAmount - AmountPaid - AmountCredited;
    
    /// <summary>Whether fully paid</summary>
    public bool IsPaid => BalanceDue <= 0;
    
    /// <summary>Currency code (ISO 4217)</summary>
    [Required]
    [MaxLength(3)]
    public string CurrencyCode { get; set; } = "USD";
    
    /// <summary>Exchange rate if foreign currency</summary>
    public decimal? ExchangeRate { get; set; }
    
    #endregion
    
    #region Early Payment Discount
    
    /// <summary>Early payment discount percentage</summary>
    public decimal? EarlyPaymentDiscountPercent { get; set; }
    
    /// <summary>Days for early payment discount</summary>
    public int? EarlyPaymentDiscountDays { get; set; }
    
    /// <summary>Early payment discount amount</summary>
    public decimal? EarlyPaymentDiscountAmount { get; set; }
    
    #endregion
    
    #region Late Fees
    
    /// <summary>Late fee percentage per period</summary>
    public decimal? LateFeePercent { get; set; }
    
    /// <summary>Flat late fee amount</summary>
    public decimal? LateFeeAmount { get; set; }
    
    /// <summary>Total late fees accrued</summary>
    public decimal LateFeeTotal { get; set; } = 0;
    
    /// <summary>Days overdue</summary>
    public int DaysOverdue => IsPaid || DueDate > DateTime.UtcNow ? 0 : (DateTime.UtcNow - DueDate).Days;
    
    #endregion
    
    #region Billing Address
    
    [MaxLength(255)]
    public string? BillingName { get; set; }
    [MaxLength(255)]
    public string? BillingCompany { get; set; }
    [MaxLength(500)]
    public string? BillingStreet { get; set; }
    [MaxLength(100)]
    public string? BillingCity { get; set; }
    [MaxLength(100)]
    public string? BillingState { get; set; }
    [MaxLength(20)]
    public string? BillingPostalCode { get; set; }
    [MaxLength(100)]
    public string? BillingCountry { get; set; }
    [MaxLength(255)]
    [EmailAddress]
    public string? BillingEmail { get; set; }
    [MaxLength(30)]
    [Phone]
    public string? BillingPhone { get; set; }
    
    #endregion
    
    #region Dunning & Collections
    
    /// <summary>Number of reminders sent</summary>
    public int ReminderCount { get; set; } = 0;
    
    /// <summary>Date of last reminder</summary>
    public DateTime? LastReminderDate { get; set; }
    
    /// <summary>Date of next scheduled reminder</summary>
    public DateTime? NextReminderDate { get; set; }
    
    /// <summary>Whether in collections</summary>
    public bool InCollections { get; set; } = false;
    
    /// <summary>Date sent to collections</summary>
    public DateTime? CollectionsDate { get; set; }
    
    /// <summary>Collections agency reference</summary>
    [MaxLength(100)]
    public string? CollectionsReference { get; set; }
    
    #endregion
    
    #region Relationships
    
    /// <summary>Customer account ID</summary>
    public int AccountId { get; set; }
    
    /// <summary>Navigation to customer account</summary>
    public Account? Account { get; set; }
    
    /// <summary>Related order ID</summary>
    public int? OrderId { get; set; }
    
    /// <summary>Navigation to related order</summary>
    public Order? Order { get; set; }
    
    /// <summary>Related subscription ID</summary>
    public int? SubscriptionId { get; set; }
    
    /// <summary>Navigation to subscription</summary>
    public Subscription? Subscription { get; set; }
    
    /// <summary>Primary contact ID</summary>
    public int? ContactId { get; set; }
    
    /// <summary>Navigation to primary contact</summary>
    public Contact? Contact { get; set; }
    
    /// <summary>Voided by user ID</summary>
    public int? VoidedById { get; set; }
    
    /// <summary>Navigation to user who voided</summary>
    public User? VoidedBy { get; set; }
    
    /// <summary>Original invoice (for credit memos)</summary>
    public int? OriginalInvoiceId { get; set; }
    
    /// <summary>Navigation to original invoice</summary>
    public Invoice? OriginalInvoice { get; set; }
    
    /// <summary>Credit memos applied to this invoice</summary>
    public ICollection<Invoice> CreditMemos { get; set; } = new List<Invoice>();
    
    /// <summary>Invoice line items</summary>
    public ICollection<InvoiceLineItem> LineItems { get; set; } = new List<InvoiceLineItem>();
    
    /// <summary>Payments received</summary>
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    
    #endregion
    
    #region Notes & Documents
    
    /// <summary>Notes displayed on invoice</summary>
    [MaxLength(2000)]
    public string? Notes { get; set; }
    
    /// <summary>Internal notes (not shown to customer)</summary>
    [MaxLength(2000)]
    public string? InternalNotes { get; set; }
    
    /// <summary>Footer text</summary>
    [MaxLength(1000)]
    public string? Footer { get; set; }
    
    /// <summary>Terms and conditions</summary>
    [MaxLength(5000)]
    public string? TermsAndConditions { get; set; }
    
    /// <summary>Void reason</summary>
    [MaxLength(500)]
    public string? VoidReason { get; set; }
    
    /// <summary>Dispute reason</summary>
    [MaxLength(500)]
    public string? DisputeReason { get; set; }
    
    /// <summary>PDF URL</summary>
    [MaxLength(500)]
    [Url]
    public string? PdfUrl { get; set; }
    
    #endregion
}

/// <summary>
/// Line item within an Invoice.
/// </summary>
public class InvoiceLineItem : BaseEntity
{
    #region Identification
    
    /// <summary>Line item number for display</summary>
    [Range(1, int.MaxValue)]
    public int LineNumber { get; set; }
    
    /// <summary>External line reference</summary>
    [MaxLength(100)]
    public string? ExternalLineId { get; set; }
    
    #endregion
    
    #region Product Details
    
    /// <summary>Item name/description</summary>
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>Detailed description</summary>
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    /// <summary>Product SKU</summary>
    [MaxLength(50)]
    public string? SKU { get; set; }
    
    /// <summary>Product code</summary>
    [MaxLength(50)]
    public string? ProductCode { get; set; }
    
    #endregion
    
    #region Quantity & Pricing
    
    /// <summary>Quantity billed</summary>
    [Range(0.001, double.MaxValue)]
    public decimal Quantity { get; set; } = 1;
    
    /// <summary>Unit of measure</summary>
    [MaxLength(50)]
    public string? UnitOfMeasure { get; set; }
    
    /// <summary>Unit price</summary>
    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; set; } = 0;
    
    /// <summary>Discount amount</summary>
    [Range(0, double.MaxValue)]
    public decimal DiscountAmount { get; set; } = 0;
    
    /// <summary>Discount percentage</summary>
    [Range(0, 100)]
    public decimal DiscountPercent { get; set; } = 0;
    
    /// <summary>Extended amount (quantity × price - discount)</summary>
    [Range(0, double.MaxValue)]
    public decimal ExtendedAmount { get; set; } = 0;
    
    /// <summary>Tax amount for this line</summary>
    [Range(0, double.MaxValue)]
    public decimal TaxAmount { get; set; } = 0;
    
    /// <summary>Tax rate for this line</summary>
    [Range(0, 100)]
    public decimal? TaxRate { get; set; }
    
    /// <summary>Total line amount including tax</summary>
    [Range(0, double.MaxValue)]
    public decimal TotalAmount { get; set; } = 0;
    
    #endregion
    
    #region Service Period
    
    /// <summary>Service period start</summary>
    public DateTime? ServiceStartDate { get; set; }
    
    /// <summary>Service period end</summary>
    public DateTime? ServiceEndDate { get; set; }
    
    #endregion
    
    #region Revenue Recognition
    
    /// <summary>Revenue recognition start date</summary>
    public DateTime? RevenueRecognitionStartDate { get; set; }
    
    /// <summary>Revenue recognition end date</summary>
    public DateTime? RevenueRecognitionEndDate { get; set; }
    
    /// <summary>Deferred revenue amount</summary>
    [Range(0, double.MaxValue)]
    public decimal? DeferredRevenue { get; set; }
    
    /// <summary>Recognized revenue amount</summary>
    [Range(0, double.MaxValue)]
    public decimal? RecognizedRevenue { get; set; }
    
    #endregion
    
    #region Relationships
    
    /// <summary>Parent invoice ID</summary>
    public int InvoiceId { get; set; }
    
    /// <summary>Navigation to parent invoice</summary>
    public Invoice? Invoice { get; set; }
    
    /// <summary>Product ID</summary>
    public int? ProductId { get; set; }
    
    /// <summary>Navigation to product</summary>
    public Product? Product { get; set; }
    
    /// <summary>Related order line item ID</summary>
    public int? OrderLineItemId { get; set; }
    
    /// <summary>Navigation to order line item</summary>
    public OrderLineItem? OrderLineItem { get; set; }
    
    /// <summary>Related subscription ID</summary>
    public int? SubscriptionId { get; set; }
    
    /// <summary>Navigation to subscription</summary>
    public Subscription? Subscription { get; set; }
    
    #endregion
    
    #region Notes
    
    /// <summary>Line item notes</summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }
    
    #endregion
}
