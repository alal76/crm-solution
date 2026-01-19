namespace CRM.Core.Entities;

/// <summary>
/// Membership of users in groups
/// </summary>
public class UserGroupMember : BaseEntity
{
    public int UserId { get; set; }
    public int UserGroupId { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual User? User { get; set; }
    public virtual UserGroup? UserGroup { get; set; }
}
