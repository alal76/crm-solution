// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text.Json;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CRM.Core.Ports.Input;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for user group management.
///
/// HEXAGONAL ARCHITECTURE:
/// - Implements IUserGroupInputPort (primary/driving port)
/// - Implements IUserGroupService (backward compatibility)
/// - Uses ICrmDbContext (secondary/driven port)
/// </summary>
public class UserGroupService : IUserGroupService, IUserGroupInputPort
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<UserGroupService> _logger;
    private readonly IAuditLogService? _auditLogService;

    public UserGroupService(
        ICrmDbContext context,
        ILogger<UserGroupService> logger,
        IAuditLogService? auditLogService = null)
    {
        _context = context;
        _logger = logger;
        _auditLogService = auditLogService;
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
            HeaderColor = g.HeaderColor,
            IsSystemAdmin = g.IsSystemAdmin,
            IsApiGroup = g.IsApiGroup,
            CreatedAt = g.CreatedAt,
            MemberCount = g.Members.Count,

            // Menu Permissions
            CanAccessDashboard = g.CanAccessDashboard,
            CanAccessAccounts = g.CanAccessAccounts,
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
            CanAccessServiceRequests = g.CanAccessServiceRequests,
            CanAccessITSM = g.CanAccessITSM,
            CanAccessReports = g.CanAccessReports,
            CanAccessSettings = g.CanAccessSettings,
            CanAccessUserManagement = g.CanAccessUserManagement,

            // Entity CRUD
            CanCreateAccounts = g.CanCreateAccounts,
            CanEditAccounts = g.CanEditAccounts,
            CanDeleteAccounts = g.CanDeleteAccounts,
            CanViewAllAccounts = g.CanViewAllAccounts,

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
        group.HeaderColor = request.HeaderColor;
        group.IsSystemAdmin = request.IsSystemAdmin;
        group.IsApiGroup = request.IsApiGroup;

        // Menu Permissions
        group.CanAccessDashboard = request.CanAccessDashboard;
        group.CanAccessAccounts = request.CanAccessAccounts;
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
        group.CanAccessServiceRequests = request.CanAccessServiceRequests;
        group.CanAccessITSM = request.CanAccessITSM;
        group.CanAccessReports = request.CanAccessReports;
        group.CanAccessSettings = request.CanAccessSettings;
        group.CanAccessUserManagement = request.CanAccessUserManagement;

        // Entity CRUD
        group.CanCreateAccounts = request.CanCreateAccounts;
        group.CanEditAccounts = request.CanEditAccounts;
        group.CanDeleteAccounts = request.CanDeleteAccounts;
        group.CanViewAllAccounts = request.CanViewAllAccounts;

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

        // AccessibleMenuItems — only update if provided (validation is performed by the caller)
        if (!string.IsNullOrWhiteSpace(request.AccessibleMenuItems))
        {
            group.AccessibleMenuItems = request.AccessibleMenuItems;
        }
    }

    public async Task<UserGroupDto> CreateGroupAsync(CreateUserGroupRequest request)
    {
        try
        {
            var existingGroup = await _context.UserGroups
                .FirstOrDefaultAsync(g => g.Name == request.Name);

            if (existingGroup != null)
            {
                throw new InvalidOperationException("Group with this name already exists");
            }

            // Enforce single default group rule (SYS003-001)
            if (request.IsDefault)
            {
                await ClearExistingDefaultGroupAsync();
            }

            // Validate AccessibleMenuItems JSON (SYS003-002)
            if (!string.IsNullOrWhiteSpace(request.AccessibleMenuItems))
            {
                var (isValid, validItems, invalidItems) = ValidateMenuItems(request.AccessibleMenuItems);
                if (!isValid)
                {
                    _logger.LogWarning(
                        "CreateGroup '{Name}': AccessibleMenuItems JSON is malformed — defaulting to empty list",
                        request.Name);
                    request.AccessibleMenuItems = "[]";
                }
                else if (invalidItems.Count > 0)
                {
                    _logger.LogWarning(
                        "CreateGroup '{Name}': Unknown menu keys [{Keys}] — they will be stored but may not render in the UI",
                        request.Name, string.Join(", ", invalidItems));
                }
            }

            var group = new UserGroup();
            MapFromRequest(group, request);

            _context.UserGroups.Add(group);
            await _context.SaveChangesAsync();

            // Audit log permission changes (SYS012-003)
            if (_auditLogService != null)
            {
                await _auditLogService.LogCreateAsync(
                    entityType: "UserGroup",
                    entityId: group.Id,
                    entityName: group.Name,
                    userId: null,
                    newValues: new Dictionary<string, object>
                    {
                        ["Name"] = group.Name,
                        ["IsSystemAdmin"] = group.IsSystemAdmin,
                        ["IsActive"] = group.IsActive,
                        ["IsDefault"] = group.IsDefault,
                        ["CanAccessDashboard"] = group.CanAccessDashboard,
                        ["CanAccessAccounts"] = group.CanAccessAccounts,
                        ["CanAccessContacts"] = group.CanAccessContacts,
                        ["CanAccessSettings"] = group.CanAccessSettings,
                        ["CanAccessUserManagement"] = group.CanAccessUserManagement,
                        ["DataAccessScope"] = group.DataAccessScope ?? "Own"
                    });
            }

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
            {
                throw new KeyNotFoundException($"Group with ID {id} not found");
            }

            // Enforce single default group rule (SYS003-001)
            if (request.IsDefault && !group.IsDefault)
            {
                // Promoting this group to default — unset any existing default
                await ClearExistingDefaultGroupAsync(excludeId: id);
            }
            else if (!request.IsDefault && group.IsDefault)
            {
                // Attempting to unset the current default — only allowed if another group will become default
                var otherDefaultExists = await _context.UserGroups
                    .AnyAsync(g => g.Id != id && g.IsDefault && !g.IsDeleted);
                if (!otherDefaultExists)
                {
                    throw new InvalidOperationException(
                        "Cannot unset the only default group. Promote another group to default first.");
                }
            }

            // Validate AccessibleMenuItems JSON (SYS003-002)
            if (!string.IsNullOrWhiteSpace(request.AccessibleMenuItems))
            {
                var (isValid, validItems, invalidItems) = ValidateMenuItems(request.AccessibleMenuItems);
                if (!isValid)
                {
                    _logger.LogWarning(
                        "UpdateGroup {Id}: AccessibleMenuItems JSON is malformed — keeping previous value",
                        id);
                    // Preserve the existing value rather than corrupting it
                    request.AccessibleMenuItems = group.AccessibleMenuItems;
                }
                else if (invalidItems.Count > 0)
                {
                    _logger.LogWarning(
                        "UpdateGroup {Id}: Unknown menu keys [{Keys}] — they will be stored but may not render in the UI",
                        id, string.Join(", ", invalidItems));
                }
            }

            // Capture old values for audit
            var oldValues = new Dictionary<string, object>
            {
                ["Name"] = group.Name,
                ["IsSystemAdmin"] = group.IsSystemAdmin,
                ["IsActive"] = group.IsActive,
                ["IsDefault"] = group.IsDefault,
                ["CanAccessDashboard"] = group.CanAccessDashboard,
                ["CanAccessAccounts"] = group.CanAccessAccounts,
                ["CanAccessContacts"] = group.CanAccessContacts,
                ["CanAccessSettings"] = group.CanAccessSettings,
                ["CanAccessUserManagement"] = group.CanAccessUserManagement,
                ["DataAccessScope"] = group.DataAccessScope ?? "Own"
            };

            MapFromRequest(group, request);

            _context.UserGroups.Update(group);
            await _context.SaveChangesAsync();

            // Audit log permission changes (SYS012-003)
            if (_auditLogService != null)
            {
                var newValues = new Dictionary<string, object>
                {
                    ["Name"] = group.Name,
                    ["IsSystemAdmin"] = group.IsSystemAdmin,
                    ["IsActive"] = group.IsActive,
                    ["IsDefault"] = group.IsDefault,
                    ["CanAccessDashboard"] = group.CanAccessDashboard,
                    ["CanAccessAccounts"] = group.CanAccessAccounts,
                    ["CanAccessContacts"] = group.CanAccessContacts,
                    ["CanAccessSettings"] = group.CanAccessSettings,
                    ["CanAccessUserManagement"] = group.CanAccessUserManagement,
                    ["DataAccessScope"] = group.DataAccessScope ?? "Own"
                };

                await _auditLogService.LogUpdateAsync(
                    entityType: "UserGroup",
                    entityId: group.Id,
                    entityName: group.Name,
                    userId: null,
                    oldValues: oldValues,
                    newValues: newValues,
                    changedProperties: newValues.Keys
                        .Where(k => !oldValues.ContainsKey(k) || !Equals(oldValues[k], newValues[k]))
                        .ToList());
            }

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
            {
                throw new KeyNotFoundException($"Group with ID {id} not found");
            }

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
            {
                throw new KeyNotFoundException($"Group with ID {groupId} not found");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            var existingMember = await _context.UserGroupMembers
                .FirstOrDefaultAsync(m => m.UserGroupId == groupId && m.UserId == userId);

            if (existingMember != null)
            {
                throw new InvalidOperationException("User is already a member of this group");
            }

            var member = new UserGroupMember
            {
                UserGroupId = groupId,
                UserId = userId,
                AddedAt = DateTime.UtcNow
            };

            _context.UserGroupMembers.Add(member);
            await _context.SaveChangesAsync();

            // Audit log for group membership changes (SYS003-003)
            if (_auditLogService != null)
            {
                await _auditLogService.LogActionAsync(
                    "UserAddedToGroup",
                    "UserGroup",
                    groupId,
                    userId,
                    $"User {userId} added to group '{group.Name}' (ID: {groupId})");
            }
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
            {
                throw new KeyNotFoundException($"User {userId} is not a member of group {groupId}");
            }

            _context.UserGroupMembers.Remove(member);
            await _context.SaveChangesAsync();

            // Audit log for group membership changes (SYS003-003)
            if (_auditLogService != null)
            {
                var group = await _context.UserGroups.FindAsync(groupId);
                await _auditLogService.LogActionAsync(
                    "UserRemovedFromGroup",
                    "UserGroup",
                    groupId,
                    userId,
                    $"User {userId} removed from group '{group?.Name ?? "Unknown"}' (ID: {groupId})");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error removing user {userId} from group {groupId}");
            throw;
        }
    }

    public async Task<bool> IsUserInGroupAsync(int userId, int groupId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.UserGroupMembers
                .AnyAsync(m => m.UserId == userId && m.UserGroupId == groupId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user {UserId} is in group {GroupId}", userId, groupId);
            throw;
        }
    }

    public async Task<IEnumerable<UserGroupDto>> GetActiveGroupsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var groups = await _context.UserGroups
                .Where(g => g.IsActive)
                .Select(g => MapToDto(g))
                .ToListAsync(cancellationToken);

            return groups;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active user groups");
            throw;
        }
    }

    // ─── Private helpers ─────────────────────────────────────────────────────

    /// <summary>
    /// Ensures only one group is marked as IsDefault (SYS003-001) and validates and
    /// normalizes the accessible menu items, removing unrecognized keys (SYS012-002).
    /// </summary>
    public async Task<UserGroupDto?> ValidateAndNormalizeGroupPermissionsAsync(int groupId, CancellationToken cancellationToken = default)
    {
        var group = await _context.UserGroups
            .FirstOrDefaultAsync(g => g.Id == groupId && !g.IsDeleted, cancellationToken);

        if (group == null)
        {
            return null;
        }

        var (isValid, validItems, _) = ValidateMenuItems(group.AccessibleMenuItems);

        if (!isValid)
        {
            // Malformed JSON – reset to empty list
            group.AccessibleMenuItems = "[]";
        }
        else
        {
            group.AccessibleMenuItems = JsonSerializer.Serialize(validItems);
        }

        _context.UserGroups.Update(group);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Normalized menu permissions for group {GroupId}: {ValidCount} item(s) kept", groupId, validItems.Count);
        return MapToDto(group);
    }

    /// <summary>
    /// Unsets IsDefault on all groups except <paramref name="excludeId"/>.
    /// </summary>
    private async Task ClearExistingDefaultGroupAsync(int excludeId = 0)
    {
        var currentDefaults = await _context.UserGroups
            .Where(g => g.IsDefault && !g.IsDeleted && g.Id != excludeId)
            .ToListAsync();

        foreach (var g in currentDefaults)
        {
            g.IsDefault = false;
            _context.UserGroups.Update(g);
        }
        // Caller is responsible for calling SaveChangesAsync
    }

    /// <summary>
    /// Validates that AccessibleMenuItems is well-formed JSON containing known menu keys (SYS003-002).
    /// Returns:  isValid — whether the JSON could be parsed;
    ///           validItems — keys that match known navigation items;
    ///           invalidItems — keys that don't match any known navigation item.
    /// </summary>
    public static (bool isValid, List<string> validItems, List<string> invalidItems) ValidateMenuItems(string json)
    {
        // Known navigation keys (sourced from DbSeed + frontend navigation config)
        var knownKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Dashboard", "Accounts", "Contacts", "Leads", "Opportunities",
            "Products", "Services", "Campaigns", "Quotes", "Tasks",
            "Activities", "Notes", "Workflows", "ServiceRequests",
            "Reports", "Settings", "UserManagement", "Admin",
            "ITSM", "Incidents", "Problems", "Changes", "CMDB",
            "KnowledgeBase", "ServiceCatalog", "Contracts", "Invoices",
            "Orders", "Payments", "Subscriptions"
        };

        try
        {
            var items = JsonSerializer.Deserialize<List<string>>(json);
            if (items == null)
            {
                return (false, new List<string>(), new List<string>());
            }

            var valid = items.Where(k => knownKeys.Contains(k)).ToList();
            var invalid = items.Where(k => !knownKeys.Contains(k)).ToList();
            return (true, valid, invalid);
        }
        catch (JsonException)
        {
            return (false, new List<string>(), new List<string>());
        }
    }
}
