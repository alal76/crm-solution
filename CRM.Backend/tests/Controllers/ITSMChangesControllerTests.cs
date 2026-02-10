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

public class ITSMChangesControllerTests
{
    private readonly Mock<IChangeManagementService> _mockService;
    private readonly ChangesController _controller;

    public ITSMChangesControllerTests()
    {
        _mockService = new Mock<IChangeManagementService>();
        _controller = new ChangesController(_mockService.Object);

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
    public async Task CreateChange_ShouldReturnCreatedAtAction()
    {
        var createDto = new CreateChangeDto { ShortDescription = "Upgrade DB", Type = ChangeType.Normal };
        var created = new ChangeDto { ChangeId = 1, ShortDescription = "Upgrade DB" };
        _mockService.Setup(s => s.CreateChangeAsync(createDto, 1)).ReturnsAsync(created);

        var result = await _controller.CreateChange(createDto);

        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(ChangesController.GetChange));
        createdResult.Value.Should().Be(created);
    }

    // ────────────────────────────────────────────────────────────────
    // GET /{id}
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetChange_ShouldReturnOk_WhenChangeExists()
    {
        var change = new ChangeDto { ChangeId = 1, ShortDescription = "Migrate servers" };
        _mockService.Setup(s => s.GetChangeByIdAsync(1)).ReturnsAsync(change);

        var result = await _controller.GetChange(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(change);
    }

    [Fact]
    public async Task GetChange_ShouldReturnNotFound_WhenChangeDoesNotExist()
    {
        _mockService.Setup(s => s.GetChangeByIdAsync(999)).ReturnsAsync((ChangeDto?)null);

        var result = await _controller.GetChange(999);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetChanges_ShouldReturnOkWithPagedResult()
    {
        var items = new List<ChangeDto>
        {
            new() { ChangeId = 1, ShortDescription = "Change A" },
            new() { ChangeId = 2, ShortDescription = "Change B" }
        };
        _mockService
            .Setup(s => s.GetChangesAsync(It.IsAny<ChangeFilterDto>()))
            .ReturnsAsync((items.AsEnumerable(), 2));

        var result = await _controller.GetChanges(null, 1, 20);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var paged = okResult.Value.Should().BeOfType<PagedResult<ChangeDto>>().Subject;
        paged.Items.Should().HaveCount(2);
        paged.TotalCount.Should().Be(2);
    }

    // ────────────────────────────────────────────────────────────────
    // PATCH /{id}/submit-approval
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SubmitForApproval_ShouldReturnOk()
    {
        _mockService.Setup(s => s.SubmitForApprovalAsync(1, 1)).ReturnsAsync(true);

        var result = await _controller.SubmitForApproval(1);

        result.Should().BeOfType<OkResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // POST /{id}/approvals
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ApproveChange_ShouldReturnOk()
    {
        _mockService.Setup(s => s.ApproveChangeAsync(1, 1, "LGTM")).ReturnsAsync(true);

        var dto = new ApproveChangeDto { Comments = "LGTM" };
        var result = await _controller.ApproveChange(1, dto);

        result.Should().BeOfType<OkResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // POST /{id}/rejections
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task RejectChange_ShouldReturnOk()
    {
        _mockService.Setup(s => s.RejectChangeAsync(1, 1, "Too risky")).ReturnsAsync(true);

        var dto = new ApproveChangeDto { Comments = "Too risky" };
        var result = await _controller.RejectChange(1, dto);

        result.Should().BeOfType<OkResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // PATCH /{id}/schedule
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ScheduleChange_ShouldReturnOk()
    {
        var start = DateTime.UtcNow.AddDays(1);
        var end = DateTime.UtcNow.AddDays(2);
        _mockService.Setup(s => s.ScheduleChangeAsync(1, start, end, 1)).ReturnsAsync(true);

        var dto = new ScheduleChangeDto { ScheduledStart = start, ScheduledEnd = end };
        var result = await _controller.ScheduleChange(1, dto);

        result.Should().BeOfType<OkResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // POST /{changeId}/impacted-cis/{ciId}
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddImpactedCI_ShouldReturnOk_WhenSuccessful()
    {
        _mockService.Setup(s => s.AddImpactedCIAsync(1, 5, 1)).ReturnsAsync(true);

        var result = await _controller.AddImpactedCI(1, 5);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task AddImpactedCI_ShouldReturnBadRequest_WhenFails()
    {
        _mockService.Setup(s => s.AddImpactedCIAsync(1, 999, 1)).ReturnsAsync(false);

        var result = await _controller.AddImpactedCI(1, 999);

        result.Should().BeOfType<BadRequestResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /{id}/impacted-cis
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetImpactedCIs_ShouldReturnOk()
    {
        var cis = new List<ConfigurationItemDto> { new() { CIId = 5, CIName = "ImpactedServer" } };
        _mockService.Setup(s => s.GetImpactedCIsAsync(1)).ReturnsAsync(cis);

        var result = await _controller.GetImpactedCIs(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returned = okResult.Value.Should().BeAssignableTo<IEnumerable<ConfigurationItemDto>>().Subject;
        returned.Should().HaveCount(1);
    }

    // ────────────────────────────────────────────────────────────────
    // POST /{id}/check-conflicts
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CheckConflicts_ShouldReturnOk_WithBoolResult()
    {
        _mockService.Setup(s => s.CheckConflictsAsync(1)).ReturnsAsync(true);

        var result = await _controller.CheckConflicts(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(true);
    }

    // ────────────────────────────────────────────────────────────────
    // GET /blackouts
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetBlackoutPeriods_ShouldReturnOk()
    {
        var blackouts = new List<BlackoutPeriodInfo>
        {
            new() { BlackoutPeriodId = 1, Name = "Year-end freeze", IsActive = true }
        };
        _mockService
            .Setup(s => s.GetBlackoutPeriodsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(blackouts);

        var result = await _controller.GetBlackoutPeriods(null, null);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
    }

    // ────────────────────────────────────────────────────────────────
    // POST /blackouts
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateBlackoutPeriod_ShouldReturnOk_WithMappedDto()
    {
        var info = new BlackoutPeriodInfo
        {
            BlackoutPeriodId = 1,
            Name = "Holiday freeze",
            Reason = "Year-end",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(14),
            IsActive = true
        };
        _mockService
            .Setup(s => s.CreateBlackoutPeriodAsync(It.IsAny<CreateBlackoutPeriodInfo>(), 1))
            .ReturnsAsync(info);

        var dto = new CreateBlackoutPeriodDto
        {
            Name = "Holiday freeze",
            Reason = "Year-end",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(14)
        };
        var result = await _controller.CreateBlackoutPeriod(dto);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returned = okResult.Value.Should().BeOfType<BlackoutPeriodDto>().Subject;
        returned.Name.Should().Be("Holiday freeze");
        returned.BlackoutPeriodId.Should().Be(1);
    }

    // ────────────────────────────────────────────────────────────────
    // GET /calendar
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetChangeCalendar_ShouldReturnOk_WithChangesAndBlackouts()
    {
        var changes = new List<ChangeDto> { new() { ChangeId = 1, ShortDescription = "Scheduled upgrade" } };
        var blackouts = new List<BlackoutPeriodInfo>
        {
            new() { BlackoutPeriodId = 1, Name = "Freeze", IsActive = true }
        };
        _mockService
            .Setup(s => s.GetChangesAsync(It.IsAny<ChangeFilterDto>()))
            .ReturnsAsync((changes.AsEnumerable(), 1));
        _mockService
            .Setup(s => s.GetBlackoutPeriodsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(blackouts);

        var result = await _controller.GetChangeCalendar(null, null);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var calendar = okResult.Value.Should().BeOfType<ChangeCalendarDto>().Subject;
        calendar.Changes.Should().HaveCount(1);
        calendar.Blackouts.Should().HaveCount(1);
    }
}
