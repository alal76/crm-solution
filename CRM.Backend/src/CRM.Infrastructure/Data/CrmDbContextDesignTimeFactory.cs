// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CRM.Infrastructure.Data;

/// <summary>
/// Design-time factory for CrmDbContext — used by EF Core migration tooling.
/// Connection string is read from the environment or falls back to a local default.
/// </summary>
public class CrmDbContextDesignTimeFactory : IDesignTimeDbContextFactory<CrmDbContext>
{
    public CrmDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

        // Design-time fallback: migrations MUST have ConnectionStrings__DefaultConnection set.
        // A deliberately invalid connection string is used so EF tooling fails fast
        // with a clear connection error rather than silently using a hardcoded credential.
        const string MigrationFallback =
            "Server=127.0.0.1;Port=3306;Database=crm_db;User=crm_user;Password=MIGRATION-REQUIRES-ENV-VAR;";

        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = connectionString ?? MigrationFallback,
            })
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<CrmDbContext>();
        optionsBuilder.UseMySql(
            configuration["ConnectionStrings:DefaultConnection"]!,
            new MariaDbServerVersion(new Version(11, 0, 0)),
            o => o.EnableRetryOnFailure());

        return new CrmDbContext(optionsBuilder.Options, configuration);
    }
}
