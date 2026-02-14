// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for dashboard data and statistics
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<DashboardService> _logger;
    private readonly HybridCache _cache;

    private static readonly HybridCacheEntryOptions StatsCacheOptions = new()
    {
        Expiration = TimeSpan.FromSeconds(30),
        LocalCacheExpiration = TimeSpan.FromSeconds(10)
    };

    private static readonly HybridCacheEntryOptions PipelineCacheOptions = new()
    {
        Expiration = TimeSpan.FromSeconds(30),
        LocalCacheExpiration = TimeSpan.FromSeconds(10)
    };

    private static readonly HybridCacheEntryOptions ActivitiesCacheOptions = new()
    {
        Expiration = TimeSpan.FromSeconds(15),
        LocalCacheExpiration = TimeSpan.FromSeconds(5)
    };

    public DashboardService(ICrmDbContext dbContext, ILogger<DashboardService> logger, HybridCache cache)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    /// <inheritdoc />
    public async Task<DashboardStats> GetStatsAsync()
    {
        _logger.LogDebug("Fetching dashboard statistics");

        try
        {
            return await _cache.GetOrCreateAsync(
                "dashboard:stats",
                async ct =>
                {
                    // Get customer/account count
                    var customerCount = await _dbContext.Accounts.CountAsync(a => !a.IsDeleted, ct);

                    // Get contact count - Contact model uses Status enum, not IsDeleted
                    var contactCount = await _dbContext.Contacts.CountAsync(c => c.Status == CRM.Core.Models.ContactStatus.Active, ct);

                    // Get opportunity stats
                    var opportunities = await _dbContext.Opportunities
                        .Where(o => !o.IsDeleted)
                        .ToListAsync(ct);

                    var wonOpportunities = opportunities.Where(o => o.Stage == OpportunityStage.ClosedWon);
                    var openOpportunities = opportunities.Where(o =>
                        o.Stage != OpportunityStage.ClosedWon && o.Stage != OpportunityStage.ClosedLost);

                    // Get product count
                    var productCount = await _dbContext.Products.CountAsync(p => !p.IsDeleted, ct);

                    // Get task stats
                    var tasks = await _dbContext.CrmTasks
                        .Where(t => !t.IsDeleted)
                        .ToListAsync(ct);

                    var pendingTasks = tasks.Count(t => t.Status != CrmTaskStatus.Completed && t.Status != CrmTaskStatus.Cancelled);

                    // Get active user count
                    var activeUsers = await _dbContext.Users
                        .Where(u => !u.IsDeleted && u.IsActive)
                        .CountAsync(ct);

                    return new DashboardStats
                    {
                        Customers = new EntityCount { Total = customerCount },
                        Contacts = new EntityCount { Total = contactCount },
                        Opportunities = new OpportunityStats
                        {
                            Total = opportunities.Count,
                            OpenValue = openOpportunities.Sum(o => o.Amount),
                            WonValue = wonOpportunities.Sum(o => o.Amount)
                        },
                        Products = new EntityCount { Total = productCount },
                        Tasks = new TaskStats
                        {
                            Total = tasks.Count,
                            Pending = pendingTasks
                        },
                        Users = new UserStats
                        {
                            Active = activeUsers
                        },
                        Timestamp = DateTime.UtcNow
                    };
                },
                StatsCacheOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching dashboard statistics");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<PipelineSummary> GetPipelineSummaryAsync()
    {
        _logger.LogDebug("Fetching pipeline summary");

        try
        {
            return await _cache.GetOrCreateAsync(
                "dashboard:pipeline",
                async ct =>
                {
                    var stages = await _dbContext.Opportunities
                        .Where(o => !o.IsDeleted)
                        .GroupBy(o => o.Stage)
                        .Select(g => new PipelineStageData
                        {
                            Stage = g.Key.ToString(),
                            StageValue = (int)g.Key,
                            Count = g.Count(),
                            TotalValue = g.Sum(o => o.Amount),
                            WeightedValue = g.Sum(o => o.Amount * (o.Probability / 100m))
                        })
                        .ToListAsync(ct);

                    var summary = new PipelineSummaryData
                    {
                        TotalValue = stages.Sum(s => s.TotalValue),
                        WeightedValue = stages.Sum(s => s.WeightedValue),
                        OpportunityCount = stages.Sum(s => s.Count)
                    };

                    return new PipelineSummary
                    {
                        Stages = stages.OrderBy(s => s.StageValue),
                        Summary = summary
                    };
                },
                PipelineCacheOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching pipeline summary");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DashboardActivity>> GetRecentActivitiesAsync(int count = 10)
    {
        _logger.LogDebug("Fetching {Count} recent activities", count);

        try
        {
            return await _cache.GetOrCreateAsync(
                $"dashboard:activities:{count}",
                async ct =>
                {
                    var activities = new List<DashboardActivity>();

                    // Get recent accounts
                    var recentAccounts = await _dbContext.Accounts
                        .Where(a => !a.IsDeleted)
                        .OrderByDescending(a => a.CreatedAt)
                        .Take(count / 3)
                        .Select(a => new DashboardActivity
                        {
                            Id = a.Id,
                            Type = "AccountCreated",
                            Title = $"New account: {a.Company ?? a.Email}",
                            ActivityDate = a.CreatedAt,
                            Description = $"Account created for {a.Company ?? "individual"}",
                            EntityType = "Account",
                            EntityId = a.Id
                        })
                        .ToListAsync(ct);
                    activities.AddRange(recentAccounts);

                    // Get recent opportunities
                    var recentOpportunities = await _dbContext.Opportunities
                        .Where(o => !o.IsDeleted)
                        .OrderByDescending(o => o.CreatedAt)
                        .Take(count / 3)
                        .Select(o => new DashboardActivity
                        {
                            Id = o.Id,
                            Type = "OpportunityCreated",
                            Title = $"New opportunity: {o.Name}",
                            ActivityDate = o.CreatedAt,
                            Description = $"Value: {o.Amount:C}",
                            EntityType = "Opportunity",
                            EntityId = o.Id
                        })
                        .ToListAsync(ct);
                    activities.AddRange(recentOpportunities);

                    // Get recent leads
                    var recentLeads = await _dbContext.Leads
                        .Where(l => !l.IsDeleted)
                        .OrderByDescending(l => l.CreatedAt)
                        .Take(count / 3)
                        .Select(l => new DashboardActivity
                        {
                            Id = l.Id,
                            Type = "LeadCreated",
                            Title = $"New lead: {l.FirstName} {l.LastName}",
                            ActivityDate = l.CreatedAt,
                            Description = l.CompanyName != null ? $"Company: {l.CompanyName}" : null,
                            EntityType = "Lead",
                            EntityId = l.Id
                        })
                        .ToListAsync(ct);
                    activities.AddRange(recentLeads);

                    // Get recent activities from Activities table if available
                    if (_dbContext.Activities != null)
                    {
                        var recentActivityRecords = await _dbContext.Activities
                            .Where(a => !a.IsDeleted)
                            .OrderByDescending(a => a.ActivityDate)
                            .Take(count / 3)
                            .Select(a => new DashboardActivity
                            {
                                Id = a.Id,
                                Type = a.ActivityType.ToString(),
                                Title = a.Title,
                                ActivityDate = a.ActivityDate,
                                Description = a.Description,
                                EntityType = a.EntityType,
                                EntityId = a.EntityId
                            })
                            .ToListAsync(ct);
                        activities.AddRange(recentActivityRecords);
                    }

                    return activities
                        .OrderByDescending(a => a.ActivityDate)
                        .Take(count)
                        .ToList();
                },
                ActivitiesCacheOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching recent activities");
            throw;
        }
    }
}
