// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

public class RecordCommentServiceTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<RecordCommentService>> _mockLogger;
    private readonly RecordCommentService _service;

    public RecordCommentServiceTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<RecordCommentService>>();
        _service = new RecordCommentService(_mockContext.Object, _mockLogger.Object);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void SetupComments(List<RecordComment>? comments = null, List<User>? users = null)
    {
        comments ??= new List<RecordComment>();
        users ??= new List<User>();

        var mockComments = MockDbSetFactory.CreateMockDbSet(comments);
        var mockUsers = MockDbSetFactory.CreateMockDbSet(users);

        _mockContext.Setup(c => c.RecordComments).Returns(mockComments.Object);
        _mockContext.Setup(c => c.Users).Returns(mockUsers.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    private static RecordComment CreateComment(
        int id,
        string entityType = "Account",
        int entityId = 1,
        int authorId = 10,
        int? parentCommentId = null,
        bool isDeleted = false,
        string content = "Test comment content")
    {
        return new RecordComment
        {
            Id = id,
            EntityType = entityType,
            EntityId = entityId,
            Content = content,
            AuthorId = authorId,
            ParentCommentId = parentCommentId,
            IsDeleted = isDeleted,
            CreatedAt = DateTime.UtcNow.AddMinutes(-id),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-id)
        };
    }

    private static User CreateUser(int id, UserRole role = UserRole.Sales, string firstName = "Test", string lastName = "User")
    {
        return new User
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            Username = $"user{id}",
            Email = $"user{id}@test.com",
            PasswordHash = "hashed",
            Role = (int)role,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    // ── GetByEntityAsync Tests ────────────────────────────────────────────────

    [Fact]
    public async Task GetByEntityAsync_ShouldReturnOnlyTopLevelComments()
    {
        var topLevel = CreateComment(1, entityType: "Account", entityId: 5, authorId: 10);
        var reply = CreateComment(2, entityType: "Account", entityId: 5, authorId: 10, parentCommentId: 1);
        var user = CreateUser(10, firstName: "Alice", lastName: "Smith");

        SetupComments([topLevel, reply], [user]);

        var result = (await _service.GetByEntityAsync("Account", 5)).ToList();

        result.Should().HaveCount(1);
        result[0].Id.Should().Be(1);
        result[0].EntityType.Should().Be("Account");
        result[0].EntityId.Should().Be(5);
    }

    [Fact]
    public async Task GetByEntityAsync_ShouldReturnEmpty_WhenNoComments()
    {
        SetupComments();

        var result = await _service.GetByEntityAsync("Account", 99);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByEntityAsync_ShouldExcludeDeletedComments()
    {
        var active = CreateComment(1, entityType: "Account", entityId: 1);
        var deleted = CreateComment(2, entityType: "Account", entityId: 1, isDeleted: true);
        var user = CreateUser(10);

        SetupComments([active, deleted], [user]);

        var result = (await _service.GetByEntityAsync("Account", 1)).ToList();

        result.Should().HaveCount(1);
        result[0].Id.Should().Be(1);
    }

    [Fact]
    public async Task GetByEntityAsync_ShouldReturnAuthorName()
    {
        var comment = CreateComment(1, entityType: "Lead", entityId: 7, authorId: 42);
        var user = CreateUser(42, firstName: "Bob", lastName: "Jones");

        SetupComments([comment], [user]);

        var result = (await _service.GetByEntityAsync("Lead", 7)).ToList();

        result.Should().HaveCount(1);
        result[0].AuthorName.Should().Be("Bob Jones");
    }

    // ── GetByIdAsync Tests ────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ShouldReturnComment_WhenFound()
    {
        var comment = CreateComment(1);
        var user = CreateUser(10);

        SetupComments([comment], [user]);

        var result = await _service.GetByIdAsync(1);

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        SetupComments();

        var result = await _service.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenCommentIsDeleted()
    {
        var deleted = CreateComment(1, isDeleted: true);

        SetupComments([deleted]);

        var result = await _service.GetByIdAsync(1);

        result.Should().BeNull();
    }

    // ── CreateAsync Tests ─────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ShouldSetEntityTypeAndEntityId()
    {
        var comments = new List<RecordComment>();
        var user = CreateUser(5, firstName: "Charlie", lastName: "Brown");

        var mockComments = MockDbSetFactory.CreateMockDbSet(comments);
        var mockUsers = MockDbSetFactory.CreateMockDbSet(new List<User> { user });
        _mockContext.Setup(c => c.RecordComments).Returns(mockComments.Object);
        _mockContext.Setup(c => c.Users).Returns(mockUsers.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Simulate add by capturing what is added
        RecordComment? captured = null;
        mockComments.Setup(m => m.Add(It.IsAny<RecordComment>()))
            .Callback<RecordComment>(comment =>
            {
                captured = comment;
                comments.Add(comment);
            });

        var dto = new CreateRecordCommentDto
        {
            EntityType = "Opportunity",
            EntityId = 55,
            Content = "Great deal!",
        };

        var result = await _service.CreateAsync(dto, authorId: 5);

        captured.Should().NotBeNull();
        captured!.EntityType.Should().Be("Opportunity");
        captured.EntityId.Should().Be(55);
        captured.AuthorId.Should().Be(5);
        captured.IsDeleted.Should().BeFalse();
        captured.CreatedAt.Should().NotBe(default);
        result.Content.Should().Be("Great deal!");
    }

    [Fact]
    public async Task CreateAsync_ShouldSupportParentCommentId_ForReplies()
    {
        var comments = new List<RecordComment>();
        var user = CreateUser(10);

        var mockComments = MockDbSetFactory.CreateMockDbSet(comments);
        var mockUsers = MockDbSetFactory.CreateMockDbSet(new List<User> { user });
        _mockContext.Setup(c => c.RecordComments).Returns(mockComments.Object);
        _mockContext.Setup(c => c.Users).Returns(mockUsers.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        int? capturedParentId = -1;
        mockComments.Setup(m => m.Add(It.IsAny<RecordComment>()))
            .Callback<RecordComment>(c =>
            {
                capturedParentId = c.ParentCommentId;
                comments.Add(c);
            });

        var dto = new CreateRecordCommentDto
        {
            EntityType = "Account",
            EntityId = 1,
            Content = "Reply text",
            ParentCommentId = 42
        };

        await _service.CreateAsync(dto, authorId: 10);

        capturedParentId.Should().Be(42);
    }

    // ── UpdateAsync Tests ─────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ShouldUpdateContent_WhenOwner()
    {
        var comment = CreateComment(1, authorId: 10, content: "Original");
        var user = CreateUser(10);

        SetupComments([comment], [user]);

        var updateDto = new UpdateRecordCommentDto { Content = "Updated content" };
        var result = await _service.UpdateAsync(1, updateDto, userId: 10);

        result.Should().NotBeNull();
        comment.Content.Should().Be("Updated content");
        comment.UpdatedAt.Should().NotBe(default);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNull_WhenNotOwner()
    {
        var comment = CreateComment(1, authorId: 10);

        SetupComments([comment]);

        var updateDto = new UpdateRecordCommentDto { Content = "Hacked" };
        var result = await _service.UpdateAsync(1, updateDto, userId: 99);

        result.Should().BeNull();
        comment.Content.Should().Be("Test comment content"); // unchanged
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNull_WhenCommentNotFound()
    {
        SetupComments();

        var updateDto = new UpdateRecordCommentDto { Content = "Anything" };
        var result = await _service.UpdateAsync(999, updateDto, userId: 10);

        result.Should().BeNull();
    }

    // ── DeleteAsync Tests ─────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ShouldSoftDelete_WhenOwner()
    {
        var comment = CreateComment(1, authorId: 10);

        SetupComments([comment]);

        var result = await _service.DeleteAsync(1, userId: 10);

        result.Should().BeTrue();
        comment.IsDeleted.Should().BeTrue();
        comment.UpdatedAt.Should().NotBe(default);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenNotOwnerAndNotAdmin()
    {
        var comment = CreateComment(1, authorId: 10);
        var nonAdmin = CreateUser(99, role: UserRole.Sales);

        SetupComments([comment], [nonAdmin]);

        var result = await _service.DeleteAsync(1, userId: 99);

        result.Should().BeFalse();
        comment.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldSucceed_WhenUserIsAdmin()
    {
        var comment = CreateComment(1, authorId: 10);
        var admin = CreateUser(99, role: UserRole.Admin);

        SetupComments([comment], [admin]);

        var result = await _service.DeleteAsync(1, userId: 99);

        result.Should().BeTrue();
        comment.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenNotFound()
    {
        SetupComments();

        var result = await _service.DeleteAsync(999, userId: 10);

        result.Should().BeFalse();
    }

    // ── GetThreadAsync Tests ──────────────────────────────────────────────────

    [Fact]
    public async Task GetThreadAsync_ShouldReturnReplies_ForParentComment()
    {
        var reply1 = CreateComment(2, authorId: 10, parentCommentId: 1);
        var reply2 = CreateComment(3, authorId: 11, parentCommentId: 1);
        var topLevel = CreateComment(4, authorId: 10, parentCommentId: null);
        var user10 = CreateUser(10, firstName: "Alice", lastName: "A");
        var user11 = CreateUser(11, firstName: "Bob", lastName: "B");

        SetupComments([reply1, reply2, topLevel], [user10, user11]);

        var result = (await _service.GetThreadAsync(1)).ToList();

        result.Should().HaveCount(2);
        result.Should().OnlyContain(r => r.ParentCommentId == 1);
    }

    [Fact]
    public async Task GetThreadAsync_ShouldReturnEmpty_WhenNoReplies()
    {
        SetupComments([CreateComment(1, authorId: 10)]);

        var result = await _service.GetThreadAsync(999);

        result.Should().BeEmpty();
    }
}
