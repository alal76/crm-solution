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

/// <summary>Tests for <see cref="TicketResolutionAgent"/> (TCOV-065).</summary>
public class TicketResolutionAgentTests
{
    private readonly Kernel _kernel = Kernel.CreateBuilder().Build();
    private readonly Mock<ILogger<TicketResolutionAgent>> _loggerMock = new();

    private TicketResolutionAgent CreateAgent() => new(_kernel, _loggerMock.Object);

    [Fact]
    public void Constructor_NullKernel_ShouldThrow()
    {
        var act = () => new TicketResolutionAgent(null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ValidParams_ShouldNotThrow()
    {
        var act = () => CreateAgent();
        act.Should().NotThrow();
    }

    [Fact]
    public void AgentName_ShouldBeTicketResolutionAgent()
    {
        CreateAgent().AgentName.Should().Be("Ticket Resolution Agent");
    }

    [Fact]
    public void AgentType_ShouldBeTicketResolution()
    {
        CreateAgent().AgentType.Should().Be(AgentType.TicketResolution);
    }

    [Fact]
    public void Temperature_ShouldBe03()
    {
        CreateAgent().Temperature.Should().Be(0.3);
    }

    [Fact]
    public void AllowedPlugins_ShouldContainServiceRequestAndKnowledgeBase()
    {
        var plugins = CreateAgent().AllowedPlugins;
        plugins.Should().Contain("ServiceRequest");
        plugins.Should().Contain("KnowledgeBase");
    }

    [Fact]
    public void SystemPrompt_ShouldMentionTicket()
    {
        CreateAgent().SystemPrompt.Should().ContainAny("ticket", "resolution", "knowledge");
    }
}
