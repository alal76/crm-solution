// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
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
    Task<UserDto> CreateUserAsync(string email, string firstName, string lastName, string password, int roleId = 2, string? username = null);
    Task<UserDto> CreateUserWithoutPasswordAsync(string email, string firstName, string lastName, int roleId = 2, string? username = null);
    Task<UserDto> UpdateUserAsync(int id, UserDto userDto);
    Task DeleteUserAsync(int id);
    Task<bool> VerifyPasswordAsync(int userId, string password);
    Task ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    Task<User?> GetUserEntityByIdAsync(int id);
    Task<UserDto?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<bool> IsUserActiveAsync(int userId, CancellationToken cancellationToken = default);
}
