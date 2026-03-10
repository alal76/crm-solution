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

/// <summary>Tests for <see cref="SalesCoachAgent"/> (TCOV-062).</summary>
public class SalesCoachAgentTests
{
    private readonly Kernel _kernel = Kernel.CreateBuilder().Build();
    private readonly Mock<ILogger<SalesCoachAgent>> _loggerMock = new();

    private SalesCoachAgent CreateAgent() => new(_kernel, _loggerMock.Object);

    [Fact]
    public void Constructor_NullKernel_ShouldThrow()
    {
        var act = () => new SalesCoachAgent(null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullLogger_ShouldThrow()
    {
        var act = () => new SalesCoachAgent(_kernel, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ValidParams_ShouldNotThrow()
    {
        var act = () => CreateAgent();
        act.Should().NotThrow();
    }

    [Fact]
    public void AgentName_ShouldBeSalesCoachAgent()
    {
        CreateAgent().AgentName.Should().Be("Sales Coach Agent");
    }

    [Fact]
    public void AgentType_ShouldBeSalesCoach()
    {
        CreateAgent().AgentType.Should().Be(AgentType.SalesCoach);
    }

    [Fact]
    public void Temperature_ShouldBe05()
    {
        CreateAgent().Temperature.Should().Be(0.5);
    }

    [Fact]
    public void AllowedPlugins_ShouldContainOpportunityAndAccount()
    {
        var plugins = CreateAgent().AllowedPlugins;
        plugins.Should().Contain("Opportunity");
        plugins.Should().Contain("Account");
        plugins.Should().Contain("Contact");
    }

    [Fact]
    public void SystemPrompt_ShouldMentionSales()
    {
        CreateAgent().SystemPrompt.Should().ContainAny("sales", "coach", "deal", "SPIN", "MEDDIC");
    }

    [Fact]
    public void MaxTokens_ShouldBe4096()
    {
        CreateAgent().MaxTokens.Should().Be(4096);
    }
}
