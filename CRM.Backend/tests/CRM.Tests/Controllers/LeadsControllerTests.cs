// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Api.Controllers;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for LeadsController.
/// Tests lead management endpoints including CRUD, conversion, and pagination.
/// </summary>
public class LeadsControllerTests
{
    private readonly Mock<ILeadService> _mockLeadService;
    private readonly Mock<ILeadAgingAlertService> _mockLeadAgingAlertService;
    private readonly Mock<ILeadQualificationService> _mockLeadQualificationService;
    private readonly LeadsController _controller;

    public LeadsControllerTests()
    {
        _mockLeadService = new Mock<ILeadService>();
        _mockLeadAgingAlertService = new Mock<ILeadAgingAlertService>();
        _mockLeadQualificationService = new Mock<ILeadQualificationService>();
        _controller = new LeadsController(_mockLeadService.Object, _mockLeadAgingAlertService.Object, _mockLeadQualificationService.Object);
    }

    private static LeadSummaryDto CreateLeadSummary(int id = 1) => new()
    {
        Id = id,
        FirstName = "Jane",
        LastName = "Smith",
        Email = $"jane.smith{id}@example.com",
        Status = "New",
        Source = "Web"
    };

    private static LeadDto CreateLeadDto(int id = 1) => new()
    {
        Id = id,
        FirstName = "Jane",
        LastName = "Smith",
        Email = $"jane.smith{id}@example.com",
        Status = "New",
        Source = "Web"
    };

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WithDefaultPagination_ReturnsOkWithPaginatedResult()
    {
        // Arrange
        var items = new List<LeadSummaryDto>
        {
            CreateLeadSummary(1),
            CreateLeadSummary(2)
        };
        _mockLeadService
            .Setup(s => s.GetAllAsync(1, 25))
            .ReturnsAsync((items, 2, 1, 25, 1));

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAll_WithCustomPageSize_PassesPaginationToService()
    {
        // Arrange
        _mockLeadService
            .Setup(s => s.GetAllAsync(2, 10))
            .ReturnsAsync((new List<LeadSummaryDto>(), 0, 2, 10, 0));

        // Act
        var result = await _controller.GetAll(page: 2, pageSize: 10);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mockLeadService.Verify(s => s.GetAllAsync(2, 10), Times.Once);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithValidId_ReturnsOkWithLeadDto()
    {
        // Arrange
        var leadDto = CreateLeadDto(1);
        _mockLeadService
            .Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(leadDto);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = okResult.Value.Should().BeOfType<LeadDto>().Subject;
        dto.Id.Should().Be(1);
        dto.Email.Should().Be("jane.smith1@example.com");
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        _mockLeadService
            .Setup(s => s.GetByIdAsync(999))
            .ReturnsAsync((LeadDto?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WithValidRequest_ReturnsCreatedAtAction()
    {
        // Arrange
        var request = new CreateLeadDto
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com",
            Company = "Acme Corp",
            Source = "Web"
        };
        _mockLeadService
            .Setup(s => s.CreateAsync(It.IsAny<Lead>()))
            .ReturnsAsync(42);

        // Act
        var result = await _controller.Create(request);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(LeadsController.GetById));
        createdResult.StatusCode.Should().Be(201);
        createdResult.RouteValues!["id"].Should().Be(42);
    }

    [Fact]
    public async Task Create_VerifiesServiceCalledWithLeadData()
    {
        // Arrange
        var request = new CreateLeadDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com"
        };
        _mockLeadService
            .Setup(s => s.CreateAsync(It.Is<Lead>(l => l.FirstName == "John" && l.LastName == "Doe")))
            .ReturnsAsync(1);

        // Act
        await _controller.Create(request);

        // Assert
        _mockLeadService.Verify(
            s => s.CreateAsync(It.Is<Lead>(l => l.FirstName == "John" && l.LastName == "Doe")),
            Times.Once);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WithValidId_ReturnsOk()
    {
        // Arrange
        _mockLeadService
            .Setup(s => s.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Delete_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        _mockLeadService
            .Setup(s => s.DeleteAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region Convert Tests

    [Fact]
    public async Task Convert_WithValidId_ReturnsOkWithOpportunityInfo()
    {
        // Arrange
        var request = new ConvertLeadDto
        {
            OpportunityName = "New Opportunity",
            AccountId = 10,
            EstimatedValue = 5000m
        };
        _mockLeadService
            .Setup(s => s.ConvertAsync(1, "New Opportunity", 10, 5000m, null))
            .ReturnsAsync((99, 1));

        // Act
        var result = await _controller.Convert(1, request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Convert_WhenServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var request = new ConvertLeadDto { OpportunityName = "Test" };
        _mockLeadService
            .Setup(s => s.ConvertAsync(It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<decimal?>(), It.IsAny<DateTime?>()))
            .ThrowsAsync(new InvalidOperationException("Lead not found"));

        // Act
        var result = await _controller.Convert(999, request);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithValidId_ReturnsOk()
    {
        // Arrange
        var request = new UpdateLeadDto { FirstName = "Updated" };
        _mockLeadService
            .Setup(s => s.UpdateAsync(1, It.IsAny<Action<Lead>>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Update(1, request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Update_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var request = new UpdateLeadDto { FirstName = "Updated" };
        _mockLeadService
            .Setup(s => s.UpdateAsync(999, It.IsAny<Action<Lead>>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Update(999, request);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Update_AppliesFieldChanges_ViaServiceCallback()
    {
        // Arrange
        Lead? capturedLead = null;
        var request = new UpdateLeadDto { FirstName = "Zed", Score = 88 };
        _mockLeadService
            .Setup(s => s.UpdateAsync(1, It.IsAny<Action<Lead>>()))
            .Callback<int, Action<Lead>>((id, apply) =>
            {
                capturedLead = new Lead { FirstName = "Old", LastName = "Name" };
                apply(capturedLead);
            })
            .ReturnsAsync(true);

        // Act
        await _controller.Update(1, request);

        // Assert
        capturedLead.Should().NotBeNull();
        capturedLead!.FirstName.Should().Be("Zed");
        capturedLead.Score.Should().Be(88);
    }

    #endregion

    #region CheckDuplicate Tests

    [Fact]
    public async Task CheckDuplicate_WithEmail_ReturnsOkWithResult()
    {
        // Arrange
        _mockLeadService
            .Setup(s => s.CheckDuplicateAsync("jane@example.com", null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, (int?)5, "email"));

        // Act
        var result = await _controller.CheckDuplicate(email: "jane@example.com");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<CheckDuplicateLeadResponse>().Subject;
        response.IsDuplicate.Should().BeTrue();
        response.ExistingLeadId.Should().Be(5);
        response.MatchedOn.Should().Be("email");
    }

    [Fact]
    public async Task CheckDuplicate_WithFirstAndLastName_ReturnsOkWithResult()
    {
        // Arrange
        _mockLeadService
            .Setup(s => s.CheckDuplicateAsync(null, "Jane", "Smith", "Acme", It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, (int?)null, (string?)null));

        // Act
        var result = await _controller.CheckDuplicate(firstName: "Jane", lastName: "Smith", company: "Acme");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<CheckDuplicateLeadResponse>().Subject;
        response.IsDuplicate.Should().BeFalse();
    }

    [Fact]
    public async Task CheckDuplicate_WithNoEmailOrFullName_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.CheckDuplicate(firstName: "Jane");

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        _mockLeadService.Verify(
            s => s.CheckDuplicateAsync(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region GetByStatus Tests

    [Fact]
    public async Task GetByStatus_WithValidStatus_ReturnsOkWithLeads()
    {
        // Arrange
        var leads = new List<LeadSummaryDto> { CreateLeadSummary(1) };
        _mockLeadService
            .Setup(s => s.GetByStatusAsync(LeadLifecycleStatus.Qualified))
            .ReturnsAsync(leads);

        // Act
        var result = await _controller.GetByStatus("Qualified");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeSameAs(leads);
    }

    [Fact]
    public async Task GetByStatus_WithInvalidStatus_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.GetByStatus("NotAStatus");

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region GetStats Tests

    [Fact]
    public async Task GetStats_ReturnsOkWithStats()
    {
        // Arrange
        var stats = new { total = 10, qualified = 3 };
        _mockLeadService.Setup(s => s.GetStatsAsync()).ReturnsAsync(stats);

        // Act
        var result = await _controller.GetStats();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeSameAs(stats);
    }

    #endregion

    #region GetSourceAnalytics Tests

    [Fact]
    public async Task GetSourceAnalytics_ReturnsOkWithAnalytics()
    {
        // Arrange
        var analytics = new List<LeadSourceAnalyticsDto>
        {
            new() { Source = "Web", TotalLeads = 10, ConvertedLeads = 2, ConversionRate = 20m }
        };
        _mockLeadService
            .Setup(s => s.GetSourceAnalyticsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(analytics);

        // Act
        var result = await _controller.GetSourceAnalytics();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeSameAs(analytics);
    }

    #endregion

    #region GetAttributionAnalytics Tests

    [Fact]
    public async Task GetAttributionAnalytics_ReturnsOkWithAnalytics()
    {
        // Arrange
        var analytics = new List<LeadAttributionDto>
        {
            new() { UtmSource = "google", UtmMedium = "cpc", TotalLeads = 5 }
        };
        _mockLeadService
            .Setup(s => s.GetAttributionAnalyticsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(analytics);

        // Act
        var result = await _controller.GetAttributionAnalytics();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeSameAs(analytics);
    }

    #endregion

    #region GetAgingAlerts Tests

    [Fact]
    public async Task GetAgingAlerts_WithDefaultStaleDays_ReturnsOkWithAlerts()
    {
        // Arrange
        var alerts = new List<LeadAgingAlertDto>
        {
            new() { LeadId = 1, LeadName = "Jane Smith", DaysSinceLastActivity = 20, StalenessLevel = "Warning" }
        };
        _mockLeadAgingAlertService
            .Setup(s => s.GetStaledLeadsAsync(14, It.IsAny<CancellationToken>()))
            .ReturnsAsync(alerts);

        // Act
        var result = await _controller.GetAgingAlerts();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeSameAs(alerts);
    }

    [Fact]
    public async Task GetAgingAlerts_WithCustomStaleDays_PassesValueToService()
    {
        // Arrange
        _mockLeadAgingAlertService
            .Setup(s => s.GetStaledLeadsAsync(30, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LeadAgingAlertDto>());

        // Act
        var result = await _controller.GetAgingAlerts(staleDays: 30);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mockLeadAgingAlertService.Verify(s => s.GetStaledLeadsAsync(30, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region AssignNurtureCampaign Tests

    [Fact]
    public async Task AssignNurtureCampaign_WithValidIds_ReturnsOk()
    {
        // Arrange
        var request = new AssignNurtureCampaignDto { CampaignId = 7 };
        _mockLeadService
            .Setup(s => s.AssignToNurtureCampaignAsync(1, 7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.AssignNurtureCampaign(1, request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task AssignNurtureCampaign_WithNonExistentLeadOrCampaign_ReturnsNotFound()
    {
        // Arrange
        var request = new AssignNurtureCampaignDto { CampaignId = 999 };
        _mockLeadService
            .Setup(s => s.AssignToNurtureCampaignAsync(999, 999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.AssignNurtureCampaign(999, request);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region GetNurtureCampaigns Tests

    [Fact]
    public async Task GetNurtureCampaigns_WhenLeadEnrolled_ReturnsOkWithCampaign()
    {
        // Arrange
        var leadDto = CreateLeadDto(1);
        var campaign = new MarketingCampaign { Id = 7, Name = "Welcome Series" };
        _mockLeadService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(leadDto);
        _mockLeadService
            .Setup(s => s.GetNurtureCampaignAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);

        // Act
        var result = await _controller.GetNurtureCampaigns(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetNurtureCampaigns_WhenLeadNotEnrolled_ReturnsOkWithEmptyArray()
    {
        // Arrange
        var leadDto = CreateLeadDto(1);
        _mockLeadService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(leadDto);
        _mockLeadService
            .Setup(s => s.GetNurtureCampaignAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MarketingCampaign?)null);

        // Act
        var result = await _controller.GetNurtureCampaigns(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var campaigns = okResult.Value.Should().BeAssignableTo<System.Collections.IEnumerable>().Subject;
        campaigns.Cast<object>().Should().BeEmpty();
    }

    [Fact]
    public async Task GetNurtureCampaigns_WithNonExistentLead_ReturnsNotFound()
    {
        // Arrange
        _mockLeadService.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((LeadDto?)null);

        // Act
        var result = await _controller.GetNurtureCampaigns(999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region RemoveFromNurtureCampaign Tests

    [Fact]
    public async Task RemoveFromNurtureCampaign_WithValidIds_ReturnsOk()
    {
        // Arrange
        _mockLeadService
            .Setup(s => s.RemoveFromNurtureCampaignAsync(1, 7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.RemoveFromNurtureCampaign(1, 7);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task RemoveFromNurtureCampaign_WhenNotEnrolled_ReturnsNotFound()
    {
        // Arrange
        _mockLeadService
            .Setup(s => s.RemoveFromNurtureCampaignAsync(1, 7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.RemoveFromNurtureCampaign(1, 7);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region QualifyLead Tests

    [Fact]
    public async Task QualifyLead_WithBantFields_UsesBantFrameworkAndReturnsOk()
    {
        // Arrange
        var dto = new LeadQualificationDto
        {
            HasBudget = true,
            HasAuthority = true,
            HasNeed = false,
            HasTimeline = true
        };
        var bantResult = new LeadQualificationResult
        {
            LeadId = 1,
            Framework = QualificationFramework.BANT,
            CombinedScore = 75,
            QualificationLevel = "SQL"
        };
        _mockLeadQualificationService
            .Setup(s => s.ScoreWithBANTAsync(1, 100, 100, 0, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bantResult);

        // Act
        var result = await _controller.QualifyLead(1, dto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var value = okResult.Value.Should().BeOfType<LeadQualificationResult>().Subject;
        value.Framework.Should().Be(QualificationFramework.BANT);
        value.CombinedScore.Should().Be(75);
    }

    [Fact]
    public async Task QualifyLead_WithMeddicFields_UsesMeddicFrameworkAndReturnsOk()
    {
        // Arrange
        var dto = new LeadQualificationDto
        {
            Metrics = "20% cost reduction",
            EconomicBuyer = "CFO",
            Champion = "VP Eng"
        };
        var meddicResult = new LeadQualificationResult
        {
            LeadId = 1,
            Framework = QualificationFramework.MEDDIC,
            CombinedScore = 50,
            QualificationLevel = "MQL"
        };
        _mockLeadQualificationService
            .Setup(s => s.ScoreWithMEDDICAsync(1, It.Is<MEDDICScores>(m =>
                m.MetricsScore == 100 &&
                m.EconomicBuyerScore == 100 &&
                m.DecisionCriteriaScore == 0 &&
                m.DecisionProcessScore == 0 &&
                m.IdentifyPainScore == 0 &&
                m.ChampionScore == 100), It.IsAny<CancellationToken>()))
            .ReturnsAsync(meddicResult);

        // Act
        var result = await _controller.QualifyLead(1, dto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var value = okResult.Value.Should().BeOfType<LeadQualificationResult>().Subject;
        value.Framework.Should().Be(QualificationFramework.MEDDIC);
    }

    [Fact]
    public async Task QualifyLead_WithNonExistentLead_ReturnsNotFound()
    {
        // Arrange
        var dto = new LeadQualificationDto { HasBudget = true };
        _mockLeadQualificationService
            .Setup(s => s.ScoreWithBANTAsync(999, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException());

        // Act
        var result = await _controller.QualifyLead(999, dto);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion
}
