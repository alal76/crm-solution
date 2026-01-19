using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Interface for user group management
/// </summary>
public interface IUserGroupService
{
    Task<IEnumerable<UserGroupDto>> GetAllGroupsAsync();
    Task<UserGroupDto?> GetGroupByIdAsync(int id);
    Task<UserGroupDto> CreateGroupAsync(CreateUserGroupRequest request);
    Task<UserGroupDto?> UpdateGroupAsync(int id, CreateUserGroupRequest request);
    Task DeleteGroupAsync(int id);
    Task<IEnumerable<UserGroupMemberDto>> GetGroupMembersAsync(int groupId);
    Task AddUserToGroupAsync(int groupId, int userId);
    Task RemoveUserFromGroupAsync(int groupId, int userId);
}
