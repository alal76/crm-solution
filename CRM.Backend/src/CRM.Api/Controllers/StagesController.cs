// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under the GNU Affero General Public License v3.0

using CRM.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// Stages Controller for pipeline stage management
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
                                    stage == OpportunityStage.ClosedLost ||
                                    stage == OpportunityStage.Disqualified
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
                                stage == OpportunityStage.ClosedLost ||
                                stage == OpportunityStage.Disqualified
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
                OpportunityStage.ClosedLost, 
                OpportunityStage.Disqualified,
                OpportunityStage.OnHold
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
        OpportunityStage.Prospecting => "Prospecting",
        OpportunityStage.Qualification => "Qualification",
        OpportunityStage.NeedsAnalysis => "Needs Analysis",
        OpportunityStage.ValueProposition => "Value Proposition",
        OpportunityStage.IdentifyDecisionMakers => "Identify Decision Makers",
        OpportunityStage.PerceptionAnalysis => "Perception Analysis",
        OpportunityStage.ProposalQuote => "Proposal/Quote",
        OpportunityStage.NegotiationReview => "Negotiation/Review",
        OpportunityStage.VerbalCommitment => "Verbal Commitment",
        OpportunityStage.ContractSent => "Contract Sent",
        OpportunityStage.ClosedWon => "Closed Won",
        OpportunityStage.ClosedLost => "Closed Lost",
        OpportunityStage.OnHold => "On Hold",
        OpportunityStage.Disqualified => "Disqualified",
        _ => stage.ToString()
    };

    private static string GetStageDescription(OpportunityStage stage) => stage switch
    {
        OpportunityStage.Prospecting => "Initial contact - identifying potential opportunities",
        OpportunityStage.Qualification => "Qualifying the opportunity using BANT criteria",
        OpportunityStage.NeedsAnalysis => "Deep dive into customer requirements and pain points",
        OpportunityStage.ValueProposition => "Presenting value proposition aligned to needs",
        OpportunityStage.IdentifyDecisionMakers => "Mapping the decision-making unit and influences",
        OpportunityStage.PerceptionAnalysis => "Understanding stakeholder perceptions and objections",
        OpportunityStage.ProposalQuote => "Formal proposal or quote submitted",
        OpportunityStage.NegotiationReview => "Active negotiation on terms, pricing, contracts",
        OpportunityStage.VerbalCommitment => "Verbal agreement obtained, pending contract",
        OpportunityStage.ContractSent => "Contract sent for signature",
        OpportunityStage.ClosedWon => "Deal successfully closed",
        OpportunityStage.ClosedLost => "Deal lost to competition or no decision",
        OpportunityStage.OnHold => "Opportunity put on hold by customer",
        OpportunityStage.Disqualified => "Opportunity disqualified - not a fit",
        _ => ""
    };

    private static int GetStageProbability(OpportunityStage stage) => stage switch
    {
        OpportunityStage.Prospecting => 5,
        OpportunityStage.Qualification => 10,
        OpportunityStage.NeedsAnalysis => 20,
        OpportunityStage.ValueProposition => 35,
        OpportunityStage.IdentifyDecisionMakers => 50,
        OpportunityStage.PerceptionAnalysis => 60,
        OpportunityStage.ProposalQuote => 70,
        OpportunityStage.NegotiationReview => 80,
        OpportunityStage.VerbalCommitment => 90,
        OpportunityStage.ContractSent => 95,
        OpportunityStage.ClosedWon => 100,
        _ => 0
    };

    private static string GetStageColor(OpportunityStage stage) => stage switch
    {
        OpportunityStage.Prospecting => "#E3F2FD",
        OpportunityStage.Qualification => "#BBDEFB",
        OpportunityStage.NeedsAnalysis => "#90CAF9",
        OpportunityStage.ValueProposition => "#64B5F6",
        OpportunityStage.IdentifyDecisionMakers => "#42A5F5",
        OpportunityStage.PerceptionAnalysis => "#2196F3",
        OpportunityStage.ProposalQuote => "#1E88E5",
        OpportunityStage.NegotiationReview => "#1976D2",
        OpportunityStage.VerbalCommitment => "#1565C0",
        OpportunityStage.ContractSent => "#0D47A1",
        OpportunityStage.ClosedWon => "#4CAF50",
        OpportunityStage.ClosedLost => "#F44336",
        OpportunityStage.OnHold => "#FF9800",
        OpportunityStage.Disqualified => "#9E9E9E",
        _ => "#757575"
    };
}
