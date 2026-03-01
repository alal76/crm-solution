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
/// Lead Source Configuration Service (TODO-CRM002-03)
/// Manages lead source attribution and tracking.
/// </summary>
public class LeadSourceConfigService : ILeadSourceConfigService
{
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<LeadSourceConfigService> _logger;

    public LeadSourceConfigService(ICrmDbContext dbContext, ILogger<LeadSourceConfigService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IEnumerable<LeadSourceConfig>> GetAllAsync(CancellationToken ct = default)
    {
        return await _dbContext.LeadSourceConfigs
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<LeadSourceConfig>> GetActiveAsync(CancellationToken ct = default)
    {
        return await _dbContext.LeadSourceConfigs
            .Where(x => !x.IsDeleted && x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
    }

    public async Task<LeadSourceConfig?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _dbContext.LeadSourceConfigs
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
    }

    public async Task<LeadSourceConfig?> GetByTrackingCodeAsync(string trackingCode, CancellationToken ct = default)
    {
        return await _dbContext.LeadSourceConfigs
            .FirstOrDefaultAsync(x => x.TrackingCode == trackingCode && !x.IsDeleted && x.IsActive, ct);
    }

    public async Task<LeadSourceConfig> CreateAsync(LeadSourceConfig source, CancellationToken ct = default)
    {
        source.CreatedAt = DateTime.UtcNow;
        _dbContext.LeadSourceConfigs.Add(source);
        await _dbContext.SaveChangesAsync(ct);
        _logger.LogInformation("Created lead source config {Id}: {Name}", source.Id, source.Name);
        return source;
    }

    public async Task<LeadSourceConfig?> UpdateAsync(int id, LeadSourceConfig source, CancellationToken ct = default)
    {
        var existing = await GetByIdAsync(id, ct);
        if (existing == null) return null;

        existing.Name = source.Name;
        existing.IsActive = source.IsActive;
        existing.CostPerLead = source.CostPerLead;
        existing.TrackingCode = source.TrackingCode;
        existing.Description = source.Description;
        existing.Category = source.Category;
        existing.CampaignId = source.CampaignId;

        await _dbContext.SaveChangesAsync(ct);
        _logger.LogInformation("Updated lead source config {Id}: {Name}", id, source.Name);
        return existing;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var existing = await GetByIdAsync(id, ct);
        if (existing == null) return false;

        existing.IsDeleted = true;
        await _dbContext.SaveChangesAsync(ct);
        _logger.LogInformation("Deleted lead source config {Id}", id);
        return true;
    }

    public async Task<Dictionary<int, int>> GetLeadCountBySourceAsync(CancellationToken ct = default)
    {
        return await _dbContext.Leads
            .Where(l => !l.IsDeleted && l.LeadSourceId.HasValue)
            .GroupBy(l => l.LeadSourceId!.Value)
            .Select(g => new { SourceId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.SourceId, x => x.Count, ct);
    }

    public async Task<decimal?> CalculateRoiAsync(int sourceId, DateTime? startDate, DateTime? endDate, CancellationToken ct = default)
    {
        var source = await GetByIdAsync(sourceId, ct);
        if (source?.CostPerLead == null || source.CostPerLead == 0)
            return null;

        var query = _dbContext.Leads.Where(l => !l.IsDeleted && l.LeadSourceId == sourceId);

        if (startDate.HasValue)
            query = query.Where(l => l.CreatedAt >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(l => l.CreatedAt <= endDate.Value);

        var leadCount = await query.CountAsync(ct);
        if (leadCount == 0) return 0;

        // Calculate conversions
        var convertedCount = await query
            .Where(l => l.Status == LeadLifecycleStatus.Converted)
            .CountAsync(ct);

        var totalCost = leadCount * source.CostPerLead.Value;
        
        // Simple ROI: (conversions / cost) * 100
        // A more sophisticated calculation would factor in opportunity values
        if (totalCost == 0) return 0;
        return (convertedCount / totalCost) * 100;
    }
}
