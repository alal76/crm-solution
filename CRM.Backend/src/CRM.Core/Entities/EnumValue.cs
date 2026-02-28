// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text.Json.Serialization;

namespace CRM.Core.Entities;

/// <summary>
/// Represents a single value within a configurable enumeration category (ENUM-BE-002).
/// Maps to the EnumValues table.
/// </summary>
public class EnumValue : BaseEntity
{
    public int CategoryId { get; set; }

    [JsonIgnore]
    public EnumCategory? Category { get; set; }

    /// <summary>Machine-readable key (e.g. "new", "in_progress")</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Human-readable display label</summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>Optional description</summary>
    public string? Description { get; set; }

    /// <summary>Display sort order</summary>
    public int SortOrder { get; set; } = 0;

    /// <summary>Whether this value is selectable</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Whether this is the default value for new records</summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>System values cannot be deleted</summary>
    public bool IsSystemValue { get; set; } = false;

    /// <summary>Optional color for UI rendering (hex or named color)</summary>
    public string? Color { get; set; }

    /// <summary>Optional icon identifier for UI rendering</summary>
    public string? Icon { get; set; }

    /// <summary>Optional JSON metadata</summary>
    public string? Metadata { get; set; }

    /// <summary>Optional validation rules in JSON format</summary>
    public string? ValidationRules { get; set; }

    [JsonIgnore]
    public ICollection<EnumTransition> AsFromTransitions { get; set; } = new List<EnumTransition>();

    [JsonIgnore]
    public ICollection<EnumTransition> AsToTransitions { get; set; } = new List<EnumTransition>();
}
