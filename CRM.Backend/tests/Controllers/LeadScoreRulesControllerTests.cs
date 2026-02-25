// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
/// Unit tests for LeadScoreRulesController.
/// Uses InMemory database per test for isolation.
/// Covers TODO-SYS008-002.
/// </summary>
public class LeadScoreRulesControllerTests : IDisposable
{
    private readonly CrmDbContext _dbContext;
    private readonly Mock<ILogger<LeadScoreRulesController>> _mockLogger;
    private readonly LeadScoreRulesController _controller;

    public LeadScoreRulesControllerTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new CrmDbContext(options, null!);
        _mockLogger = new Mock<ILogger<LeadScoreRulesController>>();
        _controller = new LeadScoreRulesController(_dbContext, _mockLogger.Object);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    private async Task<LeadScoreRule> SeedRuleAsync(
        string name = "Test Rule",
        bool isActive = true,
        LeadScoreRuleType ruleType = LeadScoreRuleType.Attribute,
        int priority = 100,
        string? category = null)
    {
        var rule = new LeadScoreRule
        {
            Name = name,
            IsActive = isActive,
            RuleType = ruleType,
            Priority = priority,
            Category = category,
            ScoreImpact = 10,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.LeadScoreRules.Add(rule);
        await _dbContext.SaveChangesAsync();
        return rule;
    }

    #region GetRules Tests

    [Fact]
    public async Task GetRules_ShouldReturnOk_WithAllRules()
    {
        // Arrange
        await SeedRuleAsync("Rule A");
        await SeedRuleAsync("Rule B");

        // Act
        var result = await _controller.GetRules();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var rules = okResult.Value as IEnumerable<LeadScoreRule>;
        rules.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetRules_ShouldReturnOnlyActive_WhenIsActiveFilterApplied()
    {
        // Arrange
        await SeedRuleAsync("Active Rule", isActive: true);
        await SeedRuleAsync("Inactive Rule", isActive: false);

        // Act
        var result = await _controller.GetRules(isActive: true);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var rules = (okResult.Value as IEnumerable<LeadScoreRule>)!.ToList();
        rules.Should().HaveCount(1);
        rules[0].Name.Should().Be("Active Rule");
    }

    [Fact]
    public async Task GetRules_ShouldFilterByRuleType_WhenTypeFilterApplied()
    {
        // Arrange
        await SeedRuleAsync("Behavior Rule", ruleType: LeadScoreRuleType.Behavior);
        await SeedRuleAsync("Attribute Rule", ruleType: LeadScoreRuleType.Attribute);

        // Act
        var result = await _controller.GetRules(ruleType: LeadScoreRuleType.Behavior);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var rules = (okResult.Value as IEnumerable<LeadScoreRule>)!.ToList();
        rules.Should().HaveCount(1);
        rules[0].Name.Should().Be("Behavior Rule");
    }

    #endregion

    #region GetRule Tests

    [Fact]
    public async Task GetRule_ShouldReturnOk_WhenRuleExists()
    {
        // Arrange
        var seeded = await SeedRuleAsync("Find Me");

        // Act
        var result = await _controller.GetRule(seeded.Id);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var rule = okResult.Value as LeadScoreRule;
        rule!.Name.Should().Be("Find Me");
    }

    [Fact]
    public async Task GetRule_ShouldReturnNotFound_WhenRuleDoesNotExist()
    {
        // Act
        var result = await _controller.GetRule(9999);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region CreateRule Tests

    [Fact]
    public async Task CreateRule_ShouldReturnCreated_WithNewRule()
    {
        // Arrange
        var dto = new LeadScoreRuleDto
        {
            Name = "New Score Rule",
            RuleType = LeadScoreRuleType.Attribute,
            ScoreImpact = 15,
            IsActive = true,
            Priority = 50
        };

        // Act
        var result = await _controller.CreateRule(dto);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        var rule = createdResult.Value as LeadScoreRule;
        rule!.Name.Should().Be("New Score Rule");
        rule.ScoreImpact.Should().Be(15);
    }

    #endregion

    #region ToggleRule Tests

    [Fact]
    public async Task ToggleRule_ShouldFlipActiveState_WhenRuleExists()
    {
        // Arrange
        var seeded = await SeedRuleAsync("Toggle Rule", isActive: true);

        // Act
        var result = await _controller.ToggleRule(seeded.Id);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var rule = okResult.Value as LeadScoreRule;
        rule!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task ToggleRule_ShouldReturnNotFound_WhenRuleDoesNotExist()
    {
        // Act
        var result = await _controller.ToggleRule(9999);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region DeleteRule Tests

    [Fact]
    public async Task DeleteRule_ShouldReturnNoContent_WhenRuleExists()
    {
        // Arrange
        var seeded = await SeedRuleAsync("To Delete");

        // Act
        var result = await _controller.DeleteRule(seeded.Id);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _dbContext.LeadScoreRules.Find(seeded.Id).Should().BeNull();
    }

    [Fact]
    public async Task DeleteRule_ShouldReturnNotFound_WhenRuleDoesNotExist()
    {
        // Act
        var result = await _controller.DeleteRule(9999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Metadata Endpoints Tests

    [Fact]
    public async Task GetStats_ShouldReturnOk_WithRuleCounts()
    {
        // Arrange
        await SeedRuleAsync("Active 1", isActive: true);
        await SeedRuleAsync("Active 2", isActive: true);
        await SeedRuleAsync("Inactive 1", isActive: false);

        // Act
        var result = await _controller.GetStats();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void GetAvailableFields_ShouldReturnOk_WithFieldList()
    {
        // Act
        var result = _controller.GetAvailableFields();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var fields = okResult.Value as IEnumerable<FieldDefinition>;
        fields.Should().NotBeEmpty();
    }

    #endregion
}
