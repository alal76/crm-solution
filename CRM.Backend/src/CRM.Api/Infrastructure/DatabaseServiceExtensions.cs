// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Interfaces;
using CRM.Core.Ports.Output.Events;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Data.Interceptors;
using CRM.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace CRM.Api.Infrastructure;

/// <summary>
/// AP-039: Database service registrations extracted from Program.cs.
/// Configures the primary <see cref="CrmDbContext"/>, optional read-only analytics replica,
/// and the <see cref="ICrmDbContext"/> interface registration.
/// </summary>
internal static class DatabaseServiceExtensions
{
    /// <summary>
    /// Registers <see cref="CrmDbContext"/>, optional <see cref="CrmReadOnlyDbContext"/>,
    /// and the <see cref="ICrmDbContext"/> interface.
    /// Returns the resolved connection string and provider name for use by downstream
    /// services (Hangfire, startup seeding, etc.).
    /// </summary>
    internal static (string? ConnectionString, string DatabaseProvider) AddDatabaseServices(
        this WebApplicationBuilder builder)
    {
        var databaseProvider = builder.Configuration["DatabaseProvider"] ?? "mariadb";

        // Build connection string from configuration or environment variables
        string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString) &&
            (databaseProvider.ToLower() == "mysql" || databaseProvider.ToLower() == "mariadb"))
        {
            var dbHost = builder.Configuration["DB_HOST"] ?? builder.Configuration["DbHost"] ?? "mariadb";
            var dbPort = builder.Configuration["DB_PORT"] ?? "3306";
            var dbName = builder.Configuration["DB_NAME"] ?? "crm_db";
            var dbUser = builder.Configuration["DB_USER"] ?? "crm_user";
            // SECURITY: DB_PASSWORD must be set in production - see SECURITY_BEST_PRACTICES.md
            var dbPass = builder.Configuration["DB_PASSWORD"] ?? builder.Configuration["DB_PASS"]
                ?? (builder.Environment.IsDevelopment()
                    ? "crm_pass"
                    : throw new InvalidOperationException("DB_PASSWORD environment variable is required in production"));
            connectionString = $"Server={dbHost};Port={dbPort};Database={dbName};Uid={dbUser};Pwd={dbPass};";
        }

        // AP-059: Domain event infrastructure
        builder.Services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();
        builder.Services.AddScoped<DomainEventDispatchInterceptor>();

        builder.Services.AddDbContext<CrmDbContext>((sp, options) =>
        {
            switch (databaseProvider.ToLower())
            {
                case "postgresql":
                    options.UseNpgsql(connectionString);
                    break;
                case "oracle":
                    options.UseOracle(connectionString);
                    break;
                case "mysql":
                case "mariadb":
                    // Use explicit MariaDB version to avoid connection attempts during startup
                    options.UseMySql(connectionString, new MariaDbServerVersion(new Version(11, 0, 0)));
                    break;
                case "inmemory":
                    options.UseInMemoryDatabase("crm_test");
                    break;
                case "sqlserver":
                    options.UseSqlServer(connectionString);
                    break;
                case "sqlite":
                default:
                    options.UseSqlite(connectionString ?? "Data Source=crm.db");
                    break;
            }

            options.AddInterceptors(
                new AuditSaveChangesInterceptor(),
                sp.GetRequiredService<DomainEventDispatchInterceptor>());
        });

        // Optional: Register CrmReadOnlyDbContext for analytics replica routing.
        // Only registered when ConnectionStrings__ReadOnlyConnection is set.
        // Points to crm-mariadb-analytics:3307 with crm_readonly user.
        // See CRM.Infrastructure/Data/CrmReadOnlyDbContext.cs for details.
        var readOnlyConnectionString = builder.Configuration.GetConnectionString("ReadOnlyConnection");
        if (!string.IsNullOrWhiteSpace(readOnlyConnectionString))
        {
            builder.Services.AddDbContext<CrmReadOnlyDbContext>(options =>
            {
                // Always use MariaDB/MySQL for the analytics replica
                options.UseMySql(
                    readOnlyConnectionString,
                    new MariaDbServerVersion(new Version(11, 0, 0)),
                    mySqlOptions => mySqlOptions.EnableRetryOnFailure(3));
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });
            builder.Services.AddScoped<CrmReadOnlyDbContext>();
        }

        // Register ICrmDbContext interface directly from CrmDbContext
        builder.Services.AddScoped<ICrmDbContext>(provider =>
            provider.GetRequiredService<CrmDbContext>());

        return (connectionString, databaseProvider);
    }
}
