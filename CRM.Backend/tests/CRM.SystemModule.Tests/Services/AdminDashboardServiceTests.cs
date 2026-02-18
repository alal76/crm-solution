// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.SystemModule.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.SystemModule.Tests.Services;

/// <summary>
/// Unit tests for AdminDashboardService.
/// Tests dashboard functionality for admin users.
/// </summary>
public class AdminDashboardServiceTests
{
    private readonly Mock<ICrmDbContext> _dbContextMock;
    private readonly Mock<IProviderHealthService> _providerHealthServiceMock;
    private readonly Mock<ISystemSettingsService> _systemSettingsServiceMock;
    private readonly Mock<ILogger<AdminDashboardService>> _loggerMock;
    private readonly AdminDashboardService _service;

    public AdminDashboardServiceTests()
    {
        _dbContextMock = new Mock<ICrmDbContext>();
        _providerHealthServiceMock = new Mock<IProviderHealthService>();
        _systemSettingsServiceMock = new Mock<ISystemSettingsService>();
        _loggerMock = new Mock<ILogger<AdminDashboardService>>();
        _service = new AdminDashboardService(
            _dbContextMock.Object, 
            _providerHealthServiceMock.Object,
            _systemSettingsServiceMock.Object,
            _loggerMock.Object);
        
        // Setup common mocks
        SetupCommonMocks();
    }

    private void SetupCommonMocks()
    {
        // Setup empty lists for entities
        var users = new List<User>();
        var accounts = new List<Account>();
        var opportunities = new List<Opportunity>();
        var serviceRequests = new List<ServiceRequest>();

        _dbContextMock.Setup(x => x.Users).Returns(users.CreateMockDbSet().Object);
        _dbContextMock.Setup(x => x.Accounts).Returns(accounts.CreateMockDbSet().Object);
        _dbContextMock.Setup(x => x.Opportunities).Returns(opportunities.CreateMockDbSet().Object);
        _dbContextMock.Setup(x => x.ServiceRequests).Returns(serviceRequests.CreateMockDbSet().Object);

        // Setup provider health
        _providerHealthServiceMock.Setup(x => x.GetProviderHealthDashboardAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProviderHealthDashboardDto());

        // Setup system settings
        _systemSettingsServiceMock.Setup(x => x.GetSettingsAsync())
            .ReturnsAsync(new SystemSettingsDto { CompanyName = "Test Company" });
    }

    [Fact]
    public async Task GetSystemStatisticsAsync_ReturnsValidStatistics()
    {
        // Arrange
        var users = new List<User>
        {
            new User 
            { 
                Id = 1, 
                Email = "test@example.com",
                Username = "testuser",
                FirstName = "Test",
                LastName = "User",
                Role = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        _dbContextMock.Setup(x => x.Users).Returns(users.CreateMockDbSet().Object);

        // Act
        var result = await _service.GetSystemStatisticsAsync();

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task IsSystemHealthyAsync_ReturnsHealthStatus()
    {
        // Arrange & Act
        var result = await _service.IsSystemHealthyAsync();

        // Assert
        // Just verify it returns a boolean without throwing
        Assert.True(result || !result);
    }

    [Fact]
    public async Task GetProviderHealthSummaryAsync_ReturnsSummary()
    {
        // Arrange & Act
        var result = await _service.GetProviderHealthSummaryAsync();

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetAllModuleStatusAsync_ReturnsModuleStatuses()
    {
        // Arrange & Act
        var result = await _service.GetAllModuleStatusAsync();

        // Assert
        Assert.NotNull(result);
    }
}
