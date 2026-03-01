// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using CRM.Core.Scripting;

namespace CRM.Infrastructure.Scripting;

/// <summary>
/// Detects breaking changes between two versions of a <see cref="ScriptDefinition"/>
/// by comparing runtime, JSON schemas, and declared permissions.
/// </summary>
/// <remarks>
/// Currently uses a simplified structural diff on serialised JSON. Production deployments
/// should integrate NJsonSchema for fine-grained schema compatibility analysis.
/// </remarks>
public class ScriptBreakingChangeDetector
{
    public BreakingChangeReport Analyze(ScriptDefinition previous, ScriptDefinition next)
    {
        var issues = new List<string>();

        if (previous.Runtime != next.Runtime)
        {
            issues.Add($"Runtime changed: {previous.Runtime} → {next.Runtime}");
        }

        if (IsBreakingSchemaChange(previous.InputSchema, next.InputSchema))
        {
            issues.Add("Input schema has breaking changes (required fields added or types changed).");
        }

        if (IsBreakingSchemaChange(previous.OutputSchema, next.OutputSchema))
        {
            issues.Add("Output schema has breaking changes (fields removed or types changed).");
        }

        var removedPermissions = previous.Permissions
            .Select(p => p.Name)
            .Except(next.Permissions.Select(p => p.Name))
            .ToList();

        if (removedPermissions.Count > 0)
        {
            issues.Add($"Permissions removed: {string.Join(", ", removedPermissions)}");
        }

        return new BreakingChangeReport(issues.Count == 0, issues);
    }

    private static bool IsBreakingSchemaChange(string? before, string? after)
    {
        if (string.Equals(before, after, System.StringComparison.Ordinal)) return false;
        if (before == null || after == null) return true;

        try
        {
            var b = JsonDocument.Parse(before);
            var a = JsonDocument.Parse(after);
            return b.RootElement.GetRawText() != a.RootElement.GetRawText();
        }
        catch
        {
            return true; // parse failure is treated as potentially breaking
        }
    }
}

/// <summary>Result of a <see cref="ScriptBreakingChangeDetector.Analyze"/> call.</summary>
/// <param name="IsCompatible"><c>true</c> when no breaking changes were detected.</param>
/// <param name="Issues">Human-readable descriptions of each issue, empty when compatible.</param>
public record BreakingChangeReport(bool IsCompatible, IReadOnlyList<string> Issues);
