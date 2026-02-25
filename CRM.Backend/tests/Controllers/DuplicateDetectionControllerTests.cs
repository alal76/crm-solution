// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.Api.Controllers;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

public class DuplicateDetectionControllerUnitTests
{
    private readonly Mock<IDuplicateDetectionService> _mockDuplicateService;
    private readonly Mock<IMergeService> _mockMergeService;
    private readonly Mock<ILogger<DuplicatesController>> _mockLogger;
    private readonly DuplicatesController _controller;

    public DuplicateDetectionControllerUnitTests()
    {
        _mockDuplicateService = new Mock<IDuplicateDetectionService>();
        _mockMergeService = new Mock<IMergeService>();
        _mockLogger = new Mock<ILogger<DuplicatesController>>();
        _controller = new DuplicatesController(_mockDuplicateService.Object, _mockMergeService.Object, _mockLogger.Object);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
    }

    [Fact]
    public async Task CheckForDuplicates_ShouldReturnOk_WhenNoDuplicatesFound()
    {
        var request = new DuplicateCheckRequest { EntityType = "Contact", FieldValues = new Dictionary<string, string?> { { "Email", "unique@example.com" } } };
        var checkResult = new DuplicateCheckResult { Duplicates = new List<DuplicateMatch>(), RecordsScanned = 50 };
        _mockDuplicateService.Setup(s => s.CheckForDuplicatesAsync(request.EntityType, request.FieldValues, request.ExcludeRecordId, It.IsAny<CancellationToken>())).ReturnsAsync(checkResult);
        var result = await _controller.CheckForDuplicates(request, CancellationToken.None);
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<DuplicateCheckResult>().Which.HasDuplicates.Should().BeFalse();
    }

    [Fact]
    public async Task CheckForDuplicates_ShouldReturnOk_WhenDuplicatesFound()
    {
        var request = new DuplicateCheckRequest { EntityType = "Lead", FieldValues = new Dictionary<string, string?> { { "Email", "dup@example.com" } } };
        var checkResult = new DuplicateCheckResult { Duplicates = new List<DuplicateMatch> { new DuplicateMatch { RecordId = 5, EntityType = "Lead", MatchScore = 95 } }, RecordsScanned = 100 };
        _mockDuplicateService.Setup(s => s.CheckForDuplicatesAsync(request.EntityType, request.FieldValues, request.ExcludeRecordId, It.IsAny<CancellationToken>())).ReturnsAsync(checkResult);
        var result = await _controller.CheckForDuplicates(request, CancellationToken.None);
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<DuplicateCheckResult>().Which.HasDuplicates.Should().BeTrue();
    }

    [Fact]
    public async Task CheckForDuplicates_ShouldReturnBadRequest_WhenEntityTypeIsEmpty()
    {
        var request = new DuplicateCheckRequest { EntityType = "", FieldValues = new Dictionary<string, string?> { { "Email", "test@example.com" } } };
        var result = await _controller.CheckForDuplicates(request, CancellationToken.None);
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CheckForDuplicates_ShouldReturnBadRequest_WhenFieldValuesAreEmpty()
    {
        var request = new DuplicateCheckRequest { EntityType = "Contact", FieldValues = new Dictionary<string, string?>() };
        var result = await _controller.CheckForDuplicates(request, CancellationToken.None);
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetActiveRules_ShouldReturnOk_WhenEntityTypeIsValid()
    {
        var rules = new List<DuplicateRule> { new DuplicateRule { Id = 1, Name = "Email Rule", EntityType = DuplicateEntityType.Contact } };
        _mockDuplicateService.Setup(s => s.GetActiveRulesAsync(DuplicateEntityType.Contact)).ReturnsAsync(rules);
        var result = await _controller.GetActiveRules("Contact");
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        (okResult.Value as IEnumerable<DuplicateRule>).Should().HaveCount(1);
    }

    [Fact]
    public async Task GetActiveRules_ShouldReturnBadRequest_WhenEntityTypeIsInvalid()
    {
        var result = await _controller.GetActiveRules("InvalidType");
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetMergeGroup_ShouldReturnOk_WhenGroupExists()
    {
        var group = new MergeGroupInfo { Id = 42 };
        _mockMergeService.Setup(s => s.GetMergeGroupAsync(42)).ReturnsAsync(group);
        var result = await _controller.GetMergeGroup(42);
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetMergeGroup_ShouldReturnNotFound_WhenGroupDoesNotExist()
    {
        _mockMergeService.Setup(s => s.GetMergeGroupAsync(999)).ReturnsAsync((MergeGroupInfo?)null);
        var result = await _controller.GetMergeGroup(999);
        result.Result.Should().BeOfType<NotFoundResult>();
    }
}
