using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Entities;

/// <summary>
/// Department entity for organizing users and access control
/// </summary>
public class Department : BaseEntity
{
    /// <summary>Department name</summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>Department description</summary>
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    /// <summary>Department code e.g., "SALES", "SUPPORT"</summary>
    [MaxLength(20)]
    public string? DepartmentCode { get; set; }
    
    /// <summary>Whether department is active</summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>Parent department ID for hierarchical departments</summary>
    public int? ParentDepartmentId { get; set; }

    // Navigation properties
    public virtual ICollection<User> Users { get; set; } = new List<User>();
    public virtual ICollection<UserProfile> Profiles { get; set; } = new List<UserProfile>();
    public virtual Department? ParentDepartment { get; set; }
    public virtual ICollection<Department> SubDepartments { get; set; } = new List<Department>();
}
