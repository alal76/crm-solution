// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Api.Controllers;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for PaymentsController.
/// Tests payment lifecycle endpoints including CRUD, status management, and completion.
/// </summary>
public class PaymentsControllerTests
{
    private readonly Mock<IPaymentService> _mockPaymentService;
    private readonly Mock<ILogger<PaymentsController>> _mockLogger;
    private readonly PaymentsController _controller;

    public PaymentsControllerTests()
    {
        _mockPaymentService = new Mock<IPaymentService>();
        _mockLogger = new Mock<ILogger<PaymentsController>>();
        _controller = new PaymentsController(_mockPaymentService.Object, _mockLogger.Object);
    }

    private static Payment CreatePayment(int id = 1, int accountId = 10) => new()
    {
        Id = id,
        PaymentNumber = $"PAY-00{id}",
        AccountId = accountId,
        Amount = 250m,
        Status = PaymentStatus.Pending,
        PaymentMethod = PaymentMethod.CreditCard,
        PaymentType = PaymentType.Payment
    };

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WithNoFilters_ReturnsOkWithPayments()
    {
        // Arrange
        var payments = new List<Payment>
        {
            CreatePayment(1),
            CreatePayment(2)
        };
        _mockPaymentService
            .Setup(s => s.GetAllAsync(null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payments);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedPayments = okResult.Value.Should().BeAssignableTo<IEnumerable<PaymentDto>>().Subject;
        returnedPayments.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_WithInvoiceIdFilter_PassesFilterToService()
    {
        // Arrange
        _mockPaymentService
            .Setup(s => s.GetAllAsync(null, 7, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Payment>());

        // Act
        var result = await _controller.GetAll(invoiceId: 7);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        _mockPaymentService.Verify(s => s.GetAllAsync(null, 7, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithValidId_ReturnsOkWithPaymentDto()
    {
        // Arrange
        var payment = CreatePayment(1);
        _mockPaymentService
            .Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = okResult.Value.Should().BeOfType<PaymentDto>().Subject;
        dto.Id.Should().Be(1);
        dto.PaymentNumber.Should().Be("PAY-001");
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        _mockPaymentService
            .Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WithValidDto_ReturnsCreatedAtAction()
    {
        // Arrange
        var dto = new CreatePaymentDto
        {
            AccountId = 10,
            Amount = 250m,
            PaymentMethod = PaymentMethod.CreditCard
        };
        var createdPayment = CreatePayment(1);
        _mockPaymentService
            .Setup(s => s.CreateAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdPayment);

        // Act
        var result = await _controller.Create(dto);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(PaymentsController.GetById));
        createdResult.StatusCode.Should().Be(201);
        var paymentDto = createdResult.Value.Should().BeOfType<PaymentDto>().Subject;
        paymentDto.Id.Should().Be(1);
    }

    #endregion

    #region UpdateStatus Tests

    [Fact]
    public async Task UpdateStatus_WithValidId_ReturnsOkWithUpdatedDto()
    {
        // Arrange
        var updatedPayment = CreatePayment(1);
        updatedPayment.Status = PaymentStatus.Completed;

        _mockPaymentService
            .Setup(s => s.UpdateStatusAsync(1, PaymentStatus.Completed, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedPayment);

        var request = new PaymentsController.PaymentStatusRequest { Status = PaymentStatus.Completed };

        // Act
        var result = await _controller.UpdateStatus(1, request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = okResult.Value.Should().BeOfType<PaymentDto>().Subject;
        dto.Status.Should().Be(PaymentStatus.Completed);
    }

    [Fact]
    public async Task UpdateStatus_WhenServiceThrowsNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockPaymentService
            .Setup(s => s.UpdateStatusAsync(999, PaymentStatus.Completed, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Payment 999 not found"));

        var request = new PaymentsController.PaymentStatusRequest { Status = PaymentStatus.Completed };

        // Act
        var result = await _controller.UpdateStatus(999, request);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region MarkAsCompleted Tests

    [Fact]
    public async Task MarkAsCompleted_WithValidId_ReturnsOkWithCompletedDto()
    {
        // Arrange
        var completedPayment = CreatePayment(1);
        completedPayment.Status = PaymentStatus.Completed;

        _mockPaymentService
            .Setup(s => s.MarkAsCompletedAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(completedPayment);

        // Act
        var result = await _controller.MarkAsCompleted(1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = okResult.Value.Should().BeOfType<PaymentDto>().Subject;
        dto.Status.Should().Be(PaymentStatus.Completed);
    }

    [Fact]
    public async Task MarkAsCompleted_WhenServiceThrowsNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockPaymentService
            .Setup(s => s.MarkAsCompletedAsync(999, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Payment 999 not found"));

        // Act
        var result = await _controller.MarkAsCompleted(999);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion
}
