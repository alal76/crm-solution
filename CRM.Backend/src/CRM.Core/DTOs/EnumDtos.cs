// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// ENUM-BE-010: DTOs for the Configurable Enums feature (IEnumManagementService contract)
namespace CRM.Core.Dtos;

// ─────────────────────────────────────────────────────────────────────────────
// Read DTOs
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Summary DTO for an enum category.</summary>
public class EnumCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public string? EntityType { get; set; }
    public string? PropertyName { get; set; }
    public bool IsSystemManaged { get; set; }
    public bool AllowCustomValues { get; set; }
    public int ValueCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>Detail DTO for a single enum value.</summary>
public class EnumValueDto
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
    public bool IsSystemValue { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public string? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>Detail DTO for a transition rule between two enum values.</summary>
public class EnumTransitionDto
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public int? FromValueId { get; set; }
    public string? FromValueLabel { get; set; }
    public int ToValueId { get; set; }
    public string ToValueLabel { get; set; } = string.Empty;
    public bool IsAllowed { get; set; }
    public bool RequiresApproval { get; set; }
    public string? AllowedRoles { get; set; }
    public int SortOrder { get; set; }
}

/// <summary>Result of validating a value against a category's rules.</summary>
public class EnumValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public string? WarningMessage { get; set; }
}

// ─────────────────────────────────────────────────────────────────────────────
// Write / Command DTOs
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Payload to create a new enum category (Admin only).</summary>
public class CreateEnumCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public string? EntityType { get; set; }
    public string? PropertyName { get; set; }
    public bool AllowCustomValues { get; set; } = true;
}

/// <summary>Payload to update an existing enum category (Admin only).</summary>
public class UpdateEnumCategoryDto
{
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public bool AllowCustomValues { get; set; } = true;
}

/// <summary>Payload to create a new enum value inside a category.</summary>
public class CreateEnumValueDto
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public string? Metadata { get; set; }
    public bool IsDefault { get; set; } = false;
}

/// <summary>Payload to update an existing enum value.</summary>
public class UpdateEnumValueDto
{
    public string? Label { get; set; }
    public string? Description { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public string? Metadata { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; } = false;
    public int SortOrder { get; set; }
}

/// <summary>Payload to create a transition rule between two values.</summary>
public class CreateEnumTransitionDto
{
    /// <summary>NULL = from any state</summary>
    public int? FromValueId { get; set; }
    public int ToValueId { get; set; }
    public bool IsAllowed { get; set; } = true;
    public bool RequiresApproval { get; set; } = false;
    public string? AllowedRoles { get; set; }
}

/// <summary>Request body for value validation endpoint.</summary>
public class ValidateEnumValueRequest
{
    public string CategoryName { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

/// <summary>Request body for reorder endpoint.</summary>
public class ReorderEnumValuesRequest
{
    public IEnumerable<int> OrderedIds { get; set; } = [];
}
