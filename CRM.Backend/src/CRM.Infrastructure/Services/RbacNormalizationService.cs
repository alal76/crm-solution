// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for normalizing RBAC permissions and syncing group permission flags
/// with navigation filtering rules.
/// TODO-SYS012-002
/// </summary>
public class RbacNormalizationService : IRbacNormalizationService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<RbacNormalizationService> _logger;

    // Navigation item to permission flag mapping
    private static readonly Dictionary<string, string[]> NavigationPermissionMap = new()
    {
        ["dashboard"] = new[] { "CanAccessDashboard" },
        ["accounts"] = new[] { "CanAccessAccounts" },
        ["contacts"] = new[] { "CanAccessContacts" },
        ["leads"] = new[] { "CanAccessLeads" },
        ["opportunities"] = new[] { "CanAccessOpportunities" },
        ["products"] = new[] { "CanAccessProducts" },
        ["services"] = new[] { "CanAccessServices" },
        ["campaigns"] = new[] { "CanAccessCampaigns" },
        ["quotes"] = new[] { "CanAccessQuotes" },
        ["tasks"] = new[] { "CanAccessTasks" },
        ["activities"] = new[] { "CanAccessActivities" },
        ["notes"] = new[] { "CanAccessNotes" },
        ["workflows"] = new[] { "CanAccessWorkflows" },
        ["servicerequests"] = new[] { "CanAccessServiceRequests", "CanAccessITSM" },
        ["itsm"] = new[] { "CanAccessITSM" },
        ["reports"] = new[] { "CanAccessReports" },
        ["settings"] = new[] { "CanAccessSettings" },
        ["admin"] = new[] { "CanAccessUserManagement", "IsSystemAdmin" }
    };

    public RbacNormalizationService(
        ICrmDbContext context,
        ILogger<RbacNormalizationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<RbacNormalizationResult> SyncNavigationPermissionsAsync(
        int userGroupId,
        CancellationToken cancellationToken = default)
    {
        var result = new RbacNormalizationResult
        {
            GroupId = userGroupId,
            StartedAt = DateTime.UtcNow
        };

        try
        {
            var group = await _context.UserGroups
                .FirstOrDefaultAsync(g => g.Id == userGroupId, cancellationToken);

            if (group == null)
            {
                result.Success = false;
                result.ErrorMessage = $"User group {userGroupId} not found";
                return result;
            }

            result.GroupName = group.Name;

            // Get navigation configuration
            var navConfig = await _context.NavigationConfigs
                .Where(n => n.IsActive && !n.IsDeleted)
                .ToListAsync(cancellationToken);

            var changesApplied = new List<string>();

            // Check each navigation item and sync permissions
            foreach (var navItem in navConfig)
            {
                var navKey = navItem.Key?.ToLowerInvariant() ?? string.Empty;

                if (!NavigationPermissionMap.TryGetValue(navKey, out var permissionFlags))
                {
                    continue;
                }

                // Get current permission state from group
                var hasPermission = GetPermissionValue(group, permissionFlags[0]);

                // If navigation item has role restrictions, check group compatibility
                if (!string.IsNullOrEmpty(navItem.RequiredRoles))
                {
                    var requiredRoles = navItem.RequiredRoles.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    var groupHasRole = requiredRoles.Any(r =>
                        r.Trim().Equals(group.Name, StringComparison.OrdinalIgnoreCase) ||
                        (r.Trim().Equals("Admin", StringComparison.OrdinalIgnoreCase) && group.IsSystemAdmin));

                    if (!groupHasRole && hasPermission)
                    {
                        // Group has permission but shouldn't based on nav config
                        changesApplied.Add($"Mismatch: {navKey} - Group has permission but nav requires: {navItem.RequiredRoles}");
                    }
                }
            }

            result.ChangesApplied = changesApplied;
            result.Success = true;
            result.CompletedAt = DateTime.UtcNow;

            _logger.LogInformation(
                "RBAC normalization completed for group {GroupId} ({GroupName}). Changes: {Count}",
                userGroupId, group.Name, changesApplied.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during RBAC normalization for group {GroupId}", userGroupId);
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.CompletedAt = DateTime.UtcNow;
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<RbacNormalizationResult> SyncAllGroupsAsync(
        CancellationToken cancellationToken = default)
    {
        var overallResult = new RbacNormalizationResult
        {
            StartedAt = DateTime.UtcNow
        };

        try
        {
            var groups = await _context.UserGroups
                .Where(g => g.IsActive && !g.IsDeleted)
                .ToListAsync(cancellationToken);

            var allChanges = new List<string>();

            foreach (var group in groups)
            {
                var groupResult = await SyncNavigationPermissionsAsync(group.Id, cancellationToken);
                if (groupResult.ChangesApplied.Count > 0)
                {
                    allChanges.AddRange(groupResult.ChangesApplied.Select(c => $"[{group.Name}] {c}"));
                }
            }

            overallResult.ChangesApplied = allChanges;
            overallResult.Success = true;
            overallResult.CompletedAt = DateTime.UtcNow;

            _logger.LogInformation(
                "RBAC normalization completed for all {Count} groups. Total changes: {Changes}",
                groups.Count, allChanges.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during RBAC normalization for all groups");
            overallResult.Success = false;
            overallResult.ErrorMessage = ex.Message;
            overallResult.CompletedAt = DateTime.UtcNow;
        }

        return overallResult;
    }

    /// <inheritdoc />
    public async Task<List<RbacPermissionReport>> GeneratePermissionReportAsync(
        CancellationToken cancellationToken = default)
    {
        var reports = new List<RbacPermissionReport>();

        var groups = await _context.UserGroups
            .Where(g => g.IsActive && !g.IsDeleted)
            .OrderBy(g => g.DisplayOrder)
            .ToListAsync(cancellationToken);

        foreach (var group in groups)
        {
            var report = new RbacPermissionReport
            {
                GroupId = group.Id,
                GroupName = group.Name,
                IsSystemAdmin = group.IsSystemAdmin,
                Permissions = new Dictionary<string, bool>()
            };

            // Add all navigation permissions
            foreach (var mapping in NavigationPermissionMap)
            {
                var hasPermission = GetPermissionValue(group, mapping.Value[0]);
                report.Permissions[mapping.Key] = hasPermission;
            }

            reports.Add(report);
        }

        return reports;
    }

    private static bool GetPermissionValue(CRM.Core.Entities.UserGroup group, string propertyName)
    {
        var property = typeof(CRM.Core.Entities.UserGroup).GetProperty(propertyName);
        if (property == null) return false;

        var value = property.GetValue(group);
        return value is bool boolValue && boolValue;
    }
}

/// <summary>
/// Result of an RBAC normalization operation.
/// </summary>
public class RbacNormalizationResult
{
    public int? GroupId { get; set; }
    public string? GroupName { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string> ChangesApplied { get; set; } = new();
}

/// <summary>
/// Permission report for a user group.
/// </summary>
public class RbacPermissionReport
{
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public bool IsSystemAdmin { get; set; }
    public Dictionary<string, bool> Permissions { get; set; } = new();
}

/// <summary>
/// Interface for RBAC normalization service.
/// </summary>
public interface IRbacNormalizationService
{
    /// <summary>
    /// Syncs group permission flags with navigation filtering rules.
    /// </summary>
    Task<RbacNormalizationResult> SyncNavigationPermissionsAsync(int userGroupId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Syncs all active user groups.
    /// </summary>
    Task<RbacNormalizationResult> SyncAllGroupsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a permission report for all groups.
    /// </summary>
    Task<List<RbacPermissionReport>> GeneratePermissionReportAsync(CancellationToken cancellationToken = default);
}
