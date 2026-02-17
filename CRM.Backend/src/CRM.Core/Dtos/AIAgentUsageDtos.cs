namespace CRM.Core.Dtos;

/// <summary>
/// DTO for AI agent usage response.
/// </summary>
public class AIAgentUsageDto
{
    public int Id { get; set; }
    public string AgentId { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public int RequestCount { get; set; }
    public int Tokens { get; set; }
    public decimal Cost { get; set; }
    public DateTime UsageDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for creating AI agent usage record.
/// </summary>
public class CreateAIAgentUsageDto
{
    public string AgentId { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public int RequestCount { get; set; }
    public int Tokens { get; set; }
    public decimal Cost { get; set; }
    public string? UsageDate { get; set; }
}
