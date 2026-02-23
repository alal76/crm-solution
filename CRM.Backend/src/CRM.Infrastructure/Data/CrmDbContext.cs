// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Linq.Expressions;
using CRM.Core.Entities;
using CRM.Core.Entities.AI;
using CRM.Core.Entities.Integration;
using CRM.Core.Entities.KnowledgeBase;
using CRM.Core.Entities.Reports;
using CRM.Core.Entities.Workers;
using CRM.Core.Entities.Workflow;
using CRM.Core.Interfaces;
using CRM.Core.Models;
using CRM.Infrastructure.Data.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ITSM = CRM.Core.Entities.ITSM; // Alias for ITSM entities to avoid conflicts

namespace CRM.Infrastructure.Data;

/// <summary>
/// CRM Database Context - Supports multiple databases (SQL Server, PostgreSQL, Oracle, MariaDB)
/// </summary>
public class CrmDbContext : DbContext, ICrmDbContext
{
    private readonly IConfiguration _configuration;

    public CrmDbContext(DbContextOptions<CrmDbContext> options, IConfiguration configuration)
        : base(options)
    {
        _configuration = configuration;
    }

    public DbSet<Account> Accounts { get; set; }

    // /// <summary>
    // /// Customers alias for Accounts (for backward compatibility)
    // /// </summary>
    // public IQueryable<Account> Customers => Accounts;

    public DbSet<Preferences> Preferences { get; set; }
    public DbSet<AccountContact> AccountContacts { get; set; }
    public DbSet<Opportunity> Opportunities { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Interaction> Interactions { get; set; }
    public DbSet<MarketingCampaign> MarketingCampaigns { get; set; }
    public DbSet<CampaignMetric> CampaignMetrics { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<OAuthToken> OAuthTokens { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<WebAuthnCredential> WebAuthnCredentials { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<UserGroup> UserGroups { get; set; }
    public DbSet<UserGroupMember> UserGroupMembers { get; set; }
    public DbSet<UserApprovalRequest> UserApprovalRequests { get; set; }

    // RBAC - Role-Based Access Control (SYS-012)
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<UserRoleAssignment> UserRoleAssignments { get; set; }

    /// <summary>Alias for UserRoleAssignments for backward compatibility</summary>
    public DbSet<UserRoleAssignment> UserRoles => UserRoleAssignments;

    public DbSet<DatabaseBackup> DatabaseBackups { get; set; }
    public DbSet<BackupSchedule> BackupSchedules { get; set; }

    // Contact entities (using Models.Contact - legacy)
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<SocialMediaLink> SocialMediaLinks { get; set; }

    // Lead entity
    public DbSet<Lead> Leads { get; set; }
    public DbSet<LeadProductInterest> LeadProductInterests { get; set; }

    // Opportunity junction table
    public DbSet<OpportunityProduct> OpportunityProducts { get; set; }

    // New comprehensive entities
    public DbSet<CrmTask> CrmTasks { get; set; }
    public DbSet<Note> Notes { get; set; }
    public DbSet<Quote> Quotes { get; set; }
    public DbSet<QuoteLineItem> QuoteLineItems { get; set; }
    public DbSet<Activity> Activities { get; set; }
    public DbSet<EventAttendee> EventAttendees { get; set; }

    // Calendar Integration entities (G4)
    public DbSet<CalendarIntegration> CalendarIntegrations { get; set; }
    public DbSet<CalendarSyncLog> CalendarSyncLogs { get; set; }
    public DbSet<CalendarEventMapping> CalendarEventMappings { get; set; }

    // Email Integration entities (G5)
    public DbSet<EmailIntegration> EmailIntegrations { get; set; }
    public DbSet<EmailSyncLog> EmailSyncLogs { get; set; }
    public DbSet<EmailMessageMapping> EmailMessageMappings { get; set; }

    // Contact info entities
    public DbSet<Address> Addresses { get; set; }
    public DbSet<ContactDetail> ContactDetails { get; set; }
    public DbSet<SocialAccount> SocialAccounts { get; set; }

    // Consolidated contact info entities (new)
    public DbSet<PhoneNumber> PhoneNumbers { get; set; }
    public DbSet<EmailAddress> EmailAddresses { get; set; }
    public DbSet<SocialMediaAccount> SocialMediaAccounts { get; set; }

    // Contact info junction tables
    public DbSet<EntityAddressLink> EntityAddressLinks { get; set; }
    public DbSet<EntityPhoneLink> EntityPhoneLinks { get; set; }
    public DbSet<EntityEmailLink> EntityEmailLinks { get; set; }
    public DbSet<EntitySocialMediaLink> EntitySocialMediaLinks { get; set; }
    public DbSet<ContactInfoLink> ContactInfoLinks { get; set; }
    public DbSet<LookupCategory> LookupCategories { get; set; }
    public DbSet<LookupItem> LookupItems { get; set; }

    // Normalization helper tables
    public DbSet<CRM.Core.Entities.Tag> Tags { get; set; }
    public DbSet<CRM.Core.Entities.EntityTag> EntityTags { get; set; }
    public DbSet<CRM.Core.Entities.CustomField> CustomFields { get; set; }

    // Service Request entities
    public DbSet<ServiceRequest> ServiceRequests { get; set; }
    public DbSet<ServiceRequestCategory> ServiceRequestCategories { get; set; }
    public DbSet<ServiceRequestSubcategory> ServiceRequestSubcategories { get; set; }
    public DbSet<ServiceRequestType> ServiceRequestTypes { get; set; }
    public DbSet<ServiceRequestCustomFieldDefinition> ServiceRequestCustomFieldDefinitions { get; set; }
    public DbSet<ServiceRequestCustomFieldValue> ServiceRequestCustomFieldValues { get; set; }

    // System settings
    public DbSet<SystemSettings> SystemSettings { get; set; }
    public DbSet<BrandingConfig> BrandingConfigs { get; set; }

    // Color palettes
    public DbSet<ColorPalette> ColorPalettes { get; set; }

    // LLM Provider Settings
    public DbSet<LLMProviderSetting> LLMProviderSettings { get; set; }

    // Provider Configuration Management (System & CRM Config)
    public DbSet<ProviderConfiguration> ProviderConfigurations { get; set; }
    public DbSet<ConfigurationChangeLog> ConfigurationChangeLogs { get; set; }

    // Module field configurations
    public DbSet<ModuleFieldConfiguration> ModuleFieldConfigurations { get; set; }
    public DbSet<ModuleUIConfig> ModuleUIConfigs { get; set; }
    public DbSet<FieldMasterDataLink> FieldMasterDataLinks { get; set; }

    // UI Preferences and Customizations
    public DbSet<UIPreference> UIPreferences { get; set; }
    public DbSet<UICustomization> UICustomizations { get; set; }
    public DbSet<DashboardCustomization> DashboardCustomizations { get; set; }

    // Feature Flags (SYS-004)
    public DbSet<FeatureFlag> FeatureFlags { get; set; }
    public DbSet<FeatureFlagVariant> FeatureFlagVariants { get; set; }

    // Feature Flag Audit Trail
    public DbSet<FeatureFlagAuditLog> FeatureFlagAuditLogs { get; set; }

    // Performance Metrics
    public DbSet<PerformanceMetric> PerformanceMetrics { get; set; }

    // Communication entities
    public DbSet<CommunicationChannel> CommunicationChannels { get; set; }
    public DbSet<CommunicationMessage> CommunicationMessages { get; set; }
    public DbSet<EmailTemplate> EmailTemplates { get; set; }
    public DbSet<EmailTemplateHistoryEntry> EmailTemplateHistoryEntries { get; set; }
    public DbSet<EmailTemplateUsage> EmailTemplateUsages { get; set; }
    public DbSet<EmailTemplateVersion> EmailTemplateVersions { get; set; }
    public DbSet<Conversation> Conversations { get; set; }

    // Master data entities
    public DbSet<ZipCode> ZipCodes { get; set; }
    public DbSet<Locality> Localities { get; set; }

    // Social media follow tracking
    public DbSet<SocialMediaFollow> SocialMediaFollows { get; set; }

    // Cloud Deployment entities
    public DbSet<CloudProvider> CloudProviders { get; set; }
    public DbSet<CloudDeployment> CloudDeployments { get; set; }
    public DbSet<DeploymentAttempt> DeploymentAttempts { get; set; }
    public DbSet<HealthCheckLog> HealthCheckLogs { get; set; }

    // Dashboard and Analytics entities
    public DbSet<Dashboard> Dashboards { get; set; }
    public DbSet<DashboardWidget> DashboardWidgets { get; set; }

    // Workflow entities
    public DbSet<WorkflowDefinition> WorkflowDefinitions { get; set; }
    public DbSet<WorkflowVersion> WorkflowVersions { get; set; }
    public DbSet<WorkflowNode> WorkflowNodes { get; set; }
    public DbSet<WorkflowTransition> WorkflowTransitions { get; set; }
    public DbSet<WorkflowInstance> WorkflowInstances { get; set; }
    public DbSet<WorkflowNodeInstance> WorkflowNodeInstances { get; set; }
    public DbSet<WorkflowTask> WorkflowTasks { get; set; }
    public DbSet<WorkflowLog> WorkflowLogs { get; set; }
    public DbSet<WorkflowTrigger> WorkflowTriggers { get; set; }
    public DbSet<WorkflowSchedule> WorkflowSchedules { get; set; }
    public DbSet<WorkflowContextVariable> WorkflowContextVariables { get; set; }
    public DbSet<WorkflowJob> WorkflowJobs { get; set; }
    public DbSet<WorkflowAuditLog> WorkflowAuditLogs { get; set; }
    public DbSet<WorkflowMetric> WorkflowMetrics { get; set; }
    public DbSet<WorkflowLlmUsage> WorkflowLlmUsages { get; set; }
    public DbSet<WorkflowCircuitBreakerState> WorkflowCircuitBreakerStates { get; set; }

    // Relationship Management entities
    public DbSet<CRM.Core.Entities.RelationshipType> RelationshipTypes { get; set; }
    public DbSet<AccountRelationship> AccountRelationships { get; set; }
    public DbSet<RelationshipInteraction> RelationshipInteractions { get; set; }
    public DbSet<AccountHealthSnapshot> AccountHealthSnapshots { get; set; }
    public DbSet<RelationshipMap> RelationshipMaps { get; set; }
    public DbSet<AccountTerritory> AccountTerritories { get; set; }
    public DbSet<AccountTerritoryAssignment> AccountTerritoryAssignments { get; set; }


    // Campaign execution entities
    public DbSet<CampaignRecipient> CampaignRecipients { get; set; }
    public DbSet<CampaignLinkClick> CampaignLinkClicks { get; set; }
    public DbSet<CampaignABTest> CampaignABTests { get; set; }
    public DbSet<CampaignConversion> CampaignConversions { get; set; }
    public DbSet<CampaignWorkflow> CampaignWorkflows { get; set; }

    // =============================================================================
    // Quote-to-Cash Entities (Order, Invoice, Payment, Subscription, Credit)
    // =============================================================================
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderLineItem> OrderLineItems { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<InvoiceLineItem> InvoiceLineItems { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<SubscriptionItem> SubscriptionItems { get; set; }
    public DbSet<SubscriptionUsage> SubscriptionUsages { get; set; }
    public DbSet<SubscriptionUsageLimit> SubscriptionUsageLimits { get; set; }
    public DbSet<SubscriptionRenewal> SubscriptionRenewals { get; set; }
    public DbSet<BillingHistory> BillingHistories { get; set; }
    public DbSet<DunningRecord> DunningRecords { get; set; }
    public DbSet<Contract> Contracts { get; set; }
    public DbSet<CreditMemo> CreditMemos { get; set; }
    public DbSet<CreditMemoLineItem> CreditMemoLineItems { get; set; }
    public DbSet<CreditApplication> CreditApplications { get; set; }

    // =============================================================================
    // Lead Management Entities (Routing, Scoring, Duplicate Detection)
    // =============================================================================
    public DbSet<LeadRoutingRule> LeadRoutingRules { get; set; }
    public DbSet<LeadRoutingCriteria> LeadRoutingCriteria { get; set; }
    public DbSet<LeadRoutingTarget> LeadRoutingTargets { get; set; }
    public DbSet<LeadRoutingLog> LeadRoutingLogs { get; set; }
    public DbSet<LeadScoreRule> LeadScoreRules { get; set; }
    public DbSet<DuplicateRule> DuplicateRules { get; set; }
    public DbSet<DuplicateMatchField> DuplicateMatchFields { get; set; }
    public DbSet<DuplicateCandidate> DuplicateCandidates { get; set; }
    public DbSet<DuplicateMergeHistory> DuplicateMergeHistories { get; set; }
    public DbSet<DuplicateMergeGroup> DuplicateMergeGroups { get; set; }
    public DbSet<DuplicateMergeGroupMember> DuplicateMergeGroupMembers { get; set; }

    // =============================================================================
    // Marketing Automation Entities (Email Sequences, Web Tracking, Forms)
    // =============================================================================
    public DbSet<EmailSequence> EmailSequences { get; set; }
    public DbSet<EmailSequenceStep> EmailSequenceSteps { get; set; }
    public DbSet<EmailSequenceEnrollment> EmailSequenceEnrollments { get; set; }
    public DbSet<EmailSequenceStepExecution> EmailSequenceStepExecutions { get; set; }
    public DbSet<WebVisitor> WebVisitors { get; set; }
    public DbSet<WebSession> WebSessions { get; set; }
    public DbSet<WebPageView> WebPageViews { get; set; }
    public DbSet<FormDefinition> FormDefinitions { get; set; }
    public DbSet<FormField> FormFields { get; set; }
    public DbSet<FormSubmission> FormSubmissions { get; set; }
    public DbSet<LandingPage> LandingPages { get; set; }
    public DbSet<LandingPageBlock> LandingPageBlocks { get; set; }
    public DbSet<LandingPageVisit> LandingPageVisits { get; set; }
    public DbSet<AttributionSetting> AttributionSettings { get; set; }
    public DbSet<CampaignTouchpoint> CampaignTouchpoints { get; set; }
    public DbSet<CampaignAttributionSummary> CampaignAttributionSummaries { get; set; }

    // =============================================================================
    // CPQ Entities (Product Bundles, Pricing Rules, Discounts)
    // =============================================================================
    public DbSet<ProductBundle> ProductBundles { get; set; }
    public DbSet<ProductBundleItem> ProductBundleItems { get; set; }
    public DbSet<ProductBundleRule> ProductBundleRules { get; set; }
    public DbSet<PriceBook> PriceBooks { get; set; }
    public DbSet<PriceBookEntry> PriceBookEntries { get; set; }
    public DbSet<PricingRule> PricingRules { get; set; }
    public DbSet<PricingRuleUsage> PricingRuleUsages { get; set; }
    public DbSet<DiscountApprovalMatrix> DiscountApprovalMatrices { get; set; }
    public DbSet<ApprovalLevel> ApprovalLevels { get; set; }
    public DbSet<ApprovalGroup> ApprovalGroups { get; set; }
    public DbSet<ApprovalGroupMember> ApprovalGroupMembers { get; set; }
    public DbSet<ApprovalRequest> ApprovalRequests { get; set; }
    public DbSet<ApprovalStep> ApprovalSteps { get; set; }

    // =============================================================================
    // E-Signature Entities
    // =============================================================================
    public DbSet<ESignatureRequest> ESignatureRequests { get; set; }
    public DbSet<ESignatureSigner> ESignatureSigners { get; set; }
    public DbSet<ESignatureDocument> ESignatureDocuments { get; set; }
    public DbSet<ESignatureAuditEvent> ESignatureAuditEvents { get; set; }

    // =============================================================================
    // Sales Performance Entities (Commission, Quota, Forecast)
    // =============================================================================
    public DbSet<CommissionPlan> CommissionPlans { get; set; }
    public DbSet<CommissionTier> CommissionTiers { get; set; }
    public DbSet<CommissionPlanAssignment> CommissionPlanAssignments { get; set; }
    public DbSet<Commission> Commissions { get; set; }
    public DbSet<CommissionStatement> CommissionStatements { get; set; }
    public DbSet<SalesQuota> SalesQuotas { get; set; }
    public DbSet<SalesForecast> SalesForecasts { get; set; }
    public DbSet<ForecastLineItem> ForecastLineItems { get; set; }
    public DbSet<ForecastHistory> ForecastHistories { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<TeamMember> TeamMembers { get; set; }

    // =============================================================================
    // Admin Configuration Entities (Commission Rules, Discount Rules, SLA, Escalation, Queues, Sales Config)
    // =============================================================================
    public DbSet<CommissionRule> CommissionRules { get; set; }
    public DbSet<CommissionHistory> CommissionHistories { get; set; }
    public DbSet<CommissionApprovalAudit> CommissionApprovalAudits { get; set; }
    public DbSet<DiscountRule> DiscountRules { get; set; }
    public DbSet<DiscountHistory> DiscountHistories { get; set; }
    public DbSet<SalesConfiguration> SalesConfigurations { get; set; }

    // =============================================================================
    // AI Entities (Lead Scoring, Predictions, Insights)
    // =============================================================================
    public DbSet<AIModel> AIModels { get; set; }
    public DbSet<Prediction> Predictions { get; set; }
    public DbSet<LeadScore> LeadScores { get; set; }
    public DbSet<OpportunityInsight> OpportunityInsights { get; set; }
    public DbSet<ChurnRisk> ChurnRisks { get; set; }
    public DbSet<ActionRecommendation> ActionRecommendations { get; set; }
    public DbSet<EmailIntelligence> EmailIntelligences { get; set; }

    // AI Agent Entities (ADR-004 — Semantic Kernel Integration)
    public DbSet<AIAgent> AIAgents { get; set; }
    public DbSet<AgentConversation> AgentConversations { get; set; }
    public DbSet<AgentAction> AgentActions { get; set; }
    public DbSet<AgentMemory> AgentMemories { get; set; }
    public DbSet<AgentApprovalRequest> AgentApprovalRequests { get; set; }

    // =============================================================================
    // Report Entities (Report Builder, Schedules)
    // =============================================================================
    public DbSet<ReportDefinition> ReportDefinitions { get; set; }
    public DbSet<ReportFolder> ReportFolders { get; set; }
    public DbSet<ReportSchedule> ReportSchedules { get; set; }
    public DbSet<ReportExecution> ReportExecutions { get; set; }
    public DbSet<ReportWidgetConfig> ReportWidgetConfigs { get; set; }

    // =============================================================================
    // Knowledge Base Entities (Articles, SLA)
    // =============================================================================
    public DbSet<KnowledgeArticle> KnowledgeArticles { get; set; }
    public DbSet<KnowledgeCategory> KnowledgeCategories { get; set; }
    public DbSet<ServiceRequestArticle> ServiceRequestArticles { get; set; }
    public DbSet<ArticleFeedback> ArticleFeedbacks { get; set; }
    public DbSet<CRM.Core.Entities.SLAPolicy> SLAPolicies { get; set; }
    public DbSet<SLATarget> SLATargets { get; set; }
    public DbSet<SLAInstance> SLAInstances { get; set; }
    public DbSet<BusinessHours> BusinessHoursConfigs { get; set; }
    // DISABLED: Conflicts with ITSM.EscalationRule - both trying to use "EscalationRule" table
    // public DbSet<CRM.Core.Entities.EscalationRule> EscalationRules { get; set; }
    public DbSet<ITSM.ServiceQueue> ServiceQueues { get; set; }
    public DbSet<ITSM.EscalationRule> ITSMEscalationRules { get; set; }

    // NOTE: EscalationPolicy, EscalationLevel, EscalationHistory services are disabled pending proper entity implementation

    // =============================================================================
    // ITSM Module Entities (Incident, Problem, Change, CMDB, Knowledge, Catalog)
    // =============================================================================

    // Incident Management
    public DbSet<ITSM.Incident> Incidents { get; set; }
    public DbSet<ITSM.IncidentComment> IncidentComments { get; set; }
    public DbSet<ITSM.IncidentAttachment> IncidentAttachments { get; set; }
    public DbSet<ITSM.IncidentHistory> IncidentHistory { get; set; }

    // Problem Management
    public DbSet<ITSM.Problem> Problems { get; set; }
    public DbSet<ITSM.ProblemIncident> ProblemIncidents { get; set; }
    public DbSet<ITSM.ProblemTask> ProblemTasks { get; set; }
    public DbSet<ITSM.ProblemComment> ProblemComments { get; set; }
    public DbSet<ITSM.ProblemAttachment> ProblemAttachments { get; set; }

    // SLA Management (Enhanced)
    public DbSet<ITSM.SLAPolicy> ITSMSLAPolicies { get; set; }
    public DbSet<ITSM.SLAInstance> ITSMSLAInstances { get; set; }
    public DbSet<ITSM.BusinessHoursSchedule> BusinessHoursSchedules { get; set; }

    // CMDB
    public DbSet<ITSM.ConfigurationItem> ConfigurationItems { get; set; }
    public DbSet<ITSM.CIRelationship> CIRelationships { get; set; }
    public DbSet<ITSM.Service> Services { get; set; }
    public DbSet<ITSM.ServiceCI> ServiceCIs { get; set; }

    // Change Management
    public DbSet<ITSM.Change> Changes { get; set; }
    public DbSet<ITSM.ChangeApproval> ChangeApprovals { get; set; }
    public DbSet<ITSM.ChangeBlackout> ChangeBlackouts { get; set; }
    public DbSet<ITSM.ChangeImpactedCI> ChangeImpactedCIs { get; set; }
    public DbSet<ITSM.ChangeTask> ChangeTasks { get; set; }
    public DbSet<ITSM.ChangeComment> ChangeComments { get; set; }
    public DbSet<ITSM.ChangeAttachment> ChangeAttachments { get; set; }

    // Knowledge Management (Enhanced)
    public DbSet<ITSM.KnowledgeArticle> ITSMKnowledgeArticles { get; set; }
    public DbSet<ITSM.ArticleRelationship> ArticleRelationships { get; set; }
    public DbSet<ITSM.ArticleIncident> ArticleIncidents { get; set; }
    public DbSet<ITSM.ArticleFeedback> ITSMArticleFeedback { get; set; }
    public DbSet<ITSM.ArticleAttachment> ArticleAttachments { get; set; }

    // Service Catalog
    public DbSet<ITSM.CatalogCategory> CatalogCategories { get; set; }
    public DbSet<ITSM.CatalogItem> CatalogItems { get; set; }
    public DbSet<ITSM.CatalogVariable> CatalogVariables { get; set; }
    public DbSet<ITSM.CatalogRequest> CatalogRequests { get; set; }
    public DbSet<ITSM.CatalogRequestApproval> CatalogRequestApprovals { get; set; }
    public DbSet<ITSM.CatalogRequestComment> CatalogRequestComments { get; set; }

    // Escalation Policies
    public DbSet<ITSM.EscalationPolicy> EscalationPolicies { get; set; }
    public DbSet<ITSM.EscalationLevel> EscalationLevels { get; set; }
    public DbSet<ITSM.EscalationHistory> EscalationHistories { get; set; }

    // =============================================================================
    // Integration & Webhook Entities
    // =============================================================================
    // Worker architecture
    public DbSet<WorkerJob> WorkerJobs { get; set; }
    public DbSet<WorkerExecution> WorkerExecutions { get; set; }
    public DbSet<OutboxEvent> OutboxEvents { get; set; }

    public DbSet<ITSM.WebhookSubscription> WebhookSubscriptions { get; set; }
    public DbSet<ITSM.WebhookDelivery> WebhookDeliveries { get; set; }

    // Previously missing DbSets (used by services via _dbContext.PropertyName)
    public DbSet<AnalyticsEvent> AnalyticsEvents { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<ITSM.CITypeDefinition> CITypes { get; set; }
    public DbSet<ITSM.ChangeTypeEntity> ChangeTypes { get; set; }

    // New entity DbSets
    public DbSet<AIAgentUsage> AIAgentUsages { get; set; }
    public DbSet<ExportJob> ExportJobs { get; set; }
    public DbSet<ImportJob> ImportJobs { get; set; }
    public DbSet<ITSM.IncidentCategory> IncidentCategories { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured && _configuration != null)
        {
            var databaseProvider = _configuration["DatabaseProvider"] ?? "mariadb";
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString) && (databaseProvider.ToLower() == "mysql" || databaseProvider.ToLower() == "mariadb"))
            {
                var dbHost = _configuration["DB_HOST"] ?? _configuration["DbHost"] ?? "mariadb";
                var dbPort = _configuration["DB_PORT"] ?? "3306";
                var dbName = _configuration["DB_NAME"] ?? "crm_db";
                var dbUser = _configuration["DB_USER"] ?? "crm_user";
                var dbPass = _configuration["DB_PASSWORD"] ?? _configuration["DB_PASS"] ?? "crm_pass";
                connectionString = $"Server={dbHost};Port={dbPort};Database={dbName};Uid={dbUser};Pwd={dbPass};";
            }

            switch (databaseProvider.ToLower())
            {
                case "postgresql":
                    optionsBuilder.UseNpgsql(connectionString);
                    break;
                case "oracle":
                    optionsBuilder.UseOracle(connectionString);
                    break;
                case "mysql":
                case "mariadb":
                    optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
                    break;
                case "inmemory":
                    optionsBuilder.UseInMemoryDatabase("crm_test");
                    break;
                case "sqlite":
                    optionsBuilder.UseSqlite(connectionString ?? "Data Source=crm.db");
                    break;
                case "sqlserver":
                default:
                    optionsBuilder.UseSqlite(connectionString ?? "Data Source=crm.db");
                    break;
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // CRITICAL: Ignore CRM.Core.Entities.EscalationRule to prevent conflict with ITSM.EscalationRule
        // Both entity types were trying to map to "EscalationRule" table. ITSM version uses "ITSMEscalationRules" instead.
        modelBuilder.Ignore<CRM.Core.Entities.EscalationRule>();


        // Use Strategy Pattern for database provider-specific configurations
        var factory = new DatabaseProviderStrategyFactory(_configuration);
        var databaseProvider = _configuration?["DatabaseProvider"]?.ToLower() ?? "mariadb";
        var providerStrategy = factory.CreateStrategy(databaseProvider, Database.ProviderName);

        // Get provider-specific column types for use in configurations
        var longTextType = providerStrategy.LongTextColumnType;
        var textType = providerStrategy.TextColumnType;

        // Worker architecture entities
        modelBuilder.Entity<WorkerJob>(entity =>
        {
            entity.Property(e => e.JobType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Payload).HasColumnType(longTextType);
            entity.Property(e => e.CorrelationId).HasMaxLength(100);
            entity.Property(e => e.LastError).HasColumnType(textType);
            entity.HasIndex(e => new { e.Status, e.NextAttemptAt })
                .HasDatabaseName("IX_WorkerJobs_Status_NextAttemptAt");
            entity.HasIndex(e => e.JobType)
                .HasDatabaseName("IX_WorkerJobs_JobType");
        });

        modelBuilder.Entity<WorkerExecution>(entity =>
        {
            entity.Property(e => e.ErrorMessage).HasColumnType(textType);
            entity.Property(e => e.NodeId).HasMaxLength(100);
            entity.HasIndex(e => e.WorkerJobId)
                .HasDatabaseName("IX_WorkerExecutions_WorkerJobId");
        });

        modelBuilder.Entity<OutboxEvent>(entity =>
        {
            entity.Property(e => e.EventType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Payload).HasColumnType(longTextType);
            entity.Property(e => e.CorrelationId).HasMaxLength(100);
            entity.Property(e => e.IdempotencyKey).HasMaxLength(100);
            entity.Property(e => e.LastError).HasColumnType(textType);
            entity.HasIndex(e => e.Status)
                .HasDatabaseName("IX_OutboxEvents_Status");
            entity.HasIndex(e => e.OccurredAt)
                .HasDatabaseName("IX_OutboxEvents_OccurredAt");
        });

        // Configure RowVersion for all entities that inherit from BaseEntity
        // This enables optimistic concurrency control using the provider strategy
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                providerStrategy.ConfigureRowVersion(modelBuilder, entityType);
            }
        }

        // Apply soft-delete query filter to all BaseEntity-derived entities
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var isDeletedProperty = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
            var isNotDeleted = Expression.Equal(isDeletedProperty, Expression.Constant(false));
            var filter = Expression.Lambda(isNotDeleted, parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
        }

        // Ignore the deprecated Customer class to prevent TPH discrimination
        // This is intentional - we need EF Core to ignore the alias class
#pragma warning disable CS0618 // Type or member is obsolete
        modelBuilder.Ignore<Customer>();
#pragma warning restore CS0618

        // Configure Account (formerly Customer)
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Company).HasMaxLength(255);
            entity.Property(e => e.LegalName).HasMaxLength(500);
            entity.Property(e => e.DbaName).HasMaxLength(255);
            entity.Property(e => e.TaxId).HasMaxLength(50);
            entity.Property(e => e.RegistrationNumber).HasMaxLength(100);
            entity.Property(e => e.Salutation).HasMaxLength(20);
            entity.Property(e => e.Suffix).HasMaxLength(20);
            entity.Property(e => e.Gender).HasMaxLength(20);
            entity.Property(e => e.Industry).HasMaxLength(100); // [FIELD GAP REMEDIATED]

            // Enum conversions for AccountType, Category, Priority, LifecycleStage
            entity.Property(e => e.AccountType).HasConversion<int>();
            entity.Property(e => e.Category).HasConversion<int>();
            entity.Property(e => e.Priority).HasConversion<int>();
            entity.Property(e => e.LifecycleStage).HasConversion<int>();

            // Custom fields JSON serialization
            entity.Property(e => e.CustomFields).HasColumnType("TEXT");

            // Map renamed properties to original database columns for backward compatibility
            entity.Property(e => e.AccountHealthScore).HasColumnName("CustomerHealthScore");
            // AccountType, ReferredByAccountId, ParentAccountId columns exist in database with these names
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.AssignedToUserId);
            entity.HasIndex(e => e.AccountManagerId);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.Company);

            // Self-referencing relationships
            entity.HasOne(e => e.ReferredByAccount)
                .WithMany()
                .HasForeignKey(e => e.ReferredByAccountId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.ParentAccount)
                .WithMany()
                .HasForeignKey(e => e.ParentAccountId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Preferences)
                .WithMany(p => p.Accounts)
                .HasForeignKey(e => e.PreferencesId)
                .OnDelete(DeleteBehavior.SetNull);

            // User relationships
            entity.HasOne(e => e.AssignedToUser)
                .WithMany()
                .HasForeignKey(e => e.AssignedToUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.AccountManager)
                .WithMany()
                .HasForeignKey(e => e.AccountManagerId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.VerifiedByUser)
                .WithMany()
                .HasForeignKey(e => e.VerifiedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Partnership relationships (self-referencing)
            entity.HasOne(e => e.ParentReseller)
                .WithMany(e => e.ResellerChildren)
                .HasForeignKey(e => e.ParentResellerAccountId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.CompetitorAccount)
                .WithMany()
                .HasForeignKey(e => e.CompetitorAccountId)
                .OnDelete(DeleteBehavior.SetNull);

            // Lead conversion relationships
            entity.HasOne(e => e.ConvertedFromLead)
                .WithMany()
                .HasForeignKey(e => e.ConvertedFromLeadId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.SourceCampaign)
                .WithMany()
                .HasForeignKey(e => e.SourceCampaignId)
                .OnDelete(DeleteBehavior.SetNull);

            // Ignore polymorphic navigation properties that use OwnerType/OwnerId or
            // EntityType/EntityId patterns — these don't have real FK columns on the
            // target tables, so EF would otherwise create shadow FK columns (AccountId)
            // which don't exist in the database.
            entity.Ignore(e => e.ContactInfoLinks);
            entity.Ignore(e => e.Addresses);
            entity.Ignore(e => e.EntityAddressLinks);
        });

        modelBuilder.Entity<Preferences>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PreferredContactMethod).HasMaxLength(50);
            entity.Property(e => e.PreferredLanguage).HasMaxLength(50);
            entity.Property(e => e.Timezone).HasMaxLength(100);

            entity.HasIndex(e => new
            {
                e.OptInEmail,
                e.OptInSms,
                e.OptInPhone,
                e.OptInPostal,
                e.PreferredContactMethod,
                e.PreferredLanguage,
                e.Timezone
            })
                .IsUnique();
        });

        // Configure CustomerContact (junction table for organization contacts)
        modelBuilder.Entity<AccountContact>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.AccountId, e.ContactId }).IsUnique();
            entity.Property(e => e.PositionAtAccount).HasMaxLength(100);
            entity.Property(e => e.DepartmentAtAccount).HasMaxLength(100);

            entity.HasOne(e => e.Account)
                .WithMany(c => c.AccountContacts)
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Contact)
                .WithMany(c => c.AccountContacts)
                .HasForeignKey(e => e.ContactId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Opportunity (3NF structure)
        modelBuilder.Entity<Opportunity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Amount).HasPrecision(18, 2);

            // Link Opportunity -> Customer (required)
            entity.HasOne(e => e.Account)
                .WithMany(c => c.Opportunities)
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            // Link Opportunity -> Lead (optional, source lead)
            entity.HasOne(e => e.Lead)
                .WithMany(l => l.Opportunities)
                .HasForeignKey(e => e.LeadId)
                .OnDelete(DeleteBehavior.SetNull);

            // Link Opportunity -> User (sales owner)
            entity.HasOne(e => e.SalesOwner)
                .WithMany()
                .HasForeignKey(e => e.SalesOwnerId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure OpportunityProduct junction table
        modelBuilder.Entity<OpportunityProduct>(entity =>
        {
            entity.HasKey(op => new { op.OpportunityId, op.ProductId });
            entity.HasOne(op => op.Opportunity)
                .WithMany(o => o.Products)
                .HasForeignKey(op => op.OpportunityId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(op => op.Product)
                .WithMany()
                .HasForeignKey(op => op.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.DiscountPercent).HasPrecision(5, 2);
            entity.Property(e => e.LineTotal).HasPrecision(18, 2);
        });

        // Configure LeadProductInterest junction table
        modelBuilder.Entity<LeadProductInterest>(entity =>
        {
            entity.HasKey(lpi => new { lpi.LeadId, lpi.ProductId });
            entity.HasOne(lpi => lpi.Lead)
                .WithMany(l => l.ProductInterests)
                .HasForeignKey(lpi => lpi.LeadId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(lpi => lpi.Product)
                .WithMany()
                .HasForeignKey(lpi => lpi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Product
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.SKU).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.Cost).HasPrecision(18, 2);
            entity.HasIndex(e => e.SKU).IsUnique();

            // Ignore navigation properties that have no direct FK on the target entity.
            // Product↔Opportunity is many-to-many via OpportunityProduct junction table,
            // not a direct one-to-many — this prevents shadow FK "ProductId" on Opportunities.
            entity.Ignore(e => e.Opportunities);

            // Product↔MarketingCampaign is a conceptual many-to-many with no junction table
            // in the database — prevents EF from creating a shadow junction table.
            entity.Ignore(e => e.MarketingCampaigns);
        });

        // Configure contact info tables
        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Line1).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Line2).HasMaxLength(500);
            entity.Property(e => e.Line3).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(200);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.County).HasMaxLength(100);
            entity.Property(e => e.CountryCode).HasMaxLength(10);
            entity.Property(e => e.Country).HasMaxLength(200);
            entity.Property(e => e.Locality).HasMaxLength(200);
            entity.Property(e => e.AddressXml).HasColumnType("TEXT");
            entity.Property(e => e.Latitude).HasPrecision(10, 6);
            entity.Property(e => e.Longitude).HasPrecision(10, 6);

            // FK to ZipCode
            entity.HasOne(e => e.ZipCodeData)
                .WithMany(z => z.Addresses)
                .HasForeignKey(e => e.ZipCodeId)
                .OnDelete(DeleteBehavior.SetNull);

            // FK to Locality
            entity.HasOne(e => e.LocalityData)
                .WithMany()
                .HasForeignKey(e => e.LocalityId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.ZipCodeId);
            entity.HasIndex(e => e.LocalityId);
            entity.HasIndex(e => e.PostalCode);
            entity.HasIndex(e => e.City);
        });

        modelBuilder.Entity<ContactDetail>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Value).IsRequired().HasMaxLength(1000);
        });

        modelBuilder.Entity<SocialAccount>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.HandleOrUrl).IsRequired().HasMaxLength(2000);
        });

        modelBuilder.Entity<ContactInfoLink>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.OwnerType, e.OwnerId });
            entity.HasIndex(e => new { e.InfoKind, e.InfoId });
            // Explicit FKs to concrete info tables to avoid EF creating ambiguous shadow FKs
            entity.HasOne(e => e.Address)
                .WithMany()
                .HasForeignKey(e => e.AddressId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.ContactDetail)
                .WithMany()
                .HasForeignKey(e => e.ContactDetailId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.SocialAccount)
                .WithMany()
                .HasForeignKey(e => e.SocialAccountId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure consolidated contact info entities
        modelBuilder.Entity<PhoneNumber>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Number).IsRequired().HasMaxLength(30);
            entity.Property(e => e.CountryCode).HasMaxLength(5).HasDefaultValue("+1");
            entity.Property(e => e.AreaCode).HasMaxLength(10);
            entity.Property(e => e.Extension).HasMaxLength(10);
            entity.Property(e => e.FormattedNumber).HasMaxLength(50);
            entity.Property(e => e.Label).HasMaxLength(100);
            entity.Property(e => e.BestTimeToCall).HasMaxLength(100);
            entity.HasIndex(e => e.Number);
            entity.HasIndex(e => e.IsDeleted);
        });

        modelBuilder.Entity<EmailAddress>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Label).HasMaxLength(100);
            entity.Property(e => e.DisplayName).HasMaxLength(200);
            entity.Property(e => e.EmailEngagementScore).HasPrecision(3, 2);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.IsDeleted);
        });

        modelBuilder.Entity<SocialMediaAccount>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.HandleOrUsername).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Platform).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.PlatformOther).HasMaxLength(100);
            entity.Property(e => e.AccountType).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.ProfileUrl).HasMaxLength(500);
            entity.Property(e => e.DisplayName).HasMaxLength(200);
            entity.Property(e => e.EngagementLevel).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(e => e.Platform);
            entity.HasIndex(e => e.HandleOrUsername);
            entity.HasIndex(e => e.IsDeleted);
        });

        // Configure junction tables for consolidated contact info
        modelBuilder.Entity<EntityAddressLink>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EntityType).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.AddressType).HasConversion<string>().HasMaxLength(50).HasDefaultValue(AddressType.Primary);
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
            entity.HasIndex(e => new { e.EntityType, e.EntityId, e.AddressId, e.AddressType }).IsUnique();
            entity.HasOne(e => e.Address)
                .WithMany(a => a.EntityAddressLinks)
                .HasForeignKey(e => e.AddressId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EntityPhoneLink>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EntityType).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.PhoneType).HasConversion<string>().HasMaxLength(50).HasDefaultValue(PhoneType.Office);
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
            entity.HasIndex(e => new { e.EntityType, e.EntityId, e.PhoneId, e.PhoneType }).IsUnique();
            entity.HasOne(e => e.PhoneNumber)
                .WithMany(p => p.EntityPhoneLinks)
                .HasForeignKey(e => e.PhoneId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EntityEmailLink>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EntityType).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.EmailType).HasConversion<string>().HasMaxLength(50).HasDefaultValue(EmailType.General);
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
            entity.HasIndex(e => new { e.EntityType, e.EntityId, e.EmailId, e.EmailType }).IsUnique();
            entity.HasOne(e => e.EmailAddress)
                .WithMany(e => e.EntityEmailLinks)
                .HasForeignKey(e => e.EmailId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EntitySocialMediaLink>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EntityType).HasConversion<string>().HasMaxLength(50);
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
            entity.HasIndex(e => new { e.EntityType, e.EntityId, e.SocialMediaAccountId }).IsUnique();
            entity.HasOne(e => e.SocialMediaAccount)
                .WithMany(s => s.EntitySocialMediaLinks)
                .HasForeignKey(e => e.SocialMediaAccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Lookup tables
        modelBuilder.Entity<LookupCategory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<LookupItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Value).IsRequired().HasMaxLength(500);
            entity.HasOne(e => e.Category).WithMany(c => c.Items).HasForeignKey(e => e.LookupCategoryId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.LookupCategoryId, e.SortOrder });
        });

        // Configure foreign keys from entities to lookups
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasOne(c => c.CurrencyLookup).WithMany().HasForeignKey(c => c.CurrencyLookupId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(c => c.BillingCycleLookup).WithMany().HasForeignKey(c => c.BillingCycleLookupId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasOne(a => a.CurrencyLookup).WithMany().HasForeignKey(a => a.CurrencyLookupId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Contact>(entity =>
        {
            // Ignore alias properties that map to existing columns (Email→EmailPrimary, Title→JobTitle)
            entity.Ignore(e => e.Email);
            entity.Ignore(e => e.Title);

            // Preferred contact method uses LookupItem
            entity.HasOne(c => c.PreferredContactMethodLookup)
                .WithMany()
                .HasForeignKey(c => c.PreferredContactMethodLookupId)
                .OnDelete(DeleteBehavior.SetNull);

            // Contact belongs to Account (one-to-many)
            entity.HasOne(c => c.Account)
                .WithMany(a => a.Contacts)
                .HasForeignKey(c => c.AccountId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(c => c.Preferences)
                .WithMany(p => p.Contacts)
                .HasForeignKey(c => c.PreferencesId)
                .OnDelete(DeleteBehavior.SetNull);

            // Ignore polymorphic ContactInfoLinks — uses OwnerType/OwnerId pattern
            entity.Ignore(e => e.ContactInfoLinks);
        });

        // Configure Interaction
        modelBuilder.Entity<Interaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.HasOne(e => e.Account)
                .WithMany(c => c.Interactions)
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            // Link Interaction -> MarketingCampaign
            entity.HasOne(e => e.Campaign)
                .WithMany()
                .HasForeignKey(e => e.CampaignId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure MarketingCampaign
        modelBuilder.Entity<MarketingCampaign>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Budget).HasPrecision(18, 2);

            // Ignore navigation properties that have no direct FK on the target entity.
            // MarketingCampaign↔Product is conceptual many-to-many with no junction table
            // in the database — prevents EF from creating a shadow junction table.
            entity.Ignore(e => e.Products);

            // Opportunity does not have MarketingCampaignId — prevents shadow FK
            // "MarketingCampaignId" on Opportunities table.
            entity.Ignore(e => e.Opportunities);
        });

        // Configure CampaignMetric
        modelBuilder.Entity<CampaignMetric>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Campaign)
                .WithMany(c => c.Metrics)
                .HasForeignKey(e => e.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();

            // Column mappings for backward compatibility with existing database schema
            entity.Property(e => e.LastLoginAt).HasColumnName("LastLoginAt");
            entity.Property(e => e.EmailVerified).HasColumnName("IsEmailVerified");
            // Role is stored as INT in database matching the UserRole enum values

            // Configure relationships
            entity.HasOne(e => e.Department)
                .WithMany(d => d.Users)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.UserProfile)
                .WithMany(p => p.Users)
                .HasForeignKey(e => e.UserProfileId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.PrimaryGroup)
                .WithMany(g => g.PrimaryUsers)
                .HasForeignKey(e => e.PrimaryGroupId)
                .OnDelete(DeleteBehavior.SetNull);

            // User.UserRoles is a backward-compatibility alias for User.RoleAssignments.
            // RoleAssignments is already configured via UserRoleAssignment Fluent API.
            // Without Ignore, EF creates a duplicate relationship with shadow FK "UserId1".
            entity.Ignore(e => e.UserRoles);

            // API User columns
            entity.Property(e => e.ApiKeyHash).HasMaxLength(128);
            entity.Property(e => e.ApiKeyPrefix).HasMaxLength(12);
            entity.Property(e => e.ApiUserDescription).HasMaxLength(500);
            entity.HasIndex(e => e.ApiKeyHash).HasDatabaseName("IX_Users_ApiKeyHash");
            entity.HasIndex(e => e.IsApiUser).HasDatabaseName("IX_Users_IsApiUser");
        });

        // Configure RefreshToken
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Token).IsRequired().HasMaxLength(128);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.DeviceInfo).HasMaxLength(500);
            entity.Property(e => e.ReplacedByToken).HasMaxLength(128);
            entity.Property(e => e.RevokedReason).HasMaxLength(200);

            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ExpiresAt);

            entity.HasOne(e => e.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure UserGroup
        modelBuilder.Entity<UserGroup>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        // Configure UserGroupMember (junction table for User-UserGroup many-to-many)
        modelBuilder.Entity<UserGroupMember>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Unique constraint: user can only be member of a group once
            entity.HasIndex(e => new { e.UserId, e.UserGroupId }).IsUnique();

            entity.HasOne(e => e.User)
                .WithMany(u => u.GroupMemberships)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.UserGroup)
                .WithMany(g => g.Members)
                .HasForeignKey(e => e.UserGroupId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Role (RBAC - SYS-012)
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.HierarchyLevel).IsRequired().HasDefaultValue(3); // Default to User level
            entity.Property(e => e.IsSystemDefined).IsRequired().HasDefaultValue(false);
            entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);

            // Indexes for common queries
            entity.HasIndex(e => e.HierarchyLevel);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => new { e.IsActive, e.IsSystemDefined });
        });

        // Configure Permission (RBAC - SYS-012)
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255); // Format: {Module}.{Action}
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Module).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(50); // Create, Update, Delete, Export, View, etc.
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsSystemDefined).IsRequired().HasDefaultValue(false);
            entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);

            // Indexes for common queries
            entity.HasIndex(e => e.Module);
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => new { e.Module, e.Category });
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => new { e.IsActive, e.IsSystemDefined });
        });

        // Configure RolePermission (Junction table for Role-Permission many-to-many)
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Unique constraint: role can only have permission once
            entity.HasIndex(e => new { e.RoleId, e.PermissionId }).IsUnique();

            entity.HasOne(e => e.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(e => e.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.AssignedAt).IsRequired().HasDefaultValueSql(providerStrategy.GetUtcNowSql());

            // Indexes for common queries
            entity.HasIndex(e => e.RoleId);
            entity.HasIndex(e => e.PermissionId);
        });

        // Configure UserRoleAssignment (Junction table for User-Role many-to-many with temporal validity)
        modelBuilder.Entity<UserRoleAssignment>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Unique constraint: user can only have a role once (active roles)
            // Multiple entries allowed with different EffectiveFrom/EffectiveTo for history
            entity.HasIndex(e => new { e.UserId, e.RoleId, e.EffectiveFrom });

            entity.HasOne(e => e.User)
                .WithMany(u => u.RoleAssignments)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.EffectiveFrom).IsRequired().HasDefaultValueSql(providerStrategy.GetUtcNowSql());
            entity.Property(e => e.EffectiveTo).IsRequired(false);
            entity.Property(e => e.AssignedAt).IsRequired().HasDefaultValueSql(providerStrategy.GetUtcNowSql());
            entity.Property(e => e.Notes).HasMaxLength(500);

            // Indexes for common queries
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.RoleId);
            entity.HasIndex(e => new { e.UserId, e.EffectiveFrom, e.EffectiveTo });
            entity.HasIndex(e => new { e.EffectiveFrom, e.EffectiveTo });
        });

        // Configure SystemSettings
        modelBuilder.Entity<SystemSettings>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CompanyName).HasMaxLength(255);
            entity.Property(e => e.CompanyLogoUrl).HasMaxLength(1000);
            entity.Property(e => e.PrimaryColor).HasMaxLength(20);
            entity.Property(e => e.SecondaryColor).HasMaxLength(20);
            entity.Property(e => e.DateFormat).HasMaxLength(50);
            entity.Property(e => e.TimeFormat).HasMaxLength(50);
            entity.Property(e => e.DefaultCurrency).HasMaxLength(10);
            entity.Property(e => e.DefaultTimezone).HasMaxLength(100);
            entity.Property(e => e.DefaultLanguage).HasMaxLength(10);

            entity.HasIndex(e => e.SelectedPaletteId);

            entity.HasOne(e => e.SelectedPalette)
                .WithMany()
                .HasForeignKey(e => e.SelectedPaletteId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure LLMProviderSetting
        modelBuilder.Entity<LLMProviderSetting>(entity =>
        {
            entity.ToTable("llm_provider_settings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SettingKey).HasColumnName("setting_key").IsRequired().HasMaxLength(100);
            entity.Property(e => e.SettingValue).HasColumnName("setting_value").IsRequired();
            entity.Property(e => e.ValueType).HasColumnName("value_type").HasMaxLength(50).HasDefaultValue("string");
            entity.Property(e => e.Category).HasColumnName("category").HasMaxLength(100).HasDefaultValue("general");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsEncrypted).HasColumnName("is_encrypted").HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
            entity.HasIndex(e => e.SettingKey).IsUnique();
            entity.HasIndex(e => e.Category);
        });

        // Configure Department
        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.DepartmentCode).HasMaxLength(20);

            // Configure hierarchical relationship
            entity.HasOne(e => e.ParentDepartment)
                .WithMany(d => d.SubDepartments)
                .HasForeignKey(e => e.ParentDepartmentId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure UserProfile
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.AccessiblePages).HasDefaultValue("[]");

            entity.HasOne(e => e.Department)
                .WithMany(d => d.Profiles)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Subscription (formerly Account - the contract/billing entity)
        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SubscriptionNumber).HasMaxLength(100);
            entity.Property(e => e.ContractReference).HasMaxLength(200);
            entity.Property(e => e.ContractFileName).HasMaxLength(1000);
            entity.Property(e => e.ContractFilePath).HasMaxLength(2000);
            entity.Property(e => e.ContractContentType).HasMaxLength(200);
            entity.Property(e => e.Currency).HasMaxLength(10);
            entity.Property(e => e.BillingCycle).HasMaxLength(50);
            entity.Property(e => e.BillingContactEmail).HasMaxLength(255);
            entity.HasIndex(e => e.SubscriptionNumber).IsUnique(false);

            entity.HasOne(e => e.Account)
                .WithMany(c => c.Subscriptions)
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.Subscriptions)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.SetNull);

            // Ignore polymorphic ContactInfoLinks — uses OwnerType/OwnerId pattern
            entity.Ignore(e => e.ContactInfoLinks);

            // Opportunity does not have SubscriptionId — prevents shadow FK
            // "SubscriptionId" on Opportunities table.
            entity.Ignore(e => e.Opportunities);
        });

        // Configure CrmTask
        modelBuilder.Entity<CrmTask>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Subject).IsRequired().HasMaxLength(255);

            entity.HasOne(e => e.Account)
                .WithMany()
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Opportunity)
                .WithMany()
                .HasForeignKey(e => e.OpportunityId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.AssignedToUser)
                .WithMany()
                .HasForeignKey(e => e.AssignedToUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.ParentTask)
                .WithMany(t => t.SubTasks)
                .HasForeignKey(e => e.ParentTaskId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.DueDate);
            entity.HasIndex(e => e.Status);

            // Link CrmTask -> MarketingCampaign
            entity.HasOne(e => e.Campaign)
                .WithMany()
                .HasForeignKey(e => e.CampaignId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure Note
        modelBuilder.Entity<Note>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Content).IsRequired();

            entity.HasOne(e => e.Account)
                .WithMany()
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Opportunity)
                .WithMany()
                .HasForeignKey(e => e.OpportunityId)
                .OnDelete(DeleteBehavior.Cascade);

            // Link Note -> MarketingCampaign
            entity.HasOne(e => e.Campaign)
                .WithMany()
                .HasForeignKey(e => e.CampaignId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.IsPinned);
        });

        // Configure Quote
        modelBuilder.Entity<Quote>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.QuoteNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);

            entity.HasOne(e => e.Account)
                .WithMany()
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Contact)
                .WithMany()
                .HasForeignKey(e => e.ContactId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Opportunity)
                .WithMany()
                .HasForeignKey(e => e.OpportunityId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.AssignedToUser)
                .WithMany()
                .HasForeignKey(e => e.AssignedToUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.RelationshipManager)
                .WithMany()
                .HasForeignKey(e => e.RelationshipManagerId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.ParentQuote)
                .WithMany(q => q.Revisions)
                .HasForeignKey(e => e.ParentQuoteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.QuoteNumber).IsUnique();
            entity.HasIndex(e => e.Status);
        });

        // Configure QuoteLineItem
        modelBuilder.Entity<QuoteLineItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.SKU).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.UnitOfMeasure).HasMaxLength(50);
            entity.Property(e => e.DiscountReason).HasMaxLength(500);
            entity.Property(e => e.TaxCode).HasMaxLength(50);
            entity.Property(e => e.BillingPeriod).HasMaxLength(50);
            entity.Property(e => e.InternalNotes).HasMaxLength(2000);
            entity.Property(e => e.QuoteNotes).HasMaxLength(2000);

            // Precision for decimal fields
            entity.Property(e => e.Quantity).HasPrecision(18, 4);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.ListPrice).HasPrecision(18, 2);
            entity.Property(e => e.CostPrice).HasPrecision(18, 2);
            entity.Property(e => e.DiscountPercent).HasPrecision(5, 2);
            entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
            entity.Property(e => e.TaxRate).HasPrecision(5, 2);
            entity.Property(e => e.Subtotal).HasPrecision(18, 2);
            entity.Property(e => e.TotalDiscount).HasPrecision(18, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.Total).HasPrecision(18, 2);
            entity.Property(e => e.Margin).HasPrecision(18, 2);

            // Quote relationship
            entity.HasOne(e => e.Quote)
                .WithMany(q => q.QuoteLineItems)
                .HasForeignKey(e => e.QuoteId)
                .OnDelete(DeleteBehavior.Cascade);

            // Product relationship
            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.SetNull);

            // Self-referencing for bundle items
            entity.HasOne(e => e.ParentLineItem)
                .WithMany(e => e.BundleItems)
                .HasForeignKey(e => e.ParentLineItemId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.QuoteId, e.LineNumber });
            entity.HasIndex(e => e.SKU);
        });

        // Configure Activity
        modelBuilder.Entity<Activity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
            entity.Property(e => e.EntityType).HasMaxLength(100);
            entity.Property(e => e.SecondaryEntityType).HasMaxLength(100);
            // Removed HasMaxLength(100) from ActivityType (enum, not string)
            // entity.Property(e => e.ActivityType).HasMaxLength(100);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Account)
                .WithMany()
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Opportunity)
                .WithMany()
                .HasForeignKey(e => e.OpportunityId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.ActivityDate);
            entity.HasIndex(e => e.ActivityType);
            entity.HasIndex(e => new { e.EntityType, e.EntityId });

            // Link Activity -> MarketingCampaign
            entity.HasOne(e => e.Campaign)
                .WithMany()
                .HasForeignKey(e => e.CampaignId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure Lead (3NF)
        modelBuilder.Entity<Lead>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(100);
            entity.Property(e => e.CompanyName).HasMaxLength(255);
            entity.Property(e => e.Website).HasMaxLength(500);
            entity.Property(e => e.Region).HasMaxLength(100);
            entity.Property(e => e.Tags).HasMaxLength(2000);
            entity.Property(e => e.QualificationNotes).HasMaxLength(4000);

            // Lead -> User (Owner)
            entity.HasOne(e => e.Owner)
                .WithMany()
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.SetNull);

            // Lead -> Campaign (EXPLICIT: only GeneratedLeads inverse)
            // This fixes the warning about multiple Lead-Campaign relationships
            entity.HasOne(e => e.Campaign)
                .WithMany(c => c.GeneratedLeads)
                .HasForeignKey(e => e.CampaignId)
                .OnDelete(DeleteBehavior.SetNull);

            // Lead -> Customer
            entity.HasOne(e => e.Account)
                .WithMany()
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.SetNull);

            // Lead -> Contact
            entity.HasOne(e => e.Contact)
                .WithMany()
                .HasForeignKey(e => e.ContactId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Score);
        });

        // Configure MarketingCampaign Lead collections (without inverse navigation)
        // ConvertedLeads and TouchedLeads don't have FK in Lead, so they need junction tables or explicit ignore
        modelBuilder.Entity<MarketingCampaign>(entity =>
        {
            // Ignore collections that don't have proper FK relationships in Lead entity
            // These would need junction tables for proper many-to-many relationships
            entity.Ignore(e => e.ConvertedLeads);
            entity.Ignore(e => e.TouchedLeads);
        });

        // Configure LeadProductInterest (junction table)
        modelBuilder.Entity<LeadProductInterest>(entity =>
        {
            entity.HasKey(e => new { e.LeadId, e.ProductId });
            entity.Property(e => e.Notes).HasMaxLength(1000);

            entity.HasOne(e => e.Lead)
                .WithMany(l => l.ProductInterests)
                .HasForeignKey(e => e.LeadId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Opportunity (3NF) - Additional property configurations
        // NOTE: Main relationship configurations are earlier in the file
        modelBuilder.Entity<Opportunity>(entity =>
        {
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.Region).HasMaxLength(100);
            entity.Property(e => e.SolutionNotes).HasMaxLength(4000);
            entity.Property(e => e.QualificationNotes).HasMaxLength(4000);

            // Opportunity -> Contact (Primary)
            entity.HasOne(e => e.PrimaryContact)
                .WithMany()
                .HasForeignKey(e => e.PrimaryContactId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.Stage);
            entity.HasIndex(e => e.ExpectedCloseDate);
        });

        // Configure OpportunityProduct (junction table)
        modelBuilder.Entity<OpportunityProduct>(entity =>
        {
            entity.HasKey(e => new { e.OpportunityId, e.ProductId });
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.DiscountPercent).HasPrecision(5, 2);
            entity.Property(e => e.LineTotal).HasPrecision(18, 2);
            entity.Property(e => e.Notes).HasMaxLength(1000);

            entity.HasOne(e => e.Opportunity)
                .WithMany(o => o.Products)
                .HasForeignKey(e => e.OpportunityId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Customer -> Lead relationship (ConvertedFromLead)
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasOne(e => e.ConvertedFromLead)
                .WithMany()
                .HasForeignKey(e => e.ConvertedFromLeadId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.SourceCampaign)
                .WithMany()
                .HasForeignKey(e => e.SourceCampaignId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure ServiceRequestCategory
        modelBuilder.Entity<ServiceRequestCategory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IconName).HasMaxLength(50);
            entity.Property(e => e.ColorCode).HasMaxLength(20);
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.DisplayOrder);
        });

        // Configure ServiceRequestSubcategory
        modelBuilder.Entity<ServiceRequestSubcategory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.DisplayOrder);

            entity.HasOne(e => e.Category)
                .WithMany(c => c.Subcategories)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure ServiceRequestCustomFieldDefinition
        modelBuilder.Entity<ServiceRequestCustomFieldDefinition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.FieldKey).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.DefaultValue).HasMaxLength(500);
            entity.Property(e => e.Placeholder).HasMaxLength(200);
            entity.Property(e => e.HelpText).HasMaxLength(500);
            entity.Property(e => e.DropdownOptions).HasMaxLength(2000);
            entity.Property(e => e.ValidationPattern).HasMaxLength(500);
            entity.Property(e => e.ValidationMessage).HasMaxLength(200);
            entity.Property(e => e.MinValue).HasPrecision(18, 4);
            entity.Property(e => e.MaxValue).HasPrecision(18, 4);
            entity.HasIndex(e => e.FieldKey);
            entity.HasIndex(e => e.DisplayOrder);

            entity.HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Subcategory)
                .WithMany()
                .HasForeignKey(e => e.SubcategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure ServiceRequestCustomFieldValue
        modelBuilder.Entity<ServiceRequestCustomFieldValue>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TextValue).HasColumnType("TEXT");
            entity.Property(e => e.NumericValue).HasPrecision(18, 4);
            entity.HasIndex(e => new { e.ServiceRequestId, e.CustomFieldDefinitionId }).IsUnique();

            entity.HasOne(e => e.ServiceRequest)
                .WithMany(sr => sr.CustomFieldValues)
                .HasForeignKey(e => e.ServiceRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.CustomFieldDefinition)
                .WithMany(f => f.FieldValues)
                .HasForeignKey(e => e.CustomFieldDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure ServiceRequest
        modelBuilder.Entity<ServiceRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TicketNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Subject).IsRequired().HasMaxLength(500);
            // Use TEXT column type for large text fields to avoid row size limits
            entity.Property(e => e.Description).HasColumnType("TEXT");
            entity.Property(e => e.RequesterName).HasMaxLength(200);
            entity.Property(e => e.RequesterEmail).HasMaxLength(200);
            entity.Property(e => e.RequesterPhone).HasMaxLength(50);
            entity.Property(e => e.ExternalReferenceId).HasMaxLength(500);
            entity.Property(e => e.SourcePhoneNumber).HasMaxLength(50);
            entity.Property(e => e.SourceEmailAddress).HasMaxLength(200);
            entity.Property(e => e.ConversationId).HasMaxLength(500);
            entity.Property(e => e.ResolutionSummary).HasColumnType("TEXT");
            entity.Property(e => e.ResolutionCode).HasMaxLength(100);
            entity.Property(e => e.RootCause).HasColumnType("TEXT");
            entity.Property(e => e.CustomerFeedback).HasColumnType("TEXT");
            entity.Property(e => e.Tags).HasMaxLength(500);
            entity.Property(e => e.InternalNotes).HasColumnType("TEXT");
            entity.Property(e => e.EstimatedEffortHours).HasPrecision(18, 2);
            entity.Property(e => e.ActualEffortHours).HasPrecision(18, 2);

            entity.HasIndex(e => e.TicketNumber).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Priority);
            entity.HasIndex(e => e.Channel);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.ResponseDueDate);
            entity.HasIndex(e => e.ResolutionDueDate);

            entity.HasOne(e => e.Category)
                .WithMany(c => c.ServiceRequests)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Subcategory)
                .WithMany(s => s.ServiceRequests)
                .HasForeignKey(e => e.SubcategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Account)
                .WithMany()
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Contact)
                .WithMany()
                .HasForeignKey(e => e.ContactId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.AssignedToUser)
                .WithMany()
                .HasForeignKey(e => e.AssignedToUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.AssignedToGroup)
                .WithMany()
                .HasForeignKey(e => e.AssignedToGroupId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure Tags
            modelBuilder.Entity<CRM.Core.Entities.Tag>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Color).HasMaxLength(20);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasIndex(e => e.IsDeleted);
            });

            modelBuilder.Entity<CRM.Core.Entities.EntityTag>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.TagName).HasMaxLength(200);

                // Unique constraint: same tag can only be assigned once per entity
                entity.HasIndex(e => new { e.EntityType, e.EntityId, e.TagId }).IsUnique();
                entity.HasIndex(e => new { e.EntityType, e.EntityId });
                entity.HasIndex(e => e.TagId);

                // Navigation to Tag with cascade delete
                entity.HasOne(e => e.Tag)
                    .WithMany(t => t.EntityTags)
                    .HasForeignKey(e => e.TagId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CRM.Core.Entities.CustomField>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Key).HasMaxLength(200);
                entity.Property(e => e.Value).HasColumnType("TEXT");
                entity.HasIndex(e => new { e.EntityType, e.EntityId });
            });

            // Configure Conversation
            modelBuilder.Entity<Conversation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ConversationId).HasMaxLength(100);
                entity.Property(e => e.Subject).HasMaxLength(500);
                entity.Property(e => e.LastMessagePreview).HasMaxLength(500);
                entity.Property(e => e.ParticipantAddress).HasMaxLength(500);
                entity.Property(e => e.ParticipantName).HasMaxLength(200);
                entity.HasIndex(e => e.ConversationId).IsUnique();
                entity.HasIndex(e => e.Status);

                // Ignore Messages navigation - relationship is via string ConversationId, not FK
                // This prevents EF from creating shadow FK ConversationId1 on CommunicationMessage
                entity.Ignore(e => e.Messages);
            });

            // Configure CommunicationMessage
            modelBuilder.Entity<CommunicationMessage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Subject).HasMaxLength(1000);
                entity.Property(e => e.FromAddress).HasMaxLength(500);
                entity.Property(e => e.FromName).HasMaxLength(200);
                entity.Property(e => e.ToAddress).HasMaxLength(500);
                entity.Property(e => e.ToName).HasMaxLength(200);
                entity.Property(e => e.ConversationId).HasMaxLength(100);
                entity.Property(e => e.ExternalMessageId).HasMaxLength(500);

                // CommunicationMessage -> Channel (with inverse navigation on CommunicationChannel)
                // Using .WithMany(c => c.Messages) prevents EF from treating
                // CommunicationChannel.Messages as a separate relationship with shadow FK.
                entity.HasOne(e => e.Channel)
                    .WithMany(c => c.Messages)
                    .HasForeignKey(e => e.ChannelId)
                    .OnDelete(DeleteBehavior.Restrict);

                // CommunicationMessage -> ParentMessage (self-referencing for threading)
                entity.HasOne(e => e.ParentMessage)
                    .WithMany(m => m.Replies)
                    .HasForeignKey(e => e.ParentMessageId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Ignore navigation to Conversation by string ID - handled via ConversationId string field
                // This prevents EF from creating shadow FK ConversationId1
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.Direction);
                entity.HasIndex(e => e.SentAt);
                entity.HasIndex(e => e.ConversationId);
            });

            // Configure ZipCodes (Master Data)
            modelBuilder.Entity<ZipCode>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Country).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CountryCode).IsRequired().HasMaxLength(10);
                entity.Property(e => e.PostalCode).IsRequired().HasMaxLength(20);
                entity.Property(e => e.City).IsRequired().HasMaxLength(200);
                entity.Property(e => e.State).HasMaxLength(100);
                entity.Property(e => e.StateCode).HasMaxLength(10);
                entity.Property(e => e.County).HasMaxLength(100);
                entity.Property(e => e.CountyCode).HasMaxLength(20);
                entity.Property(e => e.Community).HasMaxLength(100);
                entity.Property(e => e.CommunityCode).HasMaxLength(20);
                entity.Property(e => e.Latitude).HasPrecision(10, 6);
                entity.Property(e => e.Longitude).HasPrecision(10, 6);
                entity.HasIndex(e => e.PostalCode);
                entity.HasIndex(e => e.CountryCode);
                entity.HasIndex(e => new { e.CountryCode, e.PostalCode });
                entity.HasIndex(e => e.City);
                entity.HasIndex(e => e.State);

                // Navigation to Localities
                entity.HasMany(e => e.Localities)
                    .WithOne(l => l.ZipCode)
                    .HasForeignKey(l => l.ZipCodeId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Navigation to Addresses
                entity.HasMany(e => e.Addresses)
                    .WithOne(a => a.ZipCodeData)
                    .HasForeignKey(a => a.ZipCodeId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure Localities
            modelBuilder.Entity<Locality>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.AlternateName).HasMaxLength(200);
                entity.Property(e => e.City).IsRequired().HasMaxLength(200);
                entity.Property(e => e.StateCode).HasMaxLength(10);
                entity.Property(e => e.CountryCode).IsRequired().HasMaxLength(10);
                entity.Property(e => e.Latitude).HasPrecision(10, 6);
                entity.Property(e => e.Longitude).HasPrecision(10, 6);
                entity.HasIndex(e => new { e.City, e.CountryCode });
                entity.HasIndex(e => new { e.ZipCodeId });
                entity.HasIndex(e => e.Name);
            });

            // Configure SocialMediaFollow
            modelBuilder.Entity<SocialMediaFollow>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EntityType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Notes).HasMaxLength(500);

                entity.HasOne(e => e.SocialMediaAccount)
                    .WithMany(s => s.Followers)
                    .HasForeignKey(e => e.SocialMediaAccountId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.FollowedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.FollowedByUserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.SocialMediaAccountId, e.FollowedByUserId }).IsUnique();
                entity.HasIndex(e => e.FollowedByUserId);
                entity.HasIndex(e => new { e.EntityType, e.EntityId });
            });

            entity.HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.RelatedOpportunity)
                .WithMany()
                .HasForeignKey(e => e.RelatedOpportunityId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.RelatedProduct)
                .WithMany()
                .HasForeignKey(e => e.RelatedProductId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.ParentServiceRequest)
                .WithMany(sr => sr.ChildServiceRequests)
                .HasForeignKey(e => e.ParentServiceRequestId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure Cloud Deployment entities
        modelBuilder.Entity<CloudProvider>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.AccessKeyId).HasMaxLength(500);
            entity.Property(e => e.SecretAccessKey).HasMaxLength(2000);
            entity.Property(e => e.TenantId).HasMaxLength(200);
            entity.Property(e => e.SubscriptionId).HasMaxLength(200);
            entity.Property(e => e.ProjectId).HasMaxLength(200);
            entity.Property(e => e.Region).HasMaxLength(100);
            entity.Property(e => e.Endpoint).HasMaxLength(500);
            entity.Property(e => e.Configuration).HasColumnType("TEXT");
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.ProviderType);
        });

        modelBuilder.Entity<CloudDeployment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.ClusterName).HasMaxLength(200);
            entity.Property(e => e.Namespace).HasMaxLength(100);
            entity.Property(e => e.ResourceGroup).HasMaxLength(200);
            entity.Property(e => e.VpcId).HasMaxLength(100);
            entity.Property(e => e.SubnetIds).HasMaxLength(500);
            entity.Property(e => e.BackendImage).HasMaxLength(500);
            entity.Property(e => e.FrontendImage).HasMaxLength(500);
            entity.Property(e => e.DatabaseImage).HasMaxLength(500);
            entity.Property(e => e.BackendVersion).HasMaxLength(50);
            entity.Property(e => e.FrontendVersion).HasMaxLength(50);
            entity.Property(e => e.FrontendUrl).HasMaxLength(500);
            entity.Property(e => e.ApiUrl).HasMaxLength(500);
            entity.Property(e => e.DatabaseHost).HasMaxLength(200);
            entity.Property(e => e.SslCertificateArn).HasMaxLength(500);
            entity.Property(e => e.DomainName).HasMaxLength(300);
            entity.Property(e => e.LastError).HasMaxLength(2000);
            entity.Property(e => e.EnvironmentVariables).HasColumnType("TEXT");
            entity.Property(e => e.ResourceConfiguration).HasColumnType("TEXT");
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Status);

            entity.HasOne(e => e.CloudProvider)
                .WithMany(p => p.Deployments)
                .HasForeignKey(e => e.CloudProviderId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DeploymentAttempt>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AttemptNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.GitCommitHash).HasMaxLength(100);
            entity.Property(e => e.GitBranch).HasMaxLength(200);
            entity.Property(e => e.BuildNumber).HasMaxLength(50);
            entity.Property(e => e.BackendImageTag).HasMaxLength(100);
            entity.Property(e => e.FrontendImageTag).HasMaxLength(100);
            entity.Property(e => e.BuildLog).HasColumnType(longTextType);
            entity.Property(e => e.DeployLog).HasColumnType(longTextType);
            entity.Property(e => e.ErrorMessage).HasMaxLength(2000);
            entity.Property(e => e.ErrorStackTrace).HasColumnType(textType);
            entity.Property(e => e.TriggerType).HasMaxLength(50);
            entity.HasIndex(e => e.CloudDeploymentId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.StartedAt);

            entity.HasOne(e => e.CloudDeployment)
                .WithMany(d => d.Attempts)
                .HasForeignKey(e => e.CloudDeploymentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<HealthCheckLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ApiResponse).HasMaxLength(1000);
            entity.Property(e => e.FrontendResponse).HasMaxLength(1000);
            entity.Property(e => e.DatabaseResponse).HasMaxLength(1000);
            entity.Property(e => e.ErrorDetails).HasMaxLength(2000);
            entity.HasIndex(e => e.CloudDeploymentId);
            entity.HasIndex(e => e.CheckedAt);
            entity.HasIndex(e => e.Status);

            entity.HasOne(e => e.CloudDeployment)
                .WithMany(d => d.HealthChecks)
                .HasForeignKey(e => e.CloudDeploymentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Dashboard configuration
        modelBuilder.Entity<Dashboard>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IconName).HasMaxLength(50);
            entity.Property(e => e.LayoutConfig).HasColumnType("TEXT");
            entity.Property(e => e.AllowedRoles).HasMaxLength(500);
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.IsDefault);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.OwnerId);

            entity.HasOne(e => e.Owner)
                .WithMany()
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<DashboardWidget>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Subtitle).HasMaxLength(300);
            entity.Property(e => e.DataSource).IsRequired().HasMaxLength(200);
            entity.Property(e => e.IconName).HasMaxLength(50);
            entity.Property(e => e.Color).HasMaxLength(20);
            entity.Property(e => e.BackgroundColor).HasMaxLength(100);
            entity.Property(e => e.NavigationLink).HasMaxLength(300);
            entity.Property(e => e.ConfigJson).HasColumnType("TEXT");
            entity.HasIndex(e => e.DashboardId);
            entity.HasIndex(e => e.DisplayOrder);
            entity.HasIndex(e => e.WidgetType);

            entity.HasOne(e => e.Dashboard)
                .WithMany(d => d.Widgets)
                .HasForeignKey(e => e.DashboardId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Workflow Definition configuration
        modelBuilder.Entity<WorkflowDefinition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.WorkflowKey).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.IconName).HasMaxLength(50);
            entity.Property(e => e.Color).HasMaxLength(20);
            entity.Property(e => e.Tags).HasMaxLength(500);
            entity.Property(e => e.Metadata).HasColumnType("TEXT");
            entity.HasIndex(e => e.WorkflowKey).IsUnique();
            entity.HasIndex(e => e.EntityType);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.OwnerId);

            entity.HasOne(e => e.Owner)
                .WithMany()
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Workflow Version configuration
        modelBuilder.Entity<WorkflowVersion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Label).HasMaxLength(50);
            entity.Property(e => e.ChangeLog).HasMaxLength(1000);
            entity.Property(e => e.CanvasLayout).HasColumnType("TEXT");
            entity.HasIndex(e => new { e.WorkflowDefinitionId, e.VersionNumber }).IsUnique();
            entity.HasIndex(e => e.Status);

            entity.HasOne(e => e.WorkflowDefinition)
                .WithMany(d => d.Versions)
                .HasForeignKey(e => e.WorkflowDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.PublishedBy)
                .WithMany()
                .HasForeignKey(e => e.PublishedById)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Workflow Node configuration
        modelBuilder.Entity<WorkflowNode>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NodeKey).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.NodeSubType).HasMaxLength(100);
            entity.Property(e => e.IconName).HasMaxLength(50);
            entity.Property(e => e.Color).HasMaxLength(20);
            entity.Property(e => e.Configuration).HasColumnType("TEXT");
            entity.Property(e => e.PositionX).HasPrecision(10, 2);
            entity.Property(e => e.PositionY).HasPrecision(10, 2);
            entity.Property(e => e.Width).HasPrecision(10, 2);
            entity.Property(e => e.Height).HasPrecision(10, 2);
            entity.HasIndex(e => new { e.WorkflowVersionId, e.NodeKey }).IsUnique();
            entity.HasIndex(e => e.NodeType);
            entity.HasIndex(e => e.IsStartNode);
            entity.HasIndex(e => e.IsEndNode);

            entity.HasOne(e => e.WorkflowVersion)
                .WithMany(v => v.Nodes)
                .HasForeignKey(e => e.WorkflowVersionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Workflow Transition configuration
        modelBuilder.Entity<WorkflowTransition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TransitionKey).HasMaxLength(100);
            entity.Property(e => e.Label).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ConditionExpression).HasColumnType("TEXT");
            entity.Property(e => e.SourceHandle).HasMaxLength(20);
            entity.Property(e => e.TargetHandle).HasMaxLength(20);
            entity.Property(e => e.LineStyle).HasMaxLength(20);
            entity.Property(e => e.Color).HasMaxLength(20);
            entity.Property(e => e.AnimationStyle).HasMaxLength(20);
            entity.HasIndex(e => e.WorkflowVersionId);
            entity.HasIndex(e => e.SourceNodeId);
            entity.HasIndex(e => e.TargetNodeId);

            entity.HasOne(e => e.WorkflowVersion)
                .WithMany(v => v.Transitions)
                .HasForeignKey(e => e.WorkflowVersionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.SourceNode)
                .WithMany(n => n.OutgoingTransitions)
                .HasForeignKey(e => e.SourceNodeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.TargetNode)
                .WithMany(n => n.IncomingTransitions)
                .HasForeignKey(e => e.TargetNodeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Workflow Instance configuration
        modelBuilder.Entity<WorkflowInstance>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CorrelationId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TriggerEvent).HasMaxLength(100);
            entity.Property(e => e.InputData).HasColumnType("TEXT");
            entity.Property(e => e.StateData).HasColumnType("TEXT");
            entity.Property(e => e.OutputData).HasColumnType("TEXT");
            entity.Property(e => e.ErrorMessage).HasColumnType("TEXT");
            entity.Property(e => e.ErrorStackTrace).HasColumnType("TEXT");
            entity.Property(e => e.CancellationReason).HasMaxLength(500);
            entity.HasIndex(e => e.CorrelationId).IsUnique();
            entity.HasIndex(e => e.WorkflowDefinitionId);
            entity.HasIndex(e => e.WorkflowVersionId);
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ScheduledAt);
            entity.HasIndex(e => e.NextRetryAt);

            entity.HasOne(e => e.WorkflowDefinition)
                .WithMany(d => d.Instances)
                .HasForeignKey(e => e.WorkflowDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.WorkflowVersion)
                .WithMany()
                .HasForeignKey(e => e.WorkflowVersionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CurrentNode)
                .WithMany()
                .HasForeignKey(e => e.CurrentNodeId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.TriggeredBy)
                .WithMany()
                .HasForeignKey(e => e.TriggeredById)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.ParentInstance)
                .WithMany(i => i.ChildInstances)
                .HasForeignKey(e => e.ParentInstanceId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Workflow Node Instance configuration
        modelBuilder.Entity<WorkflowNodeInstance>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.InputData).HasColumnType("TEXT");
            entity.Property(e => e.OutputData).HasColumnType("TEXT");
            entity.Property(e => e.ErrorMessage).HasColumnType("TEXT");
            entity.Property(e => e.ErrorStackTrace).HasColumnType("TEXT");
            entity.Property(e => e.SkipReason).HasMaxLength(500);
            entity.Property(e => e.WorkerId).HasMaxLength(100);
            entity.HasIndex(e => e.WorkflowInstanceId);
            entity.HasIndex(e => e.WorkflowNodeId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.NextRetryAt);

            entity.HasOne(e => e.WorkflowInstance)
                .WithMany(i => i.NodeInstances)
                .HasForeignKey(e => e.WorkflowInstanceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.WorkflowNode)
                .WithMany(n => n.NodeInstances)
                .HasForeignKey(e => e.WorkflowNodeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.TransitionTaken)
                .WithMany()
                .HasForeignKey(e => e.TransitionTakenId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Workflow Task configuration
        modelBuilder.Entity<WorkflowTask>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.QueueName).HasMaxLength(100);
            entity.Property(e => e.LockedByWorkerId).HasMaxLength(100);
            entity.Property(e => e.AssignedToRole).HasMaxLength(100);
            entity.Property(e => e.InputData).HasColumnType("TEXT");
            entity.Property(e => e.OutputData).HasColumnType("TEXT");
            entity.Property(e => e.FormSchema).HasColumnType("TEXT");
            entity.Property(e => e.FormData).HasColumnType("TEXT");
            entity.Property(e => e.ErrorMessage).HasColumnType("TEXT");
            entity.Property(e => e.ErrorStackTrace).HasColumnType("TEXT");
            entity.Property(e => e.DeadLetterReason).HasMaxLength(500);
            entity.HasIndex(e => e.WorkflowInstanceId);
            entity.HasIndex(e => e.WorkflowNodeId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.QueueName);
            entity.HasIndex(e => e.ScheduledAt);
            entity.HasIndex(e => e.Priority);
            entity.HasIndex(e => e.IsDeadLetter);
            entity.HasIndex(e => e.AssignedToId);
            entity.HasIndex(e => e.LockExpiresAt);

            entity.HasOne(e => e.WorkflowInstance)
                .WithMany(i => i.Tasks)
                .HasForeignKey(e => e.WorkflowInstanceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.WorkflowNode)
                .WithMany()
                .HasForeignKey(e => e.WorkflowNodeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.NodeInstance)
                .WithMany()
                .HasForeignKey(e => e.NodeInstanceId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.AssignedTo)
                .WithMany()
                .HasForeignKey(e => e.AssignedToId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Workflow Log configuration
        modelBuilder.Entity<WorkflowLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.Message).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.Details).HasColumnType("TEXT");
            entity.Property(e => e.WorkerId).HasMaxLength(100);
            entity.Property(e => e.ExceptionType).HasMaxLength(200);
            entity.Property(e => e.StackTrace).HasColumnType("TEXT");
            entity.HasIndex(e => e.WorkflowInstanceId);
            entity.HasIndex(e => e.WorkflowNodeId);
            entity.HasIndex(e => e.Level);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.Category);

            entity.HasOne(e => e.WorkflowInstance)
                .WithMany(i => i.Logs)
                .HasForeignKey(e => e.WorkflowInstanceId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.WorkflowNode)
                .WithMany()
                .HasForeignKey(e => e.WorkflowNodeId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.NodeInstance)
                .WithMany()
                .HasForeignKey(e => e.NodeInstanceId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // WorkflowSchedule configuration
        modelBuilder.Entity<WorkflowSchedule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CronExpression).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TimeZone).HasMaxLength(100);
            entity.Property(e => e.ContextData).HasColumnType("TEXT");
            entity.HasOne(e => e.WorkflowDefinition)
                .WithMany()
                .HasForeignKey(e => e.WorkflowDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.WorkflowDefinitionId);
            entity.HasIndex(e => e.IsEnabled);
            entity.HasIndex(e => e.NextTriggerAt);
        });

        // WorkflowJob configuration
        modelBuilder.Entity<WorkflowJob>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.JobType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.StepKey).HasMaxLength(200);
            entity.Property(e => e.Payload).HasColumnType("TEXT");
            entity.Property(e => e.ProcessingWorkerId).HasMaxLength(200);
            entity.Property(e => e.ErrorMessage).HasColumnType("TEXT");
            entity.Property(e => e.ResultData).HasColumnType("TEXT");
            entity.Property(e => e.CorrelationId).HasMaxLength(200);
            entity.HasOne(e => e.WorkflowInstance)
                .WithMany()
                .HasForeignKey(e => e.WorkflowInstanceId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.WorkflowTask)
                .WithMany()
                .HasForeignKey(e => e.WorkflowTaskId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ScheduledAt);
            entity.HasIndex(e => e.CorrelationId);
        });

        // WorkflowContextVariable configuration
        modelBuilder.Entity<WorkflowContextVariable>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Value).HasColumnType("TEXT");
            entity.Property(e => e.ValueType).HasMaxLength(50);
            entity.Property(e => e.SetByStepKey).HasMaxLength(200);
            entity.HasOne(e => e.WorkflowInstance)
                .WithMany()
                .HasForeignKey(e => e.WorkflowInstanceId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.WorkflowInstanceId, e.Key }).IsUnique();
        });

        // WorkflowAuditLog configuration
        modelBuilder.Entity<WorkflowAuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ActorId).HasMaxLength(200);
            entity.Property(e => e.ActorName).HasMaxLength(200);
            entity.Property(e => e.Details).HasColumnType("TEXT");
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.HasOne(e => e.WorkflowInstance)
                .WithMany()
                .HasForeignKey(e => e.WorkflowInstanceId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.WorkflowInstanceId);
            entity.HasIndex(e => e.Action);
            entity.HasIndex(e => e.Timestamp);
        });

        // WorkflowMetric configuration
        modelBuilder.Entity<WorkflowMetric>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MetricType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.MetricName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.MetricValue).HasColumnType("decimal(18,4)");
            entity.Property(e => e.Dimensions).HasColumnType("TEXT");
            entity.HasOne(e => e.WorkflowDefinition)
                .WithMany()
                .HasForeignKey(e => e.WorkflowDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.WorkflowDefinitionId);
            entity.HasIndex(e => e.MetricType);
            entity.HasIndex(e => e.RecordedAt);
        });

        // WorkflowLlmUsage configuration
        modelBuilder.Entity<WorkflowLlmUsage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Provider).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Model).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CostEstimate).HasColumnType("decimal(10,6)");
            entity.Property(e => e.ErrorMessage).HasColumnType("TEXT");
            entity.HasOne(e => e.WorkflowInstance)
                .WithMany()
                .HasForeignKey(e => e.WorkflowInstanceId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.NodeInstance)
                .WithMany()
                .HasForeignKey(e => e.NodeInstanceId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => e.WorkflowInstanceId);
            entity.HasIndex(e => e.Provider);
        });

        // WorkflowCircuitBreakerState configuration
        modelBuilder.Entity<WorkflowCircuitBreakerState>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ServiceName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.State).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.ServiceName).IsUnique();
        });

        // ===================================================================
        // RELATIONSHIP MANAGEMENT ENTITIES
        // ===================================================================

        // RelationshipType configuration
        modelBuilder.Entity<RelationshipType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TypeName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TypeCategory).HasMaxLength(50);
            entity.Property(e => e.ReverseTypeName).HasMaxLength(100);
            entity.Property(e => e.Icon).HasMaxLength(50);
            entity.Property(e => e.Color).HasMaxLength(20);
            entity.HasIndex(e => e.TypeName).IsUnique();
            entity.HasIndex(e => e.TypeCategory);
            entity.HasIndex(e => e.IsActive);
        });

        // AccountRelationship configuration
        modelBuilder.Entity<AccountRelationship>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.StrategicImportance).HasMaxLength(50);
            entity.HasIndex(e => e.SourceAccountId);
            entity.HasIndex(e => e.TargetAccountId);
            entity.HasIndex(e => e.RelationshipTypeId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.SourceAccountId, e.TargetAccountId, e.RelationshipTypeId }).IsUnique();

            entity.HasOne(e => e.SourceAccount)
                .WithMany()
                .HasForeignKey(e => e.SourceAccountId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.TargetAccount)
                .WithMany()
                .HasForeignKey(e => e.TargetAccountId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.RelationshipType)
                .WithMany(t => t.Relationships)
                .HasForeignKey(e => e.RelationshipTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // RelationshipInteraction configuration
        modelBuilder.Entity<RelationshipInteraction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.InteractionType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Subject).HasMaxLength(255);
            entity.Property(e => e.Outcome).HasMaxLength(100);
            entity.Property(e => e.HealthImpact).HasMaxLength(50);
            entity.Property(e => e.Location).HasMaxLength(255);
            entity.Property(e => e.MeetingLink).HasMaxLength(500);
            entity.Property(e => e.ParticipantContactIds).HasColumnType("TEXT");
            entity.Property(e => e.ParticipantUserIds).HasColumnType("TEXT");
            entity.Property(e => e.ActionItems).HasColumnType("TEXT");
            entity.Property(e => e.NextSteps).HasColumnType("TEXT");
            entity.Property(e => e.Metadata).HasColumnType("TEXT");
            entity.HasIndex(e => e.AccountRelationshipId);
            entity.HasIndex(e => e.InteractionDate);
            entity.HasIndex(e => e.InteractionType);

            entity.HasOne(e => e.AccountRelationship)
                .WithMany(r => r.Interactions)
                .HasForeignKey(e => e.AccountRelationshipId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // AccountHealthSnapshot configuration
        modelBuilder.Entity<AccountHealthSnapshot>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.HealthTrend).HasMaxLength(20);
            entity.Property(e => e.RiskFactors).HasColumnType("TEXT");
            entity.Property(e => e.WarningSignals).HasColumnType("TEXT");
            entity.Property(e => e.GrowthIndicators).HasColumnType("TEXT");
            entity.Property(e => e.AnalystNotes).HasColumnType("TEXT");
            entity.HasIndex(e => e.AccountId);
            entity.HasIndex(e => e.SnapshotDate);
            entity.HasIndex(e => new { e.AccountId, e.SnapshotDate }).IsUnique();

            entity.HasOne(e => e.Account)
                .WithMany()
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // RelationshipMap configuration
        modelBuilder.Entity<RelationshipMap>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MapName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.IncludeRelationshipTypeIds).HasColumnType("TEXT");
            entity.Property(e => e.ExcludeRelationshipTypeIds).HasColumnType("TEXT");
            entity.Property(e => e.IncludeStatuses).HasColumnType("TEXT");
            entity.Property(e => e.LayoutConfig).HasColumnType("TEXT");
            entity.Property(e => e.ViewSettings).HasColumnType("TEXT");
            entity.Property(e => e.SharedWithUserIds).HasColumnType("TEXT");
            entity.Property(e => e.SharedWithGroupIds).HasColumnType("TEXT");
            entity.HasIndex(e => e.IsPublic);

            entity.HasOne(e => e.CentralAccount)
                .WithMany()
                .HasForeignKey(e => e.CentralAccountId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // AccountTerritory configuration
        modelBuilder.Entity<AccountTerritory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TerritoryName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.TerritoryCode).HasMaxLength(50);
            entity.Property(e => e.Countries).HasColumnType("TEXT");
            entity.Property(e => e.Regions).HasColumnType("TEXT");
            entity.Property(e => e.States).HasColumnType("TEXT");
            entity.Property(e => e.Cities).HasColumnType("TEXT");
            entity.Property(e => e.Industries).HasColumnType("TEXT");
            entity.Property(e => e.CustomerTypes).HasColumnType("TEXT");
            entity.Property(e => e.TeamMemberIds).HasColumnType("TEXT");
            entity.Property(e => e.QuotaCurrency).HasMaxLength(10);
            entity.HasIndex(e => e.TerritoryCode).IsUnique();
            entity.HasIndex(e => e.PrimaryOwnerId);
            entity.HasIndex(e => e.IsActive);

            entity.HasOne(e => e.PrimaryOwner)
                .WithMany()
                .HasForeignKey(e => e.PrimaryOwnerId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // CustomerTerritoryAssignment configuration
        modelBuilder.Entity<AccountTerritoryAssignment>(entity =>
        {
            entity.HasKey(e => new { e.AccountId, e.TerritoryId });
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.HasIndex(e => e.TerritoryId);
            entity.HasIndex(e => e.IsPrimary);

            entity.HasOne(e => e.Account)
                .WithMany()
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Territory)
                .WithMany(t => t.AccountAssignments)
                .HasForeignKey(e => e.TerritoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ===================================================================
        // CAMPAIGN EXECUTION ENTITIES
        // ===================================================================

        // CampaignRecipient configuration
        modelBuilder.Entity<CampaignRecipient>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Company).HasMaxLength(255);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.BounceType).HasMaxLength(50);
            entity.Property(e => e.ABTestVariant).HasMaxLength(10);
            entity.Property(e => e.PersonalizationData).HasColumnType("TEXT");
            entity.Property(e => e.BounceReason).HasColumnType("TEXT");
            entity.Property(e => e.ErrorMessage).HasColumnType("TEXT");
            entity.HasIndex(e => e.CampaignId);
            entity.HasIndex(e => e.ContactId);
            entity.HasIndex(e => e.AccountId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Email);

            entity.HasOne(e => e.Campaign)
                .WithMany()
                .HasForeignKey(e => e.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Contact)
                .WithMany()
                .HasForeignKey(e => e.ContactId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Account)
                .WithMany()
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // CampaignLinkClick configuration
        modelBuilder.Entity<CampaignLinkClick>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LinkUrl).IsRequired().HasColumnType("TEXT");
            entity.Property(e => e.LinkLabel).HasMaxLength(255);
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.Property(e => e.DeviceType).HasMaxLength(50);
            entity.Property(e => e.Browser).HasMaxLength(100);
            entity.Property(e => e.OperatingSystem).HasMaxLength(100);
            entity.Property(e => e.UserAgent).HasColumnType("TEXT");
            entity.Property(e => e.LocationData).HasColumnType("TEXT");
            entity.HasIndex(e => e.CampaignRecipientId);
            entity.HasIndex(e => e.CampaignId);
            entity.HasIndex(e => e.ClickedAt);

            entity.HasOne(e => e.CampaignRecipient)
                .WithMany(r => r.LinkClicks)
                .HasForeignKey(e => e.CampaignRecipientId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Campaign)
                .WithMany()
                .HasForeignKey(e => e.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // CampaignABTest configuration
        modelBuilder.Entity<CampaignABTest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TestName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.TestType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.TestMetric).IsRequired().HasMaxLength(50);
            entity.Property(e => e.WinnerVariant).HasMaxLength(10);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.TrafficSplit).HasColumnType("TEXT");
            entity.Property(e => e.VariantConfigs).HasColumnType("TEXT");
            entity.Property(e => e.WinningCriteria).HasColumnType("TEXT");
            entity.HasIndex(e => e.CampaignId);
            entity.HasIndex(e => e.Status);

            entity.HasOne(e => e.Campaign)
                .WithMany()
                .HasForeignKey(e => e.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // CampaignConversion configuration
        modelBuilder.Entity<CampaignConversion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ConversionType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ConversionCurrency).HasMaxLength(10);
            entity.Property(e => e.AttributionModel).HasMaxLength(50);
            entity.Property(e => e.ExternalOrderId).HasMaxLength(100);
            entity.Property(e => e.ExternalTransactionId).HasMaxLength(100);
            entity.Property(e => e.ConversionData).HasColumnType("TEXT");
            entity.HasIndex(e => e.CampaignId);
            entity.HasIndex(e => e.CampaignRecipientId);
            entity.HasIndex(e => e.ConversionType);
            entity.HasIndex(e => e.ConvertedAt);

            entity.HasOne(e => e.Campaign)
                .WithMany()
                .HasForeignKey(e => e.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.CampaignRecipient)
                .WithMany()
                .HasForeignKey(e => e.CampaignRecipientId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Contact)
                .WithMany()
                .HasForeignKey(e => e.ContactId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Account)
                .WithMany()
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // CampaignWorkflow configuration
        modelBuilder.Entity<CampaignWorkflow>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.WorkflowType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.TriggerEvent).HasMaxLength(100);
            entity.Property(e => e.TriggerConditions).HasColumnType("TEXT");
            entity.HasIndex(e => e.CampaignId);
            entity.HasIndex(e => e.WorkflowDefinitionId);
            entity.HasIndex(e => e.IsActive);

            entity.HasOne(e => e.Campaign)
                .WithMany()
                .HasForeignKey(e => e.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.WorkflowDefinition)
                .WithMany()
                .HasForeignKey(e => e.WorkflowDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // =============================================================================
        // AI Entity Configurations
        // =============================================================================

        // AIModel configuration
        modelBuilder.Entity<AIModel>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Version).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.ModelIdentifier).HasMaxLength(500);
            entity.Property(e => e.ConfigurationJson).HasColumnType(longTextType);
            entity.Property(e => e.FeatureColumnsJson).HasColumnType(longTextType);
            entity.Property(e => e.TargetColumn).HasMaxLength(200);
            entity.Property(e => e.HyperparametersJson).HasColumnType(longTextType);
            entity.Property(e => e.TrainingAccuracy).HasPrecision(10, 6);
            entity.Property(e => e.ValidationAccuracy).HasPrecision(10, 6);
            entity.Property(e => e.TestAccuracy).HasPrecision(10, 6);
            entity.Property(e => e.AucRoc).HasPrecision(10, 6);
            entity.Property(e => e.F1Score).HasPrecision(10, 6);
            entity.Property(e => e.MeanAbsoluteError).HasPrecision(18, 6);
            entity.Property(e => e.AvgInferenceTimeMs).HasPrecision(10, 3);
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.ModelType);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Provider);
        });

        // Prediction configuration
        modelBuilder.Entity<Prediction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PredictionId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PredictedValue).HasPrecision(18, 6);
            entity.Property(e => e.PredictedLabel).HasMaxLength(500);
            entity.Property(e => e.Confidence).HasPrecision(10, 6);
            entity.Property(e => e.ProbabilitiesJson).HasColumnType(longTextType);
            entity.Property(e => e.FeatureImportanceJson).HasColumnType(longTextType);
            entity.Property(e => e.Explanation).HasColumnType(textType);
            entity.Property(e => e.InferenceTimeMs).HasPrecision(10, 3);
            entity.Property(e => e.ActualValue).HasPrecision(18, 6);
            entity.Property(e => e.ActualLabel).HasMaxLength(500);
            entity.HasIndex(e => e.PredictionId).IsUnique();
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
            entity.HasIndex(e => e.AIModelId);
            entity.HasIndex(e => e.PredictedAt);

            entity.HasOne(e => e.AIModel)
                .WithMany(m => m.Predictions)
                .HasForeignKey(e => e.AIModelId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // LeadScore configuration
        modelBuilder.Entity<LeadScore>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OverallScore).HasPrecision(10, 2);
            entity.Property(e => e.Confidence).HasPrecision(10, 4);
            entity.Property(e => e.ScoreTrend).HasPrecision(10, 4);
            entity.Property(e => e.DemographicScore).HasPrecision(10, 2);
            entity.Property(e => e.FirmographicScore).HasPrecision(10, 2);
            entity.Property(e => e.BehavioralScore).HasPrecision(10, 2);
            entity.Property(e => e.EngagementScore).HasPrecision(10, 2);
            entity.Property(e => e.IntentScore).HasPrecision(10, 2);
            entity.Property(e => e.EmailOpenRate).HasPrecision(10, 4);
            entity.Property(e => e.EmailClickRate).HasPrecision(10, 4);
            entity.Property(e => e.ConversionProbability).HasPrecision(10, 4);
            entity.Property(e => e.EstimatedDealValue).HasPrecision(18, 2);
            entity.Property(e => e.BestProductFit).HasMaxLength(200);
            entity.Property(e => e.TopFactorsJson).HasColumnType(longTextType);
            entity.Property(e => e.RiskFactorsJson).HasColumnType(longTextType);
            entity.Property(e => e.AIInsights).HasColumnType(textType);
            entity.Property(e => e.ICPMatchScore).HasPrecision(10, 2);
            entity.Property(e => e.MatchingSegment).HasMaxLength(200);
            entity.Property(e => e.ModelVersion).HasMaxLength(50);
            entity.Property(e => e.PreviousScore).HasPrecision(10, 2);
            entity.HasIndex(e => e.LeadId);
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.OverallScore);
            entity.HasIndex(e => e.ScoredAt);

            entity.HasOne(e => e.Lead)
                .WithMany()
                .HasForeignKey(e => e.LeadId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.AIModel)
                .WithMany()
                .HasForeignKey(e => e.AIModelId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // OpportunityInsight configuration
        modelBuilder.Entity<OpportunityInsight>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.WinProbability).HasPrecision(10, 4);
            entity.Property(e => e.Confidence).HasPrecision(10, 4);
            entity.Property(e => e.ProbabilityTrend).HasPrecision(10, 4);
            entity.Property(e => e.PreviousProbability).HasPrecision(10, 4);
            entity.Property(e => e.HealthScore).HasPrecision(10, 2);
            entity.Property(e => e.VelocityScore).HasPrecision(10, 2);
            entity.Property(e => e.EngagementScore).HasPrecision(10, 2);
            entity.Property(e => e.StakeholderScore).HasPrecision(10, 2);
            entity.Property(e => e.PredictedValue).HasPrecision(18, 2);
            entity.Property(e => e.WeightedValue).HasPrecision(18, 2);
            entity.Property(e => e.UpsidePotential).HasPrecision(18, 2);
            entity.Property(e => e.RiskAdjustedValue).HasPrecision(18, 2);
            entity.Property(e => e.RiskLevel).HasPrecision(10, 2);
            entity.Property(e => e.RisksJson).HasColumnType(longTextType);
            entity.Property(e => e.RiskMitigationSuggestions).HasColumnType(textType);
            entity.Property(e => e.CompetitorsJson).HasColumnType(longTextType);
            entity.Property(e => e.PrimaryCompetitor).HasMaxLength(200);
            entity.Property(e => e.CompetitivePositionScore).HasPrecision(10, 2);
            entity.Property(e => e.DifferentiationSuggestions).HasColumnType(textType);
            entity.Property(e => e.ActionRecommendationsJson).HasColumnType(longTextType);
            entity.Property(e => e.TalkingPoints).HasColumnType(textType);
            entity.Property(e => e.ObjectionHandling).HasColumnType(textType);
            entity.Property(e => e.AIInsights).HasColumnType(textType);
            entity.Property(e => e.SimilarDealsWinRate).HasPrecision(10, 4);
            entity.Property(e => e.SuccessPatternsJson).HasColumnType(longTextType);
            entity.Property(e => e.ModelVersion).HasMaxLength(50);
            entity.HasIndex(e => e.OpportunityId);
            entity.HasIndex(e => e.WinCategory);
            entity.HasIndex(e => e.HealthStatus);
            entity.HasIndex(e => e.GeneratedAt);

            entity.HasOne(e => e.Opportunity)
                .WithMany()
                .HasForeignKey(e => e.OpportunityId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.AIModel)
                .WithMany()
                .HasForeignKey(e => e.AIModelId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ChurnRisk configuration
        modelBuilder.Entity<ChurnRisk>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ChurnProbability).HasPrecision(10, 4);
            entity.Property(e => e.Confidence).HasPrecision(10, 4);
            entity.Property(e => e.RiskTrend).HasPrecision(10, 4);
            entity.Property(e => e.PreviousProbability).HasPrecision(10, 4);
            entity.Property(e => e.HealthScore).HasPrecision(10, 2);
            entity.Property(e => e.CSATScore).HasPrecision(5, 2);
            entity.Property(e => e.CESScore).HasPrecision(5, 2);
            entity.Property(e => e.UsageScore).HasPrecision(10, 2);
            entity.Property(e => e.FeatureAdoption).HasPrecision(10, 2);
            entity.Property(e => e.MonthlyLogins).HasPrecision(10, 2);
            entity.Property(e => e.UsageTrend).HasPrecision(10, 4);
            entity.Property(e => e.AvgResolutionTimeHours).HasPrecision(10, 2);
            entity.Property(e => e.SupportSatisfaction).HasPrecision(5, 2);
            entity.Property(e => e.ARRAtRisk).HasPrecision(18, 2);
            entity.Property(e => e.LifetimeValue).HasPrecision(18, 2);
            entity.Property(e => e.ExpansionPotential).HasPrecision(18, 2);
            entity.Property(e => e.ChurnDriversJson).HasColumnType(longTextType);
            entity.Property(e => e.RiskIndicatorsJson).HasColumnType(longTextType);
            entity.Property(e => e.NegativeSentimentJson).HasColumnType(longTextType);
            entity.Property(e => e.ActionRecommendationsJson).HasColumnType(longTextType);
            entity.Property(e => e.RetentionPlaybook).HasMaxLength(200);
            entity.Property(e => e.SaveProbability).HasPrecision(10, 4);
            entity.Property(e => e.AIInsights).HasColumnType(textType);
            entity.Property(e => e.ChurnPatternsJson).HasColumnType(longTextType);
            entity.Property(e => e.SavePatternsJson).HasColumnType(longTextType);
            entity.Property(e => e.ModelVersion).HasMaxLength(50);
            entity.HasIndex(e => e.AccountId);
            entity.HasIndex(e => e.RiskLevel);
            entity.HasIndex(e => e.HealthSegment);
            entity.HasIndex(e => e.AssessedAt);

            entity.HasOne(e => e.Account)
                .WithMany()
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.AIModel)
                .WithMany()
                .HasForeignKey(e => e.AIModelId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ActionRecommendation configuration
        modelBuilder.Entity<ActionRecommendation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TargetEntityName).HasMaxLength(500);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Description).HasColumnType(textType);
            entity.Property(e => e.Rationale).HasColumnType(textType);
            entity.Property(e => e.SuggestedContent).HasColumnType(longTextType);
            entity.Property(e => e.TalkingPointsJson).HasColumnType(longTextType);
            entity.Property(e => e.ImpactScore).HasPrecision(10, 2);
            entity.Property(e => e.ExpectedOutcome).HasColumnType(textType);
            entity.Property(e => e.SuccessProbability).HasPrecision(10, 4);
            entity.Property(e => e.EstimatedValueImpact).HasPrecision(18, 2);
            entity.Property(e => e.RiskIfNotTaken).HasColumnType(textType);
            entity.Property(e => e.Confidence).HasPrecision(10, 4);
            entity.Property(e => e.RelevanceScore).HasPrecision(10, 2);
            entity.Property(e => e.DrivingFactorsJson).HasColumnType(longTextType);
            entity.Property(e => e.StatusReason).HasMaxLength(500);
            entity.Property(e => e.UserFeedback).HasColumnType(textType);
            entity.Property(e => e.ActualOutcome).HasColumnType(textType);
            entity.Property(e => e.AlternativeActionsJson).HasColumnType(longTextType);
            entity.Property(e => e.FollowUpActionIdsJson).HasColumnType(longTextType);
            entity.Property(e => e.ModelVersion).HasMaxLength(50);
            entity.HasIndex(e => new { e.TargetType, e.TargetEntityId });
            entity.HasIndex(e => e.AssignedUserId);
            entity.HasIndex(e => e.ActionType);
            entity.HasIndex(e => e.Priority);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.GeneratedAt);

            entity.HasOne(e => e.AssignedUser)
                .WithMany()
                .HasForeignKey(e => e.AssignedUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.AIModel)
                .WithMany()
                .HasForeignKey(e => e.AIModelId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // EmailIntelligence configuration
        modelBuilder.Entity<EmailIntelligence>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EmailMessageId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.SentimentScore).HasPrecision(10, 4);
            entity.Property(e => e.SentimentConfidence).HasPrecision(10, 4);
            entity.Property(e => e.EmotionsJson).HasColumnType(longTextType);
            entity.Property(e => e.IntentConfidence).HasPrecision(10, 4);
            entity.Property(e => e.SecondaryIntentsJson).HasColumnType(longTextType);
            entity.Property(e => e.UrgencyScore).HasPrecision(10, 2);
            entity.Property(e => e.ExtractedEntitiesJson).HasColumnType(longTextType);
            entity.Property(e => e.MentionedProductsJson).HasColumnType(longTextType);
            entity.Property(e => e.MentionedCompetitorsJson).HasColumnType(longTextType);
            entity.Property(e => e.TopicsJson).HasColumnType(longTextType);
            entity.Property(e => e.ActionItemsJson).HasColumnType(longTextType);
            entity.Property(e => e.QuestionsJson).HasColumnType(longTextType);
            entity.Property(e => e.Summary).HasColumnType(textType);
            entity.Property(e => e.KeyPointsJson).HasColumnType(longTextType);
            entity.Property(e => e.SuggestedResponse).HasColumnType(longTextType);
            entity.Property(e => e.ResponseTalkingPointsJson).HasColumnType(longTextType);
            entity.Property(e => e.RecommendedTone).HasMaxLength(100);
            entity.Property(e => e.ThreadId).HasMaxLength(255);
            entity.Property(e => e.ThreadSentimentTrend).HasMaxLength(100);
            entity.Property(e => e.UnresolvedItemsJson).HasColumnType(longTextType);
            entity.Property(e => e.OpportunityImpact).HasMaxLength(50);
            entity.Property(e => e.ProcessingTimeMs).HasPrecision(10, 3);
            entity.Property(e => e.ModelVersion).HasMaxLength(50);
            entity.HasIndex(e => e.EmailMessageId);
            entity.HasIndex(e => e.CommunicationMessageId);
            entity.HasIndex(e => e.Sentiment);
            entity.HasIndex(e => e.PrimaryIntent);
            entity.HasIndex(e => e.Urgency);
            entity.HasIndex(e => e.AnalyzedAt);

            entity.HasOne(e => e.CommunicationMessage)
                .WithMany()
                .HasForeignKey(e => e.CommunicationMessageId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.AIModel)
                .WithMany()
                .HasForeignKey(e => e.AIModelId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // =============================================================================
        // Report Entity Configurations
        // =============================================================================

        // ReportDefinition configuration
        modelBuilder.Entity<ReportDefinition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.ReportCode).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.TagsJson).HasColumnType(longTextType);
            entity.Property(e => e.ColumnsJson).HasColumnType(longTextType);
            entity.Property(e => e.FiltersJson).HasColumnType(longTextType);
            entity.Property(e => e.GroupByJson).HasColumnType(longTextType);
            entity.Property(e => e.SortByJson).HasColumnType(longTextType);
            entity.Property(e => e.AggregationsJson).HasColumnType(longTextType);
            entity.Property(e => e.CustomQuery).HasColumnType(longTextType);
            entity.Property(e => e.DateField).HasMaxLength(200);
            entity.Property(e => e.ChartConfigJson).HasColumnType(longTextType);
            entity.Property(e => e.ConditionalFormattingJson).HasColumnType(longTextType);
            entity.HasIndex(e => e.ReportCode).IsUnique();
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.DataSource);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedByUserId);

            entity.HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Folder)
                .WithMany(f => f.Reports)
                .HasForeignKey(e => e.FolderId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ReportFolder configuration
        modelBuilder.Entity<ReportFolder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.OwnerUserId);

            entity.HasOne(e => e.ParentFolder)
                .WithMany(f => f.ChildFolders)
                .HasForeignKey(e => e.ParentFolderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Owner)
                .WithMany()
                .HasForeignKey(e => e.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ReportSchedule configuration
        modelBuilder.Entity<ReportSchedule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.CronExpression).HasMaxLength(100);
            entity.Property(e => e.Timezone).HasMaxLength(100);
            entity.Property(e => e.FileNamePattern).HasMaxLength(255);
            entity.Property(e => e.EmailRecipientsJson).HasColumnType(longTextType);
            entity.Property(e => e.EmailCcJson).HasColumnType(longTextType);
            entity.Property(e => e.EmailSubject).HasMaxLength(500);
            entity.Property(e => e.EmailBody).HasColumnType(textType);
            entity.Property(e => e.StoragePath).HasMaxLength(500);
            entity.Property(e => e.WebhookUrl).HasMaxLength(1000);
            entity.Property(e => e.LastDataHash).HasMaxLength(64);
            entity.Property(e => e.LastError).HasColumnType(textType);
            entity.Property(e => e.AvgExecutionTimeSeconds).HasPrecision(10, 2);
            entity.HasIndex(e => e.ReportDefinitionId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.NextRunAt);
            entity.HasIndex(e => e.CreatedByUserId);

            entity.HasOne(e => e.ReportDefinition)
                .WithMany(r => r.Schedules)
                .HasForeignKey(e => e.ReportDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ReportExecution configuration
        modelBuilder.Entity<ReportExecution>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ParametersJson).HasColumnType(longTextType);
            entity.Property(e => e.ExecutionTimeSeconds).HasPrecision(10, 2);
            entity.Property(e => e.OutputFilePath).HasMaxLength(500);
            entity.Property(e => e.DataHash).HasMaxLength(64);
            entity.Property(e => e.DeliveredToJson).HasColumnType(longTextType);
            entity.Property(e => e.ErrorMessage).HasColumnType(textType);
            entity.Property(e => e.ErrorStackTrace).HasColumnType(longTextType);
            entity.HasIndex(e => e.ReportScheduleId);
            entity.HasIndex(e => e.ReportDefinitionId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.StartedAt);

            entity.HasOne(e => e.ReportSchedule)
                .WithMany(s => s.Executions)
                .HasForeignKey(e => e.ReportScheduleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ReportDefinition)
                .WithMany()
                .HasForeignKey(e => e.ReportDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.TriggeredByUser)
                .WithMany()
                .HasForeignKey(e => e.TriggeredByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ReportWidgetConfig configuration
        modelBuilder.Entity<ReportWidgetConfig>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FiltersOverrideJson).HasColumnType(longTextType);
            entity.HasIndex(e => e.DashboardWidgetId);
            entity.HasIndex(e => e.ReportDefinitionId);

            entity.HasOne(e => e.DashboardWidget)
                .WithMany()
                .HasForeignKey(e => e.DashboardWidgetId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ReportDefinition)
                .WithMany(r => r.WidgetConfigs)
                .HasForeignKey(e => e.ReportDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // =============================================================================
        // Knowledge Base Entity Configurations
        // =============================================================================

        // KnowledgeArticle configuration
        modelBuilder.Entity<KnowledgeArticle>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ArticleNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Summary).HasMaxLength(1000);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Content).HasColumnType(longTextType);
            entity.Property(e => e.ContentFormat).HasMaxLength(20);
            entity.Property(e => e.PlainTextContent).HasColumnType(longTextType);
            entity.Property(e => e.TableOfContentsJson).HasColumnType(longTextType);
            entity.Property(e => e.AttachmentsJson).HasColumnType(longTextType);
            entity.Property(e => e.VideoUrl).HasMaxLength(1000);
            entity.Property(e => e.ProductsJson).HasColumnType(longTextType);
            entity.Property(e => e.TagsJson).HasColumnType(longTextType);
            entity.Property(e => e.Keywords).HasMaxLength(1000);
            entity.Property(e => e.MetaTitle).HasMaxLength(200);
            entity.Property(e => e.MetaDescription).HasMaxLength(500);
            entity.Property(e => e.CanonicalUrl).HasMaxLength(500);
            entity.Property(e => e.LanguageCode).HasMaxLength(10);
            entity.Property(e => e.EmbeddingVectorJson).HasColumnType(longTextType);
            entity.Property(e => e.AISummary).HasColumnType(textType);
            entity.Property(e => e.RelatedArticleIdsJson).HasColumnType(longTextType);
            entity.Property(e => e.AISuggestionsJson).HasColumnType(longTextType);
            entity.Property(e => e.AverageRating).HasPrecision(3, 2);
            entity.Property(e => e.AvgTimeOnPageSeconds).HasPrecision(10, 2);
            entity.HasIndex(e => e.ArticleNumber).IsUnique();
            entity.HasIndex(e => e.Slug);
            entity.HasIndex(e => e.Title);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Visibility);
            entity.HasIndex(e => e.ArticleType);
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.AuthorUserId);
            entity.HasIndex(e => e.PublishedAt);

            entity.HasOne(e => e.Category)
                .WithMany(c => c.Articles)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Author)
                .WithMany()
                .HasForeignKey(e => e.AuthorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.LastUpdatedByUser)
                .WithMany()
                .HasForeignKey(e => e.LastUpdatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.ApprovedByUser)
                .WithMany()
                .HasForeignKey(e => e.ApprovedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.ParentArticle)
                .WithMany(a => a.Translations)
                .HasForeignKey(e => e.ParentArticleId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // KnowledgeCategory configuration
        modelBuilder.Entity<KnowledgeCategory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Icon).HasMaxLength(100);
            entity.HasIndex(e => e.Slug);
            entity.HasIndex(e => e.Name);

            entity.HasOne(e => e.ParentCategory)
                .WithMany(c => c.ChildCategories)
                .HasForeignKey(e => e.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ServiceRequestArticle configuration
        modelBuilder.Entity<ServiceRequestArticle>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ServiceRequestId);
            entity.HasIndex(e => e.KnowledgeArticleId);

            entity.HasOne(e => e.ServiceRequest)
                .WithMany()
                .HasForeignKey(e => e.ServiceRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.KnowledgeArticle)
                .WithMany(a => a.ServiceRequests)
                .HasForeignKey(e => e.KnowledgeArticleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.LinkedByUser)
                .WithMany()
                .HasForeignKey(e => e.LinkedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ArticleFeedback configuration
        modelBuilder.Entity<ArticleFeedback>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Comment).HasColumnType(textType);
            entity.Property(e => e.SessionId).HasMaxLength(100);
            entity.HasIndex(e => e.KnowledgeArticleId);
            entity.HasIndex(e => e.SubmittedAt);

            entity.HasOne(e => e.KnowledgeArticle)
                .WithMany(a => a.Feedback)
                .HasForeignKey(e => e.KnowledgeArticleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Account)
                .WithMany()
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ArticleRelationship configuration (ITSM self-referencing many-to-many for related articles)
        modelBuilder.Entity<ITSM.ArticleRelationship>(entity =>
        {
            entity.HasKey(e => e.RelationshipId);
            entity.HasIndex(e => e.ArticleId);
            entity.HasIndex(e => e.RelatedArticleId);
            entity.HasIndex(e => new { e.ArticleId, e.RelatedArticleId }).IsUnique();

            entity.HasOne(e => e.Article)
                .WithMany()
                .HasForeignKey(e => e.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.RelatedArticle)
                .WithMany()
                .HasForeignKey(e => e.RelatedArticleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // CIRelationship configuration (CMDB self-referencing many-to-many for CI dependencies)
        modelBuilder.Entity<ITSM.CIRelationship>(entity =>
        {
            entity.HasKey(e => e.RelationshipId);
            entity.HasIndex(e => e.ParentCIId);
            entity.HasIndex(e => e.ChildCIId);
            entity.HasIndex(e => new { e.ParentCIId, e.ChildCIId, e.RelationshipType }).IsUnique();

            entity.HasOne(e => e.ParentCI)
                .WithMany(ci => ci.ChildRelationships)
                .HasForeignKey(e => e.ParentCIId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ChildCI)
                .WithMany(ci => ci.ParentRelationships)
                .HasForeignKey(e => e.ChildCIId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ITSM entities use their DbSet property names as table names by convention:
        //   ITSMSLAPolicies, ITSMSLAInstances, ITSMKnowledgeArticles, ITSMArticleFeedback, BusinessHoursSchedules
        // These are SEPARATE from the non-ITSM entities (SLAPolicies, SLAInstances, KnowledgeArticles, etc.)
        // which exist in the KnowledgeBase namespace and map to their own tables.
        // Do NOT add ToTable() mappings that would cause shared-table conflicts between the two namespaces.

        // SLAPolicy configuration
        modelBuilder.Entity<CRM.Core.Entities.SLAPolicy>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.CustomerSegmentsJson).HasColumnType(longTextType);
            entity.Property(e => e.ProductsJson).HasColumnType(longTextType);
            entity.Property(e => e.CaseTypesJson).HasColumnType(longTextType);
            entity.Property(e => e.CustomerTiersJson).HasColumnType(longTextType);
            entity.Property(e => e.MatchConditionsJson).HasColumnType(longTextType);
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.Priority);

            // NOTE: BusinessHoursId is a foreign key but BusinessHours property is a JSON string field
            // If a navigation property to BusinessHours entity is needed, add it to the entity and uncomment the HasOne below
            // entity.HasOne(e => e.BusinessHoursNavigation)
            //     .WithMany()
            //     .HasForeignKey(e => e.BusinessHoursId)
            //     .OnDelete(DeleteBehavior.SetNull);
        });

        // SLATarget configuration
        modelBuilder.Entity<SLATarget>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.SLAPolicyId);
            entity.HasIndex(e => e.MetricType);

            entity.HasOne(e => e.SLAPolicy)
                .WithMany(p => p.Targets)
                .HasForeignKey(e => e.SLAPolicyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // BusinessHours configuration
        modelBuilder.Entity<BusinessHours>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Timezone).HasMaxLength(100);
            entity.Property(e => e.ScheduleJson).HasColumnType(longTextType);
            entity.Property(e => e.HolidaysJson).HasColumnType(longTextType);
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.IsActive);
        });

        // EscalationRule configuration
        // DISABLED: Conflicts with ITSM.EscalationRule namespace - needs table disambiguation
        // TODO: Configure separate tables for CRM.Core.Entities.EscalationRule vs ITSM.EscalationRule
        /*
        modelBuilder.Entity<CRM.Core.Entities.EscalationRule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.EmailRecipientsJson).HasColumnType(longTextType);
            entity.Property(e => e.WebhookUrl).HasMaxLength(1000);
            entity.Property(e => e.ActionConfigJson).HasColumnType(longTextType);
            entity.HasIndex(e => e.SLAPolicyId);
            entity.HasIndex(e => e.TriggerMetric);
            entity.HasIndex(e => e.IsActive);

            entity.HasOne(e => e.SLAPolicy)
                .WithMany(p => p.EscalationRules)
                .HasForeignKey(e => e.SLAPolicyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ReassignToUser)
                .WithMany()
                .HasForeignKey(e => e.ReassignToUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        */

        // SLAInstance configuration
        modelBuilder.Entity<SLAInstance>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PauseReason).HasMaxLength(500);
            entity.Property(e => e.EscalationsTriggeredJson).HasColumnType(longTextType);
            entity.HasIndex(e => e.ServiceRequestId);
            entity.HasIndex(e => e.SLAPolicyId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.DueAt);

            entity.HasOne(e => e.ServiceRequest)
                .WithMany()
                .HasForeignKey(e => e.ServiceRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.SLAPolicy)
                .WithMany()
                .HasForeignKey(e => e.SLAPolicyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.SLATarget)
                .WithMany()
                .HasForeignKey(e => e.SLATargetId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // =======================================================================
        // ALIAS PROPERTY IGNORES
        // EF Core maps all public properties to DB columns by default.
        // Alias properties redirect to other properties and do NOT have DB columns.
        // [NotMapped] alone is insufficient - entity.Ignore() is required.
        // =======================================================================

        // Product: UnitPrice -> Price
        modelBuilder.Entity<Product>(entity =>
        {
            entity.Ignore(e => e.UnitPrice);
        });

        // Contract: 10 alias properties that don't map to DB columns
        modelBuilder.Entity<Contract>(entity =>
        {
            entity.Ignore(e => e.TotalValue);          // -> Value
            entity.Ignore(e => e.ActivatedAt);          // -> ActivatedDate
            entity.Ignore(e => e.TerminatedAt);         // -> TerminatedDate
            entity.Ignore(e => e.SentForSignatureAt);   // -> SignedDate
            entity.Ignore(e => e.TerminationReason);    // no DB column
            entity.Ignore(e => e.PaymentTerms);         // no DB column
            entity.Ignore(e => e.IsSigned);             // computed from SignedDate
            entity.Ignore(e => e.SignedBy);             // no DB column
            entity.Ignore(e => e.DocumentUrl);          // -> ContractFileUrl
            entity.Ignore(e => e.TermsAndConditions);   // -> Terms
        });

        // Commission: 4 alias properties
        modelBuilder.Entity<Commission>(entity =>
        {
            entity.Ignore(e => e.Amount);               // -> CommissionAmount
            entity.Ignore(e => e.Plan);                 // -> CommissionPlan (nav)
            entity.Ignore(e => e.ApprovedAt);           // -> ApprovedDate
            entity.Ignore(e => e.PaidAt);               // -> PaidDate
            entity.Ignore(e => e.SalesRepUserId);       // -> UserId (alias)
        });

        // Payment: TransactionId -> GatewayTransactionId
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.Ignore(e => e.TransactionId);
        });

        // Opportunity: EstimatedValue -> Amount (also has [NotMapped])
        modelBuilder.Entity<Opportunity>(entity =>
        {
            entity.Ignore(e => e.EstimatedValue);
        });

        // Quote: 3 alias properties (also have [NotMapped])
        modelBuilder.Entity<Quote>(entity =>
        {
            entity.Ignore(e => e.DiscountAmount);       // -> Discount
            entity.Ignore(e => e.TaxAmount);            // -> Tax
            entity.Ignore(e => e.TotalAmount);          // -> Total
        });

        // AI Agent entities: Configure one-to-one relationship between AgentAction and AgentApprovalRequest
        modelBuilder.Entity<AgentAction>(entity =>
        {
            entity.HasOne(e => e.ApprovalRequest)
                .WithOne(a => a.AgentAction)
                .HasForeignKey<AgentApprovalRequest>(a => a.AgentActionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // =============================================================================
        // Email Sequence Configurations (Complete Entity Configuration)
        // =============================================================================
        // Apply IEntityTypeConfiguration implementations for Email Sequence entities
        modelBuilder.ApplyConfiguration(new CRM.Infrastructure.Data.Configurations.Marketing.EmailSequenceConfiguration());
        modelBuilder.ApplyConfiguration(new CRM.Infrastructure.Data.Configurations.Marketing.EmailSequenceStepConfiguration());
        modelBuilder.ApplyConfiguration(new CRM.Infrastructure.Data.Configurations.Marketing.EmailSequenceEnrollmentConfiguration());
        modelBuilder.ApplyConfiguration(new CRM.Infrastructure.Data.Configurations.Marketing.EmailSequenceStepExecutionConfiguration());

        // =============================================================================
        // Configuration Management (System & CRM Config)
        // =============================================================================
        modelBuilder.ApplyConfiguration(new CRM.Infrastructure.Data.Configurations.ProviderConfigurationConfiguration());
        modelBuilder.ApplyConfiguration(new CRM.Infrastructure.Data.Configurations.ConfigurationChangeLogConfiguration());

        // =============================================================================
        // Web Tracking Entity Configurations (Analytics & Performance)
        // =============================================================================

        // WebVisitor configuration - Track anonymous web visitors
        modelBuilder.Entity<WebVisitor>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.VisitorId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.Property(e => e.UserAgent).HasMaxLength(2000);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.Region).HasMaxLength(100);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.Browser).HasMaxLength(100);
            entity.Property(e => e.BrowserVersion).HasMaxLength(50);
            entity.Property(e => e.DeviceType).HasMaxLength(50);
            entity.Property(e => e.OperatingSystem).HasMaxLength(100);

            // Relationships
            entity.HasOne(e => e.Contact)
                .WithMany()
                .HasForeignKey(e => e.ContactId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Lead)
                .WithMany()
                .HasForeignKey(e => e.LeadId)
                .OnDelete(DeleteBehavior.SetNull);

            // WebVisitor -> WebSession (one-to-many)
            entity.HasMany(e => e.Sessions)
                .WithOne(s => s.WebVisitor)
                .HasForeignKey(s => s.WebVisitorId)
                .OnDelete(DeleteBehavior.Cascade);

            // WebVisitor -> WebPageView (one-to-many)
            entity.HasMany(e => e.PageViews)
                .WithOne(p => p.WebVisitor)
                .HasForeignKey(p => p.WebVisitorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for performance
            entity.HasIndex(e => e.VisitorId).HasDatabaseName("IX_WebVisitors_VisitorId");
            entity.HasIndex(e => e.ContactId).HasDatabaseName("IX_WebVisitors_ContactId");
            entity.HasIndex(e => e.LeadId).HasDatabaseName("IX_WebVisitors_LeadId");
            entity.HasIndex(e => e.CreatedAt).HasDatabaseName("IX_WebVisitors_CreatedAt");
        });

        // WebSession configuration - Track individual sessions
        modelBuilder.Entity<WebSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SessionId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.WebVisitorId).IsRequired();
            entity.Property(e => e.Referrer).HasMaxLength(2000);
            entity.Property(e => e.LandingPage).HasMaxLength(2000);
            entity.Property(e => e.ExitPage).HasMaxLength(2000);
            entity.Property(e => e.IpAddress).HasMaxLength(100);
            entity.Property(e => e.UtmParameters).HasColumnType("TEXT");

            // Relationship: WebSession -> WebVisitor (many-to-one)
            entity.HasOne(e => e.WebVisitor)
                .WithMany(v => v.Sessions)
                .HasForeignKey(e => e.WebVisitorId)
                .OnDelete(DeleteBehavior.Cascade);

            // WebSession -> WebPageView (one-to-many)
            entity.HasMany(e => e.PageViews)
                .WithOne(p => p.WebSession)
                .HasForeignKey(p => p.WebSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for performance
            entity.HasIndex(e => e.SessionId).HasDatabaseName("IX_WebSessions_SessionId");
            entity.HasIndex(e => e.WebVisitorId).HasDatabaseName("IX_WebSessions_WebVisitorId");
            entity.HasIndex(e => e.StartedAt).HasDatabaseName("IX_WebSessions_StartedAt");
            entity.HasIndex(e => new { e.WebVisitorId, e.StartedAt }).HasDatabaseName("IX_WebSessions_WebVisitorId_StartedAt");
        });

        // WebPageView configuration - Track individual page views
        modelBuilder.Entity<WebPageView>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PageUrl).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.PagePath).HasMaxLength(2000);
            entity.Property(e => e.PageTitle).HasMaxLength(500);
            entity.Property(e => e.WebVisitorId).IsRequired();
            entity.Property(e => e.WebSessionId);
            entity.Property(e => e.Referrer).HasMaxLength(2000);
            entity.Property(e => e.QueryParameters).HasColumnType("TEXT");

            // Relationships
            entity.HasOne(e => e.WebVisitor)
                .WithMany(v => v.PageViews)
                .HasForeignKey(e => e.WebVisitorId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.WebSession)
                .WithMany(s => s.PageViews)
                .HasForeignKey(e => e.WebSessionId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes for performance
            entity.HasIndex(e => e.WebVisitorId).HasDatabaseName("IX_WebPageViews_WebVisitorId");
            entity.HasIndex(e => e.WebSessionId).HasDatabaseName("IX_WebPageViews_WebSessionId");
            entity.HasIndex(e => e.CreatedAt).HasDatabaseName("IX_WebPageViews_CreatedAt");
            entity.HasIndex(e => new { e.WebVisitorId, e.CreatedAt }).HasDatabaseName("IX_WebPageViews_WebVisitorId_CreatedAt");
        });

        // =============================================================================
        // ITSM Relationship Configurations (Complete Missing Relationships)
        // =============================================================================

        // Problem ↔ Incident many-to-many relationship
        modelBuilder.Entity<ITSM.ProblemIncident>(entity =>
        {
            entity.HasKey(e => e.ProblemIncidentId);
            entity.HasIndex(e => new { e.ProblemId, e.IncidentId }).IsUnique().HasDatabaseName("IX_ProblemIncidents_ProblemId_IncidentId");

            // ProblemIncident -> Problem (many-to-one)
            entity.HasOne(e => e.Problem)
                .WithMany(p => p.ProblemIncidents)
                .HasForeignKey(e => e.ProblemId)
                .OnDelete(DeleteBehavior.Cascade);

            // ProblemIncident -> Incident (many-to-one)
            entity.HasOne(e => e.Incident)
                .WithMany(i => i.ProblemIncidents)
                .HasForeignKey(e => e.IncidentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Change Management relationships
        modelBuilder.Entity<ITSM.Change>(entity =>
        {
            // Change -> ChangeApproval (one-to-many)
            entity.HasMany(e => e.Approvals)
                .WithOne(a => a.Change)
                .HasForeignKey(a => a.ChangeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Change -> ChangeBlackout (one-to-many)
            entity.HasMany(e => e.Blackouts)
                .WithOne(b => b.Change)
                .HasForeignKey(b => b.ChangeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Change -> ChangeImpactedCI (one-to-many)
            entity.HasMany(e => e.ImpactedCIs)
                .WithOne(ic => ic.Change)
                .HasForeignKey(ic => ic.ChangeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Change -> ChangeTask (one-to-many)
            entity.HasMany(e => e.Tasks)
                .WithOne(t => t.Change)
                .HasForeignKey(t => t.ChangeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Change -> ChangeComment (one-to-many)
            entity.HasMany(e => e.Comments)
                .WithOne(c => c.Change)
                .HasForeignKey(c => c.ChangeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Change -> ChangeAttachment (one-to-many)
            entity.HasMany(e => e.Attachments)
                .WithOne(a => a.Change)
                .HasForeignKey(a => a.ChangeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ChangeApproval configuration
        modelBuilder.Entity<ITSM.ChangeApproval>(entity =>
        {
            entity.HasKey(e => e.ApprovalId);
            entity.HasIndex(e => e.ChangeId).HasDatabaseName("IX_ChangeApprovals_ChangeId");
            entity.HasIndex(e => e.ApproverId).HasDatabaseName("IX_ChangeApprovals_ApproverId");
            entity.HasIndex(e => e.ApprovalStatus).HasDatabaseName("IX_ChangeApprovals_ApprovalStatus");
            entity.HasIndex(e => new { e.ChangeId, e.ApprovalRole }).IsUnique().HasDatabaseName("IX_ChangeApprovals_ChangeId_ApprovalRole");

            // ChangeApproval -> Change (many-to-one)
            entity.HasOne(e => e.Change)
                .WithMany(c => c.Approvals)
                .HasForeignKey(e => e.ChangeId)
                .OnDelete(DeleteBehavior.Cascade);

            // ChangeApproval -> User (Approver)
            entity.HasOne(e => e.Approver)
                .WithMany()
                .HasForeignKey(e => e.ApproverId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ChangeImpactedCI configuration
        modelBuilder.Entity<ITSM.ChangeImpactedCI>(entity =>
        {
            entity.HasKey(e => e.ChangeImpactedCIId);
            entity.HasIndex(e => e.ChangeId).HasDatabaseName("IX_ChangeImpactedCIs_ChangeId");
            entity.HasIndex(e => e.CIId).HasDatabaseName("IX_ChangeImpactedCIs_CIId");
            entity.HasIndex(e => e.Impact).HasDatabaseName("IX_ChangeImpactedCIs_ImpactLevel");
            entity.HasIndex(e => new { e.ChangeId, e.CIId }).IsUnique().HasDatabaseName("IX_ChangeImpactedCIs_ChangeId_CIId");

            // ChangeImpactedCI -> Change (many-to-one)
            entity.HasOne(e => e.Change)
                .WithMany(c => c.ImpactedCIs)
                .HasForeignKey(e => e.ChangeId)
                .OnDelete(DeleteBehavior.Cascade);

            // ChangeImpactedCI -> ConfigurationItem (many-to-one)
            entity.HasOne(e => e.ConfigurationItem)
                .WithMany()
                .HasForeignKey(e => e.CIId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Service -> ServiceCI relationship (one-to-many)
        modelBuilder.Entity<ITSM.Service>(entity =>
        {
            entity.HasMany(e => e.ConfigurationItems)
                .WithOne(sc => sc.Service)
                .HasForeignKey(sc => sc.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ServiceCI configuration
        modelBuilder.Entity<ITSM.ServiceCI>(entity =>
        {
            entity.HasKey(e => e.ServiceCIId);
            entity.HasIndex(e => e.ServiceId).HasDatabaseName("IX_ServiceCIs_ServiceId");
            entity.HasIndex(e => e.CIId).HasDatabaseName("IX_ServiceCIs_CIId");
            entity.HasIndex(e => new { e.ServiceId, e.CIId }).IsUnique().HasDatabaseName("IX_ServiceCIs_ServiceId_CIId");

            // ServiceCI -> Service (many-to-one)
            entity.HasOne(e => e.Service)
                .WithMany(s => s.ConfigurationItems)
                .HasForeignKey(e => e.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);

            // ServiceCI -> ConfigurationItem (many-to-one)
            entity.HasOne(e => e.ConfigurationItem)
                .WithMany()
                .HasForeignKey(e => e.CIId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // =============================================================================
        // Webhook Integration Entities
        // =============================================================================
        modelBuilder.Entity<ITSM.WebhookSubscription>(entity =>
        {
            entity.ToTable("WebhookSubscriptions");
            entity.HasKey(e => e.WebhookSubscriptionId);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.TargetUrl)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.EventTypes)
                .HasDefaultValue("[]");

            entity.Property(e => e.Headers)
                .HasDefaultValue("{}");

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            entity.Property(e => e.RetryCount)
                .HasDefaultValue(3);

            entity.Property(e => e.TimeoutSeconds)
                .HasDefaultValue(30);

            entity.HasIndex(e => e.IsActive)
                .HasDatabaseName("IX_WebhookSubscriptions_IsActive");

            entity.HasIndex(e => e.LastTriggeredAt)
                .HasDatabaseName("IX_WebhookSubscriptions_LastTriggeredAt");

            entity.HasIndex(e => e.CreatedByUserId)
                .HasDatabaseName("IX_WebhookSubscriptions_CreatedByUserId");
        });

        modelBuilder.Entity<ITSM.WebhookDelivery>(entity =>
        {
            entity.ToTable("WebhookDeliveries");
            entity.HasKey(e => e.WebhookDeliveryId);

            entity.Property(e => e.EventType)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.TargetUrl)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.Success)
                .HasDefaultValue(false);

            entity.Property(e => e.AttemptNumber)
                .HasDefaultValue(1);

            entity.HasOne<ITSM.WebhookSubscription>(e => e.Subscription)
                .WithMany(s => s.Deliveries)
                .HasForeignKey(e => e.WebhookSubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.WebhookSubscriptionId)
                .HasDatabaseName("IX_WebhookDeliveries_WebhookSubscriptionId");

            entity.HasIndex(new[] { "WebhookSubscriptionId", "Success" })
                .HasDatabaseName("IX_WebhookDeliveries_WebhookSubscriptionId_Success");

            entity.HasIndex(new[] { "Success", "CreatedAt" })
                .HasDatabaseName("IX_WebhookDeliveries_Success_CreatedAt");
        });

        // --- New entity configurations for seed data endpoints ---

        modelBuilder.Entity<CRM.Core.Entities.ITSM.CITypeDefinition>(entity =>
        {
            entity.ToTable("CITypeDefinitions");
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<CRM.Core.Entities.ITSM.ChangeTypeEntity>(entity =>
        {
            entity.ToTable("ChangeTypes");
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<CRM.Core.Entities.ITSM.IncidentCategory>(entity =>
        {
            entity.ToTable("IncidentCategories");
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<AIAgentUsage>(entity =>
        {
            entity.ToTable("AIAgentUsages");
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<ExportJob>(entity =>
        {
            entity.ToTable("ExportJobs");
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<ImportJob>(entity =>
        {
            entity.ToTable("ImportJobs");
            entity.HasKey(e => e.Id);
        });

        // Configure SubscriptionRenewal
        modelBuilder.Entity<SubscriptionRenewal>(entity =>
        {
            entity.ToTable("SubscriptionRenewals");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasColumnType("DECIMAL(18,4)");
            entity.Property(e => e.Notes).HasMaxLength(1000);

            entity.HasOne(e => e.Subscription)
                .WithMany()
                .HasForeignKey(e => e.SubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Invoice)
                .WithMany()
                .HasForeignKey(e => e.InvoiceId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.SubscriptionId)
                .HasDatabaseName("IX_SubscriptionRenewals_SubscriptionId");
            entity.HasIndex(e => e.RenewalDate)
                .HasDatabaseName("IX_SubscriptionRenewals_RenewalDate");
            entity.HasIndex(e => e.Status)
                .HasDatabaseName("IX_SubscriptionRenewals_Status");
        });

        // Configure BillingHistory
        modelBuilder.Entity<BillingHistory>(entity =>
        {
            entity.ToTable("BillingHistory");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasColumnType("DECIMAL(18,4)");
            entity.Property(e => e.ProratedAmount).HasColumnType("DECIMAL(18,4)");
            entity.Property(e => e.UsageCharges).HasColumnType("DECIMAL(18,4)");
            entity.Property(e => e.DiscountAmount).HasColumnType("DECIMAL(18,4)");
            entity.Property(e => e.TaxAmount).HasColumnType("DECIMAL(18,4)");
            entity.Property(e => e.EventDetails).HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(e => e.Subscription)
                .WithMany()
                .HasForeignKey(e => e.SubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Invoice)
                .WithMany()
                .HasForeignKey(e => e.InvoiceId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.DunningRecord)
                .WithMany()
                .HasForeignKey(e => e.DunningRecordId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.SubscriptionId)
                .HasDatabaseName("IX_BillingHistory_SubscriptionId");
            entity.HasIndex(e => e.EventType)
                .HasDatabaseName("IX_BillingHistory_EventType");
            entity.HasIndex(e => e.EventDate)
                .HasDatabaseName("IX_BillingHistory_EventDate");
        });

        // Configure DunningRecord
        modelBuilder.Entity<DunningRecord>(entity =>
        {
            entity.ToTable("DunningRecords");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OutstandingAmount).HasColumnType("DECIMAL(18,4)");
            entity.Property(e => e.RecoveredAmount).HasColumnType("DECIMAL(18,4)");
            entity.Property(e => e.Reason).HasMaxLength(200);
            entity.Property(e => e.LastErrorMessage).HasMaxLength(500);
            entity.Property(e => e.NotificationEmail).HasMaxLength(255);
            entity.Property(e => e.Notes).HasMaxLength(1000);

            entity.HasOne(e => e.Subscription)
                .WithMany()
                .HasForeignKey(e => e.SubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Invoice)
                .WithMany()
                .HasForeignKey(e => e.InvoiceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.BillingHistory)
                .WithMany()
                .HasForeignKey(e => e.BillingHistoryId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.SubscriptionId)
                .HasDatabaseName("IX_DunningRecords_SubscriptionId");
            entity.HasIndex(e => e.Status)
                .HasDatabaseName("IX_DunningRecords_Status");
            entity.HasIndex(e => e.NextRetryDate)
                .HasDatabaseName("IX_DunningRecords_NextRetryDate");
        });

        // Apply provider-specific post-configuration using the Strategy Pattern
        // For SQL Server: Sets all FKs to NoAction to avoid cascade path issues
        // For MySQL/MariaDB: Converts LONGTEXT columns to TEXT to avoid row size limits
        providerStrategy.ApplyPostConfiguration(modelBuilder);
    }
}
