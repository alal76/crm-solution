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
using CRM.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace CRM.Core.Interfaces;

/// <summary>
/// Database context interface supporting multiple databases
/// </summary>
public interface ICrmDbContext
{
    DbSet<Account> Customers { get; }
    DbSet<CRM.Core.Models.Contact> Contacts { get; }
    DbSet<Lead> Leads { get; }
    DbSet<LeadProductInterest> LeadProductInterests { get; }
    DbSet<Opportunity> Opportunities { get; }
    DbSet<OpportunityProduct> OpportunityProducts { get; }
    DbSet<Product> Products { get; }
    DbSet<Interaction> Interactions { get; }
    DbSet<MarketingCampaign> MarketingCampaigns { get; }
    DbSet<CampaignMetric> CampaignMetrics { get; }
    DbSet<User> Users { get; }
    DbSet<OAuthToken> OAuthTokens { get; }
    DbSet<Department> Departments { get; }
    DbSet<UserProfile> UserProfiles { get; }
    DbSet<UserGroup> UserGroups { get; }
    DbSet<UserGroupMember> UserGroupMembers { get; }
    DbSet<UserApprovalRequest> UserApprovalRequests { get; }
    DbSet<DatabaseBackup> DatabaseBackups { get; }
    DbSet<BackupSchedule> BackupSchedules { get; }
    DbSet<SystemSettings> SystemSettings { get; }
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
    DbSet<Account> Accounts { get; }
    DbSet<Address> Addresses { get; }
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

    // Cloud Deployment entities
    DbSet<CloudProvider> CloudProviders { get; }
    DbSet<CloudDeployment> CloudDeployments { get; }
    DbSet<DeploymentAttempt> DeploymentAttempts { get; }
    DbSet<HealthCheckLog> HealthCheckLogs { get; }

    // LLM Settings
    DbSet<LLMProviderSetting> LLMProviderSettings { get; }

    // ITSM Module
    DbSet<CRM.Core.Entities.ITSM.Incident> Incidents { get; }
    DbSet<CRM.Core.Entities.ITSM.IncidentComment> IncidentComments { get; }
    DbSet<CRM.Core.Entities.ITSM.IncidentHistory> IncidentHistory { get; }
    DbSet<CRM.Core.Entities.ITSM.Problem> Problems { get; }
    DbSet<CRM.Core.Entities.ITSM.ProblemIncident> ProblemIncidents { get; }
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
    DbSet<CRM.Core.Entities.ITSM.KnowledgeArticle> ITSMKnowledgeArticles { get; }
    DbSet<CRM.Core.Entities.ITSM.ArticleFeedback> ITSMArticleFeedback { get; }
    DbSet<CRM.Core.Entities.ITSM.CatalogCategory> CatalogCategories { get; }
    DbSet<CRM.Core.Entities.ITSM.CatalogItem> CatalogItems { get; }
    DbSet<CRM.Core.Entities.ITSM.CatalogRequest> CatalogRequests { get; }

    DatabaseFacade Database { get; }

    /// <summary>
    /// Gets a DbSet for the specified entity type, enabling generic repository pattern
    /// </summary>
    DbSet<TEntity> Set<TEntity>() where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
