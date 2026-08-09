// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for managing commission rules and calculations
/// </summary>
public interface ICommissionRuleService
{
    /// <summary>Creates a new commission rule</summary>
    Task<CommissionRuleDto> CreateAsync(CreateCommissionRuleDto dto, CancellationToken ct = default);

    /// <summary>Updates an existing commission rule</summary>
    Task<CommissionRuleDto> UpdateAsync(int id, UpdateCommissionRuleDto dto, CancellationToken ct = default);

    /// <summary>Gets a commission rule by ID</summary>
    Task<CommissionRuleDto?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Gets all commission rules</summary>
    Task<List<CommissionRuleDto>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Deletes a commission rule (soft delete)</summary>
    Task DeleteAsync(int id, CancellationToken ct = default);

    /// <summary>Gets applicable commission rules for a sale type</summary>
    Task<List<CommissionRuleDto>> GetApplicableRulesAsync(string saleType, CancellationToken ct = default);

    /// <summary>Calculates commission for a given sale amount and type</summary>
    [Obsolete("Superseded by ICommissionRulesEngine.CalculateCommissionAsync, which supports tiered rates, caps, team splits, and trigger-event gating instead of a flat rate/tier lookup by sale type. Do not use in new code.")]
    Task<CommissionCalculationDto> CalculateCommissionAsync(
        decimal saleAmount,
        string saleType,
        CancellationToken ct = default);
}
