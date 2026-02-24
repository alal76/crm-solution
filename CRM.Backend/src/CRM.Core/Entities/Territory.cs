// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Core.Entities;

/// <summary>
/// Territory type for geographic or account-based assignment.
/// TODO-GAP-04: Territory-based lead assignment
/// </summary>
public enum TerritoryType
{
    /// <summary>Geographic region</summary>
    Geographic = 0,
    
    /// <summary>Industry vertical</summary>
    Industry = 1,
    
    /// <summary>Account-based (named accounts)</summary>
    Named = 2,
    
    /// <summary>Company size tier</summary>
    Segment = 3,
    
    /// <summary>Product-based territory</summary>
    Product = 4,
    
    /// <summary>Hybrid/custom territory</summary>
    Custom = 99
}

/// <summary>
/// Sales territory entity for territory-based lead assignment.
/// TODO-GAP-04: Territory-based lead assignment
/// </summary>
[Table("Territories")]
public class Territory : BaseEntity
{
    /// <summary>Territory name</summary>
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>Territory code</summary>
    [MaxLength(50)]
    public string? Code { get; set; }
    
    /// <summary>Territory description</summary>
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    /// <summary>Type of territory</summary>
    public TerritoryType Type { get; set; } = TerritoryType.Geographic;
    
    /// <summary>Parent territory ID (for hierarchical territories)</summary>
    public int? ParentTerritoryId { get; set; }
    
    /// <summary>Primary owner user ID</summary>
    public int? OwnerUserId { get; set; }
    
    /// <summary>Whether territory is active</summary>
    public bool IsActive { get; set; } = true;
    
    #region Geographic Rules
    
    /// <summary>Countries included (JSON array or comma-separated)</summary>
    [MaxLength(1000)]
    public string? Countries { get; set; }
    
    /// <summary>States/regions included (JSON array or comma-separated)</summary>
    [MaxLength(2000)]
    public string? States { get; set; }
    
    /// <summary>Cities included (JSON array or comma-separated)</summary>
    [MaxLength(2000)]
    public string? Cities { get; set; }
    
    /// <summary>Postal code patterns (JSON array)</summary>
    [MaxLength(2000)]
    public string? PostalCodePatterns { get; set; }
    
    #endregion
    
    #region Segment Rules
    
    /// <summary>Industries included (JSON array)</summary>
    [MaxLength(1000)]
    public string? Industries { get; set; }
    
    /// <summary>Company size range minimum</summary>
    public int? MinCompanySize { get; set; }
    
    /// <summary>Company size range maximum</summary>
    public int? MaxCompanySize { get; set; }
    
    /// <summary>Annual revenue minimum</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MinRevenue { get; set; }
    
    /// <summary>Annual revenue maximum</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MaxRevenue { get; set; }
    
    #endregion
    
    /// <summary>Custom assignment rules (JSON)</summary>
    [MaxLength(4000)]
    public string? AssignmentRulesJson { get; set; }
    
    #region Navigation Properties
    
    /// <summary>Parent territory navigation</summary>
    [ForeignKey("ParentTerritoryId")]
    public virtual Territory? ParentTerritory { get; set; }
    
    /// <summary>Child territories</summary>
    public virtual ICollection<Territory> ChildTerritories { get; set; } = new List<Territory>();
    
    /// <summary>Owner user navigation</summary>
    [ForeignKey("OwnerUserId")]
    public virtual User? Owner { get; set; }
    
    /// <summary>Leads assigned to this territory</summary>
    public virtual ICollection<Lead> Leads { get; set; } = new List<Lead>();
    
    #endregion
}
