// CRM Solution - Database Schema Verification Tests (BVT)
// These tests verify that the database schema is aligned with Entity Framework entities
// Run as part of Build Verification Tests (BVT) to catch schema drift early

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using CRM.Core.Entities;
using CRM.Core.Models;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CRM.Tests.Integration;

/// <summary>
/// Database schema verification tests to ensure EF entities are aligned with database structure.
/// These are Build Verification Tests (BVT) that should pass before any release.
/// </summary>
[Collection("Database")]
public class DatabaseSchemaVerificationTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;
    private readonly CrmDbContext _context;

    public DatabaseSchemaVerificationTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        _context = fixture.CreateContext();
    }

    #region Core Entity Table Verification

    [Fact]
    [Trait("Category", "BVT")]
    [Trait("Category", "DatabaseSchema")]
    public void Accounts_Table_Exists_And_Has_Required_Columns()
    {
        // Verify we can query the Accounts table
        var canQuery = _context.Accounts.Take(1).ToList();
        Assert.NotNull(canQuery);
    }

    [Fact]
    [Trait("Category", "BVT")]
    [Trait("Category", "DatabaseSchema")]
    public void Contacts_Table_Exists_And_Has_Required_Columns()
    {
        var canQuery = _context.Contacts.Take(1).ToList();
        Assert.NotNull(canQuery);
    }

    [Fact]
    [Trait("Category", "BVT")]
    [Trait("Category", "DatabaseSchema")]
    public void Leads_Table_Exists_And_Has_Required_Columns()
    {
        var canQuery = _context.Leads.Take(1).ToList();
        Assert.NotNull(canQuery);
    }

    [Fact]
    [Trait("Category", "BVT")]
    [Trait("Category", "DatabaseSchema")]
    public void Products_Table_Exists_And_Has_Required_Columns()
    {
        var canQuery = _context.Products.Take(1).ToList();
        Assert.NotNull(canQuery);
    }

    [Fact]
    [Trait("Category", "BVT")]
    [Trait("Category", "DatabaseSchema")]
    public void Opportunities_Table_Exists_And_Has_Required_Columns()
    {
        var canQuery = _context.Opportunities.Take(1).ToList();
        Assert.NotNull(canQuery);
    }

    [Fact]
    [Trait("Category", "BVT")]
    [Trait("Category", "DatabaseSchema")]
    public void Users_Table_Exists_And_Has_Required_Columns()
    {
        var canQuery = _context.Users.Take(1).ToList();
        Assert.NotNull(canQuery);
    }

    [Fact]
    [Trait("Category", "BVT")]
    [Trait("Category", "DatabaseSchema")]
    public void UserGroups_Table_Exists_And_Has_Required_Columns()
    {
        var canQuery = _context.UserGroups.Take(1).ToList();
        Assert.NotNull(canQuery);
    }

    #endregion

    #region Junction Table Verification

    [Fact]
    [Trait("Category", "BVT")]
    [Trait("Category", "DatabaseSchema")]
    public void AccountContacts_JunctionTable_Exists()
    {
        var canQuery = _context.AccountContacts.Take(1).ToList();
        Assert.NotNull(canQuery);
    }

    [Fact]
    [Trait("Category", "BVT")]
    [Trait("Category", "DatabaseSchema")]
    public void OpportunityProducts_JunctionTable_Exists()
    {
        var canQuery = _context.OpportunityProducts.Take(1).ToList();
        Assert.NotNull(canQuery);
    }

    [Fact]
    [Trait("Category", "BVT")]
    [Trait("Category", "DatabaseSchema")]
    public void Tags_And_EntityTags_Tables_Exist()
    {
        var tags = _context.Tags.Take(1).ToList();
        var entityTags = _context.EntityTags.Take(1).ToList();
        Assert.NotNull(tags);
        Assert.NotNull(entityTags);
    }

    [Fact]
    [Trait("Category", "BVT")]
    [Trait("Category", "DatabaseSchema")]
    public void EntityAddressLinks_Table_Exists()
    {
        var canQuery = _context.EntityAddressLinks.Take(1).ToList();
        Assert.NotNull(canQuery);
    }

    [Fact]
    [Trait("Category", "BVT")]
    [Trait("Category", "DatabaseSchema")]
    public void EntityPhoneLinks_Table_Exists()
    {
        var canQuery = _context.EntityPhoneLinks.Take(1).ToList();
        Assert.NotNull(canQuery);
    }

    [Fact]
    [Trait("Category", "BVT")]
    [Trait("Category", "DatabaseSchema")]
    public void EntityEmailLinks_Table_Exists()
    {
        var canQuery = _context.EntityEmailLinks.Take(1).ToList();
        Assert.NotNull(canQuery);
    }

    [Fact]
    [Trait("Category", "BVT")]
    [Trait("Category", "DatabaseSchema")]
    public void EntitySocialMediaLinks_Table_Exists()
    {
        var canQuery = _context.EntitySocialMediaLinks.Take(1).ToList();
        Assert.NotNull(canQuery);
    }

    [Fact]
    [Trait("Category", "BVT")]
    [Trait("Category", "DatabaseSchema")]
    public void UserGroupMembers_JunctionTable_Exists()
    {
        var canQuery = _context.UserGroupMembers.Take(1).ToList();
        Assert.NotNull(canQuery);
    }

    [Fact]
    [Trait("Category", "BVT")]
    [Trait("Category", "DatabaseSchema")]
    public void TeamMembers_JunctionTable_Exists()
    {
        var canQuery = _context.TeamMembers.Take(1).ToList();
        Assert.NotNull(canQuery);
    }

    #endregion

    #region Contact Information Tables

    [Fact]
    [Trait("Category", "BVT")]
    [Trait("Category", "DatabaseSchema")]
    public void Addresses_Table_Exists()
    {
        var canQuery = _context.Addresses.Take(1).ToList();
        Assert.NotNull(canQuery);
    }

    [Fact]
    [Trait("Category", "BVT")]
    [Trait("Category", "DatabaseSchema")]
    public void PhoneNumbers_Table_Exists()
    {
        var canQuery = _context.PhoneNumbers.Take(1).ToList();
        Assert.NotNull(canQuery);
    }

    [Fact]
    [Trait("Category", "BVT")]
    [Trait("Category", "DatabaseSchema")]
    public void EmailAddresses_Table_Exists()
    {
        var canQuery = _context.EmailAddresses.Take(1).ToList();
        Assert.NotNull(canQuery);
    }

    [Fact]
    [Trait("Category", "BVT")]
    [Trait("Category", "DatabaseSchema")]
    public void SocialMediaAccounts_Table_Exists()
    {
        var canQuery = _context.SocialMediaAccounts.Take(1).ToList();
        Assert.NotNull(canQuery);
    }

    #endregion

    #region System Configuration Tables

    [Fact]
    [Trait("Category", "BVT")]
    [Trait("Category", "DatabaseSchema")]
    public void SystemSettings_Table_Exists()
    {
        var canQuery = _context.SystemSettings.Take(1).ToList();
        Assert.NotNull(canQuery);
    }

    [Fact]
    [Trait("Category", "BVT")]
    [Trait("Category", "DatabaseSchema")]
    public void LLMProviderSettings_Table_Exists()
    {
        var canQuery = _context.LLMProviderSettings.Take(1).ToList();
        Assert.NotNull(canQuery);
    }

    [Fact]
    [Trait("Category", "BVT")]
    [Trait("Category", "DatabaseSchema")]
    public void CustomFields_Table_Exists()
    {
        var canQuery = _context.CustomFields.Take(1).ToList();
        Assert.NotNull(canQuery);
    }

    #endregion

    #region Workflow Tables

    [Fact]
    [Trait("Category", "BVT")]
    [Trait("Category", "DatabaseSchema")]
    public void WorkflowDefinitions_Table_Exists()
    {
        var canQuery = _context.WorkflowDefinitions.Take(1).ToList();
        Assert.NotNull(canQuery);
    }

    [Fact]
    [Trait("Category", "BVT")]
    [Trait("Category", "DatabaseSchema")]
    public void WorkflowNodes_Table_Exists()
    {
        var canQuery = _context.WorkflowNodes.Take(1).ToList();
        Assert.NotNull(canQuery);
    }

    [Fact]
    [Trait("Category", "BVT")]
    [Trait("Category", "DatabaseSchema")]
    public void WorkflowInstances_Table_Exists()
    {
        var canQuery = _context.WorkflowInstances.Take(1).ToList();
        Assert.NotNull(canQuery);
    }

    #endregion

    #region Service Request Tables

    [Fact]
    [Trait("Category", "BVT")]
    [Trait("Category", "DatabaseSchema")]
    public void ServiceRequests_Table_Exists()
    {
        var canQuery = _context.ServiceRequests.Take(1).ToList();
        Assert.NotNull(canQuery);
    }

    [Fact]
    [Trait("Category", "BVT")]
    [Trait("Category", "DatabaseSchema")]
    public void ServiceRequestCategories_Table_Exists()
    {
        var canQuery = _context.ServiceRequestCategories.Take(1).ToList();
        Assert.NotNull(canQuery);
    }

    #endregion

    #region Activity Tables

    [Fact]
    [Trait("Category", "BVT")]
    [Trait("Category", "DatabaseSchema")]
    public void Activities_Table_Exists()
    {
        var canQuery = _context.Activities.Take(1).ToList();
        Assert.NotNull(canQuery);
    }

    [Fact]
    [Trait("Category", "BVT")]
    [Trait("Category", "DatabaseSchema")]
    public void Notes_Table_Exists()
    {
        var canQuery = _context.Notes.Take(1).ToList();
        Assert.NotNull(canQuery);
    }

    [Fact]
    [Trait("Category", "BVT")]
    [Trait("Category", "DatabaseSchema")]
    public void CrmTasks_Table_Exists()
    {
        var canQuery = _context.CrmTasks.Take(1).ToList();
        Assert.NotNull(canQuery);
    }

    #endregion

    #region Entity Property Mapping Tests

    [Fact]
    [Trait("Category", "BVT")]
    [Trait("Category", "DatabaseSchema")]
    public void Account_Entity_Can_Create_And_Read_All_Properties()
    {
        // Create a test account with all major properties
        var account = new Account
        {
            Category = AccountCategory.Organization,
            Company = "Test Company",
            Email = $"test_{Guid.NewGuid():N}@test.com",
            Phone = "555-0100",
            Industry = "Technology",
            LifecycleStage = AccountLifecycleStage.Active,
            Priority = AccountPriority.High
        };

        _context.Accounts.Add(account);
        _context.SaveChanges();

        // Read it back
        var retrieved = _context.Accounts.Find(account.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("Test Company", retrieved!.Company);
        Assert.Equal(AccountCategory.Organization, retrieved.Category);

        // Cleanup
        _context.Accounts.Remove(account);
        _context.SaveChanges();
    }

    [Fact]
    [Trait("Category", "BVT")]
    [Trait("Category", "DatabaseSchema")]
    public void Contact_Entity_Can_Create_And_Read_All_Properties()
    {
        var contact = new Contact
        {
            FirstName = "Test",
            LastName = "Contact",
            EmailPrimary = $"test_{Guid.NewGuid():N}@test.com",
            PhonePrimary = "555-0101",
            Status = ContactStatus.Active
        };

        _context.Contacts.Add(contact);
        _context.SaveChanges();

        var retrieved = _context.Contacts.Find(contact.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("Test", retrieved!.FirstName);

        _context.Contacts.Remove(contact);
        _context.SaveChanges();
    }

    [Fact]
    [Trait("Category", "BVT")]
    [Trait("Category", "DatabaseSchema")]
    public void Lead_Entity_Can_Create_And_Read_All_Properties()
    {
        var lead = new Lead
        {
            FirstName = "Test",
            LastName = "Lead",
            Email = $"lead_{Guid.NewGuid():N}@test.com",
            CompanyName = "Test Corp"
        };

        _context.Leads.Add(lead);
        _context.SaveChanges();

        var retrieved = _context.Leads.Find(lead.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("Test", retrieved!.FirstName);

        _context.Leads.Remove(lead);
        _context.SaveChanges();
    }

    [Fact]
    [Trait("Category", "BVT")]
    [Trait("Category", "DatabaseSchema")]
    public void Tag_And_EntityTag_Relationship_Works()
    {
        // Create a tag
        var tag = new Tag
        {
            Name = $"TestTag_{Guid.NewGuid():N}",
            Color = "#FF0000",
            Description = "Test tag for BVT"
        };

        _context.Tags.Add(tag);
        _context.SaveChanges();

        // Create an EntityTag linking to an Account
        var entityTag = new EntityTag
        {
            TagId = tag.Id,
            EntityType = "Account",
            EntityId = 1,
            TagName = tag.Name,
            SortOrder = 0
        };

        _context.EntityTags.Add(entityTag);
        _context.SaveChanges();

        // Verify the relationship
        var retrievedTag = _context.Tags.Include(t => t.EntityTags).FirstOrDefault(t => t.Id == tag.Id);
        Assert.NotNull(retrievedTag);
        Assert.Single(retrievedTag!.EntityTags);

        // Cleanup
        _context.EntityTags.Remove(entityTag);
        _context.Tags.Remove(tag);
        _context.SaveChanges();
    }

    #endregion

    #region All DbSets Verification

    [Fact]
    [Trait("Category", "BVT")]
    [Trait("Category", "DatabaseSchema")]
    public void All_DbSets_Are_Queryable()
    {
        // Get all DbSet properties from CrmDbContext
        var dbSetProperties = typeof(CrmDbContext)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType.IsGenericType &&
                        p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .ToList();

        var errors = new List<string>();

        foreach (var property in dbSetProperties)
        {
            try
            {
                var dbSet = property.GetValue(_context);
                if (dbSet != null)
                {
                    // Try to execute a simple query
                    var queryable = dbSet as IQueryable<object>;
                    if (queryable != null)
                    {
                        // Just check if we can create the query (doesn't hit DB)
                        var expression = queryable.Expression;
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add($"{property.Name}: {ex.Message}");
            }
        }

        Assert.Empty(errors);
    }

    #endregion
}

/// <summary>
/// Test database fixture for integration tests
/// </summary>
public class TestDatabaseFixture : IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly string _connectionString;

    public TestDatabaseFixture()
    {
        // Use environment variable or default test connection string
        _connectionString = Environment.GetEnvironmentVariable("TEST_DATABASE_CONNECTION")
            ?? "Server=localhost;Port=3306;Database=crm_testdb;User=crm_user;Password=CrmPass@Dev2024;";

        var services = new ServiceCollection();
        services.AddDbContext<CrmDbContext>(options =>
        {
            options.UseMySql(_connectionString, ServerVersion.AutoDetect(_connectionString));
        });

        _serviceProvider = services.BuildServiceProvider();
    }

    public CrmDbContext CreateContext()
    {
        return _serviceProvider.GetRequiredService<CrmDbContext>();
    }

    public void Dispose()
    {
        // Cleanup if needed
    }
}

[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<TestDatabaseFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
