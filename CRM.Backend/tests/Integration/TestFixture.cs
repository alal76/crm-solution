// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CRM.Tests.Integration;

/// <summary>
/// Shared test fixture for integration tests.
/// Sets up an in-memory database and service provider for testing.
/// </summary>
public class TestFixture : IDisposable
{
    public IServiceProvider ServiceProvider { get; }
    private readonly ServiceProvider _serviceProvider;

    public TestFixture()
    {
        var services = new ServiceCollection();

        // Add in-memory database
        services.AddDbContext<CrmDbContext>(options =>
            options.UseInMemoryDatabase($"CrmTestDb_{Guid.NewGuid()}"));

        // Register ICrmDbContext
        services.AddScoped<ICrmDbContext>(sp => sp.GetRequiredService<CrmDbContext>());

        // Add logging
        services.AddLogging(builder => builder.AddDebug());

        // Register services needed for integration tests
        services.AddScoped<ISubscriptionService, SubscriptionService>();

        _serviceProvider = services.BuildServiceProvider();
        ServiceProvider = _serviceProvider;

        // Ensure database is created
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
        dbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}
