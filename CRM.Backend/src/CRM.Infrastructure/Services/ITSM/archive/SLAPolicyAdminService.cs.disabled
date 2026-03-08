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

using CRM.Core.Dtos.ITSM;
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.ITSM;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.ITSM;

/// <summary>
/// Admin service for managing SLA policies
/// </summary>
public class SLAPolicyAdminService : ISLAPolicyAdminService
{
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<SLAPolicyAdminService> _logger;

    public SLAPolicyAdminService(
        ICrmDbContext dbContext,
        ILogger<SLAPolicyAdminService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<SLAPolicyDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var policy = await _dbContext.SLAPolicies
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);

            return policy != null ? MapToDto(policy) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SLA policy {Id}", id);
            throw;
        }
    }

    public async Task<List<SLAPolicyDto>> GetAllAsync(CancellationToken ct = default)
    {
        try
        {
            var policies = await _dbContext.SLAPolicies
                .AsNoTracking()
                .Where(p => !p.IsDeleted)
                .OrderBy(p => p.Name)
                .ToListAsync(ct);

            return policies.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all SLA policies");
            throw;
        }
    }

    public async Task<SLAPolicyDto> CreateAsync(CreateSLAPolicyDto dto, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("SLA policy name is required");

            var policy = new CRM.Core.Entities.SLAPolicy
            {
                Name = dto.Name,
                Description = dto.Description,
                Priority = dto.Priority,
                Category = dto.Category,
                ResponseTimeHours = dto.ResponseTimeHours,
                ResolutionTimeHours = dto.ResolutionTimeHours,
                BusinessHoursOnly = dto.BusinessHoursOnly,
                Timezone = dto.Timezone,
                BreachAction = dto.BreachAction,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _dbContext.SLAPolicies.Add(policy);
            await _dbContext.SaveChangesAsync(ct);

            _logger.LogInformation("SLA policy created: {PolicyName} (ID: {PolicyId})", policy.Name, policy.Id);

            return MapToDto(policy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating SLA policy");
            throw;
        }
    }

    public async Task<SLAPolicyDto> UpdateAsync(int id, UpdateSLAPolicyDto dto, CancellationToken ct = default)
    {
        try
        {
            var policy = await _dbContext.SLAPolicies
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);

            if (policy == null)
                throw new KeyNotFoundException($"SLA policy with ID {id} not found");

            if (!string.IsNullOrWhiteSpace(dto.Name))
                policy.Name = dto.Name;
            
            if (dto.Description != null)
                policy.Description = dto.Description;
            
            if (!string.IsNullOrWhiteSpace(dto.Priority))
                policy.Priority = dto.Priority;
            
            if (!string.IsNullOrWhiteSpace(dto.Category))
                policy.Category = dto.Category;
            
            if (dto.ResponseTimeHours.HasValue)
                policy.ResponseTimeHours = dto.ResponseTimeHours.Value;
            
            if (dto.ResolutionTimeHours.HasValue)
                policy.ResolutionTimeHours = dto.ResolutionTimeHours.Value;
            
            if (dto.BusinessHoursOnly.HasValue)
                policy.BusinessHoursOnly = dto.BusinessHoursOnly.Value;
            
            if (!string.IsNullOrWhiteSpace(dto.Timezone))
                policy.Timezone = dto.Timezone;
            
            if (!string.IsNullOrWhiteSpace(dto.BreachAction))
                policy.BreachAction = dto.BreachAction;
            
            if (dto.IsActive.HasValue)
                policy.IsActive = dto.IsActive.Value;

            policy.UpdatedAt = DateTime.UtcNow;

            _dbContext.SLAPolicies.Update(policy);
            await _dbContext.SaveChangesAsync(ct);

            _logger.LogInformation("SLA policy updated: {PolicyName} (ID: {PolicyId})", policy.Name, policy.Id);

            return MapToDto(policy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating SLA policy {Id}", id);
            throw;
        }
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var policy = await _dbContext.SLAPolicies
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);

            if (policy == null)
                throw new KeyNotFoundException($"SLA policy with ID {id} not found");

            policy.IsDeleted = true;
            policy.UpdatedAt = DateTime.UtcNow;

            _dbContext.SLAPolicies.Update(policy);
            await _dbContext.SaveChangesAsync(ct);

            _logger.LogInformation("SLA policy deleted: {PolicyId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting SLA policy {Id}", id);
            throw;
        }
    }

    public async Task<SLAInstanceDto> AssignPolicyAsync(int policyId, int serviceRequestId, CancellationToken ct = default)
    {
        try
        {
            var policy = await _dbContext.SLAPolicies
                .FirstOrDefaultAsync(p => p.Id == policyId && !p.IsDeleted, ct);

            if (policy == null)
                throw new KeyNotFoundException($"SLA policy with ID {policyId} not found");

            var request = await _dbContext.ServiceRequests
                .FirstOrDefaultAsync(r => r.Id == serviceRequestId && !r.IsDeleted, ct);

            if (request == null)
                throw new KeyNotFoundException($"Service request with ID {serviceRequestId} not found");

            _logger.LogInformation("SLA policy {PolicyId} assigned to service request {ServiceRequestId}",
                policyId, serviceRequestId);

            return new SLAInstanceDto
            {
                Id = 0,
                PolicyId = policyId,
                ServiceRequestId = serviceRequestId,
                CreatedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning SLA policy to service request");
            throw;
        }
    }

    public async Task<List<SLAPolicyDto>> GetApplicablePoliciesAsync(
        string? priority,
        string? category,
        CancellationToken ct = default)
    {
        try
        {
            var query = _dbContext.SLAPolicies
                .AsNoTracking()
                .Where(p => p.IsActive && !p.IsDeleted);

            if (!string.IsNullOrEmpty(priority))
                query = query.Where(p => p.Priority == priority);

            if (!string.IsNullOrEmpty(category))
                query = query.Where(p => p.Category == category);

            var policies = await query
                .OrderBy(p => p.Name)
                .ToListAsync(ct);

            return policies.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting applicable SLA policies");
            throw;
        }
    }

    private SLAPolicyDto MapToDto(CRM.Core.Entities.SLAPolicy policy)
    {
        return new SLAPolicyDto
        {
            Id = policy.Id,
            Name = policy.Name,
            Description = policy.Description,
            Priority = policy.Priority,
            Category = policy.Category,
            ResponseTimeHours = policy.ResponseTimeHours,
            ResolutionTimeHours = policy.ResolutionTimeHours,
            BusinessHoursOnly = policy.BusinessHoursOnly,
            Timezone = policy.Timezone,
            BreachAction = policy.BreachAction,
            IsActive = policy.IsActive,
            CreatedAt = policy.CreatedAt,
            UpdatedAt = policy.UpdatedAt
        };
    }
}
