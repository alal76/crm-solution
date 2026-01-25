namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for dashboard analytics and statistics
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Get overall dashboard statistics
    /// </summary>
    Task<DashboardStats> GetStatsAsync();

    /// <summary>
    /// Get pipeline summary with stages and values
    /// </summary>
    Task<PipelineSummary> GetPipelineSummaryAsync();

    /// <summary>
    /// Get recent activities for dashboard
    /// </summary>
    Task<IEnumerable<DashboardActivity>> GetRecentActivitiesAsync(int count = 10);
}

/// <summary>
/// Dashboard statistics DTO
/// </summary>
public class DashboardStats
{
    public EntityCount Customers { get; set; } = new();
    public EntityCount Contacts { get; set; } = new();
    public OpportunityStats Opportunities { get; set; } = new();
    public EntityCount Products { get; set; } = new();
    public TaskStats Tasks { get; set; } = new();
    public UserStats Users { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class EntityCount
{
    public int Total { get; set; }
}

public class OpportunityStats
{
    public int Total { get; set; }
    public decimal OpenValue { get; set; }
    public decimal WonValue { get; set; }
}

public class TaskStats
{
    public int Total { get; set; }
    public int Pending { get; set; }
}

public class UserStats
{
    public int Active { get; set; }
}

/// <summary>
/// Pipeline summary DTO
/// </summary>
public class PipelineSummary
{
    public IEnumerable<PipelineStageData> Stages { get; set; } = new List<PipelineStageData>();
    public PipelineSummaryData Summary { get; set; } = new();
}

public class PipelineStageData
{
    public string Stage { get; set; } = string.Empty;
    public int StageValue { get; set; }
    public int Count { get; set; }
    public decimal TotalValue { get; set; }
    public decimal WeightedValue { get; set; }
}

public class PipelineSummaryData
{
    public decimal TotalValue { get; set; }
    public decimal WeightedValue { get; set; }
    public int OpportunityCount { get; set; }
}

/// <summary>
/// Dashboard activity DTO
/// </summary>
public class DashboardActivity
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime ActivityDate { get; set; }
    public string? Description { get; set; }
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
}
