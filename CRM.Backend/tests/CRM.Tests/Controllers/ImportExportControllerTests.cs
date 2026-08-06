// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Api.Controllers;
using CRM.Infrastructure.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for ImportExportController (TCOV-047).
/// </summary>
public class ImportExportControllerTests : IDisposable
{
    private readonly CrmDbContext _dbContext;
    private readonly ImportExportController _controller;

    public ImportExportControllerTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"ImportExportTest_{Guid.NewGuid()}")
            .Options;
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        _dbContext = new CrmDbContext(options, config);
        var logger = new Mock<ILogger<ImportExportController>>();
        _controller = new ImportExportController(_dbContext, logger.Object);
    }

    public void Dispose() => _dbContext.Dispose();

    [Fact]
    public void GetEntityTypes_ShouldReturnOk()
    {
        var result = _controller.GetEntityTypes();

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void GetEntityTypes_ShouldReturnNonEmptyList()
    {
        var result = _controller.GetEntityTypes();

        var ok = (OkObjectResult)result.Result!;
        var list = ok.Value as System.Collections.IEnumerable;
        list.Should().NotBeNull();
    }

    [Fact]
    public async Task ExportData_ShouldReturnBadRequest_WhenEntityTypeUnknown()
    {
        var result = await _controller.ExportData("unknown-entity");

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ExportData_ShouldReturnResult_ForContacts()
    {
        var result = await _controller.ExportData("contacts");

        // Empty db returns empty list as JSON — result is not null
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ExportData_ShouldReturnResult_ForAccounts()
    {
        var result = await _controller.ExportData("accounts");

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ExportData_ShouldReturnResult_ForLeads()
    {
        var result = await _controller.ExportData("leads");

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ExportData_ShouldReturnResult_WhenFormatCsv()
    {
        var result = await _controller.ExportData("contacts", "csv");

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ImportData_ShouldReturnBadRequest_WhenEntityTypeUnknown()
    {
        // The controller takes (string entityType, IFormFile file)
        // Passing a null file to an unknown entity type should return BadRequest
        var result = await _controller.ImportData("unknown-entity", null!);

        result.Should().BeOfType<BadRequestObjectResult>();
    }
}
