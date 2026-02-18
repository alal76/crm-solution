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
using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Interface for user management operations
/// </summary>
public interface IUserService
{
    Task<UserDto?> GetUserByIdAsync(int id);
    Task<UserDto?> GetUserByEmailAsync(string email);
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserDto> CreateUserAsync(string email, string firstName, string lastName, string password, int roleId = 2);
    Task<UserDto> CreateUserWithoutPasswordAsync(string email, string firstName, string lastName, int roleId = 2);
    Task<UserDto> UpdateUserAsync(int id, UserDto userDto);
    Task DeleteUserAsync(int id);
    Task<bool> VerifyPasswordAsync(int userId, string password);
    Task ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    Task<User?> GetUserEntityByIdAsync(int id);
    Task<UserDto?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<bool> IsUserActiveAsync(int userId, CancellationToken cancellationToken = default);
}
