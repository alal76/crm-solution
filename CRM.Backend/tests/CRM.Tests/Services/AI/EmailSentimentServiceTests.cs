// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Threading.Tasks;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services.AI;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services.AI;

/// <summary>
/// Unit tests for EmailSentimentService (TODO-AI-07).
/// Covers: positive text → Positive; complaint text → Negative; empty body → Neutral.
/// </summary>
public class EmailSentimentServiceTests
{
    private readonly EmailSentimentService _sut;

    public EmailSentimentServiceTests()
    {
        var mockLogger = new Mock<ILogger<EmailSentimentService>>();
        _sut = new EmailSentimentService(mockLogger.Object);
    }

    [Fact]
    public async Task AnalyzeSentimentAsync_ShouldReturnPositive_WhenBodyContainsPraiseWords()
    {
        // Arrange
        var body = "Thank you so much! Your team did an excellent job. I am very happy with the service and love working with you.";

        // Act
        var result = await _sut.AnalyzeSentimentAsync(body);

        // Assert
        result.Should().NotBeNull();
        result.Sentiment.Should().Be(SentimentCategory.Positive);
        result.Score.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task AnalyzeSentimentAsync_ShouldReturnNegative_WhenBodyContainsComplaintWords()
    {
        // Arrange
        var body = "This is terrible! I am very angry and frustrated with your awful service. The product is broken and disappointing.";

        // Act
        var result = await _sut.AnalyzeSentimentAsync(body);

        // Assert
        result.Sentiment.Should().Be(SentimentCategory.Negative);
        result.Score.Should().BeLessThan(0);
    }

    [Fact]
    public async Task AnalyzeSentimentAsync_ShouldReturnNeutral_WhenBodyIsEmptyOrGeneric()
    {
        // Arrange
        var body = string.Empty;

        // Act
        var result = await _sut.AnalyzeSentimentAsync(body);

        // Assert
        result.Sentiment.Should().Be(SentimentCategory.Neutral);
        result.Score.Should().Be(0);
    }
}
