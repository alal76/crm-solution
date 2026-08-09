// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for managing discount rules and calculations
/// </summary>
[Obsolete("Superseded by IPricingRulesService/IDynamicPricingEngine, which already cover discount-type pricing (PricingRule.DiscountMethod/DiscountValue/MinOrderAmount/MinQuantity/MaxDiscountAmount/ProductIds/CustomerSegments/Conditions/CombineWithOtherRules is a strict superset of DiscountRule's fields) alongside volume/promo/customer/tiered pricing. Do not use in new code.")]
public interface IDiscountRuleService
{
    /// <summary>Creates a new discount rule</summary>
    Task<DiscountRuleDto> CreateAsync(CreateDiscountRuleDto dto, CancellationToken ct = default);

    /// <summary>Updates an existing discount rule</summary>
    Task<DiscountRuleDto> UpdateAsync(int id, UpdateDiscountRuleDto dto, CancellationToken ct = default);

    /// <summary>Gets a discount rule by ID</summary>
    Task<DiscountRuleDto?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Gets all discount rules</summary>
    Task<List<DiscountRuleDto>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Deletes a discount rule (soft delete)</summary>
    Task DeleteAsync(int id, CancellationToken ct = default);

    /// <summary>Gets applicable discount rules for given criteria</summary>
    Task<List<DiscountRuleDto>> GetApplicableRulesAsync(
        int accountId,
        int? productId,
        decimal orderAmount,
        CancellationToken ct = default);

    /// <summary>Calculates discount(s) for an order</summary>
    Task<DiscountCalculationDto> CalculateDiscountAsync(
        int accountId,
        int? productId,
        decimal orderAmount,
        CancellationToken ct = default);
}
