// CRM Solution - Customer Relationship Management System
// FEAT-AISCORING: Unit tests for LeadScoreHistoryService
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace CRM.Tests.Unit.Services;

public sealed class LeadScoreHistoryServiceTests : IDisposable
{
    private readonly CrmDbContext _context;
    private readonly LeadScoreHistoryService _sut;

    public LeadScoreHistoryServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var config = Substitute.For<IConfiguration>();
        config["Jwt:Secret"].Returns("test-secret-key-at-least-32-characters-long!");
        config["Jwt:Issuer"].Returns("TestIssuer");
        config["Jwt:Audience"].Returns("TestAudience");
        config["Jwt:ExpirationMinutes"].Returns("60");

        _context = new CrmDbContext(options, config);
        _sut = new LeadScoreHistoryService(_context, NullLogger<LeadScoreHistoryService>.Instance);
    }

    public void Dispose() => _context.Dispose();

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    private Lead CreateLead(int fitScore = 50, DateTime? lastDecayDate = null)
    {
        var now = DateTime.UtcNow;
        var lead = new Lead
        {
            FirstName = "Test",
            LastName = "Lead",
            Email = $"lead-{Guid.NewGuid():N}@test.com",
            Status = LeadLifecycleStatus.New,
            Score = fitScore,
            FitScore = fitScore,
            LastScoreDecayDate = lastDecayDate,
            IsDeleted = false,
            CreatedAt = now,
            UpdatedAt = now,
        };
        _context.Set<Lead>().Add(lead);
        _context.SaveChanges();
        return lead;
    }

    private void AddHistoryEntry(int leadId, int score, int previous, int daysAgo)
    {
        _context.LeadScoreHistories.Add(new LeadScoreHistory
        {
            LeadId = leadId,
            Score = score,
            PreviousScore = previous,
            Delta = score - previous,
            Reason = "auto_score",
            ScoredBy = "system",
            ScoredAt = DateTime.UtcNow.AddDays(-daysAgo),
        });
        _context.SaveChanges();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GetHistoryAsync
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetHistoryAsync_ReturnsHistoryOrderedByDateDescending_WhenEntriesExist()
    {
        // Arrange
        var lead = CreateLead();
        AddHistoryEntry(lead.Id, 60, 50, daysAgo: 3);
        AddHistoryEntry(lead.Id, 70, 60, daysAgo: 1);
        AddHistoryEntry(lead.Id, 55, 70, daysAgo: 2);

        // Act
        var result = (await _sut.GetHistoryAsync(lead.Id)).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].Score.Should().Be(70); // most recent first
        result[1].Score.Should().Be(55);
        result[2].Score.Should().Be(60); // oldest last
    }

    [Fact]
    public async Task GetHistoryAsync_ReturnsEmpty_WhenNoHistoryExists()
    {
        // Arrange
        var lead = CreateLead();

        // Act
        var result = await _sut.GetHistoryAsync(lead.Id);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetHistoryAsync_RespectsLimit_WhenMoreHistoryExists()
    {
        // Arrange
        var lead = CreateLead();
        for (var i = 1; i <= 5; i++)
            AddHistoryEntry(lead.Id, 50 + i, 50, daysAgo: 6 - i);

        // Act
        var result = (await _sut.GetHistoryAsync(lead.Id, limit: 2)).ToList();

        // Assert
        result.Should().HaveCount(2);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GetExplanationAsync
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetExplanationAsync_ReturnsNull_WhenLeadNotFound()
    {
        // Act
        var result = await _sut.GetExplanationAsync(999_999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetExplanationAsync_BuildsComponentsFromLeadEntity_WhenLeadExists()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var lead = new Lead
        {
            FirstName = "Ana",
            LastName = "Garcia",
            Email = $"ana-{Guid.NewGuid():N}@test.com",
            Status = LeadLifecycleStatus.Qualified,
            FitScore = 75,
            EngagementScore = 80,
            BudgetScore = 60,
            AuthorityScore = 70,
            IsDeleted = false,
            CreatedAt = now,
            UpdatedAt = now,
        };
        _context.Set<Lead>().Add(lead);
        _context.SaveChanges();

        // Act
        var result = await _sut.GetExplanationAsync(lead.Id);

        // Assert
        result.Should().NotBeNull();
        result!.LeadId.Should().Be(lead.Id);
        result.CurrentScore.Should().Be(75);
        result.Components.Fit.Should().Be(75);
        result.Components.Engagement.Should().Be(80);
        result.Components.Budget.Should().Be(60);
        result.Components.Authority.Should().Be(70);
    }

    [Fact]
    public async Task GetExplanationAsync_ReturnsTrendStable_WhenNoHistory()
    {
        // Arrange
        var lead = CreateLead(fitScore: 60);

        // Act
        var result = await _sut.GetExplanationAsync(lead.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Trend.Should().Be("stable");
    }

    [Fact]
    public async Task GetExplanationAsync_ReturnsTrendImproving_WhenScoresAreRising()
    {
        // Arrange
        var lead = CreateLead(fitScore: 80);
        // older scores are lower; newest scores are higher → trend = improving
        // history is returned newest-first; we add older items with higher daysAgo
        AddHistoryEntry(lead.Id, 50, 45, daysAgo: 6);
        AddHistoryEntry(lead.Id, 55, 50, daysAgo: 5);
        AddHistoryEntry(lead.Id, 60, 55, daysAgo: 4);
        AddHistoryEntry(lead.Id, 70, 60, daysAgo: 3);
        AddHistoryEntry(lead.Id, 80, 70, daysAgo: 1);

        // Act
        var result = await _sut.GetExplanationAsync(lead.Id);

        // Assert
        result!.Trend.Should().Be("improving");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // RecordScoreAsync
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task RecordScoreAsync_CreatesHistoryEntryWithCorrectDelta_WhenCalled()
    {
        // Arrange
        var lead = CreateLead(fitScore: 60);

        // Act
        await _sut.RecordScoreAsync(
            leadId: lead.Id,
            newScore: 75,
            previousScore: 60,
            reason: "manual",
            scoredBy: "user");

        // Assert
        var entry = await _context.LeadScoreHistories
            .FirstOrDefaultAsync(h => h.LeadId == lead.Id);

        entry.Should().NotBeNull();
        entry!.Score.Should().Be(75);
        entry.PreviousScore.Should().Be(60);
        entry.Delta.Should().Be(15);
        entry.Reason.Should().Be("manual");
        entry.ScoredBy.Should().Be("user");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ApplyDecayAsync
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ApplyDecayAsync_ReducesScoreByFivePercent_WhenDecayDue()
    {
        // Arrange — lead last decayed 20 days ago (> 14 day threshold)
        var lead = CreateLead(fitScore: 100, lastDecayDate: DateTime.UtcNow.AddDays(-20));

        // Act
        await _sut.ApplyDecayAsync(lead.Id);

        // Assert
        var updated = await _context.Set<Lead>().FindAsync(lead.Id);
        updated!.FitScore.Should().Be(95); // floor(100 * 0.95) = 95

        var histEntry = await _context.LeadScoreHistories
            .FirstOrDefaultAsync(h => h.LeadId == lead.Id);
        histEntry.Should().NotBeNull();
        histEntry!.Reason.Should().Be("decay");
        histEntry.Delta.Should().Be(-5);
    }

    [Fact]
    public async Task ApplyDecayAsync_DoesNothing_WhenDecayedRecently()
    {
        // Arrange — lead was decayed only 5 days ago (< 14 day threshold)
        var lead = CreateLead(fitScore: 80, lastDecayDate: DateTime.UtcNow.AddDays(-5));

        // Act
        await _sut.ApplyDecayAsync(lead.Id);

        // Assert — score unchanged, no history entry
        var updated = await _context.Set<Lead>().FindAsync(lead.Id);
        updated!.FitScore.Should().Be(80);

        var histCount = await _context.LeadScoreHistories.CountAsync(h => h.LeadId == lead.Id);
        histCount.Should().Be(0, "no decay should happen when last decay was within 14 days");
    }

    [Fact]
    public async Task ApplyDecayAsync_DoesNothing_WhenLeadHasZeroScore()
    {
        // Arrange — FitScore is 0, decay should be skipped
        var lead = CreateLead(fitScore: 0);

        // Act
        await _sut.ApplyDecayAsync(lead.Id);

        // Assert
        var histCount = await _context.LeadScoreHistories.CountAsync(h => h.LeadId == lead.Id);
        histCount.Should().Be(0);
    }
}
