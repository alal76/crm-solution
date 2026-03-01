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
/// Implementation of ICommissionService for commission management operations.
/// </summary>
public class CommissionService : ICommissionService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<CommissionService> _logger;
    private const string CommissionNotFoundMessage = "Commission {0} not found";
    private const string CommissionPlanNotFoundMessage = "Commission plan {0} not found";

    public CommissionService(ICrmDbContext context, ILogger<CommissionService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region CRUD Operations

    public async Task<IEnumerable<Commission>> GetAllAsync(
        int? userId = null,
        CommissionStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Commissions
            .Include(c => c.User)
            .Include(c => c.CommissionPlan)
            .Where(c => !c.IsDeleted);

        if (userId.HasValue)
        {
            query = query.Where(c => c.UserId == userId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status.Value);
        }

        return await query.OrderByDescending(c => c.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<Commission?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Commissions
            .Include(c => c.User)
            .Include(c => c.CommissionPlan)
            .Include(c => c.Opportunity)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);
    }

    public async Task<Commission> CreateAsync(Commission commission, CancellationToken cancellationToken = default)
    {
        commission.CreatedAt = DateTime.UtcNow;
        commission.IsDeleted = false;

        _context.Commissions.Add(commission);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created commission {CommissionId} for user {UserId}", commission.Id, commission.UserId);
        return commission;
    }

    public async Task<Commission> UpdateAsync(Commission commission, CancellationToken cancellationToken = default)
    {
        var exists = await _context.Commissions
            .AsNoTracking()
            .AnyAsync(c => c.Id == commission.Id && !c.IsDeleted, cancellationToken);
        if (!exists)
        {
            throw new InvalidOperationException(string.Format(CommissionNotFoundMessage, commission.Id));
        }

        _context.Commissions.Update(commission);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated commission {CommissionId}", commission.Id);
        return commission;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var commission = await _context.Commissions.FindAsync(new object[] { id }, cancellationToken);
        if (commission == null)
        {
            return false;
        }

        commission.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted commission {CommissionId}", id);
        return true;
    }

    #endregion

    #region Commission Calculation

    public async Task<CommissionCalculation> CalculateForDealAsync(int opportunityId, CancellationToken cancellationToken = default)
    {
        var opportunity = await _context.Opportunities
            .Include(o => o.SalesOwner)
            .FirstOrDefaultAsync(o => o.Id == opportunityId && !o.IsDeleted, cancellationToken);

        if (opportunity == null)
        {
            throw new InvalidOperationException($"Opportunity {opportunityId} not found");
        }

        var userId = opportunity.SalesOwnerId ?? 0;
        var plan = await GetUserPlanAsync(userId, cancellationToken);

        var baseAmount = opportunity.Amount;
        var commissionRate = plan?.BaseRate ?? 0.05m; // Default 5%
        var calculatedAmount = baseAmount * commissionRate;

        return new CommissionCalculation
        {
            UserId = userId,
            OpportunityId = opportunityId,
            PlanId = plan?.Id,
            PlanName = plan?.Name,
            BaseAmount = baseAmount,
            CommissionRate = commissionRate,
            CalculatedAmount = calculatedAmount,
            FinalAmount = calculatedAmount,
            Breakdown = new List<CommissionBreakdown>
            {
                new CommissionBreakdown
                {
                    Description = "Base Commission",
                    Amount = baseAmount,
                    Rate = commissionRate,
                    Result = calculatedAmount
                }
            }
        };
    }

    public async Task<CommissionCalculation> CalculateForOrderAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted, cancellationToken);

        if (order == null)
        {
            throw new InvalidOperationException($"Order {orderId} not found");
        }

        var userId = order.SalesRepId ?? 0;
        var plan = await GetUserPlanAsync(userId, cancellationToken);

        var baseAmount = order.TotalAmount;
        var commissionRate = plan?.BaseRate ?? 0.05m;
        var calculatedAmount = baseAmount * commissionRate;

        return new CommissionCalculation
        {
            UserId = userId,
            OrderId = orderId,
            PlanId = plan?.Id,
            PlanName = plan?.Name,
            BaseAmount = baseAmount,
            CommissionRate = commissionRate,
            CalculatedAmount = calculatedAmount,
            FinalAmount = calculatedAmount,
            Breakdown = new List<CommissionBreakdown>
            {
                new CommissionBreakdown
                {
                    Description = "Base Commission",
                    Amount = baseAmount,
                    Rate = commissionRate,
                    Result = calculatedAmount
                }
            }
        };
    }

    public async Task<CommissionSummary> CalculateForPeriodAsync(int userId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        var commissions = await _context.Commissions
            .Where(c => c.UserId == userId && !c.IsDeleted)
            .Where(c => c.CreatedAt >= fromDate && c.CreatedAt <= toDate)
            .ToListAsync(cancellationToken);

        return new CommissionSummary
        {
            UserId = userId,
            FromDate = fromDate,
            ToDate = toDate,
            TotalEarned = commissions.Sum(c => c.Amount),
            TotalPaid = commissions.Where(c => c.Status == CommissionStatus.Paid).Sum(c => c.Amount),
            TotalPending = commissions.Where(c => c.Status == CommissionStatus.Pending).Sum(c => c.Amount),
            TotalClawedBack = commissions.Where(c => c.Status == CommissionStatus.Clawback).Sum(c => c.Amount),
            DealCount = commissions.Count,
            AverageCommission = commissions.Count > 0 ? commissions.Average(c => c.Amount) : 0,
            Commissions = commissions.ToList()
        };
    }

    public async Task<Commission> RecalculateAsync(int commissionId, CancellationToken cancellationToken = default)
    {
        var commission = await GetByIdAsync(commissionId, cancellationToken);
        if (commission == null)
        {
            throw new InvalidOperationException(string.Format(CommissionNotFoundMessage, commissionId));
        }

        if (commission.OpportunityId.HasValue)
        {
            var calc = await CalculateForDealAsync(commission.OpportunityId.Value, cancellationToken);
            commission.Amount = calc.FinalAmount;
        }
        else if (commission.OrderId.HasValue)
        {
            var calc = await CalculateForOrderAsync(commission.OrderId.Value, cancellationToken);
            commission.Amount = calc.FinalAmount;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Recalculated commission {CommissionId}", commissionId);
        return commission;
    }

    #endregion

    #region Commission Plans

    public async Task<IEnumerable<CommissionPlan>> GetPlansAsync(bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var query = _context.CommissionPlans.Where(p => !p.IsDeleted);

        if (isActive.HasValue)
        {
            query = query.Where(p => p.Status == (isActive.Value ? CommissionPlanStatus.Active : CommissionPlanStatus.Inactive));
        }

        return await query.OrderBy(p => p.Name).ToListAsync(cancellationToken);
    }

    public async Task<CommissionPlan?> GetPlanByIdAsync(int planId, CancellationToken cancellationToken = default)
    {
        return await _context.CommissionPlans
            .Include(p => p.Tiers)
            .FirstOrDefaultAsync(p => p.Id == planId && !p.IsDeleted, cancellationToken);
    }

    public async Task<CommissionPlan> CreatePlanAsync(CommissionPlan plan, CancellationToken cancellationToken = default)
    {
        plan.CreatedAt = DateTime.UtcNow;

        _context.CommissionPlans.Add(plan);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created commission plan {PlanName} with ID {PlanId}", plan.Name, plan.Id);
        return plan;
    }

    public async Task<CommissionPlan> UpdatePlanAsync(CommissionPlan plan, CancellationToken cancellationToken = default)
    {
        var exists = await _context.CommissionPlans
            .AsNoTracking()
            .AnyAsync(p => p.Id == plan.Id && !p.IsDeleted, cancellationToken);
        if (!exists)
        {
            throw new InvalidOperationException(string.Format(CommissionPlanNotFoundMessage, plan.Id));
        }

        _context.CommissionPlans.Update(plan);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated commission plan {PlanId}", plan.Id);
        return plan;
    }

    public async Task<bool> DeletePlanAsync(int planId, CancellationToken cancellationToken = default)
    {
        var plan = await _context.CommissionPlans.FindAsync(new object[] { planId }, cancellationToken);
        if (plan == null)
        {
            return false;
        }

        plan.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted commission plan {PlanId}", planId);
        return true;
    }

    public async Task<bool> AssignPlanToUserAsync(int planId, int userId, DateTime? effectiveDate = null, CancellationToken cancellationToken = default)
    {
        var plan = await GetPlanByIdAsync(planId, cancellationToken);
        if (plan == null)
        {
            throw new InvalidOperationException(string.Format(CommissionPlanNotFoundMessage, planId));
        }

        var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException($"User {userId} not found");
        }

        // Soft-delete previous assignments for this user
        var previousAssignments = await _context.CommissionPlanAssignments
            .Where(a => a.UserId == userId && !a.IsDeleted)
            .ToListAsync(cancellationToken);
        foreach (var assignment in previousAssignments)
        {
            assignment.IsDeleted = true;
        }

        // Create new assignment
        var assignmentEntity = new CommissionPlanAssignment
        {
            UserId = userId,
            CommissionPlanId = planId,
            StartDate = effectiveDate ?? DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            IsDeleted = false
        };
        _context.CommissionPlanAssignments.Add(assignmentEntity);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Assigned plan {PlanId} to user {UserId} effective {EffectiveDate}",
            planId, userId, effectiveDate ?? DateTime.UtcNow);

        return true;
    }

    public async Task<CommissionPlan?> GetUserPlanAsync(int userId, CancellationToken cancellationToken = default)
    {
        // Would need a CommissionPlanAssignment table to properly implement
        // For now, return the first active plan
        return await _context.CommissionPlans
            .FirstOrDefaultAsync(p => p.Status == CommissionPlanStatus.Active && !p.IsDeleted, cancellationToken);
    }

    #endregion

    #region Status Management

    public async Task<Commission> UpdateStatusAsync(int commissionId, CommissionStatus status, CancellationToken cancellationToken = default)
    {
        var commission = await GetByIdAsync(commissionId, cancellationToken);
        if (commission == null)
        {
            throw new InvalidOperationException(string.Format(CommissionNotFoundMessage, commissionId));
        }

        commission.Status = status;

        _context.Commissions.Update(commission);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated commission {CommissionId} status to {Status}", commissionId, status);
        return commission;
    }

    public async Task<Commission> ApproveAsync(int commissionId, int approvedById, CancellationToken cancellationToken = default)
    {
        var commission = await GetByIdAsync(commissionId, cancellationToken);
        if (commission == null)
        {
            throw new InvalidOperationException(string.Format(CommissionNotFoundMessage, commissionId));
        }

        commission.Status = CommissionStatus.Approved;
        commission.ApprovedById = approvedById;
        commission.ApprovedAt = DateTime.UtcNow;

        _context.Commissions.Update(commission);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Approved commission {CommissionId} by user {ApprovedById}", commissionId, approvedById);
        return commission;
    }

    public async Task<Commission> RejectAsync(int commissionId, string reason, CancellationToken cancellationToken = default)
    {
        var commission = await GetByIdAsync(commissionId, cancellationToken);
        if (commission == null)
        {
            throw new InvalidOperationException(string.Format(CommissionNotFoundMessage, commissionId));
        }

        commission.Status = CommissionStatus.Cancelled;
        commission.Notes = string.IsNullOrEmpty(commission.Notes)
            ? $"Rejected: {reason}"
            : $"{commission.Notes}; Rejected: {reason}";

        _context.Commissions.Update(commission);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Rejected commission {CommissionId}: {Reason}", commissionId, reason);
        return commission;
    }

    public async Task<Commission> MarkAsPaidAsync(int commissionId, DateTime? paidDate = null, CancellationToken cancellationToken = default)
    {
        var commission = await GetByIdAsync(commissionId, cancellationToken);
        if (commission == null)
        {
            throw new InvalidOperationException(string.Format(CommissionNotFoundMessage, commissionId));
        }

        commission.Status = CommissionStatus.Paid;
        commission.PaidAt = paidDate ?? DateTime.UtcNow;

        _context.Commissions.Update(commission);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Marked commission {CommissionId} as paid on {PaidDate}", commissionId, commission.PaidAt);
        return commission;
    }

    public async Task<Commission> ClawbackAsync(int commissionId, string reason, CancellationToken cancellationToken = default)
    {
        var commission = await GetByIdAsync(commissionId, cancellationToken);
        if (commission == null)
        {
            throw new InvalidOperationException(string.Format(CommissionNotFoundMessage, commissionId));
        }

        commission.Status = CommissionStatus.Clawback;
        commission.Notes = string.IsNullOrEmpty(commission.Notes)
            ? $"Clawback: {reason}"
            : $"{commission.Notes}; Clawback: {reason}";

        _context.Commissions.Update(commission);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Clawback commission {CommissionId}: {Reason}", commissionId, reason);
        return commission;
    }

    #endregion

    #region Statements

    public async Task<CommissionStatement> GenerateStatementAsync(int userId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        var summary = await CalculateForPeriodAsync(userId, fromDate, toDate, cancellationToken);
        var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);

        var statement = new CommissionStatement
        {
            UserId = userId,
            PeriodStart = fromDate,
            PeriodEnd = toDate,
            TotalCommissions = summary.TotalEarned,
            TotalDeductions = summary.TotalClawedBack,
            NetAmount = summary.TotalEarned - summary.TotalClawedBack,
            Status = CommissionStatementStatus.Draft,
            GeneratedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.CommissionStatements.Add(statement);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Generated commission statement {StatementId} for user {UserId}", statement.Id, userId);
        return statement;
    }

    public async Task<IEnumerable<CommissionStatement>> GetStatementsAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.CommissionStatements
            .Where(s => s.UserId == userId && !s.IsDeleted)
            .OrderByDescending(s => s.PeriodEnd)
            .ToListAsync(cancellationToken);
    }

    public async Task<CommissionStatement?> GetStatementByIdAsync(int statementId, CancellationToken cancellationToken = default)
    {
        return await _context.CommissionStatements
            .FirstOrDefaultAsync(s => s.Id == statementId && !s.IsDeleted, cancellationToken);
    }

    public async Task<CommissionStatement> FinalizeStatementAsync(int statementId, CancellationToken cancellationToken = default)
    {
        var statement = await GetStatementByIdAsync(statementId, cancellationToken);
        if (statement == null)
        {
            throw new InvalidOperationException($"Commission statement {statementId} not found");
        }

        statement.Status = CommissionStatementStatus.Finalized;
        statement.FinalizedAt = DateTime.UtcNow;

        _context.CommissionStatements.Update(statement);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Finalized commission statement {StatementId}", statementId);
        return statement;
    }

    #endregion

    #region Queries

    public async Task<IEnumerable<Commission>> GetByUserAsync(int userId, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Commissions
            .Include(c => c.Opportunity)
            .Where(c => c.UserId == userId && !c.IsDeleted);

        if (fromDate.HasValue)
        {
            query = query.Where(c => c.CreatedAt >= fromDate.Value);
        }
        if (toDate.HasValue)
        {
            query = query.Where(c => c.CreatedAt <= toDate.Value);
        }

        return await query.OrderByDescending(c => c.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Commission>> GetPendingApprovalsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Commissions
            .Include(c => c.User)
            .Where(c => c.Status == CommissionStatus.Pending && !c.IsDeleted)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Commission>> GetReadyForPayoutAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Commissions
            .Include(c => c.User)
            .Where(c => c.Status == CommissionStatus.Approved && !c.IsDeleted)
            .OrderBy(c => c.UserId)
            .ThenBy(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<CommissionStatistics> GetStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Commissions.Where(c => !c.IsDeleted);

        if (fromDate.HasValue)
        {
            query = query.Where(c => c.CreatedAt >= fromDate.Value);
        }
        if (toDate.HasValue)
        {
            query = query.Where(c => c.CreatedAt <= toDate.Value);
        }

        var commissions = await query.Include(c => c.CommissionPlan).ToListAsync(cancellationToken);
        var activePlans = await _context.CommissionPlans.CountAsync(p => p.Status == CommissionPlanStatus.Active && !p.IsDeleted, cancellationToken);

        return new CommissionStatistics
        {
            TotalCommissions = commissions.Sum(c => c.Amount),
            TotalPaid = commissions.Where(c => c.Status == CommissionStatus.Paid).Sum(c => c.Amount),
            TotalPending = commissions.Where(c => c.Status == CommissionStatus.Pending).Sum(c => c.Amount),
            TotalRecords = commissions.Count,
            PendingApprovals = commissions.Count(c => c.Status == CommissionStatus.Pending),
            AverageCommission = commissions.Count > 0 ? commissions.Average(c => c.Amount) : 0,
            ActivePlans = activePlans,
            CommissionsByPlan = commissions
                .GroupBy(c => c.Plan?.Name ?? "No Plan")
                .ToDictionary(g => g.Key, g => g.Sum(c => c.Amount))
        };
    }

    public async Task<IEnumerable<CommissionLeaderboard>> GetLeaderboardAsync(int topN = 10, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Commissions
            .Include(c => c.User)
            .Where(c => !c.IsDeleted);

        if (fromDate.HasValue)
        {
            query = query.Where(c => c.CreatedAt >= fromDate.Value);
        }
        if (toDate.HasValue)
        {
            query = query.Where(c => c.CreatedAt <= toDate.Value);
        }

        var commissions = await query.ToListAsync(cancellationToken);

        var leaderboard = commissions
            .GroupBy(c => c.UserId)
            .Select(g =>
            {
                var first = g.First();
                var user = first.User;
                return new CommissionLeaderboard
                {
                    UserId = g.Key,
                    UserName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown",
                    TotalEarned = g.Sum(c => c.Amount),
                    DealCount = g.Count(),
                    AverageDealSize = g.Average(c => c.Amount)
                };
            })
            .OrderByDescending(l => l.TotalEarned)
            .Take(topN)
            .ToList();

        for (int i = 0; i < leaderboard.Count; i++)
        {
            leaderboard[i].Rank = i + 1;
        }

        return leaderboard;
    }

    public async Task<CommissionForecast> GetForecastAsync(int userId, DateTime? asOfDate = null, CancellationToken cancellationToken = default)
    {
        var date = asOfDate ?? DateTime.UtcNow;
        var monthStart = new DateTime(date.Year, date.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);

        var currentCommissions = await _context.Commissions
            .Where(c => c.UserId == userId && !c.IsDeleted && c.CreatedAt >= monthStart && c.CreatedAt <= date)
            .SumAsync(c => c.CommissionAmount, cancellationToken);

        var pipeline = await _context.Opportunities
            .Where(o => o.SalesOwnerId == userId && !o.IsDeleted)
            .Where(o => o.Stage != OpportunityStage.ClosedWon && o.Stage != OpportunityStage.ClosedLost)
            .ToListAsync(cancellationToken);

        var plan = await GetUserPlanAsync(userId, cancellationToken);
        var commissionRate = plan?.BaseRate ?? 0.05m;

        var forecastedDeals = pipeline.Select(o => new ForecastedDeal
        {
            OpportunityId = o.Id,
            OpportunityName = o.Name,
            DealValue = o.Amount,
            ExpectedCommission = o.Amount * commissionRate * ((decimal)o.Probability / 100),
            Probability = o.Probability,
            ExpectedCloseDate = o.ExpectedCloseDate
        }).ToList();

        var expectedFromPipeline = forecastedDeals.Sum(d => d.ExpectedCommission);

        return new CommissionForecast
        {
            UserId = userId,
            CurrentEarned = currentCommissions,
            ForecastedEarnings = currentCommissions + expectedFromPipeline,
            PipelineValue = pipeline.Sum(o => o.Amount),
            ExpectedFromPipeline = expectedFromPipeline,
            QuotaProgress = 0, // Would need quota data
            ProjectedQuotaAttainment = 0,
            ForecastedDeals = forecastedDeals
        };
    }

    #endregion

    #region Tiers

    public async Task<IEnumerable<CommissionTier>> GetTiersAsync(int planId, CancellationToken cancellationToken = default)
    {
        return await _context.CommissionTiers
            .Where(t => t.CommissionPlanId == planId && !t.IsDeleted)
            .OrderBy(t => t.MinValue)
            .ToListAsync(cancellationToken);
    }

    public async Task<CommissionTier> AddTierAsync(int planId, CommissionTier tier, CancellationToken cancellationToken = default)
    {
        var plan = await GetPlanByIdAsync(planId, cancellationToken);
        if (plan == null)
        {
            throw new InvalidOperationException(string.Format(CommissionPlanNotFoundMessage, planId));
        }

        tier.CommissionPlanId = planId;
        tier.CreatedAt = DateTime.UtcNow;

        _context.CommissionTiers.Add(tier);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Added tier to plan {PlanId}", planId);
        return tier;
    }

    public async Task<CommissionTier> UpdateTierAsync(CommissionTier tier, CancellationToken cancellationToken = default)
    {
        var exists = await _context.CommissionTiers
            .AsNoTracking()
            .AnyAsync(t => t.Id == tier.Id && !t.IsDeleted, cancellationToken);
        if (!exists)
        {
            throw new InvalidOperationException($"Commission tier {tier.Id} not found");
        }

        _context.CommissionTiers.Update(tier);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated commission tier {TierId}", tier.Id);
        return tier;
    }

    public async Task<bool> RemoveTierAsync(int tierId, CancellationToken cancellationToken = default)
    {
        var tier = await _context.CommissionTiers.FindAsync(new object[] { tierId }, cancellationToken);
        if (tier == null)
        {
            return false;
        }

        tier.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Removed commission tier {TierId}", tierId);
        return true;
    }

    #endregion
}
