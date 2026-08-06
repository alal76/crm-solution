// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>Unit tests for QuoteService (TCOV Wave-A).</summary>
public class QuoteServiceTests : IDisposable
{
    private readonly CrmDbContext _context;
    private readonly Mock<ILogger<QuoteService>> _logger;
    private readonly QuoteService _service;

    public QuoteServiceTests()
    {
        var opts = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new CrmDbContext(opts, null!);
        _logger = new Mock<ILogger<QuoteService>>();
        _service = new QuoteService(_context, _logger.Object);
    }

    public void Dispose() => _context.Dispose();

    // ── GetQuotesAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetQuotesAsync_ShouldReturnAllNonDeleted_WhenNoFilters()
    {
        _context.Quotes.Add(new Quote { Id = 1, Name = "Q1", QuoteNumber = "QT-001", IsDeleted = false });
        _context.Quotes.Add(new Quote { Id = 2, Name = "Q2", QuoteNumber = "QT-002", IsDeleted = true });
        await _context.SaveChangesAsync();

        var result = await _service.GetQuotesAsync();

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Q1");
    }

    [Fact]
    public async Task GetQuotesAsync_ShouldFilterByAccountId()
    {
        _context.Quotes.Add(new Quote { Id = 1, Name = "Acct Quote", QuoteNumber = "QT-001", AccountId = 10, IsDeleted = false });
        _context.Quotes.Add(new Quote { Id = 2, Name = "Other Quote", QuoteNumber = "QT-002", AccountId = 20, IsDeleted = false });
        await _context.SaveChangesAsync();

        var result = await _service.GetQuotesAsync(accountId: 10);

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Acct Quote");
    }

    [Fact]
    public async Task GetQuotesAsync_ShouldFilterByStatus()
    {
        _context.Quotes.Add(new Quote { Id = 1, Name = "New", QuoteNumber = "QT-001", Status = QuoteStatus.New, IsDeleted = false });
        _context.Quotes.Add(new Quote { Id = 2, Name = "Sent", QuoteNumber = "QT-002", Status = QuoteStatus.Shared, IsDeleted = false });
        await _context.SaveChangesAsync();

        var result = await _service.GetQuotesAsync(status: QuoteStatus.Shared);

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Sent");
    }

    // ── GetByIdAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ShouldReturnQuote_WhenQuoteExists()
    {
        _context.Quotes.Add(new Quote { Id = 5, Name = "Find Me", QuoteNumber = "QT-005", IsDeleted = false });
        await _context.SaveChangesAsync();

        var result = await _service.GetByIdAsync(5);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Find Me");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenQuoteNotFound()
    {
        var result = await _service.GetByIdAsync(999);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenQuoteIsDeleted()
    {
        _context.Quotes.Add(new Quote { Id = 3, Name = "Deleted", QuoteNumber = "QT-003", IsDeleted = true });
        await _context.SaveChangesAsync();

        var result = await _service.GetByIdAsync(3);

        result.Should().BeNull();
    }

    // ── GetByQuoteNumberAsync ─────────────────────────────────────────────

    [Fact]
    public async Task GetByQuoteNumberAsync_ShouldReturnQuote_WhenNumberExists()
    {
        _context.Quotes.Add(new Quote { Id = 6, Name = "Named Quote", QuoteNumber = "QT-UNIQ", IsDeleted = false });
        await _context.SaveChangesAsync();

        var result = await _service.GetByQuoteNumberAsync("QT-UNIQ");

        result.Should().NotBeNull();
        result!.QuoteNumber.Should().Be("QT-UNIQ");
    }

    // ── CreateAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ShouldCreateQuote_WithDefaultStatusAndAutoNumber()
    {
        var quote = new Quote { Name = "New Quote" };

        var created = await _service.CreateAsync(quote);

        created.Should().NotBeNull();
        created.Name.Should().Be("New Quote");
        created.Status.Should().Be(QuoteStatus.New);
        created.QuoteNumber.Should().NotBeNullOrEmpty();
        created.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowArgumentNullException_WhenQuoteIsNull()
    {
        Func<Task> act = () => _service.CreateAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CreateAsync_ShouldSetExpirationDate_WhenValidityDaysProvided()
    {
        var quote = new Quote { Name = "Expiring Quote", ValidityDays = 30 };

        var created = await _service.CreateAsync(quote);

        created.ExpirationDate.Should().NotBeNull();
        created.ExpirationDate!.Value.Should().BeCloseTo(DateTime.UtcNow.AddDays(30), TimeSpan.FromSeconds(5));
    }

    // ── UpdateAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ShouldReturnTrue_AndUpdateFields_WhenQuoteExists()
    {
        _context.Quotes.Add(new Quote { Id = 10, Name = "Old Name", QuoteNumber = "QT-010", IsDeleted = false });
        await _context.SaveChangesAsync();

        var updates = new Quote { Name = "New Name", Status = QuoteStatus.Shared };
        var result = await _service.UpdateAsync(10, updates);

        result.Should().BeTrue();
        var inDb = await _context.Quotes.FindAsync(10);
        inDb!.Name.Should().Be("New Name");
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenQuoteNotFound()
    {
        var result = await _service.UpdateAsync(999, new Quote { Name = "Ghost" });
        result.Should().BeFalse();
    }

    // ── DeleteAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ShouldReturnTrue_AndSoftDelete_WhenQuoteExists()
    {
        _context.Quotes.Add(new Quote { Id = 20, Name = "To Delete", QuoteNumber = "QT-020", IsDeleted = false });
        await _context.SaveChangesAsync();

        var result = await _service.DeleteAsync(20);

        result.Should().BeTrue();
        var inDb = await _context.Quotes.FindAsync(20);
        inDb!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenQuoteNotFound()
    {
        var result = await _service.DeleteAsync(999);
        result.Should().BeFalse();
    }
}
