// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

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

public class ITSMProblemsControllerTests
{
    private readonly Mock<IProblemService> _mockService;
    private readonly Mock<ILogger<ProblemsController>> _mockLogger;
    private readonly ProblemsController _controller;

    public ITSMProblemsControllerTests()
    {
        _mockService = new Mock<IProblemService>();
        _mockLogger = new Mock<ILogger<ProblemsController>>();
        _controller = new ProblemsController(_mockService.Object, _mockLogger.Object);

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
    public async Task GetProblem_ShouldReturnOk_WhenProblemExists()
    {
        var dto = new ProblemDto { ProblemId = 1, ShortDescription = "Root cause unknown" };
        _mockService.Setup(s => s.GetProblemByIdAsync(1)).ReturnsAsync(dto);

        var result = await _controller.GetProblem(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(dto);
    }

    [Fact]
    public async Task GetProblem_ShouldReturnNotFound_WhenProblemDoesNotExist()
    {
        _mockService.Setup(s => s.GetProblemByIdAsync(999)).ReturnsAsync((ProblemDto?)null);

        var result = await _controller.GetProblem(999);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetProblems_ShouldReturnOkWithPagedResult()
    {
        var items = new List<ProblemDto>
        {
            new() { ProblemId = 1, ShortDescription = "Problem A" },
            new() { ProblemId = 2, ShortDescription = "Problem B" }
        };
        _mockService
            .Setup(s => s.GetProblemsAsync(It.IsAny<ProblemFilterDto>()))
            .ReturnsAsync((items.AsEnumerable(), 2));

        var result = await _controller.GetProblems(null, 1, 20);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var paged = okResult.Value.Should().BeOfType<PagedResult<ProblemDto>>().Subject;
        paged.Items.Should().HaveCount(2);
        paged.TotalCount.Should().Be(2);
    }

    // ────────────────────────────────────────────────────────────────
    // POST /
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateProblem_ShouldReturnCreatedAtAction()
    {
        var createDto = new CreateProblemDto
        {
            ShortDescription = "New problem",
            Priority = ProblemPriority.High
        };
        var createdDto = new ProblemDto { ProblemId = 3, ShortDescription = "New problem" };
        _mockService.Setup(s => s.CreateProblemAsync(createDto, 1)).ReturnsAsync(createdDto);

        var result = await _controller.CreateProblem(createDto);

        var created = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(ProblemsController.GetProblem));
        created.Value.Should().Be(createdDto);
    }

    // ────────────────────────────────────────────────────────────────
    // PUT /{id}
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateProblem_ShouldReturnOk_WhenSuccessful()
    {
        var updateDto = new UpdateProblemDto { ShortDescription = "Updated problem" };
        var updatedDto = new ProblemDto { ProblemId = 1, ShortDescription = "Updated problem" };
        _mockService.Setup(s => s.UpdateProblemAsync(1, updateDto, 1)).ReturnsAsync(updatedDto);

        var result = await _controller.UpdateProblem(1, updateDto);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(updatedDto);
    }

    // ────────────────────────────────────────────────────────────────
    // POST /{problemId}/link-incident/{incidentId}
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task LinkIncident_ShouldReturnOk_WhenSuccessful()
    {
        _mockService.Setup(s => s.LinkIncidentAsync(1, 10, 1)).ReturnsAsync(true);

        var result = await _controller.LinkIncident(1, 10);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task LinkIncident_ShouldReturnBadRequest_WhenLinkFails()
    {
        _mockService.Setup(s => s.LinkIncidentAsync(1, 999, 1)).ReturnsAsync(false);

        var result = await _controller.LinkIncident(1, 999);

        result.Should().BeOfType<BadRequestResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // PATCH /{id}/mark-known-error
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task MarkAsKnownError_ShouldReturnOk_WhenSuccessful()
    {
        _mockService.Setup(s => s.MarkAsKnownErrorAsync(1, 1)).ReturnsAsync(true);

        var result = await _controller.MarkAsKnownError(1);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task MarkAsKnownError_ShouldReturnBadRequest_WhenFailed()
    {
        _mockService.Setup(s => s.MarkAsKnownErrorAsync(1, 1)).ReturnsAsync(false);

        var result = await _controller.MarkAsKnownError(1);

        result.Should().BeOfType<BadRequestResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /{id}/related-incidents
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetRelatedIncidents_ShouldReturnOkWithIncidents()
    {
        var incidents = new List<IncidentDto>
        {
            new() { IncidentId = 10, ShortDescription = "Related A" },
            new() { IncidentId = 11, ShortDescription = "Related B" }
        };
        _mockService.Setup(s => s.GetRelatedIncidentsAsync(1)).ReturnsAsync(incidents);

        var result = await _controller.GetRelatedIncidents(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedIncidents = okResult.Value.Should().BeAssignableTo<IEnumerable<IncidentDto>>().Subject;
        returnedIncidents.Should().HaveCount(2);
    }

    // ────────────────────────────────────────────────────────────────
    // PATCH /{id}/rca
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateRootCauseAnalysis_ShouldReturnOk_WhenSuccessful()
    {
        _mockService
            .Setup(s => s.UpdateRootCauseAnalysisAsync(1, "Memory leak", "Restart service", 1))
            .ReturnsAsync(true);

        var dto = new UpdateRCADto { RootCause = "Memory leak", Workaround = "Restart service" };
        var result = await _controller.UpdateRootCauseAnalysis(1, dto);

        result.Should().BeOfType<OkResult>();
    }
}
