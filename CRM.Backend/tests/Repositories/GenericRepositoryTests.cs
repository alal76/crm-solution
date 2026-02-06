// CRM Solution - Customer Relationship Management System
// Generic Repository Unit Tests

using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace CRM.Tests.Repositories;

/// <summary>
/// Unit tests for Generic Repository
/// Covers: CRUD operations, queries, pagination, soft delete
/// </summary>
public class GenericRepositoryTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<DbSet<TestEntity>> _mockDbSet;
    private readonly Mock<ILogger<Repository<TestEntity>>> _mockLogger;

    public GenericRepositoryTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockDbSet = new Mock<DbSet<TestEntity>>();
        _mockLogger = new Mock<ILogger<Repository<TestEntity>>>();
    }

    #region GetById Tests

    [Fact]
    public async Task GetByIdAsync_ExistingEntity_ReturnsEntity()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "Test" };
        var data = new List<TestEntity> { entity }.AsQueryable();

        SetupMockDbSet(data);
        _mockContext.Setup(c => c.Set<TestEntity>()).Returns(_mockDbSet.Object);

        var repository = new Repository<TestEntity>(_mockContext.Object);

        // Act
        var result = await repository.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingEntity_ReturnsNull()
    {
        // Arrange
        var data = new List<TestEntity>().AsQueryable();

        SetupMockDbSet(data);
        _mockContext.Setup(c => c.Set<TestEntity>()).Returns(_mockDbSet.Object);

        var repository = new Repository<TestEntity>(_mockContext.Object);

        // Act
        var result = await repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_DeletedEntity_ReturnsNull()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "Test", IsDeleted = true };
        var data = new List<TestEntity> { entity }.AsQueryable();

        SetupMockDbSet(data);
        _mockContext.Setup(c => c.Set<TestEntity>()).Returns(_mockDbSet.Object);

        var repository = new Repository<TestEntity>(_mockContext.Object);

        // Act
        var result = await repository.GetByIdAsync(1);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAll Tests

    [Fact]
    public async Task GetAllAsync_HasEntities_ReturnsAll()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            new TestEntity { Id = 1, Name = "Test 1" },
            new TestEntity { Id = 2, Name = "Test 2" }
        }.AsQueryable();

        SetupMockDbSet(entities);
        _mockContext.Setup(c => c.Set<TestEntity>()).Returns(_mockDbSet.Object);

        var repository = new Repository<TestEntity>(_mockContext.Object);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_EmptyTable_ReturnsEmpty()
    {
        // Arrange
        var data = new List<TestEntity>().AsQueryable();

        SetupMockDbSet(data);
        _mockContext.Setup(c => c.Set<TestEntity>()).Returns(_mockDbSet.Object);

        var repository = new Repository<TestEntity>(_mockContext.Object);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_ExcludesDeletedEntities()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            new TestEntity { Id = 1, Name = "Active", IsDeleted = false },
            new TestEntity { Id = 2, Name = "Deleted", IsDeleted = true }
        }.AsQueryable();

        SetupMockDbSet(entities);
        _mockContext.Setup(c => c.Set<TestEntity>()).Returns(_mockDbSet.Object);

        var repository = new Repository<TestEntity>(_mockContext.Object);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Active");
    }

    #endregion

    #region Find Tests

    [Fact]
    public async Task FindAsync_MatchingPredicate_ReturnsMatches()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            new TestEntity { Id = 1, Name = "Apple" },
            new TestEntity { Id = 2, Name = "Banana" },
            new TestEntity { Id = 3, Name = "Apple Pie" }
        }.AsQueryable();

        SetupMockDbSet(entities);
        _mockContext.Setup(c => c.Set<TestEntity>()).Returns(_mockDbSet.Object);

        var repository = new Repository<TestEntity>(_mockContext.Object);

        // Act
        var result = await repository.FindAsync(e => e.Name.Contains("Apple"));

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task FindAsync_NoMatches_ReturnsEmpty()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            new TestEntity { Id = 1, Name = "Apple" }
        }.AsQueryable();

        SetupMockDbSet(entities);
        _mockContext.Setup(c => c.Set<TestEntity>()).Returns(_mockDbSet.Object);

        var repository = new Repository<TestEntity>(_mockContext.Object);

        // Act
        var result = await repository.FindAsync(e => e.Name == "Orange");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task FirstOrDefaultAsync_MatchExists_ReturnsFirst()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            new TestEntity { Id = 1, Name = "First" },
            new TestEntity { Id = 2, Name = "Second" }
        }.AsQueryable();

        SetupMockDbSet(entities);
        _mockContext.Setup(c => c.Set<TestEntity>()).Returns(_mockDbSet.Object);

        var repository = new Repository<TestEntity>(_mockContext.Object);

        // Act
        var result = await repository.FirstOrDefaultAsync(e => e.Id > 0);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Add Tests

    [Fact]
    public async Task AddAsync_ValidEntity_AddsEntity()
    {
        // Arrange
        var entity = new TestEntity { Name = "New Entity" };

        _mockDbSet.Setup(d => d.AddAsync(It.IsAny<TestEntity>(), default))
            .ReturnsAsync((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<TestEntity>)null!);

        _mockContext.Setup(c => c.Set<TestEntity>()).Returns(_mockDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        var repository = new Repository<TestEntity>(_mockContext.Object);

        // Act
        var result = await repository.AddAsync(entity);

        // Assert
        result.Should().NotBeNull();
        _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task AddAsync_SetsCreatedAt()
    {
        // Arrange
        var entity = new TestEntity { Name = "New Entity" };
        var beforeAdd = DateTime.UtcNow;

        _mockDbSet.Setup(d => d.AddAsync(It.IsAny<TestEntity>(), default))
            .ReturnsAsync((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<TestEntity>)null!);

        _mockContext.Setup(c => c.Set<TestEntity>()).Returns(_mockDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        var repository = new Repository<TestEntity>(_mockContext.Object);

        // Act
        var result = await repository.AddAsync(entity);

        // Assert
        result.CreatedAt.Should().BeOnOrAfter(beforeAdd);
    }

    [Fact]
    public async Task AddRangeAsync_MultipleEntities_AddsAll()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            new TestEntity { Name = "Entity 1" },
            new TestEntity { Name = "Entity 2" }
        };

        _mockContext.Setup(c => c.Set<TestEntity>()).Returns(_mockDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(2);

        var repository = new Repository<TestEntity>(_mockContext.Object);

        // Act
        await repository.AddRangeAsync(entities);

        // Assert
        _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task UpdateAsync_ExistingEntity_UpdatesEntity()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "Updated" };

        _mockContext.Setup(c => c.Set<TestEntity>()).Returns(_mockDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        var repository = new Repository<TestEntity>(_mockContext.Object);

        // Act
        var result = await repository.UpdateAsync(entity);

        // Assert
        result.Should().NotBeNull();
        _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_SetsUpdatedAt()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "Updated" };
        var beforeUpdate = DateTime.UtcNow;

        _mockContext.Setup(c => c.Set<TestEntity>()).Returns(_mockDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        var repository = new Repository<TestEntity>(_mockContext.Object);

        // Act
        var result = await repository.UpdateAsync(entity);

        // Assert
        result.UpdatedAt.Should().BeOnOrAfter(beforeUpdate);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task DeleteAsync_ExistingEntity_SoftDeletes()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "To Delete", IsDeleted = false };
        var data = new List<TestEntity> { entity }.AsQueryable();

        SetupMockDbSet(data);
        _mockContext.Setup(c => c.Set<TestEntity>()).Returns(_mockDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        var repository = new Repository<TestEntity>(_mockContext.Object);

        // Act
        var result = await repository.DeleteAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_NonExistingEntity_ReturnsFalse()
    {
        // Arrange
        var data = new List<TestEntity>().AsQueryable();

        SetupMockDbSet(data);
        _mockContext.Setup(c => c.Set<TestEntity>()).Returns(_mockDbSet.Object);

        var repository = new Repository<TestEntity>(_mockContext.Object);

        // Act
        var result = await repository.DeleteAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HardDeleteAsync_ExistingEntity_PermanentlyDeletes()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "To Delete" };
        var data = new List<TestEntity> { entity }.AsQueryable();

        SetupMockDbSet(data);
        _mockContext.Setup(c => c.Set<TestEntity>()).Returns(_mockDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        var repository = new Repository<TestEntity>(_mockContext.Object);

        // Act
        var result = await repository.HardDeleteAsync(1);

        // Assert
        result.Should().BeTrue();
        _mockDbSet.Verify(d => d.Remove(It.IsAny<TestEntity>()), Times.Once);
    }

    #endregion

    #region Count Tests

    [Fact]
    public async Task CountAsync_HasEntities_ReturnsCount()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            new TestEntity { Id = 1 },
            new TestEntity { Id = 2 },
            new TestEntity { Id = 3 }
        }.AsQueryable();

        SetupMockDbSet(entities);
        _mockContext.Setup(c => c.Set<TestEntity>()).Returns(_mockDbSet.Object);

        var repository = new Repository<TestEntity>(_mockContext.Object);

        // Act
        var result = await repository.CountAsync();

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public async Task CountAsync_WithPredicate_ReturnsFilteredCount()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            new TestEntity { Id = 1, Name = "Active" },
            new TestEntity { Id = 2, Name = "Active" },
            new TestEntity { Id = 3, Name = "Inactive" }
        }.AsQueryable();

        SetupMockDbSet(entities);
        _mockContext.Setup(c => c.Set<TestEntity>()).Returns(_mockDbSet.Object);

        var repository = new Repository<TestEntity>(_mockContext.Object);

        // Act
        var result = await repository.CountAsync(e => e.Name == "Active");

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async Task AnyAsync_HasMatches_ReturnsTrue()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            new TestEntity { Id = 1, Name = "Test" }
        }.AsQueryable();

        SetupMockDbSet(entities);
        _mockContext.Setup(c => c.Set<TestEntity>()).Returns(_mockDbSet.Object);

        var repository = new Repository<TestEntity>(_mockContext.Object);

        // Act
        var result = await repository.AnyAsync(e => e.Name == "Test");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task AnyAsync_NoMatches_ReturnsFalse()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            new TestEntity { Id = 1, Name = "Test" }
        }.AsQueryable();

        SetupMockDbSet(entities);
        _mockContext.Setup(c => c.Set<TestEntity>()).Returns(_mockDbSet.Object);

        var repository = new Repository<TestEntity>(_mockContext.Object);

        // Act
        var result = await repository.AnyAsync(e => e.Name == "Other");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Pagination Tests

    [Fact]
    public async Task GetPagedAsync_FirstPage_ReturnsFirstPage()
    {
        // Arrange
        var entities = Enumerable.Range(1, 50).Select(i => new TestEntity
        {
            Id = i,
            Name = $"Entity {i}"
        }).AsQueryable();

        SetupMockDbSet(entities);
        _mockContext.Setup(c => c.Set<TestEntity>()).Returns(_mockDbSet.Object);

        var repository = new Repository<TestEntity>(_mockContext.Object);

        // Act
        var result = await repository.GetPagedAsync(1, 10);

        // Assert
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(50);
        result.Page.Should().Be(1);
    }

    [Fact]
    public async Task GetPagedAsync_LastPage_ReturnsRemainingItems()
    {
        // Arrange
        var entities = Enumerable.Range(1, 25).Select(i => new TestEntity
        {
            Id = i,
            Name = $"Entity {i}"
        }).AsQueryable();

        SetupMockDbSet(entities);
        _mockContext.Setup(c => c.Set<TestEntity>()).Returns(_mockDbSet.Object);

        var repository = new Repository<TestEntity>(_mockContext.Object);

        // Act
        var result = await repository.GetPagedAsync(3, 10);

        // Assert
        result.Items.Should().HaveCount(5);
    }

    #endregion

    #region Helper Methods

    private void SetupMockDbSet(IQueryable<TestEntity> data)
    {
        _mockDbSet.As<IQueryable<TestEntity>>().Setup(m => m.Provider).Returns(data.Provider);
        _mockDbSet.As<IQueryable<TestEntity>>().Setup(m => m.Expression).Returns(data.Expression);
        _mockDbSet.As<IQueryable<TestEntity>>().Setup(m => m.ElementType).Returns(data.ElementType);
        _mockDbSet.As<IQueryable<TestEntity>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
    }

    #endregion
}

// Test entity for repository tests
public class TestEntity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
}

public class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
