// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// Spec: SPEC-CRM003-06 (Opportunity Cloning)
// TODO-CRM003-06: Opportunity Cloning — unit tests
//
// MANDATORY TEST RULE: All method signatures, namespaces, and field names
// verified against the actual source before writing these tests.
// Source files read: OpportunityService.cs, IOpportunityService.cs,
//   Opportunity.cs, ICrmDbContext.cs, CrmExceptions.cs

using CRM.Core.Entities;
using CRM.Core.Exceptions;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for OpportunityService.CloneAsync (TODO-CRM003-06).
/// Verifies default name generation, custom name, product cloning, and not-found exception.
/// </summary>
public class OpportunityCloneTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly OpportunityService _service;

    public OpportunityCloneTests()
    {
        _mockContext = new Mock<ICrmDbContext>();

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

    // ────────────────────────────────────────────────────────────────────────
    // Helpers
    // ────────────────────────────────────────────────────────────────────────

    private void SetupOpportunities(List<Opportunity> opportunities)
    {
        var mockSet = MockDbSetFactory.CreateMockDbSet(opportunities);
        _mockContext.Setup(c => c.Opportunities).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    private void SetupOpportunityProducts(List<OpportunityProduct> products)
    {
        var mockSet = MockDbSetFactory.CreateMockDbSet(products);
        _mockContext.Setup(c => c.OpportunityProducts).Returns(mockSet.Object);
    }

    private static Opportunity CreateOpportunity(int id = 1, string name = "Enterprise Deal") => new()
    {
        Id = id,
        Name = name,
        AccountId = 10,
        Amount = 50_000m,
        Stage = OpportunityStage.Proposal,
        Probability = 50,
        SalesOwnerId = 5,
        ForecastCategory = ForecastCategory.BestCase,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        IsDeleted = false
    };

    // ────────────────────────────────────────────────────────────────────────
    // Tests
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CloneAsync_ShouldReturnCopyWithDefaultName_WhenNoOptionsProvided()
    {
        // Arrange
        var original = CreateOpportunity(id: 1, name: "Big Deal");
        SetupOpportunities([original]);
        SetupOpportunityProducts([]);

        // Act
        var result = await _service.CloneAsync(1, options: null);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Copy of Big Deal");
        result.AccountId.Should().Be(original.AccountId);
        result.Amount.Should().Be(original.Amount);
    }

    [Fact]
    public async Task CloneAsync_ShouldUseCustomName_WhenNewNameProvided()
    {
        // Arrange
        var original = CreateOpportunity(id: 1, name: "Original Deal");
        SetupOpportunities([original]);
        SetupOpportunityProducts([]);

        var options = new OpportunityCloneOptions { NewName = "Q3 Follow-up" };

        // Act
        var result = await _service.CloneAsync(1, options);

        // Assert
        result.Name.Should().Be("Q3 Follow-up");
    }

    [Fact]
    public async Task CloneAsync_ShouldResetStageToDiscovery_WhenResetStageIsTrue()
    {
        // Arrange
        var original = CreateOpportunity(id: 1);
        original.Stage = OpportunityStage.Negotiation;
        original.Probability = 75;

        SetupOpportunities([original]);
        SetupOpportunityProducts([]);

        var options = new OpportunityCloneOptions { ResetStage = true };

        // Act
        var result = await _service.CloneAsync(1, options);

        // Assert
        result.Stage.Should().Be(OpportunityStage.Discovery);
        result.Probability.Should().Be(OpportunityService.StageProbabilityDefaults[OpportunityStage.Discovery]);
    }

    [Fact]
    public async Task CloneAsync_ShouldThrowEntityNotFoundException_WhenOpportunityNotFound()
    {
        // Arrange
        SetupOpportunities([]); // empty — ID 99 does not exist

        // Act
        var act = async () => await _service.CloneAsync(99);

        // Assert
        await act.Should().ThrowAsync<EntityNotFoundException>()
            .WithMessage("*99*");
    }
}
