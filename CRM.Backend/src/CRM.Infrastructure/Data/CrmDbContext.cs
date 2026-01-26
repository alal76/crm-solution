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
using CRM.Core.Entities.Workflow;
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
    public DbSet<BackupSchedule> BackupSchedules { get; set; }
    
    // Contact entities
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
    public DbSet<Activity> Activities { get; set; }
    
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
    
    // Color palettes
    public DbSet<ColorPalette> ColorPalettes { get; set; }
    
    // Module field configurations
        public DbSet<ModuleFieldConfiguration> ModuleFieldConfigurations { get; set; }
        public DbSet<ModuleUIConfig> ModuleUIConfigs { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<FieldMasterDataLink> FieldMasterDataLinks { get; set; }
    
    // Communication entities
    public DbSet<CommunicationChannel> CommunicationChannels { get; set; }
    public DbSet<CommunicationMessage> CommunicationMessages { get; set; }
    public DbSet<EmailTemplate> EmailTemplates { get; set; }
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
            
            entity.HasOne(e => e.Contact)
                .WithMany(c => c.CustomerContacts)
                .HasForeignKey(e => e.ContactId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Opportunity (3NF structure)
        modelBuilder.Entity<Opportunity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            
            // Link Opportunity -> Account (required)
            entity.HasOne(e => e.Account)
                .WithMany(a => a.Opportunities)
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
        modelBuilder.Entity<Customer>(entity =>
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
            // Preferred contact method uses LookupItem
            entity.HasOne(c => c.PreferredContactMethodLookup)
                .WithMany()
                .HasForeignKey(c => c.PreferredContactMethodLookupId)
                .OnDelete(DeleteBehavior.SetNull);
            
            // Contact belongs to Customer (one-to-many)
            entity.HasOne(c => c.Customer)
                .WithMany(cust => cust.Contacts)
                .HasForeignKey(c => c.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);
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

        // Configure Account (Contract)
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AccountNumber).HasMaxLength(100);
            entity.Property(e => e.ContractReference).HasMaxLength(200);
            entity.Property(e => e.ContractFileName).HasMaxLength(1000);
            entity.Property(e => e.ContractFilePath).HasMaxLength(2000);
            entity.Property(e => e.ContractContentType).HasMaxLength(200);
            entity.Property(e => e.Currency).HasMaxLength(10);
            entity.Property(e => e.BillingCycle).HasMaxLength(50);
            entity.Property(e => e.BillingContactEmail).HasMaxLength(255);
            entity.HasIndex(e => e.AccountNumber).IsUnique(false);

            entity.HasOne(e => e.Customer)
                .WithMany(c => c.Accounts)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.Accounts)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.SetNull);
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
            
            entity.HasOne(e => e.Customer)
                .WithMany()
                .HasForeignKey(e => e.CustomerId)
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
            
            // Lead -> Campaign
            entity.HasOne(e => e.Campaign)
                .WithMany()
                .HasForeignKey(e => e.CampaignId)
                .OnDelete(DeleteBehavior.SetNull);
            
            // Lead -> Account
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
        
        // Configure Opportunity (3NF)
        modelBuilder.Entity<Opportunity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.Region).HasMaxLength(100);
            entity.Property(e => e.SolutionNotes).HasMaxLength(4000);
            entity.Property(e => e.QualificationNotes).HasMaxLength(4000);
            
            // Opportunity -> Account (required)
            entity.HasOne(e => e.Account)
                .WithMany()
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Opportunity -> Contact (Primary)
            entity.HasOne(e => e.PrimaryContact)
                .WithMany()
                .HasForeignKey(e => e.PrimaryContactId)
                .OnDelete(DeleteBehavior.SetNull);
            
            // Opportunity -> User (Sales Owner)
            entity.HasOne(e => e.SalesOwner)
                .WithMany()
                .HasForeignKey(e => e.SalesOwnerId)
                .OnDelete(DeleteBehavior.SetNull);
            
            // Opportunity -> Lead
            entity.HasOne(e => e.Lead)
                .WithMany(l => l.Opportunities)
                .HasForeignKey(e => e.LeadId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasIndex(e => e.Stage);
            entity.HasIndex(e => e.ExpectedCloseDate);
            entity.HasIndex(e => e.AccountId);
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
            
            entity.HasOne(e => e.Customer)
                .WithMany()
                .HasForeignKey(e => e.CustomerId)
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
            entity.Property(e => e.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<CRM.Core.Entities.EntityTag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Tag).IsRequired().HasMaxLength(500);
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
            entity.HasIndex(e => e.TagId);
        });

        modelBuilder.Entity<CRM.Core.Entities.CustomField>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Key).HasMaxLength(200);
            entity.Property(e => e.Value).HasColumnType("TEXT");
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
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
            entity.Property(e => e.BuildLog).HasColumnType("LONGTEXT");
            entity.Property(e => e.DeployLog).HasColumnType("LONGTEXT");
            entity.Property(e => e.ErrorMessage).HasMaxLength(2000);
            entity.Property(e => e.ErrorStackTrace).HasColumnType("TEXT");
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
    }
}
