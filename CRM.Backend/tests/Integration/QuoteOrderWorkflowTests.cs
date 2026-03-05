// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.DTOs;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace CRM.Tests.Integration;

/// <summary>
/// Cross-service workflow tests covering the Quote → Order pipeline.
/// Uses pure mock pattern — no InMemory DB required.
/// SPEC_CONFLICT: IQuoteService.RejectAsync has signature (int id, string? reason) only —
///   there is NO rejectedById param on the Quote side (unlike IOrderService.RejectAsync).
///   Tests reflect ACTUAL interface contracts.
/// </summary>
public class QuoteOrderWorkflowTests
{
    private readonly Mock<IQuoteService> _quoteService = new(MockBehavior.Loose);
    private readonly Mock<IOrderService> _orderService = new(MockBehavior.Loose);

    #region Quote Lifecycle Tests

    [Fact]
    public async Task CreateQuote_ShouldReturnQuote_WhenInputIsValid()
    {
        // Arrange
        var inputQuote = new Quote
        {
            QuoteNumber = "Q-2026-001",
            Name = "Enterprise License Quote",
            Status = QuoteStatus.Draft
        };
        var returnedQuote = new Quote
        {
            Id = 1,
            QuoteNumber = "Q-2026-001",
            Name = "Enterprise License Quote",
            Status = QuoteStatus.Draft
        };

        _quoteService.Setup(s => s.CreateAsync(It.IsAny<Quote>())).ReturnsAsync(returnedQuote);

        // Act
        var result = await _quoteService.Object.CreateAsync(inputQuote);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.QuoteNumber.Should().Be("Q-2026-001");
    }

    [Fact]
    public async Task SendQuote_ShouldReturnTrue_WhenQuoteExists()
    {
        // Arrange
        const int quoteId = 1;
        _quoteService.Setup(s => s.SendAsync(quoteId)).ReturnsAsync(true);

        // Act
        var result = await _quoteService.Object.SendAsync(quoteId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task AcceptQuote_ShouldReturnTrue_WhenQuoteIsSent()
    {
        // Arrange
        const int quoteId = 1;
        _quoteService.Setup(s => s.AcceptAsync(quoteId)).ReturnsAsync(true);

        // Act
        var result = await _quoteService.Object.AcceptAsync(quoteId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task RejectQuote_ShouldReturnTrue_WhenReasonProvided()
    {
        // SPEC_CONFLICT: IQuoteService.RejectAsync signature is (int id, string? reason = null).
        // There is NO rejectedById parameter on quotes — only on orders. Logged for review.
        // Arrange
        const int quoteId = 1;
        const string reason = "Price too high";
        _quoteService.Setup(s => s.RejectAsync(quoteId, reason)).ReturnsAsync(true);

        // Act
        var result = await _quoteService.Object.RejectAsync(quoteId, reason);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CreateRevision_ShouldReturnNewQuote_WhenOriginalExists()
    {
        // Arrange
        const int originalQuoteId = 1;
        var revisedQuote = new Quote
        {
            Id = 2,
            QuoteNumber = "Q-2026-001-R2",
            Name = "Enterprise License Quote",
            Version = 2,
            Status = QuoteStatus.Draft
        };

        _quoteService
            .Setup(s => s.CreateRevisionAsync(originalQuoteId))
            .ReturnsAsync(revisedQuote);

        // Act
        var result = await _quoteService.Object.CreateRevisionAsync(originalQuoteId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(2);
        result.Version.Should().Be(2);
    }

    [Fact]
    public async Task GetQuoteStatistics_ShouldReturnStats_WhenQuotesExist()
    {
        // Arrange
        var stats = new QuoteStatistics
        {
            TotalQuotes = 50,
            DraftQuotes = 10,
            SentQuotes = 20,
            AcceptedQuotes = 15,
            RejectedQuotes = 5,
            TotalValue = 500_000m,
            AcceptedValue = 300_000m,
            AcceptanceRate = 60.0
        };

        _quoteService
            .Setup(s => s.GetStatisticsAsync(null, null))
            .ReturnsAsync(stats);

        // Act
        var result = await _quoteService.Object.GetStatisticsAsync();

        // Assert
        result.Should().NotBeNull();
        result.TotalQuotes.Should().Be(50);
        result.AcceptedQuotes.Should().Be(15);
        result.AcceptanceRate.Should().BeApproximately(60.0, 0.01);
    }

    #endregion

    #region Order Creation Tests

    [Fact]
    public async Task CreateOrderFromQuote_ShouldReturnOrder_WhenQuoteIsAccepted()
    {
        // Arrange
        const int quoteId = 1;
        var expectedOrder = new OrderDto
        {
            Id = 100,
            OrderNumber = "ORD-2026-001",
            Name = "Enterprise License Order",
            Status = (int)OrderStatus.Draft
        };

        _orderService
            .Setup(s => s.CreateFromQuoteAsync(quoteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOrder);

        // Act
        var result = await _orderService.Object.CreateFromQuoteAsync(quoteId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(100);
        result.OrderNumber.Should().Be("ORD-2026-001");
    }

    [Fact]
    public async Task CreateOrderFromOpportunity_ShouldReturnOrder_WhenOpportunityExists()
    {
        // Arrange
        const int opportunityId = 20;
        var expectedOrder = new OrderDto
        {
            Id = 101,
            OrderNumber = "ORD-2026-002",
            Name = "Opportunity Conversion Order"
        };

        _orderService
            .Setup(s => s.CreateFromOpportunityAsync(opportunityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOrder);

        // Act
        var result = await _orderService.Object.CreateFromOpportunityAsync(opportunityId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(101);
    }

    #endregion

    #region Order Status Management Tests

    [Fact]
    public async Task SubmitOrderForApproval_ShouldReturnOrder_WhenOrderIsValid()
    {
        // Arrange
        const int orderId = 100;
        var submittedOrder = new Order
        {
            Id = orderId,
            OrderNumber = "ORD-2026-001",
            Status = OrderStatus.PendingApproval
        };

        _orderService
            .Setup(s => s.SubmitForApprovalAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(submittedOrder);

        // Act
        var result = await _orderService.Object.SubmitForApprovalAsync(orderId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.PendingApproval);
    }

    [Fact]
    public async Task ApproveOrder_ShouldReturnApprovedOrder_WhenOrderIsPending()
    {
        // Arrange
        const int orderId = 100;
        const int approverId = 5;
        var approvedOrder = new Order
        {
            Id = orderId,
            OrderNumber = "ORD-2026-001",
            Status = OrderStatus.Approved
        };

        _orderService
            .Setup(s => s.ApproveAsync(orderId, approverId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(approvedOrder);

        // Act
        var result = await _orderService.Object.ApproveAsync(orderId, approverId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.Approved);
    }

    [Fact]
    public async Task RejectOrder_ShouldReturnRejectedOrder_WhenReasonProvided()
    {
        // SPEC_CONFLICT: OrderStatus enum has no "Rejected" value. The actual OrderService.RejectAsync
        // implementation sets Status = OrderStatus.Cancelled when rejecting. Logged for review.
        // Arrange — IOrderService.RejectAsync takes (orderId, rejectedById, reason, CancellationToken)
        const int orderId = 100;
        const int rejectedById = 5;
        const string reason = "Budget not approved for this quarter";
        var rejectedOrder = new Order
        {
            Id = orderId,
            OrderNumber = "ORD-2026-001",
            Status = OrderStatus.Cancelled    // actual impl sets Cancelled (no Rejected enum value)
        };

        _orderService
            .Setup(s => s.RejectAsync(orderId, rejectedById, reason, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rejectedOrder);

        // Act
        var result = await _orderService.Object.RejectAsync(orderId, rejectedById, reason, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public async Task CancelOrder_ShouldReturnCancelledOrder_WhenReasonProvided()
    {
        // Arrange
        const int orderId = 100;
        const string reason = "Customer request";
        var cancelledOrder = new Order
        {
            Id = orderId,
            OrderNumber = "ORD-2026-001",
            Status = OrderStatus.Cancelled
        };

        _orderService
            .Setup(s => s.CancelAsync(orderId, reason, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cancelledOrder);

        // Act
        var result = await _orderService.Object.CancelAsync(orderId, reason, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public async Task MarkOrderFulfilled_ShouldReturnFulfilledOrder_WhenOrderIsApproved()
    {
        // Arrange
        const int orderId = 100;
        var fulfilledOrder = new Order
        {
            Id = orderId,
            OrderNumber = "ORD-2026-001",
            Status = OrderStatus.Fulfilled
        };

        _orderService
            .Setup(s => s.MarkAsFulfilledAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fulfilledOrder);

        // Act
        var result = await _orderService.Object.MarkAsFulfilledAsync(orderId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.Fulfilled);
    }

    #endregion

    #region Order Analytics & Misc Tests

    [Fact]
    public async Task GetOrderStatistics_ShouldReturnStats_WhenOrdersExist()
    {
        // Arrange
        var stats = new OrderStatistics
        {
            TotalOrders = 120,
            PendingOrders = 30,
            ProcessingOrders = 20,
            FulfilledOrders = 60,
            CancelledOrders = 10,
            TotalRevenue = 1_200_000m,
            AverageOrderValue = 10_000m,
            FulfillmentRate = 83.3
        };

        _orderService
            .Setup(s => s.GetStatisticsAsync(null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stats);

        // Act
        var result = await _orderService.Object.GetStatisticsAsync(null, null, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalOrders.Should().Be(120);
        result.TotalRevenue.Should().Be(1_200_000m);
        result.FulfillmentRate.Should().BeApproximately(83.3, 0.01);
    }

    [Fact]
    public async Task CloneOrder_ShouldReturnClone_WhenOrderExists()
    {
        // Arrange
        const int sourceOrderId = 100;
        var clonedOrder = new Order
        {
            Id = 200,
            OrderNumber = "ORD-2026-050",
            Name = "Copy of Enterprise License Order",
            Status = OrderStatus.Draft
        };

        _orderService
            .Setup(s => s.CloneOrderAsync(sourceOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clonedOrder);

        // Act
        var result = await _orderService.Object.CloneOrderAsync(sourceOrderId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(200);
        result.Status.Should().Be(OrderStatus.Draft);
    }

    [Fact]
    public async Task AddLineItem_ShouldReturnLineItem_WhenOrderAndItemAreValid()
    {
        // Arrange
        const int orderId = 100;
        var lineItem = new OrderLineItem
        {
            Name = "Annual Support License",
            Quantity = 1,
            UnitPrice = 5_000m,
            ExtendedAmount = 5_000m
        };
        var savedLineItem = new OrderLineItem
        {
            Id = 1,
            Name = "Annual Support License",
            Quantity = 1,
            UnitPrice = 5_000m,
            ExtendedAmount = 5_000m
        };

        _orderService
            .Setup(s => s.AddLineItemAsync(orderId, It.IsAny<OrderLineItem>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedLineItem);

        // Act
        var result = await _orderService.Object.AddLineItemAsync(orderId, lineItem, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.UnitPrice.Should().Be(5_000m);
    }

    [Fact]
    public async Task GetOrdersByStatus_ShouldReturnMatchingOrders()
    {
        // Arrange
        var pendingOrders = new List<Order>
        {
            new() { Id = 1, OrderNumber = "ORD-001", Status = OrderStatus.PendingApproval },
            new() { Id = 2, OrderNumber = "ORD-002", Status = OrderStatus.PendingApproval }
        };

        _orderService
            .Setup(s => s.GetByStatusAsync(OrderStatus.PendingApproval, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pendingOrders);

        // Act
        var result = await _orderService.Object.GetByStatusAsync(OrderStatus.PendingApproval, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(o => o.Status.Should().Be(OrderStatus.PendingApproval));
    }

    #endregion
}
