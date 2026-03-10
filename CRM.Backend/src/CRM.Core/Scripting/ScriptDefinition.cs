// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Scripting;

/// <summary>
/// Immutable definition of a script to be compiled and executed.
/// </summary>
public record ScriptDefinition
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Name { get; init; } = string.Empty;
    public string Version { get; init; } = "1.0.0"; // SemVer string
    public ScriptKind Kind { get; init; }
    public ScriptRuntime Runtime { get; init; }
    public string Source { get; init; } = string.Empty;
    public string? InputSchema { get; init; } // JSON Schema string
    public string? OutputSchema { get; init; } // JSON Schema string
    public IReadOnlyList<ScriptPermission> Permissions { get; init; } = Array.Empty<ScriptPermission>();
    public IReadOnlyList<string> RequiredSecrets { get; init; } = Array.Empty<string>();
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);
    public int MemoryLimitMb { get; init; } = 64;
    public ScriptMetadata Metadata { get; init; } = new();
}

/// <summary>
/// Authoring metadata attached to a <see cref="ScriptDefinition"/>.
/// </summary>
public record ScriptMetadata
{
    public string? Author { get; init; }
    public string? Description { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
    public ScriptLifecycleState LifecycleState { get; init; } = ScriptLifecycleState.Draft;
    public IReadOnlyList<string> ApprovalChain { get; init; } = Array.Empty<string>();
}

/// <summary>
/// A named, optionally described permission that a script declares it requires.
/// </summary>
/// <param name="Name">Unique permission name (e.g., "crm:read:customer").</param>
/// <param name="Description">Human-readable description of why the permission is required.</param>
public record ScriptPermission(string Name, string Description = "");
