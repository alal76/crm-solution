// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using CRM.Core.Scripting;

namespace CRM.Core.Entities.AI;

/// <summary>
/// Immutable snapshot of a <see cref="ScriptPlugin"/> at a specific SemVer version.
/// Created each time a script is promoted through the approval workflow.
/// </summary>
public class ScriptVersion : BaseEntity
{
    public int ScriptPluginId { get; set; }
    public ScriptPlugin? ScriptPlugin { get; set; }

    /// <summary>SemVer string e.g. "1.3.0".</summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>Full source code snapshot at this version.</summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>SHA-256 hex digest of <see cref="Source"/>.</summary>
    public string ContentHash { get; set; } = string.Empty;

    public ScriptLifecycleState LifecycleState { get; set; } = ScriptLifecycleState.Draft;

    public string? ChangeNotes { get; set; }

    /// <summary>Username of the approver, or null if not yet approved.</summary>
    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    /// <summary>True when this is the version currently deployed to production.</summary>
    public bool IsCurrent { get; set; }
}
