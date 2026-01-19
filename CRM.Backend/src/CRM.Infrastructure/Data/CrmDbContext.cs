using CRM.Core.Entities;
using CRM.Core.Interfaces;
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
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Company).HasMaxLength(255);
            entity.HasIndex(e => e.Email).IsUnique();
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
    }
}
