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
/// Abstract base class for database provider strategies.
/// Provides common functionality and default implementations.
/// </summary>
public abstract class DatabaseProviderStrategyBase : IDatabaseProviderStrategy
{
    protected readonly DatabaseDeploymentMode _deploymentMode;

    protected DatabaseProviderStrategyBase(DatabaseDeploymentMode deploymentMode = DatabaseDeploymentMode.Standalone)
    {
        _deploymentMode = deploymentMode;
    }

    public abstract string ProviderName { get; }

    public DatabaseDeploymentMode DeploymentMode => _deploymentMode;

    public abstract string LongTextColumnType { get; }

    public abstract string TextColumnType { get; }

    public abstract string JsonColumnType { get; }

    public abstract string GuidColumnType { get; }

    public abstract string TimestampColumnType { get; }

    public abstract bool SupportsNativeJson { get; }

    public abstract bool SupportsNativeGuid { get; }

    public abstract bool SupportsSequences { get; }

    public virtual int RecommendedBatchSize => _deploymentMode switch
    {
        DatabaseDeploymentMode.Standalone => 100,
        DatabaseDeploymentMode.Clustered => 500,
        DatabaseDeploymentMode.Hyperscale => 1000,
        _ => 100
    };

    public virtual ConnectionPoolSettings ConnectionPoolSettings => _deploymentMode switch
    {
        DatabaseDeploymentMode.Standalone => ConnectionPoolSettings.Standalone,
        DatabaseDeploymentMode.Clustered => ConnectionPoolSettings.Clustered,
        DatabaseDeploymentMode.Hyperscale => ConnectionPoolSettings.Hyperscale,
        _ => ConnectionPoolSettings.Standalone
    };

    public abstract DeleteBehavior DefaultDeleteBehavior { get; }

    public abstract void ConfigureRowVersion(ModelBuilder modelBuilder, IMutableEntityType entityType);

    public abstract void ApplyPostConfiguration(ModelBuilder modelBuilder);

    public virtual void ConfigureIndexes(ModelBuilder modelBuilder)
    {
        // Default: No special index configuration
        // Override in derived classes for provider-specific index features
    }

    public virtual string OptimizeConnectionString(string baseConnectionString)
    {
        // Default: Return connection string as-is
        // Override in derived classes for deployment-specific modifications
        return baseConnectionString;
    }

    /// <summary>
    /// Helper method to check if an entity type has a RowVersion property.
    /// </summary>
    protected static bool HasRowVersionProperty(IMutableEntityType entityType)
    {
        return entityType.FindProperty("RowVersion") != null;
    }
}
