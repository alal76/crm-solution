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

using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CRM.ServiceDefaults;

/// <summary>
/// Extension methods for database configuration across microservices
/// </summary>
public static class DatabaseExtensions
{
    /// <summary>
    /// Configure MariaDB/MySQL database connection
    /// </summary>
    public static IServiceCollection AddMariaDbContext<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName = "DefaultConnection") where TContext : DbContext
    {
        // First try to get individual components from environment variables (Kubernetes secrets)
        var envDbHost = Environment.GetEnvironmentVariable("DB_HOST");
        var envDbPort = Environment.GetEnvironmentVariable("DB_PORT");
        var envDbName = Environment.GetEnvironmentVariable("DB_NAME");
        var envDbUser = Environment.GetEnvironmentVariable("DB_USER");
        var envDbPass = Environment.GetEnvironmentVariable("DB_PASSWORD");

        string connectionString;

        // If DB_USER is set (from Kubernetes secret or environment), build connection string from env vars
        if (!string.IsNullOrWhiteSpace(envDbUser))
        {
            var dbHost = envDbHost ?? "crm-db.crm-app.svc.cluster.local";
            var dbPort = envDbPort ?? "3306";
            var dbName = envDbName ?? "crm_db";
            // DB_PASSWORD is REQUIRED when using environment variable config
            if (string.IsNullOrWhiteSpace(envDbPass))
            {
                throw new InvalidOperationException("DB_PASSWORD environment variable is required when DB_USER is set");
            }
            connectionString = $"Server={dbHost};Port={dbPort};Database={dbName};Uid={envDbUser};Pwd={envDbPass};CharSet=utf8mb4;";
        }
        else
        {
            // Fall back to connection string from configuration - no hardcoded password fallback
            connectionString = configuration.GetConnectionString(connectionStringName)
                ?? throw new InvalidOperationException(
                    $"Database connection string '{connectionStringName}' is required. " +
                    "Set via ConnectionStrings:DefaultConnection in appsettings.json or DB_* environment variables.");
        }

        services.AddDbContext<TContext>(options =>
        {
            options.UseMySql(connectionString, new MariaDbServerVersion(new Version(11, 0, 0)),
                mySqlOptions =>
                {
                    mySqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                });
        });

        // Register ICrmDbContext interface - required by Repository<T>
        services.AddScoped<ICrmDbContext>(provider => provider.GetRequiredService<TContext>() as ICrmDbContext
            ?? throw new InvalidOperationException("TContext must implement ICrmDbContext"));

        return services;
    }
}
