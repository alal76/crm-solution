// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities.AI;
using CRM.Infrastructure.AI.SK.Agents;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Services.SK;

/// <summary>
/// Unit tests for the AgentSelectionStrategy static class.
/// </summary>
public class AgentSelectionStrategyTests
{
    #region DetectIntent Tests

    [Fact]
    public void DetectIntent_ShouldReturnNull_WhenMessageIsEmpty()
    {
        var result = AgentSelectionStrategy.DetectIntent(string.Empty);
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("score lead for me")]
    [InlineData("qualify lead please")]
    [InlineData("lead score analysis")]
    [InlineData("run BANT assessment")]
    public void DetectIntent_ShouldReturnLead_ForLeadKeywords(string message)
    {
        var result = AgentSelectionStrategy.DetectIntent(message);
        result.Should().Be("lead");
    }

    [Theory]
    [InlineData("draft email for customer")]
    [InlineData("write email template")]
    [InlineData("send email notification")]
    [InlineData("email template design")]
    public void DetectIntent_ShouldReturnEmail_ForEmailKeywords(string message)
    {
        var result = AgentSelectionStrategy.DetectIntent(message);
        result.Should().Be("email");
    }

    [Theory]
    [InlineData("triage this issue")]
    [InlineData("support ticket#123")]
    [InlineData("new service request")]
    public void DetectIntent_ShouldReturnSupport_ForSupportKeywords(string message)
    {
        var result = AgentSelectionStrategy.DetectIntent(message);
        result.Should().Be("support");
    }

    [Theory]
    [InlineData("quarterly forecast report")]
    [InlineData("set quota for team")]
    public void DetectIntent_ShouldReturnForecast_ForForecastKeywords(string message)
    {
        var result = AgentSelectionStrategy.DetectIntent(message);
        result.Should().Be("forecast");
    }

    [Theory]
    [InlineData("contract renewal coming up")]
    [InlineData("review contract details")]
    public void DetectIntent_ShouldReturnContract_ForContractKeywords(string message)
    {
        var result = AgentSelectionStrategy.DetectIntent(message);
        result.Should().Be("contract");
    }

    [Theory]
    [InlineData("search knowledge base article")]
    [InlineData("find kb article on this topic")]
    public void DetectIntent_ShouldReturnKnowledge_ForKnowledgeKeywords(string message)
    {
        var result = AgentSelectionStrategy.DetectIntent(message);
        result.Should().Be("knowledge");
    }

    [Theory]
    [InlineData("how do i reset my password")]
    [InlineData("help me get started")]
    public void DetectIntent_ShouldReturnOnboarding_ForHelpKeywords(string message)
    {
        var result = AgentSelectionStrategy.DetectIntent(message);
        result.Should().Be("onboarding");
    }

    [Theory]
    [InlineData("generate a report on sales")]
    [InlineData("show analytics dashboard")]
    [InlineData("get some statistics")]
    public void DetectIntent_ShouldReturnData_ForAnalyticsKeywords(string message)
    {
        var result = AgentSelectionStrategy.DetectIntent(message);
        result.Should().Be("data");
    }

    [Theory]
    [InlineData("check deal health score")]
    [InlineData("what is the win probability")]
    public void DetectIntent_ShouldReturnDeal_ForDealKeywords(string message)
    {
        var result = AgentSelectionStrategy.DetectIntent(message);
        result.Should().Be("deal");
    }

    [Theory]
    [InlineData("check churn rate")]
    [InlineData("customer churn prediction")]
    public void DetectIntent_ShouldReturnChurn_ForChurnKeywords(string message)
    {
        var result = AgentSelectionStrategy.DetectIntent(message);
        result.Should().Be("churn");
    }

    #endregion

    #region DetectEntityType Tests

    [Fact]
    public void DetectEntityType_ShouldReturnNull_WhenMessageIsEmpty()
    {
        var result = AgentSelectionStrategy.DetectEntityType(string.Empty);
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("look up this ticket please")]
    [InlineData("service request details")]
    [InlineData("incident report for today")]
    public void DetectEntityType_ShouldDetectTicketEntityType(string message)
    {
        var result = AgentSelectionStrategy.DetectEntityType(message);
        result.Should().NotBeNull();
        result.Should().BeOneOf("ticket", "servicerequest", "incident");
    }

    [Theory]
    [InlineData("this lead needs follow up")]
    [InlineData("lead score review needed")]
    public void DetectEntityType_ShouldDetectLeadEntityType(string message)
    {
        var result = AgentSelectionStrategy.DetectEntityType(message);
        result.Should().NotBeNull();
        result.Should().Be("lead");
    }

    [Theory]
    [InlineData("update this account")]
    [InlineData("customer details")]
    public void DetectEntityType_ShouldDetectAccountEntityType(string message)
    {
        var result = AgentSelectionStrategy.DetectEntityType(message);
        result.Should().NotBeNull();
        result.Should().BeOneOf("account", "customer");
    }

    #endregion

    #region RecommendAgent Tests

    [Fact]
    public void RecommendAgent_ShouldReturnGeneralAssistant_WhenNoSignals()
    {
        var result = AgentSelectionStrategy.RecommendAgent(null, null);
        result.Should().Be(AgentType.GeneralAssistant);
    }

    [Fact]
    public void RecommendAgent_ShouldReturnLeadScoring_ForLeadIntent()
    {
        var result = AgentSelectionStrategy.RecommendAgent(null, "lead");
        result.Should().Be(AgentType.LeadScoring);
    }

    [Fact]
    public void RecommendAgent_ShouldReturnEmailAssistant_ForEmailIntent()
    {
        var result = AgentSelectionStrategy.RecommendAgent(null, "email");
        result.Should().Be(AgentType.EmailAssistant);
    }

    [Fact]
    public void RecommendAgent_ShouldReturnSupportTriage_ForSupportIntent()
    {
        var result = AgentSelectionStrategy.RecommendAgent(null, "support");
        result.Should().Be(AgentType.SupportTriage);
    }

    [Fact]
    public void RecommendAgent_ShouldReturnForecastAnalyst_ForForecastIntent()
    {
        var result = AgentSelectionStrategy.RecommendAgent(null, "forecast");
        result.Should().Be(AgentType.ForecastAnalyst);
    }

    [Fact]
    public void RecommendAgent_ShouldReturnContractAnalyst_ForContractIntent()
    {
        var result = AgentSelectionStrategy.RecommendAgent(null, "contract");
        result.Should().Be(AgentType.ContractAnalyst);
    }

    [Fact]
    public void RecommendAgent_ShouldReturnKnowledgeExpert_ForKnowledgeIntent()
    {
        var result = AgentSelectionStrategy.RecommendAgent(null, "knowledge");
        result.Should().Be(AgentType.KnowledgeExpert);
    }

    [Fact]
    public void RecommendAgent_ShouldReturnOnboardingGuide_ForOnboardingIntent()
    {
        var result = AgentSelectionStrategy.RecommendAgent(null, "onboarding");
        result.Should().Be(AgentType.OnboardingGuide);
    }

    [Fact]
    public void RecommendAgent_ShouldReturnDataAnalyst_ForDataIntent()
    {
        var result = AgentSelectionStrategy.RecommendAgent(null, "data");
        result.Should().Be(AgentType.DataAnalyst);
    }

    [Fact]
    public void RecommendAgent_ShouldReturnSupportTriage_ForTicketEntityType()
    {
        var result = AgentSelectionStrategy.RecommendAgent("ticket", null);
        result.Should().Be(AgentType.SupportTriage);
    }

    [Fact]
    public void RecommendAgent_ShouldReturnLeadScoring_ForLeadEntityType()
    {
        var result = AgentSelectionStrategy.RecommendAgent("lead", null);
        result.Should().Be(AgentType.LeadScoring);
    }

    [Fact]
    public void RecommendAgent_ShouldPrioritizeIntent_OverEntityType()
    {
        // Intent "email" should take priority over entityType "lead"
        var result = AgentSelectionStrategy.RecommendAgent("lead", "email");
        result.Should().Be(AgentType.EmailAssistant);
    }

    #endregion
}
