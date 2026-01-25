// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

#pragma warning disable EF1002 // Risk of vulnerability to SQL injection. The database/table names are from config, not user input.

namespace CRM.Infrastructure.Data;

/// <summary>
/// Factory for creating DbContext instances that connect to the demo database.
/// This ensures demo data operations are completely isolated from production data.
/// </summary>
public interface IDemoDbContextFactory
{
    /// <summary>
    /// Creates a new DbContext connected to the demo database
    /// </summary>
    CrmDbContext CreateDemoContext();
    
    /// <summary>
    /// Checks if the demo database connection is available
    /// </summary>
    Task<bool> IsDemoConnectionAvailableAsync();
    
    /// <summary>
    /// Gets the demo database connection string
    /// </summary>
    string GetDemoConnectionString();
    
    /// <summary>
    /// Ensures the demo database schema is created
    /// </summary>
    Task EnsureDemoSchemaCreatedAsync();
}

/// <summary>
/// Implementation of IDemoDbContextFactory that creates isolated demo database contexts.
/// The demo database (crm_demodb) is created on the same server as the production database.
/// </summary>
public class DemoDbContextFactory : IDemoDbContextFactory
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DemoDbContextFactory> _logger;
    private readonly string _demoConnectionString;
    private readonly string _productionConnectionString;
    private const string DEMO_DATABASE_NAME = "crm_demodb";

    public DemoDbContextFactory(IConfiguration configuration, ILogger<DemoDbContextFactory> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        // Get the demo connection string or derive from production connection
        var explicitDemoConnection = configuration.GetConnectionString("DemoConnection");
        _productionConnectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        
        if (!string.IsNullOrEmpty(explicitDemoConnection))
        {
            _demoConnectionString = explicitDemoConnection;
        }
        else if (!string.IsNullOrEmpty(_productionConnectionString))
        {
            // Derive demo connection from production - same server, different database
            _demoConnectionString = DeriveConnectionString(_productionConnectionString, DEMO_DATABASE_NAME);
            _logger.LogInformation("Demo database will use same server as production with database: {DatabaseName}", DEMO_DATABASE_NAME);
        }
        else
        {
            _demoConnectionString = string.Empty;
        }
    }

    /// <summary>
    /// Derives a new connection string by replacing the database name
    /// </summary>
    private static string DeriveConnectionString(string connectionString, string newDatabaseName)
    {
        // Parse and replace Database= parameter
        var parts = connectionString.Split(';')
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrEmpty(p))
            .ToList();
        
        var newParts = parts
            .Where(p => !p.StartsWith("Database=", StringComparison.OrdinalIgnoreCase))
            .Append($"Database={newDatabaseName}")
            .ToList();
        
        return string.Join(";", newParts);
    }

    public string GetDemoConnectionString() => _demoConnectionString;

    public CrmDbContext CreateDemoContext()
    {
        if (string.IsNullOrEmpty(_demoConnectionString))
        {
            throw new InvalidOperationException("Demo database connection string is not configured. Please set ConnectionStrings:DemoConnection.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<CrmDbContext>();
        
        var serverVersion = new MariaDbServerVersion(new Version(11, 0, 0));
        optionsBuilder.UseMySql(_demoConnectionString, serverVersion, options =>
        {
            options.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null);
            options.CommandTimeout(60);
        });

        return new CrmDbContext(optionsBuilder.Options, _configuration);
    }

    public async Task<bool> IsDemoConnectionAvailableAsync()
    {
        if (string.IsNullOrEmpty(_demoConnectionString))
        {
            _logger.LogWarning("Demo connection string is not configured");
            return false;
        }

        try
        {
            using var context = CreateDemoContext();
            return await context.Database.CanConnectAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to demo database");
            return false;
        }
    }

    public async Task EnsureDemoSchemaCreatedAsync()
    {
        if (string.IsNullOrEmpty(_demoConnectionString))
        {
            throw new InvalidOperationException("Demo database connection string is not configured");
        }

        try
        {
            // First, ensure the demo database exists on the server
            await EnsureDemoDatabaseExistsAsync();
            
            using var context = CreateDemoContext();
            
            _logger.LogInformation("Checking demo database schema...");
            
            // Check if database exists and has tables
            var canConnect = await context.Database.CanConnectAsync();
            if (!canConnect)
            {
                _logger.LogInformation("Demo database not reachable, waiting...");
                // Wait for connection
                for (int i = 0; i < 10; i++)
                {
                    await Task.Delay(2000);
                    if (await context.Database.CanConnectAsync())
                    {
                        canConnect = true;
                        break;
                    }
                }
            }

            if (!canConnect)
            {
                throw new InvalidOperationException("Cannot connect to demo database after waiting");
            }

            // Check if ServiceRequestTypes table exists (new table)
            // If not, we need to recreate the schema to include new tables
            var tableExists = false;
            try
            {
                // Use a separate context for the check to avoid connection disposal issues
                using var checkContext = CreateDemoContext();
                var connection = checkContext.Database.GetDbConnection();
                await connection.OpenAsync();
                using var command = connection.CreateCommand();
                command.CommandText = $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'crm_demodb' AND TABLE_NAME = 'ServiceRequestTypes'";
                var result = await command.ExecuteScalarAsync();
                tableExists = Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not check for ServiceRequestTypes table");
            }

            if (!tableExists)
            {
                _logger.LogInformation("ServiceRequestTypes table not found, dropping and recreating demo database...");
                
                // Drop the demo database using raw SQL
                try
                {
                    var serverConnection = DeriveConnectionString(_productionConnectionString!, "mysql");
                    var optionsBuilder = new DbContextOptionsBuilder<CrmDbContext>();
                    var serverVersion = new MariaDbServerVersion(new Version(11, 0, 0));
                    optionsBuilder.UseMySql(serverConnection, serverVersion);
                    
                    using var serverContext = new CrmDbContext(optionsBuilder.Options, _configuration);
                    await serverContext.Database.ExecuteSqlRawAsync($"DROP DATABASE IF EXISTS `{DEMO_DATABASE_NAME}`");
                    await serverContext.Database.ExecuteSqlRawAsync($"CREATE DATABASE `{DEMO_DATABASE_NAME}` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci");
                    _logger.LogInformation("Demo database recreated successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to drop/recreate demo database");
                }
                
                // Now need a fresh context for the new database
                using var newContext = CreateDemoContext();
                await newContext.Database.EnsureCreatedAsync();
            }
            else
            {
                // Use EnsureCreated for demo database - creates schema without migration history
                // This is appropriate for demo database as it's meant to be disposable
                _logger.LogInformation("Demo database schema exists, ensuring all tables...");
                await context.Database.EnsureCreatedAsync();
            }
            
            _logger.LogInformation("Demo database schema is ready");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to ensure demo database schema");
            throw;
        }
    }

    /// <summary>
    /// Creates the demo database on the server if it doesn't exist.
    /// Uses the production connection to execute CREATE DATABASE command.
    /// </summary>
    private async Task EnsureDemoDatabaseExistsAsync()
    {
        if (string.IsNullOrEmpty(_productionConnectionString))
        {
            _logger.LogWarning("Production connection string not available, cannot auto-create demo database");
            return;
        }

        try
        {
            // Connect to the server (not specific database) using production credentials
            var serverConnection = DeriveConnectionString(_productionConnectionString, "mysql");
            
            var optionsBuilder = new DbContextOptionsBuilder<CrmDbContext>();
            var serverVersion = new MariaDbServerVersion(new Version(11, 0, 0));
            optionsBuilder.UseMySql(serverConnection, serverVersion);

            using var context = new CrmDbContext(optionsBuilder.Options, _configuration);
            
            _logger.LogInformation("Checking if demo database '{DatabaseName}' exists...", DEMO_DATABASE_NAME);
            
            // Check if database exists
            var sql = $"SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{DEMO_DATABASE_NAME}'";
            var exists = await context.Database.ExecuteSqlRawAsync($"SELECT 1 FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{DEMO_DATABASE_NAME}' LIMIT 1") > 0;
            
            if (!exists)
            {
                _logger.LogInformation("Creating demo database '{DatabaseName}'...", DEMO_DATABASE_NAME);
                await context.Database.ExecuteSqlRawAsync($"CREATE DATABASE IF NOT EXISTS `{DEMO_DATABASE_NAME}`");
                
                // Get username from connection string to grant permissions
                var userMatch = System.Text.RegularExpressions.Regex.Match(_productionConnectionString, @"User=([^;]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (userMatch.Success)
                {
                    var username = userMatch.Groups[1].Value;
                    await context.Database.ExecuteSqlRawAsync($"GRANT ALL PRIVILEGES ON `{DEMO_DATABASE_NAME}`.* TO '{username}'@'%'");
                    await context.Database.ExecuteSqlRawAsync("FLUSH PRIVILEGES");
                    _logger.LogInformation("Granted privileges on demo database to user '{Username}'", username);
                }
                
                _logger.LogInformation("Demo database '{DatabaseName}' created successfully", DEMO_DATABASE_NAME);
            }
            else
            {
                _logger.LogInformation("Demo database '{DatabaseName}' already exists", DEMO_DATABASE_NAME);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not auto-create demo database. It may need to be created manually.");
        }
    }
}
