// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under AGPL-3.0

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CRM.Infrastructure.Data.Providers;

/// <summary>
/// MySQL/MariaDB-specific database provider strategy.
/// Handles:
/// - BINARY(8) with concurrency token for optimistic concurrency (no native rowversion)
/// - TEXT/LONGTEXT for large text fields
/// - Post-configuration to convert LONGTEXT to TEXT to avoid 65535 byte row size limit
/// </summary>
public class MySqlProviderStrategy : IDatabaseProviderStrategy
{
    public string ProviderName => "mysql";
    
    public string LongTextColumnType => "LONGTEXT";
    
    public string TextColumnType => "TEXT";
    
    public DeleteBehavior DefaultDeleteBehavior => DeleteBehavior.Cascade;
    
    public void ConfigureRowVersion(ModelBuilder modelBuilder, IMutableEntityType entityType)
    {
        var rowVersionProperty = entityType.FindProperty("RowVersion");
        if (rowVersionProperty != null)
        {
            // MariaDB/MySQL: Use BINARY(8) for compatibility with SQL Server's rowversion size
            // Configure as concurrency token with value generated on add/update
            modelBuilder.Entity(entityType.ClrType)
                .Property("RowVersion")
                .HasColumnType("BINARY(8)")
                .IsConcurrencyToken()
                .ValueGeneratedOnAddOrUpdate();
        }
    }
    
    public void ApplyPostConfiguration(ModelBuilder modelBuilder)
    {
        // MySQL/MariaDB: Set string column types to prevent row size issues
        // Pomelo defaults to LONGTEXT which counts against the 65535 byte row limit
        // This converts LONGTEXT to TEXT where appropriate
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(string))
                {
                    // Only set column type if not already explicitly configured
                    var columnType = property.GetColumnType();
                    if (columnType == null || columnType.Equals("longtext", StringComparison.OrdinalIgnoreCase))
                    {
                        var maxLength = property.GetMaxLength();
                        if (maxLength == null || maxLength > 4000)
                        {
                            // Use TEXT type for large/unlimited strings
                            property.SetColumnType("TEXT");
                        }
                        else
                        {
                            // Use VARCHAR for smaller strings
                            property.SetColumnType($"VARCHAR({maxLength})");
                        }
                    }
                }
            }
        }
    }
}

/// <summary>
/// MariaDB-specific strategy (currently identical to MySQL).
/// Separated for future MariaDB-specific optimizations.
/// </summary>
public class MariaDbProviderStrategy : MySqlProviderStrategy
{
    public new string ProviderName => "mariadb";
}
