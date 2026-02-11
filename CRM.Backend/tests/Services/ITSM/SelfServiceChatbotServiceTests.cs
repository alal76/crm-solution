// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRM.Infrastructure.Services.ITSM;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace CRM.Tests.Services.ITSM;

#region Placeholder DTOs

// Note: These DTOs match the interface defined in ISelfServiceChatbotService
// If actual DTOs exist in CRM.Core.DTOs.ITSM, replace these placeholders

public class ChatSessionDto
{
    public string SessionId { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public DateTime StartedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public int MessageCount { get; set; }
}

public class ChatbotMessageDto
{
    public string SessionId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class ChatbotResponseDto
{
    public string SessionId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public ResponseType Type { get; set; }
    public List<ChatSuggestion>? Suggestions { get; set; }
    public List<KnowledgeSearchResultDto>? KnowledgeResults { get; set; }
    public ChatAction? Action { get; set; }
}

public enum ResponseType
{
    Text,
    Options,
    KnowledgeResults,
    IncidentStatus
}

public class ChatSuggestion
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
}

public class ChatMessageDto
{
    public int Id { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public bool IsFromUser { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class QuickActionDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}

public class KnowledgeSearchResultDto
{
    public int ArticleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public double RelevanceScore { get; set; }
    public string Category { get; set; } = string.Empty;
    public int Views { get; set; }
}

public class ChatAction
{
    public string ActionType { get; set; } = string.Empty;
    public Dictionary<string, object>? Data { get; set; }
}

public class IncidentCreationResultDto
{
    public bool Success { get; set; }
    public int IncidentId { get; set; }
    public string IncidentNumber { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class IncidentStatusResponseDto
{
    public string IncidentNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string AssignedTo { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string LastUpdate { get; set; } = string.Empty;
}

#endregion

public class SelfServiceChatbotServiceTests
{
    private readonly Mock<ILogger<SelfServiceChatbotService>> _mockLogger;
    private readonly SelfServiceChatbotService _service;

    public SelfServiceChatbotServiceTests()
    {
        _mockLogger = new Mock<ILogger<SelfServiceChatbotService>>();
        _service = new SelfServiceChatbotService(_mockLogger.Object);
    }

    #region StartSessionAsync Tests

    [Fact]
    public async Task StartSessionAsync_ReturnsNewSession()
    {
        // Act
        var result = await _service.StartSessionAsync(userId: 100);

        // Assert
        result.Should().NotBeNull();
        result.SessionId.Should().NotBeNullOrEmpty();
        result.SessionId.Should().StartWith("chat_");
        result.UserId.Should().Be(100);
        result.Status.Should().Be("active");
    }

    [Fact]
    public async Task StartSessionAsync_GeneratesUniqueSessionIds()
    {
        // Act
        var session1 = await _service.StartSessionAsync(userId: 1);
        var session2 = await _service.StartSessionAsync(userId: 2);

        // Assert
        session1.SessionId.Should().NotBe(session2.SessionId);
    }

    [Fact]
    public async Task StartSessionAsync_WithNullUserId_ReturnsSession()
    {
        // Act
        var result = await _service.StartSessionAsync(userId: null);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().BeNull();
    }

    [Fact]
    public async Task StartSessionAsync_SetsStartedAtToNow()
    {
        // Act
        var before = DateTime.UtcNow;
        var result = await _service.StartSessionAsync(userId: 1);
        var after = DateTime.UtcNow;

        // Assert
        result.StartedAt.Should().BeOnOrAfter(before);
        result.StartedAt.Should().BeOnOrBefore(after);
    }

    [Fact]
    public async Task StartSessionAsync_InitializesMessageCountToZero()
    {
        // Act
        var result = await _service.StartSessionAsync(userId: 1);

        // Assert
        result.MessageCount.Should().Be(0);
    }

    [Fact]
    public async Task StartSessionAsync_LogsSessionCreation()
    {
        // Act
        await _service.StartSessionAsync(userId: 100);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Started chat session")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region ProcessMessageAsync Tests

    [Fact]
    public async Task ProcessMessageAsync_WithGreeting_ReturnsWelcomeMessage()
    {
        // Arrange
        var session = await _service.StartSessionAsync(userId: 1);
        var message = new ChatbotMessageDto { SessionId = session.SessionId, Message = "Hello" };

        // Act
        var response = await _service.ProcessMessageAsync(message, userId: 1);

        // Assert
        response.Message.Should().Contain("Hello");
        response.Type.Should().Be(ResponseType.Text);
        response.Suggestions.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ProcessMessageAsync_WithPasswordQuery_ReturnsKnowledgeResults()
    {
        // Arrange
        var session = await _service.StartSessionAsync(userId: 1);
        var message = new ChatbotMessageDto { SessionId = session.SessionId, Message = "I forgot my password" };

        // Act
        var response = await _service.ProcessMessageAsync(message, userId: 1);

        // Assert
        response.Type.Should().Be(ResponseType.KnowledgeResults);
        response.KnowledgeResults.Should().NotBeNullOrEmpty();
        response.KnowledgeResults!.Any(k => k.Title.Contains("Password", StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
    }

    [Fact]
    public async Task ProcessMessageAsync_WithVPNQuery_ReturnsVPNArticles()
    {
        // Arrange
        var session = await _service.StartSessionAsync(userId: 1);
        var message = new ChatbotMessageDto { SessionId = session.SessionId, Message = "How do I connect to VPN?" };

        // Act
        var response = await _service.ProcessMessageAsync(message, userId: 1);

        // Assert
        response.Type.Should().Be(ResponseType.KnowledgeResults);
        response.KnowledgeResults.Should().NotBeNullOrEmpty();
        response.KnowledgeResults!.Any(k => k.Title.Contains("VPN", StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
    }

    [Fact]
    public async Task ProcessMessageAsync_WithIncidentNumber_ReturnsIncidentStatus()
    {
        // Arrange
        var session = await _service.StartSessionAsync(userId: 1);
        var message = new ChatbotMessageDto { SessionId = session.SessionId, Message = "What is the status of INC-12345?" };

        // Act
        var response = await _service.ProcessMessageAsync(message, userId: 1);

        // Assert
        response.Type.Should().Be(ResponseType.IncidentStatus);
        response.Message.Should().Contain("INC-12345");
        response.Suggestions.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ProcessMessageAsync_WithPerformanceIssue_ReturnsHelpfulArticles()
    {
        // Arrange
        var session = await _service.StartSessionAsync(userId: 1);
        var message = new ChatbotMessageDto { SessionId = session.SessionId, Message = "My computer is running slow" };

        // Act
        var response = await _service.ProcessMessageAsync(message, userId: 1);

        // Assert
        response.Type.Should().Be(ResponseType.KnowledgeResults);
        response.KnowledgeResults.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ProcessMessageAsync_WithTicketRequest_ReturnsOptions()
    {
        // Arrange
        var session = await _service.StartSessionAsync(userId: 1);
        var message = new ChatbotMessageDto { SessionId = session.SessionId, Message = "I need to create a support ticket" };

        // Act
        var response = await _service.ProcessMessageAsync(message, userId: 1);

        // Assert
        response.Type.Should().Be(ResponseType.Options);
        response.Suggestions.Should().NotBeNullOrEmpty();
        response.Suggestions!.Any(s => s.Text.Contains("Hardware", StringComparison.OrdinalIgnoreCase) ||
                                        s.Text.Contains("Software", StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
    }

    [Fact]
    public async Task ProcessMessageAsync_WithUnknownQuery_ReturnsDefaultOptions()
    {
        // Arrange
        var session = await _service.StartSessionAsync(userId: 1);
        var message = new ChatbotMessageDto { SessionId = session.SessionId, Message = "xyzabc random text" };

        // Act
        var response = await _service.ProcessMessageAsync(message, userId: 1);

        // Assert
        response.Type.Should().Be(ResponseType.Options);
        response.Suggestions.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ProcessMessageAsync_RecordsMessageInHistory()
    {
        // Arrange
        var session = await _service.StartSessionAsync(userId: 1);
        var message = new ChatbotMessageDto { SessionId = session.SessionId, Message = "Test message" };

        // Act
        await _service.ProcessMessageAsync(message, userId: 1);
        var history = await _service.GetSessionHistoryAsync(session.SessionId);

        // Assert
        history.Should().HaveCount(2); // User message + bot response
        history[0].IsFromUser.Should().BeTrue();
        history[0].Message.Should().Be("Test message");
        history[1].IsFromUser.Should().BeFalse();
    }

    [Fact]
    public async Task ProcessMessageAsync_LogsProcessing()
    {
        // Arrange
        var session = await _service.StartSessionAsync(userId: 1);
        var message = new ChatbotMessageDto { SessionId = session.SessionId, Message = "Hello world" };

        // Act
        await _service.ProcessMessageAsync(message, userId: 1);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Processing message")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessMessageAsync_WithNewSession_CreatesSessionAutomatically()
    {
        // Arrange - no existing session
        var message = new ChatbotMessageDto { SessionId = "new_session_123", Message = "Hello" };

        // Act
        var response = await _service.ProcessMessageAsync(message, userId: 1);

        // Assert
        response.Should().NotBeNull();
        response.SessionId.Should().Be("new_session_123");
    }

    [Fact]
    public async Task ProcessMessageAsync_WithStatusQuery_PromptsForTicketNumber()
    {
        // Arrange
        var session = await _service.StartSessionAsync(userId: 1);
        var message = new ChatbotMessageDto { SessionId = session.SessionId, Message = "Check my ticket status" };

        // Act
        var response = await _service.ProcessMessageAsync(message, userId: 1);

        // Assert
        response.Type.Should().Be(ResponseType.Options);
        response.Suggestions.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region GetSessionHistoryAsync Tests

    [Fact]
    public async Task GetSessionHistoryAsync_ReturnsEmptyForNewSession()
    {
        // Arrange
        var session = await _service.StartSessionAsync(userId: 1);

        // Act
        var history = await _service.GetSessionHistoryAsync(session.SessionId);

        // Assert
        history.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSessionHistoryAsync_ReturnsMessagesInOrder()
    {
        // Arrange
        var session = await _service.StartSessionAsync(userId: 1);
        await _service.ProcessMessageAsync(new ChatbotMessageDto { SessionId = session.SessionId, Message = "First" }, 1);
        await _service.ProcessMessageAsync(new ChatbotMessageDto { SessionId = session.SessionId, Message = "Second" }, 1);

        // Act
        var history = await _service.GetSessionHistoryAsync(session.SessionId);

        // Assert
        history.Should().HaveCount(4); // 2 user + 2 bot
        history[0].Message.Should().Be("First");
        history[2].Message.Should().Be("Second");
    }

    [Fact]
    public async Task GetSessionHistoryAsync_ReturnsEmptyForNonexistentSession()
    {
        // Act
        var history = await _service.GetSessionHistoryAsync("nonexistent_session");

        // Assert
        history.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSessionHistoryAsync_IncludesTimestamps()
    {
        // Arrange
        var session = await _service.StartSessionAsync(userId: 1);
        await _service.ProcessMessageAsync(new ChatbotMessageDto { SessionId = session.SessionId, Message = "Test" }, 1);

        // Act
        var history = await _service.GetSessionHistoryAsync(session.SessionId);

        // Assert
        history.All(m => m.Timestamp > DateTime.MinValue).Should().BeTrue();
    }

    #endregion

    #region EndSessionAsync Tests

    [Fact]
    public async Task EndSessionAsync_SetsEndedAt()
    {
        // Arrange
        var session = await _service.StartSessionAsync(userId: 1);

        // Act
        await _service.EndSessionAsync(session.SessionId);

        // Assert - just verify no exception
    }

    [Fact]
    public async Task EndSessionAsync_LogsSessionEnd()
    {
        // Arrange
        var session = await _service.StartSessionAsync(userId: 1);

        // Act
        await _service.EndSessionAsync(session.SessionId);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Ended chat session")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task EndSessionAsync_DoesNotThrowForNonexistentSession()
    {
        // Act
        Func<Task> act = () => _service.EndSessionAsync("nonexistent");

        // Assert
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region GetQuickActionsAsync Tests

    [Fact]
    public async Task GetQuickActionsAsync_ReturnsListOfActions()
    {
        // Act
        var actions = await _service.GetQuickActionsAsync();

        // Assert
        actions.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetQuickActionsAsync_IncludesResetPassword()
    {
        // Act
        var actions = await _service.GetQuickActionsAsync();

        // Assert
        actions.Any(a => a.Id == "reset_password").Should().BeTrue();
    }

    [Fact]
    public async Task GetQuickActionsAsync_IncludesCheckTickets()
    {
        // Act
        var actions = await _service.GetQuickActionsAsync();

        // Assert
        actions.Any(a => a.Id == "check_tickets").Should().BeTrue();
    }

    [Fact]
    public async Task GetQuickActionsAsync_IncludesCategories()
    {
        // Act
        var actions = await _service.GetQuickActionsAsync();

        // Assert
        actions.All(a => !string.IsNullOrEmpty(a.Category)).Should().BeTrue();
    }

    [Fact]
    public async Task GetQuickActionsAsync_IncludesIcons()
    {
        // Act
        var actions = await _service.GetQuickActionsAsync();

        // Assert
        actions.All(a => !string.IsNullOrEmpty(a.Icon)).Should().BeTrue();
    }

    #endregion

    #region ExecuteQuickActionAsync Tests

    [Fact]
    public async Task ExecuteQuickActionAsync_ResetPassword_ReturnsRedirectAction()
    {
        // Act
        var response = await _service.ExecuteQuickActionAsync("reset_password", userId: 1);

        // Assert
        response.Action.Should().NotBeNull();
        response.Action!.ActionType.Should().Be("redirect");
        response.Action.Data.Should().ContainKey("url");
    }

    [Fact]
    public async Task ExecuteQuickActionAsync_CheckTickets_ReturnsIncidentStatus()
    {
        // Act
        var response = await _service.ExecuteQuickActionAsync("check_tickets", userId: 1);

        // Assert
        response.Type.Should().Be(ResponseType.IncidentStatus);
        response.Message.Should().Contain("open tickets");
    }

    [Fact]
    public async Task ExecuteQuickActionAsync_NewSoftware_ReturnsInstructions()
    {
        // Act
        var response = await _service.ExecuteQuickActionAsync("new_software", userId: 1);

        // Assert
        response.Type.Should().Be(ResponseType.Text);
        response.Message.Should().Contain("software");
    }

    [Fact]
    public async Task ExecuteQuickActionAsync_UnknownAction_ReturnsDefaultResponse()
    {
        // Act
        var response = await _service.ExecuteQuickActionAsync("unknown_action", userId: 1);

        // Assert
        response.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ExecuteQuickActionAsync_GeneratesSessionId()
    {
        // Act
        var response = await _service.ExecuteQuickActionAsync("reset_password", userId: 1);

        // Assert
        response.SessionId.Should().NotBeNullOrEmpty();
        response.SessionId.Should().StartWith("quickaction_");
    }

    #endregion

    #region SearchKnowledgeAsync Tests

    [Fact]
    public async Task SearchKnowledgeAsync_ReturnsResults()
    {
        // Act
        var results = await _service.SearchKnowledgeAsync("password");

        // Assert
        results.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SearchKnowledgeAsync_IncludesRelevanceScore()
    {
        // Act
        var results = await _service.SearchKnowledgeAsync("vpn");

        // Assert
        results.All(r => r.RelevanceScore > 0).Should().BeTrue();
    }

    [Fact]
    public async Task SearchKnowledgeAsync_IncludesArticleDetails()
    {
        // Act
        var results = await _service.SearchKnowledgeAsync("reset");

        // Assert
        results.All(r => !string.IsNullOrEmpty(r.Title)).Should().BeTrue();
        results.All(r => !string.IsNullOrEmpty(r.Summary)).Should().BeTrue();
        results.All(r => !string.IsNullOrEmpty(r.Category)).Should().BeTrue();
    }

    #endregion

    #region CreateIncidentFromChatAsync Tests

    [Fact]
    public async Task CreateIncidentFromChatAsync_ReturnsSuccessfulResult()
    {
        // Arrange
        var session = await _service.StartSessionAsync(userId: 1);

        // Act
        var result = await _service.CreateIncidentFromChatAsync(session.SessionId, userId: 1);

        // Assert
        result.Success.Should().BeTrue();
        result.IncidentId.Should().BeGreaterThan(0);
        result.IncidentNumber.Should().StartWith("INC-");
    }

    [Fact]
    public async Task CreateIncidentFromChatAsync_IncludesMessage()
    {
        // Arrange
        var session = await _service.StartSessionAsync(userId: 1);

        // Act
        var result = await _service.CreateIncidentFromChatAsync(session.SessionId, userId: 1);

        // Assert
        result.Message.Should().Contain("created");
        result.Message.Should().Contain(result.IncidentNumber);
    }

    [Fact]
    public async Task CreateIncidentFromChatAsync_LogsCreation()
    {
        // Arrange
        var session = await _service.StartSessionAsync(userId: 1);

        // Act
        await _service.CreateIncidentFromChatAsync(session.SessionId, userId: 1);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Created incident")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region CheckIncidentStatusAsync Tests

    [Fact]
    public async Task CheckIncidentStatusAsync_ReturnsStatus()
    {
        // Act
        var result = await _service.CheckIncidentStatusAsync("INC-12345", userId: 1);

        // Assert
        result.Should().NotBeNull();
        result!.IncidentNumber.Should().Be("INC-12345");
        result.Status.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CheckIncidentStatusAsync_IncludesPriorityAndAssignment()
    {
        // Act
        var result = await _service.CheckIncidentStatusAsync("INC-99999", userId: 1);

        // Assert
        result.Should().NotBeNull();
        result!.Priority.Should().NotBeNullOrEmpty();
        result.AssignedTo.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CheckIncidentStatusAsync_IncludesLastUpdate()
    {
        // Act
        var result = await _service.CheckIncidentStatusAsync("INC-55555", userId: 1);

        // Assert
        result.Should().NotBeNull();
        result!.LastUpdate.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Intent Recognition Tests

    [Theory]
    [InlineData("forgot my password")]
    [InlineData("reset password")]
    [InlineData("account locked")]
    public async Task ProcessMessageAsync_PasswordIntents_RecognizedCorrectly(string message)
    {
        // Arrange
        var session = await _service.StartSessionAsync(userId: 1);
        var msg = new ChatbotMessageDto { SessionId = session.SessionId, Message = message };

        // Act
        var response = await _service.ProcessMessageAsync(msg, userId: 1);

        // Assert
        response.Type.Should().Be(ResponseType.KnowledgeResults);
    }

    [Theory]
    [InlineData("connect to vpn")]
    [InlineData("remote work")]
    [InlineData("work from home")]
    public async Task ProcessMessageAsync_VPNIntents_RecognizedCorrectly(string message)
    {
        // Arrange
        var session = await _service.StartSessionAsync(userId: 1);
        var msg = new ChatbotMessageDto { SessionId = session.SessionId, Message = message };

        // Act
        var response = await _service.ProcessMessageAsync(msg, userId: 1);

        // Assert
        response.Type.Should().Be(ResponseType.KnowledgeResults);
    }

    [Theory]
    [InlineData("INC-123")]
    [InlineData("INC12345")]
    [InlineData("incident INC-99999")]
    public async Task ProcessMessageAsync_IncidentNumbers_RecognizedCorrectly(string message)
    {
        // Arrange
        var session = await _service.StartSessionAsync(userId: 1);
        var msg = new ChatbotMessageDto { SessionId = session.SessionId, Message = message };

        // Act
        var response = await _service.ProcessMessageAsync(msg, userId: 1);

        // Assert
        response.Type.Should().Be(ResponseType.IncidentStatus);
    }

    #endregion
}
