// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// CRM-admin service for managing the Customer Portal configuration and users.
/// </summary>
public class PortalAdminService : IPortalAdminService
{
    private readonly ICrmDbContext _db;
    private readonly ILogger<PortalAdminService> _logger;

    public PortalAdminService(ICrmDbContext db, ILogger<PortalAdminService> logger)
    {
        _db = db;
        _logger = logger;
    }

    // ── Config ────────────────────────────────────────────────────────────────

    public async Task<PortalConfigDto> GetConfigAsync(CancellationToken ct = default)
    {
        var config = await _db.PortalConfigs.AsNoTracking()
            .FirstOrDefaultAsync(c => !c.IsDeleted, ct);

        return config == null
            ? new PortalConfigDto { IsEnabled = false, AllowSelfRegistration = true }
            : MapConfig(config);
    }

    public async Task<PortalConfigDto> UpdateConfigAsync(
        UpdatePortalConfigDto dto, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var config = await _db.PortalConfigs
            .FirstOrDefaultAsync(c => !c.IsDeleted, ct);

        if (config == null)
        {
            // Create on first use
            config = new PortalConfig { CreatedAt = now, UpdatedAt = now };
            _db.PortalConfigs.Add(config);
        }

        if (dto.IsEnabled.HasValue)          config.IsEnabled          = dto.IsEnabled.Value;
        if (dto.AllowSelfRegistration.HasValue) config.AllowSelfRegistration = dto.AllowSelfRegistration.Value;
        if (dto.WelcomeMessage  != null)     config.WelcomeMessage     = dto.WelcomeMessage;
        if (dto.SupportEmail    != null)     config.SupportEmail       = dto.SupportEmail;
        if (dto.LogoUrl         != null)     config.LogoUrl            = dto.LogoUrl;
        if (dto.PrimaryColor    != null)     config.PrimaryColor       = dto.PrimaryColor;
        if (dto.PortalTitle     != null)     config.PortalTitle        = dto.PortalTitle;
        if (dto.AllowedDomains  != null)     config.AllowedDomains     = dto.AllowedDomains;

        config.UpdatedAt = now;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Portal configuration updated");
        return MapConfig(config);
    }

    // ── Users ─────────────────────────────────────────────────────────────────

    public async Task<PagedResultDto<PortalUserDto>> GetPortalUsersAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = _db.PortalUsers.AsNoTracking().Where(u => !u.IsDeleted);
        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new PortalUserDto
            {
                Id = u.Id,
                Email = u.Email,
                DisplayName = u.DisplayName,
                ContactId = u.ContactId,
                AccountId = u.AccountId,
                IsActive = u.IsActive,
                LastLoginAt = u.LastLoginAt,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync(ct);

        return new PagedResultDto<PortalUserDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<bool> ActivatePortalUserAsync(int portalUserId, CancellationToken ct = default)
    {
        var user = await _db.PortalUsers
            .FirstOrDefaultAsync(u => u.Id == portalUserId && !u.IsDeleted, ct);

        if (user == null) return false;

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeactivatePortalUserAsync(int portalUserId, CancellationToken ct = default)
    {
        var user = await _db.PortalUsers
            .FirstOrDefaultAsync(u => u.Id == portalUserId && !u.IsDeleted, ct);

        if (user == null) return false;

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static PortalConfigDto MapConfig(PortalConfig c) => new PortalConfigDto
    {
        IsEnabled = c.IsEnabled,
        AllowSelfRegistration = c.AllowSelfRegistration,
        WelcomeMessage = c.WelcomeMessage,
        SupportEmail = c.SupportEmail,
        LogoUrl = c.LogoUrl,
        PrimaryColor = c.PrimaryColor,
        PortalTitle = c.PortalTitle,
        AllowedDomains = c.AllowedDomains
    };
}
