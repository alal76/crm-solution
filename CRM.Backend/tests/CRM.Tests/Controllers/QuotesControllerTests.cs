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
using CRM.Core.Ports.Input;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for QuotesController.
/// REM-TESTFAKE-001 / REM-ORPHAN-004: the controller now delegates its CRUD/lifecycle
/// operations to IQuoteService, so these tests mock that interface (Moq Verify) rather than
/// exercising a real DbContext directly. CrmDbContext/NormalizationService are still
/// constructed (backed by an empty in-memory database) purely to satisfy the controller's
/// constructor, since the quote line-item endpoints have no service layer yet.
/// </summary>
public class QuotesControllerTests : IDisposable
{
    private readonly Mock<IQuoteService> _mockQuoteService;
    private readonly CrmDbContext _context;
    private readonly Mock<ILogger<QuotesController>> _mockLogger;
    private readonly NormalizationService _normalizationService;
    private readonly QuotesController _controller;

    public QuotesControllerTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: $"QuotesControllerTests_{Guid.NewGuid()}")
            .Options;

        var mockConfig = new Mock<IConfiguration>();
        _context = new CrmDbContext(options, mockConfig.Object);
        _normalizationService = new NormalizationService(_context);
        _mockQuoteService = new Mock<IQuoteService>();
        _mockLogger = new Mock<ILogger<QuotesController>>();
        var mockPdfService = new Mock<IPdfGenerationService>();
        _controller = new QuotesController(_mockQuoteService.Object, _context, _mockLogger.Object, _normalizationService, mockPdfService.Object);
    }

    private static Quote CreateQuote(int id = 1, int accountId = 10, QuoteStatus status = QuoteStatus.Draft) => new()
    {
        Id = id,
        QuoteNumber = $"Q-{DateTime.UtcNow.Year}-{id:D5}",
        Name = "Test Quote",
        Status = status,
        AccountId = accountId,
        Subtotal = 1000m,
        Total = 1000m,
        QuoteDate = DateTime.UtcNow,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    #region GetQuotes Tests

    [Fact]
    public async Task GetQuotes_WithNoFilters_ReturnsOkWithAllQuotes()
    {
        // Arrange
        _mockQuoteService.Setup(s => s.GetQuotesAsync(null, null, null, null))
            .ReturnsAsync(new List<Quote> { CreateQuote(1), CreateQuote(2) });

        // Act
        var result = await _controller.GetQuotes();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var quotes = okResult.Value.Should().BeAssignableTo<IEnumerable<QuoteDto>>().Subject;
        quotes.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetQuotes_WithAccountIdFilter_PassesFilterToService()
    {
        // Arrange
        _mockQuoteService.Setup(s => s.GetQuotesAsync(10, null, null, null))
            .ReturnsAsync(new List<Quote> { CreateQuote(1, accountId: 10) });

        // Act
        var result = await _controller.GetQuotes(accountId: 10);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var quotes = okResult.Value.Should().BeAssignableTo<IEnumerable<QuoteDto>>().Subject;
        quotes.Should().HaveCount(1);
        quotes.First().AccountId.Should().Be(10);
        _mockQuoteService.Verify(s => s.GetQuotesAsync(10, null, null, null), Times.Once);
    }

    #endregion

    #region GetQuote Tests

    [Fact]
    public async Task GetQuote_WithValidId_ReturnsOkWithQuoteDto()
    {
        // Arrange
        var quote = CreateQuote(1);
        _mockQuoteService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(quote);

        // Act
        var result = await _controller.GetQuote(1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = okResult.Value.Should().BeOfType<QuoteDto>().Subject;
        dto.Id.Should().Be(1);
        dto.Title.Should().Be("Test Quote");
    }

    [Fact]
    public async Task GetQuote_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        _mockQuoteService.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((Quote?)null);

        // Act
        var result = await _controller.GetQuote(999);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region CreateQuote Tests

    [Fact]
    public async Task CreateQuote_WithValidQuote_ReturnsCreatedAtAction()
    {
        // Arrange
        var created = CreateQuote(1);
        _mockQuoteService.Setup(s => s.CreateAsync(It.IsAny<Quote>())).ReturnsAsync(created);

        // Act
        var result = await _controller.CreateQuote(new Quote { Name = "Test Quote", AccountId = 10, Subtotal = 1000m });

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(QuotesController.GetQuote));
        createdResult.StatusCode.Should().Be(201);
        var dto = createdResult.Value.Should().BeOfType<QuoteDto>().Subject;
        dto.Title.Should().Be("Test Quote");
        _mockQuoteService.Verify(s => s.CreateAsync(It.IsAny<Quote>()), Times.Once);
    }

    [Fact]
    public async Task CreateQuote_DelegatesQuoteNumberGeneration_ToService()
    {
        // Arrange: the controller no longer generates quote numbers itself -
        // IQuoteService.CreateAsync is responsible for that when QuoteNumber is empty.
        var created = CreateQuote(1);
        created.QuoteNumber = "Q2602-0001";
        _mockQuoteService.Setup(s => s.CreateAsync(It.IsAny<Quote>())).ReturnsAsync(created);

        // Act
        var result = await _controller.CreateQuote(new Quote { Name = "Test Quote", AccountId = 10, QuoteNumber = string.Empty });

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var dto = createdResult.Value.Should().BeOfType<QuoteDto>().Subject;
        dto.QuoteNumber.Should().Be("Q2602-0001");
    }

    #endregion

    #region UpdateQuote Tests

    [Fact]
    public async Task UpdateQuote_WithIdMismatch_ReturnsBadRequest()
    {
        var result = await _controller.UpdateQuote(1, new Quote { Id = 2 });

        result.Should().BeOfType<BadRequestResult>();
    }

    [Fact]
    public async Task UpdateQuote_OnAcceptedQuote_ReturnsBadRequest()
    {
        _mockQuoteService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(CreateQuote(1, status: QuoteStatus.Accepted));

        var result = await _controller.UpdateQuote(1, new Quote { Id = 1 });

        result.Should().BeOfType<BadRequestObjectResult>();
        _mockQuoteService.Verify(s => s.UpdateAsync(It.IsAny<int>(), It.IsAny<Quote>()), Times.Never);
    }

    [Fact]
    public async Task UpdateQuote_WithValidData_ReturnsNoContent()
    {
        _mockQuoteService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(CreateQuote(1, status: QuoteStatus.Draft));
        _mockQuoteService.Setup(s => s.UpdateAsync(1, It.IsAny<Quote>())).ReturnsAsync(true);

        var result = await _controller.UpdateQuote(1, new Quote { Id = 1, Name = "Updated" });

        result.Should().BeOfType<NoContentResult>();
        _mockQuoteService.Verify(s => s.UpdateAsync(1, It.Is<Quote>(q => q.Name == "Updated")), Times.Once);
    }

    #endregion

    #region DeleteQuote Tests

    [Fact]
    public async Task DeleteQuote_OnDraftQuote_ReturnsNoContent()
    {
        _mockQuoteService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(CreateQuote(1, status: QuoteStatus.Draft));
        _mockQuoteService.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);

        var result = await _controller.DeleteQuote(1);

        result.Should().BeOfType<NoContentResult>();
        _mockQuoteService.Verify(s => s.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteQuote_OnNonDraftQuote_ReturnsBadRequest()
    {
        _mockQuoteService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(CreateQuote(1, status: QuoteStatus.Shared));

        var result = await _controller.DeleteQuote(1);

        result.Should().BeOfType<BadRequestObjectResult>();
        _mockQuoteService.Verify(s => s.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    #endregion

    #region AcceptQuote Tests

    [Fact]
    public async Task AcceptQuote_WithValidId_ReturnsOkWithAcceptedStatus()
    {
        // Arrange
        _mockQuoteService.Setup(s => s.AcceptAsync(1)).ReturnsAsync(true);
        _mockQuoteService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(CreateQuote(1, status: QuoteStatus.Accepted));

        // Act
        var result = await _controller.AcceptQuote(1, null);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = okResult.Value.Should().BeOfType<QuoteDto>().Subject;
        dto.Status.Should().Be((int)QuoteStatus.Accepted);
    }

    [Fact]
    public async Task AcceptQuote_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        _mockQuoteService.Setup(s => s.AcceptAsync(999)).ReturnsAsync(false);

        // Act
        var result = await _controller.AcceptQuote(999, null);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task AcceptQuote_WithSignatureRequest_CallsUpdateAsync()
    {
        // Arrange
        _mockQuoteService.Setup(s => s.AcceptAsync(1)).ReturnsAsync(true);
        _mockQuoteService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(CreateQuote(1, status: QuoteStatus.Accepted));
        _mockQuoteService.Setup(s => s.UpdateAsync(1, It.IsAny<Quote>())).ReturnsAsync(true);

        // Act
        var result = await _controller.AcceptQuote(1, new AcceptQuoteRequest { IsSigned = true, SignedBy = "Jane Doe" });

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mockQuoteService.Verify(s => s.UpdateAsync(1, It.Is<Quote>(q => q.IsSigned && q.SignedBy == "Jane Doe")), Times.Once);
    }

    #endregion

    #region RejectQuote Tests

    [Fact]
    public async Task RejectQuote_WithValidId_ReturnsOkWithRejectedStatus()
    {
        // Arrange
        _mockQuoteService.Setup(s => s.RejectAsync(1, "Price too high")).ReturnsAsync(true);
        _mockQuoteService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(CreateQuote(1, status: QuoteStatus.Rejected));

        var request = new RejectQuoteRequest { Reason = "Price too high" };

        // Act
        var result = await _controller.RejectQuote(1, request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = okResult.Value.Should().BeOfType<QuoteDto>().Subject;
        dto.Status.Should().Be((int)QuoteStatus.Rejected);
        _mockQuoteService.Verify(s => s.RejectAsync(1, "Price too high"), Times.Once);
    }

    [Fact]
    public async Task RejectQuote_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        _mockQuoteService.Setup(s => s.RejectAsync(999, null)).ReturnsAsync(false);

        // Act
        var result = await _controller.RejectQuote(999, null);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region SendQuote / MarkViewed / CreateRevision Tests

    [Fact]
    public async Task SendQuote_WithValidId_ReturnsOk_AndCallsSendAsync()
    {
        _mockQuoteService.Setup(s => s.SendAsync(1)).ReturnsAsync(true);
        _mockQuoteService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(CreateQuote(1, status: QuoteStatus.Shared));

        var result = await _controller.SendQuote(1);

        result.Should().BeOfType<OkObjectResult>();
        _mockQuoteService.Verify(s => s.SendAsync(1), Times.Once);
    }

    [Fact]
    public async Task SendQuote_WithNonExistentId_ReturnsNotFound()
    {
        _mockQuoteService.Setup(s => s.SendAsync(999)).ReturnsAsync(false);

        var result = await _controller.SendQuote(999);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task MarkViewed_WithValidId_ReturnsOk()
    {
        _mockQuoteService.Setup(s => s.MarkViewedAsync(1)).ReturnsAsync(CreateQuote(1, status: QuoteStatus.Viewed));

        var result = await _controller.MarkViewed(1);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = okResult.Value.Should().BeOfType<QuoteDto>().Subject;
        dto.Status.Should().Be((int)QuoteStatus.Viewed);
    }

    [Fact]
    public async Task MarkViewed_WithNonExistentId_ReturnsNotFound()
    {
        _mockQuoteService.Setup(s => s.MarkViewedAsync(999)).ReturnsAsync((Quote?)null);

        var result = await _controller.MarkViewed(999);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task CreateRevision_WithValidId_ReturnsCreatedAtAction()
    {
        _mockQuoteService.Setup(s => s.CreateRevisionAsync(1)).ReturnsAsync(CreateQuote(2, status: QuoteStatus.Draft));

        var result = await _controller.CreateRevision(1);

        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(QuotesController.GetQuote));
        _mockQuoteService.Verify(s => s.CreateRevisionAsync(1), Times.Once);
    }

    [Fact]
    public async Task CreateRevision_WithNonExistentId_ReturnsNotFound()
    {
        _mockQuoteService.Setup(s => s.CreateRevisionAsync(999))
            .ThrowsAsync(new InvalidOperationException("Quote with ID 999 not found"));

        var result = await _controller.CreateRevision(999);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    public void Dispose()
    {
        _context.Dispose();
    }
}
