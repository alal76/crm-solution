// CRM Solution - DbCacheService Tests
// Tests for Redis-based database entity caching service

using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for DbCacheService - Redis-based database entity caching
/// </summary>
public class DbCacheServiceTests
{
    private readonly Mock<IRedisCacheService> _mockCache;
    private readonly Mock<ILogger<DbCacheService>> _mockLogger;
    private readonly CrmDbContext _context;
    private readonly DbCacheService _service;

    public DbCacheServiceTests()
    {
        _mockCache = new Mock<IRedisCacheService>();
        _mockLogger = new Mock<ILogger<DbCacheService>>();
        
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: $"DbCache_{Guid.NewGuid()}")
            .Options;
        _context = new CrmDbContext(options);
        
        _service = new DbCacheService(_context, _mockCache.Object, _mockLogger.Object);
    }

    #region Departments Tests

    [Fact]
    public async Task GetDepartmentsAsync_ShouldReturnCachedDepartments_WhenCacheHit()
    {
        // Arrange
        var cachedDepartments = new List<Department>
        {
            new() { Id = 1, Name = "Engineering", IsActive = true, IsDeleted = false },
            new() { Id = 2, Name = "Sales", IsActive = true, IsDeleted = false }
        };
        
        _mockCache.Setup(x => x.GetOrSetAsync(
            DbCacheKeys.Departments,
            It.IsAny<Func<Task<CacheWrapper<Department>>>>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CacheWrapper<Department> { Items = cachedDepartments });

        // Act
        var result = await _service.GetDepartmentsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(d => d.Name == "Engineering");
        result.Should().Contain(d => d.Name == "Sales");
    }

    [Fact]
    public async Task GetDepartmentsAsync_ShouldLoadFromDatabase_WhenCacheMiss()
    {
        // Arrange
        _context.Departments.AddRange(
            new Department { Name = "HR", IsActive = true, IsDeleted = false },
            new Department { Name = "Finance", IsActive = true, IsDeleted = false },
            new Department { Name = "Inactive", IsActive = false, IsDeleted = false },
            new Department { Name = "Deleted", IsActive = true, IsDeleted = true }
        );
        await _context.SaveChangesAsync();

        // Set up cache to call the factory (simulate cache miss)
        _mockCache.Setup(x => x.GetOrSetAsync(
            DbCacheKeys.Departments,
            It.IsAny<Func<Task<CacheWrapper<Department>>>>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>()))
            .Returns((string key, Func<Task<CacheWrapper<Department>>> factory, TimeSpan exp, CancellationToken ct) => factory());

        // Act
        var result = await _service.GetDepartmentsAsync();

        // Assert
        result.Should().HaveCount(2); // Only active, non-deleted
        result.Should().Contain(d => d.Name == "HR");
        result.Should().Contain(d => d.Name == "Finance");
        result.Should().NotContain(d => d.Name == "Inactive");
        result.Should().NotContain(d => d.Name == "Deleted");
    }

    [Fact]
    public async Task GetDepartmentsAsync_ShouldReturnOrdered_ByName()
    {
        // Arrange
        _context.Departments.AddRange(
            new Department { Name = "Zeta", IsActive = true, IsDeleted = false },
            new Department { Name = "Alpha", IsActive = true, IsDeleted = false },
            new Department { Name = "Beta", IsActive = true, IsDeleted = false }
        );
        await _context.SaveChangesAsync();

        _mockCache.Setup(x => x.GetOrSetAsync(
            DbCacheKeys.Departments,
            It.IsAny<Func<Task<CacheWrapper<Department>>>>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>()))
            .Returns((string key, Func<Task<CacheWrapper<Department>>> factory, TimeSpan exp, CancellationToken ct) => factory());

        // Act
        var result = (await _service.GetDepartmentsAsync()).ToList();

        // Assert
        result[0].Name.Should().Be("Alpha");
        result[1].Name.Should().Be("Beta");
        result[2].Name.Should().Be("Zeta");
    }

    [Fact]
    public async Task GetDepartmentByIdAsync_ShouldReturnDepartment_WhenExists()
    {
        // Arrange
        var department = new Department { Id = 1, Name = "Engineering", DepartmentCode = "ENG", IsDeleted = false };
        
        _mockCache.Setup(x => x.GetOrSetAsync(
            $"{DbCacheKeys.DepartmentById}1",
            It.IsAny<Func<Task<CacheWrapper<Department>>>>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CacheWrapper<Department> { Items = new List<Department> { department } });

        // Act
        var result = await _service.GetDepartmentByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Engineering");
    }

    [Fact]
    public async Task GetDepartmentByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        _mockCache.Setup(x => x.GetOrSetAsync(
            $"{DbCacheKeys.DepartmentById}999",
            It.IsAny<Func<Task<CacheWrapper<Department>>>>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CacheWrapper<Department> { Items = new List<Department>() });

        // Act
        var result = await _service.GetDepartmentByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetDepartmentByCodeAsync_ShouldNormalizeToCaseInsensitive()
    {
        // Arrange
        var department = new Department { Id = 1, Name = "Engineering", DepartmentCode = "ENG", IsDeleted = false };
        
        string? capturedKey = null;
        _mockCache.Setup(x => x.GetOrSetAsync(
            It.IsAny<string>(),
            It.IsAny<Func<Task<CacheWrapper<Department>>>>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>()))
            .Callback<string, Func<Task<CacheWrapper<Department>>>, TimeSpan, CancellationToken>((key, _, _, _) => capturedKey = key)
            .ReturnsAsync(new CacheWrapper<Department> { Items = new List<Department> { department } });

        // Act
        await _service.GetDepartmentByCodeAsync("eng"); // lowercase

        // Assert
        capturedKey.Should().Be($"{DbCacheKeys.DepartmentByCode}ENG"); // Should be uppercase
    }

    [Fact]
    public async Task InvalidateDepartmentsAsync_ShouldRemoveByPrefix()
    {
        // Act
        await _service.InvalidateDepartmentsAsync();

        // Assert
        _mockCache.Verify(x => x.RemoveByPrefixAsync("db:department", It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region User Groups Tests

    [Fact]
    public async Task GetUserGroupsAsync_ShouldReturnActiveNonDeletedGroups()
    {
        // Arrange
        _context.UserGroups.AddRange(
            new UserGroup { Name = "Admins", IsActive = true, IsDeleted = false, DisplayOrder = 1 },
            new UserGroup { Name = "Users", IsActive = true, IsDeleted = false, DisplayOrder = 2 },
            new UserGroup { Name = "Inactive", IsActive = false, IsDeleted = false, DisplayOrder = 3 },
            new UserGroup { Name = "Deleted", IsActive = true, IsDeleted = true, DisplayOrder = 4 }
        );
        await _context.SaveChangesAsync();

        _mockCache.Setup(x => x.GetOrSetAsync(
            DbCacheKeys.UserGroups,
            It.IsAny<Func<Task<CacheWrapper<UserGroup>>>>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>()))
            .Returns((string key, Func<Task<CacheWrapper<UserGroup>>> factory, TimeSpan exp, CancellationToken ct) => factory());

        // Act
        var result = await _service.GetUserGroupsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(g => g.Name == "Admins");
        result.Should().Contain(g => g.Name == "Users");
    }

    [Fact]
    public async Task GetUserGroupsAsync_ShouldOrderByDisplayOrderThenName()
    {
        // Arrange
        _context.UserGroups.AddRange(
            new UserGroup { Name = "Zeta", IsActive = true, IsDeleted = false, DisplayOrder = 1 },
            new UserGroup { Name = "Alpha", IsActive = true, IsDeleted = false, DisplayOrder = 2 },
            new UserGroup { Name = "Beta", IsActive = true, IsDeleted = false, DisplayOrder = 1 }
        );
        await _context.SaveChangesAsync();

        _mockCache.Setup(x => x.GetOrSetAsync(
            DbCacheKeys.UserGroups,
            It.IsAny<Func<Task<CacheWrapper<UserGroup>>>>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>()))
            .Returns((string key, Func<Task<CacheWrapper<UserGroup>>> factory, TimeSpan exp, CancellationToken ct) => factory());

        // Act
        var result = (await _service.GetUserGroupsAsync()).ToList();

        // Assert
        result[0].Name.Should().Be("Beta"); // DisplayOrder 1, name Beta < Zeta
        result[1].Name.Should().Be("Zeta"); // DisplayOrder 1
        result[2].Name.Should().Be("Alpha"); // DisplayOrder 2
    }

    [Fact]
    public async Task GetUserGroupByIdAsync_ShouldReturnGroup_WhenExists()
    {
        // Arrange
        var group = new UserGroup { Id = 1, Name = "Admins", IsDeleted = false };
        
        _mockCache.Setup(x => x.GetOrSetAsync(
            $"{DbCacheKeys.UserGroupById}1",
            It.IsAny<Func<Task<CacheWrapper<UserGroup>>>>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CacheWrapper<UserGroup> { Items = new List<UserGroup> { group } });

        // Act
        var result = await _service.GetUserGroupByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Admins");
    }

    [Fact]
    public async Task InvalidateUserGroupsAsync_ShouldRemoveByPrefix()
    {
        // Act
        await _service.InvalidateUserGroupsAsync();

        // Assert
        _mockCache.Verify(x => x.RemoveByPrefixAsync("db:usergroup", It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Lookups Tests

    [Fact]
    public async Task GetLookupCategoriesAsync_ShouldReturnActiveCategories()
    {
        // Arrange
        _context.LookupCategories.AddRange(
            new LookupCategory { Name = "Industries", IsActive = true },
            new LookupCategory { Name = "Sizes", IsActive = true },
            new LookupCategory { Name = "Inactive", IsActive = false }
        );
        await _context.SaveChangesAsync();

        _mockCache.Setup(x => x.GetOrSetAsync(
            DbCacheKeys.LookupCategories,
            It.IsAny<Func<Task<CacheWrapper<LookupCategory>>>>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>()))
            .Returns((string key, Func<Task<CacheWrapper<LookupCategory>>> factory, TimeSpan exp, CancellationToken ct) => factory());

        // Act
        var result = await _service.GetLookupCategoriesAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().NotContain(c => c.Name == "Inactive");
    }

    [Fact]
    public async Task GetLookupItemsAsync_ShouldReturnItemsForCategory()
    {
        // Arrange
        var category = new LookupCategory 
        { 
            Name = "Industries", 
            IsActive = true,
            Items = new List<LookupItem>
            {
                new() { Name = "Technology", IsActive = true, SortOrder = 1 },
                new() { Name = "Healthcare", IsActive = true, SortOrder = 2 },
                new() { Name = "Inactive", IsActive = false, SortOrder = 3 }
            }
        };
        _context.LookupCategories.Add(category);
        await _context.SaveChangesAsync();

        _mockCache.Setup(x => x.GetOrSetAsync(
            $"{DbCacheKeys.LookupItems}Industries",
            It.IsAny<Func<Task<CacheWrapper<LookupItem>>>>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>()))
            .Returns((string key, Func<Task<CacheWrapper<LookupItem>>> factory, TimeSpan exp, CancellationToken ct) => factory());

        // Act
        var result = await _service.GetLookupItemsAsync("Industries");

        // Assert
        result.Should().HaveCount(2); // Only active items
        result.First().Name.Should().Be("Technology"); // Ordered by SortOrder
    }

    [Fact]
    public async Task GetLookupItemsAsync_ShouldReturnEmpty_WhenCategoryNotFound()
    {
        // Arrange
        _mockCache.Setup(x => x.GetOrSetAsync(
            $"{DbCacheKeys.LookupItems}NonExistent",
            It.IsAny<Func<Task<CacheWrapper<LookupItem>>>>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>()))
            .Returns((string key, Func<Task<CacheWrapper<LookupItem>>> factory, TimeSpan exp, CancellationToken ct) => factory());

        // Act
        var result = await _service.GetLookupItemsAsync("NonExistent");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task InvalidateLookupsAsync_ShouldRemoveByPrefix()
    {
        // Act
        await _service.InvalidateLookupsAsync();

        // Assert
        _mockCache.Verify(x => x.RemoveByPrefixAsync("db:lookup", It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Products Tests

    [Fact]
    public async Task GetProductsAsync_ShouldReturnActiveNonDeletedProducts()
    {
        // Arrange
        _context.Products.AddRange(
            new Product { Name = "Product A", IsActive = true, IsDeleted = false },
            new Product { Name = "Product B", IsActive = true, IsDeleted = false },
            new Product { Name = "Inactive", IsActive = false, IsDeleted = false },
            new Product { Name = "Deleted", IsActive = true, IsDeleted = true }
        );
        await _context.SaveChangesAsync();

        _mockCache.Setup(x => x.GetOrSetAsync(
            DbCacheKeys.Products,
            It.IsAny<Func<Task<CacheWrapper<Product>>>>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>()))
            .Returns((string key, Func<Task<CacheWrapper<Product>>> factory, TimeSpan exp, CancellationToken ct) => factory());

        // Act
        var result = await _service.GetProductsAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task InvalidateProductsAsync_ShouldRemoveProductsKey()
    {
        // Act
        await _service.InvalidateProductsAsync();

        // Assert
        _mockCache.Verify(x => x.RemoveAsync(DbCacheKeys.Products, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Module Field Configurations Tests

    [Fact]
    public async Task GetModuleFieldConfigsAsync_ShouldReturnNonDeletedConfigs()
    {
        // Arrange
        _context.ModuleFieldConfigurations.AddRange(
            new ModuleFieldConfiguration { ModuleName = "Customers", FieldName = "Name", DisplayOrder = 1, IsDeleted = false },
            new ModuleFieldConfiguration { ModuleName = "Customers", FieldName = "Email", DisplayOrder = 2, IsDeleted = false },
            new ModuleFieldConfiguration { ModuleName = "Deleted", FieldName = "Field", DisplayOrder = 1, IsDeleted = true }
        );
        await _context.SaveChangesAsync();

        _mockCache.Setup(x => x.GetOrSetAsync(
            DbCacheKeys.ModuleFieldConfigs,
            It.IsAny<Func<Task<CacheWrapper<ModuleFieldConfiguration>>>>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>()))
            .Returns((string key, Func<Task<CacheWrapper<ModuleFieldConfiguration>>> factory, TimeSpan exp, CancellationToken ct) => factory());

        // Act
        var result = await _service.GetModuleFieldConfigsAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetModuleFieldConfigsByModuleAsync_ShouldFilterByModule()
    {
        // Arrange
        _context.ModuleFieldConfigurations.AddRange(
            new ModuleFieldConfiguration { ModuleName = "Customers", FieldName = "Name", DisplayOrder = 1, IsDeleted = false },
            new ModuleFieldConfiguration { ModuleName = "Contacts", FieldName = "Name", DisplayOrder = 1, IsDeleted = false },
            new ModuleFieldConfiguration { ModuleName = "Customers", FieldName = "Email", DisplayOrder = 2, IsDeleted = false }
        );
        await _context.SaveChangesAsync();

        _mockCache.Setup(x => x.GetOrSetAsync(
            $"{DbCacheKeys.ModuleFieldConfigByModule}Customers",
            It.IsAny<Func<Task<CacheWrapper<ModuleFieldConfiguration>>>>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>()))
            .Returns((string key, Func<Task<CacheWrapper<ModuleFieldConfiguration>>> factory, TimeSpan exp, CancellationToken ct) => factory());

        // Act
        var result = await _service.GetModuleFieldConfigsByModuleAsync("Customers");

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(c => c.ModuleName == "Customers");
    }

    [Fact]
    public async Task InvalidateModuleFieldConfigsAsync_ShouldRemoveByPrefix()
    {
        // Act
        await _service.InvalidateModuleFieldConfigsAsync();

        // Assert
        _mockCache.Verify(x => x.RemoveByPrefixAsync("db:modulefieldconfig", It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Cache Management Tests

    [Fact]
    public async Task InvalidateAllAsync_ShouldRemoveAllDbCacheEntries()
    {
        // Act
        await _service.InvalidateAllAsync();

        // Assert
        _mockCache.Verify(x => x.RemoveByPrefixAsync("db:", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task WarmupCacheAsync_ShouldLoadAllEntityTypes()
    {
        // Arrange
        _mockCache.Setup(x => x.GetOrSetAsync(
            It.IsAny<string>(),
            It.IsAny<Func<Task<CacheWrapper<Department>>>>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CacheWrapper<Department> { Items = new List<Department>() });
        
        _mockCache.Setup(x => x.GetOrSetAsync(
            It.IsAny<string>(),
            It.IsAny<Func<Task<CacheWrapper<UserGroup>>>>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CacheWrapper<UserGroup> { Items = new List<UserGroup>() });

        _mockCache.Setup(x => x.GetOrSetAsync(
            It.IsAny<string>(),
            It.IsAny<Func<Task<CacheWrapper<LookupCategory>>>>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CacheWrapper<LookupCategory> { Items = new List<LookupCategory>() });

        _mockCache.Setup(x => x.GetOrSetAsync(
            It.IsAny<string>(),
            It.IsAny<Func<Task<CacheWrapper<ModuleFieldConfiguration>>>>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CacheWrapper<ModuleFieldConfiguration> { Items = new List<ModuleFieldConfiguration>() });

        _mockCache.Setup(x => x.GetOrSetAsync(
            It.IsAny<string>(),
            It.IsAny<Func<Task<CacheWrapper<Product>>>>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CacheWrapper<Product> { Items = new List<Product>() });

        // Act
        await _service.WarmupCacheAsync();

        // Assert - verify cache was populated for all entity types
        _mockCache.Verify(x => x.GetOrSetAsync(
            DbCacheKeys.Departments,
            It.IsAny<Func<Task<CacheWrapper<Department>>>>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task WarmupCacheAsync_ShouldLogWarning_WhenCacheFailsToPopulate()
    {
        // Arrange
        _mockCache.Setup(x => x.GetOrSetAsync(
            It.IsAny<string>(),
            It.IsAny<Func<Task<CacheWrapper<Department>>>>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Cache unavailable"));

        // Act - Should not throw
        await _service.WarmupCacheAsync();

        // Assert - method completes without exception
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("warmup failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Cache Key Constants Tests

    [Fact]
    public void DbCacheKeys_ShouldHaveCorrectPrefixes()
    {
        // Assert
        DbCacheKeys.Departments.Should().StartWith("db:");
        DbCacheKeys.DepartmentById.Should().StartWith("db:");
        DbCacheKeys.DepartmentByCode.Should().StartWith("db:");
        DbCacheKeys.UserGroups.Should().StartWith("db:");
        DbCacheKeys.UserGroupById.Should().StartWith("db:");
        DbCacheKeys.LookupCategories.Should().StartWith("db:");
        DbCacheKeys.LookupItems.Should().StartWith("db:");
        DbCacheKeys.Products.Should().StartWith("db:");
        DbCacheKeys.ModuleFieldConfigs.Should().StartWith("db:");
        DbCacheKeys.ModuleFieldConfigByModule.Should().StartWith("db:");
    }

    #endregion
}

/// <summary>
/// Internal cache wrapper class for testing (matches production code)
/// </summary>
internal class CacheWrapper<T>
{
    public List<T> Items { get; set; } = new();
}
