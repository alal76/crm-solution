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
using System.Security.Claims;

namespace CRM.Tests.Controllers;

public class ITSMSLAControllerTests
{
    private readonly Mock<ISLAService> _mockService;
    private readonly SLAController _controller;

    public ITSMSLAControllerTests()
    {
        _mockService = new Mock<ISLAService>();
        _controller = new SLAController(_mockService.Object);

        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "1") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    // ────────────────────────────────────────────────────────────────
    // POST /policies
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreatePolicy_ShouldReturnCreatedAtAction()
    {
        var dto = new SLAPolicyDto { Name = "Critical SLA", SLAPolicyId = 1 };
        var created = new SLAPolicyDto { SLAPolicyId = 1, Name = "Critical SLA" };
        _mockService.Setup(s => s.CreateSLAPolicyAsync(dto, 1)).ReturnsAsync(created);

        var result = await _controller.CreatePolicy(dto);

        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(SLAController.GetPolicy));
        createdResult.Value.Should().Be(created);
    }

    // ────────────────────────────────────────────────────────────────
    // GET /policies
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetPolicies_ShouldReturnOk()
    {
        var policies = new List<SLAPolicyDto>
        {
            new() { SLAPolicyId = 1, Name = "P1 SLA" },
            new() { SLAPolicyId = 2, Name = "P2 SLA" }
        };
        _mockService.Setup(s => s.GetSLAPoliciesAsync(null)).ReturnsAsync(policies);

        var result = await _controller.GetPolicies(null);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returned = okResult.Value.Should().BeAssignableTo<IEnumerable<SLAPolicyDto>>().Subject;
        returned.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPolicies_ShouldFilterByTargetType()
    {
        var policies = new List<SLAPolicyDto> { new() { SLAPolicyId = 1 } };
        _mockService
            .Setup(s => s.GetSLAPoliciesAsync(SLATargetType.Incident))
            .ReturnsAsync(policies);

        var result = await _controller.GetPolicies((int)SLATargetType.Incident);

        _mockService.Verify(s => s.GetSLAPoliciesAsync(SLATargetType.Incident), Times.Once);
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /policies/{id}
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetPolicy_ShouldReturnOk_WhenPolicyExists()
    {
        var policies = new List<SLAPolicyDto>
        {
            new() { SLAPolicyId = 1, Name = "Target SLA" },
            new() { SLAPolicyId = 2, Name = "Other SLA" }
        };
        _mockService.Setup(s => s.GetSLAPoliciesAsync(null)).ReturnsAsync(policies);

        var result = await _controller.GetPolicy(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var policy = okResult.Value.Should().BeOfType<SLAPolicyDto>().Subject;
        policy.SLAPolicyId.Should().Be(1);
    }

    [Fact]
    public async Task GetPolicy_ShouldReturnNotFound_WhenPolicyDoesNotExist()
    {
        var policies = new List<SLAPolicyDto>
        {
            new() { SLAPolicyId = 1, Name = "Only policy" }
        };
        _mockService.Setup(s => s.GetSLAPoliciesAsync(null)).ReturnsAsync(policies);

        var result = await _controller.GetPolicy(999);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /instances/{targetId}/{targetType}
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetActiveSLA_ShouldReturnOk_WhenInstanceExists()
    {
        var instance = new SLAInstanceDto { TargetId = 10, TargetType = SLATargetType.Incident };
        _mockService
            .Setup(s => s.GetSLAInstanceAsync(10, SLATargetType.Incident))
            .ReturnsAsync(instance);

        var result = await _controller.GetActiveSLA(10, (int)SLATargetType.Incident);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(instance);
    }

    [Fact]
    public async Task GetActiveSLA_ShouldReturnNotFound_WhenNoInstance()
    {
        _mockService
            .Setup(s => s.GetSLAInstanceAsync(999, SLATargetType.Incident))
            .ReturnsAsync((SLAInstanceDto?)null);

        var result = await _controller.GetActiveSLA(999, (int)SLATargetType.Incident);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /breached
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetBreachedSLAs_ShouldReturnOk()
    {
        var breached = new List<SLAInstanceDto>
        {
            new() { TargetId = 1, ResponseBreached = true }
        };
        _mockService.Setup(s => s.GetBreachedSLAsAsync()).ReturnsAsync(breached);

        var result = await _controller.GetBreachedSLAs();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
    }

    // ────────────────────────────────────────────────────────────────
    // POST /check-breaches
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CheckSLABreaches_ShouldReturnOk()
    {
        _mockService.Setup(s => s.CheckSLABreachesAsync()).Returns(Task.CompletedTask);

        var result = await _controller.CheckSLABreaches();

        result.Should().BeOfType<OkResult>();
        _mockService.Verify(s => s.CheckSLABreachesAsync(), Times.Once);
    }

    // ────────────────────────────────────────────────────────────────
    // POST /{targetId}/{targetType}/pause
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task PauseSLA_ShouldReturnOk()
    {
        _mockService
            .Setup(s => s.PauseSLAAsync(10, SLATargetType.Incident, "Customer unresponsive"))
            .Returns(Task.CompletedTask);

        var dto = new PauseSLADto { Reason = "Customer unresponsive" };
        var result = await _controller.PauseSLA(10, (int)SLATargetType.Incident, dto);

        result.Should().BeOfType<OkResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // POST /{targetId}/{targetType}/resume
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ResumeSLA_ShouldReturnOk()
    {
        _mockService
            .Setup(s => s.ResumeSLAAsync(10, SLATargetType.Incident))
            .Returns(Task.CompletedTask);

        var result = await _controller.ResumeSLA(10, (int)SLATargetType.Incident);

        result.Should().BeOfType<OkResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /dashboard
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetDashboard_ShouldReturnOk()
    {
        var dashboard = new SLADashboardInfo
        {
            TotalActiveSLAs = 10,
            BreachedCount = 2,
            AtRiskCount = 3,
            OverallComplianceRate = 80.0
        };
        _mockService.Setup(s => s.GetSLADashboardAsync()).ReturnsAsync(dashboard);

        var result = await _controller.GetDashboard();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /at-risk
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAtRiskSLAs_ShouldReturnOk_WithDefaultThreshold()
    {
        var atRisk = new List<SLAInstanceDto> { new() { TargetId = 1 } };
        _mockService.Setup(s => s.GetAtRiskSLAsAsync(30)).ReturnsAsync(atRisk);

        var result = await _controller.GetAtRiskSLAs();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
        _mockService.Verify(s => s.GetAtRiskSLAsAsync(30), Times.Once);
    }

    [Fact]
    public async Task GetAtRiskSLAs_ShouldUseCustomThreshold()
    {
        var atRisk = new List<SLAInstanceDto>();
        _mockService.Setup(s => s.GetAtRiskSLAsAsync(60)).ReturnsAsync(atRisk);

        var result = await _controller.GetAtRiskSLAs(60);

        _mockService.Verify(s => s.GetAtRiskSLAsAsync(60), Times.Once);
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /metrics
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetMetrics_ShouldReturnOk_WithDefaultDateRange()
    {
        var metrics = new SLAMetricsInfo
        {
            AverageResponseTimeMinutes = 15.0,
            AverageResolutionTimeMinutes = 120.0,
            ResponseComplianceRate = 95.0
        };
        _mockService
            .Setup(s => s.GetSLAMetricsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(metrics);

        var result = await _controller.GetMetrics(null, null);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetMetrics_ShouldPassExplicitDateRange()
    {
        var start = new DateTime(2026, 1, 1);
        var end = new DateTime(2026, 1, 31);
        var metrics = new SLAMetricsInfo { ResponseComplianceRate = 90.0 };
        _mockService.Setup(s => s.GetSLAMetricsAsync(start, end)).ReturnsAsync(metrics);

        var result = await _controller.GetMetrics(start, end);

        _mockService.Verify(s => s.GetSLAMetricsAsync(start, end), Times.Once);
        result.Result.Should().BeOfType<OkObjectResult>();
    }
}
