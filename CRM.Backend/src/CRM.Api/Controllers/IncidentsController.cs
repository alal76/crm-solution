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
using CRM.Api.Infrastructure;
using CRM.Infrastructure.Services.ITSM;

namespace CRM.Api.Controllers;

/// <summary>
/// Incident Management API endpoints for handling service interruptions and issues.
/// </summary>
/// <remarks>
/// Incident Management is the process of managing the lifecycle of all incidents.
/// The primary objective is to return the IT service to users as quickly as possible.
/// Key features include: incident creation, assignment, escalation, resolution, and SLA tracking.
/// </remarks>
[ApiController]
[Route("api/itsm/incidents")]
[Authorize]
[Produces("application/json")]
[Consumes("application/json")]
[Tags("ITSM - Incident Management")]
public class IncidentsController : CrmControllerBase
{
    private readonly IIncidentService _incidentService;
    private readonly IAssignmentRulesEngine _assignmentRulesEngine;
    private readonly IImpactAnalysisService _impactAnalysisService;

    public IncidentsController(
        IIncidentService incidentService,
        IAssignmentRulesEngine assignmentRulesEngine,
        IImpactAnalysisService impactAnalysisService)
    {
        _incidentService = incidentService;
        _assignmentRulesEngine = assignmentRulesEngine;
        _impactAnalysisService = impactAnalysisService;
    }

    /// <summary>
    /// Create a new incident.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(IncidentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IncidentDto>> CreateIncident([FromBody] CreateIncidentDto dto)
    {
        var userId = GetCurrentUserId();
        var incident = await _incidentService.CreateIncidentAsync(dto, userId);
        return CreatedAtAction(nameof(GetIncident), new { id = incident.IncidentId }, incident);
    }

    /// <summary>
    /// Get incident by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(IncidentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IncidentDto>> GetIncident(int id)
    {
        var incident = await _incidentService.GetIncidentByIdAsync(id);
        if (incident == null)
        {
            return NotFound();
        }
        return Ok(incident);
    }

    /// <summary>
    /// Get incidents with filtering and pagination.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<IncidentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<IncidentDto>>> GetIncidents(
        [FromQuery] string? searchTerm,
        [FromQuery] int? state,
        [FromQuery] int? priority,
        [FromQuery] int? assignedToId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var filter = new IncidentFilterDto
        {
            SearchTerm = searchTerm,
            State = state.HasValue ? (IncidentState)state.Value : null,
            Priority = priority,
            AssignedToId = assignedToId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var (items, totalCount) = await _incidentService.GetIncidentsAsync(filter);
        var result = new PagedResult<IncidentDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return Ok(result);
    }

    /// <summary>
    /// Update an incident.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(IncidentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IncidentDto>> UpdateIncident(int id, [FromBody] UpdateIncidentDto dto)
    {
        var userId = GetCurrentUserId();
        var incident = await _incidentService.UpdateIncidentAsync(id, dto, userId);
        return Ok(incident);
    }

    /// <summary>
    /// Assign incident to a user or group.
    /// </summary>
    [HttpPatch("{id}/assign")]
    [ProducesResponseType(typeof(IncidentDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<IncidentDto>> AssignIncident(int id, [FromBody] AssignIncidentDto dto)
    {
        var userId = GetCurrentUserId();
        var incident = await _incidentService.AssignIncidentAsync(id, dto.AssignedToId, dto.AssignedGroupId, userId);
        return Ok(incident);
    }

    /// <summary>
    /// Escalate an incident.
    /// </summary>
    [HttpPatch("{id}/escalate")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> EscalateIncident(int id, [FromBody] EscalateIncidentDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _incidentService.EscalateIncidentAsync(id, userId);
        return Ok(result);
    }

    /// <summary>
    /// Resolve an incident.
    /// </summary>
    [HttpPatch("{id}/resolve")]
    [ProducesResponseType(typeof(IncidentDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<IncidentDto>> ResolveIncident(int id, [FromBody] ResolveIncidentDto dto)
    {
        var userId = GetCurrentUserId();
        var incident = await _incidentService.ResolveIncidentAsync(id, dto, userId);
        return Ok(incident);
    }

    /// <summary>
    /// Close an incident.
    /// </summary>
    [HttpPatch("{id}/close")]
    [ProducesResponseType(typeof(IncidentDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<IncidentDto>> CloseIncident(int id)
    {
        var userId = GetCurrentUserId();
        var incident = await _incidentService.CloseIncidentAsync(id, userId);
        return Ok(incident);
    }

    /// <summary>
    /// Reopen a closed incident.
    /// </summary>
    [HttpPatch("{id}/reopen")]
    [ProducesResponseType(typeof(IncidentDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<IncidentDto>> ReopenIncident(int id)
    {
        var userId = GetCurrentUserId();
        var incident = await _incidentService.ReopenIncidentAsync(id, userId);
        return Ok(incident);
    }

    /// <summary>
    /// Add a comment to an incident.
    /// </summary>
    [HttpPost("{id}/comments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> AddComment(int id, [FromBody] AddIncidentCommentDto dto)
    {
        var userId = GetCurrentUserId();
        await _incidentService.AddCommentAsync(id, dto.CommentText, dto.IsInternal, userId);
        return Ok();
    }

    /// <summary>
    /// Get comments for an incident.
    /// </summary>
    [HttpGet("{id}/comments")]
    [ProducesResponseType(typeof(IEnumerable<IncidentCommentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<IncidentCommentDto>>> GetComments(int id)
    {
        var comments = await _incidentService.GetCommentsAsync(id);
        return Ok(comments);
    }

    // ────────────────────────────────────────────────────────────────
    // Assignment Rules Engine
    // ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Evaluate assignment rules for an incident without applying the result.
    /// </summary>
    [HttpGet("{id:int}/assignment/evaluate")]
    [ProducesResponseType(typeof(AssignmentResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<AssignmentResult>> EvaluateAssignment(int id)
    {
        var result = await _assignmentRulesEngine.EvaluateAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Evaluate assignment rules for an incident and apply the resulting assignment.
    /// </summary>
    [HttpPost("{id:int}/assignment/auto-assign")]
    [ProducesResponseType(typeof(AssignmentResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<AssignmentResult>> AutoAssignIncident(int id)
    {
        var result = await _assignmentRulesEngine.AutoAssignAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Get all configured incident assignment rules.
    /// </summary>
    [HttpGet("assignment-rules")]
    [ProducesResponseType(typeof(List<AssignmentRule>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AssignmentRule>>> GetAssignmentRules()
    {
        var rules = await _assignmentRulesEngine.GetRulesAsync();
        return Ok(rules);
    }

    /// <summary>
    /// Create or update an incident assignment rule. Set <c>RuleId</c> to 0 to create a new rule.
    /// </summary>
    [HttpPut("assignment-rules")]
    [ProducesResponseType(typeof(AssignmentRule), StatusCodes.Status200OK)]
    public async Task<ActionResult<AssignmentRule>> SaveAssignmentRule([FromBody] AssignmentRule rule)
    {
        var saved = await _assignmentRulesEngine.SaveRuleAsync(rule);
        return Ok(saved);
    }

    /// <summary>
    /// Delete an incident assignment rule.
    /// </summary>
    [HttpDelete("assignment-rules/{ruleId:int}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> DeleteAssignmentRule(int ruleId)
    {
        var result = await _assignmentRulesEngine.DeleteRuleAsync(ruleId);
        return Ok(result);
    }

    /// <summary>
    /// Test an assignment rule against an incident without applying it.
    /// </summary>
    [HttpGet("assignment-rules/{ruleId:int}/test/{incidentId:int}")]
    [ProducesResponseType(typeof(RuleTestResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<RuleTestResult>> TestAssignmentRule(int ruleId, int incidentId)
    {
        var result = await _assignmentRulesEngine.TestRuleAsync(ruleId, incidentId);
        return Ok(result);
    }

    /// <summary>
    /// Get workload distribution across assignment groups.
    /// </summary>
    [HttpGet("assignment/group-workloads")]
    [ProducesResponseType(typeof(List<GroupWorkload>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<GroupWorkload>>> GetGroupWorkloads()
    {
        var workloads = await _assignmentRulesEngine.GetGroupWorkloadsAsync();
        return Ok(workloads);
    }

    /// <summary>
    /// Get available agents within an assignment group.
    /// </summary>
    [HttpGet("assignment/available-agents/{groupId:int}")]
    [ProducesResponseType(typeof(List<AvailableAgent>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AvailableAgent>>> GetAvailableAgents(int groupId)
    {
        var agents = await _assignmentRulesEngine.GetAvailableAgentsAsync(groupId);
        return Ok(agents);
    }

    // ────────────────────────────────────────────────────────────────
    // Impact Analysis
    // ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Run a full impact analysis for an incident.
    /// </summary>
    [HttpGet("{id:int}/impact-analysis")]
    [ProducesResponseType(typeof(IncidentImpactAnalysis), StatusCodes.Status200OK)]
    public async Task<ActionResult<IncidentImpactAnalysis>> GetImpactAnalysis(int id)
    {
        var analysis = await _impactAnalysisService.AnalyzeIncidentImpactAsync(id);
        return Ok(analysis);
    }

    /// <summary>
    /// Get services affected by an outage of the given configuration item.
    /// </summary>
    [HttpGet("impact-analysis/affected-services/{configurationItemId:int}")]
    [ProducesResponseType(typeof(List<AffectedService>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AffectedService>>> GetAffectedServices(int configurationItemId)
    {
        var services = await _impactAnalysisService.GetAffectedServicesAsync(configurationItemId);
        return Ok(services);
    }

    /// <summary>
    /// Get users and groups affected by an outage of the given configuration item.
    /// </summary>
    [HttpGet("impact-analysis/affected-users/{configurationItemId:int}")]
    [ProducesResponseType(typeof(AffectedUserGroup), StatusCodes.Status200OK)]
    public async Task<ActionResult<AffectedUserGroup>> GetAffectedUsers(int configurationItemId)
    {
        var users = await _impactAnalysisService.GetAffectedUsersAsync(configurationItemId);
        return Ok(users);
    }

    /// <summary>
    /// Calculate the business impact score for an incident.
    /// </summary>
    [HttpGet("{id:int}/business-impact")]
    [ProducesResponseType(typeof(BusinessImpactScore), StatusCodes.Status200OK)]
    public async Task<ActionResult<BusinessImpactScore>> GetBusinessImpact(int id)
    {
        var score = await _impactAnalysisService.CalculateBusinessImpactAsync(id);
        return Ok(score);
    }

    /// <summary>
    /// Get the upstream/downstream dependency chain for a configuration item.
    /// </summary>
    [HttpGet("impact-analysis/dependency-chain/{configurationItemId:int}")]
    [ProducesResponseType(typeof(DependencyChain), StatusCodes.Status200OK)]
    public async Task<ActionResult<DependencyChain>> GetDependencyChain(int configurationItemId)
    {
        var chain = await _impactAnalysisService.GetDependencyChainAsync(configurationItemId);
        return Ok(chain);
    }

    /// <summary>
    /// Predict the impact of an outage of the given configuration item.
    /// </summary>
    [HttpGet("impact-analysis/predict-outage/{configurationItemId:int}")]
    [ProducesResponseType(typeof(List<PredictedImpact>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PredictedImpact>>> PredictOutageImpact(int configurationItemId)
    {
        var impacts = await _impactAnalysisService.PredictOutageImpactAsync(configurationItemId);
        return Ok(impacts);
    }

    private int GetCurrentUserId() // NOSONAR
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return int.Parse(userIdClaim?.Value ?? "1");
    }
}

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class AssignIncidentDto
{
    public int? AssignedToId { get; set; }
    public int? AssignedGroupId { get; set; }
}

public class EscalateIncidentDto
{
    public int EscalationLevel { get; set; }
}

public class AddIncidentCommentDto
{
    public string CommentText { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
}

public class IncidentCommentDto
{
    public int CommentId { get; set; }
    public string CommentText { get; set; } = string.Empty;
    public int CreatedById { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
    public DateTime CreatedAt { get; set; }
}
