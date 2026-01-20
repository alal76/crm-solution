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

        for (int i = 0; i < count; i++)
        {
            var price = faker.Random.Decimal(99, 9999);
            var cost = price * 0.4m;
            var product = new Product
            {
                Name = faker.Commerce.ProductName(),
                Description = faker.Commerce.ProductDescription(),
                ShortDescription = faker.Commerce.ProductAdjective() + " " + faker.Commerce.Product(),
                SKU = $"SKU-{faker.Random.AlphaNumeric(8).ToUpper()}",
                Barcode = faker.Commerce.Ean13(),
                ProductType = (ProductType)faker.Random.Int(0, 5),
                Status = ProductStatus.Active,
                Category = faker.PickRandom(categories),
                Brand = faker.Company.CompanyName(),
                Price = price,
                ListPrice = price * 1.2m,
                Cost = cost,
                Margin = price - cost,
                IsTaxable = true,
                TaxRate = 8.25m,
                Quantity = faker.Random.Int(0, 1000),
                ReorderLevel = faker.Random.Int(10, 50),
                TrackInventory = true,
                ImageUrl = $"https://picsum.photos/seed/{i}/200/200",
                TotalSold = faker.Random.Int(0, 500),
                TotalRevenue = faker.Random.Decimal(10000, 500000),
                AverageRating = faker.Random.Double(3.0, 5.0),
                ReviewCount = faker.Random.Int(0, 100),
                IsFeatured = faker.Random.Bool(0.2f),
                IsActive = true,
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
                CustomerId = faker.Random.Bool(0.3f) ? faker.PickRandom(customers).Id : null,
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

        for (int i = 0; i < count; i++)
        {
            var startDate = faker.Date.Past(1);
            var endDate = startDate.AddDays(faker.Random.Int(14, 90));
            var budget = faker.Random.Decimal(5000, 100000);

            var campaign = new MarketingCampaign
            {
                Name = faker.Commerce.ProductAdjective() + " " + faker.PickRandom(new[] { "Launch", "Promotion", "Sale", "Event", "Webinar" }),
                Description = faker.Lorem.Paragraph(),
                Objective = faker.Lorem.Sentence(),
                CampaignType = (CampaignType)faker.Random.Int(0, 15),
                Status = (CampaignStatus)faker.Random.Int(0, 6),
                Priority = (CampaignPriority)faker.Random.Int(0, 3),
                StartDate = startDate,
                EndDate = endDate,
                Budget = budget,
                ActualCost = budget * faker.Random.Decimal(0.5m, 1.2m),
                TargetAudience = faker.Random.Int(1000, 50000),
                Impressions = faker.Random.Int(10000, 500000),
                Clicks = faker.Random.Int(500, 10000),
                LeadsGenerated = faker.Random.Int(10, 500),
                OpportunitiesCreated = faker.Random.Int(5, 50),
                CustomersAcquired = faker.Random.Int(0, 20),
                EmailsSent = faker.Random.Int(1000, 50000),
                EmailsOpened = faker.Random.Int(500, 25000),
                ConversionRate = faker.Random.Double(1, 10),
                ROI = faker.Random.Double(-20, 200),
                OwnerId = faker.PickRandom(users).Id,
                Tags = string.Join(",", faker.Commerce.Categories(2)),
                CreatedAt = startDate.AddDays(-7)
            };
            campaigns.Add(campaign);
        }

        context.MarketingCampaigns.AddRange(campaigns);
        await context.SaveChangesAsync();
        Console.WriteLine($"   Created {count} campaigns");
        return campaigns;
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
                CompetitorName = faker.Random.Bool(0.3f) ? faker.Company.CompanyName() : null,
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
                FollowUpRequired = faker.Random.Bool(0.3f),
                FollowUpDate = faker.Random.Bool(0.2f) ? faker.Date.Future(1) : null,
                Notes = faker.Lorem.Paragraph(),
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
                IsSystemGenerated = faker.Random.Bool(0.2f),
                IsVisible = true,
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
        var provider = configuration["DatabaseProvider"]?.ToLowerInvariant() ?? "sqlite";
        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? "Data Source=crm.db";

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
