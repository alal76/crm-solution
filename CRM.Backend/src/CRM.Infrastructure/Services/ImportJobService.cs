using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service implementation for import job management.
/// </summary>
public class ImportJobService : IImportJobService
{
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<ImportJobService> _logger;

    public ImportJobService(ICrmDbContext dbContext, ILogger<ImportJobService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ImportJobDto> CreateAsync(CreateImportJobDto dto, CancellationToken ct = default)
    {
        var entity = new ImportJob
        {
            Entity = dto.Entity,
            Source = dto.Source,
            Status = dto.Status ?? "Completed",
            SubmittedByUserId = dto.SubmittedByUserId,
            SubmittedDate = DateTime.TryParse(dto.SubmittedDate, out var d) ? d : DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        _dbContext.ImportJobs.Add(entity);
        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("Created import job {Id} for entity {Entity}", entity.Id, entity.Entity);

        return MapToDto(entity);
    }

    public async Task<ImportJobDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _dbContext.ImportJobs
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, ct);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<ImportJobDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _dbContext.ImportJobs
            .Where(e => !e.IsDeleted)
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => MapToDto(e))
            .ToListAsync(ct);
    }

    private static ImportJobDto MapToDto(ImportJob e) => new()
    {
        Id = e.Id,
        Entity = e.Entity,
        Source = e.Source,
        Status = e.Status,
        SubmittedByUserId = e.SubmittedByUserId,
        SubmittedDate = e.SubmittedDate,
        CompletedDate = e.CompletedDate,
        TotalRecords = e.TotalRecords,
        SuccessCount = e.SuccessCount,
        FailureCount = e.FailureCount,
        ErrorMessage = e.ErrorMessage,
        CreatedAt = e.CreatedAt,
    };
}
