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
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

public class ITSMCMDBControllerTests
{
    private readonly Mock<ICMDBService> _mockService;
    private readonly Mock<IAssetLifecycleService> _mockLifecycleService;
    private readonly Mock<IDiscoveryService> _mockDiscoveryService;
    private readonly CMDBController _controller;

    public ITSMCMDBControllerTests()
    {
        _mockService = new Mock<ICMDBService>();
        _mockLifecycleService = new Mock<IAssetLifecycleService>();
        _mockDiscoveryService = new Mock<IDiscoveryService>();
        _controller = new CMDBController(_mockService.Object, _mockLifecycleService.Object, _mockDiscoveryService.Object);

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

    // ────────────────────────────────────────────────────────────────
    // GET /{id}/lifecycle
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetLifecycleState_ShouldReturnOk_WhenCIExists()
    {
        var state = new AssetLifecycleState { ConfigurationItemId = 1, AssetName = "SRV01", CurrentStage = LifecycleStage.Deployed };
        _mockLifecycleService.Setup(s => s.GetLifecycleStateAsync(1)).ReturnsAsync(state);

        var result = await _controller.GetLifecycleState(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(state);
    }

    [Fact]
    public async Task GetLifecycleState_ShouldReturnNotFound_WhenCIMissing()
    {
        _mockLifecycleService.Setup(s => s.GetLifecycleStateAsync(999))
            .ThrowsAsync(new ArgumentException("Configuration item 999 not found"));

        var result = await _controller.GetLifecycleState(999);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // POST /{id}/lifecycle/transition
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task TransitionLifecycle_ShouldReturnOk_WhenTransitionAllowed()
    {
        var transition = new AssetLifecycleTransition
        {
            TransitionId = 1,
            ConfigurationItemId = 1,
            FromStage = LifecycleStage.InStock,
            ToStage = LifecycleStage.Deployed
        };
        _mockLifecycleService
            .Setup(s => s.TransitionAsync(1, LifecycleStage.Deployed, 1, "go live"))
            .ReturnsAsync(transition);

        var request = new TransitionLifecycleRequest { TargetStage = LifecycleStage.Deployed, Notes = "go live" };
        var result = await _controller.TransitionLifecycle(1, request);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(transition);
    }

    [Fact]
    public async Task TransitionLifecycle_ShouldReturnBadRequest_WhenTransitionNotAllowed()
    {
        _mockLifecycleService
            .Setup(s => s.TransitionAsync(1, It.IsAny<LifecycleStage>(), 1, null))
            .ThrowsAsync(new InvalidOperationException("Transition not allowed"));

        var request = new TransitionLifecycleRequest { TargetStage = LifecycleStage.Disposed };
        var result = await _controller.TransitionLifecycle(1, request);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task TransitionLifecycle_ShouldReturnNotFound_WhenCIMissing()
    {
        _mockLifecycleService
            .Setup(s => s.TransitionAsync(999, It.IsAny<LifecycleStage>(), 1, null))
            .ThrowsAsync(new ArgumentException("Configuration item 999 not found"));

        var request = new TransitionLifecycleRequest { TargetStage = LifecycleStage.Deployed };
        var result = await _controller.TransitionLifecycle(999, request);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /{id}/lifecycle/history
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetLifecycleHistory_ShouldReturnOk()
    {
        var history = new List<AssetLifecycleTransition> { new() { TransitionId = 1, ConfigurationItemId = 1 } };
        _mockLifecycleService.Setup(s => s.GetLifecycleHistoryAsync(1)).ReturnsAsync(history);

        var result = await _controller.GetLifecycleHistory(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returned = okResult.Value.Should().BeAssignableTo<IEnumerable<AssetLifecycleTransition>>().Subject;
        returned.Should().HaveCount(1);
    }

    // ────────────────────────────────────────────────────────────────
    // POST /{id}/lifecycle/retire
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ScheduleRetirement_ShouldReturnOk_WhenCIExists()
    {
        var retirementDate = new DateTime(2027, 1, 1);
        var schedule = new AssetRetirementSchedule { ScheduleId = 1, ConfigurationItemId = 1, ScheduledDate = retirementDate, Reason = "EOL" };
        _mockLifecycleService
            .Setup(s => s.ScheduleRetirementAsync(1, retirementDate, 1, "EOL"))
            .ReturnsAsync(schedule);

        var request = new ScheduleRetirementRequest { RetirementDate = retirementDate, Reason = "EOL" };
        var result = await _controller.ScheduleRetirement(1, request);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(schedule);
    }

    [Fact]
    public async Task ScheduleRetirement_ShouldReturnNotFound_WhenCIMissing()
    {
        _mockLifecycleService
            .Setup(s => s.ScheduleRetirementAsync(999, It.IsAny<DateTime>(), 1, It.IsAny<string>()))
            .ThrowsAsync(new ArgumentException("Configuration item 999 not found"));

        var request = new ScheduleRetirementRequest { RetirementDate = DateTime.UtcNow, Reason = "EOL" };
        var result = await _controller.ScheduleRetirement(999, request);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /{id}/lifecycle/utilization
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetUtilizationMetrics_ShouldReturnOk_WhenCIExists()
    {
        var metrics = new AssetUtilizationMetrics { ConfigurationItemId = 1, AssetName = "SRV01" };
        _mockLifecycleService.Setup(s => s.GetUtilizationMetricsAsync(1)).ReturnsAsync(metrics);

        var result = await _controller.GetUtilizationMetrics(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(metrics);
    }

    [Fact]
    public async Task GetUtilizationMetrics_ShouldReturnNotFound_WhenCIMissing()
    {
        _mockLifecycleService.Setup(s => s.GetUtilizationMetricsAsync(999))
            .ThrowsAsync(new ArgumentException("Configuration item 999 not found"));

        var result = await _controller.GetUtilizationMetrics(999);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /{id}/lifecycle/cost-analysis
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetCostAnalysis_ShouldReturnOk_WhenCIExists()
    {
        var analysis = new LifecycleCostAnalysis { ConfigurationItemId = 1, AssetName = "SRV01" };
        _mockLifecycleService.Setup(s => s.GetCostAnalysisAsync(1)).ReturnsAsync(analysis);

        var result = await _controller.GetCostAnalysis(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(analysis);
    }

    [Fact]
    public async Task GetCostAnalysis_ShouldReturnNotFound_WhenCIMissing()
    {
        _mockLifecycleService.Setup(s => s.GetCostAnalysisAsync(999))
            .ThrowsAsync(new ArgumentException("Configuration item 999 not found"));

        var result = await _controller.GetCostAnalysis(999);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /lifecycle/end-of-life-alerts
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetEndOfLifeAlerts_ShouldReturnOk()
    {
        var alerts = new List<AssetEndOfLifeAlert> { new() { ConfigurationItemId = 1, AssetName = "SRV01" } };
        _mockLifecycleService.Setup(s => s.GetEndOfLifeAlertsAsync(90)).ReturnsAsync(alerts);

        var result = await _controller.GetEndOfLifeAlerts(90);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returned = okResult.Value.Should().BeAssignableTo<IEnumerable<AssetEndOfLifeAlert>>().Subject;
        returned.Should().HaveCount(1);
    }

    // ────────────────────────────────────────────────────────────────
    // GET /lifecycle/refresh-candidates
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetRefreshCandidates_ShouldReturnOk()
    {
        var candidates = new List<AssetRefreshCandidate> { new() { ConfigurationItemId = 1, AssetName = "SRV01" } };
        _mockLifecycleService.Setup(s => s.GetRefreshCandidatesAsync()).ReturnsAsync(candidates);

        var result = await _controller.GetRefreshCandidates();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returned = okResult.Value.Should().BeAssignableTo<IEnumerable<AssetRefreshCandidate>>().Subject;
        returned.Should().HaveCount(1);
    }

    // ────────────────────────────────────────────────────────────────
    // POST /discovery/scan
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task RunDiscoveryScan_ShouldReturnOk()
    {
        var request = new DiscoveryScanRequest { Name = "Weekly Scan", Type = DiscoveryType.NetworkScan, Target = "10.0.0.0/8" };
        var scanResult = new DiscoveryScanResult { ScanId = 1 };
        _mockDiscoveryService.Setup(s => s.RunDiscoveryScanAsync(request)).ReturnsAsync(scanResult);

        var result = await _controller.RunDiscoveryScan(request);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(scanResult);
    }

    // ────────────────────────────────────────────────────────────────
    // GET /discovery/scan/{scanId}/status
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetScanStatus_ShouldReturnOk_WhenScanExists()
    {
        var status = new DiscoveryScanStatus { ScanId = 1, State = ScanState.Running };
        _mockDiscoveryService.Setup(s => s.GetScanStatusAsync(1)).ReturnsAsync(status);

        var result = await _controller.GetScanStatus(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(status);
    }

    [Fact]
    public async Task GetScanStatus_ShouldReturnNotFound_WhenScanMissing()
    {
        _mockDiscoveryService.Setup(s => s.GetScanStatusAsync(999))
            .ThrowsAsync(new ArgumentException("Scan 999 not found"));

        var result = await _controller.GetScanStatus(999);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /discovery/pending-assets
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetPendingDiscoveredAssets_ShouldReturnOk()
    {
        var assets = new List<DiscoveredAsset> { new() { DiscoveredAssetId = 1, Name = "SRV-NEW-01" } };
        _mockDiscoveryService.Setup(s => s.GetPendingAssetsAsync()).ReturnsAsync(assets);

        var result = await _controller.GetPendingDiscoveredAssets();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returned = okResult.Value.Should().BeAssignableTo<IEnumerable<DiscoveredAsset>>().Subject;
        returned.Should().HaveCount(1);
    }

    // ────────────────────────────────────────────────────────────────
    // POST /discovery/import
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ImportDiscoveredAssets_ShouldReturnOk()
    {
        var importResult = new CmdbImportResult { TotalProcessed = 2, Created = 2 };
        _mockDiscoveryService
            .Setup(s => s.ImportAssetsAsync(It.Is<List<int>>(l => l.SequenceEqual(new List<int> { 1, 2 })), 1))
            .ReturnsAsync(importResult);

        var request = new ImportDiscoveredAssetsRequest { AssetIds = new List<int> { 1, 2 } };
        var result = await _controller.ImportDiscoveredAssets(request);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(importResult);
    }

    // ────────────────────────────────────────────────────────────────
    // POST /discovery/scan/{scanId}/reconcile
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ReconcileDiscoveredAssets_ShouldReturnOk_WhenScanExists()
    {
        var reconciliation = new ReconciliationResult { TotalAssets = 5, ExactMatches = 3 };
        _mockDiscoveryService.Setup(s => s.ReconcileAssetsAsync(1)).ReturnsAsync(reconciliation);

        var result = await _controller.ReconcileDiscoveredAssets(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(reconciliation);
    }

    [Fact]
    public async Task ReconcileDiscoveredAssets_ShouldReturnNotFound_WhenScanMissing()
    {
        _mockDiscoveryService.Setup(s => s.ReconcileAssetsAsync(999))
            .ThrowsAsync(new ArgumentException("Scan 999 not found"));

        var result = await _controller.ReconcileDiscoveredAssets(999);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /discovery/schedules
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetDiscoverySchedules_ShouldReturnOk()
    {
        var schedules = new List<DiscoverySchedule> { new() { ScheduleId = 1, Name = "Weekly Network Scan" } };
        _mockDiscoveryService.Setup(s => s.GetSchedulesAsync()).ReturnsAsync(schedules);

        var result = await _controller.GetDiscoverySchedules();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returned = okResult.Value.Should().BeAssignableTo<IEnumerable<DiscoverySchedule>>().Subject;
        returned.Should().HaveCount(1);
    }

    // ────────────────────────────────────────────────────────────────
    // POST /discovery/schedules
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SaveDiscoverySchedule_ShouldReturnOk()
    {
        var saved = new DiscoverySchedule { ScheduleId = 1, Name = "Weekly Network Scan", Type = DiscoveryType.NetworkScan, Target = "10.0.0.0/8" };
        _mockDiscoveryService
            .Setup(s => s.SaveScheduleAsync(It.Is<DiscoverySchedule>(d => d.Name == "Weekly Network Scan")))
            .ReturnsAsync(saved);

        var request = new SaveDiscoveryScheduleRequest
        {
            Name = "Weekly Network Scan",
            Type = DiscoveryType.NetworkScan,
            Target = "10.0.0.0/8",
            CronExpression = "0 0 2 * * SUN",
            IsActive = true
        };
        var result = await _controller.SaveDiscoverySchedule(request);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(saved);
    }
}
