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
    DbSet<Workflow> Workflows { get; }
    DbSet<WorkflowRule> WorkflowRules { get; }
    DbSet<WorkflowRuleCondition> WorkflowRuleConditions { get; }
    DbSet<WorkflowExecution> WorkflowExecutions { get; }
    DbSet<SystemSettings> SystemSettings { get; }
    DbSet<CrmTask> CrmTasks { get; }
    DbSet<Note> Notes { get; }
    DbSet<Quote> Quotes { get; }
    
    DatabaseFacade Database { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
