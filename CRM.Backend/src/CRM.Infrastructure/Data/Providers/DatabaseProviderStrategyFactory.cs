// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under AGPL-3.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CRM.Infrastructure.Data.Providers;

/// <summary>
/// Factory for creating database provider strategies based on configuration.
/// Implements the Factory Pattern to select the appropriate strategy.
///
/// Supported Providers and Deployment Modes:
/// - SQL Server: Standalone, Always On AG (Clustered), Azure SQL Hyperscale
/// - MySQL: Standalone, Group Replication (Clustered), HeatWave (Hyperscale)
/// - MariaDB: Standalone, Galera Cluster, ColumnStore
/// - PostgreSQL: Standalone, Patroni (Clustered), Citus/Aurora (Hyperscale)
/// - Oracle: Standalone, RAC (Clustered), Autonomous/Exadata (Hyperscale)
///
/// Configuration:
/// - DatabaseProvider: sqlserver, mysql, mariadb, postgresql, oracle
/// - DatabaseDeploymentMode: standalone, clustered, hyperscale
/// - DatabaseClusterType: For specialized variants (galera, citus, aurora, rac, etc.)
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

        var deploymentModeStr = _configuration?["DatabaseDeploymentMode"]?.ToLowerInvariant() ?? "standalone";
        var deploymentMode = ParseDeploymentMode(deploymentModeStr);
        var clusterType = _configuration?["DatabaseClusterType"]?.ToLowerInvariant();

        return CreateStrategy(provider, deploymentMode, clusterType);
    }

    /// <summary>
    /// Creates a provider strategy with explicit deployment configuration.
    /// </summary>
    /// <param name="databaseProvider">The database provider name.</param>
    /// <param name="deploymentMode">The deployment mode (Standalone, Clustered, Hyperscale).</param>
    /// <param name="clusterType">Optional specific cluster type (galera, citus, aurora, rac, etc.).</param>
    /// <returns>The appropriate database provider strategy.</returns>
    public IDatabaseProviderStrategy CreateStrategy(
        string databaseProvider,
        DatabaseDeploymentMode deploymentMode,
        string? clusterType = null)
    {
        var provider = databaseProvider.ToLowerInvariant();

        return provider switch
        {
            "sqlserver" or "mssql" => CreateSqlServerStrategy(deploymentMode, clusterType),
            "mysql" => CreateMySqlStrategy(deploymentMode, clusterType),
            "mariadb" => CreateMariaDbStrategy(deploymentMode, clusterType),
            "postgresql" or "postgres" or "pgsql" => CreatePostgreSqlStrategy(deploymentMode, clusterType),
            "oracle" => CreateOracleStrategy(deploymentMode, clusterType),
            // Default to SQL Server for backward compatibility
            _ => new SqlServerProviderStrategy(deploymentMode)
        };
    }

    /// <summary>
    /// Creates a provider strategy by detecting the actual EF Core provider being used.
    /// </summary>
    /// <param name="context">The DbContext to inspect.</param>
    /// <returns>The appropriate database provider strategy.</returns>
    public IDatabaseProviderStrategy CreateFromContext(DbContext context)
    {
        var providerName = context.Database.ProviderName ?? "";
        var deploymentMode = GetDeploymentModeFromConfiguration();
        var clusterType = _configuration?["DatabaseClusterType"]?.ToLowerInvariant();

        if (providerName.Contains("SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            return CreateSqlServerStrategy(deploymentMode, clusterType);
        }

        if (providerName.Contains("Pomelo", StringComparison.OrdinalIgnoreCase) ||
            providerName.Contains("MySql", StringComparison.OrdinalIgnoreCase))
        {
            // Check if it's actually MariaDB (often uses Pomelo provider)
            var configuredProvider = _configuration?["DatabaseProvider"]?.ToLowerInvariant();
            if (configuredProvider == "mariadb")
            {
                return CreateMariaDbStrategy(deploymentMode, clusterType);
            }
            return CreateMySqlStrategy(deploymentMode, clusterType);
        }

        if (providerName.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) ||
            providerName.Contains("PostgreSQL", StringComparison.OrdinalIgnoreCase))
        {
            return CreatePostgreSqlStrategy(deploymentMode, clusterType);
        }

        if (providerName.Contains("Oracle", StringComparison.OrdinalIgnoreCase))
        {
            return CreateOracleStrategy(deploymentMode, clusterType);
        }

        // Default to SQL Server
        return new SqlServerProviderStrategy(deploymentMode);
    }

    /// <summary>
    /// Determines the provider strategy based on configuration and optional runtime detection.
    /// </summary>
    /// <param name="configuredProvider">The provider from configuration.</param>
    /// <param name="runtimeProviderName">The actual EF Core provider name at runtime.</param>
    /// <returns>The appropriate database provider strategy.</returns>
    public IDatabaseProviderStrategy CreateStrategy(string? configuredProvider, string? runtimeProviderName)
    {
        var deploymentMode = GetDeploymentModeFromConfiguration();
        var clusterType = _configuration?["DatabaseClusterType"]?.ToLowerInvariant();

        // First try runtime detection if available
        if (!string.IsNullOrEmpty(runtimeProviderName))
        {
            if (runtimeProviderName.Contains("SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                return CreateSqlServerStrategy(deploymentMode, clusterType);
            }

            if (runtimeProviderName.Contains("Pomelo", StringComparison.OrdinalIgnoreCase) ||
                runtimeProviderName.Contains("MySql", StringComparison.OrdinalIgnoreCase))
            {
                if (configuredProvider?.ToLowerInvariant() == "mariadb")
                {
                    return CreateMariaDbStrategy(deploymentMode, clusterType);
                }
                return CreateMySqlStrategy(deploymentMode, clusterType);
            }

            if (runtimeProviderName.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) ||
                runtimeProviderName.Contains("PostgreSQL", StringComparison.OrdinalIgnoreCase))
            {
                return CreatePostgreSqlStrategy(deploymentMode, clusterType);
            }

            if (runtimeProviderName.Contains("Oracle", StringComparison.OrdinalIgnoreCase))
            {
                return CreateOracleStrategy(deploymentMode, clusterType);
            }
        }

        // Fall back to configured provider
        return CreateStrategy(configuredProvider);
    }

    #region Provider-Specific Factory Methods

    private static IDatabaseProviderStrategy CreateSqlServerStrategy(
        DatabaseDeploymentMode deploymentMode,
        string? clusterType)
    {
        return clusterType switch
        {
            "hyperscale" or "azure-hyperscale" => new AzureSqlHyperscaleStrategy(),
            "alwayson" or "ag" => new SqlServerProviderStrategy(DatabaseDeploymentMode.Clustered),
            _ => new SqlServerProviderStrategy(deploymentMode)
        };
    }

    private static IDatabaseProviderStrategy CreateMySqlStrategy(
        DatabaseDeploymentMode deploymentMode,
        string? clusterType)
    {
        return clusterType switch
        {
            "groupreplication" or "innodb-cluster" => new MySqlProviderStrategy(DatabaseDeploymentMode.Clustered),
            "heatwave" => new MySqlProviderStrategy(DatabaseDeploymentMode.Hyperscale),
            _ => new MySqlProviderStrategy(deploymentMode)
        };
    }

    private static IDatabaseProviderStrategy CreateMariaDbStrategy(
        DatabaseDeploymentMode deploymentMode,
        string? clusterType)
    {
        return clusterType switch
        {
            "galera" or "galera-cluster" => new MariaDbGaleraStrategy(),
            "columnstore" => new MariaDbColumnStoreStrategy(),
            _ => new MariaDbProviderStrategy(deploymentMode)
        };
    }

    private static IDatabaseProviderStrategy CreatePostgreSqlStrategy(
        DatabaseDeploymentMode deploymentMode,
        string? clusterType)
    {
        return clusterType switch
        {
            "citus" => new PostgreSqlCitusStrategy(),
            "aurora" or "aurora-postgresql" => new AuroraPostgreSqlStrategy(),
            "patroni" => new PostgreSqlProviderStrategy(DatabaseDeploymentMode.Clustered),
            _ => new PostgreSqlProviderStrategy(deploymentMode)
        };
    }

    private static IDatabaseProviderStrategy CreateOracleStrategy(
        DatabaseDeploymentMode deploymentMode,
        string? clusterType)
    {
        return clusterType switch
        {
            "rac" or "real-application-clusters" => new OracleRacStrategy(),
            "autonomous" or "adb" => new OracleAutonomousStrategy(),
            "exadata" => new OracleExadataStrategy(),
            _ => new OracleProviderStrategy(deploymentMode)
        };
    }

    #endregion

    #region Helper Methods

    private DatabaseDeploymentMode GetDeploymentModeFromConfiguration()
    {
        var deploymentModeStr = _configuration?["DatabaseDeploymentMode"]?.ToLowerInvariant() ?? "standalone";
        return ParseDeploymentMode(deploymentModeStr);
    }

    private static DatabaseDeploymentMode ParseDeploymentMode(string? mode)
    {
        return mode?.ToLowerInvariant() switch
        {
            "standalone" or "single" => DatabaseDeploymentMode.Standalone,
            "clustered" or "cluster" or "ha" or "high-availability" => DatabaseDeploymentMode.Clustered,
            "hyperscale" or "distributed" or "cloud" => DatabaseDeploymentMode.Hyperscale,
            _ => DatabaseDeploymentMode.Standalone
        };
    }

    #endregion

    #region Static Convenience Methods

    /// <summary>
    /// Gets a list of all supported database providers.
    /// </summary>
    public static IReadOnlyList<string> SupportedProviders => new[]
    {
        "sqlserver",
        "mysql",
        "mariadb",
        "postgresql",
        "oracle"
    };

    /// <summary>
    /// Gets a list of all supported deployment modes.
    /// </summary>
    public static IReadOnlyList<string> SupportedDeploymentModes => new[]
    {
        "standalone",
        "clustered",
        "hyperscale"
    };

    /// <summary>
    /// Gets a dictionary of providers to their supported cluster types.
    /// </summary>
    public static IReadOnlyDictionary<string, string[]> SupportedClusterTypes => new Dictionary<string, string[]>
    {
        ["sqlserver"] = new[] { "alwayson", "hyperscale" },
        ["mysql"] = new[] { "groupreplication", "innodb-cluster", "heatwave" },
        ["mariadb"] = new[] { "galera", "columnstore" },
        ["postgresql"] = new[] { "patroni", "citus", "aurora" },
        ["oracle"] = new[] { "rac", "autonomous", "exadata" }
    };

    #endregion
}
