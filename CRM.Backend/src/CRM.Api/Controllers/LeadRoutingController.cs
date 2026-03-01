// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// API controller for managing lead routing rules, criteria, targets, and routing operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LeadRoutingController : CrmControllerBase
{
    private readonly ILeadRoutingService _leadRoutingService;

    public LeadRoutingController(ILeadRoutingService leadRoutingService)
    {
        _leadRoutingService = leadRoutingService;
    }

    #region Rule Management

    /// <summary>
    /// Get all lead routing rules with optional filtering.
    /// </summary>
    [HttpGet("rules")]
    [ProducesResponseType(typeof(IEnumerable<LeadRoutingRule>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LeadRoutingRule>>> GetAllRules(
        [FromQuery] RoutingRuleStatus? status = null,
        [FromQuery] int? teamId = null,
        CancellationToken cancellationToken = default)
    {
        var rules = await _leadRoutingService.GetAllRulesAsync(status, teamId, cancellationToken);
        return Ok(rules);
    }

    /// <summary>
    /// Get a routing rule by ID.
    /// </summary>
    [HttpGet("rules/{id:int}")]
    [ProducesResponseType(typeof(LeadRoutingRule), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LeadRoutingRule>> GetRuleById(int id, CancellationToken cancellationToken)
    {
        var rule = await _leadRoutingService.GetRuleByIdAsync(id, cancellationToken);
        if (rule == null)
        {
            return NotFound($"Routing rule with ID {id} not found.");
        }
        return Ok(rule);
    }

    /// <summary>
    /// Create a new routing rule.
    /// </summary>
    [HttpPost("rules")]
    [ProducesResponseType(typeof(LeadRoutingRule), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LeadRoutingRule>> CreateRule(
        [FromBody] LeadRoutingRule rule,
        CancellationToken cancellationToken)
    {
        var created = await _leadRoutingService.CreateRuleAsync(rule, cancellationToken);
        return CreatedAtAction(nameof(GetRuleById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Update an existing routing rule.
    /// </summary>
    [HttpPut("rules/{id:int}")]
    [ProducesResponseType(typeof(LeadRoutingRule), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LeadRoutingRule>> UpdateRule(
        int id,
        [FromBody] LeadRoutingRule rule,
        CancellationToken cancellationToken)
    {
        if (id != rule.Id)
        {
            return BadRequest("ID mismatch between URL and body.");
        }

        var updated = await _leadRoutingService.UpdateRuleAsync(rule, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// Delete a routing rule (soft delete).
    /// </summary>
    [HttpDelete("rules/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteRule(int id, CancellationToken cancellationToken)
    {
        var result = await _leadRoutingService.DeleteRuleAsync(id, cancellationToken);
        if (!result)
        {
            return NotFound($"Routing rule with ID {id} not found.");
        }
        return NoContent();
    }

    /// <summary>
    /// Activate a routing rule.
    /// </summary>
    [HttpPost("rules/{id:int}/activate")]
    [ProducesResponseType(typeof(LeadRoutingRule), StatusCodes.Status200OK)]
    public async Task<ActionResult<LeadRoutingRule>> ActivateRule(int id, CancellationToken cancellationToken)
    {
        var rule = await _leadRoutingService.ActivateRuleAsync(id, cancellationToken);
        return Ok(rule);
    }

    /// <summary>
    /// Deactivate a routing rule.
    /// </summary>
    [HttpPost("rules/{id:int}/deactivate")]
    [ProducesResponseType(typeof(LeadRoutingRule), StatusCodes.Status200OK)]
    public async Task<ActionResult<LeadRoutingRule>> DeactivateRule(int id, CancellationToken cancellationToken)
    {
        var rule = await _leadRoutingService.DeactivateRuleAsync(id, cancellationToken);
        return Ok(rule);
    }

    #endregion

    #region Rule Criteria Management

    /// <summary>
    /// Get all criteria for a routing rule.
    /// </summary>
    [HttpGet("rules/{ruleId:int}/criteria")]
    [ProducesResponseType(typeof(IEnumerable<LeadRoutingCriteria>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LeadRoutingCriteria>>> GetCriteria(
        int ruleId,
        CancellationToken cancellationToken)
    {
        var criteria = await _leadRoutingService.GetCriteriaAsync(ruleId, cancellationToken);
        return Ok(criteria);
    }

    /// <summary>
    /// Add criteria to a routing rule.
    /// </summary>
    [HttpPost("rules/{ruleId:int}/criteria")]
    [ProducesResponseType(typeof(LeadRoutingCriteria), StatusCodes.Status200OK)]
    public async Task<ActionResult<LeadRoutingCriteria>> AddCriteria(
        int ruleId,
        [FromBody] LeadRoutingCriteria criteria,
        CancellationToken cancellationToken)
    {
        var added = await _leadRoutingService.AddCriteriaAsync(ruleId, criteria, cancellationToken);
        return Ok(added);
    }

    /// <summary>
    /// Update a routing criteria.
    /// </summary>
    [HttpPut("criteria/{criteriaId:int}")]
    [ProducesResponseType(typeof(LeadRoutingCriteria), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LeadRoutingCriteria>> UpdateCriteria(
        int criteriaId,
        [FromBody] LeadRoutingCriteria criteria,
        CancellationToken cancellationToken)
    {
        if (criteriaId != criteria.Id)
        {
            return BadRequest("ID mismatch between URL and body.");
        }

        var updated = await _leadRoutingService.UpdateCriteriaAsync(criteria, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// Remove criteria from a routing rule.
    /// </summary>
    [HttpDelete("criteria/{criteriaId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RemoveCriteria(int criteriaId, CancellationToken cancellationToken)
    {
        var result = await _leadRoutingService.RemoveCriteriaAsync(criteriaId, cancellationToken);
        if (!result)
        {
            return NotFound($"Criteria with ID {criteriaId} not found.");
        }
        return NoContent();
    }

    #endregion

    #region Routing Target Management

    /// <summary>
    /// Get all targets for a routing rule.
    /// </summary>
    [HttpGet("rules/{ruleId:int}/targets")]
    [ProducesResponseType(typeof(IEnumerable<LeadRoutingTarget>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LeadRoutingTarget>>> GetTargets(
        int ruleId,
        CancellationToken cancellationToken)
    {
        var targets = await _leadRoutingService.GetTargetsAsync(ruleId, cancellationToken);
        return Ok(targets);
    }

    /// <summary>
    /// Add a routing target to a rule.
    /// </summary>
    [HttpPost("rules/{ruleId:int}/targets")]
    [ProducesResponseType(typeof(LeadRoutingTarget), StatusCodes.Status200OK)]
    public async Task<ActionResult<LeadRoutingTarget>> AddTarget(
        int ruleId,
        [FromBody] LeadRoutingTarget target,
        CancellationToken cancellationToken)
    {
        var added = await _leadRoutingService.AddTargetAsync(ruleId, target, cancellationToken);
        return Ok(added);
    }

    /// <summary>
    /// Update a routing target.
    /// </summary>
    [HttpPut("targets/{targetId:int}")]
    [ProducesResponseType(typeof(LeadRoutingTarget), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LeadRoutingTarget>> UpdateTarget(
        int targetId,
        [FromBody] LeadRoutingTarget target,
        CancellationToken cancellationToken)
    {
        if (targetId != target.Id)
        {
            return BadRequest("ID mismatch between URL and body.");
        }

        var updated = await _leadRoutingService.UpdateTargetAsync(target, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// Remove a routing target.
    /// </summary>
    [HttpDelete("targets/{targetId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RemoveTarget(int targetId, CancellationToken cancellationToken)
    {
        var result = await _leadRoutingService.RemoveTargetAsync(targetId, cancellationToken);
        if (!result)
        {
            return NotFound($"Target with ID {targetId} not found.");
        }
        return NoContent();
    }

    /// <summary>
    /// Get capacity information for a routing target.
    /// </summary>
    [HttpGet("targets/{targetId:int}/capacity")]
    [ProducesResponseType(typeof(TargetCapacity), StatusCodes.Status200OK)]
    public async Task<ActionResult<TargetCapacity>> GetTargetCapacity(int targetId, CancellationToken cancellationToken)
    {
        var capacity = await _leadRoutingService.GetTargetCapacityAsync(targetId, cancellationToken);
        return Ok(capacity);
    }

    #endregion

    #region Lead Routing Operations

    /// <summary>
    /// Route a lead using all active rules.
    /// </summary>
    [HttpPost("leads/{leadId:int}/route")]
    [ProducesResponseType(typeof(LeadRoutingResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<LeadRoutingResult>> RouteLead(int leadId, CancellationToken cancellationToken)
    {
        var result = await _leadRoutingService.RouteLeadAsync(leadId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Route a lead using a specific rule.
    /// </summary>
    [HttpPost("leads/{leadId:int}/route/{ruleId:int}")]
    [ProducesResponseType(typeof(LeadRoutingResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<LeadRoutingResult>> RouteLeadWithRule(
        int leadId,
        int ruleId,
        CancellationToken cancellationToken)
    {
        var result = await _leadRoutingService.RouteLeadWithRuleAsync(leadId, ruleId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Evaluate which rules match a lead without assigning.
    /// </summary>
    [HttpPost("leads/{leadId:int}/evaluate")]
    [ProducesResponseType(typeof(IEnumerable<LeadRoutingRule>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LeadRoutingRule>>> EvaluateMatchingRules(
        int leadId,
        CancellationToken cancellationToken)
    {
        var rules = await _leadRoutingService.EvaluateMatchingRulesAsync(leadId, cancellationToken);
        return Ok(rules);
    }

    /// <summary>
    /// Route multiple leads in batch.
    /// </summary>
    [HttpPost("leads/batch-route")]
    [ProducesResponseType(typeof(IEnumerable<LeadRoutingResult>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LeadRoutingResult>>> RouteLeadsBatch(
        [FromBody] IEnumerable<int> leadIds,
        CancellationToken cancellationToken)
    {
        var results = await _leadRoutingService.RouteLeadsBatchAsync(leadIds, cancellationToken);
        return Ok(results);
    }

    /// <summary>
    /// Re-route a lead (clear current assignment and route again).
    /// </summary>
    [HttpPost("leads/{leadId:int}/reroute")]
    [ProducesResponseType(typeof(LeadRoutingResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<LeadRoutingResult>> RerouteLead(
        int leadId,
        [FromBody] RerouteLeadRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _leadRoutingService.RerouteLeadAsync(leadId, request?.Reason, cancellationToken);
        return Ok(result);
    }

    #endregion

    #region Routing Logs

    /// <summary>
    /// Get routing history for a lead.
    /// </summary>
    [HttpGet("leads/{leadId:int}/history")]
    [ProducesResponseType(typeof(IEnumerable<LeadRoutingLog>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LeadRoutingLog>>> GetLeadRoutingHistory(
        int leadId,
        CancellationToken cancellationToken)
    {
        var logs = await _leadRoutingService.GetLeadRoutingHistoryAsync(leadId, cancellationToken);
        return Ok(logs);
    }

    /// <summary>
    /// Get routing logs for a specific rule.
    /// </summary>
    [HttpGet("rules/{ruleId:int}/logs")]
    [ProducesResponseType(typeof(IEnumerable<LeadRoutingLog>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LeadRoutingLog>>> GetRuleRoutingLogs(
        int ruleId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var logs = await _leadRoutingService.GetRuleRoutingLogsAsync(ruleId, fromDate, toDate, cancellationToken);
        return Ok(logs);
    }

    /// <summary>
    /// Get routing logs for a specific user (assignee).
    /// </summary>
    [HttpGet("users/{userId:int}/logs")]
    [ProducesResponseType(typeof(IEnumerable<LeadRoutingLog>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LeadRoutingLog>>> GetUserRoutingLogs(
        int userId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var logs = await _leadRoutingService.GetUserRoutingLogsAsync(userId, fromDate, toDate, cancellationToken);
        return Ok(logs);
    }

    #endregion

    #region Statistics & Analytics

    /// <summary>
    /// Get statistics for a specific rule.
    /// </summary>
    [HttpGet("rules/{ruleId:int}/statistics")]
    [ProducesResponseType(typeof(LeadRoutingStatistics), StatusCodes.Status200OK)]
    public async Task<ActionResult<LeadRoutingStatistics>> GetRuleStatistics(
        int ruleId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var stats = await _leadRoutingService.GetRuleStatisticsAsync(ruleId, fromDate, toDate, cancellationToken);
        return Ok(stats);
    }

    /// <summary>
    /// Get overall routing statistics.
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(LeadRoutingStatistics), StatusCodes.Status200OK)]
    public async Task<ActionResult<LeadRoutingStatistics>> GetOverallStatistics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var stats = await _leadRoutingService.GetOverallStatisticsAsync(fromDate, toDate, cancellationToken);
        return Ok(stats);
    }

    /// <summary>
    /// Get response time statistics.
    /// </summary>
    [HttpGet("statistics/response-time")]
    [ProducesResponseType(typeof(ResponseTimeStatistics), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseTimeStatistics>> GetResponseTimeStatistics(
        [FromQuery] int? ruleId = null,
        [FromQuery] int? userId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var stats = await _leadRoutingService.GetResponseTimeStatisticsAsync(ruleId, userId, fromDate, toDate, cancellationToken);
        return Ok(stats);
    }

    #endregion

    #region Capacity Management

    /// <summary>
    /// Reset daily lead counts for all targets.
    /// </summary>
    [HttpPost("targets/reset-daily")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> ResetDailyCounts(CancellationToken cancellationToken)
    {
        await _leadRoutingService.ResetDailyCountsAsync(cancellationToken);
        return Ok(new { message = "Daily counts reset successfully." });
    }

    /// <summary>
    /// Reset weekly lead counts for all targets.
    /// </summary>
    [HttpPost("targets/reset-weekly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> ResetWeeklyCounts(CancellationToken cancellationToken)
    {
        await _leadRoutingService.ResetWeeklyCountsAsync(cancellationToken);
        return Ok(new { message = "Weekly counts reset successfully." });
    }

    #endregion
}

#region Request DTOs

public class RerouteLeadRequest
{
    public string? Reason { get; set; }
}

#endregion
