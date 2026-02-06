// CRM Solution - Customer Relationship Management System
// Product Repository Unit Tests

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

namespace CRM.Tests.Repositories;

/// <summary>
/// Unit tests for Product Repository
/// Covers: Product-specific queries, pricing, inventory
/// </summary>
public class ProductRepositoryTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<DbSet<ProductEntity>> _mockDbSet;
    private readonly Mock<ILogger<ProductRepository>> _mockLogger;
    private readonly ProductRepository _repository;

    public ProductRepositoryTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockDbSet = new Mock<DbSet<ProductEntity>>();
        _mockLogger = new Mock<ILogger<ProductRepository>>();

        _mockContext.Setup(c => c.Set<ProductEntity>()).Returns(_mockDbSet.Object);
        _repository = new ProductRepository(_mockContext.Object, _mockLogger.Object);
    }

    #region GetByCategory Tests

    [Fact]
    public async Task GetByCategoryAsync_HasMatches_ReturnsProducts()
    {
        // Arrange
        var products = new List<ProductEntity>
        {
            new ProductEntity { Id = 1, Category = "Software" },
            new ProductEntity { Id = 2, Category = "Software" },
            new ProductEntity { Id = 3, Category = "Hardware" }
        }.AsQueryable();

        SetupMockDbSet(products);

        // Act
        var result = await _repository.GetByCategoryAsync("Software");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetCategoriesAsync_ReturnsUniqueCategories()
    {
        // Arrange
        var products = new List<ProductEntity>
        {
            new ProductEntity { Id = 1, Category = "Software" },
            new ProductEntity { Id = 2, Category = "Software" },
            new ProductEntity { Id = 3, Category = "Hardware" }
        }.AsQueryable();

        SetupMockDbSet(products);

        // Act
        var result = await _repository.GetCategoriesAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain("Software");
        result.Should().Contain("Hardware");
    }

    #endregion

    #region GetByCode Tests

    [Fact]
    public async Task GetByCodeAsync_ExistingCode_ReturnsProduct()
    {
        // Arrange
        var products = new List<ProductEntity>
        {
            new ProductEntity { Id = 1, Code = "PROD-001", Name = "Product 1" }
        }.AsQueryable();

        SetupMockDbSet(products);

        // Act
        var result = await _repository.GetByCodeAsync("PROD-001");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Product 1");
    }

    [Fact]
    public async Task GetByCodeAsync_NonExisting_ReturnsNull()
    {
        // Arrange
        var products = new List<ProductEntity>().AsQueryable();
        SetupMockDbSet(products);

        // Act
        var result = await _repository.GetByCodeAsync("NOTFOUND");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Active Products Tests

    [Fact]
    public async Task GetActiveAsync_ReturnsActiveOnly()
    {
        // Arrange
        var products = new List<ProductEntity>
        {
            new ProductEntity { Id = 1, IsActive = true },
            new ProductEntity { Id = 2, IsActive = true },
            new ProductEntity { Id = 3, IsActive = false }
        }.AsQueryable();

        SetupMockDbSet(products);

        // Act
        var result = await _repository.GetActiveAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetInactiveAsync_ReturnsInactiveOnly()
    {
        // Arrange
        var products = new List<ProductEntity>
        {
            new ProductEntity { Id = 1, IsActive = true },
            new ProductEntity { Id = 2, IsActive = false },
            new ProductEntity { Id = 3, IsActive = false }
        }.AsQueryable();

        SetupMockDbSet(products);

        // Act
        var result = await _repository.GetInactiveAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Pricing Tests

    [Fact]
    public async Task GetByPriceRangeAsync_ReturnsProductsInRange()
    {
        // Arrange
        var products = new List<ProductEntity>
        {
            new ProductEntity { Id = 1, Price = 50 },
            new ProductEntity { Id = 2, Price = 100 },
            new ProductEntity { Id = 3, Price = 200 }
        }.AsQueryable();

        SetupMockDbSet(products);

        // Act
        var result = await _repository.GetByPriceRangeAsync(50, 150);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetWithDiscountsAsync_ReturnsDiscountedProducts()
    {
        // Arrange
        var products = new List<ProductEntity>
        {
            new ProductEntity { Id = 1, DiscountPercent = 10 },
            new ProductEntity { Id = 2, DiscountPercent = 20 },
            new ProductEntity { Id = 3, DiscountPercent = 0 }
        }.AsQueryable();

        SetupMockDbSet(products);

        // Act
        var result = await _repository.GetWithDiscountsAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Inventory Tests

    [Fact]
    public async Task GetLowStockAsync_ReturnsLowStockProducts()
    {
        // Arrange
        var products = new List<ProductEntity>
        {
            new ProductEntity { Id = 1, StockQuantity = 5, ReorderLevel = 10 },
            new ProductEntity { Id = 2, StockQuantity = 3, ReorderLevel = 10 },
            new ProductEntity { Id = 3, StockQuantity = 50, ReorderLevel = 10 }
        }.AsQueryable();

        SetupMockDbSet(products);

        // Act
        var result = await _repository.GetLowStockAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetOutOfStockAsync_ReturnsOutOfStockProducts()
    {
        // Arrange
        var products = new List<ProductEntity>
        {
            new ProductEntity { Id = 1, StockQuantity = 0 },
            new ProductEntity { Id = 2, StockQuantity = 0 },
            new ProductEntity { Id = 3, StockQuantity = 10 }
        }.AsQueryable();

        SetupMockDbSet(products);

        // Act
        var result = await _repository.GetOutOfStockAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateStockAsync_UpdatesQuantity()
    {
        // Arrange
        var product = new ProductEntity { Id = 1, StockQuantity = 10 };
        var products = new List<ProductEntity> { product }.AsQueryable();
        
        SetupMockDbSet(products);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        await _repository.UpdateStockAsync(1, 15);

        // Assert
        _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task SearchAsync_ByName_ReturnsMatches()
    {
        // Arrange
        var products = new List<ProductEntity>
        {
            new ProductEntity { Id = 1, Name = "Enterprise CRM" },
            new ProductEntity { Id = 2, Name = "CRM Lite" },
            new ProductEntity { Id = 3, Name = "Analytics Suite" }
        }.AsQueryable();

        SetupMockDbSet(products);

        // Act
        var result = await _repository.SearchAsync("CRM");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchAsync_ByDescription_ReturnsMatches()
    {
        // Arrange
        var products = new List<ProductEntity>
        {
            new ProductEntity { Id = 1, Name = "Product 1", Description = "Cloud-based solution" },
            new ProductEntity { Id = 2, Name = "Product 2", Description = "On-premise solution" }
        }.AsQueryable();

        SetupMockDbSet(products);

        // Act
        var result = await _repository.SearchAsync("Cloud");

        // Assert
        result.Should().HaveCount(1);
    }

    #endregion

    #region Type Tests

    [Fact]
    public async Task GetByTypeAsync_ReturnsProductsByType()
    {
        // Arrange
        var products = new List<ProductEntity>
        {
            new ProductEntity { Id = 1, Type = "Subscription" },
            new ProductEntity { Id = 2, Type = "Subscription" },
            new ProductEntity { Id = 3, Type = "One-Time" }
        }.AsQueryable();

        SetupMockDbSet(products);

        // Act
        var result = await _repository.GetByTypeAsync("Subscription");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetSubscriptionProductsAsync_ReturnsSubscriptions()
    {
        // Arrange
        var products = new List<ProductEntity>
        {
            new ProductEntity { Id = 1, IsSubscription = true },
            new ProductEntity { Id = 2, IsSubscription = true },
            new ProductEntity { Id = 3, IsSubscription = false }
        }.AsQueryable();

        SetupMockDbSet(products);

        // Act
        var result = await _repository.GetSubscriptionProductsAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetCountByCategoryAsync_ReturnsCategoryCounts()
    {
        // Arrange
        var products = new List<ProductEntity>
        {
            new ProductEntity { Id = 1, Category = "Software" },
            new ProductEntity { Id = 2, Category = "Software" },
            new ProductEntity { Id = 3, Category = "Hardware" }
        }.AsQueryable();

        SetupMockDbSet(products);

        // Act
        var result = await _repository.GetCountByCategoryAsync();

        // Assert
        result["Software"].Should().Be(2);
    }

    [Fact]
    public async Task GetTotalInventoryValueAsync_CalculatesTotalValue()
    {
        // Arrange
        var products = new List<ProductEntity>
        {
            new ProductEntity { Id = 1, Price = 100, StockQuantity = 10 },
            new ProductEntity { Id = 2, Price = 50, StockQuantity = 20 },
            new ProductEntity { Id = 3, Price = 200, StockQuantity = 5 }
        }.AsQueryable();

        SetupMockDbSet(products);

        // Act
        var result = await _repository.GetTotalInventoryValueAsync();

        // Assert
        // (100*10) + (50*20) + (200*5) = 1000 + 1000 + 1000 = 3000
        result.Should().Be(3000);
    }

    [Fact]
    public async Task GetAveragePriceAsync_CalculatesAverage()
    {
        // Arrange
        var products = new List<ProductEntity>
        {
            new ProductEntity { Id = 1, Price = 100 },
            new ProductEntity { Id = 2, Price = 200 },
            new ProductEntity { Id = 3, Price = 300 }
        }.AsQueryable();

        SetupMockDbSet(products);

        // Act
        var result = await _repository.GetAveragePriceAsync();

        // Assert
        result.Should().Be(200);
    }

    #endregion

    #region Bundle Tests

    [Fact]
    public async Task GetBundlesAsync_ReturnsBundleProducts()
    {
        // Arrange
        var products = new List<ProductEntity>
        {
            new ProductEntity { Id = 1, IsBundle = true },
            new ProductEntity { Id = 2, IsBundle = true },
            new ProductEntity { Id = 3, IsBundle = false }
        }.AsQueryable();

        SetupMockDbSet(products);

        // Act
        var result = await _repository.GetBundlesAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetBundleComponentsAsync_ReturnsComponents()
    {
        // Arrange
        var product = new ProductEntity
        {
            Id = 1,
            IsBundle = true,
            BundleComponents = new List<BundleComponent>
            {
                new BundleComponent { ProductId = 2 },
                new BundleComponent { ProductId = 3 }
            }
        };

        var products = new List<ProductEntity> { product }.AsQueryable();
        SetupMockDbSet(products);

        // Act
        var result = await _repository.GetBundleComponentsAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Featured Products Tests

    [Fact]
    public async Task GetFeaturedAsync_ReturnsFeaturedProducts()
    {
        // Arrange
        var products = new List<ProductEntity>
        {
            new ProductEntity { Id = 1, IsFeatured = true },
            new ProductEntity { Id = 2, IsFeatured = true },
            new ProductEntity { Id = 3, IsFeatured = false }
        }.AsQueryable();

        SetupMockDbSet(products);

        // Act
        var result = await _repository.GetFeaturedAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetBestSellersAsync_ReturnsBestSellers()
    {
        // Arrange
        var products = new List<ProductEntity>
        {
            new ProductEntity { Id = 1, SalesCount = 100 },
            new ProductEntity { Id = 2, SalesCount = 50 },
            new ProductEntity { Id = 3, SalesCount = 200 }
        }.AsQueryable();

        SetupMockDbSet(products);

        // Act
        var result = await _repository.GetBestSellersAsync(2);

        // Assert
        result.Should().HaveCount(2);
        result.First().SalesCount.Should().Be(200);
    }

    #endregion

    #region Recent Activity Tests

    [Fact]
    public async Task GetRecentlyAddedAsync_ReturnsRecent()
    {
        // Arrange
        var products = new List<ProductEntity>
        {
            new ProductEntity { Id = 1, CreatedAt = DateTime.UtcNow.AddDays(-5) },
            new ProductEntity { Id = 2, CreatedAt = DateTime.UtcNow.AddDays(-15) },
            new ProductEntity { Id = 3, CreatedAt = DateTime.UtcNow.AddDays(-40) }
        }.AsQueryable();

        SetupMockDbSet(products);

        // Act
        var result = await _repository.GetRecentlyAddedAsync(30);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Helper Methods

    private void SetupMockDbSet(IQueryable<ProductEntity> data)
    {
        _mockDbSet.As<IQueryable<ProductEntity>>().Setup(m => m.Provider).Returns(data.Provider);
        _mockDbSet.As<IQueryable<ProductEntity>>().Setup(m => m.Expression).Returns(data.Expression);
        _mockDbSet.As<IQueryable<ProductEntity>>().Setup(m => m.ElementType).Returns(data.ElementType);
        _mockDbSet.As<IQueryable<ProductEntity>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
    }

    #endregion
}

// Supporting classes
public class ProductEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? Type { get; set; }
    public decimal Price { get; set; }
    public decimal DiscountPercent { get; set; }
    public int StockQuantity { get; set; }
    public int ReorderLevel { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsSubscription { get; set; }
    public bool IsBundle { get; set; }
    public bool IsFeatured { get; set; }
    public int SalesCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public List<BundleComponent> BundleComponents { get; set; } = new();
}

public class BundleComponent
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
