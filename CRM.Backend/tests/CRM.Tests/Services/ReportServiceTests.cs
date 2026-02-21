// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Claims;
using System.Text.Json;
using CRM.Core.Dtos.Reports;
using CRM.Core.Entities;
using CRM.Core.Entities.Reports;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ReportDefinitionEntity = CRM.Core.Entities.Reports.ReportDefinition;

namespace CRM.Tests.Services;

public class ReportServiceTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<ReportService>> _mockLogger;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly ReportService _service;

    public ReportServiceTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<ReportService>>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "42")
            }))
        };

        _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(httpContext);
        _service = new ReportService(_mockContext.Object, _mockLogger.Object, _mockHttpContextAccessor.Object);
    }

    private void SetupDbSets(
        List<ReportDefinitionEntity>? reportDefinitions = null,
        List<ReportExecution>? reportExecutions = null,
        List<Account>? accounts = null)
    {
        reportDefinitions ??= new List<ReportDefinitionEntity>();
        reportExecutions ??= new List<ReportExecution>();
        accounts ??= new List<Account>();

        var mockReportDefinitions = MockDbSetFactory.CreateMockDbSet(reportDefinitions);
        _mockContext.Setup(c => c.ReportDefinitions).Returns(mockReportDefinitions.Object);

        var mockReportExecutions = MockDbSetFactory.CreateMockDbSet(reportExecutions);
        mockReportExecutions.Setup(m => m.Add(It.IsAny<ReportExecution>())).Callback<ReportExecution>(e => reportExecutions.Add(e));
        _mockContext.Setup(c => c.ReportExecutions).Returns(mockReportExecutions.Object);

        var mockAccounts = MockDbSetFactory.CreateMockDbSet(accounts);
        _mockContext.Setup(c => c.Customers).Returns(mockAccounts.Object);

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnAccountRows_WithConfiguredColumns()
    {
        var report = new ReportDefinitionEntity
        {
            Id = 1,
            Name = "Accounts",
            DataSource = ReportDataSource.Accounts,
            ColumnsJson = JsonSerializer.Serialize(new[] { "name", "industry", "createdAt" }),
            Status = ReportStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        var accounts = new List<Account>
        {
            new() { Id = 10, Company = "Acme", Industry = "Tech", CreatedAt = DateTime.UtcNow, IsDeleted = false },
            new() { Id = 11, Company = "Northwind", Industry = "Finance", CreatedAt = DateTime.UtcNow, IsDeleted = false }
        };

        SetupDbSets(reportDefinitions: new List<ReportDefinitionEntity> { report }, accounts: accounts);

        var result = await _service.ExecuteAsync(1, null);

        result.RowCount.Should().Be(2);
        result.Columns.Should().Contain(new[] { "name", "industry", "createdAt" });
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldApplyParameterFilters_WhenProvided()
    {
        var report = new ReportDefinitionEntity
        {
            Id = 2,
            Name = "Accounts",
            DataSource = ReportDataSource.Accounts,
            ColumnsJson = JsonSerializer.Serialize(new[] { "name", "industry" }),
            Status = ReportStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        var accounts = new List<Account>
        {
            new() { Id = 20, Company = "Contoso", Industry = "Tech", CreatedAt = DateTime.UtcNow, IsDeleted = false },
            new() { Id = 21, Company = "Fabrikam", Industry = "Healthcare", CreatedAt = DateTime.UtcNow, IsDeleted = false }
        };

        SetupDbSets(reportDefinitions: new List<ReportDefinitionEntity> { report }, accounts: accounts);

        var parameters = new ReportParametersDto
        {
            Filters = new Dictionary<string, object> { ["industry"] = "Tech" }
        };

        var result = await _service.ExecuteAsync(2, parameters);

        result.RowCount.Should().Be(1);
        result.Data.Should().ContainSingle();
        result.Data[0]["industry"].Should().Be("Tech");
    }
}
