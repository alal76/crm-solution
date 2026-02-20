// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
//
// Tests for OrdersController covering DTO mapping, validation, and CRUD endpoints.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.Api.Controllers;
using CRM.Core.DTOs;
using CRM.Core.Entities;
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
            var orders = new List<Order> { new Order { Id = 1, Name = "Order 1" } };
            _mockOrderService.Setup(s => s.GetAllAsync(null, null, It.IsAny<CancellationToken>())).ReturnsAsync(orders);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            var returnedOrders = okResult!.Value as IEnumerable<Order>;
            returnedOrders.Should().NotBeNull();
        }

        [Fact]
        public async Task GetById_ShouldReturnOrder_WhenExists()
        {
            // Arrange
            var order = new Order { Id = 1, Name = "Order 1" };
            _mockOrderService.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(order);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            var returnedOrder = okResult!.Value as Order;
            returnedOrder.Should().NotBeNull();
            returnedOrder!.Id.Should().Be(1);
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenMissing()
        {
            // Arrange
            _mockOrderService.Setup(s => s.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync((Order?)null);

            // Act
            var result = await _controller.GetById(2);

            // Assert
            var notFound = result.Result as NotFoundObjectResult;
            notFound.Should().NotBeNull();
        }

        [Fact]
        public async Task Create_ShouldReturnCreatedOrder_WhenValid()
        {
            // Arrange
            var order = new Order { Id = 1, Name = "Order 1" };
            _mockOrderService.Setup(s => s.CreateAsync(order, It.IsAny<CancellationToken>())).ReturnsAsync(order);

            // Act
            var result = await _controller.Create(order);

            // Assert
            var created = result.Result as CreatedAtActionResult;
            created.Should().NotBeNull();
            var createdOrder = created!.Value as Order;
            createdOrder.Should().NotBeNull();
            createdOrder!.Id.Should().Be(1);
        }

        [Fact]
        public async Task Update_ShouldReturnUpdatedOrder_WhenValid()
        {
            // Arrange
            var order = new Order { Id = 1, Name = "Order 1" };
            _mockOrderService.Setup(s => s.UpdateAsync(order, It.IsAny<CancellationToken>())).ReturnsAsync(order);

            // Act
            var result = await _controller.Update(1, order);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            var updatedOrder = okResult!.Value as Order;
            updatedOrder.Should().NotBeNull();
            updatedOrder!.Id.Should().Be(1);
        }

        [Fact]
        public async Task Update_ShouldReturnBadRequest_WhenIdMismatch()
        {
            // Arrange
            var order = new Order { Id = 2, Name = "Order 2" };

            // Act
            var result = await _controller.Update(1, order);

            // Assert
            var badRequest = result.Result as BadRequestObjectResult;
            badRequest.Should().NotBeNull();
        }

        // Additional tests for validation, mapping, and error handling can be added here.
    }
}
