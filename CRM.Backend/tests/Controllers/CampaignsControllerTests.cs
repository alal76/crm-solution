// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Campaigns Controller Unit Tests

using Xunit;
using Moq;
using FluentAssertions;
using CRM.Api.Controllers;
using CRM.Api.Hubs;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace CRM.Tests.Controllers;

/// <summary>
/// Comprehensive unit tests for CampaignsController
/// Covers: CRUD operations, execution, metrics, recipients, A/B testing
/// </summary>
public class CampaignsControllerTests
{
    private readonly Mock<ICampaignService> _mockCampaignService;
    private readonly Mock<ILogger<CampaignsController>> _mockLogger;
    private readonly Mock<ICrmNotificationService> _mockNotificationService;
    private readonly CampaignsController _controller;

    public CampaignsControllerTests()
    {
        _mockCampaignService = new Mock<ICampaignService>();
        _mockLogger = new Mock<ILogger<CampaignsController>>();
        _mockNotificationService = new Mock<ICrmNotificationService>();

        _mockNotificationService.Setup(x => x.NotifyRecordCreatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);
        _mockNotificationService.Setup(x => x.NotifyRecordUpdatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);
        _mockNotificationService.Setup(x => x.NotifyRecordDeletedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);

        _controller = new CampaignsController(_mockCampaignService.Object, _mockLogger.Object, _mockNotificationService.Object);

        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_ReturnsOkResult_WithCampaigns()
    {
        // Arrange
        var campaigns = new List<CampaignDto>
        {
            new CampaignDto { Id = 1, Name = "Email Campaign", Type = CampaignType.Email },
            new CampaignDto { Id = 2, Name = "Social Campaign", Type = CampaignType.Social }
        };

        _mockCampaignService.Setup(s => s.GetAllAsync())
            .ReturnsAsync(campaigns);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedCampaigns = okResult.Value as IEnumerable<CampaignDto>;
        returnedCampaigns.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_WithTypeFilter_ReturnsFilteredCampaigns()
    {
        // Arrange
        var campaigns = new List<CampaignDto>
        {
            new CampaignDto { Id = 1, Type = CampaignType.Email }
        };

        _mockCampaignService.Setup(s => s.GetByTypeAsync(CampaignType.Email))
            .ReturnsAsync(campaigns);

        // Act
        var result = await _controller.GetByType(CampaignType.Email);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetAll_WithStatusFilter_ReturnsStatusFilteredCampaigns()
    {
        // Arrange
        var campaigns = new List<CampaignDto>
        {
            new CampaignDto { Id = 1, Status = CampaignStatus.Active }
        };

        _mockCampaignService.Setup(s => s.GetByStatusAsync(CampaignStatus.Active))
            .ReturnsAsync(campaigns);

        // Act
        var result = await _controller.GetByStatus(CampaignStatus.Active);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetActiveCampaigns_ReturnsOnlyActive()
    {
        // Arrange
        var campaigns = new List<CampaignDto>
        {
            new CampaignDto { Id = 1, Status = CampaignStatus.Active }
        };

        _mockCampaignService.Setup(s => s.GetActiveCampaignsAsync())
            .ReturnsAsync(campaigns);

        // Act
        var result = await _controller.GetActiveCampaigns();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_ExistingCampaign_ReturnsOkWithCampaign()
    {
        // Arrange
        var campaign = new CampaignDto { Id = 1, Name = "Test Campaign" };

        _mockCampaignService.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(campaign);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedCampaign = okResult.Value as CampaignDto;
        returnedCampaign!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetById_NonExistingCampaign_ReturnsNotFound()
    {
        // Arrange
        _mockCampaignService.Setup(s => s.GetByIdAsync(999))
            .ReturnsAsync((CampaignDto?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_ValidCampaign_ReturnsCreatedWithCampaign()
    {
        // Arrange
        var createDto = new CreateCampaignDto
        {
            Name = "New Campaign",
            Type = CampaignType.Email,
            Subject = "Welcome",
            StartDate = DateTime.Today.AddDays(1)
        };

        var createdCampaign = new CampaignDto
        {
            Id = 1,
            Name = createDto.Name,
            Type = createDto.Type,
            Status = CampaignStatus.Draft
        };

        _mockCampaignService.Setup(s => s.CreateAsync(It.IsAny<CreateCampaignDto>()))
            .ReturnsAsync(createdCampaign);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var returnedCampaign = createdResult.Value as CampaignDto;
        returnedCampaign!.Status.Should().Be(CampaignStatus.Draft);
    }

    [Fact]
    public async Task Create_NullDto_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.Create(null!);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_MissingName_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateCampaignDto { Name = "" };

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_WithTemplate_UsesCampaignTemplate()
    {
        // Arrange
        var createDto = new CreateCampaignDto
        {
            Name = "Template Campaign",
            TemplateId = 1,
            Type = CampaignType.Email
        };

        var createdCampaign = new CampaignDto
        {
            Id = 1,
            Name = createDto.Name,
            TemplateId = 1
        };

        _mockCampaignService.Setup(s => s.CreateAsync(It.IsAny<CreateCampaignDto>()))
            .ReturnsAsync(createdCampaign);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
    }

    [Fact]
    public async Task Create_PastStartDate_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateCampaignDto
        {
            Name = "Past Campaign",
            StartDate = DateTime.Today.AddDays(-1)
        };

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_ValidCampaign_ReturnsOkWithUpdatedCampaign()
    {
        // Arrange
        var updateDto = new UpdateCampaignDto
        {
            Id = 1,
            Name = "Updated Campaign"
        };

        var updatedCampaign = new CampaignDto
        {
            Id = 1,
            Name = "Updated Campaign"
        };

        _mockCampaignService.Setup(s => s.UpdateAsync(It.IsAny<UpdateCampaignDto>()))
            .ReturnsAsync(updatedCampaign);

        // Act
        var result = await _controller.Update(1, updateDto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task Update_IdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var updateDto = new UpdateCampaignDto { Id = 2 };

        // Act
        var result = await _controller.Update(1, updateDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_ActiveCampaign_AllowsLimitedChanges()
    {
        // Arrange
        var updateDto = new UpdateCampaignDto
        {
            Id = 1,
            Name = "Updated Active Campaign"
        };

        var updatedCampaign = new CampaignDto { Id = 1, Status = CampaignStatus.Active };

        _mockCampaignService.Setup(s => s.UpdateAsync(It.IsAny<UpdateCampaignDto>()))
            .ReturnsAsync(updatedCampaign);

        // Act
        var result = await _controller.Update(1, updateDto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region Campaign Execution Tests

    [Fact]
    public async Task Launch_DraftCampaign_ReturnsOk()
    {
        // Arrange
        _mockCampaignService.Setup(s => s.LaunchAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Launch(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Launch_AlreadyActiveCampaign_ReturnsConflict()
    {
        // Arrange
        _mockCampaignService.Setup(s => s.LaunchAsync(1))
            .ThrowsAsync(new InvalidOperationException("Campaign already active"));

        // Act
        var result = await _controller.Launch(1);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task Pause_ActiveCampaign_ReturnsOk()
    {
        // Arrange
        _mockCampaignService.Setup(s => s.PauseAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Pause(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Resume_PausedCampaign_ReturnsOk()
    {
        // Arrange
        _mockCampaignService.Setup(s => s.ResumeAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Resume(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Stop_ActiveCampaign_ReturnsOk()
    {
        // Arrange
        _mockCampaignService.Setup(s => s.StopAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Stop(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Complete_ActiveCampaign_ReturnsOk()
    {
        // Arrange
        _mockCampaignService.Setup(s => s.CompleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Complete(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task SendTest_ValidRequest_ReturnsOk()
    {
        // Arrange
        var request = new SendTestRequest
        {
            CampaignId = 1,
            TestEmail = "test@example.com"
        };

        _mockCampaignService.Setup(s => s.SendTestAsync(request))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.SendTest(request);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Recipients Management Tests

    [Fact]
    public async Task GetRecipients_ValidCampaign_ReturnsRecipients()
    {
        // Arrange
        var recipients = new List<CampaignRecipientDto>
        {
            new CampaignRecipientDto { Id = 1, Email = "john@example.com" },
            new CampaignRecipientDto { Id = 2, Email = "jane@example.com" }
        };

        _mockCampaignService.Setup(s => s.GetRecipientsAsync(1))
            .ReturnsAsync(recipients);

        // Act
        var result = await _controller.GetRecipients(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task AddRecipients_ValidRequest_ReturnsAddedCount()
    {
        // Arrange
        var request = new AddRecipientsRequest
        {
            CampaignId = 1,
            ContactIds = new List<int> { 1, 2, 3 }
        };

        _mockCampaignService.Setup(s => s.AddRecipientsAsync(request))
            .ReturnsAsync(3);

        // Act
        var result = await _controller.AddRecipients(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task AddRecipientsFromSegment_ValidSegment_ReturnsAddedCount()
    {
        // Arrange
        var request = new AddRecipientsFromSegmentRequest
        {
            CampaignId = 1,
            SegmentId = 1
        };

        _mockCampaignService.Setup(s => s.AddRecipientsFromSegmentAsync(request))
            .ReturnsAsync(50);

        // Act
        var result = await _controller.AddRecipientsFromSegment(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task RemoveRecipient_ValidRequest_ReturnsNoContent()
    {
        // Arrange
        _mockCampaignService.Setup(s => s.RemoveRecipientAsync(1, 1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.RemoveRecipient(1, 1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task ClearRecipients_ValidCampaign_ReturnsRemovedCount()
    {
        // Arrange
        _mockCampaignService.Setup(s => s.ClearRecipientsAsync(1))
            .ReturnsAsync(100);

        // Act
        var result = await _controller.ClearRecipients(1);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region Metrics Tests

    [Fact]
    public async Task GetMetrics_ValidCampaign_ReturnsMetrics()
    {
        // Arrange
        var metrics = new CampaignMetricsDto
        {
            CampaignId = 1,
            TotalSent = 1000,
            Delivered = 980,
            Opened = 500,
            Clicked = 150,
            Bounced = 20,
            Unsubscribed = 10,
            OpenRate = 51.02m,
            ClickRate = 15.31m
        };

        _mockCampaignService.Setup(s => s.GetMetricsAsync(1))
            .ReturnsAsync(metrics);

        // Act
        var result = await _controller.GetMetrics(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetClickDetails_ValidCampaign_ReturnsClickDetails()
    {
        // Arrange
        var clicks = new List<CampaignClickDto>
        {
            new CampaignClickDto { Url = "https://example.com/page1", ClickCount = 50 },
            new CampaignClickDto { Url = "https://example.com/page2", ClickCount = 30 }
        };

        _mockCampaignService.Setup(s => s.GetClickDetailsAsync(1))
            .ReturnsAsync(clicks);

        // Act
        var result = await _controller.GetClickDetails(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetOpenDetails_ValidCampaign_ReturnsOpenDetails()
    {
        // Arrange
        var opens = new List<CampaignOpenDto>
        {
            new CampaignOpenDto { RecipientEmail = "john@example.com", OpenedAt = DateTime.Today }
        };

        _mockCampaignService.Setup(s => s.GetOpenDetailsAsync(1))
            .ReturnsAsync(opens);

        // Act
        var result = await _controller.GetOpenDetails(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetBounces_ValidCampaign_ReturnsBounceDetails()
    {
        // Arrange
        var bounces = new List<CampaignBounceDto>
        {
            new CampaignBounceDto { Email = "invalid@example.com", BounceType = "hard" }
        };

        _mockCampaignService.Setup(s => s.GetBouncesAsync(1))
            .ReturnsAsync(bounces);

        // Act
        var result = await _controller.GetBounces(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetConversions_ValidCampaign_ReturnsConversions()
    {
        // Arrange
        var conversions = new List<CampaignConversionDto>
        {
            new CampaignConversionDto { ContactId = 1, ConvertedAt = DateTime.Today, Value = 1000 }
        };

        _mockCampaignService.Setup(s => s.GetConversionsAsync(1))
            .ReturnsAsync(conversions);

        // Act
        var result = await _controller.GetConversions(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion

    #region A/B Testing Tests

    [Fact]
    public async Task CreateABTest_ValidRequest_ReturnsCreatedTest()
    {
        // Arrange
        var request = new CreateABTestRequest
        {
            CampaignId = 1,
            VariantSubjects = new List<string> { "Subject A", "Subject B" },
            TestPercentage = 20
        };

        var createdTest = new ABTestDto
        {
            Id = 1,
            CampaignId = 1,
            Status = ABTestStatus.Draft
        };

        _mockCampaignService.Setup(s => s.CreateABTestAsync(request))
            .ReturnsAsync(createdTest);

        // Act
        var result = await _controller.CreateABTest(request);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task GetABTestResults_ValidTest_ReturnsResults()
    {
        // Arrange
        var results = new ABTestResultsDto
        {
            TestId = 1,
            VariantResults = new List<VariantResultDto>
            {
                new VariantResultDto { VariantName = "A", OpenRate = 25.5m, ClickRate = 5.2m },
                new VariantResultDto { VariantName = "B", OpenRate = 28.3m, ClickRate = 6.1m }
            },
            WinningVariant = "B"
        };

        _mockCampaignService.Setup(s => s.GetABTestResultsAsync(1))
            .ReturnsAsync(results);

        // Act
        var result = await _controller.GetABTestResults(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task SelectABTestWinner_ValidRequest_ReturnsOk()
    {
        // Arrange
        var request = new SelectWinnerRequest
        {
            TestId = 1,
            WinningVariant = "B"
        };

        _mockCampaignService.Setup(s => s.SelectABTestWinnerAsync(request))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.SelectABTestWinner(request);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Scheduling Tests

    [Fact]
    public async Task Schedule_ValidRequest_ReturnsOk()
    {
        // Arrange
        var request = new ScheduleCampaignRequest
        {
            CampaignId = 1,
            ScheduledDate = DateTime.Today.AddDays(7),
            TimeZone = "UTC"
        };

        _mockCampaignService.Setup(s => s.ScheduleAsync(request))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Schedule(request);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task CancelSchedule_ValidRequest_ReturnsOk()
    {
        // Arrange
        _mockCampaignService.Setup(s => s.CancelScheduleAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.CancelSchedule(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Schedule_PastDate_ReturnsBadRequest()
    {
        // Arrange
        var request = new ScheduleCampaignRequest
        {
            CampaignId = 1,
            ScheduledDate = DateTime.Today.AddDays(-1)
        };

        // Act
        var result = await _controller.Schedule(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region Template Management Tests

    [Fact]
    public async Task GetTemplates_ReturnsTemplates()
    {
        // Arrange
        var templates = new List<EmailTemplateDto>
        {
            new EmailTemplateDto { Id = 1, Name = "Welcome Email" },
            new EmailTemplateDto { Id = 2, Name = "Newsletter" }
        };

        _mockCampaignService.Setup(s => s.GetTemplatesAsync())
            .ReturnsAsync(templates);

        // Act
        var result = await _controller.GetTemplates();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task SetTemplate_ValidRequest_ReturnsOk()
    {
        // Arrange
        _mockCampaignService.Setup(s => s.SetTemplateAsync(1, 1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.SetTemplate(1, 1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Clone & Duplicate Tests

    [Fact]
    public async Task Clone_ValidCampaign_ReturnsClonedCampaign()
    {
        // Arrange
        var clonedCampaign = new CampaignDto
        {
            Id = 2,
            Name = "Original Campaign (Copy)",
            Status = CampaignStatus.Draft
        };

        _mockCampaignService.Setup(s => s.CloneAsync(1))
            .ReturnsAsync(clonedCampaign);

        // Act
        var result = await _controller.Clone(1);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
    }

    [Fact]
    public async Task Clone_NonExistingCampaign_ReturnsNotFound()
    {
        // Arrange
        _mockCampaignService.Setup(s => s.CloneAsync(999))
            .ThrowsAsync(new InvalidOperationException("Campaign not found"));

        // Act
        var result = await _controller.Clone(999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region Export Tests

    [Fact]
    public async Task ExportMetrics_ValidCampaign_ReturnsCsvFile()
    {
        // Arrange
        var csvContent = "Email,Opened,Clicked,Bounced\njohn@example.com,true,true,false";
        var bytes = System.Text.Encoding.UTF8.GetBytes(csvContent);

        _mockCampaignService.Setup(s => s.ExportMetricsToCsvAsync(1))
            .ReturnsAsync(bytes);

        // Act
        var result = await _controller.ExportMetrics(1);

        // Assert
        var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
        fileResult.ContentType.Should().Be("text/csv");
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_DraftCampaign_ReturnsNoContent()
    {
        // Arrange
        _mockCampaignService.Setup(s => s.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_ActiveCampaign_ReturnsConflict()
    {
        // Arrange
        _mockCampaignService.Setup(s => s.DeleteAsync(1))
            .ThrowsAsync(new InvalidOperationException("Cannot delete active campaign"));

        // Act
        var result = await _controller.Delete(1);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task Delete_NonExistingCampaign_ReturnsNotFound()
    {
        // Arrange
        _mockCampaignService.Setup(s => s.DeleteAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion
}
