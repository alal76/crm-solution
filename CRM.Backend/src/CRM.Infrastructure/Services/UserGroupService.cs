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
                .Select(g => MapToDto(g))
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
                .FirstOrDefaultAsync();

            return group != null ? MapToDto(group) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving group {id}");
            throw;
        }
    }
    
    private static UserGroupDto MapToDto(UserGroup g)
    {
        return new UserGroupDto
        {
            Id = g.Id,
            Name = g.Name,
            Description = g.Description,
            IsActive = g.IsActive,
            IsDefault = g.IsDefault,
            DisplayOrder = g.DisplayOrder,
            IsSystemAdmin = g.IsSystemAdmin,
            CreatedAt = g.CreatedAt,
            MemberCount = g.Members.Count,
            
            // Menu Permissions
            CanAccessDashboard = g.CanAccessDashboard,
            CanAccessCustomers = g.CanAccessCustomers,
            CanAccessContacts = g.CanAccessContacts,
            CanAccessLeads = g.CanAccessLeads,
            CanAccessOpportunities = g.CanAccessOpportunities,
            CanAccessProducts = g.CanAccessProducts,
            CanAccessServices = g.CanAccessServices,
            CanAccessCampaigns = g.CanAccessCampaigns,
            CanAccessQuotes = g.CanAccessQuotes,
            CanAccessTasks = g.CanAccessTasks,
            CanAccessActivities = g.CanAccessActivities,
            CanAccessNotes = g.CanAccessNotes,
            CanAccessWorkflows = g.CanAccessWorkflows,
            CanAccessReports = g.CanAccessReports,
            CanAccessSettings = g.CanAccessSettings,
            CanAccessUserManagement = g.CanAccessUserManagement,
            
            // Entity CRUD
            CanCreateCustomers = g.CanCreateCustomers,
            CanEditCustomers = g.CanEditCustomers,
            CanDeleteCustomers = g.CanDeleteCustomers,
            CanViewAllCustomers = g.CanViewAllCustomers,
            
            CanCreateContacts = g.CanCreateContacts,
            CanEditContacts = g.CanEditContacts,
            CanDeleteContacts = g.CanDeleteContacts,
            
            CanCreateLeads = g.CanCreateLeads,
            CanEditLeads = g.CanEditLeads,
            CanDeleteLeads = g.CanDeleteLeads,
            CanConvertLeads = g.CanConvertLeads,
            
            CanCreateOpportunities = g.CanCreateOpportunities,
            CanEditOpportunities = g.CanEditOpportunities,
            CanDeleteOpportunities = g.CanDeleteOpportunities,
            CanCloseOpportunities = g.CanCloseOpportunities,
            
            CanCreateProducts = g.CanCreateProducts,
            CanEditProducts = g.CanEditProducts,
            CanDeleteProducts = g.CanDeleteProducts,
            CanManagePricing = g.CanManagePricing,
            
            CanCreateCampaigns = g.CanCreateCampaigns,
            CanEditCampaigns = g.CanEditCampaigns,
            CanDeleteCampaigns = g.CanDeleteCampaigns,
            CanLaunchCampaigns = g.CanLaunchCampaigns,
            
            CanCreateQuotes = g.CanCreateQuotes,
            CanEditQuotes = g.CanEditQuotes,
            CanDeleteQuotes = g.CanDeleteQuotes,
            CanApproveQuotes = g.CanApproveQuotes,
            
            CanCreateTasks = g.CanCreateTasks,
            CanEditTasks = g.CanEditTasks,
            CanDeleteTasks = g.CanDeleteTasks,
            CanAssignTasks = g.CanAssignTasks,
            
            CanCreateWorkflows = g.CanCreateWorkflows,
            CanEditWorkflows = g.CanEditWorkflows,
            CanDeleteWorkflows = g.CanDeleteWorkflows,
            CanActivateWorkflows = g.CanActivateWorkflows,
            
            // Data Access
            DataAccessScope = g.DataAccessScope,
            CanExportData = g.CanExportData,
            CanImportData = g.CanImportData,
            CanBulkEdit = g.CanBulkEdit,
            CanBulkDelete = g.CanBulkDelete
        };
    }
    
    private static void MapFromRequest(UserGroup group, CreateUserGroupRequest request)
    {
        group.Name = request.Name;
        group.Description = request.Description;
        group.IsActive = request.IsActive;
        group.IsDefault = request.IsDefault;
        group.DisplayOrder = request.DisplayOrder;
        group.IsSystemAdmin = request.IsSystemAdmin;
        
        // Menu Permissions
        group.CanAccessDashboard = request.CanAccessDashboard;
        group.CanAccessCustomers = request.CanAccessCustomers;
        group.CanAccessContacts = request.CanAccessContacts;
        group.CanAccessLeads = request.CanAccessLeads;
        group.CanAccessOpportunities = request.CanAccessOpportunities;
        group.CanAccessProducts = request.CanAccessProducts;
        group.CanAccessServices = request.CanAccessServices;
        group.CanAccessCampaigns = request.CanAccessCampaigns;
        group.CanAccessQuotes = request.CanAccessQuotes;
        group.CanAccessTasks = request.CanAccessTasks;
        group.CanAccessActivities = request.CanAccessActivities;
        group.CanAccessNotes = request.CanAccessNotes;
        group.CanAccessWorkflows = request.CanAccessWorkflows;
        group.CanAccessReports = request.CanAccessReports;
        group.CanAccessSettings = request.CanAccessSettings;
        group.CanAccessUserManagement = request.CanAccessUserManagement;
        
        // Entity CRUD
        group.CanCreateCustomers = request.CanCreateCustomers;
        group.CanEditCustomers = request.CanEditCustomers;
        group.CanDeleteCustomers = request.CanDeleteCustomers;
        group.CanViewAllCustomers = request.CanViewAllCustomers;
        
        group.CanCreateContacts = request.CanCreateContacts;
        group.CanEditContacts = request.CanEditContacts;
        group.CanDeleteContacts = request.CanDeleteContacts;
        
        group.CanCreateLeads = request.CanCreateLeads;
        group.CanEditLeads = request.CanEditLeads;
        group.CanDeleteLeads = request.CanDeleteLeads;
        group.CanConvertLeads = request.CanConvertLeads;
        
        group.CanCreateOpportunities = request.CanCreateOpportunities;
        group.CanEditOpportunities = request.CanEditOpportunities;
        group.CanDeleteOpportunities = request.CanDeleteOpportunities;
        group.CanCloseOpportunities = request.CanCloseOpportunities;
        
        group.CanCreateProducts = request.CanCreateProducts;
        group.CanEditProducts = request.CanEditProducts;
        group.CanDeleteProducts = request.CanDeleteProducts;
        group.CanManagePricing = request.CanManagePricing;
        
        group.CanCreateCampaigns = request.CanCreateCampaigns;
        group.CanEditCampaigns = request.CanEditCampaigns;
        group.CanDeleteCampaigns = request.CanDeleteCampaigns;
        group.CanLaunchCampaigns = request.CanLaunchCampaigns;
        
        group.CanCreateQuotes = request.CanCreateQuotes;
        group.CanEditQuotes = request.CanEditQuotes;
        group.CanDeleteQuotes = request.CanDeleteQuotes;
        group.CanApproveQuotes = request.CanApproveQuotes;
        
        group.CanCreateTasks = request.CanCreateTasks;
        group.CanEditTasks = request.CanEditTasks;
        group.CanDeleteTasks = request.CanDeleteTasks;
        group.CanAssignTasks = request.CanAssignTasks;
        
        group.CanCreateWorkflows = request.CanCreateWorkflows;
        group.CanEditWorkflows = request.CanEditWorkflows;
        group.CanDeleteWorkflows = request.CanDeleteWorkflows;
        group.CanActivateWorkflows = request.CanActivateWorkflows;
        
        // Data Access
        group.DataAccessScope = request.DataAccessScope;
        group.CanExportData = request.CanExportData;
        group.CanImportData = request.CanImportData;
        group.CanBulkEdit = request.CanBulkEdit;
        group.CanBulkDelete = request.CanBulkDelete;
    }

    public async Task<UserGroupDto> CreateGroupAsync(CreateUserGroupRequest request)
    {
        try
        {
            var existingGroup = await _context.UserGroups
                .FirstOrDefaultAsync(g => g.Name == request.Name);

            if (existingGroup != null)
                throw new InvalidOperationException("Group with this name already exists");

            var group = new UserGroup();
            MapFromRequest(group, request);

            _context.UserGroups.Add(group);
            await _context.SaveChangesAsync();

            return MapToDto(group);
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

            MapFromRequest(group, request);

            _context.UserGroups.Update(group);
            await _context.SaveChangesAsync();

            return MapToDto(group);
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
