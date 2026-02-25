// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
//
// Spec: SPEC-SALES-003 (Order Returns Workflow)
// TODO-GAP-SALES-001: Complete Order Returns Workflow
//
// MANDATORY TEST RULE: All method signatures, namespaces, and field names
// verified against the actual source before writing these tests.
// Source files read: OrderReturnService.cs, IOrderReturnService.cs,
//   OrderReturn.cs, OrderReturnDtos.cs, OrderReturnsController.cs,
//   ICrmDbContext.cs

using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for OrderReturnService — covers complete status transition workflow and CRUD operations.
/// Status transitions: Pending → Approved/Rejected, Approved → Received, Received → Refunded → Completed,
/// any → Cancelled, as well as soft delete and statistics generation.
/// </summary>
public class OrderReturnServiceTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<OrderReturnService>> _mockLogger;
    private readonly OrderReturnService _service;

    public OrderReturnServiceTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<OrderReturnService>>();
        _service = new OrderReturnService(_mockContext.Object, _mockLogger.Object);
    }

    // ────────────────────────────────────────────────────────────────────────
    // Setup Helpers
    // ────────────────────────────────────────────────────────────────────────

    private void SetupDbSets(
        List<OrderReturn>? orderReturns = null,
        List<Order>? orders = null)
    {
        orderReturns ??= [];
        orders ??= [];

        var mockReturns = MockDbSetFactory.CreateMockDbSet(orderReturns);
        mockReturns.Setup(m => m.Add(It.IsAny<OrderReturn>()))
            .Callback<OrderReturn>(e => orderReturns.Add(e));
        mockReturns.Setup(m => m.Update(It.IsAny<OrderReturn>()));
        mockReturns.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns<object[], CancellationToken>((keys, _) =>
                new ValueTask<OrderReturn?>(orderReturns.FirstOrDefault(r => r.Id == Convert.ToInt32(keys[0]))));
        _mockContext.Setup(c => c.OrderReturns).Returns(mockReturns.Object);

        var mockOrders = MockDbSetFactory.CreateMockDbSet(orders);
        _mockContext.Setup(c => c.Orders).Returns(mockOrders.Object);

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    private static OrderReturn CreateTestReturn(
        int id = 1,
        OrderReturnStatus status = OrderReturnStatus.Pending,
        int orderId = 10,
        decimal refundAmount = 100m,
        bool isDeleted = false)
    {
        return new OrderReturn
        {
            Id = id,
            ReturnNumber = $"RET-{id:D4}",
            OrderId = orderId,
            AccountId = 20,
            Status = status,
            Reason = OrderReturnReason.Defective,
            OriginalAmount = 200m,
            RefundAmount = refundAmount,
            RestockingFee = 0m,
            ShippingRefund = 0m,
            Currency = "USD",
            RequestedAt = DateTime.UtcNow,
            IsDeleted = isDeleted,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static Order CreateTestOrder(int id = 10, decimal totalAmount = 200m)
    {
        return new Order
        {
            Id = id,
            OrderNumber = $"ORD-{id:D4}",
            TotalAmount = totalAmount,
            AccountId = 20,
            Status = OrderStatus.Approved,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    // ────────────────────────────────────────────────────────────────────────
    // GetAllAsync
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllNonDeleted_WhenNoFiltersApplied()
    {
        // Arrange
        var returns = new List<OrderReturn>
        {
            CreateTestReturn(1, OrderReturnStatus.Pending),
            CreateTestReturn(2, OrderReturnStatus.Approved),
            CreateTestReturn(3, isDeleted: true),
        };
        SetupDbSets(orderReturns: returns);

        // Act
        var result = await _service.GetAllAsync(cancellationToken: CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByOrderId_WhenOrderIdProvided()
    {
        // Arrange
        var returns = new List<OrderReturn>
        {
            CreateTestReturn(1, orderId: 10),
            CreateTestReturn(2, orderId: 20),
            CreateTestReturn(3, orderId: 10),
        };
        SetupDbSets(orderReturns: returns);

        // Act
        var result = await _service.GetAllAsync(orderId: 10, cancellationToken: CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(r => r.OrderId == 10);
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByStatus_WhenStatusProvided()
    {
        // Arrange
        var returns = new List<OrderReturn>
        {
            CreateTestReturn(1, OrderReturnStatus.Pending),
            CreateTestReturn(2, OrderReturnStatus.Approved),
            CreateTestReturn(3, OrderReturnStatus.Pending),
        };
        SetupDbSets(orderReturns: returns);

        // Act
        var result = await _service.GetAllAsync(status: OrderReturnStatus.Pending, cancellationToken: CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(r => r.Status == OrderReturnStatus.Pending);
    }

    // ────────────────────────────────────────────────────────────────────────
    // GetByIdAsync
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ShouldReturnReturn_WhenIdExists()
    {
        // Arrange
        var returns = new List<OrderReturn> { CreateTestReturn(5) };
        SetupDbSets(orderReturns: returns);

        // Act
        var result = await _service.GetByIdAsync(5, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(5);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenIdDoesNotExist()
    {
        // Arrange
        SetupDbSets();

        // Act
        var result = await _service.GetByIdAsync(999, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    // ────────────────────────────────────────────────────────────────────────
    // CreateAsync
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ShouldCreateReturn_WhenOrderExists()
    {
        // Arrange
        var orders = new List<Order> { CreateTestOrder(10, 500m) };
        var returnsList = new List<OrderReturn>();
        SetupDbSets(orderReturns: returnsList, orders: orders);

        var dto = new CreateOrderReturnDto
        {
            OrderId = 10,
            Reason = (int)OrderReturnReason.Defective,
            ReasonDescription = "Product arrived broken",
            RefundAmount = 250m,
            RestockingFee = 0m,
            ShippingRefund = 0m,
        };

        // Act
        var result = await _service.CreateAsync(dto, initiatedById: 1, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.OrderId.Should().Be(10);
        result.Status.Should().Be(OrderReturnStatus.Pending);
        result.RefundAmount.Should().Be(250m);
        result.InitiatedById.Should().Be(1);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowInvalidOperationException_WhenOrderNotFound()
    {
        // Arrange
        SetupDbSets(orders: []);

        var dto = new CreateOrderReturnDto { OrderId = 999, Reason = 0, RefundAmount = 100m };

        // Act
        Func<Task> act = () => _service.CreateAsync(dto, initiatedById: 1, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*999*");
    }

    // ────────────────────────────────────────────────────────────────────────
    // ApproveAsync — Pending → Approved
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ApproveAsync_ShouldTransitionToApproved_WhenStatusIsPending()
    {
        // Arrange
        var orderReturn = CreateTestReturn(1, OrderReturnStatus.Pending);
        SetupDbSets(orderReturns: [orderReturn]);

        // Act
        var result = await _service.ApproveAsync(1, approvedById: 5, notes: "Looks good", CancellationToken.None);

        // Assert
        result.Status.Should().Be(OrderReturnStatus.Approved);
        result.ProcessedById.Should().Be(5);
        result.ApprovedAt.Should().NotBeNull();
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ApproveAsync_ShouldThrow_WhenStatusIsNotPending()
    {
        // Arrange
        var orderReturn = CreateTestReturn(1, OrderReturnStatus.Approved);
        SetupDbSets(orderReturns: [orderReturn]);

        // Act
        Func<Task> act = () => _service.ApproveAsync(1, approvedById: 5, cancellationToken: CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*cannot be approved*");
    }

    [Fact]
    public async Task ApproveAsync_ShouldThrow_WhenReturnNotFound()
    {
        // Arrange
        SetupDbSets();

        // Act
        Func<Task> act = () => _service.ApproveAsync(999, approvedById: 1, cancellationToken: CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*999*");
    }

    // ────────────────────────────────────────────────────────────────────────
    // RejectAsync — Pending → Rejected
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task RejectAsync_ShouldTransitionToRejected_WhenStatusIsPending()
    {
        // Arrange
        var orderReturn = CreateTestReturn(1, OrderReturnStatus.Pending);
        SetupDbSets(orderReturns: [orderReturn]);

        // Act
        var result = await _service.RejectAsync(1, rejectedById: 3, reason: "Policy violation", CancellationToken.None);

        // Assert
        result.Status.Should().Be(OrderReturnStatus.Rejected);
        result.ProcessedById.Should().Be(3);
        result.Notes.Should().Contain("Policy violation");
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RejectAsync_ShouldThrow_WhenReturnNotFound()
    {
        // Arrange
        SetupDbSets();

        // Act
        Func<Task> act = () => _service.RejectAsync(999, rejectedById: 1, reason: "Not found", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ────────────────────────────────────────────────────────────────────────
    // MarkReceivedAsync — → Received
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task MarkReceivedAsync_ShouldTransitionToReceived_AndSetTrackingNumber()
    {
        // Arrange
        var orderReturn = CreateTestReturn(1, OrderReturnStatus.Approved);
        SetupDbSets(orderReturns: [orderReturn]);

        // Act
        var result = await _service.MarkReceivedAsync(1, trackingNumber: "TRACK123", CancellationToken.None);

        // Assert
        result.Status.Should().Be(OrderReturnStatus.Received);
        result.ReceivedAt.Should().NotBeNull();
        result.ReturnTrackingNumber.Should().Be("TRACK123");
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkReceivedAsync_ShouldThrow_WhenReturnNotFound()
    {
        // Arrange
        SetupDbSets();

        // Act
        Func<Task> act = () => _service.MarkReceivedAsync(999, cancellationToken: CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ────────────────────────────────────────────────────────────────────────
    // ProcessRefundAsync — → Refunded
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ProcessRefundAsync_ShouldTransitionToRefunded_AndSetTransactionId()
    {
        // Arrange
        var orderReturn = CreateTestReturn(1, OrderReturnStatus.Received);
        SetupDbSets(orderReturns: [orderReturn]);

        // Act
        var result = await _service.ProcessRefundAsync(1, transactionId: "TXN-ABC123", CancellationToken.None);

        // Assert
        result.Status.Should().Be(OrderReturnStatus.Refunded);
        result.RefundedAt.Should().NotBeNull();
        result.RefundTransactionId.Should().Be("TXN-ABC123");
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessRefundAsync_ShouldThrow_WhenReturnNotFound()
    {
        // Arrange
        SetupDbSets();

        // Act
        Func<Task> act = () => _service.ProcessRefundAsync(999, transactionId: "TXN-X", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ────────────────────────────────────────────────────────────────────────
    // CompleteAsync — → Completed
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CompleteAsync_ShouldTransitionToCompleted_WhenReturnExists()
    {
        // Arrange
        var orderReturn = CreateTestReturn(1, OrderReturnStatus.Refunded);
        SetupDbSets(orderReturns: [orderReturn]);

        // Act
        var result = await _service.CompleteAsync(1, CancellationToken.None);

        // Assert
        result.Status.Should().Be(OrderReturnStatus.Completed);
        result.CompletedAt.Should().NotBeNull();
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CompleteAsync_ShouldThrow_WhenReturnNotFound()
    {
        // Arrange
        SetupDbSets();

        // Act
        Func<Task> act = () => _service.CompleteAsync(999, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ────────────────────────────────────────────────────────────────────────
    // CancelAsync — → Cancelled
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CancelAsync_ShouldTransitionToCancelled_AndAppendReason()
    {
        // Arrange
        var orderReturn = CreateTestReturn(1, OrderReturnStatus.Pending);
        SetupDbSets(orderReturns: [orderReturn]);

        // Act
        var result = await _service.CancelAsync(1, reason: "Customer changed mind", CancellationToken.None);

        // Assert
        result.Status.Should().Be(OrderReturnStatus.Cancelled);
        result.Notes.Should().Contain("Customer changed mind");
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelAsync_ShouldThrow_WhenReturnNotFound()
    {
        // Arrange
        SetupDbSets();

        // Act
        Func<Task> act = () => _service.CancelAsync(999, reason: "reason", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ────────────────────────────────────────────────────────────────────────
    // DeleteAsync — Soft Delete
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteReturn_WhenReturnExists()
    {
        // Arrange
        var orderReturn = CreateTestReturn(1);
        var returnsList = new List<OrderReturn> { orderReturn };
        SetupDbSets(orderReturns: returnsList);

        // Act
        var result = await _service.DeleteAsync(1, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        orderReturn.IsDeleted.Should().BeTrue();
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenReturnNotFound()
    {
        // Arrange
        SetupDbSets();

        // Act
        var result = await _service.DeleteAsync(999, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    // ────────────────────────────────────────────────────────────────────────
    // GetPendingReturnsAsync
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetPendingReturnsAsync_ShouldReturnOnlyPendingReturns()
    {
        // Arrange
        var returns = new List<OrderReturn>
        {
            CreateTestReturn(1, OrderReturnStatus.Pending),
            CreateTestReturn(2, OrderReturnStatus.Approved),
            CreateTestReturn(3, OrderReturnStatus.Pending),
        };
        SetupDbSets(orderReturns: returns);

        // Act
        var result = await _service.GetPendingReturnsAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(r => r.Status == OrderReturnStatus.Pending);
    }

    // ────────────────────────────────────────────────────────────────────────
    // GetStatisticsAsync
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetStatisticsAsync_ShouldReturnCorrectCounts()
    {
        // Arrange
        var returns = new List<OrderReturn>
        {
            CreateTestReturn(1, OrderReturnStatus.Pending),
            CreateTestReturn(2, OrderReturnStatus.Approved),
            CreateTestReturn(3, OrderReturnStatus.Completed, refundAmount: 150m),
            CreateTestReturn(4, OrderReturnStatus.Rejected),
        };
        var orders = new List<Order> { CreateTestOrder(1), CreateTestOrder(2) };
        SetupDbSets(orderReturns: returns, orders: orders);

        // Act
        var stats = await _service.GetStatisticsAsync(cancellationToken: CancellationToken.None);

        // Assert
        stats.Should().NotBeNull();
        stats.TotalReturns.Should().Be(4);
        stats.PendingReturns.Should().Be(1);
        stats.ApprovedReturns.Should().Be(1);
        stats.CompletedReturns.Should().Be(1);
        stats.RejectedReturns.Should().Be(1);
    }

    // ────────────────────────────────────────────────────────────────────────
    // GenerateReturnNumberAsync
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GenerateReturnNumberAsync_ShouldReturnFormattedNumber_WhenNoExistingReturns()
    {
        // Arrange
        SetupDbSets(orderReturns: []);

        // Act
        var number = await _service.GenerateReturnNumberAsync(CancellationToken.None);

        // Assert
        number.Should().NotBeNullOrEmpty();
        number.Should().StartWith("RET-");
        number.Should().EndWith("-0001");
    }

    // ────────────────────────────────────────────────────────────────────────
    // UpdateAsync
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ShouldUpdateFields_WhenReturnExists()
    {
        // Arrange
        var orderReturn = CreateTestReturn(1, OrderReturnStatus.Pending, refundAmount: 100m);
        SetupDbSets(orderReturns: [orderReturn]);

        var dto = new UpdateOrderReturnDto
        {
            Status = (int)OrderReturnStatus.Approved,
            Notes = "Updated notes",
            RefundAmount = 200m,
            RestockingFee = 10m,
        };

        // Act
        var result = await _service.UpdateAsync(1, dto, CancellationToken.None);

        // Assert
        result.Status.Should().Be(OrderReturnStatus.Approved);
        result.Notes.Should().Be("Updated notes");
        result.RefundAmount.Should().Be(200m);
        result.RestockingFee.Should().Be(10m);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenReturnNotFound()
    {
        // Arrange
        SetupDbSets();

        var dto = new UpdateOrderReturnDto { Status = (int)OrderReturnStatus.Approved };

        // Act
        Func<Task> act = () => _service.UpdateAsync(999, dto, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ────────────────────────────────────────────────────────────────────────
    // NetRefundAmount calculation
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public void NetRefundAmount_ShouldCalculateCorrectly()
    {
        // Arrange
        var orderReturn = new OrderReturn
        {
            RefundAmount = 200m,
            RestockingFee = 20m,
            ShippingRefund = 15m,
        };

        // Act & Assert
        orderReturn.NetRefundAmount.Should().Be(195m); // 200 - 20 + 15
    }
}
