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
using System.Text.Json;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Territory Assignment Service (TODO-GAP-04)
/// Assigns leads to users based on territory rules.
/// </summary>
public class TerritoryAssignmentService : ITerritoryAssignmentService
{
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<TerritoryAssignmentService> _logger;

    public TerritoryAssignmentService(ICrmDbContext dbContext, ILogger<TerritoryAssignmentService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<int?> AssignLeadToTerritoryAsync(Lead lead, CancellationToken cancellationToken = default)
    {
        var territory = await GetMatchingTerritoryAsync(lead, cancellationToken);
        if (territory?.OwnerUserId == null)
            return null;

        // Assign lead to territory owner
        lead.OwnerId = territory.OwnerUserId;
        lead.Region = territory.Name;

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Assigned lead {LeadId} to territory {TerritoryId} (owner: {OwnerId})", 
            lead.Id, territory.Id, territory.OwnerUserId);

        return territory.OwnerUserId;
    }

    public async Task<Territory?> GetMatchingTerritoryAsync(Lead lead, CancellationToken cancellationToken = default)
    {
        var territories = await GetActiveTerritoriesAsync(cancellationToken);

        foreach (var territory in territories)
        {
            if (MatchesTerritoryRules(lead, territory))
            {
                return territory;
            }
        }

        return null;
    }

    private bool MatchesTerritoryRules(Lead lead, Territory territory)
    {
        // Check geographic match (country/state)
        if (!string.IsNullOrEmpty(territory.Countries))
        {
            // Would need to geocode lead address - simplified for now
        }

        if (!string.IsNullOrEmpty(territory.States))
        {
            // Would need to extract state from lead address
        }

        // Check industry match
        if (!string.IsNullOrEmpty(territory.Industries))
        {
            // Would need to check against lead's company industry
        }

        // If no specific rules or all rules pass, consider it a match
        // This is a simplified implementation - real logic would be more complex
        return true;
    }

    private List<string> ParseJsonArray(string? json)
    {
        if (string.IsNullOrEmpty(json)) return new List<string>();
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        catch
        {
            // Fallback to comma-separated
            return json.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        }
    }

    public async Task<IEnumerable<Territory>> GetActiveTerritoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Territories
            .Where(t => !t.IsDeleted && t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CreateTerritoryAsync(Territory territory, CancellationToken cancellationToken = default)
    {
        territory.CreatedAt = DateTime.UtcNow;
        _dbContext.Territories.Add(territory);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Created territory {Id}: {Name}", territory.Id, territory.Name);
        return territory.Id;
    }

    public async Task<bool> UpdateTerritoryAsync(Territory territory, CancellationToken cancellationToken = default)
    {
        var existing = await GetByIdAsync(territory.Id, cancellationToken);
        if (existing == null) return false;

        existing.Name = territory.Name;
        existing.Code = territory.Code;
        existing.Description = territory.Description;
        existing.Type = territory.Type;
        existing.ParentTerritoryId = territory.ParentTerritoryId;
        existing.OwnerUserId = territory.OwnerUserId;
        existing.IsActive = territory.IsActive;
        existing.Countries = territory.Countries;
        existing.States = territory.States;
        existing.Cities = territory.Cities;
        existing.PostalCodePatterns = territory.PostalCodePatterns;
        existing.Industries = territory.Industries;
        existing.MinCompanySize = territory.MinCompanySize;
        existing.MaxCompanySize = territory.MaxCompanySize;

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Updated territory {Id}: {Name}", territory.Id, territory.Name);
        return true;
    }

    public async Task<Territory?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Territories
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken);
    }

    public async Task<bool> DeleteTerritoryAsync(int id, CancellationToken cancellationToken = default)
    {
        var existing = await GetByIdAsync(id, cancellationToken);
        if (existing == null) return false;

        existing.IsDeleted = true;
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Deleted territory {Id}", id);
        return true;
    }
}
