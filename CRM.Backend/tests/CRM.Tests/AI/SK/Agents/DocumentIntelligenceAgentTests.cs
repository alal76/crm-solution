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

/// <summary>Tests for <see cref="DocumentIntelligenceAgent"/> (TCOV-067).</summary>
public class DocumentIntelligenceAgentTests
{
    private readonly Kernel _kernel = Kernel.CreateBuilder().Build();
    private readonly Mock<ILogger<DocumentIntelligenceAgent>> _loggerMock = new();

    private DocumentIntelligenceAgent CreateAgent() => new(_kernel, _loggerMock.Object);

    [Fact]
    public void Constructor_NullKernel_ShouldThrow()
    {
        var act = () => new DocumentIntelligenceAgent(null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ValidParams_ShouldNotThrow()
    {
        var act = () => CreateAgent();
        act.Should().NotThrow();
    }

    [Fact]
    public void AgentName_ShouldBeDocumentIntelligenceAgent()
    {
        CreateAgent().AgentName.Should().Be("Document Intelligence Agent");
    }

    [Fact]
    public void AgentType_ShouldBeDocumentIntelligence()
    {
        CreateAgent().AgentType.Should().Be(AgentType.DocumentIntelligence);
    }

    [Fact]
    public void Temperature_ShouldBe02()
    {
        CreateAgent().Temperature.Should().Be(0.2);
    }

    [Fact]
    public void AllowedPlugins_ShouldContainContractAndQuote()
    {
        var plugins = CreateAgent().AllowedPlugins;
        plugins.Should().Contain("Contract");
        plugins.Should().Contain("Quote");
    }

    [Fact]
    public void SystemPrompt_ShouldMentionDocument()
    {
        CreateAgent().SystemPrompt.Should().ContainAny("document", "contract", "clause");
    }
}
