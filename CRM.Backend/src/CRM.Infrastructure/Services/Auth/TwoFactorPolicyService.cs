// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.Auth;

/// <summary>
/// DTO representing a 2FA enforcement policy for a user group.
/// </summary>
public class TwoFactorPolicyDto
{
    /// <summary>Group identifier.</summary>
    public int GroupId { get; set; }

    /// <summary>Group display name.</summary>
    public string GroupName { get; set; } = string.Empty;

    /// <summary>Whether 2FA is required for members of this group.</summary>
    public bool Required { get; set; }

    /// <summary>Allowed 2FA methods: "totp", "sms", "email", "webauthn".</summary>
    public string[] AllowedMethods { get; set; } = Array.Empty<string>();

    /// <summary>Number of days after policy enablement before enforcement begins.</summary>
    public int GracePeriodDays { get; set; } = 7;
}

/// <summary>
/// Interface for managing per-group two-factor authentication enforcement policies.
/// Queries UserGroup entities' RequireTwoFactor/EnforceTwoFactor flags,
/// extended with in-memory policy metadata (allowed methods, grace period).
/// </summary>
public interface ITwoFactorPolicyService
{
    /// <summary>
    /// Checks whether 2FA is required for a specific user based on their group memberships.
    /// Returns true if ANY group the user belongs to requires 2FA.
    /// </summary>
    Task<bool> Is2FARequiredForUserAsync(int userId, CancellationToken ct = default);

    /// <summary>
    /// Gets the 2FA policy for a specific group.
    /// </summary>
    Task<TwoFactorPolicyDto> GetPolicyForGroupAsync(int groupId, CancellationToken ct = default);

    /// <summary>
    /// Sets or updates the 2FA policy for a specific group.
    /// Persists Required/EnforceTwoFactor to the UserGroup entity and caches extended metadata.
    /// </summary>
    Task SetPolicyForGroupAsync(int groupId, TwoFactorPolicyDto policy, CancellationToken ct = default);

    /// <summary>
    /// Gets all configured 2FA policies across all groups that have 2FA enabled.
    /// </summary>
    Task<IEnumerable<TwoFactorPolicyDto>> GetAllPoliciesAsync(CancellationToken ct = default);
}

/// <summary>
/// Implementation of 2FA policy enforcement per user group.
/// Uses UserGroup entities' RequireTwoFactor/EnforceTwoFactor fields for persistence,
/// and a ConcurrentDictionary for extended policy metadata (AllowedMethods, GracePeriodDays).
/// </summary>
public class TwoFactorPolicyService : ITwoFactorPolicyService
{
    /// <summary>
    /// In-memory cache for extended policy metadata (allowed methods, grace period)
    /// that doesn't have a dedicated DB column.
    /// </summary>
    private static readonly ConcurrentDictionary<int, TwoFactorPolicyExtension> _policyExtensions = new();

    private readonly CrmDbContext _dbContext;
    private readonly ILogger<TwoFactorPolicyService> _logger;

    public TwoFactorPolicyService(CrmDbContext dbContext, ILogger<TwoFactorPolicyService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<bool> Is2FARequiredForUserAsync(int userId, CancellationToken ct = default)
    {
        try
        {
            // Get all group IDs the user belongs to
            var userGroupIds = await _dbContext.UserGroupMembers
                .Where(m => m.UserId == userId)
                .Select(m => m.UserGroupId)
                .ToListAsync(ct);

            if (userGroupIds.Count == 0)
                return false;

            // Check group-level RequireTwoFactor / EnforceTwoFactor flags on UserGroup entities
            var hasRequiredGroup = await _dbContext.UserGroups
                .Where(g => userGroupIds.Contains(g.Id) && (g.RequireTwoFactor || g.EnforceTwoFactor))
                .AnyAsync(ct);

            return hasRequiredGroup;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking 2FA requirement for user {UserId}", userId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<TwoFactorPolicyDto> GetPolicyForGroupAsync(int groupId, CancellationToken ct = default)
    {
        var group = await _dbContext.UserGroups
            .AsNoTracking()
            .Where(g => g.Id == groupId)
            .Select(g => new { g.Id, g.Name, g.RequireTwoFactor, g.EnforceTwoFactor })
            .FirstOrDefaultAsync(ct);

        if (group == null)
        {
            return new TwoFactorPolicyDto
            {
                GroupId = groupId,
                GroupName = string.Empty,
                Required = false,
                AllowedMethods = new[] { "totp", "webauthn" },
                GracePeriodDays = 7
            };
        }

        var extension = _policyExtensions.GetValueOrDefault(groupId);

        return new TwoFactorPolicyDto
        {
            GroupId = group.Id,
            GroupName = group.Name,
            Required = group.RequireTwoFactor || group.EnforceTwoFactor,
            AllowedMethods = extension?.AllowedMethods ?? new[] { "totp", "webauthn" },
            GracePeriodDays = extension?.GracePeriodDays ?? 7
        };
    }

    /// <inheritdoc />
    public async Task SetPolicyForGroupAsync(int groupId, TwoFactorPolicyDto policy, CancellationToken ct = default)
    {
        var group = await _dbContext.UserGroups.FindAsync(new object[] { groupId }, ct);
        if (group == null)
        {
            _logger.LogWarning("Cannot set 2FA policy: UserGroup {GroupId} not found", groupId);
            return;
        }

        // Persist the Required flag to the UserGroup entity
        group.RequireTwoFactor = policy.Required;
        group.EnforceTwoFactor = policy.Required;
        group.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(ct);

        // Store extended metadata (AllowedMethods, GracePeriodDays) in memory
        _policyExtensions[groupId] = new TwoFactorPolicyExtension
        {
            AllowedMethods = policy.AllowedMethods.Length > 0 ? policy.AllowedMethods : new[] { "totp", "webauthn" },
            GracePeriodDays = policy.GracePeriodDays
        };

        _logger.LogInformation("2FA policy updated for group {GroupId} ({GroupName}): Required={Required}, Methods=[{Methods}], GracePeriod={GracePeriodDays}d",
            groupId, group.Name, policy.Required, string.Join(", ", policy.AllowedMethods), policy.GracePeriodDays);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TwoFactorPolicyDto>> GetAllPoliciesAsync(CancellationToken ct = default)
    {
        var groups = await _dbContext.UserGroups
            .AsNoTracking()
            .Where(g => !g.IsDeleted)
            .Select(g => new { g.Id, g.Name, g.RequireTwoFactor, g.EnforceTwoFactor })
            .ToListAsync(ct);

        return groups.Select(g =>
        {
            var extension = _policyExtensions.GetValueOrDefault(g.Id);
            return new TwoFactorPolicyDto
            {
                GroupId = g.Id,
                GroupName = g.Name,
                Required = g.RequireTwoFactor || g.EnforceTwoFactor,
                AllowedMethods = extension?.AllowedMethods ?? new[] { "totp", "webauthn" },
                GracePeriodDays = extension?.GracePeriodDays ?? 7
            };
        });
    }

    /// <summary>
    /// Extended policy metadata not persisted in DB (in-memory only).
    /// </summary>
    private sealed class TwoFactorPolicyExtension
    {
        public string[] AllowedMethods { get; init; } = Array.Empty<string>();
        public int GracePeriodDays { get; init; } = 7;
    }
}
