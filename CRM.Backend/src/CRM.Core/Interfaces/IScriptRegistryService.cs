// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Entities.AI;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service responsible for script lifecycle state-machine transitions and audit
/// history. Implements the Draft → Review → Approved → Deployed → Retired flow.
/// </summary>
public interface IScriptRegistryService
{
    Task<bool> SubmitForReviewAsync(int scriptId, string performedBy, string? notes, CancellationToken ct = default);
    Task<bool> ApproveAsync(int scriptId, string performedBy, string? notes, CancellationToken ct = default);
    Task<bool> RejectAsync(int scriptId, string performedBy, string? notes, CancellationToken ct = default);
    Task<bool> DeployAsync(int scriptId, string performedBy, CancellationToken ct = default);
    Task<bool> RetireAsync(int scriptId, string performedBy, string? notes, CancellationToken ct = default);
    Task<IReadOnlyList<ScriptAuditLog>> GetAuditLogAsync(int scriptId, CancellationToken ct = default);
    Task<IReadOnlyList<ScriptVersion>> GetVersionHistoryAsync(int scriptId, CancellationToken ct = default);
}
