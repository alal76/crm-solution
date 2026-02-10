// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under AGPL-3.0. See LICENSE for details.

using System.ComponentModel.DataAnnotations;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// API controller for subscription lifecycle, billing, plan changes, and usage.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class SubscriptionsController : ControllerBase
{
    private static readonly HashSet<string> AllowedBillingCycles = new(StringComparer.OrdinalIgnoreCase)
    {
        "Weekly", "Monthly", "Quarterly", "Yearly"
    };

    private readonly ISubscriptionService _subscriptionService;
    private readonly ILogger<SubscriptionsController> _logger;

    public SubscriptionsController(
        ISubscriptionService subscriptionService,
        ILogger<SubscriptionsController> logger)
    {
        _subscriptionService = subscriptionService;
        _logger = logger;
    }

    #region CRUD

    /// <summary>
    /// Get all subscriptions with optional filtering.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Subscription>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<Subscription>>> GetAll(
        [FromQuery] int? accountId = null,
        [FromQuery] SubscriptionStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var subscriptions = await _subscriptionService.GetAllAsync(accountId, status, cancellationToken);
        return Ok(subscriptions);
    }

    /// <summary>
    /// Get a subscription by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Subscription), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Subscription>> GetById(int id, CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionService.GetByIdAsync(id, cancellationToken);
        if (subscription == null)
        {
            return NotFound($"Subscription {id} not found.");
        }

        return Ok(subscription);
    }

    /// <summary>
    /// Create a subscription.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Subscription), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Subscription>> Create(
        [FromBody] SubscriptionCreateRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (!TryNormalizeBillingCycle(request.BillingCycle, out var normalizedCycle, out var billingError))
        {
            return BadRequest(billingError);
        }

        if (request.StartDate.HasValue && request.EndDate.HasValue && request.EndDate < request.StartDate)
        {
            return BadRequest("EndDate must be greater than or equal to StartDate.");
        }

        var subscription = MapToEntity(new Subscription(), request, normalizedCycle);

        try
        {
            var created = await _subscriptionService.CreateAsync(subscription, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating subscription");
            return HandleServiceException(ex);
        }
    }

    /// <summary>
    /// Update a subscription.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(Subscription), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Subscription>> Update(
        int id,
        [FromBody] SubscriptionUpdateRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (request.Id.HasValue && request.Id.Value != id)
        {
            return BadRequest("ID mismatch between URL and body.");
        }

        if (!TryNormalizeBillingCycle(request.BillingCycle, out var normalizedCycle, out var billingError))
        {
            return BadRequest(billingError);
        }

        if (request.StartDate.HasValue && request.EndDate.HasValue && request.EndDate < request.StartDate)
        {
            return BadRequest("EndDate must be greater than or equal to StartDate.");
        }

        var existing = await _subscriptionService.GetByIdAsync(id, cancellationToken);
        if (existing == null)
        {
            return NotFound($"Subscription {id} not found.");
        }

        MapToEntity(existing, request, normalizedCycle);

        try
        {
            var updated = await _subscriptionService.UpdateAsync(existing, cancellationToken);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating subscription {SubscriptionId}", id);
            return HandleServiceException(ex);
        }
    }

    /// <summary>
    /// Delete (soft delete) a subscription.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _subscriptionService.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound($"Subscription {id} not found.");
        }

        return NoContent();
    }

    #endregion

    #region Lifecycle

    /// <summary>
    /// Activate a subscription.
    /// </summary>
    [HttpPost("{id:int}/activate")]
    [ProducesResponseType(typeof(Subscription), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Subscription>> Activate(int id, CancellationToken cancellationToken)
    {
        return await ExecuteLifecycle(id, s => _subscriptionService.ActivateAsync(s, cancellationToken));
    }

    /// <summary>
    /// Pause a subscription.
    /// </summary>
    [HttpPost("{id:int}/pause")]
    [ProducesResponseType(typeof(Subscription), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Subscription>> Pause(
        int id,
        [FromBody] PauseRequest request,
        CancellationToken cancellationToken)
    {
        return await ExecuteLifecycle(id, s => _subscriptionService.PauseAsync(s, request.Reason, cancellationToken));
    }

    /// <summary>
    /// Resume a paused subscription.
    /// </summary>
    [HttpPost("{id:int}/resume")]
    [ProducesResponseType(typeof(Subscription), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Subscription>> Resume(int id, CancellationToken cancellationToken)
    {
        return await ExecuteLifecycle(id, s => _subscriptionService.ResumeAsync(s, cancellationToken));
    }

    /// <summary>
    /// Cancel a subscription.
    /// </summary>
    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType(typeof(Subscription), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Subscription>> Cancel(
        int id,
        [FromBody] CancelRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        return await ExecuteLifecycle(id, s => _subscriptionService.CancelAsync(s, request.Reason, request.Immediate, cancellationToken));
    }

    /// <summary>
    /// Suspend a subscription.
    /// </summary>
    [HttpPost("{id:int}/suspend")]
    [ProducesResponseType(typeof(Subscription), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Subscription>> Suspend(
        int id,
        [FromBody] SuspendRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        return await ExecuteLifecycle(id, s => _subscriptionService.SuspendAsync(s, request.Reason, cancellationToken));
    }

    /// <summary>
    /// Reactivate a suspended subscription.
    /// </summary>
    [HttpPost("{id:int}/reactivate")]
    [ProducesResponseType(typeof(Subscription), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Subscription>> Reactivate(int id, CancellationToken cancellationToken)
    {
        return await ExecuteLifecycle(id, s => _subscriptionService.ReactivateAsync(s, cancellationToken));
    }

    /// <summary>
    /// Renew a subscription.
    /// </summary>
    [HttpPost("{id:int}/renew")]
    [ProducesResponseType(typeof(Subscription), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Subscription>> Renew(int id, CancellationToken cancellationToken)
    {
        return await ExecuteLifecycle(id, s => _subscriptionService.RenewAsync(s, cancellationToken));
    }

    #endregion

    #region Plan Changes & Add-ons

    /// <summary>
    /// Change the plan for a subscription.
    /// </summary>
    [HttpPost("{id:int}/plan")]
    [ProducesResponseType(typeof(Subscription), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Subscription>> ChangePlan(
        int id,
        [FromBody] ChangePlanRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        return await ExecuteLifecycle(id, s => _subscriptionService.ChangePlanAsync(s, request.NewPlanId, request.ChangeType, cancellationToken));
    }

    /// <summary>
    /// Add an addon to a subscription.
    /// </summary>
    [HttpPost("{id:int}/addons")]
    [ProducesResponseType(typeof(Subscription), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Subscription>> AddAddon(
        int id,
        [FromBody] AddonRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        return await ExecuteLifecycle(id, s => _subscriptionService.AddAddonAsync(s, request.AddonId, request.Quantity, cancellationToken));
    }

    /// <summary>
    /// Remove an addon from a subscription.
    /// </summary>
    [HttpDelete("{id:int}/addons/{addonId:int}")]
    [ProducesResponseType(typeof(Subscription), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Subscription>> RemoveAddon(
        int id,
        int addonId,
        CancellationToken cancellationToken)
    {
        return await ExecuteLifecycle(id, s => _subscriptionService.RemoveAddonAsync(s, addonId, cancellationToken));
    }

    #endregion

    #region Billing

    /// <summary>
    /// Generate an invoice for a subscription.
    /// </summary>
    [HttpPost("{id:int}/invoice")]
    [ProducesResponseType(typeof(Invoice), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Invoice>> GenerateInvoice(int id, CancellationToken cancellationToken)
    {
        try
        {
            var invoice = await _subscriptionService.GenerateInvoiceAsync(id, cancellationToken);
            return Ok(invoice);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating invoice for subscription {SubscriptionId}", id);
            return HandleServiceException(ex);
        }
    }

    /// <summary>
    /// Get billing history for a subscription.
    /// </summary>
    [HttpGet("{id:int}/billing-history")]
    [ProducesResponseType(typeof(IEnumerable<Invoice>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<Invoice>>> GetBillingHistory(int id, CancellationToken cancellationToken)
    {
        var history = await _subscriptionService.GetBillingHistoryAsync(id, cancellationToken);
        return Ok(history);
    }

    /// <summary>
    /// Update billing details for a subscription.
    /// </summary>
    [HttpPost("{id:int}/billing-details")]
    [ProducesResponseType(typeof(Subscription), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Subscription>> UpdateBillingDetails(
        int id,
        [FromBody] BillingDetailsRequest request,
        CancellationToken cancellationToken)
    {
        var billingDetails = new BillingDetails
        {
            BillingEmail = request.BillingEmail,
            BillingName = request.BillingName,
            BillingAddress = request.BillingAddress,
            BillingCity = request.BillingCity,
            BillingState = request.BillingState,
            BillingZip = request.BillingZip,
            BillingCountry = request.BillingCountry,
            PaymentMethodId = request.PaymentMethodId
        };

        try
        {
            var updated = await _subscriptionService.UpdateBillingDetailsAsync(id, billingDetails, cancellationToken);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating billing details for subscription {SubscriptionId}", id);
            return HandleServiceException(ex);
        }
    }

    #endregion

    #region Usage

    /// <summary>
    /// Record usage for a subscription.
    /// </summary>
    [HttpPost("{id:int}/usage")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> RecordUsage(
        int id,
        [FromBody] RecordUsageRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            await _subscriptionService.RecordUsageAsync(id, request.MetricName, request.Quantity, request.Timestamp, cancellationToken);
            return Accepted();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording usage for subscription {SubscriptionId}", id);
            return HandleServiceException(ex);
        }
    }

    /// <summary>
    /// Get usage data for a subscription within a date range.
    /// </summary>
    [HttpGet("{id:int}/usage")]
    [ProducesResponseType(typeof(SubscriptionUsageData), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SubscriptionUsageData>> GetUsage(
        int id,
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        CancellationToken cancellationToken)
    {
        if (toDate < fromDate)
        {
            return BadRequest("toDate must be greater than or equal to fromDate.");
        }

        var usage = await _subscriptionService.GetUsageAsync(id, fromDate, toDate, cancellationToken);
        return Ok(usage);
    }

    /// <summary>
    /// Get usage limits for a subscription.
    /// </summary>
    [HttpGet("{id:int}/usage-limits")]
    [ProducesResponseType(typeof(IEnumerable<UsageLimit>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<UsageLimit>>> GetUsageLimits(int id, CancellationToken cancellationToken)
    {
        var limits = await _subscriptionService.GetUsageLimitsAsync(id, cancellationToken);
        return Ok(limits);
    }

    #endregion

    #region Queries

    /// <summary>
    /// Get subscriptions due for renewal within a specified number of days.
    /// </summary>
    [HttpGet("renewals")]
    [ProducesResponseType(typeof(IEnumerable<Subscription>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Subscription>>> GetDueForRenewal(
        [FromQuery] int withinDays = 30,
        CancellationToken cancellationToken = default)
    {
        var subscriptions = await _subscriptionService.GetDueForRenewalAsync(withinDays, cancellationToken);
        return Ok(subscriptions);
    }

    /// <summary>
    /// Get subscription statistics for a date range.
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(SubscriptionStatistics), StatusCodes.Status200OK)]
    public async Task<ActionResult<SubscriptionStatistics>> GetStatistics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var stats = await _subscriptionService.GetStatisticsAsync(fromDate, toDate, cancellationToken);
        return Ok(stats);
    }

    /// <summary>
    /// Get active subscriptions for a specific account.
    /// </summary>
    [HttpGet("active/{accountId:int}")]
    [ProducesResponseType(typeof(IEnumerable<Subscription>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Subscription>>> GetActiveForAccount(
        int accountId,
        CancellationToken cancellationToken = default)
    {
        var subscriptions = await _subscriptionService.GetActiveSubscriptionsAsync(accountId, cancellationToken);
        return Ok(subscriptions);
    }

    #endregion

    #region Helpers

    private static Subscription MapToEntity(Subscription target, SubscriptionBaseRequest request, string normalizedBillingCycle)
    {
        target.AccountId = request.AccountId;
        target.ProductId = request.ProductId;
        target.Amount = request.Amount;
        target.SubscriptionStatus = request.Status ?? target.SubscriptionStatus;
        target.BillingCycle = normalizedBillingCycle;
        target.StartDate = request.StartDate;
        target.EndDate = request.EndDate;
        target.BillingStartDate = request.BillingStartDate;
        target.BillingEndDate = request.BillingEndDate;
        target.IsAutoRenew = request.IsAutoRenew;
        target.Currency = request.Currency;
        target.BillingAddress = request.BillingAddress;
        target.BillingCity = request.BillingCity;
        target.BillingState = request.BillingState;
        target.BillingZip = request.BillingZip;
        target.BillingCountry = request.BillingCountry;
        target.BillingContactName = request.BillingContactName;
        target.BillingContactEmail = request.BillingContactEmail;
        target.BillingContactPhone = request.BillingContactPhone;
        target.ContractReference = request.ContractReference;
        target.ContractNotes = request.ContractNotes;
        target.CancelAtPeriodEnd = request.CancelAtPeriodEnd;
        target.MRR = request.MRR;
        target.ARR = request.ARR;
        target.SubscriptionManagerId = request.SubscriptionManagerId;
        target.OrderId = request.OrderId;
        target.Tags = request.Tags;
        target.RenewalDate = request.RenewalDate;
        target.NextBillingDate = request.NextBillingDate;
        target.CurrentPeriodEnd = request.CurrentPeriodEnd;
        target.CurrentPeriodStart = request.CurrentPeriodStart;
        target.PausedAt = request.PausedAt;
        target.PauseReason = request.PauseReason;
        return target;
    }

    private async Task<ActionResult<Subscription>> ExecuteLifecycle(
        int subscriptionId,
        Func<int, Task<Subscription>> action)
    {
        try
        {
            var result = await action(subscriptionId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lifecycle operation failed for subscription {SubscriptionId}", subscriptionId);
            return HandleServiceException(ex);
        }
    }

    private static bool TryNormalizeBillingCycle(string billingCycle, out string normalized, out string? error)
    {
        normalized = string.Empty;
        error = null;

        if (string.IsNullOrWhiteSpace(billingCycle))
        {
            error = "BillingCycle is required.";
            return false;
        }

        var candidate = billingCycle.Trim();
        if (AllowedBillingCycles.Contains(candidate))
        {
            normalized = CultureInfoInvariantTitle(candidate);
            return true;
        }

        error = "BillingCycle must be one of Weekly, Monthly, Quarterly, or Yearly.";
        return false;
    }

    private static string CultureInfoInvariantTitle(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;
        var lower = value.ToLowerInvariant();
        return lower switch
        {
            "weekly" => "Weekly",
            "monthly" => "Monthly",
            "quarterly" => "Quarterly",
            "yearly" => "Yearly",
            _ => value
        };
    }

    private ActionResult HandleServiceException(Exception ex)
    {
        if (ex is InvalidOperationException ioe && ioe.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(ioe.Message);
        }

        return BadRequest(ex.Message);
    }

    #endregion

    #region Request DTOs

    public abstract class SubscriptionBaseRequest
    {
        [Required]
        public int AccountId { get; set; }

        public int? ProductId { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public string BillingCycle { get; set; } = string.Empty;

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public DateTime? BillingStartDate { get; set; }
        public DateTime? BillingEndDate { get; set; }

        public bool IsAutoRenew { get; set; }
        public string? Currency { get; set; }

        public string? BillingAddress { get; set; }
        public string? BillingCity { get; set; }
        public string? BillingState { get; set; }
        public string? BillingZip { get; set; }
        public string? BillingCountry { get; set; }
        public string? BillingContactName { get; set; }

        [EmailAddress]
        public string? BillingContactEmail { get; set; }

        public string? BillingContactPhone { get; set; }

        public string? ContractReference { get; set; }
        public string? ContractNotes { get; set; }
        public bool CancelAtPeriodEnd { get; set; }
        public decimal? MRR { get; set; }
        public decimal? ARR { get; set; }
        public int? SubscriptionManagerId { get; set; }
        public int? OrderId { get; set; }
        public string? Tags { get; set; }
        public SubscriptionStatus? Status { get; set; }
        public DateTime? RenewalDate { get; set; }
        public DateTime? NextBillingDate { get; set; }
        public DateTime? CurrentPeriodStart { get; set; }
        public DateTime? CurrentPeriodEnd { get; set; }
        public DateTime? PausedAt { get; set; }
        public string? PauseReason { get; set; }
    }

    public class SubscriptionCreateRequest : SubscriptionBaseRequest
    {
    }

    public class SubscriptionUpdateRequest : SubscriptionBaseRequest
    {
        public int? Id { get; set; }
    }

    public class PauseRequest
    {
        public string? Reason { get; set; }
    }

    public class CancelRequest
    {
        [Required]
        public string Reason { get; set; } = string.Empty;

        public bool Immediate { get; set; }
    }

    public class SuspendRequest
    {
        [Required]
        public string Reason { get; set; } = string.Empty;
    }

    public class ChangePlanRequest
    {
        [Required]
        public int NewPlanId { get; set; }

        public SubscriptionChangeType ChangeType { get; set; } = SubscriptionChangeType.Immediate;
    }

    public class AddonRequest
    {
        [Required]
        public int AddonId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;
    }

    public class BillingDetailsRequest
    {
        [EmailAddress]
        public string? BillingEmail { get; set; }
        public string? BillingName { get; set; }
        public string? BillingAddress { get; set; }
        public string? BillingCity { get; set; }
        public string? BillingState { get; set; }
        public string? BillingZip { get; set; }
        public string? BillingCountry { get; set; }
        public string? PaymentMethodId { get; set; }
    }

    public class RecordUsageRequest
    {
        [Required]
        public string MetricName { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal Quantity { get; set; }

        public DateTime? Timestamp { get; set; }
    }

    #endregion
}
