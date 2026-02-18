// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

#nullable enable

using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for AI agent analytics including usage metrics,
/// accuracy tracking, and cost analysis.
/// </summary>
[ApiController]
[Route("api/agents/analytics")]
[Authorize]
public class AgentAnalyticsController : ControllerBase
{
    #region Fields

    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<AgentAnalyticsController> _logger;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="AgentAnalyticsController"/> class.
    /// </summary>
    /// <param name="dbContext">The CRM database context.</param>
    /// <param name="logger">The logger instance.</param>
    public AgentAnalyticsController(ICrmDbContext dbContext, ILogger<AgentAnalyticsController> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    #region DTOs

    /// <summary>
    /// DTO for per-agent usage analytics.
    /// </summary>
    /// <param name="AgentId">The agent ID.</param>
    /// <param name="AgentName">The agent name.</param>
    /// <param name="TotalConversations">Total number of conversations.</param>
    /// <param name="TotalActions">Total number of actions executed.</param>
    /// <param name="AverageMessagesPerConversation">Average messages per conversation.</param>
    public record AgentUsageMetric(
        int AgentId,
        string AgentName,
        int TotalConversations,
        int TotalActions,
        double AverageMessagesPerConversation);

    /// <summary>
    /// DTO for per-agent accuracy metrics.
    /// </summary>
    /// <param name="AgentId">The agent ID.</param>
    /// <param name="AgentName">The agent name.</param>
    /// <param name="AverageRating">Average conversation rating (1-5).</param>
    /// <param name="RatedConversations">Number of conversations that were rated.</param>
    /// <param name="TotalConversations">Total conversations for the period.</param>
    public record AgentAccuracyMetric(
        int AgentId,
        string AgentName,
        double AverageRating,
        int RatedConversations,
        int TotalConversations);

    /// <summary>
    /// DTO for per-agent cost analytics.
    /// </summary>
    /// <param name="AgentId">The agent ID.</param>
    /// <param name="AgentName">The agent name.</param>
    /// <param name="TotalActions">Total actions executed.</param>
    /// <param name="DailyCosts">Cost breakdown by date.</param>
    public record AgentCostMetric(
        int AgentId,
        string AgentName,
        int TotalActions,
        IReadOnlyList<DailyCost> DailyCosts);

    /// <summary>
    /// DTO for daily cost breakdown.
    /// </summary>
    /// <param name="Date">The date.</param>
    /// <param name="ActionCount">Number of actions on this date.</param>
    public record DailyCost(DateTime Date, int ActionCount);

    #endregion

    #region Endpoints

    /// <summary>
    /// Gets usage analytics for AI agents within an optional date range.
    /// Includes total conversations, actions, and average messages per conversation grouped by agent.
    /// </summary>
    /// <param name="from">Optional start date filter.</param>
    /// <param name="to">Optional end date filter.</param>
    /// <returns>Usage analytics grouped by agent.</returns>
    [HttpGet("usage")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsageAnalytics([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        try
        {
            var fromDate = from ?? DateTime.UtcNow.AddDays(-30);
            var toDate = to ?? DateTime.UtcNow;

            var agents = await _dbContext.AIAgents
                .AsNoTracking()
                .Where(a => !a.IsDeleted)
                .ToListAsync(HttpContext.RequestAborted);

            var conversations = await _dbContext.AgentConversations
                .AsNoTracking()
                .Where(c => !c.IsDeleted && c.CreatedAt >= fromDate && c.CreatedAt <= toDate)
                .ToListAsync(HttpContext.RequestAborted);

            var actions = await _dbContext.AgentActions
                .AsNoTracking()
                .Where(a => !a.IsDeleted && a.CreatedAt >= fromDate && a.CreatedAt <= toDate)
                .ToListAsync(HttpContext.RequestAborted);

            var metrics = agents.Select(agent =>
            {
                var agentConversations = conversations.Where(c => c.AgentId == agent.Id).ToList();
                var agentActions = actions.Where(a => a.AgentId == agent.Id).ToList();

                // Estimate message count from JSON Messages field length as a proxy
                var avgMessages = agentConversations.Count > 0
                    ? agentConversations.Average(c =>
                    {
                        if (string.IsNullOrEmpty(c.Messages))
                        {
                            return 0.0;
                        }

                        // Count occurrences of "Role" as a rough message count proxy
                        var count = c.Messages.Split("\"Role\"", StringSplitOptions.None).Length - 1;
                        return Math.Max(count, 0);
                    })
                    : 0.0;

                return new AgentUsageMetric(
                    agent.Id,
                    agent.Name,
                    agentConversations.Count,
                    agentActions.Count,
                    Math.Round(avgMessages, 2));
            }).ToList();

            return Ok(new
            {
                FromDate = fromDate,
                ToDate = toDate,
                TotalConversations = conversations.Count,
                TotalActions = actions.Count,
                AgentMetrics = metrics,
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting usage analytics");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving usage analytics.");
        }
    }

    /// <summary>
    /// Gets accuracy metrics for AI agents based on conversation ratings.
    /// </summary>
    /// <param name="from">Optional start date filter.</param>
    /// <param name="to">Optional end date filter.</param>
    /// <returns>Accuracy metrics grouped by agent.</returns>
    [HttpGet("accuracy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccuracyMetrics([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        try
        {
            var fromDate = from ?? DateTime.UtcNow.AddDays(-30);
            var toDate = to ?? DateTime.UtcNow;

            var agents = await _dbContext.AIAgents
                .AsNoTracking()
                .Where(a => !a.IsDeleted)
                .ToListAsync(HttpContext.RequestAborted);

            var conversations = await _dbContext.AgentConversations
                .AsNoTracking()
                .Where(c => !c.IsDeleted && c.CreatedAt >= fromDate && c.CreatedAt <= toDate)
                .ToListAsync(HttpContext.RequestAborted);

            var metrics = agents.Select(agent =>
            {
                var agentConversations = conversations.Where(c => c.AgentId == agent.Id).ToList();
                var ratedConversations = agentConversations.Where(c => c.Rating.HasValue).ToList();
                var avgRating = ratedConversations.Count > 0
                    ? ratedConversations.Average(c => c.Rating!.Value)
                    : 0.0;

                return new AgentAccuracyMetric(
                    agent.Id,
                    agent.Name,
                    Math.Round(avgRating, 2),
                    ratedConversations.Count,
                    agentConversations.Count);
            }).ToList();

            var overallRated = conversations.Where(c => c.Rating.HasValue).ToList();
            var overallAvg = overallRated.Count > 0
                ? Math.Round(overallRated.Average(c => c.Rating!.Value), 2)
                : 0.0;

            return Ok(new
            {
                FromDate = fromDate,
                ToDate = toDate,
                OverallAverageRating = overallAvg,
                TotalRatedConversations = overallRated.Count,
                AgentMetrics = metrics,
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting accuracy metrics");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving accuracy metrics.");
        }
    }

    /// <summary>
    /// Gets cost analytics for AI agents based on action execution.
    /// Groups action counts by agent and date for cost tracking.
    /// </summary>
    /// <param name="from">Optional start date filter.</param>
    /// <param name="to">Optional end date filter.</param>
    /// <returns>Cost analytics grouped by agent and date.</returns>
    [HttpGet("cost")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCostAnalytics([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        try
        {
            var fromDate = from ?? DateTime.UtcNow.AddDays(-30);
            var toDate = to ?? DateTime.UtcNow;

            var agents = await _dbContext.AIAgents
                .AsNoTracking()
                .Where(a => !a.IsDeleted)
                .ToListAsync(HttpContext.RequestAborted);

            var actions = await _dbContext.AgentActions
                .AsNoTracking()
                .Where(a => !a.IsDeleted && a.CreatedAt >= fromDate && a.CreatedAt <= toDate)
                .ToListAsync(HttpContext.RequestAborted);

            var metrics = agents.Select(agent =>
            {
                var agentActions = actions.Where(a => a.AgentId == agent.Id).ToList();
                var dailyCosts = agentActions
                    .GroupBy(a => a.CreatedAt.Date)
                    .Select(g => new DailyCost(g.Key, g.Count()))
                    .OrderBy(d => d.Date)
                    .ToList();

                return new AgentCostMetric(
                    agent.Id,
                    agent.Name,
                    agentActions.Count,
                    dailyCosts);
            }).ToList();

            return Ok(new
            {
                FromDate = fromDate,
                ToDate = toDate,
                TotalActions = actions.Count,
                AgentMetrics = metrics,
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cost analytics");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving cost analytics.");
        }
    }

    #endregion
}
