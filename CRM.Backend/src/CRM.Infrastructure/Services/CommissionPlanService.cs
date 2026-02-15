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
using CRM.Core.Interfaces;
using CRM.Core.Ports.Input;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Implementation of ICommissionPlanService for commission plan management.
/// Handles creation, modification, and assignment of commission plans with tiering support.
/// </summary>
public class CommissionPlanService : ICommissionPlanService, ICommissionPlanInputPort
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<CommissionPlanService> _logger;

    public CommissionPlanService(ICrmDbContext context, ILogger<CommissionPlanService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region CRUD Operations

    public async Task<IEnumerable<CommissionPlanDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var plans = await _context.CommissionPlans
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

        return plans.Select(MapToDto);
    }

    public async Task<CommissionPlanDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var plan = await _context.CommissionPlans
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);

        return plan != null ? MapToDto(plan) : null;
    }

    public async Task<CommissionPlanDto> CreateAsync(CreateCommissionPlanDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Plan name is required.", nameof(dto.Name));

        var plan = new CommissionPlan
        {
            Name = dto.Name,
            Description = dto.Description,
            BaseRate = dto.BaseRate,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.CommissionPlans.Add(plan);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Commission plan '{PlanName}' created with ID {PlanId}", plan.Name, plan.Id);
        return await GetByIdAsync(plan.Id, cancellationToken) ?? throw new InvalidOperationException("Plan creation failed");
    }

    public async Task<CommissionPlanDto> UpdateAsync(int id, UpdateCommissionPlanDto dto, CancellationToken cancellationToken = default)
    {
        var plan = await _context.CommissionPlans
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);

        if (plan == null)
            throw new InvalidOperationException($"Commission plan {id} not found");

        if (!string.IsNullOrWhiteSpace(dto.Name))
            plan.Name = dto.Name;

        if (dto.Description != null)
            plan.Description = dto.Description;

        if (dto.BaseRate.HasValue)
            plan.BaseRate = dto.BaseRate.Value;

        if (dto.IsActive.HasValue)
            plan.IsActive = dto.IsActive.Value;

        plan.UpdatedAt = DateTime.UtcNow;
        _context.CommissionPlans.Update(plan);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Commission plan {PlanId} updated", id);
        return await GetByIdAsync(plan.Id, cancellationToken) ?? throw new InvalidOperationException("Plan update failed");
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var plan = await _context.CommissionPlans
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);

        if (plan == null)
            return false;

        plan.IsDeleted = true;
        plan.UpdatedAt = DateTime.UtcNow;
        _context.CommissionPlans.Update(plan);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Commission plan {PlanId} deleted", id);
        return true;
    }

    #endregion

    #region Plan Management

    public async Task<bool> ActivateAsync(int planId, CancellationToken cancellationToken = default)
    {
        var plan = await _context.CommissionPlans
            .FirstOrDefaultAsync(p => p.Id == planId && !p.IsDeleted, cancellationToken);

        if (plan == null)
            return false;

        plan.IsActive = true;
        plan.UpdatedAt = DateTime.UtcNow;
        _context.CommissionPlans.Update(plan);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Commission plan {PlanId} activated", planId);
        return true;
    }

    public async Task<bool> DeactivateAsync(int planId, CancellationToken cancellationToken = default)
    {
        var plan = await _context.CommissionPlans
            .FirstOrDefaultAsync(p => p.Id == planId && !p.IsDeleted, cancellationToken);

        if (plan == null)
            return false;

        plan.IsActive = false;
        plan.UpdatedAt = DateTime.UtcNow;
        _context.CommissionPlans.Update(plan);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Commission plan {PlanId} deactivated", planId);
        return true;
    }

    public async Task<bool> AssignToUserAsync(int planId, int userId, DateTime? effectiveDate = null, CancellationToken cancellationToken = default)
    {
        var plan = await _context.CommissionPlans
            .FirstOrDefaultAsync(p => p.Id == planId && !p.IsDeleted, cancellationToken);

        if (plan == null)
            return false;

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);

        if (user == null)
            return false;

        // Soft-delete previous assignments for this user
        var previousAssignments = await _context.CommissionPlanAssignments
            .Where(a => a.UserId == userId && !a.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var assignment in previousAssignments)
        {
            assignment.IsDeleted = true;
            assignment.UpdatedAt = DateTime.UtcNow;
        }

        var newAssignment = new CommissionPlanAssignment
        {
            CommissionPlanId = planId,
            UserId = userId,
            EffectiveDate = effectiveDate ?? DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.CommissionPlanAssignments.Add(newAssignment);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Commission plan {PlanId} assigned to user {UserId}", planId, userId);
        return true;
    }

    public async Task<bool> RemoveFromUserAsync(int planId, int userId, CancellationToken cancellationToken = default)
    {
        var assignment = await _context.CommissionPlanAssignments
            .FirstOrDefaultAsync(a => a.CommissionPlanId == planId && a.UserId == userId && !a.IsDeleted, cancellationToken);

        if (assignment == null)
            return false;

        assignment.IsDeleted = true;
        assignment.UpdatedAt = DateTime.UtcNow;
        _context.CommissionPlanAssignments.Update(assignment);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Commission plan {PlanId} removed from user {UserId}", planId, userId);
        return true;
    }

    public async Task<CommissionPlanDto?> GetUserPlanAsync(int userId, CancellationToken cancellationToken = default)
    {
        var assignment = await _context.CommissionPlanAssignments
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.UserId == userId && !a.IsDeleted && a.EffectiveDate <= DateTime.UtcNow, cancellationToken);

        if (assignment == null)
            return null;

        return await GetByIdAsync(assignment.CommissionPlanId, cancellationToken);
    }

    public async Task<IEnumerable<UserDto>> GetPlanUsersAsync(int planId, CancellationToken cancellationToken = default)
    {
        var users = await _context.CommissionPlanAssignments
            .Where(a => a.CommissionPlanId == planId && !a.IsDeleted)
            .Include(a => a.User)
            .Select(a => a.User)
            .Where(u => !u.IsDeleted)
            .Distinct()
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToListAsync(cancellationToken);

        return users.Select(u => new UserDto
        {
            Id = u.Id,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Email = u.Email,
            CreatedAt = u.CreatedAt
        });
    }

    #endregion

    #region Tier Management

    public async Task<IEnumerable<CommissionTierDto>> GetTiersAsync(int planId, CancellationToken cancellationToken = default)
    {
        var tiers = await _context.CommissionTiers
            .Where(t => t.CommissionPlanId == planId && !t.IsDeleted)
            .OrderBy(t => t.Sequence)
            .ToListAsync(cancellationToken);

        return tiers.Select(t => MapTierToDto(t));
    }

    public async Task<CommissionTierDto> AddTierAsync(int planId, CreateCommissionTierDto dto, CancellationToken cancellationToken = default)
    {
        var plan = await _context.CommissionPlans
            .FirstOrDefaultAsync(p => p.Id == planId && !p.IsDeleted, cancellationToken);

        if (plan == null)
            throw new InvalidOperationException($"Commission plan {planId} not found");

        var tier = new CommissionTier
        {
            CommissionPlanId = planId,
            MinimumAmount = dto.MinimumAmount,
            MaximumAmount = dto.MaximumAmount,
            CommissionRate = dto.CommissionRate,
            Sequence = dto.Sequence,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.CommissionTiers.Add(tier);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Commission tier added to plan {PlanId}", planId);
        return MapTierToDto(tier);
    }

    public async Task<CommissionTierDto> UpdateTierAsync(int tierId, UpdateCommissionTierDto dto, CancellationToken cancellationToken = default)
    {
        var tier = await _context.CommissionTiers
            .FirstOrDefaultAsync(t => t.Id == tierId && !t.IsDeleted, cancellationToken);

        if (tier == null)
            throw new InvalidOperationException($"Commission tier {tierId} not found");

        if (dto.MinimumAmount.HasValue)
            tier.MinimumAmount = dto.MinimumAmount.Value;

        if (dto.MaximumAmount.HasValue)
            tier.MaximumAmount = dto.MaximumAmount;

        if (dto.CommissionRate.HasValue)
            tier.CommissionRate = dto.CommissionRate.Value;

        if (dto.Sequence.HasValue)
            tier.Sequence = dto.Sequence.Value;

        tier.UpdatedAt = DateTime.UtcNow;
        _context.CommissionTiers.Update(tier);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Commission tier {TierId} updated", tierId);
        return MapTierToDto(tier);
    }

    public async Task<bool> RemoveTierAsync(int tierId, CancellationToken cancellationToken = default)
    {
        var tier = await _context.CommissionTiers
            .FirstOrDefaultAsync(t => t.Id == tierId && !t.IsDeleted, cancellationToken);

        if (tier == null)
            return false;

        tier.IsDeleted = true;
        tier.UpdatedAt = DateTime.UtcNow;
        _context.CommissionTiers.Update(tier);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Commission tier {TierId} removed", tierId);
        return true;
    }

    #endregion

    #region Queries

    public async Task<IEnumerable<CommissionPlanDto>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var plans = await _context.CommissionPlans
            .Where(p => !p.IsDeleted && p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

        return plans.Select(MapToDto);
    }

    public async Task<CommissionPlanDto> DuplicateAsync(int planId, string newName, CancellationToken cancellationToken = default)
    {
        var original = await _context.CommissionPlans
            .Include(p => p.Tiers)
            .FirstOrDefaultAsync(p => p.Id == planId && !p.IsDeleted, cancellationToken);

        if (original == null)
            throw new InvalidOperationException($"Commission plan {planId} not found");

        var copy = new CommissionPlan
        {
            Name = newName,
            Description = $"Copy of {original.Name}",
            BaseRate = original.BaseRate,
            IsActive = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.CommissionPlans.Add(copy);
        await _context.SaveChangesAsync(cancellationToken);

        // Copy tiers
        foreach (var tier in original.Tiers.Where(t => !t.IsDeleted))
        {
            var tierCopy = new CommissionTier
            {
                CommissionPlanId = copy.Id,
                MinimumAmount = tier.MinimumAmount,
                MaximumAmount = tier.MaximumAmount,
                CommissionRate = tier.CommissionRate,
                Sequence = tier.Sequence,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.CommissionTiers.Add(tierCopy);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Commission plan {PlanId} duplicated as {NewPlanName}", planId, newName);
        return await GetByIdAsync(copy.Id, cancellationToken) ?? throw new InvalidOperationException("Duplication failed");
    }

    public async Task<IEnumerable<CommissionDto>> GetCommissionHistoryAsync(int planId, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var commissions = await _context.Commissions
            .Where(c => c.CommissionPlanId == planId && !c.IsDeleted)
            .Include(c => c.User)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return commissions.Select(c => new CommissionDto
        {
            Id = c.Id,
            CommissionNumber = c.CommissionNumber ?? string.Empty,
            UserId = c.UserId,
            UserName = $"{c.User?.FirstName} {c.User?.LastName}",
            CommissionPlanId = c.CommissionPlanId,
            CommissionAmount = c.CommissionAmount,
            Status = (int)c.Status,
            CreatedAt = c.CreatedAt
        });
    }

    #endregion

    #region Helpers

    private CommissionPlanDto MapToDto(CommissionPlan plan)
    {
        var userCount = _context.CommissionPlanAssignments
            .Count(a => a.CommissionPlanId == plan.Id && !a.IsDeleted);

        var commissionCount = _context.Commissions
            .Count(c => c.CommissionPlanId == plan.Id && !c.IsDeleted);

        return new CommissionPlanDto
        {
            Id = plan.Id,
            Name = plan.Name,
            Description = plan.Description,
            BaseRate = plan.BaseRate,
            IsActive = plan.IsActive,
            UserCount = userCount,
            CommissionCount = commissionCount,
            CreatedAt = plan.CreatedAt,
            UpdatedAt = plan.UpdatedAt
        };
    }

    private CommissionTierDto MapTierToDto(CommissionTier tier)
    {
        return new CommissionTierDto
        {
            Id = tier.Id,
            CommissionPlanId = tier.CommissionPlanId,
            MinimumAmount = tier.MinimumAmount,
            MaximumAmount = tier.MaximumAmount,
            CommissionRate = tier.CommissionRate,
            Sequence = tier.Sequence
        };
    }

    #endregion
}
