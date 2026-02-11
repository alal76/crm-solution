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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;

namespace CRM.Tests.Helpers;

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

        // FindAsync: search the backing list by primary key (first object[] arg)
        mockSet.Setup(m => m.FindAsync(It.IsAny<object[]>()))
            .Returns<object[]>(keyValues =>
            {
                var key = keyValues.FirstOrDefault();
                if (key == null) return ValueTask.FromResult<T?>(default);

                // Find the primary key property using EF Core conventions:
                // 1. Property named "Id"
                // 2. Property named "{TypeName}Id" (e.g., KnowledgeArticle → ArticleId, Incident → IncidentId)
                // 3. Fallback to shortest "*Id" property
                var typeName = typeof(T).Name;
                var allIdProps = typeof(T).GetProperties()
                    .Where(p => (p.PropertyType == typeof(int) || p.PropertyType == typeof(int?)) && p.Name.EndsWith("Id"))
                    .ToList();

                var idProp = allIdProps.FirstOrDefault(p => p.Name == "Id")
                    ?? allIdProps.FirstOrDefault(p => p.Name == typeName + "Id")
                    ?? allIdProps.FirstOrDefault(p => typeName.EndsWith(p.Name.Replace("Id", "")))
                    ?? allIdProps.OrderBy(p => p.Name.Length).FirstOrDefault();

                var entity = data.FirstOrDefault(e =>
                {
                    if (idProp == null) return false;
                    var val = idProp.GetValue(e);
                    return val != null && Equals(val, Convert.ToInt32(key));
                });
                return ValueTask.FromResult<T?>(entity);
            });

        // Add / AddAsync
        mockSet.Setup(m => m.Add(It.IsAny<T>())).Callback<T>(e => data.Add(e));
        mockSet.Setup(m => m.AddAsync(It.IsAny<T>(), It.IsAny<CancellationToken>()))
            .Callback<T, CancellationToken>((e, _) => data.Add(e))
            .ReturnsAsync((T e, CancellationToken _) => (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<T>)null!);

        return mockSet;
    }
}

/// <summary>
/// Async query provider for EF Core mocking.
/// </summary>
internal class TestAsyncQueryProvider<TEntity> : IQueryProvider, IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    internal TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;

    public IQueryable CreateQuery(Expression expression) => new TestAsyncEnumerable<TEntity>(expression);

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new TestAsyncEnumerable<TElement>(expression);

    public object? Execute(Expression expression) => _inner.Execute(expression);

    public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        var expectedResultType = typeof(TResult).GetGenericArguments()[0];
        var executionResult = typeof(IQueryProvider)
            .GetMethod(
                name: nameof(IQueryProvider.Execute),
                genericParameterCount: 1,
                types: [typeof(Expression)])
            ?.MakeGenericMethod(expectedResultType)
            .Invoke(this, [expression]);

        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))
            ?.MakeGenericMethod(expectedResultType)
            .Invoke(null, [executionResult])!;
    }
}

/// <summary>
/// Async enumerable wrapper for EF Core mocking.
/// </summary>
internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }

    public TestAsyncEnumerable(Expression expression) : base(expression) { }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}

/// <summary>
/// Async enumerator wrapper for EF Core mocking.
/// </summary>
internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
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
