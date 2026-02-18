// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;

namespace CRM.Tests.Helpers;

/// <summary>
/// Helper factory for creating test database contexts
/// Using in-memory SQLite for integration tests
/// </summary>
public static class TestDbContextFactory
{
    public static DbContextOptions<CrmDbContext> GetInMemoryOptions(string databaseName = "TestDb")
    {
        return new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;
    }

    public static DbContextOptions<CrmDbContext> GetSqliteOptions(string databasePath)
    {
        var connectionString = $"Data Source={databasePath};";
        
        return new DbContextOptionsBuilder<CrmDbContext>()
            .UseSqlite(connectionString)
            .Options;
    }

    /// <summary>
    /// Creates a new CrmDbContext instance for testing with in-memory database.
    /// </summary>
    public static CrmDbContext GetInMemoryContext(string databaseName = "TestDb")
    {
        var options = GetInMemoryOptions(databaseName);
        var mockConfiguration = new Mock<IConfiguration>();
        return new CrmDbContext(options, mockConfiguration.Object);
    }

    /// <summary>
    /// Creates a new CrmDbContext instance for testing with SQLite database.
    /// </summary>
    public static CrmDbContext GetSqliteContext(string databasePath)
    {
        var options = GetSqliteOptions(databasePath);
        var mockConfiguration = new Mock<IConfiguration>();
        return new CrmDbContext(options, mockConfiguration.Object);
    }
}
