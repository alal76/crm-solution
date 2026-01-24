using CRM.Core.Entities;
using CRM.Core.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using SocialPlatform = CRM.Core.Models.SocialMediaPlatform;

namespace CRM.Infrastructure.Data;

/// <summary>
/// Database seeding for initial admin user and test data
/// </summary>
public class DbSeed
{
    /// <summary>
    /// Hash a password using BCrypt
    /// </summary>
    private static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    /// <summary>
    /// Seed initial admin user and sample data
    /// </summary>
    public static async Task SeedAsync(CrmDbContext context)
    {
        // Seed SysAdmin Group - always required for system administration
        var sysAdminGroup = await context.UserGroups.FirstOrDefaultAsync(g => g.Name == "SysAdmin");
        if (sysAdminGroup == null)
        {
            sysAdminGroup = new UserGroup
            {
                Name = "SysAdmin",
                Description = "System Administrators with full access to all features and settings",
                IsActive = true,
                IsDefault = false,
                IsSystemAdmin = true,
                DisplayOrder = 0,
                HeaderColor = "#DC2626", // Red for admin visibility
                // Menu/Page Access
                CanAccessDashboard = true,
                CanAccessCustomers = true,
                CanAccessContacts = true,
                CanAccessLeads = true,
                CanAccessOpportunities = true,
                CanAccessProducts = true,
                CanAccessServices = true,
                CanAccessCampaigns = true,
                CanAccessQuotes = true,
                CanAccessTasks = true,
                CanAccessActivities = true,
                CanAccessNotes = true,
                CanAccessWorkflows = true,
                CanAccessServiceRequests = true,
                CanAccessReports = true,
                CanAccessSettings = true,
                CanAccessUserManagement = true,
                // Customer CRUD
                CanCreateCustomers = true,
                CanEditCustomers = true,
                CanDeleteCustomers = true,
                CanViewAllCustomers = true,
                // Contact CRUD
                CanCreateContacts = true,
                CanEditContacts = true,
                CanDeleteContacts = true,
                // Lead CRUD
                CanCreateLeads = true,
                CanEditLeads = true,
                CanDeleteLeads = true,
                CanConvertLeads = true,
                // Opportunity CRUD
                CanCreateOpportunities = true,
                CanEditOpportunities = true,
                CanDeleteOpportunities = true,
                CanCloseOpportunities = true,
                // Product CRUD
                CanCreateProducts = true,
                CanEditProducts = true,
                CanDeleteProducts = true,
                CanManagePricing = true,
                // Campaign CRUD
                CanCreateCampaigns = true,
                CanEditCampaigns = true,
                CanDeleteCampaigns = true,
                CanLaunchCampaigns = true,
                // Quote CRUD
                CanCreateQuotes = true,
                CanEditQuotes = true,
                CanDeleteQuotes = true,
                CanApproveQuotes = true,
                // Task CRUD
                CanCreateTasks = true,
                CanEditTasks = true,
                CanDeleteTasks = true,
                CanAssignTasks = true,
                // Workflow CRUD
                CanCreateWorkflows = true,
                CanEditWorkflows = true,
                CanDeleteWorkflows = true,
                CanActivateWorkflows = true,
                // Data Access
                DataAccessScope = "all",
                CanExportData = true,
                CanImportData = true,
                CanBulkEdit = true,
                CanBulkDelete = true,
                AccessibleMenuItems = "[\"Dashboard\",\"Customers\",\"Contacts\",\"Leads\",\"Opportunities\",\"Products\",\"Services\",\"Campaigns\",\"Quotes\",\"Tasks\",\"Activities\",\"Notes\",\"Workflows\",\"ServiceRequests\",\"Reports\",\"Settings\",\"UserManagement\",\"Admin\"]"
            };
            context.UserGroups.Add(sysAdminGroup);
            await context.SaveChangesAsync();
        }

        // Seed Admin User
        var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "abhi.lal@gmail.com");
        if (adminUser == null)
        {
            adminUser = new User
            {
                Username = "admin",
                Email = "abhi.lal@gmail.com",
                FirstName = "Abhishek",
                LastName = "Lal",
                PasswordHash = HashPassword("Admin@123"), // Change this in production!
                Role = (int)UserRole.Admin,
                IsActive = true,
                EmailVerified = true,
                TwoFactorEnabled = false,
                PrimaryGroupId = sysAdminGroup.Id
            };

            context.Users.Add(adminUser);
            await context.SaveChangesAsync();
        }
        else if (adminUser.PrimaryGroupId != sysAdminGroup.Id)
        {
            // Ensure existing admin user has SysAdmin as primary group
            adminUser.PrimaryGroupId = sysAdminGroup.Id;
            await context.SaveChangesAsync();
        }

        // Ensure admin user is a member of SysAdmin group
        var isMember = await context.UserGroupMembers
            .AnyAsync(m => m.UserId == adminUser.Id && m.UserGroupId == sysAdminGroup.Id);
        
        if (!isMember)
        {
            var membership = new UserGroupMember
            {
                UserId = adminUser.Id,
                UserGroupId = sysAdminGroup.Id,
                AddedAt = DateTime.UtcNow
            };
            context.UserGroupMembers.Add(membership);
            await context.SaveChangesAsync();
        }

        // Seed sample customers if none exist
        if (!context.Customers.Any())
        {
            var customers = new List<Customer>
            {
                new Customer
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@example.com",
                    Phone = "+1-555-0001",
                    Company = "Tech Corp"
                },
                new Customer
                {
                    FirstName = "Jane",
                    LastName = "Smith",
                    Email = "jane.smith@example.com",
                    Phone = "+1-555-0002",
                    Company = "Innovation Inc"
                }
            };

            context.Customers.AddRange(customers);
            await context.SaveChangesAsync();
        }

        // Seed sample products if none exist
        if (!context.Products.Any())
        {
            var products = new List<Product>
            {
                new Product
                {
                    Name = "Premium Package",
                    SKU = "PKG-001",
                    Price = 999.99m,
                    Category = "Software",
                    Quantity = 50,
                    IsActive = true
                },
                new Product
                {
                    Name = "Standard Package",
                    SKU = "PKG-002",
                    Price = 499.99m,
                    Category = "Software",
                    Quantity = 100,
                    IsActive = true
                }
            };

            context.Products.AddRange(products);
            await context.SaveChangesAsync();
        }

        // Seed lookup master-data (currencies, billing cycles, preferred contact methods)
        if (!context.LookupCategories.Any())
        {
            var currencyCategory = new LookupCategory
            {
                Name = "Currency",
                Description = "Supported currencies",
                IsActive = true
            };

            var billingCycleCategory = new LookupCategory
            {
                Name = "BillingCycle",
                Description = "Billing frequency options",
                IsActive = true
            };

            var preferredContactCategory = new LookupCategory
            {
                Name = "PreferredContactMethod",
                Description = "Preferred contact method for contacts",
                IsActive = true
            };

            context.LookupCategories.AddRange(currencyCategory, billingCycleCategory, preferredContactCategory);
            await context.SaveChangesAsync();

            var currencies = new[] {
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "USD", Value = "US Dollar", SortOrder = 1, IsActive = true },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "EUR", Value = "Euro", SortOrder = 2, IsActive = true },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "GBP", Value = "British Pound", SortOrder = 3, IsActive = true }
            };

            var billingCycles = new[] {
                new LookupItem { LookupCategoryId = billingCycleCategory.Id, Key = "Monthly", Value = "Monthly", SortOrder = 1, IsActive = true },
                new LookupItem { LookupCategoryId = billingCycleCategory.Id, Key = "Quarterly", Value = "Quarterly", SortOrder = 2, IsActive = true },
                new LookupItem { LookupCategoryId = billingCycleCategory.Id, Key = "Yearly", Value = "Yearly", SortOrder = 3, IsActive = true }
            };

            var contactMethods = new[] {
                new LookupItem { LookupCategoryId = preferredContactCategory.Id, Key = "Email", Value = "Email", SortOrder = 1, IsActive = true },
                new LookupItem { LookupCategoryId = preferredContactCategory.Id, Key = "Phone", Value = "Phone", SortOrder = 2, IsActive = true },
                new LookupItem { LookupCategoryId = preferredContactCategory.Id, Key = "SMS", Value = "SMS", SortOrder = 3, IsActive = true }
            };

            context.LookupItems.AddRange(currencies);
            context.LookupItems.AddRange(billingCycles);
            context.LookupItems.AddRange(contactMethods);
            await context.SaveChangesAsync();
        }

        // Seed sample contacts if none exist
        if (!context.Contacts.Any())
        {
            var contacts = new List<Contact>
            {
                new Contact
                {
                    ContactType = ContactType.Employee,
                    FirstName = "Michael",
                    LastName = "Johnson",
                    MiddleName = "David",
                    EmailPrimary = "michael.johnson@company.com",
                    PhonePrimary = "+1-555-0101",
                    JobTitle = "Sales Manager",
                    Department = "Sales",
                    Company = "Tech Corp",
                    City = "San Francisco",
                    State = "CA",
                    Country = "USA",
                    ZipCode = "94105",
                    DateOfBirth = new DateTime(1985, 3, 15),
                    Notes = "Senior sales manager with 10+ years experience",
                    DateAdded = DateTime.UtcNow,
                    LastModified = DateTime.UtcNow,
                    ModifiedBy = "System",
                    SocialMediaLinks = new List<SocialMediaLink>
                    {
                        new SocialMediaLink
                        {
                            Platform = SocialPlatform.LinkedIn,
                            Url = "https://linkedin.com/in/michaeljohnson",
                            Handle = "michaeljohnson",
                            DateAdded = DateTime.UtcNow
                        },
                        new SocialMediaLink
                        {
                            Platform = SocialPlatform.Twitter,
                            Url = "https://twitter.com/mjohnson",
                            Handle = "mjohnson",
                            DateAdded = DateTime.UtcNow
                        }
                    }
                },
                new Contact
                {
                    ContactType = ContactType.Customer,
                    FirstName = "Sarah",
                    LastName = "Williams",
                    EmailPrimary = "sarah.williams@clientco.com",
                    PhonePrimary = "+1-555-0102",
                    JobTitle = "Procurement Director",
                    Department = "Procurement",
                    Company = "ClientCorp",
                    City = "New York",
                    State = "NY",
                    Country = "USA",
                    ZipCode = "10001",
                    DateAdded = DateTime.UtcNow,
                    LastModified = DateTime.UtcNow,
                    ModifiedBy = "System",
                    SocialMediaLinks = new List<SocialMediaLink>
                    {
                        new SocialMediaLink
                        {
                            Platform = SocialPlatform.LinkedIn,
                            Url = "https://linkedin.com/in/sarahwilliams",
                            Handle = "sarahwilliams",
                            DateAdded = DateTime.UtcNow
                        }
                    }
                },
                new Contact
                {
                    ContactType = ContactType.Partner,
                    FirstName = "Robert",
                    LastName = "Martinez",
                    EmailPrimary = "robert@partnerco.com",
                    PhonePrimary = "+1-555-0103",
                    JobTitle = "Partnership Manager",
                    Company = "PartnerCorp",
                    City = "Austin",
                    State = "TX",
                    Country = "USA",
                    ZipCode = "78701",
                    DateAdded = DateTime.UtcNow,
                    LastModified = DateTime.UtcNow,
                    ModifiedBy = "System",
                    SocialMediaLinks = new List<SocialMediaLink>
                    {
                        new SocialMediaLink
                        {
                            Platform = SocialPlatform.Website,
                            Url = "https://partnerco.com",
                            Handle = "PartnerCorp",
                            DateAdded = DateTime.UtcNow
                        }
                    }
                },
                new Contact
                {
                    ContactType = ContactType.Lead,
                    FirstName = "Emily",
                    LastName = "Chen",
                    EmailPrimary = "emily.chen@prospect.com",
                    PhonePrimary = "+1-555-0104",
                    JobTitle = "VP of Operations",
                    Company = "Prospect Inc",
                    City = "Seattle",
                    State = "WA",
                    Country = "USA",
                    ZipCode = "98101",
                    Notes = "Qualified lead - interested in enterprise solution",
                    DateAdded = DateTime.UtcNow,
                    LastModified = DateTime.UtcNow,
                    ModifiedBy = "System",
                    SocialMediaLinks = new List<SocialMediaLink>
                    {
                        new SocialMediaLink
                        {
                            Platform = SocialPlatform.LinkedIn,
                            Url = "https://linkedin.com/in/emilychen",
                            Handle = "emilychen",
                            DateAdded = DateTime.UtcNow
                        }
                    }
                },
                new Contact
                {
                    ContactType = ContactType.Employee,
                    FirstName = "David",
                    LastName = "Anderson",
                    EmailPrimary = "david.anderson@company.com",
                    EmailSecondary = "danderson@company.com",
                    PhonePrimary = "+1-555-0105",
                    JobTitle = "Account Executive",
                    Department = "Sales",
                    Company = "Tech Corp",
                    ReportsTo = "Michael Johnson",
                    City = "San Francisco",
                    State = "CA",
                    Country = "USA",
                    ZipCode = "94105",
                    DateOfBirth = new DateTime(1990, 7, 22),
                    DateAdded = DateTime.UtcNow,
                    LastModified = DateTime.UtcNow,
                    ModifiedBy = "System",
                    SocialMediaLinks = new List<SocialMediaLink>
                    {
                        new SocialMediaLink
                        {
                            Platform = SocialPlatform.GitHub,
                            Url = "https://github.com/danderson",
                            Handle = "danderson",
                            DateAdded = DateTime.UtcNow
                        }
                    }
                },
                new Contact
                {
                    ContactType = ContactType.Vendor,
                    FirstName = "Lisa",
                    LastName = "Thompson",
                    EmailPrimary = "lisa@softwarevendor.com",
                    PhonePrimary = "+1-555-0106",
                    JobTitle = "Account Manager",
                    Company = "Software Vendor Inc",
                    City = "Boston",
                    State = "MA",
                    Country = "USA",
                    ZipCode = "02101",
                    DateAdded = DateTime.UtcNow,
                    LastModified = DateTime.UtcNow,
                    ModifiedBy = "System",
                    SocialMediaLinks = new List<SocialMediaLink>
                    {
                        new SocialMediaLink
                        {
                            Platform = SocialPlatform.LinkedIn,
                            Url = "https://linkedin.com/in/lisathompson",
                            Handle = "lisathompson",
                            DateAdded = DateTime.UtcNow
                        },
                        new SocialMediaLink
                        {
                            Platform = SocialPlatform.Website,
                            Url = "https://softwarevendor.com",
                            Handle = "info",
                            DateAdded = DateTime.UtcNow
                        }
                    }
                }
            };

            context.Contacts.AddRange(contacts);
            await context.SaveChangesAsync();
        }

        // Ensure additional lookup categories/items required by the frontend
        var ensureLookups = new Dictionary<string, string[]>
        {
            { "Salutation", new[] { "Mr", "Mrs", "Ms", "Dr" } },
            { "Gender", new[] { "Male", "Female", "Other" } },
            { "LifecycleStage", new[] { "Prospect", "Qualified", "Customer", "Churned" } },
            { "LeadSource", new[] { "Website", "Email Campaign", "Referral", "Trade Show" } },
            { "LeadStatus", new[] { "New", "Contacted", "Qualified", "Unqualified" } },
            { "OpportunityStage", new[] { "Prospecting", "Negotiation", "ClosedWon", "ClosedLost" } },
            { "ProductCategory", new[] { "Software", "Service", "Hardware" } },
            { "ProductStatus", new[] { "Active", "Deprecated", "Draft" } },
            { "QuoteStatus", new[] { "Draft", "Sent", "Accepted", "Rejected" } },
            { "Priority", new[] { "Low", "Medium", "High", "Critical" } },
            { "Industry", new[] { "Technology", "Finance", "Healthcare", "Manufacturing" } },
            { "CustomerType", new[] { "Individual", "Company", "Government" } }
        };

        foreach (var kvp in ensureLookups)
        {
            var catName = kvp.Key;
            if (!context.LookupCategories.Any(c => c.Name == catName))
            {
                var cat = new LookupCategory
                {
                    Name = catName,
                    Description = catName + " values",
                    IsActive = true
                };
                context.LookupCategories.Add(cat);
                await context.SaveChangesAsync();

                var items = kvp.Value.Select((key, idx) => new LookupItem
                {
                    LookupCategoryId = cat.Id,
                    Key = key,
                    Value = key,
                    SortOrder = idx + 1,
                    IsActive = true
                }).ToArray();

                context.LookupItems.AddRange(items);
                await context.SaveChangesAsync();
            }
        }

        // Seed default system settings with navigation configuration
        await SeedSystemSettingsAsync(context);

        // Seed default module field configurations
        await SeedModuleFieldConfigurationsAsync(context);
    }

    /// <summary>
    /// Seed default system settings with navigation config and all items visible
    /// </summary>
    private static async Task SeedSystemSettingsAsync(CrmDbContext context)
    {
        var settings = await context.SystemSettings.FirstOrDefaultAsync();
        
        // Default navigation configuration with all items visible
        var defaultNavConfig = @"{
            ""navItems"":[{""id"":""dashboard"",""order"":0,""visible"":true,""category"":""main""},
                {""id"":""customers"",""order"":1,""visible"":true,""category"":""main""},
                {""id"":""customer-overview"",""order"":2,""visible"":true,""category"":""main""},
                {""id"":""contacts"",""order"":3,""visible"":true,""category"":""main""},
                {""id"":""leads"",""order"":4,""visible"":true,""category"":""sales""},
                {""id"":""opportunities"",""order"":5,""visible"":true,""category"":""sales""},
                {""id"":""products"",""order"":6,""visible"":true,""category"":""sales""},
                {""id"":""services"",""order"":7,""visible"":true,""category"":""support""},
                {""id"":""service-requests"",""order"":8,""visible"":true,""category"":""support""},
                {""id"":""campaigns"",""order"":9,""visible"":true,""category"":""sales""},
                {""id"":""quotes"",""order"":10,""visible"":true,""category"":""sales""},
                {""id"":""my-queue"",""order"":11,""visible"":true,""category"":""productivity""},
                {""id"":""activities"",""order"":12,""visible"":true,""category"":""productivity""},
                {""id"":""notes"",""order"":13,""visible"":true,""category"":""productivity""},
                {""id"":""communications"",""order"":14,""visible"":true,""category"":""productivity""},
                {""id"":""interactions"",""order"":15,""visible"":true,""category"":""productivity""},
                {""id"":""workflows"",""order"":16,""visible"":true,""category"":""admin""},
                {""id"":""channel-settings"",""order"":17,""visible"":true,""category"":""admin""},
                {""id"":""settings"",""order"":18,""visible"":true,""category"":""admin""}],
            ""categories"":[{""id"":""main"",""label"":""Main"",""order"":0},
                {""id"":""sales"",""label"":""Sales & Marketing"",""order"":1},
                {""id"":""support"",""label"":""Customer Support"",""order"":2},
                {""id"":""productivity"",""label"":""Productivity"",""order"":3},
                {""id"":""admin"",""label"":""Administration"",""order"":4}]
        }".Replace(" ", "").Replace("\n", "").Replace("\r", "");
        
        if (settings == null)
        {
            settings = new SystemSettings
            {
                NavOrderConfig = defaultNavConfig,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            };
            context.SystemSettings.Add(settings);
            await context.SaveChangesAsync();
        }
        else if (string.IsNullOrEmpty(settings.NavOrderConfig))
        {
            // Only update if no nav config exists
            settings.NavOrderConfig = defaultNavConfig;
            settings.UpdatedAt = DateTime.UtcNow;
            settings.LastModified = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Seed default module field configurations for all entities
    /// </summary>
    private static async Task SeedModuleFieldConfigurationsAsync(CrmDbContext context)
    {
        // Check if any configurations exist - if so, skip seeding
        if (await context.ModuleFieldConfigurations.AnyAsync())
        {
            return;
        }

        await SeedModuleFieldConfigurationsAsync(context, forceReseed: false);
    }

    /// <summary>
    /// Force reseed module field configurations - deletes existing configs and reseeds
    /// </summary>
    public static async Task ForceReseedModuleFieldConfigurationsAsync(CrmDbContext context)
    {
        await SeedModuleFieldConfigurationsAsync(context, forceReseed: true);
    }

    /// <summary>
    /// Seed default module field configurations for all entities with optional force reseed
    /// </summary>
    private static async Task SeedModuleFieldConfigurationsAsync(CrmDbContext context, bool forceReseed)
    {
        // If force reseed, delete all existing configurations first
        if (forceReseed)
        {
            var existingConfigs = await context.ModuleFieldConfigurations.ToListAsync();
            if (existingConfigs.Any())
            {
                context.ModuleFieldConfigurations.RemoveRange(existingConfigs);
                await context.SaveChangesAsync();
            }
        }
        else
        {
            // Check if any configurations exist - if so, skip seeding
            if (await context.ModuleFieldConfigurations.AnyAsync())
            {
                return;
            }
        }

        var now = DateTime.UtcNow;
        var configs = new List<ModuleFieldConfiguration>();

        // Add Customer field configurations
        configs.AddRange(GetDefaultCustomerFields(now));

        // Add Contact field configurations
        configs.AddRange(GetDefaultContactFields(now));

        // Add Lead field configurations
        configs.AddRange(GetDefaultLeadFields(now));

        // Add Opportunity field configurations
        configs.AddRange(GetDefaultOpportunityFields(now));

        // Add Product field configurations
        configs.AddRange(GetDefaultProductFields(now));

        // Add Campaign field configurations
        configs.AddRange(GetDefaultCampaignFields(now));

        // Add Quote field configurations
        configs.AddRange(GetDefaultQuoteFields(now));

        if (configs.Any())
        {
            context.ModuleFieldConfigurations.AddRange(configs);
            await context.SaveChangesAsync();
        }
    }

    private static List<ModuleFieldConfiguration> GetDefaultCustomerFields(DateTime now)
    {
        return new List<ModuleFieldConfiguration>
        {
            // Basic Info Tab (0)
            new() { ModuleName = "Customers", FieldName = "category", FieldLabel = "Customer Category", FieldType = "select", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 0, IsEnabled = true, IsRequired = true, GridSize = 12, Options = "Individual,Organization", IsReorderable = false, IsRequiredConfigurable = false, IsHideable = false, CreatedAt = now },
            
            // Individual fields
            new() { ModuleName = "Customers", FieldName = "salutation", FieldLabel = "Salutation", FieldType = "select", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 10, IsEnabled = true, IsRequired = false, GridSize = 2, Options = "Mr.,Mrs.,Ms.,Dr.,Prof.", ParentField = "category", ParentFieldValue = "0", CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "firstName", FieldLabel = "First Name", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 11, IsEnabled = true, IsRequired = true, GridSize = 4, ParentField = "category", ParentFieldValue = "0", CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "lastName", FieldLabel = "Last Name", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 12, IsEnabled = true, IsRequired = true, GridSize = 4, ParentField = "category", ParentFieldValue = "0", CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "suffix", FieldLabel = "Suffix", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 13, IsEnabled = true, IsRequired = false, GridSize = 2, Placeholder = "Jr., III", ParentField = "category", ParentFieldValue = "0", CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "dateOfBirth", FieldLabel = "Date of Birth", FieldType = "date", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 14, IsEnabled = true, IsRequired = false, GridSize = 6, ParentField = "category", ParentFieldValue = "0", CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "gender", FieldLabel = "Gender", FieldType = "select", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 15, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "Male,Female,Other,Prefer not to say", ParentField = "category", ParentFieldValue = "0", CreatedAt = now },
            
            // Organization fields
            new() { ModuleName = "Customers", FieldName = "company", FieldLabel = "Company Name", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 20, IsEnabled = true, IsRequired = true, GridSize = 6, ParentField = "category", ParentFieldValue = "1", CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "legalName", FieldLabel = "Legal Name", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 21, IsEnabled = true, IsRequired = false, GridSize = 6, Placeholder = "Full legal entity name", ParentField = "category", ParentFieldValue = "1", CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "dbaName", FieldLabel = "DBA Name", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 22, IsEnabled = true, IsRequired = false, GridSize = 6, Placeholder = "Doing Business As", ParentField = "category", ParentFieldValue = "1", CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "taxId", FieldLabel = "Tax ID / EIN", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 23, IsEnabled = true, IsRequired = false, GridSize = 6, ParentField = "category", ParentFieldValue = "1", CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "registrationNumber", FieldLabel = "Registration Number", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 24, IsEnabled = true, IsRequired = false, GridSize = 6, ParentField = "category", ParentFieldValue = "1", CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "yearFounded", FieldLabel = "Year Founded", FieldType = "number", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 25, IsEnabled = true, IsRequired = false, GridSize = 6, ParentField = "category", ParentFieldValue = "1", CreatedAt = now },
            
            // Contact Information (common)
            new() { ModuleName = "Customers", FieldName = "email", FieldLabel = "Email", FieldType = "email", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 30, IsEnabled = true, IsRequired = true, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "secondaryEmail", FieldLabel = "Secondary Email", FieldType = "email", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 31, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "phone", FieldLabel = "Phone", FieldType = "phone", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 32, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "mobilePhone", FieldLabel = "Mobile Phone", FieldType = "phone", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 33, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "jobTitle", FieldLabel = "Job Title", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 34, IsEnabled = true, IsRequired = false, GridSize = 6, ParentField = "category", ParentFieldValue = "0", CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "website", FieldLabel = "Website", FieldType = "url", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 35, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            
            // Address
            new() { ModuleName = "Customers", FieldName = "address", FieldLabel = "Address", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 40, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "city", FieldLabel = "City", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 41, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "state", FieldLabel = "State", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 42, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "zipCode", FieldLabel = "Zip Code", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 43, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            
            // Business Tab (1)
            new() { ModuleName = "Customers", FieldName = "customerType", FieldLabel = "Customer Type", FieldType = "select", TabIndex = 1, TabName = "Business", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "Individual,Small Business,Mid-Market,Enterprise,Government,Non-Profit", CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "lifecycleStage", FieldLabel = "Lifecycle Stage", FieldType = "select", TabIndex = 1, TabName = "Business", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "Lead,Prospect,Opportunity,Customer,Churned,Reactivated", CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "priority", FieldLabel = "Priority", FieldType = "select", TabIndex = 1, TabName = "Business", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "Low,Medium,High,Critical", CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "industry", FieldLabel = "Industry", FieldType = "select", TabIndex = 1, TabName = "Business", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "Technology,Healthcare,Finance,Retail,Manufacturing,Education,Real Estate,Consulting,Marketing,Legal,Non-Profit,Government,Other", CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "annualRevenue", FieldLabel = "Annual Revenue ($)", FieldType = "currency", TabIndex = 1, TabName = "Business", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "numberOfEmployees", FieldLabel = "Number of Employees", FieldType = "number", TabIndex = 1, TabName = "Business", DisplayOrder = 5, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "creditLimit", FieldLabel = "Credit Limit ($)", FieldType = "currency", TabIndex = 1, TabName = "Business", DisplayOrder = 6, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "leadSource", FieldLabel = "Lead Source", FieldType = "select", TabIndex = 1, TabName = "Business", DisplayOrder = 7, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "Website,Referral,Social Media,Cold Call,Trade Show,Advertisement,Email Campaign,Partner,Other", CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "leadScore", FieldLabel = "Lead Score", FieldType = "number", TabIndex = 1, TabName = "Business", DisplayOrder = 8, IsEnabled = true, IsRequired = false, GridSize = 12, HelpText = "Lead score from 0-100", CreatedAt = now },
            
            // Contact Preferences Tab (2)
            new() { ModuleName = "Customers", FieldName = "preferredContactMethod", FieldLabel = "Preferred Contact Method", FieldType = "select", TabIndex = 2, TabName = "Contact Preferences", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "Email,Phone,SMS,Mail", CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "timezone", FieldLabel = "Timezone", FieldType = "text", TabIndex = 2, TabName = "Contact Preferences", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 6, Placeholder = "e.g., America/New_York", CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "optInEmail", FieldLabel = "Email Communications", FieldType = "checkbox", TabIndex = 2, TabName = "Contact Preferences", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "optInPhone", FieldLabel = "Phone Calls", FieldType = "checkbox", TabIndex = 2, TabName = "Contact Preferences", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "optInSms", FieldLabel = "SMS Messages", FieldType = "checkbox", TabIndex = 2, TabName = "Contact Preferences", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "linkedInUrl", FieldLabel = "LinkedIn URL", FieldType = "url", TabIndex = 2, TabName = "Contact Preferences", DisplayOrder = 5, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "twitterHandle", FieldLabel = "Twitter Handle", FieldType = "text", TabIndex = 2, TabName = "Contact Preferences", DisplayOrder = 6, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            
            // Additional Tab (3)
            new() { ModuleName = "Customers", FieldName = "territory", FieldLabel = "Territory", FieldType = "text", TabIndex = 3, TabName = "Additional", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "paymentTerms", FieldLabel = "Payment Terms", FieldType = "text", TabIndex = 3, TabName = "Additional", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 6, Placeholder = "e.g., Net 30", CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "tags", FieldLabel = "Tags (comma-separated)", FieldType = "text", TabIndex = 3, TabName = "Additional", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 12, Placeholder = "vip, enterprise, priority", CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "description", FieldLabel = "Description", FieldType = "textarea", TabIndex = 3, TabName = "Additional", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "notes", FieldLabel = "Notes", FieldType = "textarea", TabIndex = 3, TabName = "Additional", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
        };
    }

    private static List<ModuleFieldConfiguration> GetDefaultContactFields(DateTime now)
    {
        return new List<ModuleFieldConfiguration>
        {
            // Basic Info Tab (0)
            new() { ModuleName = "Contacts", FieldName = "contactType", FieldLabel = "Contact Type", FieldType = "select", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 0, IsEnabled = true, IsRequired = true, GridSize = 6, Options = "Employee,Customer,Partner,Lead,Vendor,Other", CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "salutation", FieldLabel = "Salutation", FieldType = "select", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 2, Options = "Mr.,Mrs.,Ms.,Dr.,Prof.", CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "firstName", FieldLabel = "First Name", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 2, IsEnabled = true, IsRequired = true, GridSize = 4, CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "middleName", FieldLabel = "Middle Name", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 3, CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "lastName", FieldLabel = "Last Name", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 4, IsEnabled = true, IsRequired = true, GridSize = 3, CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "emailPrimary", FieldLabel = "Primary Email", FieldType = "email", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 5, IsEnabled = true, IsRequired = true, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "emailSecondary", FieldLabel = "Secondary Email", FieldType = "email", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 6, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "phonePrimary", FieldLabel = "Primary Phone", FieldType = "phone", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 7, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "phoneSecondary", FieldLabel = "Secondary Phone", FieldType = "phone", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 8, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            
            // Work Info Tab (1)
            new() { ModuleName = "Contacts", FieldName = "company", FieldLabel = "Company", FieldType = "text", TabIndex = 1, TabName = "Work Info", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "jobTitle", FieldLabel = "Job Title", FieldType = "text", TabIndex = 1, TabName = "Work Info", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "department", FieldLabel = "Department", FieldType = "text", TabIndex = 1, TabName = "Work Info", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "reportsTo", FieldLabel = "Reports To", FieldType = "text", TabIndex = 1, TabName = "Work Info", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            
            // Address Tab (2)
            new() { ModuleName = "Contacts", FieldName = "addressLine1", FieldLabel = "Address Line 1", FieldType = "text", TabIndex = 2, TabName = "Address", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "addressLine2", FieldLabel = "Address Line 2", FieldType = "text", TabIndex = 2, TabName = "Address", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "city", FieldLabel = "City", FieldType = "text", TabIndex = 2, TabName = "Address", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "state", FieldLabel = "State/Province", FieldType = "text", TabIndex = 2, TabName = "Address", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "zipCode", FieldLabel = "Zip/Postal Code", FieldType = "text", TabIndex = 2, TabName = "Address", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "country", FieldLabel = "Country", FieldType = "text", TabIndex = 2, TabName = "Address", DisplayOrder = 5, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            
            // Additional Tab (3)
            new() { ModuleName = "Contacts", FieldName = "dateOfBirth", FieldLabel = "Date of Birth", FieldType = "date", TabIndex = 3, TabName = "Additional", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "notes", FieldLabel = "Notes", FieldType = "textarea", TabIndex = 3, TabName = "Additional", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
        };
    }

    private static List<ModuleFieldConfiguration> GetDefaultLeadFields(DateTime now)
    {
        return new List<ModuleFieldConfiguration>
        {
            // Basic Info Tab (0)
            new() { ModuleName = "Leads", FieldName = "firstName", FieldLabel = "First Name", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 0, IsEnabled = true, IsRequired = true, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Leads", FieldName = "lastName", FieldLabel = "Last Name", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 1, IsEnabled = true, IsRequired = true, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Leads", FieldName = "email", FieldLabel = "Email", FieldType = "email", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 2, IsEnabled = true, IsRequired = true, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Leads", FieldName = "phone", FieldLabel = "Phone", FieldType = "phone", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Leads", FieldName = "company", FieldLabel = "Company", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Leads", FieldName = "jobTitle", FieldLabel = "Job Title", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 5, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            
            // Lead Details Tab (1)
            new() { ModuleName = "Leads", FieldName = "status", FieldLabel = "Status", FieldType = "select", TabIndex = 1, TabName = "Lead Details", DisplayOrder = 0, IsEnabled = true, IsRequired = true, GridSize = 6, Options = "New,Contacted,Qualified,Unqualified,Converted", CreatedAt = now },
            new() { ModuleName = "Leads", FieldName = "source", FieldLabel = "Lead Source", FieldType = "select", TabIndex = 1, TabName = "Lead Details", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "Website,Referral,Social Media,Cold Call,Trade Show,Advertisement,Partner,Other", CreatedAt = now },
            new() { ModuleName = "Leads", FieldName = "rating", FieldLabel = "Rating", FieldType = "select", TabIndex = 1, TabName = "Lead Details", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "Hot,Warm,Cold", CreatedAt = now },
            new() { ModuleName = "Leads", FieldName = "score", FieldLabel = "Lead Score", FieldType = "number", TabIndex = 1, TabName = "Lead Details", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 6, HelpText = "Score from 0-100", CreatedAt = now },
            new() { ModuleName = "Leads", FieldName = "estimatedValue", FieldLabel = "Estimated Value ($)", FieldType = "currency", TabIndex = 1, TabName = "Lead Details", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Leads", FieldName = "industry", FieldLabel = "Industry", FieldType = "select", TabIndex = 1, TabName = "Lead Details", DisplayOrder = 5, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "Technology,Healthcare,Finance,Retail,Manufacturing,Education,Other", CreatedAt = now },
            
            // Address Tab (2)
            new() { ModuleName = "Leads", FieldName = "address", FieldLabel = "Address", FieldType = "text", TabIndex = 2, TabName = "Address", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = "Leads", FieldName = "city", FieldLabel = "City", FieldType = "text", TabIndex = 2, TabName = "Address", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = "Leads", FieldName = "state", FieldLabel = "State", FieldType = "text", TabIndex = 2, TabName = "Address", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = "Leads", FieldName = "zipCode", FieldLabel = "Zip Code", FieldType = "text", TabIndex = 2, TabName = "Address", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = "Leads", FieldName = "country", FieldLabel = "Country", FieldType = "text", TabIndex = 2, TabName = "Address", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            
            // Notes Tab (3)
            new() { ModuleName = "Leads", FieldName = "description", FieldLabel = "Description", FieldType = "textarea", TabIndex = 3, TabName = "Notes", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = "Leads", FieldName = "notes", FieldLabel = "Notes", FieldType = "textarea", TabIndex = 3, TabName = "Notes", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
        };
    }

    private static List<ModuleFieldConfiguration> GetDefaultOpportunityFields(DateTime now)
    {
        return new List<ModuleFieldConfiguration>
        {
            // Basic Info Tab (0)
            new() { ModuleName = "Opportunities", FieldName = "title", FieldLabel = "Opportunity Title", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 0, IsEnabled = true, IsRequired = true, GridSize = 12, CreatedAt = now },
            new() { ModuleName = "Opportunities", FieldName = "customerId", FieldLabel = "Customer", FieldType = "lookup", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 1, IsEnabled = true, IsRequired = true, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Opportunities", FieldName = "contactId", FieldLabel = "Primary Contact", FieldType = "lookup", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            
            // Opportunity Details Tab (1)
            new() { ModuleName = "Opportunities", FieldName = "stage", FieldLabel = "Stage", FieldType = "select", TabIndex = 1, TabName = "Details", DisplayOrder = 0, IsEnabled = true, IsRequired = true, GridSize = 6, Options = "Prospecting,Qualification,Proposal,Negotiation,Closed Won,Closed Lost", CreatedAt = now },
            new() { ModuleName = "Opportunities", FieldName = "probability", FieldLabel = "Probability (%)", FieldType = "number", TabIndex = 1, TabName = "Details", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 6, HelpText = "Win probability 0-100%", CreatedAt = now },
            new() { ModuleName = "Opportunities", FieldName = "amount", FieldLabel = "Amount ($)", FieldType = "currency", TabIndex = 1, TabName = "Details", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Opportunities", FieldName = "expectedCloseDate", FieldLabel = "Expected Close Date", FieldType = "date", TabIndex = 1, TabName = "Details", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Opportunities", FieldName = "source", FieldLabel = "Lead Source", FieldType = "select", TabIndex = 1, TabName = "Details", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "Website,Referral,Social Media,Cold Call,Trade Show,Partner,Other", CreatedAt = now },
            new() { ModuleName = "Opportunities", FieldName = "type", FieldLabel = "Type", FieldType = "select", TabIndex = 1, TabName = "Details", DisplayOrder = 5, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "New Business,Existing Business,Upsell,Renewal", CreatedAt = now },
            
            // Notes Tab (2)
            new() { ModuleName = "Opportunities", FieldName = "description", FieldLabel = "Description", FieldType = "textarea", TabIndex = 2, TabName = "Notes", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = "Opportunities", FieldName = "nextSteps", FieldLabel = "Next Steps", FieldType = "textarea", TabIndex = 2, TabName = "Notes", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = "Opportunities", FieldName = "competitorInfo", FieldLabel = "Competitor Information", FieldType = "textarea", TabIndex = 2, TabName = "Notes", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
        };
    }

    private static List<ModuleFieldConfiguration> GetDefaultProductFields(DateTime now)
    {
        return new List<ModuleFieldConfiguration>
        {
            // Basic Info Tab (0)
            new() { ModuleName = "Products", FieldName = "name", FieldLabel = "Product Name", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 0, IsEnabled = true, IsRequired = true, GridSize = 8, CreatedAt = now },
            new() { ModuleName = "Products", FieldName = "sku", FieldLabel = "SKU", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 1, IsEnabled = true, IsRequired = true, GridSize = 4, CreatedAt = now },
            new() { ModuleName = "Products", FieldName = "category", FieldLabel = "Category", FieldType = "select", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "Software,Hardware,Service,Subscription,Other", CreatedAt = now },
            new() { ModuleName = "Products", FieldName = "isActive", FieldLabel = "Active", FieldType = "checkbox", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            
            // Pricing Tab (1)
            new() { ModuleName = "Products", FieldName = "price", FieldLabel = "Price ($)", FieldType = "currency", TabIndex = 1, TabName = "Pricing", DisplayOrder = 0, IsEnabled = true, IsRequired = true, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Products", FieldName = "cost", FieldLabel = "Cost ($)", FieldType = "currency", TabIndex = 1, TabName = "Pricing", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Products", FieldName = "quantity", FieldLabel = "Quantity in Stock", FieldType = "number", TabIndex = 1, TabName = "Pricing", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Products", FieldName = "reorderLevel", FieldLabel = "Reorder Level", FieldType = "number", TabIndex = 1, TabName = "Pricing", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            
            // Details Tab (2)
            new() { ModuleName = "Products", FieldName = "description", FieldLabel = "Description", FieldType = "textarea", TabIndex = 2, TabName = "Details", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = "Products", FieldName = "features", FieldLabel = "Features", FieldType = "textarea", TabIndex = 2, TabName = "Details", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
        };
    }

    private static List<ModuleFieldConfiguration> GetDefaultCampaignFields(DateTime now)
    {
        return new List<ModuleFieldConfiguration>
        {
            // Basic Info Tab (0)
            new() { ModuleName = "Campaigns", FieldName = "name", FieldLabel = "Campaign Name", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 0, IsEnabled = true, IsRequired = true, GridSize = 12, CreatedAt = now },
            new() { ModuleName = "Campaigns", FieldName = "type", FieldLabel = "Campaign Type", FieldType = "select", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 1, IsEnabled = true, IsRequired = true, GridSize = 6, Options = "Email,Social Media,Event,Webinar,Advertising,Referral,Other", CreatedAt = now },
            new() { ModuleName = "Campaigns", FieldName = "status", FieldLabel = "Status", FieldType = "select", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 2, IsEnabled = true, IsRequired = true, GridSize = 6, Options = "Planning,Active,Paused,Completed,Cancelled", CreatedAt = now },
            new() { ModuleName = "Campaigns", FieldName = "startDate", FieldLabel = "Start Date", FieldType = "date", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Campaigns", FieldName = "endDate", FieldLabel = "End Date", FieldType = "date", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            
            // Budget Tab (1)
            new() { ModuleName = "Campaigns", FieldName = "budget", FieldLabel = "Budget ($)", FieldType = "currency", TabIndex = 1, TabName = "Budget", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Campaigns", FieldName = "actualCost", FieldLabel = "Actual Cost ($)", FieldType = "currency", TabIndex = 1, TabName = "Budget", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Campaigns", FieldName = "expectedRevenue", FieldLabel = "Expected Revenue ($)", FieldType = "currency", TabIndex = 1, TabName = "Budget", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Campaigns", FieldName = "expectedResponse", FieldLabel = "Expected Response (%)", FieldType = "number", TabIndex = 1, TabName = "Budget", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            
            // Details Tab (2)
            new() { ModuleName = "Campaigns", FieldName = "description", FieldLabel = "Description", FieldType = "textarea", TabIndex = 2, TabName = "Details", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = "Campaigns", FieldName = "objectives", FieldLabel = "Objectives", FieldType = "textarea", TabIndex = 2, TabName = "Details", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
        };
    }

    private static List<ModuleFieldConfiguration> GetDefaultQuoteFields(DateTime now)
    {
        return new List<ModuleFieldConfiguration>
        {
            // Basic Info Tab (0)
            new() { ModuleName = "Quotes", FieldName = "quoteNumber", FieldLabel = "Quote Number", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 0, IsEnabled = true, IsRequired = true, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Quotes", FieldName = "customerId", FieldLabel = "Customer", FieldType = "lookup", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 1, IsEnabled = true, IsRequired = true, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Quotes", FieldName = "status", FieldLabel = "Status", FieldType = "select", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 2, IsEnabled = true, IsRequired = true, GridSize = 6, Options = "Draft,Sent,Accepted,Rejected,Expired", CreatedAt = now },
            new() { ModuleName = "Quotes", FieldName = "validUntil", FieldLabel = "Valid Until", FieldType = "date", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            
            // Pricing Tab (1)
            new() { ModuleName = "Quotes", FieldName = "subtotal", FieldLabel = "Subtotal ($)", FieldType = "currency", TabIndex = 1, TabName = "Pricing", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = "Quotes", FieldName = "discount", FieldLabel = "Discount ($)", FieldType = "currency", TabIndex = 1, TabName = "Pricing", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = "Quotes", FieldName = "tax", FieldLabel = "Tax ($)", FieldType = "currency", TabIndex = 1, TabName = "Pricing", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = "Quotes", FieldName = "total", FieldLabel = "Total ($)", FieldType = "currency", TabIndex = 1, TabName = "Pricing", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            
            // Terms Tab (2)
            new() { ModuleName = "Quotes", FieldName = "terms", FieldLabel = "Terms & Conditions", FieldType = "textarea", TabIndex = 2, TabName = "Terms", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = "Quotes", FieldName = "notes", FieldLabel = "Notes", FieldType = "textarea", TabIndex = 2, TabName = "Terms", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
        };    }
}