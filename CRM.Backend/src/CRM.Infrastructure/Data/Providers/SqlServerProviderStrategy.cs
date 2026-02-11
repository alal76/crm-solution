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
/// SQL Server-specific database provider strategy.
/// Supports: Standalone, Always On Availability Groups (Clustered), Azure SQL Hyperscale.
///
/// Handles:
/// - Native rowversion for optimistic concurrency
/// - nvarchar(max) for large text fields
/// - NoAction delete behavior to avoid multiple cascade path errors
/// - Deployment-specific optimizations
/// </summary>
public class SqlServerProviderStrategy : DatabaseProviderStrategyBase
{
    public SqlServerProviderStrategy(DatabaseDeploymentMode deploymentMode = DatabaseDeploymentMode.Standalone)
        : base(deploymentMode)
    {
    }

    public override string ProviderName => "sqlserver";

    public override string LongTextColumnType => "nvarchar(max)";

    public override string TextColumnType => "nvarchar(max)";

    public override string JsonColumnType => "nvarchar(max)"; // SQL Server 2016+ has JSON functions but no native type

    public override string GuidColumnType => "uniqueidentifier";

    public override string TimestampColumnType => "datetime2";

    public override bool SupportsNativeJson => false; // Has JSON functions but stores as nvarchar

    public override bool SupportsNativeGuid => true;

    public override bool SupportsSequences => true;

    public override DeleteBehavior DefaultDeleteBehavior => DeleteBehavior.NoAction;

    public override int RecommendedBatchSize => DeploymentMode switch
    {
        DatabaseDeploymentMode.Standalone => 100,
        DatabaseDeploymentMode.Clustered => 500,  // Always On can handle more
        DatabaseDeploymentMode.Hyperscale => 2000, // Azure SQL Hyperscale optimized for large batches
        _ => 100
    };

    public override void ConfigureRowVersion(ModelBuilder modelBuilder, IMutableEntityType entityType)
    {
        var rowVersionProperty = entityType.FindProperty("RowVersion");
        if (rowVersionProperty != null)
        {
            // SQL Server uses native rowversion type (8-byte auto-incrementing value)
            modelBuilder.Entity(entityType.ClrType)
                .Property("RowVersion")
                .IsRowVersion();
        }
    }

    public override void ApplyPostConfiguration(ModelBuilder modelBuilder)
    {
        // SQL Server specific: disable cascade deletes to avoid "multiple cascade paths" errors
        foreach (var relationship in modelBuilder.Model.GetEntityTypes()
            .SelectMany(e => e.GetForeignKeys()))
        {
            relationship.DeleteBehavior = DeleteBehavior.NoAction;
        }
    }

    public override void ConfigureIndexes(ModelBuilder modelBuilder)
    {
        // SQL Server supports filtered indexes and included columns
        // These are typically configured per-entity, but we can set defaults here
        if (DeploymentMode == DatabaseDeploymentMode.Hyperscale)
        {
            // Hyperscale benefits from columnstore indexes for analytics
            // Note: Actual columnstore would require explicit configuration per table
        }
    }

    public override string OptimizeConnectionString(string baseConnectionString)
    {
        var optimizations = DeploymentMode switch
        {
            DatabaseDeploymentMode.Standalone =>
                ";MultipleActiveResultSets=True;TrustServerCertificate=True",
            DatabaseDeploymentMode.Clustered =>
                ";MultipleActiveResultSets=True;MultiSubnetFailover=True;ApplicationIntent=ReadWrite;TrustServerCertificate=True",
            DatabaseDeploymentMode.Hyperscale =>
                ";MultipleActiveResultSets=True;ApplicationIntent=ReadWrite;TrustServerCertificate=True;Command Timeout=120",
            _ => ""
        };

        return baseConnectionString.TrimEnd(';') + optimizations;
    }
}

/// <summary>
/// Azure SQL Hyperscale-specific strategy.
/// Optimized for distributed storage with read replicas.
/// </summary>
public class AzureSqlHyperscaleStrategy : SqlServerProviderStrategy
{
    public AzureSqlHyperscaleStrategy() : base(DatabaseDeploymentMode.Hyperscale) { }

    public new string ProviderName => "azuresql-hyperscale";

    public override int RecommendedBatchSize => 2000;

    public override ConnectionPoolSettings ConnectionPoolSettings => new()
    {
        MinPoolSize = 20,
        MaxPoolSize = 200,
        ConnectionTimeout = 30,
        CommandTimeout = 120, // Longer for distributed queries
        EnableRetryOnFailure = true,
        MaxRetryCount = 10,
        MaxRetryDelaySeconds = 60
    };
}
