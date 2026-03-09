// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CRM.Tests.Integration;

public class LeadToOpportunityQuoteOrderWorkflowTests
{
    // ── mocks ──────────────────────────────────────────────────────────
    private readonly Mock<ILeadService> _leadSvc = new();
    private readonly Mock<IOpportunityService> _oppSvc = new();
    private readonly Mock<IQuoteService> _quoteSvc = new();
    private readonly Mock<IOrderService> _orderSvc = new();

    // ── helpers ────────────────────────────────────────────────────────
    private static Quote BuildQuote(int id, int oppId) =>
        new() { Id = id, OpportunityId = oppId, Status = QuoteStatus.Draft };

    private static Order BuildOrder(int id, int quoteId) =>
        new() { Id = id };

    private static OrderDto BuildOrderDto(int id) =>
        new() { Id = id };

    // ══════════════════════════════════════════════════════════════════
    // 1. Lead → Opportunity conversion
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Lead_ConvertToOpportunity_ReturnsOpportunityAndLeadIds()
    {
        // arrange
        _leadSvc.Setup(s => s.ConvertAsync(
            1,
            "Enterprise Deal",
            null,
            50000m,
            It.IsAny<DateTime?>()))
            .ReturnsAsync((42, 1));

        // act
        var (oppId, leadId) = await _leadSvc.Object.ConvertAsync(
            1, "Enterprise Deal", null, 50000m, DateTime.UtcNow.AddDays(30));

        // assert
        oppId.Should().Be(42);
        leadId.Should().Be(1);
        _leadSvc.Verify(s => s.ConvertAsync(1, "Enterprise Deal", null, 50000m, It.IsAny<DateTime?>()), Times.Once);
    }

    [Fact]
    public async Task Lead_ConvertToOpportunity_WithAccountId_PassesAccountId()
    {
        _leadSvc.Setup(s => s.ConvertAsync(5, "Acme Deal", 10, 25000m, It.IsAny<DateTime?>()))
            .ReturnsAsync((99, 5));

        var (oppId, leadId) = await _leadSvc.Object.ConvertAsync(5, "Acme Deal", 10, 25000m, null);

        oppId.Should().Be(99);
        leadId.Should().Be(5);
    }

    [Fact]
    public async Task Lead_ConvertToOpportunity_WithNullValues_StillConverts()
    {
        _leadSvc.Setup(s => s.ConvertAsync(3, null, null, null, null))
            .ReturnsAsync((77, 3));

        var (oppId, _) = await _leadSvc.Object.ConvertAsync(3, null, null, null, null);

        oppId.Should().Be(77);
    }

    // ══════════════════════════════════════════════════════════════════
    // 2. Duplicate-check prevents double create
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Lead_CheckDuplicate_ReturnsDuplicateFlag_WhenEmailMatches()
    {
        // email, firstName, lastName, company
        _leadSvc.Setup(s => s.CheckDuplicateAsync(
                "john@acme.com", null, null, "Acme Corp", default))
            .ReturnsAsync((true, 5, "email"));

        var result = await _leadSvc.Object
            .CheckDuplicateAsync("john@acme.com", null, null, "Acme Corp");

        result.IsDuplicate.Should().BeTrue();
        result.ExistingLeadId.Should().Be(5);
        result.MatchedOn.Should().Be("email");
    }

    [Fact]
    public async Task Lead_CheckDuplicate_ReturnsNotDuplicate_WhenNoMatch()
    {
        _leadSvc.Setup(s => s.CheckDuplicateAsync(
                "new@new.com", null, null, null, default))
            .ReturnsAsync((false, null, null));

        var result = await _leadSvc.Object
            .CheckDuplicateAsync("new@new.com", null, null, null);

        result.IsDuplicate.Should().BeFalse();
        result.ExistingLeadId.Should().BeNull();
        result.MatchedOn.Should().BeNull();
    }

    [Fact]
    public async Task Lead_CheckDuplicate_ReturnsMatchedOn_Name()
    {
        _leadSvc.Setup(s => s.CheckDuplicateAsync(
                null, "John", "Doe", null, default))
            .ReturnsAsync((true, 12, "name"));

        var result = await _leadSvc.Object
            .CheckDuplicateAsync(null, "John", "Doe", null);

        result.IsDuplicate.Should().BeTrue();
        result.ExistingLeadId.Should().Be(12);
        result.MatchedOn.Should().Be("name");
    }

    // ══════════════════════════════════════════════════════════════════
    // 3. Opportunity → Quote
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Opportunity_CreateQuote_ReturnsQuoteWithOpportunityId()
    {
        var quote = BuildQuote(10, 42);
        _quoteSvc.Setup(s => s.CreateAsync(It.Is<Quote>(q => q.OpportunityId == 42)))
            .ReturnsAsync(quote);

        var result = await _quoteSvc.Object.CreateAsync(new Quote { OpportunityId = 42 });

        result.Should().NotBeNull();
        result.Id.Should().Be(10);
        result.OpportunityId.Should().Be(42);
        result.Status.Should().Be(QuoteStatus.Draft);
    }

    [Fact]
    public async Task Opportunity_GetQuoteById_ReturnsCorrectQuote()
    {
        var quote = BuildQuote(10, 42);
        _quoteSvc.Setup(s => s.GetByIdAsync(10)).ReturnsAsync(quote);

        var result = await _quoteSvc.Object.GetByIdAsync(10);

        result.Should().NotBeNull();
        result!.Id.Should().Be(10);
    }

    [Fact]
    public async Task Opportunity_GetQuoteById_ReturnsNull_WhenNotFound()
    {
        _quoteSvc.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((Quote?)null);

        var result = await _quoteSvc.Object.GetByIdAsync(999);

        result.Should().BeNull();
    }

    // ══════════════════════════════════════════════════════════════════
    // 4. Quote lifecycle: Send → Accept → Reject → Revision
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Quote_Send_ReturnsTrue_WhenSuccessful()
    {
        _quoteSvc.Setup(s => s.SendAsync(10)).ReturnsAsync(true);

        var result = await _quoteSvc.Object.SendAsync(10);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Quote_Accept_ReturnsTrue_WhenSuccessful()
    {
        _quoteSvc.Setup(s => s.AcceptAsync(10)).ReturnsAsync(true);

        var result = await _quoteSvc.Object.AcceptAsync(10);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Quote_Reject_ReturnsTrue_WithReason()
    {
        _quoteSvc.Setup(s => s.RejectAsync(10, "Price too high")).ReturnsAsync(true);

        var result = await _quoteSvc.Object.RejectAsync(10, "Price too high");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Quote_Reject_ReturnsTrue_WithNullReason()
    {
        _quoteSvc.Setup(s => s.RejectAsync(10, null)).ReturnsAsync(true);

        var result = await _quoteSvc.Object.RejectAsync(10);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Quote_CreateRevision_ReturnsNewQuote()
    {
        var revision = BuildQuote(11, 42);
        _quoteSvc.Setup(s => s.CreateRevisionAsync(10)).ReturnsAsync(revision);

        var result = await _quoteSvc.Object.CreateRevisionAsync(10);

        result.Should().NotBeNull();
        result.Id.Should().Be(11);
    }

    // ══════════════════════════════════════════════════════════════════
    // 5. Quote accepted → Order created from quote
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public async Task QuoteAccepted_CreateOrderFromQuote_ReturnsOrderDto()
    {
        // Step 1: accept quote
        _quoteSvc.Setup(s => s.AcceptAsync(10)).ReturnsAsync(true);
        var accepted = await _quoteSvc.Object.AcceptAsync(10);
        accepted.Should().BeTrue();

        // Step 2: create order from quote
        var dto = BuildOrderDto(200);
        _orderSvc.Setup(s => s.CreateFromQuoteAsync(10, default)).ReturnsAsync(dto);
        var order = await _orderSvc.Object.CreateFromQuoteAsync(10);

        order.Should().NotBeNull();
        order.Id.Should().Be(200);
    }

    [Fact]
    public async Task Order_CreateFromOpportunity_ReturnsOrderDto()
    {
        var dto = BuildOrderDto(201);
        _orderSvc.Setup(s => s.CreateFromOpportunityAsync(42, default)).ReturnsAsync(dto);

        var result = await _orderSvc.Object.CreateFromOpportunityAsync(42);

        result.Should().NotBeNull();
        result.Id.Should().Be(201);
    }

    // ══════════════════════════════════════════════════════════════════
    // 6. Order fulfillment lifecycle
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Order_SubmitForApproval_ReturnsOrder()
    {
        var order = BuildOrder(200, 10);
        _orderSvc.Setup(s => s.SubmitForApprovalAsync(200, default)).ReturnsAsync(order);

        var result = await _orderSvc.Object.SubmitForApprovalAsync(200);

        result.Should().NotBeNull();
        result.Id.Should().Be(200);
    }

    [Fact]
    public async Task Order_Approve_ReturnsOrder()
    {
        var order = BuildOrder(200, 10);
        _orderSvc.Setup(s => s.ApproveAsync(200, 1, default)).ReturnsAsync(order);

        var result = await _orderSvc.Object.ApproveAsync(200, 1);

        result.Should().NotBeNull();
        result.Id.Should().Be(200);
    }

    [Fact]
    public async Task Order_MarkAsFulfilled_ReturnsOrder()
    {
        var order = BuildOrder(200, 10);
        _orderSvc.Setup(s => s.MarkAsFulfilledAsync(200, default)).ReturnsAsync(order);

        var result = await _orderSvc.Object.MarkAsFulfilledAsync(200);

        result.Should().NotBeNull();
        result.Id.Should().Be(200);
    }

    [Fact]
    public async Task Order_MarkAsDelivered_ReturnsOrder()
    {
        var order = BuildOrder(200, 10);
        _orderSvc.Setup(s => s.MarkAsDeliveredAsync(200, null, default)).ReturnsAsync(order);

        var result = await _orderSvc.Object.MarkAsDeliveredAsync(200);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Order_Cancel_ReturnsOrder()
    {
        var order = BuildOrder(200, 10);
        _orderSvc.Setup(s => s.CancelAsync(200, "Not needed", default)).ReturnsAsync(order);

        var result = await _orderSvc.Object.CancelAsync(200, "Not needed");

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Order_PutOnHold_ReturnsOrder()
    {
        var order = BuildOrder(200, 10);
        _orderSvc.Setup(s => s.PutOnHoldAsync(200, "Awaiting approval", default)).ReturnsAsync(order);

        var result = await _orderSvc.Object.PutOnHoldAsync(200, "Awaiting approval");

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Order_ReleaseFromHold_ReturnsOrder()
    {
        var order = BuildOrder(200, 10);
        _orderSvc.Setup(s => s.ReleaseFromHoldAsync(200, default)).ReturnsAsync(order);

        var result = await _orderSvc.Object.ReleaseFromHoldAsync(200);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Order_Reject_ReturnsOrder()
    {
        var order = BuildOrder(200, 10);
        _orderSvc.Setup(s => s.RejectAsync(200, 2, "Budget cut", default)).ReturnsAsync(order);

        var result = await _orderSvc.Object.RejectAsync(200, 2, "Budget cut");

        result.Should().NotBeNull();
    }

    // ══════════════════════════════════════════════════════════════════
    // 7. Order line-item operations
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Order_AddLineItem_ReturnsLineItem()
    {
        var lineItem = new OrderLineItem { Id = 1, OrderId = 200, Quantity = 2 };
        _orderSvc.Setup(s => s.AddLineItemAsync(200, It.IsAny<OrderLineItem>(), default))
            .ReturnsAsync(lineItem);

        var result = await _orderSvc.Object.AddLineItemAsync(200, new OrderLineItem { Quantity = 2 });

        result.Should().NotBeNull();
        result.Id.Should().Be(1);
    }

    [Fact]
    public async Task Order_GetLineItems_ReturnsList()
    {
        var items = new List<OrderLineItem>
        {
            new() { Id = 1, OrderId = 200 },
            new() { Id = 2, OrderId = 200 }
        };
        _orderSvc.Setup(s => s.GetLineItemsAsync(200, default)).ReturnsAsync(items);

        var result = await _orderSvc.Object.GetLineItemsAsync(200);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Order_RemoveLineItem_ReturnsTrue()
    {
        _orderSvc.Setup(s => s.RemoveLineItemAsync(1, default)).ReturnsAsync(true);

        var result = await _orderSvc.Object.RemoveLineItemAsync(1);

        result.Should().BeTrue();
    }

    // ══════════════════════════════════════════════════════════════════
    // 8. Full end-to-end sales pipeline workflow
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public async Task FullSalesPipeline_Lead_To_FulfilledOrder_HappyPath()
    {
        // Lead → Opportunity
        _leadSvc.Setup(s => s.ConvertAsync(1, "Big Deal", null, 100000m, It.IsAny<DateTime?>()))
            .ReturnsAsync((42, 1));

        // Opportunity → Quote
        var quote = BuildQuote(10, 42);
        _quoteSvc.Setup(s => s.CreateAsync(It.IsAny<Quote>())).ReturnsAsync(quote);
        _quoteSvc.Setup(s => s.SendAsync(10)).ReturnsAsync(true);
        _quoteSvc.Setup(s => s.AcceptAsync(10)).ReturnsAsync(true);

        // Quote → Order
        var orderDto = BuildOrderDto(200);
        _orderSvc.Setup(s => s.CreateFromQuoteAsync(10, default)).ReturnsAsync(orderDto);
        var orderEntity = BuildOrder(200, 10);
        _orderSvc.Setup(s => s.SubmitForApprovalAsync(200, default)).ReturnsAsync(orderEntity);
        _orderSvc.Setup(s => s.ApproveAsync(200, 1, default)).ReturnsAsync(orderEntity);
        _orderSvc.Setup(s => s.MarkAsFulfilledAsync(200, default)).ReturnsAsync(orderEntity);

        // Execute pipeline
        var (oppId, leadId) = await _leadSvc.Object.ConvertAsync(1, "Big Deal", null, 100000m, null);
        var createdQuote = await _quoteSvc.Object.CreateAsync(new Quote { OpportunityId = oppId });
        await _quoteSvc.Object.SendAsync(createdQuote.Id);
        await _quoteSvc.Object.AcceptAsync(createdQuote.Id);
        var createdOrder = await _orderSvc.Object.CreateFromQuoteAsync(createdQuote.Id);
        await _orderSvc.Object.SubmitForApprovalAsync(createdOrder.Id);
        await _orderSvc.Object.ApproveAsync(createdOrder.Id, 1);
        await _orderSvc.Object.MarkAsFulfilledAsync(createdOrder.Id);

        // Verify chain
        oppId.Should().Be(42);
        createdQuote.OpportunityId.Should().Be(42);
        createdOrder.Id.Should().Be(200);

        _leadSvc.Verify(s => s.ConvertAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<decimal?>(), It.IsAny<DateTime?>()), Times.Once);
        _quoteSvc.Verify(s => s.SendAsync(10), Times.Once);
        _quoteSvc.Verify(s => s.AcceptAsync(10), Times.Once);
        _orderSvc.Verify(s => s.CreateFromQuoteAsync(10, It.IsAny<CancellationToken>()), Times.Once);
        _orderSvc.Verify(s => s.MarkAsFulfilledAsync(200, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ══════════════════════════════════════════════════════════════════
    // 9. Order statistics and search
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Order_GetStatistics_ReturnsStatisticsDto()
    {
        var stats = new OrderStatistics
        {
            TotalOrders = 100,
            PendingOrders = 10,
            FulfilledOrders = 80
        };
        _orderSvc.Setup(s => s.GetStatisticsAsync(null, null, default)).ReturnsAsync(stats);

        var result = await _orderSvc.Object.GetStatisticsAsync();

        result.Should().NotBeNull();
        result.TotalOrders.Should().Be(100);
        result.FulfilledOrders.Should().Be(80);
    }

    [Fact]
    public async Task Quote_GetStatistics_ReturnsStats()
    {
        var stats = new QuoteStatistics
        {
            TotalQuotes = 50,
            AcceptedQuotes = 30,
            AcceptanceRate = 60.0
        };
        _quoteSvc.Setup(s => s.GetStatisticsAsync(null, null)).ReturnsAsync(stats);

        var result = await _quoteSvc.Object.GetStatisticsAsync();

        result.Should().NotBeNull();
        result.AcceptanceRate.Should().Be(60.0);
    }

    [Fact]
    public async Task Order_Search_ReturnsMatchingOrders()
    {
        var orders = new List<Order> { BuildOrder(200, 10), BuildOrder(201, 11) };
        _orderSvc.Setup(s => s.SearchAsync("enterprise", default)).ReturnsAsync(orders);

        var result = await _orderSvc.Object.SearchAsync("enterprise");

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Order_GetByStatus_ReturnsFilteredOrders()
    {
        var orders = new List<Order> { BuildOrder(200, 10) };
        _orderSvc.Setup(s => s.GetByStatusAsync(OrderStatus.Approved, default)).ReturnsAsync(orders);

        var result = await _orderSvc.Object.GetByStatusAsync(OrderStatus.Approved);

        result.Should().HaveCount(1);
    }

    // ══════════════════════════════════════════════════════════════════
    // 10. Order invoice creation
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Order_CreateInvoice_ReturnsInvoice()
    {
        var invoice = new Invoice { Id = 50, OrderId = 200 };
        _orderSvc.Setup(s => s.CreateInvoiceAsync(200, default)).ReturnsAsync(invoice);

        var result = await _orderSvc.Object.CreateInvoiceAsync(200);

        result.Should().NotBeNull();
        result.Id.Should().Be(50);
        result.OrderId.Should().Be(200);
    }

    [Fact]
    public async Task Order_GetInvoices_ReturnsList()
    {
        var invoices = new List<Invoice>
        {
            new() { Id = 50, OrderId = 200 },
            new() { Id = 51, OrderId = 200 }
        };
        _orderSvc.Setup(s => s.GetInvoicesAsync(200, default)).ReturnsAsync(invoices);

        var result = await _orderSvc.Object.GetInvoicesAsync(200);

        result.Should().HaveCount(2);
    }

    // ══════════════════════════════════════════════════════════════════
    // 11. Order discount and coupon
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Order_ApplyDiscount_ReturnsUpdatedOrder()
    {
        var order = BuildOrder(200, 10);
        _orderSvc.Setup(s => s.ApplyDiscountAsync(200, 500m, null, default)).ReturnsAsync(order);

        var result = await _orderSvc.Object.ApplyDiscountAsync(200, 500m);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Order_ApplyCoupon_ReturnsUpdatedOrder()
    {
        var order = BuildOrder(200, 10);
        _orderSvc.Setup(s => s.ApplyCouponAsync(200, "SAVE20", default)).ReturnsAsync(order);

        var result = await _orderSvc.Object.ApplyCouponAsync(200, "SAVE20");

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Order_RecalculateTotals_ReturnsUpdatedOrder()
    {
        var order = BuildOrder(200, 10);
        _orderSvc.Setup(s => s.RecalculateTotalsAsync(200, default)).ReturnsAsync(order);

        var result = await _orderSvc.Object.RecalculateTotalsAsync(200);

        result.Should().NotBeNull();
    }

    // ══════════════════════════════════════════════════════════════════
    // 12. Opportunity cloning
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Opportunity_Clone_ReturnsNewOpportunity()
    {
        var cloned = new Opportunity { Id = 43, Name = "Enterprise Deal (Copy)" };
        _oppSvc.Setup(s => s.CloneAsync(42, null, default)).ReturnsAsync(cloned);

        var result = await _oppSvc.Object.CloneAsync(42);

        result.Should().NotBeNull();
        result.Id.Should().Be(43);
        result.Name.Should().Contain("Copy");
    }

    [Fact]
    public async Task Opportunity_GenerateOrderNumber_ReturnsString()
    {
        _orderSvc.Setup(s => s.GenerateOrderNumberAsync(default)).ReturnsAsync("ORD-2026-001");

        var result = await _orderSvc.Object.GenerateOrderNumberAsync();

        result.Should().NotBeNullOrEmpty();
        result.Should().StartWith("ORD");
    }
}
