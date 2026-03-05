// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text.Json;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.AI.SK.Plugins;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services.SK;

/// <summary>
/// Unit tests for the QuotePlugin Semantic Kernel plugin.
/// </summary>
public class QuotePluginTests
{
    private readonly Mock<IQuoteService> _quoteServiceMock;
    private readonly Mock<ILogger<QuotePlugin>> _loggerMock;
    private readonly QuotePlugin _sut;

    public QuotePluginTests()
    {
        _quoteServiceMock = new Mock<IQuoteService>();
        _loggerMock = new Mock<ILogger<QuotePlugin>>();
        _sut = new QuotePlugin(_quoteServiceMock.Object, _loggerMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenQuoteServiceIsNull()
    {
        var act = () => new QuotePlugin(null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("quoteService");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
    {
        var act = () => new QuotePlugin(_quoteServiceMock.Object, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Plugin Metadata Tests

    [Fact]
    public void PluginName_ShouldReturn_Quote()
    {
        _sut.PluginName.Should().Be("Quote");
    }

    [Fact]
    public void Description_ShouldNotBeNullOrEmpty()
    {
        _sut.Description.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region GetQuoteAsync Tests

    [Fact]
    public async Task GetQuoteAsync_ShouldReturnSuccessJson_AndFoundTrue_WhenQuoteExists()
    {
        var quote = new Quote
        {
            Id = 1,
            QuoteNumber = "Q-001",
            Name = "Annual Support",
            Status = QuoteStatus.Draft,
            ExpirationDate = DateTime.UtcNow.AddDays(30),
            AccountId = 1,
            Total = 5000m
        };
        _quoteServiceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(quote);

        var result = await _sut.GetQuoteAsync(1);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("data").GetProperty("quoteNumber").GetString().Should().Be("Q-001");
    }

    [Fact]
    public async Task GetQuoteAsync_ShouldReturnSuccessJson_WithFoundFalse_WhenQuoteNotFound()
    {
        _quoteServiceMock.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((Quote?)null);

        var result = await _sut.GetQuoteAsync(99);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("data").GetProperty("found").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task GetQuoteAsync_ShouldReturnErrorJson_WhenServiceThrows()
    {
        _quoteServiceMock
            .Setup(s => s.GetByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("DB timeout"));

        var result = await _sut.GetQuoteAsync(1);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    #endregion

    #region SearchQuotesAsync Tests

    [Fact]
    public async Task SearchQuotesAsync_ShouldReturnSuccessJson_WhenQuotesExist()
    {
        var quotes = new List<Quote>
        {
            new Quote { Id = 1, QuoteNumber = "Q-001", Name = "Support", Status = QuoteStatus.Shared, AccountId = 2, Total = 1000m }
        };
        _quoteServiceMock
            .Setup(s => s.GetQuotesAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<QuoteStatus?>()))
            .ReturnsAsync(quotes);

        var result = await _sut.SearchQuotesAsync(accountId: 2);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task SearchQuotesAsync_ShouldReturnErrorJson_WhenServiceThrows()
    {
        _quoteServiceMock
            .Setup(s => s.GetQuotesAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<QuoteStatus?>()))
            .ThrowsAsync(new Exception("Connection error"));

        var result = await _sut.SearchQuotesAsync();

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    #endregion

    #region GetQuoteStatisticsAsync Tests

    [Fact]
    public async Task GetQuoteStatisticsAsync_ShouldReturnSuccessJson()
    {
        var stats = new QuoteStatistics
        {
            TotalQuotes = 30,
            DraftQuotes = 5,
            SentQuotes = 15,
            AcceptedQuotes = 8,
            TotalValue = 250000m
        };
        _quoteServiceMock
            .Setup(s => s.GetStatisticsAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(stats);

        var result = await _sut.GetQuoteStatisticsAsync(90);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task GetQuoteStatisticsAsync_ShouldReturnErrorJson_WhenServiceThrows()
    {
        _quoteServiceMock
            .Setup(s => s.GetStatisticsAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ThrowsAsync(new Exception("Analytics failure"));

        var result = await _sut.GetQuoteStatisticsAsync();

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    #endregion

    #region CreateQuoteAsync Tests

    [Fact]
    public async Task CreateQuoteAsync_ShouldReturnSuccessJson_WhenQuoteCreated()
    {
        var createdQuote = new Quote
        {
            Id = 10,
            QuoteNumber = "Q-010",
            Name = "New Deal",
            Status = QuoteStatus.Draft,
            AccountId = 3,
            Total = 0m
        };
        _quoteServiceMock
            .Setup(s => s.CreateAsync(It.IsAny<Quote>()))
            .ReturnsAsync(createdQuote);

        var result = await _sut.CreateQuoteAsync("New Deal", accountId: 3, opportunityId: null, expirationDays: 30);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("data").GetProperty("quoteId").GetInt32().Should().Be(10);
    }

    [Fact]
    public async Task CreateQuoteAsync_ShouldReturnErrorJson_WhenServiceThrows()
    {
        _quoteServiceMock
            .Setup(s => s.CreateAsync(It.IsAny<Quote>()))
            .ThrowsAsync(new Exception("Validation failed"));

        var result = await _sut.CreateQuoteAsync("Bad Quote", accountId: 1);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    #endregion

    #region SendQuoteAsync Tests

    [Fact]
    public async Task SendQuoteAsync_ShouldReturnSuccessJson_WhenSendSucceeds()
    {
        var sentQuote = new Quote
        {
            Id = 1,
            QuoteNumber = "Q-001",
            Name = "Support",
            Status = QuoteStatus.Shared,
            AccountId = 1,
            SentDate = DateTime.UtcNow
        };
        _quoteServiceMock.Setup(s => s.SendAsync(1)).ReturnsAsync(true);
        _quoteServiceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(sentQuote);

        var result = await _sut.SendQuoteAsync(1);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("data").GetProperty("success").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task SendQuoteAsync_ShouldReturnErrorJson_WhenSendFails()
    {
        _quoteServiceMock
            .Setup(s => s.SendAsync(It.IsAny<int>()))
            .ThrowsAsync(new Exception("Email delivery failed"));

        var result = await _sut.SendQuoteAsync(1);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    #endregion
}
