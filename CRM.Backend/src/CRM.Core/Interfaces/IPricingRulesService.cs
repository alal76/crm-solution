// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Dynamic Pricing Rules Service (TODO-GAP-07).
/// Provides a high-level API for price calculation with volume discounts,
/// customer-specific pricing, promotional codes, and time-based rules.
/// </summary>
public interface IPricingRulesService
{
    /// <summary>
    /// Calculates the final unit price for a product.
    /// </summary>
    /// <param name="productId">Product to price</param>
    /// <param name="quantity">Quantity being purchased</param>
    /// <param name="customerId">Optional customer/account ID for customer-specific pricing</param>
    /// <param name="promoCode">Optional promotional code to apply</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Final calculated unit price</returns>
    Task<decimal> CalculatePriceAsync(
        int productId,
        int quantity,
        int? customerId,
        string? promoCode,
        CancellationToken ct = default);

    /// <summary>
    /// Returns a full price breakdown showing base price, discounts, and applied rules.
    /// </summary>
    /// <param name="request">Price calculation request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Detailed price breakdown</returns>
    Task<PriceBreakdownDto> GetPriceBreakdownAsync(
        PriceCalculationRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Returns all currently active pricing rules.
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of active pricing rules</returns>
    Task<List<PricingRule>> GetActiveRulesAsync(CancellationToken ct = default);

    /// <summary>
    /// Returns a single pricing rule by ID.
    /// </summary>
    /// <param name="id">Rule ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Pricing rule or null if not found</returns>
    Task<PricingRule?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Creates a new pricing rule.
    /// </summary>
    /// <param name="dto">Rule creation data</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Created pricing rule</returns>
    Task<PricingRule> CreateRuleAsync(CreatePricingRuleDto dto, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing pricing rule.
    /// </summary>
    /// <param name="id">Rule ID</param>
    /// <param name="dto">Updated rule data</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated pricing rule</returns>
    Task<PricingRule?> UpdateRuleAsync(int id, UpdatePricingRuleDto dto, CancellationToken ct = default);

    /// <summary>
    /// Soft-deletes a pricing rule.
    /// </summary>
    /// <param name="id">Rule ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>True if deleted, false if not found</returns>
    Task<bool> DeleteRuleAsync(int id, CancellationToken ct = default);
}
