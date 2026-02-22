// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.Api.Controllers;
using CRM.Core.DTOs;
using CRM.Core.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers
{
    public class OrdersControllerTests
    {
        private readonly Mock<IOrderService> _mockOrderService;
        private readonly Mock<ILogger<OrdersController>> _mockLogger;
        private readonly OrdersController _controller;

        public OrdersControllerTests()
        {
            _mockOrderService = new Mock<IOrderService>();
            _mockLogger = new Mock<ILogger<OrdersController>>();
            _controller = new OrdersController(_mockOrderService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetAll_ShouldReturnOrders()
        {
            // Arrange
            var orders = new List<OrderDto> { new OrderDto { Id = 1, Name = "Order 1" } };
            _mockOrderService.Setup(s => s.GetAllAsync(null, null, It.IsAny<CancellationToken>())).ReturnsAsync(orders);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
        }

        [Fact]
        public async Task GetById_ShouldReturnOrder_WhenExists()
        {
            // Arrange
            var order = new OrderDto { Id = 1, Name = "Order 1" };
            _mockOrderService.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(order);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            var returnedOrder = okResult!.Value as OrderDto;
            returnedOrder.Should().NotBeNull();
            returnedOrder!.Id.Should().Be(1);
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenMissing()
        {
            // Arrange
            _mockOrderService.Setup(s => s.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync((OrderDto?)null);

            // Act
            var result = await _controller.GetById(2);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task Create_ShouldReturnCreatedOrder_WhenValid()
        {
            // Arrange
            var dto = new CreateOrderDto { Name = "Order 1", AccountId = 1, OrderDate = "2026-02-20T00:00:00Z" };
            var created = new OrderDto { Id = 1, Name = "Order 1" };
            _mockOrderService.Setup(s => s.CreateAsync(dto, It.IsAny<CancellationToken>())).ReturnsAsync(created);

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var createdResult = result.Result as CreatedAtActionResult;
            createdResult.Should().NotBeNull();
            var createdOrder = createdResult!.Value as OrderDto;
            createdOrder.Should().NotBeNull();
            createdOrder!.Id.Should().Be(1);
        }

        [Fact]
        public async Task Update_ShouldReturnUpdatedOrder_WhenValid()
        {
            // Arrange
            var dto = new UpdateOrderDto { Id = 1, Name = "Order 1" };
            var updated = new OrderDto { Id = 1, Name = "Order 1" };
            _mockOrderService.Setup(s => s.UpdateAsync(dto, It.IsAny<CancellationToken>())).ReturnsAsync(updated);

            // Act
            var result = await _controller.Update(1, dto);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            var updatedOrder = okResult!.Value as OrderDto;
            updatedOrder.Should().NotBeNull();
            updatedOrder!.Id.Should().Be(1);
        }

        // Additional tests for validation, mapping, and error handling can be added here.
    }
}
