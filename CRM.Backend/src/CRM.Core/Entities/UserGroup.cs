namespace CRM.Core.Entities;

/// <summary>
/// User groups for organizing users and managing permissions collectively
/// </summary>
public class UserGroup : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<UserGroupMember> Members { get; set; } = new List<UserGroupMember>();
}
