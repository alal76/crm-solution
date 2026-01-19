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
    Task<UserDto> UpdateUserAsync(int id, UserDto userDto);
    Task DeleteUserAsync(int id);
    Task<bool> VerifyPasswordAsync(int userId, string password);
    Task ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    Task<User?> GetUserEntityByIdAsync(int id);
}
