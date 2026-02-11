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
using CRM.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.Api.Controllers;

/// <summary>
/// API endpoints for managing lead scoring rules (admin configuration)
/// </summary>
[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin,Manager")]
public class LeadScoreRulesController : ControllerBase
{
    private readonly CrmDbContext _context;
    private readonly ILogger<LeadScoreRulesController> _logger;

    public LeadScoreRulesController(CrmDbContext context, ILogger<LeadScoreRulesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all lead scoring rules
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LeadScoreRule>>> GetRules(
        [FromQuery] LeadScoreRuleType? ruleType = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? category = null)
    {
        var query = _context.LeadScoreRules.AsQueryable();

        if (ruleType.HasValue)
            query = query.Where(r => r.RuleType == ruleType);

        if (isActive.HasValue)
            query = query.Where(r => r.IsActive == isActive);

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(r => r.Category == category);

        var rules = await query
            .OrderBy(r => r.Priority)
            .ThenBy(r => r.Name)
            .ToListAsync();

        return Ok(rules);
    }

    /// <summary>
    /// Get a specific lead scoring rule by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<LeadScoreRule>> GetRule(int id)
    {
        var rule = await _context.LeadScoreRules.FindAsync(id);

        if (rule == null)
            return NotFound();

        return Ok(rule);
    }

    /// <summary>
    /// Create a new lead scoring rule
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<LeadScoreRule>> CreateRule([FromBody] LeadScoreRuleDto dto)
    {
        var rule = new LeadScoreRule
        {
            Name = dto.Name,
            Description = dto.Description,
            RuleType = dto.RuleType,
            FieldName = dto.FieldName,
            Operator = dto.Operator,
            Value = dto.Value,
            ConditionsJson = dto.ConditionsJson,
            ScoreImpact = dto.ScoreImpact,
            MaxApplications = dto.MaxApplications,
            DecayDaysThreshold = dto.DecayDaysThreshold,
            DecayPointsPerPeriod = dto.DecayPointsPerPeriod,
            DecayPeriodDays = dto.DecayPeriodDays,
            IsActive = dto.IsActive,
            Priority = dto.Priority,
            Category = dto.Category,
            ActionType = dto.ActionType,
            ActionIdentifier = dto.ActionIdentifier,
            CreatedAt = DateTime.UtcNow
        };

        _context.LeadScoreRules.Add(rule);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created lead score rule: {RuleName} (ID: {RuleId})", rule.Name, rule.Id);

        return CreatedAtAction(nameof(GetRule), new { id = rule.Id }, rule);
    }

    /// <summary>
    /// Update an existing lead scoring rule
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<LeadScoreRule>> UpdateRule(int id, [FromBody] LeadScoreRuleDto dto)
    {
        var rule = await _context.LeadScoreRules.FindAsync(id);

        if (rule == null)
            return NotFound();

        rule.Name = dto.Name;
        rule.Description = dto.Description;
        rule.RuleType = dto.RuleType;
        rule.FieldName = dto.FieldName;
        rule.Operator = dto.Operator;
        rule.Value = dto.Value;
        rule.ConditionsJson = dto.ConditionsJson;
        rule.ScoreImpact = dto.ScoreImpact;
        rule.MaxApplications = dto.MaxApplications;
        rule.DecayDaysThreshold = dto.DecayDaysThreshold;
        rule.DecayPointsPerPeriod = dto.DecayPointsPerPeriod;
        rule.DecayPeriodDays = dto.DecayPeriodDays;
        rule.IsActive = dto.IsActive;
        rule.Priority = dto.Priority;
        rule.Category = dto.Category;
        rule.ActionType = dto.ActionType;
        rule.ActionIdentifier = dto.ActionIdentifier;
        rule.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated lead score rule: {RuleName} (ID: {RuleId})", rule.Name, rule.Id);

        return Ok(rule);
    }

    /// <summary>
    /// Toggle active status of a rule
    /// </summary>
    [HttpPatch("{id}/toggle")]
    public async Task<ActionResult<LeadScoreRule>> ToggleRule(int id)
    {
        var rule = await _context.LeadScoreRules.FindAsync(id);

        if (rule == null)
            return NotFound();

        rule.IsActive = !rule.IsActive;
        rule.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Toggled lead score rule: {RuleName} (ID: {RuleId}) to {Status}",
            rule.Name, rule.Id, rule.IsActive ? "Active" : "Inactive");

        return Ok(rule);
    }

    /// <summary>
    /// Delete a lead scoring rule
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRule(int id)
    {
        var rule = await _context.LeadScoreRules.FindAsync(id);

        if (rule == null)
            return NotFound();

        _context.LeadScoreRules.Remove(rule);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted lead score rule: {RuleName} (ID: {RuleId})", rule.Name, id);

        return NoContent();
    }

    /// <summary>
    /// Reorder rules by updating their priorities
    /// </summary>
    [HttpPost("reorder")]
    public async Task<IActionResult> ReorderRules([FromBody] List<RulePriorityDto> priorities)
    {
        foreach (var item in priorities)
        {
            var rule = await _context.LeadScoreRules.FindAsync(item.Id);
            if (rule != null)
            {
                rule.Priority = item.Priority;
                rule.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Reordered {Count} lead score rules", priorities.Count);

        return Ok();
    }

    /// <summary>
    /// Get available field names for attribute-based rules
    /// </summary>
    [HttpGet("fields")]
    public ActionResult<IEnumerable<FieldDefinition>> GetAvailableFields()
    {
        var fields = new List<FieldDefinition>
        {
            new("JobTitle", "Job Title", "string"),
            new("Company", "Company Name", "string"),
            new("Industry", "Industry", "string"),
            new("CompanySize", "Company Size", "number"),
            new("AnnualRevenue", "Annual Revenue", "decimal"),
            new("Country", "Country", "string"),
            new("State", "State/Region", "string"),
            new("City", "City", "string"),
            new("LeadSource", "Lead Source", "string"),
            new("Website", "Website", "string"),
            new("Email", "Email Domain", "string"),
            new("Phone", "Has Phone", "boolean"),
            new("TotalInteractions", "Total Interactions", "number"),
            new("LastActivityDate", "Days Since Last Activity", "number"),
            new("EmailOpens", "Email Opens", "number"),
            new("EmailClicks", "Email Clicks", "number"),
            new("PageViews", "Page Views", "number"),
            new("FormSubmissions", "Form Submissions", "number")
        };

        return Ok(fields);
    }

    /// <summary>
    /// Get rule type definitions with descriptions
    /// </summary>
    [HttpGet("types")]
    public ActionResult<IEnumerable<object>> GetRuleTypes()
    {
        var types = new[]
        {
            new { Value = LeadScoreRuleType.Attribute, Name = "Attribute", Description = "Score based on lead attributes (job title, company, etc.)" },
            new { Value = LeadScoreRuleType.Behavior, Name = "Behavior", Description = "Score based on lead actions (email opens, page views)" },
            new { Value = LeadScoreRuleType.Decay, Name = "Decay", Description = "Reduce score for inactive leads over time" },
            new { Value = LeadScoreRuleType.Demographic, Name = "Demographic", Description = "Score based on demographic fit" },
            new { Value = LeadScoreRuleType.FitScore, Name = "Fit Score", Description = "Score based on ideal customer profile match" }
        };

        return Ok(types);
    }

    /// <summary>
    /// Get operator definitions
    /// </summary>
    [HttpGet("operators")]
    public ActionResult<IEnumerable<object>> GetOperators()
    {
        var operators = new[]
        {
            new { Value = RuleOperator.Equals, Name = "Equals", Symbol = "=" },
            new { Value = RuleOperator.NotEquals, Name = "Not Equals", Symbol = "!=" },
            new { Value = RuleOperator.Contains, Name = "Contains", Symbol = "contains" },
            new { Value = RuleOperator.NotContains, Name = "Does Not Contain", Symbol = "!contains" },
            new { Value = RuleOperator.GreaterThan, Name = "Greater Than", Symbol = ">" },
            new { Value = RuleOperator.LessThan, Name = "Less Than", Symbol = "<" },
            new { Value = RuleOperator.GreaterThanOrEquals, Name = "Greater Than or Equals", Symbol = ">=" },
            new { Value = RuleOperator.LessThanOrEquals, Name = "Less Than or Equals", Symbol = "<=" },
            new { Value = RuleOperator.IsEmpty, Name = "Is Empty", Symbol = "empty" },
            new { Value = RuleOperator.IsNotEmpty, Name = "Is Not Empty", Symbol = "!empty" },
            new { Value = RuleOperator.In, Name = "In List", Symbol = "in" },
            new { Value = RuleOperator.NotIn, Name = "Not In List", Symbol = "!in" }
        };

        return Ok(operators);
    }

    /// <summary>
    /// Get summary statistics for lead scoring rules
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult> GetStats()
    {
        var stats = new
        {
            TotalRules = await _context.LeadScoreRules.CountAsync(),
            ActiveRules = await _context.LeadScoreRules.CountAsync(r => r.IsActive),
            InactiveRules = await _context.LeadScoreRules.CountAsync(r => !r.IsActive),
            RulesByType = await _context.LeadScoreRules
                .GroupBy(r => r.RuleType)
                .Select(g => new { Type = g.Key.ToString(), Count = g.Count() })
                .ToListAsync(),
            RulesByCategory = await _context.LeadScoreRules
                .Where(r => r.Category != null)
                .GroupBy(r => r.Category)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToListAsync()
        };

        return Ok(stats);
    }
}

#region DTOs

/// <summary>
/// DTO for creating/updating lead scoring rules
/// </summary>
public class LeadScoreRuleDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public LeadScoreRuleType RuleType { get; set; }
    public string? FieldName { get; set; }
    public RuleOperator Operator { get; set; }
    public string? Value { get; set; }
    public string? ConditionsJson { get; set; }
    public int ScoreImpact { get; set; } = 10;
    public int? MaxApplications { get; set; }
    public int? DecayDaysThreshold { get; set; }
    public int? DecayPointsPerPeriod { get; set; }
    public int? DecayPeriodDays { get; set; }
    public bool IsActive { get; set; } = true;
    public int Priority { get; set; } = 100;
    public string? Category { get; set; }
    public string? ActionType { get; set; }
    public string? ActionIdentifier { get; set; }
}

/// <summary>
/// DTO for reordering rules
/// </summary>
public class RulePriorityDto
{
    public int Id { get; set; }
    public int Priority { get; set; }
}

/// <summary>
/// Field definition for available scoring fields
/// </summary>
public record FieldDefinition(string Name, string DisplayName, string DataType);

#endregion
