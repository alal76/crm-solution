// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under AGPL-3.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CRM.Infrastructure.Data.Providers;

/// <summary>
/// Factory for creating database provider strategies based on configuration.
/// Implements the Factory Pattern to select the appropriate strategy.
/// </summary>
public class DatabaseProviderStrategyFactory
{
    private readonly IConfiguration? _configuration;
    
    public DatabaseProviderStrategyFactory(IConfiguration? configuration = null)
    {
        _configuration = configuration;
    }
    
    /// <summary>
    /// Creates a provider strategy based on the configured database provider.
    /// </summary>
    /// <param name="databaseProvider">Optional explicit provider name. If null, reads from configuration.</param>
    /// <returns>The appropriate database provider strategy.</returns>
    public IDatabaseProviderStrategy CreateStrategy(string? databaseProvider = null)
    {
        var provider = databaseProvider?.ToLowerInvariant() 
            ?? _configuration?["DatabaseProvider"]?.ToLowerInvariant()
            ?? "sqlserver";
        
        return provider switch
        {
            "sqlserver" => new SqlServerProviderStrategy(),
            "mysql" => new MySqlProviderStrategy(),
            "mariadb" => new MariaDbProviderStrategy(),
            // Default to SQL Server for backward compatibility
            _ => new SqlServerProviderStrategy()
        };
    }
    
    /// <summary>
    /// Creates a provider strategy by detecting the actual EF Core provider being used.
    /// </summary>
    /// <param name="context">The DbContext to inspect.</param>
    /// <returns>The appropriate database provider strategy.</returns>
    public IDatabaseProviderStrategy CreateFromContext(DbContext context)
    {
        var providerName = context.Database.ProviderName;
        
        if (providerName?.Contains("SqlServer", StringComparison.OrdinalIgnoreCase) == true)
        {
            return new SqlServerProviderStrategy();
        }
        
        if (providerName?.Contains("MySql", StringComparison.OrdinalIgnoreCase) == true ||
            providerName?.Contains("Pomelo", StringComparison.OrdinalIgnoreCase) == true)
        {
            return new MySqlProviderStrategy();
        }
        
        // Default to SQL Server
        return new SqlServerProviderStrategy();
    }
    
    /// <summary>
    /// Determines the provider strategy based on configuration and optional runtime detection.
    /// </summary>
    /// <param name="configuredProvider">The provider from configuration.</param>
    /// <param name="runtimeProviderName">The actual EF Core provider name at runtime.</param>
    /// <returns>The appropriate database provider strategy.</returns>
    public IDatabaseProviderStrategy CreateStrategy(string? configuredProvider, string? runtimeProviderName)
    {
        // First try runtime detection if available
        if (!string.IsNullOrEmpty(runtimeProviderName))
        {
            if (runtimeProviderName.Contains("SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                return new SqlServerProviderStrategy();
            }
            
            if (runtimeProviderName.Contains("MySql", StringComparison.OrdinalIgnoreCase) ||
                runtimeProviderName.Contains("Pomelo", StringComparison.OrdinalIgnoreCase))
            {
                return new MySqlProviderStrategy();
            }
        }
        
        // Fall back to configured provider
        return CreateStrategy(configuredProvider);
    }
}
