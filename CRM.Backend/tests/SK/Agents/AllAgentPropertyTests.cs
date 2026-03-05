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
using System.Collections.Generic;
using Xunit;

namespace CRM.Tests.SK.Agents;

/// <summary>
/// Tests for all 19 concrete CRM Semantic Kernel Agents.
/// Validates abstract property contracts: AgentName, AgentType, SystemPrompt,
/// AllowedPlugins, Temperature and MaxTokens defaults.
/// All agents only require (Kernel, ILogger) — no AI backend needed for property tests.
/// </summary>
public class AllAgentPropertyTests
{
    // ── helpers ────────────────────────────────────────────────────────
    private static Kernel CreateKernel() => Kernel.CreateBuilder().Build();

    private static ILogger<T> Logger<T>() => new Mock<ILogger<T>>().Object;

    // ── agent factories ────────────────────────────────────────────────
    private static ContractAnalystAgent MakeContractAnalyst() => new(CreateKernel(), Logger<ContractAnalystAgent>());
    private static CustomerSuccessAgent MakeCustomerSuccess() => new(CreateKernel(), Logger<CustomerSuccessAgent>());
    private static DataAnalystAgent MakeDataAnalyst() => new(CreateKernel(), Logger<DataAnalystAgent>());
    private static DealIntelligenceAgent MakeDealIntelligence() => new(CreateKernel(), Logger<DealIntelligenceAgent>());
    private static DocumentIntelligenceAgent MakeDocumentIntelligence() => new(CreateKernel(), Logger<DocumentIntelligenceAgent>());
    private static EmailAssistantAgent MakeEmailAssistant() => new(CreateKernel(), Logger<EmailAssistantAgent>());
    private static ForecastAnalystAgent MakeForecastAnalyst() => new(CreateKernel(), Logger<ForecastAnalystAgent>());
    private static GeneralAssistantAgent MakeGeneralAssistant() => new(CreateKernel(), Logger<GeneralAssistantAgent>());
    private static KnowledgeExpertAgent MakeKnowledgeExpert() => new(CreateKernel(), Logger<KnowledgeExpertAgent>());
    private static LeadScoringAgent MakeLeadScoring() => new(CreateKernel(), Logger<LeadScoringAgent>());
    private static MeetingIntelligenceAgent MakeMeetingIntelligence() => new(CreateKernel(), Logger<MeetingIntelligenceAgent>());
    private static NextBestActionAgent MakeNextBestAction() => new(CreateKernel(), Logger<NextBestActionAgent>());
    private static OnboardingGuideAgent MakeOnboardingGuide() => new(CreateKernel(), Logger<OnboardingGuideAgent>());
    private static RevenueIntelligenceAgent MakeRevenueIntelligence() => new(CreateKernel(), Logger<RevenueIntelligenceAgent>());
    private static SalesAssistantAgent MakeSalesAssistant() => new(CreateKernel(), Logger<SalesAssistantAgent>());
    private static SalesCoachAgent MakeSalesCoach() => new(CreateKernel(), Logger<SalesCoachAgent>());
    private static SalesIntelligenceAgent MakeSalesIntelligence() => new(CreateKernel(), Logger<SalesIntelligenceAgent>());
    private static SupportTriageAgent MakeSupportTriage() => new(CreateKernel(), Logger<SupportTriageAgent>());
    private static TicketResolutionAgent MakeTicketResolution() => new(CreateKernel(), Logger<TicketResolutionAgent>());

    // ══════════════════════════════════════════════════════════════════
    // 1. ContractAnalystAgent
    // ══════════════════════════════════════════════════════════════════
    [Fact]
    public void ContractAnalystAgent_AgentName_IsNotNullOrEmpty()
    {
        MakeContractAnalyst().AgentName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ContractAnalystAgent_AgentType_IsContractAnalyst()
    {
        MakeContractAnalyst().AgentType.Should().Be(AgentType.ContractAnalyst);
    }

    [Fact]
    public void ContractAnalystAgent_SystemPrompt_IsNotNullOrEmpty()
    {
        MakeContractAnalyst().SystemPrompt.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ContractAnalystAgent_AllowedPlugins_IsNotEmpty()
    {
        MakeContractAnalyst().AllowedPlugins.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ContractAnalystAgent_Temperature_IsInRange()
    {
        MakeContractAnalyst().Temperature.Should().BeInRange(0.0, 1.0);
    }

    [Fact]
    public void ContractAnalystAgent_MaxTokens_IsPositive()
    {
        MakeContractAnalyst().MaxTokens.Should().BeGreaterThan(0);
    }

    // ══════════════════════════════════════════════════════════════════
    // 2. CustomerSuccessAgent
    // ══════════════════════════════════════════════════════════════════
    [Fact]
    public void CustomerSuccessAgent_AgentName_IsNotNullOrEmpty()
    {
        MakeCustomerSuccess().AgentName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void CustomerSuccessAgent_AgentType_IsCustomerSuccess()
    {
        MakeCustomerSuccess().AgentType.Should().Be(AgentType.CustomerSuccess);
    }

    [Fact]
    public void CustomerSuccessAgent_SystemPrompt_IsNotNullOrEmpty()
    {
        MakeCustomerSuccess().SystemPrompt.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void CustomerSuccessAgent_AllowedPlugins_IsNotEmpty()
    {
        MakeCustomerSuccess().AllowedPlugins.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void CustomerSuccessAgent_Temperature_IsInRange()
    {
        MakeCustomerSuccess().Temperature.Should().BeInRange(0.0, 1.0);
    }

    [Fact]
    public void CustomerSuccessAgent_MaxTokens_IsPositive()
    {
        MakeCustomerSuccess().MaxTokens.Should().BeGreaterThan(0);
    }

    // ══════════════════════════════════════════════════════════════════
    // 3. DataAnalystAgent
    // ══════════════════════════════════════════════════════════════════
    [Fact]
    public void DataAnalystAgent_AgentName_IsNotNullOrEmpty()
    {
        MakeDataAnalyst().AgentName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void DataAnalystAgent_AgentType_IsDataAnalyst()
    {
        MakeDataAnalyst().AgentType.Should().Be(AgentType.DataAnalyst);
    }

    [Fact]
    public void DataAnalystAgent_SystemPrompt_IsNotNullOrEmpty()
    {
        MakeDataAnalyst().SystemPrompt.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void DataAnalystAgent_AllowedPlugins_IsNotEmpty()
    {
        MakeDataAnalyst().AllowedPlugins.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void DataAnalystAgent_Temperature_IsInRange()
    {
        MakeDataAnalyst().Temperature.Should().BeInRange(0.0, 1.0);
    }

    // ══════════════════════════════════════════════════════════════════
    // 4. DealIntelligenceAgent
    // ══════════════════════════════════════════════════════════════════
    [Fact]
    public void DealIntelligenceAgent_AgentName_IsNotNullOrEmpty()
    {
        MakeDealIntelligence().AgentName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void DealIntelligenceAgent_AgentType_IsDealIntelligence()
    {
        MakeDealIntelligence().AgentType.Should().Be(AgentType.DealIntelligence);
    }

    [Fact]
    public void DealIntelligenceAgent_SystemPrompt_IsNotNullOrEmpty()
    {
        MakeDealIntelligence().SystemPrompt.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void DealIntelligenceAgent_AllowedPlugins_IsNotEmpty()
    {
        MakeDealIntelligence().AllowedPlugins.Should().NotBeNullOrEmpty();
    }

    // ══════════════════════════════════════════════════════════════════
    // 5. DocumentIntelligenceAgent
    // ══════════════════════════════════════════════════════════════════
    [Fact]
    public void DocumentIntelligenceAgent_AgentName_IsNotNullOrEmpty()
    {
        MakeDocumentIntelligence().AgentName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void DocumentIntelligenceAgent_AgentType_IsDocumentIntelligence()
    {
        MakeDocumentIntelligence().AgentType.Should().Be(AgentType.DocumentIntelligence);
    }

    [Fact]
    public void DocumentIntelligenceAgent_SystemPrompt_IsNotNullOrEmpty()
    {
        MakeDocumentIntelligence().SystemPrompt.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void DocumentIntelligenceAgent_AllowedPlugins_IsNotEmpty()
    {
        MakeDocumentIntelligence().AllowedPlugins.Should().NotBeNullOrEmpty();
    }

    // ══════════════════════════════════════════════════════════════════
    // 6. EmailAssistantAgent
    // ══════════════════════════════════════════════════════════════════
    [Fact]
    public void EmailAssistantAgent_AgentName_IsNotNullOrEmpty()
    {
        MakeEmailAssistant().AgentName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void EmailAssistantAgent_AgentType_IsEmailAssistant()
    {
        MakeEmailAssistant().AgentType.Should().Be(AgentType.EmailAssistant);
    }

    [Fact]
    public void EmailAssistantAgent_SystemPrompt_IsNotNullOrEmpty()
    {
        MakeEmailAssistant().SystemPrompt.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void EmailAssistantAgent_AllowedPlugins_IsNotEmpty()
    {
        MakeEmailAssistant().AllowedPlugins.Should().NotBeNullOrEmpty();
    }

    // ══════════════════════════════════════════════════════════════════
    // 7. ForecastAnalystAgent
    // ══════════════════════════════════════════════════════════════════
    [Fact]
    public void ForecastAnalystAgent_AgentName_IsNotNullOrEmpty()
    {
        MakeForecastAnalyst().AgentName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ForecastAnalystAgent_AgentType_IsForecastAnalyst()
    {
        MakeForecastAnalyst().AgentType.Should().Be(AgentType.ForecastAnalyst);
    }

    [Fact]
    public void ForecastAnalystAgent_SystemPrompt_IsNotNullOrEmpty()
    {
        MakeForecastAnalyst().SystemPrompt.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ForecastAnalystAgent_AllowedPlugins_IsNotEmpty()
    {
        MakeForecastAnalyst().AllowedPlugins.Should().NotBeNullOrEmpty();
    }

    // ══════════════════════════════════════════════════════════════════
    // 8. GeneralAssistantAgent
    // ══════════════════════════════════════════════════════════════════
    [Fact]
    public void GeneralAssistantAgent_AgentName_IsNotNullOrEmpty()
    {
        MakeGeneralAssistant().AgentName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GeneralAssistantAgent_AgentType_IsGeneralAssistant()
    {
        MakeGeneralAssistant().AgentType.Should().Be(AgentType.GeneralAssistant);
    }

    [Fact]
    public void GeneralAssistantAgent_SystemPrompt_IsNotNullOrEmpty()
    {
        MakeGeneralAssistant().SystemPrompt.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GeneralAssistantAgent_AllowedPlugins_IsNotEmpty()
    {
        MakeGeneralAssistant().AllowedPlugins.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GeneralAssistantAgent_Temperature_UsesDefault()
    {
        // GeneralAssistant may use higher temperature — just check within bounds
        MakeGeneralAssistant().Temperature.Should().BeInRange(0.0, 1.0);
    }

    // ══════════════════════════════════════════════════════════════════
    // 9. KnowledgeExpertAgent
    // ══════════════════════════════════════════════════════════════════
    [Fact]
    public void KnowledgeExpertAgent_AgentName_IsNotNullOrEmpty()
    {
        MakeKnowledgeExpert().AgentName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void KnowledgeExpertAgent_AgentType_IsKnowledgeExpert()
    {
        MakeKnowledgeExpert().AgentType.Should().Be(AgentType.KnowledgeExpert);
    }

    [Fact]
    public void KnowledgeExpertAgent_SystemPrompt_IsNotNullOrEmpty()
    {
        MakeKnowledgeExpert().SystemPrompt.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void KnowledgeExpertAgent_AllowedPlugins_IsNotEmpty()
    {
        MakeKnowledgeExpert().AllowedPlugins.Should().NotBeNullOrEmpty();
    }

    // ══════════════════════════════════════════════════════════════════
    // 10. LeadScoringAgent
    // ══════════════════════════════════════════════════════════════════
    [Fact]
    public void LeadScoringAgent_AgentName_IsNotNullOrEmpty()
    {
        MakeLeadScoring().AgentName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void LeadScoringAgent_AgentType_IsLeadScoring()
    {
        MakeLeadScoring().AgentType.Should().Be(AgentType.LeadScoring);
    }

    [Fact]
    public void LeadScoringAgent_SystemPrompt_IsNotNullOrEmpty()
    {
        MakeLeadScoring().SystemPrompt.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void LeadScoringAgent_AllowedPlugins_ContainsLeadPlugin()
    {
        MakeLeadScoring().AllowedPlugins.Should().Contain("Lead");
    }

    [Fact]
    public void LeadScoringAgent_MaxTokens_IsAtLeast1000()
    {
        MakeLeadScoring().MaxTokens.Should().BeGreaterThanOrEqualTo(1000);
    }

    // ══════════════════════════════════════════════════════════════════
    // 11. MeetingIntelligenceAgent
    // ══════════════════════════════════════════════════════════════════
    [Fact]
    public void MeetingIntelligenceAgent_AgentName_IsNotNullOrEmpty()
    {
        MakeMeetingIntelligence().AgentName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void MeetingIntelligenceAgent_AgentType_IsMeetingIntelligence()
    {
        MakeMeetingIntelligence().AgentType.Should().Be(AgentType.MeetingIntelligence);
    }

    [Fact]
    public void MeetingIntelligenceAgent_SystemPrompt_IsNotNullOrEmpty()
    {
        MakeMeetingIntelligence().SystemPrompt.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void MeetingIntelligenceAgent_AllowedPlugins_IsNotEmpty()
    {
        MakeMeetingIntelligence().AllowedPlugins.Should().NotBeNullOrEmpty();
    }

    // ══════════════════════════════════════════════════════════════════
    // 12. NextBestActionAgent
    // ══════════════════════════════════════════════════════════════════
    [Fact]
    public void NextBestActionAgent_AgentName_IsNotNullOrEmpty()
    {
        MakeNextBestAction().AgentName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void NextBestActionAgent_AgentType_IsNextBestAction()
    {
        MakeNextBestAction().AgentType.Should().Be(AgentType.NextBestAction);
    }

    [Fact]
    public void NextBestActionAgent_SystemPrompt_IsNotNullOrEmpty()
    {
        MakeNextBestAction().SystemPrompt.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void NextBestActionAgent_AllowedPlugins_IsNotEmpty()
    {
        MakeNextBestAction().AllowedPlugins.Should().NotBeNullOrEmpty();
    }

    // ══════════════════════════════════════════════════════════════════
    // 13. OnboardingGuideAgent
    // ══════════════════════════════════════════════════════════════════
    [Fact]
    public void OnboardingGuideAgent_AgentName_IsNotNullOrEmpty()
    {
        MakeOnboardingGuide().AgentName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void OnboardingGuideAgent_AgentType_IsOnboardingGuide()
    {
        MakeOnboardingGuide().AgentType.Should().Be(AgentType.OnboardingGuide);
    }

    [Fact]
    public void OnboardingGuideAgent_SystemPrompt_IsNotNullOrEmpty()
    {
        MakeOnboardingGuide().SystemPrompt.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void OnboardingGuideAgent_AllowedPlugins_IsNotEmpty()
    {
        MakeOnboardingGuide().AllowedPlugins.Should().NotBeNullOrEmpty();
    }

    // ══════════════════════════════════════════════════════════════════
    // 14. RevenueIntelligenceAgent
    // ══════════════════════════════════════════════════════════════════
    [Fact]
    public void RevenueIntelligenceAgent_AgentName_IsNotNullOrEmpty()
    {
        MakeRevenueIntelligence().AgentName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void RevenueIntelligenceAgent_AgentType_IsRevenueIntelligence()
    {
        MakeRevenueIntelligence().AgentType.Should().Be(AgentType.RevenueIntelligence);
    }

    [Fact]
    public void RevenueIntelligenceAgent_SystemPrompt_IsNotNullOrEmpty()
    {
        MakeRevenueIntelligence().SystemPrompt.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void RevenueIntelligenceAgent_AllowedPlugins_IsNotEmpty()
    {
        MakeRevenueIntelligence().AllowedPlugins.Should().NotBeNullOrEmpty();
    }

    // ══════════════════════════════════════════════════════════════════
    // 15. SalesAssistantAgent
    // ══════════════════════════════════════════════════════════════════
    [Fact]
    public void SalesAssistantAgent_AgentName_IsNotNullOrEmpty()
    {
        MakeSalesAssistant().AgentName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void SalesAssistantAgent_AgentType_IsSalesAssistant()
    {
        MakeSalesAssistant().AgentType.Should().Be(AgentType.SalesAssistant);
    }

    [Fact]
    public void SalesAssistantAgent_SystemPrompt_IsNotNullOrEmpty()
    {
        MakeSalesAssistant().SystemPrompt.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void SalesAssistantAgent_AllowedPlugins_IsNotEmpty()
    {
        MakeSalesAssistant().AllowedPlugins.Should().NotBeNullOrEmpty();
    }

    // ══════════════════════════════════════════════════════════════════
    // 16. SalesCoachAgent
    // ══════════════════════════════════════════════════════════════════
    [Fact]
    public void SalesCoachAgent_AgentName_IsNotNullOrEmpty()
    {
        MakeSalesCoach().AgentName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void SalesCoachAgent_AgentType_IsSalesCoach()
    {
        MakeSalesCoach().AgentType.Should().Be(AgentType.SalesCoach);
    }

    [Fact]
    public void SalesCoachAgent_SystemPrompt_IsNotNullOrEmpty()
    {
        MakeSalesCoach().SystemPrompt.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void SalesCoachAgent_AllowedPlugins_IsNotEmpty()
    {
        MakeSalesCoach().AllowedPlugins.Should().NotBeNullOrEmpty();
    }

    // ══════════════════════════════════════════════════════════════════
    // 17. SalesIntelligenceAgent
    // ══════════════════════════════════════════════════════════════════
    [Fact]
    public void SalesIntelligenceAgent_AgentName_IsNotNullOrEmpty()
    {
        MakeSalesIntelligence().AgentName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void SalesIntelligenceAgent_AgentType_IsSalesIntelligence()
    {
        MakeSalesIntelligence().AgentType.Should().Be(AgentType.SalesIntelligence);
    }

    [Fact]
    public void SalesIntelligenceAgent_SystemPrompt_IsNotNullOrEmpty()
    {
        MakeSalesIntelligence().SystemPrompt.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void SalesIntelligenceAgent_AllowedPlugins_IsNotEmpty()
    {
        MakeSalesIntelligence().AllowedPlugins.Should().NotBeNullOrEmpty();
    }

    // ══════════════════════════════════════════════════════════════════
    // 18. SupportTriageAgent
    // ══════════════════════════════════════════════════════════════════
    [Fact]
    public void SupportTriageAgent_AgentName_IsNotNullOrEmpty()
    {
        MakeSupportTriage().AgentName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void SupportTriageAgent_AgentType_IsSupportTriage()
    {
        MakeSupportTriage().AgentType.Should().Be(AgentType.SupportTriage);
    }

    [Fact]
    public void SupportTriageAgent_SystemPrompt_IsNotNullOrEmpty()
    {
        MakeSupportTriage().SystemPrompt.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void SupportTriageAgent_AllowedPlugins_ContainsServiceRequest()
    {
        MakeSupportTriage().AllowedPlugins.Should().Contain("ServiceRequest");
    }

    [Fact]
    public void SupportTriageAgent_MaxTokens_IsAtLeast1000()
    {
        MakeSupportTriage().MaxTokens.Should().BeGreaterThanOrEqualTo(1000);
    }

    // ══════════════════════════════════════════════════════════════════
    // 19. TicketResolutionAgent
    // ══════════════════════════════════════════════════════════════════
    [Fact]
    public void TicketResolutionAgent_AgentName_IsNotNullOrEmpty()
    {
        MakeTicketResolution().AgentName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void TicketResolutionAgent_AgentType_IsTicketResolution()
    {
        MakeTicketResolution().AgentType.Should().Be(AgentType.TicketResolution);
    }

    [Fact]
    public void TicketResolutionAgent_SystemPrompt_IsNotNullOrEmpty()
    {
        MakeTicketResolution().SystemPrompt.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void TicketResolutionAgent_AllowedPlugins_IsNotEmpty()
    {
        MakeTicketResolution().AllowedPlugins.Should().NotBeNullOrEmpty();
    }

    // ══════════════════════════════════════════════════════════════════
    // 20. Cross-cutting: All agents must have valid defaults
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public void AllAgents_AgentNames_AreDistinct()
    {
        var names = new List<string>
        {
            MakeContractAnalyst().AgentName,
            MakeCustomerSuccess().AgentName,
            MakeDataAnalyst().AgentName,
            MakeDealIntelligence().AgentName,
            MakeDocumentIntelligence().AgentName,
            MakeEmailAssistant().AgentName,
            MakeForecastAnalyst().AgentName,
            MakeGeneralAssistant().AgentName,
            MakeKnowledgeExpert().AgentName,
            MakeLeadScoring().AgentName,
            MakeMeetingIntelligence().AgentName,
            MakeNextBestAction().AgentName,
            MakeOnboardingGuide().AgentName,
            MakeRevenueIntelligence().AgentName,
            MakeSalesAssistant().AgentName,
            MakeSalesCoach().AgentName,
            MakeSalesIntelligence().AgentName,
            MakeSupportTriage().AgentName,
            MakeTicketResolution().AgentName,
        };

        names.Should().OnlyHaveUniqueItems();
        names.Should().AllSatisfy(n => n.Should().NotBeNullOrEmpty());
    }

    [Fact]
    public void AllAgents_AgentTypes_AreDistinctEnumValues()
    {
        var types = new List<AgentType>
        {
            MakeContractAnalyst().AgentType,
            MakeCustomerSuccess().AgentType,
            MakeDataAnalyst().AgentType,
            MakeDealIntelligence().AgentType,
            MakeDocumentIntelligence().AgentType,
            MakeEmailAssistant().AgentType,
            MakeForecastAnalyst().AgentType,
            MakeGeneralAssistant().AgentType,
            MakeKnowledgeExpert().AgentType,
            MakeLeadScoring().AgentType,
            MakeMeetingIntelligence().AgentType,
            MakeNextBestAction().AgentType,
            MakeOnboardingGuide().AgentType,
            MakeRevenueIntelligence().AgentType,
            MakeSalesAssistant().AgentType,
            MakeSalesCoach().AgentType,
            MakeSalesIntelligence().AgentType,
            MakeSupportTriage().AgentType,
            MakeTicketResolution().AgentType,
        };

        types.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void AllAgents_AllowedPlugins_AreAllNonEmpty()
    {
        var allPluginLists = new[]
        {
            MakeContractAnalyst().AllowedPlugins,
            MakeCustomerSuccess().AllowedPlugins,
            MakeDataAnalyst().AllowedPlugins,
            MakeDealIntelligence().AllowedPlugins,
            MakeDocumentIntelligence().AllowedPlugins,
            MakeEmailAssistant().AllowedPlugins,
            MakeForecastAnalyst().AllowedPlugins,
            MakeGeneralAssistant().AllowedPlugins,
            MakeKnowledgeExpert().AllowedPlugins,
            MakeLeadScoring().AllowedPlugins,
            MakeMeetingIntelligence().AllowedPlugins,
            MakeNextBestAction().AllowedPlugins,
            MakeOnboardingGuide().AllowedPlugins,
            MakeRevenueIntelligence().AllowedPlugins,
            MakeSalesAssistant().AllowedPlugins,
            MakeSalesCoach().AllowedPlugins,
            MakeSalesIntelligence().AllowedPlugins,
            MakeSupportTriage().AllowedPlugins,
            MakeTicketResolution().AllowedPlugins,
        };

        foreach (var plugins in allPluginLists)
        {
            plugins.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public void AllAgents_Temperature_IsInValidRange()
    {
        var temperatures = new[]
        {
            MakeContractAnalyst().Temperature,
            MakeCustomerSuccess().Temperature,
            MakeDataAnalyst().Temperature,
            MakeDealIntelligence().Temperature,
            MakeGeneralAssistant().Temperature,
            MakeLeadScoring().Temperature,
            MakeSalesAssistant().Temperature,
            MakeSupportTriage().Temperature,
            MakeTicketResolution().Temperature,
        };

        foreach (var temp in temperatures)
        {
            temp.Should().BeInRange(0.0, 1.0);
        }
    }

    [Fact]
    public void AllAgents_MaxTokens_AreAllPositive()
    {
        var maxTokensValues = new[]
        {
            MakeContractAnalyst().MaxTokens,
            MakeCustomerSuccess().MaxTokens,
            MakeDataAnalyst().MaxTokens,
            MakeDealIntelligence().MaxTokens,
            MakeGeneralAssistant().MaxTokens,
            MakeLeadScoring().MaxTokens,
            MakeSalesAssistant().MaxTokens,
            MakeSupportTriage().MaxTokens,
            MakeTicketResolution().MaxTokens,
        };

        foreach (var tokens in maxTokensValues)
        {
            tokens.Should().BeGreaterThan(0);
        }
    }

    // ══════════════════════════════════════════════════════════════════
    // 21. EnrichContextAsync returns original message by default
    // ══════════════════════════════════════════════════════════════════
    [Fact]
    public async System.Threading.Tasks.Task GeneralAssistant_EnrichContextAsync_ReturnsOriginalMessage_ByDefault()
    {
        var agent = MakeGeneralAssistant();
        const string message = "What is the pipeline status?";

        var result = await agent.EnrichContextAsync(message, null, null);

        result.Should().Be(message);
    }

    [Fact]
    public async System.Threading.Tasks.Task LeadScoringAgent_EnrichContextAsync_ReturnsEnrichedOrOriginal()
    {
        var agent = MakeLeadScoring();
        const string message = "Score this lead";

        var result = await agent.EnrichContextAsync(message, "Lead", 1);

        result.Should().NotBeNullOrEmpty();
    }

    // ══════════════════════════════════════════════════════════════════
    // 22. Constructor null guard tests
    // ══════════════════════════════════════════════════════════════════
    [Fact]
    public void LeadScoringAgent_NullKernel_ThrowsArgumentNullException()
    {
        var act = () => new LeadScoringAgent(null!, Logger<LeadScoringAgent>());
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("kernel");
    }

    [Fact]
    public void SupportTriageAgent_NullLogger_ThrowsArgumentNullException()
    {
        var act = () => new SupportTriageAgent(CreateKernel(), null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public void GeneralAssistantAgent_NullKernel_ThrowsArgumentNullException()
    {
        var act = () => new GeneralAssistantAgent(null!, Logger<GeneralAssistantAgent>());
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("kernel");
    }
}
