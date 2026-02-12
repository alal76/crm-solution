// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// Licensed under the GNU Affero General Public License v3.0.
// See https://www.gnu.org/licenses/ for details.

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
/// Unit tests for <see cref="GeneralAssistantAgent"/>.
/// Validates catch-all behavior — CanHandle always returns true.
/// </summary>
public class GeneralAssistantAgentTests
{
    #region Fields & Setup

    private readonly Kernel _kernel;
    private readonly Mock<ILogger<GeneralAssistantAgent>> _loggerMock = new();
    private readonly GeneralAssistantAgent _agent;

    public GeneralAssistantAgentTests()
    {
        _kernel = Kernel.CreateBuilder().Build();
        _agent = new GeneralAssistantAgent(_kernel, _loggerMock.Object);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void AgentName_ShouldReturnGeneralAssistant()
    {
        _agent.AgentName.Should().Be("General Assistant");
    }

    [Fact]
    public void AgentType_ShouldBeGeneralAssistant()
    {
        _agent.AgentType.Should().Be(AgentType.GeneralAssistant);
    }

    [Fact]
    public void Temperature_ShouldBe05()
    {
        _agent.Temperature.Should().Be(0.5);
    }

    [Fact]
    public void MaxTokens_ShouldBe4096()
    {
        _agent.MaxTokens.Should().Be(4096);
    }

    [Fact]
    public void AllowedPlugins_ShouldContainCorePlugins()
    {
        _agent.AllowedPlugins.Should().Contain("Account");
        _agent.AllowedPlugins.Should().Contain("Contact");
        _agent.AllowedPlugins.Should().Contain("Lead");
        _agent.AllowedPlugins.Should().Contain("Opportunity");
        _agent.AllowedPlugins.Should().Contain("Search");
    }

    [Fact]
    public void AllowedPlugins_ShouldContainCalendar()
    {
        _agent.AllowedPlugins.Should().Contain("Calendar");
    }

    [Fact]
    public void SystemPrompt_ShouldNotBeNullOrEmpty()
    {
        _agent.SystemPrompt.Should().NotBeNullOrWhiteSpace();
    }

    #endregion

    #region Constructor Validation

    [Fact]
    public void Constructor_NullKernel_ShouldThrow()
    {
        var act = () => new GeneralAssistantAgent(null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullLogger_ShouldThrow()
    {
        var act = () => new GeneralAssistantAgent(_kernel, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region CanHandle Tests — Catch-All Behavior

    [Theory]
    [InlineData("account", null)]
    [InlineData("lead", null)]
    [InlineData("contact", null)]
    [InlineData("opportunity", null)]
    [InlineData("servicerequest", null)]
    [InlineData("unknown", null)]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData(null, "score")]
    [InlineData("lead", "convert")]
    public void CanHandle_AnyInput_ShouldAlwaysReturnTrue(string? entityType, string? intent)
    {
        // GeneralAssistant is the catch-all fallback agent
        _agent.CanHandle(entityType, intent).Should().BeTrue();
    }

    [Fact]
    public void CanHandle_RandomString_ShouldReturnTrue()
    {
        _agent.CanHandle("xyzzy_nonsense_12345", "unknown_intent").Should().BeTrue();
    }

    #endregion

    #region PostProcessAsync Tests

    [Fact]
    public async Task PostProcessAsync_PlainText_ShouldReturnAsIs()
    {
        // Arrange
        var response = "Here are the top accounts by revenue...";

        // Act
        var result = await _agent.PostProcessAsync(response);

        // Assert — base implementation returns input unchanged
        result.Should().Be(response);
    }

    #endregion
}
