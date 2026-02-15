// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for managing discount rules and calculations
/// </summary>
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
