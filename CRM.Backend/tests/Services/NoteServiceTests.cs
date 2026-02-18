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

public class NoteServiceTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<NoteService>> _mockLogger;
    private readonly NoteService _service;

    public NoteServiceTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<NoteService>>();
        _service = new NoteService(_mockContext.Object, _mockLogger.Object);
    }

    private void SetupNotes(List<Note>? notes = null)
    {
        notes ??= new List<Note>();
        var mockNotes = MockDbSetFactory.CreateMockDbSet(notes);
        _mockContext.Setup(c => c.Notes).Returns(mockNotes.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    private static Note CreateNote(int id, int? accountId = null, bool pinned = false, bool deleted = false)
    {
        return new Note
        {
            Id = id,
            Title = $"Note {id}",
            Content = "Content",
            AccountId = accountId,
            IsPinned = pinned,
            CreatedAt = DateTime.UtcNow.AddMinutes(-id),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-id),
            IsDeleted = deleted
        };
    }

    [Fact]
    public async Task GetNotesAsync_ShouldFilterByAccountAndPinned()
    {
        var notes = new List<Note>
        {
            CreateNote(1, accountId: 10, pinned: true),
            CreateNote(2, accountId: 10, pinned: false),
            CreateNote(3, accountId: 20, pinned: true)
        };
        SetupNotes(notes);

        var result = await _service.GetNotesAsync(accountId: 10, pinned: true);

        result.Should().HaveCount(1);
        result.First().Id.Should().Be(1);
    }

    [Fact]
    public async Task GetNotesAsync_ShouldExcludeDeletedNotes()
    {
        var notes = new List<Note>
        {
            CreateNote(1, accountId: 10, pinned: true, deleted: true),
            CreateNote(2, accountId: 10, pinned: false)
        };
        SetupNotes(notes);

        var result = await _service.GetNotesAsync(accountId: 10);

        result.Should().HaveCount(1);
        result.First().Id.Should().Be(2);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNote_WhenExists()
    {
        var notes = new List<Note> { CreateNote(1) };
        SetupNotes(notes);

        var result = await _service.GetByIdAsync(1);

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenMissing()
    {
        SetupNotes(new List<Note>());

        var result = await _service.GetByIdAsync(99);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldSetDefaultsAndPersist()
    {
        var notes = new List<Note>();
        SetupNotes(notes);
        var note = new Note { Title = "New", Content = "Body" };

        var result = await _service.CreateAsync(note);

        result.IsDeleted.Should().BeFalse();
        result.CreatedAt.Should().NotBe(default);
        result.UpdatedAt.Should().NotBe(default);
        notes.Should().Contain(note);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenNoteMissing()
    {
        SetupNotes(new List<Note>());

        var result = await _service.UpdateAsync(1, new Note { Title = "Updated", Content = "Body" });

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateFields_WhenNoteExists()
    {
        var existing = CreateNote(1, accountId: 10, pinned: false);
        var notes = new List<Note> { existing };
        SetupNotes(notes);

        var updated = new Note
        {
            Title = "Updated",
            Content = "New Content",
            NoteType = NoteType.CallNotes,
            Visibility = NoteVisibility.Public,
            IsPinned = true,
            IsImportant = true,
            AccountId = 20
        };

        var result = await _service.UpdateAsync(1, updated);

        result.Should().BeTrue();
        existing.Title.Should().Be("Updated");
        existing.Content.Should().Be("New Content");
        existing.NoteType.Should().Be(NoteType.CallNotes);
        existing.Visibility.Should().Be(NoteVisibility.Public);
        existing.IsPinned.Should().BeTrue();
        existing.IsImportant.Should().BeTrue();
        existing.AccountId.Should().Be(20);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteNote_WhenExists()
    {
        var existing = CreateNote(1, accountId: 10, pinned: false);
        var notes = new List<Note> { existing };
        SetupNotes(notes);

        var result = await _service.DeleteAsync(1);

        result.Should().BeTrue();
        existing.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task TogglePinAsync_ShouldFlipPinnedState()
    {
        var existing = CreateNote(1, accountId: 10, pinned: false);
        var notes = new List<Note> { existing };
        SetupNotes(notes);

        var result = await _service.TogglePinAsync(1);

        result.Should().BeTrue();
        existing.IsPinned.Should().BeTrue();
    }

    [Fact]
    public async Task GetByEntityAsync_ShouldReturnNotesForAccount()
    {
        var notes = new List<Note>
        {
            CreateNote(1, accountId: 5),
            new Note { Id = 2, Title = "Entity", Content = "Body", EntityType = "account", EntityId = 5, CreatedAt = DateTime.UtcNow },
            CreateNote(3, accountId: 6)
        };
        SetupNotes(notes);

        var result = await _service.GetByEntityAsync("account", 5);

        result.Should().HaveCount(2);
        result.Should().OnlyContain(n => n.AccountId == 5 || (n.EntityType == "account" && n.EntityId == 5));
    }
}
