namespace CRM.Core.Entities;

/// <summary>
/// Department entity for organizing users and access control
/// </summary>
public class Department : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? DepartmentCode { get; set; } // e.g., "SALES", "SUPPORT"
    public bool IsActive { get; set; } = true;
    public int? ParentDepartmentId { get; set; } // For hierarchical departments

    // Navigation properties
    public virtual ICollection<User> Users { get; set; } = new List<User>();
    public virtual ICollection<UserProfile> Profiles { get; set; } = new List<UserProfile>();
    public virtual Department? ParentDepartment { get; set; }
    public virtual ICollection<Department> SubDepartments { get; set; } = new List<Department>();
}
