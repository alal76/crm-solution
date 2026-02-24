// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities.KnowledgeBase;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for managing business hours configurations.
/// Implements TODO-SYS005-001 — Business Hours Configuration and Validation.
/// </summary>
public class BusinessHoursConfigService : IBusinessHoursConfigService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<BusinessHoursConfigService> _logger;

    public BusinessHoursConfigService(ICrmDbContext context, ILogger<BusinessHoursConfigService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<BusinessHoursConfigDto>> GetAllAsync(CancellationToken ct = default)
    {
        try
        {
            var configs = await _context.BusinessHoursConfigs
                .OrderByDescending(b => b.IsDefault)
                .ThenBy(b => b.Name)
                .ToListAsync(ct);

            return configs.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving business hours configurations");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<BusinessHoursConfigDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var config = await _context.BusinessHoursConfigs
                .FirstOrDefaultAsync(b => b.Id == id, ct);

            return config == null ? null : MapToDto(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving business hours configuration {Id}", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<BusinessHoursConfigDto> CreateAsync(BusinessHoursConfigRequest request, CancellationToken ct = default)
    {
        try
        {
            // Enforce single default rule
            if (request.IsDefault)
            {
                await ClearExistingDefaultAsync(ct);
            }

            var config = new BusinessHours
            {
                Name = request.Name,
                Timezone = request.Timezone,
                Is24x7 = request.Is24x7,
                IsActive = request.IsActive,
                IsDefault = request.IsDefault,
                ScheduleJson = request.ScheduleJson,
                HolidaysJson = request.HolidaysJson,
                CreatedAt = DateTime.UtcNow
            };

            _context.BusinessHoursConfigs.Add(config);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Created business hours configuration: {Name} (Id={Id})", config.Name, config.Id);
            return MapToDto(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating business hours configuration");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<BusinessHoursConfigDto?> UpdateAsync(int id, BusinessHoursConfigRequest request, CancellationToken ct = default)
    {
        try
        {
            var config = await _context.BusinessHoursConfigs
                .FirstOrDefaultAsync(b => b.Id == id, ct);

            if (config == null)
                return null;

            // Enforce single default rule
            if (request.IsDefault && !config.IsDefault)
            {
                await ClearExistingDefaultAsync(ct);
            }

            config.Name = request.Name;
            config.Timezone = request.Timezone;
            config.Is24x7 = request.Is24x7;
            config.IsActive = request.IsActive;
            config.IsDefault = request.IsDefault;
            config.ScheduleJson = request.ScheduleJson;
            config.HolidaysJson = request.HolidaysJson;
            config.UpdatedAt = DateTime.UtcNow;

            _context.BusinessHoursConfigs.Update(config);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Updated business hours configuration {Id}", id);
            return MapToDto(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating business hours configuration {Id}", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var config = await _context.BusinessHoursConfigs
                .FirstOrDefaultAsync(b => b.Id == id, ct);

            if (config == null)
                return false;

            config.IsDeleted = true;
            config.UpdatedAt = DateTime.UtcNow;

            _context.BusinessHoursConfigs.Update(config);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Soft-deleted business hours configuration {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting business hours configuration {Id}", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<BusinessHoursConfigDto?> SetDefaultAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var config = await _context.BusinessHoursConfigs
                .FirstOrDefaultAsync(b => b.Id == id, ct);

            if (config == null)
                return null;

            // Unset any existing default
            await ClearExistingDefaultAsync(ct);

            config.IsDefault = true;
            config.UpdatedAt = DateTime.UtcNow;

            _context.BusinessHoursConfigs.Update(config);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Set business hours configuration {Id} as default", id);
            return MapToDto(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting default business hours configuration {Id}", id);
            throw;
        }
    }

    // ─── Private helpers ─────────────────────────────────────────────────────

    private async Task ClearExistingDefaultAsync(CancellationToken ct)
    {
        var existing = await _context.BusinessHoursConfigs
            .Where(b => b.IsDefault)
            .ToListAsync(ct);

        foreach (var b in existing)
        {
            b.IsDefault = false;
            b.UpdatedAt = DateTime.UtcNow;
            _context.BusinessHoursConfigs.Update(b);
        }
        // Deferred — caller must call SaveChangesAsync
    }

    private static BusinessHoursConfigDto MapToDto(BusinessHours b) => new()
    {
        Id = b.Id,
        Name = b.Name,
        Timezone = b.Timezone,
        Is24x7 = b.Is24x7,
        IsActive = b.IsActive,
        IsDefault = b.IsDefault,
        ScheduleJson = b.ScheduleJson,
        HolidaysJson = b.HolidaysJson,
        CreatedAt = b.CreatedAt,
        UpdatedAt = b.UpdatedAt
    };
}
