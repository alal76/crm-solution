// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// Unit tests for all CRM Semantic Kernel Agent implementations.
// Tests verify agent properties, null guards, and base lifecycle hooks.
// No LLM calls are made — Kernel is created via Kernel.CreateBuilder().Build().
using CRM.Core.Entities.AI;
using CRM.Infrastructure.AI.SK.Agents;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Moq;
using Xunit;

namespace CRM.Tests.Services.SK;

/// <summary>
/// Unit tests for all CRM AI agent implementations. Verifies properties,
/// constructor null-guards, and base-class lifecycle hook behaviour.
/// </summary>
public sealed class AgentBaseTests
{
    // ---------------------------------------------------------------------------
    // Helper: create a minimal real Kernel (no AI connector needed for property tests)
    // ---------------------------------------------------------------------------
    private static Kernel BuildKernel() => Kernel.CreateBuilder().Build();

    // ---------------------------------------------------------------------------
    // 1. LeadScoringAgent
    // ---------------------------------------------------------------------------

    [Fact]
    public void LeadScoringAgent_Properties_ReturnExpectedValues()
    {
        var agent = new LeadScoringAgent(BuildKernel(), new Mock<ILogger<LeadScoringAgent>>().Object);

        agent.AgentName.Should().Be("Lead Scoring Agent");
        agent.AgentType.Should().Be(AgentType.LeadScoring);
        agent.Temperature.Should().Be(0.2);
        agent.MaxTokens.Should().Be(2048);
        agent.SystemPrompt.Should().NotBeNullOrWhiteSpace();
        agent.AllowedPlugins.Should().Contain("Lead");
        agent.AllowedPlugins.Should().Contain("Account");
        agent.AllowedPlugins.Should().Contain("Search");
        agent.AllowedPlugins.Should().NotBeEmpty();
    }

    [Fact]
    public void LeadScoringAgent_Constructor_ThrowsOnNullKernel()
    {
        var act = () => new LeadScoringAgent(null!, new Mock<ILogger<LeadScoringAgent>>().Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("kernel");
    }

    [Fact]
    public void LeadScoringAgent_Constructor_ThrowsOnNullLogger()
    {
        var act = () => new LeadScoringAgent(BuildKernel(), null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public async Task LeadScoringAgent_EnrichContextAsync_ReturnsOriginalMessage()
    {
        var agent = new LeadScoringAgent(BuildKernel(), new Mock<ILogger<LeadScoringAgent>>().Object);
        var result = await agent.EnrichContextAsync("test message", null, null);
        result.Should().Be("test message");
    }

    [Fact]
    public void LeadScoringAgent_CanHandle_ReturnsTrueForLead()
    {
        var agent = new LeadScoringAgent(BuildKernel(), new Mock<ILogger<LeadScoringAgent>>().Object);
        agent.CanHandle("Lead", null).Should().BeTrue();
        agent.CanHandle("lead", null).Should().BeTrue();
        agent.CanHandle("anything", null).Should().BeFalse();
    }

    // ---------------------------------------------------------------------------
    // 2. SupportTriageAgent
    // ---------------------------------------------------------------------------

    [Fact]
    public void SupportTriageAgent_Properties_ReturnExpectedValues()
    {
        var agent = new SupportTriageAgent(BuildKernel(), new Mock<ILogger<SupportTriageAgent>>().Object);

        agent.AgentName.Should().Be("Support Triage Agent");
        agent.AgentType.Should().Be(AgentType.SupportTriage);
        agent.Temperature.Should().Be(0.2);
        agent.MaxTokens.Should().Be(2048);
        agent.SystemPrompt.Should().NotBeNullOrWhiteSpace();
        agent.AllowedPlugins.Should().Contain("ServiceRequest");
        agent.AllowedPlugins.Should().Contain("KnowledgeBase");
    }

    [Fact]
    public void SupportTriageAgent_Constructor_ThrowsOnNullKernel()
    {
        var act = () => new SupportTriageAgent(null!, new Mock<ILogger<SupportTriageAgent>>().Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("kernel");
    }

    [Fact]
    public void SupportTriageAgent_Constructor_ThrowsOnNullLogger()
    {
        var act = () => new SupportTriageAgent(BuildKernel(), null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    // ---------------------------------------------------------------------------
    // 3. SalesAssistantAgent
    // ---------------------------------------------------------------------------

    [Fact]
    public void SalesAssistantAgent_Properties_ReturnExpectedValues()
    {
        var agent = new SalesAssistantAgent(BuildKernel(), new Mock<ILogger<SalesAssistantAgent>>().Object);

        agent.AgentName.Should().Be("Sales Assistant");
        agent.AgentType.Should().Be(AgentType.SalesAssistant);
        agent.Temperature.Should().Be(0.4);
        agent.MaxTokens.Should().Be(4096);
        agent.SystemPrompt.Should().NotBeNullOrWhiteSpace();
        agent.AllowedPlugins.Should().Contain("Opportunity");
        agent.AllowedPlugins.Should().Contain("Quote");
        agent.AllowedPlugins.Should().Contain("Lead");
    }

    [Fact]
    public void SalesAssistantAgent_Constructor_ThrowsOnNullKernel()
    {
        var act = () => new SalesAssistantAgent(null!, new Mock<ILogger<SalesAssistantAgent>>().Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("kernel");
    }

    [Fact]
    public void SalesAssistantAgent_Constructor_ThrowsOnNullLogger()
    {
        var act = () => new SalesAssistantAgent(BuildKernel(), null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    // ---------------------------------------------------------------------------
    // 4. GeneralAssistantAgent
    // ---------------------------------------------------------------------------

    [Fact]
    public void GeneralAssistantAgent_Properties_ReturnExpectedValues()
    {
        var agent = new GeneralAssistantAgent(BuildKernel(), new Mock<ILogger<GeneralAssistantAgent>>().Object);

        agent.AgentName.Should().Be("General Assistant");
        agent.AgentType.Should().Be(AgentType.GeneralAssistant);
        agent.Temperature.Should().Be(0.5);
        agent.MaxTokens.Should().Be(4096);
        agent.SystemPrompt.Should().NotBeNullOrWhiteSpace();
        agent.AllowedPlugins.Should().Contain("Account");
        agent.AllowedPlugins.Should().Contain("Calendar");
    }

    [Fact]
    public void GeneralAssistantAgent_Constructor_ThrowsOnNullKernel()
    {
        var act = () => new GeneralAssistantAgent(null!, new Mock<ILogger<GeneralAssistantAgent>>().Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("kernel");
    }

    [Fact]
    public void GeneralAssistantAgent_Constructor_ThrowsOnNullLogger()
    {
        var act = () => new GeneralAssistantAgent(BuildKernel(), null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    // ---------------------------------------------------------------------------
    // 5. ForecastAnalystAgent
    // ---------------------------------------------------------------------------

    [Fact]
    public void ForecastAnalystAgent_Properties_ReturnExpectedValues()
    {
        var agent = new ForecastAnalystAgent(BuildKernel(), new Mock<ILogger<ForecastAnalystAgent>>().Object);

        agent.AgentName.Should().Be("Forecast Analyst");
        agent.AgentType.Should().Be(AgentType.ForecastAnalyst);
        agent.Temperature.Should().Be(0.2);
        agent.MaxTokens.Should().Be(4096);
        agent.SystemPrompt.Should().NotBeNullOrWhiteSpace();
        agent.AllowedPlugins.Should().Contain("Opportunity");
        agent.AllowedPlugins.Should().Contain("Contract");
    }

    [Fact]
    public void ForecastAnalystAgent_Constructor_ThrowsOnNullKernel()
    {
        var act = () => new ForecastAnalystAgent(null!, new Mock<ILogger<ForecastAnalystAgent>>().Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("kernel");
    }

    [Fact]
    public void ForecastAnalystAgent_Constructor_ThrowsOnNullLogger()
    {
        var act = () => new ForecastAnalystAgent(BuildKernel(), null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    // ---------------------------------------------------------------------------
    // 6. DataAnalystAgent
    // ---------------------------------------------------------------------------

    [Fact]
    public void DataAnalystAgent_Properties_ReturnExpectedValues()
    {
        var agent = new DataAnalystAgent(BuildKernel(), new Mock<ILogger<DataAnalystAgent>>().Object);

        agent.AgentName.Should().Be("Data Analyst");
        agent.AgentType.Should().Be(AgentType.DataAnalyst);
        agent.Temperature.Should().Be(0.2);
        agent.MaxTokens.Should().Be(4096);
        agent.SystemPrompt.Should().NotBeNullOrWhiteSpace();
        agent.AllowedPlugins.Should().Contain("Lead");
        agent.AllowedPlugins.Should().Contain("Contract");
    }

    [Fact]
    public void DataAnalystAgent_Constructor_ThrowsOnNullKernel()
    {
        var act = () => new DataAnalystAgent(null!, new Mock<ILogger<DataAnalystAgent>>().Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("kernel");
    }

    [Fact]
    public void DataAnalystAgent_Constructor_ThrowsOnNullLogger()
    {
        var act = () => new DataAnalystAgent(BuildKernel(), null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    // ---------------------------------------------------------------------------
    // 7. ContractAnalystAgent
    // ---------------------------------------------------------------------------

    [Fact]
    public void ContractAnalystAgent_Properties_ReturnExpectedValues()
    {
        var agent = new ContractAnalystAgent(BuildKernel(), new Mock<ILogger<ContractAnalystAgent>>().Object);

        agent.AgentName.Should().Be("Contract Analyst");
        agent.AgentType.Should().Be(AgentType.ContractAnalyst);
        agent.Temperature.Should().Be(0.2);
        agent.MaxTokens.Should().Be(4096);
        agent.SystemPrompt.Should().NotBeNullOrWhiteSpace();
        agent.AllowedPlugins.Should().Contain("Contract");
        agent.AllowedPlugins.Should().Contain("Quote");
    }

    [Fact]
    public void ContractAnalystAgent_Constructor_ThrowsOnNullKernel()
    {
        var act = () => new ContractAnalystAgent(null!, new Mock<ILogger<ContractAnalystAgent>>().Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("kernel");
    }

    [Fact]
    public void ContractAnalystAgent_Constructor_ThrowsOnNullLogger()
    {
        var act = () => new ContractAnalystAgent(BuildKernel(), null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    // ---------------------------------------------------------------------------
    // 8. CustomerSuccessAgent
    // ---------------------------------------------------------------------------

    [Fact]
    public void CustomerSuccessAgent_Properties_ReturnExpectedValues()
    {
        var agent = new CustomerSuccessAgent(BuildKernel(), new Mock<ILogger<CustomerSuccessAgent>>().Object);

        agent.AgentName.Should().Be("Customer Success Agent");
        agent.AgentType.Should().Be(AgentType.CustomerSuccess);
        agent.Temperature.Should().Be(0.4);
        agent.MaxTokens.Should().Be(4096);
        agent.SystemPrompt.Should().NotBeNullOrWhiteSpace();
        agent.AllowedPlugins.Should().Contain("Contract");
        agent.AllowedPlugins.Should().Contain("ServiceRequest");
        agent.AllowedPlugins.Should().Contain("Calendar");
    }

    [Fact]
    public void CustomerSuccessAgent_Constructor_ThrowsOnNullKernel()
    {
        var act = () => new CustomerSuccessAgent(null!, new Mock<ILogger<CustomerSuccessAgent>>().Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("kernel");
    }

    [Fact]
    public void CustomerSuccessAgent_Constructor_ThrowsOnNullLogger()
    {
        var act = () => new CustomerSuccessAgent(BuildKernel(), null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    // ---------------------------------------------------------------------------
    // 9. DealIntelligenceAgent
    // ---------------------------------------------------------------------------

    [Fact]
    public void DealIntelligenceAgent_Properties_ReturnExpectedValues()
    {
        var agent = new DealIntelligenceAgent(BuildKernel(), new Mock<ILogger<DealIntelligenceAgent>>().Object);

        agent.AgentName.Should().Be("Deal Intelligence Agent");
        agent.AgentType.Should().Be(AgentType.DealIntelligence);
        agent.Temperature.Should().Be(0.3);
        agent.MaxTokens.Should().Be(4096);
        agent.SystemPrompt.Should().NotBeNullOrWhiteSpace();
        agent.AllowedPlugins.Should().Contain("Opportunity");
        agent.AllowedPlugins.Should().Contain("Contact");
        agent.AllowedPlugins.Should().Contain("Quote");
        agent.AllowedPlugins.Should().Contain("Contract");
    }

    [Fact]
    public void DealIntelligenceAgent_Constructor_ThrowsOnNullKernel()
    {
        var act = () => new DealIntelligenceAgent(null!, new Mock<ILogger<DealIntelligenceAgent>>().Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("kernel");
    }

    [Fact]
    public void DealIntelligenceAgent_Constructor_ThrowsOnNullLogger()
    {
        var act = () => new DealIntelligenceAgent(BuildKernel(), null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    // ---------------------------------------------------------------------------
    // 10. DocumentIntelligenceAgent
    // ---------------------------------------------------------------------------

    [Fact]
    public void DocumentIntelligenceAgent_Properties_ReturnExpectedValues()
    {
        var agent = new DocumentIntelligenceAgent(BuildKernel(), new Mock<ILogger<DocumentIntelligenceAgent>>().Object);

        agent.AgentName.Should().Be("Document Intelligence Agent");
        agent.AgentType.Should().Be(AgentType.DocumentIntelligence);
        agent.Temperature.Should().Be(0.2);
        agent.MaxTokens.Should().Be(4096);
        agent.SystemPrompt.Should().NotBeNullOrWhiteSpace();
        agent.AllowedPlugins.Should().Contain("Contract");
        agent.AllowedPlugins.Should().Contain("Quote");
    }

    [Fact]
    public void DocumentIntelligenceAgent_Constructor_ThrowsOnNullKernel()
    {
        var act = () => new DocumentIntelligenceAgent(null!, new Mock<ILogger<DocumentIntelligenceAgent>>().Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("kernel");
    }

    [Fact]
    public void DocumentIntelligenceAgent_Constructor_ThrowsOnNullLogger()
    {
        var act = () => new DocumentIntelligenceAgent(BuildKernel(), null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    // ---------------------------------------------------------------------------
    // 11. EmailAssistantAgent
    // ---------------------------------------------------------------------------

    [Fact]
    public void EmailAssistantAgent_Properties_ReturnExpectedValues()
    {
        var agent = new EmailAssistantAgent(BuildKernel(), new Mock<ILogger<EmailAssistantAgent>>().Object);

        agent.AgentName.Should().Be("Email Assistant");
        agent.AgentType.Should().Be(AgentType.EmailAssistant);
        agent.Temperature.Should().Be(0.6);
        agent.MaxTokens.Should().Be(4096);
        agent.SystemPrompt.Should().NotBeNullOrWhiteSpace();
        agent.AllowedPlugins.Should().Contain("Email");
        agent.AllowedPlugins.Should().Contain("Contact");
    }

    [Fact]
    public void EmailAssistantAgent_Constructor_ThrowsOnNullKernel()
    {
        var act = () => new EmailAssistantAgent(null!, new Mock<ILogger<EmailAssistantAgent>>().Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("kernel");
    }

    [Fact]
    public void EmailAssistantAgent_Constructor_ThrowsOnNullLogger()
    {
        var act = () => new EmailAssistantAgent(BuildKernel(), null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    // ---------------------------------------------------------------------------
    // 12. KnowledgeExpertAgent
    // ---------------------------------------------------------------------------

    [Fact]
    public void KnowledgeExpertAgent_Properties_ReturnExpectedValues()
    {
        var agent = new KnowledgeExpertAgent(BuildKernel(), new Mock<ILogger<KnowledgeExpertAgent>>().Object);

        agent.AgentName.Should().Be("Knowledge Expert");
        agent.AgentType.Should().Be(AgentType.KnowledgeExpert);
        agent.Temperature.Should().Be(0.3);
        agent.MaxTokens.Should().Be(4096);
        agent.SystemPrompt.Should().NotBeNullOrWhiteSpace();
        agent.AllowedPlugins.Should().Contain("KnowledgeBase");
        agent.AllowedPlugins.Should().Contain("ServiceRequest");
    }

    [Fact]
    public void KnowledgeExpertAgent_Constructor_ThrowsOnNullKernel()
    {
        var act = () => new KnowledgeExpertAgent(null!, new Mock<ILogger<KnowledgeExpertAgent>>().Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("kernel");
    }

    [Fact]
    public void KnowledgeExpertAgent_Constructor_ThrowsOnNullLogger()
    {
        var act = () => new KnowledgeExpertAgent(BuildKernel(), null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    // ---------------------------------------------------------------------------
    // 13. MeetingIntelligenceAgent
    // ---------------------------------------------------------------------------

    [Fact]
    public void MeetingIntelligenceAgent_Properties_ReturnExpectedValues()
    {
        var agent = new MeetingIntelligenceAgent(BuildKernel(), new Mock<ILogger<MeetingIntelligenceAgent>>().Object);

        agent.AgentName.Should().Be("Meeting Intelligence Agent");
        agent.AgentType.Should().Be(AgentType.MeetingIntelligence);
        agent.Temperature.Should().Be(0.4);
        agent.MaxTokens.Should().Be(4096);
        agent.SystemPrompt.Should().NotBeNullOrWhiteSpace();
        agent.AllowedPlugins.Should().Contain("Calendar");
        agent.AllowedPlugins.Should().Contain("Opportunity");
    }

    [Fact]
    public void MeetingIntelligenceAgent_Constructor_ThrowsOnNullKernel()
    {
        var act = () => new MeetingIntelligenceAgent(null!, new Mock<ILogger<MeetingIntelligenceAgent>>().Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("kernel");
    }

    [Fact]
    public void MeetingIntelligenceAgent_Constructor_ThrowsOnNullLogger()
    {
        var act = () => new MeetingIntelligenceAgent(BuildKernel(), null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    // ---------------------------------------------------------------------------
    // 14. NextBestActionAgent
    // ---------------------------------------------------------------------------

    [Fact]
    public void NextBestActionAgent_Properties_ReturnExpectedValues()
    {
        var agent = new NextBestActionAgent(BuildKernel(), new Mock<ILogger<NextBestActionAgent>>().Object);

        agent.AgentName.Should().Be("Next Best Action Agent");
        agent.AgentType.Should().Be(AgentType.NextBestAction);
        agent.Temperature.Should().Be(0.4);
        agent.MaxTokens.Should().Be(4096);
        agent.SystemPrompt.Should().NotBeNullOrWhiteSpace();
        agent.AllowedPlugins.Should().Contain("ServiceRequest");
        agent.AllowedPlugins.Should().Contain("Calendar");
    }

    [Fact]
    public void NextBestActionAgent_Constructor_ThrowsOnNullKernel()
    {
        var act = () => new NextBestActionAgent(null!, new Mock<ILogger<NextBestActionAgent>>().Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("kernel");
    }

    [Fact]
    public void NextBestActionAgent_Constructor_ThrowsOnNullLogger()
    {
        var act = () => new NextBestActionAgent(BuildKernel(), null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    // ---------------------------------------------------------------------------
    // 15. OnboardingGuideAgent
    // ---------------------------------------------------------------------------

    [Fact]
    public void OnboardingGuideAgent_Properties_ReturnExpectedValues()
    {
        var agent = new OnboardingGuideAgent(BuildKernel(), new Mock<ILogger<OnboardingGuideAgent>>().Object);

        agent.AgentName.Should().Be("Onboarding Guide");
        agent.AgentType.Should().Be(AgentType.OnboardingGuide);
        agent.Temperature.Should().Be(0.5);
        agent.MaxTokens.Should().Be(4096);
        agent.SystemPrompt.Should().NotBeNullOrWhiteSpace();
        agent.AllowedPlugins.Should().Contain("Lead");
        agent.AllowedPlugins.Should().Contain("Calendar");
    }

    [Fact]
    public void OnboardingGuideAgent_Constructor_ThrowsOnNullKernel()
    {
        var act = () => new OnboardingGuideAgent(null!, new Mock<ILogger<OnboardingGuideAgent>>().Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("kernel");
    }

    [Fact]
    public void OnboardingGuideAgent_Constructor_ThrowsOnNullLogger()
    {
        var act = () => new OnboardingGuideAgent(BuildKernel(), null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    // ---------------------------------------------------------------------------
    // 16. RevenueIntelligenceAgent
    // ---------------------------------------------------------------------------

    [Fact]
    public void RevenueIntelligenceAgent_Properties_ReturnExpectedValues()
    {
        var agent = new RevenueIntelligenceAgent(BuildKernel(), new Mock<ILogger<RevenueIntelligenceAgent>>().Object);

        agent.AgentName.Should().Be("Revenue Intelligence Agent");
        agent.AgentType.Should().Be(AgentType.RevenueIntelligence);
        agent.Temperature.Should().Be(0.2);
        agent.MaxTokens.Should().Be(4096);
        agent.SystemPrompt.Should().NotBeNullOrWhiteSpace();
        agent.AllowedPlugins.Should().Contain("Opportunity");
        agent.AllowedPlugins.Should().Contain("Contract");
        agent.AllowedPlugins.Should().Contain("Quote");
    }

    [Fact]
    public void RevenueIntelligenceAgent_Constructor_ThrowsOnNullKernel()
    {
        var act = () => new RevenueIntelligenceAgent(null!, new Mock<ILogger<RevenueIntelligenceAgent>>().Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("kernel");
    }

    [Fact]
    public void RevenueIntelligenceAgent_Constructor_ThrowsOnNullLogger()
    {
        var act = () => new RevenueIntelligenceAgent(BuildKernel(), null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    // ---------------------------------------------------------------------------
    // 17. SalesCoachAgent
    // ---------------------------------------------------------------------------

    [Fact]
    public void SalesCoachAgent_Properties_ReturnExpectedValues()
    {
        var agent = new SalesCoachAgent(BuildKernel(), new Mock<ILogger<SalesCoachAgent>>().Object);

        agent.AgentName.Should().Be("Sales Coach Agent");
        agent.AgentType.Should().Be(AgentType.SalesCoach);
        agent.Temperature.Should().Be(0.5);
        agent.MaxTokens.Should().Be(4096);
        agent.SystemPrompt.Should().NotBeNullOrWhiteSpace();
        agent.AllowedPlugins.Should().Contain("Opportunity");
        agent.AllowedPlugins.Should().Contain("Lead");
    }

    [Fact]
    public void SalesCoachAgent_Constructor_ThrowsOnNullKernel()
    {
        var act = () => new SalesCoachAgent(null!, new Mock<ILogger<SalesCoachAgent>>().Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("kernel");
    }

    [Fact]
    public void SalesCoachAgent_Constructor_ThrowsOnNullLogger()
    {
        var act = () => new SalesCoachAgent(BuildKernel(), null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    // ---------------------------------------------------------------------------
    // 18. SalesIntelligenceAgent
    // ---------------------------------------------------------------------------

    [Fact]
    public void SalesIntelligenceAgent_Properties_ReturnExpectedValues()
    {
        var agent = new SalesIntelligenceAgent(BuildKernel(), new Mock<ILogger<SalesIntelligenceAgent>>().Object);

        agent.AgentName.Should().Be("Sales Intelligence Agent");
        agent.AgentType.Should().Be(AgentType.SalesIntelligence);
        agent.Temperature.Should().Be(0.3);
        agent.MaxTokens.Should().Be(4096);
        agent.SystemPrompt.Should().NotBeNullOrWhiteSpace();
        agent.AllowedPlugins.Should().Contain("Contract");
        agent.AllowedPlugins.Should().Contain("Quote");
        agent.AllowedPlugins.Should().Contain("Lead");
    }

    [Fact]
    public void SalesIntelligenceAgent_Constructor_ThrowsOnNullKernel()
    {
        var act = () => new SalesIntelligenceAgent(null!, new Mock<ILogger<SalesIntelligenceAgent>>().Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("kernel");
    }

    [Fact]
    public void SalesIntelligenceAgent_Constructor_ThrowsOnNullLogger()
    {
        var act = () => new SalesIntelligenceAgent(BuildKernel(), null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    // ---------------------------------------------------------------------------
    // 19. TicketResolutionAgent
    // ---------------------------------------------------------------------------

    [Fact]
    public void TicketResolutionAgent_Properties_ReturnExpectedValues()
    {
        var agent = new TicketResolutionAgent(BuildKernel(), new Mock<ILogger<TicketResolutionAgent>>().Object);

        agent.AgentName.Should().Be("Ticket Resolution Agent");
        agent.AgentType.Should().Be(AgentType.TicketResolution);
        agent.Temperature.Should().Be(0.3);
        agent.MaxTokens.Should().Be(4096);
        agent.SystemPrompt.Should().NotBeNullOrWhiteSpace();
        agent.AllowedPlugins.Should().Contain("ServiceRequest");
        agent.AllowedPlugins.Should().Contain("KnowledgeBase");
        agent.AllowedPlugins.Should().Contain("Contact");
    }

    [Fact]
    public void TicketResolutionAgent_Constructor_ThrowsOnNullKernel()
    {
        var act = () => new TicketResolutionAgent(null!, new Mock<ILogger<TicketResolutionAgent>>().Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("kernel");
    }

    [Fact]
    public void TicketResolutionAgent_Constructor_ThrowsOnNullLogger()
    {
        var act = () => new TicketResolutionAgent(BuildKernel(), null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    // ---------------------------------------------------------------------------
    // Base lifecycle hooks — tested on GeneralAssistantAgent as a representative
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task BaseAgent_EnrichContextAsync_ReturnsOriginalUserMessage()
    {
        var agent = new GeneralAssistantAgent(BuildKernel(), new Mock<ILogger<GeneralAssistantAgent>>().Object);
        const string input = "show me my pipeline";

        var result = await agent.EnrichContextAsync(input, "Opportunity", 42);

        result.Should().Be(input);
    }

    [Fact]
    public async Task BaseAgent_PostProcessAsync_ReturnsOriginalAgentResponse()
    {
        var agent = new GeneralAssistantAgent(BuildKernel(), new Mock<ILogger<GeneralAssistantAgent>>().Object);
        const string response = "Here is your pipeline summary.";

        var result = await agent.PostProcessAsync(response);

        result.Should().Be(response);
    }

    [Fact]
    public void BaseAgent_CanHandle_ReturnsTrueForAnyEntityType()
    {
        var agent = new GeneralAssistantAgent(BuildKernel(), new Mock<ILogger<GeneralAssistantAgent>>().Object);

        agent.CanHandle("Account", null).Should().BeTrue();
        agent.CanHandle("Lead", null).Should().BeTrue();
        agent.CanHandle("", null).Should().BeTrue();
        agent.CanHandle("UnknownEntity", "some_intent").Should().BeTrue();
    }

    // ---------------------------------------------------------------------------
    // AgentType enum integrity
    // ---------------------------------------------------------------------------

    [Theory]
    [InlineData(AgentType.LeadScoring, 0)]
    [InlineData(AgentType.SupportTriage, 1)]
    [InlineData(AgentType.NextBestAction, 2)]
    [InlineData(AgentType.SalesIntelligence, 3)]
    [InlineData(AgentType.EmailAssistant, 4)]
    [InlineData(AgentType.CustomerSuccess, 5)]
    [InlineData(AgentType.RevenueIntelligence, 6)]
    [InlineData(AgentType.TicketResolution, 7)]
    [InlineData(AgentType.DocumentIntelligence, 8)]
    [InlineData(AgentType.SalesCoach, 9)]
    [InlineData(AgentType.MeetingIntelligence, 10)]
    [InlineData(AgentType.GeneralAssistant, 13)]
    [InlineData(AgentType.SalesAssistant, 14)]
    [InlineData(AgentType.DealIntelligence, 15)]
    [InlineData(AgentType.ForecastAnalyst, 16)]
    [InlineData(AgentType.DataAnalyst, 17)]
    [InlineData(AgentType.OnboardingGuide, 18)]
    [InlineData(AgentType.ContractAnalyst, 19)]
    [InlineData(AgentType.KnowledgeExpert, 20)]
    public void AgentType_EnumValues_AreCorrect(AgentType agentType, int expectedValue)
    {
        ((int)agentType).Should().Be(expectedValue);
    }

    [Fact]
    public void AllAgents_HaveNonEmptyAllowedPlugins()
    {
        var kernel = BuildKernel();
        var agents = new CrmAgentBase[]
        {
            new LeadScoringAgent(kernel, new Mock<ILogger<LeadScoringAgent>>().Object),
            new SupportTriageAgent(kernel, new Mock<ILogger<SupportTriageAgent>>().Object),
            new SalesAssistantAgent(kernel, new Mock<ILogger<SalesAssistantAgent>>().Object),
            new GeneralAssistantAgent(kernel, new Mock<ILogger<GeneralAssistantAgent>>().Object),
            new ForecastAnalystAgent(kernel, new Mock<ILogger<ForecastAnalystAgent>>().Object),
            new DataAnalystAgent(kernel, new Mock<ILogger<DataAnalystAgent>>().Object),
            new ContractAnalystAgent(kernel, new Mock<ILogger<ContractAnalystAgent>>().Object),
            new CustomerSuccessAgent(kernel, new Mock<ILogger<CustomerSuccessAgent>>().Object),
            new DealIntelligenceAgent(kernel, new Mock<ILogger<DealIntelligenceAgent>>().Object),
            new DocumentIntelligenceAgent(kernel, new Mock<ILogger<DocumentIntelligenceAgent>>().Object),
            new EmailAssistantAgent(kernel, new Mock<ILogger<EmailAssistantAgent>>().Object),
            new KnowledgeExpertAgent(kernel, new Mock<ILogger<KnowledgeExpertAgent>>().Object),
            new MeetingIntelligenceAgent(kernel, new Mock<ILogger<MeetingIntelligenceAgent>>().Object),
            new NextBestActionAgent(kernel, new Mock<ILogger<NextBestActionAgent>>().Object),
            new OnboardingGuideAgent(kernel, new Mock<ILogger<OnboardingGuideAgent>>().Object),
            new RevenueIntelligenceAgent(kernel, new Mock<ILogger<RevenueIntelligenceAgent>>().Object),
            new SalesCoachAgent(kernel, new Mock<ILogger<SalesCoachAgent>>().Object),
            new SalesIntelligenceAgent(kernel, new Mock<ILogger<SalesIntelligenceAgent>>().Object),
            new TicketResolutionAgent(kernel, new Mock<ILogger<TicketResolutionAgent>>().Object),
        };

        foreach (var agent in agents)
        {
            agent.AllowedPlugins.Should().NotBeEmpty(because: $"{agent.AgentName} should declare at least one plugin");
            agent.SystemPrompt.Should().NotBeNullOrWhiteSpace(because: $"{agent.AgentName} must have a system prompt");
            agent.AgentName.Should().NotBeNullOrWhiteSpace(because: "AgentName must not be empty");
        }
    }

    [Fact]
    public void AllAgents_HaveUniqueAgentNames()
    {
        var kernel = BuildKernel();
        var agents = new CrmAgentBase[]
        {
            new LeadScoringAgent(kernel, new Mock<ILogger<LeadScoringAgent>>().Object),
            new SupportTriageAgent(kernel, new Mock<ILogger<SupportTriageAgent>>().Object),
            new SalesAssistantAgent(kernel, new Mock<ILogger<SalesAssistantAgent>>().Object),
            new GeneralAssistantAgent(kernel, new Mock<ILogger<GeneralAssistantAgent>>().Object),
            new ForecastAnalystAgent(kernel, new Mock<ILogger<ForecastAnalystAgent>>().Object),
            new DataAnalystAgent(kernel, new Mock<ILogger<DataAnalystAgent>>().Object),
            new ContractAnalystAgent(kernel, new Mock<ILogger<ContractAnalystAgent>>().Object),
            new CustomerSuccessAgent(kernel, new Mock<ILogger<CustomerSuccessAgent>>().Object),
            new DealIntelligenceAgent(kernel, new Mock<ILogger<DealIntelligenceAgent>>().Object),
            new DocumentIntelligenceAgent(kernel, new Mock<ILogger<DocumentIntelligenceAgent>>().Object),
            new EmailAssistantAgent(kernel, new Mock<ILogger<EmailAssistantAgent>>().Object),
            new KnowledgeExpertAgent(kernel, new Mock<ILogger<KnowledgeExpertAgent>>().Object),
            new MeetingIntelligenceAgent(kernel, new Mock<ILogger<MeetingIntelligenceAgent>>().Object),
            new NextBestActionAgent(kernel, new Mock<ILogger<NextBestActionAgent>>().Object),
            new OnboardingGuideAgent(kernel, new Mock<ILogger<OnboardingGuideAgent>>().Object),
            new RevenueIntelligenceAgent(kernel, new Mock<ILogger<RevenueIntelligenceAgent>>().Object),
            new SalesCoachAgent(kernel, new Mock<ILogger<SalesCoachAgent>>().Object),
            new SalesIntelligenceAgent(kernel, new Mock<ILogger<SalesIntelligenceAgent>>().Object),
            new TicketResolutionAgent(kernel, new Mock<ILogger<TicketResolutionAgent>>().Object),
        };

        var names = agents.Select(a => a.AgentName).ToList();
        names.Should().OnlyHaveUniqueItems(because: "each agent must have a unique AgentName");
    }

    [Fact]
    public void AllAgents_HaveUniqueAgentTypes()
    {
        var kernel = BuildKernel();
        var agents = new CrmAgentBase[]
        {
            new LeadScoringAgent(kernel, new Mock<ILogger<LeadScoringAgent>>().Object),
            new SupportTriageAgent(kernel, new Mock<ILogger<SupportTriageAgent>>().Object),
            new SalesAssistantAgent(kernel, new Mock<ILogger<SalesAssistantAgent>>().Object),
            new GeneralAssistantAgent(kernel, new Mock<ILogger<GeneralAssistantAgent>>().Object),
            new ForecastAnalystAgent(kernel, new Mock<ILogger<ForecastAnalystAgent>>().Object),
            new DataAnalystAgent(kernel, new Mock<ILogger<DataAnalystAgent>>().Object),
            new ContractAnalystAgent(kernel, new Mock<ILogger<ContractAnalystAgent>>().Object),
            new CustomerSuccessAgent(kernel, new Mock<ILogger<CustomerSuccessAgent>>().Object),
            new DealIntelligenceAgent(kernel, new Mock<ILogger<DealIntelligenceAgent>>().Object),
            new DocumentIntelligenceAgent(kernel, new Mock<ILogger<DocumentIntelligenceAgent>>().Object),
            new EmailAssistantAgent(kernel, new Mock<ILogger<EmailAssistantAgent>>().Object),
            new KnowledgeExpertAgent(kernel, new Mock<ILogger<KnowledgeExpertAgent>>().Object),
            new MeetingIntelligenceAgent(kernel, new Mock<ILogger<MeetingIntelligenceAgent>>().Object),
            new NextBestActionAgent(kernel, new Mock<ILogger<NextBestActionAgent>>().Object),
            new OnboardingGuideAgent(kernel, new Mock<ILogger<OnboardingGuideAgent>>().Object),
            new RevenueIntelligenceAgent(kernel, new Mock<ILogger<RevenueIntelligenceAgent>>().Object),
            new SalesCoachAgent(kernel, new Mock<ILogger<SalesCoachAgent>>().Object),
            new SalesIntelligenceAgent(kernel, new Mock<ILogger<SalesIntelligenceAgent>>().Object),
            new TicketResolutionAgent(kernel, new Mock<ILogger<TicketResolutionAgent>>().Object),
        };

        var types = agents.Select(a => a.AgentType).ToList();
        types.Should().OnlyHaveUniqueItems(because: "each agent must have a unique AgentType");
    }
}
