// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos.ITSM;

namespace CRM.Core.Interfaces.ITSM;

/// <summary>
/// Service interface for automatic service request assignment using configurable strategies
/// (round-robin, skill-based, and least-loaded).
/// </summary>
public interface IAutoAssignmentService
{
    /// <summary>Auto-assign a service request based on configured rules</summary>
    Task<AutoAssignmentResultDto> AssignServiceRequestAsync(int serviceRequestId, CancellationToken ct = default);

    /// <summary>Suggest an agent without actually assigning</summary>
    Task<AutoAssignmentResultDto> SuggestAssignmentAsync(int serviceRequestId, CancellationToken ct = default);

    /// <summary>Get all assignment rules</summary>
    Task<IEnumerable<AssignmentRuleDto>> GetRulesAsync(CancellationToken ct = default);

    /// <summary>Get an assignment rule by ID</summary>
    Task<AssignmentRuleDto?> GetRuleByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Create a new assignment rule</summary>
    Task<AssignmentRuleDto> CreateRuleAsync(CreateAssignmentRuleDto dto, CancellationToken ct = default);

    /// <summary>Update an assignment rule</summary>
    Task<AssignmentRuleDto?> UpdateRuleAsync(int id, UpdateAssignmentRuleDto dto, CancellationToken ct = default);

    /// <summary>Delete an assignment rule</summary>
    Task<bool> DeleteRuleAsync(int id, CancellationToken ct = default);

    /// <summary>Get next available agent using round-robin</summary>
    Task<int?> GetNextRoundRobinAgentAsync(int? queueId = null, CancellationToken ct = default);

    /// <summary>Get best agent based on skills and availability</summary>
    Task<int?> GetBestSkillMatchAgentAsync(int serviceRequestId, CancellationToken ct = default);

    /// <summary>Get least-loaded agent</summary>
    Task<int?> GetLeastLoadedAgentAsync(int? queueId = null, CancellationToken ct = default);
}
