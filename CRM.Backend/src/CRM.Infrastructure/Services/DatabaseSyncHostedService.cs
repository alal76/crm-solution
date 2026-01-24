// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Background service that runs database sync verification on application startup.
/// This is a BVT (Build Verification Test) that ensures database consistency.
/// </summary>
public class DatabaseSyncHostedService : IHostedService
{
    private readonly IDatabaseSyncService _syncService;
    private readonly ILogger<DatabaseSyncHostedService> _logger;

    public DatabaseSyncHostedService(
        IDatabaseSyncService syncService,
        ILogger<DatabaseSyncHostedService> logger)
    {
        _syncService = syncService;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("=== CRM Database Sync BVT Starting ===");
        
        try
        {
            // Run the sync check
            var result = await _syncService.RunSyncCheckAsync();

            // Log results
            if (result.Success)
            {
                _logger.LogInformation("BVT PASSED: Database sync check completed successfully");
                
                foreach (var message in result.Messages)
                {
                    _logger.LogInformation("  {Message}", message);
                }

                if (result.FieldsSynced > 0)
                {
                    _logger.LogInformation("BVT AUTO-FIX: Synced {Count} field configurations", result.FieldsSynced);
                }
            }
            else
            {
                _logger.LogWarning("BVT WARNING: Database sync check had issues");
                foreach (var message in result.Messages)
                {
                    _logger.LogWarning("  {Message}", message);
                }
            }

            _logger.LogInformation("=== CRM Database Sync BVT Complete ===");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BVT FAILED: Database sync check threw an exception");
            // Don't throw - we don't want to prevent app startup
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
