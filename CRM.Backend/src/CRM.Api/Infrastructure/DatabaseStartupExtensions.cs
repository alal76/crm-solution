// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace CRM.Api.Infrastructure;

/// <summary>
/// AP-019: Startup seeding extracted from Program.cs into a dedicated extension method
/// to eliminate inline service-locator patterns and improve Program.cs readability.
/// All <see cref="IServiceProvider"/> resolutions here are properly scoped via
/// <see cref="IServiceScopeFactory"/> (CreateScope) — not service-locator anti-patterns.
/// </summary>
internal static class DatabaseStartupExtensions
{
    /// <summary>
    /// Runs EF Core schema management (migrations / EnsureCreated) then seeds
    /// essential startup data (admin user, master data, module field configs).
    /// Called once from Program.cs after the <see cref="WebApplication"/> is built.
    /// </summary>
    internal static async Task RunStartupSeedingAsync(this WebApplication app, string databaseProvider)
    {
        // ADR-002: Unified EF Core Schema Management
        // IMPORTANT: For MariaDB/MySQL, use scripts/apply-migrations.sh instead of relying
        // on MigrateAsync() at startup. MySQL DDL is auto-committed (not transactional),
        // so partial migration failures leave orphan tables without history records.
        // Set SKIP_DB_MIGRATION=true to skip all migration/schema management at startup.
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
        var skipMigration = Environment.GetEnvironmentVariable("SKIP_DB_MIGRATION") == "true";
        if (skipMigration)
        {
            Log.Information("SKIP_DB_MIGRATION=true — skipping EF Core schema management");
        }
        try
        {
            if (!skipMigration)
            {
                var useEnsureCreated = Environment.GetEnvironmentVariable("USE_ENSURE_CREATED") == "true";

                if (!db.Database.IsRelational())
                {
                    // Non-relational providers (InMemory, etc.) — use EnsureCreated
                    Log.Information("Non-relational provider detected ({Provider}); using EnsureCreated", databaseProvider);
                    await db.Database.EnsureCreatedAsync();
                }
                else if (useEnsureCreated)
                {
                    // Development mode: use EnsureCreated to avoid migration issues
                    Log.Information("USE_ENSURE_CREATED=true — using EnsureCreated for {Provider} (development mode)", databaseProvider);
                    await db.Database.EnsureCreatedAsync();
                }
                else
                {
                    // Check if migrations are pending
                    var pendingMigrations = await db.Database.GetPendingMigrationsAsync();
                    var pendingList = pendingMigrations.ToList();
                    if (pendingList.Count > 0)
                    {
                        Log.Information("Found {Count} pending migration(s) for {Provider}: {Migrations}",
                            pendingList.Count, databaseProvider, string.Join(", ", pendingList));
                        try
                        {
                            await db.Database.MigrateAsync();
                            Log.Information("EF Core migrations applied successfully");
                        }
                        catch (Exception migEx)
                        {
                            const string migrationErrorMessage =
                                "MigrateAsync failed. For MariaDB/MySQL, use scripts/apply-migrations.sh "
                                + "to generate and apply idempotent SQL. MySQL DDL is auto-committed and cannot be "
                                + "rolled back, causing 'Table already exists' errors on retry after partial failures.";
                            Log.Error(migEx, migrationErrorMessage);
                            throw;
                        }
                    }
                    else
                    {
                        Log.Information("No pending migrations for {Provider} — database is up to date", databaseProvider);
                    }
                }
            }

            // Post-migration: check for required tables (only for relational databases)
            if (db.Database.IsRelational())
            {
                var requiredTables = new[] { "UserGroups", "Users", "UserGroupMembers", "Departments", "SystemSettings", "Products", "Accounts", "Contacts", "Quotes", "Commissions" };
                var missingTables = new List<string>();
                foreach (var tableName in requiredTables)
                {
                    var conn = db.Database.GetDbConnection();
                    if (conn.State != System.Data.ConnectionState.Open)
                        await conn.OpenAsync();
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = @tableName";
                    var param = cmd.CreateParameter();
                    param.ParameterName = "@tableName";
                    param.Value = tableName;
                    cmd.Parameters.Add(param);
                    var result = await cmd.ExecuteScalarAsync();
                    var exists = Convert.ToInt32(result) > 0;
                    if (!exists)
                        missingTables.Add(tableName);
                }
                if (missingTables.Count > 0)
                {
                    Log.Fatal("Database migration incomplete. Missing tables: {MissingTables}. Aborting seeding.", string.Join(", ", missingTables));
                    throw new InvalidOperationException($"Missing tables after migration: {string.Join(", ", missingTables)}");
                }
            }

            // DI registration check (basic seeder services only)
            var diChecks = new[] { typeof(IMasterDataSeederService) };
            foreach (var serviceType in diChecks)
            {
                if (scope.ServiceProvider.GetService(serviceType) == null)
                {
                    Log.Fatal("Required service not registered in DI: {ServiceName}", serviceType.Name);
                    throw new InvalidOperationException($"Missing DI registration: {serviceType.Name}");
                }
            }

            // Seed essential data (SysAdmin group + admin user only)
            try
            {
                await DbSeed.SeedAsync(db);
                Log.Information("Database setup completed successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during SysAdmin/admin user seeding");
                throw;
            }

            // Seed master data (ZipCodes, ColorPalettes) if not already populated
            // Skip in test environments to avoid seeding 100k+ ZipCodes into InMemory DB.
            var skipMasterDataSeeding = string.Equals(
                Environment.GetEnvironmentVariable("SKIP_MASTER_DATA_SEEDING"),
                "true",
                StringComparison.OrdinalIgnoreCase);
            if (skipMasterDataSeeding)
            {
                Log.Information("SKIP_MASTER_DATA_SEEDING=true — skipping master data seeding");
            }
            else
            {
            try
            {
                var masterDataSeeder = scope.ServiceProvider.GetRequiredService<IMasterDataSeederService>();
                await masterDataSeeder.SeedIfEmptyAsync();
                var stats = await masterDataSeeder.GetStatsAsync();
                Log.Information("Master data status: {ZipCodeCount} ZIP codes, {ColorPaletteCount} color palettes",
                    stats.ZipCodeCount, stats.ColorPaletteCount);
            }
            catch (Exception masterDataEx)
            {
                Log.Error(masterDataEx, "Failed to seed master data");
                throw;
            }
            }

            // Seed module field configurations (optional, non-blocking)
            // Set FORCE_RESEED_FIELD_CONFIGS=true to delete and re-seed all module field configs on startup
            try
            {
                var coreDataSeeder = scope.ServiceProvider.GetService<ICoreDataSeederService>();
                if (coreDataSeeder != null)
                {
                    var forceReseed = Environment.GetEnvironmentVariable("FORCE_RESEED_FIELD_CONFIGS");
                    if (string.Equals(forceReseed, "true", StringComparison.OrdinalIgnoreCase))
                    {
                        Log.Information("FORCE_RESEED_FIELD_CONFIGS is set — deleting and re-seeding all module field configurations");
                        await coreDataSeeder.ForceReseedModuleFieldConfigurationsAsync();
                        Log.Information("Module field configurations force re-seeded successfully");
                    }
                    else
                    {
                        await coreDataSeeder.SeedModuleFieldConfigurationsAsync();
                        Log.Information("Module field configurations seeded successfully");
                    }
                }
                else
                {
                    Log.Warning("ICoreDataSeederService not registered — skipping module field config seeding");
                }
            }
            catch (Exception fieldConfigEx)
            {
                Log.Warning(fieldConfigEx, "Failed to seed module field configurations (non-fatal)");
            }

            // Seed Report Templates Marketplace catalog (REV-FE-003, optional, non-blocking).
            // These are global reference/catalog rows (not per-tenant demo business
            // records), so — like module field configs above — they are seeded
            // automatically rather than requiring an admin-triggered sample data load.
            try
            {
                var coreDataSeeder = scope.ServiceProvider.GetService<ICoreDataSeederService>();
                if (coreDataSeeder != null)
                {
                    await coreDataSeeder.SeedReportTemplatesAsync();
                    Log.Information("Report templates seeded successfully");
                }
            }
            catch (Exception reportTemplateEx)
            {
                Log.Warning(reportTemplateEx, "Failed to seed report templates (non-fatal)");
            }

            // NOTE: Sample data seeding is NOT run at startup.
            // Use the Python test_data_loader.py script or POST /api/admin/seed to populate sample data.
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Error during database setup. Startup aborted.");
            throw;
        }
    }
}
