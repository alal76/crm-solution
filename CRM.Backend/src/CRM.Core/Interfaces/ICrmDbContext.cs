// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using CRM.Core.Entities;
using CRM.Core.Entities.Integration;
using CRM.Core.Entities.KnowledgeBase;
using CRM.Core.Entities.Workers;
using CRM.Core.Entities.Workflow;
using CRM.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace CRM.Core.Interfaces;

/// <summary>
/// Database context interface supporting multiple databases
/// </summary>
public interface ICrmDbContext
{
    DbSet<Account> Accounts { get; }
    
    /// <summary>
    /// Customers alias for Accounts (for backward compatibility)
    /// </summary>
    IQueryable<Account> Customers { get; }
    DbSet<Preferences> Preferences { get; }
    DbSet<CRM.Core.Models.Contact> Contacts { get; }
    DbSet<Lead> Leads { get; }
    DbSet<LeadProductInterest> LeadProductInterests { get; }
    DbSet<Opportunity> Opportunities { get; }
    DbSet<OpportunityProduct> OpportunityProducts { get; }
    DbSet<Product> Products { get; }
    DbSet<ProductBundle> ProductBundles { get; }
    DbSet<ProductBundleItem> ProductBundleItems { get; }
    DbSet<ProductBundleRule> ProductBundleRules { get; }
    DbSet<PriceBook> PriceBooks { get; }
    DbSet<PriceBookEntry> PriceBookEntries { get; }
    DbSet<PricingRule> PricingRules { get; }
    DbSet<PricingRuleUsage> PricingRuleUsages { get; }
    DbSet<Interaction> Interactions { get; }
    DbSet<MarketingCampaign> MarketingCampaigns { get; }
    DbSet<CampaignRecipient> CampaignRecipients { get; }
    DbSet<CampaignMetric> CampaignMetrics { get; }
    DbSet<CampaignConversion> CampaignConversions { get; }
    DbSet<User> Users { get; }
    DbSet<OAuthToken> OAuthTokens { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<WebAuthnCredential> WebAuthnCredentials { get; }
    DbSet<Department> Departments { get; }
    DbSet<UserProfile> UserProfiles { get; }
    DbSet<UserGroup> UserGroups { get; }
    DbSet<UserGroupMember> UserGroupMembers { get; }
    DbSet<UserApprovalRequest> UserApprovalRequests { get; }
    
    // RBAC - Role-Based Access Control (SYS-012)
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<UserRoleAssignment> UserRoleAssignments { get; }
    
    /// <summary>Alias for UserRoleAssignments</summary>
    DbSet<UserRoleAssignment> UserRoles { get; }
    
    DbSet<DatabaseBackup> DatabaseBackups { get; }
    DbSet<BackupSchedule> BackupSchedules { get; }
    DbSet<SystemSettings> SystemSettings { get; }
    DbSet<BrandingConfig> BrandingConfigs { get; }
    DbSet<CrmTask> CrmTasks { get; }
    DbSet<Note> Notes { get; }
    DbSet<Activity> Activities { get; }
    DbSet<Quote> Quotes { get; }

    // Service Request entities
    DbSet<ServiceRequest> ServiceRequests { get; }
    DbSet<ServiceRequestCategory> ServiceRequestCategories { get; }
    DbSet<ServiceRequestSubcategory> ServiceRequestSubcategories { get; }
    DbSet<ServiceRequestType> ServiceRequestTypes { get; }
    DbSet<ServiceRequestCustomFieldDefinition> ServiceRequestCustomFieldDefinitions { get; }
    DbSet<ServiceRequestCustomFieldValue> ServiceRequestCustomFieldValues { get; }
    DbSet<ModuleFieldConfiguration> ModuleFieldConfigurations { get; }
    DbSet<FieldMasterDataLink> FieldMasterDataLinks { get; }
    DbSet<Address> Addresses { get; }
    DbSet<EntityAddressLink> EntityAddressLinks { get; }
    DbSet<ContactDetail> ContactDetails { get; }
    DbSet<SocialAccount> SocialAccounts { get; }
    DbSet<ContactInfoLink> ContactInfoLinks { get; }
    DbSet<LookupCategory> LookupCategories { get; }
    DbSet<LookupItem> LookupItems { get; }
    DbSet<ZipCode> ZipCodes { get; }
    DbSet<CRM.Core.Entities.Tag> Tags { get; }
    DbSet<CRM.Core.Entities.EntityTag> EntityTags { get; }
    DbSet<CRM.Core.Entities.CustomField> CustomFields { get; }
    DbSet<ModuleUIConfig> ModuleUIConfigs { get; }

    // Contact-level social media links
    DbSet<SocialMediaLink> SocialMediaLinks { get; }

    // Color palettes
    DbSet<ColorPalette> ColorPalettes { get; }

    // Communication
    DbSet<CommunicationMessage> CommunicationMessages { get; }
    DbSet<CommunicationChannel> CommunicationChannels { get; }

    // Email Templates
    DbSet<EmailTemplate> EmailTemplates { get; }
    DbSet<EmailTemplateHistoryEntry> EmailTemplateHistoryEntries { get; }
    DbSet<EmailTemplateUsage> EmailTemplateUsages { get; }
    DbSet<EmailTemplateVersion> EmailTemplateVersions { get; }

    // Phase 4 - Sales & Billing
    DbSet<Order> Orders { get; }
    DbSet<OrderLineItem> OrderLineItems { get; }
    DbSet<Invoice> Invoices { get; }
    DbSet<InvoiceLineItem> InvoiceLineItems { get; }
    DbSet<Payment> Payments { get; }
    DbSet<Subscription> Subscriptions { get; }
    DbSet<SubscriptionItem> SubscriptionItems { get; }
    DbSet<SubscriptionUsage> SubscriptionUsages { get; }
    DbSet<SubscriptionUsageLimit> SubscriptionUsageLimits { get; }
    DbSet<Contract> Contracts { get; }

    // Credit Memos
    DbSet<CreditMemo> CreditMemos { get; }
    DbSet<CreditMemoLineItem> CreditMemoLineItems { get; }
    DbSet<CreditApplication> CreditApplications { get; }

    // Email Sequences
    DbSet<EmailSequence> EmailSequences { get; }
    DbSet<EmailSequenceStep> EmailSequenceSteps { get; }
    DbSet<EmailSequenceEnrollment> EmailSequenceEnrollments { get; }
    DbSet<EmailSequenceStepExecution> EmailSequenceStepExecutions { get; }

    // Phase 4 - Teams
    DbSet<Team> Teams { get; }
    DbSet<TeamMember> TeamMembers { get; }
    DbSet<AccountTerritory> AccountTerritories { get; }

    // Phase 4 - Commissions
    DbSet<CommissionPlan> CommissionPlans { get; }
    DbSet<CommissionTier> CommissionTiers { get; }
    DbSet<CommissionPlanAssignment> CommissionPlanAssignments { get; }
    DbSet<Commission> Commissions { get; }
    DbSet<CommissionStatement> CommissionStatements { get; }
    DbSet<CommissionRule> CommissionRules { get; }
    DbSet<CommissionHistory> CommissionHistories { get; }
    DbSet<CommissionApprovalAudit> CommissionApprovalAudits { get; }
    DbSet<DiscountRule> DiscountRules { get; }
    DbSet<DiscountHistory> DiscountHistories { get; }

    // Cloud Deployment entities
    DbSet<CloudProvider> CloudProviders { get; }
    DbSet<CloudDeployment> CloudDeployments { get; }
    DbSet<DeploymentAttempt> DeploymentAttempts { get; }
    DbSet<HealthCheckLog> HealthCheckLogs { get; }

    // LLM Settings
    DbSet<LLMProviderSetting> LLMProviderSettings { get; }

    // Lead Routing
    DbSet<LeadRoutingRule> LeadRoutingRules { get; }
    DbSet<LeadRoutingCriteria> LeadRoutingCriteria { get; }
    DbSet<LeadRoutingTarget> LeadRoutingTargets { get; }
    DbSet<LeadRoutingLog> LeadRoutingLogs { get; }

    // Territory
    DbSet<AccountTerritoryAssignment> AccountTerritoryAssignments { get; }

    // Form Builder
    DbSet<FormDefinition> FormDefinitions { get; }
    DbSet<FormField> FormFields { get; }
    DbSet<FormSubmission> FormSubmissions { get; }

    // Approval Workflow
    DbSet<DiscountApprovalMatrix> DiscountApprovalMatrices { get; }
    DbSet<ApprovalLevel> ApprovalLevels { get; }
    DbSet<ApprovalGroup> ApprovalGroups { get; }
    DbSet<ApprovalGroupMember> ApprovalGroupMembers { get; }
    DbSet<ApprovalRequest> ApprovalRequests { get; }
    DbSet<ApprovalStep> ApprovalSteps { get; }

    // ITSM Module
    DbSet<CRM.Core.Entities.ITSM.Incident> Incidents { get; }
    DbSet<CRM.Core.Entities.ITSM.IncidentComment> IncidentComments { get; }
    DbSet<CRM.Core.Entities.ITSM.IncidentAttachment> IncidentAttachments { get; }
    DbSet<CRM.Core.Entities.ITSM.IncidentHistory> IncidentHistory { get; }
    DbSet<CRM.Core.Entities.ITSM.Problem> Problems { get; }
    DbSet<CRM.Core.Entities.ITSM.ProblemIncident> ProblemIncidents { get; }
    DbSet<CRM.Core.Entities.ITSM.ProblemTask> ProblemTasks { get; }
    DbSet<CRM.Core.Entities.ITSM.ProblemComment> ProblemComments { get; }
    DbSet<CRM.Core.Entities.ITSM.ProblemAttachment> ProblemAttachments { get; }
    
    // SLA (Service Level Agreement) - Non-ITSM versions
    DbSet<CRM.Core.Entities.SLAPolicy> SLAPolicies { get; }
    DbSet<CRM.Core.Entities.KnowledgeBase.SLAInstance> SLAInstances { get; }
    // DISABLED: Conflicts with ITSM.EscalationRule - both trying to use "EscalationRule" table
    // DbSet<CRM.Core.Entities.EscalationRule> EscalationRules { get; }
    DbSet<CRM.Core.Entities.ITSM.ServiceQueue> ServiceQueues { get; }
    
    // ITSM SLA entities
    DbSet<CRM.Core.Entities.ITSM.SLAPolicy> ITSMSLAPolicies { get; }
    DbSet<CRM.Core.Entities.ITSM.SLAInstance> ITSMSLAInstances { get; }
    DbSet<CRM.Core.Entities.ITSM.BusinessHoursSchedule> BusinessHoursSchedules { get; }
    DbSet<CRM.Core.Entities.ITSM.ConfigurationItem> ConfigurationItems { get; }
    DbSet<CRM.Core.Entities.ITSM.CIRelationship> CIRelationships { get; }
    DbSet<CRM.Core.Entities.ITSM.Service> Services { get; }
    DbSet<CRM.Core.Entities.ITSM.ServiceCI> ServiceCIs { get; }
    DbSet<CRM.Core.Entities.ITSM.Change> Changes { get; }
    DbSet<CRM.Core.Entities.ITSM.ChangeApproval> ChangeApprovals { get; }
    DbSet<CRM.Core.Entities.ITSM.ChangeBlackout> ChangeBlackouts { get; }
    DbSet<CRM.Core.Entities.ITSM.ChangeImpactedCI> ChangeImpactedCIs { get; }
    DbSet<CRM.Core.Entities.ITSM.ChangeTask> ChangeTasks { get; }
    DbSet<CRM.Core.Entities.ITSM.ChangeComment> ChangeComments { get; }
    DbSet<CRM.Core.Entities.ITSM.ChangeAttachment> ChangeAttachments { get; }
    DbSet<CRM.Core.Entities.ITSM.KnowledgeArticle> ITSMKnowledgeArticles { get; }
    DbSet<CRM.Core.Entities.ITSM.ArticleRelationship> ArticleRelationships { get; }
    DbSet<CRM.Core.Entities.ITSM.ArticleIncident> ArticleIncidents { get; }
    DbSet<CRM.Core.Entities.ITSM.ArticleFeedback> ITSMArticleFeedback { get; }
    DbSet<CRM.Core.Entities.ITSM.ArticleAttachment> ArticleAttachments { get; }
    DbSet<CRM.Core.Entities.ITSM.CatalogCategory> CatalogCategories { get; }
    DbSet<CRM.Core.Entities.ITSM.CatalogItem> CatalogItems { get; }
    DbSet<CRM.Core.Entities.ITSM.CatalogVariable> CatalogVariables { get; }
    DbSet<CRM.Core.Entities.ITSM.CatalogRequest> CatalogRequests { get; }
    DbSet<CRM.Core.Entities.ITSM.CatalogRequestApproval> CatalogRequestApprovals { get; }
    DbSet<CRM.Core.Entities.ITSM.CatalogRequestComment> CatalogRequestComments { get; }

    // Workflow Engine
    DbSet<WorkflowDefinition> WorkflowDefinitions { get; }
    DbSet<WorkflowVersion> WorkflowVersions { get; }
    DbSet<WorkflowNode> WorkflowNodes { get; }
    DbSet<WorkflowTransition> WorkflowTransitions { get; }
    DbSet<WorkflowInstance> WorkflowInstances { get; }
    DbSet<WorkflowNodeInstance> WorkflowNodeInstances { get; }
    DbSet<WorkflowTask> WorkflowTasks { get; }
    DbSet<WorkflowLog> WorkflowLogs { get; }
    DbSet<WorkflowTrigger> WorkflowTriggers { get; }

    // Sales Performance
    DbSet<SalesQuota> SalesQuotas { get; }
    DbSet<SalesForecast> SalesForecasts { get; }
    DbSet<ForecastLineItem> ForecastLineItems { get; }
    DbSet<ForecastHistory> ForecastHistories { get; }

    // Communication
    DbSet<Conversation> Conversations { get; }

    // Event Attendees
    DbSet<EventAttendee> EventAttendees { get; }

    // Dashboard and Analytics
    DbSet<Dashboard> Dashboards { get; }
    DbSet<DashboardWidget> DashboardWidgets { get; }
    DbSet<UIPreference> UIPreferences { get; }
    DbSet<UICustomization> UICustomizations { get; }
    DbSet<DashboardCustomization> DashboardCustomizations { get; }
    DbSet<PerformanceMetric> PerformanceMetrics { get; }
    DbSet<FeatureFlagAuditLog> FeatureFlagAuditLogs { get; }

    // Integration & Webhooks
    DbSet<CRM.Core.Entities.ITSM.WebhookSubscription> WebhookSubscriptions { get; }
    DbSet<CRM.Core.Entities.ITSM.WebhookDelivery> WebhookDeliveries { get; }

    // Worker architecture
    DbSet<WorkerJob> WorkerJobs { get; }
    DbSet<WorkerExecution> WorkerExecutions { get; }
    DbSet<OutboxEvent> OutboxEvents { get; }

    // Reports
    DbSet<CRM.Core.Entities.Reports.ReportDefinition> ReportDefinitions { get; }
    DbSet<CRM.Core.Entities.Reports.ReportFolder> ReportFolders { get; }
    DbSet<CRM.Core.Entities.Reports.ReportSchedule> ReportSchedules { get; }
    DbSet<CRM.Core.Entities.Reports.ReportExecution> ReportExecutions { get; }
    DbSet<CRM.Core.Entities.Reports.ReportWidgetConfig> ReportWidgetConfigs { get; }

    // AI Agent Entities (ADR-004 — Semantic Kernel Integration)
    DbSet<CRM.Core.Entities.AI.AIAgent> AIAgents { get; }
    DbSet<CRM.Core.Entities.AI.AgentConversation> AgentConversations { get; }
    DbSet<CRM.Core.Entities.AI.AgentAction> AgentActions { get; }
    DbSet<CRM.Core.Entities.AI.AgentMemory> AgentMemories { get; }
    DbSet<CRM.Core.Entities.AI.AgentApprovalRequest> AgentApprovalRequests { get; }

    DatabaseFacade Database { get; }

    /// <summary>
    /// Gets a DbSet for the specified entity type, enabling generic repository pattern
    /// </summary>
    DbSet<TEntity> Set<TEntity>() where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
