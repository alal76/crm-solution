using CRM.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Core.Interfaces;

/// <summary>
/// Database context interface supporting multiple databases
/// </summary>
public interface ICrmDbContext
{
    DbSet<Customer> Customers { get; }
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

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
