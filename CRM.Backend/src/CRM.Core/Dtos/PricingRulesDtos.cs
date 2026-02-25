// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Dtos;

/// <summary>
/// Request object for an ad-hoc price calculation (TODO-GAP-07).
/// </summary>
public class PriceCalculationRequest
{
    /// <summary>Product to price</summary>
    public int ProductId { get; set; }

    /// <summary>Quantity being purchased</summary>
    public int Quantity { get; set; } = 1;

    /// <summary>Optional customer/account ID for customer-specific pricing</summary>
    public int? CustomerId { get; set; }

    /// <summary>Optional explicit price book ID</summary>
    public int? PriceBookId { get; set; }

    /// <summary>Optional promotional code to apply</summary>
    public string? PromoCode { get; set; }
}

/// <summary>
/// Full price breakdown returned by the pricing rules engine (TODO-GAP-07).
/// Shows base price, discounts, applied rules and final price.
/// </summary>
public class PriceBreakdownDto
{
    /// <summary>Product ID</summary>
    public int ProductId { get; set; }

    /// <summary>Quantity priced</summary>
    public int Quantity { get; set; }

    /// <summary>Original list price per unit</summary>
    public decimal BasePrice { get; set; }

    /// <summary>Unit price before promo discount, after volume/customer rules</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>Total discount per unit (all rules combined)</summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>Discount as a percentage of base price</summary>
    public decimal DiscountPercent { get; set; }

    /// <summary>Primary discount type applied (e.g. "VolumeDiscount", "Promotional")</summary>
    public string DiscountType { get; set; } = string.Empty;

    /// <summary>Final per-unit price after all discounts</summary>
    public decimal FinalPrice { get; set; }

    /// <summary>Extended price  =  FinalPrice × Quantity</summary>
    public decimal ExtendedPrice { get; set; }

    /// <summary>Currency code (ISO 4217)</summary>
    public string Currency { get; set; } = "USD";

    /// <summary>Promo code that was applied (null if none)</summary>
    public string? PromoCodeApplied { get; set; }

    /// <summary>Individual contribution of each applied rule</summary>
    public List<AppliedRuleSummary> AppliedRules { get; set; } = new();
}

/// <summary>
/// Summary of a single pricing rule that was applied during calculation.
/// </summary>
public class AppliedRuleSummary
{
    /// <summary>Pricing rule ID</summary>
    public int RuleId { get; set; }

    /// <summary>Human-readable rule name</summary>
    public string RuleName { get; set; } = string.Empty;

    /// <summary>Rule type string (e.g. "VolumeDiscount", "Promotional")</summary>
    public string RuleType { get; set; } = string.Empty;

    /// <summary>Discount amount this rule contributed</summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>Short description of why the rule was applied</summary>
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// DTO for creating a new pricing rule.
/// </summary>
public class CreatePricingRuleDto
{
    /// <summary>Rule name</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Rule description</summary>
    public string? Description { get; set; }

    /// <summary>Rule type (PricingRuleType enum value)</summary>
    public int RuleType { get; set; }

    /// <summary>Whether rule is active</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Priority (lower number = evaluated first)</summary>
    public int Priority { get; set; } = 100;

    /// <summary>Apply to all products</summary>
    public bool AppliesToAllProducts { get; set; } = false;

    /// <summary>Comma-separated product IDs</summary>
    public string? ProductIds { get; set; }

    /// <summary>Comma-separated product categories</summary>
    public string? ProductCategories { get; set; }

    /// <summary>Comma-separated account/customer IDs (null = all)</summary>
    public string? AccountIds { get; set; }

    /// <summary>Customer segments (comma-separated)</summary>
    public string? CustomerSegments { get; set; }

    /// <summary>Discount method (DiscountMethod enum value)</summary>
    public int DiscountMethod { get; set; }

    /// <summary>Discount value (percent or amount)</summary>
    public decimal? DiscountValue { get; set; }

    /// <summary>Fixed price override</summary>
    public decimal? FixedPrice { get; set; }

    /// <summary>Minimum quantity threshold</summary>
    public decimal? MinQuantity { get; set; }

    /// <summary>Maximum discount cap</summary>
    public decimal? MaxDiscountAmount { get; set; }

    /// <summary>Volume tier JSON</summary>
    public string? VolumeTiers { get; set; }

    /// <summary>Rule effective start date</summary>
    public DateTime? EffectiveStartDate { get; set; }

    /// <summary>Rule effective end date</summary>
    public DateTime? EffectiveEndDate { get; set; }

    /// <summary>Usage limit (null = unlimited)</summary>
    public int? UsageLimit { get; set; }

    /// <summary>Conditions JSON (used for promo codes: {"promoCode":"CODE"})</summary>
    public string? Conditions { get; set; }

    /// <summary>Whether rule can combine with others</summary>
    public bool CombineWithOtherRules { get; set; } = true;
}

/// <summary>
/// DTO for updating an existing pricing rule (extends create DTO with Id).
/// </summary>
public class UpdatePricingRuleDto : CreatePricingRuleDto
{
    /// <summary>Rule ID to update</summary>
    public int Id { get; set; }
}
