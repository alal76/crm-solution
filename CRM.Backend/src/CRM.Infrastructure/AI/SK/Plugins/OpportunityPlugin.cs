// -----------------------------------------------------------------------
// CRM Solution - Semantic Kernel AI Plugins
// Copyright (c) 2024-2026 Abhishek Lal (CRM Solution). All rights reserved.
// Licensed under the GNU Affero General Public License v3.0.
// See LICENSE file in the project root for full license information.
//
// This file is part of the CRM Solution, an enterprise-grade
// Customer Relationship Management system.
//
// Author: Abhishek Lal
// Repository: https://github.com/abhisheklal04/crm-solution
// Documentation: See /docs folder for architecture and API reference
//
// IMPORTANT: This is proprietary code. Unauthorized copying, modification,
// or distribution is strictly prohibited.
// -----------------------------------------------------------------------

#nullable enable

using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.AI.SK.Attributes;

namespace CRM.Infrastructure.AI.SK.Plugins;

/// <summary>
/// Semantic Kernel plugin for CRM Opportunity and pipeline management.
/// Provides AI-accessible functions for viewing pipeline, win rates, and updating opportunity stages.
/// </summary>
public class OpportunityPlugin : CrmPluginBase
{
    private readonly IOpportunityService _opportunityService;
    private readonly ICrmDbContext _context;

    /// <inheritdoc />
    public override string PluginName => "Opportunity";

    /// <inheritdoc />
    public override string Description => "Manage CRM opportunities — view pipeline, check win rates, update stages, and add notes to deals.";

    /// <summary>
    /// Initializes a new instance of the <see cref="OpportunityPlugin"/> class.
    /// </summary>
    /// <param name="opportunityService">The opportunity service for CRUD operations.</param>
    /// <param name="context">The database context for direct queries.</param>
    /// <param name="logger">The logger instance.</param>
    public OpportunityPlugin(
        IOpportunityService opportunityService,
        ICrmDbContext context,
        ILogger<OpportunityPlugin> logger) : base(logger)
    {
        _opportunityService = opportunityService ?? throw new ArgumentNullException(nameof(opportunityService));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    #region Read Methods

    /// <summary>
    /// Retrieves a single opportunity by its ID.
    /// </summary>
    [KernelFunction("GetOpportunity")]
    [Description("Get detailed information about a specific sales opportunity by its ID.")]
    public async Task<string> GetOpportunityAsync(
        [Description("The unique identifier of the opportunity.")] int opportunityId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var opportunity = await _opportunityService.GetOpportunityByIdAsync(opportunityId);
            return opportunity != null
                ? SuccessResult(opportunity)
                : ErrorResult("GetOpportunity", $"Opportunity with ID {opportunityId} not found.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving opportunity {OpportunityId}", opportunityId);
            return ErrorResult("GetOpportunity", ex.Message);
        }
    }

    /// <summary>
    /// Gets the sales pipeline, optionally filtered by account.
    /// </summary>
    [KernelFunction("GetPipeline")]
    [Description("Get the current sales pipeline with all open opportunities. Optionally filter by a specific account.")]
    public async Task<string> GetPipelineAsync(
        [Description("Optional account ID to filter opportunities for a specific account. Pass 0 or omit for all.")] int? accountId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            IEnumerable<Opportunity> opportunities;

            if (accountId.HasValue && accountId.Value > 0)
            {
                opportunities = await _opportunityService.GetOpportunitiesByCustomerAsync(accountId.Value);
            }
            else
            {
                opportunities = await _opportunityService.GetOpenOpportunitiesAsync();
            }

            var list = opportunities.ToList();
            var totalPipeline = await _opportunityService.GetTotalPipelineAsync();

            return SuccessResult(new
            {
                count = list.Count,
                totalPipelineValue = totalPipeline,
                opportunities = list.Select(o => new
                {
                    o.Id,
                    o.Name,
                    o.Stage,
                    o.Amount,
                    o.ExpectedCloseDate,
                    o.AccountId
                })
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving pipeline for account {AccountId}", accountId);
            return ErrorResult("GetPipeline", ex.Message);
        }
    }

    /// <summary>
    /// Gets win/loss rate statistics from all opportunities.
    /// </summary>
    [KernelFunction("GetWinRates")]
    [Description("Get win/loss rate statistics across all closed opportunities.")]
    public async Task<string> GetWinRatesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var opportunities = await _context.Opportunities
                .Where(o => !o.IsDeleted)
                .ToListAsync(cancellationToken);

            var closed = opportunities.Where(o =>
                o.Stage == OpportunityStage.ClosedWon || o.Stage == OpportunityStage.ClosedLost).ToList();
            var won = closed.Count(o => o.Stage == OpportunityStage.ClosedWon);
            var lost = closed.Count(o => o.Stage == OpportunityStage.ClosedLost);
            var total = closed.Count;
            var winRate = total > 0 ? Math.Round((double)won / total * 100, 2) : 0;
            var avgDealSize = won > 0
                ? closed.Where(o => o.Stage == OpportunityStage.ClosedWon).Average(o => o.Amount)
                : 0m;

            return SuccessResult(new
            {
                totalClosed = total,
                won,
                lost,
                winRate,
                averageDealSize = avgDealSize,
                totalOpenOpportunities = opportunities.Count(o =>
                    o.Stage != OpportunityStage.ClosedWon && o.Stage != OpportunityStage.ClosedLost)
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error calculating win rates");
            return ErrorResult("GetWinRates", ex.Message);
        }
    }

    #endregion

    #region Write Methods

    /// <summary>
    /// Updates the stage of an opportunity.
    /// </summary>
    [RequiresApproval(Tier = "standard", Description = "Changes the sales stage of an opportunity, which may affect pipeline forecasting.")]
    [KernelFunction("UpdateStage")]
    [Description("Update the sales stage of an opportunity (e.g., Qualification, Proposal, Negotiation, Closed Won).")]
    public async Task<string> UpdateStageAsync(
        [Description("The unique identifier of the opportunity.")] int opportunityId,
        [Description("The new stage name (e.g., Qualification, Proposal, Negotiation, Closed Won, Closed Lost).")] string newStage,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var opportunity = await _opportunityService.GetOpportunityByIdAsync(opportunityId);
            if (opportunity == null)
                return ErrorResult("UpdateStage", $"Opportunity with ID {opportunityId} not found.");

            var previousStage = opportunity.Stage;
            if (!Enum.TryParse<OpportunityStage>(newStage.Replace(" ", ""), true, out var parsedStage))
                return ErrorResult("UpdateStage", $"Invalid stage '{newStage}'. Valid stages: {string.Join(", ", Enum.GetNames<OpportunityStage>())}");

            opportunity.Stage = parsedStage;
            opportunity.UpdatedAt = DateTime.UtcNow;

            await _opportunityService.UpdateOpportunityAsync(opportunity);

            return SuccessResult(new { updated = true, opportunityId, previousStage, newStage });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating stage for opportunity {OpportunityId}", opportunityId);
            return ErrorResult("UpdateStage", ex.Message);
        }
    }

    /// <summary>
    /// Adds a note to an opportunity.
    /// </summary>
    [RequiresApproval(Tier = "low", Description = "Adds a text note to an opportunity record.")]
    [KernelFunction("AddOpportunityNote")]
    [Description("Add a text note to a sales opportunity for record-keeping.")]
    public async Task<string> AddOpportunityNoteAsync(
        [Description("The unique identifier of the opportunity.")] int opportunityId,
        [Description("The content of the note to add.")] string noteContent,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var opportunity = await _opportunityService.GetOpportunityByIdAsync(opportunityId);
            if (opportunity == null)
                return ErrorResult("AddOpportunityNote", $"Opportunity with ID {opportunityId} not found.");

            var note = new Note
            {
                Title = "AI-Generated Note",
                Content = noteContent,
                EntityType = "Opportunity",
                EntityId = opportunityId,
                NoteType = NoteType.General,
                Visibility = NoteVisibility.Team,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Notes.Add(note);
            await _context.SaveChangesAsync(cancellationToken);

            return SuccessResult(new { noteId = note.Id, opportunityId, content = noteContent });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error adding note to opportunity {OpportunityId}", opportunityId);
            return ErrorResult("AddOpportunityNote", ex.Message);
        }
    }

    #endregion
}
