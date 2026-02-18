// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.DTOs.ITSM;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.ITSM;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.ITSM;

public class CMDBService : ICMDBService
{
    private readonly IDbContextResolver _dbContextResolver;
    private readonly ILogger<CMDBService> _logger;

    public CMDBService(IDbContextResolver dbContextResolver, ILogger<CMDBService> logger)
    {
        _dbContextResolver = dbContextResolver;
        _logger = logger;
    }

    public async Task<ConfigurationItemDto> CreateCIAsync(CreateCIDto dto, int createdById)
    {
        var context = _dbContextResolver.ResolveContext();

        var ci = new ConfigurationItem
        {
            CINumber = await GenerateCINumberAsync(context),
            CIName = dto.CIName,
            CIType = dto.CIType,
            CISubtype = dto.CISubtype,
            Description = dto.Description,
            SerialNumber = dto.SerialNumber,
            IPAddress = dto.IPAddress,
            OwnerId = dto.OwnerId,
            OperationalStatus = dto.OperationalStatus,
            CreatedById = createdById,
            CreatedAt = DateTime.UtcNow
        };

        context.ConfigurationItems.Add(ci);
        await context.SaveChangesAsync();

        _logger.LogInformation("Created CI {CINumber}", ci.CINumber);
        return MapToDto(ci);
    }

    public async Task<ConfigurationItemDto?> GetCIByIdAsync(int ciId)
    {
        var context = _dbContextResolver.ResolveContext();
        var ci = await context.ConfigurationItems
            .Include(c => c.Owner)
            .FirstOrDefaultAsync(c => c.CIId == ciId && !c.IsDeleted);

        return ci == null ? null : MapToDto(ci);
    }

    public async Task<IEnumerable<ConfigurationItemDto>> SearchCIsAsync(string searchTerm, CIType? type, int pageNumber, int pageSize)
    {
        var context = _dbContextResolver.ResolveContext();
        var query = context.ConfigurationItems
            .Include(c => c.Owner)
            .Where(c => !c.IsDeleted);

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(c => c.CIName.Contains(searchTerm) ||
                                    c.CINumber.Contains(searchTerm) ||
                                    (c.SerialNumber != null && c.SerialNumber.Contains(searchTerm)));
        }

        if (type.HasValue)
            query = query.Where(c => c.CIType == type.Value);

        var cis = await query
            .OrderBy(c => c.CIName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return cis.Select(MapToDto);
    }

    public async Task<ConfigurationItemDto> UpdateCIAsync(int ciId, CreateCIDto dto, int modifiedById)
    {
        var context = _dbContextResolver.ResolveContext();
        var ci = await context.ConfigurationItems.FindAsync(ciId);

        if (ci == null || ci.IsDeleted)
            throw new KeyNotFoundException($"CI {ciId} not found");

        ci.CIName = dto.CIName;
        ci.CIType = dto.CIType;
        ci.CISubtype = dto.CISubtype;
        ci.Description = dto.Description;
        ci.SerialNumber = dto.SerialNumber;
        ci.IPAddress = dto.IPAddress;
        ci.OwnerId = dto.OwnerId;
        ci.OperationalStatus = dto.OperationalStatus;
        ci.ModifiedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return MapToDto(ci);
    }

    public async Task<bool> CreateRelationshipAsync(int parentCIId, int childCIId, CRM.Core.Entities.ITSM.RelationshipType type, int createdById)
    {
        var context = _dbContextResolver.ResolveContext();

        var existing = await context.CIRelationships
            .AnyAsync(r => r.ParentCIId == parentCIId && r.ChildCIId == childCIId && !r.IsDeleted);

        if (existing)
            return false;

        context.CIRelationships.Add(new CIRelationship
        {
            ParentCIId = parentCIId,
            ChildCIId = childCIId,
            RelationshipType = type,
            CreatedById = createdById,
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
        _logger.LogInformation("Created CI relationship: {ParentCIId} -> {ChildCIId}", parentCIId, childCIId);
        return true;
    }

    public async Task<IEnumerable<ConfigurationItemDto>> GetRelatedCIsAsync(int ciId)
    {
        var context = _dbContextResolver.ResolveContext();

        var childIds = await context.CIRelationships
            .Where(r => r.ParentCIId == ciId && !r.IsDeleted)
            .Select(r => r.ChildCIId)
            .ToListAsync();

        var parentIds = await context.CIRelationships
            .Where(r => r.ChildCIId == ciId && !r.IsDeleted)
            .Select(r => r.ParentCIId)
            .ToListAsync();

        var relatedIds = childIds.Concat(parentIds).Distinct().ToList();

        var cis = await context.ConfigurationItems
            .Include(c => c.Owner)
            .Where(c => relatedIds.Contains(c.CIId) && !c.IsDeleted)
            .ToListAsync();

        return cis.Select(MapToDto);
    }

    public async Task<IEnumerable<string>> GetImpactAnalysisAsync(int ciId)
    {
        var context = _dbContextResolver.ResolveContext();
        var impacts = new List<string>();

        // Get all dependent CIs recursively
        var dependents = await GetDependentCIsRecursiveAsync(context, ciId);
        impacts.Add($"Affects {dependents.Count} configuration items");

        // Get affected services
        var services = await context.ServiceCIs
            .Where(sc => sc.CIId == ciId || dependents.Contains(sc.CIId))
            .Include(sc => sc.Service)
            .Select(sc => sc.Service!.ServiceName)
            .Distinct()
            .ToListAsync();

        if (services.Any())
            impacts.Add($"Affects {services.Count} services: {string.Join(", ", services)}");

        // Get active incidents related to this CI
        var incidents = await context.Incidents
            .Where(i => i.ConfigurationItemId == ciId && i.State != IncidentState.Closed && !i.IsDeleted)
            .CountAsync();

        if (incidents > 0)
            impacts.Add($"{incidents} active incidents related to this CI");

        return impacts;
    }

    private async Task<List<int>> GetDependentCIsRecursiveAsync(ICrmDbContext context, int ciId, HashSet<int>? visited = null)
    {
        visited ??= new HashSet<int>();

        if (visited.Contains(ciId))
            return new List<int>();

        visited.Add(ciId);

        var children = await context.CIRelationships
            .Where(r => r.ParentCIId == ciId && !r.IsDeleted)
            .Select(r => r.ChildCIId)
            .ToListAsync();

        var allDependents = new List<int>(children);

        foreach (var childId in children)
        {
            var childDependents = await GetDependentCIsRecursiveAsync(context, childId, visited);
            allDependents.AddRange(childDependents);
        }

        return allDependents.Distinct().ToList();
    }

    private async Task<string> GenerateCINumberAsync(ICrmDbContext context)
    {
        var lastCI = await context.ConfigurationItems
            .OrderByDescending(c => c.CIId)
            .FirstOrDefaultAsync();

        var nextNumber = lastCI != null ? lastCI.CIId + 1 : 1;
        return $"CI{nextNumber:D7}";
    }

    private ConfigurationItemDto MapToDto(ConfigurationItem ci)
    {
        return new ConfigurationItemDto
        {
            CIId = ci.CIId,
            CIName = ci.CIName,
            CINumber = ci.CINumber,
            CIType = ci.CIType,
            CISubtype = ci.CISubtype,
            OperationalStatus = ci.OperationalStatus,
            SerialNumber = ci.SerialNumber,
            IPAddress = ci.IPAddress,
            OwnerId = ci.OwnerId,
            OwnerName = ci.Owner?.Username,
            CreatedAt = ci.CreatedAt
        };
    }
}
