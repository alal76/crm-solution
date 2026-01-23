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
/// Implementation of IDemoDbContextFactory that creates isolated demo database contexts
/// </summary>
public class DemoDbContextFactory : IDemoDbContextFactory
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DemoDbContextFactory> _logger;
    private readonly string _demoConnectionString;

    public DemoDbContextFactory(IConfiguration configuration, ILogger<DemoDbContextFactory> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _demoConnectionString = configuration.GetConnectionString("DemoConnection") ?? string.Empty;
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
            using var context = CreateDemoContext();
            
            _logger.LogInformation("Checking demo database schema...");
            
            // Check if database exists and has tables
            var canConnect = await context.Database.CanConnectAsync();
            if (!canConnect)
            {
                _logger.LogInformation("Demo database not reachable, waiting for container...");
                // Wait for container to be ready
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

            // Use EnsureCreated for demo database - creates schema without migration history
            // This is appropriate for demo database as it's meant to be disposable
            _logger.LogInformation("Creating demo database schema...");
            await context.Database.EnsureCreatedAsync();
            
            _logger.LogInformation("Demo database schema is ready");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to ensure demo database schema");
            throw;
        }
    }
}
