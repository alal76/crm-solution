namespace CRM.Core.Entities;

/// <summary>
/// Quote line item discount type
/// </summary>
public enum LineItemDiscountType
{
    None = 0,
    Percentage = 1,
    FixedAmount = 2
}

/// <summary>
/// Quote line item entity for detailed quote building
/// Each line item represents a product/service with quantity and pricing
/// </summary>
public class QuoteLineItem : BaseEntity
{
    #region Quote Relationship
    
    /// <summary>
    /// Parent quote ID (required)
    /// </summary>
    public int QuoteId { get; set; }
    
    /// <summary>
    /// Line item sequence number within the quote
    /// </summary>
    public int LineNumber { get; set; }
    
    #endregion
    
    #region Product/Service Reference
    
    /// <summary>
    /// Reference to Product entity (optional - can be manual entry)
    /// </summary>
    public int? ProductId { get; set; }
    
    /// <summary>
    /// Stock Keeping Unit - product identifier
    /// </summary>
    public string? SKU { get; set; }
    
    /// <summary>
    /// Product/service name (can differ from product name if customized)
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Line item description
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Product category for grouping
    /// </summary>
    public string? Category { get; set; }
    
    #endregion
    
    #region Quantity
    
    /// <summary>
    /// Quantity of items
    /// </summary>
    public decimal Quantity { get; set; } = 1;
    
    /// <summary>
    /// Unit of measure (e.g., "each", "hour", "month", "license")
    /// </summary>
    public string? UnitOfMeasure { get; set; } = "each";
    
    #endregion
    
    #region Pricing
    
    /// <summary>
    /// Unit price before discounts
    /// </summary>
    public decimal UnitPrice { get; set; } = 0;
    
    /// <summary>
    /// List price (MSRP) for comparison
    /// </summary>
    public decimal? ListPrice { get; set; }
    
    /// <summary>
    /// Cost price for margin calculation
    /// </summary>
    public decimal? CostPrice { get; set; }
    
    #endregion
    
    #region Discounts
    
    /// <summary>
    /// Type of discount applied to this line item
    /// </summary>
    public LineItemDiscountType DiscountType { get; set; } = LineItemDiscountType.None;
    
    /// <summary>
    /// Discount percentage (0-100)
    /// </summary>
    public decimal DiscountPercent { get; set; } = 0;
    
    /// <summary>
    /// Discount fixed amount
    /// </summary>
    public decimal DiscountAmount { get; set; } = 0;
    
    /// <summary>
    /// Reason for discount (for approval/audit)
    /// </summary>
    public string? DiscountReason { get; set; }
    
    /// <summary>
    /// Whether discount requires approval
    /// </summary>
    public bool DiscountRequiresApproval { get; set; } = false;
    
    /// <summary>
    /// Whether discount has been approved
    /// </summary>
    public bool DiscountApproved { get; set; } = false;
    
    #endregion
    
    #region Tax
    
    /// <summary>
    /// Tax rate percentage for this line item
    /// </summary>
    public decimal TaxRate { get; set; } = 0;
    
    /// <summary>
    /// Whether item is taxable
    /// </summary>
    public bool IsTaxable { get; set; } = true;
    
    /// <summary>
    /// Tax code reference
    /// </summary>
    public string? TaxCode { get; set; }
    
    #endregion
    
    #region Calculated Totals
    
    /// <summary>
    /// Subtotal before discount (Quantity * UnitPrice)
    /// </summary>
    public decimal Subtotal { get; set; } = 0;
    
    /// <summary>
    /// Total discount applied
    /// </summary>
    public decimal TotalDiscount { get; set; } = 0;
    
    /// <summary>
    /// Tax amount
    /// </summary>
    public decimal TaxAmount { get; set; } = 0;
    
    /// <summary>
    /// Final total (Subtotal - TotalDiscount + TaxAmount)
    /// </summary>
    public decimal Total { get; set; } = 0;
    
    /// <summary>
    /// Profit margin (Total - (CostPrice * Quantity))
    /// </summary>
    public decimal? Margin { get; set; }
    
    #endregion
    
    #region Service/Subscription Details
    
    /// <summary>
    /// For subscriptions: billing period (monthly, yearly, etc.)
    /// </summary>
    public string? BillingPeriod { get; set; }
    
    /// <summary>
    /// For services: warranty duration in months
    /// </summary>
    public int? WarrantyMonths { get; set; }
    
    /// <summary>
    /// For products: expected delivery date
    /// </summary>
    public DateTime? DeliveryDate { get; set; }
    
    /// <summary>
    /// For services: service start date
    /// </summary>
    public DateTime? ServiceStartDate { get; set; }
    
    /// <summary>
    /// For services: service end date
    /// </summary>
    public DateTime? ServiceEndDate { get; set; }
    
    #endregion
    
    #region Optional/Bundle
    
    /// <summary>
    /// Whether this is an optional line item
    /// </summary>
    public bool IsOptional { get; set; } = false;
    
    /// <summary>
    /// Whether this line item is included in quote total
    /// </summary>
    public bool IsIncluded { get; set; } = true;
    
    /// <summary>
    /// For bundle items: parent line item ID
    /// </summary>
    public int? ParentLineItemId { get; set; }
    
    /// <summary>
    /// Whether this is a bundle header
    /// </summary>
    public bool IsBundle { get; set; } = false;
    
    #endregion
    
    #region Notes and Custom
    
    /// <summary>
    /// Internal notes (not shown on quote)
    /// </summary>
    public string? InternalNotes { get; set; }
    
    /// <summary>
    /// Notes to show on quote
    /// </summary>
    public string? QuoteNotes { get; set; }
    
    /// <summary>
    /// Custom fields JSON
    /// </summary>
    public string? CustomFields { get; set; }
    
    #endregion
    
    #region Navigation Properties
    
    /// <summary>
    /// Parent quote
    /// </summary>
    public Quote? Quote { get; set; }
    
    /// <summary>
    /// Related product
    /// </summary>
    public Product? Product { get; set; }
    
    /// <summary>
    /// Parent line item (for bundle components)
    /// </summary>
    public QuoteLineItem? ParentLineItem { get; set; }
    
    /// <summary>
    /// Child line items (bundle components)
    /// </summary>
    public ICollection<QuoteLineItem>? BundleItems { get; set; }
    
    #endregion
    
    #region Calculation Methods
    
    /// <summary>
    /// Recalculate all totals based on quantity, price, and discounts
    /// </summary>
    public void RecalculateTotals()
    {
        Subtotal = Quantity * UnitPrice;
        
        TotalDiscount = DiscountType switch
        {
            LineItemDiscountType.Percentage => Subtotal * (DiscountPercent / 100),
            LineItemDiscountType.FixedAmount => DiscountAmount,
            _ => 0
        };
        
        var afterDiscount = Subtotal - TotalDiscount;
        TaxAmount = IsTaxable ? afterDiscount * (TaxRate / 100) : 0;
        Total = afterDiscount + TaxAmount;
        
        if (CostPrice.HasValue)
        {
            Margin = Total - (CostPrice.Value * Quantity);
        }
    }
    
    #endregion
}
