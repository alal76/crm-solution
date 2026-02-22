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
/// Service implementation for export job management.
/// </summary>
public class ExportJobService : IExportJobService
{
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<ExportJobService> _logger;

    public ExportJobService(ICrmDbContext dbContext, ILogger<ExportJobService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ExportJobDto> CreateAsync(CreateExportJobDto dto, CancellationToken ct = default)
    {
        var entity = new ExportJob
        {
            Entity = dto.Entity,
            Destination = dto.Destination,
            Status = dto.Status ?? "Completed",
            RequestedByUserId = dto.RequestedByUserId,
            RequestedDate = DateTime.TryParse(dto.RequestedDate, out var d) ? d : DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        _dbContext.ExportJobs.Add(entity);
        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("Created export job {Id} for entity {Entity}", entity.Id, entity.Entity);

        return MapToDto(entity);
    }

    public async Task<ExportJobDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _dbContext.ExportJobs
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, ct);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<ExportJobDto>> GetAllAsync(CancellationToken ct = default)
    {
        var entities = await _dbContext.ExportJobs
            .Where(e => !e.IsDeleted)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(ct);
        return entities.Select(MapToDto).ToList();
    }

    private static ExportJobDto MapToDto(ExportJob e) => new()
    {
        Id = e.Id,
        Entity = e.Entity,
        Destination = e.Destination,
        Status = e.Status,
        RequestedByUserId = e.RequestedByUserId,
        RequestedDate = e.RequestedDate,
        CompletedDate = e.CompletedDate,
        TotalRecords = e.TotalRecords,
        ErrorMessage = e.ErrorMessage,
        CreatedAt = e.CreatedAt,
    };
}
