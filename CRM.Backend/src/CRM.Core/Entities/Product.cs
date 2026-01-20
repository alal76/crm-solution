namespace CRM.Core.Entities;

/// <summary>
/// Product status enumeration
/// </summary>
public enum ProductStatus
{
    Draft = 0,
    Active = 1,
    Discontinued = 2,
    OutOfStock = 3,
    ComingSoon = 4,
    Archived = 5
}

/// <summary>
/// Product type enumeration
/// </summary>
public enum ProductType
{
    Physical = 0,
    Digital = 1,
    Service = 2,
    Subscription = 3,
    Bundle = 4,
    Rental = 5
}

/// <summary>
/// Billing frequency for subscriptions
/// </summary>
public enum BillingFrequency
{
    OneTime = 0,
    Daily = 1,
    Weekly = 2,
    Monthly = 3,
    Quarterly = 4,
    Annually = 5,
    Custom = 6
}

/// <summary>
/// Product entity for managing product information
/// </summary>
public class Product : BaseEntity
{
    // Basic Information
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string? Barcode { get; set; } // UPC, EAN, etc.
    public string? ExternalId { get; set; } // For external system integration
    
    // Classification
    public ProductType ProductType { get; set; } = ProductType.Physical;
    public ProductStatus Status { get; set; } = ProductStatus.Active;
    public string Category { get; set; } = string.Empty;
    public string? SubCategory { get; set; }
    public string? Brand { get; set; }
    public string? Manufacturer { get; set; }
    public string? Tags { get; set; } // Comma-separated tags
    
    // Pricing
    public decimal Price { get; set; } = 0;
    public decimal? ListPrice { get; set; } // MSRP
    public decimal Cost { get; set; } = 0;
    public decimal? MinimumPrice { get; set; }
    public decimal? WholesalePrice { get; set; }
    public decimal Margin { get; set; } = 0; // Calculated profit margin
    public string? CurrencyCode { get; set; } = "USD";
    public bool IsTaxable { get; set; } = true;
    public decimal? TaxRate { get; set; }
    
    // Subscription Pricing (for SaaS products)
    public BillingFrequency BillingFrequency { get; set; } = BillingFrequency.OneTime;
    public decimal? RecurringPrice { get; set; }
    public decimal? SetupFee { get; set; }
    public int? TrialPeriodDays { get; set; }
    public int? ContractLengthMonths { get; set; }
    
    // Inventory
    public int Quantity { get; set; } = 0;
    public int? ReorderLevel { get; set; }
    public int? ReorderQuantity { get; set; }
    public int? MaxQuantity { get; set; }
    public int? ReservedQuantity { get; set; }
    public int? AvailableQuantity { get; set; }
    public string? WarehouseLocation { get; set; }
    public bool TrackInventory { get; set; } = true;
    public bool AllowBackorder { get; set; } = false;
    
    // Physical Attributes
    public decimal? Weight { get; set; }
    public string? WeightUnit { get; set; } = "kg";
    public decimal? Length { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    public string? DimensionUnit { get; set; } = "cm";
    
    // Media
    public string ImageUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string? AdditionalImages { get; set; } // JSON array of URLs
    public string? VideoUrl { get; set; }
    public string? DocumentUrls { get; set; } // JSON array of document URLs
    
    // SEO & Marketing
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public string? Slug { get; set; } // URL-friendly name
    
    // Relationships
    public int? ParentProductId { get; set; } // For variants
    public int? VendorId { get; set; }
    public int? ProductFamilyId { get; set; }
    
    // Features & Specifications
    public string? Features { get; set; } // JSON array of features
    public string? Specifications { get; set; } // JSON object of specs
    public string? Warranty { get; set; }
    public string? SupportInfo { get; set; }
    
    // Sales Information
    public int TotalSold { get; set; } = 0;
    public decimal TotalRevenue { get; set; } = 0;
    public double AverageRating { get; set; } = 0;
    public int ReviewCount { get; set; } = 0;
    public bool IsFeatured { get; set; } = false;
    public bool IsBestSeller { get; set; } = false;
    
    // Dates
    public DateTime? AvailableFrom { get; set; }
    public DateTime? AvailableTo { get; set; }
    public DateTime? DiscontinuedDate { get; set; }
    
    // Legacy
    public bool IsActive { get; set; } = true;
    
    // Custom Fields
    public string? CustomFields { get; set; } // JSON for custom data

    // Navigation properties
    public ICollection<Opportunity>? Opportunities { get; set; }
    public ICollection<MarketingCampaign>? MarketingCampaigns { get; set; }
    public Product? ParentProduct { get; set; }
    public ICollection<Product>? Variants { get; set; }
}
