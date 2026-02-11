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

public class InvoiceServiceTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<InvoiceService>> _mockLogger;
    private readonly InvoiceService _service;

    public InvoiceServiceTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<InvoiceService>>();
        _service = new InvoiceService(_mockContext.Object, _mockLogger.Object);
    }

    private void SetupDbSets(
        List<Invoice>? invoices = null,
        List<Payment>? payments = null,
        List<InvoiceLineItem>? lineItems = null,
        List<Order>? orders = null,
        List<Quote>? quotes = null)
    {
        invoices ??= new List<Invoice>();
        payments ??= new List<Payment>();
        lineItems ??= new List<InvoiceLineItem>();
        orders ??= new List<Order>();
        quotes ??= new List<Quote>();

        var mockInvoices = MockDbSetFactory.CreateMockDbSet(invoices);
        mockInvoices.Setup(m => m.Add(It.IsAny<Invoice>())).Callback<Invoice>(e => invoices.Add(e));
        mockInvoices.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns<object[], CancellationToken>((keys, _) => mockInvoices.Object.FindAsync(keys));
        _mockContext.Setup(c => c.Invoices).Returns(mockInvoices.Object);

        var mockPayments = MockDbSetFactory.CreateMockDbSet(payments);
        mockPayments.Setup(m => m.Add(It.IsAny<Payment>())).Callback<Payment>(e => payments.Add(e));
        _mockContext.Setup(c => c.Payments).Returns(mockPayments.Object);

        var mockLineItems = MockDbSetFactory.CreateMockDbSet(lineItems);
        mockLineItems.Setup(m => m.Add(It.IsAny<InvoiceLineItem>())).Callback<InvoiceLineItem>(e => lineItems.Add(e));
        mockLineItems.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns<object[], CancellationToken>((keys, _) => mockLineItems.Object.FindAsync(keys));
        _mockContext.Setup(c => c.InvoiceLineItems).Returns(mockLineItems.Object);

        var mockOrders = MockDbSetFactory.CreateMockDbSet(orders);
        _mockContext.Setup(c => c.Orders).Returns(mockOrders.Object);

        var mockQuotes = MockDbSetFactory.CreateMockDbSet(quotes);
        _mockContext.Setup(c => c.Quotes).Returns(mockQuotes.Object);

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    private static Invoice CreateTestInvoice(int id = 1, InvoiceStatus status = InvoiceStatus.Draft, decimal totalAmount = 1000m, decimal amountPaid = 0m, int accountId = 10)
    {
        return new Invoice
        {
            Id = id,
            InvoiceNumber = $"INV-{id:D4}",
            Status = status,
            TotalAmount = totalAmount,
            AmountPaid = amountPaid,
            AccountId = accountId,
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
    }

    // ========================================================================
    // GetAllAsync
    // ========================================================================
    [Fact]
    public async Task GetAllAsync_ShouldReturnAllInvoices_WhenNoFilterProvided()
    {
        // Arrange
        var invoices = new List<Invoice>
        {
            CreateTestInvoice(1),
            CreateTestInvoice(2),
            CreateTestInvoice(3, accountId: 20)
        };
        SetupDbSets(invoices: invoices);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByCustomerId_WhenProvided()
    {
        // Arrange
        var invoices = new List<Invoice>
        {
            CreateTestInvoice(1, accountId: 10),
            CreateTestInvoice(2, accountId: 20),
            CreateTestInvoice(3, accountId: 10)
        };
        SetupDbSets(invoices: invoices);

        // Act
        var result = await _service.GetAllAsync(customerId: 10);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(i => i.AccountId == 10);
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByStatus_WhenProvided()
    {
        // Arrange
        var invoices = new List<Invoice>
        {
            CreateTestInvoice(1, status: InvoiceStatus.Draft),
            CreateTestInvoice(2, status: InvoiceStatus.Paid),
            CreateTestInvoice(3, status: InvoiceStatus.Draft)
        };
        SetupDbSets(invoices: invoices);

        // Act
        var result = await _service.GetAllAsync(status: InvoiceStatus.Draft);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_ShouldExcludeDeletedInvoices()
    {
        // Arrange
        var invoices = new List<Invoice>
        {
            CreateTestInvoice(1),
            new Invoice { Id = 2, InvoiceNumber = "DEL", IsDeleted = true, CreatedAt = DateTime.UtcNow }
        };
        SetupDbSets(invoices: invoices);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(1);
    }

    // ========================================================================
    // GetByIdAsync
    // ========================================================================
    [Fact]
    public async Task GetByIdAsync_ShouldReturnInvoice_WhenExists()
    {
        // Arrange
        var invoices = new List<Invoice> { CreateTestInvoice(1) };
        SetupDbSets(invoices: invoices);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        SetupDbSets(invoices: new List<Invoice>());

        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    // ========================================================================
    // CreateAsync
    // ========================================================================
    [Fact]
    public async Task CreateAsync_ShouldSetCreatedAtAndGenerateNumber()
    {
        // Arrange
        var invoices = new List<Invoice>();
        SetupDbSets(invoices: invoices);

        var newInvoice = new Invoice
        {
            AccountId = 10,
            TotalAmount = 500m,
            DueDate = DateTime.UtcNow.AddDays(30)
        };

        // Act
        var result = await _service.CreateAsync(newInvoice);

        // Assert
        result.Should().NotBeNull();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.InvoiceNumber.Should().NotBeNullOrEmpty();
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    // ========================================================================
    // DeleteAsync (Soft Delete)
    // ========================================================================
    [Fact]
    public async Task DeleteAsync_ShouldSoftDelete_WhenInvoiceExists()
    {
        // Arrange
        var invoice = CreateTestInvoice(1);
        var invoices = new List<Invoice> { invoice };
        SetupDbSets(invoices: invoices);

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        result.Should().BeTrue();
        invoice.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenInvoiceNotFound()
    {
        // Arrange
        SetupDbSets(invoices: new List<Invoice>());

        // Act
        var result = await _service.DeleteAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    // ========================================================================
    // UpdateStatusAsync
    // ========================================================================
    [Fact]
    public async Task ApproveAsync_ShouldSetApproved_WhenInDraftStatus()
    {
        // Arrange
        var invoice = CreateTestInvoice(1, status: InvoiceStatus.Draft);
        SetupDbSets(invoices: new List<Invoice> { invoice });

        // Act
        var result = await _service.ApproveAsync(1, approvedById: 5);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(InvoiceStatus.Approved);
    }

    [Fact]
    public async Task VoidAsync_ShouldThrow_WhenInvoiceIsPaid()
    {
        // Arrange
        var invoice = CreateTestInvoice(1, status: InvoiceStatus.Paid);
        SetupDbSets(invoices: new List<Invoice> { invoice });

        // Act
        var act = () => _service.VoidAsync(1, "Test void");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SendInvoiceAsync_ShouldSetStatusToSent()
    {
        // Arrange
        var invoice = CreateTestInvoice(1, status: InvoiceStatus.Approved);
        SetupDbSets(invoices: new List<Invoice> { invoice });

        // Act
        var result = await _service.SendInvoiceAsync(1);

        // Assert
        result.Should().BeTrue();
        invoice.Status.Should().Be(InvoiceStatus.Sent);
        invoice.SentDate.Should().NotBeNull();
    }

    [Fact]
    public async Task MarkAsViewedAsync_ShouldSetViewed_WhenStatusIsSent()
    {
        // Arrange
        var invoice = CreateTestInvoice(1, status: InvoiceStatus.Sent);
        SetupDbSets(invoices: new List<Invoice> { invoice });

        // Act
        var result = await _service.MarkAsViewedAsync(1);

        // Assert
        result.Should().BeTrue();
        invoice.ViewedDate.Should().NotBeNull();
    }

    [Fact]
    public async Task MarkAsPaidAsync_ShouldSetStatusToPaid()
    {
        // Arrange
        var invoice = CreateTestInvoice(1, status: InvoiceStatus.Sent, totalAmount: 1000m);
        SetupDbSets(invoices: new List<Invoice> { invoice });

        // Act
        var result = await _service.MarkAsPaidAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(InvoiceStatus.Paid);
        result.PaidDate.Should().NotBeNull();
    }

    // ========================================================================
    // RecordPaymentAsync
    // ========================================================================
    [Fact]
    public async Task RecordPaymentAsync_ShouldCreatePaymentAndUpdateInvoice()
    {
        // Arrange
        var invoice = CreateTestInvoice(1, status: InvoiceStatus.Sent, totalAmount: 1000m, amountPaid: 0m);
        var invoices = new List<Invoice> { invoice };
        var payments = new List<Payment>();
        SetupDbSets(invoices: invoices, payments: payments);

        // Act
        var result = await _service.RecordPaymentAsync(1, 1000m, PaymentMethod.CreditCard);

        // Assert
        result.Should().NotBeNull();
        result.AmountPaid.Should().Be(1000m);
        result.Status.Should().Be(InvoiceStatus.Paid);
    }

    [Fact]
    public async Task RecordPaymentAsync_ShouldSetPartiallyPaid_WhenPartialPayment()
    {
        // Arrange
        var invoice = CreateTestInvoice(1, status: InvoiceStatus.Sent, totalAmount: 1000m, amountPaid: 0m);
        var invoices = new List<Invoice> { invoice };
        var payments = new List<Payment>();
        SetupDbSets(invoices: invoices, payments: payments);

        // Act
        var result = await _service.RecordPaymentAsync(1, 500m, PaymentMethod.BankTransfer);

        // Assert
        result.Should().NotBeNull();
        invoice.AmountPaid.Should().Be(500m);
        invoice.Status.Should().Be(InvoiceStatus.PartiallyPaid);
    }

    // ========================================================================
    // GetOverdueInvoicesAsync
    // ========================================================================
    [Fact]
    public async Task GetOverdueInvoicesAsync_ShouldReturnOverdueInvoices()
    {
        // Arrange
        var invoices = new List<Invoice>
        {
            new Invoice { Id = 1, InvoiceNumber = "INV-01", DueDate = DateTime.UtcNow.AddDays(-10), Status = InvoiceStatus.Sent, IsDeleted = false, CreatedAt = DateTime.UtcNow },
            new Invoice { Id = 2, InvoiceNumber = "INV-02", DueDate = DateTime.UtcNow.AddDays(10), Status = InvoiceStatus.Sent, IsDeleted = false, CreatedAt = DateTime.UtcNow },
            new Invoice { Id = 3, InvoiceNumber = "INV-03", DueDate = DateTime.UtcNow.AddDays(-5), Status = InvoiceStatus.Paid, IsDeleted = false, CreatedAt = DateTime.UtcNow }
        };
        SetupDbSets(invoices: invoices);

        // Act
        var result = await _service.GetOverdueInvoicesAsync();

        // Assert
        result.Should().HaveCountGreaterOrEqualTo(1);
    }

    // ========================================================================
    // Line Item Operations
    // ========================================================================
    [Fact]
    public async Task AddLineItemAsync_ShouldAddLineItemToInvoice()
    {
        // Arrange
        var invoice = CreateTestInvoice(1);
        var invoices = new List<Invoice> { invoice };
        var lineItems = new List<InvoiceLineItem>();
        SetupDbSets(invoices: invoices, lineItems: lineItems);

        var lineItem = new InvoiceLineItem
        {
            Name = "Consulting Service",
            Quantity = 10,
            UnitPrice = 100m,
            TotalAmount = 1000m
        };

        // Act
        var result = await _service.AddLineItemAsync(1, lineItem);

        // Assert
        result.Should().NotBeNull();
        result.InvoiceId.Should().Be(1);
        result.Name.Should().Be("Consulting Service");
    }

    [Fact]
    public async Task RemoveLineItemAsync_ShouldSoftDeleteLineItem()
    {
        // Arrange
        var lineItem = new InvoiceLineItem
        {
            Id = 1,
            InvoiceId = 1,
            Name = "Item",
            Quantity = 1,
            UnitPrice = 100m,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        var lineItems = new List<InvoiceLineItem> { lineItem };
        // RemoveLineItemAsync calls RecalculateTotalsAsync which uses .Include(i => i.LineItems)
        var invoice = CreateTestInvoice(1);
        invoice.LineItems = lineItems;
        SetupDbSets(invoices: new List<Invoice> { invoice }, lineItems: lineItems);

        // Act
        var result = await _service.RemoveLineItemAsync(1);

        // Assert
        result.Should().BeTrue();
        lineItem.IsDeleted.Should().BeTrue();
    }

    // ========================================================================
    // GenerateInvoiceNumberAsync
    // ========================================================================
    [Fact]
    public async Task GenerateInvoiceNumberAsync_ShouldReturnFormattedNumber()
    {
        // Arrange
        SetupDbSets(invoices: new List<Invoice>());

        // Act
        var result = await _service.GenerateInvoiceNumberAsync();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().StartWith("INV-");
    }

    // ========================================================================
    // RecalculateTotalsAsync
    // ========================================================================
    [Fact]
    public async Task RecalculateTotalsAsync_ShouldUpdateTotalsFromLineItems()
    {
        // Arrange
        var invoice = CreateTestInvoice(1);
        invoice.TotalAmount = 0;
        invoice.Subtotal = 0;
        var lineItems = new List<InvoiceLineItem>
        {
            new InvoiceLineItem { Id = 1, InvoiceId = 1, Quantity = 2, UnitPrice = 100m, TotalAmount = 200m, IsDeleted = false, CreatedAt = DateTime.UtcNow },
            new InvoiceLineItem { Id = 2, InvoiceId = 1, Quantity = 1, UnitPrice = 300m, TotalAmount = 300m, IsDeleted = false, CreatedAt = DateTime.UtcNow }
        };
        // Pre-populate navigation property (.Include is a no-op on mocked DbSets)
        invoice.LineItems = lineItems;
        SetupDbSets(invoices: new List<Invoice> { invoice }, lineItems: lineItems);

        // Act
        var result = await _service.RecalculateTotalsAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Subtotal.Should().BeGreaterThan(0);
    }

    // ========================================================================
    // CreateFromOrderAsync
    // ========================================================================
    [Fact]
    public async Task CreateFromOrderAsync_ShouldCreateInvoiceFromOrder()
    {
        // Arrange
        var order = new Order
        {
            Id = 1,
            OrderNumber = "ORD-0001",
            AccountId = 10,
            TotalAmount = 2000m,
            Subtotal = 2000m,
            Status = OrderStatus.Approved,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        var invoices = new List<Invoice>();
        var orders = new List<Order> { order };
        SetupDbSets(invoices: invoices, orders: orders);

        // Act
        var result = await _service.CreateFromOrderAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.OrderId.Should().Be(1);
        result.AccountId.Should().Be(10);
        result.TotalAmount.Should().Be(2000m);
    }
}
