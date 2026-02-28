// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text.Json.Serialization;

namespace CRM.Core.Entities;

/// <summary>
/// Represents an allowed (or forbidden) state transition within a configurable enum (ENUM-BE-003).
/// Maps to the EnumTransitions table.
/// </summary>
public class EnumTransition : BaseEntity
{
    public int CategoryId { get; set; }

    [JsonIgnore]
    public EnumCategory? Category { get; set; }

    /// <summary>NULL means "from any state"</summary>
    public int? FromValueId { get; set; }

    [JsonIgnore]
    public EnumValue? FromValue { get; set; }

    public int ToValueId { get; set; }

    [JsonIgnore]
    public EnumValue? ToValue { get; set; }

    /// <summary>Whether this transition is allowed (false = explicitly blocked)</summary>
    public bool IsAllowed { get; set; } = true;

    /// <summary>Whether this transition requires approval before completing</summary>
    public bool RequiresApproval { get; set; } = false;

    /// <summary>Comma-separated role names that may make this transition (null = any role)</summary>
    public string? AllowedRoles { get; set; }

    /// <summary>Optional expression to validate before the transition is allowed</summary>
    public string? ValidateExpression { get; set; }

    /// <summary>Display sort order</summary>
    public int SortOrder { get; set; } = 0;
}
