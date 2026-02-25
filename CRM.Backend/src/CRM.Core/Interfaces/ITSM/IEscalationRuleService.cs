// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos.ITSM;

namespace CRM.Core.Interfaces.ITSM;

/// <summary>
/// Primary service interface for managing escalation rules (admin CRUD operations).
/// Replaces IEscalationRuleAdminService — use this going forward.
/// TODO-SD005-003: IEscalationRuleAdminService renamed to IEscalationRuleService.
/// </summary>
public interface IEscalationRuleService
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
