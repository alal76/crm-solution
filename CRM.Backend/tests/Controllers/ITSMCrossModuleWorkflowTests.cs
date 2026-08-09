// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Claims;
using CRM.Core.Dtos.ITSM;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces.ITSM;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// ITSM-044: Cross-module workflow tests verifying the Incident → Problem → Change lifecycle.
/// Tests the interactions between ITSM modules at the controller level.
/// </summary>
public class ITSMCrossModuleWorkflowTests
{
    private readonly Mock<IIncidentService> _mockIncidentService;
    private readonly Mock<IProblemService> _mockProblemService;

    public ITSMCrossModuleWorkflowTests()
    {
        _mockIncidentService = new Mock<IIncidentService>();
        _mockProblemService = new Mock<IProblemService>();
    }

    private static ControllerContext CreateTestContext(string userId = "1")
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }

    // --- Incident creation as the first step in the lifecycle ---

    [Fact]
    public async Task Lifecycle_Step1_CreateIncident_ShouldReturnNewIncident()
    {
        // Arrange
        var controller = new CRM.Api.Controllers.IncidentsController(
            _mockIncidentService.Object,
            new Mock<CRM.Infrastructure.Services.ITSM.IAssignmentRulesEngine>().Object,
            new Mock<CRM.Infrastructure.Services.ITSM.IImpactAnalysisService>().Object);
        controller.ControllerContext = CreateTestContext();

        var createDto = new CreateIncidentDto
        {
            ShortDescription = "Recurring network outage",
            CallerId = 10,
            Impact = IncidentImpact.High,
            Urgency = IncidentUrgency.High
        };

        _mockIncidentService.Setup(s => s.CreateIncidentAsync(createDto, 1))
            .ReturnsAsync(new IncidentDto
            {
                IncidentId = 1,
                ShortDescription = "Recurring network outage",
                State = IncidentState.New
            });

        // Act
        var result = await controller.CreateIncident(createDto);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var incident = createdResult.Value.Should().BeOfType<IncidentDto>().Subject;
        incident.State.Should().Be(IncidentState.New);
    }

    // --- Problem creation to investigate root cause ---

    [Fact]
    public async Task Lifecycle_Step2_CreateProblem_ShouldReturnNewProblem()
    {
        // Arrange
        var controller = new CRM.Api.Controllers.ProblemsController(
            _mockProblemService.Object,
            new Mock<Microsoft.Extensions.Logging.ILogger<CRM.Api.Controllers.ProblemsController>>().Object);
        controller.ControllerContext = CreateTestContext();

        var createDto = new CreateProblemDto
        {
            ShortDescription = "Root cause: network switch degradation"
        };

        _mockProblemService.Setup(s => s.CreateProblemAsync(createDto, 1))
            .ReturnsAsync(new ProblemDto
            {
                ProblemId = 1,
                ShortDescription = "Root cause: network switch degradation",
                State = ProblemState.New
            });

        // Act
        var result = await controller.CreateProblem(createDto);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var problem = createdResult.Value.Should().BeOfType<ProblemDto>().Subject;
        problem.State.Should().Be(ProblemState.New);
    }

    // --- Link incident to problem ---

    [Fact]
    public async Task Lifecycle_Step3_LinkIncidentToProblem_ShouldSucceed()
    {
        // Arrange
        _mockProblemService.Setup(s => s.LinkIncidentAsync(1, 1, 1)).ReturnsAsync(true);

        var controller = new CRM.Api.Controllers.ProblemsController(
            _mockProblemService.Object,
            new Mock<Microsoft.Extensions.Logging.ILogger<CRM.Api.Controllers.ProblemsController>>().Object);
        controller.ControllerContext = CreateTestContext();

        // Act
        var result = await controller.LinkIncident(1, 1);

        // Assert
        result.Should().BeOfType<OkResult>();
        _mockProblemService.Verify(s => s.LinkIncidentAsync(1, 1, 1), Times.Once);
    }

    // --- Get related incidents from problem ---

    [Fact]
    public async Task Lifecycle_Step4_GetRelatedIncidents_ShouldReturnLinkedIncidents()
    {
        // Arrange
        var incidents = new List<IncidentDto>
        {
            new IncidentDto { IncidentId = 1, ShortDescription = "Network outage 1" },
            new IncidentDto { IncidentId = 2, ShortDescription = "Network outage 2" }
        };

        _mockProblemService.Setup(s => s.GetRelatedIncidentsAsync(1))
            .ReturnsAsync(incidents);

        var controller = new CRM.Api.Controllers.ProblemsController(
            _mockProblemService.Object,
            new Mock<Microsoft.Extensions.Logging.ILogger<CRM.Api.Controllers.ProblemsController>>().Object);
        controller.ControllerContext = CreateTestContext();

        // Act
        var result = await controller.GetRelatedIncidents(1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var relatedIncidents = okResult.Value.Should().BeAssignableTo<IEnumerable<IncidentDto>>().Subject;
        relatedIncidents.Should().HaveCount(2);
    }

    // --- Mark problem as known error ---

    [Fact]
    public async Task Lifecycle_Step5_MarkAsKnownError_ShouldSucceed()
    {
        // Arrange
        _mockProblemService.Setup(s => s.MarkAsKnownErrorAsync(1, 1)).ReturnsAsync(true);

        var controller = new CRM.Api.Controllers.ProblemsController(
            _mockProblemService.Object,
            new Mock<Microsoft.Extensions.Logging.ILogger<CRM.Api.Controllers.ProblemsController>>().Object);
        controller.ControllerContext = CreateTestContext();

        // Act
        var result = await controller.MarkAsKnownError(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    // --- Resolve the incident after fix ---

    [Fact]
    public async Task Lifecycle_Step6_ResolveIncident_ShouldSucceed()
    {
        // Arrange
        var controller = new CRM.Api.Controllers.IncidentsController(
            _mockIncidentService.Object,
            new Mock<CRM.Infrastructure.Services.ITSM.IAssignmentRulesEngine>().Object,
            new Mock<CRM.Infrastructure.Services.ITSM.IImpactAnalysisService>().Object);
        controller.ControllerContext = CreateTestContext();

        var resolveDto = new ResolveIncidentDto
        {
            ResolutionCode = ResolutionCode.SolvedPermanently,
            ResolutionNotes = "Network switch replaced via change CHG-001"
        };

        _mockIncidentService.Setup(s => s.ResolveIncidentAsync(1, resolveDto, 1))
            .ReturnsAsync(new IncidentDto
            {
                IncidentId = 1,
                State = IncidentState.Resolved,
                ResolutionCode = ResolutionCode.SolvedPermanently
            });

        // Act
        var result = await controller.ResolveIncident(1, resolveDto);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var resolved = okResult.Value.Should().BeOfType<IncidentDto>().Subject;
        resolved.State.Should().Be(IncidentState.Resolved);
    }

    // --- Close resolved incident ---

    [Fact]
    public async Task Lifecycle_Step7_CloseIncident_ShouldSucceed()
    {
        // Arrange
        var controller = new CRM.Api.Controllers.IncidentsController(
            _mockIncidentService.Object,
            new Mock<CRM.Infrastructure.Services.ITSM.IAssignmentRulesEngine>().Object,
            new Mock<CRM.Infrastructure.Services.ITSM.IImpactAnalysisService>().Object);
        controller.ControllerContext = CreateTestContext();

        _mockIncidentService.Setup(s => s.CloseIncidentAsync(1, 1)).ReturnsAsync(true);

        // Act
        var result = await controller.CloseIncident(1);

        // Assert
        _mockIncidentService.Verify(s => s.CloseIncidentAsync(1, 1), Times.Once);
    }
}
