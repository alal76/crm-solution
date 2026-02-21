// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

public class ConversationServiceTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<ConversationService>> _mockLogger;
    private readonly ConversationService _service;

    public ConversationServiceTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<ConversationService>>();
        _service = new ConversationService(_mockContext.Object, _mockLogger.Object);
    }

    private void SetupConversations(List<Conversation>? conversations = null)
    {
        conversations ??= new List<Conversation>();
        var mockSet = MockDbSetFactory.CreateMockDbSet(conversations);
        _mockContext.Setup(c => c.Conversations).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    private static Conversation CreateConversation(int id, int? accountId = null, ConversationStatus status = ConversationStatus.Open, bool deleted = false)
    {
        return new Conversation
        {
            Id = id,
            ConversationId = $"conv-{id}",
            Subject = $"Subject {id}",
            AccountId = accountId,
            Status = status,
            CreatedAt = DateTime.UtcNow.AddMinutes(-id),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-id),
            IsDeleted = deleted
        };
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByAccountAndStatus()
    {
        var conversations = new List<Conversation>
        {
            CreateConversation(1, accountId: 10, status: ConversationStatus.Open),
            CreateConversation(2, accountId: 10, status: ConversationStatus.Pending),
            CreateConversation(3, accountId: 20, status: ConversationStatus.Open)
        };
        SetupConversations(conversations);

        var result = await _service.GetAllAsync(accountId: 10, status: ConversationStatus.Open);

        result.Should().HaveCount(1);
        result.First().Id.Should().Be(1);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenMissing()
    {
        SetupConversations(new List<Conversation>());

        var result = await _service.GetByIdAsync(42);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldSetDefaultsAndPersist()
    {
        var conversations = new List<Conversation>();
        SetupConversations(conversations);
        var conversation = new Conversation { Subject = "New" };

        var result = await _service.CreateAsync(conversation);

        result.IsDeleted.Should().BeFalse();
        result.CreatedAt.Should().NotBe(default);
        result.UpdatedAt.Should().NotBe(default);
        conversations.Should().Contain(conversation);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenMissing()
    {
        SetupConversations(new List<Conversation>());

        var result = await _service.UpdateAsync(1, new Conversation { Subject = "Updated" });

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldSetResolvedAt_WhenResolved()
    {
        var conversation = CreateConversation(1, accountId: 10, status: ConversationStatus.Open);
        var conversations = new List<Conversation> { conversation };
        SetupConversations(conversations);

        var result = await _service.UpdateStatusAsync(1, ConversationStatus.Resolved);

        result.Should().BeTrue();
        conversation.Status.Should().Be(ConversationStatus.Resolved);
        conversation.ResolvedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task AssignAsync_ShouldSetAssignedUser()
    {
        var conversation = CreateConversation(1, accountId: 10, status: ConversationStatus.Open);
        var conversations = new List<Conversation> { conversation };
        SetupConversations(conversations);

        var result = await _service.AssignAsync(1, userId: 99);

        result.Should().BeTrue();
        conversation.AssignedToUserId.Should().Be(99);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteConversation()
    {
        var conversation = CreateConversation(1, accountId: 10, status: ConversationStatus.Open);
        var conversations = new List<Conversation> { conversation };
        SetupConversations(conversations);

        var result = await _service.DeleteAsync(1);

        result.Should().BeTrue();
        conversation.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task GetByEntityAsync_ShouldReturnOnlyMatchingEntityType()
    {
        var conversations = new List<Conversation>
        {
            CreateConversation(1, accountId: 5, status: ConversationStatus.Open),
            CreateConversation(2, accountId: 6, status: ConversationStatus.Open),
            new Conversation { Id = 3, ConversationId = "lead-1", LeadId = 5, Subject = "Lead", CreatedAt = DateTime.UtcNow }
        };
        SetupConversations(conversations);

        var result = await _service.GetByEntityAsync("account", 5);

        result.Should().HaveCount(1);
        result.First().AccountId.Should().Be(5);
    }
}
