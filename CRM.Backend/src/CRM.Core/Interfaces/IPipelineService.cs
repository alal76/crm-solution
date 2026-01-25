using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for managing sales pipelines
/// </summary>
public interface IPipelineService
{
    /// <summary>
    /// Get all pipeline definitions
    /// </summary>
    Task<IEnumerable<PipelineDefinition>> GetPipelinesAsync();

    /// <summary>
    /// Get a pipeline by ID
    /// </summary>
    Task<PipelineDefinition?> GetByIdAsync(Guid id);

    /// <summary>
    /// Get pipeline statistics
    /// </summary>
    Task<PipelineStatistics> GetStatsAsync(Guid pipelineId);

    /// <summary>
    /// Get default pipeline stages
    /// </summary>
    IEnumerable<PipelineStage> GetDefaultStages();
}

/// <summary>
/// Pipeline definition DTO
/// </summary>
public class PipelineDefinition
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
    public IEnumerable<PipelineStage> Stages { get; set; } = new List<PipelineStage>();
}

/// <summary>
/// Pipeline stage DTO
/// </summary>
public class PipelineStage
{
    public int Order { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public int Probability { get; set; }
    public string Color { get; set; } = string.Empty;
}

/// <summary>
/// Pipeline statistics DTO
/// </summary>
public class PipelineStatistics
{
    public Guid PipelineId { get; set; }
    public IEnumerable<PipelineStageStats> Stats { get; set; } = new List<PipelineStageStats>();
    public int TotalOpportunities { get; set; }
    public decimal TotalValue { get; set; }
}

public class PipelineStageStats
{
    public string Stage { get; set; } = string.Empty;
    public int StageOrder { get; set; }
    public int Count { get; set; }
    public decimal TotalValue { get; set; }
    public decimal AverageValue { get; set; }
}
