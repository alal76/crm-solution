// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Dtos.ITSM;

/// <summary>
/// DTO for CI Type response.
/// </summary>
public class CITypeDto
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the CI type.
    /// </summary>
    public string TypeName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category of the CI type.
    /// </summary>
    public string TypeCategory { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the icon name for UI display.
    /// </summary>
    public string? IconName { get; set; }

    /// <summary>
    /// Gets or sets the color code for UI display.
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Gets or sets the sort order.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Gets or sets whether this CI type is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating a new CI Type.
/// </summary>
public class CreateCITypeDto
{
    /// <summary>
    /// Gets or sets the name of the CI type.
    /// </summary>
    [Required]
    [StringLength(100)]
    public string TypeName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category of the CI type.
    /// </summary>
    [Required]
    [StringLength(50)]
    public string TypeCategory { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the icon name for UI display.
    /// </summary>
    [StringLength(50)]
    public string? IconName { get; set; }

    /// <summary>
    /// Gets or sets the color code for UI display.
    /// </summary>
    [StringLength(20)]
    public string? Color { get; set; }

    /// <summary>
    /// Gets or sets the sort order.
    /// </summary>
    public int SortOrder { get; set; } = 0;

    /// <summary>
    /// Gets or sets whether this CI type is active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// DTO for updating an existing CI Type.
/// </summary>
public class UpdateCITypeDto
{
    /// <summary>
    /// Gets or sets the name of the CI type.
    /// </summary>
    [StringLength(100)]
    public string? TypeName { get; set; }

    /// <summary>
    /// Gets or sets the category of the CI type.
    /// </summary>
    [StringLength(50)]
    public string? TypeCategory { get; set; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the icon name for UI display.
    /// </summary>
    [StringLength(50)]
    public string? IconName { get; set; }

    /// <summary>
    /// Gets or sets the color code for UI display.
    /// </summary>
    [StringLength(20)]
    public string? Color { get; set; }

    /// <summary>
    /// Gets or sets the sort order.
    /// </summary>
    public int? SortOrder { get; set; }

    /// <summary>
    /// Gets or sets whether this CI type is active.
    /// </summary>
    public bool? IsActive { get; set; }
}
