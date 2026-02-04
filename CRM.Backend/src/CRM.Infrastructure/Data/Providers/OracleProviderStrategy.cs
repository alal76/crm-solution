// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under AGPL-3.0

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CRM.Infrastructure.Data.Providers;

/// <summary>
/// Oracle Database-specific provider strategy.
/// Supports: Standalone, Oracle RAC (Clustered), Oracle Autonomous/Exadata (Hyperscale).
///
/// Handles:
/// - ORA_ROWSCN for optimistic concurrency
/// - CLOB/NCLOB for large text fields
/// - RAW(16) for GUIDs (or CHAR(36) for compatibility)
/// - Native JSON type (21c+) or CLOB with JSON constraint
/// - Sequences for ID generation
/// </summary>
public class OracleProviderStrategy : DatabaseProviderStrategyBase
{
    public OracleProviderStrategy(DatabaseDeploymentMode deploymentMode = DatabaseDeploymentMode.Standalone)
        : base(deploymentMode)
    {
    }

    public override string ProviderName => "oracle";

    public override string LongTextColumnType => "NCLOB"; // Unicode CLOB for large text

    public override string TextColumnType => "NVARCHAR2(4000)"; // Max VARCHAR2 size

    public override string JsonColumnType => "NCLOB"; // JSON type in 21c+, CLOB with IS JSON constraint in 12c+

    public override string GuidColumnType => "RAW(16)"; // 16-byte binary, more efficient than CHAR(36)

    public override string TimestampColumnType => "TIMESTAMP WITH TIME ZONE";

    public override bool SupportsNativeJson => true; // 12c+ with IS JSON constraint, 21c+ native

    public override bool SupportsNativeGuid => false; // RAW(16) is close but not true UUID

    public override bool SupportsSequences => true; // Sequences are the Oracle way

    public override DeleteBehavior DefaultDeleteBehavior => DeleteBehavior.Cascade;

    public override int RecommendedBatchSize => _deploymentMode switch
    {
        DatabaseDeploymentMode.Standalone => 100,
        DatabaseDeploymentMode.Clustered => 500,   // RAC can handle parallel DML
        DatabaseDeploymentMode.Hyperscale => 2000, // Autonomous optimized for high throughput
        _ => 100
    };

    public override ConnectionPoolSettings ConnectionPoolSettings => _deploymentMode switch
    {
        DatabaseDeploymentMode.Standalone => new ConnectionPoolSettings
        {
            MinPoolSize = 5,
            MaxPoolSize = 50,
            ConnectionTimeout = 30,
            CommandTimeout = 30,
            EnableRetryOnFailure = true,
            MaxRetryCount = 3
        },
        DatabaseDeploymentMode.Clustered => new ConnectionPoolSettings
        {
            MinPoolSize = 10,
            MaxPoolSize = 100,
            ConnectionTimeout = 15,
            CommandTimeout = 30,
            ConnectionLifetime = 300, // Rotate for RAC node load balancing
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
            MaxRetryCount = 10,
            MaxRetryDelaySeconds = 60
        },
        _ => ConnectionPoolSettings.Standalone
    };

    public override void ConfigureRowVersion(ModelBuilder modelBuilder, IMutableEntityType entityType)
    {
        var rowVersionProperty = entityType.FindProperty("RowVersion");
        if (rowVersionProperty != null)
        {
            // Oracle: Use RAW(8) for compatibility with SQL Server's rowversion
            // Can also use ORA_ROWSCN pseudo-column but that requires special handling
            modelBuilder.Entity(entityType.ClrType)
                .Property("RowVersion")
                .HasColumnType("RAW(8)")
                .IsConcurrencyToken()
                .ValueGeneratedOnAddOrUpdate();
        }
    }

    public override void ApplyPostConfiguration(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                // Handle string columns - Oracle has different limits
                if (property.ClrType == typeof(string))
                {
                    var maxLength = property.GetMaxLength();
                    var columnType = property.GetColumnType();

                    if (columnType == null)
                    {
                        if (maxLength == null || maxLength > 4000)
                        {
                            // Use NCLOB for large strings
                            property.SetColumnType("NCLOB");
                        }
                        else if (maxLength <= 4000)
                        {
                            // Use NVARCHAR2 for smaller strings (Unicode support)
                            property.SetColumnType($"NVARCHAR2({maxLength})");
                        }
                    }
                }

                // Handle GUIDs - Oracle uses RAW(16)
                if (property.ClrType == typeof(Guid) || property.ClrType == typeof(Guid?))
                {
                    if (property.GetColumnType() == null)
                    {
                        property.SetColumnType("RAW(16)");
                    }
                }

                // Handle boolean - Oracle doesn't have native boolean before 23c
                if (property.ClrType == typeof(bool) || property.ClrType == typeof(bool?))
                {
                    if (property.GetColumnType() == null)
                    {
                        property.SetColumnType("NUMBER(1)");
                    }
                }
            }
        }
    }

    public override void ConfigureIndexes(ModelBuilder modelBuilder)
    {
        // Oracle supports bitmap indexes, function-based indexes, partitioned indexes
        // RAC: Consider global vs local indexes for partitioned tables
        // Autonomous: Auto-indexing handles most cases

        if (_deploymentMode == DatabaseDeploymentMode.Hyperscale)
        {
            // Autonomous Database has auto-indexing
            // Exadata has storage indexes
        }
    }

    public override string OptimizeConnectionString(string baseConnectionString)
    {
        // Oracle connection strings use different format (TNS or EZ Connect)
        // These optimizations assume standard Oracle.ManagedDataAccess connection string format
        var optimizations = _deploymentMode switch
        {
            DatabaseDeploymentMode.Standalone =>
                ";Statement Cache Size=50;Self Tuning=True",
            DatabaseDeploymentMode.Clustered =>
                // RAC: Enable load balancing and failover
                ";Statement Cache Size=100;Self Tuning=True;Load Balancing=True;HA Events=True",
            DatabaseDeploymentMode.Hyperscale =>
                // Autonomous: Use connection pooling, parallel execution
                ";Statement Cache Size=200;Self Tuning=True;Connection Timeout=30;Command Timeout=120",
            _ => ""
        };

        return baseConnectionString.TrimEnd(';') + optimizations;
    }
}

/// <summary>
/// Oracle RAC (Real Application Clusters) specific strategy.
/// Optimized for multi-node shared-disk clustering.
/// </summary>
public class OracleRacStrategy : OracleProviderStrategy
{
    public OracleRacStrategy() : base(DatabaseDeploymentMode.Clustered) { }

    public new string ProviderName => "oracle-rac";

    public override int RecommendedBatchSize => 500;

    public override string OptimizeConnectionString(string baseConnectionString)
    {
        // RAC-specific: Connect to SCAN listener, enable load balancing
        return baseConnectionString.TrimEnd(';') +
            ";Load Balancing=True;HA Events=True;Statement Cache Size=100";
    }
}

/// <summary>
/// Oracle Autonomous Database specific strategy.
/// Optimized for Oracle's self-managing cloud database.
/// </summary>
public class OracleAutonomousStrategy : OracleProviderStrategy
{
    public OracleAutonomousStrategy() : base(DatabaseDeploymentMode.Hyperscale) { }

    public new string ProviderName => "oracle-autonomous";

    public override int RecommendedBatchSize => 5000;

    public override ConnectionPoolSettings ConnectionPoolSettings => new()
    {
        MinPoolSize = 20,
        MaxPoolSize = 500, // Autonomous scales automatically
        ConnectionTimeout = 30,
        CommandTimeout = 300, // Auto-scaling may need time
        EnableRetryOnFailure = true,
        MaxRetryCount = 10
    };

    public override string OptimizeConnectionString(string baseConnectionString)
    {
        // Autonomous: Wallet-based connection, auto-scaling
        return baseConnectionString.TrimEnd(';') +
            ";Statement Cache Size=200;Self Tuning=True;Command Timeout=300";
    }
}

/// <summary>
/// Oracle Exadata specific strategy.
/// Optimized for Oracle's engineered system with smart storage.
/// </summary>
public class OracleExadataStrategy : OracleProviderStrategy
{
    public OracleExadataStrategy() : base(DatabaseDeploymentMode.Hyperscale) { }

    public new string ProviderName => "oracle-exadata";

    // Exadata with smart scan can handle very large batches
    public override int RecommendedBatchSize => 10000;

    public override ConnectionPoolSettings ConnectionPoolSettings => new()
    {
        MinPoolSize = 30,
        MaxPoolSize = 500,
        ConnectionTimeout = 30,
        CommandTimeout = 180,
        EnableRetryOnFailure = true,
        MaxRetryCount = 5
    };
}
