// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace CRM.SystemModule.Tests.Helpers;

/// <summary>
/// Base test fixture providing common mock setup for service tests.
/// Eliminates duplication of Mock&lt;ICrmDbContext&gt; and Mock&lt;ILogger&lt;T&gt;&gt; setup across test files.
/// </summary>
/// <typeparam name="TService">The service type being tested (used for logger generic type).</typeparam>
public abstract class ServiceTestFixtureBase<TService> where TService : class
{
    /// <summary>
    /// Mock database context - automatically set up with empty DbSets for all entities.
    /// </summary>
    protected Mock<ICrmDbContext> MockContext { get; }

    /// <summary>
    /// Mock logger for the service under test.
    /// </summary>
    protected Mock<ILogger<TService>> MockLogger { get; }

    /// <summary>
    /// Cancellation token for async operations (default: none).
    /// </summary>
    protected CancellationToken CancellationToken { get; } = CancellationToken.None;

    protected ServiceTestFixtureBase()
    {
        MockContext = new Mock<ICrmDbContext>();
        MockLogger = new Mock<ILogger<TService>>();
    }

    /// <summary>
    /// Helper to set up a DbSet with test data.
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <param name="data">Test data to populate the DbSet</param>
    /// <returns>The mocked DbSet</returns>
    protected Mock<DbSet<TEntity>> SetupDbSet<TEntity>(IList<TEntity> data) where TEntity : class
    {
        var queryable = data.AsQueryable();
        var mockSet = new Mock<DbSet<TEntity>>();

        mockSet.As<IQueryable<TEntity>>().Setup(m => m.Provider).Returns(queryable.Provider);
        mockSet.As<IQueryable<TEntity>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mockSet.As<IQueryable<TEntity>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mockSet.As<IQueryable<TEntity>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

        return mockSet;
    }

    /// <summary>
    /// Verifies that the logger was called with a specific log level at least once.
    /// </summary>
    /// <param name="logLevel">The expected log level</param>
    protected void VerifyLoggerCalled(LogLevel logLevel)
    {
        MockLogger.Verify(
            x => x.Log(
                logLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.AtLeastOnce);
    }

    /// <summary>
    /// Verifies that SaveChangesAsync was called on the DbContext.
    /// </summary>
    /// <param name="times">Expected call count (default: Times.Once())</param>
    protected void VerifyContextSaved(Func<Times>? times = null)
    {
        MockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), times ?? Times.Once);
    }
}
