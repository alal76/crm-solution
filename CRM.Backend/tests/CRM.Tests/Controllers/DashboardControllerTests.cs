// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Api.Controllers;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for DashboardController (TCOV-042).
/// </summary>
public class DashboardControllerTests : IDisposable
{
    private readonly Mock<IDashboardService> _mockDashboardService;
    private readonly CrmDbContext _dbContext;
    private readonly DashboardController _controller;

    public DashboardControllerTests()
    {
        _mockDashboardService = new Mock<IDashboardService>();

        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"DashboardTest_{Guid.NewGuid()}")
            .Options;
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        _dbContext = new CrmDbContext(options, config);

        _controller = new DashboardController(_dbContext, _mockDashboardService.Object);
    }

    public void Dispose() => _dbContext.Dispose();

    [Fact]
    public async Task GetStats_ShouldReturnOk_WhenNoData()
    {
        var result = await _controller.GetStats();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetSummary_ShouldReturnOk_WhenNoData()
    {
        var result = await _controller.GetSummary();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetPipelineSummary_ShouldReturnOk()
    {
        var result = await _controller.GetPipelineSummary();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetStats_ShouldReturnOk_WithAccountsInDb()
    {
        _dbContext.Accounts.Add(new CRM.Core.Entities.Account
        {
            Company = "Test Account 1",
            FirstName = "Test", LastName = "Account",
            Email = "test@example.com", Phone = "5555555555",
            IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var result = await _controller.GetStats();

        result.Should().BeOfType<OkObjectResult>();
        var ok = (OkObjectResult)result;
        ok.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetSummary_ShouldReturnOk_WithOpportunitiesInDb()
    {
        _dbContext.Opportunities.Add(new CRM.Core.Entities.Opportunity
        {
            Name = "Deal 1", Stage = CRM.Core.Entities.OpportunityStage.ClosedWon,
            Amount = 10000m, ExpectedCloseDate = DateTime.UtcNow,
            IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var result = await _controller.GetSummary();

        result.Should().BeOfType<OkObjectResult>();
    }
}
