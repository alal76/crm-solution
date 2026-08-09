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
public class ProblemsController : CrmControllerBase
{
    private readonly IProblemService _problemService;
    private readonly ILogger<ProblemsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProblemsController"/> class.
    /// </summary>
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
        try
        {
            var problem = await _problemService.CreateProblemAsync(dto, GetCurrentUserId());
            return CreatedAtAction(nameof(GetProblem), new { id = problem.ProblemId }, problem);
        }
        catch (CRM.Core.Exceptions.ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error creating problem");
            return BadRequest(new { message = ex.Message });
        }
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

    private int GetCurrentUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "1"); // NOSONAR
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
public class CMDBController : CrmControllerBase
{
    private readonly ICMDBService _cmdbService;
    private readonly IAssetLifecycleService _assetLifecycleService;
    private readonly IDiscoveryService _discoveryService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CMDBController"/> class.
    /// </summary>
    public CMDBController(
        ICMDBService cmdbService,
        IAssetLifecycleService assetLifecycleService,
        IDiscoveryService discoveryService)
    {
        _cmdbService = cmdbService;
        _assetLifecycleService = assetLifecycleService;
        _discoveryService = discoveryService;
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
        await Task.CompletedTask;
        // Return the available CI types
        var types = Enum.GetNames(typeof(CIType));
        return Ok(types);
    }

    // ────────────────────────────────────────────────────────────────
    // Asset Lifecycle Management
    // ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Get the current lifecycle state of a Configuration Item.
    /// </summary>
    /// <param name="id">The CI ID</param>
    /// <returns>The current lifecycle state</returns>
    [HttpGet("{id:int}/lifecycle")]
    [HttpGet("cis/{id:int}/lifecycle")]
    [ProducesResponseType(typeof(AssetLifecycleState), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssetLifecycleState>> GetLifecycleState(int id)
    {
        try
        {
            var state = await _assetLifecycleService.GetLifecycleStateAsync(id);
            return Ok(state);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Transition a Configuration Item to a new lifecycle stage.
    /// </summary>
    /// <param name="id">The CI ID</param>
    /// <param name="request">Target stage and optional notes</param>
    /// <returns>The recorded lifecycle transition</returns>
    [HttpPost("{id:int}/lifecycle/transition")]
    [HttpPost("cis/{id:int}/lifecycle/transition")]
    [ProducesResponseType(typeof(AssetLifecycleTransition), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssetLifecycleTransition>> TransitionLifecycle(int id, [FromBody] TransitionLifecycleRequest request)
    {
        try
        {
            var transition = await _assetLifecycleService.TransitionAsync(id, request.TargetStage, GetCurrentUserId(), request.Notes);
            return Ok(transition);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get lifecycle transition history for a Configuration Item.
    /// </summary>
    /// <param name="id">The CI ID</param>
    /// <returns>List of lifecycle transitions, most recent first</returns>
    [HttpGet("{id:int}/lifecycle/history")]
    [HttpGet("cis/{id:int}/lifecycle/history")]
    [ProducesResponseType(typeof(IEnumerable<AssetLifecycleTransition>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AssetLifecycleTransition>>> GetLifecycleHistory(int id)
    {
        var history = await _assetLifecycleService.GetLifecycleHistoryAsync(id);
        return Ok(history);
    }

    /// <summary>
    /// Schedule the retirement of a Configuration Item.
    /// </summary>
    /// <param name="id">The CI ID</param>
    /// <param name="request">Retirement date and reason</param>
    /// <returns>The created retirement schedule</returns>
    [HttpPost("{id:int}/lifecycle/retire")]
    [HttpPost("cis/{id:int}/lifecycle/retire")]
    [ProducesResponseType(typeof(AssetRetirementSchedule), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssetRetirementSchedule>> ScheduleRetirement(int id, [FromBody] ScheduleRetirementRequest request)
    {
        try
        {
            var schedule = await _assetLifecycleService.ScheduleRetirementAsync(id, request.RetirementDate, GetCurrentUserId(), request.Reason);
            return Ok(schedule);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get utilization metrics for a Configuration Item.
    /// </summary>
    /// <param name="id">The CI ID</param>
    /// <returns>Utilization metrics including MTBF/MTTR and incident counts</returns>
    [HttpGet("{id:int}/lifecycle/utilization")]
    [HttpGet("cis/{id:int}/lifecycle/utilization")]
    [ProducesResponseType(typeof(AssetUtilizationMetrics), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssetUtilizationMetrics>> GetUtilizationMetrics(int id)
    {
        try
        {
            var metrics = await _assetLifecycleService.GetUtilizationMetricsAsync(id);
            return Ok(metrics);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get lifecycle cost analysis for a Configuration Item.
    /// </summary>
    /// <param name="id">The CI ID</param>
    /// <returns>Cost analysis including total cost of ownership</returns>
    [HttpGet("{id:int}/lifecycle/cost-analysis")]
    [HttpGet("cis/{id:int}/lifecycle/cost-analysis")]
    [ProducesResponseType(typeof(LifecycleCostAnalysis), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LifecycleCostAnalysis>> GetCostAnalysis(int id)
    {
        try
        {
            var analysis = await _assetLifecycleService.GetCostAnalysisAsync(id);
            return Ok(analysis);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get assets approaching end-of-life, end-of-support, or warranty expiration.
    /// </summary>
    /// <param name="daysAhead">Look-ahead window in days (default: 90)</param>
    /// <returns>List of end-of-life alerts</returns>
    [HttpGet("lifecycle/end-of-life-alerts")]
    [ProducesResponseType(typeof(IEnumerable<AssetEndOfLifeAlert>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AssetEndOfLifeAlert>>> GetEndOfLifeAlerts([FromQuery] int daysAhead = 90)
    {
        var alerts = await _assetLifecycleService.GetEndOfLifeAlertsAsync(daysAhead);
        return Ok(alerts);
    }

    /// <summary>
    /// Get assets that are candidates for refresh/replacement.
    /// </summary>
    /// <returns>List of refresh candidates ordered by urgency</returns>
    [HttpGet("lifecycle/refresh-candidates")]
    [ProducesResponseType(typeof(IEnumerable<AssetRefreshCandidate>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AssetRefreshCandidate>>> GetRefreshCandidates()
    {
        var candidates = await _assetLifecycleService.GetRefreshCandidatesAsync();
        return Ok(candidates);
    }

    // ────────────────────────────────────────────────────────────────
    // CMDB Auto-Discovery
    // ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Run a discovery scan for a network range, cloud provider, or domain.
    /// </summary>
    /// <param name="request">Scan configuration</param>
    /// <returns>The discovery scan result</returns>
    [HttpPost("discovery/scan")]
    [ProducesResponseType(typeof(DiscoveryScanResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DiscoveryScanResult>> RunDiscoveryScan([FromBody] DiscoveryScanRequest request)
    {
        var result = await _discoveryService.RunDiscoveryScanAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Get the status of a discovery scan.
    /// </summary>
    /// <param name="scanId">The scan ID</param>
    /// <returns>The scan status</returns>
    [HttpGet("discovery/scan/{scanId:int}/status")]
    [ProducesResponseType(typeof(DiscoveryScanStatus), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DiscoveryScanStatus>> GetScanStatus(int scanId)
    {
        try
        {
            var status = await _discoveryService.GetScanStatusAsync(scanId);
            return Ok(status);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get discovered assets pending approval for CMDB import.
    /// </summary>
    /// <returns>List of pending discovered assets</returns>
    [HttpGet("discovery/pending-assets")]
    [ProducesResponseType(typeof(IEnumerable<DiscoveredAsset>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DiscoveredAsset>>> GetPendingDiscoveredAssets()
    {
        var assets = await _discoveryService.GetPendingAssetsAsync();
        return Ok(assets);
    }

    /// <summary>
    /// Approve and import discovered assets into the CMDB.
    /// </summary>
    /// <param name="request">The discovered asset IDs to import</param>
    /// <returns>The import result</returns>
    [HttpPost("discovery/import")]
    [ProducesResponseType(typeof(CmdbImportResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<CmdbImportResult>> ImportDiscoveredAssets([FromBody] ImportDiscoveredAssetsRequest request)
    {
        var result = await _discoveryService.ImportAssetsAsync(request.AssetIds, GetCurrentUserId());
        return Ok(result);
    }

    /// <summary>
    /// Reconcile discovered assets from a scan against existing CMDB entries.
    /// </summary>
    /// <param name="scanId">The scan ID</param>
    /// <returns>The reconciliation result</returns>
    [HttpPost("discovery/scan/{scanId:int}/reconcile")]
    [ProducesResponseType(typeof(ReconciliationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReconciliationResult>> ReconcileDiscoveredAssets(int scanId)
    {
        try
        {
            var result = await _discoveryService.ReconcileAssetsAsync(scanId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get configured discovery schedules.
    /// </summary>
    /// <returns>List of discovery schedules</returns>
    [HttpGet("discovery/schedules")]
    [ProducesResponseType(typeof(IEnumerable<DiscoverySchedule>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DiscoverySchedule>>> GetDiscoverySchedules()
    {
        var schedules = await _discoveryService.GetSchedulesAsync();
        return Ok(schedules);
    }

    /// <summary>
    /// Create or update a discovery schedule.
    /// </summary>
    /// <param name="request">The schedule details</param>
    /// <returns>The saved discovery schedule</returns>
    [HttpPost("discovery/schedules")]
    [ProducesResponseType(typeof(DiscoverySchedule), StatusCodes.Status200OK)]
    public async Task<ActionResult<DiscoverySchedule>> SaveDiscoverySchedule([FromBody] SaveDiscoveryScheduleRequest request)
    {
        var schedule = new DiscoverySchedule
        {
            ScheduleId = request.ScheduleId,
            Name = request.Name,
            Type = request.Type,
            Target = request.Target,
            CronExpression = request.CronExpression,
            IsActive = request.IsActive,
            AutoImportMatches = request.AutoImportMatches
        };

        var saved = await _discoveryService.SaveScheduleAsync(schedule);
        return Ok(saved);
    }

    private int GetCurrentUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "1"); // NOSONAR
}

/// <summary>
/// Request to transition a Configuration Item's lifecycle stage.
/// </summary>
public class TransitionLifecycleRequest
{
    public LifecycleStage TargetStage { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Request to schedule a Configuration Item's retirement.
/// </summary>
public class ScheduleRetirementRequest
{
    public DateTime RetirementDate { get; set; }
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Request to import a set of discovered assets into the CMDB.
/// </summary>
public class ImportDiscoveredAssetsRequest
{
    public List<int> AssetIds { get; set; } = new();
}

/// <summary>
/// Client-settable fields for creating/updating a discovery schedule.
/// LastRun/NextRun are server-computed and intentionally excluded.
/// </summary>
public class SaveDiscoveryScheduleRequest
{
    public int ScheduleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DiscoveryType Type { get; set; }
    public string Target { get; set; } = string.Empty;
    public string CronExpression { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool AutoImportMatches { get; set; }
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
