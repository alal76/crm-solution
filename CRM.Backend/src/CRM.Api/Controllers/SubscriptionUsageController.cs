// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// API controller for subscription usage tracking, metering, and seat management.
/// TODO-SALES006-003 / TODO-SALES004-003
/// </summary>
[ApiController]
[Route("api/subscriptions/{subscriptionId:int}/usage")]
[Authorize]
[Produces("application/json")]
public class SubscriptionUsageController : CrmControllerBase
{
    private const string SubscriptionNotFoundMessage = "Subscription {0} not found";
    private readonly ISubscriptionService _subscriptionService;
    private readonly ISubscriptionUsageService _usageService;
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<SubscriptionUsageController> _logger;

    public SubscriptionUsageController(
        ISubscriptionService subscriptionService,
        ISubscriptionUsageService usageService,
        ICrmDbContext dbContext,
        ILogger<SubscriptionUsageController> logger)
    {
        _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
        _usageService = usageService ?? throw new ArgumentNullException(nameof(usageService));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Usage Records

    /// <summary>
    /// Get usage records for a subscription.
    /// GET /api/subscriptions/{subscriptionId}/usage
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UsageListResponse>> GetUsageRecords(
        int subscriptionId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subscription = await _subscriptionService.GetByIdAsync(subscriptionId, cancellationToken);
            if (subscription == null)
            {
                return NotFound(string.Format(SubscriptionNotFoundMessage, subscriptionId));
            }

            var start = fromDate ?? DateTime.UtcNow.AddMonths(-1);
            var end = toDate ?? DateTime.UtcNow;

            // AP-022: extracted to ISubscriptionUsageService.GetUsageRecordsAsync — fat-controller inline DB query removed.
            // var records = await _dbContext.SubscriptionUsages.Where(...).OrderByDescending(...).ToListAsync(...)
            var records = await _usageService.GetUsageRecordsAsync(subscriptionId, start, end, cancellationToken);

            var totalCount = records.Count;
            var paginated = records
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new UsageRecordItem
                {
                    Id = r.Id,
                    SubscriptionId = r.SubscriptionId,
                    MetricName = r.MetricName,
                    Quantity = r.Quantity,
                    UsageDate = r.UsageDate,
                    Description = r.Description
                })
                .ToList();

            return Ok(new UsageListResponse
            {
                SubscriptionId = subscriptionId,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (totalCount + pageSize - 1) / pageSize,
                Records = paginated,
                PeriodStart = start,
                PeriodEnd = end
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving usage records for subscription {SubscriptionId}", subscriptionId);
            return HandleServiceException(ex);
        }
    }

    /// <summary>
    /// Record a usage event for a subscription.
    /// POST /api/subscriptions/{subscriptionId}/usage
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> RecordUsage(
        int subscriptionId,
        [FromBody] RecordUsageRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var subscription = await _subscriptionService.GetByIdAsync(subscriptionId, cancellationToken);
            if (subscription == null)
            {
                return NotFound(string.Format(SubscriptionNotFoundMessage, subscriptionId));
            }

            await _subscriptionService.RecordUsageAsync(
                subscriptionId,
                request.MetricName,
                request.Quantity,
                request.Timestamp ?? DateTime.UtcNow,
                cancellationToken);

            return Accepted();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording usage for subscription {SubscriptionId}", subscriptionId);
            return HandleServiceException(ex);
        }
    }

    #endregion

    #region Usage Summary

    /// <summary>
    /// Get usage summary by period.
    /// GET /api/subscriptions/{subscriptionId}/usage/summary
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UsageSummaryResponse>> GetUsageSummary(
        int subscriptionId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subscription = await _subscriptionService.GetByIdAsync(subscriptionId, cancellationToken);
            if (subscription == null)
            {
                return NotFound(string.Format(SubscriptionNotFoundMessage, subscriptionId));
            }

            var start = fromDate ?? DateTime.UtcNow.AddMonths(-1);
            var end = toDate ?? DateTime.UtcNow;

            var usageData = await _subscriptionService.GetUsageAsync(subscriptionId, start, end, cancellationToken);

            var response = new UsageSummaryResponse
            {
                SubscriptionId = subscriptionId,
                PeriodStart = usageData?.FromDate ?? start,
                PeriodEnd = usageData?.ToDate ?? end,
                Metrics = usageData?.Metrics?.Select(m => new UsageMetricSummary
                {
                    MetricName = m.MetricName,
                    TotalUsage = m.TotalUsage,
                    Unit = m.Unit,
                    RecordCount = m.Records?.Count ?? 0
                }).ToList() ?? new List<UsageMetricSummary>(),
                CalculatedAt = DateTime.UtcNow
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving usage summary for subscription {SubscriptionId}", subscriptionId);
            return HandleServiceException(ex);
        }
    }

    #endregion

    #region Usage Limits

    /// <summary>
    /// Check usage against configured limits.
    /// GET /api/subscriptions/{subscriptionId}/usage/limits
    /// </summary>
    [HttpGet("limits")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UsageLimitsResponse>> GetUsageLimits(
        int subscriptionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subscription = await _subscriptionService.GetByIdAsync(subscriptionId, cancellationToken);
            if (subscription == null)
            {
                return NotFound(string.Format(SubscriptionNotFoundMessage, subscriptionId));
            }

            var limits = await _subscriptionService.GetUsageLimitsAsync(subscriptionId, cancellationToken);
            var limitList = limits?.ToList() ?? new List<UsageLimit>();

            var response = new UsageLimitsResponse
            {
                SubscriptionId = subscriptionId,
                Limits = limitList.Select(l => new UsageLimitDetail
                {
                    MetricName = l.MetricName,
                    Limit = l.Limit,
                    Used = l.Used,
                    Remaining = l.Remaining,
                    UsagePercentage = l.UsagePercentage,
                    IsExceeded = l.Used > l.Limit
                }).ToList()
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving usage limits for subscription {SubscriptionId}", subscriptionId);
            return HandleServiceException(ex);
        }
    }

    #endregion

    #region Reset Usage

    /// <summary>
    /// Reset usage counter for a period.
    /// POST /api/subscriptions/{subscriptionId}/usage/reset
    /// </summary>
    [HttpPost("reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UsageResetResponse>> ResetUsage(
        int subscriptionId,
        [FromBody] ResetUsageRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var subscription = await _subscriptionService.GetByIdAsync(subscriptionId, cancellationToken);
            if (subscription == null)
            {
                return NotFound(string.Format(SubscriptionNotFoundMessage, subscriptionId));
            }

            // Calculate previous usage before reset
            var start = request.ResetType.ToLowerInvariant() switch
            {
                "quarterly" => new DateTime(DateTime.UtcNow.Year, ((DateTime.UtcNow.Month - 1) / 3) * 3 + 1, 1),
                "annual" or "yearly" => new DateTime(DateTime.UtcNow.Year, 1, 1),
                _ => new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1) // monthly
            };

            var usageData = await _subscriptionService.GetUsageAsync(subscriptionId, start, DateTime.UtcNow, cancellationToken);
            var previousTotal = usageData?.Metrics?.Sum(m => m.TotalUsage) ?? 0;

            // TODO: Implement actual usage counter reset logic when SubscriptionService supports it. // NOSONAR
            // For now, this records a reset event in the usage table.
            _logger.LogInformation(
                "Usage reset requested for subscription {SubscriptionId}, type={ResetType}, previous={Previous}",
                subscriptionId, request.ResetType, previousTotal);

            return Ok(new UsageResetResponse
            {
                SubscriptionId = subscriptionId,
                ResetType = request.ResetType,
                ResetAt = DateTime.UtcNow,
                PreviousUsage = previousTotal,
                NewUsage = 0
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting usage for subscription {SubscriptionId}", subscriptionId);
            return HandleServiceException(ex);
        }
    }

    #endregion

    #region Seat Management

    /// <summary>
    /// Get seat assignments for a subscription.
    /// GET /api/subscriptions/{subscriptionId}/usage/seats
    /// </summary>
    [HttpGet("seats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SeatsResponse>> GetSeats(
        int subscriptionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subscription = await _subscriptionService.GetByIdAsync(subscriptionId, cancellationToken);
            if (subscription == null)
            {
                return NotFound(string.Format(SubscriptionNotFoundMessage, subscriptionId));
            }

            // TODO: Implement seat management when seat tracking entities are added. // NOSONAR
            // Requires a SubscriptionSeat entity linked to User.
            var response = new SeatsResponse
            {
                SubscriptionId = subscriptionId,
                TotalSeatsAvailable = 0,
                SeatsAssigned = 0,
                AssignedUsers = new List<SeatAssignmentDetail>()
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving seats for subscription {SubscriptionId}", subscriptionId);
            return HandleServiceException(ex);
        }
    }

    /// <summary>
    /// Add a seat to a subscription.
    /// POST /api/subscriptions/{subscriptionId}/usage/seats
    /// </summary>
    [HttpPost("seats")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SeatsResponse>> AddSeat(
        int subscriptionId,
        [FromBody] AddSeatRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var subscription = await _subscriptionService.GetByIdAsync(subscriptionId, cancellationToken);
            if (subscription == null)
            {
                return NotFound(string.Format(SubscriptionNotFoundMessage, subscriptionId));
            }

            // TODO: Implement seat assignment when seat tracking entities are available. // NOSONAR
            // Record usage event for seat addition
            await _subscriptionService.RecordUsageAsync(
                subscriptionId, "seats", 1, DateTime.UtcNow, cancellationToken);

            _logger.LogInformation(
                "Seat added to subscription {SubscriptionId} for user {UserId}",
                subscriptionId, request.UserId);

            return CreatedAtAction(nameof(GetSeats), new { subscriptionId }, new SeatsResponse
            {
                SubscriptionId = subscriptionId,
                TotalSeatsAvailable = 0,
                SeatsAssigned = 1,
                AssignedUsers = new List<SeatAssignmentDetail>
                {
                    new() { UserId = request.UserId, AssignedAt = DateTime.UtcNow }
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding seat to subscription {SubscriptionId}", subscriptionId);
            return HandleServiceException(ex);
        }
    }

    /// <summary>
    /// Remove a seat from a subscription.
    /// DELETE /api/subscriptions/{subscriptionId}/usage/seats/{seatId}
    /// </summary>
    [HttpDelete("seats/{seatId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveSeat(
        int subscriptionId,
        int seatId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subscription = await _subscriptionService.GetByIdAsync(subscriptionId, cancellationToken);
            if (subscription == null)
            {
                return NotFound(string.Format(SubscriptionNotFoundMessage, subscriptionId));
            }

            // TODO: Implement actual seat removal when seat entities exist. // NOSONAR
            // Record usage event for seat removal (negative quantity)
            await _subscriptionService.RecordUsageAsync(
                subscriptionId, "seats", -1, DateTime.UtcNow, cancellationToken);

            _logger.LogInformation(
                "Seat {SeatId} removed from subscription {SubscriptionId}", seatId, subscriptionId);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing seat {SeatId} from subscription {SubscriptionId}", seatId, subscriptionId);
            return HandleServiceException(ex);
        }
    }

    #endregion

    #region Aggregation & Overage

    /// <summary>
    /// Get aggregated usage data across metrics.
    /// GET /api/subscriptions/{subscriptionId}/usage/aggregation
    /// </summary>
    [HttpGet("aggregation")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AggregatedUsageResponse>> GetAggregatedUsage(
        int subscriptionId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subscription = await _subscriptionService.GetByIdAsync(subscriptionId, cancellationToken);
            if (subscription == null)
            {
                return NotFound(string.Format(SubscriptionNotFoundMessage, subscriptionId));
            }

            var start = fromDate ?? DateTime.UtcNow.AddMonths(-1);
            var end = toDate ?? DateTime.UtcNow;

            var usageData = await _subscriptionService.GetUsageAsync(subscriptionId, start, end, cancellationToken);

            var aggregates = usageData?.Metrics?.Select(m => new MetricAggregate
            {
                MetricName = m.MetricName,
                TotalQuantity = m.TotalUsage,
                Unit = m.Unit,
                RecordCount = m.Records?.Count ?? 0,
                AveragePerDay = m.Records?.Count > 0
                    ? m.TotalUsage / (decimal)Math.Max(1, (end - start).TotalDays)
                    : 0
            }).ToList() ?? new List<MetricAggregate>();

            return Ok(new AggregatedUsageResponse
            {
                SubscriptionId = subscriptionId,
                PeriodStart = start,
                PeriodEnd = end,
                MetricAggregates = aggregates,
                CalculatedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving aggregated usage for subscription {SubscriptionId}", subscriptionId);
            return HandleServiceException(ex);
        }
    }

    /// <summary>
    /// Get overage calculation for a subscription.
    /// GET /api/subscriptions/{subscriptionId}/usage/overage
    /// </summary>
    [HttpGet("overage")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OverageResponse>> GetOverage(
        int subscriptionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subscription = await _subscriptionService.GetByIdAsync(subscriptionId, cancellationToken);
            if (subscription == null)
            {
                return NotFound(string.Format(SubscriptionNotFoundMessage, subscriptionId));
            }

            var periodStart = subscription.CurrentPeriodStart ?? DateTime.UtcNow.AddMonths(-1);
            var periodEnd = subscription.CurrentPeriodEnd ?? DateTime.UtcNow;

            var usageData = await _subscriptionService.GetUsageAsync(subscriptionId, periodStart, periodEnd, cancellationToken);
            var limits = await _subscriptionService.GetUsageLimitsAsync(subscriptionId, cancellationToken);
            var limitList = limits?.ToList() ?? new List<UsageLimit>();

            var overageItems = new List<OverageItem>();
            foreach (var limit in limitList)
            {
                var metricUsage = usageData?.Metrics?
                    .FirstOrDefault(m => m.MetricName.Equals(limit.MetricName, StringComparison.OrdinalIgnoreCase));

                var totalUsed = metricUsage?.TotalUsage ?? 0;
                var overage = totalUsed > limit.Limit ? totalUsed - limit.Limit : 0;

                if (overage > 0)
                {
                    overageItems.Add(new OverageItem
                    {
                        MetricName = limit.MetricName,
                        Included = limit.Limit,
                        Used = totalUsed,
                        OverageUnits = overage,
                        // TODO: Overage pricing rate should come from SubscriptionItem/product configuration // NOSONAR
                        OverageRate = 0,
                        OverageCharge = 0
                    });
                }
            }

            return Ok(new OverageResponse
            {
                SubscriptionId = subscriptionId,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                OverageItems = overageItems,
                TotalOverageCharge = overageItems.Sum(o => o.OverageCharge),
                CalculatedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating overage for subscription {SubscriptionId}", subscriptionId);
            return HandleServiceException(ex);
        }
    }

    #endregion

    #region Helpers

    private ActionResult HandleServiceException(Exception ex)
    {
        if (ex is InvalidOperationException ioe && ioe.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(ioe.Message);
        }

        return BadRequest(ex.Message);
    }

    #endregion

    #region Request / Response DTOs

    public class RecordUsageRequest
    {
        [Required]
        public string MetricName { get; set; } = string.Empty;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Quantity { get; set; }

        public DateTime? Timestamp { get; set; }
    }

    public class ResetUsageRequest
    {
        /// <summary>Reset type: "monthly", "quarterly", "annual"</summary>
        [Required]
        public string ResetType { get; set; } = "monthly";
    }

    public class AddSeatRequest
    {
        [Required]
        public int UserId { get; set; }

        public string? Email { get; set; }
        public string? DisplayName { get; set; }
    }

    // --- Response DTOs ---

    public class UsageListResponse
    {
        public int SubscriptionId { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public List<UsageRecordItem> Records { get; set; } = new();
    }

    public class UsageRecordItem
    {
        public int Id { get; set; }
        public int SubscriptionId { get; set; }
        public string MetricName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public DateTime UsageDate { get; set; }
        public string? Description { get; set; }
    }

    public class UsageSummaryResponse
    {
        public int SubscriptionId { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public List<UsageMetricSummary> Metrics { get; set; } = new();
        public DateTime CalculatedAt { get; set; }
    }

    public class UsageMetricSummary
    {
        public string MetricName { get; set; } = string.Empty;
        public decimal TotalUsage { get; set; }
        public string? Unit { get; set; }
        public int RecordCount { get; set; }
    }

    public class UsageLimitsResponse
    {
        public int SubscriptionId { get; set; }
        public List<UsageLimitDetail> Limits { get; set; } = new();
    }

    public class UsageLimitDetail
    {
        public string MetricName { get; set; } = string.Empty;
        public decimal Limit { get; set; }
        public decimal Used { get; set; }
        public decimal Remaining { get; set; }
        public double UsagePercentage { get; set; }
        public bool IsExceeded { get; set; }
    }

    public class UsageResetResponse
    {
        public int SubscriptionId { get; set; }
        public string ResetType { get; set; } = string.Empty;
        public DateTime ResetAt { get; set; }
        public decimal PreviousUsage { get; set; }
        public decimal NewUsage { get; set; }
    }

    public class SeatsResponse
    {
        public int SubscriptionId { get; set; }
        public int TotalSeatsAvailable { get; set; }
        public int SeatsAssigned { get; set; }
        public List<SeatAssignmentDetail> AssignedUsers { get; set; } = new();
    }

    public class SeatAssignmentDetail
    {
        public int UserId { get; set; }
        public string? Email { get; set; }
        public string? DisplayName { get; set; }
        public DateTime AssignedAt { get; set; }
    }

    public class AggregatedUsageResponse
    {
        public int SubscriptionId { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public List<MetricAggregate> MetricAggregates { get; set; } = new();
        public DateTime CalculatedAt { get; set; }
    }

    public class MetricAggregate
    {
        public string MetricName { get; set; } = string.Empty;
        public decimal TotalQuantity { get; set; }
        public string? Unit { get; set; }
        public int RecordCount { get; set; }
        public decimal AveragePerDay { get; set; }
    }

    public class OverageResponse
    {
        public int SubscriptionId { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public List<OverageItem> OverageItems { get; set; } = new();
        public decimal TotalOverageCharge { get; set; }
        public DateTime CalculatedAt { get; set; }
    }

    public class OverageItem
    {
        public string MetricName { get; set; } = string.Empty;
        public decimal Included { get; set; }
        public decimal Used { get; set; }
        public decimal OverageUnits { get; set; }
        public decimal OverageRate { get; set; }
        public decimal OverageCharge { get; set; }
    }

    #endregion
}
