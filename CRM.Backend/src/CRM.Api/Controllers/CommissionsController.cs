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

using System.ComponentModel.DataAnnotations;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;

namespace CRM.Api.Controllers;

/// <summary>
/// API controller for commission management, plans, tiers, statements, and payouts.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class CommissionsController : ControllerBase
{
    private readonly ICommissionService _commissionService;
    private readonly IFeatureManager _featureManager;
    private readonly ILogger<CommissionsController> _logger;

    public CommissionsController(
        ICommissionService commissionService,
        IFeatureManager featureManager,
        ILogger<CommissionsController> logger)
    {
        _commissionService = commissionService;
        _featureManager = featureManager;
        _logger = logger;
    }

    #region Commission CRUD

    /// <summary>
    /// Get all commissions with optional filtering.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Commission>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<Commission>>> GetAll(
        [FromQuery] int? userId = null,
        [FromQuery] CommissionStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var commissions = await _commissionService.GetAllAsync(userId, status, cancellationToken);
        return Ok(commissions);
    }

    /// <summary>
    /// Get a commission by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Commission), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Commission>> GetById(int id, CancellationToken cancellationToken)
    {
        var commission = await _commissionService.GetByIdAsync(id, cancellationToken);
        if (commission == null)
        {
            return NotFound($"Commission {id} not found.");
        }

        return Ok(commission);
    }

    /// <summary>
    /// Create a commission.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Commission), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Commission>> Create(
        [FromBody] CommissionCreateRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var commission = new Commission
        {
            UserId = request.UserId,
            CommissionPlanId = request.CommissionPlanId ?? 0,
            OpportunityId = request.OpportunityId,
            OrderId = request.OrderId,
            InvoiceId = request.InvoiceId,
            SubscriptionId = request.SubscriptionId,
            DealAmount = request.DealAmount,
            CommissionableAmount = request.CommissionableAmount ?? request.DealAmount,
            CommissionRate = request.CommissionRate,
            CommissionAmount = request.CommissionAmount,
            SplitPercent = request.SplitPercent ?? 100m,
            FinalCommissionAmount = request.FinalCommissionAmount ?? request.CommissionAmount,
            CurrencyCode = request.CurrencyCode ?? "USD",
            Status = CommissionStatus.Pending,
            Notes = request.Notes
        };

        try
        {
            var created = await _commissionService.CreateAsync(commission, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating commission");
            return HandleServiceException(ex);
        }
    }

    /// <summary>
    /// Update a commission.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(Commission), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Commission>> Update(
        int id,
        [FromBody] CommissionUpdateRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var existing = await _commissionService.GetByIdAsync(id, cancellationToken);
        if (existing == null)
        {
            return NotFound($"Commission {id} not found.");
        }

        // Only allow updates if commission is still pending
        if (existing.Status != CommissionStatus.Pending && existing.Status != CommissionStatus.Held)
        {
            return BadRequest("Cannot update commission after it has been approved or paid.");
        }

        existing.DealAmount = request.DealAmount ?? existing.DealAmount;
        existing.CommissionableAmount = request.CommissionableAmount ?? existing.CommissionableAmount;
        existing.CommissionRate = request.CommissionRate ?? existing.CommissionRate;
        existing.CommissionAmount = request.CommissionAmount ?? existing.CommissionAmount;
        existing.SplitPercent = request.SplitPercent ?? existing.SplitPercent;
        existing.FinalCommissionAmount = request.FinalCommissionAmount ?? existing.FinalCommissionAmount;
        existing.Notes = request.Notes ?? existing.Notes;

        try
        {
            var updated = await _commissionService.UpdateAsync(existing, cancellationToken);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating commission {CommissionId}", id);
            return HandleServiceException(ex);
        }
    }

    /// <summary>
    /// Delete a commission (soft delete).
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var existing = await _commissionService.GetByIdAsync(id, cancellationToken);
        if (existing == null)
        {
            return NotFound($"Commission {id} not found.");
        }

        if (existing.Status == CommissionStatus.Paid)
        {
            return BadRequest("Cannot delete a paid commission.");
        }

        await _commissionService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    #endregion

    #region Status Management

    /// <summary>
    /// Update commission status.
    /// </summary>
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(Commission), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Commission>> UpdateStatus(
        int id,
        [FromBody] CommissionStatusUpdateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var commission = await _commissionService.UpdateStatusAsync(id, request.Status, cancellationToken);
            return Ok(commission);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating commission status");
            return HandleServiceException(ex);
        }
    }

    /// <summary>
    /// Approve a commission for payout.
    /// </summary>
    [HttpPost("{id:int}/approve")]
    [ProducesResponseType(typeof(Commission), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Commission>> Approve(
        int id,
        [FromBody] CommissionApproveRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var commission = await _commissionService.ApproveAsync(id, request.ApprovedById, cancellationToken);
            return Ok(commission);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving commission {CommissionId}", id);
            return HandleServiceException(ex);
        }
    }

    /// <summary>
    /// Reject a commission.
    /// </summary>
    [HttpPost("{id:int}/reject")]
    [ProducesResponseType(typeof(Commission), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Commission>> Reject(
        int id,
        [FromBody] CommissionRejectRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            return BadRequest("Reason is required when rejecting a commission.");
        }

        try
        {
            var commission = await _commissionService.RejectAsync(id, request.Reason, cancellationToken);
            return Ok(commission);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting commission {CommissionId}", id);
            return HandleServiceException(ex);
        }
    }

    /// <summary>
    /// Mark a commission as paid.
    /// </summary>
    [HttpPost("{id:int}/mark-paid")]
    [ProducesResponseType(typeof(Commission), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Commission>> MarkAsPaid(
        int id,
        [FromBody] CommissionMarkPaidRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var commission = await _commissionService.MarkAsPaidAsync(id, request?.PaidDate, cancellationToken);
            return Ok(commission);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking commission {CommissionId} as paid", id);
            return HandleServiceException(ex);
        }
    }

    /// <summary>
    /// Claw back a paid commission.
    /// </summary>
    [HttpPost("{id:int}/clawback")]
    [ProducesResponseType(typeof(Commission), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Commission>> Clawback(
        int id,
        [FromBody] CommissionClawbackRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            return BadRequest("Reason is required for clawback.");
        }

        try
        {
            var commission = await _commissionService.ClawbackAsync(id, request.Reason, cancellationToken);
            return Ok(commission);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clawing back commission {CommissionId}", id);
            return HandleServiceException(ex);
        }
    }

    #endregion

    #region Commission Calculation

    /// <summary>
    /// Calculate commission for a deal/opportunity.
    /// </summary>
    [HttpGet("calculate/deal/{opportunityId:int}")]
    [ProducesResponseType(typeof(CommissionCalculation), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CommissionCalculation>> CalculateForDeal(
        int opportunityId,
        CancellationToken cancellationToken)
    {
        try
        {
            var calculation = await _commissionService.CalculateForDealAsync(opportunityId, cancellationToken);
            return Ok(calculation);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating commission for opportunity {OpportunityId}", opportunityId);
            return HandleServiceException(ex);
        }
    }

    /// <summary>
    /// Calculate commission for an order.
    /// </summary>
    [HttpGet("calculate/order/{orderId:int}")]
    [ProducesResponseType(typeof(CommissionCalculation), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CommissionCalculation>> CalculateForOrder(
        int orderId,
        CancellationToken cancellationToken)
    {
        try
        {
            var calculation = await _commissionService.CalculateForOrderAsync(orderId, cancellationToken);
            return Ok(calculation);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating commission for order {OrderId}", orderId);
            return HandleServiceException(ex);
        }
    }

    /// <summary>
    /// Recalculate a commission after changes.
    /// </summary>
    [HttpPost("{id:int}/recalculate")]
    [ProducesResponseType(typeof(Commission), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Commission>> Recalculate(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            var commission = await _commissionService.RecalculateAsync(id, cancellationToken);
            return Ok(commission);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recalculating commission {CommissionId}", id);
            return HandleServiceException(ex);
        }
    }

    #endregion

    #region Queries

    /// <summary>
    /// Get commissions for a specific user.
    /// </summary>
    [HttpGet("user/{userId:int}")]
    [ProducesResponseType(typeof(IEnumerable<Commission>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Commission>>> GetByUser(
        int userId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var commissions = await _commissionService.GetByUserAsync(userId, fromDate, toDate, cancellationToken);
        return Ok(commissions);
    }

    /// <summary>
    /// Get pending commissions awaiting approval.
    /// </summary>
    [HttpGet("pending-approvals")]
    [ProducesResponseType(typeof(IEnumerable<Commission>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Commission>>> GetPendingApprovals(
        CancellationToken cancellationToken)
    {
        var commissions = await _commissionService.GetPendingApprovalsAsync(cancellationToken);
        return Ok(commissions);
    }

    /// <summary>
    /// Get commissions ready for payout.
    /// </summary>
    [HttpGet("ready-for-payout")]
    [ProducesResponseType(typeof(IEnumerable<Commission>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Commission>>> GetReadyForPayout(
        CancellationToken cancellationToken)
    {
        var commissions = await _commissionService.GetReadyForPayoutAsync(cancellationToken);
        return Ok(commissions);
    }

    /// <summary>
    /// Get commission statistics.
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(CommissionStatistics), StatusCodes.Status200OK)]
    public async Task<ActionResult<CommissionStatistics>> GetStatistics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var statistics = await _commissionService.GetStatisticsAsync(fromDate, toDate, cancellationToken);
        return Ok(statistics);
    }

    /// <summary>
    /// Get commission leaderboard.
    /// </summary>
    [HttpGet("leaderboard")]
    [ProducesResponseType(typeof(IEnumerable<CommissionLeaderboard>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CommissionLeaderboard>>> GetLeaderboard(
        [FromQuery] int topN = 10,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var leaderboard = await _commissionService.GetLeaderboardAsync(topN, fromDate, toDate, cancellationToken);
        return Ok(leaderboard);
    }

    /// <summary>
    /// Get commission forecast for a user.
    /// </summary>
    [HttpGet("forecast/{userId:int}")]
    [ProducesResponseType(typeof(CommissionForecast), StatusCodes.Status200OK)]
    public async Task<ActionResult<CommissionForecast>> GetForecast(
        int userId,
        [FromQuery] DateTime? asOfDate = null,
        CancellationToken cancellationToken = default)
    {
        var forecast = await _commissionService.GetForecastAsync(userId, asOfDate, cancellationToken);
        return Ok(forecast);
    }

    /// <summary>
    /// Get commission summary for a period.
    /// </summary>
    [HttpGet("summary/{userId:int}")]
    [ProducesResponseType(typeof(CommissionSummary), StatusCodes.Status200OK)]
    public async Task<ActionResult<CommissionSummary>> GetPeriodSummary(
        int userId,
        [FromQuery, Required] DateTime fromDate,
        [FromQuery, Required] DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        var summary = await _commissionService.CalculateForPeriodAsync(userId, fromDate, toDate, cancellationToken);
        return Ok(summary);
    }

    #endregion

    #region Commission Plans

    /// <summary>
    /// Get all commission plans.
    /// </summary>
    [HttpGet("plans")]
    [ProducesResponseType(typeof(IEnumerable<CommissionPlan>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CommissionPlan>>> GetPlans(
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var plans = await _commissionService.GetPlansAsync(isActive, cancellationToken);
        return Ok(plans);
    }

    /// <summary>
    /// Get a commission plan by ID.
    /// </summary>
    [HttpGet("plans/{planId:int}")]
    [ProducesResponseType(typeof(CommissionPlan), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommissionPlan>> GetPlanById(int planId, CancellationToken cancellationToken)
    {
        var plan = await _commissionService.GetPlanByIdAsync(planId, cancellationToken);
        if (plan == null)
        {
            return NotFound($"Commission plan {planId} not found.");
        }

        return Ok(plan);
    }

    /// <summary>
    /// Create a commission plan.
    /// </summary>
    [HttpPost("plans")]
    [ProducesResponseType(typeof(CommissionPlan), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CommissionPlan>> CreatePlan(
        [FromBody] CommissionPlanCreateRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var plan = new CommissionPlan
        {
            Name = request.Name,
            Code = request.Code ?? GeneratePlanCode(request.Name),
            Description = request.Description,
            Status = CommissionPlanStatus.Draft,
            EffectiveStartDate = request.EffectiveStartDate ?? DateTime.UtcNow,
            EffectiveEndDate = request.EffectiveEndDate,
            FiscalYear = request.FiscalYear,
            CommissionType = request.CommissionType ?? CommissionType.FlatPercentage,
            BaseRate = request.BaseRate ?? 0.05m,
            Trigger = request.Trigger ?? CommissionTrigger.OnClose,
            ClawbackPeriodDays = request.ClawbackPeriodDays ?? 90,
            MinDealSize = request.MinDealSize,
            MaxCommissionPerDeal = request.MaxCommissionPerDeal,
            MaxCommissionPerPeriod = request.MaxCommissionPerPeriod,
            AllowSplits = request.AllowSplits ?? true,
            DefaultOverlayPercent = request.DefaultOverlayPercent
        };

        try
        {
            var created = await _commissionService.CreatePlanAsync(plan, cancellationToken);
            return CreatedAtAction(nameof(GetPlanById), new { planId = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating commission plan");
            return HandleServiceException(ex);
        }
    }

    /// <summary>
    /// Update a commission plan.
    /// </summary>
    [HttpPut("plans/{planId:int}")]
    [ProducesResponseType(typeof(CommissionPlan), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CommissionPlan>> UpdatePlan(
        int planId,
        [FromBody] CommissionPlanUpdateRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var existing = await _commissionService.GetPlanByIdAsync(planId, cancellationToken);
        if (existing == null)
        {
            return NotFound($"Commission plan {planId} not found.");
        }

        existing.Name = request.Name ?? existing.Name;
        existing.Description = request.Description ?? existing.Description;
        existing.Status = request.Status ?? existing.Status;
        existing.EffectiveStartDate = request.EffectiveStartDate ?? existing.EffectiveStartDate;
        existing.EffectiveEndDate = request.EffectiveEndDate ?? existing.EffectiveEndDate;
        existing.FiscalYear = request.FiscalYear ?? existing.FiscalYear;
        existing.CommissionType = request.CommissionType ?? existing.CommissionType;
        existing.BaseRate = request.BaseRate ?? existing.BaseRate;
        existing.Trigger = request.Trigger ?? existing.Trigger;
        existing.ClawbackPeriodDays = request.ClawbackPeriodDays ?? existing.ClawbackPeriodDays;
        existing.MinDealSize = request.MinDealSize ?? existing.MinDealSize;
        existing.MaxCommissionPerDeal = request.MaxCommissionPerDeal ?? existing.MaxCommissionPerDeal;
        existing.MaxCommissionPerPeriod = request.MaxCommissionPerPeriod ?? existing.MaxCommissionPerPeriod;
        existing.AllowSplits = request.AllowSplits ?? existing.AllowSplits;
        existing.DefaultOverlayPercent = request.DefaultOverlayPercent ?? existing.DefaultOverlayPercent;

        try
        {
            var updated = await _commissionService.UpdatePlanAsync(existing, cancellationToken);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating commission plan {PlanId}", planId);
            return HandleServiceException(ex);
        }
    }

    /// <summary>
    /// Delete a commission plan (soft delete).
    /// </summary>
    [HttpDelete("plans/{planId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePlan(int planId, CancellationToken cancellationToken)
    {
        var result = await _commissionService.DeletePlanAsync(planId, cancellationToken);
        if (!result)
        {
            return NotFound($"Commission plan {planId} not found.");
        }

        return NoContent();
    }

    /// <summary>
    /// Assign a plan to a user.
    /// </summary>
    [HttpPost("plans/{planId:int}/assign")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AssignPlanToUser(
        int planId,
        [FromBody] CommissionPlanAssignRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _commissionService.AssignPlanToUserAsync(planId, request.UserId, request.EffectiveDate, cancellationToken);
            if (!result)
            {
                return BadRequest("Failed to assign plan to user.");
            }

            return Ok(new { Message = $"Plan {planId} assigned to user {request.UserId}" });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning plan {PlanId} to user {UserId}", planId, request.UserId);
            return HandleServiceException(ex);
        }
    }

    /// <summary>
    /// Get the active plan for a user.
    /// </summary>
    [HttpGet("plans/user/{userId:int}")]
    [ProducesResponseType(typeof(CommissionPlan), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommissionPlan>> GetUserPlan(
        int userId,
        CancellationToken cancellationToken)
    {
        var plan = await _commissionService.GetUserPlanAsync(userId, cancellationToken);
        if (plan == null)
        {
            return NotFound($"No active commission plan found for user {userId}.");
        }

        return Ok(plan);
    }

    #endregion

    #region Commission Tiers

    /// <summary>
    /// Get tiers for a commission plan.
    /// </summary>
    [HttpGet("plans/{planId:int}/tiers")]
    [ProducesResponseType(typeof(IEnumerable<CommissionTier>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CommissionTier>>> GetTiers(
        int planId,
        CancellationToken cancellationToken)
    {
        var tiers = await _commissionService.GetTiersAsync(planId, cancellationToken);
        return Ok(tiers);
    }

    /// <summary>
    /// Add a tier to a plan.
    /// </summary>
    [HttpPost("plans/{planId:int}/tiers")]
    [ProducesResponseType(typeof(CommissionTier), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CommissionTier>> AddTier(
        int planId,
        [FromBody] CommissionTierCreateRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var tier = new CommissionTier
        {
            Name = request.Name,
            TierOrder = request.TierOrder ?? 1,
            MinValue = request.MinValue ?? 0,

            MinAttainmentPercent = request.MinAttainmentPercent ?? 0,
            MaxAttainmentPercent = request.MaxAttainmentPercent,
            CommissionRate = request.CommissionRate ?? 0m,
            FixedAmount = request.FixedAmount,
            Multiplier = request.Multiplier ?? 1.0m
        };

        try
        {
            var created = await _commissionService.AddTierAsync(planId, tier, cancellationToken);
            return CreatedAtAction(nameof(GetTiers), new { planId }, created);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding tier to plan {PlanId}", planId);
            return HandleServiceException(ex);
        }
    }

    /// <summary>
    /// Update a tier.
    /// </summary>
    [HttpPut("tiers/{tierId:int}")]
    [ProducesResponseType(typeof(CommissionTier), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CommissionTier>> UpdateTier(
        int tierId,
        [FromBody] CommissionTierUpdateRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var existingTiers = await _commissionService.GetTiersAsync(request.PlanId ?? 0, cancellationToken);
        var existing = existingTiers.FirstOrDefault(t => t.Id == tierId);
        if (existing == null)
        {
            return NotFound($"Commission tier {tierId} not found.");
        }

        existing.Name = request.Name ?? existing.Name;
        existing.TierOrder = request.TierOrder ?? existing.TierOrder;
        existing.MinValue = request.MinValue ?? existing.MinValue;

        existing.MinAttainmentPercent = request.MinAttainmentPercent ?? existing.MinAttainmentPercent;
        existing.MaxAttainmentPercent = request.MaxAttainmentPercent ?? existing.MaxAttainmentPercent;
        existing.CommissionRate = request.CommissionRate ?? existing.CommissionRate;
        existing.FixedAmount = request.FixedAmount ?? existing.FixedAmount;
        existing.Multiplier = request.Multiplier ?? existing.Multiplier;

        try
        {
            var updated = await _commissionService.UpdateTierAsync(existing, cancellationToken);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating commission tier {TierId}", tierId);
            return HandleServiceException(ex);
        }
    }

    /// <summary>
    /// Remove a tier from a plan.
    /// </summary>
    [HttpDelete("tiers/{tierId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveTier(int tierId, CancellationToken cancellationToken)
    {
        var result = await _commissionService.RemoveTierAsync(tierId, cancellationToken);
        if (!result)
        {
            return NotFound($"Commission tier {tierId} not found.");
        }

        return NoContent();
    }

    #endregion

    #region Commission Statements

    /// <summary>
    /// Generate a commission statement for a user.
    /// </summary>
    [HttpPost("statements/generate")]
    [ProducesResponseType(typeof(CommissionStatement), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CommissionStatement>> GenerateStatement(
        [FromBody] CommissionStatementGenerateRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (request.ToDate <= request.FromDate)
        {
            return BadRequest("ToDate must be after FromDate.");
        }

        try
        {
            var statement = await _commissionService.GenerateStatementAsync(
                request.UserId, request.FromDate, request.ToDate, cancellationToken);
            return CreatedAtAction(nameof(GetStatementById), new { statementId = statement.Id }, statement);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating commission statement for user {UserId}", request.UserId);
            return HandleServiceException(ex);
        }
    }

    /// <summary>
    /// Get statements for a user.
    /// </summary>
    [HttpGet("statements/user/{userId:int}")]
    [ProducesResponseType(typeof(IEnumerable<CommissionStatement>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CommissionStatement>>> GetStatements(
        int userId,
        CancellationToken cancellationToken)
    {
        var statements = await _commissionService.GetStatementsAsync(userId, cancellationToken);
        return Ok(statements);
    }

    /// <summary>
    /// Get a statement by ID.
    /// </summary>
    [HttpGet("statements/{statementId:int}")]
    [ProducesResponseType(typeof(CommissionStatement), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommissionStatement>> GetStatementById(
        int statementId,
        CancellationToken cancellationToken)
    {
        var statement = await _commissionService.GetStatementByIdAsync(statementId, cancellationToken);
        if (statement == null)
        {
            return NotFound($"Commission statement {statementId} not found.");
        }

        return Ok(statement);
    }

    /// <summary>
    /// Finalize a statement for payout.
    /// </summary>
    [HttpPost("statements/{statementId:int}/finalize")]
    [ProducesResponseType(typeof(CommissionStatement), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CommissionStatement>> FinalizeStatement(
        int statementId,
        CancellationToken cancellationToken)
    {
        try
        {
            var statement = await _commissionService.FinalizeStatementAsync(statementId, cancellationToken);
            return Ok(statement);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finalizing statement {StatementId}", statementId);
            return HandleServiceException(ex);
        }
    }

    #endregion

    #region Helpers

    private static string GeneratePlanCode(string name)
    {
        // Generate a code from the plan name (e.g., "Standard Sales Plan" -> "SSP-2026")
        var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var initials = string.Concat(words.Select(w => char.ToUpperInvariant(w[0])));
        return $"{initials}-{DateTime.UtcNow.Year}";
    }

    private ActionResult HandleServiceException(Exception ex)
    {
        if (ex is InvalidOperationException)
        {
            return BadRequest(ex.Message);
        }

        return StatusCode(500, "An unexpected error occurred.");
    }

    #endregion
}

#region Request DTOs

/// <summary>
/// Request to create a commission.
/// </summary>
public class CommissionCreateRequest
{
    [Required]
    public int UserId { get; set; }

    public int? CommissionPlanId { get; set; }
    public int? OpportunityId { get; set; }
    public int? OrderId { get; set; }
    public int? InvoiceId { get; set; }
    public int? SubscriptionId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "DealAmount must be greater than 0.")]
    public decimal DealAmount { get; set; }

    public decimal? CommissionableAmount { get; set; }

    [Required]
    [Range(0, 1, ErrorMessage = "CommissionRate must be between 0 and 1.")]
    public decimal CommissionRate { get; set; }

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "CommissionAmount must be >= 0.")]
    public decimal CommissionAmount { get; set; }

    public decimal? SplitPercent { get; set; }
    public decimal? FinalCommissionAmount { get; set; }
    public string? CurrencyCode { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Request to update a commission.
/// </summary>
public class CommissionUpdateRequest
{
    [Range(0.01, double.MaxValue)]
    public decimal? DealAmount { get; set; }

    public decimal? CommissionableAmount { get; set; }

    [Range(0, 1)]
    public decimal? CommissionRate { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? CommissionAmount { get; set; }

    public decimal? SplitPercent { get; set; }
    public decimal? FinalCommissionAmount { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Request to update commission status.
/// </summary>
public class CommissionStatusUpdateRequest
{
    [Required]
    public CommissionStatus Status { get; set; }
}

/// <summary>
/// Request to approve a commission.
/// </summary>
public class CommissionApproveRequest
{
    [Required]
    public int ApprovedById { get; set; }
}

/// <summary>
/// Request to reject a commission.
/// </summary>
public class CommissionRejectRequest
{
    [Required]
    [MinLength(5, ErrorMessage = "Reason must be at least 5 characters.")]
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Request to mark a commission as paid.
/// </summary>
public class CommissionMarkPaidRequest
{
    public DateTime? PaidDate { get; set; }
}

/// <summary>
/// Request to claw back a commission.
/// </summary>
public class CommissionClawbackRequest
{
    [Required]
    [MinLength(5, ErrorMessage = "Reason must be at least 5 characters.")]
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Request to create a commission plan.
/// </summary>
public class CommissionPlanCreateRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Code { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public DateTime? EffectiveStartDate { get; set; }
    public DateTime? EffectiveEndDate { get; set; }
    public int? FiscalYear { get; set; }
    public CommissionType? CommissionType { get; set; }

    [Range(0, 1)]
    public decimal? BaseRate { get; set; }

    public CommissionTrigger? Trigger { get; set; }
    public int? ClawbackPeriodDays { get; set; }
    public decimal? MinDealSize { get; set; }
    public decimal? MaxCommissionPerDeal { get; set; }
    public decimal? MaxCommissionPerPeriod { get; set; }
    public bool? AllowSplits { get; set; }
    public decimal? DefaultOverlayPercent { get; set; }
}

/// <summary>
/// Request to update a commission plan.
/// </summary>
public class CommissionPlanUpdateRequest
{
    [MaxLength(200)]
    public string? Name { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public CommissionPlanStatus? Status { get; set; }
    public DateTime? EffectiveStartDate { get; set; }
    public DateTime? EffectiveEndDate { get; set; }
    public int? FiscalYear { get; set; }
    public CommissionType? CommissionType { get; set; }

    [Range(0, 1)]
    public decimal? BaseRate { get; set; }

    public CommissionTrigger? Trigger { get; set; }
    public int? ClawbackPeriodDays { get; set; }
    public decimal? MinDealSize { get; set; }
    public decimal? MaxCommissionPerDeal { get; set; }
    public decimal? MaxCommissionPerPeriod { get; set; }
    public bool? AllowSplits { get; set; }
    public decimal? DefaultOverlayPercent { get; set; }
}

/// <summary>
/// Request to assign a plan to a user.
/// </summary>
public class CommissionPlanAssignRequest
{
    [Required]
    public int UserId { get; set; }

    public DateTime? EffectiveDate { get; set; }
}

/// <summary>
/// Request to create a commission tier.
/// </summary>
public class CommissionTierCreateRequest
{
    [MaxLength(100)]
    public string? Name { get; set; }

    public int? TierOrder { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public decimal? MinAttainmentPercent { get; set; }
    public decimal? MaxAttainmentPercent { get; set; }

    [Range(0, 1)]
    public decimal? CommissionRate { get; set; }

    public decimal? FixedAmount { get; set; }
    public decimal? Multiplier { get; set; }
}

/// <summary>
/// Request to update a commission tier.
/// </summary>
public class CommissionTierUpdateRequest
{
    public int? PlanId { get; set; }

    [MaxLength(100)]
    public string? Name { get; set; }

    public int? TierOrder { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public decimal? MinAttainmentPercent { get; set; }
    public decimal? MaxAttainmentPercent { get; set; }

    [Range(0, 1)]
    public decimal? CommissionRate { get; set; }

    public decimal? FixedAmount { get; set; }
    public decimal? Multiplier { get; set; }
}

/// <summary>
/// Request to generate a commission statement.
/// </summary>
public class CommissionStatementGenerateRequest
{
    [Required]
    public int UserId { get; set; }

    [Required]
    public DateTime FromDate { get; set; }

    [Required]
    public DateTime ToDate { get; set; }
}

#endregion
