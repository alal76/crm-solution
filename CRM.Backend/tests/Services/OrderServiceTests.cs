// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.DTOs;
using CRM.Core.Entities;
using CRM.Core.Entities.Workflow;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Comprehensive unit tests for OrderService using InMemory database.
/// Covers CRUD operations, status management, fulfillment, line items,
/// statistics, and event dispatching.
/// </summary>
public class OrderServiceTests : IDisposable
{
    private readonly CrmDbContext _dbContext;
    private readonly Mock<ILogger<OrderService>> _mockLogger;
    private readonly Mock<IEntityEventDispatcher> _mockEventDispatcher;
    private readonly OrderService _service;

    public OrderServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: $"OrderServiceTests_{Guid.NewGuid()}")
            .Options;

        _dbContext = new CrmDbContext(options, null!);
        _mockLogger = new Mock<ILogger<OrderService>>();
        _mockEventDispatcher = new Mock<IEntityEventDispatcher>();
        _service = new OrderService(_dbContext, _mockLogger.Object, _mockEventDispatcher.Object);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    #region Helper Methods

    private CrmDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new CrmDbContext(options, null!);
    }

    private OrderService CreateService(CrmDbContext context)
    {
        var logger = new Mock<ILogger<OrderService>>();
        var eventDispatcher = new Mock<IEntityEventDispatcher>();
        return new OrderService(context, logger.Object, eventDispatcher.Object);
    }

    private async Task EnsureAccountExistsAsync(int accountId)
    {
        if (!await _dbContext.Accounts.AnyAsync(a => a.Id == accountId))
        {
            _dbContext.Accounts.Add(new Account
            {
                Id = accountId,
                Email = $"account{accountId}@test.com",
                Company = $"Test Company {accountId}",
                FirstName = "Test",
                LastName = "Account",
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            await _dbContext.SaveChangesAsync();
        }
    }

    private async Task<Order> SeedOrderAsync(
        int accountId = 10,
        string name = "Test Order",
        OrderStatus status = OrderStatus.Draft,
        decimal totalAmount = 2000m,
        bool isDeleted = false)
    {
        await EnsureAccountExistsAsync(accountId);

        var order = new Order
        {
            OrderNumber = $"ORD-{Guid.NewGuid().ToString()[..8]}",
            Name = name,
            Status = status,
            OrderType = OrderType.Standard,
            TotalAmount = totalAmount,
            Subtotal = totalAmount,
            AccountId = accountId,
            OrderDate = DateTime.UtcNow,
            IsDeleted = isDeleted,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();
        return order;
    }

    private async Task<Order> SeedOrderWithLineItemsAsync(
        int accountId = 10,
        OrderStatus status = OrderStatus.Draft)
    {
        await EnsureAccountExistsAsync(accountId);

        var order = new Order
        {
            OrderNumber = $"ORD-{Guid.NewGuid().ToString()[..8]}",
            Name = "Order With Items",
            Status = status,
            OrderType = OrderType.Standard,
            Subtotal = 300m,
            TotalAmount = 300m,
            AccountId = accountId,
            OrderDate = DateTime.UtcNow,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            LineItems = new List<OrderLineItem>
            {
                new OrderLineItem
                {
                    LineNumber = 1,
                    Name = "Widget A",
                    Quantity = 2,
                    UnitPrice = 100m,
                    TotalAmount = 200m,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new OrderLineItem
                {
                    LineNumber = 2,
                    Name = "Widget B",
                    Quantity = 1,
                    UnitPrice = 100m,
                    TotalAmount = 100m,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            }
        };
        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();
        return order;
    }

    #endregion

    // ========================================================================
    // Constructor Null Checks
    // ========================================================================

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenContextIsNull()
    {
        // Act
        var act = () => new OrderService(null!, _mockLogger.Object, _mockEventDispatcher.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("context");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
    {
        // Act
        var act = () => new OrderService(_dbContext, null!, _mockEventDispatcher.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenEventDispatcherIsNull()
    {
        // Act
        var act = () => new OrderService(_dbContext, _mockLogger.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("eventDispatcher");
    }

    // ========================================================================
    // GetAllAsync
    // ========================================================================

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllNonDeletedOrders_WhenNoFilters()
    {
        // Arrange
        await SeedOrderAsync(name: "Order 1");
        await SeedOrderAsync(name: "Order 2");
        await SeedOrderAsync(name: "Order 3");

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllAsync_ShouldExcludeDeletedOrders()
    {
        // Arrange
        await SeedOrderAsync(name: "Active Order");
        await SeedOrderAsync(name: "Deleted Order", isDeleted: true);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Active Order");
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByAccountId()
    {
        // Arrange
        await SeedOrderAsync(accountId: 10, name: "Account 10 Order 1");
        await SeedOrderAsync(accountId: 20, name: "Account 20 Order");
        await SeedOrderAsync(accountId: 10, name: "Account 10 Order 2");

        // Act
        var result = await _service.GetAllAsync(accountId: 10);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(o => o.AccountId.Should().Be(10));
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByStatus()
    {
        // Arrange
        await SeedOrderAsync(status: OrderStatus.Draft);
        await SeedOrderAsync(status: OrderStatus.Approved);
        await SeedOrderAsync(status: OrderStatus.Draft);

        // Act
        var result = await _service.GetAllAsync(status: OrderStatus.Draft);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(o => o.Status.Should().Be((int)OrderStatus.Draft));
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmpty_WhenNoMatchingOrders()
    {
        // Arrange
        await SeedOrderAsync(accountId: 10);

        // Act
        var result = await _service.GetAllAsync(accountId: 999);

        // Assert
        result.Should().BeEmpty();
    }

    // ========================================================================
    // GetByIdAsync
    // ========================================================================

    [Fact]
    public async Task GetByIdAsync_ShouldReturnOrder_WhenExists()
    {
        // Arrange
        var order = await SeedOrderAsync(name: "Findable Order");

        // Act
        var result = await _service.GetByIdAsync(order.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(order.Id);
        result.Name.Should().Be("Findable Order");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenOrderIsDeleted()
    {
        // Arrange
        var order = await SeedOrderAsync(isDeleted: true);

        // Act
        var result = await _service.GetByIdAsync(order.Id);

        // Assert
        result.Should().BeNull();
    }

    // ========================================================================
    // GetByOrderNumberAsync
    // ========================================================================

    [Fact]
    public async Task GetByOrderNumberAsync_ShouldReturnOrder_WhenExists()
    {
        // Arrange
        var order = await SeedOrderAsync();

        // Act
        var result = await _service.GetByOrderNumberAsync(order.OrderNumber);

        // Assert
        result.Should().NotBeNull();
        result!.OrderNumber.Should().Be(order.OrderNumber);
    }

    [Fact]
    public async Task GetByOrderNumberAsync_ShouldReturnNull_WhenNotFound()
    {
        // Act
        var result = await _service.GetByOrderNumberAsync("NONEXISTENT-0001");

        // Assert
        result.Should().BeNull();
    }

    // ========================================================================
    // CreateAsync
    // ========================================================================

    [Fact]
    public async Task CreateAsync_ShouldCreateOrder_WithValidDto()
    {
        // Arrange
        var dto = new CreateOrderDto
        {
            AccountId = 10,
            Name = "New Order",
            OrderType = (int)OrderType.Standard,
            OrderDate = DateTime.UtcNow.ToString("o")
        };

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Order");
        result.AccountId.Should().Be(10);
        result.OrderType.Should().Be((int)OrderType.Standard);
    }

    [Fact]
    public async Task CreateAsync_ShouldGenerateOrderNumber_WhenNotProvided()
    {
        // Arrange
        var dto = new CreateOrderDto
        {
            AccountId = 10,
            Name = "Auto Number Order",
            OrderType = (int)OrderType.Standard,
            OrderDate = DateTime.UtcNow.ToString("o")
        };

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        result.OrderNumber.Should().NotBeNullOrEmpty();
        result.OrderNumber.Should().StartWith("ORD-");
    }

    [Fact]
    public async Task CreateAsync_ShouldSetTimestamps()
    {
        // Arrange
        var dto = new CreateOrderDto
        {
            AccountId = 10,
            Name = "Timestamped Order",
            OrderType = (int)OrderType.Standard,
            OrderDate = DateTime.UtcNow.ToString("o")
        };

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        result.CreatedAt.Should().NotBeNullOrEmpty();
        result.UpdatedAt.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistToDatabase()
    {
        // Arrange
        var dto = new CreateOrderDto
        {
            AccountId = 10,
            Name = "Persisted Order",
            OrderType = (int)OrderType.Renewal,
            OrderDate = DateTime.UtcNow.ToString("o")
        };

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        var fromDb = await _dbContext.Orders.FindAsync(result.Id);
        fromDb.Should().NotBeNull();
        fromDb!.Name.Should().Be("Persisted Order");
        fromDb.OrderType.Should().Be(OrderType.Renewal);
    }

    [Fact]
    public async Task CreateAsync_ShouldDispatchCreateEvent()
    {
        // Arrange
        var dto = new CreateOrderDto
        {
            AccountId = 10,
            Name = "Event Order",
            OrderType = (int)OrderType.Standard,
            OrderDate = DateTime.UtcNow.ToString("o")
        };

        // Act
        await _service.CreateAsync(dto);

        // Assert
        _mockEventDispatcher.Verify(
            d => d.DispatchEntityEventAsync(
                "Order",
                It.IsAny<int>(),
                WorkflowTriggerType.OnCreate,
                It.IsAny<int?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ========================================================================
    // UpdateAsync
    // ========================================================================

    [Fact]
    public async Task UpdateAsync_ShouldUpdateProperties()
    {
        // Arrange
        var order = await SeedOrderAsync(name: "Original Name");

        var dto = new UpdateOrderDto
        {
            Id = order.Id,
            Name = "Updated Name",
            Description = "New description"
        };

        // Act
        var result = await _service.UpdateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenOrderNotFound()
    {
        // Arrange
        var dto = new UpdateOrderDto { Id = 999, Name = "Won't Work" };

        // Act
        var act = () => _service.UpdateAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*999*not found*");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateStatusViaDto()
    {
        // Arrange
        var order = await SeedOrderAsync(status: OrderStatus.Draft);
        var dto = new UpdateOrderDto
        {
            Id = order.Id,
            Status = (int)OrderStatus.Processing
        };

        // Act
        var result = await _service.UpdateAsync(dto);

        // Assert
        result.Status.Should().Be((int)OrderStatus.Processing);
    }

    // ========================================================================
    // DeleteAsync
    // ========================================================================

    [Fact]
    public async Task DeleteAsync_ShouldSoftDelete_WhenOrderExists()
    {
        // Arrange
        var order = await SeedOrderAsync();

        // Act
        var result = await _service.DeleteAsync(order.Id);

        // Assert
        result.Should().BeTrue();
        var fromDb = await _dbContext.Orders.FindAsync(order.Id);
        fromDb!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenOrderNotFound()
    {
        // Act
        var result = await _service.DeleteAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenAlreadyDeleted()
    {
        // Arrange
        var order = await SeedOrderAsync(isDeleted: true);

        // Act
        var result = await _service.DeleteAsync(order.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDispatchDeleteEvent()
    {
        // Arrange
        var order = await SeedOrderAsync();

        // Act
        await _service.DeleteAsync(order.Id);

        // Assert
        _mockEventDispatcher.Verify(
            d => d.DispatchEntityEventAsync(
                "Order",
                order.Id,
                WorkflowTriggerType.OnDelete,
                It.IsAny<int?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ========================================================================
    // UpdateStatusAsync
    // ========================================================================

    [Fact]
    public async Task UpdateStatusAsync_ShouldChangeStatus()
    {
        // Arrange
        var order = await SeedOrderAsync(status: OrderStatus.Draft);

        // Act
        var result = await _service.UpdateStatusAsync(order.Id, OrderStatus.Processing);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.Processing);
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldThrow_WhenOrderNotFound()
    {
        // Act
        var act = () => _service.UpdateStatusAsync(999, OrderStatus.Approved);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ========================================================================
    // SubmitForApprovalAsync
    // ========================================================================

    [Fact]
    public async Task SubmitForApprovalAsync_ShouldSetPendingApprovalAndSubmittedDate_WhenDraft()
    {
        // Arrange
        var order = await SeedOrderAsync(status: OrderStatus.Draft);

        // Act
        var result = await _service.SubmitForApprovalAsync(order.Id);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.PendingApproval);
        result.SubmittedDate.Should().NotBeNull();
        result.SubmittedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task SubmitForApprovalAsync_ShouldThrow_WhenNotInDraftStatus()
    {
        // Arrange
        var order = await SeedOrderAsync(status: OrderStatus.Approved);

        // Act
        var act = () => _service.SubmitForApprovalAsync(order.Id);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Draft*");
    }

    [Fact]
    public async Task SubmitForApprovalAsync_ShouldThrow_WhenOrderNotFound()
    {
        // Act
        var act = () => _service.SubmitForApprovalAsync(999);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ========================================================================
    // ApproveAsync
    // ========================================================================

    [Fact]
    public async Task ApproveAsync_ShouldSetApprovedStatusAndDetails()
    {
        // Arrange
        var order = await SeedOrderAsync(status: OrderStatus.PendingApproval);

        // Act
        var result = await _service.ApproveAsync(order.Id, approvedById: 5);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.Approved);
        result.ApprovedById.Should().Be(5);
        result.ApprovedDate.Should().NotBeNull();
        result.ApprovedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ApproveAsync_ShouldThrow_WhenOrderNotFound()
    {
        // Act
        var act = () => _service.ApproveAsync(999, approvedById: 5);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ========================================================================
    // RejectAsync
    // ========================================================================

    [Fact]
    public async Task RejectAsync_ShouldSetCancelledStatusAndRejectionReason()
    {
        // Arrange
        var order = await SeedOrderAsync(status: OrderStatus.PendingApproval);

        // Act
        var result = await _service.RejectAsync(order.Id, rejectedById: 5, reason: "Budget exceeded");

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.Cancelled);
        result.RejectionReason.Should().Be("Budget exceeded");
    }

    [Fact]
    public async Task RejectAsync_ShouldThrow_WhenOrderNotFound()
    {
        // Act
        var act = () => _service.RejectAsync(999, rejectedById: 5, reason: "N/A");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ========================================================================
    // CancelAsync
    // ========================================================================

    [Fact]
    public async Task CancelAsync_ShouldSetCancelledStatusAndDetails()
    {
        // Arrange
        var order = await SeedOrderAsync(status: OrderStatus.Approved);

        // Act
        var result = await _service.CancelAsync(order.Id, "Customer request");

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.Cancelled);
        result.CancellationReason.Should().Be("Customer request");
        result.CancelledDate.Should().NotBeNull();
        result.CancelledDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CancelAsync_ShouldThrow_WhenOrderIsFulfilled()
    {
        // Arrange
        var order = await SeedOrderAsync(status: OrderStatus.Fulfilled);

        // Act
        var act = () => _service.CancelAsync(order.Id, "Too late");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*cancel*fulfilled*delivered*");
    }

    [Fact]
    public async Task CancelAsync_ShouldThrow_WhenOrderIsDelivered()
    {
        // Arrange
        var order = await SeedOrderAsync(status: OrderStatus.Delivered);

        // Act
        var act = () => _service.CancelAsync(order.Id, "Too late");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ========================================================================
    // PutOnHoldAsync / ReleaseFromHoldAsync
    // ========================================================================

    [Fact]
    public async Task PutOnHoldAsync_ShouldSetOnHoldStatusAndReason()
    {
        // Arrange
        var order = await SeedOrderAsync(status: OrderStatus.Processing);

        // Act
        var result = await _service.PutOnHoldAsync(order.Id, "Pending credit check");

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.OnHold);
        result.HoldReason.Should().Be("Pending credit check");
        result.HoldDate.Should().NotBeNull();
    }

    [Fact]
    public async Task ReleaseFromHoldAsync_ShouldSetProcessingStatus_WhenOnHold()
    {
        // Arrange
        var order = await SeedOrderAsync(status: OrderStatus.OnHold);

        // Act
        var result = await _service.ReleaseFromHoldAsync(order.Id);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.Processing);
        result.HoldReason.Should().BeNull();
    }

    [Fact]
    public async Task ReleaseFromHoldAsync_ShouldThrow_WhenNotOnHold()
    {
        // Arrange
        var order = await SeedOrderAsync(status: OrderStatus.Draft);

        // Act
        var act = () => _service.ReleaseFromHoldAsync(order.Id);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not on hold*");
    }

    // ========================================================================
    // MarkAsFulfilledAsync / MarkAsDeliveredAsync
    // ========================================================================

    [Fact]
    public async Task MarkAsFulfilledAsync_ShouldSetFulfilledStatusAndDate()
    {
        // Arrange
        var order = await SeedOrderWithLineItemsAsync(status: OrderStatus.Processing);

        // Act
        var result = await _service.MarkAsFulfilledAsync(order.Id);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.Fulfilled);
        result.FulfilledDate.Should().NotBeNull();
    }

    [Fact]
    public async Task MarkAsDeliveredAsync_ShouldSetDeliveredStatusAndDate()
    {
        // Arrange
        var order = await SeedOrderAsync(status: OrderStatus.Fulfilled);

        // Act
        var result = await _service.MarkAsDeliveredAsync(order.Id);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.Delivered);
        result.DeliveredDate.Should().NotBeNull();
    }

    [Fact]
    public async Task MarkAsDeliveredAsync_ShouldUseProvidedDeliveryDate()
    {
        // Arrange
        var order = await SeedOrderAsync(status: OrderStatus.Fulfilled);
        var specificDate = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);

        // Act
        var result = await _service.MarkAsDeliveredAsync(order.Id, deliveryDate: specificDate);

        // Assert
        result.DeliveredDate.Should().Be(specificDate);
    }

    // ========================================================================
    // GetStatisticsAsync
    // ========================================================================

    [Fact]
    public async Task GetStatisticsAsync_ShouldReturnCorrectCounts()
    {
        // Arrange
        await SeedOrderAsync(status: OrderStatus.Draft, totalAmount: 1000m);
        await SeedOrderAsync(status: OrderStatus.PendingApproval, totalAmount: 2000m);
        await SeedOrderAsync(status: OrderStatus.Approved, totalAmount: 3000m);
        await SeedOrderAsync(status: OrderStatus.Fulfilled, totalAmount: 4000m);
        await SeedOrderAsync(status: OrderStatus.Cancelled, totalAmount: 500m);

        // Act
        var stats = await _service.GetStatisticsAsync();

        // Assert
        stats.Should().NotBeNull();
        stats.TotalOrders.Should().Be(5);
        stats.PendingOrders.Should().Be(2); // Draft + PendingApproval
        stats.ProcessingOrders.Should().Be(1); // Approved
        stats.FulfilledOrders.Should().Be(1); // Fulfilled
        stats.CancelledOrders.Should().Be(1);
    }

    [Fact]
    public async Task GetStatisticsAsync_ShouldCalculateTotalRevenue_FromFulfilledOrders()
    {
        // Arrange
        await SeedOrderAsync(status: OrderStatus.Fulfilled, totalAmount: 1000m);
        await SeedOrderAsync(status: OrderStatus.Delivered, totalAmount: 2000m);
        await SeedOrderAsync(status: OrderStatus.Completed, totalAmount: 3000m);
        await SeedOrderAsync(status: OrderStatus.Draft, totalAmount: 9999m);

        // Act
        var stats = await _service.GetStatisticsAsync();

        // Assert
        stats.TotalRevenue.Should().Be(6000m); // 1000 + 2000 + 3000
    }

    [Fact]
    public async Task GetStatisticsAsync_ShouldReturnZeros_WhenNoOrders()
    {
        // Act
        var stats = await _service.GetStatisticsAsync();

        // Assert
        stats.TotalOrders.Should().Be(0);
        stats.TotalRevenue.Should().Be(0);
        stats.AverageOrderValue.Should().Be(0);
        stats.FulfillmentRate.Should().Be(0);
    }

    // ========================================================================
    // GenerateOrderNumberAsync
    // ========================================================================

    [Fact]
    public async Task GenerateOrderNumberAsync_ShouldReturnFormattedOrderNumber()
    {
        // Act
        var result = await _service.GenerateOrderNumberAsync();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().StartWith("ORD-");
        result.Should().MatchRegex(@"^ORD-\d{4}-\d{4}$");
    }

    // ========================================================================
    // CloneOrderAsync
    // ========================================================================

    [Fact]
    public async Task CloneOrderAsync_ShouldCreateCopyWithDraftStatus()
    {
        // Arrange
        var order = await SeedOrderAsync(status: OrderStatus.Approved, totalAmount: 5000m, accountId: 42);

        // Act
        var result = await _service.CloneOrderAsync(order.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBe(order.Id);
        result.Status.Should().Be(OrderStatus.Draft);
        result.AccountId.Should().Be(42);
        result.TotalAmount.Should().Be(5000m);
        result.OrderNumber.Should().NotBe(order.OrderNumber);
    }

    [Fact]
    public async Task CloneOrderAsync_ShouldThrow_WhenOrderNotFound()
    {
        // Act
        var act = () => _service.CloneOrderAsync(999);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ========================================================================
    // RecalculateTotalsAsync
    // ========================================================================

    [Fact]
    public async Task RecalculateTotalsAsync_ShouldUpdateOrderTotals()
    {
        // Arrange
        var order = await SeedOrderWithLineItemsAsync();

        // Act
        var result = await _service.RecalculateTotalsAsync(order.Id);

        // Assert
        result.Should().NotBeNull();
        // Subtotal = (2 * 100) + (1 * 100) = 300
        result.Subtotal.Should().Be(300m);
    }

    // ========================================================================
    // ApplyDiscountAsync
    // ========================================================================

    [Fact]
    public async Task ApplyDiscountAsync_ShouldApplyDiscountAmount()
    {
        // Arrange
        var order = await SeedOrderAsync(totalAmount: 1000m);

        // Act
        var result = await _service.ApplyDiscountAsync(order.Id, 100m, "SAVE10");

        // Assert
        result.Should().NotBeNull();
        result.DiscountAmount.Should().Be(100m);
        result.DiscountCode.Should().Be("SAVE10");
    }

    // ========================================================================
    // ApplyCouponAsync
    // ========================================================================

    [Fact]
    public async Task ApplyCouponAsync_ShouldStoreCouponCode()
    {
        // Arrange
        var order = await SeedOrderAsync();

        // Act
        var result = await _service.ApplyCouponAsync(order.Id, "SUMMER2026");

        // Assert
        result.Should().NotBeNull();
        result.CouponCode.Should().Be("SUMMER2026");
    }
}
