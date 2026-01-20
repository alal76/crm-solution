using CRM.Core.Entities;
using CRM.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace CRM.Infrastructure.Data;

/// <summary>
/// Database seeding for initial admin user and test data
/// </summary>
public class DbSeed
{
    /// <summary>
    /// Hash a password using SHA-256
    /// </summary>
    private static string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    /// <summary>
    /// Seed initial admin user and sample data
    /// </summary>
    public static async Task SeedAsync(CrmDbContext context)
    {
        // Seed Admin User
        if (!context.Users.Any(u => u.Email == "abhi.lal@gmail.com"))
        {
            var adminUser = new User
            {
                Username = "admin",
                Email = "abhi.lal@gmail.com",
                FirstName = "Abhishek",
                LastName = "Lal",
                PasswordHash = HashPassword("Admin@123"), // Change this in production!
                Role = (int)UserRole.Admin,
                IsActive = true,
                EmailVerified = true,
                TwoFactorEnabled = false
            };

            context.Users.Add(adminUser);
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
                            Platform = SocialMediaPlatform.LinkedIn,
                            Url = "https://linkedin.com/in/michaeljohnson",
                            Handle = "michaeljohnson",
                            DateAdded = DateTime.UtcNow
                        },
                        new SocialMediaLink
                        {
                            Platform = SocialMediaPlatform.Twitter,
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
                            Platform = SocialMediaPlatform.LinkedIn,
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
                            Platform = SocialMediaPlatform.Website,
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
                            Platform = SocialMediaPlatform.LinkedIn,
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
                            Platform = SocialMediaPlatform.GitHub,
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
                            Platform = SocialMediaPlatform.LinkedIn,
                            Url = "https://linkedin.com/in/lisathompson",
                            Handle = "lisathompson",
                            DateAdded = DateTime.UtcNow
                        },
                        new SocialMediaLink
                        {
                            Platform = SocialMediaPlatform.Website,
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
    }
}
