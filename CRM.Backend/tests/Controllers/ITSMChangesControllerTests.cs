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

/// <summary>
/// Tests for <see cref="ChangesController"/>, which is backed by
/// <see cref="IChangeManagementServiceEx"/> and shaped to match
/// <c>CRM.Frontend/src/services/changeService.ts</c>. Also covers the
/// <see cref="IChangeCalendarService"/> scheduling-assistant endpoints and the
/// <see cref="IChangeImpactService"/> impact-analysis endpoints.
/// </summary>
public class ITSMChangesControllerTests
{
    private readonly Mock<IChangeManagementServiceEx> _mockService;
    private readonly Mock<IChangeCalendarService> _mockCalendarService;
    private readonly Mock<IChangeImpactService> _mockImpactService;
    private readonly Mock<ILogger<ChangesController>> _mockLogger;
    private readonly ChangesController _controller;

    public ITSMChangesControllerTests()
    {
        _mockService = new Mock<IChangeManagementServiceEx>();
        _mockCalendarService = new Mock<IChangeCalendarService>();
        _mockImpactService = new Mock<IChangeImpactService>();
        _mockLogger = new Mock<ILogger<ChangesController>>();
        _controller = new ChangesController(_mockService.Object, _mockCalendarService.Object, _mockImpactService.Object, _mockLogger.Object);

        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "1") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    private static ChangeDto MakeChangeDto(
        int id = 1,
        ChangeState state = ChangeState.New,
        ChangeType type = ChangeType.Normal,
        ChangeRisk risk = ChangeRisk.Medium) => new()
    {
        ChangeId = id,
        Number = $"CHG{id:D7}",
        ShortDescription = "Test change",
        Description = "Test description",
        Type = type,
        State = state,
        ApprovalStatus = ApprovalStatus.Requested,
        Risk = risk,
        Impact = ChangeImpact.Medium,
        PlannedStartDate = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
        PlannedEndDate = new DateTime(2026, 3, 2, 0, 0, 0, DateTimeKind.Utc),
        RequestorId = 1,
        RequestorName = "tester",
        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    };

    private static ChangeApprovalDto MakeApprovalDto(int changeId = 1, ApprovalStatus status = ApprovalStatus.Approved) => new()
    {
        ApprovalId = 10,
        ChangeId = changeId,
        ApproverId = 2,
        ApproverName = "approver",
        ApprovalRole = "ChangeManager",
        ApprovalStatus = status,
        ApprovalDate = DateTime.UtcNow,
        Comments = "LGTM",
        CreatedAt = DateTime.UtcNow
    };

    // ────────────────────────────────────────────────────────────────
    // GET /
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ShouldReturnOkWithPagedResult()
    {
        var changes = new List<ChangeDto> { MakeChangeDto(1), MakeChangeDto(2) };
        _mockService
            .Setup(s => s.ListChangesAsync(It.IsAny<ChangeFilterDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((changes.AsEnumerable(), 2));

        var result = await _controller.GetAll(1, 20);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var paged = okResult.Value.Should().BeOfType<PagedChangeResponse>().Subject;
        paged.Items.Should().HaveCount(2);
        paged.TotalCount.Should().Be(2);
        paged.Page.Should().Be(1);
        paged.PageSize.Should().Be(20);
    }

    // ────────────────────────────────────────────────────────────────
    // GET /{id}
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenChangeExists()
    {
        var change = MakeChangeDto(1);
        _mockService.Setup(s => s.GetChangeByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(change);
        _mockService.Setup(s => s.GetApprovalsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ChangeApprovalDto> { MakeApprovalDto(1) });
        _mockService.Setup(s => s.AnalyzeChangeImpactAsync(1, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChangeImpactDto
            {
                ChangeId = 1,
                ChangeNumber = change.Number,
                ImpactedServices = new List<ImpactedServiceDto> { new() { CIId = 5, CIName = "Server-01" } }
            });

        var result = await _controller.GetById(1);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = okResult.Value.Should().BeOfType<ChangeResponse>().Subject;
        body.Id.Should().Be(1);
        body.Title.Should().Be("Test change");
        body.AffectedServices.Should().ContainSingle().Which.Should().Be("Server-01");
        body.Approvals.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenChangeDoesNotExist()
    {
        _mockService.Setup(s => s.GetChangeByIdAsync(999, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Change 999 not found"));

        var result = await _controller.GetById(999);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // POST /
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_ShouldReturnCreatedAtAction()
    {
        var created = MakeChangeDto(1);
        _mockService
            .Setup(s => s.CreateChangeAsync(It.IsAny<CreateChangeDto>(), 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        var request = new CreateChangeApiRequest
        {
            Title = "Upgrade DB",
            Description = "Upgrade primary DB",
            Priority = 0, // Critical
            RiskLevel = 2, // High
            StartDate = new DateTime(2026, 3, 1),
            EndDate = new DateTime(2026, 3, 2),
            ImpactAnalysis = "High impact",
            RollbackPlan = "Restore from backup"
        };

        var result = await _controller.Create(request);

        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(ChangesController.GetById));
        var body = createdResult.Value.Should().BeOfType<ChangeResponse>().Subject;
        body.Id.Should().Be(1);

        _mockService.Verify(s => s.CreateChangeAsync(
            It.Is<CreateChangeDto>(d => d.Type == ChangeType.Emergency && d.Risk == ChangeRisk.High),
            1,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // ────────────────────────────────────────────────────────────────
    // PUT /{id}
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Update_ShouldReturnOk_WhenChangeExists()
    {
        var existing = MakeChangeDto(1);
        var updated = MakeChangeDto(1);
        updated.ShortDescription = "Renamed change";

        _mockService.Setup(s => s.GetChangeByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        _mockService
            .Setup(s => s.UpdateChangeAsync(1, It.IsAny<CreateChangeDto>(), 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updated);

        var request = new UpdateChangeApiRequest { Title = "Renamed change" };
        var result = await _controller.Update(1, request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = okResult.Value.Should().BeOfType<ChangeResponse>().Subject;
        body.Title.Should().Be("Renamed change");
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenChangeDoesNotExist()
    {
        _mockService.Setup(s => s.GetChangeByIdAsync(999, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Change 999 not found"));

        var result = await _controller.Update(999, new UpdateChangeApiRequest { Title = "X" });

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Update_ShouldApplyStatusTransition_WhenStatusProvided()
    {
        var existing = MakeChangeDto(1, state: ChangeState.New);
        var afterFieldUpdate = MakeChangeDto(1, state: ChangeState.New);
        var afterTransition = MakeChangeDto(1, state: ChangeState.Assess);

        _mockService.Setup(s => s.GetChangeByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _mockService
            .Setup(s => s.UpdateChangeAsync(1, It.IsAny<CreateChangeDto>(), 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(afterFieldUpdate);
        _mockService
            .Setup(s => s.RequestApprovalAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(afterTransition);

        // status = 1 (SubmittedForApproval)
        var request = new UpdateChangeApiRequest { Status = 1 };
        var result = await _controller.Update(1, request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = okResult.Value.Should().BeOfType<ChangeResponse>().Subject;
        body.Status.Should().Be(1); // SubmittedForApproval
        _mockService.Verify(s => s.RequestApprovalAsync(1, 1, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ────────────────────────────────────────────────────────────────
    // DELETE /{id}
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenChangeExists()
    {
        _mockService.Setup(s => s.DeleteChangeAsync(1, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _controller.Delete(1);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenChangeDoesNotExist()
    {
        _mockService.Setup(s => s.DeleteChangeAsync(999, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Change 999 not found"));

        var result = await _controller.Delete(999);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // PATCH /{id}/status
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ChangeStatus_ShouldReturnOk_ForApprovedTransition()
    {
        var approved = MakeChangeDto(1, state: ChangeState.Approved);
        _mockService.Setup(s => s.ApproveChangeAsync(1, 1, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeApprovalDto(1));
        _mockService.Setup(s => s.GetChangeByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(approved);

        var result = await _controller.ChangeStatus(1, new ChangeStatusRequest { Status = 3 });

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = okResult.Value.Should().BeOfType<ChangeResponse>().Subject;
        body.Status.Should().Be(3); // Approved
    }

    [Fact]
    public async Task ChangeStatus_ShouldReturnBadRequest_ForUnsupportedTransition()
    {
        // status = 0 (Draft) has no direct backend transition
        var result = await _controller.ChangeStatus(1, new ChangeStatusRequest { Status = 0 });

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ChangeStatus_ShouldReturnNotFound_WhenChangeDoesNotExist()
    {
        _mockService.Setup(s => s.StartImplementationAsync(999, 1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Change 999 not found"));

        var result = await _controller.ChangeStatus(999, new ChangeStatusRequest { Status = 6 });

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // POST /{id}/submit-approval
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SubmitForApproval_ShouldReturnOk_WithoutApprovers()
    {
        var afterSubmit = MakeChangeDto(1, state: ChangeState.Assess);
        _mockService.Setup(s => s.RequestApprovalAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(afterSubmit);

        var result = await _controller.SubmitForApproval(1, new SubmitApprovalRequest { Approvers = new List<int>() });

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<ChangeResponse>();
        _mockService.Verify(s => s.CreateCABAsync(It.IsAny<CreateCABDto>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SubmitForApproval_ShouldCreateCAB_WhenApproversProvided()
    {
        var afterSubmit = MakeChangeDto(1, state: ChangeState.Assess);
        var afterCab = MakeChangeDto(1, state: ChangeState.Assess);
        _mockService.Setup(s => s.RequestApprovalAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(afterSubmit);
        _mockService
            .Setup(s => s.CreateCABAsync(It.Is<CreateCABDto>(d => d.ChangeId == 1 && d.MemberIds.Count == 2), 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CABDto { CABId = 1, ChangeId = 1 });
        _mockService.Setup(s => s.GetChangeByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(afterCab);

        var result = await _controller.SubmitForApproval(1, new SubmitApprovalRequest { Approvers = new List<int> { 2, 3 } });

        result.Should().BeOfType<OkObjectResult>();
        _mockService.Verify(s => s.CreateCABAsync(It.IsAny<CreateCABDto>(), 1, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ────────────────────────────────────────────────────────────────
    // POST /{id}/approve, /reject
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Approve_ShouldReturnOk_WhenChangeExists()
    {
        var approval = MakeApprovalDto(1, ApprovalStatus.Approved);
        _mockService.Setup(s => s.ApproveChangeAsync(1, 1, "LGTM", It.IsAny<CancellationToken>())).ReturnsAsync(approval);

        var result = await _controller.Approve(1, new ApproveChangeApiRequest { Comments = "LGTM" });

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = okResult.Value.Should().BeOfType<ChangeApprovalResponse>().Subject;
        body.Status.Should().Be(1); // frontend ApprovalStatus.Approved
        body.Comments.Should().Be("LGTM");
    }

    [Fact]
    public async Task Approve_ShouldReturnNotFound_WhenChangeDoesNotExist()
    {
        _mockService.Setup(s => s.ApproveChangeAsync(999, 1, It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Change 999 not found"));

        var result = await _controller.Approve(999, new ApproveChangeApiRequest());

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Reject_ShouldReturnOk_WhenChangeExists()
    {
        var approval = MakeApprovalDto(1, ApprovalStatus.Rejected);
        _mockService.Setup(s => s.RejectChangeAsync(1, 1, "Too risky", It.IsAny<CancellationToken>())).ReturnsAsync(approval);

        var result = await _controller.Reject(1, new RejectChangeApiRequest { Reason = "Too risky" });

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = okResult.Value.Should().BeOfType<ChangeApprovalResponse>().Subject;
        body.Status.Should().Be(2); // frontend ApprovalStatus.Rejected
    }

    // ────────────────────────────────────────────────────────────────
    // POST /{id}/schedule, /start, /complete, /rollback
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Schedule_ShouldReturnOk()
    {
        var scheduled = MakeChangeDto(1, state: ChangeState.Scheduled);
        _mockService
            .Setup(s => s.ScheduleChangeAsync(1, It.IsAny<ScheduleChangeImplementationDto>(), 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(scheduled);

        var request = new ScheduleChangeApiRequest { StartDate = DateTime.UtcNow.AddDays(1), EndDate = DateTime.UtcNow.AddDays(2) };
        var result = await _controller.Schedule(1, request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = okResult.Value.Should().BeOfType<ChangeResponse>().Subject;
        body.Status.Should().Be(5); // Scheduled
    }

    [Fact]
    public async Task Schedule_ShouldReturnBadRequest_OnBlackoutConflict()
    {
        _mockService
            .Setup(s => s.ScheduleChangeAsync(1, It.IsAny<ScheduleChangeImplementationDto>(), 1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Scheduling conflicts with blackout periods: Freeze"));

        var request = new ScheduleChangeApiRequest { StartDate = DateTime.UtcNow.AddDays(1), EndDate = DateTime.UtcNow.AddDays(2) };
        var result = await _controller.Schedule(1, request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task StartImplementation_ShouldReturnOk()
    {
        var started = MakeChangeDto(1, state: ChangeState.Implement);
        _mockService.Setup(s => s.StartImplementationAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync(started);

        var result = await _controller.StartImplementation(1);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = okResult.Value.Should().BeOfType<ChangeResponse>().Subject;
        body.Status.Should().Be(6); // InProgress
    }

    [Fact]
    public async Task Complete_ShouldReturnOk()
    {
        var completed = MakeChangeDto(1, state: ChangeState.Review);
        _mockService.Setup(s => s.CompleteChangeAsync(1, true, "Done", 1, It.IsAny<CancellationToken>())).ReturnsAsync(completed);

        var result = await _controller.Complete(1, new CompleteChangeApiRequest { Comments = "Done" });

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = okResult.Value.Should().BeOfType<ChangeResponse>().Subject;
        body.Status.Should().Be(8); // Completed
    }

    [Fact]
    public async Task Rollback_ShouldReturnOk()
    {
        var rolledBack = MakeChangeDto(1, state: ChangeState.Failed);
        _mockService
            .Setup(s => s.ExecuteRollbackAsync(1, It.Is<ExecuteRollbackDto>(d => d.Reason == "Bad deploy"), 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rolledBack);

        var result = await _controller.Rollback(1, new RollbackChangeApiRequest { Reason = "Bad deploy" });

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = okResult.Value.Should().BeOfType<ChangeResponse>().Subject;
        body.Status.Should().Be(9); // Rolled_Back
    }

    // ────────────────────────────────────────────────────────────────
    // GET /{id}/activity, GET/POST /{id}/comments
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetActivity_ShouldReturnOk_WithMergedApprovalsAndComments()
    {
        _mockService.Setup(s => s.GetChangeByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(MakeChangeDto(1));
        _mockService.Setup(s => s.GetApprovalsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ChangeApprovalDto> { MakeApprovalDto(1) });
        _mockService.Setup(s => s.GetCommentsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ChangeCommentDto>
            {
                new() { CommentId = 5, ChangeId = 1, Comment = "note", CreatedById = 1, CreatedAt = DateTime.UtcNow }
            });

        var result = await _controller.GetActivity(1);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = okResult.Value.Should().BeAssignableTo<IEnumerable<ChangeActivityResponse>>().Subject.ToList();
        body.Should().HaveCount(2);
        body.Select(a => a.Type).Should().Contain(new[] { "approval", "comment" });
    }

    [Fact]
    public async Task GetActivity_ShouldReturnNotFound_WhenChangeDoesNotExist()
    {
        _mockService.Setup(s => s.GetChangeByIdAsync(999, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Change 999 not found"));

        var result = await _controller.GetActivity(999);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetComments_ShouldReturnOk()
    {
        _mockService.Setup(s => s.GetCommentsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ChangeCommentDto>
            {
                new() { CommentId = 1, ChangeId = 1, Comment = "hello", CreatedById = 1, CreatedAt = DateTime.UtcNow }
            });

        var result = await _controller.GetComments(1);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = okResult.Value.Should().BeAssignableTo<IEnumerable<ChangeActivityResponse>>().Subject.ToList();
        body.Should().ContainSingle().Which.Content.Should().Be("hello");
    }

    [Fact]
    public async Task AddComment_ShouldReturnOk()
    {
        var comment = new ChangeCommentDto { CommentId = 7, ChangeId = 1, Comment = "new comment", CreatedById = 1, CreatedAt = DateTime.UtcNow };
        _mockService
            .Setup(s => s.AddCommentAsync(1, It.Is<CreateChangeCommentDto>(d => d.Comment == "new comment"), 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comment);

        var result = await _controller.AddComment(1, new AddChangeCommentApiRequest { Content = "new comment" });

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = okResult.Value.Should().BeOfType<ChangeActivityResponse>().Subject;
        body.Content.Should().Be("new comment");
        body.Type.Should().Be("comment");
    }

    // ────────────────────────────────────────────────────────────────
    // GET /calendar, /statistics, /{id}/conflicts, /{id}/related-*
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetCalendarEvents_ShouldReturnOk()
    {
        var changes = new List<ChangeDto> { MakeChangeDto(1, state: ChangeState.Scheduled) };
        _mockService
            .Setup(s => s.ListChangesAsync(It.IsAny<ChangeFilterDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((changes.AsEnumerable(), 1));

        var result = await _controller.GetCalendarEvents(DateTime.UtcNow, DateTime.UtcNow.AddDays(7));

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = okResult.Value.Should().BeAssignableTo<IEnumerable<ChangeCalendarEventResponse>>().Subject.ToList();
        body.Should().ContainSingle().Which.Status.Should().Be(5); // Scheduled
    }

    [Fact]
    public async Task GetStatistics_ShouldReturnOk()
    {
        _mockService
            .Setup(s => s.GetChangeMetricsAsync(null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChangeMetricsDto { TotalChanges = 10, SuccessfulChanges = 8, SuccessRate = 80.0 });
        _mockService
            .Setup(s => s.ListChangesAsync(It.IsAny<ChangeFilterDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enumerable.Empty<ChangeDto>(), 3));

        var result = await _controller.GetStatistics();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = okResult.Value.Should().BeOfType<ChangeStatisticsResponse>().Subject;
        body.TotalChanges.Should().Be(10);
        body.SuccessRate.Should().Be(80.0);
        body.ApprovedChanges.Should().Be(3);
        body.ScheduledChanges.Should().Be(3);
        body.CompletedChanges.Should().Be(3);
    }

    [Fact]
    public async Task DetectConflicts_ShouldReturnOk_WithMappedChanges()
    {
        var conflictCheck = new ChangeConflictCheckDto
        {
            ChangeId = 1,
            ConflictsDetected = true,
            ConflictingChanges = new List<ConflictingChangeDto>
            {
                new() { ChangeId = 2, ChangeNumber = "CHG0000002", ShortDescription = "Other change", ConflictReason = "Overlap" }
            }
        };
        _mockService.Setup(s => s.CheckConflictsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(conflictCheck);
        _mockService.Setup(s => s.GetChangeByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(MakeChangeDto(2));

        var result = await _controller.DetectConflicts(1);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = okResult.Value.Should().BeAssignableTo<IEnumerable<ChangeResponse>>().Subject.ToList();
        body.Should().ContainSingle().Which.Id.Should().Be(2);
    }

    [Fact]
    public async Task GetRelatedIncidents_ShouldReturnEmptyArray_WhenChangeExists()
    {
        _mockService.Setup(s => s.GetChangeByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(MakeChangeDto(1));

        var result = await _controller.GetRelatedIncidents(1);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeAssignableTo<IEnumerable<object>>().Which.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRelatedProblems_ShouldReturnEmptyArray_WhenChangeExists()
    {
        _mockService.Setup(s => s.GetChangeByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(MakeChangeDto(1));

        var result = await _controller.GetRelatedProblems(1);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeAssignableTo<IEnumerable<object>>().Which.Should().BeEmpty();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /available-slots, GET /maintenance-windows (IChangeCalendarService)
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAvailableSlots_ShouldReturnOk_WithSlots()
    {
        var slots = new List<AvailableSlot>
        {
            new() { StartTime = DateTime.UtcNow.AddDays(1), EndTime = DateTime.UtcNow.AddDays(1).AddHours(2), DurationMinutes = 120, Quality = SlotQuality.Optimal, IsMaintenanceWindow = true, MaintenanceWindowName = "Weekend Maintenance" }
        };
        _mockCalendarService
            .Setup(s => s.FindAvailableSlotsAsync(5, 120, 14))
            .ReturnsAsync(slots);

        var result = await _controller.GetAvailableSlots(5, 120, 14);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = okResult.Value.Should().BeAssignableTo<IEnumerable<AvailableSlot>>().Subject.ToList();
        body.Should().ContainSingle().Which.MaintenanceWindowName.Should().Be("Weekend Maintenance");
    }

    [Fact]
    public async Task GetAvailableSlots_ShouldReturnBadRequest_WhenServiceThrowsInvalidOperationException()
    {
        _mockCalendarService
            .Setup(s => s.FindAvailableSlotsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Invalid duration"));

        var result = await _controller.GetAvailableSlots(5, -1, 14);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetMaintenanceWindows_ShouldReturnOk_WithWindows()
    {
        var windows = new List<MaintenanceWindow>
        {
            new() { WindowId = 1, Name = "Weekend Maintenance (Saturday)", DayOfWeek = DayOfWeek.Saturday, IsActive = true }
        };
        _mockCalendarService.Setup(s => s.GetMaintenanceWindowsAsync()).ReturnsAsync(windows);

        var result = await _controller.GetMaintenanceWindows();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = okResult.Value.Should().BeAssignableTo<IEnumerable<MaintenanceWindow>>().Subject.ToList();
        body.Should().ContainSingle().Which.Name.Should().Be("Weekend Maintenance (Saturday)");
    }

    [Fact]
    public async Task GetMaintenanceWindows_ShouldReturnBadRequest_WhenServiceThrowsInvalidOperationException()
    {
        _mockCalendarService
            .Setup(s => s.GetMaintenanceWindowsAsync())
            .ThrowsAsync(new InvalidOperationException("boom"));

        var result = await _controller.GetMaintenanceWindows();

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /{id}/impact-analysis-full, POST /impact-analysis/impacted-cis,
    // GET /{id}/impacted-services, GET /{id}/risk-score, GET /{id}/impact-notifications
    // (IChangeImpactService)
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetImpactAnalysisFull_ShouldReturnOk_WhenChangeExists()
    {
        var analysis = new ChangeImpactAnalysis
        {
            ChangeRequestId = 1,
            ChangeNumber = "CHG0000001",
            ImpactSummary = "2 configuration items affected"
        };
        _mockImpactService.Setup(s => s.AnalyzeChangeImpactAsync(1)).ReturnsAsync(analysis);

        var result = await _controller.GetImpactAnalysisFull(1);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = okResult.Value.Should().BeOfType<ChangeImpactAnalysis>().Subject;
        body.ChangeNumber.Should().Be("CHG0000001");
    }

    [Fact]
    public async Task GetImpactAnalysisFull_ShouldReturnNotFound_WhenServiceThrowsArgumentException()
    {
        _mockImpactService
            .Setup(s => s.AnalyzeChangeImpactAsync(999))
            .ThrowsAsync(new ArgumentException("Change request 999 not found"));

        var result = await _controller.GetImpactAnalysisFull(999);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetImpactedCIs_ShouldReturnOk_WithCIs()
    {
        var cis = new List<ImpactedCI>
        {
            new() { ConfigurationItemId = 5, Name = "Server-01", ImpactLevel = ImpactLevel.Direct }
        };
        _mockImpactService
            .Setup(s => s.GetImpactedCIsAsync(It.Is<List<int>>(l => l.Contains(5)), 2))
            .ReturnsAsync(cis);

        var request = new GetImpactedCIsApiRequest { PrimaryCIIds = new List<int> { 5 }, MaxDepth = 2 };
        var result = await _controller.GetImpactedCIs(request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = okResult.Value.Should().BeAssignableTo<IEnumerable<ImpactedCI>>().Subject.ToList();
        body.Should().ContainSingle().Which.Name.Should().Be("Server-01");
    }

    [Fact]
    public async Task GetImpactedCIs_ShouldReturnBadRequest_WhenServiceThrowsInvalidOperationException()
    {
        _mockImpactService
            .Setup(s => s.GetImpactedCIsAsync(It.IsAny<List<int>>(), It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        var request = new GetImpactedCIsApiRequest { PrimaryCIIds = new List<int> { 5 } };
        var result = await _controller.GetImpactedCIs(request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetImpactedServices_ShouldReturnOk_WithServices()
    {
        var services = new List<ImpactedService>
        {
            new() { ServiceId = 1, ServiceName = "Billing Service" }
        };
        _mockImpactService.Setup(s => s.GetImpactedServicesAsync(1)).ReturnsAsync(services);

        var result = await _controller.GetImpactedServices(1);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = okResult.Value.Should().BeAssignableTo<IEnumerable<ImpactedService>>().Subject.ToList();
        body.Should().ContainSingle().Which.ServiceName.Should().Be("Billing Service");
    }

    [Fact]
    public async Task GetImpactedServices_ShouldReturnNotFound_WhenServiceThrowsKeyNotFoundException()
    {
        _mockImpactService
            .Setup(s => s.GetImpactedServicesAsync(999))
            .ThrowsAsync(new KeyNotFoundException("Change 999 not found"));

        var result = await _controller.GetImpactedServices(999);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetRiskScore_ShouldReturnOk_WithRiskAssessment()
    {
        var risk = new RiskAssessmentResult
        {
            OverallRiskScore = 42,
            RiskLevel = CRM.Infrastructure.Services.ITSM.RiskLevel.Medium
        };
        _mockImpactService.Setup(s => s.CalculateRiskScoreAsync(1)).ReturnsAsync(risk);

        var result = await _controller.GetRiskScore(1);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = okResult.Value.Should().BeOfType<RiskAssessmentResult>().Subject;
        body.OverallRiskScore.Should().Be(42);
    }

    [Fact]
    public async Task GetRiskScore_ShouldReturnNotFound_WhenServiceThrowsKeyNotFoundException()
    {
        _mockImpactService
            .Setup(s => s.CalculateRiskScoreAsync(999))
            .ThrowsAsync(new KeyNotFoundException("Change 999 not found"));

        var result = await _controller.GetRiskScore(999);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetImpactNotifications_ShouldReturnOk_WithNotifications()
    {
        var notifications = new List<ImpactNotification>
        {
            new() { RecipientType = "Requestor", RecipientName = "Jane Doe", Priority = NotificationPriority.Normal }
        };
        _mockImpactService.Setup(s => s.GetImpactNotificationsAsync(1)).ReturnsAsync(notifications);

        var result = await _controller.GetImpactNotifications(1);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = okResult.Value.Should().BeAssignableTo<IEnumerable<ImpactNotification>>().Subject.ToList();
        body.Should().ContainSingle().Which.RecipientName.Should().Be("Jane Doe");
    }

    [Fact]
    public async Task GetImpactNotifications_ShouldReturnNotFound_WhenServiceThrowsKeyNotFoundException()
    {
        _mockImpactService
            .Setup(s => s.GetImpactNotificationsAsync(999))
            .ThrowsAsync(new KeyNotFoundException("Change 999 not found"));

        var result = await _controller.GetImpactNotifications(999);

        result.Should().BeOfType<NotFoundObjectResult>();
    }
}
