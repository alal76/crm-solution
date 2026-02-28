// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Dtos;

/// <summary>
/// Represents a dunning schedule step returned from the API.
/// BACK-010: Dunning Schedule CRUD.
/// </summary>
public sealed class DunningScheduleDto
{
    /// <summary>Primary key.</summary>
    public int Id { get; init; }

    /// <summary>Descriptive name for this dunning step.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Days past invoice due date that trigger this step.</summary>
    public int DaysOverdue { get; init; }

    /// <summary>Email subject for the dunning message.</summary>
    public string EmailSubject { get; init; } = string.Empty;

    /// <summary>Email body template for this step.</summary>
    public string EmailBody { get; init; } = string.Empty;

    /// <summary>Whether this step is active in automated processing.</summary>
    public bool IsActive { get; init; }

    /// <summary>Ordinal position (lower = earlier step).</summary>
    public int StepOrder { get; init; }

    /// <summary>Record creation timestamp (UTC).</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>Last-modified timestamp (UTC).</summary>
    public DateTime UpdatedAt { get; init; }
}

/// <summary>
/// Payload for creating a new dunning schedule step.
/// BACK-010: Dunning Schedule CRUD.
/// </summary>
public sealed class CreateDunningScheduleDto
{
    /// <summary>Descriptive name (required, max 200 chars).</summary>
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Name { get; init; } = string.Empty;

    /// <summary>Days past invoice due date that trigger this step (must be ≥ 0).</summary>
    [Range(0, 365)]
    public int DaysOverdue { get; init; }

    /// <summary>Email subject line (required, max 500 chars).</summary>
    [Required]
    [StringLength(500, MinimumLength = 1)]
    public string EmailSubject { get; init; } = string.Empty;

    /// <summary>Email body template (required, max 10000 chars).</summary>
    [Required]
    [StringLength(10000, MinimumLength = 1)]
    public string EmailBody { get; init; } = string.Empty;

    /// <summary>Whether this step participates in automated processing (default: true).</summary>
    public bool IsActive { get; init; } = true;

    /// <summary>Ordinal position used to sort steps (lower = earlier).</summary>
    [Range(0, int.MaxValue)]
    public int StepOrder { get; init; }
}

/// <summary>
/// Payload for updating an existing dunning schedule step.
/// All fields are optional; only supplied fields are updated.
/// BACK-010: Dunning Schedule CRUD.
/// </summary>
public sealed class UpdateDunningScheduleDto
{
    /// <summary>Descriptive name (max 200 chars).</summary>
    [StringLength(200, MinimumLength = 1)]
    public string? Name { get; init; }

    /// <summary>Days past invoice due date that trigger this step (0-365).</summary>
    [Range(0, 365)]
    public int? DaysOverdue { get; init; }

    /// <summary>Email subject line (max 500 chars).</summary>
    [StringLength(500, MinimumLength = 1)]
    public string? EmailSubject { get; init; }

    /// <summary>Email body template (max 10000 chars).</summary>
    [StringLength(10000, MinimumLength = 1)]
    public string? EmailBody { get; init; }

    /// <summary>Whether this step participates in automated processing.</summary>
    public bool? IsActive { get; init; }

    /// <summary>Ordinal position used to sort steps.</summary>
    [Range(0, int.MaxValue)]
    public int? StepOrder { get; init; }
}
