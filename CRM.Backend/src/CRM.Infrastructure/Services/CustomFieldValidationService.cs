// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text.RegularExpressions;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

#region Interfaces and DTOs

/// <summary>
/// Service for validating custom field values against configured rules.
/// </summary>
public interface ICustomFieldValidationService
{
    /// <summary>
    /// Validates a custom field value against its definition rules.
    /// </summary>
    Task<CustomFieldValidationResult> ValidateAsync(string entityType, string fieldKey, string? value, CancellationToken ct = default);

    /// <summary>
    /// Validates all custom field values for a given entity instance.
    /// </summary>
    Task<List<CustomFieldValidationResult>> ValidateAllAsync(string entityType, int entityId, Dictionary<string, string?> fieldValues, CancellationToken ct = default);

    /// <summary>
    /// Registers or updates a custom field definition with validation rules.
    /// </summary>
    Task<CustomFieldDefinition> UpsertDefinitionAsync(CustomFieldDefinition definition, CancellationToken ct = default);

    /// <summary>
    /// Gets all field definitions for an entity type.
    /// </summary>
    Task<List<CustomFieldDefinition>> GetDefinitionsAsync(string entityType, CancellationToken ct = default);
}

/// <summary>
/// Result of validating a single custom field value.
/// </summary>
public class CustomFieldValidationResult
{
    public string FieldKey { get; set; } = string.Empty;
    public bool IsValid { get; set; } = true;
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Definition of a custom field including its validation rules.
/// Stored in-memory or in the CustomFields metadata table.
/// </summary>
public class CustomFieldDefinition
{
    public int Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string FieldKey { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public CustomFieldDataType DataType { get; set; } = CustomFieldDataType.Text;
    public bool IsRequired { get; set; }
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    public string? RegexPattern { get; set; }
    public string? RegexErrorMessage { get; set; }
    public double? MinValue { get; set; }
    public double? MaxValue { get; set; }
    public string? DefaultValue { get; set; }
    public List<string> Options { get; set; } = new();
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Data types supported by custom fields.
/// </summary>
public enum CustomFieldDataType
{
    Text = 0,
    Number = 1,
    Date = 2,
    Dropdown = 3,
    Checkbox = 4,
    MultiSelect = 5,
    Email = 6,
    Url = 7,
    TextArea = 8,
    Currency = 9
}

#endregion

/// <summary>
/// Validates custom field values against configured rules (required, min/max length, regex pattern, range).
/// </summary>
public class CustomFieldValidationService : ICustomFieldValidationService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<CustomFieldValidationService> _logger;

    // In-memory cache of field definitions keyed by entity type
    private readonly Dictionary<string, List<CustomFieldDefinition>> _definitionCache = new();

    public CustomFieldValidationService(
        ICrmDbContext context,
        ILogger<CustomFieldValidationService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<CustomFieldValidationResult> ValidateAsync(
        string entityType, string fieldKey, string? value, CancellationToken ct = default)
    {
        var result = new CustomFieldValidationResult { FieldKey = fieldKey };
        var definitions = await GetDefinitionsAsync(entityType, ct);
        var definition = definitions.FirstOrDefault(d => d.FieldKey == fieldKey);

        if (definition == null)
        {
            _logger.LogWarning("No custom field definition found for {EntityType}.{FieldKey}", entityType, fieldKey);
            // Unknown fields are considered valid (no rules to violate)
            return result;
        }

        ValidateValue(definition, value, result);
        return result;
    }

    /// <inheritdoc />
    public async Task<List<CustomFieldValidationResult>> ValidateAllAsync(
        string entityType, int entityId, Dictionary<string, string?> fieldValues, CancellationToken ct = default)
    {
        var results = new List<CustomFieldValidationResult>();
        var definitions = await GetDefinitionsAsync(entityType, ct);

        // Validate provided values
        foreach (var kvp in fieldValues)
        {
            var definition = definitions.FirstOrDefault(d => d.FieldKey == kvp.Key);
            if (definition == null) continue;

            var result = new CustomFieldValidationResult { FieldKey = kvp.Key };
            ValidateValue(definition, kvp.Value, result);
            results.Add(result);
        }

        // Check for missing required fields
        foreach (var def in definitions.Where(d => d.IsRequired && d.IsActive))
        {
            if (!fieldValues.ContainsKey(def.FieldKey))
            {
                results.Add(new CustomFieldValidationResult
                {
                    FieldKey = def.FieldKey,
                    IsValid = false,
                    Errors = { $"'{def.Label}' is required." }
                });
            }
        }

        return results;
    }

    /// <inheritdoc />
    public async Task<CustomFieldDefinition> UpsertDefinitionAsync(
        CustomFieldDefinition definition, CancellationToken ct = default)
    {
        // Store definition as a CustomField metadata record
        var existing = await _context.CustomFields
            .FirstOrDefaultAsync(cf =>
                cf.EntityType == definition.EntityType &&
                cf.Key == $"__def__{definition.FieldKey}" &&
                !cf.IsDeleted, ct);

        var serialized = System.Text.Json.JsonSerializer.Serialize(definition);

        if (existing != null)
        {
            existing.Value = serialized;
        }
        else
        {
            var record = new CustomField
            {
                EntityType = definition.EntityType,
                EntityId = 0, // metadata record, not tied to a specific entity
                Key = $"__def__{definition.FieldKey}",
                Value = serialized,
                CreatedAt = DateTime.UtcNow
            };
            await _context.CustomFields.AddAsync(record, ct);
        }

        await _context.SaveChangesAsync(ct);

        // Invalidate cache
        _definitionCache.Remove(definition.EntityType);

        _logger.LogInformation("Upserted custom field definition {EntityType}.{FieldKey}",
            definition.EntityType, definition.FieldKey);

        return definition;
    }

    /// <inheritdoc />
    public async Task<List<CustomFieldDefinition>> GetDefinitionsAsync(string entityType, CancellationToken ct = default)
    {
        if (_definitionCache.TryGetValue(entityType, out var cached))
            return cached;

        var records = await _context.CustomFields
            .Where(cf => cf.EntityType == entityType &&
                         cf.Key != null && cf.Key.StartsWith("__def__") &&
                         !cf.IsDeleted)
            .ToListAsync(ct);

        var definitions = new List<CustomFieldDefinition>();
        foreach (var record in records)
        {
            if (string.IsNullOrEmpty(record.Value)) continue;
            try
            {
                var def = System.Text.Json.JsonSerializer.Deserialize<CustomFieldDefinition>(record.Value);
                if (def != null)
                {
                    def.Id = record.Id;
                    definitions.Add(def);
                }
            }
            catch (System.Text.Json.JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize custom field definition {Id}", record.Id);
            }
        }

        definitions = definitions.OrderBy(d => d.SortOrder).ToList();
        _definitionCache[entityType] = definitions;
        return definitions;
    }

    /// <summary>
    /// Validates a single value against a field definition's rules.
    /// </summary>
    private static void ValidateValue(CustomFieldDefinition definition, string? value, CustomFieldValidationResult result)
    {
        var isEmpty = string.IsNullOrWhiteSpace(value);

        // Required check
        if (definition.IsRequired && isEmpty)
        {
            result.IsValid = false;
            result.Errors.Add($"'{definition.Label}' is required.");
            return; // No point checking further
        }

        // If empty and not required, skip other checks
        if (isEmpty) return;

        // Min length
        if (definition.MinLength.HasValue && value!.Length < definition.MinLength.Value)
        {
            result.IsValid = false;
            result.Errors.Add($"'{definition.Label}' must be at least {definition.MinLength} characters.");
        }

        // Max length
        if (definition.MaxLength.HasValue && value!.Length > definition.MaxLength.Value)
        {
            result.IsValid = false;
            result.Errors.Add($"'{definition.Label}' must be at most {definition.MaxLength} characters.");
        }

        // Regex pattern
        if (!string.IsNullOrEmpty(definition.RegexPattern))
        {
            try
            {
                if (!Regex.IsMatch(value!, definition.RegexPattern))
                {
                    result.IsValid = false;
                    result.Errors.Add(definition.RegexErrorMessage ?? $"'{definition.Label}' does not match required pattern.");
                }
            }
            catch (RegexParseException ex)
            {
                result.IsValid = false;
                result.Errors.Add($"Invalid regex pattern configured for '{definition.Label}': {ex.Message}");
            }
        }

        // Numeric range
        if (definition.DataType is CustomFieldDataType.Number or CustomFieldDataType.Currency)
        {
            if (double.TryParse(value, out double numValue))
            {
                if (definition.MinValue.HasValue && numValue < definition.MinValue.Value)
                {
                    result.IsValid = false;
                    result.Errors.Add($"'{definition.Label}' must be at least {definition.MinValue}.");
                }
                if (definition.MaxValue.HasValue && numValue > definition.MaxValue.Value)
                {
                    result.IsValid = false;
                    result.Errors.Add($"'{definition.Label}' must be at most {definition.MaxValue}.");
                }
            }
            else
            {
                result.IsValid = false;
                result.Errors.Add($"'{definition.Label}' must be a valid number.");
            }
        }

        // Date validation
        if (definition.DataType == CustomFieldDataType.Date && !DateTime.TryParse(value, out _))
        {
            result.IsValid = false;
            result.Errors.Add($"'{definition.Label}' must be a valid date.");
        }

        // Dropdown / MultiSelect: value must be in options list
        if (definition.DataType is CustomFieldDataType.Dropdown or CustomFieldDataType.MultiSelect
            && definition.Options.Count > 0)
        {
            if (definition.DataType == CustomFieldDataType.MultiSelect)
            {
                var selectedValues = value!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var invalidValues = selectedValues.Where(v => !definition.Options.Contains(v, StringComparer.OrdinalIgnoreCase)).ToList();
                if (invalidValues.Count > 0)
                {
                    result.IsValid = false;
                    result.Errors.Add($"'{definition.Label}' contains invalid options: {string.Join(", ", invalidValues)}.");
                }
            }
            else
            {
                if (!definition.Options.Contains(value!, StringComparer.OrdinalIgnoreCase))
                {
                    result.IsValid = false;
                    result.Errors.Add($"'{definition.Label}' must be one of: {string.Join(", ", definition.Options)}.");
                }
            }
        }

        // Email validation
        if (definition.DataType == CustomFieldDataType.Email && !Regex.IsMatch(value!, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            result.IsValid = false;
            result.Errors.Add($"'{definition.Label}' must be a valid email address.");
        }

        // URL validation
        if (definition.DataType == CustomFieldDataType.Url && !Uri.TryCreate(value, UriKind.Absolute, out _))
        {
            result.IsValid = false;
            result.Errors.Add($"'{definition.Label}' must be a valid URL.");
        }
    }
}
