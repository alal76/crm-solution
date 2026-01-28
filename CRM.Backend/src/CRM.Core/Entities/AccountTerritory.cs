// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under the GNU Affero General Public License v3.0

using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Entities;

/// <summary>
/// Territory for account management and assignment
/// </summary>
public class AccountTerritory : BaseEntity
{
    /// <summary>
    /// Name of the territory
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string TerritoryName { get; set; } = string.Empty;
    
    /// <summary>
    /// Unique code for the territory
    /// </summary>
    [MaxLength(50)]
    public string? TerritoryCode { get; set; }
    
    /// <summary>
    /// Description of the territory
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// JSON array of countries in this territory
    /// </summary>
    public string? Countries { get; set; }
    
    /// <summary>
    /// JSON array of regions
    /// </summary>
    public string? Regions { get; set; }
    
    /// <summary>
    /// JSON array of states
    /// </summary>
    public string? States { get; set; }
    
    /// <summary>
    /// JSON array of cities
    /// </summary>
    public string? Cities { get; set; }
    
    /// <summary>
    /// JSON array of industries
    /// </summary>
    public string? Industries { get; set; }
    
    /// <summary>
    /// JSON array of customer types
    /// </summary>
    public string? CustomerTypes { get; set; }
    
    /// <summary>
    /// Minimum revenue range
    /// </summary>
    public decimal? RevenueRangeMin { get; set; }
    
    /// <summary>
    /// Maximum revenue range
    /// </summary>
    public decimal? RevenueRangeMax { get; set; }
    
    /// <summary>
    /// Primary owner user ID
    /// </summary>
    public int? PrimaryOwnerId { get; set; }
    
    /// <summary>
    /// JSON array of team member user IDs
    /// </summary>
    public string? TeamMemberIds { get; set; }
    
    /// <summary>
    /// Annual quota for the territory
    /// </summary>
    public decimal? AnnualQuota { get; set; }
    
    /// <summary>
    /// Currency for quota
    /// </summary>
    [MaxLength(10)]
    public string QuotaCurrency { get; set; } = "USD";
    
    /// <summary>
    /// Target number of accounts
    /// </summary>
    public int? TargetAccountCount { get; set; }
    
    /// <summary>
    /// Whether the territory is active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual User? PrimaryOwner { get; set; }
    public virtual ICollection<CustomerTerritoryAssignment> CustomerAssignments { get; set; } = new List<CustomerTerritoryAssignment>();
}

/// <summary>
/// Many-to-many assignment of customers to territories
/// </summary>
public class CustomerTerritoryAssignment
{
    /// <summary>
    /// The customer being assigned
    /// </summary>
    [Required]
    public int CustomerId { get; set; }
    
    /// <summary>
    /// The territory being assigned to
    /// </summary>
    [Required]
    public int TerritoryId { get; set; }
    
    /// <summary>
    /// Date of assignment
    /// </summary>
    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Whether this is the primary territory for the customer
    /// </summary>
    public bool IsPrimary { get; set; } = true;
    
    /// <summary>
    /// User who made the assignment
    /// </summary>
    public int? AssignedBy { get; set; }
    
    /// <summary>
    /// Notes about the assignment
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    // Navigation properties
    public virtual Customer? Customer { get; set; }
    public virtual AccountTerritory? Territory { get; set; }
    public virtual User? AssignedByUser { get; set; }
}
