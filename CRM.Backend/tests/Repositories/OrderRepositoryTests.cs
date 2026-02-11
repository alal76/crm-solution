// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

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
/// Unit tests for Order Repository
/// Covers: Order-specific queries, fulfillment, revenue
/// </summary>
public class OrderRepositoryTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<DbSet<OrderEntity>> _mockDbSet;
    private readonly Mock<ILogger<OrderRepository>> _mockLogger;
    private readonly OrderRepository _repository;

    public OrderRepositoryTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockDbSet = new Mock<DbSet<OrderEntity>>();
        _mockLogger = new Mock<ILogger<OrderRepository>>();

        _mockContext.Setup(c => c.Set<OrderEntity>()).Returns(_mockDbSet.Object);
        _repository = new OrderRepository(_mockContext.Object, _mockLogger.Object);
    }

    #region GetByStatus Tests

    [Fact]
    public async Task GetByStatusAsync_HasMatches_ReturnsOrders()
    {
        // Arrange
        var orders = new List<OrderEntity>
        {
            new OrderEntity { Id = 1, Status = "Pending" },
            new OrderEntity { Id = 2, Status = "Pending" },
            new OrderEntity { Id = 3, Status = "Shipped" }
        }.AsQueryable();

        SetupMockDbSet(orders);

        // Act
        var result = await _repository.GetByStatusAsync("Pending");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPendingAsync_ReturnsPendingOrders()
    {
        // Arrange
        var orders = new List<OrderEntity>
        {
            new OrderEntity { Id = 1, Status = "Pending" },
            new OrderEntity { Id = 2, Status = "Pending" },
            new OrderEntity { Id = 3, Status = "Completed" }
        }.AsQueryable();

        SetupMockDbSet(orders);

        // Act
        var result = await _repository.GetPendingAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetProcessingAsync_ReturnsProcessingOrders()
    {
        // Arrange
        var orders = new List<OrderEntity>
        {
            new OrderEntity { Id = 1, Status = "Processing" },
            new OrderEntity { Id = 2, Status = "Processing" },
            new OrderEntity { Id = 3, Status = "Pending" }
        }.AsQueryable();

        SetupMockDbSet(orders);

        // Act
        var result = await _repository.GetProcessingAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetShippedAsync_ReturnsShippedOrders()
    {
        // Arrange
        var orders = new List<OrderEntity>
        {
            new OrderEntity { Id = 1, Status = "Shipped" },
            new OrderEntity { Id = 2, Status = "Shipped" },
            new OrderEntity { Id = 3, Status = "Pending" }
        }.AsQueryable();

        SetupMockDbSet(orders);

        // Act
        var result = await _repository.GetShippedAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetDeliveredAsync_ReturnsDeliveredOrders()
    {
        // Arrange
        var orders = new List<OrderEntity>
        {
            new OrderEntity { Id = 1, Status = "Delivered" },
            new OrderEntity { Id = 2, Status = "Delivered" },
            new OrderEntity { Id = 3, Status = "Shipped" }
        }.AsQueryable();

        SetupMockDbSet(orders);

        // Act
        var result = await _repository.GetDeliveredAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetCancelledAsync_ReturnsCancelledOrders()
    {
        // Arrange
        var orders = new List<OrderEntity>
        {
            new OrderEntity { Id = 1, Status = "Cancelled" },
            new OrderEntity { Id = 2, Status = "Cancelled" },
            new OrderEntity { Id = 3, Status = "Pending" }
        }.AsQueryable();

        SetupMockDbSet(orders);

        // Act
        var result = await _repository.GetCancelledAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region GetByAccount Tests

    [Fact]
    public async Task GetByAccountAsync_HasOrders_ReturnsAccountOrders()
    {
        // Arrange
        var orders = new List<OrderEntity>
        {
            new OrderEntity { Id = 1, AccountId = 1 },
            new OrderEntity { Id = 2, AccountId = 1 },
            new OrderEntity { Id = 3, AccountId = 2 }
        }.AsQueryable();

        SetupMockDbSet(orders);

        // Act
        var result = await _repository.GetByAccountAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region GetByOrderNumber Tests

    [Fact]
    public async Task GetByOrderNumberAsync_Exists_ReturnsOrder()
    {
        // Arrange
        var orders = new List<OrderEntity>
        {
            new OrderEntity { Id = 1, OrderNumber = "ORD-001" },
            new OrderEntity { Id = 2, OrderNumber = "ORD-002" }
        }.AsQueryable();

        SetupMockDbSet(orders);

        // Act
        var result = await _repository.GetByOrderNumberAsync("ORD-001");

        // Assert
        result.Should().NotBeNull();
        result!.OrderNumber.Should().Be("ORD-001");
    }

    #endregion

    #region Revenue Tests

    [Fact]
    public async Task GetTotalRevenueAsync_CalculatesTotalRevenue()
    {
        // Arrange
        var orders = new List<OrderEntity>
        {
            new OrderEntity { Id = 1, TotalAmount = 10000, Status = "Delivered" },
            new OrderEntity { Id = 2, TotalAmount = 20000, Status = "Delivered" },
            new OrderEntity { Id = 3, TotalAmount = 30000, Status = "Delivered" }
        }.AsQueryable();

        SetupMockDbSet(orders);

        // Act
        var result = await _repository.GetTotalRevenueAsync();

        // Assert
        result.Should().Be(60000);
    }

    [Fact]
    public async Task GetRevenueByDateRangeAsync_ReturnsRevenueInRange()
    {
        // Arrange
        var orders = new List<OrderEntity>
        {
            new OrderEntity { Id = 1, TotalAmount = 10000, OrderDate = DateTime.UtcNow.AddDays(-5), Status = "Delivered" },
            new OrderEntity { Id = 2, TotalAmount = 20000, OrderDate = DateTime.UtcNow.AddDays(-10), Status = "Delivered" },
            new OrderEntity { Id = 3, TotalAmount = 30000, OrderDate = DateTime.UtcNow.AddDays(-40), Status = "Delivered" }
        }.AsQueryable();

        SetupMockDbSet(orders);

        // Act
        var result = await _repository.GetRevenueByDateRangeAsync(
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow);

        // Assert
        result.Should().Be(30000);
    }

    [Fact]
    public async Task GetAverageOrderValueAsync_CalculatesAverage()
    {
        // Arrange
        var orders = new List<OrderEntity>
        {
            new OrderEntity { Id = 1, TotalAmount = 10000 },
            new OrderEntity { Id = 2, TotalAmount = 20000 },
            new OrderEntity { Id = 3, TotalAmount = 30000 }
        }.AsQueryable();

        SetupMockDbSet(orders);

        // Act
        var result = await _repository.GetAverageOrderValueAsync();

        // Assert
        result.Should().Be(20000);
    }

    #endregion

    #region Date Range Tests

    [Fact]
    public async Task GetByDateRangeAsync_ReturnsOrdersInRange()
    {
        // Arrange
        var orders = new List<OrderEntity>
        {
            new OrderEntity { Id = 1, OrderDate = DateTime.UtcNow.AddDays(-5) },
            new OrderEntity { Id = 2, OrderDate = DateTime.UtcNow.AddDays(-15) },
            new OrderEntity { Id = 3, OrderDate = DateTime.UtcNow.AddDays(-40) }
        }.AsQueryable();

        SetupMockDbSet(orders);

        // Act
        var result = await _repository.GetByDateRangeAsync(
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetTodayAsync_ReturnsTodayOrders()
    {
        // Arrange
        var orders = new List<OrderEntity>
        {
            new OrderEntity { Id = 1, OrderDate = DateTime.UtcNow.Date },
            new OrderEntity { Id = 2, OrderDate = DateTime.UtcNow.Date },
            new OrderEntity { Id = 3, OrderDate = DateTime.UtcNow.AddDays(-5) }
        }.AsQueryable();

        SetupMockDbSet(orders);

        // Act
        var result = await _repository.GetTodayAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Payment Tests

    [Fact]
    public async Task GetPaidAsync_ReturnsPaidOrders()
    {
        // Arrange
        var orders = new List<OrderEntity>
        {
            new OrderEntity { Id = 1, PaymentStatus = "Paid" },
            new OrderEntity { Id = 2, PaymentStatus = "Paid" },
            new OrderEntity { Id = 3, PaymentStatus = "Pending" }
        }.AsQueryable();

        SetupMockDbSet(orders);

        // Act
        var result = await _repository.GetPaidAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetUnpaidAsync_ReturnsUnpaidOrders()
    {
        // Arrange
        var orders = new List<OrderEntity>
        {
            new OrderEntity { Id = 1, PaymentStatus = "Pending" },
            new OrderEntity { Id = 2, PaymentStatus = "Pending" },
            new OrderEntity { Id = 3, PaymentStatus = "Paid" }
        }.AsQueryable();

        SetupMockDbSet(orders);

        // Act
        var result = await _repository.GetUnpaidAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetOverduePaymentsAsync_ReturnsOverdueOrders()
    {
        // Arrange
        var orders = new List<OrderEntity>
        {
            new OrderEntity { Id = 1, PaymentDueDate = DateTime.UtcNow.AddDays(-5), PaymentStatus = "Pending" },
            new OrderEntity { Id = 2, PaymentDueDate = DateTime.UtcNow.AddDays(-1), PaymentStatus = "Pending" },
            new OrderEntity { Id = 3, PaymentDueDate = DateTime.UtcNow.AddDays(10), PaymentStatus = "Pending" }
        }.AsQueryable();

        SetupMockDbSet(orders);

        // Act
        var result = await _repository.GetOverduePaymentsAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Shipping Tests

    [Fact]
    public async Task GetReadyToShipAsync_ReturnsReadyOrders()
    {
        // Arrange
        var orders = new List<OrderEntity>
        {
            new OrderEntity { Id = 1, Status = "Processing", PaymentStatus = "Paid" },
            new OrderEntity { Id = 2, Status = "Processing", PaymentStatus = "Paid" },
            new OrderEntity { Id = 3, Status = "Processing", PaymentStatus = "Pending" }
        }.AsQueryable();

        SetupMockDbSet(orders);

        // Act
        var result = await _repository.GetReadyToShipAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByShippingMethodAsync_ReturnsOrdersByMethod()
    {
        // Arrange
        var orders = new List<OrderEntity>
        {
            new OrderEntity { Id = 1, ShippingMethod = "Express" },
            new OrderEntity { Id = 2, ShippingMethod = "Express" },
            new OrderEntity { Id = 3, ShippingMethod = "Standard" }
        }.AsQueryable();

        SetupMockDbSet(orders);

        // Act
        var result = await _repository.GetByShippingMethodAsync("Express");

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task SearchAsync_ByOrderNumber_ReturnsMatches()
    {
        // Arrange
        var orders = new List<OrderEntity>
        {
            new OrderEntity { Id = 1, OrderNumber = "ORD-2025-001" },
            new OrderEntity { Id = 2, OrderNumber = "ORD-2025-002" },
            new OrderEntity { Id = 3, OrderNumber = "ORD-2024-001" }
        }.AsQueryable();

        SetupMockDbSet(orders);

        // Act
        var result = await _repository.SearchAsync("2025");

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetCountByStatusAsync_ReturnsStatusCounts()
    {
        // Arrange
        var orders = new List<OrderEntity>
        {
            new OrderEntity { Id = 1, Status = "Pending" },
            new OrderEntity { Id = 2, Status = "Pending" },
            new OrderEntity { Id = 3, Status = "Shipped" }
        }.AsQueryable();

        SetupMockDbSet(orders);

        // Act
        var result = await _repository.GetCountByStatusAsync();

        // Assert
        result["Pending"].Should().Be(2);
    }

    [Fact]
    public async Task GetDailyRevenueAsync_ReturnsDailyRevenue()
    {
        // Arrange
        var orders = new List<OrderEntity>
        {
            new OrderEntity { Id = 1, OrderDate = DateTime.UtcNow.Date, TotalAmount = 10000, Status = "Delivered" },
            new OrderEntity { Id = 2, OrderDate = DateTime.UtcNow.Date, TotalAmount = 20000, Status = "Delivered" },
            new OrderEntity { Id = 3, OrderDate = DateTime.UtcNow.AddDays(-1).Date, TotalAmount = 15000, Status = "Delivered" }
        }.AsQueryable();

        SetupMockDbSet(orders);

        // Act
        var result = await _repository.GetDailyRevenueAsync(7);

        // Assert
        result.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task GetFulfillmentRateAsync_CalculatesRate()
    {
        // Arrange
        var orders = new List<OrderEntity>
        {
            new OrderEntity { Id = 1, Status = "Delivered" },
            new OrderEntity { Id = 2, Status = "Delivered" },
            new OrderEntity { Id = 3, Status = "Shipped" },
            new OrderEntity { Id = 4, Status = "Pending" }
        }.AsQueryable();

        SetupMockDbSet(orders);

        // Act
        var result = await _repository.GetFulfillmentRateAsync();

        // Assert
        result.Should().Be(50); // 2 delivered out of 4 = 50%
    }

    #endregion

    #region Priority Tests

    [Fact]
    public async Task GetPriorityOrdersAsync_ReturnsPriorityOrders()
    {
        // Arrange
        var orders = new List<OrderEntity>
        {
            new OrderEntity { Id = 1, Priority = "High" },
            new OrderEntity { Id = 2, Priority = "Urgent" },
            new OrderEntity { Id = 3, Priority = "Normal" }
        }.AsQueryable();

        SetupMockDbSet(orders);

        // Act
        var result = await _repository.GetPriorityOrdersAsync();

        // Assert
        result.Should().HaveCountGreaterThan(0);
    }

    #endregion

    #region Recent Activity Tests

    [Fact]
    public async Task GetRecentAsync_ReturnsRecentOrders()
    {
        // Arrange
        var orders = new List<OrderEntity>
        {
            new OrderEntity { Id = 1, OrderDate = DateTime.UtcNow.AddDays(-1) },
            new OrderEntity { Id = 2, OrderDate = DateTime.UtcNow.AddDays(-5) },
            new OrderEntity { Id = 3, OrderDate = DateTime.UtcNow.AddDays(-15) }
        }.AsQueryable();

        SetupMockDbSet(orders);

        // Act
        var result = await _repository.GetRecentAsync(7);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Bulk Operations Tests

    [Fact]
    public async Task BulkUpdateStatusAsync_UpdatesStatus()
    {
        // Arrange
        var orderIds = new[] { 1, 2, 3 };
        var newStatus = "Shipped";

        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(3);

        // Act
        var result = await _repository.BulkUpdateStatusAsync(orderIds, newStatus);

        // Assert
        result.Should().Be(3);
    }

    #endregion

    #region Helper Methods

    private void SetupMockDbSet(IQueryable<OrderEntity> data)
    {
        _mockDbSet.As<IQueryable<OrderEntity>>().Setup(m => m.Provider).Returns(data.Provider);
        _mockDbSet.As<IQueryable<OrderEntity>>().Setup(m => m.Expression).Returns(data.Expression);
        _mockDbSet.As<IQueryable<OrderEntity>>().Setup(m => m.ElementType).Returns(data.ElementType);
        _mockDbSet.As<IQueryable<OrderEntity>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
    }

    #endregion
}

// Supporting class
public class OrderEntity
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public string PaymentStatus { get; set; } = "Pending";
    public string? Priority { get; set; }
    public string? ShippingMethod { get; set; }
    public int? AccountId { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? PaymentDueDate { get; set; }
    public bool IsDeleted { get; set; }
}
