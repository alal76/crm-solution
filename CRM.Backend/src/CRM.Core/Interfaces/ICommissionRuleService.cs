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
    Task<CommissionCalculationDto> CalculateCommissionAsync(
        decimal saleAmount,
        string saleType,
        CancellationToken ct = default);
}
