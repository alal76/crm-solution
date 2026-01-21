namespace CRM.Core.Entities;

#region Product & Service Enumerations

/// <summary>
/// FUNCTIONAL: Lifecycle status of the product/service offering.
/// TECHNICAL: Controls visibility and purchasability in the system.
/// </summary>
public enum ProductStatus
{
    /// <summary>Being configured, not available for sale</summary>
    Draft = 0,
    
    /// <summary>Available for sale</summary>
    Active = 1,
    
    /// <summary>No longer sold, may be supported</summary>
    Discontinued = 2,
    
    /// <summary>Temporarily unavailable due to stock</summary>
    OutOfStock = 3,
    
    /// <summary>Announced but not yet available</summary>
    ComingSoon = 4,
    
    /// <summary>Removed from catalog, historical only</summary>
    Archived = 5,
    
    /// <summary>Limited availability for select customers</summary>
    Limited = 6,
    
    /// <summary>Beta/pilot offering</summary>
    Beta = 7,
    
    /// <summary>End of life, no new sales or support</summary>
    EndOfLife = 8
}

/// <summary>
/// FUNCTIONAL: Primary classification of the offering type.
/// TECHNICAL: Determines which pricing model and fields apply.
/// </summary>
public enum ProductType
{
    /// <summary>Tangible goods with inventory</summary>
    Physical = 0,
    
    /// <summary>Downloadable or digital goods</summary>
    Digital = 1,
    
    /// <summary>Professional or managed service</summary>
    Service = 2,
    
    /// <summary>Recurring subscription (SaaS, etc.)</summary>
    Subscription = 3,
    
    /// <summary>Bundle of products/services</summary>
    Bundle = 4,
    
    /// <summary>Equipment rental with return</summary>
    Rental = 5,
    
    /// <summary>One-time consulting engagement</summary>
    Consulting = 6,
    
    /// <summary>Ongoing managed service</summary>
    ManagedService = 7,
    
    /// <summary>Support and maintenance contract</summary>
    SupportContract = 8,
    
    /// <summary>Training and education</summary>
    Training = 9,
    
    /// <summary>Software license (perpetual)</summary>
    License = 10,
    
    /// <summary>Professional services hours</summary>
    ProfessionalServices = 11,
    
    /// <summary>Implementation/onboarding service</summary>
    Implementation = 12
}

/// <summary>
/// FUNCTIONAL: Billing cycle frequency for recurring items.
/// TECHNICAL: Drives billing automation and revenue recognition.
/// </summary>
public enum BillingFrequency
{
    /// <summary>Single payment, no recurrence</summary>
    OneTime = 0,
    
    /// <summary>Daily billing cycle</summary>
    Daily = 1,
    
    /// <summary>Weekly billing cycle</summary>
    Weekly = 2,
    
    /// <summary>Bi-weekly (every 2 weeks)</summary>
    BiWeekly = 3,
    
    /// <summary>Monthly billing cycle</summary>
    Monthly = 4,
    
    /// <summary>Quarterly billing cycle</summary>
    Quarterly = 5,
    
    /// <summary>Semi-annual billing cycle</summary>
    SemiAnnually = 6,
    
    /// <summary>Annual billing cycle</summary>
    Annually = 7,
    
    /// <summary>Multi-year billing</summary>
    MultiYear = 8,
    
    /// <summary>Custom billing period</summary>
    Custom = 9,
    
    /// <summary>Usage-based billing</summary>
    UsageBased = 10
}

/// <summary>
/// FUNCTIONAL: Pricing model for the product/service.
/// TECHNICAL: Determines how price is calculated and displayed.
/// </summary>
public enum PricingModel
{
    /// <summary>Fixed price per unit</summary>
    FixedPrice = 0,
    
    /// <summary>Tiered pricing based on quantity</summary>
    TieredPricing = 1,
    
    /// <summary>Volume-based discounts</summary>
    VolumePricing = 2,
    
    /// <summary>Usage-based metered billing</summary>
    UsageBased = 3,
    
    /// <summary>Per-user/seat pricing</summary>
    PerUser = 4,
    
    /// <summary>Per-feature/module pricing</summary>
    PerFeature = 5,
    
    /// <summary>Flat rate for unlimited usage</summary>
    FlatRate = 6,
    
    /// <summary>Hourly rate (services)</summary>
    Hourly = 7,
    
    /// <summary>Daily rate (services)</summary>
    Daily = 8,
    
    /// <summary>Project-based pricing</summary>
    ProjectBased = 9,
    
    /// <summary>Custom quote required</summary>
    CustomQuote = 10,
    
    /// <summary>Freemium with paid tiers</summary>
    Freemium = 11
}

/// <summary>
/// FUNCTIONAL: Unit of measure for product/service delivery.
/// TECHNICAL: Used in pricing calculations and invoicing.
/// </summary>
public enum UnitOfMeasure
{
    /// <summary>Each individual unit</summary>
    Each = 0,
    
    /// <summary>Per hour of service</summary>
    Hour = 1,
    
    /// <summary>Per day of service</summary>
    Day = 2,
    
    /// <summary>Per week</summary>
    Week = 3,
    
    /// <summary>Per month</summary>
    Month = 4,
    
    /// <summary>Per year</summary>
    Year = 5,
    
    /// <summary>Per user/seat</summary>
    User = 6,
    
    /// <summary>Per device</summary>
    Device = 7,
    
    /// <summary>Per transaction</summary>
    Transaction = 8,
    
    /// <summary>Per GB of storage</summary>
    Gigabyte = 9,
    
    /// <summary>Per API call</summary>
    ApiCall = 10,
    
    /// <summary>Per project</summary>
    Project = 11,
    
    /// <summary>Per license</summary>
    License = 12,
    
    /// <summary>Per kilogram</summary>
    Kilogram = 13,
    
    /// <summary>Per meter</summary>
    Meter = 14,
    
    /// <summary>Per liter</summary>
    Liter = 15,
    
    /// <summary>Per case/box</summary>
    Case = 16,
    
    /// <summary>Per pallet</summary>
    Pallet = 17
}

/// <summary>
/// FUNCTIONAL: Revenue recognition method.
/// TECHNICAL: Determines how revenue is recorded for accounting.
/// </summary>
public enum RevenueRecognitionMethod
{
    /// <summary>Recognize immediately at sale</summary>
    Immediate = 0,
    
    /// <summary>Recognize over service period</summary>
    OverTime = 1,
    
    /// <summary>Recognize on delivery/completion</summary>
    OnDelivery = 2,
    
    /// <summary>Milestone-based recognition</summary>
    Milestone = 3,
    
    /// <summary>Percentage of completion</summary>
    PercentageOfCompletion = 4
}

/// <summary>
/// FUNCTIONAL: Service level tier for support/service contracts.
/// TECHNICAL: Determines SLA terms and pricing tier.
/// </summary>
public enum ServiceTier
{
    /// <summary>Basic/free tier</summary>
    Basic = 0,
    
    /// <summary>Standard tier</summary>
    Standard = 1,
    
    /// <summary>Professional tier</summary>
    Professional = 2,
    
    /// <summary>Enterprise tier</summary>
    Enterprise = 3,
    
    /// <summary>Premium/platinum tier</summary>
    Premium = 4,
    
    /// <summary>Custom/negotiated tier</summary>
    Custom = 5
}

/// <summary>
/// FUNCTIONAL: Contract term length category for discounting.
/// TECHNICAL: Used to determine applicable term discounts.
/// </summary>
public enum ContractTermCategory
{
    /// <summary>No contract, pay as you go</summary>
    NoContract = 0,
    
    /// <summary>Weekly contract</summary>
    Weekly = 1,
    
    /// <summary>Monthly contract</summary>
    Monthly = 2,
    
    /// <summary>Quarterly contract</summary>
    Quarterly = 3,
    
    /// <summary>Semi-annual contract</summary>
    SemiAnnual = 4,
    
    /// <summary>Annual contract</summary>
    Annual = 5,
    
    /// <summary>Two-year contract</summary>
    TwoYear = 6,
    
    /// <summary>Three-year contract</summary>
    ThreeYear = 7,
    
    /// <summary>Five-year contract</summary>
    FiveYear = 8,
    
    /// <summary>Custom term</summary>
    Custom = 9
}

#endregion

/// <summary>
/// FUNCTIONAL VIEW:
/// ================
/// Comprehensive Product &amp; Service entity representing all sellable offerings:
/// 
/// FOR PRODUCTS:
/// - Per-unit pricing with cost, margin, and discount capabilities
/// - Inventory management with reorder tracking
/// - Physical attributes (weight, dimensions)
/// - SKU/barcode tracking
/// - Volume and tiered pricing support
/// 
/// FOR SERVICES:
/// - Weekly, monthly, and annual contract pricing
/// - Configurable term-based discounts (longer term = bigger discount)
/// - Hourly/daily/project-based pricing
/// - Service level tiers (Basic → Enterprise)
/// - SLA configuration
/// 
/// FOR SUBSCRIPTIONS:
/// - Recurring billing at various frequencies
/// - Setup fees and trial periods
/// - Per-user/seat pricing
/// - Auto-renewal configuration
/// 
/// PRICING FEATURES:
/// - Multiple pricing tiers (JSON structure)
/// - Volume discounts (quantity-based)
/// - Term discounts (contract length-based)
/// - Partner/reseller pricing
/// - Currency support
/// 
/// TECHNICAL VIEW:
/// ===============
/// Unified entity supporting multiple product types with flexible pricing.
/// Uses JSON fields for complex pricing structures:
/// - PricingTiers: Array of tier definitions
/// - VolumeDiscounts: Quantity-based discount matrix
/// - TermDiscounts: Contract length discount matrix
/// - ContractPricing: Weekly/monthly/annual pricing
/// 
/// Key Relationships:
/// - Parent/child for product variants
/// - Vendor relationship
/// - Product family grouping
/// - Bundle components (JSON)
/// </summary>
public class Product : BaseEntity
{
    #region Identification & Basic Information
    
    /// <summary>
    /// FUNCTIONAL: Display name for the product/service
    /// TECHNICAL: Required, used in search and display
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// FUNCTIONAL: Full description with features and benefits
    /// TECHNICAL: Supports rich text/markdown
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// FUNCTIONAL: Brief description for lists and summaries
    /// TECHNICAL: Max 200 characters recommended
    /// </summary>
    public string? ShortDescription { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Stock keeping unit identifier
    /// TECHNICAL: Unique business key
    /// </summary>
    public string SKU { get; set; } = string.Empty;
    
    /// <summary>
    /// FUNCTIONAL: Product identifier (different from SKU for variants)
    /// TECHNICAL: Groups all variants together
    /// </summary>
    public string? ProductCode { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Universal barcode (UPC, EAN, ISBN)
    /// TECHNICAL: For inventory scanning integration
    /// </summary>
    public string? Barcode { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: External system identifier
    /// TECHNICAL: For ERP/PIM integration
    /// </summary>
    public string? ExternalId { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Internal reference number
    /// TECHNICAL: Auto-generated format: PRD-XXXXXX or SVC-XXXXXX
    /// </summary>
    public string? InternalReference { get; set; }
    
    #endregion
    
    #region Classification
    
    /// <summary>
    /// FUNCTIONAL: Primary type (Product, Service, Subscription, etc.)
    /// TECHNICAL: Determines applicable fields and pricing logic
    /// </summary>
    public ProductType ProductType { get; set; } = ProductType.Physical;
    
    /// <summary>
    /// FUNCTIONAL: Lifecycle status
    /// TECHNICAL: Controls availability for sale
    /// </summary>
    public ProductStatus Status { get; set; } = ProductStatus.Active;
    
    /// <summary>
    /// FUNCTIONAL: Primary category
    /// TECHNICAL: Used for catalog organization
    /// </summary>
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// FUNCTIONAL: Secondary category
    /// TECHNICAL: For deeper categorization
    /// </summary>
    public string? SubCategory { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Product family for grouping
    /// TECHNICAL: Links related products
    /// </summary>
    public string? ProductFamily { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Brand name
    /// TECHNICAL: For brand-based filtering
    /// </summary>
    public string? Brand { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Manufacturer name
    /// TECHNICAL: For supplier tracking
    /// </summary>
    public string? Manufacturer { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Searchable tags
    /// TECHNICAL: Comma-separated or JSON array
    /// </summary>
    public string? Tags { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Service tier level
    /// TECHNICAL: Applies to Service/Subscription types
    /// </summary>
    public ServiceTier ServiceTier { get; set; } = ServiceTier.Standard;
    
    /// <summary>
    /// FUNCTIONAL: Whether this is a service vs product
    /// TECHNICAL: Quick type check flag
    /// </summary>
    public bool IsService { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Whether this is a recurring subscription
    /// TECHNICAL: Quick type check flag
    /// </summary>
    public bool IsSubscription { get; set; } = false;
    
    #endregion
    
    #region Unit Pricing (Products)
    
    /// <summary>
    /// FUNCTIONAL: Standard unit price
    /// TECHNICAL: Base price before discounts
    /// </summary>
    public decimal Price { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Manufacturer's suggested retail price
    /// TECHNICAL: For comparison/savings display
    /// </summary>
    public decimal? ListPrice { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Cost to acquire/produce
    /// TECHNICAL: For margin calculations
    /// </summary>
    public decimal Cost { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Floor price (cannot sell below)
    /// TECHNICAL: Enforced in discount calculations
    /// </summary>
    public decimal? MinimumPrice { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Price for wholesale/reseller
    /// TECHNICAL: Applied for partner accounts
    /// </summary>
    public decimal? WholesalePrice { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Partner/reseller price
    /// TECHNICAL: Applied for partner accounts
    /// </summary>
    public decimal? PartnerPrice { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Calculated profit margin %
    /// TECHNICAL: (Price - Cost) / Price × 100
    /// </summary>
    public decimal Margin { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Target margin percentage
    /// TECHNICAL: For pricing optimization
    /// </summary>
    public decimal? TargetMargin { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Unit of measure for pricing
    /// TECHNICAL: Each, Hour, User, etc.
    /// </summary>
    public UnitOfMeasure UnitOfMeasure { get; set; } = UnitOfMeasure.Each;
    
    /// <summary>
    /// FUNCTIONAL: Custom unit of measure description
    /// TECHNICAL: When UnitOfMeasure is non-standard
    /// </summary>
    public string? CustomUnitOfMeasure { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Minimum quantity for purchase
    /// TECHNICAL: Enforced in cart/order
    /// </summary>
    public int MinimumQuantity { get; set; } = 1;
    
    /// <summary>
    /// FUNCTIONAL: Maximum quantity per order
    /// TECHNICAL: Enforced in cart/order
    /// </summary>
    public int? MaximumQuantity { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Quantity increment (e.g., buy in packs of 5)
    /// TECHNICAL: Enforced in cart/order
    /// </summary>
    public int QuantityIncrement { get; set; } = 1;
    
    #endregion
    
    #region Service Contract Pricing
    
    /// <summary>
    /// FUNCTIONAL: Weekly contract/service price
    /// TECHNICAL: Base weekly rate before term discounts
    /// </summary>
    public decimal? WeeklyPrice { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Monthly contract/service price
    /// TECHNICAL: Base monthly rate before term discounts
    /// </summary>
    public decimal? MonthlyPrice { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Quarterly contract/service price
    /// TECHNICAL: Base quarterly rate before term discounts
    /// </summary>
    public decimal? QuarterlyPrice { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Semi-annual contract/service price
    /// TECHNICAL: Base semi-annual rate before term discounts
    /// </summary>
    public decimal? SemiAnnualPrice { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Annual contract/service price
    /// TECHNICAL: Base annual rate before term discounts
    /// </summary>
    public decimal? AnnualPrice { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Two-year contract price
    /// TECHNICAL: Often includes multi-year discount
    /// </summary>
    public decimal? TwoYearPrice { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Three-year contract price
    /// TECHNICAL: Often includes multi-year discount
    /// </summary>
    public decimal? ThreeYearPrice { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Comprehensive contract pricing matrix
    /// TECHNICAL: JSON object with pricing per term:
    /// {
    ///   "weekly": {"price": 100, "discount": 0},
    ///   "monthly": {"price": 350, "discount": 12.5},
    ///   "quarterly": {"price": 900, "discount": 25},
    ///   "semiAnnual": {"price": 1600, "discount": 33},
    ///   "annual": {"price": 2800, "discount": 42},
    ///   "twoYear": {"price": 5000, "discount": 52},
    ///   "threeYear": {"price": 6600, "discount": 58}
    /// }
    /// </summary>
    public string? ContractPricing { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Default contract term for new sales
    /// TECHNICAL: Pre-selected term in quote/order
    /// </summary>
    public ContractTermCategory DefaultContractTerm { get; set; } = ContractTermCategory.Monthly;
    
    /// <summary>
    /// FUNCTIONAL: Minimum contract term allowed
    /// TECHNICAL: Enforced in quote/order
    /// </summary>
    public ContractTermCategory MinimumContractTerm { get; set; } = ContractTermCategory.NoContract;
    
    #endregion
    
    #region Term-Based Discounts (Configurable)
    
    /// <summary>
    /// FUNCTIONAL: Discount % for weekly commitment
    /// TECHNICAL: Applied automatically based on term
    /// </summary>
    public decimal WeeklyTermDiscount { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Discount % for monthly commitment
    /// TECHNICAL: Applied automatically based on term
    /// </summary>
    public decimal MonthlyTermDiscount { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Discount % for quarterly commitment
    /// TECHNICAL: Applied automatically based on term
    /// </summary>
    public decimal QuarterlyTermDiscount { get; set; } = 5;
    
    /// <summary>
    /// FUNCTIONAL: Discount % for semi-annual commitment
    /// TECHNICAL: Applied automatically based on term
    /// </summary>
    public decimal SemiAnnualTermDiscount { get; set; } = 10;
    
    /// <summary>
    /// FUNCTIONAL: Discount % for annual commitment
    /// TECHNICAL: Applied automatically based on term
    /// </summary>
    public decimal AnnualTermDiscount { get; set; } = 15;
    
    /// <summary>
    /// FUNCTIONAL: Discount % for two-year commitment
    /// TECHNICAL: Applied automatically based on term
    /// </summary>
    public decimal TwoYearTermDiscount { get; set; } = 20;
    
    /// <summary>
    /// FUNCTIONAL: Discount % for three-year commitment
    /// TECHNICAL: Applied automatically based on term
    /// </summary>
    public decimal ThreeYearTermDiscount { get; set; } = 25;
    
    /// <summary>
    /// FUNCTIONAL: Comprehensive term discount matrix
    /// TECHNICAL: JSON array for flexible discount configuration:
    /// [
    ///   {"termMonths": 1, "discountPercent": 0, "label": "Monthly"},
    ///   {"termMonths": 3, "discountPercent": 5, "label": "Quarterly"},
    ///   {"termMonths": 6, "discountPercent": 10, "label": "Semi-Annual"},
    ///   {"termMonths": 12, "discountPercent": 15, "label": "Annual"},
    ///   {"termMonths": 24, "discountPercent": 20, "label": "2-Year"},
    ///   {"termMonths": 36, "discountPercent": 25, "label": "3-Year"},
    ///   {"termMonths": 60, "discountPercent": 30, "label": "5-Year"}
    /// ]
    /// </summary>
    public string? TermDiscounts { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Maximum term discount allowed
    /// TECHNICAL: Cap on term-based discounts
    /// </summary>
    public decimal MaxTermDiscount { get; set; } = 30;
    
    #endregion
    
    #region Volume Discounts
    
    /// <summary>
    /// FUNCTIONAL: Volume/quantity-based discount matrix
    /// TECHNICAL: JSON array for tiered discounts:
    /// [
    ///   {"minQuantity": 1, "maxQuantity": 9, "discountPercent": 0},
    ///   {"minQuantity": 10, "maxQuantity": 24, "discountPercent": 5},
    ///   {"minQuantity": 25, "maxQuantity": 49, "discountPercent": 10},
    ///   {"minQuantity": 50, "maxQuantity": 99, "discountPercent": 15},
    ///   {"minQuantity": 100, "maxQuantity": null, "discountPercent": 20}
    /// ]
    /// </summary>
    public string? VolumeDiscounts { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Tiered pricing structure
    /// TECHNICAL: JSON array for tier-based pricing:
    /// [
    ///   {"tier": "Starter", "minUsers": 1, "maxUsers": 10, "pricePerUser": 15},
    ///   {"tier": "Growth", "minUsers": 11, "maxUsers": 50, "pricePerUser": 12},
    ///   {"tier": "Business", "minUsers": 51, "maxUsers": 200, "pricePerUser": 10},
    ///   {"tier": "Enterprise", "minUsers": 201, "maxUsers": null, "pricePerUser": 8}
    /// ]
    /// </summary>
    public string? PricingTiers { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Maximum volume discount allowed
    /// TECHNICAL: Cap on volume-based discounts
    /// </summary>
    public decimal MaxVolumeDiscount { get; set; } = 25;
    
    /// <summary>
    /// FUNCTIONAL: Combined max discount (term + volume)
    /// TECHNICAL: Total discount cannot exceed this
    /// </summary>
    public decimal MaxTotalDiscount { get; set; } = 40;
    
    #endregion
    
    #region Subscription & Recurring Pricing
    
    /// <summary>
    /// FUNCTIONAL: Billing cycle frequency
    /// TECHNICAL: Determines billing automation
    /// </summary>
    public BillingFrequency BillingFrequency { get; set; } = BillingFrequency.OneTime;
    
    /// <summary>
    /// FUNCTIONAL: Pricing model type
    /// TECHNICAL: Determines calculation method
    /// </summary>
    public PricingModel PricingModel { get; set; } = PricingModel.FixedPrice;
    
    /// <summary>
    /// FUNCTIONAL: Recurring charge amount
    /// TECHNICAL: Charged each billing cycle
    /// </summary>
    public decimal? RecurringPrice { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: One-time setup/activation fee
    /// TECHNICAL: Charged on initial purchase
    /// </summary>
    public decimal? SetupFee { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Activation/onboarding fee
    /// TECHNICAL: Separate from setup if applicable
    /// </summary>
    public decimal? ActivationFee { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Cancellation/early termination fee
    /// TECHNICAL: Applied on early cancellation
    /// </summary>
    public decimal? CancellationFee { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Free trial period length
    /// TECHNICAL: Days before billing starts
    /// </summary>
    public int? TrialPeriodDays { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Standard contract length
    /// TECHNICAL: In months
    /// </summary>
    public int? ContractLengthMonths { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Minimum contract length
    /// TECHNICAL: In months, enforced in ordering
    /// </summary>
    public int? MinContractLengthMonths { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Billing cycle day of month
    /// TECHNICAL: 1-31 for monthly billing
    /// </summary>
    public int? BillingDayOfMonth { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Auto-renewal enabled
    /// TECHNICAL: Continues after term expires
    /// </summary>
    public bool AutoRenewal { get; set; } = true;
    
    /// <summary>
    /// FUNCTIONAL: Renewal price (if different from current)
    /// TECHNICAL: Applied on renewal
    /// </summary>
    public decimal? RenewalPrice { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Price increase cap on renewal (%)
    /// TECHNICAL: Maximum annual price increase
    /// </summary>
    public decimal? RenewalPriceIncreaseCapPercent { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Notice period for cancellation (days)
    /// TECHNICAL: Required advance notice
    /// </summary>
    public int? CancellationNoticeDays { get; set; }
    
    #endregion
    
    #region Service-Specific Fields
    
    /// <summary>
    /// FUNCTIONAL: Hourly rate for services
    /// TECHNICAL: Base hourly billing rate
    /// </summary>
    public decimal? HourlyRate { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Daily rate for services
    /// TECHNICAL: Base daily billing rate
    /// </summary>
    public decimal? DailyRate { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Minimum billable hours
    /// TECHNICAL: Floor for time-based billing
    /// </summary>
    public decimal? MinimumBillableHours { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Billable hour increment (e.g., 0.25 = 15 min)
    /// TECHNICAL: Rounding for time tracking
    /// </summary>
    public decimal BillableHourIncrement { get; set; } = 0.25m;
    
    /// <summary>
    /// FUNCTIONAL: Overtime rate multiplier
    /// TECHNICAL: Applied for work outside normal hours
    /// </summary>
    public decimal? OvertimeMultiplier { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Weekend rate multiplier
    /// TECHNICAL: Applied for weekend work
    /// </summary>
    public decimal? WeekendMultiplier { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Holiday rate multiplier
    /// TECHNICAL: Applied for holiday work
    /// </summary>
    public decimal? HolidayMultiplier { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Service includes on-site work
    /// TECHNICAL: For travel/expense planning
    /// </summary>
    public bool IncludesOnsiteWork { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Travel expenses included in price
    /// TECHNICAL: Or billed separately
    /// </summary>
    public bool TravelIncluded { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Materials/expenses included
    /// TECHNICAL: Or billed separately
    /// </summary>
    public bool MaterialsIncluded { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Service delivery method
    /// TECHNICAL: Remote, On-site, Hybrid
    /// </summary>
    public string? DeliveryMethod { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Estimated service duration
    /// TECHNICAL: In hours or days
    /// </summary>
    public decimal? EstimatedDuration { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Duration unit (Hours, Days, Weeks)
    /// TECHNICAL: For duration display
    /// </summary>
    public string? DurationUnit { get; set; }
    
    #endregion
    
    #region SLA & Service Levels
    
    /// <summary>
    /// FUNCTIONAL: Service level agreement details
    /// TECHNICAL: JSON object with SLA terms:
    /// {
    ///   "responseTime": "4 hours",
    ///   "resolutionTime": "24 hours",
    ///   "uptime": "99.9%",
    ///   "supportHours": "24/7",
    ///   "supportChannels": ["email", "phone", "chat"]
    /// }
    /// </summary>
    public string? SlaDetails { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Guaranteed uptime percentage
    /// TECHNICAL: For SaaS/hosting services
    /// </summary>
    public decimal? UptimeGuaranteePercent { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Response time SLA (hours)
    /// TECHNICAL: Maximum response time
    /// </summary>
    public decimal? ResponseTimeSlaHours { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Resolution time SLA (hours)
    /// TECHNICAL: Maximum resolution time
    /// </summary>
    public decimal? ResolutionTimeSlaHours { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Support hours description
    /// TECHNICAL: e.g., "24/7", "Business Hours"
    /// </summary>
    public string? SupportHours { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Available support channels
    /// TECHNICAL: JSON array: ["email", "phone", "chat", "portal"]
    /// </summary>
    public string? SupportChannels { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Number of support incidents included
    /// TECHNICAL: Per month/year, null = unlimited
    /// </summary>
    public int? IncludedSupportIncidents { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Price per additional support incident
    /// TECHNICAL: Overage billing
    /// </summary>
    public decimal? AdditionalIncidentPrice { get; set; }
    
    #endregion
    
    #region Usage Limits & Metering
    
    /// <summary>
    /// FUNCTIONAL: Usage-based pricing configuration
    /// TECHNICAL: JSON object for metered billing:
    /// {
    ///   "includedUnits": 1000,
    ///   "unitType": "API calls",
    ///   "overagePrice": 0.01,
    ///   "overageTiers": [
    ///     {"min": 1001, "max": 10000, "price": 0.008},
    ///     {"min": 10001, "max": null, "price": 0.005}
    ///   ]
    /// }
    /// </summary>
    public string? UsagePricing { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Included usage units per period
    /// TECHNICAL: Before overage charges apply
    /// </summary>
    public int? IncludedUnits { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Type of usage unit
    /// TECHNICAL: e.g., "API calls", "GB storage"
    /// </summary>
    public string? UsageUnitType { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Price per additional unit
    /// TECHNICAL: Overage pricing
    /// </summary>
    public decimal? OverageUnitPrice { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Maximum users/seats included
    /// TECHNICAL: For per-seat licensing
    /// </summary>
    public int? IncludedUsers { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Price per additional user/seat
    /// TECHNICAL: For per-seat overage
    /// </summary>
    public decimal? AdditionalUserPrice { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Maximum storage included (GB)
    /// TECHNICAL: For storage-based services
    /// </summary>
    public decimal? IncludedStorageGb { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Price per additional GB
    /// TECHNICAL: Storage overage pricing
    /// </summary>
    public decimal? AdditionalStoragePrice { get; set; }
    
    #endregion
    
    #region Tax & Currency
    
    /// <summary>
    /// FUNCTIONAL: Currency code for all prices
    /// TECHNICAL: ISO 4217 (USD, EUR, GBP, etc.)
    /// </summary>
    public string? CurrencyCode { get; set; } = "USD";
    
    /// <summary>
    /// FUNCTIONAL: Whether tax applies to this item
    /// TECHNICAL: For tax calculation
    /// </summary>
    public bool IsTaxable { get; set; } = true;
    
    /// <summary>
    /// FUNCTIONAL: Default tax rate (%)
    /// TECHNICAL: May be overridden by jurisdiction
    /// </summary>
    public decimal? TaxRate { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Tax category/class
    /// TECHNICAL: For tax rule application
    /// </summary>
    public string? TaxCategory { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Tax exemption code if applicable
    /// TECHNICAL: For exempt items
    /// </summary>
    public string? TaxExemptionCode { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Revenue recognition method
    /// TECHNICAL: For accounting compliance
    /// </summary>
    public RevenueRecognitionMethod RevenueRecognition { get; set; } = RevenueRecognitionMethod.Immediate;
    
    /// <summary>
    /// FUNCTIONAL: GL account code for revenue
    /// TECHNICAL: Accounting integration
    /// </summary>
    public string? RevenueAccountCode { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Deferred revenue account code
    /// TECHNICAL: For subscription revenue
    /// </summary>
    public string? DeferredRevenueAccountCode { get; set; }
    
    #endregion
    
    #region Inventory (Products Only)
    
    /// <summary>
    /// FUNCTIONAL: Current stock quantity
    /// TECHNICAL: Updated on sale/receipt
    /// </summary>
    public int Quantity { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Reorder trigger level
    /// TECHNICAL: Triggers reorder alert/workflow
    /// </summary>
    public int? ReorderLevel { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Standard reorder quantity
    /// TECHNICAL: Default PO quantity
    /// </summary>
    public int? ReorderQuantity { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Maximum inventory level
    /// TECHNICAL: Storage capacity limit
    /// </summary>
    public int? MaxQuantity { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Quantity reserved for orders
    /// TECHNICAL: Committed but not shipped
    /// </summary>
    public int? ReservedQuantity { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Available for new orders
    /// TECHNICAL: Quantity - ReservedQuantity
    /// </summary>
    public int? AvailableQuantity { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Storage location identifier
    /// TECHNICAL: Warehouse/bin location
    /// </summary>
    public string? WarehouseLocation { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Enable inventory tracking
    /// TECHNICAL: False for services
    /// </summary>
    public bool TrackInventory { get; set; } = true;
    
    /// <summary>
    /// FUNCTIONAL: Allow orders when out of stock
    /// TECHNICAL: Backorder capability
    /// </summary>
    public bool AllowBackorder { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Lead time for restocking (days)
    /// TECHNICAL: Used for availability estimates
    /// </summary>
    public int? LeadTimeDays { get; set; }
    
    #endregion
    
    #region Physical Attributes (Products Only)
    
    /// <summary>
    /// FUNCTIONAL: Product weight
    /// TECHNICAL: For shipping calculations
    /// </summary>
    public decimal? Weight { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Weight unit of measure
    /// TECHNICAL: kg, lb, oz, g
    /// </summary>
    public string? WeightUnit { get; set; } = "kg";
    
    /// <summary>
    /// FUNCTIONAL: Package length
    /// TECHNICAL: For shipping calculations
    /// </summary>
    public decimal? Length { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Package width
    /// TECHNICAL: For shipping calculations
    /// </summary>
    public decimal? Width { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Package height
    /// TECHNICAL: For shipping calculations
    /// </summary>
    public decimal? Height { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Dimension unit of measure
    /// TECHNICAL: cm, in, m
    /// </summary>
    public string? DimensionUnit { get; set; } = "cm";
    
    /// <summary>
    /// FUNCTIONAL: Shipping class/category
    /// TECHNICAL: For carrier rate lookup
    /// </summary>
    public string? ShippingClass { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Is hazardous material
    /// TECHNICAL: Special handling required
    /// </summary>
    public bool IsHazardous { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Requires special handling
    /// TECHNICAL: Fragile, temperature-sensitive, etc.
    /// </summary>
    public string? SpecialHandling { get; set; }
    
    #endregion
    
    #region Media & Documentation
    
    /// <summary>
    /// FUNCTIONAL: Primary product image URL
    /// TECHNICAL: Main display image
    /// </summary>
    public string ImageUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// FUNCTIONAL: Thumbnail image URL
    /// TECHNICAL: For lists and search results
    /// </summary>
    public string? ThumbnailUrl { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Additional product images
    /// TECHNICAL: JSON array of URLs
    /// </summary>
    public string? AdditionalImages { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Product video URL
    /// TECHNICAL: Demo or marketing video
    /// </summary>
    public string? VideoUrl { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Related documents
    /// TECHNICAL: JSON array of {name, url, type}
    /// </summary>
    public string? DocumentUrls { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Datasheet/specification PDF
    /// TECHNICAL: Technical documentation
    /// </summary>
    public string? DatasheetUrl { get; set; }
    
    #endregion
    
    #region SEO & Marketing
    
    /// <summary>
    /// FUNCTIONAL: SEO page title
    /// TECHNICAL: HTML title tag
    /// </summary>
    public string? MetaTitle { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: SEO meta description
    /// TECHNICAL: HTML meta description
    /// </summary>
    public string? MetaDescription { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: SEO keywords
    /// TECHNICAL: Comma-separated
    /// </summary>
    public string? MetaKeywords { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: URL-friendly identifier
    /// TECHNICAL: For e-commerce URLs
    /// </summary>
    public string? Slug { get; set; }
    
    #endregion
    
    #region Relationships
    
    /// <summary>
    /// FUNCTIONAL: Parent product for variants
    /// TECHNICAL: Self-referencing FK
    /// </summary>
    public int? ParentProductId { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Vendor/supplier ID
    /// TECHNICAL: FK to vendor entity
    /// </summary>
    public int? VendorId { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Product family grouping
    /// TECHNICAL: FK to product family
    /// </summary>
    public int? ProductFamilyId { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Bundle component products
    /// TECHNICAL: JSON array of {productId, quantity, price}
    /// </summary>
    public string? BundleComponents { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Related/recommended products
    /// TECHNICAL: JSON array of product IDs
    /// </summary>
    public string? RelatedProducts { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Upsell products
    /// TECHNICAL: JSON array of product IDs
    /// </summary>
    public string? UpsellProducts { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Cross-sell products
    /// TECHNICAL: JSON array of product IDs
    /// </summary>
    public string? CrossSellProducts { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Required add-on products
    /// TECHNICAL: Must be purchased together
    /// </summary>
    public string? RequiredAddons { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Optional add-on products
    /// TECHNICAL: Suggested additions
    /// </summary>
    public string? OptionalAddons { get; set; }
    
    #endregion
    
    #region Features & Specifications
    
    /// <summary>
    /// FUNCTIONAL: Key features list
    /// TECHNICAL: JSON array of feature strings
    /// </summary>
    public string? Features { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Technical specifications
    /// TECHNICAL: JSON object of key-value specs
    /// </summary>
    public string? Specifications { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Warranty information
    /// TECHNICAL: Warranty terms text
    /// </summary>
    public string? Warranty { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Warranty duration (months)
    /// TECHNICAL: For warranty tracking
    /// </summary>
    public int? WarrantyMonths { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Extended warranty available
    /// TECHNICAL: Upsell opportunity
    /// </summary>
    public bool ExtendedWarrantyAvailable { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Extended warranty price
    /// TECHNICAL: Per year or total
    /// </summary>
    public decimal? ExtendedWarrantyPrice { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Support information
    /// TECHNICAL: How to get support
    /// </summary>
    public string? SupportInfo { get; set; }
    
    #endregion
    
    #region Sales Performance
    
    /// <summary>
    /// FUNCTIONAL: Total units sold (lifetime)
    /// TECHNICAL: Aggregate counter
    /// </summary>
    public int TotalSold { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Total revenue generated
    /// TECHNICAL: Lifetime revenue
    /// </summary>
    public decimal TotalRevenue { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Average customer rating
    /// TECHNICAL: 0-5 scale
    /// </summary>
    public double AverageRating { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Number of reviews
    /// TECHNICAL: Review count
    /// </summary>
    public int ReviewCount { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Featured product flag
    /// TECHNICAL: For homepage/promotions
    /// </summary>
    public bool IsFeatured { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Best seller flag
    /// TECHNICAL: High volume product
    /// </summary>
    public bool IsBestSeller { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: New product flag
    /// TECHNICAL: Recently added
    /// </summary>
    public bool IsNew { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: On sale flag
    /// TECHNICAL: Currently discounted
    /// </summary>
    public bool IsOnSale { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Sale price
    /// TECHNICAL: Temporary discounted price
    /// </summary>
    public decimal? SalePrice { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Sale start date
    /// TECHNICAL: When sale begins
    /// </summary>
    public DateTime? SaleStartDate { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Sale end date
    /// TECHNICAL: When sale ends
    /// </summary>
    public DateTime? SaleEndDate { get; set; }
    
    #endregion
    
    #region Availability Dates
    
    /// <summary>
    /// FUNCTIONAL: Available for sale from
    /// TECHNICAL: Launch date
    /// </summary>
    public DateTime? AvailableFrom { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Available for sale until
    /// TECHNICAL: End of life date
    /// </summary>
    public DateTime? AvailableTo { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Discontinued date
    /// TECHNICAL: When discontinued
    /// </summary>
    public DateTime? DiscontinuedDate { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Last price update date
    /// TECHNICAL: For price tracking
    /// </summary>
    public DateTime? LastPriceUpdate { get; set; }
    
    #endregion
    
    #region Legacy & Status
    
    /// <summary>
    /// FUNCTIONAL: Active status flag
    /// TECHNICAL: Legacy compatibility
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// FUNCTIONAL: Visible in catalog
    /// TECHNICAL: Can be hidden but still orderable
    /// </summary>
    public bool IsVisible { get; set; } = true;
    
    /// <summary>
    /// FUNCTIONAL: Available for purchase
    /// TECHNICAL: Can be added to cart
    /// </summary>
    public bool IsPurchasable { get; set; } = true;
    
    #endregion
    
    #region Custom Fields & Integration
    
    /// <summary>
    /// FUNCTIONAL: Custom fields for extensibility
    /// TECHNICAL: JSON object
    /// </summary>
    public string? CustomFields { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: External system sync status
    /// TECHNICAL: Integration tracking
    /// </summary>
    public string? SyncStatus { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Last sync timestamp
    /// TECHNICAL: Integration tracking
    /// </summary>
    public DateTime? LastSyncDate { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: External system IDs
    /// TECHNICAL: JSON {system: id}
    /// </summary>
    public string? ExternalIds { get; set; }
    
    #endregion

    #region Navigation Properties
    
    /// <summary>Related opportunities</summary>
    public ICollection<Opportunity>? Opportunities { get; set; }
    
    /// <summary>Accounts associated with this product/service</summary>
    public ICollection<Account>? Accounts { get; set; }
    
    /// <summary>Associated marketing campaigns</summary>
    public ICollection<MarketingCampaign>? MarketingCampaigns { get; set; }
    
    /// <summary>Parent product for variants</summary>
    public Product? ParentProduct { get; set; }
    
    /// <summary>Product variants</summary>
    public ICollection<Product>? Variants { get; set; }
    
    #endregion
    
    #region Calculated Properties
    
    /// <summary>
    /// FUNCTIONAL: Whether this is a physical product
    /// TECHNICAL: Requires inventory/shipping
    /// </summary>
    public bool IsPhysical => ProductType == ProductType.Physical || ProductType == ProductType.Rental;
    
    /// <summary>
    /// FUNCTIONAL: Whether this requires recurring billing
    /// TECHNICAL: Based on billing frequency
    /// </summary>
    public bool IsRecurring => BillingFrequency != BillingFrequency.OneTime;
    
    /// <summary>
    /// FUNCTIONAL: Effective price considering sales
    /// TECHNICAL: Returns SalePrice if on sale, otherwise Price
    /// </summary>
    public decimal EffectivePrice => IsOnSale && SalePrice.HasValue ? SalePrice.Value : Price;
    
    /// <summary>
    /// FUNCTIONAL: Profit margin percentage
    /// TECHNICAL: (Price - Cost) / Price × 100
    /// </summary>
    public decimal CalculatedMargin => Price > 0 ? ((Price - Cost) / Price) * 100 : 0;
    
    /// <summary>
    /// FUNCTIONAL: Is currently on sale
    /// TECHNICAL: Based on sale dates and flag
    /// </summary>
    public bool IsCurrentlyOnSale => IsOnSale && 
        (!SaleStartDate.HasValue || SaleStartDate <= DateTime.UtcNow) &&
        (!SaleEndDate.HasValue || SaleEndDate >= DateTime.UtcNow);
    
    #endregion
}
