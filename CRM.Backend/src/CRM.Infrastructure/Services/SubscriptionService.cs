// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Implementation of ISubscriptionService for subscription management operations.
/// </summary>
public class SubscriptionService : ISubscriptionService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<SubscriptionService> _logger;
    private static readonly HashSet<string> AllowedBillingCycles = new(StringComparer.OrdinalIgnoreCase)
    {
        "weekly",
        "monthly",
        "quarterly",
        "yearly",
        "annual"
    };

    public SubscriptionService(ICrmDbContext context, ILogger<SubscriptionService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region CRUD Operations

    public async Task<IEnumerable<Subscription>> GetAllAsync(
        int? accountId = null,
        SubscriptionStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Subscriptions
            .Include(s => s.Account)
            .Include(s => s.Product)
            .Where(s => !s.IsDeleted);

        if (accountId.HasValue)
        {
            query = query.Where(s => s.AccountId == accountId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(s => s.SubscriptionStatus == status.Value);
        }

        return await query.OrderByDescending(s => s.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<Subscription?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .Include(s => s.Account)
            .Include(s => s.Product)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, cancellationToken);
    }

    public async Task<Subscription?> GetBySubscriptionNumberAsync(string subscriptionNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .Include(s => s.Account)
            .Include(s => s.Product)
            .FirstOrDefaultAsync(s => s.SubscriptionNumber == subscriptionNumber && !s.IsDeleted, cancellationToken);
    }

    public async Task<Subscription> CreateAsync(Subscription subscription, CancellationToken cancellationToken = default)
    {
        ValidateSubscriptionInput(subscription);
        subscription.BillingCycle = NormalizeBillingCycle(subscription.BillingCycle);
        subscription.SubscriptionNumber = await GenerateSubscriptionNumberAsync(cancellationToken);
        subscription.CreatedAt = DateTime.UtcNow;
        subscription.UpdatedAt = DateTime.UtcNow;

        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created subscription {SubscriptionNumber} for account {AccountId}", subscription.SubscriptionNumber, subscription.AccountId);
        return subscription;
    }

    public async Task<Subscription> UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Subscriptions.FindAsync(new object[] { subscription.Id }, cancellationToken);
        if (existing == null || existing.IsDeleted)
        {
            throw new InvalidOperationException($"Subscription {subscription.Id} not found");
        }

        if (string.IsNullOrWhiteSpace(subscription.SubscriptionNumber))
        {
            throw new ArgumentException("SubscriptionNumber is required.", nameof(subscription));
        }

        ValidateSubscriptionInput(subscription);
        subscription.BillingCycle = NormalizeBillingCycle(subscription.BillingCycle);
        subscription.UpdatedAt = DateTime.UtcNow;
        _context.Subscriptions.Update(subscription);

        // TODO-SALES006-022: Handle DbUpdateConcurrencyException for optimistic locking
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict while updating subscription {SubscriptionId}. The record was modified by another process.", subscription.Id);
            throw new InvalidOperationException($"Subscription {subscription.Id} was modified by another process. Please refresh and try again.", ex);
        }

        _logger.LogInformation("Updated subscription {SubscriptionId}", subscription.Id);
        return subscription;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var subscription = await _context.Subscriptions.FindAsync(new object[] { id }, cancellationToken);
        if (subscription == null)
            return false;

        subscription.IsDeleted = true;
        subscription.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted subscription {SubscriptionId}", id);
        return true;
    }

    #endregion

    #region Subscription Operations

    public async Task<Subscription> CreateFromOrderAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .Include(o => o.LineItems)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted, cancellationToken);

        if (order == null)
        {
            throw new InvalidOperationException($"Order {orderId} not found");
        }

        var subscription = new Subscription
        {
            SubscriptionNumber = await GenerateSubscriptionNumberAsync(cancellationToken),
            AccountId = order.AccountId,
            SubscriptionStatus = SubscriptionStatus.Active,
            MRR = order.TotalAmount,
            ARR = order.TotalAmount * 12,
            BillingCycle = "Monthly",
            BillingStartDate = DateTime.UtcNow.Date,
            ContractStartDate = DateTime.UtcNow.Date,
            ContractEndDate = DateTime.UtcNow.Date.AddYears(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        ValidateSubscriptionInput(subscription);
        subscription.BillingCycle = NormalizeBillingCycle(subscription.BillingCycle);

        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created subscription {SubscriptionNumber} from order {OrderId}", subscription.SubscriptionNumber, orderId);
        return subscription;
    }

    public async Task<string> GenerateSubscriptionNumberAsync(CancellationToken cancellationToken = default)
    {
        var prefix = $"SUB-{DateTime.UtcNow:yyMM}-";
        var lastSubscription = await _context.Subscriptions
            .Where(s => s.SubscriptionNumber.StartsWith(prefix))
            .OrderByDescending(s => s.SubscriptionNumber)
            .FirstOrDefaultAsync(cancellationToken);

        var sequence = 1;
        if (lastSubscription != null)
        {
            var lastNum = lastSubscription.SubscriptionNumber.Split('-').LastOrDefault();
            if (int.TryParse(lastNum, out var num))
            {
                sequence = num + 1;
            }
        }

        return $"{prefix}{sequence:D4}";
    }

    public async Task<Subscription> ActivateAsync(int subscriptionId, CancellationToken cancellationToken = default)
    {
        var subscription = await GetByIdAsync(subscriptionId, cancellationToken);
        if (subscription == null)
        {
            throw new InvalidOperationException($"Subscription {subscriptionId} not found");
        }

        subscription.SubscriptionStatus = SubscriptionStatus.Active;
        subscription.UpdatedAt = DateTime.UtcNow;

        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Activated subscription {SubscriptionId}", subscriptionId);
        return subscription;
    }

    public async Task<Subscription> PauseAsync(int subscriptionId, string? reason = null, CancellationToken cancellationToken = default)
    {
        var subscription = await GetByIdAsync(subscriptionId, cancellationToken);
        if (subscription == null)
        {
            throw new InvalidOperationException($"Subscription {subscriptionId} not found");
        }

        subscription.SubscriptionStatus = SubscriptionStatus.Paused;
        subscription.ContractNotes = string.IsNullOrEmpty(subscription.ContractNotes)
            ? $"Paused: {reason}"
            : $"{subscription.ContractNotes}; Paused: {reason}";
        subscription.UpdatedAt = DateTime.UtcNow;

        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Paused subscription {SubscriptionId}: {Reason}", subscriptionId, reason);
        return subscription;
    }

    public async Task<Subscription> ResumeAsync(int subscriptionId, CancellationToken cancellationToken = default)
    {
        var subscription = await GetByIdAsync(subscriptionId, cancellationToken);
        if (subscription == null)
        {
            throw new InvalidOperationException($"Subscription {subscriptionId} not found");
        }

        if (subscription.SubscriptionStatus != SubscriptionStatus.Paused)
        {
            throw new InvalidOperationException($"Subscription {subscriptionId} is not paused");
        }

        subscription.SubscriptionStatus = SubscriptionStatus.Active;
        subscription.UpdatedAt = DateTime.UtcNow;

        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Resumed subscription {SubscriptionId}", subscriptionId);
        return subscription;
    }

    public async Task<Subscription> CancelAsync(int subscriptionId, string reason, bool immediate = false, CancellationToken cancellationToken = default)
    {
        var subscription = await GetByIdAsync(subscriptionId, cancellationToken);
        if (subscription == null)
        {
            throw new InvalidOperationException($"Subscription {subscriptionId} not found");
        }

        if (immediate)
        {
            subscription.SubscriptionStatus = SubscriptionStatus.Cancelled;
            subscription.ContractEndDate = DateTime.UtcNow;
        }
        else
        {
            subscription.SubscriptionStatus = SubscriptionStatus.PendingCancellation;
        }

        subscription.ContractNotes = string.IsNullOrEmpty(subscription.ContractNotes)
            ? $"Cancellation reason: {reason}"
            : $"{subscription.ContractNotes}; Cancellation reason: {reason}";
        subscription.UpdatedAt = DateTime.UtcNow;

        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Cancelled subscription {SubscriptionId}: {Reason}", subscriptionId, reason);
        return subscription;
    }

    #endregion

    #region Status Management

    public async Task<Subscription> UpdateStatusAsync(int subscriptionId, SubscriptionStatus status, CancellationToken cancellationToken = default)
    {
        var subscription = await GetByIdAsync(subscriptionId, cancellationToken);
        if (subscription == null)
        {
            throw new InvalidOperationException($"Subscription {subscriptionId} not found");
        }

        subscription.SubscriptionStatus = status;
        subscription.UpdatedAt = DateTime.UtcNow;

        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated subscription {SubscriptionId} status to {Status}", subscriptionId, status);
        return subscription;
    }

    public async Task<Subscription> SuspendAsync(int subscriptionId, string reason, CancellationToken cancellationToken = default)
    {
        var subscription = await GetByIdAsync(subscriptionId, cancellationToken);
        if (subscription == null)
        {
            throw new InvalidOperationException($"Subscription {subscriptionId} not found");
        }

        subscription.SubscriptionStatus = SubscriptionStatus.Suspended;
        subscription.ContractNotes = string.IsNullOrEmpty(subscription.ContractNotes)
            ? $"Suspended: {reason}"
            : $"{subscription.ContractNotes}; Suspended: {reason}";
        subscription.UpdatedAt = DateTime.UtcNow;

        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Suspended subscription {SubscriptionId}: {Reason}", subscriptionId, reason);
        return subscription;
    }

    public async Task<Subscription> ReactivateAsync(int subscriptionId, CancellationToken cancellationToken = default)
    {
        var subscription = await GetByIdAsync(subscriptionId, cancellationToken);
        if (subscription == null)
        {
            throw new InvalidOperationException($"Subscription {subscriptionId} not found");
        }

        subscription.SubscriptionStatus = SubscriptionStatus.Active;
        subscription.UpdatedAt = DateTime.UtcNow;

        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Reactivated subscription {SubscriptionId}", subscriptionId);
        return subscription;
    }

    #endregion

    #region Billing

    public async Task<Invoice> GenerateInvoiceAsync(int subscriptionId, CancellationToken cancellationToken = default)
    {
        var subscription = await GetByIdAsync(subscriptionId, cancellationToken);
        if (subscription == null)
        {
            throw new InvalidOperationException($"Subscription {subscriptionId} not found");
        }

        var invoice = new Invoice
        {
            InvoiceNumber = await GenerateInvoiceNumberAsync(cancellationToken),
            AccountId = subscription.AccountId,
            Status = InvoiceStatus.Draft,
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Subtotal = subscription.MRR ?? 0,
            TaxAmount = 0,
            TotalAmount = subscription.MRR ?? 0,
            Notes = $"Subscription billing for {subscription.SubscriptionNumber}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Generated invoice {InvoiceNumber} for subscription {SubscriptionId}", invoice.InvoiceNumber, subscriptionId);
        return invoice;
    }

    public async Task<IEnumerable<Invoice>> GetBillingHistoryAsync(int subscriptionId, CancellationToken cancellationToken = default)
    {
        var subscription = await GetByIdAsync(subscriptionId, cancellationToken);
        if (subscription == null)
        {
            return Enumerable.Empty<Invoice>();
        }

        return await _context.Invoices
            .Where(i => i.AccountId == subscription.AccountId && !i.IsDeleted)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> CalculateProratedAmountAsync(int subscriptionId, DateTime changeDate, decimal newAmount, CancellationToken cancellationToken = default)
    {
        var subscription = await GetByIdAsync(subscriptionId, cancellationToken);
        if (subscription == null)
        {
            throw new InvalidOperationException($"Subscription {subscriptionId} not found");
        }

        var billingEnd = subscription.BillingEndDate ?? DateTime.UtcNow.AddMonths(1);
        var daysRemaining = (int)Math.Ceiling((billingEnd - changeDate).TotalDays);
        var totalDays = subscription.BillingPeriod switch
        {
            BillingPeriod.Weekly => 7,
            BillingPeriod.Monthly => 30,
            BillingPeriod.Quarterly => 90,
            BillingPeriod.Yearly => 365,
            _ => 30
        };

        var proratedAmount = (newAmount / totalDays) * daysRemaining;
        return Math.Round(proratedAmount, 2);
    }

    public async Task<DateTime?> GetNextBillingDateAsync(int subscriptionId, CancellationToken cancellationToken = default)
    {
        var subscription = await GetByIdAsync(subscriptionId, cancellationToken);
        if (subscription == null)
        {
            return null;
        }

        if (!subscription.BillingStartDate.HasValue)
        {
            return null;
        }

        var nextDate = subscription.BillingEndDate ?? subscription.BillingStartDate.Value.AddMonths(1);
        return nextDate;
    }

    public async Task<Subscription> UpdateBillingDetailsAsync(int subscriptionId, BillingDetails details, CancellationToken cancellationToken = default)
    {
        var subscription = await GetByIdAsync(subscriptionId, cancellationToken);
        if (subscription == null)
        {
            throw new InvalidOperationException($"Subscription {subscriptionId} not found");
        }

        subscription.BillingAddress = details.BillingAddress;
        subscription.BillingCity = details.BillingCity;
        subscription.UpdatedAt = DateTime.UtcNow;

        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated billing details for subscription {SubscriptionId}", subscriptionId);
        return subscription;
    }

    #endregion

    #region Plan Changes

    public async Task<Subscription> UpgradeAsync(int subscriptionId, int newPlanId, bool immediate = true, CancellationToken cancellationToken = default)
    {
        return await ChangePlanAsync(subscriptionId, newPlanId, immediate ? SubscriptionChangeType.Immediate : SubscriptionChangeType.NextBillingCycle, cancellationToken);
    }

    public async Task<Subscription> DowngradeAsync(int subscriptionId, int newPlanId, CancellationToken cancellationToken = default)
    {
        return await ChangePlanAsync(subscriptionId, newPlanId, SubscriptionChangeType.EndOfPeriod, cancellationToken);
    }

    public async Task<Subscription> ChangePlanAsync(int subscriptionId, int newPlanId, SubscriptionChangeType changeType, CancellationToken cancellationToken = default)
    {
        var subscription = await GetByIdAsync(subscriptionId, cancellationToken);
        if (subscription == null)
        {
            throw new InvalidOperationException($"Subscription {subscriptionId} not found");
        }

        var product = await _context.Products.FindAsync(new object[] { newPlanId }, cancellationToken);
        if (product == null)
        {
            throw new InvalidOperationException($"Product/Plan {newPlanId} not found");
        }

        if (changeType == SubscriptionChangeType.Immediate)
        {
            subscription.ProductId = newPlanId;
            subscription.MRR = product.UnitPrice;
            subscription.ARR = product.UnitPrice * 12;
        }
        else
        {
            subscription.ContractNotes = $"Plan change to {product.Name} scheduled for {(changeType == SubscriptionChangeType.EndOfPeriod ? "end of period" : "next billing cycle")}";
        }

        subscription.UpdatedAt = DateTime.UtcNow;

        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Changed plan for subscription {SubscriptionId} to {NewPlanId}", subscriptionId, newPlanId);
        return subscription;
    }

    public async Task<Subscription> AddAddonAsync(int subscriptionId, int addonId, int quantity = 1, CancellationToken cancellationToken = default)
    {
        var subscription = await GetByIdAsync(subscriptionId, cancellationToken);
        if (subscription == null)
        {
            throw new InvalidOperationException($"Subscription {subscriptionId} not found");
        }

        var addon = await _context.Products.FindAsync(new object[] { addonId }, cancellationToken);
        if (addon == null)
        {
            throw new InvalidOperationException($"Addon product {addonId} not found");
        }

        subscription.MRR = (subscription.MRR ?? 0) + (addon.UnitPrice * quantity);
        subscription.ARR = subscription.MRR * 12;
        subscription.UpdatedAt = DateTime.UtcNow;

        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Added addon {AddonId} x{Quantity} to subscription {SubscriptionId}", addonId, quantity, subscriptionId);
        return subscription;
    }

    public async Task<Subscription> RemoveAddonAsync(int subscriptionId, int addonId, CancellationToken cancellationToken = default)
    {
        var subscription = await GetByIdAsync(subscriptionId, cancellationToken);
        if (subscription == null)
        {
            throw new InvalidOperationException($"Subscription {subscriptionId} not found");
        }

        var addon = await _context.Products.FindAsync(new object[] { addonId }, cancellationToken);
        if (addon == null)
        {
            throw new InvalidOperationException($"Addon product {addonId} not found");
        }

        subscription.MRR = Math.Max(0, (subscription.MRR ?? 0) - addon.UnitPrice);
        subscription.ARR = subscription.MRR * 12;
        subscription.UpdatedAt = DateTime.UtcNow;

        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Removed addon {AddonId} from subscription {SubscriptionId}", addonId, subscriptionId);
        return subscription;
    }

    #endregion

    #region Renewal

    public async Task<Subscription> RenewAsync(int subscriptionId, CancellationToken cancellationToken = default)
    {
        var subscription = await GetByIdAsync(subscriptionId, cancellationToken);
        if (subscription == null)
        {
            throw new InvalidOperationException($"Subscription {subscriptionId} not found");
        }

        if (subscription.SubscriptionStatus == SubscriptionStatus.Paused ||
            subscription.SubscriptionStatus == SubscriptionStatus.Cancelled ||
            subscription.SubscriptionStatus == SubscriptionStatus.Suspended)
        {
            throw new InvalidOperationException(
                $"Cannot renew subscription {subscriptionId} with status {subscription.SubscriptionStatus}. Only Active subscriptions can be renewed.");
        }

        var termLength = subscription.BillingPeriod switch
        {
            BillingPeriod.Weekly => TimeSpan.FromDays(7),
            BillingPeriod.Monthly => TimeSpan.FromDays(30),
            BillingPeriod.Quarterly => TimeSpan.FromDays(90),
            BillingPeriod.Yearly => TimeSpan.FromDays(365),
            _ => TimeSpan.FromDays(30)
        };

        subscription.ContractStartDate = subscription.ContractEndDate ?? DateTime.UtcNow;
        subscription.ContractEndDate = subscription.ContractStartDate.Value.Add(termLength);
        subscription.BillingStartDate = subscription.ContractStartDate;
        subscription.BillingEndDate = subscription.ContractEndDate;
        subscription.SubscriptionStatus = SubscriptionStatus.Active;
        subscription.UpdatedAt = DateTime.UtcNow;

        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Renewed subscription {SubscriptionId}", subscriptionId);
        return subscription;
    }

    public async Task<IEnumerable<Subscription>> GetDueForRenewalAsync(int withinDays, CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.Date.AddDays(withinDays);

        return await _context.Subscriptions
            .Include(s => s.Account)
            .Where(s => !s.IsDeleted)
            .Where(s => s.SubscriptionStatus == SubscriptionStatus.Active)
            .Where(s => s.ContractEndDate.HasValue && s.ContractEndDate.Value <= cutoffDate && s.ContractEndDate.Value >= DateTime.UtcNow.Date)
            .OrderBy(s => s.ContractEndDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Subscription> SetAutoRenewalAsync(int subscriptionId, bool autoRenew, CancellationToken cancellationToken = default)
    {
        var subscription = await GetByIdAsync(subscriptionId, cancellationToken);
        if (subscription == null)
        {
            throw new InvalidOperationException($"Subscription {subscriptionId} not found");
        }

        subscription.ContractNotes = autoRenew ? "Auto-renewal enabled" : "Auto-renewal disabled";
        subscription.IsAutoRenew = autoRenew;
        subscription.UpdatedAt = DateTime.UtcNow;

        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Set auto-renewal to {AutoRenew} for subscription {SubscriptionId}", autoRenew, subscriptionId);
        return subscription;
    }

    #endregion

    #region Usage

    public async Task<bool> RecordUsageAsync(int subscriptionId, string metricName, decimal quantity, DateTime? timestamp = null, CancellationToken cancellationToken = default)
    {
        var subscription = await GetByIdAsync(subscriptionId, cancellationToken);
        if (subscription == null)
        {
            throw new InvalidOperationException($"Subscription {subscriptionId} not found");
        }

        var normalizedMetric = metricName?.Trim() ?? string.Empty;
        var limit = await _context.SubscriptionUsageLimits
            .FirstOrDefaultAsync(l => l.SubscriptionId == subscriptionId && l.MetricName == normalizedMetric && !l.IsDeleted, cancellationToken);

        if (limit != null)
        {
            var used = await _context.SubscriptionUsages
                .Where(u => u.SubscriptionId == subscriptionId && u.MetricName == normalizedMetric)
                .SumAsync(u => u.Quantity, cancellationToken);

            if (limit.EnforceCap && used + quantity > limit.Limit)
            {
                throw new InvalidOperationException($"Usage for {normalizedMetric} exceeds the configured limit.");
            }
        }

        var usage = new SubscriptionUsage
        {
            SubscriptionId = subscriptionId,
            MetricName = normalizedMetric,
            Quantity = quantity,
            Timestamp = timestamp ?? DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.SubscriptionUsages.Add(usage);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Recorded usage {MetricName}={Quantity} for subscription {SubscriptionId}", metricName, quantity, subscriptionId);
        return true;
    }

    public async Task<SubscriptionUsageData> GetUsageAsync(int subscriptionId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        var usages = await _context.SubscriptionUsages
            .Where(u => u.SubscriptionId == subscriptionId && u.Timestamp >= fromDate && u.Timestamp <= toDate)
            .ToListAsync(cancellationToken);

        var metrics = usages
            .GroupBy(u => u.MetricName)
            .Select(g => new UsageMetric
            {
                MetricName = g.Key,
                TotalUsage = g.Sum(u => u.Quantity),
                Records = g.Select(u => new UsageRecord
                {
                    Timestamp = u.Timestamp ?? u.UsageDate,
                    Quantity = u.Quantity
                }).ToList()
            })
            .ToList();

        return new SubscriptionUsageData
        {
            SubscriptionId = subscriptionId,
            FromDate = fromDate,
            ToDate = toDate,
            Metrics = metrics
        };
    }

    public async Task<IEnumerable<UsageLimit>> GetUsageLimitsAsync(int subscriptionId, CancellationToken cancellationToken = default)
    {
        var limits = await _context.SubscriptionUsageLimits
            .Where(l => l.SubscriptionId == subscriptionId && !l.IsDeleted)
            .ToListAsync(cancellationToken);

        if (!limits.Any())
        {
            return Enumerable.Empty<UsageLimit>();
        }

        var usageByMetric = await _context.SubscriptionUsages
            .Where(u => u.SubscriptionId == subscriptionId)
            .GroupBy(u => u.MetricName)
            .Select(g => new { Metric = g.Key, Used = g.Sum(u => u.Quantity) })
            .ToDictionaryAsync(x => x.Metric, x => x.Used, cancellationToken);

        return limits.Select(l => new UsageLimit
        {
            MetricName = l.MetricName,
            Limit = l.Limit,
            Used = usageByMetric.TryGetValue(l.MetricName, out var used) ? used : 0
        }).ToList();
    }

    /// <summary>
    /// Records multiple usage records in a batch for performance optimization.
    /// TODO-SALES006-024: Batch usage recording implementation.
    /// </summary>
    public async Task<int> RecordUsageBatchAsync(List<UsageRecordBatchDto> usageRecords, CancellationToken cancellationToken = default)
    {
        if (usageRecords == null || !usageRecords.Any())
        {
            return 0;
        }

        _logger.LogInformation("Recording batch of {Count} usage records", usageRecords.Count);

        // Validate all subscription IDs exist
        var subscriptionIds = usageRecords.Select(r => r.SubscriptionId).Distinct().ToList();
        var validSubscriptions = await _context.Subscriptions
            .Where(s => subscriptionIds.Contains(s.Id) && !s.IsDeleted)
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);

        var invalidIds = subscriptionIds.Except(validSubscriptions).ToList();
        if (invalidIds.Any())
        {
            throw new InvalidOperationException($"Invalid subscription IDs: {string.Join(", ", invalidIds)}");
        }

        // Check usage limits (optional - can be enforced or just logged)
        var now = DateTime.UtcNow;
        var usageEntities = usageRecords.Select(r => new SubscriptionUsage
        {
            SubscriptionId = r.SubscriptionId,
            MetricName = r.MetricName?.Trim() ?? string.Empty,
            Quantity = r.Quantity,
            Timestamp = r.Timestamp ?? now,
            UsageDate = r.Timestamp?.Date ?? now.Date,
            Description = r.Description,
            CreatedAt = now,
            UpdatedAt = now
        }).ToList();

        // Batch insert
        foreach (var usage in usageEntities)
        {
            _context.SubscriptionUsages.Add(usage);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully recorded {Count} usage records in batch", usageEntities.Count);
        return usageEntities.Count;
    }

    #endregion

    #region Timezone-Aware Billing (TODO-SALES006-023)

    /// <summary>
    /// Gets the next billing date using the subscription's billing timezone.
    /// </summary>
    public async Task<DateTimeOffset?> GetNextBillingDateWithTimezoneAsync(int subscriptionId, CancellationToken cancellationToken = default)
    {
        var subscription = await GetByIdAsync(subscriptionId, cancellationToken);
        if (subscription == null)
        {
            return null;
        }

        if (!subscription.NextBillingDate.HasValue)
        {
            // Calculate next billing date from billing start
            if (!subscription.BillingStartDate.HasValue)
            {
                return null;
            }

            var nextDate = CalculateNextBillingDate(subscription.BillingStartDate.Value, subscription.BillingPeriod);
            subscription.NextBillingDate = nextDate;
        }

        // Apply timezone if configured
        var billingDate = subscription.NextBillingDate.Value;
        if (!string.IsNullOrWhiteSpace(subscription.BillingTimezone))
        {
            try
            {
                var timeZone = TimeZoneInfo.FindSystemTimeZoneById(subscription.BillingTimezone);
                return new DateTimeOffset(billingDate, timeZone.GetUtcOffset(billingDate));
            }
            catch (TimeZoneNotFoundException)
            {
                _logger.LogWarning("Invalid timezone '{Timezone}' for subscription {SubscriptionId}, using UTC",
                    subscription.BillingTimezone, subscriptionId);
            }
        }

        return new DateTimeOffset(billingDate, TimeSpan.Zero);
    }

    private static DateTime CalculateNextBillingDate(DateTime startDate, BillingPeriod period)
    {
        var now = DateTime.UtcNow;
        var nextDate = startDate;

        while (nextDate <= now)
        {
            nextDate = period switch
            {
                BillingPeriod.Weekly => nextDate.AddDays(7),
                BillingPeriod.Monthly => nextDate.AddMonths(1),
                BillingPeriod.Quarterly => nextDate.AddMonths(3),
                BillingPeriod.Yearly => nextDate.AddYears(1),
                _ => nextDate.AddMonths(1)
            };
        }

        return nextDate;
    }

    #endregion

    #region Queries

    public async Task<IEnumerable<Subscription>> GetActiveSubscriptionsAsync(int accountId, CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .Include(s => s.Product)
            .Where(s => s.AccountId == accountId && s.SubscriptionStatus == SubscriptionStatus.Active && !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Subscription>> GetExpiringSubscriptionsAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .Include(s => s.Account)
            .Where(s => !s.IsDeleted)
            .Where(s => s.SubscriptionStatus == SubscriptionStatus.Active)
            .Where(s => s.ContractEndDate.HasValue && s.ContractEndDate.Value >= fromDate && s.ContractEndDate.Value <= toDate)
            .OrderBy(s => s.ContractEndDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<SubscriptionStatistics> GetStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Subscriptions.Where(s => !s.IsDeleted);

        if (fromDate.HasValue)
        {
            query = query.Where(s => s.CreatedAt >= fromDate.Value);
        }
        if (toDate.HasValue)
        {
            query = query.Where(s => s.CreatedAt <= toDate.Value);
        }

        var subscriptions = await query.ToListAsync(cancellationToken);

        var active = subscriptions.Where(s => s.SubscriptionStatus == SubscriptionStatus.Active).ToList();
        var mrr = active.Sum(s => s.MRR ?? 0);

        return new SubscriptionStatistics
        {
            TotalSubscriptions = subscriptions.Count,
            ActiveSubscriptions = active.Count,
            TrialSubscriptions = subscriptions.Count(s => s.SubscriptionStatus == SubscriptionStatus.Trial),
            CancelledSubscriptions = subscriptions.Count(s => s.SubscriptionStatus == SubscriptionStatus.Cancelled),
            PausedSubscriptions = subscriptions.Count(s => s.SubscriptionStatus == SubscriptionStatus.Paused),
            MRR = mrr,
            ARR = mrr * 12,
            ChurnRate = await GetChurnRateAsync(fromDate ?? DateTime.UtcNow.AddMonths(-1), toDate ?? DateTime.UtcNow, cancellationToken),
            ConversionRate = 0, // Would need trial-to-paid tracking
            AverageRevenuePerUser = active.Count > 0 ? mrr / active.Count : 0,
            NewSubscriptionsThisMonth = subscriptions.Count(s => s.CreatedAt >= DateTime.UtcNow.AddMonths(-1)),
            CancellationsThisMonth = subscriptions.Count(s => s.SubscriptionStatus == SubscriptionStatus.Cancelled && s.UpdatedAt >= DateTime.UtcNow.AddMonths(-1)),
            SubscriptionsByPlan = subscriptions.GroupBy(s => s.Product?.Name ?? "Unknown").ToDictionary(g => g.Key, g => g.Count())
        };
    }

    public async Task<decimal> CalculateMRRAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .Where(s => !s.IsDeleted && s.SubscriptionStatus == SubscriptionStatus.Active)
            .SumAsync(s => s.MRR ?? 0, cancellationToken);
    }

    public async Task<decimal> CalculateARRAsync(CancellationToken cancellationToken = default)
    {
        var mrr = await CalculateMRRAsync(cancellationToken);
        return mrr * 12;
    }

    public async Task<double> GetChurnRateAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        var startCount = await _context.Subscriptions
            .CountAsync(s => !s.IsDeleted && s.CreatedAt < fromDate &&
                (s.SubscriptionStatus == SubscriptionStatus.Active ||
                 (s.SubscriptionStatus == SubscriptionStatus.Cancelled &&
                  s.UpdatedAt >= fromDate && s.UpdatedAt <= toDate)),
                cancellationToken);

        if (startCount == 0)
            return 0;

        var churned = await _context.Subscriptions
            .CountAsync(s => !s.IsDeleted && s.SubscriptionStatus == SubscriptionStatus.Cancelled && s.UpdatedAt >= fromDate && s.UpdatedAt <= toDate, cancellationToken);

        return (double)churned / startCount * 100;
    }

    #endregion

    #region Helpers

    private static string NormalizeBillingCycle(string? billingCycle)
    {
        if (string.IsNullOrWhiteSpace(billingCycle))
            return "Monthly";

        var normalized = billingCycle.Trim().ToLowerInvariant();
        if (!AllowedBillingCycles.Contains(normalized))
        {
            throw new ArgumentException($"Unsupported billing cycle '{billingCycle}'. Allowed values: Weekly, Monthly, Quarterly, Yearly, Annual.");
        }

        return normalized switch
        {
            "annual" => "Yearly",
            _ => char.ToUpperInvariant(normalized[0]) + normalized[1..]
        };
    }

    private static void ValidateSubscriptionInput(Subscription subscription)
    {
        if (subscription.AccountId <= 0)
        {
            throw new ArgumentException("AccountId is required for a subscription.");
        }

        if (subscription.Amount < 0)
        {
            throw new ArgumentException("Amount must be greater than or equal to zero.");
        }

        if (string.IsNullOrWhiteSpace(subscription.BillingCycle))
        {
            throw new ArgumentException("BillingCycle is required.", nameof(subscription));
        }

        if (subscription.IsAutoRenew && subscription.SubscriptionStatus == SubscriptionStatus.Cancelled)
        {
            throw new InvalidOperationException("Cannot set AutoRenew on a cancelled subscription.");
        }

        if (subscription.MRR.HasValue && subscription.MRR.Value < 0)
        {
            throw new ArgumentException("MRR must be greater than or equal to zero.");
        }

        if (subscription.ARR.HasValue && subscription.ARR.Value < 0)
        {
            throw new ArgumentException("ARR must be greater than or equal to zero.");
        }

        if (subscription.StartDate.HasValue && subscription.EndDate.HasValue && subscription.EndDate < subscription.StartDate)
        {
            throw new ArgumentException("EndDate must be greater than or equal to StartDate.");
        }

        if (subscription.BillingStartDate.HasValue && subscription.BillingEndDate.HasValue && subscription.BillingEndDate < subscription.BillingStartDate)
        {
            throw new ArgumentException("BillingEndDate must be greater than or equal to BillingStartDate.");
        }

        // TODO-SALES006-019: Trial date validation
        if (subscription.TrialStartDate.HasValue && subscription.TrialEndDate.HasValue
            && subscription.TrialEndDate <= subscription.TrialStartDate)
        {
            throw new ArgumentException("TrialEndDate must be greater than TrialStartDate.");
        }

        if (subscription.TrialEndDate.HasValue && !subscription.TrialStartDate.HasValue)
        {
            throw new ArgumentException("TrialStartDate is required when TrialEndDate is set.");
        }

        // TODO-SALES006-019: Proration type validation
        if (!Enum.IsDefined(typeof(ProrationStrategy), subscription.ProrationType))
        {
            throw new ArgumentException($"Invalid ProrationType value: {subscription.ProrationType}.");
        }

        // TODO-SALES006-025: Dunning validation
        if (subscription.DunningGracePeriodDays < 0)
        {
            throw new ArgumentException("DunningGracePeriodDays must be greater than or equal to zero.");
        }

        _ = NormalizeBillingCycle(subscription.BillingCycle); // will throw if invalid
    }

    private async Task<string> GenerateInvoiceNumberAsync(CancellationToken cancellationToken)
    {
        var prefix = "INV";
        var year = DateTime.UtcNow.ToString("yy");
        var month = DateTime.UtcNow.ToString("MM");

        var lastInvoice = await _context.Invoices
            .Where(i => i.InvoiceNumber.StartsWith($"{prefix}-{year}{month}"))
            .OrderByDescending(i => i.InvoiceNumber)
            .FirstOrDefaultAsync(cancellationToken);

        int sequence = 1;
        if (lastInvoice != null)
        {
            var parts = lastInvoice.InvoiceNumber.Split('-');
            if (parts.Length >= 2 && int.TryParse(parts[^1], out int lastSeq))
            {
                sequence = lastSeq + 1;
            }
        }

        return $"{prefix}-{year}{month}-{sequence:D4}";
    }

    #endregion
}
