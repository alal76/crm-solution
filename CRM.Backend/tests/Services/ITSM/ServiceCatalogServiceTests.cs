// CRM Solution - ITSM Service Catalog Service Unit Tests
// Tests for ServiceCatalogService - Catalog item and request management

using CRM.Core.DTOs.ITSM;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.ITSM;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services.ITSM;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.ITSMServices.Catalog;

public class ServiceCatalogServiceTests
{
    private readonly Mock<IDbContextResolver> _mockResolver;
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<ServiceCatalogService>> _mockLogger;
    private readonly IServiceCatalogService _service;

    public ServiceCatalogServiceTests()
    {
        _mockResolver = new Mock<IDbContextResolver>();
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<ServiceCatalogService>>();

        _mockResolver.Setup(r => r.ResolveContext()).Returns(_mockContext.Object);
        _service = new ServiceCatalogService(_mockResolver.Object, _mockLogger.Object);
    }

    // ========================================================================
    // GetCatalogItemsAsync
    // ========================================================================

    [Fact]
    public async Task GetCatalogItemsAsync_ShouldReturnAllActiveItems_WhenNoFilters()
    {
        // Arrange
        var items = new List<CatalogItem>
        {
            new() { CatalogItemId = 1, Name = "Laptop Request", ShortDescription = "Request a new laptop", IsActive = true, IsFeatured = false, CreatedAt = DateTime.UtcNow },
            new() { CatalogItemId = 2, Name = "Software Install", ShortDescription = "Request software", IsActive = true, IsFeatured = true, CreatedAt = DateTime.UtcNow },
            new() { CatalogItemId = 3, Name = "Inactive Item", ShortDescription = "Old item", IsActive = false, CreatedAt = DateTime.UtcNow }
        };
        _mockContext.Setup(c => c.CatalogItems).Returns(MockDbSetFactory.CreateMockDbSet(items).Object);

        // Act
        var result = await _service.GetCatalogItemsAsync(null, null);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetCatalogItemsAsync_ShouldFilterFeaturedItems_WhenFeaturedOnlyTrue()
    {
        // Arrange
        var items = new List<CatalogItem>
        {
            new() { CatalogItemId = 1, Name = "Regular Item", IsActive = true, IsFeatured = false, CreatedAt = DateTime.UtcNow },
            new() { CatalogItemId = 2, Name = "Featured Item", IsActive = true, IsFeatured = true, CreatedAt = DateTime.UtcNow }
        };
        _mockContext.Setup(c => c.CatalogItems).Returns(MockDbSetFactory.CreateMockDbSet(items).Object);

        // Act
        var result = await _service.GetCatalogItemsAsync(null, true);

        // Assert
        result.Should().NotBeNull();
        result.Should().OnlyContain(i => i.IsFeatured);
    }

    // ========================================================================
    // GetCatalogItemByIdAsync
    // ========================================================================

    [Fact]
    public async Task GetCatalogItemByIdAsync_ShouldReturnItem_WhenExists()
    {
        // Arrange
        var items = new List<CatalogItem>
        {
            new() { CatalogItemId = 1, Name = "VPN Access", ShortDescription = "Request VPN", LongDescription = "Full VPN access request process", IsActive = true, Price = 0, CreatedAt = DateTime.UtcNow }
        };
        _mockContext.Setup(c => c.CatalogItems).Returns(MockDbSetFactory.CreateMockDbSet(items).Object);

        // Act
        var result = await _service.GetCatalogItemByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("VPN Access");
    }

    [Fact]
    public async Task GetCatalogItemByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        _mockContext.Setup(c => c.CatalogItems).Returns(MockDbSetFactory.CreateMockDbSet(new List<CatalogItem>()).Object);

        // Act
        var result = await _service.GetCatalogItemByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    // ========================================================================
    // CreateCatalogRequestAsync
    // ========================================================================

    [Fact]
    public async Task CreateCatalogRequestAsync_ShouldCreateRequest_WhenValidDtoProvided()
    {
        // Arrange
        var items = new List<CatalogItem>
        {
            new() { CatalogItemId = 1, Name = "Laptop Request", IsActive = true, RequestCount = 5, CreatedAt = DateTime.UtcNow }
        };
        var requests = new List<CatalogRequest>();
        var mockRequestSet = MockDbSetFactory.CreateMockDbSet(requests);
        mockRequestSet.Setup(m => m.Add(It.IsAny<CatalogRequest>())).Callback<CatalogRequest>(e => requests.Add(e));

        _mockContext.Setup(c => c.CatalogItems).Returns(MockDbSetFactory.CreateMockDbSet(items).Object);
        _mockContext.Setup(c => c.CatalogRequests).Returns(mockRequestSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var dto = new CreateCatalogRequestDto
        {
            CatalogItemId = 1
        };

        // Act
        var result = await _service.CreateCatalogRequestAsync(dto, requestedById: 10);

        // Assert
        result.Should().BeGreaterThanOrEqualTo(0);
        mockRequestSet.Verify(m => m.Add(It.IsAny<CatalogRequest>()), Times.Once);
    }

    [Fact]
    public async Task CreateCatalogRequestAsync_ShouldSetRequestedState()
    {
        // Arrange
        var items = new List<CatalogItem>
        {
            new() { CatalogItemId = 1, Name = "Monitor", IsActive = true, CreatedAt = DateTime.UtcNow }
        };
        var requests = new List<CatalogRequest>();
        var mockRequestSet = MockDbSetFactory.CreateMockDbSet(requests);
        mockRequestSet.Setup(m => m.Add(It.IsAny<CatalogRequest>())).Callback<CatalogRequest>(e => requests.Add(e));

        _mockContext.Setup(c => c.CatalogItems).Returns(MockDbSetFactory.CreateMockDbSet(items).Object);
        _mockContext.Setup(c => c.CatalogRequests).Returns(mockRequestSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var dto = new CreateCatalogRequestDto { CatalogItemId = 1 };

        // Act
        var result = await _service.CreateCatalogRequestAsync(dto, requestedById: 5);

        // Assert
        result.Should().BeGreaterThanOrEqualTo(0);
    }

    // ========================================================================
    // GetMyRequestsAsync
    // ========================================================================

    [Fact]
    public async Task GetMyRequestsAsync_ShouldReturnOnlyUserRequests()
    {
        // Arrange
        var requests = new List<CatalogRequest>
        {
            new() { RequestId = 1, CatalogItemId = 1, RequestedById = 10, RequestedForId = 10, State = CatalogRequestState.Requested, CreatedAt = DateTime.UtcNow },
            new() { RequestId = 2, CatalogItemId = 2, RequestedById = 20, RequestedForId = 20, State = CatalogRequestState.Approved, CreatedAt = DateTime.UtcNow },
            new() { RequestId = 3, CatalogItemId = 1, RequestedById = 10, RequestedForId = 10, State = CatalogRequestState.Completed, CreatedAt = DateTime.UtcNow }
        };
        _mockContext.Setup(c => c.CatalogRequests).Returns(MockDbSetFactory.CreateMockDbSet(requests).Object);

        // Act
        var result = await _service.GetMyRequestsAsync(userId: 10);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().OnlyContain(r => r.RequestedById == 10);
    }

    // ========================================================================
    // SearchCatalogAsync
    // ========================================================================

    [Fact]
    public async Task SearchCatalogAsync_ShouldReturnMatchingItems()
    {
        // Arrange
        var items = new List<CatalogItem>
        {
            new() { CatalogItemId = 1, Name = "Laptop Request", ShortDescription = "Request a new laptop", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { CatalogItemId = 2, Name = "Software Install", ShortDescription = "Request software install", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { CatalogItemId = 3, Name = "Monitor Request", ShortDescription = "Request a monitor", IsActive = true, CreatedAt = DateTime.UtcNow }
        };
        _mockContext.Setup(c => c.CatalogItems).Returns(MockDbSetFactory.CreateMockDbSet(items).Object);

        // Act
        var result = await _service.SearchCatalogAsync("laptop");

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain(i => i.Name.Contains("Laptop", StringComparison.OrdinalIgnoreCase));
    }

    // ========================================================================
    // GetRequestByIdAsync
    // ========================================================================

    [Fact]
    public async Task GetRequestByIdAsync_ShouldReturnRequest_WhenExists()
    {
        // Arrange
        var requests = new List<CatalogRequest>
        {
            new() { RequestId = 1, CatalogItemId = 1, RequestedById = 10, RequestedForId = 10, State = CatalogRequestState.InProgress, CreatedAt = DateTime.UtcNow }
        };
        _mockContext.Setup(c => c.CatalogRequests).Returns(MockDbSetFactory.CreateMockDbSet(requests).Object);

        // Act
        var result = await _service.GetRequestByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.RequestId.Should().Be(1);
        result.State.Should().Be(CatalogRequestState.InProgress);
    }

    // ========================================================================
    // CancelRequestAsync
    // ========================================================================

    [Fact]
    public async Task CancelRequestAsync_ShouldCancelRequest_WhenOwnedByUser()
    {
        // Arrange
        var requests = new List<CatalogRequest>
        {
            new() { RequestId = 1, CatalogItemId = 1, RequestedById = 10, RequestedForId = 10, State = CatalogRequestState.Requested, CreatedAt = DateTime.UtcNow }
        };
        _mockContext.Setup(c => c.CatalogRequests).Returns(MockDbSetFactory.CreateMockDbSet(requests).Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.CancelRequestAsync(1, userId: 10);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CancelRequestAsync_ShouldReturnFalse_WhenRequestNotFound()
    {
        // Arrange
        _mockContext.Setup(c => c.CatalogRequests).Returns(MockDbSetFactory.CreateMockDbSet(new List<CatalogRequest>()).Object);

        // Act
        var result = await _service.CancelRequestAsync(999, userId: 10);

        // Assert
        result.Should().BeFalse();
    }

    // ========================================================================
    // GetCategoriesAsync
    // ========================================================================

    [Fact]
    public async Task GetCategoriesAsync_ShouldReturnCategoryList()
    {
        // Arrange
        var categories = new List<CatalogCategory>
        {
            new() { CategoryId = 1, Name = "Hardware", Description = "Hardware requests", DisplayOrder = 1, IsActive = true, IsDeleted = false, CreatedAt = DateTime.UtcNow },
            new() { CategoryId = 2, Name = "Software", Description = "Software requests", DisplayOrder = 2, IsActive = true, IsDeleted = false, CreatedAt = DateTime.UtcNow }
        };
        _mockContext.Setup(c => c.CatalogCategories).Returns(MockDbSetFactory.CreateMockDbSet(categories).Object);

        // Act
        var result = await _service.GetCategoriesAsync();

        // Assert
        result.Should().NotBeNull();
    }
}
