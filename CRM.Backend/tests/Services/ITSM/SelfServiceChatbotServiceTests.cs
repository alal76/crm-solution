// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// KB-013: Unit tests for SelfServiceChatbotService wired to IUnifiedKnowledgeSearchService.
using CRM.Core.Interfaces.ITSM;
using CRM.Core.Ports.Input;
using CRM.Infrastructure.Services.ITSM;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services.ITSM;

/// <summary>
/// Unit tests for <see cref="SelfServiceChatbotService"/>.
/// KB-013: Verifies live KB search wiring, result mapping, and graceful fallback.
/// </summary>
public class SelfServiceChatbotServiceTests
{
    private readonly Mock<ILogger<SelfServiceChatbotService>> _mockLogger;
    private readonly Mock<IUnifiedKnowledgeSearchService> _mockKnowledgeSearch;
    private readonly SelfServiceChatbotService _service;

    public SelfServiceChatbotServiceTests()
    {
        _mockLogger = new Mock<ILogger<SelfServiceChatbotService>>();
        _mockKnowledgeSearch = new Mock<IUnifiedKnowledgeSearchService>();
        _service = new SelfServiceChatbotService(_mockLogger.Object, _mockKnowledgeSearch.Object);
    }

    // ========================================================================
    // SearchKnowledgeAsync
    // ========================================================================

    [Fact]
    public async Task SearchKnowledgeAsync_ShouldReturnMappedResults_WhenSearchReturnsArticles()
    {
        // Arrange
        var unifiedResults = new List<UnifiedKnowledgeSearchResultDto>
        {
            new() { Id = 1, Title = "Password Reset Guide", Summary = "How to reset your password.", RelevanceScore = 0.95, Category = "Access", ViewCount = 120 },
            new() { Id = 2, Title = "VPN Setup",           Summary = "Connecting via VPN.",          RelevanceScore = 0.80, Category = "Network", ViewCount = 55 }
        };
        _mockKnowledgeSearch
            .Setup(s => s.SearchAsync("password", 5, KnowledgeSource.All, It.IsAny<CancellationToken>()))
            .ReturnsAsync(unifiedResults);

        // Act
        var results = await _service.SearchKnowledgeAsync("password");

        // Assert — KB-013: fields mapped from UnifiedKnowledgeSearchResultDto → KnowledgeSearchResultDto
        results.Should().HaveCount(2);

        results[0].ArticleId.Should().Be(1);
        results[0].Title.Should().Be("Password Reset Guide");
        results[0].Summary.Should().Be("How to reset your password.");
        results[0].RelevanceScore.Should().BeApproximately(0.95, 0.001);
        results[0].Category.Should().Be("Access");
        results[0].Views.Should().Be(120);

        results[1].ArticleId.Should().Be(2);
        results[1].Title.Should().Be("VPN Setup");
    }

    [Fact]
    public async Task SearchKnowledgeAsync_ShouldReturnEmptyList_WhenSearchReturnsNoArticles()
    {
        // Arrange
        _mockKnowledgeSearch
            .Setup(s => s.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<KnowledgeSource>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<UnifiedKnowledgeSearchResultDto>());

        // Act
        var results = await _service.SearchKnowledgeAsync("unknown topic that returns nothing");

        // Assert — KB-013: graceful fallback to empty list, no exception thrown
        results.Should().NotBeNull();
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchKnowledgeAsync_ShouldHandleNullCategoryGracefully_WhenCategoryIsNull()
    {
        // Arrange
        var unifiedResults = new List<UnifiedKnowledgeSearchResultDto>
        {
            new() { Id = 3, Title = "Uncategorised Article", Summary = "No category set.", RelevanceScore = 0.6, Category = null, ViewCount = 10 }
        };
        _mockKnowledgeSearch
            .Setup(s => s.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<KnowledgeSource>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(unifiedResults);

        // Act
        var results = await _service.SearchKnowledgeAsync("some query");

        // Assert — null Category maps to empty string, not null
        results.Should().HaveCount(1);
        results[0].Category.Should().Be(string.Empty);
        results[0].Category.Should().NotBeNull();
    }

    // ========================================================================
    // ProcessMessageAsync — KB search intent branches
    // ========================================================================

    [Fact]
    public async Task ProcessMessageAsync_ShouldReturnKnowledgeResults_WhenMessageContainsPasswordKeyword()
    {
        // Arrange
        var sessionResult = await _service.StartSessionAsync(userId: 1);
        var unifiedResults = new List<UnifiedKnowledgeSearchResultDto>
        {
            new() { Id = 10, Title = "How to Reset Password", Summary = "Self-service reset steps.", RelevanceScore = 0.99, Category = "Access", ViewCount = 300 }
        };
        _mockKnowledgeSearch
            .Setup(s => s.SearchAsync(It.IsAny<string>(), 5, KnowledgeSource.All, It.IsAny<CancellationToken>()))
            .ReturnsAsync(unifiedResults);

        var msg = new ChatbotMessageDto { SessionId = sessionResult.SessionId, Message = "I forgot my password" };

        // Act
        var response = await _service.ProcessMessageAsync(msg, userId: 1);

        // Assert — KB-013: response carries live search results
        response.Should().NotBeNull();
        response.Type.Should().Be(ResponseType.KnowledgeResults);
        response.KnowledgeResults.Should().NotBeNull().And.HaveCount(1);
        response.KnowledgeResults![0].Title.Should().Be("How to Reset Password");
        response.KnowledgeResults![0].ArticleId.Should().Be(10);
    }

    [Fact]
    public async Task ProcessMessageAsync_ShouldReturnEmptyKnowledgeResults_WhenSearchReturnsEmpty()
    {
        // Arrange
        var sessionResult = await _service.StartSessionAsync(userId: 2);
        _mockKnowledgeSearch
            .Setup(s => s.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<KnowledgeSource>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<UnifiedKnowledgeSearchResultDto>());

        var msg = new ChatbotMessageDto { SessionId = sessionResult.SessionId, Message = "password reset help" };

        // Act
        var response = await _service.ProcessMessageAsync(msg, userId: 2);

        // Assert — KB-013: falls back to empty list without throwing
        response.Should().NotBeNull();
        response.KnowledgeResults.Should().NotBeNull();
        response.KnowledgeResults!.Should().BeEmpty();
    }

    [Fact]
    public async Task ProcessMessageAsync_ShouldReturnTextResponse_WhenGreetingReceived()
    {
        // Arrange — greeting does NOT trigger KB search
        var sessionResult = await _service.StartSessionAsync(userId: 3);
        var msg = new ChatbotMessageDto { SessionId = sessionResult.SessionId, Message = "hello" };

        // Act
        var response = await _service.ProcessMessageAsync(msg, userId: 3);

        // Assert — greeting path does not call the knowledge search
        response.Should().NotBeNull();
        response.Type.Should().Be(ResponseType.Text);
        _mockKnowledgeSearch.Verify(
            s => s.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<KnowledgeSource>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ========================================================================
    // CancellationToken propagation — KB-013
    // ========================================================================

    [Fact]
    public async Task SearchKnowledgeAsync_ShouldCallSearchAsync_WithAnyCancellationToken()
    {
        // Arrange — KB-013: verify CT is forwarded through to IUnifiedKnowledgeSearchService
        _mockKnowledgeSearch
            .Setup(s => s.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<KnowledgeSource>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<UnifiedKnowledgeSearchResultDto>());

        // Act
        await _service.SearchKnowledgeAsync("vpn issue");

        // Assert — SearchAsync must have been called (CT accepted via It.IsAny)
        _mockKnowledgeSearch.Verify(
            s => s.SearchAsync("vpn issue", 5, KnowledgeSource.All, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
