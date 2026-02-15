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

using CRM.Core.Dtos.ITSM;

namespace CRM.Core.Interfaces.ITSM;

/// <summary>
/// Admin service interface for managing escalation rules
/// </summary>
public interface IEscalationRuleAdminService
{
    /// <summary>Creates a new escalation rule</summary>
    Task<EscalationRuleDto> CreateAsync(CreateEscalationRuleDto dto, CancellationToken ct = default);

    /// <summary>Updates an existing escalation rule</summary>
    Task<EscalationRuleDto> UpdateAsync(int id, UpdateEscalationRuleDto dto, CancellationToken ct = default);

    /// <summary>Gets an escalation rule by ID</summary>
    Task<EscalationRuleDto?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Gets all escalation rules</summary>
    Task<List<EscalationRuleDto>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Deletes an escalation rule (soft delete)</summary>
    Task DeleteAsync(int id, CancellationToken ct = default);

    /// <summary>Tests if a rule would apply to a service request</summary>
    Task<EscalationRuleTestResultDto> TestRuleAsync(
        int ruleId,
        int serviceRequestId,
        CancellationToken ct = default);

    /// <summary>Gets applicable escalation rules for given priority</summary>
    Task<List<EscalationRuleDto>> GetApplicableRulesAsync(
        string priority,
        CancellationToken ct = default);
}
