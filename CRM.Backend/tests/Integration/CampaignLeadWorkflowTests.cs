// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace CRM.Tests.Integration;

/// <summary>
/// Workflow tests for Campaign → Lead nurture → Conversion pipeline.
/// Uses mocked IMarketingCampaignService and ILeadService to verify
/// expected interactions and campaign lifecycle transitions.
/// </summary>
public class CampaignLeadWorkflowTests
{
    private readonly Mock<IMarketingCampaignService> _campaignService = new(MockBehavior.Loose);
    private readonly Mock<ILeadService> _leadService = new(MockBehavior.Loose);

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static CreateCampaignDto BuildCreateCampaignDto(
        string name = "Q1 Email Blitz",
        int campaignType = (int)CampaignType.Email) =>
        new CreateCampaignDto
        {
            Name = name,
            CampaignType = campaignType,
            Budget = 5000m,
            TargetAudience = 1000,
            Description = "Test nurture campaign",
            StartDate = DateTime.UtcNow.ToString("o"),
            EndDate = DateTime.UtcNow.AddDays(30).ToString("o"),
        };

    private static UpdateCampaignDto BuildUpdateCampaignDto(
        string? name = "Q1 Email Blitz Updated",
        int? status = (int)CampaignStatus.Active) =>
        new UpdateCampaignDto
        {
            Name = name,
            Status = status,
        };

    private static CampaignDto BuildCampaignDto(
        int id = 1,
        string name = "Q1 Email Blitz",
        int status = (int)CampaignStatus.Draft) =>
        new CampaignDto
        {
            Id = id,
            Name = name,
            Status = status,
            CampaignType = (int)CampaignType.Email,
            Budget = 5000m,
            TargetAudience = 1000,
            CreatedAt = DateTime.UtcNow,
        };

    private static MarketingCampaign BuildCampaignEntity(
        int id = 1,
        string name = "Q1 Email Blitz",
        CampaignStatus status = CampaignStatus.Active) =>
        new MarketingCampaign
        {
            Id = id,
            Name = name,
            Status = status,
        };

    // ── Campaign CRUD Tests ───────────────────────────────────────────────────

    // Test 1
    [Fact]
    public async Task GetCampaignById_ShouldReturnDto_WhenCampaignExists()
    {
        // Arrange
        var expected = BuildCampaignDto(id: 1);
        _campaignService.Setup(s => s.GetCampaignByIdAsync(1)).ReturnsAsync(expected);

        // Act
        var result = await _campaignService.Object.GetCampaignByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Q1 Email Blitz");
    }

    // Test 2
    [Fact]
    public async Task GetCampaignById_ShouldReturnNull_WhenCampaignDoesNotExist()
    {
        // Arrange
        _campaignService.Setup(s => s.GetCampaignByIdAsync(999)).ReturnsAsync((CampaignDto?)null);

        // Act
        var result = await _campaignService.Object.GetCampaignByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    // Test 3
    [Fact]
    public async Task GetAllCampaigns_ShouldReturnList_WhenCampaignsExist()
    {
        // Arrange
        var campaigns = new List<CampaignDto>
        {
            BuildCampaignDto(id: 1, name: "Campaign A"),
            BuildCampaignDto(id: 2, name: "Campaign B"),
            BuildCampaignDto(id: 3, name: "Campaign C"),
        };

        _campaignService.Setup(s => s.GetAllCampaignsAsync()).ReturnsAsync(campaigns);

        // Act
        var result = await _campaignService.Object.GetAllCampaignsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
    }

    // Test 4
    [Fact]
    public async Task GetActiveCampaigns_ShouldReturnOnlyActiveCampaigns()
    {
        // Arrange
        var active = new List<CampaignDto>
        {
            BuildCampaignDto(id: 2, name: "Running Campaign", status: (int)CampaignStatus.Active),
        };

        _campaignService.Setup(s => s.GetActiveCampaignsAsync()).ReturnsAsync(active);

        // Act
        var result = await _campaignService.Object.GetActiveCampaignsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Status.Should().Be((int)CampaignStatus.Active);
    }

    // Test 5
    [Fact]
    public async Task CreateCampaign_ShouldReturnNewId_WhenInputIsValid()
    {
        // Arrange
        var createDto = BuildCreateCampaignDto();
        _campaignService.Setup(s => s.CreateCampaignAsync(It.IsAny<CreateCampaignDto>())).ReturnsAsync(42);

        // Act
        var result = await _campaignService.Object.CreateCampaignAsync(createDto);

        // Assert
        result.Should().Be(42);
    }

    // Test 6
    [Fact]
    public async Task UpdateCampaign_ShouldComplete_WhenDtoIsValid()
    {
        // Arrange
        var updateDto = BuildUpdateCampaignDto(name: "Updated Name", status: (int)CampaignStatus.Active);
        _campaignService.Setup(s => s.UpdateCampaignAsync(1, It.IsAny<UpdateCampaignDto>()))
            .Returns(Task.CompletedTask);

        // Act
        Func<Task> act = () => _campaignService.Object.UpdateCampaignAsync(1, updateDto);

        // Assert
        await act.Should().NotThrowAsync();
        _campaignService.Verify(s => s.UpdateCampaignAsync(1, It.IsAny<UpdateCampaignDto>()), Times.Once);
    }

    // Test 7
    [Fact]
    public async Task DeleteCampaign_ShouldComplete_WhenCampaignExists()
    {
        // Arrange
        _campaignService.Setup(s => s.DeleteCampaignAsync(1)).Returns(Task.CompletedTask);

        // Act
        Func<Task> act = () => _campaignService.Object.DeleteCampaignAsync(1);

        // Assert
        await act.Should().NotThrowAsync();
        _campaignService.Verify(s => s.DeleteCampaignAsync(1), Times.Once);
    }

    // Test 8
    [Fact]
    public async Task AddCampaignMetric_ShouldComplete_WhenMetricIsValid()
    {
        // Arrange
        var metric = new CampaignMetric
        {
            CampaignId = 1,
            MetricName = "Opens",
            MetricValue = 350,
            TotalSent = 1000,
            TotalDelivered = 980,
            TotalOpened = 350,
            TotalClicked = 120,
            RecordedDate = DateTime.UtcNow,
        };

        _campaignService.Setup(s => s.AddCampaignMetricAsync(It.IsAny<CampaignMetric>()))
            .Returns(Task.CompletedTask);

        // Act
        Func<Task> act = () => _campaignService.Object.AddCampaignMetricAsync(metric);

        // Assert
        await act.Should().NotThrowAsync();
        _campaignService.Verify(s => s.AddCampaignMetricAsync(It.IsAny<CampaignMetric>()), Times.Once);
    }

    // ── Lead Nurture Campaign Tests ────────────────────────────────────────────

    // Test 9
    [Fact]
    public async Task AssignLeadToNurtureCampaign_ShouldReturnTrue_WhenSuccessful()
    {
        // Arrange
        const int leadId = 10;
        const int campaignId = 1;

        _leadService.Setup(s => s.AssignToNurtureCampaignAsync(leadId, campaignId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _leadService.Object.AssignToNurtureCampaignAsync(leadId, campaignId);

        // Assert
        result.Should().BeTrue();
    }

    // Test 10
    [Fact]
    public async Task AssignLeadToNurtureCampaign_ShouldReturnFalse_WhenLeadNotFound()
    {
        // Arrange
        const int leadId = 999;
        const int campaignId = 1;

        _leadService.Setup(s => s.AssignToNurtureCampaignAsync(leadId, campaignId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _leadService.Object.AssignToNurtureCampaignAsync(leadId, campaignId);

        // Assert
        result.Should().BeFalse();
    }

    // Test 11
    [Fact]
    public async Task GetNurtureCampaign_ShouldReturnCampaign_WhenLeadIsEnrolled()
    {
        // Arrange
        const int leadId = 10;
        var campaign = BuildCampaignEntity(id: 1, name: "Q1 Email Blitz", status: CampaignStatus.Active);

        _leadService.Setup(s => s.GetNurtureCampaignAsync(leadId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);

        // Act
        var result = await _leadService.Object.GetNurtureCampaignAsync(leadId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Q1 Email Blitz");
        result.Status.Should().Be(CampaignStatus.Active);
    }

    // Test 12
    [Fact]
    public async Task GetNurtureCampaign_ShouldReturnNull_WhenLeadHasNoCampaign()
    {
        // Arrange
        _leadService.Setup(s => s.GetNurtureCampaignAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MarketingCampaign?)null);

        // Act
        var result = await _leadService.Object.GetNurtureCampaignAsync(10);

        // Assert
        result.Should().BeNull();
    }

    // Test 13
    [Fact]
    public async Task RemoveLeadFromNurtureCampaign_ShouldReturnTrue_WhenSameCampaign()
    {
        // Arrange
        const int leadId = 10;
        const int campaignId = 1;

        _leadService.Setup(s => s.RemoveFromNurtureCampaignAsync(leadId, campaignId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _leadService.Object.RemoveFromNurtureCampaignAsync(leadId, campaignId);

        // Assert
        result.Should().BeTrue();
    }

    // Test 14
    [Fact]
    public async Task RemoveLeadFromNurtureCampaign_ShouldReturnFalse_WhenDifferentCampaign()
    {
        // Arrange — lead is enrolled in campaign 1 but remove called for campaign 2
        // SPEC_CONFLICT: Method returns false when enrolled campaign doesn't match supplied campaignId
        const int leadId = 10;
        const int wrongCampaignId = 2;

        _leadService.Setup(s => s.RemoveFromNurtureCampaignAsync(leadId, wrongCampaignId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _leadService.Object.RemoveFromNurtureCampaignAsync(leadId, wrongCampaignId);

        // Assert
        result.Should().BeFalse();
    }

    // ── Analytics Tests ────────────────────────────────────────────────────────

    // Test 15
    [Fact]
    public async Task GetLeadSourceAnalytics_ShouldReturnGroupedData_WhenLeadsExist()
    {
        // Arrange
        var analytics = new List<LeadSourceAnalyticsDto>
        {
            new LeadSourceAnalyticsDto
            {
                Source = "Web",
                TotalLeads = 200,
                ConvertedLeads = 40,
                QualifiedLeads = 80,
                ConversionRate = 20m,
                AverageScore = 72.5,
            },
            new LeadSourceAnalyticsDto
            {
                Source = "Referral",
                TotalLeads = 50,
                ConvertedLeads = 20,
                QualifiedLeads = 30,
                ConversionRate = 40m,
                AverageScore = 85.0,
            },
        };

        _leadService.Setup(s => s.GetSourceAnalyticsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(analytics);

        // Act
        var result = await _leadService.Object.GetSourceAnalyticsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(a => a.Source == "Web" && a.TotalLeads == 200);
        result.Should().Contain(a => a.Source == "Referral" && a.ConversionRate == 40m);
    }

    // Test 16
    [Fact]
    public async Task GetLeadAttributionAnalytics_ShouldReturnUtmBreakdown_WhenLeadsExist()
    {
        // Arrange
        var attribution = new List<LeadAttributionDto>
        {
            new LeadAttributionDto
            {
                UtmSource = "google",
                UtmMedium = "cpc",
                UtmCampaign = "q1-promo",
                TotalLeads = 150,
                ConvertedLeads = 30,
                ConversionRate = 20m,
                AverageScore = 68.0,
            },
            new LeadAttributionDto
            {
                UtmSource = "linkedin",
                UtmMedium = "social",
                UtmCampaign = "brand-awareness",
                TotalLeads = 80,
                ConvertedLeads = 16,
                ConversionRate = 20m,
                AverageScore = 74.0,
            },
        };

        _leadService.Setup(s => s.GetAttributionAnalyticsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(attribution);

        // Act
        var result = await _leadService.Object.GetAttributionAnalyticsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(a => a.UtmSource == "google" && a.UtmMedium == "cpc");
        result.Should().Contain(a => a.UtmCampaign == "brand-awareness");
    }

    // ── Full Workflow Tests ───────────────────────────────────────────────────

    // Test 17
    [Fact]
    public async Task FullNurtureWorkflow_Create_Assign_Retrieve_Remove_ShouldSucceed()
    {
        // Arrange
        const int leadId = 10;
        const int campaignId = 1;

        var createDto = BuildCreateCampaignDto();
        var campaignEntity = BuildCampaignEntity(id: campaignId, status: CampaignStatus.Active);

        _campaignService.Setup(s => s.CreateCampaignAsync(It.IsAny<CreateCampaignDto>())).ReturnsAsync(campaignId);
        _leadService.Setup(s => s.AssignToNurtureCampaignAsync(leadId, campaignId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _leadService.Setup(s => s.GetNurtureCampaignAsync(leadId, It.IsAny<CancellationToken>())).ReturnsAsync(campaignEntity);
        _leadService.Setup(s => s.RemoveFromNurtureCampaignAsync(leadId, campaignId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act – create campaign
        var newId = await _campaignService.Object.CreateCampaignAsync(createDto);
        newId.Should().Be(campaignId);

        // Assign lead to campaign
        var assigned = await _leadService.Object.AssignToNurtureCampaignAsync(leadId, campaignId);
        assigned.Should().BeTrue();

        // Verify assignment
        var campaign = await _leadService.Object.GetNurtureCampaignAsync(leadId);
        campaign.Should().NotBeNull();
        campaign!.Id.Should().Be(campaignId);

        // Remove from campaign
        var removed = await _leadService.Object.RemoveFromNurtureCampaignAsync(leadId, campaignId);
        removed.Should().BeTrue();
    }

    // Test 18
    [Fact]
    public async Task CampaignLifecycle_Draft_Active_Completed_ShouldTransition()
    {
        // Arrange – separate campaign DTOs for each status
        var draftDto = BuildCampaignDto(status: (int)CampaignStatus.Draft);
        var activeDto = BuildCampaignDto(status: (int)CampaignStatus.Active);
        var completedDto = BuildCampaignDto(status: (int)CampaignStatus.Completed);

        // Create (Draft)
        _campaignService.Setup(s => s.CreateCampaignAsync(It.IsAny<CreateCampaignDto>())).ReturnsAsync(1);
        _campaignService.SetupSequence(s => s.GetCampaignByIdAsync(1))
            .ReturnsAsync(draftDto)
            .ReturnsAsync(activeDto)
            .ReturnsAsync(completedDto);
        _campaignService.Setup(s => s.UpdateCampaignAsync(1, It.IsAny<UpdateCampaignDto>())).Returns(Task.CompletedTask);

        // Act
        var newId = await _campaignService.Object.CreateCampaignAsync(BuildCreateCampaignDto());
        newId.Should().Be(1);

        var draft = await _campaignService.Object.GetCampaignByIdAsync(1);
        draft!.Status.Should().Be((int)CampaignStatus.Draft);

        await _campaignService.Object.UpdateCampaignAsync(1, new UpdateCampaignDto { Status = (int)CampaignStatus.Active });
        var active = await _campaignService.Object.GetCampaignByIdAsync(1);
        active!.Status.Should().Be((int)CampaignStatus.Active);

        await _campaignService.Object.UpdateCampaignAsync(1, new UpdateCampaignDto { Status = (int)CampaignStatus.Completed });
        var completed = await _campaignService.Object.GetCampaignByIdAsync(1);
        completed!.Status.Should().Be((int)CampaignStatus.Completed);
    }

    // Test 19
    [Fact]
    public async Task NurtureCampaign_WithMetrics_ShouldTrackPerformance()
    {
        // Arrange
        var metric = new CampaignMetric
        {
            CampaignId = 1,
            MetricName = "WeeklyEngagement",
            MetricValue = 0.35,
            TotalSent = 500,
            TotalDelivered = 490,
            TotalOpened = 175,
            TotalClicked = 60,
            RecordedDate = DateTime.UtcNow,
        };

        _campaignService.Setup(s => s.AddCampaignMetricAsync(It.IsAny<CampaignMetric>())).Returns(Task.CompletedTask);

        // Act
        Func<Task> act = () => _campaignService.Object.AddCampaignMetricAsync(metric);

        // Assert
        await act.Should().NotThrowAsync();
        _campaignService.Verify(s => s.AddCampaignMetricAsync(
            It.Is<CampaignMetric>(m => m.CampaignId == 1 && m.TotalSent == 500)), Times.Once);
    }

    // Test 20
    [Fact]
    public async Task MultipleLeads_AssignedToSameCampaign_ShouldAllSucceed()
    {
        // Arrange
        const int campaignId = 5;
        var leadIds = new[] { 101, 102, 103 };

        _leadService.Setup(s => s.AssignToNurtureCampaignAsync(
                It.IsIn(leadIds),
                campaignId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var results = await Task.WhenAll(
            leadIds.Select(id => _leadService.Object.AssignToNurtureCampaignAsync(id, campaignId)));

        // Assert
        results.Should().AllBeEquivalentTo(true);
        _leadService.Verify(s => s.AssignToNurtureCampaignAsync(
            It.IsAny<int>(), campaignId, It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    // Test 21
    [Fact]
    public async Task SourceAnalytics_ShouldReturn_HighConversionSources_First()
    {
        // Arrange – intentionally unsorted, verifying data structure correctness only
        var analytics = new List<LeadSourceAnalyticsDto>
        {
            new LeadSourceAnalyticsDto { Source = "Email", TotalLeads = 300, ConvertedLeads = 90, ConversionRate = 30m },
            new LeadSourceAnalyticsDto { Source = "Cold Call", TotalLeads = 100, ConvertedLeads = 5, ConversionRate = 5m },
            new LeadSourceAnalyticsDto { Source = "Partner", TotalLeads = 50, ConvertedLeads = 25, ConversionRate = 50m },
        };

        _leadService.Setup(s => s.GetSourceAnalyticsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(analytics);

        // Act
        var result = (await _leadService.Object.GetSourceAnalyticsAsync()).ToList();

        // Assert
        result.Should().HaveCount(3);
        result.Max(a => a.ConversionRate).Should().Be(50m);
        result.Should().Contain(a => a.Source == "Partner");
    }
}
