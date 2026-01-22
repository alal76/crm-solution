// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using Xunit;
using FluentAssertions;
using Moq;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for SystemSettingsService
/// Tests cover system settings CRUD and module status operations
/// </summary>
public class SystemSettingsServiceTests : IDisposable
{
    private readonly CrmDbContext _dbContext;
    private readonly SystemSettingsService _service;
    private readonly Mock<ILogger<SystemSettingsService>> _loggerMock;

    public SystemSettingsServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_SystemSettings_{Guid.NewGuid()}")
            .Options;

        _dbContext = new CrmDbContext(options, null);
        _loggerMock = new Mock<ILogger<SystemSettingsService>>();
        _service = new SystemSettingsService(_dbContext, _loggerMock.Object);
    }

    #region GetSettingsAsync Tests

    [Fact]
    public async Task GetSettingsAsync_WhenSettingsExist_ReturnsSettings()
    {
        // Arrange
        var settings = new SystemSettings
        {
            CustomersEnabled = true,
            ContactsEnabled = true,
            LeadsEnabled = false,
            CompanyName = "Test Company",
            MinPasswordLength = 8,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.SystemSettings.Add(settings);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetSettingsAsync();

        // Assert
        result.Should().NotBeNull();
        result.CustomersEnabled.Should().BeTrue();
        result.ContactsEnabled.Should().BeTrue();
        result.LeadsEnabled.Should().BeFalse();
        result.CompanyName.Should().Be("Test Company");
    }

    [Fact]
    public async Task GetSettingsAsync_WhenNoSettingsExist_CreatesDefaultSettings()
    {
        // Act
        var result = await _service.GetSettingsAsync();

        // Assert
        result.Should().NotBeNull();
        result.CustomersEnabled.Should().BeTrue();  // Default enabled
        result.ContactsEnabled.Should().BeTrue();   // Default enabled
        
        // Verify settings were persisted
        var savedSettings = await _dbContext.SystemSettings.FirstOrDefaultAsync();
        savedSettings.Should().NotBeNull();
    }

    #endregion

    #region GetModuleStatusAsync Tests

    [Fact]
    public async Task GetModuleStatusAsync_ReturnsAllModuleStatuses()
    {
        // Arrange
        var settings = new SystemSettings
        {
            CustomersEnabled = true,
            ContactsEnabled = true,
            LeadsEnabled = false,
            OpportunitiesEnabled = true,
            ProductsEnabled = false,
            CampaignsEnabled = true,
            QuotesEnabled = true,
            TasksEnabled = true,
            ActivitiesEnabled = false,
            WorkflowsEnabled = true,
            ReportsEnabled = true,
            DashboardEnabled = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.SystemSettings.Add(settings);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetModuleStatusAsync();

        // Assert
        result.Should().NotBeNull();
        result.CustomersEnabled.Should().BeTrue();
        result.ContactsEnabled.Should().BeTrue();
        result.LeadsEnabled.Should().BeFalse();
        result.OpportunitiesEnabled.Should().BeTrue();
        result.ProductsEnabled.Should().BeFalse();
    }

    #endregion

    #region UpdateSettingsAsync Tests

    [Fact]
    public async Task UpdateSettingsAsync_UpdatesExistingSettings()
    {
        // Arrange
        var settings = new SystemSettings
        {
            CustomersEnabled = true,
            ContactsEnabled = true,
            CompanyName = "Old Company",
            MinPasswordLength = 6,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.SystemSettings.Add(settings);
        await _dbContext.SaveChangesAsync();

        var updateRequest = new UpdateSystemSettingsRequest
        {
            CompanyName = "New Company",
            MinPasswordLength = 10,
            CustomersEnabled = false
        };

        // Act
        var result = await _service.UpdateSettingsAsync(updateRequest, 1);

        // Assert
        result.CompanyName.Should().Be("New Company");
        result.MinPasswordLength.Should().Be(10);
        result.CustomersEnabled.Should().BeFalse();
        
        // Verify persistence
        var updatedSettings = await _dbContext.SystemSettings.FirstAsync();
        updatedSettings.CompanyName.Should().Be("New Company");
    }

    [Fact]
    public async Task UpdateSettingsAsync_WhenNoSettingsExist_CreatesNewSettings()
    {
        // Arrange
        var updateRequest = new UpdateSystemSettingsRequest
        {
            CompanyName = "New Company",
            CustomersEnabled = true,
            ContactsEnabled = false
        };

        // Act
        var result = await _service.UpdateSettingsAsync(updateRequest);

        // Assert
        result.Should().NotBeNull();
        result.CompanyName.Should().Be("New Company");
        result.ContactsEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateSettingsAsync_PartialUpdate_OnlyUpdatesProvidedFields()
    {
        // Arrange
        var settings = new SystemSettings
        {
            CustomersEnabled = true,
            ContactsEnabled = true,
            CompanyName = "Original Company",
            PrimaryColor = "#6750A4",
            MinPasswordLength = 8,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.SystemSettings.Add(settings);
        await _dbContext.SaveChangesAsync();

        var updateRequest = new UpdateSystemSettingsRequest
        {
            CompanyName = "Updated Company"
            // Not updating other fields
        };

        // Act
        var result = await _service.UpdateSettingsAsync(updateRequest, 1);

        // Assert
        result.CompanyName.Should().Be("Updated Company");
        result.CustomersEnabled.Should().BeTrue();  // Unchanged
        result.PrimaryColor.Should().Be("#6750A4"); // Unchanged
    }

    #endregion

    #region Security Settings Tests

    [Fact]
    public async Task UpdateSettingsAsync_SecuritySettings_AreUpdated()
    {
        // Arrange
        var settings = new SystemSettings
        {
            RequireTwoFactor = false,
            MinPasswordLength = 6,
            SessionTimeoutMinutes = 30,
            AllowUserRegistration = false,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.SystemSettings.Add(settings);
        await _dbContext.SaveChangesAsync();

        var updateRequest = new UpdateSystemSettingsRequest
        {
            RequireTwoFactor = true,
            MinPasswordLength = 12,
            SessionTimeoutMinutes = 60,
            AllowUserRegistration = true
        };

        // Act
        var result = await _service.UpdateSettingsAsync(updateRequest);

        // Assert
        result.RequireTwoFactor.Should().BeTrue();
        result.MinPasswordLength.Should().Be(12);
        result.SessionTimeoutMinutes.Should().Be(60);
        result.AllowUserRegistration.Should().BeTrue();
    }

    #endregion

    #region Module Toggle Tests

    [Fact]
    public async Task UpdateSettingsAsync_CanDisableModule()
    {
        // Arrange
        var settings = new SystemSettings
        {
            CustomersEnabled = true,
            LeadsEnabled = true,
            WorkflowsEnabled = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.SystemSettings.Add(settings);
        await _dbContext.SaveChangesAsync();

        var updateRequest = new UpdateSystemSettingsRequest
        {
            CustomersEnabled = false,
            LeadsEnabled = false,
            WorkflowsEnabled = false
        };

        // Act
        var result = await _service.UpdateSettingsAsync(updateRequest);

        // Assert
        result.CustomersEnabled.Should().BeFalse();
        result.LeadsEnabled.Should().BeFalse();
        result.WorkflowsEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateSettingsAsync_CanEnableModule()
    {
        // Arrange
        var settings = new SystemSettings
        {
            CustomersEnabled = false,
            LeadsEnabled = false,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.SystemSettings.Add(settings);
        await _dbContext.SaveChangesAsync();

        var updateRequest = new UpdateSystemSettingsRequest
        {
            CustomersEnabled = true,
            LeadsEnabled = true
        };

        // Act
        var result = await _service.UpdateSettingsAsync(updateRequest);

        // Assert
        result.CustomersEnabled.Should().BeTrue();
        result.LeadsEnabled.Should().BeTrue();
    }

    #endregion

    #region Branding Settings Tests

    [Fact]
    public async Task UpdateSettingsAsync_BrandingSettings_AreUpdated()
    {
        // Arrange
        var settings = new SystemSettings
        {
            CompanyName = "Old Name",
            PrimaryColor = "#000000",
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.SystemSettings.Add(settings);
        await _dbContext.SaveChangesAsync();

        var updateRequest = new UpdateSystemSettingsRequest
        {
            CompanyName = "Acme Corporation",
            CompanyLogoUrl = "https://example.com/logo.png",
            PrimaryColor = "#FF5733",
            SecondaryColor = "#33FF57"
        };

        // Act
        var result = await _service.UpdateSettingsAsync(updateRequest);

        // Assert
        result.CompanyName.Should().Be("Acme Corporation");
        result.CompanyLogoUrl.Should().Be("https://example.com/logo.png");
        result.PrimaryColor.Should().Be("#FF5733");
        result.SecondaryColor.Should().Be("#33FF57");
    }

    #endregion

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
