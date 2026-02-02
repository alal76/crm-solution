// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

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
