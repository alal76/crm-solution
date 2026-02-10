// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Leads Controller Unit Tests

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
/// Comprehensive unit tests for LeadsController
/// Covers: CRUD operations, conversion, scoring, routing, qualification
/// </summary>
public class LeadsControllerTests
{
    private readonly Mock<ILeadService> _mockLeadService;
    private readonly Mock<ILogger<LeadsController>> _mockLogger;
    private readonly Mock<ICrmNotificationService> _mockNotificationService;
    private readonly LeadsController _controller;

    public LeadsControllerTests()
    {
        _mockLeadService = new Mock<ILeadService>();
        _mockLogger = new Mock<ILogger<LeadsController>>();
        _mockNotificationService = new Mock<ICrmNotificationService>();

        _mockNotificationService.Setup(x => x.NotifyRecordCreatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);
        _mockNotificationService.Setup(x => x.NotifyRecordUpdatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);
        _mockNotificationService.Setup(x => x.NotifyRecordDeletedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);

        _controller = new LeadsController(_mockLeadService.Object, _mockLogger.Object);

        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_ReturnsOkResult_WithLeads()
    {
        // Arrange
        var leads = new List<LeadDto>
        {
            new LeadDto { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", Status = LeadStatus.New },
            new LeadDto { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane@example.com", Status = LeadStatus.Qualified }
        };

        _mockLeadService.Setup(s => s.GetAllAsync())
            .ReturnsAsync(leads);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedLeads = okResult.Value as IEnumerable<LeadDto>;
        returnedLeads.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_WithStatusFilter_ReturnsFilteredLeads()
    {
        // Arrange
        var leads = new List<LeadDto>
        {
            new LeadDto { Id = 1, Status = LeadStatus.Qualified }
        };

        _mockLeadService.Setup(s => s.GetByStatusAsync(LeadStatus.Qualified))
            .ReturnsAsync(leads);

        // Act
        var result = await _controller.GetByStatus(LeadStatus.Qualified);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedLeads = okResult.Value as IEnumerable<LeadDto>;
        returnedLeads.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAll_WithOwnerFilter_ReturnsOwnedLeads()
    {
        // Arrange
        var leads = new List<LeadDto>
        {
            new LeadDto { Id = 1, OwnerId = 1 },
            new LeadDto { Id = 2, OwnerId = 1 }
        };

        _mockLeadService.Setup(s => s.GetByOwnerAsync(1))
            .ReturnsAsync(leads);

        // Act
        var result = await _controller.GetByOwner(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedLeads = okResult.Value as IEnumerable<LeadDto>;
        returnedLeads.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_EmptyList_ReturnsOkWithEmptyArray()
    {
        // Arrange
        _mockLeadService.Setup(s => s.GetAllAsync())
            .ReturnsAsync(new List<LeadDto>());

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedLeads = okResult.Value as IEnumerable<LeadDto>;
        returnedLeads.Should().BeEmpty();
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_ExistingLead_ReturnsOkWithLead()
    {
        // Arrange
        var lead = new LeadDto { Id = 1, FirstName = "John", LastName = "Doe" };

        _mockLeadService.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(lead);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedLead = okResult.Value as LeadDto;
        returnedLead!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetById_NonExistingLead_ReturnsNotFound()
    {
        // Arrange
        _mockLeadService.Setup(s => s.GetByIdAsync(999))
            .ReturnsAsync((LeadDto?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_InvalidId_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.GetById(0);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_ValidLead_ReturnsCreatedWithLead()
    {
        // Arrange
        var createDto = new CreateLeadDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Company = "Acme Corp",
            Source = LeadSource.Website
        };

        var createdLead = new LeadDto
        {
            Id = 1,
            FirstName = createDto.FirstName,
            LastName = createDto.LastName,
            Email = createDto.Email,
            Status = LeadStatus.New
        };

        _mockLeadService.Setup(s => s.CreateAsync(It.IsAny<CreateLeadDto>()))
            .ReturnsAsync(createdLead);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var returnedLead = createdResult.Value as LeadDto;
        returnedLead!.Status.Should().Be(LeadStatus.New);
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
    public async Task Create_MissingRequiredFields_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateLeadDto { FirstName = "" };

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_DuplicateLead_ReturnsConflict()
    {
        // Arrange
        var createDto = new CreateLeadDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "existing@example.com"
        };

        _mockLeadService.Setup(s => s.CreateAsync(It.IsAny<CreateLeadDto>()))
            .ThrowsAsync(new InvalidOperationException("Duplicate lead detected"));

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task Create_WithCampaignSource_SetsLeadSource()
    {
        // Arrange
        var createDto = new CreateLeadDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            CampaignId = 1,
            Source = LeadSource.Campaign
        };

        var createdLead = new LeadDto
        {
            Id = 1,
            Source = LeadSource.Campaign,
            CampaignId = 1
        };

        _mockLeadService.Setup(s => s.CreateAsync(It.IsAny<CreateLeadDto>()))
            .ReturnsAsync(createdLead);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var returnedLead = createdResult.Value as LeadDto;
        returnedLead!.Source.Should().Be(LeadSource.Campaign);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_ValidLead_ReturnsOkWithUpdatedLead()
    {
        // Arrange
        var updateDto = new UpdateLeadDto
        {
            Id = 1,
            FirstName = "John Updated",
            LastName = "Doe"
        };

        var updatedLead = new LeadDto
        {
            Id = 1,
            FirstName = "John Updated",
            LastName = "Doe"
        };

        _mockLeadService.Setup(s => s.UpdateAsync(It.IsAny<UpdateLeadDto>()))
            .ReturnsAsync(updatedLead);

        // Act
        var result = await _controller.Update(1, updateDto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedLead = okResult.Value as LeadDto;
        returnedLead!.FirstName.Should().Be("John Updated");
    }

    [Fact]
    public async Task Update_IdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var updateDto = new UpdateLeadDto { Id = 2 };

        // Act
        var result = await _controller.Update(1, updateDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_NonExistingLead_ReturnsNotFound()
    {
        // Arrange
        var updateDto = new UpdateLeadDto { Id = 999 };

        _mockLeadService.Setup(s => s.UpdateAsync(It.IsAny<UpdateLeadDto>()))
            .ReturnsAsync((LeadDto?)null);

        // Act
        var result = await _controller.Update(999, updateDto);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_ExistingLead_ReturnsNoContent()
    {
        // Arrange
        _mockLeadService.Setup(s => s.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_NonExistingLead_ReturnsNotFound()
    {
        // Arrange
        _mockLeadService.Setup(s => s.DeleteAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Lead Conversion Tests

    [Fact]
    public async Task ConvertToOpportunity_ValidLead_ReturnsOkWithOpportunity()
    {
        // Arrange
        var convertRequest = new ConvertLeadRequest
        {
            LeadId = 1,
            CreateAccount = true,
            CreateContact = true,
            OpportunityName = "New Opportunity"
        };

        var conversionResult = new LeadConversionResult
        {
            Success = true,
            AccountId = 1,
            ContactId = 1,
            OpportunityId = 1
        };

        _mockLeadService.Setup(s => s.ConvertToOpportunityAsync(It.IsAny<ConvertLeadRequest>()))
            .ReturnsAsync(conversionResult);

        // Act
        var result = await _controller.ConvertToOpportunity(convertRequest);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedResult = okResult.Value as LeadConversionResult;
        returnedResult!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ConvertToOpportunity_NonExistingLead_ReturnsNotFound()
    {
        // Arrange
        var convertRequest = new ConvertLeadRequest { LeadId = 999 };

        _mockLeadService.Setup(s => s.ConvertToOpportunityAsync(It.IsAny<ConvertLeadRequest>()))
            .ThrowsAsync(new InvalidOperationException("Lead not found"));

        // Act
        var result = await _controller.ConvertToOpportunity(convertRequest);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task ConvertToOpportunity_AlreadyConverted_ReturnsConflict()
    {
        // Arrange
        var convertRequest = new ConvertLeadRequest { LeadId = 1 };

        _mockLeadService.Setup(s => s.ConvertToOpportunityAsync(It.IsAny<ConvertLeadRequest>()))
            .ThrowsAsync(new InvalidOperationException("Lead already converted"));

        // Act
        var result = await _controller.ConvertToOpportunity(convertRequest);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task ConvertToOpportunity_WithExistingAccount_LinksToAccount()
    {
        // Arrange
        var convertRequest = new ConvertLeadRequest
        {
            LeadId = 1,
            CreateAccount = false,
            AccountId = 1
        };

        var conversionResult = new LeadConversionResult
        {
            Success = true,
            AccountId = 1,
            OpportunityId = 1
        };

        _mockLeadService.Setup(s => s.ConvertToOpportunityAsync(It.IsAny<ConvertLeadRequest>()))
            .ReturnsAsync(conversionResult);

        // Act
        var result = await _controller.ConvertToOpportunity(convertRequest);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedResult = okResult.Value as LeadConversionResult;
        returnedResult!.AccountId.Should().Be(1);
    }

    #endregion

    #region Lead Status Tests

    [Fact]
    public async Task UpdateStatus_ValidRequest_ReturnsOk()
    {
        // Arrange
        var request = new UpdateLeadStatusRequest
        {
            LeadId = 1,
            NewStatus = LeadStatus.Qualified
        };

        _mockLeadService.Setup(s => s.UpdateStatusAsync(1, LeadStatus.Qualified))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateStatus(request);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task UpdateStatus_InvalidTransition_ReturnsBadRequest()
    {
        // Arrange
        var request = new UpdateLeadStatusRequest
        {
            LeadId = 1,
            NewStatus = LeadStatus.Converted
        };

        _mockLeadService.Setup(s => s.UpdateStatusAsync(1, LeadStatus.Converted))
            .ThrowsAsync(new InvalidOperationException("Invalid status transition"));

        // Act
        var result = await _controller.UpdateStatus(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Qualify_ValidLead_ReturnsOk()
    {
        // Arrange
        _mockLeadService.Setup(s => s.QualifyAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Qualify(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Disqualify_ValidLead_ReturnsOk()
    {
        // Arrange
        var request = new DisqualifyLeadRequest
        {
            LeadId = 1,
            Reason = "Not interested"
        };

        _mockLeadService.Setup(s => s.DisqualifyAsync(1, request.Reason))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Disqualify(request);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Lead Scoring Tests

    [Fact]
    public async Task GetLeadScore_ExistingLead_ReturnsScore()
    {
        // Arrange
        var scoreDetails = new LeadScoreDto
        {
            LeadId = 1,
            Score = 85,
            Breakdown = new Dictionary<string, int>
            {
                { "Company Size", 20 },
                { "Industry Match", 30 },
                { "Engagement", 35 }
            }
        };

        _mockLeadService.Setup(s => s.GetLeadScoreAsync(1))
            .ReturnsAsync(scoreDetails);

        // Act
        var result = await _controller.GetLeadScore(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedScore = okResult.Value as LeadScoreDto;
        returnedScore!.Score.Should().Be(85);
    }

    [Fact]
    public async Task RecalculateScore_ValidLead_ReturnsNewScore()
    {
        // Arrange
        _mockLeadService.Setup(s => s.RecalculateScoreAsync(1))
            .ReturnsAsync(90);

        // Act
        var result = await _controller.RecalculateScore(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetHighScoreLeads_ReturnsQualifiedLeads()
    {
        // Arrange
        var leads = new List<LeadDto>
        {
            new LeadDto { Id = 1, Score = 90 },
            new LeadDto { Id = 2, Score = 85 }
        };

        _mockLeadService.Setup(s => s.GetHighScoreLeadsAsync(80))
            .ReturnsAsync(leads);

        // Act
        var result = await _controller.GetHighScoreLeads(80);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedLeads = okResult.Value as IEnumerable<LeadDto>;
        returnedLeads.Should().HaveCount(2);
    }

    #endregion

    #region Lead Assignment Tests

    [Fact]
    public async Task AssignLead_ValidRequest_ReturnsOk()
    {
        // Arrange
        _mockLeadService.Setup(s => s.AssignAsync(1, 2))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Assign(1, 2);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task AssignLead_InvalidUserId_ReturnsNotFound()
    {
        // Arrange
        _mockLeadService.Setup(s => s.AssignAsync(1, 999))
            .ThrowsAsync(new InvalidOperationException("User not found"));

        // Act
        var result = await _controller.Assign(1, 999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task BulkAssign_ValidRequest_ReturnsOk()
    {
        // Arrange
        var request = new BulkAssignLeadsRequest
        {
            LeadIds = new List<int> { 1, 2, 3 },
            OwnerId = 1
        };

        _mockLeadService.Setup(s => s.BulkAssignAsync(request.LeadIds, request.OwnerId))
            .ReturnsAsync(3);

        // Act
        var result = await _controller.BulkAssign(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task AutoAssign_ValidLead_ReturnsAssignedUser()
    {
        // Arrange
        var assignmentResult = new LeadAssignmentResult
        {
            LeadId = 1,
            AssignedUserId = 2,
            RuleName = "Round Robin"
        };

        _mockLeadService.Setup(s => s.AutoAssignAsync(1))
            .ReturnsAsync(assignmentResult);

        // Act
        var result = await _controller.AutoAssign(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion

    #region Lead Activities Tests

    [Fact]
    public async Task GetLeadActivities_ReturnsActivities()
    {
        // Arrange
        var activities = new List<ActivityDto>
        {
            new ActivityDto { Id = 1, Description = "Initial contact" },
            new ActivityDto { Id = 2, Description = "Follow-up call" }
        };

        _mockLeadService.Setup(s => s.GetActivitiesAsync(1))
            .ReturnsAsync(activities);

        // Act
        var result = await _controller.GetActivities(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedActivities = okResult.Value as IEnumerable<ActivityDto>;
        returnedActivities.Should().HaveCount(2);
    }

    [Fact]
    public async Task AddActivity_ValidRequest_ReturnsCreatedActivity()
    {
        // Arrange
        var createDto = new CreateActivityDto
        {
            Description = "Sent proposal",
            Type = ActivityType.Email
        };

        var createdActivity = new ActivityDto
        {
            Id = 1,
            Description = createDto.Description
        };

        _mockLeadService.Setup(s => s.AddActivityAsync(1, It.IsAny<CreateActivityDto>()))
            .ReturnsAsync(createdActivity);

        // Act
        var result = await _controller.AddActivity(1, createDto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    #endregion

    #region Bulk Operations Tests

    [Fact]
    public async Task BulkCreate_ValidLeads_ReturnsCreatedLeads()
    {
        // Arrange
        var createDtos = new List<CreateLeadDto>
        {
            new CreateLeadDto { FirstName = "John", LastName = "Doe", Email = "john@example.com" },
            new CreateLeadDto { FirstName = "Jane", LastName = "Smith", Email = "jane@example.com" }
        };

        var createdLeads = new List<LeadDto>
        {
            new LeadDto { Id = 1, FirstName = "John" },
            new LeadDto { Id = 2, FirstName = "Jane" }
        };

        _mockLeadService.Setup(s => s.BulkCreateAsync(It.IsAny<List<CreateLeadDto>>()))
            .ReturnsAsync(createdLeads);

        // Act
        var result = await _controller.BulkCreate(createDtos);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedLeads = okResult.Value as IEnumerable<LeadDto>;
        returnedLeads.Should().HaveCount(2);
    }

    [Fact]
    public async Task BulkDelete_ValidIds_ReturnsDeletedCount()
    {
        // Arrange
        var ids = new List<int> { 1, 2, 3 };

        _mockLeadService.Setup(s => s.BulkDeleteAsync(ids))
            .ReturnsAsync(3);

        // Act
        var result = await _controller.BulkDelete(ids);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task BulkQualify_ValidIds_ReturnsCount()
    {
        // Arrange
        var ids = new List<int> { 1, 2, 3 };

        _mockLeadService.Setup(s => s.BulkQualifyAsync(ids))
            .ReturnsAsync(3);

        // Act
        var result = await _controller.BulkQualify(ids);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task Search_ValidQuery_ReturnsMatchingLeads()
    {
        // Arrange
        var leads = new List<LeadDto>
        {
            new LeadDto { Id = 1, FirstName = "John", Company = "Acme" }
        };

        _mockLeadService.Setup(s => s.SearchAsync("Acme"))
            .ReturnsAsync(leads);

        // Act
        var result = await _controller.Search("Acme");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task Search_EmptyQuery_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.Search("");

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region Import/Export Tests

    [Fact]
    public async Task Export_ValidRequest_ReturnsCsvFile()
    {
        // Arrange
        var csvContent = "Id,FirstName,LastName,Email,Status\n1,John,Doe,john@example.com,New";
        var bytes = System.Text.Encoding.UTF8.GetBytes(csvContent);

        _mockLeadService.Setup(s => s.ExportToCsvAsync())
            .ReturnsAsync(bytes);

        // Act
        var result = await _controller.ExportToCsv();

        // Assert
        var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
        fileResult.ContentType.Should().Be("text/csv");
    }

    [Fact]
    public async Task Import_ValidFile_ReturnsImportResult()
    {
        // Arrange
        var importResult = new ImportResult
        {
            TotalRows = 10,
            SuccessCount = 9,
            FailureCount = 1,
            Errors = new List<string> { "Row 5: Invalid email" }
        };

        _mockLeadService.Setup(s => s.ImportFromCsvAsync(It.IsAny<byte[]>()))
            .ReturnsAsync(importResult);

        var file = new Mock<IFormFile>();
        file.Setup(f => f.Length).Returns(100);

        // Act
        var result = await _controller.Import(file.Object);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion

    #region Duplicate Detection Tests

    [Fact]
    public async Task CheckDuplicates_ExistingLeadEmail_ReturnsMatches()
    {
        // Arrange
        var duplicates = new List<LeadDto>
        {
            new LeadDto { Id = 2, Email = "john@example.com", FirstName = "John" }
        };

        _mockLeadService.Setup(s => s.FindDuplicatesAsync("john@example.com"))
            .ReturnsAsync(duplicates);

        // Act
        var result = await _controller.CheckDuplicates("john@example.com");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedDuplicates = okResult.Value as IEnumerable<LeadDto>;
        returnedDuplicates.Should().HaveCount(1);
    }

    [Fact]
    public async Task MergeLeads_ValidRequest_ReturnsMergedLead()
    {
        // Arrange
        var request = new MergeLeadsRequest
        {
            PrimaryLeadId = 1,
            SecondaryLeadIds = new List<int> { 2, 3 }
        };

        var mergedLead = new LeadDto { Id = 1, FirstName = "John" };

        _mockLeadService.Setup(s => s.MergeLeadsAsync(request))
            .ReturnsAsync(mergedLead);

        // Act
        var result = await _controller.MergeLeads(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion
}
