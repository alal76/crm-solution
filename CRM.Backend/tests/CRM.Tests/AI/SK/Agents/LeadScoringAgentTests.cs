// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities.AI;
using CRM.Infrastructure.AI.SK.Agents;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Moq;
using Xunit;

namespace CRM.Tests.AI.SK.Agents;

#nullable enable

/// <summary>
/// Unit tests for <see cref="LeadScoringAgent"/>.
/// Validates BANT scoring post-processing, CanHandle routing, and agent properties.
/// </summary>
public class LeadScoringAgentTests
{
    #region Fields & Setup

    private readonly Kernel _kernel;
    private readonly Mock<ILogger<LeadScoringAgent>> _loggerMock = new();
    private readonly LeadScoringAgent _agent;

    public LeadScoringAgentTests()
    {
        _kernel = Kernel.CreateBuilder().Build();
        _agent = new LeadScoringAgent(_kernel, _loggerMock.Object);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void AgentName_ShouldReturnLeadScoringAgent()
    {
        _agent.AgentName.Should().Be("Lead Scoring Agent");
    }

    [Fact]
    public void AgentType_ShouldBeLeadScoring()
    {
        _agent.AgentType.Should().Be(AgentType.LeadScoring);
    }

    [Fact]
    public void Temperature_ShouldBe02()
    {
        _agent.Temperature.Should().Be(0.2);
    }

    [Fact]
    public void MaxTokens_ShouldBe2048()
    {
        _agent.MaxTokens.Should().Be(2048);
    }

    [Fact]
    public void AllowedPlugins_ShouldContainLeadAndAccount()
    {
        _agent.AllowedPlugins.Should().Contain("Lead");
        _agent.AllowedPlugins.Should().Contain("Account");
    }

    [Fact]
    public void AllowedPlugins_ShouldContainSearchAndContact()
    {
        _agent.AllowedPlugins.Should().Contain("Search");
        _agent.AllowedPlugins.Should().Contain("Contact");
    }

    [Fact]
    public void SystemPrompt_ShouldNotBeNullOrEmpty()
    {
        _agent.SystemPrompt.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void SystemPrompt_ShouldMentionBANT()
    {
        _agent.SystemPrompt.Should().ContainAny("BANT", "budget", "authority", "need", "timeline");
    }

    #endregion

    #region Constructor Validation

    [Fact]
    public void Constructor_NullKernel_ShouldThrow()
    {
        var act = () => new LeadScoringAgent(null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullLogger_ShouldThrow()
    {
        var act = () => new LeadScoringAgent(_kernel, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region CanHandle Tests

    [Fact]
    public void CanHandle_LeadEntityType_ShouldReturnTrue()
    {
        _agent.CanHandle("lead", null).Should().BeTrue();
    }

    [Fact]
    public void CanHandle_LeadEntityTypeCaseInsensitive_ShouldReturnTrue()
    {
        _agent.CanHandle("Lead", null).Should().BeTrue();
        _agent.CanHandle("LEAD", null).Should().BeTrue();
    }

    [Fact]
    public void CanHandle_NonLeadEntityType_ShouldReturnFalse()
    {
        _agent.CanHandle("account", null).Should().BeFalse();
        _agent.CanHandle("contact", null).Should().BeFalse();
        _agent.CanHandle("opportunity", null).Should().BeFalse();
    }

    [Fact]
    public void CanHandle_NullEntityType_ShouldReturnFalse()
    {
        _agent.CanHandle(null, null).Should().BeFalse();
    }

    #endregion

    #region PostProcessAsync Tests

    [Fact]
    public async Task PostProcessAsync_ValidBANTJson_ShouldReturnParsedResult()
    {
        // Arrange
        var validJson = """
        {
            "budget": 20,
            "authority": 25,
            "need": 15,
            "timeline": 20,
            "total": 80,
            "category": "Hot"
        }
        """;

        // Act
        var result = await _agent.PostProcessAsync(validJson);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain("budget");
    }

    [Fact]
    public async Task PostProcessAsync_JsonWithCodeFences_ShouldStripFences()
    {
        // Arrange
        var jsonWithFences = """
        ```json
        {
            "budget": 20,
            "authority": 25,
            "need": 15,
            "timeline": 20,
            "total": 80,
            "category": "Warm"
        }
        ```
        """;

        // Act
        var result = await _agent.PostProcessAsync(jsonWithFences);

        // Assert
        result.Should().NotContain("```");
    }

    [Fact]
    public async Task PostProcessAsync_InvalidJson_ShouldReturnErrorWithZeros()
    {
        // Arrange
        var invalidJson = "This is not valid JSON at all";

        // Act
        var result = await _agent.PostProcessAsync(invalidJson);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain("0");
    }

    [Fact]
    public async Task PostProcessAsync_EmptyString_ShouldHandleGracefully()
    {
        // Act
        var result = await _agent.PostProcessAsync(string.Empty);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion
}
