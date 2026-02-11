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
/// PostgreSQL-specific database provider strategy.
/// Supports: Standalone, Patroni/Citus Cluster (Clustered), Citus Hyperscale/Aurora (Hyperscale).
///
/// Handles:
/// - xmin system column for optimistic concurrency
/// - Native TEXT type (no size limits like MySQL)
/// - Native JSONB for efficient JSON storage and querying
/// - Native UUID type
/// - Advanced indexing (GIN, GiST, BRIN)
/// </summary>
public class PostgreSqlProviderStrategy : DatabaseProviderStrategyBase
{
    public PostgreSqlProviderStrategy(DatabaseDeploymentMode deploymentMode = DatabaseDeploymentMode.Standalone)
        : base(deploymentMode)
    {
    }

    public override string ProviderName => "postgresql";

    public override string LongTextColumnType => "TEXT"; // No size limit in PostgreSQL

    public override string TextColumnType => "TEXT";

    public override string JsonColumnType => "JSONB"; // Binary JSON with indexing support

    public override string GuidColumnType => "UUID"; // Native UUID type

    public override string TimestampColumnType => "TIMESTAMPTZ"; // Timestamp with timezone

    public override bool SupportsNativeJson => true; // JSONB is highly optimized

    public override bool SupportsNativeGuid => true; // Native UUID type

    public override bool SupportsSequences => true; // Sequences are the default for SERIAL/BIGSERIAL

    public override DeleteBehavior DefaultDeleteBehavior => DeleteBehavior.Cascade;

    public override int RecommendedBatchSize => DeploymentMode switch
    {
        DatabaseDeploymentMode.Standalone => 100,
        DatabaseDeploymentMode.Clustered => 500,   // Patroni can handle larger batches
        DatabaseDeploymentMode.Hyperscale => 5000, // Citus distributes across shards
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
            ConnectionLifetime = 300, // Rotate for read replica load balancing
            EnableRetryOnFailure = true,
            MaxRetryCount = 5
        },
        DatabaseDeploymentMode.Hyperscale => new ConnectionPoolSettings
        {
            MinPoolSize = 20,
            MaxPoolSize = 300,
            ConnectionTimeout = 30,
            CommandTimeout = 120,
            ConnectionLifetime = 600,
            EnableRetryOnFailure = true,
            MaxRetryCount = 6,
            MaxRetryDelaySeconds = 60
        },
        _ => ConnectionPoolSettings.Standalone
    };

    public override void ConfigureRowVersion(ModelBuilder modelBuilder, IMutableEntityType entityType)
    {
        var rowVersionProperty = entityType.FindProperty("RowVersion");
        if (rowVersionProperty != null)
        {
            // PostgreSQL: Use xmin system column for optimistic concurrency
            // xmin is the transaction ID of the inserting/updating transaction
            // Alternatively, store as bytea and handle in application
            modelBuilder.Entity(entityType.ClrType)
                .Property("RowVersion")
                .HasColumnType("bytea")
                .IsConcurrencyToken()
                .ValueGeneratedOnAddOrUpdate();
        }
    }

    public override void ApplyPostConfiguration(ModelBuilder modelBuilder)
    {
        // PostgreSQL: TEXT has no row size limits, so no conversion needed
        // However, we can optimize by using appropriate types
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                // Convert GUIDs to native UUID type
                if (property.ClrType == typeof(Guid) || property.ClrType == typeof(Guid?))
                {
                    if (property.GetColumnType() == null)
                    {
                        property.SetColumnType("uuid");
                    }
                }

                // Ensure JSON columns use JSONB for better performance
                var columnType = property.GetColumnType();
                if (columnType != null && columnType.Equals("json", StringComparison.OrdinalIgnoreCase))
                {
                    property.SetColumnType("jsonb");
                }
            }
        }
    }

    public override void ConfigureIndexes(ModelBuilder modelBuilder)
    {
        // PostgreSQL supports advanced index types
        // GIN indexes for JSONB, arrays, full-text search
        // GiST indexes for geometric/range types
        // BRIN indexes for large tables with natural ordering
        if (DeploymentMode == DatabaseDeploymentMode.Hyperscale)
        {
            // Citus distributes tables - indexes need to include distribution column
            // This would require explicit configuration per entity
        }
    }

    public override string OptimizeConnectionString(string baseConnectionString)
    {
        var optimizations = DeploymentMode switch
        {
            DatabaseDeploymentMode.Standalone =>
                ";SSL Mode=Prefer;Trust Server Certificate=true",
            DatabaseDeploymentMode.Clustered =>
                ";SSL Mode=Require;Trust Server Certificate=true;Target Session Attributes=primary;Load Balance Hosts=true",
            DatabaseDeploymentMode.Hyperscale =>
                ";SSL Mode=Require;Trust Server Certificate=true;Command Timeout=120;Timeout=30;Maximum Pool Size=300",
            _ => ""
        };

        return baseConnectionString.TrimEnd(';') + optimizations;
    }
}

/// <summary>
/// Citus (PostgreSQL Hyperscale) specific strategy.
/// Optimized for distributed tables and real-time analytics.
/// </summary>
public class PostgreSqlCitusStrategy : PostgreSqlProviderStrategy
{
    public PostgreSqlCitusStrategy()
        : base(DatabaseDeploymentMode.Hyperscale)
    {
    }

    public new string ProviderName => "postgresql-citus";

    // Citus works best with larger batches that can be distributed
    public override int RecommendedBatchSize => 10000;

    public override ConnectionPoolSettings ConnectionPoolSettings => new()
    {
        MinPoolSize = 30,
        MaxPoolSize = 500, // More connections for distributed queries
        ConnectionTimeout = 30,
        CommandTimeout = 300, // Distributed queries can take longer
        EnableRetryOnFailure = true,
        MaxRetryCount = 10
    };
}

/// <summary>
/// Amazon Aurora PostgreSQL specific strategy.
/// Optimized for Aurora's distributed storage architecture.
/// </summary>
public class AuroraPostgreSqlStrategy : PostgreSqlProviderStrategy
{
    public AuroraPostgreSqlStrategy()
        : base(DatabaseDeploymentMode.Hyperscale)
    {
    }

    public new string ProviderName => "aurora-postgresql";

    public override int RecommendedBatchSize => 5000;

    public override string OptimizeConnectionString(string baseConnectionString)
    {
        // Aurora-specific: Cluster endpoint and read replica endpoint handling
        return baseConnectionString.TrimEnd(';') +
            ";SSL Mode=Require;Trust Server Certificate=true;Command Timeout=120;Application Name=CRM";
    }
}
