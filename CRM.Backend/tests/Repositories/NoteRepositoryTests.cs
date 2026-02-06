// CRM Solution - Customer Relationship Management System
// Note Repository Unit Tests

using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace CRM.Tests.Repositories;

/// <summary>
/// Unit tests for Note Repository
/// Covers: Note-specific queries, entity relationships, privacy
/// </summary>
public class NoteRepositoryTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<DbSet<NoteEntity>> _mockDbSet;
    private readonly Mock<ILogger<NoteRepository>> _mockLogger;
    private readonly NoteRepository _repository;

    public NoteRepositoryTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockDbSet = new Mock<DbSet<NoteEntity>>();
        _mockLogger = new Mock<ILogger<NoteRepository>>();

        _mockContext.Setup(c => c.Set<NoteEntity>()).Returns(_mockDbSet.Object);
        _repository = new NoteRepository(_mockContext.Object, _mockLogger.Object);
    }

    #region GetByEntity Tests

    [Fact]
    public async Task GetByAccountAsync_HasNotes_ReturnsAccountNotes()
    {
        // Arrange
        var notes = new List<NoteEntity>
        {
            new NoteEntity { Id = 1, EntityType = "Account", EntityId = 1 },
            new NoteEntity { Id = 2, EntityType = "Account", EntityId = 1 },
            new NoteEntity { Id = 3, EntityType = "Account", EntityId = 2 }
        }.AsQueryable();

        SetupMockDbSet(notes);

        // Act
        var result = await _repository.GetByAccountAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByContactAsync_HasNotes_ReturnsContactNotes()
    {
        // Arrange
        var notes = new List<NoteEntity>
        {
            new NoteEntity { Id = 1, EntityType = "Contact", EntityId = 1 },
            new NoteEntity { Id = 2, EntityType = "Contact", EntityId = 1 },
            new NoteEntity { Id = 3, EntityType = "Contact", EntityId = 2 }
        }.AsQueryable();

        SetupMockDbSet(notes);

        // Act
        var result = await _repository.GetByContactAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByOpportunityAsync_HasNotes_ReturnsOpportunityNotes()
    {
        // Arrange
        var notes = new List<NoteEntity>
        {
            new NoteEntity { Id = 1, EntityType = "Opportunity", EntityId = 1 },
            new NoteEntity { Id = 2, EntityType = "Opportunity", EntityId = 1 },
            new NoteEntity { Id = 3, EntityType = "Opportunity", EntityId = 2 }
        }.AsQueryable();

        SetupMockDbSet(notes);

        // Act
        var result = await _repository.GetByOpportunityAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByLeadAsync_HasNotes_ReturnsLeadNotes()
    {
        // Arrange
        var notes = new List<NoteEntity>
        {
            new NoteEntity { Id = 1, EntityType = "Lead", EntityId = 1 },
            new NoteEntity { Id = 2, EntityType = "Lead", EntityId = 1 },
            new NoteEntity { Id = 3, EntityType = "Lead", EntityId = 2 }
        }.AsQueryable();

        SetupMockDbSet(notes);

        // Act
        var result = await _repository.GetByLeadAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByEntityAsync_GenericQuery_ReturnsNotes()
    {
        // Arrange
        var notes = new List<NoteEntity>
        {
            new NoteEntity { Id = 1, EntityType = "Quote", EntityId = 1 },
            new NoteEntity { Id = 2, EntityType = "Quote", EntityId = 1 },
            new NoteEntity { Id = 3, EntityType = "Quote", EntityId = 2 }
        }.AsQueryable();

        SetupMockDbSet(notes);

        // Act
        var result = await _repository.GetByEntityAsync("Quote", 1);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region GetByAuthor Tests

    [Fact]
    public async Task GetByAuthorAsync_HasNotes_ReturnsAuthorNotes()
    {
        // Arrange
        var notes = new List<NoteEntity>
        {
            new NoteEntity { Id = 1, AuthorId = 1 },
            new NoteEntity { Id = 2, AuthorId = 1 },
            new NoteEntity { Id = 3, AuthorId = 2 }
        }.AsQueryable();

        SetupMockDbSet(notes);

        // Act
        var result = await _repository.GetByAuthorAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Privacy Tests

    [Fact]
    public async Task GetPublicAsync_ReturnsPublicNotes()
    {
        // Arrange
        var notes = new List<NoteEntity>
        {
            new NoteEntity { Id = 1, IsPrivate = false },
            new NoteEntity { Id = 2, IsPrivate = false },
            new NoteEntity { Id = 3, IsPrivate = true }
        }.AsQueryable();

        SetupMockDbSet(notes);

        // Act
        var result = await _repository.GetPublicAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPrivateAsync_ReturnsPrivateNotes()
    {
        // Arrange
        var notes = new List<NoteEntity>
        {
            new NoteEntity { Id = 1, IsPrivate = true },
            new NoteEntity { Id = 2, IsPrivate = true },
            new NoteEntity { Id = 3, IsPrivate = false }
        }.AsQueryable();

        SetupMockDbSet(notes);

        // Act
        var result = await _repository.GetPrivateAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPrivateByUserAsync_ReturnsUserPrivateNotes()
    {
        // Arrange
        var notes = new List<NoteEntity>
        {
            new NoteEntity { Id = 1, IsPrivate = true, AuthorId = 1 },
            new NoteEntity { Id = 2, IsPrivate = true, AuthorId = 1 },
            new NoteEntity { Id = 3, IsPrivate = true, AuthorId = 2 }
        }.AsQueryable();

        SetupMockDbSet(notes);

        // Act
        var result = await _repository.GetPrivateByUserAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Category Tests

    [Fact]
    public async Task GetByCategoryAsync_HasNotes_ReturnsCategoryNotes()
    {
        // Arrange
        var notes = new List<NoteEntity>
        {
            new NoteEntity { Id = 1, Category = "Meeting" },
            new NoteEntity { Id = 2, Category = "Meeting" },
            new NoteEntity { Id = 3, Category = "Call" }
        }.AsQueryable();

        SetupMockDbSet(notes);

        // Act
        var result = await _repository.GetByCategoryAsync("Meeting");

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Pinned Tests

    [Fact]
    public async Task GetPinnedAsync_ReturnsPinnedNotes()
    {
        // Arrange
        var notes = new List<NoteEntity>
        {
            new NoteEntity { Id = 1, IsPinned = true },
            new NoteEntity { Id = 2, IsPinned = true },
            new NoteEntity { Id = 3, IsPinned = false }
        }.AsQueryable();

        SetupMockDbSet(notes);

        // Act
        var result = await _repository.GetPinnedAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPinnedByEntityAsync_ReturnsPinnedEntityNotes()
    {
        // Arrange
        var notes = new List<NoteEntity>
        {
            new NoteEntity { Id = 1, IsPinned = true, EntityType = "Account", EntityId = 1 },
            new NoteEntity { Id = 2, IsPinned = true, EntityType = "Account", EntityId = 1 },
            new NoteEntity { Id = 3, IsPinned = false, EntityType = "Account", EntityId = 1 }
        }.AsQueryable();

        SetupMockDbSet(notes);

        // Act
        var result = await _repository.GetPinnedByEntityAsync("Account", 1);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task SearchAsync_ByContent_ReturnsMatches()
    {
        // Arrange
        var notes = new List<NoteEntity>
        {
            new NoteEntity { Id = 1, Title = "Meeting Notes", Content = "Discussed project timeline" },
            new NoteEntity { Id = 2, Title = "Project Update", Content = "Project status review" },
            new NoteEntity { Id = 3, Title = "Call Summary", Content = "Customer inquiry" }
        }.AsQueryable();

        SetupMockDbSet(notes);

        // Act
        var result = await _repository.SearchAsync("project");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchByTitleAsync_ReturnsMatches()
    {
        // Arrange
        var notes = new List<NoteEntity>
        {
            new NoteEntity { Id = 1, Title = "Meeting Notes" },
            new NoteEntity { Id = 2, Title = "Meeting Summary" },
            new NoteEntity { Id = 3, Title = "Call Summary" }
        }.AsQueryable();

        SetupMockDbSet(notes);

        // Act
        var result = await _repository.SearchByTitleAsync("Meeting");

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Date Range Tests

    [Fact]
    public async Task GetByDateRangeAsync_ReturnsNotesInRange()
    {
        // Arrange
        var notes = new List<NoteEntity>
        {
            new NoteEntity { Id = 1, CreatedAt = DateTime.UtcNow.AddDays(-5) },
            new NoteEntity { Id = 2, CreatedAt = DateTime.UtcNow.AddDays(-15) },
            new NoteEntity { Id = 3, CreatedAt = DateTime.UtcNow.AddDays(-40) }
        }.AsQueryable();

        SetupMockDbSet(notes);

        // Act
        var result = await _repository.GetByDateRangeAsync(
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetRecentAsync_ReturnsRecentNotes()
    {
        // Arrange
        var notes = new List<NoteEntity>
        {
            new NoteEntity { Id = 1, CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new NoteEntity { Id = 2, CreatedAt = DateTime.UtcNow.AddDays(-5) },
            new NoteEntity { Id = 3, CreatedAt = DateTime.UtcNow.AddDays(-15) }
        }.AsQueryable();

        SetupMockDbSet(notes);

        // Act
        var result = await _repository.GetRecentAsync(7);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetTodayAsync_ReturnsTodayNotes()
    {
        // Arrange
        var notes = new List<NoteEntity>
        {
            new NoteEntity { Id = 1, CreatedAt = DateTime.UtcNow.Date },
            new NoteEntity { Id = 2, CreatedAt = DateTime.UtcNow.Date },
            new NoteEntity { Id = 3, CreatedAt = DateTime.UtcNow.AddDays(-5) }
        }.AsQueryable();

        SetupMockDbSet(notes);

        // Act
        var result = await _repository.GetTodayAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetCountByEntityTypeAsync_ReturnsEntityTypeCounts()
    {
        // Arrange
        var notes = new List<NoteEntity>
        {
            new NoteEntity { Id = 1, EntityType = "Account" },
            new NoteEntity { Id = 2, EntityType = "Account" },
            new NoteEntity { Id = 3, EntityType = "Contact" }
        }.AsQueryable();

        SetupMockDbSet(notes);

        // Act
        var result = await _repository.GetCountByEntityTypeAsync();

        // Assert
        result["Account"].Should().Be(2);
    }

    [Fact]
    public async Task GetCountByAuthorAsync_ReturnsAuthorCounts()
    {
        // Arrange
        var notes = new List<NoteEntity>
        {
            new NoteEntity { Id = 1, AuthorId = 1 },
            new NoteEntity { Id = 2, AuthorId = 1 },
            new NoteEntity { Id = 3, AuthorId = 2 }
        }.AsQueryable();

        SetupMockDbSet(notes);

        // Act
        var result = await _repository.GetCountByAuthorAsync();

        // Assert
        result[1].Should().Be(2);
    }

    [Fact]
    public async Task GetDailyCountAsync_ReturnsDailyCounts()
    {
        // Arrange
        var notes = new List<NoteEntity>
        {
            new NoteEntity { Id = 1, CreatedAt = DateTime.UtcNow.Date },
            new NoteEntity { Id = 2, CreatedAt = DateTime.UtcNow.Date },
            new NoteEntity { Id = 3, CreatedAt = DateTime.UtcNow.AddDays(-1).Date }
        }.AsQueryable();

        SetupMockDbSet(notes);

        // Act
        var result = await _repository.GetDailyCountAsync(7);

        // Assert
        result.Should().HaveCountGreaterThan(0);
    }

    #endregion

    #region Attachment Tests

    [Fact]
    public async Task GetWithAttachmentsAsync_ReturnsNotesWithAttachments()
    {
        // Arrange
        var notes = new List<NoteEntity>
        {
            new NoteEntity { Id = 1, HasAttachments = true },
            new NoteEntity { Id = 2, HasAttachments = true },
            new NoteEntity { Id = 3, HasAttachments = false }
        }.AsQueryable();

        SetupMockDbSet(notes);

        // Act
        var result = await _repository.GetWithAttachmentsAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Bulk Operations Tests

    [Fact]
    public async Task BulkDeleteAsync_DeletesNotes()
    {
        // Arrange
        var noteIds = new[] { 1, 2, 3 };
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(3);

        // Act
        var result = await _repository.BulkDeleteAsync(noteIds);

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public async Task BulkUpdatePrivacyAsync_UpdatesPrivacy()
    {
        // Arrange
        var noteIds = new[] { 1, 2, 3 };
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(3);

        // Act
        var result = await _repository.BulkUpdatePrivacyAsync(noteIds, true);

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public async Task DeleteByEntityAsync_DeletesEntityNotes()
    {
        // Arrange
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(5);

        // Act
        var result = await _repository.DeleteByEntityAsync("Account", 1);

        // Assert
        result.Should().Be(5);
    }

    #endregion

    #region Helper Methods

    private void SetupMockDbSet(IQueryable<NoteEntity> data)
    {
        _mockDbSet.As<IQueryable<NoteEntity>>().Setup(m => m.Provider).Returns(data.Provider);
        _mockDbSet.As<IQueryable<NoteEntity>>().Setup(m => m.Expression).Returns(data.Expression);
        _mockDbSet.As<IQueryable<NoteEntity>>().Setup(m => m.ElementType).Returns(data.ElementType);
        _mockDbSet.As<IQueryable<NoteEntity>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
    }

    #endregion
}

// Supporting class
public class NoteEntity
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public int AuthorId { get; set; }
    public string? Category { get; set; }
    public bool IsPrivate { get; set; }
    public bool IsPinned { get; set; }
    public bool HasAttachments { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
