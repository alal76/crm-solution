// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRM.Api.Controllers;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Input;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers
{
    /// <summary>
    /// Unit tests for <see cref="QuotesController"/>.
    /// REM-TESTFAKE-001 / REM-ORPHAN-004: the controller delegates its CRUD/lifecycle
    /// operations to <see cref="IQuoteService"/>, so these tests mock that interface and
    /// verify the controller calls the right service methods with the right arguments and
    /// maps the results to the correct HTTP responses.
    /// </summary>
    public class QuotesControllerTests : IDisposable
    {
        private readonly Mock<IQuoteService> _mockQuoteService;
        private readonly Mock<ILogger<QuotesController>> _mockLogger;
        private readonly Mock<IPdfGenerationService> _mockPdfService;
        private readonly CrmDbContext _context;
        private readonly NormalizationService _normalizationService;
        private readonly QuotesController _controller;

        public QuotesControllerTests()
        {
            _mockQuoteService = new Mock<IQuoteService>();
            _mockLogger = new Mock<ILogger<QuotesController>>();
            _mockPdfService = new Mock<IPdfGenerationService>();

            // NormalizationService and CrmDbContext are still constructor dependencies
            // because the quote line-item endpoints have no service layer yet; an empty
            // in-memory database is sufficient since the tests below only exercise the
            // quote CRUD/lifecycle endpoints that go through IQuoteService.
            var options = new DbContextOptionsBuilder<CrmDbContext>()
                .UseInMemoryDatabase(databaseName: $"QuotesControllerTests_{Guid.NewGuid()}")
                .Options;
            var mockConfig = new Mock<IConfiguration>();
            _context = new CrmDbContext(options, mockConfig.Object);
            _normalizationService = new NormalizationService(_context);

            _controller = new QuotesController(
                _mockQuoteService.Object,
                _context,
                _mockLogger.Object,
                _normalizationService,
                _mockPdfService.Object);
        }

        private static Quote CreateQuote(int id = 1, int accountId = 10, QuoteStatus status = QuoteStatus.Draft) => new()
        {
            Id = id,
            QuoteNumber = $"Q-2026-{id:D5}",
            Name = "Test Quote",
            Status = status,
            AccountId = accountId,
            Subtotal = 1000m,
            Total = 1000m,
            QuoteDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        #region GetQuotes

        [Fact]
        public async Task GetQuotes_ReturnsOk_WithMappedDtos()
        {
            var quotes = new List<Quote> { CreateQuote(1), CreateQuote(2) };
            _mockQuoteService
                .Setup(s => s.GetQuotesAsync(null, null, null, null))
                .ReturnsAsync(quotes);

            var result = await _controller.GetQuotes();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var dtos = Assert.IsAssignableFrom<IEnumerable<QuoteDto>>(okResult.Value);
            Assert.Equal(2, dtos.Count());
            _mockQuoteService.Verify(s => s.GetQuotesAsync(null, null, null, null), Times.Once);
        }

        [Fact]
        public async Task GetQuotes_PassesFiltersThrough_ToService()
        {
            _mockQuoteService
                .Setup(s => s.GetQuotesAsync(10, 5, QuoteStatus.Shared, true))
                .ReturnsAsync(new List<Quote> { CreateQuote(1, accountId: 10, status: QuoteStatus.Shared) });

            var result = await _controller.GetQuotes(accountId: 10, opportunityId: 5, status: QuoteStatus.Shared, expired: true);

            Assert.IsType<OkObjectResult>(result.Result);
            _mockQuoteService.Verify(s => s.GetQuotesAsync(10, 5, QuoteStatus.Shared, true), Times.Once);
        }

        #endregion

        #region GetQuote

        [Fact]
        public async Task GetQuote_WithValidId_ReturnsOkWithDto()
        {
            var quote = CreateQuote(1);
            _mockQuoteService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(quote);

            var result = await _controller.GetQuote(1);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<QuoteDto>(okResult.Value);
            Assert.Equal(1, dto.Id);
            Assert.Equal("Test Quote", dto.Title);
            _mockQuoteService.Verify(s => s.GetByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetQuote_WithNonExistentId_ReturnsNotFound()
        {
            _mockQuoteService.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((Quote?)null);

            var result = await _controller.GetQuote(999);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        #endregion

        #region GetQuoteByNumber

        [Fact]
        public async Task GetQuoteByNumber_WithValidNumber_ReturnsOkWithDto()
        {
            var quote = CreateQuote(1);
            _mockQuoteService.Setup(s => s.GetByQuoteNumberAsync("Q-2026-00001")).ReturnsAsync(quote);

            var result = await _controller.GetQuoteByNumber("Q-2026-00001");

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.IsType<QuoteDto>(okResult.Value);
        }

        [Fact]
        public async Task GetQuoteByNumber_WithNonExistentNumber_ReturnsNotFound()
        {
            _mockQuoteService.Setup(s => s.GetByQuoteNumberAsync("MISSING")).ReturnsAsync((Quote?)null);

            var result = await _controller.GetQuoteByNumber("MISSING");

            Assert.IsType<NotFoundResult>(result.Result);
        }

        #endregion

        #region CreateQuote

        [Fact]
        public async Task CreateQuote_ReturnsCreatedAtAction_WithMappedDto()
        {
            var input = new Quote { Name = "New Quote", AccountId = 10, Subtotal = 500m };
            var created = CreateQuote(42);
            _mockQuoteService
                .Setup(s => s.CreateAsync(It.IsAny<Quote>()))
                .ReturnsAsync(created);

            var result = await _controller.CreateQuote(input);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(QuotesController.GetQuote), createdResult.ActionName);
            Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
            var dto = Assert.IsType<QuoteDto>(createdResult.Value);
            Assert.Equal(42, dto.Id);
            _mockQuoteService.Verify(s => s.CreateAsync(It.IsAny<Quote>()), Times.Once);
        }

        [Fact]
        public async Task CreateQuote_CalculatesTotals_BeforeCallingService()
        {
            var input = new Quote { Name = "Discounted Quote", AccountId = 10, Subtotal = 1000m, DiscountPercent = 10m, TaxRate = 5m };
            Quote? passedToService = null;
            _mockQuoteService
                .Setup(s => s.CreateAsync(It.IsAny<Quote>()))
                .Callback<Quote>(q => passedToService = q)
                .ReturnsAsync((Quote q) => q);

            await _controller.CreateQuote(input);

            Assert.NotNull(passedToService);
            // Subtotal 1000, 10% discount -> 100 discount, 900 after discount, 5% tax -> 45 tax, total 945
            Assert.Equal(100m, passedToService!.Discount);
            Assert.Equal(45m, passedToService.Tax);
            Assert.Equal(945m, passedToService.Total);
        }

        #endregion

        #region UpdateQuote

        [Fact]
        public async Task UpdateQuote_WithMismatchedId_ReturnsBadRequest()
        {
            var result = await _controller.UpdateQuote(1, new Quote { Id = 2 });

            Assert.IsType<BadRequestResult>(result);
            _mockQuoteService.Verify(s => s.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task UpdateQuote_WithNonExistentId_ReturnsNotFound()
        {
            _mockQuoteService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((Quote?)null);

            var result = await _controller.UpdateQuote(1, new Quote { Id = 1 });

            Assert.IsType<NotFoundResult>(result);
        }

        [Theory]
        [InlineData(QuoteStatus.Accepted)]
        [InlineData(QuoteStatus.Rejected)]
        public async Task UpdateQuote_OnClosedQuote_ReturnsBadRequest_AndDoesNotCallUpdate(QuoteStatus status)
        {
            _mockQuoteService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(CreateQuote(1, status: status));

            var result = await _controller.UpdateQuote(1, new Quote { Id = 1 });

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("accepted or rejected", badRequest.Value!.ToString());
            _mockQuoteService.Verify(s => s.UpdateAsync(It.IsAny<int>(), It.IsAny<Quote>()), Times.Never);
        }

        [Fact]
        public async Task UpdateQuote_WithValidData_ReturnsNoContent_AndCallsUpdate()
        {
            _mockQuoteService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(CreateQuote(1, status: QuoteStatus.Draft));
            _mockQuoteService.Setup(s => s.UpdateAsync(1, It.IsAny<Quote>())).ReturnsAsync(true);

            var result = await _controller.UpdateQuote(1, new Quote { Id = 1, Name = "Updated" });

            Assert.IsType<NoContentResult>(result);
            _mockQuoteService.Verify(s => s.UpdateAsync(1, It.Is<Quote>(q => q.Name == "Updated")), Times.Once);
        }

        [Fact]
        public async Task UpdateQuote_WhenServiceReturnsFalse_ReturnsNotFound()
        {
            _mockQuoteService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(CreateQuote(1, status: QuoteStatus.Draft));
            _mockQuoteService.Setup(s => s.UpdateAsync(1, It.IsAny<Quote>())).ReturnsAsync(false);

            var result = await _controller.UpdateQuote(1, new Quote { Id = 1 });

            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        #region DeleteQuote

        [Fact]
        public async Task DeleteQuote_WithNonExistentId_ReturnsNotFound()
        {
            _mockQuoteService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((Quote?)null);

            var result = await _controller.DeleteQuote(1);

            Assert.IsType<NotFoundResult>(result);
            _mockQuoteService.Verify(s => s.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task DeleteQuote_OnNonDraftQuote_ReturnsBadRequest_AndDoesNotCallDelete()
        {
            _mockQuoteService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(CreateQuote(1, status: QuoteStatus.Shared));

            var result = await _controller.DeleteQuote(1);

            Assert.IsType<BadRequestObjectResult>(result);
            _mockQuoteService.Verify(s => s.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task DeleteQuote_OnDraftQuote_ReturnsNoContent_AndCallsDelete()
        {
            _mockQuoteService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(CreateQuote(1, status: QuoteStatus.Draft));
            _mockQuoteService.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);

            var result = await _controller.DeleteQuote(1);

            Assert.IsType<NoContentResult>(result);
            _mockQuoteService.Verify(s => s.DeleteAsync(1), Times.Once);
        }

        #endregion

        #region SendQuote

        [Fact]
        public async Task SendQuote_WithNonExistentId_ReturnsNotFound()
        {
            _mockQuoteService.Setup(s => s.SendAsync(999)).ReturnsAsync(false);

            var result = await _controller.SendQuote(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task SendQuote_WithValidId_ReturnsOk_AndCallsSendAsync()
        {
            _mockQuoteService.Setup(s => s.SendAsync(1)).ReturnsAsync(true);
            _mockQuoteService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(CreateQuote(1, status: QuoteStatus.Shared));

            var result = await _controller.SendQuote(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<QuoteDto>(okResult.Value);
            _mockQuoteService.Verify(s => s.SendAsync(1), Times.Once);
        }

        #endregion

        #region MarkViewed

        [Fact]
        public async Task MarkViewed_WithNonExistentId_ReturnsNotFound()
        {
            _mockQuoteService.Setup(s => s.MarkViewedAsync(999)).ReturnsAsync((Quote?)null);

            var result = await _controller.MarkViewed(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task MarkViewed_WithValidId_ReturnsOk()
        {
            _mockQuoteService.Setup(s => s.MarkViewedAsync(1)).ReturnsAsync(CreateQuote(1, status: QuoteStatus.Viewed));

            var result = await _controller.MarkViewed(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<QuoteDto>(okResult.Value);
            Assert.Equal((int)QuoteStatus.Viewed, dto.Status);
        }

        #endregion

        #region AcceptQuote

        [Fact]
        public async Task AcceptQuote_WithNonExistentId_ReturnsNotFound()
        {
            _mockQuoteService.Setup(s => s.AcceptAsync(999)).ReturnsAsync(false);

            var result = await _controller.AcceptQuote(999, null);

            Assert.IsType<NotFoundResult>(result);
            _mockQuoteService.Verify(s => s.UpdateAsync(It.IsAny<int>(), It.IsAny<Quote>()), Times.Never);
        }

        [Fact]
        public async Task AcceptQuote_WithoutSignatureRequest_ReturnsOk_AndDoesNotCallUpdate()
        {
            _mockQuoteService.Setup(s => s.AcceptAsync(1)).ReturnsAsync(true);
            _mockQuoteService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(CreateQuote(1, status: QuoteStatus.Accepted));

            var result = await _controller.AcceptQuote(1, null);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<QuoteDto>(okResult.Value);
            Assert.Equal((int)QuoteStatus.Accepted, dto.Status);
            _mockQuoteService.Verify(s => s.AcceptAsync(1), Times.Once);
            _mockQuoteService.Verify(s => s.UpdateAsync(It.IsAny<int>(), It.IsAny<Quote>()), Times.Never);
        }

        [Fact]
        public async Task AcceptQuote_WithSignatureRequest_CallsUpdateAsync_WithSignatureFields()
        {
            _mockQuoteService.Setup(s => s.AcceptAsync(1)).ReturnsAsync(true);
            _mockQuoteService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(CreateQuote(1, status: QuoteStatus.Accepted));
            _mockQuoteService.Setup(s => s.UpdateAsync(1, It.IsAny<Quote>())).ReturnsAsync(true);

            var request = new AcceptQuoteRequest { IsSigned = true, SignedBy = "Jane Doe" };
            var result = await _controller.AcceptQuote(1, request);

            Assert.IsType<OkObjectResult>(result);
            _mockQuoteService.Verify(s => s.UpdateAsync(1, It.Is<Quote>(q =>
                q.IsSigned == true && q.SignedBy == "Jane Doe" && q.SignedDate != null)), Times.Once);
        }

        #endregion

        #region RejectQuote

        [Fact]
        public async Task RejectQuote_WithNonExistentId_ReturnsNotFound()
        {
            _mockQuoteService.Setup(s => s.RejectAsync(999, null)).ReturnsAsync(false);

            var result = await _controller.RejectQuote(999, null);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task RejectQuote_WithReason_PassesReasonToService()
        {
            _mockQuoteService.Setup(s => s.RejectAsync(1, "Too expensive")).ReturnsAsync(true);
            _mockQuoteService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(CreateQuote(1, status: QuoteStatus.Rejected));

            var result = await _controller.RejectQuote(1, new RejectQuoteRequest { Reason = "Too expensive" });

            var okResult = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<QuoteDto>(okResult.Value);
            Assert.Equal((int)QuoteStatus.Rejected, dto.Status);
            _mockQuoteService.Verify(s => s.RejectAsync(1, "Too expensive"), Times.Once);
        }

        #endregion

        #region CreateRevision

        [Fact]
        public async Task CreateRevision_WithNonExistentId_ReturnsNotFound()
        {
            _mockQuoteService
                .Setup(s => s.CreateRevisionAsync(999))
                .ThrowsAsync(new InvalidOperationException("Quote with ID 999 not found"));

            var result = await _controller.CreateRevision(999);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateRevision_WithValidId_ReturnsCreatedAtAction()
        {
            var revision = CreateQuote(2, status: QuoteStatus.Draft);
            _mockQuoteService.Setup(s => s.CreateRevisionAsync(1)).ReturnsAsync(revision);

            var result = await _controller.CreateRevision(1);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(QuotesController.GetQuote), createdResult.ActionName);
            var dto = Assert.IsType<QuoteDto>(createdResult.Value);
            Assert.Equal(2, dto.Id);
            _mockQuoteService.Verify(s => s.CreateRevisionAsync(1), Times.Once);
        }

        #endregion

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
