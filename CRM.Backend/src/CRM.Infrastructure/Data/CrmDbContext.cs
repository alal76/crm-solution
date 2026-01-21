using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

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

    public DbSet<Customer> Customers { get; set; }
    public DbSet<CustomerContact> CustomerContacts { get; set; }
    public DbSet<Opportunity> Opportunities { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Interaction> Interactions { get; set; }
    public DbSet<MarketingCampaign> MarketingCampaigns { get; set; }
    public DbSet<CampaignMetric> CampaignMetrics { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<OAuthToken> OAuthTokens { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<UserGroup> UserGroups { get; set; }
    public DbSet<UserGroupMember> UserGroupMembers { get; set; }
    public DbSet<UserApprovalRequest> UserApprovalRequests { get; set; }
    public DbSet<DatabaseBackup> DatabaseBackups { get; set; }
    
    // Workflow entities
    public DbSet<Workflow> Workflows { get; set; }
    public DbSet<WorkflowRule> WorkflowRules { get; set; }
    public DbSet<WorkflowRuleCondition> WorkflowRuleConditions { get; set; }
    public DbSet<WorkflowExecution> WorkflowExecutions { get; set; }
    
    // Contact entities
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<SocialMediaLink> SocialMediaLinks { get; set; }
    
    // Lead entity
    public DbSet<Lead> Leads { get; set; }
    
    // New comprehensive entities
    public DbSet<CrmTask> CrmTasks { get; set; }
    public DbSet<Note> Notes { get; set; }
    public DbSet<Quote> Quotes { get; set; }
    public DbSet<Activity> Activities { get; set; }
    
    // System settings
    public DbSet<SystemSettings> SystemSettings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured && _configuration != null)
        {
            var databaseProvider = _configuration["DatabaseProvider"] ?? "sqlite";
            var connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "Data Source=crm.db";

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
                case "sqlite":
                    optionsBuilder.UseSqlite(connectionString);
                    break;
                case "sqlserver":
                default:
                    optionsBuilder.UseSqlite("Data Source=crm.db");
                    break;
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Customer
        modelBuilder.Entity<Customer>(entity =>
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
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.Company);
            
            // Self-referencing relationships
            entity.HasOne(e => e.ReferredByCustomer)
                .WithMany()
                .HasForeignKey(e => e.ReferredByCustomerId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(e => e.ParentCustomer)
                .WithMany()
                .HasForeignKey(e => e.ParentCustomerId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure CustomerContact (junction table for organization contacts)
        modelBuilder.Entity<CustomerContact>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.CustomerId, e.ContactId }).IsUnique();
            entity.Property(e => e.PositionAtCustomer).HasMaxLength(100);
            entity.Property(e => e.DepartmentAtCustomer).HasMaxLength(100);
            
            entity.HasOne(e => e.Customer)
                .WithMany(c => c.CustomerContacts)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Opportunity
        modelBuilder.Entity<Opportunity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.HasOne(e => e.Customer)
                .WithMany(c => c.Opportunities)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Product)
                .WithMany(p => p.Opportunities)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.SetNull);
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
        });

        // Configure Interaction
        modelBuilder.Entity<Interaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.HasOne(e => e.Customer)
                .WithMany(c => c.Interactions)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure MarketingCampaign
        modelBuilder.Entity<MarketingCampaign>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Budget).HasPrecision(18, 2);
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
        });
        
        // Configure UserGroup
        modelBuilder.Entity<UserGroup>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
        });
        
        // Configure UserGroupMember
        modelBuilder.Entity<UserGroupMember>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.GroupMemberships)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.UserGroup)
                .WithMany(g => g.Members)
                .HasForeignKey(e => e.UserGroupId)
                .OnDelete(DeleteBehavior.Cascade);
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

        // Configure Workflow
        modelBuilder.Entity<Workflow>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EntityType).IsRequired().HasMaxLength(50);

            entity.HasMany(e => e.Rules)
                .WithOne(r => r.Workflow)
                .HasForeignKey(r => r.WorkflowId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Executions)
                .WithOne(ex => ex.Workflow)
                .HasForeignKey(ex => ex.WorkflowId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure WorkflowRule
        modelBuilder.Entity<WorkflowRule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ConditionLogic).HasMaxLength(10).HasDefaultValue("AND");

            entity.HasOne(e => e.Workflow)
                .WithMany(w => w.Rules)
                .HasForeignKey(e => e.WorkflowId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.TargetUserGroup)
                .WithMany()
                .HasForeignKey(e => e.TargetUserGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Conditions)
                .WithOne(c => c.WorkflowRule)
                .HasForeignKey(c => c.WorkflowRuleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure WorkflowRuleCondition
        modelBuilder.Entity<WorkflowRuleCondition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FieldName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Operator).IsRequired().HasMaxLength(20);

            entity.HasOne(e => e.WorkflowRule)
                .WithMany(r => r.Conditions)
                .HasForeignKey(e => e.WorkflowRuleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure WorkflowExecution
        modelBuilder.Entity<WorkflowExecution>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EntityType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Success");

            entity.HasOne(e => e.Workflow)
                .WithMany(w => w.Executions)
                .HasForeignKey(e => e.WorkflowId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.WorkflowRule)
                .WithMany()
                .HasForeignKey(e => e.WorkflowRuleId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.SourceUserGroup)
                .WithMany()
                .HasForeignKey(e => e.SourceUserGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.TargetUserGroup)
                .WithMany()
                .HasForeignKey(e => e.TargetUserGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            // Index for audit trail queries
            entity.HasIndex(e => new { e.WorkflowId, e.CreatedAt });
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
        });

        // Configure CrmTask
        modelBuilder.Entity<CrmTask>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Subject).IsRequired().HasMaxLength(255);
            
            entity.HasOne(e => e.Customer)
                .WithMany()
                .HasForeignKey(e => e.CustomerId)
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
        });

        // Configure Note
        modelBuilder.Entity<Note>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Content).IsRequired();
            
            entity.HasOne(e => e.Customer)
                .WithMany()
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Opportunity)
                .WithMany()
                .HasForeignKey(e => e.OpportunityId)
                .OnDelete(DeleteBehavior.Cascade);
            
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
            
            entity.HasOne(e => e.Customer)
                .WithMany()
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasOne(e => e.Opportunity)
                .WithMany()
                .HasForeignKey(e => e.OpportunityId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasOne(e => e.AssignedToUser)
                .WithMany()
                .HasForeignKey(e => e.AssignedToUserId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasOne(e => e.ParentQuote)
                .WithMany(q => q.Revisions)
                .HasForeignKey(e => e.ParentQuoteId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasIndex(e => e.QuoteNumber).IsUnique();
            entity.HasIndex(e => e.Status);
        });

        // Configure Activity
        modelBuilder.Entity<Activity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasOne(e => e.Customer)
                .WithMany()
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Opportunity)
                .WithMany()
                .HasForeignKey(e => e.OpportunityId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => e.ActivityDate);
            entity.HasIndex(e => e.ActivityType);
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
        });

        // Configure Lead
        modelBuilder.Entity<Lead>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Lead -> Customer (ConvertedCustomer) - Lead has ConvertedCustomerId
            entity.HasOne(e => e.ConvertedCustomer)
                .WithMany()
                .HasForeignKey(e => e.ConvertedCustomerId)
                .OnDelete(DeleteBehavior.SetNull);
            
            // Lead -> Opportunity (ConvertedOpportunity)
            entity.HasOne(e => e.ConvertedOpportunity)
                .WithMany()
                .HasForeignKey(e => e.ConvertedOpportunityId)
                .OnDelete(DeleteBehavior.SetNull);
            
            // Lead -> Customer (ReferredByCustomer)
            entity.HasOne(e => e.ReferredByCustomer)
                .WithMany()
                .HasForeignKey(e => e.ReferredByCustomerId)
                .OnDelete(DeleteBehavior.SetNull);
            
            // Lead -> Product (PrimaryProductInterest)
            entity.HasOne(e => e.PrimaryProductInterest)
                .WithMany()
                .HasForeignKey(e => e.PrimaryProductInterestId)
                .OnDelete(DeleteBehavior.SetNull);
            
            // Lead -> Lead (MasterLead for duplicates)
            entity.HasOne(e => e.MasterLead)
                .WithMany(l => l.DuplicateLeads)
                .HasForeignKey(e => e.MasterLeadId)
                .OnDelete(DeleteBehavior.SetNull);
            
            // Lead -> User (Owner)
            entity.HasOne(e => e.Owner)
                .WithMany()
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.SetNull);
            
            // Lead -> User (ConvertedByUser)
            entity.HasOne(e => e.ConvertedByUser)
                .WithMany()
                .HasForeignKey(e => e.ConvertedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
            
            // Lead -> User (DisqualifiedByUser)
            entity.HasOne(e => e.DisqualifiedByUser)
                .WithMany()
                .HasForeignKey(e => e.DisqualifiedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
            
            // Lead -> Campaign (PrimaryCampaign)
            entity.HasOne(e => e.PrimaryCampaign)
                .WithMany()
                .HasForeignKey(e => e.PrimaryCampaignId)
                .OnDelete(DeleteBehavior.SetNull);
            
            // Lead -> Campaign (ConvertingCampaign)
            entity.HasOne(e => e.ConvertingCampaign)
                .WithMany()
                .HasForeignKey(e => e.ConvertingCampaignId)
                .OnDelete(DeleteBehavior.SetNull);
            
            // Lead -> Campaign (LastCampaign)
            entity.HasOne(e => e.LastCampaign)
                .WithMany()
                .HasForeignKey(e => e.LastCampaignId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.LeadScore);
        });

        // Configure Customer -> Lead relationship (ConvertedFromLead)
        modelBuilder.Entity<Customer>(entity =>
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
    }
}
