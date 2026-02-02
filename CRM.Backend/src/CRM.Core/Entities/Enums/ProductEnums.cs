// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under AGPL-3.0

namespace CRM.Core.Entities;

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
