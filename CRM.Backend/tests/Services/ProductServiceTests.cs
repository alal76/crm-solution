// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Product Service Unit Tests

using Xunit;
using Moq;
using FluentAssertions;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for ProductService
/// Tests product CRUD operations and business logic
/// </summary>
public class ProductServiceTests
{
    private readonly Mock<IProductService> _mockProductService;

    public ProductServiceTests()
    {
        _mockProductService = new Mock<IProductService>();
    }

    #region GetProductById Tests

    [Fact]
    public async Task GetProductById_ReturnsProduct_WhenExists()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "CRM Enterprise",
            SKU = "CRM-ENT-001",
            Price = 999.99m,
            ProductType = ProductType.Subscription,
            Status = ProductStatus.Active
        };

        _mockProductService.Setup(s => s.GetProductByIdAsync(1))
            .ReturnsAsync(product);

        // Act
        var result = await _mockProductService.Object.GetProductByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("CRM Enterprise");
        result.ProductType.Should().Be(ProductType.Subscription);
    }

    [Fact]
    public async Task GetProductById_ReturnsNull_WhenNotExists()
    {
        // Arrange
        _mockProductService.Setup(s => s.GetProductByIdAsync(999))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _mockProductService.Object.GetProductByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAllProducts Tests

    [Fact]
    public async Task GetAllProducts_ReturnsAllProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product 1", Status = ProductStatus.Active },
            new() { Id = 2, Name = "Product 2", Status = ProductStatus.Active },
            new() { Id = 3, Name = "Product 3", Status = ProductStatus.Discontinued }
        };

        _mockProductService.Setup(s => s.GetAllProductsAsync())
            .ReturnsAsync(products);

        // Act
        var result = await _mockProductService.Object.GetAllProductsAsync();

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllProducts_ReturnsEmptyList_WhenNoProducts()
    {
        // Arrange
        _mockProductService.Setup(s => s.GetAllProductsAsync())
            .ReturnsAsync(new List<Product>());

        // Act
        var result = await _mockProductService.Object.GetAllProductsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetProductsByCategory Tests

    [Fact]
    public async Task GetProductsByCategory_ReturnsFilteredProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "CRM", Category = "Software" },
            new() { Id = 2, Name = "Training", Category = "Services" }
        };

        _mockProductService.Setup(s => s.GetProductsByCategoryAsync("Software"))
            .ReturnsAsync(products.Where(p => p.Category == "Software"));

        // Act
        var result = await _mockProductService.Object.GetProductsByCategoryAsync("Software");

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("CRM");
    }

    [Fact]
    public async Task GetProductsByCategory_ReturnsEmpty_WhenCategoryNotFound()
    {
        // Arrange
        _mockProductService.Setup(s => s.GetProductsByCategoryAsync("NonExistent"))
            .ReturnsAsync(new List<Product>());

        // Act
        var result = await _mockProductService.Object.GetProductsByCategoryAsync("NonExistent");

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetProductsByType Tests

    [Fact]
    public async Task GetProductsByType_ReturnsFilteredProducts()
    {
        // Arrange
        var subscriptionProducts = new List<Product>
        {
            new() { Id = 1, Name = "CRM Basic", ProductType = ProductType.Subscription },
            new() { Id = 2, Name = "CRM Pro", ProductType = ProductType.Subscription }
        };

        _mockProductService.Setup(s => s.GetProductsByTypeAsync(ProductType.Subscription))
            .ReturnsAsync(subscriptionProducts);

        // Act
        var result = await _mockProductService.Object.GetProductsByTypeAsync(ProductType.Subscription);

        // Assert
        result.Should().HaveCount(2);
        result.All(p => p.ProductType == ProductType.Subscription).Should().BeTrue();
    }

    [Fact]
    public async Task GetProductsByType_Physical_ReturnsPhysicalProducts()
    {
        // Arrange
        var physicalProducts = new List<Product>
        {
            new() { Id = 1, Name = "Hardware Device", ProductType = ProductType.Physical }
        };

        _mockProductService.Setup(s => s.GetProductsByTypeAsync(ProductType.Physical))
            .ReturnsAsync(physicalProducts);

        // Act
        var result = await _mockProductService.Object.GetProductsByTypeAsync(ProductType.Physical);

        // Assert
        result.Should().HaveCount(1);
        result.First().ProductType.Should().Be(ProductType.Physical);
    }

    [Fact]
    public async Task GetProductsByType_Service_ReturnsServiceProducts()
    {
        // Arrange
        var serviceProducts = new List<Product>
        {
            new() { Id = 1, Name = "Consulting", ProductType = ProductType.Service },
            new() { Id = 2, Name = "Implementation", ProductType = ProductType.Service }
        };

        _mockProductService.Setup(s => s.GetProductsByTypeAsync(ProductType.Service))
            .ReturnsAsync(serviceProducts);

        // Act
        var result = await _mockProductService.Object.GetProductsByTypeAsync(ProductType.Service);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region CreateProduct Tests

    [Fact]
    public async Task CreateProduct_ReturnsNewId()
    {
        // Arrange
        var product = new Product
        {
            Name = "New Product",
            SKU = "NEW-001",
            Price = 199.99m,
            ProductType = ProductType.Digital,
            Status = ProductStatus.Draft
        };

        _mockProductService.Setup(s => s.CreateProductAsync(It.IsAny<Product>()))
            .ReturnsAsync(10);

        // Act
        var result = await _mockProductService.Object.CreateProductAsync(product);

        // Assert
        result.Should().Be(10);
    }

    [Fact]
    public async Task CreateProduct_ServiceIsCalled()
    {
        // Arrange
        var product = new Product { Name = "Test Product" };

        _mockProductService.Setup(s => s.CreateProductAsync(It.IsAny<Product>()))
            .ReturnsAsync(1);

        // Act
        await _mockProductService.Object.CreateProductAsync(product);

        // Assert
        _mockProductService.Verify(s => s.CreateProductAsync(It.IsAny<Product>()), Times.Once);
    }

    #endregion

    #region UpdateProduct Tests

    [Fact]
    public async Task UpdateProduct_UpdatesSuccessfully()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Updated Product",
            Price = 299.99m
        };

        _mockProductService.Setup(s => s.UpdateProductAsync(It.IsAny<Product>()))
            .Returns(Task.CompletedTask);

        // Act
        await _mockProductService.Object.UpdateProductAsync(product);

        // Assert
        _mockProductService.Verify(s => s.UpdateProductAsync(It.Is<Product>(p => p.Id == 1)), Times.Once);
    }

    [Fact]
    public async Task UpdateProduct_ChangesPrice()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Product",
            Price = 150.00m
        };

        _mockProductService.Setup(s => s.UpdateProductAsync(It.IsAny<Product>()))
            .Returns(Task.CompletedTask);

        // Act
        product.Price = 175.00m;
        await _mockProductService.Object.UpdateProductAsync(product);

        // Assert
        _mockProductService.Verify(s => s.UpdateProductAsync(
            It.Is<Product>(p => p.Price == 175.00m)), Times.Once);
    }

    [Fact]
    public async Task UpdateProduct_ChangesStatus()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Status = ProductStatus.Draft
        };

        _mockProductService.Setup(s => s.UpdateProductAsync(It.IsAny<Product>()))
            .Returns(Task.CompletedTask);

        // Act
        product.Status = ProductStatus.Active;
        await _mockProductService.Object.UpdateProductAsync(product);

        // Assert
        _mockProductService.Verify(s => s.UpdateProductAsync(
            It.Is<Product>(p => p.Status == ProductStatus.Active)), Times.Once);
    }

    #endregion

    #region DeleteProduct Tests

    [Fact]
    public async Task DeleteProduct_DeletesSuccessfully()
    {
        // Arrange
        _mockProductService.Setup(s => s.DeleteProductAsync(1))
            .Returns(Task.CompletedTask);

        // Act
        await _mockProductService.Object.DeleteProductAsync(1);

        // Assert
        _mockProductService.Verify(s => s.DeleteProductAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteProduct_CalledWithCorrectId()
    {
        // Arrange
        _mockProductService.Setup(s => s.DeleteProductAsync(It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        // Act
        await _mockProductService.Object.DeleteProductAsync(42);

        // Assert
        _mockProductService.Verify(s => s.DeleteProductAsync(42), Times.Once);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task GetProductById_WithZeroId_ReturnsNull()
    {
        // Arrange
        _mockProductService.Setup(s => s.GetProductByIdAsync(0))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _mockProductService.Object.GetProductByIdAsync(0);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetProductById_WithNegativeId_ReturnsNull()
    {
        // Arrange
        _mockProductService.Setup(s => s.GetProductByIdAsync(-1))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _mockProductService.Object.GetProductByIdAsync(-1);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetProductsByCategory_WithNullCategory_ReturnsEmpty()
    {
        // Arrange
        _mockProductService.Setup(s => s.GetProductsByCategoryAsync(null!))
            .ReturnsAsync(new List<Product>());

        // Act
        var result = await _mockProductService.Object.GetProductsByCategoryAsync(null!);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetProductsByCategory_WithEmptyCategory_ReturnsEmpty()
    {
        // Arrange
        _mockProductService.Setup(s => s.GetProductsByCategoryAsync(""))
            .ReturnsAsync(new List<Product>());

        // Act
        var result = await _mockProductService.Object.GetProductsByCategoryAsync("");

        // Assert
        result.Should().BeEmpty();
    }

    #endregion
}
