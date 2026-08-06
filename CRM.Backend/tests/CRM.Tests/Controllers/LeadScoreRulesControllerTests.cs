// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Api.Controllers;
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for LeadScoreRulesController (TCOV-044).
/// </summary>
public class LeadScoreRulesControllerTests : IDisposable
{
    private readonly CrmDbContext _dbContext;
    private readonly LeadScoreRulesController _controller;

    public LeadScoreRulesControllerTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"LeadScoreRulesTest_{Guid.NewGuid()}")
            .Options;
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        _dbContext = new CrmDbContext(options, config);

        var logger = new Mock<ILogger<LeadScoreRulesController>>();
        _controller = new LeadScoreRulesController(_dbContext, logger.Object);
    }

    public void Dispose() => _dbContext.Dispose();

    private async Task<LeadScoreRule> SeedRule(string name = "Rule1", bool isActive = true)
    {
        var rule = new LeadScoreRule
        {
            Name = name, RuleType = LeadScoreRuleType.Demographic,
            IsActive = isActive, ScoreImpact = 10, Priority = 1,
            FieldName = "JobTitle", Operator = RuleOperator.Equals, Value = "CEO",
            Category = "Title", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };
        _dbContext.LeadScoreRules.Add(rule);
        await _dbContext.SaveChangesAsync();
        return rule;
    }

    [Fact]
    public async Task GetRules_ShouldReturnOk_WithEmptyList()
    {
        var result = await _controller.GetRules();

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetRules_ShouldReturnOk_WithRules()
    {
        await SeedRule("Rule A");
        await SeedRule("Rule B");

        var result = await _controller.GetRules();

        result.Result.Should().BeOfType<OkObjectResult>();
        var ok = (OkObjectResult)result.Result!;
        ((IEnumerable<LeadScoreRule>)ok.Value!).Should().HaveCount(2);
    }

    [Fact]
    public async Task GetRule_ShouldReturnNotFound_WhenNotExists()
    {
        var result = await _controller.GetRule(999);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetRule_ShouldReturnOk_WhenExists()
    {
        var rule = await SeedRule("Rule X");

        var result = await _controller.GetRule(rule.Id);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetRules_ShouldFilterByIsActive()
    {
        await SeedRule("Active Rule", true);
        await SeedRule("Inactive Rule", false);

        var result = await _controller.GetRules(isActive: true);

        result.Result.Should().BeOfType<OkObjectResult>();
        var ok = (OkObjectResult)result.Result!;
        ((IEnumerable<LeadScoreRule>)ok.Value!).Should().HaveCount(1);
    }
}
