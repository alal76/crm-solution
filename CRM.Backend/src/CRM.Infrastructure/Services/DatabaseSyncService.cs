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
/// Service to verify and sync database configurations between production and demo databases.
/// Runs on application startup as a BVT (Build Verification Test).
/// </summary>
public interface IDatabaseSyncService
{
    /// <summary>
    /// Runs database sync verification and fixes any inconsistencies.
    /// </summary>
    Task<DatabaseSyncResult> RunSyncCheckAsync();
    
    /// <summary>
    /// Syncs module field configurations from production to demo or vice versa.
    /// </summary>
    Task<int> SyncModuleFieldConfigurationsAsync(string sourceDb, string targetDb);
}

public class DatabaseSyncResult
{
    public bool Success { get; set; }
    public List<string> Messages { get; set; } = new();
    public Dictionary<string, int> ProductionFieldCounts { get; set; } = new();
    public Dictionary<string, int> DemoFieldCounts { get; set; } = new();
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
            var demoFactory = scope.ServiceProvider.GetService<IDemoDbContextFactory>();
            var fieldConfigService = scope.ServiceProvider.GetService<ModuleFieldConfigurationService>();

            _logger.LogInformation("Starting database sync verification (BVT check)...");
            result.Messages.Add("Starting database sync verification...");

            // Step 1: Ensure production database has all field configs from the service
            if (fieldConfigService != null)
            {
                var productionSynced = await EnsureProductionHasAllFieldConfigsAsync(productionContext, fieldConfigService);
                if (productionSynced > 0)
                {
                    result.Messages.Add($"Production DB: Added {productionSynced} missing field configurations");
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

            // Check demo database if factory exists
            if (demoFactory != null)
            {
                try
                {
                    using var demoContext = demoFactory.CreateDemoContext();
                    var demoCounts = await demoContext.ModuleFieldConfigurations
                        .GroupBy(f => f.ModuleName)
                        .Select(g => new { ModuleName = g.Key, Count = g.Count() })
                        .ToDictionaryAsync(x => x.ModuleName, x => x.Count);

                    result.DemoFieldCounts = demoCounts;
                    _logger.LogInformation("Demo DB has {Count} modules configured", demoCounts.Count);

                    // Compare and sync if needed
                    var allModules = prodCounts.Keys.Union(demoCounts.Keys).ToList();
                    var needsSync = false;

                    foreach (var module in allModules)
                    {
                        var prodCount = prodCounts.GetValueOrDefault(module, 0);
                        var demoCount = demoCounts.GetValueOrDefault(module, 0);

                        if (prodCount != demoCount)
                        {
                            needsSync = true;
                            result.Messages.Add($"Module {module}: Production={prodCount}, Demo={demoCount} - MISMATCH");
                            _logger.LogWarning("Module {Module} has mismatched field counts: Production={Prod}, Demo={Demo}", 
                                module, prodCount, demoCount);
                        }
                        else
                        {
                            result.Messages.Add($"Module {module}: {prodCount} fields - OK");
                        }
                    }

                    if (needsSync)
                    {
                        result.Messages.Add("Syncing field configurations from production to demo...");
                        result.FieldsSynced = await SyncFieldConfigsAsync(productionContext, demoContext);
                        result.Messages.Add($"Synced {result.FieldsSynced} field configurations");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not check demo database - may not exist yet");
                    result.Messages.Add($"Demo database check skipped: {ex.Message}");
                }
            }
            else
            {
                result.Messages.Add("Demo database factory not available - skipping demo sync");
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

    private async Task<int> SyncFieldConfigsAsync(CrmDbContext productionContext, CrmDbContext demoContext)
    {
        var synced = 0;

        // Get all production field configs
        var prodConfigs = await productionContext.ModuleFieldConfigurations.ToListAsync();
        var demoModules = await demoContext.ModuleFieldConfigurations
            .Select(f => f.ModuleName)
            .Distinct()
            .ToListAsync();

        foreach (var moduleName in prodConfigs.Select(c => c.ModuleName).Distinct())
        {
            var prodModuleConfigs = prodConfigs.Where(c => c.ModuleName == moduleName).ToList();
            var existingDemo = await demoContext.ModuleFieldConfigurations
                .Where(f => f.ModuleName == moduleName)
                .ToListAsync();

            // If demo has different count, replace with production
            if (existingDemo.Count != prodModuleConfigs.Count)
            {
                // Remove existing
                demoContext.ModuleFieldConfigurations.RemoveRange(existingDemo);

                // Add from production (with new IDs)
                foreach (var config in prodModuleConfigs)
                {
                    var newConfig = new ModuleFieldConfiguration
                    {
                        ModuleName = config.ModuleName,
                        FieldName = config.FieldName,
                        FieldLabel = config.FieldLabel,
                        FieldType = config.FieldType,
                        TabIndex = config.TabIndex,
                        TabName = config.TabName,
                        DisplayOrder = config.DisplayOrder,
                        IsEnabled = config.IsEnabled,
                        IsRequired = config.IsRequired,
                        GridSize = config.GridSize,
                        Placeholder = config.Placeholder,
                        HelpText = config.HelpText,
                        Options = config.Options,
                        ParentField = config.ParentField,
                        ParentFieldValue = config.ParentFieldValue,
                        IsReorderable = config.IsReorderable,
                        IsRequiredConfigurable = config.IsRequiredConfigurable,
                        IsHideable = config.IsHideable,
                        CreatedAt = DateTime.UtcNow
                    };
                    demoContext.ModuleFieldConfigurations.Add(newConfig);
                    synced++;
                }

                await demoContext.SaveChangesAsync();
                _logger.LogInformation("Synced {Count} field configs for module {Module}", 
                    prodModuleConfigs.Count, moduleName);
            }
        }

        return synced;
    }

    public async Task<int> SyncModuleFieldConfigurationsAsync(string sourceDb, string targetDb)
    {
        // This method can be called via API to manually sync databases
        using var scope = _serviceProvider.CreateScope();
        var productionContext = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
        var demoFactory = scope.ServiceProvider.GetService<IDemoDbContextFactory>();

        if (demoFactory == null)
        {
            _logger.LogWarning("Demo database factory not available");
            return 0;
        }

        using var demoContext = demoFactory.CreateDemoContext();
        return await SyncFieldConfigsAsync(productionContext, demoContext);
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
