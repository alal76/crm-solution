// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
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
            {
                throw new ArgumentException("SLA policy name is required");
            }

            var policy = new CRM.Core.Entities.SLAPolicy
            {
                Name = dto.Name,
                Description = dto.Description ?? string.Empty,
                Priority = CRM.Core.Entities.ServicePriority.Medium,
                InitialResponseTimeMinutes = dto.ResponseTimeHours * 60,
                ResolutionTimeMinutes = dto.ResolutionTimeHours * 60,
                WorkingHoursOnly = dto.BusinessHoursOnly,
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
            {
                throw new KeyNotFoundException($"SLA policy with ID {id} not found");
            }

            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                policy.Name = dto.Name;
            }

            if (dto.Description != null)
            {
                policy.Description = dto.Description;
            }

            if (dto.ResponseTimeHours.HasValue)
            {
                policy.InitialResponseTimeMinutes = dto.ResponseTimeHours.Value * 60;
            }

            if (dto.ResolutionTimeHours.HasValue)
            {
                policy.ResolutionTimeMinutes = dto.ResolutionTimeHours.Value * 60;
            }

            if (dto.BusinessHoursOnly.HasValue)
            {
                policy.WorkingHoursOnly = dto.BusinessHoursOnly.Value;
            }

            if (dto.IsActive.HasValue)
            {
                policy.IsActive = dto.IsActive.Value;
            }


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
            {
                throw new KeyNotFoundException($"SLA policy with ID {id} not found");
            }

            policy.IsDeleted = true;

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
            {
                throw new KeyNotFoundException($"SLA policy with ID {policyId} not found");
            }

            var request = await _dbContext.ServiceRequests
                .FirstOrDefaultAsync(r => r.Id == serviceRequestId && !r.IsDeleted, ct);

            if (request == null)
            {
                throw new KeyNotFoundException($"Service request with ID {serviceRequestId} not found");
            }

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
            Priority = policy.Priority.ToString(),
            Category = string.Empty,
            ResponseTimeHours = policy.InitialResponseTimeMinutes / 60,
            ResolutionTimeHours = policy.ResolutionTimeMinutes / 60,
            BusinessHoursOnly = policy.WorkingHoursOnly,
            Timezone = "UTC",
            BreachAction = "Notify",
            IsActive = policy.IsActive,
            CreatedAt = policy.CreatedAt,
            UpdatedAt = policy.UpdatedAt
        };
    }
}
