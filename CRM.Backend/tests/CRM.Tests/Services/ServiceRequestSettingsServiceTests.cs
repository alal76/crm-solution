// CRM Solution - ServiceRequestSettingsService Tests
// Tests for Service Request Category, Subcategory, Custom Field, and Type services

using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for ServiceRequestCategoryService
/// </summary>
public class ServiceRequestCategoryServiceTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<ServiceRequestCategoryService>> _mockLogger;

    public ServiceRequestCategoryServiceTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<ServiceRequestCategoryService>>();
    }

    private ServiceRequestCategoryService CreateService() =>
        new(_mockContext.Object, _mockLogger.Object);

    [Fact]
    public async Task GetAllCategoriesAsync_ShouldReturnActiveCategories_ByDefault()
    {
        // Arrange
        var categories = new List<ServiceRequestCategory>
        {
            new() { Id = 1, Name = "Hardware", IsActive = true, IsDeleted = false, DisplayOrder = 1 },
            new() { Id = 2, Name = "Software", IsActive = true, IsDeleted = false, DisplayOrder = 2 },
            new() { Id = 3, Name = "Inactive", IsActive = false, IsDeleted = false, DisplayOrder = 3 },
            new() { Id = 4, Name = "Deleted", IsActive = true, IsDeleted = true, DisplayOrder = 4 }
        };

        var mockSet = CreateMockDbSet(categories);
        _mockContext.Setup(x => x.ServiceRequestCategories).Returns(mockSet.Object);

        var service = CreateService();

        // Act
        var result = await service.GetAllCategoriesAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(c => c.IsActive && c.Name != "Inactive" && c.Name != "Deleted");
    }

    [Fact]
    public async Task GetAllCategoriesAsync_ShouldIncludeInactive_WhenRequested()
    {
        // Arrange
        var categories = new List<ServiceRequestCategory>
        {
            new() { Id = 1, Name = "Active", IsActive = true, IsDeleted = false },
            new() { Id = 2, Name = "Inactive", IsActive = false, IsDeleted = false }
        };

        var mockSet = CreateMockDbSet(categories);
        _mockContext.Setup(x => x.ServiceRequestCategories).Returns(mockSet.Object);

        var service = CreateService();

        // Act
        var result = await service.GetAllCategoriesAsync(includeInactive: true);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllCategoriesAsync_ShouldOrderByDisplayOrder()
    {
        // Arrange
        var categories = new List<ServiceRequestCategory>
        {
            new() { Id = 1, Name = "Third", IsActive = true, IsDeleted = false, DisplayOrder = 3 },
            new() { Id = 2, Name = "First", IsActive = true, IsDeleted = false, DisplayOrder = 1 },
            new() { Id = 3, Name = "Second", IsActive = true, IsDeleted = false, DisplayOrder = 2 }
        };

        var mockSet = CreateMockDbSet(categories);
        _mockContext.Setup(x => x.ServiceRequestCategories).Returns(mockSet.Object);

        var service = CreateService();

        // Act
        var result = await service.GetAllCategoriesAsync();

        // Assert
        result[0].Name.Should().Be("First");
        result[1].Name.Should().Be("Second");
        result[2].Name.Should().Be("Third");
    }

    [Fact]
    public async Task GetCategoryByIdAsync_ShouldReturnCategory_WhenExists()
    {
        // Arrange
        var categories = new List<ServiceRequestCategory>
        {
            new() { Id = 1, Name = "Hardware", IsActive = true, IsDeleted = false, Description = "Hardware issues" }
        };

        var mockSet = CreateMockDbSet(categories);
        _mockContext.Setup(x => x.ServiceRequestCategories).Returns(mockSet.Object);

        var service = CreateService();

        // Act
        var result = await service.GetCategoryByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Hardware");
        result.Description.Should().Be("Hardware issues");
    }

    [Fact]
    public async Task GetCategoryByIdAsync_ShouldReturnNull_WhenDeleted()
    {
        // Arrange
        var categories = new List<ServiceRequestCategory>
        {
            new() { Id = 1, Name = "Deleted", IsActive = true, IsDeleted = true }
        };

        var mockSet = CreateMockDbSet(categories);
        _mockContext.Setup(x => x.ServiceRequestCategories).Returns(mockSet.Object);

        var service = CreateService();

        // Act
        var result = await service.GetCategoryByIdAsync(1);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateCategoryAsync_ShouldCreateAndReturn()
    {
        // Arrange
        var categories = new List<ServiceRequestCategory>();
        var mockSet = CreateMockDbSet(categories);
        
        ServiceRequestCategory? addedCategory = null;
        mockSet.Setup(x => x.Add(It.IsAny<ServiceRequestCategory>()))
            .Callback<ServiceRequestCategory>(c => 
            {
                c.Id = 1;
                addedCategory = c;
                categories.Add(c);
            });

        _mockContext.Setup(x => x.ServiceRequestCategories).Returns(mockSet.Object);
        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        var service = CreateService();
        var dto = new CreateServiceRequestCategoryDto
        {
            Name = "New Category",
            Description = "Test description",
            DisplayOrder = 1,
            IsActive = true,
            IconName = "folder",
            ColorCode = "#FF0000"
        };

        // Act
        var result = await service.CreateCategoryAsync(dto);

        // Assert
        addedCategory.Should().NotBeNull();
        addedCategory!.Name.Should().Be("New Category");
        addedCategory.Description.Should().Be("Test description");
        addedCategory.IconName.Should().Be("folder");
        addedCategory.ColorCode.Should().Be("#FF0000");
    }

    [Fact]
    public async Task DeleteCategoryAsync_ShouldSoftDelete()
    {
        // Arrange
        var category = new ServiceRequestCategory { Id = 1, Name = "ToDelete", IsDeleted = false };
        var categories = new List<ServiceRequestCategory> { category };
        var mockSet = CreateMockDbSet(categories);
        
        mockSet.Setup(x => x.FindAsync(1)).ReturnsAsync(category);
        _mockContext.Setup(x => x.ServiceRequestCategories).Returns(mockSet.Object);
        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(1));

        var service = CreateService();

        // Act
        var result = await service.DeleteCategoryAsync(1);

        // Assert
        result.Should().BeTrue();
        category.IsDeleted.Should().BeTrue();
        category.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteCategoryAsync_ShouldReturnFalse_WhenNotFound()
    {
        // Arrange
        var categories = new List<ServiceRequestCategory>();
        var mockSet = CreateMockDbSet(categories);
        mockSet.Setup(x => x.FindAsync(999)).ReturnsAsync((ServiceRequestCategory?)null);
        _mockContext.Setup(x => x.ServiceRequestCategories).Returns(mockSet.Object);

        var service = CreateService();

        // Act
        var result = await service.DeleteCategoryAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ReorderCategoriesAsync_ShouldUpdateDisplayOrder()
    {
        // Arrange
        var cat1 = new ServiceRequestCategory { Id = 1, DisplayOrder = 0 };
        var cat2 = new ServiceRequestCategory { Id = 2, DisplayOrder = 1 };
        var cat3 = new ServiceRequestCategory { Id = 3, DisplayOrder = 2 };

        var mockSet = CreateMockDbSet(new List<ServiceRequestCategory> { cat1, cat2, cat3 });
        mockSet.Setup(x => x.FindAsync(1)).ReturnsAsync(cat1);
        mockSet.Setup(x => x.FindAsync(2)).ReturnsAsync(cat2);
        mockSet.Setup(x => x.FindAsync(3)).ReturnsAsync(cat3);
        
        _mockContext.Setup(x => x.ServiceRequestCategories).Returns(mockSet.Object);
        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(1));

        var service = CreateService();

        // Act - Reverse order
        var result = await service.ReorderCategoriesAsync(new List<int> { 3, 2, 1 });

        // Assert
        result.Should().BeTrue();
        cat3.DisplayOrder.Should().Be(0);
        cat2.DisplayOrder.Should().Be(1);
        cat1.DisplayOrder.Should().Be(2);
    }

    private static Mock<DbSet<T>> CreateMockDbSet<T>(List<T> data) where T : class
    {
        var queryable = data.AsQueryable();
        var mockSet = new Mock<DbSet<T>>();
        
        mockSet.As<IAsyncEnumerable<T>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(data.GetEnumerator()));
        
        mockSet.As<IQueryable<T>>().Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
        
        return mockSet;
    }
}

/// <summary>
/// Unit tests for ServiceRequestSubcategoryService
/// </summary>
public class ServiceRequestSubcategoryServiceTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<ServiceRequestSubcategoryService>> _mockLogger;

    public ServiceRequestSubcategoryServiceTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<ServiceRequestSubcategoryService>>();
    }

    private ServiceRequestSubcategoryService CreateService() =>
        new(_mockContext.Object, _mockLogger.Object);

    [Fact]
    public async Task GetSubcategoriesByCategoryAsync_ShouldFilterByCategory()
    {
        // Arrange
        var category1 = new ServiceRequestCategory { Id = 1, Name = "Hardware" };
        var category2 = new ServiceRequestCategory { Id = 2, Name = "Software" };
        
        var subcategories = new List<ServiceRequestSubcategory>
        {
            new() { Id = 1, Name = "Printers", CategoryId = 1, Category = category1, IsActive = true, IsDeleted = false },
            new() { Id = 2, Name = "Laptops", CategoryId = 1, Category = category1, IsActive = true, IsDeleted = false },
            new() { Id = 3, Name = "Office", CategoryId = 2, Category = category2, IsActive = true, IsDeleted = false }
        };

        var mockSet = CreateMockDbSet(subcategories);
        _mockContext.Setup(x => x.ServiceRequestSubcategories).Returns(mockSet.Object);

        var service = CreateService();

        // Act
        var result = await service.GetSubcategoriesByCategoryAsync(1);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(s => s.CategoryId == 1);
    }

    [Fact]
    public async Task CreateSubcategoryAsync_ShouldSetDefaultPriority()
    {
        // Arrange
        var subcategories = new List<ServiceRequestSubcategory>();
        var mockSet = CreateMockDbSet(subcategories);
        
        ServiceRequestSubcategory? added = null;
        mockSet.Setup(x => x.Add(It.IsAny<ServiceRequestSubcategory>()))
            .Callback<ServiceRequestSubcategory>(s => 
            {
                s.Id = 1;
                added = s;
                subcategories.Add(s);
            });

        _mockContext.Setup(x => x.ServiceRequestSubcategories).Returns(mockSet.Object);
        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(1));

        var service = CreateService();
        var dto = new CreateServiceRequestSubcategoryDto
        {
            Name = "Test Subcategory",
            CategoryId = 1,
            DefaultPriority = "High",
            ResponseTimeHours = 4,
            ResolutionTimeHours = 24
        };

        // Act
        var result = await service.CreateSubcategoryAsync(dto);

        // Assert
        added.Should().NotBeNull();
        added!.DefaultPriority.Should().Be("High");
        added.ResponseTimeHours.Should().Be(4);
        added.ResolutionTimeHours.Should().Be(24);
    }

    private static Mock<DbSet<T>> CreateMockDbSet<T>(List<T> data) where T : class
    {
        var queryable = data.AsQueryable();
        var mockSet = new Mock<DbSet<T>>();
        
        mockSet.As<IAsyncEnumerable<T>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(data.GetEnumerator()));
        
        mockSet.As<IQueryable<T>>().Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
        
        return mockSet;
    }
}

/// <summary>
/// Unit tests for ServiceRequestCustomFieldService
/// </summary>
public class ServiceRequestCustomFieldServiceTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<ServiceRequestCustomFieldService>> _mockLogger;

    public ServiceRequestCustomFieldServiceTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<ServiceRequestCustomFieldService>>();
    }

    private ServiceRequestCustomFieldService CreateService() =>
        new(_mockContext.Object, _mockLogger.Object);

    [Fact]
    public async Task CreateFieldDefinitionAsync_ShouldEnforceMaxFieldLimit()
    {
        // Arrange
        var fields = Enumerable.Range(1, 15)
            .Select(i => new ServiceRequestCustomFieldDefinition { Id = i, IsActive = true, IsDeleted = false })
            .ToList();

        var mockSet = CreateMockDbSet(fields);
        _mockContext.Setup(x => x.ServiceRequestCustomFieldDefinitions).Returns(mockSet.Object);

        var service = CreateService();
        var dto = new CreateServiceRequestCustomFieldDefinitionDto
        {
            Name = "New Field",
            FieldKey = "new_field",
            FieldType = "text",
            IsActive = true
        };

        // Act & Assert
        await service.Invoking(s => s.CreateFieldDefinitionAsync(dto))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Maximum of 15 custom fields*");
    }

    [Fact]
    public async Task GetFieldDefinitionsByCategoryAsync_ShouldReturnGlobalAndCategoryFields()
    {
        // Arrange
        var fields = new List<ServiceRequestCustomFieldDefinition>
        {
            new() { Id = 1, Name = "Global", CategoryId = null, SubcategoryId = null, IsActive = true, IsDeleted = false },
            new() { Id = 2, Name = "Category1", CategoryId = 1, SubcategoryId = null, IsActive = true, IsDeleted = false },
            new() { Id = 3, Name = "Category2", CategoryId = 2, SubcategoryId = null, IsActive = true, IsDeleted = false },
            new() { Id = 4, Name = "Subcategory", SubcategoryId = 10, IsActive = true, IsDeleted = false }
        };

        var mockSet = CreateMockDbSet(fields);
        _mockContext.Setup(x => x.ServiceRequestCustomFieldDefinitions).Returns(mockSet.Object);

        var service = CreateService();

        // Act
        var result = await service.GetFieldDefinitionsByCategoryAsync(categoryId: 1, subcategoryId: null);

        // Assert
        result.Should().HaveCount(2); // Global + Category1
        result.Should().Contain(f => f.Name == "Global");
        result.Should().Contain(f => f.Name == "Category1");
    }

    [Fact]
    public async Task GetActiveFieldCountAsync_ShouldCountOnlyActiveNonDeleted()
    {
        // Arrange
        var fields = new List<ServiceRequestCustomFieldDefinition>
        {
            new() { Id = 1, IsActive = true, IsDeleted = false },
            new() { Id = 2, IsActive = true, IsDeleted = false },
            new() { Id = 3, IsActive = false, IsDeleted = false },
            new() { Id = 4, IsActive = true, IsDeleted = true }
        };

        var mockSet = CreateMockDbSet(fields);
        _mockContext.Setup(x => x.ServiceRequestCustomFieldDefinitions).Returns(mockSet.Object);

        var service = CreateService();

        // Act
        var result = await service.GetActiveFieldCountAsync();

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async Task UpdateFieldDefinitionAsync_ShouldSerializeDropdownOptions()
    {
        // Arrange
        var field = new ServiceRequestCustomFieldDefinition 
        { 
            Id = 1, 
            Name = "Status", 
            FieldKey = "status",
            FieldType = "dropdown" 
        };

        var mockSet = CreateMockDbSet(new List<ServiceRequestCustomFieldDefinition> { field });
        mockSet.Setup(x => x.FindAsync(1)).ReturnsAsync(field);
        _mockContext.Setup(x => x.ServiceRequestCustomFieldDefinitions).Returns(mockSet.Object);
        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(1));

        var service = CreateService();
        var dto = new UpdateServiceRequestCustomFieldDefinitionDto
        {
            Name = "Status",
            FieldKey = "status",
            FieldType = "dropdown",
            DropdownOptions = new List<string> { "Open", "In Progress", "Closed" },
            IsActive = true
        };

        // Act
        await service.UpdateFieldDefinitionAsync(1, dto);

        // Assert
        field.DropdownOptions.Should().NotBeNullOrEmpty();
        field.DropdownOptions.Should().Contain("Open");
        field.DropdownOptions.Should().Contain("Closed");
    }

    private static Mock<DbSet<T>> CreateMockDbSet<T>(List<T> data) where T : class
    {
        var queryable = data.AsQueryable();
        var mockSet = new Mock<DbSet<T>>();
        
        mockSet.As<IAsyncEnumerable<T>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(data.GetEnumerator()));
        
        mockSet.As<IQueryable<T>>().Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
        
        return mockSet;
    }
}

/// <summary>
/// Unit tests for ServiceRequestTypeService
/// </summary>
public class ServiceRequestTypeServiceTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<ServiceRequestTypeService>> _mockLogger;

    public ServiceRequestTypeServiceTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<ServiceRequestTypeService>>();
    }

    private ServiceRequestTypeService CreateService() =>
        new(_mockContext.Object, _mockLogger.Object);

    [Fact]
    public async Task GetTypesGroupedAsync_ShouldGroupByCategoryAndSubcategory()
    {
        // Arrange
        var cat1 = new ServiceRequestCategory { Id = 1, Name = "Hardware" };
        var cat2 = new ServiceRequestCategory { Id = 2, Name = "Software" };
        var sub1 = new ServiceRequestSubcategory { Id = 1, Name = "Printers" };
        var sub2 = new ServiceRequestSubcategory { Id = 2, Name = "Laptops" };

        var types = new List<ServiceRequestType>
        {
            new() { Id = 1, Name = "Printer Issue", CategoryId = 1, Category = cat1, SubcategoryId = 1, Subcategory = sub1, IsActive = true, IsDeleted = false },
            new() { Id = 2, Name = "Printer Setup", CategoryId = 1, Category = cat1, SubcategoryId = 1, Subcategory = sub1, IsActive = true, IsDeleted = false },
            new() { Id = 3, Name = "Laptop Repair", CategoryId = 1, Category = cat1, SubcategoryId = 2, Subcategory = sub2, IsActive = true, IsDeleted = false },
            new() { Id = 4, Name = "Software Install", CategoryId = 2, Category = cat2, SubcategoryId = null, IsActive = true, IsDeleted = false }
        };

        var mockSet = CreateMockDbSet(types);
        _mockContext.Setup(x => x.ServiceRequestTypes).Returns(mockSet.Object);

        var service = CreateService();

        // Act
        var result = await service.GetTypesGroupedAsync();

        // Assert
        result.Should().HaveCount(2); // 2 categories
        var hwCategory = result.First(c => c.CategoryName == "Hardware");
        hwCategory.Subcategories.Should().HaveCount(2); // Printers and Laptops
    }

    [Fact]
    public async Task GetTypesBySubcategoryAsync_ShouldFilterBySubcategory()
    {
        // Arrange
        var types = new List<ServiceRequestType>
        {
            new() { Id = 1, Name = "Type1", SubcategoryId = 1, IsActive = true, IsDeleted = false },
            new() { Id = 2, Name = "Type2", SubcategoryId = 1, IsActive = true, IsDeleted = false },
            new() { Id = 3, Name = "Type3", SubcategoryId = 2, IsActive = true, IsDeleted = false }
        };

        var mockSet = CreateMockDbSet(types);
        _mockContext.Setup(x => x.ServiceRequestTypes).Returns(mockSet.Object);

        var service = CreateService();

        // Act
        var result = await service.GetTypesBySubcategoryAsync(1);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(t => t.SubcategoryId == 1);
    }

    [Fact]
    public async Task CreateTypeAsync_ShouldSetWorkflowAndResolutions()
    {
        // Arrange
        var types = new List<ServiceRequestType>();
        var mockSet = CreateMockDbSet(types);
        
        ServiceRequestType? added = null;
        mockSet.Setup(x => x.Add(It.IsAny<ServiceRequestType>()))
            .Callback<ServiceRequestType>(t => 
            {
                t.Id = 1;
                added = t;
                types.Add(t);
            });

        _mockContext.Setup(x => x.ServiceRequestTypes).Returns(mockSet.Object);
        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(1));

        var service = CreateService();
        var dto = new CreateServiceRequestTypeDto
        {
            Name = "Password Reset",
            RequestType = "Service Request",
            CategoryId = 1,
            SubcategoryId = 1,
            WorkflowName = "PasswordResetWorkflow",
            PossibleResolutions = "Reset via self-service;Admin reset;Account unlock",
            FinalCustomerResolutions = "Password reset successful;Account unlocked"
        };

        // Act
        var result = await service.CreateTypeAsync(dto);

        // Assert
        added.Should().NotBeNull();
        added!.WorkflowName.Should().Be("PasswordResetWorkflow");
        added.PossibleResolutions.Should().Contain("self-service");
        added.FinalCustomerResolutions.Should().Contain("Password reset successful");
    }

    [Fact]
    public async Task ReorderTypesAsync_ShouldOnlyReorderWithinSubcategory()
    {
        // Arrange
        var type1 = new ServiceRequestType { Id = 1, SubcategoryId = 1, DisplayOrder = 0 };
        var type2 = new ServiceRequestType { Id = 2, SubcategoryId = 1, DisplayOrder = 1 };
        var type3 = new ServiceRequestType { Id = 3, SubcategoryId = 2, DisplayOrder = 0 }; // Different subcategory

        var mockSet = CreateMockDbSet(new List<ServiceRequestType> { type1, type2, type3 });
        _mockContext.Setup(x => x.ServiceRequestTypes).Returns(mockSet.Object);
        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(1));

        var service = CreateService();

        // Act - Try to reorder including type3 which is in different subcategory
        var result = await service.ReorderTypesAsync(1, new List<int> { 2, 1, 3 });

        // Assert
        result.Should().BeTrue();
        type2.DisplayOrder.Should().Be(0);
        type1.DisplayOrder.Should().Be(1);
        type3.DisplayOrder.Should().Be(0); // Unchanged - different subcategory
    }

    private static Mock<DbSet<T>> CreateMockDbSet<T>(List<T> data) where T : class
    {
        var queryable = data.AsQueryable();
        var mockSet = new Mock<DbSet<T>>();
        
        mockSet.As<IAsyncEnumerable<T>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(data.GetEnumerator()));
        
        mockSet.As<IQueryable<T>>().Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
        
        return mockSet;
    }
}

#region Test Infrastructure

/// <summary>
/// Async query provider for mocking EF Core async operations
/// </summary>
internal class TestAsyncQueryProvider<T> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    public TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;

    public IQueryable CreateQuery(Expression expression) => 
        new TestAsyncEnumerable<T>(expression);

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression) =>
        new TestAsyncEnumerable<TElement>(expression);

    public object? Execute(Expression expression) => _inner.Execute(expression);

    public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        var resultType = typeof(TResult).GetGenericArguments()[0];
        var executeMethod = typeof(IQueryProvider)
            .GetMethods()
            .First(m => m.Name == "Execute" && m.IsGenericMethod)
            .MakeGenericMethod(resultType);
        
        var result = executeMethod.Invoke(_inner, new object[] { expression });
        return (TResult)typeof(Task).GetMethod("FromResult")!
            .MakeGenericMethod(resultType)
            .Invoke(null, new[] { result })!;
    }
}

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
    public TestAsyncEnumerable(Expression expression) : base(expression) { }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
        new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}

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

#endregion
