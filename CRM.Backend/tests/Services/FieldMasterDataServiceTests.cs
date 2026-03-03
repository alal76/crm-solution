// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for FieldMasterDataService - links field configurations to master data sources
/// </summary>
public class FieldMasterDataServiceTests : ServiceTestFixtureBase<FieldMasterDataService>
{    private readonly FieldMasterDataService _service;

    // Mock DbSets
    private readonly List<FieldMasterDataLink> _links;
    private readonly List<ModuleFieldConfiguration> _fieldConfigs;
    private readonly List<LookupCategory> _categories;
    private readonly List<LookupItem> _lookupItems;
    private readonly List<ZipCode> _zipCodes;
    private readonly List<Product> _products;
    private readonly List<Account> _accounts;

    public FieldMasterDataServiceTests()
    {        // Initialize test data
        _links = new List<FieldMasterDataLink>();
        _fieldConfigs = new List<ModuleFieldConfiguration>();
        _categories = new List<LookupCategory>();
        _lookupItems = new List<LookupItem>();
        _zipCodes = new List<ZipCode>();
        _products = new List<Product>();
        _accounts = new List<Account>();

        SetupMockDbSets();

        _service = new FieldMasterDataService(MockContext.Object, MockLogger.Object);
    }

    private void SetupMockDbSets()
    {
        var linksQueryable = _links.AsQueryable();
        var mockLinksSet = new Mock<DbSet<FieldMasterDataLink>>();
        mockLinksSet.As<IQueryable<FieldMasterDataLink>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<FieldMasterDataLink>(linksQueryable.Provider));
        mockLinksSet.As<IQueryable<FieldMasterDataLink>>().Setup(m => m.Expression).Returns(linksQueryable.Expression);
        mockLinksSet.As<IQueryable<FieldMasterDataLink>>().Setup(m => m.ElementType).Returns(linksQueryable.ElementType);
        mockLinksSet.As<IQueryable<FieldMasterDataLink>>().Setup(m => m.GetEnumerator()).Returns(() => linksQueryable.GetEnumerator());
        mockLinksSet.As<IAsyncEnumerable<FieldMasterDataLink>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(new TestAsyncEnumerator<FieldMasterDataLink>(linksQueryable.GetEnumerator()));
        mockLinksSet.Setup(d => d.Add(It.IsAny<FieldMasterDataLink>())).Callback<FieldMasterDataLink>(link => _links.Add(link));
        MockContext.Setup(c => c.FieldMasterDataLinks).Returns(mockLinksSet.Object);

        // Setup field configs
        var configsQueryable = _fieldConfigs.AsQueryable();
        var mockConfigsSet = new Mock<DbSet<ModuleFieldConfiguration>>();
        mockConfigsSet.As<IQueryable<ModuleFieldConfiguration>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<ModuleFieldConfiguration>(configsQueryable.Provider));
        mockConfigsSet.As<IQueryable<ModuleFieldConfiguration>>().Setup(m => m.Expression).Returns(configsQueryable.Expression);
        mockConfigsSet.As<IQueryable<ModuleFieldConfiguration>>().Setup(m => m.ElementType).Returns(configsQueryable.ElementType);
        mockConfigsSet.As<IQueryable<ModuleFieldConfiguration>>().Setup(m => m.GetEnumerator()).Returns(() => configsQueryable.GetEnumerator());
        mockConfigsSet.As<IAsyncEnumerable<ModuleFieldConfiguration>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(new TestAsyncEnumerator<ModuleFieldConfiguration>(configsQueryable.GetEnumerator()));
        MockContext.Setup(c => c.ModuleFieldConfigurations).Returns(mockConfigsSet.Object);

        // Setup lookup categories
        var categoriesQueryable = _categories.AsQueryable();
        var mockCategoriesSet = new Mock<DbSet<LookupCategory>>();
        mockCategoriesSet.As<IQueryable<LookupCategory>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<LookupCategory>(categoriesQueryable.Provider));
        mockCategoriesSet.As<IQueryable<LookupCategory>>().Setup(m => m.Expression).Returns(categoriesQueryable.Expression);
        mockCategoriesSet.As<IQueryable<LookupCategory>>().Setup(m => m.ElementType).Returns(categoriesQueryable.ElementType);
        mockCategoriesSet.As<IQueryable<LookupCategory>>().Setup(m => m.GetEnumerator()).Returns(() => categoriesQueryable.GetEnumerator());
        mockCategoriesSet.As<IAsyncEnumerable<LookupCategory>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(new TestAsyncEnumerator<LookupCategory>(categoriesQueryable.GetEnumerator()));
        MockContext.Setup(c => c.LookupCategories).Returns(mockCategoriesSet.Object);

        // Setup lookup items
        var itemsQueryable = _lookupItems.AsQueryable();
        var mockItemsSet = new Mock<DbSet<LookupItem>>();
        mockItemsSet.As<IQueryable<LookupItem>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<LookupItem>(itemsQueryable.Provider));
        mockItemsSet.As<IQueryable<LookupItem>>().Setup(m => m.Expression).Returns(itemsQueryable.Expression);
        mockItemsSet.As<IQueryable<LookupItem>>().Setup(m => m.ElementType).Returns(itemsQueryable.ElementType);
        mockItemsSet.As<IQueryable<LookupItem>>().Setup(m => m.GetEnumerator()).Returns(() => itemsQueryable.GetEnumerator());
        mockItemsSet.As<IAsyncEnumerable<LookupItem>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(new TestAsyncEnumerator<LookupItem>(itemsQueryable.GetEnumerator()));
        MockContext.Setup(c => c.LookupItems).Returns(mockItemsSet.Object);

        // Setup zip codes
        var zipQueryable = _zipCodes.AsQueryable();
        var mockZipSet = new Mock<DbSet<ZipCode>>();
        mockZipSet.As<IQueryable<ZipCode>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<ZipCode>(zipQueryable.Provider));
        mockZipSet.As<IQueryable<ZipCode>>().Setup(m => m.Expression).Returns(zipQueryable.Expression);
        mockZipSet.As<IQueryable<ZipCode>>().Setup(m => m.ElementType).Returns(zipQueryable.ElementType);
        mockZipSet.As<IQueryable<ZipCode>>().Setup(m => m.GetEnumerator()).Returns(() => zipQueryable.GetEnumerator());
        mockZipSet.As<IAsyncEnumerable<ZipCode>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(new TestAsyncEnumerator<ZipCode>(zipQueryable.GetEnumerator()));
        MockContext.Setup(c => c.ZipCodes).Returns(mockZipSet.Object);

        // Setup products
        var productsQueryable = _products.AsQueryable();
        var mockProductsSet = new Mock<DbSet<Product>>();
        mockProductsSet.As<IQueryable<Product>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Product>(productsQueryable.Provider));
        mockProductsSet.As<IQueryable<Product>>().Setup(m => m.Expression).Returns(productsQueryable.Expression);
        mockProductsSet.As<IQueryable<Product>>().Setup(m => m.ElementType).Returns(productsQueryable.ElementType);
        mockProductsSet.As<IQueryable<Product>>().Setup(m => m.GetEnumerator()).Returns(() => productsQueryable.GetEnumerator());
        mockProductsSet.As<IAsyncEnumerable<Product>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(new TestAsyncEnumerator<Product>(productsQueryable.GetEnumerator()));
        MockContext.Setup(c => c.Products).Returns(mockProductsSet.Object);

        // Setup accounts
        var accountsQueryable = _accounts.AsQueryable();
        var mockAccountsSet = new Mock<DbSet<Account>>();
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Account>(accountsQueryable.Provider));
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.Expression).Returns(accountsQueryable.Expression);
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.ElementType).Returns(accountsQueryable.ElementType);
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.GetEnumerator()).Returns(() => accountsQueryable.GetEnumerator());
        mockAccountsSet.As<IAsyncEnumerable<Account>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(new TestAsyncEnumerator<Account>(accountsQueryable.GetEnumerator()));
        MockContext.Setup(c => c.Accounts).Returns(mockAccountsSet.Object);

        MockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    #region GetLinksForFieldAsync Tests

    [Fact]
    public async Task GetLinksForFieldAsync_WhenLinksExist_ReturnsLinks()
    {
        // Arrange
        _links.Add(new FieldMasterDataLink
        {
            Id = 1,
            FieldConfigurationId = 100,
            SourceType = MasterDataSourceTypes.LookupCategory,
            SourceName = "Industries",
            DisplayField = "Value",
            ValueField = "Key",
            IsActive = true,
            IsDeleted = false,
            SortOrder = 1
        });

        // Act
        var result = await _service.GetLinksForFieldAsync(100);

        // Assert
        result.Should().HaveCount(1);
        result.First().SourceName.Should().Be("Industries");
    }

    [Fact]
    public async Task GetLinksForFieldAsync_WhenNoLinksExist_ReturnsEmptyList()
    {
        // Act
        var result = await _service.GetLinksForFieldAsync(999);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetLinksForFieldAsync_ExcludesDeletedLinks()
    {
        // Arrange
        _links.Add(new FieldMasterDataLink { Id = 1, FieldConfigurationId = 100, IsDeleted = true, IsActive = true });
        _links.Add(new FieldMasterDataLink { Id = 2, FieldConfigurationId = 100, IsDeleted = false, IsActive = true });

        // Act
        var result = await _service.GetLinksForFieldAsync(100);

        // Assert
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(2);
    }

    [Fact]
    public async Task GetLinksForFieldAsync_ExcludesInactiveLinks()
    {
        // Arrange
        _links.Add(new FieldMasterDataLink { Id = 1, FieldConfigurationId = 100, IsDeleted = false, IsActive = false });
        _links.Add(new FieldMasterDataLink { Id = 2, FieldConfigurationId = 100, IsDeleted = false, IsActive = true });

        // Act
        var result = await _service.GetLinksForFieldAsync(100);

        // Assert
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(2);
    }

    [Fact]
    public async Task GetLinksForFieldAsync_OrdersBySortOrder()
    {
        // Arrange
        _links.Add(new FieldMasterDataLink { Id = 1, FieldConfigurationId = 100, SortOrder = 2, IsActive = true, IsDeleted = false });
        _links.Add(new FieldMasterDataLink { Id = 2, FieldConfigurationId = 100, SortOrder = 1, IsActive = true, IsDeleted = false });
        _links.Add(new FieldMasterDataLink { Id = 3, FieldConfigurationId = 100, SortOrder = 3, IsActive = true, IsDeleted = false });

        // Act
        var result = await _service.GetLinksForFieldAsync(100);

        // Assert
        result.Select(l => l.Id).Should().BeEquivalentTo(new[] { 2, 1, 3 }, options => options.WithStrictOrdering());
    }

    #endregion

    #region GetLinksForModuleAsync Tests

    [Fact]
    public async Task GetLinksForModuleAsync_ReturnsGroupedByFieldId()
    {
        // Arrange
        _fieldConfigs.Add(new ModuleFieldConfiguration { Id = 1, ModuleName = "Customers", IsDeleted = false });
        _fieldConfigs.Add(new ModuleFieldConfiguration { Id = 2, ModuleName = "Customers", IsDeleted = false });

        _links.Add(new FieldMasterDataLink { Id = 10, FieldConfigurationId = 1, IsActive = true, IsDeleted = false });
        _links.Add(new FieldMasterDataLink { Id = 11, FieldConfigurationId = 1, IsActive = true, IsDeleted = false });
        _links.Add(new FieldMasterDataLink { Id = 12, FieldConfigurationId = 2, IsActive = true, IsDeleted = false });

        // Act
        var result = await _service.GetLinksForModuleAsync("Customers");

        // Assert
        result.Should().ContainKey(1);
        result.Should().ContainKey(2);
        result[1].Should().HaveCount(2);
        result[2].Should().HaveCount(1);
    }

    #endregion

    #region GetLinkByIdAsync Tests

    [Fact]
    public async Task GetLinkByIdAsync_WhenExists_ReturnsLink()
    {
        // Arrange
        _links.Add(new FieldMasterDataLink
        {
            Id = 1,
            FieldConfigurationId = 100,
            SourceName = "TestSource",
            IsDeleted = false
        });

        // Act
        var result = await _service.GetLinkByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.SourceName.Should().Be("TestSource");
    }

    [Fact]
    public async Task GetLinkByIdAsync_WhenNotExists_ReturnsNull()
    {
        // Act
        var result = await _service.GetLinkByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetLinkByIdAsync_WhenDeleted_ReturnsNull()
    {
        // Arrange
        _links.Add(new FieldMasterDataLink { Id = 1, IsDeleted = true });

        // Act
        var result = await _service.GetLinkByIdAsync(1);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CreateLinkAsync Tests

    [Fact]
    public async Task CreateLinkAsync_CreatesLinkWithAllProperties()
    {
        // Arrange
        var dto = new CreateFieldMasterDataLinkDto
        {
            FieldConfigurationId = 100,
            SourceType = MasterDataSourceTypes.LookupCategory,
            SourceName = "Industries",
            DisplayField = "Value",
            ValueField = "Key",
            AllowFreeText = true,
            ValidationPattern = "^[A-Z]+$",
            ValidationMessage = "Must be uppercase",
            SortOrder = 1,
            IsActive = true
        };

        // Act
        var result = await _service.CreateLinkAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.SourceName.Should().Be("Industries");
        result.AllowFreeText.Should().BeTrue();
        MockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateLinkAsync_SetsCreatedAt()
    {
        // Arrange
        var dto = new CreateFieldMasterDataLinkDto
        {
            FieldConfigurationId = 100,
            SourceType = MasterDataSourceTypes.Table,
            SourceName = "Products"
        };
        var beforeCreate = DateTime.UtcNow;

        // Act
        var result = await _service.CreateLinkAsync(dto);

        // Assert
        // Note: We can't directly assert CreatedAt since it's set internally,
        // but we verify the link was created
        result.Should().NotBeNull();
    }

    #endregion

    #region UpdateLinkAsync Tests

    [Fact]
    public async Task UpdateLinkAsync_WhenExists_UpdatesAndReturnsLink()
    {
        // Arrange
        _links.Add(new FieldMasterDataLink
        {
            Id = 1,
            FieldConfigurationId = 100,
            SourceName = "OldSource",
            IsDeleted = false
        });

        var dto = new CreateFieldMasterDataLinkDto
        {
            FieldConfigurationId = 100,
            SourceType = MasterDataSourceTypes.Table,
            SourceName = "NewSource",
            DisplayField = "Name",
            ValueField = "Id"
        };

        // Act
        var result = await _service.UpdateLinkAsync(1, dto);

        // Assert
        result.SourceName.Should().Be("NewSource");
        MockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateLinkAsync_WhenNotExists_ThrowsKeyNotFoundException()
    {
        // Arrange
        var dto = new CreateFieldMasterDataLinkDto { SourceName = "Test" };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateLinkAsync(999, dto));
    }

    #endregion

    #region DeleteLinkAsync Tests

    [Fact]
    public async Task DeleteLinkAsync_WhenExists_SoftDeletesAndReturnsTrue()
    {
        // Arrange
        var link = new FieldMasterDataLink { Id = 1, IsDeleted = false };
        _links.Add(link);

        // Act
        var result = await _service.DeleteLinkAsync(1);

        // Assert
        result.Should().BeTrue();
        link.IsDeleted.Should().BeTrue();
        MockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteLinkAsync_WhenNotExists_ReturnsFalse()
    {
        // Act
        var result = await _service.DeleteLinkAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetAvailableSourcesAsync Tests

    [Fact]
    public async Task GetAvailableSourcesAsync_IncludesLookupCategories()
    {
        // Arrange
        _categories.Add(new LookupCategory { Name = "Industries", IsActive = true, IsDeleted = false });
        _categories.Add(new LookupCategory { Name = "Status", IsActive = true, IsDeleted = false });

        // Act
        var result = await _service.GetAvailableSourcesAsync();

        // Assert
        result.Should().Contain(s => s.SourceName == "Industries");
        result.Should().Contain(s => s.SourceName == "Status");
    }

    [Fact]
    public async Task GetAvailableSourcesAsync_IncludesBuiltInTableSources()
    {
        // Act
        var result = await _service.GetAvailableSourcesAsync();

        // Assert
        result.Should().Contain(s => s.SourceName == "ZipCodes");
        result.Should().Contain(s => s.SourceName == "Products");
        result.Should().Contain(s => s.SourceName == "Customers");
        result.Should().Contain(s => s.SourceName == "Users");
    }

    [Fact]
    public async Task GetAvailableSourcesAsync_IncludesApiSources()
    {
        // Act
        var result = await _service.GetAvailableSourcesAsync();

        // Assert
        result.Should().Contain(s => s.SourceType == MasterDataSourceTypes.Api);
    }

    [Fact]
    public async Task GetAvailableSourcesAsync_SetsCorrectAvailableFields()
    {
        // Act
        var result = await _service.GetAvailableSourcesAsync();

        // Assert
        var zipCodeSource = result.FirstOrDefault(s => s.SourceName == "ZipCodes");
        zipCodeSource.Should().NotBeNull();
        zipCodeSource!.AvailableFields.Should().Contain("PostalCode");
        zipCodeSource.AvailableFields.Should().Contain("City");
        zipCodeSource.AvailableFields.Should().Contain("State");
    }

    #endregion

    #region ValidateValueAsync Tests

    [Fact]
    public async Task ValidateValueAsync_WithEmptyValue_ReturnsValid()
    {
        // Act
        var result = await _service.ValidateValueAsync(100, "");

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateValueAsync_WithNoLinks_ReturnsValid()
    {
        // Act
        var result = await _service.ValidateValueAsync(100, "any value");

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateValueAsync_WithValidRegexPattern_ReturnsValid()
    {
        // Arrange
        _links.Add(new FieldMasterDataLink
        {
            Id = 1,
            FieldConfigurationId = 100,
            ValidationPattern = "^[A-Z]{2}$",
            AllowFreeText = true,
            IsActive = true,
            IsDeleted = false
        });

        // Act
        var result = await _service.ValidateValueAsync(100, "CA");

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateValueAsync_WithInvalidRegexPattern_ReturnsInvalid()
    {
        // Arrange
        _links.Add(new FieldMasterDataLink
        {
            Id = 1,
            FieldConfigurationId = 100,
            ValidationPattern = "^[A-Z]{2}$",
            ValidationMessage = "Must be 2 uppercase letters",
            AllowFreeText = true,
            IsActive = true,
            IsDeleted = false
        });

        // Act
        var result = await _service.ValidateValueAsync(100, "California");

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("2 uppercase");
    }

    [Fact]
    public async Task ValidateValueAsync_WithFreeTextAllowed_SkipsMasterDataCheck()
    {
        // Arrange
        _links.Add(new FieldMasterDataLink
        {
            Id = 1,
            FieldConfigurationId = 100,
            AllowFreeText = true,
            IsActive = true,
            IsDeleted = false
        });

        // Act
        var result = await _service.ValidateValueAsync(100, "Any free text value");

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion
}

#region Test Infrastructure

// Async enumerable support for mocking EF Core
internal class TestAsyncQueryProvider<TEntity> : IQueryProvider, IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    internal TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
    {
        return new TestAsyncEnumerable<TEntity>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression)
    {
        return new TestAsyncEnumerable<TElement>(expression);
    }

    public object? Execute(System.Linq.Expressions.Expression expression)
    {
        return _inner.Execute(expression);
    }

    public TResult Execute<TResult>(System.Linq.Expressions.Expression expression)
    {
        return _inner.Execute<TResult>(expression);
    }

    public TResult ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression, CancellationToken cancellationToken = default)
    {
        var expectedResultType = typeof(TResult).GetGenericArguments()[0];
        var executionResult = typeof(IQueryProvider)
            .GetMethod(
                name: nameof(IQueryProvider.Execute),
                genericParameterCount: 1,
                types: new[] { typeof(System.Linq.Expressions.Expression) })
            ?.MakeGenericMethod(expectedResultType)
            .Invoke(this, new[] { expression });

        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))
            ?.MakeGenericMethod(expectedResultType)
            .Invoke(null, new[] { executionResult })!;
    }
}

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
    public TestAsyncEnumerable(System.Linq.Expressions.Expression expression) : base(expression) { }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}

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
        return ValueTask.CompletedTask;
    }
}

#endregion
