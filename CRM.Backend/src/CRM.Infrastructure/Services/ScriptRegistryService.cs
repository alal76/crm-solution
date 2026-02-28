// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CRM.Core.Entities.AI;
using CRM.Core.Interfaces;
using CRM.Core.Scripting;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Implements the script lifecycle state machine:
/// Draft → Review → Approved → Deployed → Retired
/// with full audit trail written to <see cref="ScriptAuditLog"/>.
/// </summary>
public class ScriptRegistryService : IScriptRegistryService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<ScriptRegistryService> _logger;

    public ScriptRegistryService(ICrmDbContext context, ILogger<ScriptRegistryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public Task<bool> SubmitForReviewAsync(int scriptId, string performedBy, string? notes, CancellationToken ct = default)
        => TransitionAsync(scriptId, ScriptLifecycleState.Draft, ScriptLifecycleState.Review, "submitted_for_review", performedBy, notes, ct);

    public Task<bool> ApproveAsync(int scriptId, string performedBy, string? notes, CancellationToken ct = default)
        => TransitionAsync(scriptId, ScriptLifecycleState.Review, ScriptLifecycleState.Approved, "approved", performedBy, notes, ct);

    public Task<bool> RejectAsync(int scriptId, string performedBy, string? notes, CancellationToken ct = default)
        => TransitionAsync(scriptId, ScriptLifecycleState.Review, ScriptLifecycleState.Draft, "rejected", performedBy, notes, ct);

    public Task<bool> DeployAsync(int scriptId, string performedBy, CancellationToken ct = default)
        => TransitionAsync(scriptId, ScriptLifecycleState.Approved, ScriptLifecycleState.Deployed, "deployed", performedBy, null, ct);

    public Task<bool> RetireAsync(int scriptId, string performedBy, string? notes, CancellationToken ct = default)
        => TransitionAsync(scriptId, ScriptLifecycleState.Deployed, ScriptLifecycleState.Retired, "retired", performedBy, notes, ct);

    public async Task<IReadOnlyList<ScriptAuditLog>> GetAuditLogAsync(int scriptId, CancellationToken ct = default)
    {
        return await _context.ScriptAuditLogs
            .Where(l => l.ScriptPluginId == scriptId)
            .OrderByDescending(l => l.PerformedAt)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ScriptVersion>> GetVersionHistoryAsync(int scriptId, CancellationToken ct = default)
    {
        return await _context.ScriptVersions
            .Where(v => v.ScriptPluginId == scriptId)
            .OrderByDescending(v => v.CreatedAt)
            .Take(10)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Private helpers
    // ─────────────────────────────────────────────────────────────────────────

    private async Task<bool> TransitionAsync(
        int scriptId,
        ScriptLifecycleState fromState,
        ScriptLifecycleState toState,
        string eventType,
        string performedBy,
        string? notes,
        CancellationToken ct)
    {
        var script = await _context.ScriptPlugins
            .FirstOrDefaultAsync(s => s.Id == scriptId, ct);

        if (script == null)
        {
            _logger.LogWarning("Script {ScriptId} not found — transition {EventType} aborted.", scriptId, eventType);
            return false;
        }

        if (script.LifecycleState != fromState)
        {
            _logger.LogWarning(
                "Script {ScriptId} state mismatch for {EventType}: expected {Expected}, actual {Actual}.",
                scriptId, eventType, fromState, script.LifecycleState);
            return false;
        }

        var prevState = script.LifecycleState.ToString();
        script.LifecycleState = toState;
        script.UpdatedAt = DateTime.UtcNow;

        _context.ScriptAuditLogs.Add(new ScriptAuditLog
        {
            ScriptPluginId = scriptId,
            EventType = eventType,
            PerformedBy = performedBy,
            PerformedAt = DateTime.UtcNow,
            Notes = notes,
            PreviousState = prevState,
            NewState = toState.ToString(),
        });

        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("Script {ScriptId}: {PrevState} → {NewState} by {User}.", scriptId, prevState, toState, performedBy);
        return true;
    }
}
