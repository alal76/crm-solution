// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service to verify database configurations.
/// Runs on application startup as a BVT (Build Verification Test).
/// </summary>
public interface IDatabaseSyncService
{
    /// <summary>
    /// Runs database sync verification and fixes any inconsistencies.
    /// </summary>
    Task<DatabaseSyncResult> RunSyncCheckAsync();
}

public class DatabaseSyncResult
{
    public bool Success { get; set; }
    public List<string> Messages { get; set; } = new();
    public Dictionary<string, int> ProductionFieldCounts { get; set; } = new();
    public int FieldsSynced { get; set; }
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
}

public class DatabaseSyncService : IDatabaseSyncService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseSyncService> _logger;

    public DatabaseSyncService(
        IServiceProvider serviceProvider,
        ILogger<DatabaseSyncService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<DatabaseSyncResult> RunSyncCheckAsync()
    {
        var result = new DatabaseSyncResult();
        
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var productionContext = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
            var fieldConfigService = scope.ServiceProvider.GetService<ModuleFieldConfigurationService>();

            _logger.LogInformation("Starting database sync verification (BVT check)...");
            result.Messages.Add("Starting database sync verification...");

            // Ensure production database has all field configs from the service
            if (fieldConfigService != null)
            {
                var productionSynced = await EnsureProductionHasAllFieldConfigsAsync(productionContext, fieldConfigService);
                if (productionSynced > 0)
                {
                    result.Messages.Add($"Production DB: Added {productionSynced} missing field configurations");
                    result.FieldsSynced = productionSynced;
                    _logger.LogInformation("Added {Count} missing field configs to production", productionSynced);
                }
            }

            // Get production field counts
            var prodCounts = await productionContext.ModuleFieldConfigurations
                .GroupBy(f => f.ModuleName)
                .Select(g => new { ModuleName = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ModuleName, x => x.Count);

            result.ProductionFieldCounts = prodCounts;
            _logger.LogInformation("Production DB has {Count} modules configured", prodCounts.Count);

            foreach (var kvp in prodCounts)
            {
                result.Messages.Add($"Module {kvp.Key}: {kvp.Value} fields - OK");
            }

            result.Success = true;
            _logger.LogInformation("Database sync verification completed successfully");
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Messages.Add($"Error during sync check: {ex.Message}");
            _logger.LogError(ex, "Database sync verification failed");
        }

        return result;
    }

    /// <summary>
    /// Ensures the production database has all field configurations defined in the service.
    /// This adds any missing module field configurations to production.
    /// </summary>
    private async Task<int> EnsureProductionHasAllFieldConfigsAsync(CrmDbContext context, ModuleFieldConfigurationService fieldConfigService)
    {
        var initialCounts = await context.ModuleFieldConfigurations
            .GroupBy(f => f.ModuleName)
            .Select(g => new { ModuleName = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ModuleName, x => x.Count);

        // Call the service's InitializeAllModulesAsync to ensure all modules have their configs
        var results = await fieldConfigService.InitializeAllModulesAsync();

        // Count how many were added (modules that weren't in initial counts or had 0)
        var added = 0;
        foreach (var result in results)
        {
            if (!initialCounts.TryGetValue(result.Key, out var initialCount) || initialCount == 0)
            {
                added += result.Value;
                _logger.LogInformation("Added {Count} field configs for module {Module} to production", 
                    result.Value, result.Key);
            }
        }

        return added;
    }
}
