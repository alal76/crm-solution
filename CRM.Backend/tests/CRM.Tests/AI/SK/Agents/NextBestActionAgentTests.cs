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

/// <summary>Tests for <see cref="NextBestActionAgent"/> (TCOV-064).</summary>
public class NextBestActionAgentTests
{
    private readonly Kernel _kernel = Kernel.CreateBuilder().Build();
    private readonly Mock<ILogger<NextBestActionAgent>> _loggerMock = new();

    private NextBestActionAgent CreateAgent() => new(_kernel, _loggerMock.Object);

    [Fact]
    public void Constructor_NullKernel_ShouldThrow()
    {
        var act = () => new NextBestActionAgent(null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ValidParams_ShouldNotThrow()
    {
        var act = () => CreateAgent();
        act.Should().NotThrow();
    }

    [Fact]
    public void AgentName_ShouldBeNextBestActionAgent()
    {
        CreateAgent().AgentName.Should().Be("Next Best Action Agent");
    }

    [Fact]
    public void AgentType_ShouldBeNextBestAction()
    {
        CreateAgent().AgentType.Should().Be(AgentType.NextBestAction);
    }

    [Fact]
    public void Temperature_ShouldBe04()
    {
        CreateAgent().Temperature.Should().Be(0.4);
    }

    [Fact]
    public void AllowedPlugins_ShouldContainAccountAndLead()
    {
        var plugins = CreateAgent().AllowedPlugins;
        plugins.Should().Contain("Account");
        plugins.Should().Contain("Lead");
    }

    [Fact]
    public void SystemPrompt_ShouldNotBeNullOrWhiteSpace()
    {
        CreateAgent().SystemPrompt.Should().NotBeNullOrWhiteSpace();
    }
}
