// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under the GNU Affero General Public License v3.0

using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.Api.Controllers;

/// <summary>
/// Dashboard Controller for analytics and statistics
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly CrmDbContext _context;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(CrmDbContext context, ILogger<DashboardController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get dashboard statistics
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        try
        {
            var customerCount = await _context.Customers.CountAsync(c => !c.IsDeleted);
            var contactCount = await _context.Contacts.CountAsync();
            var opportunityCount = await _context.Opportunities.CountAsync(o => !o.IsDeleted);
            var openOpportunityValue = await _context.Opportunities
                .Where(o => !o.IsDeleted && o.Stage != OpportunityStage.ClosedWon && o.Stage != OpportunityStage.ClosedLost)
                .SumAsync(o => o.Amount);
            var wonOpportunityValue = await _context.Opportunities
                .Where(o => !o.IsDeleted && o.Stage == OpportunityStage.ClosedWon)
                .SumAsync(o => o.Amount);
            var productCount = await _context.Products.CountAsync(p => !p.IsDeleted);
            var taskCount = await _context.CrmTasks.CountAsync(t => !t.IsDeleted);
            var notStartedTaskCount = await _context.CrmTasks.CountAsync(t => !t.IsDeleted && t.Status == CrmTaskStatus.NotStarted);
            var activeUserCount = await _context.Users.CountAsync(u => !u.IsDeleted && u.IsActive);

            return Ok(new
            {
                customers = new { total = customerCount },
                contacts = new { total = contactCount },
                opportunities = new
                {
                    total = opportunityCount,
                    openValue = openOpportunityValue,
                    wonValue = wonOpportunityValue
                },
                products = new { total = productCount },
                tasks = new
                {
                    total = taskCount,
                    pending = notStartedTaskCount
                },
                users = new { active = activeUserCount },
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard stats");
            return StatusCode(500, new { message = "An error occurred while retrieving dashboard statistics" });
        }
    }

    /// <summary>
    /// Get pipeline summary statistics
    /// </summary>
    [HttpGet("pipeline")]
    public async Task<IActionResult> GetPipelineSummary()
    {
        try
        {
            var pipelineData = await _context.Opportunities
                .Where(o => !o.IsDeleted && o.Stage != OpportunityStage.ClosedWon && o.Stage != OpportunityStage.ClosedLost)
                .GroupBy(o => o.Stage)
                .Select(g => new
                {
                    Stage = g.Key.ToString(),
                    StageValue = (int)g.Key,
                    Count = g.Count(),
                    TotalValue = g.Sum(o => o.Amount),
                    WeightedValue = g.Sum(o => o.Amount * ((decimal)o.Probability / 100m))
                })
                .OrderBy(p => p.StageValue)
                .ToListAsync();

            var totalPipeline = pipelineData.Sum(p => p.TotalValue);
            var weightedPipeline = pipelineData.Sum(p => p.WeightedValue);

            return Ok(new
            {
                stages = pipelineData,
                summary = new
                {
                    totalValue = totalPipeline,
                    weightedValue = weightedPipeline,
                    opportunityCount = pipelineData.Sum(p => p.Count)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pipeline summary");
            return StatusCode(500, new { message = "An error occurred while retrieving pipeline data" });
        }
    }

    /// <summary>
    /// Get recent activities
    /// </summary>
    [HttpGet("activities")]
    public async Task<IActionResult> GetRecentActivities([FromQuery] int count = 10)
    {
        try
        {
            var activities = await _context.Activities
                .Where(a => !a.IsDeleted)
                .OrderByDescending(a => a.ActivityDate)
                .Take(count)
                .Select(a => new
                {
                    a.Id,
                    Type = a.ActivityType.ToString(),
                    a.Title,
                    a.ActivityDate,
                    a.Description,
                    a.EntityType,
                    a.EntityId
                })
                .ToListAsync();

            return Ok(activities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recent activities");
            return StatusCode(500, new { message = "An error occurred while retrieving activities" });
        }
    }
}
