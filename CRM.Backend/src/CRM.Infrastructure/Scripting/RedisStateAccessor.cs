// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using CRM.Core.Scripting;

namespace CRM.Infrastructure.Scripting;

/// <summary>
/// Redis-backed <see cref="IStateAccessor"/> for scripts.
/// All keys are scoped to the execution context via a prefix
/// (<c>script:state:{executionId}:</c>), providing isolation between executions.
/// Stored values have a 1-hour absolute TTL.
/// </summary>
public class RedisStateAccessor : IStateAccessor
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisStateAccessor> _logger;
    private readonly string _prefix;

    /// <summary>
    /// Initialises a new <see cref="RedisStateAccessor"/> scoped to <paramref name="executionId"/>.
    /// </summary>
    public RedisStateAccessor(IDistributedCache cache, ILogger<RedisStateAccessor> logger, string executionId)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _prefix = $"script:state:{executionId}:";
    }

    /// <inheritdoc/>
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await _cache.GetStringAsync(_prefix + key, cancellationToken).ConfigureAwait(false);
            if (data == null)
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(data);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "State get failed for key {Key}", key);
            return default;
        }
    }

    /// <inheritdoc/>
    public async Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(value);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
        };
        await _cache.SetStringAsync(_prefix + key, json, options, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
        => await _cache.RemoveAsync(_prefix + key, cancellationToken).ConfigureAwait(false);
}
