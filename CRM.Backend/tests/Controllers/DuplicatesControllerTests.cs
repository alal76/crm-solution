// CRM Solution - Customer Relationship Management System
// Duplicates Controller Unit Tests

using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CRM.Api.Controllers;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for DuplicatesController
/// Covers: Duplicate detection, merging, rules
/// </summary>
public class DuplicatesControllerTests
{
    private readonly Mock<IDuplicateService> _mockDuplicateService;
    private readonly Mock<ICrmNotificationService> _mockNotificationService;
    private readonly Mock<ILogger<DuplicatesController>> _mockLogger;
    private readonly DuplicatesController _controller;

    public DuplicatesControllerTests()
    {
        _mockDuplicateService = new Mock<IDuplicateService>();
        _mockNotificationService = new Mock<ICrmNotificationService>();
        _mockLogger = new Mock<ILogger<DuplicatesController>>();

        _controller = new DuplicatesController(
            _mockDuplicateService.Object,
            _mockNotificationService.Object,
            _mockLogger.Object);

        SetupUserContext();
    }

    private void SetupUserContext()
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Email, "admin@example.com"),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    #region Find Duplicates Tests

    [Fact]
    public async Task FindDuplicates_Account_ReturnsDuplicates()
    {
        // Arrange
        var duplicates = new List<DuplicateCandidateDto>
        {
            new DuplicateCandidateDto
            {
                Record1Id = 1,
                Record2Id = 2,
                EntityType = "Account",
                MatchScore = 85,
                MatchFields = new List<string> { "Name", "Email" }
            }
        };

        _mockDuplicateService.Setup(s => s.FindDuplicatesAsync("Account", null))
            .ReturnsAsync(duplicates);

        // Act
        var result = await _controller.FindDuplicates("Account");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedDuplicates = okResult.Value.Should().BeAssignableTo<IEnumerable<DuplicateCandidateDto>>().Subject;
        returnedDuplicates.Should().HaveCount(1);
    }

    [Fact]
    public async Task FindDuplicates_WithFilters_ReturnsFilteredDuplicates()
    {
        // Arrange
        var filters = new DuplicateSearchFilters
        {
            MinMatchScore = 80,
            MatchFields = new List<string> { "Email" }
        };

        var duplicates = new List<DuplicateCandidateDto>
        {
            new DuplicateCandidateDto { Record1Id = 1, Record2Id = 2, MatchScore = 90 }
        };

        _mockDuplicateService.Setup(s => s.FindDuplicatesAsync("Account", filters))
            .ReturnsAsync(duplicates);

        // Act
        var result = await _controller.FindDuplicates("Account", filters);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeAssignableTo<IEnumerable<DuplicateCandidateDto>>();
    }

    [Fact]
    public async Task FindDuplicates_InvalidEntity_ReturnsBadRequest()
    {
        // Arrange
        _mockDuplicateService.Setup(s => s.FindDuplicatesAsync("Invalid", null))
            .ThrowsAsync(new ArgumentException("Invalid entity type"));

        // Act
        var result = await _controller.FindDuplicates("Invalid");

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task FindDuplicatesForRecord_ReturnsPotentialDuplicates()
    {
        // Arrange
        var duplicates = new List<DuplicateCandidateDto>
        {
            new DuplicateCandidateDto { Record1Id = 1, Record2Id = 5, MatchScore = 75 },
            new DuplicateCandidateDto { Record1Id = 1, Record2Id = 8, MatchScore = 60 }
        };

        _mockDuplicateService.Setup(s => s.FindDuplicatesForRecordAsync("Account", 1))
            .ReturnsAsync(duplicates);

        // Act
        var result = await _controller.FindDuplicatesForRecord("Account", 1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedDuplicates = okResult.Value.Should().BeAssignableTo<IEnumerable<DuplicateCandidateDto>>().Subject;
        returnedDuplicates.Should().HaveCount(2);
    }

    #endregion

    #region Merge Tests

    [Fact]
    public async Task MergeRecords_ValidRequest_ReturnsOkWithMergedId()
    {
        // Arrange
        var mergeRequest = new MergeRecordsDto
        {
            EntityType = "Account",
            MasterRecordId = 1,
            DuplicateRecordIds = new List<int> { 2, 3 },
            FieldSelections = new Dictionary<string, int>
            {
                { "Name", 1 },
                { "Phone", 2 }
            }
        };

        _mockDuplicateService.Setup(s => s.MergeRecordsAsync(mergeRequest))
            .ReturnsAsync(1);
        _mockNotificationService.Setup(n => n.NotifyEntityUpdatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.MergeRecords(mergeRequest);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(new { MergedRecordId = 1 });
    }

    [Fact]
    public async Task MergeRecords_SameMasterAndDuplicate_ReturnsBadRequest()
    {
        // Arrange
        var mergeRequest = new MergeRecordsDto
        {
            EntityType = "Account",
            MasterRecordId = 1,
            DuplicateRecordIds = new List<int> { 1, 2 }
        };

        _mockDuplicateService.Setup(s => s.MergeRecordsAsync(mergeRequest))
            .ThrowsAsync(new ArgumentException("Master record cannot be in duplicate list"));

        // Act
        var result = await _controller.MergeRecords(mergeRequest);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task MergeRecords_RecordNotFound_ReturnsNotFound()
    {
        // Arrange
        var mergeRequest = new MergeRecordsDto
        {
            EntityType = "Account",
            MasterRecordId = 999,
            DuplicateRecordIds = new List<int> { 1 }
        };

        _mockDuplicateService.Setup(s => s.MergeRecordsAsync(mergeRequest))
            .ThrowsAsync(new KeyNotFoundException("Record not found"));

        // Act
        var result = await _controller.MergeRecords(mergeRequest);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task PreviewMerge_ReturnsPreview()
    {
        // Arrange
        var mergeRequest = new MergeRecordsDto
        {
            EntityType = "Account",
            MasterRecordId = 1,
            DuplicateRecordIds = new List<int> { 2 }
        };

        var preview = new MergePreviewDto
        {
            MasterRecord = new Dictionary<string, object> { { "Name", "Acme Corp" } },
            DuplicateRecords = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object> { { "Name", "Acme Corporation" } }
            },
            SuggestedMerge = new Dictionary<string, object> { { "Name", "Acme Corp" } }
        };

        _mockDuplicateService.Setup(s => s.PreviewMergeAsync(mergeRequest))
            .ReturnsAsync(preview);

        // Act
        var result = await _controller.PreviewMerge(mergeRequest);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<MergePreviewDto>();
    }

    #endregion

    #region Duplicate Rules Tests

    [Fact]
    public async Task GetDuplicateRules_ReturnsRules()
    {
        // Arrange
        var rules = new List<DuplicateRuleDto>
        {
            new DuplicateRuleDto
            {
                Id = 1,
                Name = "Account Name Match",
                EntityType = "Account",
                IsActive = true,
                MatchFields = new List<string> { "Name" }
            }
        };

        _mockDuplicateService.Setup(s => s.GetDuplicateRulesAsync())
            .ReturnsAsync(rules);

        // Act
        var result = await _controller.GetDuplicateRules();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedRules = okResult.Value.Should().BeAssignableTo<IEnumerable<DuplicateRuleDto>>().Subject;
        returnedRules.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetDuplicateRuleById_ExistingRule_ReturnsOk()
    {
        // Arrange
        var rule = new DuplicateRuleDto
        {
            Id = 1,
            Name = "Account Name Match",
            EntityType = "Account"
        };

        _mockDuplicateService.Setup(s => s.GetDuplicateRuleByIdAsync(1))
            .ReturnsAsync(rule);

        // Act
        var result = await _controller.GetDuplicateRuleById(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<DuplicateRuleDto>();
    }

    [Fact]
    public async Task GetDuplicateRuleById_NonExistingRule_ReturnsNotFound()
    {
        // Arrange
        _mockDuplicateService.Setup(s => s.GetDuplicateRuleByIdAsync(999))
            .ReturnsAsync((DuplicateRuleDto?)null);

        // Act
        var result = await _controller.GetDuplicateRuleById(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task CreateDuplicateRule_ValidRule_ReturnsCreatedAtAction()
    {
        // Arrange
        var createDto = new CreateDuplicateRuleDto
        {
            Name = "Email Match Rule",
            EntityType = "Contact",
            MatchFields = new List<string> { "Email" },
            MatchThreshold = 90
        };

        var createdRule = new DuplicateRuleDto
        {
            Id = 2,
            Name = "Email Match Rule",
            EntityType = "Contact"
        };

        _mockDuplicateService.Setup(s => s.CreateDuplicateRuleAsync(createDto))
            .ReturnsAsync(createdRule);

        // Act
        var result = await _controller.CreateDuplicateRule(createDto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task UpdateDuplicateRule_ValidRule_ReturnsOk()
    {
        // Arrange
        var updateDto = new UpdateDuplicateRuleDto
        {
            Id = 1,
            Name = "Updated Rule Name",
            MatchThreshold = 85
        };

        var updatedRule = new DuplicateRuleDto
        {
            Id = 1,
            Name = "Updated Rule Name"
        };

        _mockDuplicateService.Setup(s => s.UpdateDuplicateRuleAsync(updateDto))
            .ReturnsAsync(updatedRule);

        // Act
        var result = await _controller.UpdateDuplicateRule(1, updateDto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task DeleteDuplicateRule_ExistingRule_ReturnsNoContent()
    {
        // Arrange
        _mockDuplicateService.Setup(s => s.DeleteDuplicateRuleAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteDuplicateRule(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task ActivateDuplicateRule_ReturnsOk()
    {
        // Arrange
        _mockDuplicateService.Setup(s => s.ActivateDuplicateRuleAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ActivateDuplicateRule(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task DeactivateDuplicateRule_ReturnsOk()
    {
        // Arrange
        _mockDuplicateService.Setup(s => s.DeactivateDuplicateRuleAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeactivateDuplicateRule(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Dismiss/Ignore Tests

    [Fact]
    public async Task DismissDuplicate_ValidIds_ReturnsOk()
    {
        // Arrange
        var dismissRequest = new DismissDuplicateDto
        {
            EntityType = "Account",
            Record1Id = 1,
            Record2Id = 2,
            Reason = "Not actually duplicates - different companies"
        };

        _mockDuplicateService.Setup(s => s.DismissDuplicateAsync(dismissRequest))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DismissDuplicate(dismissRequest);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task GetDismissedDuplicates_ReturnsDismissedPairs()
    {
        // Arrange
        var dismissed = new List<DismissedDuplicateDto>
        {
            new DismissedDuplicateDto
            {
                EntityType = "Account",
                Record1Id = 1,
                Record2Id = 2,
                DismissedAt = DateTime.UtcNow,
                DismissedBy = "admin@example.com"
            }
        };

        _mockDuplicateService.Setup(s => s.GetDismissedDuplicatesAsync("Account"))
            .ReturnsAsync(dismissed);

        // Act
        var result = await _controller.GetDismissedDuplicates("Account");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeAssignableTo<IEnumerable<DismissedDuplicateDto>>();
    }

    [Fact]
    public async Task UndismissDuplicate_ValidIds_ReturnsOk()
    {
        // Arrange
        _mockDuplicateService.Setup(s => s.UndismissDuplicateAsync("Account", 1, 2))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UndismissDuplicate("Account", 1, 2);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetDuplicateStatistics_ReturnsStats()
    {
        // Arrange
        var stats = new DuplicateStatisticsDto
        {
            TotalDuplicates = 150,
            DuplicatesByEntity = new Dictionary<string, int>
            {
                { "Account", 80 },
                { "Contact", 50 },
                { "Lead", 20 }
            },
            MergedCount = 45,
            DismissedCount = 30
        };

        _mockDuplicateService.Setup(s => s.GetDuplicateStatisticsAsync())
            .ReturnsAsync(stats);

        // Act
        var result = await _controller.GetDuplicateStatistics();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedStats = okResult.Value.Should().BeOfType<DuplicateStatisticsDto>().Subject;
        returnedStats.TotalDuplicates.Should().Be(150);
    }

    [Fact]
    public async Task GetDuplicateStatisticsByEntity_ReturnsEntityStats()
    {
        // Arrange
        var stats = new EntityDuplicateStatisticsDto
        {
            EntityType = "Account",
            TotalDuplicates = 80,
            HighConfidenceCount = 30,
            MediumConfidenceCount = 35,
            LowConfidenceCount = 15
        };

        _mockDuplicateService.Setup(s => s.GetDuplicateStatisticsByEntityAsync("Account"))
            .ReturnsAsync(stats);

        // Act
        var result = await _controller.GetDuplicateStatisticsByEntity("Account");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<EntityDuplicateStatisticsDto>();
    }

    #endregion

    #region Batch Operations Tests

    [Fact]
    public async Task BatchMerge_ValidRequests_ReturnsOkWithResults()
    {
        // Arrange
        var mergeRequests = new List<MergeRecordsDto>
        {
            new MergeRecordsDto { EntityType = "Account", MasterRecordId = 1, DuplicateRecordIds = new List<int> { 2 } },
            new MergeRecordsDto { EntityType = "Account", MasterRecordId = 3, DuplicateRecordIds = new List<int> { 4 } }
        };

        var results = new BatchMergeResultDto
        {
            SuccessCount = 2,
            FailureCount = 0,
            Results = new List<MergeResultDto>
            {
                new MergeResultDto { Success = true, MergedRecordId = 1 },
                new MergeResultDto { Success = true, MergedRecordId = 3 }
            }
        };

        _mockDuplicateService.Setup(s => s.BatchMergeAsync(mergeRequests))
            .ReturnsAsync(results);

        // Act
        var result = await _controller.BatchMerge(mergeRequests);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedResults = okResult.Value.Should().BeOfType<BatchMergeResultDto>().Subject;
        returnedResults.SuccessCount.Should().Be(2);
    }

    [Fact]
    public async Task BatchDismiss_ValidRequests_ReturnsOkWithCount()
    {
        // Arrange
        var dismissRequests = new List<DismissDuplicateDto>
        {
            new DismissDuplicateDto { EntityType = "Account", Record1Id = 1, Record2Id = 2 },
            new DismissDuplicateDto { EntityType = "Account", Record1Id = 3, Record2Id = 4 }
        };

        _mockDuplicateService.Setup(s => s.BatchDismissAsync(dismissRequests))
            .ReturnsAsync(2);

        // Act
        var result = await _controller.BatchDismiss(dismissRequests);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(new { DismissedCount = 2 });
    }

    #endregion

    #region Scan Tests

    [Fact]
    public async Task StartDuplicateScan_ReturnsJobId()
    {
        // Arrange
        var scanRequest = new DuplicateScanRequestDto
        {
            EntityType = "Account",
            ScanAllRecords = true
        };

        _mockDuplicateService.Setup(s => s.StartDuplicateScanAsync(scanRequest))
            .ReturnsAsync("scan-job-123");

        // Act
        var result = await _controller.StartDuplicateScan(scanRequest);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(new { JobId = "scan-job-123" });
    }

    [Fact]
    public async Task GetScanStatus_ReturnsStatus()
    {
        // Arrange
        var status = new DuplicateScanStatusDto
        {
            JobId = "scan-job-123",
            Status = "InProgress",
            Progress = 60,
            DuplicatesFound = 25
        };

        _mockDuplicateService.Setup(s => s.GetScanStatusAsync("scan-job-123"))
            .ReturnsAsync(status);

        // Act
        var result = await _controller.GetScanStatus("scan-job-123");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<DuplicateScanStatusDto>();
    }

    #endregion
}
