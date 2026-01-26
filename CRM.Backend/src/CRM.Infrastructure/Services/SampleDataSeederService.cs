using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service to seed comprehensive sample data for the CRM system.
/// Seeds data directly to the production database.
/// Includes 100+ entries for products, services, customers, contacts, leads, opportunities, etc.
/// Admin users can clear all sample data while preserving master data (ZipCodes, ColorPalettes).
/// </summary>
public class SampleDataSeederService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<SampleDataSeederService> _logger;
    private readonly Random _random = new();
    
    // Sample company and person name data
    private static readonly string[] CompanyPrefixes = { "Tech", "Global", "Digital", "Smart", "Cloud", "Cyber", "Data", "Info", "Net", "Soft", "Pro", "Prime", "Elite", "Advanced", "Modern", "Innovative", "Strategic", "Dynamic", "Unified", "Integrated" };
    private static readonly string[] CompanySuffixes = { "Solutions", "Systems", "Technologies", "Services", "Consulting", "Partners", "Group", "Corp", "Inc", "Labs", "Networks", "Dynamics", "Enterprises", "Associates", "Innovations" };
    private static readonly string[] FirstNames = { "James", "Mary", "John", "Patricia", "Robert", "Jennifer", "Michael", "Linda", "William", "Elizabeth", "David", "Barbara", "Richard", "Susan", "Joseph", "Jessica", "Thomas", "Sarah", "Charles", "Karen", "Christopher", "Nancy", "Daniel", "Lisa", "Matthew", "Betty", "Anthony", "Margaret", "Mark", "Sandra", "Donald", "Ashley", "Steven", "Kimberly", "Paul", "Emily", "Andrew", "Donna", "Joshua", "Michelle" };
    private static readonly string[] LastNames = { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson", "Thomas", "Taylor", "Moore", "Jackson", "Martin", "Lee", "Perez", "Thompson", "White", "Harris", "Sanchez", "Clark", "Ramirez", "Lewis", "Robinson" };
    private static readonly string[] Streets = { "Main St", "Oak Ave", "Technology Blvd", "Innovation Way", "Business Park Dr", "Corporate Center", "Commerce St", "Enterprise Rd", "Industrial Blvd", "Silicon Valley Way" };
    private static readonly string[] Cities = { "New York", "Los Angeles", "Chicago", "Houston", "Phoenix", "Philadelphia", "San Antonio", "San Diego", "Dallas", "San Jose", "Austin", "Seattle", "Denver", "Boston", "Atlanta", "Miami", "Portland", "Minneapolis", "Detroit", "Charlotte" };
    private static readonly string[] States = { "NY", "CA", "IL", "TX", "AZ", "PA", "TX", "CA", "TX", "CA", "TX", "WA", "CO", "MA", "GA", "FL", "OR", "MN", "MI", "NC" };
    
    public SampleDataSeederService(
        ICrmDbContext context, 
        ILogger<SampleDataSeederService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Seed all sample data to the production database
    /// </summary>
    public async Task SeedAllSampleDataAsync()
    {
        _logger.LogInformation("Starting sample data seeding to production database...");
        
        try
        {
            var dbContext = _context as CrmDbContext ?? throw new InvalidOperationException("Context must be CrmDbContext");
            
            // Check if sample data already exists
            var settings = await dbContext.SystemSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new SystemSettings
                {
                    SampleDataSeeded = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                dbContext.SystemSettings.Add(settings);
                await dbContext.SaveChangesAsync();
            }

            // Seed in order to maintain relationships
            await SeedSampleUsersToContextAsync(dbContext);
            await SeedProductsToContextAsync(dbContext);
            await SeedServiceRequestCategoriesToContextAsync(dbContext);
            await SeedCustomersToContextAsync(dbContext);
            await SeedContactsToContextAsync(dbContext);
            await SeedLeadsToContextAsync(dbContext);
            await SeedOpportunitiesToContextAsync(dbContext);
            
            // Update settings to mark sample data as seeded
            settings.SampleDataSeeded = true;
            settings.SampleDataLastSeeded = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();
            
            _logger.LogInformation("Sample data seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding sample data");
            throw;
        }
    }

    /// <summary>
    /// Check if sample data has been seeded
    /// </summary>
    public async Task<bool> IsSampleDataSeededAsync()
    {
        var dbContext = _context as CrmDbContext ?? throw new InvalidOperationException("Context must be CrmDbContext");
        var settings = await dbContext.SystemSettings.FirstOrDefaultAsync();
        return settings?.SampleDataSeeded ?? false;
    }

    /// <summary>
    /// Seed sample users
    /// </summary>
    public async Task SeedSampleUsersAsync()
    {
        var dbContext = _context as CrmDbContext ?? throw new InvalidOperationException("Context must be CrmDbContext");
        await SeedSampleUsersToContextAsync(dbContext);
    }

    /// <summary>
    /// Seed 10 sample users with different groups and profiles
    /// </summary>
    private async Task SeedSampleUsersToContextAsync(CrmDbContext context)
    {
        _logger.LogInformation("Seeding sample users...");
        
        // Check if demo users already exist
        var existingDemoUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "demo.admin");
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
            var existing = await context.UserGroups.FirstOrDefaultAsync(g => g.Name == groupName);
            if (existing == null)
            {
                var group = new UserGroup
                {
                    Name = groupName,
                    Description = $"Demo {groupName} group",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                context.UserGroups.Add(group);
                await context.SaveChangesAsync();
                groupIds[groupName] = group.Id;
            }
            else
            {
                groupIds[groupName] = existing.Id;
            }
        }

        // Create a demo department if needed
        var department = await context.Departments.FirstOrDefaultAsync(d => d.Name == "Demo Department");
        if (department == null)
        {
            department = new Department
            {
                Name = "Demo Department",
                Description = "Department for demo users",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            context.Departments.Add(department);
            await context.SaveChangesAsync();
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
            
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Add to group membership
            var membership = new UserGroupMember
            {
                UserId = user.Id,
                UserGroupId = groupIds[groupName],
                AddedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            context.UserGroupMembers.Add(membership);
        }
        
        await context.SaveChangesAsync();
        _logger.LogInformation("Seeded 10 sample users");
    }

    /// <summary>
    /// Seed products to production database
    /// </summary>
    public async Task SeedProductsAsync()
    {
        var dbContext = _context as CrmDbContext ?? throw new InvalidOperationException("Context must be CrmDbContext");
        await SeedProductsToContextAsync(dbContext);
    }

    /// <summary>
    /// Seed 100 products: 30 Hardware, 35 Software, 35 IT Services
    /// </summary>
    private async Task SeedProductsToContextAsync(CrmDbContext context)
    {
        _logger.LogInformation("Seeding sample products...");
        
        var existingProducts = await context.Products.CountAsync();
        if (existingProducts >= 50)
        {
            _logger.LogInformation("Products already exist. Skipping...");
            return;
        }

        var products = new List<Product>();
        int skuCounter = 1001;

        // =============================================================================
        // HARDWARE PRODUCTS
        // =============================================================================
        
        // End-User Computing
        var endUserComputing = new[]
        {
            ("Business Laptop Pro 15", "High-performance business laptop with security features, Intel Core i7, 16GB RAM, 512GB SSD", 1899.99m, "End-User Computing"),
            ("Enterprise Desktop D7", "Tower workstation for office productivity, Intel Core i5, 8GB RAM, 256GB SSD", 1199.99m, "End-User Computing"),
            ("Workstation Elite X1", "Professional workstation for CAD/3D/development, Xeon processor, 32GB RAM, 1TB NVMe", 3499.99m, "End-User Computing"),
            ("Thin Client Terminal", "Virtual desktop terminal for VDI environments", 549.99m, "End-User Computing"),
            ("All-in-One PC 27-inch", "Space-saving all-in-one computer with 4K display", 1599.99m, "End-User Computing"),
        };
        foreach (var (name, desc, price, subcat) in endUserComputing)
        {
            products.Add(new Product { Name = name, Description = $"{desc}. Per Unit + 15-25% Markup. Typical range: $500-$3000/device", SKU = $"HW-{skuCounter++}", Category = "Hardware", ProductType = ProductType.Physical, Status = ProductStatus.Active, Price = price, IsService = false, CreatedAt = DateTime.UtcNow });
        }

        // Networking Equipment
        var networkingEquipment = new[]
        {
            ("Network Switch 48-Port Managed", "Enterprise managed switch with PoE+ and VLAN support", 1299.99m, "Networking Equipment"),
            ("Network Switch 24-Port Unmanaged", "Basic network switch for small deployments", 299.99m, "Networking Equipment"),
            ("Router Enterprise Edge", "Enterprise edge router with advanced routing protocols", 2499.99m, "Networking Equipment"),
            ("Core Switch 10GbE", "High-performance core switch for data center", 12999.99m, "Networking Equipment"),
            ("Wireless Access Point Pro", "Enterprise WiFi 6E access point with cloud management", 599.99m, "Networking Equipment"),
            ("Wireless Controller", "Centralized wireless network controller", 4999.99m, "Networking Equipment"),
            ("Load Balancer Appliance", "Application delivery and load balancing", 8999.99m, "Networking Equipment"),
            ("SD-WAN Edge Device", "Software-defined WAN edge appliance", 3499.99m, "Networking Equipment"),
        };
        foreach (var (name, desc, price, subcat) in networkingEquipment)
        {
            products.Add(new Product { Name = name, Description = $"{desc}. Per Unit + 20-30% Markup. Typical range: $200-$50000/device", SKU = $"HW-{skuCounter++}", Category = "Hardware", ProductType = ProductType.Physical, Status = ProductStatus.Active, Price = price, IsService = false, CreatedAt = DateTime.UtcNow });
        }

        // Server Infrastructure
        var serverInfrastructure = new[]
        {
            ("Enterprise Server Pro", "High-performance dual-socket server, 2x Xeon Gold, 256GB RAM, 8x SAS bays", 18999.99m, "Server Infrastructure"),
            ("Rack Server 1U Entry", "Entry-level rack server for SMB deployments", 4999.99m, "Server Infrastructure"),
            ("Blade Server Chassis", "High-density blade server enclosure, holds 16 blades", 45999.99m, "Server Infrastructure"),
            ("GPU Compute Server", "AI/ML optimized server with 8x GPU slots", 75999.99m, "Server Infrastructure"),
            ("Storage Array NAS-500", "Network-attached storage with 50TB raw capacity", 12999.99m, "Server Infrastructure"),
            ("SAN Storage System", "Enterprise SAN with FC connectivity, 100TB usable", 89999.99m, "Server Infrastructure"),
            ("Hyper-Converged Node", "HCI appliance with compute, storage, and networking", 35999.99m, "Server Infrastructure"),
        };
        foreach (var (name, desc, price, subcat) in serverInfrastructure)
        {
            products.Add(new Product { Name = name, Description = $"{desc}. Per Unit + 20-35% Markup. Typical range: $2000-$100000/server", SKU = $"HW-{skuCounter++}", Category = "Hardware", ProductType = ProductType.Physical, Status = ProductStatus.Active, Price = price, IsService = false, CreatedAt = DateTime.UtcNow });
        }

        // Data Center Equipment
        var dataCenterEquipment = new[]
        {
            ("Rack Cabinet 42U", "Enterprise server rack with cable management and cooling", 2599.99m, "Data Center Equipment"),
            ("UPS Battery Backup 10kVA", "Online double-conversion UPS with extended runtime", 8999.99m, "Data Center Equipment"),
            ("PDU Rack Mount Metered", "Intelligent rack power distribution unit", 899.99m, "Data Center Equipment"),
            ("Precision Cooling Unit", "In-row cooling for data center hot spots", 25999.99m, "Data Center Equipment"),
            ("KVM Over IP 32-Port", "Remote server management console", 4999.99m, "Data Center Equipment"),
            ("Environmental Monitor", "Data center environmental monitoring sensors", 1599.99m, "Data Center Equipment"),
            ("Fire Suppression System", "Clean agent fire suppression for server rooms", 45999.99m, "Data Center Equipment"),
        };
        foreach (var (name, desc, price, subcat) in dataCenterEquipment)
        {
            products.Add(new Product { Name = name, Description = $"{desc}. Per Unit + 25-40% Markup. Typical range: $5000-$500000/system", SKU = $"HW-{skuCounter++}", Category = "Hardware", ProductType = ProductType.Physical, Status = ProductStatus.Active, Price = price, IsService = false, CreatedAt = DateTime.UtcNow });
        }

        // Mobile & Telecommunications
        var mobileTelecom = new[]
        {
            ("Business Smartphone Pro", "Enterprise smartphone with MDM support and enhanced security", 1099.99m, "Mobile & Telecommunications"),
            ("Rugged Tablet 10-inch", "Industrial tablet for field workers, IP67 rated", 1499.99m, "Mobile & Telecommunications"),
            ("IP Desk Phone Executive", "HD voice desk phone with video capability", 449.99m, "Mobile & Telecommunications"),
            ("Wireless Headset UC", "Unified communications wireless headset", 299.99m, "Mobile & Telecommunications"),
            ("Mobile Hotspot Enterprise", "Secure mobile hotspot with enterprise management", 399.99m, "Mobile & Telecommunications"),
            ("Conference Speakerphone", "360-degree conference room speakerphone", 599.99m, "Mobile & Telecommunications"),
        };
        foreach (var (name, desc, price, subcat) in mobileTelecom)
        {
            products.Add(new Product { Name = name, Description = $"{desc}. Per Unit + 10-20% Markup. Typical range: $300-$2000/device", SKU = $"HW-{skuCounter++}", Category = "Hardware", ProductType = ProductType.Physical, Status = ProductStatus.Active, Price = price, IsService = false, CreatedAt = DateTime.UtcNow });
        }

        // Printing & Imaging
        var printingImaging = new[]
        {
            ("Laser Printer Enterprise", "High-volume enterprise laser printer, 60 ppm", 2499.99m, "Printing & Imaging"),
            ("MFP Color Enterprise", "Multi-function color printer with scanning and fax", 4999.99m, "Printing & Imaging"),
            ("Document Scanner Pro", "High-speed duplex document scanner, 100 ppm", 2999.99m, "Printing & Imaging"),
            ("Wide Format Printer", "Large format printer for CAD/engineering", 8999.99m, "Printing & Imaging"),
            ("Label Printer Industrial", "Industrial label and barcode printer", 1299.99m, "Printing & Imaging"),
            ("Desktop Printer Workgroup", "Workgroup laser printer, 40 ppm", 699.99m, "Printing & Imaging"),
        };
        foreach (var (name, desc, price, subcat) in printingImaging)
        {
            products.Add(new Product { Name = name, Description = $"{desc}. Per Unit + 15-25% Markup. Typical range: $200-$15000/device", SKU = $"HW-{skuCounter++}", Category = "Hardware", ProductType = ProductType.Physical, Status = ProductStatus.Active, Price = price, IsService = false, CreatedAt = DateTime.UtcNow });
        }

        // Point of Sale & Specialized
        var posSpecialized = new[]
        {
            ("POS Terminal Complete", "All-in-one POS system with receipt printer and cash drawer", 1899.99m, "Point of Sale & Specialized"),
            ("Barcode Scanner Wireless", "Enterprise wireless barcode scanner", 449.99m, "Point of Sale & Specialized"),
            ("Digital Signage Display 55", "Commercial-grade 55-inch digital signage display", 2999.99m, "Point of Sale & Specialized"),
            ("Kiosk Terminal Interactive", "Self-service interactive kiosk with touchscreen", 5999.99m, "Point of Sale & Specialized"),
            ("Time Clock Biometric", "Biometric employee time clock system", 899.99m, "Point of Sale & Specialized"),
            ("Receipt Printer Thermal", "High-speed thermal receipt printer", 349.99m, "Point of Sale & Specialized"),
        };
        foreach (var (name, desc, price, subcat) in posSpecialized)
        {
            products.Add(new Product { Name = name, Description = $"{desc}. Per Unit + 20-30% Markup. Typical range: $500-$10000/system", SKU = $"HW-{skuCounter++}", Category = "Hardware", ProductType = ProductType.Physical, Status = ProductStatus.Active, Price = price, IsService = false, CreatedAt = DateTime.UtcNow });
        }

        // =============================================================================
        // SOFTWARE PRODUCTS
        // =============================================================================
        skuCounter = 2001;

        // Operating Systems
        var operatingSystems = new[]
        {
            ("Windows Server 2022 Standard", "Windows Server standard edition license, 16 cores", 999.99m, ProductType.License),
            ("Windows Server 2022 Datacenter", "Windows Server datacenter edition license", 6499.99m, ProductType.License),
            ("Windows 11 Pro", "Windows 11 Professional license", 199.99m, ProductType.License),
            ("Red Hat Enterprise Linux", "RHEL subscription with support, per socket", 799.99m, ProductType.License),
            ("VMware vSphere Enterprise Plus", "vSphere virtualization license per CPU", 4299.99m, ProductType.License),
        };
        foreach (var (name, desc, price, type) in operatingSystems)
        {
            products.Add(new Product { Name = name, Description = $"{desc}. Per License + 10-20% Markup. Typical range: $50-$500/license", SKU = $"SW-{skuCounter++}", Category = "Software", ProductType = type, Status = ProductStatus.Active, Price = price, IsService = false, IsSubscription = false, CreatedAt = DateTime.UtcNow });
        }

        // Productivity & Collaboration
        var productivityCollab = new[]
        {
            ("Microsoft 365 Business Premium", "Complete productivity and security suite, per user/month", 22.99m, ProductType.Subscription),
            ("Microsoft 365 E3", "Enterprise productivity with advanced compliance, per user/month", 36.99m, ProductType.Subscription),
            ("Google Workspace Business Plus", "Google productivity suite with enhanced storage, per user/month", 18.99m, ProductType.Subscription),
            ("Slack Business+", "Team collaboration platform, per user/month", 12.50m, ProductType.Subscription),
            ("Zoom Business", "Video conferencing platform, per user/month", 19.99m, ProductType.Subscription),
            ("Asana Business", "Project management platform, per user/month", 24.99m, ProductType.Subscription),
        };
        foreach (var (name, desc, price, type) in productivityCollab)
        {
            products.Add(new Product { Name = name, Description = $"{desc}. Annual Subscription + 15-25% Margin. Typical range: $10-$50/user/month", SKU = $"SW-{skuCounter++}", Category = "Software", ProductType = type, Status = ProductStatus.Active, Price = price, IsService = false, IsSubscription = true, CreatedAt = DateTime.UtcNow });
        }

        // Security Software
        var securitySoftware = new[]
        {
            ("CrowdStrike Falcon Pro", "Next-gen endpoint protection, per endpoint/year", 89.99m, ProductType.Subscription),
            ("SentinelOne Complete", "AI-powered endpoint security, per endpoint/year", 79.99m, ProductType.Subscription),
            ("Proofpoint Email Protection", "Advanced email security, per user/year", 45.99m, ProductType.Subscription),
            ("Okta Identity Cloud", "Identity and access management, per user/month", 8.99m, ProductType.Subscription),
            ("KnowBe4 Security Awareness", "Security awareness training, per user/year", 29.99m, ProductType.Subscription),
            ("Mimecast Email Security", "Email security and archiving, per user/year", 55.99m, ProductType.Subscription),
        };
        foreach (var (name, desc, price, type) in securitySoftware)
        {
            products.Add(new Product { Name = name, Description = $"{desc}. Annual Subscription + 20-30% Margin. Typical range: $30-$150/user/year", SKU = $"SW-{skuCounter++}", Category = "Software", ProductType = type, Status = ProductStatus.Active, Price = price, IsService = false, IsSubscription = true, CreatedAt = DateTime.UtcNow });
        }

        // Business Applications
        var businessApps = new[]
        {
            ("Salesforce Sales Cloud", "CRM and sales automation, per user/month", 150.99m, ProductType.Subscription),
            ("SAP Business One", "ERP for small/medium business, per user/month", 249.99m, ProductType.Subscription),
            ("HubSpot Marketing Hub", "Marketing automation platform, per month", 890.99m, ProductType.Subscription),
            ("QuickBooks Online Advanced", "Accounting and financial management, per month", 180.99m, ProductType.Subscription),
            ("ServiceNow ITSM Pro", "IT service management, per user/month", 99.99m, ProductType.Subscription),
            ("Workday HCM", "Human capital management, per user/month", 85.99m, ProductType.Subscription),
        };
        foreach (var (name, desc, price, type) in businessApps)
        {
            products.Add(new Product { Name = name, Description = $"{desc}. Per License + 15-30% Markup. Typical range: $50-$500/user/month", SKU = $"SW-{skuCounter++}", Category = "Software", ProductType = type, Status = ProductStatus.Active, Price = price, IsService = false, IsSubscription = true, CreatedAt = DateTime.UtcNow });
        }

        // Development & DevOps
        var devDevOps = new[]
        {
            ("GitHub Enterprise Cloud", "Source control and collaboration, per user/month", 21.99m, ProductType.Subscription),
            ("Azure DevOps Services", "DevOps platform, per user/month", 30.99m, ProductType.Subscription),
            ("JetBrains All Products Pack", "Complete IDE suite, per user/year", 649.99m, ProductType.Subscription),
            ("Docker Business", "Container platform, per user/month", 24.99m, ProductType.Subscription),
            ("HashiCorp Terraform Cloud", "Infrastructure as code, per user/month", 70.99m, ProductType.Subscription),
            ("Atlassian Jira Premium", "Project tracking and agile management, per user/month", 14.99m, ProductType.Subscription),
        };
        foreach (var (name, desc, price, type) in devDevOps)
        {
            products.Add(new Product { Name = name, Description = $"{desc}. Per License + 15-25% Markup. Typical range: $25-$200/user/month", SKU = $"SW-{skuCounter++}", Category = "Software", ProductType = type, Status = ProductStatus.Active, Price = price, IsService = false, IsSubscription = true, CreatedAt = DateTime.UtcNow });
        }

        // Backup & Recovery
        var backupRecovery = new[]
        {
            ("Veeam Backup Enterprise Plus", "Enterprise backup solution, per VM/month", 18.99m, ProductType.Subscription),
            ("Acronis Cyber Backup", "Backup with cybersecurity, per device/month", 12.99m, ProductType.Subscription),
            ("Commvault Complete Backup", "Enterprise data protection, per TB/month", 35.99m, ProductType.Subscription),
            ("Datto SIRIS", "Business continuity and disaster recovery, per device/month", 89.99m, ProductType.Subscription),
            ("Druva Cloud Platform", "Cloud-native data protection, per TB/month", 25.99m, ProductType.Subscription),
            ("Carbonite Backup for Business", "Cloud backup for endpoints, per device/month", 9.99m, ProductType.Subscription),
        };
        foreach (var (name, desc, price, type) in backupRecovery)
        {
            products.Add(new Product { Name = name, Description = $"{desc}. Per TB or Per Device + 20-30% Margin. Typical range: $5-$50/TB/month", SKU = $"SW-{skuCounter++}", Category = "Software", ProductType = type, Status = ProductStatus.Active, Price = price, IsService = false, IsSubscription = true, CreatedAt = DateTime.UtcNow });
        }

        // Network & Systems Management
        var networkSysMgmt = new[]
        {
            ("SolarWinds NPM", "Network performance monitoring, per device/month", 12.99m, ProductType.Subscription),
            ("Datadog Infrastructure", "Infrastructure monitoring, per host/month", 18.99m, ProductType.Subscription),
            ("PRTG Network Monitor", "All-in-one network monitoring, per sensor", 1.99m, ProductType.License),
            ("ManageEngine OpManager", "Network management, per device/month", 8.99m, ProductType.Subscription),
            ("Splunk Enterprise", "Log management and analytics, per GB/day", 150.99m, ProductType.Subscription),
            ("New Relic One", "Observability platform, per user/month", 99.99m, ProductType.Subscription),
        };
        foreach (var (name, desc, price, type) in networkSysMgmt)
        {
            products.Add(new Product { Name = name, Description = $"{desc}. Per Device or Subscription + 20-30%. Typical range: $5-$25/device/month", SKU = $"SW-{skuCounter++}", Category = "Software", ProductType = type, Status = ProductStatus.Active, Price = price, IsService = false, IsSubscription = type == ProductType.Subscription, CreatedAt = DateTime.UtcNow });
        }

        // =============================================================================
        // CLOUD SERVICES
        // =============================================================================
        skuCounter = 3001;

        // Infrastructure as a Service (IaaS)
        var iaas = new[]
        {
            ("Azure Virtual Machines", "Microsoft Azure VM hosting, monthly usage-based", 500.99m, ProductType.Subscription),
            ("AWS EC2 Instances", "Amazon EC2 compute instances, monthly usage-based", 450.99m, ProductType.Subscription),
            ("Google Compute Engine", "GCP virtual machines, monthly usage-based", 420.99m, ProductType.Subscription),
            ("Azure Storage Account", "Cloud storage services, per TB/month", 25.99m, ProductType.Subscription),
            ("AWS S3 Storage", "Object storage, per TB/month", 23.99m, ProductType.Subscription),
        };
        foreach (var (name, desc, price, type) in iaas)
        {
            products.Add(new Product { Name = name, Description = $"{desc}. Monthly Usage + 15-30% Margin. Typical range: $50-$10000/month", SKU = $"CLD-{skuCounter++}", Category = "Cloud Services", ProductType = type, Status = ProductStatus.Active, Price = price, IsService = true, IsSubscription = true, CreatedAt = DateTime.UtcNow });
        }

        // Platform as a Service (PaaS)
        var paas = new[]
        {
            ("Azure App Service", "Web application hosting platform, per instance/month", 89.99m, ProductType.Subscription),
            ("AWS Elastic Beanstalk", "Application deployment platform, monthly", 120.99m, ProductType.Subscription),
            ("Heroku Enterprise", "Cloud application platform, per dyno/month", 250.99m, ProductType.Subscription),
            ("Azure SQL Database", "Managed SQL database, per DTU/month", 150.99m, ProductType.Subscription),
            ("AWS RDS", "Managed relational database service, monthly", 180.99m, ProductType.Subscription),
        };
        foreach (var (name, desc, price, type) in paas)
        {
            products.Add(new Product { Name = name, Description = $"{desc}. Monthly Usage + 15-25% Margin. Typical range: $100-$5000/month", SKU = $"CLD-{skuCounter++}", Category = "Cloud Services", ProductType = type, Status = ProductStatus.Active, Price = price, IsService = true, IsSubscription = true, CreatedAt = DateTime.UtcNow });
        }

        // Software as a Service (SaaS)
        var saas = new[]
        {
            ("Dropbox Business Advanced", "Cloud file sync and sharing, per user/month", 19.99m, ProductType.Subscription),
            ("Box Business Plus", "Enterprise content management, per user/month", 25.99m, ProductType.Subscription),
            ("Freshdesk Pro", "Customer support platform, per agent/month", 49.99m, ProductType.Subscription),
            ("Monday.com Pro", "Work management platform, per user/month", 16.99m, ProductType.Subscription),
            ("DocuSign Business Pro", "Electronic signature platform, per user/month", 40.99m, ProductType.Subscription),
        };
        foreach (var (name, desc, price, type) in saas)
        {
            products.Add(new Product { Name = name, Description = $"{desc}. Per User Subscription + 15-25% Margin. Typical range: $10-$100/user/month", SKU = $"CLD-{skuCounter++}", Category = "Cloud Services", ProductType = type, Status = ProductStatus.Active, Price = price, IsService = true, IsSubscription = true, CreatedAt = DateTime.UtcNow });
        }

        // =============================================================================
        // CONSULTING & STRATEGY SERVICES
        // =============================================================================
        skuCounter = 4001;

        var consultingStrategy = new[]
        {
            ("IT Strategy & Roadmap Development", "Comprehensive IT strategy and multi-year technology roadmap", 25000.99m, ProductType.Consulting, "$150-$350/hour or project-based"),
            ("Technology Assessment & Audit", "Complete assessment of current IT infrastructure and systems", 15000.99m, ProductType.Consulting, "$5000-$50000/project"),
            ("Digital Transformation Consulting", "End-to-end digital transformation strategy and planning", 75000.99m, ProductType.Consulting, "$10000-$200000/project"),
            ("Cloud Migration Planning", "Cloud readiness assessment and migration strategy", 18000.99m, ProductType.Consulting, "$5000-$75000/project"),
            ("Cybersecurity Consulting", "Security strategy, risk assessment, and remediation planning", 350.99m, ProductType.Consulting, "$175-$400/hour"),
            ("Compliance & Regulatory Consulting", "HIPAA, SOC2, PCI-DSS, GDPR compliance consulting", 45000.99m, ProductType.Consulting, "$10000-$100000/project"),
            ("Business Continuity Planning", "Business continuity and disaster recovery planning", 22000.99m, ProductType.Consulting, "$8000-$50000/project"),
            ("Vendor Selection & Evaluation", "Technology vendor evaluation and selection assistance", 12000.99m, ProductType.Consulting, "$5000-$25000/project"),
        };
        foreach (var (name, desc, price, type, pricing) in consultingStrategy)
        {
            products.Add(new Product { Name = name, Description = $"{desc}. {pricing}", SKU = $"CON-{skuCounter++}", Category = "Consulting & Strategy", ProductType = type, Status = ProductStatus.Active, Price = price, IsService = true, IsSubscription = false, CreatedAt = DateTime.UtcNow });
        }

        // =============================================================================
        // IMPLEMENTATION & INTEGRATION SERVICES
        // =============================================================================
        skuCounter = 5001;

        var implementationIntegration = new[]
        {
            ("Software Implementation & Deployment", "Full software implementation including configuration and training", 45000.99m, ProductType.Implementation, "$5000-$150000/project"),
            ("System Integration Services", "Integration of disparate systems and applications", 225.99m, ProductType.Implementation, "$125-$300/hour"),
            ("Data Migration Services", "Data extraction, transformation, and loading services", 35000.99m, ProductType.Implementation, "$2-$10/GB or $10000-$100000"),
            ("Custom Application Development", "Bespoke software development services", 175.99m, ProductType.Implementation, "$100-$250/hour"),
            ("API Integration", "RESTful API development and integration", 12000.99m, ProductType.Implementation, "$3000-$25000/integration"),
            ("Legacy System Modernization", "Modernization of legacy applications and infrastructure", 150000.99m, ProductType.Implementation, "$25000-$500000/project"),
            ("Network Design & Implementation", "Complete network architecture design and deployment", 65000.99m, ProductType.Implementation, "$10000-$200000/project"),
            ("Infrastructure Deployment", "Server, storage, and infrastructure deployment services", 85000.99m, ProductType.Implementation, "$15000-$300000/project"),
        };
        foreach (var (name, desc, price, type, pricing) in implementationIntegration)
        {
            products.Add(new Product { Name = name, Description = $"{desc}. {pricing}", SKU = $"IMP-{skuCounter++}", Category = "Implementation & Integration", ProductType = type, Status = ProductStatus.Active, Price = price, IsService = true, IsSubscription = false, CreatedAt = DateTime.UtcNow });
        }

        // =============================================================================
        // MANAGED SERVICES
        // =============================================================================
        skuCounter = 6001;

        var managedServices = new[]
        {
            ("Managed IT Services (MSP) - Basic", "Core IT management for small business, per user/month", 95.99m, ProductType.ManagedService, "$75-$250/user/month"),
            ("Managed IT Services (MSP) - Pro", "Comprehensive IT management with proactive support, per user/month", 175.99m, ProductType.ManagedService, "$75-$250/user/month"),
            ("Managed IT Services (MSP) - Enterprise", "Full-service IT management with dedicated team, per user/month", 249.99m, ProductType.ManagedService, "$75-$250/user/month"),
            ("Managed Security Services (MSSP)", "24/7 security monitoring and incident response, per device/month", 125.99m, ProductType.ManagedService, "$50-$300/device/month"),
            ("Managed Network Services", "Network monitoring, management, and optimization", 2500.99m, ProductType.ManagedService, "$100-$5000/month"),
            ("Managed Cloud Services", "Cloud infrastructure management and optimization", 3500.99m, ProductType.ManagedService, "10-25% of spend or $500-$10000/month"),
            ("Database Administration", "Managed database services including tuning and backup, per DB/month", 1500.99m, ProductType.ManagedService, "$500-$5000/database/month"),
            ("Application Management", "Application support, maintenance, and updates, per app/month", 4500.99m, ProductType.ManagedService, "$1000-$10000/app/month"),
            ("Desktop as a Service (DaaS)", "Virtual desktop infrastructure, per desktop/month", 95.99m, ProductType.ManagedService, "$50-$150/desktop/month"),
            ("Monitoring & Alerting Services", "Infrastructure monitoring and alerting, per device/month", 18.99m, ProductType.ManagedService, "$5-$50/device/month"),
        };
        foreach (var (name, desc, price, type, pricing) in managedServices)
        {
            products.Add(new Product { Name = name, Description = $"{desc}. {pricing}", SKU = $"MSP-{skuCounter++}", Category = "Managed Services", ProductType = type, Status = ProductStatus.Active, Price = price, IsService = true, IsSubscription = true, CreatedAt = DateTime.UtcNow });
        }

        // =============================================================================
        // SUPPORT & MAINTENANCE SERVICES
        // =============================================================================
        skuCounter = 7001;

        var supportMaintenance = new[]
        {
            ("Help Desk Support (Tier 1/2/3)", "Multi-tier technical support, per user/month", 55.99m, ProductType.SupportContract, "$25-$100/user/month or $50-$200/ticket"),
            ("On-Site Support Services", "On-site technician support with minimum hours", 175.99m, ProductType.Service, "$125-$250/hour (2-4 hour minimum)"),
            ("Remote Support & Troubleshooting", "Remote technical support per incident or bundle", 99.99m, ProductType.Service, "$75-$150/incident or $500-$2000/month"),
            ("Hardware Maintenance & Repair", "Hardware repair services hourly plus parts", 150.99m, ProductType.Service, "$100-$200/hour + parts"),
            ("Software Maintenance & Updates", "Annual software maintenance and updates", 0m, ProductType.SupportContract, "15-25% of license cost annually"),
            ("SLA Management", "Service level agreement management and reporting", 0m, ProductType.SupportContract, "Included in service contracts"),
            ("After-Hours & Emergency Support", "24/7 emergency support at premium rates", 350.99m, ProductType.Service, "$200-$400/hour"),
            ("End-User Training & Documentation", "User training and documentation services", 150.99m, ProductType.Training, "$100-$200/hour or $50-$150/user"),
        };
        foreach (var (name, desc, price, type, pricing) in supportMaintenance)
        {
            products.Add(new Product { Name = name, Description = $"{desc}. {pricing}", SKU = $"SUP-{skuCounter++}", Category = "Support & Maintenance", ProductType = type, Status = ProductStatus.Active, Price = price, IsService = true, IsSubscription = type == ProductType.SupportContract, CreatedAt = DateTime.UtcNow });
        }

        // =============================================================================
        // SECURITY SERVICES
        // =============================================================================
        skuCounter = 8001;

        var securityServices = new[]
        {
            ("Security Assessment & Audit", "Comprehensive security posture assessment", 25000.99m, ProductType.Service, "$5000-$50000/assessment"),
            ("Penetration Testing & Ethical Hacking", "Internal and external penetration testing", 35000.99m, ProductType.Service, "$8000-$75000/test"),
            ("SOC Services", "Security Operations Center monitoring, per device/month", 85.99m, ProductType.ManagedService, "$50-$200/device/month"),
            ("Incident Response & Forensics", "Security incident response retainer", 12500.99m, ProductType.SupportContract, "$5000-$25000/month retainer"),
            ("Vulnerability Management", "Continuous vulnerability scanning and management, per device/month", 25.99m, ProductType.ManagedService, "$10-$50/device/month"),
            ("Compliance Auditing & Remediation", "Regulatory compliance audit and remediation", 55000.99m, ProductType.Service, "$10000-$100000/project"),
            ("Security Awareness Training", "Employee security awareness program, per user/year", 45.99m, ProductType.Training, "$25-$75/user/year"),
            ("Security Architecture Design", "Enterprise security architecture design", 75000.99m, ProductType.Consulting, "$15000-$150000/project"),
        };
        foreach (var (name, desc, price, type, pricing) in securityServices)
        {
            products.Add(new Product { Name = name, Description = $"{desc}. {pricing}", SKU = $"SEC-{skuCounter++}", Category = "Security Services", ProductType = type, Status = ProductStatus.Active, Price = price, IsService = true, IsSubscription = type == ProductType.ManagedService, CreatedAt = DateTime.UtcNow });
        }

        // =============================================================================
        // CLOUD SERVICES (PROFESSIONAL)
        // =============================================================================
        skuCounter = 9001;

        var cloudProfessional = new[]
        {
            ("Cloud Migration & Deployment", "Full cloud migration project execution", 85000.99m, ProductType.Implementation, "$10000-$250000/project"),
            ("Cloud Infrastructure Management", "Ongoing cloud infrastructure management", 8500.99m, ProductType.ManagedService, "$2000-$20000/month"),
            ("Cloud Optimization & Cost Management", "FinOps and cloud cost optimization", 6500.99m, ProductType.ManagedService, "20-40% of savings or $3000-$15000/month"),
            ("Cloud Security & Compliance", "Cloud security posture management", 12500.99m, ProductType.ManagedService, "$2500-$25000/month"),
            ("Hybrid Cloud Implementation", "Hybrid cloud architecture implementation", 125000.99m, ProductType.Implementation, "$25000-$300000/project"),
            ("Multi-Cloud Strategy & Management", "Multi-cloud orchestration and management", 15000.99m, ProductType.ManagedService, "$5000-$30000/month"),
            ("Cloud Disaster Recovery", "Cloud-based DR implementation and management", 10000.99m, ProductType.ManagedService, "$10000 setup + $1000-$10000/month"),
            ("Cloud Application Development", "Cloud-native application development", 195.99m, ProductType.Implementation, "$125-$275/hour"),
        };
        foreach (var (name, desc, price, type, pricing) in cloudProfessional)
        {
            products.Add(new Product { Name = name, Description = $"{desc}. {pricing}", SKU = $"CLP-{skuCounter++}", Category = "Cloud Services (Professional)", ProductType = type, Status = ProductStatus.Active, Price = price, IsService = true, IsSubscription = type == ProductType.ManagedService, CreatedAt = DateTime.UtcNow });
        }

        // =============================================================================
        // DATA & ANALYTICS
        // =============================================================================
        skuCounter = 10001;

        var dataAnalytics = new[]
        {
            ("Data Center Design & Build", "Complete data center design and construction", 750000.99m, ProductType.Implementation, "$50000-$5000000/project"),
            ("Data Analytics & Business Intelligence", "BI platform implementation and reporting", 85000.99m, ProductType.Implementation, "$15000-$200000/project"),
            ("Data Warehousing Solutions", "Enterprise data warehouse implementation", 175000.99m, ProductType.Implementation, "$25000-$500000/project"),
            ("Big Data Platform Implementation", "Hadoop, Spark, and big data ecosystem setup", 250000.99m, ProductType.Implementation, "$50000-$750000/project"),
            ("AI & Machine Learning Implementation", "ML model development and deployment", 185000.99m, ProductType.Implementation, "$30000-$500000/project"),
            ("Data Governance Consulting", "Data governance framework and implementation", 65000.99m, ProductType.Consulting, "$15000-$150000/project"),
            ("Master Data Management", "MDM strategy and implementation", 125000.99m, ProductType.Implementation, "$25000-$300000/project"),
            ("Data Quality & Cleansing Services", "Data cleansing and quality improvement", 45000.99m, ProductType.Service, "$0.10-$5/record or $10000-$100000"),
        };
        foreach (var (name, desc, price, type, pricing) in dataAnalytics)
        {
            products.Add(new Product { Name = name, Description = $"{desc}. {pricing}", SKU = $"DAT-{skuCounter++}", Category = "Data & Analytics", ProductType = type, Status = ProductStatus.Active, Price = price, IsService = true, IsSubscription = false, CreatedAt = DateTime.UtcNow });
        }

        // =============================================================================
        // SPECIALIZED SERVICES
        // =============================================================================
        skuCounter = 11001;

        var specializedServices = new[]
        {
            ("VoIP & Unified Communications Setup", "VoIP system implementation, per user setup + monthly", 350.99m, ProductType.Implementation, "$200-$500 setup + $30-$75/user/month"),
            ("Video Conferencing Setup & Support", "Conference room AV installation and support", 15000.99m, ProductType.Implementation, "$5000-$50000/room + $100-$500/month"),
            ("Wireless Site Survey & Design", "Enterprise WiFi site survey and design", 8500.99m, ProductType.Service, "$0.25-$1/sq ft or $2500-$25000"),
            ("Cabling & Infrastructure Services", "Structured cabling installation, per drop", 275.99m, ProductType.Service, "$150-$400/drop or $75-$150/hour"),
            ("Asset Tracking & Management", "IT asset lifecycle management, per asset/month", 5.99m, ProductType.ManagedService, "$2-$10/asset/month"),
            ("IT Procurement Services", "Hardware and software procurement services", 0m, ProductType.Service, "5-15% of purchase or $500-$5000/project"),
            ("Technology Lifecycle Management", "End-to-end technology lifecycle services, per device/month", 12.99m, ProductType.ManagedService, "$5-$25/device/month"),
            ("Green IT & Sustainability Consulting", "Sustainable IT strategy and implementation", 45000.99m, ProductType.Consulting, "$10000-$100000/project"),
        };
        foreach (var (name, desc, price, type, pricing) in specializedServices)
        {
            products.Add(new Product { Name = name, Description = $"{desc}. {pricing}", SKU = $"SPC-{skuCounter++}", Category = "Specialized Services", ProductType = type, Status = ProductStatus.Active, Price = price, IsService = true, IsSubscription = type == ProductType.ManagedService, CreatedAt = DateTime.UtcNow });
        }

        context.Products.AddRange(products);
        await context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} sample products across all categories", products.Count);
    }

    /// <summary>
    /// Seed service request categories to production database
    /// </summary>
    public async Task SeedServiceRequestCategoriesAsync()
    {
        var dbContext = _context as CrmDbContext ?? throw new InvalidOperationException("Context must be CrmDbContext");
        await SeedServiceRequestCategoriesToContextAsync(dbContext);
    }

    /// <summary>
    /// Seed comprehensive service request categories, subcategories, and types
    /// Based on MSP/IT service desk best practices
    /// </summary>
    private async Task SeedServiceRequestCategoriesToContextAsync(CrmDbContext context)
    {
        _logger.LogInformation("Seeding comprehensive service request categories and types...");
        
        // Check if already seeded
        var existingTypes = await context.ServiceRequestTypes.CountAsync();
        if (existingTypes >= 50)
        {
            _logger.LogInformation("Service request types already exist. Skipping...");
            return;
        }
        
        // Clear existing data for fresh seed
        context.ServiceRequestTypes.RemoveRange(context.ServiceRequestTypes);
        context.ServiceRequestSubcategories.RemoveRange(context.ServiceRequestSubcategories);
        context.ServiceRequestCategories.RemoveRange(context.ServiceRequestCategories);
        await context.SaveChangesAsync();
        
        // Dictionary to track categories and subcategories for relationships
        var categories = new Dictionary<string, ServiceRequestCategory>();
        var subcategories = new Dictionary<string, ServiceRequestSubcategory>();
        
        // Define all service request type data
        var serviceRequestData = GetServiceRequestTypeData();
        
        foreach (var data in serviceRequestData)
        {
            // Get or create category
            if (!categories.TryGetValue(data.Category, out var category))
            {
                category = new ServiceRequestCategory
                {
                    Name = data.Category,
                    Description = GetCategoryDescription(data.Category),
                    IsActive = true,
                    DisplayOrder = categories.Count + 1,
                    CreatedAt = DateTime.UtcNow
                };
                context.ServiceRequestCategories.Add(category);
                await context.SaveChangesAsync();
                categories[data.Category] = category;
            }
            
            // Get or create subcategory
            var subcategoryKey = $"{data.Category}|{data.Subcategory}";
            if (!subcategories.TryGetValue(subcategoryKey, out var subcategory))
            {
                subcategory = new ServiceRequestSubcategory
                {
                    CategoryId = category.Id,
                    Name = data.Subcategory,
                    Description = $"{data.Subcategory} services under {data.Category}",
                    IsActive = true,
                    DisplayOrder = subcategories.Count(s => s.Key.StartsWith(data.Category)) + 1,
                    CreatedAt = DateTime.UtcNow
                };
                context.ServiceRequestSubcategories.Add(subcategory);
                await context.SaveChangesAsync();
                subcategories[subcategoryKey] = subcategory;
            }
            
            // Create service request type
            var requestType = new ServiceRequestType
            {
                CategoryId = category.Id,
                SubcategoryId = subcategory.Id,
                Name = data.Name,
                RequestType = data.Type,
                DetailedDescription = data.Description,
                WorkflowName = data.WorkflowName,
                PossibleResolutions = data.PossibleResolutions,
                FinalCustomerResolutions = data.FinalCustomerResolutions,
                IsActive = true,
                DisplayOrder = context.ServiceRequestTypes.Count(t => t.SubcategoryId == subcategory.Id) + 1,
                DefaultPriority = data.Type == "Complaint" ? ServiceRequestPriority.High : ServiceRequestPriority.Medium,
                CreatedAt = DateTime.UtcNow
            };
            context.ServiceRequestTypes.Add(requestType);
        }
        
        await context.SaveChangesAsync();
        _logger.LogInformation($"Seeded {categories.Count} categories, {subcategories.Count} subcategories, and {serviceRequestData.Count} service request types");
    }
    
    private static string GetCategoryDescription(string category) => category switch
    {
        "Hardware" => "Hardware repair, installation, upgrade, and maintenance services",
        "Software" => "Software installation, configuration, troubleshooting, and licensing",
        "Cloud Services" => "Cloud deployment, migration, IaaS, PaaS, SaaS, and hybrid cloud services",
        "Managed Services" => "Ongoing managed IT, security, and cloud services",
        "Support & Maintenance" => "Help desk, on-site, and remote support services",
        "Security Services" => "Security assessments, penetration testing, and incident response",
        "Specialized Services" => "VoIP, unified communications, and cabling services",
        "Data & Analytics" => "Business intelligence, data warehousing, and analytics services",
        _ => $"{category} services"
    };
    
    private static List<ServiceRequestTypeData> GetServiceRequestTypeData()
    {
        return new List<ServiceRequestTypeData>
        {
            // Hardware - End-User Computing
            new("Hardware", "End-User Computing", "Device Not Powering On", "Complaint",
                "Customer reports desktop/laptop will not power on or boot up",
                "Hardware Troubleshooting Workflow",
                "Check power cable and outlet; Test with different power adapter; Replace power supply; Replace motherboard; Device replacement",
                "Device repaired on-site; Replacement device shipped; Loaner device provided"),
            new("Hardware", "End-User Computing", "Performance Degradation", "Complaint",
                "Device running slower than expected or experiencing lag",
                "Performance Optimization Workflow",
                "Run diagnostics; Clean temporary files; Upgrade RAM; Replace HDD with SSD; Reinstall OS; Device replacement",
                "Device optimized and performance restored; Hardware upgraded; Replacement issued"),
            new("Hardware", "End-User Computing", "Screen/Display Issues", "Complaint",
                "Broken screen cracked display flickering or no display output",
                "Display Repair Workflow",
                "Replace screen; Update graphics drivers; Replace graphics card; Test external monitor; Full device replacement",
                "Screen replaced; Device replaced; Temporary workaround with external monitor"),
            new("Hardware", "End-User Computing", "Peripheral Connection Issues", "Request",
                "Keyboard mouse or other peripheral not connecting or working properly",
                "Peripheral Support Workflow",
                "Update drivers; Test on different USB port; Replace peripheral; Clean connection ports; BIOS/firmware update",
                "Peripheral replaced; Driver updated; Issue resolved through configuration"),
            new("Hardware", "End-User Computing", "Hardware Upgrade Request", "Request",
                "Customer requests RAM storage or component upgrades",
                "Hardware Upgrade Workflow",
                "Verify compatibility; Order components; Schedule installation; Perform upgrade; Test and validate",
                "Upgrade completed and tested; Alternative solution provided if incompatible"),
            
            // Hardware - Networking Equipment
            new("Hardware", "Networking Equipment", "Network Connectivity Loss", "Complaint",
                "Network devices offline or intermittent connectivity issues",
                "Network Outage Resolution Workflow",
                "Reboot device; Check cable connections; Update firmware; Replace faulty hardware; Reconfigure network settings",
                "Connectivity restored; Faulty device replaced; Configuration corrected"),
            new("Hardware", "Networking Equipment", "Slow Network Performance", "Complaint",
                "Network speeds below expected throughput or high latency",
                "Network Performance Workflow",
                "Run speed tests; Check bandwidth utilization; Update firmware; Replace cables; Upgrade hardware; Optimize QoS settings",
                "Performance optimized; Hardware upgraded; Configuration tuned"),
            new("Hardware", "Networking Equipment", "Configuration Change Request", "Request",
                "Customer needs firewall rules VLANs or routing changes",
                "Network Configuration Workflow",
                "Review request; Create change plan; Implement during maintenance window; Test and validate; Document changes",
                "Configuration successfully updated; Rollback if issues detected"),
            new("Hardware", "Networking Equipment", "Firmware Update Required", "Request",
                "Device firmware is outdated and needs updating",
                "Firmware Update Workflow",
                "Backup current config; Download latest firmware; Schedule maintenance window; Apply update; Verify functionality",
                "Firmware updated successfully; Rollback to previous version if needed"),
            new("Hardware", "Networking Equipment", "Port/Interface Failure", "Complaint",
                "Network port or interface not functioning",
                "Hardware Port Repair Workflow",
                "Test with different cable; Update drivers; Replace network module; Replace entire device; Reconfigure redundant path",
                "Port repaired; Device replaced; Traffic rerouted to backup interface"),
            
            // Hardware - Server Infrastructure
            new("Hardware", "Server Infrastructure", "Server Crash/Downtime", "Complaint",
                "Server unexpectedly down or crashed requiring immediate attention",
                "Critical Server Recovery Workflow",
                "Check hardware status; Review system logs; Reboot server; Replace failed components; Restore from backup; Failover to secondary",
                "Server restored to operation; Root cause identified and remediated"),
            new("Hardware", "Server Infrastructure", "High CPU/Memory Usage", "Complaint",
                "Server experiencing resource exhaustion impacting performance",
                "Resource Optimization Workflow",
                "Identify resource-heavy processes; Optimize applications; Add RAM/CPU; Migrate workloads; Implement load balancing",
                "Resources optimized; Hardware upgraded; Workloads redistributed"),
            new("Hardware", "Server Infrastructure", "Storage Capacity Warning", "Request",
                "Server storage reaching capacity threshold",
                "Storage Expansion Workflow",
                "Clean old files; Archive data; Add storage drives; Implement tiered storage; Migrate to larger storage solution",
                "Storage expanded; Data archived; Capacity monitoring implemented"),
            new("Hardware", "Server Infrastructure", "RAID Array Degraded", "Complaint",
                "RAID array showing degraded status or disk failure",
                "RAID Recovery Workflow",
                "Replace failed disk; Rebuild array; Verify data integrity; Update firmware; Replace RAID controller if needed",
                "Array rebuilt; Data verified intact; Preventive maintenance scheduled"),
            new("Hardware", "Server Infrastructure", "Virtualization Issues", "Complaint",
                "VM performance problems migration failures or hypervisor errors",
                "Virtualization Support Workflow",
                "Check host resources; Optimize VM settings; Update hypervisor; Redistribute VMs; Add host capacity",
                "VMs optimized; Resources balanced; Hypervisor updated"),
            
            // Hardware - Data Center Equipment
            new("Hardware", "Data Center Equipment", "Cooling System Alert", "Complaint",
                "Temperature warnings or HVAC system failures in data center",
                "Environmental Alert Workflow",
                "Check HVAC status; Increase cooling capacity; Redistribute heat load; Emergency cooling deployment; Schedule maintenance",
                "Temperature normalized; HVAC repaired; Monitoring enhanced"),
            new("Hardware", "Data Center Equipment", "Power Distribution Issue", "Complaint",
                "PDU failure power redundancy loss or circuit overload",
                "Power System Recovery Workflow",
                "Switch to backup power; Balance power load; Replace PDU; Add power capacity; Update power monitoring",
                "Power restored; Redundancy verified; Load balanced"),
            new("Hardware", "Data Center Equipment", "UPS Battery Replacement", "Request",
                "UPS batteries reaching end of life or failed battery test",
                "UPS Maintenance Workflow",
                "Test battery capacity; Schedule replacement; Install new batteries; Test UPS functionality; Update maintenance logs",
                "Batteries replaced; UPS tested and operational; Next service scheduled"),
            new("Hardware", "Data Center Equipment", "Rack Space Request", "Request",
                "Customer needs additional rack space or reorganization",
                "Rack Allocation Workflow",
                "Survey available space; Plan layout; Schedule installation; Mount equipment; Cable management; Documentation",
                "Equipment installed; Cabling completed; Documentation updated"),
            
            // Hardware - Mobile & Telecommunications
            new("Hardware", "Mobile & Telecommunications", "Device Malfunction", "Complaint",
                "Mobile device not functioning properly hardware defects",
                "Mobile Device Support Workflow",
                "Remote diagnostics; Factory reset; Replace device; Restore from backup; Provide loaner device",
                "Device replaced under warranty; Issue resolved remotely"),
            new("Hardware", "Mobile & Telecommunications", "SIM/Connectivity Issues", "Complaint",
                "Cannot connect to cellular network or SIM not recognized",
                "Mobile Connectivity Workflow",
                "Check SIM card; Verify account status; Update carrier settings; Replace SIM; Reconfigure APN settings",
                "SIM replaced; Network settings corrected; Connectivity restored"),
            new("Hardware", "Mobile & Telecommunications", "MDM Enrollment Problem", "Request",
                "Issues enrolling device in mobile device management system",
                "MDM Support Workflow",
                "Verify credentials; Check MDM server; Factory reset device; Re-enroll device; Update MDM profile",
                "Device successfully enrolled; Policies applied; User trained"),
            
            // Hardware - Printing & Imaging
            new("Hardware", "Printing & Imaging", "Printer Offline/Not Printing", "Complaint",
                "Printer shows offline or jobs stuck in queue",
                "Printer Troubleshooting Workflow",
                "Restart printer; Check connections; Update drivers; Clear print queue; Reinstall printer; Network configuration",
                "Printer back online; Jobs printing successfully"),
            new("Hardware", "Printing & Imaging", "Paper Jam/Feed Issues", "Complaint",
                "Repeated paper jams or paper feed mechanism failure",
                "Printer Maintenance Workflow",
                "Clear paper path; Clean rollers; Replace pickup assembly; Adjust paper tray; Replace feed mechanism",
                "Mechanism cleaned/replaced; Jam issue resolved"),
            new("Hardware", "Printing & Imaging", "Print Quality Issues", "Complaint",
                "Poor print quality streaks fading or color problems",
                "Print Quality Workflow",
                "Clean print heads; Replace toner/ink; Calibrate printer; Replace drum; Replace imaging unit",
                "Print quality restored; Consumables replaced; Calibration completed"),
            new("Hardware", "Printing & Imaging", "Toner/Ink Replacement", "Request",
                "Customer needs consumables replacement or supplies order",
                "Consumables Order Workflow",
                "Verify model; Order supplies; Ship to customer; Provide installation instructions; Recycle old cartridges",
                "Supplies delivered; Installation confirmed; Recycling arranged"),
            new("Hardware", "Printing & Imaging", "Scanner Not Working", "Complaint",
                "Scanner function not operating or producing errors",
                "Scanner Support Workflow",
                "Update scanner drivers; Test scanner software; Clean scanner glass; Replace scanner module; Reinstall software",
                "Scanner operational; Software updated; Issue resolved"),
            
            // Software - Operating Systems
            new("Software", "Operating Systems", "System Won't Boot", "Complaint",
                "Operating system fails to start or stuck in boot loop",
                "OS Recovery Workflow",
                "Boot into safe mode; Run startup repair; Restore system; Reinstall OS; Recover data; Restore from backup",
                "OS repaired; System restored; Data recovered"),
            new("Software", "Operating Systems", "Blue Screen/Crash Errors", "Complaint",
                "Frequent system crashes BSOD or kernel panics",
                "System Stability Workflow",
                "Analyze crash dumps; Update drivers; Check hardware; Remove problematic software; Reinstall OS",
                "Stability restored; Faulty driver/software removed; System optimized"),
            new("Software", "Operating Systems", "License Activation Issues", "Complaint",
                "OS not activated or license validation errors",
                "License Resolution Workflow",
                "Verify license key; Reactivate license; Contact vendor; Issue new license; Update activation server",
                "License activated successfully; Documentation updated"),
            new("Software", "Operating Systems", "OS Update Request", "Request",
                "Customer needs OS upgraded to newer version",
                "OS Upgrade Workflow",
                "Backup system; Check compatibility; Download OS; Perform upgrade; Migrate settings; Test applications",
                "OS successfully upgraded; Applications tested; User trained"),
            new("Software", "Operating Systems", "Patch/Update Failures", "Complaint",
                "Windows updates failing or causing system issues",
                "Update Troubleshooting Workflow",
                "Clear update cache; Run troubleshooter; Manual download; Repair Windows components; Rollback update",
                "Updates installed successfully; System stable; Update schedule optimized"),
            
            // Software - Productivity & Collaboration
            new("Software", "Productivity & Collaboration", "Email Access Issues", "Complaint",
                "Cannot access email account sync errors or login problems",
                "Email Support Workflow",
                "Verify credentials; Check email server; Reconfigure email client; Reset password; Clear cache; Update software",
                "Email access restored; Sync working; Credentials updated"),
            new("Software", "Productivity & Collaboration", "Office Application Crashes", "Complaint",
                "Word Excel PowerPoint crashing frequently or not opening",
                "Office Repair Workflow",
                "Repair Office installation; Disable add-ins; Clear temp files; Reinstall application; Check file corruption",
                "Application stable; Add-ins managed; Files recovered"),
            new("Software", "Productivity & Collaboration", "License Addition Request", "Request",
                "Need to add more user licenses or upgrade subscription",
                "License Management Workflow",
                "Review current licenses; Process order; Assign licenses; Configure new users; Update billing",
                "Licenses added; Users activated; Billing confirmed"),
            new("Software", "Productivity & Collaboration", "OneDrive/SharePoint Sync Issues", "Complaint",
                "Files not syncing properly or sync errors occurring",
                "Cloud Sync Resolution Workflow",
                "Reset sync; Check permissions; Clear cache; Reinstall sync client; Verify network; Check storage quota",
                "Sync restored; Permissions corrected; Files up to date"),
            new("Software", "Productivity & Collaboration", "Teams/Meeting Issues", "Complaint",
                "Video conferencing problems audio issues or meeting failures",
                "Collaboration Tools Support Workflow",
                "Check audio/video settings; Update application; Test network; Verify permissions; Reinstall client; Optimize bandwidth",
                "Meeting tools working; Audio/video tested; Network optimized"),
            
            // Software - Security Software
            new("Software", "Security Software", "Antivirus Not Updating", "Complaint",
                "Security definitions not updating or update failures",
                "Security Update Workflow",
                "Check internet connection; Verify subscription; Manual update; Reinstall software; Check firewall settings",
                "Definitions updated; Subscription verified; Protection active"),
            new("Software", "Security Software", "False Positive Detection", "Complaint",
                "Legitimate files flagged as threats or quarantined incorrectly",
                "False Positive Workflow",
                "Analyze file; Whitelist application; Submit to vendor; Adjust scan settings; Restore quarantined files",
                "File restored; Whitelist updated; Settings adjusted"),
            new("Software", "Security Software", "Performance Impact", "Complaint",
                "Security software causing system slowdown or high resource usage",
                "Performance Tuning Workflow",
                "Adjust scan schedule; Configure exclusions; Update software; Reduce scan intensity; Consider alternative solution",
                "Performance improved; Scans optimized; Settings configured"),
            new("Software", "Security Software", "Threat Detection Alert", "Complaint",
                "Malware virus or security threat detected on system",
                "Threat Remediation Workflow",
                "Isolate system; Run full scan; Remove threats; Check for data breach; Restore clean backup; Strengthen security",
                "Threat removed; System cleaned; Prevention measures implemented"),
            new("Software", "Security Software", "License Renewal Required", "Request",
                "Security subscription expiring or expired",
                "License Renewal Workflow",
                "Generate renewal quote; Process payment; Apply new license; Verify activation; Update records",
                "License renewed; Protection active; Next renewal scheduled"),
            
            // Software - Business Applications
            new("Software", "Business Applications", "CRM/ERP Performance Issues", "Complaint",
                "Business application running slow or timing out",
                "Application Performance Workflow",
                "Check database; Optimize queries; Add resources; Clear cache; Update application; Index databases",
                "Performance improved; Database optimized; Resources added"),
            new("Software", "Business Applications", "Data Import/Export Errors", "Complaint",
                "Problems importing or exporting data format errors",
                "Data Integration Support Workflow",
                "Validate file format; Check field mapping; Fix data errors; Adjust import settings; Manual correction",
                "Data successfully imported; Errors corrected; Process documented"),
            new("Software", "Business Applications", "User Access Request", "Request",
                "New user needs access or permission changes required",
                "User Access Workflow",
                "Verify authorization; Create account; Assign roles; Configure permissions; Provide training; Document access",
                "User access granted; Permissions configured; Training completed"),
            new("Software", "Business Applications", "Custom Report Request", "Request",
                "Customer needs new report or dashboard created",
                "Report Development Workflow",
                "Gather requirements; Design report; Develop solution; Test output; Deploy to production; Train users",
                "Report delivered; Users trained; Documentation provided"),
            new("Software", "Business Applications", "Integration Failure", "Complaint",
                "Third-party integration not working or API errors",
                "Integration Support Workflow",
                "Check API credentials; Verify endpoints; Review error logs; Update integration; Reconnect services; Test functionality",
                "Integration restored; Data flowing; Monitoring enabled"),
            
            // Software - Backup & Recovery
            new("Software", "Backup & Recovery", "Backup Job Failure", "Complaint",
                "Scheduled backup failed or incomplete",
                "Backup Troubleshooting Workflow",
                "Review error logs; Check storage space; Verify credentials; Adjust schedule; Manual backup; Fix configuration",
                "Backup successful; Schedule optimized; Alerts configured"),
            new("Software", "Backup & Recovery", "Restore Request", "Request",
                "Customer needs files or system restored from backup",
                "Data Restoration Workflow",
                "Verify backup availability; Confirm restore point; Perform restoration; Validate data integrity; Test restored system",
                "Data successfully restored; Integrity verified; User confirmed"),
            new("Software", "Backup & Recovery", "Slow Backup Performance", "Complaint",
                "Backups taking too long or impacting system performance",
                "Backup Optimization Workflow",
                "Optimize backup window; Implement incremental backups; Add bandwidth; Deduplicate data; Upgrade backup solution",
                "Backup speed improved; Impact minimized; Deduplication enabled"),
            new("Software", "Backup & Recovery", "Storage Quota Exceeded", "Complaint",
                "Backup storage full or approaching capacity limit",
                "Storage Management Workflow",
                "Review retention policy; Delete old backups; Add storage; Implement tiering; Compress backups",
                "Storage expanded; Old backups archived; Policy updated"),
            
            // Cloud Services - IaaS
            new("Cloud Services", "IaaS", "Virtual Machine Not Starting", "Complaint",
                "VM fails to start or stuck in provisioning state",
                "VM Recovery Workflow",
                "Check resource availability; Review error messages; Restart VM service; Rebuild VM; Restore from snapshot",
                "VM operational; Issue identified; Monitoring added"),
            new("Cloud Services", "IaaS", "High Cloud Costs", "Complaint",
                "Unexpected high billing or cost overruns",
                "Cost Optimization Workflow",
                "Analyze usage; Right-size resources; Implement auto-scaling; Remove unused resources; Reserved instances; Budget alerts",
                "Costs reduced; Right-sizing completed; Alerts configured"),
            new("Cloud Services", "IaaS", "Resource Scaling Request", "Request",
                "Need to increase/decrease VM resources or storage",
                "Resource Adjustment Workflow",
                "Review current usage; Plan scaling; Schedule change; Apply new configuration; Test performance; Update documentation",
                "Resources scaled; Performance validated; Documentation updated"),
            new("Cloud Services", "IaaS", "Snapshot/Backup Request", "Request",
                "Customer needs VM snapshot or backup created",
                "Cloud Backup Workflow",
                "Create snapshot; Verify snapshot; Test restoration; Schedule automated backups; Document retention policy",
                "Snapshot created; Backup schedule configured; Policy documented"),
            
            // Cloud Services - PaaS
            new("Cloud Services", "PaaS", "Database Connection Errors", "Complaint",
                "Application cannot connect to cloud database",
                "Database Connectivity Workflow",
                "Check connection string; Verify firewall rules; Test credentials; Review network security; Restart database service",
                "Connection restored; Firewall configured; Monitoring enabled"),
            new("Cloud Services", "PaaS", "Database Performance Issues", "Complaint",
                "Slow query performance or database timeouts",
                "Database Optimization Workflow",
                "Analyze slow queries; Add indexes; Optimize queries; Increase DTUs/resources; Implement caching; Partition data",
                "Performance improved; Queries optimized; Resources scaled"),
            new("Cloud Services", "PaaS", "Database Migration Request", "Request",
                "Need to migrate database to different tier or region",
                "Database Migration Workflow",
                "Plan migration; Backup data; Create target database; Migrate data; Test applications; Update connections; Cutover",
                "Migration completed; Applications tested; Zero downtime achieved"),
            
            // Cloud Services - SaaS
            new("Cloud Services", "SaaS", "User Provisioning Issues", "Complaint",
                "New users not created or SSO not working",
                "User Management Workflow",
                "Verify identity provider; Check synchronization; Manually provision; Fix SSO configuration; Test authentication",
                "Users provisioned; SSO working; Sync verified"),
            new("Cloud Services", "SaaS", "Feature Not Available", "Complaint",
                "Expected feature missing or not working in SaaS application",
                "Feature Support Workflow",
                "Verify subscription tier; Check feature rollout; Enable feature flag; Upgrade subscription; Provide workaround",
                "Feature enabled; Subscription upgraded; User trained on feature"),
            new("Cloud Services", "SaaS", "Data Export Request", "Request",
                "Customer needs to export data from SaaS platform",
                "Data Export Workflow",
                "Verify export permissions; Generate export; Format conversion; Deliver securely; Verify data completeness",
                "Data exported; Format verified; Securely delivered"),
            
            // Cloud Services - Hybrid & Multi-Cloud
            new("Cloud Services", "Hybrid & Multi-Cloud", "VPN Connection Failure", "Complaint",
                "Site-to-site VPN down or intermittent connectivity",
                "VPN Troubleshooting Workflow",
                "Check VPN status; Verify credentials; Review firewall rules; Restart VPN service; Reconfigure tunnel; Update configuration",
                "VPN restored; Redundancy verified; Monitoring enhanced"),
            new("Cloud Services", "Hybrid & Multi-Cloud", "Cloud Sync Issues", "Complaint",
                "Data not syncing between on-premises and cloud",
                "Hybrid Sync Workflow",
                "Check sync service; Verify permissions; Review network; Clear sync cache; Manual sync; Update sync client",
                "Sync operational; Data consistent; Monitoring configured"),
            
            // Managed Services - Managed IT Services
            new("Managed Services", "Managed IT Services", "Routine Maintenance Request", "Request",
                "Customer requests scheduled maintenance or health check",
                "Scheduled Maintenance Workflow",
                "Schedule maintenance window; Perform updates; Run diagnostics; Generate report; Review with customer",
                "Maintenance completed; Systems optimized; Report delivered"),
            new("Managed Services", "Managed IT Services", "Add New Device to Management", "Request",
                "New device needs to be added to managed services",
                "Device Onboarding Workflow",
                "Register device; Install agents; Configure monitoring; Apply policies; Test connectivity; Update inventory",
                "Device onboarded; Monitoring active; Documentation updated"),
            new("Managed Services", "Managed IT Services", "Alert Fatigue Complaint", "Complaint",
                "Too many alerts or false alarms from monitoring",
                "Alert Tuning Workflow",
                "Review alert thresholds; Adjust sensitivity; Group alerts; Implement smart alerting; Reduce noise; Customize escalation",
                "Alerts optimized; False positives reduced; Escalation tuned"),
            
            // Managed Services - Managed Security Services
            new("Managed Services", "Managed Security Services", "Security Incident Alert", "Complaint",
                "Security event detected requiring investigation",
                "Security Incident Response Workflow",
                "Assess threat; Contain incident; Investigate root cause; Remediate vulnerability; Restore systems; Post-incident review",
                "Incident contained; Systems secured; Report provided"),
            new("Managed Services", "Managed Security Services", "Firewall Rule Change", "Request",
                "Customer needs firewall rules modified",
                "Firewall Change Workflow",
                "Review request; Assess security impact; Create change plan; Implement rule; Test connectivity; Document change",
                "Rule implemented; Access verified; Change documented"),
            new("Managed Services", "Managed Security Services", "Security Report Request", "Request",
                "Customer requests security posture or compliance report",
                "Security Reporting Workflow",
                "Gather security data; Analyze vulnerabilities; Generate report; Review findings; Provide recommendations; Present to customer",
                "Report delivered; Findings reviewed; Remediation plan created"),
            
            // Managed Services - Managed Cloud Services
            new("Managed Services", "Managed Cloud Services", "Cloud Cost Alert", "Complaint",
                "Unexpected cloud spending increase or budget exceeded",
                "Cloud Cost Management Workflow",
                "Analyze spending; Identify cost drivers; Right-size resources; Implement cost controls; Set up alerts; Optimize licensing",
                "Costs controlled; Alerts configured; Optimization ongoing"),
            new("Managed Services", "Managed Cloud Services", "Cloud Architecture Review", "Request",
                "Customer requests architecture assessment or optimization",
                "Architecture Review Workflow",
                "Assess current state; Identify improvements; Recommend changes; Create roadmap; Implement changes; Validate results",
                "Review completed; Recommendations delivered; Roadmap created"),
            
            // Support & Maintenance - Help Desk Support
            new("Support & Maintenance", "Help Desk Support", "Password Reset Request", "Request",
                "User locked out or forgot password",
                "Password Reset Workflow",
                "Verify identity; Reset password; Send temporary credentials; Force password change on login; Update documentation",
                "Password reset; Access restored; User notified"),
            new("Support & Maintenance", "Help Desk Support", "Software Installation Request", "Request",
                "User needs new software or application installed",
                "Software Installation Workflow",
                "Verify license; Check compatibility; Download software; Install application; Configure settings; Train user; Update inventory",
                "Software installed; User trained; License tracked"),
            new("Support & Maintenance", "Help Desk Support", "Slow Computer Complaint", "Complaint",
                "User reports computer running slowly",
                "Performance Support Workflow",
                "Run diagnostics; Check for malware; Clear temp files; Disable startup programs; Add RAM if needed; Optimize system",
                "Performance improved; Malware removed; User satisfied"),
            new("Support & Maintenance", "Help Desk Support", "Email Signature Update", "Request",
                "User needs email signature created or modified",
                "Email Configuration Workflow",
                "Gather requirements; Create signature template; Apply to email client; Test rendering; Provide instructions; Update centrally if applicable",
                "Signature created; Applied successfully; Documentation provided"),
            new("Support & Maintenance", "Help Desk Support", "General How-To Question", "Request",
                "User needs help with using software or feature",
                "User Training Support Workflow",
                "Understand requirement; Provide step-by-step guidance; Share documentation; Offer training resources; Follow up",
                "User trained; Issue resolved; Documentation shared"),
            
            // Support & Maintenance - On-Site Support
            new("Support & Maintenance", "On-Site Support", "Equipment Setup", "Request",
                "New equipment needs on-site installation and configuration",
                "On-Site Installation Workflow",
                "Schedule visit; Prepare equipment; Travel to site; Install hardware; Configure system; Test functionality; Train user",
                "Equipment installed; Testing complete; User trained on-site"),
            new("Support & Maintenance", "On-Site Support", "Network Troubleshooting", "Complaint",
                "On-site network issues requiring physical inspection",
                "On-Site Network Workflow",
                "Travel to location; Diagnose issue; Test cables; Check switches; Replace hardware; Verify connectivity; Document resolution",
                "Issue resolved on-site; Hardware replaced; Network operational"),
            
            // Support & Maintenance - Remote Support
            new("Support & Maintenance", "Remote Support", "Remote Desktop Issues", "Complaint",
                "Cannot connect remotely or remote tools not working",
                "Remote Access Workflow",
                "Check VPN status; Verify credentials; Test remote software; Configure firewall; Reinstall remote agent; Alternative access method",
                "Remote access restored; Alternative provided; User connected"),
            
            // Security Services - Security Assessments
            new("Security Services", "Security Assessments", "Vulnerability Scan Request", "Request",
                "Customer requests security vulnerability assessment",
                "Vulnerability Assessment Workflow",
                "Schedule scan; Execute scan; Analyze results; Prioritize vulnerabilities; Generate report; Provide remediation guidance",
                "Scan completed; Report delivered; Remediation plan provided"),
            new("Security Services", "Security Assessments", "Compliance Audit Request", "Request",
                "Customer needs compliance audit (HIPAA PCI SOC2)",
                "Compliance Audit Workflow",
                "Review requirements; Gather evidence; Assess controls; Identify gaps; Generate compliance report; Remediation recommendations",
                "Audit completed; Gaps identified; Remediation roadmap delivered"),
            
            // Security Services - Penetration Testing
            new("Security Services", "Penetration Testing", "Pentest Scheduling Request", "Request",
                "Customer wants to schedule penetration testing",
                "Pentest Scheduling Workflow",
                "Define scope; Schedule test; Execute testing; Document findings; Provide executive summary; Retest after remediation",
                "Pentest completed; Vulnerabilities documented; Retest scheduled"),
            
            // Security Services - Incident Response
            new("Security Services", "Incident Response", "Security Breach Suspected", "Complaint",
                "Potential security breach or compromise detected",
                "Breach Response Workflow",
                "Activate incident response; Contain breach; Forensic analysis; Identify scope; Remediate; Notify stakeholders; Post-mortem",
                "Breach contained; Forensics completed; Security hardened"),
            new("Security Services", "Incident Response", "Ransomware Attack", "Complaint",
                "System infected with ransomware or encryption detected",
                "Ransomware Response Workflow",
                "Isolate infected systems; Identify ransomware variant; Attempt decryption; Restore from backup; Strengthen defenses; User training",
                "Systems restored; Ransomware removed; Prevention enhanced"),
            
            // Specialized Services - VoIP & UC
            new("Specialized Services", "VoIP & UC", "No Dial Tone", "Complaint",
                "Phone not working or cannot make/receive calls",
                "VoIP Troubleshooting Workflow",
                "Check phone registration; Verify network; Test handset; Reconfigure phone; Replace hardware; Check PBX status",
                "Phone operational; Calls working; Configuration verified"),
            new("Specialized Services", "VoIP & UC", "Call Quality Issues", "Complaint",
                "Poor audio quality echo jitter or dropped calls",
                "Call Quality Workflow",
                "Test network quality; Check bandwidth; Implement QoS; Update firmware; Adjust codec; Optimize network path",
                "Call quality improved; QoS implemented; Network optimized"),
            new("Specialized Services", "VoIP & UC", "Extension Addition", "Request",
                "New user needs phone extension or voicemail setup",
                "User Provisioning Workflow",
                "Create extension; Configure voicemail; Assign phone; Program features; Test functionality; Train user",
                "Extension active; Voicemail configured; User trained"),
            
            // Specialized Services - Cabling Services
            new("Specialized Services", "Cabling Services", "Cable Run Request", "Request",
                "New cable installation or additional network drops needed",
                "Cabling Installation Workflow",
                "Site survey; Plan cable route; Pull cables; Terminate connections; Test certification; Label cables; Update documentation",
                "Cables installed; Testing passed; Documentation updated"),
            new("Specialized Services", "Cabling Services", "Cable Certification", "Request",
                "Customer needs cable testing and certification report",
                "Cable Testing Workflow",
                "Test all runs; Generate test reports; Identify failures; Remediate issues; Provide certification documentation",
                "Cables certified; Reports provided; Issues resolved"),
            
            // Data & Analytics - Business Intelligence
            new("Data & Analytics", "Business Intelligence", "Report Not Loading", "Complaint",
                "Dashboard or report failing to load or showing errors",
                "Report Troubleshooting Workflow",
                "Check data source; Verify credentials; Refresh cache; Rebuild report; Optimize queries; Test data connection",
                "Report operational; Data refreshing; Performance optimized"),
            new("Data & Analytics", "Business Intelligence", "Custom Dashboard Request", "Request",
                "User needs new dashboard or report created",
                "Dashboard Development Workflow",
                "Gather requirements; Design layout; Build dashboard; Connect data sources; Test functionality; Deploy; Train users",
                "Dashboard delivered; Users trained; Refresh scheduled"),
            
            // Data & Analytics - Data Warehousing
            new("Data & Analytics", "Data Warehousing", "ETL Job Failure", "Complaint",
                "Data extraction transformation or loading process failed",
                "ETL Troubleshooting Workflow",
                "Review error logs; Check source connection; Validate data; Fix transformation logic; Rerun job; Monitor completion",
                "ETL job successful; Data loaded; Monitoring enhanced"),
            new("Data & Analytics", "Data Warehousing", "Data Quality Issues", "Complaint",
                "Incorrect duplicate or missing data in warehouse",
                "Data Quality Workflow",
                "Identify data issues; Trace to source; Implement validation; Clean data; Add quality checks; Prevent recurrence",
                "Data quality improved; Validation rules added; Issues resolved")
        };
    }
    
    // Helper class for service request type data
    private record ServiceRequestTypeData(
        string Category, 
        string Subcategory, 
        string Name, 
        string Type, 
        string Description, 
        string WorkflowName, 
        string PossibleResolutions, 
        string FinalCustomerResolutions);

    /// <summary>
    /// Seed customers to production database
    /// </summary>
    public async Task SeedCustomersAsync()
    {
        var dbContext = _context as CrmDbContext ?? throw new InvalidOperationException("Context must be CrmDbContext");
        await SeedCustomersToContextAsync(dbContext);
    }

    /// <summary>
    /// Seed 100 customers (70 organizations, 30 individuals)
    /// </summary>
    private async Task SeedCustomersToContextAsync(CrmDbContext context)
    {
        _logger.LogInformation("Seeding sample customers...");
        
        var existingCustomers = await context.Customers.CountAsync();
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

        context.Customers.AddRange(customers);
        await context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} sample customers", customers.Count);
    }

    /// <summary>
    /// Seed contacts to production database
    /// </summary>
    public async Task SeedContactsAsync()
    {
        var dbContext = _context as CrmDbContext ?? throw new InvalidOperationException("Context must be CrmDbContext");
        await SeedContactsToContextAsync(dbContext);
    }

    /// <summary>
    /// Seed contacts for organization customers
    /// </summary>
    private async Task SeedContactsToContextAsync(CrmDbContext context)
    {
        _logger.LogInformation("Seeding sample contacts...");
        
        var existingContacts = await context.Contacts.CountAsync();
        if (existingContacts >= 50)
        {
            _logger.LogInformation("Contacts already exist. Skipping...");
            return;
        }

        var orgCustomers = await context.Customers
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
                
                context.Contacts.Add(contact);
            }
        }

        await context.SaveChangesAsync();
        _logger.LogInformation("Seeded customer contacts");
    }

    /// <summary>
    /// Seed leads to production database
    /// </summary>
    public async Task SeedLeadsAsync()
    {
        var dbContext = _context as CrmDbContext ?? throw new InvalidOperationException("Context must be CrmDbContext");
        await SeedLeadsToContextAsync(dbContext);
    }

    /// <summary>
    /// Seed 100 leads (60 organizations, 40 individuals)
    /// Uses new 3NF Lead entity with LeadLifecycleStatus and simplified LeadSource
    /// </summary>
    private async Task SeedLeadsToContextAsync(CrmDbContext context)
    {
        _logger.LogInformation("Seeding sample leads...");
        
        var existingLeads = await context.Leads.CountAsync();
        if (existingLeads >= 50)
        {
            _logger.LogInformation("Leads already exist. Skipping...");
            return;
        }

        var leads = new List<Lead>();
        var leadStatuses = new[] { LeadLifecycleStatus.New, LeadLifecycleStatus.Working, LeadLifecycleStatus.Nurturing, LeadLifecycleStatus.Qualified, LeadLifecycleStatus.Disqualified, LeadLifecycleStatus.Converted };
        var leadSources = new[] { LeadSource.Web, LeadSource.Campaign, LeadSource.Referral, LeadSource.Event, LeadSource.Partner, LeadSource.Manual };
        var regions = new[] { "North America", "West Coast", "East Coast", "Midwest", "South", "International" };
        var jobTitles = new[] { "IT Director", "CTO", "IT Manager", "Procurement Manager", "Owner", "VP Engineering", "Director of Operations" };

        // 60 Organization Leads
        for (int i = 1; i <= 60; i++)
        {
            var prefix = CompanyPrefixes[_random.Next(CompanyPrefixes.Length)];
            var suffix = CompanySuffixes[_random.Next(CompanySuffixes.Length)];
            var firstName = FirstNames[_random.Next(FirstNames.Length)];
            var lastName = LastNames[_random.Next(LastNames.Length)];
            var status = leadStatuses[_random.Next(leadStatuses.Length)];
            var score = _random.Next(10, 100);
            
            leads.Add(new Lead
            {
                FirstName = firstName,
                LastName = lastName,
                Email = $"{firstName.ToLower()}.{lastName.ToLower()}@{prefix.ToLower()}{suffix.ToLower()}.com",
                Phone = $"555-{_random.Next(100, 999)}-{_random.Next(1000, 9999)}",
                CompanyName = $"{prefix} {suffix}",
                Title = jobTitles[_random.Next(jobTitles.Length)],
                Website = $"https://www.{prefix.ToLower()}{suffix.ToLower()}.com",
                Status = status,
                Source = leadSources[_random.Next(leadSources.Length)],
                Score = score,
                FitScore = _random.Next(20, 80),
                EngagementScore = _random.Next(10, 50),
                Region = regions[_random.Next(regions.Length)],
                QualificationNotes = status == LeadLifecycleStatus.Qualified ? "Meets BANT criteria" : null,
                MqlDate = status == LeadLifecycleStatus.Qualified || status == LeadLifecycleStatus.Converted ? DateTime.UtcNow.AddDays(-_random.Next(10, 60)) : null,
                SqlDate = status == LeadLifecycleStatus.Converted ? DateTime.UtcNow.AddDays(-_random.Next(1, 30)) : null,
                CreatedAt = DateTime.UtcNow.AddDays(-_random.Next(1, 180))
            });
        }

        // 40 Individual Leads
        for (int i = 1; i <= 40; i++)
        {
            var firstName = FirstNames[_random.Next(FirstNames.Length)];
            var lastName = LastNames[_random.Next(LastNames.Length)];
            var status = leadStatuses[_random.Next(leadStatuses.Length)];
            var score = _random.Next(10, 80);
            
            leads.Add(new Lead
            {
                FirstName = firstName,
                LastName = lastName,
                Email = $"{firstName.ToLower()}.{lastName.ToLower()}@personalmail.com",
                Phone = $"555-{_random.Next(100, 999)}-{_random.Next(1000, 9999)}",
                Status = status,
                Source = leadSources[_random.Next(leadSources.Length)],
                Score = score,
                FitScore = _random.Next(10, 60),
                EngagementScore = _random.Next(5, 40),
                Region = regions[_random.Next(regions.Length)],
                CreatedAt = DateTime.UtcNow.AddDays(-_random.Next(1, 180))
            });
        }

        context.Leads.AddRange(leads);
        await context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} sample leads", leads.Count);
    }

    /// <summary>
    /// Seed opportunities to production database
    /// </summary>
    public async Task SeedOpportunitiesAsync()
    {
        var dbContext = _context as CrmDbContext ?? throw new InvalidOperationException("Context must be CrmDbContext");
        await SeedOpportunitiesToContextAsync(dbContext);
    }

    /// <summary>
    /// Seed 100 opportunities linked to accounts
    /// Uses new 3NF Opportunity entity with OpportunityStage and Account relationships
    /// </summary>
    private async Task SeedOpportunitiesToContextAsync(CrmDbContext context)
    {
        _logger.LogInformation("Seeding sample opportunities...");
        
        var existingOpportunities = await context.Opportunities.CountAsync();
        if (existingOpportunities >= 50)
        {
            _logger.LogInformation("Opportunities already exist. Skipping...");
            return;
        }

        var accounts = await context.Accounts.Take(100).ToListAsync();
        if (!accounts.Any())
        {
            _logger.LogWarning("No accounts found. Skipping opportunities seeding.");
            return;
        }

        var opportunities = new List<Opportunity>();
        var stages = new[] { OpportunityStage.Discovery, OpportunityStage.Qualification, OpportunityStage.Proposal, OpportunityStage.Negotiation };
        var pricingModels = new[] { OpportunityPricingModel.Subscription, OpportunityPricingModel.OneTime, OpportunityPricingModel.UsageBased, OpportunityPricingModel.Hybrid };
        var regions = new[] { "North America", "West Coast", "East Coast", "Midwest", "South", "International" };
        var currencies = new[] { "USD", "EUR", "GBP" };
        var termLengths = new[] { 12, 24, 36 };
        var opportunityNames = new[] { "Server Upgrade", "Software License", "Managed Services", "Cloud Migration", "Support Contract", "Hardware Purchase", "IT Assessment", "Security Audit" };

        for (int i = 1; i <= 100; i++)
        {
            var account = accounts[_random.Next(accounts.Count)];
            var stage = stages[_random.Next(stages.Length)];
            var amount = _random.Next(1, 50) * 1000m;
            var probability = stage switch
            {
                OpportunityStage.Discovery => 10,
                OpportunityStage.Qualification => 25,
                OpportunityStage.Proposal => 50,
                OpportunityStage.Negotiation => 75,
                _ => 50
            };

            opportunities.Add(new Opportunity
            {
                Name = $"{account.AccountNumber} - {opportunityNames[_random.Next(opportunityNames.Length)]}",
                AccountId = account.Id,
                SolutionNotes = "Demo opportunity for testing the CRM system",
                Amount = amount,
                Stage = stage,
                Probability = probability,
                Currency = currencies[_random.Next(currencies.Length)],
                PricingModel = pricingModels[_random.Next(pricingModels.Length)],
                TermLengthMonths = termLengths[_random.Next(termLengths.Length)],
                Region = regions[_random.Next(regions.Length)],
                ExpectedCloseDate = DateTime.UtcNow.AddDays(_random.Next(7, 90)),
                QualificationReason = stage >= OpportunityStage.Qualification ? (QualificationReason?)_random.Next(0, 5) : null,
                QualificationNotes = stage >= OpportunityStage.Qualification ? "Qualified through BANT methodology" : null,
                CreatedAt = DateTime.UtcNow.AddDays(-_random.Next(1, 60))
            });
        }

        context.Opportunities.AddRange(opportunities);
        await context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} demo opportunities", opportunities.Count);
    }

    /// <summary>
    /// Clear all sample data from the production database.
    /// Preserves master data (ZipCodes, ColorPalettes) and admin users.
    /// </summary>
    public async Task ClearSampleDataAsync()
    {
        _logger.LogInformation("Clearing sample data from production database...");
        
        var dbContext = _context as CrmDbContext ?? throw new InvalidOperationException("Context must be CrmDbContext");
        
        // Clear all sample data tables in reverse dependency order
        // Preserves: ZipCodes, ColorPalettes, SystemSettings, non-demo Users
        
        // Clear service requests first (depends on everything)
        var serviceRequests = await dbContext.ServiceRequests.ToListAsync();
        dbContext.ServiceRequests.RemoveRange(serviceRequests);
        await dbContext.SaveChangesAsync();
        
        // Opportunities depend on Accounts
        var opportunities = await dbContext.Opportunities.ToListAsync();
        dbContext.Opportunities.RemoveRange(opportunities);
        await dbContext.SaveChangesAsync();
        
        // Leads
        var leads = await dbContext.Leads.ToListAsync();
        dbContext.Leads.RemoveRange(leads);
        await dbContext.SaveChangesAsync();
        
        // Contacts depend on Customers
        var contacts = await dbContext.Contacts.ToListAsync();
        dbContext.Contacts.RemoveRange(contacts);
        await dbContext.SaveChangesAsync();
        
        // Customers/Accounts
        var customers = await dbContext.Customers.ToListAsync();
        dbContext.Customers.RemoveRange(customers);
        await dbContext.SaveChangesAsync();
        
        // Service Request Types (depends on subcategories and categories)
        var types = await dbContext.ServiceRequestTypes.ToListAsync();
        dbContext.ServiceRequestTypes.RemoveRange(types);
        await dbContext.SaveChangesAsync();
        
        // Service Request Subcategories (depends on categories)
        var subcategories = await dbContext.ServiceRequestSubcategories.ToListAsync();
        dbContext.ServiceRequestSubcategories.RemoveRange(subcategories);
        await dbContext.SaveChangesAsync();
        
        // Service Request Categories
        var categories = await dbContext.ServiceRequestCategories.ToListAsync();
        dbContext.ServiceRequestCategories.RemoveRange(categories);
        await dbContext.SaveChangesAsync();
        
        // Products
        var products = await dbContext.Products.ToListAsync();
        dbContext.Products.RemoveRange(products);
        await dbContext.SaveChangesAsync();
        
        // Sample users (username starts with "demo." or "sample.")
        var sampleUsers = await dbContext.Users
            .Where(u => u.Username.StartsWith("demo.") || u.Username.StartsWith("sample."))
            .ToListAsync();
        dbContext.Users.RemoveRange(sampleUsers);
        await dbContext.SaveChangesAsync();
        
        // Update settings if they exist
        var settings = await dbContext.SystemSettings.FirstOrDefaultAsync();
        if (settings != null)
        {
            settings.SampleDataSeeded = false;
        }
        
        await dbContext.SaveChangesAsync();
        _logger.LogInformation("Sample data cleared from production database. Master data (ZipCodes, ColorPalettes) preserved.");
    }

    /// <summary>
    /// Get sample data statistics
    /// </summary>
    public async Task<SampleDataStats> GetSampleDataStatsAsync()
    {
        var dbContext = _context as CrmDbContext ?? throw new InvalidOperationException("Context must be CrmDbContext");
        
        return new SampleDataStats
        {
            ProductCount = await dbContext.Products.CountAsync(),
            CustomerCount = await dbContext.Customers.CountAsync(),
            ContactCount = await dbContext.Contacts.CountAsync(),
            LeadCount = await dbContext.Leads.CountAsync(),
            OpportunityCount = await dbContext.Opportunities.CountAsync(),
            ServiceRequestTypeCount = await dbContext.ServiceRequestTypes.CountAsync(),
            ServiceRequestCategoryCount = await dbContext.ServiceRequestCategories.CountAsync(),
            ServiceRequestSubcategoryCount = await dbContext.ServiceRequestSubcategories.CountAsync(),
            SampleUserCount = await dbContext.Users.CountAsync(u => u.Username.StartsWith("demo.") || u.Username.StartsWith("sample."))
        };
    }
}

/// <summary>
/// Statistics about sample data in the database
/// </summary>
public class SampleDataStats
{
    public int ProductCount { get; set; }
    public int CustomerCount { get; set; }
    public int ContactCount { get; set; }
    public int LeadCount { get; set; }
    public int OpportunityCount { get; set; }
    public int ServiceRequestTypeCount { get; set; }
    public int ServiceRequestCategoryCount { get; set; }
    public int ServiceRequestSubcategoryCount { get; set; }
    public int SampleUserCount { get; set; }
}
