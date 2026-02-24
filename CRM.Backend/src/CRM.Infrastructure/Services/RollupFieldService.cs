// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

#region Interfaces and DTOs

/// <summary>
/// Service for computing rollup summary fields that aggregate child records.
/// Supports SUM, COUNT, AVG, MIN, MAX aggregation functions.
/// </summary>
public interface IRollupFieldService
{
    /// <summary>
    /// Calculates a rollup value for a parent entity by aggregating child records.
    /// </summary>
    /// <param name="request">Rollup calculation request.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The computed rollup result.</returns>
    Task<RollupResult> CalculateAsync(RollupRequest request, CancellationToken ct = default);

    /// <summary>
    /// Recalculates all rollup fields for a given parent entity.
    /// </summary>
    Task<List<RollupResult>> RecalculateAllForEntityAsync(string parentEntityType, int parentId, CancellationToken ct = default);

    /// <summary>
    /// Registers a rollup field definition.
    /// </summary>
    Task<RollupDefinition> RegisterRollupAsync(RollupDefinition definition, CancellationToken ct = default);

    /// <summary>
    /// Gets all rollup definitions for a parent entity type.
    /// </summary>
    Task<List<RollupDefinition>> GetDefinitionsAsync(string parentEntityType, CancellationToken ct = default);
}

/// <summary>
/// Request to calculate a rollup value.
/// </summary>
public class RollupRequest
{
    /// <summary>Parent entity type (e.g., "Account").</summary>
    public string ParentEntityType { get; set; } = string.Empty;

    /// <summary>Parent entity ID.</summary>
    public int ParentEntityId { get; set; }

    /// <summary>Child entity type (e.g., "Opportunity").</summary>
    public string ChildEntityType { get; set; } = string.Empty;

    /// <summary>Field on child to aggregate.</summary>
    public string ChildFieldName { get; set; } = string.Empty;

    /// <summary>Foreign key field on child referencing parent.</summary>
    public string ForeignKeyField { get; set; } = string.Empty;

    /// <summary>Aggregation function to apply.</summary>
    public RollupFunction Function { get; set; } = RollupFunction.Sum;

    /// <summary>Optional filter on child records (JSON filter criteria).</summary>
    public string? FilterCriteria { get; set; }
}

/// <summary>
/// Result of a rollup calculation.
/// </summary>
public class RollupResult
{
    public string FieldName { get; set; } = string.Empty;
    public string ParentEntityType { get; set; } = string.Empty;
    public int ParentEntityId { get; set; }
    public RollupFunction Function { get; set; }
    public double? NumericValue { get; set; }
    public int RecordCount { get; set; }
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    public bool Success { get; set; } = true;
    public string? Error { get; set; }
}

/// <summary>
/// Definition of a rollup summary field.
/// </summary>
public class RollupDefinition
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ParentEntityType { get; set; } = string.Empty;
    public string ChildEntityType { get; set; } = string.Empty;
    public string ChildFieldName { get; set; } = string.Empty;
    public string ForeignKeyField { get; set; } = string.Empty;
    public RollupFunction Function { get; set; } = RollupFunction.Sum;
    public string? FilterCriteria { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Supported rollup aggregation functions.
/// </summary>
public enum RollupFunction
{
    Sum = 0,
    Count = 1,
    Average = 2,
    Min = 3,
    Max = 4
}

#endregion

/// <summary>
/// Service that aggregates child records using SUM, COUNT, AVG, MIN, MAX.
/// Uses dynamic LINQ queries against the database context.
/// </summary>
public class RollupFieldService : IRollupFieldService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<RollupFieldService> _logger;

    // In-memory store for rollup definitions
    private readonly List<RollupDefinition> _definitions = new();
    private int _nextId = 1;

    // Entity type to DbSet property mapping for dynamic queries
    private static readonly Dictionary<string, string> EntityTypeMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Opportunity"] = "Opportunities",
        ["Contact"] = "Contacts",
        ["Lead"] = "Leads",
        ["ServiceRequest"] = "ServiceRequests",
        ["Order"] = "Orders",
        ["Invoice"] = "Invoices",
        ["Payment"] = "Payments",
        ["Interaction"] = "Interactions",
        ["Quote"] = "Quotes",
        ["Contract"] = "Contracts"
    };

    public RollupFieldService(
        ICrmDbContext context,
        ILogger<RollupFieldService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<RollupResult> CalculateAsync(RollupRequest request, CancellationToken ct = default)
    {
        var result = new RollupResult
        {
            FieldName = $"{request.Function}({request.ChildEntityType}.{request.ChildFieldName})",
            ParentEntityType = request.ParentEntityType,
            ParentEntityId = request.ParentEntityId,
            Function = request.Function
        };

        try
        {
            // Get child records dynamically based on entity type
            var values = await GetChildFieldValues(
                request.ChildEntityType,
                request.ForeignKeyField,
                request.ParentEntityId,
                request.ChildFieldName,
                ct);

            if (values.Count == 0)
            {
                result.NumericValue = request.Function == RollupFunction.Count ? 0 : null;
                result.RecordCount = 0;
                return result;
            }

            result.RecordCount = values.Count;

            result.NumericValue = request.Function switch
            {
                RollupFunction.Sum => values.Sum(),
                RollupFunction.Count => values.Count,
                RollupFunction.Average => values.Average(),
                RollupFunction.Min => values.Min(),
                RollupFunction.Max => values.Max(),
                _ => throw new ArgumentOutOfRangeException(nameof(request.Function))
            };

            _logger.LogDebug(
                "Rollup {Function}({ChildEntity}.{ChildField}) for {ParentEntity}#{ParentId} = {Value}",
                request.Function, request.ChildEntityType, request.ChildFieldName,
                request.ParentEntityType, request.ParentEntityId, result.NumericValue);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Rollup calculation failed for {ParentEntity}#{ParentId}",
                request.ParentEntityType, request.ParentEntityId);
            result.Success = false;
            result.Error = ex.Message;
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<List<RollupResult>> RecalculateAllForEntityAsync(
        string parentEntityType, int parentId, CancellationToken ct = default)
    {
        var definitions = await GetDefinitionsAsync(parentEntityType, ct);
        var results = new List<RollupResult>();

        foreach (var def in definitions.Where(d => d.IsActive))
        {
            var request = new RollupRequest
            {
                ParentEntityType = parentEntityType,
                ParentEntityId = parentId,
                ChildEntityType = def.ChildEntityType,
                ChildFieldName = def.ChildFieldName,
                ForeignKeyField = def.ForeignKeyField,
                Function = def.Function,
                FilterCriteria = def.FilterCriteria
            };

            results.Add(await CalculateAsync(request, ct));
        }

        return results;
    }

    /// <inheritdoc />
    public Task<RollupDefinition> RegisterRollupAsync(RollupDefinition definition, CancellationToken ct = default)
    {
        definition.Id = _nextId++;
        _definitions.Add(definition);
        _logger.LogInformation("Registered rollup: {Function}({ChildEntity}.{ChildField}) on {ParentEntity}",
            definition.Function, definition.ChildEntityType, definition.ChildFieldName, definition.ParentEntityType);
        return Task.FromResult(definition);
    }

    /// <inheritdoc />
    public Task<List<RollupDefinition>> GetDefinitionsAsync(string parentEntityType, CancellationToken ct = default)
    {
        var defs = _definitions
            .Where(d => d.ParentEntityType.Equals(parentEntityType, StringComparison.OrdinalIgnoreCase))
            .ToList();
        return Task.FromResult(defs);
    }

    /// <summary>
    /// Dynamically queries child entity records and extracts numeric field values.
    /// Uses reflection to access the appropriate DbSet and field.
    /// </summary>
    private async Task<List<double>> GetChildFieldValues(
        string childEntityType, string foreignKeyField, int parentId,
        string fieldName, CancellationToken ct)
    {
        var values = new List<double>();

        // Use well-known entity types for type-safe queries
        switch (childEntityType.ToLowerInvariant())
        {
            case "opportunity":
                var opportunities = await _context.Opportunities
                    .Where(o => !o.IsDeleted)
                    .ToListAsync(ct);
                values = ExtractValues(opportunities, foreignKeyField, parentId, fieldName);
                break;

            case "lead":
                var leads = await _context.Leads
                    .Where(l => !l.IsDeleted)
                    .ToListAsync(ct);
                values = ExtractValues(leads, foreignKeyField, parentId, fieldName);
                break;

            case "interaction":
                var interactions = await _context.Interactions
                    .Where(i => !i.IsDeleted)
                    .ToListAsync(ct);
                values = ExtractValues(interactions, foreignKeyField, parentId, fieldName);
                break;

            case "servicerequest":
                var tickets = await _context.ServiceRequests
                    .Where(s => !s.IsDeleted)
                    .ToListAsync(ct);
                values = ExtractValues(tickets, foreignKeyField, parentId, fieldName);
                break;

            default:
                _logger.LogWarning("Unsupported child entity type for rollup: {EntityType}", childEntityType);
                break;
        }

        return values;
    }

    /// <summary>
    /// Extracts numeric values from entities using reflection.
    /// </summary>
    private static List<double> ExtractValues<T>(
        List<T> entities, string foreignKeyField, int parentId, string fieldName)
    {
        var values = new List<double>();
        var fkProp = typeof(T).GetProperty(foreignKeyField);
        var valueProp = typeof(T).GetProperty(fieldName);

        if (fkProp == null || valueProp == null) return values;

        foreach (var entity in entities)
        {
            var fkValue = fkProp.GetValue(entity);
            if (fkValue == null) continue;

            int fkInt;
            if (fkValue is int i) fkInt = i;
            else if (fkValue != null && int.TryParse(fkValue.ToString(), out var parsedFk)) fkInt = parsedFk;
            else continue;

            if (fkInt != parentId) continue;

            var rawValue = valueProp.GetValue(entity);
            if (rawValue == null) continue;

            if (rawValue is double d) values.Add(d);
            else if (rawValue is decimal dec) values.Add((double)dec);
            else if (rawValue is int intVal) values.Add(intVal);
            else if (rawValue is long longVal) values.Add(longVal);
            else if (rawValue is float f) values.Add(f);
            else if (double.TryParse(rawValue.ToString(), out var parsed)) values.Add(parsed);
        }

        return values;
    }
}
