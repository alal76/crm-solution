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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CRM.Api.Infrastructure;
using ChangeDto = CRM.Core.Dtos.ITSM.ChangeDto;

namespace CRM.Api.Controllers;

/// <summary>
/// API Controller for managing system changes and change management workflows.
/// Backed by <see cref="IChangeManagementServiceEx"/> — the full ITSM change lifecycle
/// implementation (CRUD, CAB, approvals, scheduling, implementation, rollback, conflicts,
/// blackout periods, comments and metrics). Response/request shapes on this controller are
/// intentionally aligned with <c>CRM.Frontend/src/services/changeService.ts</c> so the
/// frontend does not need any changes.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChangesController : CrmControllerBase
{
    private readonly IChangeManagementServiceEx _service;
    private readonly ILogger<ChangesController> _logger;

    /// <summary>
    /// Initializes a new instance of the ChangesController.
    /// </summary>
    public ChangesController(
        IChangeManagementServiceEx service,
        ILogger<ChangesController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ────────────────────────────────────────────────────────────────
    // CRUD
    // ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Gets all changes with pagination and filtering.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedChangeResponse), StatusCodes.Status200OK)]
    public Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int? status = null,
        [FromQuery] int? priority = null,
        [FromQuery] int? riskLevel = null,
        [FromQuery] int? implementedById = null,
        [FromQuery] string? search = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default) => ExecuteAsync(async () =>
    {
        _logger.LogInformation("Getting all changes: page={Page}, pageSize={PageSize}, status={Status}", page, pageSize, status);

        // NOTE: riskLevel/implementedById are accepted for frontend contract compatibility but
        // ChangeFilterDto currently has no backing field for them, so they are not yet filtered on.
        var filter = new ChangeFilterDto
        {
            SearchTerm = search,
            State = status.HasValue ? ChangeContractMapper.ToBackendState(status.Value) : null,
            Type = priority.HasValue ? ChangeContractMapper.ToBackendType(priority.Value) : null,
            PlannedStartFrom = startDate,
            PlannedStartTo = endDate,
            PageNumber = page,
            PageSize = pageSize
        };

        var (items, totalCount) = await _service.ListChangesAsync(filter, cancellationToken);

        var result = new PagedChangeResponse
        {
            Items = items.Select(i => MapToResponse(i)).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = pageSize > 0 ? (int)Math.Ceiling(totalCount / (double)pageSize) : 0
        };

        return Ok(result);
    });

    /// <summary>
    /// Gets a specific change by ID, including its approvals and affected services.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ChangeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetById(int id, CancellationToken cancellationToken = default) => ExecuteAsync(async () =>
    {
        _logger.LogInformation("Getting change: id={Id}", id);

        var change = await _service.GetChangeByIdAsync(id, cancellationToken);
        var approvals = await _service.GetApprovalsAsync(id, cancellationToken);
        var impact = await _service.AnalyzeChangeImpactAsync(id, GetCurrentUserId(), cancellationToken);
        var affectedServices = impact.ImpactedServices.Select(s => s.CIName).ToList();

        return Ok(MapToResponse(change, approvals, affectedServices));
    });

    /// <summary>
    /// Creates a new change record.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ChangeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Create([FromBody] CreateChangeApiRequest request, CancellationToken cancellationToken = default) => ExecuteAsync(async () =>
    {
        _logger.LogInformation("Creating new change: {ChangeTitle}", request.Title);

        var dto = new CreateChangeDto
        {
            ShortDescription = request.Title,
            Description = request.Description,
            Type = ChangeContractMapper.ToBackendType(request.Priority),
            Risk = ChangeContractMapper.ToBackendRisk(request.RiskLevel),
            Impact = ChangeContractMapper.ToBackendImpact(request.RiskLevel),
            PlannedStartDate = request.StartDate,
            PlannedEndDate = request.EndDate,
            BackoutPlan = request.RollbackPlan,
            RiskAssessmentNotes = request.ImpactAnalysis
        };

        var created = await _service.CreateChangeAsync(dto, GetCurrentUserId(), cancellationToken);
        var response = MapToResponse(created);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    });

    /// <summary>
    /// Updates an existing change record. Only supplied fields are changed; a <c>status</c>
    /// field (if present) is applied as a lifecycle transition after the field update.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ChangeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Update(int id, [FromBody] UpdateChangeApiRequest request, CancellationToken cancellationToken = default) => ExecuteAsync(async () =>
    {
        _logger.LogInformation("Updating change: id={Id}", id);

        var existing = await _service.GetChangeByIdAsync(id, cancellationToken);
        var userId = GetCurrentUserId();

        var dto = new CreateChangeDto
        {
            ShortDescription = request.Title ?? existing.ShortDescription,
            Description = request.Description ?? existing.Description,
            Type = request.Priority.HasValue ? ChangeContractMapper.ToBackendType(request.Priority.Value) : existing.Type,
            Risk = request.RiskLevel.HasValue ? ChangeContractMapper.ToBackendRisk(request.RiskLevel.Value) : existing.Risk,
            Impact = request.RiskLevel.HasValue ? ChangeContractMapper.ToBackendImpact(request.RiskLevel.Value) : existing.Impact,
            PlannedStartDate = request.StartDate ?? existing.PlannedStartDate,
            PlannedEndDate = request.EndDate ?? existing.PlannedEndDate,
            ImplementationPlan = existing.ImplementationPlan,
            BackoutPlan = request.RollbackPlan ?? existing.BackoutPlan,
            TestingPlan = request.TestingPlan ?? existing.TestingPlan,
            RiskAssessmentNotes = request.ImpactAnalysis ?? existing.RiskAssessmentNotes
        };

        var updated = await _service.UpdateChangeAsync(id, dto, userId, cancellationToken);

        if (request.Status.HasValue)
        {
            updated = await ApplyStatusTransitionAsync(id, request.Status.Value, userId, cancellationToken) ?? updated;
        }

        return Ok(MapToResponse(updated));
    });

    /// <summary>
    /// Deletes a change record (soft delete).
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default) => ExecuteAsync(async () =>
    {
        _logger.LogInformation("Deleting change: id={Id}", id);
        await _service.DeleteChangeAsync(id, cancellationToken);
        return NoContent();
    });

    // ────────────────────────────────────────────────────────────────
    // Lifecycle / workflow
    // ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Generic status transition endpoint. Dispatches to the matching lifecycle action on
    /// <see cref="IChangeManagementServiceEx"/> based on the requested frontend status.
    /// </summary>
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(ChangeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeStatusRequest request, CancellationToken cancellationToken = default) => ExecuteAsync(async () =>
    {
        _logger.LogInformation("Changing status of change {Id} to {Status}", id, request.Status);

        var updated = await ApplyStatusTransitionAsync(id, request.Status, GetCurrentUserId(), cancellationToken);
        if (updated == null)
        {
            return BadRequest(new { message = $"Unsupported status transition: {request.Status}" });
        }

        return Ok(MapToResponse(updated));
    });

    /// <summary>
    /// Submits a change for approval, optionally registering CAB members.
    /// </summary>
    [HttpPost("{id:int}/submit-approval")]
    [ProducesResponseType(typeof(ChangeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> SubmitForApproval(int id, [FromBody] SubmitApprovalRequest request, CancellationToken cancellationToken = default) => ExecuteAsync(async () =>
    {
        _logger.LogInformation("Submitting change {Id} for approval with {ApproverCount} approver(s)", id, request.Approvers?.Count ?? 0);

        var userId = GetCurrentUserId();
        var updated = await _service.RequestApprovalAsync(id, userId, cancellationToken);

        if (request.Approvers?.Count > 0)
        {
            var cabDto = new CreateCABDto
            {
                ChangeId = id,
                ScheduledDate = DateTime.UtcNow,
                MemberIds = request.Approvers
            };
            await _service.CreateCABAsync(cabDto, userId, cancellationToken);
            updated = await _service.GetChangeByIdAsync(id, cancellationToken);
        }

        return Ok(MapToResponse(updated));
    });

    /// <summary>
    /// Approves a submitted change and records the approval decision.
    /// </summary>
    [HttpPost("{id:int}/approve")]
    [ProducesResponseType(typeof(ChangeApprovalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> Approve(int id, [FromBody] ApproveChangeApiRequest? request, CancellationToken cancellationToken = default) => ExecuteAsync(async () =>
    {
        _logger.LogInformation("Approving change: id={Id}", id);
        var approval = await _service.ApproveChangeAsync(id, GetCurrentUserId(), request?.Comments, cancellationToken);
        return Ok(MapToApprovalResponse(approval));
    });

    /// <summary>
    /// Rejects a submitted change and records the rejection decision.
    /// </summary>
    [HttpPost("{id:int}/reject")]
    [ProducesResponseType(typeof(ChangeApprovalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> Reject(int id, [FromBody] RejectChangeApiRequest request, CancellationToken cancellationToken = default) => ExecuteAsync(async () =>
    {
        _logger.LogInformation("Rejecting change: id={Id}, reason={Reason}", id, request.Reason);
        var approval = await _service.RejectChangeAsync(id, GetCurrentUserId(), request.Reason, cancellationToken);
        return Ok(MapToApprovalResponse(approval));
    });

    /// <summary>
    /// Schedules a change for implementation.
    /// </summary>
    [HttpPost("{id:int}/schedule")]
    [ProducesResponseType(typeof(ChangeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Schedule(int id, [FromBody] ScheduleChangeApiRequest request, CancellationToken cancellationToken = default) => ExecuteAsync(async () =>
    {
        _logger.LogInformation("Scheduling change {Id} from {Start} to {End}", id, request.StartDate, request.EndDate);
        var dto = new ScheduleChangeImplementationDto
        {
            ScheduledStartDate = request.StartDate,
            ScheduledEndDate = request.EndDate
        };
        var updated = await _service.ScheduleChangeAsync(id, dto, GetCurrentUserId(), cancellationToken);
        return Ok(MapToResponse(updated));
    });

    /// <summary>
    /// Starts implementation of a scheduled change.
    /// </summary>
    [HttpPost("{id:int}/start")]
    [ProducesResponseType(typeof(ChangeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> StartImplementation(int id, CancellationToken cancellationToken = default) => ExecuteAsync(async () =>
    {
        _logger.LogInformation("Starting implementation for change {Id}", id);
        var updated = await _service.StartImplementationAsync(id, GetCurrentUserId(), cancellationToken);
        return Ok(MapToResponse(updated));
    });

    /// <summary>
    /// Marks a change implementation as complete (successful).
    /// </summary>
    [HttpPost("{id:int}/complete")]
    [ProducesResponseType(typeof(ChangeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Complete(int id, [FromBody] CompleteChangeApiRequest? request, CancellationToken cancellationToken = default) => ExecuteAsync(async () =>
    {
        _logger.LogInformation("Completing change {Id}", id);
        var updated = await _service.CompleteChangeAsync(id, true, request?.Comments, GetCurrentUserId(), cancellationToken);
        return Ok(MapToResponse(updated));
    });

    /// <summary>
    /// Executes a rollback of a change.
    /// </summary>
    [HttpPost("{id:int}/rollback")]
    [ProducesResponseType(typeof(ChangeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> Rollback(int id, [FromBody] RollbackChangeApiRequest request, CancellationToken cancellationToken = default) => ExecuteAsync(async () =>
    {
        _logger.LogInformation("Rolling back change {Id}. Reason: {Reason}", id, request.Reason);
        var dto = new ExecuteRollbackDto { Reason = request.Reason };
        var updated = await _service.ExecuteRollbackAsync(id, dto, GetCurrentUserId(), cancellationToken);
        return Ok(MapToResponse(updated));
    });

    // ────────────────────────────────────────────────────────────────
    // Activity / comments
    // ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Gets a merged activity timeline (approvals + comments) for a change.
    /// </summary>
    [HttpGet("{id:int}/activity")]
    [ProducesResponseType(typeof(IEnumerable<ChangeActivityResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetActivity(int id, CancellationToken cancellationToken = default) => ExecuteAsync(async () =>
    {
        // Ensures the change exists (throws KeyNotFoundException -> 404) before building the feed.
        await _service.GetChangeByIdAsync(id, cancellationToken);

        var approvals = await _service.GetApprovalsAsync(id, cancellationToken);
        var comments = await _service.GetCommentsAsync(id, cancellationToken);

        var activity = new List<ChangeActivityResponse>();
        activity.AddRange(approvals.Select(a => new ChangeActivityResponse
        {
            Id = a.ApprovalId,
            ChangeId = id,
            Type = "approval",
            UserId = a.ApproverId,
            UserName = a.ApproverName,
            Content = string.IsNullOrEmpty(a.Comments) ? $"{a.ApprovalRole} — {a.ApprovalStatus}" : a.Comments,
            Timestamp = (a.ApprovalDate ?? a.CreatedAt).ToString("o")
        }));
        activity.AddRange(comments.Select(MapCommentToActivity));

        return Ok(activity.OrderByDescending(a => a.Timestamp).ToList());
    });

    /// <summary>
    /// Gets comments for a change (as activity entries).
    /// </summary>
    [HttpGet("{id:int}/comments")]
    [ProducesResponseType(typeof(IEnumerable<ChangeActivityResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetComments(int id, CancellationToken cancellationToken = default) => ExecuteAsync(async () =>
    {
        var comments = await _service.GetCommentsAsync(id, cancellationToken);
        return Ok(comments.Select(MapCommentToActivity).ToList());
    });

    /// <summary>
    /// Adds a comment to a change.
    /// </summary>
    [HttpPost("{id:int}/comments")]
    [ProducesResponseType(typeof(ChangeActivityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> AddComment(int id, [FromBody] AddChangeCommentApiRequest request, CancellationToken cancellationToken = default) => ExecuteAsync(async () =>
    {
        _logger.LogInformation("Adding comment to change {Id}", id);
        var dto = new CreateChangeCommentDto { Comment = request.Content, IsInternal = false };
        var comment = await _service.AddCommentAsync(id, dto, GetCurrentUserId(), cancellationToken);
        return Ok(MapCommentToActivity(comment));
    });

    // ────────────────────────────────────────────────────────────────
    // Calendar / statistics / conflicts / related records
    // ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Gets change calendar events for a date range (backed by planned start/end dates).
    /// </summary>
    [HttpGet("calendar")]
    [ProducesResponseType(typeof(IEnumerable<ChangeCalendarEventResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetCalendarEvents([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, CancellationToken cancellationToken = default) => ExecuteAsync(async () =>
    {
        var filter = new ChangeFilterDto
        {
            PlannedStartFrom = startDate,
            PlannedStartTo = endDate,
            PageNumber = 1,
            PageSize = 500
        };
        var (items, _) = await _service.ListChangesAsync(filter, cancellationToken);

        var events = items.Select(i => new ChangeCalendarEventResponse
        {
            Id = i.ChangeId,
            Title = i.ShortDescription,
            Start = ChangeContractMapper.ToIsoOrEmpty(i.PlannedStartDate),
            End = ChangeContractMapper.ToIsoOrEmpty(i.PlannedEndDate),
            Status = ChangeContractMapper.ToFrontendStatus(i.State),
            Priority = ChangeContractMapper.ToFrontendPriority(i.Type),
            RiskLevel = ChangeContractMapper.ToFrontendRiskLevel(i.Risk)
        }).ToList();

        return Ok(events);
    });

    /// <summary>
    /// Gets aggregate change statistics.
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(ChangeStatisticsResponse), StatusCodes.Status200OK)]
    public Task<IActionResult> GetStatistics(CancellationToken cancellationToken = default) => ExecuteAsync(async () =>
    {
        var metrics = await _service.GetChangeMetricsAsync(null, null, cancellationToken);

        var (_, approvedCount) = await _service.ListChangesAsync(
            new ChangeFilterDto { ApprovalStatus = ApprovalStatus.Approved, PageNumber = 1, PageSize = 1 }, cancellationToken);
        var (_, scheduledCount) = await _service.ListChangesAsync(
            new ChangeFilterDto { State = ChangeState.Scheduled, PageNumber = 1, PageSize = 1 }, cancellationToken);
        var (_, completedCount) = await _service.ListChangesAsync(
            new ChangeFilterDto { State = ChangeState.Closed, PageNumber = 1, PageSize = 1 }, cancellationToken);

        var stats = new ChangeStatisticsResponse
        {
            TotalChanges = metrics.TotalChanges,
            ApprovedChanges = approvedCount,
            ScheduledChanges = scheduledCount,
            CompletedChanges = completedCount,
            SuccessRate = metrics.SuccessRate,
            // Not tracked by the current data model: there is no approval-request timestamp
            // distinct from Change.CreatedAt, so a true "time to approval" cannot be computed yet.
            AvgApprovalTime = 0.0
        };

        return Ok(stats);
    });

    /// <summary>
    /// Detects other changes that conflict with this change's scheduled window or blackout periods.
    /// </summary>
    [HttpGet("{id:int}/conflicts")]
    [ProducesResponseType(typeof(IEnumerable<ChangeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> DetectConflicts(int id, CancellationToken cancellationToken = default) => ExecuteAsync(async () =>
    {
        var check = await _service.CheckConflictsAsync(id, cancellationToken);

        var conflicting = new List<ChangeResponse>();
        foreach (var conflict in check.ConflictingChanges)
        {
            var change = await _service.GetChangeByIdAsync(conflict.ChangeId, cancellationToken);
            conflicting.Add(MapToResponse(change));
        }

        return Ok(conflicting);
    });

    /// <summary>
    /// Gets incidents related to a change. No Change&lt;-&gt;Incident linkage table exists in the
    /// current data model, so this currently always returns an empty list once the change is confirmed to exist.
    /// </summary>
    [HttpGet("{id:int}/related-incidents")]
    [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetRelatedIncidents(int id, CancellationToken cancellationToken = default) => ExecuteAsync(async () =>
    {
        await _service.GetChangeByIdAsync(id, cancellationToken);
        return Ok(Array.Empty<object>());
    });

    /// <summary>
    /// Gets problems related to a change. No Change&lt;-&gt;Problem linkage table exists in the
    /// current data model, so this currently always returns an empty list once the change is confirmed to exist.
    /// </summary>
    [HttpGet("{id:int}/related-problems")]
    [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetRelatedProblems(int id, CancellationToken cancellationToken = default) => ExecuteAsync(async () =>
    {
        await _service.GetChangeByIdAsync(id, cancellationToken);
        return Ok(Array.Empty<object>());
    });

    // ────────────────────────────────────────────────────────────────
    // Helpers
    // ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Runs an action, translating <see cref="KeyNotFoundException"/> to 404 and
    /// <see cref="InvalidOperationException"/> (invalid lifecycle transitions) to 400.
    /// <c>ChangeManagementServiceEx</c>-family exceptions do not derive from
    /// <c>CRM.Core.Exceptions.CrmException</c>, so the global error middleware would otherwise
    /// surface them as 500s.
    /// </summary>
    private async Task<IActionResult> ExecuteAsync(Func<Task<IActionResult>> action)
    {
        try
        {
            return await action();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private int GetCurrentUserId() // NOSONAR
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return int.Parse(userIdClaim?.Value ?? "1");
    }

    /// <summary>
    /// Dispatches a frontend <c>ChangeStatus</c> value to the matching lifecycle action on
    /// <see cref="IChangeManagementServiceEx"/>. Returns null for transitions that have no safe
    /// direct equivalent (e.g. Draft, OnHold) or that are missing required data (e.g. Scheduled
    /// without planned dates already set).
    /// </summary>
    private async Task<ChangeDto?> ApplyStatusTransitionAsync(int id, int frontendStatus, int userId, CancellationToken cancellationToken)
    {
        switch (frontendStatus)
        {
            case 1: // SubmittedForApproval
            case 2: // ApprovalInProgress
                return await _service.RequestApprovalAsync(id, userId, cancellationToken);
            case 3: // Approved
                await _service.ApproveChangeAsync(id, userId, null, cancellationToken);
                return await _service.GetChangeByIdAsync(id, cancellationToken);
            case 4: // Rejected
                await _service.RejectChangeAsync(id, userId, "Status changed to Rejected", cancellationToken);
                return await _service.GetChangeByIdAsync(id, cancellationToken);
            case 5: // Scheduled
                {
                    var current = await _service.GetChangeByIdAsync(id, cancellationToken);
                    if (!current.PlannedStartDate.HasValue || !current.PlannedEndDate.HasValue)
                    {
                        return null;
                    }

                    var scheduleDto = new ScheduleChangeImplementationDto
                    {
                        ScheduledStartDate = current.PlannedStartDate.Value,
                        ScheduledEndDate = current.PlannedEndDate.Value
                    };
                    return await _service.ScheduleChangeAsync(id, scheduleDto, userId, cancellationToken);
                }

            case 6: // InProgress
                return await _service.StartImplementationAsync(id, userId, cancellationToken);
            case 8: // Completed
                return await _service.CompleteChangeAsync(id, true, null, userId, cancellationToken);
            case 9: // Rolled_Back
                return await _service.ExecuteRollbackAsync(
                    id, new ExecuteRollbackDto { Reason = "Status changed to Rolled_Back" }, userId, cancellationToken);
            case 10: // Cancelled
                return await _service.CancelChangeAsync(id, "Status changed to Cancelled", userId, cancellationToken);
            default: // Draft(0), OnHold(7), or unrecognized value: no safe direct transition
                return null;
        }
    }

    private static ChangeResponse MapToResponse(
        ChangeDto dto,
        IEnumerable<ChangeApprovalDto>? approvals = null,
        IEnumerable<string>? affectedServices = null) => new()
    {
        Id = dto.ChangeId,
        Number = dto.Number,
        Title = dto.ShortDescription,
        Description = dto.Description ?? string.Empty,
        Status = ChangeContractMapper.ToFrontendStatus(dto.State),
        Priority = ChangeContractMapper.ToFrontendPriority(dto.Type),
        RiskLevel = ChangeContractMapper.ToFrontendRiskLevel(dto.Risk),
        CreatedById = dto.RequestorId,
        CreatedByName = dto.RequestorName,
        ImplementedById = dto.AssignedToId,
        ImplementedByName = dto.AssignedToName,
        StartDate = ChangeContractMapper.ToIsoOrEmpty(dto.PlannedStartDate),
        EndDate = ChangeContractMapper.ToIsoOrEmpty(dto.PlannedEndDate),
        ActualStartDate = ChangeContractMapper.ToIso(dto.ActualStartDate),
        ActualEndDate = ChangeContractMapper.ToIso(dto.ActualEndDate),
        ImpactAnalysis = dto.RiskAssessmentNotes ?? string.Empty,
        RollbackPlan = dto.BackoutPlan ?? string.Empty,
        TestingPlan = dto.TestingPlan,
        CommunicationPlan = null,
        AffectedServices = affectedServices?.ToList() ?? new List<string>(),
        RelatedIncidents = new List<int>(),
        RelatedProblems = new List<int>(),
        RelatedAssets = new List<int>(),
        Approvals = approvals?.Select(MapToApprovalResponse).ToList(),
        CreatedAt = dto.CreatedAt.ToString("o"),
        UpdatedAt = dto.ModifiedAt?.ToString("o") ?? dto.CreatedAt.ToString("o")
    };

    private static ChangeApprovalResponse MapToApprovalResponse(ChangeApprovalDto dto) => new()
    {
        Id = dto.ApprovalId,
        ChangeId = dto.ChangeId,
        ApproverId = dto.ApproverId,
        ApproverName = dto.ApproverName,
        Status = ChangeContractMapper.ToFrontendApprovalStatus(dto.ApprovalStatus),
        Comments = dto.Comments,
        CreatedAt = dto.CreatedAt.ToString("o"),
        RespondedAt = dto.ApprovalDate?.ToString("o")
    };

    private static ChangeActivityResponse MapCommentToActivity(ChangeCommentDto comment) => new()
    {
        Id = comment.CommentId,
        ChangeId = comment.ChangeId,
        Type = "comment",
        UserId = comment.CreatedById,
        UserName = comment.CreatedByName,
        Content = comment.Comment,
        Timestamp = comment.CreatedAt.ToString("o")
    };
}

// ============================================================================
// API request/response models — shaped to match CRM.Frontend/src/services/changeService.ts
// exactly (path params, query params, and request/response bodies) so no frontend changes
// are required.
// ============================================================================

/// <summary>Response shape matching the frontend's <c>Change</c> interface.</summary>
public class ChangeResponse
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Status { get; set; }
    public int Priority { get; set; }
    public int RiskLevel { get; set; }
    public int CreatedById { get; set; }
    public string? CreatedByName { get; set; }
    public int? ImplementedById { get; set; }
    public string? ImplementedByName { get; set; }
    public string StartDate { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;
    public string? ActualStartDate { get; set; }
    public string? ActualEndDate { get; set; }
    public string ImpactAnalysis { get; set; } = string.Empty;
    public string RollbackPlan { get; set; } = string.Empty;
    public string? TestingPlan { get; set; }
    public string? CommunicationPlan { get; set; }
    public List<string> AffectedServices { get; set; } = new();
    public List<int> RelatedIncidents { get; set; } = new();
    public List<int> RelatedProblems { get; set; } = new();
    public List<int> RelatedAssets { get; set; } = new();
    public List<ChangeApprovalResponse>? Approvals { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
}

/// <summary>Response shape matching the frontend's <c>ChangeApproval</c> interface.</summary>
public class ChangeApprovalResponse
{
    public int Id { get; set; }
    public int ChangeId { get; set; }
    public int ApproverId { get; set; }
    public string? ApproverName { get; set; }
    public int Status { get; set; }
    public string? Comments { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public string? RespondedAt { get; set; }
}

/// <summary>Response shape matching the frontend's <c>ChangeActivity</c> interface.</summary>
public class ChangeActivityResponse
{
    public int Id { get; set; }
    public int ChangeId { get; set; }

    /// <summary>One of: status_change | comment | approval | attachment.</summary>
    public string Type { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Timestamp { get; set; } = string.Empty;
}

/// <summary>Response shape matching the frontend's <c>PagedChangeResult</c> interface.</summary>
public class PagedChangeResponse
{
    public List<ChangeResponse> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

/// <summary>Response shape matching the frontend's <c>ChangeCalendarEvent</c> interface.</summary>
public class ChangeCalendarEventResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Start { get; set; } = string.Empty;
    public string End { get; set; } = string.Empty;
    public int Status { get; set; }
    public int Priority { get; set; }
    public int RiskLevel { get; set; }
}

/// <summary>Response shape matching the frontend's <c>ChangeStatistics</c> interface.</summary>
public class ChangeStatisticsResponse
{
    public int TotalChanges { get; set; }
    public int ApprovedChanges { get; set; }
    public int ScheduledChanges { get; set; }
    public int CompletedChanges { get; set; }
    public double SuccessRate { get; set; }
    public double AvgApprovalTime { get; set; }
}

/// <summary>Request shape matching the frontend's <c>CreateChangeRequest</c> interface.</summary>
public class CreateChangeApiRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Priority { get; set; }
    public int RiskLevel { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? ImpactAnalysis { get; set; }
    public string? RollbackPlan { get; set; }
}

/// <summary>Request shape matching the frontend's <c>UpdateChangeRequest</c> interface.</summary>
public class UpdateChangeApiRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int? Priority { get; set; }
    public int? RiskLevel { get; set; }
    public int? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? ImpactAnalysis { get; set; }
    public string? RollbackPlan { get; set; }
    public string? TestingPlan { get; set; }
    public string? CommunicationPlan { get; set; }
}

/// <summary>Request body for <c>PATCH /changes/{id}/status</c>.</summary>
public class ChangeStatusRequest
{
    public int Status { get; set; }
}

/// <summary>Request body for <c>POST /changes/{id}/submit-approval</c>.</summary>
public class SubmitApprovalRequest
{
    public List<int> Approvers { get; set; } = new();
}

/// <summary>Request body for <c>POST /changes/{id}/approve</c>.</summary>
public class ApproveChangeApiRequest
{
    public string? Comments { get; set; }
}

/// <summary>Request body for <c>POST /changes/{id}/reject</c>.</summary>
public class RejectChangeApiRequest
{
    public string Reason { get; set; } = string.Empty;
}

/// <summary>Request body for <c>POST /changes/{id}/schedule</c>.</summary>
public class ScheduleChangeApiRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

/// <summary>Request body for <c>POST /changes/{id}/complete</c>.</summary>
public class CompleteChangeApiRequest
{
    public string? Comments { get; set; }
}

/// <summary>Request body for <c>POST /changes/{id}/rollback</c>.</summary>
public class RollbackChangeApiRequest
{
    public string Reason { get; set; } = string.Empty;
}

/// <summary>Request body for <c>POST /changes/{id}/comments</c>.</summary>
public class AddChangeCommentApiRequest
{
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// Enum/date conversions between backend ITSM change entities and the numeric enums used by
/// <c>CRM.Frontend/src/services/changeService.ts</c>. Kept in one place because several backend
/// states legitimately collapse onto the same frontend status (the reverse mapping used for
/// filtering is therefore best-effort/primary, not bijective).
/// </summary>
internal static class ChangeContractMapper
{
    public static int ToFrontendStatus(ChangeState state) => state switch
    {
        ChangeState.New => 0,               // Draft
        ChangeState.Assess => 1,            // SubmittedForApproval
        ChangeState.AwaitingApproval => 2,  // ApprovalInProgress
        ChangeState.Authorize => 3,         // Approved
        ChangeState.Approved => 3,          // Approved
        ChangeState.Rejected => 4,          // Rejected
        ChangeState.Scheduled => 5,         // Scheduled
        ChangeState.Implement => 6,         // InProgress
        ChangeState.Review => 8,            // Completed (post-implementation review)
        ChangeState.Closed => 8,            // Completed
        ChangeState.Implemented => 8,       // Completed
        ChangeState.Failed => 9,            // Rolled_Back (closest available frontend status)
        ChangeState.Cancelled => 10,        // Cancelled
        _ => 0
    };

    public static ChangeState ToBackendState(int frontendStatus) => frontendStatus switch
    {
        0 => ChangeState.New,
        1 => ChangeState.Assess,
        2 => ChangeState.AwaitingApproval,
        3 => ChangeState.Approved,
        4 => ChangeState.Rejected,
        5 => ChangeState.Scheduled,
        6 => ChangeState.Implement,
        7 => ChangeState.Implement,
        8 => ChangeState.Closed,
        9 => ChangeState.Failed,
        10 => ChangeState.Cancelled,
        _ => ChangeState.New
    };

    public static int ToFrontendPriority(ChangeType type) => type switch
    {
        ChangeType.Emergency => 0, // Critical
        ChangeType.Normal => 2,    // Medium
        ChangeType.Standard => 3,  // Low
        _ => 2
    };

    public static ChangeType ToBackendType(int priority) => priority switch
    {
        0 => ChangeType.Emergency, // Critical
        1 => ChangeType.Normal,    // High
        2 => ChangeType.Normal,    // Medium
        3 => ChangeType.Standard,  // Low
        4 => ChangeType.Standard,  // Planning
        _ => ChangeType.Normal
    };

    public static int ToFrontendRiskLevel(ChangeRisk risk) => risk switch
    {
        ChangeRisk.Low => 0,
        ChangeRisk.Medium => 1,
        ChangeRisk.High => 2,
        _ => 1
    };

    public static ChangeRisk ToBackendRisk(int riskLevel) => riskLevel switch
    {
        0 => ChangeRisk.Low,
        1 => ChangeRisk.Medium,
        2 => ChangeRisk.High,
        3 => ChangeRisk.High, // VeryHigh has no dedicated backend level; clamp to High
        _ => ChangeRisk.Medium
    };

    public static ChangeImpact ToBackendImpact(int riskLevel) => riskLevel switch
    {
        0 => ChangeImpact.Low,
        1 => ChangeImpact.Medium,
        2 => ChangeImpact.High,
        3 => ChangeImpact.High,
        _ => ChangeImpact.Medium
    };

    public static int ToFrontendApprovalStatus(ApprovalStatus status) => status switch
    {
        ApprovalStatus.Requested => 0, // Pending
        ApprovalStatus.Approved => 1,
        ApprovalStatus.Rejected => 2,
        ApprovalStatus.MoreInfo => 3,  // Deferred
        _ => 0
    };

    public static string? ToIso(DateTime? dt) => dt?.ToString("o");

    public static string ToIsoOrEmpty(DateTime? dt) => dt?.ToString("o") ?? string.Empty;
}
