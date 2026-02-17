using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service implementation for AI agent usage tracking.
/// </summary>
public class AIAgentUsageService : IAIAgentUsageService
{
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<AIAgentUsageService> _logger;

    public AIAgentUsageService(ICrmDbContext dbContext, ILogger<AIAgentUsageService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<AIAgentUsageDto> CreateAsync(CreateAIAgentUsageDto dto, CancellationToken ct = default)
    {
        var entity = new AIAgentUsage
        {
            AgentId = dto.AgentId,
            UserId = dto.UserId,
            RequestCount = dto.RequestCount,
            Tokens = dto.Tokens,
            Cost = dto.Cost,
            UsageDate = DateTime.TryParse(dto.UsageDate, out var d) ? d : DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        _dbContext.AIAgentUsages.Add(entity);
        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("Created AI agent usage record {Id} for agent {AgentId}", entity.Id, entity.AgentId);

        return MapToDto(entity);
    }

    public async Task<AIAgentUsageDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _dbContext.AIAgentUsages
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, ct);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<AIAgentUsageDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _dbContext.AIAgentUsages
            .Where(e => !e.IsDeleted)
            .OrderByDescending(e => e.UsageDate)
            .Select(e => MapToDto(e))
            .ToListAsync(ct);
    }

    private static AIAgentUsageDto MapToDto(AIAgentUsage e) => new()
    {
        Id = e.Id,
        AgentId = e.AgentId,
        UserId = e.UserId,
        RequestCount = e.RequestCount,
        Tokens = e.Tokens,
        Cost = e.Cost,
        UsageDate = e.UsageDate,
        CreatedAt = e.CreatedAt,
    };
}
