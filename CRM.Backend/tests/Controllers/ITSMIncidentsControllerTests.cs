// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using Xunit;
using Moq;
using FluentAssertions;
using CRM.Api.Controllers;
using CRM.Core.DTOs.ITSM;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces.ITSM;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace CRM.Tests.Controllers;

public class ITSMIncidentsControllerTests
{
    private readonly Mock<IIncidentService> _mockService;
    private readonly Mock<ILogger<IncidentsController>> _mockLogger;
    private readonly IncidentsController _controller;

    public ITSMIncidentsControllerTests()
    {
        _mockService = new Mock<IIncidentService>();
        _mockLogger = new Mock<ILogger<IncidentsController>>();
        _controller = new IncidentsController(_mockService.Object, _mockLogger.Object);

        // Set up HttpContext with a mock user claim
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "1") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    // ────────────────────────────────────────────────────────────────
    // GET /{id}
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetIncident_ShouldReturnOk_WhenIncidentExists()
    {
        var dto = new IncidentDto { IncidentId = 1, ShortDescription = "Server down" };
        _mockService.Setup(s => s.GetIncidentByIdAsync(1)).ReturnsAsync(dto);

        var result = await _controller.GetIncident(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(dto);
    }

    [Fact]
    public async Task GetIncident_ShouldReturnNotFound_WhenIncidentDoesNotExist()
    {
        _mockService.Setup(s => s.GetIncidentByIdAsync(999)).ReturnsAsync((IncidentDto?)null);

        var result = await _controller.GetIncident(999);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetIncidents_ShouldReturnOkWithPagedResult()
    {
        var items = new List<IncidentDto>
        {
            new() { IncidentId = 1, ShortDescription = "Incident A" },
            new() { IncidentId = 2, ShortDescription = "Incident B" }
        };
        _mockService
            .Setup(s => s.GetIncidentsAsync(It.IsAny<IncidentFilterDto>()))
            .ReturnsAsync((items.AsEnumerable(), 2));

        var result = await _controller.GetIncidents(null, null, null, null, 1, 20);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var paged = okResult.Value.Should().BeOfType<PagedResult<IncidentDto>>().Subject;
        paged.Items.Should().HaveCount(2);
        paged.TotalCount.Should().Be(2);
    }

    // ────────────────────────────────────────────────────────────────
    // POST /
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateIncident_ShouldReturnCreatedAtAction()
    {
        var createDto = new CreateIncidentDto
        {
            ShortDescription = "New incident",
            CallerId = 10,
            Impact = IncidentImpact.Medium,
            Urgency = IncidentUrgency.Medium
        };
        var createdDto = new IncidentDto { IncidentId = 5, ShortDescription = "New incident" };
        _mockService
            .Setup(s => s.CreateIncidentAsync(createDto, 1))
            .ReturnsAsync(createdDto);

        var result = await _controller.CreateIncident(createDto);

        var created = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(IncidentsController.GetIncident));
        created.Value.Should().Be(createdDto);
    }

    // ────────────────────────────────────────────────────────────────
    // PUT /{id}
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateIncident_ShouldReturnOk_WhenSuccessful()
    {
        var updateDto = new UpdateIncidentDto { ShortDescription = "Updated" };
        var updatedDto = new IncidentDto { IncidentId = 1, ShortDescription = "Updated" };
        _mockService.Setup(s => s.UpdateIncidentAsync(1, updateDto, 1)).ReturnsAsync(updatedDto);

        var result = await _controller.UpdateIncident(1, updateDto);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(updatedDto);
    }

    // ────────────────────────────────────────────────────────────────
    // PATCH /{id}/assign
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task AssignIncident_ShouldReturnOk_WhenSuccessful()
    {
        _mockService
            .Setup(s => s.AssignIncidentAsync(1, 5, null, 1))
            .ReturnsAsync(true);

        var dto = new AssignIncidentDto { AssignedToId = 5, AssignedGroupId = null };
        var result = await _controller.AssignIncident(1, dto);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // PATCH /{id}/escalate
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task EscalateIncident_ShouldReturnOk_WhenSuccessful()
    {
        _mockService.Setup(s => s.EscalateIncidentAsync(1, 1)).ReturnsAsync(true);

        var dto = new EscalateIncidentDto { EscalationLevel = 2 };
        var result = await _controller.EscalateIncident(1, dto);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // PATCH /{id}/resolve
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ResolveIncident_ShouldReturnOk_WithResolvedIncident()
    {
        var resolveDto = new ResolveIncidentDto
        {
            ResolutionCode = ResolutionCode.SolvedPermanently,
            ResolutionNotes = "Fixed by restarting service"
        };
        var resolvedDto = new IncidentDto
        {
            IncidentId = 1,
            State = IncidentState.Resolved,
            ResolutionNotes = "Fixed by restarting service"
        };
        _mockService.Setup(s => s.ResolveIncidentAsync(1, resolveDto, 1)).ReturnsAsync(resolvedDto);

        var result = await _controller.ResolveIncident(1, resolveDto);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var incident = okResult.Value.Should().BeOfType<IncidentDto>().Subject;
        incident.State.Should().Be(IncidentState.Resolved);
    }

    // ────────────────────────────────────────────────────────────────
    // PATCH /{id}/close
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CloseIncident_ShouldReturnOk_WhenSuccessful()
    {
        _mockService.Setup(s => s.CloseIncidentAsync(1, 1)).ReturnsAsync(true);

        var result = await _controller.CloseIncident(1);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // PATCH /{id}/reopen
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ReopenIncident_ShouldReturnOk_WhenSuccessful()
    {
        _mockService.Setup(s => s.ReopenIncidentAsync(1, 1)).ReturnsAsync(true);

        var result = await _controller.ReopenIncident(1);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // POST /{id}/comments
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddComment_ShouldReturnOk()
    {
        _mockService
            .Setup(s => s.AddCommentAsync(1, "Test comment", false, 1))
            .ReturnsAsync(true);

        var dto = new AddIncidentCommentDto { CommentText = "Test comment", IsInternal = false };
        var result = await _controller.AddComment(1, dto);

        result.Should().BeOfType<OkResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /{id}/comments
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetComments_ShouldReturnOkWithComments()
    {
        var comments = new List<IncidentComment>
        {
            new() { Comment = "First comment" },
            new() { Comment = "Second comment" }
        };
        _mockService.Setup(s => s.GetCommentsAsync(1)).ReturnsAsync(comments);

        var result = await _controller.GetComments(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedComments = okResult.Value.Should().BeAssignableTo<IEnumerable<IncidentComment>>().Subject;
        returnedComments.Should().HaveCount(2);
    }
}
