// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for cohort analysis and customer segmentation.
/// Supports RFM analysis, behavioral cohorts, and custom segmentation.
/// TODO-RPT-07
/// </summary>
public class CohortAnalysisService : ICohortAnalysisService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<CohortAnalysisService> _logger;

    public CohortAnalysisService(
        ICrmDbContext context,
        ILogger<CohortAnalysisService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<CohortAnalysisResult> AnalyzeCohortAsync(
        CohortCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting cohort analysis with type: {Type}", criteria.Type);

        var result = new CohortAnalysisResult
        {
            AnalyzedAt = DateTime.UtcNow,
            CohortType = criteria.Type
        };

        try
        {
            result.Cohorts = criteria.Type switch
            {
                CohortType.Acquisition => await AnalyzeAcquisitionCohortsAsync(criteria, cancellationToken),
                CohortType.Revenue => await AnalyzeRevenueCohortsAsync(criteria, cancellationToken),
                CohortType.Activity => await AnalyzeActivityCohortsAsync(criteria, cancellationToken),
                CohortType.Industry => await AnalyzeIndustryCohortsAsync(criteria, cancellationToken),
                CohortType.Geography => await AnalyzeGeographyCohortsAsync(criteria, cancellationToken),
                _ => await AnalyzeAcquisitionCohortsAsync(criteria, cancellationToken)
            };

            result.TotalCustomers = result.Cohorts.Sum(c => c.CustomerCount);
            result.Success = true;

            _logger.LogInformation(
                "Cohort analysis completed. Type: {Type}, Cohorts: {Count}, Customers: {Total}",
                criteria.Type, result.Cohorts.Count, result.TotalCustomers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during cohort analysis");
            result.Success = false;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    private async Task<List<CohortGroup>> AnalyzeAcquisitionCohortsAsync(
        CohortCriteria criteria,
        CancellationToken cancellationToken)
    {
        var startDate = criteria.StartDate ?? DateTime.UtcNow.AddYears(-1);
        var endDate = criteria.EndDate ?? DateTime.UtcNow;

        // Group customers by acquisition month
        var customers = await _context.Customers
            .Where(c => !c.IsDeleted && c.CreatedAt >= startDate && c.CreatedAt <= endDate)
            .Select(c => new { c.Id, c.CreatedAt })
            .ToListAsync(cancellationToken);

        return customers
            .GroupBy(c => new DateTime(c.CreatedAt.Year, c.CreatedAt.Month, 1))
            .Select(g => new CohortGroup
            {
                Name = g.Key.ToString("yyyy-MM"),
                CohortDate = g.Key,
                CustomerCount = g.Count(),
                CustomerIds = g.Select(c => c.Id).ToList()
            })
            .OrderBy(c => c.CohortDate)
            .ToList();
    }

    private async Task<List<CohortGroup>> AnalyzeRevenueCohortsAsync(
        CohortCriteria criteria,
        CancellationToken cancellationToken)
    {
        // Segment customers by total revenue
        var customerRevenue = await _context.Opportunities
            .Where(o => !o.IsDeleted && o.Stage == "Closed Won" && o.AccountId.HasValue)
            .GroupBy(o => o.AccountId!.Value)
            .Select(g => new { CustomerId = g.Key, TotalRevenue = g.Sum(o => o.Amount) })
            .ToListAsync(cancellationToken);

        var cohorts = new List<CohortGroup>();
        var thresholds = criteria.RevenueThresholds ?? new[] { 0m, 10000m, 50000m, 100000m, 500000m };

        for (int i = 0; i < thresholds.Length; i++)
        {
            var min = thresholds[i];
            var max = i < thresholds.Length - 1 ? thresholds[i + 1] : decimal.MaxValue;

            var inRange = customerRevenue.Where(c => c.TotalRevenue >= min && c.TotalRevenue < max).ToList();

            cohorts.Add(new CohortGroup
            {
                Name = max == decimal.MaxValue ? $"${min:N0}+" : $"${min:N0} - ${max:N0}",
                CustomerCount = inRange.Count,
                CustomerIds = inRange.Select(c => c.CustomerId).ToList(),
                Metrics = new Dictionary<string, decimal>
                {
                    ["totalRevenue"] = inRange.Sum(c => c.TotalRevenue),
                    ["avgRevenue"] = inRange.Count > 0 ? inRange.Average(c => c.TotalRevenue) : 0
                }
            });
        }

        return cohorts;
    }

    private async Task<List<CohortGroup>> AnalyzeActivityCohortsAsync(
        CohortCriteria criteria,
        CancellationToken cancellationToken)
    {
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var ninetyDaysAgo = DateTime.UtcNow.AddDays(-90);
        var oneYearAgo = DateTime.UtcNow.AddDays(-365);

        // Get last activity date for each customer
        var customerActivity = await _context.Activities
            .Where(a => !a.IsDeleted)
            .GroupBy(a => a.EntityId)
            .Select(g => new { CustomerId = g.Key, LastActivity = g.Max(a => a.ActivityDate) })
            .ToListAsync(cancellationToken);

        var cohorts = new List<CohortGroup>
        {
            new()
            {
                Name = "Active (Last 30 days)",
                CustomerCount = customerActivity.Count(c => c.LastActivity >= thirtyDaysAgo),
                CustomerIds = customerActivity.Where(c => c.LastActivity >= thirtyDaysAgo).Select(c => c.CustomerId).ToList()
            },
            new()
            {
                Name = "At Risk (30-90 days)",
                CustomerCount = customerActivity.Count(c => c.LastActivity >= ninetyDaysAgo && c.LastActivity < thirtyDaysAgo),
                CustomerIds = customerActivity.Where(c => c.LastActivity >= ninetyDaysAgo && c.LastActivity < thirtyDaysAgo).Select(c => c.CustomerId).ToList()
            },
            new()
            {
                Name = "Dormant (90-365 days)",
                CustomerCount = customerActivity.Count(c => c.LastActivity >= oneYearAgo && c.LastActivity < ninetyDaysAgo),
                CustomerIds = customerActivity.Where(c => c.LastActivity >= oneYearAgo && c.LastActivity < ninetyDaysAgo).Select(c => c.CustomerId).ToList()
            },
            new()
            {
                Name = "Churned (>365 days)",
                CustomerCount = customerActivity.Count(c => c.LastActivity < oneYearAgo),
                CustomerIds = customerActivity.Where(c => c.LastActivity < oneYearAgo).Select(c => c.CustomerId).ToList()
            }
        };

        return cohorts;
    }

    private async Task<List<CohortGroup>> AnalyzeIndustryCohortsAsync(
        CohortCriteria criteria,
        CancellationToken cancellationToken)
    {
        var customers = await _context.Customers
            .Where(c => !c.IsDeleted)
            .GroupBy(c => c.Industry ?? "Unknown")
            .Select(g => new { Industry = g.Key, CustomerIds = g.Select(c => c.Id).ToList() })
            .ToListAsync(cancellationToken);

        return customers
            .Select(g => new CohortGroup
            {
                Name = g.Industry,
                CustomerCount = g.CustomerIds.Count,
                CustomerIds = g.CustomerIds
            })
            .OrderByDescending(c => c.CustomerCount)
            .ToList();
    }

    private async Task<List<CohortGroup>> AnalyzeGeographyCohortsAsync(
        CohortCriteria criteria,
        CancellationToken cancellationToken)
    {
        var customers = await _context.Customers
            .Where(c => !c.IsDeleted)
            .GroupBy(c => c.BillingCountry ?? "Unknown")
            .Select(g => new { Country = g.Key, CustomerIds = g.Select(c => c.Id).ToList() })
            .ToListAsync(cancellationToken);

        return customers
            .Select(g => new CohortGroup
            {
                Name = g.Country,
                CustomerCount = g.CustomerIds.Count,
                CustomerIds = g.CustomerIds
            })
            .OrderByDescending(c => c.CustomerCount)
            .ToList();
    }

    /// <inheritdoc />
    public async Task<RetentionAnalysisResult> AnalyzeRetentionAsync(
        DateTime startDate,
        DateTime endDate,
        string periodType = "monthly",
        CancellationToken cancellationToken = default)
    {
        var result = new RetentionAnalysisResult
        {
            AnalyzedAt = DateTime.UtcNow,
            StartDate = startDate,
            EndDate = endDate,
            PeriodType = periodType
        };

        try
        {
            // Get acquisition cohorts
            var customers = await _context.Customers
                .Where(c => !c.IsDeleted && c.CreatedAt >= startDate && c.CreatedAt <= endDate)
                .Select(c => new { c.Id, c.CreatedAt })
                .ToListAsync(cancellationToken);

            // Get all activities for these customers
            var customerIds = customers.Select(c => c.Id).ToList();
            var activities = await _context.Activities
                .Where(a => !a.IsDeleted && customerIds.Contains(a.EntityId))
                .Select(a => new { a.EntityId, a.ActivityDate })
                .ToListAsync(cancellationToken);

            // Group by acquisition period
            var cohorts = customers
                .GroupBy(c => new DateTime(c.CreatedAt.Year, c.CreatedAt.Month, 1))
                .OrderBy(g => g.Key)
                .ToList();

            foreach (var cohort in cohorts)
            {
                var cohortCustomers = cohort.Select(c => c.Id).ToHashSet();
                var cohortSize = cohortCustomers.Count;
                var cohortDate = cohort.Key;

                var matrix = new RetentionCohort
                {
                    CohortDate = cohortDate,
                    CohortName = cohortDate.ToString("yyyy-MM"),
                    InitialSize = cohortSize,
                    RetentionByPeriod = new List<RetentionPeriod>()
                };

                // Calculate retention for each subsequent period
                var currentPeriod = cohortDate.AddMonths(1);
                var periodIndex = 1;

                while (currentPeriod <= endDate)
                {
                    var nextPeriod = currentPeriod.AddMonths(1);
                    var activeInPeriod = activities
                        .Where(a => cohortCustomers.Contains(a.EntityId) && 
                                    a.ActivityDate >= currentPeriod && 
                                    a.ActivityDate < nextPeriod)
                        .Select(a => a.EntityId)
                        .Distinct()
                        .Count();

                    matrix.RetentionByPeriod.Add(new RetentionPeriod
                    {
                        PeriodIndex = periodIndex,
                        PeriodLabel = $"Month {periodIndex}",
                        RetainedCount = activeInPeriod,
                        RetentionRate = cohortSize > 0 ? (decimal)activeInPeriod / cohortSize * 100 : 0
                    });

                    currentPeriod = nextPeriod;
                    periodIndex++;
                }

                result.Cohorts.Add(matrix);
            }

            result.Success = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during retention analysis");
            result.Success = false;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }
}

/// <summary>
/// Types of cohort analysis.
/// </summary>
public enum CohortType
{
    Acquisition,
    Revenue,
    Activity,
    Industry,
    Geography,
    Custom
}

/// <summary>
/// Criteria for cohort analysis.
/// </summary>
public class CohortCriteria
{
    public CohortType Type { get; set; } = CohortType.Acquisition;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal[]? RevenueThresholds { get; set; }
    public Dictionary<string, object>? CustomFilters { get; set; }
}

/// <summary>
/// Result of cohort analysis.
/// </summary>
public class CohortAnalysisResult
{
    public DateTime AnalyzedAt { get; set; }
    public CohortType CohortType { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public List<CohortGroup> Cohorts { get; set; } = new();
    public int TotalCustomers { get; set; }
}

/// <summary>
/// A cohort group with its members.
/// </summary>
public class CohortGroup
{
    public string Name { get; set; } = string.Empty;
    public DateTime? CohortDate { get; set; }
    public int CustomerCount { get; set; }
    public List<int> CustomerIds { get; set; } = new();
    public Dictionary<string, decimal> Metrics { get; set; } = new();
}

/// <summary>
/// Result of retention analysis.
/// </summary>
public class RetentionAnalysisResult
{
    public DateTime AnalyzedAt { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string PeriodType { get; set; } = "monthly";
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public List<RetentionCohort> Cohorts { get; set; } = new();
}

/// <summary>
/// Retention data for a single cohort.
/// </summary>
public class RetentionCohort
{
    public DateTime CohortDate { get; set; }
    public string CohortName { get; set; } = string.Empty;
    public int InitialSize { get; set; }
    public List<RetentionPeriod> RetentionByPeriod { get; set; } = new();
}

/// <summary>
/// Retention metrics for a single period.
/// </summary>
public class RetentionPeriod
{
    public int PeriodIndex { get; set; }
    public string PeriodLabel { get; set; } = string.Empty;
    public int RetainedCount { get; set; }
    public decimal RetentionRate { get; set; }
}

/// <summary>
/// Interface for cohort analysis service.
/// </summary>
public interface ICohortAnalysisService
{
    /// <summary>
    /// Analyzes cohorts based on specified criteria.
    /// </summary>
    Task<CohortAnalysisResult> AnalyzeCohortAsync(CohortCriteria criteria, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs retention analysis across cohorts.
    /// </summary>
    Task<RetentionAnalysisResult> AnalyzeRetentionAsync(DateTime startDate, DateTime endDate, string periodType = "monthly", CancellationToken cancellationToken = default);
}
