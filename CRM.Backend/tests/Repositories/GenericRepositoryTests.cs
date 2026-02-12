using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Repositories;
using CRM.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using FluentAssertions;

namespace CRM.Tests.Repositories;

/// <summary>
/// Unit tests for Repository&lt;T&gt; generic implementation.
/// Tests all 7 IRepository&lt;T&gt; methods using MockDbSetFactory and Mock&lt;ICrmDbContext&gt;.
/// </summary>
public class GenericRepositoryTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly List<Account> _accountData;
    private readonly Mock<DbSet<Account>> _mockDbSet;
    private readonly Repository<Account> _repository;

    public GenericRepositoryTests()
    {
        _accountData = new List<Account>
        {
            new Account { Id = 1, Company = "Acme Corp", Email = "acme@test.com", IsDeleted = false, CreatedAt = DateTime.UtcNow },
            new Account { Id = 2, Company = "Globex Inc", Email = "globex@test.com", IsDeleted = false, CreatedAt = DateTime.UtcNow },
            new Account { Id = 3, Company = "Deleted Co", Email = "del@test.com", IsDeleted = true, CreatedAt = DateTime.UtcNow },
        };

        _mockDbSet = MockDbSetFactory.CreateMockDbSet(_accountData);
        _mockContext = new Mock<ICrmDbContext>();
        _mockContext.Setup(c => c.Set<Account>()).Returns(_mockDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _repository = new Repository<Account>(_mockContext.Object);
    }

    // ========== GetByIdAsync ==========

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity_WhenEntityExists()
    {
        // Act
        var result = await _repository.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Company.Should().Be("Acme Corp");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenEntityDoesNotExist()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenEntityIsDeleted()
    {
        // Act
        var result = await _repository.GetByIdAsync(3);

        // Assert
        result.Should().BeNull();
    }

    // ========== GetAllAsync ==========

    [Fact]
    public async Task GetAllAsync_ShouldReturnOnlyNonDeletedEntities()
    {
        // Act
        var results = await _repository.GetAllAsync();

        // Assert
        var list = results.ToList();
        list.Should().HaveCount(2);
        list.Should().NotContain(a => a.Company == "Deleted Co");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmpty_WhenAllDeleted()
    {
        // Arrange
        var allDeleted = new List<Account>
        {
            new Account { Id = 10, Company = "Gone1", Email = "g1@test.com", IsDeleted = true },
            new Account { Id = 11, Company = "Gone2", Email = "g2@test.com", IsDeleted = true },
        };
        var mockSet = MockDbSetFactory.CreateMockDbSet(allDeleted);
        var mockCtx = new Mock<ICrmDbContext>();
        mockCtx.Setup(c => c.Set<Account>()).Returns(mockSet.Object);
        var repo = new Repository<Account>(mockCtx.Object);

        // Act
        var results = await repo.GetAllAsync();

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmpty_WhenNoEntities()
    {
        // Arrange
        var emptyList = new List<Account>();
        var mockSet = MockDbSetFactory.CreateMockDbSet(emptyList);
        var mockCtx = new Mock<ICrmDbContext>();
        mockCtx.Setup(c => c.Set<Account>()).Returns(mockSet.Object);
        var repo = new Repository<Account>(mockCtx.Object);

        // Act
        var results = await repo.GetAllAsync();

        // Assert
        results.Should().BeEmpty();
    }

    // ========== FindAsync ==========

    [Fact]
    public async Task FindAsync_ShouldReturnMatchingEntities()
    {
        // Act
        var results = await _repository.FindAsync(a => a.Company!.Contains("Acme"));

        // Assert
        var list = results.ToList();
        list.Should().HaveCount(1);
        list[0].Company.Should().Be("Acme Corp");
    }

    [Fact]
    public async Task FindAsync_ShouldExcludeDeletedEntities()
    {
        // Act — predicate matches the deleted entity's company
        var results = await _repository.FindAsync(a => a.Company!.Contains("Deleted"));

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task FindAsync_ShouldReturnEmpty_WhenNoMatch()
    {
        // Act
        var results = await _repository.FindAsync(a => a.Company == "Nonexistent");

        // Assert
        results.Should().BeEmpty();
    }

    // ========== AddAsync ==========

    [Fact]
    public async Task AddAsync_ShouldCallAddAsyncOnDbSet()
    {
        // Arrange
        var newEntity = new Account { Company = "NewCo", Email = "new@test.com" };

        // Act
        await _repository.AddAsync(newEntity);

        // Assert
        _mockDbSet.Verify(s => s.AddAsync(newEntity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddAsync_ShouldNotCallSaveChanges()
    {
        // Arrange
        var newEntity = new Account { Company = "NewCo2", Email = "new2@test.com" };

        // Act
        await _repository.AddAsync(newEntity);

        // Assert — AddAsync should NOT auto-save
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ========== UpdateAsync ==========

    [Fact]
    public async Task UpdateAsync_ShouldCallUpdateOnDbSet()
    {
        // Arrange — ICrmDbContext mock does NOT cast to DbContext,
        // so Repository falls into the else branch: _context.Set<T>().Update(entity)
        var entity = new Account { Id = 1, Company = "Updated Acme", Email = "acme@test.com" };

        // Act
        await _repository.UpdateAsync(entity);

        // Assert
        _mockDbSet.Verify(s => s.Update(entity), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldNotCallSaveChanges()
    {
        // Arrange
        var entity = new Account { Id = 1, Company = "Updated", Email = "up@test.com" };

        // Act
        await _repository.UpdateAsync(entity);

        // Assert
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ========== DeleteAsync ==========

    [Fact]
    public async Task DeleteAsync_ShouldSetIsDeletedToTrue()
    {
        // Arrange
        var entity = new Account { Id = 1, Company = "Acme Corp", Email = "acme@test.com", IsDeleted = false };

        // Act
        await _repository.DeleteAsync(entity);

        // Assert
        entity.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallUpdateOnDbSet()
    {
        // Arrange
        var entity = new Account { Id = 2, Company = "Globex Inc", Email = "globex@test.com", IsDeleted = false };

        // Act
        await _repository.DeleteAsync(entity);

        // Assert — soft delete calls Update internally
        _mockDbSet.Verify(s => s.Update(entity), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldNotHardDeleteEntity()
    {
        // Arrange
        var entity = new Account { Id = 2, Company = "Globex Inc", Email = "globex@test.com", IsDeleted = false };

        // Act
        await _repository.DeleteAsync(entity);

        // Assert — Remove should NOT be called (soft delete, not hard delete)
        _mockDbSet.Verify(s => s.Remove(It.IsAny<Account>()), Times.Never);
    }

    // ========== SaveAsync ==========

    [Fact]
    public async Task SaveAsync_ShouldCallSaveChangesAsync()
    {
        // Act
        await _repository.SaveAsync();

        // Assert
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SaveAsync_ShouldPropagateException_WhenSaveFails()
    {
        // Arrange
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateException("Save failed"));

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateException>(() => _repository.SaveAsync());
    }

    // ========== Full Lifecycle ==========

    [Fact]
    public async Task FullLifecycle_AddUpdateDeleteSave()
    {
        // Arrange
        var entity = new Account { Company = "Lifecycle Co", Email = "lc@test.com" };

        // Act — Add
        await _repository.AddAsync(entity);
        _mockDbSet.Verify(s => s.AddAsync(entity, It.IsAny<CancellationToken>()), Times.Once);

        // Act — Save after add
        await _repository.SaveAsync();
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        // Act — Update
        entity.Company = "Updated Lifecycle Co";
        await _repository.UpdateAsync(entity);
        _mockDbSet.Verify(s => s.Update(entity), Times.Once);

        // Act — Delete
        await _repository.DeleteAsync(entity);
        entity.IsDeleted.Should().BeTrue();

        // Act — Final save
        await _repository.SaveAsync();
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}
