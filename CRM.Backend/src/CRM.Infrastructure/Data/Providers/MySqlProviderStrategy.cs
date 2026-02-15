// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CRM.Infrastructure.Data.Providers;

/// <summary>
/// MySQL-specific database provider strategy.
/// Supports: Standalone, MySQL Group Replication/InnoDB Cluster (Clustered), MySQL HeatWave (Hyperscale).
///
/// Handles:
/// - BINARY(8) with concurrency token for optimistic concurrency (no native rowversion)
/// - TEXT/LONGTEXT for large text fields
/// - Post-configuration to convert LONGTEXT to TEXT to avoid 65535 byte row size limit
/// </summary>
public class MySqlProviderStrategy : DatabaseProviderStrategyBase
{
    public MySqlProviderStrategy(DatabaseDeploymentMode deploymentMode = DatabaseDeploymentMode.Standalone)
        : base(deploymentMode)
    {
    }

    public override string ProviderName => "mysql";

    public override string LongTextColumnType => "LONGTEXT";

    public override string TextColumnType => "TEXT";

    public override string JsonColumnType => "JSON"; // MySQL 5.7+ has native JSON

    public override string GuidColumnType => "CHAR(36)"; // UUID stored as string

    public override string TimestampColumnType => "DATETIME(6)"; // Microsecond precision

    public override bool SupportsNativeJson => true; // MySQL 5.7+

    public override bool SupportsNativeGuid => false; // Stored as CHAR(36)

    public override bool SupportsSequences => false; // Uses AUTO_INCREMENT

    public override DeleteBehavior DefaultDeleteBehavior => DeleteBehavior.Cascade;

    public override int RecommendedBatchSize => DeploymentMode switch
    {
        DatabaseDeploymentMode.Standalone => 100,
        DatabaseDeploymentMode.Clustered => 200,   // Group Replication has write overhead
        DatabaseDeploymentMode.Hyperscale => 1000, // HeatWave optimized for analytics
        _ => 100
    };

    public override void ConfigureRowVersion(ModelBuilder modelBuilder, IMutableEntityType entityType)
    {
        var rowVersionProperty = entityType.FindProperty("RowVersion");
        if (rowVersionProperty != null)
        {
            // MySQL: Use BINARY(8) for compatibility with SQL Server's rowversion size
            modelBuilder.Entity(entityType.ClrType)
                .Property("RowVersion")
                .HasColumnType("BINARY(8)")
                .IsConcurrencyToken()
                .ValueGeneratedOnAddOrUpdate();
        }
    }

    public override void ApplyPostConfiguration(ModelBuilder modelBuilder)
    {
        // MySQL: Set string column types to prevent row size issues
        // Pomelo defaults to LONGTEXT which counts against the 65535 byte row limit
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(string))
                {
                    var columnType = property.GetColumnType();
                    if (columnType == null || columnType.Equals("longtext", StringComparison.OrdinalIgnoreCase))
                    {
                        var maxLength = property.GetMaxLength();
                        if (maxLength == null || maxLength > 4000)
                        {
                            property.SetColumnType("TEXT");
                        }
                        else
                        {
                            property.SetColumnType($"VARCHAR({maxLength})");
                        }
                    }
                }
            }
        }
    }

    public override string OptimizeConnectionString(string baseConnectionString)
    {
        var optimizations = DeploymentMode switch
        {
            DatabaseDeploymentMode.Standalone =>
                ";CharSet=utf8mb4;SslMode=Preferred",
            DatabaseDeploymentMode.Clustered =>
                ";CharSet=utf8mb4;SslMode=Required;ConnectionTimeout=15;DefaultCommandTimeout=30",
            DatabaseDeploymentMode.Hyperscale =>
                ";CharSet=utf8mb4;SslMode=Required;ConnectionTimeout=30;DefaultCommandTimeout=120;MaximumPoolSize=200",
            _ => ""
        };

        return baseConnectionString.TrimEnd(';') + optimizations;
    }

    public override string GetUtcNowSql() => "UTC_TIMESTAMP()";
}

/// <summary>
/// MariaDB-specific database provider strategy.
/// Supports: Standalone, Galera Cluster (Clustered), MariaDB ColumnStore (Hyperscale).
///
/// Differences from MySQL:
/// - Better JSON handling with JSON_TABLE (10.6+)
/// - System-versioned tables for temporal data
/// - Galera Cluster for synchronous multi-master replication
/// - ColumnStore for columnar analytics (hyperscale)
/// </summary>
public class MariaDbProviderStrategy : DatabaseProviderStrategyBase
{
    public MariaDbProviderStrategy(DatabaseDeploymentMode deploymentMode = DatabaseDeploymentMode.Standalone)
        : base(deploymentMode)
    {
    }

    public override string ProviderName => "mariadb";

    public override string LongTextColumnType => "LONGTEXT";

    public override string TextColumnType => "TEXT";

    // MariaDB 10.2+ supports JSON as an alias for LONGTEXT with validation
    public override string JsonColumnType => "JSON";

    public override string GuidColumnType => "CHAR(36)";

    public override string TimestampColumnType => "DATETIME(6)";

    public override bool SupportsNativeJson => true; // MariaDB 10.2+ (stored as LONGTEXT with validation)

    public override bool SupportsNativeGuid => false;

    public override bool SupportsSequences => true; // MariaDB 10.3+ supports sequences

    public override DeleteBehavior DefaultDeleteBehavior => DeleteBehavior.Cascade;

    public override int RecommendedBatchSize => DeploymentMode switch
    {
        DatabaseDeploymentMode.Standalone => 100,
        DatabaseDeploymentMode.Clustered => 50,    // Galera has certification overhead, smaller batches better
        DatabaseDeploymentMode.Hyperscale => 5000, // ColumnStore is optimized for bulk operations
        _ => 100
    };

    public override ConnectionPoolSettings ConnectionPoolSettings => DeploymentMode switch
    {
        DatabaseDeploymentMode.Standalone => ConnectionPoolSettings.Standalone,
        DatabaseDeploymentMode.Clustered => new ConnectionPoolSettings
        {
            MinPoolSize = 10,
            MaxPoolSize = 100,
            ConnectionTimeout = 15,
            CommandTimeout = 30,
            ConnectionLifetime = 60, // Rotate frequently for Galera load balancing
            EnableRetryOnFailure = true,
            MaxRetryCount = 5
        },
        DatabaseDeploymentMode.Hyperscale => new ConnectionPoolSettings
        {
            MinPoolSize = 20,
            MaxPoolSize = 200,
            ConnectionTimeout = 30,
            CommandTimeout = 300, // ColumnStore queries can be long-running
            EnableRetryOnFailure = true,
            MaxRetryCount = 3
        },
        _ => ConnectionPoolSettings.Standalone
    };

    public override void ConfigureRowVersion(ModelBuilder modelBuilder, IMutableEntityType entityType)
    {
        var rowVersionProperty = entityType.FindProperty("RowVersion");
        if (rowVersionProperty != null)
        {
            // MariaDB: Use BINARY(8) with concurrency token
            // Note: MariaDB 10.3+ supports system-versioned tables which could be an alternative
            modelBuilder.Entity(entityType.ClrType)
                .Property("RowVersion")
                .HasColumnType("BINARY(8)")
                .IsConcurrencyToken()
                .ValueGeneratedOnAddOrUpdate();
        }
    }

    public override void ApplyPostConfiguration(ModelBuilder modelBuilder)
    {
        // MariaDB: Same row size limitations as MySQL
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(string))
                {
                    var columnType = property.GetColumnType();
                    if (columnType == null || columnType.Equals("longtext", StringComparison.OrdinalIgnoreCase))
                    {
                        var maxLength = property.GetMaxLength();
                        if (maxLength == null || maxLength > 4000)
                        {
                            property.SetColumnType("TEXT");
                        }
                        else
                        {
                            property.SetColumnType($"VARCHAR({maxLength})");
                        }
                    }
                }
            }
        }

        // For Galera Cluster: No additional model configuration needed
        // For ColumnStore: Would typically use a different storage engine per table
    }

    public override string OptimizeConnectionString(string baseConnectionString)
    {
        var optimizations = DeploymentMode switch
        {
            DatabaseDeploymentMode.Standalone =>
                ";CharSet=utf8mb4;SslMode=Preferred",
            DatabaseDeploymentMode.Clustered =>
                // Galera-specific: Short timeouts for quick failover, wsrep_sync_wait for read-your-writes
                ";CharSet=utf8mb4;SslMode=Required;ConnectionTimeout=10;DefaultCommandTimeout=30",
            DatabaseDeploymentMode.Hyperscale =>
                // ColumnStore: Longer timeouts for analytical queries
                ";CharSet=utf8mb4;SslMode=Required;ConnectionTimeout=30;DefaultCommandTimeout=600",
            _ => ""
        };

        return baseConnectionString.TrimEnd(';') + optimizations;
    }

    public override string GetUtcNowSql() => "UTC_TIMESTAMP()";
}

/// <summary>
/// MariaDB Galera Cluster-specific strategy.
/// Optimized for synchronous multi-master replication.
/// </summary>
public class MariaDbGaleraStrategy : MariaDbProviderStrategy
{
    public MariaDbGaleraStrategy() : base(DatabaseDeploymentMode.Clustered) { }

    public new string ProviderName => "mariadb-galera";

    // Galera works best with smaller transactions due to certification
    public override int RecommendedBatchSize => 50;
}

/// <summary>
/// MariaDB ColumnStore-specific strategy.
/// Optimized for columnar analytics and HTAP workloads.
/// </summary>
public class MariaDbColumnStoreStrategy : MariaDbProviderStrategy
{
    public MariaDbColumnStoreStrategy() : base(DatabaseDeploymentMode.Hyperscale) { }

    public new string ProviderName => "mariadb-columnstore";

    // ColumnStore is optimized for bulk inserts
    public override int RecommendedBatchSize => 10000;
}
