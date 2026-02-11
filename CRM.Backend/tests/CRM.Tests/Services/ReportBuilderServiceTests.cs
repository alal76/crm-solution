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

using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

public class ReportBuilderServiceTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<ReportBuilderService>> _mockLogger;
    private readonly ReportBuilderService _service;

    public ReportBuilderServiceTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<ReportBuilderService>>();

        _service = new ReportBuilderService(
            _mockContext.Object,
            _mockLogger.Object);
    }

    // ========================================================================
    // CreateReportAsync
    // ========================================================================

    [Fact]
    public async Task CreateReportAsync_ShouldReturnReport_WithId()
    {
        // Arrange
        var report = new ReportDefinition
        {
            Name = "Sales Report",
            Description = "Monthly sales",
            UserId = 1,
            EntitySource = "Opportunities",
            Columns = new List<string> { "Name", "Amount", "Stage" },
            Type = ReportType.Tabular
        };

        // Act
        var result = await _service.CreateReportAsync(report);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeNullOrEmpty();
        result.Name.Should().Be("Sales Report");
        result.EntitySource.Should().Be("Opportunities");
        result.Columns.Should().HaveCount(3);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CreateReportAsync_ShouldGenerateUniqueIds()
    {
        // Act
        var r1 = await _service.CreateReportAsync(new ReportDefinition { Name = "R1", UserId = 1, EntitySource = "Leads" });
        var r2 = await _service.CreateReportAsync(new ReportDefinition { Name = "R2", UserId = 1, EntitySource = "Leads" });

        // Assert
        r1.Id.Should().NotBe(r2.Id);
    }

    // ========================================================================
    // GetReportAsync / GetReportsAsync
    // ========================================================================

    [Fact]
    public async Task GetReportAsync_ShouldReturnReport_WhenExists()
    {
        // Arrange
        var created = await _service.CreateReportAsync(new ReportDefinition { Name = "Test", UserId = 1, EntitySource = "Accounts" });

        // Act
        var retrieved = await _service.GetReportAsync(created.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Test");
    }

    [Fact]
    public async Task GetReportAsync_ShouldReturnNull_WhenNotFound()
    {
        // Act
        var result = await _service.GetReportAsync("nonexistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetReportsAsync_ShouldReturnOnlyUserReports()
    {
        // Arrange
        await _service.CreateReportAsync(new ReportDefinition { Name = "User 1 Report", UserId = 1, EntitySource = "Leads" });
        await _service.CreateReportAsync(new ReportDefinition { Name = "User 2 Report", UserId = 2, EntitySource = "Leads" });

        // Act
        var reports = await _service.GetReportsAsync(1);

        // Assert
        reports.Should().HaveCount(1);
        reports.First().Name.Should().Be("User 1 Report");
    }

    // ========================================================================
    // UpdateReportAsync / DeleteReportAsync
    // ========================================================================

    [Fact]
    public async Task UpdateReportAsync_ShouldModifyReport()
    {
        // Arrange
        var created = await _service.CreateReportAsync(new ReportDefinition { Name = "Original", UserId = 1, EntitySource = "Leads" });
        created.Name = "Updated";
        created.Description = "Updated description";

        // Act
        var result = await _service.UpdateReportAsync(created);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated");
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateReportAsync_ShouldReturnNull_WhenNotFound()
    {
        // Act
        var result = await _service.UpdateReportAsync(new ReportDefinition { Id = "nonexistent", Name = "X", UserId = 1 });

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteReportAsync_ShouldReturnTrue_WhenExists()
    {
        // Arrange
        var created = await _service.CreateReportAsync(new ReportDefinition { Name = "Del", UserId = 1, EntitySource = "Leads" });

        // Act
        var result = await _service.DeleteReportAsync(created.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteReportAsync_ShouldReturnFalse_WhenNotFound()
    {
        // Act
        var result = await _service.DeleteReportAsync("nonexistent");

        // Assert
        result.Should().BeFalse();
    }

    // ========================================================================
    // ExecuteReportAsync
    // ========================================================================

    [Fact]
    public async Task ExecuteReportAsync_ShouldReturnResults_ForAccountsSource()
    {
        // Arrange
        var customers = new List<Account>
        {
            new() { Id = 1, Company = "Acme Corp", Email = "acme@test.com", Industry = "Tech", LifecycleStage = AccountLifecycleStage.Active, IsDeleted = false },
            new() { Id = 2, Company = "Beta Inc", Email = "beta@test.com", Industry = "Finance", LifecycleStage = AccountLifecycleStage.Active, IsDeleted = false }
        };
        var mockSet = MockDbSetFactory.CreateMockDbSet(customers);
        _mockContext.Setup(c => c.Customers).Returns(mockSet.Object);

        var report = await _service.CreateReportAsync(new ReportDefinition
        {
            Name = "Accounts Report",
            UserId = 1,
            EntitySource = "Accounts",
            Columns = new List<string> { "Company", "Industry" }
        });

        // Act
        var result = await _service.ExecuteReportAsync(report.Id);

        // Assert
        result.Should().NotBeNull();
        result!.ReportName.Should().Be("Accounts Report");
        result.Rows.Should().HaveCount(2);
        result.ExecutedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ExecuteReportAsync_ShouldReturnResults_ForLeadsSource()
    {
        // Arrange
        var leads = new List<Lead>
        {
            new() { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@test.com", Status = LeadLifecycleStatus.New, IsDeleted = false }
        };
        var mockSet = MockDbSetFactory.CreateMockDbSet(leads);
        _mockContext.Setup(c => c.Leads).Returns(mockSet.Object);

        var report = await _service.CreateReportAsync(new ReportDefinition
        {
            Name = "Leads Report",
            UserId = 1,
            EntitySource = "Leads",
            Columns = new List<string> { "FirstName", "LastName", "Email" }
        });

        // Act
        var result = await _service.ExecuteReportAsync(report.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Rows.Should().HaveCount(1);
    }

    [Fact]
    public async Task ExecuteReportAsync_ShouldReturnResults_ForOpportunitiesSource()
    {
        // Arrange
        var opps = new List<Opportunity>
        {
            new() { Id = 1, Name = "Big Deal", Stage = OpportunityStage.Proposal, Amount = 50000, IsDeleted = false }
        };
        var mockSet = MockDbSetFactory.CreateMockDbSet(opps);
        _mockContext.Setup(c => c.Opportunities).Returns(mockSet.Object);

        var report = await _service.CreateReportAsync(new ReportDefinition
        {
            Name = "Opp Report",
            UserId = 1,
            EntitySource = "Opportunities",
            Columns = new List<string> { "Name", "Amount", "Stage" }
        });

        // Act
        var result = await _service.ExecuteReportAsync(report.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Rows.Should().HaveCount(1);
    }

    [Fact]
    public async Task ExecuteReportAsync_ShouldReturnNull_WhenReportNotFound()
    {
        // Act
        var result = await _service.ExecuteReportAsync("nonexistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteReportAsync_ShouldRespectMaxRows()
    {
        // Arrange
        var leads = Enumerable.Range(1, 50).Select(i => new Lead
        {
            Id = i,
            FirstName = $"Lead{i}",
            LastName = "Test",
            Email = $"lead{i}@test.com",
            Status = LeadLifecycleStatus.New,
            IsDeleted = false
        }).ToList();
        var mockSet = MockDbSetFactory.CreateMockDbSet(leads);
        _mockContext.Setup(c => c.Leads).Returns(mockSet.Object);

        var report = await _service.CreateReportAsync(new ReportDefinition
        {
            Name = "Limited Report",
            UserId = 1,
            EntitySource = "Leads",
            MaxRows = 10
        });

        // Act
        var result = await _service.ExecuteReportAsync(report.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Rows.Should().HaveCountLessOrEqualTo(10);
    }

    // ========================================================================
    // ExportToCsvAsync
    // ========================================================================

    [Fact]
    public async Task ExportToCsvAsync_ShouldReturnCsvString()
    {
        // Arrange
        var customers = new List<Account>
        {
            new() { Id = 1, Company = "Acme Corp", Email = "acme@test.com", Industry = "Tech", LifecycleStage = AccountLifecycleStage.Active, IsDeleted = false }
        };
        var mockSet = MockDbSetFactory.CreateMockDbSet(customers);
        _mockContext.Setup(c => c.Customers).Returns(mockSet.Object);

        var report = await _service.CreateReportAsync(new ReportDefinition
        {
            Name = "CSV Report",
            UserId = 1,
            EntitySource = "Accounts",
            Columns = new List<string> { "Company", "Industry" }
        });

        // Act
        var csvBytes = await _service.ExportToCsvAsync(report.Id);

        // Assert
        csvBytes.Should().NotBeNull();
        csvBytes.Should().NotBeEmpty();
        var csv = System.Text.Encoding.UTF8.GetString(csvBytes!);
        csv.Should().Contain("Company");
        csv.Should().Contain("Acme Corp");
    }

    [Fact]
    public async Task ExportToCsvAsync_ShouldReturnNull_WhenReportNotFound()
    {
        // Act
        var csvBytes = await _service.ExportToCsvAsync("nonexistent");

        // Assert
        csvBytes.Should().BeNull();
    }

    [Fact]
    public async Task ExportToCsvAsync_ShouldEscapeCommasInValues()
    {
        // Arrange
        var customers = new List<Account>
        {
            new() { Id = 1, Company = "Acme, Inc.", Email = "acme@test.com", Industry = "Tech", LifecycleStage = AccountLifecycleStage.Active, IsDeleted = false }
        };
        var mockSet = MockDbSetFactory.CreateMockDbSet(customers);
        _mockContext.Setup(c => c.Customers).Returns(mockSet.Object);

        var report = await _service.CreateReportAsync(new ReportDefinition
        {
            Name = "Escape Test",
            UserId = 1,
            EntitySource = "Accounts",
            Columns = new List<string> { "Company" }
        });

        // Act
        var csvBytes = await _service.ExportToCsvAsync(report.Id);

        // Assert
        csvBytes.Should().NotBeNull();
        var csv = System.Text.Encoding.UTF8.GetString(csvBytes!);
        csv.Should().Contain("\"Acme, Inc.\""); // Comma-containing values should be quoted
    }

    // ========================================================================
    // GetAvailableSources
    // ========================================================================

    [Fact]
    public void GetAvailableSources_ShouldReturnEntitySources()
    {
        // Act
        var sources = _service.GetAvailableSources();

        // Assert
        sources.Should().NotBeEmpty();
        sources.Should().Contain(s => s.Name == "Accounts");
        sources.Should().Contain(s => s.Name == "Leads");
        sources.Should().Contain(s => s.Name == "Opportunities");
    }

    [Fact]
    public void GetAvailableSources_ShouldHaveFieldsForEachSource()
    {
        // Act
        var sources = _service.GetAvailableSources();

        // Assert
        foreach (var source in sources)
        {
            source.Fields.Should().NotBeEmpty($"Source '{source.Name}' should have fields");
            source.DisplayName.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public void GetAvailableSources_ShouldIncludeContactsAndServiceRequests()
    {
        // Act
        var sources = _service.GetAvailableSources();

        // Assert
        sources.Should().Contain(s => s.Name == "Contacts");
        sources.Should().Contain(s => s.Name == "ServiceRequests");
    }
}
