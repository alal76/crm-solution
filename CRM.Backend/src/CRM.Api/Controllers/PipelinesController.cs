// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under the GNU Affero General Public License v3.0

using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.Api.Controllers;

/// <summary>
/// Pipelines Controller for sales pipeline management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PipelinesController : ControllerBase
{
    private readonly CrmDbContext _context;
    private readonly ILogger<PipelinesController> _logger;

    public PipelinesController(CrmDbContext context, ILogger<PipelinesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all pipeline definitions (stages)
    /// </summary>
    [HttpGet]
    public IActionResult GetPipelines()
    {
        try
        {
            // Return the default sales pipeline with all stages
            var defaultPipeline = new
            {
                Id = Guid.NewGuid(),
                Name = "Sales Pipeline",
                Description = "Default sales pipeline for opportunity management",
                IsDefault = true,
                Stages = GetDefaultPipelineStages()
            };

            return Ok(new[] { defaultPipeline });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pipelines");
            return StatusCode(500, new { message = "An error occurred while retrieving pipelines" });
        }
    }

    /// <summary>
    /// Get pipeline by ID
    /// </summary>
    [HttpGet("{id}")]
    public IActionResult GetPipeline(Guid id)
    {
        try
        {
            var pipeline = new
            {
                Id = id,
                Name = "Sales Pipeline",
                Description = "Default sales pipeline for opportunity management",
                IsDefault = true,
                Stages = GetDefaultPipelineStages()
            };

            return Ok(pipeline);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pipeline {PipelineId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the pipeline" });
        }
    }

    /// <summary>
    /// Get pipeline statistics
    /// </summary>
    [HttpGet("{id}/stats")]
    public async Task<IActionResult> GetPipelineStats(Guid id)
    {
        try
        {
            var stats = await _context.Opportunities
                .Where(o => !o.IsDeleted && o.Stage != OpportunityStage.ClosedWon && o.Stage != OpportunityStage.ClosedLost)
                .GroupBy(o => o.Stage)
                .Select(g => new
                {
                    Stage = g.Key.ToString(),
                    StageOrder = (int)g.Key,
                    Count = g.Count(),
                    TotalValue = g.Sum(o => o.Amount),
                    AverageValue = g.Average(o => o.Amount)
                })
                .OrderBy(s => s.StageOrder)
                .ToListAsync();

            return Ok(new
            {
                PipelineId = id,
                Stats = stats,
                TotalOpportunities = stats.Sum(s => s.Count),
                TotalValue = stats.Sum(s => s.TotalValue)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pipeline stats");
            return StatusCode(500, new { message = "An error occurred while retrieving pipeline statistics" });
        }
    }

    private static object[] GetDefaultPipelineStages()
    {
        return new object[]
        {
            new { Order = 0, Name = "Prospecting", Key = "Prospecting", Probability = 5, Color = "#E3F2FD" },
            new { Order = 1, Name = "Qualification", Key = "Qualification", Probability = 10, Color = "#BBDEFB" },
            new { Order = 2, Name = "Needs Analysis", Key = "NeedsAnalysis", Probability = 20, Color = "#90CAF9" },
            new { Order = 3, Name = "Value Proposition", Key = "ValueProposition", Probability = 35, Color = "#64B5F6" },
            new { Order = 4, Name = "Identify Decision Makers", Key = "IdentifyDecisionMakers", Probability = 50, Color = "#42A5F5" },
            new { Order = 5, Name = "Perception Analysis", Key = "PerceptionAnalysis", Probability = 60, Color = "#2196F3" },
            new { Order = 6, Name = "Proposal/Quote", Key = "ProposalQuote", Probability = 70, Color = "#1E88E5" },
            new { Order = 7, Name = "Negotiation/Review", Key = "NegotiationReview", Probability = 80, Color = "#1976D2" },
            new { Order = 8, Name = "Verbal Commitment", Key = "VerbalCommitment", Probability = 90, Color = "#1565C0" },
            new { Order = 9, Name = "Contract Sent", Key = "ContractSent", Probability = 95, Color = "#0D47A1" },
            new { Order = 10, Name = "Closed Won", Key = "ClosedWon", Probability = 100, Color = "#4CAF50" },
            new { Order = 11, Name = "Closed Lost", Key = "ClosedLost", Probability = 0, Color = "#F44336" }
        };
    }
}
