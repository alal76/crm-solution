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

using CRM.Core.Dtos.ITSM;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.ITSM;
using CRM.Infrastructure.Services.ITSM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services.ITSM;

public class SLAPolicyAdminServiceTests
{
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<ILogger<SLAPolicyAdminService>> _mockLogger;
    private readonly SLAPolicyAdminService _service;

    public SLAPolicyAdminServiceTests()
    {
        _mockDbContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<SLAPolicyAdminService>>();
        _service = new SLAPolicyAdminService(_mockDbContext.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsCreatedPolicy()
    {
        // Arrange
        var dto = new CreateSLAPolicyDto
        {
            Name = "Critical Priority SLA",
            Priority = "Critical",
            ResponseTimeHours = 2,
            ResolutionTimeHours = 24
        };

        var policies = new List<SLAPolicy>().AsQueryable();
        var mockSet = CreateMockDbSet(policies);
        _mockDbContext.Setup(c => c.SLAPolicies).Returns(mockSet.Object);
        _mockDbContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.Name, result.Name);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidTimes_ThrowsException()
    {
        // Arrange
        var dto = new CreateSLAPolicyDto { Name = "Test", ResponseTimeHours = -1, ResolutionTimeHours = 0 };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllActivePolicies()
    {
        // Arrange
        var policies = new List<SLAPolicy>
        {
            new SLAPolicy { SLAPolicyId = 1, Name = "Critical", IsActive = true },
            new SLAPolicy { SLAPolicyId = 2, Name = "High", IsActive = true }
        }.AsQueryable();

        var mockSet = CreateMockDbSet(policies);
        _mockDbContext.Setup(c => c.SLAPolicies).Returns(mockSet.Object);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    private static Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> data) where T : class
    {
        var mockSet = new Mock<DbSet<T>>();
        mockSet.As<IAsyncEnumerable<T>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(data.GetEnumerator()));
        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<T>(data.Provider));
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
        return mockSet;
    }
}

public class EscalationRuleAdminServiceTests
{
    private readonly Mock<IRepository<EscalationRule>> _mockRuleRepository;
    private readonly Mock<IRepository<CRM.Core.Entities.ServiceRequest>> _mockSrRepository;
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<ILogger<EscalationRuleAdminService>> _mockLogger;
    private readonly EscalationRuleAdminService _service;

    public EscalationRuleAdminServiceTests()
    {
        _mockRuleRepository = new Mock<IRepository<EscalationRule>>();
        _mockSrRepository = new Mock<IRepository<CRM.Core.Entities.ServiceRequest>>();
        _mockDbContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<EscalationRuleAdminService>>();
        _service = new EscalationRuleAdminService(
            _mockRuleRepository.Object,
            _mockSrRepository.Object,
            _mockDbContext.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsCreatedRule()
    {
        // Arrange
        var dto = new CreateEscalationRuleDto
        {
            Name = "Auto-Escalate Critical",
            Priority = "Critical",
            AgeInMinutes = 60,
            TargetType = "User"
        };

        _mockRuleRepository.Setup(r => r.AddAsync(It.IsAny<EscalationRule>(), It.IsAny<CancellationToken>()));
        _mockRuleRepository.Setup(r => r.SaveAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.Name, result.Name);
        Assert.Equal(60, result.AgeInMinutes);
    }

    [Fact]
    public async Task TestRuleAsync_WithMatchingConditions_ReturnsMatched()
    {
        // Arrange
        var rule = new EscalationRule { Id = 1, Priority = "Critical", IsActive = true };
        var sr = new CRM.Core.Entities.ServiceRequest { Id = 100, Priority = "Critical" };

        _mockRuleRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(rule);
        _mockSrRepository.Setup(r => r.GetByIdAsync(100, It.IsAny<CancellationToken>())).ReturnsAsync(sr);

        // Act
        var result = await _service.TestRuleAsync(1, 100);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.RuleMatched);
    }

    [Fact]
    public async Task GetApplicableRulesAsync_FiltersByPriority_ReturnsMatchingRules()
    {
        // Arrange
        var rules = new List<EscalationRule>
        {
            new EscalationRule { Id = 1, Priority = "Critical", IsActive = true },
            new EscalationRule { Id = 2, Priority = "High", IsActive = true }
        };

        _mockRuleRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(rules);

        // Act
        var result = await _service.GetApplicableRulesAsync("Critical");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Critical", result.First().Priority);
    }
}

public class ServiceQueueServiceTests
{
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<ILogger<ServiceQueueService>> _mockLogger;
    private readonly ServiceQueueService _service;

    public ServiceQueueServiceTests()
    {
        _mockDbContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<ServiceQueueService>>();
        _service = new ServiceQueueService(_mockDbContext.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsCreatedQueue()
    {
        // Arrange
        var dto = new CreateServiceQueueDto { Name = "Support Queue", Priority = 5 };

        var queues = new List<CRM.Core.Entities.ITSM.ServiceQueue>().AsQueryable();
        var mockSet = CreateMockDbSet(queues);
        _mockDbContext.Setup(c => c.Set<CRM.Core.Entities.ITSM.ServiceQueue>()).Returns(mockSet.Object);
        _mockDbContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.Name, result.Name);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllQueues()
    {
        // Arrange
        var queues = new List<CRM.Core.Entities.ITSM.ServiceQueue>
        {
            new CRM.Core.Entities.ITSM.ServiceQueue { Id = 1, Name = "Support Queue" },
            new CRM.Core.Entities.ITSM.ServiceQueue { Id = 2, Name = "IT Queue" }
        }.AsQueryable();

        var mockSet = CreateMockDbSet(queues);
        _mockDbContext.Setup(c => c.Set<CRM.Core.Entities.ITSM.ServiceQueue>()).Returns(mockSet.Object);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    private static Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> data) where T : class
    {
        var mockSet = new Mock<DbSet<T>>();
        mockSet.As<IAsyncEnumerable<T>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(data.GetEnumerator()));
        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<T>(data.Provider));
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
        return mockSet;
    }
}

// Test helpers for async enumerable mocking
internal class TestAsyncQueryProvider<T> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    internal TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;

    public IQueryable CreateQuery(System.Linq.Expressions.Expression expression) => new TestAsyncEnumerable<T>(expression);
    public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression) => new TestAsyncEnumerable<TElement>(expression);
    public object? Execute(System.Linq.Expressions.Expression expression) => _inner.Execute(expression);
    public TResult Execute<TResult>(System.Linq.Expressions.Expression expression) => _inner.Execute<TResult>(expression);
    public TResult ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression, CancellationToken cancellationToken = default) =>
        Task.FromResult(Execute<TResult>(expression)).Result;
}

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
    public TestAsyncEnumerable(System.Linq.Expressions.Expression expression) : base(expression) { }
    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
        new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}

internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;
    public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;
    public T Current => _inner.Current;
    public ValueTask<bool> MoveNextAsync() => new ValueTask<bool>(_inner.MoveNext());
    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }
}
