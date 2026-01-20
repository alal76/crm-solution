namespace CRM.Core.Entities;

/// <summary>
/// Quote status enumeration
/// </summary>
public enum QuoteStatus
{
    Draft = 0,
    Pending = 1,
    Sent = 2,
    Viewed = 3,
    Accepted = 4,
    Rejected = 5,
    Expired = 6,
    Revised = 7,
    Cancelled = 8
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
    public QuoteStatus Status { get; set; } = QuoteStatus.Draft;
    
    // Dates
    public DateTime QuoteDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpirationDate { get; set; }
    public DateTime? SentDate { get; set; }
    public DateTime? ViewedDate { get; set; }
    public DateTime? AcceptedDate { get; set; }
    public DateTime? RejectedDate { get; set; }
    
    // Pricing
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
    
    // Line Items (JSON for flexibility)
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
    
    // Approval
    public bool RequiresApproval { get; set; } = false;
    public bool IsApproved { get; set; } = false;
    public DateTime? ApprovalDate { get; set; }
    public string? ApprovalNotes { get; set; }
    
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
    
    // Custom Fields
    public string? CustomFields { get; set; }

    // Navigation properties
    public Customer? Customer { get; set; }
    public Opportunity? Opportunity { get; set; }
    public User? AssignedToUser { get; set; }
    public User? CreatedByUser { get; set; }
    public User? ApprovedByUser { get; set; }
    public Quote? ParentQuote { get; set; }
    public ICollection<Quote>? Revisions { get; set; }
}
