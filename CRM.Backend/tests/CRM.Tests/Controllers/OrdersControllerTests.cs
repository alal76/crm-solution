// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// TCOV2-D14 — OrdersController unit tests
using CRM.Api.Controllers;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for OrdersController (TCOV2-D14).
/// Route: /api/orders
/// </summary>
public class OrdersControllerTests
{
    private readonly Mock<IOrderService> _mockService;
    private readonly OrdersController _controller;

    public OrdersControllerTests()
    {
        _mockService = new Mock<IOrderService>();
        var mockLogger = new Mock<ILogger<OrdersController>>();
        _controller = new OrdersController(_mockService.Object, mockLogger.Object);
    }

    private static OrderDto MakeOrderDto(int id = 1) => new()
    {
        Id = id,
        OrderNumber = $"ORD-{id:D5}",
        Status = 0,
        TotalAmount = 9_999m
    };

    // ── GetAll ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ShouldReturnOk_WithOrders()
    {
        // Arrange
        _mockService.Setup(s => s.GetAllAsync(
                It.IsAny<int?>(), It.IsAny<OrderStatus?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OrderDto> { MakeOrderDto(1) });

        // Act
        var result = await _controller.GetAll();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk_WhenEmpty()
    {
        _mockService.Setup(s => s.GetAllAsync(
                It.IsAny<int?>(), It.IsAny<OrderStatus?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OrderDto>());

        var result = await _controller.GetAll();

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    // ── GetById ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenOrderExists()
    {
        // Arrange
        var order = MakeOrderDto(3);
        _mockService.Setup(s => s.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        // Act
        var result = await _controller.GetById(3);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        ((OkObjectResult)result.Result!).Value.Should().Be(order);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenOrderMissing()
    {
        // Arrange
        _mockService.Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderDto?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ── GetByOrderNumber ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetByOrderNumber_ShouldReturnNotFound_WhenMissing()
    {
        _mockService.Setup(s => s.GetByOrderNumberAsync("ORD-MISSING", It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderDto?)null);

        var result = await _controller.GetByOrderNumber("ORD-MISSING");

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ── Create ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_ShouldReturnCreated_WhenDtoValid()
    {
        // Arrange
        var dto = new CreateOrderDto { AccountId = 1, Name = "Test Order", OrderDate = "2025-01-01", OrderType = 0 };
        var created = MakeOrderDto(50);
        _mockService.Setup(s => s.CreateAsync(dto, It.IsAny<CancellationToken>())).ReturnsAsync(created);

        // Act
        var result = await _controller.Create(dto);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
        ((CreatedAtActionResult)result.Result!).StatusCode.Should().Be(201);
    }
}
