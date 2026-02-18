// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for managing sales pipelines
/// </summary>
public class PipelineService : IPipelineService
{
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<PipelineService> _logger;

    // Default pipeline stages based on OpportunityStage enum
    private static readonly List<PipelineStage> DefaultStages = new()
    {
        new PipelineStage { Order = 1, Name = "Qualification", Key = "Qualification", Probability = 10, Color = "#6B7280" },
        new PipelineStage { Order = 2, Name = "Needs Analysis", Key = "NeedsAnalysis", Probability = 20, Color = "#3B82F6" },
        new PipelineStage { Order = 3, Name = "Value Proposition", Key = "ValueProposition", Probability = 40, Color = "#8B5CF6" },
        new PipelineStage { Order = 4, Name = "Identify Decision Makers", Key = "IdentifyDecisionMakers", Probability = 50, Color = "#EC4899" },
        new PipelineStage { Order = 5, Name = "Perception Analysis", Key = "PerceptionAnalysis", Probability = 60, Color = "#F97316" },
        new PipelineStage { Order = 6, Name = "Proposal/Price Quote", Key = "ProposalPriceQuote", Probability = 70, Color = "#EAB308" },
        new PipelineStage { Order = 7, Name = "Negotiation/Review", Key = "NegotiationReview", Probability = 80, Color = "#22C55E" },
        new PipelineStage { Order = 8, Name = "Won", Key = "Won", Probability = 100, Color = "#10B981" },
        new PipelineStage { Order = 9, Name = "Lost", Key = "Lost", Probability = 0, Color = "#EF4444" }
    };

    // Default pipeline definition
    private static readonly PipelineDefinition DefaultPipeline = new()
    {
        Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
        Name = "Default Sales Pipeline",
        Description = "Standard sales pipeline with qualification through closure stages",
        IsDefault = true,
        Stages = DefaultStages
    };

    public PipelineService(ICrmDbContext dbContext, ILogger<PipelineService> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task<IEnumerable<PipelineDefinition>> GetPipelinesAsync()
    {
        _logger.LogDebug("Getting all pipelines");

        // For now, return the default pipeline
        // This can be extended to support custom pipelines stored in the database
        var pipelines = new List<PipelineDefinition> { DefaultPipeline };

        return Task.FromResult<IEnumerable<PipelineDefinition>>(pipelines);
    }

    /// <inheritdoc />
    public Task<PipelineDefinition?> GetByIdAsync(Guid id)
    {
        _logger.LogDebug("Getting pipeline by ID: {PipelineId}", id);

        // For now, only the default pipeline exists
        if (id == DefaultPipeline.Id)
        {
            return Task.FromResult<PipelineDefinition?>(DefaultPipeline);
        }

        return Task.FromResult<PipelineDefinition?>(null);
    }

    /// <inheritdoc />
    public async Task<PipelineStatistics> GetStatsAsync(Guid pipelineId)
    {
        _logger.LogDebug("Getting statistics for pipeline: {PipelineId}", pipelineId);

        try
        {
            // Get all opportunities grouped by stage
            var opportunities = await _dbContext.Opportunities
                .Where(o => !o.IsDeleted)
                .ToListAsync();

            var stageStats = opportunities
                .GroupBy(o => o.Stage)
                .Select(g => new PipelineStageStats
                {
                    Stage = g.Key.ToString(),
                    StageOrder = (int)g.Key,
                    Count = g.Count(),
                    TotalValue = g.Sum(o => o.Amount),
                    AverageValue = g.Any() ? g.Average(o => o.Amount) : 0
                })
                .OrderBy(s => s.StageOrder)
                .ToList();

            // Ensure all stages are represented, even if empty
            var allStageStats = new List<PipelineStageStats>();
            foreach (var stage in DefaultStages)
            {
                var existing = stageStats.FirstOrDefault(s => s.Stage == stage.Key);
                if (existing != null)
                {
                    allStageStats.Add(existing);
                }
                else
                {
                    allStageStats.Add(new PipelineStageStats
                    {
                        Stage = stage.Key,
                        StageOrder = stage.Order,
                        Count = 0,
                        TotalValue = 0,
                        AverageValue = 0
                    });
                }
            }

            return new PipelineStatistics
            {
                PipelineId = pipelineId,
                Stats = allStageStats,
                TotalOpportunities = opportunities.Count,
                TotalValue = opportunities.Sum(o => o.Amount)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pipeline statistics for {PipelineId}", pipelineId);
            throw;
        }
    }

    /// <inheritdoc />
    public IEnumerable<PipelineStage> GetDefaultStages()
    {
        _logger.LogDebug("Getting default pipeline stages");
        return DefaultStages;
    }
}
