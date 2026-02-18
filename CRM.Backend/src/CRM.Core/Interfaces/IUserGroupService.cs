// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

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
}
