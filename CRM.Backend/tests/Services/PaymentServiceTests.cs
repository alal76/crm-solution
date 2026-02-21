// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
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

public class PaymentServiceTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<PaymentService>> _mockLogger;
    private readonly PaymentService _service;

    public PaymentServiceTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<PaymentService>>();
        _service = new PaymentService(_mockContext.Object, _mockLogger.Object);
    }

    private void SetupDbSets(
        List<Payment>? payments = null,
        List<Invoice>? invoices = null)
    {
        payments ??= new List<Payment>();
        invoices ??= new List<Invoice>();

        var mockPayments = MockDbSetFactory.CreateMockDbSet(payments);
        mockPayments.Setup(m => m.Add(It.IsAny<Payment>())).Callback<Payment>(e => payments.Add(e));
        mockPayments.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns<object[], CancellationToken>((keys, _) => mockPayments.Object.FindAsync(keys));
        _mockContext.Setup(c => c.Payments).Returns(mockPayments.Object);

        var mockInvoices = MockDbSetFactory.CreateMockDbSet(invoices);
        mockInvoices.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns<object[], CancellationToken>((keys, _) => mockInvoices.Object.FindAsync(keys));
        _mockContext.Setup(c => c.Invoices).Returns(mockInvoices.Object);

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    private static Payment CreateTestPayment(
        int id = 1,
        PaymentStatus status = PaymentStatus.Completed,
        decimal amount = 500m,
        int invoiceId = 1,
        int accountId = 10,
        PaymentType paymentType = PaymentType.Payment)
    {
        return new Payment
        {
            Id = id,
            PaymentNumber = $"PAY-{id:D4}",
            Status = status,
            Amount = amount,
            AmountApplied = amount,
            InvoiceId = invoiceId,
            AccountId = accountId,
            PaymentMethod = PaymentMethod.CreditCard,
            PaymentType = paymentType,
            PaymentDate = DateTime.UtcNow,
            RefundedAmount = 0m,
            RetryCount = 0,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static Invoice CreateTestInvoice(int id = 1, decimal totalAmount = 1000m, decimal amountPaid = 0m)
    {
        return new Invoice
        {
            Id = id,
            InvoiceNumber = $"INV-{id:D4}",
            Status = InvoiceStatus.Sent,
            TotalAmount = totalAmount,
            AmountPaid = amountPaid,
            AccountId = 10,
            DueDate = DateTime.UtcNow.AddDays(30),
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    // ========================================================================
    // GetAllAsync
    // ========================================================================
    [Fact]
    public async Task GetAllAsync_ShouldReturnAllPayments_WhenNoFilter()
    {
        // Arrange
        var payments = new List<Payment>
        {
            CreateTestPayment(1),
            CreateTestPayment(2),
            CreateTestPayment(3)
        };
        SetupDbSets(payments: payments);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByInvoiceId()
    {
        // Arrange
        var payments = new List<Payment>
        {
            CreateTestPayment(1, invoiceId: 1),
            CreateTestPayment(2, invoiceId: 2),
            CreateTestPayment(3, invoiceId: 1)
        };
        SetupDbSets(payments: payments);

        // Act
        var result = await _service.GetAllAsync(invoiceId: 1);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_ShouldExcludeDeleted()
    {
        // Arrange
        var payments = new List<Payment>
        {
            CreateTestPayment(1),
            new Payment { Id = 2, PaymentNumber = "DEL", IsDeleted = true, CreatedAt = DateTime.UtcNow }
        };
        SetupDbSets(payments: payments);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(1);
    }

    // ========================================================================
    // GetByIdAsync / GetByTransactionIdAsync
    // ========================================================================
    [Fact]
    public async Task GetByIdAsync_ShouldReturnPayment_WhenExists()
    {
        // Arrange
        var payments = new List<Payment> { CreateTestPayment(1) };
        SetupDbSets(payments: payments);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetByTransactionIdAsync_ShouldReturnPayment_WhenExists()
    {
        // Arrange
        var payment = CreateTestPayment(1);
        payment.GatewayTransactionId = "txn_abc123";
        SetupDbSets(payments: new List<Payment> { payment });

        // Act
        var result = await _service.GetByTransactionIdAsync("txn_abc123");

        // Assert
        result.Should().NotBeNull();
        result!.TransactionId.Should().Be("txn_abc123");
    }

    // ========================================================================
    // DeleteAsync (Soft Delete)
    // ========================================================================
    [Fact]
    public async Task DeleteAsync_ShouldSoftDelete_WhenExists()
    {
        // Arrange
        var payment = CreateTestPayment(1);
        SetupDbSets(payments: new List<Payment> { payment });

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        result.Should().BeTrue();
        payment.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenNotFound()
    {
        // Arrange
        SetupDbSets();

        // Act
        var result = await _service.DeleteAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    // ========================================================================
    // ProcessPaymentAsync
    // ========================================================================
    [Fact]
    public async Task ProcessPaymentAsync_ShouldCreatePaymentAndUpdateInvoice()
    {
        // Arrange
        var invoice = CreateTestInvoice(1, totalAmount: 1000m, amountPaid: 0m);
        var payments = new List<Payment>();
        SetupDbSets(payments: payments, invoices: new List<Invoice> { invoice });

        // Act
        var result = await _service.ProcessPaymentAsync(1, 1000m, PaymentMethod.CreditCard, new PaymentDetails());

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Payment.Should().NotBeNull();
        invoice.AmountPaid.Should().Be(1000m);
        invoice.Status.Should().Be(InvoiceStatus.Paid);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldSetPartiallyPaid_WhenPartialAmount()
    {
        // Arrange
        var invoice = CreateTestInvoice(1, totalAmount: 1000m, amountPaid: 0m);
        SetupDbSets(payments: new List<Payment>(), invoices: new List<Invoice> { invoice });

        // Act
        var result = await _service.ProcessPaymentAsync(1, 400m, PaymentMethod.BankTransfer, new PaymentDetails());

        // Assert
        result.Success.Should().BeTrue();
        invoice.AmountPaid.Should().Be(400m);
        invoice.Status.Should().Be(InvoiceStatus.PartiallyPaid);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldRejectOverpayment()
    {
        // Arrange
        var invoice = CreateTestInvoice(1, totalAmount: 1000m, amountPaid: 900m);
        SetupDbSets(payments: new List<Payment>(), invoices: new List<Invoice> { invoice });

        // Act
        var result = await _service.ProcessPaymentAsync(1, 200m, PaymentMethod.CreditCard, new PaymentDetails());

        // Assert
        result.Success.Should().BeFalse();
    }

    // ========================================================================
    // ProcessRefundAsync
    // ========================================================================
    [Fact]
    public async Task ProcessRefundAsync_ShouldCreateRefund_WhenPaymentCompleted()
    {
        // Arrange
        var payment = CreateTestPayment(1, status: PaymentStatus.Completed, amount: 500m);
        payment.RefundedAmount = 0m;
        var payments = new List<Payment> { payment };
        var invoice = CreateTestInvoice(1, totalAmount: 1000m, amountPaid: 500m);
        SetupDbSets(payments: payments, invoices: new List<Invoice> { invoice });

        // Act
        var result = await _service.ProcessRefundAsync(1, 200m, "Customer request");

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessRefundAsync_ShouldReject_WhenPaymentNotCompleted()
    {
        // Arrange
        var payment = CreateTestPayment(1, status: PaymentStatus.Pending, amount: 500m);
        SetupDbSets(payments: new List<Payment> { payment });

        // Act
        var result = await _service.ProcessRefundAsync(1, 200m, "Refund");

        // Assert
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task ProcessRefundAsync_ShouldReject_WhenRefundExceedsOriginalAmount()
    {
        // Arrange
        var payment = CreateTestPayment(1, status: PaymentStatus.Completed, amount: 500m);
        payment.RefundedAmount = 400m;
        SetupDbSets(payments: new List<Payment> { payment });

        // Act
        var result = await _service.ProcessRefundAsync(1, 200m, "Excess refund");

        // Assert
        result.Success.Should().BeFalse();
    }

    // ========================================================================
    // VoidPaymentAsync
    // ========================================================================
    [Fact]
    public async Task VoidPaymentAsync_ShouldVoid_WhenPending()
    {
        // Arrange
        var payment = CreateTestPayment(1, status: PaymentStatus.Pending);
        SetupDbSets(payments: new List<Payment> { payment });

        // Act
        var result = await _service.VoidPaymentAsync(1, "Cancelled by customer");

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Payment!.Status.Should().Be(PaymentStatus.Voided);
    }

    [Fact]
    public async Task VoidPaymentAsync_ShouldFail_WhenCompleted()
    {
        // Arrange
        var payment = CreateTestPayment(1, status: PaymentStatus.Completed);
        SetupDbSets(payments: new List<Payment> { payment });

        // Act
        var result = await _service.VoidPaymentAsync(1, "Attempt void");

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("CANNOT_VOID");
    }

    // ========================================================================
    // CapturePaymentAsync
    // ========================================================================
    [Fact]
    public async Task CapturePaymentAsync_ShouldCapture_WhenAuthorizationType()
    {
        // Arrange
        var payment = CreateTestPayment(1, status: PaymentStatus.Completed);
        payment.PaymentType = PaymentType.Authorization;
        SetupDbSets(payments: new List<Payment> { payment });

        // Act
        var result = await _service.CapturePaymentAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }

    // ========================================================================
    // RetryPaymentAsync
    // ========================================================================
    [Fact]
    public async Task RetryPaymentAsync_ShouldRetry_WhenFailed()
    {
        // Arrange
        var payment = CreateTestPayment(1, status: PaymentStatus.Failed);
        payment.RetryCount = 1;
        payment.InvoiceId = 1;
        var invoice = CreateTestInvoice(1, totalAmount: 500m, amountPaid: 0m);
        SetupDbSets(payments: new List<Payment> { payment }, invoices: new List<Invoice> { invoice });

        // Act
        var result = await _service.RetryPaymentAsync(1);

        // Assert
        result.Should().NotBeNull();
    }

    // ========================================================================
    // GetStatisticsAsync
    // ========================================================================
    [Fact]
    public async Task GetStatisticsAsync_ShouldReturnCorrectCounts()
    {
        // Arrange
        var payments = new List<Payment>
        {
            CreateTestPayment(1, status: PaymentStatus.Completed, amount: 500m),
            CreateTestPayment(2, status: PaymentStatus.Completed, amount: 300m),
            CreateTestPayment(3, status: PaymentStatus.Failed, amount: 200m)
        };
        SetupDbSets(payments: payments);

        // Act
        var result = await _service.GetStatisticsAsync();

        // Assert
        result.Should().NotBeNull();
        result.TotalPayments.Should().Be(3);
        result.SuccessfulPayments.Should().Be(2);
        result.FailedPayments.Should().Be(1);
    }

    // ========================================================================
    // MarkAsFailedAsync
    // ========================================================================
    [Fact]
    public async Task MarkAsFailedAsync_ShouldIncrementRetryCount()
    {
        // Arrange
        var payment = CreateTestPayment(1, status: PaymentStatus.Processing);
        payment.RetryCount = 0;
        SetupDbSets(payments: new List<Payment> { payment });

        // Act
        var result = await _service.MarkAsFailedAsync(1, "Gateway timeout");

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(PaymentStatus.Failed);
        result.FailureReason.Should().Be("Gateway timeout");
        result.RetryCount.Should().Be(1);
    }

    // ========================================================================
    // GetPendingPaymentsAsync
    // ========================================================================
    [Fact]
    public async Task GetPendingPaymentsAsync_ShouldReturnOnlyPending()
    {
        // Arrange
        var payments = new List<Payment>
        {
            CreateTestPayment(1, status: PaymentStatus.Pending),
            CreateTestPayment(2, status: PaymentStatus.Completed),
            CreateTestPayment(3, status: PaymentStatus.Pending)
        };
        SetupDbSets(payments: payments);

        // Act
        var result = await _service.GetPendingPaymentsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(p => p.Status == PaymentStatus.Pending);
    }
}
