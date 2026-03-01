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
/// Connection string is read exclusively from the <c>ConnectionStrings__DefaultConnection</c>
/// environment variable. Throws <see cref="InvalidOperationException"/> if not set.
/// </summary>
public class CrmDbContextDesignTimeFactory : IDesignTimeDbContextFactory<CrmDbContext>
{
    public CrmDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

        // Design-time migrations REQUIRE ConnectionStrings__DefaultConnection to be set.
        // No fallback is provided — fail fast with a clear message rather than using any hardcoded value.
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "The environment variable 'ConnectionStrings__DefaultConnection' must be set before running EF Core migrations. " +
                "Example: export ConnectionStrings__DefaultConnection='Server=127.0.0.1;Port=3306;Database=crm_db;User=crm_user;Password=<yourpassword>;'");
        }

        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = connectionString,
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
