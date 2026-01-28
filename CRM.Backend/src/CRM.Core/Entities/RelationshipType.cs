// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under the GNU Affero General Public License v3.0

using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Entities;

/// <summary>
/// Relationship type category
/// </summary>
public enum RelationshipCategory
{
    Business = 0,
    Partnership = 1,
    Hierarchy = 2,
    Dependency = 3
}

/// <summary>
/// Defines the types of relationships that can exist between accounts/customers
/// </summary>
public class RelationshipType : BaseEntity
{
    /// <summary>
    /// Unique name of the relationship type
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string TypeName { get; set; } = string.Empty;
    
    /// <summary>
    /// Category of the relationship
    /// </summary>
    [MaxLength(50)]
    public string? TypeCategory { get; set; }
    
    /// <summary>
    /// Description of this relationship type
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Whether the relationship applies both ways
    /// </summary>
    public bool IsBidirectional { get; set; } = false;
    
    /// <summary>
    /// For bidirectional relationships, the reverse type name
    /// E.g., "Parent Company" has reverse "Subsidiary"
    /// </summary>
    [MaxLength(100)]
    public string? ReverseTypeName { get; set; }
    
    /// <summary>
    /// Icon name for UI display
    /// </summary>
    [MaxLength(50)]
    public string? Icon { get; set; }
    
    /// <summary>
    /// Color hex code for UI display
    /// </summary>
    [MaxLength(20)]
    public string? Color { get; set; }
    
    /// <summary>
    /// Whether this relationship type is active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// System-defined types cannot be deleted
    /// </summary>
    public bool IsSystem { get; set; } = false;
    
    /// <summary>
    /// Display order for UI sorting
    /// </summary>
    public int DisplayOrder { get; set; } = 0;
    
    // Navigation properties
    public virtual ICollection<AccountRelationship> Relationships { get; set; } = new List<AccountRelationship>();
}
