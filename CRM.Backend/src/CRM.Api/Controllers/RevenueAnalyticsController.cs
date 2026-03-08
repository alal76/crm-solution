// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// Revenue Analytics API — ARR/MRR metrics, trend, movements, and snapshot management.
/// </summary>
[ApiController]
[Route("api/revenue")]
[Authorize]
public class RevenueAnalyticsController : CrmControllerBase
{
    private readonly IRevenueAnalyticsService _revenueService;

    public RevenueAnalyticsController(
        IRevenueAnalyticsService revenueService)
    {
        _revenueService = revenueService;
    }

    /// <summary>Returns aggregated revenue metrics (MRR, ARR, growth, churn, NRR).</summary>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(RevenueMetricsDto), 200)]
    public async Task<IActionResult> GetMetrics(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct)
    {
        var metrics = await _revenueService.GetMetricsAsync(from, to, ct);
        return Ok(metrics);
    }

    /// <summary>Returns the last N monthly snapshots as a trend series.</summary>
    [HttpGet("trend")]
    [ProducesResponseType(typeof(IEnumerable<RevenueSnapshotDto>), 200)]
    public async Task<IActionResult> GetTrend([FromQuery] int months = 12, CancellationToken ct = default)
    {
        var trend = await _revenueService.GetTrendAsync(months, ct);
        return Ok(trend);
    }

    /// <summary>Returns MRR waterfall movements for the last N months.</summary>
    [HttpGet("movements")]
    [ProducesResponseType(typeof(IEnumerable<RevenueMRRMovementDto>), 200)]
    public async Task<IActionResult> GetMRRMovements([FromQuery] int months = 12, CancellationToken ct = default)
    {
        var movements = await _revenueService.GetMRRMovementsAsync(months, ct);
        return Ok(movements);
    }

    /// <summary>Returns the current MRR value.</summary>
    [HttpGet("mrr")]
    [ProducesResponseType(typeof(decimal), 200)]
    public async Task<IActionResult> GetCurrentMRR(CancellationToken ct)
    {
        var mrr = await _revenueService.GetCurrentMRRAsync(ct);
        return Ok(new { mrr });
    }

    /// <summary>Returns the current ARR value (MRR * 12).</summary>
    [HttpGet("arr")]
    [ProducesResponseType(typeof(decimal), 200)]
    public async Task<IActionResult> GetCurrentARR(CancellationToken ct)
    {
        var arr = await _revenueService.GetCurrentARRAsync(ct);
        return Ok(new { arr });
    }

    /// <summary>Returns the churn rate as a percentage for the specified date range.</summary>
    [HttpGet("churn-rate")]
    [ProducesResponseType(typeof(decimal), 200)]
    public async Task<IActionResult> GetChurnRate(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct)
    {
        var churnRate = await _revenueService.GetChurnRateAsync(from, to, ct);
        return Ok(new { churnRate });
    }

    /// <summary>Creates a manual revenue snapshot. Admin only.</summary>
    [HttpPost("snapshots")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(RevenueSnapshotDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateSnapshot([FromBody] CreateRevenueSnapshotDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var snapshot = await _revenueService.CreateSnapshotAsync(dto, ct);
        return CreatedAtAction(nameof(GetMetrics), new { }, snapshot);
    }

    /// <summary>Calculates and persists a snapshot from live subscription data. Admin only.</summary>
    [HttpPost("snapshots/calculate")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(RevenueSnapshotDto), 201)]
    public async Task<IActionResult> CalculateCurrentSnapshot(CancellationToken ct)
    {
        var snapshot = await _revenueService.CalculateCurrentSnapshotAsync(ct);
        return CreatedAtAction(nameof(GetMetrics), new { }, snapshot);
    }

    /// <summary>Returns contraction MRR for the specified period.</summary>
    [HttpGet("contraction")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<IActionResult> GetContraction(
        [FromQuery] int months = 6,
        CancellationToken ct = default)
    {
        var movements = await _revenueService.GetMRRMovementsAsync(months, ct);
        var contraction = movements.Sum(m => m.ContractionMRR);
        return Ok(new { contraction, months });
    }

    /// <summary>Returns expansion MRR for the specified period.</summary>
    [HttpGet("expansion")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<IActionResult> GetExpansion(
        [FromQuery] int months = 6,
        CancellationToken ct = default)
    {
        var movements = await _revenueService.GetMRRMovementsAsync(months, ct);
        var expansion = movements.Sum(m => m.ExpansionMRR);
        return Ok(new { expansion, months });
    }

    /// <summary>Returns new MRR for the specified period.</summary>
    [HttpGet("new")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<IActionResult> GetNewRevenue(
        [FromQuery] int months = 6,
        CancellationToken ct = default)
    {
        var movements = await _revenueService.GetMRRMovementsAsync(months, ct);
        var newRevenue = movements.Sum(m => m.NewMRR);
        return Ok(new { newRevenue, months });
    }

    /// <summary>Returns reactivation MRR for the specified period.</summary>
    [HttpGet("reactivation")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<IActionResult> GetReactivation(
        [FromQuery] int months = 6,
        CancellationToken ct = default)
    {
        var movements = await _revenueService.GetMRRMovementsAsync(months, ct);
        var reactivation = movements.Sum(m => m.NewMRR) - movements.Sum(m => m.ChurnMRR);
        return Ok(new { reactivation, months });
    }
}
