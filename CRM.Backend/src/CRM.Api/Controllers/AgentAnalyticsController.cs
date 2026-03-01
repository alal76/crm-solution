// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
#nullable enable

using CRM.Core.Entities.AI;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for AI agent analytics including usage metrics,
/// accuracy tracking, and cost analysis.
/// </summary>
[ApiController]
[Route("api/agents/analytics")]
[Authorize]
public class AgentAnalyticsController : CrmControllerBase
{
    #region Fields

    private readonly ICrmDbContext _dbContext;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="AgentAnalyticsController"/> class.
    /// </summary>
    /// <param name="dbContext">The CRM database context.</param>
    /// <param name="logger">The logger instance.</param>
    public AgentAnalyticsController(ICrmDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
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

    /// <summary>DTO for model-level cost/token breakdown used in the AI analytics summary.</summary>
    public record ModelBreakdown(string Model, decimal Cost, long Tokens, int Executions);

    /// <summary>DTO for agent-type-level breakdown used in the AI analytics summary.</summary>
    public record NodeTypeBreakdown(string NodeType, decimal Cost, long Tokens, int Executions);

    /// <summary>DTO for a single recent AI conversation execution row.</summary>
    public record RecentExecution(
        int NodeId,
        string NodeType,
        string Model,
        int InputTokens,
        int OutputTokens,
        int TotalTokens,
        decimal Cost,
        long LatencyMs,
        bool Success,
        string? ErrorMessage,
        DateTime Timestamp);

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
            var ratedConversations = agentConversations.Where(c => c.UserRating.HasValue).ToList();
            var avgRating = ratedConversations.Count > 0
                ? ratedConversations.Average(c => c.UserRating!.Value)
                : 0.0;

            return new AgentAccuracyMetric(
                agent.Id,
                agent.Name,
                Math.Round(avgRating, 2),
                ratedConversations.Count,
                agentConversations.Count);
        }).ToList();

        var overallRated = conversations.Where(c => c.UserRating.HasValue).ToList();
        var overallAvg = overallRated.Count > 0
            ? Math.Round(overallRated.Average(c => c.UserRating!.Value), 2)
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

    /// <summary>
    /// Gets a unified AI analytics summary combining cost, token usage, success rate,
    /// latency, and recent conversation executions. Powers the AI Analytics Dashboard UI.
    /// </summary>
    /// <param name="days">Number of days to look back (default: 30).</param>
    /// <returns>Aggregated AI analytics summary.</returns>
    [HttpGet("summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAnalyticsSummary([FromQuery] int days = 30) // NOSONAR
    {
                var lookback = Math.Max(1, Math.Abs(days));
        var fromDate = DateTime.UtcNow.AddDays(-lookback);
        var toDate = DateTime.UtcNow;
        var period = lookback switch
        {
            <= 1 => "today",
            <= 7 => "week",
            <= 31 => "month",
            <= 93 => "quarter",
            _ => "year"
        };

        // ── Load reference data ──────────────────────────────────────────────
        var agents = await _dbContext.AIAgents
            .AsNoTracking()
            .Where(a => !a.IsDeleted)
            .ToListAsync(HttpContext.RequestAborted);

        var agentLookup = agents.ToDictionary(a => a.Id);

        // ── Conversations in the period ──────────────────────────────────────
        var conversations = await _dbContext.AgentConversations
            .AsNoTracking()
            .Where(c => !c.IsDeleted && c.CreatedAt >= fromDate && c.CreatedAt <= toDate)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(HttpContext.RequestAborted);

        // ── Actions for latency and recent-execution detail ──────────────────
        var recentConvIds = conversations.Take(50).Select(c => c.Id).ToHashSet();

        var recentActions = await _dbContext.AgentActions
            .AsNoTracking()
            .Where(a => !a.IsDeleted && recentConvIds.Contains(a.ConversationId))
            .ToListAsync(HttpContext.RequestAborted);

        // Actions across the full period (for overall avg latency)
        var allPeriodActions = await _dbContext.AgentActions
            .AsNoTracking()
            .Where(a => !a.IsDeleted && a.CreatedAt >= fromDate && a.CreatedAt <= toDate && a.ExecutionTimeMs > 0)
            .Select(a => (long)a.ExecutionTimeMs)
            .ToListAsync(HttpContext.RequestAborted);

        // ── Summary totals ───────────────────────────────────────────────────
        var totalCost = conversations.Sum(c => c.EstimatedCost);
        var totalTokens = conversations.Sum(c => (long)c.TotalTokensUsed);
        var totalExecutions = conversations.Count;
        var completedCount = conversations.Count(c =>
            c.Status == ConversationStatus.Completed || c.Status == ConversationStatus.Active);
        var successRate = totalExecutions > 0
            ? Math.Round((double)completedCount / totalExecutions * 100, 1)
            : 0.0;
        var averageLatencyMs = allPeriodActions.Count > 0
            ? Math.Round(allPeriodActions.Average(ms => (double)ms), 1)
            : 0.0;

        // ── Group by model ───────────────────────────────────────────────────
        var byModel = conversations
            .GroupBy(c =>
            {
                var agent = agentLookup.TryGetValue(c.AgentId, out var ag) ? ag : null;
                return agent?.ModelOverride ?? "System Default";
            })
            .Select(g => new ModelBreakdown(
                g.Key,
                g.Sum(c => c.EstimatedCost),
                g.Sum(c => (long)c.TotalTokensUsed),
                g.Count()))
            .OrderByDescending(m => m.Cost)
            .ToList();

        // ── Group by agent type (node type) ──────────────────────────────────
        var byNodeType = conversations
            .GroupBy(c =>
            {
                var agent = agentLookup.TryGetValue(c.AgentId, out var ag) ? ag : null;
                return agent != null ? agent.AgentType.ToString() : "Unknown";
            })
            .Select(g => new NodeTypeBreakdown(
                g.Key,
                g.Sum(c => c.EstimatedCost),
                g.Sum(c => (long)c.TotalTokensUsed),
                g.Count()))
            .OrderByDescending(n => n.Cost)
            .ToList();

        // ── Recent executions (last 20 conversations) ─────────────────────────
        var actionsByConversation = recentActions
            .GroupBy(a => a.ConversationId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var recentExecutions = conversations
            .Take(20)
            .Select(c =>
            {
                var agent = agentLookup.TryGetValue(c.AgentId, out var ag) ? ag : null;
                var convActions = actionsByConversation.TryGetValue(c.Id, out var ca) ? ca : new List<AgentAction>();
                var latencyMs = convActions.Count > 0
                    ? (long)convActions.Average(a => (double)a.ExecutionTimeMs)
                    : 0L;
                var inputTokens = (int)(c.TotalTokensUsed * 0.75);
                var outputTokens = c.TotalTokensUsed - inputTokens;
                var success = c.Status is ConversationStatus.Completed or ConversationStatus.Active;
                var errorMsg = c.Status == ConversationStatus.Failed ? "Conversation failed" : (string?)null;

                return new RecentExecution(
                    c.Id,
                    agent?.AgentType.ToString() ?? "Unknown",
                    agent?.ModelOverride ?? "System Default",
                    inputTokens,
                    outputTokens,
                    c.TotalTokensUsed,
                    c.EstimatedCost,
                    latencyMs,
                    success,
                    errorMsg,
                    c.CreatedAt);
            })
            .ToList();

        return Ok(new
        {
            Period = period,
            TotalCost = totalCost,
            TotalTokens = totalTokens,
            TotalExecutions = totalExecutions,
            SuccessRate = successRate,
            AverageLatencyMs = averageLatencyMs,
            ByModel = byModel,
            ByNodeType = byNodeType,
            RecentExecutions = recentExecutions,
        });
    }

    #endregion
}
