// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Linq.Expressions;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
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
        var mockSet = new Mock<DbSet<T>>();

        // Setup IAsyncEnumerable
        mockSet.As<IAsyncEnumerable<T>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(() => new TestAsyncEnumerator<T>(items.GetEnumerator()));

        // Setup IQueryable with async provider
        mockSet.As<IQueryable<T>>()
            .Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<T>(items.AsQueryable()));

        mockSet.As<IQueryable<T>>()
            .Setup(m => m.Expression)
            .Returns(items.AsQueryable().Expression);

        mockSet.As<IQueryable<T>>()
            .Setup(m => m.ElementType)
            .Returns(items.AsQueryable().ElementType);

        mockSet.As<IQueryable<T>>()
            .Setup(m => m.GetEnumerator())
            .Returns(() => items.GetEnumerator());

        // Setup FindAsync
        mockSet.Setup(m => m.FindAsync(It.IsAny<object[]>()))
            .Returns<object[]>(keyValues =>
            {
                var key = keyValues.FirstOrDefault();
                return ValueTask.FromResult(FindEntityByKey(items, key));
            });

        mockSet.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns<object[], CancellationToken>((keyValues, _) =>
            {
                var key = keyValues.FirstOrDefault();
                return ValueTask.FromResult(FindEntityByKey(items, key));
            });

        // Setup Add/AddAsync
        mockSet.Setup(m => m.Add(It.IsAny<T>())).Callback<T>(e => items.Add(e));
        mockSet.Setup(m => m.AddAsync(It.IsAny<T>(), It.IsAny<CancellationToken>()))
            .Callback<T, CancellationToken>((e, _) => items.Add(e))
            .ReturnsAsync((T e, CancellationToken _) => (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<T>)null!);

        return mockSet;
    }

    private static T? FindEntityByKey<T>(List<T> items, object? key) where T : class
    {
        if (key == null)
            return default;

        var idProp = typeof(T).GetProperty("Id");
        return idProp == null
            ? items.FirstOrDefault()
            : items.FirstOrDefault(e =>
            {
                var val = idProp.GetValue(e);
                return val != null && Equals(val, Convert.ToInt32(key));
            });
    }
}

/// <summary>
/// Async query provider for EF Core mocking.
/// </summary>
public class TestAsyncQueryProvider<TEntity> : IQueryProvider, IAsyncQueryProvider
{
    private readonly IQueryable<TEntity> _inner;

    public TestAsyncQueryProvider(IQueryable<TEntity> inner) => _inner = inner;

    public IQueryable CreateQuery(Expression expression) => new TestAsyncEnumerable<TEntity>(expression, this);

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new TestAsyncEnumerable<TElement>(expression, this);

    public object? Execute(Expression expression) => _inner.Provider.Execute(expression);

    public TResult Execute<TResult>(Expression expression) => _inner.Provider.Execute<TResult>(expression);

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        var expectedResultType = typeof(TResult).GetGenericArguments()[0];
        var executionResult = typeof(IQueryProvider)
            .GetMethod(
                name: nameof(IQueryProvider.Execute),
                genericParameterCount: 1,
                types: new[] { typeof(Expression) })
            ?.MakeGenericMethod(expectedResultType)
            .Invoke(this, new[] { expression });

        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))
            ?.MakeGenericMethod(expectedResultType)
            .Invoke(null, new[] { executionResult })!;
    }
}

/// <summary>
/// Async enumerable wrapper for EF Core mocking.
/// </summary>
public class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    private readonly IQueryProvider _provider;

    public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable)
    {
        _provider = new TestAsyncQueryProvider<T>(this);
    }

    public TestAsyncEnumerable(Expression expression, IQueryProvider provider) : base(expression)
    {
        _provider = provider;
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

    IQueryProvider IQueryable.Provider => _provider;
}

/// <summary>
/// Async enumerator wrapper for EF Core mocking.
/// </summary>
public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;

    public T Current => _inner.Current;

    public ValueTask<bool> MoveNextAsync() => new(_inner.MoveNext());

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }
}
