// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Scripting.Roslyn;

/// <summary>
/// Monitors process-level GC allocations during script execution and signals when
/// memory exceeds the per-script limit so the caller can cancel the execution CTS.
/// </summary>
public class MemoryWatchdog
{
    private readonly ILogger<MemoryWatchdog> _logger;

    public MemoryWatchdog(ILogger<MemoryWatchdog> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Polls GC heap size every 100 ms until either the execution token fires or
    /// the memory limit is exceeded.
    /// </summary>
    /// <param name="memoryLimitBytes">Threshold in bytes above which <c>false</c> is returned.</param>
    /// <param name="executionCts">Fires when script execution completes normally.</param>
    /// <param name="ct">External cancellation.</param>
    /// <returns><c>true</c> if normal termination; <c>false</c> if limit exceeded.</returns>
    public async Task<bool> MonitorAsync(
        long memoryLimitBytes,
        CancellationToken executionCts,
        CancellationToken ct = default)
    {
        using var combined = CancellationTokenSource.CreateLinkedTokenSource(executionCts, ct);
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));

        while (await timer.WaitForNextTickAsync(combined.Token).ConfigureAwait(false))
        {
            var currentBytes = GC.GetTotalMemory(forceFullCollection: false);
            if (currentBytes > memoryLimitBytes)
            {
                _logger.LogWarning(
                    "Script memory limit exceeded: {CurrentMB} MB > {LimitMB} MB.",
                    currentBytes / 1_048_576, memoryLimitBytes / 1_048_576);
                return false;
            }
        }

        return true;
    }
}
