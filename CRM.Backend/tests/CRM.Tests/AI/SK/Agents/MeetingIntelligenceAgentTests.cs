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

/// <summary>Tests for <see cref="MeetingIntelligenceAgent"/> (TCOV-061).</summary>
public class MeetingIntelligenceAgentTests
{
    private readonly Kernel _kernel = Kernel.CreateBuilder().Build();
    private readonly Mock<ILogger<MeetingIntelligenceAgent>> _loggerMock = new();

    private MeetingIntelligenceAgent CreateAgent() =>
        new(_kernel, _loggerMock.Object);

    [Fact]
    public void Constructor_NullKernel_ShouldThrow()
    {
        var act = () => new MeetingIntelligenceAgent(null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullLogger_ShouldThrow()
    {
        var act = () => new MeetingIntelligenceAgent(_kernel, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ValidParams_ShouldNotThrow()
    {
        var act = () => CreateAgent();
        act.Should().NotThrow();
    }

    [Fact]
    public void AgentName_ShouldBeMeetingIntelligenceAgent()
    {
        CreateAgent().AgentName.Should().Be("Meeting Intelligence Agent");
    }

    [Fact]
    public void AgentType_ShouldBeMeetingIntelligence()
    {
        CreateAgent().AgentType.Should().Be(AgentType.MeetingIntelligence);
    }

    [Fact]
    public void Temperature_ShouldBe04()
    {
        CreateAgent().Temperature.Should().Be(0.4);
    }

    [Fact]
    public void MaxTokens_ShouldBe4096()
    {
        CreateAgent().MaxTokens.Should().Be(4096);
    }

    [Fact]
    public void AllowedPlugins_ShouldContainCalendarAndAccount()
    {
        var plugins = CreateAgent().AllowedPlugins;
        plugins.Should().Contain("Calendar");
        plugins.Should().Contain("Account");
        plugins.Should().Contain("Contact");
    }

    [Fact]
    public void SystemPrompt_ShouldNotBeNullOrEmpty()
    {
        CreateAgent().SystemPrompt.Should().NotBeNullOrWhiteSpace();
    }
}
