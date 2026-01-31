using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Entities;

#region Product Bundle Enumerations

/// <summary>
/// FUNCTIONAL: Bundle item type.
/// TECHNICAL: Determines pricing behavior in bundle.
/// </summary>
public enum BundleItemType
{
    /// <summary>Required - must be included</summary>
    Required = 0,
    
    /// <summary>Optional - customer can add</summary>
    Optional = 1,
    
    /// <summary>Default - included but removable</summary>
    Default = 2,
    
    /// <summary>Exclusive - choose one from group</summary>
    Exclusive = 3
}

/// <summary>
/// FUNCTIONAL: How bundle price is calculated.
/// TECHNICAL: Determines pricing logic.
/// </summary>
public enum BundlePricingType
{
    /// <summary>Fixed bundle price (regardless of items)</summary>
    FixedPrice = 0,
    
    /// <summary>Sum of component prices</summary>
    ComponentSum = 1,
    
    /// <summary>Percentage discount on component sum</summary>
    PercentDiscount = 2,
    
    /// <summary>Custom pricing rules</summary>
    Custom = 3
}

/// <summary>
/// FUNCTIONAL: Bundle status.
/// TECHNICAL: Controls visibility and sellability.
/// </summary>
public enum BundleStatus
{
    /// <summary>Draft - not available for sale</summary>
    Draft = 0,
    
    /// <summary>Active - available for sale</summary>
    Active = 1,
    
    /// <summary>Inactive - disabled</summary>
    Inactive = 2,
    
    /// <summary>Archived - historical only</summary>
    Archived = 3
}

#endregion

/// <summary>
/// Product bundle definition for CPQ.
/// Allows selling multiple products together with special pricing.
/// </summary>
public class ProductBundle : BaseEntity
{
    #region Identification
    
    /// <summary>Bundle name</summary>
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>Bundle SKU</summary>
    [MaxLength(50)]
    public string? SKU { get; set; }
    
    /// <summary>Bundle code</summary>
    [MaxLength(50)]
    public string? BundleCode { get; set; }
    
    /// <summary>Bundle description</summary>
    [MaxLength(2000)]
    public string? Description { get; set; }
    
    /// <summary>Short description for listings</summary>
    [MaxLength(500)]
    public string? ShortDescription { get; set; }
    
    /// <summary>Bundle status</summary>
    public BundleStatus Status { get; set; } = BundleStatus.Draft;
    
    #endregion
    
    #region Pricing
    
    /// <summary>Pricing type</summary>
    public BundlePricingType PricingType { get; set; } = BundlePricingType.ComponentSum;
    
    /// <summary>Fixed bundle price (if fixed pricing)</summary>
    [Range(0, double.MaxValue)]
    public decimal? FixedPrice { get; set; }
    
    /// <summary>Discount percentage (if percent discount)</summary>
    [Range(0, 100)]
    public decimal? DiscountPercent { get; set; }
    
    /// <summary>Minimum bundle price (floor)</summary>
    [Range(0, double.MaxValue)]
    public decimal? MinimumPrice { get; set; }
    
    /// <summary>Maximum discount allowed</summary>
    [Range(0, 100)]
    public decimal? MaxDiscountPercent { get; set; }
    
    /// <summary>Calculated list price</summary>
    [Range(0, double.MaxValue)]
    public decimal? ListPrice { get; set; }
    
    /// <summary>Currency code</summary>
    [Required]
    [MaxLength(3)]
    public string CurrencyCode { get; set; } = "USD";
    
    #endregion
    
    #region Configuration
    
    /// <summary>Minimum items required in bundle</summary>
    [Range(0, 1000)]
    public int? MinItems { get; set; }
    
    /// <summary>Maximum items allowed in bundle</summary>
    [Range(1, 1000)]
    public int? MaxItems { get; set; }
    
    /// <summary>Allow quantity changes for items</summary>
    public bool AllowQuantityChange { get; set; } = true;
    
    /// <summary>Show component prices to customer</summary>
    public bool ShowComponentPrices { get; set; } = true;
    
    /// <summary>Allow partial configuration</summary>
    public bool AllowPartialConfiguration { get; set; } = false;
    
    #endregion
    
    #region Validity
    
    /// <summary>Bundle effective start date</summary>
    public DateTime? EffectiveStartDate { get; set; }
    
    /// <summary>Bundle effective end date</summary>
    public DateTime? EffectiveEndDate { get; set; }
    
    /// <summary>Whether bundle is currently valid</summary>
    public bool IsValid => (!EffectiveStartDate.HasValue || EffectiveStartDate <= DateTime.UtcNow) 
                           && (!EffectiveEndDate.HasValue || EffectiveEndDate >= DateTime.UtcNow)
                           && Status == BundleStatus.Active;
    
    #endregion
    
    #region Display
    
    /// <summary>Image URL</summary>
    [MaxLength(500)]
    [Url]
    public string? ImageUrl { get; set; }
    
    /// <summary>Display order in catalog</summary>
    [Range(0, int.MaxValue)]
    public int DisplayOrder { get; set; } = 0;
    
    /// <summary>Whether featured bundle</summary>
    public bool IsFeatured { get; set; } = false;
    
    /// <summary>Tags for categorization</summary>
    [MaxLength(500)]
    public string? Tags { get; set; }
    
    #endregion
    
    #region Relationships
    
    /// <summary>Bundle items</summary>
    public ICollection<ProductBundleItem> Items { get; set; } = new List<ProductBundleItem>();
    
    /// <summary>Bundle rules</summary>
    public ICollection<ProductBundleRule> Rules { get; set; } = new List<ProductBundleRule>();
    
    #endregion
}

/// <summary>
/// Individual product within a bundle.
/// </summary>
public class ProductBundleItem : BaseEntity
{
    #region Item Details
    
    /// <summary>Item type</summary>
    public BundleItemType ItemType { get; set; } = BundleItemType.Required;
    
    /// <summary>Default quantity</summary>
    [Range(0, double.MaxValue)]
    public decimal DefaultQuantity { get; set; } = 1;
    
    /// <summary>Minimum quantity</summary>
    [Range(0, double.MaxValue)]
    public decimal MinQuantity { get; set; } = 0;
    
    /// <summary>Maximum quantity</summary>
    [Range(0, double.MaxValue)]
    public decimal? MaxQuantity { get; set; }
    
    /// <summary>Display order within bundle</summary>
    [Range(0, int.MaxValue)]
    public int DisplayOrder { get; set; } = 0;
    
    #endregion
    
    #region Pricing Override
    
    /// <summary>Override price for this item in bundle</summary>
    [Range(0, double.MaxValue)]
    public decimal? OverridePrice { get; set; }
    
    /// <summary>Discount percentage for this item</summary>
    [Range(0, 100)]
    public decimal? DiscountPercent { get; set; }
    
    /// <summary>Whether this item is free in bundle</summary>
    public bool IsFree { get; set; } = false;
    
    /// <summary>Custom pricing rule (JSON)</summary>
    [MaxLength(4000)]
    public string? CustomPricing { get; set; }
    
    #endregion
    
    #region Options
    
    /// <summary>Exclusive group name (for exclusive items)</summary>
    [MaxLength(100)]
    public string? ExclusiveGroup { get; set; }
    
    /// <summary>Whether selected by default (for optional)</summary>
    public bool IsDefaultSelected { get; set; } = false;
    
    /// <summary>Allow quantity change</summary>
    public bool AllowQuantityChange { get; set; } = true;
    
    /// <summary>Allow removal</summary>
    public bool AllowRemoval { get; set; } = true;
    
    #endregion
    
    #region Relationships
    
    /// <summary>Parent bundle ID</summary>
    public int ProductBundleId { get; set; }
    
    /// <summary>Navigation to bundle</summary>
    public ProductBundle? ProductBundle { get; set; }
    
    /// <summary>Product ID</summary>
    public int ProductId { get; set; }
    
    /// <summary>Navigation to product</summary>
    public Product? Product { get; set; }
    
    #endregion
}

/// <summary>
/// Configuration rule for bundle (dependencies, incompatibilities).
/// </summary>
public class ProductBundleRule : BaseEntity
{
    /// <summary>Rule name</summary>
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>Rule type (requires, excludes, suggests)</summary>
    [Required]
    [MaxLength(50)]
    public string RuleType { get; set; } = "requires";
    
    /// <summary>Source product ID (if selected...)</summary>
    public int? SourceProductId { get; set; }
    
    /// <summary>Navigation to source product</summary>
    public Product? SourceProduct { get; set; }
    
    /// <summary>Target product ID (...then require/exclude this)</summary>
    public int? TargetProductId { get; set; }
    
    /// <summary>Navigation to target product</summary>
    public Product? TargetProduct { get; set; }
    
    /// <summary>Error message when rule violated</summary>
    [MaxLength(500)]
    public string? ErrorMessage { get; set; }
    
    /// <summary>Whether rule is active</summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>Rule priority</summary>
    [Range(0, 1000)]
    public int Priority { get; set; } = 0;
    
    /// <summary>Condition expression (JSON)</summary>
    [MaxLength(4000)]
    public string? Condition { get; set; }
    
    /// <summary>Parent bundle ID</summary>
    public int ProductBundleId { get; set; }
    
    /// <summary>Navigation to bundle</summary>
    public ProductBundle? ProductBundle { get; set; }
}
