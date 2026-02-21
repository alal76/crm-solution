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
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

public class OrderServiceTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<OrderService>> _mockLogger;
    private readonly Mock<IEntityEventDispatcher> _mockEventDispatcher;
    private readonly OrderService _service;

    public OrderServiceTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<OrderService>>();
        _mockEventDispatcher = new Mock<IEntityEventDispatcher>();
        _service = new OrderService(_mockContext.Object, _mockLogger.Object, _mockEventDispatcher.Object);
    }

    private void SetupDbSets(
        List<Order>? orders = null,
        List<OrderLineItem>? lineItems = null,
        List<Quote>? quotes = null,
        List<Opportunity>? opportunities = null,
        List<Invoice>? invoices = null)
    {
        orders ??= new List<Order>();
        lineItems ??= new List<OrderLineItem>();
        quotes ??= new List<Quote>();
        opportunities ??= new List<Opportunity>();
        invoices ??= new List<Invoice>();

        var mockOrders = MockDbSetFactory.CreateMockDbSet(orders);
        mockOrders.Setup(m => m.Add(It.IsAny<Order>())).Callback<Order>(e => orders.Add(e));
        mockOrders.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns<object[], CancellationToken>((keys, _) => mockOrders.Object.FindAsync(keys));
        _mockContext.Setup(c => c.Orders).Returns(mockOrders.Object);

        var mockLineItems = MockDbSetFactory.CreateMockDbSet(lineItems);
        mockLineItems.Setup(m => m.Add(It.IsAny<OrderLineItem>())).Callback<OrderLineItem>(e => lineItems.Add(e));
        mockLineItems.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns<object[], CancellationToken>((keys, _) => mockLineItems.Object.FindAsync(keys));
        _mockContext.Setup(c => c.OrderLineItems).Returns(mockLineItems.Object);

        var mockQuotes = MockDbSetFactory.CreateMockDbSet(quotes);
        _mockContext.Setup(c => c.Quotes).Returns(mockQuotes.Object);

        var mockOpportunities = MockDbSetFactory.CreateMockDbSet(opportunities);
        _mockContext.Setup(c => c.Opportunities).Returns(mockOpportunities.Object);

        var mockInvoices = MockDbSetFactory.CreateMockDbSet(invoices);
        mockInvoices.Setup(m => m.Add(It.IsAny<Invoice>())).Callback<Invoice>(e => invoices.Add(e));
        _mockContext.Setup(c => c.Invoices).Returns(mockInvoices.Object);

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    private static Order CreateTestOrder(
        int id = 1,
        OrderStatus status = OrderStatus.Draft,
        decimal totalAmount = 2000m,
        int accountId = 10)
    {
        return new Order
        {
            Id = id,
            OrderNumber = $"ORD-{id:D4}",
            Name = $"Order {id}",
            Status = status,
            TotalAmount = totalAmount,
            Subtotal = totalAmount,
            AccountId = accountId,
            OrderDate = DateTime.UtcNow,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    // ========================================================================
    // GetAllAsync
    // ========================================================================
    [Fact]
    public async Task GetAllAsync_ShouldReturnAllOrders_WhenNoFilter()
    {
        // Arrange
        var orders = new List<Order>
        {
            CreateTestOrder(1),
            CreateTestOrder(2),
            CreateTestOrder(3)
        };
        SetupDbSets(orders: orders);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByCustomerId()
    {
        // Arrange
        var orders = new List<Order>
        {
            CreateTestOrder(1, accountId: 10),
            CreateTestOrder(2, accountId: 20),
            CreateTestOrder(3, accountId: 10)
        };
        SetupDbSets(orders: orders);

        // Act
        var result = await _service.GetAllAsync(accountId: 10);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_ShouldExcludeDeleted()
    {
        // Arrange
        var orders = new List<Order>
        {
            CreateTestOrder(1),
            new Order { Id = 2, OrderNumber = "DEL", IsDeleted = true, CreatedAt = DateTime.UtcNow }
        };
        SetupDbSets(orders: orders);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(1);
    }

    // ========================================================================
    // GetByIdAsync
    // ========================================================================
    [Fact]
    public async Task GetByIdAsync_ShouldReturnOrder_WhenExists()
    {
        // Arrange
        SetupDbSets(orders: new List<Order> { CreateTestOrder(1) });

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        SetupDbSets(orders: new List<Order>());

        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    // ========================================================================
    // CreateAsync + Event Dispatch
    // ========================================================================
    [Fact]
    public async Task CreateAsync_ShouldSetTimestampsAndDispatchEvent()
    {
        // Arrange
        var orders = new List<Order>();
        SetupDbSets(orders: orders);

        var newOrder = new CreateOrderDto
        {
            Name = "New Order",
            AccountId = 10,
            OrderDate = DateTime.UtcNow.ToString("o")
        };

        // Act
        var result = await _service.CreateAsync(newOrder);

        // Assert
        result.Should().NotBeNull();
        result.CreatedAt.Should().NotBeNullOrEmpty();
        result.OrderNumber.Should().NotBeNullOrEmpty();
        _mockEventDispatcher.Verify(d => d.DispatchEntityEvent(
            "Order", It.IsAny<int>(), WorkflowTriggerType.OnCreate,
            It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>()), Times.Once);
    }

    // ========================================================================
    // DeleteAsync + Event Dispatch
    // ========================================================================
    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteAndDispatchEvent()
    {
        // Arrange
        var order = CreateTestOrder(1);
        SetupDbSets(orders: new List<Order> { order });

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        result.Should().BeTrue();
        order.IsDeleted.Should().BeTrue();
        _mockEventDispatcher.Verify(d => d.DispatchEntityEvent(
            "Order", 1, WorkflowTriggerType.OnDelete,
            It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>()), Times.Once);
    }

    // ========================================================================
    // SubmitForApprovalAsync
    // ========================================================================
    [Fact]
    public async Task SubmitForApprovalAsync_ShouldSetPendingApproval_WhenDraft()
    {
        // Arrange
        var order = CreateTestOrder(1, status: OrderStatus.Draft);
        SetupDbSets(orders: new List<Order> { order });

        // Act
        var result = await _service.SubmitForApprovalAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.PendingApproval);
        result.SubmittedDate.Should().NotBeNull();
    }

    [Fact]
    public async Task SubmitForApprovalAsync_ShouldThrow_WhenNotDraft()
    {
        // Arrange
        var order = CreateTestOrder(1, status: OrderStatus.Approved);
        SetupDbSets(orders: new List<Order> { order });

        // Act
        var act = () => _service.SubmitForApprovalAsync(1);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ========================================================================
    // ApproveAsync / RejectAsync
    // ========================================================================
    [Fact]
    public async Task ApproveAsync_ShouldSetApprovedStatus()
    {
        // Arrange
        var order = CreateTestOrder(1, status: OrderStatus.PendingApproval);
        SetupDbSets(orders: new List<Order> { order });

        // Act
        var result = await _service.ApproveAsync(1, approvedById: 5);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.Approved);
        result.ApprovedById.Should().Be(5);
        result.ApprovedDate.Should().NotBeNull();
    }

    [Fact]
    public async Task RejectAsync_ShouldSetCancelledWithReason()
    {
        // Arrange
        var order = CreateTestOrder(1, status: OrderStatus.PendingApproval);
        SetupDbSets(orders: new List<Order> { order });

        // Act
        var result = await _service.RejectAsync(1, rejectedById: 5, reason: "Budget exceeded");

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.Cancelled);
        result.RejectionReason.Should().Be("Budget exceeded");
    }

    // ========================================================================
    // CancelAsync
    // ========================================================================
    [Fact]
    public async Task CancelAsync_ShouldCancel_WhenNotFulfilledOrDelivered()
    {
        // Arrange
        var order = CreateTestOrder(1, status: OrderStatus.Approved);
        SetupDbSets(orders: new List<Order> { order });

        // Act
        var result = await _service.CancelAsync(1, "Customer request");

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.Cancelled);
        result.CancellationReason.Should().Be("Customer request");
    }

    [Fact]
    public async Task CancelAsync_ShouldThrow_WhenAlreadyFulfilled()
    {
        // Arrange
        var order = CreateTestOrder(1, status: OrderStatus.Fulfilled);
        SetupDbSets(orders: new List<Order> { order });

        // Act
        var act = () => _service.CancelAsync(1, "Trying to cancel");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ========================================================================
    // PutOnHoldAsync / ReleaseFromHoldAsync
    // ========================================================================
    [Fact]
    public async Task PutOnHoldAsync_ShouldSetOnHoldStatus()
    {
        // Arrange
        var order = CreateTestOrder(1, status: OrderStatus.Approved);
        SetupDbSets(orders: new List<Order> { order });

        // Act
        var result = await _service.PutOnHoldAsync(1, "Pending credit check");

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.OnHold);
        result.HoldReason.Should().Be("Pending credit check");
    }

    [Fact]
    public async Task ReleaseFromHoldAsync_ShouldRelease_WhenOnHold()
    {
        // Arrange
        var order = CreateTestOrder(1, status: OrderStatus.OnHold);
        SetupDbSets(orders: new List<Order> { order });

        // Act
        var result = await _service.ReleaseFromHoldAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().NotBe(OrderStatus.OnHold);
    }

    [Fact]
    public async Task ReleaseFromHoldAsync_ShouldThrow_WhenNotOnHold()
    {
        // Arrange
        var order = CreateTestOrder(1, status: OrderStatus.Draft);
        SetupDbSets(orders: new List<Order> { order });

        // Act
        var act = () => _service.ReleaseFromHoldAsync(1);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ========================================================================
    // MarkAsFulfilledAsync / MarkAsDeliveredAsync
    // ========================================================================
    [Fact]
    public async Task MarkAsFulfilledAsync_ShouldSetFulfilledStatus()
    {
        // Arrange
        var order = CreateTestOrder(1, status: OrderStatus.Approved);
        SetupDbSets(orders: new List<Order> { order });

        // Act
        var result = await _service.MarkAsFulfilledAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.Fulfilled);
        result.FulfilledDate.Should().NotBeNull();
    }

    [Fact]
    public async Task MarkAsDeliveredAsync_ShouldSetDeliveredStatus()
    {
        // Arrange
        var order = CreateTestOrder(1, status: OrderStatus.Fulfilled);
        SetupDbSets(orders: new List<Order> { order });

        // Act
        var result = await _service.MarkAsDeliveredAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.Delivered);
        result.DeliveredDate.Should().NotBeNull();
    }

    // ========================================================================
    // Line Items
    // ========================================================================
    [Fact]
    public async Task AddLineItemAsync_ShouldAddToOrder()
    {
        // Arrange
        var order = CreateTestOrder(1);
        var lineItems = new List<OrderLineItem>();
        SetupDbSets(orders: new List<Order> { order }, lineItems: lineItems);

        var lineItem = new OrderLineItem
        {
            Name = "Widget",
            Quantity = 5,
            UnitPrice = 100m,
            TotalAmount = 500m
        };

        // Act
        var result = await _service.AddLineItemAsync(1, lineItem);

        // Assert
        result.Should().NotBeNull();
        result.OrderId.Should().Be(1);
        result.Name.Should().Be("Widget");
    }

    [Fact]
    public async Task RemoveLineItemAsync_ShouldSoftDeleteLineItem()
    {
        // Arrange
        var lineItem = new OrderLineItem
        {
            Id = 1,
            OrderId = 1,
            Name = "Widget",
            Quantity = 1,
            UnitPrice = 100m,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        // RemoveLineItemAsync calls RecalculateTotalsAsync which uses .Include(o => o.LineItems)
        var order = CreateTestOrder(1);
        order.LineItems = new List<OrderLineItem> { lineItem };
        SetupDbSets(orders: new List<Order> { order }, lineItems: new List<OrderLineItem> { lineItem });

        // Act
        var result = await _service.RemoveLineItemAsync(1);

        // Assert
        result.Should().BeTrue();
        lineItem.IsDeleted.Should().BeTrue();
    }

    // ========================================================================
    // GenerateOrderNumberAsync
    // ========================================================================
    [Fact]
    public async Task GenerateOrderNumberAsync_ShouldReturnFormattedNumber()
    {
        // Arrange
        SetupDbSets();

        // Act
        var result = await _service.GenerateOrderNumberAsync();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().StartWith("ORD-");
    }

    // ========================================================================
    // CreateInvoiceAsync
    // ========================================================================
    [Fact]
    public async Task CreateInvoiceAsync_ShouldCreateInvoiceFromOrder()
    {
        // Arrange
        var order = CreateTestOrder(1, status: OrderStatus.Approved, totalAmount: 3000m);
        var invoices = new List<Invoice>();
        SetupDbSets(orders: new List<Order> { order }, invoices: invoices);

        // Act
        var result = await _service.CreateInvoiceAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.OrderId.Should().Be(1);
        result.TotalAmount.Should().Be(3000m);
    }

    // ========================================================================
    // CloneOrderAsync
    // ========================================================================
    [Fact]
    public async Task CloneOrderAsync_ShouldCreateCopyWithDraftStatus()
    {
        // Arrange
        var order = CreateTestOrder(1, status: OrderStatus.Approved, totalAmount: 2000m);
        var orders = new List<Order> { order };
        SetupDbSets(orders: orders);

        // Act
        var result = await _service.CloneOrderAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.Draft);
        result.AccountId.Should().Be(order.AccountId);
    }
}
