// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Dtos.ITSM;

/// <summary>
/// DTO for change type response
/// </summary>
public class ChangeTypeDto
{
    /// <summary>Primary key</summary>
    public int Id { get; set; }

    /// <summary>The name of the change type</summary>
    public string TypeName { get; set; } = string.Empty;

    /// <summary>Description of the change type</summary>
    public string? Description { get; set; }

    /// <summary>Whether this change type requires CAB review</summary>
    public bool RequiresCAB { get; set; }

    /// <summary>Whether this change type requires approval</summary>
    public bool RequiresApproval { get; set; }

    /// <summary>Default risk level (Low, Medium, High)</summary>
    public string DefaultRiskLevel { get; set; } = "Medium";

    /// <summary>Minimum lead time in days</summary>
    public int LeadTimeDays { get; set; }

    /// <summary>Whether this change type is active</summary>
    public bool IsActive { get; set; }

    /// <summary>Created timestamp</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Updated timestamp</summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating a new change type
/// </summary>
public class CreateChangeTypeDto
{
    /// <summary>The name of the change type</summary>
    [Required(ErrorMessage = "TypeName is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "TypeName must be between 1 and 100 characters")]
    public string TypeName { get; set; } = string.Empty;

    /// <summary>Description of the change type</summary>
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    /// <summary>Whether this change type requires CAB review</summary>
    public bool RequiresCAB { get; set; }

    /// <summary>Whether this change type requires approval</summary>
    public bool RequiresApproval { get; set; }

    /// <summary>Default risk level (Low, Medium, High)</summary>
    [StringLength(20, ErrorMessage = "DefaultRiskLevel cannot exceed 20 characters")]
    public string DefaultRiskLevel { get; set; } = "Medium";

    /// <summary>Minimum lead time in days</summary>
    [Range(0, 365, ErrorMessage = "LeadTimeDays must be between 0 and 365")]
    public int LeadTimeDays { get; set; }

    /// <summary>Whether this change type is active</summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// DTO for updating an existing change type
/// </summary>
public class UpdateChangeTypeDto
{
    /// <summary>The name of the change type</summary>
    [StringLength(100, MinimumLength = 1, ErrorMessage = "TypeName must be between 1 and 100 characters")]
    public string? TypeName { get; set; }

    /// <summary>Description of the change type</summary>
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    /// <summary>Whether this change type requires CAB review</summary>
    public bool? RequiresCAB { get; set; }

    /// <summary>Whether this change type requires approval</summary>
    public bool? RequiresApproval { get; set; }

    /// <summary>Default risk level (Low, Medium, High)</summary>
    [StringLength(20, ErrorMessage = "DefaultRiskLevel cannot exceed 20 characters")]
    public string? DefaultRiskLevel { get; set; }

    /// <summary>Minimum lead time in days</summary>
    [Range(0, 365, ErrorMessage = "LeadTimeDays must be between 0 and 365")]
    public int? LeadTimeDays { get; set; }

    /// <summary>Whether this change type is active</summary>
    public bool? IsActive { get; set; }
}
