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

            // Complete list of ISO 4217 currencies
            var currencies = new[] {
                // Major currencies (sorted first for common use)
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "USD", Value = "US Dollar ($)", SortOrder = 1, IsActive = true, Meta = "{\"symbol\":\"$\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "EUR", Value = "Euro (€)", SortOrder = 2, IsActive = true, Meta = "{\"symbol\":\"€\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "GBP", Value = "British Pound (£)", SortOrder = 3, IsActive = true, Meta = "{\"symbol\":\"£\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "JPY", Value = "Japanese Yen (¥)", SortOrder = 4, IsActive = true, Meta = "{\"symbol\":\"¥\",\"decimal\":0}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "CHF", Value = "Swiss Franc (CHF)", SortOrder = 5, IsActive = true, Meta = "{\"symbol\":\"CHF\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "CAD", Value = "Canadian Dollar (C$)", SortOrder = 6, IsActive = true, Meta = "{\"symbol\":\"C$\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "AUD", Value = "Australian Dollar (A$)", SortOrder = 7, IsActive = true, Meta = "{\"symbol\":\"A$\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "CNY", Value = "Chinese Yuan (¥)", SortOrder = 8, IsActive = true, Meta = "{\"symbol\":\"¥\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "INR", Value = "Indian Rupee (₹)", SortOrder = 9, IsActive = true, Meta = "{\"symbol\":\"₹\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "NZD", Value = "New Zealand Dollar (NZ$)", SortOrder = 10, IsActive = true, Meta = "{\"symbol\":\"NZ$\",\"decimal\":2}" },
                // A-Z currencies
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "AED", Value = "UAE Dirham (د.إ)", SortOrder = 11, IsActive = true, Meta = "{\"symbol\":\"د.إ\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "AFN", Value = "Afghan Afghani (؋)", SortOrder = 12, IsActive = true, Meta = "{\"symbol\":\"؋\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "ALL", Value = "Albanian Lek (L)", SortOrder = 13, IsActive = true, Meta = "{\"symbol\":\"L\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "AMD", Value = "Armenian Dram (֏)", SortOrder = 14, IsActive = true, Meta = "{\"symbol\":\"֏\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "ANG", Value = "Netherlands Antillean Guilder (ƒ)", SortOrder = 15, IsActive = true, Meta = "{\"symbol\":\"ƒ\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "AOA", Value = "Angolan Kwanza (Kz)", SortOrder = 16, IsActive = true, Meta = "{\"symbol\":\"Kz\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "ARS", Value = "Argentine Peso ($)", SortOrder = 17, IsActive = true, Meta = "{\"symbol\":\"$\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "AWG", Value = "Aruban Florin (ƒ)", SortOrder = 18, IsActive = true, Meta = "{\"symbol\":\"ƒ\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "AZN", Value = "Azerbaijani Manat (₼)", SortOrder = 19, IsActive = true, Meta = "{\"symbol\":\"₼\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "BAM", Value = "Bosnia-Herzegovina Convertible Mark (KM)", SortOrder = 20, IsActive = true, Meta = "{\"symbol\":\"KM\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "BBD", Value = "Barbadian Dollar ($)", SortOrder = 21, IsActive = true, Meta = "{\"symbol\":\"$\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "BDT", Value = "Bangladeshi Taka (৳)", SortOrder = 22, IsActive = true, Meta = "{\"symbol\":\"৳\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "BGN", Value = "Bulgarian Lev (лв)", SortOrder = 23, IsActive = true, Meta = "{\"symbol\":\"лв\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "BHD", Value = "Bahraini Dinar (BD)", SortOrder = 24, IsActive = true, Meta = "{\"symbol\":\"BD\",\"decimal\":3}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "BIF", Value = "Burundian Franc (FBu)", SortOrder = 25, IsActive = true, Meta = "{\"symbol\":\"FBu\",\"decimal\":0}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "BMD", Value = "Bermudan Dollar ($)", SortOrder = 26, IsActive = true, Meta = "{\"symbol\":\"$\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "BND", Value = "Brunei Dollar ($)", SortOrder = 27, IsActive = true, Meta = "{\"symbol\":\"$\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "BOB", Value = "Bolivian Boliviano (Bs)", SortOrder = 28, IsActive = true, Meta = "{\"symbol\":\"Bs\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "BRL", Value = "Brazilian Real (R$)", SortOrder = 29, IsActive = true, Meta = "{\"symbol\":\"R$\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "BSD", Value = "Bahamian Dollar ($)", SortOrder = 30, IsActive = true, Meta = "{\"symbol\":\"$\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "BTN", Value = "Bhutanese Ngultrum (Nu)", SortOrder = 31, IsActive = true, Meta = "{\"symbol\":\"Nu\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "BWP", Value = "Botswanan Pula (P)", SortOrder = 32, IsActive = true, Meta = "{\"symbol\":\"P\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "BYN", Value = "Belarusian Ruble (Br)", SortOrder = 33, IsActive = true, Meta = "{\"symbol\":\"Br\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "BZD", Value = "Belize Dollar ($)", SortOrder = 34, IsActive = true, Meta = "{\"symbol\":\"$\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "CDF", Value = "Congolese Franc (FC)", SortOrder = 35, IsActive = true, Meta = "{\"symbol\":\"FC\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "CLP", Value = "Chilean Peso ($)", SortOrder = 36, IsActive = true, Meta = "{\"symbol\":\"$\",\"decimal\":0}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "COP", Value = "Colombian Peso ($)", SortOrder = 37, IsActive = true, Meta = "{\"symbol\":\"$\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "CRC", Value = "Costa Rican Colón (₡)", SortOrder = 38, IsActive = true, Meta = "{\"symbol\":\"₡\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "CUP", Value = "Cuban Peso ($)", SortOrder = 39, IsActive = true, Meta = "{\"symbol\":\"$\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "CVE", Value = "Cape Verdean Escudo ($)", SortOrder = 40, IsActive = true, Meta = "{\"symbol\":\"$\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "CZK", Value = "Czech Koruna (Kč)", SortOrder = 41, IsActive = true, Meta = "{\"symbol\":\"Kč\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "DJF", Value = "Djiboutian Franc (Fdj)", SortOrder = 42, IsActive = true, Meta = "{\"symbol\":\"Fdj\",\"decimal\":0}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "DKK", Value = "Danish Krone (kr)", SortOrder = 43, IsActive = true, Meta = "{\"symbol\":\"kr\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "DOP", Value = "Dominican Peso ($)", SortOrder = 44, IsActive = true, Meta = "{\"symbol\":\"$\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "DZD", Value = "Algerian Dinar (د.ج)", SortOrder = 45, IsActive = true, Meta = "{\"symbol\":\"د.ج\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "EGP", Value = "Egyptian Pound (E£)", SortOrder = 46, IsActive = true, Meta = "{\"symbol\":\"E£\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "ERN", Value = "Eritrean Nakfa (Nfk)", SortOrder = 47, IsActive = true, Meta = "{\"symbol\":\"Nfk\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "ETB", Value = "Ethiopian Birr (Br)", SortOrder = 48, IsActive = true, Meta = "{\"symbol\":\"Br\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "FJD", Value = "Fijian Dollar ($)", SortOrder = 49, IsActive = true, Meta = "{\"symbol\":\"$\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "FKP", Value = "Falkland Islands Pound (£)", SortOrder = 50, IsActive = true, Meta = "{\"symbol\":\"£\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "GEL", Value = "Georgian Lari (₾)", SortOrder = 51, IsActive = true, Meta = "{\"symbol\":\"₾\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "GHS", Value = "Ghanaian Cedi (₵)", SortOrder = 52, IsActive = true, Meta = "{\"symbol\":\"₵\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "GIP", Value = "Gibraltar Pound (£)", SortOrder = 53, IsActive = true, Meta = "{\"symbol\":\"£\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "GMD", Value = "Gambian Dalasi (D)", SortOrder = 54, IsActive = true, Meta = "{\"symbol\":\"D\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "GNF", Value = "Guinean Franc (FG)", SortOrder = 55, IsActive = true, Meta = "{\"symbol\":\"FG\",\"decimal\":0}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "GTQ", Value = "Guatemalan Quetzal (Q)", SortOrder = 56, IsActive = true, Meta = "{\"symbol\":\"Q\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "GYD", Value = "Guyanaese Dollar ($)", SortOrder = 57, IsActive = true, Meta = "{\"symbol\":\"$\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "HKD", Value = "Hong Kong Dollar (HK$)", SortOrder = 58, IsActive = true, Meta = "{\"symbol\":\"HK$\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "HNL", Value = "Honduran Lempira (L)", SortOrder = 59, IsActive = true, Meta = "{\"symbol\":\"L\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "HRK", Value = "Croatian Kuna (kn)", SortOrder = 60, IsActive = true, Meta = "{\"symbol\":\"kn\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "HTG", Value = "Haitian Gourde (G)", SortOrder = 61, IsActive = true, Meta = "{\"symbol\":\"G\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "HUF", Value = "Hungarian Forint (Ft)", SortOrder = 62, IsActive = true, Meta = "{\"symbol\":\"Ft\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "IDR", Value = "Indonesian Rupiah (Rp)", SortOrder = 63, IsActive = true, Meta = "{\"symbol\":\"Rp\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "ILS", Value = "Israeli New Shekel (₪)", SortOrder = 64, IsActive = true, Meta = "{\"symbol\":\"₪\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "IQD", Value = "Iraqi Dinar (ع.د)", SortOrder = 65, IsActive = true, Meta = "{\"symbol\":\"ع.د\",\"decimal\":3}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "IRR", Value = "Iranian Rial (﷼)", SortOrder = 66, IsActive = true, Meta = "{\"symbol\":\"﷼\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "ISK", Value = "Icelandic Króna (kr)", SortOrder = 67, IsActive = true, Meta = "{\"symbol\":\"kr\",\"decimal\":0}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "JMD", Value = "Jamaican Dollar ($)", SortOrder = 68, IsActive = true, Meta = "{\"symbol\":\"$\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "JOD", Value = "Jordanian Dinar (JD)", SortOrder = 69, IsActive = true, Meta = "{\"symbol\":\"JD\",\"decimal\":3}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "KES", Value = "Kenyan Shilling (KSh)", SortOrder = 70, IsActive = true, Meta = "{\"symbol\":\"KSh\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "KGS", Value = "Kyrgystani Som (с)", SortOrder = 71, IsActive = true, Meta = "{\"symbol\":\"с\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "KHR", Value = "Cambodian Riel (៛)", SortOrder = 72, IsActive = true, Meta = "{\"symbol\":\"៛\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "KMF", Value = "Comorian Franc (CF)", SortOrder = 73, IsActive = true, Meta = "{\"symbol\":\"CF\",\"decimal\":0}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "KPW", Value = "North Korean Won (₩)", SortOrder = 74, IsActive = true, Meta = "{\"symbol\":\"₩\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "KRW", Value = "South Korean Won (₩)", SortOrder = 75, IsActive = true, Meta = "{\"symbol\":\"₩\",\"decimal\":0}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "KWD", Value = "Kuwaiti Dinar (KD)", SortOrder = 76, IsActive = true, Meta = "{\"symbol\":\"KD\",\"decimal\":3}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "KYD", Value = "Cayman Islands Dollar ($)", SortOrder = 77, IsActive = true, Meta = "{\"symbol\":\"$\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "KZT", Value = "Kazakhstani Tenge (₸)", SortOrder = 78, IsActive = true, Meta = "{\"symbol\":\"₸\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "LAK", Value = "Laotian Kip (₭)", SortOrder = 79, IsActive = true, Meta = "{\"symbol\":\"₭\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "LBP", Value = "Lebanese Pound (ل.ل)", SortOrder = 80, IsActive = true, Meta = "{\"symbol\":\"ل.ل\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "LKR", Value = "Sri Lankan Rupee (Rs)", SortOrder = 81, IsActive = true, Meta = "{\"symbol\":\"Rs\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "LRD", Value = "Liberian Dollar ($)", SortOrder = 82, IsActive = true, Meta = "{\"symbol\":\"$\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "LSL", Value = "Lesotho Loti (L)", SortOrder = 83, IsActive = true, Meta = "{\"symbol\":\"L\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "LYD", Value = "Libyan Dinar (ل.د)", SortOrder = 84, IsActive = true, Meta = "{\"symbol\":\"ل.د\",\"decimal\":3}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "MAD", Value = "Moroccan Dirham (د.م.)", SortOrder = 85, IsActive = true, Meta = "{\"symbol\":\"د.م.\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "MDL", Value = "Moldovan Leu (L)", SortOrder = 86, IsActive = true, Meta = "{\"symbol\":\"L\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "MGA", Value = "Malagasy Ariary (Ar)", SortOrder = 87, IsActive = true, Meta = "{\"symbol\":\"Ar\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "MKD", Value = "Macedonian Denar (ден)", SortOrder = 88, IsActive = true, Meta = "{\"symbol\":\"ден\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "MMK", Value = "Myanmar Kyat (K)", SortOrder = 89, IsActive = true, Meta = "{\"symbol\":\"K\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "MNT", Value = "Mongolian Tugrik (₮)", SortOrder = 90, IsActive = true, Meta = "{\"symbol\":\"₮\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "MOP", Value = "Macanese Pataca (MOP$)", SortOrder = 91, IsActive = true, Meta = "{\"symbol\":\"MOP$\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "MRU", Value = "Mauritanian Ouguiya (UM)", SortOrder = 92, IsActive = true, Meta = "{\"symbol\":\"UM\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "MUR", Value = "Mauritian Rupee (Rs)", SortOrder = 93, IsActive = true, Meta = "{\"symbol\":\"Rs\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "MVR", Value = "Maldivian Rufiyaa (Rf)", SortOrder = 94, IsActive = true, Meta = "{\"symbol\":\"Rf\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "MWK", Value = "Malawian Kwacha (MK)", SortOrder = 95, IsActive = true, Meta = "{\"symbol\":\"MK\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "MXN", Value = "Mexican Peso ($)", SortOrder = 96, IsActive = true, Meta = "{\"symbol\":\"$\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "MYR", Value = "Malaysian Ringgit (RM)", SortOrder = 97, IsActive = true, Meta = "{\"symbol\":\"RM\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "MZN", Value = "Mozambican Metical (MT)", SortOrder = 98, IsActive = true, Meta = "{\"symbol\":\"MT\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "NAD", Value = "Namibian Dollar ($)", SortOrder = 99, IsActive = true, Meta = "{\"symbol\":\"$\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "NGN", Value = "Nigerian Naira (₦)", SortOrder = 100, IsActive = true, Meta = "{\"symbol\":\"₦\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "NIO", Value = "Nicaraguan Córdoba (C$)", SortOrder = 101, IsActive = true, Meta = "{\"symbol\":\"C$\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "NOK", Value = "Norwegian Krone (kr)", SortOrder = 102, IsActive = true, Meta = "{\"symbol\":\"kr\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "NPR", Value = "Nepalese Rupee (Rs)", SortOrder = 103, IsActive = true, Meta = "{\"symbol\":\"Rs\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "OMR", Value = "Omani Rial (ر.ع.)", SortOrder = 104, IsActive = true, Meta = "{\"symbol\":\"ر.ع.\",\"decimal\":3}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "PAB", Value = "Panamanian Balboa (B/.)", SortOrder = 105, IsActive = true, Meta = "{\"symbol\":\"B/.\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "PEN", Value = "Peruvian Sol (S/)", SortOrder = 106, IsActive = true, Meta = "{\"symbol\":\"S/\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "PGK", Value = "Papua New Guinean Kina (K)", SortOrder = 107, IsActive = true, Meta = "{\"symbol\":\"K\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "PHP", Value = "Philippine Peso (₱)", SortOrder = 108, IsActive = true, Meta = "{\"symbol\":\"₱\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "PKR", Value = "Pakistani Rupee (Rs)", SortOrder = 109, IsActive = true, Meta = "{\"symbol\":\"Rs\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "PLN", Value = "Polish Zloty (zł)", SortOrder = 110, IsActive = true, Meta = "{\"symbol\":\"zł\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "PYG", Value = "Paraguayan Guarani (₲)", SortOrder = 111, IsActive = true, Meta = "{\"symbol\":\"₲\",\"decimal\":0}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "QAR", Value = "Qatari Rial (ر.ق)", SortOrder = 112, IsActive = true, Meta = "{\"symbol\":\"ر.ق\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "RON", Value = "Romanian Leu (lei)", SortOrder = 113, IsActive = true, Meta = "{\"symbol\":\"lei\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "RSD", Value = "Serbian Dinar (дин)", SortOrder = 114, IsActive = true, Meta = "{\"symbol\":\"дин\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "RUB", Value = "Russian Ruble (₽)", SortOrder = 115, IsActive = true, Meta = "{\"symbol\":\"₽\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "RWF", Value = "Rwandan Franc (FRw)", SortOrder = 116, IsActive = true, Meta = "{\"symbol\":\"FRw\",\"decimal\":0}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "SAR", Value = "Saudi Riyal (ر.س)", SortOrder = 117, IsActive = true, Meta = "{\"symbol\":\"ر.س\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "SBD", Value = "Solomon Islands Dollar ($)", SortOrder = 118, IsActive = true, Meta = "{\"symbol\":\"$\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "SCR", Value = "Seychellois Rupee (Rs)", SortOrder = 119, IsActive = true, Meta = "{\"symbol\":\"Rs\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "SDG", Value = "Sudanese Pound (ج.س.)", SortOrder = 120, IsActive = true, Meta = "{\"symbol\":\"ج.س.\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "SEK", Value = "Swedish Krona (kr)", SortOrder = 121, IsActive = true, Meta = "{\"symbol\":\"kr\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "SGD", Value = "Singapore Dollar (S$)", SortOrder = 122, IsActive = true, Meta = "{\"symbol\":\"S$\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "SHP", Value = "Saint Helena Pound (£)", SortOrder = 123, IsActive = true, Meta = "{\"symbol\":\"£\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "SLE", Value = "Sierra Leonean Leone (Le)", SortOrder = 124, IsActive = true, Meta = "{\"symbol\":\"Le\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "SOS", Value = "Somali Shilling (Sh)", SortOrder = 125, IsActive = true, Meta = "{\"symbol\":\"Sh\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "SRD", Value = "Surinamese Dollar ($)", SortOrder = 126, IsActive = true, Meta = "{\"symbol\":\"$\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "SSP", Value = "South Sudanese Pound (£)", SortOrder = 127, IsActive = true, Meta = "{\"symbol\":\"£\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "STN", Value = "São Tomé and Príncipe Dobra (Db)", SortOrder = 128, IsActive = true, Meta = "{\"symbol\":\"Db\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "SYP", Value = "Syrian Pound (£S)", SortOrder = 129, IsActive = true, Meta = "{\"symbol\":\"£S\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "SZL", Value = "Swazi Lilangeni (L)", SortOrder = 130, IsActive = true, Meta = "{\"symbol\":\"L\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "THB", Value = "Thai Baht (฿)", SortOrder = 131, IsActive = true, Meta = "{\"symbol\":\"฿\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "TJS", Value = "Tajikistani Somoni (ЅМ)", SortOrder = 132, IsActive = true, Meta = "{\"symbol\":\"ЅМ\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "TMT", Value = "Turkmenistani Manat (m)", SortOrder = 133, IsActive = true, Meta = "{\"symbol\":\"m\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "TND", Value = "Tunisian Dinar (د.ت)", SortOrder = 134, IsActive = true, Meta = "{\"symbol\":\"د.ت\",\"decimal\":3}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "TOP", Value = "Tongan Paʻanga (T$)", SortOrder = 135, IsActive = true, Meta = "{\"symbol\":\"T$\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "TRY", Value = "Turkish Lira (₺)", SortOrder = 136, IsActive = true, Meta = "{\"symbol\":\"₺\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "TTD", Value = "Trinidad and Tobago Dollar ($)", SortOrder = 137, IsActive = true, Meta = "{\"symbol\":\"$\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "TWD", Value = "New Taiwan Dollar (NT$)", SortOrder = 138, IsActive = true, Meta = "{\"symbol\":\"NT$\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "TZS", Value = "Tanzanian Shilling (TSh)", SortOrder = 139, IsActive = true, Meta = "{\"symbol\":\"TSh\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "UAH", Value = "Ukrainian Hryvnia (₴)", SortOrder = 140, IsActive = true, Meta = "{\"symbol\":\"₴\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "UGX", Value = "Ugandan Shilling (USh)", SortOrder = 141, IsActive = true, Meta = "{\"symbol\":\"USh\",\"decimal\":0}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "UYU", Value = "Uruguayan Peso ($)", SortOrder = 142, IsActive = true, Meta = "{\"symbol\":\"$\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "UZS", Value = "Uzbekistan Som (сўм)", SortOrder = 143, IsActive = true, Meta = "{\"symbol\":\"сўм\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "VES", Value = "Venezuelan Bolívar (Bs)", SortOrder = 144, IsActive = true, Meta = "{\"symbol\":\"Bs\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "VND", Value = "Vietnamese Dong (₫)", SortOrder = 145, IsActive = true, Meta = "{\"symbol\":\"₫\",\"decimal\":0}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "VUV", Value = "Vanuatu Vatu (Vt)", SortOrder = 146, IsActive = true, Meta = "{\"symbol\":\"Vt\",\"decimal\":0}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "WST", Value = "Samoan Tala (T)", SortOrder = 147, IsActive = true, Meta = "{\"symbol\":\"T\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "XAF", Value = "CFA Franc BEAC (FCFA)", SortOrder = 148, IsActive = true, Meta = "{\"symbol\":\"FCFA\",\"decimal\":0}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "XCD", Value = "East Caribbean Dollar ($)", SortOrder = 149, IsActive = true, Meta = "{\"symbol\":\"$\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "XOF", Value = "CFA Franc BCEAO (CFA)", SortOrder = 150, IsActive = true, Meta = "{\"symbol\":\"CFA\",\"decimal\":0}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "XPF", Value = "CFP Franc (₣)", SortOrder = 151, IsActive = true, Meta = "{\"symbol\":\"₣\",\"decimal\":0}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "YER", Value = "Yemeni Rial (﷼)", SortOrder = 152, IsActive = true, Meta = "{\"symbol\":\"﷼\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "ZAR", Value = "South African Rand (R)", SortOrder = 153, IsActive = true, Meta = "{\"symbol\":\"R\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "ZMW", Value = "Zambian Kwacha (ZK)", SortOrder = 154, IsActive = true, Meta = "{\"symbol\":\"ZK\",\"decimal\":2}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "ZWL", Value = "Zimbabwean Dollar ($)", SortOrder = 155, IsActive = true, Meta = "{\"symbol\":\"$\",\"decimal\":2}" },
                // Precious metals
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "XAU", Value = "Gold (Troy Ounce)", SortOrder = 156, IsActive = true, Meta = "{\"symbol\":\"XAU\",\"decimal\":6}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "XAG", Value = "Silver (Troy Ounce)", SortOrder = 157, IsActive = true, Meta = "{\"symbol\":\"XAG\",\"decimal\":6}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "XPT", Value = "Platinum (Troy Ounce)", SortOrder = 158, IsActive = true, Meta = "{\"symbol\":\"XPT\",\"decimal\":6}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "XPD", Value = "Palladium (Troy Ounce)", SortOrder = 159, IsActive = true, Meta = "{\"symbol\":\"XPD\",\"decimal\":6}" },
                // Crypto (popular ones)
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "BTC", Value = "Bitcoin (₿)", SortOrder = 160, IsActive = true, Meta = "{\"symbol\":\"₿\",\"decimal\":8}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "ETH", Value = "Ethereum (Ξ)", SortOrder = 161, IsActive = true, Meta = "{\"symbol\":\"Ξ\",\"decimal\":18}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "USDT", Value = "Tether (₮)", SortOrder = 162, IsActive = true, Meta = "{\"symbol\":\"₮\",\"decimal\":6}" },
                new LookupItem { LookupCategoryId = currencyCategory.Id, Key = "USDC", Value = "USD Coin (USDC)", SortOrder = 163, IsActive = true, Meta = "{\"symbol\":\"USDC\",\"decimal\":6}" }
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

        // Seed additional master data categories if not exist
        await SeedAdditionalMasterDataAsync(context);

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
        };
    }

    /// <summary>
    /// Seeds additional master data categories for addresses, contact methods, and account locations
    /// </summary>
    private static async Task SeedAdditionalMasterDataAsync(CrmDbContext context)
    {
        // Check if AddressType category exists
        var addressTypeCategory = await context.LookupCategories.FirstOrDefaultAsync(c => c.Name == "AddressType");
        if (addressTypeCategory == null)
        {
            addressTypeCategory = new LookupCategory
            {
                Name = "AddressType",
                Description = "Types of addresses (Primary, Billing, Shipping, Work, Home, etc.)",
                IsActive = true
            };
            context.LookupCategories.Add(addressTypeCategory);
            await context.SaveChangesAsync();

            var addressTypes = new[]
            {
                new LookupItem { LookupCategoryId = addressTypeCategory.Id, Key = "Primary", Value = "Primary", SortOrder = 1, IsActive = true, Meta = "{\"icon\":\"🏠\"}" },
                new LookupItem { LookupCategoryId = addressTypeCategory.Id, Key = "Billing", Value = "Billing", SortOrder = 2, IsActive = true, Meta = "{\"icon\":\"💳\"}" },
                new LookupItem { LookupCategoryId = addressTypeCategory.Id, Key = "Shipping", Value = "Shipping", SortOrder = 3, IsActive = true, Meta = "{\"icon\":\"📦\"}" },
                new LookupItem { LookupCategoryId = addressTypeCategory.Id, Key = "Work", Value = "Work", SortOrder = 4, IsActive = true, Meta = "{\"icon\":\"🏢\"}" },
                new LookupItem { LookupCategoryId = addressTypeCategory.Id, Key = "Home", Value = "Home", SortOrder = 5, IsActive = true, Meta = "{\"icon\":\"🏡\"}" },
                new LookupItem { LookupCategoryId = addressTypeCategory.Id, Key = "Office", Value = "Office", SortOrder = 6, IsActive = true, Meta = "{\"icon\":\"🏬\"}" },
                new LookupItem { LookupCategoryId = addressTypeCategory.Id, Key = "Headquarters", Value = "Headquarters", SortOrder = 7, IsActive = true, Meta = "{\"icon\":\"🏛️\"}" },
                new LookupItem { LookupCategoryId = addressTypeCategory.Id, Key = "Branch", Value = "Branch", SortOrder = 8, IsActive = true, Meta = "{\"icon\":\"🏪\"}" },
                new LookupItem { LookupCategoryId = addressTypeCategory.Id, Key = "Store", Value = "Store", SortOrder = 9, IsActive = true, Meta = "{\"icon\":\"🛒\"}" },
                new LookupItem { LookupCategoryId = addressTypeCategory.Id, Key = "Factory", Value = "Factory", SortOrder = 10, IsActive = true, Meta = "{\"icon\":\"🏭\"}" },
                new LookupItem { LookupCategoryId = addressTypeCategory.Id, Key = "Warehouse", Value = "Warehouse", SortOrder = 11, IsActive = true, Meta = "{\"icon\":\"📦\"}" },
                new LookupItem { LookupCategoryId = addressTypeCategory.Id, Key = "Other", Value = "Other", SortOrder = 99, IsActive = true, Meta = "{\"icon\":\"📍\"}" }
            };
            context.LookupItems.AddRange(addressTypes);
            await context.SaveChangesAsync();
        }

        // Check if ContactMethodType category exists
        var contactMethodTypeCategory = await context.LookupCategories.FirstOrDefaultAsync(c => c.Name == "ContactMethodType");
        if (contactMethodTypeCategory == null)
        {
            contactMethodTypeCategory = new LookupCategory
            {
                Name = "ContactMethodType",
                Description = "Types of contact methods (Work, Home, Mobile, Personal, Other)",
                IsActive = true
            };
            context.LookupCategories.Add(contactMethodTypeCategory);
            await context.SaveChangesAsync();

            var contactMethodTypes = new[]
            {
                new LookupItem { LookupCategoryId = contactMethodTypeCategory.Id, Key = "Work", Value = "Work", SortOrder = 1, IsActive = true, Meta = "{\"icon\":\"🏢\"}" },
                new LookupItem { LookupCategoryId = contactMethodTypeCategory.Id, Key = "Home", Value = "Home", SortOrder = 2, IsActive = true, Meta = "{\"icon\":\"🏠\"}" },
                new LookupItem { LookupCategoryId = contactMethodTypeCategory.Id, Key = "Mobile", Value = "Mobile", SortOrder = 3, IsActive = true, Meta = "{\"icon\":\"📱\"}" },
                new LookupItem { LookupCategoryId = contactMethodTypeCategory.Id, Key = "Personal", Value = "Personal", SortOrder = 4, IsActive = true, Meta = "{\"icon\":\"👤\"}" },
                new LookupItem { LookupCategoryId = contactMethodTypeCategory.Id, Key = "Other", Value = "Other", SortOrder = 99, IsActive = true, Meta = "{\"icon\":\"📋\"}" }
            };
            context.LookupItems.AddRange(contactMethodTypes);
            await context.SaveChangesAsync();
        }

        // Check if ContactPriority category exists
        var contactPriorityCategory = await context.LookupCategories.FirstOrDefaultAsync(c => c.Name == "ContactPriority");
        if (contactPriorityCategory == null)
        {
            contactPriorityCategory = new LookupCategory
            {
                Name = "ContactPriority",
                Description = "Priority of contact information (Primary, Secondary, Other)",
                IsActive = true
            };
            context.LookupCategories.Add(contactPriorityCategory);
            await context.SaveChangesAsync();

            var contactPriorities = new[]
            {
                new LookupItem { LookupCategoryId = contactPriorityCategory.Id, Key = "Primary", Value = "Primary", SortOrder = 1, IsActive = true, Meta = "{\"icon\":\"⭐\"}" },
                new LookupItem { LookupCategoryId = contactPriorityCategory.Id, Key = "Secondary", Value = "Secondary", SortOrder = 2, IsActive = true, Meta = "{\"icon\":\"✦\"}" },
                new LookupItem { LookupCategoryId = contactPriorityCategory.Id, Key = "Other", Value = "Other", SortOrder = 99, IsActive = true, Meta = "{\"icon\":\"○\"}" }
            };
            context.LookupItems.AddRange(contactPriorities);
            await context.SaveChangesAsync();
        }

        // Check if AccountLocationType category exists
        var accountLocationCategory = await context.LookupCategories.FirstOrDefaultAsync(c => c.Name == "AccountLocationType");
        if (accountLocationCategory == null)
        {
            accountLocationCategory = new LookupCategory
            {
                Name = "AccountLocationType",
                Description = "Types of account/corporate locations (Office, HQ, Branch, Store, Factory, Warehouse, etc.)",
                IsActive = true
            };
            context.LookupCategories.Add(accountLocationCategory);
            await context.SaveChangesAsync();

            var accountLocationTypes = new[]
            {
                new LookupItem { LookupCategoryId = accountLocationCategory.Id, Key = "Office", Value = "Office", SortOrder = 1, IsActive = true, Meta = "{\"icon\":\"🏢\",\"description\":\"General office location\"}" },
                new LookupItem { LookupCategoryId = accountLocationCategory.Id, Key = "Headquarters", Value = "Headquarters", SortOrder = 2, IsActive = true, Meta = "{\"icon\":\"🏛️\",\"description\":\"Main headquarters\"}" },
                new LookupItem { LookupCategoryId = accountLocationCategory.Id, Key = "RegionalHQ", Value = "Regional HQ", SortOrder = 3, IsActive = true, Meta = "{\"icon\":\"🌍\",\"description\":\"Regional headquarters\"}" },
                new LookupItem { LookupCategoryId = accountLocationCategory.Id, Key = "Branch", Value = "Branch", SortOrder = 4, IsActive = true, Meta = "{\"icon\":\"🏬\",\"description\":\"Branch office\"}" },
                new LookupItem { LookupCategoryId = accountLocationCategory.Id, Key = "Store", Value = "Store", SortOrder = 5, IsActive = true, Meta = "{\"icon\":\"🛒\",\"description\":\"Retail store\"}" },
                new LookupItem { LookupCategoryId = accountLocationCategory.Id, Key = "Factory", Value = "Factory", SortOrder = 6, IsActive = true, Meta = "{\"icon\":\"🏭\",\"description\":\"Manufacturing plant\"}" },
                new LookupItem { LookupCategoryId = accountLocationCategory.Id, Key = "Warehouse", Value = "Warehouse", SortOrder = 7, IsActive = true, Meta = "{\"icon\":\"📦\",\"description\":\"Storage warehouse\"}" },
                new LookupItem { LookupCategoryId = accountLocationCategory.Id, Key = "DistributionCenter", Value = "Distribution Center", SortOrder = 8, IsActive = true, Meta = "{\"icon\":\"🚚\",\"description\":\"Distribution hub\"}" },
                new LookupItem { LookupCategoryId = accountLocationCategory.Id, Key = "DataCenter", Value = "Data Center", SortOrder = 9, IsActive = true, Meta = "{\"icon\":\"🖥️\",\"description\":\"Data center facility\"}" },
                new LookupItem { LookupCategoryId = accountLocationCategory.Id, Key = "RDCenter", Value = "R&D Center", SortOrder = 10, IsActive = true, Meta = "{\"icon\":\"🔬\",\"description\":\"Research & development\"}" },
                new LookupItem { LookupCategoryId = accountLocationCategory.Id, Key = "Other", Value = "Other", SortOrder = 99, IsActive = true, Meta = "{\"icon\":\"📍\",\"description\":\"Other location type\"}" }
            };
            context.LookupItems.AddRange(accountLocationTypes);
            await context.SaveChangesAsync();
        }

        // Check if SocialMediaPlatform category exists
        var socialPlatformCategory = await context.LookupCategories.FirstOrDefaultAsync(c => c.Name == "SocialMediaPlatform");
        if (socialPlatformCategory == null)
        {
            socialPlatformCategory = new LookupCategory
            {
                Name = "SocialMediaPlatform",
                Description = "Social media platforms (LinkedIn, Twitter, Facebook, etc.)",
                IsActive = true
            };
            context.LookupCategories.Add(socialPlatformCategory);
            await context.SaveChangesAsync();

            var socialPlatforms = new[]
            {
                new LookupItem { LookupCategoryId = socialPlatformCategory.Id, Key = "LinkedIn", Value = "LinkedIn", SortOrder = 1, IsActive = true, Meta = "{\"icon\":\"💼\",\"urlPrefix\":\"https://linkedin.com/in/\"}" },
                new LookupItem { LookupCategoryId = socialPlatformCategory.Id, Key = "Twitter", Value = "Twitter/X", SortOrder = 2, IsActive = true, Meta = "{\"icon\":\"🐦\",\"urlPrefix\":\"https://twitter.com/\"}" },
                new LookupItem { LookupCategoryId = socialPlatformCategory.Id, Key = "Facebook", Value = "Facebook", SortOrder = 3, IsActive = true, Meta = "{\"icon\":\"📘\",\"urlPrefix\":\"https://facebook.com/\"}" },
                new LookupItem { LookupCategoryId = socialPlatformCategory.Id, Key = "Instagram", Value = "Instagram", SortOrder = 4, IsActive = true, Meta = "{\"icon\":\"📷\",\"urlPrefix\":\"https://instagram.com/\"}" },
                new LookupItem { LookupCategoryId = socialPlatformCategory.Id, Key = "YouTube", Value = "YouTube", SortOrder = 5, IsActive = true, Meta = "{\"icon\":\"🎬\",\"urlPrefix\":\"https://youtube.com/@\"}" },
                new LookupItem { LookupCategoryId = socialPlatformCategory.Id, Key = "TikTok", Value = "TikTok", SortOrder = 6, IsActive = true, Meta = "{\"icon\":\"🎵\",\"urlPrefix\":\"https://tiktok.com/@\"}" },
                new LookupItem { LookupCategoryId = socialPlatformCategory.Id, Key = "GitHub", Value = "GitHub", SortOrder = 7, IsActive = true, Meta = "{\"icon\":\"💻\",\"urlPrefix\":\"https://github.com/\"}" },
                new LookupItem { LookupCategoryId = socialPlatformCategory.Id, Key = "Website", Value = "Website", SortOrder = 8, IsActive = true, Meta = "{\"icon\":\"🌐\",\"urlPrefix\":\"\"}" },
                new LookupItem { LookupCategoryId = socialPlatformCategory.Id, Key = "Other", Value = "Other", SortOrder = 99, IsActive = true, Meta = "{\"icon\":\"🔗\",\"urlPrefix\":\"\"}" }
            };
            context.LookupItems.AddRange(socialPlatforms);
            await context.SaveChangesAsync();
        }
    }
}