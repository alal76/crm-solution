// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Dynamic Pricing Engine Interface (TODO-GAP-07)
/// Calculates prices based on pricing rules, volume discounts, and customer segments.
/// </summary>
public interface IDynamicPricingEngine
{
    /// <summary>
    /// Calculates the final price for a product given context.
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="quantity">Quantity being purchased</param>
    /// <param name="accountId">Optional account/customer ID for customer-specific pricing</param>
    /// <param name="priceBookId">Optional price book ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Calculated price result</returns>
    Task<PriceCalculationResult> CalculatePriceAsync(
        int productId,
        int quantity,
        int? accountId = null,
        int? priceBookId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates prices for multiple line items (quote/order pricing).
    /// </summary>
    /// <param name="lineItems">Line items to price</param>
    /// <param name="accountId">Customer account ID</param>
    /// <param name="priceBookId">Price book to use</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<List<PriceCalculationResult>> CalculateBatchPricesAsync(
        IEnumerable<PriceLineItem> lineItems,
        int? accountId = null,
        int? priceBookId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets applicable pricing rules for a product/customer combination.
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="accountId">Account ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<IEnumerable<PricingRule>> GetApplicableRulesAsync(int productId, int? accountId = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Input line item for batch pricing.
/// </summary>
public class PriceLineItem
{
    /// <summary>Product ID to price</summary>
    public int ProductId { get; set; }

    /// <summary>Quantity</summary>
    public int Quantity { get; set; }

    /// <summary>Optional override base price</summary>
    public decimal? BasePrice { get; set; }
}

/// <summary>
/// Result of a price calculation.
/// </summary>
public class PriceCalculationResult
{
    /// <summary>Product ID</summary>
    public int ProductId { get; set; }

    /// <summary>Quantity priced</summary>
    public int Quantity { get; set; }

    /// <summary>Original list price per unit</summary>
    public decimal ListPrice { get; set; }

    /// <summary>Unit price after discounts</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>Total discount amount per unit</summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>Discount percentage applied</summary>
    public decimal DiscountPercent { get; set; }

    /// <summary>Extended price (UnitPrice * Quantity)</summary>
    public decimal ExtendedPrice { get; set; }

    /// <summary>Pricing rules applied</summary>
    public List<AppliedPricingRule> AppliedRules { get; set; } = new();

    /// <summary>Whether pricing was successful</summary>
    public bool Success { get; set; } = true;

    /// <summary>Error message if pricing failed</summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Information about a pricing rule that was applied.
/// </summary>
public class AppliedPricingRule
{
    /// <summary>Rule ID</summary>
    public int RuleId { get; set; }

    /// <summary>Rule name</summary>
    public string RuleName { get; set; } = string.Empty;

    /// <summary>Rule type</summary>
    public string RuleType { get; set; } = string.Empty;

    /// <summary>Discount/adjustment amount this rule contributed</summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>Priority order</summary>
    public int Priority { get; set; }
}
