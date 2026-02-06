// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Invoices Controller Unit Tests

using Xunit;
using Moq;
using FluentAssertions;
using CRM.Api.Controllers;
using CRM.Core.Dtos;
using CRM.Core.Entities;
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
/// Comprehensive unit tests for InvoicesController
/// Covers: Invoice CRUD, payments, status, PDF, aging
/// </summary>
public class InvoicesControllerTests
{
    private readonly Mock<IInvoiceService> _mockInvoiceService;
    private readonly Mock<ILogger<InvoicesController>> _mockLogger;
    private readonly Mock<ICrmNotificationService> _mockNotificationService;
    private readonly InvoicesController _controller;

    public InvoicesControllerTests()
    {
        _mockInvoiceService = new Mock<IInvoiceService>();
        _mockLogger = new Mock<ILogger<InvoicesController>>();
        _mockNotificationService = new Mock<ICrmNotificationService>();

        _mockNotificationService.Setup(x => x.NotifyRecordCreatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);
        _mockNotificationService.Setup(x => x.NotifyRecordUpdatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);
        _mockNotificationService.Setup(x => x.NotifyRecordDeletedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);

        _controller = new InvoicesController(_mockInvoiceService.Object, _mockLogger.Object, _mockNotificationService.Object);

        var httpContext = new DefaultHttpContext();
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "1") };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_ReturnsOkResult_WithInvoices()
    {
        // Arrange
        var invoices = new List<InvoiceDto>
        {
            new InvoiceDto { Id = 1, InvoiceNumber = "INV-2024-001", Status = "Paid" },
            new InvoiceDto { Id = 2, InvoiceNumber = "INV-2024-002", Status = "Outstanding" }
        };

        _mockInvoiceService.Setup(s => s.GetAllAsync())
            .ReturnsAsync(invoices);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedInvoices = okResult.Value as IEnumerable<InvoiceDto>;
        returnedInvoices.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByStatus_ReturnsFilteredInvoices()
    {
        // Arrange
        var invoices = new List<InvoiceDto>
        {
            new InvoiceDto { Id = 1, Status = "Outstanding" }
        };

        _mockInvoiceService.Setup(s => s.GetByStatusAsync("Outstanding"))
            .ReturnsAsync(invoices);

        // Act
        var result = await _controller.GetByStatus("Outstanding");

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetByAccount_ReturnsAccountInvoices()
    {
        // Arrange
        var invoices = new List<InvoiceDto>
        {
            new InvoiceDto { Id = 1, AccountId = 1 }
        };

        _mockInvoiceService.Setup(s => s.GetByAccountAsync(1))
            .ReturnsAsync(invoices);

        // Act
        var result = await _controller.GetByAccount(1);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetByDateRange_ReturnsFilteredInvoices()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(-30);
        var endDate = DateTime.Today;
        var invoices = new List<InvoiceDto>
        {
            new InvoiceDto { Id = 1, InvoiceDate = DateTime.Today.AddDays(-15) }
        };

        _mockInvoiceService.Setup(s => s.GetByDateRangeAsync(startDate, endDate))
            .ReturnsAsync(invoices);

        // Act
        var result = await _controller.GetByDateRange(startDate, endDate);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetOverdue_ReturnsOverdueInvoices()
    {
        // Arrange
        var invoices = new List<InvoiceDto>
        {
            new InvoiceDto { Id = 1, DueDate = DateTime.Today.AddDays(-10), Status = "Overdue" }
        };

        _mockInvoiceService.Setup(s => s.GetOverdueAsync())
            .ReturnsAsync(invoices);

        // Act
        var result = await _controller.GetOverdue();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetDueSoon_ReturnsInvoicesDueSoon()
    {
        // Arrange
        var invoices = new List<InvoiceDto>
        {
            new InvoiceDto { Id = 1, DueDate = DateTime.Today.AddDays(3) }
        };

        _mockInvoiceService.Setup(s => s.GetDueSoonAsync(7))
            .ReturnsAsync(invoices);

        // Act
        var result = await _controller.GetDueSoon(7);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_ExistingInvoice_ReturnsOkWithInvoice()
    {
        // Arrange
        var invoice = new InvoiceDto { Id = 1, InvoiceNumber = "INV-2024-001" };

        _mockInvoiceService.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(invoice);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedInvoice = okResult.Value as InvoiceDto;
        returnedInvoice!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetById_NonExistingInvoice_ReturnsNotFound()
    {
        // Arrange
        _mockInvoiceService.Setup(s => s.GetByIdAsync(999))
            .ReturnsAsync((InvoiceDto?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetByInvoiceNumber_ExistingInvoice_ReturnsOk()
    {
        // Arrange
        var invoice = new InvoiceDto { Id = 1, InvoiceNumber = "INV-2024-001" };

        _mockInvoiceService.Setup(s => s.GetByInvoiceNumberAsync("INV-2024-001"))
            .ReturnsAsync(invoice);

        // Act
        var result = await _controller.GetByInvoiceNumber("INV-2024-001");

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_ValidInvoice_ReturnsCreatedWithInvoice()
    {
        // Arrange
        var createDto = new CreateInvoiceDto
        {
            AccountId = 1,
            InvoiceDate = DateTime.Today,
            DueDate = DateTime.Today.AddDays(30),
            LineItems = new List<CreateInvoiceLineItemDto>
            {
                new CreateInvoiceLineItemDto { Description = "Service", Amount = 100 }
            }
        };

        var createdInvoice = new InvoiceDto
        {
            Id = 1,
            InvoiceNumber = "INV-2024-001",
            Status = "Draft",
            TotalAmount = 100
        };

        _mockInvoiceService.Setup(s => s.CreateAsync(It.IsAny<CreateInvoiceDto>()))
            .ReturnsAsync(createdInvoice);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var returnedInvoice = createdResult.Value as InvoiceDto;
        returnedInvoice!.InvoiceNumber.Should().Be("INV-2024-001");
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
    public async Task CreateFromOrder_ValidOrder_ReturnsCreated()
    {
        // Arrange
        var createdInvoice = new InvoiceDto { Id = 1, OrderId = 1 };

        _mockInvoiceService.Setup(s => s.CreateFromOrderAsync(1))
            .ReturnsAsync(createdInvoice);

        // Act
        var result = await _controller.CreateFromOrder(1);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_ValidInvoice_ReturnsOkWithUpdatedInvoice()
    {
        // Arrange
        var updateDto = new UpdateInvoiceDto
        {
            Id = 1,
            Notes = "Updated notes"
        };

        var updatedInvoice = new InvoiceDto
        {
            Id = 1,
            Notes = "Updated notes"
        };

        _mockInvoiceService.Setup(s => s.UpdateAsync(It.IsAny<UpdateInvoiceDto>()))
            .ReturnsAsync(updatedInvoice);

        // Act
        var result = await _controller.Update(1, updateDto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Update_IdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var updateDto = new UpdateInvoiceDto { Id = 2 };

        // Act
        var result = await _controller.Update(1, updateDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_AlreadyPaid_ReturnsConflict()
    {
        // Arrange
        var updateDto = new UpdateInvoiceDto { Id = 1 };

        _mockInvoiceService.Setup(s => s.UpdateAsync(It.IsAny<UpdateInvoiceDto>()))
            .ThrowsAsync(new InvalidOperationException("Cannot modify paid invoice"));

        // Act
        var result = await _controller.Update(1, updateDto);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(409);
    }

    #endregion

    #region Status Management Tests

    [Fact]
    public async Task SendInvoice_ValidInvoice_ReturnsOk()
    {
        // Arrange
        _mockInvoiceService.Setup(s => s.SendAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.SendInvoice(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task SendInvoice_InvalidStatus_ReturnsConflict()
    {
        // Arrange
        _mockInvoiceService.Setup(s => s.SendAsync(1))
            .ThrowsAsync(new InvalidOperationException("Invoice already sent"));

        // Act
        var result = await _controller.SendInvoice(1);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task MarkAsPaid_ValidInvoice_ReturnsOk()
    {
        // Arrange
        _mockInvoiceService.Setup(s => s.MarkAsPaidAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.MarkAsPaid(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task VoidInvoice_ValidInvoice_ReturnsOk()
    {
        // Arrange
        _mockInvoiceService.Setup(s => s.VoidAsync(1, "Duplicate invoice"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.VoidInvoice(1, "Duplicate invoice");

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task WriteOffInvoice_ValidInvoice_ReturnsOk()
    {
        // Arrange
        _mockInvoiceService.Setup(s => s.WriteOffAsync(1, "Uncollectable"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.WriteOffInvoice(1, "Uncollectable");

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Payment Tests

    [Fact]
    public async Task RecordPayment_ValidPayment_ReturnsCreated()
    {
        // Arrange
        var paymentDto = new CreatePaymentDto
        {
            InvoiceId = 1,
            Amount = 100,
            PaymentDate = DateTime.Today,
            PaymentMethod = "Credit Card"
        };

        var createdPayment = new PaymentDto { Id = 1, InvoiceId = 1, Amount = 100 };

        _mockInvoiceService.Setup(s => s.RecordPaymentAsync(paymentDto))
            .ReturnsAsync(createdPayment);

        // Act
        var result = await _controller.RecordPayment(1, paymentDto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task RecordPayment_ExceedsBalance_ReturnsConflict()
    {
        // Arrange
        var paymentDto = new CreatePaymentDto
        {
            InvoiceId = 1,
            Amount = 10000,
            PaymentDate = DateTime.Today
        };

        _mockInvoiceService.Setup(s => s.RecordPaymentAsync(paymentDto))
            .ThrowsAsync(new InvalidOperationException("Payment exceeds balance due"));

        // Act
        var result = await _controller.RecordPayment(1, paymentDto);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task GetPayments_ValidInvoice_ReturnsPayments()
    {
        // Arrange
        var payments = new List<PaymentDto>
        {
            new PaymentDto { Id = 1, InvoiceId = 1, Amount = 50 },
            new PaymentDto { Id = 2, InvoiceId = 1, Amount = 50 }
        };

        _mockInvoiceService.Setup(s => s.GetPaymentsAsync(1))
            .ReturnsAsync(payments);

        // Act
        var result = await _controller.GetPayments(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task RefundPayment_ValidPayment_ReturnsOk()
    {
        // Arrange
        var refundDto = new RefundPaymentDto
        {
            PaymentId = 1,
            RefundAmount = 50,
            Reason = "Overcharge"
        };

        _mockInvoiceService.Setup(s => s.RefundPaymentAsync(1, 1, refundDto))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.RefundPayment(1, 1, refundDto);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Line Item Tests

    [Fact]
    public async Task GetLineItems_ValidInvoice_ReturnsLineItems()
    {
        // Arrange
        var lineItems = new List<InvoiceLineItemDto>
        {
            new InvoiceLineItemDto { Id = 1, Description = "Service A", Amount = 100 },
            new InvoiceLineItemDto { Id = 2, Description = "Service B", Amount = 200 }
        };

        _mockInvoiceService.Setup(s => s.GetLineItemsAsync(1))
            .ReturnsAsync(lineItems);

        // Act
        var result = await _controller.GetLineItems(1);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task AddLineItem_ValidItem_ReturnsCreated()
    {
        // Arrange
        var createDto = new CreateInvoiceLineItemDto
        {
            Description = "New Service",
            Amount = 150
        };

        var createdItem = new InvoiceLineItemDto { Id = 1, Description = "New Service" };

        _mockInvoiceService.Setup(s => s.AddLineItemAsync(1, createDto))
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
        var updateDto = new UpdateInvoiceLineItemDto
        {
            Id = 1,
            Description = "Updated Service",
            Amount = 200
        };

        var updatedItem = new InvoiceLineItemDto { Id = 1, Description = "Updated Service" };

        _mockInvoiceService.Setup(s => s.UpdateLineItemAsync(1, updateDto))
            .ReturnsAsync(updatedItem);

        // Act
        var result = await _controller.UpdateLineItem(1, 1, updateDto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task RemoveLineItem_ValidItem_ReturnsNoContent()
    {
        // Arrange
        _mockInvoiceService.Setup(s => s.RemoveLineItemAsync(1, 1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.RemoveLineItem(1, 1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    #endregion

    #region PDF Tests

    [Fact]
    public async Task GeneratePdf_ValidInvoice_ReturnsFile()
    {
        // Arrange
        var pdfData = new byte[] { 37, 80, 68, 70 }; // PDF header

        _mockInvoiceService.Setup(s => s.GeneratePdfAsync(1))
            .ReturnsAsync(pdfData);

        // Act
        var result = await _controller.GeneratePdf(1);

        // Assert
        var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
        fileResult.ContentType.Should().Be("application/pdf");
    }

    [Fact]
    public async Task PreviewPdf_ValidInvoice_ReturnsFile()
    {
        // Arrange
        var pdfData = new byte[] { 37, 80, 68, 70 };

        _mockInvoiceService.Setup(s => s.PreviewPdfAsync(1))
            .ReturnsAsync(pdfData);

        // Act
        var result = await _controller.PreviewPdf(1);

        // Assert
        result.Should().BeOfType<FileContentResult>();
    }

    #endregion

    #region Aging Report Tests

    [Fact]
    public async Task GetAgingReport_ReturnsAgingData()
    {
        // Arrange
        var agingReport = new AgingReportDto
        {
            Current = 10000,
            Days1To30 = 5000,
            Days31To60 = 3000,
            Days61To90 = 2000,
            Over90Days = 1000,
            TotalOutstanding = 21000
        };

        _mockInvoiceService.Setup(s => s.GetAgingReportAsync())
            .ReturnsAsync(agingReport);

        // Act
        var result = await _controller.GetAgingReport();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetAccountAgingReport_ReturnsAccountAgingData()
    {
        // Arrange
        var agingReport = new AgingReportDto
        {
            Current = 1000,
            Days1To30 = 500,
            TotalOutstanding = 1500
        };

        _mockInvoiceService.Setup(s => s.GetAgingReportByAccountAsync(1))
            .ReturnsAsync(agingReport);

        // Act
        var result = await _controller.GetAccountAgingReport(1);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region Credit Note Tests

    [Fact]
    public async Task CreateCreditNote_ValidInvoice_ReturnsCreated()
    {
        // Arrange
        var creditNoteDto = new CreateCreditNoteDto
        {
            InvoiceId = 1,
            Amount = 50,
            Reason = "Service adjustment"
        };

        var creditNote = new CreditNoteDto { Id = 1, InvoiceId = 1, Amount = 50 };

        _mockInvoiceService.Setup(s => s.CreateCreditNoteAsync(creditNoteDto))
            .ReturnsAsync(creditNote);

        // Act
        var result = await _controller.CreateCreditNote(1, creditNoteDto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task GetCreditNotes_ValidInvoice_ReturnsCreditNotes()
    {
        // Arrange
        var creditNotes = new List<CreditNoteDto>
        {
            new CreditNoteDto { Id = 1, Amount = 50 }
        };

        _mockInvoiceService.Setup(s => s.GetCreditNotesAsync(1))
            .ReturnsAsync(creditNotes);

        // Act
        var result = await _controller.GetCreditNotes(1);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region Search and Export Tests

    [Fact]
    public async Task Search_ValidQuery_ReturnsMatchingInvoices()
    {
        // Arrange
        var invoices = new List<InvoiceDto>
        {
            new InvoiceDto { Id = 1, InvoiceNumber = "INV-2024-001" }
        };

        _mockInvoiceService.Setup(s => s.SearchAsync("INV-2024"))
            .ReturnsAsync(invoices);

        // Act
        var result = await _controller.Search("INV-2024");

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Export_ValidRequest_ReturnsFile()
    {
        // Arrange
        var exportData = new byte[] { 1, 2, 3 };

        _mockInvoiceService.Setup(s => s.ExportAsync("csv"))
            .ReturnsAsync(exportData);

        // Act
        var result = await _controller.Export("csv");

        // Assert
        result.Should().BeOfType<FileContentResult>();
    }

    #endregion

    #region Bulk Operations Tests

    [Fact]
    public async Task BulkSend_ValidInvoices_ReturnsCount()
    {
        // Arrange
        var invoiceIds = new List<int> { 1, 2, 3 };

        _mockInvoiceService.Setup(s => s.BulkSendAsync(invoiceIds))
            .ReturnsAsync(3);

        // Act
        var result = await _controller.BulkSend(invoiceIds);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task SendReminder_ValidInvoice_ReturnsOk()
    {
        // Arrange
        _mockInvoiceService.Setup(s => s.SendReminderAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.SendReminder(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task BulkSendReminders_OverdueInvoices_ReturnsCount()
    {
        // Arrange
        _mockInvoiceService.Setup(s => s.BulkSendRemindersAsync())
            .ReturnsAsync(10);

        // Act
        var result = await _controller.BulkSendReminders();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetStatistics_ReturnsStats()
    {
        // Arrange
        var stats = new InvoiceStatisticsDto
        {
            TotalInvoices = 500,
            TotalRevenue = 500000,
            OutstandingBalance = 50000,
            OverdueAmount = 10000,
            AverageInvoiceValue = 1000
        };

        _mockInvoiceService.Setup(s => s.GetStatisticsAsync())
            .ReturnsAsync(stats);

        // Act
        var result = await _controller.GetStatistics();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_DraftInvoice_ReturnsNoContent()
    {
        // Arrange
        _mockInvoiceService.Setup(s => s.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_NonExistingInvoice_ReturnsNotFound()
    {
        // Arrange
        _mockInvoiceService.Setup(s => s.DeleteAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_SentInvoice_ReturnsConflict()
    {
        // Arrange
        _mockInvoiceService.Setup(s => s.DeleteAsync(1))
            .ThrowsAsync(new InvalidOperationException("Cannot delete sent invoice, void instead"));

        // Act
        var result = await _controller.Delete(1);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(409);
    }

    #endregion
}
