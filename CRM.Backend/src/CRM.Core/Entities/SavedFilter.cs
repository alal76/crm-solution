// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Entities;

/// <summary>
/// Represents a saved search/filter preset that users can create and reuse.
/// Stored as serialized JSON filter criteria.
/// </summary>
public class SavedFilter : BaseEntity
{
    /// <summary>
    /// Display name for the saved filter.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Entity type this filter applies to (e.g., "Account", "Contact", "Opportunity", "Lead").
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Serialized JSON of filter criteria.
    /// Example: [{"field":"Status","operator":"equals","value":"Active"},{"field":"Revenue","operator":"greaterThan","value":"10000"}]
    /// </summary>
    [Required]
    public string FilterCriteriaJson { get; set; } = "[]";

    /// <summary>
    /// Serialized JSON of sort configuration.
    /// Example: {"field":"Name","direction":"asc"}
    /// </summary>
    public string? SortConfigJson { get; set; }

    /// <summary>
    /// User ID of the filter owner.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Whether this filter is shared with all users (public) or private to the owner.
    /// </summary>
    public bool IsPublic { get; set; } = false;

    /// <summary>
    /// Whether this filter is pinned/favorited by the owner.
    /// </summary>
    public bool IsPinned { get; set; } = false;

    /// <summary>
    /// Optional description of what this filter does.
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Number of times this filter has been used.
    /// </summary>
    public int UsageCount { get; set; } = 0;

    /// <summary>
    /// Last time this filter was applied.
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// Navigation property to the owning user.
    /// </summary>
    public virtual User? User { get; set; }
}
