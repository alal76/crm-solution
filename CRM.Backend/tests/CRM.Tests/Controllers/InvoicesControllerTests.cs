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
/// Unit tests for InvoicesController.
/// Tests invoice lifecycle endpoints including CRUD, status management, and voiding.
/// </summary>
public class InvoicesControllerTests
{
    private readonly Mock<IInvoiceService> _mockInvoiceService;
    private readonly Mock<ILogger<InvoicesController>> _mockLogger;
    private readonly InvoicesController _controller;

    public InvoicesControllerTests()
    {
        _mockInvoiceService = new Mock<IInvoiceService>();
        _mockLogger = new Mock<ILogger<InvoicesController>>();
        _controller = new InvoicesController(_mockInvoiceService.Object, _mockLogger.Object);
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WithNoFilters_ReturnsOkWithInvoices()
    {
        // Arrange
        var invoices = new List<Invoice>
        {
            new() { Id = 1, InvoiceNumber = "INV-001", AccountId = 10, Status = InvoiceStatus.Draft, LineItems = new List<InvoiceLineItem>() },
            new() { Id = 2, InvoiceNumber = "INV-002", AccountId = 10, Status = InvoiceStatus.Sent, LineItems = new List<InvoiceLineItem>() }
        };
        _mockInvoiceService
            .Setup(s => s.GetAllAsync(null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoices);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedInvoices = okResult.Value.Should().BeAssignableTo<IEnumerable<InvoiceDto>>().Subject;
        returnedInvoices.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_WithAccountIdFilter_PassesFilterToService()
    {
        // Arrange
        _mockInvoiceService
            .Setup(s => s.GetAllAsync(5, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Invoice>());

        // Act
        var result = await _controller.GetAll(accountId: 5);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        _mockInvoiceService.Verify(s => s.GetAllAsync(5, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithValidId_ReturnsOkWithInvoiceDto()
    {
        // Arrange
        var invoice = new Invoice
        {
            Id = 1,
            InvoiceNumber = "INV-001",
            AccountId = 10,
            Status = InvoiceStatus.Draft,
            LineItems = new List<InvoiceLineItem>()
        };
        _mockInvoiceService
            .Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = okResult.Value.Should().BeOfType<InvoiceDto>().Subject;
        dto.Id.Should().Be(1);
        dto.InvoiceNumber.Should().Be("INV-001");
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        _mockInvoiceService
            .Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Invoice?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WithValidRequest_ReturnsCreatedAtAction()
    {
        // Arrange
        var request = new CreateInvoiceDto
        {
            AccountId = 10,
            Subtotal = 500m,
            CurrencyCode = "USD"
        };
        var createdInvoice = new Invoice
        {
            Id = 1,
            InvoiceNumber = "INV-001",
            AccountId = 10,
            Status = InvoiceStatus.Draft,
            LineItems = new List<InvoiceLineItem>()
        };
        _mockInvoiceService
            .Setup(s => s.CreateAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdInvoice);

        // Act
        var result = await _controller.Create(request);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(InvoicesController.GetById));
        createdResult.StatusCode.Should().Be(201);
        var dto = createdResult.Value.Should().BeOfType<InvoiceDto>().Subject;
        dto.Id.Should().Be(1);
    }

    #endregion

    #region UpdateStatus Tests

    [Fact]
    public async Task UpdateStatus_WithValidId_ReturnsOkWithUpdatedDto()
    {
        // Arrange
        var updatedInvoice = new Invoice
        {
            Id = 1,
            InvoiceNumber = "INV-001",
            AccountId = 10,
            Status = InvoiceStatus.Sent,
            LineItems = new List<InvoiceLineItem>()
        };
        _mockInvoiceService
            .Setup(s => s.UpdateStatusAsync(1, InvoiceStatus.Sent, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedInvoice);

        var request = new InvoicesController.InvoiceStatusRequest { Status = InvoiceStatus.Sent };

        // Act
        var result = await _controller.UpdateStatus(1, request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = okResult.Value.Should().BeOfType<InvoiceDto>().Subject;
        dto.Status.Should().Be(InvoiceStatus.Sent);
    }

    [Fact]
    public async Task UpdateStatus_WhenServiceThrowsNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockInvoiceService
            .Setup(s => s.UpdateStatusAsync(999, InvoiceStatus.Sent, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Invoice 999 not found"));

        var request = new InvoicesController.InvoiceStatusRequest { Status = InvoiceStatus.Sent };

        // Act
        var result = await _controller.UpdateStatus(999, request);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region VoidInvoice Tests

    [Fact]
    public async Task VoidInvoice_WithValidId_ReturnsOkWithVoidedDto()
    {
        // Arrange
        var voidedInvoice = new Invoice
        {
            Id = 1,
            InvoiceNumber = "INV-001",
            AccountId = 10,
            Status = InvoiceStatus.Voided,
            VoidReason = "Duplicate",
            LineItems = new List<InvoiceLineItem>()
        };
        _mockInvoiceService
            .Setup(s => s.VoidAsync(1, "Duplicate", It.IsAny<CancellationToken>()))
            .ReturnsAsync(voidedInvoice);

        var request = new InvoicesController.VoidRequest { Reason = "Duplicate" };

        // Act
        var result = await _controller.VoidInvoice(1, request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = okResult.Value.Should().BeOfType<InvoiceDto>().Subject;
        dto.Status.Should().Be(InvoiceStatus.Voided);
        dto.VoidReason.Should().Be("Duplicate");
    }

    [Fact]
    public async Task VoidInvoice_WhenServiceThrowsNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockInvoiceService
            .Setup(s => s.VoidAsync(999, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Invoice 999 not found"));

        var request = new InvoicesController.VoidRequest { Reason = "Test" };

        // Act
        var result = await _controller.VoidInvoice(999, request);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion
}
