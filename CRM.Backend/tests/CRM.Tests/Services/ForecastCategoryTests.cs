// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.DTOs;
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
/// Unit tests for OpportunityService — forecast category operations (TODO-CRM003-07).
/// </summary>
public class ForecastCategoryTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly OpportunityService _service;
    private readonly List<Opportunity> _opportunities;

    public ForecastCategoryTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _opportunities = new List<Opportunity>();

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
        RefreshOpportunities();

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _mockContext.Setup(c => c.OpportunityCompetitors)
            .Returns(MockDbSetFactory.CreateMockDbSet(new List<OpportunityCompetitor>()).Object);
    }

    private void RefreshOpportunities()
    {
        var mockOpps = MockDbSetFactory.CreateMockDbSet(_opportunities);
        _mockContext.Setup(c => c.Opportunities).Returns(mockOpps.Object);
    }

    // ========================================================================
    // PatchForecastCategoryAsync Tests
    // ========================================================================

    [Fact]
    public async Task PatchForecastCategoryAsync_ShouldReturnFalse_WhenOpportunityNotFound()
    {
        // Arrange — no opportunities in context
        RefreshOpportunities();

        // Act
        var result = await _service.PatchForecastCategoryAsync(999, ForecastCategory.Commit);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task PatchForecastCategoryAsync_ShouldReturnFalse_WhenOpportunityIsDeleted()
    {
        // Arrange
        _opportunities.Add(new Opportunity
        {
            Id = 1,
            IsDeleted = true,
            ForecastCategory = ForecastCategory.Pipeline,
            Amount = 5000m,
            Probability = 10,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        RefreshOpportunities();

        // Act
        var result = await _service.PatchForecastCategoryAsync(1, ForecastCategory.BestCase);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task PatchForecastCategoryAsync_ShouldUpdateCategory_WhenOpportunityExists()
    {
        // Arrange
        var opp = new Opportunity
        {
            Id = 5,
            IsDeleted = false,
            ForecastCategory = ForecastCategory.Pipeline,
            Amount = 20000m,
            Probability = 40,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _opportunities.Add(opp);
        RefreshOpportunities();

        // Act
        var result = await _service.PatchForecastCategoryAsync(5, ForecastCategory.Commit);

        // Assert
        result.Should().BeTrue();
        opp.ForecastCategory.Should().Be(ForecastCategory.Commit);
    }

    [Fact]
    public async Task PatchForecastCategoryAsync_ShouldCallSaveChanges_WhenSuccessful()
    {
        // Arrange
        _opportunities.Add(new Opportunity
        {
            Id = 2,
            IsDeleted = false,
            ForecastCategory = ForecastCategory.Pipeline,
            Amount = 10000m,
            Probability = 25,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        RefreshOpportunities();

        // Act
        await _service.PatchForecastCategoryAsync(2, ForecastCategory.BestCase);

        // Assert
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ========================================================================
    // GetForecastSummaryAsync Tests
    // ========================================================================

    [Fact]
    public async Task GetForecastSummaryAsync_ShouldReturnEmptySummary_WhenNoOpportunities()
    {
        // Arrange — no data
        RefreshOpportunities();

        // Act
        var summary = await _service.GetForecastSummaryAsync();

        // Assert
        summary.Should().NotBeNull();
        summary.Categories.Should().BeEmpty();
        summary.TotalPipelineAmount.Should().Be(0m);
        summary.TotalWeightedAmount.Should().Be(0m);
    }

    [Fact]
    public async Task GetForecastSummaryAsync_ShouldGroupByForecastCategory()
    {
        // Arrange — 2 Pipeline, 1 Commit
        _opportunities.AddRange(new[]
        {
            new Opportunity { Id = 1, ForecastCategory = ForecastCategory.Pipeline, Amount = 10000m, Probability = 20, PricingModel = OpportunityPricingModel.OneTime, TermLengthMonths = 0, IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Opportunity { Id = 2, ForecastCategory = ForecastCategory.Pipeline, Amount = 5000m,  Probability = 15, PricingModel = OpportunityPricingModel.OneTime, TermLengthMonths = 0, IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Opportunity { Id = 3, ForecastCategory = ForecastCategory.Commit,   Amount = 30000m, Probability = 90, PricingModel = OpportunityPricingModel.OneTime, TermLengthMonths = 0, IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        });
        RefreshOpportunities();

        // Act
        var summary = await _service.GetForecastSummaryAsync();

        // Assert
        summary.Categories.Should().HaveCount(2);
        summary.TotalPipelineAmount.Should().Be(45000m);

        var pipelineLine = summary.Categories.FirstOrDefault(c => c.CategoryValue == (int)ForecastCategory.Pipeline);
        pipelineLine.Should().NotBeNull();
        pipelineLine!.Count.Should().Be(2);
        pipelineLine.TotalAmount.Should().Be(15000m);
    }

    [Fact]
    public async Task GetForecastSummaryAsync_ShouldComputeMrr_ForSubscriptionDeals()
    {
        // Arrange — subscription deal with 12-month term
        _opportunities.Add(new Opportunity
        {
            Id = 1,
            ForecastCategory = ForecastCategory.Commit,
            Amount = 12000m,
            Probability = 80,
            PricingModel = OpportunityPricingModel.Subscription,
            TermLengthMonths = 12,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        RefreshOpportunities();

        // Act
        var summary = await _service.GetForecastSummaryAsync();

        // Assert: MRR = 12000 / 12 = 1000, ARR = 1000 * 12 = 12000
        var commitLine = summary.Categories.First(c => c.CategoryValue == (int)ForecastCategory.Commit);
        commitLine.Mrr.Should().BeApproximately(1000m, 0.01m);
        commitLine.Arr.Should().BeApproximately(12000m, 0.01m);
        summary.TotalMrr.Should().BeApproximately(1000m, 0.01m);
    }

    [Fact]
    public async Task GetForecastSummaryAsync_ShouldComputeWeightedAmount_Correctly()
    {
        // Arrange
        _opportunities.Add(new Opportunity
        {
            Id = 1,
            ForecastCategory = ForecastCategory.BestCase,
            Amount = 20000m,
            Probability = 60,
            PricingModel = OpportunityPricingModel.OneTime,
            TermLengthMonths = 0,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        RefreshOpportunities();

        // Act
        var summary = await _service.GetForecastSummaryAsync();

        // Assert: weighted = 20000 * (60/100) = 12000
        summary.TotalWeightedAmount.Should().BeApproximately(12000m, 0.01m);
    }

    [Fact]
    public async Task GetForecastSummaryAsync_ShouldExcludeDeletedOpportunities()
    {
        // Arrange — mix of deleted and active
        _opportunities.AddRange(new[]
        {
            new Opportunity { Id = 1, ForecastCategory = ForecastCategory.Commit, Amount = 50000m, Probability = 90, PricingModel = OpportunityPricingModel.OneTime, TermLengthMonths = 0, IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Opportunity { Id = 2, ForecastCategory = ForecastCategory.Commit, Amount = 99999m, Probability = 90, PricingModel = OpportunityPricingModel.OneTime, TermLengthMonths = 0, IsDeleted = true,  CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        });
        RefreshOpportunities();

        // Act
        var summary = await _service.GetForecastSummaryAsync();

        // Assert: only the non-deleted one counts
        summary.TotalPipelineAmount.Should().Be(50000m);
        summary.Categories.First().Count.Should().Be(1);
    }
}
