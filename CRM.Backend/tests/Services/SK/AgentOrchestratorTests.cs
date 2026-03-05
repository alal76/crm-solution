// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// Unit tests for AgentOrchestrator and AgentSelectionStrategy.
using CRM.Core.Entities.AI;
using CRM.Core.Interfaces;
using CRM.Infrastructure.AI.SK.Agents;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Moq;
using Xunit;

namespace CRM.Tests.Services.SK;

/// <summary>
/// Unit tests for <see cref="AgentOrchestrator"/> and <see cref="AgentSelectionStrategy"/>.
/// Covers constructor null-guards, routing logic, and intent/entity detection helpers.
/// No LLM calls are made — all agents are constructed with a headless Kernel.
/// </summary>
public sealed class AgentOrchestratorTests
{
    // ---------------------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------------------

    private static Kernel BuildKernel() => Kernel.CreateBuilder().Build();

    private static ICrmDbContext CreateMockDbContext() =>
        new Mock<ICrmDbContext>().Object;

    private static ILogger<AgentOrchestrator> CreateLogger() =>
        new Mock<ILogger<AgentOrchestrator>>().Object;

    // Build a real IServiceProvider that registers all 19 agents.
    private static IServiceProvider BuildServiceProviderWithAgents()
    {
        var services = new ServiceCollection();
        var kernel = BuildKernel();

        services.AddSingleton<CrmAgentBase>(new LeadScoringAgent(kernel, new Mock<ILogger<LeadScoringAgent>>().Object));
        services.AddSingleton<CrmAgentBase>(new SupportTriageAgent(kernel, new Mock<ILogger<SupportTriageAgent>>().Object));
        services.AddSingleton<CrmAgentBase>(new SalesAssistantAgent(kernel, new Mock<ILogger<SalesAssistantAgent>>().Object));
        services.AddSingleton<CrmAgentBase>(new GeneralAssistantAgent(kernel, new Mock<ILogger<GeneralAssistantAgent>>().Object));
        services.AddSingleton<CrmAgentBase>(new ForecastAnalystAgent(kernel, new Mock<ILogger<ForecastAnalystAgent>>().Object));
        services.AddSingleton<CrmAgentBase>(new DataAnalystAgent(kernel, new Mock<ILogger<DataAnalystAgent>>().Object));
        services.AddSingleton<CrmAgentBase>(new ContractAnalystAgent(kernel, new Mock<ILogger<ContractAnalystAgent>>().Object));
        services.AddSingleton<CrmAgentBase>(new CustomerSuccessAgent(kernel, new Mock<ILogger<CustomerSuccessAgent>>().Object));
        services.AddSingleton<CrmAgentBase>(new DealIntelligenceAgent(kernel, new Mock<ILogger<DealIntelligenceAgent>>().Object));
        services.AddSingleton<CrmAgentBase>(new DocumentIntelligenceAgent(kernel, new Mock<ILogger<DocumentIntelligenceAgent>>().Object));
        services.AddSingleton<CrmAgentBase>(new EmailAssistantAgent(kernel, new Mock<ILogger<EmailAssistantAgent>>().Object));
        services.AddSingleton<CrmAgentBase>(new KnowledgeExpertAgent(kernel, new Mock<ILogger<KnowledgeExpertAgent>>().Object));
        services.AddSingleton<CrmAgentBase>(new MeetingIntelligenceAgent(kernel, new Mock<ILogger<MeetingIntelligenceAgent>>().Object));
        services.AddSingleton<CrmAgentBase>(new NextBestActionAgent(kernel, new Mock<ILogger<NextBestActionAgent>>().Object));
        services.AddSingleton<CrmAgentBase>(new OnboardingGuideAgent(kernel, new Mock<ILogger<OnboardingGuideAgent>>().Object));
        services.AddSingleton<CrmAgentBase>(new RevenueIntelligenceAgent(kernel, new Mock<ILogger<RevenueIntelligenceAgent>>().Object));
        services.AddSingleton<CrmAgentBase>(new SalesCoachAgent(kernel, new Mock<ILogger<SalesCoachAgent>>().Object));
        services.AddSingleton<CrmAgentBase>(new SalesIntelligenceAgent(kernel, new Mock<ILogger<SalesIntelligenceAgent>>().Object));
        services.AddSingleton<CrmAgentBase>(new TicketResolutionAgent(kernel, new Mock<ILogger<TicketResolutionAgent>>().Object));

        return services.BuildServiceProvider();
    }

    // Build a service provider with only GeneralAssistantAgent (for fallback tests).
    private static IServiceProvider BuildServiceProviderWithGeneralAssistant()
    {
        var services = new ServiceCollection();
        var kernel = BuildKernel();
        services.AddSingleton<CrmAgentBase>(new GeneralAssistantAgent(kernel, new Mock<ILogger<GeneralAssistantAgent>>().Object));
        return services.BuildServiceProvider();
    }

    // Build an empty service provider (no agents registered).
    private static IServiceProvider BuildEmptyServiceProvider() =>
        new ServiceCollection().BuildServiceProvider();

    // ---------------------------------------------------------------------------
    // Constructor null-guard tests
    // ---------------------------------------------------------------------------

    [Fact]
    public void Constructor_ShouldThrow_WhenServiceProviderIsNull()
    {
        var act = () => new AgentOrchestrator(
            null!,
            CreateMockDbContext(),
            CreateLogger());

        act.Should().Throw<ArgumentNullException>().WithParameterName("serviceProvider");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenDbContextIsNull()
    {
        var act = () => new AgentOrchestrator(
            BuildEmptyServiceProvider(),
            null!,
            CreateLogger());

        act.Should().Throw<ArgumentNullException>().WithParameterName("dbContext");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenLoggerIsNull()
    {
        var act = () => new AgentOrchestrator(
            BuildEmptyServiceProvider(),
            CreateMockDbContext(),
            null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public void Constructor_ShouldSucceed_WhenAllDependenciesProvided()
    {
        var act = () => new AgentOrchestrator(
            BuildEmptyServiceProvider(),
            CreateMockDbContext(),
            CreateLogger());

        act.Should().NotThrow();
    }

    // ---------------------------------------------------------------------------
    // RouteToAgentAsync — no agents registered
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task RouteToAgentAsync_ShouldThrow_WhenNoAgentsRegistered()
    {
        var orchestrator = new AgentOrchestrator(
            BuildEmptyServiceProvider(),
            CreateMockDbContext(),
            CreateLogger());

        var act = async () => await orchestrator.RouteToAgentAsync("hello");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No AI agents are registered*");
    }

    // ---------------------------------------------------------------------------
    // RouteToAgentAsync — fallback to GeneralAssistant
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task RouteToAgentAsync_ShouldReturnGeneralAssistantAgent_WhenOnlyGeneralAssistantIsRegistered()
    {
        var orchestrator = new AgentOrchestrator(
            BuildServiceProviderWithGeneralAssistant(),
            CreateMockDbContext(),
            CreateLogger());

        var agent = await orchestrator.RouteToAgentAsync("hello, what can you do?");

        agent.Should().BeOfType<GeneralAssistantAgent>();
    }

    [Fact]
    public async Task RouteToAgentAsync_ShouldReturnGeneralAssistantAgent_WhenMessageHasNoRecognisedKeywords()
    {
        var orchestrator = new AgentOrchestrator(
            BuildServiceProviderWithAgents(),
            CreateMockDbContext(),
            CreateLogger());

        // Message with no action/entity keywords → falls back to GeneralAssistant
        var agent = await orchestrator.RouteToAgentAsync("something completely unrelated xyz 12345");

        agent.Should().BeOfType<GeneralAssistantAgent>();
    }

    // ---------------------------------------------------------------------------
    // RouteToAgentAsync — specialized agent selection
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task RouteToAgentAsync_ShouldReturnLeadScoringAgent_WhenScoreLeadKeywordInMessage()
    {
        var orchestrator = new AgentOrchestrator(
            BuildServiceProviderWithAgents(),
            CreateMockDbContext(),
            CreateLogger());

        var agent = await orchestrator.RouteToAgentAsync("can you score lead for me?");

        agent.Should().BeOfType<LeadScoringAgent>();
    }

    [Fact]
    public async Task RouteToAgentAsync_ShouldReturnSupportTriageAgent_WhenTicketKeywordInMessage()
    {
        var orchestrator = new AgentOrchestrator(
            BuildServiceProviderWithAgents(),
            CreateMockDbContext(),
            CreateLogger());

        var agent = await orchestrator.RouteToAgentAsync("I have a support ticket issue");

        agent.Should().BeOfType<SupportTriageAgent>();
    }

    [Fact]
    public async Task RouteToAgentAsync_ShouldReturnForecastAnalystAgent_WhenForecastKeywordInMessage()
    {
        var orchestrator = new AgentOrchestrator(
            BuildServiceProviderWithAgents(),
            CreateMockDbContext(),
            CreateLogger());

        var agent = await orchestrator.RouteToAgentAsync("show me the sales forecast for Q3");

        agent.Should().BeOfType<ForecastAnalystAgent>();
    }

    [Fact]
    public async Task RouteToAgentAsync_ShouldReturnEmailAssistantAgent_WhenEmailKeywordInMessage()
    {
        var orchestrator = new AgentOrchestrator(
            BuildServiceProviderWithAgents(),
            CreateMockDbContext(),
            CreateLogger());

        var agent = await orchestrator.RouteToAgentAsync("draft me an email follow-up");

        agent.Should().BeOfType<EmailAssistantAgent>();
    }

    [Fact]
    public async Task RouteToAgentAsync_ShouldReturnContractAnalystAgent_WhenRenewalKeywordInMessage()
    {
        var orchestrator = new AgentOrchestrator(
            BuildServiceProviderWithAgents(),
            CreateMockDbContext(),
            CreateLogger());

        var agent = await orchestrator.RouteToAgentAsync("help with contract renewal terms");

        agent.Should().BeOfType<ContractAnalystAgent>();
    }

    [Fact]
    public async Task RouteToAgentAsync_ShouldReturnKnowledgeExpertAgent_WhenKnowledgeKeywordInMessage()
    {
        var orchestrator = new AgentOrchestrator(
            BuildServiceProviderWithAgents(),
            CreateMockDbContext(),
            CreateLogger());

        var agent = await orchestrator.RouteToAgentAsync("search knowledge base articles");

        agent.Should().BeOfType<KnowledgeExpertAgent>();
    }

    [Fact]
    public async Task RouteToAgentAsync_ShouldReturnDataAnalystAgent_WhenDataKeywordInMessage()
    {
        var orchestrator = new AgentOrchestrator(
            BuildServiceProviderWithAgents(),
            CreateMockDbContext(),
            CreateLogger());

        var agent = await orchestrator.RouteToAgentAsync("generate analytics report for management");

        agent.Should().BeOfType<DataAnalystAgent>();
    }

    [Fact]
    public async Task RouteToAgentAsync_ShouldHandleDealHealthKeyword_WithSalesOrDealAgent()
    {
        var orchestrator = new AgentOrchestrator(
            BuildServiceProviderWithAgents(),
            CreateMockDbContext(),
            CreateLogger());

        var agent = await orchestrator.RouteToAgentAsync("assess deal health and win probability");

        // Both SalesAssistantAgent and DealIntelligenceAgent handle "deal" entity type.
        // SalesAssistantAgent is registered first in DI so it takes precedence.
        agent.Should().BeAssignableTo<CrmAgentBase>();
        agent.AgentType.Should().BeOneOf(AgentType.SalesAssistant, AgentType.DealIntelligence);
    }

    [Fact]
    public async Task RouteToAgentAsync_ShouldReturnCustomerSuccessAgent_WhenChurnKeywordInMessage()
    {
        var orchestrator = new AgentOrchestrator(
            BuildServiceProviderWithAgents(),
            CreateMockDbContext(),
            CreateLogger());

        var agent = await orchestrator.RouteToAgentAsync("customer at risk of churn, need help");

        agent.Should().BeOfType<CustomerSuccessAgent>();
    }

    // ---------------------------------------------------------------------------
    // AgentSelectionStrategy — DetectIntent
    // ---------------------------------------------------------------------------

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AgentSelectionStrategy_DetectIntent_ShouldReturnNull_WhenMessageIsNullOrWhitespace(string? message)
    {
        var result = AgentSelectionStrategy.DetectIntent(message!);
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("score lead for me", "lead")]
    [InlineData("qualify lead", "lead")]
    [InlineData("draft email follow-up for customer", "email")]
    [InlineData("write email", "email")]
    [InlineData("sales forecast for quarter", "forecast")]
    [InlineData("support ticket resolution", "support")]
    [InlineData("contract renewal coming up", "contract")]
    [InlineData("knowledge base search", "knowledge")]
    [InlineData("how many deals in pipeline", "data")]
    [InlineData("total revenue report", "data")]
    [InlineData("customer onboarding", "onboarding")]
    [InlineData("churn risk assessment", "churn")]
    [InlineData("deal health check", "deal")]
    public void AgentSelectionStrategy_DetectIntent_ShouldReturnExpectedIntent_WhenKeywordPresent(
        string message, string expectedIntent)
    {
        var result = AgentSelectionStrategy.DetectIntent(message);
        result.Should().Be(expectedIntent);
    }

    [Fact]
    public void AgentSelectionStrategy_DetectIntent_ShouldReturnNull_WhenNoKeywordMatches()
    {
        var result = AgentSelectionStrategy.DetectIntent("hello world nothing special here");
        result.Should().BeNull();
    }

    // ---------------------------------------------------------------------------
    // AgentSelectionStrategy — DetectEntityType
    // ---------------------------------------------------------------------------

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AgentSelectionStrategy_DetectEntityType_ShouldReturnNull_WhenMessageIsNullOrWhitespace(string? message)
    {
        var result = AgentSelectionStrategy.DetectEntityType(message!);
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("tell me about the lead pipeline", "lead")]
    [InlineData("update opportunity details", "opportunity")]
    [InlineData("view account information", "account")]
    [InlineData("contact me via email", "contact")]
    [InlineData("this is a service request", "servicerequest")]
    [InlineData("contract renewal is due", "contract")]
    [InlineData("create a new quote", "quote")]
    [InlineData("knowledge article search", "knowledge")]
    [InlineData("email campaign results", "email")]
    public void AgentSelectionStrategy_DetectEntityType_ShouldReturnExpectedEntityType_WhenKeywordPresent(
        string message, string expectedEntityType)
    {
        var result = AgentSelectionStrategy.DetectEntityType(message);
        result.Should().Be(expectedEntityType);
    }

    [Fact]
    public void AgentSelectionStrategy_DetectEntityType_ShouldReturnNull_WhenNoKeywordMatches()
    {
        var result = AgentSelectionStrategy.DetectEntityType("something completely different");
        result.Should().BeNull();
    }

    // ---------------------------------------------------------------------------
    // AgentSelectionStrategy — RecommendAgent
    // ---------------------------------------------------------------------------

    [Theory]
    [InlineData(null, null, AgentType.GeneralAssistant)]
    [InlineData("", null, AgentType.GeneralAssistant)]
    [InlineData("lead", null, AgentType.LeadScoring)]
    [InlineData("email", null, AgentType.EmailAssistant)]
    [InlineData("support", null, AgentType.SupportTriage)]
    [InlineData("ticket", null, AgentType.SupportTriage)]
    [InlineData("servicerequest", null, AgentType.SupportTriage)]
    [InlineData("forecast", null, AgentType.ForecastAnalyst)]
    [InlineData("contract", null, AgentType.ContractAnalyst)]
    [InlineData("renewal", null, AgentType.ContractAnalyst)]
    [InlineData("knowledge", null, AgentType.KnowledgeExpert)]
    [InlineData("kb", null, AgentType.KnowledgeExpert)]
    [InlineData("article", null, AgentType.KnowledgeExpert)]
    [InlineData("onboarding", null, AgentType.OnboardingGuide)]
    [InlineData("help", null, AgentType.OnboardingGuide)]
    [InlineData("data", null, AgentType.DataAnalyst)]
    [InlineData("report", null, AgentType.DataAnalyst)]
    [InlineData("analytics", null, AgentType.DataAnalyst)]
    [InlineData("deal", null, AgentType.DealIntelligence)]
    [InlineData("opportunity", null, AgentType.SalesAssistant)]
    [InlineData("quote", null, AgentType.SalesAssistant)]
    [InlineData("account", null, AgentType.CustomerSuccess)]
    [InlineData("customer", null, AgentType.CustomerSuccess)]
    [InlineData("churn", null, AgentType.CustomerSuccess)]
    public void AgentSelectionStrategy_RecommendAgent_ShouldReturnExpectedType(
        string? entityType, string? intent, AgentType expectedType)
    {
        var result = AgentSelectionStrategy.RecommendAgent(entityType, intent);
        result.Should().Be(expectedType);
    }

    [Fact]
    public void AgentSelectionStrategy_RecommendAgent_IntentTakesPriorityOverEntityType()
    {
        // Even if entityType says "account" (→ CustomerSuccess),
        // intent "lead" (→ LeadScoring) should win when both are provided.
        // The implementation uses: primarySignal = intent ?? entityType
        var result = AgentSelectionStrategy.RecommendAgent("account", "lead");
        result.Should().Be(AgentType.LeadScoring);
    }
}
