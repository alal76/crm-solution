// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CRM.Infrastructure.Scripting.Workflow;

/// <summary>
/// Simplified CEL (Common Expression Language) evaluator for WDL expressions.
/// Supports: ${steps.step_name.output.field}, ${input.field}, bool/comparison ops.
/// Full CEL spec: https://github.com/google/cel-spec
/// Production upgrade: use google/cel-csharp when available.
/// </summary>
public class CelExpressionEvaluator
{
    private static readonly Regex ExpressionPattern =
        new(@"\$\{([^}]+)\}", RegexOptions.Compiled, TimeSpan.FromSeconds(1));

    private static readonly Regex EqualityPattern =
        new(@"^(.+?)\s*==\s*['""]?(.+?)['""]?\s*$", RegexOptions.Compiled, TimeSpan.FromSeconds(1));

    private static readonly Regex NotNullPattern =
        new(@"^(.+?)\s*!=\s*null\s*$", RegexOptions.Compiled, TimeSpan.FromSeconds(1));

    /// <summary>Resolves ${...} expressions in a template string using provided context.</summary>
    public string Resolve(string template, IReadOnlyDictionary<string, object?> context)
    {
        if (string.IsNullOrEmpty(template) || !template.Contains("${", StringComparison.Ordinal))
            return template;

        return ExpressionPattern.Replace(template, match =>
        {
            var expr = match.Groups[1].Value.Trim();
            var resolved = ResolveExpression(expr, context);
            return resolved?.ToString() ?? string.Empty;
        });
    }

    /// <summary>Evaluates a boolean CEL expression for workflow conditions.</summary>
    public bool EvaluateCondition(string celExpression, IReadOnlyDictionary<string, object?> context)
    {
        if (string.IsNullOrEmpty(celExpression))
            return true;

        var resolved = Resolve(celExpression, context);

        if (resolved.Equals("true", StringComparison.OrdinalIgnoreCase))
            return true;

        if (resolved.Equals("false", StringComparison.OrdinalIgnoreCase))
            return false;

        // Simple equality: value == 'other'
        var eqMatch = EqualityPattern.Match(resolved);
        if (eqMatch.Success)
        {
            var left = eqMatch.Groups[1].Value.Trim();
            var right = eqMatch.Groups[2].Value.Trim();
            var leftVal = ResolveExpression(left, context)?.ToString();
            return string.Equals(leftVal, right, StringComparison.Ordinal);
        }

        // Not-null check: value != null
        var notNullMatch = NotNullPattern.Match(resolved);
        if (notNullMatch.Success)
        {
            var varName = notNullMatch.Groups[1].Value.Trim();
            return ResolveExpression(varName, context) != null;
        }

        return false; // Unknown expression treated as false (safe default)
    }

    private static object? ResolveExpression(string expr, IReadOnlyDictionary<string, object?> context)
    {
        var parts = expr.Split('.');
        if (parts.Length == 0)
            return null;

        if (!context.TryGetValue(parts[0], out var current))
            return null;

        for (int i = 1; i < parts.Length; i++)
        {
            if (current == null)
                return null;

            if (current is Dictionary<string, object?> dict)
            {
                current = dict.TryGetValue(parts[i], out var v) ? v : null;
            }
            else if (current is JsonElement elem)
            {
                if (elem.ValueKind == JsonValueKind.Object && elem.TryGetProperty(parts[i], out var prop))
                    current = prop.GetRawText().Trim('"');
                else
                    return null;
            }
            else
            {
                var prop = current.GetType().GetProperty(parts[i]);
                current = prop?.GetValue(current);
            }
        }

        return current;
    }
}
