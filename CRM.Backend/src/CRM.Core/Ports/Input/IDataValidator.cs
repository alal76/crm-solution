// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Ports.Input;

// ============================================================================
// Supporting types
// ============================================================================

/// <summary>
/// Result of validating a single record.
/// </summary>
public class ValidationResult
{
    /// <summary>Whether the record passed all validation rules.</summary>
    public bool IsValid { get; set; }

    /// <summary>1-based row number (null when validating a single record outside a batch).</summary>
    public int? RowNumber { get; set; }

    /// <summary>List of field-level errors found during validation.</summary>
    public List<FieldValidationError> Errors { get; set; } = [];
}

/// <summary>
/// A single field-level validation failure.
/// </summary>
public class FieldValidationError
{
    /// <summary>The field name (same casing as the DTO/entity property).</summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>Human-readable description of the error.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Optional machine-readable error code, e.g. "REQUIRED", "INVALID_EMAIL", "TOO_LONG".
    /// </summary>
    public string? Code { get; set; }
}

/// <summary>
/// Describes a single validation rule applied to a field.
/// Exposed via <see cref="IDataValidator.GetValidationRules"/> so that the
/// frontend can mirror backend rules in the column-mapper / preview step.
/// </summary>
public class FieldValidationRule
{
    /// <summary>Field name this rule applies to.</summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>Whether the field must be present and non-empty.</summary>
    public bool Required { get; set; }

    /// <summary>
    /// Optional semantic format: "email", "phone", "date", "url".
    /// Null when no specific format is required.
    /// </summary>
    public string? Format { get; set; }

    /// <summary>Maximum allowed string length (null = unlimited).</summary>
    public int? MaxLength { get; set; }

    /// <summary>Minimum allowed string length (null = no minimum).</summary>
    public int? MinLength { get; set; }
}

// ============================================================================
// Port interface
// ============================================================================

/// <summary>
/// Input port for data validation during import operations.
/// Validates individual records or batches against entity-specific rules.
/// </summary>
public interface IDataValidator
{
    /// <summary>
    /// Validates a single record against the rules for the given entity type.
    /// </summary>
    /// <param name="entityType">Lowercase entity name, e.g. "accounts", "contacts".</param>
    /// <param name="record">Dictionary of field name → raw value pairs.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<ValidationResult> ValidateRecordAsync(
        string entityType,
        Dictionary<string, object?> record,
        CancellationToken ct = default);

    /// <summary>
    /// Validates a batch of records, assigning sequential 1-based row numbers.
    /// </summary>
    /// <param name="entityType">Lowercase entity name, e.g. "accounts", "contacts".</param>
    /// <param name="records">Ordered sequence of field-value dictionaries.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IEnumerable<ValidationResult>> ValidateBatchAsync(
        string entityType,
        IEnumerable<Dictionary<string, object?>> records,
        CancellationToken ct = default);

    /// <summary>
    /// Returns the set of validation rules for the given entity type.
    /// Used by the frontend to render required-field indicators and format hints.
    /// </summary>
    /// <param name="entityType">Lowercase entity name.</param>
    IEnumerable<FieldValidationRule> GetValidationRules(string entityType);
}
