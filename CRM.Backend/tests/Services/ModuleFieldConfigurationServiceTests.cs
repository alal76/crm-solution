// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Linq;
using System.Threading.Tasks;
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

public class ModuleFieldConfigurationServiceTests : IDisposable
{
    private readonly CrmDbContext _dbContext;
    private readonly ModuleFieldConfigurationService _service;
    private readonly Mock<ILogger<ModuleFieldConfigurationService>> _loggerMock;

    public ModuleFieldConfigurationServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_ModuleFieldConfig_{Guid.NewGuid()}")
            .Options;

        _dbContext = new CrmDbContext(options, null);
        _loggerMock = new Mock<ILogger<ModuleFieldConfigurationService>>();
        _service = new ModuleFieldConfigurationService(_dbContext, _loggerMock.Object);
    }

    [Fact]
    public async Task GetFieldConfigurationsAsync_ReturnsAccounts_WhenAlreadyExists()
    {
        // Arrange
        _dbContext.ModuleFieldConfigurations.Add(new ModuleFieldConfiguration
        {
            ModuleName = ModuleNames.Accounts,
            FieldName = "company",
            FieldLabel = "Company",
            FieldType = "text",
            TabIndex = 0,
            TabName = "Basic Info",
            DisplayOrder = 0,
            IsEnabled = true,
            IsRequired = false,
            GridSize = 12,
            CreatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetFieldConfigurationsAsync(ModuleNames.Accounts);

        // Assert
        result.Should().HaveCount(1);
        result.First().ModuleName.Should().Be(ModuleNames.Accounts);
        // there should be no configurations saved under the legacy name
        (await _dbContext.ModuleFieldConfigurations.CountAsync(c => c.ModuleName == "Customers"))
            .Should().Be(0);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
