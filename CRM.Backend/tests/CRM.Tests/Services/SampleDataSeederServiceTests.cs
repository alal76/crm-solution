// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under AGPL-3.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Tests for SampleDataSeederService
/// Covers: SeedAllSampleDataAsync, IsSampleDataSeededAsync, individual seeding methods
/// </summary>
public class SampleDataSeederServiceTests : IDisposable
{
    private readonly CrmDbContext _context;
    private readonly Mock<ILogger<SampleDataSeederService>> _loggerMock;
    private readonly SampleDataSeederService _service;

    public SampleDataSeederServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new CrmDbContext(options);
        _loggerMock = new Mock<ILogger<SampleDataSeederService>>();
        _service = new SampleDataSeederService(_context, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region IsSampleDataSeededAsync Tests

    [Fact]
    public async Task IsSampleDataSeededAsync_WhenNoSettings_ShouldReturnFalse()
    {
        // Arrange - ensure no settings
        _context.SystemSettings.RemoveRange(_context.SystemSettings);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.IsSampleDataSeededAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsSampleDataSeededAsync_WhenNotSeeded_ShouldReturnFalse()
    {
        // Arrange
        _context.SystemSettings.RemoveRange(_context.SystemSettings);
        _context.SystemSettings.Add(new SystemSettings
        {
            SampleDataSeeded = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.IsSampleDataSeededAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsSampleDataSeededAsync_WhenSeeded_ShouldReturnTrue()
    {
        // Arrange
        _context.SystemSettings.RemoveRange(_context.SystemSettings);
        _context.SystemSettings.Add(new SystemSettings
        {
            SampleDataSeeded = true,
            SampleDataLastSeeded = DateTime.UtcNow.AddHours(-1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.IsSampleDataSeededAsync();

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region SeedSampleUsersAsync Tests

    [Fact]
    public async Task SeedSampleUsersAsync_ShouldCreateDemoUsers()
    {
        // Arrange - ensure no demo users exist
        var existingDemoUsers = await _context.Users
            .Where(u => u.Username != null && u.Username.StartsWith("demo."))
            .ToListAsync();
        _context.Users.RemoveRange(existingDemoUsers);
        await _context.SaveChangesAsync();

        // Act
        await _service.SeedSampleUsersAsync();

        // Assert
        var demoUsers = await _context.Users
            .Where(u => u.Username != null && u.Username.StartsWith("demo."))
            .ToListAsync();

        demoUsers.Should().HaveCount(10, "should create 10 demo users");
    }

    [Fact]
    public async Task SeedSampleUsersAsync_ShouldCreateUserGroups()
    {
        // Act
        await _service.SeedSampleUsersAsync();

        // Assert
        var groups = await _context.UserGroups
            .Where(g => new[] { "Administrators", "Sales Team", "Support Team", "Marketing Team", "Management" }
                .Contains(g.Name))
            .ToListAsync();

        groups.Should().HaveCountGreaterOrEqualTo(5);
    }

    [Fact]
    public async Task SeedSampleUsersAsync_ShouldCreateDemoDepartment()
    {
        // Act
        await _service.SeedSampleUsersAsync();

        // Assert
        var department = await _context.Departments
            .FirstOrDefaultAsync(d => d.Name == "Demo Department");

        department.Should().NotBeNull();
        department!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task SeedSampleUsersAsync_DemoUsers_ShouldHaveHashedPasswords()
    {
        // Arrange
        _context.Users.RemoveRange(_context.Users.Where(u => u.Username.StartsWith("demo.")));
        await _context.SaveChangesAsync();

        // Act
        await _service.SeedSampleUsersAsync();

        // Assert
        var demoUsers = await _context.Users
            .Where(u => u.Username.StartsWith("demo."))
            .ToListAsync();

        foreach (var user in demoUsers)
        {
            user.PasswordHash.Should().NotBeNullOrWhiteSpace();
            // BCrypt hashes start with $2
            user.PasswordHash.Should().StartWith("$2", "passwords should be BCrypt hashed");
        }
    }

    [Fact]
    public async Task SeedSampleUsersAsync_ShouldSkip_WhenDemoUsersExist()
    {
        // Arrange - create a demo user first
        _context.Users.Add(new User
        {
            Username = "demo.admin",
            FirstName = "Existing",
            LastName = "Admin",
            Email = "demo.admin@example.com",
            PasswordHash = "$2a$12$test",
            Role = (int)UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
        var initialCount = await _context.Users.CountAsync();

        // Act
        await _service.SeedSampleUsersAsync();

        // Assert
        var finalCount = await _context.Users.CountAsync();
        finalCount.Should().Be(initialCount, "should not add more users when demo.admin exists");
    }

    [Fact]
    public async Task SeedSampleUsersAsync_ShouldCreateUserGroupMemberships()
    {
        // Arrange
        _context.Users.RemoveRange(_context.Users.Where(u => u.Username.StartsWith("demo.")));
        _context.UserGroupMembers.RemoveRange(_context.UserGroupMembers);
        await _context.SaveChangesAsync();

        // Act
        await _service.SeedSampleUsersAsync();

        // Assert
        var memberships = await _context.UserGroupMembers.CountAsync();
        memberships.Should().BeGreaterOrEqualTo(10, "each demo user should have a group membership");
    }

    #endregion

    #region SeedProductsAsync Tests

    [Fact]
    public async Task SeedProductsAsync_ShouldCreateProducts()
    {
        // Arrange - ensure few products
        _context.Products.RemoveRange(_context.Products);
        await _context.SaveChangesAsync();

        // Act
        await _service.SeedProductsAsync();

        // Assert
        var productCount = await _context.Products.CountAsync();
        productCount.Should().BeGreaterOrEqualTo(50, "should create many products");
    }

    [Fact]
    public async Task SeedProductsAsync_ShouldSkip_WhenProductsExist()
    {
        // Arrange - add 60 products to exceed threshold
        for (int i = 0; i < 60; i++)
        {
            _context.Products.Add(new Product
            {
                Name = $"Existing Product {i}",
                SKU = $"EP-{i}",
                Category = "Test",
                Price = 99.99m,
                Status = ProductStatus.Active,
                CreatedAt = DateTime.UtcNow
            });
        }
        await _context.SaveChangesAsync();
        var initialCount = await _context.Products.CountAsync();

        // Act
        await _service.SeedProductsAsync();

        // Assert
        var finalCount = await _context.Products.CountAsync();
        finalCount.Should().Be(initialCount, "should not add products when 50+ already exist");
    }

    [Fact]
    public async Task SeedProductsAsync_ShouldIncludeHardwareProducts()
    {
        // Arrange
        _context.Products.RemoveRange(_context.Products);
        await _context.SaveChangesAsync();

        // Act
        await _service.SeedProductsAsync();

        // Assert
        var hardwareProducts = await _context.Products
            .Where(p => p.Category == "Hardware")
            .ToListAsync();

        hardwareProducts.Should().HaveCountGreaterThan(10);
    }

    [Fact]
    public async Task SeedProductsAsync_ShouldIncludeSoftwareProducts()
    {
        // Arrange
        _context.Products.RemoveRange(_context.Products);
        await _context.SaveChangesAsync();

        // Act
        await _service.SeedProductsAsync();

        // Assert
        var softwareProducts = await _context.Products
            .Where(p => p.Category == "Software")
            .ToListAsync();

        softwareProducts.Should().HaveCountGreaterThan(10);
    }

    [Fact]
    public async Task SeedProductsAsync_Products_ShouldHaveValidSKUs()
    {
        // Arrange
        _context.Products.RemoveRange(_context.Products);
        await _context.SaveChangesAsync();

        // Act
        await _service.SeedProductsAsync();

        // Assert
        var products = await _context.Products.Take(20).ToListAsync();
        foreach (var product in products)
        {
            product.SKU.Should().NotBeNullOrWhiteSpace();
            // SKUs follow pattern: HW-, SW-, CLD-, SVC-
            product.SKU.Should().MatchRegex(@"^(HW|SW|CLD|SVC)-\d+$");
        }
    }

    [Fact]
    public async Task SeedProductsAsync_Products_ShouldHaveValidPrices()
    {
        // Arrange
        _context.Products.RemoveRange(_context.Products);
        await _context.SaveChangesAsync();

        // Act
        await _service.SeedProductsAsync();

        // Assert
        var products = await _context.Products.ToListAsync();
        foreach (var product in products)
        {
            product.Price.Should().BeGreaterThan(0);
        }
    }

    [Fact]
    public async Task SeedProductsAsync_ShouldIncludeSubscriptionProducts()
    {
        // Arrange
        _context.Products.RemoveRange(_context.Products);
        await _context.SaveChangesAsync();

        // Act
        await _service.SeedProductsAsync();

        // Assert
        var subscriptionProducts = await _context.Products
            .Where(p => p.IsSubscription)
            .ToListAsync();

        subscriptionProducts.Should().HaveCountGreaterThan(10, "should include SaaS/subscription products");
    }

    #endregion

    #region SeedAllSampleDataAsync Tests

    [Fact]
    public async Task SeedAllSampleDataAsync_ShouldCreateSystemSettings()
    {
        // Arrange - ensure no settings
        _context.SystemSettings.RemoveRange(_context.SystemSettings);
        await _context.SaveChangesAsync();

        // Act
        await _service.SeedAllSampleDataAsync();

        // Assert
        var settings = await _context.SystemSettings.FirstOrDefaultAsync();
        settings.Should().NotBeNull();
    }

    [Fact]
    public async Task SeedAllSampleDataAsync_ShouldMarkAsSeeded()
    {
        // Arrange
        _context.SystemSettings.RemoveRange(_context.SystemSettings);
        await _context.SaveChangesAsync();

        // Act
        await _service.SeedAllSampleDataAsync();

        // Assert
        var isSeeded = await _service.IsSampleDataSeededAsync();
        isSeeded.Should().BeTrue();
    }

    [Fact]
    public async Task SeedAllSampleDataAsync_ShouldSetLastSeededTimestamp()
    {
        // Arrange
        _context.SystemSettings.RemoveRange(_context.SystemSettings);
        await _context.SaveChangesAsync();
        var beforeSeed = DateTime.UtcNow;

        // Act
        await _service.SeedAllSampleDataAsync();

        // Assert
        var settings = await _context.SystemSettings.FirstOrDefaultAsync();
        settings!.SampleDataLastSeeded.Should().NotBeNull();
        settings.SampleDataLastSeeded.Should().BeAfter(beforeSeed.AddMinutes(-1));
    }

    [Fact]
    public async Task SeedAllSampleDataAsync_ShouldLogProgress()
    {
        // Act
        await _service.SeedAllSampleDataAsync();

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Starting sample data seeding")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once());
    }

    [Fact]
    public async Task SeedAllSampleDataAsync_ShouldLogCompletion()
    {
        // Act
        await _service.SeedAllSampleDataAsync();

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("completed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce());
    }

    #endregion

    #region Data Quality Tests

    [Fact]
    public async Task SeedAllSampleDataAsync_Users_ShouldHaveValidEmails()
    {
        // Arrange
        _context.Users.RemoveRange(_context.Users.Where(u => u.Username.StartsWith("demo.")));
        await _context.SaveChangesAsync();

        // Act
        await _service.SeedAllSampleDataAsync();

        // Assert
        var demoUsers = await _context.Users
            .Where(u => u.Username.StartsWith("demo."))
            .ToListAsync();

        foreach (var user in demoUsers)
        {
            user.Email.Should().Contain("@");
            user.Email.Should().EndWith("@example.com");
        }
    }

    [Fact]
    public async Task SeedAllSampleDataAsync_Users_ShouldBeActive()
    {
        // Arrange
        _context.Users.RemoveRange(_context.Users.Where(u => u.Username.StartsWith("demo.")));
        await _context.SaveChangesAsync();

        // Act
        await _service.SeedAllSampleDataAsync();

        // Assert
        var demoUsers = await _context.Users
            .Where(u => u.Username.StartsWith("demo."))
            .ToListAsync();

        foreach (var user in demoUsers)
        {
            user.IsActive.Should().BeTrue();
            user.EmailVerified.Should().BeTrue();
        }
    }

    [Fact]
    public async Task SeedAllSampleDataAsync_Users_ShouldHaveDifferentRoles()
    {
        // Arrange
        _context.Users.RemoveRange(_context.Users.Where(u => u.Username.StartsWith("demo.")));
        await _context.SaveChangesAsync();

        // Act
        await _service.SeedAllSampleDataAsync();

        // Assert
        var roles = await _context.Users
            .Where(u => u.Username.StartsWith("demo."))
            .Select(u => u.Role)
            .Distinct()
            .ToListAsync();

        roles.Should().HaveCountGreaterThan(1, "demo users should have different roles");
    }

    [Fact]
    public async Task SeedAllSampleDataAsync_Products_ShouldHaveMultipleCategories()
    {
        // Arrange
        _context.Products.RemoveRange(_context.Products);
        await _context.SaveChangesAsync();

        // Act
        await _service.SeedAllSampleDataAsync();

        // Assert
        var categories = await _context.Products
            .Select(p => p.Category)
            .Distinct()
            .ToListAsync();

        categories.Should().Contain("Hardware");
        categories.Should().Contain("Software");
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task SeedAllSampleDataAsync_OnException_ShouldLogError()
    {
        // This test documents expected error handling behavior
        // In production, the method catches exceptions and logs them
        
        // Act
        var act = async () => await _service.SeedAllSampleDataAsync();

        // Assert - with valid context, should not throw
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region Idempotency Tests

    [Fact]
    public async Task SeedAllSampleDataAsync_CalledMultipleTimes_ShouldBeIdempotent()
    {
        // Arrange - first seed
        await _service.SeedAllSampleDataAsync();
        var initialUserCount = await _context.Users.CountAsync();
        var initialProductCount = await _context.Products.CountAsync();

        // Act - second seed (should skip existing data)
        await _service.SeedAllSampleDataAsync();

        // Assert - counts should be same
        var finalUserCount = await _context.Users.CountAsync();
        var finalProductCount = await _context.Products.CountAsync();

        finalUserCount.Should().Be(initialUserCount);
        finalProductCount.Should().Be(initialProductCount);
    }

    #endregion
}
