// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Scripting;

/// <summary>
/// Redis-backed store for compiled script artefacts.
/// Uses the SHA-256 content hash of the source as the cache key,
/// enabling content-addressed deduplication across re-deploys.
/// </summary>
public class ScriptArtefactStore
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<ScriptArtefactStore> _logger;
    private static readonly TimeSpan DefaultExpiry = TimeSpan.FromHours(24);

    public ScriptArtefactStore(IDistributedCache cache, ILogger<ScriptArtefactStore> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    private static string CacheKey(string contentHash) => $"script:compiled:{contentHash}";

    public async Task<byte[]?> GetAsync(string contentHash, CancellationToken ct = default)
    {
        try
        {
            return await _cache.GetAsync(CacheKey(contentHash), ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache read failed for script artefact {Hash}.", contentHash);
            return null;
        }
    }

    public async Task SetAsync(string contentHash, byte[] artefact, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiry ?? DefaultExpiry,
        };
        try
        {
            await _cache.SetAsync(CacheKey(contentHash), artefact, options, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache write failed for script artefact {Hash}.", contentHash);
        }
    }

    public async Task RemoveAsync(string contentHash, CancellationToken ct = default)
    {
        try
        {
            await _cache.RemoveAsync(CacheKey(contentHash), ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache remove failed for script artefact {Hash}.", contentHash);
        }
    }
}
