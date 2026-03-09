// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos.ITSM;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.ITSM;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.ITSM;

/// <summary>
/// Service for managing escalation policies with multiple levels.
/// </summary>
public class EscalationPolicyService : IEscalationPolicyService
{
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<EscalationPolicyService> _logger;

    public EscalationPolicyService(
        ICrmDbContext dbContext,
        ILogger<EscalationPolicyService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IEnumerable<EscalationPolicyDto>> GetPoliciesAsync(bool? isActive = null)
    {
        try
        {
            var query = _dbContext.EscalationPolicies.AsNoTracking().Where(p => !p.IsDeleted);

            if (isActive.HasValue)
            {
                query = query.Where(p => p.IsActive == isActive);
            }

            var policies = await query
                .OrderByDescending(p => p.IsDefault)
                .ThenBy(p => p.Name)
                .ToListAsync();

            return policies.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting escalation policies");
            throw;
        }
    }

    public async Task<EscalationPolicyDto?> GetPolicyByIdAsync(int id)
    {
        try
        {
            var policy = await _dbContext.EscalationPolicies
                .AsNoTracking()
                .Include(p => p.Levels)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            return policy != null ? MapToDto(policy) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting escalation policy {Id}", id);
            throw;
        }
    }

    public async Task<EscalationPolicyDto> CreatePolicyAsync(CreateEscalationPolicyDto dto, int createdById)
    {
        try
        {
            var policy = new EscalationPolicy
            {
                Name = dto.Name,
                Description = dto.Description,
                IsActive = dto.IsActive,
                IsDefault = dto.IsDefault,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _dbContext.EscalationPolicies.Add(policy);
            await _dbContext.SaveChangesAsync();

            if (dto.Levels != null && dto.Levels.Any())
            {
                foreach (var level in dto.Levels)
                {
                    var policyLevel = new EscalationLevel
                    {
                        PolicyId = policy.Id,
                        LevelNumber = level.LevelNumber,
                        Name = level.Name,
                        EscalateAfterMinutes = level.EscalateAfterMinutes,
                        NotifyUserId = level.NotifyUserId,
                        NotifyTeamId = level.NotifyTeamId,
                        SendEmail = level.SendEmail,
                        SendSms = level.SendSms,
                        EmailTemplateId = level.EmailTemplateId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    };

                    _dbContext.EscalationLevels.Add(policyLevel);
                }

                await _dbContext.SaveChangesAsync();
            }

            _logger.LogInformation("Escalation policy created: {PolicyId} ({PolicyName})", policy.Id, policy.Name);

            await _dbContext.EscalationPolicies.Entry(policy).Collection(p => p.Levels).LoadAsync();

            return MapToDto(policy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating escalation policy");
            throw;
        }
    }

    public async Task<EscalationPolicyDto> UpdatePolicyAsync(int id, UpdateEscalationPolicyDto dto, int modifiedById)
    {
        try
        {
            var policy = await _dbContext.EscalationPolicies
                .Include(p => p.Levels)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (policy == null)
            {
                throw new KeyNotFoundException($"Escalation policy with ID {id} not found");
            }

            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                policy.Name = dto.Name;
            }

            if (dto.IsActive.HasValue)
            {
                policy.IsActive = dto.IsActive.Value;
            }

            if (dto.IsDefault.HasValue)
            {
                policy.IsDefault = dto.IsDefault.Value;
            }

            if (!string.IsNullOrWhiteSpace(dto.Description))
            {
                policy.Description = dto.Description;
            }


            _dbContext.EscalationPolicies.Update(policy);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Escalation policy updated: {PolicyId}", id);

            return MapToDto(policy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating escalation policy {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeletePolicyAsync(int id)
    {
        try
        {
            var policy = await _dbContext.EscalationPolicies
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (policy == null)
            {
                return false;
            }

            policy.IsDeleted = true;

            var levels = await _dbContext.EscalationLevels
                .Where(l => l.PolicyId == id && !l.IsDeleted)
                .ToListAsync();

            foreach (var level in levels)
            {
                level.IsDeleted = true;
            }

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Escalation policy deleted: {PolicyId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting escalation policy {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<EscalationLevelDto>> GetPolicyLevelsAsync(int policyId)
    {
        try
        {
            var levels = await _dbContext.EscalationLevels
                .AsNoTracking()
                .Where(l => l.PolicyId == policyId && !l.IsDeleted)
                .OrderBy(l => l.LevelNumber)
                .ToListAsync();

            return levels.Select(MapLevelToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting levels for policy {PolicyId}", policyId);
            throw;
        }
    }

    public async Task<EscalationLevelDto> AddLevelAsync(int policyId, CreateEscalationLevelDto dto, int createdById)
    {
        try
        {
            var policy = await _dbContext.EscalationPolicies
                .FirstOrDefaultAsync(p => p.Id == policyId && !p.IsDeleted);

            if (policy == null)
            {
                throw new KeyNotFoundException($"Escalation policy with ID {policyId} not found");
            }

            var level = new EscalationLevel
            {
                PolicyId = policyId,
                LevelNumber = dto.LevelNumber,
                Name = dto.Name,
                EscalateAfterMinutes = dto.EscalateAfterMinutes,
                NotifyUserId = dto.NotifyUserId,
                NotifyTeamId = dto.NotifyTeamId,
                SendEmail = dto.SendEmail,
                SendSms = dto.SendSms,
                EmailTemplateId = dto.EmailTemplateId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _dbContext.EscalationLevels.Add(level);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Level added to policy {PolicyId}: {LevelNumber}", policyId, dto.LevelNumber);
            return MapLevelToDto(level);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding level to policy {PolicyId}", policyId);
            throw;
        }
    }

    public async Task<EscalationLevelDto> UpdateLevelAsync(int levelId, CreateEscalationLevelDto dto, int modifiedById)
    {
        try
        {
            var level = await _dbContext.EscalationLevels
                .FirstOrDefaultAsync(l => l.Id == levelId && !l.IsDeleted);

            if (level == null)
            {
                throw new KeyNotFoundException($"Escalation level with ID {levelId} not found");
            }

            level.LevelNumber = dto.LevelNumber;
            level.Name = dto.Name;
            level.EscalateAfterMinutes = dto.EscalateAfterMinutes;
            level.NotifyUserId = dto.NotifyUserId;
            level.NotifyTeamId = dto.NotifyTeamId;
            level.SendEmail = dto.SendEmail;
            level.SendSms = dto.SendSms;
            level.EmailTemplateId = dto.EmailTemplateId;

            _dbContext.EscalationLevels.Update(level);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Escalation level updated: {LevelId}", levelId);
            return MapLevelToDto(level);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating escalation level {LevelId}", levelId);
            throw;
        }
    }

    public async Task<bool> DeleteLevelAsync(int levelId)
    {
        try
        {
            var level = await _dbContext.EscalationLevels
                .FirstOrDefaultAsync(l => l.Id == levelId && !l.IsDeleted);

            if (level == null)
            {
                return false;
            }

            level.IsDeleted = true;

            _dbContext.EscalationLevels.Update(level);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Escalation level deleted: {LevelId}", levelId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting escalation level {LevelId}", levelId);
            throw;
        }
    }

    public async Task<bool> AssignPolicyToRequestAsync(int serviceRequestId, int policyId, int assignedById)
    {
        try
        {
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning policy to service request");
            throw;
        }
    }

    public async Task<EscalationPolicyDto?> GetDefaultPolicyAsync(int? categoryId, int? priority)
    {
        try
        {
            var policy = await _dbContext.EscalationPolicies
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.IsDefault && p.IsActive && !p.IsDeleted);

            return policy != null ? MapToDto(policy) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting default escalation policy");
            throw;
        }
    }

    public async Task<bool> SetAsDefaultAsync(int policyId, int? categoryId, int? priority)
    {
        try
        {
            var policy = await _dbContext.EscalationPolicies
                .FirstOrDefaultAsync(p => p.Id == policyId && !p.IsDeleted);

            if (policy == null)
            {
                return false;
            }

            var existingDefaults = await _dbContext.EscalationPolicies
                .Where(p => p.IsDefault && !p.IsDeleted)
                .ToListAsync();

            foreach (var existing in existingDefaults)
            {
                existing.IsDefault = false;
            }

            policy.IsDefault = true;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Escalation policy {PolicyId} set as default", policyId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting default escalation policy");
            throw;
        }
    }

    private EscalationPolicyDto MapToDto(EscalationPolicy policy)
    {
        return new EscalationPolicyDto
        {
            Id = policy.Id,
            Name = policy.Name,
            Description = policy.Description,
            IsActive = policy.IsActive,
            IsDefault = policy.IsDefault,
            Levels = policy.Levels
                ?.Where(l => !l.IsDeleted)
                .OrderBy(l => l.LevelNumber)
                .Select(MapLevelToDto)
                .ToList() ?? new List<EscalationLevelDto>(),
            CreatedAt = policy.CreatedAt,
            UpdatedAt = policy.UpdatedAt ?? DateTime.UtcNow
        };
    }

    private EscalationLevelDto MapLevelToDto(EscalationLevel level)
    {
        return new EscalationLevelDto
        {
            Id = level.Id,
            PolicyId = level.PolicyId,
            LevelNumber = level.LevelNumber,
            Name = level.Name ?? string.Empty,
            EscalateAfterMinutes = level.EscalateAfterMinutes,
            NotifyUserId = level.NotifyUserId,
            NotifyTeamId = level.NotifyTeamId,
            SendEmail = level.SendEmail,
            SendSms = level.SendSms,
            EmailTemplateId = level.EmailTemplateId
        };
    }
}
