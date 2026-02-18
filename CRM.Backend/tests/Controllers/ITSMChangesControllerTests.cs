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
using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace CRM.Tests.Controllers;

public class ITSMChangesControllerTests
{
    private readonly Mock<IChangeService> _mockService;
    private readonly Mock<ILogger<ChangesController>> _mockLogger;
    private readonly ChangesController _controller;

    public ITSMChangesControllerTests()
    {
        _mockService = new Mock<IChangeService>();
        _mockLogger = new Mock<ILogger<ChangesController>>();
        _controller = new ChangesController(_mockService.Object, _mockLogger.Object);

        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "1") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    // ────────────────────────────────────────────────────────────────
    // GET /
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ShouldReturnOkWithPagedResult()
    {
        var paginatedResult = new PaginatedDto<ChangeDto>
        {
            Items = new List<ChangeDto>
            {
                new() { Id = 1, Title = "Change A" },
                new() { Id = 2, Title = "Change B" }
            },
            TotalCount = 2,
            Page = 1,
            PageSize = 20
        };
        _mockService
            .Setup(s => s.GetAllAsync(1, 20, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paginatedResult);

        var result = await _controller.GetAll(1, 20, null);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var paged = okResult.Value.Should().BeOfType<PaginatedDto<ChangeDto>>().Subject;
        paged.Items.Should().HaveCount(2);
        paged.TotalCount.Should().Be(2);
    }

    // ────────────────────────────────────────────────────────────────
    // GET /{id}
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenChangeExists()
    {
        var change = new ChangeDto { Id = 1, Title = "Migrate servers" };
        _mockService.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(change);

        var result = await _controller.GetById(1);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(change);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenChangeDoesNotExist()
    {
        _mockService.Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((ChangeDto?)null);

        var result = await _controller.GetById(999);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // POST /
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_ShouldReturnCreatedAtAction()
    {
        var createDto = new CreateChangeDto { Title = "Upgrade DB" };
        var created = new ChangeDto { Id = 1, Title = "Upgrade DB" };
        _mockService.Setup(s => s.CreateAsync(createDto, It.IsAny<CancellationToken>())).ReturnsAsync(created);

        var result = await _controller.Create(createDto);

        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(ChangesController.GetById));
        createdResult.Value.Should().Be(created);
    }

    // ────────────────────────────────────────────────────────────────
    // POST /{id}/submit
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Submit_ShouldReturnOk_WhenChangeExists()
    {
        var change = new ChangeDto { Id = 1, Title = "Upgrade DB", Status = "Submitted" };
        _mockService.Setup(s => s.SubmitAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(change);

        var result = await _controller.Submit(1);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(change);
    }

    [Fact]
    public async Task Submit_ShouldReturnNotFound_WhenChangeDoesNotExist()
    {
        _mockService.Setup(s => s.SubmitAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((ChangeDto?)null);

        var result = await _controller.Submit(999);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // POST /{id}/approve
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Approve_ShouldReturnOk_WhenChangeExists()
    {
        var change = new ChangeDto { Id = 1, Title = "Upgrade DB", Status = "Approved" };
        var dto = new ChangeApprovalDto { ApproverNotes = "LGTM" };
        _mockService.Setup(s => s.ApproveAsync(1, dto, It.IsAny<CancellationToken>())).ReturnsAsync(change);

        var result = await _controller.Approve(1, dto);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(change);
    }

    [Fact]
    public async Task Approve_ShouldReturnNotFound_WhenChangeDoesNotExist()
    {
        var dto = new ChangeApprovalDto { ApproverNotes = "LGTM" };
        _mockService.Setup(s => s.ApproveAsync(999, dto, It.IsAny<CancellationToken>())).ReturnsAsync((ChangeDto?)null);

        var result = await _controller.Approve(999, dto);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // POST /{id}/reject
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Reject_ShouldReturnOk_WhenChangeExists()
    {
        var change = new ChangeDto { Id = 1, Title = "Upgrade DB", Status = "Rejected" };
        var dto = new ChangeRejectionDto { RejectionReason = "Too risky" };
        _mockService.Setup(s => s.RejectAsync(1, dto, It.IsAny<CancellationToken>())).ReturnsAsync(change);

        var result = await _controller.Reject(1, dto);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(change);
    }

    [Fact]
    public async Task Reject_ShouldReturnNotFound_WhenChangeDoesNotExist()
    {
        var dto = new ChangeRejectionDto { RejectionReason = "Too risky" };
        _mockService.Setup(s => s.RejectAsync(999, dto, It.IsAny<CancellationToken>())).ReturnsAsync((ChangeDto?)null);

        var result = await _controller.Reject(999, dto);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

#if false
    // TODO: Controller methods not implemented - these tests are for future ITSM enhancements
    // The following methods do not exist on ChangesController:
    // - ScheduleChange, AddImpactedCI, GetImpactedCIs, CheckConflicts
    // - GetBlackoutPeriods, CreateBlackoutPeriod, GetChangeCalendar

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
#endif
}
