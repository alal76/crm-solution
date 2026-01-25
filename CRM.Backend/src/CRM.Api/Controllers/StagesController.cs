// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under the GNU Affero General Public License v3.0

using CRM.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// Stages Controller for pipeline stage management
/// Uses simplified 3NF OpportunityStage enum: Discovery, Qualification, Proposal, Negotiation, ClosedWon, ClosedLost
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StagesController : ControllerBase
{
    private readonly ILogger<StagesController> _logger;

    public StagesController(ILogger<StagesController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get all available pipeline stages
    /// </summary>
    [HttpGet]
    public IActionResult GetStages()
    {
        try
        {
            var stages = Enum.GetValues<OpportunityStage>()
                .Select(stage => new
                {
                    Id = (int)stage,
                    Key = stage.ToString(),
                    Name = GetStageName(stage),
                    Description = GetStageDescription(stage),
                    Probability = GetStageProbability(stage),
                    Color = GetStageColor(stage),
                    Order = (int)stage,
                    IsClosedStage = stage == OpportunityStage.ClosedWon || 
                                    stage == OpportunityStage.ClosedLost
                })
                .OrderBy(s => s.Order)
                .ToList();

            return Ok(stages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving stages");
            return StatusCode(500, new { message = "An error occurred while retrieving stages" });
        }
    }

    /// <summary>
    /// Get stage by ID
    /// </summary>
    [HttpGet("{id}")]
    public IActionResult GetStage(int id)
    {
        try
        {
            if (!Enum.IsDefined(typeof(OpportunityStage), id))
            {
                return NotFound(new { message = $"Stage with ID {id} not found" });
            }

            var stage = (OpportunityStage)id;
            return Ok(new
            {
                Id = id,
                Key = stage.ToString(),
                Name = GetStageName(stage),
                Description = GetStageDescription(stage),
                Probability = GetStageProbability(stage),
                Color = GetStageColor(stage),
                Order = id,
                IsClosedStage = stage == OpportunityStage.ClosedWon || 
                                stage == OpportunityStage.ClosedLost
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving stage {StageId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the stage" });
        }
    }

    /// <summary>
    /// Get active (non-closed) stages only
    /// </summary>
    [HttpGet("active")]
    public IActionResult GetActiveStages()
    {
        try
        {
            var closedStages = new[] 
            { 
                OpportunityStage.ClosedWon, 
                OpportunityStage.ClosedLost
            };

            var stages = Enum.GetValues<OpportunityStage>()
                .Where(s => !closedStages.Contains(s))
                .Select(stage => new
                {
                    Id = (int)stage,
                    Key = stage.ToString(),
                    Name = GetStageName(stage),
                    Probability = GetStageProbability(stage),
                    Color = GetStageColor(stage),
                    Order = (int)stage
                })
                .OrderBy(s => s.Order)
                .ToList();

            return Ok(stages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active stages");
            return StatusCode(500, new { message = "An error occurred while retrieving active stages" });
        }
    }

    private static string GetStageName(OpportunityStage stage) => stage switch
    {
        OpportunityStage.Discovery => "Discovery",
        OpportunityStage.Qualification => "Qualification",
        OpportunityStage.Proposal => "Proposal",
        OpportunityStage.Negotiation => "Negotiation",
        OpportunityStage.ClosedWon => "Closed Won",
        OpportunityStage.ClosedLost => "Closed Lost",
        _ => stage.ToString()
    };

    private static string GetStageDescription(OpportunityStage stage) => stage switch
    {
        OpportunityStage.Discovery => "Initial discovery phase - understanding customer needs",
        OpportunityStage.Qualification => "Qualifying the opportunity using BANT criteria",
        OpportunityStage.Proposal => "Formal proposal or quote submitted to customer",
        OpportunityStage.Negotiation => "Active negotiation on terms, pricing, and contracts",
        OpportunityStage.ClosedWon => "Deal successfully closed",
        OpportunityStage.ClosedLost => "Deal lost to competition or no decision",
        _ => ""
    };

    private static int GetStageProbability(OpportunityStage stage) => stage switch
    {
        OpportunityStage.Discovery => 10,
        OpportunityStage.Qualification => 25,
        OpportunityStage.Proposal => 50,
        OpportunityStage.Negotiation => 75,
        OpportunityStage.ClosedWon => 100,
        OpportunityStage.ClosedLost => 0,
        _ => 0
    };

    private static string GetStageColor(OpportunityStage stage) => stage switch
    {
        OpportunityStage.Discovery => "#E3F2FD",
        OpportunityStage.Qualification => "#BBDEFB",
        OpportunityStage.Proposal => "#64B5F6",
        OpportunityStage.Negotiation => "#1976D2",
        OpportunityStage.ClosedWon => "#4CAF50",
        OpportunityStage.ClosedLost => "#F44336",
        _ => "#757575"
    };
}
