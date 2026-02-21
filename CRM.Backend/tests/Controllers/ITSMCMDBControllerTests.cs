// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Claims;
using CRM.Api.Controllers;
using CRM.Core.DTOs.ITSM;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces.ITSM;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

public class ITSMCMDBControllerTests
{
    private readonly Mock<ICMDBService> _mockService;
    private readonly CMDBController _controller;

    public ITSMCMDBControllerTests()
    {
        _mockService = new Mock<ICMDBService>();
        _controller = new CMDBController(_mockService.Object);

        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "1") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    // ────────────────────────────────────────────────────────────────
    // POST /
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateCI_ShouldReturnCreatedAtAction()
    {
        var createDto = new CreateCIDto { CIName = "WebServer01", CIType = CIType.Server };
        var created = new ConfigurationItemDto { CIId = 1, CIName = "WebServer01" };
        _mockService.Setup(s => s.CreateCIAsync(createDto, 1)).ReturnsAsync(created);

        var result = await _controller.CreateCI(createDto);

        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(CMDBController.GetCI));
        createdResult.Value.Should().Be(created);
    }

    // ────────────────────────────────────────────────────────────────
    // GET /{id}
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetCI_ShouldReturnOk_WhenCIExists()
    {
        var ci = new ConfigurationItemDto { CIId = 1, CIName = "AppServer01" };
        _mockService.Setup(s => s.GetCIByIdAsync(1)).ReturnsAsync(ci);

        var result = await _controller.GetCI(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(ci);
    }

    [Fact]
    public async Task GetCI_ShouldReturnNotFound_WhenCIDoesNotExist()
    {
        _mockService.Setup(s => s.GetCIByIdAsync(999)).ReturnsAsync((ConfigurationItemDto?)null);

        var result = await _controller.GetCI(999);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SearchCIs_ShouldReturnOkWithResults()
    {
        var cis = new List<ConfigurationItemDto>
        {
            new() { CIId = 1, CIName = "DB01" },
            new() { CIId = 2, CIName = "DB02" }
        };
        _mockService
            .Setup(s => s.SearchCIsAsync("DB", null, 1, 20))
            .ReturnsAsync(cis);

        var result = await _controller.SearchCIs("DB", 1, 20);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returned = okResult.Value.Should().BeAssignableTo<IEnumerable<ConfigurationItemDto>>().Subject;
        returned.Should().HaveCount(2);
    }

    // ────────────────────────────────────────────────────────────────
    // PUT /{id}
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateCI_ShouldReturnOk_WhenSuccessful()
    {
        var updateDto = new CreateCIDto { CIName = "UpdatedServer" };
        var updated = new ConfigurationItemDto { CIId = 1, CIName = "UpdatedServer" };
        _mockService.Setup(s => s.UpdateCIAsync(1, updateDto, 1)).ReturnsAsync(updated);

        var result = await _controller.UpdateCI(1, updateDto);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(updated);
    }

    // ────────────────────────────────────────────────────────────────
    // POST /{parentId}/relationships/{childId}
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateRelationship_ShouldReturnOk_WhenSuccessful()
    {
        _mockService
            .Setup(s => s.CreateRelationshipAsync(1, 2, It.IsAny<RelationshipType>(), 1))
            .ReturnsAsync(true);

        var dto = new CreateRelationshipDto { RelationshipType = 0 };
        var result = await _controller.CreateRelationship(1, 2, dto);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task CreateRelationship_ShouldReturnBadRequest_WhenFails()
    {
        _mockService
            .Setup(s => s.CreateRelationshipAsync(1, 2, It.IsAny<RelationshipType>(), 1))
            .ReturnsAsync(false);

        var dto = new CreateRelationshipDto { RelationshipType = 0 };
        var result = await _controller.CreateRelationship(1, 2, dto);

        result.Should().BeOfType<BadRequestResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /{id}/related
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetRelatedCIs_ShouldReturnOk()
    {
        var cis = new List<ConfigurationItemDto> { new() { CIId = 3, CIName = "Linked" } };
        _mockService.Setup(s => s.GetRelatedCIsAsync(1)).ReturnsAsync(cis);

        var result = await _controller.GetRelatedCIs(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returned = okResult.Value.Should().BeAssignableTo<IEnumerable<ConfigurationItemDto>>().Subject;
        returned.Should().HaveCount(1);
    }

    // ────────────────────────────────────────────────────────────────
    // GET /{id}/impact-analysis
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetImpactAnalysis_ShouldReturnOk()
    {
        var impacts = new List<string> { "DB02 depends on AppServer01" };
        _mockService.Setup(s => s.GetImpactAnalysisAsync(1)).ReturnsAsync(impacts);

        var result = await _controller.GetImpactAnalysis(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returned = okResult.Value.Should().BeAssignableTo<IEnumerable<string>>().Subject;
        returned.Should().Contain("DB02 depends on AppServer01");
    }

    // ────────────────────────────────────────────────────────────────
    // GET /{id}/service-map
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetServiceMap_ShouldReturnOk_WithRootAndRelatedCIs()
    {
        var rootCI = new ConfigurationItemDto { CIId = 1, CIName = "Root" };
        var relatedCIs = new List<ConfigurationItemDto> { new() { CIId = 2, CIName = "Child" } };
        _mockService.Setup(s => s.GetCIByIdAsync(1)).ReturnsAsync(rootCI);
        _mockService.Setup(s => s.GetRelatedCIsAsync(1)).ReturnsAsync(relatedCIs);

        var result = await _controller.GetServiceMap(1, 3);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var map = okResult.Value.Should().BeOfType<ServiceMapDto>().Subject;
        map.RootCI.Should().NotBeNull();
        map.RootCI!.CIId.Should().Be(1);
        map.RelatedCIs.Should().HaveCount(1);
        map.Depth.Should().Be(3);
    }

    // ────────────────────────────────────────────────────────────────
    // GET /types
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetCITypes_ShouldReturnOk_WithEnumNames()
    {
        var result = await _controller.GetCITypes();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var types = okResult.Value.Should().BeAssignableTo<string[]>().Subject;
        types.Should().NotBeEmpty();
    }
}
