using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BCrypt.Net;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for user management operations
/// </summary>
public class UserService : IUserService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<UserService> _logger;

    public UserService(ICrmDbContext context, ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        try
        {
            var user = await _context.Users
                .Where(u => u.Id == id)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Role = ((UserRole)u.Role).ToString(),
                    IsActive = u.IsActive,
                    LastLoginDate = u.LastLoginDate,
                    DepartmentId = u.DepartmentId,
                    UserProfileId = u.UserProfileId,
                    CreatedAt = u.CreatedAt
                })
                .FirstOrDefaultAsync();

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving user {id}");
            throw;
        }
    }

    public async Task<UserDto?> GetUserByEmailAsync(string email)
    {
        try
        {
            var user = await _context.Users
                .Where(u => u.Email == email)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Role = ((UserRole)u.Role).ToString(),
                    IsActive = u.IsActive,
                    LastLoginDate = u.LastLoginDate,
                    DepartmentId = u.DepartmentId,
                    UserProfileId = u.UserProfileId,
                    CreatedAt = u.CreatedAt
                })
                .FirstOrDefaultAsync();

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving user by email {email}");
            throw;
        }
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        try
        {
            var users = await _context.Users
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Role = ((UserRole)u.Role).ToString(),
                    IsActive = u.IsActive,
                    LastLoginDate = u.LastLoginDate,
                    DepartmentId = u.DepartmentId,
                    UserProfileId = u.UserProfileId,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            return users;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all users");
            throw;
        }
    }

    public async Task<UserDto> CreateUserAsync(string email, string firstName, string lastName, string password, int roleId = 2)
    {
        try
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (existingUser != null)
                throw new InvalidOperationException("User with this email already exists");

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            var user = new User
            {
                Email = email,
                Username = email,
                FirstName = firstName,
                LastName = lastName,
                PasswordHash = passwordHash,
                Role = roleId,
                IsActive = true,
                EmailVerified = false
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = ((UserRole)user.Role).ToString(),
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error creating user {email}");
            throw;
        }
    }

    public async Task<UserDto> UpdateUserAsync(int id, UserDto userDto)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {id} not found");

            user.FirstName = userDto.FirstName;
            user.LastName = userDto.LastName;
            user.IsActive = userDto.IsActive;
            if (!string.IsNullOrEmpty(userDto.Role))
            {
                user.Role = (int)Enum.Parse<UserRole>(userDto.Role);
            }
            user.DepartmentId = userDto.DepartmentId;
            user.UserProfileId = userDto.UserProfileId;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return userDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating user {id}");
            throw;
        }
    }

    public async Task DeleteUserAsync(int id)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {id} not found");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting user {id}");
            throw;
        }
    }

    public async Task<bool> VerifyPasswordAsync(int userId, string password)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error verifying password for user {userId}");
            return false;
        }
    }

    public async Task ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found");

            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
                throw new InvalidOperationException("Current password is incorrect");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error changing password for user {userId}");
            throw;
        }
    }

    public async Task<User?> GetUserEntityByIdAsync(int id)
    {
        try
        {
            return await _context.Users.FindAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving user entity {id}");
            throw;
        }
    }
}
