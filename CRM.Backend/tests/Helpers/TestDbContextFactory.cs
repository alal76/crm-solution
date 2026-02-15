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
