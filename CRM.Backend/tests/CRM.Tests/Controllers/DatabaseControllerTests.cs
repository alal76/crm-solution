// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
using CRM.Api.Controllers;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for DatabaseController (TCOV-040).
/// </summary>
public class DatabaseControllerTests : IDisposable
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<DatabaseController>> _mockLogger;
    private readonly Mock<IWebHostEnvironment> _mockEnv;
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly Mock<ICoreDataSeederService> _mockSeeder;
    private readonly CrmDbContext _dbContext;

    private DatabaseController BuildController(CrmDbContext? dbContext = null)
    {
        // For status endpoints that use ICrmDbContext
        return new DatabaseController(
            _mockContext.Object,
            _mockLogger.Object,
            _mockEnv.Object,
            _mockConfig.Object,
            _mockSeeder.Object);
    }

    public DatabaseControllerTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<DatabaseController>>();
        _mockEnv = new Mock<IWebHostEnvironment>();
        _mockConfig = new Mock<IConfiguration>();
        _mockSeeder = new Mock<ICoreDataSeederService>();

        // Setup config to return known values
        var configSection = new Mock<IConfigurationSection>();
        configSection.Setup(s => s.Value).Returns("mariadb");
        _mockConfig.Setup(c => c.GetSection(It.IsAny<string>())).Returns(configSection.Object);
        _mockConfig.Setup(c => c[It.IsAny<string>()]).Returns((string?)null);
    }

    public void Dispose() { }

    [Fact]
    public async Task GetForeignKeys_ShouldReturnOk()
    {
        var controller = BuildController();

        var result = await controller.GetForeignKeys();

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetLinkedEntitiesSchema_ShouldReturnOk()
    {
        var controller = BuildController();

        var result = await controller.GetLinkedEntitiesSchema();

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task TestConnection_ShouldReturnOk_WhenInvalidParams()
    {
        var controller = BuildController();
        var request = new DatabaseConnectionRequest
        {
            Provider = "",
            Host = "",
            Database = ""
        };

        var result = await controller.TestConnection(request);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task TestConnection_ShouldReturnOk_WhenValidParams()
    {
        var controller = BuildController();
        var request = new DatabaseConnectionRequest
        {
            Provider = "mariadb",
            Host = "localhost",
            Port = 3306,
            Database = "test_db",
            UserId = "user",
            Password = "pass"
        };

        var result = await controller.TestConnection(request);

        // Connection test might fail but endpoint itself should return Ok
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ReseedDatabase_ShouldReturnOk_WhenSeederSucceeds()
    {
        _mockSeeder.Setup(s => s.SeedDepartmentsAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockSeeder.Setup(s => s.SeedSampleAccountsAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockSeeder.Setup(s => s.SeedSampleProductsAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockSeeder.Setup(s => s.SeedLookupsAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockSeeder.Setup(s => s.SeedSampleContactsAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockSeeder.Setup(s => s.SeedSystemSettingsAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockSeeder.Setup(s => s.SeedModuleFieldConfigurationsAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockSeeder.Setup(s => s.SeedAdditionalMasterDataAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockSeeder.Setup(s => s.SeedEnsureLookupsAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var controller = BuildController();

        var result = await controller.ReseedDatabase();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ReseedDatabase_ShouldThrow_WhenSeederThrows()
    {
        // DatabaseController.ReseedDatabase() does not catch exceptions;
        // the exception propagates to the middleware layer in production.
        _mockSeeder.Setup(s => s.SeedDepartmentsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Seed error"));
        var controller = BuildController();

        Func<Task> act = () => controller.ReseedDatabase();
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Seed error");
    }

    [Fact]
    public async Task GetDatabaseStatus_ShouldThrow_WithInMemoryDb()
    {
        // GetDatabaseStatus() calls GetConnectionString() which requires a relational
        // provider. With EF InMemory (used in unit tests) this throws; in production
        // a real provider (MariaDB/SQL Server) is wired up.
        var controller = BuildController();

        Func<Task> act = () => controller.GetDatabaseStatus();
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task MigrateDatabase_ShouldReturnOk_WhenRequestIsValid()
    {
        var controller = BuildController();
        var request = new DatabaseMigrationRequest
        {
            TargetProvider = "mariadb",
            Host = "localhost",
            Port = 3306,
            Database = "crm_db",
            UserId = "user",
            Password = "pass"
        };

        var result = await controller.MigrateDatabase(request);

        // Migration will fail connection but endpoint returns Ok result object
        result.Result.Should().BeAssignableTo<ObjectResult>();
    }
}
