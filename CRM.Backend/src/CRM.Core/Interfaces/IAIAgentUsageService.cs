using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for AI agent usage tracking.
/// </summary>
public interface IAIAgentUsageService
{
    Task<AIAgentUsageDto> CreateAsync(CreateAIAgentUsageDto dto, CancellationToken ct = default);
    Task<AIAgentUsageDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<AIAgentUsageDto>> GetAllAsync(CancellationToken ct = default);
}
