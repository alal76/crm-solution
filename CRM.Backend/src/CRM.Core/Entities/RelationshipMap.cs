// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Entities;

/// <summary>
/// Saved relationship map configuration for visualization
/// </summary>
public class RelationshipMap : BaseEntity
{
    /// <summary>
    /// Name of the saved map
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string MapName { get; set; } = string.Empty;

    /// <summary>
    /// Description of the map
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The central customer/account for the map
    /// </summary>
    public int? CentralAccountId { get; set; }

    /// <summary>
    /// How many relationship levels to show (1-5)
    /// </summary>
    public int RelationshipDepth { get; set; } = 2;

    /// <summary>
    /// JSON array of relationship type IDs to include
    /// </summary>
    public string? IncludeRelationshipTypeIds { get; set; }

    /// <summary>
    /// JSON array of relationship type IDs to exclude
    /// </summary>
    public string? ExcludeRelationshipTypeIds { get; set; }

    /// <summary>
    /// Minimum strength score to show (0-100)
    /// </summary>
    public int MinRelationshipStrength { get; set; } = 0;

    /// <summary>
    /// JSON array of statuses to include
    /// </summary>
    public string? IncludeStatuses { get; set; }

    /// <summary>
    /// Start of date range filter
    /// </summary>
    public DateTime? DateRangeStart { get; set; }

    /// <summary>
    /// End of date range filter
    /// </summary>
    public DateTime? DateRangeEnd { get; set; }

    /// <summary>
    /// Layout configuration as JSON (node positions, colors, etc.)
    /// </summary>
    public string? LayoutConfig { get; set; }

    /// <summary>
    /// View settings as JSON (zoom, pan, etc.)
    /// </summary>
    public string? ViewSettings { get; set; }

    /// <summary>
    /// Whether this map is public
    /// </summary>
    public bool IsPublic { get; set; } = false;

    /// <summary>
    /// JSON array of user IDs this map is shared with
    /// </summary>
    public string? SharedWithUserIds { get; set; }

    /// <summary>
    /// JSON array of group IDs this map is shared with
    /// </summary>
    public string? SharedWithGroupIds { get; set; }

    // Navigation property
    public virtual Account? CentralAccount { get; set; }
}
