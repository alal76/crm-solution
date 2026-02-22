// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Models;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using SocialPlatform = CRM.Core.Models.SocialMediaPlatform;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for seeding core application data (departments, sample accounts/contacts/products,
/// lookups, system settings, and module field configurations).
/// Consolidates all non-auth seeding previously in DbSeed into a dedicated service
/// as part of ADR-002: Unified EF Core Schema Management.
/// </summary>
public interface ICoreDataSeederService
{
    /// <summary>Seeds departments if none exist.</summary>
    Task SeedDepartmentsAsync(CancellationToken cancellationToken = default);

    /// <summary>Seeds sample accounts if none exist.</summary>
    Task SeedSampleAccountsAsync(CancellationToken cancellationToken = default);

    /// <summary>Seeds sample products if none exist.</summary>
    Task SeedSampleProductsAsync(CancellationToken cancellationToken = default);

    /// <summary>Seeds lookup categories and items (currencies, billing cycles, contact methods, and 12 ensure-lookup categories).</summary>
    Task SeedLookupsAsync(CancellationToken cancellationToken = default);

    /// <summary>Seeds sample contacts if none exist.</summary>
    Task SeedSampleContactsAsync(CancellationToken cancellationToken = default);

    /// <summary>Seeds system settings (navigation order config).</summary>
    Task SeedSystemSettingsAsync(CancellationToken cancellationToken = default);

    /// <summary>Seeds module field configurations if none exist.</summary>
    Task SeedModuleFieldConfigurationsAsync(CancellationToken cancellationToken = default);

    /// <summary>Force re-seeds all module field configurations (clears existing first).</summary>
    Task ForceReseedModuleFieldConfigurationsAsync(CancellationToken cancellationToken = default);

    /// <summary>Seeds additional master data lookup categories (AddressType, ContactMethodType, ContactPriority, AccountLocationType, SocialMediaPlatform).</summary>
    Task SeedAdditionalMasterDataAsync(CancellationToken cancellationToken = default);

    /// <summary>Seeds 12 ensure-lookup categories (Salutation, Gender, LifecycleStage, etc.) if they don't already exist.</summary>
    Task SeedEnsureLookupsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of <see cref="ICoreDataSeederService"/>.
/// </summary>
public class CoreDataSeederService : ICoreDataSeederService
{
    private readonly CrmDbContext _context;
    private readonly ILogger<CoreDataSeederService> _logger;
    private readonly ModuleFieldConfigurationService _fieldConfigService;

    public CoreDataSeederService(CrmDbContext context, ILogger<CoreDataSeederService> logger, ModuleFieldConfigurationService fieldConfigService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fieldConfigService = fieldConfigService ?? throw new ArgumentNullException(nameof(fieldConfigService));
    }

    // ──────────────────────────────────────────────
    // Departments
    // ──────────────────────────────────────────────

    public async Task SeedDepartmentsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Seeding departments...");

        if (_context.Departments.Any())
            return;

        var departments = new List<Department>
        {
            new() { Name = "Executive", Description = "Executive leadership and C-suite", DepartmentCode = "EXEC", IsActive = true },
            new() { Name = "Sales", Description = "Sales and business development", DepartmentCode = "SALES", IsActive = true },
            new() { Name = "Marketing", Description = "Marketing and communications", DepartmentCode = "MKTG", IsActive = true },
            new() { Name = "Customer Support", Description = "Customer service and support", DepartmentCode = "SUPPORT", IsActive = true },
            new() { Name = "Customer Success", Description = "Customer success and retention", DepartmentCode = "CS", IsActive = true },
            new() { Name = "Engineering", Description = "Software engineering and development", DepartmentCode = "ENG", IsActive = true },
            new() { Name = "Product", Description = "Product management and design", DepartmentCode = "PROD", IsActive = true },
            new() { Name = "Finance", Description = "Finance and accounting", DepartmentCode = "FIN", IsActive = true },
            new() { Name = "Human Resources", Description = "Human resources and talent acquisition", DepartmentCode = "HR", IsActive = true },
            new() { Name = "Legal", Description = "Legal and compliance", DepartmentCode = "LEGAL", IsActive = true },
            new() { Name = "Operations", Description = "Operations and logistics", DepartmentCode = "OPS", IsActive = true },
            new() { Name = "IT", Description = "Information technology and infrastructure", DepartmentCode = "IT", IsActive = true },
            new() { Name = "Quality Assurance", Description = "Quality assurance and testing", DepartmentCode = "QA", IsActive = true },
            new() { Name = "Research & Development", Description = "Research and development", DepartmentCode = "RD", IsActive = true },
            new() { Name = "Procurement", Description = "Procurement and vendor management", DepartmentCode = "PROC", IsActive = true },
        };

        _context.Departments.AddRange(departments);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded {Count} departments", departments.Count);
    }

    // ──────────────────────────────────────────────
    // Sample Accounts
    // ──────────────────────────────────────────────

    public async Task SeedSampleAccountsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Seeding sample accounts...");

        if (_context.Accounts.Any())
            return;

        var accounts = new List<Account>
        {
            new()
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Phone = "+1-555-0001",
                Company = "Tech Corp",
                Category = AccountCategory.Organization,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@example.com",
                Phone = "+1-555-0002",
                Company = "Innovation Inc",
                Category = AccountCategory.Organization,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _context.Accounts.AddRange(accounts);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded {Count} sample accounts", accounts.Count);
    }

    // ──────────────────────────────────────────────
    // Sample Products
    // ──────────────────────────────────────────────

    public async Task SeedSampleProductsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Seeding sample products...");

        if (_context.Products.Any())
            return;

        var products = new List<Product>
        {
            new()
            {
                Name = "Premium Package",
                SKU = "PKG-001",
                Description = "Premium CRM package with all features",
                Price = 999.99m,
                Quantity = 50,
                Category = "Software",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Name = "Standard Package",
                SKU = "PKG-002",
                Description = "Standard CRM package with essential features",
                Price = 499.99m,
                Quantity = 100,
                Category = "Software",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _context.Products.AddRange(products);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded {Count} sample products", products.Count);
    }

    // ──────────────────────────────────────────────
    // Lookups (Currencies, BillingCycles, ContactMethods + 12 ensure-lookups)
    // ──────────────────────────────────────────────

    public async Task SeedLookupsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Seeding lookups...");

        if (!_context.LookupCategories.Any())
        {
            // ── Currency category ──
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
            var contactMethodCategory = new LookupCategory
            {
                Name = "PreferredContactMethod",
                Description = "Preferred contact method for contacts",
                IsActive = true
            };

            _context.LookupCategories.Add(currencyCategory);
            _context.LookupCategories.Add(billingCycleCategory);
            _context.LookupCategories.Add(contactMethodCategory);
            await _context.SaveChangesAsync(cancellationToken);

            // ── Currency items (163 ISO 4217 + precious metals + crypto) ──
            var currencies = new List<LookupItem>
            {
                // Major currencies (SortOrder 1-10)
                new() { LookupCategoryId = currencyCategory.Id, Key = "USD", Value = "US Dollar", Meta = "{\"symbol\":\"$\",\"decimal\":2}", SortOrder = 1, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "EUR", Value = "Euro", Meta = "{\"symbol\":\"€\",\"decimal\":2}", SortOrder = 2, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "GBP", Value = "British Pound", Meta = "{\"symbol\":\"£\",\"decimal\":2}", SortOrder = 3, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "JPY", Value = "Japanese Yen", Meta = "{\"symbol\":\"¥\",\"decimal\":0}", SortOrder = 4, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "CHF", Value = "Swiss Franc", Meta = "{\"symbol\":\"CHF\",\"decimal\":2}", SortOrder = 5, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "CAD", Value = "Canadian Dollar", Meta = "{\"symbol\":\"C$\",\"decimal\":2}", SortOrder = 6, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "AUD", Value = "Australian Dollar", Meta = "{\"symbol\":\"A$\",\"decimal\":2}", SortOrder = 7, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "CNY", Value = "Chinese Yuan", Meta = "{\"symbol\":\"¥\",\"decimal\":2}", SortOrder = 8, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "INR", Value = "Indian Rupee", Meta = "{\"symbol\":\"₹\",\"decimal\":2}", SortOrder = 9, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "NZD", Value = "New Zealand Dollar", Meta = "{\"symbol\":\"NZ$\",\"decimal\":2}", SortOrder = 10, IsActive = true },

                // World currencies A-Z (SortOrder 11-155)
                new() { LookupCategoryId = currencyCategory.Id, Key = "AED", Value = "UAE Dirham", Meta = "{\"symbol\":\"د.إ\",\"decimal\":2}", SortOrder = 11, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "AFN", Value = "Afghan Afghani", Meta = "{\"symbol\":\"؋\",\"decimal\":2}", SortOrder = 12, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "ALL", Value = "Albanian Lek", Meta = "{\"symbol\":\"L\",\"decimal\":2}", SortOrder = 13, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "AMD", Value = "Armenian Dram", Meta = "{\"symbol\":\"֏\",\"decimal\":2}", SortOrder = 14, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "ANG", Value = "Netherlands Antillean Guilder", Meta = "{\"symbol\":\"ƒ\",\"decimal\":2}", SortOrder = 15, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "AOA", Value = "Angolan Kwanza", Meta = "{\"symbol\":\"Kz\",\"decimal\":2}", SortOrder = 16, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "ARS", Value = "Argentine Peso", Meta = "{\"symbol\":\"$\",\"decimal\":2}", SortOrder = 17, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "AWG", Value = "Aruban Florin", Meta = "{\"symbol\":\"ƒ\",\"decimal\":2}", SortOrder = 18, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "AZN", Value = "Azerbaijani Manat", Meta = "{\"symbol\":\"₼\",\"decimal\":2}", SortOrder = 19, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "BAM", Value = "Bosnia-Herzegovina Convertible Mark", Meta = "{\"symbol\":\"KM\",\"decimal\":2}", SortOrder = 20, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "BBD", Value = "Barbadian Dollar", Meta = "{\"symbol\":\"Bds$\",\"decimal\":2}", SortOrder = 21, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "BDT", Value = "Bangladeshi Taka", Meta = "{\"symbol\":\"৳\",\"decimal\":2}", SortOrder = 22, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "BGN", Value = "Bulgarian Lev", Meta = "{\"symbol\":\"лв\",\"decimal\":2}", SortOrder = 23, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "BHD", Value = "Bahraini Dinar", Meta = "{\"symbol\":\"BD\",\"decimal\":3}", SortOrder = 24, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "BIF", Value = "Burundian Franc", Meta = "{\"symbol\":\"FBu\",\"decimal\":0}", SortOrder = 25, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "BMD", Value = "Bermudan Dollar", Meta = "{\"symbol\":\"$\",\"decimal\":2}", SortOrder = 26, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "BND", Value = "Brunei Dollar", Meta = "{\"symbol\":\"B$\",\"decimal\":2}", SortOrder = 27, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "BOB", Value = "Bolivian Boliviano", Meta = "{\"symbol\":\"Bs.\",\"decimal\":2}", SortOrder = 28, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "BRL", Value = "Brazilian Real", Meta = "{\"symbol\":\"R$\",\"decimal\":2}", SortOrder = 29, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "BSD", Value = "Bahamian Dollar", Meta = "{\"symbol\":\"B$\",\"decimal\":2}", SortOrder = 30, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "BTN", Value = "Bhutanese Ngultrum", Meta = "{\"symbol\":\"Nu.\",\"decimal\":2}", SortOrder = 31, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "BWP", Value = "Botswanan Pula", Meta = "{\"symbol\":\"P\",\"decimal\":2}", SortOrder = 32, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "BYN", Value = "Belarusian Ruble", Meta = "{\"symbol\":\"Br\",\"decimal\":2}", SortOrder = 33, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "BZD", Value = "Belize Dollar", Meta = "{\"symbol\":\"BZ$\",\"decimal\":2}", SortOrder = 34, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "CDF", Value = "Congolese Franc", Meta = "{\"symbol\":\"FC\",\"decimal\":2}", SortOrder = 35, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "CLP", Value = "Chilean Peso", Meta = "{\"symbol\":\"$\",\"decimal\":0}", SortOrder = 36, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "COP", Value = "Colombian Peso", Meta = "{\"symbol\":\"$\",\"decimal\":2}", SortOrder = 37, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "CRC", Value = "Costa Rican Colón", Meta = "{\"symbol\":\"₡\",\"decimal\":2}", SortOrder = 38, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "CUP", Value = "Cuban Peso", Meta = "{\"symbol\":\"$\",\"decimal\":2}", SortOrder = 39, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "CVE", Value = "Cape Verdean Escudo", Meta = "{\"symbol\":\"$\",\"decimal\":2}", SortOrder = 40, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "CZK", Value = "Czech Koruna", Meta = "{\"symbol\":\"Kč\",\"decimal\":2}", SortOrder = 41, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "DJF", Value = "Djiboutian Franc", Meta = "{\"symbol\":\"Fdj\",\"decimal\":0}", SortOrder = 42, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "DKK", Value = "Danish Krone", Meta = "{\"symbol\":\"kr\",\"decimal\":2}", SortOrder = 43, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "DOP", Value = "Dominican Peso", Meta = "{\"symbol\":\"RD$\",\"decimal\":2}", SortOrder = 44, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "DZD", Value = "Algerian Dinar", Meta = "{\"symbol\":\"د.ج\",\"decimal\":2}", SortOrder = 45, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "EGP", Value = "Egyptian Pound", Meta = "{\"symbol\":\"E£\",\"decimal\":2}", SortOrder = 46, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "ERN", Value = "Eritrean Nakfa", Meta = "{\"symbol\":\"Nfk\",\"decimal\":2}", SortOrder = 47, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "ETB", Value = "Ethiopian Birr", Meta = "{\"symbol\":\"Br\",\"decimal\":2}", SortOrder = 48, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "FJD", Value = "Fijian Dollar", Meta = "{\"symbol\":\"FJ$\",\"decimal\":2}", SortOrder = 49, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "FKP", Value = "Falkland Islands Pound", Meta = "{\"symbol\":\"FK£\",\"decimal\":2}", SortOrder = 50, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "GEL", Value = "Georgian Lari", Meta = "{\"symbol\":\"₾\",\"decimal\":2}", SortOrder = 51, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "GHS", Value = "Ghanaian Cedi", Meta = "{\"symbol\":\"GH₵\",\"decimal\":2}", SortOrder = 52, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "GIP", Value = "Gibraltar Pound", Meta = "{\"symbol\":\"£\",\"decimal\":2}", SortOrder = 53, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "GMD", Value = "Gambian Dalasi", Meta = "{\"symbol\":\"D\",\"decimal\":2}", SortOrder = 54, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "GNF", Value = "Guinean Franc", Meta = "{\"symbol\":\"FG\",\"decimal\":0}", SortOrder = 55, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "GTQ", Value = "Guatemalan Quetzal", Meta = "{\"symbol\":\"Q\",\"decimal\":2}", SortOrder = 56, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "GYD", Value = "Guyanaese Dollar", Meta = "{\"symbol\":\"GY$\",\"decimal\":2}", SortOrder = 57, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "HKD", Value = "Hong Kong Dollar", Meta = "{\"symbol\":\"HK$\",\"decimal\":2}", SortOrder = 58, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "HNL", Value = "Honduran Lempira", Meta = "{\"symbol\":\"L\",\"decimal\":2}", SortOrder = 59, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "HRK", Value = "Croatian Kuna", Meta = "{\"symbol\":\"kn\",\"decimal\":2}", SortOrder = 60, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "HTG", Value = "Haitian Gourde", Meta = "{\"symbol\":\"G\",\"decimal\":2}", SortOrder = 61, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "HUF", Value = "Hungarian Forint", Meta = "{\"symbol\":\"Ft\",\"decimal\":2}", SortOrder = 62, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "IDR", Value = "Indonesian Rupiah", Meta = "{\"symbol\":\"Rp\",\"decimal\":2}", SortOrder = 63, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "ILS", Value = "Israeli New Shekel", Meta = "{\"symbol\":\"₪\",\"decimal\":2}", SortOrder = 64, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "IQD", Value = "Iraqi Dinar", Meta = "{\"symbol\":\"ع.د\",\"decimal\":3}", SortOrder = 65, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "IRR", Value = "Iranian Rial", Meta = "{\"symbol\":\"﷼\",\"decimal\":2}", SortOrder = 66, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "ISK", Value = "Icelandic Króna", Meta = "{\"symbol\":\"kr\",\"decimal\":0}", SortOrder = 67, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "JMD", Value = "Jamaican Dollar", Meta = "{\"symbol\":\"J$\",\"decimal\":2}", SortOrder = 68, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "JOD", Value = "Jordanian Dinar", Meta = "{\"symbol\":\"JD\",\"decimal\":3}", SortOrder = 69, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "KES", Value = "Kenyan Shilling", Meta = "{\"symbol\":\"KSh\",\"decimal\":2}", SortOrder = 70, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "KGS", Value = "Kyrgystani Som", Meta = "{\"symbol\":\"сом\",\"decimal\":2}", SortOrder = 71, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "KHR", Value = "Cambodian Riel", Meta = "{\"symbol\":\"៛\",\"decimal\":2}", SortOrder = 72, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "KMF", Value = "Comorian Franc", Meta = "{\"symbol\":\"CF\",\"decimal\":0}", SortOrder = 73, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "KPW", Value = "North Korean Won", Meta = "{\"symbol\":\"₩\",\"decimal\":2}", SortOrder = 74, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "KRW", Value = "South Korean Won", Meta = "{\"symbol\":\"₩\",\"decimal\":0}", SortOrder = 75, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "KWD", Value = "Kuwaiti Dinar", Meta = "{\"symbol\":\"د.ك\",\"decimal\":3}", SortOrder = 76, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "KYD", Value = "Cayman Islands Dollar", Meta = "{\"symbol\":\"CI$\",\"decimal\":2}", SortOrder = 77, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "KZT", Value = "Kazakhstani Tenge", Meta = "{\"symbol\":\"₸\",\"decimal\":2}", SortOrder = 78, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "LAK", Value = "Laotian Kip", Meta = "{\"symbol\":\"₭\",\"decimal\":2}", SortOrder = 79, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "LBP", Value = "Lebanese Pound", Meta = "{\"symbol\":\"ل.ل\",\"decimal\":2}", SortOrder = 80, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "LKR", Value = "Sri Lankan Rupee", Meta = "{\"symbol\":\"Rs\",\"decimal\":2}", SortOrder = 81, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "LRD", Value = "Liberian Dollar", Meta = "{\"symbol\":\"L$\",\"decimal\":2}", SortOrder = 82, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "LSL", Value = "Lesotho Loti", Meta = "{\"symbol\":\"L\",\"decimal\":2}", SortOrder = 83, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "LYD", Value = "Libyan Dinar", Meta = "{\"symbol\":\"ل.د\",\"decimal\":3}", SortOrder = 84, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "MAD", Value = "Moroccan Dirham", Meta = "{\"symbol\":\"د.م.\",\"decimal\":2}", SortOrder = 85, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "MDL", Value = "Moldovan Leu", Meta = "{\"symbol\":\"L\",\"decimal\":2}", SortOrder = 86, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "MGA", Value = "Malagasy Ariary", Meta = "{\"symbol\":\"Ar\",\"decimal\":2}", SortOrder = 87, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "MKD", Value = "Macedonian Denar", Meta = "{\"symbol\":\"ден\",\"decimal\":2}", SortOrder = 88, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "MMK", Value = "Myanmar Kyat", Meta = "{\"symbol\":\"K\",\"decimal\":2}", SortOrder = 89, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "MNT", Value = "Mongolian Tugrik", Meta = "{\"symbol\":\"₮\",\"decimal\":2}", SortOrder = 90, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "MOP", Value = "Macanese Pataca", Meta = "{\"symbol\":\"MOP$\",\"decimal\":2}", SortOrder = 91, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "MRU", Value = "Mauritanian Ouguiya", Meta = "{\"symbol\":\"UM\",\"decimal\":2}", SortOrder = 92, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "MUR", Value = "Mauritian Rupee", Meta = "{\"symbol\":\"₨\",\"decimal\":2}", SortOrder = 93, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "MVR", Value = "Maldivian Rufiyaa", Meta = "{\"symbol\":\"Rf\",\"decimal\":2}", SortOrder = 94, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "MWK", Value = "Malawian Kwacha", Meta = "{\"symbol\":\"MK\",\"decimal\":2}", SortOrder = 95, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "MXN", Value = "Mexican Peso", Meta = "{\"symbol\":\"Mex$\",\"decimal\":2}", SortOrder = 96, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "MYR", Value = "Malaysian Ringgit", Meta = "{\"symbol\":\"RM\",\"decimal\":2}", SortOrder = 97, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "MZN", Value = "Mozambican Metical", Meta = "{\"symbol\":\"MT\",\"decimal\":2}", SortOrder = 98, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "NAD", Value = "Namibian Dollar", Meta = "{\"symbol\":\"N$\",\"decimal\":2}", SortOrder = 99, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "NGN", Value = "Nigerian Naira", Meta = "{\"symbol\":\"₦\",\"decimal\":2}", SortOrder = 100, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "NIO", Value = "Nicaraguan Córdoba", Meta = "{\"symbol\":\"C$\",\"decimal\":2}", SortOrder = 101, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "NOK", Value = "Norwegian Krone", Meta = "{\"symbol\":\"kr\",\"decimal\":2}", SortOrder = 102, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "NPR", Value = "Nepalese Rupee", Meta = "{\"symbol\":\"₨\",\"decimal\":2}", SortOrder = 103, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "OMR", Value = "Omani Rial", Meta = "{\"symbol\":\"ر.ع.\",\"decimal\":3}", SortOrder = 104, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "PAB", Value = "Panamanian Balboa", Meta = "{\"symbol\":\"B/.\",\"decimal\":2}", SortOrder = 105, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "PEN", Value = "Peruvian Sol", Meta = "{\"symbol\":\"S/.\",\"decimal\":2}", SortOrder = 106, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "PGK", Value = "Papua New Guinean Kina", Meta = "{\"symbol\":\"K\",\"decimal\":2}", SortOrder = 107, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "PHP", Value = "Philippine Peso", Meta = "{\"symbol\":\"₱\",\"decimal\":2}", SortOrder = 108, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "PKR", Value = "Pakistani Rupee", Meta = "{\"symbol\":\"₨\",\"decimal\":2}", SortOrder = 109, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "PLN", Value = "Polish Zloty", Meta = "{\"symbol\":\"zł\",\"decimal\":2}", SortOrder = 110, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "PYG", Value = "Paraguayan Guarani", Meta = "{\"symbol\":\"₲\",\"decimal\":0}", SortOrder = 111, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "QAR", Value = "Qatari Rial", Meta = "{\"symbol\":\"ر.ق\",\"decimal\":2}", SortOrder = 112, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "RON", Value = "Romanian Leu", Meta = "{\"symbol\":\"lei\",\"decimal\":2}", SortOrder = 113, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "RSD", Value = "Serbian Dinar", Meta = "{\"symbol\":\"din.\",\"decimal\":2}", SortOrder = 114, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "RUB", Value = "Russian Ruble", Meta = "{\"symbol\":\"₽\",\"decimal\":2}", SortOrder = 115, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "RWF", Value = "Rwandan Franc", Meta = "{\"symbol\":\"RF\",\"decimal\":0}", SortOrder = 116, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "SAR", Value = "Saudi Riyal", Meta = "{\"symbol\":\"ر.س\",\"decimal\":2}", SortOrder = 117, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "SBD", Value = "Solomon Islands Dollar", Meta = "{\"symbol\":\"SI$\",\"decimal\":2}", SortOrder = 118, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "SCR", Value = "Seychellois Rupee", Meta = "{\"symbol\":\"₨\",\"decimal\":2}", SortOrder = 119, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "SDG", Value = "Sudanese Pound", Meta = "{\"symbol\":\"ج.س.\",\"decimal\":2}", SortOrder = 120, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "SEK", Value = "Swedish Krona", Meta = "{\"symbol\":\"kr\",\"decimal\":2}", SortOrder = 121, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "SGD", Value = "Singapore Dollar", Meta = "{\"symbol\":\"S$\",\"decimal\":2}", SortOrder = 122, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "SHP", Value = "Saint Helena Pound", Meta = "{\"symbol\":\"£\",\"decimal\":2}", SortOrder = 123, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "SLE", Value = "Sierra Leonean Leone", Meta = "{\"symbol\":\"Le\",\"decimal\":2}", SortOrder = 124, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "SOS", Value = "Somali Shilling", Meta = "{\"symbol\":\"Sh\",\"decimal\":2}", SortOrder = 125, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "SRD", Value = "Surinamese Dollar", Meta = "{\"symbol\":\"$\",\"decimal\":2}", SortOrder = 126, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "SSP", Value = "South Sudanese Pound", Meta = "{\"symbol\":\"£\",\"decimal\":2}", SortOrder = 127, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "STN", Value = "São Tomé and Príncipe Dobra", Meta = "{\"symbol\":\"Db\",\"decimal\":2}", SortOrder = 128, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "SYP", Value = "Syrian Pound", Meta = "{\"symbol\":\"£S\",\"decimal\":2}", SortOrder = 129, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "SZL", Value = "Swazi Lilangeni", Meta = "{\"symbol\":\"E\",\"decimal\":2}", SortOrder = 130, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "THB", Value = "Thai Baht", Meta = "{\"symbol\":\"฿\",\"decimal\":2}", SortOrder = 131, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "TJS", Value = "Tajikistani Somoni", Meta = "{\"symbol\":\"SM\",\"decimal\":2}", SortOrder = 132, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "TMT", Value = "Turkmenistani Manat", Meta = "{\"symbol\":\"T\",\"decimal\":2}", SortOrder = 133, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "TND", Value = "Tunisian Dinar", Meta = "{\"symbol\":\"د.ت\",\"decimal\":3}", SortOrder = 134, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "TOP", Value = "Tongan Paʻanga", Meta = "{\"symbol\":\"T$\",\"decimal\":2}", SortOrder = 135, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "TRY", Value = "Turkish Lira", Meta = "{\"symbol\":\"₺\",\"decimal\":2}", SortOrder = 136, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "TTD", Value = "Trinidad & Tobago Dollar", Meta = "{\"symbol\":\"TT$\",\"decimal\":2}", SortOrder = 137, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "TWD", Value = "New Taiwan Dollar", Meta = "{\"symbol\":\"NT$\",\"decimal\":2}", SortOrder = 138, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "TZS", Value = "Tanzanian Shilling", Meta = "{\"symbol\":\"TSh\",\"decimal\":2}", SortOrder = 139, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "UAH", Value = "Ukrainian Hryvnia", Meta = "{\"symbol\":\"₴\",\"decimal\":2}", SortOrder = 140, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "UGX", Value = "Ugandan Shilling", Meta = "{\"symbol\":\"USh\",\"decimal\":0}", SortOrder = 141, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "UYU", Value = "Uruguayan Peso", Meta = "{\"symbol\":\"$U\",\"decimal\":2}", SortOrder = 142, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "UZS", Value = "Uzbekistan Som", Meta = "{\"symbol\":\"сўм\",\"decimal\":2}", SortOrder = 143, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "VES", Value = "Venezuelan Bolívar", Meta = "{\"symbol\":\"Bs.\",\"decimal\":2}", SortOrder = 144, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "VND", Value = "Vietnamese Dong", Meta = "{\"symbol\":\"₫\",\"decimal\":0}", SortOrder = 145, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "VUV", Value = "Vanuatu Vatu", Meta = "{\"symbol\":\"VT\",\"decimal\":0}", SortOrder = 146, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "WST", Value = "Samoan Tala", Meta = "{\"symbol\":\"WS$\",\"decimal\":2}", SortOrder = 147, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "XAF", Value = "Central African CFA Franc", Meta = "{\"symbol\":\"FCFA\",\"decimal\":0}", SortOrder = 148, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "XCD", Value = "East Caribbean Dollar", Meta = "{\"symbol\":\"EC$\",\"decimal\":2}", SortOrder = 149, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "XOF", Value = "West African CFA Franc", Meta = "{\"symbol\":\"CFA\",\"decimal\":0}", SortOrder = 150, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "XPF", Value = "CFP Franc", Meta = "{\"symbol\":\"₣\",\"decimal\":0}", SortOrder = 151, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "YER", Value = "Yemeni Rial", Meta = "{\"symbol\":\"﷼\",\"decimal\":2}", SortOrder = 152, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "ZAR", Value = "South African Rand", Meta = "{\"symbol\":\"R\",\"decimal\":2}", SortOrder = 153, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "ZMW", Value = "Zambian Kwacha", Meta = "{\"symbol\":\"ZK\",\"decimal\":2}", SortOrder = 154, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "ZWL", Value = "Zimbabwean Dollar", Meta = "{\"symbol\":\"Z$\",\"decimal\":2}", SortOrder = 155, IsActive = true },

                // Precious metals (SortOrder 156-159)
                new() { LookupCategoryId = currencyCategory.Id, Key = "XAU", Value = "Gold", Meta = "{\"symbol\":\"XAU\",\"decimal\":6}", SortOrder = 156, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "XAG", Value = "Silver", Meta = "{\"symbol\":\"XAG\",\"decimal\":6}", SortOrder = 157, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "XPT", Value = "Platinum", Meta = "{\"symbol\":\"XPT\",\"decimal\":6}", SortOrder = 158, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "XPD", Value = "Palladium", Meta = "{\"symbol\":\"XPD\",\"decimal\":6}", SortOrder = 159, IsActive = true },

                // Crypto currencies (SortOrder 160-163)
                new() { LookupCategoryId = currencyCategory.Id, Key = "BTC", Value = "Bitcoin", Meta = "{\"symbol\":\"₿\",\"decimal\":8}", SortOrder = 160, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "ETH", Value = "Ethereum", Meta = "{\"symbol\":\"Ξ\",\"decimal\":18}", SortOrder = 161, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "USDT", Value = "Tether", Meta = "{\"symbol\":\"₮\",\"decimal\":6}", SortOrder = 162, IsActive = true },
                new() { LookupCategoryId = currencyCategory.Id, Key = "USDC", Value = "USD Coin", Meta = "{\"symbol\":\"USDC\",\"decimal\":6}", SortOrder = 163, IsActive = true },
            };

            // ── Billing cycle items ──
            var billingCycles = new List<LookupItem>
            {
                new() { LookupCategoryId = billingCycleCategory.Id, Key = "Monthly", Value = "Monthly", SortOrder = 1, IsActive = true },
                new() { LookupCategoryId = billingCycleCategory.Id, Key = "Quarterly", Value = "Quarterly", SortOrder = 2, IsActive = true },
                new() { LookupCategoryId = billingCycleCategory.Id, Key = "Yearly", Value = "Yearly", SortOrder = 3, IsActive = true },
            };

            // ── Preferred contact method items ──
            var contactMethods = new List<LookupItem>
            {
                new() { LookupCategoryId = contactMethodCategory.Id, Key = "Email", Value = "Email", SortOrder = 1, IsActive = true },
                new() { LookupCategoryId = contactMethodCategory.Id, Key = "Phone", Value = "Phone", SortOrder = 2, IsActive = true },
                new() { LookupCategoryId = contactMethodCategory.Id, Key = "SMS", Value = "SMS", SortOrder = 3, IsActive = true },
            };

            _context.LookupItems.AddRange(currencies);
            _context.LookupItems.AddRange(billingCycles);
            _context.LookupItems.AddRange(contactMethods);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Seeded {CurrencyCount} currencies, {BillingCount} billing cycles, {ContactCount} contact methods",
                currencies.Count, billingCycles.Count, contactMethods.Count);
        }

        // Also seed the 12 ensure-lookup categories
        await SeedEnsureLookupsAsync(cancellationToken);

        // Also seed additional master data
        await SeedAdditionalMasterDataAsync(cancellationToken);

        _logger.LogInformation("Lookups seeding complete");
    }

    // ──────────────────────────────────────────────
    // Ensure Lookups (12 standard categories)
    // ──────────────────────────────────────────────

    public async Task SeedEnsureLookupsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Ensuring standard lookup categories...");

        var lookups = new Dictionary<string, string[]>
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
            { "CustomerType", new[] { "Individual", "Company", "Government" } },
        };

        foreach (var (categoryName, items) in lookups)
        {
            if (!await _context.LookupCategories.AnyAsync(c => c.Name == categoryName, cancellationToken))
            {
                var category = new LookupCategory
                {
                    Name = categoryName,
                    Description = categoryName + " values",
                    IsActive = true
                };
                _context.LookupCategories.Add(category);
                await _context.SaveChangesAsync(cancellationToken);

                var lookupItems = items.Select((item, idx) => new LookupItem
                {
                    LookupCategoryId = category.Id,
                    Key = item,
                    Value = item,
                    SortOrder = idx + 1,
                    IsActive = true
                }).ToList();

                _context.LookupItems.AddRange(lookupItems);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        _logger.LogInformation("Ensure lookups complete");
    }

    // ──────────────────────────────────────────────
    // Sample Contacts
    // ──────────────────────────────────────────────

    public async Task SeedSampleContactsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Seeding sample contacts...");

        if (_context.Contacts.Any())
            return;

        var contacts = new List<Contact>
        {
            new()
            {
                ContactType = ContactType.Employee,
                Salutation = "Mr.",
                FirstName = "Michael",
                MiddleName = "David",
                LastName = "Johnson",
                JobTitle = "Sales Manager",
                Department = "Sales",
                Company = "Tech Corp",
                EmailPrimary = "michael.johnson@company.com",
                PhonePrimary = "+1-555-0101",
                Address = "123 Business Ave",
                City = "San Francisco",
                State = "CA",
                ZipCode = "94105",
                Country = "USA",
                DateOfBirth = new DateTime(1985, 3, 15),
                Notes = "Key account manager for enterprise clients. Handles major negotiations and maintains relationships with Fortune 500 accounts.",
                SocialMediaLinks = new List<SocialMediaLink>
                {
                    new() { Platform = SocialPlatform.LinkedIn, Url = "https://linkedin.com/in/michaeljohnson", Handle = "michaeljohnson" },
                    new() { Platform = SocialPlatform.Twitter, Url = "https://twitter.com/mjohnson", Handle = "mjohnson" }
                }
            },
            new()
            {
                ContactType = ContactType.Customer,
                Salutation = "Ms.",
                FirstName = "Sarah",
                LastName = "Williams",
                JobTitle = "Procurement Director",
                Company = "ClientCorp",
                EmailPrimary = "sarah.williams@clientcorp.com",
                PhonePrimary = "+1-555-0102",
                Address = "456 Client Street",
                City = "New York",
                State = "NY",
                ZipCode = "10001",
                Country = "USA",
                SocialMediaLinks = new List<SocialMediaLink>
                {
                    new() { Platform = SocialPlatform.LinkedIn, Url = "https://linkedin.com/in/sarahwilliams", Handle = "sarahwilliams" }
                }
            },
            new()
            {
                ContactType = ContactType.Partner,
                Salutation = "Mr.",
                FirstName = "Robert",
                LastName = "Martinez",
                JobTitle = "Partnership Manager",
                Company = "PartnerCorp",
                EmailPrimary = "robert.martinez@partnercorp.com",
                PhonePrimary = "+1-555-0103",
                Address = "789 Partner Blvd",
                City = "Austin",
                State = "TX",
                ZipCode = "78701",
                Country = "USA",
                SocialMediaLinks = new List<SocialMediaLink>
                {
                    new() { Platform = SocialPlatform.Website, Url = "https://www.partnerco.com", Handle = "PartnerCorp" }
                }
            },
            new()
            {
                ContactType = ContactType.Lead,
                Salutation = "Ms.",
                FirstName = "Emily",
                LastName = "Chen",
                JobTitle = "VP of Operations",
                Company = "Prospect Inc",
                EmailPrimary = "emily.chen@prospect.com",
                PhonePrimary = "+1-555-0104",
                Address = "321 Prospect Lane",
                City = "Seattle",
                State = "WA",
                ZipCode = "98101",
                Country = "USA",
                Notes = "Qualified lead - interested in enterprise solution",
                SocialMediaLinks = new List<SocialMediaLink>
                {
                    new() { Platform = SocialPlatform.LinkedIn, Url = "https://linkedin.com/in/emilychen", Handle = "emilychen" }
                }
            },
            new()
            {
                ContactType = ContactType.Employee,
                Salutation = "Mr.",
                FirstName = "David",
                LastName = "Anderson",
                JobTitle = "Account Executive",
                Department = "Sales",
                Company = "Tech Corp",
                EmailPrimary = "david.anderson@company.com",
                EmailSecondary = "danderson@company.com",
                PhonePrimary = "+1-555-0105",
                Address = "123 Business Ave",
                City = "San Francisco",
                State = "CA",
                ZipCode = "94105",
                Country = "USA",
                DateOfBirth = new DateTime(1990, 7, 22),
                ReportsTo = "Michael Johnson",
                SocialMediaLinks = new List<SocialMediaLink>
                {
                    new() { Platform = SocialPlatform.GitHub, Url = "https://github.com/danderson", Handle = "danderson" }
                }
            },
            new()
            {
                ContactType = ContactType.Vendor,
                Salutation = "Ms.",
                FirstName = "Lisa",
                LastName = "Thompson",
                JobTitle = "Account Manager",
                Company = "Software Vendor Inc",
                EmailPrimary = "lisa.thompson@vendor.com",
                PhonePrimary = "+1-555-0106",
                Address = "555 Vendor Way",
                City = "Boston",
                State = "MA",
                ZipCode = "02101",
                Country = "USA",
                SocialMediaLinks = new List<SocialMediaLink>
                {
                    new() { Platform = SocialPlatform.LinkedIn, Url = "https://linkedin.com/in/lisathompson", Handle = "lisathompson" },
                    new() { Platform = SocialPlatform.Website, Url = "https://www.softwarevendor.com", Handle = "info" }
                }
            }
        };

        _context.Contacts.AddRange(contacts);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded {Count} sample contacts", contacts.Count);
    }

    // ──────────────────────────────────────────────
    // System Settings (NavOrderConfig)
    // ──────────────────────────────────────────────

    public async Task SeedSystemSettingsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Seeding system settings...");

        var navConfig = @"{
            ""navItems"": [
                { ""id"": ""dashboard"", ""group"": ""main"", ""order"": 0 },
                { ""id"": ""accounts"", ""group"": ""main"", ""order"": 1 },
                { ""id"": ""account-overview"", ""group"": ""main"", ""order"": 2 },
                { ""id"": ""contacts"", ""group"": ""main"", ""order"": 3 },
                { ""id"": ""relationships"", ""group"": ""main"", ""order"": 4 },
                { ""id"": ""leads"", ""group"": ""sales"", ""order"": 5 },
                { ""id"": ""opportunities"", ""group"": ""sales"", ""order"": 6 },
                { ""id"": ""products"", ""group"": ""sales"", ""order"": 7 },
                { ""id"": ""services"", ""group"": ""support"", ""order"": 8 },
                { ""id"": ""service-requests"", ""group"": ""support"", ""order"": 9 },
                { ""id"": ""campaigns"", ""group"": ""sales"", ""order"": 10 },
                { ""id"": ""campaign-execution"", ""group"": ""sales"", ""order"": 11 },
                { ""id"": ""quotes"", ""group"": ""sales"", ""order"": 12 },
                { ""id"": ""my-queue"", ""group"": ""productivity"", ""order"": 13 },
                { ""id"": ""activities"", ""group"": ""productivity"", ""order"": 14 },
                { ""id"": ""notes"", ""group"": ""productivity"", ""order"": 15 },
                { ""id"": ""communications"", ""group"": ""productivity"", ""order"": 16 },
                { ""id"": ""interactions"", ""group"": ""productivity"", ""order"": 17 },
                { ""id"": ""about"", ""group"": ""info"", ""order"": 18 },
                { ""id"": ""help"", ""group"": ""info"", ""order"": 19 },
                { ""id"": ""licenses"", ""group"": ""info"", ""order"": 20 },
                { ""id"": ""workflows"", ""group"": ""admin"", ""order"": 21 },
                { ""id"": ""channel-settings"", ""group"": ""admin"", ""order"": 22 },
                { ""id"": ""settings"", ""group"": ""admin"", ""order"": 23 }
            ],
            ""categories"": [
                { ""id"": ""main"", ""label"": ""Main"", ""order"": 0 },
                { ""id"": ""sales"", ""label"": ""Sales & Marketing"", ""order"": 1 },
                { ""id"": ""support"", ""label"": ""Customer Support"", ""order"": 2 },
                { ""id"": ""productivity"", ""label"": ""Productivity"", ""order"": 3 },
                { ""id"": ""info"", ""label"": ""Help & Info"", ""order"": 4 },
                { ""id"": ""admin"", ""label"": ""Administration"", ""order"": 5 }
            ]
        }".Replace(" ", "").Replace("\n", "").Replace("\r", "");

        var settings = await _context.SystemSettings.FirstOrDefaultAsync(cancellationToken);

        if (settings == null)
        {
            settings = new SystemSettings
            {
                NavOrderConfig = navConfig,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            };
            _context.SystemSettings.Add(settings);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Created system settings with NavOrderConfig");
        }
        else if (string.IsNullOrEmpty(settings.NavOrderConfig))
        {
            settings.NavOrderConfig = navConfig;
            settings.UpdatedAt = DateTime.UtcNow;
            settings.LastModified = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Updated system settings NavOrderConfig");
        }
    }

    // ──────────────────────────────────────────────
    // Module Field Configurations
    // ──────────────────────────────────────────────

    public async Task SeedModuleFieldConfigurationsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Seeding module field configurations (delegating to ModuleFieldConfigurationService)...");

        if (await _context.ModuleFieldConfigurations.AnyAsync(cancellationToken))
        {
            _logger.LogInformation("Module field configurations already exist, skipping seed");
            return;
        }

        var results = await _fieldConfigService.InitializeAllModulesAsync();
        var total = results.Values.Sum();
        _logger.LogInformation("Seeded {Total} module field configurations across {Count} modules", total, results.Count);
    }

    public async Task ForceReseedModuleFieldConfigurationsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Force re-seeding module field configurations (delegating to ModuleFieldConfigurationService)...");
        var results = await _fieldConfigService.ForceReinitializeAllAsync();
        var total = results.Values.Sum();
        _logger.LogInformation("Force re-seeded {Total} module field configurations across {Count} modules: {Details}",
            total, results.Count, string.Join(", ", results.Select(r => $"{r.Key}={r.Value}")));
    }

    // ──────────────────────────────────────────────
    // Additional Master Data (5 categories)
    // ──────────────────────────────────────────────

    public async Task SeedAdditionalMasterDataAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Seeding additional master data...");

        // ── AddressType ──
        var addressTypeCat = await _context.LookupCategories.FirstOrDefaultAsync(c => c.Name == "AddressType", cancellationToken);
        if (addressTypeCat == null)
        {
            addressTypeCat = new LookupCategory
            {
                Name = "AddressType",
                Description = "Types of addresses (Primary, Billing, Shipping, Work, Home, etc.)",
                IsActive = true
            };
            _context.LookupCategories.Add(addressTypeCat);
            await _context.SaveChangesAsync(cancellationToken);

            var addressTypes = new List<LookupItem>
            {
                new() { LookupCategoryId = addressTypeCat.Id, Key = "Primary", Value = "Primary", Meta = "{\"icon\":\"🏠\"}", SortOrder = 1, IsActive = true },
                new() { LookupCategoryId = addressTypeCat.Id, Key = "Billing", Value = "Billing", Meta = "{\"icon\":\"💳\"}", SortOrder = 2, IsActive = true },
                new() { LookupCategoryId = addressTypeCat.Id, Key = "Shipping", Value = "Shipping", Meta = "{\"icon\":\"📦\"}", SortOrder = 3, IsActive = true },
                new() { LookupCategoryId = addressTypeCat.Id, Key = "Work", Value = "Work", Meta = "{\"icon\":\"🏢\"}", SortOrder = 4, IsActive = true },
                new() { LookupCategoryId = addressTypeCat.Id, Key = "Home", Value = "Home", Meta = "{\"icon\":\"🏡\"}", SortOrder = 5, IsActive = true },
                new() { LookupCategoryId = addressTypeCat.Id, Key = "Office", Value = "Office", Meta = "{\"icon\":\"🏬\"}", SortOrder = 6, IsActive = true },
                new() { LookupCategoryId = addressTypeCat.Id, Key = "Headquarters", Value = "Headquarters", Meta = "{\"icon\":\"🏛️\"}", SortOrder = 7, IsActive = true },
                new() { LookupCategoryId = addressTypeCat.Id, Key = "Branch", Value = "Branch", Meta = "{\"icon\":\"🏪\"}", SortOrder = 8, IsActive = true },
                new() { LookupCategoryId = addressTypeCat.Id, Key = "Store", Value = "Store", Meta = "{\"icon\":\"🛒\"}", SortOrder = 9, IsActive = true },
                new() { LookupCategoryId = addressTypeCat.Id, Key = "Factory", Value = "Factory", Meta = "{\"icon\":\"🏭\"}", SortOrder = 10, IsActive = true },
                new() { LookupCategoryId = addressTypeCat.Id, Key = "Warehouse", Value = "Warehouse", Meta = "{\"icon\":\"📦\"}", SortOrder = 11, IsActive = true },
                new() { LookupCategoryId = addressTypeCat.Id, Key = "Other", Value = "Other", Meta = "{\"icon\":\"📍\"}", SortOrder = 99, IsActive = true },
            };
            _context.LookupItems.AddRange(addressTypes);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // ── ContactMethodType ──
        var contactMethodTypeCat = await _context.LookupCategories.FirstOrDefaultAsync(c => c.Name == "ContactMethodType", cancellationToken);
        if (contactMethodTypeCat == null)
        {
            contactMethodTypeCat = new LookupCategory
            {
                Name = "ContactMethodType",
                Description = "Types of contact methods (Work, Home, Mobile, Personal, Other)",
                IsActive = true
            };
            _context.LookupCategories.Add(contactMethodTypeCat);
            await _context.SaveChangesAsync(cancellationToken);

            var contactMethodTypes = new List<LookupItem>
            {
                new() { LookupCategoryId = contactMethodTypeCat.Id, Key = "Work", Value = "Work", Meta = "{\"icon\":\"🏢\"}", SortOrder = 1, IsActive = true },
                new() { LookupCategoryId = contactMethodTypeCat.Id, Key = "Home", Value = "Home", Meta = "{\"icon\":\"🏠\"}", SortOrder = 2, IsActive = true },
                new() { LookupCategoryId = contactMethodTypeCat.Id, Key = "Mobile", Value = "Mobile", Meta = "{\"icon\":\"📱\"}", SortOrder = 3, IsActive = true },
                new() { LookupCategoryId = contactMethodTypeCat.Id, Key = "Personal", Value = "Personal", Meta = "{\"icon\":\"👤\"}", SortOrder = 4, IsActive = true },
                new() { LookupCategoryId = contactMethodTypeCat.Id, Key = "Other", Value = "Other", Meta = "{\"icon\":\"📋\"}", SortOrder = 99, IsActive = true },
            };
            _context.LookupItems.AddRange(contactMethodTypes);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // ── ContactPriority ──
        var contactPriorityCat = await _context.LookupCategories.FirstOrDefaultAsync(c => c.Name == "ContactPriority", cancellationToken);
        if (contactPriorityCat == null)
        {
            contactPriorityCat = new LookupCategory
            {
                Name = "ContactPriority",
                Description = "Priority of contact information (Primary, Secondary, Other)",
                IsActive = true
            };
            _context.LookupCategories.Add(contactPriorityCat);
            await _context.SaveChangesAsync(cancellationToken);

            var contactPriorities = new List<LookupItem>
            {
                new() { LookupCategoryId = contactPriorityCat.Id, Key = "Primary", Value = "Primary", Meta = "{\"icon\":\"⭐\"}", SortOrder = 1, IsActive = true },
                new() { LookupCategoryId = contactPriorityCat.Id, Key = "Secondary", Value = "Secondary", Meta = "{\"icon\":\"✦\"}", SortOrder = 2, IsActive = true },
                new() { LookupCategoryId = contactPriorityCat.Id, Key = "Other", Value = "Other", Meta = "{\"icon\":\"○\"}", SortOrder = 99, IsActive = true },
            };
            _context.LookupItems.AddRange(contactPriorities);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // ── AccountLocationType ──
        var accountLocationTypeCat = await _context.LookupCategories.FirstOrDefaultAsync(c => c.Name == "AccountLocationType", cancellationToken);
        if (accountLocationTypeCat == null)
        {
            accountLocationTypeCat = new LookupCategory
            {
                Name = "AccountLocationType",
                Description = "Types of account/corporate locations (Office, HQ, Branch, Store, Factory, Warehouse, etc.)",
                IsActive = true
            };
            _context.LookupCategories.Add(accountLocationTypeCat);
            await _context.SaveChangesAsync(cancellationToken);

            var accountLocationTypes = new List<LookupItem>
            {
                new() { LookupCategoryId = accountLocationTypeCat.Id, Key = "Office", Value = "Office", Meta = "{\"icon\":\"🏢\",\"description\":\"General office location\"}", SortOrder = 1, IsActive = true },
                new() { LookupCategoryId = accountLocationTypeCat.Id, Key = "Headquarters", Value = "Headquarters", Meta = "{\"icon\":\"🏛️\",\"description\":\"Main headquarters\"}", SortOrder = 2, IsActive = true },
                new() { LookupCategoryId = accountLocationTypeCat.Id, Key = "RegionalHQ", Value = "Regional HQ", Meta = "{\"icon\":\"🌍\",\"description\":\"Regional headquarters\"}", SortOrder = 3, IsActive = true },
                new() { LookupCategoryId = accountLocationTypeCat.Id, Key = "Branch", Value = "Branch", Meta = "{\"icon\":\"🏬\",\"description\":\"Branch office\"}", SortOrder = 4, IsActive = true },
                new() { LookupCategoryId = accountLocationTypeCat.Id, Key = "Store", Value = "Store", Meta = "{\"icon\":\"🛒\",\"description\":\"Retail store\"}", SortOrder = 5, IsActive = true },
                new() { LookupCategoryId = accountLocationTypeCat.Id, Key = "Factory", Value = "Factory", Meta = "{\"icon\":\"🏭\",\"description\":\"Manufacturing plant\"}", SortOrder = 6, IsActive = true },
                new() { LookupCategoryId = accountLocationTypeCat.Id, Key = "Warehouse", Value = "Warehouse", Meta = "{\"icon\":\"📦\",\"description\":\"Storage warehouse\"}", SortOrder = 7, IsActive = true },
                new() { LookupCategoryId = accountLocationTypeCat.Id, Key = "DistributionCenter", Value = "Distribution Center", Meta = "{\"icon\":\"🚚\",\"description\":\"Distribution hub\"}", SortOrder = 8, IsActive = true },
                new() { LookupCategoryId = accountLocationTypeCat.Id, Key = "DataCenter", Value = "Data Center", Meta = "{\"icon\":\"🖥️\",\"description\":\"Data center facility\"}", SortOrder = 9, IsActive = true },
                new() { LookupCategoryId = accountLocationTypeCat.Id, Key = "RDCenter", Value = "R&D Center", Meta = "{\"icon\":\"🔬\",\"description\":\"Research & development\"}", SortOrder = 10, IsActive = true },
                new() { LookupCategoryId = accountLocationTypeCat.Id, Key = "Other", Value = "Other", Meta = "{\"icon\":\"📍\",\"description\":\"Other location type\"}", SortOrder = 99, IsActive = true },
            };
            _context.LookupItems.AddRange(accountLocationTypes);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // ── SocialMediaPlatform ──
        var socialMediaPlatformCat = await _context.LookupCategories.FirstOrDefaultAsync(c => c.Name == "SocialMediaPlatform", cancellationToken);
        if (socialMediaPlatformCat == null)
        {
            socialMediaPlatformCat = new LookupCategory
            {
                Name = "SocialMediaPlatform",
                Description = "Social media platforms (LinkedIn, Twitter, Facebook, etc.)",
                IsActive = true
            };
            _context.LookupCategories.Add(socialMediaPlatformCat);
            await _context.SaveChangesAsync(cancellationToken);

            var socialMediaPlatforms = new List<LookupItem>
            {
                new() { LookupCategoryId = socialMediaPlatformCat.Id, Key = "LinkedIn", Value = "LinkedIn", Meta = "{\"icon\":\"💼\",\"urlPrefix\":\"https://linkedin.com/in/\"}", SortOrder = 1, IsActive = true },
                new() { LookupCategoryId = socialMediaPlatformCat.Id, Key = "Twitter", Value = "Twitter/X", Meta = "{\"icon\":\"🐦\",\"urlPrefix\":\"https://twitter.com/\"}", SortOrder = 2, IsActive = true },
                new() { LookupCategoryId = socialMediaPlatformCat.Id, Key = "Facebook", Value = "Facebook", Meta = "{\"icon\":\"📘\",\"urlPrefix\":\"https://facebook.com/\"}", SortOrder = 3, IsActive = true },
                new() { LookupCategoryId = socialMediaPlatformCat.Id, Key = "Instagram", Value = "Instagram", Meta = "{\"icon\":\"📷\",\"urlPrefix\":\"https://instagram.com/\"}", SortOrder = 4, IsActive = true },
                new() { LookupCategoryId = socialMediaPlatformCat.Id, Key = "YouTube", Value = "YouTube", Meta = "{\"icon\":\"🎬\",\"urlPrefix\":\"https://youtube.com/@\"}", SortOrder = 5, IsActive = true },
                new() { LookupCategoryId = socialMediaPlatformCat.Id, Key = "TikTok", Value = "TikTok", Meta = "{\"icon\":\"🎵\",\"urlPrefix\":\"https://tiktok.com/@\"}", SortOrder = 6, IsActive = true },
                new() { LookupCategoryId = socialMediaPlatformCat.Id, Key = "GitHub", Value = "GitHub", Meta = "{\"icon\":\"💻\",\"urlPrefix\":\"https://github.com/\"}", SortOrder = 7, IsActive = true },
                new() { LookupCategoryId = socialMediaPlatformCat.Id, Key = "Website", Value = "Website", Meta = "{\"icon\":\"🌐\",\"urlPrefix\":\"\"}", SortOrder = 8, IsActive = true },
                new() { LookupCategoryId = socialMediaPlatformCat.Id, Key = "Other", Value = "Other", Meta = "{\"icon\":\"🔗\",\"urlPrefix\":\"\"}", SortOrder = 99, IsActive = true },
            };
            _context.LookupItems.AddRange(socialMediaPlatforms);
            await _context.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation("Additional master data seeding complete");
    }
}

