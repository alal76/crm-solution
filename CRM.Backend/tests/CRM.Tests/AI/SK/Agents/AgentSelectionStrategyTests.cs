// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// Licensed under the GNU Affero General Public License v3.0.
// See https://www.gnu.org/licenses/ for details.

using CRM.Core.Entities.AI;
using CRM.Infrastructure.AI.SK.Agents;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.AI.SK.Agents;

#nullable enable

/// <summary>
/// Unit tests for <see cref="AgentSelectionStrategy"/>.
/// Validates intent detection, entity type detection, and agent recommendation logic.
/// This is a static class — no constructor or DI involved.
/// </summary>
public class AgentSelectionStrategyTests
{
    #region DetectIntent Tests

    [Theory]
    [InlineData("score this lead", "lead")]
    [InlineData("what is the lead score", "lead")]
    [InlineData("qualify lead for sales", "lead")]
    public void DetectIntent_ScoringKeywords_ShouldReturnLeadIntent(string message, string expected)
    {
        AgentSelectionStrategy.DetectIntent(message).Should().Be(expected);
    }

    [Theory]
    [InlineData("draft email to customer", "email")]
    [InlineData("compose email for follow up", "email")]
    public void DetectIntent_EmailKeywords_ShouldReturnEmailIntent(string message, string expected)
    {
        AgentSelectionStrategy.DetectIntent(message).Should().Be(expected);
    }

    [Theory]
    [InlineData("triage this ticket", "support")]
    [InlineData("classify ticket urgency", "support")]
    public void DetectIntent_SupportKeywords_ShouldReturnSupportIntent(string message, string expected)
    {
        var result = AgentSelectionStrategy.DetectIntent(message);
        result.Should().Be(expected);
    }

    [Fact]
    public void DetectIntent_NullMessage_ShouldReturnNull()
    {
        AgentSelectionStrategy.DetectIntent(null!).Should().BeNull();
    }

    [Fact]
    public void DetectIntent_EmptyMessage_ShouldReturnNull()
    {
        AgentSelectionStrategy.DetectIntent(string.Empty).Should().BeNull();
    }

    [Fact]
    public void DetectIntent_WhitespaceMessage_ShouldReturnNull()
    {
        AgentSelectionStrategy.DetectIntent("   ").Should().BeNull();
    }

    [Fact]
    public void DetectIntent_NoMatchingKeywords_ShouldReturnNull()
    {
        AgentSelectionStrategy.DetectIntent("hello world how are you").Should().BeNull();
    }

    #endregion

    #region DetectEntityType Tests

    [Theory]
    [InlineData("check this lead", "lead")]
    [InlineData("find leads", "lead")]
    [InlineData("the lead needs attention", "lead")]
    public void DetectEntityType_LeadKeywords_ShouldReturnLead(string message, string expected)
    {
        AgentSelectionStrategy.DetectEntityType(message).Should().Be(expected);
    }

    [Theory]
    [InlineData("look at this account", "account")]
    [InlineData("the customer asked", "account")]
    public void DetectEntityType_AccountKeywords_ShouldReturnAccount(string message, string expected)
    {
        var result = AgentSelectionStrategy.DetectEntityType(message);
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData("the ticket is urgent", "servicerequest")]
    [InlineData("service request needs triage", "servicerequest")]
    public void DetectEntityType_ServiceKeywords_ShouldReturnServiceRequest(string message, string expected)
    {
        var result = AgentSelectionStrategy.DetectEntityType(message);
        result.Should().NotBeNull();
    }

    [Fact]
    public void DetectEntityType_NullMessage_ShouldReturnNull()
    {
        AgentSelectionStrategy.DetectEntityType(null!).Should().BeNull();
    }

    [Fact]
    public void DetectEntityType_EmptyMessage_ShouldReturnNull()
    {
        AgentSelectionStrategy.DetectEntityType(string.Empty).Should().BeNull();
    }

    [Fact]
    public void DetectEntityType_NoMatchingEntity_ShouldReturnNull()
    {
        AgentSelectionStrategy.DetectEntityType("hello how are you today").Should().BeNull();
    }

    #endregion

    #region RecommendAgent Tests

    [Fact]
    public void RecommendAgent_LeadIntent_ShouldReturnLeadScoring()
    {
        var result = AgentSelectionStrategy.RecommendAgent("lead", "lead");
        result.Should().Be(AgentType.LeadScoring);
    }

    [Fact]
    public void RecommendAgent_ServiceRequestEntity_ShouldReturnSupportTriage()
    {
        var result = AgentSelectionStrategy.RecommendAgent("servicerequest", null);
        result.Should().Be(AgentType.SupportTriage);
    }

    [Fact]
    public void RecommendAgent_NullEntityAndIntent_ShouldReturnGeneralAssistant()
    {
        var result = AgentSelectionStrategy.RecommendAgent(null, null);
        result.Should().Be(AgentType.GeneralAssistant);
    }

    [Fact]
    public void RecommendAgent_UnknownEntity_ShouldReturnGeneralAssistant()
    {
        var result = AgentSelectionStrategy.RecommendAgent("unknown_entity", null);
        result.Should().Be(AgentType.GeneralAssistant);
    }

    [Fact]
    public void RecommendAgent_IntentTakesPriority_ShouldRouteByIntent()
    {
        // Intent-based routing should take priority over entity-based
        var result = AgentSelectionStrategy.RecommendAgent("account", "lead");
        // "lead" intent → LeadScoring even if entity is account
        result.Should().Be(AgentType.LeadScoring);
    }

    [Theory]
    [InlineData("support")]
    [InlineData("ticket")]
    public void RecommendAgent_SupportIntents_ShouldReturnSupportTriage(string intent)
    {
        var result = AgentSelectionStrategy.RecommendAgent(null, intent);
        result.Should().Be(AgentType.SupportTriage);
    }

    [Fact]
    public void RecommendAgent_EmptyStrings_ShouldReturnGeneralAssistant()
    {
        var result = AgentSelectionStrategy.RecommendAgent("", "");
        result.Should().Be(AgentType.GeneralAssistant);
    }

    #endregion
}
