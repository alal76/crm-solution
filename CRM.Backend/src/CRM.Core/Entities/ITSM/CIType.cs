// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Entities.ITSM;

/// <summary>
/// Represents a Configuration Item Type definition for categorizing CIs in the CMDB.
/// Named CITypeDefinition to avoid conflict with the CIType enum in ConfigurationItem.cs.
/// </summary>
public class CITypeDefinition : BaseEntity
{
    /// <summary>
    /// Gets or sets the name of the CI type (e.g., Server, Workstation, Application).
    /// </summary>
    [Required]
    [StringLength(100)]
    public string TypeName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category of the CI type (e.g., Hardware, Software, Service, Facility).
    /// </summary>
    [Required]
    [StringLength(50)]
    public string TypeCategory { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the CI type.
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the icon name for UI display (e.g., Material Icons name).
    /// </summary>
    [StringLength(50)]
    public string? IconName { get; set; }

    /// <summary>
    /// Gets or sets the color code for UI display (e.g., #2196F3).
    /// </summary>
    [StringLength(20)]
    public string? Color { get; set; }

    /// <summary>
    /// Gets or sets the sort order for display purposes.
    /// </summary>
    public int SortOrder { get; set; } = 0;

    /// <summary>
    /// Gets or sets whether this CI type is active and available for use.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
