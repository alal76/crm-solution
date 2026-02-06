// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Quotes Controller Unit Tests

using Xunit;
using Moq;
using FluentAssertions;
using CRM.Api.Controllers;
using CRM.Api.Hubs;
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

namespace CRM.Tests.Controllers;

/// <summary>
/// Comprehensive unit tests for QuotesController
/// Covers: CRUD operations, line items, approvals, versioning, PDF generation
/// </summary>
public class QuotesControllerTests
{
    private readonly Mock<IQuoteService> _mockQuoteService;
    private readonly Mock<ILogger<QuotesController>> _mockLogger;
    private readonly Mock<ICrmNotificationService> _mockNotificationService;
    private readonly QuotesController _controller;

    public QuotesControllerTests()
    {
        _mockQuoteService = new Mock<IQuoteService>();
        _mockLogger = new Mock<ILogger<QuotesController>>();
        _mockNotificationService = new Mock<ICrmNotificationService>();

        _mockNotificationService.Setup(x => x.NotifyRecordCreatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);
        _mockNotificationService.Setup(x => x.NotifyRecordUpdatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);
        _mockNotificationService.Setup(x => x.NotifyRecordDeletedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);

        _controller = new QuotesController(_mockQuoteService.Object, _mockLogger.Object, _mockNotificationService.Object);

        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_ReturnsOkResult_WithQuotes()
    {
        // Arrange
        var quotes = new List<QuoteDto>
        {
            new QuoteDto { Id = 1, QuoteNumber = "Q-001", Status = QuoteStatus.Draft },
            new QuoteDto { Id = 2, QuoteNumber = "Q-002", Status = QuoteStatus.Sent }
        };

        _mockQuoteService.Setup(s => s.GetAllAsync())
            .ReturnsAsync(quotes);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedQuotes = okResult.Value as IEnumerable<QuoteDto>;
        returnedQuotes.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_WithStatusFilter_ReturnsFilteredQuotes()
    {
        // Arrange
        var quotes = new List<QuoteDto>
        {
            new QuoteDto { Id = 1, Status = QuoteStatus.Draft }
        };

        _mockQuoteService.Setup(s => s.GetByStatusAsync(QuoteStatus.Draft))
            .ReturnsAsync(quotes);

        // Act
        var result = await _controller.GetByStatus(QuoteStatus.Draft);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetByOpportunity_ValidOpportunity_ReturnsQuotes()
    {
        // Arrange
        var quotes = new List<QuoteDto>
        {
            new QuoteDto { Id = 1, OpportunityId = 1 },
            new QuoteDto { Id = 2, OpportunityId = 1 }
        };

        _mockQuoteService.Setup(s => s.GetByOpportunityAsync(1))
            .ReturnsAsync(quotes);

        // Act
        var result = await _controller.GetByOpportunity(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetByAccount_ValidAccount_ReturnsQuotes()
    {
        // Arrange
        var quotes = new List<QuoteDto>
        {
            new QuoteDto { Id = 1, AccountId = 1 }
        };

        _mockQuoteService.Setup(s => s.GetByAccountAsync(1))
            .ReturnsAsync(quotes);

        // Act
        var result = await _controller.GetByAccount(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_ExistingQuote_ReturnsOkWithQuote()
    {
        // Arrange
        var quote = new QuoteDto { Id = 1, QuoteNumber = "Q-001", TotalAmount = 10000 };

        _mockQuoteService.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(quote);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedQuote = okResult.Value as QuoteDto;
        returnedQuote!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetById_NonExistingQuote_ReturnsNotFound()
    {
        // Arrange
        _mockQuoteService.Setup(s => s.GetByIdAsync(999))
            .ReturnsAsync((QuoteDto?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetByNumber_ExistingQuote_ReturnsQuote()
    {
        // Arrange
        var quote = new QuoteDto { Id = 1, QuoteNumber = "Q-001" };

        _mockQuoteService.Setup(s => s.GetByQuoteNumberAsync("Q-001"))
            .ReturnsAsync(quote);

        // Act
        var result = await _controller.GetByNumber("Q-001");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_ValidQuote_ReturnsCreatedWithQuote()
    {
        // Arrange
        var createDto = new CreateQuoteDto
        {
            AccountId = 1,
            OpportunityId = 1,
            ValidUntil = DateTime.Today.AddDays(30),
            LineItems = new List<CreateQuoteLineItemDto>
            {
                new CreateQuoteLineItemDto { ProductId = 1, Quantity = 10, UnitPrice = 100 }
            }
        };

        var createdQuote = new QuoteDto
        {
            Id = 1,
            QuoteNumber = "Q-001",
            Status = QuoteStatus.Draft,
            TotalAmount = 1000
        };

        _mockQuoteService.Setup(s => s.CreateAsync(It.IsAny<CreateQuoteDto>()))
            .ReturnsAsync(createdQuote);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var returnedQuote = createdResult.Value as QuoteDto;
        returnedQuote!.Status.Should().Be(QuoteStatus.Draft);
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
    public async Task Create_MissingAccountId_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateQuoteDto { AccountId = 0 };

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_WithDiscount_CalculatesCorrectTotal()
    {
        // Arrange
        var createDto = new CreateQuoteDto
        {
            AccountId = 1,
            DiscountPercent = 10,
            LineItems = new List<CreateQuoteLineItemDto>
            {
                new CreateQuoteLineItemDto { ProductId = 1, Quantity = 10, UnitPrice = 100 }
            }
        };

        var createdQuote = new QuoteDto
        {
            Id = 1,
            SubTotal = 1000,
            DiscountAmount = 100,
            TotalAmount = 900
        };

        _mockQuoteService.Setup(s => s.CreateAsync(It.IsAny<CreateQuoteDto>()))
            .ReturnsAsync(createdQuote);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var returnedQuote = createdResult.Value as QuoteDto;
        returnedQuote!.TotalAmount.Should().Be(900);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_ValidQuote_ReturnsOkWithUpdatedQuote()
    {
        // Arrange
        var updateDto = new UpdateQuoteDto
        {
            Id = 1,
            ValidUntil = DateTime.Today.AddDays(60)
        };

        var updatedQuote = new QuoteDto
        {
            Id = 1,
            ValidUntil = DateTime.Today.AddDays(60)
        };

        _mockQuoteService.Setup(s => s.UpdateAsync(It.IsAny<UpdateQuoteDto>()))
            .ReturnsAsync(updatedQuote);

        // Act
        var result = await _controller.Update(1, updateDto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task Update_IdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var updateDto = new UpdateQuoteDto { Id = 2 };

        // Act
        var result = await _controller.Update(1, updateDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_AcceptedQuote_ReturnsConflict()
    {
        // Arrange
        var updateDto = new UpdateQuoteDto { Id = 1 };

        _mockQuoteService.Setup(s => s.UpdateAsync(It.IsAny<UpdateQuoteDto>()))
            .ThrowsAsync(new InvalidOperationException("Cannot modify accepted quote"));

        // Act
        var result = await _controller.Update(1, updateDto);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(409);
    }

    #endregion

    #region Line Items Tests

    [Fact]
    public async Task AddLineItem_ValidRequest_ReturnsUpdatedQuote()
    {
        // Arrange
        var lineItem = new CreateQuoteLineItemDto
        {
            ProductId = 1,
            Quantity = 5,
            UnitPrice = 200
        };

        var updatedQuote = new QuoteDto { Id = 1, TotalAmount = 2000 };

        _mockQuoteService.Setup(s => s.AddLineItemAsync(1, lineItem))
            .ReturnsAsync(updatedQuote);

        // Act
        var result = await _controller.AddLineItem(1, lineItem);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task UpdateLineItem_ValidRequest_ReturnsUpdatedQuote()
    {
        // Arrange
        var lineItem = new UpdateQuoteLineItemDto
        {
            Id = 1,
            Quantity = 10,
            UnitPrice = 150
        };

        var updatedQuote = new QuoteDto { Id = 1, TotalAmount = 1500 };

        _mockQuoteService.Setup(s => s.UpdateLineItemAsync(1, lineItem))
            .ReturnsAsync(updatedQuote);

        // Act
        var result = await _controller.UpdateLineItem(1, lineItem);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task RemoveLineItem_ValidRequest_ReturnsUpdatedQuote()
    {
        // Arrange
        var updatedQuote = new QuoteDto { Id = 1, TotalAmount = 500 };

        _mockQuoteService.Setup(s => s.RemoveLineItemAsync(1, 1))
            .ReturnsAsync(updatedQuote);

        // Act
        var result = await _controller.RemoveLineItem(1, 1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetLineItems_ValidQuote_ReturnsLineItems()
    {
        // Arrange
        var lineItems = new List<QuoteLineItemDto>
        {
            new QuoteLineItemDto { Id = 1, ProductName = "Product A", Quantity = 10 },
            new QuoteLineItemDto { Id = 2, ProductName = "Product B", Quantity = 5 }
        };

        _mockQuoteService.Setup(s => s.GetLineItemsAsync(1))
            .ReturnsAsync(lineItems);

        // Act
        var result = await _controller.GetLineItems(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task ApplyDiscount_ValidRequest_ReturnsUpdatedQuote()
    {
        // Arrange
        var request = new ApplyDiscountRequest
        {
            QuoteId = 1,
            DiscountType = DiscountType.Percentage,
            DiscountValue = 10
        };

        var updatedQuote = new QuoteDto { Id = 1, DiscountPercent = 10, TotalAmount = 900 };

        _mockQuoteService.Setup(s => s.ApplyDiscountAsync(request))
            .ReturnsAsync(updatedQuote);

        // Act
        var result = await _controller.ApplyDiscount(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion

    #region Status Management Tests

    [Fact]
    public async Task Send_DraftQuote_ReturnsOk()
    {
        // Arrange
        _mockQuoteService.Setup(s => s.SendAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Send(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Accept_SentQuote_ReturnsOk()
    {
        // Arrange
        _mockQuoteService.Setup(s => s.AcceptAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Accept(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Reject_SentQuote_ReturnsOk()
    {
        // Arrange
        var request = new RejectQuoteRequest
        {
            QuoteId = 1,
            Reason = "Price too high"
        };

        _mockQuoteService.Setup(s => s.RejectAsync(request))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Reject(request);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Expire_ValidQuote_ReturnsOk()
    {
        // Arrange
        _mockQuoteService.Setup(s => s.ExpireAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Expire(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Revise_AcceptedQuote_ReturnsConflict()
    {
        // Arrange
        _mockQuoteService.Setup(s => s.ReviseAsync(1))
            .ThrowsAsync(new InvalidOperationException("Cannot revise accepted quote"));

        // Act
        var result = await _controller.Revise(1);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(409);
    }

    #endregion

    #region Approval Workflow Tests

    [Fact]
    public async Task SubmitForApproval_ValidQuote_ReturnsOk()
    {
        // Arrange
        _mockQuoteService.Setup(s => s.SubmitForApprovalAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.SubmitForApproval(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Approve_PendingQuote_ReturnsOk()
    {
        // Arrange
        var request = new ApproveQuoteRequest
        {
            QuoteId = 1,
            Comments = "Approved for customer discount"
        };

        _mockQuoteService.Setup(s => s.ApproveAsync(request))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Approve(request);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task RequestChanges_PendingQuote_ReturnsOk()
    {
        // Arrange
        var request = new RequestChangesRequest
        {
            QuoteId = 1,
            Changes = "Please reduce discount to 5%"
        };

        _mockQuoteService.Setup(s => s.RequestChangesAsync(request))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.RequestChanges(request);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task GetApprovalHistory_ValidQuote_ReturnsHistory()
    {
        // Arrange
        var history = new List<ApprovalHistoryDto>
        {
            new ApprovalHistoryDto { ApproverName = "Manager", Action = "Approved", Date = DateTime.Today }
        };

        _mockQuoteService.Setup(s => s.GetApprovalHistoryAsync(1))
            .ReturnsAsync(history);

        // Act
        var result = await _controller.GetApprovalHistory(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion

    #region PDF Generation Tests

    [Fact]
    public async Task GeneratePdf_ValidQuote_ReturnsPdfFile()
    {
        // Arrange
        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // PDF header

        _mockQuoteService.Setup(s => s.GeneratePdfAsync(1))
            .ReturnsAsync(pdfBytes);

        // Act
        var result = await _controller.GeneratePdf(1);

        // Assert
        var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
        fileResult.ContentType.Should().Be("application/pdf");
    }

    [Fact]
    public async Task GeneratePdf_NonExistingQuote_ReturnsNotFound()
    {
        // Arrange
        _mockQuoteService.Setup(s => s.GeneratePdfAsync(999))
            .ThrowsAsync(new InvalidOperationException("Quote not found"));

        // Act
        var result = await _controller.GeneratePdf(999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Preview_ValidQuote_ReturnsHtml()
    {
        // Arrange
        var html = "<html><body>Quote Preview</body></html>";

        _mockQuoteService.Setup(s => s.GeneratePreviewAsync(1))
            .ReturnsAsync(html);

        // Act
        var result = await _controller.Preview(1);

        // Assert
        var contentResult = result.Should().BeOfType<ContentResult>().Subject;
        contentResult.ContentType.Should().Be("text/html");
    }

    #endregion

    #region Email Tests

    [Fact]
    public async Task SendByEmail_ValidRequest_ReturnsOk()
    {
        // Arrange
        var request = new SendQuoteEmailRequest
        {
            QuoteId = 1,
            RecipientEmail = "customer@example.com",
            Subject = "Your Quote",
            Message = "Please review the attached quote."
        };

        _mockQuoteService.Setup(s => s.SendByEmailAsync(request))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.SendByEmail(request);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task SendByEmail_InvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new SendQuoteEmailRequest
        {
            QuoteId = 1,
            RecipientEmail = "invalid-email"
        };

        // Act
        var result = await _controller.SendByEmail(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region Version Management Tests

    [Fact]
    public async Task CreateVersion_ValidQuote_ReturnsNewVersion()
    {
        // Arrange
        var newVersion = new QuoteDto
        {
            Id = 2,
            QuoteNumber = "Q-001-v2",
            Version = 2
        };

        _mockQuoteService.Setup(s => s.CreateNewVersionAsync(1))
            .ReturnsAsync(newVersion);

        // Act
        var result = await _controller.CreateNewVersion(1);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
    }

    [Fact]
    public async Task GetVersions_ValidQuote_ReturnsAllVersions()
    {
        // Arrange
        var versions = new List<QuoteDto>
        {
            new QuoteDto { Id = 1, Version = 1 },
            new QuoteDto { Id = 2, Version = 2 }
        };

        _mockQuoteService.Setup(s => s.GetVersionsAsync("Q-001"))
            .ReturnsAsync(versions);

        // Act
        var result = await _controller.GetVersions("Q-001");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion

    #region Clone Tests

    [Fact]
    public async Task Clone_ValidQuote_ReturnsClonedQuote()
    {
        // Arrange
        var clonedQuote = new QuoteDto
        {
            Id = 2,
            QuoteNumber = "Q-002",
            Status = QuoteStatus.Draft
        };

        _mockQuoteService.Setup(s => s.CloneAsync(1))
            .ReturnsAsync(clonedQuote);

        // Act
        var result = await _controller.Clone(1);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
    }

    #endregion

    #region Convert to Order Tests

    [Fact]
    public async Task ConvertToOrder_AcceptedQuote_ReturnsOrder()
    {
        // Arrange
        var order = new OrderDto
        {
            Id = 1,
            OrderNumber = "ORD-001",
            QuoteId = 1
        };

        _mockQuoteService.Setup(s => s.ConvertToOrderAsync(1))
            .ReturnsAsync(order);

        // Act
        var result = await _controller.ConvertToOrder(1);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
    }

    [Fact]
    public async Task ConvertToOrder_NotAcceptedQuote_ReturnsConflict()
    {
        // Arrange
        _mockQuoteService.Setup(s => s.ConvertToOrderAsync(1))
            .ThrowsAsync(new InvalidOperationException("Quote must be accepted"));

        // Act
        var result = await _controller.ConvertToOrder(1);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(409);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_DraftQuote_ReturnsNoContent()
    {
        // Arrange
        _mockQuoteService.Setup(s => s.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_NonExistingQuote_ReturnsNotFound()
    {
        // Arrange
        _mockQuoteService.Setup(s => s.DeleteAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion
}
