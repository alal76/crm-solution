namespace CRM.Core.Entities;

#region Pricing Enumerations

/// <summary>
/// FUNCTIONAL: Type of pricing rule.
/// TECHNICAL: Determines pricing calculation logic.
/// </summary>
public enum PricingRuleType
{
    /// <summary>Volume/quantity discount</summary>
    VolumeDiscount = 0,
    
    /// <summary>Customer-specific price</summary>
    CustomerSpecific = 1,
    
    /// <summary>Contract price</summary>
    ContractPrice = 2,
    
    /// <summary>Promotional price</summary>
    Promotional = 3,
    
    /// <summary>Tiered pricing</summary>
    TieredPricing = 4,
    
    /// <summary>Package/bundle price</summary>
    PackagePrice = 5,
    
    /// <summary>Seasonal price</summary>
    Seasonal = 6,
    
    /// <summary>Partner/reseller price</summary>
    PartnerPrice = 7,
    
    /// <summary>Markup from cost</summary>
    CostPlusMarkup = 8,
    
    /// <summary>Price floor/ceiling</summary>
    PriceGuardrail = 9
}

/// <summary>
/// FUNCTIONAL: How discount is applied.
/// TECHNICAL: Determines calculation method.
/// </summary>
public enum DiscountMethod
{
    /// <summary>Percentage off list price</summary>
    PercentOff = 0,
    
    /// <summary>Fixed amount off</summary>
    AmountOff = 1,
    
    /// <summary>Fixed price override</summary>
    FixedPrice = 2,
    
    /// <summary>Markup from cost</summary>
    CostMarkup = 3,
    
    /// <summary>Margin target</summary>
    MarginTarget = 4
}

/// <summary>
/// FUNCTIONAL: Price book status.
/// TECHNICAL: Controls visibility and applicability.
/// </summary>
public enum PriceBookStatus
{
    /// <summary>Draft - not active</summary>
    Draft = 0,
    
    /// <summary>Active - in use</summary>
    Active = 1,
    
    /// <summary>Inactive - disabled</summary>
    Inactive = 2,
    
    /// <summary>Archived - historical</summary>
    Archived = 3
}

#endregion

/// <summary>
/// Price book for grouping product prices.
/// Supports multiple currencies, customer segments, and regions.
/// </summary>
public class PriceBook : BaseEntity
{
    #region Identification
    
    /// <summary>Price book name</summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>Price book code</summary>
    public string? Code { get; set; }
    
    /// <summary>Description</summary>
    public string? Description { get; set; }
    
    /// <summary>Status</summary>
    public PriceBookStatus Status { get; set; } = PriceBookStatus.Draft;
    
    /// <summary>Whether this is the standard/default price book</summary>
    public bool IsStandard { get; set; } = false;
    
    #endregion
    
    #region Applicability
    
    /// <summary>Currency code (ISO 4217)</summary>
    public string CurrencyCode { get; set; } = "USD";
    
    /// <summary>Country codes (comma-separated, null = all)</summary>
    public string? Countries { get; set; }
    
    /// <summary>Customer segment (Enterprise, SMB, etc.)</summary>
    public string? CustomerSegment { get; set; }
    
    /// <summary>Channel (direct, partner, etc.)</summary>
    public string? Channel { get; set; }
    
    #endregion
    
    #region Validity
    
    /// <summary>Effective start date</summary>
    public DateTime? EffectiveStartDate { get; set; }
    
    /// <summary>Effective end date</summary>
    public DateTime? EffectiveEndDate { get; set; }
    
    /// <summary>Whether currently valid</summary>
    public bool IsValid => Status == PriceBookStatus.Active
                           && (!EffectiveStartDate.HasValue || EffectiveStartDate <= DateTime.UtcNow)
                           && (!EffectiveEndDate.HasValue || EffectiveEndDate >= DateTime.UtcNow);
    
    #endregion
    
    #region Priority
    
    /// <summary>Priority (higher = more specific, takes precedence)</summary>
    public int Priority { get; set; } = 0;
    
    #endregion
    
    #region Relationships
    
    /// <summary>Price book entries</summary>
    public ICollection<PriceBookEntry> Entries { get; set; } = new List<PriceBookEntry>();
    
    /// <summary>Accounts using this price book</summary>
    public ICollection<Account> Accounts { get; set; } = new List<Account>();
    
    #endregion
}

/// <summary>
/// Individual product price within a price book.
/// </summary>
public class PriceBookEntry : BaseEntity
{
    #region Pricing
    
    /// <summary>Unit list price</summary>
    public decimal ListPrice { get; set; } = 0;
    
    /// <summary>Unit price (may be discounted)</summary>
    public decimal UnitPrice { get; set; } = 0;
    
    /// <summary>Minimum price (floor)</summary>
    public decimal? MinPrice { get; set; }
    
    /// <summary>Maximum price (ceiling)</summary>
    public decimal? MaxPrice { get; set; }
    
    /// <summary>Cost for margin calculation</summary>
    public decimal? Cost { get; set; }
    
    /// <summary>Standard discount percentage</summary>
    public decimal? StandardDiscount { get; set; }
    
    #endregion
    
    #region Validity
    
    /// <summary>Whether entry is active</summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>Effective start date</summary>
    public DateTime? EffectiveStartDate { get; set; }
    
    /// <summary>Effective end date</summary>
    public DateTime? EffectiveEndDate { get; set; }
    
    #endregion
    
    #region Relationships
    
    /// <summary>Price book ID</summary>
    public int PriceBookId { get; set; }
    
    /// <summary>Navigation to price book</summary>
    public PriceBook? PriceBook { get; set; }
    
    /// <summary>Product ID</summary>
    public int ProductId { get; set; }
    
    /// <summary>Navigation to product</summary>
    public Product? Product { get; set; }
    
    #endregion
}

/// <summary>
/// Dynamic pricing rule for automated price adjustments.
/// </summary>
public class PricingRule : BaseEntity
{
    #region Identification
    
    /// <summary>Rule name</summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>Rule description</summary>
    public string? Description { get; set; }
    
    /// <summary>Rule type</summary>
    public PricingRuleType RuleType { get; set; }
    
    /// <summary>Whether rule is active</summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>Priority (lower = higher priority)</summary>
    public int Priority { get; set; } = 100;
    
    #endregion
    
    #region Applicability
    
    /// <summary>Apply to all products</summary>
    public bool AppliesToAllProducts { get; set; } = false;
    
    /// <summary>Product IDs (comma-separated)</summary>
    public string? ProductIds { get; set; }
    
    /// <summary>Product categories (comma-separated)</summary>
    public string? ProductCategories { get; set; }
    
    /// <summary>Customer IDs (comma-separated, null = all)</summary>
    public string? CustomerIds { get; set; }
    
    /// <summary>Customer segments (comma-separated)</summary>
    public string? CustomerSegments { get; set; }
    
    #endregion
    
    #region Discount Configuration
    
    /// <summary>Discount method</summary>
    public DiscountMethod DiscountMethod { get; set; } = DiscountMethod.PercentOff;
    
    /// <summary>Discount value (percent or amount)</summary>
    public decimal? DiscountValue { get; set; }
    
    /// <summary>Fixed price (for fixed price method)</summary>
    public decimal? FixedPrice { get; set; }
    
    /// <summary>Minimum order amount for rule</summary>
    public decimal? MinOrderAmount { get; set; }
    
    /// <summary>Minimum quantity for rule</summary>
    public decimal? MinQuantity { get; set; }
    
    /// <summary>Maximum discount amount (cap)</summary>
    public decimal? MaxDiscountAmount { get; set; }
    
    #endregion
    
    #region Volume Tiers
    
    /// <summary>Volume pricing tiers (JSON array)</summary>
    public string? VolumeTiers { get; set; }
    /*
    [
        { "minQty": 1, "maxQty": 10, "discount": 0 },
        { "minQty": 11, "maxQty": 50, "discount": 5 },
        { "minQty": 51, "maxQty": null, "discount": 10 }
    ]
    */
    
    #endregion
    
    #region Validity
    
    /// <summary>Effective start date</summary>
    public DateTime? EffectiveStartDate { get; set; }
    
    /// <summary>Effective end date</summary>
    public DateTime? EffectiveEndDate { get; set; }
    
    /// <summary>Usage limit (total times rule can be used)</summary>
    public int? UsageLimit { get; set; }
    
    /// <summary>Current usage count</summary>
    public int UsageCount { get; set; } = 0;
    
    #endregion
    
    #region Conditions
    
    /// <summary>Condition expression (JSON)</summary>
    public string? Conditions { get; set; }
    
    /// <summary>Combine with other rules (or exclusive)</summary>
    public bool CombineWithOtherRules { get; set; } = true;
    
    #endregion
}

/// <summary>
/// Usage record for pricing rule audit.
/// </summary>
public class PricingRuleUsage : BaseEntity
{
    /// <summary>Pricing rule ID</summary>
    public int PricingRuleId { get; set; }
    
    /// <summary>Navigation to pricing rule</summary>
    public PricingRule? PricingRule { get; set; }
    
    /// <summary>Quote ID where applied</summary>
    public int? QuoteId { get; set; }
    
    /// <summary>Navigation to quote</summary>
    public Quote? Quote { get; set; }
    
    /// <summary>Order ID where applied</summary>
    public int? OrderId { get; set; }
    
    /// <summary>Navigation to order</summary>
    public Order? Order { get; set; }
    
    /// <summary>Discount amount applied</summary>
    public decimal DiscountAmount { get; set; }
    
    /// <summary>Date applied</summary>
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>User who applied</summary>
    public int? AppliedById { get; set; }
    
    /// <summary>Navigation to user</summary>
    public User? AppliedBy { get; set; }
}
