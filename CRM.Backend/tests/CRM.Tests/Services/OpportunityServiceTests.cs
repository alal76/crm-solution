// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
using CRM.Core.Entities;
using CRM.Core.Entities.Workflow;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>Unit tests for OpportunityService (TCOV Wave-A).</summary>
public class OpportunityServiceTests
{
    private readonly Mock<IRepository<Opportunity>> _mockRepo;
    private readonly Mock<IRepository<CRM.Core.Entities.EntityTag>> _mockTagRepo;
    private readonly Mock<IRepository<CRM.Core.Entities.CustomField>> _mockCustomFieldRepo;
    private readonly NormalizationService _normalizationService;
    private readonly Mock<IEntityEventDispatcher> _mockDispatcher;
    private readonly Mock<IDuplicateDetectionService> _mockDuplicates;
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<ILogger<OpportunityService>> _mockLogger;
    private readonly OpportunityService _service;

    public OpportunityServiceTests()
    {
        _mockRepo = new Mock<IRepository<Opportunity>>();
        _mockTagRepo = new Mock<IRepository<CRM.Core.Entities.EntityTag>>();
        _mockCustomFieldRepo = new Mock<IRepository<CRM.Core.Entities.CustomField>>();
        _mockDispatcher = new Mock<IEntityEventDispatcher>();
        _mockDuplicates = new Mock<IDuplicateDetectionService>();
        _mockDbContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<OpportunityService>>();

        // Duplicate detection returns no duplicates — allows creation to proceed
        _mockDuplicates
            .Setup(d => d.CheckForDuplicatesAsync(
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string?>>(),
                It.IsAny<int?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DuplicateCheckResult());

        // Event dispatcher completes without side effects
        _mockDispatcher
            .Setup(e => e.DispatchEntityEventAsync(
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<WorkflowTriggerType>(),
                It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string?>(),
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // NormalizationService uses ICrmDbContext — use mock
        _normalizationService = new NormalizationService(_mockDbContext.Object);

        _service = new OpportunityService(
            _mockRepo.Object,
            _mockTagRepo.Object,
            _mockCustomFieldRepo.Object,
            _normalizationService,
            _mockDispatcher.Object,
            _mockDuplicates.Object,
            _mockDbContext.Object,
            _mockLogger.Object);
    }

    // ------------------------------------------------------------------
    // GetOpportunityByIdAsync
    // ------------------------------------------------------------------

    [Fact]
    public async Task GetOpportunityByIdAsync_ShouldReturnOpportunity_WhenExists()
    {
        var opp = new Opportunity { Id = 1, Name = "Big Deal", AccountId = 10 };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(opp);

        var result = await _service.GetOpportunityByIdAsync(1);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Big Deal");
    }

    [Fact]
    public async Task GetOpportunityByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        _mockRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Opportunity?)null);

        var result = await _service.GetOpportunityByIdAsync(999);

        result.Should().BeNull();
    }

    // ------------------------------------------------------------------
    // GetOpportunitiesByAccountAsync
    // ------------------------------------------------------------------

    [Fact]
    public async Task GetOpportunitiesByAccountAsync_ShouldReturnAccountOpportunities()
    {
        var opps = new List<Opportunity>
        {
            new() { Id = 1, Name = "Opp A", AccountId = 5, IsDeleted = false },
            new() { Id = 2, Name = "Opp B", AccountId = 5, IsDeleted = false }
        };
        _mockRepo
            .Setup(r => r.FindAsync(It.IsAny<Func<Opportunity, bool>>()))
            .ReturnsAsync(opps);

        var result = await _service.GetOpportunitiesByAccountAsync(5);

        result.Should().HaveCount(2);
    }

    // ------------------------------------------------------------------
    // GetOpenOpportunitiesAsync
    // ------------------------------------------------------------------

    [Fact]
    public async Task GetOpenOpportunitiesAsync_ShouldReturnOnlyOpenOpportunities()
    {
        var opps = new List<Opportunity>
        {
            new() { Id = 1, Name = "Open Opp", Stage = OpportunityStage.Proposal, IsDeleted = false }
        };
        _mockRepo
            .Setup(r => r.FindAsync(It.IsAny<Func<Opportunity, bool>>()))
            .ReturnsAsync(opps);

        var result = await _service.GetOpenOpportunitiesAsync();

        result.Should().HaveCount(1);
    }

    // ------------------------------------------------------------------
    // CreateOpportunityAsync
    // ------------------------------------------------------------------

    [Fact]
    public async Task CreateOpportunityAsync_ShouldCallAddAndSave_WhenValidOpportunity()
    {
        var opp = new Opportunity { Name = "New Deal", AccountId = 3 };
        _mockRepo.Setup(r => r.AddAsync(opp)).Returns(Task.CompletedTask);
        _mockRepo.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

        var id = await _service.CreateOpportunityAsync(opp);

        _mockRepo.Verify(r => r.AddAsync(opp), Times.Once);
        _mockRepo.Verify(r => r.SaveAsync(), Times.Once);
    }

    // ------------------------------------------------------------------
    // UpdateOpportunityAsync
    // ------------------------------------------------------------------

    [Fact]
    public async Task UpdateOpportunityAsync_ShouldCallUpdateAndSave()
    {
        var opp = new Opportunity { Id = 7, Name = "Updated Deal" };
        _mockRepo.Setup(r => r.UpdateAsync(opp)).Returns(Task.CompletedTask);
        _mockRepo.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

        await _service.UpdateOpportunityAsync(opp);

        _mockRepo.Verify(r => r.UpdateAsync(opp), Times.Once);
        _mockRepo.Verify(r => r.SaveAsync(), Times.Once);
    }

    // ------------------------------------------------------------------
    // DeleteOpportunityAsync
    // ------------------------------------------------------------------

    [Fact]
    public async Task DeleteOpportunityAsync_ShouldCallDeleteAndSave_WhenOpportunityExists()
    {
        var opp = new Opportunity { Id = 8, Name = "Delete Me" };
        _mockRepo.Setup(r => r.GetByIdAsync(8)).ReturnsAsync(opp);
        _mockRepo.Setup(r => r.DeleteAsync(opp)).Returns(Task.CompletedTask);
        _mockRepo.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

        await _service.DeleteOpportunityAsync(8);

        _mockRepo.Verify(r => r.DeleteAsync(opp), Times.Once);
        _mockRepo.Verify(r => r.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteOpportunityAsync_ShouldDoNothing_WhenOpportunityNotFound()
    {
        _mockRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Opportunity?)null);

        await _service.DeleteOpportunityAsync(999);

        _mockRepo.Verify(r => r.DeleteAsync(It.IsAny<Opportunity>()), Times.Never);
    }

    // ------------------------------------------------------------------
    // New field flow-through (OpportunityDto gap remediation)
    // ------------------------------------------------------------------

    [Fact]
    public async Task GetOpportunityByIdAsync_ShouldReturnForecastAndWinLossFields_WhenSetOnEntity()
    {
        var opp = new Opportunity
        {
            Id = 21,
            Name = "Closed Deal",
            AccountId = 10,
            ForecastCategory = ForecastCategory.ClosedWon,
            LossReasonCategory = LossReasonCategory.Price,
            LossReason = "Client chose a cheaper vendor.",
            CompetitorWinnerId = 33,
            WinLossNotes = "Reassess pricing tier for renewal.",
            ClosedDate = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc)
        };
        _mockRepo.Setup(r => r.GetByIdAsync(21)).ReturnsAsync(opp);

        var result = await _service.GetOpportunityByIdAsync(21);

        result.Should().NotBeNull();
        result!.ForecastCategory.Should().Be(ForecastCategory.ClosedWon);
        result.LossReasonCategory.Should().Be(LossReasonCategory.Price);
        result.LossReason.Should().Be("Client chose a cheaper vendor.");
        result.CompetitorWinnerId.Should().Be(33);
        result.WinLossNotes.Should().Be("Reassess pricing tier for renewal.");
        result.ClosedDate.Should().Be(new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public async Task CreateOpportunityAsync_ShouldPersistForecastCategory_WhenSet()
    {
        var opp = new Opportunity { Name = "New Forecast Deal", AccountId = 3, ForecastCategory = ForecastCategory.BestCase };
        _mockRepo.Setup(r => r.AddAsync(opp)).Returns(Task.CompletedTask);
        _mockRepo.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

        await _service.CreateOpportunityAsync(opp);

        opp.ForecastCategory.Should().Be(ForecastCategory.BestCase);
        _mockRepo.Verify(r => r.AddAsync(opp), Times.Once);
    }
}
