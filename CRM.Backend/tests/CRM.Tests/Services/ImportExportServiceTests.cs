// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for ImportExportService.
/// Covers entity type listing, export (JSON/CSV), import (JSON/CSV),
/// template generation, and error handling.
/// </summary>
public class ImportExportServiceTests : ServiceTestFixtureBase<ImportExportService>
{
    private readonly Mock<ICrmDbContext> _mockDbContext;    private readonly ImportExportService _service;

    public ImportExportServiceTests()
    {
        _mockDbContext = new Mock<ICrmDbContext>();        SetupEmptyDbSets();

        _service = new ImportExportService(_mockDbContext.Object, MockLogger.Object);
    }

    private void SetupEmptyDbSets()
    {
        _mockDbContext.Setup(c => c.Accounts).Returns(MockDbSetFactory.CreateMockDbSet(new List<CRM.Core.Entities.Account>()).Object);
        _mockDbContext.Setup(c => c.Contacts).Returns(MockDbSetFactory.CreateMockDbSet(new List<CRM.Core.Models.Contact>()).Object);
        _mockDbContext.Setup(c => c.Leads).Returns(MockDbSetFactory.CreateMockDbSet(new List<CRM.Core.Entities.Lead>()).Object);
        _mockDbContext.Setup(c => c.Opportunities).Returns(MockDbSetFactory.CreateMockDbSet(new List<CRM.Core.Entities.Opportunity>()).Object);
        _mockDbContext.Setup(c => c.Products).Returns(MockDbSetFactory.CreateMockDbSet(new List<CRM.Core.Entities.Product>()).Object);
        _mockDbContext.Setup(c => c.Interactions).Returns(MockDbSetFactory.CreateMockDbSet(new List<CRM.Core.Entities.Interaction>()).Object);
        _mockDbContext.Setup(c => c.CrmTasks).Returns(MockDbSetFactory.CreateMockDbSet(new List<CRM.Core.Entities.CrmTask>()).Object);
        _mockDbContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    // ========================================================================
    // Constructor Tests
    // ========================================================================

    [Fact]
    public void Constructor_ShouldCreateInstance_WithValidDependencies()
    {
        _service.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenDbContextIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ImportExportService(null!, MockLogger.Object));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenLoggerIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ImportExportService(_mockDbContext.Object, null!));
    }

    // ========================================================================
    // GetEntityTypes Tests
    // ========================================================================

    [Fact]
    public void GetEntityTypes_ShouldReturnSupportedTypes()
    {
        // Act
        var result = _service.GetEntityTypes().ToList();

        // Assert
        result.Should().NotBeEmpty();
        result.Should().Contain(e => e.Name == "accounts");
        result.Should().Contain(e => e.Name == "contacts");
        result.Should().Contain(e => e.Name == "leads");
        result.Should().Contain(e => e.Name == "opportunities");
        result.Should().Contain(e => e.Name == "products");
    }

    [Fact]
    public void GetEntityTypes_ShouldIncludeImportExportCapabilities()
    {
        // Act
        var result = _service.GetEntityTypes().ToList();

        // Assert
        var accounts = result.First(e => e.Name == "accounts");
        accounts.CanImport.Should().BeTrue();
        accounts.CanExport.Should().BeTrue();

        var interactions = result.First(e => e.Name == "interactions");
        interactions.CanImport.Should().BeFalse();
        interactions.CanExport.Should().BeTrue();
    }

    // ========================================================================
    // ExportToJsonAsync Tests
    // ========================================================================

    [Fact]
    public async Task ExportToJsonAsync_ShouldReturnJsonBytes_ForValidEntityType()
    {
        // Act
        var result = await _service.ExportToJsonAsync("accounts");

        // Assert
        result.Should().NotBeNull();
        result.Length.Should().BeGreaterThan(0);
        var json = System.Text.Encoding.UTF8.GetString(result);
        json.Should().StartWith("[");
    }

    [Fact]
    public async Task ExportToJsonAsync_ShouldThrow_ForUnsupportedEntityType()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.ExportToJsonAsync("invalid_entity"));
    }

    // ========================================================================
    // ExportToCsvAsync Tests
    // ========================================================================

    [Fact]
    public async Task ExportToCsvAsync_ShouldReturnCsvBytes_ForValidEntityType()
    {
        // Act
        var result = await _service.ExportToCsvAsync("accounts");

        // Assert
        result.Should().NotBeNull();
        result.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ExportToCsvAsync_ShouldThrow_ForUnsupportedEntityType()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.ExportToCsvAsync("invalid_entity"));
    }

    // ========================================================================
    // GetTemplateJson Tests
    // ========================================================================

    [Fact]
    public void GetTemplateJson_ShouldReturnJsonTemplate_ForAccounts()
    {
        // Act
        var result = _service.GetTemplateJson("accounts");

        // Assert
        result.Should().NotBeNull();
        var json = System.Text.Encoding.UTF8.GetString(result);
        json.Should().Contain("Email");
    }

    [Fact]
    public void GetTemplateJson_ShouldReturnJsonTemplate_ForLeads()
    {
        // Act
        var result = _service.GetTemplateJson("leads");

        // Assert
        result.Should().NotBeNull();
        var json = System.Text.Encoding.UTF8.GetString(result);
        json.Should().Contain("Source");
    }

    [Fact]
    public void GetTemplateJson_ShouldReturnEmptyArray_ForUnknownType()
    {
        // Act
        var result = _service.GetTemplateJson("unknown");

        // Assert
        result.Should().NotBeNull();
        var json = System.Text.Encoding.UTF8.GetString(result);
        json.Should().Contain("[]");
    }

    // ========================================================================
    // GetTemplateCsv Tests
    // ========================================================================

    [Fact]
    public void GetTemplateCsv_ShouldReturnCsvHeader_ForAccounts()
    {
        // Act
        var result = _service.GetTemplateCsv("accounts");

        // Assert
        result.Should().NotBeNull();
        var csv = System.Text.Encoding.UTF8.GetString(result);
        csv.Should().Contain("FirstName");
        csv.Should().Contain("Email");
    }

    [Fact]
    public void GetTemplateCsv_ShouldReturnCsvHeader_ForContacts()
    {
        // Act
        var result = _service.GetTemplateCsv("contacts");

        // Assert
        var csv = System.Text.Encoding.UTF8.GetString(result);
        csv.Should().Contain("FirstName");
        csv.Should().Contain("JobTitle");
    }

    // ========================================================================
    // ImportFromJsonAsync Tests
    // ========================================================================

    [Fact]
    public async Task ImportFromJsonAsync_ShouldReturnFailure_WhenDataIsEmpty()
    {
        // Arrange
        var emptyData = System.Text.Encoding.UTF8.GetBytes("");

        // Act
        var result = await _service.ImportFromJsonAsync("accounts", emptyData);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ImportFromJsonAsync_ShouldReturnFailure_ForUnsupportedEntityType()
    {
        // Arrange
        var data = System.Text.Encoding.UTF8.GetBytes("[{\"name\":\"test\"}]");

        // Act
        var result = await _service.ImportFromJsonAsync("interactions", data);

        // Assert
        result.Success.Should().BeFalse();
    }

    // ========================================================================
    // ImportFromCsvAsync Tests
    // ========================================================================

    [Fact]
    public async Task ImportFromCsvAsync_ShouldReturnFailure_WhenDataIsEmpty()
    {
        // Arrange
        var emptyData = System.Text.Encoding.UTF8.GetBytes("");

        // Act
        var result = await _service.ImportFromCsvAsync("accounts", emptyData);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ImportFromCsvAsync_ShouldReturnFailure_ForUnsupportedEntityType()
    {
        // Arrange
        var data = System.Text.Encoding.UTF8.GetBytes("Name\nTest");

        // Act
        var result = await _service.ImportFromCsvAsync("interactions", data);

        // Assert
        result.Success.Should().BeFalse();
    }
}
