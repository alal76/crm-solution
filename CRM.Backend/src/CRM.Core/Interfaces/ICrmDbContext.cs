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
    DbSet<Customer> Customers { get; }
    DbSet<Contact> Contacts { get; }
    DbSet<Lead> Leads { get; }
    DbSet<Opportunity> Opportunities { get; }
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
    DbSet<Workflow> Workflows { get; }
    DbSet<WorkflowRule> WorkflowRules { get; }
    DbSet<WorkflowRuleCondition> WorkflowRuleConditions { get; }
    DbSet<WorkflowExecution> WorkflowExecutions { get; }
    DbSet<SystemSettings> SystemSettings { get; }
    DbSet<CrmTask> CrmTasks { get; }
    DbSet<Note> Notes { get; }
    DbSet<Quote> Quotes { get; }
    
    // Service Request entities
    DbSet<ServiceRequest> ServiceRequests { get; }
    DbSet<ServiceRequestCategory> ServiceRequestCategories { get; }
    DbSet<ServiceRequestSubcategory> ServiceRequestSubcategories { get; }
    DbSet<ServiceRequestCustomFieldDefinition> ServiceRequestCustomFieldDefinitions { get; }
    DbSet<ServiceRequestCustomFieldValue> ServiceRequestCustomFieldValues { get; }
    DbSet<ModuleFieldConfiguration> ModuleFieldConfigurations { get; }
    DbSet<Account> Accounts { get; }
    DbSet<Address> Addresses { get; }
    DbSet<ContactDetail> ContactDetails { get; }
    DbSet<SocialAccount> SocialAccounts { get; }
    DbSet<ContactInfoLink> ContactInfoLinks { get; }
    DbSet<LookupCategory> LookupCategories { get; }
    DbSet<LookupItem> LookupItems { get; }
    DbSet<CRM.Core.Entities.Tag> Tags { get; }
    DbSet<CRM.Core.Entities.EntityTag> EntityTags { get; }
    DbSet<CRM.Core.Entities.CustomField> CustomFields { get; }
    DbSet<ModuleUIConfig> ModuleUIConfigs { get; }
    
    DatabaseFacade Database { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
