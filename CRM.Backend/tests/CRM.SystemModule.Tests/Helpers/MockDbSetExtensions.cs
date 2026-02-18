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

using System.Linq.Expressions;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace CRM.SystemModule.Tests.Helpers;

/// <summary>
/// Extension methods for creating mock DbSet instances from lists.
/// </summary>
public static class MockDbSetExtensions
{
    /// <summary>
    /// Create a mock DbSet from a list of items that supports async queries.
    /// </summary>
    public static Mock<DbSet<T>> CreateMockDbSet<T>(this List<T> items) where T : class
    {
        var queryable = items.AsQueryable();
        var mockSet = new Mock<DbSet<T>>();

        // Setup IAsyncEnumerable
        mockSet.As<IAsyncEnumerable<T>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new AsyncEnumerator<T>(items.GetEnumerator()));

        // Setup IQueryable
        mockSet.As<IQueryable<T>>()
            .Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<T>(queryable.Provider));

        mockSet.As<IQueryable<T>>()
            .Setup(m => m.Expression)
            .Returns(queryable.Expression);

        mockSet.As<IQueryable<T>>()
            .Setup(m => m.ElementType)
            .Returns(queryable.ElementType);

        mockSet.As<IQueryable<T>>()
            .Setup(m => m.GetEnumerator())
            .Returns(queryable.GetEnumerator());

        // Setup FindAsync
        mockSet.Setup(m => m.FindAsync(It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .Returns<object?[]?, CancellationToken>((keys, ct) =>
            {
                var item = items.FirstOrDefault();
                return new ValueTask<T?>(item);
            });

        return mockSet;
    }

    /// <summary>
    /// Setup a DbSet property on a mock context to return the mock DbSet.
    /// </summary>
    public static void SetupDbSet<T>(this Mock<ICrmDbContext> contextMock, Mock<DbSet<T>> dbSetMock) 
        where T : class
    {
        // This is called by setting up the property on the context
    }
}

/// <summary>
/// Async enumerator for mock DbSet.
/// </summary>
public class AsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public AsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public T Current => _inner.Current;

    public async ValueTask<bool> MoveNextAsync()
    {
        return await Task.FromResult(_inner.MoveNext());
    }

    public async ValueTask DisposeAsync()
    {
        await Task.CompletedTask;
        _inner.Dispose();
    }
}

/// <summary>
/// Async query provider for mock DbSet.
/// </summary>
public class TestAsyncQueryProvider<TEntity> : IQueryProvider where TEntity : class
{
    private readonly IQueryProvider _inner;

    public TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(Expression expression)
    {
        return new TestAsyncEnumerable<TEntity>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new TestAsyncEnumerable<TElement>(expression);
    }

    public object Execute(Expression expression)
    {
        return _inner.Execute(expression)!;
    }

    public TResult Execute<TResult>(Expression expression)
    {
        return _inner.Execute<TResult>(expression);
    }

    public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
    {
        return new TestAsyncEnumerable<TResult>(expression);
    }

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
    {
        var result = _inner.Execute(expression);
        return (TResult)(object)Task.FromResult(result)!;
    }
}

/// <summary>
/// Async enumerable for test queries.
/// </summary>
public class TestAsyncEnumerable<T> : IAsyncEnumerable<T>, IQueryable<T>
{
    private readonly IQueryable<T> _inner;

    public TestAsyncEnumerable(Expression expression)
    {
        _inner = new List<T>().AsQueryable().Provider.CreateQuery<T>(expression);
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(_inner.GetEnumerator());
    }

    public IEnumerator<T> GetEnumerator()
    {
        return _inner.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return _inner.GetEnumerator();
    }

    public Expression Expression => _inner.Expression;
    public Type ElementType => _inner.ElementType;
    public IQueryProvider Provider => _inner.Provider;
}

/// <summary>
/// Generic async enumerator for non-class types.
/// </summary>
public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public T Current => _inner.Current;

    public async ValueTask<bool> MoveNextAsync()
    {
        return await Task.FromResult(_inner.MoveNext());
    }

    public async ValueTask DisposeAsync()
    {
        await Task.CompletedTask;
        _inner.Dispose();
    }
}
