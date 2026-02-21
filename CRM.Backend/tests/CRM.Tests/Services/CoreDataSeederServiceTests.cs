// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Models;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for <see cref="CoreDataSeederService"/>.
/// Validates that seed methods populate the in-memory database correctly
/// and behave idempotently when data already exists.
/// </summary>
public class CoreDataSeederServiceTests
{
    private readonly Mock<ILogger<CoreDataSeederService>> _loggerMock;

    public CoreDataSeederServiceTests()
    {
        _loggerMock = new Mock<ILogger<CoreDataSeederService>>();
    }

    private static CrmDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();
        return new CrmDbContext(options, configuration);
    }

    private CoreDataSeederService CreateService(CrmDbContext context)
    {
        return new CoreDataSeederService(context, _loggerMock.Object);
    }

    // ──────────────────────────────────────────────
    // SeedDepartmentsAsync
    // ──────────────────────────────────────────────

    [Fact]
    public async Task SeedDepartmentsAsync_ShouldSeedDepartments_WhenNoneExist()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = CreateService(context);

        // Act
        await service.SeedDepartmentsAsync();

        // Assert
        var departments = await context.Departments.ToListAsync();
        Assert.NotEmpty(departments);
    }

    [Fact]
    public async Task SeedDepartmentsAsync_ShouldNotDuplicate_WhenDepartmentsAlreadyExist()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = CreateService(context);

        // Seed once
        await service.SeedDepartmentsAsync();
        var countAfterFirstSeed = await context.Departments.CountAsync();

        // Act - seed again
        await service.SeedDepartmentsAsync();
        var countAfterSecondSeed = await context.Departments.CountAsync();

        // Assert
        Assert.Equal(countAfterFirstSeed, countAfterSecondSeed);
    }

    [Fact]
    public async Task SeedDepartmentsAsync_ShouldSeedAtLeastTenDepartments_WhenNoneExist()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = CreateService(context);

        // Act
        await service.SeedDepartmentsAsync();

        // Assert
        var count = await context.Departments.CountAsync();
        Assert.True(count >= 10, $"Expected at least 10 departments but found {count}");
    }

    [Fact]
    public async Task SeedDepartmentsAsync_ShouldSetRequiredProperties_WhenSeeding()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = CreateService(context);

        // Act
        await service.SeedDepartmentsAsync();

        // Assert
        var departments = await context.Departments.ToListAsync();
        Assert.All(departments, dept =>
        {
            Assert.False(string.IsNullOrWhiteSpace(dept.Name));
        });
    }

    // ──────────────────────────────────────────────
    // SeedSampleAccountsAsync
    // ──────────────────────────────────────────────

    [Fact]
    public async Task SeedSampleAccountsAsync_ShouldSeedAccounts_WhenNoneExist()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = CreateService(context);

        // Act
        await service.SeedSampleAccountsAsync();

        // Assert
        var accounts = await context.Accounts.ToListAsync();
        Assert.NotEmpty(accounts);
    }

    [Fact]
    public async Task SeedSampleAccountsAsync_ShouldNotDuplicate_WhenAccountsAlreadyExist()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = CreateService(context);

        // Seed once
        await service.SeedSampleAccountsAsync();
        var countAfterFirstSeed = await context.Accounts.CountAsync();

        // Act - seed again
        await service.SeedSampleAccountsAsync();
        var countAfterSecondSeed = await context.Accounts.CountAsync();

        // Assert
        Assert.Equal(countAfterFirstSeed, countAfterSecondSeed);
    }

    [Fact]
    public async Task SeedSampleAccountsAsync_ShouldSetEmailOnAccounts_WhenSeeding()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = CreateService(context);

        // Act
        await service.SeedSampleAccountsAsync();

        // Assert
        var accounts = await context.Accounts.ToListAsync();
        Assert.All(accounts, account =>
        {
            Assert.False(string.IsNullOrWhiteSpace(account.Email));
        });
    }

    // ──────────────────────────────────────────────
    // SeedSampleProductsAsync
    // ──────────────────────────────────────────────

    [Fact]
    public async Task SeedSampleProductsAsync_ShouldSeedProducts_WhenNoneExist()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = CreateService(context);

        // Act
        await service.SeedSampleProductsAsync();

        // Assert
        var products = await context.Products.ToListAsync();
        Assert.NotEmpty(products);
    }

    [Fact]
    public async Task SeedSampleProductsAsync_ShouldNotDuplicate_WhenProductsAlreadyExist()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = CreateService(context);

        // Seed once
        await service.SeedSampleProductsAsync();
        var countAfterFirstSeed = await context.Products.CountAsync();

        // Act - seed again
        await service.SeedSampleProductsAsync();
        var countAfterSecondSeed = await context.Products.CountAsync();

        // Assert
        Assert.Equal(countAfterFirstSeed, countAfterSecondSeed);
    }

    // ──────────────────────────────────────────────
    // SeedSampleContactsAsync
    // ──────────────────────────────────────────────

    [Fact]
    public async Task SeedSampleContactsAsync_ShouldSeedContacts_WhenNoneExist()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = CreateService(context);

        // Act
        await service.SeedSampleContactsAsync();

        // Assert
        var contacts = await context.Contacts.ToListAsync();
        Assert.NotEmpty(contacts);
    }

    [Fact]
    public async Task SeedSampleContactsAsync_ShouldNotDuplicate_WhenContactsAlreadyExist()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = CreateService(context);

        // Seed once
        await service.SeedSampleContactsAsync();
        var countAfterFirstSeed = await context.Contacts.CountAsync();

        // Act - seed again
        await service.SeedSampleContactsAsync();
        var countAfterSecondSeed = await context.Contacts.CountAsync();

        // Assert
        Assert.Equal(countAfterFirstSeed, countAfterSecondSeed);
    }

    // ──────────────────────────────────────────────
    // SeedLookupsAsync
    // ──────────────────────────────────────────────

    [Fact]
    public async Task SeedLookupsAsync_ShouldSeedLookupCategories_WhenNoneExist()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = CreateService(context);

        // Act
        await service.SeedLookupsAsync();

        // Assert
        var categories = await context.LookupCategories.ToListAsync();
        Assert.NotEmpty(categories);
    }

    [Fact]
    public async Task SeedLookupsAsync_ShouldNotDuplicateLookups_WhenCalledTwice()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = CreateService(context);

        // Seed once
        await service.SeedLookupsAsync();
        var categoriesAfterFirst = await context.LookupCategories.CountAsync();
        var itemsAfterFirst = await context.LookupItems.CountAsync();

        // Act - seed again
        await service.SeedLookupsAsync();
        var categoriesAfterSecond = await context.LookupCategories.CountAsync();
        var itemsAfterSecond = await context.LookupItems.CountAsync();

        // Assert
        Assert.Equal(categoriesAfterFirst, categoriesAfterSecond);
        Assert.Equal(itemsAfterFirst, itemsAfterSecond);
    }

    // ──────────────────────────────────────────────
    // SeedSystemSettingsAsync
    // ──────────────────────────────────────────────

    [Fact]
    public async Task SeedSystemSettingsAsync_ShouldSeedSettings_WhenNoneExist()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = CreateService(context);

        // Act
        await service.SeedSystemSettingsAsync();

        // Assert
        var settings = await context.SystemSettings.ToListAsync();
        Assert.NotEmpty(settings);
    }

    [Fact]
    public async Task SeedSystemSettingsAsync_ShouldNotDuplicate_WhenCalledTwice()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = CreateService(context);

        // Seed once
        await service.SeedSystemSettingsAsync();
        var countAfterFirst = await context.SystemSettings.CountAsync();

        // Act - seed again
        await service.SeedSystemSettingsAsync();
        var countAfterSecond = await context.SystemSettings.CountAsync();

        // Assert
        Assert.Equal(countAfterFirst, countAfterSecond);
    }

    // ──────────────────────────────────────────────
    // SeedModuleFieldConfigurationsAsync
    // ──────────────────────────────────────────────

    [Fact]
    public async Task SeedModuleFieldConfigurationsAsync_ShouldSeedConfigurations_WhenNoneExist()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = CreateService(context);

        // Act
        await service.SeedModuleFieldConfigurationsAsync();

        // Assert
        var configs = await context.ModuleFieldConfigurations.ToListAsync();
        Assert.NotEmpty(configs);
    }

    // ──────────────────────────────────────────────
    // ForceReseedModuleFieldConfigurationsAsync
    // ──────────────────────────────────────────────

    [Fact]
    public async Task ForceReseedModuleFieldConfigurationsAsync_ShouldClearAndReseed_WhenConfigsExist()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = CreateService(context);

        // Seed initial data
        await service.SeedModuleFieldConfigurationsAsync();
        var initialCount = await context.ModuleFieldConfigurations.CountAsync();
        Assert.True(initialCount > 0, "Expected initial seed to produce module field configurations");

        // Act - force reseed
        await service.ForceReseedModuleFieldConfigurationsAsync();

        // Assert - should still have configs (cleared then reseeded)
        var reseedCount = await context.ModuleFieldConfigurations.CountAsync();
        Assert.True(reseedCount > 0, "Expected force reseed to repopulate module field configurations");
    }

    [Fact]
    public async Task ForceReseedModuleFieldConfigurationsAsync_ShouldSucceed_WhenNoConfigsExist()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = CreateService(context);

        // Act - force reseed on empty database (should not throw)
        await service.ForceReseedModuleFieldConfigurationsAsync();

        // Assert
        var configs = await context.ModuleFieldConfigurations.ToListAsync();
        Assert.NotEmpty(configs);
    }

    // ──────────────────────────────────────────────
    // SeedEnsureLookupsAsync
    // ──────────────────────────────────────────────

    [Fact]
    public async Task SeedEnsureLookupsAsync_ShouldSeedLookups_WhenNoneExist()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = CreateService(context);

        // Act
        await service.SeedEnsureLookupsAsync();

        // Assert
        var categories = await context.LookupCategories.CountAsync();
        var items = await context.LookupItems.CountAsync();
        Assert.True(categories > 0 || items > 0, "Expected EnsureLookups to seed at least some lookup data");
    }

    // ──────────────────────────────────────────────
    // SeedAdditionalMasterDataAsync
    // ──────────────────────────────────────────────

    [Fact]
    public async Task SeedAdditionalMasterDataAsync_ShouldNotThrow_WhenCalledOnEmptyDatabase()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = CreateService(context);

        // Act & Assert - should complete without exceptions
        var exception = await Record.ExceptionAsync(() => service.SeedAdditionalMasterDataAsync());
        Assert.Null(exception);
    }
}
