// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos.Reports;
using CRM.Core.Enums;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for ReportService.MigrateReportQueryAsync (TODO-AI005-FE-002).
/// Validates V1→V2 schema migration of report query payloads.
/// </summary>
public class ReportSchemaVersionTests
{
    private readonly ReportService _service;

    public ReportSchemaVersionTests()
    {
        var mockContext = new Mock<ICrmDbContext>();
        var mockLogger = new Mock<ILogger<ReportService>>();
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

        _service = new ReportService(
            mockContext.Object,
            mockLogger.Object,
            mockHttpContextAccessor.Object);
    }

    // ─── MigrateReportQueryAsync ──────────────────────────────────────────────

    [Fact]
    public async Task MigrateReportQueryAsync_ShouldReturnUnchanged_WhenAlreadyV2()
    {
        // Arrange
        var query = new ReportQueryDto
        {
            SchemaVersion = ReportQuerySchemaVersion.V2,
            FilterGroups = new List<ReportFilterDescriptor>
            {
                new() { Field = "Status", Operator = FilterOperator.Equals, Value = "Active" }
            }
        };

        // Act
        var result = await _service.MigrateReportQueryAsync(query);

        // Assert
        result.SchemaVersion.Should().Be(ReportQuerySchemaVersion.V2);
        result.FilterGroups.Should().HaveCount(1);
        result.Filters.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task MigrateReportQueryAsync_ShouldPromoteFiltersToFilterGroups_FromV1()
    {
        // Arrange
        var query = new ReportQueryDto
        {
            SchemaVersion = ReportQuerySchemaVersion.V1,
            Filters = new Dictionary<string, object>
            {
                ["Status"] = "Active",
                ["Region"] = "NorthAmerica"
            }
        };

        // Act
        var result = await _service.MigrateReportQueryAsync(query);

        // Assert
        result.SchemaVersion.Should().Be(ReportQuerySchemaVersion.V2);
        result.FilterGroups.Should().HaveCount(2);
        result.FilterGroups.Should().Contain(f => f.Field == "Status" && f.Value == "Active");
        result.FilterGroups.Should().Contain(f => f.Field == "Region" && f.Value == "NorthAmerica");
    }

    [Fact]
    public async Task MigrateReportQueryAsync_ShouldSetOperatorToEquals_ForAllPromotedFilters()
    {
        // Arrange
        var query = new ReportQueryDto
        {
            SchemaVersion = ReportQuerySchemaVersion.V1,
            Filters = new Dictionary<string, object>
            {
                ["Country"] = "US",
                ["Industry"] = "Technology"
            }
        };

        // Act
        var result = await _service.MigrateReportQueryAsync(query);

        // Assert
        result.FilterGroups.Should().AllSatisfy(f =>
            f.Operator.Should().Be(FilterOperator.Equals));
    }

    [Fact]
    public async Task MigrateReportQueryAsync_ShouldClearLegacyFilters_AfterMigration()
    {
        // Arrange
        var query = new ReportQueryDto
        {
            SchemaVersion = ReportQuerySchemaVersion.V1,
            Filters = new Dictionary<string, object> { ["Stage"] = "Prospect" }
        };

        // Act
        var result = await _service.MigrateReportQueryAsync(query);

        // Assert
        result.Filters.Should().BeNullOrEmpty();
        result.SchemaVersion.Should().Be(ReportQuerySchemaVersion.V2);
    }

    [Fact]
    public async Task MigrateReportQueryAsync_ShouldHandleNullFilters_Gracefully()
    {
        // Arrange
        var query = new ReportQueryDto
        {
            SchemaVersion = ReportQuerySchemaVersion.V1,
            Filters = null
        };

        // Act
        Func<Task> act = async () => await _service.MigrateReportQueryAsync(query);

        // Assert — should not throw
        await act.Should().NotThrowAsync();
        query.SchemaVersion.Should().Be(ReportQuerySchemaVersion.V2);
        query.FilterGroups.Should().NotBeNull().And.BeEmpty();
    }
}
