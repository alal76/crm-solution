// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Api.Controllers;
using CRM.Core.DTOs;
using CRM.Core.Entities;
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
/// Uses an in-memory database since QuotesController directly accesses CrmDbContext.
/// Tests quote lifecycle endpoints including CRUD, acceptance, and rejection.
/// </summary>
public class QuotesControllerTests : IDisposable
{
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
        _mockLogger = new Mock<ILogger<QuotesController>>();
        _controller = new QuotesController(_context, _mockLogger.Object, _normalizationService);
    }

    private static Quote CreateQuote(int accountId = 10, QuoteStatus status = QuoteStatus.Draft) => new()
    {
        QuoteNumber = $"Q-{DateTime.UtcNow.Year}-{new Random().Next(1, 99999):D5}",
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
        _context.Quotes.AddRange(CreateQuote(), CreateQuote());
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetQuotes();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var quotes = okResult.Value.Should().BeAssignableTo<IEnumerable<QuoteDto>>().Subject;
        quotes.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetQuotes_WithAccountIdFilter_ReturnsOnlyMatchingQuotes()
    {
        // Arrange
        _context.Quotes.AddRange(CreateQuote(accountId: 10), CreateQuote(accountId: 20));
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetQuotes(accountId: 10);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var quotes = okResult.Value.Should().BeAssignableTo<IEnumerable<QuoteDto>>().Subject;
        quotes.Should().HaveCount(1);
        quotes.First().AccountId.Should().Be(10);
    }

    #endregion

    #region GetQuote Tests

    [Fact]
    public async Task GetQuote_WithValidId_ReturnsOkWithQuoteDto()
    {
        // Arrange
        var quote = CreateQuote();
        _context.Quotes.Add(quote);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetQuote(quote.Id);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = okResult.Value.Should().BeOfType<QuoteDto>().Subject;
        dto.Id.Should().Be(quote.Id);
        dto.Title.Should().Be("Test Quote");
    }

    [Fact]
    public async Task GetQuote_WithNonExistentId_ReturnsNotFound()
    {
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
        var quote = CreateQuote();

        // Act
        var result = await _controller.CreateQuote(quote);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(QuotesController.GetQuote));
        createdResult.StatusCode.Should().Be(201);
        var dto = createdResult.Value.Should().BeOfType<QuoteDto>().Subject;
        dto.Title.Should().Be("Test Quote");
    }

    [Fact]
    public async Task CreateQuote_WithoutQuoteNumber_GeneratesQuoteNumber()
    {
        // Arrange
        var quote = CreateQuote();
        quote.QuoteNumber = string.Empty; // Simulate missing quote number

        // Act
        var result = await _controller.CreateQuote(quote);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var dto = createdResult.Value.Should().BeOfType<QuoteDto>().Subject;
        dto.QuoteNumber.Should().NotBeNullOrEmpty();
        dto.QuoteNumber.Should().StartWith($"Q-{DateTime.UtcNow.Year}-");
    }

    #endregion

    #region AcceptQuote Tests

    [Fact]
    public async Task AcceptQuote_WithValidId_ReturnsOkWithAcceptedStatus()
    {
        // Arrange
        var quote = CreateQuote(status: QuoteStatus.Shared);
        _context.Quotes.Add(quote);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.AcceptQuote(quote.Id, null);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = okResult.Value.Should().BeOfType<QuoteDto>().Subject;
        dto.Status.Should().Be((int)QuoteStatus.Accepted);
    }

    [Fact]
    public async Task AcceptQuote_WithNonExistentId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.AcceptQuote(999, null);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region RejectQuote Tests

    [Fact]
    public async Task RejectQuote_WithValidId_ReturnsOkWithRejectedStatus()
    {
        // Arrange
        var quote = CreateQuote(status: QuoteStatus.Shared);
        _context.Quotes.Add(quote);
        await _context.SaveChangesAsync();

        var request = new RejectQuoteRequest { Reason = "Price too high" };

        // Act
        var result = await _controller.RejectQuote(quote.Id, request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = okResult.Value.Should().BeOfType<QuoteDto>().Subject;
        dto.Status.Should().Be((int)QuoteStatus.Rejected);
    }

    [Fact]
    public async Task RejectQuote_WithNonExistentId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.RejectQuote(999, null);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    public void Dispose()
    {
        _context.Dispose();
    }
}
