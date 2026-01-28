// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under the GNU Affero General Public License v3.0

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
    public int? CentralCustomerId { get; set; }
    
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
    public virtual Customer? CentralCustomer { get; set; }
}
