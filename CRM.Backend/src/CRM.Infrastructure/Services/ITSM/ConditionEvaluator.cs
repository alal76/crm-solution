// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text.Json;
using System.Text.Json.Serialization;
using CRM.Core.Entities;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.ITSM;

// ============================================================================
// Condition DSL model
// ============================================================================

/// <summary>
/// Top-level condition node — either a simple field comparison or a compound
/// AND / OR group whose children are also condition nodes.
/// JSON shape:
///   Simple:   { "field": "Priority", "op": "eq", "value": "High" }
///   Compound: { "operator": "AND", "conditions": [ ... ] }
/// </summary>
public class JsonConditionNode
{
    // --- Compound node fields ---

    /// <summary>Logical operator: "AND" | "OR" (only set for compound nodes).</summary>
    [JsonPropertyName("operator")]
    public string? Operator { get; set; }

    /// <summary>Child nodes (only set for compound nodes).</summary>
    [JsonPropertyName("conditions")]
    public List<JsonConditionNode>? Conditions { get; set; }

    // --- Simple condition fields ---

    /// <summary>Service-request field name: Priority | Status | CategoryId | DueDate.</summary>
    [JsonPropertyName("field")]
    public string? Field { get; set; }

    /// <summary>Comparison operator: eq | ne | gt | lt | gte | lte | contains.</summary>
    [JsonPropertyName("op")]
    public string? Op { get; set; }

    /// <summary>Value to compare against (always stored as string; parsed at evaluation time).</summary>
    [JsonPropertyName("value")]
    public string? Value { get; set; }

    /// <summary>Returns true when this node is a compound (AND/OR) node.</summary>
    [JsonIgnore]
    public bool IsCompound =>
        !string.IsNullOrWhiteSpace(Operator) && Conditions != null;
}

// ============================================================================
// Evaluator
// ============================================================================

/// <summary>
/// Evaluates a JSON condition tree against a <see cref="ServiceRequest"/>.
/// TODO-SD005-012: Complex Condition Expression Support.
/// </summary>
public class ConditionEvaluator
{
    private readonly ILogger<ConditionEvaluator>? _logger;

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public ConditionEvaluator(ILogger<ConditionEvaluator>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Parses <paramref name="conditionJson"/> and evaluates the resulting tree
    /// against <paramref name="request"/>.
    /// Returns <c>true</c> when the service request matches the condition.
    /// Returns <c>false</c> on parse errors or null JSON.
    /// </summary>
    public bool Evaluate(string? conditionJson, ServiceRequest request)
    {
        if (string.IsNullOrWhiteSpace(conditionJson))
            return true; // No conditions — always matches.

        JsonConditionNode? root;
        try
        {
            root = JsonSerializer.Deserialize<JsonConditionNode>(conditionJson, JsonOpts);
        }
        catch (JsonException ex)
        {
            _logger?.LogWarning(ex, "Failed to parse condition JSON: {Json}", conditionJson);
            return false;
        }

        if (root == null)
            return true;

        return EvaluateNode(root, request);
    }

    /// <summary>
    /// Evaluates a <see cref="JsonConditionNode"/> against a service request recursively.
    /// </summary>
    public bool EvaluateNode(JsonConditionNode node, ServiceRequest request)
    {
        if (node.IsCompound)
            return EvaluateCompound(node, request);

        return EvaluateSimple(node, request);
    }

    // -----------------------------------------------------------------------
    // Private helpers
    // -----------------------------------------------------------------------

    private bool EvaluateCompound(JsonConditionNode node, ServiceRequest request)
    {
        var op = node.Operator?.ToUpperInvariant();
        var children = node.Conditions!;

        if (children.Count == 0)
            return true;

        return op switch
        {
            "AND" => children.All(c => EvaluateNode(c, request)),
            "OR"  => children.Any(c => EvaluateNode(c, request)),
            _     => throw new InvalidOperationException($"Unknown logical operator: {node.Operator}")
        };
    }

    private bool EvaluateSimple(JsonConditionNode node, ServiceRequest request)
    {
        if (string.IsNullOrWhiteSpace(node.Field) || string.IsNullOrWhiteSpace(node.Op))
        {
            _logger?.LogWarning("Malformed simple condition — missing field or op: {Field}/{Op}", node.Field, node.Op);
            return false;
        }

        var op = node.Op.ToLowerInvariant();
        var rawValue = node.Value ?? string.Empty;

        return node.Field.ToLowerInvariant() switch
        {
            "priority"   => ComparePriority(request.Priority, op, rawValue),
            "status"     => CompareStatus(request.Status, op, rawValue),
            "categoryid" => CompareInt(request.CategoryId, op, rawValue),
            "duedate"    => CompareDate(request.DueDate, op, rawValue),
            "subject"    => CompareString(request.Subject, op, rawValue),
            _ => throw new InvalidOperationException($"Unknown field: {node.Field}")
        };
    }

    // -----------------------------------------------------------------------
    // Field-type helpers
    // -----------------------------------------------------------------------

    private static bool ComparePriority(ServiceRequestPriority actual, string op, string rawValue)
    {
        if (!Enum.TryParse<ServiceRequestPriority>(rawValue, ignoreCase: true, out var expected))
        {
            // Also accept integer strings
            if (int.TryParse(rawValue, out var intVal))
                expected = (ServiceRequestPriority)intVal;
            else
                return false;
        }

        return op switch
        {
            "eq"  => actual == expected,
            "ne"  => actual != expected,
            "gt"  => (int)actual > (int)expected,
            "lt"  => (int)actual < (int)expected,
            "gte" => (int)actual >= (int)expected,
            "lte" => (int)actual <= (int)expected,
            _     => throw new InvalidOperationException($"Unsupported op '{op}' for Priority")
        };
    }

    private static bool CompareStatus(ServiceRequestStatus actual, string op, string rawValue)
    {
        if (!Enum.TryParse<ServiceRequestStatus>(rawValue, ignoreCase: true, out var expected))
        {
            if (int.TryParse(rawValue, out var intVal))
                expected = (ServiceRequestStatus)intVal;
            else
                return false;
        }

        return op switch
        {
            "eq" => actual == expected,
            "ne" => actual != expected,
            _    => throw new InvalidOperationException($"Unsupported op '{op}' for Status")
        };
    }

    private static bool CompareInt(int? actual, string op, string rawValue)
    {
        if (!int.TryParse(rawValue, out var expected))
            return false;

        return op switch
        {
            "eq"  => actual == expected,
            "ne"  => actual != expected,
            "gt"  => actual.HasValue && actual.Value > expected,
            "lt"  => actual.HasValue && actual.Value < expected,
            "gte" => actual.HasValue && actual.Value >= expected,
            "lte" => actual.HasValue && actual.Value <= expected,
            _     => throw new InvalidOperationException($"Unsupported op '{op}' for integer field")
        };
    }

    private static bool CompareDate(DateTime? actual, string op, string rawValue)
    {
        if (!DateTime.TryParse(rawValue, out var expected))
            return false;

        return op switch
        {
            "eq"  => actual.HasValue && actual.Value.Date == expected.Date,
            "ne"  => !actual.HasValue || actual.Value.Date != expected.Date,
            "gt"  => actual.HasValue && actual.Value > expected,
            "lt"  => actual.HasValue && actual.Value < expected,
            "gte" => actual.HasValue && actual.Value >= expected,
            "lte" => actual.HasValue && actual.Value <= expected,
            _     => throw new InvalidOperationException($"Unsupported op '{op}' for date field")
        };
    }

    private static bool CompareString(string? actual, string op, string rawValue)
    {
        return op switch
        {
            "eq"       => string.Equals(actual, rawValue, StringComparison.OrdinalIgnoreCase),
            "ne"       => !string.Equals(actual, rawValue, StringComparison.OrdinalIgnoreCase),
            "contains" => actual?.Contains(rawValue, StringComparison.OrdinalIgnoreCase) == true,
            _          => throw new InvalidOperationException($"Unsupported op '{op}' for string field")
        };
    }
}
