// This file is part of the CRM Solution.
// Tests for ServiceCatalogService - ITSM catalog management

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRM.Core.DTOs.ITSM;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services.ITSM;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace CRM.Tests.Services.ITSM;

public class ServiceCatalogServiceTests
{
    private readonly Mock<IDbContextResolver> _mockContextResolver;
    private readonly Mock<ILogger<ServiceCatalogService>> _mockLogger;
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly ServiceCatalogService _service;

    public ServiceCatalogServiceTests()
    {
        _mockContextResolver = new Mock<IDbContextResolver>();
        _mockLogger = new Mock<ILogger<ServiceCatalogService>>();
        _mockContext = new Mock<ICrmDbContext>();
        
        _mockContextResolver.Setup(x => x.ResolveContext()).Returns(_mockContext.Object);
        
        _service = new ServiceCatalogService(
            _mockContextResolver.Object,
            _mockLogger.Object);
    }

    #region GetCatalogItemsAsync Tests

    [Fact]
    public async Task GetCatalogItemsAsync_WithNoFilters_ReturnsAllActiveItems()
    {
        // Arrange
        var category = new CatalogCategory { CategoryId = 1, Name = "Hardware", IsActive = true };
        var items = new List<CatalogItem>
        {
            new CatalogItem { CatalogItemId = 1, Name = "Laptop", IsActive = true, Category = category, DisplayOrder = 1 },
            new CatalogItem { CatalogItemId = 2, Name = "Desktop", IsActive = true, Category = category, DisplayOrder = 2 },
            new CatalogItem { CatalogItemId = 3, Name = "Deleted Item", IsActive = true, IsDeleted = true, Category = category }
        };

        var mockSet = CreateMockDbSet(items);
        _mockContext.Setup(c => c.CatalogItems).Returns(mockSet.Object);

        // Act
        var result = await _service.GetCatalogItemsAsync(null, null);

        // Assert
        result.Should().HaveCount(2);
        result.Select(r => r.Name).Should().Contain("Laptop", "Desktop");
    }

    [Fact]
    public async Task GetCatalogItemsAsync_WithCategoryFilter_ReturnsOnlyMatchingCategory()
    {
        // Arrange
        var hardware = new CatalogCategory { CategoryId = 1, Name = "Hardware", IsActive = true };
        var software = new CatalogCategory { CategoryId = 2, Name = "Software", IsActive = true };
        var items = new List<CatalogItem>
        {
            new CatalogItem { CatalogItemId = 1, Name = "Laptop", IsActive = true, CategoryId = 1, Category = hardware },
            new CatalogItem { CatalogItemId = 2, Name = "Office Suite", IsActive = true, CategoryId = 2, Category = software }
        };

        var mockSet = CreateMockDbSet(items);
        _mockContext.Setup(c => c.CatalogItems).Returns(mockSet.Object);

        // Act
        var result = await _service.GetCatalogItemsAsync(categoryId: 1, null);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Laptop");
    }

    [Fact]
    public async Task GetCatalogItemsAsync_WithFeaturedOnly_ReturnsOnlyFeatured()
    {
        // Arrange
        var category = new CatalogCategory { CategoryId = 1, Name = "Hardware", IsActive = true };
        var items = new List<CatalogItem>
        {
            new CatalogItem { CatalogItemId = 1, Name = "Featured Laptop", IsActive = true, IsFeatured = true, Category = category },
            new CatalogItem { CatalogItemId = 2, Name = "Regular Desktop", IsActive = true, IsFeatured = false, Category = category }
        };

        var mockSet = CreateMockDbSet(items);
        _mockContext.Setup(c => c.CatalogItems).Returns(mockSet.Object);

        // Act
        var result = await _service.GetCatalogItemsAsync(null, featuredOnly: true);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Featured Laptop");
    }

    [Fact]
    public async Task GetCatalogItemsAsync_ExcludesInactiveItems()
    {
        // Arrange
        var category = new CatalogCategory { CategoryId = 1, Name = "Hardware", IsActive = true };
        var items = new List<CatalogItem>
        {
            new CatalogItem { CatalogItemId = 1, Name = "Active Laptop", IsActive = true, Category = category },
            new CatalogItem { CatalogItemId = 2, Name = "Inactive Desktop", IsActive = false, Category = category }
        };

        var mockSet = CreateMockDbSet(items);
        _mockContext.Setup(c => c.CatalogItems).Returns(mockSet.Object);

        // Act
        var result = await _service.GetCatalogItemsAsync(null, null);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Active Laptop");
    }

    [Fact]
    public async Task GetCatalogItemsAsync_OrdersByDisplayOrderThenName()
    {
        // Arrange
        var category = new CatalogCategory { CategoryId = 1, Name = "Hardware", IsActive = true };
        var items = new List<CatalogItem>
        {
            new CatalogItem { CatalogItemId = 1, Name = "Zebra", IsActive = true, DisplayOrder = 2, Category = category },
            new CatalogItem { CatalogItemId = 2, Name = "Alpha", IsActive = true, DisplayOrder = 2, Category = category },
            new CatalogItem { CatalogItemId = 3, Name = "First", IsActive = true, DisplayOrder = 1, Category = category }
        };

        var mockSet = CreateMockDbSet(items);
        _mockContext.Setup(c => c.CatalogItems).Returns(mockSet.Object);

        // Act
        var result = (await _service.GetCatalogItemsAsync(null, null)).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].Name.Should().Be("First");
        result[1].Name.Should().Be("Alpha");
        result[2].Name.Should().Be("Zebra");
    }

    #endregion

    #region GetCatalogItemByIdAsync Tests

    [Fact]
    public async Task GetCatalogItemByIdAsync_WhenExists_ReturnsItem()
    {
        // Arrange
        var category = new CatalogCategory { CategoryId = 1, Name = "Hardware" };
        var items = new List<CatalogItem>
        {
            new CatalogItem 
            { 
                CatalogItemId = 1, 
                Name = "Laptop", 
                ShortDescription = "Business laptop",
                Price = 1200.00m,
                IsActive = true, 
                IsFeatured = true,
                CategoryId = 1,
                Category = category
            }
        };

        var mockSet = CreateMockDbSet(items);
        _mockContext.Setup(c => c.CatalogItems).Returns(mockSet.Object);

        // Act
        var result = await _service.GetCatalogItemByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Laptop");
        result.ShortDescription.Should().Be("Business laptop");
        result.Price.Should().Be(1200.00m);
        result.CategoryName.Should().Be("Hardware");
    }

    [Fact]
    public async Task GetCatalogItemByIdAsync_WhenDeleted_ReturnsNull()
    {
        // Arrange
        var items = new List<CatalogItem>
        {
            new CatalogItem { CatalogItemId = 1, Name = "Deleted Item", IsDeleted = true }
        };

        var mockSet = CreateMockDbSet(items);
        _mockContext.Setup(c => c.CatalogItems).Returns(mockSet.Object);

        // Act
        var result = await _service.GetCatalogItemByIdAsync(1);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCatalogItemByIdAsync_WhenNotFound_ReturnsNull()
    {
        // Arrange
        var items = new List<CatalogItem>();
        var mockSet = CreateMockDbSet(items);
        _mockContext.Setup(c => c.CatalogItems).Returns(mockSet.Object);

        // Act
        var result = await _service.GetCatalogItemByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CreateCatalogRequestAsync Tests

    [Fact]
    public async Task CreateCatalogRequestAsync_CreatesRequestWithCorrectData()
    {
        // Arrange
        var dto = new CreateCatalogRequestDto
        {
            CatalogItemId = 1,
            RequestedForId = 100,
            VariableValues = new Dictionary<string, string> { { "location", "HQ" } }
        };
        var requests = new List<CatalogRequest>();
        var mockSet = CreateMockDbSet(requests);
        _mockContext.Setup(c => c.CatalogRequests).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await _service.CreateCatalogRequestAsync(dto, requestedById: 50);

        // Assert
        mockSet.Verify(m => m.Add(It.Is<CatalogRequest>(r => 
            r.CatalogItemId == 1 &&
            r.RequestedById == 50 &&
            r.RequestedForId == 100 &&
            r.State == CatalogRequestState.Requested)), Times.Once);
    }

    [Fact]
    public async Task CreateCatalogRequestAsync_SerializesVariableValues()
    {
        // Arrange
        var dto = new CreateCatalogRequestDto
        {
            CatalogItemId = 1,
            VariableValues = new Dictionary<string, string> 
            { 
                { "memory", "16GB" },
                { "storage", "512GB SSD" }
            }
        };
        var requests = new List<CatalogRequest>();
        var mockSet = CreateMockDbSet(requests);
        CatalogRequest? capturedRequest = null;
        mockSet.Setup(m => m.Add(It.IsAny<CatalogRequest>()))
            .Callback<CatalogRequest>(r => capturedRequest = r);
        _mockContext.Setup(c => c.CatalogRequests).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        await _service.CreateCatalogRequestAsync(dto, requestedById: 1);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.VariableValues.Should().Contain("memory");
        capturedRequest.VariableValues.Should().Contain("512GB SSD");
    }

    [Fact]
    public async Task CreateCatalogRequestAsync_LogsCreation()
    {
        // Arrange
        var dto = new CreateCatalogRequestDto { CatalogItemId = 5 };
        var requests = new List<CatalogRequest>();
        var mockSet = CreateMockDbSet(requests);
        _mockContext.Setup(c => c.CatalogRequests).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        await _service.CreateCatalogRequestAsync(dto, requestedById: 1);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Created catalog request")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region SearchCatalogAsync Tests

    [Fact]
    public async Task SearchCatalogAsync_SearchesByName()
    {
        // Arrange
        var category = new CatalogCategory { CategoryId = 1, Name = "Hardware" };
        var items = new List<CatalogItem>
        {
            new CatalogItem { CatalogItemId = 1, Name = "Dell Laptop", IsActive = true, Category = category },
            new CatalogItem { CatalogItemId = 2, Name = "HP Desktop", IsActive = true, Category = category },
            new CatalogItem { CatalogItemId = 3, Name = "Lenovo Laptop", IsActive = true, Category = category }
        };

        var mockSet = CreateMockDbSet(items);
        _mockContext.Setup(c => c.CatalogItems).Returns(mockSet.Object);

        // Act
        var result = await _service.SearchCatalogAsync("Laptop");

        // Assert
        result.Should().HaveCount(2);
        result.Select(r => r.Name).Should().Contain("Dell Laptop", "Lenovo Laptop");
    }

    [Fact]
    public async Task SearchCatalogAsync_SearchesByDescription()
    {
        // Arrange
        var category = new CatalogCategory { CategoryId = 1, Name = "Hardware" };
        var items = new List<CatalogItem>
        {
            new CatalogItem 
            { 
                CatalogItemId = 1, 
                Name = "Office PC", 
                ShortDescription = "Standard business laptop configuration",
                IsActive = true, 
                Category = category 
            },
            new CatalogItem 
            { 
                CatalogItemId = 2, 
                Name = "Workstation", 
                ShortDescription = "High-performance desktop for development",
                IsActive = true, 
                Category = category 
            }
        };

        var mockSet = CreateMockDbSet(items);
        _mockContext.Setup(c => c.CatalogItems).Returns(mockSet.Object);

        // Act
        var result = await _service.SearchCatalogAsync("laptop");

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Office PC");
    }

    [Fact]
    public async Task SearchCatalogAsync_ExcludesDeletedAndInactiveItems()
    {
        // Arrange
        var category = new CatalogCategory { CategoryId = 1, Name = "Hardware" };
        var items = new List<CatalogItem>
        {
            new CatalogItem { CatalogItemId = 1, Name = "Active Laptop", IsActive = true, Category = category },
            new CatalogItem { CatalogItemId = 2, Name = "Deleted Laptop", IsActive = true, IsDeleted = true, Category = category },
            new CatalogItem { CatalogItemId = 3, Name = "Inactive Laptop", IsActive = false, Category = category }
        };

        var mockSet = CreateMockDbSet(items);
        _mockContext.Setup(c => c.CatalogItems).Returns(mockSet.Object);

        // Act
        var result = await _service.SearchCatalogAsync("Laptop");

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Active Laptop");
    }

    [Fact]
    public async Task SearchCatalogAsync_LimitsTo20Results()
    {
        // Arrange
        var category = new CatalogCategory { CategoryId = 1, Name = "Hardware" };
        var items = Enumerable.Range(1, 30)
            .Select(i => new CatalogItem 
            { 
                CatalogItemId = i, 
                Name = $"Laptop {i}", 
                IsActive = true, 
                Category = category 
            })
            .ToList();

        var mockSet = CreateMockDbSet(items);
        _mockContext.Setup(c => c.CatalogItems).Returns(mockSet.Object);

        // Act
        var result = await _service.SearchCatalogAsync("Laptop");

        // Assert
        result.Should().HaveCount(20);
    }

    #endregion

    #region GetMyRequestsAsync Tests

    [Fact]
    public async Task GetMyRequestsAsync_ReturnsUserRequests()
    {
        // Arrange
        var requests = new List<CatalogRequest>
        {
            new CatalogRequest { RequestId = 1, RequestedById = 100, CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new CatalogRequest { RequestId = 2, RequestedById = 100, CreatedAt = DateTime.UtcNow },
            new CatalogRequest { RequestId = 3, RequestedById = 200, CreatedAt = DateTime.UtcNow } // Different user
        };

        var mockSet = CreateMockDbSet(requests);
        _mockContext.Setup(c => c.CatalogRequests).Returns(mockSet.Object);

        // Act
        var result = await _service.GetMyRequestsAsync(100);

        // Assert
        result.Should().HaveCount(2);
        result.All(r => r.RequestedById == 100).Should().BeTrue();
    }

    [Fact]
    public async Task GetMyRequestsAsync_ExcludesDeletedRequests()
    {
        // Arrange
        var requests = new List<CatalogRequest>
        {
            new CatalogRequest { RequestId = 1, RequestedById = 100, IsDeleted = false },
            new CatalogRequest { RequestId = 2, RequestedById = 100, IsDeleted = true }
        };

        var mockSet = CreateMockDbSet(requests);
        _mockContext.Setup(c => c.CatalogRequests).Returns(mockSet.Object);

        // Act
        var result = await _service.GetMyRequestsAsync(100);

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetMyRequestsAsync_OrdersByCreatedAtDescending()
    {
        // Arrange
        var requests = new List<CatalogRequest>
        {
            new CatalogRequest { RequestId = 1, RequestedById = 100, CreatedAt = DateTime.UtcNow.AddDays(-2) },
            new CatalogRequest { RequestId = 2, RequestedById = 100, CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new CatalogRequest { RequestId = 3, RequestedById = 100, CreatedAt = DateTime.UtcNow }
        };

        var mockSet = CreateMockDbSet(requests);
        _mockContext.Setup(c => c.CatalogRequests).Returns(mockSet.Object);

        // Act
        var result = (await _service.GetMyRequestsAsync(100)).ToList();

        // Assert
        result[0].RequestId.Should().Be(3); // Most recent first
        result[2].RequestId.Should().Be(1); // Oldest last
    }

    #endregion

    #region GetCategoriesAsync Tests

    [Fact]
    public async Task GetCategoriesAsync_ReturnsActiveCategories()
    {
        // Arrange
        var categories = new List<CatalogCategory>
        {
            new CatalogCategory 
            { 
                CategoryId = 1, 
                Name = "Hardware", 
                Description = "Physical equipment",
                IconName = "computer",
                IsActive = true,
                CatalogItems = new List<CatalogItem>
                {
                    new CatalogItem { IsActive = true },
                    new CatalogItem { IsActive = true }
                }
            },
            new CatalogCategory 
            { 
                CategoryId = 2, 
                Name = "Software", 
                IsActive = true,
                CatalogItems = new List<CatalogItem>
                {
                    new CatalogItem { IsActive = true }
                }
            },
            new CatalogCategory { CategoryId = 3, Name = "Inactive", IsActive = false }
        };

        var mockSet = CreateMockDbSet(categories);
        _mockContext.Setup(c => c.CatalogCategories).Returns(mockSet.Object);

        // Act
        var result = (await _service.GetCategoriesAsync()).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().NotContain(c => c.Name == "Inactive");
    }

    [Fact]
    public async Task GetCategoriesAsync_ExcludesDeletedCategories()
    {
        // Arrange
        var categories = new List<CatalogCategory>
        {
            new CatalogCategory { CategoryId = 1, Name = "Active", IsActive = true, IsDeleted = false },
            new CatalogCategory { CategoryId = 2, Name = "Deleted", IsActive = true, IsDeleted = true }
        };

        var mockSet = CreateMockDbSet(categories);
        _mockContext.Setup(c => c.CatalogCategories).Returns(mockSet.Object);

        // Act
        var result = await _service.GetCategoriesAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Active");
    }

    [Fact]
    public async Task GetCategoriesAsync_IncludesItemCounts()
    {
        // Arrange
        var categories = new List<CatalogCategory>
        {
            new CatalogCategory 
            { 
                CategoryId = 1, 
                Name = "Hardware", 
                IsActive = true,
                CatalogItems = new List<CatalogItem>
                {
                    new CatalogItem { IsActive = true, IsDeleted = false },
                    new CatalogItem { IsActive = true, IsDeleted = false },
                    new CatalogItem { IsActive = false, IsDeleted = false }, // Inactive
                    new CatalogItem { IsActive = true, IsDeleted = true }    // Deleted
                }
            }
        };

        var mockSet = CreateMockDbSet(categories);
        _mockContext.Setup(c => c.CatalogCategories).Returns(mockSet.Object);

        // Act
        var result = (await _service.GetCategoriesAsync()).First();

        // Assert
        result.ItemCount.Should().Be(2); // Only active, non-deleted items
    }

    #endregion

    #region CreateCatalogRequestForOthersAsync Tests

    [Fact]
    public async Task CreateCatalogRequestForOthersAsync_CreatesRequestForDifferentUser()
    {
        // Arrange
        var dto = new CreateCatalogRequestForOthersDto
        {
            CatalogItemId = 5,
            RequestedForUserId = 200,
            FormData = new Dictionary<string, object> { { "urgency", "high" } }
        };
        var requests = new List<CatalogRequest>();
        var mockSet = CreateMockDbSet(requests);
        CatalogRequest? capturedRequest = null;
        mockSet.Setup(m => m.Add(It.IsAny<CatalogRequest>()))
            .Callback<CatalogRequest>(r => capturedRequest = r);
        _mockContext.Setup(c => c.CatalogRequests).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        await _service.CreateCatalogRequestForOthersAsync(dto, requestedById: 100);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.RequestedById.Should().Be(100);
        capturedRequest.RequestedForId.Should().Be(200);
        capturedRequest.CatalogItemId.Should().Be(5);
    }

    [Fact]
    public async Task CreateCatalogRequestForOthersAsync_LogsCreation()
    {
        // Arrange
        var dto = new CreateCatalogRequestForOthersDto
        {
            CatalogItemId = 5,
            RequestedForUserId = 200
        };
        var requests = new List<CatalogRequest>();
        var mockSet = CreateMockDbSet(requests);
        _mockContext.Setup(c => c.CatalogRequests).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        await _service.CreateCatalogRequestForOthersAsync(dto, requestedById: 100);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("on behalf of user")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region GetRequestByIdAsync Tests

    [Fact]
    public async Task GetRequestByIdAsync_WhenExists_ReturnsRequest()
    {
        // Arrange
        var item = new CatalogItem { CatalogItemId = 10, Name = "Laptop" };
        var requests = new List<CatalogRequest>
        {
            new CatalogRequest 
            { 
                RequestId = 1, 
                CatalogItemId = 10, 
                CatalogItem = item, 
                RequestedById = 100 
            }
        };

        var mockSet = CreateMockDbSet(requests);
        _mockContext.Setup(c => c.CatalogRequests).Returns(mockSet.Object);

        // Act
        var result = await _service.GetRequestByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.RequestId.Should().Be(1);
        result.CatalogItem.Should().NotBeNull();
    }

    [Fact]
    public async Task GetRequestByIdAsync_WhenDeleted_ReturnsNull()
    {
        // Arrange
        var requests = new List<CatalogRequest>
        {
            new CatalogRequest { RequestId = 1, IsDeleted = true }
        };

        var mockSet = CreateMockDbSet(requests);
        _mockContext.Setup(c => c.CatalogRequests).Returns(mockSet.Object);

        // Act
        var result = await _service.GetRequestByIdAsync(1);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetRequestByIdAsync_WhenNotFound_ReturnsNull()
    {
        // Arrange
        var requests = new List<CatalogRequest>();
        var mockSet = CreateMockDbSet(requests);
        _mockContext.Setup(c => c.CatalogRequests).Returns(mockSet.Object);

        // Act
        var result = await _service.GetRequestByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CancelRequestAsync Tests

    [Fact]
    public async Task CancelRequestAsync_WhenValid_CancelsRequest()
    {
        // Arrange
        var requests = new List<CatalogRequest>
        {
            new CatalogRequest 
            { 
                RequestId = 1, 
                RequestedById = 100, 
                State = CatalogRequestState.Requested 
            }
        };

        var mockSet = CreateMockDbSet(requests);
        _mockContext.Setup(c => c.CatalogRequests).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await _service.CancelRequestAsync(1, userId: 100);

        // Assert
        result.Should().BeTrue();
        requests[0].State.Should().Be(CatalogRequestState.Cancelled);
    }

    [Fact]
    public async Task CancelRequestAsync_WhenNotRequester_ReturnsFalse()
    {
        // Arrange
        var requests = new List<CatalogRequest>
        {
            new CatalogRequest 
            { 
                RequestId = 1, 
                RequestedById = 100, 
                State = CatalogRequestState.Requested 
            }
        };

        var mockSet = CreateMockDbSet(requests);
        _mockContext.Setup(c => c.CatalogRequests).Returns(mockSet.Object);

        // Act
        var result = await _service.CancelRequestAsync(1, userId: 999); // Different user

        // Assert
        result.Should().BeFalse();
        requests[0].State.Should().Be(CatalogRequestState.Requested); // State unchanged
    }

    [Fact]
    public async Task CancelRequestAsync_WhenAlreadyFulfilling_ReturnsFalse()
    {
        // Arrange
        var requests = new List<CatalogRequest>
        {
            new CatalogRequest 
            { 
                RequestId = 1, 
                RequestedById = 100, 
                State = CatalogRequestState.Fulfilling 
            }
        };

        var mockSet = CreateMockDbSet(requests);
        _mockContext.Setup(c => c.CatalogRequests).Returns(mockSet.Object);

        // Act
        var result = await _service.CancelRequestAsync(1, userId: 100);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CancelRequestAsync_WhenPendingApproval_AllowsCancellation()
    {
        // Arrange
        var requests = new List<CatalogRequest>
        {
            new CatalogRequest 
            { 
                RequestId = 1, 
                RequestedById = 100, 
                State = CatalogRequestState.PendingApproval 
            }
        };

        var mockSet = CreateMockDbSet(requests);
        _mockContext.Setup(c => c.CatalogRequests).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await _service.CancelRequestAsync(1, userId: 100);

        // Assert
        result.Should().BeTrue();
        requests[0].State.Should().Be(CatalogRequestState.Cancelled);
    }

    [Fact]
    public async Task CancelRequestAsync_WhenNotFound_ReturnsFalse()
    {
        // Arrange
        var requests = new List<CatalogRequest>();
        var mockSet = CreateMockDbSet(requests);
        _mockContext.Setup(c => c.CatalogRequests).Returns(mockSet.Object);

        // Act
        var result = await _service.CancelRequestAsync(999, userId: 100);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CancelRequestAsync_LogsCancellation()
    {
        // Arrange
        var requests = new List<CatalogRequest>
        {
            new CatalogRequest 
            { 
                RequestId = 1, 
                RequestedById = 100, 
                State = CatalogRequestState.Requested 
            }
        };

        var mockSet = CreateMockDbSet(requests);
        _mockContext.Setup(c => c.CatalogRequests).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        await _service.CancelRequestAsync(1, userId: 100);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Cancelled catalog request")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region MapToDto Tests

    [Fact]
    public async Task MapToDto_MapsAllProperties()
    {
        // Arrange
        var category = new CatalogCategory { CategoryId = 1, Name = "Hardware" };
        var items = new List<CatalogItem>
        {
            new CatalogItem 
            { 
                CatalogItemId = 1, 
                Name = "Laptop", 
                ShortDescription = "Business laptop",
                CategoryId = 1,
                Category = category,
                Price = 1500.00m,
                IsFeatured = true,
                IsActive = true
            }
        };

        var mockSet = CreateMockDbSet(items);
        _mockContext.Setup(c => c.CatalogItems).Returns(mockSet.Object);

        // Act
        var result = await _service.GetCatalogItemByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.CatalogItemId.Should().Be(1);
        result.Name.Should().Be("Laptop");
        result.ShortDescription.Should().Be("Business laptop");
        result.CategoryId.Should().Be(1);
        result.CategoryName.Should().Be("Hardware");
        result.Price.Should().Be(1500.00m);
        result.IsFeatured.Should().BeTrue();
        result.IsActive.Should().BeTrue();
    }

    #endregion

    #region Helper Methods

    private static Mock<DbSet<T>> CreateMockDbSet<T>(IEnumerable<T> data) where T : class
    {
        var queryable = data.AsQueryable();
        var mockSet = new Mock<DbSet<T>>();
        
        mockSet.As<IAsyncEnumerable<T>>()
            .Setup(m => m.GetAsyncEnumerator(default))
            .Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));
        
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

        return mockSet;
    }

    #endregion
}

#region Async Test Helpers

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

internal class TestAsyncQueryProvider<T> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    public TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;

    public IQueryable CreateQuery(Expression expression) => _inner.CreateQuery(expression);

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => 
        new TestAsyncEnumerable<TElement>(expression);

    public object? Execute(Expression expression) => _inner.Execute(expression);

    public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        var expectedResultType = typeof(TResult).GetGenericArguments()[0];
        var executionResult = typeof(IQueryProvider)
            .GetMethod(
                name: nameof(IQueryProvider.Execute),
                genericParameterCount: 1,
                types: new[] { typeof(Expression) })!
            .MakeGenericMethod(expectedResultType)
            .Invoke(this, new[] { expression });

        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
            .MakeGenericMethod(expectedResultType)
            .Invoke(null, new[] { executionResult })!;
    }
}

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(Expression expression) : base(expression) { }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
        new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}

#endregion
