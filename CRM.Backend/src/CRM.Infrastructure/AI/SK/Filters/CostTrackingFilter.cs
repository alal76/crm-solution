// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using CRM.Infrastructure.AI.SK.Configuration;

namespace CRM.Infrastructure.AI.SK.Filters;

/// <summary>
/// Semantic Kernel function invocation filter that tracks token usage and estimated
/// costs across agent conversations. Enforces a configurable daily budget limit.
/// </summary>
/// <remarks>
/// Cost data is stored in-memory per day. In production, this should be backed by
/// a persistent store (database or Redis) for multi-instance deployments.
/// </remarks>
public class CostTrackingFilter : IFunctionInvocationFilter
{
    #region Fields

    private readonly ILogger<CostTrackingFilter> _logger;
    private readonly AgentOptions _agentOptions;

    /// <summary>
    /// Thread-safe daily cost accumulator keyed by date string (yyyy-MM-dd).
    /// </summary>
    private static readonly ConcurrentDictionary<string, decimal> _dailyCosts = new();

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="CostTrackingFilter"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="options">Semantic Kernel configuration containing budget settings.</param>
    public CostTrackingFilter(
        ILogger<CostTrackingFilter> logger,
        IOptions<SemanticKernelOptions> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _agentOptions = options?.Value?.Agents ?? throw new ArgumentNullException(nameof(options));
    }

    #endregion

    #region IFunctionInvocationFilter Implementation

    /// <inheritdoc />
    public async Task OnFunctionInvocationAsync(
        FunctionInvocationContext context,
        Func<FunctionInvocationContext, Task> next)
    {
        // Check budget BEFORE execution
        var todayKey = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var currentDailyCost = _dailyCosts.GetOrAdd(todayKey, 0m);

        if (_agentOptions.CostBudgetPerDay > 0 && currentDailyCost >= _agentOptions.CostBudgetPerDay)
        {
            _logger.LogWarning(
                "Daily cost budget EXCEEDED: {CurrentCost:C} >= {Budget:C} — blocking execution of {Plugin}.{Function}",
                currentDailyCost,
                _agentOptions.CostBudgetPerDay,
                context.Function.PluginName,
                context.Function.Name);

            context.Result = new FunctionResult(
                context.Function,
                "BUDGET_EXCEEDED: Daily AI cost limit reached. Please try again tomorrow or contact an administrator.");

            return;
        }

        await next(context);

        // Extract token usage from result metadata if available
        if (context.Result?.Metadata != null
            && context.Result.Metadata.TryGetValue("TokensUsed", out var tokensObj)
            && tokensObj is int tokensUsed
            && tokensUsed > 0)
        {
            // Rough cost estimate: $0.00001 per token (adjustable per model)
            var estimatedCost = tokensUsed * 0.00001m;
            RecordCost(estimatedCost);

            _logger.LogDebug(
                "Token usage recorded: {Tokens} tokens ≈ {Cost:C6} | Daily total: {DailyTotal:C4}",
                tokensUsed,
                estimatedCost,
                GetDailyCost());
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Records a specific cost amount against today's budget.
    /// </summary>
    /// <param name="cost">The cost to record.</param>
    public void RecordCost(decimal cost)
    {
        if (cost <= 0)
        {
            return;
        }

        var todayKey = DateTime.UtcNow.ToString("yyyy-MM-dd");
        _dailyCosts.AddOrUpdate(todayKey, cost, (_, existing) => existing + cost);
    }

    /// <summary>
    /// Gets the accumulated cost for today (UTC).
    /// </summary>
    /// <returns>Today's total estimated cost.</returns>
    public decimal GetDailyCost()
    {
        var todayKey = DateTime.UtcNow.ToString("yyyy-MM-dd");
        return _dailyCosts.GetOrAdd(todayKey, 0m);
    }

    /// <summary>
    /// Gets cost data for all tracked days. Useful for reporting.
    /// </summary>
    /// <returns>A dictionary of date → cost.</returns>
    public IReadOnlyDictionary<string, decimal> GetAllCosts()
    {
        return _dailyCosts;
    }

    /// <summary>
    /// Resets cost tracking data. Intended for testing only.
    /// </summary>
    internal static void ResetForTesting()
    {
        _dailyCosts.Clear();
    }

    #endregion
}
