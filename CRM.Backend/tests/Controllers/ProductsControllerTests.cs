// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Api.Controllers;
using CRM.Api.Hubs;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for ProductsController
/// </summary>
public class ProductsControllerTests
{
    private readonly Mock<IProductService> _mockProductService;
    private readonly Mock<ILogger<ProductsController>> _mockLogger;
    private readonly Mock<ICrmNotificationService> _mockNotificationService;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _mockProductService = new Mock<IProductService>();
        _mockLogger = new Mock<ILogger<ProductsController>>();
        _mockNotificationService = new Mock<ICrmNotificationService>();

        // Setup default returns for notification service
        _mockNotificationService.Setup(x => x.NotifyRecordCreatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);
        _mockNotificationService.Setup(x => x.NotifyRecordUpdatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);
        _mockNotificationService.Setup(x => x.NotifyRecordDeletedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);

        _controller = new ProductsController(_mockProductService.Object, _mockLogger.Object, _mockNotificationService.Object);

        // Setup HttpContext with Response.Headers for ETag support
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_ReturnsOkResult_WithProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product A", SKU = "SKU-001", Price = 99.99m },
            new() { Id = 2, Name = "Product B", SKU = "SKU-002", Price = 149.99m }
        };
        _mockProductService.Setup(s => s.GetAllProductsAsync())
            .ReturnsAsync(products);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedProducts = okResult.Value.Should().BeAssignableTo<IEnumerable<Product>>().Subject;
        returnedProducts.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_ReturnsEmptyList_WhenNoProducts()
    {
        // Arrange
        _mockProductService.Setup(s => s.GetAllProductsAsync())
            .ReturnsAsync(new List<Product>());

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedProducts = okResult.Value.Should().BeAssignableTo<IEnumerable<Product>>().Subject;
        returnedProducts.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAll_Returns500_OnException()
    {
        // Arrange
        _mockProductService.Setup(s => s.GetAllProductsAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetAll();

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_ReturnsOkResult_WhenProductExists()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Test Product",
            SKU = "SKU-TEST",
            Price = 199.99m,
            Cost = 100m,
            Status = ProductStatus.Active
        };
        _mockProductService.Setup(s => s.GetProductByIdAsync(1))
            .ReturnsAsync(product);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedProduct = okResult.Value.Should().BeOfType<Product>().Subject;
        returnedProduct.Id.Should().Be(1);
        returnedProduct.Name.Should().Be("Test Product");
        returnedProduct.SKU.Should().Be("SKU-TEST");
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenProductDoesNotExist()
    {
        // Arrange
        _mockProductService.Setup(s => s.GetProductByIdAsync(999))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetById_Returns500_OnException()
    {
        // Arrange
        _mockProductService.Setup(s => s.GetProductByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
    }

    #endregion

    #region GetByCategory Tests

    [Fact]
    public async Task GetByCategory_ReturnsOkResult_WithProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Software A", Category = "Software", Price = 299.99m },
            new() { Id = 2, Name = "Software B", Category = "Software", Price = 399.99m }
        };
        _mockProductService.Setup(s => s.GetProductsByCategoryAsync("Software"))
            .ReturnsAsync(products);

        // Act
        var result = await _controller.GetByCategory("Software");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedProducts = okResult.Value.Should().BeAssignableTo<IEnumerable<Product>>().Subject;
        returnedProducts.Should().HaveCount(2);
        returnedProducts.All(p => p.Category == "Software").Should().BeTrue();
    }

    [Fact]
    public async Task GetByCategory_ReturnsEmptyList_WhenNoCategoryMatch()
    {
        // Arrange
        _mockProductService.Setup(s => s.GetProductsByCategoryAsync("NonExistent"))
            .ReturnsAsync(new List<Product>());

        // Act
        var result = await _controller.GetByCategory("NonExistent");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedProducts = okResult.Value.Should().BeAssignableTo<IEnumerable<Product>>().Subject;
        returnedProducts.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByCategory_Returns500_OnException()
    {
        // Arrange
        _mockProductService.Setup(s => s.GetProductsByCategoryAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetByCategory("Software");

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_ReturnsCreatedResult_WhenSuccessful()
    {
        // Arrange
        var product = new Product
        {
            Name = "New Product",
            SKU = "SKU-NEW",
            Price = 49.99m,
            Status = ProductStatus.Active
        };
        _mockProductService.Setup(s => s.CreateProductAsync(It.IsAny<Product>()))
            .ReturnsAsync(10);

        // Act
        var result = await _controller.Create(product);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(ProductsController.GetById));
        createdResult.RouteValues!["id"].Should().Be(10);
    }

    [Fact]
    public async Task Create_Returns500_OnException()
    {
        // Arrange
        var product = new Product { Name = "Test" };
        _mockProductService.Setup(s => s.CreateProductAsync(It.IsAny<Product>()))
            .ThrowsAsync(new Exception("Creation failed"));

        // Act
        var result = await _controller.Create(product);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_ReturnsNoContent_WhenSuccessful()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Updated Product",
            Price = 79.99m
        };
        _mockProductService.Setup(s => s.UpdateProductAsync(It.IsAny<Product>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Update(1, product);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Update_SetsIdFromRoute()
    {
        // Arrange
        var product = new Product { Name = "Test" };
        _mockProductService.Setup(s => s.UpdateProductAsync(It.Is<Product>(p => p.Id == 5)))
            .Returns(Task.CompletedTask);

        // Act
        await _controller.Update(5, product);

        // Assert
        _mockProductService.Verify(s => s.UpdateProductAsync(It.Is<Product>(p => p.Id == 5)), Times.Once);
    }

    [Fact]
    public async Task Update_Returns500_OnException()
    {
        // Arrange
        var product = new Product { Name = "Test" };
        _mockProductService.Setup(s => s.UpdateProductAsync(It.IsAny<Product>()))
            .ThrowsAsync(new Exception("Update failed"));

        // Act
        var result = await _controller.Update(1, product);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenSuccessful()
    {
        // Arrange
        _mockProductService.Setup(s => s.DeleteProductAsync(1))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_Returns500_OnException()
    {
        // Arrange
        _mockProductService.Setup(s => s.DeleteProductAsync(It.IsAny<int>()))
            .ThrowsAsync(new Exception("Delete failed"));

        // Act
        var result = await _controller.Delete(1);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task GetAll_HandlesLargeProductList()
    {
        // Arrange
        var products = Enumerable.Range(1, 1000)
            .Select(i => new Product { Id = i, Name = $"Product {i}", SKU = $"SKU-{i:D4}" })
            .ToList();
        _mockProductService.Setup(s => s.GetAllProductsAsync())
            .ReturnsAsync(products);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedProducts = okResult.Value.Should().BeAssignableTo<IEnumerable<Product>>().Subject;
        returnedProducts.Should().HaveCount(1000);
    }

    #endregion
}
