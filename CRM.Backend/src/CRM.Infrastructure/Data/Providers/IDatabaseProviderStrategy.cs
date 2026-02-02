// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under AGPL-3.0

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CRM.Infrastructure.Data.Providers;

/// <summary>
/// Strategy interface for database provider-specific EF Core configurations.
/// Implements the Strategy Pattern to handle differences between SQL Server, MySQL/MariaDB, etc.
/// </summary>
public interface IDatabaseProviderStrategy
{
    /// <summary>
    /// Gets the provider name (e.g., "sqlserver", "mysql", "mariadb").
    /// </summary>
    string ProviderName { get; }
    
    /// <summary>
    /// Gets the column type for large text fields (nvarchar(max) for SQL Server, LONGTEXT for MySQL).
    /// </summary>
    string LongTextColumnType { get; }
    
    /// <summary>
    /// Gets the column type for medium text fields.
    /// </summary>
    string TextColumnType { get; }
    
    /// <summary>
    /// Configures the RowVersion property for optimistic concurrency.
    /// SQL Server uses native rowversion, MySQL uses BINARY(8) with concurrency token.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="entityType">The entity type to configure.</param>
    void ConfigureRowVersion(ModelBuilder modelBuilder, IMutableEntityType entityType);
    
    /// <summary>
    /// Applies post-configuration adjustments specific to the database provider.
    /// For SQL Server: Sets all FKs to NoAction to avoid cascade path issues.
    /// For MySQL: Converts LONGTEXT columns to TEXT to avoid row size limits.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    void ApplyPostConfiguration(ModelBuilder modelBuilder);
    
    /// <summary>
    /// Gets the recommended delete behavior for foreign keys.
    /// SQL Server typically uses NoAction to avoid multiple cascade paths.
    /// </summary>
    DeleteBehavior DefaultDeleteBehavior { get; }
}
