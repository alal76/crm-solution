// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;

namespace CRM.Core.Entities.AI;

/// <summary>
/// Immutable audit trail for script lifecycle transitions.
/// Records are never updated or soft-deleted — only inserted.
/// </summary>
public class ScriptAuditLog
{
    public int Id { get; set; }
    public int ScriptPluginId { get; set; }
    public ScriptPlugin? ScriptPlugin { get; set; }

    /// <summary>
    /// String token such as "submitted_for_review", "approved", "rejected",
    /// "deployed", or "retired".
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    public string PerformedBy { get; set; } = string.Empty;
    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;

    public string? Notes { get; set; }

    /// <summary>Serialised string of the lifecycle state before the transition.</summary>
    public string? PreviousState { get; set; }

    /// <summary>Serialised string of the lifecycle state after the transition.</summary>
    public string? NewState { get; set; }

    /// <summary>Optional JSON blob for extra context (e.g., diff summary).</summary>
    public string? Metadata { get; set; }
}
