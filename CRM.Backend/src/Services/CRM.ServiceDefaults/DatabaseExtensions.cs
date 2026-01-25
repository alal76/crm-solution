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
        
        // If DB_USER is set (from Kubernetes secret), build connection string from env vars
        if (!string.IsNullOrWhiteSpace(envDbUser))
        {
            var dbHost = envDbHost ?? "crm-db.crm-app.svc.cluster.local";
            var dbPort = envDbPort ?? "3306";
            var dbName = envDbName ?? "crm_db";
            connectionString = $"Server={dbHost};Port={dbPort};Database={dbName};Uid={envDbUser};Pwd={envDbPass};CharSet=utf8mb4;";
        }
        else
        {
            // Fall back to connection string from configuration
            connectionString = configuration.GetConnectionString(connectionStringName) 
                ?? "Server=crm-db;Port=3306;Database=crm_db;Uid=crm_user;Pwd=CrmPass@Dev2024;CharSet=utf8mb4;";
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
