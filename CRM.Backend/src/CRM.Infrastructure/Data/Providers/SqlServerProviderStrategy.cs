// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under AGPL-3.0

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CRM.Infrastructure.Data.Providers;

/// <summary>
/// SQL Server-specific database provider strategy.
/// Handles:
/// - Native rowversion for optimistic concurrency
/// - nvarchar(max) for large text fields
/// - NoAction delete behavior to avoid multiple cascade path errors
/// </summary>
public class SqlServerProviderStrategy : IDatabaseProviderStrategy
{
    public string ProviderName => "sqlserver";
    
    public string LongTextColumnType => "nvarchar(max)";
    
    public string TextColumnType => "nvarchar(max)";
    
    public DeleteBehavior DefaultDeleteBehavior => DeleteBehavior.NoAction;
    
    public void ConfigureRowVersion(ModelBuilder modelBuilder, IMutableEntityType entityType)
    {
        var rowVersionProperty = entityType.FindProperty("RowVersion");
        if (rowVersionProperty != null)
        {
            // SQL Server uses native rowversion type
            modelBuilder.Entity(entityType.ClrType)
                .Property("RowVersion")
                .IsRowVersion();
        }
    }
    
    public void ApplyPostConfiguration(ModelBuilder modelBuilder)
    {
        // SQL Server specific: disable cascade deletes to avoid "multiple cascade paths" errors
        // This MUST be applied after all entity configurations
        foreach (var relationship in modelBuilder.Model.GetEntityTypes()
            .SelectMany(e => e.GetForeignKeys()))
        {
            // SQL Server doesn't support cascade delete on multiple paths
            relationship.DeleteBehavior = DeleteBehavior.NoAction;
        }
    }
}
