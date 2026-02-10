// This file is part of the CRM Solution.
// Copyright (c) 2025 CRM Solution Contributors
// Licensed under the AGPL-3.0 license.

using CRM.Core.DTOs.ITSM;
using CRM.Core.Interfaces.ITSM;
using CRM.Core.Entities.ITSM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CRM.Api.Controllers;

/// <summary>
/// Problem Management API endpoints for identifying and managing root causes of incidents.
/// </summary>
/// <remarks>
/// Problem Management focuses on preventing incidents from recurring by identifying and resolving root causes.
/// Key features include: problem creation, incident linking, root cause analysis, and known error management.
/// </remarks>
[ApiController]
[Route("api/itsm/problems")]
[Authorize]
[Produces("application/json")]
[Consumes("application/json")]
[Tags("ITSM - Problem Management")]
public class ProblemsController : ControllerBase
{
    private readonly IProblemService _problemService;
    private readonly ILogger<ProblemsController> _logger;

    public ProblemsController(IProblemService problemService, ILogger<ProblemsController> logger)
    {
        _problemService = problemService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new problem record.
    /// </summary>
    /// <param name="dto">Problem creation details</param>
    /// <returns>The created problem</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ProblemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ProblemDto>> CreateProblem([FromBody] CreateProblemDto dto)
    {
        var problem = await _problemService.CreateProblemAsync(dto, GetCurrentUserId());
        return CreatedAtAction(nameof(GetProblem), new { id = problem.ProblemId }, problem);
    }

    /// <summary>
    /// Get a problem by ID.
    /// </summary>
    /// <param name="id">The problem ID</param>
    /// <returns>The problem details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProblemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProblemDto>> GetProblem(int id)
    {
        var problem = await _problemService.GetProblemByIdAsync(id);
        return problem == null ? NotFound() : Ok(problem);
    }

    /// <summary>
    /// Get problems with filtering and pagination.
    /// </summary>
    /// <param name="searchTerm">Optional search term</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <returns>Paginated list of problems</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProblemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ProblemDto>>> GetProblems(
        [FromQuery] string? searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var filter = new ProblemFilterDto
        {
            SearchTerm = searchTerm,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var (items, totalCount) = await _problemService.GetProblemsAsync(filter);
        return Ok(new PagedResult<ProblemDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        });
    }

    /// <summary>
    /// Update an existing problem.
    /// </summary>
    /// <param name="id">The problem ID</param>
    /// <param name="dto">Updated problem data</param>
    /// <returns>The updated problem</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ProblemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProblemDto>> UpdateProblem(int id, [FromBody] UpdateProblemDto dto)
    {
        var problem = await _problemService.UpdateProblemAsync(id, dto, GetCurrentUserId());
        return Ok(problem);
    }

    /// <summary>
    /// Link an incident to a problem.
    /// </summary>
    /// <param name="problemId">The problem ID</param>
    /// <param name="incidentId">The incident ID to link</param>
    /// <returns>Success status</returns>
    [HttpPost("{problemId}/link-incident/{incidentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> LinkIncident(int problemId, int incidentId)
    {
        var result = await _problemService.LinkIncidentAsync(problemId, incidentId, GetCurrentUserId());
        return result ? Ok() : BadRequest();
    }

    /// <summary>
    /// Mark a problem as a known error.
    /// </summary>
    /// <param name="id">The problem ID</param>
    /// <returns>Success status</returns>
    [HttpPatch("{id}/mark-known-error")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> MarkAsKnownError(int id)
    {
        var result = await _problemService.MarkAsKnownErrorAsync(id, GetCurrentUserId());
        return result ? Ok() : BadRequest();
    }

    /// <summary>
    /// Get all incidents related to a problem.
    /// </summary>
    /// <param name="id">The problem ID</param>
    /// <returns>List of related incidents</returns>
    [HttpGet("{id:int}/related-incidents")]
    [HttpGet("{id:int}/incidents")]
    [ProducesResponseType(typeof(IEnumerable<IncidentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<IncidentDto>>> GetRelatedIncidents(int id)
    {
        var incidents = await _problemService.GetRelatedIncidentsAsync(id);
        return Ok(incidents);
    }

    /// <summary>
    /// Update root cause analysis for a problem.
    /// </summary>
    /// <param name="id">The problem ID</param>
    /// <param name="dto">Root cause and workaround details</param>
    /// <returns>Success status</returns>
    [HttpPatch("{id}/rca")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UpdateRootCauseAnalysis(int id, [FromBody] UpdateRCADto dto)
    {
        var result = await _problemService.UpdateRootCauseAnalysisAsync(id, dto.RootCause, dto.Workaround, GetCurrentUserId());
        return result ? Ok() : BadRequest("Failed to update root cause analysis");
    }

    private int GetCurrentUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "1");
}

public class UpdateRCADto
{
    public string RootCause { get; set; } = string.Empty;
    public string? Workaround { get; set; }
}

/// <summary>
/// Configuration Management Database (CMDB) API endpoints.
/// </summary>
/// <remarks>
/// CMDB stores information about Configuration Items (CIs) such as servers, applications, 
/// network devices, and their relationships. Key features include: CI lifecycle management,
/// relationship mapping, impact analysis, and service mapping.
/// </remarks>
[ApiController]
[Route("api/itsm/cmdb")]
[Authorize]
[Produces("application/json")]
[Consumes("application/json")]
[Tags("ITSM - CMDB")]
public class CMDBController : ControllerBase
{
    private readonly ICMDBService _cmdbService;

    public CMDBController(ICMDBService cmdbService)
    {
        _cmdbService = cmdbService;
    }

    /// <summary>
    /// Create a new Configuration Item.
    /// </summary>
    /// <param name="dto">Configuration Item details</param>
    /// <returns>The created CI</returns>
    [HttpPost]
    [HttpPost("cis")]
    [ProducesResponseType(typeof(ConfigurationItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ConfigurationItemDto>> CreateCI([FromBody] CreateCIDto dto)
    {
        var ci = await _cmdbService.CreateCIAsync(dto, GetCurrentUserId());
        return CreatedAtAction(nameof(GetCI), new { id = ci.CIId }, ci);
    }

    /// <summary>
    /// Get a Configuration Item by ID.
    /// </summary>
    /// <param name="id">The CI ID</param>
    /// <returns>The CI details</returns>
    [HttpGet("{id:int}")]
    [HttpGet("cis/{id:int}")]
    [ProducesResponseType(typeof(ConfigurationItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConfigurationItemDto>> GetCI(int id)
    {
        var ci = await _cmdbService.GetCIByIdAsync(id);
        return ci == null ? NotFound() : Ok(ci);
    }

    /// <summary>
    /// Search Configuration Items.
    /// </summary>
    /// <param name="searchTerm">Search term for CI name, description, or asset tag</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <returns>List of matching CIs</returns>
    [HttpGet]
    [HttpGet("cis")]
    [ProducesResponseType(typeof(IEnumerable<ConfigurationItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ConfigurationItemDto>>> SearchCIs(
        [FromQuery] string? searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var cis = await _cmdbService.SearchCIsAsync(searchTerm ?? string.Empty, null, pageNumber, pageSize);
        return Ok(cis);
    }

    /// <summary>
    /// Update a Configuration Item.
    /// </summary>
    /// <param name="id">The CI ID</param>
    /// <param name="dto">Updated CI data</param>
    /// <returns>The updated CI</returns>
    [HttpPut("{id:int}")]
    [HttpPut("cis/{id:int}")]
    [ProducesResponseType(typeof(ConfigurationItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConfigurationItemDto>> UpdateCI(int id, [FromBody] CreateCIDto dto)
    {
        var ci = await _cmdbService.UpdateCIAsync(id, dto, GetCurrentUserId());
        return Ok(ci);
    }

    /// <summary>
    /// Create a relationship between two Configuration Items.
    /// </summary>
    /// <param name="parentId">The parent CI ID</param>
    /// <param name="childId">The child CI ID</param>
    /// <param name="dto">Relationship type details</param>
    /// <returns>Success status</returns>
    [HttpPost("{parentId}/relationships/{childId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CreateRelationship(int parentId, int childId, [FromBody] CreateRelationshipDto dto)
    {
        var result = await _cmdbService.CreateRelationshipAsync(parentId, childId, (RelationshipType)dto.RelationshipType, GetCurrentUserId());
        return result ? Ok() : BadRequest();
    }

    /// <summary>
    /// Get all CIs related to a specific CI.
    /// </summary>
    /// <param name="id">The CI ID</param>
    /// <returns>List of related CIs</returns>
    [HttpGet("{id:int}/related")]
    [HttpGet("cis/{id:int}/relationships")]
    [HttpGet("cis/{id:int}/related")]
    [ProducesResponseType(typeof(IEnumerable<ConfigurationItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ConfigurationItemDto>>> GetRelatedCIs(int id)
    {
        var cis = await _cmdbService.GetRelatedCIsAsync(id);
        return Ok(cis);
    }

    /// <summary>
    /// Get impact analysis for a Configuration Item.
    /// </summary>
    /// <remarks>
    /// Returns a list of downstream dependencies that would be affected if this CI fails.
    /// </remarks>
    /// <param name="id">The CI ID</param>
    /// <returns>List of impact descriptions</returns>
    [HttpGet("{id:int}/impact-analysis")]
    [HttpGet("cis/{id:int}/impact")]
    [HttpGet("cis/{id:int}/impact-analysis")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<string>>> GetImpactAnalysis(int id)
    {
        var impacts = await _cmdbService.GetImpactAnalysisAsync(id);
        return Ok(impacts);
    }

    /// <summary>
    /// Get service map visualization data for a CI.
    /// </summary>
    /// <param name="id">The CI ID</param>
    /// <param name="depth">Depth of relationships to include (default: 2)</param>
    /// <returns>Service map with root CI and related CIs</returns>
    [HttpGet("{id:int}/service-map")]
    [HttpGet("cis/{id:int}/service-map")]
    [ProducesResponseType(typeof(ServiceMapDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceMapDto>> GetServiceMap(int id, [FromQuery] int depth = 2)
    {
        var relatedCIs = await _cmdbService.GetRelatedCIsAsync(id);
        var rootCI = await _cmdbService.GetCIByIdAsync(id);
        
        var serviceMap = new ServiceMapDto
        {
            RootCI = rootCI,
            RelatedCIs = relatedCIs.ToList(),
            Depth = depth
        };
        
        return Ok(serviceMap);
    }

    /// <summary>
    /// Get all available CI types.
    /// </summary>
    /// <returns>List of CI type names</returns>
    [HttpGet("types")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<string>>> GetCITypes()
    {
        // Return the available CI types
        var types = Enum.GetNames(typeof(CIType));
        return Ok(types);
    }

    private int GetCurrentUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "1");
}

public class ServiceMapDto
{
    public ConfigurationItemDto? RootCI { get; set; }
    public List<ConfigurationItemDto> RelatedCIs { get; set; } = new();
    public int Depth { get; set; }
}

public class CreateRelationshipDto
{
    public int RelationshipType { get; set; }
}

/// <summary>
/// Change Management API endpoints for managing IT changes.
/// </summary>
/// <remarks>
/// Change Management ensures that standardized methods and procedures are used for efficient and prompt handling 
/// of all changes. Key features include: change requests, approvals, scheduling, conflict detection, and blackout periods.
/// </remarks>
[ApiController]
[Route("api/itsm/changes")]
[Authorize]
[Produces("application/json")]
[Consumes("application/json")]
[Tags("ITSM - Change Management")]
public class ChangesController : ControllerBase
{
    private readonly IChangeManagementService _changeService;

    public ChangesController(IChangeManagementService changeService)
    {
        _changeService = changeService;
    }

    /// <summary>
    /// Create a new change request.
    /// </summary>
    /// <param name="dto">Change request details</param>
    /// <returns>The created change request</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ChangeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ChangeDto>> CreateChange([FromBody] CreateChangeDto dto)
    {
        var change = await _changeService.CreateChangeAsync(dto, GetCurrentUserId());
        return CreatedAtAction(nameof(GetChange), new { id = change.ChangeId }, change);
    }

    /// <summary>
    /// Get a change request by ID.
    /// </summary>
    /// <param name="id">The change ID</param>
    /// <returns>The change request details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ChangeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChangeDto>> GetChange(int id)
    {
        var change = await _changeService.GetChangeByIdAsync(id);
        return change == null ? NotFound() : Ok(change);
    }

    /// <summary>
    /// Get change requests with filtering and pagination.
    /// </summary>
    /// <param name="searchTerm">Optional search term</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <returns>Paginated list of changes</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ChangeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ChangeDto>>> GetChanges(
        [FromQuery] string? searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var filter = new ChangeFilterDto
        {
            SearchTerm = searchTerm,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        var (items, totalCount) = await _changeService.GetChangesAsync(filter);
        return Ok(new PagedResult<ChangeDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        });
    }

    /// <summary>
    /// Submit a change request for approval.
    /// </summary>
    /// <param name="id">The change ID</param>
    /// <returns>Success status</returns>
    [HttpPatch("{id}/submit-approval")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> SubmitForApproval(int id)
    {
        await _changeService.SubmitForApprovalAsync(id, GetCurrentUserId());
        return Ok();
    }

    /// <summary>
    /// Approve a change request.
    /// </summary>
    /// <param name="id">The change ID</param>
    /// <param name="dto">Approval comments</param>
    /// <returns>Success status</returns>
    [HttpPost("{id}/approvals")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ApproveChange(int id, [FromBody] ApproveChangeDto dto)
    {
        await _changeService.ApproveChangeAsync(id, GetCurrentUserId(), dto.Comments);
        return Ok();
    }

    /// <summary>
    /// Reject a change request.
    /// </summary>
    /// <param name="id">The change ID</param>
    /// <param name="dto">Rejection reason</param>
    /// <returns>Success status</returns>
    [HttpPost("{id}/rejections")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> RejectChange(int id, [FromBody] ApproveChangeDto dto)
    {
        await _changeService.RejectChangeAsync(id, GetCurrentUserId(), dto.Comments);
        return Ok();
    }

    /// <summary>
    /// Schedule a change for implementation.
    /// </summary>
    /// <param name="id">The change ID</param>
    /// <param name="dto">Scheduled start and end times</param>
    /// <returns>Success status</returns>
    [HttpPatch("{id}/schedule")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ScheduleChange(int id, [FromBody] ScheduleChangeDto dto)
    {
        await _changeService.ScheduleChangeAsync(id, dto.ScheduledStart, dto.ScheduledEnd, GetCurrentUserId());
        return Ok();
    }

    /// <summary>
    /// Add an impacted Configuration Item to a change.
    /// </summary>
    /// <param name="changeId">The change ID</param>
    /// <param name="ciId">The CI ID to add</param>
    /// <returns>Success status</returns>
    [HttpPost("{changeId}/impacted-cis/{ciId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> AddImpactedCI(int changeId, int ciId)
    {
        var result = await _changeService.AddImpactedCIAsync(changeId, ciId, GetCurrentUserId());
        return result ? Ok() : BadRequest();
    }

    /// <summary>
    /// Get all CIs impacted by a change.
    /// </summary>
    /// <param name="id">The change ID</param>
    /// <returns>List of impacted CIs</returns>
    [HttpGet("{id}/impacted-cis")]
    [ProducesResponseType(typeof(IEnumerable<ConfigurationItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ConfigurationItemDto>>> GetImpactedCIs(int id)
    {
        var cis = await _changeService.GetImpactedCIsAsync(id);
        return Ok(cis);
    }

    /// <summary>
    /// Check for scheduling conflicts with other changes.
    /// </summary>
    /// <param name="id">The change ID</param>
    /// <returns>True if conflicts exist</returns>
    [HttpPost("{id}/check-conflicts")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> CheckConflicts(int id)
    {
        var conflicts = await _changeService.CheckConflictsAsync(id);
        return Ok(conflicts);
    }

    /// <summary>
    /// Get blackout periods during which changes are not allowed.
    /// </summary>
    /// <param name="startDate">Start of date range (default: now)</param>
    /// <param name="endDate">End of date range (default: 3 months from now)</param>
    /// <returns>List of blackout periods</returns>
    [HttpGet("blackouts")]
    [ProducesResponseType(typeof(IEnumerable<BlackoutPeriodDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BlackoutPeriodDto>>> GetBlackoutPeriods([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var blackouts = await _changeService.GetBlackoutPeriodsAsync(startDate ?? DateTime.UtcNow, endDate ?? DateTime.UtcNow.AddMonths(3));
        return Ok(blackouts);
    }

    /// <summary>
    /// Create a new blackout period.
    /// </summary>
    /// <param name="dto">Blackout period details</param>
    /// <returns>The created blackout period</returns>
    [HttpPost("blackouts")]
    [ProducesResponseType(typeof(BlackoutPeriodDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BlackoutPeriodDto>> CreateBlackoutPeriod([FromBody] CreateBlackoutPeriodDto dto)
    {
        var createDto = new CreateBlackoutPeriodInfo
        {
            Name = dto.Name,
            Reason = dto.Reason,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate
        };
        var blackout = await _changeService.CreateBlackoutPeriodAsync(createDto, GetCurrentUserId());
        return Ok(new BlackoutPeriodDto
        {
            BlackoutPeriodId = blackout.BlackoutPeriodId,
            Name = blackout.Name,
            Reason = blackout.Reason,
            StartDate = blackout.StartDate,
            EndDate = blackout.EndDate,
            IsActive = blackout.IsActive
        });
    }

    /// <summary>
    /// Get the change calendar showing scheduled changes and blackout periods.
    /// </summary>
    /// <param name="startDate">Start of date range (default: 7 days ago)</param>
    /// <param name="endDate">End of date range (default: 30 days from now)</param>
    /// <returns>Calendar data with changes and blackout periods</returns>
    [HttpGet("calendar")]
    [ProducesResponseType(typeof(ChangeCalendarDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ChangeCalendarDto>> GetChangeCalendar([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-7);
        var end = endDate ?? DateTime.UtcNow.AddDays(30);
        
        var filter = new ChangeFilterDto
        {
            PlannedStartFrom = start,
            PlannedStartTo = end,
            PageNumber = 1,
            PageSize = 100
        };
        
        var (changes, _) = await _changeService.GetChangesAsync(filter);
        var blackouts = await _changeService.GetBlackoutPeriodsAsync(start, end);
        
        return Ok(new ChangeCalendarDto
        {
            Changes = changes.ToList(),
            Blackouts = blackouts.Select(b => new BlackoutPeriodDto
            {
                BlackoutPeriodId = b.BlackoutPeriodId,
                Name = b.Name,
                Reason = b.Reason,
                StartDate = b.StartDate,
                EndDate = b.EndDate,
                IsActive = b.IsActive
            }).ToList(),
            StartDate = start,
            EndDate = end
        });
    }

    private int GetCurrentUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "1");
}

public class BlackoutPeriodDto
{
    public int BlackoutPeriodId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
}

public class CreateBlackoutPeriodDto
{
    public string Name { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class ChangeCalendarDto
{
    public List<ChangeDto> Changes { get; set; } = new();
    public List<BlackoutPeriodDto> Blackouts { get; set; } = new();
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class ApproveChangeDto { public string? Comments { get; set; } }
public class ScheduleChangeDto { public DateTime ScheduledStart { get; set; } public DateTime ScheduledEnd { get; set; } }
