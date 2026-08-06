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

/// <summary>Tests for <see cref="RevenueIntelligenceAgent"/> (TCOV-066).</summary>
public class RevenueIntelligenceAgentTests
{
    private readonly Kernel _kernel = Kernel.CreateBuilder().Build();
    private readonly Mock<ILogger<RevenueIntelligenceAgent>> _loggerMock = new();

    private RevenueIntelligenceAgent CreateAgent() => new(_kernel, _loggerMock.Object);

    [Fact]
    public void Constructor_NullKernel_ShouldThrow()
    {
        var act = () => new RevenueIntelligenceAgent(null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ValidParams_ShouldNotThrow()
    {
        var act = () => CreateAgent();
        act.Should().NotThrow();
    }

    [Fact]
    public void AgentName_ShouldBeRevenueIntelligenceAgent()
    {
        CreateAgent().AgentName.Should().Be("Revenue Intelligence Agent");
    }

    [Fact]
    public void AgentType_ShouldBeRevenueIntelligence()
    {
        CreateAgent().AgentType.Should().Be(AgentType.RevenueIntelligence);
    }

    [Fact]
    public void Temperature_ShouldBe02()
    {
        CreateAgent().Temperature.Should().Be(0.2);
    }

    [Fact]
    public void AllowedPlugins_ShouldContainOpportunityAndContract()
    {
        var plugins = CreateAgent().AllowedPlugins;
        plugins.Should().Contain("Opportunity");
        plugins.Should().Contain("Contract");
    }

    [Fact]
    public void SystemPrompt_ShouldMentionRevenue()
    {
        CreateAgent().SystemPrompt.Should().ContainAny("revenue", "ARR", "MRR", "forecast");
    }
}
