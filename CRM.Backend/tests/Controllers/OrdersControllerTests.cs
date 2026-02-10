// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Orders Controller Unit Tests

using Xunit;
using Moq;
using FluentAssertions;
using CRM.Api.Controllers;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Api.Hubs;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Security.Claims;

namespace CRM.Tests.Controllers;

/// <summary>
/// Comprehensive unit tests for OrdersController
/// Covers: Order CRUD, line items, status, fulfillment, invoicing
/// </summary>
public class OrdersControllerTests
{
    private readonly Mock<IOrderService> _mockOrderService;
    private readonly Mock<ILogger<OrdersController>> _mockLogger;
    private readonly Mock<ICrmNotificationService> _mockNotificationService;
    private readonly OrdersController _controller;

    public OrdersControllerTests()
    {
        _mockOrderService = new Mock<IOrderService>();
        _mockLogger = new Mock<ILogger<OrdersController>>();
        _mockNotificationService = new Mock<ICrmNotificationService>();

        _mockNotificationService.Setup(x => x.NotifyRecordCreatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);
        _mockNotificationService.Setup(x => x.NotifyRecordUpdatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);
        _mockNotificationService.Setup(x => x.NotifyRecordDeletedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);

        _controller = new OrdersController(_mockOrderService.Object, _mockLogger.Object);

        var httpContext = new DefaultHttpContext();
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "1") };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_ReturnsOkResult_WithOrders()
    {
        // Arrange
        var orders = new List<OrderDto>
        {
            new OrderDto { Id = 1, OrderNumber = "ORD-2024-001", Status = "Confirmed" },
            new OrderDto { Id = 2, OrderNumber = "ORD-2024-002", Status = "Shipped" }
        };

        _mockOrderService.Setup(s => s.GetAllAsync())
            .ReturnsAsync(orders);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedOrders = okResult.Value as IEnumerable<OrderDto>;
        returnedOrders.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByStatus_ReturnsFilteredOrders()
    {
        // Arrange
        var orders = new List<OrderDto>
        {
            new OrderDto { Id = 1, Status = "Confirmed" }
        };

        _mockOrderService.Setup(s => s.GetByStatusAsync("Confirmed"))
            .ReturnsAsync(orders);

        // Act
        var result = await _controller.GetByStatus("Confirmed");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetByAccount_ReturnsAccountOrders()
    {
        // Arrange
        var orders = new List<OrderDto>
        {
            new OrderDto { Id = 1, AccountId = 1 }
        };

        _mockOrderService.Setup(s => s.GetByAccountAsync(1))
            .ReturnsAsync(orders);

        // Act
        var result = await _controller.GetByAccount(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetByDateRange_ReturnsFilteredOrders()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(-30);
        var endDate = DateTime.Today;
        var orders = new List<OrderDto>
        {
            new OrderDto { Id = 1, OrderDate = DateTime.Today.AddDays(-15) }
        };

        _mockOrderService.Setup(s => s.GetByDateRangeAsync(startDate, endDate))
            .ReturnsAsync(orders);

        // Act
        var result = await _controller.GetByDateRange(startDate, endDate);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetPendingOrders_ReturnsPending()
    {
        // Arrange
        var orders = new List<OrderDto>
        {
            new OrderDto { Id = 1, Status = "Pending" }
        };

        _mockOrderService.Setup(s => s.GetPendingOrdersAsync())
            .ReturnsAsync(orders);

        // Act
        var result = await _controller.GetPendingOrders();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_ExistingOrder_ReturnsOkWithOrder()
    {
        // Arrange
        var order = new OrderDto { Id = 1, OrderNumber = "ORD-2024-001" };

        _mockOrderService.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(order);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedOrder = okResult.Value as OrderDto;
        returnedOrder!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetById_NonExistingOrder_ReturnsNotFound()
    {
        // Arrange
        _mockOrderService.Setup(s => s.GetByIdAsync(999))
            .ReturnsAsync((OrderDto?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetByOrderNumber_ExistingOrder_ReturnsOk()
    {
        // Arrange
        var order = new OrderDto { Id = 1, OrderNumber = "ORD-2024-001" };

        _mockOrderService.Setup(s => s.GetByOrderNumberAsync("ORD-2024-001"))
            .ReturnsAsync(order);

        // Act
        var result = await _controller.GetByOrderNumber("ORD-2024-001");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_ValidOrder_ReturnsCreatedWithOrder()
    {
        // Arrange
        var createDto = new CreateOrderDto
        {
            AccountId = 1,
            OrderDate = DateTime.Today,
            LineItems = new List<CreateOrderLineItemDto>
            {
                new CreateOrderLineItemDto { ProductId = 1, Quantity = 2, UnitPrice = 100 }
            }
        };

        var createdOrder = new OrderDto
        {
            Id = 1,
            OrderNumber = "ORD-2024-001",
            Status = "Draft",
            TotalAmount = 200
        };

        _mockOrderService.Setup(s => s.CreateAsync(It.IsAny<CreateOrderDto>()))
            .ReturnsAsync(createdOrder);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var returnedOrder = createdResult.Value as OrderDto;
        returnedOrder!.OrderNumber.Should().Be("ORD-2024-001");
    }

    [Fact]
    public async Task Create_NullDto_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.Create(null!);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_InvalidAccountId_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateOrderDto { AccountId = 999 };

        _mockOrderService.Setup(s => s.CreateAsync(It.IsAny<CreateOrderDto>()))
            .ThrowsAsync(new ArgumentException("Invalid account"));

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateFromQuote_ValidQuote_ReturnsCreated()
    {
        // Arrange
        var createdOrder = new OrderDto { Id = 1, QuoteId = 1 };

        _mockOrderService.Setup(s => s.CreateFromQuoteAsync(1))
            .ReturnsAsync(createdOrder);

        // Act
        var result = await _controller.CreateFromQuote(1);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task CreateFromQuote_QuoteNotApproved_ReturnsConflict()
    {
        // Arrange
        _mockOrderService.Setup(s => s.CreateFromQuoteAsync(1))
            .ThrowsAsync(new InvalidOperationException("Quote is not approved"));

        // Act
        var result = await _controller.CreateFromQuote(1);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(409);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_ValidOrder_ReturnsOkWithUpdatedOrder()
    {
        // Arrange
        var updateDto = new UpdateOrderDto
        {
            Id = 1,
            ShippingAddress = "New Address"
        };

        var updatedOrder = new OrderDto
        {
            Id = 1,
            ShippingAddress = "New Address"
        };

        _mockOrderService.Setup(s => s.UpdateAsync(It.IsAny<UpdateOrderDto>()))
            .ReturnsAsync(updatedOrder);

        // Act
        var result = await _controller.Update(1, updateDto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task Update_IdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var updateDto = new UpdateOrderDto { Id = 2 };

        // Act
        var result = await _controller.Update(1, updateDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_OrderAlreadyShipped_ReturnsConflict()
    {
        // Arrange
        var updateDto = new UpdateOrderDto { Id = 1 };

        _mockOrderService.Setup(s => s.UpdateAsync(It.IsAny<UpdateOrderDto>()))
            .ThrowsAsync(new InvalidOperationException("Cannot modify shipped order"));

        // Act
        var result = await _controller.Update(1, updateDto);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(409);
    }

    #endregion

    #region Status Management Tests

    [Fact]
    public async Task ConfirmOrder_ValidOrder_ReturnsOk()
    {
        // Arrange
        _mockOrderService.Setup(s => s.ConfirmAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ConfirmOrder(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task ConfirmOrder_InsufficientInventory_ReturnsConflict()
    {
        // Arrange
        _mockOrderService.Setup(s => s.ConfirmAsync(1))
            .ThrowsAsync(new InvalidOperationException("Insufficient inventory"));

        // Act
        var result = await _controller.ConfirmOrder(1);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task ShipOrder_ValidOrder_ReturnsOk()
    {
        // Arrange
        var shipmentDto = new ShipOrderDto
        {
            TrackingNumber = "TRACK123",
            Carrier = "UPS",
            ShippedDate = DateTime.Today
        };

        _mockOrderService.Setup(s => s.ShipAsync(1, shipmentDto))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ShipOrder(1, shipmentDto);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task DeliverOrder_ValidOrder_ReturnsOk()
    {
        // Arrange
        _mockOrderService.Setup(s => s.DeliverAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeliverOrder(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task CancelOrder_ValidOrder_ReturnsOk()
    {
        // Arrange
        _mockOrderService.Setup(s => s.CancelAsync(1, "Customer request"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.CancelOrder(1, "Customer request");

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task CancelOrder_AlreadyShipped_ReturnsConflict()
    {
        // Arrange
        _mockOrderService.Setup(s => s.CancelAsync(1, "Cancel"))
            .ThrowsAsync(new InvalidOperationException("Cannot cancel shipped order"));

        // Act
        var result = await _controller.CancelOrder(1, "Cancel");

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task HoldOrder_ValidOrder_ReturnsOk()
    {
        // Arrange
        _mockOrderService.Setup(s => s.HoldAsync(1, "Payment pending"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.HoldOrder(1, "Payment pending");

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task ReleaseHold_ValidOrder_ReturnsOk()
    {
        // Arrange
        _mockOrderService.Setup(s => s.ReleaseHoldAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ReleaseHold(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Line Item Tests

    [Fact]
    public async Task GetLineItems_ValidOrder_ReturnsLineItems()
    {
        // Arrange
        var lineItems = new List<OrderLineItemDto>
        {
            new OrderLineItemDto { Id = 1, ProductId = 1, Quantity = 2 },
            new OrderLineItemDto { Id = 2, ProductId = 2, Quantity = 1 }
        };

        _mockOrderService.Setup(s => s.GetLineItemsAsync(1))
            .ReturnsAsync(lineItems);

        // Act
        var result = await _controller.GetLineItems(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task AddLineItem_ValidItem_ReturnsCreated()
    {
        // Arrange
        var createDto = new CreateOrderLineItemDto
        {
            ProductId = 1,
            Quantity = 2,
            UnitPrice = 100
        };

        var createdItem = new OrderLineItemDto { Id = 1, ProductId = 1 };

        _mockOrderService.Setup(s => s.AddLineItemAsync(1, createDto))
            .ReturnsAsync(createdItem);

        // Act
        var result = await _controller.AddLineItem(1, createDto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task UpdateLineItem_ValidItem_ReturnsOk()
    {
        // Arrange
        var updateDto = new UpdateOrderLineItemDto
        {
            Id = 1,
            Quantity = 5
        };

        var updatedItem = new OrderLineItemDto { Id = 1, Quantity = 5 };

        _mockOrderService.Setup(s => s.UpdateLineItemAsync(1, updateDto))
            .ReturnsAsync(updatedItem);

        // Act
        var result = await _controller.UpdateLineItem(1, 1, updateDto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task RemoveLineItem_ValidItem_ReturnsNoContent()
    {
        // Arrange
        _mockOrderService.Setup(s => s.RemoveLineItemAsync(1, 1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.RemoveLineItem(1, 1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    #endregion

    #region Invoice Tests

    [Fact]
    public async Task GenerateInvoice_ValidOrder_ReturnsCreated()
    {
        // Arrange
        var invoice = new InvoiceDto { Id = 1, OrderId = 1, InvoiceNumber = "INV-2024-001" };

        _mockOrderService.Setup(s => s.GenerateInvoiceAsync(1))
            .ReturnsAsync(invoice);

        // Act
        var result = await _controller.GenerateInvoice(1);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task GenerateInvoice_AlreadyInvoiced_ReturnsConflict()
    {
        // Arrange
        _mockOrderService.Setup(s => s.GenerateInvoiceAsync(1))
            .ThrowsAsync(new InvalidOperationException("Order already invoiced"));

        // Act
        var result = await _controller.GenerateInvoice(1);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task GetInvoice_ValidOrder_ReturnsInvoice()
    {
        // Arrange
        var invoice = new InvoiceDto { Id = 1, OrderId = 1 };

        _mockOrderService.Setup(s => s.GetInvoiceAsync(1))
            .ReturnsAsync(invoice);

        // Act
        var result = await _controller.GetInvoice(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion

    #region Fulfillment Tests

    [Fact]
    public async Task GetFulfillmentStatus_ReturnsStatus()
    {
        // Arrange
        var status = new FulfillmentStatusDto
        {
            OrderId = 1,
            TotalItems = 5,
            ShippedItems = 3,
            DeliveredItems = 2
        };

        _mockOrderService.Setup(s => s.GetFulfillmentStatusAsync(1))
            .ReturnsAsync(status);

        // Act
        var result = await _controller.GetFulfillmentStatus(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task UpdateTracking_ValidData_ReturnsOk()
    {
        // Arrange
        var trackingDto = new UpdateTrackingDto
        {
            TrackingNumber = "TRACK456",
            Carrier = "FedEx"
        };

        _mockOrderService.Setup(s => s.UpdateTrackingAsync(1, trackingDto))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateTracking(1, trackingDto);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Return/Refund Tests

    [Fact]
    public async Task CreateReturn_ValidOrder_ReturnsCreated()
    {
        // Arrange
        var returnDto = new CreateReturnDto
        {
            OrderId = 1,
            Reason = "Defective product",
            LineItems = new List<ReturnLineItemDto>
            {
                new ReturnLineItemDto { OrderLineItemId = 1, Quantity = 1 }
            }
        };

        var createdReturn = new ReturnDto { Id = 1, OrderId = 1 };

        _mockOrderService.Setup(s => s.CreateReturnAsync(returnDto))
            .ReturnsAsync(createdReturn);

        // Act
        var result = await _controller.CreateReturn(1, returnDto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task ProcessRefund_ValidReturn_ReturnsOk()
    {
        // Arrange
        var refundDto = new ProcessRefundDto
        {
            ReturnId = 1,
            RefundAmount = 100,
            RefundMethod = "Credit"
        };

        _mockOrderService.Setup(s => s.ProcessRefundAsync(1, refundDto))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ProcessRefund(1, refundDto);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Search and Export Tests

    [Fact]
    public async Task Search_ValidQuery_ReturnsMatchingOrders()
    {
        // Arrange
        var orders = new List<OrderDto>
        {
            new OrderDto { Id = 1, OrderNumber = "ORD-2024-001" }
        };

        _mockOrderService.Setup(s => s.SearchAsync("ORD-2024"))
            .ReturnsAsync(orders);

        // Act
        var result = await _controller.Search("ORD-2024");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task Export_ValidRequest_ReturnsFile()
    {
        // Arrange
        var exportData = new byte[] { 1, 2, 3 };

        _mockOrderService.Setup(s => s.ExportAsync("csv"))
            .ReturnsAsync(exportData);

        // Act
        var result = await _controller.Export("csv");

        // Assert
        result.Should().BeOfType<FileContentResult>();
    }

    #endregion

    #region Bulk Operations Tests

    [Fact]
    public async Task BulkConfirm_ValidOrders_ReturnsCount()
    {
        // Arrange
        var orderIds = new List<int> { 1, 2, 3 };

        _mockOrderService.Setup(s => s.BulkConfirmAsync(orderIds))
            .ReturnsAsync(3);

        // Act
        var result = await _controller.BulkConfirm(orderIds);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task BulkCancel_ValidOrders_ReturnsCount()
    {
        // Arrange
        var request = new BulkCancelOrdersRequest
        {
            OrderIds = new List<int> { 1, 2 },
            Reason = "Bulk cancellation"
        };

        _mockOrderService.Setup(s => s.BulkCancelAsync(request.OrderIds, request.Reason))
            .ReturnsAsync(2);

        // Act
        var result = await _controller.BulkCancel(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetStatistics_ReturnsStats()
    {
        // Arrange
        var stats = new OrderStatisticsDto
        {
            TotalOrders = 1000,
            TotalRevenue = 500000,
            AverageOrderValue = 500,
            PendingOrders = 50
        };

        _mockOrderService.Setup(s => s.GetStatisticsAsync())
            .ReturnsAsync(stats);

        // Act
        var result = await _controller.GetStatistics();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_ExistingOrder_ReturnsNoContent()
    {
        // Arrange
        _mockOrderService.Setup(s => s.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_NonExistingOrder_ReturnsNotFound()
    {
        // Arrange
        _mockOrderService.Setup(s => s.DeleteAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_OrderWithInvoice_ReturnsConflict()
    {
        // Arrange
        _mockOrderService.Setup(s => s.DeleteAsync(1))
            .ThrowsAsync(new InvalidOperationException("Cannot delete order with invoice"));

        // Act
        var result = await _controller.Delete(1);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(409);
    }

    #endregion
}
