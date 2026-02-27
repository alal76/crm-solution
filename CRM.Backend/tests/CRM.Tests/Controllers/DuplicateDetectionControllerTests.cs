// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Claims;
using CRM.Api.Controllers;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for DuplicatesController.
/// Tests duplicate detection, merge, unmerge, and history endpoints.
/// </summary>
public class DuplicateDetectionControllerTests
{
    private readonly Mock<IDuplicateDetectionService> _mockDuplicateService;
    private readonly Mock<IMergeService> _mockMergeService;
    private readonly DuplicatesController _controller;

    public DuplicateDetectionControllerTests()
    {
        _mockDuplicateService = new Mock<IDuplicateDetectionService>();
        _mockMergeService = new Mock<IMergeService>();
        _controller = new DuplicatesController(
            _mockDuplicateService.Object,
            _mockMergeService.Object);

        // Set up default HttpContext with user claims
        var claims = new List<Claim> { new Claim("sub", "1") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    #region CheckForDuplicates Tests

    [Fact]
    public async Task CheckForDuplicates_ShouldReturnOk_WhenRequestIsValid()
    {
        // Arrange
        var request = new DuplicateCheckRequest
        {
            EntityType = "Contact",
            FieldValues = new Dictionary<string, string?> { { "Email", "test@example.com" } }
        };
        var expectedResult = new DuplicateCheckResult
        {
            Duplicates = new List<DuplicateMatch>
            {
                new() { RecordId = 2, EntityType = "Contact", MatchScore = 90 }
            },
            RecordsScanned = 100
        };

        _mockDuplicateService
            .Setup(s => s.CheckForDuplicatesAsync(
                request.EntityType,
                request.FieldValues,
                request.ExcludeRecordId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.CheckForDuplicates(request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var checkResult = okResult.Value.Should().BeOfType<DuplicateCheckResult>().Subject;
        checkResult.Duplicates.Should().HaveCount(1);
        checkResult.RecordsScanned.Should().Be(100);
    }

    [Fact]
    public async Task CheckForDuplicates_ShouldReturnBadRequest_WhenEntityTypeIsEmpty()
    {
        // Arrange
        var request = new DuplicateCheckRequest
        {
            EntityType = "",
            FieldValues = new Dictionary<string, string?> { { "Email", "test@example.com" } }
        };

        // Act
        var result = await _controller.CheckForDuplicates(request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CheckForDuplicates_ShouldReturnBadRequest_WhenFieldValuesIsNull()
    {
        // Arrange
        var request = new DuplicateCheckRequest
        {
            EntityType = "Contact",
            FieldValues = null!
        };

        // Act
        var result = await _controller.CheckForDuplicates(request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CheckForDuplicates_ShouldReturnBadRequest_WhenFieldValuesIsEmpty()
    {
        // Arrange
        var request = new DuplicateCheckRequest
        {
            EntityType = "Contact",
            FieldValues = new Dictionary<string, string?>()
        };

        // Act
        var result = await _controller.CheckForDuplicates(request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CheckForDuplicates_ShouldReturnOkWithNoDuplicates_WhenNoMatchesFound()
    {
        // Arrange
        var request = new DuplicateCheckRequest
        {
            EntityType = "Lead",
            FieldValues = new Dictionary<string, string?> { { "Email", "unique@example.com" } }
        };
        var expectedResult = new DuplicateCheckResult
        {
            Duplicates = new List<DuplicateMatch>(),
            RecordsScanned = 50
        };

        _mockDuplicateService
            .Setup(s => s.CheckForDuplicatesAsync(
                request.EntityType,
                request.FieldValues,
                request.ExcludeRecordId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.CheckForDuplicates(request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var checkResult = okResult.Value.Should().BeOfType<DuplicateCheckResult>().Subject;
        checkResult.HasDuplicates.Should().BeFalse();
    }

    [Fact]
    public async Task CheckForDuplicates_ShouldPassExcludeRecordId_WhenProvided()
    {
        // Arrange
        var request = new DuplicateCheckRequest
        {
            EntityType = "Account",
            FieldValues = new Dictionary<string, string?> { { "Name", "Acme Corp" } },
            ExcludeRecordId = 42
        };
        var expectedResult = new DuplicateCheckResult();

        _mockDuplicateService
            .Setup(s => s.CheckForDuplicatesAsync(
                request.EntityType,
                request.FieldValues,
                42,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        await _controller.CheckForDuplicates(request, CancellationToken.None);

        // Assert
        _mockDuplicateService.Verify(s => s.CheckForDuplicatesAsync(
            "Account",
            It.IsAny<Dictionary<string, string?>>(),
            42,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetActiveRules Tests

    [Fact]
    public async Task GetActiveRules_ShouldReturnOk_WhenEntityTypeIsValid()
    {
        // Arrange
        var rules = new List<DuplicateRule>
        {
            new() { Id = 1, Name = "Contact Email Match", EntityType = DuplicateEntityType.Contact, IsActive = true }
        };

        _mockDuplicateService
            .Setup(s => s.GetActiveRulesAsync(DuplicateEntityType.Contact))
            .ReturnsAsync(rules);

        // Act
        var result = await _controller.GetActiveRules("Contact");

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedRules = okResult.Value.Should().BeAssignableTo<IEnumerable<DuplicateRule>>().Subject;
        returnedRules.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetActiveRules_ShouldReturnBadRequest_WhenEntityTypeIsInvalid()
    {
        // Arrange & Act
        var result = await _controller.GetActiveRules("InvalidType");

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Theory]
    [InlineData("Lead")]
    [InlineData("Contact")]
    [InlineData("Account")]
    [InlineData("lead")]
    [InlineData("contact")]
    [InlineData("account")]
    public async Task GetActiveRules_ShouldAcceptValidEntityTypes_WhenCaseInsensitive(string entityType)
    {
        // Arrange
        _mockDuplicateService
            .Setup(s => s.GetActiveRulesAsync(It.IsAny<DuplicateEntityType>()))
            .ReturnsAsync(new List<DuplicateRule>());

        // Act
        var result = await _controller.GetActiveRules(entityType);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region ScanForDuplicates Tests

    [Fact]
    public async Task ScanForDuplicates_ShouldReturnOk_WhenEntityTypeIsValid()
    {
        // Arrange
        var candidates = new List<DuplicateCandidate>
        {
            new() { Id = 1, EntityType = DuplicateEntityType.Contact },
            new() { Id = 2, EntityType = DuplicateEntityType.Contact }
        };

        _mockDuplicateService
            .Setup(s => s.ScanForDuplicatesAsync(
                DuplicateEntityType.Contact,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidates);

        // Act
        var result = await _controller.ScanForDuplicates("Contact", null, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var scanResult = okResult.Value.Should().BeOfType<ScanResult>().Subject;
        scanResult.DuplicateCandidatesFound.Should().Be(2);
        scanResult.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ScanForDuplicates_ShouldReturnBadRequest_WhenEntityTypeIsInvalid()
    {
        // Arrange & Act
        var result = await _controller.ScanForDuplicates("InvalidType", null, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ScanForDuplicates_ShouldPassRuleId_WhenProvided()
    {
        // Arrange
        _mockDuplicateService
            .Setup(s => s.ScanForDuplicatesAsync(
                DuplicateEntityType.Lead,
                5,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DuplicateCandidate>());

        // Act
        await _controller.ScanForDuplicates("Lead", 5, CancellationToken.None);

        // Assert
        _mockDuplicateService.Verify(s => s.ScanForDuplicatesAsync(
            DuplicateEntityType.Lead,
            5,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetPendingCandidates Tests

    [Fact]
    public async Task GetPendingCandidates_ShouldReturnOk_WhenEntityTypeIsValid()
    {
        // Arrange
        var candidates = new List<DuplicateCandidate>
        {
            new() { Id = 1, EntityType = DuplicateEntityType.Account }
        };

        _mockDuplicateService
            .Setup(s => s.GetPendingCandidatesAsync(DuplicateEntityType.Account, 1, 20))
            .ReturnsAsync(candidates);

        // Act
        var result = await _controller.GetPendingCandidates("Account", 1, 20);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeAssignableTo<IEnumerable<DuplicateCandidate>>();
    }

    [Fact]
    public async Task GetPendingCandidates_ShouldReturnBadRequest_WhenEntityTypeIsInvalid()
    {
        // Arrange & Act
        var result = await _controller.GetPendingCandidates("BadType", 1, 20);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetPendingCandidates_ShouldUsePaginationDefaults_WhenNotSpecified()
    {
        // Arrange
        _mockDuplicateService
            .Setup(s => s.GetPendingCandidatesAsync(DuplicateEntityType.Contact, 1, 20))
            .ReturnsAsync(new List<DuplicateCandidate>());

        // Act
        await _controller.GetPendingCandidates("Contact");

        // Assert
        _mockDuplicateService.Verify(s => s.GetPendingCandidatesAsync(
            DuplicateEntityType.Contact, 1, 20), Times.Once);
    }

    #endregion

    #region PreviewMerge Tests

    [Fact]
    public async Task PreviewMerge_ShouldReturnOk_WhenRequestIsValid()
    {
        // Arrange
        var request = new MergeRequest
        {
            EntityType = "Contact",
            MasterRecordId = 1,
            RecordsToMerge = new List<int> { 2, 3 }
        };
        var preview = new MergePreview
        {
            PreviewRecord = new Dictionary<string, object?> { { "Email", "test@example.com" } },
            Warnings = new List<string>()
        };

        _mockMergeService
            .Setup(s => s.PreviewMergeAsync(It.IsAny<MergeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(preview);

        // Act
        var result = await _controller.PreviewMerge(request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<MergePreview>();
    }

    [Fact]
    public async Task PreviewMerge_ShouldReturnBadRequest_WhenEntityTypeIsEmpty()
    {
        // Arrange
        var request = new MergeRequest
        {
            EntityType = "",
            MasterRecordId = 1,
            RecordsToMerge = new List<int> { 2 }
        };

        // Act
        var result = await _controller.PreviewMerge(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task PreviewMerge_ShouldReturnBadRequest_WhenMasterRecordIdIsInvalid()
    {
        // Arrange
        var request = new MergeRequest
        {
            EntityType = "Contact",
            MasterRecordId = 0,
            RecordsToMerge = new List<int> { 2 }
        };

        // Act
        var result = await _controller.PreviewMerge(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task PreviewMerge_ShouldReturnBadRequest_WhenRecordsToMergeIsEmpty()
    {
        // Arrange
        var request = new MergeRequest
        {
            EntityType = "Contact",
            MasterRecordId = 1,
            RecordsToMerge = new List<int>()
        };

        // Act
        var result = await _controller.PreviewMerge(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task PreviewMerge_ShouldReturnBadRequest_WhenMasterRecordIsInRecordsToMerge()
    {
        // Arrange
        var request = new MergeRequest
        {
            EntityType = "Contact",
            MasterRecordId = 1,
            RecordsToMerge = new List<int> { 1, 2 }
        };

        // Act
        var result = await _controller.PreviewMerge(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region MergeRecords Tests

    [Fact]
    public async Task MergeRecords_ShouldReturnOk_WhenMergeSucceeds()
    {
        // Arrange
        var request = new MergeRequest
        {
            EntityType = "Contact",
            MasterRecordId = 1,
            RecordsToMerge = new List<int> { 2, 3 }
        };
        var mergeResult = new MergeResult
        {
            Success = true,
            MergeGroupId = 10,
            MasterRecordId = 1,
            RecordsMerged = 2
        };

        _mockMergeService
            .Setup(s => s.MergeRecordsAsync(It.IsAny<MergeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mergeResult);

        // Act
        var result = await _controller.MergeRecords(request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returned = okResult.Value.Should().BeOfType<MergeResult>().Subject;
        returned.Success.Should().BeTrue();
        returned.RecordsMerged.Should().Be(2);
    }

    [Fact]
    public async Task MergeRecords_ShouldReturnBadRequest_WhenMergeFails()
    {
        // Arrange
        var request = new MergeRequest
        {
            EntityType = "Contact",
            MasterRecordId = 1,
            RecordsToMerge = new List<int> { 2 }
        };
        var mergeResult = new MergeResult
        {
            Success = false,
            ErrorMessage = "Merge failed due to conflict"
        };

        _mockMergeService
            .Setup(s => s.MergeRecordsAsync(It.IsAny<MergeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mergeResult);

        // Act
        var result = await _controller.MergeRecords(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task MergeRecords_ShouldReturnBadRequest_WhenEntityTypeIsEmpty()
    {
        // Arrange
        var request = new MergeRequest
        {
            EntityType = "",
            MasterRecordId = 1,
            RecordsToMerge = new List<int> { 2 }
        };

        // Act
        var result = await _controller.MergeRecords(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task MergeRecords_ShouldSetUserIdFromClaims_WhenSubClaimExists()
    {
        // Arrange
        var request = new MergeRequest
        {
            EntityType = "Contact",
            MasterRecordId = 1,
            RecordsToMerge = new List<int> { 2 }
        };
        var mergeResult = new MergeResult { Success = true };

        MergeRequest? capturedRequest = null;
        _mockMergeService
            .Setup(s => s.MergeRecordsAsync(It.IsAny<MergeRequest>(), It.IsAny<CancellationToken>()))
            .Callback<MergeRequest, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(mergeResult);

        // Act
        await _controller.MergeRecords(request);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.UserId.Should().Be(1);
    }

    [Fact]
    public async Task MergeRecords_ShouldDefaultUserId_WhenNoClaimFound()
    {
        // Arrange - set up controller with no claims
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
        };

        var request = new MergeRequest
        {
            EntityType = "Contact",
            MasterRecordId = 1,
            RecordsToMerge = new List<int> { 2 }
        };
        var mergeResult = new MergeResult { Success = true };

        MergeRequest? capturedRequest = null;
        _mockMergeService
            .Setup(s => s.MergeRecordsAsync(It.IsAny<MergeRequest>(), It.IsAny<CancellationToken>()))
            .Callback<MergeRequest, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(mergeResult);

        // Act
        await _controller.MergeRecords(request);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.UserId.Should().Be(1); // defaults to admin user ID 1
    }

    #endregion

    #region UnmergeRecords Tests

    [Fact]
    public async Task UnmergeRecords_ShouldReturnOk_WhenUnmergeSucceeds()
    {
        // Arrange
        var request = new UnmergeRequest
        {
            MergeGroupId = 10,
            RestoreRelatedRecords = true
        };
        var unmergeResult = new UnmergeResult
        {
            Success = true,
            RestoredRecordIds = new List<int> { 2, 3 }
        };

        _mockMergeService
            .Setup(s => s.UnmergeRecordsAsync(It.IsAny<UnmergeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(unmergeResult);

        // Act
        var result = await _controller.UnmergeRecords(request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returned = okResult.Value.Should().BeOfType<UnmergeResult>().Subject;
        returned.Success.Should().BeTrue();
        returned.RestoredRecordIds.Should().HaveCount(2);
    }

    [Fact]
    public async Task UnmergeRecords_ShouldReturnBadRequest_WhenMergeGroupIdIsInvalid()
    {
        // Arrange
        var request = new UnmergeRequest { MergeGroupId = 0 };

        // Act
        var result = await _controller.UnmergeRecords(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UnmergeRecords_ShouldReturnBadRequest_WhenUnmergeFails()
    {
        // Arrange
        var request = new UnmergeRequest { MergeGroupId = 10 };
        var unmergeResult = new UnmergeResult
        {
            Success = false,
            ErrorMessage = "Merge group not found"
        };

        _mockMergeService
            .Setup(s => s.UnmergeRecordsAsync(It.IsAny<UnmergeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(unmergeResult);

        // Act
        var result = await _controller.UnmergeRecords(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region GetMergeHistory Tests

    [Fact]
    public async Task GetMergeHistory_ShouldReturnOk_WhenRecordExists()
    {
        // Arrange
        var history = new List<MergeGroupInfo>
        {
            new() { Id = 1, EntityType = "Contact", MasterRecordId = 1, Status = "Merged" }
        };

        _mockMergeService
            .Setup(s => s.GetMergeHistoryAsync(1, "Contact"))
            .ReturnsAsync(history);

        // Act
        var result = await _controller.GetMergeHistory("Contact", 1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returned = okResult.Value.Should().BeAssignableTo<IEnumerable<MergeGroupInfo>>().Subject;
        returned.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetMergeHistory_ShouldReturnOkWithEmptyList_WhenNoHistory()
    {
        // Arrange
        _mockMergeService
            .Setup(s => s.GetMergeHistoryAsync(999, "Contact"))
            .ReturnsAsync(new List<MergeGroupInfo>());

        // Act
        var result = await _controller.GetMergeHistory("Contact", 999);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returned = okResult.Value.Should().BeAssignableTo<IEnumerable<MergeGroupInfo>>().Subject;
        returned.Should().BeEmpty();
    }

    #endregion

    #region GetMergedRecords Tests

    [Fact]
    public async Task GetMergedRecords_ShouldReturnOk_WhenMasterRecordExists()
    {
        // Arrange
        var records = new List<MergedRecordInfo>
        {
            new() { RecordId = 2, EntityType = "Contact", IsMaster = false, Status = "Merged" },
            new() { RecordId = 3, EntityType = "Contact", IsMaster = false, Status = "Merged" }
        };

        _mockMergeService
            .Setup(s => s.GetMergedRecordsAsync(1, "Contact"))
            .ReturnsAsync(records);

        // Act
        var result = await _controller.GetMergedRecords("Contact", 1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returned = okResult.Value.Should().BeAssignableTo<IEnumerable<MergedRecordInfo>>().Subject;
        returned.Should().HaveCount(2);
    }

    #endregion

    #region GetMergeGroup Tests

    [Fact]
    public async Task GetMergeGroup_ShouldReturnOk_WhenGroupExists()
    {
        // Arrange
        var group = new MergeGroupInfo
        {
            Id = 10,
            EntityType = "Contact",
            MasterRecordId = 1,
            Status = "Merged",
            Members = new List<MergedRecordInfo>()
        };

        _mockMergeService
            .Setup(s => s.GetMergeGroupAsync(10))
            .ReturnsAsync(group);

        // Act
        var result = await _controller.GetMergeGroup(10);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returned = okResult.Value.Should().BeOfType<MergeGroupInfo>().Subject;
        returned.Id.Should().Be(10);
    }

    [Fact]
    public async Task GetMergeGroup_ShouldReturnNotFound_WhenGroupDoesNotExist()
    {
        // Arrange
        _mockMergeService
            .Setup(s => s.GetMergeGroupAsync(999))
            .ReturnsAsync((MergeGroupInfo?)null);

        // Act
        var result = await _controller.GetMergeGroup(999);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    #endregion
}
