// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos.ITSM;
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.ITSM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRM.Api.Infrastructure;
using ITSMSLADashboardDto = CRM.Core.Dtos.ITSM.SLADashboardDto;
namespace CRM.Api.Controllers.ITSM;

/// <summary>
/// API controller for managing SLA policies.
/// Provides CRUD operations plus policy assignment and applicable-policy queries.
/// </summary>
[ApiController]
[Route("api/slapolicies")]
[Produces("application/json")]
[Consumes("application/json")]
[Tags("ITSM - SLA Policies")]
public class SLAPoliciesController : CrmControllerBase
{
    private readonly ISLAPolicyAdminService _slaPolicyService;
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<SLAPoliciesController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SLAPoliciesController"/> class.
    /// </summary>
    public SLAPoliciesController(ISLAPolicyAdminService slaPolicyService, ICrmDbContext dbContext, ILogger<SLAPoliciesController> logger)
    {
        _slaPolicyService = slaPolicyService ?? throw new ArgumentNullException(nameof(slaPolicyService));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all SLA policies.
    /// </summary>
    /// <returns>A list of all SLA policies</returns>
    /// <response code="200">Returns the list of SLA policies</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(List<SLAPolicyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
    {
                var policies = await _slaPolicyService.GetAllAsync(cancellationToken);
        return Ok(policies);
    }

    /// <summary>
    /// Get SLA policies applicable to a given priority and/or category.
    /// </summary>
    /// <param name="priority">Optional priority filter (e.g. High, Critical)</param>
    /// <param name="category">Optional category filter (e.g. Network, Hardware)</param>
    /// <returns>Matching SLA policies</returns>
    /// <response code="200">Returns applicable SLA policies</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("applicable")]
    [Authorize]
    [ProducesResponseType(typeof(List<SLAPolicyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetApplicable([FromQuery] string? priority, [FromQuery] string? category, CancellationToken cancellationToken = default)
    {
                var policies = await _slaPolicyService.GetApplicablePoliciesAsync(priority, category, cancellationToken);
        return Ok(policies);
    }

    /// <summary>
    /// Get an SLA policy by ID.
    /// </summary>
    /// <param name="id">The policy ID</param>
    /// <returns>The SLA policy</returns>
    /// <response code="200">Returns the SLA policy</response>
    /// <response code="404">Policy not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(SLAPolicyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken = default)
    {
                var policy = await _slaPolicyService.GetByIdAsync(id, cancellationToken);
        if (policy == null)
            return NotFound(new { message = $"SLA policy with ID {id} not found" });
        return Ok(policy);
    }

    /// <summary>
    /// Create a new SLA policy.
    /// </summary>
    /// <param name="dto">The SLA policy creation data</param>
    /// <returns>The created SLA policy</returns>
    /// <response code="201">Policy created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(SLAPolicyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateSLAPolicyDto dto, CancellationToken cancellationToken = default)
    {
                if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var policy = await _slaPolicyService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = policy.Id }, policy);
    }

    /// <summary>
    /// Update an existing SLA policy.
    /// </summary>
    /// <param name="id">The policy ID</param>
    /// <param name="dto">The update data</param>
    /// <returns>The updated SLA policy</returns>
    /// <response code="200">Policy updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Policy not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(SLAPolicyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSLAPolicyDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var policy = await _slaPolicyService.UpdateAsync(id, dto, cancellationToken);
            return Ok(policy);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"SLA policy with ID {id} not found" });
        }
    }

    /// <summary>
    /// Delete an SLA policy.
    /// </summary>
    /// <param name="id">The policy ID</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Policy deleted successfully</response>
    /// <response code="404">Policy not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _slaPolicyService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"SLA policy with ID {id} not found" });
        }
    }

    /// <summary>
    /// Assign an SLA policy to a service request, creating an SLA tracking instance.
    /// </summary>
    /// <param name="policyId">The SLA policy ID</param>
    /// <param name="serviceRequestId">The service request ID</param>
    /// <returns>The created SLA instance</returns>
    /// <response code="200">SLA instance created successfully</response>
    /// <response code="404">Policy or service request not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("{policyId:int}/assign/{serviceRequestId:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(SLAInstanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AssignPolicy(int policyId, int serviceRequestId, CancellationToken cancellationToken = default)
    {
        try
        {
            var instance = await _slaPolicyService.AssignPolicyAsync(policyId, serviceRequestId, cancellationToken);
            return Ok(instance);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get SLA dashboard metrics with aggregate compliance data.
    /// Returns total tickets, breach counts, compliance rate, average times, and daily trends.
    /// </summary>
    /// <param name="startDate">Optional start date filter (defaults to 30 days ago)</param>
    /// <param name="endDate">Optional end date filter (defaults to today)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>SLA dashboard metrics</returns>
    /// <response code="200">Returns the SLA dashboard data</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("dashboard")]
    [Authorize]
    [ProducesResponseType(typeof(ITSMSLADashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSLADashboard(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken ct)
    {
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        _logger.LogInformation("Fetching SLA dashboard data from {Start} to {End}", start, end);

        // Query service requests within the date range
        var requests = await _dbContext.ServiceRequests
            .AsNoTracking()
            .Where(sr => !sr.IsDeleted && sr.CreatedAt >= start && sr.CreatedAt <= end)
            .Select(sr => new
            {
                sr.Id,
                sr.Priority,
                sr.Status,
                sr.CreatedAt,
                sr.ResponseDueDate,
                sr.ResolutionDueDate,
                sr.FirstResponseDate,
                sr.ResolvedDate,
                sr.ResponseSlaBreached,
                sr.ResolutionSlaBreached
            })
            .ToListAsync(ct);

        var totalTickets = requests.Count;
        var breachedCount = requests.Count(r => r.ResponseSlaBreached || r.ResolutionSlaBreached);
        var withinSLA = totalTickets - breachedCount;
        var complianceRate = totalTickets > 0 ? (double)withinSLA / totalTickets * 100.0 : 100.0;

        // Average response time (minutes) for tickets that have a first response
        var respondedTickets = requests
            .Where(r => r.FirstResponseDate.HasValue)
            .ToList();
        var avgResponseTime = respondedTickets.Count > 0
            ? respondedTickets.Average(r => (r.FirstResponseDate!.Value - r.CreatedAt).TotalMinutes)
            : 0.0;

        // Average resolution time (minutes) for resolved tickets
        var resolvedTickets = requests
            .Where(r => r.ResolvedDate.HasValue)
            .ToList();
        var avgResolutionTime = resolvedTickets.Count > 0
            ? resolvedTickets.Average(r => (r.ResolvedDate!.Value - r.CreatedAt).TotalMinutes)
            : 0.0;

        // Breaches by priority
        var breachesByPriority = requests
            .Where(r => r.ResponseSlaBreached || r.ResolutionSlaBreached)
            .GroupBy(r => r.Priority.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        // Daily trend
        var dailyTrend = requests
            .GroupBy(r => r.CreatedAt.Date)
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var dayTotal = g.Count();
                var dayBreached = g.Count(r => r.ResponseSlaBreached || r.ResolutionSlaBreached);
                var dayWithin = dayTotal - dayBreached;
                return new SLATrendPoint
                {
                    Date = g.Key,
                    ComplianceRate = dayTotal > 0 ? (double)dayWithin / dayTotal * 100.0 : 100.0,
                    TotalTickets = dayTotal
                };
            })
            .ToList();

        var dashboard = new ITSMSLADashboardDto
        {
            TotalTickets = totalTickets,
            WithinSLA = withinSLA,
            BreachedSLA = breachedCount,
            ComplianceRate = Math.Round(complianceRate, 2),
            AvgResponseTimeMinutes = Math.Round(avgResponseTime, 2),
            AvgResolutionTimeMinutes = Math.Round(avgResolutionTime, 2),
            BreachesByPriority = breachesByPriority,
            DailyTrend = dailyTrend
        };

        return Ok(dashboard);
    }
}
