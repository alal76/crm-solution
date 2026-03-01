// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
#nullable enable

using System.ComponentModel;
using CRM.Core.Interfaces;
using CRM.Infrastructure.AI.SK.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace CRM.Infrastructure.AI.SK.Plugins;

/// <summary>
/// Semantic Kernel plugin for CRM Lead management operations.
/// Provides AI-accessible functions for querying, scoring, and converting leads.
/// </summary>
public class LeadPlugin : CrmPluginBase
{
    private readonly ILeadService _leadService;

    /// <inheritdoc />
    public override string PluginName => "Lead";

    /// <inheritdoc />
    public override string Description => "Manage CRM leads — search, view details, check scores, get statistics, update scores, and convert leads to opportunities.";

    /// <summary>
    /// Initializes a new instance of the <see cref="LeadPlugin"/> class.
    /// </summary>
    /// <param name="leadService">The lead service for CRUD operations.</param>
    /// <param name="logger">The logger instance.</param>
    public LeadPlugin(
        ILeadService leadService,
        ILogger<LeadPlugin> logger) : base(logger)
    {
        _leadService = leadService ?? throw new ArgumentNullException(nameof(leadService));
    }

    #region Read Methods

    /// <summary>
    /// Retrieves a single lead by its ID.
    /// </summary>
    [KernelFunction("GetLead")]
    [Description("Get detailed information about a specific sales lead by its ID.")]
    public async Task<string> GetLeadAsync(
        [Description("The unique identifier of the lead.")] int leadId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var lead = await _leadService.GetByIdAsync(leadId);
            return lead != null
                ? SuccessResult(lead)
                : ErrorResult("GetLead", $"Lead with ID {leadId} not found.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving lead {LeadId}", leadId);
            return ErrorResult("GetLead", ex.Message);
        }
    }

    /// <summary>
    /// Searches leads by a query string.
    /// </summary>
    [KernelFunction("SearchLeads")]
    [Description("Search for sales leads by name, email, company, or other fields.")]
    public async Task<string> SearchLeadsAsync(
        [Description("The search query string (e.g., name, email, company).")] string query,
        [Description("Maximum number of results to return.")] int maxResults = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var results = await _leadService.SearchAsync(query);
            var limited = results.Take(maxResults).ToList();
            return SuccessResult(new { count = limited.Count, leads = limited });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error searching leads with query '{Query}'", query);
            return ErrorResult("SearchLeads", ex.Message);
        }
    }

    /// <summary>
    /// Gets the score for a specific lead.
    /// </summary>
    [KernelFunction("GetLeadScore")]
    [Description("Get the lead score for a specific lead, indicating its conversion potential.")]
    public async Task<string> GetLeadScoreAsync(
        [Description("The unique identifier of the lead.")] int leadId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var lead = await _leadService.GetByIdAsync(leadId);
            if (lead == null)
            {
                return ErrorResult("GetLeadScore", $"Lead with ID {leadId} not found.");
            }

            // Extract score via reflection since GetByIdAsync returns object
            var score = lead.GetType().GetProperty("Score")?.GetValue(lead)
                     ?? lead.GetType().GetProperty("LeadScore")?.GetValue(lead)
                     ?? 0;

            return SuccessResult(new { leadId, score });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving score for lead {LeadId}", leadId);
            return ErrorResult("GetLeadScore", ex.Message);
        }
    }

    /// <summary>
    /// Gets aggregate lead statistics.
    /// </summary>
    [KernelFunction("GetLeadStats")]
    [Description("Get aggregate statistics about all leads in the CRM (counts by status, sources, conversion rates, etc.).")]
    public async Task<string> GetLeadStatsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var stats = await _leadService.GetStatsAsync();
            return SuccessResult(stats);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving lead statistics");
            return ErrorResult("GetLeadStats", ex.Message);
        }
    }

    #endregion

    #region Write Methods

    /// <summary>
    /// Updates the score for a lead.
    /// </summary>
    [RequiresApproval(Tier = "low", Description = "Updates the lead score, which affects prioritization and routing.")]
    [KernelFunction("UpdateLeadScore")]
    [Description("Update the score of a lead to reflect its conversion potential (0-100).")]
    public async Task<string> UpdateLeadScoreAsync(
        [Description("The unique identifier of the lead.")] int leadId,
        [Description("The new score value (0-100).")] int score,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (score < 0 || score > 100)
            {
                return ErrorResult("UpdateLeadScore", "Score must be between 0 and 100.");
            }

            var success = await _leadService.UpdateAsync(leadId, lead =>
            {
                // Set Score property using reflection for safety
                var scoreProp = lead.GetType().GetProperty("Score")
                             ?? lead.GetType().GetProperty("LeadScore");
                scoreProp?.SetValue(lead, score);
            });

            return success
                ? SuccessResult(new { updated = true, leadId, score })
                : ErrorResult("UpdateLeadScore", $"Lead with ID {leadId} not found or update failed.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating score for lead {LeadId}", leadId);
            return ErrorResult("UpdateLeadScore", ex.Message);
        }
    }

    /// <summary>
    /// Converts a lead into an opportunity.
    /// </summary>
    [RequiresApproval(Tier = "standard", Description = "Converts a lead to an opportunity, creating related account and contact records.")]
    [KernelFunction("ConvertLead")]
    [Description("Convert a qualified lead into an opportunity, which also creates associated account and contact records.")]
    public async Task<string> ConvertLeadAsync(
        [Description("The unique identifier of the lead to convert.")] int leadId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _leadService.ConvertAsync(leadId, null, null, null, null);
            return SuccessResult(new
            {
                converted = true,
                leadId,
                opportunityId = result.OpportunityId,
                resultLeadId = result.LeadId
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error converting lead {LeadId}", leadId);
            return ErrorResult("ConvertLead", ex.Message);
        }
    }

    #endregion
}
