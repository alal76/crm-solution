// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Scripting.AgentHooks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Scripting.AgentHooks;

/// <summary>
/// Enforces per-agent budget limits: max tokens per call, max calls per hour, max cost per day.
/// SARCH-071: AIAgent budget fields enforcement.
/// </summary>
public class AgentBudgetEnforcer
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<AgentBudgetEnforcer> _logger;

    public AgentBudgetEnforcer(IDistributedCache cache, ILogger<AgentBudgetEnforcer> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<HookResult> CheckBudgetAsync(
        string agentId,
        int? maxTokensPerCall,
        int? maxCallsPerHour,
        decimal? maxCostPerDay,
        int requestedTokens,
        decimal estimatedCost,
        CancellationToken ct = default)
    {
        if (maxTokensPerCall.HasValue && requestedTokens > maxTokensPerCall.Value)
        {
            return new HookResult
            {
                Continue = false,
                Reason = $"Token limit exceeded: {requestedTokens} > {maxTokensPerCall}",
            };
        }

        if (maxCallsPerHour.HasValue)
        {
            var callKey = $"agent:budget:calls:{agentId}:{DateTime.UtcNow:yyyyMMddHH}";
            var callsStr = await _cache.GetStringAsync(callKey, ct);
            var calls = callsStr != null ? int.Parse(callsStr, CultureInfo.InvariantCulture) : 0;

            if (calls >= maxCallsPerHour.Value)
            {
                return new HookResult
                {
                    Continue = false,
                    Reason = $"Hourly call limit exceeded: {calls}/{maxCallsPerHour}",
                };
            }

            await _cache.SetStringAsync(
                callKey,
                (calls + 1).ToString(CultureInfo.InvariantCulture),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) },
                ct);
        }

        if (maxCostPerDay.HasValue)
        {
            var costKey = $"agent:budget:cost:{agentId}:{DateTime.UtcNow:yyyyMMdd}";
            var costStr = await _cache.GetStringAsync(costKey, ct);
            var dailyCost = costStr != null ? decimal.Parse(costStr, CultureInfo.InvariantCulture) : 0m;

            if (dailyCost + estimatedCost > maxCostPerDay.Value)
            {
                return new HookResult
                {
                    Continue = false,
                    Reason = $"Daily cost limit exceeded: ${dailyCost + estimatedCost:F4} > ${maxCostPerDay:F4}",
                };
            }

            await _cache.SetStringAsync(
                costKey,
                (dailyCost + estimatedCost).ToString("F6", CultureInfo.InvariantCulture),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1) },
                ct);
        }

        return new HookResult { Continue = true };
    }
}
