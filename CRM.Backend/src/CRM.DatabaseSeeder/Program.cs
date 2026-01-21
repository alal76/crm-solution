using CRM.Core.Entities;
using CRM.Core.Models;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace CRM.DatabaseSeeder;

class Program
{
    static async Task<int> Main(string[] args)
    {
        Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║           CRM Database Seeder & Initializer               ║");
        Console.WriteLine("║                    Version 1.0.0                          ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();

        // Setup logging
        var serviceProvider = new ServiceCollection()
            .AddLogging(builder =>
            {
                builder.AddConfiguration(configuration.GetSection("Logging"));
                builder.AddConsole();
            })
            .BuildServiceProvider();

        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<Program>();

        try
        {
            // Parse command line arguments
            var command = args.Length > 0 ? args[0].ToLowerInvariant() : "seed";

            switch (command)
            {
                case "create":
                    await CreateDatabaseAsync(configuration, logger);
                    break;
                case "seed":
                    await SeedDatabaseAsync(configuration, logger);
                    break;
                case "reset":
                    await ResetDatabaseAsync(configuration, logger);
                    break;
                case "admin":
                    await CreateAdminUserAsync(configuration, logger);
                    break;
                case "help":
                case "--help":
                case "-h":
                    ShowHelp();
                    break;
                default:
                    Console.WriteLine($"Unknown command: {command}");
                    ShowHelp();
                    return 1;
            }

            return 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during database seeding");
            Console.WriteLine($"\n❌ Error: {ex.Message}");
            return 1;
        }
    }

    static void ShowHelp()
    {
        Console.WriteLine("Usage: CRM.DatabaseSeeder [command] [options]");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  create    Create the database schema (runs migrations)");
        Console.WriteLine("  seed      Create schema and seed with sample data (default)");
        Console.WriteLine("  reset     Drop and recreate database with fresh data");
        Console.WriteLine("  admin     Create only the admin user (no sample data)");
        Console.WriteLine("  help      Show this help message");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --DatabaseProvider=<provider>  Database provider (sqlite, sqlserver, postgresql, mysql)");
        Console.WriteLine("  --ConnectionStrings:DefaultConnection=<connection>  Database connection string");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  dotnet run seed");
        Console.WriteLine("  dotnet run create --DatabaseProvider=postgresql");
        Console.WriteLine("  dotnet run admin --ConnectionStrings:DefaultConnection=\"Server=localhost;Database=crm;...\"");
    }

    static async Task CreateDatabaseAsync(IConfiguration configuration, ILogger logger)
    {
        logger.LogInformation("Creating database schema...");

        var dbContext = CreateDbContext(configuration);
        
        // Ensure database is created
        var created = await dbContext.Database.EnsureCreatedAsync();
        
        if (created)
        {
            Console.WriteLine("✅ Database created successfully");
        }
        else
        {
            Console.WriteLine("✅ Database already exists");
        }

        var provider = configuration["DatabaseProvider"] ?? "sqlite";
        Console.WriteLine($"   Provider: {provider}");
        Console.WriteLine($"   Connection: {GetMaskedConnectionString(configuration)}");
    }

    static async Task SeedDatabaseAsync(IConfiguration configuration, ILogger logger)
    {
        // First create the database
        await CreateDatabaseAsync(configuration, logger);

        Console.WriteLine("\n📊 Seeding database with sample data...\n");

        var dbContext = CreateDbContext(configuration);

        // Seed admin user
        await SeedAdminUser(dbContext, configuration, logger);

        // Seed sample data
        await SeedSampleData(dbContext, configuration, logger);

        Console.WriteLine("\n✅ Database seeding completed successfully!");
    }

    static async Task ResetDatabaseAsync(IConfiguration configuration, ILogger logger)
    {
        Console.WriteLine("⚠️  This will DELETE all existing data. Are you sure? (yes/no): ");
        var confirmation = Console.ReadLine();

        if (confirmation?.ToLowerInvariant() != "yes")
        {
            Console.WriteLine("Operation cancelled.");
            return;
        }

        var dbContext = CreateDbContext(configuration);

        logger.LogInformation("Dropping database...");
        await dbContext.Database.EnsureDeletedAsync();
        Console.WriteLine("✅ Database dropped");

        await SeedDatabaseAsync(configuration, logger);
    }

    static async Task CreateAdminUserAsync(IConfiguration configuration, ILogger logger)
    {
        await CreateDatabaseAsync(configuration, logger);

        var dbContext = CreateDbContext(configuration);
        await SeedAdminUser(dbContext, configuration, logger);
    }

    static async Task SeedAdminUser(CrmDbContext context, IConfiguration configuration, ILogger logger)
    {
        Console.WriteLine("👤 Creating admin user...");

        var adminConfig = configuration.GetSection("Seeding:AdminUser");
        var username = adminConfig["Username"] ?? "admin";
        var email = adminConfig["Email"] ?? "admin@crm.local";
        var firstName = adminConfig["FirstName"] ?? "System";
        var lastName = adminConfig["LastName"] ?? "Administrator";
        var password = adminConfig["Password"] ?? "Admin@123";

        // Check if admin already exists
        var existingAdmin = await context.Users.FirstOrDefaultAsync(u => u.Username == username || u.Email == email);
        
        if (existingAdmin != null)
        {
            Console.WriteLine($"   Admin user already exists: {existingAdmin.Username} ({existingAdmin.Email})");
            return;
        }

        var adminUser = new User
        {
            Username = username,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            PasswordHash = HashPassword(password),
            Role = (int)UserRole.Admin,
            IsActive = true,
            EmailVerified = true,
            TwoFactorEnabled = false,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(adminUser);
        await context.SaveChangesAsync();

        Console.WriteLine($"   ✅ Admin user created:");
        Console.WriteLine($"      Username: {username}");
        Console.WriteLine($"      Email: {email}");
        Console.WriteLine($"      Password: {password}");
        Console.WriteLine($"      Role: Admin");
    }

    static async Task SeedSampleData(CrmDbContext context, IConfiguration configuration, ILogger logger)
    {
        var seedingConfig = configuration.GetSection("Seeding:SampleData");
        
        // Create Departments
        Console.WriteLine("🏢 Creating departments...");
        var departments = await SeedDepartments(context);

        // Create additional users
        Console.WriteLine("👥 Creating sample users...");
        var users = await SeedUsers(context, departments);

        // Create Customers
        var customerCount = int.Parse(seedingConfig["Customers"] ?? "50");
        Console.WriteLine($"👤 Creating {customerCount} customers...");
        var customers = await SeedCustomers(context, users, customerCount);

        // Create Products
        var productCount = int.Parse(seedingConfig["Products"] ?? "25");
        Console.WriteLine($"📦 Creating {productCount} products...");
        var products = await SeedProducts(context, productCount);

        // Create Contacts
        var contactCount = int.Parse(seedingConfig["Contacts"] ?? "100");
        Console.WriteLine($"📇 Creating {contactCount} contacts...");
        var contacts = await SeedContacts(context, customers, contactCount);

        // Create Campaigns
        var campaignCount = int.Parse(seedingConfig["Campaigns"] ?? "10");
        Console.WriteLine($"📢 Creating {campaignCount} campaigns...");
        var campaigns = await SeedCampaigns(context, users, campaignCount);

        // Create Leads (linked to campaigns)
        var leadCount = int.Parse(seedingConfig["Leads"] ?? "150");
        Console.WriteLine($"🎯 Creating {leadCount} leads...");
        var leads = await SeedLeads(context, campaigns, users, products, leadCount);

        // Create Opportunities
        var opportunityCount = int.Parse(seedingConfig["Opportunities"] ?? "30");
        Console.WriteLine($"💰 Creating {opportunityCount} opportunities...");
        var opportunities = await SeedOpportunities(context, customers, products, users, campaigns, opportunityCount);

        // Create Tasks
        var taskCount = int.Parse(seedingConfig["Tasks"] ?? "50");
        Console.WriteLine($"✅ Creating {taskCount} tasks...");
        await SeedTasks(context, customers, opportunities, users, taskCount);

        // Create Notes
        var noteCount = int.Parse(seedingConfig["Notes"] ?? "75");
        Console.WriteLine($"📝 Creating {noteCount} notes...");
        await SeedNotes(context, customers, opportunities, users, noteCount);

        // Create Quotes
        var quoteCount = int.Parse(seedingConfig["Quotes"] ?? "20");
        Console.WriteLine($"📋 Creating {quoteCount} quotes...");
        await SeedQuotes(context, customers, opportunities, users, quoteCount);

        // Create Interactions
        var interactionCount = int.Parse(seedingConfig["Interactions"] ?? "100");
        Console.WriteLine($"💬 Creating {interactionCount} interactions...");
        await SeedInteractions(context, customers, contacts, users, interactionCount);

        // Create Activities
        Console.WriteLine("📊 Creating activity log...");
        await SeedActivities(context, users, customers, opportunities, 50);
    }

    static async Task<List<Department>> SeedDepartments(CrmDbContext context)
    {
        if (await context.Departments.AnyAsync())
        {
            return await context.Departments.ToListAsync();
        }

        var departments = new List<Department>
        {
            new() { Name = "Sales", Description = "Sales and business development", DepartmentCode = "SALES", IsActive = true },
            new() { Name = "Marketing", Description = "Marketing and campaigns", DepartmentCode = "MKTG", IsActive = true },
            new() { Name = "Support", Description = "Customer support and service", DepartmentCode = "SUPP", IsActive = true },
            new() { Name = "Engineering", Description = "Product development", DepartmentCode = "ENG", IsActive = true },
            new() { Name = "Finance", Description = "Finance and accounting", DepartmentCode = "FIN", IsActive = true }
        };

        context.Departments.AddRange(departments);
        await context.SaveChangesAsync();
        Console.WriteLine($"   Created {departments.Count} departments");
        return departments;
    }

    static async Task<List<User>> SeedUsers(CrmDbContext context, List<Department> departments)
    {
        var existingUsers = await context.Users.ToListAsync();
        if (existingUsers.Count > 1)
        {
            return existingUsers;
        }

        var users = new List<User>
        {
            new() { Username = "jsmith", Email = "john.smith@company.com", FirstName = "John", LastName = "Smith", PasswordHash = HashPassword("User@123"), Role = (int)UserRole.Manager, IsActive = true, EmailVerified = true, DepartmentId = departments.First(d => d.DepartmentCode == "SALES").Id },
            new() { Username = "mwilson", Email = "mary.wilson@company.com", FirstName = "Mary", LastName = "Wilson", PasswordHash = HashPassword("User@123"), Role = (int)UserRole.Sales, IsActive = true, EmailVerified = true, DepartmentId = departments.First(d => d.DepartmentCode == "SALES").Id },
            new() { Username = "djones", Email = "david.jones@company.com", FirstName = "David", LastName = "Jones", PasswordHash = HashPassword("User@123"), Role = (int)UserRole.Sales, IsActive = true, EmailVerified = true, DepartmentId = departments.First(d => d.DepartmentCode == "SALES").Id },
            new() { Username = "sbrown", Email = "sarah.brown@company.com", FirstName = "Sarah", LastName = "Brown", PasswordHash = HashPassword("User@123"), Role = (int)UserRole.Manager, IsActive = true, EmailVerified = true, DepartmentId = departments.First(d => d.DepartmentCode == "MKTG").Id },
            new() { Username = "mgarcia", Email = "mike.garcia@company.com", FirstName = "Mike", LastName = "Garcia", PasswordHash = HashPassword("User@123"), Role = (int)UserRole.Support, IsActive = true, EmailVerified = true, DepartmentId = departments.First(d => d.DepartmentCode == "SUPP").Id }
        };

        context.Users.AddRange(users);
        await context.SaveChangesAsync();
        Console.WriteLine($"   Created {users.Count} users");
        return await context.Users.ToListAsync();
    }

    static async Task<List<Customer>> SeedCustomers(CrmDbContext context, List<User> users, int count)
    {
        if (await context.Customers.AnyAsync())
        {
            return await context.Customers.ToListAsync();
        }

        var faker = new Bogus.Faker();
        var customers = new List<Customer>();
        var salesUsers = users.Where(u => u.Role == (int)UserRole.Sales || u.Role == (int)UserRole.Manager).ToList();

        for (int i = 0; i < count; i++)
        {
            var customer = new Customer
            {
                FirstName = faker.Name.FirstName(),
                LastName = faker.Name.LastName(),
                Email = faker.Internet.Email(),
                Phone = faker.Phone.PhoneNumber("+1-###-###-####"),
                MobilePhone = faker.Phone.PhoneNumber("+1-###-###-####"),
                Company = faker.Company.CompanyName(),
                JobTitle = faker.Name.JobTitle(),
                Website = faker.Internet.Url(),
                Address = faker.Address.StreetAddress(),
                City = faker.Address.City(),
                State = faker.Address.StateAbbr(),
                ZipCode = faker.Address.ZipCode(),
                Country = "USA",
                Industry = faker.Commerce.Categories(1)[0],
                NumberOfEmployees = faker.Random.Int(10, 5000),
                AnnualRevenue = faker.Random.Decimal(100000, 10000000),
                CustomerType = (CustomerType)faker.Random.Int(0, 5),
                Priority = (CustomerPriority)faker.Random.Int(0, 3),
                LifecycleStage = (CustomerLifecycleStage)faker.Random.Int(0, 5),
                LeadSource = faker.PickRandom(new[] { "Website", "Referral", "LinkedIn", "Trade Show", "Cold Call", "Advertisement" }),
                FirstContactDate = faker.Date.Past(2),
                LeadScore = faker.Random.Int(0, 100),
                CustomerHealthScore = faker.Random.Int(30, 100),
                OptInEmail = faker.Random.Bool(0.8f),
                OptInPhone = faker.Random.Bool(0.6f),
                AssignedToUserId = faker.PickRandom(salesUsers).Id,
                Tags = string.Join(",", faker.Commerce.Categories(faker.Random.Int(1, 3))),
                Notes = faker.Lorem.Paragraph(),
                CreatedAt = faker.Date.Past(1)
            };
            customers.Add(customer);
        }

        context.Customers.AddRange(customers);
        await context.SaveChangesAsync();
        Console.WriteLine($"   Created {count} customers");
        return customers;
    }

    static async Task<List<Product>> SeedProducts(CrmDbContext context, int count)
    {
        if (await context.Products.AnyAsync())
        {
            return await context.Products.ToListAsync();
        }

        var faker = new Bogus.Faker();
        var products = new List<Product>();

        var categories = new[] { "Software", "Hardware", "Services", "Subscriptions", "Training", "Support" };
        var serviceTiers = new[] { ServiceTier.Basic, ServiceTier.Standard, ServiceTier.Professional, ServiceTier.Enterprise, ServiceTier.Premium };
        var pricingModels = new[] { PricingModel.FixedPrice, PricingModel.TieredPricing, PricingModel.PerUser, PricingModel.Hourly, PricingModel.UsageBased };

        for (int i = 0; i < count; i++)
        {
            var price = faker.Random.Decimal(99, 9999);
            var cost = price * 0.4m;
            var productType = (ProductType)faker.Random.Int(0, 12);
            var isService = productType == ProductType.Service || productType == ProductType.Consulting || 
                           productType == ProductType.ManagedService || productType == ProductType.SupportContract ||
                           productType == ProductType.Training || productType == ProductType.ProfessionalServices;
            var isSubscription = productType == ProductType.Subscription || productType == ProductType.ManagedService;
            
            var product = new Product
            {
                // Basic Information
                Name = faker.Commerce.ProductName(),
                Description = faker.Commerce.ProductDescription(),
                ShortDescription = faker.Commerce.ProductAdjective() + " " + faker.Commerce.Product(),
                SKU = $"{(isService ? "SVC" : "PRD")}-{faker.Random.AlphaNumeric(8).ToUpper()}",
                ProductCode = $"PC-{faker.Random.Int(10000, 99999)}",
                Barcode = isService ? null : faker.Commerce.Ean13(),
                ExternalId = faker.Random.Bool(0.3f) ? $"EXT-{faker.Random.Int(1000, 9999)}" : null,
                InternalReference = $"{(isService ? "SVC" : "PRD")}-{faker.Random.Int(100000, 999999)}",
                
                // Classification
                ProductType = productType,
                Status = ProductStatus.Active,
                Category = faker.PickRandom(categories),
                SubCategory = faker.Commerce.Categories(1)[0],
                ProductFamily = faker.PickRandom(new[] { "Enterprise", "Professional", "Starter", "Custom" }),
                Brand = faker.Company.CompanyName(),
                Manufacturer = isService ? null : faker.Company.CompanyName(),
                Tags = string.Join(",", faker.Commerce.Categories(3)),
                ServiceTier = isService ? faker.PickRandom(serviceTiers) : ServiceTier.Standard,
                IsService = isService,
                IsSubscription = isSubscription,
                
                // Unit Pricing
                Price = price,
                ListPrice = price * 1.2m,
                Cost = cost,
                MinimumPrice = price * 0.7m,
                WholesalePrice = price * 0.75m,
                PartnerPrice = price * 0.8m,
                Margin = price - cost,
                TargetMargin = 40m,
                UnitOfMeasure = isService ? UnitOfMeasure.Hour : UnitOfMeasure.Each,
                MinimumQuantity = 1,
                MaximumQuantity = isService ? 1000 : 10000,
                QuantityIncrement = 1,
                
                // Contract Pricing (for services)
                WeeklyPrice = isService ? price * 0.3m : null,
                MonthlyPrice = isService ? price : null,
                QuarterlyPrice = isService ? price * 2.7m : null,
                SemiAnnualPrice = isService ? price * 5m : null,
                AnnualPrice = isService ? price * 9m : null,
                TwoYearPrice = isService ? price * 16m : null,
                ThreeYearPrice = isService ? price * 21m : null,
                DefaultContractTerm = isSubscription ? ContractTermCategory.Annual : ContractTermCategory.NoContract,
                MinimumContractTerm = isSubscription ? ContractTermCategory.Monthly : ContractTermCategory.NoContract,
                
                // Term Discounts
                WeeklyTermDiscount = 0,
                MonthlyTermDiscount = 0,
                QuarterlyTermDiscount = 10,
                SemiAnnualTermDiscount = 17,
                AnnualTermDiscount = 25,
                TwoYearTermDiscount = 33,
                ThreeYearTermDiscount = 42,
                MaxTermDiscount = 45,
                
                // Volume Discounts
                MaxVolumeDiscount = 25,
                MaxTotalDiscount = 50,
                
                // Subscription Fields
                BillingFrequency = isSubscription ? (BillingFrequency)faker.Random.Int(4, 7) : BillingFrequency.OneTime,
                PricingModel = faker.PickRandom(pricingModels),
                RecurringPrice = isSubscription ? price : null,
                SetupFee = isService ? faker.Random.Decimal(500, 5000) : null,
                ActivationFee = isSubscription ? faker.Random.Decimal(0, 500) : null,
                CancellationFee = isSubscription ? faker.Random.Decimal(0, 1000) : null,
                TrialPeriodDays = isSubscription ? faker.Random.Int(7, 30) : null,
                ContractLengthMonths = isSubscription ? faker.PickRandom(new[] { 12, 24, 36 }) : null,
                MinContractLengthMonths = isSubscription ? 1 : null,
                AutoRenewal = isSubscription,
                CancellationNoticeDays = isSubscription ? 30 : null,
                
                // Service Fields
                HourlyRate = isService ? faker.Random.Decimal(75, 350) : null,
                DailyRate = isService ? faker.Random.Decimal(600, 2800) : null,
                MinimumBillableHours = isService ? 1m : null,
                BillableHourIncrement = isService ? 0.25m : 0.25m,
                OvertimeMultiplier = isService ? 1.5m : null,
                WeekendMultiplier = isService ? 1.5m : null,
                HolidayMultiplier = isService ? 2m : null,
                IncludesOnsiteWork = isService && faker.Random.Bool(0.3f),
                TravelIncluded = isService && faker.Random.Bool(0.2f),
                MaterialsIncluded = isService && faker.Random.Bool(0.5f),
                DeliveryMethod = isService ? faker.PickRandom(new[] { "Remote", "On-site", "Hybrid" }) : null,
                EstimatedDuration = isService ? faker.Random.Decimal(2, 80) : null,
                DurationUnit = isService ? faker.PickRandom(new[] { "Hours", "Days", "Weeks" }) : null,
                
                // SLA Fields
                UptimeGuaranteePercent = isSubscription ? faker.PickRandom(new[] { 99.0m, 99.5m, 99.9m, 99.99m }) : null,
                ResponseTimeSlaHours = isService ? faker.PickRandom(new[] { 1m, 4m, 8m, 24m }) : null,
                ResolutionTimeSlaHours = isService ? faker.PickRandom(new[] { 8m, 24m, 48m, 72m }) : null,
                SupportHours = isService ? faker.PickRandom(new[] { "24/7", "Business Hours (9-5 EST)", "Extended (6am-10pm)" }) : null,
                IncludedSupportIncidents = isSubscription ? faker.Random.Int(5, 50) : null,
                AdditionalIncidentPrice = isSubscription ? faker.Random.Decimal(50, 250) : null,
                
                // Usage Limits
                IncludedUnits = isSubscription ? faker.Random.Int(1000, 50000) : null,
                UsageUnitType = isSubscription ? faker.PickRandom(new[] { "API Calls", "GB Storage", "Transactions", "Reports" }) : null,
                OverageUnitPrice = isSubscription ? faker.Random.Decimal(0.001m, 0.1m) : null,
                IncludedUsers = isSubscription ? faker.Random.Int(1, 25) : null,
                AdditionalUserPrice = isSubscription ? faker.Random.Decimal(5, 50) : null,
                IncludedStorageGb = isSubscription ? faker.Random.Decimal(10, 500) : null,
                AdditionalStoragePrice = isSubscription ? faker.Random.Decimal(0.05m, 0.5m) : null,
                
                // Tax & Currency
                CurrencyCode = "USD",
                IsTaxable = true,
                TaxRate = 8.25m,
                TaxCategory = faker.PickRandom(new[] { "Standard", "Reduced", "Software", "Services" }),
                RevenueRecognition = isSubscription ? RevenueRecognitionMethod.OverTime : RevenueRecognitionMethod.Immediate,
                
                // Inventory (physical products only)
                Quantity = isService ? 0 : faker.Random.Int(0, 1000),
                ReorderLevel = isService ? null : faker.Random.Int(10, 50),
                ReorderQuantity = isService ? null : faker.Random.Int(50, 200),
                MaxQuantity = isService ? null : faker.Random.Int(500, 2000),
                WarehouseLocation = isService ? null : $"WH-{faker.Random.AlphaNumeric(4).ToUpper()}",
                TrackInventory = !isService,
                AllowBackorder = !isService && faker.Random.Bool(0.3f),
                LeadTimeDays = isService ? null : faker.Random.Int(3, 21),
                
                // Physical Attributes
                Weight = isService ? null : faker.Random.Decimal(0.5m, 50m),
                WeightUnit = isService ? null : "kg",
                Length = isService ? null : faker.Random.Decimal(10, 100),
                Width = isService ? null : faker.Random.Decimal(10, 100),
                Height = isService ? null : faker.Random.Decimal(5, 50),
                DimensionUnit = isService ? null : "cm",
                ShippingClass = isService ? null : faker.PickRandom(new[] { "Standard", "Express", "Freight", "Oversized" }),
                IsHazardous = !isService && faker.Random.Bool(0.05f),
                
                // Media
                ImageUrl = $"https://picsum.photos/seed/{i}/200/200",
                ThumbnailUrl = $"https://picsum.photos/seed/{i}/100/100",
                
                // Features & Warranty
                Warranty = isService ? null : $"{faker.Random.Int(1, 5)} Year Limited Warranty",
                WarrantyMonths = isService ? null : faker.Random.Int(12, 60),
                ExtendedWarrantyAvailable = !isService && faker.Random.Bool(0.5f),
                ExtendedWarrantyPrice = !isService && faker.Random.Bool(0.5f) ? faker.Random.Decimal(50, 500) : null,
                
                // Sales Performance
                TotalSold = faker.Random.Int(0, 500),
                TotalRevenue = faker.Random.Decimal(10000, 500000),
                AverageRating = faker.Random.Double(3.0, 5.0),
                ReviewCount = faker.Random.Int(0, 100),
                IsFeatured = faker.Random.Bool(0.2f),
                IsBestSeller = faker.Random.Bool(0.1f),
                IsNew = faker.Random.Bool(0.15f),
                IsOnSale = faker.Random.Bool(0.1f),
                SalePrice = faker.Random.Bool(0.1f) ? price * 0.85m : null,
                
                // Availability
                AvailableFrom = faker.Date.Past(1),
                LastPriceUpdate = faker.Date.Recent(90),
                
                // Status
                IsActive = true,
                IsVisible = true,
                IsPurchasable = true,
                
                CreatedAt = faker.Date.Past(2)
            };
            products.Add(product);
        }

        context.Products.AddRange(products);
        await context.SaveChangesAsync();
        Console.WriteLine($"   Created {count} products");
        return products;
    }

    static async Task<List<Contact>> SeedContacts(CrmDbContext context, List<Customer> customers, int count)
    {
        if (await context.Contacts.AnyAsync())
        {
            return await context.Contacts.ToListAsync();
        }

        var faker = new Bogus.Faker();
        var contacts = new List<Contact>();

        for (int i = 0; i < count; i++)
        {
            var contact = new Contact
            {
                ContactType = (ContactType)faker.Random.Int(0, 8),
                Status = ContactStatus.Active,
                FirstName = faker.Name.FirstName(),
                LastName = faker.Name.LastName(),
                EmailPrimary = faker.Internet.Email(),
                PhonePrimary = faker.Phone.PhoneNumber("+1-###-###-####"),
                PhoneMobile = faker.Phone.PhoneNumber("+1-###-###-####"),
                JobTitle = faker.Name.JobTitle(),
                Department = faker.Commerce.Department(),
                Company = faker.Company.CompanyName(),
                Address = faker.Address.StreetAddress(),
                City = faker.Address.City(),
                State = faker.Address.StateAbbr(),
                Country = "USA",
                ZipCode = faker.Address.ZipCode(),
                LeadScore = faker.Random.Int(0, 100),
                LeadSource = faker.PickRandom(new[] { "Website", "Referral", "LinkedIn", "Trade Show", "Email Campaign" }),
                AccountId = faker.Random.Bool(0.3f) ? faker.PickRandom(customers).Id : null,
                Notes = faker.Lorem.Paragraph(),
                DateAdded = faker.Date.Past(1)
            };
            contacts.Add(contact);
        }

        context.Contacts.AddRange(contacts);
        await context.SaveChangesAsync();
        Console.WriteLine($"   Created {count} contacts");
        return contacts;
    }

    static async Task<List<MarketingCampaign>> SeedCampaigns(CrmDbContext context, List<User> users, int count)
    {
        if (await context.MarketingCampaigns.AnyAsync())
        {
            return await context.MarketingCampaigns.ToListAsync();
        }

        var faker = new Bogus.Faker();
        var campaigns = new List<MarketingCampaign>();
        var campaignTypes = Enum.GetValues<CampaignType>();
        var objectives = Enum.GetValues<CampaignObjective>();
        var audienceTypes = Enum.GetValues<AudienceType>();
        var successMetrics = Enum.GetValues<SuccessMetric>();

        for (int i = 0; i < count; i++)
        {
            var startDate = faker.Date.Past(1);
            var endDate = startDate.AddDays(faker.Random.Int(14, 90));
            var budget = faker.Random.Decimal(5000, 100000);
            var actualCost = budget * faker.Random.Decimal(0.5m, 1.2m);
            var leadsGenerated = faker.Random.Int(10, 500);
            var mqlsGenerated = (int)(leadsGenerated * faker.Random.Double(0.2, 0.5));
            var sqlsGenerated = (int)(mqlsGenerated * faker.Random.Double(0.3, 0.6));
            var opportunitiesCreated = faker.Random.Int(5, 50);
            var dealsWon = (int)(opportunitiesCreated * faker.Random.Double(0.1, 0.4));
            var impressions = faker.Random.Int(10000, 500000);
            var clicks = faker.Random.Int(500, 10000);
            var emailsSent = faker.Random.Int(1000, 50000);
            var emailsDelivered = (int)(emailsSent * faker.Random.Double(0.9, 0.98));
            var emailsOpened = (int)(emailsDelivered * faker.Random.Double(0.15, 0.35));
            var campaignType = faker.PickRandom(campaignTypes);

            var campaign = new MarketingCampaign
            {
                // Basic Information
                Name = faker.Commerce.ProductAdjective() + " " + faker.PickRandom(new[] { "Launch Campaign", "Promotion", "Flash Sale", "Executive Event", "Webinar Series", "Product Showcase" }),
                CampaignCode = $"CMP-{faker.Random.Int(10000, 99999)}",
                Description = faker.Lorem.Paragraphs(2),
                Objective = faker.Lorem.Sentence(),
                ObjectiveType = faker.PickRandom(objectives),
                CampaignType = campaignType,
                Status = (CampaignStatus)faker.Random.Int(0, 6),
                Priority = (CampaignPriority)faker.Random.Int(0, 4),
                PrimarySuccessMetric = faker.PickRandom(successMetrics),
                Theme = faker.Commerce.ProductAdjective() + " " + faker.Commerce.ProductMaterial(),
                ValueProposition = faker.Lorem.Sentence(),
                
                // Timeline
                StartDate = startDate,
                EndDate = endDate,
                ActualStartDate = startDate.AddDays(faker.Random.Int(-2, 2)),
                DurationDays = (int)(endDate - startDate).TotalDays,
                IsEvergreen = faker.Random.Bool(0.1f),
                
                // Budget & Financials
                Budget = budget,
                ActualCost = actualCost,
                DailyBudget = budget / 30,
                ExpectedRevenue = budget * faker.Random.Decimal(3, 10),
                ActualRevenue = dealsWon > 0 ? faker.Random.Decimal(10000, 500000) : 0,
                PipelineCreated = opportunitiesCreated * faker.Random.Decimal(5000, 50000),
                CostPerLead = leadsGenerated > 0 ? actualCost / leadsGenerated : 0,
                CostPerMql = mqlsGenerated > 0 ? actualCost / mqlsGenerated : 0,
                CostPerSql = sqlsGenerated > 0 ? actualCost / sqlsGenerated : 0,
                CostPerOpportunity = opportunitiesCreated > 0 ? actualCost / opportunitiesCreated : 0,
                CurrencyCode = "USD",
                
                // Target Audience
                TargetAudience = faker.Random.Int(1000, 50000),
                TargetAudienceDescription = faker.Lorem.Sentence(),
                AudienceType = faker.PickRandom(audienceTypes),
                TargetGeography = faker.PickRandom(new[] { "North America", "EMEA", "APAC", "Global", "United States", "Enterprise Accounts" }),
                TargetIndustries = string.Join(",", faker.Commerce.Categories(3)),
                TargetSegments = faker.PickRandom(new[] { "Enterprise", "Mid-Market", "SMB", "Startups", "Government" }),
                TargetJobTitles = string.Join(",", new[] { "CTO", "VP Engineering", "IT Director", "Developer", "Product Manager" }.OrderBy(x => faker.Random.Int()).Take(3)),
                
                // Lead Generation Metrics
                LeadsGenerated = leadsGenerated,
                MqlsGenerated = mqlsGenerated,
                SqlsGenerated = sqlsGenerated,
                SalsGenerated = (int)(sqlsGenerated * faker.Random.Double(0.5, 0.8)),
                OpportunitiesCreated = opportunitiesCreated,
                OpportunitiesInfluenced = faker.Random.Int(10, 100),
                DealsWon = dealsWon,
                CustomersAcquired = dealsWon,
                LeadToMqlRate = leadsGenerated > 0 ? (double)mqlsGenerated / leadsGenerated * 100 : 0,
                MqlToSqlRate = mqlsGenerated > 0 ? (double)sqlsGenerated / mqlsGenerated * 100 : 0,
                ConversionRate = faker.Random.Double(1, 10),
                AverageLeadScore = faker.Random.Double(30, 80),
                
                // Reach & Engagement
                Impressions = impressions,
                Reach = (int)(impressions * faker.Random.Double(0.3, 0.7)),
                Frequency = faker.Random.Double(1.5, 4.0),
                Clicks = clicks,
                ClickThroughRate = impressions > 0 ? (double)clicks / impressions * 100 : 0,
                LandingPageVisits = (int)(clicks * faker.Random.Double(0.7, 0.95)),
                FormSubmissions = leadsGenerated,
                ContentDownloads = faker.Random.Int(50, 500),
                DemoRequests = faker.Random.Int(5, 50),
                TrialSignups = faker.Random.Int(10, 100),
                
                // Email Metrics
                EmailsSent = emailsSent,
                EmailsDelivered = emailsDelivered,
                DeliveryRate = emailsSent > 0 ? (double)emailsDelivered / emailsSent * 100 : 0,
                EmailsOpened = emailsOpened,
                OpenRate = emailsDelivered > 0 ? (double)emailsOpened / emailsDelivered * 100 : 0,
                EmailClicks = (int)(emailsOpened * faker.Random.Double(0.1, 0.3)),
                Bounces = emailsSent - emailsDelivered,
                BounceRate = emailsSent > 0 ? (double)(emailsSent - emailsDelivered) / emailsSent * 100 : 0,
                Unsubscribes = faker.Random.Int(10, 200),
                SpamComplaints = faker.Random.Int(0, 10),
                
                // Social Metrics
                SocialReach = faker.Random.Int(5000, 100000),
                SocialEngagement = faker.Random.Int(100, 5000),
                SocialShares = faker.Random.Int(20, 500),
                SocialComments = faker.Random.Int(10, 200),
                SocialLikes = faker.Random.Int(50, 2000),
                NewFollowers = faker.Random.Int(10, 500),
                
                // Event Metrics (for event campaigns)
                Registrations = campaignType == CampaignType.Event || campaignType == CampaignType.Webinar ? faker.Random.Int(50, 500) : 0,
                Attendance = campaignType == CampaignType.Event || campaignType == CampaignType.Webinar ? faker.Random.Int(25, 300) : 0,
                
                // ROI
                ROI = faker.Random.Double(-20, 200),
                TargetRoi = faker.Random.Double(100, 300),
                TargetLeads = leadsGenerated + faker.Random.Int(-50, 50),
                GoalAchievementPercent = faker.Random.Double(60, 120),
                CampaignHealthScore = faker.Random.Int(40, 95),
                
                // Content
                MessageSubject = faker.Lorem.Sentence(5),
                CallToAction = faker.PickRandom(new[] { "Learn More", "Get Started", "Request Demo", "Download Now", "Start Free Trial", "Register Now" }),
                LandingPageUrl = $"https://example.com/campaign/{faker.Random.AlphaNumeric(8)}",
                
                // UTM Tracking
                UtmSource = faker.PickRandom(new[] { "email", "google", "linkedin", "facebook", "twitter", "direct" }),
                UtmMedium = faker.PickRandom(new[] { "email", "cpc", "social", "organic", "display", "referral" }),
                UtmCampaign = faker.Lorem.Slug(3),
                
                // A/B Testing
                IsABTest = faker.Random.Bool(0.3f),
                
                // Assignment
                OwnerId = faker.PickRandom(users).Id,
                AssignedToUserId = faker.PickRandom(users).Id,
                Department = "Marketing",
                
                // Classification
                Tags = string.Join(",", faker.Commerce.Categories(2)),
                Category = faker.PickRandom(new[] { "Demand Generation", "Brand Awareness", "Product Marketing", "Customer Marketing", "Field Marketing" }),
                FiscalQuarter = faker.PickRandom(new[] { "Q1", "Q2", "Q3", "Q4" }),
                FiscalYear = DateTime.Now.Year,
                
                CreatedAt = startDate.AddDays(-7)
            };
            campaigns.Add(campaign);
        }

        context.MarketingCampaigns.AddRange(campaigns);
        await context.SaveChangesAsync();
        Console.WriteLine($"   Created {count} campaigns");
        return campaigns;
    }

    static async Task<List<Lead>> SeedLeads(CrmDbContext context, List<MarketingCampaign> campaigns, List<User> users, List<Product> products, int count)
    {
        if (await context.Leads.AnyAsync())
        {
            return await context.Leads.ToListAsync();
        }

        var faker = new Bogus.Faker();
        var leads = new List<Lead>();
        var leadSources = Enum.GetValues<LeadSource>();
        var industries = Enum.GetValues<LeadIndustry>();
        var companySizes = Enum.GetValues<CompanySize>();
        var disqualReasons = Enum.GetValues<DisqualificationReason>();

        for (int i = 0; i < count; i++)
        {
            var status = (CRM.Core.Entities.LeadStatus)faker.Random.Int(0, 12);
            var isConverted = status == CRM.Core.Entities.LeadStatus.Converted;
            var isDisqualified = status == CRM.Core.Entities.LeadStatus.Disqualified;
            var createdDate = faker.Date.Past(1);
            var primaryCampaign = faker.Random.Bool(0.8f) ? faker.PickRandom(campaigns) : null;
            var leadScore = faker.Random.Int(0, 100);
            var emailsSent = faker.Random.Int(0, 20);
            var emailsOpened = (int)(emailsSent * faker.Random.Double(0.2, 0.6));
            
            var lead = new Lead
            {
                // Contact Information
                Salutation = faker.PickRandom(new[] { "Mr.", "Ms.", "Mrs.", "Dr.", null }),
                FirstName = faker.Name.FirstName(),
                LastName = faker.Name.LastName(),
                Email = faker.Internet.Email(),
                SecondaryEmail = faker.Random.Bool(0.2f) ? faker.Internet.Email() : null,
                Phone = faker.Phone.PhoneNumber("+1-###-###-####"),
                MobilePhone = faker.Random.Bool(0.5f) ? faker.Phone.PhoneNumber("+1-###-###-####") : null,
                LinkedInUrl = faker.Random.Bool(0.4f) ? $"https://linkedin.com/in/{faker.Internet.UserName()}" : null,
                
                // Company Information
                Company = faker.Company.CompanyName(),
                JobTitle = faker.Name.JobTitle(),
                Department = faker.Commerce.Department(),
                Industry = faker.PickRandom(industries),
                Website = faker.Internet.Url(),
                NumberOfEmployees = faker.Random.Int(10, 10000),
                CompanySize = faker.PickRandom(companySizes),
                AnnualRevenue = faker.Random.Decimal(100000, 100000000),
                RevenueRange = faker.PickRandom(new[] { "<$1M", "$1M-$10M", "$10M-$50M", "$50M-$100M", "$100M+" }),
                
                // Address
                Address = faker.Address.StreetAddress(),
                City = faker.Address.City(),
                State = faker.Address.StateAbbr(),
                ZipCode = faker.Address.ZipCode(),
                Country = faker.PickRandom(new[] { "USA", "Canada", "UK", "Germany", "France", "Australia" }),
                Region = faker.PickRandom(new[] { "North America", "EMEA", "APAC", "LATAM" }),
                Timezone = faker.PickRandom(new[] { "EST", "PST", "CST", "GMT", "CET" }),
                
                // Status & Qualification
                Status = status,
                Rating = (LeadRating)faker.Random.Int(0, 4),
                LeadScore = leadScore,
                FitScore = faker.Random.Int(0, 100),
                BehaviorScore = faker.Random.Int(0, 100),
                Grade = faker.PickRandom(new[] { "A", "B", "C", "D", "F" }),
                IsMql = leadScore >= 50,
                MqlDate = leadScore >= 50 ? createdDate.AddDays(faker.Random.Int(1, 14)) : null,
                IsSql = leadScore >= 70,
                SqlDate = leadScore >= 70 ? createdDate.AddDays(faker.Random.Int(7, 21)) : null,
                
                // BANT Qualification
                HasBudget = faker.Random.Bool(0.4f),
                BudgetAmount = faker.Random.Bool(0.3f) ? faker.Random.Decimal(10000, 500000) : null,
                BudgetRange = faker.PickRandom(new[] { "$10K-$25K", "$25K-$50K", "$50K-$100K", "$100K-$250K", "$250K+" }),
                HasAuthority = faker.Random.Bool(0.3f),
                AuthorityLevel = faker.PickRandom(new[] { "Decision Maker", "Influencer", "Recommender", "End User", "Evaluator" }),
                HasNeed = faker.Random.Bool(0.5f),
                PrimaryPainPoint = faker.PickRandom(new[] { "Efficiency", "Cost Reduction", "Scalability", "Compliance", "Integration", "Automation" }),
                UseCase = faker.Lorem.Sentence(),
                CurrentSolution = faker.Random.Bool(0.4f) ? faker.Company.CompanyName() : "None",
                HasTimeline = faker.Random.Bool(0.3f),
                ExpectedPurchaseDate = faker.Random.Bool(0.3f) ? faker.Date.Future(1) : null,
                TimelineDescription = faker.PickRandom(new[] { "Immediate", "1-3 months", "3-6 months", "6-12 months", "Next fiscal year" }),
                BantScore = faker.Random.Int(0, 4),
                
                // Lead Source & Attribution
                Source = faker.PickRandom(leadSources),
                SourceDescription = faker.Lorem.Sentence(3),
                PrimaryCampaignId = primaryCampaign?.Id,
                ConvertingCampaignId = isConverted && primaryCampaign != null ? primaryCampaign.Id : null,
                LastCampaignId = primaryCampaign?.Id,
                CampaignTouchCount = faker.Random.Int(1, 5),
                UtmSource = faker.PickRandom(new[] { "google", "linkedin", "facebook", "email", "direct" }),
                UtmMedium = faker.PickRandom(new[] { "cpc", "organic", "social", "email", "referral" }),
                UtmCampaign = faker.Lorem.Slug(2),
                LandingPageUrl = $"https://example.com/landing/{faker.Random.AlphaNumeric(6)}",
                
                // Engagement Tracking
                WebsiteVisits = faker.Random.Int(1, 50),
                PageViews = faker.Random.Int(2, 200),
                LastWebsiteVisit = faker.Date.Recent(30),
                ContentDownloads = faker.Random.Int(0, 5),
                WebinarsAttended = faker.Random.Int(0, 3),
                EmailsSent = emailsSent,
                EmailsOpened = emailsOpened,
                EmailClicks = (int)(emailsOpened * faker.Random.Double(0.1, 0.4)),
                LastEmailOpenDate = faker.Random.Bool(0.5f) ? faker.Date.Recent(14) : null,
                CallsMade = faker.Random.Int(0, 10),
                CallsConnected = faker.Random.Int(0, 5),
                MeetingsScheduled = faker.Random.Int(0, 3),
                MeetingsCompleted = faker.Random.Int(0, 2),
                TotalTouchpoints = faker.Random.Int(1, 20),
                
                // Product Interest
                PrimaryProductInterestId = faker.Random.Bool(0.6f) && products.Count > 0 ? faker.PickRandom(products).Id : null,
                RequestedDemo = faker.Random.Bool(0.2f),
                DemoRequestDate = faker.Random.Bool(0.2f) ? faker.Date.Recent(60) : null,
                DemoCompleted = faker.Random.Bool(0.1f),
                StartedTrial = faker.Random.Bool(0.15f),
                TrialStartDate = faker.Random.Bool(0.15f) ? faker.Date.Recent(30) : null,
                TrialStatus = faker.PickRandom(new[] { "Active", "Expired", "Converted", null }),
                EstimatedValue = faker.Random.Decimal(5000, 250000),
                
                // Communication Preferences
                OptInEmail = faker.Random.Bool(0.9f),
                OptInSms = faker.Random.Bool(0.3f),
                OptInPhone = faker.Random.Bool(0.7f),
                PreferredContactMethod = faker.PickRandom(new[] { "Email", "Phone", "LinkedIn", "SMS" }),
                PreferredContactTime = faker.PickRandom(new[] { "Morning", "Afternoon", "Evening" }),
                DoNotCall = faker.Random.Bool(0.05f),
                DoNotEmail = faker.Random.Bool(0.05f),
                PreferredLanguage = faker.PickRandom(new[] { "en", "es", "fr", "de" }),
                
                // Assignment
                OwnerId = faker.PickRandom(users).Id,
                AssignedDate = createdDate.AddHours(faker.Random.Int(1, 48)),
                AssignmentMethod = faker.PickRandom(new[] { "Round Robin", "Territory", "Manual", "Auto-Assignment" }),
                Territory = faker.PickRandom(new[] { "West", "East", "Central", "International" }),
                LastActivityDate = faker.Date.Recent(14),
                NextFollowUpDate = DateTime.UtcNow.AddDays(faker.Random.Int(1, 30)),
                NextAction = faker.PickRandom(new[] { "Follow-up call", "Send proposal", "Schedule demo", "Email check-in", "Qualify budget" }),
                IsStale = faker.Random.Bool(0.1f),
                
                // Conversion Information
                IsConverted = isConverted,
                ConvertedDate = isConverted ? createdDate.AddDays(faker.Random.Int(14, 90)) : null,
                ConvertedByUserId = isConverted ? faker.PickRandom(users).Id : null,
                ConversionType = isConverted ? (ConversionType)faker.Random.Int(1, 5) : ConversionType.NotConverted,
                ConvertedRevenue = isConverted ? faker.Random.Decimal(10000, 500000) : null,
                DaysToConvert = isConverted ? faker.Random.Int(14, 90) : null,
                
                // Disqualification
                IsDisqualified = isDisqualified,
                DisqualificationReason = isDisqualified ? faker.PickRandom(disqualReasons) : DisqualificationReason.NotSpecified,
                DisqualificationNotes = isDisqualified ? faker.Lorem.Sentence() : null,
                DisqualifiedDate = isDisqualified ? createdDate.AddDays(faker.Random.Int(1, 30)) : null,
                DisqualifiedByUserId = isDisqualified ? faker.PickRandom(users).Id : null,
                
                // Duplicate Detection
                IsDuplicate = faker.Random.Bool(0.02f),
                DuplicateCheckPerformed = true,
                
                // Data Enrichment
                IsEnriched = faker.Random.Bool(0.4f),
                EnrichedDate = faker.Random.Bool(0.4f) ? faker.Date.Recent(60) : null,
                EnrichmentSource = faker.PickRandom(new[] { "Clearbit", "ZoomInfo", "LinkedIn", null }),
                
                // Documentation
                Description = faker.Lorem.Sentence(),
                QualificationNotes = faker.Lorem.Paragraph(),
                Notes = faker.Lorem.Paragraph(),
                Tags = string.Join(",", faker.Commerce.Categories(2)),
                
                CreatedAt = createdDate
            };
            leads.Add(lead);
        }

        context.Leads.AddRange(leads);
        await context.SaveChangesAsync();
        Console.WriteLine($"   Created {count} leads");
        return leads;
    }

    static async Task<List<Opportunity>> SeedOpportunities(CrmDbContext context, List<Customer> customers, List<Product> products, List<User> users, List<MarketingCampaign> campaigns, int count)
    {
        if (await context.Opportunities.AnyAsync())
        {
            return await context.Opportunities.ToListAsync();
        }

        var faker = new Bogus.Faker();
        var opportunities = new List<Opportunity>();
        var salesUsers = users.Where(u => u.Role == (int)UserRole.Sales || u.Role == (int)UserRole.Manager).ToList();

        for (int i = 0; i < count; i++)
        {
            var amount = faker.Random.Decimal(5000, 500000);
            var probability = faker.Random.Double(10, 90);

            var opportunity = new Opportunity
            {
                Name = $"{faker.Company.CompanyName()} - {faker.Commerce.ProductName()}",
                Description = faker.Lorem.Paragraph(),
                OpportunityType = (OpportunityType)faker.Random.Int(0, 5),
                Priority = (OpportunityPriority)faker.Random.Int(0, 3),
                Amount = amount,
                ExpectedRevenue = amount * (decimal)(probability / 100),
                Stage = (OpportunityStage)faker.Random.Int(0, 9),
                Probability = probability,
                ForecastCategory = (ForecastCategory)faker.Random.Int(0, 4),
                CloseDate = faker.Date.Future(1),
                ExpectedCloseDate = faker.Date.Future(1),
                NextStep = faker.Lorem.Sentence(),
                LeadSource = faker.PickRandom(new[] { "Website", "Referral", "Campaign", "Partner" }),
                CustomerId = faker.PickRandom(customers).Id,
                ProductId = faker.Random.Bool(0.7f) ? faker.PickRandom(products).Id : null,
                AssignedToUserId = faker.PickRandom(salesUsers).Id,
                CampaignId = faker.Random.Bool(0.4f) ? faker.PickRandom(campaigns).Id : null,
                PrimaryCompetitor = faker.Random.Bool(0.3f) ? faker.Company.CompanyName() : null,
                CompetitiveSituation = (CompetitiveSituation)faker.Random.Int(0, 7),
                BudgetStatus = (BudgetStatus)faker.Random.Int(0, 4),
                Health = (OpportunityHealth)faker.Random.Int(0, 2),
                EngagementLevel = (EngagementLevel)faker.Random.Int(0, 4),
                BantScore = faker.Random.Int(0, 4),
                MeddicScore = faker.Random.Int(0, 100),
                Tags = string.Join(",", faker.Commerce.Categories(2)),
                Notes = faker.Lorem.Paragraph(),
                CreatedAt = faker.Date.Past(1)
            };
            opportunities.Add(opportunity);
        }

        context.Opportunities.AddRange(opportunities);
        await context.SaveChangesAsync();
        Console.WriteLine($"   Created {count} opportunities");
        return opportunities;
    }

    static async Task SeedTasks(CrmDbContext context, List<Customer> customers, List<Opportunity> opportunities, List<User> users, int count)
    {
        if (await context.CrmTasks.AnyAsync())
        {
            return;
        }

        var faker = new Bogus.Faker();
        var tasks = new List<CrmTask>();
        var salesUsers = users.Where(u => u.Role != (int)UserRole.Guest).ToList();

        for (int i = 0; i < count; i++)
        {
            var task = new CrmTask
            {
                Subject = faker.PickRandom(new[] { "Follow up call", "Send proposal", "Schedule demo", "Review contract", "Send information", "Check in", "Prepare presentation" }) + " - " + faker.Name.FullName(),
                Description = faker.Lorem.Paragraph(),
                TaskType = (CrmTaskType)faker.Random.Int(0, 8),
                Status = (CrmTaskStatus)faker.Random.Int(0, 5),
                Priority = (CrmTaskPriority)faker.Random.Int(0, 3),
                DueDate = faker.Date.Between(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow.AddDays(30)),
                StartDate = faker.Date.Past(1),
                PercentComplete = faker.Random.Int(0, 100),
                EstimatedMinutes = faker.Random.Int(15, 120),
                CustomerId = faker.Random.Bool(0.5f) ? faker.PickRandom(customers).Id : null,
                OpportunityId = faker.Random.Bool(0.3f) ? faker.PickRandom(opportunities).Id : null,
                AssignedToUserId = faker.PickRandom(salesUsers).Id,
                CreatedByUserId = faker.PickRandom(salesUsers).Id,
                Tags = faker.Random.Bool(0.3f) ? string.Join(",", faker.Commerce.Categories(2)) : null,
                CreatedAt = faker.Date.Past(1)
            };
            tasks.Add(task);
        }

        context.CrmTasks.AddRange(tasks);
        await context.SaveChangesAsync();
        Console.WriteLine($"   Created {count} tasks");
    }

    static async Task SeedNotes(CrmDbContext context, List<Customer> customers, List<Opportunity> opportunities, List<User> users, int count)
    {
        if (await context.Notes.AnyAsync())
        {
            return;
        }

        var faker = new Bogus.Faker();
        var notes = new List<Note>();

        for (int i = 0; i < count; i++)
        {
            var note = new Note
            {
                Title = faker.Lorem.Sentence(5),
                Content = faker.Lorem.Paragraphs(faker.Random.Int(1, 3)),
                Summary = faker.Lorem.Sentence(),
                NoteType = (NoteType)faker.Random.Int(0, 8),
                Visibility = (NoteVisibility)faker.Random.Int(0, 2),
                IsPinned = faker.Random.Bool(0.1f),
                IsImportant = faker.Random.Bool(0.15f),
                CustomerId = faker.Random.Bool(0.4f) ? faker.PickRandom(customers).Id : null,
                OpportunityId = faker.Random.Bool(0.3f) ? faker.PickRandom(opportunities).Id : null,
                CreatedByUserId = faker.PickRandom(users).Id,
                Tags = faker.Random.Bool(0.3f) ? string.Join(",", faker.Commerce.Categories(2)) : null,
                CreatedAt = faker.Date.Past(1)
            };
            notes.Add(note);
        }

        context.Notes.AddRange(notes);
        await context.SaveChangesAsync();
        Console.WriteLine($"   Created {count} notes");
    }

    static async Task SeedQuotes(CrmDbContext context, List<Customer> customers, List<Opportunity> opportunities, List<User> users, int count)
    {
        if (await context.Quotes.AnyAsync())
        {
            return;
        }

        var faker = new Bogus.Faker();
        var quotes = new List<Quote>();
        var salesUsers = users.Where(u => u.Role == (int)UserRole.Sales || u.Role == (int)UserRole.Manager).ToList();

        for (int i = 0; i < count; i++)
        {
            var subtotal = faker.Random.Decimal(5000, 200000);
            var discountPercent = faker.Random.Decimal(0, 20);
            var discount = subtotal * (discountPercent / 100);
            var taxRate = 8.25m;
            var tax = (subtotal - discount) * (taxRate / 100);

            var customer = faker.PickRandom(customers);
            var quote = new Quote
            {
                QuoteNumber = $"Q-{DateTime.UtcNow.Year}-{(i + 1).ToString().PadLeft(5, '0')}",
                Name = $"Quote for {customer.Company}",
                Description = faker.Lorem.Sentence(),
                Status = (QuoteStatus)faker.Random.Int(0, 8),
                QuoteDate = faker.Date.Past(1),
                ExpirationDate = faker.Date.Future(1),
                Subtotal = subtotal,
                Discount = discount,
                DiscountPercent = discountPercent,
                Tax = tax,
                TaxRate = taxRate,
                Total = subtotal - discount + tax,
                PaymentTerms = faker.PickRandom(new[] { "Net 30", "Net 60", "Due on Receipt", "Net 15" }),
                ValidityDays = faker.PickRandom(new[] { 15, 30, 45, 60 }),
                BillingName = $"{customer.FirstName} {customer.LastName}",
                BillingAddress = customer.Address,
                BillingCity = customer.City,
                BillingState = customer.State,
                BillingZipCode = customer.ZipCode,
                BillingCountry = customer.Country,
                CustomerId = customer.Id,
                OpportunityId = faker.Random.Bool(0.5f) ? faker.PickRandom(opportunities).Id : null,
                AssignedToUserId = faker.PickRandom(salesUsers).Id,
                CreatedByUserId = faker.PickRandom(salesUsers).Id,
                CreatedAt = faker.Date.Past(1)
            };
            quotes.Add(quote);
        }

        context.Quotes.AddRange(quotes);
        await context.SaveChangesAsync();
        Console.WriteLine($"   Created {count} quotes");
    }

    static async Task SeedInteractions(CrmDbContext context, List<Customer> customers, List<Contact> contacts, List<User> users, int count)
    {
        if (await context.Interactions.AnyAsync())
        {
            return;
        }

        var faker = new Bogus.Faker();
        var interactions = new List<Interaction>();

        for (int i = 0; i < count; i++)
        {
            var customer = faker.PickRandom(customers);
            var interaction = new Interaction
            {
                InteractionType = (InteractionType)faker.Random.Int(0, 15),
                Type = faker.PickRandom(new[] { "Call", "Email", "Meeting", "Demo", "Follow-up" }),
                Direction = (InteractionDirection)faker.Random.Int(0, 2),
                Subject = faker.Lorem.Sentence(5),
                Description = faker.Lorem.Paragraph(),
                InteractionDate = faker.Date.Past(1),
                DurationMinutes = faker.Random.Int(5, 90),
                Outcome = (InteractionOutcome)faker.Random.Int(0, 7),
                Sentiment = (InteractionSentiment)faker.Random.Int(0, 4),
                IsCompleted = faker.Random.Bool(0.8f),
                CustomerId = customer.Id,
                ContactId = faker.Random.Bool(0.3f) ? faker.PickRandom(contacts).Id : null,
                AssignedToUserId = faker.PickRandom(users).Id,
                CreatedByUserId = faker.PickRandom(users).Id,
                FollowUpDate = faker.Random.Bool(0.2f) ? faker.Date.Future(1) : null,
                FollowUpNotes = faker.Lorem.Paragraph(),
                CreatedAt = faker.Date.Past(1)
            };
            interactions.Add(interaction);
        }

        context.Interactions.AddRange(interactions);
        await context.SaveChangesAsync();
        Console.WriteLine($"   Created {count} interactions");
    }

    static async Task SeedActivities(CrmDbContext context, List<User> users, List<Customer> customers, List<Opportunity> opportunities, int count)
    {
        if (await context.Activities.AnyAsync())
        {
            return;
        }

        var faker = new Bogus.Faker();
        var activities = new List<Activity>();

        for (int i = 0; i < count; i++)
        {
            var user = faker.PickRandom(users);
            var activityType = (ActivityType)faker.Random.Int(0, 64);

            var activity = new Activity
            {
                ActivityType = activityType,
                Title = GetActivityTitle(activityType, faker),
                Description = faker.Lorem.Sentence(),
                ActivityDate = faker.Date.Past(1),
                UserId = user.Id,
                UserName = $"{user.FirstName} {user.LastName}",
                UserEmail = user.Email,
                EntityType = faker.PickRandom(new[] { "Customer", "Opportunity", "Contact", "Product" }),
                EntityId = faker.Random.Int(1, 50),
                CustomerId = faker.Random.Bool(0.4f) ? faker.PickRandom(customers).Id : null,
                OpportunityId = faker.Random.Bool(0.3f) ? faker.PickRandom(opportunities).Id : null,
                IsSystem = faker.Random.Bool(0.2f),
                IsPrivate = false,
                CreatedAt = faker.Date.Past(1)
            };
            activities.Add(activity);
        }

        context.Activities.AddRange(activities);
        await context.SaveChangesAsync();
        Console.WriteLine($"   Created {count} activities");
    }

    static string GetActivityTitle(ActivityType type, Bogus.Faker faker)
    {
        return type switch
        {
            ActivityType.EmailSent => $"Email sent to {faker.Name.FullName()}",
            ActivityType.EmailReceived => $"Email received from {faker.Name.FullName()}",
            ActivityType.CallMade => $"Outbound call to {faker.Phone.PhoneNumber()}",
            ActivityType.CallReceived => $"Inbound call from {faker.Name.FullName()}",
            ActivityType.MeetingScheduled => $"Meeting scheduled with {faker.Name.FullName()}",
            ActivityType.MeetingCompleted => $"Meeting completed with {faker.Company.CompanyName()}",
            ActivityType.CustomerCreated => $"New customer: {faker.Company.CompanyName()}",
            ActivityType.CustomerUpdated => $"Customer updated: {faker.Company.CompanyName()}",
            ActivityType.OpportunityCreated => $"New opportunity created",
            ActivityType.OpportunityUpdated => $"Opportunity updated",
            ActivityType.OpportunityWon => $"Opportunity won: {faker.Commerce.ProductName()}",
            ActivityType.OpportunityLost => $"Opportunity lost",
            ActivityType.QuoteCreated => $"Quote created for {faker.Company.CompanyName()}",
            ActivityType.QuoteSent => $"Quote sent to {faker.Name.FullName()}",
            ActivityType.TaskCreated => $"Task created: {faker.Lorem.Sentence(4)}",
            ActivityType.TaskCompleted => $"Task completed",
            ActivityType.NoteAdded => $"Note added",
            _ => faker.Lorem.Sentence(5)
        };
    }

    static CrmDbContext CreateDbContext(IConfiguration configuration)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CrmDbContext>();
        var provider = configuration["DatabaseProvider"]?.ToLowerInvariant() ?? "mariadb";
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString) && (provider == "mysql" || provider == "mariadb"))
        {
            var dbHost = configuration["DB_HOST"] ?? configuration["DbHost"] ?? "mariadb";
            var dbPort = configuration["DB_PORT"] ?? "3306";
            var dbName = configuration["DB_NAME"] ?? "crm_db";
            var dbUser = configuration["DB_USER"] ?? "crm_user";
            var dbPass = configuration["DB_PASSWORD"] ?? configuration["DB_PASS"] ?? "crm_pass";
            connectionString = $"Server={dbHost};Port={dbPort};Database={dbName};Uid={dbUser};Pwd={dbPass};";
        }

        switch (provider)
        {
            case "sqlserver":
            case "mssql":
                optionsBuilder.UseSqlServer(connectionString);
                break;
            case "postgresql":
            case "postgres":
            case "npgsql":
                optionsBuilder.UseNpgsql(connectionString);
                break;
            case "mysql":
            case "mariadb":
                optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
                break;
            default:
                optionsBuilder.UseSqlite(connectionString);
                break;
        }

        return new CrmDbContext(optionsBuilder.Options, configuration);
    }

    static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    static string GetMaskedConnectionString(IConfiguration configuration)
    {
        var connStr = configuration.GetConnectionString("DefaultConnection") ?? "";
        if (connStr.Contains("Password=", StringComparison.OrdinalIgnoreCase))
        {
            return System.Text.RegularExpressions.Regex.Replace(connStr, @"Password=[^;]+", "Password=****");
        }
        return connStr;
    }
}
