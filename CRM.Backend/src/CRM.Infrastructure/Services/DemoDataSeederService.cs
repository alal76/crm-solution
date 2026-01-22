using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service to seed comprehensive demo data for the CRM system.
/// Includes 100+ entries for products, services, customers, contacts, leads, opportunities, etc.
/// </summary>
public class DemoDataSeederService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<DemoDataSeederService> _logger;
    private readonly Random _random = new();
    
    // Demo company and person name data
    private static readonly string[] CompanyPrefixes = { "Tech", "Global", "Digital", "Smart", "Cloud", "Cyber", "Data", "Info", "Net", "Soft", "Pro", "Prime", "Elite", "Advanced", "Modern", "Innovative", "Strategic", "Dynamic", "Unified", "Integrated" };
    private static readonly string[] CompanySuffixes = { "Solutions", "Systems", "Technologies", "Services", "Consulting", "Partners", "Group", "Corp", "Inc", "Labs", "Networks", "Dynamics", "Enterprises", "Associates", "Innovations" };
    private static readonly string[] FirstNames = { "James", "Mary", "John", "Patricia", "Robert", "Jennifer", "Michael", "Linda", "William", "Elizabeth", "David", "Barbara", "Richard", "Susan", "Joseph", "Jessica", "Thomas", "Sarah", "Charles", "Karen", "Christopher", "Nancy", "Daniel", "Lisa", "Matthew", "Betty", "Anthony", "Margaret", "Mark", "Sandra", "Donald", "Ashley", "Steven", "Kimberly", "Paul", "Emily", "Andrew", "Donna", "Joshua", "Michelle" };
    private static readonly string[] LastNames = { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson", "Thomas", "Taylor", "Moore", "Jackson", "Martin", "Lee", "Perez", "Thompson", "White", "Harris", "Sanchez", "Clark", "Ramirez", "Lewis", "Robinson" };
    private static readonly string[] Streets = { "Main St", "Oak Ave", "Technology Blvd", "Innovation Way", "Business Park Dr", "Corporate Center", "Commerce St", "Enterprise Rd", "Industrial Blvd", "Silicon Valley Way" };
    private static readonly string[] Cities = { "New York", "Los Angeles", "Chicago", "Houston", "Phoenix", "Philadelphia", "San Antonio", "San Diego", "Dallas", "San Jose", "Austin", "Seattle", "Denver", "Boston", "Atlanta", "Miami", "Portland", "Minneapolis", "Detroit", "Charlotte" };
    private static readonly string[] States = { "NY", "CA", "IL", "TX", "AZ", "PA", "TX", "CA", "TX", "CA", "TX", "WA", "CO", "MA", "GA", "FL", "OR", "MN", "MI", "NC" };
    
    public DemoDataSeederService(ICrmDbContext context, ILogger<DemoDataSeederService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Seed all demo data
    /// </summary>
    public async Task SeedAllDemoDataAsync()
    {
        _logger.LogInformation("Starting demo data seeding...");
        
        try
        {
            // Check if demo data already exists
            var settings = await _context.SystemSettings.FirstOrDefaultAsync();
            if (settings?.DemoDataSeeded == true)
            {
                _logger.LogInformation("Demo data already seeded. Skipping...");
                return;
            }

            // Seed in order to maintain relationships
            await SeedDemoUsersAsync();
            await SeedProductsAsync();
            await SeedServiceRequestCategoriesAsync();
            await SeedCustomersAsync();
            await SeedContactsAsync();
            await SeedLeadsAsync();
            await SeedOpportunitiesAsync();
            
            // Update settings to mark demo data as seeded
            if (settings != null)
            {
                settings.DemoDataSeeded = true;
                settings.DemoDataLastSeeded = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            
            _logger.LogInformation("Demo data seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding demo data");
            throw;
        }
    }

    /// <summary>
    /// Seed 10 demo users with different groups and profiles
    /// </summary>
    public async Task SeedDemoUsersAsync()
    {
        _logger.LogInformation("Seeding demo users...");
        
        // Check if demo users already exist
        var existingDemoUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "demo.admin");
        if (existingDemoUser != null)
        {
            _logger.LogInformation("Demo users already exist. Skipping...");
            return;
        }

        // Create user groups if they don't exist
        var groupNames = new[] { "Administrators", "Sales Team", "Support Team", "Marketing Team", "Management" };
        var groupIds = new Dictionary<string, int>();
        
        foreach (var groupName in groupNames)
        {
            var existing = await _context.UserGroups.FirstOrDefaultAsync(g => g.Name == groupName);
            if (existing == null)
            {
                var group = new UserGroup
                {
                    Name = groupName,
                    Description = $"Demo {groupName} group",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.UserGroups.Add(group);
                await _context.SaveChangesAsync();
                groupIds[groupName] = group.Id;
            }
            else
            {
                groupIds[groupName] = existing.Id;
            }
        }

        // Create a demo department if needed
        var department = await _context.Departments.FirstOrDefaultAsync(d => d.Name == "Demo Department");
        if (department == null)
        {
            department = new Department
            {
                Name = "Demo Department",
                Description = "Department for demo users",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Departments.Add(department);
            await _context.SaveChangesAsync();
        }

        // Password hash for "Admin@123"
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
        
        // Demo users data: (username, first, last, email, role, groupName)
        var demoUsers = new[]
        {
            ("demo.admin", "Demo", "Administrator", "demo.admin@example.com", (int)UserRole.Admin, "Administrators"),
            ("demo.sales1", "Sarah", "Sales", "demo.sales1@example.com", (int)UserRole.Sales, "Sales Team"),
            ("demo.sales2", "Samuel", "Seller", "demo.sales2@example.com", (int)UserRole.Sales, "Sales Team"),
            ("demo.sales3", "Sally", "Salesrep", "demo.sales3@example.com", (int)UserRole.Sales, "Sales Team"),
            ("demo.support1", "Scott", "Support", "demo.support1@example.com", (int)UserRole.Support, "Support Team"),
            ("demo.support2", "Stephanie", "Helper", "demo.support2@example.com", (int)UserRole.Support, "Support Team"),
            ("demo.marketing1", "Mark", "Marketer", "demo.marketing1@example.com", (int)UserRole.Sales, "Marketing Team"),
            ("demo.marketing2", "Maria", "Campaign", "demo.marketing2@example.com", (int)UserRole.Sales, "Marketing Team"),
            ("demo.manager1", "Mike", "Manager", "demo.manager1@example.com", (int)UserRole.Manager, "Management"),
            ("demo.manager2", "Monica", "Director", "demo.manager2@example.com", (int)UserRole.Manager, "Management"),
        };

        foreach (var (username, firstName, lastName, email, role, groupName) in demoUsers)
        {
            var user = new User
            {
                Username = username,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                PasswordHash = passwordHash,
                Role = role,
                IsActive = true,
                EmailVerified = true,
                DepartmentId = department.Id,
                PrimaryGroupId = groupIds[groupName],
                CreatedAt = DateTime.UtcNow
            };
            
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Add to group membership
            var membership = new UserGroupMember
            {
                UserId = user.Id,
                UserGroupId = groupIds[groupName],
                AddedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            _context.UserGroupMembers.Add(membership);
        }
        
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded 10 demo users");
    }

    /// <summary>
    /// Seed 100 products: 30 Hardware, 35 Software, 35 IT Services
    /// </summary>
    public async Task SeedProductsAsync()
    {
        _logger.LogInformation("Seeding demo products...");
        
        var existingProducts = await _context.Products.CountAsync();
        if (existingProducts >= 50)
        {
            _logger.LogInformation("Products already exist. Skipping...");
            return;
        }

        var products = new List<Product>();
        
        // Hardware Products (30)
        var hardwareItems = new[]
        {
            ("Enterprise Server Pro", "High-performance enterprise-grade server", 12999.99m),
            ("Workstation Elite X1", "Professional workstation for demanding applications", 4599.99m),
            ("Network Switch 48-Port", "Managed network switch with PoE", 1299.99m),
            ("Storage Array NAS-500", "Network-attached storage with 50TB capacity", 8999.99m),
            ("Firewall Appliance FW-2000", "Next-gen firewall with advanced threat protection", 5499.99m),
            ("Laptop Business Pro 15", "Business laptop with security features", 1899.99m),
            ("Desktop Workstation D7", "Tower workstation for CAD/3D work", 2999.99m),
            ("UPS Battery Backup 3000VA", "Uninterruptible power supply", 899.99m),
            ("Wireless Access Point Pro", "Enterprise WiFi 6E access point", 499.99m),
            ("KVM Switch 8-Port", "Remote server management", 799.99m),
            ("Rack Cabinet 42U", "Server rack with cooling", 1599.99m),
            ("SSD Enterprise 4TB", "Enterprise solid-state drive", 699.99m),
            ("RAM DDR5 64GB Kit", "Server memory upgrade", 399.99m),
            ("GPU Compute Card", "AI/ML acceleration card", 4999.99m),
            ("Router Enterprise Edge", "Enterprise edge router", 2499.99m),
            ("Monitor 32-inch 4K", "Professional display", 799.99m),
            ("Keyboard Mechanical Pro", "Ergonomic mechanical keyboard", 149.99m),
            ("Mouse Wireless Enterprise", "Business wireless mouse", 79.99m),
            ("Webcam 4K Conference", "Video conferencing camera", 299.99m),
            ("Headset Business Pro", "Noise-canceling headset", 199.99m),
            ("Docking Station USB-C", "Universal laptop dock", 249.99m),
            ("Printer Laser Pro", "Enterprise laser printer", 899.99m),
            ("Scanner Document Pro", "High-speed document scanner", 599.99m),
            ("External SSD 2TB", "Portable storage drive", 199.99m),
            ("Network Cable Cat6a 100m", "Bulk ethernet cable", 89.99m),
            ("Patch Panel 24-Port", "Network patch panel", 129.99m),
            ("PDU Rack Mount", "Power distribution unit", 349.99m),
            ("Cable Management Kit", "Rack cable organization", 79.99m),
            ("Server Rails Kit", "Universal server mounting rails", 99.99m),
            ("Cooling Fan 140mm x4", "Server cooling fans", 59.99m),
        };

        int skuCounter = 1001;
        foreach (var (name, desc, price) in hardwareItems)
        {
            products.Add(new Product
            {
                Name = name,
                Description = desc,
                SKU = $"HW-{skuCounter++}",
                Category = "Hardware",
                ProductType = ProductType.Physical,
                Status = ProductStatus.Active,
                Price = price,
                IsService = false,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Software Products (35)
        var softwareItems = new[]
        {
            ("Office Suite Enterprise", "Complete office productivity suite", 299.99m, ProductType.License),
            ("Antivirus Business", "Enterprise antivirus protection", 49.99m, ProductType.Subscription),
            ("Backup Solution Pro", "Automated backup software", 199.99m, ProductType.Subscription),
            ("Virtualization Platform", "Server virtualization software", 999.99m, ProductType.License),
            ("Database Server Enterprise", "Enterprise database platform", 2499.99m, ProductType.License),
            ("Monitoring Suite", "Infrastructure monitoring", 599.99m, ProductType.Subscription),
            ("Collaboration Platform", "Team collaboration software", 12.99m, ProductType.Subscription),
            ("Project Management Pro", "Project management software", 24.99m, ProductType.Subscription),
            ("CRM Software", "Customer relationship management", 79.99m, ProductType.Subscription),
            ("ERP System", "Enterprise resource planning", 149.99m, ProductType.Subscription),
            ("CAD Software Pro", "Computer-aided design software", 1999.99m, ProductType.License),
            ("Video Editing Suite", "Professional video editing", 499.99m, ProductType.License),
            ("Graphic Design Pro", "Vector and image editing", 249.99m, ProductType.Subscription),
            ("Accounting Software", "Financial management software", 39.99m, ProductType.Subscription),
            ("HR Management System", "Human resources software", 8.99m, ProductType.Subscription),
            ("Inventory Management", "Inventory tracking software", 49.99m, ProductType.Subscription),
            ("Email Server", "Enterprise email solution", 4.99m, ProductType.Subscription),
            ("VPN Enterprise", "Secure VPN solution", 9.99m, ProductType.Subscription),
            ("Remote Desktop Pro", "Remote access software", 14.99m, ProductType.Subscription),
            ("PDF Editor Pro", "PDF creation and editing", 149.99m, ProductType.License),
            ("Code Editor Enterprise", "Developer IDE", 199.99m, ProductType.License),
            ("Cloud Storage 1TB", "Cloud file storage", 9.99m, ProductType.Subscription),
            ("Password Manager Business", "Enterprise password management", 6.99m, ProductType.Subscription),
            ("Endpoint Security", "Device security management", 29.99m, ProductType.Subscription),
            ("Network Analyzer", "Network traffic analysis", 799.99m, ProductType.License),
            ("DevOps Platform", "CI/CD and automation", 149.99m, ProductType.Subscription),
            ("Container Platform", "Container orchestration", 99.99m, ProductType.Subscription),
            ("API Gateway", "API management platform", 199.99m, ProductType.Subscription),
            ("Log Management", "Centralized logging", 79.99m, ProductType.Subscription),
            ("SIEM Solution", "Security information management", 299.99m, ProductType.Subscription),
            ("Ticketing System", "IT helpdesk software", 19.99m, ProductType.Subscription),
            ("Asset Management", "IT asset tracking", 39.99m, ProductType.Subscription),
            ("Network Documentation", "Network diagram software", 149.99m, ProductType.License),
            ("Digital Signage", "Display management software", 24.99m, ProductType.Subscription),
            ("Learning Platform", "E-learning management system", 4.99m, ProductType.Subscription),
        };

        skuCounter = 2001;
        foreach (var (name, desc, price, type) in softwareItems)
        {
            products.Add(new Product
            {
                Name = name,
                Description = desc,
                SKU = $"SW-{skuCounter++}",
                Category = "Software",
                ProductType = type,
                Status = ProductStatus.Active,
                Price = price,
                IsService = false,
                IsSubscription = type == ProductType.Subscription,
                CreatedAt = DateTime.UtcNow
            });
        }

        // IT Services (35)
        var serviceItems = new[]
        {
            ("IT Consulting - Basic", "General IT consulting per hour", 150m, ProductType.Consulting),
            ("IT Consulting - Senior", "Senior consultant per hour", 250m, ProductType.Consulting),
            ("Network Assessment", "Complete network infrastructure assessment", 2500m, ProductType.Service),
            ("Security Audit", "Comprehensive security audit", 5000m, ProductType.Service),
            ("Cloud Migration", "Migration to cloud infrastructure", 10000m, ProductType.Implementation),
            ("Server Installation", "Physical server installation", 500m, ProductType.Service),
            ("Desktop Deployment", "Per-device deployment service", 75m, ProductType.Service),
            ("Network Cable Installation", "Per-drop cable installation", 125m, ProductType.Service),
            ("Hardware Repair - Desktop", "Desktop computer repair", 99m, ProductType.Service),
            ("Hardware Repair - Laptop", "Laptop repair service", 149m, ProductType.Service),
            ("Hardware Repair - Server", "Server hardware repair", 299m, ProductType.Service),
            ("Software Installation", "Software setup and configuration", 99m, ProductType.Service),
            ("OS Upgrade", "Operating system upgrade", 149m, ProductType.Service),
            ("Data Recovery", "Data recovery service", 399m, ProductType.Service),
            ("Virus Removal", "Malware and virus removal", 149m, ProductType.Service),
            ("Managed Services - Basic", "Basic managed IT services monthly", 999m, ProductType.ManagedService),
            ("Managed Services - Pro", "Professional managed services monthly", 2499m, ProductType.ManagedService),
            ("Managed Services - Enterprise", "Enterprise managed services monthly", 4999m, ProductType.ManagedService),
            ("Help Desk Support", "Monthly help desk support", 499m, ProductType.SupportContract),
            ("24/7 Support Contract", "Round-the-clock support", 1999m, ProductType.SupportContract),
            ("On-Site Support", "On-site technician support", 199m, ProductType.Service),
            ("Remote Support Hour", "Remote support per hour", 99m, ProductType.Service),
            ("Training - Basic IT", "Basic IT training session", 500m, ProductType.Training),
            ("Training - Advanced", "Advanced training session", 1000m, ProductType.Training),
            ("Training - Custom", "Custom training program", 2500m, ProductType.Training),
            ("Reseller - Hardware", "Hardware reseller services", 0m, ProductType.Service),
            ("Reseller - Software", "Software reseller services", 0m, ProductType.Service),
            ("Cloud Hosting - Basic", "Basic cloud hosting monthly", 99m, ProductType.Subscription),
            ("Cloud Hosting - Pro", "Professional cloud hosting", 299m, ProductType.Subscription),
            ("Cloud Hosting - Enterprise", "Enterprise cloud hosting", 999m, ProductType.Subscription),
            ("Backup Service - 100GB", "Cloud backup service", 29m, ProductType.Subscription),
            ("Backup Service - 1TB", "Cloud backup service", 99m, ProductType.Subscription),
            ("Disaster Recovery", "DR-as-a-Service monthly", 499m, ProductType.ManagedService),
            ("Compliance Consulting", "Regulatory compliance consulting", 300m, ProductType.Consulting),
            ("Project Management", "IT project management hourly", 175m, ProductType.ProfessionalServices),
        };

        skuCounter = 3001;
        foreach (var (name, desc, price, type) in serviceItems)
        {
            products.Add(new Product
            {
                Name = name,
                Description = desc,
                SKU = $"SVC-{skuCounter++}",
                Category = "IT Services",
                ProductType = type,
                Status = ProductStatus.Active,
                Price = price,
                IsService = true,
                IsSubscription = type == ProductType.Subscription || type == ProductType.ManagedService || type == ProductType.SupportContract,
                CreatedAt = DateTime.UtcNow
            });
        }

        _context.Products.AddRange(products);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} demo products", products.Count);
    }

    /// <summary>
    /// Seed service request categories and subcategories
    /// </summary>
    public async Task SeedServiceRequestCategoriesAsync()
    {
        _logger.LogInformation("Seeding service request categories...");
        
        var existingCategories = await _context.ServiceRequestCategories.CountAsync();
        if (existingCategories >= 5)
        {
            _logger.LogInformation("Service request categories already exist. Skipping...");
            return;
        }

        // Hardware Support
        var hardwareCategory = new ServiceRequestCategory
        {
            Name = "Hardware Support",
            Description = "Hardware repair, installation, and upgrade services",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.ServiceRequestCategories.Add(hardwareCategory);
        await _context.SaveChangesAsync();

        var hardwareSubcategories = new[]
        {
            "Desktop Repair", "Laptop Repair", "Server Repair", "Network Equipment Repair",
            "Printer Repair", "Hardware Installation", "Hardware Upgrade", "Component Replacement",
            "Warranty Claim", "Hardware Assessment"
        };
        foreach (var sub in hardwareSubcategories)
        {
            _context.ServiceRequestSubcategories.Add(new ServiceRequestSubcategory
            {
                CategoryId = hardwareCategory.Id,
                Name = sub,
                Description = $"{sub} service",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Software Support
        var softwareCategory = new ServiceRequestCategory
        {
            Name = "Software Support",
            Description = "Software installation, configuration, and troubleshooting",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.ServiceRequestCategories.Add(softwareCategory);
        await _context.SaveChangesAsync();

        var softwareSubcategories = new[]
        {
            "Software Installation", "Software Configuration", "Software Upgrade",
            "License Activation", "Software Troubleshooting", "Virus/Malware Removal",
            "Data Recovery", "Backup Configuration", "Email Setup"
        };
        foreach (var sub in softwareSubcategories)
        {
            _context.ServiceRequestSubcategories.Add(new ServiceRequestSubcategory
            {
                CategoryId = softwareCategory.Id,
                Name = sub,
                Description = $"{sub} service",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Managed Services
        var managedCategory = new ServiceRequestCategory
        {
            Name = "Managed Services",
            Description = "Ongoing managed IT services",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.ServiceRequestCategories.Add(managedCategory);
        await _context.SaveChangesAsync();

        var managedSubcategories = new[]
        {
            "Onboarding", "Service Review", "Escalation", "SLA Issue",
            "Monitoring Alert", "Maintenance Request", "Performance Issue",
            "Capacity Planning", "Monthly Review"
        };
        foreach (var sub in managedSubcategories)
        {
            _context.ServiceRequestSubcategories.Add(new ServiceRequestSubcategory
            {
                CategoryId = managedCategory.Id,
                Name = sub,
                Description = $"{sub} service",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Cloud Services
        var cloudCategory = new ServiceRequestCategory
        {
            Name = "Cloud Services",
            Description = "Cloud deployment, migration, and management",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.ServiceRequestCategories.Add(cloudCategory);
        await _context.SaveChangesAsync();

        var cloudSubcategories = new[]
        {
            "AWS Deployment", "Azure Deployment", "Google Cloud Deployment",
            "Cloud Migration", "Cloud Optimization", "Cloud Security",
            "Backup to Cloud", "Cloud Monitoring", "Cost Optimization"
        };
        foreach (var sub in cloudSubcategories)
        {
            _context.ServiceRequestSubcategories.Add(new ServiceRequestSubcategory
            {
                CategoryId = cloudCategory.Id,
                Name = sub,
                Description = $"{sub} service",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Network Services
        var networkCategory = new ServiceRequestCategory
        {
            Name = "Network Services",
            Description = "Network installation, configuration, and troubleshooting",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.ServiceRequestCategories.Add(networkCategory);
        await _context.SaveChangesAsync();

        var networkSubcategories = new[]
        {
            "Network Installation", "WiFi Setup", "Firewall Configuration",
            "VPN Setup", "Network Troubleshooting", "Cabling",
            "Network Assessment", "Bandwidth Upgrade", "Network Security"
        };
        foreach (var sub in networkSubcategories)
        {
            _context.ServiceRequestSubcategories.Add(new ServiceRequestSubcategory
            {
                CategoryId = networkCategory.Id,
                Name = sub,
                Description = $"{sub} service",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded service request categories");
    }

    /// <summary>
    /// Seed 100 customers (70 organizations, 30 individuals)
    /// </summary>
    public async Task SeedCustomersAsync()
    {
        _logger.LogInformation("Seeding demo customers...");
        
        var existingCustomers = await _context.Customers.CountAsync();
        if (existingCustomers >= 50)
        {
            _logger.LogInformation("Customers already exist. Skipping...");
            return;
        }

        var customers = new List<Customer>();
        var industries = new[] { "Technology", "Healthcare", "Finance", "Manufacturing", "Retail", "Education", "Legal", "Construction", "Media", "Transportation" };
        
        // 70 Organization Customers
        for (int i = 1; i <= 70; i++)
        {
            var prefix = CompanyPrefixes[_random.Next(CompanyPrefixes.Length)];
            var suffix = CompanySuffixes[_random.Next(CompanySuffixes.Length)];
            var cityIndex = _random.Next(Cities.Length);
            
            customers.Add(new Customer
            {
                Category = CustomerCategory.Organization,
                Company = $"{prefix} {suffix}",
                Email = $"info@{prefix.ToLower()}{suffix.ToLower()}.com",
                Phone = $"555-{_random.Next(100, 999)}-{_random.Next(1000, 9999)}",
                Address = $"{_random.Next(100, 9999)} {Streets[_random.Next(Streets.Length)]}",
                City = Cities[cityIndex],
                State = States[cityIndex],
                ZipCode = $"{_random.Next(10000, 99999)}",
                Country = "USA",
                Industry = industries[_random.Next(industries.Length)],
                CustomerType = (CustomerType)_random.Next(1, 5),
                Priority = (CustomerPriority)_random.Next(0, 4),
                LifecycleStage = CustomerLifecycleStage.Customer,
                AnnualRevenue = _random.Next(1, 100) * 100000m,
                NumberOfEmployees = _random.Next(10, 5000),
                CreatedAt = DateTime.UtcNow.AddDays(-_random.Next(1, 365))
            });
        }

        // 30 Individual Customers
        for (int i = 1; i <= 30; i++)
        {
            var firstName = FirstNames[_random.Next(FirstNames.Length)];
            var lastName = LastNames[_random.Next(LastNames.Length)];
            var cityIndex = _random.Next(Cities.Length);
            
            customers.Add(new Customer
            {
                Category = CustomerCategory.Individual,
                FirstName = firstName,
                LastName = lastName,
                Email = $"{firstName.ToLower()}.{lastName.ToLower()}@email.com",
                Phone = $"555-{_random.Next(100, 999)}-{_random.Next(1000, 9999)}",
                Address = $"{_random.Next(100, 9999)} {Streets[_random.Next(Streets.Length)]}",
                City = Cities[cityIndex],
                State = States[cityIndex],
                ZipCode = $"{_random.Next(10000, 99999)}",
                Country = "USA",
                CustomerType = CustomerType.Individual,
                Priority = (CustomerPriority)_random.Next(0, 3),
                LifecycleStage = CustomerLifecycleStage.Customer,
                CreatedAt = DateTime.UtcNow.AddDays(-_random.Next(1, 365))
            });
        }

        _context.Customers.AddRange(customers);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} demo customers", customers.Count);
    }

    /// <summary>
    /// Seed contacts for organization customers
    /// </summary>
    public async Task SeedContactsAsync()
    {
        _logger.LogInformation("Seeding demo contacts...");
        
        var existingContacts = await _context.Contacts.CountAsync();
        if (existingContacts >= 50)
        {
            _logger.LogInformation("Contacts already exist. Skipping...");
            return;
        }

        var orgCustomers = await _context.Customers
            .Where(c => c.Category == CustomerCategory.Organization)
            .Take(50)
            .ToListAsync();

        var titles = new[] { "CEO", "CTO", "CFO", "IT Director", "IT Manager", "Procurement Manager", "Operations Manager", "Project Manager", "Technical Lead", "System Administrator" };
        var contactTypes = new[] { CRM.Core.Models.ContactType.Employee, CRM.Core.Models.ContactType.Partner, CRM.Core.Models.ContactType.Customer };

        foreach (var customer in orgCustomers)
        {
            // 2-4 contacts per organization
            var contactCount = _random.Next(2, 5);
            
            for (int i = 0; i < contactCount; i++)
            {
                var firstName = FirstNames[_random.Next(FirstNames.Length)];
                var lastName = LastNames[_random.Next(LastNames.Length)];
                
                var contact = new CRM.Core.Models.Contact
                {
                    FirstName = firstName,
                    LastName = lastName,
                    EmailPrimary = $"{firstName.ToLower()}.{lastName.ToLower()}@{customer.Company.Replace(" ", "").ToLower()}.com",
                    PhonePrimary = $"555-{_random.Next(100, 999)}-{_random.Next(1000, 9999)}",
                    ContactType = contactTypes[_random.Next(contactTypes.Length)],
                    Status = CRM.Core.Models.ContactStatus.Active,
                    Company = customer.Company,
                    JobTitle = titles[_random.Next(titles.Length)],
                    Address = customer.Address,
                    City = customer.City,
                    State = customer.State,
                    Country = customer.Country,
                    ZipCode = customer.ZipCode,
                };
                
                _context.Contacts.Add(contact);
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded customer contacts");
    }

    /// <summary>
    /// Seed 100 leads (60 organizations, 40 individuals)
    /// </summary>
    public async Task SeedLeadsAsync()
    {
        _logger.LogInformation("Seeding demo leads...");
        
        var existingLeads = await _context.Leads.CountAsync();
        if (existingLeads >= 50)
        {
            _logger.LogInformation("Leads already exist. Skipping...");
            return;
        }

        var leads = new List<Lead>();
        var leadStatuses = new[] { LeadStatus.New, LeadStatus.Contacted, LeadStatus.Responded, LeadStatus.Engaged, LeadStatus.Working, LeadStatus.Qualified, LeadStatus.Nurturing };
        var leadSources = new[] { LeadSource.WebsiteForm, LeadSource.Referral, LeadSource.TradeShow, LeadSource.EmailCampaign, LeadSource.PhoneInbound, LeadSource.Partner, LeadSource.LiveChat, LeadSource.DemoRequest };
        var leadRatings = new[] { LeadRating.Cold, LeadRating.Warm, LeadRating.Hot };
        var leadIndustries = new[] { LeadIndustry.Technology, LeadIndustry.Healthcare, LeadIndustry.Finance, LeadIndustry.Manufacturing, LeadIndustry.Retail, LeadIndustry.Education };

        // 60 Organization Leads
        for (int i = 1; i <= 60; i++)
        {
            var prefix = CompanyPrefixes[_random.Next(CompanyPrefixes.Length)];
            var suffix = CompanySuffixes[_random.Next(CompanySuffixes.Length)];
            var firstName = FirstNames[_random.Next(FirstNames.Length)];
            var lastName = LastNames[_random.Next(LastNames.Length)];
            var cityIndex = _random.Next(Cities.Length);
            
            leads.Add(new Lead
            {
                FirstName = firstName,
                LastName = lastName,
                Email = $"{firstName.ToLower()}.{lastName.ToLower()}@{prefix.ToLower()}{suffix.ToLower()}.com",
                Phone = $"555-{_random.Next(100, 999)}-{_random.Next(1000, 9999)}",
                Company = $"{prefix} {suffix}",
                JobTitle = new[] { "IT Director", "CTO", "IT Manager", "Procurement Manager", "Owner" }[_random.Next(5)],
                Industry = leadIndustries[_random.Next(leadIndustries.Length)],
                Address = $"{_random.Next(100, 9999)} {Streets[_random.Next(Streets.Length)]}",
                City = Cities[cityIndex],
                State = States[cityIndex],
                ZipCode = $"{_random.Next(10000, 99999)}",
                Country = "USA",
                Status = leadStatuses[_random.Next(leadStatuses.Length)],
                Source = leadSources[_random.Next(leadSources.Length)],
                Rating = leadRatings[_random.Next(leadRatings.Length)],
                LeadScore = _random.Next(10, 100),
                CreatedAt = DateTime.UtcNow.AddDays(-_random.Next(1, 180))
            });
        }

        // 40 Individual Leads
        for (int i = 1; i <= 40; i++)
        {
            var firstName = FirstNames[_random.Next(FirstNames.Length)];
            var lastName = LastNames[_random.Next(LastNames.Length)];
            var cityIndex = _random.Next(Cities.Length);
            
            leads.Add(new Lead
            {
                FirstName = firstName,
                LastName = lastName,
                Email = $"{firstName.ToLower()}.{lastName.ToLower()}@personalmail.com",
                Phone = $"555-{_random.Next(100, 999)}-{_random.Next(1000, 9999)}",
                Company = "Individual",
                Address = $"{_random.Next(100, 9999)} {Streets[_random.Next(Streets.Length)]}",
                City = Cities[cityIndex],
                State = States[cityIndex],
                ZipCode = $"{_random.Next(10000, 99999)}",
                Country = "USA",
                Status = leadStatuses[_random.Next(leadStatuses.Length)],
                Source = leadSources[_random.Next(leadSources.Length)],
                Rating = leadRatings[_random.Next(leadRatings.Length)],
                LeadScore = _random.Next(10, 80),
                CreatedAt = DateTime.UtcNow.AddDays(-_random.Next(1, 180))
            });
        }

        _context.Leads.AddRange(leads);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} demo leads", leads.Count);
    }

    /// <summary>
    /// Seed 100 opportunities linked to customers
    /// </summary>
    public async Task SeedOpportunitiesAsync()
    {
        _logger.LogInformation("Seeding demo opportunities...");
        
        var existingOpportunities = await _context.Opportunities.CountAsync();
        if (existingOpportunities >= 50)
        {
            _logger.LogInformation("Opportunities already exist. Skipping...");
            return;
        }

        var customers = await _context.Customers.Take(100).ToListAsync();
        if (!customers.Any())
        {
            _logger.LogWarning("No customers found. Skipping opportunities seeding.");
            return;
        }

        var opportunities = new List<Opportunity>();
        var stages = new[] { OpportunityStage.Prospecting, OpportunityStage.Qualification, OpportunityStage.NeedsAnalysis, OpportunityStage.ValueProposition, OpportunityStage.ProposalQuote, OpportunityStage.NegotiationReview };
        var oppTypes = new[] { OpportunityType.NewBusiness, OpportunityType.ExistingBusiness, OpportunityType.Renewal };
        var priorities = new[] { OpportunityPriority.Low, OpportunityPriority.Medium, OpportunityPriority.High };

        for (int i = 1; i <= 100; i++)
        {
            var customer = customers[_random.Next(customers.Count)];
            var stage = stages[_random.Next(stages.Length)];
            var amount = _random.Next(1, 50) * 1000m;
            var probability = stage switch
            {
                OpportunityStage.Prospecting => 10,
                OpportunityStage.Qualification => 20,
                OpportunityStage.NeedsAnalysis => 40,
                OpportunityStage.ValueProposition => 60,
                OpportunityStage.ProposalQuote => 75,
                OpportunityStage.NegotiationReview => 90,
                _ => 50
            };

            opportunities.Add(new Opportunity
            {
                Name = $"{customer.DisplayName} - {new[] { "Server Upgrade", "Software License", "Managed Services", "Cloud Migration", "Support Contract", "Hardware Purchase", "IT Assessment", "Security Audit" }[_random.Next(8)]}",
                CustomerId = customer.Id,
                Description = "Demo opportunity for testing",
                Amount = amount,
                ExpectedRevenue = amount * (decimal)(probability / 100.0),
                Stage = stage,
                Probability = probability,
                OpportunityType = oppTypes[_random.Next(oppTypes.Length)],
                Priority = priorities[_random.Next(priorities.Length)],
                CloseDate = DateTime.UtcNow.AddDays(_random.Next(7, 90)),
                CreatedAt = DateTime.UtcNow.AddDays(-_random.Next(1, 60))
            });
        }

        _context.Opportunities.AddRange(opportunities);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} demo opportunities", opportunities.Count);
    }

    /// <summary>
    /// Clear demo user data
    /// </summary>
    public async Task ClearDemoDataAsync()
    {
        _logger.LogInformation("Clearing demo data...");
        
        // Remove demo users
        var demoUsers = await _context.Users.Where(u => u.Username.StartsWith("demo.")).ToListAsync();
        foreach (var user in demoUsers)
        {
            user.IsDeleted = true;
            user.UpdatedAt = DateTime.UtcNow;
        }
        
        // Update settings
        var settings = await _context.SystemSettings.FirstOrDefaultAsync();
        if (settings != null)
        {
            settings.DemoDataSeeded = false;
            settings.UseDemoDatabase = false;
        }
        
        await _context.SaveChangesAsync();
        _logger.LogInformation("Demo data cleared");
    }
}
