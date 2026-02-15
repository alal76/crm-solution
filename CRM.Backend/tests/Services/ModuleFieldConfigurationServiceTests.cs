// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

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
    public async Task GetFieldConfigurationsAsync_MigratesLegacyCustomers_WhenAccountsMissing()
    {
        // Arrange
        _dbContext.ModuleFieldConfigurations.Add(new ModuleFieldConfiguration
        {
            ModuleName = ModuleNames.Customers,
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

        var stored = await _dbContext.ModuleFieldConfigurations.ToListAsync();
        stored.Should().ContainSingle();
        stored.Single().ModuleName.Should().Be(ModuleNames.Accounts);
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
        (await _dbContext.ModuleFieldConfigurations.CountAsync(c => c.ModuleName == ModuleNames.Customers))
            .Should().Be(0);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
