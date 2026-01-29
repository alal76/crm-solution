using CRM.Core.Models;

namespace CRM.Core.Entities;

/// <summary>
/// Quote status enumeration - Full lifecycle
/// </summary>
public enum QuoteStatus
{
    /// <summary>New quote, not yet edited</summary>
    New = 0,
    /// <summary>Quote is being drafted</summary>
    Draft = 1,
    /// <summary>Quote is under internal approval</summary>
    UnderApproval = 2,
    /// <summary>Quote has been approved internally</summary>
    Approved = 3,
    /// <summary>Quote has been shared/sent to customer</summary>
    Shared = 4,
    /// <summary>Customer has viewed the quote</summary>
    Viewed = 5,
    /// <summary>Customer has accepted the quote</summary>
    Accepted = 6,
    /// <summary>Customer has rejected the quote</summary>
    Rejected = 7,
    /// <summary>Quote has expired (past expiration date)</summary>
    Expired = 8,
    /// <summary>Quote has been revised (superseded by new version)</summary>
    Revised = 9,
    /// <summary>Quote is cancelled</summary>
    Cancelled = 10,
    /// <summary>Quote has been converted to order</summary>
    Converted = 11,
    /// <summary>End of life - no longer active or relevant</summary>
    EndOfLife = 12
}

/// <summary>
/// Quote entity for sales quotes and proposals
/// </summary>
public class Quote : BaseEntity
{
    // Identification
    public string QuoteNumber { get; set; } = string.Empty;
    public string? ExternalQuoteId { get; set; }
    public int Version { get; set; } = 1;
    
    // Basic Information
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public QuoteStatus Status { get; set; } = QuoteStatus.New;
    
    // Dates
    public DateTime QuoteDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpirationDate { get; set; }
    public DateTime? SentDate { get; set; }
    public DateTime? ViewedDate { get; set; }
    public DateTime? AcceptedDate { get; set; }
    public DateTime? RejectedDate { get; set; }
    
    // Pricing (calculated from line items)
    public decimal Subtotal { get; set; } = 0;
    public decimal Discount { get; set; } = 0;
    public decimal DiscountPercent { get; set; } = 0;
    public string? DiscountReason { get; set; }
    public decimal Tax { get; set; } = 0;
    public decimal TaxRate { get; set; } = 0;
    public decimal ShippingCost { get; set; } = 0;
    public decimal Total { get; set; } = 0;
    public string? CurrencyCode { get; set; } = "USD";
    
    // Terms
    public string? PaymentTerms { get; set; } // Net 30, Net 60, etc.
    public string? DeliveryTerms { get; set; }
    public string? TermsAndConditions { get; set; }
    public string? Warranty { get; set; }
    public int? ValidityDays { get; set; } = 30;
    
    // Line Items (Legacy JSON field - prefer QuoteLineItems collection)
    public string? LineItems { get; set; } // JSON array of quote line items
    
    // Billing Address
    public string? BillingName { get; set; }
    public string? BillingAddress { get; set; }
    public string? BillingCity { get; set; }
    public string? BillingState { get; set; }
    public string? BillingZipCode { get; set; }
    public string? BillingCountry { get; set; }
    
    // Shipping Address
    public string? ShippingName { get; set; }
    public string? ShippingAddress { get; set; }
    public string? ShippingCity { get; set; }
    public string? ShippingState { get; set; }
    public string? ShippingZipCode { get; set; }
    public string? ShippingCountry { get; set; }
    
    // Contact Information
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    
    // Relationships
    public int? CustomerId { get; set; }
    public int? ContactId { get; set; }
    public int? OpportunityId { get; set; }
    public int? AssignedToUserId { get; set; }
    public int? CreatedByUserId { get; set; }
    public int? ApprovedByUserId { get; set; }
    public int? ParentQuoteId { get; set; } // For revisions
    
    // Relationship Manager
    public int? RelationshipManagerId { get; set; }
    
    // Approval
    public bool RequiresApproval { get; set; } = false;
    public bool IsApproved { get; set; } = false;
    public DateTime? ApprovalDate { get; set; }
    public string? ApprovalNotes { get; set; }
    public DateTime? SubmittedForApprovalDate { get; set; }
    
    // Signature
    public bool IsSigned { get; set; } = false;
    public DateTime? SignedDate { get; set; }
    public string? SignedBy { get; set; }
    public string? SignatureUrl { get; set; }
    
    // Documentation
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }
    public string? Attachments { get; set; } // JSON array
    public string? QuotePdfUrl { get; set; }
    
    // Classification
    public string? Tags { get; set; }
    public string? Category { get; set; }
    
    // Service/Delivery tracking
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? ActualDeliveryDate { get; set; }
    public int? WarrantyMonths { get; set; }
    public DateTime? WarrantyEndDate { get; set; }
    public DateTime? ServiceStartDate { get; set; }
    public DateTime? ServiceEndDate { get; set; }
    
    // Custom Fields
    public string? CustomFields { get; set; }

    // Navigation properties
    public Customer? Customer { get; set; }
    public Contact? Contact { get; set; }
    public Opportunity? Opportunity { get; set; }
    public User? AssignedToUser { get; set; }
    public User? CreatedByUser { get; set; }
    public User? ApprovedByUser { get; set; }
    public User? RelationshipManager { get; set; }
    public Quote? ParentQuote { get; set; }
    public ICollection<Quote>? Revisions { get; set; }
    
    /// <summary>
    /// Quote line items (structured line items)
    /// </summary>
    public ICollection<QuoteLineItem>? QuoteLineItems { get; set; }
    
    #region Calculation Methods
    
    /// <summary>
    /// Recalculate quote totals from line items
    /// </summary>
    public void RecalculateFromLineItems()
    {
        if (QuoteLineItems == null || !QuoteLineItems.Any())
        {
            Subtotal = 0;
            Tax = 0;
            Total = ShippingCost;
            return;
        }
        
        // Only include line items that are marked as included
        var includedItems = QuoteLineItems.Where(li => li.IsIncluded && !li.IsDeleted).ToList();
        
        foreach (var item in includedItems)
        {
            item.RecalculateTotals();
        }
        
        Subtotal = includedItems.Sum(li => li.Subtotal);
        var totalLineDiscount = includedItems.Sum(li => li.TotalDiscount);
        Tax = includedItems.Sum(li => li.TaxAmount);
        
        // Apply quote-level discount
        var afterLineDiscounts = Subtotal - totalLineDiscount;
        var quoteDiscount = DiscountPercent > 0 
            ? afterLineDiscounts * (DiscountPercent / 100) 
            : Discount;
        
        Total = afterLineDiscounts - quoteDiscount + Tax + ShippingCost;
        Discount = totalLineDiscount + quoteDiscount;
    }
    
    /// <summary>
    /// Check if quote is expired
    /// </summary>
    public bool IsExpired => ExpirationDate.HasValue && ExpirationDate.Value < DateTime.UtcNow;
    
    /// <summary>
    /// Check if quote can be edited
    /// </summary>
    public bool CanEdit => Status == QuoteStatus.New || Status == QuoteStatus.Draft;
    
    /// <summary>
    /// Check if quote can be submitted for approval
    /// </summary>
    public bool CanSubmitForApproval => Status == QuoteStatus.Draft && RequiresApproval && !IsApproved;
    
    /// <summary>
    /// Check if quote can be shared with customer
    /// </summary>
    public bool CanShare => (Status == QuoteStatus.Draft && !RequiresApproval) || 
                            (Status == QuoteStatus.Approved);
    
    #endregion
}
