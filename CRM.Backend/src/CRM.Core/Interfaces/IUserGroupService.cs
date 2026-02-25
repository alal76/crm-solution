// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
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
    Task<bool> IsUserInGroupAsync(int userId, int groupId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserGroupDto>> GetActiveGroupsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates and normalizes the accessible menu items for a group, removing any unrecognized keys. (TODO-SYS012-002)
    /// </summary>
    Task<UserGroupDto?> ValidateAndNormalizeGroupPermissionsAsync(int groupId, CancellationToken cancellationToken = default);
}
