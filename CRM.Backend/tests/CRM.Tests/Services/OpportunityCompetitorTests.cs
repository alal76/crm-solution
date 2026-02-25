// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for OpportunityService — competitor update operations (TODO-CRM003-03).
/// </summary>
public class OpportunityCompetitorTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly OpportunityService _service;
    private readonly List<Opportunity> _opportunities;
    private readonly List<OpportunityCompetitor> _competitors;

    public OpportunityCompetitorTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _opportunities = new List<Opportunity>();
        _competitors = new List<OpportunityCompetitor>();

        SetupMocks();

        _service = new OpportunityService(
            Mock.Of<IRepository<Opportunity>>(),
            Mock.Of<IRepository<CRM.Core.Entities.EntityTag>>(),
            Mock.Of<IRepository<CRM.Core.Entities.CustomField>>(),
            new NormalizationService(_mockContext.Object),
            Mock.Of<IEntityEventDispatcher>(),
            Mock.Of<IDuplicateDetectionService>(),
            _mockContext.Object,
            Mock.Of<ILogger<OpportunityService>>());
    }

    private void SetupMocks()
    {
        var mockCompetitors = MockDbSetFactory.CreateMockDbSet(_competitors);
        _mockContext.Setup(c => c.OpportunityCompetitors).Returns(mockCompetitors.Object);

        var mockOpps = MockDbSetFactory.CreateMockDbSet(_opportunities);
        _mockContext.Setup(c => c.Opportunities).Returns(mockOpps.Object);

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    private void Refresh()
    {
        var mockCompetitors = MockDbSetFactory.CreateMockDbSet(_competitors);
        _mockContext.Setup(c => c.OpportunityCompetitors).Returns(mockCompetitors.Object);

        var mockOpps = MockDbSetFactory.CreateMockDbSet(_opportunities);
        _mockContext.Setup(c => c.Opportunities).Returns(mockOpps.Object);
    }

    // ========================================================================
    // UpdateCompetitorAsync Tests
    // ========================================================================

    [Fact]
    public async Task UpdateCompetitorAsync_ShouldReturnNull_WhenCompetitorNotFound()
    {
        // Arrange — no competitors in context
        Refresh();

        var patch = new OpportunityCompetitor
        {
            ThreatLevel = CompetitorThreatLevel.High,
            Status = OpportunityCompetitorStatus.Active
        };

        // Act
        var result = await _service.UpdateCompetitorAsync(1, 99, patch);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateCompetitorAsync_ShouldReturnUpdated_WhenExists()
    {
        // Arrange
        var existing = new OpportunityCompetitor
        {
            OpportunityId = 1,
            CompetitorId = 10,
            ThreatLevel = CompetitorThreatLevel.Low,
            Status = OpportunityCompetitorStatus.Identified,
            CompetitorPrice = 5000m,
            Notes = "Old note",
            WonAgainst = null,
            Competitor = new Competitor
            {
                Id = 10,
                Name = "Rival Corp",
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };
        _competitors.Add(existing);
        Refresh();

        var patch = new OpportunityCompetitor
        {
            ThreatLevel = CompetitorThreatLevel.High,
            Status = OpportunityCompetitorStatus.Active,
            CompetitorPrice = 8000m,
            Notes = "Updated note",
            WonAgainst = true
        };

        // Act
        var result = await _service.UpdateCompetitorAsync(1, 10, patch);

        // Assert
        result.Should().NotBeNull();
        result!.ThreatLevel.Should().Be(CompetitorThreatLevel.High);
        result.Status.Should().Be(OpportunityCompetitorStatus.Active);
        result.CompetitorPrice.Should().Be(8000m);
        result.Notes.Should().Be("Updated note");
        result.WonAgainst.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateCompetitorAsync_ShouldNotOverwritePrice_WhenPatchPriceIsNull()
    {
        // Arrange
        var existing = new OpportunityCompetitor
        {
            OpportunityId = 2,
            CompetitorId = 5,
            ThreatLevel = CompetitorThreatLevel.Medium,
            Status = OpportunityCompetitorStatus.Identified,
            CompetitorPrice = 12000m,
            Competitor = new Competitor
            {
                Id = 5,
                Name = "CompA",
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };
        _competitors.Add(existing);
        Refresh();

        var patch = new OpportunityCompetitor
        {
            ThreatLevel = CompetitorThreatLevel.Critical,
            Status = OpportunityCompetitorStatus.Leading
            // CompetitorPrice deliberately left null
        };

        // Act
        var result = await _service.UpdateCompetitorAsync(2, 5, patch);

        // Assert
        result.Should().NotBeNull();
        result!.CompetitorPrice.Should().Be(12000m);
        result.ThreatLevel.Should().Be(CompetitorThreatLevel.Critical);
    }

    [Fact]
    public async Task UpdateCompetitorAsync_ShouldCallSaveChanges_WhenUpdateSucceeds()
    {
        // Arrange
        var existing = new OpportunityCompetitor
        {
            OpportunityId = 3,
            CompetitorId = 7,
            ThreatLevel = CompetitorThreatLevel.Low,
            Status = OpportunityCompetitorStatus.Identified,
            Competitor = new Competitor
            {
                Id = 7,
                Name = "CompB",
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };
        _competitors.Add(existing);
        Refresh();

        var patch = new OpportunityCompetitor
        {
            ThreatLevel = CompetitorThreatLevel.High,
            Status = OpportunityCompetitorStatus.Active
        };

        // Act
        await _service.UpdateCompetitorAsync(3, 7, patch);

        // Assert
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCompetitorAsync_ShouldNotCallSaveChanges_WhenCompetitorNotFound()
    {
        // Arrange — empty context
        Refresh();

        var patch = new OpportunityCompetitor
        {
            ThreatLevel = CompetitorThreatLevel.High,
            Status = OpportunityCompetitorStatus.Active
        };

        // Act
        await _service.UpdateCompetitorAsync(42, 99, patch);

        // Assert
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
