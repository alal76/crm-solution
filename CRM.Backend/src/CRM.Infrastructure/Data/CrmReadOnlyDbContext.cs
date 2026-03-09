// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace CRM.Infrastructure.Data;

/// <summary>
/// Read-only DbContext that connects to the analytics replica (crm-mariadb-analytics:3307).
/// All entity queries use AsNoTracking by default — this context CANNOT be used for writes.
///
/// Only registered when the "ReadOnlyConnection" connection string is present in configuration.
/// This allows the application to run without the analytics replica in simpler deployments.
/// </summary>
public class CrmReadOnlyDbContext : CrmDbContext
{
    /// <summary>
    /// Initialises a new read-only DbContext.
    /// Uses <see cref="DbContextOptions{CrmReadOnlyDbContext}"/> so it is registered
    /// independently from the primary <see cref="CrmDbContext"/>.
    /// </summary>
    public CrmReadOnlyDbContext(
        DbContextOptions<CrmReadOnlyDbContext> options,
        IConfiguration configuration)
        : base(GetBaseOptions(options), configuration)
    {
    }

    /// <summary>
    /// Converts typed options to the base <see cref="DbContextOptions{CrmDbContext}"/>
    /// expected by the parent constructor.
    /// </summary>
    private static DbContextOptions<CrmDbContext> GetBaseOptions(
        DbContextOptions<CrmReadOnlyDbContext> options)
    {
        // Copy all option extensions from the typed options into the base type
        var builder = new DbContextOptionsBuilder<CrmDbContext>();
        foreach (var extension in options.Extensions)
        {
            ((IDbContextOptionsBuilderInfrastructure)builder).AddOrUpdateExtension(extension);
        }
        return builder.Options;
    }

    /// <summary>
    /// Overrides model creation to enforce read-only behaviour:
    /// - Disables change tracking on all entity types
    /// - Marks all entities as having no key (cannot be inserted/updated)
    ///   Note: We keep keys so EF can still materialise navigation properties,
    ///   but the context itself is never SaveChanges'd.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply QueryTrackingBehavior.NoTracking at the model level
        // (also enforced via ChangeTracker.QueryTrackingBehavior below)
    }

    /// <summary>
    /// Overrides options builder to set NoTracking as the default behaviour.
    /// </summary>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        // Ensure no tracking is the default for all queries on this context
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    }

    /// <summary>
    /// Throws NotSupportedException — this context is read-only.
    /// </summary>
    public override int SaveChanges()
        => throw new NotSupportedException(
            "CrmReadOnlyDbContext is read-only. Use the primary CrmDbContext for writes.");

    /// <summary>
    /// Throws NotSupportedException — this context is read-only.
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => throw new NotSupportedException(
            "CrmReadOnlyDbContext is read-only. Use the primary CrmDbContext for writes.");

    /// <summary>
    /// Throws NotSupportedException — this context is read-only.
    /// </summary>
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
        => throw new NotSupportedException(
            "CrmReadOnlyDbContext is read-only. Use the primary CrmDbContext for writes.");

    /// <summary>
    /// Throws NotSupportedException — this context is read-only.
    /// </summary>
    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
        => throw new NotSupportedException(
            "CrmReadOnlyDbContext is read-only. Use the primary CrmDbContext for writes.");
}
