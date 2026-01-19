using CRM.Core.Entities;
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
        await context.Database.MigrateAsync();

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
    }
}
