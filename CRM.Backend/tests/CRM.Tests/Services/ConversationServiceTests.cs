// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>Unit tests for ConversationService (TCOV Wave-A).</summary>
public class ConversationServiceTests : IDisposable
{
    private readonly CrmDbContext _context;
    private readonly Mock<ILogger<ConversationService>> _logger;
    private readonly ConversationService _service;

    public ConversationServiceTests()
    {
        var opts = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new CrmDbContext(opts, null!);
        _logger = new Mock<ILogger<ConversationService>>();
        _service = new ConversationService(_context, _logger.Object);
    }

    public void Dispose() => _context.Dispose();

    // ── GetAllAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllNonDeleted_WhenNoFilters()
    {
        _context.Conversations.Add(new Conversation { Id = 1, Subject = "Open Conv", IsDeleted = false, Status = ConversationStatus.Open });
        _context.Conversations.Add(new Conversation { Id = 2, Subject = "Deleted Conv", IsDeleted = true });
        await _context.SaveChangesAsync();

        var result = await _service.GetAllAsync();

        result.Should().HaveCount(1);
        result.First().Subject.Should().Be("Open Conv");
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByAccountId()
    {
        _context.Conversations.Add(new Conversation { Id = 1, Subject = "Acct Conv", AccountId = 10, IsDeleted = false });
        _context.Conversations.Add(new Conversation { Id = 2, Subject = "Other Conv", AccountId = 20, IsDeleted = false });
        await _context.SaveChangesAsync();

        var result = await _service.GetAllAsync(accountId: 10);

        result.Should().HaveCount(1);
        result.First().Subject.Should().Be("Acct Conv");
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByStatus()
    {
        _context.Conversations.Add(new Conversation { Id = 1, Subject = "Open", Status = ConversationStatus.Open, IsDeleted = false });
        _context.Conversations.Add(new Conversation { Id = 2, Subject = "Closed", Status = ConversationStatus.Closed, IsDeleted = false });
        await _context.SaveChangesAsync();

        var result = await _service.GetAllAsync(status: ConversationStatus.Closed);

        result.Should().HaveCount(1);
        result.First().Subject.Should().Be("Closed");
    }

    // ── GetByIdAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ShouldReturnConversation_WhenExists()
    {
        _context.Conversations.Add(new Conversation { Id = 5, Subject = "Find Me", IsDeleted = false });
        await _context.SaveChangesAsync();

        var result = await _service.GetByIdAsync(5);

        result.Should().NotBeNull();
        result!.Subject.Should().Be("Find Me");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _service.GetByIdAsync(999);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenDeleted()
    {
        _context.Conversations.Add(new Conversation { Id = 3, Subject = "Deleted", IsDeleted = true });
        await _context.SaveChangesAsync();

        var result = await _service.GetByIdAsync(3);

        result.Should().BeNull();
    }

    // ── CreateAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ShouldCreateConversation_WithTimestamp()
    {
        var conversation = new Conversation { Subject = "New Thread", Status = ConversationStatus.Open };

        var created = await _service.CreateAsync(conversation);

        created.Should().NotBeNull();
        created.Subject.Should().Be("New Thread");
        created.IsDeleted.Should().BeFalse();
        created.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowArgumentNullException_WhenConversationIsNull()
    {
        Func<Task> act = () => _service.CreateAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // ── UpdateAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ShouldReturnTrue_AndUpdateFields_WhenExists()
    {
        _context.Conversations.Add(new Conversation { Id = 10, Subject = "Old Subject", IsDeleted = false });
        await _context.SaveChangesAsync();

        var updates = new Conversation { Subject = "Updated Subject", Status = ConversationStatus.Resolved };
        var result = await _service.UpdateAsync(10, updates);

        result.Should().BeTrue();
        var inDb = await _context.Conversations.FindAsync(10);
        inDb!.Subject.Should().Be("Updated Subject");
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenNotFound()
    {
        var result = await _service.UpdateAsync(999, new Conversation { Subject = "Ghost" });
        result.Should().BeFalse();
    }

    // ── DeleteAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ShouldReturnTrue_AndSoftDelete_WhenExists()
    {
        _context.Conversations.Add(new Conversation { Id = 20, Subject = "To Delete", IsDeleted = false });
        await _context.SaveChangesAsync();

        var result = await _service.DeleteAsync(20);

        result.Should().BeTrue();
        var inDb = await _context.Conversations.FindAsync(20);
        inDb!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenNotFound()
    {
        var result = await _service.DeleteAsync(999);
        result.Should().BeFalse();
    }

    // ── UpdateStatusAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task UpdateStatusAsync_ShouldReturnTrue_AndChangeStatus_WhenExists()
    {
        _context.Conversations.Add(new Conversation { Id = 30, Subject = "Status Test", Status = ConversationStatus.Open, IsDeleted = false });
        await _context.SaveChangesAsync();

        var result = await _service.UpdateStatusAsync(30, ConversationStatus.Closed);

        result.Should().BeTrue();
        var inDb = await _context.Conversations.FindAsync(30);
        inDb!.Status.Should().Be(ConversationStatus.Closed);
    }
}
