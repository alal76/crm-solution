using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for user group management
/// </summary>
public class UserGroupService : IUserGroupService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<UserGroupService> _logger;

    public UserGroupService(ICrmDbContext context, ILogger<UserGroupService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<UserGroupDto>> GetAllGroupsAsync()
    {
        try
        {
            var groups = await _context.UserGroups
                .Where(g => g.IsActive)
                .Select(g => new UserGroupDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    Description = g.Description,
                    IsActive = g.IsActive,
                    CreatedAt = g.CreatedAt,
                    MemberCount = g.Members.Count
                })
                .ToListAsync();

            return groups;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user groups");
            throw;
        }
    }

    public async Task<UserGroupDto?> GetGroupByIdAsync(int id)
    {
        try
        {
            var group = await _context.UserGroups
                .Where(g => g.Id == id)
                .Select(g => new UserGroupDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    Description = g.Description,
                    IsActive = g.IsActive,
                    CreatedAt = g.CreatedAt,
                    MemberCount = g.Members.Count
                })
                .FirstOrDefaultAsync();

            return group;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving group {id}");
            throw;
        }
    }

    public async Task<UserGroupDto> CreateGroupAsync(CreateUserGroupRequest request)
    {
        try
        {
            var existingGroup = await _context.UserGroups
                .FirstOrDefaultAsync(g => g.Name == request.Name);

            if (existingGroup != null)
                throw new InvalidOperationException("Group with this name already exists");

            var group = new UserGroup
            {
                Name = request.Name,
                Description = request.Description,
                IsActive = request.IsActive
            };

            _context.UserGroups.Add(group);
            await _context.SaveChangesAsync();

            return new UserGroupDto
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description,
                IsActive = group.IsActive,
                CreatedAt = group.CreatedAt,
                MemberCount = 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error creating user group {request.Name}");
            throw;
        }
    }

    public async Task<UserGroupDto?> UpdateGroupAsync(int id, CreateUserGroupRequest request)
    {
        try
        {
            var group = await _context.UserGroups.FindAsync(id);
            if (group == null)
                throw new KeyNotFoundException($"Group with ID {id} not found");

            group.Name = request.Name;
            group.Description = request.Description;
            group.IsActive = request.IsActive;

            _context.UserGroups.Update(group);
            await _context.SaveChangesAsync();

            return new UserGroupDto
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description,
                IsActive = group.IsActive,
                CreatedAt = group.CreatedAt,
                MemberCount = group.Members.Count
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating group {id}");
            throw;
        }
    }

    public async Task DeleteGroupAsync(int id)
    {
        try
        {
            var group = await _context.UserGroups.FindAsync(id);
            if (group == null)
                throw new KeyNotFoundException($"Group with ID {id} not found");

            _context.UserGroups.Remove(group);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting group {id}");
            throw;
        }
    }

    public async Task<IEnumerable<UserGroupMemberDto>> GetGroupMembersAsync(int groupId)
    {
        try
        {
            var members = await _context.UserGroupMembers
                .Where(m => m.UserGroupId == groupId)
                .Include(m => m.User)
                .Select(m => new UserGroupMemberDto
                {
                    UserId = m.UserId,
                    Email = m.User != null ? m.User.Email : string.Empty,
                    FullName = m.User != null ? $"{m.User.FirstName} {m.User.LastName}" : string.Empty,
                    AddedAt = m.AddedAt
                })
                .ToListAsync();

            return members;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving members for group {groupId}");
            throw;
        }
    }

    public async Task AddUserToGroupAsync(int groupId, int userId)
    {
        try
        {
            var group = await _context.UserGroups.FindAsync(groupId);
            if (group == null)
                throw new KeyNotFoundException($"Group with ID {groupId} not found");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found");

            var existingMember = await _context.UserGroupMembers
                .FirstOrDefaultAsync(m => m.UserGroupId == groupId && m.UserId == userId);

            if (existingMember != null)
                throw new InvalidOperationException("User is already a member of this group");

            var member = new UserGroupMember
            {
                UserGroupId = groupId,
                UserId = userId,
                AddedAt = DateTime.UtcNow
            };

            _context.UserGroupMembers.Add(member);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error adding user {userId} to group {groupId}");
            throw;
        }
    }

    public async Task RemoveUserFromGroupAsync(int groupId, int userId)
    {
        try
        {
            var member = await _context.UserGroupMembers
                .FirstOrDefaultAsync(m => m.UserGroupId == groupId && m.UserId == userId);

            if (member == null)
                throw new KeyNotFoundException($"User {userId} is not a member of group {groupId}");

            _context.UserGroupMembers.Remove(member);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error removing user {userId} from group {groupId}");
            throw;
        }
    }
}
