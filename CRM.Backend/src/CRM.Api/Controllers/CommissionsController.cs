// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// API controller for commission management, plans, tiers, statements, and payouts.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class CommissionsController : CrmControllerBase
{
    private const string CommissionPlanNotFoundMessage = "Commission plan {0} not found.";
    private const string CommissionNotFoundMessage = "Commission {0} not found.";
    private readonly ICommissionService _commissionService;
    private readonly ICommissionRulesEngine _commissionRulesEngine;
    private readonly ICommissionRuleService _commissionRuleService;
    private readonly IOpportunityService _opportunityService;
    private readonly IOrderService _orderService;
    private readonly ILogger<CommissionsController> _logger;

    public CommissionsController(
        ICommissionService commissionService,
        ICommissionRulesEngine commissionRulesEngine,
        ICommissionRuleService commissionRuleService,
        IOpportunityService opportunityService,
        IOrderService orderService,
        ILogger<CommissionsController> logger)
    {
        _commissionService = commissionService;
        _commissionRulesEngine = commissionRulesEngine;
        _commissionRuleService = commissionRuleService;
        _opportunityService = opportunityService;
        _orderService = orderService;
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
    [ProducesResponseType(typeof(CommissionDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CommissionDetailsDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var commission = await _commissionService.GetByIdAsync(id, cancellationToken);
        if (commission == null)
        {
            return NotFound(string.Format(CommissionNotFoundMessage, id));
        }

        var agentFirstName = commission.User?.FirstName ?? string.Empty;
        var agentLastName = commission.User?.LastName ?? string.Empty;
        var agentName = $"{agentFirstName} {agentLastName}".Trim();
        if (string.IsNullOrEmpty(agentName))
        {
            agentName = commission.User?.Username ?? $"User #{commission.UserId}";
        }

        var dto = new CommissionDetailsDto
        {
            Id = commission.Id,
            CommissionNumber = commission.CommissionNumber ?? string.Empty,
            UserId = commission.UserId,
            UserName = commission.User?.Username,
            CommissionPlanId = commission.CommissionPlanId,
            PlanName = commission.CommissionPlan?.Name,
            OpportunityId = commission.OpportunityId,
            OrderId = commission.OrderId,
            InvoiceId = commission.InvoiceId,
            SubscriptionId = commission.SubscriptionId,
            DealAmount = commission.DealAmount,
            CommissionableAmount = commission.CommissionableAmount,
            CommissionRate = commission.CommissionRate,
            CommissionAmount = commission.CommissionAmount,
            SplitPercent = commission.SplitPercent,
            FinalCommissionAmount = commission.FinalCommissionAmount,
            Status = (int)commission.Status,
            StatusName = commission.Status.ToString(),
            CurrencyCode = commission.CurrencyCode,
            PaidDate = commission.PaidAt,
            ClawbackDate = commission.ClawbackDate,
            ClawbackReason = commission.ClawbackReason,
            Notes = commission.Notes,
            ApprovedById = commission.ApprovedById,
            ApprovedAt = commission.ApprovedAt,
            CreatedAt = commission.CreatedAt,
            UpdatedAt = commission.UpdatedAt,
            AgentName = agentName,
            CommissionPeriod = commission.CommissionPeriod,
            PeriodStartDate = commission.PeriodStartDate,
            PeriodEndDate = commission.PeriodEndDate,
            CommissionPlanName = commission.CommissionPlan?.Name,
            Tiers = commission.CommissionPlan?.Tiers?
                .OrderBy(t => t.MinAttainmentPercent)
                .Select(t => new CommissionTierDetailDto
                {
                    MinAmount = t.MinAttainmentPercent,
                    MaxAmount = t.MaxAttainmentPercent,
                    Rate = t.CommissionRate,
                    IsApplied = commission.CommissionRate == t.CommissionRate,
                })
                .ToList() ?? new List<CommissionTierDetailDto>(),
        };

        return Ok(dto);
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
            return NotFound(string.Format(CommissionNotFoundMessage, id));
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
            return NotFound(string.Format(CommissionNotFoundMessage, id));
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
    /// <remarks>
    /// Routed through <see cref="ICommissionRulesEngine"/> (REM-ORPHAN-002) instead of the flat-rate
    /// <see cref="ICommissionService.CalculateForDealAsync"/>, so tiered rates, caps, and team splits
    /// configured on <c>CommissionRule</c> records are honored.
    /// </remarks>
    [HttpGet("calculate/deal/{opportunityId:int}")]
    [ProducesResponseType(typeof(CommissionCalculationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CommissionCalculationResultDto>> CalculateForDeal(
        int opportunityId,
        CancellationToken cancellationToken)
    {
        try
        {
            var opportunity = await _opportunityService.GetOpportunityByIdAsync(opportunityId);
            if (opportunity == null)
            {
                return NotFound($"Opportunity {opportunityId} not found.");
            }

            var userId = opportunity.SalesOwnerId ?? 0;
            var calculation = await _commissionRulesEngine.CalculateCommissionAsync(opportunityId, userId, cancellationToken);
            return Ok(NormalizeCalculationResult(calculation));
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
    /// <remarks>
    /// Routed through <see cref="ICommissionRulesEngine"/> (REM-ORPHAN-002) instead of the flat-rate
    /// <see cref="ICommissionService.CalculateForOrderAsync"/>. The rules engine is opportunity-centric,
    /// so the order is resolved to its originating opportunity first.
    /// </remarks>
    [HttpGet("calculate/order/{orderId:int}")]
    [ProducesResponseType(typeof(CommissionCalculationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CommissionCalculationResultDto>> CalculateForOrder(
        int orderId,
        CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderService.GetByIdAsync(orderId, cancellationToken);
            if (order == null)
            {
                return NotFound($"Order {orderId} not found.");
            }

            if (!order.OpportunityId.HasValue)
            {
                return BadRequest($"Order {orderId} is not linked to an opportunity; commission calculation requires an associated opportunity.");
            }

            var opportunity = await _opportunityService.GetOpportunityByIdAsync(order.OpportunityId.Value);
            var userId = opportunity?.SalesOwnerId ?? 0;

            var calculation = await _commissionRulesEngine.CalculateCommissionAsync(order.OpportunityId.Value, userId, cancellationToken);
            calculation.OrderId = orderId;
            return Ok(NormalizeCalculationResult(calculation));
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

    #region Commission Rules

    /// <summary>
    /// Get all commission rules.
    /// </summary>
    [HttpGet("rules")]
    [ProducesResponseType(typeof(IEnumerable<CommissionRuleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CommissionRuleDto>>> GetRules(CancellationToken cancellationToken)
    {
        var rules = await _commissionRuleService.GetAllAsync(cancellationToken);
        return Ok(rules);
    }

    /// <summary>
    /// Get a commission rule by ID.
    /// </summary>
    [HttpGet("rules/{id:int}")]
    [ProducesResponseType(typeof(CommissionRuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommissionRuleDto>> GetRuleById(int id, CancellationToken cancellationToken)
    {
        var rule = await _commissionRuleService.GetByIdAsync(id, cancellationToken);
        if (rule == null)
        {
            return NotFound($"Commission rule {id} not found.");
        }

        return Ok(rule);
    }

    /// <summary>
    /// Get commission rules applicable to a sale type.
    /// </summary>
    [HttpGet("rules/applicable/{saleType}")]
    [ProducesResponseType(typeof(IEnumerable<CommissionRuleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CommissionRuleDto>>> GetApplicableRules(
        string saleType,
        CancellationToken cancellationToken)
    {
        var rules = await _commissionRuleService.GetApplicableRulesAsync(saleType, cancellationToken);
        return Ok(rules);
    }

    /// <summary>
    /// Create a commission rule.
    /// </summary>
    [HttpPost("rules")]
    [ProducesResponseType(typeof(CommissionRuleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CommissionRuleDto>> CreateRule(
        [FromBody] CreateCommissionRuleDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await _commissionRuleService.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetRuleById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Update a commission rule.
    /// </summary>
    [HttpPut("rules/{id:int}")]
    [ProducesResponseType(typeof(CommissionRuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommissionRuleDto>> UpdateRule(
        int id,
        [FromBody] UpdateCommissionRuleDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _commissionRuleService.UpdateAsync(id, dto, cancellationToken);
            return Ok(updated);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Delete a commission rule (soft delete).
    /// </summary>
    [HttpDelete("rules/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRule(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _commissionRuleService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
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
            return NotFound(string.Format(CommissionPlanNotFoundMessage, planId));
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
            return NotFound(string.Format(CommissionPlanNotFoundMessage, planId));
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
            return NotFound(string.Format(CommissionPlanNotFoundMessage, planId));
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
            Name = request.Name ?? string.Empty,
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

    #region Commission Analytics

    /// <summary>
    /// Get commission analytics by time period.
    /// </summary>
    [HttpGet("analytics/by-period")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAnalyticsByPeriod(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var allCommissions = await _commissionService.GetAllAsync(null, null, cancellationToken);
        var fromDate = from ?? DateTime.UtcNow.AddMonths(-12);
        var toDate = to ?? DateTime.UtcNow;

        var filtered = allCommissions
            .Where(c => c.CreatedAt >= fromDate && c.CreatedAt <= toDate)
            .ToList();

        var byMonth = filtered
            .GroupBy(c => new { c.CreatedAt.Year, c.CreatedAt.Month })
            .Select(g => new
            {
                Period = $"{g.Key.Year}-{g.Key.Month:D2}",
                TotalAmount = g.Sum(c => c.CommissionAmount),
                Count = g.Count()
            })
            .OrderBy(x => x.Period)
            .ToList();

        return Ok(new { periods = byMonth, totalAmount = filtered.Sum(c => c.CommissionAmount), totalCount = filtered.Count });
    }

    /// <summary>
    /// Get commission analytics by sales rep.
    /// </summary>
    [HttpGet("analytics/by-rep")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAnalyticsByRep(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var allCommissions = await _commissionService.GetAllAsync(null, null, cancellationToken);
        var fromDate = from ?? DateTime.UtcNow.AddMonths(-12);
        var toDate = to ?? DateTime.UtcNow;

        var filtered = allCommissions
            .Where(c => c.CreatedAt >= fromDate && c.CreatedAt <= toDate)
            .ToList();

        var byRep = filtered
            .GroupBy(c => c.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                TotalAmount = g.Sum(c => c.CommissionAmount),
                Count = g.Count(),
                AverageAmount = g.Average(c => c.CommissionAmount)
            })
            .OrderByDescending(x => x.TotalAmount)
            .ToList();

        return Ok(new { reps = byRep, totalAmount = filtered.Sum(c => c.CommissionAmount) });
    }

    /// <summary>
    /// Get commission analytics overview.
    /// </summary>
    [HttpGet("analytics/overview")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAnalyticsOverview(CancellationToken cancellationToken = default)
    {
        var allCommissions = await _commissionService.GetAllAsync(null, null, cancellationToken);
        var thisMonth = allCommissions.Where(c => c.CreatedAt >= new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1)).ToList();
        var lastMonth = allCommissions.Where(c => c.CreatedAt >= new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-1) && c.CreatedAt < new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1)).ToList();

        return Ok(new
        {
            totalCommissions = allCommissions.Count(),
            totalAmount = allCommissions.Sum(c => c.CommissionAmount),
            thisMonthAmount = thisMonth.Sum(c => c.CommissionAmount),
            thisMonthCount = thisMonth.Count,
            lastMonthAmount = lastMonth.Sum(c => c.CommissionAmount),
            lastMonthCount = lastMonth.Count,
            uniqueReps = allCommissions.Select(c => c.UserId).Distinct().Count()
        });
    }

    /// <summary>
    /// Get commission periods.
    /// </summary>
    [HttpGet("periods")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPeriods(
        [FromQuery] int months = 12,
        CancellationToken cancellationToken = default)
    {
        var allCommissions = await _commissionService.GetAllAsync(null, null, cancellationToken);
        var cutoff = DateTime.UtcNow.AddMonths(-months);

        var periods = allCommissions
            .Where(c => c.CreatedAt >= cutoff)
            .GroupBy(c => new { c.CreatedAt.Year, c.CreatedAt.Month })
            .Select(g => new
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Period = $"{g.Key.Year}-{g.Key.Month:D2}",
                TotalAmount = g.Sum(c => c.CommissionAmount),
                Count = g.Count()
            })
            .OrderByDescending(x => x.Year)
            .ThenByDescending(x => x.Month)
            .ToList();

        return Ok(new { periods });
    }

    /// <summary>
    /// Get commission settings/configuration.
    /// </summary>
    [HttpGet("settings")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSettings(CancellationToken cancellationToken = default)
    {
        var plans = await _commissionService.GetPlansAsync(null, cancellationToken);
        return Ok(new
        {
            totalPlans = plans.Count(),
            activePlans = plans.Count(p => p.IsActive),
            plans = plans.Select(p => new { p.Id, p.Name, p.Code, p.IsActive, p.BaseRate, p.EffectiveStartDate, p.EffectiveEndDate })
        });
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

    /// <summary>
    /// Fills the legacy-shaped fields on <see cref="CommissionCalculationResultDto"/> (BaseAmount,
    /// CommissionRate, FinalAmount, Breakdown) from the engine-native fields it actually populates
    /// (BaseCommissionAmount, BaseCommissionRate, FinalCommissionAmount), preserving the response
    /// contract that existed when this endpoint was backed by <c>CommissionService</c>.
    /// </summary>
    private static CommissionCalculationResultDto NormalizeCalculationResult(CommissionCalculationResultDto result)
    {
        result.BaseAmount = result.BaseCommissionAmount;
        result.CommissionRate = result.BaseCommissionRate;
        result.FinalAmount = result.FinalCommissionAmount;
        result.Amount = result.FinalCommissionAmount;

        if (result.Breakdown.Count == 0)
        {
            result.Breakdown.Add(new CommissionBreakdownDto
            {
                Description = "Rules Engine Commission",
                Amount = result.DealAmount,
                Rate = result.BaseCommissionRate,
                Result = result.FinalCommissionAmount
            });
        }

        return result;
    }

    private static ActionResult HandleServiceException(Exception ex)
    {
        if (ex is InvalidOperationException)
        {
            return new BadRequestObjectResult(ex.Message);
        }

        return new ObjectResult("An unexpected error occurred.") { StatusCode = 500 };
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
