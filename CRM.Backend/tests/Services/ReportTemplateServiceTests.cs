// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// REV-FE-003 — Report Templates Marketplace backend
// Uses a real EF Core InMemory CrmDbContext (not a mocked DbSet) so JSON column
// round-tripping (TagsJson / ReportConfigJson) and Include(t => t.AuthorUser)
// are exercised the same way they run against a real database.

using CRM.Core.Entities;
using CRM.Core.Entities.Reports;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

public class ReportTemplateServiceTests
{
    private static CrmDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        return new CrmDbContext(options, configuration);
    }

    private static ReportTemplateService CreateService(CrmDbContext context)
    {
        return new ReportTemplateService(context, Mock.Of<ILogger<ReportTemplateService>>());
    }

    private static ReportTemplate MakeTemplate(int id, string name = "Sales Pipeline Report", int downloads = 10) => new()
    {
        Id = id,
        Name = name,
        Description = "Comprehensive pipeline analysis",
        Category = "Sales",
        AuthorDisplayName = "CRM Solution Team",
        Rating = 4.8m,
        Downloads = downloads,
        TagsJson = "[\"sales\",\"pipeline\",\"forecasting\"]",
        ReportConfigJson = "{\"type\":\"pipeline\",\"groupBy\":\"stage\",\"metrics\":[\"count\",\"value\"]}",
        CreatedAt = DateTime.UtcNow,
        IsDeleted = false
    };

    // ── GetAllAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsAllNonDeletedTemplates_MappedFromJsonColumns()
    {
        using var context = CreateContext();
        context.ReportTemplates.Add(MakeTemplate(1));
        context.ReportTemplates.Add(new ReportTemplate
        {
            Id = 2,
            Name = "Deleted Template",
            Category = "Marketing",
            AuthorDisplayName = "Marketing Ops",
            IsDeleted = true
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = (await service.GetAllAsync()).ToList();

        result.Should().HaveCount(1);
        var dto = result.Single();
        dto.Id.Should().Be(1);
        dto.Name.Should().Be("Sales Pipeline Report");
        dto.Author.Should().Be("CRM Solution Team");
        dto.Rating.Should().Be(4.8m);
        dto.Downloads.Should().Be(10);
        dto.Tags.Should().BeEquivalentTo(new[] { "sales", "pipeline", "forecasting" });
        dto.ReportConfig.Should().ContainKey("type");
        dto.ReportConfig["groupBy"].ToString().Should().Be("stage");
    }

    [Fact]
    public async Task GetAllAsync_UsesLinkedUserName_WhenAuthorUserIdSet()
    {
        using var context = CreateContext();
        var user = new User { Id = 42, Email = "sales.ops@test.com", FirstName = "Sales", LastName = "Ops" };
        context.Users.Add(user);
        var template = MakeTemplate(1);
        template.AuthorUserId = 42;
        template.AuthorDisplayName = "Fallback Name";
        context.ReportTemplates.Add(template);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = (await service.GetAllAsync()).Single();

        result.Author.Should().Be("Sales Ops");
    }

    [Fact]
    public async Task GetAllAsync_OrdersByDownloadsDescending()
    {
        using var context = CreateContext();
        context.ReportTemplates.Add(MakeTemplate(1, "Low Downloads", downloads: 5));
        context.ReportTemplates.Add(MakeTemplate(2, "High Downloads", downloads: 500));
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = (await service.GetAllAsync()).ToList();

        result.Should().HaveCount(2);
        result[0].Name.Should().Be("High Downloads");
        result[1].Name.Should().Be("Low Downloads");
    }

    // ── ApplyAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task ApplyAsync_IncrementsDownloads_AndReturnsReportConfig()
    {
        using var context = CreateContext();
        context.ReportTemplates.Add(MakeTemplate(1, downloads: 10));
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.ApplyAsync(1);

        result.Should().NotBeNull();
        result!.TemplateId.Should().Be(1);
        result.TemplateName.Should().Be("Sales Pipeline Report");
        result.Downloads.Should().Be(11);
        result.ReportConfig.Should().ContainKey("type");

        var persisted = await context.ReportTemplates.FindAsync(1);
        persisted!.Downloads.Should().Be(11);
    }

    [Fact]
    public async Task ApplyAsync_ReturnsNull_WhenTemplateDoesNotExist()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        var result = await service.ApplyAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task ApplyAsync_ReturnsNull_WhenTemplateIsDeleted()
    {
        using var context = CreateContext();
        var template = MakeTemplate(1);
        template.IsDeleted = true;
        context.ReportTemplates.Add(template);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.ApplyAsync(1);

        result.Should().BeNull();
    }

    [Fact]
    public async Task ApplyAsync_CalledTwice_IncrementsDownloadsEachTime()
    {
        using var context = CreateContext();
        context.ReportTemplates.Add(MakeTemplate(1, downloads: 0));
        await context.SaveChangesAsync();

        var service = CreateService(context);

        await service.ApplyAsync(1);
        var second = await service.ApplyAsync(1);

        second!.Downloads.Should().Be(2);
    }
}
