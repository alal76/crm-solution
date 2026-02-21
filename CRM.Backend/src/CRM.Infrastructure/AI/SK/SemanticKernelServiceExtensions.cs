// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
#nullable enable

using CRM.Infrastructure.AI.SK.Agents;
using CRM.Infrastructure.AI.SK.Configuration;
using CRM.Infrastructure.AI.SK.Connectors;
using CRM.Infrastructure.AI.SK.Filters;
using CRM.Infrastructure.AI.SK.Plugins;
using CRM.Infrastructure.AI.SK.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CRM.Infrastructure.AI.SK;

/// <summary>
/// Extension methods for registering all Semantic Kernel AI services,
/// connectors, plugins, agents, and filters with the dependency injection container.
/// </summary>
public static class SemanticKernelServiceExtensions
{
    /// <summary>
    /// Adds all Semantic Kernel AI services to the service collection.
    /// This includes configuration binding, connectors, plugins, agents,
    /// orchestration services, and execution filters.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    /// <param name="configuration">The application configuration for binding options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSemanticKernel(
        this IServiceCollection services,
        IConfiguration configuration)
    {

        // Bind SemanticKernel configuration section to options
        services.Configure<SemanticKernelOptions>(configuration.GetSection("SemanticKernel"));



        // Register kernel factory and LLM connectors
        services.AddScoped<CrmKernelFactory>();
        services.AddScoped<CrmChatCompletionConnector>();
        services.AddScoped<CrmEmbeddingConnector>();



        // Register all 12 CRM domain plugins
        services.AddScoped<AccountPlugin>();
        services.AddScoped<ContactPlugin>();
        services.AddScoped<OpportunityPlugin>();
        services.AddScoped<LeadPlugin>();
        services.AddScoped<ServiceRequestPlugin>();
        services.AddScoped<EmailPlugin>();
        services.AddScoped<KnowledgeBasePlugin>();
        services.AddScoped<SearchPlugin>();
        services.AddScoped<CalendarPlugin>();
        services.AddScoped<NotificationPlugin>();
        services.AddScoped<QuotePlugin>();
        services.AddScoped<ContractPlugin>();



        // Register all 12 specialized CRM agents as CrmAgentBase for orchestrator resolution
        services.AddScoped<CrmAgentBase, GeneralAssistantAgent>();
        services.AddScoped<CrmAgentBase, SalesAssistantAgent>();
        services.AddScoped<CrmAgentBase, LeadScoringAgent>();
        services.AddScoped<CrmAgentBase, SupportTriageAgent>();
        services.AddScoped<CrmAgentBase, DealIntelligenceAgent>();
        services.AddScoped<CrmAgentBase, EmailAssistantAgent>();
        services.AddScoped<CrmAgentBase, DataAnalystAgent>();
        services.AddScoped<CrmAgentBase, OnboardingGuideAgent>();
        services.AddScoped<CrmAgentBase, ForecastAnalystAgent>();
        services.AddScoped<CrmAgentBase, CustomerSuccessAgent>();
        services.AddScoped<CrmAgentBase, ContractAnalystAgent>();
        services.AddScoped<CrmAgentBase, KnowledgeExpertAgent>();



        // Register orchestrator for multi-agent routing and execution
        services.AddScoped<AgentOrchestrator>();
        services.AddScoped<AgentExecutionService>();



        // Register SK execution filters for cross-cutting concerns
        services.AddScoped<AuditLoggingFilter>();
        services.AddScoped<HumanApprovalFilter>();
        services.AddScoped<CostTrackingFilter>();


        return services;
    }
}
