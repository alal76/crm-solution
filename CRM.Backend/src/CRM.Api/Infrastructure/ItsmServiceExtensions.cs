// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace CRM.Api.Infrastructure;

/// <summary>
/// AP-039: ITSM service registrations extracted from Program.cs.
/// Groups all IT Service Management service registrations (Incident, Problem, Change,
/// CMDB, Knowledge, SLA, Escalation, Notifications, Automation, CAB etc.) into a
/// single discoverable extension method.
/// </summary>
internal static class ItsmServiceExtensions
{
    /// <summary>
    /// Registers all ITSM services with the DI container.
    /// Extracted from Program.cs (AP-039) — behavior is functionally identical.
    /// </summary>
    internal static IServiceCollection AddItsmServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ITSM Services - IT Service Management (Incident, Problem, Change, CMDB, Knowledge, SLA)
        // PHASE 1: Core critical services re-enabled (Feb 16, 2026)
        services.AddScoped<CRM.Infrastructure.Services.ITSM.IBusinessHoursCalculator, CRM.Infrastructure.Services.ITSM.BusinessHoursCalculator>();
        services.AddScoped<CRM.Core.Interfaces.ITSM.IIncidentService, CRM.Infrastructure.Services.ITSM.IncidentService>();
        services.AddScoped<CRM.Core.Interfaces.ITSM.ISLAService, CRM.Infrastructure.Services.ITSM.SLAService>();
        Log.Information("ITSM Phase 1 Tier-1 Services registered: BusinessHoursCalculator, IncidentService, SLAService");

        // PHASE 2-4: Additional ITSM services
        services.AddScoped<CRM.Core.Interfaces.ITSM.IProblemManagementService, CRM.Infrastructure.Services.ITSM.ProblemManagementService>();
        services.AddScoped<CRM.Core.Interfaces.ITSM.IProblemService, CRM.Infrastructure.Services.ITSM.ProblemService>();
        services.AddScoped<CRM.Core.Interfaces.ITSM.ICMDBService, CRM.Infrastructure.Services.ITSM.CMDBService>();
        services.AddScoped<CRM.Core.Interfaces.ITSM.IChangeManagementService, CRM.Infrastructure.Services.ITSM.ChangeManagementService>();
        services.AddScoped<CRM.Core.Interfaces.ITSM.IChangeManagementServiceEx, CRM.Infrastructure.Services.ITSM.ChangeManagementServiceEx>();
        services.AddScoped<CRM.Core.Interfaces.IChangeService, CRM.Infrastructure.Services.ChangeService>();
        services.AddScoped<CRM.Core.Interfaces.ITSM.IKnowledgeManagementService, CRM.Infrastructure.Services.ITSM.KnowledgeManagementService>();

        // General Knowledge Base service (CRM.Core.Ports.Input.IKnowledgeBaseService)
        services.AddScoped<CRM.Core.Ports.Input.IKnowledgeBaseService, CRM.Infrastructure.Services.KnowledgeBaseService>();
        // KB-010/KB-011: Unified Knowledge Search facade (General KB + ITSM KB)
        services.AddScoped<CRM.Core.Ports.Input.IUnifiedKnowledgeSearchService, CRM.Infrastructure.Services.UnifiedKnowledgeSearchService>();

        // KB search index schema is configured by KnowledgeBaseSearchIndexService on startup
        services.AddScoped<CRM.Infrastructure.Services.Search.IKnowledgeBaseSearchIndexService, CRM.Infrastructure.Services.Search.KnowledgeBaseSearchIndexService>();
        services.AddScoped<CRM.Core.Interfaces.ITSM.IServiceCatalogService, CRM.Infrastructure.Services.ITSM.ServiceCatalogService>();

        // IEscalationRulePolicyService is the SLA-focused service (renamed from IEscalationRuleService)
        services.AddScoped<CRM.Core.Interfaces.ITSM.IEscalationRulePolicyService, CRM.Infrastructure.Services.ITSM.EscalationRuleService>(); // Renamed from IEscalationRuleService
        services.AddScoped<CRM.Core.Interfaces.ITSM.IEscalationPolicyService, CRM.Infrastructure.Services.ITSM.EscalationPolicyService>();

        // ITSM Escalation Analytics (TODO-SD005-011)
        services.AddScoped<CRM.Core.Interfaces.ITSM.IEscalationAnalyticsService, CRM.Infrastructure.Services.ITSM.EscalationAnalyticsService>();

        // SMS Notification Service (TODO-SD005-009) — use Twilio when config present, else built-in stub
        var twilioAccountSid = configuration["Providers:Notifications:Twilio:AccountSid"];
        if (!string.IsNullOrWhiteSpace(twilioAccountSid))
        {
            services.Configure<CRM.Infrastructure.Providers.Twilio.TwilioConfiguration>(
                configuration.GetSection(CRM.Infrastructure.Providers.Twilio.TwilioConfiguration.SectionName));
            services.AddScoped<CRM.Core.Interfaces.Notifications.ISmsNotificationService,
                CRM.Infrastructure.Providers.Twilio.TwilioSmsService>();
            Log.Information("SMS notification service: TwilioSmsService");
        }
        else
        {
            services.AddScoped<CRM.Core.Interfaces.Notifications.ISmsNotificationService,
                CRM.Infrastructure.Services.Notifications.SmsNotificationService>();
            Log.Information("SMS notification service: SmsNotificationService (stub)");
        }

        // ITSM Phase 4 - Advanced Automation & Integration Services
        services.AddScoped<CRM.Core.Interfaces.ITSM.IWebhookNotificationService, CRM.Infrastructure.Services.ITSM.WebhookNotificationService>();
        services.AddScoped<CRM.Core.Interfaces.ITSM.IEmailToTicketService, CRM.Infrastructure.Services.ITSM.EmailToTicketService>();
        services.AddScoped<CRM.Core.Interfaces.ITSM.IITSMDashboardService, CRM.Infrastructure.Services.ITSM.ITSMDashboardService>();
        services.AddScoped<CRM.Core.Interfaces.ITSM.IMonitoringIntegrationService, CRM.Infrastructure.Services.ITSM.MonitoringIntegrationService>();
        services.AddScoped<CRM.Core.Interfaces.ITSM.ICICDIntegrationService, CRM.Infrastructure.Services.ITSM.CICDIntegrationService>();
        services.AddScoped<CRM.Core.Interfaces.ITSM.ISelfServiceChatbotService, CRM.Infrastructure.Services.ITSM.SelfServiceChatbotService>();

        // ITSM Extended Services — CAB, Calendar, Impact, Article Recommendations
        services.AddScoped<CRM.Infrastructure.Services.ITSM.ICABWorkflowService, CRM.Infrastructure.Services.ITSM.CABWorkflowService>();
        services.AddScoped<CRM.Infrastructure.Services.ITSM.IChangeCalendarService, CRM.Infrastructure.Services.ITSM.ChangeCalendarService>();
        services.AddScoped<CRM.Infrastructure.Services.ITSM.IChangeImpactService, CRM.Infrastructure.Services.ITSM.ChangeImpactService>();
        services.AddScoped<CRM.Core.Interfaces.ITSM.IArticleRecommendationService, CRM.Infrastructure.Services.ITSM.ArticleRecommendationService>();

        // ITSM Advanced Services — Assignment, Catalog, Discovery, Impact Analysis, KCS, Asset Lifecycle
        services.AddScoped<CRM.Infrastructure.Services.ITSM.IAssignmentRulesEngine, CRM.Infrastructure.Services.ITSM.AssignmentRulesEngine>();
        services.AddScoped<CRM.Infrastructure.Services.ITSM.ICatalogApprovalService, CRM.Infrastructure.Services.ITSM.CatalogApprovalService>();
        services.AddScoped<CRM.Infrastructure.Services.ITSM.ICatalogFulfillmentService, CRM.Infrastructure.Services.ITSM.CatalogFulfillmentService>();
        services.AddScoped<CRM.Infrastructure.Services.ITSM.IDiscoveryService, CRM.Infrastructure.Services.ITSM.DiscoveryService>();
        services.AddScoped<CRM.Infrastructure.Services.ITSM.IImpactAnalysisService, CRM.Infrastructure.Services.ITSM.ImpactAnalysisService>();
        services.AddScoped<CRM.Infrastructure.Services.ITSM.IKCSWorkflowService, CRM.Infrastructure.Services.ITSM.KCSWorkflowService>();
        services.AddScoped<CRM.Infrastructure.Services.ITSM.IAssetLifecycleService, CRM.Infrastructure.Services.ITSM.AssetLifecycleService>();

        // Slack/Teams notification channels for ITSM — registered when external notification provider is configured
        services.AddHttpClient<CRM.Infrastructure.Services.ITSM.SlackItsmNotificationService>();
        services.AddHttpClient<CRM.Infrastructure.Services.ITSM.TeamsItsmNotificationService>();
        services.AddScoped<CRM.Core.Interfaces.ITSM.IItsmNotificationChannel>(sp =>
            sp.GetRequiredService<CRM.Infrastructure.Services.ITSM.SlackItsmNotificationService>());
        services.AddScoped<CRM.Core.Interfaces.ITSM.IItsmNotificationChannel>(sp =>
            sp.GetRequiredService<CRM.Infrastructure.Services.ITSM.TeamsItsmNotificationService>());
        services.AddScoped<CRM.Core.Interfaces.ITSM.IItsmNotificationDispatcher,
            CRM.Infrastructure.Services.ITSM.ItsmNotificationDispatcher>();
        Log.Information("ITSM notification channels registered: Slack, Teams (TODO-SD005-010)");

        // SLA Enforcement Background Service - runs continuously to monitor and enforce SLAs
        services.AddHostedService<CRM.Infrastructure.Services.ITSM.SLAEnforcementHostedService>();

        // Auto-close resolved items background service (auto-closes incidents, service requests, changes, problems)
        services.AddHostedService<CRM.Infrastructure.Services.ITSM.AutoCloseHostedService>();

        // Escalation background service (auto-escalates incidents/service requests based on SLA thresholds)
        services.AddHostedService<CRM.Infrastructure.Services.ITSM.EscalationHostedService>();

        return services;
    }
}
