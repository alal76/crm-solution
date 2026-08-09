// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Claims;
using CRM.Api.Controllers;
using CRM.Core.Dtos.ITSM;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces.ITSM;
using CRM.Infrastructure.Services.ITSM;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

public class ITSMIncidentsControllerTests
{
    private readonly Mock<IIncidentService> _mockService;
    private readonly Mock<IAssignmentRulesEngine> _mockAssignmentRulesEngine;
    private readonly Mock<IImpactAnalysisService> _mockImpactAnalysisService;
    private readonly IncidentsController _controller;

    public ITSMIncidentsControllerTests()
    {
        _mockService = new Mock<IIncidentService>();
        _mockAssignmentRulesEngine = new Mock<IAssignmentRulesEngine>();
        _mockImpactAnalysisService = new Mock<IImpactAnalysisService>();
        _controller = new IncidentsController(
            _mockService.Object,
            _mockAssignmentRulesEngine.Object,
            _mockImpactAnalysisService.Object);

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

    // ────────────────────────────────────────────────────────────────
    // GET /{id}/assignment/evaluate
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task EvaluateAssignment_ShouldReturnOk_WhenSuccessful()
    {
        var evalResult = new AssignmentResult { IncidentId = 1, WasAssigned = true };
        _mockAssignmentRulesEngine.Setup(s => s.EvaluateAsync(1)).ReturnsAsync(evalResult);

        var result = await _controller.EvaluateAssignment(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(evalResult);
    }

    [Fact]
    public async Task EvaluateAssignment_ShouldThrow_WhenIncidentDoesNotExist()
    {
        _mockAssignmentRulesEngine
            .Setup(s => s.EvaluateAsync(999))
            .ThrowsAsync(new ArgumentException("Incident 999 not found"));

        var act = async () => await _controller.EvaluateAssignment(999);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    // ────────────────────────────────────────────────────────────────
    // POST /{id}/assignment/auto-assign
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task AutoAssignIncident_ShouldReturnOk_WhenSuccessful()
    {
        var evalResult = new AssignmentResult { IncidentId = 1, WasAssigned = true, AssignedGroupId = 2 };
        _mockAssignmentRulesEngine.Setup(s => s.AutoAssignAsync(1)).ReturnsAsync(evalResult);

        var result = await _controller.AutoAssignIncident(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(evalResult);
    }

    [Fact]
    public async Task AutoAssignIncident_ShouldThrow_WhenIncidentDoesNotExist()
    {
        _mockAssignmentRulesEngine
            .Setup(s => s.AutoAssignAsync(999))
            .ThrowsAsync(new ArgumentException("Incident 999 not found"));

        var act = async () => await _controller.AutoAssignIncident(999);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /assignment-rules
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAssignmentRules_ShouldReturnOkWithRules()
    {
        var rules = new List<AssignmentRule> { new() { RuleId = 1, Name = "Rule A" } };
        _mockAssignmentRulesEngine.Setup(s => s.GetRulesAsync()).ReturnsAsync(rules);

        var result = await _controller.GetAssignmentRules();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(rules);
    }

    // ────────────────────────────────────────────────────────────────
    // PUT /assignment-rules
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SaveAssignmentRule_ShouldReturnOk_WhenCreating()
    {
        var newRule = new AssignmentRule { RuleId = 0, Name = "New Rule" };
        var savedRule = new AssignmentRule { RuleId = 10, Name = "New Rule" };
        _mockAssignmentRulesEngine.Setup(s => s.SaveRuleAsync(newRule)).ReturnsAsync(savedRule);

        var result = await _controller.SaveAssignmentRule(newRule);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(savedRule);
    }

    [Fact]
    public async Task SaveAssignmentRule_ShouldReturnOk_WhenUpdating()
    {
        var existingRule = new AssignmentRule { RuleId = 5, Name = "Updated Rule" };
        _mockAssignmentRulesEngine.Setup(s => s.SaveRuleAsync(existingRule)).ReturnsAsync(existingRule);

        var result = await _controller.SaveAssignmentRule(existingRule);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(existingRule);
    }

    // ────────────────────────────────────────────────────────────────
    // DELETE /assignment-rules/{ruleId}
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAssignmentRule_ShouldReturnOkTrue_WhenDeleted()
    {
        _mockAssignmentRulesEngine.Setup(s => s.DeleteRuleAsync(1)).ReturnsAsync(true);

        var result = await _controller.DeleteAssignmentRule(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(true);
    }

    [Fact]
    public async Task DeleteAssignmentRule_ShouldReturnOkFalse_WhenRuleDoesNotExist()
    {
        _mockAssignmentRulesEngine.Setup(s => s.DeleteRuleAsync(999)).ReturnsAsync(false);

        var result = await _controller.DeleteAssignmentRule(999);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(false);
    }

    // ────────────────────────────────────────────────────────────────
    // GET /assignment-rules/{ruleId}/test/{incidentId}
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task TestAssignmentRule_ShouldReturnOk_WhenSuccessful()
    {
        var testResult = new RuleTestResult { RuleId = 1, IncidentId = 1, WouldMatch = true };
        _mockAssignmentRulesEngine.Setup(s => s.TestRuleAsync(1, 1)).ReturnsAsync(testResult);

        var result = await _controller.TestAssignmentRule(1, 1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(testResult);
    }

    [Fact]
    public async Task TestAssignmentRule_ShouldThrow_WhenRuleDoesNotExist()
    {
        _mockAssignmentRulesEngine
            .Setup(s => s.TestRuleAsync(999, 1))
            .ThrowsAsync(new ArgumentException("Rule 999 not found"));

        var act = async () => await _controller.TestAssignmentRule(999, 1);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /assignment/group-workloads
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetGroupWorkloads_ShouldReturnOkWithWorkloads()
    {
        var workloads = new List<GroupWorkload> { new() { GroupId = 1, GroupName = "Group 1" } };
        _mockAssignmentRulesEngine.Setup(s => s.GetGroupWorkloadsAsync()).ReturnsAsync(workloads);

        var result = await _controller.GetGroupWorkloads();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(workloads);
    }

    // ────────────────────────────────────────────────────────────────
    // GET /assignment/available-agents/{groupId}
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAvailableAgents_ShouldReturnOkWithAgents()
    {
        var agents = new List<AvailableAgent> { new() { UserId = 1, DisplayName = "Agent A" } };
        _mockAssignmentRulesEngine.Setup(s => s.GetAvailableAgentsAsync(2)).ReturnsAsync(agents);

        var result = await _controller.GetAvailableAgents(2);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(agents);
    }

    [Fact]
    public async Task GetAvailableAgents_ShouldReturnOkEmpty_WhenGroupHasNoAgents()
    {
        _mockAssignmentRulesEngine.Setup(s => s.GetAvailableAgentsAsync(999)).ReturnsAsync(new List<AvailableAgent>());

        var result = await _controller.GetAvailableAgents(999);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var agents = okResult.Value.Should().BeAssignableTo<IEnumerable<AvailableAgent>>().Subject;
        agents.Should().BeEmpty();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /{id}/impact-analysis
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetImpactAnalysis_ShouldReturnOk_WhenSuccessful()
    {
        var analysis = new IncidentImpactAnalysis { IncidentId = 1, IncidentNumber = "INC0001" };
        _mockImpactAnalysisService.Setup(s => s.AnalyzeIncidentImpactAsync(1)).ReturnsAsync(analysis);

        var result = await _controller.GetImpactAnalysis(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(analysis);
    }

    [Fact]
    public async Task GetImpactAnalysis_ShouldThrow_WhenIncidentDoesNotExist()
    {
        _mockImpactAnalysisService
            .Setup(s => s.AnalyzeIncidentImpactAsync(999))
            .ThrowsAsync(new ArgumentException("Incident 999 not found"));

        var act = async () => await _controller.GetImpactAnalysis(999);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /impact-analysis/affected-services/{configurationItemId}
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAffectedServices_ShouldReturnOkWithServices()
    {
        var services = new List<AffectedService> { new() { ServiceId = 1, ServiceName = "Email" } };
        _mockImpactAnalysisService.Setup(s => s.GetAffectedServicesAsync(1)).ReturnsAsync(services);

        var result = await _controller.GetAffectedServices(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(services);
    }

    [Fact]
    public async Task GetAffectedServices_ShouldReturnOkEmpty_WhenCIDoesNotExist()
    {
        _mockImpactAnalysisService.Setup(s => s.GetAffectedServicesAsync(999)).ReturnsAsync(new List<AffectedService>());

        var result = await _controller.GetAffectedServices(999);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var services = okResult.Value.Should().BeAssignableTo<IEnumerable<AffectedService>>().Subject;
        services.Should().BeEmpty();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /impact-analysis/affected-users/{configurationItemId}
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAffectedUsers_ShouldReturnOk_WhenSuccessful()
    {
        var userGroup = new AffectedUserGroup { TotalUsersAffected = 50 };
        _mockImpactAnalysisService.Setup(s => s.GetAffectedUsersAsync(1)).ReturnsAsync(userGroup);

        var result = await _controller.GetAffectedUsers(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(userGroup);
    }

    [Fact]
    public async Task GetAffectedUsers_ShouldReturnOkDefault_WhenCIDoesNotExist()
    {
        _mockImpactAnalysisService.Setup(s => s.GetAffectedUsersAsync(999)).ReturnsAsync(new AffectedUserGroup());

        var result = await _controller.GetAffectedUsers(999);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var userGroup = okResult.Value.Should().BeOfType<AffectedUserGroup>().Subject;
        userGroup.TotalUsersAffected.Should().Be(0);
    }

    // ────────────────────────────────────────────────────────────────
    // GET /{id}/business-impact
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetBusinessImpact_ShouldReturnOk_WhenSuccessful()
    {
        var score = new BusinessImpactScore { OverallScore = 75, ImpactLevel = "High" };
        _mockImpactAnalysisService.Setup(s => s.CalculateBusinessImpactAsync(1)).ReturnsAsync(score);

        var result = await _controller.GetBusinessImpact(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(score);
    }

    [Fact]
    public async Task GetBusinessImpact_ShouldReturnOkDefault_WhenIncidentDoesNotExist()
    {
        _mockImpactAnalysisService.Setup(s => s.CalculateBusinessImpactAsync(999)).ReturnsAsync(new BusinessImpactScore());

        var result = await _controller.GetBusinessImpact(999);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var score = okResult.Value.Should().BeOfType<BusinessImpactScore>().Subject;
        score.OverallScore.Should().Be(0);
    }

    // ────────────────────────────────────────────────────────────────
    // GET /impact-analysis/dependency-chain/{configurationItemId}
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetDependencyChain_ShouldReturnOk_WhenSuccessful()
    {
        var chain = new DependencyChain { ConfigurationItemId = 1, CIName = "Web Server" };
        _mockImpactAnalysisService.Setup(s => s.GetDependencyChainAsync(1)).ReturnsAsync(chain);

        var result = await _controller.GetDependencyChain(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(chain);
    }

    [Fact]
    public async Task GetDependencyChain_ShouldReturnOkUnknown_WhenCIDoesNotExist()
    {
        var chain = new DependencyChain { ConfigurationItemId = 999, CIName = "Unknown" };
        _mockImpactAnalysisService.Setup(s => s.GetDependencyChainAsync(999)).ReturnsAsync(chain);

        var result = await _controller.GetDependencyChain(999);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returned = okResult.Value.Should().BeOfType<DependencyChain>().Subject;
        returned.CIName.Should().Be("Unknown");
    }

    // ────────────────────────────────────────────────────────────────
    // GET /impact-analysis/predict-outage/{configurationItemId}
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task PredictOutageImpact_ShouldReturnOkWithPredictions()
    {
        var predictions = new List<PredictedImpact> { new() { Category = "Service Availability", Severity = 5 } };
        _mockImpactAnalysisService.Setup(s => s.PredictOutageImpactAsync(1)).ReturnsAsync(predictions);

        var result = await _controller.PredictOutageImpact(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(predictions);
    }

    [Fact]
    public async Task PredictOutageImpact_ShouldReturnOkEmpty_WhenCIDoesNotExist()
    {
        _mockImpactAnalysisService.Setup(s => s.PredictOutageImpactAsync(999)).ReturnsAsync(new List<PredictedImpact>());

        var result = await _controller.PredictOutageImpact(999);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var predictions = okResult.Value.Should().BeAssignableTo<IEnumerable<PredictedImpact>>().Subject;
        predictions.Should().BeEmpty();
    }
}
