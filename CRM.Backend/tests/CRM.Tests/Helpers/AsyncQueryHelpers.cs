// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;

namespace CRM.Tests.Helpers;

/// <summary>
/// Test async query provider for EF Core mocking.
/// </summary>
/// <typeparam name="T">Entity type.</typeparam>
internal class TestAsyncQueryProvider<T> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    internal TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(Expression expression)
    {
        return new TestAsyncEnumerable<T>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new TestAsyncEnumerable<TElement>(expression);
    }

    public object? Execute(Expression expression)
    {
        return _inner.Execute(expression);
    }

    public TResult Execute<TResult>(Expression expression)
    {
        return _inner.Execute<TResult>(expression);
    }

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        var resultType = typeof(TResult).GetGenericArguments()[0];
        var executeMethod = typeof(IQueryProvider)
            .GetMethod(nameof(IQueryProvider.Execute), 1, new[] { typeof(Expression) });
        var result = executeMethod!.MakeGenericMethod(resultType).Invoke(_inner, new object[] { expression });
        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
            .MakeGenericMethod(resultType)
            .Invoke(null, new[] { result })!;
    }
}

/// <summary>
/// Test async enumerable for EF Core mocking.
/// </summary>
/// <typeparam name="T">Entity type.</typeparam>
internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable)
        : base(enumerable)
    {
    }

    public TestAsyncEnumerable(Expression expression)
        : base(expression)
    {
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}

/// <summary>
/// Test async enumerator for EF Core mocking.
/// </summary>
/// <typeparam name="T">Entity type.</typeparam>
internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public T Current => _inner.Current;

    public ValueTask<bool> MoveNextAsync()
    {
        return new ValueTask<bool>(_inner.MoveNext());
    }

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return new ValueTask();
    }
}

/// <summary>
/// Extension methods for creating mock DbSets.
/// </summary>
internal static class MockDbSetExtensions
{
    /// <summary>
    /// Creates a mock DbSet from a list of entities.
    /// </summary>
    public static TestAsyncEnumerable<T> AsAsyncQueryable<T>(this IEnumerable<T> source)
    {
        return new TestAsyncEnumerable<T>(source);
    }
}

/// <summary>
/// Creates mock DbSet instances backed by in-memory lists for EF Core unit testing.
/// </summary>
internal static class MockDbSetFactory
{
    /// <summary>
    /// Creates a mock DbSet&lt;T&gt; that supports async LINQ queries.
    /// </summary>
    public static Mock<DbSet<T>> CreateMockDbSet<T>(List<T> data) where T : class
    {
        var queryable = data.AsQueryable();
        var mockSet = new Mock<DbSet<T>>();

        mockSet.As<IAsyncEnumerable<T>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(() => new TestAsyncEnumerator<T>(data.AsQueryable().GetEnumerator()));

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
            .Returns(() => data.AsQueryable().GetEnumerator());

        T? FindEntity(object? key)
        {
            if (key == null)
                return default;

            var typeName = typeof(T).Name;
            var allIdProps = typeof(T).GetProperties()
                .Where(p => (p.PropertyType == typeof(int) || p.PropertyType == typeof(int?)) && p.Name.EndsWith("Id"))
                .ToList();

            var idProp = allIdProps.FirstOrDefault(p => p.Name == "Id")
                ?? allIdProps.FirstOrDefault(p => p.Name == typeName + "Id")
                ?? allIdProps.FirstOrDefault(p => typeName.EndsWith(p.Name.Replace("Id", "")))
                ?? allIdProps.OrderBy(p => p.Name.Length).FirstOrDefault();

            return data.FirstOrDefault(e =>
            {
                if (idProp == null)
                    return false;
                var val = idProp.GetValue(e);
                return val != null && Equals(val, Convert.ToInt32(key));
            });
        }

        // FindAsync: search the backing list by primary key
        mockSet.Setup(m => m.FindAsync(It.IsAny<object[]>()))
            .Returns<object[]>(keyValues =>
            {
                var key = keyValues.FirstOrDefault();
                return ValueTask.FromResult<T?>(FindEntity(key));
            });

        mockSet.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns<object[], CancellationToken>((keyValues, _) =>
            {
                var key = keyValues.FirstOrDefault();
                return ValueTask.FromResult<T?>(FindEntity(key));
            });

        // Add / AddAsync
        mockSet.Setup(m => m.Add(It.IsAny<T>())).Callback<T>(e => data.Add(e));
        mockSet.Setup(m => m.AddAsync(It.IsAny<T>(), It.IsAny<CancellationToken>()))
            .Callback<T, CancellationToken>((e, _) => data.Add(e))
            .ReturnsAsync((T e, CancellationToken _) => (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<T>)null!);

        return mockSet;
    }
}
