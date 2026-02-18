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
/// Unit tests for <see cref="SupportTriageAgent"/>.
/// Validates CanHandle routing for support entities and context enrichment.
/// </summary>
public class SupportTriageAgentTests
{
    #region Fields & Setup

    private readonly Kernel _kernel;
    private readonly Mock<ILogger<SupportTriageAgent>> _loggerMock = new();
    private readonly SupportTriageAgent _agent;

    public SupportTriageAgentTests()
    {
        _kernel = Kernel.CreateBuilder().Build();
        _agent = new SupportTriageAgent(_kernel, _loggerMock.Object);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void AgentName_ShouldReturnSupportTriageAgent()
    {
        _agent.AgentName.Should().Be("Support Triage Agent");
    }

    [Fact]
    public void AgentType_ShouldBeSupportTriage()
    {
        _agent.AgentType.Should().Be(AgentType.SupportTriage);
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
    public void AllowedPlugins_ShouldContainServiceRequestAndKnowledgeBase()
    {
        _agent.AllowedPlugins.Should().Contain("ServiceRequest");
        _agent.AllowedPlugins.Should().Contain("KnowledgeBase");
    }

    [Fact]
    public void AllowedPlugins_ShouldContainAccountAndSearch()
    {
        _agent.AllowedPlugins.Should().Contain("Account");
        _agent.AllowedPlugins.Should().Contain("Search");
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
        var act = () => new SupportTriageAgent(null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullLogger_ShouldThrow()
    {
        var act = () => new SupportTriageAgent(_kernel, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region CanHandle Tests

    [Theory]
    [InlineData("servicerequest")]
    [InlineData("ticket")]
    [InlineData("support")]
    public void CanHandle_SupportEntityTypes_ShouldReturnTrue(string entityType)
    {
        _agent.CanHandle(entityType, null).Should().BeTrue();
    }

    [Theory]
    [InlineData("ServiceRequest")]
    [InlineData("Ticket")]
    [InlineData("SUPPORT")]
    public void CanHandle_SupportEntityTypesCaseInsensitive_ShouldReturnTrue(string entityType)
    {
        _agent.CanHandle(entityType, null).Should().BeTrue();
    }

    [Theory]
    [InlineData("account")]
    [InlineData("lead")]
    [InlineData("opportunity")]
    [InlineData("contact")]
    public void CanHandle_NonSupportEntityTypes_ShouldReturnFalse(string entityType)
    {
        _agent.CanHandle(entityType, null).Should().BeFalse();
    }

    [Fact]
    public void CanHandle_NullEntityType_ShouldReturnFalse()
    {
        _agent.CanHandle(null, null).Should().BeFalse();
    }

    [Fact]
    public void CanHandle_EmptyEntityType_ShouldReturnFalse()
    {
        _agent.CanHandle(string.Empty, null).Should().BeFalse();
    }

    #endregion

    #region EnrichContextAsync Tests

    [Fact]
    public async Task EnrichContextAsync_ShouldEnrichMessage()
    {
        // Arrange
        var userMessage = "My printer is not working";

        // Act
        var result = await _agent.EnrichContextAsync(userMessage, null, null);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Length.Should().BeGreaterThanOrEqualTo(userMessage.Length);
    }

    [Fact]
    public async Task EnrichContextAsync_WithEntityType_ShouldIncludeContext()
    {
        // Arrange
        var userMessage = "Check the status of ticket";

        // Act
        var result = await _agent.EnrichContextAsync(userMessage, "servicerequest", 42);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task EnrichContextAsync_EmptyMessage_ShouldStillReturnResult()
    {
        // Act
        var result = await _agent.EnrichContextAsync(string.Empty, null, null);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion
}
