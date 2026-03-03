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

/// <summary>
/// Unit tests for LeadQualificationService (TODO-CRM002-08).
/// Covers BANT scoring, MEDDIC scoring, qualification levels, and edge cases.
/// </summary>
public class LeadQualificationServiceTests : ServiceTestFixtureBase<LeadQualificationService>
{    private readonly LeadQualificationService _service;
    private readonly List<Lead> _leads;

    public LeadQualificationServiceTests()
    {        _leads = new List<Lead>();
        Refresh();

        MockContext
            .Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _service = new LeadQualificationService(
            MockContext.Object,
            MockLogger.Object);
    }

    private void Refresh()
    {
        var mockLeads = MockDbSetFactory.CreateMockDbSet(_leads);
        MockContext.Setup(c => c.Leads).Returns(mockLeads.Object);
    }

    // ─── ScoreWithBANTAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task ScoreWithBANTAsync_ShouldReturnCombinedScore_WhenLeadExists()
    {
        // Arrange
        var lead = new Lead { Id = 1, FirstName = "Jane", LastName = "Doe", IsDeleted = false };
        _leads.Add(lead);
        Refresh();

        // Act
        var result = await _service.ScoreWithBANTAsync(
            leadId: 1,
            budgetScore: 80,
            authorityScore: 60,
            needScore: 70,
            timelineScore: 50);

        // Assert
        result.Should().NotBeNull();
        result.LeadId.Should().Be(1);
        result.CombinedScore.Should().Be((80 + 60 + 70 + 50) / 4); // 65
        result.DimensionScores.Should().ContainKey("Budget").WhoseValue.Should().Be(80);
        result.DimensionScores.Should().ContainKey("Authority").WhoseValue.Should().Be(60);
        result.Framework.Should().Be(QualificationFramework.BANT);
    }

    [Fact]
    public async Task ScoreWithBANTAsync_ShouldThrowKeyNotFoundException_WhenLeadNotFound()
    {
        // Arrange — empty list
        Refresh();

        // Act
        Func<Task> act = async () => await _service.ScoreWithBANTAsync(
            leadId: 99, budgetScore: 50, authorityScore: 50, needScore: 50, timelineScore: 50);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*99*");
    }

    [Fact]
    public async Task ScoreWithBANTAsync_ShouldClampScores_WhenOutOfRange()
    {
        // Arrange
        var lead = new Lead { Id = 2, FirstName = "Bob", LastName = "Smith", IsDeleted = false };
        _leads.Add(lead);
        Refresh();

        // Act
        var result = await _service.ScoreWithBANTAsync(2, 150, -20, 50, 50);

        // Assert — clamped to [0,100]
        result.DimensionScores["Budget"].Should().Be(100);
        result.DimensionScores["Authority"].Should().Be(0);
    }

    [Fact]
    public async Task ScoreWithBANTAsync_ShouldPersistFrameworkType_OnLead()
    {
        // Arrange
        var lead = new Lead { Id = 3, FirstName = "Alice", LastName = "Johnson", IsDeleted = false };
        _leads.Add(lead);
        Refresh();

        // Act
        await _service.ScoreWithBANTAsync(3, 70, 70, 70, 70);

        // Assert — lead entity was mutated
        lead.QualificationFrameworkType.Should().Be(QualificationFramework.BANT);
        lead.BudgetScore.Should().Be(70);
    }

    // ─── ScoreWithMEDDICAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task ScoreWithMEDDICAsync_ShouldComputeAverageOfSixDimensions()
    {
        // Arrange
        var lead = new Lead { Id = 4, FirstName = "Carlos", LastName = "Rivera", IsDeleted = false };
        _leads.Add(lead);
        Refresh();

        var scores = new MEDDICScores
        {
            MetricsScore = 80,
            EconomicBuyerScore = 60,
            DecisionCriteriaScore = 70,
            DecisionProcessScore = 50,
            IdentifyPainScore = 90,
            ChampionScore = 40
        };

        // Act
        var result = await _service.ScoreWithMEDDICAsync(4, scores);

        // Assert
        int expected = (80 + 60 + 70 + 50 + 90 + 40) / 6; // 65
        result.CombinedScore.Should().Be(expected);
        result.Framework.Should().Be(QualificationFramework.MEDDIC);
        result.DimensionScores.Should().ContainKey("Metrics").WhoseValue.Should().Be(80);
    }

    [Fact]
    public async Task ScoreWithMEDDICAsync_ShouldThrowKeyNotFoundException_WhenLeadMissing()
    {
        // Arrange — empty list
        Refresh();

        var scores = new MEDDICScores { MetricsScore = 50, EconomicBuyerScore = 50 };

        // Act
        Func<Task> act = async () => await _service.ScoreWithMEDDICAsync(404, scores);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    // ─── GetQualificationLevelAsync ───────────────────────────────────────────

    [Fact]
    public async Task GetQualificationAsync_ShouldReturnStoredScores_WhenLeadHasScores()
    {
        // Arrange
        var lead = new Lead
        {
            Id = 5,
            FirstName = "Dana",
            LastName = "Lee",
            IsDeleted = false,
            BudgetScore = 90,
            AuthorityScore = 85,
            NeedScore = 80,
            TimelineScore = 75,
            QualificationFrameworkType = QualificationFramework.BANT
        };
        _leads.Add(lead);
        Refresh();

        // Act
        var result = await _service.GetQualificationAsync(5);

        // Assert
        result.Should().NotBeNull();
        result!.Framework.Should().Be(QualificationFramework.BANT);
    }
}
