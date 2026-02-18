// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CRM.Infrastructure.Data.Providers;

/// <summary>
/// Deployment mode for database configurations.
/// Affects connection handling, replication, and performance optimizations.
/// </summary>
public enum DatabaseDeploymentMode
{
    /// <summary>
    /// Single database instance. Simple configuration, no replication.
    /// </summary>
    Standalone = 0,

    /// <summary>
    /// Clustered deployment with multiple nodes for high availability.
    /// Examples: SQL Server Always On, PostgreSQL Patroni, Oracle RAC, MariaDB Galera.
    /// </summary>
    Clustered = 1,

    /// <summary>
    /// Hyperscale deployment for massive workloads with read replicas and sharding.
    /// Examples: Azure SQL Hyperscale, PostgreSQL Citus, Oracle Autonomous, MariaDB ColumnStore.
    /// </summary>
    Hyperscale = 2
}

/// <summary>
/// Strategy interface for database provider-specific EF Core configurations.
/// Implements the Strategy Pattern to handle differences between SQL Server, MySQL/MariaDB,
/// PostgreSQL, Oracle, and other providers across different deployment modes.
/// </summary>
public interface IDatabaseProviderStrategy
{
    /// <summary>
    /// Gets the provider name (e.g., "sqlserver", "mysql", "mariadb", "postgresql", "oracle").
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Gets the deployment mode (Standalone, Clustered, or Hyperscale).
    /// </summary>
    DatabaseDeploymentMode DeploymentMode { get; }

    /// <summary>
    /// Gets the column type for large text fields.
    /// SQL Server: nvarchar(max), PostgreSQL: TEXT, Oracle: CLOB, MySQL/MariaDB: LONGTEXT/TEXT
    /// </summary>
    string LongTextColumnType { get; }

    /// <summary>
    /// Gets the column type for medium text fields.
    /// </summary>
    string TextColumnType { get; }

    /// <summary>
    /// Gets the column type for JSON data.
    /// SQL Server: nvarchar(max), PostgreSQL: jsonb, Oracle: CLOB, MySQL 8+: JSON
    /// </summary>
    string JsonColumnType { get; }

    /// <summary>
    /// Gets the column type for UUID/GUID fields.
    /// SQL Server: uniqueidentifier, PostgreSQL: uuid, Oracle: RAW(16), MySQL: CHAR(36)
    /// </summary>
    string GuidColumnType { get; }

    /// <summary>
    /// Gets the column type for timestamp/datetime with timezone.
    /// </summary>
    string TimestampColumnType { get; }

    /// <summary>
    /// Indicates whether the provider supports native JSON operations.
    /// </summary>
    bool SupportsNativeJson { get; }

    /// <summary>
    /// Indicates whether the provider supports native UUID/GUID type.
    /// </summary>
    bool SupportsNativeGuid { get; }

    /// <summary>
    /// Indicates whether the provider supports sequences for ID generation.
    /// </summary>
    bool SupportsSequences { get; }

    /// <summary>
    /// Gets the recommended batch size for bulk operations based on deployment mode.
    /// Hyperscale typically supports larger batch sizes.
    /// </summary>
    int RecommendedBatchSize { get; }

    /// <summary>
    /// Gets connection pool settings optimized for the deployment mode.
    /// </summary>
    ConnectionPoolSettings ConnectionPoolSettings { get; }

    /// <summary>
    /// Configures the RowVersion property for optimistic concurrency.
    /// SQL Server uses native rowversion, PostgreSQL uses xmin, Oracle uses ORA_ROWSCN.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="entityType">The entity type to configure.</param>
    void ConfigureRowVersion(ModelBuilder modelBuilder, IMutableEntityType entityType);

    /// <summary>
    /// Applies post-configuration adjustments specific to the database provider.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    void ApplyPostConfiguration(ModelBuilder modelBuilder);

    /// <summary>
    /// Gets the recommended delete behavior for foreign keys.
    /// </summary>
    DeleteBehavior DefaultDeleteBehavior { get; }

    /// <summary>
    /// Configures provider-specific index options (e.g., include columns, fill factor).
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    void ConfigureIndexes(ModelBuilder modelBuilder);

    /// <summary>
    /// Gets any provider-specific connection string modifications for the deployment mode.
    /// </summary>
    /// <param name="baseConnectionString">The base connection string.</param>
    /// <returns>Modified connection string with deployment-specific settings.</returns>
    string OptimizeConnectionString(string baseConnectionString);

    /// <summary>
    /// Gets the SQL function to retrieve current UTC timestamp.
    /// Used for HasDefaultValueSql in entity model configurations.
    /// SQL Server: GETUTCDATE(), PostgreSQL: CURRENT_TIMESTAMP AT TIME ZONE 'UTC', etc.
    /// </summary>
    /// <returns>Provider-specific UTC timestamp SQL function.</returns>
    string GetUtcNowSql();
}

/// <summary>
/// Connection pool settings optimized for different deployment modes.
/// </summary>
public class ConnectionPoolSettings
{
    /// <summary>Minimum number of connections in the pool.</summary>
    public int MinPoolSize { get; init; } = 5;

    /// <summary>Maximum number of connections in the pool.</summary>
    public int MaxPoolSize { get; init; } = 100;

    /// <summary>Connection timeout in seconds.</summary>
    public int ConnectionTimeout { get; init; } = 30;

    /// <summary>Command timeout in seconds.</summary>
    public int CommandTimeout { get; init; } = 30;

    /// <summary>Connection lifetime in seconds (0 = infinite).</summary>
    public int ConnectionLifetime { get; init; } = 0;

    /// <summary>Enable connection resiliency for transient failures.</summary>
    public bool EnableRetryOnFailure { get; init; } = true;

    /// <summary>Maximum retry count for transient failures.</summary>
    public int MaxRetryCount { get; init; } = 3;

    /// <summary>Maximum retry delay in seconds.</summary>
    public int MaxRetryDelaySeconds { get; init; } = 30;

    /// <summary>Preset for standalone deployment.</summary>
    public static ConnectionPoolSettings Standalone => new()
    {
        MinPoolSize = 5,
        MaxPoolSize = 50,
        ConnectionTimeout = 30,
        CommandTimeout = 30,
        EnableRetryOnFailure = true,
        MaxRetryCount = 3
    };

    /// <summary>Preset for clustered deployment with higher pool for failover.</summary>
    public static ConnectionPoolSettings Clustered => new()
    {
        MinPoolSize = 10,
        MaxPoolSize = 100,
        ConnectionTimeout = 15,
        CommandTimeout = 30,
        ConnectionLifetime = 300, // Rotate connections for load balancing
        EnableRetryOnFailure = true,
        MaxRetryCount = 5
    };

    /// <summary>Preset for hyperscale deployment with aggressive pooling.</summary>
    public static ConnectionPoolSettings Hyperscale => new()
    {
        MinPoolSize = 20,
        MaxPoolSize = 200,
        ConnectionTimeout = 10,
        CommandTimeout = 60, // Longer for distributed queries
        ConnectionLifetime = 600,
        EnableRetryOnFailure = true,
        MaxRetryCount = 6,
        MaxRetryDelaySeconds = 60
    };
}
