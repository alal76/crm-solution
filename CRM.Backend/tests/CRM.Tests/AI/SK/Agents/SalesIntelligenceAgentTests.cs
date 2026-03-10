// CRM Solution — CRM Test Suite
using CRM.Core.Entities.AI;
using CRM.Infrastructure.AI.SK.Agents;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Moq;
using Xunit;

namespace CRM.Tests.AI.SK.Agents;

#nullable enable

/// <summary>Tests for <see cref="SalesIntelligenceAgent"/> (TCOV-063).</summary>
public class SalesIntelligenceAgentTests
{
    private readonly Kernel _kernel = Kernel.CreateBuilder().Build();
    private readonly Mock<ILogger<SalesIntelligenceAgent>> _loggerMock = new();

    private SalesIntelligenceAgent CreateAgent() => new(_kernel, _loggerMock.Object);

    [Fact]
    public void Constructor_NullKernel_ShouldThrow()
    {
        var act = () => new SalesIntelligenceAgent(null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ValidParams_ShouldNotThrow()
    {
        var act = () => CreateAgent();
        act.Should().NotThrow();
    }

    [Fact]
    public void AgentName_ShouldBeSalesIntelligenceAgent()
    {
        CreateAgent().AgentName.Should().Be("Sales Intelligence Agent");
    }

    [Fact]
    public void AgentType_ShouldBeSalesIntelligence()
    {
        CreateAgent().AgentType.Should().Be(AgentType.SalesIntelligence);
    }

    [Fact]
    public void Temperature_ShouldBe03()
    {
        CreateAgent().Temperature.Should().Be(0.3);
    }

    [Fact]
    public void AllowedPlugins_ShouldContainOpportunityLeadAndSearch()
    {
        var plugins = CreateAgent().AllowedPlugins;
        plugins.Should().Contain("Opportunity");
        plugins.Should().Contain("Lead");
        plugins.Should().Contain("Search");
    }

    [Fact]
    public void SystemPrompt_ShouldNotBeNullOrWhiteSpace()
    {
        CreateAgent().SystemPrompt.Should().NotBeNullOrWhiteSpace();
    }
}
