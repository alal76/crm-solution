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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CRM.Core.Entities;
using CRM.Core.Interfaces;

namespace CRM.Api.Controllers;

/// <summary>
/// API controller for approval workflow operations - handling quote, discount, and contract approvals.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApprovalsController : ControllerBase
{
    private readonly IApprovalWorkflowService _approvalService;
    private readonly ILogger<ApprovalsController> _logger;

    public ApprovalsController(IApprovalWorkflowService approvalService, ILogger<ApprovalsController> logger)
    {
        _approvalService = approvalService;
        _logger = logger;
    }

    #region Approval Matrix Management

    /// <summary>
    /// Get all approval matrices with optional filtering.
    /// </summary>
    [HttpGet("matrices")]
    [ProducesResponseType(typeof(IEnumerable<DiscountApprovalMatrix>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DiscountApprovalMatrix>>> GetAllMatrices(
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var matrices = await _approvalService.GetAllMatricesAsync(isActive, cancellationToken);
        return Ok(matrices);
    }

    /// <summary>
    /// Get an approval matrix by ID.
    /// </summary>
    [HttpGet("matrices/{matrixId:int}")]
    [ProducesResponseType(typeof(DiscountApprovalMatrix), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DiscountApprovalMatrix>> GetMatrixById(int matrixId, CancellationToken cancellationToken)
    {
        var matrix = await _approvalService.GetMatrixByIdAsync(matrixId, cancellationToken);
        if (matrix == null)
        {
            return NotFound($"Approval matrix with ID {matrixId} not found.");
        }
        return Ok(matrix);
    }

    /// <summary>
    /// Create a new approval matrix.
    /// </summary>
    [HttpPost("matrices")]
    [ProducesResponseType(typeof(DiscountApprovalMatrix), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DiscountApprovalMatrix>> CreateMatrix(
        [FromBody] DiscountApprovalMatrix matrix,
        CancellationToken cancellationToken)
    {
        var created = await _approvalService.CreateMatrixAsync(matrix, cancellationToken);
        return CreatedAtAction(nameof(GetMatrixById), new { matrixId = created.Id }, created);
    }

    /// <summary>
    /// Update an existing approval matrix.
    /// </summary>
    [HttpPut("matrices/{matrixId:int}")]
    [ProducesResponseType(typeof(DiscountApprovalMatrix), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DiscountApprovalMatrix>> UpdateMatrix(
        int matrixId,
        [FromBody] DiscountApprovalMatrix matrix,
        CancellationToken cancellationToken)
    {
        if (matrixId != matrix.Id)
        {
            return BadRequest("ID mismatch between URL and body.");
        }

        var updated = await _approvalService.UpdateMatrixAsync(matrix, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// Delete an approval matrix (soft delete).
    /// </summary>
    [HttpDelete("matrices/{matrixId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteMatrix(int matrixId, CancellationToken cancellationToken)
    {
        var result = await _approvalService.DeleteMatrixAsync(matrixId, cancellationToken);
        if (!result)
        {
            return NotFound($"Approval matrix with ID {matrixId} not found.");
        }
        return NoContent();
    }

    /// <summary>
    /// Activate an approval matrix.
    /// </summary>
    [HttpPost("matrices/{matrixId:int}/activate")]
    [ProducesResponseType(typeof(DiscountApprovalMatrix), StatusCodes.Status200OK)]
    public async Task<ActionResult<DiscountApprovalMatrix>> ActivateMatrix(int matrixId, CancellationToken cancellationToken)
    {
        var matrix = await _approvalService.ActivateMatrixAsync(matrixId, cancellationToken);
        return Ok(matrix);
    }

    /// <summary>
    /// Deactivate an approval matrix.
    /// </summary>
    [HttpPost("matrices/{matrixId:int}/deactivate")]
    [ProducesResponseType(typeof(DiscountApprovalMatrix), StatusCodes.Status200OK)]
    public async Task<ActionResult<DiscountApprovalMatrix>> DeactivateMatrix(int matrixId, CancellationToken cancellationToken)
    {
        var matrix = await _approvalService.DeactivateMatrixAsync(matrixId, cancellationToken);
        return Ok(matrix);
    }

    #endregion

    #region Approval Level Management

    /// <summary>
    /// Get all approval levels for a matrix.
    /// </summary>
    [HttpGet("matrices/{matrixId:int}/levels")]
    [ProducesResponseType(typeof(IEnumerable<ApprovalLevel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ApprovalLevel>>> GetMatrixLevels(int matrixId, CancellationToken cancellationToken)
    {
        var levels = await _approvalService.GetMatrixLevelsAsync(matrixId, cancellationToken);
        return Ok(levels);
    }

    /// <summary>
    /// Get an approval level by ID.
    /// </summary>
    [HttpGet("levels/{levelId:int}")]
    [ProducesResponseType(typeof(ApprovalLevel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApprovalLevel>> GetLevelById(int levelId, CancellationToken cancellationToken)
    {
        var level = await _approvalService.GetLevelByIdAsync(levelId, cancellationToken);
        if (level == null)
        {
            return NotFound($"Approval level with ID {levelId} not found.");
        }
        return Ok(level);
    }

    /// <summary>
    /// Add an approval level to a matrix.
    /// </summary>
    [HttpPost("matrices/{matrixId:int}/levels")]
    [ProducesResponseType(typeof(ApprovalLevel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApprovalLevel>> AddLevel(
        int matrixId,
        [FromBody] ApprovalLevel level,
        CancellationToken cancellationToken)
    {
        var added = await _approvalService.AddLevelAsync(matrixId, level, cancellationToken);
        return CreatedAtAction(nameof(GetLevelById), new { levelId = added.Id }, added);
    }

    /// <summary>
    /// Update an approval level.
    /// </summary>
    [HttpPut("levels/{levelId:int}")]
    [ProducesResponseType(typeof(ApprovalLevel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApprovalLevel>> UpdateLevel(
        int levelId,
        [FromBody] ApprovalLevel level,
        CancellationToken cancellationToken)
    {
        if (levelId != level.Id)
        {
            return BadRequest("ID mismatch between URL and body.");
        }

        var updated = await _approvalService.UpdateLevelAsync(level, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// Remove an approval level.
    /// </summary>
    [HttpDelete("levels/{levelId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RemoveLevel(int levelId, CancellationToken cancellationToken)
    {
        var result = await _approvalService.RemoveLevelAsync(levelId, cancellationToken);
        if (!result)
        {
            return NotFound($"Approval level with ID {levelId} not found.");
        }
        return NoContent();
    }

    /// <summary>
    /// Reorder approval levels within a matrix.
    /// </summary>
    [HttpPut("matrices/{matrixId:int}/levels/reorder")]
    [ProducesResponseType(typeof(IEnumerable<ApprovalLevel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<ApprovalLevel>>> ReorderLevels(
        int matrixId,
        [FromBody] List<int> levelIdsInOrder,
        CancellationToken cancellationToken)
    {
        var levels = await _approvalService.ReorderLevelsAsync(matrixId, levelIdsInOrder, cancellationToken);
        return Ok(levels);
    }

    #endregion

    #region Approval Group Management

    /// <summary>
    /// Get all approval groups.
    /// </summary>
    [HttpGet("groups")]
    [ProducesResponseType(typeof(IEnumerable<ApprovalGroup>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ApprovalGroup>>> GetAllGroups(
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var groups = await _approvalService.GetAllGroupsAsync(isActive, cancellationToken);
        return Ok(groups);
    }

    /// <summary>
    /// Get an approval group by ID.
    /// </summary>
    [HttpGet("groups/{groupId:int}")]
    [ProducesResponseType(typeof(ApprovalGroup), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApprovalGroup>> GetGroupById(int groupId, CancellationToken cancellationToken)
    {
        var group = await _approvalService.GetGroupByIdAsync(groupId, cancellationToken);
        if (group == null)
        {
            return NotFound($"Approval group with ID {groupId} not found.");
        }
        return Ok(group);
    }

    /// <summary>
    /// Create an approval group.
    /// </summary>
    [HttpPost("groups")]
    [ProducesResponseType(typeof(ApprovalGroup), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApprovalGroup>> CreateGroup(
        [FromBody] ApprovalGroup group,
        CancellationToken cancellationToken)
    {
        var created = await _approvalService.CreateGroupAsync(group, cancellationToken);
        return CreatedAtAction(nameof(GetGroupById), new { groupId = created.Id }, created);
    }

    /// <summary>
    /// Update an approval group.
    /// </summary>
    [HttpPut("groups/{groupId:int}")]
    [ProducesResponseType(typeof(ApprovalGroup), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApprovalGroup>> UpdateGroup(
        int groupId,
        [FromBody] ApprovalGroup group,
        CancellationToken cancellationToken)
    {
        if (groupId != group.Id)
        {
            return BadRequest("ID mismatch between URL and body.");
        }

        var updated = await _approvalService.UpdateGroupAsync(group, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// Delete an approval group (soft delete).
    /// </summary>
    [HttpDelete("groups/{groupId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteGroup(int groupId, CancellationToken cancellationToken)
    {
        var result = await _approvalService.DeleteGroupAsync(groupId, cancellationToken);
        if (!result)
        {
            return NotFound($"Approval group with ID {groupId} not found.");
        }
        return NoContent();
    }

    /// <summary>
    /// Get members of an approval group.
    /// </summary>
    [HttpGet("groups/{groupId:int}/members")]
    [ProducesResponseType(typeof(IEnumerable<ApprovalGroupMember>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ApprovalGroupMember>>> GetGroupMembers(int groupId, CancellationToken cancellationToken)
    {
        var members = await _approvalService.GetGroupMembersAsync(groupId, cancellationToken);
        return Ok(members);
    }

    /// <summary>
    /// Add a member to an approval group.
    /// </summary>
    [HttpPost("groups/{groupId:int}/members/{userId:int}")]
    [ProducesResponseType(typeof(ApprovalGroupMember), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApprovalGroupMember>> AddGroupMember(int groupId, int userId, CancellationToken cancellationToken)
    {
        var member = await _approvalService.AddGroupMemberAsync(groupId, userId, cancellationToken);
        return Ok(member);
    }

    /// <summary>
    /// Remove a member from an approval group.
    /// </summary>
    [HttpDelete("groups/{groupId:int}/members/{userId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RemoveGroupMember(int groupId, int userId, CancellationToken cancellationToken)
    {
        var result = await _approvalService.RemoveGroupMemberAsync(groupId, userId, cancellationToken);
        if (!result)
        {
            return NotFound("Group member not found.");
        }
        return NoContent();
    }

    #endregion

    #region Approval Request Management

    /// <summary>
    /// Get all approval requests with filtering.
    /// </summary>
    [HttpGet("requests")]
    [ProducesResponseType(typeof(IEnumerable<ApprovalRequest>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ApprovalRequest>>> GetAllRequests(
        [FromQuery] DiscountApprovalStatus? status = null,
        [FromQuery] int? submitterId = null,
        [FromQuery] int? quoteId = null,
        CancellationToken cancellationToken = default)
    {
        var requests = await _approvalService.GetAllRequestsAsync(status, submitterId, quoteId, cancellationToken);
        return Ok(requests);
    }

    /// <summary>
    /// Get an approval request by ID.
    /// </summary>
    [HttpGet("requests/{requestId:int}")]
    [ProducesResponseType(typeof(ApprovalRequest), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApprovalRequest>> GetRequestById(int requestId, CancellationToken cancellationToken)
    {
        var request = await _approvalService.GetRequestByIdAsync(requestId, cancellationToken);
        if (request == null)
        {
            return NotFound($"Approval request with ID {requestId} not found.");
        }
        return Ok(request);
    }

    /// <summary>
    /// Get an approval request by request number.
    /// </summary>
    [HttpGet("requests/by-number/{requestNumber}")]
    [ProducesResponseType(typeof(ApprovalRequest), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApprovalRequest>> GetRequestByNumber(string requestNumber, CancellationToken cancellationToken)
    {
        var request = await _approvalService.GetRequestByNumberAsync(requestNumber, cancellationToken);
        if (request == null)
        {
            return NotFound($"Approval request with number '{requestNumber}' not found.");
        }
        return Ok(request);
    }

    /// <summary>
    /// Get pending approvals for the current user.
    /// </summary>
    [HttpGet("requests/pending")]
    [ProducesResponseType(typeof(IEnumerable<ApprovalRequest>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<ApprovalRequest>>> GetPendingApprovalsForCurrentUser(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var requests = await _approvalService.GetPendingApprovalsForUserAsync(userId.Value, cancellationToken);
        return Ok(requests);
    }

    /// <summary>
    /// Get pending approvals for a specific user.
    /// </summary>
    [HttpGet("requests/pending/{userId:int}")]
    [ProducesResponseType(typeof(IEnumerable<ApprovalRequest>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ApprovalRequest>>> GetPendingApprovalsForUser(int userId, CancellationToken cancellationToken)
    {
        var requests = await _approvalService.GetPendingApprovalsForUserAsync(userId, cancellationToken);
        return Ok(requests);
    }

    /// <summary>
    /// Get approval requests submitted by a user.
    /// </summary>
    [HttpGet("requests/submitted/{submitterId:int}")]
    [ProducesResponseType(typeof(IEnumerable<ApprovalRequest>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ApprovalRequest>>> GetRequestsBySubmitter(int submitterId, CancellationToken cancellationToken)
    {
        var requests = await _approvalService.GetRequestsBySubmitterAsync(submitterId, cancellationToken);
        return Ok(requests);
    }

    #endregion

    #region Approval Workflow Operations

    /// <summary>
    /// Submit a quote for approval.
    /// </summary>
    [HttpPost("submit/{quoteId:int}")]
    [ProducesResponseType(typeof(ApprovalSubmissionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApprovalSubmissionResult>> SubmitForApproval(
        int quoteId,
        [FromBody] SubmitForApprovalRequest request,
        CancellationToken cancellationToken)
    {
        var submitterId = GetCurrentUserId();
        if (!submitterId.HasValue)
        {
            return Unauthorized();
        }

        var result = await _approvalService.SubmitForApprovalAsync(quoteId, submitterId.Value, request.Justification, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Determine approval requirements for a quote (preview without submitting).
    /// </summary>
    [HttpGet("requirements/{quoteId:int}")]
    [ProducesResponseType(typeof(ApprovalRequirementResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApprovalRequirementResult>> DetermineApprovalRequirements(int quoteId, CancellationToken cancellationToken)
    {
        var result = await _approvalService.DetermineApprovalRequirementsAsync(quoteId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Approve a pending approval step.
    /// </summary>
    [HttpPost("requests/{requestId:int}/approve")]
    [ProducesResponseType(typeof(ApprovalActionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApprovalActionResult>> ApproveStep(
        int requestId,
        [FromBody] ApprovalActionRequest request,
        CancellationToken cancellationToken)
    {
        var approverId = GetCurrentUserId();
        if (!approverId.HasValue)
        {
            return Unauthorized();
        }

        var result = await _approvalService.ApproveStepAsync(requestId, approverId.Value, request.Comments, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Reject a pending approval step.
    /// </summary>
    [HttpPost("requests/{requestId:int}/reject")]
    [ProducesResponseType(typeof(ApprovalActionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApprovalActionResult>> RejectStep(
        int requestId,
        [FromBody] RejectApprovalRequest request,
        CancellationToken cancellationToken)
    {
        var approverId = GetCurrentUserId();
        if (!approverId.HasValue)
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            return BadRequest("A reason is required when rejecting an approval request.");
        }

        var result = await _approvalService.RejectStepAsync(requestId, approverId.Value, request.Reason, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Recall (cancel) a submitted approval request.
    /// </summary>
    [HttpPost("requests/{requestId:int}/recall")]
    [ProducesResponseType(typeof(ApprovalRequest), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApprovalRequest>> RecallRequest(
        int requestId,
        [FromBody] RecallApprovalRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var result = await _approvalService.RecallRequestAsync(requestId, userId.Value, request.Reason, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Reassign a pending approval step to a different user.
    /// </summary>
    [HttpPost("steps/{stepId:int}/reassign")]
    [ProducesResponseType(typeof(ApprovalStep), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApprovalStep>> ReassignStep(
        int stepId,
        [FromBody] ReassignStepRequest request,
        CancellationToken cancellationToken)
    {
        var reassignedById = GetCurrentUserId();
        if (!reassignedById.HasValue)
        {
            return Unauthorized();
        }

        var step = await _approvalService.ReassignStepAsync(stepId, request.NewAssigneeId, reassignedById.Value, request.Reason, cancellationToken);
        return Ok(step);
    }

    /// <summary>
    /// Escalate a pending approval step.
    /// </summary>
    [HttpPost("steps/{stepId:int}/escalate")]
    [ProducesResponseType(typeof(ApprovalStep), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApprovalStep>> EscalateStep(
        int stepId,
        [FromBody] EscalateStepRequest request,
        CancellationToken cancellationToken)
    {
        var escalatedById = GetCurrentUserId();
        if (!escalatedById.HasValue)
        {
            return Unauthorized();
        }

        var step = await _approvalService.EscalateStepAsync(stepId, escalatedById.Value, request.Reason, cancellationToken);
        return Ok(step);
    }

    #endregion

    #region Matrix Selection

    /// <summary>
    /// Find the applicable approval matrix for a quote.
    /// </summary>
    [HttpGet("matrices/applicable/{quoteId:int}")]
    [ProducesResponseType(typeof(DiscountApprovalMatrix), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DiscountApprovalMatrix>> FindApplicableMatrix(int quoteId, CancellationToken cancellationToken)
    {
        var matrix = await _approvalService.FindApplicableMatrixAsync(quoteId, cancellationToken);
        if (matrix == null)
        {
            return NotFound("No applicable approval matrix found for this quote.");
        }
        return Ok(matrix);
    }

    /// <summary>
    /// Check if a discount requires approval.
    /// </summary>
    [HttpGet("requires-approval")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> RequiresApproval(
        [FromQuery] decimal discountPercent,
        [FromQuery] decimal? dealAmount = null,
        [FromQuery] int? matrixId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _approvalService.RequiresApprovalAsync(discountPercent, dealAmount, matrixId, cancellationToken);
        return Ok(new { requiresApproval = result });
    }

    /// <summary>
    /// Get the maximum discount the current user can approve without escalation.
    /// </summary>
    [HttpGet("approval-limit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<decimal>> GetCurrentUserApprovalLimit(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var limit = await _approvalService.GetUserApprovalLimitAsync(userId.Value, cancellationToken);
        return Ok(new { approvalLimit = limit });
    }

    /// <summary>
    /// Get the maximum discount a specific user can approve without escalation.
    /// </summary>
    [HttpGet("approval-limit/{userId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<decimal>> GetUserApprovalLimit(int userId, CancellationToken cancellationToken)
    {
        var limit = await _approvalService.GetUserApprovalLimitAsync(userId, cancellationToken);
        return Ok(new { approvalLimit = limit });
    }

    #endregion

    #region Notifications & Reminders

    /// <summary>
    /// Send reminder notifications for overdue approvals (admin operation).
    /// </summary>
    [HttpPost("reminders/send-overdue")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> SendOverdueReminders(CancellationToken cancellationToken)
    {
        var count = await _approvalService.SendOverdueRemindersAsync(cancellationToken);
        return Ok(new { sentCount = count });
    }

    /// <summary>
    /// Process auto-escalations for timed-out steps (admin operation).
    /// </summary>
    [HttpPost("escalations/process-auto")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> ProcessAutoEscalations(CancellationToken cancellationToken)
    {
        var count = await _approvalService.ProcessAutoEscalationsAsync(cancellationToken);
        return Ok(new { escalatedCount = count });
    }

    #endregion

    #region Statistics & Reporting

    /// <summary>
    /// Get approval workflow statistics.
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(ApprovalStatistics), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApprovalStatistics>> GetStatistics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var stats = await _approvalService.GetStatisticsAsync(fromDate, toDate, cancellationToken);
        return Ok(stats);
    }

    /// <summary>
    /// Get approver performance statistics.
    /// </summary>
    [HttpGet("statistics/approvers")]
    [ProducesResponseType(typeof(IEnumerable<ApproverPerformance>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ApproverPerformance>>> GetApproverPerformance(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var performance = await _approvalService.GetApproverPerformanceAsync(fromDate, toDate, cancellationToken);
        return Ok(performance);
    }

    /// <summary>
    /// Get approval history for a quote.
    /// </summary>
    [HttpGet("quotes/{quoteId:int}/history")]
    [ProducesResponseType(typeof(IEnumerable<ApprovalRequest>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ApprovalRequest>>> GetQuoteApprovalHistory(int quoteId, CancellationToken cancellationToken)
    {
        var history = await _approvalService.GetQuoteApprovalHistoryAsync(quoteId, cancellationToken);
        return Ok(history);
    }

    #endregion

    #region Helper Methods

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        return null;
    }

    #endregion
}

#region Request DTOs

public class SubmitForApprovalRequest
{
    public string? Justification { get; set; }
}

public class ApprovalActionRequest
{
    public string? Comments { get; set; }
}

public class RejectApprovalRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class RecallApprovalRequest
{
    public string? Reason { get; set; }
}

public class ReassignStepRequest
{
    public int NewAssigneeId { get; set; }
    public string? Reason { get; set; }
}

public class EscalateStepRequest
{
    public string? Reason { get; set; }
}

#endregion
