namespace CRM.Core.Dtos;

/// <summary>
/// DTO for user group operations
/// </summary>
public class UserGroupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int MemberCount { get; set; }
}

/// <summary>
/// DTO for creating/updating user groups
/// </summary>
public class CreateUserGroupRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// DTO for group member information
/// </summary>
public class UserGroupMemberDto
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; }
}
