// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
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
